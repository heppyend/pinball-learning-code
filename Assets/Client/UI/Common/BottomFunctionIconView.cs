using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>底部功能入口的可配置视图，Root 的 Button 是唯一点击判定。</summary>
    public sealed class BottomFunctionIconView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _label;

        public Image IconImage { get { return _icon; } }

        public void Initialize(Image icon, Text label)
        {
            _icon = icon;
            _label = label;
        }

        /// <summary>
        /// 配置图标与文字。
        ///
        /// <para>⚠️ 2026-09-21（UI 契约审计发现）：本方法原先**没有判空**，而实测场景里
        /// `Bottom1`..`Bottom4` 四个实例的 `_icon` / `_label` **都是 `fileID 0`（未接线）** ——
        /// 一旦有人调用本方法就会**空引用抛异常**。现改为**逐项判空**（未接线即跳过，不崩也不报错），
        /// 与工程其它视图字段的处理方式一致。**语义未变**：接线后行为完全相同。</para>
        /// </summary>
        public void Configure(Sprite iconSprite, string label)
        {
            if (_icon != null)
                _icon.sprite = iconSprite;
            if (_label != null)
                _label.text = label;
        }
    }
}
