using System;
using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;

namespace Pinball.Client.Stats
{
    /// <summary>
    /// **扮演权威服务器的本地实现**（服务器契约未接入前的替身）。
    ///
    /// <para>⚠️ 关键边界：**本类是"服务器侧"**，因此它<u>允许</u>读配置表、<u>允许</u>做数值计算
    /// （真实服务器就是这么算的）。这些计算**不得**泄漏到 Model / Service / View / 页面 ——
    /// 客户端其它地方只能拿到"已经算好的值"。接入真实服务器时，整个类被替换掉，别处不动。</para>
    ///
    /// <para><b>它给什么</b></para>
    /// <list type="bullet">
    /// <item><b>属性</b>：按英雄**1 级**取值（`THero.HpBase/Attack/Defence/Speed/CritRate/CritHurt` 的第 0 项）。
    /// 真实等级由服务器给，这里固定 1 级 —— 与本地模拟的英雄等级一致。</item>
    /// <item><b>战力</b>：**不下发**。战力公式未确认（`MODULE.md` §15.3），
    /// 服务器将来会给；在此之前客户端查不到该属性 ⇒ UI 显示空，**不报错、不猜公式**。</item>
    /// <item><b>能力状态</b>：技能与被动一律给"1 级、已解锁、可升级"。真实的等级/冷却/是否可升级由服务器判定。</item>
    /// </list>
    ///
    /// <para><b>GC</b>：<see cref="CopyAttributes"/> 直接写进调用方的列表（先清空），不产生新对象；
    /// <see cref="TryGetAbilityState"/> 用 <c>struct</c> 出参，零分配。</para>
    /// </summary>
    public sealed class LocalClientStatsGateway : IClientStatsGateway
    {
        /// <summary>本地模拟产出的技能/天赋等级（真实值由服务器给）。</summary>
        private const int SimulatedAbilityLevel = 1;

        private readonly IClientConfigService _config;

        public event Action<int> Changed;

        public LocalClientStatsGateway(IClientConfigService config)
        {
            _config = config;
        }

        /// <summary>通知订阅者"数据变了"（本地模拟用；真实环境由服务器推送触发）。</summary>
        public void NotifyChanged(int ownerId)
        {
            Action<int> handler = Changed;
            if (handler != null)
                handler(ownerId);
        }

        public void CopyAttributes(int ownerId, List<ClientAttributeValue> destination)
        {
            if (destination == null)
                return;

            destination.Clear();

            if (_config == null || !_config.IsReady)
                return;

            ClientHeroConfig hero;
            if (!_config.TryGetHero(ownerId, out hero) || hero == null)
                return;

            // 「服务器」按 1 级取属性（数组第 0 项）。
            AddIfPresent(destination, ClientAttributeIds.Hp, hero.HpBase);
            AddIfPresent(destination, ClientAttributeIds.Attack, hero.Attack);
            AddIfPresent(destination, ClientAttributeIds.Defence, hero.Defence);
            AddIfPresent(destination, ClientAttributeIds.Speed, hero.Speed);
            AddIfPresent(destination, ClientAttributeIds.CritRate, hero.CritRate);
            AddIfPresent(destination, ClientAttributeIds.CritHurt, hero.CritHurt);

            // 战力：公式未确认 ⇒ 服务器暂不下发（见类注释）。
        }

        public bool TryGetAbilityState(int abilityId, out ClientAbilityState state)
        {
            state = default(ClientAbilityState);

            if (_config == null || !_config.IsReady)
                return false;

            ClientSkillConfig skill;
            if (_config.TryGetSkill(abilityId, out skill) && skill != null)
            {
                state.AbilityId = abilityId;
                state.Level = SimulatedAbilityLevel;
                state.Unlocked = true;
                state.CanUpgrade = true;
                return true;
            }

            ClientPotencyConfig potency;
            if (_config.TryGetPotency(abilityId, out potency) && potency != null)
            {
                state.AbilityId = abilityId;
                state.Level = SimulatedAbilityLevel;
                state.Unlocked = true;
                state.CanUpgrade = true;
                return true;
            }

            return false;
        }

        /// <summary>取数组第 0 项写进列表；数组为空则**跳过**（属性缺失不是错误，UI 显示空）。</summary>
        private static void AddIfPresent(List<ClientAttributeValue> destination, int attributeId, int[] values)
        {
            if (values == null || values.Length == 0)
                return;

            int level = SimulatedAbilityLevel > 0 ? SimulatedAbilityLevel - 1 : 0;
            if (level >= values.Length)
                level = values.Length - 1;

            destination.Add(new ClientAttributeValue(attributeId, values[level]));
        }
    }
}
