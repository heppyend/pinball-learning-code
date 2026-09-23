using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client.UI
{
    /// <summary>
    /// **收藏品条目**的显示组件（头像 / 头像框 / 徽章 / 铭牌 / 称号 五类共用）。
    ///
    /// <para>与 <see cref="ClientHeroCard"/> 同一套原则：<b>只写控件</b> —— 不查数据、不算数、不读表、不找节点；
    /// 数据（名称 / 是否拥有 / 是否使用中 / 图标）全部由 <see cref="Bind"/> 推入，图标由调用方解析。</para>
    ///
    /// <para>场景事实（2026-09-20 取证）：每个列表项（如 `头像已解锁Image`）**内部已经带一个"未解锁"子节点**
    /// （如 `头像已解锁Image/头像未解锁Image/Image`）⇒ 表达"拥有 / 未拥有"只需**切那个子节点**，
    /// 不需要换整张卡、也不需要两套模板。</para>
    ///
    /// <para><b>GC</b>：值没变就不写控件（`TMP_Text.text` 的 setter 会触发文本重建）。</para>
    /// </summary>
    public sealed class ClientCollectionItemView : MonoBehaviour
    {
        [Tooltip("图标 Image（头像 / 徽章 / 铭牌 / 头像框 / 称号的图案）")]
        [SerializeField] private Image _icon;

        [Tooltip("未拥有时显示的\"未解锁\"子节点（名字里含「未解锁」的那个）")]
        [SerializeField] private GameObject _lockRoot;

        [Tooltip("使用中标记（可空；没有就留空）")]
        [SerializeField] private GameObject _selectedRoot;

        [Tooltip("名称文本（可空；收藏品列表通常不显示名字）")]
        [SerializeField] private TMP_Text _nameText;

        [Tooltip("自身 Button（留空则运行时取本节点）")]
        [SerializeField] private Button _button;

        private Sprite _lastIcon;
        private string _lastName;
        private bool _lastOwned = true;
        private bool _lastSelected;

        /// <summary>本条目自己的 Button（模板根上已有）。</summary>
        public Button Button
        {
            get
            {
                if (_button == null)
                    _button = GetComponent<Button>();
                return _button;
            }
        }

        /// <summary>
        /// 绑定一个收藏品条目。
        /// </summary>
        /// <param name="displayName">名称（可为空串）</param>
        /// <param name="owned">是否已拥有（false ⇒ 显示"未解锁"子节点）</param>
        /// <param name="selected">是否使用中（无标记节点时忽略）</param>
        /// <param name="icon">图标；传 null 表示不改动（保持模板图）</param>
        /// <param name="onClick">点击回调</param>
        public void Bind(string displayName, bool owned, bool selected, Sprite icon, Action onClick)
        {
            if (_icon != null && icon != null && !ReferenceEquals(_lastIcon, icon))
            {
                _icon.sprite = icon;
                _lastIcon = icon;
            }

            SetText(_nameText, displayName, ref _lastName);
            SetActive(_lockRoot, !owned);
            SetActive(_selectedRoot, selected);

            _lastOwned = owned;
            _lastSelected = selected;

            if (Button != null)
            {
                Button.onClick.RemoveAllListeners();
                if (onClick != null)
                    Button.onClick.AddListener(() => onClick());
            }
        }

        /// <summary>整条显隐（列表里多余的实例用它隐藏，而不是销毁）。</summary>
        public void SetVisible(bool visible)
        {
            if (gameObject.activeSelf != visible)
                gameObject.SetActive(visible);
        }

        /// <summary>最近一次绑定的拥有状态（诊断用，不参与逻辑）。</summary>
        public bool LastOwned { get { return _lastOwned; } }

        /// <summary>最近一次绑定的使用中状态（诊断用，不参与逻辑）。</summary>
        public bool LastSelected { get { return _lastSelected; } }

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
