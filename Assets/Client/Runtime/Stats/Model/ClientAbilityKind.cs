namespace Pinball.Client.Stats
{
    /// <summary>能力的种类：技能 或 被动加成（天赋表 `Potency`）。</summary>
    public enum ClientAbilityKind
    {
        /// <summary>技能（表 `Skill`）</summary>
        Skill = 1,

        /// <summary>被动加成 / 天赋（表 `Potency`）</summary>
        Potency = 2,
    }
}
