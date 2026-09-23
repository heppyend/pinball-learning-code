using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client.Stats.View
{
    /// <summary>
    /// **一条能力**（技能 / 天赋）的显示组件（View）。
    ///
    /// <para>传入 <see cref="ClientAbilityEntry"/> 即可：说明文本用 <c>DisplayDescription</c>
    /// （已由 <see cref="IClientDescriptionFormatter"/> 按当前等级渲染好），**View 不做任何格式化**。</para>
    ///
    /// <para><b>GC</b>：等级没变就不重建等级字符串、不写控件；数值/名称用引用比较后再赋值。</para>
    /// </summary>
    public sealed class ClientAbilityEntryView : MonoBehaviour
    {
        [Tooltip("图标")]
        [SerializeField] private Image _icon;

        [Tooltip("名称文本")]
        [SerializeField] private TMP_Text _nameText;

        [Tooltip("等级文本（形如 Lv.3；等级 0 时置空）")]
        [SerializeField] private TMP_Text _levelText;

        [Tooltip("说明文本")]
        [SerializeField] private TMP_Text _descriptionText;

        [Tooltip("未拥有时显示的锁 / 灰态根节点（可空）")]
        [SerializeField] private GameObject _lockRoot;

        [Tooltip("整条根节点；留空则用本节点")]
        [SerializeField] private GameObject _root;

        /// <summary>复用同一个 StringBuilder，避免每次拼 "Lv.N" 都产生垃圾。</summary>
        private readonly StringBuilder _levelBuilder = new StringBuilder(8);

        private string _lastName;
        private string _lastDescription;
        private int _lastLevel = int.MinValue;
        private Sprite _lastIcon;

        /// <summary>绑定一条能力；<paramref name="entry"/> 为 null 时整条隐藏（空态，不报错）。</summary>
        public void Bind(ClientAbilityEntry entry, Sprite icon)
        {
            if (entry == null)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            SetText(_nameText, entry.Name, ref _lastName);
            SetText(_descriptionText, entry.DisplayDescription, ref _lastDescription);
            SetLevel(entry.Level);

            if (_icon != null && !ReferenceEquals(_lastIcon, icon))
            {
                _icon.sprite = icon;
                _lastIcon = icon;
            }

            SetActive(_lockRoot, !entry.IsOwned);
        }

        public void SetVisible(bool visible)
        {
            GameObject root = _root != null ? _root : gameObject;
            if (root.activeSelf != visible)
                root.SetActive(visible);
        }

        private void SetLevel(int level)
        {
            if (_levelText == null || _lastLevel == level)
                return;

            _lastLevel = level;

            _levelBuilder.Length = 0;
            if (level > 0)
            {
                _levelBuilder.Append("Lv.");
                _levelBuilder.Append(level);
            }

            _levelText.text = _levelBuilder.ToString();
        }

        private static void SetText(TMP_Text text, string value, ref string cache)
        {
            if (text == null)
                return;
            if (ReferenceEquals(cache, value) || cache == value)
                return;

            cache = value;
            text.text = value ?? string.Empty;
        }

        private static void SetActive(GameObject target, bool visible)
        {
            if (target != null && target.activeSelf != visible)
                target.SetActive(visible);
        }
    }
}
