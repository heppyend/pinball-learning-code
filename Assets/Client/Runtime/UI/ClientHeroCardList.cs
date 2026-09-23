using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pinball.Client.UI
{
    /// <summary>
    /// "1 个模板 + N 个实例"的英雄卡牌列表管理器。
    ///
    /// 负责人 2026-09-20 要求把卡牌逻辑独立出来以解耦：英雄页（仅已解锁）与
    /// 图鉴页（全部 + 锁/遮罩）**共用同一套生成与刷新逻辑**，只是传入的数据与
    /// `unlockedOf` 不同。未来的编队、背包列表也可直接复用。
    ///
    /// 与 `ClientHeroCard` 一样刻意不依赖具体数据模型：用委托把领域对象映射成基础类型，
    /// 因此不引入对 `ClientHero` 的依赖。
    ///
    /// 实例管理原则：**实例数恒等于数据条数**；不足才新建，多余的隐藏而非销毁
    /// （数据增多时可复用，避免反复 Instantiate）。**每次 Show 都重新对齐**，
    /// 杜绝此前"只按绑定数量判断、从不回收"导致的 2→4→6 张累积问题。
    /// </summary>
    public sealed class ClientHeroCardList
    {
        private readonly RectTransform _container;
        private readonly ClientHeroCard _template;
        private readonly List<ClientHeroCard> _instances = new List<ClientHeroCard>();

        public ClientHeroCardList(RectTransform container, GameObject template)
        {
            _container = container;
            _template = template != null ? template.GetComponent<ClientHeroCard>() : null;
            if (_template != null)
                _template.gameObject.SetActive(false);
        }

        public bool IsValid { get { return _container != null && _template != null; } }

        public int Count { get { return _instances.Count; } }

        /// <summary>
        /// 按数据刷新整个列表。
        /// </summary>
        /// <param name="items">数据列表</param>
        /// <param name="nameOf">取名称</param>
        /// <param name="levelOf">取等级（&lt;=0 不显示等级）</param>
        /// <param name="unlockedOf">取解锁状态（false → 显示锁 + 遮罩）</param>
        /// <param name="spriteOf">取立绘（可返回 null）</param>
        /// <param name="onClick">点击回调，参数为该卡在列表中的下标</param>
        public void Show<T>(IList<T> items,
                            Func<T, string> nameOf,
                            Func<T, int> levelOf,
                            Func<T, bool> unlockedOf,
                            Func<T, Sprite> spriteOf,
                            Action<int> onClick)
        {
            Show(items, nameOf, levelOf, unlockedOf, spriteOf, null, null, onClick);
        }

        /// <summary>
        /// 带「队内编号」装饰的重载 —— `FormationPage编队` 专用：
        /// <paramref name="teamSlotOf"/> 返回该卡所在槽位（1..3），`0` = 不显示编号；
        /// <paramref name="teamSlotDigits"/> 是数字贴图，下标 0..9 对应数字 0..9（工程内已存在）。
        ///
        /// 其余语义与上面的重载**完全一致**（实例数恒等于数据条数）。
        /// 编号在 `Apply` **之后**才写 —— 因为 `ClientHeroCard.Apply` 末尾会把队内编号强制隐藏。
        /// </summary>
        public void Show<T>(IList<T> items,
                            Func<T, string> nameOf,
                            Func<T, int> levelOf,
                            Func<T, bool> unlockedOf,
                            Func<T, Sprite> spriteOf,
                            Func<T, int> teamSlotOf,
                            Sprite[] teamSlotDigits,
                            Action<int> onClick)
        {
            if (!IsValid)
                return;

            // 模板永不可见，只作克隆源。
            _template.gameObject.SetActive(false);

            int needed = items != null ? items.Count : 0;
            EnsureCapacity(needed);

            for (int i = 0; i < _instances.Count; i++)
            {
                bool exists = i < needed;
                if (_instances[i].gameObject.activeSelf != exists)
                    _instances[i].gameObject.SetActive(exists);
                if (!exists)
                    continue;

                int index = i;
                T item = items[i];
                _instances[i].Apply(
                    nameOf != null ? nameOf(item) : string.Empty,
                    levelOf != null ? levelOf(item) : 0,
                    unlockedOf != null && unlockedOf(item),
                    spriteOf != null ? spriteOf(item) : null,
                    onClick != null ? (Action)(() => onClick(index)) : null);

                if (teamSlotOf != null)
                {
                    int slotNumber = teamSlotOf(item);
                    _instances[i].SetTeamSlot(slotNumber, ResolveDigit(teamSlotDigits, slotNumber));
                }
            }
        }

        /// <summary>按槽位号（1..N）取数字贴图；数组缺失或越界返回 null（只显隐、不换图）。</summary>
        private static Sprite ResolveDigit(Sprite[] digits, int slotNumber)
        {
            if (digits == null || slotNumber < 1 || slotNumber >= digits.Length)
                return null;
            return digits[slotNumber];
        }

        /// <summary>回收全部运行时实例（离开页面时调用，避免残留）。</summary>
        public void Clear()
        {
            for (int i = 0; i < _instances.Count; i++)
            {
                if (_instances[i] != null)
                    UnityEngine.Object.Destroy(_instances[i].gameObject);
            }
            _instances.Clear();
        }

        private void EnsureCapacity(int needed)
        {
            while (_instances.Count < needed)
            {
                GameObject clone = UnityEngine.Object.Instantiate(_template.gameObject, _container);
                clone.name = _template.gameObject.name + "_Instance";

                ClientHeroCard card = clone.GetComponent<ClientHeroCard>();
                if (card == null)
                {
                    // 模板上缺少组件时不再无限尝试，直接销毁并停止扩容。
                    Debug.LogError("[Client] 英雄卡模板上没有 ClientHeroCard 组件，无法生成卡牌。");
                    UnityEngine.Object.Destroy(clone);
                    break;
                }

                clone.SetActive(false);
                _instances.Add(card);
            }
        }
    }
}
