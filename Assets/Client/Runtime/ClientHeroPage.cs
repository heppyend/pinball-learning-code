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
    /// <summary>绑定 HeroPage英雄现有卡牌。不会生成、移动或重排任何 UI 节点。</summary>
    public sealed class ClientHeroPage : ClientPageViewBase, IClientNavigationHost
    {
        private sealed class CardBinding
        {
            public Button Button;
            public Image Illustration;
            public UnityAction Listener;
        }

        [SerializeField] private GameObject _pageRoot;
        private readonly List<CardBinding> _cardBindings = new List<CardBinding>();
        private Transform _content;
        private bool _isBound;
        private IClientHeroDetailOpener _heroDetailOpener;

        [Header("列表模板（MODULE.md §7：1 个模板 + 运行时生成）")]
        [Tooltip("英雄卡模板：场景中保留 1 个并禁用，运行时按英雄数克隆。" +
                 "未指定时回退为直接收集场景内的英雄卡。")]
        [SerializeField] private GameObject _cardTemplate;

        [Tooltip("英雄卡容器：MiddleCanvas/Scroll View/Viewport/Content。")]
        [SerializeField] private RectTransform _cardListRoot;

        private bool UseCardTemplate { get { return _cardTemplate != null && _cardListRoot != null; } }

        [Header("子界面（方案 A：本页维护当前子页，两个子页共用同一套卡牌逻辑）")]
        [Tooltip("HeroSubPage（英雄子界面，仅显示已解锁英雄）")]
        [SerializeField] private GameObject _heroSubPage;

        [Tooltip("GuideSubPage（图鉴子界面，显示全部英雄，按解锁状态设 锁 + Mark 遮罩）")]
        [SerializeField] private GameObject _guideSubPage;

        [Header("页签")]
        [SerializeField] private Button _heroTabButton;
        [SerializeField] private Button _guideTabButton;

        [Tooltip("页签选中态的承载节点（可选）。你之前设的 animation 状态节点拖到这里即可；留空则不处理。")]
        [SerializeField] private GameObject _heroTabSelected;
        [SerializeField] private GameObject _guideTabSelected;

        [Header("进度显示（HeroSubPage/upCanvas/底框Image 下）")]
        [SerializeField] private TMP_Text _ownedProgressText;
        [SerializeField] private TMP_Text _totalHeroCountText;

        /// <summary>两个子页各自的卡牌列表（模板 + 实例的回收复用由 ClientHeroCardList 负责）。</summary>
        private ClientHeroCardList _heroCards;
        private ClientHeroCardList _guideCards;

        /// <summary>当前显示的是否为英雄子页。默认英雄页。</summary>
        private bool _showingHeroSubPage = true;

        /// <summary>当前子页的英雄数据快照，供点击回调按下标取用（避免两个列表索引错位）。</summary>
        private List<ClientHero> _heroListSnapshot = new List<ClientHero>();
        private List<ClientHero> _guideListSnapshot = new List<ClientHero>();

        /// <summary>由导航内核注入，取代原先的 static event HeroDetailRequested。</summary>
        public void BindNavigation(IClientNavigation navigation)
        {
            _heroDetailOpener = navigation as IClientHeroDetailOpener;
        }

        protected override GameObject PageRoot { get { return _pageRoot; } }
        protected override void OnPageRefresh() { BindTabs(); Show(); }

        /// <summary>
        /// 绑定两个页签按钮。用 RemoveListener 再 AddListener，保证重复进入页面不会叠加监听。
        /// `_heroTabSelected` / `_guideTabSelected`（负责人自定义 animation 状态节点）不在此处理，
        /// 由 ApplySubPageVisibility 按当前子页显隐。
        /// </summary>
        private void BindTabs()
        {
            if (_heroTabButton != null)
            {
                _heroTabButton.onClick.RemoveListener(ShowHeroSubPage);
                _heroTabButton.onClick.AddListener(ShowHeroSubPage);
            }
            if (_guideTabButton != null)
            {
                _guideTabButton.onClick.RemoveListener(ShowGuideSubPage);
                _guideTabButton.onClick.AddListener(ShowGuideSubPage);
            }
        }
        protected override void OnPageHidden() { Hide(); }

        private void Awake() { EnsureBound(); }

        private void OnEnable()
        {
            EnsureBound();
            RefreshCards();
        }

        private void OnDisable()
        {
            UnbindCards();
            _isBound = false;
        }

        public void Configure(GameObject pageRoot, GameObject ownedCard, GameObject lockedCard, Text status)
        {
            // 兼容旧编辑器构建入口；旧的双卡和状态文本逻辑已经停用。
            _pageRoot = pageRoot;
        }

        public void Show()
        {
            // 2026-09-21（负责人决定：其余列表页也接入）：竖向列表撑高 / Clamped / 顶部对齐。
            ClientScrollFix.FixAll(gameObject, true);
            if (_pageRoot == null)
                _pageRoot = gameObject;
            _pageRoot.SetActive(true);
            EnsureBound();
            RefreshCards();
        }

        public void Hide()
        {
            if (_pageRoot != null)
                _pageRoot.SetActive(false);
        }

        public void OpenCardByIndex(int cardIndex)
        {
            IReadOnlyList<ClientHero> heroes = ClientServices.Data.GetHeroes();
            if (heroes == null || heroes.Count == 0 || cardIndex < 0)
                return;

            // 保留旧入口（SelectOwnedHero / SelectLockedHero 在用），内部统一走 OpenHero。
            OpenHero(heroes[cardIndex % heroes.Count], null);
        }

        /// <summary>
        /// 按**英雄对象**直接打开详情，不再用"下标 % 全表长度"映射。
        ///
        /// 为什么必须这样：英雄子页只显示已解锁英雄、图鉴子页显示全部，
        /// 两个列表的长度与顺序都不同 —— 用同一个下标去索引全表必然错位。
        /// 各列表在自己的回调里按自己的快照取对象，从根上消除歧义。
        /// </summary>
        public void OpenHero(ClientHero hero, Sprite illustration)
        {
            if (hero == null)
                return;

            if (_heroDetailOpener != null)
                _heroDetailOpener.OpenHeroDetail(hero, illustration);
            else
                Debug.LogWarning("[Client] 英雄详情导航尚未就绪，未打开英雄：" + hero.HeroId);
        }

        public void SetElementFilter(string element)
        {
            // 元素与卡牌的正式映射尚未定义；保留入口但不再硬编码火/水规则。
            RefreshCards();
        }

        public void SelectOwnedHero() { OpenCardByIndex(0); }
        public void SelectLockedHero() { OpenCardByIndex(1); }

        private void EnsureBound()
        {
            if (_isBound)
                return;
            if (_pageRoot == null)
                _pageRoot = gameObject;

            // 模板模式：容器由场景收敛阶段写入，卡牌改由 ClientHeroCardList 统一管理
            // （RefreshCards → _heroCards / _guideCards .Show）。
            //
            // ⚠️ 这里**绝不能再调用 EnsureTemplateInstances**：
            // 那是收敛期的旧路径，它另生成一批克隆体且**从不调用 Apply**，
            // 因而保留模板默认状态（模板里 锁Image 与 编队队内编号Image 默认激活），
            // 表现为"英雄页冒出 2 张带锁、带队内编号的多余卡牌"。
            // 实测日志：EnsureTemplateInstances 与 RefreshCards 两条路径曾同时运行。
            if (UseCardTemplate)
            {
                _content = _cardListRoot;
                _isBound = true;
                return;
            }

            _content = FindDeepChild(_pageRoot.transform, "Content");
            if (_content == null)
            {
                Debug.LogError("[Client] HeroPage 未找到 MiddleCanvas/Scroll View/Viewport/Content。");
                return;
            }

            for (int index = 0; index < _content.childCount; index++)
            {
                Transform card = _content.GetChild(index);
                Button button = card.GetComponent<Button>();
                if (button == null)
                    continue;

                int capturedIndex = _cardBindings.Count;
                UnityAction listener = () => OpenCardByIndex(capturedIndex);
                button.onClick.AddListener(listener);
                Transform illustrationNode = FindDeepChild(card, "立绘Image");
                _cardBindings.Add(new CardBinding
                {
                    Button = button,
                    Illustration = illustrationNode != null ? illustrationNode.GetComponent<Image>() : null,
                    Listener = listener,
                });
            }

            _isBound = _cardBindings.Count > 0;
            if (!_isBound)
                Debug.LogError("[Client] HeroPage Content 中未找到可点击的卡牌 Button。");
        }

        private void RefreshCards()
        {
            if (!ClientServices.IsInitialized)
                return;

            IReadOnlyList<ClientHero> heroes = ClientServices.Data.GetHeroes();
            if (heroes == null)
                return;

            EnsureCardLists();

            // 英雄子页：仅已解锁；图鉴子页：全部（按 IsOwned 决定 锁 + Mark，两者由 ClientHeroCard 同步）。
            _heroListSnapshot = new List<ClientHero>();
            _guideListSnapshot = new List<ClientHero>();
            for (int i = 0; i < heroes.Count; i++)
            {
                ClientHero hero = heroes[i];
                if (hero == null)
                    continue;
                _guideListSnapshot.Add(hero);
                if (hero.IsOwned)
                    _heroListSnapshot.Add(hero);
            }

            if (_heroCards != null && _heroCards.IsValid)
            {
                List<ClientHero> list = _heroListSnapshot;
                _heroCards.Show(list, h => h.Name, h => h.Level, h => true, h => null,
                                i => OpenHero(list[i], null));
            }
            if (_guideCards != null && _guideCards.IsValid)
            {
                List<ClientHero> list = _guideListSnapshot;
                _guideCards.Show(list, h => h.Name, h => h.Level, h => h.IsOwned, h => null,
                                 i => OpenHero(list[i], null));
            }

            RefreshProgress();
            ApplySubPageVisibility();
            // 诊断日志已按待办 8 移除：原先每次 RefreshCards 都打印"英雄页：已解锁 N / 共 M…"，
            // 是控制台第二大噪声源（每次切子页 / 每次 Show 都触发）。数据条数随时可在调试器里
            // 看 `_heroListSnapshot.Count` / `_guideListSnapshot.Count`，无需靠日志。
        }

        /// <summary>
        /// 两个子页各自的"容器 + 模板"都需要一份，故各自建一个 `ClientHeroCardList`。
        /// 复制出 GuideSubPage 时其内部也带了一份卡牌模板，因此两个子页互不干扰。
        /// </summary>
        private void EnsureCardLists()
        {
            if (_heroCards == null && _heroSubPage != null)
            {
                Transform content = FindDeepChild(_heroSubPage.transform, "Content");
                Transform template = FindDeepChild(_heroSubPage.transform, "英雄卡Item");
                _heroCards = new Pinball.Client.UI.ClientHeroCardList(
                    content as RectTransform, template != null ? template.gameObject : null);
            }
            if (_guideCards == null && _guideSubPage != null)
            {
                Transform content = FindDeepChild(_guideSubPage.transform, "Content");
                Transform template = FindDeepChild(_guideSubPage.transform, "英雄卡Item");
                _guideCards = new Pinball.Client.UI.ClientHeroCardList(
                    content as RectTransform, template != null ? template.gameObject : null);
            }
        }

        /// <summary>切换子页（方案 A：同一套卡牌逻辑，两个子页只是数据范围不同）。</summary>
        public void SwitchSubPage(bool showHero)
        {
            _showingHeroSubPage = showHero;
            ApplySubPageVisibility();
            RefreshCards();
        }

        /// <summary>供页签按钮绑定。</summary>
        public void ShowHeroSubPage() { SwitchSubPage(true); }

        /// <summary>供页签按钮绑定。</summary>
        public void ShowGuideSubPage() { SwitchSubPage(false); }

        private void ApplySubPageVisibility()
        {
            if (_heroSubPage != null && _heroSubPage.activeSelf != _showingHeroSubPage)
                _heroSubPage.SetActive(_showingHeroSubPage);
            if (_guideSubPage != null && _guideSubPage.activeSelf == _showingHeroSubPage)
                _guideSubPage.SetActive(!_showingHeroSubPage);

            // 可选节点：若负责人另有独立的选中态节点，拖进 _heroTabSelected/_guideTabSelected 即可；
            // 留空则不影响任何东西（不会报错、不会空引用）。
            if (_heroTabSelected != null && _heroTabSelected.activeSelf != _showingHeroSubPage)
                _heroTabSelected.SetActive(_showingHeroSubPage);
            if (_guideTabSelected != null && _guideTabSelected.activeSelf == _showingHeroSubPage)
                _guideTabSelected.SetActive(!_showingHeroSubPage);

            // ★ 页签选中态：直接驱动 Animator 的 trigger。
            // 负责人确认"只有 pressed 和 selected 是黄色、其余为灰" ——
            // 说明用的是 Button 的 Animation 过渡（Unity 标准 trigger 名：Normal/Highlighted/Pressed/Selected/Disabled）。
            // 只设 EventSystem 选中对象不够：被点过的按钮会**停在 Pressed 状态**，
            // 于是两个页签同时呈黄色。这里显式把非当前页签打回 Normal。
            DriveTabAnimator(_heroTabButton, _showingHeroSubPage);
            DriveTabAnimator(_guideTabButton, !_showingHeroSubPage);
        }

        /// <summary>把页签的 Animator 打到 Selected 或 Normal 状态（trigger 用 Unity 标准命名）。</summary>
        private static void DriveTabAnimator(Button tab, bool selected)
        {
            if (tab == null)
                return;

            Animator animator = tab.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null)
                return;

            animator.ResetTrigger("Pressed");
            animator.ResetTrigger("Selected");
            animator.ResetTrigger("Normal");
            animator.ResetTrigger("Highlighted");
            animator.SetTrigger(selected ? "Selected" : "Normal");
        }

        private void RefreshProgress()
        {
            // 口径："已收集 / 总数" = 已拥有英雄数 / 英雄总数（与设计文案 "55/99" 的意图一致）。
            string owned = _heroListSnapshot.Count.ToString();
            string total = "/" + _guideListSnapshot.Count;

            // ⚠️ 场景里**每个子页各有一套**进度文本：
            //     HeroSubPage/upCanvas/底框Image/{已收集进度, 总英雄数量}
            //     GuideSubPage/upCanvas/底框Image/{已收集进度, 总英雄数量}
            // 两套的设计文案都是烘死的 "55" 与 "/99"。早期只在 Inspector 上接了**一套**，
            // 于是另一个子页永远停在烘死文案上 —— 实测负责人看到"图鉴还显示 55/99"。
            // 这里改为**按子页根节点各自解析**、两套都写，不再依赖单点接线。
            ApplyProgressTexts(_heroSubPage, owned, total);
            ApplyProgressTexts(_guideSubPage, owned, total);

            // 兼容：Inspector 上显式接线的槽位也照写（若与上面指向同一节点，写同值无副作用）。
            if (_ownedProgressText != null)
                _ownedProgressText.text = owned;
            if (_totalHeroCountText != null)
                _totalHeroCountText.text = total;
        }

        private static void ApplyProgressTexts(GameObject subPage, string owned, string total)
        {
            if (subPage == null)
                return;

            SetText(FindDeepChild(subPage.transform, "已收集进度"), owned);
            SetText(FindDeepChild(subPage.transform, "总英雄数量"), total);
        }

        private static void SetText(Transform node, string value)
        {
            if (node == null)
                return;

            TMP_Text text = node.GetComponent<TMP_Text>();
            if (text == null)
                text = node.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
                text.text = value;
        }

        /// <summary>
        /// 按英雄数据条数克隆模板并建立 CardBinding。
        ///
        /// 场景原本有 9 张英雄卡而英雄数据只有 2 条，`RefreshCards` 以前用
        /// `heroes[index % heroes.Count]` **取模填充**，于是 9 张卡反复显示同样的 2 个英雄。
        /// 模板化后卡片数 = 英雄数，重复问题一并消失。
        /// </summary>
        private void EnsureTemplateInstances()
        {
            _cardTemplate.SetActive(false);

            int needed = 0;
            if (ClientServices.IsInitialized && ClientServices.Data.GetHeroes() != null)
                needed = ClientServices.Data.GetHeroes().Count;

            // ⚠️ 必须先回收/复用容器里的旧克隆体。
            //
            // 原实现只按 `_cardBindings.Count < needed` 判断、**从不清理已生成的克隆体**，
            // 而 `UnbindCards()` 会清空 `_cardBindings` 却不销毁克隆体 ——
            // 于是每进出一次英雄页就多生成一轮，实测 **2 → 4 → 6 张累加**，
            // 且旧克隆体的点击下标越界，表现为"只有某一张能进详情"。
            //
            // 现改为：清空绑定 → 收集容器内**非模板**的既有克隆体 → 按需复用 →
            // 不足才新建 → 多余的隐藏（不销毁，便于数据增多时复用）。
            _cardBindings.Clear();

            List<GameObject> existing = new List<GameObject>();
            for (int i = 0; i < _cardListRoot.childCount; i++)
            {
                Transform child = _cardListRoot.GetChild(i);
                if (child.gameObject == _cardTemplate)
                    continue;
                existing.Add(child.gameObject);
            }

            for (int i = 0; i < needed; i++)
            {
                GameObject card;
                if (i < existing.Count)
                {
                    card = existing[i];
                }
                else
                {
                    card = Instantiate(_cardTemplate, _cardListRoot);
                    card.name = "英雄卡Item";
                    if (card.GetComponent<Button>() == null)
                    {
                        Debug.LogWarning("[Client] 英雄卡模板上没有 Button，无法生成可点击卡牌。");
                        Destroy(card);
                        break;
                    }
                }

                card.SetActive(true);
                Button button = card.GetComponent<Button>();
                int capturedIndex = _cardBindings.Count;
                UnityAction listener = () => OpenCardByIndex(capturedIndex);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(listener);
                Transform illustrationNode = FindDeepChild(card.transform, "立绘Image");
                _cardBindings.Add(new CardBinding
                {
                    Button = button,
                    Illustration = illustrationNode != null ? illustrationNode.GetComponent<Image>() : null,
                    Listener = listener,
                });
            }

            // 数据减少时把多余克隆体隐藏，保证显示数量恒等于英雄数。
            for (int i = needed; i < existing.Count; i++)
            {
                if (existing[i] != null && existing[i].activeSelf)
                    existing[i].SetActive(false);
            }

            Debug.Log("[Client] 英雄卡：需要 " + needed + " 张，容器内既有克隆体 " + existing.Count +
                      " 个，已绑定 " + _cardBindings.Count + " 张（模板模式，已回收多余实例）。");
        }

        private void UnbindCards()
        {
            foreach (CardBinding binding in _cardBindings)
                if (binding.Button != null && binding.Listener != null)
                    binding.Button.onClick.RemoveListener(binding.Listener);
            _cardBindings.Clear();
        }

        private static TMP_Text FindText(Transform root, string nodeName)
        {
            Transform node = FindDeepChild(root, nodeName);
            return node != null ? node.GetComponent<TMP_Text>() : null;
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
