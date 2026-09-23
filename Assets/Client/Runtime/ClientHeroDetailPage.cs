using System;
using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pinball.Client
{
    public interface IClientHeroDetailSpriteResolver
    {
        Sprite Resolve(int imageId);
    }

    /// <summary>只绑定负责人已完成的 HeroDetailPage英雄详情主页，不生成或调整 GUI。</summary>
    public sealed class ClientHeroDetailPage : ClientPageViewBase
    {
        private const string ExpectedRootName = "HeroDetailPage英雄详情主页";

        private sealed class MaterialBinding
        {
            public Image Icon;
            public TMP_Text Owned;
            public TMP_Text Separator;
            public TMP_Text Required;
        }

        public event Action<ClientHero, ClientHeroAbilityType> UpgradeSucceeded;
        public event Action<ClientHeroAbilityType, string> UpgradeFailed;
        public event Action<string> TipRequested;
        public event Action<int> EquipmentSlotRequested;

        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Color _enoughColor = Color.white;
        [SerializeField] private Color _insufficientColor = new Color(0.95f, 0.25f, 0.2f, 1f);

        protected override GameObject PageRoot { get { return _pageRoot; } }
        // 英雄与立绘由 Controller 经 Show(hero, illustration) 注入；本页没有无参刷新，
        // 因此不覆写 OnPageRefresh（基类默认空实现），避免用未注入的 null 英雄重绘。
        protected override void OnPageHidden() { Hide(); }

        private readonly Dictionary<int, Sprite> _registeredSprites = new Dictionary<int, Sprite>();
        private readonly List<Image> _verticalImages = new List<Image>();
        private readonly List<Image> _cardInfoImages = new List<Image>();
        private readonly List<Image> _leftMiddleImages = new List<Image>();
        private readonly List<TMP_Text> _leftMiddleTexts = new List<TMP_Text>();
        private readonly List<TMP_Text> _attributeValues = new List<TMP_Text>();
        private readonly List<Button> _equipmentButtons = new List<Button>();
        private readonly List<UnityAction> _equipmentListeners = new List<UnityAction>();
        private readonly MaterialBinding[] _materials = { new MaterialBinding(), new MaterialBinding() };

        private IClientHeroDetailSpriteResolver _spriteResolver;
        private ClientHero _currentHero;
        private ClientHeroDetailData _currentData;
        private ClientHeroAbilityType _currentSection = ClientHeroAbilityType.Talent;
        private List<int> _illustrationIds = new List<int>();
        private int _illustrationIndex;
        private bool _isBound;
        private bool _dataSubscribed;

        /// <summary>是否只读（未解锁英雄）。见 SetReadOnly。</summary>
        private bool _readOnly;

        private Image _upImage;
        private Image _illustration;
        private Button _leftButton;
        private Button _rightButton;
        private Button _backButton;
        private Button _upgradeButton;
        private Toggle _talentToggle;
        private Toggle _secretToggle;
        private Toggle _ultimateToggle;
        private ClientHeroDetailToggleSkin _toggleSkin;
        private GameObject _talentPage;
        private GameObject _secretPage;
        private GameObject _ultimatePage;
        private TMP_Text _cardName;

        private void Awake()
        {
            if (gameObject.name != ExpectedRootName)
            {
                // 旧脚本曾误挂在“英雄详情装备页”；保留该 GUI，但彻底停用旧详情逻辑。
                enabled = false;
                return;
            }
            EnsureBound();
        }

        private void OnEnable()
        {
            if (gameObject.name != ExpectedRootName)
                return;
            EnsureBound();
            SubscribeData();
        }

        private void OnDisable()
        {
            UnsubscribeData();
        }

        private void OnDestroy()
        {
            UnbindControls();
        }

        public void Configure(GameObject root, Text name, Text level, Text combatPower)
        {
            // 兼容旧编辑器脚本的调用签名；旧 UGUI Text 字段不再参与新主页绑定。
            _pageRoot = root;
        }

        public void ConfigureDetails(Text element, Text starLevel, Text ownership) { }

        public void Show(ClientHero hero) { Show(hero, null); }

        public void Show(ClientHero hero, Sprite cardIllustration)
        {
            ClientScrollFix.FixAll(gameObject, true);   // 2026-09-21：竖向列表撑高 / Clamped / 顶部对齐
            if (hero == null || gameObject.name != ExpectedRootName)
                return;
            if (_pageRoot == null)
                _pageRoot = gameObject;
            _pageRoot.SetActive(true);
            EnsureBound();
            SubscribeData();

            _currentHero = hero;

            // 未解锁英雄进详情 → **只读**（负责人 2026-09-20 需求）。
            //
            // 语义澄清：详情页那个"获取"按钮**不是获取英雄**，而是获取英雄**技能/天赋的升级材料**，
            // 所以未解锁英雄不能点它是正确的 —— 不存在"无法获取"的死结。
            // 而"未解锁也能进详情"同样是需求（图鉴页可点未解锁英雄）。
            // 这里按 IsOwned 自动判定，保证**任何入口**（英雄页 / 图鉴页 / 其它）行为一致。
            SetReadOnly(hero == null || !hero.IsOwned);
            _currentData = ClientServices.Data.GetHeroDetail(hero.HeroId);
            if (_currentData == null)
                _currentData = CreateFallbackData(hero);
            ApplyData(_currentData);
            if (cardIllustration != null && _illustration != null)
                _illustration.sprite = cardIllustration;
            SelectSection(ClientHeroAbilityType.Talent);
        }

        /// <summary>
        /// 只读模式 —— 未解锁英雄的详情页用（负责人 2026-09-20 需求）。
        ///
        /// 只影响**交互**：不改任何视觉结构、不动排版、不隐藏任何节点。
        /// - 装备栏（`upCanvas/Panel`、`Panel (1)` 里的槽位按钮）不可点
        /// - `前往获取+已满级Button`（语义是"获取技能/天赋的升级材料"）不可点
        ///
        /// **不限制** 天赋 / 秘技 / 终结技 三个页签 Toggle —— 负责人 2026-09-20 明确：
        /// 只读只作用于装备槽与升级按钮，三个 Toggle **任何时候都可切**。
        ///
        /// 已解锁英雄传入 `false` 即全部恢复。**幂等**，可重复调用
        /// （因为按钮引用可能在首次绑定时才装配好，故 Show 后需再调用一次）。
        /// </summary>
        public void SetReadOnly(bool readOnly)
        {
            _readOnly = readOnly;

            Transform root = PageRoot != null ? PageRoot.transform : transform;

            // 直接按路径解析，**不依赖 `_equipmentButtons` / `_upgradeButton`**：
            // 两者都不够稳 —— `_upgradeButton` 找的是 "前往获取Button"、场景实际节点名是
            // "前往获取+已满级Button"，上一版 SetReadOnly 因此拿到"空列表 + null"，什么都不改
            // （负责人反馈"没有区别"）。`_equipmentButtons` 虽由 `BindEquipmentButtons` 填充，
            // 但只在 `EnsureBound` 里装配，首次 `Show` 时可能尚未就绪；按路径解析对调用时机不敏感。
            List<Button> targets = new List<Button>();

            // 装备栏：负责人指明是 upCanvas/Panel 与 upCanvas/Panel (1) 里的 4 个 button。
            Transform upCanvas = FindDeepChild(root, "upCanvas");
            CollectButtons(upCanvas, "Panel", targets);
            CollectButtons(upCanvas, "Panel (1)", targets);

            // 升级/获取按钮：语义是"获取英雄技能/天赋的升级材料"。
            Transform upgrade = FindDeepChild(root, "前往获取+已满级Button");
            if (upgrade == null)
                upgrade = FindDeepChild(root, "前往获取Button");
            if (upgrade != null)
            {
                Button upgradeButton = upgrade.GetComponentInChildren<Button>(true);
                if (upgradeButton != null && !targets.Contains(upgradeButton))
                    targets.Add(upgradeButton);
            }

            for (int i = 0; i < targets.Count; i++)
                targets[i].interactable = !readOnly;

            // 三个页签 Toggle **不受只读影响**（负责人 2026-09-20 明确）：
            // 只读只作用于装备槽与升级按钮，天赋/秘技/终结技**任何时候都可切**。
            // 早期版本曾在此把三个 Toggle 一并锁住 —— 属超范围实现，已撤除。

            Debug.Log("[Client] 英雄详情只读=" + readOnly + "，已作用于 " + targets.Count +
                      " 个按钮（装备栏 panel=" + CountButtons(upCanvas, "Panel") +
                      " / panel(1)=" + CountButtons(upCanvas, "Panel (1)") +
                      "），升级按钮=" + (upgrade != null ? upgrade.name : "(未找到)"));
        }

        /// <summary>收集指定子节点下的全部 Button（含未激活），去重后并入 into。</summary>
        private static void CollectButtons(Transform parent, string childName, List<Button> into)
        {
            if (parent == null)
                return;
            Transform child = parent.Find(childName);
            if (child == null)
                return;
            Button[] buttons = child.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
                if (buttons[i] != null && !into.Contains(buttons[i]))
                    into.Add(buttons[i]);
        }

        private static int CountButtons(Transform parent, string childName)
        {
            if (parent == null)
                return -1;
            Transform child = parent.Find(childName);
            return child != null ? child.GetComponentsInChildren<Button>(true).Length : -1;
        }

        /// <summary>当前是否只读（未解锁英雄）。</summary>
        public bool IsReadOnly { get { return _readOnly; } }

        public void Hide()        {
            if (_pageRoot != null)
                _pageRoot.SetActive(false);
        }

        public void SetSpriteResolver(IClientHeroDetailSpriteResolver resolver)
        {
            _spriteResolver = resolver;
            if (_currentData != null)
                ApplyData(_currentData);
        }

        public void RegisterSprite(int imageId, Sprite sprite)
        {
            if (imageId <= 0 || sprite == null)
                return;
            _registeredSprites[imageId] = sprite;
        }

        public void ApplyData(ClientHeroDetailData data)
        {
            if (data == null)
                return;
            EnsureBound();
            _currentData = data;
            SetUpImage(data.UpImageId);
            SetIllustrations(data.IllustrationIds);
            SetVerticalImages(data.VerticalImageIds);
            SetCardInfoImages(data.CardInfoImageIds);
            SetCardName(data.CardName);
            SetLeftMiddleImages(data.LeftMiddleImageIds);
            SetLeftMiddleTexts(data.LeftMiddleTexts);
            SetAttributeValues(data.AttributeValues);
        }

        public void SetUpImage(int imageId) { SetImageById(_upImage, imageId); }

        public void SetIllustrations(IReadOnlyList<int> imageIds)
        {
            _illustrationIds = imageIds != null ? new List<int>(imageIds) : new List<int>();
            _illustrationIndex = 0;
            RefreshIllustration();
        }

        public void ShowPreviousIllustration()
        {
            if (_illustrationIds.Count == 0)
                return;
            _illustrationIndex = (_illustrationIndex - 1 + _illustrationIds.Count) % _illustrationIds.Count;
            RefreshIllustration();
        }

        public void ShowNextIllustration()
        {
            if (_illustrationIds.Count == 0)
                return;
            _illustrationIndex = (_illustrationIndex + 1) % _illustrationIds.Count;
            RefreshIllustration();
        }

        public void SetVerticalImages(IReadOnlyList<int> imageIds) { SetIndexedImages(_verticalImages, imageIds); }
        public void SetCardInfoImages(IReadOnlyList<int> imageIds) { SetIndexedImages(_cardInfoImages, imageIds); }
        public void SetLeftMiddleImages(IReadOnlyList<int> imageIds) { SetIndexedImages(_leftMiddleImages, imageIds); }

        public void SetCardName(string value)
        {
            if (_cardName != null)
                _cardName.text = value ?? string.Empty;
        }

        public void SetLeftMiddleTexts(IReadOnlyList<string> values)
        {
            if (values == null)
                return;
            for (int index = 0; index < _leftMiddleTexts.Count && index < values.Count; index++)
                _leftMiddleTexts[index].text = values[index] ?? string.Empty;
        }

        public void SetLeftMiddleTextStyle(int index, TMP_FontAsset font, float fontSize, Color color)
        {
            if (index < 0 || index >= _leftMiddleTexts.Count)
                return;
            TMP_Text text = _leftMiddleTexts[index];
            if (font != null)
                text.font = font;
            if (fontSize > 0f)
                text.fontSize = fontSize;
            text.color = color;
        }

        public void SetAttributeValues(IReadOnlyList<string> values)
        {
            if (values == null)
                return;
            for (int index = 0; index < _attributeValues.Count && index < values.Count; index++)
                _attributeValues[index].text = values[index] ?? string.Empty;
        }

        public void SetAttributeValue(int index, string value)
        {
            if (index >= 0 && index < _attributeValues.Count)
                _attributeValues[index].text = value ?? string.Empty;
        }

        public void SelectTalent() { SelectSection(ClientHeroAbilityType.Talent); }
        public void SelectSecretTechnique() { SelectSection(ClientHeroAbilityType.SecretTechnique); }
        public void SelectUltimate() { SelectSection(ClientHeroAbilityType.Ultimate); }

        public void SelectSection(ClientHeroAbilityType section)
        {
            _currentSection = section;
            SetToggleWithoutNotify(_talentToggle, section == ClientHeroAbilityType.Talent);
            SetToggleWithoutNotify(_secretToggle, section == ClientHeroAbilityType.SecretTechnique);
            SetToggleWithoutNotify(_ultimateToggle, section == ClientHeroAbilityType.Ultimate);
            if (_talentPage != null) _talentPage.SetActive(section == ClientHeroAbilityType.Talent);
            if (_secretPage != null) _secretPage.SetActive(section == ClientHeroAbilityType.SecretTechnique);
            if (_ultimatePage != null) _ultimatePage.SetActive(section == ClientHeroAbilityType.Ultimate);
            ApplyToggleSkin(section);
            RefreshUpgradePreview();
        }

        /// <summary>
        /// 让三个页签的**底框/文字贴图**跟着选中段变（负责人 2026-09-21 反馈的原始需求）。
        /// 换 sprite 的引用由编辑器工具写入场景，此处只按选中态切换显隐。
        /// </summary>
        private void ApplyToggleSkin(ClientHeroAbilityType section)
        {
            if (_toggleSkin == null)
                return;

            int index = section == ClientHeroAbilityType.Talent ? 0
                : section == ClientHeroAbilityType.SecretTechnique ? 1 : 2;
            _toggleSkin.Apply(index);
        }

        public void SetSectionText(ClientHeroAbilityType section, string value)
        {
            GameObject page = section == ClientHeroAbilityType.Talent ? _talentPage
                : section == ClientHeroAbilityType.SecretTechnique ? _secretPage : _ultimatePage;
            TMP_Text text = page != null ? page.GetComponentInChildren<TMP_Text>(true) : null;
            if (text != null)
                text.text = value ?? string.Empty;
        }

        public void RequestUpgrade()
        {
            if (_currentHero == null)
                return;
            string failureReason;
            if (ClientServices.Data.TryUpgradeHeroAbility(_currentHero.HeroId, _currentSection, out failureReason))
            {
                UpgradeSucceeded?.Invoke(_currentHero, _currentSection);
                _currentData = ClientServices.Data.GetHeroDetail(_currentHero.HeroId);
                if (_currentData != null)
                    ApplyData(_currentData);
                RefreshUpgradePreview();
                return;
            }

            if (string.IsNullOrEmpty(failureReason))
                failureReason = "暂时无法升级";
            UpgradeFailed?.Invoke(_currentSection, failureReason);
            TipRequested?.Invoke(failureReason);
            Debug.LogWarning("[Client] " + failureReason + "（Tips 弹窗待接入）");
            RefreshUpgradePreview();
        }

        public void SetMaterialTextStyle(int slotIndex, TMP_FontAsset font, float fontSize, Color enoughColor, Color insufficientColor)
        {
            if (slotIndex < 0 || slotIndex >= _materials.Length)
                return;
            _enoughColor = enoughColor;
            _insufficientColor = insufficientColor;
            MaterialBinding binding = _materials[slotIndex];
            TMP_Text[] texts = { binding.Owned, binding.Separator, binding.Required };
            foreach (TMP_Text text in texts)
            {
                if (text == null)
                    continue;
                if (font != null)
                    text.font = font;
                if (fontSize > 0f)
                    text.fontSize = fontSize;
            }
            RefreshUpgradePreview();
        }

        public void SetEquipmentSlotVisible(int slotIndex, bool visible)
        {
            if (slotIndex >= 0 && slotIndex < _equipmentButtons.Count)
                _equipmentButtons[slotIndex].gameObject.SetActive(visible);
        }

        public void SetEquipmentSlotInteractable(int slotIndex, bool interactable)
        {
            if (slotIndex >= 0 && slotIndex < _equipmentButtons.Count)
                _equipmentButtons[slotIndex].interactable = interactable;
        }

        public void SetEquipmentSlotImage(int slotIndex, int imageId)
        {
            if (slotIndex < 0 || slotIndex >= _equipmentButtons.Count)
                return;
            Transform imageNode = FindDirectChild(_equipmentButtons[slotIndex].transform, "Image");
            SetImageById(imageNode != null ? imageNode.GetComponent<Image>() : _equipmentButtons[slotIndex].GetComponent<Image>(), imageId);
        }

        public void SetImageById(Image target, int imageId)
        {
            if (target == null || imageId <= 0)
                return;
            Sprite sprite;
            if (!_registeredSprites.TryGetValue(imageId, out sprite) && _spriteResolver != null)
                sprite = _spriteResolver.Resolve(imageId);
            if (sprite != null)
                target.sprite = sprite;
        }

        private void EnsureBound()
        {
            if (_isBound || gameObject.name != ExpectedRootName)
                return;
            if (_pageRoot == null)
                _pageRoot = gameObject;

            Transform upCanvas = FindDeepChild(_pageRoot.transform, "upCanvas");
            Transform downCanvas = FindDeepChild(_pageRoot.transform, "downCanvas");
            _upImage = GetImage(FindDirectChild(upCanvas, "upImage"));
            _illustration = GetImage(FindDirectChild(upCanvas, "立绘"));
            _leftButton = GetButton(FindDirectChild(upCanvas, "left"));
            _rightButton = GetButton(FindDirectChild(upCanvas, "right"));
            _backButton = GetButton(FindDirectChild(_pageRoot.transform, "后退Button"));

            Transform upgradeCanvas = FindDirectChild(_pageRoot.transform, "前往获取Canvas");
            _upgradeButton = GetButton(FindDeepChild(upgradeCanvas, "前往获取Button"));
            // 场景实际不存在 `前往获取Canvas`（已用场景文件核对：全场景 0 个同名节点），
            // 因此 `_upgradeButton` 一直是 null → 下方 `BindControls` 的 onClick 从不装配，
            // 该按钮对**已解锁**英雄也点不动。补上与 `SetReadOnly` 相同的名字回退，两处目标一致。
            if (_upgradeButton == null)
                _upgradeButton = GetButton(FindDeepChild(_pageRoot.transform, "前往获取+已满级Button"));
            // 绑定完成后重新套用只读状态：Show 里首次调用时按钮引用可能尚未装配。
            SetReadOnly(_readOnly);

            Transform verticalPanel = FindDirectChild(upCanvas, "垂直Panel");
            AddComponents(verticalPanel, _verticalImages);
            Transform cardInfoCanvas = FindDirectChild(upCanvas, "卡牌信息Canvas");
            AddComponents(cardInfoCanvas, _cardInfoImages);
            _cardName = FindLastText(cardInfoCanvas, "卡牌名称");

            Transform leftMiddle = FindDirectChild(downCanvas, "leftmiddleCanvas");
            AddComponents(leftMiddle, _leftMiddleImages);
            AddComponents(leftMiddle, _leftMiddleTexts);

            BindAttributes(downCanvas);
            BindSections(downCanvas);
            BindMaterials(downCanvas);
            BindEquipmentButtons(upCanvas);
            BindControls();
            SelectSection(ClientHeroAbilityType.Talent);

            _isBound = upCanvas != null && downCanvas != null;
            if (!_isBound)
                Debug.LogError("[Client] HeroDetailPage英雄详情主页缺少 upCanvas 或 downCanvas。");
        }

        private void BindAttributes(Transform downCanvas)
        {
            string[] canvasNames =
            {
                "攻击属性Canvas", "生命属性Canvas", "防御属性Canvas",
                "暴击率属性Canvas", "暴击伤害属性Canvas", "速度属性Canvas",
            };
            Transform attributePanel = FindDeepChild(downCanvas, "属性Panel");
            foreach (string canvasName in canvasNames)
            {
                Transform canvas = FindDeepChild(attributePanel, canvasName);
                Transform value = FindDeepChild(canvas, "数值");
                TMP_Text text = value != null ? value.GetComponent<TMP_Text>() : null;
                if (text != null)
                    _attributeValues.Add(text);
            }
        }

        private void BindSections(Transform downCanvas)
        {
            _talentPage = GetObject(FindDirectChild(downCanvas, "天赋页"));
            _secretPage = GetObject(FindDirectChild(downCanvas, "秘技页"));
            _ultimatePage = GetObject(FindDirectChild(downCanvas, "终结技页"));
            Transform group = FindDeepChild(downCanvas, "group");
            _talentToggle = GetToggle(FindDirectChild(group, "天赋Toggle"));
            _secretToggle = GetToggle(FindDirectChild(group, "秘技Toggle"));
            _ultimateToggle = GetToggle(FindDirectChild(group, "终结技Toggle"));

            // 三个页签的**选中态皮肤**由 ClientHeroDetailToggleSkin 统一驱动，见该组件注释：
            // `Background` 只在该页签选中时显示（进游戏那套「选中框」贴图）、`Checkmark` 是常态恒显的文字贴图。
            //
            // ⚠️ 2026-09-21 修正：这里**不能**再把 `toggle.graphic` 置空。
            // 旧实现把它置空，于是点击页签时"页面切换了、页签自己的贴图却永远不变"（负责人反馈的原始现象）。
            // 但也不能反过来把文字贴图交给 `graphic` —— Unity 的 Toggle 在 `isOn == false` 时会把
            // `graphic` 的 alpha 置 0（见 UI.Toggle.PlayEffect），那样**未选中的两个页签文字会消失**。
            // 因此改为：`graphic` 保持置空（由皮肤组件控显隐），文字/底框贴图由皮肤组件直接切换。
            _toggleSkin = GetComponent<ClientHeroDetailToggleSkin>();
            if (_toggleSkin == null)
                _toggleSkin = gameObject.AddComponent<ClientHeroDetailToggleSkin>();
            BindToggleSkin(_talentToggle);
            BindToggleSkin(_secretToggle);
            BindToggleSkin(_ultimateToggle);

            if (_talentToggle != null) _talentToggle.graphic = null;
            if (_secretToggle != null) _secretToggle.graphic = null;
            if (_ultimateToggle != null) _ultimateToggle.graphic = null;
        }

        /// <summary>把一个页签的 `Background`（底框）与 `Checkmark`（文字贴图）绑给皮肤组件。</summary>
        private void BindToggleSkin(Toggle toggle)
        {
            if (_toggleSkin == null || toggle == null)
                return;

            Transform root = toggle.transform;
            Transform frame = FindDirectChild(root, "Background");
            // ⚠️ 实测：`Checkmark` 是 **`Background` 的子节点**，不是 Toggle 的直接子节点
            //（`Toggle/Background/Checkmark`）。上一版按"Toggle 的直接子节点"找，永远找不到 ⇒
            // 文字贴图恒不显示。这里两级都找，兼容将来把它挪回 Toggle 下的情况。
            Transform label = FindDirectChild(root, "Checkmark");
            if (label == null)
                label = FindDirectChild(frame, "Checkmark");

            _toggleSkin.Bind(toggle,
                             frame != null ? frame.GetComponent<Image>() : null,
                             label != null ? label.GetComponent<Image>() : null,
                             null);   // 贴图由编辑器工具写入场景，运行时只负责按选中态切换
        }

        private void BindMaterials(Transform downCanvas)
        {
            Transform materialsRoot = FindDeepChild(downCanvas, "materials");
            for (int index = 0; index < _materials.Length; index++)
            {
                Transform slot = FindDirectChild(materialsRoot, "Material " + (index + 1));
                _materials[index].Icon = GetImage(FindDirectChild(slot, "CSIconImage"));
                _materials[index].Owned = GetText(FindDirectChild(slot, "CSText (TMP)left"));
                _materials[index].Separator = GetText(FindDirectChild(slot, "CSText (TMP)middle"));
                _materials[index].Required = GetText(FindDirectChild(slot, "CSText (TMP)right"));
                if (_materials[index].Separator != null)
                    _materials[index].Separator.text = "/";
            }
        }

        private void BindEquipmentButtons(Transform upCanvas)
        {
            Transform firstPanel = FindDirectChild(upCanvas, "Panel");
            Transform secondPanel = FindDirectChild(upCanvas, "Panel (1)");
            AddComponents(firstPanel, _equipmentButtons);
            AddComponents(secondPanel, _equipmentButtons);
        }

        private void BindControls()
        {
            if (_leftButton != null)
            {
                _leftButton.onClick = new Button.ButtonClickedEvent();
                _leftButton.onClick.AddListener(ShowPreviousIllustration);
            }
            if (_rightButton != null)
            {
                _rightButton.onClick = new Button.ButtonClickedEvent();
                _rightButton.onClick.AddListener(ShowNextIllustration);
            }
            if (_upgradeButton != null)
            {
                _upgradeButton.onClick = new Button.ButtonClickedEvent();
                _upgradeButton.onClick.AddListener(RequestUpgrade);
            }
            if (_backButton != null)
            {
                _backButton.onClick = new Button.ButtonClickedEvent();
                ClientUiNavigator navigator = GetComponentInParent<ClientUiNavigator>();
                if (navigator != null)
                    _backButton.onClick.AddListener(navigator.Back);
            }

            BindToggle(_talentToggle, ClientHeroAbilityType.Talent);
            BindToggle(_secretToggle, ClientHeroAbilityType.SecretTechnique);
            BindToggle(_ultimateToggle, ClientHeroAbilityType.Ultimate);

            for (int index = 0; index < _equipmentButtons.Count; index++)
            {
                int capturedIndex = index;
                UnityAction listener = () => EquipmentSlotRequested?.Invoke(capturedIndex);
                _equipmentButtons[index].onClick.AddListener(listener);
                _equipmentListeners.Add(listener);
            }
        }

        private void BindToggle(Toggle toggle, ClientHeroAbilityType section)
        {
            if (toggle == null)
                return;
            toggle.onValueChanged = new Toggle.ToggleEvent();
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                    SelectSection(section);
            });
        }

        private void UnbindControls()
        {
            for (int index = 0; index < _equipmentButtons.Count && index < _equipmentListeners.Count; index++)
                if (_equipmentButtons[index] != null)
                    _equipmentButtons[index].onClick.RemoveListener(_equipmentListeners[index]);
            _equipmentListeners.Clear();
        }

        private void RefreshIllustration()
        {
            if (_illustrationIds.Count > 0)
                SetImageById(_illustration, _illustrationIds[_illustrationIndex]);
        }

        private void RefreshUpgradePreview()
        {
            if (_currentHero == null || !ClientServices.IsInitialized)
                return;
            ClientHeroAbilityUpgradePreview preview = ClientServices.Data.GetHeroAbilityUpgradePreview(_currentHero.HeroId, _currentSection);
            for (int index = 0; index < _materials.Length; index++)
            {
                MaterialBinding binding = _materials[index];
                ClientHeroUpgradeCost cost = preview != null && index < preview.Costs.Count ? preview.Costs[index] : null;
                if (binding.Icon != null)
                    binding.Icon.gameObject.SetActive(cost != null);
                if (binding.Owned != null)
                {
                    binding.Owned.gameObject.SetActive(cost != null);
                    if (cost != null)
                    {
                        binding.Owned.text = cost.OwnedQuantity.ToString();
                        binding.Owned.color = cost.IsEnough ? _enoughColor : _insufficientColor;
                    }
                }
                if (binding.Separator != null)
                {
                    binding.Separator.gameObject.SetActive(cost != null);
                    binding.Separator.text = "/";
                }
                if (binding.Required != null)
                {
                    binding.Required.gameObject.SetActive(cost != null);
                    if (cost != null)
                    {
                        binding.Required.text = cost.RequiredQuantity.ToString();
                        binding.Required.color = cost.IsEnough ? _enoughColor : _insufficientColor;
                    }
                }
                if (cost != null)
                    SetImageById(binding.Icon, cost.IconId);
            }
        }

        private void SubscribeData()
        {
            if (_dataSubscribed || !ClientServices.IsInitialized)
                return;
            ClientServices.Data.DataChanged += HandleDataChanged;
            _dataSubscribed = true;
        }

        private void UnsubscribeData()
        {
            if (!_dataSubscribed || !ClientServices.IsInitialized)
                return;
            ClientServices.Data.DataChanged -= HandleDataChanged;
            _dataSubscribed = false;
        }

        private void HandleDataChanged()
        {
            RefreshUpgradePreview();
        }

        private void SetIndexedImages(List<Image> targets, IReadOnlyList<int> imageIds)
        {
            if (imageIds == null)
                return;
            for (int index = 0; index < targets.Count && index < imageIds.Count; index++)
                SetImageById(targets[index], imageIds[index]);
        }

        private static void SetToggleWithoutNotify(Toggle toggle, bool value)
        {
            if (toggle != null)
                toggle.SetIsOnWithoutNotify(value);
        }

        private static ClientHeroDetailData CreateFallbackData(ClientHero hero)
        {
            ClientHeroDetailData data = new ClientHeroDetailData { HeroId = hero.HeroId, CardName = hero.Name };
            data.LeftMiddleTexts.AddRange(new[] { "Lv.", hero.Level.ToString(), hero.StarLevel.ToString() });
            data.AttributeValues.AddRange(new[] { "0", "0", "0", "0%", "0%", "0" });
            return data;
        }

        private static Transform FindDirectChild(Transform root, string nodeName)
        {
            if (root == null)
                return null;
            for (int index = 0; index < root.childCount; index++)
                if (root.GetChild(index).name == nodeName)
                    return root.GetChild(index);
            return null;
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

        private static TMP_Text FindLastText(Transform root, string nodeName)
        {
            TMP_Text result = null;
            if (root == null)
                return null;
            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text text in texts)
                if (text.name == nodeName)
                    result = text;
            return result;
        }

        private static void AddComponents<T>(Transform root, List<T> target) where T : Component
        {
            if (root == null)
                return;
            target.AddRange(root.GetComponentsInChildren<T>(true));
        }

        private static GameObject GetObject(Transform value) { return value != null ? value.gameObject : null; }
        private static Image GetImage(Transform value) { return value != null ? value.GetComponent<Image>() : null; }
        private static Button GetButton(Transform value) { return value != null ? value.GetComponent<Button>() : null; }
        private static Toggle GetToggle(Transform value) { return value != null ? value.GetComponent<Toggle>() : null; }
        private static Graphic GetGraphic(Transform value) { return value != null ? value.GetComponent<Graphic>() : null; }
        private static TMP_Text GetText(Transform value) { return value != null ? value.GetComponent<TMP_Text>() : null; }
    }
}
