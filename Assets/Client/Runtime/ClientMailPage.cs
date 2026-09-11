using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>开发期邮箱：本地邮件、已读状态和附件领取，不包含真实邮件协议。</summary>
    public sealed class ClientMailPage : MonoBehaviour
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text[] _mailLabels;
        [SerializeField] private Text _detail;
        [SerializeField] private Text _status;
        [SerializeField] private GameObject _emptyHint;
        private readonly List<ClientMail> _mails = new List<ClientMail>();
        private int _selectedIndex;
        private bool _unreadOnly;

        public void Configure(GameObject pageRoot, Text[] mailLabels, Text detail, Text status, GameObject emptyHint)
        {
            _pageRoot = pageRoot;
            _mailLabels = mailLabels;
            _detail = detail;
            _status = status;
            _emptyHint = emptyHint;
        }

        public void Show()
        {
            _pageRoot.SetActive(true);
            Refresh();
        }

        public void SelectMail(int index)
        {
            if (index < 0 || index >= _mails.Count)
                return;
            _selectedIndex = index;
            ClientMail mail = _mails[index];
            ClientServices.Data.TryReadMail(mail.MailId);
            _detail.text = mail.Title + "\n\n" + mail.Content + "\n\n附件：养成材料 × " + mail.AttachmentQuantity;
            _status.text = mail.IsClaimed ? "附件已领取" : "点击领取附件";
            RefreshLabels();
        }

        public void ClaimSelected()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _mails.Count)
            {
                _status.text = "请选择邮件";
                return;
            }
            string failureReason;
            if (!ClientServices.Data.TryClaimMail(_mails[_selectedIndex].MailId, out failureReason))
            {
                _status.text = failureReason;
                return;
            }
            _status.text = "附件已加入本地背包";
            Refresh();
            SelectMail(_selectedIndex);
        }

        public void ClaimAll()
        {
            string failureReason;
            int count = ClientServices.Data.ClaimAllMails(out failureReason);
            _status.text = count > 0 ? "已批量领取 " + count + " 封邮件附件" : failureReason;
            ClientUiFeedback.ShowToast(_status.text);
            Refresh();
        }

        public void ShowAll() { _unreadOnly = false; Refresh(); }
        public void ShowUnread() { _unreadOnly = true; Refresh(); }

        private void Refresh()
        {
            _mails.Clear();
            foreach (ClientMail mail in ClientServices.Data.GetMails())
                if (!_unreadOnly || !mail.IsRead)
                    _mails.Add(mail);
            _emptyHint.SetActive(_mails.Count == 0);
            RefreshLabels();
            if (_mails.Count == 0)
            {
                _detail.text = "暂无邮件";
                _status.text = "本地邮件列表为空";
                return;
            }
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _mails.Count - 1);
            SelectMail(_selectedIndex);
        }

        private void RefreshLabels()
        {
            for (int index = 0; index < _mailLabels.Length; index++)
            {
                bool exists = index < _mails.Count;
                _mailLabels[index].transform.parent.gameObject.SetActive(exists);
                if (!exists)
                    continue;
                ClientMail mail = _mails[index];
                _mailLabels[index].text = (mail.IsRead ? "" : "● ") + mail.Title + (mail.IsClaimed ? "\n已领取" : "\n附件待领取");
            }
        }
    }
}
