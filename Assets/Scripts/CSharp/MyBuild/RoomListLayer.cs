using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum mystatus
{
    None = 0,
    Check,
    NoNeedDownload,
    NeedDownload,
    FinishDownload,
    DowonloadError,
}

public class UpdateConfig
{
    public string newfileurl;
    public string dst;
    public string src;
    public string downurl;

    public bool isBase;
    public bool isClient;
}

public class GameInfo
{
    public int _KindID;
    public int gameid;
    public int status;
    public string _GameName;
    public string _KindName;
    public string _Module;
    public string _KindVersion;
    public int _ServerResVersion;
    public string _Type;
    public bool _Active;
    public string massage;
}

public class downItem
{
    public string path;
    public string name;

    public downItem(string p, string n)
    {
        this.path = p;
        this.name = n;
    }
}

public class RoomListLayer : MonoBehaviour {
    bool m_bGameUpdate = false;
   
   // List<string> downlist;
   // Dictionary<int, string> game_ver_list;

    GameInfo[] _downgameinfo;

    public GameObject[] m_spDownloadBtn;
    public GameObject[] m_spDownloadMask;
    GameObject[] m_labDownloadTip;
    GameObject[] m_labDownloadjt;
    GameObject[] m_labDownloadjt1;
    private const float bytesToMb = 1048576;
    private const float bytesToKb = 1024;

    public Action<GameObject> mCallback;

    int roomCount = 3;

    public void InitGameList(int roomCount)
    {
        //Debug.LogError("InitGameList::::");
        this.roomCount = roomCount;
        // /*
        //MyApp app = transform.GetComponent<MyApp>();
        yl.updateList = new Dictionary<int, AssetUpdater>();
        yl._gameList = new List<GameInfo>();
        
        _downgameinfo = new GameInfo[roomCount];
        for (int i=0;i< roomCount; i++)
        {
            var gameinfo = new GameInfo();
            // gameinfo._KindID = item["KindID"].ToString();
            gameinfo._KindID = i+1;
            gameinfo.gameid = i;
            gameinfo.status = (int)mystatus.None;
            gameinfo.massage = "";
            //gameinfo.status = Convert.ToInt32(item["Nullity"]);
            //gameinfo._GameName = item["KindName"].ToString();
            //gameinfo._KindName = item["ModuleName"].ToString().ToLower() + ".";
            //gameinfo._Module = gameinfo._KindName.Replace(".", "/");
            //gameinfo._KindVersion = item["ClientVersion"].ToString();
            gameinfo._ServerResVersion = i+1;
            gameinfo._Type ="Room"+ (i + 1).ToString();

            _downgameinfo[i] = gameinfo;
            yl._gameList.Add(gameinfo);
            
        }//*/

       // UpdateType _updateType = UpdateType.Win;                    
       // AssetSourceType _gameAssetSource = AssetSourceType.UpdateAssetBundle;

        AppRoot.Game = new Context[roomCount];
        for (int i = 0; i < roomCount; i++)
        {
            var gameinfo = _downgameinfo[i];
            // 
            AppRoot.Game[gameinfo.gameid] = new Context();
            AppRoot.Game[gameinfo.gameid].Config = new AssetConfig();
            // AppRoot.Game[gameinfo.gameid].Config = AppRoot.Game[gameinfo.gameid].Config.Clone();
            AppRoot.Game[gameinfo.gameid].Config.SetAssetType(AppRoot.Get()._updateType, AppRoot.Get()._gameAssetSource);
            // Debug.LogError(">>>>" + i + "" + gameinfo.gameid + "，" + AppRoot.Game[gameinfo.gameid].Config.IsEditorAssets);
            AppRoot.Game[gameinfo.gameid].Loader = new AssetLoader(AppRoot.Game[gameinfo.gameid]);
            AppRoot.Game[gameinfo.gameid].BundleMgr = new BundleManager(AppRoot.Game[gameinfo.gameid]);

        }

        m_labDownloadTip = new GameObject[roomCount];
        m_labDownloadjt = new GameObject[roomCount];
        m_labDownloadjt1 = new GameObject[roomCount];
        m_spDownloadBtn = new GameObject[roomCount];
        m_spDownloadMask = new GameObject[roomCount];
    }

    // Use this for initialization
    void Start() {
        // Debug.LogError(">>>>"+ this.gameObject);
        
        
        
        
       // SysDefines.OssUrl = "https://game.hofoo.top";
       

        

        //InitGameList();
       // 
       // m_spDownloadMask = new GameObject[3];
        
        Init();
    }

    public void Init()
    {
        for (int i = 0; i < roomCount; i++)
        {
           // Debug.LogError("房间>>>i:"+i+","+ m_spDownloadBtn[i].name);
            EventTriggerListener.Get(m_spDownloadBtn[i]).SetEventHandle(EnumTouchEventType.OnClick, onTogClick, null, i);

            DownloadGame(_downgameinfo[i], m_spDownloadBtn[i]);
        }
    }
    private void onTogClick(GameObject listener, object eventData, object[] args)
    {
        var arg = int.Parse(args[0].ToString());
        //Debug.LogError("onTogClick:" + arg);

        // UpdateGame(_downgameinfo[arg], m_spDownloadBtn[arg]);

        if (AppRoot.Game[arg].Config.IsEditorAssets)
        {
            GameInfo gameinfo1 = _downgameinfo[arg];
            EnterGameInfo(gameinfo1);
        }
        else
        {

            int count = 0;
            for (int i = 0; i < arg; i++)
            {
                GameInfo gameinfo = _downgameinfo[i];

                Debug.LogError("======前面按钮的状态:" + i + "," + gameinfo.status);

                if (gameinfo.status <= (int)mystatus.NeedDownload)
                    ClickGameBtn(i, false);
                else
                    count++;
            }

            Debug.LogError("onTogClick:" + arg+ ",count:" + count);

            //test
            if (arg==0)
            {
                int i = 3;
                GameInfo gameinfo = _downgameinfo[i];
                if (gameinfo.status <= (int)mystatus.NeedDownload)
                    ClickGameBtn(i, false);
                else
                    count++;

                ClickGameBtn(arg, count == 1 ? true : false);
            }
            else if (arg == 2)
            {
                int i = 4;
                GameInfo gameinfo = _downgameinfo[i];
                if (gameinfo.status <= (int)mystatus.NeedDownload)
                    ClickGameBtn(i, false);
                else
                    count++;

                ClickGameBtn(arg, count == 1 ? true : false);
            }
            else
            ClickGameBtn(arg, count == arg ? true : false);
        }
    }

    void ClickGameBtn(int arg,bool flg)
    {
        GameInfo gameinfo = _downgameinfo[arg];
        
        var targetPlatform = Application.platform;
        // Platform check
        //if (targetPlatform == RuntimePlatform.WindowsPlayer ||
        //    targetPlatform == RuntimePlatform.WindowsEditor)
        //{
        //    EnterGameInfo(gameinfo);
        //}
        //else
        {
            // Version check logic
            //bool isAppStoreVersion = false;//appdf.APPSTORE_VERSION; // Assuming this is defined somewhere

            if (gameinfo.status <= (int)mystatus.Check)
            {
                return;
            }

            //for (int i = 0; i < i + 1; i++)
            //{ 
            //    ClickGameBtn(i, false);
            //}
            Debug.LogError("status:" + (mystatus)gameinfo.status + ",index:" + arg+ ",gameinfo.gameid：" + gameinfo.gameid);

            if (gameinfo.status == (int)mystatus.NeedDownload || gameinfo.status == (int)mystatus.DowonloadError)
            {
                UpdateGame(gameinfo, m_spDownloadBtn[arg]);
            }
            else
            {
                if (flg)
                    EnterGameInfo(gameinfo);
            }
        }        
    }

    // Update is called once per frame
    void Update() {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    UpdateGame(_downgameinfo[2], m_spDownloadBtn[2]);
        //}
    }
    // Initialize configuration
    public void InitCfg()
    {
        // /*
        //var app = _scene.GetApp();
        //downlist = app._serverConfig.downUrlLsit ?? new List<string>();
        // GlobalUserItem.tb_gameType = app._serverConfig.name_type ?? new Dictionary<int, string>();
        //game_ver_list = app._serverConfig.game_ver_list ?? new Dictionary<int, string>();
        //*/
    }

    

    // Display game progress
    public void ShowGameProgress(int wKindID, float nPercent)
    {
        // /* 
        string permsg = string.Format("{0:F0}%", nPercent);

        // Check if m_spDownloadMask exists for the given wKindID
        if (m_spDownloadMask[wKindID] != null)
        {
            GameObject downbg = m_spDownloadMask[wKindID].transform.Find("downbg").gameObject;

            if (downbg != null)
            {
                GameObject propressTimer = downbg.transform.Find("download_mask_sp").gameObject;
                if (propressTimer != null)
                {
                    // Assuming there's a SetPercentage method on the component
                    //111
                    //var progressComponent = propressTimer.GetComponent<ProgressBar>(); // Replace with actual component type
                    //if (progressComponent != null)
                    //{
                    //    progressComponent.SetPercentage(nPercent);
                    //}
                }
            }
        }

        // Check if m_labDownloadTip exists for the given wKindID
        if (m_labDownloadTip[wKindID] != null)
        {
            var labDownloadTip = m_labDownloadTip[wKindID];
            var textComponent = labDownloadTip.GetComponent<Text>(); // Assuming Unity UI Text component
            if (textComponent != null)
            {
                textComponent.text = permsg;
            }

            Debug.LogError("隐藏按钮>>>>");
            // Handle m_labDownloadjt visibility
            var labDownloadjt = m_labDownloadjt[wKindID];
            if (labDownloadjt != null)
            {
                labDownloadjt.SetActive(false);
            }

            //labDownloadTip.SetActive(true);

            var labDownloadjt1 = m_labDownloadjt1[wKindID];
            if (labDownloadjt1 != null)
            {
                labDownloadjt1.SetActive(true);
            }

        }//*/
    }

    // Hide game progress
    public void HideGameProgress(int wKindID, bool isSucc)
    {
        // If download was successful
       // /*
          if (isSucc)
         {
             var downloadMask = m_spDownloadMask[wKindID];
             if (downloadMask != null)
             {
                 downloadMask.SetActive(false);

                 // Handle animation
                 var animat = downloadMask.GetComponent<Animation>(); // Replace with actual animation component
                 if (animat != null)
                 {
                    //111
                     //if (mAniTable.ContainsKey(wKindID) && mAniTable[wKindID].format == "cocos")
                     //{
                     //    animat.Play("Animation1");
                     //}
                     //else if (mAniTable.ContainsKey(wKindID))
                     //{
                     //    var spineAnim = mAniTable[wKindID].ani as Spine.Unity.SkeletonAnimation; // Example for Spine
                     //    if (spineAnim != null)
                     //    {
                     //        spineAnim.AnimationState.SetAnimation(0, "animation", true);
                     //    }

                     //    if (mAniTable[wKindID].node != null)
                     //    {
                     //        mAniTable[wKindID].node.SetActive(true);
                     //    }
                     //}
                 }

                 // Handle download tip and arrow
                 var labDownloadTip = m_labDownloadTip[wKindID];
                 if (labDownloadTip != null)
                 {
                     labDownloadTip.SetActive(false);

                    Debug.LogError("显示按钮>>>>" + wKindID);
                    var labDownloadjt = m_labDownloadjt[wKindID];
                     if (labDownloadjt != null)
                     {
                         labDownloadjt.SetActive(true);
                     }

                    var labDownloadjt1 = m_labDownloadjt1[wKindID];
                    if (labDownloadjt1 != null)
                    {
                        //labDownloadjt1.transform.DOKill(); // 停止旋转
                        //labDownloadjt1.SetActive(false);
                        
                    }
               // }
             }
         }
         else // If download failed
         {
            Debug.LogError("wKindID:" + wKindID);
             var spMask = m_spDownloadMask[wKindID];

             if (spMask != null)
             {
                 spMask.SetActive(true);
                /*
                 var downbg = spMask.transform.Find("downbg").gameObject;
                 if (downbg != null)
                 {
                     downbg.SetActive(true);

                     var spTipLab = downbg.transform.Find("download_mask_tip").gameObject;
                     var spjiantou = downbg.transform.Find("download_mask_jt").gameObject;

                     if (spTipLab != null) spTipLab.SetActive(false);
                     if (spjiantou != null) spjiantou.SetActive(true);

                     var propressTimer = downbg.transform.Find("download_mask_sp").gameObject;
                     if (propressTimer != null)
                     {
                        //111
                        var progressBar = propressTimer.GetComponent<Slider>(); // Replace with actual component
                        if (progressBar != null)
                        {
                            progressBar.value = 0;
                        }
                    }*/

                    var labDownloadTip = m_labDownloadTip[wKindID];
                    if (labDownloadTip != null)
                    {
                        labDownloadTip.SetActive(false);

                        Debug.LogError("隐藏按钮>>>>");
                        var labDownloadjt = m_labDownloadjt[wKindID];
                        if (labDownloadjt != null)
                        {
                            labDownloadjt.SetActive(false);
                        }

                        var labDownloadjt1 = m_labDownloadjt1[wKindID];
                        if (labDownloadjt1 != null)
                        {
                        //labDownloadjt1.transform.DOKill(); // 停止旋转
                        labDownloadjt1.SetActive(true);

                    }
                    }
                    }
             }
         }//*/
    }
    Vector2 m_szMaskSize;
    public void UpdateGame(GameInfo gameinfo, GameObject sender)
    {
        // Check if game is already in update list
       // /* 
        if (yl.updateList.ContainsKey(gameinfo.gameid))
         {
             ShowToast("正在下载中", 1); // "Downloading..."
             return;
         }
        
         onGameZipUpdate(gameinfo, sender);
       // Debug.LogError(">>>>sender：" + sender);
        if (sender != null)
         {
             var spMask = sender.transform.Find("download_mask").gameObject;

           // Debug.LogError(">>>>spMask：" + spMask);

            if (spMask != null)
             {
                 var downbg = spMask.transform.Find("downbg").gameObject;
               // Debug.LogError(">>>>downbg：" + downbg);
                if (downbg != null)
                 {
                     downbg.SetActive(true);

                     // Store reference to the mask in dynamic field
                     m_spDownloadMask[gameinfo.gameid] = spMask;
                     spMask.SetActive(true);

                     if (spMask != null)
                     {
                         // Assuming you have a way to get content size in Unity
                         m_szMaskSize = new Vector2(
                             spMask.GetComponent<RectTransform>().rect.width,
                             spMask.GetComponent<RectTransform>().rect.height
                         );
                     }

                     var spTipLab = downbg.transform.Find("download_mask_tip").gameObject;
                     var spjiantou = downbg.transform.Find("download_mask_jt").gameObject;
                    var spjiantou1 = downbg.transform.Find("download_mask_jt1").gameObject;
                    //Debug.LogError(">>>>spTipLab:" + spTipLab + ",spjiantou1:" + spjiantou1);

                    // if (spTipLab != null) spTipLab.SetActive(true);
                    if (spjiantou != null) spjiantou.SetActive(false);

                     // Store references to UI elements
                     m_labDownloadTip[gameinfo.gameid] = spTipLab;
                     m_labDownloadjt[gameinfo.gameid] = spjiantou;
                    m_labDownloadjt1[gameinfo.gameid] = spjiantou1;

                    if (spTipLab != null)
                     {
                         var textComponent = spTipLab.GetComponent<Text>();
                         if (textComponent != null)
                         {
                             textComponent.text = "0%";
                         }
                     }
                 }
             }
         }//*/
    }
    // Update game zip
    public void onGameZipUpdate(GameInfo gameinfo, GameObject ts = null)
    {
        Debug.LogError("onGameZipUpdate");
        // Fail retry
        // /*
        if (gameinfo == null)
        {
            return;
        }

        // Check if already in update list
        if (yl.updateList.ContainsKey(gameinfo.gameid))
        {
            return;
        }

        // Validate game info
        var downgameinfoField = _downgameinfo[gameinfo.gameid];
        if (gameinfo == null && (downgameinfoField == null))
        {
            ShowToast("无效游戏信息！", 1); // "Invalid game info!"
            return;
        }

        // Record game info
        if (gameinfo != null)
        {
            _downgameinfo[gameinfo.gameid] = gameinfo;
        }

        // Get download config
        string version = "";
        var _update = ts.GetComponent<AssetUpdater>();

        _update.StartHotUpdateDownload();
        yl.updateList[gameinfo.gameid] = _update;

        // Show download mask
        var spMask = m_spDownloadMask[gameinfo.gameid];
        if (spMask != null)
        {
            var downbg = spMask.transform.Find("downbg").gameObject;
            if (downbg != null)
            {
                downbg.SetActive(true);
            }
        }//*/
    }
   

    private void onProgress(ProgressType type, int loaded, int total,int gameid)
    {
        if (type == ProgressType.Download)
        {
            Debug.LogError("loaded:" + loaded + ",total:" + total);
            float mainpersent = 0;

            if(total!=0)
                mainpersent = (float)loaded / (float)total;

            float sub = 0;
            string msg = "";
            OnUpdateProgress(sub, msg, mainpersent, gameid);
            UpdateDownloadProgress(gameid, mainpersent);
        }
    }

    private void onCheckDone(int totalBytes, int totalBytes1, int gameid)
    {
        //Debug.LogError("onCheckDone>>>>");
        if (totalBytes > 0)
        {
            string str = "";
            if (totalBytes >= bytesToMb)
            {
                str = UnityHelper.NumberFormat((long)((float)totalBytes / bytesToMb), 0) + "M";
            }
            else if (totalBytes >= bytesToKb)
            {
                str = UnityHelper.NumberFormat((long)((float)totalBytes / bytesToKb), 0) + "K";
            }
            else
            {
                str = totalBytes + "B";
            }
            var msg = $"{gameid}您有{str}资源需要下载";
            Debug.LogError("kkkkkkk=====>"+msg+ ",gameid:" + gameid);

            var gameinfo = _downgameinfo[gameid];
            gameinfo.status = (int)mystatus.NeedDownload;
            //UIManager.Instance.OpenMessageBoxUI(msg, 0, EnumMessageBoxType.OK_CANCEL, onBtnOK, null, onBtnCancel);
        }
        else
        {
           // onComplete(gameid);
            //Debug.LogError("没有需要下载，"+gameid);
            var gameinfo = _downgameinfo[gameid];
           
            gameinfo.status = (int)mystatus.NoNeedDownload;
        }
    }

    void onComplete(int gameid)
    {
        this.HideGameProgress(gameid, true);
        //Debug.LogError("onComplete:gameid:===" + gameid);
        OnUpdateResult(true, "", gameid);
    }

    private void onError(int errorCode, string error,int gameid)
    {
        //Debug.LogError("onError:gameid:===" + gameid);
        this.HideGameProgress(gameid, false);
        OnUpdateResult(false, error, gameid);
    }

    // Show update error notification
    public void OnUpdataNotify()
    {
        ShowToast("游戏版本信息错误！", 1); // "Game version info error!"
    }

    // Show game update waiting state
    public void ShowGameUpdateWait()
    {
        m_bGameUpdate = true;
        // ExternalFun.PopupTouchFilter(1, false, "游戏更新中,请稍候！"); 
        // ("Game updating, please wait!")
    }

    // Update progress callback
    public void OnUpdateProgress(float sub, string msg, float mainpersent, int kindID)
    {
       // /* 
         string permsg = string.Format("{0:F0}%", mainpersent);
        Debug.Log($"updateProgress>>>>>> {permsg}, m_labDownloadTip{kindID}, {kindID}");

        // Get download mask
        var maskField = m_spDownloadMask[kindID];
         var spDownloadMask = maskField;

         if (spDownloadMask != null)
         {
             float scale = (101 - mainpersent) / 100;

             // Update progress bar
             var downbg = spDownloadMask.transform.Find("downbg").gameObject;
             if (downbg != null)
             {
                 var propressTimer = downbg.transform.Find("download_mask_sp").gameObject;
                 if (propressTimer != null)
                 {
                     var progressBar = propressTimer.GetComponent<Slider>(); // Replace with your progress component
                     progressBar.value = mainpersent; // Or appropriate progress setting method
                 }
             }
         }

         // Update progress text
         var tipField = m_labDownloadTip[kindID];
         var labDownloadTip = tipField;

       // Debug.LogError("Update progress text=="+ tipField+","+ kindID);
         if (labDownloadTip != null)
         {
             var textComponent = labDownloadTip.GetComponent<Text>();
             if (textComponent != null)
             {
                 textComponent.text = permsg;
             }
            //
            // Hide/show related UI elements
            var jtField = m_labDownloadjt[kindID];
             var labDownloadjt = jtField;
             if (labDownloadjt != null)
             {
                 labDownloadjt.SetActive(false);
             }

            
            // labDownloadTip.SetActive(true);

            var labDownloadjt1 = m_labDownloadjt1[kindID];
            if (labDownloadjt1 != null)
            {
                labDownloadjt1.SetActive(true);
                Debug.LogError("旋转>>>>");
                labDownloadjt1.transform.DOLocalRotate(new Vector3(0, 0, 360), 2f, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart) // -1 表示无限循环
                    .SetRelative(false); // 使用绝对角度
            }
        }
         else
         {
             // InitDownloadList(); // Uncomment if needed
         }

     //*/
    }
    // Update result handler
    public void OnUpdateResult(bool result, string msg, int kindID)
    {
       // /* 
         DismissGameUpdateWait();
        //Debug.LogError("kindId:" + kindID+ ",result:" + result);

        if (result)
         {
             // Success case
             ShowToast(string.IsNullOrEmpty(msg) ? "下载成功" : msg, 1); // "Download successful"

          //  MyApp app = transform.GetComponent<MyApp>(); // Adjust based on your hierarchy
         //    if (app == null) return;

             // Handle download mask visibility and animation
             var maskField = m_spDownloadMask[kindID];
             var spDownloadMask = maskField;

             if (spDownloadMask != null)
             {
                 spDownloadMask.SetActive(false);

                 // Handle animations
                 //var anim = spDownloadMask.GetComponent<Animation>(); // Replace with your animation component
                 //if (anim != null && mAniTable.ContainsKey(kindID))
                 //{
                 //    if (mAniTable[kindID].format == "cocos")
                 //    {
                 //        anim.Play("Animation1");
                 //    }
                 //    else
                 //    {
                 //        // Example for Spine animation
                 //        var spineAnim = mAniTable[kindID].ani as SkeletonAnimation;
                 //        if (spineAnim != null)
                 //        {
                 //            spineAnim.AnimationState.SetAnimation(0, "animation", true);
                 //        }

                 //        if (mAniTable[kindID].node != null)
                 //        {
                 //            mAniTable[kindID].node.SetActive(true);
                 //        }
                 //    }
                 //}

                 // Update UI elements
                /* var tipField =m_labDownloadTip[kindID];
                 var labDownloadTip = tipField;
                 if (labDownloadTip != null)
                 {
                     labDownloadTip.SetActive(false);

                    Debug.LogError("显示按钮>>>>"+ msg);

                    var jtField = m_labDownloadjt[kindID];
                     var labDownloadjt = jtField;
                     if (labDownloadjt != null)
                     {
                         labDownloadjt.SetActive(true);
                     }

                    var labDownloadjt1 = m_labDownloadjt1[kindID];
                    if (labDownloadjt1 != null)
                    {
                        //labDownloadjt1.SetActive(false);
                    }
                }*/
             }

             // Update version number
             var downgameinfoField = _downgameinfo[kindID];
             var downgameinfo = downgameinfoField;

            var gameinfo = _downgameinfo[kindID];
            gameinfo.status = (int)mystatus.FinishDownload;

            if (downgameinfo != null)
             {
                 //foreach (var game in app._gameList)
                 //{
                 //    if (game._KindID == downgameinfo._KindID)
                 //    {
                 //        app.GetVersionMgr().SetResVersion(game._ServerResVersion, game._KindID);

                 //        string gamename = "游戏"; // Default to "game"
                 //        if (yl.GAME_NOTICE_TYPE.ContainsKey(kindID))
                 //        {
                 //            gamename = yl.GAME_NOTICE_TYPE[kindID];
                 //        }
                 //       Debug.LogError("" + gamename + " 下载完成");
                 //       // ShowToast(_scene, ""+gamename+" 下载完成", 2); // "[GameName] download completed"
                 //        break;
                 //    }
                 //}
             }
         }
         else
         {
            Debug.LogError("下载失败???>>>>" + msg+ ",kindID:" + kindID);

            // Failure case
            ShowToast(string.IsNullOrEmpty(msg) ? "下载失败" : msg, 2); // "Download failed"

             if (yl.updateList.ContainsKey(kindID))
             {
                 yl.updateList.Remove(kindID);
             }

            var gameinfo = _downgameinfo[kindID];
            gameinfo.status = (int)mystatus.DowonloadError;
            gameinfo.massage = string.IsNullOrEmpty(msg) ? "下载失败" : msg;


            // Update UI for failure case
            var tipField = m_labDownloadTip[kindID];
             var labDownloadTip = tipField;
             if (labDownloadTip != null)
             {
                 var textComp = labDownloadTip.GetComponent<Text>();
                 if (textComp != null) textComp.text = "下载失败";

                Debug.LogError("下载失败 显示按钮>>>>");
                var jtField = m_labDownloadjt[kindID];
                 var labDownloadjt = jtField;
                 if (labDownloadjt != null)
                 {
                     labDownloadjt.SetActive(true);
                 }

                var labDownloadjt1 = m_labDownloadjt1[kindID];
                if (labDownloadjt1 != null)
                {
                    labDownloadjt1.SetActive(false);
                }
            }

             // Reset progress display
             var maskField = m_spDownloadMask[kindID];
             var spMask = maskField;
             if (spMask != null)
             {
                 spMask.SetActive(true);

                 var downbg = spMask.transform.Find("downbg").gameObject;
                 if (downbg != null)
                 {
                     downbg.SetActive(true);

                    var propressTimer = downbg.transform.Find("download_mask_sp").gameObject;
                    if (propressTimer != null)
                    {
                        var progressBar = propressTimer.GetComponent<Slider>();
                        progressBar.value =0;
                    }
                }
             }
         }
       //  */
    }
    // Dismiss game update waiting state
    public void DismissGameUpdateWait()
    {
        m_bGameUpdate = false;
        /* ExternalFun.DismissTouchFilter();*/
    }

    // Show popup wait indicator
    public void ShowPopWait(int gameid)
    {
        /*if (_scene != null)
        {
            _scene.ShowPopWait(null, gameid);
        }*/
    }

    // Dismiss popup wait indicator
    public void DismissPopWait()
    {
        /*if (_scene != null)
        {
            _scene.DismissPopWait();
        }*/
    }

    // Start game handler
    public void OnStartGame(int index)
    {
        /*var iteminfo = GlobalUserItem.GetRoomInfo(index);
        Debug.Log($"dengyp onStartGame {index}, {JsonUtility.ToJson(iteminfo)}");

        if (iteminfo != null)
        {
            _scene.OnStartGame(index);
        }*/
    }

    //====================
    // Handle subview button click
    //public void BtnTouched(GameInfo gameinfo, GameObject sender)
    //{
    //    ///*
    //     // ExternalFun.PlayClickEffect();

    //     if (gameinfo == null)
    //     {
    //       //  ShowToast(transform.parent.parent.GetComponent<MonoBehaviour>(),"未找到游戏信息！", 2); // "Game info not found!"
    //         return;
    //     }

    //     // Get app reference
    //    // var app = transform.parent.parent.GetComponent<MyApp>();
    //    // if (app == null) return;

    //     // Get version info
    //   //  int version = app.GetVersionMgr().GetResVersion(gameinfo._KindID);
    //     var targetPlatform = Application.platform;

    //     // Platform check
    //     if (targetPlatform == RuntimePlatform.WindowsPlayer ||
    //         targetPlatform == RuntimePlatform.WindowsEditor)
    //     {
    //         EnterGameInfo(gameinfo);
    //     }
    //     else
    //     {
    //        // Version check logic
    //        bool isAppStoreVersion = false;//appdf.APPSTORE_VERSION; // Assuming this is defined somewhere

    //         if ((version == -1 && !isAppStoreVersion) || version < 0)
    //         {
    //             DownloadGame(gameinfo, sender);
    //             return;
    //         }

    //         if (version == -1 || gameinfo._ServerResVersion > version && !isAppStoreVersion)
    //         {
    //             UpdateGame(gameinfo, sender);
    //         }
    //         else
    //         {
    //             EnterGameInfo(gameinfo);
    //         }
    //     }//*/
    //}

    void EnterGameInfo(GameInfo gameinfo )
    {

        Context.selectIndex = gameinfo.gameid;
       

        Debug.LogError("EnterGameInfo,id:" + gameinfo.gameid);
        
        for (int i = 0; i < Context.selectIndex+1; i++)
        {
            if (!AppRoot.Game[i].Config.IsEditorAssets)
            {
                Debug.LogError("3---EnterGameInfo,i:" + i+",");
                //AppRoot.Game[i].BundleMgr.Test();

                // AppRoot.Game[i].BundleMgr.Clear();
                AppRoot.Game[i].BundleMgr.Init();
            }
        }

        //if(gameinfo.gameid == 0)
        //{
        //    int i = 2;

        //    if (!AppRoot.Game[i].Config.IsEditorAssets)
        //    {
        //        Debug.LogError("2---EnterGameInfo,id:" + gameinfo.gameid);

        //        AppRoot.Game[i].BundleMgr.Clear();
        //       // AppRoot.Game[i].BundleMgr.Init();
        //    }
        //}

        if (mCallback != null)
            mCallback(m_spDownloadBtn[gameinfo.gameid]);
    }

    void ShowToast(string str, int flg)
    {
       // Debug.LogError("ShowToast:" + str);
    }
    

    public void DownloadGame(GameInfo gameinfo, GameObject sender)
    {
        // Check if already in download list
        
        //if (yl.updateList.ContainsKey(gameinfo.gameid))
        //{
        //    ShowToast("正在下载中", 1); // "Downloading..."
        //    return;
        //}

        // var app = _scene.GetApp();
        //        MyApp app = transform.GetComponent<MyApp>();
        //        var cfg = app._serverConfig.game_res_downloadUrl;
        //        string updateUrl = cfg[UnityEngine.Random.Range(0, cfg.Length)];
        /////*
        //        // Build download URL
        //        string fileurl = ""+updateUrl+"/"+gameinfo._Module.Substring(0, gameinfo._Module.Length - 1)+".zip";

        //        // Get filename
        //        int pos = gameinfo._Module.IndexOf('/');
        //        string savename = gameinfo._Module.Substring(pos + 1, gameinfo._Module.Length - pos - 2)+".zip";

        //        // Save path
        //        string savepath = Path.Combine(Application.persistentDataPath, "download/");
        string HotUpdateUrl = "HotUpdate/Room/" + gameinfo._Type + "/";
        //Context.Hall.Config.SetPathParam(_assetUrl + "Hall/", "Hall", true);
       // Debug.LogError("HotUpdateUrl:"+ HotUpdateUrl);
        
        //Debug.LogError( ",Game:" + AppRoot.Get().Game[gameinfo.gameid].Config);
        AppRoot.Game[gameinfo.gameid].Config.SetPathParam($"{SysDefines.OssUrl}/{SysDefines.ZoneId}/{HotUpdateUrl}", "Room/" + gameinfo._Type + "/", false);
      
        //Config.File_List_Name = "ab_file_list.ftxt";

        // Create update
        //var _update = new ClientUpdate(newfileurl, dst, src, downurl, gameinfo.gameid);
        //_update.UpDateClient(this);
        string version = "";
        var _update = sender.AddComponent<AssetUpdater>();
        _update.gameId = gameinfo.gameid;
        _update.onCheckDone = onCheckDone;
        _update.updateComplete = onComplete;
        _update.onProgress = onProgress;
        _update.onError = onError;

        gameinfo.status = (int)mystatus.Check;

       // Debug.LogError("gameinfo.gameid：" + gameinfo.gameid);

        _update.StartUpdate(AppRoot.Game[gameinfo.gameid].Config, version, gameinfo.gameid);

        // yl.updateList[gameinfo.gameid] = _update;

        // Create directory if needed
        //if (!Directory.Exists(savepath))
        //{
        //    Directory.CreateDirectory(savepath);
        //}
        //Debug.LogError("gameid>>" + gameinfo.gameid + ","+ AppRoot.Game[gameinfo.gameid].Config.IsUpdate);
        // Setup UI elements if sender exists
        if (sender != null && AppRoot.Game[gameinfo.gameid].Config.IsUpdate)
        {
            Transform spMask = sender.transform.Find("download_mask");
            if (spMask != null)
            {
                Transform downbg = spMask.Find("downbg");
                if (downbg != null)
                {
                    downbg.gameObject.SetActive(true);

                    // Store reference to the mask
                    m_spDownloadMask[gameinfo.gameid] = spMask.gameObject;
                    spMask.gameObject.SetActive(true);

                    // Store size
                    RectTransform rect = spMask.GetComponent<RectTransform>();
                    m_szMaskSize = rect.sizeDelta;

                    // Setup progress UI
                    Transform spTipLab = downbg.Find("download_mask_tip");
                    Transform spjiantou = downbg.Find("download_mask_jt");
                    Transform spjiantou1 = downbg.Find("download_mask_jt1");

                    // Debug.LogError(">>>>" + spTipLab + "," + spjiantou);
                    if (spTipLab != null)
                    {
                        spTipLab.gameObject.SetActive(true);
                     
                        m_labDownloadTip[gameinfo.gameid] = spTipLab.gameObject;

                        spTipLab.GetComponent<Text>().text = "需要下载";
                    }

                    if (spjiantou != null)
                    {
                        spjiantou.gameObject.SetActive(true);
                       // this.GetType().GetField($"m_labDownloadjt{gameinfo.gameid}")?.SetValue(this, spjiantou.gameObject);

                        m_labDownloadjt[gameinfo.gameid] = spjiantou.gameObject;
                    }

                    if (spjiantou1 != null)
                    {
                        spjiantou1.gameObject.SetActive(false);
                        // this.GetType().GetField($"m_labDownloadjt{gameinfo.gameid}")?.SetValue(this, spjiantou.gameObject);

                        m_labDownloadjt1[gameinfo.gameid] = spjiantou1.gameObject;
                    }
                }
            }
        }

        // Start download
        //StartCoroutine(DownloadGameZip(fileurl, Path.Combine(savepath, savename), gameinfo.gameid));
    }
    
    private IEnumerator DownloadGameZip(string url, string savePath, int gameId)
    {
        yield return new WaitForSeconds(1f);
        /* 
        using (UnityWebRequest www = UnityWebRequest.Get(url))
         {
             www.downloadHandler = new DownloadHandlerFile(savePath);
             www.SendWebRequest();

             while (!www.isDone)
             {
                 // Update progress
                 float progress = www.downloadProgress * 100f;
                 UpdateDownloadProgress(gameId, progress);
                 yield return null;
             }

             if (www.isNetworkError || www.isHttpError)
             {
                 ShowToast("下载失败:"+www.error, 2); // "Download failed"
                                                           // Clean up failed download
                 if (File.Exists(savePath))
                 {
                     File.Delete(savePath);
                 }
             }
             else
             {
                 // Extract zip file and handle completion
                 HandleDownloadComplete(gameId, savePath);
             }
         }//*/
    }

    private void UpdateDownloadProgress(int gameId, float progress)
    {
        // /*
        var maskField = m_spDownloadMask[ gameId];
        var spMask = maskField as GameObject;
       // Debug.LogError("进度条 maskField:" + maskField);
        if (spMask != null)
        {
            var tipField = m_labDownloadTip[ gameId];
            var labDownloadTip = tipField as GameObject;
            Debug.LogError("进度条 tipField:" + tipField+"," + Mathf.FloorToInt(progress) + "%");
            if (labDownloadTip != null)
            {
                labDownloadTip.GetComponent<Text>().text =""+ Mathf.FloorToInt(progress)+"%";
            }
        }//*/
    }

    private void HandleDownloadComplete(int gameId, string zipPath)
    {
       ///* 
        try
        {
            string extractPath = Path.Combine(Application.persistentDataPath, "games/");
           // ZipFile.ExtractToDirectory(zipPath, extractPath);
            File.Delete(zipPath);

            // Update game status
            yl.updateList.Remove(gameId);
            ShowToast("下载完成", 1); // "Download complete"
        }
        catch (Exception ex)
        {
           // Debug.LogError($"Extraction failed: {ex.Message}");
            ShowToast( "安装失败", 2); // "Installation failed"
        }//*/
    }

   
}