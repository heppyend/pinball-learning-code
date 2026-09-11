using UnityEngine;
using UnityEngine.UI;
using Pinball.Client.Services;

namespace Pinball.Client
{
    /// <summary>加载页只负责展示进度；资源系统通过 IClientResourceLoadSource 接入。</summary>
    public sealed class ClientLoadingPage : MonoBehaviour
    {
        [SerializeField] private Image _progressFill;
        [SerializeField] private Text _progressText;
        [SerializeField] private ClientLoadingDotsAnimator _dotsAnimator;
        [SerializeField] private GameObject _completeOverlay;
        private IClientResourceLoadSource _source;

        public float Progress { get; private set; }

        private void Awake() { AutoBindFromHierarchy(); }

        /// <summary>按现有加载页节点名自动取引用，不创建、移动或重排任何 UI。</summary>
        public void AutoBindFromHierarchy()
        {
            Transform root = transform;
            if (_progressFill == null)
            {
                Transform fill = root.Find("加载底框/加载进度条");
                if (fill != null) _progressFill = fill.GetComponent<Image>();
            }
            if (_dotsAnimator == null)
            {
                _dotsAnimator = GetComponent<ClientLoadingDotsAnimator>();
                if (_dotsAnimator != null)
                {
                    RectTransform[] dots = new RectTransform[3];
                    for (int i = 0; i < dots.Length; i++)
                    {
                        Transform dot = root.Find("加载标点" + (i + 1));
                        if (dot == null) dot = root.Find("加载标点" + (i + 1));
                        dots[i] = dot != null ? dot.GetComponent<RectTransform>() : null;
                    }
                    _dotsAnimator.Configure(dots);
                }
            }
            if (_progressFill != null)
            {
                _progressFill.type = Image.Type.Filled;
                _progressFill.fillMethod = Image.FillMethod.Horizontal;
                _progressFill.fillOrigin = 0;
            }
        }

        public void Configure(Image progressFill, Text progressText, ClientLoadingDotsAnimator dotsAnimator, GameObject completeOverlay)
        {
            _progressFill = progressFill;
            _progressText = progressText;
            _dotsAnimator = dotsAnimator;
            _completeOverlay = completeOverlay;
        }

        public void Bind(IClientResourceLoadSource source)
        {
            Unbind();
            _source = source;
            if (_source == null) { SetProgress(0f); return; }
            _source.ProgressChanged += SetProgress;
            SetProgress(_source.CurrentProgress);
            if (_source.IsCompleted) Complete();
        }

        public void Unbind()
        {
            if (_source != null) _source.ProgressChanged -= SetProgress;
            _source = null;
        }

        public void Begin()
        {
            if (_completeOverlay != null) _completeOverlay.SetActive(false);
            SetProgress(0f);
            if (_dotsAnimator != null) _dotsAnimator.Begin();
        }

        public void SetProgress(float value)
        {
            Progress = Mathf.Clamp01(value);
            if (_progressFill != null) _progressFill.fillAmount = Progress;
            if (_progressText != null) _progressText.text = Mathf.RoundToInt(Progress * 100f) + "%";
        }

        public void Complete()
        {
            SetProgress(1f);
            if (_dotsAnimator != null) _dotsAnimator.Stop();
            if (_completeOverlay != null) _completeOverlay.SetActive(true);
        }

        private void OnEnable() { Begin(); }
        private void OnDisable() { Unbind(); if (_dotsAnimator != null) _dotsAnimator.Stop(); }
    }
}
