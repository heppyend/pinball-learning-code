using System.Text;

namespace Pinball.Client.Stats
{
    /// <summary>
    /// **说明文本的唯一格式化入口**（"逻辑说明显示"）。
    ///
    /// <para>职责：把配置表里的**描述模板**按**当前等级**渲染成玩家可见文本。
    /// 表里的描述常带按等级分档的写法，例如
    /// `"对敌人造成60%/70%/80%攻击力的水属性伤害"` 配合 `EffectLevel = [1,10,20]`
    /// ⇒ 等级 5 取第 1 档 `60%`，等级 15 取第 2 档 `70%`。</para>
    ///
    /// <para><b>边界</b>：只做"文本选择与替换"，**不做任何数值计算**（不乘系数、不叠加加成）——
    /// 那是服务器的事。View **禁止**自己拼接字符串，一律用这里的结果。</para>
    /// </summary>
    public interface IClientDescriptionFormatter
    {
        /// <summary>
        /// 渲染说明文本。同一 <c>(abilityId, level)</c> 的结果会**缓存**，不会重复拼接、不会反复分配。
        /// </summary>
        /// <param name="abilityId">技能 / 天赋 id（只用于做缓存键）。</param>
        /// <param name="template">描述模板（表里的原文本，可为空）。</param>
        /// <param name="effectLevels">等级门槛数组（表 `EffectLevel`），可为 null/空。</param>
        /// <param name="level">当前等级（服务器下发）。</param>
        string Format(int abilityId, string template, int[] effectLevels, int level);

        /// <summary>
        /// **零分配路径**：渲染进调用方复用的 <see cref="StringBuilder"/>（适合高频刷新）。
        /// 返回值表示模板里是否存在被替换的分档片段。
        /// </summary>
        bool TryFormat(int abilityId, string template, int[] effectLevels, int level, StringBuilder buffer);

        /// <summary>清空缓存（退出 Play / 域重载 / 表更新后调用）。</summary>
        void Clear();
    }
}
