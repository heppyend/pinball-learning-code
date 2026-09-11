using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TDan
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 徽章名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 美术资源
    /// </summary>
    public int ResId { get; set; }
}

public static class TDanHelper
{
    public static readonly string TableName = "Dan";
    public static readonly Type TableType = typeof(TDan);

    public static Dictionary<int, TDan> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TDan>();
        foreach (var t in rows.Cast<TDan>())
            DataMap[t.Id] = t;
    }

    public static TDan GetRow(int id)
    {
        TDan r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
