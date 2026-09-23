using System;

namespace Pinball.Client.Stats
{
    /// <summary>
    /// **服务端契约层** —— 客户端唯一的数值入口。
    ///
    /// <para>负责人 2026-09-20 的架构原则：**数值只由权威服务器计算并分发**。
    /// 因此本接口只提供"取已算好的值"，**不提供任何计算**；接入真实服务器时**只替换本接口的实现**，
    /// <see cref="ClientStatsService"/>、View 与页面都不改。</para>
    ///
    /// <para>未接入服务器前由 `LocalClientStatsGateway` 扮演服务器（本地模拟）。</para>
    /// </summary>
    public interface IClientStatsGateway
    {
        /// <summary>
        /// 把 <paramref name="ownerId"/> 的**属性快照**写进 <paramref name="destination"/>（**先清空再填**）。
        ///
        /// <para>刻意用"写进调用方的 List"而不是返回新 List：避免每次刷新都产生一个数组（GC 原则）。</para>
        /// </summary>
        /// <param name="ownerId">拥有者 id（英雄 id 或玩家标识；由上层约定）。</param>
        void CopyAttributes(int ownerId, System.Collections.Generic.List<ClientAttributeValue> destination);

        /// <summary>取某能力（技能 / 天赋）的状态；服务器没有该记录时返回 false。</summary>
        bool TryGetAbilityState(int abilityId, out ClientAbilityState state);

        /// <summary>
        /// 数据发生变化（服务器推送，或本地模拟主动触发）。参数是变化的拥有者 id / 英雄 id。
        /// **刷新一律由事件驱动，禁止在 Update 里轮询。**
        /// </summary>
        event Action<int> Changed;
    }
}
