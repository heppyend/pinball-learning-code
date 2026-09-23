/******************************************************************************
 * 
 *  Title:  弹珠项目
 *
 *  Version:  1.0版
 *
 *  Description:
 *         1：配置表加载
 *
 *  Author:  BingNan
 *       
 *  Date:  2026-07-13
 * 
 ******************************************************************************/
using System;
using System.Collections.Generic;

public class TableLoadConfig
{
    public string LocalJsonDirectory { get; set; }

    public List<Type> TableHelperTypeList { get; set; }

    public TableLoadConfig()
    {
        LocalJsonDirectory = "Table/";

        TableHelperTypeList = new List<Type>()
        {
            typeof(TAudiosHelper),
            typeof(TCoefficientHelper),
            typeof(TDanHelper),
            typeof(TEnergyHelper),
            typeof(THeadHelper),
            typeof(THeadFrameHelper),
            typeof(THeroHelper),
            typeof(TItemHelper),
            typeof(TMapCopyHelper),
            typeof(TMonsterHelper),
            typeof(TMonsterTemplateHelper),
            typeof(TPinBallRoomHelper),
            typeof(TRandomHeroHelper),
            typeof(TResourcesHelper),
            typeof(TServerControlHelper),
            typeof(TSkillHelper),
            typeof(TTitleHelper),
            // 2026-09-20 补入：配置目录（弹珠配置\excel\1服-弹珠\Client\Cs）里生成、工程里此前缺失的 4 张表。
            // 缺它们正是 `Assets/Client` 的「徽章 / 铭牌」与「英雄天赋潜能」一直没有数据源的原因
            // （不是"待确认"，是**生成类没导进来**、也没在本清单里）。见 CURRENT_STATE.md「📦 配置表来源已确认」。
            typeof(TBadgeHelper),
            typeof(TNameplateHelper),
            typeof(TPotencyHelper),
            typeof(TPotencyLevelHelper),
        }; 
    }
    
}

