using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TMonsterTemplate
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
    /// 元素属性
    /// </summary>
    public int Element { get; set; }

    /// <summary>
    /// 怪物类型
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// 怪物技能
    /// </summary>
    public JArray Skill { get; set; }

    /// <summary>
    /// 杀死获得经验
    /// </summary>
    public int Rewards { get; set; }

    /// <summary>
    /// 受创范围x
    /// </summary>
    public int BeAtkRangeX { get; set; }

    /// <summary>
    /// 受创范围y
    /// </summary>
    public int BeAtkRangeY { get; set; }

    /// <summary>
    /// 美术资源
    /// </summary>
    public int ResId { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string MonsterHead { get; set; }

    /// <summary>
    /// 半身像
    /// </summary>
    public string MonsterImg { get; set; }

    /// <summary>
    /// 受击音
    /// </summary>
    public int HittedScreamAudios { get; set; }

    /// <summary>
    /// 死亡音
    /// </summary>
    public int DeadAudio { get; set; }
}

public static class TMonsterTemplateHelper
{
    public static readonly string TableName = "MonsterTemplate";
    public static readonly Type TableType = typeof(TMonsterTemplate);

    public static Dictionary<int, TMonsterTemplate> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TMonsterTemplate>();
        foreach (var t in rows.Cast<TMonsterTemplate>())
            DataMap[t.Id] = t;
    }

    public static TMonsterTemplate GetRow(int id)
    {
        TMonsterTemplate r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
