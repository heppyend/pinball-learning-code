using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    public sealed class ClientGachaResultPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _content;

        public void Configure(GameObject root, Text content)
        {
            _root = root;
            _content = content;
            Hide();
        }

        public void Show(string text)
        {
            if (_content != null) _content.text = text;
            if (_root != null) _root.SetActive(true);
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }
    }
}
