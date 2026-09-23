using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client.UI
{
    /// <summary>
    /// PopupLayer 的唯一所有者：弹窗栈、遮罩显隐、输入阻断与关闭回调。
    ///
    /// 设计要点：
    /// - 遮罩（mask）是可选的序列化槽位——把 PopupLayer 下的遮罩节点拖进来即可，
    ///   服务只负责按栈深度显隐它，不改动它的尺寸与排版。
    /// - 实现 IClientNavigationObserver：页面级导航发生时自动 CloseAll()，
    ///   从结构上保证"切页之后不遗留遮罩与输入阻断"。
    /// - 复用既有的"二级遮罩"等节点作为遮罩，不新建 UI。
    /// </summary>
    public sealed class ClientPopupService : MonoBehaviour, IClientPopupService
    {
        [Tooltip("遮罩根节点（可空）。拖入 PopupLayer 下的遮罩节点即可，服务不改其尺寸。")]
        [SerializeField] private GameObject _maskRoot;

        [Tooltip("弹窗父节点（可空）。设置后打开的弹窗会被置于末尾以保证渲染在上层。")]
        [SerializeField] private Transform _popupLayer;

        /// <summary>PopupLayer 下的全部弹窗（含不参与 pageId 路由的二级弹窗），用于模态控制。</summary>
        private readonly List<ClientUiPopup> _allPopups = new List<ClientUiPopup>();

        /// <summary>大厅根节点（PagesLayer 的第一个子节点），弹窗打开时需要屏蔽它的射线。</summary>
        private Transform _lobbyRoot;

        private readonly List<ClientUiPopup> _stack = new List<ClientUiPopup>();
        private readonly Dictionary<ClientUiPageId, ClientUiPopup> _popupById =
            new Dictionary<ClientUiPageId, ClientUiPopup>();

        public bool HasOpenPopup { get { return _stack.Count > 0; } }

        public int OpenCount { get { return _stack.Count; } }

        private void Awake()
        {
            // _popupLayer 未指定时默认取自身——本组件就挂在 PopupLayer 上。
            //
            // 没有它，Open() 末尾的 SetAsLastSibling 会被整段跳过，后打开的弹窗
            // 只能按场景里原始的兄弟顺序渲染，于是会被先打开的整屏界面盖住。
            // 实测证据：点商城道具卡后 Product Purchase Interface 确实被打开
            // （日志 Open 栈深=2），但它在 PopupLayer 中是第 11 个子节点，
            // 而 ShopPage商店 是第 4 个 → 购买界面被商店挡在后面，视觉上像"没反应"。
            if (_popupLayer == null)
                _popupLayer = transform;

            if (_maskRoot != null)
            {
                _maskRoot.SetActive(false);
                EnsureMaskClickClosesTop();
            }
            CollectPopups();
        }

        /// <summary>
        /// **2026-09-21（负责人反馈：客服会话界面点"确定键"或空白处都返回不了上一级）**：
        /// 遮罩此前**只被显隐、没有任何点击处理** ⇒ 点空白处毫无反应 —— 这是**所有**弹窗的共同问题。
        ///
        /// 这里在**运行时**给遮罩补一个 `Button`（不写场景、不改尺寸与排版），点击即关闭**栈顶**弹窗，
        /// 即"点空白返回上一级"。遮罩的 `raycastTarget` 本来就为 `true`（用于阻断下层输入），
        /// 因此它天然能接收点击，不需要额外加图。
        ///
        /// 幂等：已存在则只重绑监听；`transition = None` 避免遮罩出现按下变色动画。
        /// </summary>
        private void EnsureMaskClickClosesTop()
        {
            Button button = _maskRoot.GetComponent<Button>();
            if (button == null)
            {
                button = _maskRoot.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
            }

            button.onClick.RemoveListener(OnMaskClicked);
            button.onClick.AddListener(OnMaskClicked);
        }

        /// <summary>遮罩被点击：关闭栈顶弹窗（没有弹窗时什么也不做）。</summary>
        private void OnMaskClicked()
        {
            if (_stack.Count > 0)
                CloseTop();
        }

        /// <summary>
        /// 把 PopupLayer 下实际存在的弹窗收录为"页面 ID → 弹窗"路由表。
        /// 与 ClientUiNavigator 自动收录页面同理：不再依赖人工维护的序列化列表。
        /// </summary>
        private void CollectPopups()
        {
            _popupById.Clear();
            _allPopups.Clear();
            ClientUiPopup[] found = GetComponentsInChildren<ClientUiPopup>(true);
            int routed = 0;
            int referenceOnly = 0;
            for (int i = 0; i < found.Length; i++)
            {
                ClientUiPopup popup = found[i];
                if (popup == null)
                    continue;

                // 模态控制需要"全部弹窗"的清单（含不参与 pageId 路由的二级弹窗）。
                _allPopups.Add(popup);

                // 二级弹窗由调用方持引用直接 Open，不参与 pageId 路由，避免与一级界面争抢 ID。
                if (!popup.RouteByPageId)
                {
                    referenceOnly++;
                    continue;
                }

                if (_popupById.ContainsKey(popup.PageId))
                {
                    Debug.LogError("[Client] 弹窗页面 ID 重复：" + popup.PageId +
                                   "，节点 " + _popupById[popup.PageId].name + " 将被 " + popup.name + " 覆盖。");
                }
                _popupById[popup.PageId] = popup;
                routed++;
            }
            Debug.Log("[Client] PopupLayer 收录弹窗：按 pageId 路由 " + routed +
                      " 个，仅按引用打开 " + referenceOnly + " 个。");
        }

        /// <summary>按页面 ID 打开弹窗。返回是否命中路由表。</summary>
        public bool TryOpen(ClientUiPageId pageId)
        {
            if (_popupById.Count == 0)
                CollectPopups();
            ClientUiPopup popup;
            if (!_popupById.TryGetValue(pageId, out popup) || popup == null)
                return false;
            Open(popup);
            return true;
        }

        /// <summary>
        /// 按页面 ID 取出**已登记**的弹窗节点（未登记返回 null，不打开任何东西）。
        ///
        /// 为什么需要它：`ClientUiNavigator.Open(pageId)` 打开弹窗之后，还要拿到该页面的 View
        /// 才能喂数据（例如英雄详情的 `Show(hero, …)`）。而 `_pageById` 里只有 PagesLayer 的
        /// `ClientUiPage`（本场景仅 `MainPage主页` 一个），弹窗从来不在其中 ——
        /// 于是 `Fallback(HeroDetail)` 恒为 null，页面被打开了却从未被初始化。
        /// 弹窗注册表归本服务所有（§12.4 同一所有者原则），就应由本服务回答"pageId 对应哪个节点"。
        /// </summary>
        public ClientUiPopup GetPopup(ClientUiPageId pageId)
        {
            if (_popupById.Count == 0)
                CollectPopups();
            ClientUiPopup popup;
            return _popupById.TryGetValue(pageId, out popup) ? popup : null;
        }

        /// <summary>按页面 ID 关闭弹窗。返回是否命中。</summary>
        public bool TryClose(ClientUiPageId pageId)
        {
            ClientUiPopup popup;
            if (!_popupById.TryGetValue(pageId, out popup) || popup == null)
                return false;
            Close(popup);
            return true;
        }

        /// <summary>当前栈顶弹窗对应的页面 ID；栈空时视为主页（大厅）。</summary>
        public ClientUiPageId CurrentPageId
        {
            get
            {
                return _stack.Count > 0 && _stack[_stack.Count - 1] != null
                    ? _stack[_stack.Count - 1].PageId
                    : ClientUiPageId.Home;
            }
        }

        /// <summary>栈中是否已经打开了某个页面 ID 的弹窗。</summary>
        public bool IsOpen(ClientUiPageId pageId)
        {
            for (int i = 0; i < _stack.Count; i++)
                if (_stack[i] != null && _stack[i].PageId == pageId)
                    return true;
            return false;
        }

        public void Open(ClientUiPopup popup)
        {
            if (popup == null || _stack.Contains(popup))
                return;

            // 新弹窗覆盖当前栈顶：栈顶只被**挂起**——保留可见、停止交互。
            //
            // ⚠️ 原实现在此调用了 previousTop.SetVisible(false)，把被覆盖的那一层直接隐藏。
            // 那是"整屏页面替换"时期的语义，与负责人要求的**层层覆盖**相反，实测后果：
            //   打开购买界面 → 商店被隐藏 → 身后露出大厅（负责人："商店被推到主页"）
            //   打开成功页   → 购买界面也被隐藏 → 三层只剩最上面一层可见
            // 栈快照证据：Open Purchase Success Page 后
            //   [0] ShopPage商店 activeSelf=False
            //   [1] Product Purchase Interface activeSelf=False
            //   [2] Purchase Success Page activeSelf=True
            // 因此这里**只挂起、不隐藏**；隐藏只发生在 Close。
            ClientUiPopup previousTop = _stack.Count > 0 ? _stack[_stack.Count - 1] : null;
            if (previousTop != null)
                NotifyView(previousTop, PopupAction.Suspend);

            _stack.Add(popup);

            ApplyStackOrder();

            popup.SetVisible(true);
            NotifyView(popup, PopupAction.Open);

            ClientUiTrace.Line("弹窗", "Open " + ClientUiTrace.Path(popup.Root) +
                                       "  pageId=" + (popup.RouteByPageId ? popup.PageId.ToString() : "(不路由)") +
                                       "  栈深=" + _stack.Count);
            DumpVisibility(popup.Root);
            ApplyMask();
        }

        /// <summary>
        /// 诊断：弹窗被 Open 之后到底可不可见。
        ///
        /// 负责人反馈"点卡牌后购买界面不呈现"，但日志显示 Open 确实执行了（栈深=2）。
        /// 仅凭调用成功无法判断可见性，故直接打印运行时真实状态：
        /// 自身/祖先激活情况、兄弟序号（是否被压在其它弹窗之后）、缩放、CanvasGroup alpha、Rect。
        /// 定位到原因后本方法可移除。
        /// </summary>
        private void DumpVisibility(GameObject root)
        {
            if (root == null)
            {
                ClientUiTrace.Warn("弹窗", "可见性诊断：root 为 null");
                return;
            }

            Transform parent = root.transform.parent;
            int lastIndex = parent != null ? parent.childCount - 1 : -1;
            RectTransform rect = root.GetComponent<RectTransform>();
            CanvasGroup group = root.GetComponent<CanvasGroup>();

            // 找出第一个把它连坐隐藏的祖先。
            string blocker = "(无)";
            Transform current = root.transform.parent;
            int guard = 0;
            while (current != null && guard < 32)
            {
                if (!current.gameObject.activeSelf)
                {
                    blocker = current.name;
                    break;
                }
                current = current.parent;
                guard++;
            }

            ClientUiTrace.Line("弹窗", "可见性诊断 " + ClientUiTrace.Path(root) +
                "\n    activeSelf=" + root.activeSelf +
                "  activeInHierarchy=" + root.activeInHierarchy +
                "  首个未激活祖先=" + blocker +
                "\n    兄弟序号=" + root.transform.GetSiblingIndex() + "/" + lastIndex +
                "  localScale=" + root.transform.localScale.ToString("F2") +
                "  canvasGroupAlpha=" + (group != null ? group.alpha.ToString("F2") : "(无CanvasGroup)") +
                "\n    anchoredPos=" + (rect != null ? rect.anchoredPosition.ToString("F1") : "(无Rect)") +
                "  size=" + (rect != null ? rect.rect.size.ToString("F1") : "(无Rect)"));

            DumpStack();
        }

        /// <summary>
        /// 打印当前栈内**每一个**弹窗的可见性。
        ///
        /// 由来：负责人截图显示成功页身后是主页，而日志却是 棧深=3——
        /// 说明栈里的一级/二级弹窗被压着但**没有显示**。只看新开的那一个不足以定位，
        /// 必须同时看到栈内其它成员的 activeSelf 才能判断是谁把谁藏了。
        /// </summary>
        /// <summary>
        /// 按栈顺序整体重排 PopupLayer 的兄弟次序（自底向顶依次置后，最终顺序 = 栈顺序）。
        ///
        /// 为什么不能只把"新打开的那一个"置后：各页面 View 里还残留"整屏替换"时期的
        /// **越权置顶**调用（例如 ClientShopPage.Show 里的 BringToFront → SetAsLastSibling），
        /// 会把下层重新提到最上，造成**兄弟序号对、显示却反了**。
        /// 实测：栈快照显示 商店16 &lt; 购买17 &lt; 成功18（顺序本应正确），
        /// 但界面上商店仍压在最上，即为页面侧事后改写所致。
        /// 每次打开后整体重排，层级由服务唯一决定。
        /// </summary>
        private void ApplyStackOrder()
        {
            if (_popupLayer == null)
                return;

            for (int i = 0; i < _stack.Count; i++)
            {
                ClientUiPopup item = _stack[i];
                if (item == null || item.Root == null)
                    continue;
                if (item.Root.transform.parent != _popupLayer)
                    continue;
                item.Root.transform.SetAsLastSibling();
            }
        }

        private void DumpStack()
        {
            if (_stack.Count == 0)
            {
                ClientUiTrace.Line("弹窗", "栈快照：(空)");
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("栈快照（自底向顶，共 ").Append(_stack.Count).Append(" 层）:");
            for (int i = 0; i < _stack.Count; i++)
            {
                ClientUiPopup item = _stack[i];
                if (item == null || item.Root == null)
                    continue;
                builder.Append("\n    [").Append(i).Append("] ").Append(item.Root.name)
                       .Append("  activeSelf=").Append(item.Root.activeSelf)
                       .Append("  activeInHierarchy=").Append(item.Root.activeInHierarchy)
                       .Append("  兄弟=").Append(item.Root.transform.GetSiblingIndex())
                       .Append("  ").Append(DescribeCanvasChain(item.Root));
            }
            ClientUiTrace.Line("弹窗", builder.ToString());
        }

        /// <summary>
        /// 报告该节点自身及其子树中"会覆盖层级"的 Canvas 设置。
        ///
        /// 为什么需要：实测出现"兄弟序号正确、渲染顺序却相反"的情况，
        /// 那只能是某个嵌套 Canvas 用 overrideSorting + sortingOrder 覆盖了层级。
        /// 只查兄弟序号会得出"顺序没问题"的错误结论。
        /// </summary>
        private static string DescribeCanvasChain(GameObject root)
        {
            if (root == null)
                return "(null)";

            StringBuilder text = new StringBuilder();
            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null)
                    continue;
                text.Append("Canvas[").Append(ClientUiTrace.Path(canvas.transform))
                    .Append(" override=").Append(canvas.overrideSorting)
                    .Append(" order=").Append(canvas.sortingOrder)
                    .Append("] ");
            }
            if (text.Length == 0)
                text.Append("(子树内无 Canvas)");
            return text.ToString();
        }

        public void Close(ClientUiPopup popup)
        {
            if (popup == null)
                return;
            int index = _stack.IndexOf(popup);
            if (index < 0)
                return;

            bool wasTop = index == _stack.Count - 1;
            _stack.RemoveAt(index);
            popup.SetVisible(false);
            NotifyView(popup, PopupAction.Close);

            if (wasTop && _stack.Count > 0)
            {
                ClientUiPopup newTop = _stack[_stack.Count - 1];
                newTop.SetVisible(true);
                NotifyView(newTop, PopupAction.Resume);
                ClientUiTrace.Line("弹窗", "Resume " + ClientUiTrace.Path(newTop.Root));
            }

            ClientUiTrace.Line("弹窗", "Close " + ClientUiTrace.Path(popup.Root) + "  栈深=" + _stack.Count);
            ApplyMask();
        }

        public bool CloseTop()
        {
            if (_stack.Count == 0)
                return false;
            Close(_stack[_stack.Count - 1]);
            return true;
        }

        public void CloseAll()
        {
            // 从栈顶往下关闭，保证每个弹窗都能收到 Resume/Close 的正确顺序。
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                ClientUiPopup popup = _stack[i];
                if (popup == null)
                    continue;
                popup.SetVisible(false);
                NotifyView(popup, PopupAction.Close);
            }
            ClientUiTrace.Line("弹窗", "CloseAll 关闭 " + _stack.Count + " 个弹窗");
            _stack.Clear();
            ApplyMask();
        }

        // 注意：本服务**刻意不实现** IClientNavigationObserver。
        //
        // 曾经实现在这里收到"页面已切换"就 CloseAll()，用于防止遮罩跨页遗留。
        // 但导航器 Open(pageId) 的流程是：TryOpen(pageId) 打开弹窗 → NotifyObservers(pageId)。
        // 于是弹窗刚被压栈，就立刻被自己的回调关掉，表现为"点了没反应、不跳转"。
        //
        // 收口职责改由导航器承担：ClientUiNavigator.ReturnHome() 会调用 CloseAll()，
        // 而 Back() 会 CloseTop()，两者都只在真正需要出栈时执行。

        private void ApplyMask()
        {
            ApplyModalState();

            if (_maskRoot == null)
                return;

            bool needMask = false;
            for (int i = 0; i < _stack.Count; i++)
            {
                if (_stack[i] != null && _stack[i].BlocksInput)
                {
                    needMask = true;
                    break;
                }
            }
            if (_maskRoot.activeSelf != needMask)
            {
                ClientUiTrace.Line("弹窗", "遮罩 " + ClientUiTrace.Path(_maskRoot) + " visible=" + needMask);
                _maskRoot.SetActive(needMask);
            }
            if (!needMask)
                return;

            // 遮罩必须夹在"**栈顶弹窗之下、其余弹窗之上**"。
            //
            // 原来直接 SetAsLastSibling() 是错的——那会把遮罩放到栈顶弹窗之上，
            // 连弹窗自己都点不动了。正确做法：先把栈顶弹窗压到最上，
            // 再把遮罩插到它前面一个位置。
            if (_stack.Count > 0 && _stack[_stack.Count - 1] != null)
            {
                Transform top = _stack[_stack.Count - 1].Root.transform;
                Transform parent = _maskRoot.transform.parent;
                if (parent != null && top.parent == parent)
                {
                    top.SetAsLastSibling();
                    _maskRoot.transform.SetSiblingIndex(Mathf.Max(0, top.GetSiblingIndex() - 1));
                    return;
                }
            }

            _maskRoot.transform.SetAsLastSibling();
        }

        // ------------------------------------------------------------------
        // 模态（MODULE.md §6：每页根自带 CanvasGroup，由它控制 blocksRaycasts）
        //
        // 为什么必须有：撤掉全局遮罩后，全屏弹窗打开时**主页底部图标仍然可点**，
        // 点击穿透会让每点一次就往栈上再叠一层，用户只看到最上面那层、
        // 等上层关掉下面那些"早就打开了"的弹窗才依次显现 —— 就是之前那个"延迟弹出"现象。
        // 全局遮罩曾用来堵这个洞，但它同时盖住了栈顶弹窗（购买界面点不动）且会残留锁死界面，已作废。
        // 正解是**逐节点屏蔽射线**：只有栈顶弹窗可点，其下的弹窗与大厅一律屏蔽。
        // 仅添加 CanvasGroup 交互组件，不新建 UI 节点、不改任何 rect。
        // ------------------------------------------------------------------

        private void ApplyModalState()
        {
            // ============================================================
            // 【2026-09-20 临时停用 —— 待重新取证】
            //
            // 负责人 Play 反馈：进商店显示 3 张卡（模板生成正常），点任意一张卡后
            // **没有出现购买界面**，界面像"回到主页且完全点不动"。
            //
            // 日志（Editor.log）证据：
            //   点击 (800.48,1605.99) 命中 .../Content/道具卡牌Item
            //   Open .../Product Purchase Interface  栈深=2
            //   模态 .../ShopPage商店 blocksRaycasts=False
            //   模态 .../Product Purchase Interface blocksRaycasts=True
            //   其后点击全部"未命中任何 UI（点到了空处）"
            //   **ReturnHome 0 次、Back() 0 次** —— 不是导航导致
            //
            // 即：服务层认为购买界面已打开并把它设为唯一可点对象，但它在屏幕上**并未呈现**，
            // 于是商店与大厅都被屏蔽、点什么都不命中。当前证据不足以解释"为何不呈现"，
            // 因此**先行停用本机制**，恢复到此改动之前的行为，先把界面解封；
            // 待重新取证（查明购买界面未呈现的真正原因）后再决定是否重启。
            //
            // 注意：停用后"弹窗打开时点击穿透到下层界面"的问题会回来，
            // 这是已知的、被这次更严重的问题所掩盖的旧问题。
            // ============================================================
            return;

#pragma warning disable CS0162
            bool anyOpen = _stack.Count > 0;
            Transform top = anyOpen && _stack[_stack.Count - 1] != null
                ? _stack[_stack.Count - 1].Root.transform
                : null;

            SetRaycastBlocked(ResolveLobbyRoot(), anyOpen);

            for (int i = 0; i < _allPopups.Count; i++)
            {
                ClientUiPopup popup = _allPopups[i];
                if (popup == null || popup.Root == null)
                    continue;
                bool isTop = anyOpen && popup.Root.transform == top;
                SetRaycastBlocked(popup.Root.transform, anyOpen && !isTop);
            }
#pragma warning restore CS0162
        }

        private Transform ResolveLobbyRoot()
        {
            if (_lobbyRoot != null)
                return _lobbyRoot;

            Transform canvas = transform.parent;
            if (canvas == null)
                return null;
            Transform pagesLayer = canvas.Find("PagesLayer");
            if (pagesLayer == null || pagesLayer.childCount == 0)
                return null;

            _lobbyRoot = pagesLayer.GetChild(0);
            return _lobbyRoot;
        }

        /// <summary>按需补一个 CanvasGroup 并设置 blocksRaycasts；只加交互组件，不动排版。</summary>
        private static void SetRaycastBlocked(Transform node, bool blocked)
        {
            if (node == null)
                return;

            CanvasGroup group = node.GetComponent<CanvasGroup>();
            if (group == null)
                group = node.gameObject.AddComponent<CanvasGroup>();

            bool wanted = !blocked;
            if (group.blocksRaycasts == wanted)
                return;

            group.blocksRaycasts = wanted;
            ClientUiTrace.Line("弹窗", "模态 " + ClientUiTrace.Path(node) + " blocksRaycasts=" + wanted);
        }

        private enum PopupAction { Open, Suspend, Resume, Close }

        private static void NotifyView(ClientUiPopup popup, PopupAction action)
        {
            // 弹窗专用脚本可选：与 ClientUiPopup 同节点即被发现，无需额外接线。
            MonoBehaviour[] behaviours = popup.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                IClientPopupView view = behaviours[i] as IClientPopupView;
                if (view == null)
                    continue;
                switch (action)
                {
                    case PopupAction.Open: view.OnPopupOpen(); break;
                    case PopupAction.Suspend: view.OnPopupSuspend(); break;
                    case PopupAction.Resume: view.OnPopupResume(); break;
                    case PopupAction.Close: view.OnPopupClose(); break;
                }
                return;
            }
        }
    }
}
