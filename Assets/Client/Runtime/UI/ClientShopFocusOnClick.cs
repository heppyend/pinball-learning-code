using UnityEngine;
using UnityEngine.EventSystems;

namespace Pinball.Client.UI
{
    /// <summary>将既有兑换码图标或文字的点击转发为输入框聚焦。</summary>
    public sealed class ClientShopFocusOnClick : MonoBehaviour, IPointerClickHandler
    {
        private System.Action _onClick;

        public void Configure(System.Action onClick) { _onClick = onClick; }
        public void OnPointerClick(PointerEventData eventData) { _onClick?.Invoke(); }
    }
}
