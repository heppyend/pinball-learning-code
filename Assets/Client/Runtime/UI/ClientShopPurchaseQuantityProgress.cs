using System;
using Pinball.Client.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 商城购买数量的离散进度条。
    /// 将本组件挂到“Progress bar icon”，并在 Inspector 或 ConfigureReferences 中绑定同一购买区的节点。
    /// </summary>
    public sealed class ClientShopPurchaseQuantityProgress : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform _progressBar;
        [SerializeField] private RectTransform _progressBarIcon;
        [SerializeField] private Image _progressFill;
        [SerializeField] private TMP_Text _progressBarIconText;
        [SerializeField] private TMP_Text _minimumText;
        [SerializeField] private TMP_Text _maximumText;
        [SerializeField] private Button _subtractButton;
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _maxButton;

        private int _maximumQuantity = 1;
        private int _quantity;
        private bool _isDragging;

        public int Quantity { get { return _quantity; } }
        public int MaximumQuantity { get { return _maximumQuantity; } }
        public float NormalizedQuantity { get { return _maximumQuantity > 0 ? (float)_quantity / _maximumQuantity : 0f; } }

        /// <summary>数量变化时触发；返回值始终是 0 到 MaximumQuantity 间的整数挡位。</summary>
        public event Action<int> QuantityChanged;

        private void Awake()
        {
            if (_progressBarIcon == null)
                _progressBarIcon = transform as RectTransform;
            BindButtons();
            RefreshVisuals();
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        /// <summary>
        /// 绑定现有购买区域节点。不会创建或调整任何 UI 节点。
        /// addButton 可为空；未提供时只支持减、最大值和拖动。
        /// </summary>
        public void ConfigureReferences(
            RectTransform progressBar,
            RectTransform progressBarIcon,
            Image progressFill,
            TMP_Text progressBarIconText,
            TMP_Text minimumText,
            TMP_Text maximumText,
            Button subtractButton,
            Button addButton,
            Button maxButton)
        {
            UnbindButtons();
            _progressBar = progressBar;
            _progressBarIcon = progressBarIcon;
            _progressFill = progressFill;
            _progressBarIconText = progressBarIconText;
            _minimumText = minimumText;
            _maximumText = maximumText;
            _subtractButton = subtractButton;
            _addButton = addButton;
            _maxButton = maxButton;
            BindButtons();
            RefreshVisuals();
        }

        /// <summary>
        /// 在点击商品后调用。maximumQuantity 应传该商品本次可选的最大购买数；例如限购 6 时传 6。
        /// initialQuantity 默认从 0 开始，超出范围的值会自动收敛到有效挡位。
        /// </summary>
        public void ConfigureQuantity(int maximumQuantity, int initialQuantity = 0)
        {
            _maximumQuantity = Mathf.Max(0, maximumQuantity);
            SetQuantity(initialQuantity, false);
            RefreshVisuals();
        }

        /// <summary>商城条目适配入口：使用剩余限购数作为最大挡位。</summary>
        public void ConfigureForListing(ClientShopListing listing, int initialQuantity = 0)
        {
            ConfigureQuantity(listing == null ? 0 : listing.PurchaseLimit, initialQuantity);
        }

        public void Increase()
        {
            SetQuantity(_quantity + 1);
        }

        public void Decrease()
        {
            SetQuantity(_quantity - 1);
        }

        public void SetMaximum()
        {
            SetQuantity(_maximumQuantity);
        }

        /// <summary>直接切换到指定整数挡位；所有输入均会限制在 0 到 MaximumQuantity。</summary>
        public void SetQuantity(int quantity)
        {
            SetQuantity(quantity, true);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            SetQuantityFromPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging)
                SetQuantityFromPointer(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging)
                SetQuantityFromPointer(eventData);
            _isDragging = false;
        }

        private void SetQuantityFromPointer(PointerEventData eventData)
        {
            if (_progressBar == null || _maximumQuantity <= 0)
                return;

            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _progressBar, eventData.position, eventData.pressEventCamera, out localPoint))
                return;

            float normalized = Mathf.InverseLerp(_progressBar.rect.xMin, _progressBar.rect.xMax, localPoint.x);
            SetQuantity(Mathf.RoundToInt(normalized * _maximumQuantity));
        }

        private void SetQuantity(int quantity, bool notify)
        {
            int clampedQuantity = Mathf.Clamp(quantity, 0, _maximumQuantity);
            bool changed = _quantity != clampedQuantity;
            _quantity = clampedQuantity;
            RefreshVisuals();
            if (changed && notify && QuantityChanged != null)
                QuantityChanged(_quantity);
        }

        private void RefreshVisuals()
        {
            float normalized = NormalizedQuantity;

            if (_progressFill != null)
            {
                _progressFill.type = Image.Type.Filled;
                _progressFill.fillMethod = Image.FillMethod.Horizontal;
                _progressFill.fillOrigin = 0;
                _progressFill.fillAmount = normalized;
            }

            if (_progressBarIcon != null && _progressBar != null)
            {
                Vector3 trackPosition = _progressBar.TransformPoint(new Vector3(
                    Mathf.Lerp(_progressBar.rect.xMin, _progressBar.rect.xMax, normalized),
                    0f,
                    0f));
                _progressBarIcon.position = new Vector3(trackPosition.x, _progressBarIcon.position.y, _progressBarIcon.position.z);
            }

            if (_progressBarIconText != null)
                _progressBarIconText.text = _quantity.ToString();
            if (_minimumText != null)
                _minimumText.text = "0";
            if (_maximumText != null)
                _maximumText.text = _maximumQuantity.ToString();
            if (_subtractButton != null)
                _subtractButton.interactable = _quantity > 0;
            if (_addButton != null)
                _addButton.interactable = _quantity < _maximumQuantity;
            if (_maxButton != null)
                _maxButton.interactable = _quantity < _maximumQuantity;
        }

        private void BindButtons()
        {
            if (_subtractButton != null) _subtractButton.onClick.AddListener(Decrease);
            if (_addButton != null) _addButton.onClick.AddListener(Increase);
            if (_maxButton != null) _maxButton.onClick.AddListener(SetMaximum);
        }

        private void UnbindButtons()
        {
            if (_subtractButton != null) _subtractButton.onClick.RemoveListener(Decrease);
            if (_addButton != null) _addButton.onClick.RemoveListener(Increase);
            if (_maxButton != null) _maxButton.onClick.RemoveListener(SetMaximum);
        }
    }
}
