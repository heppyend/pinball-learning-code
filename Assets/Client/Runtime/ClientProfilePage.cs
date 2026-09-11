using System;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>
    /// 个人中心的数据绑定与局部交互。视觉节点由 ClientShell 场景维护。
    /// </summary>
    public sealed class ClientProfilePage : MonoBehaviour
    {
        public static event Action HomeShowcaseRequested;
        public static event Action BadgeRequested;
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text _playerName;
        [SerializeField] private Text _playerId;
        [SerializeField] private Text _combatPower;
        [SerializeField] private Text _notice;
        [SerializeField] private GameObject _renameDialog;
        [SerializeField] private InputField _renameInput;
        private bool _canBindData;

        private void OnEnable()
        {
            if (_canBindData)
                BindData();
        }

        private void Start()
        {
            // Start 在场景内全部 Awake 完成后调用，避免依赖根节点的启用顺序。
            _canBindData = true;
            BindData();
            BindPageActions();
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

        public void Configure(
            GameObject pageRoot,
            Text playerName,
            Text playerId,
            Text combatPower,
            Text notice,
            GameObject renameDialog,
            InputField renameInput)
        {
            _pageRoot = pageRoot;
            _playerName = playerName;
            _playerId = playerId;
            _combatPower = combatPower;
            _notice = notice;
            _renameDialog = renameDialog;
            _renameInput = renameInput;
        }

        public void Show()
        {
            if (_pageRoot != null)
                _pageRoot.SetActive(true);
        }

        public void Hide()
        {
            if (_renameDialog != null)
                _renameDialog.SetActive(false);

            if (_pageRoot != null)
                _pageRoot.SetActive(false);
        }

        public void OpenRenameDialog()
        {
            if (_renameDialog == null || _renameInput == null)
                return;

            _renameInput.text = ClientServices.Data.GetProfile().DisplayName;
            _renameDialog.SetActive(true);
        }

        public void OpenHomeShowcase()
        {
            HomeShowcaseRequested?.Invoke();
        }

        public void OpenBadge() { BadgeRequested?.Invoke(); }

        private void BindPageActions()
        {
            BindButton("HomeDisplayButton", OpenHomeShowcase);
            BindButton("PersonalizeButton", OpenBadge);
        }

        private void BindButton(string name, UnityEngine.Events.UnityAction action)
        {
            Transform target = transform.Find(name);
            if (target == null || target.GetComponent<Button>() == null)
                return;
            Button button = target.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(action);
        }

        public void CancelRename()
        {
            if (_renameDialog != null)
                _renameDialog.SetActive(false);
        }

        public void ConfirmRename()
        {
            if (_renameInput == null)
                return;

            if (!ClientServices.Data.TryRename(_renameInput.text))
            {
                ShowNotice("昵称不能为空");
                return;
            }

            CancelRename();
            ShowNotice("昵称已更新（本地模拟）");
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
            if (_playerName == null || _playerId == null || _combatPower == null)
                return;

            ClientPlayerProfile profile = ClientServices.Data.GetProfile();
            _playerName.text = profile.DisplayName;
            _playerId.text = "玩家ID: " + profile.PlayerId;
            _combatPower.text = profile.CombatPower.ToString();
        }
    }
}
