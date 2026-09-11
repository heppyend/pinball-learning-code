/******************************************************************************
 * 
 *  Title:  弹珠项目
 *
 *  Version:  1.0版
 *
 *  Description: 玩家控制器
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
using BNRoom.Static;
using JBPROTO;
using UnityEngine;
using UnityEngine.Serialization;

public class BNPlayerController : MonoBehaviour
{
    // 单例
    public static BNPlayerController Instance;

    #region 其它单例
    private BNLoadOtherPrefab bnLoadOtherPrefab;
    #endregion

    #region 变量
    [Header("弹珠存放点")]
    public Transform MarbleRoom;

    [Header("弹珠实例化坐标")]
    public Vector3[] MarbleSpawnpositons;

    [Header("敌方弹珠实例化坐标")]
    public Vector3[] EnemyMarbleSpawnpositons;

    [Header("玩家弹珠列表")]
    public List<GameObject> Marbles = new List<GameObject>();
    
    [Header("玩家弹珠脚本列表")]
    public List<BNMarbleLogic> marbleLogics = new List<BNMarbleLogic>();

    [Header("当前场景敌方弹珠列表")]
    public List<GameObject> EnemyMarblesInThisWarZone = new List<GameObject>();

    [Header("玩家弹珠旋转角度")]
    public List<Vector3> MarblesRotation = new List<Vector3>() 
    {
        new Vector3(),
        new Vector3(),
        new Vector3(),
        new Vector3(),
    };

    [Header("弹珠指示线长度")]
    public float MarbleLineLength = 0;

    [Header("弹珠指示线调整幅度")]
    private float MarbleLineAdjustRange = 0.08f;

    [Header("玩家弹珠旋转速度")]
    public float MarbleRotationSpeed = 1f;

    [Header("玩家目前生命")]
    public int CurrentHealth;

    [Header("玩家总生命")]
    public int totalHealth;
    
    // 回合数
    private int RoundCount;
    
    // 当前回合击中敌人数
    private int ComboCount;
    
    // 当前回合总伤害
    private int ComboDamage;
    // 经验值
    public int experience;
    // 局内等级
    private int _levelInGame;
    
    // 正在播放绝招
    public bool isPlayingUltimate;

    [Header("当前玩家移动序号")]
    [Tooltip("0.无法移动 1.一号弹珠 2.二号弹珠...")]
    public int PlayerMoveNum;

    [Header("当前敌方移动序号")]
    [Tooltip("0.无法移动 1.一号弹珠 2.二号弹珠...")]
    public int EnemyMoveNum;

    [Header("敌我移动")]
    [Tooltip("1.玩家移动 2.敌人移动")]
    public int MarbleMoveType;
    
    // 弹珠是否初始化完毕
    private bool MarbleIsInitialized = false;

    // 缓存射线检测层级
    private static int BulletRaycastMask;

    #endregion
    
    #region 设置值
    
    private void SetTrun(Message msg)
    {
        if (BNGameManager.Instance._isGameSettlement) return;
        int whoseTurn = (int)msg.Content;
        MarbleMoveType = whoseTurn;
        switch (whoseTurn)
        {
            case 1:
                BNGameManager.Instance.CanDecreaseChargeTime = true;
                RoundCount++;
                MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ADDROUNDCOUNT,this,RoundCount);
                BNGameManager.Instance.hasAttackTimesCheck = false;
                break;
            case 2:
                StartCoroutine(EnemyRound());
                PlayerMoveNum = PlayerMoveNum % 4 + 1;
                SkillCooldownManager.OnTurnEnd();
                BuffManager.ReduceAllBuffs();
                break;
        }
    }
    #endregion
    void Awake()
    {
        #region 单例初始化
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // 添加事件监听
            MessageListener();
            BulletRaycastMask = LayerMask.GetMask("Border", "EnemyMarble", "MarbleTrigger", "Marble");
        }
        #endregion
    }
    
    private void MessageListener()
    {
        // 获取英雄信息
        MessageCenter.Instance.AddListener(MsgType.NET_CONTROL_GET_HERO_INFO, HeroInfo);
        // 敌人数量检测
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_ENEMYCOUNTCHECK, EnemyCountCheck);
        // 获取谁的回合
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_WHOSE_TURN_CALLBACK, SetTrun);
        // 增加敌人在当前场景的数量
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_ADDENEMYCOUNT, AddEnemyCount);
        // 减少敌人在当前场景的数量
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_SUBENEMYCOUNT, RemoveEnemyCount);
        // 设置玩家生命值
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_SETHEALTH, GetMessageHealth);
        // 重置弹珠
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_RESETMARBLE, ResetMarble);
        // 关卡检测
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_LEVELCHECK, LevelCheck);
        // 设置连击数
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_ADDCOMBOCOUNT, SetComboCount);
        // 添加经验
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_ADDEXPERIENCE, AddExperience);

        StartCoroutine(WaitToGetInfo());
    }

    private void OnDestroy()
    {
        // 移除事件监听
        MessageCenter.Instance.RemoveListener(MsgType.NET_CONTROL_GET_HERO_INFO, HeroInfo);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_ENEMYCOUNTCHECK, EnemyCountCheck);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_WHOSE_TURN_CALLBACK, SetTrun);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_ADDENEMYCOUNT, AddEnemyCount);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_SUBENEMYCOUNT, RemoveEnemyCount);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_SETHEALTH, GetMessageHealth);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_RESETMARBLE, ResetMarble);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_LEVELCHECK, LevelCheck);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_ADDCOMBOCOUNT, SetComboCount);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_ADDEXPERIENCE, AddExperience);
    }
    
    IEnumerator WaitToGetInfo()
    {
        yield return null;
        NetController.Instance.SendGetHeroReq(MsgType.NET_CONTROL_GET_HERO_INFO);
    }

    private void Update()
    {
        // 安全检测->弹珠是否初始化完毕 & 弹珠列表内是否存在空对象
        bool safetyDetection = !MarbleIsInitialized || Marbles.Exists(m => !m);
        if (safetyDetection) return;

        MarbleRaycastLayer();
        
        if (isPlayingUltimate) return;
        
        CurrentMarbleRotateControl();

        CurrentMarbleLaunchControl();

        UseMarbleUltimate();
    }
    
    /// <summary>
    /// 使用弹珠绝招
    /// </summary>
    private void UseMarbleUltimate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            marbleLogics[0].TraverseSkill(4,null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            marbleLogics[1].TraverseSkill(4,null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            marbleLogics[2].TraverseSkill(4,null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            marbleLogics[3].TraverseSkill(4,null);
        }
    }
    
    /// <summary>
    /// 获取英雄信息
    /// </summary>
    /// <param name="msg"></param>
    private void HeroInfo(Message msg)
    {
        // 解包
        var rsp = msg.Content as CLPFGetHeroAck;
        // 遍历英雄信息
        for (int i = 0; i < rsp.heros.Length; i++)
        {
            // 创建玩家弹珠
            CreateMarble(i, rsp);
        }
        // 设置弹珠初始化完毕
        MarbleIsInitialized = true;
        MessageCenter.Instance.SendMessage(MsgType.NET_GET_HERO_INFO_CALLBACK, this);
    }
    
    /// <summary>
    /// Ui播放完毕
    /// </summary>
    public void UiPlayFinished()
    {
        // 设置当前玩家移动序号
        PlayerMoveNum = 1;
        // 设置当前为玩家移动
        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ENEMYCOUNTCHECK, this,1);
    }

    /// <summary>
    /// 创建玩家弹珠
    /// </summary>
    public void CreateMarble(int num,CLPFGetHeroAck rsp)
    {
        // 英雄ID
        int heroId = rsp.heros[num].heroId;
        // 路径
        var marblePath = SysDefines.MARBLES + "Marble";
        var bubblePath = SysDefines.MARBLEBUBBLES + "Bubble" +THeroHelper.GetRow(heroId).Quality;
        var marbleRendererPath = SysDefines.MARBLERENDERERS + "MarbleRendererFont" + heroId;
        // 英雄信息
        CLPFHeroInfo heroInfo = rsp.heros[num];
        // 创建玩家弹珠
        GameObject marble = ObjectPoolManager.Instance.Spawn(ResManager.Instance.LoadPrefab(marblePath), MarbleSpawnpositons[num], Quaternion.identity, MarbleRoom);
        GameObject bubble = ObjectPoolManager.Instance.Spawn(ResManager.Instance.LoadPrefab(bubblePath),marble.transform);
        GameObject marbleRendererFont = ObjectPoolManager.Instance.Spawn(ResManager.Instance.LoadPrefab(marbleRendererPath),marble.transform);
        bubble.name = "Bubble";
        marbleRendererFont.name = "MarbleRendererFont";
        bubble.transform.SetSiblingIndex(0);
        marbleRendererFont.transform.SetSiblingIndex(1);
        // 获取弹珠逻辑
        BNMarbleLogic marbleLogic = marble.GetComponent<BNMarbleLogic>();
        // 设置弹珠ID
        marbleLogic.MarbleID = heroId;
        // 设置弹珠编号
        marbleLogic.MarbleNumber = num;
        // 设置弹珠弹射类型
        marbleLogic.catapultType = THeroHelper.GetRow(heroId).CatapultType;
        // 设置弹珠暴击伤害
        marbleLogic.CriticalDamage = heroInfo.crit_hurt;
        // 设置弹珠暴击率
        marbleLogic.CriticalRate = heroInfo.crit_rate;
        // 设置弹珠速度系数
        marbleLogic.startMoveSpeedMultiply = heroInfo.speed;
        // 设置弹珠元素类型
        marbleLogic.ElementType = THeroHelper.GetRow(heroId).Element;
        // 设置弹珠等级
        marbleLogic.level = heroInfo.level;
        // 设置弹珠技能ID
        marbleLogic.SkillID = THeroHelper.GetRow(heroId).Skill;
        // 设置弹珠生命
        marbleLogic.Health = (int)heroInfo.hp;
        // 设置弹珠攻击力
        marbleLogic.startAttackDamage = (int)heroInfo.attack;
        // 设置弹珠防御力
        marbleLogic.Defense = (int)heroInfo.defence;
        // 设置弹珠初始位置
        marbleLogic.startPosition = Vector3.zero;
        // 设置弹珠初始线段位置
        marbleLogic.LineRender.SetPosition(1, Vector3.zero);
        //marbleLogic.marbleSpriteRenderer.sprite = marbleLogic.marblesAssets[i + 1];
        // 加入弹珠列表
        Marbles.Add(marble);
        marbleLogics.Add(marble.GetComponent<BNMarbleLogic>());
        // 设置总生命
        SettotalHealth(marbleLogic.Health,true);
        marbleLogic.RegisterSkill();
    }
    
    /// <summary>
    /// 弹珠射线与层级
    /// </summary>
    void MarbleRaycastLayer()
    {
        if(!BNGameManager.Instance.CanDecreaseChargeTime) return;
        // 遍历弹珠实例化坐标
        for (int i = 0; i < MarbleSpawnpositons.Length; i++)
        {
            // 获取弹珠逻辑
            BNMarbleLogic MarbleLogic = Marbles[i].GetComponent<BNMarbleLogic>();
            // 检测是否是当前玩家移动的弹珠
            if (i != PlayerMoveNum - 1 || isPlayingUltimate)
            {
                // 禁用弹珠射线
                MarbleLogic.LineRender.enabled = false;
                // 设置弹珠层级
                MarbleLogic.marbleLayer = 100;
            }
            else
            {
                if (MarbleLogic.CanMove)
                {
                    // 禁用弹珠射线
                    MarbleLogic.LineRender.enabled = false;
                }
                else if(MarbleMoveType == 1)
                {
                    // 检测弹珠是否在播放Idle动画
                    if (MarbleLogic.Bubble.AnimationState.GetCurrent(0).Animation.Name == "Idle")
                    {
                        // 播放选中动画
                        MarbleLogic.Bubble.AnimationState.SetAnimation(0, "xuanzhong2", false);
                        MarbleLogic.Bubble.AnimationState.AddAnimation(0, "xuanzhong", true, 0);
                        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_SHOWMARBLESELECT,this,PlayerMoveNum);
                    }
                    // 启用弹珠射线
                    MarbleLogic.LineRender.enabled = true;
                    // 设置弹珠层级
                    MarbleLogic.marbleLayer = 101;
                }
            }
        }
    }

    /// <summary>
    /// 当前弹珠旋转控制
    /// </summary>
    void CurrentMarbleRotateControl()
    {
        if(!BNGameManager.Instance.CanDecreaseChargeTime) return;
        // 安全检测->弹珠是否存在 & 弹珠是否可以移动 & 是否是敌人移动
        if (PlayerMoveNum == 0) return;
        var currentMarble = Marbles[PlayerMoveNum - 1];
        var currentMarblelogic = marbleLogics[PlayerMoveNum - 1];
        bool safetyDetection = (currentMarble && currentMarble.GetComponent<BNMarbleLogic>().CanMove) || MarbleMoveType != 1;
        if (safetyDetection) return;
        
        // 获取当前弹珠旋转角度
        Vector3 Rotation = MarblesRotation[PlayerMoveNum - 1];
        // 调整旋转角度
        Rotation.z = Mathf.Repeat(Rotation.z, 360f);
        
        // 获取输入
        float input = 0;
        if (Input.GetKey(KeyCode.A)) input += MarbleRotationSpeed;
        if (Input.GetKey(KeyCode.D)) input -= MarbleRotationSpeed;
        // 调整旋转角度
        Rotation.z += input;
        // 设置旋转角度
        MarblesRotation[PlayerMoveNum - 1] = Rotation;
        currentMarblelogic.MarbleTrigger.transform.rotation = Quaternion.Euler(Rotation);
    }

    /// <summary>
    /// 当前弹珠发射控制
    /// </summary>
    void CurrentMarbleLaunchControl()
    {
        if(!BNGameManager.Instance.CanDecreaseChargeTime) return;
        if (PlayerMoveNum == 0) return;
        var currentMarble = Marbles[PlayerMoveNum - 1];
        var currentMarbleLogic = marbleLogics[PlayerMoveNum - 1];
        // 安全检测->弹珠是否存在 & 是否是敌人移动
        if (!currentMarble || MarbleMoveType != 1) return;
        
        // 安全检测->弹珠是否可以移动
        if (currentMarbleLogic.CanMove) return;

        // 调整指示线长度,数值范围限定在0-2之间
        float maxLength = 8f;
        if (Input.GetKey(KeyCode.J) || Input.GetMouseButton(0))
        {   
            MarbleLineLength = Mathf.Clamp(MarbleLineLength + MarbleLineAdjustRange, 0f, maxLength);
        }
        else if (Input.GetKey(KeyCode.K) || Input.GetMouseButton(1))
        {
            MarbleLineLength = Mathf.Clamp(MarbleLineLength - MarbleLineAdjustRange, 0f, maxLength);
        }

        if (MarbleLineLength > 0f)
        {
            if (currentMarbleLogic.Bubble.AnimationState.GetCurrent(0).Animation.Name != "Attack")
            {
                currentMarbleLogic.Bubble.AnimationState.SetAnimation(0, "Attack", true);
            }
        }
        else
        {
            if (currentMarbleLogic.Bubble.AnimationState.GetCurrent(0).Animation.Name == "Attack")
            {
                currentMarbleLogic.Bubble.AnimationState.SetAnimation(0, "xuanzhong", true);
                MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_SHOWMARBLESELECT,this,PlayerMoveNum);
            }
        }
            
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (MarbleLineLength <= 0f) return;
            // 重置攻击次数
            BNGameManager.Instance.AttacksPerRound = 0;
            // 设置弹珠
            currentMarbleLogic.SetMarble(MarbleLineLength, true);
            // 重置指示线长度
            MarbleLineLength = 0;
            //  重置弹珠状态
            for (int i = 0; i < Marbles.Count; i++)
            {
                if (i == PlayerMoveNum - 1) continue;
                Marbles[i].GetComponent<BNMarbleLogic>().ResetMarble();
            }
            //  重置计时器
            BNGameManager.Instance.CanDecreaseChargeTime = false;
        }
        // 获取弹珠Transform
        var marbleTransform = currentMarbleLogic.MarbleTrigger.transform;
        // 获取弹珠Collider
        var marbleCollider = currentMarbleLogic.MarbleCollider;
        // 获取弹珠Collider的bounds大小
        Vector2 boxSize = marbleCollider.bounds.size * 0.55f;
        // 获取射线检测的层
        int raycastMask = BulletRaycastMask;
        // 调整是否检测第一个碰撞器
        Physics2D.queriesStartInColliders = false;
        // 进行射线检测
        if (currentMarbleLogic.catapultType == 2)
        {
            raycastMask = LayerMask.GetMask("Border");
        }
        RaycastHit2D hit = Physics2D.BoxCast
            (marbleCollider.bounds.center, 
                boxSize, 
                0f, 
                marbleTransform.up, 
                200, 
                raycastMask);
        // 恢复是否检测第一个碰撞器
        Physics2D.queriesStartInColliders = true;
        // 检测是否击中碰撞器
        if (hit.collider)
        {
            // 设置指示线长度
            currentMarbleLogic.LineRender.SetPosition(1, new Vector3(0, hit.distance, 0));

            // 反弹方向
            Vector2 inDir = hit.point - (Vector2)marbleTransform.position;
            Vector2 bounceDir = Vector3.Reflect(inDir, hit.normal).normalized;

            // 从碰撞点发射反弹BoxCast
            Physics2D.queriesStartInColliders = false;
            RaycastHit2D bounceHit = Physics2D.BoxCast(hit.point, boxSize, 0f, bounceDir, 200, raycastMask);
            Physics2D.queriesStartInColliders = true;

            // BounceLine终点：碰撞点 + 反弹方向 × 距离
            if (bounceHit.collider)
            {
                float bounceDistance = bounceHit.distance;
                Vector3 bounceDirLocal = marbleTransform.InverseTransformDirection(bounceDir);
                currentMarbleLogic.BounceLine.SetPosition(1, bounceDirLocal * bounceDistance);
            }
            else
            {
                Vector3 bounceDirLocal = marbleTransform.InverseTransformDirection(bounceDir);
                currentMarbleLogic.BounceLine.SetPosition(1, bounceDirLocal * 200);
            }
        }
    }
    
    /// <summary>
    /// 强制发射
    /// </summary>
    public void ForceLaunch()
    {
        // 重置攻击次数
        BNGameManager.Instance.AttacksPerRound = 0;
        // 强制发射
        Marbles[PlayerMoveNum - 1].GetComponent<BNMarbleLogic>().SetMarble(MarbleLineLength, true);
        //  重置弹珠状态
        for (int i = 0; i < Marbles.Count; i++)
        {
            if (i == PlayerMoveNum - 1) continue;
            Marbles[i].GetComponent<BNMarbleLogic>().ResetMarble();
        }
        // 重置指示线长度
        MarbleLineLength = 0;
        //  重置计时器
        BNGameManager.Instance.CanDecreaseChargeTime = false;
    }

    /// <summary>
    /// 弹珠重置
    /// </summary>
    private void ResetMarble(Message msg)
    {
        for (int i = 0; i < Marbles.Count; i++)
        {
            // 获取弹珠逻辑
            BNMarbleLogic MarbleLogic = marbleLogics[i];
            // 重置弹珠位置
            MarbleLogic.startPosition = Vector3.zero;
            // 重置弹珠旋转
            MarbleLogic.MarbleTrigger.transform.eulerAngles = Vector3.zero;
            // 播放Idle动画
            MarbleLogic.Bubble.AnimationState.SetAnimation(0, "Idle", true);
            MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_CLOSEMARBLESELECT,this,PlayerMoveNum);
        }
    }

    /// <summary>
    /// 获取血量消息
    /// </summary>
    /// <param name="msg"></param>
    private void GetMessageHealth(Message msg)
    {
        int hp = (int)msg.Content;
        SettotalHealth(hp);
    }

    /// <summary>
    /// 设置总生命
    /// </summary>
    private void SettotalHealth(int value = 0, bool isInit = false)
    { 
        if (isInit)
        {
            totalHealth += value;
            CurrentHealth = totalHealth;
        }
        else
        {
            CurrentHealth += value;
            // 检测当前生命是否小于等于0
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                BNGameManager.Instance._isGameSettlement = true;
                // 通知UI
                MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_GAMESETTLEMENT,this,false);
                for (int i = 0; i < marbleLogics.Count; i++)
                {
                    marbleLogics[i].Dead();
                }
            }
        }
        var copy = new Dictionary<string, object>();
        copy.Add("CurrentHealth", CurrentHealth);
        copy.Add("totalHealth", totalHealth);
        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_MODIFYHEALTH,this,null,copy);
    }

    /// <summary>
    /// 敌人回合
    /// </summary>
    private IEnumerator EnemyRound()
    {
        // 敌人回合UI显示
        if(EnemyMarblesInThisWarZone.Count <= 0) yield break;
        BNGameManager.Instance.EnemyMarbleAttackTimes += EnemyMarblesInThisWarZone.Count;
        for (int i = 0;i < EnemyMarblesInThisWarZone.Count; i++)
        {
            BNEnemyMarbleLogic enemyMarbleLogic = EnemyMarblesInThisWarZone[i].GetComponent<BNEnemyMarbleLogic>();
            enemyMarbleLogic._hasUseSkillCount = true;
            int useSkillNum = enemyMarbleLogic.GetUseSkillNum();
            enemyMarbleLogic.Attack(useSkillNum);
            if (useSkillNum == -1)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
    
    /// <summary>
    /// 关卡检测
    /// </summary>
    private void LevelCheck(Message msg)
    {
        if(EnemyMarblesInThisWarZone.Count == 0)
        {
            ResetComboCount();
            MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_LEVELCHECK_CALLBACK,this);
        }
    }
    
    /// <summary>
    /// 敌人数量检测
    /// </summary>
    private void EnemyCountCheck(Message msg)
    {
        int whoseTurn = (int)msg.Content;
        for (int i = 0; i < Marbles.Count; i++)
        {
            BNMarbleLogic marbleLogic = Marbles[i].GetComponent<BNMarbleLogic>();
            if (marbleLogic.CanMove || marbleLogic.CanBeMoveAfterHitByOtherMarble)
            {
                return;
            }
        }
        if(EnemyMarblesInThisWarZone.Count > 0)
        {
            ResetComboCount();
            MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_WHOSE_TURN, this, whoseTurn);
        }
    }
    
    /// <summary>
    /// 增加当前敌人数量
    /// </summary>
    /// <param name="msg"></param>
    private void AddEnemyCount(Message msg)
    {
        GameObject enemyMarble = msg.Content as GameObject;
        EnemyMarblesInThisWarZone.Add(enemyMarble);
    }

    /// <summary>
    /// 减少当前敌人数量
    /// </summary>
    /// <param name="msg"></param>
    private void RemoveEnemyCount(Message msg)
    {
        GameObject enemyMarble = msg.Content as GameObject;
        if (EnemyMarblesInThisWarZone.Contains(enemyMarble))
        {
            EnemyMarblesInThisWarZone.Remove(enemyMarble);
        }
    }

    /// <summary>
    /// 设置连击数
    /// </summary>
    /// <param name="msg"></param>
    private void SetComboCount(Message msg)
    {
        ComboCount++;
        ComboDamage += (int)msg.Content;
        var copy = new Dictionary<string, object>();
        copy.Add("ComboCount", ComboCount);
        copy.Add("ComboDamage", ComboDamage);
        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ADDCOMBOCOUNT_CALLBACK, this, false,copy);
    }
    
    /// <summary>
    /// 添加经验
    /// </summary>
    private void AddExperience(Message msg)
    {
        experience += (int)msg.Content;
        if (experience >= 10)
        {
            _levelInGame++;
            experience -= 10;
        }
        var copy = new Dictionary<string, object>();
        copy.Add("levelInGame", _levelInGame);
        copy.Add("experience", experience);
        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ADDEXPERIENCE_CALLBACK, this, null,copy);
    }
    
    /// <summary>
    ///  重置连击数
    /// </summary>
    private void ResetComboCount()
    {
        ComboCount = 0;
        ComboDamage = 0;
        var copy = new Dictionary<string, object>();
        copy.Add("ComboCount", ComboCount);
        copy.Add("ComboDamage", ComboDamage);
        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ADDCOMBOCOUNT_CALLBACK, this, true,copy);
    }
}
