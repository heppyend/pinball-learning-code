using System.Collections.Generic;
using UnityEngine;

namespace BNRoom.Static
{
    /// <summary>
    /// 管理每个角色每个技能每个效果的独立CD和触发次数
    /// 支持：maxCooldown=0 表示无CD；maxTriggers=0 表示无次数限制
    /// </summary>
    public static class SkillCooldownManager
    {
        // 数据结构：GameObject → 技能ID → 效果索引 → 状态
        private static readonly Dictionary<GameObject, Dictionary<int, List<EffectState>>> Data 
            = new Dictionary<GameObject, Dictionary<int, List<EffectState>>>();

        private class EffectState
        {
            public readonly int maxCooldown;   // 原始CD（0表示无CD）
            public readonly int maxTriggers;   // 上限触发次数（0表示无限）
            public int currentCooldown;        // 当前剩余CD
            public int currentTriggers;        // 当前剩余触发次数

            public EffectState(int maxCd, int maxTrig)
            {
                maxCooldown = maxCd;
                maxTriggers = maxTrig;
                currentCooldown = 0;           // 初始可用
                // 如果maxTriggers==0，则设为0（表示无限，不减少）
                currentTriggers = (maxTrig == 0) ? 0 : maxTrig;
            }
        }

        /// <summary>
        /// 注册/更新一个技能效果（在初始化时调用）
        /// </summary>
        public static void RegisterSkillEffect(GameObject caster, int skillId, int effectIndex, int maxCooldown, int maxTriggerCount)
        {
            if (caster == null) return;

            if (!Data.TryGetValue(caster, out var skillDict))
            {
                skillDict = new Dictionary<int, List<EffectState>>();
                Data[caster] = skillDict;
            }

            if (!skillDict.TryGetValue(skillId, out var effectList))
            {
                effectList = new List<EffectState>();
                skillDict[skillId] = effectList;
            }

            while (effectList.Count <= effectIndex)
                effectList.Add(null);

            // 替换为新状态（重置）
            effectList[effectIndex] = new EffectState(maxCooldown, maxTriggerCount);
        }

        /// <summary>
        /// 尝试使用技能效果（包括第一次调用）
        /// </summary>
        /// <returns>true表示可用（并已扣除触发次数），false表示不可用</returns>
        public static bool UseSkillEffect(GameObject caster, int skillId, int effectIndex)
        {
            if (caster == null) return false;

            var state = GetState(caster, skillId, effectIndex);
            if (state == null) return false;

            // 检查CD：如果maxCooldown != 0 且 currentCooldown > 0，则不可用
            if (state.maxCooldown != 0 && state.currentCooldown > 0)
                return false;

            // 检查触发次数：如果maxTriggers != 0 且 currentTriggers <= 0，则不可用
            if (state.maxTriggers != 0 && state.currentTriggers <= 0)
                return false;

            // 可用：如果有次数限制（maxTriggers != 0），则减少一次
            if (state.maxTriggers != 0)
                state.currentTriggers--;

            return true;
        }

        /// <summary>
        /// 回合结束时调用：减少所有CD，重置触发次数
        /// </summary>
        public static void OnTurnEnd()
        {
            foreach (var kvp in Data)
            {
                foreach (var skillKvp in kvp.Value)
                {
                    foreach (var state in skillKvp.Value)
                    {
                        if (state == null) continue;

                        // 1. 减少CD（如果maxCooldown != 0，且当前CD>0）
                        if (state.maxCooldown != 0 && state.currentCooldown > 0)
                            state.currentCooldown--;

                        // 2. 重置触发次数（如果maxTriggers != 0，则重置为上限；否则保持0）
                        state.currentTriggers = (state.maxTriggers == 0) ? 0 : state.maxTriggers;
                    }
                }
            }
        }

        /// <summary>
        /// 清理某个角色的所有数据（在角色销毁时调用）
        /// </summary>
        public static void ClearCasterData(GameObject caster)
        {
            if (caster != null && Data.ContainsKey(caster))
                Data.Remove(caster);
        }
        
        /// <summary>
        /// 清除所有数据
        /// </summary>
        public static void ClearAllData()
        {
            Data.Clear();
        }

        /// <summary>
        /// 获取指定效果的当前状态
        /// </summary>
        public static (int cd, int triggers) GetStateInfo(GameObject caster, int skillId, int effectIndex)
        {
            var state = GetState(caster, skillId, effectIndex);
            if (state == null) return (-1, -1);
            return (state.currentCooldown, state.currentTriggers);
        }

        private static EffectState GetState(GameObject caster, int skillId, int effectIndex)
        {
            if (!Data.TryGetValue(caster, out var skillDict)) return null;
            if (!skillDict.TryGetValue(skillId, out var effectList)) return null;
            if (effectIndex < 0 || effectIndex >= effectList.Count) return null;
            return effectList[effectIndex];
        }
    }
}