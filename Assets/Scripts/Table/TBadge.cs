using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TBadge
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 勋章名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 勋章描述
    /// </summary>
    public string Desc { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sorting { get; set; }

    /// <summary>
    /// 前端屏蔽显示
    /// </summary>
    public int ClientShow { get; set; }

    /// <summary>
    /// 未解锁是否显示
    /// </summary>
    public int NotUnlockedClientShow { get; set; }

    /// <summary>
    /// 美术资源
    /// </summary>
    public int ResId { get; set; }
}

public static class TBadgeHelper
{
    public static readonly string TableName = "Badge";
    public static readonly Type TableType = typeof(TBadge);

    public static Dictionary<int, TBadge> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TBadge>();
        foreach (var t in rows.Cast<TBadge>())
            DataMap[t.Id] = t;
    }

    public static TBadge GetRow(int id)
    {
        TBadge r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
