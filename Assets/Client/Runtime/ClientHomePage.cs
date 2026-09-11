using System;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public enum ClientHomeDestination
    {
        Profile,
        Hero,
        Backpack,
        Shop,
        Mail,
        Rank,
        Activity,
        Task,
        Gacha,
        Notice
        ,Formation
        ,Marble
    }

    /// <summary>
    /// 主页面的数据绑定与导航入口。页面层级由 ClientShellUiBuilder 在编辑器中生成。
    /// </summary>
    public sealed class ClientHomePage : MonoBehaviour
    {
        public static event Action<ClientHomeDestination> NavigationRequested;

        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text _playerName;
        [SerializeField] private Text _combatPower;
        [SerializeField] private Text _notice;
        [SerializeField] private ClientBottomNavigationBar _bottomNavigation;
        private bool _canBindData;

        private void OnEnable()
        {
            if (_canBindData)
            {
                BindData();
                BindSceneNavigation();
                SelectBottomHome();
            }
        }

        private void Start()
        {
            // Start 在场景内全部 Awake 完成后调用，避免依赖根节点的启用顺序。
            _canBindData = true;
            BindData();
            BindSceneNavigation();
            SelectBottomHome();
        }

        private void BindSceneNavigation()
        {
            BindPrefabNavigation("TeamButton", ClientHomeDestination.Formation);
            BindPrefabNavigation("GachaButton", ClientHomeDestination.Gacha);
            BindPrefabNavigation("AvatarButton", ClientHomeDestination.Profile);
            BindPrefabNavigation("ProfileButton", ClientHomeDestination.Profile);
            BindPrefabNavigation("MarbleButton", ClientHomeDestination.Marble);
            BindPrefabNavigation("BackpackButton", ClientHomeDestination.Backpack);
            BindPrefabNavigation("ActivityButton", ClientHomeDestination.Activity);
            BindBottomHomeButton();
            BindPrefabNavigation("RankButton", ClientHomeDestination.Rank);
            BindPrefabNavigation("ShopButton", ClientHomeDestination.Shop);
            BindPrefabNavigation("BottomFunctionIcon_Shop", ClientHomeDestination.Shop);
            BindPrefabNavigation("MailButton", ClientHomeDestination.Mail);
            BindPrefabNavigation("SupportButton", ClientHomeDestination.Notice);
            BindPrefabNavigation("ScanButton", ClientHomeDestination.Notice);
        }

        private void OnDisable()
        {
            if (!_canBindData)
                return;

            try
            {
                ClientServices.Data.DataChanged -= Refresh;
            }
            catch (InvalidOperationException)
            {
                // 编辑器域重载时服务可能已释放，无需影响页面销毁。
            }
        }

        private void BindData()
        {
            ClientServices.Data.DataChanged -= Refresh;
            ClientServices.Data.DataChanged += Refresh;
            Refresh();
        }

        private void BindPrefabNavigation(string buttonName, ClientHomeDestination destination)
        {
            foreach (Button button in _pageRoot.GetComponentsInChildren<Button>(true))
            {
                if (button.name != buttonName)
                    continue;

                // 场景预制体可能保留旧导航器的回调。每次主页重新显示时只接入
                // 当前 Canvas 页面栈，避免旧回调吞掉或并行处理点击。
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(() => RequestNavigation(destination));
                return;
            }

            Debug.LogWarning("[Client] MainPage 中未找到 " + buttonName + "，未绑定导航。");
        }

        public void Configure(GameObject pageRoot, Text playerName, Text combatPower, Text notice)
        {
            _pageRoot = pageRoot;
            _playerName = playerName;
            _combatPower = combatPower;
            _notice = notice;
        }

        public void RequestNavigation(ClientHomeDestination destination)
        {
            if (_bottomNavigation != null)
            {
                if (destination == ClientHomeDestination.Profile) _bottomNavigation.Select("ProfileButton");
                else if (destination == ClientHomeDestination.Marble) _bottomNavigation.Select("MarbleButton");
                else if (destination == ClientHomeDestination.Backpack) _bottomNavigation.Select("BackpackButton");
                else if (destination == ClientHomeDestination.Activity) _bottomNavigation.Select("ActivityButton");
            }
            NavigationRequested?.Invoke(destination);
        }

        public void ConfigureBottomNavigation(ClientBottomNavigationBar bottomNavigation) { _bottomNavigation = bottomNavigation; }

        public void SetVisible(bool visible)
        {
            if (_pageRoot != null)
                _pageRoot.SetActive(visible);
        }

        public void ShowNotice(string message)
        {
            if (_notice != null)
            {
                _notice.text = message;
                _notice.gameObject.SetActive(true);
            }

            Debug.Log("[Client] " + message);
        }

        private void Refresh()
        {
            if (_playerName == null || _combatPower == null)
                return;

            ClientPlayerProfile profile = ClientServices.Data.GetProfile();
            _playerName.text = profile.DisplayName;
            _combatPower.text = profile.CombatPower.ToString();
        }

        private void BindBottomHomeButton()
        {
            foreach (Button button in _pageRoot.GetComponentsInChildren<Button>(true))
            {
                if (button.name != "HomeButton") continue;
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(() => { SelectBottomHome(); ShowNotice("当前已在主页"); });
                return;
            }
        }

        private void SelectBottomHome()
        {
            if (_bottomNavigation != null) _bottomNavigation.Select("HomeButton");
        }

        private static string GetDestinationLabel(ClientHomeDestination destination)
        {
            switch (destination)
            {
                case ClientHomeDestination.Hero: return "英雄";
                case ClientHomeDestination.Backpack: return "背包";
                case ClientHomeDestination.Shop: return "商城";
                case ClientHomeDestination.Mail: return "邮箱";
                case ClientHomeDestination.Rank: return "排行榜";
                case ClientHomeDestination.Activity: return "活动";
                case ClientHomeDestination.Task: return "任务";
                case ClientHomeDestination.Gacha: return "扭蛋";
                case ClientHomeDestination.Notice: return "公告";
                default: return "客户端";
            }
        }
    }
}
