using System;
using System.Collections;
using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public enum ClientCompleteGuideEventImageSlot
    {
        Background,
        HeaderBackground,
        HeaderCharacter,
        Title,
        PeriodFrame,
        PeriodPanel,
        PeriodNumber,
        PeriodUnit,
        CollectionCard,
        RulesTextFrame,
        RulesTitle,
        RulesDecoration,
        ProgressPanel,
        ProgressTrack,
        ProgressBar,
        ClaimButton,
        ClaimLabel,
        ClaimedIcon,
        DateFrame,
        DateLabel,
        HelpButton,
        BottomBar,
        BackButton,
    }

    public enum ClientCompleteGuideEventTextSlot
    {
        Collection,
        Rules,
        ProgressCurrent,
        ProgressSeparator,
        ProgressTarget,
        Date,
    }

    /// <summary>绑定现有 Complete Guide Event Page 全图鉴活动节点；不生成、删除或重排 GUI。</summary>
    public sealed class ClientCompleteGuideEventPage : ClientPageViewBase
    {
        private const string ExpectedRootName = "Complete Guide Event Page全图鉴活动";

        private sealed class TextBinding
        {
            public TMP_Text Tmp;
            public Text Legacy;

            public string Value
            {
                set
                {
                    if (Tmp != null) Tmp.text = value ?? string.Empty;
                    if (Legacy != null) Legacy.text = value ?? string.Empty;
                }
            }

            public void BringToFront()
            {
                if (Tmp != null) Tmp.transform.SetAsLastSibling();
                else if (Legacy != null) Legacy.transform.SetAsLastSibling();
            }
        }

        public event Action<ClientCompleteGuideEventData> ClaimStarted;
        public event Action<ClientCompleteGuideEventData, ClientActivityTaskClaimResult> ClaimFinished;
        public event Action<ClientCompleteGuideEventData, string> ClaimFailed;
        public event Action<string> TipRequested;
        public event Action<bool> RulesVisibilityChanged;

        private readonly Dictionary<ClientCompleteGuideEventImageSlot, Image> _images =
            new Dictionary<ClientCompleteGuideEventImageSlot, Image>();
        private readonly Dictionary<ClientCompleteGuideEventTextSlot, TextBinding> _texts =
            new Dictionary<ClientCompleteGuideEventTextSlot, TextBinding>();
        private readonly Dictionary<ClientCompleteGuideEventImageSlot, int> _imageIds =
            new Dictionary<ClientCompleteGuideEventImageSlot, int>();
        private readonly Dictionary<int, Sprite> _registeredSprites = new Dictionary<int, Sprite>();

        private IClientActivitySpriteResolver _spriteResolver;
        private ClientActivityPage _activityPage;
        private ClientUiNavigator _navigator;
        private ClientCompleteGuideEventData _data;
        private GameObject _rulesPopup;
        private RectTransform _progressTrack;
        private RectTransform _progressBar;
        private Button _claimButton;
        private Animator _claimAnimator;
        private GameObject _claimedIcon;
        private Sprite _pendingButtonSprite;
        private Sprite _pendingLabelSprite;
        private Sprite _claimableButtonSprite;
        private Sprite _claimableLabelSprite;
        private Coroutine _claimSpriteCaptureRoutine;
        private float _fullProgressWidth;
        private bool _isBound;
        private bool _dataSubscribed;
        private bool _initialized;

        // 本页根节点即自身（Awake 中已按场景约定自我隐藏），故只需覆写刷新与隐藏。
        protected override void OnPageRefresh() { Show(); }
        protected override void OnPageHidden() { Hide(); }

        private void Awake()
        {
            if (gameObject.name != ExpectedRootName)
            {
                enabled = false;
                return;
            }
            EnsureBound();
            SetRulesVisible(false);
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (!_initialized) return;
            SubscribeData();
            Refresh();
            BeginClaimSpriteCapture();
        }

        private void OnDisable()
        {
            UnsubscribeData();
            _claimSpriteCaptureRoutine = null;
        }

        private void OnDestroy()
        {
            UnsubscribeData();
        }

        public void Initialize(ClientUiNavigator navigator, ClientActivityPage activityPage)
        {
            EnsureBound();
            _navigator = navigator;
            _activityPage = activityPage;
            _initialized = true;
        }

        public void Show()
        {
            // 2026-09-21（负责人决定：其余列表页也接入）：竖向列表撑高 / Clamped / 顶部对齐。
            ClientScrollFix.FixAll(gameObject, true);
            ClientCompleteGuideEventData data = ClientServices.IsInitialized
                ? ClientServices.Data.GetCompleteGuideEvent()
                : null;
            Show(data);
        }

        public void Show(ClientCompleteGuideEventData data)
        {
            EnsureBound();
            _data = data;
            SetRulesVisible(false);
            gameObject.SetActive(true);
            SubscribeData();
            ApplyData(data);
            BeginClaimSpriteCapture();
        }

        public void Hide()
        {
            SetRulesVisible(false);
            gameObject.SetActive(false);
        }

        public void Back()
        {
            if (_navigator != null)
            {
                _navigator.Back();
                if (_activityPage != null) _activityPage.ShowSeasonalTasks();
                return;
            }
            Hide();
            if (_activityPage != null)
            {
                _activityPage.gameObject.SetActive(true);
                _activityPage.ShowSeasonalTasks();
            }
        }

        public void ShowRules() { SetRulesVisible(true); }
        public void HideRules() { SetRulesVisible(false); }

        public void SetRulesVisible(bool visible)
        {
            if (_rulesPopup != null) _rulesPopup.SetActive(visible);
            RulesVisibilityChanged?.Invoke(visible);
        }

        public void SetSpriteResolver(IClientActivitySpriteResolver resolver)
        {
            _spriteResolver = resolver;
            foreach (KeyValuePair<ClientCompleteGuideEventImageSlot, int> item in _imageIds)
                ApplyImage(item.Key, item.Value);
        }

        public void RegisterSprite(int imageId, Sprite sprite)
        {
            if (imageId <= 0 || sprite == null) return;
            _registeredSprites[imageId] = sprite;
            foreach (KeyValuePair<ClientCompleteGuideEventImageSlot, int> item in _imageIds)
                if (item.Value == imageId) ApplyImage(item.Key, imageId);
        }

        public void SetImage(ClientCompleteGuideEventImageSlot slot, int imageId)
        {
            _imageIds[slot] = imageId;
            ApplyImage(slot, imageId);
        }

        public void SetText(ClientCompleteGuideEventTextSlot slot, string value)
        {
            TextBinding binding;
            if (_texts.TryGetValue(slot, out binding) && binding != null)
                binding.Value = value;
        }

        public void SetProgress(int progress, int target)
        {
            int safeTarget = Mathf.Max(0, target);
            int safeProgress = Mathf.Clamp(progress, 0, safeTarget);
            SetText(ClientCompleteGuideEventTextSlot.ProgressCurrent, safeProgress.ToString());
            SetText(ClientCompleteGuideEventTextSlot.ProgressSeparator, "/");
            SetText(ClientCompleteGuideEventTextSlot.ProgressTarget, safeTarget.ToString());

            float ratio = safeTarget > 0 ? (float)safeProgress / safeTarget : 0f;
            if (_progressBar != null)
            {
                _progressBar.gameObject.SetActive(ratio > 0f);
                if (ratio > 0f)
                {
                    _progressBar.anchorMin = new Vector2(0f, 0.5f);
                    _progressBar.anchorMax = new Vector2(0f, 0.5f);
                    _progressBar.pivot = new Vector2(0f, 0.5f);
                    _progressBar.anchoredPosition = new Vector2(5f, 0f);
                    _progressBar.SetSizeWithCurrentAnchors(
                        RectTransform.Axis.Horizontal, Mathf.Max(0f, _fullProgressWidth * ratio));
                    Image fillImage = _progressBar.GetComponent<Image>();
                    if (fillImage != null && fillImage.sprite != null && fillImage.sprite.border.sqrMagnitude > 0f)
                        fillImage.type = Image.Type.Sliced;
                }
            }
            BringProgressTextToFront();

            if (_data != null)
            {
                _data.Progress = safeProgress;
                _data.Target = safeTarget;
                ApplyClaimState(_data);
            }
        }

        public void SetClaimImages(int pendingLabelImageId, int claimLabelImageId, int claimableButtonImageId)
        {
            if (_data == null) return;
            _data.PendingLabelImageId = pendingLabelImageId;
            _data.ClaimLabelImageId = claimLabelImageId;
            _data.ClaimableButtonImageId = claimableButtonImageId;
            ApplyClaimState(_data);
        }

        public void SetData(ClientCompleteGuideEventData data)
        {
            _data = data;
            ApplyData(data);
        }

        private void EnsureBound()
        {
            if (_isBound || gameObject.name != ExpectedRootName) return;
            Canvas.ForceUpdateCanvases();
            Transform root = transform;

            BindImage(ClientCompleteGuideEventImageSlot.Background, FindDirectChild(root, "background"));
            BindImage(ClientCompleteGuideEventImageSlot.HeaderBackground, FindDirectChild(root, "顶部背景-Image"));
            BindImage(ClientCompleteGuideEventImageSlot.HeaderCharacter, FindDirectChild(root, "顶部角色-Image"));
            BindImage(ClientCompleteGuideEventImageSlot.Title, FindDirectChild(root, "task title"));
            Transform period = FindDirectChild(root, "期数-inage");
            BindImage(ClientCompleteGuideEventImageSlot.PeriodFrame, period);
            BindImage(ClientCompleteGuideEventImageSlot.PeriodPanel, FindDeepChild(period, "Panel"));
            BindImage(ClientCompleteGuideEventImageSlot.PeriodNumber, FindDeepChild(period, "数字"));
            BindImage(ClientCompleteGuideEventImageSlot.PeriodUnit, FindDeepChild(period, "期"));
            Transform collection = FindDirectChild(root, "收集卡-image");
            BindImage(ClientCompleteGuideEventImageSlot.CollectionCard, collection);
            BindText(ClientCompleteGuideEventTextSlot.Collection, FindDeepChild(collection, "Text (Legacy)"));

            Transform popup = FindDirectChild(root, "弹窗");
            _rulesPopup = popup != null ? popup.gameObject : null;
            Transform rulesTextFrame = FindDeepChild(popup, "文字底框Image");
            BindImage(ClientCompleteGuideEventImageSlot.RulesTextFrame, rulesTextFrame);
            BindText(ClientCompleteGuideEventTextSlot.Rules, FindDeepChild(rulesTextFrame, "Text (TMP)"));
            Transform rulesTitle = FindDeepChild(popup, "规则说明Image");
            BindImage(ClientCompleteGuideEventImageSlot.RulesTitle, rulesTitle);
            BindImage(ClientCompleteGuideEventImageSlot.RulesDecoration, FindDirectChild(rulesTitle, "Image"));

            Transform progressPanel = FindDirectChild(root, "进度底框-Image");
            BindImage(ClientCompleteGuideEventImageSlot.ProgressPanel, progressPanel);
            Transform progressTrack = FindDirectChild(progressPanel, "进度条底框");
            Transform progressBar = FindDirectChild(progressTrack, "进度条");
            BindImage(ClientCompleteGuideEventImageSlot.ProgressTrack, progressTrack);
            BindImage(ClientCompleteGuideEventImageSlot.ProgressBar, progressBar);
            _progressTrack = progressTrack as RectTransform;
            _progressBar = progressBar as RectTransform;
            _fullProgressWidth = _progressBar != null && _progressBar.rect.width > 0f
                ? _progressBar.rect.width
                : (_progressTrack != null ? Mathf.Max(0f, _progressTrack.rect.width - 10f) : 0f);
            BindText(ClientCompleteGuideEventTextSlot.ProgressCurrent, FindDirectChild(progressPanel, "进度文本表示左"));
            BindText(ClientCompleteGuideEventTextSlot.ProgressSeparator, FindDirectChild(progressPanel, "进度文本表示中"));
            BindText(ClientCompleteGuideEventTextSlot.ProgressTarget, FindDirectChild(progressPanel, "进度文本表示右"));

            Transform claimNode = FindDirectChild(progressPanel, "领取+待完成键Button");
            BindImage(ClientCompleteGuideEventImageSlot.ClaimButton, claimNode);
            BindImage(ClientCompleteGuideEventImageSlot.ClaimLabel, FindDirectChild(claimNode, "Image"));
            _claimButton = claimNode != null ? claimNode.GetComponent<Button>() : null;
            _claimAnimator = claimNode != null ? claimNode.GetComponent<Animator>() : null;
            Transform claimed = FindDirectChild(progressPanel, "已领取icon");
            BindImage(ClientCompleteGuideEventImageSlot.ClaimedIcon, claimed);
            _claimedIcon = claimed != null ? claimed.gameObject : null;
            CapturePendingClaimSprites();

            Transform dateFrame = FindDirectChild(root, "活动时间底框-Image");
            BindImage(ClientCompleteGuideEventImageSlot.DateFrame, dateFrame);
            BindImage(ClientCompleteGuideEventImageSlot.DateLabel, FindDirectChild(dateFrame, "活动时间-image"));
            BindText(ClientCompleteGuideEventTextSlot.Date, FindDirectChild(dateFrame, "活动时间-Text"));

            Transform helpNode = FindDirectChild(root, "问号-button");
            BindImage(ClientCompleteGuideEventImageSlot.HelpButton, helpNode);
            Button helpButton = helpNode != null ? helpNode.GetComponent<Button>() : null;
            BindButton(helpButton, ShowRules);
            Transform bottomBar = FindDirectChild(root, "Bottom page function bar");
            BindImage(ClientCompleteGuideEventImageSlot.BottomBar, bottomBar);
            Transform backNode = FindDirectChild(bottomBar, "BackoffButton");
            BindImage(ClientCompleteGuideEventImageSlot.BackButton, backNode);
            BindButton(backNode != null ? backNode.GetComponent<Button>() : null, Back);
            BindButton(_claimButton, RequestClaim);

            _isBound = _images.Count == Enum.GetValues(typeof(ClientCompleteGuideEventImageSlot)).Length &&
                       _texts.Count == Enum.GetValues(typeof(ClientCompleteGuideEventTextSlot)).Length &&
                       _rulesPopup != null && _progressTrack != null && _progressBar != null &&
                       _claimButton != null && _claimedIcon != null;
            if (!_isBound)
                Debug.LogError("[Client] Complete Guide Event Page全图鉴活动缺少必需节点或组件。");
        }

        private void BindImage(ClientCompleteGuideEventImageSlot slot, Transform node)
        {
            Image image = node != null ? node.GetComponent<Image>() : null;
            if (image != null) _images[slot] = image;
        }

        private void BindText(ClientCompleteGuideEventTextSlot slot, Transform node)
        {
            if (node == null) return;
            TMP_Text tmp = node.GetComponent<TMP_Text>();
            Text legacy = node.GetComponent<Text>();
            if (tmp != null || legacy != null)
                _texts[slot] = new TextBinding { Tmp = tmp, Legacy = legacy };
        }

        private void ApplyData(ClientCompleteGuideEventData data)
        {
            if (data == null) return;
            SetText(ClientCompleteGuideEventTextSlot.Collection, data.CollectionText);
            SetText(ClientCompleteGuideEventTextSlot.Rules, data.RulesText);
            SetText(ClientCompleteGuideEventTextSlot.Date, data.DateText);
            SetProgress(data.Progress, data.Target);
            ApplyClaimState(data);
        }

        private void ApplyClaimState(ClientCompleteGuideEventData data)
        {
            if (data == null) return;
            bool claimed = data.IsClaimed;
            bool claimable = data.IsComplete && !claimed && !data.IsClaimPending;
            if (_claimButton != null)
            {
                _claimButton.gameObject.SetActive(!claimed);
                _claimButton.interactable = claimable;
            }
            if (_claimedIcon != null) _claimedIcon.SetActive(claimed);
            ApplyClaimVisual(claimable);
            SetImage(ClientCompleteGuideEventImageSlot.ClaimLabel,
                claimable ? data.ClaimLabelImageId : data.PendingLabelImageId);
            if (claimable)
                SetImage(ClientCompleteGuideEventImageSlot.ClaimButton, data.ClaimableButtonImageId);
        }

        private void CapturePendingClaimSprites()
        {
            Image buttonImage = GetImage(ClientCompleteGuideEventImageSlot.ClaimButton);
            Image labelImage = GetImage(ClientCompleteGuideEventImageSlot.ClaimLabel);
            _pendingButtonSprite = buttonImage != null ? buttonImage.sprite : null;
            _pendingLabelSprite = labelImage != null ? labelImage.sprite : null;
        }

        private void BeginClaimSpriteCapture()
        {
            if (_claimSpriteCaptureRoutine != null ||
                (_claimableButtonSprite != null && _claimableLabelSprite != null) ||
                _claimAnimator == null || _claimAnimator.runtimeAnimatorController == null)
                return;
            _claimAnimator.enabled = true;
            _claimSpriteCaptureRoutine = StartCoroutine(CaptureClaimSpritesWhenReady());
        }

        private IEnumerator CaptureClaimSpritesWhenReady()
        {
            for (int frame = 0; frame < 3; frame++)
            {
                yield return null;
                if (CaptureClaimSprites()) break;
            }
            _claimSpriteCaptureRoutine = null;
            ApplyClaimState(_data);
        }

        private bool CaptureClaimSprites()
        {
            Image buttonImage = GetImage(ClientCompleteGuideEventImageSlot.ClaimButton);
            Image labelImage = GetImage(ClientCompleteGuideEventImageSlot.ClaimLabel);
            int pressedState = Animator.StringToHash("Pressed");
            if (_claimAnimator == null || _claimAnimator.runtimeAnimatorController == null ||
                !_claimAnimator.isInitialized || !_claimAnimator.HasState(0, pressedState)) return false;

            bool wasEnabled = _claimAnimator.enabled;
            _claimAnimator.enabled = true;
            _claimAnimator.Play(pressedState, 0, 0f);
            _claimAnimator.Update(0f);
            _claimableButtonSprite = buttonImage != null ? buttonImage.sprite : null;
            _claimableLabelSprite = labelImage != null ? labelImage.sprite : null;
            _claimAnimator.Rebind();
            _claimAnimator.Update(0f);
            if (buttonImage != null) buttonImage.sprite = _pendingButtonSprite;
            if (labelImage != null) labelImage.sprite = _pendingLabelSprite;
            _claimAnimator.enabled = wasEnabled;
            return true;
        }

        private void ApplyClaimVisual(bool claimable)
        {
            if (_claimAnimator != null)
            {
                if (claimable && _claimableButtonSprite != null && _claimableLabelSprite != null)
                    _claimAnimator.enabled = false;
                else if (!claimable)
                {
                    _claimAnimator.enabled = true;
                    if (_claimAnimator.isInitialized)
                    {
                        _claimAnimator.Rebind();
                        _claimAnimator.Update(0f);
                    }
                }
                else _claimAnimator.enabled = true;
            }
            Image buttonImage = GetImage(ClientCompleteGuideEventImageSlot.ClaimButton);
            Image labelImage = GetImage(ClientCompleteGuideEventImageSlot.ClaimLabel);
            if (buttonImage != null)
                buttonImage.sprite = claimable && _claimableButtonSprite != null
                    ? _claimableButtonSprite : _pendingButtonSprite;
            if (labelImage != null)
                labelImage.sprite = claimable && _claimableLabelSprite != null
                    ? _claimableLabelSprite : _pendingLabelSprite;
        }

        private void RequestClaim()
        {
            if (_data == null || _data.IsClaimed || _data.IsClaimPending || !_data.IsComplete ||
                !ClientServices.IsInitialized) return;
            ClaimStarted?.Invoke(_data);
            ClientActivityTaskClaimResult result;
            bool accepted = ClientServices.Data.TryClaimCompleteGuideEvent(_data.EventId, out result);
            if (!accepted || result == null || result.Status == ClientRewardDeliveryStatus.Rejected)
            {
                string message = result != null && !string.IsNullOrEmpty(result.Message)
                    ? result.Message : "奖励领取失败";
                ClaimFailed?.Invoke(_data, message);
                TipRequested?.Invoke(message);
                return;
            }
            ClaimFinished?.Invoke(_data, result);
            Refresh();
        }

        private void Refresh()
        {
            if (!ClientServices.IsInitialized) return;
            _data = ClientServices.Data.GetCompleteGuideEvent();
            ApplyData(_data);
        }

        private void HandleDataChanged() { Refresh(); }

        private void SubscribeData()
        {
            if (_dataSubscribed || !ClientServices.IsInitialized) return;
            ClientServices.Data.DataChanged += HandleDataChanged;
            _dataSubscribed = true;
        }

        private void UnsubscribeData()
        {
            if (!_dataSubscribed || !ClientServices.IsInitialized) return;
            ClientServices.Data.DataChanged -= HandleDataChanged;
            _dataSubscribed = false;
        }

        private void ApplyImage(ClientCompleteGuideEventImageSlot slot, int imageId)
        {
            if (imageId <= 0) return;
            Image target = GetImage(slot);
            if (target == null) return;
            Sprite sprite;
            if (!_registeredSprites.TryGetValue(imageId, out sprite) && _spriteResolver != null)
                sprite = _spriteResolver.Resolve(imageId);
            if (sprite != null) target.sprite = sprite;
        }

        private Image GetImage(ClientCompleteGuideEventImageSlot slot)
        {
            Image image;
            return _images.TryGetValue(slot, out image) ? image : null;
        }

        private void BringProgressTextToFront()
        {
            TextBinding binding;
            if (_texts.TryGetValue(ClientCompleteGuideEventTextSlot.ProgressCurrent, out binding)) binding.BringToFront();
            if (_texts.TryGetValue(ClientCompleteGuideEventTextSlot.ProgressSeparator, out binding)) binding.BringToFront();
            if (_texts.TryGetValue(ClientCompleteGuideEventTextSlot.ProgressTarget, out binding)) binding.BringToFront();
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(action);
        }

        private static Transform FindDirectChild(Transform root, string nodeName)
        {
            if (root == null) return null;
            for (int index = 0; index < root.childCount; index++)
                if (root.GetChild(index).name == nodeName) return root.GetChild(index);
            return null;
        }

        private static Transform FindDeepChild(Transform root, string nodeName)
        {
            if (root == null) return null;
            for (int index = 0; index < root.childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (child.name == nodeName) return child;
                Transform found = FindDeepChild(child, nodeName);
                if (found != null) return found;
            }
            return null;
        }
    }
}
