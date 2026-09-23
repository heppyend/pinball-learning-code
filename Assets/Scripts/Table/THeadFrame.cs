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

    // 2026-09-20 增量补入（只增不删）：`Desc / Sorting / ClientShow / NotUnlockedClientShow`
    // 是 `Assets/Client` 个人中心「头像框」列表表驱动的规则来源。
    // 详见 `CURRENT_STATE.md`「📦 配置表来源已确认」。

    /// <summary>描述</summary>
    public string Desc { get; set; }

    /// <summary>排序</summary>
    public int Sorting { get; set; }

    /// <summary>前端是否显示</summary>
    public int ClientShow { get; set; }

    /// <summary>未解锁时前端是否显示</summary>
    public int NotUnlockedClientShow { get; set; }
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
