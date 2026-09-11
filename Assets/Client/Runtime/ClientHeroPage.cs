using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public sealed class ClientHeroPage : MonoBehaviour
    {
        public static event System.Action<ClientHero> HeroDetailRequested;
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private GameObject _ownedCard;
        [SerializeField] private GameObject _lockedCard;
        [SerializeField] private Text _status;

        public void Configure(GameObject pageRoot, GameObject ownedCard, GameObject lockedCard, Text status)
        {
            _pageRoot = pageRoot;
            _ownedCard = ownedCard;
            _lockedCard = lockedCard;
            _status = status;
        }

        public void Show() { _pageRoot.SetActive(true); SetElementFilter("All"); }
        public void Hide() { _pageRoot.SetActive(false); }

        public void SetElementFilter(string element)
        {
            bool showOwned = element == "All" || element == "Fire";
            bool showLocked = element == "All" || element == "Water";
            _ownedCard.SetActive(showOwned);
            _lockedCard.SetActive(showLocked);
            RefreshCardOwnership();
            _status.text = element == "All" ? "全部英雄" : element + " 属性英雄";
        }

        public void SelectOwnedHero()
        {
            ClientHero hero = ClientServices.Data.GetHeroes()[0];
            HeroDetailRequested?.Invoke(hero);
        }

        public void SelectLockedHero()
        {
            ClientHero hero = ClientServices.Data.GetHeroes()[1];
            if (hero.IsOwned)
            {
                HeroDetailRequested?.Invoke(hero);
                return;
            }
            _status.text = "该英雄尚未拥有";
        }

        private void RefreshCardOwnership()
        {
            if (_lockedCard == null || ClientServices.Data.GetHeroes().Count < 2)
                return;
            ClientHero hero = ClientServices.Data.GetHeroes()[1];
            Transform overlay = _lockedCard.transform.Find("LockedOverlay");
            if (overlay != null)
                overlay.gameObject.SetActive(!hero.IsOwned);
            Transform lockedLabel = _lockedCard.transform.Find("LockedLabel");
            if (lockedLabel != null)
                lockedLabel.gameObject.SetActive(!hero.IsOwned);
            Transform name = _lockedCard.transform.Find("Name");
            if (name != null && name.GetComponent<Text>() != null)
                name.GetComponent<Text>().text = hero.IsOwned ? hero.Name : "未拥有英雄";
        }
    }
}
