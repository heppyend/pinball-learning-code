using System.Collections.Generic;
using Pinball.Client;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pinball.Client.UI
{
    public enum ClientProfileRoute
    {
        Profile,
        Showcase,
        Nameplate,
        Title,
        Avatar,
        Badge,
        Frame,
    }

    /// <summary>
    /// Single owner of the seven profile-related pages.
    /// Owns the complete route state. The scene only supplies the authored Toggle
    /// groups; page visibility is never delegated to serialized SetActive callbacks.
    /// </summary>
    public sealed class ClientProfileNavigationController : MonoBehaviour, IClientNavigationObserver
    {
        // 场景中的真实节点名，已逐一核验：每个名字在 ClientShell 中恰好出现 1 次。
        //
        // 原值 "HomepageDisplayPage主页展示" 与 5 个 "BadgePage个性化-*" 在场景中出现 0 次，
        // 导致 AddPage 对 7 条路由中的 6 条直接 LogError，整个个人中心路由失效。
        //
        // 关键事实：这 7 个目标都不是 PagesLayer 下的独立页面，而是 ProfilePage个人中心
        // 内部的子面板，属于"页内子路由"，因此本控制器只应切换它们的激活状态。
        private static readonly string[] PageNames =
        {
            "Panel_Profile",      // 个人中心
            "Panel_Showcase",     // 主页展示
            "Content_Widget",     // 个性化 - 铭牌
            "Content_Title",      // 个性化 - 称号
            "Content_Avatar",     // 个性化 - 头像
            "Content_Info",       // 个性化 - 徽章
            "Content_Frame",      // 个性化 - 头像框
        };

        private static readonly string[] TopToggleNames =
        {
            "个人中心Toggle",
            "主页展示Toggle",
            "个性化Toggle",
        };

        private static readonly ClientProfileRoute[] PersonalizationRoutesByToggleIndex =
        {
            ClientProfileRoute.Nameplate,
            ClientProfileRoute.Title,
            ClientProfileRoute.Avatar,
            ClientProfileRoute.Badge,
            ClientProfileRoute.Frame,
        };

        [Tooltip("个人中心子页的搜索根。留空则用当前场景根；建议把 ProfilePage 节点拖进来以缩小搜索范围。")]
        [SerializeField] private Transform _pageSearchRoot;

        [Tooltip("3 个主面板（个人中心/主页展示/个性化）的互斥容器，即 PageContentRoot。留空则按名字自动查找。")]
        [SerializeField] private Transform _mainPanelRoot;

        [Tooltip("个性化面板，即 Panel_Personalize。其子节点 Top functional text / SubTabBar / SubContentRoot 必须常驻不变。留空则自动查找。")]
        [SerializeField] private Transform _personalizePanelRoot;

        [Tooltip("个性化 5 个子页的互斥容器，即 SubContentRoot。留空则自动查找。")]
        [SerializeField] private Transform _personalizeContentRoot;

        private readonly Dictionary<ClientProfileRoute, Transform> _pages = new Dictionary<ClientProfileRoute, Transform>();
        private readonly Dictionary<Toggle, UnityAction<bool>> _topBindings = new Dictionary<Toggle, UnityAction<bool>>();
        private readonly Dictionary<Toggle, UnityAction<bool>> _nestedBindings = new Dictionary<Toggle, UnityAction<bool>>();
        private readonly Dictionary<Toggle, ClientProfileRoute> _nestedRoutes = new Dictionary<Toggle, ClientProfileRoute>();
        private readonly Stack<ClientProfileRoute> _history = new Stack<ClientProfileRoute>();
        private readonly List<Transform> _pageLayerRoots = new List<Transform>();
        private readonly List<Transform> _allPageInstances = new List<Transform>();
        private ClientProfileRoute _currentRoute;
        private ClientProfileRoute _lastPersonalizationRoute = ClientProfileRoute.Nameplate;
        private bool _hasCurrentRoute;
        private bool _bound;
        private bool _isApplyingRoute;

        private void Awake()
        {
            // 必须放在 Awake，不能放在 Start。
            //
            // 本组件挂在 ProfilePage个人中心 上，而该节点现在是一个"启动即隐藏"的弹窗
            // （ClientUiPopup._hideOnAwake）。Unity 不会对未激活的 GameObject 调用 Start，
            // 所以放在 Start 会导致：第一次打开个人中心时 _pages 仍为空，
            // OnNavigated(Profile) → Navigate 直接报"页面未绑定"，
            // 面板保持场景初始状态（表现为"该显示个人中心却显示了主页展示"）。
            Bind();
        }

        private void Start()
        {
            Debug.Log("[ClientProfileNavigationController] Start：" + GetPath(transform) +
                      "，已绑定路由=" + _pages.Count);
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<Toggle, UnityAction<bool>> binding in _topBindings)
                if (binding.Key != null)
                    binding.Key.onValueChanged.RemoveListener(binding.Value);
            foreach (KeyValuePair<Toggle, UnityAction<bool>> binding in _nestedBindings)
                if (binding.Key != null)
                    binding.Key.onValueChanged.RemoveListener(binding.Value);
            _topBindings.Clear();
            _nestedBindings.Clear();
            _nestedRoutes.Clear();
        }

        private void Bind()
        {
            if (_bound)
                return;

            _pages.Clear();
            AddPage(ClientProfileRoute.Profile, PageNames[0]);
            AddPage(ClientProfileRoute.Showcase, PageNames[1]);
            AddPage(ClientProfileRoute.Nameplate, PageNames[2]);
            AddPage(ClientProfileRoute.Title, PageNames[3]);
            AddPage(ClientProfileRoute.Avatar, PageNames[4]);
            AddPage(ClientProfileRoute.Badge, PageNames[5]);
            AddPage(ClientProfileRoute.Frame, PageNames[6]);
            BindPageLayerRoots();
            BindAllPageInstances();

            BindTopToggles();
            BindNestedToggles();
            _bound = true;

            Debug.Log("[ClientProfileNavigationController] Bind：pages=" + _pages.Count +
                      "，topToggles=" + _topBindings.Count +
                      "，nestedToggles=" + _nestedBindings.Count +
                      "，pageLayerRoots=" + _pageLayerRoots.Count +
                      "，allPageInstances=" + _allPageInstances.Count +
                      "，nestedUnresolved=" + CountNestedToggles() + 
                      "，history=" + _history.Count);

            if (TryFindActiveRoute(out ClientProfileRoute activeRoute))
                Navigate(activeRoute, false);
            else
                Debug.Log("[ClientProfileNavigationController] Bind：启动时 7 个页面均未激活，等待主页入口或顶部Toggle。");
        }

        private void AddPage(ClientProfileRoute route, string pageName)
        {
            Transform page = FindPage(pageName);
            if (page != null)
                _pages[route] = page;
            else
                Debug.LogError("[ClientProfileNavigationController] 未找到页面：" + pageName);
        }

        private void BindPageLayerRoots()
        {
            _pageLayerRoots.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child != null)
                    _pageLayerRoots.Add(child);
            }
        }

        private void BindAllPageInstances()
        {
            _allPageInstances.Clear();

            // 原实现用 Resources.FindObjectsOfTypeAll 扫描"全部已加载对象"（含资源与预制体），
            // 既昂贵又依赖 "PagesLayer" 祖先名判断。改为在场景层级内按指定根搜索。
            Transform root = _pageSearchRoot != null ? _pageSearchRoot : transform.root;            if (root != null)
            {
                Transform[] candidates = root.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < candidates.Length; i++)
                {
                    Transform candidate = candidates[i];
                    if (candidate != null && IsPageName(candidate.name) && !_allPageInstances.Contains(candidate))
                        _allPageInstances.Add(candidate);
                }
            }

            // The normal scene path must always be included, even while Unity is
            // rebuilding inactive objects during an editor play-mode transition.
            foreach (Transform page in _pages.Values)
                if (page != null && !_allPageInstances.Contains(page))
                    _allPageInstances.Add(page);
        }

        private void BindTopToggles()
        {
            Toggle[] toggles = GetComponentsInChildren<Toggle>(true);
            for (int i = 0; i < toggles.Length; i++)
            {
                Toggle toggle = toggles[i];
                if (!IsTopToggle(toggle) || _topBindings.ContainsKey(toggle))
                    continue;

                Toggle capturedToggle = toggle;
                UnityAction<bool> listener = isOn => OnTopToggleChanged(capturedToggle, isOn);
                toggle.onValueChanged.AddListener(listener);
                _topBindings.Add(toggle, listener);
            }
        }

        private void BindNestedToggles()
        {
            // 原来只在 _pages.Values（即 5 个 Content_* 子页）内部寻找"恰好 5 元"的 ToggleGroup，
            // 但 5 个个性化 toggle 实际位于
            //   Panel_Personalize/SubTabBar/Image/Panel
            // 不在任何 Content_* 之下，所以一个都找不到（日志里 nestedToggles=0）。
            // 改为在整个个人中心范围内发现分组，分组语义与索引保持原样。
            HashSet<ToggleGroup> groups = new HashSet<ToggleGroup>();
            Toggle[] allToggles = SearchRoot.GetComponentsInChildren<Toggle>(true);
            for (int i = 0; i < allToggles.Length; i++)
            {
                ToggleGroup group = allToggles[i].group;
                if (group != null && group.GetComponentsInChildren<Toggle>(true).Length == 5)
                    groups.Add(group);
            }

            foreach (ToggleGroup group in groups)
            {
                Toggle[] toggles = group.GetComponentsInChildren<Toggle>(true);
                for (int i = 0; i < toggles.Length; i++)
                {
                    Toggle toggle = toggles[i];
                    if (_nestedBindings.ContainsKey(toggle) || !TryResolveNestedRoute(toggle, i, out ClientProfileRoute route))
                        continue;

                    _nestedRoutes[toggle] = route;
                    Toggle capturedToggle = toggle;
                    UnityAction<bool> listener = isOn => OnNestedToggleChanged(capturedToggle, isOn);
                    toggle.onValueChanged.AddListener(listener);
                    _nestedBindings.Add(toggle, listener);
                }
            }
        }

        private bool IsTopToggle(Toggle toggle)
        {
            if (toggle == null)
                return false;

            // 不能把"必须属于某个 3 元 ToggleGroup"当前置条件。
            //
            // 实测：场景里 ToggleGroup A 节点上根本没有 ToggleGroup 组件，
            // 3 个顶部 Toggle 的 m_Group 全为 0。原实现因此一个都绑不上，
            // 表现为"点击 Toggle 只改外观、没有任何逻辑响应"，且三者不互斥。
            // 改为以名字为主；若确实挂了 3 元 ToggleGroup 也一并接受。
            for (int i = 0; i < TopToggleNames.Length; i++)
                if (toggle.name == TopToggleNames[i])
                    return true;

            return toggle.group != null &&
                   toggle.group.GetComponentsInChildren<Toggle>(true).Length == 3;
        }

        /// <summary>
        /// 由导航内核在页面级路由变化后回调。本控制器只管理个人中心内部的子路由，
        /// 不再订阅静态事件，也不控制 PagesLayer 上的同级页面。
        /// </summary>
        public void OnNavigated(ClientUiPageId pageId)
        {
            if (pageId != ClientUiPageId.Profile)
                return;

            if (!_bound)
                Bind();
            ClientUiTrace.Line("路由", "页面路由进入 Profile → 重置到个人中心子路由（已绑定路由数=" + _pages.Count + "）");
            _history.Clear();
            Navigate(ClientProfileRoute.Profile, false);
        }

        private void OnTopToggleChanged(Toggle source, bool isOn)
        {
            if (!isOn || source == null || _isApplyingRoute)
                return;

            Debug.Log("[ClientProfileNavigationController] TopToggle：" + source.name + " isOn=" + isOn);

            if (source.name == TopToggleNames[0])
                Navigate(ClientProfileRoute.Profile, true);
            else if (source.name == TopToggleNames[1])
                Navigate(ClientProfileRoute.Showcase, true);
            else if (source.name == TopToggleNames[2])
                // 负责人明确：进入"个性化"默认显示 Content_Widget（铭牌），
                // 包括从其它页签切回来、以及退出重进，因此这里固定走 Nameplate，
                // 不使用 _lastPersonalizationRoute 的"记忆"语义。
                Navigate(ClientProfileRoute.Nameplate, true);
        }

        private void OnNestedToggleChanged(Toggle source, bool isOn)
        {
            if (_isApplyingRoute || !isOn || source == null || !_nestedRoutes.TryGetValue(source, out ClientProfileRoute route))
                return;
            Debug.Log("[ClientProfileNavigationController] NestedToggle：" + source.name + " -> " + route);
            Navigate(route, true);
        }

        public void Back()
        {
            if (!_bound)
                Bind();

            ClientProfileRoute previousRoute = _history.Count > 0
                ? _history.Pop()
                : ClientProfileRoute.Profile;
            Debug.Log("[ClientProfileNavigationController] Back：target=" + previousRoute + "，remainingHistory=" + _history.Count);
            Navigate(previousRoute, false);
        }

        private void Navigate(ClientProfileRoute route, bool recordHistory)
        {
            // 兜底：弹窗节点启动时未激活，Awake/Start 可能都还没跑过。
            if (!_bound)
                Bind();

            if (!_pages.ContainsKey(route))
            {
                ClientUiTrace.Warn("路由", "Navigate 失败，页面未绑定：" + route +
                                           "（已绑定=" + _pages.Count + "，搜索根=" +
                                           (_pageSearchRoot != null ? _pageSearchRoot.name : transform.name) + "）");
                return;
            }

            Debug.Log("[ClientProfileNavigationController] Navigate：from=" +
                      (_hasCurrentRoute ? _currentRoute.ToString() : "<none>") +
                      " to=" + route + " recordHistory=" + recordHistory);

            if (_hasCurrentRoute && _currentRoute == route)
            {
                ApplyRoute(route);
                return;
            }

            if (recordHistory && _hasCurrentRoute)
                _history.Push(_currentRoute);

            _currentRoute = route;
            _hasCurrentRoute = true;
            if (IsPersonalizationRoute(route))
                _lastPersonalizationRoute = route;

            ApplyRoute(route);
        }

        private void ApplyRoute(ClientProfileRoute route)
        {
            if (_isApplyingRoute)
            {
                Debug.Log("[ClientProfileNavigationController] ApplyRoute 忽略重入：" + route);
                return;
            }

            _isApplyingRoute = true;
            try
            {
                if (!_pages.TryGetValue(route, out Transform target) || target == null)
                {
                    Debug.LogError("[ClientProfileNavigationController] 路由未绑定，忽略：" + route);
                    return;
                }

                ResolveContainersIfNeeded();

                // 互斥只发生在两个明确的容器内，绝不沿祖先链泛化：
                //
                // 1) PageContentRoot 下 3 个主面板（个人中心/主页展示/个性化）互斥；
                // 2) SubContentRoot 下 5 个个性化子页（铭牌/称号/头像/徽章/头像框）互斥。
                //
                // 关键约束（负责人明确）：个性化面板里的 Top functional text 与 SubTabBar
                // 是常驻不变的，只有 SubContentRoot 的内容随子页切换。
                // 所以绝不能对 Panel_Personalize 的子节点做互斥——那会把这两个常驻元素关掉。
                bool personalize = IsPersonalizationRoute(route);

                SetExclusiveActive(_mainPanelRoot, personalize ? _personalizePanelRoot : target);

                if (personalize)
                    SetExclusiveActive(_personalizeContentRoot, target);

                SyncTopToggles(route);
                SyncNestedToggles(route);

                int activeCount = 0;
                string activeNames = string.Empty;
                foreach (KeyValuePair<ClientProfileRoute, Transform> page in _pages)
                {
                    if (!page.Value.gameObject.activeSelf)
                        continue;
                    activeCount++;
                    activeNames += (activeNames.Length == 0 ? string.Empty : ",") + page.Key;
                }

                if (activeCount != 1)
                    Debug.LogError("[ClientProfileNavigationController] 互斥失败：activeCount=" + activeCount + "，active=" + activeNames);
                else
                    Debug.Log("[ClientProfileNavigationController] ApplyRoute：active=" + activeNames);

                LogActivePageLayerRoots();
                LogActivePageInstances();
                LogActiveVisualCanvases();
            }
            finally
            {
                _isApplyingRoute = false;
            }
        }

        private void SyncTopToggles(ClientProfileRoute route)
        {
            string selectedName = route == ClientProfileRoute.Profile
                ? TopToggleNames[0]
                : route == ClientProfileRoute.Showcase ? TopToggleNames[1] : TopToggleNames[2];

            foreach (Toggle toggle in _topBindings.Keys)
                toggle.SetIsOnWithoutNotify(toggle.name == selectedName);
        }

        private void SyncNestedToggles(ClientProfileRoute route)
        {
            foreach (KeyValuePair<Toggle, ClientProfileRoute> toggle in _nestedRoutes)
                toggle.Key.SetIsOnWithoutNotify(toggle.Value == route);
        }

        private void LogActivePageLayerRoots()
        {
            string activeRoots = string.Empty;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (!child.gameObject.activeInHierarchy)
                    continue;

                activeRoots += (activeRoots.Length == 0 ? string.Empty : " | ") +
                               child.name + "[" + GetPath(child) + "]";
            }

            Debug.Log("[ClientProfileNavigationController] PagesLayerActiveRoots：" + activeRoots);
        }

        private void LogActivePageInstances()
        {
            int activeCount = 0;
            string activePaths = string.Empty;
            for (int i = 0; i < _allPageInstances.Count; i++)
            {
                Transform page = _allPageInstances[i];
                if (page == null || !page.gameObject.activeInHierarchy)
                    continue;

                activeCount++;
                activePaths += (activePaths.Length == 0 ? string.Empty : " | ") + GetPath(page);
            }

            Debug.Log("[ClientProfileNavigationController] AllPageInstancesActive：count=" +
                      activeCount + "，paths=" + activePaths);
        }

        private void LogActiveVisualCanvases()
        {
            Canvas[] canvases = transform.root.GetComponentsInChildren<Canvas>(true);
            int activeCount = 0;
            string activePaths = string.Empty;

            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null || !canvas.isActiveAndEnabled)
                    continue;

                activeCount++;
                activePaths += (activePaths.Length == 0 ? string.Empty : " | ") +
                               GetPath(canvas.transform) +
                               "(sort=" + canvas.sortingOrder + ")";
            }

            Debug.Log("[ClientProfileNavigationController] ActiveVisualCanvases：count=" +
                      activeCount + "，paths=" + activePaths);
        }

        private bool TryResolveNestedRoute(Toggle toggle, int toggleIndex, out ClientProfileRoute route)
        {
            if (toggleIndex >= 0 && toggleIndex < PersonalizationRoutesByToggleIndex.Length)
            {
                route = PersonalizationRoutesByToggleIndex[toggleIndex];
                return true;
            }

            route = ClientProfileRoute.Nameplate;
            return false;
        }

        private static bool IsPersonalizationRoute(ClientProfileRoute route)
        {
            return route >= ClientProfileRoute.Nameplate;
        }

        private bool TryFindActiveRoute(out ClientProfileRoute route)
        {
            foreach (KeyValuePair<ClientProfileRoute, Transform> page in _pages)
            {
                if (page.Value.gameObject.activeSelf)
                {
                    route = page.Key;
                    return true;
                }
            }

            route = ClientProfileRoute.Profile;
            return false;
        }

        private int CountNestedToggles()
        {
            int total = 0;
            foreach (Transform page in _pages.Values)
            {
                Toggle[] toggles = page.GetComponentsInChildren<Toggle>(true);
                for (int i = 0; i < toggles.Length; i++)
                {
                    ToggleGroup group = toggles[i].group;
                    if (group != null && group.GetComponentsInChildren<Toggle>(true).Length == 5 &&
                        !_nestedRoutes.ContainsKey(toggles[i]))
                        total++;
                }
            }
            return total;
        }

        private static string GetPath(Transform target)
        {
            string path = target.name;
            while (target.parent != null)
            {
                target = target.parent;
                path = target.name + "/" + path;
            }
            return path;
        }

        private Transform FindPage(string pageName)
        {
            Transform[] transforms = SearchRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i].name == pageName)
                    return transforms[i];
            return null;
        }

        /// <summary>
        /// 子路由查找的根。
        ///
        /// 关键：7 条路由的目标（Panel_Profile / Panel_Showcase / Content_*）都在
        /// ProfilePage个人中心 内部。按"PagesLayer 只留大厅、其余全是 PopupLayer 弹窗"的架构，
        /// ProfilePage个人中心 已经不在 PagesLayer 子树里，因此不能再以自身 transform 为搜索根——
        /// 否则会像修复前那样 7 条路由全部"未找到页面"。
        /// 优先使用 Inspector 指定的 _pageSearchRoot（拖入 ProfilePage个人中心 即可）。
        /// </summary>
        private Transform SearchRoot
        {
            get { return _pageSearchRoot != null ? _pageSearchRoot : transform; }
        }

        // ------------------------------------------------------------------
        // 互斥容器
        // ------------------------------------------------------------------

        /// <summary>
        /// 解析两个互斥容器。优先使用 Inspector 指定的引用（推荐直接拖节点），未指定时按名字自动查找，
        /// 并容忍节点名首尾空格——场景中的个性化面板实际名为 "Panel_Personalize  "（带尾随空格）。
        /// </summary>
        private void ResolveContainersIfNeeded()
        {
            if (_mainPanelRoot == null)
                _mainPanelRoot = FindPageTrimmed("PageContentRoot");
            if (_personalizePanelRoot == null)
                _personalizePanelRoot = FindPageTrimmed("Panel_Personalize");
            if (_personalizeContentRoot == null)
                _personalizeContentRoot = FindPageTrimmed("SubContentRoot");

            if (_mainPanelRoot == null || _personalizeContentRoot == null)
                Debug.LogError("[ClientProfileNavigationController] 互斥容器未解析：" +
                               "PageContentRoot=" + (_mainPanelRoot != null) +
                               "，SubContentRoot=" + (_personalizeContentRoot != null) +
                               "。请在 Inspector 指定，或检查节点名。");
        }

        /// <summary>
        /// 在 parent 的**直接子节点**中做互斥：只让 activeChild 激活，其余同级全部关闭。
        ///
        /// 刻意不递归修改 activeChild 自身的子树，这样个性化面板内的常驻元素
        /// （Top functional text、SubTabBar）不会被误关。
        /// </summary>
        private static void SetExclusiveActive(Transform parent, Transform activeChild)
        {
            if (parent == null || activeChild == null)
            {
                ClientUiTrace.Warn("路由", "互斥被跳过：parent=" +
                                           (parent != null ? parent.name : "null") +
                                           "，activeChild=" + (activeChild != null ? activeChild.name : "null"));
                return;
            }
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                bool shouldBeActive = child == activeChild;
                if (child.gameObject.activeSelf == shouldBeActive)
                    continue;
                ClientUiTrace.Line("路由", "互斥 " + parent.name + " → " +
                                           (shouldBeActive ? "显示 " : "隐藏 ") + child.name);
                child.gameObject.SetActive(shouldBeActive);
            }
        }

        /// <summary>按名字查找，忽略首尾空格。</summary>
        private Transform FindPageTrimmed(string pageName)
        {
            Transform[] transforms = SearchRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i].name.Trim() == pageName)
                    return transforms[i];
            return null;
        }

        private static bool IsPageName(string candidate)
        {
            for (int i = 0; i < PageNames.Length; i++)
                if (PageNames[i] == candidate)
                    return true;
            return false;
        }

        private static Transform FindAncestor(Transform target, string ancestorName)
        {
            Transform current = target.parent;
            while (current != null)
            {
                if (current.name == ancestorName)
                    return current;
                current = current.parent;
            }
            return null;
        }
    }
}
