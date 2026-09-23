using System.Collections.Generic;
using Pinball.Client.Domain;

namespace Pinball.Client.Services
{
    /// <summary>
    /// **静态配置**（人人相同、来自配置表）的只读入口。
    ///
    /// 与 <see cref="IClientDataService"/> 的分工（负责人 2026-09-20 定调「**表驱动**」）：
    /// <list type="bullet">
    /// <item>本接口回答「游戏里**有哪些**英雄 / 头像 / 头像框 / 徽章 / 铭牌 / 称号」——
    /// 数据来自配置表，**不需要服务端**。</item>
    /// <item><see cref="IClientDataService"/> 只回答「**我**拥有哪些、等级多少、装备了哪个」——
    /// 那是玩家状态，来自服务端（未接入前是本地模拟）。</item>
    /// </list>
    ///
    /// 实现见 `TableClientConfigService`（适配 `Assets/Scripts/Table` 的 `T*Helper`）。
    /// **UI 只依赖本接口**，不直接引用原项目的表类型 —— 这样换表 / 换加载方式都不影响页面。
    /// </summary>
    public interface IClientConfigService
    {
        /// <summary>
        /// 配置表是否已加载。`false` 时所有查询返回空集合（UI 应显示空态，不得崩溃）。
        /// 表缺失时底层只打一条警告然后**按空表继续**，所以这个标志是"UI 有没有真数据"的唯一判据。
        /// </summary>
        bool IsReady { get; }

        /// <summary>前端可见的英雄（已按 `ClientShow` 过滤，按 `HeroId` 升序）。</summary>
        IReadOnlyList<ClientHeroConfig> GetHeroes();

        /// <summary>按 Id 取英雄定义；表里没有或前端不显示时返回 false。</summary>
        bool TryGetHero(int heroId, out ClientHeroConfig hero);

        /// <summary>某类收藏品的**前端可见**条目（已按 `ClientShow` 过滤，按 `Sorting` 升序）。</summary>
        IReadOnlyList<ClientCollectionConfig> GetCollections(ClientCollectionKind kind);

        // ------------------------------------------------------------------
        // 道具 / 技能 / 天赋 / 随机英雄池（2026-09-20 补齐；同样只回答"有哪些"）
        // ------------------------------------------------------------------

        /// <summary>全部道具（表 `Item`，客户端 9 行；按 `Id` 升序）。</summary>
        IReadOnlyList<ClientItemConfig> GetItems();

        /// <summary>按 Id 取道具定义（道具名 / 描述 / 图标名 / 类型 / 品质）。</summary>
        bool TryGetItem(int itemId, out ClientItemConfig item);

        /// <summary>按 Id 取技能定义（表 `Skill`）。</summary>
        bool TryGetSkill(int skillId, out ClientSkillConfig skill);

        /// <summary>按 Id 取天赋 / 潜能定义（表 `Potency`）。</summary>
        bool TryGetPotency(int potencyId, out ClientPotencyConfig potency);

        /// <summary>某英雄的技能（按 `THero.Skill` 的顺序，实测 4 个）；英雄不存在时返回空集合。</summary>
        IReadOnlyList<ClientSkillConfig> GetHeroSkills(int heroId);

        /// <summary>某英雄的天赋 / 潜能（按 `THero.Potency` 的顺序，实测 13 个）；英雄不存在时返回空集合。</summary>
        IReadOnlyList<ClientPotencyConfig> GetHeroPotencies(int heroId);

        /// <summary>随机英雄池（表 `RandomHero`，4 行）。⚠️ 表内**没有**权重与概率，那些属服务端。</summary>
        IReadOnlyList<ClientRandomHeroConfig> GetRandomHeroPool();
    }
}
