namespace Pinball.Client.Stats
{
    /// <summary>
    /// 英雄详情页的能力页签 —— 与 UI 上的 `天赋Toggle` / `秘技Toggle` / `终结技Toggle` 一一对应。
    ///
    /// <para>与技能 `Type` 的对应关系由 <see cref="ClientAbilitySectionRule"/> 唯一定义（2026-09-20 由数据推出，
    /// 见 `Assets/Client/MODULE.md` §15.7）：`Type2` → 天赋、`Type3` → 秘技、`Type4` → 终结技；
    /// `Type1` 是普攻，**不在详情页展示**（放在 <see cref="ClientHeroAbilitySet.BasicAttack"/>）。</para>
    /// </summary>
    public enum ClientHeroAbilitySection
    {
        /// <summary>天赋（技能 `Type = 2`）</summary>
        Talent = 1,

        /// <summary>秘技（技能 `Type = 3`）</summary>
        SecretTechnique = 2,

        /// <summary>终结技（技能 `Type = 4`）</summary>
        Ultimate = 3,
    }
}
