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
    public sealed class ClientShopPage : MonoBehaviour
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

        private void Awake() { BindExistingGui(); }
        private void OnEnable() { if (ClientServices.Data != null) ClientServices.Data.DataChanged += Refresh; }
        private void OnDisable() { if (ClientServices.Data != null) ClientServices.Data.DataChanged -= Refresh; }

        public void Show() { gameObject.SetActive(true); BringToFront(gameObject); ShowRegularMall(); Refresh(); }
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
            if (!ClientServices.Data.TryPurchaseShopListing(_selected.ListingId, _quantity.Quantity, out reason)) { ClientUiFeedback.ShowToast(reason); return; }
            // 高清商品图替换和角色背包入库，待素材、角色道具契约确认后接入。
            Set(_purchase, false); Set(_success, true); ResetPurchase();
        }
        public void CloseSuccessPage() { Set(_success, false); }
        public void OpenExchangeCode() { Set(_exchange, true); FocusExchangeInput(); }
        public void CloseExchangeCode() { if (_exchangeInput != null) _exchangeInput.text = string.Empty; Set(_exchange, false); }
        public void SubmitExchangeCode() { ClientUiFeedback.ShowToast("兑换码服务待接入"); }
        public void OpenOrderHistory() { Set(_orders, true); }
        public void OpenPendingShipment() { Set(_shipment, true); }
        public void CloseOrderHistory() { Set(_orders, false); }
        public void ClosePendingShipment() { Set(_shipment, false); }
        public void Back()
        {
            if (Active(_purchase)) { CancelPurchase(); return; } if (Active(_exchange)) { CloseExchangeCode(); return; }
            if (Active(_success)) { CloseSuccessPage(); return; } if (Active(_orders)) { CloseOrderHistory(); return; }
            if (Active(_shipment)) { ClosePendingShipment(); return; }
            ClientUiNavigator nav = UnityEngine.Object.FindObjectOfType<ClientUiNavigator>(true);
            if (nav != null) nav.ReturnHome(); else gameObject.SetActive(false);
        }

        // 商城主页面的后退必须离开商城；其它弹窗的后退仍走 Back() 逐层关闭。
        private void ReturnHomeFromShop()
        {
            LogNavigationDiagnostic("商城后退 onClick 已触发；" + DescribeNavigationState());
            ResetPurchase();
            ClosePopups();
            ClientUiNavigator nav = UnityEngine.Object.FindObjectOfType<ClientUiNavigator>(true);
            if (nav != null) nav.ReturnHome();

            // 导航器未登记或当前页状态丢失时，仍确保商城被关闭并恢复主页。
            gameObject.SetActive(false);
            ClientHomePage home = UnityEngine.Object.FindObjectOfType<ClientHomePage>(true);
            if (home != null) home.gameObject.SetActive(true);
            LogNavigationDiagnostic("商城后退处理完成；" + DescribeNavigationState());
        }

        private void BindExistingGui()
        {
            _regular = Find("Product page"); _activity = Find("Activity Mall Page"); _purchase = Find("Product Purchase Interface"); _bottomFunctionBar = Find("Bottom page function bar");
            _success = Find("Purchase Success Page"); _exchange = Find("Exchange code interface"); _orders = Find("Historical Order Interface"); _shipment = Find("Pending shipmentCanvas");
            _quantity = _purchase == null ? null : _purchase.GetComponentInChildren<ClientShopPurchaseQuantityProgress>(true);
            if (_quantity != null) _quantity.QuantityChanged += UpdateCost;
            _costText = Text(_purchase, "CSText (TMP)");
            _crystalText = Text(Find("Regular store resource exchange - Crystal stonesCanvas"), "CSText (TMP)") ?? Text(Find("Regular store resource exchange - Crystal stones"), "CSText (TMP)");
            _limitText = Text(Find("Prop Description"), "限购数量") ?? Text(Find("Prop Description"), "限购");
            BindPrefix(_regular, "展示卡牌Button", OpenListing); BindShopHomeBack(); Bind(_purchase, "购买键Button", ConfirmPurchase); Bind(_purchase, "取消键Button", CancelPurchase);
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
        private static void Set(GameObject item, bool state) { if (item == null) return; item.SetActive(state); if (state) BringToFront(item); }
        private static void BringToFront(GameObject item)
        {
            if (item == null) return;
            if (item.transform.parent != null) item.transform.SetAsLastSibling();

            // 展示卡牌 Scroll View 使用独立 Canvas 时，同级顺序不会影响它。
            // 只提升当前页面根 Canvas，避免重写卡牌自身的视觉层级。
            Canvas canvas = item.GetComponent<Canvas>();
            if (canvas == null) return;
            canvas.overrideSorting = true;
            canvas.sortingOrder = ++_nextCanvasSortingOrder;
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
            ClientUiNavigator nav = UnityEngine.Object.FindObjectOfType<ClientUiNavigator>(true);
            ClientHomePage home = UnityEngine.Object.FindObjectOfType<ClientHomePage>(true);
            return $"shopSelf={gameObject.activeSelf}, shopHierarchy={gameObject.activeInHierarchy}, regular={Active(_regular)}, nav={(nav == null ? "missing" : "found")}, home={(home == null ? "missing" : home.gameObject.activeInHierarchy.ToString())}";
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
