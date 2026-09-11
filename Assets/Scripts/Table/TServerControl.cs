using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TServerControl
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 登录模式
    /// </summary>
    public JArray LoginType { get; set; }
}

public static class TServerControlHelper
{
    public static readonly string TableName = "ServerControl";
    public static readonly Type TableType = typeof(TServerControl);

    public static Dictionary<int, TServerControl> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TServerControl>();
        foreach (var t in rows.Cast<TServerControl>())
            DataMap[t.Id] = t;
    }

    public static TServerControl GetRow(int id)
    {
        TServerControl r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
