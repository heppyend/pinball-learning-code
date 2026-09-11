using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class THeadFrame
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 头像框名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 美术资源
    /// </summary>
    public int ResId { get; set; }
}

public static class THeadFrameHelper
{
    public static readonly string TableName = "HeadFrame";
    public static readonly Type TableType = typeof(THeadFrame);

    public static Dictionary<int, THeadFrame> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, THeadFrame>();
        foreach (var t in rows.Cast<THeadFrame>())
            DataMap[t.Id] = t;
    }

    public static THeadFrame GetRow(int id)
    {
        THeadFrame r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
