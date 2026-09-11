
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

using Spine.Unity;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using YooAsset;
//using UnityEngine.AddressableAssets;
//using UnityEngine.ResourceManagement.AsyncOperations; // 包含 AsyncOperationHandle

public class AssetInfo<T> where T : UnityEngine.Object
{
    private T loadObj;
    public string Path { get; set; }
    public int RefCount { get; set; }

    public string Ext { get; set; }

    public bool IsLoaded
    {
        get
        {
            return !ReferenceEquals(loadObj, null);
        }
    }
    public T AssetObject
    {
        get
        {
            if (ReferenceEquals(loadObj, null))
                resourcesLoad();
            return loadObj;
        }
    }

    //public T AssetObject1
    //{
    //    get
    //    {
    //        if (ReferenceEquals(loadObj, null))
    //            CoroutineController.Instance.StartCoroutine(ResourcesLoadAsync(null));
    //        return loadObj;
    //    }
    //}
    #region public function
    //协程加载
    public IEnumerator GetObjectByCoroutine(Action<T> loaded)
    {
        while (ReferenceEquals(loadObj, null))
        {
            yield return null;
            resourcesLoad();
        }
        loaded?.Invoke(loadObj);
    }

    //异步加载
    public IEnumerator GetObjectAsync(Action<T> loaded)
    {
        return GetObjectAsync(loaded, null);
    }

    public IEnumerator GetObjectAsync(Action<T> loaded, Action<float> progress)
    {
        if (!ReferenceEquals(loadObj, null))
        {
            loaded?.Invoke(loadObj);
            yield break;
        }
        var request = Resources.LoadAsync(Path);
        if (!ReferenceEquals(progress, null))
        {
            while (!request.isDone)
            {
                progress(request.progress);
            }
        }
        yield return request;
        if (ReferenceEquals(request.asset, null))
        {
            //loadObj = Context.Hall.Loader.LoadAsset<T>($"Assets/Resources_HotUpdate/{Path}.prefab");
            // Path.StartsWith(SysDefines.ROOMPATH)
            if (false)
            {
                string[] array = Path.Split("/");
                int index = int.Parse(array[1]);

                Debug.LogError("Path:" + Path);
                //if(index == 3)
                //{
                //    index = 1;
                //}

                loadObj = AppRoot.Game[index - 1].Loader.LoadAsset<T>(SysDefines.HOTUPDATEPATH + $"{Path}{Ext}");
            }
            else
                loadObj = AppRoot.Hall.Loader.LoadAsset<T>(SysDefines.HOTUPDATEPATH + $"{Path}{Ext}");
            if (ReferenceEquals(loadObj, null))
                Debug.LogErrorFormat($"Resources Load Failure! Path:{Path}");
            else
            {
                loadObj = request.asset as T;
                loaded?.Invoke(loadObj);
                yield return request;
            }
        }
        else
        {
            loadObj = request.asset as T;
            loaded?.Invoke(loadObj);
            yield return request;
        }

        //if (ReferenceEquals(request.asset, null))
        //    Debug.LogErrorFormat($"Resources Load Failure! Path:{Path}");
        //else
        //{
        //    loadObj = request.asset as T;
        //    loaded?.Invoke(loadObj);
        //    yield return request;
        //}
    }
    #endregion
    // 新添加：异步版本的 resourcesLoad
    public IEnumerator ResourcesLoadAsync(Action<T> callback)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string location = Path;
        if (location.StartsWith(SysDefines.BNUIPREFAB, StringComparison.Ordinal))
            location = location.Substring(SysDefines.BNUIPREFAB.Length);

        AssetHandle yooHandle = YooAssets.LoadAssetAsync<T>(location);
        yield return yooHandle;
        if (yooHandle.Status == EOperationStatus.Succeed)
            loadObj = yooHandle.AssetObject as T;
        else
            Debug.LogError($"[WebGL资源加载] YooAsset异步加载失败：Path={Path}, Location={location}, Error={yooHandle.LastError}");
        callback?.Invoke(loadObj);
        yield break;
#endif
		//  try
		//{
		//  Debug.LogError("==================ResourcesLoadAsync:" + Path);
		// 计时点1：Resources.LoadAsync
		System.Diagnostics.Stopwatch sw1 = System.Diagnostics.Stopwatch.StartNew();
		sw1.Start();
		// 1. 先尝试 Resources 异步加载
		var resourceRequest = Resources.LoadAsync<T>(Path);
            yield return resourceRequest;
		sw1.Stop();
		//Debug.LogError($"0---Resources.LoadAsync耗时: {sw1.ElapsedMilliseconds}ms");

		loadObj = resourceRequest.asset as T;

        //sw1.Restart();

        // 2. 如果 Resources 加载失败，走热更新异步路径
        if (ReferenceEquals(loadObj, null))
            {
                string fullPath = SysDefines.HOTUPDATEPATH + $"{Path}{Ext}";

			//Debug.LogError("==================loadObj:Path:" + Path);

            // Path.StartsWith(SysDefines.ROOMPATH)
			if (false)
                {
                    string[] array = Path.Split("/");
                    string name = array[1].Replace("Room", "");
                    int index = int.Parse(name);

                    var loader = AppRoot.Game[index - 1].Loader;

                    if (index <= Context.selectIndex + 1 || index == 3)
                    {
                        // 使用 AssetLoader 的异步方法
                        yield return loader.LoadAssetAsync<T>(fullPath, (asset) => {
                            loadObj = asset;
                        });
                    }
                }
                else
                {
                //   System.Diagnostics.Stopwatch loaderSw = System.Diagnostics.Stopwatch.StartNew();
              //  sw1.Restart();
                var loader = AppRoot.Hall.Loader;
               // sw1.Stop();
                //Debug.LogError($"获取Loader耗时: {sw1.ElapsedMilliseconds}ms");

                //yield return null;
                sw1.Restart();

                // 使用 AssetLoader 的异步方法
                yield return loader.LoadAssetAsync<T>(fullPath, (asset) => {
                    sw1.Stop();

                    //Debug.LogError($"================Resources.LoadAsync耗时: ={sw1.ElapsedMilliseconds}ms,=fullPath:{fullPath}");
					loadObj = asset;
                });
                }
            }

      //  Debug.LogError($"Resources.LoadAsync loadObj: {loadObj}");

		//sw1.Stop();
		//Debug.LogError($"Resources.LoadAsync耗时: {sw1.ElapsedMilliseconds},{loadObj}ms");

		callback?.Invoke(loadObj);
		//}
		//catch (Exception e)
		//{
		//    Debug.LogError(e.ToString());
		//    callback?.Invoke(null);
		//}
	}

#region private function
    private void resourcesLoad()
    {
        try
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL 不支持同步等待；大厅 UI 由 ResourcesLoadAsync 异步加载。
            string location = Path;
            if (location.StartsWith(SysDefines.BNUIPREFAB, StringComparison.Ordinal))
                location = location.Substring(SysDefines.BNUIPREFAB.Length);
            Debug.LogError($"[WebGL资源加载] WebGL禁止同步加载：Path={Path}, Location={location}");
            return;
#endif
            //loadObj = Resources.Load<T>(Path);
            //if (ReferenceEquals(loadObj, null))
            // Debug.LogErrorFormat($"Resources Load Failure! Path:{Path}");
            //Debug.LogError("resourcesLoad:0");
            loadObj = Resources.Load<T>(Path);
           // Debug.LogError("resourcesLoad:" + Path);
            if (ReferenceEquals(loadObj, null))
            {
                loadObj = AppRoot.Hall.Loader.LoadAsset<T>(SysDefines.HOTUPDATEPATH + $"{Path}{Ext}");
                return;
                // Debug.LogError("resourcesLoad:1");
                // Path.StartsWith(SysDefines.ROOMPATH)
                if (true)
                {
                    string[] array = Path.Split("/");
                    string name = array[1].Replace("Room", "");
                  //  Debug.LogError("resourcesLoad:," + array[1] + "," + name);
                    int index = int.Parse(name);

                    //Debug.LogError("index:" + index+","+ Context.selectIndex + ",IsEditorAssets?" + AppRoot.Game[index - 1].Config.IsEditorAssets);
                    if (index <= Context.selectIndex + 1)
                    {
                        loadObj = AppRoot.Game[index - 1].Loader.LoadAsset<T>(SysDefines.HOTUPDATEPATH + $"{Path}{Ext}");
                    }
                    else
                    {
                        //if (index == 3)
                        //{
                        //    loadObj = AppRoot.Game[index - 1].Loader.LoadAsset<T>(SysDefines.HOTUPDATEPATH + $"{Path}{Ext}");
                        //}else if (index == 5)
                        //{
                        //    loadObj = AppRoot.Game[index - 1].Loader.LoadAsset<T>(SysDefines.HOTUPDATEPATH + $"{Path}{Ext}");
                        //}
                    }
                }
                else
                {
                    
                   // loadObj = CoroutineController.Instance.StartCoroutine(AppRoot.Hall.Loader.LoadAssetAsync<T>(SysDefines.HOTUPDATEPATH + $"{Path}{Ext}"));
                }
            }
            if (ReferenceEquals(loadObj, null))
                Debug.LogErrorFormat($"Resources Load Failure! Path:{SysDefines.HOTUPDATEPATH}{Path}{Ext}");
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    #endregion
}


public class ResManager : Singleton<ResManager>
{
    //资源缓存集合
    private Hashtable hashTable;

    //初始化
    public override void Init()
    {
        hashTable = new Hashtable();
    }

    #region public function

    public bool HasCache<T>(string path)where T:UnityEngine.Object
    {
        string hashKey = string.Format($"{path}{typeof(T)}");
        return hashTable.ContainsKey(hashKey);
    }

    //load
    public GameObject LoadPrefab(string path)
    {
        //  Debug.LogError("创建预制体:"+ path);

        return Load<GameObject>(path);
    }
   

    // 在 ResManager 中添加
    public IEnumerator LoadPrefabAsyncCoroutine(string path, Action<GameObject> callback)
    {
        var info = getAssetInfo<GameObject>(path, ".prefab");
        if (!ReferenceEquals(info, null))
        {
            var assetInfo = info as AssetInfo<GameObject>;

            bool isDone = false;
            GameObject result = null;

            assetInfo.GetObjectAsync((prefab) => {
                result = prefab;
                isDone = true;
            });

            while (!isDone)
            {
                yield return null;
            }

            callback?.Invoke(result);
        }
        else
        {
            callback?.Invoke(null);
        }
    }

    // 添加真正的异步方法
    public IEnumerator LoadAsyncCoroutine<T>(string path, Action<T> callback, string ext = ".prefab") where T : UnityEngine.Object
    {
        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        //// 第1步：加载预制体
        //sw.Start();

        AssetInfo<T> info = getAssetInfo<T>(path, ext);

        //sw.Stop();
        //Debug.LogError($"getAssetInfo体耗时: {sw.ElapsedMilliseconds}ms");

        if (ReferenceEquals(info, null))
        {
            Debug.LogError("LoadAsyncCoroutine，空");

            callback?.Invoke(default(T));
            yield break;
        }
        // 使用协程异步加载
        yield return ((AssetInfo<T>)info).ResourcesLoadAsync(callback);
    }

    //=============================================
    // 保持原有的异步方法不变


    public Task<GameObject> LoadPrefabAsync(string path)
    {
        //Debug.LogError("LoadPrefabAsync:path:" + path);
        var tcs = new TaskCompletionSource<GameObject>();

        // 启动协程
        CoroutineController.Instance.StartCoroutine(
            LoadAsyncCoroutine<GameObject>(path, (prefab) => {
                //Debug.LogError("LoadPrefabAsync prefab:" + prefab);
                tcs.SetResult(prefab);
            })
        );

        return tcs.Task;  // 直接返回Task
    }
    

    // === 新添加的真正的异步加载方法 ===
    // 方法重载1：真正的异步加载（回调方式）
    //public void LoadAsyncReal<T>(string path, Action<T> loaded, string ext = ".prefab") where T : UnityEngine.Object
    //{
    //    var info = getAssetInfo<T>(path, null, ext);
    //    if (!ReferenceEquals(info, null))
    //    {
    //        CoroutineController.Instance.StartCoroutine(info.GetObjectAsync(loaded));
    //    }
    //}

    //// 方法重载2：带进度的异步加载
    //public void LoadAsyncReal<T>(string path, Action<T> loaded, Action<float> progress, string ext = ".prefab") where T : UnityEngine.Object
    //{
    //    var info = getAssetInfo<T>(path, null, ext);
    //    if (!ReferenceEquals(info, null))
    //    {
    //        CoroutineController.Instance.StartCoroutine(info.GetObjectAsync(loaded, progress));
    //    }
    //}

    // 预制体专用的真正异步加载
    //public void LoadPrefabAsyncReal(string path, Action<GameObject> loaded)
    //{
    //    LoadAsyncReal<GameObject>(path, loaded, ".prefab");
    //}

    //// === 添加基于Task的异步方法（可选）===
    //public Task<T> LoadAsyncTask<T>(string path, string ext = ".prefab") where T : UnityEngine.Object
    //{
    //    var tcs = new TaskCompletionSource<T>();

    //    LoadAsyncReal<T>(path, (obj) => {
    //        if (obj != null)
    //            tcs.SetResult(obj);
    //        else
    //            tcs.SetException(new Exception($"Failed to load asset: {path}"));
    //    }, ext);

    //    return tcs.Task;
    //}

    //public Task<GameObject> LoadPrefabAsyncTask(string path)
    //{
    //    return LoadAsyncTask<GameObject>(path, ".prefab");
    //}
    // === 新添加方法结束 ===

    public PlayableAsset LoadPlayable(string path)
    {
        return Load<PlayableAsset>(path,".playable");
    }
    
    public GameObject LoadTmx(string path)
    {
        return Load<GameObject>(path,".tmx");
    }

    public Sprite LoadSprite(string path)
    {
        return Load<Sprite>(path,".png");
    }
    public AudioClip LoadAudioClip(string path,string ext= ".ogg")
    {
        return Load<AudioClip>(path, ext);
    }

    public Font LoadFont(string path)
    {
        return Load<Font>(path);
    }
    //public FishConfigManager LoadFishConfigManager(string configName = "FishConfigManager")
    //{
    //    return Load<FishConfigManager>($"ScriptConfig/{configName}");
    //}
    public Shader LoadShader(string path)
    {
        // 使用 Shader.Find 或 Resources.Load<Shader> 加载 Shader
        Shader shader = Shader.Find(path); // 方法1：通过名称查找 Shader
        if (shader == null)
        {
            shader = Resources.Load<Shader>(path); // 方法2：从 Resources 文件夹加载 Shader
        }

        if (shader == null)
        {
            Debug.LogError($"Shader not found: {path}");
        }

        return shader;
    }
    public SkeletonDataAsset LoadSpineData(string path)
    {
        return Load<SkeletonDataAsset>(path, ".asset");
    }

    public T Load<T>(string path, string ext = ".prefab") where T : UnityEngine.Object
    {
        AssetInfo<T> info = getAssetInfo<T>(path, ext);
        if (!ReferenceEquals(info, null))
            return info.AssetObject;
        return default(T);
    }

    //Instance
    public T LoadInstance<T>(string path) where T : UnityEngine.Object
    {
        var obj = Load<T>(path);
        return Instantiate(obj);
    }

	//Instance & Coroutine
	public void LoadInstanceCoroutine(string path, Action<GameObject> loaded)
	{
		LoadInstanceCoroutine<GameObject>(path, loaded);
	}

	public void LoadInstanceCoroutine<T>(string path, Action<T> loaded) where T : UnityEngine.Object
	{
		LoadCoroutine<T>(path, obj => { Instantiate<T>(obj, loaded); });
	}

	public void LoadCoroutine<T>(string path, Action<T> loaded) where T : UnityEngine.Object
	{
		var info = getAssetInfo(path, loaded);
		if (!ReferenceEquals(info, null))
			CoroutineController.Instance.StartCoroutine(info.GetObjectByCoroutine(loaded));
	}

	//Instance & Async
	//public void LoadInstanceAsync(string path, Action<GameObject> loaded)
	//{
	//    LoadInstanceAsync<GameObject>(path, loaded);
	//}

	//public void LoadInstanceAsync<T>(string path, Action<T> loaded) where T : UnityEngine.Object
	//{
	//    LoadAsync<T>(path, obj => { Instantiate<T>(obj, loaded); });
	//}

	//public void LoadInstanceAsync(string path, Action<GameObject> loaded, Action<float> progress)
	//{
	//    LoadInstanceAsync<GameObject>(path, loaded, progress);
	//}

	//public void LoadInstanceAsync<T>(string path, Action<T> loaded, Action<float> progress) where T : UnityEngine.Object
	//{
	//    LoadAsync<T>(path, obj => { Instantiate<T>(obj, loaded); }, progress);
	//}

	//public void LoadAsync<T>(string path, Action<T> loaded) where T : UnityEngine.Object
	//{
	//    LoadAsync<T>(path, loaded, null);
	//}

	//public void LoadAsync<T>(string path, Action<T> loaded, Action<float> progress) where T : UnityEngine.Object
	//{
	//    var info = getAssetInfo<T>(path, loaded);
	//    if (!ReferenceEquals(info, null))
	//        CoroutineController.Instance.StartCoroutine(info.GetObjectAsync(loaded, progress));
	//}

	//释放资源
	public void UnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
    }
    #endregion

    #region private function

    private AssetInfo<T> getAssetInfo<T>(string path, string ext = ".prefab") where T : UnityEngine.Object
    {
        return getAssetInfo<T>(path, null, ext);
    }
    private AssetInfo<T> getAssetInfo<T>(string path, Action<T> loaded,string ext= ".prefab") where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Error: null path name.");
            loaded?.Invoke(null);
        }
        else
        {
            AssetInfo<T> info = null;
            string hashKey = string.Format($"{path}{typeof(T)}");
            if (!hashTable.ContainsKey(hashKey))
            {
                info = new AssetInfo<T>();
                info.Path = path;
                info.Ext = ext;
                hashTable.Add(hashKey, info);
            }
            else
                info = hashTable[hashKey] as AssetInfo<T>;
            info.RefCount++;
            return info;
        }
        return null;
    }

    private T Instantiate<T>(T obj) where T : UnityEngine.Object
    {
        return Instantiate<T>(obj, null);
    }
    private T Instantiate<T>(T obj, Action<T> loaded) where T : UnityEngine.Object
    {
        T retObj = default(T);
        if (!ReferenceEquals(obj, null))
        {
            retObj = MonoBehaviour.Instantiate(obj);
            if (!ReferenceEquals(retObj, null))
                loaded?.Invoke(retObj);
            else
                Debug.LogError("Error: null Instantiate retObj.");
        }
        else
            Debug.LogError("Error: null Resources Load return obj.");
        return retObj;
    }
    #endregion


    // ... [保持其他原有代码不变] ...

}
