using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TPotencyLevel
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 获取途径
    /// </summary>
    public int Exp { get; set; }
}

public static class TPotencyLevelHelper
{
    public static readonly string TableName = "PotencyLevel";
    public static readonly Type TableType = typeof(TPotencyLevel);

    public static Dictionary<int, TPotencyLevel> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TPotencyLevel>();
        foreach (var t in rows.Cast<TPotencyLevel>())
            DataMap[t.Id] = t;
    }

    public static TPotencyLevel GetRow(int id)
    {
        TPotencyLevel r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
