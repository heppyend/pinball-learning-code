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
        }; 
    }
    
}

