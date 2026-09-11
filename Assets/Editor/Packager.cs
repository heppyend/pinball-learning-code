using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
//using LuaFramework;
using System.Linq;
using System;
using LuaFramework;
//using LuaFramework;

public class Packager {
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();
    static Dictionary<string, List<AssetBundleBuild>> maps = new Dictionary<string, List<AssetBundleBuild>>();
   // static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();

    ///-----------------------------------------------------------
    static string[] exts = { ".txt", ".xml", ".lua", ".assetbundle", ".json" };
    static bool CanCopy(string ext) {   //能不能复制
        foreach (string e in exts) {
            if (ext.Equals(e)) return true;
        }
        return false;
    }

    /// <summary>
    /// 载入素材
    /// </summary>
    //static UnityEngine.Object LoadAsset(string file) {
    //    if (file.EndsWith(".lua")) file += ".txt";
    //    return AssetDatabase.LoadMainAssetAtPath("Assets/LuaFramework/Examples/Builds/" + file);
    //}

    [MenuItem("LuaFramework/Build iPhone Resource", false, 100)]
    public static void BuildiPhoneResource() {
        BuildTarget target;
#if UNITY_5
        target = BuildTarget.iOS;
#else
        target = BuildTarget.iOS;
#endif
        BuildAssetResource(target);
    }

    [MenuItem("LuaFramework/Build Android Resource", false, 101)]
    public static void BuildAndroidResource() {
        BuildAssetResource(BuildTarget.Android);
    }

    [MenuItem("LuaFramework/Build Windows Resource", false, 102)]
    public static void BuildWindowsResource() {
        BuildAssetResource(BuildTarget.StandaloneWindows);
    }

    /// <summary>
    /// 生成绑定素材
    /// </summary>
    public static void BuildAssetResource(BuildTarget target) {
        //if (Directory.Exists(Util.DataPath)) {
        //    Directory.Delete(Util.DataPath, true);
        //}
        string streamPath = Application.streamingAssetsPath;
        if (Directory.Exists(streamPath)) {
            Directory.Delete(streamPath, true);
        }
        Directory.CreateDirectory(streamPath);
        AssetDatabase.Refresh();

        maps.Clear();

        //if (AppConst.LuaBundleMode)
        //{
        //    HandleLuaBundle();
        //}
        //else
        //{
        //    HandleLuaFile();
        //}
        //if (AppConst.ExampleMode) {
        HandleExampleBundle();
        //}

        //    for (int i = 0; i < maps.Count; i++)
        //{
        //    UnityEngine.Debug.Log("assetBundleName:" + maps[i].assetBundleName);
        //    for (int j = 0; j < maps[i].assetNames.Length; j++)
        //    {
        //        //UnityEngine.Debug.Log("assetNames:" + maps[i].assetNames[j]);
        //        if (!File.Exists(maps[i].assetNames[j]))
        //        {
        //            UnityEngine.Debug.LogWarning("这个文件不存在:" + maps[i].assetNames[j]);
        //        }
        //    }
        //}
        List<String> fileList = new List<string>();

        StringBuilder sb = new StringBuilder();

        //StringBuilder sb = new StringBuilder();
        /* string[] abNames = AssetDatabase.GetAllAssetBundleNames();
         foreach (var abName in abNames)
         {

             //if (!abName.StartsWith("model/customcommonres")&& !abName.StartsWith("model/room")
             //    && !abName.StartsWith("textures/customcommonres/env/"))
             {
                 var abNameNoHashPostfix = abName.EndsWith(AssetConfig.Bundle_PostFix) ?
                     abName.Substring(0, abName.Length - AssetConfig.Bundle_PostFix.Length) : abName;
                 var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(abName);

                 if (assetPaths != null && assetPaths.Length > 0 && !abName.EndsWith(AssetConfig.Bundle_PostFix))
                 {
                     throw new Exception("No .bundle postfix for AssetBundle " + abName);
                 }

                 if (!abNameNoHashPostfix.Equals(AssetConfig.AssetBundle_Build_List_Name) && assetPaths != null && assetPaths.Length > 0)
                 {
                     sb.Append(abNameNoHashPostfix).Append("\n");
                     foreach (var assetPath in assetPaths)
                     {
                         sb.Append("\t").Append(assetPath).Append("\n");

                     }
                 }
             }
             //else
             //    Debug.LogError(">>" + abName);
         }

         var dir = Path.GetDirectoryName(AssetConfig.AssetBundle_Build_List_Path);
         if (!Directory.Exists(dir))
             Directory.CreateDirectory(dir);
         File.WriteAllText(AssetConfig.AssetBundle_Build_List_Path, sb.ToString());

         AssetDatabase.Refresh();

         var importer = AssetImporter.GetAtPath(AssetConfig.AssetBundle_Build_List_Path);
        // Debug.LogError("importer:" + importer);
         importer.SetAssetBundleNameAndVariant(AssetConfig.AssetBundle_Build_List_Name + AssetConfig.Bundle_PostFix, string.Empty);
         */
        UnityEngine.Debug.LogError("maps.Count:" + maps.Count);
        foreach (var item in maps)
        {
           // output_path += "/" + item.Key;

            var output_path = GetBuildTargetOutputPath(target, item.Key);

            UnityEngine.Debug.LogError("Key:" + item.Key + ","+ item.Value.Count);

            if (!Directory.Exists(output_path))
                Directory.CreateDirectory(output_path);
            // string resPath = "Assets/" + AppConst.AssetDir;
            AssetBundleManifest manifest = null;

            manifest = BuildPipeline.BuildAssetBundles(output_path, item.Value.ToArray(), BuildAssetBundleOptions.AppendHashToAssetBundleName, target);
            // BuildFileIndex();        

            if (manifest == null)
                continue;

            UnityEngine.Debug.LogWarning("manifest:" + manifest);
            
            sb.Length = 0;
            // string[] abs = manifest.GetAllAssetBundles();
           // UnityEngine.Debug.LogError(">>>>" + abs.Length);

            //foreach (var ab in maps)
            //{
            //    var hash = "";// ab.GetHashCode().ToString();

            //    var ab_no_hash = ab.assetBundleName.Replace("_" + hash + AssetConfig.Bundle_PostFix, string.Empty);
            //    UnityEngine.Debug.LogError(">>>>"+ output_path+"," + ab_no_hash);
            //    var file = output_path + "/" + ab_no_hash;
            //    string md51 = Util.md5file(file);
            //    var len = new FileInfo(file).Length;
            //    //sb.Append(ab_no_hash).Append("|").Append(hash).Append("|").Append(len).Append("\n");
            //    sb.Append(ab_no_hash).Append("|").Append(md51).Append("|").Append(len).Append("\n");

            //    //fileList.Add(hash);
            //}
            string[] abs = manifest.GetAllAssetBundles();
            foreach (var ab in abs)
            {
                var hash = manifest.GetAssetBundleHash(ab).ToString();
                var ab_no_hash = ab.Replace("_" + hash + AssetConfig.Bundle_PostFix, string.Empty);
                var len = new FileInfo(output_path + "/" + ab).Length;
                sb.Append(ab_no_hash).Append("|").Append(hash).Append("|").Append(len).Append("\n");

                fileList.Add(hash);
            }
            // return;
            var manifestName = Path.GetFileName(output_path);
            UnityEngine.Debug.LogError("manifestName:" + manifestName);
            var md5 = Util.md5(sb.ToString());
            var newFile = output_path + "/" + AssetConfig.AssetBundleManifest_Name + "_" + md5 + AssetConfig.Bundle_PostFix;
            if (File.Exists(newFile))
                File.Delete(newFile);
            File.Move(output_path + "/" + manifestName, newFile);
        
        var manifest_len = new FileInfo(newFile).Length;
        var new_sb = new StringBuilder();
        TimeSpan timeSpan = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        new_sb.Append(AssetConfig.Version)
            .Append("#")
            .Append(item.Value.Count + 1)
            .Append("#")
            .Append(Convert.ToInt64(timeSpan.TotalMilliseconds))
            .Append("\n");
        new_sb.Append(sb);
        new_sb.Append(AssetConfig.AssetBundleManifest_Name)
            .Append("|")
            .Append(md5)
            .Append("|")
            .Append(manifest_len)
            .Append("\n");

        fileList.Add(md5);
        fileList.Add(AssetConfig.File_List_Name);
        //Debug.LogError("output_path:" + output_path);
        File.WriteAllText(output_path + "/" + AssetConfig.File_List_Name, new_sb.ToString());
    
        //string streamDir = Application.dataPath + "/" + AppConst.LuaTempDir;
        //if (Directory.Exists(streamDir)) Directory.Delete(streamDir, true);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        
            DeleteAllFiles(output_path.Replace("StreamingAssets", "StreamingAssetsPublish"));
        CopyUsefulFiles(output_path, fileList);
        }
        
        //*/
        UnityEngine.Debug.Log("done!");
    }
    public static string AssetBundle_Output_Path = "StreamingAssets";
    static string GetBuildTargetOutputPath(BuildTarget target, string dir)
    {
        if (target == BuildTarget.Android)
            return AssetBundle_Output_Path +"/"+ dir + "/Android";

        if (target == BuildTarget.iOS)
            return AssetBundle_Output_Path + "/" + dir + "/iOS";

        return AssetBundle_Output_Path + "/" + dir + "/Win";
    }
    static void DeleteAllFiles(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        if (directoryInfo.Exists)
        {
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                fileInfo.Delete();
            }
            foreach (DirectoryInfo directoryInfoTemp in directoryInfo.GetDirectories())
            {
                DeleteAllFiles(directoryInfoTemp.FullName);
            }
        }
    }

    static void CopyUsefulFiles(string output_path, List<String> fileList)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(output_path);
        foreach (FileInfo fileInfo in directoryInfo.GetFiles())
        {
            bool needCopy = false;
            foreach (string filename in fileList)
            {
                if (fileInfo.Name.Contains(filename))
                {
                    needCopy = true;
                    break;
                }
            }
            if (needCopy)
            {
                DirectoryInfo tempInfo = new DirectoryInfo(fileInfo.DirectoryName.Replace("StreamingAssets", "StreamingAssetsPublish"));
                if (!tempInfo.Exists)
                {
                    tempInfo.Create();
                }
                fileInfo.CopyTo(fileInfo.FullName.Replace("StreamingAssets", "StreamingAssetsPublish"), true);
            }
        }
        foreach (DirectoryInfo directoryInfoTemp in directoryInfo.GetDirectories())
        {
            CopyUsefulFiles(directoryInfoTemp.FullName, fileList);
        }
    }

    static void AddBuildMap(string projectName, string bundleName, string pattern, string path) {
		//Elaine Add begain
        //string[] files = Directory.GetFiles(path, pattern);
		string[] files1 = null;
        if (pattern.Contains("|"))
        {
            string[] spl = pattern.Split('|');
            files1 = Directory.GetFiles(path).Where((x) =>
            {
                bool result = false;
                for (int i = 0; i < spl.Length; i++)
                {
                    result |= x.EndsWith(spl[i], StringComparison.OrdinalIgnoreCase);
                    if (result) return true;
                }
                return result;
            }).ToArray();
        }
        else if (pattern.Contains("*all*"))
        {
           // files = Directory.GetFiles(path);

            paths.Clear();
            files.Clear();
            Recursive(path);
            files1 = paths.ToArray();
        }
        else
        {
            files1 = Directory.GetFiles(path, pattern);
        }
        
		//Elaine Add end
		
        if (files1.Length == 0) return;

        for (int i = 0; i < files1.Length; i++) {
            files1[i] = files1[i].Replace('\\', '/');
        }
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = bundleName;
        build.assetNames = files1;
        //UnityEngine.Debug.LogError("Add:" + bundleName+","+ files1.Length);

        if (!maps.ContainsKey(projectName))
        {
            maps.Add(projectName, new List<AssetBundleBuild>());
        }

        List<AssetBundleBuild> list = maps[projectName];
       
        list.Add(build);
    }

    /// <summary>
    /// 处理Lua代码包
    /// </summary>
    //static void HandleLuaBundle() {
    //    string streamDir = Application.dataPath + "/" + AppConst.LuaTempDir;
    //    if (!Directory.Exists(streamDir)) Directory.CreateDirectory(streamDir);

    //    string[] srcDirs = { CustomSettings.luaDir, CustomSettings.FrameworkPath + "/ToLua/Lua" };
    //    for (int i = 0; i < srcDirs.Length; i++) {
    //        if (AppConst.LuaByteMode) {
    //            string sourceDir = srcDirs[i];
    //            string[] files = Directory.GetFiles(sourceDir, "*.lua", SearchOption.AllDirectories);
    //            int len = sourceDir.Length;

    //            if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\') {
    //                --len;
    //            }
    //            for (int j = 0; j < files.Length; j++) {
    //                string str = files[j].Remove(0, len);
    //                string dest = streamDir + str + ".bytes";
    //                string dir = Path.GetDirectoryName(dest);
    //                Directory.CreateDirectory(dir);
    //                EncodeLuaFile(files[j], dest);
    //            }    
    //        } else {
    //            ToLuaMenu.CopyLuaBytesFiles(srcDirs[i], streamDir);
    //        }
    //    }
    //    string[] dirs = Directory.GetDirectories(streamDir, "*", SearchOption.AllDirectories);
    //    for (int i = 0; i < dirs.Length; i++) {
    //        string name = dirs[i].Replace(streamDir, string.Empty);
    //        name = name.Replace('\\', '_').Replace('/', '_');
    //        name = "lua/lua_" + name.ToLower() + AppConst.ExtName;

    //        string path = "Assets" + dirs[i].Replace(Application.dataPath, "");
    //        AddBuildMap(name, "*.bytes", path);
    //    }
    //    AddBuildMap("lua/lua" + AppConst.ExtName, "*.bytes", "Assets/" + AppConst.LuaTempDir);

    //    //-------------------------------处理非Lua文件----------------------------------
    //    string luaPath = AppDataPath + "/StreamingAssets/lua/";
    //    for (int i = 0; i < srcDirs.Length; i++) {
    //        paths.Clear(); files.Clear();
    //        string luaDataPath = srcDirs[i].ToLower();
    //        Recursive(luaDataPath);
    //        foreach (string f in files) {
    //            if (f.EndsWith(".meta") || f.EndsWith(".lua")) continue;
    //            string newfile = f.Replace(luaDataPath, "");
    //            string path = Path.GetDirectoryName(luaPath + newfile);
    //            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

    //            string destfile = path + "/" + Path.GetFileName(f);
    //            File.Copy(f, destfile, true);
    //        }
    //    }
    //    AssetDatabase.Refresh();
    //}

    /// <summary>
    /// 处理框架实例包
    /// </summary>
    static void HandleExampleBundle() {
        string resPath = AppDataPath + "/" + AppConst.AssetDir + "/";
        if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);
		//Elaine Add
/*
        AddBuildMap("prompt" + AppConst.ExtName, "*.prefab", "Assets/LuaFramework/Examples/Builds/Prompt");
        AddBuildMap("message" + AppConst.ExtName, "*.prefab", "Assets/LuaFramework/Examples/Builds/Message");

        AddBuildMap("prompt_asset" + AppConst.ExtName, "*.png", "Assets/LuaFramework/Examples/Textures/Prompt");
        AddBuildMap("shared_asset" + AppConst.ExtName, "*.png", "Assets/LuaFramework/Examples/Textures/Shared");
//*/
		string content = File.ReadAllText(Application.dataPath + "/AssetBundleInfo.csv");
        string[] contents = content.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        UnityEngine.Debug.LogError("contents.Length:" + contents.Length);
        for (int i = 0; i < contents.Length; i++)
        {
            //if (contents[i][0] == '<') continue;
            //string[] a = contents[i].Split(',');
            //UnityEngine.Debug.Log(a[0]); UnityEngine.Debug.Log(a[1]); UnityEngine.Debug.Log(a[2]);
            //var name = a[0].Replace('_', '/');//.Replace('/', '_');
            //AddBuildMap(a[0], a[1], a[2]);

            string oneLine = contents[i];
            if (oneLine[0] == '<')
            {
                oneLine = oneLine.Trim(new[] { '<', '>', ',' });
                string[] spx = oneLine.Split(',');
                string pName = spx[0]; //项目名称
                int abCount = int.Parse(spx[1]); //项目AssetBundle数量

                UnityEngine.Debug.LogError("pName:" + pName+","+ abCount);

                // pName = AddProject(pName);
                // countMap[pName] = abCount;
                for (int j = 0; j < abCount; j++)
                {
                    string[] a = contents[i + j + 1].Split(',');
                   // AddItem(pName, a[0], StringToEnum(a[1]), a[2]);

                    AddBuildMap(pName, a[0], a[1], a[2]);
                }

                
            }
        }
    }

    /// <summary>
    /// 处理Lua文件
    /// </summary>
    //static void HandleLuaFile() {
    //    string resPath = AppDataPath + "/StreamingAssets/";
    //    string luaPath = resPath + "/lua/";

    //    //----------复制Lua文件----------------
    //    if (!Directory.Exists(luaPath)) {
    //        Directory.CreateDirectory(luaPath); 
    //    }
    //    string[] luaPaths = { AppDataPath + "/LuaFramework/lua/", 
    //                          AppDataPath + "/LuaFramework/Tolua/Lua/" };

    //    for (int i = 0; i < luaPaths.Length; i++) {
    //        paths.Clear(); files.Clear();
    //        string luaDataPath = luaPaths[i].ToLower();
    //        Recursive(luaDataPath);
    //        int n = 0;
    //        foreach (string f in files) {
    //            if (f.EndsWith(".meta")) continue;
    //            string newfile = f.Replace(luaDataPath, "");
    //            string newpath = luaPath + newfile;
    //            string path = Path.GetDirectoryName(newpath);
    //            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

    //            if (File.Exists(newpath)) {
    //                File.Delete(newpath);
    //            }
    //            if (AppConst.LuaByteMode) {
    //                EncodeLuaFile(f, newpath);
    //            } else {
    //                File.Copy(f, newpath, true);
    //            }
    //            UpdateProgress(n++, files.Count, newpath);
    //        } 
    //    }
    //    EditorUtility.ClearProgressBar();
    //    AssetDatabase.Refresh();
    //}

    static void BuildFileIndex() {
        string resPath = AppDataPath + "/StreamingAssets/";
        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "/files.txt";
        if (File.Exists(newFilePath))
            File.Delete(newFilePath);

        paths.Clear();
        files.Clear();
        Recursive(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++) {
            string file = files[i];
            string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store")) continue;

            string md5 = Util.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            sw.WriteLine(value + "|" + md5);
        }
        sw.Close();
        fs.Close();
    }

    /// <summary>
    /// 数据目录
    /// </summary>
    static string AppDataPath {
        get { return Application.dataPath.ToLower(); }
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path) {
        //UnityEngine.Debug.LogError("path:" + path);
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names) {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs) {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    static void UpdateProgress(int progress, int progressMax, string desc) {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }

    //public static void EncodeLuaFile(string srcFile, string outFile) {
    //    if (!srcFile.ToLower().EndsWith(".lua")) {
    //        File.Copy(srcFile, outFile, true);
    //        return;
    //    }
    //    bool isWin = true; 
    //    string luaexe = string.Empty;
    //    string args = string.Empty;
    //    string exedir = string.Empty;
    //    string currDir = Directory.GetCurrentDirectory();
    //    if (Application.platform == RuntimePlatform.WindowsEditor) {
    //        isWin = true;
    //        luaexe = "luajit.exe";
    //        args = "-b -g " + srcFile + " " + outFile;
    //        exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit/";
    //    } else if (Application.platform == RuntimePlatform.OSXEditor) {
    //        isWin = false;
    //        luaexe = "./luajit";
    //        args = "-b -g " + srcFile + " " + outFile;
    //        exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit_mac/";
    //    }
    //    Directory.SetCurrentDirectory(exedir);
    //    ProcessStartInfo info = new ProcessStartInfo();
    //    info.FileName = luaexe;
    //    info.Arguments = args;
    //    info.WindowStyle = ProcessWindowStyle.Hidden;
    //    info.UseShellExecute = isWin;
    //    info.ErrorDialog = true;
    //    //Util.Log(info.FileName + " " + info.Arguments);

    //    Process pro = Process.Start(info);
    //    pro.WaitForExit();
    //    Directory.SetCurrentDirectory(currDir);
    //}

    //[MenuItem("LuaFramework/Build Protobuf-lua-gen File")]
    //public static void BuildProtobufFile() {
    //    if (!AppConst.ExampleMode) {
    //        UnityEngine.Debug.LogError("若使用编码Protobuf-lua-gen功能，需要自己配置外部环境！！");
    //        return;
    //    }
    //    string dir = AppDataPath + "/Lua/3rd/pblua";
    //    paths.Clear(); files.Clear(); Recursive(dir);

    //    string protoc = "d:/protobuf-2.4.1/src/protoc.exe";
    //    string protoc_gen_dir = "\"d:/protoc-gen-lua/plugin/protoc-gen-lua.bat\"";

    //    foreach (string f in files) {
    //        string name = Path.GetFileName(f);
    //        string ext = Path.GetExtension(f);
    //        if (!ext.Equals(".proto")) continue;

    //        ProcessStartInfo info = new ProcessStartInfo();
    //        info.FileName = protoc;
    //        info.Arguments = " --lua_out=./ --plugin=protoc-gen-lua=" + protoc_gen_dir + " " + name;
    //        info.WindowStyle = ProcessWindowStyle.Hidden;
    //        info.UseShellExecute = true;
    //        info.WorkingDirectory = dir;
    //        info.ErrorDialog = true;
    //        //Util.Log(info.FileName + " " + info.Arguments);

    //        Process pro = Process.Start(info);
    //        pro.WaitForExit();
    //    }
    //    AssetDatabase.Refresh();
    //}
}
