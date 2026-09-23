using System;
using System.Collections.Generic;

namespace Pinball.Client.Stats
{
    /// <summary>
    /// **数值与说明的查询入口** —— 页面（Controller）只依赖它，不依赖 `Gateway`、配置表或具体实现。
    ///
    /// <para>所有查询都是**只读**且**不做计算**：返回的值要么来自权威服务器（`Gateway`），
    /// 要么是配置表里的展示字段。客户端**永远不推导数值**。</para>
    /// </summary>
    public interface IClientStatsService
    {
        // ------------------------------------------------------------------
        // 属性接口
        // ------------------------------------------------------------------

        /// <summary>取某拥有者的单个属性值（服务器已算好）。没有该属性返回 false。</summary>
        bool TryGetAttribute(int ownerId, int attributeId, out int value);

        /// <summary>
        /// 取某拥有者的全部属性。返回的是**复用列表**（下次调用内容会被覆盖）——
        /// 调用方请**立即消费**，不要缓存引用。顺序由 `Gateway` 决定。
        /// </summary>
        IReadOnlyList<ClientAttributeValue> GetAttributes(int ownerId);

        // ------------------------------------------------------------------
        // 技能 / 天赋 数值接口
        // ------------------------------------------------------------------

        /// <summary>取某能力的状态（等级 / 解锁 / 冷却，全部服务器值）。没有该能力返回 false。</summary>
        bool TryGetAbilityState(int abilityId, out ClientAbilityState state);

        /// <summary>
        /// 取某英雄的**完整能力**（3 个页签 + 普攻）—— 页面唯一需要调用的方法。
        /// 返回对象**按英雄缓存并原地刷新**，不要长期持有后跨帧比较（内容会变）。
        /// </summary>
        ClientHeroAbilitySet GetHeroAbilities(int heroId);

        // ------------------------------------------------------------------
        // 变更通知（事件驱动；**禁止在 Update 里轮询**）
        // ------------------------------------------------------------------

        /// <summary>属性变化。参数 = 拥有者 id。</summary>
        event Action<int> AttributesChanged;

        /// <summary>技能 / 天赋数值变化。参数 = 英雄 id。</summary>
        event Action<int> AbilitiesChanged;
    }
}
