 using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TLanguage
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 功能文本字段
    /// </summary>
    public string key { get; set; }

    /// <summary>
    /// 内容
    /// 保留指定位数的小数{2:N1|a/10000}  代表保留1个小数
    /// 默认  {2|a/10000} 取整，不保留小数（11服功能）
    /// </summary>
    public string CN { get; set; }
}

public static class TLanguageHelper
{
    public static readonly string TableName = "Language";
    public static readonly Type TableType = typeof(TLanguage);

    public static Dictionary<int, TLanguage> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TLanguage>();
        foreach (var t in rows.Cast<TLanguage>())
            DataMap[t.Id] = t;
    }

    public static TLanguage GetRow(int id)
    {
        TLanguage r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
