using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    [RequireComponent(typeof(Button))]
    public sealed class ClientBadgeButton : MonoBehaviour
    {
        [SerializeField] private int _badgeIndex;
        [SerializeField] private bool _confirm;

        public void Configure(int badgeIndex, bool confirm)
        {
            _badgeIndex = badgeIndex;
            _confirm = confirm;
        }

        private void Awake()
        {
            ClientBadgePage page = GetComponentInParent<ClientBadgePage>();
            Button button = GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            if (page == null)
                return;
            if (_confirm)
                button.onClick.AddListener(page.Confirm);
            else
                button.onClick.AddListener(() => page.SelectBadge(_badgeIndex));
        }
    }
}
