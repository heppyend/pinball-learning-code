using HybridCLR.Editor.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor
{
    public static class AssetBundleBuildCommand
    {
        public static string HybridCLRBuildCacheDir => Application.dataPath + "/HybridCLRBuildCache";

        public static string AssetBundleOutputDir => $"{HybridCLRBuildCacheDir}/AssetBundleOutput";

        //public static string AssetBundleSourceDataTempDir => $"{HybridCLRBuildCacheDir}/AssetBundleSourceData";
        public static string AssetBundleSourceDataTempDir => $"{Application.dataPath}/HotUpdateResources/Dll";

        public static string GetAssetBundleOutputDirByTarget(BuildTarget target)
        {
            return $"{AssetBundleOutputDir}/{target}";
        }

        public static string GetAssetBundleTempDirByTarget(BuildTarget target)
        {
            return $"{AssetBundleSourceDataTempDir}/{target}";
        }

        public static string ToRelativeAssetPath(string s)
        {
            return s.Substring(s.IndexOf("Assets/"));
        }

        /// <summary>
        /// 将HotFix.dll和HotUpdatePrefab.prefab打入common包.
        /// 将HotUpdateScene.unity打入scene包.
        /// </summary>
        /// <param name="tempDir"></param>
        /// <param name="outputDir"></param>
        /// <param name="target"></param>
        private static void BuildAssetBundles(string tempDir, string outputDir, BuildTarget target)
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(outputDir);
            //CompileDllCommand.CompileDll(target);

            List<string> notSceneAssets = new List<string>();

            string hotfixDllSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            Debug.Log(SettingsUtil.HotUpdateAssemblyNamesExcludePreserved.Count);
            foreach (var dll in SettingsUtil.HotUpdateAssemblyNamesExcludePreserved)
            {
                Debug.Log($"{hotfixDllSrcDir}/{dll}.dll");
                string dllPath = $"{hotfixDllSrcDir}/{dll}.dll";
                Debug.Log(dllPath);
                string dllBytesPath = $"{tempDir}/{dll}.dll.bytes";
                File.Copy(dllPath, dllBytesPath, true);
                notSceneAssets.Add(dllBytesPath);
                Debug.Log($"[BuildAssetBundles] copy hotfix dll {dllPath} -> {dllBytesPath}");
            }

            List<string> failList = new List<string>();
            var aotDlls = HybridCLRSettings.Instance.patchAOTAssemblies.Select(dll => dll + ".dll").ToArray();

            string aotDllDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            Debug.Log(aotDllDir.Length);
            foreach (var dll in aotDlls)
            {
                Debug.Log($"{aotDllDir}/{dll}");
                string dllPath = $"{aotDllDir}/{dll}";
                if (!File.Exists(dllPath))
                {
                    Debug.LogError($"ab中添加AOT补充元数据dll:{dllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    continue;
                }
                string dllBytesPath = $"{tempDir}/{dll}.bytes";
                File.Copy(dllPath, dllBytesPath, true);
                notSceneAssets.Add(dllBytesPath);
                Debug.Log($"[BuildAssetBundles] copy AOT dll {dllPath} -> {dllBytesPath}");
            }
        }

        public static void BuildAssetBundleByTarget(BuildTarget target)
        {
            BuildAssetBundles(GetAssetBundleTempDirByTarget(target), GetAssetBundleOutputDirByTarget(target), target);
        }

        [MenuItem("HybridCLR/BuildBundles/ActiveBuildTarget")]
        public static void BuildSceneAssetBundleActiveBuildTarget()
        {
            BuildAssetBundleByTarget(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("HybridCLR/BuildBundles/Win64")]
        public static void BuildSceneAssetBundleWin64()
        {
            var target = BuildTarget.StandaloneWindows64;
            BuildAssetBundleByTarget(target);
        }

        [MenuItem("HybridCLR/BuildBundles/Win32")]
        public static void BuildSceneAssetBundleWin32()
        {
            var target = BuildTarget.StandaloneWindows;
            BuildAssetBundleByTarget(target);
        }

        [MenuItem("HybridCLR/BuildBundles/Android")]
        public static void BuildSceneAssetBundleAndroid()
        {
            var target = BuildTarget.Android;
            BuildAssetBundleByTarget(target);
        }

        [MenuItem("HybridCLR/BuildBundles/IOS")]
        public static void BuildSceneAssetBundleIOS()
        {
            var target = BuildTarget.iOS;
            BuildAssetBundleByTarget(target);
        }
    }
}
