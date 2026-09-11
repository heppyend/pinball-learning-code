using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TAudios
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 声音名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 声音资源名
    /// </summary>
    public string ResName { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public int ActionName { get; set; }

    /// <summary>
    /// 是否循环播放
    /// </summary>
    public int ScaleType { get; set; }

    /// <summary>
    /// 音量
    /// </summary>
    public int ScaleX { get; set; }
}

public static class TAudiosHelper
{
    public static readonly string TableName = "Audios";
    public static readonly Type TableType = typeof(TAudios);

    public static Dictionary<int, TAudios> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TAudios>();
        foreach (var t in rows.Cast<TAudios>())
            DataMap[t.Id] = t;
    }

    public static TAudios GetRow(int id)
    {
        TAudios r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
