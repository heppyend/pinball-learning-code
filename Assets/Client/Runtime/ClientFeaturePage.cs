using Pinball.Client.Domain;
using Pinball.Client.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>一级入口的临时内容页：承载真实导航，并为后续独立业务页保留替换点。</summary>
    public sealed class ClientFeaturePage : MonoBehaviour
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text _title;
        [SerializeField] private Text _content;

        public void Configure(GameObject pageRoot, Text title, Text content)
        {
            _pageRoot = pageRoot;
            _title = title;
            _content = content;
        }

        public void Show(ClientHomeDestination destination)
        {
            _pageRoot.SetActive(true);
            _title.text = GetTitle(destination);
            _content.text = GetContent(destination);
        }

        public void Hide() { _pageRoot.SetActive(false); }

        private static string GetTitle(ClientHomeDestination destination)
        {
            return destination == ClientHomeDestination.Hero ? "英雄 / 图鉴" :
                destination == ClientHomeDestination.Backpack ? "背包" :
                destination == ClientHomeDestination.Shop ? "商城" :
                destination == ClientHomeDestination.Mail ? "邮箱" :
                destination == ClientHomeDestination.Rank ? "排行榜" :
                destination == ClientHomeDestination.Activity ? "活动" :
                destination == ClientHomeDestination.Task ? "任务" :
                destination == ClientHomeDestination.Gacha ? "扭蛋" : "公告";
        }

        private static string GetContent(ClientHomeDestination destination)
        {
            if (destination == ClientHomeDestination.Hero)
            {
                string result = "本地英雄：\n";
                foreach (ClientHero hero in ClientServices.Data.GetHeroes())
                    result += hero.Name + "  Lv." + hero.Level + (hero.IsOwned ? "  已拥有\n" : "  未拥有\n");
                return result;
            }

            if (destination == ClientHomeDestination.Backpack)
            {
                string result = "本地背包：\n";
                foreach (ClientInventoryItem item in ClientServices.Data.GetInventory())
                    result += item.ItemId + " × " + item.Quantity + "\n";
                return result;
            }

            return "页面导航已接入。\n该模块的商品、邮件、排行、活动、任务、卡池与公告规则尚待产品/服务端契约确认。";
        }
    }
}
