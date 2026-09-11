using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TRandomHero
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 随机权重
    /// </summary>
    public int Weight { get; set; }
}

public static class TRandomHeroHelper
{
    public static readonly string TableName = "RandomHero";
    public static readonly Type TableType = typeof(TRandomHero);

    public static Dictionary<int, TRandomHero> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TRandomHero>();
        foreach (var t in rows.Cast<TRandomHero>())
            DataMap[t.Id] = t;
    }

    public static TRandomHero GetRow(int id)
    {
        TRandomHero r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
