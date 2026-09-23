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
    /// <summary>商城既有 GUI 的控制器：不生成或调整任何 UI 节点。</summary>
    public sealed class ClientShopPage : ClientPageViewBase, IClientFeedbackHost, IClientNavigationHost, IClientPopupHost
    {
        private const int DefaultUnitCrystalStoneCost = 10;
        private static int _nextCanvasSortingOrder = 1000;
        [SerializeField] private bool enableNavigationDiagnostics;
        private readonly List<ClientShopListing> _listings = new List<ClientShopListing>();
        private GameObject _regular, _activity, _purchase, _success, _exchange, _orders, _shipment, _bottomFunctionBar;
        private ClientShopPurchaseQuantityProgress _quantity;
        private TMP_Text _costText, _crystalText, _limitText;
        private TMP_InputField _exchangeInput;
        private ClientShopListing _selected;
        private IClientFeedback _feedback;
        private IClientNavigation _navigation;
        private IClientPopupService _popups;

        // 这 5 个弹窗已按确认架构迁入 PopupLayer，不再是商城的内部子节点，
        // 因此原来的 Find("Product Purchase Interface") 之类查找会返回 null。
        // 由场景修复工具按节点名自动写入；也可在 Inspector 手动拖拽。
        [Header("PopupLayer 中的弹窗（已迁出商城内部）")]
        [SerializeField] private GameObject _purchasePopup;
        [SerializeField] private GameObject _successPopup;
        [SerializeField] private GameObject _exchangePopup;
        [SerializeField] private GameObject _ordersPopup;
        [SerializeField] private GameObject _shipmentPopup;

        /// <summary>由组合根注入：弹窗的显隐统一交给弹窗服务，页面不再自己 SetActive。</summary>
        public void BindPopups(IClientPopupService popups) { _popups = popups; }

        /// <summary>按层级顺序缓存的商城卡牌节点（**回退路径**：未配置模板时使用）。</summary>
        private readonly List<Button> _cards = new List<Button>();

        [Header("列表模板（MODULE.md §7：1 个模板 + 运行时生成）")]
        [Tooltip("商城卡牌模板：场景中保留 1 个并禁用，运行时按商品数克隆。" +
                 "未指定时回退为直接使用场景内的卡牌节点。")]
        [SerializeField] private GameObject _cardTemplate;

        [Tooltip("卡牌容器：Product page/展示卡牌Scroll View/Viewport/Content。" +
                 "该容器自带 GridLayoutGroup（cell 214.2×285.6，固定列数 2），克隆体会被自动排布。")]
        [SerializeField] private RectTransform _cardListRoot;

        /// <summary>运行时按数据生成的卡牌实例。</summary>
        private readonly List<Button> _cardInstances = new List<Button>();

        private bool UseCardTemplate { get { return _cardTemplate != null && _cardListRoot != null; } }

        /// <summary>
        /// 收集商城卡牌并按下标显式绑定。
        ///
        /// 原实现用 BindPrefix 一把绑 0..N，但场景里有 **13 张卡** 而 `_listings` 只有 **3 条**
        /// （见 LocalClientDataService._shopListings），于是 10 张卡点了没有任何反应——
        /// `OpenListing` 有越界保护，所以表现为**静默无响应**，极难排查。
        /// 现在改为显式映射，并由 ApplyCards 按数据条数控制显隐。
        /// </summary>
        private void BindCards()
        {
            _cards.Clear();
            if (_regular == null)
                return;

            // 已配置模板时走"运行时生成"，不再收集场景内的内联卡牌。
            if (UseCardTemplate)
            {
                Debug.Log("[Client] 商城卡牌走模板生成模式（模板=" + _cardTemplate.name +
                          "，容器=" + _cardListRoot.name + "）。");
                return;
            }

            foreach (Button button in _regular.GetComponentsInChildren<Button>(true))
                if (button.name.StartsWith("道具卡牌prefab", StringComparison.Ordinal))
                    _cards.Add(button);

            for (int i = 0; i < _cards.Count; i++)
            {
                int index = i;
                // 这些卡牌是场景修复工具补的 Button，没有 Inspector 持久监听，可安全清空。
                _cards[i].onClick.RemoveAllListeners();
                _cards[i].onClick.AddListener(() => OpenListing(index));
            }
            Debug.Log("[Client] 商城卡牌已绑定 " + _cards.Count + " 张（按层级顺序映射到 _listings 下标）。");
        }

        /// <summary>
        /// 按数据条数控制卡牌显隐并写入商品名。
        /// 多出的卡牌**隐藏**而不是留作死区——这是 MODULE.md「列表项数量必须与数据源条数对应」的落地。
        /// 只改 activeSelf 与文本，不动任何 anchor / sizeDelta / anchoredPosition。
        /// </summary>
        private void ApplyCards()
        {
            if (UseCardTemplate)
            {
                ApplyCardsFromTemplate();
                return;
            }

            if (_cards.Count == 0)
                return;

            if (_cards.Count != _listings.Count)
                Debug.Log("[Client] 商城卡牌数(" + _cards.Count + ") 与商品数(" + _listings.Count +
                          ") 不一致：超出部分隐藏。建议按 MODULE.md 约定改为「1 个模板 + 运行时生成」。");

            for (int i = 0; i < _cards.Count; i++)
            {
                bool hasListing = i < _listings.Count;
                if (_cards[i].gameObject.activeSelf != hasListing)
                    _cards[i].gameObject.SetActive(hasListing);
                if (!hasListing)
                    continue;

                TMP_Text label = Text(_cards[i].gameObject, "道具名称");
                if (label != null)
                    label.text = _listings[i].DisplayName;
                ApplyPriceBadge(_cards[i].gameObject, _listings[i]);
            }
        }

        /// <summary>
        /// 1 个模板 + 运行时生成（MODULE.md §7）。
        ///
        /// 容器 `Product page/展示卡牌Scroll View/Viewport/Content` 自带
        /// `GridLayoutGroup(cell 214.2×285.6, 固定列数 2)`，克隆体会被自动排布，
        /// 因此本方式**不改变既有美术与布局尺寸**——模板的 rect 沿用被替换实例的 rect。
        ///
        /// 模板自身始终禁用，只作克隆源；超出的实例保持隐藏而非销毁，便于后续数据增多时复用。
        /// </summary>
        private void ApplyCardsFromTemplate()
        {
            _cardTemplate.SetActive(false);

            while (_cardInstances.Count < _listings.Count)
            {
                GameObject clone = Instantiate(_cardTemplate, _cardListRoot);
                clone.name = "道具卡牌Item";
                clone.SetActive(false);

                Button button = clone.GetComponent<Button>();
                if (button == null)
                {
                    Debug.LogWarning("[Client] 商城卡牌模板上没有 Button，无法生成可点击卡牌。");
                    Destroy(clone);
                    break;
                }
                int index = _cardInstances.Count;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OpenListing(index));
                _cardInstances.Add(button);
            }

            if (_cardInstances.Count != _listings.Count)
                Debug.Log("[Client] 商城卡牌实例(" + _cardInstances.Count + ") 与商品数(" + _listings.Count +
                          ") 不一致：超出部分隐藏。");

            for (int i = 0; i < _cardInstances.Count; i++)
            {
                bool exists = i < _listings.Count;
                if (_cardInstances[i].gameObject.activeSelf != exists)
                    _cardInstances[i].gameObject.SetActive(exists);
                if (!exists)
                    continue;

                TMP_Text label = Text(_cardInstances[i].gameObject, "道具名称");
                if (label != null)
                    label.text = _listings[i].DisplayName;
                ApplyPriceBadge(_cardInstances[i].gameObject, _listings[i]);
            }
        }

        /// <summary>由组合根注入；取代原先的 ClientUiFeedback 静态单例。</summary>
        public void BindFeedback(IClientFeedback feedback) { _feedback = feedback; }

        /// <summary>由导航内核注入；取代原先的 FindObjectOfType&lt;ClientUiNavigator&gt;。</summary>
        public void BindNavigation(IClientNavigation navigation) { _navigation = navigation; }

        private void Toast(string message)
        {
            if (_feedback != null)
                _feedback.ShowToast(message);
            else
                Debug.Log("[Client] " + message);
        }

        protected override void OnPageRefresh() { Show(); }
        // 离开商城时必须收起购买/成功/兑换等内嵌弹窗，避免遗留遮罩与输入阻断。
        protected override void OnPageHidden() { ClosePopups(); }

        private void Awake() { BindExistingGui(); }
        private void OnEnable() { if (ClientServices.Data != null) ClientServices.Data.DataChanged += Refresh; }
        private void OnDisable() { if (ClientServices.Data != null) ClientServices.Data.DataChanged -= Refresh; }

        // 层级由 ClientPopupService 统一决定（Open 时 SetAsLastSibling）。
        //
        // ⚠️ 原实现此处调用 BringToFront(gameObject) → SetAsLastSibling()，
        // 会把商店强行提到 PopupLayer 最后一位＝最上层，从而**压住其后打开的购买界面与成功页**。
        // 这是"整屏页面替换"时期的遗留：那时一个界面独占屏幕，谁显示谁置顶；
        // 但在**弹窗栈**语义下，下层必须留在上层之下。实测现象：
        //   打开购买界面与成功页后，三层都被商店盖住（负责人："层级全部跑到商店后面"）。
        // 定位依据：栈快照显示兄弟序号本已正确（商店16 < 购买17 < 成功18），
        // 却仍被商店压住 → 说明另有代码改写了层级，即本行。
        public void Show() {
            // 2026-09-21（负责人决定：其余列表页也接入）：竖向列表撑高 / Clamped / 顶部对齐。
            ClientScrollFix.FixAll(gameObject, true); gameObject.SetActive(true); ShowRegularMall(); Refresh(); }
        public void ShowRegularMall() { Set(_regular, true); Set(_activity, false); ClosePopups(); }
        public void ShowActivityMall() { Set(_regular, false); Set(_activity, true); ClosePopups(); }
        public void OpenListing(int index)
        {
            RefreshListings();
            if (index < 0 || index >= _listings.Count) return;
            _selected = _listings[index]; Set(_purchase, true); RefreshPurchase();
        }
        public void CancelPurchase() { ResetPurchase(); Set(_purchase, false); }
        public void ConfirmPurchase()
        {
            if (_selected == null || _quantity == null) return;
            string reason;
            if (!ClientServices.Data.TryPurchaseShopListing(_selected.ListingId, _quantity.Quantity, out reason)) { Toast(reason); return; }
            // 高清商品图替换和角色背包入库，待素材、角色道具契约确认后接入。
            //
            // 购买界面**不关闭**，只把成功页压在它上面：负责人要求"成功页后面显示购买界面、
            // 再后面是商店"，三层层层覆盖。
            // 原实现先 Set(_purchase,false) 再开成功页，成功页身后就只剩商店。
            // 现改为仅打开成功页——弹窗服务会自动把下层的购买界面挂起（保留可见、停止交互），
            // 关闭成功页时再 Resume 回来，正好回到购买界面。
            Set(_success, true);
            ResetPurchase();
        }
        public void CloseSuccessPage() { Set(_success, false); }
        public void OpenExchangeCode() { Set(_exchange, true); FocusExchangeInput(); }
        public void CloseExchangeCode() { if (_exchangeInput != null) _exchangeInput.text = string.Empty; Set(_exchange, false); }
        public void SubmitExchangeCode() { Toast("兑换码服务待接入"); }
        public void OpenOrderHistory() { Set(_orders, true); }
        public void OpenPendingShipment() { Set(_shipment, true); }
        public void CloseOrderHistory() { Set(_orders, false); }
        public void ClosePendingShipment() { Set(_shipment, false); }
        public void Back()
        {
            if (Active(_purchase)) { CancelPurchase(); return; } if (Active(_exchange)) { CloseExchangeCode(); return; }
            if (Active(_success)) { CloseSuccessPage(); return; } if (Active(_orders)) { CloseOrderHistory(); return; }
            if (Active(_shipment)) { ClosePendingShipment(); return; }
            if (_navigation != null) _navigation.ReturnHome(); else gameObject.SetActive(false);
        }

        // 商城主页面的后退必须离开商城；其它弹窗的后退仍走 Back() 逐层关闭。
        private void ReturnHomeFromShop()
        {
            LogNavigationDiagnostic("商城后退 onClick 已触发；" + DescribeNavigationState());
            ResetPurchase();
            ClosePopups();
            // 页面可见性只由导航内核负责。商城不再自行 SetActive 主页，
            // 否则会重新引入"两个所有者同时改写可见性"的结构问题。
            if (_navigation != null)
                _navigation.ReturnHome();
            else
                gameObject.SetActive(false);

            LogNavigationDiagnostic("商城后退处理完成；" + DescribeNavigationState());
        }

        private void BindExistingGui()
        {
            _regular = Find("Product page"); _activity = Find("Activity Mall Page"); _bottomFunctionBar = Find("Bottom page function bar");
            // 这 5 个已迁到 PopupLayer：优先用序列化引用，保留 Find 作为回退（例如未来又移回页面内）。
            _purchase = _purchasePopup != null ? _purchasePopup : Find("Product Purchase Interface");
            _success = _successPopup != null ? _successPopup : Find("Purchase Success Page");
            _exchange = _exchangePopup != null ? _exchangePopup : Find("Exchange code interface");
            _orders = _ordersPopup != null ? _ordersPopup : Find("Historical Order Interface");
            _shipment = _shipmentPopup != null ? _shipmentPopup : Find("Pending shipmentCanvas");
            _quantity = _purchase == null ? null : _purchase.GetComponentInChildren<ClientShopPurchaseQuantityProgress>(true);
            if (_quantity != null) _quantity.QuantityChanged += UpdateCost;
            _costText = Text(_purchase, "CSText (TMP)");
            _crystalText = Text(Find("Regular store resource exchange - Crystal stonesCanvas"), "CSText (TMP)") ?? Text(Find("Regular store resource exchange - Crystal stones"), "CSText (TMP)");
            _limitText = Text(Find("Prop Description"), "限购数量") ?? Text(Find("Prop Description"), "限购");
            // 卡牌节点实际名为 道具卡牌prefab（后缀 (1)(2)...），原来找的 "展示卡牌Button" 在场景中不存在，
            // 加上卡牌本身没有任何 Button，导致道具卡完全点不开购买界面。
            // 现改为显式收集 + 下标映射（见 BindCards），不再用一把梭的 BindPrefix。
            BindCards(); BindShopHomeBack(); Bind(_purchase, "购买键Button", ConfirmPurchase); Bind(_purchase, "取消键Button", CancelPurchase);
            Bind(_success, "点击空白处关闭", CloseSuccessPage); Bind(_exchange, "修改Button", FocusExchangeInput); Bind(_exchange, "确认键Button", SubmitExchangeCode); Bind(_exchange, "取消键Button", CloseExchangeCode); Bind(_exchange, "BackoffButton", CloseExchangeCode);
            Bind(_orders, "BackoffButton", CloseOrderHistory); Bind(_shipment, "BackoffButton", ClosePendingShipment); Bind(gameObject, "Exchange code", OpenExchangeCode); Bind(gameObject, "Order", OpenOrderHistory); Bind(gameObject, "Pending shipment", OpenPendingShipment); Bind(gameObject, "Regular StoreButton", ShowRegularMall); Bind(gameObject, "Event MallButton", ShowActivityMall); BindAll(gameObject, "后退Button", Back);
            BindSuccessDismissHandlers();
            GameObject inputRoot = _exchange == null ? null : Find(_exchange.transform, "Input of redemption code"); TMP_Text text = Text(_exchange, "Text (TMP)");
            if (inputRoot != null && text != null)
            {
                _exchangeInput = inputRoot.GetComponent<TMP_InputField>() ?? inputRoot.AddComponent<TMP_InputField>();
                _exchangeInput.textComponent = text;
                _exchangeInput.targetGraphic = inputRoot.GetComponent<Graphic>() ?? inputRoot.GetComponentInChildren<Graphic>(true);
                Bind(inputRoot, "Image", FocusExchangeInput);
                BindExchangeFocusHandlers(inputRoot);
            }
        }
        private void Refresh()
        {
            if (ClientServices.Data == null) return;
            RefreshListings();
            if (_crystalText != null) _crystalText.text = ClientServices.Data.GetWallet().GetBalance(ClientCurrencyType.Diamond).ToString(); if (_selected != null) RefreshPurchase();
        }
        private void RefreshListings()
        {
            if (ClientServices.Data == null) return;
            _listings.Clear();
            foreach (ClientShopListing listing in ClientServices.Data.GetShopListings())
            {
                if (listing.UnitCrystalStoneCost <= 0) listing.UnitCrystalStoneCost = DefaultUnitCrystalStoneCost;
                _listings.Add(listing);
            }
            ApplyCards();
        }
        private void RefreshPurchase()
        {
            if (_selected == null || ClientServices.Data == null) return; int bought = ClientServices.Data.GetShopPurchasedQuantity(_selected.ListingId); int remaining = Mathf.Max(0, _selected.PurchaseLimit - bought);
            if (_quantity != null) _quantity.ConfigureQuantity(remaining); if (_limitText != null) _limitText.text = "限购 " + remaining + "/" + _selected.PurchaseLimit; UpdateCost(0);
        }
        private void UpdateCost(int number) { if (_costText != null) _costText.text = ((_selected == null ? DefaultUnitCrystalStoneCost : _selected.UnitCrystalStoneCost) * number).ToString(); }
        private void ResetPurchase() { if (_quantity != null) _quantity.ConfigureQuantity(0); UpdateCost(0); _selected = null; }
        private void ClosePopups() { Set(_purchase, false); Set(_success, false); Set(_exchange, false); Set(_orders, false); Set(_shipment, false); }
        private void FocusExchangeInput() { if (_exchangeInput != null) { _exchangeInput.ActivateInputField(); _exchangeInput.Select(); } }
        private GameObject Find(string name) { return Find(transform, name); }
        private static GameObject Find(Transform root, string name) { if (root == null) return null; foreach (Transform item in root.GetComponentsInChildren<Transform>(true)) if (item.name == name) return item.gameObject; return null; }
        private static TMP_Text Text(GameObject root, string name) { GameObject item = root == null ? null : Find(root.transform, name); return item == null ? null : item.GetComponent<TMP_Text>(); }

        /// <summary>
        /// 商品卡面写入**售价**（负责人 2026-09-20 明确：商店卡面那个数字 = 售价，不是拥有数）。
        ///
        /// 节点依据（直接解析 `Assets/Client/UI/Prefabs/道具卡牌prefab.prefab`，该卡是 prefab 实例）：
        /// 卡上**只有两个文本节点** —— `道具名称` 与 `道具数量`，且两者的设计时 `m_text` 就是
        /// **各自的节点名**（典型占位符，运行时必须被覆盖）。
        /// 左下角锚点 (0,0) 的 `数量角标` 与 `数量黑底` **都是纯 Image、没有文本**
        /// —— 写它们不会有任何显示，必须写 `道具数量`。
        ///
        /// ⚠️ 语义由**页面**决定：同一张卡在奖励 / 邮件场景下 `道具数量` 表示"数量"
        /// （`ClientActivityPage` 绑的就是它）。本次**只接商店**卡面。
        /// 容错：`Find` 是深搜（运行时能穿透 prefab 实例），节点缺失则静默跳过。
        /// </summary>
        private static void ApplyPriceBadge(GameObject card, ClientShopListing listing)
        {
            if (card == null || listing == null)
                return;

            TMP_Text price = Text(card, "道具数量");
            if (price != null)
                price.text = listing.UnitCrystalStoneCost.ToString();
        }
        private void Set(GameObject item, bool state)
        {
            if (item == null) return;

            // 迁到 PopupLayer 的弹窗统一走弹窗服务：栈顺序、遮罩、输入阻断与关闭回调
            // 都由它负责；页面若再直接 SetActive 就会形成"两个所有者"。
            if (_popups != null)
            {
                ClientUiPopup popup = item.GetComponent<ClientUiPopup>();
                if (popup != null)
                {
                    if (state) _popups.Open(popup);
                    else _popups.Close(popup);
                    return;
                }
            }

            item.SetActive(state);
            if (state) BringToFront(item);
        }
        private static void BringToFront(GameObject item)
        {
            if (item == null) return;
            if (item.transform.parent != null) item.transform.SetAsLastSibling();

            // ⚠️ 原实现在此把 item 的 Canvas 设为 overrideSorting=true 并递增 sortingOrder：
            //
            //     Canvas canvas = item.GetComponent<Canvas>();
            //     canvas.overrideSorting = true;
            //     canvas.sortingOrder = ++_nextCanvasSortingOrder;
            //
            // 后果：`overrideSorting=true` 的 Canvas **完全无视层级顺序**，只按 sortingOrder 排序。
            // 而 Set() 会对商店内部的 Product page / Activity Mall Page 调用本方法，
            // 于是这两个**商店内部容器**被抬到 order 1001..1005，**永远渲染在所有弹窗之上** ——
            // 商品购买界面与购买成功页因此被商店压住。
            //
            // 实测证据（栈快照）：
            //   Canvas[.../ShopPage商店/Product page override=True order=1005]
            //   Canvas[.../ShopPage商店/Bottom page function bar/Event Mall/Activity Mall Page override=True order=1002]
            // 这也解释了为何按兄弟序号做的一切修复（SetAsLastSibling、整体重排栈）都无效：
            // overrideSorting 打开时，兄弟序号根本不参与排序。
            //
            // 这两个节点是商店**内部**的分页容器，只该在同级内调整顺序，不得越出弹窗栈；
            // 弹窗之间的层级统一由 ClientPopupService.ApplyStackOrder 决定。
            // 因此这里只保留同级置后。
        }
        private static bool Active(GameObject item) { return item != null && item.activeSelf; }
        private static void Bind(GameObject root, string name, UnityEngine.Events.UnityAction call) { GameObject item = root == null ? null : (root.name == name ? root : Find(root.transform, name)); Button button = item == null ? null : item.GetComponent<Button>(); if (button != null) { button.onClick.RemoveListener(call); button.onClick.AddListener(call); } }
        private static void BindAll(GameObject root, string name, UnityEngine.Events.UnityAction call) { if (root == null) return; foreach (Button button in root.GetComponentsInChildren<Button>(true)) if (button.name == name) { button.onClick.RemoveListener(call); button.onClick.AddListener(call); } }
        private static void BindPrefix(GameObject root, string prefix, Action<int> call) { if (root == null) return; int index = 0; foreach (Button button in root.GetComponentsInChildren<Button>(true)) if (button.name.StartsWith(prefix, StringComparison.Ordinal)) { int id = index++; button.onClick.AddListener(() => call(id)); } }
        private void BindShopHomeBack()
        {
            // 实际商城主返回位于 Bottom page function bar/BackoffButton，不在 Product page 内。
            GameObject root = _bottomFunctionBar;
            Button button = root == null ? null : Find(root.transform, "BackoffButton")?.GetComponent<Button>();
            if (button == null)
            {
                if (enableNavigationDiagnostics) Debug.LogWarning("[ClientShop导航诊断] 未找到商城主返回按钮 Bottom page function bar/BackoffButton。", this);
                return;
            }
            button.onClick.RemoveListener(ReturnHomeFromShop);
            button.onClick.AddListener(ReturnHomeFromShop);
            ClientShopNavigationDiagnostics diagnostics = button.GetComponent<ClientShopNavigationDiagnostics>() ?? button.gameObject.AddComponent<ClientShopNavigationDiagnostics>();
            diagnostics.Configure(DescribeNavigationState, enableNavigationDiagnostics);
            LogNavigationDiagnostic("已绑定商城主返回按钮 Bottom page function bar/BackoffButton；" + DescribeNavigationState());
        }
        private string DescribeNavigationState()
        {
            return $"shopSelf={gameObject.activeSelf}, shopHierarchy={gameObject.activeInHierarchy}, regular={Active(_regular)}, nav={(_navigation == null ? "missing" : _navigation.CurrentPage.ToString())}";
        }
        private void LogNavigationDiagnostic(string message)
        {
            if (enableNavigationDiagnostics) Debug.Log("[ClientShop导航诊断] " + message, this);
        }
        private void BindSuccessDismissHandlers()
        {
            if (_success == null) return;
            foreach (Graphic graphic in _success.GetComponentsInChildren<Graphic>(true))
            {
                // 负责人要求："点击任何地方直接退回购买界面"。
                // 这些 Graphic 在前一轮"射线解耦"中被当成装饰关闭了 raycastTarget，
                // 于是挂上处理器也收不到点击。这里显式打开，让整块成功页都能接收点击。
                if (!graphic.raycastTarget)
                    graphic.raycastTarget = true;

                ClientShopCloseOnClick handler = graphic.GetComponent<ClientShopCloseOnClick>();
                if (handler == null) handler = graphic.gameObject.AddComponent<ClientShopCloseOnClick>();
                handler.Configure(CloseSuccessPage);
            }
        }
        private void BindExchangeFocusHandlers(GameObject inputRoot)
        {
            foreach (Graphic graphic in inputRoot.GetComponentsInChildren<Graphic>(true))
            {
                ClientShopFocusOnClick handler = graphic.GetComponent<ClientShopFocusOnClick>();
                if (handler == null) handler = graphic.gameObject.AddComponent<ClientShopFocusOnClick>();
                handler.Configure(FocusExchangeInput);
            }
        }
    }
}
