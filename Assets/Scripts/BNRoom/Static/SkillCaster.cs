using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNRoom.Static
{
    public static class SkillCaster
    {
        public static void CastSkill(
            int skillID,
            int level,
            int attackDamage,
            int element,
            int criticalDamage,
            int criticalRate,
            float attackerTypeFactor,
            GameObject caster,
            GameObject originalTarget,
            int marbleNum = -1
            )
        {
            var skillDataList = GetSkillEffectParameters.GetSkillDataList(skillID,level);
            List<GameObject> targets = new List<GameObject>() { originalTarget };
            int skillType = TSkillHelper.GetRow(skillID).Type;

            for (int i = 0; i < skillDataList.Count; i++)
            {
                if (skillType != 5 && skillType != 6)
                {
                    bool isSkillUsable = SkillCooldownManager.UseSkillEffect(caster, skillID, i);
                    if (!isSkillUsable) continue;
                }
                var skillData = skillDataList[i];
                if (skillData == null) break;
                int attackType = GetAttackType(skillID);
                if (skillData.EffectTarget is 1 or 5 && (attackType != 0 || skillData.EffectTargetSub1 != 0))
                {
                    targets = TargetFinder.FindTargets(
                        caster.transform.position,
                        caster.tag,
                        skillData.EffectTarget,
                        skillData.EffectTargetSub1,
                        skillData.EffectTargetSub2
                    );
                }
                else if (skillData.EffectTarget == 2)
                {
                    targets = new List<GameObject>() { caster };
                }
                
                switch (skillData.EffectType)
                {
                    case 1 or 2:
                        // 计算攻击力
                        int damage = CheckEffectType(skillData.EffectType, skillData.EffectParam, attackDamage);
                        foreach (var target in targets)
                        {
                            if (attackType == 0)
                            {
                                if (target.GetComponent<BNEnemyMarbleLogic>() != null)
                                {
                                    BNEnemyMarbleLogic enemyMarbleLogic = target.GetComponent<BNEnemyMarbleLogic>();
                                    // 造成伤害
                                    enemyMarbleLogic.TakeDamage(
                                        damage,
                                        attackerTypeFactor,
                                        element,
                                        criticalDamage,
                                        criticalRate,
                                        marbleNum
                                    );
                                }
                                else if (target.GetComponent<BNMarbleLogic>() != null)
                                {
                                    BNMarbleLogic marbleLogic = target.GetComponent<BNMarbleLogic>();
                                    // 造成伤害
                                    marbleLogic.GetHit(
                                        damage,
                                        attackerTypeFactor,
                                        element,
                                        criticalDamage,
                                        criticalRate
                                    );
                                }
                                
                            }
                            else if (attackType == 1)
                            {
                                // 远程攻击
                                GameObject bullet = ObjectPoolManager.Instance.Spawn(
                                    ResManager.Instance.LoadPrefab(SysDefines.MARBLEEFFECTS + (EnumElementType)element + "Element/" + "Attack"),
                                    caster.transform.position,
                                    Quaternion.identity,
                                    BNGameManager.Instance.EffectRoom
                                );
                                bullet.GetComponent<BNBulletLogic>().Init(
                                    damage,
                                    attackerTypeFactor,
                                    element,
                                    criticalDamage,
                                    criticalRate,
                                    skillData.EffectType,
                                    skillData.EffectParam,
                                    target,
                                    marbleNum
                                );
                            }
                        }
                        break;
                    case 3 or 4 or 5 or 6 or 7 or 8 or 9 or 10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 18 or 19 or 20 or 21 or 22:
                        foreach (var target in targets)
                        {
                            BuffManager.AddBuff(target, (EnumEffectValueType)skillData.EffectType, skillData.EffectDuration, skillData.EffectParam);
                            ObjectPoolManager.Instance.Spawn(
                                ResManager.Instance.LoadPrefab(SysDefines.MARBLEEFFECTS + (EnumEffectValueType)skillData.EffectType),
                                Vector3.zero,
                                Quaternion.identity,
                                target.transform
                            );
                        }
                        break;
                }
            }
        }
        
        /// <summary>
        /// 获取攻击类型{0:近战，1:远程}
        /// </summary>
        private static int GetAttackType(int skillID)
        {
            int attackType = TSkillHelper.GetRow(skillID).Atktype;
            return attackType;
        }
        
        /// <summary>
        /// 检测效果类型
        /// </summary>
        private static int CheckEffectType(int effectType,int effectParam,int needToChangeValue)
        {
            int value = 0;
            switch ((EnumEffectValueType)effectType)
            {
                case EnumEffectValueType.ReduceHpByPercentageOfAttack:
                    value = (int)(needToChangeValue * (effectParam / 100f));
                    break;
                case EnumEffectValueType.ReduceHp:
                    value = effectParam;
                    break;
            }
            return value;
        }
    }
}
