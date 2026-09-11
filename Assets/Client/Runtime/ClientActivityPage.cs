using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>开发期活动页：本地活动进度和奖励领取，不包含运营日历或服务器状态。</summary>
    public sealed class ClientActivityPage : MonoBehaviour
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text[] _activityLabels;
        [SerializeField] private Text _detail;
        [SerializeField] private Text _status;
        private readonly List<ClientActivity> _activities = new List<ClientActivity>();
        private int _selectedIndex;
        private bool _claimedOnly;

        public void Configure(GameObject pageRoot, Text[] activityLabels, Text detail, Text status)
        {
            _pageRoot = pageRoot;
            _activityLabels = activityLabels;
            _detail = detail;
            _status = status;
        }

        public void Show()
        {
            _pageRoot.SetActive(true);
            Refresh();
        }

        public void SelectActivity(int index)
        {
            if (index < 0 || index >= _activities.Count)
                return;
            _selectedIndex = index;
            ClientActivity activity = _activities[index];
            _detail.text = activity.Title + "\n\n" + activity.Description + "\n进度 " + activity.Progress + "/" + activity.Target + "    奖励：养成材料 × " + activity.RewardQuantity;
            _status.text = activity.IsClaimed ? "奖励已领取" : (activity.Progress >= activity.Target ? "可领取奖励" : "进度未完成");
        }

        public void ClaimSelected()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _activities.Count)
                return;
            string failureReason;
            if (!ClientServices.Data.TryClaimActivity(_activities[_selectedIndex].ActivityId, out failureReason))
            {
                _status.text = failureReason;
                return;
            }
            Refresh();
            SelectActivity(_selectedIndex);
        }

        public void ShowAvailable() { _claimedOnly = false; Refresh(); }
        public void ShowClaimed() { _claimedOnly = true; Refresh(); }

        private void Refresh()
        {
            _activities.Clear();
            foreach (ClientActivity activity in ClientServices.Data.GetActivities())
                if (activity.IsOpen && (!_claimedOnly || activity.IsClaimed))
                    _activities.Add(activity);
            for (int index = 0; index < _activityLabels.Length; index++)
            {
                bool exists = index < _activities.Count;
                _activityLabels[index].transform.parent.gameObject.SetActive(exists);
                if (!exists)
                    continue;
                ClientActivity activity = _activities[index];
                _activityLabels[index].text = activity.Title + "\n" + activity.Progress + "/" + activity.Target + (activity.IsClaimed ? "  已领取" : "");
            }
            if (_activities.Count == 0)
            {
                _detail.text = "当前没有进行中的活动";
                _status.text = "活动开启时间待服务端配置";
                return;
            }
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _activities.Count - 1);
            SelectActivity(_selectedIndex);
        }
    }
}
