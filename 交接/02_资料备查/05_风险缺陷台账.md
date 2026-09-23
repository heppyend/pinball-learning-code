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
- 证据与复现步骤：已导入 `minigame.202608251231` 的 WX-WASM-SDK-V2；导出使用的 AppID 为 `<微信小游戏AppID>`，负责人已确认其“快速配”能力开通。小游戏资源 CDN/域名白名单、隐私声明、服务端换票协议及线上发布资料仍未确认。
- 暂定处理或规避：不写入凭据、不接入真实账号/支付/服务端；保持默认桥接返回 `WX_MINIGAME_NOT_CONFIGURED`。
- 关闭条件：负责人确认上述平台资料与 SDK 支持矩阵，并完成受控导出和人工开发者工具/真机验证。

### RISK-007：SDK 与项目存在 LitJson 重复程序集
- 状态：调查中
- 类型：风险
- 影响范围：`Assets/WX-WASM-SDK-V2/Runtime/Plugins/LitJson.dll`（<服务器地址>）和 `Assets/Plugins/LitJson.dll`（<服务器地址>）。
- 证据与复现步骤：SDK 导入编译日志显示 Unity 选择了 SDK 的 <服务器地址>，并忽略项目已有的 <服务器地址>。
- 暂定处理或规避：本轮不删除、替换或禁用任一 DLL；后续导出前核验现有网络/协议代码对旧版 LitJson API 的兼容性。
- 关闭条件：完成项目现有调用的兼容性检查，并在受控导出中确认没有程序集加载或序列化异常。

### RISK-008：小游戏 YooAsset 远端资源地址仍指向 Windows 构建目录
- 状态：已复现，WebGL 资源已发布，待主工程运行验证
- 类型：风险
- 影响范围：小游戏启动后的资源版本、清单和资源包加载。
- 证据与复现步骤：`Assets/Main/Boot.unity` 的 `Init` 组件将默认与备用地址配置为 `https://<CDN域名>/1/HotUpdate/NewAB/StandaloneWindows64`。负责人在开发者工具运行主工程时，YooAsset 下载该目录的 AssetBundle 后报告其构建目标为 `19`，与当前 WebGL/小游戏平台不兼容，资源加载失败。2026-08-27 已以 WebGL 和 `BuiltinBuildPipeline` 成功构建 `DefaultPackage` 版本 `2026-08-27-686`；输出目录包含版本文件、清单和 14 个资源包，报告构建目标为 `20`，DLL 地址均来自 `Assets/HotUpdateResources/Dll/WebGL`。负责人已将文件发布到 OSS 的内层版本目录；外网读取该目录的 `PackageManifest_DefaultPackage.version` 返回 HTTP 200 且内容匹配 `2026-08-27-686`。
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

### BUG-016：英雄详情页“前往获取+已满级”按钮从未绑定点击事件
- 状态：**已关闭**（负责人 2026-09-20 Play 验收通过）
- 类型：缺陷
- 影响范围：`ClientHeroDetailPage` 的升级/获取入口。对**已解锁**英雄表现为“按钮点得动但毫无反应”；对未解锁英雄因处于只读态而不可点，反而掩盖了该缺陷。
- 证据与复现步骤：只读场景核对（`Logs/verify-scene-facts.ps1` → `Logs/verify-scene-facts.txt`）确认 `Assets/Client/Scenes/ClientShell.unity` **全场景没有任何名为 `前往获取Canvas` 的节点**（0 个）。而 `EnsureBound` 用 `FindDirectChild(_pageRoot.transform, "前往获取Canvas")` 取容器 → 容器恒为 `null` → `_upgradeButton` 恒为 `null` → `BindControls` 中 `if (_upgradeButton != null)` 分支永不进入，`RequestUpgrade` 因此从不装配。场景侧该按钮 `Button.m_OnClick` 的 `m_MethodName` 条目数实测为 **0**（无持久监听），故点击确实无响应。本轮之前只给 `SetReadOnly` 补了节点名回退，漏掉了这条绑定路径。
- 暂定处理或规避：`EnsureBound` 在 `前往获取Canvas/前往获取Button` 落空时，回退查找场景实际节点名 `前往获取+已满级Button`，与 `SetReadOnly` 的目标保持一致（两处同一按钮）。仅改 `Assets/Client/Runtime/ClientHeroDetailPage.cs`，未修改场景与任何资源；Roslyn 三目标（Editor / Assembly-CSharp-Editor / WebGL）**0 error**。
- 关闭条件：负责人在 Play 中用**已解锁**英雄进入详情页点击该按钮，出现升级成功或明确失败提示（`UpgradeSucceeded` / `TipRequested`，或 `[Client] ...（Tips 弹窗待接入）` 警告），即该按钮已通。
- 交叉引用：本条只解决了“onClick 没装配”。**仅修本条不会让按钮生效** —— 还有 BUG-017 那道更前置的阻断（`Show()` 从未被调用，`_currentHero` 恒为 `null`，`RequestUpgrade` 会在开头 `return`）。两条都已修，需一并复测。

### BUG-017：弹窗页面从未被 `Fallback` 解析，导致 `Show()` 不被调用（英雄详情“只读”与“升级”同时失效）
- 状态：**已关闭**（负责人 2026-09-20 Play 验收通过：“可以，完美实现”）
- 类型：缺陷
- 影响范围：所有经 `ClientUiNavigator.OpenHeroDetail` 的入口（英雄页 / 图鉴页点卡进详情），以及**同类写法**：任何"先 `Open(pageId)` 再用 `Fallback(pageId)` 取节点喂数据"的弹窗。表现极具误导性：详情页**能正常打开**，但从不按所点英雄初始化 —— 未解锁英雄的只读不生效（页面停在启动时的 `SetReadOnly(false)`），升级按钮即便装上了 onClick 也会在开头 `return`。
- 证据与复现步骤：
  1. **真实时序**（`%LOCALAPPDATA%\Unity\Editor\Editor.log`）：`[1940] [Client] 英雄详情只读=False，已作用于 5 个按钮…` 出现在**场景启动绑定**（`EnsureBound` 经 `Awake`）时；而 `[2056] 点击图鉴卡` → `[2059] [UI追踪][弹窗] Open …HeroDetail…` 之后**再没有任何 `只读=` 日志** ⇒ `Show()` 确实没被调用。此前把这条 `False` 读成"负责人点的是已解锁英雄"，是误判。
  2. **代码根因**：`OpenHeroDetail` 在 `Open(pageId)` 成功后查 `Fallback(ClientUiPageId.HeroDetail)`；`Fallback` 只查 `_pageById`，而该表只收录 `ClientUiPage`（PagesLayer 页面）。只读场景核对：**全场景仅 1 个 `ClientUiPage`**（`PagesLayer/MainPage主页`，`pageId=0 (Home)`）。弹窗登记在 `ClientPopupService._popupById`（键为 `ClientUiPopup._pageId`），**从不进入 `_pageById`** ⇒ `Fallback(HeroDetail)` 恒为 `null` ⇒ 提前 `return`。
  3. **点击追踪器佐证**：被点的确实是图鉴页那张**未解锁**卡（命中 `HeroPage英雄/GuideSubPage/…/Content/英雄卡Item_Instance/Mark`，`Mark` 即未解锁遮罩），却走成了"已解锁"路径 —— 说明所点英雄压根没传到页面。
  4. **排除下标错位**：`ClientHeroCardList.Show` 用 `int index = i` 逐次捕获，`unlockedOf(items[i])` 与 `onClick(index)` 同源同序；实例数恒等于数据条数。数据侧 `已解锁 1 / 共 2`。
- 暂定处理或规避：`ClientPopupService` 新增 `GetPopup(ClientUiPageId)` —— 弹窗注册表归该服务所有（§12.4 同一所有者原则），就该由它回答"pageId 对应哪个节点"；`ClientUiNavigator.OpenHeroDetail` 改为先向弹窗服务取节点并 `Show`，保留 `Fallback` 作为兼容路径。仅改 `ClientPopupService.cs` 与 `ClientUiNavigator.cs`，未写场景；Roslyn 三目标 0 error。
- 关闭条件：Play 中点图鉴页那张带锁卡，日志出现 `只读=True，已作用于 5 个按钮`，且**装备栏 4 槽与升级按钮点不动**；随后用已解锁英雄复测 `只读=False` 且升级按钮有响应（配合 BUG-016）。
- 范围澄清（负责人 2026-09-20 明确）：只读**仅**作用于装备槽与升级按钮；**天赋 / 秘技 / 终结技三个 Toggle 不受限制，任何时候都可切**。早期实现曾把三个 Toggle 一并锁住，属超范围，已撤除（见 `ClientHeroDetailPage.SetReadOnly`）。
- 工具局限（供后续会话）：只读场景核对脚本 `Logs/verify-scene-facts.ps1` 解析的是场景 YAML，**看不到 prefab 实例内部的组件与名字**（`HeroSubPage` 的英雄卡模板是 prefab 实例，其 `ClientHeroCard` 因此未被脚本列出，一度与日志 `英雄卡列表=True` 矛盾）。凡涉及 prefab 实例的结论，须以运行日志或 Inspector 复核。

### BUG-018：个人中心玩家名 / 玩家ID / 战力从未接线，页面一直显示美术占位字
- 状态：**已修复并由负责人 Play 验收通过（2026-09-21 第 81 轮复验）** —— 玩家名/玩家ID/战力三处 TMP 文本已接线（`Logs/repair-profilelists.log`）。
- 类型：缺陷
- 影响范围：`ProfilePage个人中心` 的玩家名 / 玩家ID / 战力三处文本。表现**不崩、无报错**，只是永远显示场景里烘死的占位文字 —— 这正是它长期未被发现的原因（同类：`_notice` 也是 `fileID 0`）。
- 证据与复现步骤：新增只读取证「未接线字段审计」（`Logs/verify-scene-facts.ps1` 的 `CLIENT COMPONENT FIELD AUDIT` 段 → `Logs/verify-unwired-fields.txt`）列出：`ClientProfilePage @ ClientCanvas/PopupLayer/ProfilePage个人中心  [空引用] _playerName,_playerId,_combatPower,_notice`。对照代码 `ClientProfilePage.Refresh()` 开头即 `if (_playerName == null || _playerId == null || _combatPower == null) return;` ⇒ 三个字段全空时**必然提前返回**，真实数据永远写不进去。全场景 `Client` 脚本共 53 个、含未接线字段的组件 39 个。
- 暂定处理或规避：需**场景写入**（把三个文本节点接到字段上）。目标节点尚未最终指认 —— 按名搜索已见到 `ProfilePage个人中心/…/SubContentRoot/Content_Title/称号Image/玩家名称Image`、`…/Content_Avatar/头像Image/玩家名称Image`、`…/Content_Info/徽章Image/玩家名称Image`、`…/Content_Title/称号Image/战力底框` 等候选，须先 dump 该页子树确认哪个才是"玩家名 / 玩家ID / 战力"文本节点，再执行写入（AGENTS.md §4：场景写入需负责人确认）。
- 关闭条件：Play 进入个人中心，三处文本与 `LocalClientDataService.GetProfile()` 一致（非场景占位字）。
- 交叉引用：`ClientHomePage._notice` 同为 `fileID 0`，但已由全局 Toast 路径绕过（第 47~49 轮），不影响功能。

### BUG-019：排行榜 6 张精灵查找表全为空，排行/阵容卡的头像·框·徽章·立绘·属性·品质解析不出图
- 状态：**部分修复** —— 品质（5 项）与立绘（4 项）查找表已按实际贴图重建（`Logs/repair-rankvisuals.log`）；**头像/头像框/徽章/属性四张表仍为空**（工程内无对应贴图，需美术或热更包）。
- 类型：缺陷
- 影响范围：`RankPage排行榜` 的玩家行与阵容卡视觉；`ClientRankPage` 的 `_avatars / _frames / _badges / _illustrations / _attributes / _qualities` 六个 `VisualBinding[]` 在场景里**全部为 `[]`** ⇒ `SetSprite` 拿不到任何图，卡片只剩场景烘死的占位图。
- 证据与复现步骤：`Logs/verify-unwired-fields.txt` 中 `ClientRankPage @ ClientCanvas/PopupLayer/RankPage排行榜  [空数组] _avatars,_frames,_badges,_illustrations,_attributes,_qualities`；代码侧 `ClientRankPage` 第 144 / 159 行把六张表分别喂给玩家行与阵容卡。
- 暂定处理或规避：**不要凭猜填**。需要负责人给出"资源 id（头像/头像框/徽章/属性/品质）↔ 具体 sprite 资源"的对应关系，或指明这些数据本该由哪张配置表/哪个资源目录驱动；随后可用与 `_teamSlotDigits` 相同的方式（编辑器按目录批量 `LoadAssetAtPath`）写入。
- 关闭条件：排行榜玩家行能显示头像与头像框、阵容卡能显示立绘/属性/品质图，且换 id 会换图。

### BUG-020：购买数量进度条组件三个字段未接线，进度与上下限文本不驱动
- 状态：**已修复并由负责人 Play 验收通过（2026-09-21）** —— `_progressFill` 等字段已接线并复验通过（`Logs/repair-shopbug020.log`）。
- 类型：缺陷
- 影响范围：`Product Purchase Interface/Purchase of digital items/…/Progress bar icon` 上的 `ClientShopPurchaseQuantityProgress`：`_progressFill` / `_minimumText` / `_maximumText` 均为 `fileID 0` ⇒ 购买数量条的填充值与 0 / 上限文本不随数量变化（字段各自判空，故不报错）。
- 证据与复现步骤：`Logs/verify-unwired-fields.txt` 中 `ClientShopPurchaseQuantityProgress @ ClientCanvas/PopupLayer/Product Purchase Interface/…/Progress bar icon  [空引用] _progressFill,_minimumText,_maximumText`；代码第 164 / 183 行分别按字段判空后写入 `fillAmount` 与 `"0"`。
- 暂定处理或规避：需场景写入；先 dump 该节点子树确认填充 Image 与两处文本节点，再接线（同 BUG-018 的前置流程）。
- 关闭条件：拖动/点击购买数量时进度条填充与上下限文本同步变化。

### RISK-015：其余未接线项汇总（红点无目标节点 / 详情底键图标 / 页签选中图 / 邮件详情 / 弹窗遮罩）
- 状态：待确认（均**已静态确证为未接线**，但目标节点、产品语义或是否本期范围尚不明确）
- 类型：风险
- 影响范围与证据（同一份 `Logs/verify-unwired-fields.txt`）：
  - `ClientHomeRedDotController._mailDot / _noticeDot`：邮件 / 公告红点永不显示。**全场景按名搜索只有一个红点节点** —— `ClientCanvas/PagesLayer/MainPage主页/downCanvas/ActivityButton/RedDot` ⇒ 目标节点**不存在**，要么新建（属新建 UI，需批准），要么把该控制器改为"按名查找现有红点"。
  - `BottomFunctionIconView._icon / _label`（英雄详情 `upCanvas/Panel/Bottom1..4` 共 4 个）：底部功能键的图标与文字不驱动。
  - `ClientHeroPage._heroTabSelected / _guideTabSelected`：页签选中图不切（目前靠 Button 过渡色替代，视觉上尚可接受，故排在后面）。
  - `ClientMailPage._detail / _status`：邮件详情与状态区无数据（**早已在 `Assets/Client/MODULE.md` 记录为"尚未绑定"**）；同页 `_emptyHint` 有按名自愈，无影响。
  - `ClientPopupService._maskRoot`：弹窗**无遮罩、不阻断**背后点击；场景里遮罩内嵌于各页面，是否要给弹窗服务统一遮罩需产品决定。
- 暂定处理或规避：本轮**一律不改**（负责人 2026-09-20：先等他的东西）。上述项都属"静默失效、不报错"型，改动前先确认产品语义与是否本期范围。
- 关闭条件：逐项由负责人确认「要做 / 不做」；要做的按 `BUG-018` 的前置流程（先 dump 子树确认目标节点，再场景写入）执行。

### RISK-016：未接线字段中**已核实为良性**的清单（勿再重复排查）
- 状态：已核实（`fileID: 0` ≠ 缺陷，这些都有自愈或运行时注入）
- 类型：风险
- 证据与结论（同一份审计输出，逐项读过代码确认）：
  - `ClientHeroCard._teamBadgeNumber`（编队卡 ×6）：`SetTeamSlot` 在字段为空时会 `FindDeepChild(_teamBadge.transform, "编号数字")` 自愈；已用 prefab 源核实 `编号数字 -> 角色卡牌Button-final 1/编队队内编号Image/编号数字` **确为子节点** ⇒ 换数字生效。
  - `ClientUiPopup._popupRoot`（×18）：`Awake` 与 `PageRoot` 取值均回退 `gameObject`。
  - `ClientShellController._popupService / _feedback`：由组合根运行时注入。
  - `ClientSystemFeedback._toastText`：本工程走 TMP 槽位 `_toastLabel`（两个槽位"任选其一"）。
  - `ClientMailPage._emptyHint`：有按名自愈。
  - `ClientHomePage._notice`：已被全局 Toast 路径绕过（第 47~49 轮）。
- 暂定处理或规避：**不要**为了"补全 Inspector"而去写这些字段（会产生无意义且不可逆的场景差异）。
- 关闭条件：无需关闭；本条作为**排除清单**长期保留。

### BUG-021：磁盘缓存 `config.pkg` 优先级高于本地表 json，导致新表"放了不生效"
- 状态：**已定位，规避已执行（备份后删除了旧缓存）**；机制本身**不是缺陷**，但极容易误判为"表没生效"
- 类型：缺陷（**排查陷阱**）
- 影响范围：所有通过 `TableLoadHelper` 读配置表的路径 —— 编辑器、WebGL、微信小游戏真机**同理**。表现是"本地 json 明明更新了，运行时数据还是旧的"，且**不报错**。
- 证据与复现步骤：
  1. `Assets/Scripts/Table/TableLoadHelper.cs` 第 47~60 行 `LoadFromLocalPackage()`：先看 `{Application.persistentDataPath}/config.pkg`，**文件存在就直接用它**；`Resources/Table/<TableName>.json` 只是**没有缓存时**的兜底。
  2. 实测：本地已部署 21 张 json（`Hero.json` **54 行 / 54 个唯一 Id**），但表探针只报 **`THeroHelper = 12 行`**。
  3. 找到缓存：`C:\Users\Administrator\AppData\LocalLow\xiaoshi\xiaozhen\config.pkg`（**2026-09-03 16:34**，9.3 KB，9/3 WebGL 调试遗留的旧表包）。
  4. **备份到 `Logs/config.pkg.bak-20260903` 后删除**该缓存，重跑探针 ⇒ `THeroHelper = 54 行`，**21 张表全部可用（0 空表 / 0 缺失）**，且 0 次「本地配置表缺失」警告。
- 暂定处理或规避：
  - **测试新表前，先确认 `{persistentDataPath}/config.pkg` 是否还在**；在就备份后删掉，否则永远读到旧表。命令行一键查看/清理见 `CURRENT_STATE.md`「第 50 轮」。
  - 生产环境靠 `LoadFromNet(ossUrl, md5)` 覆盖：仅当 `localPackageMd5_ != remoteMd5` 才下载新包并落盘（`_asyncLoadFromNet`）。
  - ⚠️ 因此**"只更新本地 json"在生产链路里不会生效** —— 本地 json 是**首次启动兜底**，缓存/远端包才是后续事实来源。这是既定设计，不是 bug；但必须写清楚，否则每次改表都会重踩。
- 关闭条件：无需关闭（机制性陷阱）。已加只读探针把"真实行数"打出来，避免再次靠猜。

### RISK-017：命令行 Roslyn 验证脚本**不覆盖 `HotFix` 程序集**（`Assets/Scripts`）
- 状态：已确认（工具盲区）
- 类型：风险
- 影响范围：任何对 `Assets/Scripts/**` 的改动 —— 用 `Logs/verify-compile.ps1` 验证会**看到假失败 / 假成功**。
- 证据与复现步骤：
  1. `Assets/Scripts/HotFix.asmdef` 存在 ⇒ `Assets/Scripts/**`（含 `Table/*.cs`）编入 **`HotFix`** 程序集，不在 `Assembly-CSharp` 里。
  2. `Logs/verify-compile.ps1` 的编译清单取自 `Library/Bee/artifacts/<dag>/Assembly-CSharp.rsp`，其中 **`Scripts\Table` 源文件命中 = 0**，只有 `-r:"…/HotFix.ref.dll"`（**上次 Unity 构建**产出的引用程序集）。
  3. 实测后果：给 `THero` 补 `Portrait1/Portrait2` 后跑 Roslyn 验证，报 **23 个 `CS1061`（'THero' 不含 'Portrait1'）** —— 实际代码没问题，是引用的是旧 ref。
- 暂定处理或规避：
  - 改 `Assets/Scripts/**` 时，验证必须走**命令行 Unity 全工程编译**：`-batchmode -quit -executeMethod Pinball.Client.Editor.ClientTableProbe.LogRowCounts`（顺带打印表行数），或任意不产生副作用的 Editor 静态方法。
  - **不要**用 `RepairAll` 之类会写场景的方法来做编译验证。
- 关闭条件：无需关闭；本条作为**验证纪律**长期保留。

### RISK-018：`Assets/Client` 直接引用 `HotFix` 类型会复发 IL1005 / 读到旧成员
- 状态：已确认（架构边界纪律）
- 类型：风险
- 影响范围：`Assembly-CSharp`（含 `Assets/Client`）与 `HotFix` 之间的任何编译期耦合。
- 证据与复现步骤：
  1. `TCP_WEBGL_HANDOFF.md`「当前阻塞与禁止事项」明确记录：**不要再以"直接引用 HotFix 类型"解决反射问题；这会复发 IL1005/程序集解析失败**。
  2. 同一份文档的既定做法是 `ClientTcpConnectionProbe` 用 `Type.GetType("NetController, HotFix")` **反射**跨边界。
  3. 本轮 `TableClientConfigService` 第一版直接引用 `THero/THead/TBadge`… ⇒ 既撞上 RISK-017 的旧 ref 问题，也违反上述纪律；已改为**全程反射**（`THeroHelper.DataMap` 用 `GetField`，行字段用 `PropertyInfo` + 缓存）。
- 暂定处理或规避：`Assets/Client` 访问 `HotFix` 内类型**一律走反射**，并在客户端侧定义自己的 DTO（如 `ClientHeroConfig`）承接，UI 只依赖接口。
- 关闭条件：无需关闭；本条作为**分层纪律**长期保留。

### BUG-022：工程生成类与实际配置表**结构漂移**，运行时 99 条 `FormatException`
- 状态：**对 Client 已隔离（第 52 轮）**；对战斗侧仍开放，但按负责人 2026-09-20 的边界「只负责 Client 与平台移植」，**不在本次范围**
- 类型：缺陷
- **隔离方式（第 52 轮）**：`Assets/Client` 不再调用 `TableManager.InitData()`、不再反射 `T*Helper`，改为 `ClientTableSource` + `LC.Newtonsoft.Json.Linq` **自己只读需要的那 6 张表**（Hero / Head / HeadFrame / Badge / Nameplate / Title）。于是本条的 99 条报错**从根上不再出现**（实测 `Logs/client-table-probe.log`：`配置表解析出错` = 0）。
  战斗侧若仍要整批加载 21 张，本条依旧存在，需战斗侧自行对齐表结构。
- 影响范围：所有通过 `TableLoadHelper` 读表的路径（编辑器 / WebGL / 小游戏）。表现：**Console 刷 99 条红色 `配置表解析出错：System.FormatException`**，同时若干字段**静默丢失或恒为 0**。
- 证据与复现步骤：
  1. **报错原文（`%LOCALAPPDATA%\Unity\Editor\Editor.log`，实测 99 条）**：
     `配置表解析出错：System.FormatException: Input string was not in a correct format.`
     堆栈：`TableLoadHelper._setObjectPropertyValue`(TableLoadHelper.cs:313) → `_createObjectByJToken`(:297) → `JToken.ToObject<T>` → `op_Explicit` → `Convert.ToInt32(string)`。
  2. **根因**：`Skill.json` 有 33 行，其 `Effect1Subtype / Effect2Subtype / Effect3Subtype` 的值是 **`"[]"`（数组字符串）**，而**工程里** `Assets/Scripts/Table/TSkill.cs` 把这三个字段声明为 **`int`** ⇒ 33 × 3 = **99** 条，数字完全吻合。配置目录的**新** `TSkill.cs` 已把它们改成 `JArray`，并删掉 `*SubtypeParameter`、新增 `*ResId`/`Attack`。
  3. **全表漂移普查**（可复用脚本：`Logs/verify-table-drift.ps1`；行数已自检与 json 一致）：
     - `Skill`：类型不符 ×3；json 独有（被丢弃）`Effect1/2/3ResId`、`Attack`；类独有（恒 0）`Atktype`、`Effect1/2/3SubtypeParameter`。
     - `Resources`：json 独有（被丢弃）`SkeletonData`；类独有（恒 0）`ScaleX/ScaleY/ScaleType/ActionName/AnimationName/BbWidth/BbHeight/Int`。**256 行全表受影响**。
     - `RandomHero`：json 独有 `Heroid` 被丢弃；类的 `Weight` 恒 0 ⇒ **随机权重失效**。
     - `Coefficient`：json 独有 `SkillBeneficialEffect`、`SkillNegativeEffect` 被丢弃。
     - `MonsterTemplate.Rewards` 恒 0；`Hero.Portrait` / `Head.ResId` 恒空（**这两个是本轮刻意保留的兼容字段**，属预期）。
  4. **为什么以前没暴露**：此前工程内**没有任何表 json**，`TableLoadHelper` 一直走"本地配置表缺失，使用空表继续"分支 ⇒ 表压根没加载，自然不会报解析错。本轮投放本地表后才显现。
- 暂定处理或规避：**本轮不动**。修复方式是让 `Assets/Scripts/Table/T*.cs` 与配置目录对齐（整批重新生成而不是手改），但**这会碰到战斗代码**：
  `Assets/Scripts/BNRoom/Static/GetSkillEffectParameters.cs:48~53` 正把 `Effect1/2/3Subtype` 与 `*SubtypeParameter` **当 `int` 用**（`sub1.Add(...)`）；改成 `JArray` 会破坏战斗技能参数装配。**属原公司战斗逻辑，超出本次授权范围。**
  → 需负责人决定：① 由战斗侧适配新表结构（推荐）；② 暂不投放 `Skill`（但 `TableLoadHelper` 是按 21 张清单整批加载的，需另议机制）；③ 由导出侧提供兼容旧结构的 json。
- 关闭条件：Console 不再出现 `配置表解析出错`；且 `Resources.SkeletonData`、`RandomHero.Heroid/Weight`、`Skill.*` 等字段在运行时取值正确（可用 `Client/配置表/打印各表行数` + 字段抽查）。
- 交叉引用：`RISK-017`（Roslyn 验证不覆盖 HotFix）—— 本条正是"只验 `error CS` 会漏掉运行时解析错误"的实例。

### RISK-019：验证纪律 —— 只看 `error CS` 会漏掉运行时 `LogError`
- 状态：已确认（本轮**实际踩坑**）
- 类型：风险
- 影响范围：所有"用命令行 Unity 跑一遍就算验证过"的场景。
- 证据与复现步骤：本轮 `Logs/table-probe-2.log` 中实际存在 **99 条 `配置表解析出错`**（含堆栈共 198 行），但当时只 grep 了 `error CS`，于是对外报告为"0 error" —— 属**报告失真**。负责人 Play 时立刻看到 99+ 报错。
- 暂定处理或规避：批处理/Play 验证后，**必须同时扫描**：`error CS`、`Exception`、`LogError` 业务关键字（本项目为 `配置表解析出错`、`本地配置表缺失`），再下"0 error"的结论。
- 关闭条件：无需关闭；作为**验证纪律**长期保留。

### RISK-020：`SystemLayer` 分层与 `HANDOVER_PLAN.md` §3 不一致（**待负责人定 A/B/C**）
- 状态：待负责人裁决（**未改动任何场景/代码**）
- 类型：架构偏差（风险）
- 影响范围：全局 UI 分层、Toast/Loading 宿主、未来红点与系统级提示。
- 证据与复现步骤（2026-09-20 只读取证）：
  `ClientCanvas` 直接子节点与激活态实测为：`[1] PopupLayer`、`[1] PagesLayer`、`[0] SystemLayer`、`[0] ToastRoot`；
  而 `SystemLayer` **恒为 inactive**，其唯一子节点是 `加载Page`。
  仓库内 `HANDOVER_PLAN.md` §3 要求：`PagesLayer`=可导航页面、`PopupLayer`=业务弹窗、`SystemLayer`=Loading/Toast/红点/系统提示。
  代码耦合：`ClientLoadingService` **显式开关 SystemLayer**（因为加载页在这个恒关的层里）；
  `ClientSystemFeedback`(Toast) 原先挂在 SystemLayer，因该层恒关导致 `StartCoroutine ... game object is inactive`，已移到 `ClientCanvas`；
  `ClientPopupService` 依赖 `PagesLayer` 的第一个子节点（大厅）做射线屏蔽；修复工具里有 **2 处**"清理 SystemLayer 下失效节点"的逻辑。
- 三个方案与影响：
  1. **A 按计划归位**：SystemLayer 置为常开并**移到最后一个 sibling**（否则 Toast 会被弹窗遮挡）；ToastRoot/Feedback 移回；
     修复工具 2 处清理逻辑反转；`ClientLoadingService` 去掉开关层逻辑。风险=遮挡/排序 + 要求 `加载Page` 显隐自管（否则常显）+ 反复跑工具会来回搬。工作量约半天。
  2. **B 接受现状**：只改 `HANDOVER_PLAN.md` §3 的分层描述（Toast 归 ClientCanvas）。零场景风险，但需负责人认这个偏差；红点/系统提示宿主仍待定。约 0.5 小时。
  3. **C 折中（推荐）**：`SystemLayer` 改为**常开**（`加载Page` 仍由 `ClientLoadingService` 全权开关），**不搬 Toast**；修复工具那 2 处改为不清理。
     收益=消掉"恒 inactive"这个反直觉状态（已致一次真实故障），零遮挡风险；代价=Toast 与 Loading 仍分处两层。约 1 小时。
- 暂定处理或规避：**未改动**；等负责人选 A/B/C。此前为让 Toast 可用，已把 `ClientSystemFeedback` + `ToastRoot` 放在 `ClientCanvas`（属 C 方案的现状）。
- 关闭条件：负责人选定方案并落地完成、且 `HANDOVER_PLAN.md` §3 与实际一致。

### BUG-023：客户端「UI / 功能接口」与「数据表」对不上的总清单（2026-09-20 全量审计）
- 状态：开放（**已静态确证**；多数需负责人给规则或确认归属）
- 类型：缺陷 / 契约偏差
- 影响范围：所有消费配置表数据的页面；是"图标显示不出、排行 6 张表为空、详情页取不到真图"等一串现象的共同根因。
- 证据与复现步骤（用 `grep` 全量核对 `Assets/Client` 与 20 张表）：

  **① 资源标识"两种形态"，且客户端没有任何解析通路（影响最广）**
  · 表给**名字**：`Hero.QualityIcon`(`QualityIcon_3`) / `CatapultIcon` / `ElementIcon` / `Avatar`(`HeroAvatar_13001`) / `Portrait1|2`(`HeroPortrait_13001_c|_d`) / `Item.Icon`(`Item_110001`)；
  · 表给**整型 id**：`Skill.SkillIcon` / `Potency.SkillIcon` / 收藏品 `HeadFrame|Badge|Nameplate|Title.ResId` / 排行页 6 张查找表。
  · **客户端把这些字段读进了模型，但没有任何 UI 消费它们，也没有"名字或 id → Sprite"的解析器** ——
    `grep QualityIcon|CatapultIcon|ElementIcon|Portrait1|Portrait2|.Icon` 的结果**只出现在模型定义与探针**里，零消费点。
  · 后果：英雄卡无品质/元素/立绘（`ClientHeroCard.Apply` 的 `illustration` 页面恒传 `null`）；ProfilePage 收藏品只能显示模板图；`BUG-019` 排行 6 张表全空。
  · 需负责人给：**资源名/资源 id ↔ sprite 的对应规则**（例如是否经 `Resources` 表把 id 换成名字、再由 YooAsset 地址加载），或指明应由哪张表/哪个目录驱动。

  **② 语义不一致**
  · `ClientItemType` = `Material(1)/Equipment(2)/Consumable(3)`，而表 `Item.Type` 实测 `1` = **货币**（点数/源晶/晶核）、`3` = 消耗品、**`2` 表内没有任何一行**。
  · ✅ **2026-09-20 第 70 轮已修（此项关闭）**：
    - 枚举按表对齐为 `Unknown = 0` / `Currency = 1` / `Consumable = 3`，**`2` 不设成员**（表内无该值，语义未定义 ⇒ 不猜）；
    - 5 处硬编码（邮件附件 / 活动奖励 / 两条奖励循环 / 任务奖励）改为**表驱动** `ResolveItemType(itemId)`
      （`Item.Type`：1→`Currency`、3→`Consumable`、表内无 ⇒ `Unknown`）—— 不再随表变化而变错；
    - **副产物（真缺陷）**：表内**根本没有"装备"类型**，而 `TrySetEquipmentEquipped` 依赖 `ClientItemType.Equipment`（值 2）
      ⇒ 该判定**永远失败**。为不擅自引入未确认的产品规则，改为显式 `IsEquippableItem(item)`（**当前恒 false**）+ 明确失败原因
      "装备功能尚未接入（配置表暂无装备类型）"，**行为与原先一致**；等负责人确认装备数据来源后再放开。
    - **验证**：编译三目标 0 error；新增 `ClientTableProbe.VerifyItemTypeMapping()` **真的走一遍"一键领取"**，
      实测 `入包道具 110004（表 Type=3）ItemType=Consumable ✓`（`Logs/verify-itemtype.log`），并给出结论行"与配置表完全一致"。
  · `Prop Description/限购数量` 烘死文案是 **`（1/6）`（当前/上限）**，而 `ClientShopPurchaseQuantityProgress` 写的是 `_minimumText="0"` / `_maximumText=max` ⇒ 语义不同；
    故 BUG-020 只接了 `_progressFill`，**上下限两个字段刻意留空**（硬接会覆盖设计文案）。

  **③ UI 需要、表里根本没有（属服务端）**
  · 商城商品（`ClientShopListing`：名称/单价/限购）、卡池价格与概率（`RandomHero` 只有 `Id`/`Heroid`）、邮件、活动、任务、公告、
    排行榜行、玩家档案/钱包/拥有列表、编队 —— **20 张表里都没有**。
  · **战力**：`Hero` 表**没有战力字段**（`Coefficient` 有 `HPFight/AttackFight/DefenceFight/SpeedFight`，但公式未确认）⇒ 已按"服务器下发"处理，客户端不自算。

  **④ 表里有、UI 没有对应位置（数据闲置）**
  · `Hero.CatapultType/CatapultIcon`（弹射类型/图标）、`Hero.LevelCap`（突破上限 `[10,20,30,40,50]`）、`Hero.LnlayUnlock`（`[1,15,30,45]`，语义未确认）；
  · `Item.Place / is_use / is_use_value / Stack`；`Potency.EffectType`（只显示 `Desc`，不解释类型含义）；`Energy` / `ServerControl` / `Dan` / `PinBallRoom` 等表客户端零消费。

  **⑤ 形态不一致**
  · 英雄详情 `ClientHeroDetailData` 用 **int 图片 id**（`UpImageId` / `IllustrationIds` / `VerticalImageIds` / `CardInfoImageIds`），
    而表给的是**资源名**；本地模拟只能编 `1000+heroId`、`2000+heroId`、`4001..4004` 这类假 id ⇒ **详情页图片拿不到真图**。
  · `ClientPlayerProfile.PlayerId` 是 **string**，而配置实体 id 一律 int（玩家标识属服务端，待确认是否应为数字）。
- 暂定处理或规避：①②⑤ 需负责人给规则/确认归属后才动；③ 维持本地模拟；④ 记录为"数据闲置"，不擅自加 UI。
- 关闭条件：所有 UI 需要的资源标识都能解析出图；枚举/文案语义与表一致；UI 需要而表缺失的数据要么由服务端提供、要么明确记为本地模拟。

#### BUG-023 补充（2026-09-20 第 5 轮取证）：① 的「规则未定义」已被推翻 —— 规则就在 `Resources` 表里

**映射规则（从数据中找到，无需负责人提供）**：`Resources` 表（256 行）的 `Id → SkeletonData` 就是「资源 id → 资源名」映射表，
收藏品的 `ResId` **正是** `Resources.Id`：

| 收藏品表 | `ResId` 例 | `Resources.SkeletonData`（资源名） |
|---|---|---|
| `Title`（称号） | 51001 | `Title_51001` |
| `HeadFrame`（头像框） | 52001 | `AvatarFrame_52001` |
| `Nameplate`（铭牌） | 53001 | `Nameplate_53001` |
| `Badge`（徽章） | 54001 | `Medal_54001`（徽章的资源名叫 **Medal**） |

⇒ 解析链应为：`ResId` →（`Resources` 表）→ 资源名 → 加载 Sprite。**这一步已可实现。**

**但 `Resources` 表只覆盖「特效/骨骼 + 收藏品」**：搜 `SkeletonData` 含 `HeroAvatar_` / `QualityIcon_` / `ElementIcon_` / `AttributeIcon_` /
`Item_` / `HeroPortrait_` / `CatapultIcon_` 均 **0 条** ⇒ 这些族的名字不来自该表（分别直接写在 `Hero.Avatar` / `Hero.QualityIcon` / `Hero.ElementIcon` / `Item.Icon` 等字段里）。

**工程内美术盘点（按名搜 `Assets`，排除 `.meta`）**：

| 资源族 | 工程内 | 影响 |
|---|---|---|
| `HeroPortrait_*`（英雄立绘） | ✅ 有（**4 个**：12001 / 13001 / 14001 / 15001） | 立绘可接，未覆盖的英雄取不到图 |
| `QualityIcon_*`（品质） | ✅ 有（**5 个**：`_2`..`_6`，另有 5 个 `*Effect.prefab`） | **可接**（第 65 轮已接） |
| `ElementIcon_*`（元素） | ✅ 有（**5 个**：`_1`..`_5`）⇒ **元素 6（暗）缺图** | 可接，暗元素取不到图 |

#### 缺图清单（2026-09-21 实测；负责人已同意提供素材）

工程内**按命名规范的贴图实测为 0**（`HeroAvatar*` / `AvatarFrame*` / `Medal*` / `Nameplate*` / `Title_*` / `AttributeIcon*` / `Item_*` 全部 0 个文件）；
但 `Assets/Client/UI/切图(11)/**` 里有**中文名原始切图**（头像 30 / 头像框 11 / 徽章 7 / 铭牌 14 / 称号 14 张）。
**要接上解析，需要"id ↔ 贴图"的对应**（二选一：按右列命名导出到 Sprites 目录，或给我一份对照表）。

| # | 类别 | 需要 | 期望命名（按资源 id 规则） | 数据侧现状 |
|---|---|---|---|---|
| 1 | **头像** `Head` | 6 张（默认头像/筑梦者/玉兔/嫦娥/唐三藏/孙悟空） | ⚠️ **表里 `ResId` 字段为空** ⇒ 需先定 id 或命名 | 表缺 `ResId` |
| 2 | **头像框** `HeadFrame` | 9 张 | `AvatarFrame_52001` … `_52009` | 表内 ResId 齐 |
| 3 | **徽章** `Badge` | 6 张 | `Medal_54001` … `_54006` | 表内 ResId 齐 |
| 4 | **铭牌** `Nameplate` | 5 张 | `Nameplate_53001` … `_53005` | 表内 ResId 齐 |
| 5 | **称号** `Title` | 9 张 | `Title_51001` … `_51009` | 表内 ResId 齐 |
| 6 | **属性图标** | 6 个（光水土风火暗） | `AttributeIcon_1` … `_6` | 工程内 **0** 张 |
| 7 | **元素图标** | 补 **元素 6（暗）** | `ElementIcon_6` | 已有 1..5 |
| 8 | **道具图标** | 商店/背包商品图 | 命名待定（需道具资源 id 映射规则） | 工程内 **0** 张 |
| 9 | 排行榜条目的头像/框/徽章 | 同 1~3 | — | ⚠️ 模拟数据 `AvatarId/AvatarFrameId/BadgeId` **全为 0** ⇒ 列表根本不查图（数据侧我可补） |

#### 2026-09-21 负责人决策落地（第二批）

| 决策（负责人原话） | 落地情况 |
|---|---|
| 个性化"演示效果"选 **B**（打开表里更多 `ClientShow` 行） | ✅ 已把 `Head/HeadFrame/Badge/Nameplate/Title` 五张表**全部行**改为 `ClientShow: 1`（6/9/6/5/9 = 35 行）⇒ 列表可见条数从 `1/1/1/3/1` 变成 **6/9/6/5/9**，未拥有的显示为"未解锁"态 |
| **个性化确定键语义** = "点条目选中 → 点确定键替换"（**不是返回**） | ✅ `ClientProfilePage`：点条目改为**只选中**（高亮 + Toast「已选择…（点确定键替换）」），5 个子界面（`Content_Avatar/Frame/Info/Title/Widget`）的 `个性化确定键Button` 绑定为**确认替换**；未选中时提示"请先选择…"；旧的"点击即装备"入口保留为转发 |
| **其余列表页接入滚动修复** | ✅ 已接入 **英雄 / 商店 / 活动 / 主页展示 / 全图鉴活动 / 英雄详情**（`ClientScrollFix.FixAll(gameObject, true)` 于各自 `Show`） |
| **装备**：不管数据，有接口就行 | ✅ 不动（接口已在，`TrySetEquipmentEquipped` 保持现状） |
| **战力**：显示哪个都行，后期按场景改 | ✅ 不动 |
| **邮件**：没问题了 | ✅ 已结（`BUG-027` ③ 验收通过） |
| **未接入页面不做** | ✅ 不再提议；`Email Details / Success Receipt / MarblePage / BackpackPage / GachaPage / NoticePage` 保持现状 |
| **清理我新建的 3 张冗余阵容卡** | ⏸ **需你关掉 Unity 编辑器后由批处理删除**（场景写入走结构修复工具 + 备份）；若嫌麻烦**也可以不清**（仅视觉冗余、当前被隐藏，无功能影响） |

> ⚠️ **2026-09-20 第 65 轮更正我自己的数据错误**：上表原先写的是「`QualityIcon_*` 20 个 / `ElementIcon_*` 10 个 / `HeroPortrait_*` 8 个」，
> 那是**错的**（我把 `*Effect.prefab` 之类也算了进去、并外推）。**实测（`Get-ChildItem` 按 `.png` 计数）为 5 / 5 / 4**。
> 教训：**数量类结论必须来自一次明确的计数命令，不能靠印象外推。**
| `HeroAvatar_*`（英雄头像） | ❌ **没有** | 个人中心头像、排行头像取不到图 |
| `Title_*` / `AvatarFrame_*` / `Nameplate_*` / `Medal_*`（收藏品） | ❌ **没有** | 收藏品列表只能显示模板图 |
| `Item_*`（道具图标） | ❌ **没有** | 道具图标取不到图 |
| `CatapultIcon_*`（弹射图标） | ❌ **没有** | 弹射图标取不到图 |
| `AttributeIcon_*`（属性图标） | ❌ **没有** | 属性图标取不到图 |

⇒ **① 的真实阻断已从「规则未定义」更正为「部分美术缺资源」**：品质 / 元素 / 立绘**可以接**；
其余需美术补图，或负责人确认它们本应来自热更资源包（`Resources_HotUpdate` 目前只覆盖品质/元素/立绘/部分图标）。

**另一个「接口与 prefab 对不上」**：卡prefab 里有 `品质Image` / `属性图标Image` 节点，但 `ClientHeroCard` **没有对应字段**
（字段只有 立绘 / 名称 / 等级 / 锁 / 遮罩 / 队内编号）⇒ 卡片要显示品质与元素徽标，需**加字段 + 接线**。

#### BUG-019 更正：阻断原因不是「映射规则未定义」，而是「美术缺资源」

- 排行页 6 张查找表分别需要：头像（`HeroAvatar_*` ❌ 缺）、头像框（`AvatarFrame_*` ❌ 缺，规则 = 52001 段）、
  徽章（`Medal_*` ❌ 缺，规则 = 54001 段）、立绘（`HeroPortrait_*` ⚠️ 部分有）、属性（`AttributeIcon_*` ❌ 缺）、品质（`QualityIcon_*` ✅ 有）。
- ⇒ **品质一类现在就能接**；其余 5 类需美术补资源。映射规则（收藏品经 `Resources` 表）已明确，**不再需要负责人提供**。
- 另：`ClientRankPage` 的 6 张 `VisualBinding[]` 是**按字符串 id 绑定**的 Inspector 表，与「收藏品 id 是 int」存在形态差
  （当前由 `entry.AvatarId.ToString()` 临时桥接）。

### BUG-024：MailPage 的「邮件正文 / 领取状态」在场景里**没有文本节点**（设计缺口，需负责人定方案）

- 状态：开放（**已静态确证**；需负责人在三个方案中选一个）
- 类型：交互稿缺页 / 设计缺口
- 影响范围：`MailPage邮件` 的**详情展示**。`ClientMailPage.SelectMail()` 会写 `_detail`（标题+正文+附件）与 `_status`（"附件已领取"/"点击领取附件"），
  但两者在场景里都是 `fileID 0`，**且场景内没有任何可接的目标节点** ⇒ 选中邮件后**看不到正文，也看不到领取状态**。
- 证据与复现步骤（2026-09-20 取证）：
  · `ClientMailPage` 组件实例 `1008869757`：`_detail: 0`、`_status: 0`、`_emptyHint: 0`、`_mailLabels: []`；
  · 全页 **TMP 文本节点穷举**只有列表项内的：`邮件Item/邮件名称底框/邮件名称`、`邮件Item/日期text`、以及各 `道具卡牌prefab*/道具名称|数量角标/道具数量`；
  · `Email Title` 整棵子树（`Email Title/Canvas/Emial title background frame/Email text`）**没有任何文本组件** —— 名字叫 `Email text` 的节点其实是 **`Image`**；
  · 全页**没有**任何语义为"状态/领取"的节点。
- ⚠️ **同时更正三条过时记录（本页）**：
  1. ~~`_emptyHint` 未绑定~~ ⇒ **不是缺口**：`ClientMailPage.ResolveMissingReferences()` 在**运行时**用 `_pageRoot.transform.Find("Default")` 自补 ✓；
  2. ~~`_mailLabels` 未绑定是缺陷~~ ⇒ 旧实现的 2 槽数组**已被"1 模板 + 运行时生成"取代**，代码注释已说明，属历史字段（保留序列化数据）；
  3. ~~邮件卡没有点击绑定~~ ⇒ `ApplyMailCardsFromTemplate()` 已给克隆体绑 `SelectMail(index)` ✓。
- 暂定处理或规避：**不擅自新建 UI 节点**（`AGENTS.md` §4）。三个可选方案：
  · **A** 允许在 `Email Title/Canvas/Emial title background frame` 下新增/改造一个 TMP 文本作正文区（最小改动，符合现有面板）；
  · **B** 正文改用**弹窗**（场景已有 `Email Details Page（Have）/(No)`，已迁入 `PopupLayer`，但同样没有打开逻辑）—— 需要确认进入条件与内容；
  · **C** 维持现状：正文不显示，列表内只展示"标题 + 日期 + 附件"。
- 关闭条件：负责人选定方案后落地，选中邮件能看到正文与领取状态（或明确记录为不显示）。

### BUG-025：构建场景清单不含 `ClientShell` ⇒ WebGL/小游戏包**进不去客户端 UI**（B0 门禁硬阻断）

- 状态：**已按负责人选定的方案 C 落地（2026-09-21）** —— 新增客户端专用启动场景 `Assets/Client/Scenes/ClientBoot.unity`（对象 `ClientBoot` + `ClientBootLoader`，只在 `Start()` 里加载 `ClientShell`），
  并**置首**于 `EditorBuildSettings`：实测构建顺序为 **`ClientBoot → Boot → ClientShell`**（原 `Boot.unity` **保留未动**，可随时调整顺序回滚）。
  工具方法 `EnsureClientBootScene()` 幂等；证据 `Logs/repair-clientboot.log`（`构建场景顺序（前 5 个）：ClientBoot → Boot → ClientShell`）与 `ProjectSettings/EditorBuildSettings.asset`。
- 类型：阻断（阶段 B 前置）
- 影响范围：**全部客户端页面的可移植性**。按现状构建 WebGL，包内**不含客户端场景**，运行后停在原公司 `Boot` 流程，永远到不了客户端 UI。
- 证据与复现步骤（2026-09-20 取证）：
  · `ProjectSettings/EditorBuildSettings.asset` **只有一个场景**：`Assets/Main/Boot.unity`（`enabled: 1`）；
  · 全工程 `Assets/**/*.cs` 中**没有任何 `LoadScene` / `SceneManager` 加载 `ClientShell`** —— 只有编辑器工具引用其**路径常量**（`ClientHierarchyAudit.cs`、`ClientShellStructuralRepair.cs`）；
  · `Assets/Main/Boot.unity`（32.4 KB）中 `ClientServices` / `ClientShell` / `ClientBootstrap` / `ClientLoadingPage` 出现次数**均为 0**。
- 三个可选方案（代理**不擅自**改构建清单或原流程；`AGENTS.md` §1/§3 对改动既有流程有约束）：
  · **A** 把 `Assets/Client/Scenes/ClientShell.unity` 加入 Build Settings 并作为**启动场景（index 0）** —— 直接进客户端，但改变现有构建入口；
  · **B** 保留 `Boot`，在其流程后追加 `SceneManager.LoadScene("ClientShell")` —— 要改原公司 Boot 流程，需单独授权；
  · **C** 新建一个只做客户端初始化的 `ClientBoot` 场景，加进清单并置首，`Boot` 完全不动 —— 最符合"客户端新功能从 `Assets/Client` 独立场景开始"的约束，代价是需要新建场景（场景写入需授权）。
- 完整门禁记录见 **`WEBGL_B0_GATE.md`**（含工具链版本、WebGL 暴露面、兼容矩阵、首包、真机清单与待办）。
- 关闭条件：选定方案并落地后，WebGL 构建产物能进入客户端 `ClientShell`（以构建日志/运行截图或日志为证）。
- **状态更新（2026-09-22）：已解决，可关闭。**
  最终落地方式**不同于** 09-21 记录的「方案 C（`ClientBoot` 置首）」：经负责人 2026-09-22 逐次授权后改为
  ① `ProjectSettings/EditorBuildSettings.asset` = `Boot(0) → ClientShell(1)`（`ClientBoot` 退出启动链）；
  ② `Assets/Main/Init.cs` 新增 `ClientStartScene`，并在目标场景**属于构建清单**时改用 `SceneManager.LoadSceneAsync` 原生加载
     （内置场景不在 YooAsset 收集器范围内，按地址加载必然 `Failed to mapping location to asset path`）；
  ③ 追加修复「`Single` 模式销毁 `Init` 协程 ⇒ 加载后代码不执行」——新增跨场景宿主 `Assets/Main/WebGLPostSceneLoadRunner.cs`。
  证据：构建日志 `[Builder] Scenes [1]: Assets/Client/Scenes/ClientShell.unity, [x]`；
  容器 Console：`首个场景已加载（sceneLoaded 事件）：ClientShell` + `ActiveVisualCanvases 含 ClientCanvas/PagesLayer/MainPage主页`；
  **真机**首页正常渲染 + TCP 六包齐全（`WEBGL_B2_REALDEVICE.md` §八）。
  详细根因与方案对比 → `HOME_PAGE_SCENE_FIX_OPTIONS.md`（§三 方案对比、§九 实施与复验记录）；总入口 → `WEBGL_MINIGAME_HANDBOOK.md`。

### BUG-026：UI 契约审计结果 —— 未接线字段与缺失节点清单（2026-09-21 全量扫描）

- 状态：**6 项全部处置完毕并经负责人验收通过（2026-09-21）** —— 见下方"BUG-026 处置结果"；仅第 5 项（个性化「确定键」语义）保留待确认。
- 类型：缺陷（UI 显示/交互缺口）+ 一处潜伏崩溃（已修）
- 审计方法：`Logs/ui-contract-audit.ps1`（纯 ASCII 脚本，可重跑）→ 报告 `Logs/ui-contract-audit.md`。
  两条判据：**A** 代码里"按名查找"的字面量在**场景 + 全部 prefab（920 个名字）**里都不存在；**B** 场景里客户端组件的序列化字段为 `fileID 0`。
- **A 类（节点名缺口）真命中 1 处**，其余为假阳性（编辑器工具里的路径片段、已兜底的 `前往获取*`、前缀字面量）：
  · **排行榜阵容卡**：`ClientRankPage` 找 `展示卡牌*` 及其子节点 `卡牌名称` / `LV等级` / `星级图标Image (1..5)` ——
    **在场景与全部 prefab 中 0 命中** ⇒ "查看阵容"弹窗**只有 层数/回合数/战力，没有卡牌**。
    **已核实不会崩**：`OpenLineup` 按 `_lineupCards.Count` 循环，列表为空即空转 ✓（`Logs/` 审计 + 代码复核）。
- **B 类（未接线字段）40 条，分类后真缺口 5 项 + 潜伏崩溃 1 项（已修）**：

  | # | 字段 / 组件 | 影响 | 性质 |
  |---|---|---|---|
  | 1 | `ClientPopupService._maskRoot` | 弹窗**没有遮罩与输入阻断**（交互稿要求"可叠加、可关闭、带遮罩/阻断语义"） | 真缺口 · 需指定**现成**节点 |
  | 2 | `ClientHomeRedDotController._mailDot` / `_noticeDot` | 大厅**邮件 / 公告红点不显示**（`_activityDot` 已接线 ✓） | 真缺口 · 需指定现成节点 |
  | 3 | `ClientHomePage._notice` / `ClientProfilePage._notice` | 页面提示文本**静默不显示**（`ClientHomePage.cs:217` 已有取证注释） | 真缺口（轻微）· 需指定现成文本节点 |
  | 4 | `BottomFunctionIconView._icon` / `_label`（`Bottom1`..`Bottom4`） | 底栏 4 个入口的图标/文字无法被 `Configure` 写入（目前**无调用方** ⇒ 无可见影响） | 真缺口（潜在）· 需指定节点 |
  | 5 | 排行榜**阵容卡**（见 A 类） | 查看阵容无卡牌 | 真缺口 · **需新建节点**或改为不显示 |
  | 6 | `BottomFunctionIconView.Configure` **未判空** | 字段未接线时一旦被调用即 **NullReferenceException** | ✅ **已修**（逐项判空，语义不变） |

- **判定为"非缺口"（有运行时兜底或本就可选，无需处理）**：
  · `ClientUiPopup._popupRoot` ×18 ⇒ `Root` 属性回退 `gameObject`（`ClientUiPopup.cs:53/75/84`）；
  · `ClientShellController._popupService` / `_feedback` ⇒ `GetComponentInChildren` 兜底（`:65-68`）；
  · `ClientHeroPage._heroTabSelected` / `_guideTabSelected` ⇒ 代码注释明确为**可选**节点（`:310`）；
  · `ClientHeroCard._teamBadgeNumber` ×7 ⇒ 仅编队页使用；
  · `ClientSystemFeedback._toastText` ⇒ 旧版 `Text` 遗留字段，TMP 的 `_toastLabel` 已接线；
  · `ClientMailPage._emptyHint` ⇒ `ResolveMissingReferences()` 运行时自补；
  · `ClientShopPurchaseQuantityProgress._minimumText/_maximumText` ⇒ `BUG-020` 已定为**刻意留空**（设计无对应节点）；
  · `ClientRankPage._avatars[]/_frames[]/_badges[]/_attributes[]` ⇒ `BUG-019` 已定为**缺美术资源**。
- 暂定处理或规避：**不擅自写场景**（`AGENTS.md` §4）—— 第 1~5 项**列出待授权清单**，负责人确认后由结构修复工具接线（全部为幂等步骤）。
- 关闭条件：第 1~5 项各自落地或由负责人明确记为"本阶段不做"；审计脚本重跑后 A 类只剩假阳性。

### BUG-027：负责人首轮验收反馈（2026-09-21）—— 3 项逻辑/功能问题已修 + 1 项解释

- 状态：**3 项已修并经负责人验收通过（客服弹窗 ✅ / 邮箱全部删除 ✅ / 排行榜 50 名 ✅）**；滚动列表根因修复后又修正了一次副作用（整列右移），**排行榜最终验收通过**；个性化"删改"为说明；UI 错位负责人明确后续专门改。
- 来源：负责人首轮人工验收（"大部分逻辑和功能实现都没有问题"）。

#### ① 客服会话界面：点"确定键"或空白处都返回不了上一级 ✅ 已修

- **根因（两条）**：
  1. **遮罩只被显隐、没有任何点击处理** ⇒ 点空白处毫无反应 —— 这是**所有弹窗的共同问题**，不只客服页；
  2. 客服弹窗内**没有任何关闭键绑定**（`BindContactServiceButton` 只绑了"打开"）。
- **修法（纯代码，不写场景）**：
  - `ClientPopupService.EnsureMaskClickClosesTop()`：运行时给遮罩补 `Button`（`transition = None`，不改尺寸/排版），点击 → `CloseTop()` ⇒ **所有弹窗**都支持"点空白返回上一级"；
  - `ClientHomePage.BindContactPopupClose()`：按常见命名（`确定键Button`/`确定Button`/`确定`/`关闭键Button`/`关闭Button`/`退出Button`）在客服弹窗内找关闭键并绑定；**找不到只记一行日志**；另把 `background`/`底框` 绑定为"点空白关闭"作为兜底（覆盖"不在弹窗栈上"的情况）。

#### ② 排行榜只显示前 5 名，要求前 50 名 ✅ 已修

- **根因**：模拟数据**每个榜只有 5 条**（`CreateLeaderboardEntry(1..5)`）⇒ 界面自然只有 5 行。
- **修法**：
  - `LocalClientDataService.BuildLeaderboard(50, …)`：前 5 名保留原样例，其余**按确定性规则**补足到 **50 条**（**不用随机数**，保证每次运行/每台机器一致，便于验收与自检）；
  - `ClientRankPage`：新增"**行容器 + 首行模板**"，`EnsureRankRows()` 按数据条数在运行时克隆补足，上限 `MaxLeaderboardRows = 50`（**不写场景**，克隆体与原行同层同排版，由容器布局组件排布）。

#### ③ 邮件"全部删除" ✅ 已修（按负责人给出的规则）

- **规则（负责人原话）**："全部领取和已读后点击可以全部删除，然后背景显示暂无邮件"。
- **修法**：
  - 新增 `IClientDataService.TryDeleteAllMails(out string failureReason)`：**逐封校验**"已读 + 已领取"，任一不满足即**拒绝并给出原因**（"还有未读邮件，请先全部已读" / "还有附件未领取，请先全部领取"）；通过则清空列表并 `DataChanged`；
  - `ClientMailPage.DeleteAll()` + 绑定：**场景里的删除键挂在 `Panel/All delete/Button` 下，而按钮节点本身叫 `Button`**（名字太通用）⇒ 按**父节点 `All delete`** 查找再取子 Button（`BindButtonUnder`），避免误绑；
  - 删除成功后 `Refresh()` 走既有空态分支 ⇒ `Default/No emails` + 文本"暂无邮件" ✓（场景**本来就有** `MailPage邮件/Default/No emails` 节点）。

#### ④ 说明：个性化页签的 5 个子界面"被删改"是什么情况（**非缺陷，是我做的列表收敛**）

- **当时做了什么**（证据 `Logs/repair-profilelists.log`，共 53 处变更）：
  - `删除内联项 头像未解锁Image / 头像已解锁Image (2..5)` ⇒ 场景里**预先摆好的演示项被删掉**；
  - `列表模板重命名：头像已解锁Image -> 头像Item` + **`列表模板已禁用（仅作克隆源，永不显示）`**；
  - `ClientProfilePage._avatarTemplate <- 头像Item，_avatarRoot <- Content（改走模板生成模式）`；
  - 徽章 / 铭牌 / 称号 / 头像框 同样处理（每个列表"1 个模板 + 运行时按数据生成"）。
- **为什么**：场景里每个列表固定摆 5 个**演示条目**，而配置表按 `ClientShow` 过滤后的**可见条数是 1/1/1/3/1** ⇒ 不收敛就会出现"数据只有 1 条、界面却显示 5 个假条目"✗；收敛后**界面严格等于数据** ✓。
- **代价（你现在看到的）**：那 5 个"默认效果演示"不见了，列表看起来"空/少"。
- **三个可选方向（等你定）**：
  - **A 保持现状**（数据驱动，界面=数据；想让内容多就补数据/表）
  - **B 想看更丰富的演示**：把 `Item`/`Head`/`HeadFrame`/`Badge`/`Nameplate`/`Title` 表里对应行的 `ClientShow` 打开更多行，或给模拟数据多几个"已拥有" ⇒ **界面自动变多**（推荐：既不写死场景、又能演示）
  - **C 恢复内联演示项**（不推荐：会与数据条数冲突，又回到"假条目"）

#### ⑤ UI 错位 / 不适配（负责人明确：**意料之中，后续专门改，现在不用管**）

- 已记录为**暂缓项**，本轮**未做任何 UI 排版改动**。

- **验证**：编译三目标 **0 error**（`Logs/verify-compile-*.log`）。
  ⚠️ **自检未在本轮重跑**：负责人 Unity 编辑器处于打开状态，跑批处理必须先关掉它 ⇒ 为避免打断验收，本轮**未执行**；待其关闭后补跑 `ClientSelfTest.RunAll`。

#### ⑥ 排行榜/编队 滚动列表"只显示 6 条、松手弹回、默认不在最上面" ✅ 已修（根因是场景配置，不是逻辑）

- **负责人复验反馈**：排行榜仍不行 —— 只显示 6 个、往下滑松手被强制拉回、默认不显示最上面；**编队的 Scroll View 同样**；
  负责人推测"估计是 middle center 导致的" —— **推测正确**，而且还有两条叠加原因。
- **场景实测根因（三条叠加）**：
  1. `Content` 的 **`ContentSizeFitter.m_VerticalFit: 0`（Unconstrained）** ⇒ **内容从不撑高**，只有作者高度可见 ⇒
     "只显示 6 条"，并且**内容高度 ≈ 视口高度 ⇒ 滚动范围为 0 ⇒ 一滑就回弹**；
  2. `ScrollRect.` **`m_MovementType: 1`（Elastic）** ⇒ 松手**弹回**（`0=Unrestricted / 1=Elastic / 2=Clamped`）；
  3. `GridLayoutGroup.` **`m_ChildAlignment: 4`（MiddleCenter）** ⇒ 内容**垂直居中**，所以默认看到的是中间而非顶部。
  - 参考实测值：排行榜 `Content` 的网格 `CellSize 673×191`、`Spacing (0,13)`、`Constraint 0`（Flexible）、`ConstraintCount 1`；
    编队 `CellSize 204×326`、`Spacing (20.9,37)`、`Constraint 1`（FixedColumnCount）、`ConstraintCount 3`。
- **修法（运行时、纯代码，不写场景、不动美术尺寸）**：新增 **`Assets/Client/Runtime/UI/ClientScrollFix.cs`**：
  · `ContentSizeFitter.verticalFit = PreferredSize`（让容器按内容撑高）；
  · `ScrollRect.movementType = Clamped`（松手不弹回）；
  · `GridLayoutGroup/VerticalLayoutGroup.childAlignment = UpperCenter` + 容器 `anchorMin(0,1)/anchorMax(1,1)/pivot(0.5,1)`（顶部对齐）；
  · `LayoutRebuilder.ForceRebuildLayoutImmediate` 后 `verticalNormalizedPosition = 1`（回到最上面）。
  **只处理竖向列表**（`vertical && !horizontal`），横向与二维滚动不动。
- **已接入**：`ClientRankPage.Show()`（两个榜）、`ClientFormationPage.Show()`、`ClientProfilePage.Show()`（收藏品列表）。
- **未接入（等你一句话）**：英雄 / 商店 / 邮箱 / 活动 等其余列表页 —— 它们大概率有**同样**的配置问题；
  修复工具已写好，接入是一行调用，但为避免在你复验期间引入额外变化，我**先不动**。

##### ⑥ 追加（第 83 轮）：**修复引入的副作用——排行榜整列被推到右半边** ✅ 已改

- **负责人复验**：编队/个人中心等**其它都通过**，但**排行榜所有行整体右移、右边缘被裁掉、左半边空着**（附截图证据）。
- **根因（是我的第一版修复引入的）**：第一版把容器的 **水平**也改成拉伸（`anchorMin.x = 0` / `anchorMax.x = 1`）⇒
  容器宽度 = "视口宽 + 作者设定的 `sizeDelta.x`"，**比原本宽很多** ⇒ 网格按居中排布后**整列右移并被 Viewport 裁切**。
- **修法（第 83 轮）**：`ClientScrollFix` 改为**只调整竖直方向**：
  · `anchorMin = (作者的 x, 1)`、`anchorMax = (作者的 x, 1)`、`pivot = (作者的 x, 1)` ⇒ **完全保留水平几何**；
  · 对齐只改**竖直分量**，水平分量保留：`TextAnchor` 编码为 `0..2 = Upper{Left,Center,Right}`、`3..5 = Middle…`、`6..8 = Lower…`，
    故 `(int)childAlignment % 3` 即水平分量，目标值取该水平分量（即"作者的 Left/Center/Right + 强制 Upper"）。
- **验证**：编译三目标 **0 error** ✓；**视觉结果需负责人 Play 复验**（我无法截图，也不能代替 GUI 验证）。
- **教训**：修"滚动/撑高"这类布局问题时，**只应改与问题相关的轴**；我第一版图省事用了"水平拉伸"的通用写法，
  在"作者已把容器宽度调到与子项一致"的页面上就变成了副作用。**最小改动优先于看起来更标准的写法。**

#### 待核验清理项（2026-09-21 第 78 轮登记，**等负责人验收后再处理**）

| 候选 | 判断依据 | 影响范围 | 回滚方式 |
|---|---|---|---|
| `ClientCanvas/PopupLayer/Player lineup/玩家阵容/Content/展示卡牌1..3`（**本轮我新建的 3 个 prefab 实例**） | 场景里**本来就有 6 张 `角色卡牌Button-final`**（既有列表）；代码现已**两种名字都收** ⇒ 我新建的 3 张与既有列表**功能重复**，且当前数据只有 3 条时它们会一直处于隐藏态 | 仅视觉冗余，无功能影响 | 删除 3 个节点即可；写入前备份 `Logs/ClientShell.before-bug026b.unity.bak`；删除后重跑自检应仍为全绿（断言为"至少 1 张卡"） |
**处理前置条件（负责人确认）**：验收清单 **E2/E4** 通过（即阵容弹窗能正常显示卡牌、且不出现整片空白）⇒ 说明既有 6 张已足够，可安全清理我新建的 3 张。
**若 E2/E4 不通过**，则优先排查既有列表（卡 prefab 的 `等级-Text` 回退、贴图表），暂不清理。

#### BUG-026 处置结果（2026-09-21 第 19 轮，负责人 **"全部授权" + "允许在合适的位置新建"**）

| # | 项 | 处置 | 证据 |
|---|---|---|---|
| ① | 弹窗遮罩 / 输入阻断 | ✅ **已修** | 工具里的 `EnsurePopupMask()` **早已写好却从未被调用**（场景里因此一直没有遮罩节点）⇒ 本轮补上调用；新建 `ClientCanvas/PopupLayer/弹窗遮罩`（全屏 + `raycastTarget`，默认隐藏），并接线 `ClientPopupService._maskRoot` ✓ |
| ② | 邮件红点 | ✅ **已修** | 克隆现成的 `ActivityButton/RedDot` → `CanvasBottomFunction0/BottomFunctionIcon_Mail/RedDot`（默认隐藏），接线 `ClientHomeRedDotController._mailDot` ✓ |
| ③ | 页面提示文本 | ✅ **已修（两种情形）** | · `ClientHomePage` **本来就已把提示发给全局 Toast**（`ShowNotice` → `_feedback.ShowToast`）⇒ 我此前把它列为"真缺口"是**误报**，已更正；<br>· `ClientProfilePage` **确实没有** → 本轮让它实现 `IClientFeedbackHost` + `BindFeedback`，`ShowNotice` **优先走全局 Toast**（`_notice` 保留为兼容回退）✓ |
| ④ | 底栏图标 / 文字 | ✅ **已接（一半）** | `BottomFunctionIconView._icon` 已接（`Bottom1..4/Image`）；**`_label` 无对应节点**（`Bottom1..4` 只有一个 `Image` 子节点、没有文字），且 `Configure` 目前无调用方 ⇒ 如实记录，**不臆造文字节点** |
| ⑤ | 排行榜阵容卡 | ✅ **已建** | 在 `Player lineup/玩家阵容` 下新建 `Content`（GridLayoutGroup 3 列）+ **3 个共用卡 prefab 实例**命名 `展示卡牌1..3`（保留 prefab 关联）；该 prefab **已含** `卡牌名称`/`等级-Text`/`立绘Image`/`品质Image`/`品质底框Image`/`属性图标Image`/`星级图标Image (1..5)` ⇒ 正好覆盖 `LineupCardRow` 所需 ✓ |
| ⑥ | 阵容弹窗关闭手段 | ✅ **已建** | `Player lineup` 右上角新建 `关闭Button`（克隆自排行榜返回按钮），`ClientRankPage.OpenLineup()` 绑定 `CloseLineup`（找不到时回退 `BackoffButton`）✓ |

**同时更正我自己的两处审计误报**：
1. ~~`星级图标Image (1..5)` 在工程中不存在~~ ⇒ **错**：共用卡 prefab **本来就有这 5 个节点**；审计误报的原因是代码里写的是**前缀字面量** `"星级图标Image (" + i + ")"`，而我的比对当时只做全名相等。
   **已修审计判据**：字面量只要是任一节点名的**前缀**即算命中 ⇒ A 类清单随即变干净。
2. ~~`ClientHomePage._notice` 未接线是真缺口~~ ⇒ **错**：该页 `ShowNotice` 早已走全局 Toast（见上表 ③）。

**最终审计状态（第 19 轮重跑）**：
- **A 类**：只剩①编辑器工具里的字符串/路径片段（假阳性）②`前往获取Button`/`前往获取Canvas`（代码**已有兜底**并注明）③`LV等级`（**已加 `等级-Text` 回退**，见 `ClientRankPage.LineupCardRow`）⇒ **无未处理真缺口** ✓
- **B 类 39 条**：均为**已解释的非缺口**（`ClientUiPopup._popupRoot` 回退 `gameObject`、`_teamBadgeNumber` 仅编队用、`_label` 无节点、`_notice` 兼容回退、`_noticeDot` **公告页在场景里没有入口**、`BUG-019` 四张表缺美术、`BUG-020` 两个文本字段设计上就没有）。
- **代码侧健壮性**：`BottomFunctionIconView.Configure` 未判空 → **已加逐项判空**（语义不变）；`ClientRankPage` 等级名加回退；`ClientProfilePage` 接入反馈通道。

**验证**：编译三目标 **0 error**；`Logs/repair-bug026a.log`（37 处变更）与 `Logs/repair-bug026b.log`（43 处变更）均 `error CS 0` / 异常 0；自检 **70 项 / 失败 0**；场景 YAML 复核 `_maskRoot`/`_mailDot`/`_icon` 非 0、`弹窗遮罩`（`active=false`）、`BottomFunctionIcon_Mail/RedDot`（`active=false`）、`玩家阵容/Content`、`关闭Button` 均存在 ✓。
**备份**：`Logs/ClientShell.before-bug026a.unity.bak`、`Logs/ClientShell.before-bug026b.unity.bak`。

#### BUG-026 补充（2026-09-21 第 77 轮）： **死按钮审计**（点击无反应的按钮）

新增 `Logs/ui-dead-buttons.ps1`（纯 ASCII，可重跑）→ 报告 `Logs/ui-dead-buttons.md`。
判据：场景里的 `Button` **既没有场景持久事件**（`m_MethodName`）**其名字也没有出现在任何运行时代码的绑定/查找行里**。

**实测**：场景内共 **169 个按钮**，其中 **0 个**走场景持久事件（全场景只有 3 个 `SetActive` 持久调用，且**不在按钮上**——按钮一律由代码绑定，与本工程架构一致）；
首轮候选 **67 个**，复核后确认 **5 处真缺口**（处理函数早就有，只是没人绑定）：

| # | 按钮 | 问题 | 处置 |
|---|---|---|---|
| 1 | `ProfilePage个人中心/PageContentRoot/Panel_Profile/底框/改名称` | **改名入口点不开** | ✅ 已修（代码绑定 `OpenRenameDialog`） |
| 2 | `…/Panel_Profile/RenameDialog/ConfirmButton` | **确认改名没反应** | ✅ 已修（绑定 `ConfirmRename`） |
| 3 | `…/Panel_Profile/RenameDialog/CancelButton` | **取消没反应** | ✅ 已修（绑定 `CancelRename`） |
| 4 | `MailPage邮件/Email come/Panel/All read/All read button` | **一键领取附件没反应** | ✅ 已修（绑定 `ClaimAll`） |
| 5 | `…/SubContentRoot/Content_*/默认*Image/个性化确定键Button` ×6 | 个性化页各分类的"确定"按钮无绑定 | ⏸ **保留待确认**：本实现是"**点条目即装备**"，该"确定键"是否需要（以及取消/回滚语义）属**产品规则**，**不臆造** |

**同时修掉一个静默失败的根因**：`ClientProfilePage.BindButton` 原先用 `transform.Find`（**只查直接子级**），
而 `改名称` 在 `Panel_Profile/底框/` 下、确认/取消在 `Panel_Profile/RenameDialog/` 下 ⇒ **一个都找不到且不报错**。
已改为**深搜**（`FindDeep`），与其它页面一致 ✓

**未实现而非缺陷（不臆造行为，仅记录）**：
- `MailPage` 的 `All delete`（全部删除）：`IClientDataService` **没有**任何删除/批量已读接口（只有 `ClaimAllMails`）⇒ 属未实现功能；
- 元素筛选按钮（`HeroPage`/`FormationPage` 的 `光/水/土/风/火/暗Button`）：负责人此前明确**本阶段不做**；
- `Email Details Page（Have）/(No)`、`Success Receipt Interface` 等**未接入页面**内的按钮：页面本身就没有运行时代码。

**审计工具已知假阳性来源（如实记录）**：本判据**看不到"用序列化字段接的按钮"**
（如 `ClientHeroDetailPage` 的 `_leftButton`/`_rightButton`/`_equipmentButtons`、`ClientHeroPage` 的 `_heroTabButton`/`_guideTabButton`、
`ClientShopPurchaseQuantityProgress` 的 `_addButton`/`_subtractButton`/`_maxButton`）⇒ 这些按钮会**误报**为候选，需人工复核。

**验证**：编译三目标 **0 error**；修复后重跑审计，上述 4 个按钮**已从候选中消失**（候选 67 → **63**）；自检 **70 项 / 失败 0**、退出码 0。

#### BUG-026 补充（2026-09-21 第 18 轮）：页面/弹窗「返回·关闭闭环」核对

新增 `Logs/ui-flow-matrix.ps1`（**纯 ASCII**；中文关键词用**字符码**构造，避免 `.ps1` 被按 ANSI 读坏 —— 本轮我在这里又踩了一次坑）
→ 报告 `Logs/ui-flow-matrix.md`（19 个已登记场景节点的 pageId / 代码引用数 / 是否有返回·关闭节点）。

**核对结论（返回闭环基本完整 ✓）**：各页返回**都绑定在 `BackoffButton`** 上（**不是** `后退Button`），且场景里各页都有该节点：

| 页面 | 返回绑定（代码证据） |
|---|---|
| 排行榜 | `ClientRankPage:80 BindButton("BackoffButton", Back)` |
| 活动页 | `ClientActivityPage:298 GetButton(FindDirectChild(bottomBar,"BackoffButton"))`（赛季子页另有 `:302`） |
| 商店 | `ClientShopPage:401 BindShopHomeBack()` → `Bottom page function bar/BackoffButton`；各子弹窗另有绑定 |
| 全图鉴活动 | `ClientCompleteGuideEventPage:345-347 FindDirectChild(bottomBar,"BackoffButton")` → `Back()` |
| 英雄/英雄详情/编队/邮件/个人中心 | 走 `后退Button`（工具 `BackButtonPaths` 5 条）✓ |

**🔴 新发现缺口（并入本缺陷第 6 项）**：
- **`Player lineup`（玩家阵容）子树里没有任何按钮** ⇒ **打开后没有关闭手段**（与"阵容卡节点不存在"同源，该弹窗整体未完成）。

**审计工具已知局限（如实记录，避免误读"back=False"）**：
- 判据只认 `后退`/`关闭`/`Backoff` 等关键词，**不覆盖 `取消键Button` 这类业务命名** ⇒ 例如 `Product Purchase Interface` 实际由 `取消键Button` 关闭（`ClientShopPage:285`），矩阵里显示 `back=False` 属**判据盲区**而非缺陷；
- **prefab 实例内部**的节点不在场景 YAML 中 ⇒ 实例内的关闭按钮可能被漏判。

---

## 2026-09-22 移植侧新增条目

### BUG-028：真机（未开调试）资源下载被微信域名校验拦截 —— `downloadFile:fail url not in domain list`

- 状态：**已定性**；两条路径中「开调试」已实测有效，「配合法域名」待验证
- 类型：平台接入前置（**非客户端代码缺陷**）
- 影响范围：**全部真机运行**（开发版预览）。拦在资源下载阶段 ⇒ 连 Loading 都过不去，更到不了登录与 TCP
- 证据与复现步骤（2026-09-22 真机 A/B 对照，同一构建 `DATA_FILE_MD5=356e1b81e1be0bb4`、同一网络）：
  · 未开调试 ⇒ 弹窗「**资源下载失败 / `downloadFile:fail url not in domain list`**」；
  · **仅把手机「调试」打开**后重进 ⇒ 资源下载通过、进入首页、TCP 六包齐全；
  · 局域网因素已排除：手机浏览器实测 `http://<服务器地址>:8000/router/rest` 有响应
    （`<error_response><code>2</code><msg>请求无效</msg></error_response>`，缺参数导致的正常报错）。
- 暂定处理或规避：① 真机测试期**开「调试」**；② 公众平台 → 开发管理 → 开发设置 → 服务器域名，
  把 `https://<CDN域名>` 加入 **`downloadFile` 合法域名**（建议同时加 `request`）
- 关闭条件：**体验版/正式版在未开调试的真机上**能正常下载资源并进入首页

### RISK-021：正式环境需服务端提供 HTTPS + 已备案域名（裸 IP 无法加入合法域名白名单）

- 状态：待负责人 / 服务端决策
- 类型：风险（**上线前置**）
- 影响范围：正式版/体验版的**整条网络链**。网关发现服务 `http://<服务器地址>:8000/router/rest` 与
  TCP 网关 `<服务器地址>:53413` 均为**裸 IP + HTTP**
- 证据与复现步骤：地址来源可查 —— `Assets/Scripts/WebWork/Protocol/Core/DefaultQLClient.cs:35`（默认 `ServerUrl` 即内网 IP）、
  `Assets/Scripts/GameLogic/NetController.cs:50-56`（仅 `ZoneId < 0` 才切公网 `<服务器地址>:8000`）、
  `Assets/Scripts/Frame/Defines/SysDefines.cs:452`（`ZoneId = 1` ⇒ 走内网）；本机实测 `<服务器地址>:8000/53413` 可达、
  公网备用环境不可达。微信「合法域名」不接受 IP 且要求 HTTPS —— **官方条文未取证**，仅依据真机报错行为与通行规则
- 暂定处理或规避：由服务端在测试/正式环境下发**手机可达的 HTTPS 域名**地址，并在公众平台配置 `request` / `socket` 合法域名；
  代理**不得**修改客户端 IP/端口/服务端（`TCP_WEBGL_HANDOFF.md` 硬约束）
- 关闭条件：体验版在未开调试的真机上能完成 TCP 六包

### 待核验：`UnityBridgeManager` 脚本在运行时缺失（旧街机残留）

- 状态：待核验
- 类型：风险 / 待核验项
- 影响范围：`Boot.unity` 中某 Behaviour 引用的 `UnityBridgeManager` 在运行时缺失
  （Console：`The referenced script (UnityBridgeManager) on this Behaviour is missing!`）
- 证据与复现步骤：容器与真机 Console 均出现该警告；首页与业务属 `Assets/Client` 模块，**与首页表现无关**
- 暂定处理或规避：**不删**；先确认它是否属原街机流程遗留、是否仍在启动链上
- 关闭条件：确认归属并记录（或按授权移除引用）
