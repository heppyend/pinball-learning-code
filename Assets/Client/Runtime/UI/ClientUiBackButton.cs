using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client.UI
{
    [RequireComponent(typeof(Button))]
    public sealed class ClientUiBackButton : MonoBehaviour
    {
        private Button _button;

        private void OnEnable()
        {
            _button = _button != null ? _button : GetComponent<Button>();
            _button.onClick = new Button.ButtonClickedEvent();
            ClientUiNavigator navigator = GetComponentInParent<ClientUiNavigator>();
            if (navigator != null)
                _button.onClick.AddListener(navigator.Back);
            else
                Debug.LogError("[Client] 返回按钮未找到 ClientUiNavigator：" + name);
        }
    }
}
