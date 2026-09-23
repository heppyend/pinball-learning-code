namespace Pinball.Client.Stats
{
    /// <summary>
    /// **技能 `Type` → 页签** 的映射规则 —— **唯一可替换点**。
    ///
    /// <para>来源：2026-09-20 由配置数据推出（证据链见 `Assets/Client/MODULE.md` §15.7）：
    /// `Type2` = 天赋、`Type3` = 秘技、`Type4` = 终结技；`Type1` 是普攻，不进详情页。
    /// 依据是两条独立证据：① 天赋表里点名提到技能的行（`秘技强化` → 月华追踪 = Type3、
    /// `紧箍觉醒`("终结技…") → 紧箍咒 = Type4）；② 冷却特征（`Type2/3` 有冷却、`Type4` 无冷却但耗能量）。</para>
    ///
    /// <para><b>产品给了正式定义后，只改这张表即可</b> —— <see cref="ClientStatsService"/> 与所有 View / 页面都不用动。</para>
    /// </summary>
    public sealed class ClientAbilitySectionRule
    {
        public ClientAbilitySectionRule(ClientHeroAbilitySection section, string displayName, int skillType, bool catchUnmatchedPotency)
        {
            Section = section;
            DisplayName = displayName;
            SkillType = skillType;
            CatchUnmatchedPotency = catchUnmatchedPotency;
        }

        /// <summary>页签身份。</summary>
        public ClientHeroAbilitySection Section;

        /// <summary>页签显示名（"天赋" / "秘技" / "终结技"）。</summary>
        public string DisplayName;

        /// <summary>本页签的主技能取哪个 `TSkill.Type`。</summary>
        public int SkillType;

        /// <summary>
        /// 被动（表 `Potency`）**无法按描述关联到任何主技能**时，是否兜底挂到本页签。
        /// 目前只有"天赋"为 `true`。
        ///
        /// <para>⚠️ 被动的归属目前是**弱关联**（靠"描述里是否出现了主技能名"判断，例如
        /// `300110 秘技强化` 的描述含"月华追踪"⇒ 归秘技）。产品若给出强结构，替换
        /// <see cref="ClientStatsService"/> 里的归属方法即可。</para>
        /// </summary>
        public bool CatchUnmatchedPotency;
    }
}
