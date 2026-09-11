using System;
using LC.Newtonsoft.Json.Linq;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using BNRoom.Static;
using Spine.Unity;
using Spine;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BNEnemyMarbleLogic : MonoBehaviour
{
    #region 变量
    [Header("敌方弹珠ID")]
    public int EnemyMarbleID;

    // 等级
    public int level;
    // 击杀获得经验
    public int Experience;
    // 生命值
    public int Health;
    // 攻击力
    public int AttackDamage;
    // 防御力
    public int Defense;
    // 暴击率
    public int CriticalRate;
    // 暴击伤害
    public int CriticalDamage;
    // 技能回合
    public JArray SkillTurn;
    // 当前技能回合
    public List<int> CurrentSkillTurn;
    // 技能ID
    public JArray SkillID;
    // 怪物模版ID
    public int MonsterTemplateID;
    // 元素类型
    public int Element;
    // 怪物类型(1-小怪、2-精英、3-小Boss、4-大Boss)
    public int monsterType;
    // 类型系数
    public float attackerTypeFactor;
    // 被连击次数
    public Dictionary<int, int> ComboCount = new Dictionary<int, int>();
    // 特效存放点
    public Transform effectRoom;
    // 弹珠目前生命
    public int currentHealth;
    // 死亡Spine动画
    private TrackEntry _dieTrackEntry;
    // 攻击Spine动画
    private TrackEntry _attackTrackEntry;
    // 当前技能ID
    private int _currentSkillID;
    // 攻击点
    private Vector3 _attackPoint;
    // 生命值分段显示:1-是，0-否
    private int _healthSegment;
    // 单段血量
    private int _segHpVal;
    // 血条分段
    private int _healthSegmentNum;
    // 当前是第几段
    private int _currentSegment;
    // Boss血条资源总数
    private int _bossHealthBarTotalNum = 6;
    // Boss血条当前资源
    private List<Sprite> bossHealthBar = new List<Sprite>();
    // 初始化是否完毕
    private bool _isInit = false;
    // 是否有使用技能次数
    public bool _hasUseSkillCount = false;
    // 使用技能后检查协程
    private Coroutine _hasUsedSkillCheck;
    #endregion
    
    #region 组件
    
    // Canvas
    private RectTransform canvasTransform;
    public RectTransform CanvasTransform
    {
        get
        {
            if (ReferenceEquals(canvasTransform, null))
            {
                canvasTransform = UnityHelper.FindTheChild(gameObject, "EnemyCanvas").GetComponent<RectTransform>();
            }
            return canvasTransform;
        }
    }
    
    // 弹珠血条
    private RectTransform healthBar;
    public RectTransform HealthBar
    {
        get
        {
            if (ReferenceEquals(healthBar, null))
            {
                healthBar = UnityHelper.FindTheChild(CanvasTransform.gameObject, "HealthBar")
                    .GetComponent<RectTransform>();
            }
            return healthBar;
        }
    }

    // 弹珠血条填充
    private Image healthBarFill;
    public Image HealthBarFill
    {
        get
        {
            if (ReferenceEquals(healthBarFill, null))
            {
                healthBarFill = UnityHelper.FindTheChild(HealthBar.gameObject, "HealthBarFill")
                    .GetComponent<Image>();
            }
            return healthBarFill;
        }
    }
    
    // BOSS血条表填充
    private Image healthBarFillFront;
    public Image HealthBarFillFront
    {
        get
        {
            if (ReferenceEquals(healthBarFillFront, null))
            {
                healthBarFillFront = UnityHelper.FindTheChild(HealthBar.gameObject, "HealthBarFillFront")
                    .GetComponent<Image>();
            }
            return healthBarFillFront;
        }
    }
    
    // BOSS血条里填充
    private Image healthBarFillBack;
    public Image HealthBarFillBack
    {
        get
        {
            if (ReferenceEquals(healthBarFillBack, null))
            {
                healthBarFillBack = UnityHelper.FindTheChild(HealthBar.gameObject, "HealthBarFillBack")
                    .GetComponent<Image>();
            }
            return healthBarFillBack;
        }
    }
    
    // BOSS血条段数
    private TextMeshProUGUI bossHealthCount;
    public TextMeshProUGUI BossHealthCount
    {
        get
        {
            if (ReferenceEquals(bossHealthCount, null))
            {
                bossHealthCount = UnityHelper.FindTheChild(HealthBar.gameObject, "HealthCount")
                    .GetComponent<TextMeshProUGUI>();
            }
            return bossHealthCount;
        }
    }
    
    // 弹珠血条延迟
    private Image healthBarDelay;
    public Image HealthBarDelay
    {
        get
        {
            if (ReferenceEquals(healthBarDelay, null))
            {
                healthBarDelay = UnityHelper.FindTheChild(HealthBar.gameObject, "HealthBarDelay")
                    .GetComponent<Image>();
            }
            return healthBarDelay;
        }
    }
    
    // 弹珠血条闪光遮罩
    private Image healthBarFlashMask;
    public Image HealthBarFlashMask
    {
        get
        {
            if (ReferenceEquals(healthBarFlashMask, null))
            {
                healthBarFlashMask = UnityHelper.FindTheChild(HealthBar.gameObject, "HealthBarFlashMask")
                    .GetComponent<Image>();
            }
            return healthBarFlashMask;
        }
    }
    
    // 弹珠回合数显示
    private TextMeshProUGUI roundCount1;
    public TextMeshProUGUI RoundCount1
    {
        get
        {
            if (ReferenceEquals(roundCount1, null))
            {
                roundCount1 = UnityHelper.FindTheChild(CanvasTransform.gameObject, "RoundCount1")
                    .GetComponent<TextMeshProUGUI>();
            }
            return roundCount1;
        }
    }
    
    private TextMeshProUGUI roundCount2;
    public TextMeshProUGUI RoundCount2
    {
        get
        {
            if (ReferenceEquals(roundCount2, null))
            {
                roundCount2 = UnityHelper.FindTheChild(CanvasTransform.gameObject, "RoundCount2")
                    .GetComponent<TextMeshProUGUI>();
            }
            return roundCount2;
        }
    }
    
    private TextMeshProUGUI roundCount3;
    public TextMeshProUGUI RoundCount3
    {
        get
        {
            if (ReferenceEquals(roundCount3, null))
            {
                roundCount3 = UnityHelper.FindTheChild(CanvasTransform.gameObject, "RoundCount3")
                    .GetComponent<TextMeshProUGUI>();
            }
            return roundCount3;
        }
    }
    
    private List<TextMeshProUGUI> roundCounts = new List<TextMeshProUGUI>(3);

    // 弹珠Spine
    private SkeletonAnimation enemySkeleton;
    public SkeletonAnimation EnemySkeleton
    {
        get
        {
            if (ReferenceEquals(enemySkeleton, null))
            {
                enemySkeleton = UnityHelper.FindTheChild(gameObject, "EnemySkeleton").GetComponent<SkeletonAnimation>();
            }
            return enemySkeleton;
        }
    }
    
    #endregion

    private void Awake()
    {
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_WHOSE_TURN_CALLBACK, SetTrun);
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_BUFFADDENEMYHEALTH, CalculateAddHealth);
        
        
    }

    private void OnDestroy()
    {
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_WHOSE_TURN_CALLBACK, SetTrun);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_BUFFADDENEMYHEALTH, CalculateAddHealth);
        
    }

    void OnEnable()
    {
        effectRoom = BNGameManager.Instance.EffectRoom;
        if (transform.GetComponent<Collider2D>() != null)
        {
            transform.GetComponent<Collider2D>().enabled = true;
        }
        if (EnemySkeleton.Skeleton != null)
        {
            EnemySkeleton.Skeleton.SetColor(Color.white);
        }
        StartCoroutine(EnterEnemyMarbleList());
        if (monsterType == 4)
        {
            AudioManager.Instance.PlayMusic(SysDefines.AUDIO + "BGM/" + "bgm_BOSS");
        }
        
        if (_isInit)
        {
            for (int i = 0; i < SkillTurn.Count; i++)
            {
                if ((int)SkillTurn[i] > 0)
                {
                    roundCounts[i].gameObject.SetActive(true);
                }
            }
        }
        
        // 1. 找到骨骼
        var boneName = "AttackPoint";
        Bone targetBone = EnemySkeleton.Skeleton.FindBone(boneName);
        if (targetBone == null)
        {
            Debug.LogWarning($"怪物{EnemyMarbleID}的骨骼 '{boneName}' 未找到！");
            return;
        }
        // 2. 获取骨骼在 Unity 世界空间中的位置
        _attackPoint = targetBone.GetWorldPosition(EnemySkeleton.transform);
    }
    
    private void SetTrun(Message msg)
    { 
        ComboCount.Clear();
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    public void InitData(int ID,int layer,int direction)
    {
        EnemyMarbleID = ID;
        level = TMonsterHelper.GetRow(EnemyMarbleID).Level;
        Experience = TMonsterHelper.GetRow(EnemyMarbleID).Exp;
        Health = TMonsterHelper.GetRow(EnemyMarbleID).HpBase;
        AttackDamage = TMonsterHelper.GetRow(EnemyMarbleID).Attack;
        Defense = TMonsterHelper.GetRow(EnemyMarbleID).Defence;
        CriticalRate = TMonsterHelper.GetRow(EnemyMarbleID).CritRate;
        CriticalDamage = TMonsterHelper.GetRow(EnemyMarbleID).CritHurt;
        SkillTurn = TMonsterHelper.GetRow(EnemyMarbleID).SkillTurn;
        MonsterTemplateID = TMonsterHelper.GetRow(EnemyMarbleID).TpltId;
        Element = TMonsterTemplateHelper.GetRow(MonsterTemplateID).Element;
        monsterType = TMonsterTemplateHelper.GetRow(MonsterTemplateID).Type;
        SkillID = TMonsterTemplateHelper.GetRow(MonsterTemplateID).Skill;
        
        EnemySkeleton.skeletonDataAsset = ResManager.Instance.LoadSpineData(SysDefines.ENEMYS + MonsterTemplateID + $"/{MonsterTemplateID}_SkeletonData");
        EnemySkeleton.Initialize(true);
        transform.GetComponent<BoundingBoxFollower>().Initialize();
        if (direction == 1)
        {
            EnemySkeleton.transform.localEulerAngles = new Vector3(0, 180, 0);
        }

        if (monsterType == 4)
        {
            GameObject enemyCanvas = ObjectPoolManager.Instance.Spawn(
                ResManager.Instance.LoadPrefab(SysDefines.ENEMYCANVAS + "EnemyCanvas" + MonsterTemplateID), Vector3.zero, Quaternion.identity,transform);
            enemyCanvas.name = "EnemyCanvas";
        }
        else
        {
            GameObject enemyCanvas = ObjectPoolManager.Instance.Spawn(
                ResManager.Instance.LoadPrefab(SysDefines.ENEMYCANVAS + "EnemyCanvas"), Vector3.zero, Quaternion.identity,transform);
            enemyCanvas.name = "EnemyCanvas";
            EnemySkeleton.GetComponent<MeshRenderer>().sortingOrder = layer;
            enemyCanvas.GetComponent<Canvas>().sortingOrder = layer;
        }
        
        roundCounts.Add(RoundCount1);
        roundCounts.Add(RoundCount2);
        roundCounts.Add(RoundCount3);
        for (int i = 0; i < 3; i++)
        {
            roundCounts[i].gameObject.SetActive(false);
        }
        
        JArray coefficient = TCoefficientHelper.GetRow(1).AttackCoefficient;
        attackerTypeFactor = (float)coefficient[3];

        _healthSegment = TMonsterHelper.GetRow(EnemyMarbleID).SegHpShow;
        if (_healthSegment == 1)
        {
            _segHpVal = TMonsterHelper.GetRow(EnemyMarbleID).SegHpVal; 
            _healthSegmentNum = Mathf.CeilToInt((float)Health / _segHpVal);
            for (int i = 0; i < _healthSegmentNum; i++)
            {
                int index = i;
                if (index > _bossHealthBarTotalNum - 1)
                {
                    index = i % _bossHealthBarTotalNum;
                }
                bossHealthBar.Add(ResManager.Instance.LoadSprite(SysDefines.BOSSHEALTHBAR + "BOSSBar" + index));
            }
        }
        
        // CurrentHealth = Health;
        ModifyHealth(Health, EnumCalculateType.Equals);
        for (int i = 0; i < SkillTurn.Count; i++)
        {
            CurrentSkillTurn.Add((int)SkillTurn[i]);
            roundCounts[i].text = CurrentSkillTurn[i].ToString();
            if ((int)SkillTurn[i] > 0)
            {
               roundCounts[i].gameObject.SetActive(true);
            }
        }
        _isInit = true;
    }

    IEnumerator EnterEnemyMarbleList()
    {
        yield return new WaitForSeconds(0.5f);
        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ADDENEMYCOUNT,this,gameObject);
    }

    void Update()
    {
        if (!_isInit) return;
        HealthBarUpdate();
    }
    
    /// <summary>
    /// 血条更新
    /// </summary>
    public void HealthBarUpdate()
    {
        float healthRatio;

        if (monsterType != 4)
        {
            healthRatio = (float)currentHealth / Health;
            if (!Mathf.Approximately(HealthBarFill.fillAmount, healthRatio))
            {
                HealthBarFill.fillAmount = healthRatio;
                HealthBar.DOScaleY(0.004f, 0.2f)
                    .OnComplete(() => HealthBar.DOScaleY(0.003f, 0.3f));
                HealthBarFlashMask.DOColor(Color.white, 0.2f)
                    .OnComplete(() => HealthBarFlashMask.DOColor(Color.clear, 0.3f));
                HealthBarDelay.DOFillAmount(
                    healthRatio, 
                    (HealthBarDelay.fillAmount - healthRatio) * 2.5f
                );
            }
        }
        else
        {
            _currentSegment = Mathf.CeilToInt((Health - currentHealth) / (float)_segHpVal);
            BossHealthCount.text = "x" + (_healthSegmentNum - _currentSegment);
            if (_currentSegment == 0) return;
            healthRatio = GetSegmentFill();
            if (!Mathf.Approximately(HealthBarFillFront.fillAmount, healthRatio))
            {
                if (_currentSegment == _healthSegmentNum)
                    HealthBarFillBack.sprite = null;
                else
                    HealthBarFillBack.sprite = bossHealthBar[_currentSegment];
                HealthBarFillFront.sprite = bossHealthBar[_currentSegment - 1];
                
                HealthBarFillFront.fillAmount = healthRatio;
                HealthBar.DOScaleY(0.004f, 0.2f)
                    .OnComplete(() => HealthBar.DOScaleY(0.003f, 0.3f));
                HealthBarFlashMask.DOColor(Color.white, 0.2f)
                    .OnComplete(() => HealthBarFlashMask.DOColor(Color.clear, 0.3f));
                HealthBarDelay.DOFillAmount(
                    healthRatio, 
                    (HealthBarDelay.fillAmount - healthRatio) * 2.5f
                );
            }
        } 
    }
    
    private float GetSegmentFill()
    {
        if (currentHealth == Health) return 1;
        int biLi = currentHealth % _segHpVal;
        float healthRatio = (float)biLi / _segHpVal;
        return healthRatio;
    }

    /// <summary>
    /// 敌方弹珠被攻击
    /// </summary>
    public void TakeDamage(int attackerATK,float attackerTypeFactor,int attackerElement,int attackerCritDamage,int attackerCritRate,int attackNum,bool enableCombo = true)
    {
        var copy = new Dictionary<string, object>();
        copy.Add("killer", attackNum);
        copy.Add("monsterType", monsterType);
        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ADDMARBLEENERGY,this,null, copy);
        // 播放受击动画
        SkeletonAnimSwitch("Hit");
        GameObject damageNumber = ObjectPoolManager.Instance.Spawn(
            ResManager.Instance.LoadPrefab(SysDefines.MARBLEUI + "DamageNumbers"),
            transform.position,
            Quaternion.identity,
            transform
        );
        
        damageNumber.transform.GetComponent<BNDamageNumberLogic>().MoveStart(transform.position, monsterType);

        int finalComboCount = 1;
        
        if (enableCombo)
        {
            if (ComboCount.ContainsKey(attackNum))
            {
                ComboCount[attackNum]++;
            }
            else
            {
                ComboCount.Add(attackNum,1);
            } 
            finalComboCount = ComboCount[attackNum];
        }
        
        // 计算最终伤害
        int finalDamage = DamageCalculator.CalculateDamage(
            attackerATK,
            attackerTypeFactor,
            attackerElement,
            Element,
            Defense,
            attackerCritDamage,
            attackerCritRate,
            (float)TCoefficientHelper.GetRow(1).BaseRestrain,
            (float)TCoefficientHelper.GetRow(1).BaseBeRestrain,
            (float)TCoefficientHelper.GetRow(1).SpecialRestrain,
            (float)TCoefficientHelper.GetRow(1).SpecialBaseRestrain,
            (float)TCoefficientHelper.GetRow(1).BaseSpecialRestrain,
            finalComboCount
            );
        
        if (finalDamage > 0)
        {
            // CurrentHealth -= finalDamage;
            ModifyHealth(finalDamage, EnumCalculateType.Subtract);
            damageNumber.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "-" + finalDamage;
            MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ADDCOMBOCOUNT,this,finalDamage);
        }
        else 
        {
            damageNumber.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "-0";
        }
        if (currentHealth <= 0)
        {
            // CurrentHealth = 0;
            ModifyHealth(0, EnumCalculateType.Equals);
            Die(attackNum);
        }
    }
    
    /// <summary>
    /// 切换Spine动画
    /// </summary>
    public void SkeletonAnimSwitch(string animName, bool isLoop = false,bool isAddIdle = true)
    {
        if (animName is "Attack" or "Attack2")
            _attackTrackEntry = EnemySkeleton.AnimationState.SetAnimation(0, animName, isLoop);
        else
            EnemySkeleton.AnimationState.SetAnimation(0, animName, isLoop);
        if (!isAddIdle) return;
        EnemySkeleton.AnimationState.AddAnimation(0, "Idle", true,0f);
    }

    /// <summary>
    /// 敌方弹珠死亡
    /// </summary>
    public void Die(int killer)
    {
        if (gameObject.activeSelf)
        {
            if (_hasUseSkillCount)
            {
                if (_hasUsedSkillCheck != null)
                {
                    StopCoroutine(_hasUsedSkillCheck);
                }
                BNGameManager.Instance.EnemyMarbleAttackTimes--;
            }
            
            MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_SUBENEMYCOUNT,this,gameObject);
            var copy = new Dictionary<string, object>();
            copy.Add("killer", killer);
            copy.Add("monsterType", monsterType);
            MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ADDMARBLEENERGY,this,null, copy);
            MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_ADDEXPERIENCE,this,Experience);
            transform.GetComponent<Collider2D>().enabled = false;
            _dieTrackEntry = EnemySkeleton.AnimationState.SetAnimation(0, "Die", false);
            _dieTrackEntry.Complete += (trackEntry) =>
            {
                Elapse();
            };
        }
    }
    
    private void CalculateAddHealth(Message msg)
    {
        int value = (int)msg["Value"];
        bool isPercentage = (bool)msg["IsPercentage"];
        if (isPercentage)
        {
            value = Health * (value / 100);
        }
        ModifyHealth(value, EnumCalculateType.Add);
    }

    /// <summary>
    /// 血量处理
    /// </summary>
    private void ModifyHealth(int value,EnumCalculateType calculateType)
    {
        switch (calculateType)
        {
            case EnumCalculateType.Add:
                currentHealth += value;
                if (currentHealth > Health)
                {
                    currentHealth = Health;
                }
                break;
            case EnumCalculateType.Subtract:
                currentHealth -= value;
                if (currentHealth < 0)
                {
                    currentHealth = 0;
                }
                break;
            case EnumCalculateType.Multiply:
                currentHealth *= value;
                break;
            case EnumCalculateType.Divide:
                currentHealth /= value;
                break;
            case EnumCalculateType.Equals:
                currentHealth = value;
                break;
        }
    }
    
    public void Elapse()
    {
        DOTween.ToAlpha(() => EnemySkeleton.Skeleton.GetColor(), x => EnemySkeleton.Skeleton.SetColor(x), 0, 0.5f)
            .OnComplete(() =>
            {
                MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_LEVELCHECK,this);
                if (gameObject.activeSelf)
                {
                    ObjectPoolManager.Instance.Unspawn(gameObject);
                }
            });
        return;
    }

    /// <summary>
    /// 敌方弹珠攻击
    /// </summary>
    public void Attack(int num)
    {
        int useSkillNum = num;
        if (useSkillNum == -1)
        {
            _hasUsedSkillCheck = StartCoroutine(HasUsedSkillCheck());
            return;
        }
        int sklId = (int)SkillID[useSkillNum];
        _currentSkillID = sklId;
        UseSkill();
    }

    /// <summary>
    /// 获取当前技能编号
    /// </summary>
    /// <returns></returns>
    public int GetUseSkillNum()
    {
        bool isNormalAttack = true;
        int useSkillNum = 0;
        bool noNormal = true;
        bool hasUseSkill = false;
        for (int i = 0; i < CurrentSkillTurn.Count; i++)
        {
            if ((int)SkillTurn[i] == 0)
            {
                noNormal = false;
                continue;
            }
            if (CurrentSkillTurn[i] == 0 && !hasUseSkill)
            {
                isNormalAttack = false;
                noNormal = false;
                useSkillNum = i;
                CurrentSkillTurn[i] = (int)SkillTurn[i];
                hasUseSkill = true;
            }
            else if (CurrentSkillTurn[i] > 0)
            {
                CurrentSkillTurn[i]--;
            }
            roundCounts[i].text = CurrentSkillTurn[i].ToString();
        }
        if (noNormal)
            return -1;
        if (isNormalAttack)
            return 0;
        
        return useSkillNum;
    }
    
    /// <summary>
    /// 使用技能
    /// </summary>
    private void UseSkill()
    {
        string spineAnim = GetSkillSpineAnim(_currentSkillID);
        if (TSkillHelper.GetRow(_currentSkillID).Type == 6)
        {
            GameObject ultimate = ObjectPoolManager.Instance.Spawn(
                ResManager.Instance.LoadPrefab(SysDefines.SPINES + "MonsterUltimates/" + "Ultimate" + MonsterTemplateID), 
                Vector3.zero,
                Quaternion.identity,
                BNGameManager.Instance.EffectRoom
            );
            TrackEntry ultimateTrackEntry = ultimate.GetComponent<SkeletonAnimation>().AnimationState.GetCurrent(0);
            ultimateTrackEntry.Event += OnUltimateEvent;
        }
        else
        {
            if (!string.IsNullOrEmpty(spineAnim))
            {
                // 播放攻击动画
                SkeletonAnimSwitch(spineAnim);
                // 创建蓄力特效
                ObjectPoolManager.Instance.Spawn(
                    ResManager.Instance.LoadPrefab(SysDefines.MARBLEEFFECTS + (EnumElementType)Element + "Element/" + "MonsterCharge"),
                    transform.position,
                    Quaternion.identity,
                    effectRoom
                );
                EnemySkeleton.GetComponent<MeshRenderer>().sortingOrder = 200;
                if (CanvasTransform.GetComponent<Canvas>().sortingOrder < 1000)
                {
                    CanvasTransform.GetComponent<Canvas>().sortingOrder = 200;
                }
                _attackTrackEntry.Event += OnAttackEvent;
                _attackTrackEntry.Complete += OnAttackComplete;
            }
            else
            {
                SkillEffect();
                _hasUsedSkillCheck = StartCoroutine(HasUsedSkillCheck());
            }
        }
        
    }
    
    private void OnAttackEvent(TrackEntry trackEntry, Spine.Event e)
    {
        SkillEffect();
        _hasUsedSkillCheck = StartCoroutine(HasUsedSkillCheck());
    }
    
    private void OnAttackComplete(TrackEntry trackEntry)
    {
        EnemySkeleton.GetComponent<MeshRenderer>().sortingOrder = 0;
        if (CanvasTransform.GetComponent<Canvas>().sortingOrder < 1000)
        {
            CanvasTransform.GetComponent<Canvas>().sortingOrder = 0;
        }
        _attackTrackEntry.Event -= OnAttackEvent;
        _attackTrackEntry.Complete -= OnAttackComplete;
    }
    
    private void OnUltimateEvent(TrackEntry trackEntry, Spine.Event e)
    {
        SkillEffect();
        _hasUsedSkillCheck = StartCoroutine(HasUsedSkillCheck());
        trackEntry.Event -= OnUltimateEvent;
    }

    private string GetSkillSpineAnim(int skillID)
    {
        return TSkillHelper.GetRow(skillID).ResId;
    }

    /// <summary>
    /// 技能效果
    /// </summary>
    private void SkillEffect()
    {
        SkillCaster.CastSkill(
            _currentSkillID,
            level,
            AttackDamage,
            Element,
            CriticalDamage,
            CriticalRate,
            attackerTypeFactor,
            gameObject,
            gameObject
        );
    }

    /// <summary>
    /// 使用技能完毕检查
    /// </summary>
    /// <returns></returns>
    private IEnumerator HasUsedSkillCheck()
    {
        if (!_hasUseSkillCount) yield break;
        _hasUseSkillCount = false;
        BNGameManager.Instance.EnemyMarbleAttackTimes--;
        if (BNGameManager.Instance.EnemyMarbleAttackTimes > 0) yield break;
        yield return new WaitForSeconds(1.2f);
        BNGameManager.Instance.EnemyMarbleAttackTimesCheck();
    }
    
}
