using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public sealed class ClientFormationPage : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _slot;
        [SerializeField] private Text _status;
        public void Configure(GameObject root, Text slot, Text status) { _root=root; _slot=slot; _status=status; }
        public void Show() { _root.SetActive(true); Refresh(); }
        public void SelectHero(string heroId)
        {
            if (!ClientServices.Data.TrySetFormationHero(heroId)) { _status.text="该英雄尚未拥有"; return; }
            _status.text="编队已保存（本地模拟）"; Refresh();
        }
        private void Refresh()
        {
            string selected=ClientServices.Data.GetFormationHeroId(); ClientHero hero=null;
            foreach (ClientHero item in ClientServices.Data.GetHeroes()) if (item.HeroId==selected) hero=item;
            _slot.text="出战槽位 1\n"+(hero==null ? "未选择" : hero.Name+"  Lv."+hero.Level);
            if (string.IsNullOrEmpty(_status.text)) _status.text="点击下方英雄调整编队";
        }
    }
}
