using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>主页五个底部入口的选中视觉。按钮导航仍由 ClientHomePage 统一处理。</summary>
    public sealed class ClientBottomNavigationBar : MonoBehaviour
    {
        [SerializeField] private string[] _buttonNames;
        [SerializeField] private Image[] _icons;
        [SerializeField] private GameObject[] _selectedFrames;
        [SerializeField] private Sprite[] _normalSprites;
        [SerializeField] private Sprite[] _selectedSprites;

        public void Configure(string[] buttonNames, Image[] icons, GameObject[] selectedFrames, Sprite[] normalSprites, Sprite[] selectedSprites)
        {
            _buttonNames = buttonNames;
            _icons = icons;
            _selectedFrames = selectedFrames;
            _normalSprites = normalSprites;
            _selectedSprites = selectedSprites;
        }

        public void Select(string buttonName)
        {
            for (int index = 0; index < _buttonNames.Length; index++)
            {
                bool selected = _buttonNames[index] == buttonName;
                if (_icons[index] != null)
                    _icons[index].sprite = selected ? _selectedSprites[index] : _normalSprites[index];
                if (_selectedFrames[index] != null)
                    _selectedFrames[index].SetActive(selected);
            }
        }
    }
}
