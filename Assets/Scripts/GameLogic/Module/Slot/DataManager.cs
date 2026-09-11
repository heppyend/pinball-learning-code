using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QT.Stage.GameSlotSparrow25
{
    public class DataManager
    {
        public const int m_iVerUI = 0;        //UI版本号

        public static WaitForSeconds m_Delay05 = new WaitForSeconds(0.5f);
        public static WaitForSeconds m_Delay1 = new WaitForSeconds(1f);
        public static WaitForSeconds m_Delay2 = new WaitForSeconds(2f);

        public const int m_iKeepDecPoint = 2;      //分数保留几位小数点
        public static string m_strKeepDecPoint = "f" + m_iKeepDecPoint;

        public const int m_iItem_9 = 9;            //（百搭1）
        //public const int m_iItem_10 = 28;            //（百搭2）
        public const int m_iItem_11 = 20;            //庄（免费）
        //public const int m_iItem_12 = 12;            //彩金

        public const int m_iItemCol = 3;//5;                         //5列
        public const int m_iItemRow = 1;                         //3行
        public const int m_iItemResCount = 6;//21;                   //每套item资源数量，最后一张为透明图
        public const int m_iLinesCount = 8;                     //总线数

        //=======彩金、说明========
        public const int m_iJackpotCount = 3;                 //彩金总数
        public const int m_iRuleCount = 4;                    //说明页数
        public const int m_iPayTypeCountGen = 8;              //几个item有普通倍率
        public const int m_iPayTypeCountGenSpe = 4;           //每个普通倍率有几种种，中5个，中4个...

        public static readonly List<string> m_list_StrGObjName = new List<string>()       //玩家游戏体名字
        {
            "Image_BG_Player","Image_Logo","Panel_Below","Panel_Effect","Panel_FreeSpinPar","Panel_ItemAnim",
            "Panel_ItemAnim_Bonus","Panel_JP","Panel_JPTop","Panel_Lines","Panel_LinesFrame","Panel_LinesScore",
            "Panel_PlayerJackpot","Panel_PrizeNum","Panel_Prompt","Panel_Rule","Panel_SeatNumP","Panel_Trans","Panel_Middle","Panel_Middle_1"
        };

        public const int m_iItem_Jackpot0 = 11;            //彩金0
        public const int m_iItem_Jackpot1 = 12;            //彩金1
        public const int m_iItem_Jackpot2 = 13;            //彩金2
        public const int m_iItem_Jackpot3 = 14;            //彩金3

        //==============游戏场景==============
        private static GameObject m_gRoot;
        public static GameObject M_gRoot
        {
            get
            {
                if (m_gRoot == null)
                {
                    m_gRoot = GameObject.Find("Root");
                }
                return m_gRoot;
            }
        }

        private static Transform m_transPlayerTop;
        public static Transform M_transPlayerTop
        {
            get
            {
                if (m_transPlayerTop == null)
                {
                    m_transPlayerTop = M_gRoot.transform.Find("Canvas_UITop_0/Panel_PlayerTop");
                }
                return m_transPlayerTop;
            }
        }
    }

    /// <summary>
    /// 声音数据
    /// </summary>
    /// <returns></returns>
    public class AudioData
    {
        //声音名字
        public string[] ary_AudioNames =
        {
            "BGM_Gen","BGM_Free",
            "Sound_Button","Sound_EmitParticle","Sound_FreeEnab","Sound_FreeExit","Sound_Icon_8",
            "Sound_Icon_9","Sound_LoongFlaming","Sound_LoongPenetrate","Sound_Spin","Sound_SpinStop",
            "Sound_TransScore","Sound_WinJackpot",
        };

        //声音音量
        public float[] ary_AudioVolume =
        {
            0.3f,0.3f,
            1f,0.15f,0.6f,0.4f,1f,
            0.5f,1f,1f,0.6f,1f,
            1f,1f,
        };
    }
}



