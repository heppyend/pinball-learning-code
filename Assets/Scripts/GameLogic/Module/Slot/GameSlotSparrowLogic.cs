//#define MyTest       //111内部测试

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

//using QT.NET;
using QT.Module;
using QT.Module.Slot;
using DG.Tweening;
//using QT.Server.Slot;
//using SMAlg;

namespace QT.Stage.GameSlotSparrow25
{
    //游戏状态机
    public enum eGameState
    {
        eGS_None = 0,               //初始状态
        eGS_CanChangeBet,           //可以改变押分状态（普通模式才有这状态）
        //eGS_Demo,                   //demo状态
        eGS_Playing,                //游戏进行中
        //eGS_ReturnCoin,             //退币
        //eGS_Backstage,              //进后台
        //eGS_TransGameMode
    }

    //当前局游戏模式
    public enum eGameTypeCur
    {
        eGTC_General = 0,     //普通模式
        eGTC_FreeGame,        //免费模式
        eGTN_SpecialGame
    }

    //下一局游戏模式
    public enum eGameTypeNext
    {
        eGTN_General = 0,     //普通模式
        eGTN_FreeGame        //免费模式
    }

    //得奖类型
    //public enum ePrizeType
    //{
    //    ePT_None = 0,             //没中奖
    //    ePT_General,              //一般奖
    //    ePT_Small,                //小奖
    //    ePT_Medium,               //中等奖
    //    ePT_Big,                  //大奖
    //    ePT_AllAward,             //全盘奖
    //}

    //开始键状态
    public enum eStartKey
    {
        eSK_None = 0,
        eSK_CanSpin,               //滚动
        eSK_StopAllItem,           //全停
        eSK_ShowLines,             //显示中奖线
        eSK_CanGetScoreFast,       //可以快速获取分数
        eSK_CanGetScoreAll,        //可以获取全部分数
        eSK_ExitRule,              //退出说明
        eSK_WinFSWaitPlay,         //中了免费游戏，等待操作
        eSK_WinSpecialWaitPlay,    //中了特殊游戏，等待操作
    }

    //按钮状态
    public enum eBtnState
    {
        eBS_None = 0,            //初始
        eBS_GenIdle,             //普通模式待机
        eBS_FSIdle,              //免费模式待机
        eBS_Playing,             //游戏中
        eBS_Auto,                //自动玩
        eBS_Manual,              //切换到手动玩
        eBS_ReturnCoin,          //退币
        eBS_Backstage,           //进后台
        eBS_Trans
    }

    //显示模式
    public enum eShowMode
    {
        eSM_None = -1,
        eSM_Score,          //分数模式
        eSM_Money,          //金钱模式

        Max,
    }

    //得奖信息
    //public struct StructPrizeInfos
    //{
    //    public int id;            //元素id
    //    public int line;          //中奖线
    //    public int dir;           //方向，0-左起，1-右起
    //    public int num;           //线上中奖元素个数
    //    public int point;         //中奖倍数
    //    public int[] ary_ilineFrom;    //中奖的物种位置，-1（没中），0，1，2
    //}

    //与协议有关的变量
    //public struct SlotMSGVarStruct
    //{
    //    public bool m_bIsUpScoreX10;           //是否10倍上分

    //    public int m_iBetType;                 //改变押分,0-最大押分，-1-减押分，1-加押分
    //    public int m_iBetPctType;              //改变押分比率,-1-减押分比率，1-加押分比率

    //    public int m_iSoundVolType;            //改变音量，负-减，正-加  he_Add_20180626
    //}

    //玩的模式
    public enum ePlayMode
    {
        ePM_Man = 0,        //手动
        ePM_Auto,           //自动
    }

    public class GameSlotSparrowLogic : MonoBehaviour
    {
        public Sprite[] m_ary_SprItems;
        public GameObject m_gSlotNumPrefab;
        public GameObject m_gNumPrefab;              //数字预设体
        public Sprite[] m_ary_SprNumJackpot;

        public GameObject m_caishenPrefab;

        public DealItemRelevant m_DealItem;                                   //处理与item相关
      
        //=====事件与委托====
        
        public eGameState m_GameState = eGameState.eGS_None;                        //游戏状态机
       // public ePrizeType m_PrizeType = ePrizeType.ePT_None;                        //得奖类型
        public eStartKey m_eStartKey = eStartKey.eSK_None;                          //开始键可以执行事件状态
        //public eShowMode m_eShowMode = eShowMode.eSM_Money;                         //数据显示模式

        //public int m_iSeatID = 0;                             //座位号(在本机台的座位号0,1...,相当于场景id)
        //private int m_iRefundMode = 0;                        //退分模式，0-退币，1-退票
        //private int m_iCoinInRatio = 0;                       //投币(或进钞)比率

        public int m_iWinType = 0;                  //中奖类型，0没中奖，1一般奖，2全盘奖
        public int[,] m_arys_ItemResult = new int[DataManager.m_iItemCol, DataManager.m_iItemRow];   //item结果

        public bool m_bIsHasFSFromInit = false;             //是否在初始化时收到免费信号（玩家上一次是在免费模式中离线）
        //public bool m_bIsBSNeedEnab = false;                //是否需要激活后台
        private bool m_bStartCtrl = false;                  //第一次进入游戏初始化后设置成true
        private bool m_bIsIdle = false;                     //是否在待机状态，回合开始后设置为false，回合结束后设置为true


        //public Transform m_Root;
        
        public int m_iBet = 1;

        public ePlayMode m_ePlayMode = ePlayMode.ePM_Man;     //手动/自动

        Image m_ImageTrans;
        GameObject Image_caishen;

        void Awake()
        {
            this.m_DealItem = new DealItemRelevant(this);
            //Sprite[] m_ary_SprItems = new Sprite[DataManager.m_iItemResCount];              //item资源
            //for (int i = 0; i < m_ary_SprItems.Length; i++)
            //{
            //    // m_ary_SprItems[i] = (Sprite)ResManager.Instance.GetAsset(eRes_Assets.e_Sprite, "Item_0_" + i);

            //    m_ary_SprItems[i] = (Sprite)Resources.Load("Duck/Items/Item_0_" + i, typeof(Sprite));
            //    //Debug.LogError("Items/Item_0_" + +i);
            //}
            //Image[,] m_arys_ImageItems = new Image[DataManager.m_iItemCol, DataManager.m_iItemRow + 2];         //滚动item
            m_ImageTrans = this.transform.Find("Panel_Middle").GetComponent<Image>();
            //Image m_ImageTrans = this.transform.GetComponent<Image>();
            Transform transP = m_ImageTrans.transform.Find("Panel_Trans/Panel_TransItems");
            //for (int i = 0; i < m_arys_ImageItems.GetLength(0); i++)
            //{
            //    Transform trans = transP.Find("Panel_Items_" + i);
            //    for (int j = 0; j < m_arys_ImageItems.GetLength(1); j++)
            //    {
            //        m_arys_ImageItems[i, j] = trans.Find("Image_Item_" + j).GetComponent<Image>();

            //        //if (j > 0 && j < m_arys_ImageItems.GetLength(1) - 1)
            //        //{
            //        //    m_arys_VecItems[i, DataManager.m_iItemRow - j] = m_arys_ImageItems[i, j].transform.position;
            //        //}
            //    }
            //}
            //this.m_DealItem.m_ItemCtrl = new ItemCtrl_CR(m_ary_SprItems, m_arys_ImageItems, this.ShowLottery, this.m_DealItem.AColStopEvn, 2200, true, 40, 0.3f);
            List<Sprite[]> m_ary_SprItems1 = new List<Sprite[]>();              //item资源
            Image[,] m_arys_ImageItems = new Image[DataManager.m_iItemCol, DataManager.m_iItemRow + 2];
            //for (int i = 0; i < 1; i++)
            //{
            //    Sprite[] spArray = new Sprite[DataManager.m_iItemResCount];

            //    for (int j = 0; j < spArray.Length; j++)
            //    {
            //        // m_ary_SprItems[i] = (Sprite)ResManager.Instance.GetAsset(eRes_Assets.e_Sprite, "Item_0_" + i);
            //        if (i >= 9 && i <= 18)
            //            spArray[j] = (Sprite)Resources.Load("Items_3/Item_" + i + "_" + j, typeof(Sprite));
            //        else
            //            spArray[j] = (Sprite)Resources.Load("Items_3/Item_0_" + j, typeof(Sprite));

            //        //Debug.LogError("Items/Item_0_" + +i);
            //    }

            //    m_ary_SprItems.Add(spArray);
            //}
            //for test
            
            m_ary_SprItems1.Add(m_ary_SprItems);
            

            //数据准备
            //Sprite[] m_ary_SprNumJackpot = new Sprite[14];              //彩金数字
            List<KeyValuePair<int, char>> m_list_icPairNumSpe = new List<KeyValuePair<int, char>>();   //图片数字特殊符号与数字资源数组id对应表

            m_list_icPairNumSpe.Add(new KeyValuePair<int, char>(10, '.'));
            m_list_icPairNumSpe.Add(new KeyValuePair<int, char>(11, ','));
            m_list_icPairNumSpe.Add(new KeyValuePair<int, char>(12, '$'));
            m_list_icPairNumSpe.Add(new KeyValuePair<int, char>(15, 'c'));
            m_list_icPairNumSpe.Add(new KeyValuePair<int, char>(16, 'j'));
            //for (int i = 0; i < m_ary_SprNumJackpot.Length; i++)    //12
            //{
            //    m_ary_SprNumJackpot[i] = (Sprite)Resources.Load("JU_Number01/" + i, typeof(Sprite));
            //}

            for (int i = 0; i < m_arys_ImageItems.GetLength(0); i++)
            {
                Transform trans = transP.Find("Panel_Items_" + i);
                
                //Button btn = trans.Find("Button_SingleStop_").GetComponent<Button>();
                //btn.name += i;
                //SetBtnEvent(btn, true, true);
                
                for (int j = 0; j < m_arys_ImageItems.GetLength(1); j++)
                {
                    m_arys_ImageItems[i, j] = trans.Find("Image_Item_" + j).GetComponent<Image>();
                   
                    //if (j > 0 && j < m_arys_ImageItems.GetLength(1) - 1)
                    //{
                    //    m_arys_VecItems[i, DataManager.m_iItemRow - j] = m_arys_ImageItems[i, j].transform.position;
                    //}

                    var Panel_NumPar = m_arys_ImageItems[i, j].transform.Find("Panel_NumPar");
                    
                }
                
                
            }
            
            
            
            this.DataInit();
            this.DataInitOk();
            //for test
            /*
            OnManualSpin();
            //测试数据
            randomNumber[0] = 0;
            randomNumber[1] = 50;
            randomNumber[2] = 80;

            mDeltaGoldArray[0] = 20;
            mDeltaGoldArray[1] = 40;
            mDeltaGoldArray[2] = 60;
            mTotalGold = 25461156;
            //for (int i = 0; i < randomNumber.Length; i++)
            //{
            //    if (score[i] == randomNumber[i]) 
            //        randomNumber[i] = i;
            //}
            //Debug.LogError("GunValue:" + GunValue);
            this.SetResult();
            //*/
        }
        private Action<int ,long> action;
        //动画播完调用
        private void endAnimation(int index,long deltaCurrency)
        {
           // Destroy(gameObject);
            action?.Invoke(index,deltaCurrency);
        }
        public void SetParameters(int[] multiple, long[] deltaGoldArray, long TotalGold, Action<int,long> action)
        {
            this.action = action;
           // Debug.LogError("SetParameters>>>>"+ TotalGold);
            OnManualSpin();
            //测试数据
            //randomNumber[0] = 20;
            //randomNumber[1] = 50;
            //randomNumber[2] = 80;
            for (int i = 0; i < randomNumber.Length; i++)
            {
                randomNumber[i] = multiple[i];
               // Debug.LogError("i>>>>"+i+ ",multiple[i]"+ multiple[i]);
            }
            for (int i = 0; i < deltaGoldArray.Length; i++)
            {
                mDeltaGoldArray[i] = deltaGoldArray[i];
              //  Debug.LogError("i>>>>" + i + ",deltaGoldArray[i]" + deltaGoldArray[i]);
            }
            //GunValue = gunValue;
            mTotalGold = TotalGold;
            //Debug.LogError("GunValue:" + GunValue);
            this.SetResult();

            
        }

        void Update()
        {
            this.UpdateGameSlot();
        }

        /// <summary>
        /// Update()调用
        /// </summary>
        /// <returns></returns>
        public void UpdateGameSlot()
        {
            
            if (m_bStartCtrl /*&& m_DealItem!=null*/)
            {
                this.m_DealItem.UpdateState();
                                
                m_DealItem.DealTransStop();
               
            }
        }

        /// <summary>
        /// 实现开始键事件
        /// </summary>
        /// <returns></returns>
        private void OnKeyStartAction()
        {
            switch (m_eStartKey)
            {
                case eStartKey.eSK_None:
                    break;
                case eStartKey.eSK_CanSpin:
 				    OnManualSpin();
                    break;
                case eStartKey.eSK_StopAllItem:
                    m_DealItem.AllStopItem(); 
                    break;
                case eStartKey.eSK_ShowLines:
                  //  m_DealWinPro.SetIsCanShowScore(true);
                    m_eStartKey = eStartKey.eSK_None;
                    break;
                case eStartKey.eSK_CanGetScoreFast:
                case eStartKey.eSK_CanGetScoreAll:
                   // m_DealWinPro.GetWinFast();
                    break;
                case eStartKey.eSK_ExitRule:
                    //m_DealRule.RetrueButtonCotrol();
                    break;
                case eStartKey.eSK_WinFSWaitPlay:
                   // m_DealWinPro.M_bIsWinFSWaitPlayOver = true;
                    m_eStartKey = eStartKey.eSK_None;
                    break;
            }
           
        }

        /// <summary>
        /// 手动滚动
        /// </summary>
        /// <returns></returns>
        private void OnManualSpin()
        {
            switch (m_GameState)
            {
                case eGameState.eGS_None:
                    if (this.m_ePlayMode != ePlayMode.ePM_Man)
                        return;
                    break;
                case eGameState.eGS_CanChangeBet:
                    break;
                default:
                    return;
            }
            OnSpin();
        }

        /// <summary>
        /// 滚动
        /// </summary>
        /// <returns></returns>
        private void OnSpin()
        {
           // m_DealWinPro.m_ShowWinNumCtrl.m_transP.gameObject.SetActive(false);
            //m_DealWinPro.m_Text_Tips.gameObject.SetActive(true);
            ChangeGameState(eGameState.eGS_Playing);
        }

        /// <summary>
        /// 开奖
        /// </summary>
        /// <returns></returns>
        public void ShowLottery()
        {
           // Debug.LogError("m_GameState:" + m_GameState);
            // m_DealSound.SEStop(m_DealItem.m_SoundSpin, m_iSeatID);
            switch (m_GameState)
            {
                case eGameState.eGS_Playing:
                    m_DealItem.ResetParam();
                    // m_DealWinPro.ShowLottery();
                    ShowLottery1();
                    break;
               // case eGameState.eGS_Demo:
                   // m_DealDemo.OnDemo();
                 //   break;
            }
        }

        /// <summary>
        /// 待机
        /// </summary>
        /// <returns></returns>
        public void StartIdle()
        {
            //if (m_GameState != eGameState.eGS_Demo)
            {
                m_GameState = eGameState.eGS_None;
                ChangeGameState(m_GameState);
            }
            m_bIsIdle = true;
            
            if (this.m_ePlayMode == ePlayMode.ePM_Man)      //手动
            {
                this.m_GameState = eGameState.eGS_CanChangeBet;
            }
        }


        //===========================与协议有关============================
        #region 与协议有关
        

        /// <summary>
        /// 从服务器获取信息,初始化数据
        /// </summary>
        /// <returns></returns>
        public void DataInit()
        {
            ResetParam();

            int[,] ary_ItemResult = new int[,] { { 0 },{ 0 },{ 0 } };
            SetItemResult(ary_ItemResult);            

           // m_GameTypeNext = eGameTypeNext.eGTN_General;
                      
            m_bIsHasFSFromInit = false;
                       
            m_DealItem.SetAllItemResult(m_arys_ItemResult);
            m_DealItem.ShowInitResultSpr();            

        }

        /// <summary>
        /// 参数复位
        /// </summary>
        /// <returns></returns>
        private void ResetParam()
        {
          
        }
        System.Random m_Rand = new System.Random();
        /// <summary>
        /// 从服务器得到item结果后，存储在数组
        /// </summary>
        /// <param name="strItemsReels">item结果字符串</param>
        /// <returns></returns>
        public void SetItemResult(int[,] ary_ItemResult)
        {
            for (int i = 0; i < ary_ItemResult.GetLength(0); i++)
            {
                for (int j = 0; j < ary_ItemResult.GetLength(1); j++)
                {
                    m_arys_ItemResult[i, j] = ary_ItemResult[i, j];                    
                }
            }

        }


        /// <summary>
        /// 初始化数据完成
        /// </summary>
        /// <returns></returns>
        public void DataInitOk()
        {
            //Debug.LogError("===DataInitOk,"+ m_bIsHasFSFromInit);

            if (m_bIsHasFSFromInit)
            {
               // SendCommand(SlotDefine.SLOTCOM_PlayBonus);
                return;  
            }
            //AddAction();
            switch (m_GameState)
            {
                case eGameState.eGS_Playing:
                //case eGameState.eGS_ReturnCoin:
                    break;
                default:
                  //  m_DealWinPro.CheckCapped();
                    break;
            }
            StartIdle();
           // if (m_GameTypeCur != eGameTypeCur.eGTC_FreeGame) m_DealSound.PlayBGM((int)eAudioName.BGM_Gen);
            m_bStartCtrl = true;
           
        }

        /// <summary>
        /// 设置中奖结果
        /// </summary>
        /// <returns></returns>
        public void SetResult()
        {
            
            int[,] ary_ItemResult = new int[,] { { 5 }, { 5 }, { 5 } };
         
            for (int i = 0; i < ary_ItemResult.GetLength(0); i++)
            {
                for (int j = 0; j < ary_ItemResult.GetLength(1); j++)
                {
                    //如果是彩金，传过来的值则为0
                    if (randomNumber[i] == 0)
                    {
                        ary_ItemResult[i, j] = 4;
                    }
                    else
                    {
                        for (int k = 0; k < score.Length; k++)
                        {
                            if (score[k] == randomNumber[i])
                            {
                                ary_ItemResult[i, j] = k;
                                break;
                            }
                        }
                    }
                }
            }

            SetItemResult(ary_ItemResult);
        
           // int m_dWinScoreFSItem = 0;
         
           // m_GameTypeNext = eGameTypeNext.eGTN_General;         

            m_DealItem.m_bIsGetResult = true;
        }

        #endregion 与协议有关



        //===========================游戏状态机============================
        #region 游戏状态机
        /// <summary>
        /// 游戏状态机
        /// </summary>
        /// <returns></returns>
        private void ChangeGameState(eGameState gameState)
        {
            switch (gameState)
            {
                case eGameState.eGS_None:
                    IdleRest();
                    break;
                case eGameState.eGS_Playing:
                    {
                       // ..m_DealWinPro.SetLineActive(false);
                       
                    }
                    m_bIsIdle = false;
                    m_GameState = eGameState.eGS_Playing;
                    SetPlayingData();
                    break;
            }
        }

        /// <summary>
        /// idle状态时数据复位
        /// </summary>
        /// <returns></returns>
        private void IdleRest()
        {
            m_eStartKey = eStartKey.eSK_CanSpin;
        }

        /// <summary>
        /// 设置游戏在玩数据
        /// </summary>
        /// <returns></returns>
        private void SetPlayingData()
        {
           // SetBtnState(eBtnState.eBS_Playing);
           
            m_eStartKey = eStartKey.eSK_StopAllItem;
        }
        #endregion 游戏状态机



        //===========================触摸事件==============================
       
        /// <summary>
        /// 结束
        /// </summary>
        public void Final()
        {
            //RemoveAction();
            m_bStartCtrl = false;
            //m_PanelManage = null;
            //for (int i = 0; i < m_list_BaseDeal.Count; i++)
            //{
            //    m_list_BaseDeal[i].Final();
            //}
            //m_list_BaseDeal = null;
        }

        public void StartInitIdle()
        {
            //if (this.m_GameTypeCur == eGameTypeCur.eGTC_General)
            //    this.SetBtnState(eBtnState.eBS_GenIdle);
            //else
            //    this.SetBtnState(eBtnState.eBS_FSIdle);

            this.m_GameState = eGameState.eGS_None;
            this.ChangeGameState(this.m_GameState);

            this.m_bIsIdle = true;
        }


        //=======================================

        void ShowLottery1()
        {
            //Debug.LogError("调用");
            //NewDamageDisplay blackHoleMove = GetComponent<NewDamageDisplay>();
            //if (blackHoleMove == null)
            //    blackHoleMove = gameObject.AddComponent<NewDamageDisplay>();
            //else blackHoleMove.enabled = true;

            small = 1;
            parentTransform = transform.Find("Panel_Num");
            //textPrefab = ResManager.Instance.LoadPrefab(SysDefines.UICONTROLSPREFAB + "DamageTextPrefab");
            //ImagePrefab = ResManager.Instance.LoadPrefab(SysDefines.UICONTROLSPREFAB + "DamageImagePrefab");
            StartCoroutine(DelayedObjectProcessing());
        }
        public GameObject textPrefab; // 统一的文本预制体
        public GameObject ImagePrefab;//统一的图片预制体
        private Transform parentTransform; // 父物体
        public float animationDuration = 0.25f; // 单个对象的动画持续时间
        public float intervalDuration = 0.5f; // 对象动画开始的时间间隔

        private List<GameObject> objects = new List<GameObject>();
        private Vector3[] positions = {
            new Vector3(-500, -350, 0),
            new Vector3(-250, -350, 0),
            new Vector3(0, -350, 0),
            new Vector3(250, -350, 0),
            new Vector3(500, -350, 0)
        };

        float small;
        public int[] randomNumber = new int[3];
        //public long GunValue = 1;
        long[] mDeltaGoldArray;
        long mTotalGold;

        private void CreateAndAnimateObjects()
        {
            int Num = 0;

            if (transform.name == "slotMachineNew_Other(Clone)"|| transform.name == "slotMachineNew_Other") small = 0.5f;
            GameObject Image_obj = Instantiate(ImagePrefab, parentTransform);
            Image_obj.SetActive(true);
            Image_obj.transform.localScale = Vector3.one * 10 * small * 0.1f;
            Image_obj.transform.localPosition = parentTransform.position + new Vector3(-700, -315, 0) * small;
            for (int i = 0; i < positions.Length; i++)
            {
                GameObject obj = Instantiate(textPrefab, parentTransform);
                Text textComponent = obj.GetComponent<Text>();

                if (i % 2 == 0)
                {
                    // 偶数位置设置为随机数字加亿
                    //int randomNumber = Random.Range(1, 10);
                    //Debug.LogError("GunValue:" + GunValue+",tesNumt=" + Num +","+ randomNumber[Num]);

                    //endAnimation(Num, mDeltaGoldArray[Num]);

                    textComponent.text = mDeltaGoldArray[Num++]  + "亿";
                    obj.transform.localScale = Vector3.one * 10 * small;

                    
                }
                else
                {
                    // 奇数位置设置为加号
                    textComponent.text = "P";
                    obj.transform.localScale = Vector3.zero * small;
                }

                // 设置对象位置
                obj.transform.localPosition = parentTransform.position + positions[i] * small;
                objects.Add(obj);

                // 先将对象设置为不激活状态
                obj.SetActive(false);

                // 在合适的时间激活对象并开始动画
                int index = i; // 避免闭包问题
                DOVirtual.DelayedCall(index * intervalDuration, () =>
                {
                    objects[index].SetActive(true);
                    if (index % 2 == 0)
                    {
                        objects[index].transform.DOScale(Vector3.one * small, animationDuration).SetEase(Ease.OutQuad);
                    }
                    else
                    {
                        objects[index].transform.DOScale(Vector3.one * small, animationDuration).SetEase(Ease.OutQuad);
                    }
                });
            }
        }

        private void ActivateAndResetObjects()
        {
            int Num = 0;
            GameObject Image_obj = Instantiate(ImagePrefab, parentTransform);
            Image_obj.SetActive(true);
            Image_obj.transform.localScale = Vector3.one * 10 * small;
            Image_obj.transform.localPosition = parentTransform.position + new Vector3(-700, -350, 0) * small;
            for (int i = 0; i < objects.Count; i++)
            {

                GameObject obj = objects[i];
                Text textComponent = obj.GetComponent<Text>();

                if (i % 2 == 0)
                {
                    // 偶数位置设置为随机数字加亿
                    //int randomNumber = Random.Range(1, 10);
                    textComponent.text = mDeltaGoldArray[Num++]  + "亿";
                    obj.transform.localScale = Vector3.one * 10 * small;
                }
                else
                {
                    // 奇数位置设置为加号
                    textComponent.text = "P";
                    obj.transform.localScale = Vector3.zero * small;
                }

                // 设置对象位置
                obj.transform.localPosition = parentTransform.position + positions[i] * small;

                // 先将对象设置为不激活状态
                obj.SetActive(false);

                // 在合适的时间激活对象并开始动画
                int index = i; // 避免闭包问题
                DOVirtual.DelayedCall(index * intervalDuration, () =>
                {
                    objects[index].SetActive(true);
                    if (index % 2 == 0)
                    {
                        objects[index].transform.DOScale(Vector3.one * small, animationDuration).SetEase(Ease.OutQuad);
                    }
                    else
                    {
                        objects[index].transform.DOScale(Vector3.one * small, animationDuration).SetEase(Ease.OutQuad);
                    }
                });
            }
        }
        private IEnumerator DelayedObjectProcessing()
        {
            // 延迟指定时间
            //yield return new WaitForSeconds(11.5f);
            yield return new WaitForSeconds(0.5f);

            if (objects.Count == 0)
            {
                // 如果对象列表为空，创建新对象
                CreateAndAnimateObjects();
            }
            else
            {
                parentTransform.gameObject.SetActive(true);
                // 如果对象已经存在，激活并重置它们
                ActivateAndResetObjects();
            }
            yield return new WaitForSeconds(3.5f);

            m_ImageTrans.gameObject.SetActive(false);
            parentTransform.gameObject.SetActive(false);


            if (small == 0.5f)
            {
                // yield return new WaitForSeconds(4.5f);
                endAnimation(0, mDeltaGoldArray[0]);
            }
            else
            {
                if (Image_caishen == null)
                {
                    Image_caishen = Instantiate(m_caishenPrefab, this.transform);
                    Image_caishen.transform.SetParent(transform, false);

                    List<KeyValuePair<int, char>> m_list_icPairNumSpe = new List<KeyValuePair<int, char>>();   //图片数字特殊符号与数字资源数组id对应表

                    m_list_icPairNumSpe.Add(new KeyValuePair<int, char>(10, '.'));
                    m_list_icPairNumSpe.Add(new KeyValuePair<int, char>(11, ','));
                    m_list_icPairNumSpe.Add(new KeyValuePair<int, char>(12, '$'));
                }
                else
                {
                    Image_caishen.SetActive(true);
                }
                endAnimation(0, mDeltaGoldArray[0]);

                yield return new WaitForSeconds(4.5f);
                Image_caishen.SetActive(false);
            }
            
        }
        

        int[] score = new int[] { 20, 30, 80, 100, 200, 50 };
        public int GetFireNumStr(int index)
        {
            int num = 0;
            //if (this.isFireNum(index))
            {
                num = score[index];
            }
            return num;
        }

        private void OnDestroy()
        {
          //  Debug.LogError("结束！");
            this.Final();
        }
    }

}

