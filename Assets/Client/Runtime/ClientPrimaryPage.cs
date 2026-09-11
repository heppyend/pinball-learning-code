using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>主页一级入口的独立页面控制器；业务规则将在各页面资料确认后替换。</summary>
    public sealed class ClientPrimaryPage : MonoBehaviour
    {
        [SerializeField] private ClientHomeDestination _destination;
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text _title;
        [SerializeField] private Text _status;

        public ClientHomeDestination Destination { get { return _destination; } }

        public void Configure(ClientHomeDestination destination, GameObject pageRoot, Text title, Text status)
        {
            _destination = destination;
            _pageRoot = pageRoot;
            _title = title;
            _status = status;
        }

        public void Show()
        {
            _pageRoot.SetActive(true);
            _title.text = GetTitle(_destination);
            _status.text = GetTitle(_destination) + "入口已完成\n具体数据与规则待接入";
        }

        public void Hide()
        {
            if (_pageRoot != null)
                _pageRoot.SetActive(false);
        }

        private static string GetTitle(ClientHomeDestination destination)
        {
            switch (destination)
            {
                case ClientHomeDestination.Gacha: return "扭蛋";
                case ClientHomeDestination.Shop: return "商城";
                case ClientHomeDestination.Mail: return "邮箱";
                case ClientHomeDestination.Rank: return "排行榜";
                case ClientHomeDestination.Activity: return "活动";
                case ClientHomeDestination.Task: return "任务";
                default: return "公告";
            }
        }
    }
}
