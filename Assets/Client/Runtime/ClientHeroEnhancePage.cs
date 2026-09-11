using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>开发期英雄培养页：本地金币与材料升级，正式数值规则待确认。</summary>
    public sealed class ClientHeroEnhancePage : MonoBehaviour
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text _heroInfo;
        [SerializeField] private Text _costInfo;
        [SerializeField] private Text _status;
        private ClientHero _hero;

        public void Configure(GameObject root, Text heroInfo, Text costInfo, Text status)
        { _pageRoot = root; _heroInfo = heroInfo; _costInfo = costInfo; _status = status; }

        public void Show(ClientHero hero)
        {
            _hero = hero;
            _pageRoot.SetActive(true);
            Refresh();
        }

        public void Upgrade()
        {
            if (_hero == null) return;
            string failureReason;
            if (!ClientServices.Data.TryUpgradeHero(_hero.HeroId, out failureReason)) { _status.text = failureReason; return; }
            _status.text = "升级成功（本地模拟）";
            Refresh();
        }

        public void StarUp()
        {
            if (_hero == null) return;
            string failureReason;
            if (!ClientServices.Data.TryStarUpHero(_hero.HeroId, out failureReason)) { _status.text = failureReason; return; }
            _status.text = "升星成功（本地模拟）";
            Refresh();
        }

        private void Refresh()
        {
            _heroInfo.text = _hero.Name + "\nLv." + _hero.Level + "   ★" + _hero.StarLevel + "   战力 " + _hero.CombatPower;
            int nextLevel = Mathf.Min(10, _hero.Level + 1);
            int nextStar = Mathf.Min(5, _hero.StarLevel + 1);
            _costInfo.text = "升级：金币 50 / 材料 5  → Lv." + nextLevel + "\n升星：金币 100 / 材料 10  → ★" + nextStar + "\n本地上限：等级 10、星级 5";
            if (string.IsNullOrEmpty(_status.text)) _status.text = "正式升级公式与材料配置待确认";
        }
    }
}
