using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class THead
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 头像名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 美术资源
    /// </summary>
    public int ResId { get; set; }
}

public static class THeadHelper
{
    public static readonly string TableName = "Head";
    public static readonly Type TableType = typeof(THead);

    public static Dictionary<int, THead> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, THead>();
        foreach (var t in rows.Cast<THead>())
            DataMap[t.Id] = t;
    }

    public static THead GetRow(int id)
    {
        THead r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
