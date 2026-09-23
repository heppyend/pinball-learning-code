using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 客户端页面导航内核——页面可见性的唯一所有者。
    ///
    /// 职责（MVC 的 Controller 侧基础设施）：
    /// - 维护页面注册表与返回栈；
    /// - 按 <see cref="IClientPageView"/> 多态派发 Enter/Pause/Resume/Exit，
    ///   不再按具体页面类型逐个 GetComponent（旧实现是 15 分支 if/else）；
    /// - 向需要发起导航的视图注入 <see cref="IClientNavigation"/>。
    ///
    /// 边界：不持有任何页面的业务状态，不查询数据服务来做导航决策以外的判断。
    /// 页面不得自行 SetActive 其它页面，也不得再使用静态导航事件。
    /// </summary>
    public sealed class ClientUiNavigator : MonoBehaviour, IClientNavigation, IClientHomeNavigation, IClientHeroDetailOpener
    {
        private enum PageAction { Enter, Pause, Resume, Exit }

        [SerializeField] private List<ClientUiPage> _pages = new List<ClientUiPage>();

        private readonly Dictionary<ClientUiPageId, ClientUiPage> _pageById =
            new Dictionary<ClientUiPageId, ClientUiPage>();
        private readonly Dictionary<ClientUiPageId, IClientPageView> _viewById =
            new Dictionary<ClientUiPageId, IClientPageView>();
        private readonly Stack<ClientUiPageId> _history = new Stack<ClientUiPageId>();
        private readonly List<IClientNavigationObserver> _observers = new List<IClientNavigationObserver>();

        private ClientUiPageId _currentPage = ClientUiPageId.Home;
        private ClientActivityPage _completeGuideActivityPage;
        private bool _completeGuideSubscribed;
        private ClientPopupService _popupService;

        /// <summary>
        /// 当前页面 ID。弹窗栈非空时以栈顶弹窗为准——按负责人确认的架构，
        /// 除 MainPage主页 外的所有界面都是 PopupLayer 的全屏弹窗。
        /// </summary>
        public ClientUiPageId CurrentPage
        {
            get
            {
                ResolvePopupService();
                return _popupService != null && _popupService.HasOpenPopup
                    ? _popupService.CurrentPageId
                    : _currentPage;
            }
        }

        public bool CanGoBack
        {
            get
            {
                ResolvePopupService();
                return _popupService != null ? _popupService.HasOpenPopup : _history.Count > 0;
            }
        }

        private void Awake()
        {
            RebuildRegistry();
        }

        private void Start()
        {
            // 场景是页面的唯一事实来源：序列化 _pages 会在页面被重命名或复制后漂移
            // （当前场景已存在重复 ID 与漏登记）。此处把 ClientCanvas 下所有
            // ClientUiPage 合并进注册表，登记不再依赖人工维护的列表。
            CollectPagesFromScene();

            // 组合根注入：替代原先的 public static event。静态事件会让所有活着的
            // 订阅者同时响应同一次点击，是"多处并行改写可见性"的根因。
            BindNavigationHosts();
        }

        /// <summary>
        /// 解析弹窗服务。它与导航器同在 ClientCanvas 下（挂在 PopupLayer），
        /// 因此自动解析即可，无需额外场景接线。
        /// </summary>
        private void ResolvePopupService()
        {
            if (_popupService == null)
                _popupService = GetComponentInChildren<ClientPopupService>(true);
        }

        /// <summary>把场景中实际存在、但未登记在 _pages 里的页面并入注册表。</summary>
        private void CollectPagesFromScene()
        {
            ClientUiPage[] found = GetComponentsInChildren<ClientUiPage>(true);
            int added = 0;
            for (int i = 0; i < found.Length; i++)
            {
                ClientUiPage page = found[i];
                if (page == null || _pages.Contains(page))
                    continue;
                _pages.Add(page);
                added++;
            }
            if (added > 0)
            {
                RebuildRegistry();
                Debug.Log("[Client] 从场景自动收录 " + added + " 个未登记页面。");
            }
        }

        private void OnEnable()
        {
            SubscribeCompleteGuideEntry();
        }

        private void OnDisable()
        {
            UnsubscribeCompleteGuideEntry();
        }

        // ------------------------------------------------------------------
        // 注册
        // ------------------------------------------------------------------

        public void Configure(List<ClientUiPage> pages)
        {
            _pages = pages ?? new List<ClientUiPage>();
            RebuildRegistry();
        }

        public void Register(ClientUiPage page)
        {
            if (page == null)
                return;
            if (!_pages.Contains(page))
                _pages.Add(page);
            _pageById[page.PageId] = page;
            _viewById.Remove(page.PageId);
        }

        private void RebuildRegistry()
        {
            _pageById.Clear();
            _viewById.Clear();
            for (int i = 0; i < _pages.Count; i++)
            {
                ClientUiPage page = _pages[i];
                if (page == null)
                    continue;
                if (_pageById.ContainsKey(page.PageId))
                {
                    // 同一个 PageId 被两个节点占用时后写覆盖前者，属场景接线错误，必须暴露。
                    Debug.LogError("[Client] 页面 ID 重复注册：" + page.PageId +
                                   "，节点 " + _pageById[page.PageId].name + " 将被 " + page.name + " 覆盖。");
                }
                _pageById[page.PageId] = page;
            }
        }

        // ------------------------------------------------------------------
        // 导航
        // ------------------------------------------------------------------

        public void Open(ClientUiPageId pageId)
        {
            // 按负责人确认的架构：PagesLayer 只保留 MainPage主页（大厅），
            // 其余界面全部是 PopupLayer 的全屏弹窗，因此优先走弹窗栈。
            // 这样既有的页面 View 仍然只调用 Open(pageId)，无需任何改动。
            if (pageId != ClientUiPageId.Home)
            {
                ResolvePopupService();
                if (_popupService != null && _popupService.TryOpen(pageId))
                {
                    ClientUiTrace.Line("导航", "Open(" + pageId + ") → 命中 PopupLayer 弹窗，已压栈");
                    NotifyObservers(pageId);
                    return;
                }
                ClientUiTrace.Warn("导航", "Open(" + pageId + ") → PopupLayer 无此弹窗，尝试 PagesLayer 页面栈");
            }

            if (pageId == _currentPage)
            {
                ClientUiTrace.Line("导航", "Open(" + pageId + ") 被忽略：已是当前页面");
                return;
            }
            if (!_pageById.ContainsKey(pageId) || !_pageById.ContainsKey(_currentPage))
            {
                ClientUiTrace.Warn("导航", "Open(" + pageId + ") 被忽略：既不是 PopupLayer 弹窗，也未注册为 PagesLayer 页面");
                return;
            }

            ClientUiTrace.Line("导航", "Open(" + pageId + ") → PagesLayer 页面栈");
            Dispatch(_currentPage, PageAction.Pause);
            _history.Push(_currentPage);
            _currentPage = pageId;
            Dispatch(_currentPage, PageAction.Enter);
            NotifyObservers(_currentPage);
        }

        public void Back()
        {
            ResolvePopupService();
            if (_popupService != null && _popupService.CloseTop())
            {
                ClientUiTrace.Line("导航", "Back() → 关闭栈顶弹窗，剩余栈深=" + _popupService.OpenCount);
                NotifyObservers(CurrentPage);
                return;
            }
            ClientUiTrace.Line("导航", "Back() → 无弹窗可关，走 PagesLayer 返回栈（深度=" + _history.Count + "）");

            if (_history.Count == 0)
            {
                // 域重载或从 Inspector 直接激活页面时，返回栈可能为空；
                // 此时仍应允许用户回到主页，而不是留下无响应的返回按钮。
                if (_currentPage != ClientUiPageId.Home &&
                    _pageById.ContainsKey(_currentPage) &&
                    _pageById.ContainsKey(ClientUiPageId.Home))
                {
                    Dispatch(_currentPage, PageAction.Exit);
                    _currentPage = ClientUiPageId.Home;
                    Dispatch(_currentPage, PageAction.Resume);
                    NotifyObservers(_currentPage);
                }
                return;
            }

            if (!_pageById.ContainsKey(_currentPage))
                return;

            Dispatch(_currentPage, PageAction.Exit);
            _currentPage = _history.Pop();
            if (!_pageById.ContainsKey(_currentPage))
                return;
            Dispatch(_currentPage, PageAction.Resume);
            NotifyObservers(_currentPage);
        }

        /// <summary>用于一级页面的明确“返回主页”操作，不保留多余的导航历史。</summary>
        public void ReturnHome()
        {
            // 先收口弹窗栈，再回到大厅。
            ResolvePopupService();
            ClientUiTrace.Line("导航", "ReturnHome() → 关闭全部弹窗并回到大厅");
            if (_popupService != null)
                _popupService.CloseAll();

            if (!_pageById.ContainsKey(ClientUiPageId.Home))
            {
                NotifyObservers(ClientUiPageId.Home);
                return;
            }

            if (_currentPage != ClientUiPageId.Home && _pageById.ContainsKey(_currentPage))
                Dispatch(_currentPage, PageAction.Exit);

            _history.Clear();
            _currentPage = ClientUiPageId.Home;
            Dispatch(_currentPage, PageAction.Resume);
            NotifyObservers(_currentPage);
        }

        // ------------------------------------------------------------------
        // 派发
        // ------------------------------------------------------------------

        private void Dispatch(ClientUiPageId pageId, PageAction action)
        {
            IClientPageView view = ResolveView(pageId);
            if (view != null)
            {
                switch (action)
                {
                    case PageAction.Enter: view.OnPageEnter(); return;
                    case PageAction.Pause: view.OnPagePause(); return;
                    case PageAction.Resume: view.OnPageResume(); return;
                    case PageAction.Exit: view.OnPageExit(); return;
                }
                return;
            }

            // 尚未实现 IClientPageView 的页面退回旧的可见性开关，保证迁移期不断档。
            ClientUiPage page = Fallback(pageId);
            if (page == null)
            {
                Debug.LogWarning("[Client] 页面 " + pageId + " 既无 View 实现也未注册，导航动作被忽略。");
                return;
            }
            switch (action)
            {
                case PageAction.Enter: page.Enter(); break;
                case PageAction.Pause: page.Pause(); break;
                case PageAction.Resume: page.Resume(); break;
                case PageAction.Exit: page.Exit(); break;
            }
        }

        private IClientPageView ResolveView(ClientUiPageId pageId)
        {
            if (_viewById.TryGetValue(pageId, out IClientPageView cached))
                return cached;

            if (!_pageById.TryGetValue(pageId, out ClientUiPage page) || page == null)
                return null;

            // 场景中 View 组件与 ClientUiPage 位于同一节点（已核验 10/10）。
            // 只查本节点，不向下递归，避免误取子页面或全局控制器的实现。
            IClientPageView resolved = null;
            MonoBehaviour[] behaviours = page.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IClientPageView pageView)
                {
                    resolved = pageView;
                    break;
                }
            }

            if (resolved == null)
                Debug.LogWarning("[Client] 页面节点 " + page.name + " 上没有 IClientPageView 实现，将退回 SetActive 控制。");

            _viewById[pageId] = resolved;
            return resolved;
        }

        private ClientUiPage Fallback(ClientUiPageId pageId)
        {
            ClientUiPage page;
            return _pageById.TryGetValue(pageId, out page) ? page : null;
        }

        private void BindNavigationHosts()
        {
            MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            int bound = 0;
            _observers.Clear();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IClientNavigationHost host)
                {
                    host.BindNavigation(this);
                    bound++;
                }
                if (behaviours[i] is IClientNavigationObserver observer && !_observers.Contains(observer))
                    _observers.Add(observer);
            }
            if (bound > 0)
                Debug.Log("[Client] 导航能力已注入 " + bound + " 个页面视图，导航观察者 " + _observers.Count + " 个。");

            NotifyObservers(_currentPage);
        }

        private void NotifyObservers(ClientUiPageId pageId)
        {
            for (int i = 0; i < _observers.Count; i++)
            {
                if (_observers[i] != null)
                    _observers[i].OnNavigated(pageId);
            }
        }

        // ------------------------------------------------------------------
        // 静态事件适配（迁移期）
        // ------------------------------------------------------------------

        /// <summary>主页一级入口：目的地到页面 ID 的翻译属 Controller 职责，由内核承担。</summary>
        public void OpenFromHome(ClientHomeDestination destination)
        {
            Open(Map(destination));
        }

        /// <summary>
        /// 目的地当前能否真的打开：主页恒可；其余看**弹窗注册表**（`ClientPopupService` 的 pageId 表）
        /// 与 **PagesLayer 注册表**（`_pageById`）。
        ///
        /// 2026-09-20 只读普查结论：场景里可按 pageId 路由的弹窗**只有 9 个**
        /// （`Profile/Hero/HeroDetail/Shop/Mail/Rank/Activity/Formation/CompleteGuideEvent`）；
        /// 扭蛋 / 背包 / 任务 / 公告 / 弹珠 **在场景中不存在**，主页据此提示“开发中”。
        /// </summary>
        public bool CanOpenFromHome(ClientHomeDestination destination)
        {
            ClientUiPageId pageId = Map(destination);
            if (pageId == ClientUiPageId.Home)
                return true;

            ResolvePopupService();
            if (_popupService != null && _popupService.GetPopup(pageId) != null)
                return true;

            return _pageById.ContainsKey(pageId);
        }

        public void OpenHeroDetail(ClientHero hero, Sprite cardIllustration)
        {
            Open(ClientUiPageId.HeroDetail);
            ClientHeroDetailPage detail = ResolveHeroDetailPage();
            if (detail != null)
                detail.Show(hero, cardIllustration);
        }

        /// <summary>
        /// 解析英雄详情页节点。
        ///
        /// ⚠️ 2026-09-20 根因修复：`HeroDetail` 是 PopupLayer 的全屏弹窗，**不登记在 `_pageById`**
        /// （那里只有 PagesLayer 的 `ClientUiPage`，本场景仅 `MainPage主页` 一个，已用场景核对确认）。
        /// 原实现只查 `Fallback(HeroDetail)` → 恒为 `null` → 提前 `return` → `Show(hero, …)`
        /// **从未被调用**：未解锁只读不生效（页面停在启动时的 `SetReadOnly(false)`），
        /// `_currentHero` 也恒为 `null`，于是升级按钮即便装上了 onClick 也会在开头 `return`。
        /// 先向弹窗服务（弹窗注册表的所有者）要节点，再保留旧的页面注册表作为兼容路径。
        /// </summary>
        private ClientHeroDetailPage ResolveHeroDetailPage()
        {
            ResolvePopupService();
            if (_popupService != null)
            {
                ClientUiPopup popup = _popupService.GetPopup(ClientUiPageId.HeroDetail);
                if (popup != null)
                {
                    ClientHeroDetailPage fromPopup = popup.GetComponent<ClientHeroDetailPage>();
                    if (fromPopup != null)
                        return fromPopup;
                }
            }

            // 兼容路径：若 HeroDetail 将来回迁为 PagesLayer 页面，`Fallback` 仍然可用。
            ClientUiPage page = Fallback(ClientUiPageId.HeroDetail);
            return page != null ? page.GetComponent<ClientHeroDetailPage>() : null;
        }

        private void OpenCompleteGuideEvent(ClientActivity activity)
        {
            ClientCompleteGuideEventData data = ClientServices.IsInitialized
                ? ClientServices.Data.GetCompleteGuideEvent()
                : null;
            if (activity == null || data == null || activity.ActivityId != data.EventId)
                return;
            Open(ClientUiPageId.CompleteGuideEvent);
        }

        private void SubscribeCompleteGuideEntry()
        {
            if (_completeGuideSubscribed || _completeGuideActivityPage == null) return;
            _completeGuideActivityPage.SeasonalActivityRequested += OpenCompleteGuideEvent;
            _completeGuideSubscribed = true;
        }

        private void UnsubscribeCompleteGuideEntry()
        {
            if (!_completeGuideSubscribed || _completeGuideActivityPage == null) return;
            _completeGuideActivityPage.SeasonalActivityRequested -= OpenCompleteGuideEvent;
            _completeGuideSubscribed = false;
        }

        public void ConfigureCompleteGuideEntry(ClientActivityPage activityPage)
        {
            UnsubscribeCompleteGuideEntry();
            _completeGuideActivityPage = activityPage;
            SubscribeCompleteGuideEntry();
        }

        private static ClientUiPageId Map(ClientHomeDestination destination)
        {
            switch (destination)
            {
                case ClientHomeDestination.Profile: return ClientUiPageId.Profile;
                case ClientHomeDestination.Hero: return ClientUiPageId.Hero;
                case ClientHomeDestination.Backpack: return ClientUiPageId.Backpack;
                case ClientHomeDestination.Gacha: return ClientUiPageId.Gacha;
                case ClientHomeDestination.Shop: return ClientUiPageId.Shop;
                case ClientHomeDestination.Mail: return ClientUiPageId.Mail;
                case ClientHomeDestination.Rank: return ClientUiPageId.Rank;
                case ClientHomeDestination.Activity: return ClientUiPageId.Activity;
                case ClientHomeDestination.Task: return ClientUiPageId.Task;
                case ClientHomeDestination.Formation: return ClientUiPageId.Formation;
                case ClientHomeDestination.Marble: return ClientUiPageId.Marble;
                default: return ClientUiPageId.Notice;
            }
        }
    }
}
