using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public enum ClientLoadingImageSlot
    {
        Background,
        Logo,
        ProgressTrack,
        ProgressBar,
        Dot1,
        Dot2,
        Dot3,
    }

    /// <summary>把业务数字图片 ID 转为加载页可显示的 Sprite；后续可由 YooAsset 实现替换。</summary>
    public interface IClientLoadingImageResolver
    {
        Sprite Resolve(int imageId);
    }

    /// <summary>只绑定 SystemLayer 中负责人已有的“加载Page”，不创建或调整 UI 节点。</summary>
    public sealed class ClientLoadingPage : MonoBehaviour
    {
        [Header("Images")]
        [SerializeField] private Image _background;
        [SerializeField] private Image _logo;
        [SerializeField] private Image _progressTrack;
        [SerializeField] private Image _progressBar;
        [SerializeField] private Image[] _loadingDots = new Image[3];

        [Header("Text")]
        [SerializeField] private TMP_Text _loadingText;

        [Header("Progress")]
        [SerializeField, Min(0.01f)] private float _progressSmoothTime = 0.15f;

        [Header("Dot animation")]
        [SerializeField, Min(0.01f)] private float _dotStepInterval = 0.2f;
        [SerializeField, Min(0f)] private float _dotCycleInterval = 0.2f;

        private readonly Dictionary<int, Sprite> _registeredSprites = new Dictionary<int, Sprite>();
        private readonly Dictionary<ClientLoadingImageSlot, int> _imageIds =
            new Dictionary<ClientLoadingImageSlot, int>();

        private IClientLoadingImageResolver _imageResolver;
        private Coroutine _progressRoutine;
        private Coroutine _dotRoutine;
        private float _targetProgress;
        private float _progressVelocity;
#if UNITY_EDITOR
        private Coroutine _previewRoutine;
#endif

        public event Action<ClientLoadingImageSlot, int> ImageIdChanged;
        public event Action<float> ProgressChanged;

        public float Progress { get; private set; }
        public float TargetProgress { get { return _targetProgress; } }

        private void Awake()
        {
            AutoBindFromHierarchy();
            ApplyProgress(Progress);
        }

        private void OnEnable()
        {
            AutoBindFromHierarchy();
            if (!Mathf.Approximately(Progress, _targetProgress))
                StartProgressSmoothing();
            StartDotAnimation();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            _previewRoutine = null;
#endif
            StopProgressSmoothing();
            StopDotAnimation();
        }

        /// <summary>按新加载页的现有节点名绑定引用，不创建、移动或重排节点。</summary>
        public void AutoBindFromHierarchy()
        {
            if (_background == null)
                _background = FindComponent<Image>("background");
            if (_logo == null)
                _logo = FindComponent<Image>("加载Logo");
            if (_progressTrack == null)
                _progressTrack = FindComponent<Image>("加载底框");
            if (_progressBar == null)
                _progressBar = FindComponent<Image>("加载底框/加载进度条");
            if (_loadingText == null)
                _loadingText = FindComponent<TMP_Text>("加载文本");

            EnsureDotArray();
            for (int index = 0; index < _loadingDots.Length; index++)
            {
                if (_loadingDots[index] == null)
                    _loadingDots[index] = FindComponent<Image>("加载标点" + (index + 1));
            }

            if (_progressBar != null)
            {
                _progressBar.type = Image.Type.Filled;
                _progressBar.fillMethod = Image.FillMethod.Horizontal;
                _progressBar.fillOrigin = (int)Image.OriginHorizontal.Left;
                _progressBar.fillClockwise = true;
            }
        }

        public void SetImageResolver(IClientLoadingImageResolver resolver)
        {
            _imageResolver = resolver;
            ReapplyImageIds();
        }

        public void RegisterSprite(int imageId, Sprite sprite)
        {
            if (imageId <= 0 || sprite == null)
                return;
            _registeredSprites[imageId] = sprite;
            ReapplyImageId(imageId);
        }

        /// <summary>所有 Image 共用的数字 ID 接口。</summary>
        public void SetImage(ClientLoadingImageSlot slot, int imageId)
        {
            _imageIds[slot] = imageId;
            Image target = GetImage(slot);
            if (target != null && imageId > 0)
            {
                Sprite sprite = ResolveSprite(imageId);
                if (sprite != null)
                    target.sprite = sprite;
            }
            ImageIdChanged?.Invoke(slot, imageId);
        }

        public void SetBackgroundImage(int imageId) { SetImage(ClientLoadingImageSlot.Background, imageId); }
        public void SetLogoImage(int imageId) { SetImage(ClientLoadingImageSlot.Logo, imageId); }
        public void SetProgressTrackImage(int imageId) { SetImage(ClientLoadingImageSlot.ProgressTrack, imageId); }
        public void SetProgressBarImage(int imageId) { SetImage(ClientLoadingImageSlot.ProgressBar, imageId); }

        public void SetLoadingDotImage(int dotIndex, int imageId)
        {
            if (dotIndex < 0 || dotIndex >= 3)
                throw new ArgumentOutOfRangeException(nameof(dotIndex), "加载标点索引必须为 0、1 或 2。");
            SetImage((ClientLoadingImageSlot)((int)ClientLoadingImageSlot.Dot1 + dotIndex), imageId);
        }

        public void SetLoadingDotImages(int dot1ImageId, int dot2ImageId, int dot3ImageId)
        {
            SetImage(ClientLoadingImageSlot.Dot1, dot1ImageId);
            SetImage(ClientLoadingImageSlot.Dot2, dot2ImageId);
            SetImage(ClientLoadingImageSlot.Dot3, dot3ImageId);
        }

        /// <summary>加载文本泛型接口；数字、枚举和字符串均可直接传入。</summary>
        public void SetLoadingText<T>(T value)
        {
            if (_loadingText == null)
                AutoBindFromHierarchy();
            if (_loadingText != null)
                _loadingText.text = value == null
                    ? string.Empty
                    : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        /// <summary>接收 0~1 的目标加载进度，显示值会平滑追赶目标值。</summary>
        public void SetProgress(float value)
        {
            _targetProgress = Mathf.Clamp01(value);
            ProgressChanged?.Invoke(_targetProgress);

            if (!isActiveAndEnabled)
            {
                SetProgressImmediate(_targetProgress);
                return;
            }

            StartProgressSmoothing();
        }

        /// <summary>接收 0~100 的整数百分比。</summary>
        public void SetProgressPercent(int percent)
        {
            SetProgress(Mathf.Clamp(percent, 0, 100) / 100f);
        }

        /// <summary>立即设置进度，不执行插值；用于开始、完成或页面尚未激活时同步状态。</summary>
        public void SetProgressImmediate(float value)
        {
            StopProgressSmoothing();
            Progress = Mathf.Clamp01(value);
            _targetProgress = Progress;
            _progressVelocity = 0f;
            ApplyProgress(Progress);
        }

        public void Begin()
        {
            gameObject.SetActive(true);
            SetProgressImmediate(0f);
            StartDotAnimation();
        }

        public void Complete()
        {
            SetProgressImmediate(1f);
            StopDotAnimation();
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public void SetDotAnimationTiming(float stepInterval, float cycleInterval)
        {
            _dotStepInterval = Mathf.Max(0.01f, stepInterval);
            _dotCycleInterval = Mathf.Max(0f, cycleInterval);
            if (isActiveAndEnabled)
                StartDotAnimation();
        }

#if UNITY_EDITOR
        /// <summary>仅供 Unity Inspector 在 Play 模式下一键验证，不进入正式 Player 构建。</summary>
        public void PlayFiveSecondPreview()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[ClientLoadingPage] 请先进入 Play 模式，再播放 5 秒加载演示。", this);
                return;
            }

            // ⚠️ 必须先打开 SystemLayer：本页挂在**恒为 inactive** 的该层下，
            //    只 SetActive 自己，`activeInHierarchy` 仍为 false ⇒ 下面的 StartCoroutine 会报
            //    "Coroutine couldn't be started because the game object is inactive"，
            //    而且画面也不会出现。交给 ClientLoadingService 统一处理开关。
            ClientLoadingService.Begin();

            if (_previewRoutine != null)
                StopCoroutine(_previewRoutine);
            _previewRoutine = StartCoroutine(PlayPreviewRoutine());
        }

        private IEnumerator PlayPreviewRoutine()
        {
            const float duration = 5f;
            float[] checkpoints = { 0f, 0.08f, 0.22f, 0.47f, 0.63f, 0.82f, 0.94f, 1f };
            float checkpointInterval = duration / checkpoints.Length;
            Begin();

            for (int index = 1; index < checkpoints.Length; index++)
            {
                SetProgress(checkpoints[index]);
                yield return WaitUnscaled(checkpointInterval);
            }

            yield return WaitUnscaled(checkpointInterval);
            Complete();
            // 满进度停留一下便于观察，然后收尾隐藏并关掉 SystemLayer ——
            // 否则演示结束会把整屏加载页留在画面上（以前它显示不出来，所以这个收尾一直没暴露）。
            yield return WaitUnscaled(0.6f);
            ClientLoadingService.Hide();
            _previewRoutine = null;
        }
#endif

        private void StartProgressSmoothing()
        {
            if (Mathf.Approximately(Progress, _targetProgress))
            {
                Progress = _targetProgress;
                ApplyProgress(Progress);
                return;
            }
            if (_progressRoutine == null)
                _progressRoutine = StartCoroutine(SmoothProgressToTarget());
        }

        private void StopProgressSmoothing()
        {
            if (_progressRoutine != null)
                StopCoroutine(_progressRoutine);
            _progressRoutine = null;
            _progressVelocity = 0f;
        }

        private IEnumerator SmoothProgressToTarget()
        {
            const float completionThreshold = 0.0001f;
            while (Mathf.Abs(Progress - _targetProgress) > completionThreshold)
            {
                Progress = Mathf.SmoothDamp(
                    Progress,
                    _targetProgress,
                    ref _progressVelocity,
                    _progressSmoothTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);
                ApplyProgress(Progress);
                yield return null;
            }

            Progress = _targetProgress;
            _progressVelocity = 0f;
            ApplyProgress(Progress);
            _progressRoutine = null;
        }

        private void ApplyProgress(float value)
        {
            if (_progressBar == null)
                AutoBindFromHierarchy();
            if (_progressBar != null)
                _progressBar.fillAmount = value;
        }

        private void StartDotAnimation()
        {
            StopDotAnimation();
            SetAllDotsVisible(true);
            if (isActiveAndEnabled)
                _dotRoutine = StartCoroutine(AnimateDots());
        }

        private void StopDotAnimation()
        {
            if (_dotRoutine != null)
                StopCoroutine(_dotRoutine);
            _dotRoutine = null;
            SetAllDotsVisible(true);
        }

        private IEnumerator AnimateDots()
        {
            while (isActiveAndEnabled)
            {
                for (int index = 0; index < _loadingDots.Length; index++)
                {
                    SetDotVisible(index, false);
                    yield return WaitUnscaled(_dotStepInterval);
                }

                for (int index = 0; index < _loadingDots.Length; index++)
                {
                    SetDotVisible(index, true);
                    yield return WaitUnscaled(_dotStepInterval);
                }

                if (_dotCycleInterval > 0f)
                    yield return WaitUnscaled(_dotCycleInterval);
            }
        }

        private void SetAllDotsVisible(bool visible)
        {
            EnsureDotArray();
            for (int index = 0; index < _loadingDots.Length; index++)
                SetDotVisible(index, visible);
        }

        private void SetDotVisible(int index, bool visible)
        {
            if (_loadingDots[index] != null)
                _loadingDots[index].gameObject.SetActive(visible);
        }

        private Image GetImage(ClientLoadingImageSlot slot)
        {
            switch (slot)
            {
                case ClientLoadingImageSlot.Background: return _background;
                case ClientLoadingImageSlot.Logo: return _logo;
                case ClientLoadingImageSlot.ProgressTrack: return _progressTrack;
                case ClientLoadingImageSlot.ProgressBar: return _progressBar;
                case ClientLoadingImageSlot.Dot1: return _loadingDots[0];
                case ClientLoadingImageSlot.Dot2: return _loadingDots[1];
                case ClientLoadingImageSlot.Dot3: return _loadingDots[2];
                default: throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }
        }

        private Sprite ResolveSprite(int imageId)
        {
            if (_registeredSprites.TryGetValue(imageId, out Sprite registered))
                return registered;
            return _imageResolver != null ? _imageResolver.Resolve(imageId) : null;
        }

        private void ReapplyImageIds()
        {
            foreach (KeyValuePair<ClientLoadingImageSlot, int> entry in _imageIds)
                ApplyResolvedSprite(entry.Key, entry.Value);
        }

        private void ReapplyImageId(int imageId)
        {
            foreach (KeyValuePair<ClientLoadingImageSlot, int> entry in _imageIds)
                if (entry.Value == imageId)
                    ApplyResolvedSprite(entry.Key, entry.Value);
        }

        private void ApplyResolvedSprite(ClientLoadingImageSlot slot, int imageId)
        {
            Image target = GetImage(slot);
            Sprite sprite = ResolveSprite(imageId);
            if (target != null && sprite != null)
                target.sprite = sprite;
        }

        private void EnsureDotArray()
        {
            if (_loadingDots == null || _loadingDots.Length != 3)
                _loadingDots = new Image[3];
        }

        private T FindComponent<T>(string relativePath) where T : Component
        {
            Transform child = transform.Find(relativePath);
            return child != null ? child.GetComponent<T>() : null;
        }

        private static IEnumerator WaitUnscaled(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
