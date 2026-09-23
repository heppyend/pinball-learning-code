using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>开发期任务页：本地任务进度和奖励领取，不包含服务器日/周重置。</summary>
    public sealed class ClientTaskPage : ClientPageViewBase
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text[] _taskLabels;
        [SerializeField] private Text _detail;
        [SerializeField] private Text _status;
        private readonly List<ClientTask> _tasks = new List<ClientTask>();
        private int _selectedIndex;
        private bool _claimedOnly;

        protected override GameObject PageRoot { get { return _pageRoot; } }
        protected override void OnPageRefresh() { Show(); }

        public void Configure(GameObject pageRoot, Text[] taskLabels, Text detail, Text status)
        {
            _pageRoot = pageRoot;
            _taskLabels = taskLabels;
            _detail = detail;
            _status = status;
        }

        public void Show() { _pageRoot.SetActive(true); Refresh(); }

        public void SelectTask(int index)
        {
            if (index < 0 || index >= _tasks.Count)
                return;
            _selectedIndex = index;
            ClientTask task = _tasks[index];
            _detail.text = task.Title + "\n\n" + task.Description + "\n进度 " + task.Progress + "/" + task.Target + "    奖励：养成材料 × " + task.RewardQuantity;
            _status.text = task.IsClaimed ? "奖励已领取" : (task.Progress >= task.Target ? "可领取奖励" : "任务未完成");
        }

        public void ClaimSelected()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _tasks.Count)
                return;
            string failureReason;
            if (!ClientServices.Data.TryClaimTask(_tasks[_selectedIndex].TaskId, out failureReason))
            {
                _status.text = failureReason;
                return;
            }
            Refresh();
            SelectTask(_selectedIndex);
        }

        public void ShowAvailable() { _claimedOnly = false; Refresh(); }
        public void ShowClaimed() { _claimedOnly = true; Refresh(); }

        private void Refresh()
        {
            _tasks.Clear();
            foreach (ClientTask task in ClientServices.Data.GetTasks())
                if (!_claimedOnly || task.IsClaimed)
                    _tasks.Add(task);
            for (int index = 0; index < _taskLabels.Length; index++)
            {
                bool exists = index < _tasks.Count;
                _taskLabels[index].transform.parent.gameObject.SetActive(exists);
                if (exists)
                {
                    ClientTask task = _tasks[index];
                    _taskLabels[index].text = task.Title + "\n" + task.Progress + "/" + task.Target + (task.IsClaimed ? "  已领取" : "");
                }
            }
            if (_tasks.Count == 0)
            {
                _detail.text = "暂无任务";
                _status.text = "任务列表待服务端配置";
                return;
            }
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _tasks.Count - 1);
            SelectTask(_selectedIndex);
        }
    }
}
