using System;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public enum ClientHomeDestination
    {
        Profile,
        Hero,
        Backpack,
        Shop,
        Mail,
        Rank,
        Activity,
        Task,
        Gacha,
        Notice
        ,Formation
        ,Marble
    }

    /// <summary>
    /// 主页面的数据绑定与导航入口。页面层级由场景维护（ClientShell.unity）。
    /// </summary>
    public sealed class ClientHomePage : ClientPageViewBase, IClientNavigationHost, IClientPopupHost, IClientFeedbackHost
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text _playerName;
        [SerializeField] private Text _combatPower;
        [SerializeField] private Text _notice;
        [SerializeField] private ClientBottomNavigationBar _bottomNavigation;
        private bool _canBindData;
        private IClientHomeNavigation _homeNavigation;
        private IClientPopupService _popups;
        private IClientFeedback _feedback;

        // 主页的"联系客服"图标弹出的是 PopupLayer 里的 联系客服Page。
        // 它不属于 ClientUiPageId 路由，因此用序列化引用直接打开（工具自动写入 / 也可手动拖拽）。
        [SerializeField] private GameObject _contactServicePopup;

        public void BindPopups(IClientPopupService popups) { _popups = popups; }

        /// <summary>由组合根注入全局提示（`SystemLayer` 的 `ClientSystemFeedback`）。</summary>
        public void BindFeedback(IClientFeedback feedback) { _feedback = feedback; }

        /// <summary>由导航内核注入，取代原先的 static event NavigationRequested。</summary>
        public void BindNavigation(IClientNavigation navigation)
        {
            _homeNavigation = navigation as IClientHomeNavigation;
        }

        protected override GameObject PageRoot { get { return _pageRoot; } }
        // Start 之前不刷新，避免服务未初始化时抛异常；导航绑定仍在 Start/OnEnable 完成。
        protected override void OnPageRefresh() { if (_canBindData) Refresh(); }

        private void OnEnable()
        {
            if (_canBindData)
            {
                BindData();
                BindSceneNavigation();
                SelectBottomHome();
            }
        }

        private void Start()
        {
            // Start 在场景内全部 Awake 完成后调用，避免依赖根节点的启用顺序。
            _canBindData = true;
            BindData();
            BindSceneNavigation();
            SelectBottomHome();
        }

        private void BindSceneNavigation()
        {
            BindPrefabNavigation("TeamButton", ClientHomeDestination.Formation);
            BindPrefabNavigation("GachaButton", ClientHomeDestination.Gacha);
            BindPrefabNavigation("AvatarButton", ClientHomeDestination.Profile);
            BindPrefabNavigation("ProfileButton", ClientHomeDestination.Profile);
            // 负责人确认：MarbleButton 进入 HeroPage英雄 界面，而不是独立的弹珠页（弹珠页未实现）。
            BindPrefabNavigation("MarbleButton", ClientHomeDestination.Hero);
            BindPrefabNavigation("BackpackButton", ClientHomeDestination.Backpack);
            BindPrefabNavigation("ActivityButton", ClientHomeDestination.Activity);
            BindBottomHomeButton();
            // 真实节点名（已用场景结构核验）：底部功能图标是 BottomFunctionIcon_*。
            // 原来的 RankButton / ShopButton / MailButton / SupportButton / ScanButton 在场景中并不存在，
            // 保留只会在每次进主页时刷 5 条"未找到…未绑定导航"警告，故移除。
            BindPrefabNavigation("BottomFunctionIcon_Rank", ClientHomeDestination.Rank);
            BindPrefabNavigation("BottomFunctionIcon_Shop", ClientHomeDestination.Shop);
            BindPrefabNavigation("BottomFunctionIcon_Mail", ClientHomeDestination.Mail);
            BindContactServiceButton();
        }

        /// <summary>
        /// 主页的"联系客服"图标：负责人确认它弹出 联系客服Page（现位于 PopupLayer）。
        /// 该弹窗不参与 ClientUiPageId 路由，因此用序列化引用直接交给弹窗服务打开。
        /// </summary>
        private void BindContactServiceButton()
        {
            if (_contactServicePopup == null)
            {
                Debug.LogWarning("[Client] 未指定联系客服弹窗（_contactServicePopup），BottomFunctionIcon 点击无效。");
                return;
            }

            foreach (Button button in _pageRoot.GetComponentsInChildren<Button>(true))
            {
                if (button.name != "BottomFunctionIcon")
                    continue;
                button.onClick = new Button.ButtonClickedEvent();
                GameObject popupNode = _contactServicePopup;
                button.onClick.AddListener(() =>
                {
                    if (_popups == null)
                    {
                        Debug.LogWarning("[Client] 弹窗服务未注入，联系客服弹窗无法打开。");
                        return;
                    }
                    ClientUiPopup popup = popupNode.GetComponent<ClientUiPopup>();
                    if (popup != null)
                        _popups.Open(popup);
                });
                BindContactPopupClose(popupNode);
                return;
            }

            Debug.LogWarning("[Client] MainPage 中未找到 BottomFunctionIcon（联系客服入口），未绑定。");
        }

        /// <summary>
        /// **2026-09-21（负责人反馈：客服会话界面点"确定键"或空白处都返回不了上一级）** —— 两条关闭路径都补上：
        ///
        /// <list type="number">
        /// <item>**确定键 / 关闭键**：按常见命名在弹窗内查找（`确定键Button` / `确定Button` / `确定` / `关闭键Button` /
        /// `关闭Button`），找到就绑定**关闭该弹窗**；找不到只记一条日志，不报错（不同弹窗命名可能不同）。</item>
        /// <item>**点空白处**：给弹窗自己的背景图（`background` / `底框` / 根节点上的全屏 Image）绑定点击关闭，
        /// 作为**遮罩点击**（`ClientPopupService` 已统一处理）之外的兜底 —— 有些弹窗不在弹窗栈上。</item>
        /// </list>
        ///
        /// 全程**不改场景、不加节点**；重复调用只重绑监听。
        /// </summary>
        private void BindContactPopupClose(GameObject popupNode)
        {
            if (popupNode == null)
                return;

            // 1) 确定键 / 关闭键
            string[] closeNames = { "确定键Button", "确定Button", "确定", "关闭键Button", "关闭Button", "退出Button" };
            bool boundClose = false;
            Transform[] all = popupNode.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length && !boundClose; i++)
            {
                for (int n = 0; n < closeNames.Length; n++)
                {
                    if (all[i].name != closeNames[n])
                        continue;

                    Button button = all[i].GetComponent<Button>();
                    if (button == null)
                        continue;

                    GameObject target = popupNode;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => CloseContactPopup(target));
                    Debug.Log("[Client] 联系客服弹窗：已把「" + all[i].name + "」绑定为关闭。");
                    boundClose = true;
                    break;
                }
            }

            if (!boundClose)
                Debug.Log("[Client] 联系客服弹窗：未找到确定/关闭键节点（可用遮罩点击关闭）。");

            // 2) 点背景空白关闭（兜底）
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name != "background" && all[i].name != "底框" && all[i].name != "底框Image")
                    continue;

                Image image = all[i].GetComponent<Image>();
                if (image == null)
                    continue;

                image.raycastTarget = true;
                Button background = all[i].GetComponent<Button>();
                if (background == null)
                    background = all[i].gameObject.AddComponent<Button>();
                background.transition = Selectable.Transition.None;

                GameObject target = popupNode;
                background.onClick.RemoveAllListeners();
                background.onClick.AddListener(() => CloseContactPopup(target));
                Debug.Log("[Client] 联系客服弹窗：已把「" + all[i].name + "」绑定为点空白关闭。");
                break;
            }
        }

        /// <summary>关闭联系客服弹窗：优先走弹窗服务（在栈中则正常出栈），再兜底隐藏节点（幂等）。</summary>
        private void CloseContactPopup(GameObject popupNode)
        {
            if (popupNode == null)
                return;

            // `IClientPopupService.Close` 的语义是"关闭指定弹窗（**若在栈中**）"，不在栈中也不会抛异常，
            // 因此这里可以直接调用，再用 SetActive(false) 兜底覆盖"该弹窗不在弹窗栈上"的情况。
            ClientUiPopup popup = popupNode.GetComponent<ClientUiPopup>();
            if (_popups != null && popup != null)
                _popups.Close(popup);

            popupNode.SetActive(false);
        }

        private void OnDisable()
        {
            if (!_canBindData)
                return;

            try
            {
                ClientServices.Data.DataChanged -= Refresh;
            }
            catch (InvalidOperationException)
            {
                // 编辑器域重载时服务可能已释放，无需影响页面销毁。
            }
        }

        private void BindData()
        {
            ClientServices.Data.DataChanged -= Refresh;
            ClientServices.Data.DataChanged += Refresh;
            Refresh();
        }

        private void BindPrefabNavigation(string buttonName, ClientHomeDestination destination)
        {
            foreach (Button button in _pageRoot.GetComponentsInChildren<Button>(true))
            {
                if (button.name != buttonName)
                    continue;

                // 场景预制体可能保留旧导航器的回调。每次主页重新显示时只接入
                // 当前 Canvas 页面栈，避免旧回调吞掉或并行处理点击。
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(() => RequestNavigation(destination));
                return;
            }

            Debug.LogWarning("[Client] MainPage 中未找到 " + buttonName + "，未绑定导航。");
        }

        public void Configure(GameObject pageRoot, Text playerName, Text combatPower, Text notice)
        {
            _pageRoot = pageRoot;
            _playerName = playerName;
            _combatPower = combatPower;
            _notice = notice;
        }

        public void RequestNavigation(ClientHomeDestination destination)
        {
            // 场景里**尚未实现**的页面（扭蛋 / 背包 / 任务 / 公告 / 弹珠）⇒ 提示“开发中”后直接返回。
            // 依据 2026-09-20 只读普查：可按 pageId 路由的弹窗只有 9 个，这几个在场景中不存在；
            // 若照旧调用导航，只会被 `Open` 忽略 —— 表现为“点了没反应”。
            // 放在最前面还有第二个好处：不可达时**不改底部页签选中态**。
            if (_homeNavigation != null && !_homeNavigation.CanOpenFromHome(destination))
            {
                ShowNotice(GetDestinationLabel(destination) + "功能开发中");
                return;
            }

            if (_bottomNavigation != null)
            {
                if (destination == ClientHomeDestination.Profile) _bottomNavigation.Select("ProfileButton");
                else if (destination == ClientHomeDestination.Marble) _bottomNavigation.Select("MarbleButton");
                else if (destination == ClientHomeDestination.Backpack) _bottomNavigation.Select("BackpackButton");
                else if (destination == ClientHomeDestination.Activity) _bottomNavigation.Select("ActivityButton");
            }
            if (_homeNavigation != null)
                _homeNavigation.OpenFromHome(destination);
            else
                Debug.LogWarning("[Client] 主页导航尚未就绪，未处理目的地：" + destination);
        }

        public void ConfigureBottomNavigation(ClientBottomNavigationBar bottomNavigation) { _bottomNavigation = bottomNavigation; }

        public void SetVisible(bool visible)
        {
            if (_pageRoot != null)
                _pageRoot.SetActive(visible);
        }

        /// <summary>
        /// 主页提示。**优先走全局 Toast（有视觉）**；没有组合根注入时才退回主页自己的文本槽位。
        ///
        /// 2026-09-20 取证：`_notice` 在场景里是 `fileID 0`（未接线），所以此前 `ShowNotice`
        /// 只在 Console 里有反应 —— 负责人反馈“只在日志弹有点太拉了”。
        /// 全局 Toast 由 `ClientShellStructuralRepair.EnsureSystemToast()` 在 `SystemLayer` 下建好并接线。
        /// </summary>
        public void ShowNotice(string message)
        {
            if (_feedback != null)
                _feedback.ShowToast(message);
            else if (_notice != null)
            {
                _notice.text = message;
                _notice.gameObject.SetActive(true);
            }

            Debug.Log("[Client] " + message);
        }

        private void Refresh()
        {
            if (_playerName == null || _combatPower == null)
                return;

            ClientPlayerProfile profile = ClientServices.Data.GetProfile();
            _playerName.text = profile.DisplayName;
            _combatPower.text = profile.CombatPower.ToString();
        }

        private void BindBottomHomeButton()
        {
            foreach (Button button in _pageRoot.GetComponentsInChildren<Button>(true))
            {
                if (button.name != "HomeButton") continue;
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(() => { SelectBottomHome(); ShowNotice("当前已在主页"); });
                return;
            }
        }

        private void SelectBottomHome()
        {
            if (_bottomNavigation != null) _bottomNavigation.Select("HomeButton");
        }

        private static string GetDestinationLabel(ClientHomeDestination destination)
        {
            switch (destination)
            {
                case ClientHomeDestination.Hero: return "英雄";
                case ClientHomeDestination.Backpack: return "背包";
                case ClientHomeDestination.Shop: return "商城";
                case ClientHomeDestination.Mail: return "邮箱";
                case ClientHomeDestination.Rank: return "排行榜";
                case ClientHomeDestination.Activity: return "活动";
                case ClientHomeDestination.Task: return "任务";
                case ClientHomeDestination.Gacha: return "扭蛋";
                case ClientHomeDestination.Notice: return "公告";
                default: return "客户端";
            }
        }
    }
}
