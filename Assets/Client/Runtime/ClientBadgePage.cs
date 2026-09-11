using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public sealed class ClientBadgePage : MonoBehaviour
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Image _preview;
        [SerializeField] private Sprite[] _badges;
        [SerializeField] private Text _status;
        private string _selectedBadgeId;

        public void Configure(GameObject root, Image preview, Sprite[] badges, Text status)
        {
            _pageRoot = root;
            _preview = preview;
            _badges = badges;
            _status = status;
        }

        public void Show()
        {
            _pageRoot.SetActive(true);
            _selectedBadgeId = ClientServices.Data.GetEquippedBadgeId();
            Refresh();
        }

        public void SelectBadge(int index)
        {
            _selectedBadgeId = "badge-" + (index + 1).ToString("000");
            if (_preview != null && _badges != null && index >= 0 && index < _badges.Length)
                _preview.sprite = _badges[index];
            _status.text = "已选择铭牌 " + (index + 1);
        }

        public void Confirm()
        {
            if (!ClientServices.Data.TrySetEquippedBadge(_selectedBadgeId))
            {
                _status.text = "请选择有效铭牌";
                return;
            }
            _status.text = "铭牌已使用（本地模拟）";
        }

        private void Refresh()
        {
            if (string.IsNullOrEmpty(_selectedBadgeId))
                _selectedBadgeId = "badge-001";
            int index = int.Parse(_selectedBadgeId.Substring(_selectedBadgeId.Length - 3)) - 1;
            SelectBadge(Mathf.Clamp(index, 0, _badges.Length - 1));
            _status.text = "使用中的铭牌";
        }
    }
}
