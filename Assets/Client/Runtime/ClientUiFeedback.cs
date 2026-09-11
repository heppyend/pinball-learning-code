using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>全局轻提示与加载遮罩。节点固定在 ClientCanvas/SystemLayer，由场景维护。</summary>
    public sealed class ClientUiFeedback : MonoBehaviour
    {
        private static ClientUiFeedback _instance;

        [SerializeField] private GameObject _toastRoot;
        [SerializeField] private Text _toastText;
        [SerializeField] private GameObject _loadingRoot;
        private Coroutine _hideToastCoroutine;

        private void Awake()
        {
            _instance = this;
            if (_toastRoot != null) _toastRoot.SetActive(false);
            if (_loadingRoot != null) _loadingRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void Configure(GameObject toastRoot, Text toastText, GameObject loadingRoot)
        {
            _toastRoot = toastRoot;
            _toastText = toastText;
            _loadingRoot = loadingRoot;
            if (_toastRoot != null) _toastRoot.SetActive(false);
            if (_loadingRoot != null) _loadingRoot.SetActive(false);
        }

        public static void ShowToast(string message)
        {
            if (_instance == null || _instance._toastRoot == null) return;
            if (_instance._toastText != null) _instance._toastText.text = message;
            _instance._toastRoot.SetActive(true);
            _instance._toastRoot.transform.SetAsLastSibling();
            if (_instance._hideToastCoroutine != null)
                _instance.StopCoroutine(_instance._hideToastCoroutine);
            _instance._hideToastCoroutine = _instance.StartCoroutine(_instance.HideToastAfterDelay());
        }

        public static void HideToast()
        {
            if (_instance != null && _instance._toastRoot != null)
            {
                _instance._toastRoot.SetActive(false);
                _instance._hideToastCoroutine = null;
            }
        }

        public static void SetLoading(bool visible)
        {
            if (_instance != null && _instance._loadingRoot != null) _instance._loadingRoot.SetActive(visible);
        }

        private System.Collections.IEnumerator HideToastAfterDelay()
        {
            yield return new WaitForSecondsRealtime(2f);
            HideToast();
        }
    }
}
