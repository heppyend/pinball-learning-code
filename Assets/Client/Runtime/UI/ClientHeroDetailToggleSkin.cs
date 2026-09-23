using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 英雄详情页 `天赋 / 秘技 / 终结技` 三个页签的**选中态皮肤**。
    ///
    /// <para><b>背景（2026-09-21 负责人反馈）</b>：点页签时"只有页面切换、页签自己的贴图不跟着变"。
    /// 负责人原话：*"我指的是它们 3 个的点击的动画，也就是改变：点击一个时 3 个的字体贴图和底框都要变"*。</para>
    ///
    /// <para><b>实测根因（`Logs/ui-node-toggle.md` + 场景 YAML 实读）</b>：</para>
    /// <list type="number">
    /// <item>层级实际是 `Toggle/Background/Checkmark` —— **`Checkmark` 是 `Background` 的子节点**，
    /// 不是 `Toggle` 的直接子节点。此前代码按"Toggle 的直接子节点"找 Checkmark，**永远找不到** ⇒
    /// `_toggleSkin` 里 Label 恒为 null，文字贴图自然永远不显示；</item>
    /// <item>`技能区/{天赋,秘技,终结技}选中框.png` 这三张图**全工程零引用**（负责人画好了忘了接）；</item>
    /// <item>`秘技Toggle/Background` 的 Image 还是 `m_Enabled: 0`；</item>
    /// <item>`ClientHeroDetailPage.BindSections` 过去把三个 Toggle 的 `graphic` 置空，
    /// 于是 Unity 的 Toggle 既不换贴图也不控显隐。</item>
    /// </list>
    ///
    /// <para><b>本组件的职责</b>：选中时把该页签的 `Background.sprite` 换成「选中框」贴图、
    /// 未选中时换回「未选中底框」贴图；`Checkmark`（文字贴图）**恒显**。
    /// 之所以用**换 sprite**而不是 `SetActive` 切换节点：Checkmark 是 Background 的子节点，
    /// 隐藏 Background 会连带隐藏文字。</para>
    ///
    /// <para>不改锚点 / 尺寸 / 层级；贴图与组件由编辑器工具 `ClientShellStructuralRepair.FixHeroDetailSkillTabs` 写入场景。</para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ClientHeroDetailToggleSkin : MonoBehaviour
    {
        /// <summary>一个页签的皮肤绑定。</summary>
        [System.Serializable]
        public sealed class Binding
        {
            [Tooltip("该页签的 Toggle（提供选中事件）。")]
            public Toggle Toggle;

            [Tooltip("底框 Image：选中时换成 SelectedSprite，未选中时 sprite 置空（只留文字贴图）。")]
            public Image Frame;

            [Tooltip("文字贴图 Image：常态恒显，不随选中态变化。")]
            public Image Label;

            [Tooltip("选中态贴图（技能区/*选中框.png）。未选中时底框 sprite 置空。")]
            public Sprite SelectedSprite;

            public bool IsValid { get { return Toggle != null && Frame != null; } }
        }

        [Tooltip("按 天赋 / 秘技 / 终结技 的顺序填写。")]
        [SerializeField] private Binding[] _bindings = new Binding[0];

        /// <summary>
        /// 依次绑定一个页签（第 1 次调用 = 天赋、第 2 次 = 秘技、第 3 次 = 终结技）。
        /// 已存在同一 Toggle 的绑定时**原地覆盖**，因此可重复调用。
        /// </summary>
        public void Bind(Toggle toggle, Image frame, Image label, Sprite selected)
        {
            if (toggle == null || frame == null)
                return;

            for (int i = 0; i < _bindings.Length; i++)
            {
                if (_bindings[i] != null && _bindings[i].Toggle == toggle)
                {
                    // ⚠️ 只在传入了新贴图时覆盖 —— 运行时用 null 调用本方法时，
                    //    绝不能把编辑器工具写进场景的贴图清掉。
                    _bindings[i].Frame = frame;
                    _bindings[i].Label = label;
                    if (selected != null)
                        _bindings[i].SelectedSprite = selected;
                    return;
                }
            }

            Binding binding = new Binding
            {
                Toggle = toggle,
                Frame = frame,
                Label = label,
                SelectedSprite = selected,
            };

            for (int i = 0; i < _bindings.Length; i++)
            {
                if (_bindings[i] == null || _bindings[i].Toggle == null)
                {
                    _bindings[i] = binding;
                    return;
                }
            }

            Binding[] grown = new Binding[_bindings.Length + 1];
            System.Array.Copy(_bindings, grown, _bindings.Length);
            grown[_bindings.Length] = binding;
            _bindings = grown;
        }

        /// <summary>
        /// 把三个页签刷成"只有 <paramref name="selectedIndex"/> 用选中框贴图"的状态，
        /// 并确保三个文字贴图都可见。
        /// </summary>
        public void Apply(int selectedIndex)
        {
            for (int i = 0; i < _bindings.Length; i++)
            {
                Binding binding = _bindings[i];
                if (binding == null || !binding.IsValid)
                    continue;

                bool selected = i == selectedIndex;

                // 文字贴图：常态恒显（Toggle.graphic 会在 isOn=false 时把 alpha 置 0，故不交给它）。
                if (binding.Label != null && !binding.Label.gameObject.activeSelf)
                    binding.Label.gameObject.SetActive(true);

                // 底框：选中 → 换成「选中框」贴图；未选中 → 清空 sprite（只留文字贴图，
                // 与项目里《参考.png》的"只有选中页签有底框"一致）。
                //
                // ⚠️ 不能用 SetActive 隐藏底框：`Checkmark`（文字）是 `Background` 的子节点，
                //    隐藏 Background 会连带隐藏文字。也不能清空 SelectedSprite 之外的来源 ——
                //    未选中态没有独立美术资源（工程里只有 6 张：3 张文字 + 3 张选中框）。
                Sprite want = selected ? binding.SelectedSprite : null;
                if (binding.Frame.sprite != want)
                    binding.Frame.sprite = want;

                if (!binding.Frame.enabled)
                    binding.Frame.enabled = true;
            }
        }

        /// <summary>自检/审计用：返回"绑定齐全"的数量（含底框/文字/两张贴图）。</summary>
        public int CountValid()
        {
            int count = 0;
            for (int i = 0; i < _bindings.Length; i++)
            {
                Binding b = _bindings[i];
                if (b != null && b.IsValid && b.Label != null && b.SelectedSprite != null)
                    count++;
            }
            return count;
        }

        /// <summary>编辑器工具用：读第 <paramref name="index"/> 个绑定（越界返回 null）。</summary>
        public Binding GetBinding(int index)
        {
            if (index < 0 || index >= _bindings.Length)
                return null;
            return _bindings[index];
        }

        /// <summary>编辑器工具用：判断指定绑定是否已经是目标值（幂等判定）。</summary>
        public bool Matches(int index, Toggle toggle, Image frame, Image label, Sprite selected)
        {
            Binding b = GetBinding(index);
            return b != null && b.Toggle == toggle && b.Frame == frame && b.Label == label &&
                   b.SelectedSprite == selected;
        }
    }
}
