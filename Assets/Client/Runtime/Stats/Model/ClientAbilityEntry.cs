using System.Collections.Generic;

namespace Pinball.Client.Stats
{
    /// <summary>
    /// 一条"能力"（技能 或 被动加成）—— **View 绑定的最小单位**。
    ///
    /// <para>字段分三类，界限必须守住：</para>
    /// <list type="number">
    /// <item><b>结构</b>（来自配置表）：<see cref="Id"/> / <see cref="Name"/> / <see cref="Description"/> /
    /// <see cref="IconId"/> / <see cref="SkillType"/> / <see cref="EffectLevels"/> … 只描述"这是个什么能力"。</item>
    /// <item><b>数值状态</b>（来自权威服务器）：<see cref="Level"/> / <see cref="Unlocked"/> /
    /// <see cref="CooldownRemaining"/> / <see cref="CanUpgrade"/>。</item>
    /// <item><b>显示产物</b>：<see cref="DisplayDescription"/> —— 由
    /// <see cref="IClientDescriptionFormatter"/> 按当前等级渲染，**View 直接显示、不再拼接字符串**。</item>
    /// </list>
    ///
    /// <para>用 <c>class</c>（不是 struct）：<see cref="ClientStatsService"/> 会把它**建一次、原地更新**，
    /// 每次刷新只改字段、不产生新对象（GC 原则）。</para>
    /// </summary>
    public sealed class ClientAbilityEntry
    {
        private static readonly int[] EmptyInts = new int[0];

        // ------------------------------------------------------------------
        // 1) 结构（配置表）
        // ------------------------------------------------------------------

        public ClientAbilityKind Kind;

        /// <summary>技能 id 或 天赋 id。</summary>
        public int Id;

        public string Name = string.Empty;

        /// <summary>描述**模板** —— 原样取表，可能含按等级分档的写法（如 `60%/70%/80%`）。</summary>
        public string Description = string.Empty;

        /// <summary>图标 id —— 注意配置表里 `SkillIcon` 是 **int**，与 `Hero.Avatar`（名字）不同源。</summary>
        public int IconId;

        /// <summary>技能类型：`1` 普攻 / `2` 天赋 / `3` 秘技 / `4` 终结技（被动为 `0`）。</summary>
        public int SkillType;

        /// <summary>被动效果类型（表 `Potency.EffectType`）—— **表自有语义，客户端只显示不解释**。</summary>
        public int EffectType;

        /// <summary>品质（被动有，实测 3/4）。</summary>
        public int Quality;

        /// <summary>技能：等级门槛（表 `EffectLevel`，如 `[1,10,20]`）。被动为空。</summary>
        public int[] EffectLevels = EmptyInts;

        /// <summary>被动：效果参数（表 `EffectParameters`，如 `[18]` = +18%）。技能为空。</summary>
        public int[] EffectParameters = EmptyInts;

        // ------------------------------------------------------------------
        // 2) 数值状态（权威服务器）
        // ------------------------------------------------------------------

        public int Level;
        public bool Unlocked;
        public int CooldownRemaining;
        public bool CanUpgrade;

        // ------------------------------------------------------------------
        // 3) 显示产物
        // ------------------------------------------------------------------

        /// <summary>按当前等级渲染好的说明文本（由 <see cref="IClientDescriptionFormatter"/> 填充）。</summary>
        public string DisplayDescription = string.Empty;

        /// <summary>
        /// 上次渲染 <see cref="DisplayDescription"/> 时的等级 —— 等级没变就不重复格式化。
        /// 由 <see cref="ClientStatsService"/> 维护，View 不用管。
        /// </summary>
        internal int FormattedLevel = -1;

        /// <summary>是否已拥有 —— 仅反映服务器给的状态，不代表客户端做过任何计算。</summary>
        public bool IsOwned
        {
            get { return Level > 0 || Unlocked; }
        }

        /// <summary>把服务器状态写进来（原地更新，不产生对象）。</summary>
        public void ApplyState(ClientAbilityState state)
        {
            Level = state.Level;
            Unlocked = state.Unlocked;
            CooldownRemaining = state.CooldownRemaining;
            CanUpgrade = state.CanUpgrade;
        }

        /// <summary>取等级门槛数组（永不为 null，便于 `for` 循环零分配遍历）。</summary>
        public static int[] SafeLevels(int[] source)
        {
            return source != null && source.Length > 0 ? source : EmptyInts;
        }
    }

    /// <summary>
    /// 一个页签（天赋 / 秘技 / 终结技）—— 主技能 + 该技能的被动强化列表。
    /// <see cref="Primary"/> 为 null 表示**数据缺失**（例如天赋表里那 50 个英雄的 `[1,2]` 占位）
    /// ⇒ 页面显示**空态**，不是错误。
    /// </summary>
    public sealed class ClientAbilitySection
    {
        public ClientHeroAbilitySection Section;
        public string DisplayName = string.Empty;

        /// <summary>该页签的**主技能**（`Type2/3/4`）；可为 null ⇒ 空态。</summary>
        public ClientAbilityEntry Primary;

        /// <summary>该技能的被动强化（表 `Potency`）。**列表实例复用**，刷新时原地增删。</summary>
        public readonly List<ClientAbilityEntry> Enhancements = new List<ClientAbilityEntry>();

        public bool IsEmpty
        {
            get { return Primary == null && Enhancements.Count == 0; }
        }
    }

    /// <summary>
    /// 一个英雄的**完整能力** —— 页面（Controller）唯一需要拿的东西。
    ///
    /// <para>页面拿到它之后只做：`Sections[i].Primary` / `Sections[i].Enhancements` 逐个推给 View，
    /// **不需要知道** `Type → 页签` 的规则，也不需要读配置表。</para>
    /// </summary>
    public sealed class ClientHeroAbilitySet
    {
        public int HeroId;

        /// <summary>`Type1` 普攻 —— 详情页不展示，供战斗 / 其它界面使用。</summary>
        public ClientAbilityEntry BasicAttack;

        /// <summary>固定 3 个页签，顺序 = 天赋 / 秘技 / 终结技。</summary>
        public readonly List<ClientAbilitySection> Sections = new List<ClientAbilitySection>(3);

        /// <summary>按页签取；不存在返回 null。</summary>
        public ClientAbilitySection GetSection(ClientHeroAbilitySection section)
        {
            for (int index = 0; index < Sections.Count; index++)
            {
                if (Sections[index] != null && Sections[index].Section == section)
                    return Sections[index];
            }
            return null;
        }
    }
}
