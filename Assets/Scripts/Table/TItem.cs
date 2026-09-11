using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TItem
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 物品名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 物品说明
    /// </summary>
    public string Desc { get; set; }

    /// <summary>
    /// 物品图标资源ID
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 物品类型
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// 品质
    /// </summary>
    public int Quality { get; set; }

    /// <summary>
    /// 堆叠上限
    /// </summary>
    public int Stack { get; set; }

    /// <summary>
    /// 背包页签
    /// </summary>
    public int Place { get; set; }

    /// <summary>
    /// 交互使用方式
    /// </summary>
    public int is_use { get; set; }

    /// <summary>
    /// 使用类型参数
    /// </summary>
    public int is_use_value { get; set; }
}

public static class TItemHelper
{
    public static readonly string TableName = "Item";
    public static readonly Type TableType = typeof(TItem);

    public static Dictionary<int, TItem> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TItem>();
        foreach (var t in rows.Cast<TItem>())
            DataMap[t.Id] = t;
    }

    public static TItem GetRow(int id)
    {
        TItem r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
