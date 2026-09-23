using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client.UI
{
    /// <summary>
    /// SystemLayer 的通用提示所有者：Toast 的显示/隐藏与计时。
    ///
    /// 与加载页的分工：加载页（ClientLoadingPage）独立管理加载进度；
    /// 本组件只管跨页面的轻提示，不参与页面导航，也不持有业务状态。
    ///
    /// 接线方式：把 SystemLayer 下的提示根节点与文本拖到下面的槽位即可，
    /// 本组件不改动任何尺寸与排版（仅做 SetActive 与文本赋值）。
    /// </summary>
    public sealed class ClientSystemFeedback : MonoBehaviour, IClientFeedback
    {
        [Tooltip("提示根节点；显示/隐藏只切这一个节点。")]
        [SerializeField] private GameObject _toastRoot;

        [Tooltip("旧版 UGUI 文本；与 TMP 槽位任选其一填写。")]
        [SerializeField] private Text _toastText;

        [Tooltip("TextMeshPro 文本；与 UGUI 槽位任选其一填写。")]
        [SerializeField] private TMP_Text _toastLabel;

        [Tooltip("自动隐藏延时（秒，使用不受 timeScale 影响的实际时间）。")]
        [SerializeField] private float _toastDuration = 2f;

        private Coroutine _hideRoutine;

        /// <summary>仅供 Editor 场景写入工具调用，用于绑定已存在的节点。</summary>
        public void Configure(GameObject toastRoot, Text toastText)
        {
            _toastRoot = toastRoot;
            _toastText = toastText;
            if (_toastRoot != null)
                _toastRoot.SetActive(false);
        }

        private void Awake()
        {
            if (_toastRoot != null)
                _toastRoot.SetActive(false);
        }

        public void ShowToast(string message)
        {
            if (_toastRoot == null)
            {
                Debug.LogWarning("[Client] SystemLayer 提示未接线，Toast 被忽略：" + message);
                return;
            }

            if (_toastText != null)
                _toastText.text = message;
            if (_toastLabel != null)
                _toastLabel.text = message;

            _toastRoot.SetActive(true);
            _toastRoot.transform.SetAsLastSibling();

            if (_hideRoutine != null)
                StopCoroutine(_hideRoutine);
            _hideRoutine = StartCoroutine(HideAfterDelay());
        }

        public void HideToast()
        {
            if (_hideRoutine != null)
            {
                StopCoroutine(_hideRoutine);
                _hideRoutine = null;
            }
            if (_toastRoot != null)
                _toastRoot.SetActive(false);
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_toastDuration);
            _hideRoutine = null;
            if (_toastRoot != null)
                _toastRoot.SetActive(false);
        }
    }
}
