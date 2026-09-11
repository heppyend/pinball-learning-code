/******************************************************************************
 * 
 *  Title:  弹珠项目
 *
 *  Version:  1.0版
 *
 *  Description: 游戏管理器
 *
 *  Author:  LingBin
 *       
 *  Date:  2026
 * 
 ******************************************************************************/

using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BNGameManager : GameManagerBase
{
    // 创建单例
    public static BNGameManager Instance;

    #region 其它单例
    private BNPlayerController bnPlayerController;
    #endregion

    #region 变量
    [Header("特效存放点")]
    public Transform EffectRoom;

    [Header("空气墙")]
    public GameObject AirWall;

    [Header("背景")]
    public GameObject Background;

    [Header("弹珠存放点")]
    public GameObject MarbleRoom;

    [Header("伤害数字存放点")]
    public Transform DamageNumbersRoom;

    [Header("弹珠指示线与速度比例")]
    public float LineRendererSpeedRatio;

    [Header("减速间隔 为0时是1帧")]
    public float DelayTime;

    [Header("弹珠速度衰减值")]
    public float frictionFactor;

    [Header("弹珠给其它弹珠传递速度比例 被撞弹珠 : 主弹珠")]
    public float MoveSpeedForOtherMarble = 0.2f;

    [Header("弹珠给其它弹珠传递速度乘")]
    public float MoveSpeedForOtherMarbleMultiply = 2f;

    [Header("单回合攻击次数")]
    public int AttacksPerRound;

    [Header("敌方弹珠可攻击次数")]
    public int EnemyMarbleAttackTimes;

    [Header("当前场景")]
    public int CurrentScene = 1;

    [Header("最大场景数量")]
    public int MaxScene = 3;
    
    [Header("蓄力时间")]
    public float ChargeTime = 15;
    public float MaxChargeTime{ get; private set; }
    public bool CanDecreaseChargeTime;
    
    [Header("关卡")]
    public List<Transform> stageTransforms = new List<Transform>();
    // 装饰框
    public GameObject decorateFrame;
    // 迷雾
    public GameObject fog;
    // 当前地图ID
    public int currentMapID;
    
    public bool hasAttackTimesCheck = false;
    
    // 是否正在切换关卡
    private bool isChangingLevel = false;
    // 游戏是否结算
    public bool _isGameSettlement;
    
    
    #endregion

    private void Awake()
    {
        #region 单例初始化
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            GameController.Instance.MainCanvas.transform.position = Vector3.zero;
            bnPlayerController = BNPlayerController.Instance;
            var go = gameObject.AddComponent<BNGlobalManager>();
            go.GameMode = ModeSelectionTool.Instance.mode;
        }
        #endregion

        MessageListener();
        MaxChargeTime = ChargeTime;
    }

    private void MessageListener()
    {
        // 关卡数量
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_CURRENTSTAGECOUNT, SetStageCount);
        // 关卡检测回调
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_LEVELCHECK_CALLBACK, ChangeLevel);
    }

    private void OnDestroy()
    {
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_CURRENTSTAGECOUNT, SetStageCount);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_LEVELCHECK_CALLBACK, ChangeLevel);
    }
    
    public override void StartGame()
    {
        
    }

    public override void EndGame()
    {
        
    }

    private void Update()
    {
        if (CanDecreaseChargeTime && !_isGameSettlement)
        {
            DecreaseChargeTime();
        }
        else
        {
            ChargeTime = MaxChargeTime;
        }
    }
    
    /// <summary>
    /// 倒计时
    /// </summary>
    public void DecreaseChargeTime()
    {
        if (bnPlayerController.isPlayingUltimate) return;
        if (ChargeTime <= 0)
        {
            CanDecreaseChargeTime = false;
            ChargeTime = 0;
            BNPlayerController.Instance.ForceLaunch();
            ChargeTime = 15;
            return;
        }

        int lastTime = (int)Math.Truncate(ChargeTime);
        ChargeTime -= Time.fixedDeltaTime;
        int currentTime = (int)Math.Truncate(ChargeTime);
        if (lastTime > currentTime)
        {
            MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_MODIFYCOUNTDOWN,this,currentTime);
        }
    }

    /// <summary>
    /// 敌方弹珠可攻击次数检测
    /// </summary>
    public void EnemyMarbleAttackTimesCheck()
    {
        if (hasAttackTimesCheck) return;
        hasAttackTimesCheck = true;
        if (bnPlayerController.MarbleMoveType == 2 && EnemyMarbleAttackTimes == 0)
        {
            MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ENEMYCOUNTCHECK, this,1);
        }
    }

    /// <summary>
    /// 切换关卡
    /// </summary>
    /// <param name="msg"></param>
    private void ChangeLevel(Message msg)
    {
        if (isChangingLevel) return;
        StartCoroutine(DelayChangeLevel());
    }
    
    /// <summary>
    /// 延迟切换关卡
    /// </summary>
    private IEnumerator DelayChangeLevel()
    {
        isChangingLevel = true;
        fog.SetActive(false);
        yield return new WaitForSeconds(1f);
        // 检测是否是最后一关
        if (CurrentScene >= MaxScene)
        {
            _isGameSettlement = true;
            MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_GAMESETTLEMENT,this,true);
            // UIManager.Instance.OpenUI(EnumUIType.LoadingUI, true, EnumUIType.LobbyUI, EnumSceneType.MainScene);
            yield break;
        }
        // 加载下一关
        CurrentScene++;
        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ADDSTAGECOUNT,this,CurrentScene);
        // 激活当前关卡
        stageTransforms[CurrentScene - 1].gameObject.SetActive(true);
        // 临时取消空气墙
        AirWall.SetActive(false);
        // 移动背景
        Background.transform.DOMoveY(Background.transform.position.y - 10.8f, 1f);
        // 移动弹珠到出生位置，禁用触发器
        for (int i = 0; i < bnPlayerController.Marbles.Count; i++)
        {
            BNMarbleLogic logic = bnPlayerController.Marbles[i].GetComponent<BNMarbleLogic>();
            logic.MarbleTrigger.enabled = false;
            bnPlayerController.Marbles[i].transform.DOMove(bnPlayerController.MarbleSpawnpositons[i], 1f);
        }
        yield return new WaitForSeconds(1f);
        // 震动相机
        GameController.Instance.BNShakeCamera(1f);
        decorateFrame.transform.DOMoveY(decorateFrame.transform.position.y + 10.8f, 1f);
        fog.transform.DOMoveY(decorateFrame.transform.position.y + 10.8f, 1f);
        // 激活空气墙
        AirWall.SetActive(true);
        // 激活弹珠触发器
        for (int i = 0; i < bnPlayerController.Marbles.Count; i++)
        {
            bnPlayerController.Marbles[i].GetComponent<BNMarbleLogic>().MarbleTrigger.enabled = true;
        }
        yield return new WaitForSeconds(1f);
        fog.SetActive(true);
        // 完成关卡切换
        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ENEMYCOUNTCHECK, this,1);
        bnPlayerController.PlayerMoveNum = bnPlayerController.PlayerMoveNum % 4 + 1;
        isChangingLevel = false;
    }

    /// <summary>
    /// 设置关卡数
    /// </summary>
    /// <param name="msg"></param>
    public void SetStageCount(Message msg)
    {
        MaxScene = (int)msg.Content;
        CurrentScene = 1;
    }
}
