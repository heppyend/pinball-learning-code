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
    public sealed class ClientProfileNavigationController : MonoBehaviour
    {
        private static readonly string[] PageNames =
        {
            "ProfilePage个人中心",
            "HomepageDisplayPage主页展示",
            "BadgePage个性化-铭牌",
            "BadgePage个性化-称号",
            "BadgePage个性化-头像",
            "BadgePage个性化-徽章",
            "BadgePage个性化-头像框",
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

        private void OnEnable()
        {
            ClientHomePage.NavigationRequested += OnHomeNavigationRequested;
        }

        private void Start()
        {
            Debug.Log("[ClientProfileNavigationController] Start：" + GetPath(transform));
            Bind();
        }

        private void OnDestroy()
        {
            ClientHomePage.NavigationRequested -= OnHomeNavigationRequested;

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

            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null || !candidate.gameObject.scene.IsValid() ||
                    !IsPageName(candidate.name) || FindAncestor(candidate, "PagesLayer") == null)
                    continue;

                if (!_allPageInstances.Contains(candidate))
                    _allPageInstances.Add(candidate);
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
            HashSet<ToggleGroup> groups = new HashSet<ToggleGroup>();
            foreach (Transform page in _pages.Values)
            {
                Toggle[] toggles = page.GetComponentsInChildren<Toggle>(true);
                for (int i = 0; i < toggles.Length; i++)
                {
                    ToggleGroup group = toggles[i].group;
                    if (group != null && group.GetComponentsInChildren<Toggle>(true).Length == 5)
                        groups.Add(group);
                }
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
            if (toggle == null || toggle.group == null || toggle.group.GetComponentsInChildren<Toggle>(true).Length != 3)
                return false;

            for (int i = 0; i < TopToggleNames.Length; i++)
                if (toggle.name == TopToggleNames[i])
                    return true;
            return false;
        }

        private void OnHomeNavigationRequested(ClientHomeDestination destination)
        {
            if (destination != ClientHomeDestination.Profile)
                return;

            Debug.Log("[ClientProfileNavigationController] HomeNavigation：ProfileButton -> Profile");
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
                Navigate(_lastPersonalizationRoute, true);
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
            if (!_pages.ContainsKey(route))
            {
                Debug.LogError("[ClientProfileNavigationController] Navigate 失败，页面未绑定：" + route);
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
                // PagesLayer is the single visibility boundary. Close every
                // authored first-level page before opening the requested route.
                // This also removes unrelated legacy pages or wrapper canvases
                // that are not part of the seven-route dictionary.
                // Deactivate every matching page instance in the current scene,
                // not only the seven references found by the first lookup. This
                // also closes stale/duplicated page roots left by older UI setup.
                for (int i = 0; i < _allPageInstances.Count; i++)
                    if (_allPageInstances[i] != null)
                        _allPageInstances[i].gameObject.SetActive(false);

                // Keep the existing PagesLayer boundary as a second guard for
                // unrelated legacy pages and wrapper objects.
                for (int i = 0; i < _pageLayerRoots.Count; i++)
                    if (_pageLayerRoots[i] != null)
                        _pageLayerRoots[i].gameObject.SetActive(false);

                _pages[route].gameObject.SetActive(true);

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
            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i].name == pageName)
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
