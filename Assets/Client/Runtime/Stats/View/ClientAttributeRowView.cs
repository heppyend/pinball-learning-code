using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client.Stats.View
{
    /// <summary>
    /// **一行属性**的显示组件（View）。
    ///
    /// <para><b>只负责写控件</b>：不查数据、不算数、不读表、不找节点。
    /// 文本由调用方备好后传进来（<see cref="IClientDescriptionFormatter"/> 负责所有拼接）。</para>
    ///
    /// <para><b>GC</b>：值没变就不写控件（`TMP_Text.text` 的 setter 会触发文本重建），
    /// 也不产生任何中间字符串。</para>
    /// </summary>
    public sealed class ClientAttributeRowView : MonoBehaviour
    {
        [Tooltip("属性图标（可空）")]
        [SerializeField] private Image _icon;

        [Tooltip("属性名文本")]
        [SerializeField] private TMP_Text _nameText;

        [Tooltip("属性值文本")]
        [SerializeField] private TMP_Text _valueText;

        [Tooltip("整行根节点；留空则用本节点")]
        [SerializeField] private GameObject _root;

        private string _lastName;
        private string _lastValue;
        private Sprite _lastIcon;

        /// <summary>绑定一行属性。名称与数值文本**由调用方备好**（含单位与格式）。</summary>
        public void Bind(string displayName, string displayValue, Sprite icon)
        {
            SetText(_nameText, displayName, ref _lastName);
            SetText(_valueText, displayValue, ref _lastValue);

            if (_icon != null && !ReferenceEquals(_lastIcon, icon))
            {
                _icon.sprite = icon;
                _lastIcon = icon;
            }
        }

        /// <summary>整行显隐（例如该属性服务器没有下发时隐藏）。</summary>
        public void SetVisible(bool visible)
        {
            GameObject root = _root != null ? _root : gameObject;
            if (root.activeSelf != visible)
                root.SetActive(visible);
        }

        private static void SetText(TMP_Text text, string value, ref string cache)
        {
            if (text == null)
                return;

            // 值没变就不写（TMP 的 text setter 会触发重建，是常见 GC / CPU 热点）
            if (ReferenceEquals(cache, value) || cache == value)
                return;

            cache = value;
            text.text = value ?? string.Empty;
        }
    }
}
