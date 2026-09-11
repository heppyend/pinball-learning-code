using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>开发期公告页：使用本地公告数据，不连接运营后台。</summary>
    public sealed class ClientNoticePage : MonoBehaviour
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text[] _noticeLabels;
        [SerializeField] private Text _detail;
        [SerializeField] private GameObject _emptyHint;
        private readonly List<ClientNotice> _notices = new List<ClientNotice>();
        private bool _unreadOnly;

        public void Configure(GameObject pageRoot, Text[] noticeLabels, Text detail, GameObject emptyHint)
        {
            _pageRoot = pageRoot;
            _noticeLabels = noticeLabels;
            _detail = detail;
            _emptyHint = emptyHint;
        }

        public void Show()
        {
            _pageRoot.SetActive(true);
            Refresh();
        }

        public void SelectNotice(int index)
        {
            if (index < 0 || index >= _notices.Count)
                return;
            ClientNotice notice = _notices[index];
            ClientServices.Data.TryReadNotice(notice.NoticeId);
            _detail.text = notice.Title + "\n\n" + notice.Content;
            RefreshLabels();
        }

        public void ShowAll() { _unreadOnly = false; Refresh(); }
        public void ShowUnread() { _unreadOnly = true; Refresh(); }

        private void Refresh()
        {
            _notices.Clear();
            foreach (ClientNotice notice in ClientServices.Data.GetNotices())
                if (!_unreadOnly || !notice.IsRead)
                    _notices.Add(notice);
            _emptyHint.SetActive(_notices.Count == 0);
            RefreshLabels();
            if (_notices.Count > 0)
                SelectNotice(0);
            else
                _detail.text = "暂无公告";
        }

        private void RefreshLabels()
        {
            for (int index = 0; index < _noticeLabels.Length; index++)
            {
                bool exists = index < _notices.Count;
                _noticeLabels[index].transform.parent.gameObject.SetActive(exists);
                if (exists)
                    _noticeLabels[index].text = (_notices[index].IsRead ? "" : "● ") + _notices[index].Title;
            }
        }
    }
}
