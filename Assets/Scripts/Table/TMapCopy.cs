using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TMapCopy
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 地图名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 是否开启
    /// </summary>
    public int Open { get; set; }

    /// <summary>
    /// 场景类型
    /// </summary>
    public int SceneType { get; set; }

    /// <summary>
    /// 场景子类
    /// </summary>
    public int SceneSubclass { get; set; }

    /// <summary>
    /// 星级难度
    /// </summary>
    public int StarDifficulty { get; set; }

    /// <summary>
    /// 前置场景
    /// </summary>
    public int PreCopy { get; set; }

    /// <summary>
    /// 通关奖励
    /// </summary>
    public JArray Rewards { get; set; }

    /// <summary>
    /// 关卡章节
    /// </summary>
    public int StageCnt { get; set; }

    /// <summary>
    /// 通关时间
    /// </summary>
    public int FightTime { get; set; }

    /// <summary>
    /// 通关时间标准
    /// </summary>
    public JArray CopyPasstime { get; set; }

    /// <summary>
    /// 最高连击标准
    /// </summary>
    public JArray CopyMaxhits { get; set; }

    /// <summary>
    /// 受击数标准
    /// </summary>
    public JArray CopyHitby { get; set; }

    /// <summary>
    /// 场景音乐
    /// </summary>
    public JArray Music { get; set; }

    /// <summary>
    /// 副本图片
    /// </summary>
    public string CopyIcon { get; set; }

    /// <summary>
    /// 展示标签
    /// </summary>
    public int DisplayLabel { get; set; }

    /// <summary>
    /// 推荐战斗力
    /// </summary>
    public int Fightpoint { get; set; }

    /// <summary>
    /// 地图描述
    /// </summary>
    public string CopyDesc { get; set; }
}

public static class TMapCopyHelper
{
    public static readonly string TableName = "MapCopy";
    public static readonly Type TableType = typeof(TMapCopy);

    public static Dictionary<int, TMapCopy> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TMapCopy>();
        foreach (var t in rows.Cast<TMapCopy>())
            DataMap[t.Id] = t;
    }

    public static TMapCopy GetRow(int id)
    {
        TMapCopy r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
