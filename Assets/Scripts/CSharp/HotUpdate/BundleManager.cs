using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System;
using System.Linq;

public class BundleManager
{
    private readonly Context _context;
    private AssetBundleManifest _manifest;
    //private readonly Dictionary<string, string> _assetPath_to_bundleName = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _bundleName_to_hashName = new Dictionary<string, string>();
    private readonly Dictionary<string, AssetBundle> _bundleCache = new Dictionary<string, AssetBundle>();

    public BundleManager(Context context)
    {
        _context = context;
    }

    public async Task Init1()
    {
        Clear();
        LoadABFileList();  // 如果这个也是异步的
        await LoadManifest1();
        // await LoadAssetsMap(); // 其他异步方法
    }

    public void Init()
    {
        Clear();
        LoadABFileList();  // 如果这个也是异步的

        LoadManifest();
        // await LoadAssetsMap(); // 其他异步方法
    }

    private void LoadABFileList()
    {
        var path = _context.Config.GetPath(AssetConfig.File_List_Name, false);
       // Debug.LogError(">>LoadABFileList:" + path+","+ AssetConfig.File_List_Name);
        if (path == null)
        {
            Debug.LogError("No ab_file_list!");
            return;
        }

        var text = AssetUtil.ReadFile(path);

       

        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("ab_file_list read error!"+ path);
            return;
        }

        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line) || line.Contains("#"))
                continue;

            var strs = line.Split('|');
            if (strs.Length >= 3)
            {
                var bundleName = strs[0];

               // Debug.LogError(">>LoadABFileList:bundleName:" + bundleName);

                var hashName = StringUtil.Concat(strs[0], "_", strs[1], AssetConfig.Bundle_PostFix);
                if (_bundleName_to_hashName.ContainsKey(bundleName))
                {
                    _bundleName_to_hashName[bundleName] = hashName;
                }
                else
                {
                    //Debug.LogError("Add:"+ bundleName);
                    _bundleName_to_hashName.Add(bundleName, hashName);
                }
            }
        }

        Debug.LogError(">>LoadABFileList:_bundleName_to_hashName.Count？" + _bundleName_to_hashName.Count);
    }

    private async Task LoadManifest1()
    {
        string hashName = GetAssetBundleHashName(AssetConfig.AssetBundleManifest_Name);
        if (string.IsNullOrEmpty(hashName))
        {
            Debug.LogError("No AssetBundleManifest in file list");
            return;
        }
       

        var fullPath = _context.Config.GetPath(hashName);
        //Debug.LogError("LoadManifest:" + AssetConfig.AssetBundleManifest_Name + ",完整的路径名称:" + fullPath);
        if (string.IsNullOrEmpty(fullPath))
        {
            Debug.LogError("No AssetBundleManifest path");
            return;
        }



        //foreach (AssetBundle bundle1 in AssetBundle.GetAllLoadedAssetBundles())
        //{
        //    //  count++;
        //    string bundleName = string.IsNullOrEmpty(bundle1.name) ? "未命名Bundle" : bundle1.name;
        //    Debug.LogError($"Bundle : {bundleName}");
        //}


        // 或者检查Bundle是否还在内存中
        // 检查Bundle是否还在内存中（使用LINQ）
        //AssetBundle existingBundle = AssetBundle.GetAllLoadedAssetBundles()
        //    .FirstOrDefault(bundle => bundle.name == hashName);

        //if (existingBundle != null)
        //{
        //    Debug.Log($"AssetBundle {hashName} 已存在于内存中");
        //   // _loadedBundle = existingBundle;
        //    return;
        //}
        //list.Fi
        //AssetBundle existingBundle = .FirstOrDefault(bundle => bundle.name == _bundleName);

        //if (existingBundle != null)
        //{
        //    Debug.Log($"AssetBundle {_bundleName} 已存在于内存中");
        //    _loadedBundle = existingBundle;
        //    return;
        //}
        //Debug.LogError("LoadManifest===0");
        // 让主线程休息一帧
        await Task.Delay(10);
        Debug.LogError("LoadManifest===1:"+ hashName +","+ fullPath);
        var bundleCreateRequest = AssetBundle.LoadFromFileAsync(fullPath);
        
        // 等待加载完成，但不阻塞主线程
        while (!bundleCreateRequest.isDone)
        {
             await Task.Delay(100); // 等待1ms，相当于一帧
            //Debug.LogError("LoadManifest===2");
        }
        // 让主线程休息一帧
        await Task.Delay(100);
        Debug.LogError("LoadManifest===3");
        AssetBundle bundle = bundleCreateRequest.assetBundle;
        if (bundle == null)
        {
            Debug.LogError("AssetBundleManifest bundle load error");
            return;
        }
        // 让主线程休息一帧
        await Task.Delay(100);
        Debug.LogError("LoadManifest===4");

        // 异步加载 AssetBundleManifest
        var assetLoadRequest = bundle.LoadAssetAsync<AssetBundleManifest>("AssetBundleManifest");
        while (!assetLoadRequest.isDone)
        {
            await Task.Delay(100); // 等待1ms，相当于一帧
            //Debug.LogError("LoadManifest===5");
        }
        
        await Task.Delay(100);
        Debug.LogError("LoadManifest===6");
        bundle.Unload(false);
        await Task.Delay(100);
        Debug.LogError("LoadManifest===end");
    }


    private void LoadManifest()
    {
        string hashName = GetAssetBundleHashName(AssetConfig.AssetBundleManifest_Name);
        if (string.IsNullOrEmpty(hashName))
        {
            Debug.LogError("No AssetBundleManifest in file list");
            return;
        }

        var fullPath = _context.Config.GetPath(hashName);
        // Debug.LogError(AssetConfig.AssetBundleManifest_Name+",完整的路径名称:" + fullPath);
        if (string.IsNullOrEmpty(fullPath))
        {
            Debug.LogError("No AssetBundleManifest path");
            return;
        }

        AssetBundle bundle = AssetBundle.LoadFromFile(fullPath);
        if (bundle == null)
        {
            Debug.LogError("AssetBundleManifest bundle load error");
            return;
        }

        _manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        if (_manifest == null)
        {
            Debug.LogError("AssetBundleManifest load error");
            bundle.Unload(true);
            return;
        }

        bundle.Unload(false);
    }
    //private void LoadAssetsMap()
    //{
    //    string hashName = GetAssetBundleHashName(AssetConfig.AssetBundle_Build_List_Name);
    //    if (string.IsNullOrEmpty(hashName))
    //    {
    //        Debug.LogError("No AssetBundle_Build_List in file list");
    //        return;
    //    }

    //    var fullPath = _context.Config.GetPath(hashName);
    //    if (string.IsNullOrEmpty(fullPath))
    //    {
    //        Debug.LogError("No AssetBundle_Build_List path");
    //        return;
    //    }

    //    AssetBundle bundle = AssetBundle.LoadFromFile(fullPath);
    //    if (bundle == null)
    //    {
    //        Debug.LogError("AssetBundle_Build_List bundle load error");
    //        return;
    //    }

    //    var textAsset = bundle.LoadAsset<TextAsset>(AssetConfig.AssetBundle_Build_List_Path);
    //    if (textAsset == null)
    //    {
    //        Debug.LogError("AssetBundle_Build_List load error");
    //        bundle.Unload(true);
    //        return;
    //    }

    //    var lines = textAsset.text.Split('\n');
    //    bundle.Unload(true);
    //    string bundleName = null;
    //    foreach (var line in lines)
    //    {
    //        if (string.IsNullOrEmpty(line))
    //            continue;

    //        if (line.StartsWith("\t"))
    //        {
    //            if (bundleName != null)
    //            {
    //                var assetPath = line.Substring(1);

    //                if (_assetPath_to_bundleName.ContainsKey(assetPath))
    //                {
    //                    _assetPath_to_bundleName[assetPath] = bundleName;
    //                }
    //                else
    //                {
    //                    _assetPath_to_bundleName.Add(assetPath, bundleName);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            bundleName = line;
    //        }
    //    }
    //}

    public string GetAssetBundleHashName(string bundleName)
    {
        string hashName = null;
        _bundleName_to_hashName.TryGetValue(bundleName, out hashName);
        return hashName;
    }

    public string GetAssetBundleName(string assetPath)
    {
        string bundleName = null;
        string path =  assetPath.Replace(SysDefines.HOTUPDATEPATH, "");
        // path.StartsWith(SysDefines.ROOMPATH)
        if(false)
        {
            string[] array = path.Split("/");
            bundleName =  array[1].ToLower();
          //  Debug.LogError("返回包名:"+ bundleName+","+ assetPath);
            return bundleName;
        }
        else
        {
            string[] array = path.Split("/");
            bundleName = array[0].ToLower();

           // Debug.LogError("返回包名:" + bundleName);
            return bundleName;
        }
        //_assetPath_to_bundleName.TryGetValue(assetPath, out bundleName);
        return bundleName;
    }

    public AssetBundle LoadAssetBundle(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
            return null;

        var hashName = GetAssetBundleHashName(bundleName);
       //Debug.LogError("LoadAssetBundle>>>>>bundleName:" + bundleName + ",hashName:" + hashName);
        if (string.IsNullOrEmpty(hashName))
            return null;

        return LoadAssetBundleByHashName(hashName);
    }

    public AssetBundle LoadAssetBundleByHashName(string hashName)
    {
       // Debug.LogError("LoadAssetBundleByHashName hashName: " + hashName+","+ _manifest);
        if (_manifest != null)
        {
            string[] dependencies = _manifest.GetAllDependencies(hashName);
            foreach (var dependency in dependencies)
            {
                _LoadAssetBundleByHashName(dependency);
            }
        }

        return _LoadAssetBundleByHashName(hashName);
    }

    private AssetBundle _LoadAssetBundleByHashName(string hashName)
    {
        //Debug.LogError("_LoadAssetBundleByHashName hashName:" + hashName);
        if (string.IsNullOrEmpty(hashName))
            return null;

        AssetBundle assetBundle = null;
        if (_bundleCache.TryGetValue(hashName, out assetBundle))
        {
            //Debug.LogError("_bundleCache TryGetValue->" + hashName);
            return assetBundle;
        }
        
        var fullPath = _context.Config.GetPath(hashName);

        //Debug.LogError("hashName->" + fullPath);
        if (string.IsNullOrEmpty(fullPath))
            return null;

        assetBundle = AssetBundle.LoadFromFile(fullPath);
        if (assetBundle == null)
            return null;

        //Debug.LogError("_bundleCache Add->" + hashName);
        _bundleCache.Add(hashName, assetBundle);
        return assetBundle;
    }

    public AssetBundle GetCachedAssetBundle(string bundleName)
    {
        var hashName = GetAssetBundleHashName(bundleName);
        if (string.IsNullOrEmpty(hashName))
            return null;

        AssetBundle assetBundle = null;
        if (_bundleCache.TryGetValue(hashName, out assetBundle))
            return assetBundle;

        return null;
    }

    public void UnloadAssetBundle(string bundleName)
    {
        var hashName = GetAssetBundleHashName(bundleName);
        if (string.IsNullOrEmpty(hashName))
            return;

        AssetBundle assetBundle = null;
        if (_bundleCache.TryGetValue(hashName, out assetBundle))
        {
            assetBundle.Unload(false);
            _bundleCache.Remove(hashName);
        }
    }

    public void Test()
    {
        Debug.LogError("_bundleName_to_hashName.Count：" + _bundleName_to_hashName.Count);
    }

    public void Clear()
    {
        Debug.LogError("Clear.！！！！！：" );

        if (_manifest != null)
            Resources.UnloadAsset(_manifest);
        _manifest = null;

        //_assetPath_to_bundleName.Clear();
        _bundleName_to_hashName.Clear();
       // Debug.LogError("BundleManager Clear>>>>" + _bundleCache.Count);
        foreach (var pair in _bundleCache)
        {
          //  Debug.LogError("BundleManager pair.Key>>>>" + pair.Key);

            var bundle = pair.Value;
            if (bundle != null)
            {
                bundle.Unload(false);
            }
        }
        _bundleCache.Clear();
    }

    //=======================================
    public IEnumerator LoadAssetBundleAsync(string bundleName, Action<AssetBundle> callback)
    {
        //System.Diagnostics.Stopwatch totalSw = System.Diagnostics.Stopwatch.StartNew();
        //Debug.LogError($"开始LoadAssetBundleAsync: {bundleName}, 时间: {DateTime.Now:HH:mm:ss.fff}");

        var hashName = GetAssetBundleHashName(bundleName);
        //Debug.LogError($"hashName: {hashName}, 获取时间: {DateTime.Now:HH:mm:ss.fff}");

        if (string.IsNullOrEmpty(hashName))
        {
            callback?.Invoke(null);
            yield break;
        }

        yield return LoadAssetBundleByHashNameAsync(hashName, (bundle) => {
            //totalSw.Stop();
            //Debug.LogError($"LoadAssetBundleAsync完成，总耗时: {totalSw.ElapsedMilliseconds}ms, 时间: {DateTime.Now:HH:mm:ss.fff}");
            callback?.Invoke(bundle);
        });
    }

    public IEnumerator LoadAssetBundleByHashNameAsync(string hashName, Action<AssetBundle> callback)
    {
        //Debug.LogError($"开始LoadAssetBundleByHashNameAsync: {hashName}, 时间: {DateTime.Now:HH:mm:ss.fff}");

        // 1. 检查缓存
        //System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        if (_bundleCache.TryGetValue(hashName, out AssetBundle mainBundle))
        {
            //sw.Stop();
            //Debug.LogError($"从缓存获取，耗时: {sw.ElapsedMilliseconds}ms");
            callback?.Invoke(mainBundle);
            yield break;
        }

        // 2. 加载依赖
        if (_manifest != null)
        {
            string[] dependencies = _manifest.GetAllDependencies(hashName);
            //Debug.LogError($"有 {dependencies.Length} 个依赖: {string.Join(", ", dependencies)}");

            foreach (var dependency in dependencies)
            {
                //Debug.LogError($"开始加载依赖: {dependency}, 时间: {DateTime.Now:HH:mm:ss.fff}");

                if (!_bundleCache.ContainsKey(dependency))
                {
                    yield return LoadSingleBundleAsync(dependency);
                }
                else
                {
                    Debug.LogError($"依赖 {dependency} 已在缓存中");
                }
            }
        }

        // 3. 加载主bundle
        //Debug.LogError($"开始加载主bundle: {hashName}, 时间: {DateTime.Now:HH:mm:ss.fff}");
        yield return LoadSingleBundleAsync(hashName, (bundle) => {
            //Debug.LogError($"主bundle加载完成: {hashName}, bundle: {bundle != null}, 时间: {DateTime.Now:HH:mm:ss.fff}");
            callback?.Invoke(bundle);
        });
    }

    private IEnumerator LoadSingleBundleAsync(string hashName, Action<AssetBundle> callback = null)
    {
        //Debug.LogError($"开始LoadSingleBundleAsync: {hashName}, 时间: {DateTime.Now:HH:mm:ss.fff}");
        //System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        // 再次检查缓存
        if (_bundleCache.TryGetValue(hashName, out AssetBundle bundle))
        {
            //sw.Stop();
            //Debug.LogError($"LoadSingleBundleAsync: {hashName} 已在缓存中，耗时: {sw.ElapsedMilliseconds}ms");
            callback?.Invoke(bundle);
            yield break;
        }

        var fullPath = _context.Config.GetPath(hashName);
        //Debug.LogError($"bundle路径: {fullPath}, 时间: {DateTime.Now:HH:mm:ss.fff}");

        if (string.IsNullOrEmpty(fullPath))
        {
            callback?.Invoke(null);
            yield break;
        }

        // 异步加载
        var bundleRequest = AssetBundle.LoadFromFileAsync(fullPath);
        //Debug.LogError($"开始LoadFromFileAsync, 时间: {DateTime.Now:HH:mm:ss.fff}");

        while (!bundleRequest.isDone)
        {
            yield return null;
        }

        //sw.Stop();
        //Debug.LogError($"LoadFromFileAsync完成，耗时: {sw.ElapsedMilliseconds}ms, 时间: {DateTime.Now:HH:mm:ss.fff}");

        if (bundleRequest.assetBundle != null)
        {
            _bundleCache.Add(hashName, bundleRequest.assetBundle);
            //Debug.LogError($"添加到缓存: {hashName}");
        }

        callback?.Invoke(bundleRequest.assetBundle);
    }
}
