using System;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public sealed class ClientHomeShowcasePage : ClientPageViewBase
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Image _heroVisual;
        [SerializeField] private Text _heroName;
        [SerializeField] private Text _notice;
        [SerializeField] private Sprite _hero001Visual;
        [SerializeField] private Sprite _hero002Visual;
        private int _selectedHeroId;
        private bool _canBindData;

        protected override GameObject PageRoot { get { return _pageRoot; } }
        protected override void OnPageRefresh() { Refresh(); }
        protected override void OnPageHidden() { Hide(); }

        private void OnEnable()
        {
            if (_canBindData)
                Refresh();
        }

        private void Start()
        {
            _canBindData = true;
            Refresh();
        }

        public void Configure(GameObject pageRoot, Image heroVisual, Text heroName, Text notice, Sprite hero001Visual, Sprite hero002Visual)
        {
            _pageRoot = pageRoot;
            _heroVisual = heroVisual;
            _heroName = heroName;
            _notice = notice;
            _hero001Visual = hero001Visual;
            _hero002Visual = hero002Visual;
        }

        public void Show() {
            // 2026-09-21（负责人决定：其余列表页也接入）：竖向列表撑高 / Clamped / 顶部对齐。
            ClientScrollFix.FixAll(gameObject, true); _pageRoot.SetActive(true); }
        public void Hide() { _pageRoot.SetActive(false); }

        public void SelectHero(int heroId)
        {
            ClientHero hero = FindHero(heroId);
            if (hero == null || !hero.IsOwned)
            {
                ShowNotice("该英雄尚未拥有");
                return;
            }

            _selectedHeroId = hero.HeroId;
            RefreshPreview(hero);
        }

        public void ConfirmSelection()
        {
            if (!ClientServices.Data.TrySetShowcaseHero(_selectedHeroId))
            {
                ShowNotice("展示英雄设置失败");
                return;
            }

            ShowNotice("主页展示已更新（本地模拟）");
        }

        public void ShowNotice(string message)
        {
            _notice.text = message;
            _notice.gameObject.SetActive(true);
        }

        private void Refresh()
        {
            _selectedHeroId = ClientServices.Data.GetShowcaseHeroId();
            ClientHero hero = FindHero(_selectedHeroId);
            if (hero != null)
                RefreshPreview(hero);
        }

        private void RefreshPreview(ClientHero hero)
        {
            _heroName.text = hero.Name + "  Lv." + hero.Level;
            _heroVisual.sprite = _hero001Visual;   // 【待接入】按 hero.Portrait1 名字加载真实立绘（HeroPortrait_<id>_c）；当前统一占位图
            _heroVisual.preserveAspect = true;
        }

        private static ClientHero FindHero(int heroId)
        {
            foreach (ClientHero hero in ClientServices.Data.GetHeroes())
            {
                if (hero.HeroId == heroId)
                    return hero;
            }

            return null;
        }
    }
}
