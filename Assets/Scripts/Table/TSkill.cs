using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TSkill
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 技能名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 技能描述
    /// </summary>
    public string Desc { get; set; }

    /// <summary>
    /// 技能图标
    /// </summary>
    public int SkillIcon { get; set; }

    /// <summary>
    /// 技能类型
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// 对应解锁等级
    /// </summary>
    public JArray EffectLevel { get; set; }

    /// <summary>
    /// 效果参数1
    /// </summary>
    public JArray Effect1 { get; set; }

    /// <summary>
    /// 效果1目标子类型
    /// </summary>
    public int Effect1Subtype { get; set; }

    /// <summary>
    /// 效果1目标子类型个数
    /// </summary>
    public int Effect1SubtypeParameter { get; set; }

    /// <summary>
    /// 效果参数2
    /// </summary>
    public JArray Effect2 { get; set; }

    /// <summary>
    /// 效果2目标子类型
    /// </summary>
    public int Effect2Subtype { get; set; }

    /// <summary>
    /// 效果2目标子类型个数
    /// </summary>
    public int Effect2SubtypeParameter { get; set; }

    /// <summary>
    /// 效果参数3
    /// </summary>
    public JArray Effect3 { get; set; }

    /// <summary>
    /// 效果3目标子类型
    /// </summary>
    public int Effect3Subtype { get; set; }

    /// <summary>
    /// 效果3目标子类型个数
    /// </summary>
    public int Effect3SubtypeParameter { get; set; }

    /// <summary>
    /// 美术资源
    /// </summary>
    public string ResId { get; set; }

    /// <summary>
    /// 攻击类型
    /// </summary>
    public int Atktype { get; set; }
}

public static class TSkillHelper
{
    public static readonly string TableName = "Skill";
    public static readonly Type TableType = typeof(TSkill);

    public static Dictionary<int, TSkill> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TSkill>();
        foreach (var t in rows.Cast<TSkill>())
            DataMap[t.Id] = t;
    }

    public static TSkill GetRow(int id)
    {
        TSkill r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
