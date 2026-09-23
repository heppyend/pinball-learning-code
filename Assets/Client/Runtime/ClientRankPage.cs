using System;
using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>仅绑定既有 RankPage 节点；不创建、移动或调整 UI。</summary>
    public sealed class ClientRankPage : ClientPageViewBase, IClientNavigationHost, IClientPopupHost
    {
        [Serializable] private struct VisualBinding { public string Id; public Sprite Sprite; }
        [SerializeField] private VisualBinding[] _avatars = new VisualBinding[0];
        [SerializeField] private VisualBinding[] _frames = new VisualBinding[0];
        [SerializeField] private VisualBinding[] _badges = new VisualBinding[0];
        [SerializeField] private VisualBinding[] _illustrations = new VisualBinding[0];
        [SerializeField] private VisualBinding[] _attributes = new VisualBinding[0];
        [SerializeField] private VisualBinding[] _qualities = new VisualBinding[0];

        private GameObject _battleRoot;
        private GameObject _challengeRoot;
        private GameObject _lineupRoot;
        private Image _characterIllustration;
        private TMP_Text _tabTitle;
        private readonly List<RankRow> _battleRows = new List<RankRow>();
        private readonly List<RankRow> _challengeRows = new List<RankRow>();

        // 2026-09-21（负责人反馈）：排行榜要求显示**前 50 名**，而场景里每个榜只有 5 行。
        // 这里记录"行容器 + 首行模板"，运行时按数据条数克隆补足（不写场景、不动排版）。
        private Transform _battleContent;
        private Transform _challengeContent;
        private GameObject _battleRowTemplate;
        private GameObject _challengeRowTemplate;

        /// <summary>排行榜最多显示多少名（负责人要求：前 50 名）。</summary>
        private const int MaxLeaderboardRows = 50;
        private readonly List<LineupCardRow> _lineupCards = new List<LineupCardRow>();
        private ClientLeaderboardKind _activeKind = ClientLeaderboardKind.BattlePower;
        private bool _lineupOpen;
        private bool _subscribed;
        private IClientNavigation _navigation;
        private IClientPopupService _popups;

        // Player lineup 已按确认架构迁入 PopupLayer，不再是排行榜的内部子节点，
        // 因此原来的 FindDeep(transform, "Player lineup") 会返回 null。
        [SerializeField] private GameObject _playerLineupPopup;

        public void BindPopups(IClientPopupService popups) { _popups = popups; }

        /// <summary>阵容弹窗的显隐统一交给弹窗服务（栈顺序 / 遮罩 / 输入阻断）。</summary>
        private void SetLineupVisible(bool visible)
        {
            if (_lineupRoot == null)
                return;
            if (_popups != null)
            {
                ClientUiPopup popup = _lineupRoot.GetComponent<ClientUiPopup>();
                if (popup != null)
                {
                    if (visible) _popups.Open(popup);
                    else _popups.Close(popup);
                    return;
                }
            }
            _lineupRoot.SetActive(visible);
        }

        protected override void OnPageRefresh() { Show(); }

        /// <summary>由导航内核注入；取代原先的 FindObjectOfType&lt;ClientUiNavigator&gt;。</summary>
        public void BindNavigation(IClientNavigation navigation) { _navigation = navigation; }

        private void Awake()
        {
            _battleRoot = FindDeep(transform, "Battle Power Ranking")?.gameObject;
            _challengeRoot = FindDeep(transform, "Challenge List")?.gameObject;
            _lineupRoot = _playerLineupPopup != null ? _playerLineupPopup : FindDeep(transform, "Player lineup")?.gameObject;
            Transform illustration = FindDeep(transform, "Character illustration");
            _characterIllustration = illustration != null ? illustration.GetComponent<Image>() : null;
            Transform title = FindDeep(transform, "Challange/Ballte text");
            _tabTitle = title != null ? title.GetComponent<TMP_Text>() : null;
            CollectRankRows(_battleRoot, _battleRows, "玩家战力排行prefabs", out _battleContent, out _battleRowTemplate);
            CollectRankRows(_challengeRoot, _challengeRows, "玩家排行perfabs", out _challengeContent, out _challengeRowTemplate);
            CollectLineupCards();
            BindButton("Battle powerButton", SelectBattlePowerLeaderboard);
            BindButton("challenge Button", SelectChallengeLeaderboard);
            BindButton("BackoffButton", Back);
        }

        private void OnEnable()
        {
            SubscribeDataChanges();
        }

        private void OnDisable()
        {
            if (_subscribed && ClientServices.IsInitialized) ClientServices.Data.DataChanged -= Refresh;
            _subscribed = false;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            SubscribeDataChanges();
            CloseLineup();
            SelectLeaderboard(ClientLeaderboardKind.BattlePower);

            // 2026-09-21（负责人反馈）：排行榜原先"只显示 6 条、下滑松手被弹回、默认不在最上面" ——
            // 场景实测根因是 ContentSizeFitter 未撑高 + ScrollRect 为 Elastic + 网格居中。
            // 这里在运行时统一修正为竖向列表标准配置，并回到最上面。
            ClientScrollFix.FixAll(gameObject, true);
        }

        public void Refresh() { RefreshLeaderboard(_activeKind); }
        public void SelectBattlePowerLeaderboard() { SelectLeaderboard(ClientLeaderboardKind.BattlePower); }
        public void SelectChallengeLeaderboard() { SelectLeaderboard(ClientLeaderboardKind.Challenge); }

        /// <summary>未来资源适配器可通过 ID 替换角色立绘；空/未映射 ID 保留现有素材。</summary>
        public void SetCharacterIllustration(string illustrationId) { SetSprite(_characterIllustration, illustrationId, _illustrations); }
        /// <summary>未来本地化或运营配置可替换 Challenge/Battle 标题。</summary>
        public void SetLeaderboardTitle(string title) { if (_tabTitle != null) _tabTitle.text = title ?? string.Empty; }

        public void CloseLineup()
        {
            _lineupOpen = false;
            if (_lineupRoot != null) SetLineupVisible(false);
        }

        private void SubscribeDataChanges()
        {
            if (!_subscribed && ClientServices.IsInitialized)
            {
                ClientServices.Data.DataChanged += Refresh;
                _subscribed = true;
            }
        }

        private void SelectLeaderboard(ClientLeaderboardKind kind)
        {
            if (_lineupOpen) return;
            _activeKind = kind;
            if (_battleRoot != null) _battleRoot.SetActive(kind == ClientLeaderboardKind.BattlePower);
            if (_challengeRoot != null) _challengeRoot.SetActive(kind == ClientLeaderboardKind.Challenge);
            SetLeaderboardTitle(kind == ClientLeaderboardKind.BattlePower ? "Battle Power Ranking" : "Challenge List");
            RefreshLeaderboard(kind);
        }

        private void RefreshLeaderboard(ClientLeaderboardKind kind)
        {
            if (!ClientServices.IsInitialized) return;
            IReadOnlyList<ClientLeaderboardEntry> entries = ClientServices.Data.GetLeaderboardEntries(kind);
            List<RankRow> rows = kind == ClientLeaderboardKind.BattlePower ? _battleRows : _challengeRows;

            // 2026-09-21：先按数据条数补足行（前 50 名），再逐行刷新。
            EnsureRankRows(rows,
                kind == ClientLeaderboardKind.BattlePower ? _battleContent : _challengeContent,
                kind == ClientLeaderboardKind.BattlePower ? _battleRowTemplate : _challengeRowTemplate,
                entries != null ? Mathf.Min(entries.Count, MaxLeaderboardRows) : 0);

            for (int index = 0; index < rows.Count; index++)
            {
                ClientLeaderboardEntry entry = entries != null && index < entries.Count && index < MaxLeaderboardRows
                    ? entries[index] : null;
                rows[index].Refresh(entry, kind == ClientLeaderboardKind.Challenge, _avatars, _frames, _badges, OpenLineup);
            }
        }

        private void OpenLineup(string playerId)
        {
            if (_lineupOpen || !ClientServices.IsInitialized || _lineupRoot == null) return;
            ClientPlayerLineup lineup = ClientServices.Data.GetPlayerLineup(playerId);
            if (lineup == null) return;
            _lineupOpen = true;
            SetLineupVisible(true);
            SetText(_lineupRoot.transform, "层数", lineup.Floor.ToString());
            SetText(_lineupRoot.transform, "回合数", lineup.TurnCount.ToString());
            SetText(_lineupRoot.transform, "战力", lineup.CombatPower.ToString());

            // 2026-09-21（BUG-026 ⑥）：阵容弹窗原先**子树里没有任何按钮** ⇒ 打开后关不掉。
            // 这里为结构修复工具新建的 `关闭Button` 绑定关闭动作（找不到时回退通用的 BackoffButton）。
            Transform closeNode = FindDeep(_lineupRoot.transform, "关闭Button");
            if (closeNode == null)
                closeNode = FindDeep(_lineupRoot.transform, "BackoffButton");
            Button closeButton = closeNode != null ? closeNode.GetComponent<Button>() : null;
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseLineup);
                closeButton.onClick.AddListener(CloseLineup);
            }
            for (int index = 0; index < _lineupCards.Count; index++)
                _lineupCards[index].Refresh(index < lineup.Cards.Count ? lineup.Cards[index] : null, _illustrations, _attributes, _qualities);
        }

        private void Back()
        {
            if (_lineupOpen) { CloseLineup(); return; }
            if (_navigation != null) _navigation.ReturnHome();
        }

        private void BindButton(string name, UnityEngine.Events.UnityAction action)
        {
            Transform node = FindDeep(transform, name);
            Button button = node != null ? node.GetComponent<Button>() : null;
            if (button != null) button.onClick.AddListener(action);
        }

        private static void CollectRankRows(GameObject root, List<RankRow> rows, string prefix, out Transform content, out GameObject template)
        {
            rows.Clear();
            template = null;
            content = FindDeep(root != null ? root.transform : null, "Content");
            if (content == null) return;
            for (int index = 0; index < content.childCount; index++)
            {
                if (!content.GetChild(index).name.StartsWith(prefix, StringComparison.Ordinal))
                    continue;

                if (template == null)
                    template = content.GetChild(index).gameObject;   // 首行留作克隆模板
                rows.Add(new RankRow(content.GetChild(index)));
            }
        }

        /// <summary>
        /// 按需把行克隆到 <paramref name="needed"/> 条（上限 <see cref="MaxLeaderboardRows"/>）。
        ///
        /// 场景里每个榜只有 5 行，而数据有 50 条 ⇒ 不补足就只能显示前 5 名。
        /// 克隆体与原行同层同排版，由容器自身的布局组件排布；**不修改场景**，只在运行时生成。
        /// </summary>
        private static void EnsureRankRows(List<RankRow> rows, Transform content, GameObject template, int needed)
        {
            if (content == null || template == null || needed <= rows.Count)
                return;

            int target = Mathf.Min(needed, MaxLeaderboardRows);
            for (int index = rows.Count; index < target; index++)
            {
                GameObject clone = UnityEngine.Object.Instantiate(template, content);
                clone.name = template.name + " (runtime " + index + ")";
                clone.SetActive(false);
                rows.Add(new RankRow(clone.transform));
            }
        }

        private void CollectLineupCards()
        {
            // 2026-09-21（BUG-026 ⑤ 根因）：原先只查**第一个** `Content`、且只认 `展示卡牌` 前缀 ——
            // 而实测阵容弹窗里**本来就有 6 张 `角色卡牌Button-final`**（既有列表），于是**一张都没被收集**。
            // 现改为：在**整个阵容弹窗内**按名收集，两种前缀都收（`展示卡牌` = 结构修复工具新建；
            // `角色卡牌Button` = 场景既有卡），保证"有卡就显示"。
            _lineupCards.Clear();
            if (_lineupRoot == null)
                return;

            Transform[] all = _lineupRoot.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < all.Length; index++)
            {
                string name = all[index].name;

                // ⚠️ 必须排除**容器**名：场景里的滚动容器就叫 `展示卡牌Scroll View`（同样以 `展示卡牌` 开头）。
                // 若把它当成一张卡，`Refresh(null)` 会 `SetActive(false)` ⇒ **整个滚动区连同里面的卡一起被关掉**。
                if (name.IndexOf("Scroll", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                bool isCard = name.StartsWith("角色卡牌Button", StringComparison.Ordinal)
                              || (name.StartsWith("展示卡牌", StringComparison.Ordinal) &&
                                  name.Length > "展示卡牌".Length &&
                                  char.IsDigit(name["展示卡牌".Length]));
                if (isCard)
                    _lineupCards.Add(new LineupCardRow(all[index]));
            }
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int index = 0; index < root.childCount; index++)
            {
                Transform match = FindDeep(root.GetChild(index), name);
                if (match != null) return match;
            }
            return null;
        }

        private static TMP_Text GetText(Transform root, string name)
        {
            Transform node = FindDeep(root, name);
            return node != null ? node.GetComponentInChildren<TMP_Text>(true) : null;
        }
        private static Image GetImage(Transform root, string name)
        {
            Transform node = FindDeep(root, name);
            return node != null ? node.GetComponent<Image>() : null;
        }
        private static void SetText(Transform root, string name, string value)
        {
            TMP_Text text = GetText(root, name);
            if (text != null) text.text = value ?? string.Empty;
        }
        private static void SetSprite(Image image, string id, VisualBinding[] bindings)
        {
            if (image == null || bindings == null || string.IsNullOrEmpty(id)) return;
            for (int index = 0; index < bindings.Length; index++)
                if (bindings[index].Id == id && bindings[index].Sprite != null) { image.sprite = bindings[index].Sprite; return; }
        }

        private sealed class RankRow
        {
            private readonly Transform _root;
            private readonly TMP_Text _rank, _name, _power, _title, _floor, _turn;
            private readonly Image _avatar, _frame, _badge;
            private readonly Button _view;

            public RankRow(Transform root)
            {
                _root = root;
                _rank = GetText(root, "排名数字"); _name = GetText(root, "玩家名称");
                _power = GetText(root, "战力"); _title = GetText(root, "称号");
                _floor = GetText(root, "层数"); _turn = GetText(root, "回合数");
                _avatar = GetImage(root, "Avatar"); _frame = GetImage(root, "AvatarFrame"); _badge = GetImage(root, "徽章");
                Transform view = FindDeep(root, "查看"); _view = view != null ? view.GetComponent<Button>() : null;
            }

            public void Refresh(ClientLeaderboardEntry entry, bool showProgress, VisualBinding[] avatars, VisualBinding[] frames, VisualBinding[] badges, Action<string> onView)
            {
                _root.gameObject.SetActive(entry != null);
                if (entry == null) return;
                Set(_rank, entry.Rank.ToString()); Set(_name, entry.DisplayName); Set(_power, entry.CombatPower.ToString()); Set(_title, entry.Title);
                Set(_floor, showProgress ? entry.Floor.ToString() : string.Empty); Set(_turn, showProgress ? entry.TurnCount.ToString() : string.Empty);
                SetSprite(_avatar, entry.AvatarId.ToString(), avatars); SetSprite(_frame, entry.AvatarFrameId.ToString(), frames); SetSprite(_badge, entry.BadgeId.ToString(), badges);
                if (_view != null) { _view.onClick.RemoveAllListeners(); string playerId = entry.PlayerId; _view.onClick.AddListener(() => onView(playerId)); }
            }
            private static void Set(TMP_Text text, string value) { if (text != null) text.text = value ?? string.Empty; }
        }

        private sealed class LineupCardRow
        {
            private readonly Transform _root;
            private readonly TMP_Text _name, _level;
            private readonly Image _illustration, _attribute, _qualityFrame, _quality;
            private readonly List<Image> _stars = new List<Image>();
            public LineupCardRow(Transform root)
            {
                _root = root; _name = GetText(root, "卡牌名称");
                // 2026-09-21（BUG-026 ⑤）：共用卡 prefab 里的等级文本叫 `等级-Text`，而这里原先只找 `LV等级`
                // ⇒ 阵容卡即使建出来也读不到等级。改为**优先 `LV等级`、回退 `等级-Text`**（不新增美术、不改 prefab）。
                _level = GetText(root, "LV等级") ?? GetText(root, "等级-Text");
                _illustration = GetImage(root, "立绘Image"); _attribute = GetImage(root, "属性图标Image");
                _qualityFrame = GetImage(root, "品质底框Image"); _quality = GetImage(root, "品质Image");
                for (int index = 1; index <= 5; index++) _stars.Add(GetImage(root, "星级图标Image (" + index + ")"));
            }
            public void Refresh(ClientLineupCard card, VisualBinding[] illustrations, VisualBinding[] attributes, VisualBinding[] qualities)
            {
                _root.gameObject.SetActive(card != null);
                if (card == null) return;
                Set(_name, card.DisplayName); Set(_level, "LV." + card.Level);
                SetSprite(_illustration, card.IllustrationId, illustrations); SetSprite(_attribute, card.AttributeIconId, attributes);
                SetSprite(_qualityFrame, card.QualityId, qualities); SetSprite(_quality, card.QualityId, qualities);
                for (int index = 0; index < _stars.Count; index++) if (_stars[index] != null) _stars[index].gameObject.SetActive(index < Mathf.Clamp(card.StarCount, 0, 5));
            }
            private static void Set(TMP_Text text, string value) { if (text != null) text.text = value ?? string.Empty; }
        }
    }
}
