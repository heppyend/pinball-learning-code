using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pinball.Client.UI
{
    /// <summary>开发期商城导航探针：记录返回按钮实际收到的 UI 事件。</summary>
    public sealed class ClientShopNavigationDiagnostics : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        private Func<string> _state;
        private bool _enabled;

        public void Configure(Func<string> state, bool enabled) { _state = state; _enabled = enabled; }

        public void OnPointerDown(PointerEventData eventData) { Log("PointerDown"); }
        public void OnPointerUp(PointerEventData eventData) { Log("PointerUp"); }
        public void OnPointerClick(PointerEventData eventData) { Log("PointerClick"); }

        private void Log(string phase)
        {
            if (!_enabled) return;
            Debug.Log($"[ClientShop导航诊断] {phase}; target={gameObject.name}; {_state?.Invoke()}", this);
        }
    }
}
