using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TMonster
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 怪物名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 等级
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 杀死获得经验
    /// </summary>
    public int Exp { get; set; }

    /// <summary>
    /// 生命
    /// </summary>
    public int HpBase { get; set; }

    /// <summary>
    /// 分段生命值显示
    /// </summary>
    public int SegHpShow { get; set; }

    /// <summary>
    /// 单段血量
    /// </summary>
    public int SegHpVal { get; set; }

    /// <summary>
    /// 攻击
    /// </summary>
    public int Attack { get; set; }

    /// <summary>
    /// 防御
    /// </summary>
    public int Defence { get; set; }

    /// <summary>
    /// 暴击率(万分比)
    /// </summary>
    public int CritRate { get; set; }

    /// <summary>
    /// 爆击伤害(万分比)
    /// </summary>
    public int CritHurt { get; set; }

    /// <summary>
    /// 技能回合
    /// </summary>
    public JArray SkillTurn { get; set; }

    /// <summary>
    /// 怪物模版
    /// </summary>
    public int TpltId { get; set; }
}

public static class TMonsterHelper
{
    public static readonly string TableName = "Monster";
    public static readonly Type TableType = typeof(TMonster);

    public static Dictionary<int, TMonster> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TMonster>();
        foreach (var t in rows.Cast<TMonster>())
            DataMap[t.Id] = t;
    }

    public static TMonster GetRow(int id)
    {
        TMonster r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
