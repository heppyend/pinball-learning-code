using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TPotency
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 潜能名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 潜能描述
    /// </summary>
    public string Desc { get; set; }

    /// <summary>
    /// 潜能效果
    /// </summary>
    public int EffectType { get; set; }

    /// <summary>
    /// 效果参数数值
    /// </summary>
    public JArray EffectParameters { get; set; }

    /// <summary>
    /// 品质
    /// </summary>
    public int Quality { get; set; }

    /// <summary>
    /// 潜能图标
    /// </summary>
    public int SkillIcon { get; set; }
}

public static class TPotencyHelper
{
    public static readonly string TableName = "Potency";
    public static readonly Type TableType = typeof(TPotency);

    public static Dictionary<int, TPotency> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TPotency>();
        foreach (var t in rows.Cast<TPotency>())
            DataMap[t.Id] = t;
    }

    public static TPotency GetRow(int id)
    {
        TPotency r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
