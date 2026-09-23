using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class THead
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 头像名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 美术资源
    /// </summary>
    public int ResId { get; set; }

    // ------------------------------------------------------------------
    // 2026-09-20 增量补入（**只增不删**）
    // 配置表最新版（`弹珠配置\excel\1服-弹珠\Client\Cs\THead.cs`）把 `Head` 定型为
    // `Id / Name / Desc / Sorting / ClientShow / NotUnlockedClientShow / Avatar`
    // —— 其中 `Sorting`（排序）、`ClientShow`（前端是否显示）、`NotUnlockedClientShow`（未解锁是否显示）
    // 是 `Assets/Client` 个人中心「头像」列表做**表驱动**所必需的规则来源；
    // 工程里的旧版没有这些字段，届时会把全部条目当"要显示"处理。
    // `ResId` **保留**（旧代码可能在读），新增 `Avatar` 供新表使用。
    // 详见 `CURRENT_STATE.md`「📦 配置表来源已确认」。
    // ------------------------------------------------------------------

    /// <summary>描述</summary>
    public string Desc { get; set; }

    /// <summary>排序</summary>
    public int Sorting { get; set; }

    /// <summary>前端是否显示</summary>
    public int ClientShow { get; set; }

    /// <summary>未解锁时前端是否显示</summary>
    public int NotUnlockedClientShow { get; set; }

    /// <summary>头像资源名（约定 `HeroAvatar_&lt;id&gt;`）；新表以它替代整型 `ResId`</summary>
    public string Avatar { get; set; }
}

public static class THeadHelper
{
    public static readonly string TableName = "Head";
    public static readonly Type TableType = typeof(THead);

    public static Dictionary<int, THead> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, THead>();
        foreach (var t in rows.Cast<THead>())
            DataMap[t.Id] = t;
    }

    public static THead GetRow(int id)
    {
        THead r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
