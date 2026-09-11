using System.Collections.Generic;
using LC.Newtonsoft.Json.Linq;
using UnityEngine;

namespace BNRoom.Static
{
    public class SkillEffectParameters
    {
        public int EffectType;
        public int EffectTarget;
        public int EffectTargetSub1;
        public int EffectTargetSub2;
        public int EffectDuration;
        public int EffectInterval;
        public int EffectTriggerCount;
        public int EffectParam;
    }
    
    public static class GetSkillEffectParameters
    {
        public static SkillEffectParameters GetParameters(int skillID, int effectNum, int level)
        {
            SkillEffectParameters parameters = new SkillEffectParameters();
            switch (effectNum)
            {
                case 1:
                    if (TSkillHelper.GetRow(skillID).Effect1.Count < 6) return null;
                    break;
                case 2:
                    if (TSkillHelper.GetRow(skillID).Effect2.Count < 6) return null;
                    break;
                case 3:
                    if (TSkillHelper.GetRow(skillID).Effect3.Count < 6) return null;
                    break;
            }
            JArray skillData1 = TSkillHelper.GetRow(skillID).Effect1;
            JArray skillData2 = TSkillHelper.GetRow(skillID).Effect2;
            JArray skillData3 = TSkillHelper.GetRow(skillID).Effect3;
            List<JArray> skillDataList = new List<JArray>();
            skillDataList.Add(null);
            skillDataList.Add(skillData1);
            skillDataList.Add(skillData2);
            skillDataList.Add(skillData3);
            Dictionary<int,List<int>> effectTargetSub = new Dictionary<int, List<int>>();
            List<int> sub1 = new List<int>();
            List<int> sub2 = new List<int>();
            List<int> sub3 = new List<int>();
            sub1.Add(TSkillHelper.GetRow(skillID).Effect1Subtype);
            sub1.Add(TSkillHelper.GetRow(skillID).Effect1SubtypeParameter);
            sub2.Add(TSkillHelper.GetRow(skillID).Effect2Subtype);
            sub2.Add(TSkillHelper.GetRow(skillID).Effect2SubtypeParameter);
            sub3.Add(TSkillHelper.GetRow(skillID).Effect3Subtype);
            sub3.Add(TSkillHelper.GetRow(skillID).Effect3SubtypeParameter);
            effectTargetSub.Add(1,sub1);
            effectTargetSub.Add(2,sub2);
            effectTargetSub.Add(3,sub3);
            
            parameters.EffectType = (int)skillDataList[effectNum][0][0];
            parameters.EffectTarget = (int)skillDataList[effectNum][1][0];
            parameters.EffectTargetSub1 = effectTargetSub[effectNum][0];
            parameters.EffectTargetSub2 = effectTargetSub[effectNum][1];
            parameters.EffectDuration = (int)skillDataList[effectNum][2][0];
            parameters.EffectInterval = (int)skillDataList[effectNum][3][0];
            parameters.EffectTriggerCount = (int)skillDataList[effectNum][4][0];
            parameters.EffectParam = (int)skillDataList[effectNum][5][LevelCheck(skillID, level)];
            
            return parameters;
        }
        
        /// <summary>
        /// 等级检测
        /// </summary>
        private static int LevelCheck(int skillID, int level)
        { 
            JArray levelData = TSkillHelper.GetRow(skillID).EffectLevel;
            if (levelData == null) return 0;
            int dataNum = 0;
            for (int i = 0; i < levelData.Count; i++)
            {
                if (level < (int)levelData[i])
                {
                    dataNum = i - 1;
                    break;
                }
            }
            return dataNum;
        }
        
        /// <summary>
        /// 通过技能ID获取技能效果参数
        /// </summary>
        /// <returns></returns>
        public static List<SkillEffectParameters> GetSkillDataList(int skillID, int level)
        {
            SkillEffectParameters skillData1 = GetParameters(skillID, 1, level);
            SkillEffectParameters skillData2 = GetParameters(skillID, 2, level);
            SkillEffectParameters skillData3 = GetParameters(skillID, 3, level);
            List<SkillEffectParameters> skillDataList = new List<SkillEffectParameters>() { skillData1, skillData2, skillData3 };
            return skillDataList;
        }
    }
}
