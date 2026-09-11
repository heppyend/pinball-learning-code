using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TTitle
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 称号名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 美术资源
    /// </summary>
    public int ResId { get; set; }
}

public static class TTitleHelper
{
    public static readonly string TableName = "Title";
    public static readonly Type TableType = typeof(TTitle);

    public static Dictionary<int, TTitle> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TTitle>();
        foreach (var t in rows.Cast<TTitle>())
            DataMap[t.Id] = t;
    }

    public static TTitle GetRow(int id)
    {
        TTitle r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
