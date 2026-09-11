using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public sealed class ClientMarblePage : MonoBehaviour
    {
        [SerializeField] private GameObject _root; [SerializeField] private Text _equipped; [SerializeField] private Text _status;
        public void Configure(GameObject root, Text equipped, Text status) { _root=root; _equipped=equipped; _status=status; }
        public void Show() { _root.SetActive(true); Refresh(); }
        public void SelectMarble(string marbleId)
        {
            if (!ClientServices.Data.TrySetEquippedMarble(marbleId)) { _status.text="该弹珠尚未拥有"; return; }
            _status.text="弹珠已装备（本地模拟）"; Refresh();
        }
        private void Refresh()
        {
            string id=ClientServices.Data.GetEquippedMarbleId(); ClientMarble marble=null;
            foreach (ClientMarble item in ClientServices.Data.GetMarbles()) if (item.MarbleId==id) marble=item;
            _equipped.text="当前装备\n"+(marble==null ? "未选择" : marble.DisplayName+"\n"+marble.Description);
            if (string.IsNullOrEmpty(_status.text)) _status.text="选择已拥有弹珠进行装备";
        }
    }
}
