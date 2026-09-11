using System;
using System.IO;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Pinball.EditorTools
{
    /// <summary>
    /// Keeps the WebGL HotFix asset-bundle input in sync with the freshly compiled HybridCLR output.
    /// This is intentionally WebGL-only so it cannot overwrite another platform's output.
    /// </summary>
    public static class WebGLHotUpdateDllCommand
    {
        private const BuildTarget Target = BuildTarget.WebGL;
        private const string HotFixAssemblyName = "HotFix.dll";

        [MenuItem("Build/WebGL/Compile And Sync HotFix DLL")]
        public static void CompileAndSync()
        {
            CompileDllCommand.CompileDll(Target);

            var sourcePath = Path.Combine(HybridCLR.Editor.SettingsUtil.GetHotUpdateDllsOutputDirByTarget(Target), HotFixAssemblyName);
            var destinationPath = Path.Combine(Application.dataPath, "HotUpdateResources", "Dll", "WebGL", HotFixAssemblyName + ".bytes");
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("HybridCLR did not produce the WebGL HotFix DLL.", sourcePath);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? throw new InvalidOperationException("Invalid HotFix destination path."));
            File.Copy(sourcePath, destinationPath, true);
            AssetDatabase.Refresh();

            var outputSize = new FileInfo(destinationPath).Length;
            Debug.Log($"[WebGL 热更新程序集] 已编译并同步 HotFix.dll：{sourcePath} -> {destinationPath}（{outputSize} bytes）");
        }
    }
}
