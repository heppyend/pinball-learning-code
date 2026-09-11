using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

public enum IPType
{
    LocalIP,WinIP,LinuxIP
}

public class Init : MonoBehaviour
{
    private ResourcePackage _package;

    /// <summary>
    /// 资源系统运行模式
    /// </summary>
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    public string DefaultHostServer;
    public string FallbackHostServer;
    public bool UpdateUrl;

    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL/微信小游戏运行时不支持 Editor Simulate 模式。
        // 保留 Inspector 的编辑器默认值，运行时改用 WebGL 专用初始化分支。
        PlayMode = EPlayMode.WebPlayMode;
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += ApplyWebGLRenderCompatibility;
#endif
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
    }

    private void Start()
    {
       // Debug.Log("this is Init: PlayMode:"+ PlayMode);

        if (PlayMode == EPlayMode.EditorSimulateMode)
            InitPackage();
        else
        {
            //IngameDebugConsole.SetActive(true);
            StartCoroutine(OnStart());
            Helper helper = new Helper();
        }
    }

    IEnumerator OnStart()
    {
        // 初始化事件系统
        //UniEvent.Initalize();
        //1.初始化
        Debug.Log("1. 初始化");
        // 初始化资源系统
        YooAssets.Initialize();

        // 创建默认的资源包
        var package = YooAssets.CreatePackage("DefaultPackage");

        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(package);

        //2.资源系统的运行模式
        Debug.Log(DefaultHostServer);
        Debug.Log("2. 资源系统的运行模式");
        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            var initParameters = new EditorSimulateModeParameters();//EditorSimulateModeParameters继承自InitializeParameters
            string simulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline.ToString(), "DefaultPackage");
            initParameters.SimulateManifestFilePath = simulateManifestFilePath;
            yield return package.InitializeAsync(initParameters);
        }
        else if (PlayMode == EPlayMode.OfflinePlayMode)
        {
            var initParameters = new OfflinePlayModeParameters();//OfflinePlayModeParameters继承自InitializeParameters
            initParameters.DecryptionServices = new FileOffsetDecryption();//需要补充这个
            yield return package.InitializeAsync(initParameters);
        }
        else if (PlayMode == EPlayMode.HostPlayMode)
        {
            // 注意：GameQueryServices.cs 太空战机的脚本类，详细见StreamingAssetsHelper.cs
            string defaultHostServer = DefaultHostServer;
            string fallbackHostServer = FallbackHostServer;
            //defaultHostServer = fallbackHostServer = "https://a.unity.cn/client_api/v1/buckets/ed440155-7aad-48a9-aeab-552fdcd26deb/content/StreamingAssets/1";
            var initParameters = new HostPlayModeParameters();//HostPlayModeParameters继承自InitializeParameters
            initParameters.BuildinQueryServices = new GameQueryServices();//内置资源查询服务接口
            initParameters.DecryptionServices = new FileOffsetDecryption();//如果资源包在构建的时候有加密，需要提供实现IDecryptionServices接口的实例类。
            initParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);//远端服务器查询服务接口
            var initOperation = package.InitializeAsync(initParameters);
            yield return initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
            {
                Debug.Log("资源包初始化成功！");
            }
            else
            {
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
            }
        }
        else if (PlayMode == EPlayMode.WebPlayMode)
        {
            YooAssets.SetCacheSystemDisableCacheOnWebGL();
            string defaultHostServer = DefaultHostServer;
            string fallbackHostServer = FallbackHostServer;
            //fallbackHostServer=defaultHostServer = "11";
            //string defaultHostServer = "http://127.0.0.1/CDN/WebGL/V1.0";
            //string fallbackHostServer = "http://127.0.0.1/CDN/WebGL/V1.0";
            //defaultHostServer = fallbackHostServer = "https://a.unity.cn/client_api/v1/buckets/ed440155-7aad-48a9-aeab-552fdcd26deb/content/StreamingAssets/1";
            var initParameters = new WebPlayModeParameters();//WebPlayModeParameters继承自InitializeParameters
            initParameters.BuildinQueryServices = new GameQueryServices();
            initParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            var initOperation = package.InitializeAsync(initParameters);
            yield return initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
            {
                Debug.Log("资源包初始化成功！");
            }
            else
            {
                Debug.LogError($"资源包初始化失败：{initOperation.Error}");
            }
        }

        //3.获取资源版本：UpdatePackageVersionAsync
        Debug.Log("3. 获取资源版本");
        var operation = package.UpdatePackageVersionAsync(false);
        yield return operation;

        if (operation.Status == EOperationStatus.Succeed)
        {

            string packageVersion = operation.PackageVersion;
            Debug.Log($"Updated package Version : {packageVersion}");

            //4.更新资源清单：对于联机运行模式，在获取到资源版本号之后，就可以更新资源清单了：UpdatePackageManifestAsync
            //联机运行模式
            //通过传入的清单版本，优先比对当前激活清单的版本，如果相同就直接返回成功。如果有差异就从缓存里去查找匹配的清单，如果缓存里不存在，就去远端下载并保存到沙盒里。最后加载沙盒内匹配的清单文件。
            Debug.Log("4. 更新资源清单");
            bool savePackageVersion = true;
            var operation2 = package.UpdatePackageManifestAsync(packageVersion, savePackageVersion);

            yield return operation2;

            if (operation2.Status == EOperationStatus.Succeed)
            {
                //5.资源包下载
                Debug.Log("5. 资源包下载");
                //yield return Download();
                
#if UNITY_EDITOR

                Debug.Log("6.编辑器模式下，直接查找程序集");
                // 编辑器模式下，直接进入
                yield return StartCoroutine(LoadDll(package));
                Assembly hotUpdateAss = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotFix");
#else
               Debug.Log("6. 强制加载可能缺失的系统程序集");
                yield return StartCoroutine(PreloadSystemAssemblies());
               yield return StartCoroutine(LoadDll(package));


#endif
                Debug.Log("加载Init");
                const string firstSceneName = "LoginScene";
                var sceneOperation = YooAssets.LoadSceneAsync(firstSceneName);
                yield return sceneOperation;

#if UNITY_WEBGL && !UNITY_EDITOR
                // 微信小游戏 WebGL 不支持当前 URP 后处理使用的若干 Shader。
                // 先处理当前已创建的相机；后续异步实例化的 Canvas 相机由 onPreCull 回调处理。
                yield return null;
                ApplyWebGLRenderCompatibilityToLoadedCameras();
                var diagnosticsHost = new GameObject("WebGLFirstScreenDiagnostics");
                UnityEngine.Object.DontDestroyOnLoad(diagnosticsHost);
                diagnosticsHost.AddComponent<WebGLFirstScreenDiagnosticsHost>().Begin();
#endif
            }
            else
            {
                //更新失败
                Debug.LogError(operation.Error);
            }
        }
        else
        {
            //更新失败
            Debug.LogError(operation.Error);
        }
    }

    private IEnumerator LoadDll(ResourcePackage package)
    {
        //6.加载热更代码,YooAsset2.0版本rawfile文件需要用RawFile管线单独打包,否则用LoadRawFileAsync时候会报错
        //把热更代码改成txt后缀用textasset类型读取可解决
        Debug.Log("6.加载补充元数据与热更代码>>");
        //补充元数据
        yield return StartCoroutine(LoadMetadataForAOTAssemblies(package));
        //热更代码
        var codeHandle = package.LoadAssetAsync<TextAsset>("HotFix.dll");
        yield return codeHandle;
        TextAsset textAsset = codeHandle.AssetObject as UnityEngine.TextAsset;
        //Debug.Log(textAsset);
        //Debug.Log(textAsset.bytes.Length);
        Debug.Log(Application.persistentDataPath);
        Assembly hotUpdateAss = Assembly.Load(textAsset.bytes);
        //Type type = hotUpdateAss.GetType("HotUpdateEntry");
        //type.GetMethod("EntryGame").Invoke(null, null);
    }

    private IEnumerator PreloadSystemAssemblies()
    {
        // 强制加载可能缺失的系统程序集
        var systemRuntime = typeof(System.Runtime.CompilerServices.YieldAwaitable).Assembly;
        var systemThreading = typeof(System.Threading.Tasks.Task).Assembly;
        var systemCollections = typeof(System.Collections.Generic.List<>).Assembly;

        Debug.Log("系统程序集预加载完成");
        yield return null;
    }

    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private IEnumerator LoadMetadataForAOTAssemblies(ResourcePackage package)
    {
        List<string> AOTMetaAssemblyFiles = new List<string>()
        {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll",
        "Demigiant.dll",
        "YooAsset.dll",
        "LC.Newtonsoft.Json.dll",
#if !UNITY_WEBGL
        "Unity.Timeline.dll",
#endif
        };

        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyFiles)
        {
            var codeHandle = package.LoadAssetAsync<TextAsset>(aotDllName);
            yield return codeHandle;
            TextAsset textAsset = codeHandle.AssetObject as UnityEngine.TextAsset;
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private static readonly HashSet<int> WebGLRenderCompatibleCameraIds = new HashSet<int>();

    private static void ApplyWebGLRenderCompatibilityToLoadedCameras()
    {
        var cameras = FindObjectsOfType<Camera>(true);
        foreach (var camera in cameras)
        {
            ConfigureWebGLRenderCompatibility(camera);
        }

        Debug.Log($"WebGL 渲染兼容性已注册：已检查 {cameras.Length} 台相机；后续相机将在 URP 渲染前处理。");
    }

    private static void ApplyWebGLRenderCompatibility(UnityEngine.Rendering.ScriptableRenderContext context, Camera camera)
    {
        ConfigureWebGLRenderCompatibility(camera);
    }

    private static void ConfigureWebGLRenderCompatibility(Camera camera)
    {
        if (camera == null || !WebGLRenderCompatibleCameraIds.Add(camera.GetInstanceID()))
            return;

        camera.allowHDR = false;

        var additionalCameraData = camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (additionalCameraData != null)
            additionalCameraData.renderPostProcessing = false;

        Debug.Log($"WebGL 渲染兼容性已应用：{camera.name}");
    }

    // 仅用于定位微信小游戏与 Unity Editor 首屏不一致的问题。
    // 该协程不写入任何相机、Canvas、材质或 UI 状态；确认根因后可整体移除。
    private static void LogWebGLFirstScreenSnapshot(string checkpoint)
    {
        var activeScene = SceneManager.GetActiveScene();
        var cameras = FindObjectsOfType<Camera>(true);
        var canvases = FindObjectsOfType<Canvas>(true);
        var renderPipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        var skybox = RenderSettings.skybox;
        var reflection = RenderSettings.customReflectionTexture;

        Debug.Log($"[WebGL 首屏诊断 {checkpoint}] scene={activeScene.name}, loaded={activeScene.isLoaded}, " +
                  $"resolution={Screen.width}x{Screen.height}, cameras={cameras.Length}, canvases={canvases.Length}, " +
                  $"pipeline={(renderPipeline == null ? "Builtin" : renderPipeline.name)}, " +
                  $"skybox={(skybox == null ? "none" : skybox.name)}, " +
                  $"reflection={(reflection == null ? "none" : reflection.name)}");

        foreach (var camera in cameras)
        {
            var cameraData = camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            Debug.Log($"[WebGL 首屏相机 {checkpoint}] name={camera.name}, active={camera.gameObject.activeInHierarchy}, " +
                      $"enabled={camera.enabled}, depth={camera.depth}, clear={camera.clearFlags}, " +
                      $"rect={camera.rect}, target={(camera.targetTexture == null ? "screen" : camera.targetTexture.name)}, " +
                      $"hdr={camera.allowHDR}, post={(cameraData != null && cameraData.renderPostProcessing)}");
        }

        foreach (var canvas in canvases)
        {
            Debug.Log($"[WebGL 首屏画布 {checkpoint}] name={canvas.name}, active={canvas.gameObject.activeInHierarchy}, " +
                      $"enabled={canvas.enabled}, mode={canvas.renderMode}, sorting={canvas.sortingOrder}, " +
                      $"camera={(canvas.worldCamera == null ? "none" : canvas.worldCamera.name)}, " +
                      $"root={canvas.rootCanvas.name}");
        }
    }

    private sealed class WebGLFirstScreenDiagnosticsHost : MonoBehaviour
    {
        internal void Begin()
        {
            StartCoroutine(LogWebGLFirstScreenDiagnostics());
        }

        private IEnumerator LogWebGLFirstScreenDiagnostics()
        {
            yield return new WaitForSecondsRealtime(1f);
            LogWebGLFirstScreenSnapshot("1s");
            yield return new WaitForSecondsRealtime(9f);
            LogWebGLFirstScreenSnapshot("10s");
            Destroy(gameObject);
        }
    }
#endif

    IEnumerator Download()
    {
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        var package = YooAssets.GetPackage("DefaultPackage");

        //创建资源下载器，下载所有资源
        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

        //没有需要下载的资源
        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("没有需要下载的资源");
            Debug.Log("TotalDownloadCount = 0");

            yield break;
        }

        //需要下载的文件总数和总大小
        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;

        //注册回调方法
        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
        downloader.OnDownloadOverCallback = OnDownloadOverFunction;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

        //开启下载
        downloader.BeginDownload();
        yield return downloader;

        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            Debug.Log("Finish");
        }
        else
        {
            Debug.Log("DownLoad Failed");
        }
    }

    private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
    {
        Debug.Log("fileName:" + fileName + ",sizeBytes:" + sizeBytes);
    }

    private void OnDownloadOverFunction(bool isSucceed)
    {
        Debug.Log("isSucceed");
        //EventCenter.Instance.EventTrigger("下载完成");
    }

    private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        Debug.Log("totalDownloadCount:" + totalDownloadCount + ",currentDownloadCount" + currentDownloadCount + ",totalDownloadBytes:" + totalDownloadBytes + ",currentDownloadBytes" + currentDownloadBytes);
        double progress = (double)currentDownloadBytes / totalDownloadBytes;
        //EventCenter.Instance.EventTrigger("更新下载进度", (float)progress);
        Debug.LogError(progress);
    }

    private void OnDownloadErrorFunction(string fileName, string error)
    {
        Debug.Log("DownloadError:" + fileName + ",error:" + error);
    }

    /// <summary>
    /// 资源文件偏移加载解密类
    /// </summary>
    private class FileOffsetDecryption : IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = null;
            return AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
        }

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
        {
            managedStream = null;
            return AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
        }

        private static ulong GetFileOffset()
        {
            return 32;
        }

    }

    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }





    public void InitPackage()
    {
        YooAssets.Initialize();
        // 创建默认的资源包
        string packageName = "DefaultPackage";
        _package = YooAssets.TryGetPackage(packageName);
        if (_package == null)
        {
            _package = YooAssets.CreatePackage(packageName);

            if (PlayMode == EPlayMode.EditorSimulateMode)
            {
                var initParameters = new EditorSimulateModeParameters();//EditorSimulateModeParameters继承自InitializeParameters
                string simulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline.ToString(), "DefaultPackage");
                initParameters.SimulateManifestFilePath = simulateManifestFilePath;
                var _async = _package.InitializeAsync(initParameters);

                //创建资源包初始化参数（用来定义资源路径）
                _async.Completed += OnCompleted;
                YooAssets.SetDefaultPackage(_package);
            }
        }
    }

    private void OnCompleted(AsyncOperationBase obj)
    {
        //SceneManager.LoadScene("LoginScene");
        YooAssets.LoadSceneAsync("LoginScene");
    }

}
