using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class TCoefficient
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 攻方类型系数
    /// </summary>
    public JArray AttackCoefficient { get; set; }

    /// <summary>
    /// 常规克制伤害系数
    /// </summary>
    public double BaseRestrain { get; set; }

    /// <summary>
    /// 常规被克制伤害系数
    /// </summary>
    public double BaseBeRestrain { get; set; }

    /// <summary>
    /// 特殊克制伤害系数
    /// </summary>
    public double SpecialRestrain { get; set; }

    /// <summary>
    /// 特殊常规伤害系数
    /// </summary>
    public double SpecialBaseRestrain { get; set; }

    /// <summary>
    /// 常规特殊伤害系数
    /// </summary>
    public double BaseSpecialRestrain { get; set; }

    /// <summary>
    /// 随机波动
    /// </summary>
    public JArray RandomFluctuation { get; set; }

    /// <summary>
    /// 生命战力系数
    /// </summary>
    public double HPFight { get; set; }

    /// <summary>
    /// 攻击战力系数
    /// </summary>
    public double AttackFight { get; set; }

    /// <summary>
    /// 防御战力系数
    /// </summary>
    public double DefenceFight { get; set; }

    /// <summary>
    /// 速度战力系数
    /// </summary>
    public double SpeedFight { get; set; }
}

public static class TCoefficientHelper
{
    public static readonly string TableName = "Coefficient";
    public static readonly Type TableType = typeof(TCoefficient);

    public static Dictionary<int, TCoefficient> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, TCoefficient>();
        foreach (var t in rows.Cast<TCoefficient>())
            DataMap[t.Id] = t;
    }

    public static TCoefficient GetRow(int id)
    {
        TCoefficient r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
