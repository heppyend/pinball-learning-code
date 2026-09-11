using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public sealed class ClientBackpackPage : MonoBehaviour
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private GameObject _materialSlot;
        [SerializeField] private GameObject _equipmentSlot;
        [SerializeField] private Text _materialLabel;
        [SerializeField] private Text _equipmentLabel;
        [SerializeField] private Text _status;
        [SerializeField] private Text _detail;
        private string _filter = "All";
        private string _selectedItemId;
        private bool _waitingEquipmentConfirmation;

        public void Configure(GameObject root, GameObject materialSlot, GameObject equipmentSlot, Text materialLabel, Text equipmentLabel, Text status)
        {
            _pageRoot = root;
            _materialSlot = materialSlot;
            _equipmentSlot = equipmentSlot;
            _materialLabel = materialLabel;
            _equipmentLabel = equipmentLabel;
            _status = status;
        }

        public void ConfigureDetail(Text detail) { _detail = detail; }

        public void Show()
        {
            _pageRoot.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            if (_pageRoot != null)
                _pageRoot.SetActive(false);
        }

        public void SetFilter(string filter)
        {
            _filter = filter;
            Refresh();
        }

        public void SelectMaterial() { SelectItem("material-001", "养成材料用途待产品规则确认"); }
        public void SelectEquipment() { SelectItem("equipment-001", "演示装备属性与穿戴规则待产品确认"); }

        public void ToggleSelectedLock()
        {
            if (string.IsNullOrEmpty(_selectedItemId))
            {
                _status.text = "请先选择一个道具";
                return;
            }
            ClientInventoryItem selected = null;
            foreach (ClientInventoryItem item in ClientServices.Data.GetInventory())
                if (item.ItemId == _selectedItemId) selected = item;
            if (selected == null || !ClientServices.Data.TrySetInventoryItemLocked(selected.ItemId, !selected.IsLocked))
            {
                _status.text = "道具状态更新失败";
                return;
            }
            Refresh();
            _status.text = selected.IsLocked ? "道具已锁定" : "道具已解锁";
        }

        public void ToggleSelectedEquipment()
        {
            if (_selectedItemId != "equipment-001")
            {
                _status.text = "请先选择装备";
                return;
            }
            ClientInventoryItem equipment = null;
            foreach (ClientInventoryItem item in ClientServices.Data.GetInventory())
                if (item.ItemId == _selectedItemId) equipment = item;
            if (equipment == null) { _status.text = "装备不存在"; return; }
            string failureReason;
            bool equip = string.IsNullOrEmpty(equipment.EquippedHeroId);
            if (equip && !_waitingEquipmentConfirmation)
            {
                _waitingEquipmentConfirmation = true;
                _status.text = "将装备给演示英雄，再次点击确认";
                ClientUiFeedback.ShowToast("再次点击“装备/卸下”确认穿戴");
                return;
            }
            if (!ClientServices.Data.TrySetEquipmentEquipped(equipment.ItemId, equip, out failureReason))
            {
                _status.text = failureReason;
                return;
            }
            Refresh();
            _waitingEquipmentConfirmation = false;
            _status.text = equip ? "已装备给演示英雄" : "装备已卸下";
        }

        private void Refresh()
        {
            if (_materialSlot == null || _equipmentSlot == null)
                return;

            ClientInventoryItem material = null;
            ClientInventoryItem equipment = null;
            foreach (ClientInventoryItem item in ClientServices.Data.GetInventory())
            {
                if (item.ItemType == ClientItemType.Material)
                    material = item;
                else if (item.ItemType == ClientItemType.Equipment)
                    equipment = item;
            }

            _materialSlot.SetActive((_filter == "All" || _filter == "Material") && material != null);
            _equipmentSlot.SetActive((_filter == "All" || _filter == "Equipment") && equipment != null);
            if (material != null)
                _materialLabel.text = "养成材料\n× " + material.Quantity + (material.IsLocked ? "  [已锁]" : "");
            if (equipment != null)
                _equipmentLabel.text = "演示装备\n× " + equipment.Quantity + (equipment.IsLocked ? "  [已锁]" : "") + (string.IsNullOrEmpty(equipment.EquippedHeroId) ? "" : "  [已装备]");
            _status.text = _filter == "All" ? "全部道具" : (_filter == "Material" ? "材料" : "装备");
        }

        private void SelectItem(string itemId, string message)
        {
            _selectedItemId = itemId;
            _waitingEquipmentConfirmation = false;
            _status.text = message;
            if (_detail != null)
            {
                ClientInventoryItem selected = null;
                foreach (ClientInventoryItem item in ClientServices.Data.GetInventory())
                    if (item.ItemId == itemId) selected = item;
                _detail.text = selected == null ? "道具不存在" : (itemId == "equipment-001" ? "演示装备\n数量：" + selected.Quantity + "\n属性详情待产品确认\n可装备/卸下或锁定" : "养成材料\n数量：" + selected.Quantity + "\n用途与来源待产品确认");
            }
        }
    }
}
