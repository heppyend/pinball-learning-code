# Main 模块说明

## 职责

项目启动与资源初始化模块，也是 WebGL / 微信小游戏的启动链宿主：
`Boot.unity` → `Init.cs`（YooAsset 初始化 + HybridCLR 元数据/热更程序集加载）→ 加载**首个游戏场景**。

## 主要内容

- `Boot.unity`：应用启动场景，**构建清单第 0 位**。序列化字段：
  `PlayMode`、`DefaultHostServer` / `FallbackHostServer`（当前指向 CDN 根 `…/WebGL/pinball-client`）、`ClientStartScene`。
- `Init.cs`：启动编排入口。关键成员：
  - `ClientStartScene`：首个场景名（留空 = 原行为 `LoginScene`；当前 = `ClientShell`）；
  - `ResolveFirstSceneName()`：显式配置优先，否则回落 `LoginScene`；
  - `LoadFirstSceneAsync(sceneName)`：**内置场景走 `SceneManager.LoadSceneAsync`**（起不来则回落 `YooAssets.LoadSceneAsync`）。
    背景：`ClientShell` 由 Build Settings 打进播放器，不在 YooAsset 收集器范围内，按地址加载必然失败
    （`Failed to mapping location to asset path : ClientShell`）。详见 `HOME_PAGE_SCENE_FIX_OPTIONS.md` §九。
  - `RunPostSceneLoadCompatibility()`：WebGL 相机兼容处理 + 首屏诊断入口，**由跨场景宿主调用**（见下）。
  - `#if UNITY_WEBGL && !UNITY_EDITOR` 段：相机 HDR/后处理关闭（`WebGLRenderCompatibleCameraIds` 去重）、
    `LogWebGLFirstScreenSnapshot()` 首屏诊断（1s / 10s 各一次）。
- `WebGLPostSceneLoadRunner.cs`（2026-09-22 新增）：**跨场景宿主**。
  `Init` 在触发加载**之前**创建它并 `DontDestroyOnLoad`；它订阅 `SceneManager.sceneLoaded`，
  在场景切换完成后调用 `Init.RunPostSceneLoadCompatibility()` 一次，然后自毁。
  存在的理由：`LoadSceneMode.Single` 卸载 `Boot` 场景时 `Init` 及其协程一并销毁，
  写在协程 `yield return` **之后**的代码永远不会执行（容器日志实测：该分支输出 0 命中）。
  **本类不得声明任何 `[SerializeField]` 字段**（字段布局不一致会让 Unity 拒绝构建）。
- `Yooasset`：项目使用的 YooAsset 运行时与编辑器支持代码。
- `ResHotUpdate` / `Helper.cs` 等：热更新与工具支持。

## 依赖与边界

- 上游：`ProjectSettings/EditorBuildSettings.asset` 的构建清单顺序（当前 `Boot(0) → ClientShell(1)`）。
- 下游：`ClientShell` 场景内自有组合根 `ClientBootstrap`；TCP 探针
  （`Assets/Client/Runtime/ClientTcpConnectionProbe.cs`）挂在 `Boot` 的 `Init` 对象所在场景，
  依赖 `Init` 同场景才能反射到 HotFix 程序集 —— **不得**把探针移出 `Boot`，也**不要**把 `Init` 改成
  `DontDestroyOnLoad`（`StartPersistentProbe` 明确依赖"Boot 场景会被卸载"这一前提）。
- 平台：WebGL / 微信小游戏专用分支不得影响 Editor 与原街机流程。

## 约束与验证

- 不改变默认包名、资源地址、运行模式或 DLL 名称，除非同步完成 YooAsset/HybridCLR 构建验证。
- 修改启动逻辑至少验证 Editor Simulate Mode；涉及非编辑器分支时还应验证目标平台构建与启动。
- **授权边界**：`Init.cs` 与 `Boot.unity` 属负责人**逐次授权**文件，改动前必须逐一确认（见 `AGENTS.md` §3 与本模块会话记录）。
- 一次性验证入口：
  - 编译三目标：`pwsh -File Logs\verify-compile.ps1 -Profile All`（可加 `-DiscoverRoot Assets\Main` 覆盖新增文件）；
  - 一键出包：`pwsh -File Logs\webgl-port\run-pipeline-2phase.ps1 -PackageVersion <版本>`（**先关 Unity 编辑器**）；
  - 构建日志必查：`[启动链校验] 构建清单场景数 = 2` + `[Builder] Scenes [1]: …/ClientShell.unity, [x]`；
  - 运行判据：`首个场景已加载（sceneLoaded 事件）：ClientShell` + `WebGL 渲染兼容性已注册` + `[WebGL 首屏诊断 1s]`。

## 待确认

- `LoginScene` 等热更场景将来是否需要与 `ClientShell` 共存切换（影响 `LoadFirstSceneAsync` 的分支策略）。
- `WebGLFirstScreenDiagnosticsHost` 属**可回收诊断代码**：真机与容器均稳定后可整体移除。
