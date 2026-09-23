using System;
using System.Collections.Generic;

namespace Pinball.Client.Domain
{
    /// <summary>
    /// **静态配置**（表驱动）的领域模型 —— 与「玩家状态」严格分开。
    ///
    /// 来源：`弹珠配置\excel\1服-弹珠\Client\`（`Json` = 真实数据、`Cs` = 生成类），
    /// 运行时经 `Assets/Scripts/Table/TableLoadHelper` 读取：
    /// **本地 `Assets/Resources/Table/*.json` 兜底 + OSS `{OssUrl}/{ZoneId}/Table/Data/config.pkg` 热更**。
    ///
    /// ⚠️ **id 一律是 `int`（真实契约）**，不是 `Assets/Client` 早期本地模拟用的 `"hero-001"` 这类字符串。
    /// `ClientHero`（本地模拟的玩家状态）与 `ClientHeroConfig`（表里的英雄定义）是两件事，不要混用。
    /// </summary>
    [Serializable]
    public sealed class ClientHeroConfig
    {
        /// <summary>英雄 Id（真实表：`13001` 起）。</summary>
        public int HeroId;

        public string Name;

        /// <summary>品质（真实表：`3..6`）</summary>
        public int Quality;

        /// <summary>品质图标资源名，约定 `QualityIcon_&lt;Quality&gt;`</summary>
        public string QualityIcon;

        /// <summary>弹射类型（`1..3`）</summary>
        public int CatapultType;

        /// <summary>弹射图标资源名，约定 `CatapultIcon_&lt;CatapultType&gt;`</summary>
        public string CatapultIcon;

        /// <summary>元素类型（`1..6`，**不是** `"Fire"/"Water"`）。编队页 7 个筛选按钮 = `全` + 这 6 个。</summary>
        public int Element;

        /// <summary>元素图标资源名，约定 `ElementIcon_&lt;Element&gt;`</summary>
        public string ElementIcon;

        public int Star;

        public int MaxStar;

        /// <summary>英雄头像资源名，约定 `HeroAvatar_&lt;HeroId&gt;`</summary>
        public string Avatar;

        /// <summary>半身像-卡面，约定 `HeroPortrait_&lt;HeroId&gt;_c`</summary>
        public string Portrait1;

        /// <summary>半身像-出战位，约定 `HeroPortrait_&lt;HeroId&gt;_d`</summary>
        public string Portrait2;

        /// <summary>前端是否显示（表字段 `ClientShow != 0`）</summary>
        public bool ShowInClient;

        /// <summary>该英雄的技能 id 列表（表字段 `Skill`，实测每英雄 **4** 个，指向 <see cref="ClientSkillConfig.Id"/>）。</summary>
        public List<int> Skills = new List<int>();

        /// <summary>该英雄的天赋（潜能）id 列表（表字段 `Potency`，实测每英雄 **13** 个，指向 <see cref="ClientPotencyConfig.Id"/>）。</summary>
        public List<int> Potencies = new List<int>();

        // ------------------------------------------------------------------
        // 以下数组是**服务器计算属性用的输入**（表里的逐级数值）。
        // ⚠️ 客户端**不得**用它们套公式：属性 / 战力等一律取服务器算好的值（见 `Stats/MODULE.md` §16）。
        //    目前只有"扮演服务器"的本地实现 `LocalClientStatsGateway` 读它们。
        // ------------------------------------------------------------------

        /// <summary>逐级生命（表字段 `HpBase`，字符串数组如 `"[2300,2387,…]"`；下标 = 等级-1）</summary>
        public int[] HpBase = EmptyInts;

        /// <summary>逐级攻击（表字段 `Attack`）</summary>
        public int[] Attack = EmptyInts;

        /// <summary>逐级防御（表字段 `Defence`）</summary>
        public int[] Defence = EmptyInts;

        /// <summary>逐级速度（表字段 `Speed`）</summary>
        public int[] Speed = EmptyInts;

        /// <summary>逐级暴击率（表字段 `CritRate`）</summary>
        public int[] CritRate = EmptyInts;

        /// <summary>逐级暴击伤害（表字段 `CritHurt`）</summary>
        public int[] CritHurt = EmptyInts;

        /// <summary>突破等级上限（表字段 `levelCap`，如 `"[10,20,30,40,50]"`；具体含义待确认）</summary>
        public int[] LevelCap = EmptyInts;

        private static readonly int[] EmptyInts = new int[0];
    }

    /// <summary>
    /// 收藏品条目 —— `Head` / `HeadFrame` / `Badge` / `Nameplate` / `Title` **五张表同一形状**，
    /// 规则**完全由数据给出**（不再需要人工约定）：
    /// <list type="bullet">
    /// <item><see cref="ShowInClient"/> ← `ClientShow`：前端是否显示</item>
    /// <item><see cref="ShowWhenLocked"/> ← `NotUnlockedClientShow`：**未解锁时是否显示**</item>
    /// <item><see cref="Sorting"/> ← 排序</item>
    /// <item>美术：<see cref="ResId"/>（整型资源 id）或 <see cref="Avatar"/>（资源名，仅 `Head`）</item>
    /// </list>
    /// </summary>
    [Serializable]
    public sealed class ClientCollectionConfig
    {
        public int Id;
        public string Name;
        public string Desc;
        public int Sorting;

        /// <summary>前端是否显示（`ClientShow != 0`）</summary>
        public bool ShowInClient;

        /// <summary>未解锁时是否仍然显示（`NotUnlockedClientShow != 0`）</summary>
        public bool ShowWhenLocked;

        /// <summary>美术资源 id（`Head` 之外的四张表用它）</summary>
        public int ResId;

        /// <summary>美术资源名（仅 `Head` 使用，约定 `HeroAvatar_&lt;Id&gt;`）</summary>
        public string Avatar;
    }

    /// <summary>收藏品分类 —— 对应五张同形状的表。</summary>
    public enum ClientCollectionKind
    {
        /// <summary>头像</summary>
        Head = 0,

        /// <summary>头像框</summary>
        HeadFrame = 1,

        /// <summary>徽章</summary>
        Badge = 2,

        /// <summary>铭牌</summary>
        Nameplate = 3,

        /// <summary>称号</summary>
        Title = 4,
    }

    /// <summary>
    /// 道具定义（表 `Item`，客户端 9 行）。
    ///
    /// ⚠️ **`Type` 的语义以数据为准，不要沿用早期想当然的命名**：
    /// 实测 9 行里 `Type` 只出现 **1** 与 **3** ——
    /// `1` = **货币**（点数 `110001` / 源晶 `110002` / 晶核 `110003`，描述里写明"通用货币""通过充值获得"），
    /// `3` = **消耗品**（启迪之星、共鸣结晶系列）。
    /// **`2` 在表里没有任何一行**，其含义未定义 ⇒ 不推测（见 `MODULE.md` §15）。
    /// </summary>
    [Serializable]
    public sealed class ClientItemConfig
    {
        public int Id;
        public string Name;
        public string Desc;

        /// <summary>图标资源名（表字段 `Icon`，如 `Item_110001`；注意 `120002` 的图标名是 `Item_120001`，**不要按 Id 拼**）。</summary>
        public string Icon;

        /// <summary>道具类型（`1` = 货币、`3` = 消耗品；`2` 表内未出现）。</summary>
        public int Type;

        /// <summary>品质（实测 `2..6`）</summary>
        public int Quality;

        /// <summary>堆叠上限（`0` = 不可堆叠）</summary>
        public int Stack;

        /// <summary>所在位置（`0`/`1`，具体含义待确认）</summary>
        public int Place;

        /// <summary>是否可使用</summary>
        public bool IsUse;

        /// <summary>使用值（配合 `IsUse`）</summary>
        public int IsUseValue;
    }

    /// <summary>
    /// 技能定义（表 `Skill`，33 行）。英雄通过 `THero.Skill`（每英雄 4 个）关联。
    ///
    /// ⚠️ `Type` 实测有 1 / 2 / 3 / 4，但**「哪个 Type 对应详情页的 秘技 / 终结技 页签」没有权威依据**，
    /// 记入待确认（见 `MODULE.md` §15），不得把猜测写成产品规则。
    /// </summary>
    [Serializable]
    public sealed class ClientSkillConfig
    {
        public int Id;
        public string Name;
        public string Desc;

        /// <summary>技能图标 id（表字段 `SkillIcon`，整型）</summary>
        public int SkillIcon;

        /// <summary>技能类型（实测 1..4；与详情页页签的对应关系待确认）</summary>
        public int Type;

        /// <summary>效果等级门槛（表字段 `EffectLevel`，如 `[1,10,20]`）</summary>
        public List<int> EffectLevel = new List<int>();
    }

    /// <summary>
    /// 天赋 / 潜能定义（表 `Potency`，68 行）。英雄通过 `THero.Potency`（每英雄 13 个）关联；
    /// 等级经验走 `PotencyLevel`（3 级）。
    /// </summary>
    [Serializable]
    public sealed class ClientPotencyConfig
    {
        public int Id;
        public string Name;
        public string Desc;

        /// <summary>效果类型（表字段 `EffectType`，如 `13` = 生命强化、`1` = 攻击强化、`9` = 防御强化、`19` = 速度强化）</summary>
        public int EffectType;

        /// <summary>效果参数（表字段 `EffectParameters`，如 `[18]` = +18%）</summary>
        public List<int> EffectParameters = new List<int>();

        /// <summary>品质（实测 3）</summary>
        public int Quality;

        /// <summary>图标 id（表字段 `SkillIcon`，整型）</summary>
        public int SkillIcon;
    }

    /// <summary>
    /// 随机英雄池条目（表 `RandomHero`，4 行）。
    /// ⚠️ 表里**只有 `Id` 与 `Heroid`** —— **没有**权重、价格或概率（旧生成类里的 `Weight` 在新表中不存在，恒为 0）。
    /// 抽卡消耗与概率属服务端，见 `MODULE.md` §15.2。
    /// </summary>
    [Serializable]
    public sealed class ClientRandomHeroConfig
    {
        /// <summary>池子条目 id（表字段 `Id`）</summary>
        public int Id;

        /// <summary>英雄 id（表字段 `Heroid`，指向 <see cref="ClientHeroConfig.HeroId"/>）</summary>
        public int HeroId;
    }
}
