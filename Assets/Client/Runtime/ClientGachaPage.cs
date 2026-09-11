using System.Text;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>开发期扭蛋页：只展示本地模拟卡池与抽取结果，不包含正式概率或支付。</summary>
    public sealed class ClientGachaPage : MonoBehaviour
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text _poolTitle;
        [SerializeField] private Text _currency;
        [SerializeField] private Text _status;
        [SerializeField] private Text _result;
        [SerializeField] private Text _history;
        [SerializeField] private ClientGachaResultPopup _resultPopup;

        public void Configure(GameObject pageRoot, Text poolTitle, Text currency, Text status, Text result)
        {
            _pageRoot = pageRoot;
            _poolTitle = poolTitle;
            _currency = currency;
            _status = status;
            _result = result;
        }
        public void ConfigureHistory(Text history) { _history = history; }
        public void ConfigureResultPopup(ClientGachaResultPopup popup) { _resultPopup = popup; }

        public void Show()
        {
            _pageRoot.SetActive(true);
            Refresh();
        }

        public void DrawOne() { Draw(1); }
        public void DrawTen() { Draw(10); }

        private void Draw(int count)
        {
            ClientGachaPool pool = ClientServices.Data.GetGachaPool();
            ClientGachaDrawResult result;
            if (!ClientServices.Data.TryDrawGacha(pool.PoolId, count, out result))
            {
                _status.text = "钻石不足，无法抽取（本地模拟）";
                return;
            }

            StringBuilder names = new StringBuilder();
            foreach (string heroId in result.RewardHeroIds)
            {
                ClientHero hero = FindHero(heroId);
                if (names.Length > 0)
                    names.Append("、");
                names.Append(hero != null ? hero.Name : heroId);
            }
            _result.text = "获得：" + names;
            _status.text = count == 1 ? "单抽完成" : "十连完成";
            if (_resultPopup != null)
                _resultPopup.Show((count == 1 ? "单抽结果" : "十连结果") + "\n\n获得：" + names);
            RefreshWallet();
            RefreshHistory();
        }

        private void Refresh()
        {
            ClientGachaPool pool = ClientServices.Data.GetGachaPool();
            _poolTitle.text = pool.DisplayName + "\n单抽 " + pool.SingleDrawCost + " / 十连 " + pool.TenDrawCost;
            _result.text = "点击下方按钮进行本地模拟抽取";
            _status.text = "正式概率、保底和卡池时间待产品确认";
            RefreshWallet();
            RefreshHistory();
        }

        private void RefreshHistory()
        {
            if (_history == null) return;
            var entries = ClientServices.Data.GetGachaHistory();
            _history.text = entries.Count == 0 ? "暂无抽取记录" : "抽取记录：" + entries.Count + " 次";
        }

        private void RefreshWallet()
        {
            ClientGachaPool pool = ClientServices.Data.GetGachaPool();
            _currency.text = "钻石：" + ClientServices.Data.GetWallet().GetBalance(pool.CostCurrency);
        }

        private static ClientHero FindHero(string heroId)
        {
            foreach (ClientHero hero in ClientServices.Data.GetHeroes())
                if (hero.HeroId == heroId)
                    return hero;
            return null;
        }
    }
}
