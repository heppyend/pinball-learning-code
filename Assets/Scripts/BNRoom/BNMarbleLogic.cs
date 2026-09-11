/******************************************************************************
 * 
 *  Title:  弹珠项目
 *
 *  Version:  1.0 版
 *
 *  Description: 弹珠逻辑
 *
 *  Author:  LingBin
 *       
 *  Date:  2026
 * 
 ******************************************************************************/
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BNRoom.Static;
using LC.Newtonsoft.Json.Linq;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class BNMarbleLogic : MonoBehaviour
{

    #region 其它单例
    public BNGameManager bnGameManager;
    #endregion
    #region 变量
    [Header("游戏模式")]
    public int GameMode;
    
    [Header("特效存放点")]
    public Transform EffectRoom;

    // 受击数字存放点缓存
    private Transform cachedDamageNumbersRoom;

    // LineRenderer速度比例缓存
    private float cachedLineRendererSpeedRatio;

    // 传递给其他弹珠的速度缓存
    private float cachedMoveSpeedForOtherMarble;
    private float cachedMoveSpeedForOtherMarbleMultiply;

    [Header("弹珠ID")]
    public int MarbleID;
    
    [Header("弹珠弹射类型:1、反射型 2、穿透型 3、追踪型")]
    public int catapultType;

    [Header("追踪型参数")]
    [Tooltip("追踪力度，越大弧线越明显")]
    private float homingStrength = 5f;
    [Tooltip("目标切换间隔")]
    private float homingRetargetInterval = 0.1f;
    private float homingRetargetTimer = 0f;
    private Transform homingTarget;
    // 是否已命中敌人
    private bool hasHitEnemy;
    // 是否已撞过队友（撞过队友后只追踪敌人）
    private bool hasHitTeammate;
    // 是否在碰撞体内
    public bool isInCollider;

    [Header("当前对局弹珠编号")]
    public int MarbleNumber;

    [Header("弹珠是哪个玩家的")]
    public int WhoseMarble;

    [Header("弹珠显示层")]
    public int marbleLayer;

    // 生命
    public int Health = 100;
    // 攻击力
    private int _attackDamage;
    // 攻击力增幅以及减益
    public int attackBoost;
    // 初始攻击力
    public int startAttackDamage;
    // 防御
    public int Defense = 10;
    // 元素类型
    public int ElementType;
    // 暴击伤害
    public int CriticalDamage;
    // 暴击率
    public int CriticalRate;
    // 技能ID
    public JArray SkillID;
    // 类型系数
    private float _attackerTypeFactor;
    // 能量
    private int _energy;
    // 速度系数
    private int _moveSpeedMultiply;
    // 速度增幅以及减益
    private int _moveSpeedBoost;
    // 初始速度系数
    public int startMoveSpeedMultiply;
    // 等级
    public int level;
    // 技能CD
    private Dictionary<int,Dictionary<int, int>> _skillCd = new Dictionary<int,Dictionary<int, int>>();
    // 技能单回合触发次数上限
    private Dictionary<int,Dictionary<int, int>> _skillTriggerCount = new Dictionary<int,Dictionary<int, int>>();
    [Header("弹珠移动速度")]
    public float MoveSpeed;
    
    // 弹珠开始发射速度
    private float _startMoveSpeed;

    [Header("弹珠被撞速度")]
    private float MoveSpeedAfterHitByOtherMarble;

    [Header("弹珠是否可以移动")]
    public bool CanMove;

    [Header("弹珠被撞后是否可以移动")]
    public bool CanBeMoveAfterHitByOtherMarble;

    [Header("弹珠是否已经死亡")]
    public bool HasDead;

    [Header("弹珠触碰到其它弹珠")]
    private bool wasTouchOtherMarble;

    [Header("弹珠移动方向")]
    private Vector2 moveDirection;

    [Header("弹珠初始位置")]
    public Vector3 startPosition;

    [Header("DoTween")]
    private Tween MoveTween;
    private Tween MoveAfterHitTween;
    private Tween MoveSlowTween;

    float DelayTime;
    float frictionFactor;
    #endregion

    #region 组件
    
    // 弹珠渲染器父物体
    private Transform marbleRendererFont;
    public Transform MarbleRendererFont
    {
        get
        {
            if (ReferenceEquals(marbleRendererFont, null))
            {
                marbleRendererFont = UnityHelper.FindTheChild(gameObject, "MarbleRendererFont").GetComponent<Transform>();
            }
            return marbleRendererFont;
        }
    }

    // 弹珠渲染器
    private MeshRenderer marbleRenderer;
    public MeshRenderer MarbleRenderer
    {
        get
        {
            if (ReferenceEquals(marbleRenderer, null))
            {
                marbleRenderer = UnityHelper.FindTheChild(MarbleRendererFont.gameObject, "MarbleRenderer").GetComponent<MeshRenderer>();
            }
            return marbleRenderer;
        }
    }
    
    // 弹珠Spine
    private SkeletonAnimation marbleSkeletonAnimation;
    public SkeletonAnimation MarbleSkeletonAnimation
    {
        get
        {
            if (ReferenceEquals(marbleSkeletonAnimation, null))
            {
                marbleSkeletonAnimation = UnityHelper.FindTheChild(MarbleRendererFont.gameObject, "MarbleRenderer").GetComponent<SkeletonAnimation>();
            }
            return marbleSkeletonAnimation;
        }
    }
    
    // 弹珠移动指示线
    private LineRenderer lineRender;
    public LineRenderer LineRender
    {
        get
        {
            if (ReferenceEquals(lineRender, null))
            {
                lineRender = UnityHelper.FindTheChild(MarbleTrigger.gameObject, "LineRender").GetComponent<LineRenderer>();
            }
            return lineRender;
        }
    }
    
    // 弹珠反弹指示线
    private LineRenderer bounceLine;
    public LineRenderer BounceLine
    {
        get
        {
            if (ReferenceEquals(bounceLine, null))
            {
                bounceLine = UnityHelper.FindTheChild(LineRender.gameObject, "Target/BounceLine").GetComponent<LineRenderer>();
            }
            return bounceLine;
        }
    }

    // 弹珠刚体
    private Rigidbody2D Rb;
    public Rigidbody2D rb
    {
        get
        {
            if (ReferenceEquals(Rb, null))
            {
                Rb = GetComponent<Rigidbody2D>();
            }
            return Rb;
        }
    }
    
    // 弹珠碰撞体
    private Collider2D marbleCollider;
    public Collider2D MarbleCollider
    {
        get
        {
            if (ReferenceEquals(marbleCollider, null))
            {
                marbleCollider = GetComponent<Collider2D>();
            }
            return marbleCollider;
        }
    }
    
    // 弹珠触发器
    private Collider2D marbleTrigger;
    public Collider2D MarbleTrigger
    {
        get
        {
            if (ReferenceEquals(marbleTrigger, null))
            {
                marbleTrigger = UnityHelper.FindTheChild(gameObject, "MarbleTrigger").GetComponent<Collider2D>();
            }
            return marbleTrigger;
        }
    }
    
    // 泡泡Spine
    private SkeletonAnimation bubble;
    public SkeletonAnimation Bubble
    {
        get
        {
            if (ReferenceEquals(bubble, null))
            {
                bubble = UnityHelper.FindTheChild(gameObject, "Bubble").GetComponent<SkeletonAnimation>();
            }
            return bubble;
        }
    }
    
    #endregion
    #region 协程
    [Header("减速协程")]
    public Coroutine attenuationSpeed;
    [Header("异常停止检查协程")]
    public Coroutine exceptionStopCheck;
    #endregion
    
    void Awake()
    {
        // 关卡检测回调
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_LEVELCHECK_CALLBACK, StopMarble);
        // 添加弹珠能量
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_ADDMARBLEENERGY, AddEnergy);
        // 回合结束修改技能CD以及技能持续时间
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_MODIFYSKILLCD , ResetSkillCdAndTriggerCount);
    }
    
    void OnDestroy()
    {
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_LEVELCHECK_CALLBACK, StopMarble);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_ADDMARBLEENERGY, AddEnergy);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_MODIFYSKILLCD , ResetSkillCdAndTriggerCount);
    }

    void Start()
    {
        // 获取游戏模式
        GameMode = BNGlobalManager.Instance.GameMode;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        // 获取单例
        GetSingleton();
        if (Bubble && MarbleRenderer)
        {
            // 设置泡泡排序层
            Bubble.transform.GetComponent<MeshRenderer>().sortingOrder = marbleLayer + 2;
            // 设置弹珠渲染器子物体排序层
            MarbleRenderer.sortingOrder = marbleLayer + 1;
        }
        // 移动弹珠
        MoveMarble();
        // 设置弹珠反弹指示线是否启用
        BounceLine.enabled = LineRender.enabled;
        ProcessEffectBoost();
    }
    
    /// <summary>
    /// 处理效果增幅以及减益
    /// </summary>
    private void ProcessEffectBoost()
    {
        var effectTypesAndValues = BuffManager.GetBuffInfo(gameObject);
        int finalValue = 0;
        
        foreach (var effectTypeAndValue in effectTypesAndValues)
        {
            switch (effectTypeAndValue.type)
            {
                case EnumEffectValueType.IncreaseAttack:
                    finalValue += (int)effectTypeAndValue.parameter;
                    attackBoost = finalValue;
                    break;
                case EnumEffectValueType.IncreaseSpeedByPercentage:
                    finalValue += (int)(startMoveSpeedMultiply * (effectTypeAndValue.parameter / 100));
                    _moveSpeedBoost = finalValue;
                    break;
            }
            finalValue = 0;
        }
        
        _attackDamage = startAttackDamage + attackBoost;
        _moveSpeedMultiply = startMoveSpeedMultiply + _moveSpeedBoost;
    }

    /// <summary>
    /// 获取单例
    /// </summary>
    void GetSingleton()
    {
        if (GameMode == 1)
        {
            if (bnGameManager == null)
            {
                bnGameManager = BNGameManager.Instance;
                EffectRoom = bnGameManager.EffectRoom;
                DelayTime = bnGameManager.DelayTime;
                frictionFactor = bnGameManager.frictionFactor;
                cachedDamageNumbersRoom = bnGameManager.DamageNumbersRoom;
                cachedLineRendererSpeedRatio = bnGameManager.LineRendererSpeedRatio;
                cachedMoveSpeedForOtherMarble = bnGameManager.MoveSpeedForOtherMarble;
                cachedMoveSpeedForOtherMarbleMultiply = bnGameManager.MoveSpeedForOtherMarbleMultiply;
            }
        }
    }

    /// <summary>
    /// 弹珠受击
    /// </summary>
    public void GetHit(int attackerATK,float attackerTypeFactor,int attackerElement,int attackerCritDamage,int attackerCritRate)
    {
        // 播放受击动画
        SkeletonAnimSwitch("Hit");
        // 获取受击数字存放点
        Transform DamageNumbersRoom = cachedDamageNumbersRoom != null ? cachedDamageNumbersRoom : transform;
        // 创建受击数字
        GameObject DamageNumber = ObjectPoolManager.Instance.Spawn(
            ResManager.Instance.LoadPrefab(SysDefines.MARBLEUI + "DamageNumbers"),
            transform.position,
            Quaternion.identity,
            DamageNumbersRoom
            );
        // 启动受击数字动画
        DamageNumber.transform.GetComponent<BNDamageNumberLogic>().MoveStart(transform.position, 1);
        // 缓存受击数字文本组件
        var damageText = DamageNumber.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        // 计算最终伤害
        int finalDamage = DamageCalculator.CalculateDamage(
            attackerATK,
            attackerTypeFactor,
            attackerElement,
            ElementType,
            Defense,
            attackerCritDamage,
            attackerCritRate,
            (float)TCoefficientHelper.GetRow(1).BaseRestrain,
            (float)TCoefficientHelper.GetRow(1).BaseBeRestrain,
            (float)TCoefficientHelper.GetRow(1).SpecialRestrain,
            (float)TCoefficientHelper.GetRow(1).SpecialBaseRestrain,
            (float)TCoefficientHelper.GetRow(1).BaseSpecialRestrain,
            1
            );
        // 根据伤害值设置受击数字文本
        if (finalDamage > 0)
        {
            // 根据游戏模式扣除对应玩家的生命值
            if (GameMode == 1)
            {
                MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_SETHEALTH,this,-finalDamage);
            }
            // 设置受击数字文本
            damageText.text = "-" + finalDamage;
        }
        else
        {
            // 设置受击数字文本为0
            damageText.text = "-0";
        }
    }

    /// <summary>
    /// 切换Spine动画
    /// </summary>
    /// <param name="animName">动画名称</param>
    /// <param name="isLoop">是否循环</param>
    public void SkeletonAnimSwitch(string animName, bool isLoop = false,bool isAddIdle = true)
    {
        // 设置动画
        MarbleSkeletonAnimation.AnimationState.SetAnimation(0, animName, isLoop);
        // 添加Idle动画
        if (isAddIdle) MarbleSkeletonAnimation.AnimationState.AddAnimation(0, "Idle", true,0f);
    }

    /// <summary>
    /// 弹珠死亡
    /// </summary>
    public void Dead()
    {
        // 如果弹珠已经死亡则返回
        if (HasDead) return;
        // 设置弹珠死亡标志
        HasDead = true;
        // 检测弹珠是否被激活
        if (gameObject.activeSelf)
        {
            // 禁用弹珠碰撞体和触发器
            MarbleCollider.enabled = false;
            MarbleTrigger.enabled = false;
            
            SkeletonAnimSwitch("Die",false,false);
        }
    }

    /// <summary>
    /// 移动弹珠
    /// </summary>
    void MoveMarble()
    {
        // 如果弹珠可以移动
        if (CanMove)
        {
            // 如果弹珠移动速度大于0
            if (MoveSpeed > 0)
            {
                // 追踪型：自动微调朝向敌方
                if (catapultType == 3)
                {
                    // 已命中敌人则停止追踪
                    if (!hasHitEnemy)
                    {
                        // 定时重新锁定目标（0.1秒）
                        homingRetargetTimer -= Time.fixedDeltaTime;
                        if (homingRetargetTimer <= 0f)
                        {
                            homingRetargetTimer = homingRetargetInterval;
                            UpdateHomingTarget();
                        }
                        // 有目标则微调朝向，无目标沿原方向直线飞行
                        if (homingTarget != null)
                        {
                            Vector2 toTarget = ((Vector2)homingTarget.position - rb.position).normalized;
                            moveDirection = Vector2.Lerp(moveDirection, toTarget, homingStrength * Time.fixedDeltaTime).normalized;
                        }
                    }
                }
                // 移动弹珠
                rb.MovePosition(rb.position + moveDirection * (MoveSpeed * Time.fixedDeltaTime));
            }
            else
            {
                // 停止弹珠移动
                CanMove = false;
                // 重置速度
                ModifyMoveSpeed(0, EnumCalculateType.Equals);

                // 穿透型：停止时检测重叠并推开
                if (catapultType == 2 || catapultType == 3)
                {
                    // ResolveOverlap();
                }

                if (GameMode == 1)
                {
                    MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ENEMYCOUNTCHECK, this,2);
                    MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_RESETMARBLE,this);
                }
            }
        }
        else if (CanBeMoveAfterHitByOtherMarble)
        {
            // 如果弹珠被其他弹珠撞到后移动速度大于0
            if (MoveSpeedAfterHitByOtherMarble > 0)
            {
                // 移动弹珠
                rb.MovePosition(rb.position + moveDirection * (MoveSpeedAfterHitByOtherMarble * Time.fixedDeltaTime));
            }
            else
            {
                // 停止弹珠移动
                CanBeMoveAfterHitByOtherMarble = false;
                // 重置速度
                MoveSpeedAfterHitByOtherMarble = 0;
                // 停止弹珠移动
                rb.velocity = Vector2.zero;
                MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ENEMYCOUNTCHECK, this,2);
            }
        }
    }
    
    /// <summary>
    /// 修改移动速度
    /// </summary>
    /// <param name="value">需要修改多少</param>
    /// <param name="calculateType">操作类型</param>
    private void ModifyMoveSpeed(float value,EnumCalculateType calculateType)
    {
        switch (calculateType)
        {
            case EnumCalculateType.Add:
                MoveSpeed += value;
                break;
            case EnumCalculateType.Subtract:
                MoveSpeed -= value;
                break;
            case EnumCalculateType.Multiply:
                MoveSpeed *= value;
                break;
            case EnumCalculateType.Divide:
                MoveSpeed /= value;
                break;
            case EnumCalculateType.Equals:
                MoveSpeed = value;
                _startMoveSpeed = MoveSpeed;
                break;
        }
    }

    /// <summary>
    /// 更新追踪目标
    /// </summary>
    private void UpdateHomingTarget()
    {
        // 当前目标仍存活则不切换
        if (homingTarget != null && homingTarget.gameObject.activeSelf) return;

        float closestEnemyDist = float.MaxValue;
        Transform closestEnemy = null;
        float closestTeammateDist = float.MaxValue;
        Transform closestTeammate = null;

        // 找最近的敌人
        List<GameObject> enemies = BNPlayerController.Instance.EnemyMarblesInThisWarZone;
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null || !enemies[i].activeSelf) continue;
            float dist = Vector2.Distance(rb.position, enemies[i].transform.position);
            if (dist < closestEnemyDist)
            {
                closestEnemyDist = dist;
                closestEnemy = enemies[i].transform;
            }
        }

        // 未撞过队友时，找最近的队友
        if (!hasHitTeammate)
        {
            for (int i = 0; i < BNPlayerController.Instance.Marbles.Count; i++)
            {
                if (i == MarbleNumber) continue;
                if (BNPlayerController.Instance.Marbles[i] == null || !BNPlayerController.Instance.Marbles[i].activeSelf) continue;
                float dist = Vector2.Distance(rb.position, BNPlayerController.Instance.Marbles[i].transform.position);
                if (dist < closestTeammateDist)
                {
                    closestTeammateDist = dist;
                    closestTeammate = BNPlayerController.Instance.Marbles[i].transform;
                }
            }
        }

        // 队友比敌人近 → 追踪队友，否则追踪敌人
        if (closestTeammate != null && closestTeammateDist < closestEnemyDist)
            homingTarget = closestTeammate;
        else
            homingTarget = closestEnemy;
    }

    /// <summary>
    /// 检测重叠并推开（贯穿型停止时调用）
    /// </summary>
    private void ResolveOverlap()
    {
        Collider2D col = MarbleCollider;
        if (col == null) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        List<Collider2D> results = new List<Collider2D>();
        Physics2D.OverlapCollider(col, filter, results);

        if (results.Count == 0) return;

        Vector2 totalPush = Vector2.zero;
        foreach (var other in results)
        {
            if (other == col) continue;
            Vector2 dir = (other.transform.position - transform.position).normalized;
            float overlap = col.Distance(other).distance;
            if (overlap < 0)
            {
                totalPush -= dir * Mathf.Abs(overlap);
            }
        }

        if (totalPush != Vector2.zero)
        {
            // 加一点随机偏移让推开方向更自然
            float randomAngle = Random.Range(-15f, 15f);
            Vector2 pushDir = Quaternion.Euler(0, 0, randomAngle) * totalPush.normalized;
            float pushDist = totalPush.magnitude + 0.1f;

            Vector3 targetPos = transform.position + (Vector3)(pushDir * pushDist);
            transform.DOMove(targetPos, 0.15f).SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// 等待延迟时间
    /// </summary>
    private IEnumerator WaitDelayTime()
    {
        if (DelayTime == 0)
            yield return null;
        else if (DelayTime > 0)
            yield return new WaitForSeconds(DelayTime);
        else
            yield break;
    }

    /// <summary>
    /// 弹珠发射后开始衰减移动速度
    /// </summary>
    public IEnumerator AttenuationSpeed()
    {
        if (CanMove)
        {
            yield return WaitDelayTime();
            // 减少移动速度
            ModifyMoveSpeed(frictionFactor, EnumCalculateType.Subtract);
            if (MoveSpeed <= 0) yield break;
        }
        if (CanBeMoveAfterHitByOtherMarble)
        {
            yield return WaitDelayTime();
            // 减少移动速度
            MoveSpeedAfterHitByOtherMarble -= frictionFactor;
            if (MoveSpeedAfterHitByOtherMarble <= 0) yield break;
        }
        attenuationSpeed = StartCoroutine(AttenuationSpeed());
    }
    
    /// <summary>
    /// 弹珠异常停止检查
    /// </summary>
    private IEnumerator MarbleExceptionStopCheck()
    {
        if (CanMove)
        {
            Vector3 pos = transform.position;
            Vector2 dir = moveDirection;
            yield return new WaitForSeconds(2f);
            if (transform.position == pos && moveDirection == dir)
            {
                moveDirection = -moveDirection;
            }
            exceptionStopCheck = StartCoroutine(MarbleExceptionStopCheck());
        }
    }

    /// <summary>
    /// 设置弹珠
    /// </summary>
    public void SetMarble(float speed, bool canMove)
    {
        if (CanMove) return;
        // 设置弹珠是否可以移动
        rb.bodyType = RigidbodyType2D.Dynamic;
        // 设置弹珠移动速度
        MoveSpeedAfterHitByOtherMarble = 0;
        ModifyMoveSpeed(speed * cachedLineRendererSpeedRatio * _moveSpeedMultiply, EnumCalculateType.Equals);
        // 设置弹珠是否可以移动
        CanMove = canMove;
        // 启动弹珠速度衰减协程
        attenuationSpeed = StartCoroutine(AttenuationSpeed());
        exceptionStopCheck = StartCoroutine(MarbleExceptionStopCheck());
        // 禁用LineRenderer
        LineRender.enabled = false;
        // 设置弹珠移动方向
        moveDirection = MarbleTrigger.transform.up;
        // 启用弹珠碰撞体
        MarbleCollider.enabled = true;
        // 禁用弹珠触发器
        MarbleTrigger.enabled = false;
        // 切换泡泡动画
        Bubble.AnimationState.SetAnimation(0, "xuanzhong", true);
    }

    /// <summary>
    /// 停止弹珠
    /// </summary>
    /// <param name="msg"></param>
    private void StopMarble(Message msg)
    {
        ModifyMoveSpeed(0f, EnumCalculateType.Equals);
        MoveSpeedAfterHitByOtherMarble = 0f;
    }

    /// <summary>
    /// 被其他弹珠撞后设置弹珠
    /// </summary>
    public void SetMarbleAfterHitByOtherMarble(float speed, Vector3 direction)
    {
        // 设置弹珠被其他弹珠撞到后是否可以移动
        CanBeMoveAfterHitByOtherMarble = true;
        // 设置弹珠被其他弹珠撞到后的速度与
        MoveSpeedAfterHitByOtherMarble = speed;
        // 设置弹珠被其他弹珠撞到后的方向
        moveDirection = direction;
        // 弹珠发射后开始衰减移动速度
        attenuationSpeed = StartCoroutine(AttenuationSpeed());
    }

    /// <summary>
    /// 弹珠碰撞
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 检测碰撞对象是否为墙壁
        if (!collision.gameObject.CompareTag("Border")) return;
        // 检测速度是否大于0
        if (MoveSpeed <= 0 && MoveSpeedAfterHitByOtherMarble <= 0) return;
        // 震动相机
        GameController.Instance.BNShakeCamera();
        // 检测是否同时碰到两面墙
        if (collision.contacts.Length >= 2)
        {
            // 减速
            ModifyMoveSpeed(frictionFactor * 4, EnumCalculateType.Subtract);
            // 处理角落反弹
            HandleCornerBounce(collision.contacts);
        }
        else
        {
            // 减速
            ModifyMoveSpeed(frictionFactor * 2, EnumCalculateType.Subtract);
            // 反弹方向 
            moveDirection = Vector3.Reflect(moveDirection.normalized, collision.contacts[0].normal).normalized;
        }
    }

    /// <summary>
    /// 处理角落反弹（两面墙）
    /// </summary>
    private void HandleCornerBounce(ContactPoint2D[] contacts)
    {
        // 获取两个法线
        Vector2 normal1 = contacts[0].normal;
        Vector2 normal2 = contacts[1].normal;
        // 检查两个法线是否不同
        if (Vector2.Dot(normal1, normal2) > 0.9f)
        {
            // 两个法线相同，正常反弹
            moveDirection = Vector3.Reflect(moveDirection.normalized, normal1).normalized;
            return;
        }
        // 计算合成法线
        Vector2 combinedNormal = (normal1 + normal2).normalized;
        // 计算反弹方向
        moveDirection = Vector3.Reflect(moveDirection.normalized, combinedNormal).normalized;
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("MarbleTrigger") || other.CompareTag("EnemyMarble"))
        {
            isInCollider = false;
        }
    }

    /// <summary>
    /// 弹珠触发器碰撞
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MarbleTrigger") || collision.CompareTag("EnemyMarble"))
        {
            isInCollider = true;
        }
        // 检测碰撞对象是否为弹珠触发器
        if (collision.CompareTag("MarbleTrigger"))
        {
            // 公共：获取对方弹珠、碰撞方向、创建特效
            BNMarbleLogic other = collision.transform.parent.GetComponent<BNMarbleLogic>();
            // 获取碰撞方向
            Vector2 dirNorm = (collision.transform.position - transform.position).normalized;
            // 遍历技能
            if (CanMove)
            {
                TraverseSkill(2, other.gameObject);
                other.TraverseSkill(3, gameObject);
            }

            // 穿透型：不反弹，只触发脚本
            if (catapultType == 2 && MoveSpeed > _startMoveSpeed * 0.1f && CanMove)
            {
                if (GameMode == 1)
                {
                    if (CanMove)
                    {
                        ModifyMoveSpeed(MoveSpeed * cachedMoveSpeedForOtherMarble, EnumCalculateType.Subtract);
                    }
                    else
                    {
                        MoveSpeedAfterHitByOtherMarble -= MoveSpeedAfterHitByOtherMarble * cachedMoveSpeedForOtherMarble;
                    }
                }
                return;
            }

            // 反弹方向（反射型和追踪型）
            moveDirection = -dirNorm;

            // 追踪型
            if (catapultType == 3 && CanMove)
            {
                // 场景无怪物：碰到任何弹珠都停下
                if (BNPlayerController.Instance.EnemyMarblesInThisWarZone.Count == 0)
                {
                    hasHitEnemy = true;
                    ModifyMoveSpeed(0,EnumCalculateType.Equals);
                    CanMove = false;
                    return;
                }
                return;
            }

            if (GameMode == 1)
            {
                if (CanMove)
                {
                    ModifyMoveSpeed(MoveSpeed * cachedMoveSpeedForOtherMarble, EnumCalculateType.Subtract);
                    if(other.isInCollider) return;
                    other.SetMarbleAfterHitByOtherMarble(MoveSpeed * cachedMoveSpeedForOtherMarble * cachedMoveSpeedForOtherMarbleMultiply, dirNorm);
                }
                else
                {
                    MoveSpeedAfterHitByOtherMarble -= MoveSpeedAfterHitByOtherMarble * cachedMoveSpeedForOtherMarble;
                    if(other.isInCollider) return;
                    other.SetMarbleAfterHitByOtherMarble(MoveSpeedAfterHitByOtherMarble * cachedMoveSpeedForOtherMarble * 4, dirNorm);
                }
            }
        }
        else if (collision.CompareTag("EnemyMarble"))
        {
            if (GameMode != 1) return;
            bnGameManager.AttacksPerRound += 1;
            // 创建击中特效
            ObjectPoolManager.Instance.Spawn(
                ResManager.Instance.LoadPrefab(SysDefines.MARBLEEFFECTS + "MarbleHit"),
                collision.transform.position,
                Quaternion.identity,
                EffectRoom
                );
            // 遍历技能
            TraverseSkill(1, collision.gameObject);

            if (catapultType == 3 && CanMove)
            {
                // 追踪型：命中敌人后停止
                hasHitEnemy = true;
                ModifyMoveSpeed(0,EnumCalculateType.Equals);
                MoveSpeedAfterHitByOtherMarble = 0;
                return;
            }

            // 穿透型：不反弹
            if (catapultType == 2 && MoveSpeed > _startMoveSpeed * 0.1f && CanMove)
            {
                if (CanMove) ModifyMoveSpeed(MoveSpeed * cachedMoveSpeedForOtherMarble, EnumCalculateType.Subtract);
                return;
            }
            
            moveDirection = -moveDirection;
            if (CanMove) ModifyMoveSpeed(MoveSpeed * cachedMoveSpeedForOtherMarble, EnumCalculateType.Subtract);
        }
    }

    /// <summary>
    /// 重置弹珠
    /// </summary>
    public void ResetMarble()
    {
        // 重置弹珠旋转
        MarbleTrigger.transform.eulerAngles = Vector3.zero;
        BNPlayerController.Instance.MarblesRotation[MarbleNumber] = Vector3.zero;
        // 设置弹珠被撞后是否可以移动
        CanBeMoveAfterHitByOtherMarble = true;
        // 设置弹珠的触发器
        MarbleTrigger.enabled = true;
        // 重置追踪状态
        homingTarget = null;
        hasHitEnemy = false;
        hasHitTeammate = false;
        homingRetargetTimer = 0f;
    }

    /// <summary>
    /// 注册技能
    /// </summary>
    public void RegisterSkill()
    {
        foreach (var skillID in SkillID)
        {
            int skillIDInt = (int)skillID;
            var skillDataList = GetSkillEffectParameters.GetSkillDataList(skillIDInt,level);
            for (int i = 0; i < skillDataList.Count; i++)
            {
                var skillData = skillDataList[i];
                if (skillData != null)
                {
                    SkillCooldownManager.RegisterSkillEffect(gameObject, skillIDInt, i,
                        skillData.EffectInterval, skillData.EffectTriggerCount);
                }
            }
        }
    }
    
    /// <summary>
    /// 遍历技能表中技能
    /// </summary>
    public void TraverseSkill(int skillType,GameObject attackerOrCollideder)
    {
        for (int i = 0; i < SkillID.Count; i++)
        {
            int skillID = (int)SkillID[i];
            int type = TSkillHelper.GetRow(skillID).Type;
            // 最大能量
            int maxEnergy = TEnergyHelper.GetRow(1).HeroLimit;
            if (type == skillType)
            {
                // 绝招检测能量
                // if (skillType == 4 && _energy < maxEnergy) return;
                // 使用技能
                UseSkill(skillID,attackerOrCollideder);
            }
        }
    }

    /// <summary>
    /// 使用技能
    /// </summary>
    private void UseSkill(int skillID,GameObject attackerOrCollideder)
    {
        // 获取攻击系数
        JArray typeFactor = TCoefficientHelper.GetRow(1).AttackCoefficient;
        _attackerTypeFactor = (float)typeFactor[catapultType - 1];
        SkillCaster.CastSkill(
            skillID,
            level,
            _attackDamage,
            ElementType,
            CriticalDamage,
            CriticalRate,
            _attackerTypeFactor,
            gameObject,
            attackerOrCollideder,
            MarbleNumber
            );
        
        
        switch (skillID)
        {
            case 100204:
                BNPlayerController.Instance.isPlayingUltimate = true;
                ObjectPoolManager.Instance.Spawn(
                    ResManager.Instance.LoadPrefab(SysDefines.SPINES + "HeroUltimates/" + "Ultimate" + MarbleID), 
                    Vector3.zero,
                    Quaternion.identity,
                    EffectRoom
                );
                break;
        }
    }
    
    
    /// <summary>
    /// 回合结束修改CD以及重置触发次数
    /// </summary>
    private void ResetSkillCdAndTriggerCount(Message msg)
    {
        for (int i = 0; i < SkillID.Count; i++)
        {
            int skillID = (int)SkillID[i];
            if (_skillCd.TryGetValue(skillID, out var effectCd))
            {
                for (int j = 0; j < 3; j++)
                {
                    if (effectCd.TryGetValue(i, out var effectInterval))
                    {
                        if (effectInterval > 0)
                        {
                            _skillCd[skillID][j]--;
                        }
                    }
                }
            }
            if (_skillTriggerCount.TryGetValue(skillID, out var effectTriggerCount))
            {
                for (int j = 1; j < 4; j++)
                {
                    if (effectTriggerCount.TryGetValue(i, out var effectTriggerCountValue))
                    {
                        if (effectTriggerCountValue == 0)
                        {
                            _skillTriggerCount[skillID][j] = GetSkillEffectParameters.GetSkillDataList(skillID, level)[j].EffectTriggerCount;
                        }
                    }
                }
            }
        }
    }
    
    
    /// <summary>
    /// 增加能量
    /// </summary>
    /// <param name="msg"></param>
    private void AddEnergy(Message msg)
    {
        int killer = (int)msg["killer"];
        int monsterType = (int)msg["monsterType"];

        if (killer != MarbleNumber) return;

        int needToAddEnergy = 0;
        switch (monsterType)
        {
            case 1:
                needToAddEnergy = TEnergyHelper.GetRow(1).KillMinMonster;
                break;
            case 2:
                needToAddEnergy = TEnergyHelper.GetRow(1).KillEliteMonster;
                break;
            case 3:
                needToAddEnergy = TEnergyHelper.GetRow(1).KillMinBoss;
                break;
            case 4:
                needToAddEnergy = TEnergyHelper.GetRow(1).KillBoss;
                break;
        }
        _energy += needToAddEnergy;
        // 最大能量
        int maxEnergy = TEnergyHelper.GetRow(1).HeroLimit;
        if (_energy > maxEnergy) _energy = maxEnergy;
        var copy = new Dictionary<string, object>();
        copy.Add("energy", _energy);
        copy.Add("MarbleNumber", MarbleNumber);
        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ADDMARBLEENERGY_CALLBACK,this,null, copy);
    }
}