using System.Collections;
using UnityEngine;

namespace Pinball.Client
{
    /// <summary>加载页三个点按 1→2→3 依次跳动，使用协程避免 Update 轮询。</summary>
    public sealed class ClientLoadingDotsAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform[] _dots = new RectTransform[3];
        [SerializeField] private float _jumpHeight = 22f;
        [SerializeField] private float _jumpDuration = 0.22f;
        [SerializeField] private float _betweenDotDelay = 0.06f;
        [SerializeField] private float _cycleDelay = 0.18f;

        private Vector2[] _basePositions;
        private Coroutine _routine;

        public void Configure(RectTransform[] dots)
        {
            _dots = dots;
            CaptureBasePositions();
        }

        private void Awake() { CaptureBasePositions(); }

        public void Begin()
        {
            CaptureBasePositions();
            Stop();
            _routine = StartCoroutine(AnimateLoop());
        }

        public void Stop()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            RestoreBasePositions();
        }

        private void OnDisable() { Stop(); }

        private void CaptureBasePositions()
        {
            if (_dots == null) return;
            _basePositions = new Vector2[_dots.Length];
            for (int i = 0; i < _dots.Length; i++)
                if (_dots[i] != null) _basePositions[i] = _dots[i].anchoredPosition;
        }

        private void RestoreBasePositions()
        {
            if (_dots == null || _basePositions == null) return;
            for (int i = 0; i < _dots.Length; i++)
                if (_dots[i] != null && i < _basePositions.Length) _dots[i].anchoredPosition = _basePositions[i];
        }

        private IEnumerator AnimateLoop()
        {
            while (isActiveAndEnabled)
            {
                for (int i = 0; i < _dots.Length; i++)
                {
                    if (_dots[i] != null) yield return AnimateDot(i);
                    if (_betweenDotDelay > 0f) yield return WaitUnscaled(_betweenDotDelay);
                }
                if (_cycleDelay > 0f) yield return WaitUnscaled(_cycleDelay);
            }
        }

        private IEnumerator AnimateDot(int index)
        {
            float duration = Mathf.Max(0.01f, _jumpDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float y = Mathf.Sin(t * Mathf.PI) * _jumpHeight;
                _dots[index].anchoredPosition = _basePositions[index] + Vector2.up * y;
                yield return null;
            }
            _dots[index].anchoredPosition = _basePositions[index];
        }

        private static IEnumerator WaitUnscaled(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds) { elapsed += Time.unscaledDeltaTime; yield return null; }
        }
    }
}
