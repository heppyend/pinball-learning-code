using UnityEngine;
using UnityEngine.EventSystems;

namespace Pinball.Client.UI
{
    /// <summary>购买成功页的任意点击关闭适配器，挂在既有可射线 UI 节点上。</summary>
    public sealed class ClientShopCloseOnClick : MonoBehaviour, IPointerClickHandler
    {
        private System.Action _onClick;

        public void Configure(System.Action onClick) { _onClick = onClick; }
        public void OnPointerClick(PointerEventData eventData) { _onClick?.Invoke(); }
    }
}
