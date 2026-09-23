using System.Collections.Generic;
using UnityEngine;

namespace Pinball.Client.Stats.View
{
    /// <summary>
    /// **一个能力页签**（天赋 / 秘技 / 终结技）的显示组件（View）。
    ///
    /// <para>内容 = 主技能（<see cref="ClientAbilitySection.Primary"/>）+ 被动强化列表
    /// （<see cref="ClientAbilitySection.Enhancements"/>）。</para>
    ///
    /// <para><b>空态不是错误</b>：主技能与强化都为空时显示 <see cref="_emptyHint"/> 并隐藏强化区
    /// （对应那 50 个英雄的天赋占位数据，负责人已确认"空着、不报错"）。</para>
    ///
    /// <para><b>GC</b>：强化条目用**对象池**（复用已生成的 View，只切显隐，不反复 Instantiate/Destroy）。</para>
    /// </summary>
    public sealed class ClientAbilitySectionView : MonoBehaviour
    {
        [Tooltip("页签内容根节点（切页签时整体显隐；可空）")]
        [SerializeField] private GameObject _pageRoot;

        [Tooltip("主技能条目 View")]
        [SerializeField] private ClientAbilityEntryView _primaryView;

        [Tooltip("空态提示（无主技能也无强化时显示）")]
        [SerializeField] private GameObject _emptyHint;

        [Tooltip("强化区根节点（无强化时隐藏；可空）")]
        [SerializeField] private GameObject _enhancementSection;

        [Tooltip("强化条目的容器")]
        [SerializeField] private RectTransform _enhancementRoot;

        [Tooltip("强化条目模板（放在容器里、默认隐藏）")]
        [SerializeField] private ClientAbilityEntryView _enhancementTemplate;

        /// <summary>已生成的强化条目（对象池；复用，不销毁）。</summary>
        private readonly List<ClientAbilityEntryView> _enhancementViews = new List<ClientAbilityEntryView>();

        private bool _templateHidden;

        /// <summary>绑定一个页签的内容。</summary>
        /// <param name="section">页签数据；为 null 时按空态处理。</param>
        /// <param name="resolveIcon">图标解析（由页面提供并缓存；传 null 表示不设图标）。</param>
        public void Bind(ClientAbilitySection section, ClientIconResolver resolveIcon)
        {
            ClientAbilityEntry primary = section != null ? section.Primary : null;

            if (_primaryView != null)
                _primaryView.Bind(primary, ResolveIcon(resolveIcon, primary));

            BindEnhancements(section, resolveIcon);

            bool empty = section == null || section.IsEmpty;
            SetActive(_emptyHint, empty);
        }

        /// <summary>切换本页签内容的显隐（由页签 Toggle 的控制器调用）。</summary>
        public void SetPageActive(bool active)
        {
            GameObject root = _pageRoot != null ? _pageRoot : gameObject;
            if (root.activeSelf != active)
                root.SetActive(active);
        }

        private void BindEnhancements(ClientAbilitySection section, ClientIconResolver resolveIcon)
        {
            List<ClientAbilityEntry> entries = section != null ? section.Enhancements : null;
            int count = entries != null ? entries.Count : 0;

            HideTemplate();

            // 复用已有 View，不够才新建（对象池）
            while (_enhancementViews.Count < count)
            {
                if (_enhancementTemplate == null || _enhancementRoot == null)
                    break;

                ClientAbilityEntryView clone = Instantiate(_enhancementTemplate, _enhancementRoot);
                clone.gameObject.name = _enhancementTemplate.gameObject.name + " (Clone)";
                _enhancementViews.Add(clone);
            }

            for (int index = 0; index < _enhancementViews.Count; index++)
            {
                ClientAbilityEntryView view = _enhancementViews[index];
                if (view == null)
                    continue;

                if (index >= count)
                {
                    view.SetVisible(false);   // 多余的隐藏（不销毁，便于数据变多时复用）
                    continue;
                }

                ClientAbilityEntry entry = entries[index];
                view.SetVisible(true);
                view.Bind(entry, ResolveIcon(resolveIcon, entry));
            }

            SetActive(_enhancementSection, count > 0);
        }

        /// <summary>模板要一直隐藏，否则会多出一个占位条目。</summary>
        private void HideTemplate()
        {
            if (_templateHidden || _enhancementTemplate == null)
                return;

            _templateHidden = true;
            _enhancementTemplate.Bind(null, null);
        }

        private static Sprite ResolveIcon(ClientIconResolver resolver, ClientAbilityEntry entry)
        {
            if (resolver == null || entry == null || entry.IconId == 0)
                return null;
            return resolver(entry.IconId);
        }

        private static void SetActive(GameObject target, bool visible)
        {
            if (target != null && target.activeSelf != visible)
                target.SetActive(visible);
        }
    }
}
