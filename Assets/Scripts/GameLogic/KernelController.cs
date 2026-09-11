using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
//using LuaInterface;
//using LuaFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using DG.Tweening;
//using NUnit.Framework;

public enum VersionType
{
    ShenHe, CeShi, FaBu
}
public enum GameLogType
{
    None, Print, Save, PrintAndSave
}

public class KernelController : MonoBehaviour
{
    //public static VersionType VersionType
    //{
    //    get { return KernelController.Instance.mVersionType; }
    //    set { KernelController.Instance.mVersionType = value; }
    //}
    //public static GameLogType GameLogType
    //{
    //    get { return KernelController.Instance.mGameLogType; }
    //    set { KernelController.Instance.mGameLogType = value; }
    //}

    /// <summary>
    ///  是否用自定义账号登录
    /// </summary>
    public bool isCustomAccountLogin = false;
    /// <summary>
    /// 是否使用内网
    /// </summary>
    //[NoToLua]
    //public bool isNeiWang = false;

    /// <summary>
    /// 是否启动点击按钮音效
    /// </summary>
    public bool clickAudioValid = true;

    public delegate void EventHandler();
    public delegate void Vector2EventHandler(Vector2 point);
    public Vector2EventHandler OnMouseDownEvent; //当手指按下屏幕
    public Vector2EventHandler OnMouseDragEvent; //当手指在屏幕上拖拽
    public Vector2EventHandler OnMouseUpEvent; //当手指离开屏幕
    public EventHandler OnPressBackEvent; //当玩家按下手机返回键(仅Android)
    public EventHandler OnDownloadHeadIconSuccess; //当玩家按下手机返回键(仅Android)

    protected Transform mainCanvas;
    protected GraphicRaycaster graphicRaycaster;

    [SerializeField]
    public string DEBUG_LOG = "";
    [SerializeField]
    private VersionType mVersionType; //打包类型
    [SerializeField]
    private GameLogType mGameLogType = GameLogType.Print; //打包类型

    private bool isMouseDown = false;
    private Vector2 point = Vector2.zero;

    private GameObject shareUi_Go;

    protected void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.LogError("Awake=" + mGameLogType);

        #region 判断分辨率
        float rs = (float)Screen.width / Screen.height;
        //if (Mathf.Abs(rs - 16f / 9) <= 0.001f)
        //{
        //    Globals.resolutionRatio = ResolutionRatio.R16_9;
        //}
        //else if (Mathf.Abs(rs - 1136f / 640) <= 0.001f)
        //{
        //    Globals.resolutionRatio = ResolutionRatio.R1136_640;
        //}
        //else if (Mathf.Abs(rs - 1024f / 600) <= 0.001f)
        //{
        //    Globals.resolutionRatio = ResolutionRatio.R1024_600;
        //}
        //else if (Mathf.Abs(rs - 16f / 10) <= 0.001f)
        //{
        //    Globals.resolutionRatio = ResolutionRatio.R16_10;
        //}
        //else if (Mathf.Abs(rs - 14f / 9) <= 0.001f)
        //{
        //    Globals.resolutionRatio = ResolutionRatio.R14_9;
        //}
        //else if (Mathf.Abs(rs - 8f / 5) <= 0.01f)
        //{
        //    Globals.resolutionRatio = ResolutionRatio.R8_5;
        //}
        //else if (Mathf.Abs(rs - 5f / 3) <= 0.001f)
        //{
        //    Globals.resolutionRatio = ResolutionRatio.R5_3;
        //}
        //else if (Mathf.Abs(rs - 4f / 3) <= 0.001f)
        //{
        //    Globals.resolutionRatio = ResolutionRatio.R4_3;
        //}
        //else if (Mathf.Abs(rs - 3f / 2) <= 0.001f)
        //{
        //    Globals.resolutionRatio = ResolutionRatio.R3_2;
        //}
        //else
        //{
        //    Debug.Log("未知分辨率: " + Screen.width + ", " + Screen.height);
        //}
       // Debug.Log("当前分辨率" + Globals.resolutionRatio);
        #endregion

       // if (mGameLogType == GameLogType.None) Debug.unityLogger.logEnabled = false;
        GL.Clear(false, true, Color.black);
        //AppFacade.Instance.StartUp();
        if (mGameLogType >= GameLogType.Print)
        {
            Application.logMessageReceivedThreaded += OnMessageReceived;
        }
    }

    private byte[] bytes;
    private string logName = DateTime.Now.ToString("yyyy-MM-dd");
    private string a = "", b = "";
    private int size = Screen.width / 64;
    private Rect remove10LineRect = new Rect(Screen.width - Screen.width / 11, Screen.height / 2 - Screen.height / 14.4f / 2, Screen.width / 11, Screen.height / 14.4f);
    private Rect removeAllRect = new Rect(Screen.width - Screen.width / 11, Screen.height / 2 + Screen.height / 14.4f / 2, Screen.width / 11, Screen.height / 14.4f);

    protected void Start()
    {
        //AppConst.SetProjectRoot();

        //if (PlayerPrefs.HasKey("MusicVolume"))
        //{
        //    AudioManager.Instance.SetBackgroundMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));
        //    Globals.normalVolume = PlayerPrefs.GetFloat("SoundVolume");
        //}
        //Debug.LogError("Start=" + mGameLogType);
        //if (mGameLogType >= GameLogType.Print)
        //{
            //if (mGameLogType >= GameLogType.Save)
            //{
            //    FileStream fs = new FileStream(AppConst.GetProjectRoot() + "/" + logName + ".txt", FileMode.OpenOrCreate);
            //    fs.Seek(0, SeekOrigin.End);
            //    bytes = Utility.GetBytes_UTF8(DateTime.Now.ToString("\n\n\n==========================打开游戏==========================\n"));
            //    fs.Write(bytes, 0, bytes.Length);
            //    fs.Close();

            //}
            //Application.logMessageReceivedThreaded += OnMessageReceived;
       // }
    }

    protected void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            if (Input.touchCount > 0) point = Input.GetTouch(0).position;
            else point = Input.mousePosition;
            if (OnMouseDownEvent != null) OnMouseDownEvent(point);
        }
        else if (Input.GetMouseButton(0) && OnMouseDragEvent != null)
        {
            if (Input.touchCount > 0) point = Input.GetTouch(0).position;
            else point = Input.mousePosition;
            OnMouseDragEvent(point);
        }
        else if (Input.GetMouseButtonUp(0) && OnMouseUpEvent != null)
        {
            if (Input.touchCount > 0) point = Input.GetTouch(0).position;
            else point = Input.mousePosition;
            OnMouseUpEvent(point);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (OnPressBackEvent != null) OnPressBackEvent();
        }

        if (!clickAudioValid) return;
        if (isMouseDown) RaycastUI(point);
        isMouseDown = false;
    }

    private void OnGUI()
    {
        if (mask != null)
        {
            GUI.DrawTexture(screenRect, mask, ScaleMode.StretchToFill);
        }
        if (mGameLogType == GameLogType.Print || mGameLogType == GameLogType.PrintAndSave)
        {
            if (!string.IsNullOrEmpty(DEBUG_LOG))
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = size;
                style.richText = true;
                GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "<color=#ff0000>" + DEBUG_LOG + "</color>", style);
            }
            if (GUI.Button(remove10LineRect, "清除10行"))
            {
                string str = DEBUG_LOG;
                string[] sp = str.Split('\n');
                if (sp.Length > 10)
                {
                    string newStr = "";
                    for (int i = 10; i < sp.Length; i++)
                    {
                        newStr += sp[i] + '\n';
                    }
                    DEBUG_LOG = newStr;
                }
                else
                {
                    DEBUG_LOG = "";
                }
            }
            if (GUI.Button(removeAllRect, "清除所有"))
            {
                DEBUG_LOG = "";
            }
        }
    }
    bool bReceived = false;
    protected void OnMessageReceived(string condition, string stackTrace, LogType type)
    {
        //Debug.LogError("OnMessageReceived="+ type);
        bReceived = true;
        if (mGameLogType == GameLogType.None) return;
        FileStream fs = null;
        //if (mGameLogType >= GameLogType.Save)
        //{
        //    fs = new FileStream(AppConst.GetProjectRoot() + "/" + logName + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        //    fs.Seek(0, SeekOrigin.End);
        //}

        if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
        {
            a = condition + '\n' + stackTrace;
            b = "--------------报错--------------\n";
            if (mGameLogType == GameLogType.Print || mGameLogType == GameLogType.PrintAndSave)
            {
                DEBUG_LOG += a;
                DEBUG_LOG += b;
            }
            //if (mGameLogType >= GameLogType.Save)
            //{
            //    if (fs == null) return;
            //    bytes = Utility.GetBytes_UTF8(DateTime.Now.ToString("HH-mm-ss:  ") + a);
            //    fs.Write(bytes, 0, bytes.Length);

            //    bytes = Utility.GetBytes_UTF8(DateTime.Now.ToString("HH-mm-ss:  ") + b);
            //    fs.Write(bytes, 0, bytes.Length);
            //}
        }
        else
        {
            a = condition + '\n';
            if (mGameLogType == GameLogType.Print || mGameLogType == GameLogType.PrintAndSave) DEBUG_LOG += a;
            //if (mGameLogType >= GameLogType.Save)
            //{
            //    if (fs == null) return;
            //    bytes = Utility.GetBytes_UTF8(DateTime.Now.ToString("HH-mm-ss:  ") + a);
            //    fs.Write(bytes, 0, bytes.Length);
            //}
        }
        if (mGameLogType >= GameLogType.Save)
        {
            if (fs != null)
            {
                fs.Close();
                fs.Dispose();
            }
        }
    }

    PointerEventData eventData;
    List<RaycastResult> rList = new List<RaycastResult>();
    protected void RaycastUI(Vector2 point)
    {
        /*if (graphicRaycaster == null)
        {
            GameObject mCanvas = GameObject.FindWithTag("MainCanvas");
            if (mCanvas != null)
            {
                mainCanvas = mCanvas.transform;
                graphicRaycaster = mainCanvas.GetComponent<GraphicRaycaster>();
            }
            if (graphicRaycaster == null) return;
        }

        rList.Clear();
        eventData = new PointerEventData(EventSystem.current);
        eventData.position = point;
        graphicRaycaster.Raycast(eventData, rList);
        */
        //if (rList.Count > 0 && !GlobalCanvas.Instance.IsDialogMask() &&
        //    rList[0].gameObject.GetComponent<Button>() != null &&
        //    rList[0].gameObject.GetComponent<Button>().IsInteractable())
        //{
        //    //AudioManager.Instance.PlaySound(AudioManager.GlobalSound.uiclick);
        //}
    }

    //private LuaFunction _GameDataMemory;
    //public void InitSceneLoader(LuaFunction func)
    //{
    //    _GameDataMemory = func;
    //}
    /// <summary>
    /// 刷新日志类型
    /// </summary>
    public void RefreshLogType()
    {
        Debug.unityLogger.logEnabled = mGameLogType != GameLogType.None;
        if (mGameLogType >= GameLogType.Print)
        {
            //if (mGameLogType >= GameLogType.Save)
            //{
            //    FileStream fs = new FileStream(AppConst.GetProjectRoot() + "/" + logName + ".txt", FileMode.OpenOrCreate);
            //    fs.Seek(0, SeekOrigin.End);
            //    bytes = Utility.GetBytes_UTF8(DateTime.Now.ToString("\n\n===============刷新Log类型===============n"));
            //    fs.Write(bytes, 0, bytes.Length);
            //    fs.Close();
            //}
            if (!bReceived) Application.logMessageReceivedThreaded += OnMessageReceived;
        }
    }
    private Texture2D mask;
    private Coroutine captureCor;
    private Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);


    #region 加载场景

    AssetBundleCreateRequest request;
    AssetBundle assetbundle; //可以用于卸载场景资源
    AsyncOperation async;

    /// <summary>
    /// 上次加载的场景名称 用于判断当前场景
    /// </summary>
    private string lastSceneName = string.Empty;
    byte[] GetBuffer(string path)
    {
        byte[] buffer = null;

        using (FileStream fs = new FileStream(path, FileMode.Open))
        {
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
        }
        return buffer;
    }

    public void LoadScene(string fullPath, string sceneName)
    {
       // StartCoroutine(IELoadScene(fullPath, sceneName));
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="fullPath">资源的完整路径</param>
    /// <param name="sceneName">场景名称</param>
    /// <returns></returns>
    private IEnumerator IELoadScene(string fullPath, string sceneName)
    {
        if (request == null)
        {
            request = AssetBundle.LoadFromMemoryAsync(GetBuffer(fullPath));
            yield return request;
            assetbundle = request.assetBundle;
        }

        async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        ///将场景激活关闭 ，加载完成之后打开 
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
        {
            //Debug.LogWarning("加载进度：" + async.progress);
            yield return null;//等待一帧 再执行
        }
        //Debug.LogWarning("已完成：" + async.progress);
        async.allowSceneActivation = true;//场景加载完毕 激活场景
    }

    #endregion
    /// <summary>
    /// 切换场景
    /// </summary>
//    public void LoadScene(string str)
//    {
//        //避免重复加载
//        if (lastSceneName.Equals(str, StringComparison.CurrentCultureIgnoreCase))
//            return;

//        Scene scene = SceneManager.GetActiveScene();
//        Debug.LogWarning(scene.name.CompareTo(str) + "【】" + str + "【】" + scene.name);

//        if (scene.name.CompareTo(str) != 0 && !scene.name.Equals(str, StringComparison.CurrentCultureIgnoreCase))
//        {
//            lastSceneName = str;
//            Debug.LogWarning("切换场景 ===================> " + str);
//            BeginCaptureShot();
//            _GameDataMemory.Call(str);
//            if (str.Equals("Empty", StringComparison.CurrentCultureIgnoreCase) || str.Equals("Lobby", StringComparison.CurrentCultureIgnoreCase) || str.Equals("Login", StringComparison.CurrentCultureIgnoreCase))
//            {
//                SceneManager.LoadScene(str);
//            }
//            else
//            {
//                string path = string.Empty;
//#if UNITY_EDITOR || UNITY_STANDALONE
//                path = Application.streamingAssetsPath + "/" + "myframe_updatescene.unity3d";
//#else
//                path = UnityEngine.Application.persistentDataPath + "/luaframework/myframe_updatescene.unity3d";
//#endif
//                StartCoroutine(IELoadScene(path, str));
//            }
//        }
//    }
    /// <summary>
    /// 开始屏幕截图并将截图投影到屏幕上
    /// </summary>
    public void BeginCaptureShot()
    {
        //captureCor = StartCoroutine(ScreenCaptureShot());
    }
    /// <summary>
    /// 取消屏幕覆盖截图
    /// </summary>
    public void StopCaptureShot()
    {
      //  if (captureCor != null) StopCoroutine(captureCor);
        mask = null;
        captureCor = null;
    }
    /// <summary>
    /// 对屏幕进行截图并分享到微信并指定分享类型
    /// </summary>
    /// <param name="shareType">0: 好友, 1: 朋友圈</param>
    /// <param name="ShareMode">分享方式:0: 微信, 1: 闲聊 ...........</param>
    public void ToScreenShot(short shareType)
    {
      //  StartCoroutine(ScreenCaptureShot2(shareType));
    }
    private IEnumerator ScreenCaptureShot()
    {
        yield return new WaitForEndOfFrame();
        Texture2D tex = new Texture2D((int)screenRect.width, (int)screenRect.height, TextureFormat.RGB24, false);
        tex.ReadPixels(screenRect, 0, 0);
        tex.Apply();
        yield return 0;
        mask = tex;
    }
    //private IEnumerator ScreenCaptureShot2(short type)
    //{
    //    yield return new WaitForEndOfFrame();
    //    Texture2D tex = new Texture2D((int)screenRect.width, (int)screenRect.height, TextureFormat.RGB24, false);
    //    tex.ReadPixels(screenRect, 0, 0);
    //    tex.Apply();

    //    byte[] bytes = tex.EncodeToJPG();
    //    string filename = AppConst.GetProjectRoot() + "/scene.jpg";
    //    File.WriteAllBytes(filename, bytes);
    //    yield return 0;
    //    GameObject go_ShareUI = GameObject.Instantiate(shareUi_Go);
    //    go_ShareUI.transform.SetParent(GlobalCanvas.Instance.shareUIParent.transform, false);
    //    for (int i = 0; i <= go_ShareUI.transform.childCount - 1; i++)
    //    {
    //        if (go_ShareUI.transform.GetChild(i).name == "Mask") { }
    //        else if (go_ShareUI.transform.GetChild(i).name == "WeiXin")
    //        {
    //            go_ShareUI.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(delegate ()
    //            {
    //                MissionWeiXin.shareGameResult(0, 0, filename);
    //                GameObject.Destroy(go_ShareUI);
    //            });
    //        }
    //        else if (go_ShareUI.transform.GetChild(i).name == "XianLiao")
    //        {
    //            go_ShareUI.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(delegate ()
    //            {
    //                MissionWeiXin.shareGameResult(1, 0, filename);
    //                GameObject.Destroy(go_ShareUI);
    //            });
    //        }
    //        else if (go_ShareUI.transform.GetChild(i).name == "DingDing")
    //        {
    //            go_ShareUI.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(delegate ()
    //            {
    //                MissionWeiXin.shareGameResult(3, 0, filename);
    //                GameObject.Destroy(go_ShareUI);
    //            });
    //        }
    //        else if (go_ShareUI.transform.GetChild(i).name == "YiXin")
    //        {
    //            go_ShareUI.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(delegate ()
    //            {
    //                MissionWeiXin.shareGameResult(2, 0, filename);
    //                GameObject.Destroy(go_ShareUI);
    //            });
    //        }
    //        else if (go_ShareUI.transform.GetChild(i).name == "Liaobei")
    //        {
    //            go_ShareUI.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(delegate ()
    //            {
    //                MissionWeiXin.shareGameResult(4, 0, filename);
    //                GameObject.Destroy(go_ShareUI);
    //            });
    //        }
    //        else if (go_ShareUI.transform.GetChild(i).name == "Close")
    //        {
    //            go_ShareUI.transform.GetChild(i).GetComponent<Button>().onClick.AddListener(delegate ()
    //            {
    //                GameObject.Destroy(go_ShareUI);
    //            });
    //        }
    //    }
    //    //MissionWeiXin.shareRankWeiXin(type, filename);
    //}
    public void ShareUIGo(GameObject go)
    {
        shareUi_Go = go;
    }

    #region Excute
    /// <summary>
    /// 等待指定帧数过后，执行动作
    /// </summary>
    //public Coroutine ToExecuteStep(int frameCount, Action action)
    //{
    //    if (action == null) return null;
    //    return StartCoroutine(WaitFrame(frameCount, action));
    //}
    ///// <summary>
    ///// 等待time秒，然后执行action
    ///// </summary>
    //public Coroutine ToExecuteWait(float time, Action action)
    //{
    //    if (action == null) return null;
    //    return StartCoroutine(WaitTime(time, action));
    //}
    ///// <summary>
    ///// 等待条件符合，执行动作(每帧判断一次)
    ///// </summary>
    //public Coroutine ToExecuteCondition(Func<bool> condition, Action action)
    //{
    //    if (action == null || condition == null) return null;
    //    return StartCoroutine(WaitUntilCondition(condition, action));
    //}
    ///// <summary>
    ///// 每隔interval秒判断条件是否符合，执行动作(会先判断一次)
    ///// </summary>
    //public Coroutine ToExecuteConditionEx(Func<bool> condition, Action action, float interval)
    //{
    //    if (action == null || condition == null) return null;
    //    return StartCoroutine(WaitUntilCondition(condition, action, interval));
    //}
    /// <summary>
    /// 执行动作count次 每次间隔interval秒
    /// </summary>
    //public Coroutine ToExecuteRepeat(int count, float interval, Action action)
    //{
    //    if (action == null) return null;
    //    return StartCoroutine(RepeatExecute(count, interval, action));
    //}
    /// <summary>
    /// 执行动作count次 每次间隔interval秒
    /// </summary>
    //public Coroutine ToExecuteRepeat_Int(int count, float interval, Action<int> action)
    //{
    //    if (action == null) return null;
    //    return StartCoroutine(RepeatExecute_Int(count, interval, action));
    //}
    ///// <summary>
    ///// 执行动作count次 每次间隔interval秒 结束后执行动作2
    ///// </summary>
    //public Coroutine ToExecuteRepeatEx(int count, float interval, Action action, Action action2)
    //{
    //    if (action == null) return null;
    //    return StartCoroutine(RepeatExecute(count, interval, action, action2));
    //}
    /// <summary>
    /// 执行动作count次 每次间隔interval秒 结束后执行动作2
    /// </summary>
    //public Coroutine ToExecuteRepeatEx_Int(int count, float interval, Action<int> action, Action action2)
    //{
    //    if (action == null) return null;
    //    return StartCoroutine(RepeatExecute_Int(count, interval, action, action2));
    //}

    protected IEnumerator RepeatExecute(int count, float interval, Action action)
    {
        YieldInstruction cor = new WaitForSeconds(interval);
        for (int i = 0; i < count; i++)
        {
            action();
            yield return cor;
        }
    }
    protected IEnumerator RepeatExecute_Int(int count, float interval, Action<int> action)
    {
        YieldInstruction cor = new WaitForSeconds(interval);
        for (int i = 0; i < count; i++)
        {
            action(i);
            yield return cor;
        }
    }
    protected IEnumerator RepeatExecute(int count, float interval, Action action, Action action2)
    {
        YieldInstruction cor = new WaitForSeconds(interval);
        for (int i = 0; i < count; i++)
        {
            action();
            yield return cor;
        }
        if (action2 != null) action2();
    }
    protected IEnumerator RepeatExecute_Int(int count, float interval, Action<int> action, Action action2)
    {
        YieldInstruction cor = new WaitForSeconds(interval);
        for (int i = 0; i < count; i++)
        {
            action(i);
            yield return cor;
        }
        if (action2 != null) action2();
    }
    protected IEnumerator WaitFrame(int frameCount, Action action)
    {
        YieldInstruction cor = new WaitForEndOfFrame();
        for (int i = 0; i < frameCount; i++)
        {
            yield return cor;
        }
        action();
    }
    protected IEnumerator WaitTime(float time, Action action)
    {
        YieldInstruction cor = new WaitForSeconds(time);
        yield return cor;
        action();
    }
    protected IEnumerator WaitUntilCondition(Func<bool> condition, Action action)
    {
        IEnumerator cor = new WaitUntil(condition);
        yield return cor;
        action();
    }
    protected IEnumerator WaitUntilCondition(Func<bool> condition, Action action, float interval)
    {
        YieldInstruction cor = new WaitForSeconds(interval);
        while (true)
        {
            if (condition()) break;
            yield return cor;
        }
        action();
    }
    #endregion

    private Coroutine downloadIcon;
    /// <summary>
    /// 下载头像
    /// </summary>
    //public void DownloadMyHeadIcon(string path)
    //{
    //    Debug.Log("我的头像地址: " + path);
    //    if (downloadIcon != null) StopCoroutine(downloadIcon);
    //    downloadIcon = StartCoroutine(DownloadIcon(path));
    //}
    //private IEnumerator DownloadIcon(string path)
    //{
    //    if (string.IsNullOrEmpty(path))
    //    {
    //        ResourceManager resMgr = LuaHelper.GetResManager();
    //        resMgr.LoadPrefab("common_global", new[] { "DefaultIcon_Man", "DefaultIcon_Women" }, (UnityEngine.Object[] objs) =>
    //        {
    //            Globals.headIcon = (Globals.wxUserInfo.sex == 0 ? objs[0] : objs[1]) as Sprite;
    //            if (OnDownloadHeadIconSuccess != null) OnDownloadHeadIconSuccess();
    //        });
    //        yield break;
    //    }
    //    path = path.Substring(0, path.LastIndexOf('/') + 1) + "132";
    //    int count = 0;
    //start:
    //    WWW www = new WWW(path);
    //    yield return www;
    //    Debug.Log("头像www下载完成");
    //    if (string.IsNullOrEmpty(www.error))
    //    {
    //        Texture2D tex = www.texture;
    //        Globals.headIcon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    //        if (OnDownloadHeadIconSuccess != null) OnDownloadHeadIconSuccess();
    //        Debug.Log("我的头像获取成功");
    //    }
    //    else
    //    {
    //        count++;
    //        if (count <= 999999)
    //        {
    //            //Debug.Log("我的头像获取失败: " + www.error + ", 正在尝试第" + count + "次下载");
    //            www.Dispose();
    //            goto start;
    //        }
    //        ResourceManager resMgr = LuaHelper.GetResManager();
    //        resMgr.LoadPrefab("common_global", new[] { "DefaultIcon_Man", "DefaultIcon_Women" }, (UnityEngine.Object[] objs) =>
    //        {
    //            Globals.headIcon = (Globals.wxUserInfo.sex == 0 ? objs[0] : objs[1]) as Sprite;
    //            if (OnDownloadHeadIconSuccess != null) OnDownloadHeadIconSuccess();
    //        });
    //    }
    //    www.Dispose();
    //}

}