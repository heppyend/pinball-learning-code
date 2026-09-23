using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;
using WeChatWASM;
using Pinball.EditorTools;
using static WeChatWASM.WXConvertCore;

namespace Pinball.Client.Editor
{
    /// <summary>
    /// WebGL / 微信小游戏移植链路的**命令行入口**。
    ///
    /// <para><b>为什么需要它</b>：`HANDOVER_PLAN.md` §5 B1 的构建链路里，
    /// YooAsset 打包（`YooAsset/AssetBundle Builder`）与微信转换（`WXEditorWindow`）原本只有 GUI 入口，
    /// 代理不得代操作 GUI（`AGENTS.md` §4）⇒ 构建无法复现、每次都要人工点。
    /// 本文件把这两个入口包成 public static，使其可用 `-batchmode -executeMethod` 触发。</para>
    ///
    /// <para><b>等价性</b>：`BuildBundles` 逐项照抄 `BuiltinBuildPipelineViewer.ExecuteBuild()` 的取值方式
    /// （同样从 `AssetBundleBuilderSetting` 读持久化设置、同样的输出根与首包拷贝选项、同一个 `BuiltinBuildPipeline`），
    /// 因此命令行产出与 GUI 点击产出**同构**；`ConvertMiniGame` 直接调用官方 `WXConvertCore.DoExport(true)`。
    /// **本文件不新增任何构建逻辑，也不做任何平台设置改写。**</para>
    ///
    /// <para><b>用法</b>（`ALLUSERSPROFILE` 必须临时传入，见 `AGENTS.md` §3）：</para>
    /// <code>
    /// $env:ALLUSERSPROFILE = 'C:\ProgramData'
    /// $unity = 'C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe'
    /// $proj  = 'D:\unity project\pinball'
    ///
    /// # ① 只打 YooAsset 资源包（版本号默认取“当前时间”式，与 GUI 一致）
    /// &amp; $unity -batchmode -quit -projectPath $proj `
    ///   -executeMethod Pinball.Client.Editor.ClientWebGLBuildCommand.BuildBundles `
    ///   -logFile "$proj\Logs\b1-yooasset-bundles.log"
    ///
    /// # ② 打资源包 + 同步 WebGL 热更 DLL
    /// &amp; $unity -batchmode -quit -projectPath $proj `
    ///   -executeMethod Pinball.Client.Editor.ClientWebGLBuildCommand.BuildBundlesAndHotFix `
    ///   -logFile "$proj\Logs\b1-bundles-hotfix.log"
    ///
    /// # ③ 只同步 WebGL 热更 DLL（等价于菜单 Build/WebGL/Compile And Sync HotFix DLL）
    /// &amp; $unity -batchmode -quit -projectPath $proj `
    ///   -executeMethod Pinball.Client.Editor.ClientWebGLBuildCommand.SyncHotFixDll `
    ///   -logFile "$proj\Logs\b1-hotfix.log"
    ///
    /// # ④ WebGL Build + 微信小游戏转换（等价于转换面板点“转换”）
    /// &amp; $unity -batchmode -quit -projectPath $proj `
    ///   -executeMethod Pinball.Client.Editor.ClientWebGLBuildCommand.ConvertMiniGame `
    ///   -logFile "$proj\Logs\b1-wx-convert.log"
    /// </code>
    ///
    /// <para><b>退出码</b>：成功 0；失败非 0（`EditorApplication.Exit`），便于脚本判定。</para>
    /// </summary>
    public static class ClientWebGLBuildCommand
    {
        private const string DefaultPackageName = "DefaultPackage";
        private const string HotFixAssemblyName = "HotFix.dll";

        /// <summary>
        /// 与 GUI 的 `GetDefaultPackageVersion()` 同式：`yyyy-MM-dd-<当天分钟数>`。
        /// </summary>
        private static string MakeDefaultPackageVersion()
        {
            int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
        }

        /// <summary>
        /// 构建 YooAsset 资源包。产物目录：`{项目}/Bundles/{BuildTarget}/{包名}/{版本}`。
        /// 版本号可用 `-packageVersion &lt;值&gt;` 覆盖，便于复现某一次构建。
        /// </summary>
        public static void BuildBundles()
        {
            string packageVersion = ReadArg("-packageVersion", MakeDefaultPackageVersion());
            int exitCode = RunBuildBundles(packageVersion);
            EditorApplication.Exit(exitCode);
        }

        /// <summary>构建资源包，成功后再同步 WebGL 热更 DLL。</summary>
        public static void BuildBundlesAndHotFix()
        {
            string packageVersion = ReadArg("-packageVersion", MakeDefaultPackageVersion());
            int exitCode = RunBuildBundles(packageVersion);
            if (exitCode == 0)
                exitCode = RunSyncHotFixDll();
            EditorApplication.Exit(exitCode);
        }

        /// <summary>只同步 WebGL 热更 DLL（`Assets/HotUpdateResources/Dll/WebGL/HotFix.dll.bytes`）。</summary>
        public static void SyncHotFixDll()
        {
            EditorApplication.Exit(RunSyncHotFixDll());
        }

        /// <summary>
        /// **只读**校验客户端启动链的接线（不写任何文件）。
        /// 关闭条件来自 `BUG-025`："WebGL 构建产物能进入客户端 `ClientShell`"——
        /// 这里把它拆成构建前可机器判定的三条：入口场景有加载器、客户端场景在清单里、TCP 探针有宿主场景。
        /// </summary>
        public static void VerifyStartupChain()
        {
            EditorApplication.Exit(RunVerifyStartupChain());
        }

        /// <summary>
        /// 校验启动链接线。返回 0 表示通过，非 0 表示存在未接线项。
        ///
        /// <para>为什么单独做：`BUG-025` 的根因是"场景在清单外 + 无代码加载"，
        /// 这类问题的共同点是**编辑器编译全部通过、只有真机/构建后才暴露**。
        /// 本方法把它提前到构建前判定。</para>
        /// </summary>
        public static int RunVerifyStartupChain()
        {
            // 设计 A（2026-09-22 负责人选定）：回归历史链路。
            // 入口 = Boot.unity（它负责 YooAsset → AOT 元数据 → 加载 HotFix），
            // Boot 上的 Init 通过 ClientStartScene 直接把首个场景指向 ClientShell。
            // TCP 探针挂在 Boot.unity 的 Init 对象上（历史位置），不序列化在任何客户端场景里。
            const string bootScenePath = "Assets/Main/Boot.unity";
            const string shellScenePath = "Assets/Client/Scenes/ClientShell.unity";
            const string probeToken = "8c9ef21efe1aa4c4e912a27659d090c4";      // ClientTcpConnectionProbe.cs
            const string initToken = "c8c618737b9bb5e419d59cd80172d4e0";       // Init.cs
            const string deprecatedClientBootPath = "Assets/Client/Scenes/ClientBoot.unity";

            int failures = 0;

            // ① 入口场景存在。
            var bootScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootScenePath);
            if (bootScene == null)
            {
                Debug.LogError($"[启动链校验] 入口场景不存在：{bootScenePath}");
                failures++;
            }
            else
            {
                Debug.Log($"[启动链校验] 入口场景存在：{bootScenePath}");
            }

            // ② 入口场景必须排在构建清单首位且启用；客户端场景必须在清单内。
            var scenes = UnityEditor.EditorBuildSettings.scenes;
            Debug.Log($"[启动链校验] 构建清单场景数 = {scenes.Length}");
            for (int i = 0; i < scenes.Length; i++)
                Debug.Log($"[启动链校验]   场景[{i}] enabled={scenes[i].enabled} path={scenes[i].path}");

            if (scenes.Length == 0 || scenes[0].path != bootScenePath || !scenes[0].enabled)
            {
                Debug.LogError($"[启动链校验] 入口场景未排在构建清单首位且启用：{bootScenePath}");
                failures++;
            }

            if (!ContainsEnabledScene(scenes, shellScenePath))
            {
                Debug.LogError($"[启动链校验] 客户端场景未加入构建清单或未启用：{shellScenePath}");
                failures++;
            }

            if (ContainsEnabledScene(scenes, deprecatedClientBootPath))
            {
                Debug.LogError($"[启动链校验] 已弃用的 ClientBoot 仍在构建清单里（设计 A 下它会让 Boot/Init 不被加载，" +
                               $"导致 HotFix 与 TCP 断链）：{deprecatedClientBootPath}");
                failures++;
            }

            // ③ 客户端场景资产必须存在。
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(shellScenePath) == null)
            {
                Debug.LogError($"[启动链校验] 客户端场景不存在：{shellScenePath}");
                failures++;
            }

            // ④ 入口场景必须同时持有 Init 与 TCP 探针。
            //    探针挂在 Init 对象上是**历史位置**，它靠自身的 StartPersistentProbe() 迁移到
            //    DontDestroyOnLoad 对象存活过场景切换；而 HotFix（含反射目标 NetController）
            //    只有 Init 能加载 —— 两者必须在同一场景，否则探针必然等超时。
            int initRefs = CountOccurrences(bootScenePath, initToken);
            int probeRefs = CountOccurrences(bootScenePath, probeToken);

            if (initRefs == 0)
            {
                Debug.LogError($"[启动链校验] 入口场景里没有 Init（HotFix/AOT 元数据将不会被加载）：{bootScenePath}");
                failures++;
            }
            else
            {
                Debug.Log($"[启动链校验] 入口场景持有 Init（{initRefs} 处）—— YooAsset / AOT 元数据 / HotFix 加载链在此。");
            }

            if (probeRefs == 0)
            {
                Debug.LogError($"[启动链校验] 入口场景里没有 TCP 探针：{bootScenePath}");
                failures++;
            }
            else
            {
                Debug.Log($"[启动链校验] 入口场景持有 TCP 探针（{probeRefs} 处）—— 与 Init 同场景，能反射到 HotFix。");
            }

            if (failures == 0)
            {
                Debug.Log("[启动链校验] 通过：入口 → 客户端 → 网络 三层接线齐备。");
                return 0;
            }

            Debug.LogError($"[启动链校验] 失败项 = {failures}");
            return 1;
        }

        private static bool ContainsEnabledScene(EditorBuildSettingsScene[] scenes, string path)
        {
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled && scenes[i].path == path)
                    return true;
            }
            return false;
        }

        private static int CountOccurrences(string assetPath, string token)
        {
            // batchmode 下 CurrentDirectory 即项目根目录，assetPath 用正斜杠的工程相对路径。
            string fullPath = System.IO.Path.Combine(
                System.IO.Directory.GetCurrentDirectory(), assetPath.Replace('/', System.IO.Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fullPath))
            {
                Debug.LogWarning($"[启动链校验] 待查文件不存在：{fullPath}");
                return 0;
            }

            int count = 0;
            foreach (string line in System.IO.File.ReadLines(fullPath))
            {
                if (line.IndexOf(token, StringComparison.Ordinal) >= 0)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// WebGL 构建 + 微信小游戏转换。走官方 `WXConvertCore.DoExport(true)`：
        /// 它内部自带 `PreCheck()`（校验导出路径已配置）与 `PreInit()`，并会按 `MiniGameConfig.DST` 输出。
        /// </summary>
        public static void ConvertMiniGame()
        {
            Debug.Log("[WebGL构建] 调用 WXConvertCore.DoExport(buildWebGL: true) —— WebGL Build + 小游戏转换");
            WXExportError result = WXConvertCore.DoExport(true);
            Debug.Log($"[WebGL构建] WXConvertCore.DoExport 返回：{result}");

            if (result == WXExportError.SUCCEED)
            {
                Debug.Log("[WebGL构建] 小游戏转换成功。");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[WebGL构建] 小游戏转换失败：{result}");
                EditorApplication.Exit(1);
            }
        }

        // ------------------------------------------------------------------
        // 以下两个 Run* 是**纯逻辑**入口（不调用 EditorApplication.Exit），
        // 便于被其它 executeMethod 或后续组合入口复用。
        // ------------------------------------------------------------------

        /// <summary>
        /// 构建资源包。返回 0 表示成功。
        /// 取值方式与 `BuiltinBuildPipelineViewer.ExecuteBuild()` 一一对应，仅补上 `buildTarget`。
        /// </summary>
        public static int RunBuildBundles(string packageVersion)
        {
            if (string.IsNullOrEmpty(packageVersion))
            {
                Debug.LogError("[WebGL构建] packageVersion 为空，终止。");
                return 1;
            }

            const EBuildPipeline pipeline = EBuildPipeline.BuiltinBuildPipeline;
            const BuildTarget buildTarget = BuildTarget.WebGL;

            // 与 GUI 完全相同的持久化设置来源（EditorPrefs，键名在 AssetBundleBuilderSetting 内）。
            var buildMode = AssetBundleBuilderSetting.GetPackageBuildMode(DefaultPackageName, pipeline);
            var fileNameStyle = AssetBundleBuilderSetting.GetPackageFileNameStyle(DefaultPackageName, pipeline);
            var buildinFileCopyOption = AssetBundleBuilderSetting.GetPackageBuildinFileCopyOption(DefaultPackageName, pipeline);
            var buildinFileCopyParams = AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(DefaultPackageName, pipeline);
            var compressOption = AssetBundleBuilderSetting.GetPackageCompressOption(DefaultPackageName, pipeline);

            var buildParameters = new BuiltinBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = pipeline.ToString();
            buildParameters.BuildTarget = buildTarget;
            buildParameters.BuildMode = buildMode;
            buildParameters.PackageName = DefaultPackageName;
            buildParameters.PackageVersion = packageVersion;
            buildParameters.EnableSharePackRule = true;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = fileNameStyle;
            buildParameters.BuildinFileCopyOption = buildinFileCopyOption;
            buildParameters.BuildinFileCopyParams = buildinFileCopyParams;
            buildParameters.CompressOption = compressOption;

            Debug.Log($"[WebGL构建] YooAsset 开始：包={DefaultPackageName}，目标={buildTarget}，管线={pipeline}，" +
                      $"版本={packageVersion}，模式={buildMode}，压缩={compressOption}，文件名样式={fileNameStyle}，" +
                      $"首包拷贝={buildinFileCopyOption}（参数='{buildinFileCopyParams}'）");

            BuildResult buildResult = new BuiltinBuildPipeline().Run(buildParameters, true);

            if (buildResult.Success)
            {
                Debug.Log($"[WebGL构建] YooAsset 构建成功，产物目录：{buildResult.OutputPackageDirectory}（版本 {packageVersion}）");
                return 0;
            }

            Debug.LogError($"[WebGL构建] YooAsset 构建失败：失败任务={buildResult.FailedTask}；错误={buildResult.ErrorInfo}");
            return 1;
        }

        /// <summary>
        /// 重新编译 HybridCLR 的 WebGL 热更程序集，并把 `HotFix.dll` 同步到运行时加载路径。
        ///
        /// <para><b>为什么不复用 `WebGLHotUpdateDllCommand.CompileAndSync()`</b>：那个方法最后会调用
        /// `AssetDatabase.Refresh()`，在批处理里会触发**域重载并中断当前正在执行的方法**，
        /// 使多步管线在它之后的部分全部丢失。这里改为先编译、拷贝完成后再单独触发一次导入，
        /// 从而既保证资产是最新的，又不打断管线。</para>
        ///
        /// <para>复用既有工具语义（同一 `CompileDllCommand`、同一目标、同一 development 开关），
        /// 只去掉会中断管线的刷新时机。</para>
        /// </summary>
        public static int RunSyncHotFixDll()
        {
            try
            {
                string output = CompileHotFixWebGL();

                string path = System.IO.Path.Combine(
                    Application.dataPath, "HotUpdateResources", "Dll", "WebGL", HotFixAssemblyName + ".bytes");

                if (!System.IO.File.Exists(path))
                {
                    Debug.LogError($"[WebGL构建] 热更 DLL 同步后未找到文件：{path}");
                    return 1;
                }

                // 单独重导入该资产，使其在随后的资源包构建中可用；
                // 这里不用 AssetDatabase.Refresh()，避免域重载打断管线。
                AssetDatabase.ImportAsset(
                    "Assets/HotUpdateResources/Dll/WebGL/" + HotFixAssemblyName + ".bytes",
                    ImportAssetOptions.ForceUpdate);

                var info = new System.IO.FileInfo(path);
                Debug.Log($"[WebGL构建] 热更 DLL 已同步：{path}（{info.Length} bytes，{info.LastWriteTime:yyyy-MM-dd HH:mm:ss}）；HybridCLR 输出目录={output}");
                return 0;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[WebGL构建] 热更 DLL 同步失败：{exception}");
                return 1;
            }
        }

        /// <summary>
        /// 编译 WebGL 热更程序集，并把 `HotFix.dll` 拷到 `HotUpdateResources/Dll/WebGL/HotFix.dll.bytes`。
        /// **同时负责把 WebGL 播放器程序集刷新到当前源码状态** —— 这一步是资源包构建的前置条件：
        /// 若播放器侧程序集落后于编辑器侧，`BuildAssetBundles` 会直接报
        /// `Error building player because script class layout is incompatible between the editor and the player.`
        /// （实测 2026-09-22，见 `Logs/b1-full-pipeline-20260922-133928.log`）。
        /// </summary>
        /// <returns>HybridCLR 的热更 DLL 输出目录。</returns>
        private static string CompileHotFixWebGL()
        {
            const BuildTarget target = BuildTarget.WebGL;

            Debug.Log($"[WebGL构建] 编译 WebGL 热更程序集：target={target}，development={EditorUserBuildSettings.development}");
            HybridCLR.Editor.Commands.CompileDllCommand.CompileDll(target, EditorUserBuildSettings.development);

            string outputDir = HybridCLR.Editor.SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            string sourcePath = System.IO.Path.Combine(outputDir, HotFixAssemblyName);
            if (!System.IO.File.Exists(sourcePath))
                throw new System.IO.FileNotFoundException($"HybridCLR did not produce the WebGL HotFix DLL: {sourcePath}", sourcePath);

            string destinationPath = System.IO.Path.Combine(
                Application.dataPath, "HotUpdateResources", "Dll", "WebGL", HotFixAssemblyName + ".bytes");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destinationPath));
            System.IO.File.Copy(sourcePath, destinationPath, true);

            return outputDir;
        }

        /// <summary>
        /// **一次 Unity 启动跑完整个移植构建链**。存在的理由：每一步都要冷启动一个完整
        /// Unity 工程（导入 + 编译 Assembly-CSharp + ILPP，实测单次 3~8 分钟），
        /// 分步调用 5 步 = 5 次冷启动，代价过高。本入口把它们串成一次。
        ///
        /// <para>用法：</para>
        /// <code>
        /// &amp; $unity -batchmode -quit -projectPath $proj `
        ///   -executeMethod Pinball.Client.Editor.ClientWebGLBuildCommand.RunFullPortPipeline `
        ///   -logFile "$proj\Logs\b1-full-pipeline.log"
        /// </code>
        ///
        /// <para><b>顺序</b>（先校验、再产资源、再热更、最后构建+转换）：</para>
        /// 1/5 启动链接线校验（只读，失败即停）→ 2/5 客户端自检 → 3/5 配置表基线
        /// → 4/5 YooAsset 资源包 → 5/5 WebGL 构建 + 微信小游戏转换。
        ///
        /// <para>热更 DLL 的重新编译与同步由 <see cref="WXConvertCore.DoExport"/> 之前的
        /// `CompileDllCommand.CompileDll(WebGL)` 完成（见 <see cref="RunSyncHotFixDll"/> 的说明）。</para>
        ///
        /// <para><b>注意</b>：本方法整体是同步的。若其中某一步触发 Unity 域重载（domain reload），
        /// 方法会被中断，后续步骤不会执行 —— 日志中会停在最后一个已打印的 `[管线] n/5` 标记上，
        /// 可据此定位。这是刻意保留的可观测性，不要用 try/catch 吞掉。</para>
        /// </summary>
        /// <summary>
        /// **阶段 A / 共两阶段。**
        ///
        /// <para><b>为什么必须分两阶段</b>：`CompileDllCommand.CompileDll(WebGL)` 只把新的程序集**写到磁盘**，
        /// 当前编辑器进程里加载的仍是旧程序集。`BuildPipeline.BuildAssetBundles` 依据**已加载**的程序集
        /// 校验序列化布局，于是报
        /// `Error building player because script class layout is incompatible between the editor and the player.`
        /// 修法是让编辑器重载程序集，而这必须跨进程：`AssetDatabase.Refresh()` 触发的域重载会
        /// **中断当前正在执行的 executeMethod**（实测 2026-09-22：
        /// `Logs/b1-full-pipeline-20260922-133928.log`，自检正是在此处被 `EditorApplication.Exit` 截断）。
        ///
        /// <para>本阶段只做"改程序集"的事：编译 WebGL 播放器/热更程序集 → 拷贝 `HotFix.dll.bytes`
        /// → `AssetDatabase.Refresh()` 让下个进程以最新程序集启动。**不建资源包、不构建玩家。**</para>
        /// </summary>
        public static void PreparePlayerAssemblies()
        {
            Debug.Log("[阶段A] 编译 WebGL 程序集 + 同步热更 DLL，然后刷新资产库供下一阶段使用");

            int code = RunSyncHotFixDll();
            if (code != 0)
            {
                Debug.LogError("[阶段A] 失败：热更 DLL 同步未成功。");
                EditorApplication.Exit(1);
                return;
            }

            // 让 AssetDatabase 重导入 HotFix.dll.bytes 并重载程序集。
            // 这里**允许**域重载打断本方法——阶段 A 的产物已经落盘，阶段 B 会重新开始。
            Debug.Log("[阶段A] 触发 AssetDatabase.Refresh()（本进程可能被域重载中断，属预期）");
            AssetDatabase.Refresh();

            Debug.Log("[阶段A] 完成。");
            EditorApplication.Exit(0);
        }

        /// <summary>
        /// **阶段 B / 共两阶段。** 在程序集已刷新（新进程）之后：校验 → 自检 → 配置表基线
        /// → YooAsset 资源包 → WebGL 构建 + 微信小游戏转换。
        ///
        /// <para>本阶段**不再触碰程序集**，因此不会触发域重载，同进程内可安全串行。</para>
        /// </summary>
        public static void BuildBundlesAndConvert()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int step = 0;
            const int totalSteps = 4;

            Debug.Log($"[管线B] 开始：阶段B（{totalSteps} 步，目标 WebGL）");

            // ---- 1/4 启动链接线校验（只读，失败即停）----
            step++;
            Debug.Log($"[管线B] {step}/{totalSteps} 启动链接线校验 >>> ({stopwatch.Elapsed.TotalSeconds:N1}s)");
            if (RunVerifyStartupChain() != 0)
            {
                Debug.LogError($"[管线B] 中止于 {step}/{totalSteps}：启动链接线未通过。");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log($"[管线B] {step}/{totalSteps} 启动链接线校验 完成 <<< ({stopwatch.Elapsed.TotalSeconds:N1}s)");

            // ---- 2/4 配置表基线 ----
            step++;
            Debug.Log($"[管线B] {step}/{totalSteps} 配置表基线 >>> ({stopwatch.Elapsed.TotalSeconds:N1}s)");
            RunTableBaseline();
            Debug.Log($"[管线B] {step}/{totalSteps} 配置表基线 完成 <<< ({stopwatch.Elapsed.TotalSeconds:N1}s)");

            // ---- 3/4 YooAsset 资源包 ----
            step++;
            string packageVersion = ReadArg("-packageVersion", MakeDefaultPackageVersion());
            Debug.Log($"[管线B] {step}/{totalSteps} YooAsset 资源包 >>> 版本 {packageVersion} ({stopwatch.Elapsed.TotalSeconds:N1}s)");
            if (RunBuildBundles(packageVersion) != 0)
            {
                Debug.LogError($"[管线B] 中止于 {step}/{totalSteps}：YooAsset 资源包构建失败。");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log($"[管线B] {step}/{totalSteps} YooAsset 资源包 完成 <<< ({stopwatch.Elapsed.TotalSeconds:N1}s)");

            // ---- 4/4 WebGL 构建 + 微信小游戏转换 ----
            step++;
            Debug.Log($"[管线B] {step}/{totalSteps} WebGL 构建 + 微信转换 >>> ({stopwatch.Elapsed.TotalSeconds:N1}s)");
            WXExportError result = WXConvertCore.DoExport(true);
            Debug.Log($"[管线B] {step}/{totalSteps} WXConvertCore.DoExport 返回：{result} ({stopwatch.Elapsed.TotalSeconds:N1}s)");

            if (result == WXExportError.SUCCEED)
            {
                Debug.Log($"[管线B] 全部完成，总耗时 {stopwatch.Elapsed.TotalMinutes:N1} 分钟。");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[管线B] 小游戏转换失败：{result}（{stopwatch.Elapsed.TotalMinutes:N1} 分钟）");
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// **一次 Unity 启动跑完整个移植构建链**。
        ///
        /// <para>⚠️ **不要用本方法**：它把"改程序集"和"建资源包"放在同一进程，必然触发
        /// `script class layout is incompatible`（实测 2026-09-22）。
        /// 保留仅为向后兼容与说明；正确入口是分两阶段的
        /// <see cref="PreparePlayerAssemblies"/>（阶段 A）+ <see cref="BuildBundlesAndConvert"/>（阶段 B），
        /// 由 `Logs/webgl-port/run-pipeline-once.ps1` 依次调用。</para>
        /// </summary>
        [System.Obsolete("在单进程内既改程序集又建资源包会因序列化布局不一致而失败；请用 PreparePlayerAssemblies + BuildBundlesAndConvert 两阶段。")]
        public static void RunFullPortPipeline()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int step = 0;
            const int totalSteps = 6;

            Debug.Log($"[管线] 开始：一次启动串行执行 {totalSteps} 步（目标 WebGL）");

            // ---- 1/6 启动链接线校验（只读，失败即停）----
            step++;
            Debug.Log($"[管线] {step}/{totalSteps} 启动链接线校验 >>> ({stopwatch.Elapsed.TotalSeconds:N1}s)");
            if (RunVerifyStartupChain() != 0)
            {
                Debug.LogError($"[管线] 中止于 {step}/{totalSteps}：启动链接线未通过。");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log($"[管线] {step}/{totalSteps} 启动链接线校验 完成 <<< ({stopwatch.Elapsed.TotalSeconds:N1}s)");

            // ---- 2/6 客户端纯逻辑自检（失败不中止：含与移植无关的既有失败项）----
            step++;
            Debug.Log($"[管线] {step}/{totalSteps} 客户端自检 >>> ({stopwatch.Elapsed.TotalSeconds:N1}s)");
            if (RunSelfTest() != 0)
            {
                Debug.LogWarning($"[管线] {step}/{totalSteps} 客户端自检存在失败项，但**不中止管线**：" +
                                 "自检覆盖 UI 适配锚点等属于其它工作线的检查，不应阻断移植构建。" +
                                 "如需单独门禁，请直接调用 ClientSelfTest.RunAll（它会以退出码结束进程）。");
            }
            Debug.Log($"[管线] {step}/{totalSteps} 客户端自检 结束 <<< ({stopwatch.Elapsed.TotalSeconds:N1}s)");

            // ---- 3/6 配置表基线 ----
            step++;
            Debug.Log($"[管线] {step}/{totalSteps} 配置表基线 >>> ({stopwatch.Elapsed.TotalSeconds:N1}s)");
            RunTableBaseline();
            Debug.Log($"[管线] {step}/{totalSteps} 配置表基线 完成 <<< ({stopwatch.Elapsed.TotalSeconds:N1}s)");

            // ---- 4/6 WebGL 程序集编译 + 热更 DLL 同步 ----
            // 必须在资源包构建之前：① bundle 会收集 HotFix.dll.bytes；② 播放器侧程序集若落后
            // 于编辑器侧，BuildAssetBundles 会报 script class layout incompatible（实测）。
            step++;
            Debug.Log($"[管线] {step}/{totalSteps} 编译 WebGL 程序集 + 同步热更 DLL >>> ({stopwatch.Elapsed.TotalSeconds:N1}s)");
            if (RunSyncHotFixDll() != 0)
            {
                Debug.LogError($"[管线] 中止于 {step}/{totalSteps}：热更 DLL 同步失败。");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log($"[管线] {step}/{totalSteps} 编译与同步 完成 <<< ({stopwatch.Elapsed.TotalSeconds:N1}s)");

            // ---- 5/6 YooAsset 资源包 ----
            step++;
            string packageVersion = ReadArg("-packageVersion", MakeDefaultPackageVersion());
            Debug.Log($"[管线] {step}/{totalSteps} YooAsset 资源包 >>> 版本 {packageVersion} ({stopwatch.Elapsed.TotalSeconds:N1}s)");
            if (RunBuildBundles(packageVersion) != 0)
            {
                Debug.LogError($"[管线] 中止于 {step}/{totalSteps}：YooAsset 资源包构建失败。");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log($"[管线] {step}/{totalSteps} YooAsset 资源包 完成 <<< ({stopwatch.Elapsed.TotalSeconds:N1}s)");

            // ---- 6/6 WebGL 构建 + 微信小游戏转换 ----
            step++;
            Debug.Log($"[管线] {step}/{totalSteps} WebGL 构建 + 微信转换 >>> ({stopwatch.Elapsed.TotalSeconds:N1}s)");

            WXExportError result = WXConvertCore.DoExport(true);
            Debug.Log($"[管线] {step}/{totalSteps} WXConvertCore.DoExport 返回：{result} ({stopwatch.Elapsed.TotalSeconds:N1}s)");

            if (result == WXExportError.SUCCEED)
            {
                Debug.Log($"[管线] 全部 {totalSteps} 步完成，总耗时 {stopwatch.Elapsed.TotalMinutes:N1} 分钟。");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[管线] 小游戏转换失败：{result}（{stopwatch.Elapsed.TotalMinutes:N1} 分钟）");
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// 调用客户端既有自检的核心检查并判定结果。
        ///
        /// <para><b>为什么用 <c>RunAllCore</c> 而不是 <c>RunAll</c></b>：
        /// `RunAll` 在批处理模式下会调用 `EditorApplication.Exit(...)` **直接结束进程**
        /// （那是对"单独 CLI 调用"刻意设计的行为），在多步管线中途调用会把 Unity 杀掉，
        /// 后续步骤全部丢失。`RunAllCore` 是同一套检查但不结束进程的入口。</para>
        ///
        /// <para><b>失败是否中止管线</b>：自检覆盖面包含**与本轮移植无关**的既有失败项
        /// （例如 UI 适配锚点，属另一条工作线）。因此这里把结论明确打印，并交给调用方决定：
        /// 见 <see cref="RunFullPortPipeline"/> 中对返回值的使用。</para>
        /// </summary>
        public static int RunSelfTest()
        {
            int failures = Pinball.Client.Editor.ClientSelfTest.RunAllCore();

            if (failures > 0)
            {
                Debug.LogWarning($"[管线] 客户端自检有 {failures} 项失败（详见上方 [自检] 失败项）。" +
                                 "注意：其中可能包含与本轮移植无关的既有失败项。");
                return 1;
            }

            Debug.Log("[管线] 客户端自检全部通过。");
            return 0;
        }

        /// <summary>调用客户端既有配置表基线打印（只读基线，不做判定）。</summary>
        public static void RunTableBaseline()
        {
            Pinball.Client.Editor.ClientTableProbe.LogFullBaseline();
        }

        /// <summary>
        /// 自检由 <c>ClientSelfTest.RunAllCore</c> 直接返回失败项数量，不再需要日志计数。
        /// 保留此说明以免后人重复引入计数方案。
        /// </summary>
        private static string ReadArg(string name, string fallback)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return fallback;
        }
    }
}
