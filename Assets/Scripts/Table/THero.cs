using LC.Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class THero
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 英雄名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 品质
    /// </summary>
    public int Quality { get; set; }

    /// <summary>
    /// 品质图标
    /// </summary>
    public string QualityIcon { get; set; }

    /// <summary>
    /// 弹射类型
    /// </summary>
    public int CatapultType { get; set; }

    /// <summary>
    /// 弹射图标
    /// </summary>
    public string CatapultIcon { get; set; }

    /// <summary>
    /// 元素类型
    /// </summary>
    public int Element { get; set; }

    /// <summary>
    /// 元素图标
    /// </summary>
    public string ElementIcon { get; set; }

    /// <summary>
    /// 初始星级
    /// </summary>
    public int Star { get; set; }

    /// <summary>
    /// 最高星级
    /// </summary>
    public int MaxStar { get; set; }

    /// <summary>
    /// 升星所需道具
    /// </summary>
    public int StarupItem { get; set; }

    /// <summary>
    /// 等级上限
    /// </summary>
    public JArray levelCap { get; set; }

    /// <summary>
    /// 装备孔位解锁等级
    /// </summary>
    public JArray LnlayUnlock { get; set; }

    /// <summary>
    /// 生命
    /// </summary>
    public JArray HpBase { get; set; }

    /// <summary>
    /// 攻击
    /// </summary>
    public JArray Attack { get; set; }

    /// <summary>
    /// 防御
    /// </summary>
    public JArray Defence { get; set; }

    /// <summary>
    /// 速度
    /// </summary>
    public JArray Speed { get; set; }

    /// <summary>
    /// 暴击率(万分比)
    /// </summary>
    public JArray CritRate { get; set; }

    /// <summary>
    /// 爆击伤害(万分比)
    /// </summary>
    public JArray CritHurt { get; set; }

    /// <summary>
    /// 天赋潜能库
    /// </summary>
    public JArray Potency { get; set; }

    /// <summary>
    /// 技能ID
    /// </summary>
    public JArray Skill { get; set; }

    /// <summary>
    /// 抽卡获得英雄奖励
    /// </summary>
    public JArray Rewards { get; set; }

    /// <summary>
    /// 前端是否显示
    /// </summary>
    public int ClientShow { get; set; }

    /// <summary>
    /// 美术资源
    /// </summary>
    public int ResId { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string Avatar { get; set; }

    /// <summary>
    /// 半身像
    /// </summary>
    public string Portrait { get; set; }

    /// <summary>
    /// 半身像-卡面
    /// ⚠️ 2026-09-20 增量补入：最新配置表（`弹珠配置\excel\1服-弹珠\Client\Cs\THero.cs`）把旧字段 `Portrait`
    /// 拆成了 `Portrait1`(卡面) / `Portrait2`(出战位)，而 `Assets/Client` 的卡牌需要按名加载立绘。
    /// **`Portrait` 予以保留** —— 原项目 `Assets/Scripts/BNRoom/UI/MarbleUI.cs:1148` 仍在读它，
    /// 删掉会破坏原项目编译。三个字段并存，各取所需。
    /// </summary>
    public string Portrait1 { get; set; }

    /// <summary>
    /// 半身像-出战位（同上，增量补入）
    /// </summary>
    public string Portrait2 { get; set; }
}

public static class THeroHelper
{
    public static readonly string TableName = "Hero";
    public static readonly Type TableType = typeof(THero);

    public static Dictionary<int, THero> DataMap;

    public static void LoadData(List<object> rows)
    {
        DataMap = new Dictionary<int, THero>();
        foreach (var t in rows.Cast<THero>())
            DataMap[t.Id] = t;
    }

    public static THero GetRow(int id)
    {
        THero r = null;
        return DataMap.TryGetValue(id, out r) ? r : null;
    }
}
