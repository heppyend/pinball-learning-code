using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>
    /// 编队页（`FormationPage编队`）的数据绑定与交互。
    ///
    /// 结构与数据来源（2026-09-20 只读取证，证据 `Logs/verify-formationpage.txt`）：
    ///   3 个出战槽位 ← `编队卡牌-Panel` 的 3 张槽位卡（自带 `编队队内编号Image`）
    ///   4 支队伍     ← `页面分栏-Image/Panel` 的 `选中点1..4` + `编队分组-Image/左|右Button` + `编组号-Text`
    ///   英雄选择列表 ← `Scroll View/Viewport/Content`（9 张 `角色卡牌Button-final 1`）
    ///   编队战力     ← `战力框/战力`
    ///
    /// 数据全部经 <see cref="IClientDataService"/> 的编队接口取用（4 队伍 × 3 槽位，本地模拟、可替换）；
    /// 卡面显示一律经卡牌管理器 <see cref="ClientHeroCard"/> / <see cref="ClientHeroCardList"/>，
    /// **本类不直接改任何卡面子节点**（MODULE.md §13）。
    ///
    /// ⚠️ 两处**未定义，本类刻意不猜**（见 MODULE.md §14）：
    ///   ① 元素筛选（全/光/水/土/风/火/暗）与 `ClientHero.Element`（数据侧为 "Fire"/"Water"）的映射；
    ///   ② `选中点1..4` 的“当前队伍”高亮表现、`确认键Button` 的语义。
    ///   这两处只留接口与占位，不落任何规则。
    ///
    /// ⚠️ **场景尚未就绪**：本页的卡（3 张槽位卡 + 9 张选卡）目前**都没有 `ClientHeroCard`**
    ///   （全场景该组件仅 HeroPage 两处），且卡prefab的 `卡牌名称Image` 没有文本子节点。
    ///   因此下面的卡牌逻辑在补齐组件前**不会生效**（`ClientHeroCardList.IsValid == false`）——
    ///   这是预期的，不是 bug；补齐属场景写入，另行确认。
    /// </summary>
    public sealed class ClientFormationPage : ClientPageViewBase
    {
        [SerializeField] private GameObject _root;

        [Tooltip("选卡列表模板（1 模板 + N 实例）。由结构修复的列表收敛（ListTarget）写入；留空则运行时按名字回退查找。")]
        [SerializeField] private GameObject _cardTemplate;

        [Tooltip("选卡列表容器。由结构修复的列表收敛（ListTarget）写入；留空则运行时按名字回退查找。")]
        [SerializeField] private RectTransform _cardListRoot;

        [Tooltip("队内编号的数字贴图，下标 0..9 对应数字 0..9。\n" +
                 "工程内已存在：Assets/Client/UI/HeroDetailPage/Sprites/属性icon/新建文件夹/数字/0..9.png。\n" +
                 "由结构修复工具写入；留空则编号只显隐、不换数字。")]
        [SerializeField] private Sprite[] _teamSlotDigits;

        private bool _isBound;
        private readonly List<ClientHeroCard> _slotCards = new List<ClientHeroCard>();
        private ClientHeroCardList _pickerCards;
        private GameObject _teamCodeNode;
        private GameObject _combatPowerNode;
        private Button _leftTeamButton;
        private Button _rightTeamButton;
        private Button _confirmButton;
        private readonly List<Image> _teamDots = new List<Image>();
        private IClientNavigation _navigation;

        [Tooltip("非当前队伍时「选中点」的透明度。\n" +
                 "⚠️ 高亮表现**未定义**：4 个选中点在场景里是同一贴图、同一颜色（m_Color 1,1,1,1），" +
                 "工程中没有“选中态”变体，因此暂用 alpha 区分；数值可在此调，或告知我应有的表现。")]
        [SerializeField, Range(0f, 1f)] private float _teamDotDimAlpha = 0.35f;

        protected override GameObject PageRoot { get { return _root; } }
        protected override void OnPageRefresh() { Show(); }
        protected override void OnPageHidden() { Hide(); }

        public void Show()
        {
            EnsureBound();
            if (_root != null)
                _root.SetActive(true);
            Refresh();

            // 2026-09-21（负责人反馈：编队的 Scroll View 与排行榜同样"松手弹回、默认不在最上面"）：
            // 运行时修正竖向列表配置（撑高 / Clamped / 顶部对齐），并回到最上面。
            ClientScrollFix.FixAll(gameObject, true);
        }

        public void Hide()
        {
            if (_root != null)
                _root.SetActive(false);
        }

        // ------------------------------------------------------------------
        // 交互入口
        // ------------------------------------------------------------------

        /// <summary>
        /// 点选一张英雄卡 —— **切换语义**（负责人 2026-09-20 明确，取代原先“先点空位再点卡”的流程）：
        /// <list type="bullet">
        /// <item>不在队中 ⇒ **自动**补到**编号最小的空位**（无需先选空位）；</item>
        /// <item>已在队中 ⇒ **移出**它，其余槽位**不补位**，该编号空出；</item>
        /// <item>三个编号都占满 ⇒ **不加入**。</item>
        /// </list>
        /// 规则本体在 <see cref="IClientDataService.TryToggleFormationHero"/>，本页只负责提示与刷新。
        /// </summary>
        public void SelectHero(int heroId)
        {
            if (!ClientServices.IsInitialized)
                return;

            int slotNumber;
            string failureReason;
            if (!ClientServices.Data.TryToggleFormationHero(heroId, out slotNumber, out failureReason))
            {
                SetStatus(string.IsNullOrEmpty(failureReason) ? "该英雄无法加入编队" : failureReason);
                return;
            }

            if (slotNumber > 0)
                SetStatus("已加入编队，编号 " + slotNumber + "（本地模拟）");
            else
                SetStatus("已移出编队，该编号空出（其余不补位）");

            Refresh();
        }

        /// <summary>左右切换队伍（对应 `编组号-Text` 与 `选中点1..4`）。</summary>
        public void SwitchTeam(int delta)
        {
            if (!ClientServices.IsInitialized || delta == 0)
                return;

            IReadOnlyList<ClientFormationTeam> teams = ClientServices.Data.GetFormationTeams();
            if (teams == null || teams.Count == 0)
                return;

            int count = teams.Count;
            int next = ((ClientServices.Data.GetFormationTeamIndex() + delta) % count + count) % count;
            if (!ClientServices.Data.TrySwitchFormationTeam(next))
                return;

            // 不再需要“重置当前槽位”：点选是切换语义，加入时**自动**补编号最小的空位。
            Refresh();
        }

        public void NextTeam() { SwitchTeam(1); }
        public void PreviousTeam() { SwitchTeam(-1); }

        /// <summary>
        /// 「确认键」（`下功能底框/确认键Button`）—— 确认当前编队并返回。
        ///
        /// 语义说明：本地模拟是**即时写入**（点卡即存），不存在独立的“保存”动作，
        /// 因此确认键 = 确认并关闭本页（等同 `后退Button`）。若你要的是别的语义，告知我改。
        /// </summary>
        public void ConfirmTeam()
        {
            if (_navigation != null)
                _navigation.Back();
            else
                SetStatus("编队已确认（本地模拟）；无导航可返回");
        }

        /// <summary>
        /// 刷新「选中点1..4」：当前队伍那个点保持原样，其余降低 alpha。
        ///
        /// ⚠️ 取证（场景 YAML）：4 个选中点是**同一贴图、同一颜色**（`m_Color (1,1,1,1)`、`raycastTarget=0`），
        /// 工程里**没有“选中态”贴图或颜色变体**。所以只能用 alpha 区分，数值 `_teamDotDimAlpha` 可在 Inspector 调。
        /// 另外：这 4 个点的**名字编号与屏幕横坐标正好相反**（选中点1 在最右），
        /// 这里按“名字编号 = 队伍编号”理解（选中点1 ↔ 队伍 01）；若实际显示顺序相反，换一处索引即可。
        /// </summary>
        private void RefreshTeamDots(int teamIndex)
        {
            for (int index = 0; index < _teamDots.Count; index++)
            {
                Image dot = _teamDots[index];
                if (dot == null)
                    continue;

                Color color = dot.color;
                color.a = index == teamIndex ? 1f : _teamDotDimAlpha;
                dot.color = color;
            }
        }

        // ------------------------------------------------------------------
        // 绑定与刷新
        // ------------------------------------------------------------------

        private void EnsureBound()
        {
            if (_isBound)
                return;
            if (_root == null)
                _root = gameObject;

            _navigation = GetComponentInParent<IClientNavigation>();

            // 3 个出战槽位：`编队卡牌-Panel` 下按名字前缀取 3 张卡。
            _slotCards.Clear();
            Transform slotPanel = FindDeepChild(_root.transform, "编队卡牌-Panel");
            if (slotPanel != null)
            {
                for (int index = 0; index < slotPanel.childCount && _slotCards.Count < ClientFormationTeam.SlotCount; index++)
                {
                    Transform child = slotPanel.GetChild(index);
                    if (!child.name.StartsWith("角色卡牌Button-final", System.StringComparison.Ordinal))
                        continue;
                    ClientHeroCard card = child.GetComponent<ClientHeroCard>();
                    if (card != null)
                        _slotCards.Add(card);
                }
            }

            // 英雄选择列表：优先用列表收敛写入的「模板 + 容器」（ConvergeList → ListTarget），
            // 未接线时按名字回退查找 —— 两种情况都不抛异常。
            RectTransform pickerRoot = _cardListRoot;
            if (pickerRoot == null)
                pickerRoot = FindDeepChild(_root.transform, PickerContentNodeName) as RectTransform;

            GameObject pickerTemplate = _cardTemplate;
            if (pickerTemplate == null)
            {
                // 收敛后模板名 = 白名单 ListTarget.TemplateName；未收敛时是场景里的原卡名。
                Transform foundTemplate = FindDeepChild(_root.transform, ConvergedTemplateNodeName)
                                       ?? FindDeepChild(_root.transform, PickerTemplateNodeName);
                pickerTemplate = foundTemplate != null ? foundTemplate.gameObject : null;
            }
            _pickerCards = new ClientHeroCardList(pickerRoot, pickerTemplate);

            _teamCodeNode = FindObject(_root.transform, "编组号-Text");
            _combatPowerNode = FindObject(_root.transform, "战力");
            _leftTeamButton = FindButton(_root.transform, "左Button");
            _rightTeamButton = FindButton(_root.transform, "右Button");
            _confirmButton = FindButton(_root.transform, "确认键Button");

            // 选中点1..4：按名字逐个取 Image（名字形如 `选中点1-Image`）。
            // 缺哪个就放 null（不新建任何节点），刷新时逐个判空。
            _teamDots.Clear();
            for (int dotIndex = 1; dotIndex <= 4; dotIndex++)
            {
                GameObject dotNode = FindObject(_root.transform, "选中点" + dotIndex + "-Image");
                _teamDots.Add(dotNode != null ? dotNode.GetComponent<Image>() : null);
            }

            BindButton(_leftTeamButton, PreviousTeam);
            BindButton(_rightTeamButton, NextTeam);
            BindButton(_confirmButton, ConfirmTeam);
            BindBackButton();

            _isBound = true;
        }

        private void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(action);
        }

        private void BindBackButton()
        {
            Button back = FindButton(_root.transform, "后退Button");
            if (back == null || _navigation == null)
                return;
            back.onClick = new Button.ButtonClickedEvent();
            back.onClick.AddListener(() => _navigation.Back());
        }

        private void Refresh()
        {
            if (!ClientServices.IsInitialized)
                return;

            IReadOnlyList<ClientFormationTeam> teams = ClientServices.Data.GetFormationTeams();
            ClientFormationTeam team = GetCurrentTeam(teams);

            if (_teamCodeNode != null)
                SetText(_teamCodeNode, team != null ? team.TeamCode : string.Empty);

            if (_combatPowerNode != null)
                SetText(_combatPowerNode, ClientServices.Data.GetFormationCombatPower(ClientServices.Data.GetFormationTeamIndex()).ToString());

            // 「选中点1..4」反映当前队伍（高亮表现见 RefreshTeamDots 注释）。
            RefreshTeamDots(ClientServices.Data.GetFormationTeamIndex());

            RefreshSlots(team);
            RefreshPicker();
        }

        private void RefreshSlots(ClientFormationTeam team)
        {
            for (int slotIndex = 0; slotIndex < _slotCards.Count; slotIndex++)
            {
                ClientHeroCard card = _slotCards[slotIndex];
                if (card == null)
                    continue;

                int heroId = team != null && slotIndex < team.SlotHeroIds.Count
                    ? team.SlotHeroIds[slotIndex]
                    : 0;
                ClientHero hero = FindHero(heroId);

                // 空槽位 = **保留格位、不显示卡牌**（负责人 2026-09-20）：
                // 只切**子节点**（`SetContentVisible`），**不动本节点** —— 槽位容器带 HorizontalLayoutGroup，
                // 隐藏本节点会让其余槽位重排。将来换成专门的空槽 image 时，改这一处即可。
                if (hero == null)
                {
                    card.SetContentVisible(false);
                    card.SetTeamSlot(0);
                    continue;
                }

                card.SetContentVisible(true);
                // 槽位卡**接同一个 toggle**（负责人 2026-09-20）：点槽位卡 = 点选卡上那张编号卡，
                // 二者都能把该英雄移出编队（槽位里既然有人，toggle 必然走“移出”分支）。
                int capturedHeroId = hero.HeroId;
                card.Apply(hero.Name, hero.Level, hero.IsOwned, null,
                           () => SelectHero(capturedHeroId));
                // 队内编号**不在槽位卡上显示**（负责人 2026-09-20：该编号显示在 Scroll View 的卡上）。
                card.SetTeamSlot(0);
            }
        }

        private void RefreshPicker()
        {
            if (_pickerCards == null || !_pickerCards.IsValid)
                return;

            IReadOnlyList<ClientHero> heroes = ClientServices.Data.GetHeroes();
            if (heroes == null)
                return;

            List<ClientHero> list = new List<ClientHero>(heroes);
            // teamSlotOf：该英雄在**当前队伍**里的槽位号（1..3），不在队中返回 0 ⇒ 不显示编号。
            // 数字随编队**动态变化**：换队伍 / 放入英雄 / 清空槽位都会走到这里刷新。
            _pickerCards.Show(
                list,
                hero => hero.Name,
                hero => hero.Level,
                hero => hero.IsOwned,
                hero => null,
                hero => GetSlotNumberOf(hero.HeroId),
                _teamSlotDigits,
                index => SelectHero(list[index].HeroId));
        }

        /// <summary>返回该英雄在**当前队伍**中的槽位号（1 起）；不在队中或未初始化返回 0。</summary>
        private static int GetSlotNumberOf(int heroId)
        {
            if (heroId <= 0 || !ClientServices.IsInitialized)
                return 0;

            IReadOnlyList<ClientFormationTeam> teams = ClientServices.Data.GetFormationTeams();
            if (teams == null || teams.Count == 0)
                return 0;

            int teamIndex = ClientServices.Data.GetFormationTeamIndex();
            if (teamIndex < 0 || teamIndex >= teams.Count)
                return 0;

            ClientFormationTeam team = teams[teamIndex];
            for (int index = 0; index < team.SlotHeroIds.Count; index++)
                if (team.SlotHeroIds[index] == heroId)
                    return index + 1;
            return 0;
        }

        /// <summary>
        /// 状态/提示输出。
        ///
        /// 历史：本类曾有 `_slot` / `_status` 两个 `Text` 序列化槽位，但场景里**没有可用文本节点**
        /// （编队页只有 `编组号-Text` / `战力` / 卡上的 `等级-Text`，前两个已各有所属），
        /// `SystemLayer` 下也没有 Toast 节点 ⇒ 2026-09-20 按负责人指示**删除这两个字段**。
        /// 当前提示只进 Console；将来要显示，需先给 `SystemLayer` 加提示节点并接线 `ClientSystemFeedback`。
        /// </summary>
        private void SetStatus(string message)
        {
            Debug.Log("[Client] 编队：" + message);
        }

        // ------------------------------------------------------------------
        // 小工具
        // ------------------------------------------------------------------

        /// <summary>列表收敛（`ListTarget`）写入的模板名 —— 必须与 `ClientShellStructuralRepair` 白名单一致。</summary>
        private const string ConvergedTemplateNodeName = "编队英雄卡Item";

        /// <summary>未收敛时场景里的原卡名（回退用）。</summary>
        private const string PickerTemplateNodeName = "角色卡牌Button-final 1";

        /// <summary>选卡列表容器名（回退用）。</summary>
        private const string PickerContentNodeName = "Content";

        private static ClientFormationTeam GetCurrentTeam(IReadOnlyList<ClientFormationTeam> teams)
        {
            if (teams == null || teams.Count == 0)
                return null;
            int index = ClientServices.Data.GetFormationTeamIndex();
            if (index < 0 || index >= teams.Count)
                return null;
            return teams[index];
        }

        private static ClientHero FindHero(int heroId)
        {
            if (heroId <= 0 || !ClientServices.IsInitialized)
                return null;
            foreach (ClientHero hero in ClientServices.Data.GetHeroes())
                if (hero != null && hero.HeroId == heroId)
                    return hero;
            return null;
        }

        /// <summary>场景里 `编组号-Text` / `战力` 的组件类型尚未核实，故 TMP 与旧版 UGUI 文本都兼容。</summary>
        private static void SetText(GameObject node, string value)
        {
            if (node == null)
                return;
            TMP_Text tmp = node.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = value;
                return;
            }
            Text legacy = node.GetComponent<Text>();
            if (legacy != null)
                legacy.text = value;
        }

        private static GameObject FindObject(Transform root, string nodeName)
        {
            Transform found = FindDeepChild(root, nodeName);
            return found != null ? found.gameObject : null;
        }

        private static Button FindButton(Transform root, string nodeName)
        {
            Transform found = FindDeepChild(root, nodeName);
            return found != null ? found.GetComponent<Button>() : null;
        }

        private static Transform FindDeepChild(Transform root, string nodeName)
        {
            if (root == null)
                return null;
            for (int index = 0; index < root.childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (child.name == nodeName)
                    return child;
                Transform found = FindDeepChild(child, nodeName);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
