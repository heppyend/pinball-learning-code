# 版本记录

本文件记录已完成、可追溯的版本变更；计划、猜测和未验证的调整不得写为发布版本。每次合并可交付改动或制作构建包时追加记录。

## 记录格式

```text
## [版本号或日期] - 状态（开发中 / 已验证 / 已发布）

### 变更
- 用户可见或工程结构变更。

### 验证
- 已执行的测试、构建或人工路径。

### 兼容性与回滚
- 配置、资源包、存档或接口影响；必要时写明回滚方式。
```

## [2026-08-28-720] - 诊断构建待验证

### 变更
- 使用当前 WebGL 黑屏诊断日志版本，包含 `StartGame`、`GameController` 的可回收启动节点日志和 Canvas 加载失败保护。
- 该版本替代 `578` 作为本轮 HotFix、YooAsset、OSS/CDN 和小游戏转换的统一版本号。

### 验证
- 代码静态检查已通过；待负责人重新生成 HotFix、构建 DefaultPackage、上传并在开发者工具验证。

## 历史

## [2026-08-28] - 开发中

### 变更
- 将当前产品实施范围重置为小程序非战斗客户端；新建 `Assets/Client` 模块和不进入既有启动链的 `ClientShell` 独立场景。
- 重新生成开发流程，明确 C0—C6 阶段、模拟数据优先和服务端/战斗/总架构的隔离边界。
- 记录可再生产物清理候选 RISK-014；未删除任何文件，既有街机、热更新、WebGL 与微信 SDK 差异全部保留。
- 新增 `ClientBootstrap`、账户/货币/英雄/背包领域模型及 `IClientDataService` 本地模拟实现；未来服务端只能通过替换该接口实现接入。
- 在 `ClientShell` 新增主页面运行时 UI、入口热点及本地资料绑定；视觉采用负责人提供的 `UI/主页/参考.png`，中央立绘尚未获得独立切图，正式分层资源待补。
- 新增个人中心运行时页面：从主页面进入并返回，展示本地资料；改名弹窗只更新 `IClientDataService` 的本地实现，未来服务端适配可保持 UI 不变。
- 根据负责人对场景 GUI 工作流的确认，改为由 `Client/Build ClientShell Visual UI` 编辑器菜单在 `ClientShell` 中生成可直接调整的 Canvas、主页、个人中心、按钮和改名弹窗；运行时组件改为只做数据绑定与导航，不再临时创建 Canvas。

### 验证
- `git diff --check` 已通过。Unity 2022.3.57f1c2 命令行编译因已有 Editor 正打开此项目而被拒绝，尚未进入编译；日志为 `Logs/codex-client-c0-compile.log`。Unity Editor 场景打开验证待负责人执行。

### 兼容性与回滚
- 未修改 Build Settings、Boot、YooAsset、HybridCLR、XLua、URP、微信 SDK 或服务端边界。删除新增 `Assets/Client`（含 `.meta`）并回退文档即可恢复此前结构，执行前仍须负责人确认。

## [2026-08-28] - 开发中

### 变更
- WebGL/微信小游戏启动资源加载成功后改为进入 `MainScene`，跳过 `LoginScene`；在大厅场景加入既有 `StartGame` 热更新入口，继续复用 Canvas、模块注册和本地大厅演示分支。
- WebGL 大厅模式、副本和关卡卡片增加触摸/鼠标点击绑定；非 WebGL 输入和登录链路保持不变。
- 记录 WebGL/HybridCLR/YooAsset 生成目录清理候选，暂不删除未完成引用与发布依赖审计的资源。

### 验证
- Unity `2022.3.57f1c2` 命令行脚本编译成功，退出码 0；日志为 `Logs/codex-webgl-migration-compile.log`。
- `git diff --check` 仅报告 Unity YAML 行尾空白提示；开发者工具、重新转换、CDN 和真机验证尚未执行。
- 负责人已在 Unity YooAsset AssetBundle Builder 中成功构建 WebGL DefaultPackage `2026-08-28-577`；参数为 `BuiltinBuildPipeline`、`ForceRebuild`、`ClearAndCopyByTags` 且复制参数为空。

### 兼容性与回滚
- 首场景切换仅受 `UNITY_WEBGL && !UNITY_EDITOR` 条件控制；删除 `MainScene` 中的 `StartGame` 对象并恢复 `Init` 场景选择即可回滚。
- 未删除资源、未修改 Packages、YooAsset、HybridCLR、XLua 或 URP 版本。

## [2026-08-27] - 开发中

### 变更
- 增加仅在非 Editor WebGL Player 生效的本地大厅演示分支：跳过既有登录、下载地址和英雄数据请求，以本地表格配置直接打开 `LobbyUI`。
- 为首屏状态增加可回收的 WebGL 诊断宿主；诊断输出相机、Canvas 和场景快照，不改变非 WebGL 行为。
- 新增 `Build/WebGL/Compile And Sync HotFix DLL` 编辑器命令，编译并同步 WebGL `HotFix.dll` 到既有 YooAsset 构建输入；没有上传或改写远端资源。
- 已本地构建 WebGL DefaultPackage `2026-08-27-1218`；本地 `HybridCLRData/HotUpdateDlls/WebGL/HotFix.dll` 与 `Assets/HotUpdateResources/Dll/WebGL/HotFix.dll.bytes` SHA-256 一致。

### 验证
- 已完成文本差异检查；新编辑器命令的 `HybridCLR.Editor.SettingsUtil` 类型已从当前 `HybridCLR.Editor.dll` 反射确认存在且为 public。
- 受原项目正由 Unity Editor 打开影响，命令行 Unity 2022.3.57f1c2 WebGL 编译被拒绝（`HandleProjectAlreadyOpenInAnotherInstance`）；本次修改尚无命令行编译成功记录。
- YooAsset 资源包已由负责人在 Unity GUI 构建；尚未确认 CDN 发布、Boot/CDN 地址更新、重新转换或开发者工具大厅展示。

### 兼容性与回滚
- 大厅分支和诊断均受 `UNITY_WEBGL && !UNITY_EDITOR` 条件限制；移除对应条件分支/编辑器命令即可回滚，不涉及 Packages、URP、YooAsset、HybridCLR 或 XLua 的版本变更。

## [2026-08-27] - 开发中

### 变更
- 为微信小游戏 WebGL 运行时增加 URP 相机兼容处理：注册 URP 的 `RenderPipelineManager.beginCameraRendering` 回调，关闭当前及后续异步实例化 Canvas 相机的 HDR 和后处理；Unity 编辑器及非 WebGL 平台不改变原行为。
- 将启动阶段 WebGL CDN 根地址输出由 `Debug.LogError` 调整为普通日志，避免伪错误干扰开发者工具排查。

### 验证
- 已在微信开发者工具确认：YooAsset 初始化、版本更新、6 个 AOT 元数据加载和 `AppRoot`/Canvas 创建均已执行；远端首包与全部 14 个 Bundle 均可访问且文件长度匹配构建报告。
- 已直接在原项目执行 Unity `2022.3.57f1c2` WebGL 批处理编译三次，均返回码 0；最新日志为 `Logs/Codex-WebGL-UrpCameraCallback-Compile.log`。

### 兼容性与回滚
- 仅绕过微信 WebGL 不支持的 URP 后处理路径，不调整 URP 包、渲染资源或桌面画面。删除该条件编译分支即可回滚；开发者工具与真机画面回归仍待人工验证。

## [2026-08-27] - 开发中

### 变更
- 将 DefaultPackage 的 Dll Collector 收集范围限定为 `Assets/HotUpdateResources/Dll/WebGL`，保留 Windows DLL 与既有文件名地址规则。
- 已使用 WebGL 的 BuiltinBuildPipeline 完成 DefaultPackage 资源构建，生成包版本 `2026-08-27-686`。
- WebGL 运行时跳过 `Unity.Timeline.dll` 的 HybridCLR AOT 元数据加载；非 WebGL 平台保持原加载行为。

### 验证
- 构建产物含版本文件、清单及 14 个资源包；构建报告目标为 WebGL，且 `Demigiant.dll`、`HotFix.dll` 等 DLL 均映射到 WebGL 目录，未再出现同地址冲突。
- OSS 的 `StreamingAssets/yoo/DefaultPackage/PackageManifest_DefaultPackage.version` 已可由外网读取，返回版本 `2026-08-27-686`。
- 已直接在原项目执行 Unity `2022.3.57f1c2` WebGL 批处理脚本编译，返回码 0；日志为 `Logs/Codex-WebGL-Timeline-Compile.log`。

### 兼容性与回滚
- 已发布 WebGL 资源并由外网读取版本文件验证；尚未改写远端地址或完成小游戏主工程启动验证。Windows 资源目录未删除。将 Collector 路径恢复为原值会重新引入跨平台同名 DLL 地址冲突。

## [2026-08-26] - 已验证

### 变更
- 修复 WebGL 大厅端条件编译与 XLua WebGL 原生源码缺失问题，完成微信小游戏导出。

### 验证
- 负责人已在 Unity `2022.3.57f1c2` 的微信小游戏导出流程中人工确认导出成功；`Context.Game` 编译错误和 `WebGLPlugins/lapi.c` 缺失错误均未再出现。

### 兼容性与回滚
- 保留 WebGL 的 `HALL` 定义及恢复的官方 XLua `WebGLPlugins`。该验证仅覆盖导出成功，不覆盖微信开发者工具加载、运行时 Lua、网络、账号、隐私合规或真机验证。

## [2026-08-26] - 开发中

### 变更
- 经负责人确认，从 Tencent xLua 官方源恢复工程根目录缺失的 `WebGLPlugins` WebGL 原生源码（65 个文件）；现有 WebGL 入口与恢复来源的 SHA-256 一致。

### 验证
- 已完成逐文件 SHA-256 一致性检查；后续已由负责人完成 Unity Editor 导出验证，结果见本文件最新条目。

### 兼容性与回滚
- 未升级、替换或禁用 XLua；移除 `WebGLPlugins` 目录即可回到恢复前状态，但会重新出现 BUG-004。

## [2026-08-26] - 已验证

### 变更
- 经负责人授权导入 `minigame.202608251231.unitypackage`，新增 WX-WASM-SDK-V2 和微信 WebGL 模板。

### 验证
- Unity `2022.3.57f1c2` 批处理导入后完成脚本编译并以返回码 0 退出。

### 兼容性与回滚
- 导入日志发现 LitJson <服务器地址> 与项目 <服务器地址> 重复；未作删除或替换，风险由 `RISK-007` 跟踪。

## [2026-08-26] - 已验证

### 变更
- 建立独立的微信小游戏桥接契约、默认不可用回退和注册入口；未接入 SDK、账号、网络、支付或服务端。

### 验证
- 已通过纯 C# 静态语法编译。
- 已直接对原项目执行 Unity `2022.3.57f1c2` 批处理编译；日志显示 `CompileScripts: 2047.954ms`，未报告 C# 编译错误。

### 兼容性与回滚
- 不改变既有 Android/iOS `UnityBridgeManager`、YooAsset、HybridCLR、XLua、URP 或包清单；删除 `Assets/Scripts/Platform/WeChatMiniProgram` 即可回滚本模块。

## [2026-08-26] - 已验证

### 变更
- 建立项目概况、版本记录、缺陷/风险记录和模块目录说明的文档治理基线。
- 完善开发流程：增加需求执行七步闭环、实现前检查清单、人工 GUI 验证模板、资源清理/目录调整流程，以及版本、缺陷与模块文档的更新规则。
- 新增面向面试展示的 `Project Introduction/Project Introduction.md`，说明项目现状、技术栈、架构、进度、风险与演示方式。
- 按负责人确认移除不用于小程序的 SuperTiled2Unity 历史怪物生成脚本及其 HybridCLR 链接条目，消除该缺失依赖引发的 CS0246。

### 验证
- 已核对 Unity 构建设置中的启动场景与 `Assets/Main/Init.cs` 的资源初始化链路。
- 已检查流程与长期约束中的“代理不操作 GUI、所有工作遵循根目录治理文档”规则一致。
- 已依据 `ProjectVersion.txt`、`Packages/manifest.json`、启动代码、程序集定义和模块目录核对项目介绍中的技术事实。
- 已静态检查项目源码与 HybridCLR 链接配置，不再包含 SuperTiled2Unity 编译期引用；FishScene 的历史组件清理待人工验证。
- 负责人已在 Unity 中人工确认：Console 红色 Error 数量为 0、已退出 Safe Mode，工程可正常打开并完成编译。

### 兼容性与回滚
- 文档治理改动可通过删除对应 Markdown 文件并同步移除约束条款回滚。
- SuperTiled2Unity 清理若需回滚，须恢复 `BNMonsterSpawnManager` 及其 `.meta`、HybridCLR 链接条目，并重新引入与当前 Unity 版本兼容的依赖；不应仅恢复脚本。
- 本次人工验证未加载历史 `FishScene`，其可能遗留的 Missing Script 风险继续由 `BUG_TRACKER.md` 的 RISK-005 跟踪。
## 2026-08-28（进行中：WebGL 资源收集修复）

- 修正 `Resources_HotUpdate` YooAsset UI 收集器：改用 `AddressDisable`，避免同一目录内 JSON/PNG 等同名不同扩展名资源生成重复地址。
- 运行时 `AssetLoader` 按 `Assets/Resources_HotUpdate/...` 完整路径加载，地址禁用不改变现有路径加载契约。
- 负责人已使用 Unity `2022.3.57f1c2` 成功重新构建 DefaultPackage `2026-08-28-578`；Console 显示 `Resource pipeline build success`。截图中的 `.tmx` “Cannot pack default asset” 为警告，未阻断构建。
