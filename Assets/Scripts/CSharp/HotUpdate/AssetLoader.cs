using UnityEngine;
using System;
//using XLua;
using Object = UnityEngine.Object;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class AssetLoader
{
    private readonly Context _context;

    public AssetLoader(Context context)
    {
        _context = context;
    }

    public T LoadAsset<T>(string assetPath, bool logError = true) where T : Object
    {
        return (T)LoadAsset(assetPath, typeof(T), logError);
    }

    // ===== 异步版本 =====
    // ===== 添加异步加载方法 =====
    public IEnumerator LoadAssetAsync<T>(string assetPath, Action<T> callback, bool logError = true) where T : Object
    {
       

        if (string.IsNullOrEmpty(assetPath))
        {
            callback?.Invoke(null);
            yield break;
        }

		if (_context.Config.IsEditorAssets)
		{
#if UNITY_EDITOR
			yield return null; // 等待一帧

			var importer = AssetImporter.GetAtPath(assetPath);
			if (importer == null)
			{
				if (logError)
					Debug.LogError("No Asset:" + assetPath);
				callback?.Invoke(null);
				yield break;
			}

			AssetDatabase.Refresh();
			T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			callback?.Invoke(asset);
#else
            callback?.Invoke(null);
#endif
		}
		else
		{


            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sw.Start();

            var bundleMgr = _context.BundleMgr;
			var bundleName = bundleMgr.GetAssetBundleName(assetPath);


			var bundle = bundleMgr.LoadAssetBundle(bundleName);

           // Debug.LogError("LoadAssetBundleAsync>>>>"+ assetPath+","+ bundleName);

            //yield return bundleMgr.LoadAssetBundleAsync(bundleName,(AssetBundle bundle) =>{



            sw.Stop();
          //  Debug.LogError($"0. LoadAssetBundle体耗时: {sw.ElapsedMilliseconds}ms，{bundle}");
            // 
            if (bundle == null)
            {
                Debug.LogError("LoadAsset>>>>原来不存在");
                //if (_context.Config.newFileListMap.ContainsKey(bundleName))
                //{
                //    FileNode node = _context.Config.newFileListMap[bundleName];

                //    //  Debug.LogError("包含>>>>>>>:" + node.hash);


                //    // 方法2：如果没有，用 Unity 原生的 AssetBundle 异步加载
                //    string bundlePath = GetBundleFullPath("Hall/" + bundleName + "_" + node.hash + AssetConfig.Bundle_PostFix); // 你需要实现这个方法
                //                                                                                                                // var bundlePath = bundleMgr.GetAssetBundleName(bundleName);·
                //    Debug.LogError("bundlePath>>>>>>>:" + bundlePath);
                //    var bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
                //    yield return bundleRequest;

                //    bundle = bundleRequest.assetBundle;
                //    if (bundle == null)
                //    {
                //        if (logError)
                //            Debug.LogError("---Failed to load bundle:" + bundlePath);
                //        callback?.Invoke(null);
                //        yield break;
                //    }
                //}
            }

            //sw.Restart();

            // 异步加载资源
            var assetRequest = bundle.LoadAssetAsync<T>(assetPath);
			//yield return assetRequest;

            //sw.Stop();
            //Debug.LogError($"1. 加载预制体耗时: {sw.ElapsedMilliseconds}ms");
            //sw.Restart();

            T asset = assetRequest.asset as T;

			// 卸载bundle（如果不再需要）
			//bundle.Unload(false);

			callback?.Invoke(asset);
            //sw.Stop();
            //Debug.LogError($"2. 加载预制体耗时: {sw.ElapsedMilliseconds}ms");

          //  });
        }
    }

    // 如果没有 bundleMgr.GetBundlePath，用这个简单版本
    private string GetBundleFullPath(string bundleName)
    {
        // 根据平台获取正确的路径
        string streamingPath = Path.Combine(Application.streamingAssetsPath, bundleName);
        string persistentPath = Path.Combine(Application.persistentDataPath, bundleName);

        // 优先检查热更新路径
        if (File.Exists(persistentPath))
            return persistentPath;

        // 检查StreamingAssets
        if (File.Exists(streamingPath))
            return streamingPath;

        Debug.LogError($"Bundle not found: {bundleName}");
        Debug.LogError($"Checked paths: {persistentPath}, {streamingPath}");
        return null;
    }


    public static Object LoadEditorAsset(string assetPath, Type type, bool logError)
    {
#if UNITY_EDITOR
        var importer = AssetImporter.GetAtPath(assetPath);
        if (importer == null)
        {
            //if (logError)
            //    Debug.LogError("No Asset:" + assetPath);
            return null;
        }

        if (string.IsNullOrEmpty(importer.assetBundleName))
        {
           // if (logError)
           //     Debug.LogError("No AssetBundle:" + assetPath);
        }

        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath(assetPath, type);
#else
        Debug.LogError("LoadAsset By LocalAssets is invalid in Player mode");
        return null;
#endif
    }

    public Object LoadAsset(string assetPath, Type type, bool logError = false)
    {
       // Debug.LogError("LoadAsset>>>>0");
        if (string.IsNullOrEmpty(assetPath))
            return null;

        if (_context.Config.IsEditorAssets)
        {
            return LoadEditorAsset(assetPath, type, logError);
        }
        else
        {
           // Debug.LogError("LoadAsset===>" + assetPath + ",persistentDataPath>>>>" + _context.Config.persistentDataPath);
            // Debug.LogError("LoadAsset>>>>2");
            var bundleMgr = _context.BundleMgr;
            var bundleName = bundleMgr.GetAssetBundleName(assetPath);
           // Debug.LogError("LoadAsset>>>>3");
            if (string.IsNullOrEmpty(bundleName))
            {
                if (logError)
                    Debug.LogError("No bundleName:" + assetPath);
                return null;
            }
            //Debug.LogError("LoadAsset>>>>"+ _context.Config.url);
            var bundle = bundleMgr.LoadAssetBundle(bundleName);
           // Debug.LogError("LoadAsset>>>>4");
            if (bundle == null)
            {
                if (logError)
                    Debug.LogError("No bundle:" + bundleName);
                return null;
            }
            //Debug.LogError("LoadAsset>>>>5");
            return bundle.LoadAsset(assetPath, type);
        }
    }

    public void UnLoadAsset(string assetPath)
    {
        var bundleMgr = _context.BundleMgr;
        bundleMgr.UnloadAssetBundle(assetPath);
    }

    public void BindHeadIcon(Image image, string url, string cacheId = "")
    {
        BindSprite(image, url, string.IsNullOrEmpty(cacheId) ? "" : $"HeadIcon/{cacheId}");
    }

    public void BindActivityPicture(Image image, string url, string cacheId = "")
    {
        url = $"{SysDefines.OssUrl}/{SysDefines.ZoneId}/{url}";
        BindSprite(image, url, string.IsNullOrEmpty(cacheId) ? "" : $"ActivityPicture/{cacheId}");
    }

    private void BindSprite(Image image, string url, string cachePath)
    {
        CoroutineController.Instance.StartCoroutine(LoadTextureFromWeb(url, cachePath, texture => {
            if (ReferenceEquals(texture, null) || ReferenceEquals(image, null))
                return;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            image.sprite = sprite;
        }));
    }

    private IEnumerator LoadTextureFromWeb(string url, string cachePath, Action<Texture2D> loaded)
    {
        if (string.IsNullOrEmpty(url))
        {
            loaded?.Invoke(null);
            yield break;
        }
        string loadUrl = url;
        string fileName = string.Empty;
        string cacheDir = Path.Combine(Application.persistentDataPath, "CacheFile/Texture");
        string cacheFile = string.Empty;
        if (!string.IsNullOrEmpty(cachePath))
        {
            cacheDir = Path.Combine(cacheDir, cachePath);
            fileName = url.Replace("\\", "_").Replace("/", "_").Replace(":", "_")
             .Replace("*", "_").Replace("?", "_").Replace("<", "_")
             .Replace(">", "_").Replace("|", "_").Replace("\"", "_");
            cacheFile = Path.Combine(cacheDir, fileName);
            if (File.Exists(cacheFile))
                loadUrl = cacheFile;
            else
            {
                if (Directory.Exists(cacheDir))
                    Directory.Delete(cacheDir, true);
            }
        }
        if (!loadUrl.Contains("://"))
            loadUrl = new Uri(loadUrl).AbsoluteUri;
        using (UnityWebRequest request = UnityWebRequest.Get(loadUrl))
        {
            DownloadHandlerTexture dhTexture = new DownloadHandlerTexture(true);
            request.downloadHandler = dhTexture;
            yield return request.SendWebRequest();
            if (request.isNetworkError)
            {
                Debug.LogError($"load failed from {loadUrl} : is network error");
                loaded?.Invoke(null);
                yield break;
            }
            else if (!string.IsNullOrEmpty(request.error))
            {
                Debug.LogError($"load failed from {loadUrl} : {request.error}");
                loaded?.Invoke(null);
                yield break;
            }
            else if (request.responseCode != 200)
            {
                Debug.LogError($"load failed from {loadUrl} : code {request.responseCode.ToString()}");
                loaded?.Invoke(null);
                yield break;
            }
            else
            {
                if (!string.IsNullOrEmpty(cachePath))
                {
                    if (!File.Exists(cacheFile))
                    {
                        if (!Directory.Exists(cacheDir))
                            Directory.CreateDirectory(cacheDir);
                        File.WriteAllBytes(cacheFile, request.downloadHandler.data);
                    }
                }
                loaded?.Invoke(dhTexture.texture);
            }
        }
    }
}
