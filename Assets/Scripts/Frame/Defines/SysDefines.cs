
/******************************************************************************
 * 
 *  Title:  捕鱼项目
 *
 *  Version:  1.0版
 *
 *  Description:
 *    1：系统常量
 *    2：全局枚举对象
 *    3：全局委托
 *
 *  Author:  WangXingXing
 *       
 *  Date:  2018
 * 
 ******************************************************************************/

using System;

using UnityEngine;

//全局委托
public delegate void StateChangeEvent(object sender, EnumObjectState newState, EnumObjectState oldState);
public delegate void MessageEvent(Message msg);
public delegate void OnTouchEventHandle(GameObject listener, object eventData, params object[] args);
public delegate void MethodAction(object args);

// 签到操作类型（区分日常签到和补签）
public enum SignOperationType
{
    DailyCheckIn,  // 日常签到
    SupplementSign // 补签
}

/// <summary>
/// 表示操作结果的枚举（成功/失败）
/// </summary>
public enum ResultStatus
{
    /// <summary>
    /// 操作失败
    /// </summary>
    Failure,

    /// <summary>
    /// 操作成功
    /// </summary>
    Success
}

/// <summary>
/// 选择时间
/// </summary>
public enum TimeUnits
{
    Second=1,
    HalfMinute=30,
    OneMinute=60,
    TenMinutes=600,
    OneHour=3600,
    OneDay=3600*24
}

/// <summary>
/// 通用状态枚举，表示开启和关闭状态
/// </summary>
public enum EnumStripStatus
{
    /// <summary>
    /// 关闭状态
    /// </summary>
    Off=-1,

    /// <summary>
    /// 开启状态
    /// </summary>
    On
}
/// <summary>
/// 通用状态枚举，表示开启和关闭状态
/// </summary>
public enum EnumTaskStatus
{
    /// <summary>
    /// 关闭状态
    /// </summary>
    Off,

    /// <summary>
    /// 开启状态
    /// </summary>
    On
}
/// <summary>
/// 对象当前状态
/// </summary>
public enum EnumObjectState
{
    None,
    Initial,                 //初始化
    Loading,                 //装载中
    Ready,                   //准备结束
    Disabled,                //过去的
    Closing,                 //关闭
}

//ui界面类型
public enum EnumUIType
{
    None = -1,
    #region 旧版游戏界面
    LuaUI,                              //所有热更界面共用
    FishingSelectUI,                    //竞技场渔场选择界面
    FishingCommonUI,                    //普通竞技场界面
    LoadingUI,                          //过度界面
    LoginUI,                            //登录界面
    MainUI,                             //主界面
    MessageBoxUI,                       //消息弹窗界面
    SettingUI,                          //设置界面
    SystemNoticeUI,                     //系统消息
    MessageLeaveFishUI,                 //退出渔场等待界面
    UserAgreementUI,                    //用户协议
    PrivacyGuidelinesUI,                //用户隐私
    HotUpdateUI,                        //热更界面
    PayUI,                              //支付界面
    RegisterUI,                         //账号注册界面
    PhoneBindUI,                        //手机绑定/更换界面
    PayAgentUI,                         //代理支付界面
    PayQRCodeUI,                        //扫码支付界面
    #endregion
    LobbyUI,        //大厅UI
    MarbleUI,       //弹珠游戏主UI
    MultiPlayerUI,  //多人对战UI
    PVPVEUI,        //多人环境对战UI
}

// 效果作用值类型
public enum EnumEffectValueType
{
    None,
    ReduceHpByPercentageOfAttack,       //1  减少百分比攻击力的生命
    ReduceHp,                           //2  减少生命
    ReduceAttackByPercentage,           //3  减少百分比攻击力
    ReduceAttack,                       //4  减少攻击力
    ReduceDefenseByPercentage,          //5  减少百分比防御力
    ReduceDefense,                      //6  减少防御力
    ReduceSpeedByPercentage,            //7  减少百分比速度
    ReduceSpeed,                        //8  减少速度
    ReduceCriticalRate,                 //9  减少暴击率
    ReduceCriticalDamage,               //10 减少暴击伤害
    RestoreHpByPercentage,              //11 恢复百分比生命
    RestoreHp,                          //12 恢复生命
    IncreaseAttackByPercentage,         //13 提升百分比攻击力
    IncreaseAttack,                     //14 提升攻击力
    IncreaseDefenseByPercentage,        //15 提升百分比防御力
    IncreaseDefense,                    //16 提升防御力
    IncreaseSpeedByPercentage,          //17 提升百分比速度
    IncreaseSpeed,                      //18 提升速度
    IncreaseCriticalRate,               //19 提升暴击率
    IncreaseCriticalDamage,             //20 提升暴击伤害
    GainShieldBasedOnPercentageOfMaxHp, //21 获得百分比最大生命的护盾
    GainShield,                         //22 获得护盾
}

// 计算操作类型
public enum EnumCalculateType
{
    None,
    Add,
    Subtract,
    Multiply,
    Divide,
    Equals,
}

// 元素对应ID
public enum EnumElementType
{
    None = 0,
    Fire,
    Wind,
    Earth,
    Water,
    Light,
    Dark,
}


//event事件类型
public enum EnumTouchEventType
{
    OnBeginDrag,
    OnCancel,
    OnDeselect,
    OnDrag,
    OnDrop,
    OnEndDrag,
    OnInitializePotentialDrag,
    OnMove,
    OnClick,
    OnDoubleClick,
    OnDown,
    OnEnter,
    OnExit,
    OnUp,
    OnScroll,
    OnSelect,
    OnSubmit,
    OnUpdateSelected,
}

//场景类型
public enum EnumSceneType
{
    None = 0,
    LoginScene,
    LoadingScene,
    MainScene,
    FishScene,
    MultiPlayerScene,
    PVPVEScene,
}

//UI打开效果
public enum EnumAnimationType
{
    None,
    Scale,
}

//服务器连接状态
public enum EnumNetConnectState
{
    None = 0,
    Error,
    Established,
    Disconnect,
}

//弹窗类型
public enum EnumMessageBoxType
{
    OK,
    OK_CANCEL,
}

//鱼的类型
public enum EnumFishType
{
    None = 0,
    Small,                   //小
    Midle,                   //中
    Big,                     //大
    Special,                 //特殊
    Gold,                    //金
    Boss,                    //Boss
    Max,
}

//鱼的附属类型
public enum EnumFishSecType
{
    None = 0,
    Combine,                //组合鱼
    King,                   //鱼王
}

//鱼组件类型
public enum EnumFishScriptType
{
    None = 0,
    Normal,                 //普通鱼
    WorldBoss,              //世界Boss
}

public enum EnumFishPicType
{
    None = 0,
    Common,                  //普通
    Bonus,                   //赏金
    Special,                 //特殊
    Max,                     //最大
}

public enum EnumAccessServiceType
{
    None = 0,
    JoinGame,                //加入游戏
    QuitGame,                //退出游戏
}

//item的效果类型
public enum EnumItemEffectType
{
    None = 0,
    Lock,                    //锁定
    Frozen,                  //冰冻
    Summon,                  //召唤葫芦
    Rage,                    //狂暴卡
    Cloned,                  //分身卡
    Bomb,                    //弹头
    Muiltiple,              //倍击
}

//item的大类型分类
public enum EnumItemType
{
    None = -1,
    Platform,                //平台
    Fish,                    //捕鱼
}

//游戏玩法
public enum EnumSiteType
{
    None = 0,
    Fishing,
}

//捕鱼玩法的类型
public enum EnumFishingMatchType
{
    None = -1,
    Common,
    FreeMatch,
    RewardMatch,
}

//捕鱼玩法的房间类型
public enum EnumFishingRoomType
{
    None = 0,
    Common,
    FreeMatch,
    RewardMatch,
    Energy,
}

//按钮点击可变参数的键值类型
public enum EnumHashtableParamsType
{
    None = 0,
    Audio,
    LockAllClick,
    LockSelfClick,
}

public enum EnumNoticeType
{
    KillGetGold = 1,         //击杀得金币
    KillGetBomb,             //击杀得弹头
    DrawGetBomb,             //抽奖得弹头
}

//鱼死亡原因
public enum EnumCreateBombType
{
    CommonFish,              //普通鱼
    SlotMachineFish,         //老虎机鱼
    TurntableFish,           //转盘鱼
    HornBossFish,           //号角boss
    SlotMachineFish1,         //老虎机鱼
}

//爆金效果
public enum EnumFishEffect
{
    Common = 0,              //普通
    Roulette,                //轮盘
    Rich,                    //发财了
    Zillionaire,             //大富翁
    Dead
}

/// <summary>
/// 玩家性别
/// </summary>
public enum EnumPlayerGenderType
{
    Unknown = 0,              //保密
    Male,                     //男
    Female,                   //女
}

//鱼死亡后需处理的效果
public enum EnumFishDieEvent
{
    None = -1,
    Die,                    //鱼死亡特效
    GoldPop,                //金币(积分)和数字弹出
    Bonus,                  //爆金
}

public enum EnumBranchEvent
{
    Free,                   //免费
    Deficient,              //不足（通用）
    Adequate,               //充足

    SupplementSignDeficient //补签次数不足（特定场景）
}
public class UIPathDefines
{
    public static string GetPrefabPathByType(EnumUIType uiType, string componentType)
    {
        var prefabName = string.IsNullOrEmpty(componentType) ? uiType.ToString() : componentType;

        string temp = string.Empty;
        if (uiType == EnumUIType.LoginUI || uiType == EnumUIType.LoadingUI || uiType == EnumUIType.MainUI || uiType == EnumUIType.SystemNoticeUI || uiType == EnumUIType.FishingSelectUI)
            temp = SysDefines.LOADING;
        // else if (uiType == EnumUIType.CurrencyRefundBonusUI)
        //     temp = SysDefines.UIPREFAB1;
        else if (uiType == EnumUIType.MarbleUI || uiType == EnumUIType.PVPVEUI || uiType == EnumUIType.LobbyUI)
            temp = SysDefines.BNUIPREFAB;
        else
            temp = SysDefines.UIPREFAB;

        var path = string.Format($"{temp}{prefabName}");
        var msg = string.Empty;
        if (uiType == EnumUIType.None)
            msg = string.Format($"没有该类型的预制:{uiType.ToString()}");
        if (!string.IsNullOrEmpty(msg))
            Debug.LogWarning(msg);
        return path;
    }

    public static Type GetUIScriptByType(EnumUIType uiType, string componentType)
    {
        var msg = string.Empty;
        var scriptType = Type.GetType(string.IsNullOrEmpty(componentType) ? uiType.ToString() : componentType);
        if (uiType == EnumUIType.None)
            msg = string.Format($"没有该类型对应的脚本:{uiType.ToString()}");
        if (!string.IsNullOrEmpty(msg))
            Debug.LogWarning(msg);
        return scriptType;
    }
}

public class SysDefines
{
    
    #region 只读变量

    //UI预设
    public const string UIPREFAB = "UIPrefab/";

    public const string LOADING = "Loading/";

    public const string BNUIPREFAB = "BNRes/UI/";
    //区服ID
    public const int ZoneId = 1;
    //短信渠道
    public const int SmsChannel = 0;
    //产品版本号
    public const uint Version = 7;
    //昵称的默认的字符长度
    public const int NickNameLength = 12;
    //积分显示权重
    public const long ScoreViewPara = 10000;
    //大厅的游戏ID(热更新)
    public const int GameID_Hall = 1001;
    //捕鱼服务组Id
    public const int GroupId_Fish = 1;
    //小游戏读取线上配置表
    public const bool IsInnerGameLoadFromNet = true;
    //CA3加密固定密钥
    public const int CA3Key = 19357;
    //手机短信验证间隔
    public const int VerifyWait = 60;
    
    // 弹珠资源路径
    // 角色半身像框
    public const string PORTRAITFRAME = "BNRes/Textures/PortraitFrame/";
    // 角色半身像亮光
    public const string PORTRAITLIGHT = "BNRes/Textures/PortraitLight/";
    // 角色半身像
    public const string PORTRAIT = "BNRes/Textures/Portrait/";
    // 角色元素图标
    public const string ELEMENTICON = "BNRes/Textures/ElementIcon/";
    // 角色能量条
    public const string MARBLEENERGYBAR = "BNRes/Textures/MarbleEnergyBar/";
    // 角色品质图标
    public const string QUALITYICON = "BNRes/Textures/QualityIcon/";
    // Boss血条
    public const string BOSSHEALTHBAR = "BNRes/Textures/BossBar/";
    // 关卡选择
    public const string LEVELSELECT = "BNRes/Textures/LevelSelectUI/";
    // 场景贴图
    public const string SCENETEX = "BNRes/Textures/SceneTex/";
    // 结算界面
    public const string SETTLEMENT = "BNRes/Textures/SettlementUI/";
    // 战斗半身像Spine
    public const string BATTLEPORTRAIT = "BNRes/Spines/BattlePortrait/";
    // 敌人Spine
    public const string ENEMYS = "BNRes/Spines/Enemys/";
    
    // Tilemap路径
    public const string TILEMAP = "BNRes/Tilemap/";

    public const string HOTUPDATEPATH = "Assets/Resources_HotUpdate/";

    #endregion

    #region 静态变量
    //渠道ID
    public static int ChannelId = 0;
    //用户昵称
    public static string UserNickName;
    //射线获得指定的层级
    public static int FishLayer = 1 << LayerMask.NameToLayer("Fish");
    //是否检测过版本信息
    public static bool IsCheckVersion = false;
    public static string Ip = string.Empty;
    public static long Port;
    public static string OssUrl = string.Empty;
    public static string HotUpdateUrl = string.Empty;
    public static string PopularizeUrl = string.Empty;
    public static string OpeninstallToken = string.Empty;
    // 运行平台 1:IOS 2:ANDRIOD 3:WINDOWS 4:LINUX 5:MAC
    public static uint Platform;
    //登录方式 1游客 2三方平台 3QQ 4微信 5Facebook 6GooglePlay 7GameCenter
    public static byte LoginType = 0;
    //登录标识
    public static string LoginToken;
    //本次进入游戏是否首次登录      0是1否
    public static int FirstLogin = 0;
    //本次进入大厅是否首次        0是1否 
    public static int FirstEnterHall = 0;
    //加入玩法
    //public static EnumSiteType SiteId = EnumSiteType.None;
    public static int SiteId = -1;
    //加入房间的ID
    public static int RoomConfigID = 0;
    //当前房间视角    1侧视角 2斜俯
    public static int RoomViewAngle = 1;
    //是否断开连接
    public static bool IsDisconnect;
    //是否在渔场中
    public static bool IsInFishingGame;
    //当前场景
    public static EnumSceneType SceneType = EnumSceneType.None;
    //预加载界面 大小大于1M的都加入预加载
    public static EnumUIType[] preloadUIArray = new EnumUIType[]
    {
		//EnumUIType.LoginUI,
		EnumUIType.FishingSelectUI,
        EnumUIType.MarbleUI,
        EnumUIType.PVPVEUI,
    };

    //加载弹珠的路径
    public const string MARBLES = "BNRes/Marbles/";
    // 加载弹珠泡泡的路径
    public const string MARBLEBUBBLES = "BNRes/MarbleBubbles/";
    // 加载弹珠渲染器的路径
    public const string MARBLERENDERERS = "BNRes/MarbleRenderers/";
    //加载敌方弹珠的路径
    public const string ENEMYMARBLES = "BNRes/EnemyMarbles/";
    //加载特效的路径
    public const string MARBLEEFFECTS = "BNRes/Effects/";
    //加载Spine的路径
    public const string SPINES = "BNRes/Spines/";
    //加载UI的路径
    public const string MARBLEUI = "BNRes/UI/";
    //加载子弹的路径
    public const string BULLETS = "BNRes/Bullets/";
    //加载敌方子弹的路径
    public const string ENEMYBULLETS = "BNRes/EnemyBullets/";
    //加载地图背景预制体的路径
    public const string MAPBACKGROUND = "BNRes/MapBackground/";
    //加载敌人血条预制体的路径
    public const string ENEMYCANVAS = "BNRes/EnemyCanvas/";
    // 音频路径
    public const string AUDIO = "BNRes/Audios/";

    //预先加载的预制体
    public static string[][] preloadPrefab = new string[][]
    {
        //弹珠预制体
        new string[]{ "Marble1", MARBLES + "Marble1" },
        new string[]{ "Marble2", MARBLES + "Marble2" },
        new string[]{ "Marble3", MARBLES + "Marble3" },
        new string[]{ "Marble4", MARBLES + "Marble4" },

        //敌方弹珠预制体
        new string[]{ "EnemyMarble1", ENEMYMARBLES + "EnemyMarble1" },
        new string[]{ "EnemyMarble2", ENEMYMARBLES + "EnemyMarble2" },
        new string[]{ "EnemyMarble3", ENEMYMARBLES + "EnemyMarble3" },
        new string[]{ "EnemyMarble4", ENEMYMARBLES + "EnemyMarble4" },
        new string[]{ "EnemyMarble5", ENEMYMARBLES + "EnemyMarble5" },
        new string[]{ "EnemyMarble6", ENEMYMARBLES + "EnemyMarble6" },
        new string[]{ "EnemyMarble7", ENEMYMARBLES + "EnemyMarble7" },
        new string[]{ "EnemyMarble8", ENEMYMARBLES + "EnemyMarble8" },
        new string[]{ "EnemyMarble9", ENEMYMARBLES + "EnemyMarble9" },
        new string[]{ "EnemyMarble10", ENEMYMARBLES + "EnemyMarble10" },

        //特效预制体
        new string[]{ "HitEffect", MARBLEEFFECTS + "HitEffect" },

        //伤害数字预制体
        new string[]{ "DamageNumbers", MARBLEUI + "DamageNumbers" },

        //子弹预制体
        new string[]{ "Bullet1", BULLETS + "Bullet1" },

        //敌方子弹预制体
        new string[]{ "EnemyBullet1", ENEMYBULLETS + "EnemyBullet1" },

        new string[]{ "Spear", ENEMYBULLETS + "Spear" },
    };
#endregion

    #region 游戏内提示文字
    public const string DiamondInsufficient = "钻石不足";
    public const string QuitGame = "确定要退出游戏？";
    public const string QuitFishScene = "确定离开捕鱼界面";
    public const string QuitMainScene = "确定返回登录界面";
    public const string UnLock = "至少解锁{0}倍炮台才可进入游戏！";
    public const string ShortGold = "金币不足";
    public const string ForgeUnLock = "解锁到{0}倍即可开启锻造功能";
    public const string ForgeEssenceCount = "水晶精华不足";
    public const string SelectGifts = "请选择礼物";
    public const string SelectPlayer = "请填写赠送玩家ID";
    public const string DontGiveGiftsToSelf = "不能赠送礼物给自己";
    public const string GiveGiftsSuccess = "赠送成功";
    public const string PlayerIDError = "ID输入错误";
    public const string NewInvestAddTip = "请先进行投资";
    public const string CheckPlayer = "请先检测赠送玩家是否存在";
    #endregion
}
