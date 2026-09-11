using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JBPROTO;
using Spine.Unity;
using Spine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarbleUI : BaseUI
{
    public override EnumUIType GetUIType()
    {
        return EnumUIType.MarbleUI;
    }
    protected override int UIOrder => 1;
    protected override EnumAnimationType AnimType => EnumAnimationType.None;
    public override bool EscapeClose => false;
    
    #region UI组件
    
    // 单机模式
    private RectTransform singleMode;
    public RectTransform SingleMode
    {
        get
        {
            if (ReferenceEquals(singleMode, null))
            {
                singleMode = UnityHelper.FindTheChild(gameObject, "SingleMode").GetComponent<RectTransform>();
            }
            return singleMode;
        }
    }
    
    // 多人模式
    private RectTransform onlineMode;
    public RectTransform OnlineMode
    {
        get
        {
            if (ReferenceEquals(onlineMode, null))
            {
                onlineMode = UnityHelper.FindTheChild(gameObject, "OnlineMode").GetComponent<RectTransform>();
            }
            return onlineMode;
        }
    }

    // 我的信息
    private RectTransform myInfo;
    public RectTransform MyInfo
    {
        get
        {
            if (ReferenceEquals(myInfo, null))
            {
                myInfo = UnityHelper.FindTheChild(gameObject, "MyInfo").GetComponent<RectTransform>();
            }
            return myInfo;
        }
    }
    
    // 我的生命值条背景
    private RectTransform myHealthBarBack;
    public RectTransform MyHealthBarBack
    {
        get
        {
            if (ReferenceEquals(myHealthBarBack, null))
            {
                myHealthBarBack = UnityHelper.FindTheChild(MyInfo.gameObject, "MyHealthBarBack").GetComponent<RectTransform>();
            }
            return myHealthBarBack;
        }
    }
    
    // 我的生命值条前端
    private Image myHealthBarFront;
    public Image MyHealthBarFront
    {
        get
        {
            if (ReferenceEquals(myHealthBarFront, null))
            {
                myHealthBarFront = UnityHelper.FindTheChild(MyHealthBarBack.gameObject, "MyHealthBarFront").GetComponent<Image>();
            }
            return myHealthBarFront;
        }
    }
    
    // 我的生命值条延迟
    private Image myHealthBarDelay;
    public Image MyHealthBarDelay
    {
        get
        {
            if (ReferenceEquals(myHealthBarDelay, null))
            {
                myHealthBarDelay = UnityHelper.FindTheChild(MyHealthBarBack.gameObject, "MyHealthBarDelay").GetComponent<Image>();
            }
            return myHealthBarDelay;
        }
    }
    
    // 我的生命值条发光
    private Image myHealthBarFlashMask;
    public Image MyHealthBarFlashMask
    {
        get
        {
            if (ReferenceEquals(myHealthBarFlashMask, null))
            {
                myHealthBarFlashMask = UnityHelper.FindTheChild(MyHealthBarBack.gameObject, "MyHealthBarFlashMask").GetComponent<Image>();
            }
            return myHealthBarFlashMask;
        }
    }
    
    // 我的生命值
    private TextMeshProUGUI myHealthCount;
    public TextMeshProUGUI MyHealthCount
    {
        get
        {
            if (ReferenceEquals(myHealthCount, null))
            {
                myHealthCount = UnityHelper.FindTheChild(MyInfo.gameObject, "MyHealthCount").GetComponent<TextMeshProUGUI>();
            }
            return myHealthCount;
        }
    }
    
    // 我的头像
    private Image myAvatar;
    public Image MyAvatar
    {
        get
        {
            if (ReferenceEquals(myAvatar, null))
            {
                myAvatar = UnityHelper.FindTheChild(MyInfo.gameObject, "MyAvatar").GetComponent<Image>();
            }
            return myAvatar;
        }
    }
    
    // 我的昵称
    private Text myName;
    public Text MyName
    {
        get
        {
            if (ReferenceEquals(myName, null))
            {
                myName = UnityHelper.FindTheChild(MyInfo.gameObject, "MyName").GetComponent<Text>();
            }
            return myName;
        }
    }
    
    // 倒计时
    private RectTransform countDown;
    public RectTransform CountDown
    {
        get
        {
            if (ReferenceEquals(countDown, null))
            {
                countDown = UnityHelper.FindTheChild(gameObject, "CountDown").GetComponent<RectTransform>();
            }
            return countDown;
        }
    }
    
    // 倒计时文本
    private TextMeshProUGUI countDownText;
    public TextMeshProUGUI CountDownText
    {
        get
        {
            if (ReferenceEquals(countDownText, null))
            {
                countDownText = UnityHelper.FindTheChild(CountDown.gameObject, "CountDownTimeText").GetComponent<TextMeshProUGUI>();
            }
            return countDownText;
        }
    }
    
    // 经验信息
    private RectTransform experience;
    public RectTransform Experience
    {
        get
        {
            if (ReferenceEquals(experience, null))
            {
                experience = UnityHelper.FindTheChild(SingleMode.gameObject, "Experience").GetComponent<RectTransform>();
            }
            return experience;
        }
    }
    
    // 经验条
    private Image experienceBar;
    public Image ExperienceBar
    {
        get
        {
            if (ReferenceEquals(experienceBar, null))
            {
                experienceBar = UnityHelper.FindTheChild(Experience.gameObject, "ExperienceBar").GetComponent<Image>();
            }
            return experienceBar;
        }
    }
    
    // 等级
    private TextMeshProUGUI level;
    public TextMeshProUGUI Level
    {
        get
        {
            if (ReferenceEquals(level, null))
            {
                level = UnityHelper.FindTheChild(Experience.gameObject, "Level").GetComponent<TextMeshProUGUI>();
            }
            return level;
        }
    }
    
    // 关卡信息
    private RectTransform stageInfo;
    public RectTransform StageInfo
    {
        get
        {
            if (ReferenceEquals(stageInfo, null))
            {
                stageInfo = UnityHelper.FindTheChild(SingleMode.gameObject, "StageInfo").GetComponent<RectTransform>();
            }
            return stageInfo;
        }
    }
    
    // 关卡名称
    private Image stageName;
    public Image StageName
    {
        get
        {
            if (ReferenceEquals(stageName, null))
            {
                stageName = UnityHelper.FindTheChild(StageInfo.gameObject, "StageName").GetComponent<Image>();
            }
            return stageName;
        }
    }
    
    // 副本名称
    private Image dungeonName;
    public Image DungeonName
    {
        get
        {
            if (ReferenceEquals(dungeonName, null))
            {
                dungeonName = UnityHelper.FindTheChild(StageInfo.gameObject, "DungeonName").GetComponent<Image>();
            }
            return dungeonName;
        }
    }
    
    // 关卡数
    private TextMeshProUGUI stageCount;
    public TextMeshProUGUI StageCount
    {
        get
        {
            if (ReferenceEquals(stageCount, null))
            {
                stageCount = UnityHelper.FindTheChild(StageInfo.gameObject, "StageCount").GetComponent<TextMeshProUGUI>();
            }
            return stageCount;
        }
    }
    
    // 关卡总数
    private TextMeshProUGUI totalStageCount;
    public TextMeshProUGUI TotalStageCount
    {
        get
        {
            if (ReferenceEquals(totalStageCount, null))
            {
                totalStageCount = UnityHelper.FindTheChild(StageInfo.gameObject, "TotalStageCount").GetComponent<TextMeshProUGUI>();
            }
            return totalStageCount;
        }
    }
    
    // 回合数
    private TextMeshProUGUI roundCount;
    public TextMeshProUGUI RoundCount
    {
        get
        {
            if (ReferenceEquals(roundCount, null))
            {
                roundCount = UnityHelper.FindTheChild(StageInfo.gameObject, "RoundCount").GetComponent<TextMeshProUGUI>();
            }
            return roundCount;
        }
    }
    
    // 战斗信息
    private RectTransform battleInfo;
    public RectTransform BattleInfo
    {
        get
        {
            if (ReferenceEquals(battleInfo, null))
            {
                battleInfo = UnityHelper.FindTheChild(SingleMode.gameObject, "BattleInfo").GetComponent<RectTransform>();
            }
            return battleInfo;
        }
    }
    
    // 通关条件
    private RectTransform clearCondition;
    public RectTransform ClearCondition
    {
        get
        {
            if (ReferenceEquals(clearCondition, null))
            {
                clearCondition = UnityHelper.FindTheChild(BattleInfo.gameObject, "ClearCondition").GetComponent<RectTransform>();
            }
            return clearCondition;
        }
    }
    
    // 通关条件图标1（全灭敌人）
    private RectTransform clearConditionIconOne;
    public RectTransform ClearConditionIconOne
    {
        get
        {
            if (ReferenceEquals(clearConditionIconOne, null))
            {
                clearConditionIconOne = UnityHelper.FindTheChild(ClearCondition.gameObject, "ClearConditionIconOne").GetComponent<RectTransform>();
            }
            return clearConditionIconOne;
        }
    }
    
    // 通关条件图标2（通关条件）
    private RectTransform clearConditionIconTwo;
    public RectTransform ClearConditionIconTwo
    {
        get
        {
            if (ReferenceEquals(clearConditionIconTwo, null))
            {
                clearConditionIconTwo = UnityHelper.FindTheChild(ClearCondition.gameObject, "ClearConditionIconTwo").GetComponent<RectTransform>();
            }
            return clearConditionIconTwo;
        }
    }
    
    // 通关条件Spine
    private SkeletonGraphic clearConditionSpine;
    public SkeletonGraphic ClearConditionSpine
    {
        get
        {
            if (ReferenceEquals(clearConditionSpine, null))
            {
                clearConditionSpine = UnityHelper.FindTheChild(ClearCondition.gameObject, "ClearConditionSpine").GetComponent<SkeletonGraphic>();
            }
            return clearConditionSpine;
        }
    }
    
    // 战斗开始
    private RectTransform battleStart;
    public RectTransform BattleStart
    {
        get
        {
            if (ReferenceEquals(battleStart, null))
            {
                battleStart = UnityHelper.FindTheChild(BattleInfo.gameObject, "BattleStart").GetComponent<RectTransform>();
            }
            return battleStart;
        }
    }
    
    // 战斗开始图标
    private RectTransform battleStartIcon;
    public RectTransform BattleStartIcon
    {
        get
        {
            if (ReferenceEquals(battleStartIcon, null))
            {
                battleStartIcon = UnityHelper.FindTheChild(BattleStart.gameObject, "BattleStartIcon").GetComponent<RectTransform>();
            }
            return battleStartIcon;
        }
    }
    
    // 战斗开始Spine
    private RectTransform battleStartSpine;
    public RectTransform BattleStartSpine
    {
        get
        {
            if (ReferenceEquals(battleStartSpine, null))
            {
                battleStartSpine = UnityHelper.FindTheChild(BattleStart.gameObject, "BattleStartSpine").GetComponent<RectTransform>();
            }
            return battleStartSpine;
        }
    }
    
    // 敌我回合
    private RectTransform factionTurn;
    public RectTransform FactionTurn
    {
        get
        {
            if (ReferenceEquals(factionTurn, null))
            {
                factionTurn = UnityHelper.FindTheChild(gameObject, "FactionTurn").GetComponent<RectTransform>();
            }
            return factionTurn;
        }
    }
    
    // 我方回合背景
    private RectTransform myTurnBack;
    public RectTransform MyTurnBack
    {
        get
        {
            if (ReferenceEquals(myTurnBack, null))
            {
                myTurnBack = UnityHelper.FindTheChild(FactionTurn.gameObject, "MyTurnBack").GetComponent<RectTransform>();
            }
            return myTurnBack;
        }
    }
    
    // 我方回合字体
    private RectTransform myTurnFront;
    public RectTransform MyTurnFront
    {
        get
        {
            if (ReferenceEquals(myTurnFront, null))
            {
                myTurnFront = UnityHelper.FindTheChild(FactionTurn.gameObject, "MyTurnFront").GetComponent<RectTransform>();
            }
            return myTurnFront;
        }
    }
    
    // 敌方回合背景
    private RectTransform enemyTurnBack;
    public RectTransform EnemyTurnBack
    {
        get
        {
            if (ReferenceEquals(enemyTurnBack, null))
            {
                enemyTurnBack = UnityHelper.FindTheChild(FactionTurn.gameObject, "EnemyTurnBack").GetComponent<RectTransform>();
            }
            return enemyTurnBack;
        }
    }
    
    // 敌方回合字体
    private RectTransform enemyTurnFront;
    public RectTransform EnemyTurnFront
    {
        get
        {
            if (ReferenceEquals(enemyTurnFront, null))
            {
                enemyTurnFront = UnityHelper.FindTheChild(FactionTurn.gameObject, "EnemyTurnFront").GetComponent<RectTransform>();
            }
            return enemyTurnFront;
        }
    }
    
    // 伤害信息
    private RectTransform hitInfo;
    public RectTransform HitInfo
    {
        get
        {
            if (ReferenceEquals(hitInfo, null))
            {
                hitInfo = UnityHelper.FindTheChild(gameObject, "HitInfo").GetComponent<RectTransform>();
            }
            return hitInfo;
        }
    }
    
    // 连击图标
    private RectTransform comboIcon;
    public RectTransform ComboIcon
    {
        get
        {
            if (ReferenceEquals(comboIcon, null))
            {
                comboIcon = UnityHelper.FindTheChild(HitInfo.gameObject, "ComboIcon").GetComponent<RectTransform>();
            }
            return comboIcon;
        }
    }
    
    // 连击数
    private TextMeshProUGUI comboCount;
    public TextMeshProUGUI ComboCount
    {
        get
        {
            if (ReferenceEquals(comboCount, null))
            {
                comboCount = UnityHelper.FindTheChild(ComboIcon.gameObject, "ComboCount").GetComponent<TextMeshProUGUI>();
            }
            return comboCount;
        }
    }
    
    // 总伤害
    private TextMeshProUGUI totalDamageCount;
    public TextMeshProUGUI TotalDamageCount
    {
        get
        {
            if (ReferenceEquals(totalDamageCount, null))
            {
                totalDamageCount = UnityHelper.FindTheChild(HitInfo.gameObject, "TotalDamageCount").GetComponent<TextMeshProUGUI>();
            }
            return totalDamageCount;
        }
    }

    #region 结算界面
    
    private RectTransform gameSettlement;
    public RectTransform GameSettlement
    {
        get
        {
            if (ReferenceEquals(gameSettlement, null))
            {
                gameSettlement = UnityHelper.FindTheChild(gameObject, "GameSettlement").GetComponent<RectTransform>();
            }
            return gameSettlement;
        }
    }
    
    // 结算界面Spine
    private SkeletonGraphic fullBodyAnimation;
    public SkeletonGraphic FullBodyAnimation
    {
        get
        {
            if (ReferenceEquals(fullBodyAnimation, null))
            {
                fullBodyAnimation = UnityHelper.FindTheChild(GameSettlement.gameObject, "FullBodyAnimation").GetComponent<SkeletonGraphic>();
            }
            return fullBodyAnimation;
        }
    }
    
    // 结算输赢
    private Image winOrLose;
    public Image WinOrLose
    {
        get
        {
            if (ReferenceEquals(winOrLose, null))
            {
                winOrLose = UnityHelper.FindTheChild(GameSettlement.gameObject, "WinOrLose").GetComponent<Image>();
            }
            return winOrLose;
        }
    }
    
    // 战斗用时
    private TextMeshProUGUI battleTimeText;
    public TextMeshProUGUI BattleTimeText
    {
        get
        {
            if (ReferenceEquals(battleTimeText, null))
            {
                battleTimeText = UnityHelper.FindTheChild(GameSettlement.gameObject, "BattleTimeText").GetComponent<TextMeshProUGUI>();
            }
            return battleTimeText;
        }
    }
    
    // 结算界面品质
    private TextMeshProUGUI gameSettlementQuality;
    public TextMeshProUGUI GameSettlementQuality
    {
        get
        {
            if (ReferenceEquals(gameSettlementQuality, null))
            {
                gameSettlementQuality = UnityHelper.FindTheChild(GameSettlement.gameObject, "Quality").GetComponent<TextMeshProUGUI>();
            }
            return gameSettlementQuality;
        }
    }
    
    #endregion
    
    #region 弹珠信息

    // 弹珠容器
    private RectTransform marbles;
    public RectTransform Marbles
    {
        get
        {
            if (ReferenceEquals(marbles, null))
            {
                marbles = UnityHelper.FindTheChild(gameObject, "Marbles").GetComponent<RectTransform>();
            }
            return marbles;
        }
    }

    // 弹珠序号列表
    private List<RectTransform> marbleSerialNumber = new List<RectTransform>(4);
    public List<RectTransform> MarbleSerialNumber
    {
        get
        {
            for (int i = 0; i < 4; i++)
            {
                marbleSerialNumber.Add(UnityHelper.FindTheChild(Marbles.gameObject, (i + 1).ToString())
                    .GetComponent<RectTransform>());
            }
            return marbleSerialNumber;
        }
    }
    
    // 弹珠框架
    private List<Image> marbleFrame = new List<Image>(4);
    public List<Image> MarbleFrame
    {
        get
        {
            foreach (var t in MarbleSerialNumber)
            {
                marbleFrame.Add(UnityHelper.FindTheChild(t.gameObject, "Frame")
                    .GetComponent<Image>());
            }
            return marbleFrame;
        }
    }
    
    // 弹珠发光框架
    private List<Image> marbleGlowFrame = new List<Image>(4);
    public List<Image> MarbleGlowFrame
    {
        get
        {
            foreach (var t in MarbleSerialNumber)
            {
                marbleGlowFrame.Add(UnityHelper.FindTheChild(t.gameObject, "Light")
                    .GetComponent<Image>());
            }
            return marbleGlowFrame;
        }
    }
    
    // 弹珠半身像
    private List<Image> portrait = new List<Image>(4);
    public List<Image> Portrait
    {
        get
        {
            foreach (var t in MarbleSerialNumber)
            {
                portrait.Add(UnityHelper.FindTheChild(t.gameObject, "Icon")
                    .GetComponent<Image>());
            }
            return portrait;
        }
    }
    
    // 弹珠Spine
    private List<SkeletonGraphic> marbleSpine = new List<SkeletonGraphic>(4);
    public List<SkeletonGraphic> MarbleSpine
    {
        get
        {
            foreach (var t in MarbleSerialNumber)
            {
                marbleSpine.Add(UnityHelper.FindTheChild(t.gameObject, "Spine")
                    .GetComponent<SkeletonGraphic>());
            }
            return marbleSpine;
        }
    }
    
    // 弹珠等级
    private List<TextMeshProUGUI> marbleLevel = new List<TextMeshProUGUI>(4);
    public List<TextMeshProUGUI> MarbleLevel
    {
        get
        {
            foreach (var t in MarbleSerialNumber)
            {
                marbleLevel.Add(UnityHelper.FindTheChild(t.gameObject, "Level")
                    .GetComponent<TextMeshProUGUI>());
            }
            return marbleLevel;
        }
    }
    
    // 弹珠Buff
    private List<RectTransform> marbleBuff = new List<RectTransform>(4);
    public List<RectTransform> MarbleBuff
    {
        get
        {
            foreach (var t in MarbleSerialNumber)
            {
                marbleBuff.Add(UnityHelper.FindTheChild(t.gameObject, "Buffs")
                    .GetComponent<RectTransform>());
            }
            return marbleBuff;
        }
    }
    
    // 弹珠星级
    private List<RectTransform> marbleStars = new List<RectTransform>(4);
    public List<RectTransform> MarbleStars
    {
        get
        {
            foreach (var t in MarbleSerialNumber)
            {
                marbleStars.Add(UnityHelper.FindTheChild(t.gameObject, "Stars")
                    .GetComponent<RectTransform>());
            }
            return marbleStars;
        }
    }
    
    // 弹珠品质
    private List<Image> marbleQuality = new List<Image>(4);
    public List<Image> MarbleQuality
    {
        get
        {
            foreach (var t in MarbleSerialNumber)
            {
                marbleQuality.Add(UnityHelper.FindTheChild(t.gameObject, "Quality")
                    .GetComponent<Image>());
            }
            return marbleQuality;
        }
    }
    
    // 弹珠元素
    private List<Image> marbleElement = new List<Image>(4);
    public List<Image> MarbleElement
    {
        get
        {
            foreach (var t in MarbleSerialNumber)
            {
                marbleElement.Add(UnityHelper.FindTheChild(t.gameObject, "Element")
                    .GetComponent<Image>());
            }
            return marbleElement;
        }
    }
    
    // 弹珠能量条
    private List<Image> marbleEnergyBar = new List<Image>(4);
    public List<Image> MarbleEnergyBar
    {
        get
        {
            foreach (var t in MarbleSerialNumber)
            {
                marbleEnergyBar.Add(UnityHelper.FindTheChild(t.gameObject, "EnergyBarFront")
                    .GetComponent<Image>());
            }
            return marbleEnergyBar;
        }
    }
    
    // 弹珠选中特效
    private List<GameObject> selectEffect = new List<GameObject>(4);
    public List<GameObject> SelectEffect
    {
        get
        {
            foreach (var t in MarbleSerialNumber)
            {
                selectEffect.Add(UnityHelper.FindTheChild(t.gameObject, "SelectEffect").gameObject);
            }
            return selectEffect;
        }
    }
    
    #endregion
    
    #endregion

    protected override void OnStart()
    {
        // 添加事件监听
        // 获取英雄信息
        MessageCenter.Instance.AddListener(MsgType.NET_UI_GET_HERO_INFO, HeroInfo);
        // 切换回合
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_WHOSE_TURN, WhoseTurn);
        // 修改生命值
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_MODIFYHEALTH , ModifyHealth);
        // 获取当前关卡
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_ADDSTAGECOUNT , ModifyStageCount);
        // 获取回合数
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_ADDROUNDCOUNT , ModifyRoundCount);
        // 获取连击和总伤
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_ADDCOMBOCOUNT_CALLBACK, ModifyComboCount);
        // 获取正在选中的弹珠
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_SHOWMARBLESELECT, MarbleSelect);
        // 关闭选中弹珠效果
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_CLOSEMARBLESELECT, HideMarbleSelect);
        // 获取能量
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_ADDMARBLEENERGY_CALLBACK, ModifyEnergyBar);
        // 获取经验
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_ADDEXPERIENCE_CALLBACK, ModifyExperience);
        // 获取结算消息
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_GAMESETTLEMENT, ShowGameSettlement);
        // 获取倒计时信息
        MessageCenter.Instance.AddListener(MsgType.LOCAL_SEND_MODIFYCOUNTDOWN, ModifyCountDown);

        StartCoroutine(WaitToGetInfo());
    }

    protected override void OnRelease()
    {
        // 移除事件监听
        MessageCenter.Instance.RemoveListener(MsgType.NET_UI_GET_HERO_INFO, HeroInfo);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_WHOSE_TURN, WhoseTurn);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_MODIFYHEALTH , ModifyHealth);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_ADDSTAGECOUNT , ModifyStageCount);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_ADDROUNDCOUNT , ModifyRoundCount);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_ADDCOMBOCOUNT_CALLBACK, ModifyComboCount);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_SHOWMARBLESELECT, MarbleSelect);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_CLOSEMARBLESELECT, HideMarbleSelect);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_ADDMARBLEENERGY_CALLBACK, ModifyEnergyBar);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_ADDEXPERIENCE_CALLBACK, ModifyExperience);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_GAMESETTLEMENT, ShowGameSettlement);
        MessageCenter.Instance.RemoveListener(MsgType.LOCAL_SEND_MODIFYCOUNTDOWN, ModifyCountDown);
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (!BNGameManager.Instance || !BNPlayerController.Instance) return;

        TotalStageCount.text = "/" + BNGameManager.Instance.MaxScene;

        var inputSpace = Input.GetKeyDown(KeyCode.Space);

        if (inputSpace && BNGameManager.Instance._isGameSettlement)
        {
            UIManager.Instance.OpenUI(EnumUIType.LoadingUI, true, EnumUIType.LobbyUI, EnumSceneType.MainScene);
        }
    }

    IEnumerator WaitToGetInfo()
    {
        yield return null;
        NetController.Instance.SendGetHeroReq(MsgType.NET_UI_GET_HERO_INFO);
        MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_SETHEALTH,this,0);
    }

    /// <summary>
    /// 显示生命值
    /// </summary>
    /// <param name="msg"></param>
    private void ModifyHealth(Message msg)
    {
        // 解包
        int currentHealth = (int)msg["CurrentHealth"];
        int totalHealth = (int)msg["totalHealth"];
        // 计算生命值百分比
        float healthRatio = (float)currentHealth / totalHealth;
        // 显示生命值
        MyHealthCount.text = currentHealth + " / " + totalHealth;
        // 放大生命值条
        MyHealthBarBack.DOScaleY(1.2f, 0.2f)
            .OnComplete(() => MyHealthBarBack.DOScaleY(1f, 0.3f));
        // 闪烁生命值条
        MyHealthBarFlashMask.DOColor(Color.white, 0.2f)
            .OnComplete(() => MyHealthBarFlashMask.DOColor(Color.clear, 0.3f));
        // 显示生命值
        MyHealthBarFront.fillAmount = healthRatio;
        // 延迟显示生命值
        MyHealthBarDelay.DOFillAmount(
            healthRatio, 
            (MyHealthBarDelay.fillAmount - healthRatio) * 10
        );
    }

    /// <summary>
    /// 显示场景号
    /// </summary>
    /// <param name="msg"></param>
    private void ModifyStageCount(Message msg)
    {
        StageCount.text = msg.Content.ToString();
    }

    /// <summary>
    /// 显示回合数
    /// </summary>
    /// <param name="msg"></param>
    private void ModifyRoundCount(Message msg)
    {
        RoundCount.text = msg.Content.ToString();
    }
    
    /// <summary>
    /// 显示倒计时
    /// </summary>
    private void ModifyCountDown(Message msg)
    {
        CountDownText.text = msg.Content.ToString();
        Vector3 countDownPos = CountDownText.transform.localPosition;
        CountDown.DOShakePosition(0.5f, 5f, 15, 90, false, false)
            .OnComplete(() =>
            {
                CountDownText.transform.localPosition = countDownPos;
            });
    }
    
    /// <summary>
    /// 显示连击和总伤数
    /// </summary>
    private void ModifyComboCount(Message msg)
    {
        int comboCount = (int)msg["ComboCount"];
        int comboDamage = (int)msg["ComboDamage"];
        bool isCombo = (bool)msg.Content;
        ComboCount.text = comboCount.ToString();
        TotalDamageCount.text = "zs" + comboDamage;
        if (isCombo)
        {
            HitInfo.gameObject.SetActive(false);
        }
        else
        {
            HitInfo.gameObject.SetActive(true);
            Sequence seq = DOTween.Sequence();
            seq.Append(ComboIcon.DOScale(1f, 0f));
            seq.Append(ComboIcon.DOScale(1.3f, 0.2f));
            seq.Append(ComboIcon.DOScale(1f, 0.2f));
        }
        
    }

    /// <summary>
    /// 显示能量
    /// </summary>
    private void ModifyEnergyBar(Message msg)
    {
        int energy = (int)msg["energy"];
        int marbleNum = (int)msg["MarbleNumber"];
        int maxEnergy = TEnergyHelper.GetRow(1).HeroLimit;
        MarbleEnergyBar[marbleNum].fillAmount = (float)energy / maxEnergy;
    }
    
    /// <summary>
    /// 显示弹珠选中特效
    /// </summary>
    private void MarbleSelect(Message msg)
    { 
        int marbleSerialNumber = (int)msg.Content - 1;
        SelectEffect[marbleSerialNumber].SetActive(true);
    }

    /// <summary>
    /// 显示经验值
    /// </summary>
    private void ModifyExperience(Message msg)
    {
        int exp = (int)msg["experience"];
        int levelInGame = (int)msg["levelInGame"];
        ExperienceBar.fillAmount = (float)exp / 10;
        Level.text = levelInGame.ToString();
    }
    
    /// <summary>
    /// 关闭弹珠选中特效
    /// </summary>
    private void HideMarbleSelect(Message msg)
    { 
        int marbleSerialNumber = (int)msg.Content - 1;
        SelectEffect[marbleSerialNumber].SetActive(false);
    }
    
    public void OnLoadingUiClose()
    {
        BattleInfo.gameObject.SetActive(true);
        ClearConditionIconOne.localPosition = new Vector3(-1370, 300, 0);
        ClearConditionIconTwo.localPosition = new Vector3(1650, 210, 0);
        Sequence seq = DOTween.Sequence();
        seq.Append(ClearConditionIconOne.DOLocalMoveX(-278, 0.3f).SetEase(Ease.OutQuad));
        seq.Join(ClearConditionIconTwo.DOLocalMoveX(0, 0.3f).SetEase(Ease.OutQuad));
        
        seq.Append(ClearConditionIconOne.DOScale(1.4f, 0.25f));
        seq.Join(ClearConditionIconTwo.DOScale(1.4f, 0.25f));
        
        seq.Append(ClearConditionIconOne.DOScale(1, 0.25f));
        seq.Join(ClearConditionIconTwo.DOScale(1f, 0.25f));
        
        seq.AppendCallback(() =>
        {
            ClearConditionSpine.gameObject.SetActive(true);
            ClearConditionIconOne.gameObject.SetActive(false);
            ClearConditionIconTwo.gameObject.SetActive(false);
            StartCoroutine(CloseClearCondition());
        });
    }
    
    IEnumerator CloseClearCondition()
    {
        yield return new WaitForSeconds(1.4f);
        BattleStart.gameObject.SetActive(true);
        BattleStart.localScale = new Vector3(1, 1, 0);
        ClearCondition.gameObject.SetActive(false);

        TrackEntry battleStartTrackEntry =
            BattleStartSpine.GetComponent<SkeletonGraphic>().AnimationState.GetCurrent(0);
        battleStartTrackEntry.Complete += TrackEntryComplete;
    }
    
    void TrackEntryComplete(TrackEntry trackEntry)
    {
        AudioManager.Instance.PlayMusic(SysDefines.AUDIO + "BGM/" + "bgm_Battle");
        BattleInfo.gameObject.SetActive(false);
        BattleStart.gameObject.SetActive(false);
        BNPlayerController.Instance.UiPlayFinished();
        trackEntry.Complete -= TrackEntryComplete;
    }
    
    private void WhoseTurn(Message msg)
    {
        int whoseTurn = (int)msg.Content;
        switch (whoseTurn)
        {
            case 1:
                MyTurnBack.localScale = Vector3.zero;
                MyTurnFront.localPosition = new Vector3(1500, -600, 0);
                MyTurnBack.gameObject.SetActive(true);
                MyTurnFront.gameObject.SetActive(true);
                Sequence seq1 = DOTween.Sequence();
                seq1.Append(MyTurnBack.DOScale(1f, 0.2f));
                seq1.Append(MyTurnFront.DOLocalMoveX(0, 0.2f));
                seq1.AppendInterval(1f);
                seq1.Append(MyTurnBack.DOScale(0f, 0.2f));
                seq1.Append(MyTurnFront.DOLocalMoveX(-1500, 0.2f));
                seq1.AppendCallback(() =>
                {
                    MyTurnBack.gameObject.SetActive(false);
                    MyTurnFront.gameObject.SetActive(false);
                    MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_WHOSE_TURN_CALLBACK,this,whoseTurn);
                });
                break;
            case 2:
                EnemyTurnBack.localScale = new Vector3(2, 2, 2);
                EnemyTurnFront.localPosition = new Vector3(-1500, 800, 0);
                EnemyTurnBack.gameObject.SetActive(true);
                EnemyTurnFront.gameObject.SetActive(true);
                Sequence seq2 = DOTween.Sequence();
                seq2.Append(EnemyTurnBack.DOScale(1f, 0.2f));
                seq2.Append(EnemyTurnFront.DOLocalMoveX(0, 0.2f));
                seq2.AppendInterval(1f);
                seq2.Append(EnemyTurnBack.DOScale(0f, 0.2f));
                seq2.Append(EnemyTurnFront.DOLocalMoveX(1500, 0.2f));
                seq2.AppendCallback(() =>
                {
                    EnemyTurnBack.gameObject.SetActive(false);
                    EnemyTurnFront.gameObject.SetActive(false);
                    MessageCenter.Instance.SendMessage(MsgType.LOCAL_SEND_WHOSE_TURN_CALLBACK,this,whoseTurn);
                });
                break;
        }
    }

    private void ShowGameSettlement(Message msg)
    {
        bool isWin = (bool)msg.Content;
        BattleTimeText.text = RoundCount.text;
        if (isWin)
        {
            WinOrLose.sprite = ResManager.Instance.LoadSprite(SysDefines.SETTLEMENT + "Win");
            AudioManager.Instance.PlayMusic(SysDefines.AUDIO + "BGM/" + "bgm_GameWin");
        }
        else
        {
            WinOrLose.sprite = ResManager.Instance.LoadSprite(SysDefines.SETTLEMENT + "Lose");
            AudioManager.Instance.PlayMusic(SysDefines.AUDIO + "BGM/" + "bgm_GameLose");
        }

        StartCoroutine(DelayShowGameSettlement());
        BNGameManager.Instance._isGameSettlement = true;
    }

    private IEnumerator DelayShowGameSettlement()
    {
        yield return new WaitForSeconds(3f);
        GameSettlement.gameObject.SetActive(true);
    }
    
    private void HeroInfo(Message msg)
    {
        var rsp = msg.Content as CLPFGetHeroAck;
        int showInSettlement = Random.Range(0, 4);
        for (int i = 0; i < rsp.heros.Length; i++)
        {
            // 英雄ID
            int heroId = rsp.heros[i].heroId;
            // 设置半身像框
            MarbleFrame[i].sprite = ResManager.Instance.LoadSprite(SysDefines.PORTRAITFRAME + "PortraitFrame_" + THeroHelper.GetRow(heroId).Quality);
            // 设置半身像光晕
            MarbleGlowFrame[i].sprite = ResManager.Instance.LoadSprite(SysDefines.PORTRAITLIGHT + "PortraitLight_" + THeroHelper.GetRow(heroId).Quality);
            // 设置半身像
            Portrait[i].sprite = ResManager.Instance.LoadSprite(SysDefines.PORTRAIT + THeroHelper.GetRow(heroId).Portrait);
            // 设置Spine
            MarbleSpine[i].skeletonDataAsset = ResManager.Instance.LoadSpineData(SysDefines.BATTLEPORTRAIT + heroId + "/" + heroId + "_SkeletonData");
            // 设置弹珠等级
            MarbleLevel[i].text = rsp.heros[i].level.ToString();
            // 设置星级
            int starCount = rsp.heros[i].star;
            for (int j = 0; j <  MarbleStars[i].childCount; j++)
            {
                MarbleStars[i].transform.GetChild(j).gameObject.SetActive(j < starCount);
            }
            // 设置品质
            MarbleQuality[i].sprite = ResManager.Instance.LoadSprite(SysDefines.QUALITYICON + THeroHelper.GetRow(heroId).QualityIcon);
            ObjectPoolManager.Instance.Spawn(
                ResManager.Instance.LoadPrefab(
                    SysDefines.QUALITYICON + THeroHelper.GetRow(heroId).QualityIcon + "Effect"),
                MarbleQuality[i].transform
                );
            // 设置元素
            MarbleElement[i].sprite = ResManager.Instance.LoadSprite(SysDefines.ELEMENTICON + THeroHelper.GetRow(heroId).ElementIcon);
            // 设置弹珠能量条
            MarbleEnergyBar[i].sprite = ResManager.Instance.LoadSprite(SysDefines.MARBLEENERGYBAR + "MarbleEnergyBar_" + THeroHelper.GetRow(heroId).Quality);

            if (i == showInSettlement)
            {
                FullBodyAnimation.skeletonDataAsset =
                    ResManager.Instance.LoadSpineData(SysDefines.SPINES + "GameSettlement/" + heroId + $"/{heroId}_SkeletonData");
                GameSettlementQuality.text = (THeroHelper.GetRow(heroId).Quality - 2).ToString();
            }

            MarbleSpine[i].gameObject.SetActive(true);
        }
    }
}
