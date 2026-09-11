using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using JBPROTO;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class LobbyUI : BaseUI
{
    public override EnumUIType GetUIType()
    {
        return EnumUIType.LobbyUI;
    }
    protected override int UIOrder => 1;
    protected override EnumAnimationType AnimType => EnumAnimationType.None;
    public override bool EscapeClose => false;

    #region 公用组件

    private RectTransform rotateSelection;
    public RectTransform RotateSelection
    {
        get
        {
            if (ReferenceEquals(rotateSelection,null))
            {
                rotateSelection = UnityHelper.FindTheChild(gameObject, "RotateSelection").GetComponent<RectTransform>();
            }
            return rotateSelection;
        }
    }

    private RectTransform television;
    public RectTransform Television
    {
        get
        {
            if (ReferenceEquals(television,null))
            {
                television = UnityHelper.FindTheChild(gameObject, "Television").GetComponent<RectTransform>();
            }
            return television;
        }
    }
    
    // 用户信息
    private RectTransform userInfo;
    public RectTransform UserInfo
    {
        get
        {
            if (ReferenceEquals(userInfo,null))
            {
                userInfo = UnityHelper.FindTheChild(gameObject, "UserInfo").GetComponent<RectTransform>();
            }
            return userInfo;
        }
    }
    
    // 用户昵称
    private Text userNickName;
    public Text UserNickName
    {
        get
        {
            if (ReferenceEquals(userNickName,null))
            {
                userNickName = UnityHelper.FindTheChild(UserInfo.gameObject, "NickName").GetComponent<Text>();
            }
            return userNickName;
        }
    }
    
    // 用户战力
    private TextMeshProUGUI userCombatPower;
    public TextMeshProUGUI UserCombatPower
    {
        get
        {
            if (ReferenceEquals(userCombatPower,null))
            {
                userCombatPower = UnityHelper.FindTheChild(UserInfo.gameObject, "CombatPower").GetComponent<TextMeshProUGUI>();
            }
            return userCombatPower;
        }
    }
    
    // 倒计时
    private RectTransform countDownTime;
    public RectTransform CountDownTime
    {
        get
        {
            if (ReferenceEquals(countDownTime,null))
            {
                countDownTime = UnityHelper.FindTheChild(gameObject, "CountDownTime").GetComponent<RectTransform>();
            }
            return countDownTime;
        }
    }
    
    // 倒计时条
    private Image countDownTimeFront;
    public Image CountDownTimeFront
    {
        get
        {
            if (ReferenceEquals(countDownTimeFront,null))
            {
                countDownTimeFront = UnityHelper.FindTheChild(CountDownTime.gameObject, "CountDownTimeFront").GetComponent<Image>();
            }
            return countDownTimeFront;
        }
    }
    
    // 倒计时本文
    private TextMeshProUGUI countDownTimeText;
    public TextMeshProUGUI CountDownTimeText
    {
        get
        {
            if (ReferenceEquals(countDownTimeText,null))
            {
                countDownTimeText = UnityHelper.FindTheChild(CountDownTime.gameObject, "CountDownTimeText").GetComponent<TextMeshProUGUI>();
            }
            return countDownTimeText;
        }
    }
    
    #endregion

    #region UI组件Modes

    private RectTransform modes;
    public RectTransform Modes
    {
        get
        {
            if (ReferenceEquals(modes,null))
            {
                modes = UnityHelper.FindTheChild(gameObject, "Modes").GetComponent<RectTransform>();
            }
            return modes;
        }
    }
    
    // 单人模式
    private RectTransform singleMode;
    public RectTransform SingleMode
    {
        get
        {
            if (ReferenceEquals(singleMode,null))
            {
                singleMode = UnityHelper.FindTheChild(Modes.gameObject, "SingleMode").GetComponent<RectTransform>();
            }
            return singleMode;
        }
    }
    
    // 单人模式Spine
    private SkeletonGraphic singleModeSpine;
    public SkeletonGraphic SingleModeSpine
    {
        get
        {
            if (ReferenceEquals(singleModeSpine,null))
            {
                singleModeSpine = UnityHelper.FindTheChild(SingleMode.gameObject, "SingleModeSpine").GetComponent<SkeletonGraphic>();
            }
            return singleModeSpine;
        }
    }
    
    // 联机模式
    private RectTransform onlineMode;
    public RectTransform OnlineMode
    {
        get
        {
            if (ReferenceEquals(onlineMode,null))
            {
                onlineMode = UnityHelper.FindTheChild(Modes.gameObject, "OnlineMode").GetComponent<RectTransform>();
            }
            return onlineMode;
        }
    }
    
    // 联机模式Spine
    private SkeletonGraphic onlineModeSpine;
    public SkeletonGraphic OnlineModeSpine
    {
        get
        {
            if (ReferenceEquals(onlineModeSpine,null))
            {
                onlineModeSpine = UnityHelper.FindTheChild(OnlineMode.gameObject, "OnlineModeSpine").GetComponent<SkeletonGraphic>();
            }
            return onlineModeSpine;
        }
    }
    
    // 抽卡模式
    private RectTransform gachaMode;
    public RectTransform GachaMode
    {
        get
        {
            if (ReferenceEquals(gachaMode,null))
            {
                gachaMode = UnityHelper.FindTheChild(Modes.gameObject, "GachaMode").GetComponent<RectTransform>();
            }
            return gachaMode;
        }
    }
    
    // 抽卡模式Spine
    private SkeletonGraphic gachaModeSpine;
    public SkeletonGraphic GachaModeSpine
    {
        get
        {
            if (ReferenceEquals(gachaModeSpine,null))
            {
                gachaModeSpine = UnityHelper.FindTheChild(GachaMode.gameObject, "GachaModeSpine").GetComponent<SkeletonGraphic>();
            }
            return gachaModeSpine;
        }
    }
    
    private LobbyModule lobbyModule;
    private LobbyModule LobbyModule
    {
        get
        {
            if (null == lobbyModule)
                lobbyModule = ModuleManager.Instance.Get<LobbyModule>();
            return lobbyModule;
        }
    }
    #endregion

    #region UI组件Dungeon

    private RectTransform dungeon;
    public RectTransform Dungeon
    {
        get
        {
            if (ReferenceEquals(dungeon, null))
                dungeon = UnityHelper.GetTheChildComponent<RectTransform>(gameObject, "Dungeon");
            return dungeon;
        }
    }

    private RectTransform gateOfTrials;
    public RectTransform GateOfTrials
    {
        get
        {
            if (ReferenceEquals(gateOfTrials, null))
                gateOfTrials = UnityHelper.GetTheChildComponent<RectTransform>(Dungeon.gameObject, "GateOfTrials");
            return gateOfTrials;
        }
    }

    private RectTransform towerOfSurvival;
    public RectTransform TowerOfSurvival
    {
        get
        {
            if (ReferenceEquals(towerOfSurvival, null))
                towerOfSurvival = UnityHelper.GetTheChildComponent<RectTransform>(Dungeon.gameObject, "TowerOfSurvival");
            return towerOfSurvival;
        }
    }

    private RectTransform towerOfCoin;
    public RectTransform TowerOfCoin
    {
        get
        {
            if (ReferenceEquals(towerOfCoin, null))
                towerOfCoin = UnityHelper.GetTheChildComponent<RectTransform>(Dungeon.gameObject, "TowerOfCoin");
            return towerOfCoin;
        }
    }

    private RectTransform toBeDetermined;
    public RectTransform ToBeDetermined
    {
        get
        {
            if (ReferenceEquals(toBeDetermined, null))
                toBeDetermined = UnityHelper.GetTheChildComponent<RectTransform>(Dungeon.gameObject, "ToBeDetermined");
            return toBeDetermined;
        }
    }
    
    private ParticleSystem dungeonSelectEffect;
    public ParticleSystem DungeonSelectEffect
    {
        get
        {
            if (ReferenceEquals(dungeonSelectEffect, null))
                dungeonSelectEffect = UnityHelper.GetTheChildComponent<ParticleSystem>(Dungeon.gameObject, "DungeonSelectEffect");
            return dungeonSelectEffect;
        }
    }

    private RectTransform dungeonLeftIcon;
    public RectTransform DungeonLeftIcon
    {
        get
        {
            if (ReferenceEquals(dungeonLeftIcon, null))
                dungeonLeftIcon = UnityHelper.GetTheChildComponent<RectTransform>(Dungeon.gameObject, "LeftIcon");
            return dungeonLeftIcon;
        }
    }
    
    private RectTransform dungeonRightIcon;
    public RectTransform DungeonRightIcon
    {
        get
        {
            if (ReferenceEquals(dungeonRightIcon, null))
                dungeonRightIcon = UnityHelper.GetTheChildComponent<RectTransform>(Dungeon.gameObject, "RightIcon");
            return dungeonRightIcon;
        }
    }

    #endregion

    #region UI组件Levels

    private RectTransform levels;
    public RectTransform Levels
    {
        get
        {
            if (ReferenceEquals(levels, null))
                levels = UnityHelper.GetTheChildComponent<RectTransform>(gameObject, "Levels");
            return levels;
        }
    }

    private RectTransform levelLeftIcon;
    public RectTransform LevelLeftIcon
    {
        get
        {
            if (ReferenceEquals(levelLeftIcon, null))
                levelLeftIcon = UnityHelper.GetTheChildComponent<RectTransform>(Levels.gameObject, "LeftIcon");
            return levelLeftIcon;
        }
    }
    
    private RectTransform levelRightIcon;
    public RectTransform LevelRightIcon
    {
        get
        {
            if (ReferenceEquals(levelRightIcon, null))
                levelRightIcon = UnityHelper.GetTheChildComponent<RectTransform>(Levels.gameObject, "RightIcon");
            return levelRightIcon;
        }
    }

    private RectTransform levelSelects;
    public RectTransform LevelSelects
    {
        get
        {
            if (ReferenceEquals(levelSelects, null))
                levelSelects = UnityHelper.GetTheChildComponent<RectTransform>(Levels.gameObject, "LevelSelects");
            return levelSelects;
        }
    }

    #endregion

    #region  变量
    
    // 用户昵称
    private string _userNickName;
    // 用户战斗力
    private int _userCombatPowerCount;
    
    // 倒计时剩余时间
    private int _leftTime;
    // 倒计时初始时间
    private int _initialTime = 60;
    // 倒计时协程
    Coroutine countDownCoroutine;
    // 自动选择
    private bool _autoSelect;
    
    // 大厅显示枚举
    public enum EnumLobbyDisplay
    {
        None,
        Modes,
        Dungeon,
        Levels,
        Max
    }
    // 大厅当前显示
    EnumLobbyDisplay lobbyDisplay = EnumLobbyDisplay.Modes;
    // 模式索引
    List<int> ModeIndex = new List<int>()
    {
        0,
        1, 
        2, 
        3
    };
    // 模式位置
    List<Vector3> ModePosition = new List<Vector3>()
    {
        Vector3.zero,
        new Vector3(0,-930,0), 
        new Vector3(-720,-711,0),
        new Vector3(760,-711,0),
    };
    // 模式缩放
    List<Vector3> ModeScale = new List<Vector3>()
    {
        Vector3.zero,
        Vector3.one, 
        new Vector3(0.7f,0.7f,0.7f),
        new Vector3(0.7f,0.7f,0.7f),
    };
    // 模式SiblingIndex加
    List<int> ModeSiblingIndexAdd = new List<int>()
    {
        0,
        2, 
        1, 
        0
    };
    // 模式SiblingIndex减
    List<int> ModeSiblingIndexSub = new List<int>()
    {
        0,
        2, 
        0, 
        1
    };
    // 模式RectTransform
    private List<RectTransform> ModeRectTransform = new List<RectTransform>(4);
    // 模式SkeletonGraphic
    private List<SkeletonGraphic> ModeSkeletonGraphic = new List<SkeletonGraphic>(4);
    // 模式动画序列
    private List<Sequence> ModeSequences = new List<Sequence>();
    // 模式移动时间
    private float ModeMoveTime = 0.5f;
    // 副本Rect列表
    private List<RectTransform> _dungeonRects = new List<RectTransform>();
    // 副本选择灰色遮罩颜色
    private Color32 _dungeonGrayColor = new Color32(155, 155, 155, 255);
    // 副本选择间隔
    private float _dungeonSelectInterval = 730f;
    // 副本枚举
    public enum EnumDungeon
    {
        None,
        GateOfTrials,
        TowerOfSurvival,
        TowerOfCoin,
        ToBeDetermined,
        Max
    }
    // 当前副本
    EnumDungeon currentDungeon = EnumDungeon.GateOfTrials;
    // 副本选择特效协程
    Coroutine dungeonSelectEffectCoroutine;
    // 关卡索引
    private int _levelIndex = 0;
    // 关卡Rect列表
    private List<RectTransform> _levelRects = new List<RectTransform>();
    // 关卡选择间隔
    private float _levelSelectInterval = 2160f;
    
    Coroutine matchingCoroutine;
    int matchingTime = 0;
    
    #endregion
    
    protected override void OnStart()
    {
        ModeRectTransform.Add(null); // 索引0占位
        ModeRectTransform.Add(SingleMode);
        ModeRectTransform.Add(GachaMode);
        ModeRectTransform.Add(OnlineMode);
        
        ModeSkeletonGraphic.Add(null);
        ModeSkeletonGraphic.Add(SingleModeSpine);
        ModeSkeletonGraphic.Add(GachaModeSpine);
        ModeSkeletonGraphic.Add(OnlineModeSpine);
        
        _dungeonRects.Add(null);
        _dungeonRects.Add(GateOfTrials);
        _dungeonRects.Add(TowerOfSurvival);
        _dungeonRects.Add(TowerOfCoin);
        _dungeonRects.Add(ToBeDetermined);
        
#if UNITY_WEBGL && !UNITY_EDITOR
        BindWebGLPointerInteractions();
#endif
        MessageListener();
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL/微信小游戏没有键盘作为默认输入，补齐大厅卡片的触摸/鼠标点击入口。
    private void BindWebGLPointerInteractions()
    {
        BindPointer(SingleMode, () => { ModeIndex[1] = 1; ModesOnSelect(); });
        BindPointer(GachaMode, () => { ModeIndex[1] = 2; ModesOnSelect(); });
        BindPointer(OnlineMode, () => { ModeIndex[1] = 3; ModesOnSelect(); });
        BindPointer(GateOfTrials, () => { currentDungeon = EnumDungeon.GateOfTrials; DungeonOnSelect(); });
    }

    private static void BindPointer(Component target, System.Action action)
    {
        if (target == null) return;
        EventTriggerListener.Get(target.gameObject).SetEventHandle(
            EnumTouchEventType.OnClick, (listener, eventData, args) => action());
    }
#endif

    private void MessageListener()
    {
        //添加事件监听
        MessageCenter.Instance.AddListener(MsgType.ROOM_PLAYER_MATCHING, isMatching);
        MessageCenter.Instance.AddListener(MsgType.ROOM_ALL_PLAYER_SET_READY, hasMatched);
        MessageCenter.Instance.AddListener(MsgType.NET_GET_HERO_INFO, GetHeroInfo);
        
        // 发送请求
#if UNITY_WEBGL && !UNITY_EDITOR
        _userCombatPowerCount = 0;
        UserCombatPower.text = _userCombatPowerCount.ToString();
        Debug.Log("[WebGL 大厅演示] 使用本地英雄战力占位，不请求服务端英雄数据。");
#else
        NetController.Instance.SendGetHeroReq(MsgType.NET_GET_HERO_INFO);
#endif

        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(0.5f);
        GetUserNickName();
        GenerateLevels();
#if UNITY_WEBGL && !UNITY_EDITOR
        for (int i = 0; i < _levelRects.Count; i++)
        {
            var level = _levelRects[i];
            var index = i;
            BindPointer(level, () => { _levelIndex = index; LevelOnSelect(); });
        }
#endif
        DungeonChangeIndex(0);
        AudioManager.Instance.PlayMusic(SysDefines.AUDIO + "BGM/" + "bgm_Home");
        countDownCoroutine = StartCoroutine(CountDown());
    }
    
    protected override void OnRelease()
    {
        //移除事件监听
        MessageCenter.Instance.RemoveListener(MsgType.ROOM_PLAYER_MATCHING, isMatching);
        MessageCenter.Instance.RemoveListener(MsgType.ROOM_ALL_PLAYER_SET_READY, hasMatched);
        MessageCenter.Instance.RemoveListener(MsgType.NET_GET_HERO_INFO, GetHeroInfo);
    }
    
    protected override void OnUpdate(float deltaTime)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ModuleManager.Instance.Get<LoginModule>().AccessServiceReq(SysDefines.GroupId_Fish, EnumAccessServiceType.JoinGame, gameData =>
            {
                NetController.Instance.SendEnterSiteReq(SysDefines.SiteId);
            });
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ModuleManager.Instance.Get<LoginModule>()
                .AccessServiceReq(SysDefines.GroupId_Fish, EnumAccessServiceType.QuitGame, null);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            NetController.Instance.SendExitGameReq(() =>
            {
                NetController.Instance.SendExitSiteReq(SysDefines.SiteId);
            });
        }

        InputOperation();
    }
    
    /// <summary>
    /// 输入操作
    /// </summary>
    private void InputOperation()
    {
        var inputAdd = Input.GetKeyDown(KeyCode.D);
        var inputSub = Input.GetKeyDown(KeyCode.A);
        var inputSelect = Input.GetKeyDown(KeyCode.Space) || _autoSelect;
        
        if (inputSelect)
        {
            _autoSelect = false;
            if (countDownCoroutine != null) StopCoroutine(countDownCoroutine);
            countDownCoroutine = StartCoroutine(CountDown());
        }
        
        switch (lobbyDisplay)
        {
            case EnumLobbyDisplay.Modes:
                if (inputAdd) ModesChangeIndex(1);
                if (inputSub) ModesChangeIndex(-1);
                if (inputSelect) ModesOnSelect();
                break;
            case EnumLobbyDisplay.Dungeon:
                if (inputAdd) DungeonChangeIndex(1);
                if (inputSub) DungeonChangeIndex(-1);
                if (inputSelect) DungeonOnSelect();
                break;
            case EnumLobbyDisplay.Levels:
                if (inputAdd) LevelChangeIndex(1);
                if (inputSub) LevelChangeIndex(-1);
                if (inputSelect) LevelOnSelect();
                break;
        }

        
    } 
    
    /// <summary>
    /// 模式选择
    /// </summary>
    private void ModesOnSelect()
    {
        switch (ModeIndex[1])
        {
            case 1:
                ChangeLobbyDisplay(EnumLobbyDisplay.Dungeon);
                // ModeSelectionTool.Instance.mode = 1;
                // UIManager.Instance.OpenUICloseOthers(EnumUIType.LoadingUI, true, EnumUIType.MarbleUI, EnumSceneType.FishScene);
                break;
            case 2:
                ModeSelectionTool.Instance.mode = 2;
                // UIManager.Instance.OpenUICloseOthers(EnumUIType.LoadingUI, true, EnumUIType.MarbleUI, EnumSceneType.FishScene);
                break;
            case 3:
                break;
        }
    }

    /// <summary>
    /// 模式切换
    /// </summary>
    private void ModesChangeIndex(int num)
    {
        // 打断非选中状态的动画并重置scale
        for (int i = 1; i < ModeIndex.Count; i++)
        {
            int newIndex = ModeIndex[i] + num;
            if (newIndex > 3) newIndex = 1;
            else if (newIndex < 1) newIndex = 3;

            if (newIndex != 1 && i < ModeSequences.Count && ModeSequences[i] != null)
            {
                ModeSequences[i].Kill();
                ModeSequences[i] = null;
                ModeRectTransform[i].DOScale(ModeScale[newIndex], 0f);
            }
        }

        for (int i = 1; i < ModeIndex.Count; i++)
        {
            ModeIndex[i] += num;
            if (ModeIndex[i] > 3)
            {
                ModeIndex[i] = 1;
            }
            else if (ModeIndex[i] < 1)
            {
                ModeIndex[i] = 3;
            }
            ModeRectTransform[i].DOLocalMove(ModePosition[ModeIndex[i]], ModeMoveTime);
            var i1 = i;

            // 打断旧的Sequence
            if (i < ModeSequences.Count && ModeSequences[i] != null)
            {
                ModeSequences[i].Kill();
            }

            Sequence seq = DOTween.Sequence();
            seq.Append(ModeRectTransform[i].DOScale(ModeScale[ModeIndex[i]], ModeMoveTime));
            if (ModeIndex[i] == 1)
            {
                seq.Append(ModeRectTransform[i1].DOScale(1.2f, 0.15f));
                seq.Append(ModeRectTransform[i1].DOScale(Vector3.one, 0.25f));
            }
            seq.Play();

            if (i < ModeSequences.Count)
                ModeSequences[i] = seq;
            else
                ModeSequences.Add(seq);

            ModeSkeletonGraphic[i].gameObject.SetActive(ModeIndex[i] == 1);
            if (num > 0)
            {
                ModeRectTransform[i].SetSiblingIndex(ModeSiblingIndexAdd[ModeIndex[i]]);
            }
            else if (num < 0)
            {
                ModeRectTransform[i].SetSiblingIndex(ModeSiblingIndexSub[ModeIndex[i]]);
            }   
            
        }
    }
    
    /// <summary>
    /// 副本选择
    /// </summary>
    private void DungeonOnSelect()
    {
        switch (currentDungeon)
        {
            case EnumDungeon.GateOfTrials:
                ChangeLobbyDisplay(EnumLobbyDisplay.Levels);
                // ModeSelectionTool.Instance.mode = 1;
                // UIManager.Instance.OpenUICloseOthers(EnumUIType.LoadingUI, true, EnumUIType.MarbleUI, EnumSceneType.FishScene);
                break;
            case EnumDungeon.TowerOfSurvival:
                break;
            case EnumDungeon.TowerOfCoin:
                break;
            case EnumDungeon.ToBeDetermined:
                break;
        }
    }
    
    /// <summary>
    /// 副本切换
    /// </summary>
    private void DungeonChangeIndex(int num)
    {
        int currentIndex = (int)currentDungeon;
        currentIndex += num;
        if ((EnumDungeon)currentIndex is EnumDungeon.None or EnumDungeon.Max) return;
        currentDungeon = (EnumDungeon)currentIndex;
        if (currentIndex == 1) DungeonLeftIcon.gameObject.SetActive(false);
        if (currentIndex  > 1) DungeonLeftIcon.gameObject.SetActive(true);
        if (currentDungeon == EnumDungeon.Max - 1) DungeonRightIcon.gameObject.SetActive(false);
        if (currentDungeon < EnumDungeon.Max - 1) DungeonRightIcon.gameObject.SetActive(true);
        DungeonSelectEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        for (int i = 1; i < _dungeonRects.Count; i++)
        {
            float targetX = ((int)(EnumDungeon)i - (int)currentDungeon) * _dungeonSelectInterval;
            float targetScale = 1;
            Color targetColor = Color.white;
            _dungeonRects[i].DOLocalMoveX(targetX, 0.5f).SetEase(Ease.OutQuad);
            if (i == currentIndex)
            {
                targetScale = 1f;
                targetColor = Color.white;
                _dungeonRects[i].Find("Spine").GetComponent<SkeletonGraphic>().AnimationState.TimeScale = 1;
            }
            else
            {
                targetScale = 0.7f;
                targetColor = _dungeonGrayColor;
                _dungeonRects[i].Find("Spine").GetComponent<SkeletonGraphic>().AnimationState.TimeScale = 0;
            }
            _dungeonRects[i].DOScale(targetScale, 0.5f).SetEase(Ease.OutQuad);
            _dungeonRects[i].GetComponent<Image>().DOColor(targetColor, 0.5f).SetEase(Ease.OutQuad);
        }
        if (dungeonSelectEffectCoroutine != null)
        {
            StopCoroutine(dungeonSelectEffectCoroutine);
        }
        dungeonSelectEffectCoroutine = StartCoroutine(PlayDungeonSelectEffect());
    }

    IEnumerator PlayDungeonSelectEffect()
    {
        yield return new WaitForSeconds(0.5f);
        DungeonSelectEffect.Play();
    }
    
    /// <summary>
    /// 关卡选择
    /// </summary>
    private void LevelOnSelect()
    {
        ModeSelectionTool.Instance.mode = 1;
        UIManager.Instance.OpenUICloseOthers(EnumUIType.LoadingUI, true, EnumUIType.MarbleUI, EnumSceneType.FishScene);
    }
    
    /// <summary>
    /// 关卡切换
    /// </summary>
    private void LevelChangeIndex(int num)
    {
        int currentIndex = _levelIndex;
        currentIndex += num;
        if (currentIndex < 0 || currentIndex >= _levelRects.Count) return;
        _levelIndex = currentIndex;
        if (_levelIndex == 0) LevelLeftIcon.gameObject.SetActive(false);
        if (_levelIndex > 0) LevelLeftIcon.gameObject.SetActive(true);
        if (_levelIndex == _levelRects.Count - 1) LevelRightIcon.gameObject.SetActive(false);
        if (_levelIndex < _levelRects.Count - 1) LevelRightIcon.gameObject.SetActive(true);
        for (int i = 0; i < _levelRects.Count; i++)
        {
            _levelRects[i].DOLocalMoveX(_levelSelectInterval * (i - currentIndex), 0.5f).SetEase(Ease.OutQuad);
        }
    }
    
    /// <summary>
    /// 生成关卡
    /// </summary>
    private void GenerateLevels()
    {
        float posX = 0;
        foreach (var t in TMapCopyHelper.DataMap)
        {
            if (t.Value.SceneType != 1) continue;
            GameObject level = ObjectPoolManager.Instance.Spawn(ResManager.Instance.LoadPrefab(SysDefines.LEVELSELECT + "LevelSelect"), LevelSelects.transform);
            _levelRects.Add(level.GetComponent<RectTransform>());
            level.transform.localPosition = new Vector3(posX, 0, 0);
            posX += _levelSelectInterval;
            level.name = t.Key.ToString();
            Image levelIcon = UnityHelper.FindTheChild(level, "LevelIcon").GetComponent<Image>();
            Image levelName = UnityHelper.FindTheChild(level, "LevelName").GetComponent<Image>();
            SkeletonGraphic levelSpine = UnityHelper.FindTheChild(level, "LevelSpine").GetComponent<SkeletonGraphic>();
            RectTransform stars = UnityHelper.FindTheChild(level, "Stars").GetComponent<RectTransform>();
            levelIcon.sprite = ResManager.Instance.LoadSprite(SysDefines.LEVELSELECT + t.Key + "Icon");
            levelName.sprite = ResManager.Instance.LoadSprite(SysDefines.LEVELSELECT + t.Key + "Name");
            levelSpine.skeletonDataAsset = ResManager.Instance.LoadSpineData(SysDefines.SPINES + "LevelSelect/" + t.Key + $"/{t.Key}_SkeletonData");
            levelSpine.gameObject.SetActive(true);
            for (int i = 0; i < t.Value.StarDifficulty; i++)
            {
                stars.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    private void isMatching(Message msg)
    {
    }

    private void hasMatched(Message msg)
    {
    }
    
    
    /// <summary>
    /// 切换大厅显示
    /// </summary>
    private void ChangeLobbyDisplay(EnumLobbyDisplay display)
    {
        EnumLobbyDisplay currentDisplay = lobbyDisplay;
        switch (currentDisplay)
        {
            case EnumLobbyDisplay.Modes:
                Modes.DOScale(0, 0.5f).SetEase(Ease.OutQuad);
                Modes.DOLocalMoveX(-2160, 0.5f).SetEase(Ease.OutQuad)
                    .OnComplete(() => Modes.gameObject.SetActive(false));
                break;
            case EnumLobbyDisplay.Dungeon:
                Dungeon.DOScale(0, 0.5f).SetEase(Ease.OutQuad);
                Dungeon.DOLocalMoveX(-2160, 0.5f).SetEase(Ease.OutQuad)
                    .OnComplete(() => Dungeon.gameObject.SetActive(false));
                break;
            case EnumLobbyDisplay.Levels:
                Levels.DOScale(0, 0.5f).SetEase(Ease.OutQuad);
                Levels.DOLocalMoveX(-2160, 0.5f).SetEase(Ease.OutQuad)
                    .OnComplete(() => Levels.gameObject.SetActive(false));
                break;
        }
        
        lobbyDisplay = display;
        switch (display)
        {
            case EnumLobbyDisplay.Modes:
                Modes.gameObject.SetActive(true);
                Modes.DOScale(1, 0.5f).SetEase(Ease.OutQuad);
                Modes.DOLocalMoveX(0, 0.5f).SetEase(Ease.OutQuad);
                RotateSelection.DOLocalMoveX(0, 0.25f).SetEase(Ease.OutQuad);
                break;
            case EnumLobbyDisplay.Dungeon:
                Dungeon.gameObject.SetActive(true);
                Dungeon.DOScale(1, 0.5f).SetEase(Ease.OutQuad);
                Dungeon.DOLocalMoveX(0, 0.5f).SetEase(Ease.OutQuad);
                RotateSelection.DOLocalMoveX(-900, 0.25f).SetEase(Ease.OutQuad);
                break;
            case EnumLobbyDisplay.Levels:
                Levels.gameObject.SetActive(true);
                Levels.DOScale(1, 0.5f).SetEase(Ease.OutQuad);
                Levels.DOLocalMoveX(0, 0.5f).SetEase(Ease.OutQuad);
                RotateSelection.DOLocalMoveX(-900, 0.25f).SetEase(Ease.OutQuad);
                break;
        }
    }
    
    /// <summary>
    /// 获取英雄信息
    /// </summary>
    private void GetHeroInfo(Message msg)
    {
        var rsp = msg.Content as CLPFGetHeroAck;
        int fight = 0;
        if (rsp.hero_teams.Length > 0)
        {
            fight = (int)rsp.hero_teams[0].fight;
        }
        _userCombatPowerCount = fight;
        UserCombatPower.text = _userCombatPowerCount.ToString();
    }
    
    /// <summary>
    /// 获取用户昵称
    /// </summary>
    private void GetUserNickName()
    {
        _userNickName = SysDefines.UserNickName;
        UserNickName.text = _userNickName;
    }
    
    
    
    /// <summary>
    /// 倒计时
    /// </summary>
    private IEnumerator CountDown()
    {
        _leftTime = _initialTime;
        while (true)
        {
            CountDownTimeText.text = _leftTime.ToString();
            CountDownTimeFront.DOFillAmount(0, 1f);
            yield return new WaitForSeconds(1f);
            _leftTime--;
            CountDownTimeFront.fillAmount = 1;
            if (_leftTime <= 0)
            {
                _autoSelect = true;
                break;
            }
        }
    }
}
