using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class FileNode
{
    public string filePath;
    public string hash;
    public int length;
}
public class AssetUpdater : MonoBehaviour
{
    //class FileNode
    //{
    //    public string filePath;
    //    public string hash;
    //    public int length;
    //}

    public Action<int, int, int> onCheckDone;
    public Action<int> updateComplete;
    public Action<ProgressType, int, int, int> onProgress;
    public Action<int, string, int> onError;

    private readonly List<string> _updateBundles = new List<string>();
    private readonly List<string> _deleteBundles = new List<string>();
    private int _loadBundleIndex = 0;
    private string _loadFileList = null;
    private AssetConfig _config;

    private int totalBytes;
    private int totalBytes1;
    public int gameId = 0;

    public async void StartUpdate(AssetConfig config, string version,int id=0)
    {
        //Debug.LogError("StartUpdate,id:" + id+","+ gameId);
        if (!config.IsUpdate)
        {
            updateComplete?.Invoke(gameId);
            return;
        }

        _config = config;
        totalBytes = 0;
        totalBytes1 = 0;

        // 异步创建目录
        await Task.Run(() => {
            if (!Directory.Exists(_config.persistentDataPath))
                Directory.CreateDirectory(_config.persistentDataPath);
        });

        //var _instance = this.transform.GetOrAddComponent<UnityMainThreadDispatcher>();
        //UnityMainThreadDispatcher._instance = _instance;

        //UnityMainThreadDispatcher.Instance();
        //transform.GetOrAddComponent<MainThreadDispatcher>();
        //UnityMainThreadDispatcher.Initialize();

        //Debug.Log("StartUpdate," + _config.persistentDataPath);
        await StartCheckFileListAsync();

        // 获取所有已加载的程序集
        //var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        //foreach (var assembly in assemblies)
        //{
        //    // 查找包含 Task 的程序集
        //    if (assembly.FullName.Contains("Threading.Tasks") ||
        //        assembly.FullName.Contains("System.Runtime") ||
        //        assembly.FullName.Contains("mscorlib"))
        //    {
        //        Debug.Log($"程序集: {assembly.FullName}");

        //        // 尝试获取 Location 来推断
        //        try
        //        {
        //            if (!string.IsNullOrEmpty(assembly.Location))
        //            {
        //                Debug.Log($"位置: {assembly.Location}");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Debug.Log($"无法获取位置: {e.Message}");
        //        }
        //    }
        //}
    }

    public async void StartHotUpdateDownload()
    {
        await TryLoadNextBundle();
    }

    private async Task StartCheckFileListAsync()
    {
        onProgress?.Invoke(ProgressType.CheckFileList, 0, 0, gameId);
        //Debug.LogError("StartCheckFileList:" + _config.url + AssetConfig.File_List_Name);

        await DownloadAsync(_config.url + AssetConfig.File_List_Name, null, OnFileListDone, OnError);
    }

    private void OnError(int errCode, string error)
    {
        Debug.LogError("OnError?" + error+ ",gameId:" + gameId);
        onError?.Invoke(errCode, error, gameId);
    }

    private async void OnFileListDone(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("服务器内容为空！");
            return;
        }
        _loadFileList = text;
        
        // 将耗时的文件对比操作移到后台线程
        await Task.Run(() => ProcessFileComparison());
        //await ProcessFileComparison();

        //Debug.LogError("_updateBundles.Count:"+ _updateBundles.Count);
        //if (_updateBundles.Count > 0)
        //{
        //    _loadBundleIndex = 0;
        //    onCheckDone?.Invoke(totalBytes, totalBytes1, gameId);
        //}
        //else
        //{
        //    //UnityMainThreadDispatcher.Instance().Enqueue(() => {
        //    //    onCheckDone?.Invoke(0, totalBytes1, gameId);
        //    //});
        if (_updateBundles.Count > 0)
        {
            _loadBundleIndex = 0;
            onCheckDone?.Invoke(totalBytes, totalBytes1, gameId);
        }
        else
        {
           // Debug.LogError("OnFileListDone?gameId:" + gameId + ",_updateBundles.Count:" + _updateBundles.Count + "======>_updateBundles.:" + text);

            //UnityMainThreadDispatcher.Instance().Enqueue(() => {
            onCheckDone?.Invoke(0, totalBytes1, gameId);
            //});
            // 等待2秒
            await Task.Delay(1);
            OnAllLoadDone();
        }
        // 立即回到主线程执行
        //await ExecuteOnMainThreadImmediate(() => {
        //    OnAllLoadDone();
        //});
        //}
    }

    // 高性能的主线程执行方法
    //private Task ExecuteOnMainThreadImmediate()
    //{
    //    var tcs = new TaskCompletionSource<bool>();
    //    Debug.LogError("高性能的主线程执行方法,");
    //    // 立即启动协程，下一帧就执行
    //    StartCoroutine(ExecuteImmediateCoroutine());

    //    //updateComplete?.Invoke(gameId);

    //    return tcs.Task;
    //}

    //private IEnumerator ExecuteImmediateCoroutine()
    //{
    //    // 下一帧立即执行
    //    yield return null;
    //    Debug.LogError("ExecuteImmediateCoroutine," );
    //    updateComplete?.Invoke(gameId);
    //    //tcs.SetResult(true);
    //}

    private void ProcessFileComparison()
    {
       // Dictionary<string, FileNode> newFileListMap = new Dictionary<string, FileNode>();
        Dictionary<string, FileNode> oldFileListMap = new Dictionary<string, FileNode>();

        TextToBundleMap(_loadFileList, _config.newFileListMap);

        

        var curFileListPath = _config.GetPath(AssetConfig.File_List_Name, false);
        if (curFileListPath != null)
        {
            var oldText = AssetUtil.ReadFile(curFileListPath);
            if (!string.IsNullOrEmpty(oldText))
            {
                TextToBundleMap(oldText, oldFileListMap);
            }
        }
        //Debug.LogError("ProcessFileComparison======" + _config.newFileListMap.Count + "," + oldFileListMap.Count);
        //Debug.LogError("newFileListMap.Count:" + newFileListMap.Count+","+ oldFileListMap.Count);

        _updateBundles.Clear();
        _deleteBundles.Clear();

        foreach (var pair in _config.newFileListMap)
        {
            var fileName = pair.Key;
            var assetPath = StringUtil.Concat(pair.Value.filePath, "_", pair.Value.hash, AssetConfig.Bundle_PostFix);

            FileNode node;
            if (oldFileListMap.TryGetValue(fileName, out node))
            {
                //Debug.LogError("包含 assetPath:" + assetPath + "," + pair.Value.length);

                //if (!node.hash.Equals(pair.Value.hash) || !node.length.Equals(pair.Value.length) || _config.GetPath(assetPath) == null)
                {
                    AddUpdateBundle(assetPath);

                  
                    totalBytes += pair.Value.length;
                }
            }
            else
            {
                //Debug.LogError("不包含 assetPath:" + assetPath + "," + pair.Value.length);

                AddUpdateBundle(assetPath);
                totalBytes += pair.Value.length;
            }
            
            totalBytes1 += pair.Value.length;
        }

        //Debug.LogError("totalBytes======" + totalBytes + "," + totalBytes1+","+ _updateBundles.Count);


        foreach (var pair in oldFileListMap)
        {
            var fileName = pair.Key;
            var assetPath = StringUtil.Concat(pair.Value.filePath, "_", pair.Value.hash, AssetConfig.Bundle_PostFix);

            FileNode node;
            if (_config.newFileListMap.TryGetValue(fileName, out node))
            {
                if (!node.hash.Equals(pair.Value.hash))
                {
                    _deleteBundles.Add(assetPath);
                }
            }
            else
            {
                _deleteBundles.Add(assetPath);
            }
        }

        // 回到主线程调用回调

        //Debug.LogError("_updateBundles.Count:" + _updateBundles.Count);
        

    }

    private void AddUpdateBundle(string path)
    {
      //  Debug.LogError("AddUpdateBundle,path:" + path);
        //_updateBundles.Add(path);
        if (_config.GetPath(path, true) == null)
        {
           // Debug.LogError("AddUpdateBundle,"+ path);
            _updateBundles.Add(path);
        }
        //else
        //    Debug.LogError("no AddUpdateBundle," + path);
    }

    private async Task TryLoadNextBundle()
    {
        onProgress?.Invoke(ProgressType.Download, _loadBundleIndex, _updateBundles.Count, gameId);

        Debug.Log("TryLoadNextBundle:" + _loadBundleIndex + "," + _updateBundles.Count);

        if (_loadBundleIndex >= _updateBundles.Count /*&& _updateBundles.Count>0*/)
        {
            OnAllLoadDone();
            return;
        }

        string bundle = _updateBundles[_loadBundleIndex];
        _loadBundleIndex++;

        
        //StartCoroutine(Download(_config.url + bundle, bundle, OnBundleDone, OnError));

        await DownloadAsync(_config.url + bundle, bundle, OnBundleDone, OnError);
    }

    private async void OnBundleDone(string file)
    {
        await TryLoadNextBundle();
    }

    private void TextToBundleMap(string text, Dictionary<string, FileNode> map)
    {
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line) || line.Contains("#"))
                continue;

            var strs = line.Split('|');
            if (strs.Length >= 3)
            {
                int length;
                if (int.TryParse(strs[2], out length))
                {
                    var node = new FileNode();
                    node.filePath = strs[0];
                    node.hash = strs[1];
                    node.length = length;
                    map.Add(node.filePath, node);
                }
            }
        }
    }

    private  void OnAllLoadDone()
    {
        //Debug.LogError("OnAllLoadDone？");
        // 异步删除文件
        //Task.Run(() =>
        //{
        foreach (var bundle in _deleteBundles)
            {
                var path = StringUtil.Concat(_config.persistentDataPath, bundle);
                if (File.Exists(path))
                    File.Delete(path);
            }
            File.WriteAllText(StringUtil.Concat(_config.persistentDataPath, AssetConfig.File_List_Name), _loadFileList);
        // 回到主线程完成后续操作
        //UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //{
       
        //UnityMainThreadDispatcher.ExecuteOnMainThread(() =>
        //    {
                //Debug.LogError("有没有执行到这里？" + Time.deltaTime + "," + UnityMainThreadDispatcher._actions.Count);
                updateComplete?.Invoke(gameId);
                //Task.Run(() => ExecuteOnMainThreadImmediate());
                //await ExecuteOnMainThreadImmediate();
                //StartCoroutine(ExecuteImmediateCoroutine(updateComplete));
            //});
        //});
    }

    // 新增异步下载方法
    private async Task DownloadAsync(string url, string savePath, Action<string> onDone, Action<int, string> onError)
    {
        using (var request = UnityWebRequest.Get(url))
        {
            var operation = request.SendWebRequest();
           // Debug.LogError("下载什么文件？"+ url);

            while (!operation.isDone)
            {
                await Task.Yield(); // 每帧让出控制权，避免阻塞
            }

            if (request.isNetworkError || !string.IsNullOrEmpty(request.error))
            {
                onError?.Invoke(1, StringUtil.Concat(url, "\n", request.error));
            }
            else if (request.responseCode != 200)
            {
                onError?.Invoke(2, StringUtil.Concat(url, "\nresponseCode:", request.responseCode.ToString()));
            }
            else
            {
             //   Debug.LogError("request.downloadHandler.text:：" + request.downloadHandler.text);

                if (!string.IsNullOrEmpty(savePath))
                {
                    string path = _config.persistentDataPath + savePath;

                  //  Debug.LogError("path：" + path);

                    string dir = Path.GetDirectoryName(path);
                    byte[] data = request.downloadHandler.data;
                    await Task.Run(() => {
                        if (dir != null && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        File.WriteAllBytes(path, data);
                    });

                    onDone?.Invoke(savePath);
                }
                else
                {
                    onDone?.Invoke(request.downloadHandler.text);
                }
            }
        }
    }

    // 保留原有协程方法供下载bundle使用
    //private IEnumerator Download(string url, string savePath, Action<string> onDone, Action<int, string> onError)
    //{
    //    var request = UnityWebRequest.Get(url);
    //    yield return request.SendWebRequest();

    //    if (request.isNetworkError || !string.IsNullOrEmpty(request.error))
    //    {
    //        onError?.Invoke(1, StringUtil.Concat(url, "\n", request.error));
    //    }
    //    else if (request.responseCode != 200)
    //    {
    //        onError?.Invoke(2, StringUtil.Concat(url, "\nresponseCode:", request.responseCode.ToString()));
    //    }
    //    else
    //    {
    //        if (!string.IsNullOrEmpty(savePath))
    //        {
    //            string path = _config.persistentDataPath + savePath;
    //            string dir = Path.GetDirectoryName(path);

    //            // 文件写入也异步化
    //            Task.Run(() => {
    //                if (dir != null && !Directory.Exists(dir))
    //                    Directory.CreateDirectory(dir);
    //                File.WriteAllBytes(path, request.downloadHandler.data);
    //            }).ContinueWith(t => {
    //               // UnityMainThreadDispatcher.Instance().Enqueue(() => onDone?.Invoke(savePath));
    //            });
    //        }
    //        else
    //        {
    //            onDone?.Invoke(request.downloadHandler.text);
    //        }
    //    }

    //    request.Dispose();
    //}
}

public enum ProgressType
{
    CheckFileList,
    Download,
}
