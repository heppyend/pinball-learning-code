using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TResources
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 资源名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 资源类型
    /// </summary>
    public int Int { get; set; }

    /// <summary>
    /// 动画文件名
    /// </summary>
    public string AnimationName { get; set; }

    /// <summary>
    /// 动作名
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// 缩放类型
    /// </summary>
    public int ScaleType { get; set; }

    /// <summary>
    /// 缩放x
    /// </summary>
    public int ScaleX { get; set; }

    /// <summary>
    /// 缩放y
    /// </summary>
    public int ScaleY { get; set; }

    /// <summary>
    /// 包围盒宽
    /// </summary>
    public int BbWidth { get; set; }

    /// <summary>
    /// 包围盒高
    /// </summary>
    public int BbHeight { get; set; }
}

public static class TResourcesHelper
{
    public static readonly string TableName = "Resources";
    public static readonly Type TableType = typeof(TResources);

    public static Dictionary<int, TResources> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TResources>();
        foreach (var t in rows.Cast<TResources>())
            DataMap[t.Id] = t;
    }

    public static TResources GetRow(int id)
    {
        TResources r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
