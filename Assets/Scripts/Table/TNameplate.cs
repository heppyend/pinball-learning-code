using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TNameplate
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 铭牌名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 铭牌描述
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

public static class TNameplateHelper
{
    public static readonly string TableName = "Nameplate";
    public static readonly Type TableType = typeof(TNameplate);

    public static Dictionary<int, TNameplate> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TNameplate>();
        foreach (var t in rows.Cast<TNameplate>())
            DataMap[t.Id] = t;
    }

    public static TNameplate GetRow(int id)
    {
        TNameplate r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
