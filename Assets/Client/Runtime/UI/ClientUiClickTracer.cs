using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 鼠标点击追踪器：每次按下左键时，用 EventSystem 做一次射线检测，
    /// 打印"命中了哪些 UI 节点、哪一层会真正接收这次点击"。
    ///
    /// 用途：排查"点了没反应 / 点到了别的东西 / 被遮罩挡住"这类问题。
    /// 挂在 ClientCanvas 上（或任意常驻节点），由 ClientShellController 自动补挂。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ClientUiClickTracer : MonoBehaviour
    {
        [Tooltip("是否记录点击。诊断结束后取消勾选即可静音。")]
        [SerializeField] private bool _logClicks = true;

        [Tooltip("最多打印多少层命中结果（从最上层开始）。")]
        [SerializeField] private int _maxHits = 4;

        private readonly List<RaycastResult> _hits = new List<RaycastResult>();
        private readonly StringBuilder _hitText = new StringBuilder(256);
        private readonly StringBuilder _handlerText = new StringBuilder(256);

        private void Update()
        {
            if (!_logClicks || !ClientUiTrace.Enabled || !ClientUiTrace.LogClicks)
                return;

            if (!TryGetPressPosition(out Vector2 position))
                return;

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                ClientUiTrace.Warn("点击", "EventSystem.current 为空，无法做射线检测。");
                return;
            }

            PointerEventData pointer = new PointerEventData(eventSystem) { position = position };
            _hits.Clear();
            eventSystem.RaycastAll(pointer, _hits);

            if (_hits.Count == 0)
            {
                ClientUiTrace.ClickMiss(position);
                return;
            }

            _hitText.Length = 0;
            int limit = Mathf.Min(_maxHits, _hits.Count);
            for (int i = 0; i < limit; i++)
            {
                if (i > 0) _hitText.Append("  |  ");
                _hitText.Append(ClientUiTrace.Path(_hits[i].gameObject));
            }

            _handlerText.Length = 0;
            bool anyHandler = false;
            for (int i = 0; i < _hits.Count; i++)
            {
                string handler = DescribeHandlers(_hits[i].gameObject);
                if (string.IsNullOrEmpty(handler))
                    continue;
                if (anyHandler) _handlerText.Append("  |  ");
                _handlerText.Append(handler);
                anyHandler = true;
            }
            if (!anyHandler)
                _handlerText.Append("(该节点链上没有任何 Button/Toggle/点击处理器 —— 这次点击不会有任何效果)");

            ClientUiTrace.Click(position, _hitText.ToString(), _handlerText.ToString());
        }

        /// <summary>兼容旧输入与触摸；取不到则本次不记录。</summary>
        private static bool TryGetPressPosition(out Vector2 position)
        {
            position = Vector2.zero;
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0))
            {
                position = Input.mousePosition;
                return true;
            }
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                position = Input.GetTouch(0).position;
                return true;
            }
#endif
            return false;
        }

        /// <summary>从命中节点向上找第一个会处理点击的组件，并列出沿途的 Button/Toggle。</summary>
        private static string DescribeHandlers(GameObject hit)
        {
            StringBuilder builder = new StringBuilder(128);
            Transform current = hit != null ? hit.transform : null;
            int guard = 0;

            while (current != null && guard < 16)
            {
                Button button = current.GetComponent<Button>();
                if (button != null)
                {
                    // 注意：GetPersistentEventCount() 只统计 Inspector 里持久化的监听；
                    // 代码用 AddListener 注册的运行时监听它永远返回 0。
                    // 因此这里明确写成"Inspector持久监听"，避免被误读为"没有绑定"。
                    int persistent = button.onClick.GetPersistentEventCount();
                    builder.Append("Button '").Append(current.name)
                           .Append("' Inspector持久监听=").Append(persistent)
                           .Append("（运行时AddListener不在此计数）")
                           .Append(button.interactable ? "" : " [不可交互]")
                           .Append(" <- ").Append(ClientUiTrace.Path(current));
                    // Button 内部通过 IPointerClickHandler 响应，继续向上没有意义
                    return builder.ToString();
                }

                Toggle toggle = current.GetComponent<Toggle>();
                if (toggle != null)
                {
                    builder.Append("Toggle '").Append(current.name)
                           .Append("' isOn=").Append(toggle.isOn)
                           .Append(" group=").Append(toggle.group != null ? toggle.group.name : "(无)")
                           .Append(" interactable=").Append(toggle.interactable)
                           .Append(" <- ").Append(ClientUiTrace.Path(current));
                    return builder.ToString();
                }

                current = current.parent;
                guard++;
            }

            // 没有 Button/Toggle，退一步报告是否有其它点击处理器
            current = hit != null ? hit.transform : null;
            guard = 0;
            while (current != null && guard < 16)
            {
                MonoBehaviour[] behaviours = current.GetComponents<MonoBehaviour>();
                for (int i = 0; i < behaviours.Length; i++)
                {
                    if (behaviours[i] is IPointerClickHandler)
                        return behaviours[i].GetType().Name + "@" + ClientUiTrace.Path(current);
                }
                current = current.parent;
                guard++;
            }
            return null;
        }
    }
}
