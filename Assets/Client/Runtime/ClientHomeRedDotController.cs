using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>主页入口红点，统一由本地服务数据刷新。</summary>
    public sealed class ClientHomeRedDotController : MonoBehaviour
    {
        [SerializeField] private Text _mailDot;
        [SerializeField] private Text _activityDot;
        [SerializeField] private Text _noticeDot;

        private void Start()
        {
            ClientServices.Data.DataChanged += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            try { ClientServices.Data.DataChanged -= Refresh; }
            catch (System.InvalidOperationException) { }
        }

        public void Configure(Text mailDot, Text activityDot, Text noticeDot)
        {
            _mailDot = mailDot;
            _activityDot = activityDot;
            _noticeDot = noticeDot;
        }

        private void Refresh()
        {
            bool hasMail = false;
            foreach (ClientMail mail in ClientServices.Data.GetMails()) if (!mail.IsRead || !mail.IsClaimed) { hasMail = true; break; }
            bool hasActivity = false;
            foreach (ClientActivity activity in ClientServices.Data.GetActivities()) if (activity.IsOpen && !activity.IsClaimed && activity.Progress >= activity.Target) { hasActivity = true; break; }
            bool hasNotice = false;
            foreach (ClientNotice notice in ClientServices.Data.GetNotices()) if (!notice.IsRead) { hasNotice = true; break; }
            SetVisible(_mailDot, hasMail); SetVisible(_activityDot, hasActivity); SetVisible(_noticeDot, hasNotice);
        }

        private static void SetVisible(Text dot, bool visible)
        {
            if (dot != null) dot.gameObject.SetActive(visible);
        }
    }
}
