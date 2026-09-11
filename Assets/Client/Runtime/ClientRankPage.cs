using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>开发期排行榜：展示本地快照，不接入服务器排名或跨服规则。</summary>
    public sealed class ClientRankPage : MonoBehaviour
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text[] _topEntries;
        [SerializeField] private Text _selfEntry;
        [SerializeField] private Text _status;

        public void Configure(GameObject pageRoot, Text[] topEntries, Text selfEntry, Text status)
        {
            _pageRoot = pageRoot;
            _topEntries = topEntries;
            _selfEntry = selfEntry;
            _status = status;
        }

        public void Show()
        {
            _pageRoot.SetActive(true);
            Refresh();
        }

        public void Refresh()
        {
            int topIndex = 0;
            foreach (ClientRankEntry entry in ClientServices.Data.GetRankEntries())
            {
                if (entry.IsCurrentPlayer)
                {
                    _selfEntry.text = "我的排名  #" + entry.Rank + "   " + entry.DisplayName + "   战力 " + entry.CombatPower;
                    continue;
                }
                if (topIndex < _topEntries.Length)
                {
                    _topEntries[topIndex].text = "#" + entry.Rank + "    " + entry.DisplayName + "    战力 " + entry.CombatPower;
                    topIndex++;
                }
            }
            _status.text = "本地榜单快照；赛季、奖励与实时刷新待服务端契约确认";
        }
    }
}
