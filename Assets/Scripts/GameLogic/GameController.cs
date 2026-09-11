
/******************************************************************************
 * 
 *  Title:  捕鱼项目
 *
 *  Version:  1.0版
 *
 *  Description:
 *
 *  Author:  WangXingXing
 *       
 *  Date:  2018
 * 
 ******************************************************************************/
#define LOCAL_DEBUG

using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using io.openinstall.unity;
using LC.Newtonsoft.Json.Linq;


public class GameController : DDOLSingleton<GameController>
{
    private RectTransform thisT;
    public RectTransform ThisT
    {
        get
        {
            if (ReferenceEquals(thisT, null))
                thisT = transform as RectTransform;
            return thisT;
        }
    }

    private RectTransform uiParent;
    public RectTransform UIParent
    {
        get
        {
            if (ReferenceEquals(uiParent, null))
                uiParent = UnityHelper.GetTheChildComponent<RectTransform>(gameObject, "UIParent");
            return uiParent;
        }
    }

    private RectTransform uiEffect;
    public RectTransform UIEffect
    {
        get
        {
            if (ReferenceEquals(uiEffect, null))
                uiEffect = UnityHelper.GetTheChildComponent<RectTransform>(gameObject, "UIEffect");
            return uiEffect;
        }
    }

    private RectTransform hintMessage;
    public RectTransform HintMessage
    {
        get
        {
            if (ReferenceEquals(hintMessage, null))
                hintMessage = UnityHelper.GetTheChildComponent<RectTransform>(gameObject, "HintMessage");
            return hintMessage;
        }
    }

    private Canvas mainCanvas;
    public Canvas MainCanvas
    {
        get
        {
            if (null == mainCanvas)
                mainCanvas = ThisT.GetComponent<Canvas>();
            return mainCanvas;
        }
    }

    private CanvasScaler mainCanvasScaler;
    public CanvasScaler MainCanvasScaler
    {
        get
        {
            if (null == mainCanvasScaler)
                mainCanvasScaler = ThisT.GetComponent<CanvasScaler>();
            return mainCanvasScaler;
        }
    }

    private Camera mainCamera;
    public Camera MainCamera
    {
        get
        {
            if (null == mainCamera)
                mainCamera = UnityHelper.GetTheChildComponent<Camera>(gameObject, "MainCamera");
            return mainCamera;
        }
    }

    private DOTweenAnimation cameraAnim;
    public DOTweenAnimation CameraAnim
    {
        get
        {
            if (null == cameraAnim)
                cameraAnim = UnityHelper.GetTheChildComponent<DOTweenAnimation>(gameObject, "MainCamera");
            return cameraAnim;
        }
    }

    private CommonModule commonModule;
    private CommonModule CommonModule
    {
        get
        {
            if (null == commonModule)
                commonModule = ModuleManager.Instance.Get<CommonModule>();
            return commonModule;
        }
    }

    private Transform uiLeftMask;
    public Transform UILeftMask
    {
        get
        {
            if (ReferenceEquals(uiLeftMask, null))
                uiLeftMask = UnityHelper.GetTheChildComponent<Transform>(gameObject, "UILeftMask");
            return uiLeftMask;
        }
    }

    //暂时放这
    public GamePlayer Player;
    public GamePlayer Buffer;

    // public RedisPlayer RedisPlayer; 

    public override void Init()
    {
        //Debug.LogError("GameController Init :");

        getRunPlatform();
        Player = null;
        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        ThisT.localPosition = new Vector3(0, 0, MainCanvas.planeDistance);
#if UNITY_IOS
        if(UnityEngine.iOS.Device.generation < UnityEngine.iOS.DeviceGeneration.iPhoneX
            || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPadUnknown
            || UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPodTouchUnknown)
            UILeftMask.gameObject.SetActive(false);
        else
        {
            UILeftMask.gameObject.SetActive(true);
            UIParent.GetComponent<RectTransform>().offsetMin = new Vector2(90, 0);
        }
#else
            UILeftMask.gameObject.SetActive(false);
#endif
    }

    private void Start()
    {
        //Debug.LogError("GameController----");
#if LOCAL_DEBUG
        Debug.unityLogger.logEnabled = true;
#else
        Debug.unityLogger.logEnabled = false;
#endif
        Debug.Log("[WebGL诊断] GameController.Start 开始");
        //m_LastUpdateShowTime = Time.realtimeSinceStartup;
        //TableManager.InitData();

        //onStart();

#if UNITY_IOS && !UNITY_EDITOR
                var openinstall = GameObject.Find("OpenInstall").GetComponent<OpenInstall>();
                openinstall.GetInstall(5, getInstallFinish);
#else
        onStart();
#endif

    }

#if UNITY_WEBGL && !UNITY_EDITOR
    // 微信小游戏当前只验证大厅展示与既有交互；不发起账号、配置或英雄数据网络请求。
    // 数据表仍通过项目的本地 Resources/Table 加载路径提供，后续可替换为正式服务边界。
    private void StartWebGLLobbyPreview()
    {
        try
        {
            Debug.Log("[WebGL诊断] StartWebGLLobbyPreview 开始");
            TableManager.InitData();
            Debug.Log("[WebGL诊断] TableManager.InitData 完成");
            ModuleManager.Instance.RegisterAllModules();
            Debug.Log("[WebGL诊断] ModuleManager.RegisterAllModules 完成");
            SysDefines.UserNickName = "本地演示玩家";
            UIManager.Instance.OpenUICloseOthers(EnumUIType.LobbyUI);
            Debug.Log("[WebGL诊断] LobbyUI 打开调用完成");
            Debug.Log("[WebGL 大厅演示] 已使用本地配置直接打开大厅，未调用登录或服务端。");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[WebGL 大厅演示] 本地大厅初始化失败：{exception}");
        }
    }
#endif

    private void getInstallFinish(OpenInstallData installData)
    {
        string dataStr = "";
        try
        {
            if (!string.IsNullOrEmpty(installData.bindData))
            {
                var json = JObject.Parse(installData.bindData);
                dataStr = $"u={json["u"].ToString()}";
            }
        }
        catch (Exception) { }
        
        SysDefines.OpeninstallToken = dataStr;
        onStart();
    }

    private void onStart()
    {
        //setGameNameIcon();
        //Debug.LogError("onStart");
       //
        //EnumUIType[] UIArray = new EnumUIType[]
        //{
        //    EnumUIType.LoginUI,
        //};

        //UIManager.Instance.PreloadUI(UIArray);

        UIManager.Instance.OpenUI(EnumUIType.HotUpdateUI);

        // Debug.LogError("OpenUI :" + SysDefines.OssUrl);
        NetController.Instance.GetDownloadUrl((md5) =>
        {
           // Debug.LogError("GetDownloadUrl md5:" + md5);
            TableLoadHelper.LoadFromNet(SysDefines.OssUrl, md5, () =>
            {

                ModuleManager.Instance.RegisterAllModules();

              //  Debug.LogError("RegisterAllModules");

                UnityBridgeManager.Instance.Init();

               // Debug.LogError("UnityBridgeManager Init");

                // var hotUpdate = THotUpdateHelper.DataMap.Values.Where(t => t.GameId == SysDefines.GameID_Hall).Last();
                string url = "HotUpdate/Hall/";
                Debug.LogWarning("hotUpdate.Url:" + url);

                SysDefines.HotUpdateUrl = url;
                MessageCenter.Instance.SendMessage(MsgType.CLIENT_JSON_UPDATE_ACK, this, null);

               // Debug.LogError("SendMessage :" + MsgType.CLIENT_JSON_UPDATE_ACK);
            });
        });
    }

    private void Update()
    {
        // 旧版ESC
        if (Input.GetKeyDown(KeyCode.Escape) && false)
        {
            return;
            var curUIType = UIManager.Instance.GetCurrentUI();
            if (curUIType != EnumUIType.None && curUIType != EnumUIType.LoadingUI)
            {
                var curBaseUI = UIManager.Instance.GetUI<BaseUI>(curUIType);
                if (curBaseUI.EscapeClose)
                    UIManager.Instance.CloseUI(curUIType);
                else
                {
                    var sceneName = (EnumSceneType)Enum.Parse(typeof(EnumSceneType), SceneManager.GetActiveScene().name, true);
                    switch (sceneName)
                    {
                        case EnumSceneType.LoginScene:
                            QuitGame();
                            break;
                        case EnumSceneType.LoadingScene:
                            break;
                        case EnumSceneType.MainScene:
                            if (AppRoot.Get().curState == 0)
                                QuitScene(SysDefines.QuitMainScene);
                            else
                                QuitGame();
                            break;
                        case EnumSceneType.FishScene:
                            QuitScene(SysDefines.QuitFishScene);
                            break;
                    }
                }
            }
        }

        //新版ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var curUIType = UIManager.Instance.GetCurrentUI();
            if (curUIType != EnumUIType.None && curUIType != EnumUIType.LoadingUI)
            {
                var sceneName = (EnumSceneType)Enum.Parse(typeof(EnumSceneType), SceneManager.GetActiveScene().name, true);
                switch (sceneName)
                {
                    case EnumSceneType.FishScene:
                        UIManager.Instance.OpenUI(EnumUIType.LoadingUI, true, EnumUIType.LobbyUI,
                            EnumSceneType.MainScene);
                        break;
                    case EnumSceneType.MultiPlayerScene:
                        UIManager.Instance.OpenUI(EnumUIType.LoadingUI, true, EnumUIType.LobbyUI,
                            EnumSceneType.MainScene);
                        break;
                    case EnumSceneType.PVPVEScene:
                        UIManager.Instance.OpenUI(EnumUIType.LoadingUI, true, EnumUIType.LobbyUI,
                            EnumSceneType.MainScene);
                        break;
                }
            }
        }

        if (!ReferenceEquals(CommonModule, null))
            CommonModule.Update();

        //m_FrameUpdate++;
        //if (Time.realtimeSinceStartup - m_LastUpdateShowTime >= m_UpdateShowDeltaTime)
        //{
        //    m_FPS = m_FrameUpdate / (Time.realtimeSinceStartup - m_LastUpdateShowTime);
        //    m_FrameUpdate = 0;
        //    m_LastUpdateShowTime = Time.realtimeSinceStartup;
        //}
    }

#region public function
    //相机震动
    public void ShakeCamera(float duration = 1.0f)
    {
        MainCamera.DOShakePosition(duration, 15.0f, 30, 45.0f);
    }

    /// <summary>
    /// 相机震动
    /// </summary>
    public void BNShakeCamera(float durtion = 0.5f, float strength = 5.0f, bool fadeout = true, int hz = 15)
    {
        // 执行震动动画并在完成后返回原位
        MainCamera.transform.DOShakePosition(
            durtion,       // 持续时间0.5秒
            strength,          // 强度5
            hz,            // 振动频率15
            90f,           // 完全随机方向
            false,         // 不启用snapping
            fadeout        // 淡出效果
        ).OnComplete(() => {
            MainCamera.transform.DOLocalMove(new Vector3(0, 0, -1000), 0.1f);
        });
    }

    //赏金1 高倍随机倍2 弹头鱼3
    public void ShakeCamera(int power)
    {
        switch (power)
        {
            case 1:
                CameraAnim.duration = 0.6f;
                CameraAnim.endValueV3 = Vector3.one * 20;
                CameraAnim.optionalInt0 = 15;
                CameraAnim.DORewind();
                CameraAnim.DOPlay();
                break;
            case 2:
                CameraAnim.duration = 0.8f;
                CameraAnim.endValueV3 = Vector3.one * 30;
                CameraAnim.optionalInt0 = 15;
                CameraAnim.DORewind();
                CameraAnim.DOPlay();
                break;
            case 3:
                CameraAnim.duration = 1f;
                CameraAnim.endValueV3 = Vector3.one * 50;
                CameraAnim.optionalInt0 = 20;
                CameraAnim.DORewind();
                CameraAnim.DOPlay();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 返回到登录界面
    /// </summary>
    public void ReturnToLogin(object agrs = null)
    {
        if (SysDefines.SceneType != EnumSceneType.LoginScene)
            UIManager.Instance.OpenUI(EnumUIType.LoadingUI, true, EnumUIType.LoginUI, EnumSceneType.LoginScene);
    }

    /// <summary>
    /// 离开当前场景
    /// </summary>
    /// <param name="content">提示框内容</param>
    public void QuitScene(string content)
    {
        if (content == SysDefines.QuitFishScene)
            UIManager.Instance.OpenUI(EnumUIType.MessageLeaveFishUI);
        else
            UIManager.Instance.OpenMessageBoxUI(content, 10, EnumMessageBoxType.OK_CANCEL, doQuitScene);
    }

    public void DoQuitScene()
    {
        doQuitScene(null);
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        if (SysDefines.ChannelId > 0 && SysDefines.LoginType == 8)
            UnityBridgeManager.Instance.OnChannelExit();
        else
            QuitGameWithMessage();
    }

    public void QuitGameWithMessage()
    {
        UIManager.Instance.OpenMessageBoxUI(SysDefines.QuitGame, 10, EnumMessageBoxType.OK_CANCEL, QuitGameDirect);
    }

    public void QuitGameDirect(object agrs)
    {
        Application.Quit();
    }

    //提示文本
    private Queue hintMsgQueue = new Queue();
    private bool isHintMsgShowing = false;
    private ulong lastHintTimestamp = 0;
    public void CreateHintMessage(string content)
    {
        content = content.Replace("公会", "小镇");
        if (string.IsNullOrEmpty(content)) return;
        if (hintMsgQueue.Contains(content)) return;
        hintMsgQueue.Enqueue(content);
        if ((TimeHelper.GetServerTimestamp() - lastHintTimestamp) >= 5000 && isHintMsgShowing) isHintMsgShowing = false;
        if (!isHintMsgShowing) showHintMessage();
    }

    //根据协议对象以及内部的错误码显示提示文本
    public void CreateHintMessageByResponseProtocol(JBPROTO.INetProtocol rsp)
    {
        var hintMsg = ErrcodeHelper.GetText(rsp);
        //Debug.LogError("错误码 hintMsg:" + hintMsg);
        if (!string.IsNullOrEmpty(hintMsg))
        {
            CreateHintMessage(hintMsg);
            Debug.LogError(hintMsg);
        }
    }
    
    public void LoadSetHeadIcon(Image image, int useId, int headId)
    {

        if (ReferenceEquals(image, null) || headId < 0)
            return;
        //临时屏蔽
      //  LoadImageIsSprite(image, SysDefines.UISETHEAD, "head", headId);
    }
    public void LoadHangIcon(Image image, int useId, int hangId)
    {

        if (ReferenceEquals(image, null) || hangId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UIHANG, "hang", hangId);
    }
    public void LoadVIPIcon(Image image, int useId,int vipId)
    {
        
        if (ReferenceEquals(image, null) || vipId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UIIVIP, "vip", vipId);
    }
    public void LoadDefectIcon(Image image, int useId, int defectId)
    {

        if (ReferenceEquals(image, null) || defectId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UIDEFECT, "defect", defectId);
    }
    public void LoadSevenDefectIcon(Image image, int useId, int defectId)
    {

        if (ReferenceEquals(image, null) || defectId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UISDEFECT, "base", defectId);
    }
    public void LoadViewIcon(Image image, int useId, int viewId)//查看界面UI换背景的方法
    {

        if (ReferenceEquals(image, null) || viewId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UICVIEW, "minuteC", viewId);
    }
    public void LoadTipsIcon(Image image, int useId, int tipsId)
    {

        if (ReferenceEquals(image, null) || tipsId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UITIPS, "tips", tipsId);
    }
    public void LoadDefect_Icon(Image image, int useId, int defectId)//炮台
    {

        if (ReferenceEquals(image, null) || defectId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UICANNON, "defect", defectId);
    }
    public void LoadRewardsBossTask_Icon(Image image, int useId, int bossId)//盛典悬赏的Boss任务
    {

        if (ReferenceEquals(image, null) || bossId < 0)
            return;
        // LoadImageIsSprite(image, SysDefines.UIPAGEENT, "activityCarnivalPostRewardTask", bossId);
    }
    public void LoadRewardsSprintTask_Icon(Image image, int useId, int sprintId)//盛典悬赏的Boss任务
    {

        if (ReferenceEquals(image, null) || sprintId < 0)
            return;
        // LoadImageIsSprite(image, SysDefines.UIPAGEENT, "sprint", sprintId);
    }
    public void LoadDefect__Icon(Image image, int useId, int defectId)//翅膀
    {

        if (ReferenceEquals(image, null) || defectId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UIWINGS, "defect", defectId);
    }
    public void LoadCannonNameDefect_Icon(Image image, int useId, int defectId)
    {

        if (ReferenceEquals(image, null) || defectId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UICANNON, "typeName", defectId);
    }
    public void LoadCannonGrounpDefect_Icon(Image image, int useId, int defectId)
    {

        if (ReferenceEquals(image, null) || defectId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UICANNON, "typeGrounp", defectId);
    }
    public void LoadCannonDefect_Icon(Image image, int useId, int defectId)
    {

        if (ReferenceEquals(image, null) || defectId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UICANNON, "typeName", defectId);
    }
    public void LoadRechargItem(Image image, int useId, int RechargItemId)
    {

        if (ReferenceEquals(image, null) || RechargItemId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UISHOP, "rechargItem", RechargItemId);
    }
    public void LoadBoxItem(Image image, int boxId, int boxItemId)//宝箱读取
    {

        if (ReferenceEquals(image, null) || boxItemId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UIRANDOMPREFAB, "box"+boxId, boxItemId);
    }
    public void LoadStoreIcon(Image image, int useId, string StoreIconId)
    {

        if (ReferenceEquals(image, null) || StoreIconId ==null)
            return;
        // LoadStringImageSprite(image, SysDefines.UISPICON, "storeIcon", StoreIconId);
    }
    public void LoadHighlightIcon(Image image, int useId, int highlightId)
    {

        if (ReferenceEquals(image, null) || highlightId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UIHIGH, "highlight", highlightId);
    }
    public Sprite LoadWeekItemIcon(Sprite image, int useId, int vipId)
    {

        if (ReferenceEquals(image, null) || vipId < 0)
            return null;
       // return LoadImageSprite(image, SysDefines.UIIWEEK, "week", vipId);
       return null;
    }
    public void LoadSignedChestIcon(Image image,string signed ,int useId, int signId)
    {

        if (ReferenceEquals(image, null) || signId < 0)
            return;
        // LoadImageSprite(image, SysDefines.UIKSIGN+signed+"/", "sign", signId);
    }
    public void LoadItemIcon(Image image, ItemInfo item)
    {
        LoadItemIcon(image, item.ItemID, item.ItemSubID);
    }

    public void LoadItemIcon(Image image, int id, int subId)
    {
        // LoadImageSprite(image, SysDefines.UIITEM, TItemHelper.GetRow(id, subId).Icon);
    }

    public void LoadImageSprite(Image image, string category, string prefix, int id)
    {
        LoadImageSprite(image, category, $"{prefix}_{id}");
    }
    public void LoadImageIsSprite(Image image, string category, string prefix, int id)
    {
        LoadImageIsSprite(image, category, $"{prefix}_{id}");
    }
    public void LoadStringImageSprite(Image image, string category, string prefix, string id)
    {
        LoadImageSprite(image, category, $"{prefix}_{id}");
    }
    public Sprite LoadImageSprite(Sprite image, string category, string prefix, int id)
    {
       return LoadImageSprite(image, category, $"{prefix}_{id}");
       
    }
    public void LoadImageSprite(Image image,string category, string name,bool id=false)
    {
        LoadImageSprite(image, category, $"{name}");
    }
    public void LoadImageSprite(Image image, string category, string name)
    {
        //Debug.LogError("LoadImageSprite>>" + category+","+name);
        image.sprite = ResManager.Instance.LoadPrefab(
            string.Format($"{category}{name}")).GetComponent<Image>().sprite;
    }
    public void LoadImageIsSprite(Image image, string category, string name)
    {
        //Debug.LogError("LoadImageSprite>>" + category+","+name);
        image.sprite = ResManager.Instance.LoadSprite(
            string.Format($"{category}{name}"));
    }
    public Sprite LoadImageSprite(Sprite image, string category, string name)
    {
        Debug.LogError($"{category}{name}");

        image = ResManager.Instance.LoadPrefab(
            string.Format($"{category}{name}")).GetComponent<Image>().sprite;
        return image;
        
    }

    public void CreateMessageBox(string content, int countTime, int btnType, Action onConfirm, Action onCancel = null)
    {
        EnumMessageBoxType btnEnumType = (EnumMessageBoxType)btnType;
        UIManager.Instance.OpenMessageBoxUI(content, countTime, btnEnumType,
            obj => onConfirm?.Invoke(), null, obj => onCancel?.Invoke(), null);
    }

    private void showHintMessage()
    {
        lastHintTimestamp = TimeHelper.GetServerTimestamp();
        var msg = hintMsgQueue.Dequeue().ToString();
        // var prefab = ResManager.Instance.LoadPrefab(string.Format($"{SysDefines.UIPREFAB}popupTipsUI"));
        // var obj = ObjectPoolManager.Instance.Spawn(prefab, HintMessage);
        // obj.GetOrAddComponent<HintMessage>().SetHintContent(msg, onHintMessageEnd);
        isHintMsgShowing = true;
        //ResManager.Instance.LoadAsync<GameObject>(string.Format($"{SysDefines.UICONTROLSPREFAB}popupTipsUI"), prefab =>
        //{
        //    var obj = ObjectPoolManager.Instance.Spawn(prefab, HintMessage);
        //    obj.GetOrAddComponent<HintMessage>().SetHintContent(msg, onHintMessageEnd);
        //    isHintMsgShowing = true;
        //});
    }

    private void onHintMessageEnd()
    {
        if (hintMsgQueue.Count > 0)
            showHintMessage();
        else
            isHintMsgShowing = false;
    }

    public void BuyGoods(int goodsId, int goodsType, string goodsName, int goodsPrice)
    {
        if (SysDefines.LoginType == 1 && string.IsNullOrEmpty(GameController.Instance.Player.Phone))//游客登录充值前强制绑定手机号
        {
            UIManager.Instance.OpenUI(EnumUIType.PhoneBindUI, 0);
            return;
        }
        //if ((SysDefines.ChannelId > 0 && SysDefines.LoginType == 8) || SysDefines.IsDebugMode)
        //{
        //    NetController.Instance.SendRechargeReq(100 + SysDefines.ChannelId, goodsId, goodsType, goodsName, goodsPrice);
        //}
        //else
        //{
        UIManager.Instance.OpenUI(EnumUIType.PayUI, new Dictionary<string, object>() {
                {"Price", goodsPrice * 0.01},
                {"Name", goodsName},
                {"ShopId", goodsId},
                {"ContentType", goodsType},
            });
        //}
    }
    #endregion

    #region private function
    /// <summary>
    /// 获取当前运行平台
    /// </summary>
    private void getRunPlatform()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.IPhonePlayer:
                SysDefines.Platform = 1;
                break;
            case RuntimePlatform.Android:
                SysDefines.Platform = 2;
                break;
            case RuntimePlatform.WindowsPlayer:
                SysDefines.Platform = 3;
                break;
            case RuntimePlatform.LinuxPlayer:
                SysDefines.Platform = 4;
                break;
            case RuntimePlatform.OSXPlayer:
                SysDefines.Platform = 5;
                break;
            case RuntimePlatform.WebGLPlayer:
                // 握手协议约定 WebGL/微信小游戏平台编号为 6。
                SysDefines.Platform = 6;
                break;
            default:
                SysDefines.Platform = 3;//测试阶段 todo
                break;
        }
    }

    private void doQuitScene(object agrs)
    {
        var sceneType = (EnumSceneType)Enum.Parse(typeof(EnumSceneType), SceneManager.GetActiveScene().name, true);
        switch (sceneType)
        {
            case EnumSceneType.LoginScene:
                break;
            case EnumSceneType.LoadingScene:
                break;
            case EnumSceneType.MainScene:
                NetController.Instance.SendLogoutReq();
                //NetController.Instance.SendAccessServiceReq(EnumGameGroupType.Fish, EnumAccessServiceType.QuitGame);
                break;
            case EnumSceneType.FishScene:
                NetController.Instance.SendExitGameReq(() =>
                {
                    NetController.Instance.SendExitSiteReq(SysDefines.SiteId);
                });
                break;
        }
    }

    private void doQuitGame(object agrs)
    {
        Application.Quit();
    }

    private void setGameNameIcon()
    {
        var gameNameRoot = GameObject.Find("LoginBg/GameName").transform;
        var count = gameNameRoot.childCount;
        for (int i = 0; i < count; i++)
        {
            var name = gameNameRoot.GetChild(i);
            name.gameObject.SetActive(name.name == $"GameName_0{SysDefines.ZoneId}");
        }
    }
#endregion

#region FPS
    //private float m_LastUpdateShowTime = 0f;  //上一次更新帧率的时间;  
    //private float m_UpdateShowDeltaTime = 0.1f;//更新帧率的时间间隔;  
    //private int m_FrameUpdate = 0;//帧数;
    //private float m_FPS = 0;

    //private void OnGUI()
    //{
    //    GUI.Label(new Rect(Screen.width / 2, 30, 300, 30), $"<color=red>FPS:  {m_FPS.ToString("f2")}</color>");
    //}
#endregion
}
