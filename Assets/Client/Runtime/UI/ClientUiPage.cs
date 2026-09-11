using UnityEngine;

namespace Pinball.Client.UI
{
    public enum ClientUiPageId
    {
        Home,
        Profile,
        HomeShowcase,
        Hero,
        HeroDetail,
        Backpack,
        Gacha,
        Shop,
        Mail,
        Rank,
        Activity,
        Task,
        Notice,
        Badge,
        HeroEnhance,
        Formation,
        Marble,
    }

    public sealed class ClientUiPage : MonoBehaviour
    {
        [SerializeField] private ClientUiPageId _pageId;
        public ClientUiPageId PageId { get { return _pageId; } }

        public void Configure(ClientUiPageId pageId) { _pageId = pageId; }
        public void Enter() { gameObject.SetActive(true); }
        public void Pause() { gameObject.SetActive(false); }
        public void Resume() { gameObject.SetActive(true); }
        public void Exit() { gameObject.SetActive(false); }
    }
}
