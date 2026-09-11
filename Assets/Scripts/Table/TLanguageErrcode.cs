 using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TLanguageErrcode
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 文本Key
    /// </summary>
    public string key { get; set; }

    /// <summary>
    /// 文本内容
    /// </summary>
    public string CN { get; set; }
}

public static class TLanguageErrcodeHelper
{
    public static readonly string TableName = "LanguageErrcode";
    public static readonly Type TableType = typeof(TLanguageErrcode);

    public static Dictionary<int, TLanguageErrcode> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TLanguageErrcode>();
        foreach (var t in rows.Cast<TLanguageErrcode>())
            DataMap[t.Id] = t;
    }

    public static TLanguageErrcode GetRow(int id)
    {
        TLanguageErrcode r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
