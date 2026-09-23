using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pinball.Client.UI
{
    /// <summary>
    /// **收藏品列表管理器**：1 个模板 + N 个实例（头像 / 头像框 / 徽章 / 铭牌 / 称号 五类共用）。
    ///
    /// <para>与 <see cref="ClientHeroCardList"/> **完全同一套范式**（负责人 2026-09-20 要求卡牌/列表逻辑独立且复用）：
    /// 实例数恒等于数据条数；不足才新建，**多余的隐藏而非销毁**（数据变多时可复用）；每次 `Show` 都重新对齐，
    /// 杜绝"只加不减"导致的实例累积。</para>
    ///
    /// <para>同样刻意不依赖领域模型：用委托把数据映射成基础类型（名称 / 是否拥有 / 是否使用中 / 图标），
    /// 所以 `ClientProfilePage` 之外的地方（例如未来的图鉴、活动奖励预览）也能直接复用。</para>
    /// </summary>
    public sealed class ClientCollectionList
    {
        private readonly RectTransform _container;
        private readonly ClientCollectionItemView _template;
        private readonly List<ClientCollectionItemView> _instances = new List<ClientCollectionItemView>();

        public ClientCollectionList(RectTransform container, GameObject template)
        {
            _container = container;
            _template = template != null ? template.GetComponent<ClientCollectionItemView>() : null;

            // 模板永不可见，只作克隆源。
            if (_template != null)
                _template.gameObject.SetActive(false);
        }

        public bool IsValid { get { return _container != null && _template != null; } }

        public int Count { get { return _instances.Count; } }

        /// <summary>
        /// 按数据刷新整个列表。
        /// </summary>
        /// <param name="items">数据列表</param>
        /// <param name="nameOf">取名称（可为 null）</param>
        /// <param name="ownedOf">取是否已拥有</param>
        /// <param name="selectedOf">取是否使用中（可为 null）</param>
        /// <param name="spriteOf">取图标（可为 null ⇒ 保持模板图）</param>
        /// <param name="onClick">点击回调，参数为该条目在列表中的下标</param>
        public void Show<T>(IReadOnlyList<T> items,
                            Func<T, string> nameOf,
                            Func<T, bool> ownedOf,
                            Func<T, bool> selectedOf,
                            Func<T, Sprite> spriteOf,
                            Action<int> onClick)
        {
            if (!IsValid)
                return;

            _template.gameObject.SetActive(false);

            int needed = items != null ? items.Count : 0;
            EnsureCapacity(needed);

            for (int i = 0; i < _instances.Count; i++)
            {
                ClientCollectionItemView instance = _instances[i];
                if (instance == null)
                    continue;

                bool exists = i < needed;
                instance.SetVisible(exists);
                if (!exists)
                    continue;

                T item = items[i];
                instance.Bind(
                    nameOf != null ? nameOf(item) : string.Empty,
                    ownedOf != null && ownedOf(item),
                    selectedOf != null && selectedOf(item),
                    spriteOf != null ? spriteOf(item) : null,
                    onClick != null ? (Action)(() => onClick(i)) : null);
            }
        }

        /// <summary>隐藏全部实例（空态时用；不销毁，便于复用）。</summary>
        public void HideAll()
        {
            for (int i = 0; i < _instances.Count; i++)
            {
                if (_instances[i] != null)
                    _instances[i].SetVisible(false);
            }
        }

        /// <summary>显示已拥有的条目数（诊断用）。</summary>
        public int OwnedCount()
        {
            int owned = 0;
            for (int i = 0; i < _instances.Count; i++)
            {
                if (_instances[i] != null && _instances[i].gameObject.activeSelf && _instances[i].LastOwned)
                    owned++;
            }
            return owned;
        }

        private void EnsureCapacity(int needed)
        {
            while (_instances.Count < needed)
            {
                if (_template == null || _container == null)
                    break;

                GameObject clone = UnityEngine.Object.Instantiate(_template.gameObject, _container);
                clone.name = _template.gameObject.name + "_Instance";

                ClientCollectionItemView view = clone.GetComponent<ClientCollectionItemView>();
                if (view == null)
                {
                    Debug.LogError("[Client] 收藏品模板上没有 ClientCollectionItemView 组件，无法生成条目。");
                    UnityEngine.Object.Destroy(clone);
                    break;
                }

                clone.SetActive(false);
                _instances.Add(view);
            }
        }
    }
}
