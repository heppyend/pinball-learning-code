using System.Collections.Generic;
using UnityEngine;

namespace Pinball.Client.UI
{
    public sealed class ClientUiNavigator : MonoBehaviour
    {
        [SerializeField] private List<ClientUiPage> _pages = new List<ClientUiPage>();
        private readonly Dictionary<ClientUiPageId, ClientUiPage> _pageById = new Dictionary<ClientUiPageId, ClientUiPage>();
        private readonly Stack<ClientUiPageId> _history = new Stack<ClientUiPageId>();
        private ClientUiPageId _currentPage = ClientUiPageId.Home;

        private void Awake()
        {
            foreach (ClientUiPage page in _pages)
                if (page != null)
                    _pageById[page.PageId] = page;
        }

        private void OnEnable()
        {
            ClientHomePage.NavigationRequested += OpenFromHome;
            ClientProfilePage.HomeShowcaseRequested += OpenHomeShowcase;
            ClientProfilePage.BadgeRequested += OpenBadge;
            ClientHeroPage.HeroDetailRequested += OpenHeroDetail;
            ClientHeroDetailPage.UpgradeRequested += OpenHeroEnhance;
        }

        private void OnDisable()
        {
            ClientHomePage.NavigationRequested -= OpenFromHome;
            ClientProfilePage.HomeShowcaseRequested -= OpenHomeShowcase;
            ClientProfilePage.BadgeRequested -= OpenBadge;
            ClientHeroPage.HeroDetailRequested -= OpenHeroDetail;
            ClientHeroDetailPage.UpgradeRequested -= OpenHeroEnhance;
        }

        public void Configure(List<ClientUiPage> pages)
        {
            _pages = pages;
            _pageById.Clear();
            foreach (ClientUiPage page in pages)
                if (page != null)
                    _pageById[page.PageId] = page;
        }

        public void Register(ClientUiPage page)
        {
            if (page == null)
                return;
            if (!_pages.Contains(page))
                _pages.Add(page);
            _pageById[page.PageId] = page;
        }

        public void Open(ClientUiPageId pageId)
        {
            if (pageId == _currentPage || !_pageById.ContainsKey(pageId) || !_pageById.ContainsKey(_currentPage))
                return;
            _pageById[_currentPage].Pause();
            _history.Push(_currentPage);
            _currentPage = pageId;
            _pageById[_currentPage].Enter();
            RefreshPage(_currentPage);
        }

        public void Back()
        {
            if (_history.Count == 0)
            {
                // 域重载或从 Inspector 直接激活页面时，返回栈可能为空；此时仍应
                // 允许用户回到主页，而不是留下无响应的返回按钮。
                if (_currentPage != ClientUiPageId.Home &&
                    _pageById.TryGetValue(_currentPage, out ClientUiPage activePage) &&
                    _pageById.TryGetValue(ClientUiPageId.Home, out ClientUiPage homePage))
                {
                    activePage.Exit();
                    _currentPage = ClientUiPageId.Home;
                    homePage.Resume();
                    RefreshPage(_currentPage);
                }
                return;
            }

            if (!_pageById.TryGetValue(_currentPage, out ClientUiPage currentPage))
                return;

            currentPage.Exit();
            _currentPage = _history.Pop();
            if (!_pageById.TryGetValue(_currentPage, out ClientUiPage previousPage))
                return;
            previousPage.Resume();
            RefreshPage(_currentPage);
        }

        /// <summary>用于一级页面的明确“返回主页”操作，不保留多余的导航历史。</summary>
        public void ReturnHome()
        {
            if (!_pageById.TryGetValue(ClientUiPageId.Home, out ClientUiPage homePage))
                return;
            if (_currentPage != ClientUiPageId.Home && _pageById.TryGetValue(_currentPage, out ClientUiPage currentPage))
                currentPage.Exit();
            _history.Clear();
            _currentPage = ClientUiPageId.Home;
            homePage.Resume();
            RefreshPage(_currentPage);
        }

        private void OpenFromHome(ClientHomeDestination destination)
        {
            Open(Map(destination));
        }

        private void OpenHomeShowcase() { Open(ClientUiPageId.HomeShowcase); }
        private void OpenBadge() { Open(ClientUiPageId.Badge); }

        private void OpenHeroDetail(Pinball.Client.Domain.ClientHero hero)
        {
            Open(ClientUiPageId.HeroDetail);
            ClientHeroDetailPage detail = _pageById[ClientUiPageId.HeroDetail].GetComponent<ClientHeroDetailPage>();
            if (detail != null)
                detail.Show(hero);
        }
        private void OpenHeroEnhance(Pinball.Client.Domain.ClientHero hero)
        {
            Open(ClientUiPageId.HeroEnhance);
            ClientHeroEnhancePage page = _pageById[ClientUiPageId.HeroEnhance].GetComponent<ClientHeroEnhancePage>();
            if (page != null) page.Show(hero);
        }

        private void RefreshPage(ClientUiPageId pageId)
        {
            if (pageId == ClientUiPageId.Profile)
                _pageById[pageId].GetComponent<ClientProfilePage>()?.Show();
            else if (pageId == ClientUiPageId.Hero)
                _pageById[pageId].GetComponent<ClientHeroPage>()?.Show();
            else if (pageId == ClientUiPageId.Backpack)
                _pageById[pageId].GetComponent<ClientBackpackPage>()?.Show();
            else if (pageId == ClientUiPageId.Badge)
                _pageById[pageId].GetComponent<ClientBadgePage>()?.Show();
            else if (pageId == ClientUiPageId.Gacha)
                _pageById[pageId].GetComponent<ClientGachaPage>()?.Show();
            else if (pageId == ClientUiPageId.Shop)
                _pageById[pageId].GetComponent<ClientShopPage>()?.Show();
            else if (pageId == ClientUiPageId.Mail)
                _pageById[pageId].GetComponent<ClientMailPage>()?.Show();
            else if (pageId == ClientUiPageId.Rank)
                _pageById[pageId].GetComponent<ClientRankPage>()?.Show();
            else if (pageId == ClientUiPageId.Activity)
                _pageById[pageId].GetComponent<ClientActivityPage>()?.Show();
            else if (pageId == ClientUiPageId.Task)
                _pageById[pageId].GetComponent<ClientTaskPage>()?.Show();
            else if (pageId == ClientUiPageId.Notice)
                _pageById[pageId].GetComponent<ClientNoticePage>()?.Show();
            else if (pageId == ClientUiPageId.Formation)
                _pageById[pageId].GetComponent<ClientFormationPage>()?.Show();
            else if (pageId == ClientUiPageId.Marble)
                _pageById[pageId].GetComponent<ClientMarblePage>()?.Show();
            else if (pageId == ClientUiPageId.Home)
                _pageById[pageId].GetComponent<ClientHomePage>()?.SetVisible(true);
            else
                _pageById[pageId].GetComponent<ClientPrimaryPage>()?.Show();
        }

        private static ClientUiPageId Map(ClientHomeDestination destination)
        {
            switch (destination)
            {
                case ClientHomeDestination.Profile: return ClientUiPageId.Profile;
                case ClientHomeDestination.Hero: return ClientUiPageId.Hero;
                case ClientHomeDestination.Backpack: return ClientUiPageId.Backpack;
                case ClientHomeDestination.Gacha: return ClientUiPageId.Gacha;
                case ClientHomeDestination.Shop: return ClientUiPageId.Shop;
                case ClientHomeDestination.Mail: return ClientUiPageId.Mail;
                case ClientHomeDestination.Rank: return ClientUiPageId.Rank;
                case ClientHomeDestination.Activity: return ClientUiPageId.Activity;
                case ClientHomeDestination.Task: return ClientUiPageId.Task;
                case ClientHomeDestination.Formation: return ClientUiPageId.Formation;
                case ClientHomeDestination.Marble: return ClientUiPageId.Marble;
                default: return ClientUiPageId.Notice;
            }
        }
    }
}
