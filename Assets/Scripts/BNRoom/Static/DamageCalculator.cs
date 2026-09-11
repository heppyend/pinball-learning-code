using UnityEngine;

namespace BNRoom.Static
{
    /// <summary>
    /// 伤害计算工具类
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// 计算伤害
        /// </summary>
        /// <param name="attackerAtk">攻击方攻击力</param>
        /// <param name="attackerTypeFactor">攻击方类型系数</param>
        /// <param name="attackerElement">攻击方属性：1火 2风 3土 4水 5光 6暗</param>
        /// <param name="defenderElement">防御方属性：1火 2风 3土 4水 5光 6暗</param>
        /// <param name="defenderDef">防御方防御力</param>
        /// <param name="critDamage">暴击伤害（万分比，如 15000 表示 150%）</param>
        /// <param name="critRate">暴击率（万分比，如 5000 表示 50%）</param>
        /// <param name="normalAdvantageBonus">常规克制伤害加成（如 0.3 表示 +30%）</param>
        /// <param name="normalDisadvantageBonus">常规被克制伤害加成（如 -0.15 表示 -15%）</param>
        /// <param name="specialAdvantageBonus">特殊克制伤害加成（光暗互打）</param>
        /// <param name="specialToNormalBonus">特殊打常规伤害加成</param>
        /// <param name="normalToSpecialBonus">常规打特殊伤害加成</param>
        /// <param name="attackCount">本回合已攻击次数（从1开始）</param>
        /// <returns>最终伤害值</returns>
        public static int CalculateDamage(
            int attackerAtk,
            float attackerTypeFactor,
            int attackerElement,
            int defenderElement,
            int defenderDef,
            int critDamage,
            int critRate,
            float normalAdvantageBonus,
            float normalDisadvantageBonus,
            float specialAdvantageBonus,
            float specialToNormalBonus,
            float normalToSpecialBonus,
            int attackCount)
        {
            // ---------- 1. 属性克制加成 ----------
            float elementBonus = 0f;
            bool isAttackerNormal = IsNormalElement(attackerElement);
            bool isDefenderNormal = IsNormalElement(defenderElement);

            if (isAttackerNormal && isDefenderNormal)
            {
                // 双方常规属性
                if (IsNormalAdvantage(attackerElement, defenderElement))
                {
                    elementBonus = normalAdvantageBonus;
                }
                else if (IsNormalDisadvantage(attackerElement, defenderElement))
                {
                    elementBonus = normalDisadvantageBonus;
                }
                // 否则无克制，保持0
            }
            else if (!isAttackerNormal && !isDefenderNormal)
            {
                // 双方特殊属性（光暗互打）
                elementBonus = specialAdvantageBonus;
            }
            else if (!isAttackerNormal && isDefenderNormal)
            {
                // 攻方特殊，守方常规
                elementBonus = specialToNormalBonus;
            }
            else if (isAttackerNormal && !isDefenderNormal)
            {
                // 攻方常规，守方特殊
                elementBonus = normalToSpecialBonus;
            }

            // ---------- 2. 暴击判定 ----------
            bool isCrit = Random.Range(0f, 1f) < (critRate / 10000f);
            float critMultiplier = isCrit ? (critDamage / 10000f) : 1f;

            // ---------- 3. 减伤率 ----------
            float reduction = defenderDef / (float)(defenderDef + attackerAtk);

            // ---------- 4. 攻击次数递增倍率（上限150%） ----------
            float attackMultiplier = Mathf.Min(1f + 0.05f * (attackCount - 1), 1.5f);

            // ---------- 5. 随机波动 ----------
            float randomFactor = Random.Range(0.95f, 1.05f);

            // ---------- 6. 最终伤害 ----------
            float damage = attackerAtk
                           * attackerTypeFactor
                           * (1f + elementBonus)
                           * randomFactor
                           * (1f - reduction)
                           * critMultiplier
                           * attackMultiplier;

            // ---------- 7. 四舍五入为整数 ----------
            int finalDamage = Mathf.RoundToInt(damage);
            return finalDamage;
        }

        // ---------- 辅助方法 ----------
        private static bool IsNormalElement(int element)
        {
            return element is >= 1 and <= 4;
        }

        /// <summary>
        /// 判断攻方是否常规克制守方
        /// 常规克制链：火(1)克风(2)，风(2)克土(3)，土(3)克水(4)，水(4)克火(1)
        /// </summary>
        private static bool IsNormalAdvantage(int attacker, int defender)
        {
            return (attacker == 1 && defender == 2) ||
                   (attacker == 2 && defender == 3) ||
                   (attacker == 3 && defender == 4) ||
                   (attacker == 4 && defender == 1);
        }

        /// <summary>
        /// 判断攻方是否常规被克制（即守方克攻方）
        /// </summary>
        private static bool IsNormalDisadvantage(int attacker, int defender)
        {
            return (attacker == 1 && defender == 4) ||  // 水克火
                   (attacker == 2 && defender == 1) ||  // 火克风
                   (attacker == 3 && defender == 2) ||  // 风克土
                   (attacker == 4 && defender == 3);    // 土克水
        }
    }
}