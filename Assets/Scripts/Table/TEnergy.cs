using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TEnergy
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 英雄绝招能量上限
    /// </summary>
    public int HeroLimit { get; set; }

    /// <summary>
    /// 击杀小怪增加能量
    /// </summary>
    public int KillMinMonster { get; set; }

    /// <summary>
    /// 击杀精英怪增加能量
    /// </summary>
    public int KillEliteMonster { get; set; }

    /// <summary>
    /// 击杀小Boss增加能量
    /// </summary>
    public int KillMinBoss { get; set; }

    /// <summary>
    /// 击杀Boss增加能量
    /// </summary>
    public int KillBoss { get; set; }

    /// <summary>
    /// 命中怪物增加能量
    /// </summary>
    public int Assist { get; set; }
}

public static class TEnergyHelper
{
    public static readonly string TableName = "Energy";
    public static readonly Type TableType = typeof(TEnergy);

    public static Dictionary<int, TEnergy> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TEnergy>();
        foreach (var t in rows.Cast<TEnergy>())
            DataMap[t.Id] = t;
    }

    public static TEnergy GetRow(int id)
    {
        TEnergy r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
