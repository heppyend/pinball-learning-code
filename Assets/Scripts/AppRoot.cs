using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class AppRoot : MonoBehaviour
{
    //集合的运行环境
    public static Context Hall = new Context();

    //小游戏的运行环境
    public static Context[] Game;

    [SerializeField]
    public UpdateType _updateType = UpdateType.CurrentPlatform;
#if HALL || UNITY_WEBGL
    [SerializeField]
    private AssetSourceType _hallAssetSource = AssetSourceType.LocalAssets;
#endif
    [SerializeField]
    public AssetSourceType _gameAssetSource = AssetSourceType.LocalAssets;

    private static AppRoot _instance;
    public static AppRoot Get()
    {
        return _instance;
    }

    private static string _assetUrl = string.Empty;
    public string AssetUrl()
    {
        return _assetUrl;
    }

    public bool IsRunInHall()
    {
#if HALL || UNITY_WEBGL
        return true;
#else
        return false;
#endif
    }

    public void HotUpdate(string version)
    {
        InitContext();
        Debug.LogWarning("HotUpdate:"+ AppRoot.Hall.Config.IsUpdate);
        if (AppRoot.Hall.Config.IsUpdate)
            StartUpdate(version);
        else
            OnABUpdateComplete(0);
    }

    public void StartHotUpdateDownload()
    {
        gameObject.GetComponent<AssetUpdater>().StartHotUpdateDownload();
    }

    public void AddHotUpdateListener(Action<int,int,int> onCheckDone, Action<ProgressType, int, int,int> onProgress,
        Action<int, string,int> onError, Action<int> onComplete)
    {
        this.onCheckDone = onCheckDone;
        this.onProgress = onProgress;
        this.onError = onError;
        this.onComplete = onComplete;
    }

    private int curGameGroup = 0;
    public int curState = 0;       //0大厅 1进入小游戏
    public void HallToGame(int gameId)
    {
#if HALL || UNITY_WEBGL
        Debug.Log($"HallToGame id = {gameId}");
        var table = THotUpdateHelper.DataMap.Values.Where(t => t.GameId == gameId).ToList().Last();
        if (curState == 1) return;
        GoToGame(table);
#endif
    }

    public void GoToGame(THotUpdate table) {
        ModuleManager.Instance.Get<LoginModule>().AccessServiceReq(table.GameGroupType, EnumAccessServiceType.JoinGame, gameData => {
            curGameGroup = table.GameGroupType;

            if (!AppRoot.Hall.Config.IsEditorAssets)
                AppRoot.Hall.BundleMgr.Clear();

           // Context.Game.Config.SetPathParam($"{SysDefines.OssUrl}/{SysDefines.ZoneId}/{table.Url}", table.NameEN, false);
           // Context.Game.BundleMgr.Init();
           // Context.Game.LuaClient.Init(Context.Game);
          //  if (!string.IsNullOrEmpty(gameData))
          //      Context.Game.LuaClient.OnGameReconnected(gameData);

            curState = 1;

            if (table.ScreenOrientation == 2) {
                Screen.orientation = ScreenOrientation.Portrait;
                GameController.Instance.MainCanvasScaler.referenceResolution = new Vector2(2160, 3840);
                MessageCenter.Instance.SendMessage(MsgType.CLIENT_INNERGAME_OPENCLOSE, null);
            }

            UIManager.Instance.CloseUI(EnumUIType.LobbyUI);
        });
    }

    public void GameToHall()
    {
#if HALL || UNITY_WEBGL
        Debug.Log("GameToHall");

        ModuleManager.Instance.Get<LoginModule>().AccessServiceReq(curGameGroup, EnumAccessServiceType.QuitGame, gameData => 
        {
            curGameGroup = 0;

           // Context.Game.BundleMgr.Clear();
           // Context.Game.LuaClient.Destroy();

            if (!AppRoot.Hall.Config.IsEditorAssets)
                AppRoot.Hall.BundleMgr.Init();
                       
            //if (!Context.Hall.Config.IsEditorAssets)
            //{
            //    var bundleMgr = Context.Hall.BundleMgr;
            //    var bundleName = bundleMgr.GetAssetBundleName("Assets/Scenes/MainScene.unity");
            //    bundleMgr.LoadAssetBundle(bundleName);
            //}
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
            // AudioManager.Instance.PlayMusic(SysDefines.AUDIO + "main");

            curState = 0;

            Screen.orientation = ScreenOrientation.Portrait;
            GameController.Instance.MainCanvasScaler.referenceResolution = new Vector2(2160, 3840);
            MessageCenter.Instance.SendMessage(MsgType.CLIENT_INNERGAME_OPENCLOSE, null);

            UIManager.Instance.OpenUI(EnumUIType.LobbyUI);
        });
#endif
    }

    public void ForceQuit()
    {
        curGameGroup = 0;

       // Context.Game.BundleMgr.Clear();
       // Context.Game.LuaClient.Destroy();

        if (!AppRoot.Hall.Config.IsEditorAssets)
            AppRoot.Hall.BundleMgr.Init();

        curState = 0;
    }

    private void Start()
    {
        if (!ReferenceEquals(Get(), null))
        {
          //  GetComponent<XLuaMain>().enabled = false;
          Debug.LogWarning("AppRoot = >>>enabled = false," + this.transform.name);
            //enabled = false;
            return;
        }

        Debug.LogWarning("AppRoot = >>>"+this.transform.name);

        _instance = this;

        //InitContext();

#if HALL || UNITY_WEBGL
        //HotUpdate();      //临时放此处,应由登录代码控制什么时候启动热更新
#endif
    }

    private void InitContext()
    {
#if HALL
        // SysDefines.OssUrl = "https://dingyoubuyu-oss.oss-cn-shenzhen.aliyuncs.com";//"https://game.hofoo.top";
        SysDefines.OssUrl = "https://localpinball-oss.oss-cn-shenzhen.aliyuncs.com";//"https://game.hofoo.top";
        //Debug.LogError("InitContext "+ _updateType+","+ _hallAssetSource+ SysDefines.OssUrl+","+SysDefines.ZoneId+"，"+ SysDefines.HotUpdateUrl);

        AppRoot.Hall.Config = new AssetConfig();
        AppRoot.Hall.Config.SetAssetType(_updateType, _hallAssetSource);
        //Context.Hall.Config.SetPathParam(_assetUrl + "Hall/", "Hall", true);
        AppRoot.Hall.Config.SetPathParam($"{SysDefines.OssUrl}/{SysDefines.ZoneId}/{SysDefines.HotUpdateUrl}", "Hall", true);
        AppRoot.Hall.Loader = new AssetLoader(AppRoot.Hall);
        AppRoot.Hall.BundleMgr = new BundleManager(AppRoot.Hall);

      //  if (Context.Hall.LuaClient == null)
      //      Context.Hall.LuaClient = GetComponent<XLuaMain>();

        //Context.Game.Config = new AssetConfig();
        //if (_gameAssetSource == AssetSourceType.LocalAssets)
        //    _gameAssetSource = AssetSourceType.UpdateAssetBundle;   //大厅里启动小游戏只能通过热更新
        //Context.Game.Config.SetAssetType(_updateType, _gameAssetSource);
        //Context.Game.BundleMgr = new BundleManager(Context.Game);
      //  if (Context.Game.LuaClient == null)
      //      Context.Game.LuaClient = gameObject.AddComponent<XLuaMain>();
#else
        Context.Game.Config = new AssetConfig();
        Context.Game.Config.SetAssetType(_updateType, _gameAssetSource);
        Context.Game.Config.SetPathParam(_assetUrl + "Game/", "game", true);
        Context.Game.Loader = new AssetLoader(Context.Game);
        Context.Game.BundleMgr = new BundleManager(Context.Game);
        if (!Context.Game.Config.IsEditorAssets)
            Context.Game.BundleMgr.Init();
       // Context.Game.LuaClient = GetComponent<XLuaMain>();
       // Context.Game.LuaClient.Init(Context.Game);
#endif
    }

    private void StartUpdate(string version)
    {
        var updater = gameObject.AddComponent<AssetUpdater>();
        updater.onCheckDone = OnCheckDone;
        updater.updateComplete = OnABUpdateComplete;
        updater.onProgress = OnProgress;
        updater.onError = OnError;
        updater.StartUpdate(AppRoot.Hall.Config, version);
    }

    private Action<int,int,int> onCheckDone;
    private Action<ProgressType, int, int,int> onProgress;
    private Action<int, string,int> onError;
    private Action<int> onComplete;
    private async void OnABUpdateComplete1(int gameid)
    {
        Debug.LogError("OnABUpdateComplete????"+ AppRoot.Hall.Config.IsEditorAssets);
        //UnityMainThreadDispatcher.ExecuteOnMainThread(() => { 

        //AppRoot.Hall.Config.SetAssetType(UpdateType.Android, AssetSourceType.LocalAssets);
        if (!AppRoot.Hall.Config.IsEditorAssets)
           await AppRoot.Hall.BundleMgr.Init1();

            // Context.Hall.LuaClient.Init(Context.Hall);
            //Debug.LogError("OnABUpdateComplete," + onComplete);
            //await Task.Delay(100);
        onComplete?.Invoke(gameid);
        //});
    }

    private void OnABUpdateComplete(int gameid)
    {
        Debug.LogWarning("OnABUpdateComplete????" + AppRoot.Hall.Config.IsEditorAssets);
        //UnityMainThreadDispatcher.ExecuteOnMainThread(() => { 

        //AppRoot.Hall.Config.SetAssetType(UpdateType.Android, AssetSourceType.LocalAssets);
        if (!AppRoot.Hall.Config.IsEditorAssets)
            AppRoot.Hall.BundleMgr.Init();

        // Context.Hall.LuaClient.Init(Context.Hall);
        //Debug.LogError("OnABUpdateComplete," + onComplete);
        //await Task.Delay(100);
        onComplete?.Invoke(gameid);
        //});
    }

    private void OnCheckDone(int totalBytes, int totalBytes1, int gameid)
    {
        onCheckDone?.Invoke(totalBytes, totalBytes1, gameid);
    }

    private void OnProgress(ProgressType type, int loaded, int total,int gameid)
    {
        onProgress?.Invoke(type, loaded, total,gameid);
    }

    private void OnError(int errorCode, string error,int gameid)
    {
        onError?.Invoke(errorCode, error,gameid);
    }
}
