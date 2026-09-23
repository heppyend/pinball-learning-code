using System;
using System.Collections.Generic;
using TMPro;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>
    /// 个人中心的数据绑定与局部交互。视觉节点由 ClientShell 场景维护。
    ///
    /// <para><b>2026-09-20 收口（阶段 A）</b>：</para>
    /// <list type="bullet">
    /// <item><b>BUG-018</b>：玩家名 / 玩家ID / 战力 三个文本在场景里是 **`TMP_Text`**（实测 guid `f4688fdb…`），
    /// 而本类原先声明成旧版 `UnityEngine.UI.Text` —— 违反 `AGENTS.md` §4（文本引用必须按场景真实组件类型声明），
    /// 这也是它一直没能接线的原因之一。已改为 `TMP_Text`。</item>
    /// <item><b>收藏品列表</b>：头像 / 徽章 / 铭牌 / 头像框 / 称号 五个列表改走「1 模板 + N 实例」
    /// （<see cref="ClientCollectionList"/>），数据分两层：<b>有哪些</b>来自配置表
    /// （<see cref="IClientConfigService.GetCollections"/>，已按 `ClientShow` 过滤、按 `Sorting` 排序），
    /// <b>我拥有哪些 / 使用中</b>来自玩家状态（<see cref="IClientDataService.GetOwnedCollectionIds"/> 等）。</item>
    /// <item><b>主页展示</b>：容器在原场景里是**空的**，模板卡由结构修复工具新建（`展示卡Item`），
    /// 走 <see cref="ClientHeroCardList"/>，与英雄页 / 编队选卡共用同一套卡牌逻辑。</item>
    /// </list>
    ///
    /// <para>⚠️ <b>图标暂不解析</b>：收藏品的图标来源是 `ResId`（整型资源 id）或 `Avatar`（名字，仅头像），
    /// 而"资源 id → Sprite"的映射规则**尚未定义**（与 `BUG-019` 同类阻断），且工程内 `HeroAvatar_*` 贴图为 0 个。
    /// 因此这里**传 null 保持模板图**，等负责人给出映射规则或补齐贴图后再接。</para>
    /// </summary>
    public sealed class ClientProfilePage : ClientPageViewBase, IClientNavigationHost, IClientFeedbackHost
    {
        [SerializeField] private GameObject _pageRoot;

        // ---- BUG-018：场景里是 TMP 文本（不是旧版 Text） ----
        [SerializeField] private TMP_Text _playerName;
        [SerializeField] private TMP_Text _playerId;
        [SerializeField] private TMP_Text _combatPower;

        [SerializeField] private Text _notice;
        [SerializeField] private GameObject _renameDialog;
        [SerializeField] private InputField _renameInput;

        [Header("收藏品列表（模板 + 容器由结构修复工具接线）")]
        [SerializeField] private GameObject _avatarTemplate;
        [SerializeField] private RectTransform _avatarRoot;
        [SerializeField] private GameObject _badgeTemplate;
        [SerializeField] private RectTransform _badgeRoot;
        [SerializeField] private GameObject _nameplateTemplate;
        [SerializeField] private RectTransform _nameplateRoot;
        [SerializeField] private GameObject _frameTemplate;
        [SerializeField] private RectTransform _frameRoot;
        [SerializeField] private GameObject _titleTemplate;
        [SerializeField] private RectTransform _titleRoot;

        [Header("主页展示列表")]
        [SerializeField] private GameObject _showcaseTemplate;
        [SerializeField] private RectTransform _showcaseRoot;

        private bool _canBindData;
        private IClientNavigation _navigation;
        private IClientFeedback _feedback;

        /// <summary>个性化：每类当前"已选中但还没替换"的条目 id（2026-09-21 负责人确认的交互）。</summary>
        private readonly Dictionary<ClientCollectionKind, int> _pendingSelections = new Dictionary<ClientCollectionKind, int>();

        private ClientCollectionList _avatarList;
        private ClientCollectionList _badgeList;
        private ClientCollectionList _nameplateList;
        private ClientCollectionList _frameList;
        private ClientCollectionList _titleList;
        private ClientHeroCardList _showcaseList;

        /// <summary>主页展示只有一位英雄（`GetShowcaseHeroId`），复用同一个缓冲避免每次分配。</summary>
        private readonly List<ClientHero> _showcaseBuffer = new List<ClientHero>(1);

        /// <summary>由导航内核注入，取代原先的 HomeShowcaseRequested / BadgeRequested 静态事件。</summary>
        public void BindNavigation(IClientNavigation navigation) { _navigation = navigation; }

        /// <summary>
        /// 由组合根注入全局轻提示（`ClientShellController` 对实现 `IClientFeedbackHost` 的视图统一注入）。
        ///
        /// <para>2026-09-21（BUG-026 ③）：本页原先只往 `_notice` 这个**未接线**的文本写提示 ⇒ 选中收藏品后的
        /// "尚未拥有 / 已使用 / 切换失败"、改名失败等反馈**用户完全看不到**。现与 `ClientHomePage` 一致：
        /// **优先走全局 Toast**，`_notice` 仅作为仍接线时的兼容回退。</para>
        /// </summary>
        public void BindFeedback(IClientFeedback feedback) { _feedback = feedback; }

        protected override GameObject PageRoot { get { return _pageRoot; } }
        protected override void OnPageRefresh() { Show(); }
        protected override void OnPageHidden() { Hide(); }

        private void OnEnable()
        {
            if (_canBindData)
                BindData();
        }

        private void Start()
        {
            // Start 在场景内全部 Awake 完成后调用，避免依赖根节点的启用顺序。
            _canBindData = true;
            BindData();
            BindPageActions();
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

        /// <summary>
        /// 代码侧装配入口（当前无调用方，保留以便将来由工具或测试直接装配）。
        /// ⚠️ 三个文本形参是 **`TMP_Text`** —— 场景里就是 TMP（实测 guid `f4688fdb…`），
        /// 按 `AGENTS.md` §4 不得用旧版 `UnityEngine.UI.Text` 互换。
        /// </summary>
        public void Configure(
            GameObject pageRoot,
            TMP_Text playerName,
            TMP_Text playerId,
            TMP_Text combatPower,
            Text notice,
            GameObject renameDialog,
            InputField renameInput)
        {
            _pageRoot = pageRoot;
            _playerName = playerName;
            _playerId = playerId;
            _combatPower = combatPower;
            _notice = notice;
            _renameDialog = renameDialog;
            _renameInput = renameInput;
        }

        public void Show()
        {
            if (_pageRoot != null)
                _pageRoot.SetActive(true);

            // 2026-09-21：与排行榜/编队同一问题（内容不撑高 + Elastic + 居中）⇒ 收藏品列表同样修正为竖向列表标准配置。
            ClientScrollFix.FixAll(gameObject, false);
        }

        public void Hide()
        {
            if (_renameDialog != null)
                _renameDialog.SetActive(false);

            if (_pageRoot != null)
                _pageRoot.SetActive(false);
        }

        public void OpenRenameDialog()
        {
            if (_renameDialog == null || _renameInput == null)
                return;

            _renameInput.text = ClientServices.Data.GetProfile().DisplayName;
            _renameDialog.SetActive(true);
        }

        public void OpenHomeShowcase() { OpenPage(ClientUiPageId.HomeShowcase); }

        public void OpenBadge() { OpenPage(ClientUiPageId.Badge); }

        private void OpenPage(ClientUiPageId pageId)
        {
            if (_navigation != null)
                _navigation.Open(pageId);
            else
                Debug.LogWarning("[Client] 个人中心导航尚未就绪，未打开页面：" + pageId);
        }

        // ------------------------------------------------------------------
        // 按钮绑定 / 改名称
        //
        // ⚠️ 2026-09-20 事故与修复：上一版重写本文件时，我**没读全原文就整文件覆盖**，
        //    丢掉了下面这几个方法（`BindPageActions` / `BindButton` / `CancelRename` / `ConfirmRename`）。
        //    已按 git 中的原始语义补回。教训：**覆盖整文件前必须完整读取原文件**。
        //
        // 事实：场景里**唯一的 UnityEvent 是 3 个 `SetActive`** —— 没有任何按钮通过场景事件调用本类的方法，
        //      交互全靠这里按名字绑定。所以这些方法一旦丢失，表现是"按钮完全没反应"且**不报错**。
        // ------------------------------------------------------------------

        private void BindPageActions()
        {
            // 说明：`HomeDisplayButton` / `PersonalizeButton` 在当前场景里**并不存在**（已核实），
            // 这两行绑定是历史兼容、实际不会命中，保留以免将来节点补回后失效。
            BindButton("HomeDisplayButton", OpenHomeShowcase);
            BindButton("PersonalizeButton", OpenBadge);

            // 2026-09-21（死按钮审计发现）：改名链路的三个按钮**此前完全没有绑定** ——
            // `改名称` 点不开输入弹窗、`ConfirmButton` / `CancelButton` 点了没有任何反应。
            // 处理函数（`OpenRenameDialog` / `ConfirmRename` / `CancelRename`）一直都在，只是没人接上。
            BindButton("改名称", OpenRenameDialog);
            BindButton("ConfirmButton", ConfirmRename);
            BindButton("CancelButton", CancelRename);

            // 2026-09-21（负责人确认的交互）：5 个子界面的「确定键」= **确认替换**（不是返回）。
            BindPersonalizeConfirmButtons();
        }

        /// <summary>
        /// 按名绑定按钮。
        ///
        /// <para>2026-09-21：改为**深搜** —— 页面内的按钮并不都在根节点的直接子级
        /// （`改名称` 在 `Panel_Profile/底框/` 下、改名弹窗的确认/取消在 `Panel_Profile/RenameDialog/` 下），
        /// 原先的 `transform.Find` 只查直接子级 ⇒ 一个都找不到，且**静默失败不报错**。</para>
        /// </summary>
        private void BindButton(string name, UnityEngine.Events.UnityAction action)
        {
            Transform target = FindDeep(transform, name);
            if (target == null || target.GetComponent<Button>() == null)
                return;

            Button button = target.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(action);
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
                return null;
            if (root.name == name)
                return root;

            for (int index = 0; index < root.childCount; index++)
            {
                Transform found = FindDeep(root.GetChild(index), name);
                if (found != null)
                    return found;
            }
            return null;
        }

        public void CancelRename()
        {
            if (_renameDialog != null)
                _renameDialog.SetActive(false);
        }

        public void ConfirmRename()
        {
            if (_renameInput == null)
                return;

            if (!ClientServices.Data.TryRename(_renameInput.text))
            {
                ShowNotice("昵称不能为空");
                return;
            }

            CancelRename();
            ShowNotice("昵称已更新（本地模拟）");
        }

        public void ShowNotice(string message)
        {
            // 优先走全局 Toast（场景里的 Toast 已接线 ✓）；`_notice` 未接线时不再"静默吞掉"提示。
            if (_feedback != null)
                _feedback.ShowToast(message);

            if (_notice != null)
            {
                _notice.text = message;
                _notice.gameObject.SetActive(true);
            }

            Debug.Log("[Client] " + message);
        }

        // ------------------------------------------------------------------
        // 数据绑定
        // ------------------------------------------------------------------

        private void Refresh()
        {
            RefreshProfileTexts();
            RefreshCollections();
            RefreshShowcase();
        }

        /// <summary>BUG-018：玩家名 / 玩家ID / 战力。</summary>
        private void RefreshProfileTexts()
        {
            if (_playerName == null && _playerId == null && _combatPower == null)
                return;

            ClientPlayerProfile profile = ClientServices.Data.GetProfile();
            if (profile == null)
                return;

            if (_playerName != null)
                _playerName.text = profile.DisplayName;
            if (_playerId != null)
                _playerId.text = "玩家ID: " + profile.PlayerId;
            if (_combatPower != null)
                _combatPower.text = profile.CombatPower.ToString();
        }

        private void RefreshCollections()
        {
            if (!ClientServices.IsInitialized || !ClientServices.HasConfig)
                return;

            EnsureLists();

            BindCollection(ClientCollectionKind.Head, _avatarList);
            BindCollection(ClientCollectionKind.Badge, _badgeList);
            BindCollection(ClientCollectionKind.Nameplate, _nameplateList);
            BindCollection(ClientCollectionKind.HeadFrame, _frameList);
            BindCollection(ClientCollectionKind.Title, _titleList);
        }

        /// <summary>
        /// 主页展示 = 玩家当前展示的那一位英雄（<see cref="IClientDataService.GetShowcaseHeroId"/>）。
        /// 点击卡牌进入「主页展示」页更换。
        /// </summary>
        private void RefreshShowcase()
        {
            if (!ClientServices.IsInitialized || !ClientServices.HasConfig)
                return;

            EnsureLists();
            if (_showcaseList == null || !_showcaseList.IsValid)
                return;

            _showcaseBuffer.Clear();

            int heroId = ClientServices.Data.GetShowcaseHeroId();
            ClientHeroConfig config;
            if (heroId > 0 && ClientServices.Config.TryGetHero(heroId, out config) && config != null)
            {
                // 展示位只需要"名字 + 等级 + 拥有状态"，等级/拥有仍取玩家状态层。
                _showcaseBuffer.Add(new ClientHero
                {
                    HeroId = config.HeroId,
                    Name = config.Name,
                    Level = 1,
                    StarLevel = config.Star,
                    IsOwned = true,
                });
            }

            List<ClientHero> list = _showcaseBuffer;
            _showcaseList.Show(list, hero => hero.Name, hero => hero.Level, hero => hero.IsOwned, hero => null,
                               index => OpenHomeShowcase());
        }

        private void EnsureLists()
        {
            if (_avatarList == null)
                _avatarList = new ClientCollectionList(_avatarRoot, _avatarTemplate);
            if (_badgeList == null)
                _badgeList = new ClientCollectionList(_badgeRoot, _badgeTemplate);
            if (_nameplateList == null)
                _nameplateList = new ClientCollectionList(_nameplateRoot, _nameplateTemplate);
            if (_frameList == null)
                _frameList = new ClientCollectionList(_frameRoot, _frameTemplate);
            if (_titleList == null)
                _titleList = new ClientCollectionList(_titleRoot, _titleTemplate);
            if (_showcaseList == null)
                _showcaseList = new ClientHeroCardList(_showcaseRoot, _showcaseTemplate);
        }

        /// <summary>
        /// 绑一类收藏品：**列表内容来自配置表**（已过滤/排序），**拥有状态与使用中来自玩家状态**。
        /// 图标暂传 null（映射规则未定义，见类注释），保持模板图。
        /// </summary>
        private void BindCollection(ClientCollectionKind kind, ClientCollectionList list)
        {
            if (list == null || !list.IsValid)
                return;

            IReadOnlyList<ClientCollectionConfig> entries = ClientServices.Config.GetCollections(kind);
            IReadOnlyList<int> owned = ClientServices.Data.GetOwnedCollectionIds(kind);
            int equipped = ClientServices.Data.GetEquippedCollectionId(kind);

            list.Show(entries,
                      entry => entry.Name,
                      entry => IsOwned(owned, entry.Id),
                      entry => entry.Id == equipped || entry.Id == PendingOf(kind),
                      entry => null,
                      index => SelectFromList(kind, entries, index));
        }

        /// <summary>该类当前"已选中但还没替换"的条目 id（0 = 未选）。</summary>
        private int PendingOf(ClientCollectionKind kind)
        {
            int pending;
            return _pendingSelections.TryGetValue(kind, out pending) ? pending : 0;
        }

        /// <summary>
        /// **2026-09-21 负责人确认的交互**：点条目 = **只选中**（高亮，不立即替换），
        /// 再点该子界面的**「确定键」**才真正替换；「确定键」**不是返回**。
        /// </summary>
        private static string PersonalizeContentNode(ClientCollectionKind kind)
        {
            switch (kind)
            {
                case ClientCollectionKind.Head: return "Content_Avatar";      // 头像
                case ClientCollectionKind.HeadFrame: return "Content_Frame";  // 头像框
                case ClientCollectionKind.Badge: return "Content_Info";       // 徽章
                case ClientCollectionKind.Title: return "Content_Title";      // 称号
                case ClientCollectionKind.Nameplate: return "Content_Widget"; // 铭牌
                default: return null;
            }
        }

        private static string PersonalizeKindLabel(ClientCollectionKind kind)
        {
            switch (kind)
            {
                case ClientCollectionKind.Head: return "头像";
                case ClientCollectionKind.HeadFrame: return "头像框";
                case ClientCollectionKind.Badge: return "徽章";
                case ClientCollectionKind.Title: return "称号";
                case ClientCollectionKind.Nameplate: return "铭牌";
                default: return "条目";
            }
        }

        /// <summary>把 5 个子界面的「个性化确定键Button」绑到各自的"确认替换"。</summary>
        private void BindPersonalizeConfirmButtons()
        {
            ClientCollectionKind[] kinds =
            {
                ClientCollectionKind.Head, ClientCollectionKind.HeadFrame, ClientCollectionKind.Badge,
                ClientCollectionKind.Title, ClientCollectionKind.Nameplate,
            };

            for (int index = 0; index < kinds.Length; index++)
            {
                ClientCollectionKind kind = kinds[index];
                string contentName = PersonalizeContentNode(kind);
                if (string.IsNullOrEmpty(contentName))
                    continue;

                Transform content = FindDeep(transform, contentName);
                if (content == null)
                {
                    Debug.LogWarning("[Client] 个性化：找不到子界面「" + contentName + "」，确定键未绑定。");
                    continue;
                }

                Transform node = FindDeep(content, "个性化确定键Button");
                Button button = node != null ? node.GetComponent<Button>() : null;
                if (button == null)
                {
                    Debug.LogWarning("[Client] 个性化：「" + contentName + "」下没有「个性化确定键Button」。");
                    continue;
                }

                ClientCollectionKind captured = kind;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ConfirmPersonalize(captured));
                Debug.Log("[Client] 个性化：已把「" + contentName + "/个性化确定键Button」绑定为确认替换「" + PersonalizeKindLabel(kind) + "」。");
            }
        }

        /// <summary>点「确定键」：把该子界面**已选中**的条目替换上去（未选中 / 未拥有时给提示）。</summary>
        private void ConfirmPersonalize(ClientCollectionKind kind)
        {
            int pending = PendingOf(kind);
            if (pending <= 0)
            {
                ShowNotice("请先选择一个" + PersonalizeKindLabel(kind) + "，再点确定键");
                return;
            }

            ClientCollectionConfig entry = null;
            IReadOnlyList<ClientCollectionConfig> entries = ClientServices.Config.GetCollections(kind);
            for (int index = 0; index < entries.Count; index++)
                if (entries[index] != null && entries[index].Id == pending) { entry = entries[index]; break; }

            if (!ClientServices.Data.TryEquipCollection(kind, pending))
            {
                ShowNotice("替换失败：" + (entry != null ? entry.Name : pending.ToString()));
                return;
            }

            ShowNotice("已替换：" + (entry != null ? entry.Name : pending.ToString()));
            _pendingSelections.Remove(kind);
            RefreshCollections();
        }

        private static bool IsOwned(IReadOnlyList<int> owned, int id)
        {
            if (owned == null)
                return false;
            for (int i = 0; i < owned.Count; i++)
            {
                if (owned[i] == id)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// **点条目 = 只选中**（2026-09-21 负责人确认的交互）：已拥有 ⇒ 记为待替换并高亮；
        /// 未拥有 ⇒ 提示且不选中。真正替换由该子界面的「确定键」完成（见 <see cref="ConfirmPersonalize"/>）。
        /// </summary>
        private void SelectFromList(ClientCollectionKind kind, IReadOnlyList<ClientCollectionConfig> entries, int index)
        {
            if (entries == null || index < 0 || index >= entries.Count)
                return;

            ClientCollectionConfig entry = entries[index];
            if (entry == null)
                return;

            if (!IsOwned(ClientServices.Data.GetOwnedCollectionIds(kind), entry.Id))
            {
                ShowNotice("尚未拥有：" + entry.Name);
                return;
            }

            _pendingSelections[kind] = entry.Id;
            ShowNotice("已选择：" + entry.Name + "（点确定键替换）");
            RefreshCollections();
        }

        /// <summary>旧的"点击即装备"入口，保留为兼容转发（当前没有调用方）。</summary>
        private void EquipFromList(ClientCollectionKind kind, IReadOnlyList<ClientCollectionConfig> entries, int index)
        {
            SelectFromList(kind, entries, index);
        }
    }
}
