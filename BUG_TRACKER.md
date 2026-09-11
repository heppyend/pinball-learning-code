# 缺陷、风险与待核验清理项

本文件用于记录已复现缺陷、潜在缺陷、技术债与尚未获得删除授权的清理候选。每一项必须描述证据、影响与验证/关闭条件；未复现的问题应标为“风险”而非“缺陷”。

## 条目格式

```text
### BUG/RISK-编号：标题
- 状态：开放 / 调查中 / 已修复待验证 / 已关闭
- 类型：缺陷 / 风险 / 清理候选
- 影响范围：
- 证据与复现步骤：
- 暂定处理或规避：
- 关闭条件：
```

## 开放条目

### RISK-014：客户端启动前的可再生产物清理待负责人执行
- 状态：待确认
- 类型：清理候选
- 影响范围：本地编辑器与 IDE 缓存；不包括已跟踪或未跟踪的 Unity 资源、WebGL 包、HybridCLR、YooAsset、微信 SDK 与场景文件。
- 证据与复现步骤：`.gitignore` 已覆盖 `Library/`、`Logs/`、`Temp/`、`.vs/`、`obj/`、`*.csproj` 和 `*.sln`；这些文件由 Unity/IDE 生成，工作区当前同时存在不可安全判断的业务与构建差异。
- 暂定处理或规避：本轮不删除。负责人关闭 Unity/IDE 后，可按本机磁盘压力选择清理上述缓存，再重新打开项目让它们再生；任何不在本条列表内的路径均须另建候选并完成引用核验。
- 关闭条件：负责人确认清理已完成，项目重新打开且 Unity 编译通过；或明确保留这些本地缓存。

### RISK-011：WebGL 大厅直进与资源清理仍待人工验收
- 状态：待人工验证
- 类型：风险 / 清理候选
- 影响范围：`MainScene` 首次加载、小游戏大厅触摸交互，以及未纳入版本控制的构建/热更新产物。
- 证据与复现步骤：`Init` 在 WebGL 运行时已改为加载 `MainScene`；`MainScene` 新增与登录场景相同的 `StartGame` 入口，由热更新程序集实例化 Canvas 并调用本地大厅预览。`LobbyUI` 仅在 WebGL 运行时为模式、副本和关卡卡片绑定触摸点击。当前尚未在开发者工具或真机回传新包日志。工作区存在 `Bundles/WebGL`、`HybridCLRData/*/WebGL`、`TextToolDatas`、`WebGLPlugins` 等未追踪目录，来源和发布依赖尚未逐项闭环。
- 暂定处理或规避：先发布并验证新的 WebGL 资源包与首屏大厅，再按路径、生成命令、消费者和回滚方式审计清理候选；在证据不足前不删除任何资源或生成目录。
- 关闭条件：开发者工具和真机确认跳过登录、进入大厅并可点击切换；每个清理候选均完成引用/生成物扫描并经负责人确认后才可删除。

### BUG-010：微信小游戏首包版本文件上传层级不匹配
- 状态：已复现，修复待重新转换验证
- 类型：缺陷
- 影响范围：YooAsset WebPlayMode 初始化；资源清单初始化前即停止，尚未进入大厅。
- 证据与复现步骤：开发者工具 720 日志请求 `.../WebGL/2026-08-28-720/2026-08-28-720/StreamingAssets/yoo/DefaultPackage/PackageManifest_DefaultPackage.version` 并返回 `404`；OSS 单层路径已通过 HTTP 200 验证，说明是 Boot HostServer 重复版本目录。
- 暂定处理或规避：OSS 保持单层 `WebGL/2026-08-28-720/StreamingAssets/yoo/DefaultPackage/`；已将 Boot 的 Default/Fallback HostServer 改为单层 `.../WebGL/2026-08-28-720`，保存后重新转换，不修改 SDK 的 `jsbridge` 调用。
- 关闭条件：开发者工具不再请求该路径 404，YooAsset 完成版本/清单初始化，并继续出现 `[WebGL 大厅演示]` 日志。

### BUG-011：本地表配置缺失导致 WebGL 大厅演示初始化空引用
- 状态：已修复待验证
- 类型：缺陷
- 影响范围：`GameController.StartWebGLLobbyPreview` 的本地表初始化；资源包初始化成功后大厅仍未打开。
- 证据与复现步骤：2026-08-28 开发者工具日志显示无 `404`，但 `TableLoadHelper._loadJsonImpl` 在 `Resources.Load<TextAsset>("Table/" + tableName)` 返回空后访问 `.text`，堆栈为 `LoadFromLocalPackage -> GameController.StartWebGLLobbyPreview`。
- 暂定处理或规避：缺少本地表文件时记录明确警告，并通过对应 `LoadData` 初始化空集合后继续本地演示；同时确认 `LobbyUI` 的 `ResManager` 路径要求，已将 `Assets/Resources_HotUpdate` 纳入 YooAsset UI 收集入口。不伪造业务数值、不触发登录或服务端请求。完整关卡数据仍需后续接入真实配置包。
- 关闭条件：重新编译并同步 WebGL `HotFix.dll`、重建/发布资源包后，开发者工具出现大厅演示标记并显示大厅；若关卡为空，另行记录为数据准备项。

### BUG-012：Resources_HotUpdate 整目录收集产生重复资源地址
- 状态：已修复待线上验证
- 类型：缺陷
- 影响范围：YooAsset BuiltinBuildPipeline 构建阶段。
- 证据与复现步骤：为收集 `Resources_HotUpdate` 使用 `AddressByFileName` 时，构建报 `The address is existed`，示例冲突地址为 `Attack`；同一目录树下存在多个同名资源。
- 暂定处理或规避：确认 `AddressByFolderAndFileName` 仍会因同一目录下 JSON/PNG 等同名不同扩展名资源产生冲突（如 `12001_玉兔插拆`）。为避免整目录收集造成约 652 MB 的过量包，UI 收集器现改为仅收集 `Assets/Resources_HotUpdate/BNRes/UI/LobbyUI.prefab`，使用 `AddressByFileName`；YooAsset 会收集其依赖，运行时仍按完整路径定位资源，不删除资源。
- 关闭条件：以当前版本 `2026-08-28-720` 发布后，LobbyUI 及其依赖可由 `ResManager` 加载；当前已从 1209 个 bundle/约 652 MB 降至 54 个文件/约 55.54 MB，仍待线上验证。

### BUG-013：小游戏导出工程启动页未在 app.json 定义
- 状态：调查中
- 类型：缺陷
- 影响范围：微信开发者工具加载导出工程的启动阶段；当前日志未进入 Unity/Wasm 资源初始化，因此表现为黑屏。
- 证据与复现步骤：旧日志在 11:04:26 明确报告启动页未定义；最新 11:21:09 日志已不再出现该错误，说明已越过启动页检查，但只到 `game starting`、`mgp inited`、公共目录存储 patch 和性能检测，未出现 WebGL2 上下文、Wasm、`资源包初始化成功` 或 `[WebGL 大厅演示]`。
- 暂定处理或规避：保留干净导出与开发者工具清缓存操作；下一次采集须等待至少 30 秒并导出完整 Console，重点检查是否有后续网络/patch/Wasm 错误。不得直接手改生成工程后作为长期修复，也不修改 `WAGame.js`。
- 关闭条件：新导出工程不再报告 app.json 启动页错误，并出现 WebGL 上下文、Unity 资源包初始化和大厅演示日志；随后继续核对大厅显示。

### BUG-009：微信小游戏 WebGL 首屏黑屏及 URP Shader 不支持日志的关联待核验
- 状态：调查中
- 类型：缺陷 / 风险
- 影响范围：微信开发者工具中的 LoginScene/Canvas 首帧渲染；Unity 编辑器 Play 模式不受本缺陷影响。
- 证据与复现步骤：2026-08-27 主工程的 YooAsset 初始化、版本 `2026-08-27-686` 更新、6 个 AOT 元数据加载及 `AppRoot -> Canvas(Clone)(Clone)` 均已输出；同时观察到 `Hidden/Universal/HDRDebugView` 等 URP Shader 不支持日志和黑屏/不完整首图。日志仅表明二者同时发生，尚不能证明 Shader 或后处理是黑屏根因。
- 暂定处理或规避：现有 WebGL 相机 HDR/后处理关闭代码保留为待验证的诊断性规避；不升级或调整 URP 包、渲染资源和桌面配置。下一轮须回传新版资源包从启动到大厅停住后的完整 Console，核对 WebGL2、`[WebGL 大厅演示]`、首屏诊断及全部 Error/Exception。
- 关闭条件：取得可将首屏表现与具体日志或可回收检测关联的证据，并完成开发者工具和一次真机验证；否则不得将任一推断标记为根因或修复。

### RISK-010：WebGL 直进大厅代码尚未被小游戏实际加载
- 状态：待人工验证
- 类型：风险
- 影响范围：微信小游戏主工程的启动路径、LobbyUI 展示和交互。
- 证据与复现步骤：旧开发者工具日志不含 `[WebGL 大厅演示]`，且 HotFix 业务栈经 HybridCLR 解释器执行，证明当时仍载入旧热更新程序集。当前本地 WebGL DefaultPackage `2026-08-27-1218` 已构建，且源 WebGL `HotFix.dll` 与 `Assets/HotUpdateResources/Dll/WebGL/HotFix.dll.bytes` SHA-256 一致；但 CDN 发布、Boot/CDN 地址更新与重新转换均未获运行日志确认。
- 暂定处理或规避：负责人上传本地版本目录全部文件至新的 OSS 内层版本目录，更新 Boot 的 Default/Fallback Host 与微信转换面板 CDN 到同一 URL，重新转换后在开发者工具清空 Console 运行。直进大厅成功的必要标记为 `[WebGL 大厅演示] 已使用本地配置直接打开大厅，未调用登录或服务端。`。
- 关闭条件：开发者工具加载新包并出现该标记，随后显示可交互的 LobbyUI；再完成真机验证。

### BUG-007：WebPlayMode 首包缺少 YooAsset 内置版本清单
- 状态：已修复待验证
- 类型：缺陷
- 影响范围：微信小游戏 WebGL 初始化阶段；在进入远端 OSS 下载链路前失败。
- 证据与复现步骤：2026-08-27 主工程重新转换后，微信开发者工具请求 `StreamingAssets/yoo/DefaultPackage/PackageManifest_DefaultPackage.version` 返回 404，继而报告 `PackageManifest_DefaultPackage.version下载失败`。YooAsset 的 WebPlayMode 初始化先读取首包版本/清单。修复构建后，`Assets/StreamingAssets/yoo/DefaultPackage` 已包含版本文件、清单 bytes 与 hash；三份文件已直接发布至 OSS 的同一 `StreamingAssets/yoo/DefaultPackage` 路径，外网读取版本文件返回 HTTP 200 和 `2026-08-27-686`。后续出现的 `Unity.Timeline.dll` 错误由 BUG-008 单独跟踪。
- 暂定处理或规避：在 WebGL 的 DefaultPackage/BuiltinBuildPipeline 构建配置中选择 `ClearAndCopyByTags`，保持标签参数为空，以清理并复制版本文件、清单 bytes 与 hash 到首包，而不将任何未标记资源包纳入首包；构建版本保持 `2026-08-27-686` 并选择 `ForceRebuild`。同版本的输出目录已存在时，`IncrementalBuild` 会被 YooAsset 拒绝；`ForceRebuild` 只清理本机的 WebGL DefaultPackage 输出后重建，不删除 OSS 或 Windows 资源。远端 OSS 的 WebGL 资源根目录保持不变。
- 关闭条件：重新导出的小游戏不再请求失败的首包版本文件，能够读取版本 `2026-08-27-686`、完成清单初始化并继续加载远端 DLL/场景资源。

### BUG-008：WebGL 仍尝试加载已裁剪的 Unity.Timeline AOT 元数据
- 状态：已修复待验证
- 类型：缺陷
- 影响范围：微信小游戏主工程在 YooAsset 清单初始化后的 AOT 元数据加载。
- 证据与复现步骤：首包清单 404 修复后，开发者工具不再报告 `PackageManifest_DefaultPackage.version` 下载失败，随即显示 `Failed to mapping location to asset path : Unity.Timeline.dll` 和空引用。`Assets/Main/Init.cs` 的 `LoadMetadataForAOTAssemblies` 曾无条件请求该地址，但 WebGL 的 Dll 目录与 WebGL DefaultPackage 清单均不包含 `Unity.Timeline.dll`。
- 暂定处理或规避：仅以 `#if !UNITY_WEBGL` 保留 Windows 等非 WebGL 平台的 `Unity.Timeline.dll` AOT 元数据加载；不删除 Timeline 包、不更改 HybridCLR 配置，也不删除 Windows DLL。2026-08-27 已完成 Unity 2022.3.57f1c2 WebGL 批处理脚本编译，返回码 0。
- 关闭条件：重新转换并在微信开发者工具启动后，不再出现该 DLL 映射失败及其后续空引用。

### BUG-006：微信开发者工具运行时 jsbridge 未就绪循环报错
- 状态：已确认非阻塞（待主工程验证）
- 类型：缺陷 / 外部工具兼容性
- 影响范围：微信开发者工具 Console 的错误噪音；会干扰运行时错误判读，但当前未证实阻断游戏启动。
- 证据与复现步骤：稳定版开发者工具（`mg 2.02.2608060`、基础库 `3.17.1`）仍持续出现 `invoke getSystemInfo fail: jsbridge not ready`，调用栈落在其 `WAGame.js` 的 `deviceOrientation` 读取处。导出工程未启用 SDK 的 `autoAdaptScreen`，也未注入 `plugins/screen-adapter`。负责人随后以同一已开通“快速配”的 AppID 导出独立空场景；Console 仍出现该日志，但 SDK 已继续加载资源并成功启动空场景。
- 暂定处理或规避：不篡改导出 JS、SDK 或设备信息 API；将该日志视为工具侧非阻塞噪音，优先处理其后的首条 `PLUGIN ERROR`、资源加载或 Unity/Wasm 错误。
- 关闭条件：主工程在开发者工具和一次真机预览中均能越过资源加载并进入 Unity 首场景；若日志仍存在但不影响该路径，则将本条关闭为已知工具兼容性噪音。

### BUG-005：微信小游戏运行时误用 YooAsset Editor Simulate 模式
- 状态：已修复待验证
- 类型：缺陷
- 影响范围：微信开发者工具中的小游戏启动。
- 证据与复现步骤：负责人在开发者工具中运行导出工程时，`YooAsset.EditorSimulateModeHelper.SimulateBuild` 抛出“Only support in unity editor”。`Assets/Main/Init.cs` 的序列化默认模式为 `EditorSimulateMode`，而原先的非 Editor WebGL 切换代码处于注释状态。
- 暂定处理或规避：仅在 `UNITY_WEBGL && !UNITY_EDITOR` 运行时强制选择 `WebPlayMode`；不改变 Unity Editor 中的模拟运行方式，也不修改 YooAsset 本体。
- 关闭条件：重新导出并在微信开发者工具启动，确认不再调用 `EditorSimulateModeHelper.SimulateBuild`。

### BUG-003：WebGL 未定义 HALL 导致 AppRoot 编译失败
- 状态：已关闭
- 类型：缺陷
- 影响范围：WebGL/微信小游戏转换前的 Unity 脚本编译。
- 证据与复现步骤：在 WebGL 未定义 `HALL` 时，`Assets/Scripts/AppRoot.cs` 进入旧的独立小游戏条件分支；该分支引用已移除的 `Context.Game`，报出 CS0117，继而使 Player 构建中断。
- 暂定处理或规避：已为 WebGL 添加与 Android、Standalone、iPhone 一致的 `HALL` 脚本定义，使其编译并使用项目当前大厅启动链路；未恢复或改写废弃的 `Context.Game` API。
- 关闭条件：已由负责人在 Unity Editor 的微信小游戏导出流程中验证：构建成功，未再出现该 CS0117。

### BUG-004：XLua 的 WebGLPlugins 原生源码目录缺失
- 状态：已关闭
- 类型：缺陷
- 影响范围：WebGL/微信小游戏 Player 构建。
- 证据与复现步骤：`Assets/Plugins/WebGL/xlua_webgl.cpp` 引用工程根目录 `WebGLPlugins/lapi.c` 等 XLua WebGL 原生源码；当前该目录不存在，构建在第一个缺失文件处以 fatal error 中断。
- 暂定处理或规避：已从 Tencent xLua 官方提交 `59bf42685dbe36fe0e1678a6ba8597f859ef7ca3` 恢复完整 `WebGLPlugins` 目录（65 个文件）；现有 `xlua_webgl.cpp` 与该来源 SHA-256 一致，且逐文件复核无差异。未禁用插件或混用 `build` 下不同 Lua 版本。
- 关闭条件：已由负责人在 Unity Editor 的微信小游戏导出流程中验证：导出成功，已越过 XLua 原生插件编译阶段。运行时 Lua 初始化仍需后续开发者工具/真机验证。

### RISK-001：资源包与发布目录的保留策略未确认
- 状态：调查中
- 类型：风险 / 清理候选
- 影响范围：`Bundles`、`AssetBundles`、`StreamingAssets`、`StreamingAssetsPublish`、`Assets/HotUpdateResources`。
- 证据与复现步骤：启动入口 `Assets/Main/Init.cs` 依赖 YooAsset 包、热更新 DLL 与 `LoginScene`；上述目录中部分内容已由 Git 追踪，尚未找到权威的生成/发布说明。
- 暂定处理或规避：本轮不删除这些目录或其中资源；先核对 YooAsset 构建配置、资源地址和目标平台发布流程。
- 关闭条件：确认每个目录的生成来源、消费者、保留周期与可重复构建命令，并完成一次验证构建。

### RISK-002：历史场景与第三方示例的引用状态未核验
- 状态：开放
- 类型：风险 / 清理候选
- 影响范围：`Assets/Scenes` 下的测试/效果场景及第三方包自带示例目录。
- 证据与复现步骤：构建设置当前只显式启用 `Assets/Main/Boot.unity`；但资源可能由 YooAsset 地址、代码字符串或编辑器工具间接加载。
- 暂定处理或规避：不按“未加入 Build Settings”判定无用；后续通过资源包清单、代码引用和运行验证逐项核验。
- 关闭条件：完成地址/引用扫描并在目标运行模式下完成启动、登录场景和关键资源加载验证。

### RISK-003：启动代码存在遗留的无效 Helper 实例化
- 状态：开放
- 类型：风险
- 影响范围：`Assets/Main/Init.cs`、`Assets/Main/Helper.cs` 的非编辑器启动分支。
- 证据与复现步骤：`Init.Start` 在非 `EditorSimulateMode` 分支创建 `new Helper()`；其构造函数仅创建未挂载、未保存的 Unity 组件实例，且未参与后续逻辑。
- 暂定处理或规避：本轮不改动启动行为；待确认该代码是否是旧平台兼容占位后，以目标平台构建验证为前提清除或替换。
- 关闭条件：确认无外部副作用，并在目标运行模式完成启动与资源加载回归。

### RISK-004：根目录可再生 IDE 产物尚待在具备删除权限的环境清理
- 状态：开放
- 类型：清理候选
- 影响范围：根目录 `*.csproj`、`UnityProject.sln`、`UnityProject.sln.DotSettings.user`、`obj`、`Logs`。
- 证据与复现步骤：上述内容由 `.gitignore` 明确排除，且属于 Unity/IDE 编译与日志产物；本次执行环境禁止删除命令，未发生删除。
- 暂定处理或规避：在允许删除的本地环境中关闭 Unity/IDE 后删除这些明确路径；重新打开 Unity 或 IDE 会按需生成工程文件。
- 关闭条件：完成删除并重新打开 Unity，确认项目可编译且所需工程文件已再生。

### RISK-005：历史 FishScene 可能保留已移除的 BNMonsterSpawnManager 缺失组件
- 状态：待人工验证
- 类型：风险
- 影响范围：`Assets/HotUpdateResources/Scene/FishScene.unity`。
- 证据与复现步骤：该场景仍序列化了 `BNMonsterSpawnManager` 对象；负责人已确认小程序不使用 Tiled 地图怪物生成，因此对应脚本与 SuperTiled2Unity 的 HybridCLR 链接条目已移除。
- 暂定处理或规避：不加载该历史 FishScene；若后续仍需维护该场景，由负责人在 Unity Editor 中移除该 GameObject 上的 Missing Script 组件，或一并下线该场景资源。
- 关闭条件：负责人确认 FishScene 不进入小程序资源包，或完成场景组件清理并回传 Unity Console 验证结果。

### RISK-006：微信小游戏导出与线上验证前置条件未确认
- 状态：开放
- 类型：风险
- 影响范围：小游戏 SDK 适配器、导出工程、开发者工具和真机测试。
- 证据与复现步骤：已导入 `minigame.202608251231` 的 WX-WASM-SDK-V2；导出使用的 AppID 为 `wx9ba7dea1539e1c4f`，负责人已确认其“快速配”能力开通。小游戏资源 CDN/域名白名单、隐私声明、服务端换票协议及线上发布资料仍未确认。
- 暂定处理或规避：不写入凭据、不接入真实账号/支付/服务端；保持默认桥接返回 `WX_MINIGAME_NOT_CONFIGURED`。
- 关闭条件：负责人确认上述平台资料与 SDK 支持矩阵，并完成受控导出和人工开发者工具/真机验证。

### RISK-007：SDK 与项目存在 LitJson 重复程序集
- 状态：调查中
- 类型：风险
- 影响范围：`Assets/WX-WASM-SDK-V2/Runtime/Plugins/LitJson.dll`（1.0.0.0）和 `Assets/Plugins/LitJson.dll`（0.9.0.0）。
- 证据与复现步骤：SDK 导入编译日志显示 Unity 选择了 SDK 的 1.0.0.0，并忽略项目已有的 0.9.0.0。
- 暂定处理或规避：本轮不删除、替换或禁用任一 DLL；后续导出前核验现有网络/协议代码对旧版 LitJson API 的兼容性。
- 关闭条件：完成项目现有调用的兼容性检查，并在受控导出中确认没有程序集加载或序列化异常。

### RISK-008：小游戏 YooAsset 远端资源地址仍指向 Windows 构建目录
- 状态：已复现，WebGL 资源已发布，待主工程运行验证
- 类型：风险
- 影响范围：小游戏启动后的资源版本、清单和资源包加载。
- 证据与复现步骤：`Assets/Main/Boot.unity` 的 `Init` 组件将默认与备用地址配置为 `https://localpinball-oss.oss-cn-shenzhen.aliyuncs.com/1/HotUpdate/NewAB/StandaloneWindows64`。负责人在开发者工具运行主工程时，YooAsset 下载该目录的 AssetBundle 后报告其构建目标为 `19`，与当前 WebGL/小游戏平台不兼容，资源加载失败。2026-08-27 已以 WebGL 和 `BuiltinBuildPipeline` 成功构建 `DefaultPackage` 版本 `2026-08-27-686`；输出目录包含版本文件、清单和 14 个资源包，报告构建目标为 `20`，DLL 地址均来自 `Assets/HotUpdateResources/Dll/WebGL`。负责人已将文件发布到 OSS 的内层版本目录；外网读取该目录的 `PackageManifest_DefaultPackage.version` 返回 HTTP 200 且内容匹配 `2026-08-27-686`。
- 暂定处理或规避：将 `Boot.unity` 的默认与备用地址改为已验证的 WebGL 内层版本目录，再重新导出并在开发者工具和真机复测；不改写或删除 Windows 发布目录。
- 关闭条件：可访问的 WebGL/小游戏资源目录包含匹配 WebGL 构建目标的 YooAsset 版本、清单与资源包；主工程在开发者工具和真机均成功完成资源初始化并进入 Unity 首场景。

### BUG-014：WebGL 直达大厅仍通过旧 AssetLoader 加载 LobbyUI
- 状态：代码已修复，待重新导出验证
- 类型：缺陷
- 影响范围：微信小游戏进入大厅后的 UI 实例化与首屏显示。
- 证据与复现步骤：开发者工具日志已确认 Canvas、Wasm、YooAsset 首包初始化及 `GameController.StartWebGLLobbyPreview` 均完成；随后在 `UIManager.AsyncLoadData` 调用 `ResManager.Load` 时，`AssetInfo<T>.resourcesLoad()` 抛出 `NullReferenceException`。直达大厅分支未初始化旧版 `AppRoot.Hall.Loader`，而 `LobbyUI` 地址已由 YooAsset 收集器提供。
- 暂定处理或规避：`Assets/Scripts/Frame/Manager/ResManager.cs` 在 WebGL 非编辑器下按收集器地址调用 `YooAssets.LoadAssetSync<T>`；`BNRes/UI/LobbyUI` 映射为 `LobbyUI`，失败时输出明确的地址与错误信息。其他平台保持原有 AssetLoader 路径。
- 关闭条件：重新编译 HotFix、重新构建并发布当前版本后，开发者工具日志不再出现该空引用，且大厅 UI 可见、可点击。

### BUG-015：小游戏 SDK 自动追加 `/Assets`，OSS 资源当前层级不匹配
- 状态：已复现，待补传到正确目录
- 类型：缺陷
- 影响范围：微信小游戏 YooAsset 远程清单初始化。
- 证据与复现步骤：通过微信开发者工具 GUI 重新运行后，Console 报 `PackageManifest_DefaultPackage.version` 404。导出工程 `game.js` 在模块准备后将 `assetPath` 设置为 `${DATA_CDN}/Assets`；当前根目录 `.../WebGL/2026-08-28-720/PackageManifest_DefaultPackage.version` 可访问，但 `.../WebGL/2026-08-28-720/Assets/PackageManifest_DefaultPackage.version` 返回 404。
- 暂定处理或规避：将 DefaultPackage 的运行时文件整体上传到 OSS 的 `WebGL/2026-08-28-720/Assets/`，并保留小游戏导出包内的 `StreamingAssets/yoo/DefaultPackage`；不要修改 `DATA_CDN` 继续叠加版本层级。
- 关闭条件：`.../720/Assets/PackageManifest_DefaultPackage.version`、对应 manifest 和 bundle 均可访问，开发者工具重新运行不再出现清单 404。
