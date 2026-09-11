using UnityEngine;
using System.Collections.Generic;

namespace Pinball.Client
{
    public sealed class ClientPageNavigator : MonoBehaviour
    {
        [SerializeField] private ClientHomePage _homePage;
        [SerializeField] private ClientProfilePage _profilePage;
        [SerializeField] private ClientHomeShowcasePage _homeShowcasePage;
        [SerializeField] private ClientFeaturePage _featurePage;
        [SerializeField] private ClientHeroPage _heroPage;
        [SerializeField] private ClientHeroDetailPage _heroDetailPage;
        [SerializeField] private ClientBackpackPage _backpackPage;
        [SerializeField] private List<ClientPrimaryPage> _primaryPages = new List<ClientPrimaryPage>();

        private void Awake()
        {
            if (_homePage == null)
                _homePage = GetComponent<ClientHomePage>();

            if (_profilePage == null)
                _profilePage = GetComponent<ClientProfilePage>();
        }

        private void OnEnable()
        {
            ClientHomePage.NavigationRequested += HandleHomeNavigation;
            ClientProfilePage.HomeShowcaseRequested += OpenHomeShowcase;
            ClientHeroPage.HeroDetailRequested += OpenHeroDetail;
        }

        private void OnDisable()
        {
            ClientHomePage.NavigationRequested -= HandleHomeNavigation;
            ClientProfilePage.HomeShowcaseRequested -= OpenHomeShowcase;
            ClientHeroPage.HeroDetailRequested -= OpenHeroDetail;
        }

        private void HandleHomeNavigation(ClientHomeDestination destination)
        {
            if (_homePage == null || _profilePage == null)
            {
                Debug.LogError("[Client] ClientShell 页面引用未配置。请执行 Client/Build ClientShell Visual UI。");
                return;
            }

            _homePage.SetVisible(false);
            if (destination == ClientHomeDestination.Profile)
                _profilePage.Show();
            else if (destination == ClientHomeDestination.Hero && _heroPage != null)
                _heroPage.Show();
            else if (destination == ClientHomeDestination.Backpack && _backpackPage != null)
                _backpackPage.Show();
            else if (ShowPrimaryPage(destination))
                return;
            else if (_featurePage != null)
                _featurePage.Show(destination);
        }

        public void Configure(ClientHomePage homePage, ClientProfilePage profilePage, ClientHomeShowcasePage homeShowcasePage, ClientFeaturePage featurePage)
        {
            _homePage = homePage;
            _profilePage = profilePage;
            _homeShowcasePage = homeShowcasePage;
            _featurePage = featurePage;
        }

        public void ReturnToHome()
        {
            if (_homePage == null || _profilePage == null)
                return;

            _profilePage.Hide();
            _homePage.SetVisible(true);
        }

        public void ReturnFromFeature()
        {
            if (_featurePage != null)
                _featurePage.Hide();
            _homePage.SetVisible(true);
        }

        public void ConfigureHeroPage(ClientHeroPage heroPage) { _heroPage = heroPage; }

        public void ReturnFromHero()
        {
            if (_heroPage != null)
                _heroPage.Hide();
            _homePage.SetVisible(true);
        }

        public void ConfigureHeroDetailPage(ClientHeroDetailPage detailPage) { _heroDetailPage = detailPage; }
        public void ConfigureBackpackPage(ClientBackpackPage backpackPage) { _backpackPage = backpackPage; }
        public void ConfigurePrimaryPages(List<ClientPrimaryPage> pages) { _primaryPages = pages; }

        public void ReturnFromPrimary()
        {
            foreach (ClientPrimaryPage page in _primaryPages)
                if (page != null)
                    page.Hide();
            _homePage.SetVisible(true);
        }

        public void ReturnFromBackpack()
        {
            if (_backpackPage != null)
                _backpackPage.Hide();
            _homePage.SetVisible(true);
        }

        private bool ShowPrimaryPage(ClientHomeDestination destination)
        {
            foreach (ClientPrimaryPage page in _primaryPages)
            {
                if (page == null || page.Destination != destination)
                    continue;
                page.Show();
                return true;
            }
            return false;
        }

        private void OpenHeroDetail(Pinball.Client.Domain.ClientHero hero)
        {
            if (_heroPage == null || _heroDetailPage == null)
                return;
            _heroPage.Hide();
            _heroDetailPage.Show(hero);
        }

        public void ReturnFromHeroDetail()
        {
            if (_heroDetailPage != null)
                _heroDetailPage.Hide();
            _heroPage.Show();
        }

        private void OpenHomeShowcase()
        {
            if (_profilePage == null || _homeShowcasePage == null)
                return;

            _profilePage.Hide();
            _homeShowcasePage.Show();
        }

        public void ReturnToProfile()
        {
            if (_profilePage == null || _homeShowcasePage == null)
                return;

            _homeShowcasePage.Hide();
            _profilePage.Show();
        }
    }
}
