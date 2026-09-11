using Pinball.Client.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public sealed class ClientHeroDetailPage : MonoBehaviour
    {
        public static event System.Action<ClientHero> UpgradeRequested;
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text _name;
        [SerializeField] private Text _level;
        [SerializeField] private Text _combatPower;
        [SerializeField] private Text _element;
        [SerializeField] private Text _starLevel;
        [SerializeField] private Text _ownership;
        private ClientHero _currentHero;

        public void Configure(GameObject root, Text name, Text level, Text combatPower)
        { _pageRoot = root; _name = name; _level = level; _combatPower = combatPower; }

        public void ConfigureDetails(Text element, Text starLevel, Text ownership)
        { _element = element; _starLevel = starLevel; _ownership = ownership; }

        public void Show(ClientHero hero)
        {
            if (_pageRoot == null || _name == null || _level == null || _combatPower == null)
            {
                Debug.LogError("[Client] 英雄详情页引用未配置。");
                return;
            }

            _pageRoot.SetActive(true);
            _currentHero = hero;
            _name.text = hero.Name;
            _level.text = "Lv." + hero.Level;
            _combatPower.text = "战力 " + hero.CombatPower;
            if (_element != null) _element.text = "属性：" + hero.Element;
            if (_starLevel != null) _starLevel.text = "星级：" + hero.StarLevel;
            if (_ownership != null) _ownership.text = hero.IsOwned ? "已拥有" : "未拥有";
        }

        public void RequestUpgrade()
        {
            if (_currentHero != null)
                UpgradeRequested?.Invoke(_currentHero);
        }

        public void Hide()
        {
            if (_pageRoot != null)
                _pageRoot.SetActive(false);
        }
    }
}
