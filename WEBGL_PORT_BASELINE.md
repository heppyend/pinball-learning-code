# WebGL / 微信小游戏移植：只读基线盘点（A0）

> 建立时间：2026-09-20 18:2x（本会话，接手「弹珠项目」WebGL 移植与真机测试）
> 性质：**只读盘点**。本文件建立过程中**未修改任何代码、场景、资源、ProjectSettings 或构建产物**，
> 未清理、未回滚、未覆盖工作区既有未提交改动。
> 治理归属：本文件是移植事务（`HANDOVER_PLAN.md` 阶段 B）的**证据底稿**；
> 当前状态短入口仍是 `CURRENT_STATE.md`，缺陷归属仍是 `BUG_TRACKER.md`。

---

## 0.1 负责人决定（2026-09-20，盘点后当场拍定）

| # | 决定 | 含义 / 对计划的影响 |
|---|---|---|
| D1 | **入口方案选 C：先只做移植链路，不动入口** | `Boot → LoginScene` 保持原样，`Init.cs` **不改**，`ClientShell` **不进 Build Settings**。本会话目标收窄为：把 WebGL Build → 微信转换 → 上传 → 开发者工具 → 真机这条**链路本身**跑稳并建立日志/性能基线。§7 步骤 2 的 A/B 候选**作废**；P0-1（`ClientShell` 无入口）**降级为"已知事实、暂不处理"**，不再是本会话阻塞项。 |
| D2 | **TCP 成功的环境**：**Unity Play 模式**跑通过，**微信开发者工具**也跑通过 | "TCP 链路在 WebGL/微信容器内可用" 已有证据；但**真机（Android/iOS）尚未验证过**。⇒ §6 里"H2 真机未验证"保持为独立风险；`HANDOVER_PLAN.md` §5 B2 的真机矩阵仍必须执行。 |
| D3 | **先等负责人清完 `BUG-009` / `BUG-013`，再动手** | 本会话在接到"可以继续"之前**不写任何代码/场景/资源**；只允许只读取证与文档。避免与并行会话争抢工程写入。 |

> D1 使 P0-2（热更 DLL 落后）成为**当前唯一的 P0 层级技术风险**：既然只做链路，链路里跑的就是 `HotFix.dll`，它必须是新的。
> D3 意味着 `§8` 表格里的动作**现在都不执行**，等负责人放行。

---

## 0.2 第二轮盘点（2026-09-22 12:09–12:15）：三个决定已被现实改变

> 触发：负责人下令「开始进行移植」。**开工前重做冻结点**（`Logs/webgl-port/baseline-20260922-120956.txt`），
> 结果发现 **D1 已被另一个会话推翻、D3 已被负责人解除**。以下为本轮实测新增/更正的事实。

### ⚠️ 更正一：`ClientShell` 的 WebGL 入口**已经存在**（我的 09-20 P0-1 已过期）

另一个会话在 **2026-09-21** 按 `BUG-025` 的**方案 C** 落地了：

| 物 | 值 | 证据 |
|---|---|---|
| `Assets/Client/Scenes/ClientBoot.unity` | 4.8 KB，场景根对象 `ClientBoot` + `ClientBootLoader` | 场景 YAML |
| `Assets/Client/Runtime/ClientBootLoader.cs` | `Start()` → `SceneManager.LoadScene("ClientShell")`，`_clientSceneName: ClientShell` | 源码 + 场景序列化 |
| `ProjectSettings/EditorBuildSettings.asset` | **三个场景、全部 enabled**：`ClientBoot`(0) → `Boot`(1) → `ClientShell`(2) | 文件实测 |
| `BUG_TRACKER.md` BUG-025 | 状态「已按方案 C 落地（2026-09-21）」 | tracker |

⇒ **§2.1 的「`ClientShell` 没有任何运行时入口」与 §6 的 P0-1 均已作废**，改为「已解决（方案 C）」。
原本 `Boot.unity` 保留在清单 index 1 未动，符合"可随时回滚"。

### 🔴 更正二（**新增 P0**）：`Boot.unity` 在新链路里**永远不会被加载** ⇒ TCP 探针与 YooAsset/HybridCLR 初始化全部不执行

`ClientBootLoader` 用的是 `SceneManager.LoadScene(name)`（**Single 模式**，单参重载默认 Single）⇒ `ClientBoot` 被 `ClientShell` **替换**。
`Boot.unity` 虽在构建清单 index 1，但**没有任何代码加载它**。

后果（按影响排序）：

| # | 后果 | 证据 |
|---|---|---|
| 1 | `ClientTcpConnectionProbe`（挂 `Boot`，`runOnStart: 1`）**不会运行** ⇒ 整条 TCP 链路在新入口下**没有网络** | `Assets/Main/Boot.unity` 是探针 guid `8c9ef21efe1aa4c4e912a27659d090c4` 的**唯一**宿主场景；全工程无其它引用 |
| 2 | `Init.cs` 的 YooAsset(HostPlayMode/WebPlayMode) + `LoadDll`(HybridCLR) + `firstSceneName` 加载流程**全部不执行** | `Init.cs` 只在 `Boot.unity` 上 |
| 3 | `LoginScene` / `MainScene` / 旧 `LobbyUI` 链路不再进入（若这是有意的，属预期） | 同上 |

**但这未必阻断客户端本身**（有实测支撑）：

- `ClientShell` 侧**不依赖 YooAsset**：`Assets/Client/Runtime/**` 里对 `YooAssets.` 的运行时引用为 **0**（只有 `ClientLoadingPage` 的一句注释提到"后续可由 YooAsset 实现替换"）。
- 客户端读表走 **`Resources/Table/*.json`**（随包，实测 22 个 json 存在）或 `{persistentDataPath}/config.pkg`，两者都不经 YooAsset。
- ⇒ **客户端 UI 可以在没有 YooAsset 的情况下独立跑起来**；被牺牲的是**网络（TCP 探针）**与旧大厅链路。

**待负责人裁决**（不擅自改）：

| 方案 | 做法 | 代价 |
|---|---|---|
| **C-1** | 把 `ClientTcpConnectionProbe` 也挂到 `ClientBoot` 上（或 `ClientShell` 里新增宿主） | 需场景写入授权；但这是"可启动 + 可操作 + 网络可用"目标的最小闭环 |
| **C-2** | 在 `ClientBootLoader` 里先加载 `Boot`、再由 `Boot` 的 `Init` 转到 `ClientShell` | 要改原公司启动流程，`AGENTS.md` §1/§3 约束 |
| **C-3** | 接受"客户端无网络"，网络验证另开一条链路跑 | 目标 B 的"可操作"在数据侧一直是本地模拟，但把"真实会话"排除在移植目标之外，需负责人明确 |

### 🔴 更正五（**C-1 不成立**）：探针依赖 HotFix 程序集，而 HotFix **只有** `Boot/Init` 这条 YooAsset 链能加载

负责人选定 C-1 后，我按"探针自带 `StartPersistentProbe()` → 跨场景迁移"的思路，把 `ClientTcpConnectionProbe` 挂到了
`ClientBoot`（见 0.3 节）。**但继续追依赖时发现 C-1 单独不成立**，证据链如下（每一环都有出处）：

| 环 | 事实 | 出处 |
|---|---|---|
| 1 | 探针的反射目标 `NetController` / `SysDefines` 在 **HotFix** 程序集里 | `Assets/Scripts/HotFix.asmdef`（`name: HotFix`）覆盖整个 `Assets/Scripts`；`NetController.cs` / `SysDefines` 在其下 |
| 2 | 探针等待 HotFix 就绪的时限 **15 秒**，超时即判失败 | `ClientTcpConnectionProbe.cs:24`（`HotFixWaitSeconds = 15f`）、:206-211（`FailSession("等待 HotFix/NetController 超时")`） |
| 3 | **全工程只有一处**把 HotFix 加载进 AppDomain | `Init.cs:216` `Assembly.Load(textAsset.bytes)`（grep `Assembly.Load` / `LoadMetadataForAOTAssembly` 全仓确认，`Assets/Editor/HybridCLR/*` 只产出资产，不做运行时加载） |
| 4 | 该处**先**加载 AOT 补充元数据，**再**加载 HotFix；两步都从 **YooAsset 包**取字节 | `Init.cs:202-219`：`LoadDll(package)` → `LoadMetadataForAOTAssemblies(package)`（:236-260，`package.LoadAssetAsync<TextAsset>(...)` + `RuntimeApi.LoadMetadataForAOTAssembly`）→ `package.LoadAssetAsync<TextAsset>("HotFix.dll")` |
| 5 | 整条 `Init` 链只挂在 `Boot.unity`，而 `Boot` 在新入口下 **永不加载** | 更正二 |

⇒ **结论**：把探针挂到 `ClientBoot` 后，真机上会稳定地在 **15 秒后**打出
`[TCP客户端] 失败退出 TCP：原因=等待 HotFix/NetController 超时`，**而不是**接通网络。
原因是**没有 YooAsset → 没有 AOT 元数据 → 没有 HotFix 程序集**，反射必然落空。
**这是"预期结果"而非环境问题**，所以不要在这种状态下反复重跑构建。

⇒ **同时也说明更正二的影响被低估了**：新入口砍掉的不是"一个探针"，而是
**整个 HotFix（`Assets/Scripts`，即原公司全部游戏逻辑）的运行时加载前提**。
客户端 `Assets/Client` 目前只用 `Resources/Table` 本地表与本地模拟数据，因此**界面仍能显示**；
但**任何需要 HotFix 的功能（网络、原大厅、协议）在新入口下全都不可用**。

**因此 C-1 需要升级**，三条可选（都需要负责人裁决，我不擅自改原公司启动流程）：

| 方案 | 做法 | 是否改原流程 | 代价 / 风险 |
|---|---|---|---|
| **C-2′** | `ClientBootLoader` 先**附加加载** `Boot`（`LoadSceneMode.Additive`）让 `Init` 跑完，待 HotFix 就绪后再切 `ClientShell` | 否（`Init.cs` 一行不改） | 需在 `ClientBootLoader` 里加"等待就绪"的编排；`Init` 结束时会自行 `LoadScene("LoginScene")`（Single）→ **会把 ClientShell 冲掉**，必须先阻断这一步 |
| **C-2″** | 在 `Init.cs` 增加一个**可配置的加载目标**（新 `[SerializeField] string`，默认 `"LoginScene"`，WebGL 下指向 `ClientShell`） | **是**（`Init.cs` 属启动/原公司代码边界，D1 明确点名不改） | 改动最小、可逆（一个字段 + 一处引用）；但需负责人明确授权 |
| **C-4** | 接受"客户端无 HotFix"，把 TCP/协议验证**移出**移植目标，用独立链路单独验 | 否 | 目标 B 的"真实会话"不再随客户端生效；需负责人确认这是可接受的验收口径 |

> 补充事实（对 C-2′/C-2″ 都有利）：`Init.cs:175` 的加载目标是**常量** `const string firstSceneName = "LoginScene";`，
> 且 `OnCompleted`（:501）里也硬编码了一次 `YooAssets.LoadSceneAsync("LoginScene")` ⇒ **共有两处**需要一并处理，别只改一处。

### 🟡 更正三：`ClientShell` 场景内**没有相机**

`Assets/Client/Scenes/ClientShell.unity` 内**不存在任何名为 `*Camera*` 的 GameObject**（已 grep）。
首帧依赖运行时兜底：`ClientBootstrap.EnsureUiClearCamera()`（`ClientBootstrap.cs:37-50`）在**找不到任何 enabled 且在层级内的相机**时才新建一个
`clearFlags=SolidColor` / `cullingMask=0` / `depth=-100` 的"Client UI Clear Camera"。

⇒ 这是**结构性单点**：若场景里存在"已禁用或不在层级内"的相机，或该兜底被改动，`ClientShell` 会**没有相机**。
登记为 B2 首屏取证项（**不下根因结论**，`BUG-009` 纪律）。`ClientBoot.unity` 也没有相机，但它只是加载器，无影响。

**补充实测（同一轮，降低了该单点的严重性）**：`ClientShell.unity` 内 **Canvas 组件 171 个，`m_RenderMode` 全部为 `0`（ScreenSpaceOverlay）**，
且场景内 **Camera 组件数 = 0**。ScreenSpaceOverlay 的画布**不经过相机渲染**，直接由引擎叠加到后备缓冲
⇒ 即使 `ClientBootstrap.EnsureUiClearCamera()` 不生效，UI **仍然会显示**。该兜底的实际作用是"清屏底色"，属于观感项而非显示前提。
⇒ 结论修正：**相机问题不是首屏可显示的阻塞项**；首屏取证仍按 B2 清单采日志，但不因"场景无相机"推断黑屏。

### ✅ 更正四：OSS 的 `/Assets` 层对**客户端移植路径**已不是阻塞

实测（HEAD 探测，2026-09-22）：

| URL | 状态 |
|---|---|
| `…/WebGL/2026-08-28-720/Assets/PackageManifest_DefaultPackage.version` | **404** |
| `…/WebGL/2026-08-28-720/Assets/PackageManifest_DefaultPackage.bytes` | **404** |
| `…/WebGL/2026-08-28-720/Assets/StreamingAssets/yoo/DefaultPackage/…version` | **404** |
| `…/WebGL/2026-08-28-720/PackageManifest_DefaultPackage.version` | **200**（内容 `2026-09-09-562`） |
| `…/WebGL/2026-08-28-720/StreamingAssets/yoo/DefaultPackage/…version` | **200** |
| `…/WebGL/{2026-08-27-686, 2026-08-27-1218, 2026-09-08-1036, 2026-09-09-562, 2026-09-09-598}/…version` | 全部 **404**（只有 `2026-08-28-720` 这个目录名在 OSS 上存在） |

⇒ `/Assets` 层**确实不存在**（`BUG-015` 证据成立）；但因为 **`ClientShell` 链路不经过 YooAsset**（更正二），
它**不再阻断本次客户端移植**。只有"要让 YooAsset/旧大厅链路在远端资源上工作"时才需要处理。
**决定：本轮不补传、不指向、不改 `assetPath`**（三个方案都不选），避免在无证据下改动基础设施。

### 🆕 新增能力：构建链路**已可命令行复现**（不再需要手点 GUI）

本轮新增 `Assets/Client/Editor/ClientWebGLBuildCommand.cs`，把原本只有 GUI 入口的两步包成 `public static`：

| 步骤 | 原本 | 现在 |
|---|---|---|
| YooAsset 资源包 | 菜单 `YooAsset/AssetBundle Builder`（GUI 手点） | `-executeMethod …ClientWebGLBuildCommand.BuildBundles` |
| WebGL Build + 微信转换 | 转换面板点「转换」 | `-executeMethod …ClientWebGLBuildCommand.ConvertMiniGame`（内部调官方 `WXConvertCore.DoExport(true)`） |
| 热更 DLL 同步 | 菜单 `Build/WebGL/Compile And Sync HotFix DLL` | `-executeMethod …ClientWebGLBuildCommand.SyncHotFixDll`（复用既有 `WebGLHotUpdateDllCommand`） |

`BuildBundles` **逐项照抄** `BuiltinBuildPipelineViewer.ExecuteBuild()` 的取值方式（同 `AssetBundleBuilderSetting` 的 EditorPrefs、同
`GetDefaultBuildOutputRoot()`= `{项目}/Bundles`、同 `BuiltinBuildPipeline`、同首包拷贝选项），**不新增任何构建逻辑**；
输出目录与既有格式一致：`Bundles/WebGL/DefaultPackage/<版本>`。
司机脚本：`Logs/webgl-port/run-webgl-port.ps1`（**纯 ASCII**，因 PowerShell 5.1 会把无 BOM 的 UTF-8 当 ANSI 读，中文会破坏解析）。

> ⚠️ **实测限制**：Unity Editor 打开本工程时，`-batchmode` 会被
> `It looks like another Unity instance is running with this project open.` 直接拒绝，
> 且 **Unity 不写 `-logFile`**（返回码为空）。⇒ 判定必须同时看"是否产生日志文件"，不能只看退出码。
> 本脚本已按此实现，并在起手处检测 `Temp/UnityLockfile` 预警。

### 本轮编译验证（只读，2026-09-22）

`Logs/verify-compile.ps1 -Profile All` → **Editor / Assembly-CSharp-Editor / WebGL 三目标 0 error**（含 09-21 新增的
`ClientBootLoader` / `ClientTableProbe` / `ClientSelfTest` / `ClientCollectionList` 等，以及本轮新增的 `ClientWebGLBuildCommand`）。

> 过程中该脚本**真的抓到一个错误**：`ClientWebGLBuildCommand` 初版漏了 `using Pinball.EditorTools;`
> → `error CS0103: The name 'WebGLHotUpdateDllCommand' does not exist`。
> 说明"三目标编译验证"确实能拦住真实缺陷（WebGL profile 只覆盖 `Assembly-CSharp`，Editor 目标覆盖 Editor 程序集）。

### 本轮工作区变化

`git status --porcelain`：**M 113 / D 39 / ?? 275**（09-20 快照为 M 104 / D 37 / ?? 233）。
另一会话在 `2026-09-20 19:17` ~ `2026-09-22 12:09` 之间改动了客户端配置表、多个页面、`ClientShell.unity`，
并新增 `WEBGL_B0_GATE.md` / `WEBGL_B1_BUILD.md` / `WEBGL_B2_DIAGNOSTICS.md` / `UI_*` 系列文档。**基线以 `Logs/webgl-port/` 下的时间戳快照为准。**

### 0.3 本会话已写入的改动（可回滚，逐项列明）

| # | 文件 | 改动 | 备份 / 回滚 |
|---|---|---|---|
| 1 | `Assets/Client/Editor/ClientWebGLBuildCommand.cs` | **新增**。命令行构建入口（`BuildBundles` / `BuildBundlesAndHotFix` / `SyncHotFixDll` / `ConvertMiniGame` / `VerifyStartupChain`） | 直接删除该文件即回滚；不影响任何既有代码 |
| 2 | `Logs/webgl-port/run-webgl-port.ps1` | **新增**。司机脚本（纯 ASCII），按序跑 步骤 0~5 | 删除即回滚 |
| 3 | `Logs/webgl-port/baseline-*.txt` | **新增**。只读冻结点快照 | 证据文件，保留 |
| 4 | `Assets/Client/Scenes/ClientBoot.unity` | **修改**。给既有 `ClientBoot` 对象**追加** `ClientTcpConnectionProbe` 组件（fileID `900000001`，`runOnStart: 1`），按负责人选定的 C-1 | 备份 `Logs/ClientBoot.before-tcpprobe-20260922-121524.unity.bak`（SHA-256 `353C45D265E4CCD54E8437886F3119D6D8743F07226AF5ECE587F9C87CEA21A6`） |
| 5 | `WEBGL_PORT_BASELINE.md` / `CURRENT_STATE.md` | 文档更新 | 文档，无回滚需求 |

> ⚠️ 关于第 4 项：改动 3 的目标（让探针在新入口下运行）**已被 §0.2 更正五 证伪** —— 探针会因 HotFix 未加载而 15 秒超时。
> 该组件**当前无实际收益，但也不是有害残留**（`ClientBootLoader` 会无条件 `LoadScene("ClientShell")`，探针的存在不改变启动结果）。
> **是否保留，等负责人就 C-2′/C-2″/C-4 作出裁决后再定**；若选 C-2″（改 `Init.cs` 加载目标），本组件就是正确解法的一半，应当保留。

### 0.4 C-2″ 已落地（2026-09-22，负责人选定）

负责人选定 **C-2″**：给 `Init.cs` 增加**可配置的加载目标**，复用既有 YooAsset/HybridCLR 初始化链直达客户端。

| # | 文件 | 改动 | 回滚 |
|---|---|---|---|
| 1 | `Assets/Main/Init.cs` | 新增 `public string ClientStartScene`（默认 `string.Empty`）+ 私有 `ResolveFirstSceneName()`；`OnStart` 的第 175 行常量改为解析结果并加一行日志；`OnCompleted` 的硬编码 `"LoginScene"` 一并改为同一解析器 | `git checkout -- Assets/Main/Init.cs`；备份无需（改动 19+/2−） |
| 2 | `Assets/Main/Boot.unity` | `Init` 组件新增 `ClientStartScene: ClientShell`（1 行） | 备份 `Logs/Boot.before-clientstartscene-20260922-132138.unity.bak`（SHA-256 `719E460EAA1B95222E97404B60F91E083824F38623675096D514AF11DC9B06DB`） |

**行为等价性（关键）**：`ClientStartScene` 为空 ⇒ `ResolveFirstSceneName` 返回 `"LoginScene"`，**与原行为逐字符一致**。
因此除 `Boot.unity` 显式配置为 `ClientShell` 之外，**任何既有场景/构建路径的行为都未改变**。两处加载点共用同一解析器，避免历史上"改了 `OnStart` 漏了 `OnCompleted`"的隐患。

**验证**：`Logs/verify-compile.ps1 -Profile All` → Editor / Assembly-CSharp-Editor / WebGL **三目标 0 error**。

> ⚠️ **仍需真机/构建确认的部分（未验证，别当已完成）**：`Init` 会加载 `HotFix.dll` 并执行其逻辑，
> 而 `ClientStartScene = ClientShell` 时**旧 `LoginScene` 不再加载**。HotFix 中是否有代码假定 `LoginScene` 已存在（例如 `AppRoot` / `GameController` 的启动假设），
> 本会话**未取证**。首次构建后的 Console 必须按 `WEBGL_B2_DIAGNOSTICS.md` §2.2 的标记集合核对"卡在哪一步"。

### 0.5 环境：shell 与文件编码（本轮踩坑与结论）

| 事项 | 结论 |
|---|---|
| **PowerShell 7** | 已装（MSIX `Microsoft.PowerShell 7.6.6`，`C:\Program Files\WindowsApps\Microsoft.PowerShell_<服务器地址>_x64__8wekyb3d8bbwe`，**无需管理员**）。`pwsh` 已在 PATH。 |
| 缘何此前仍是 5.1 | `dsh-pwsh-local` 的 `resolvePwshPath` 依序探测 ①`C:\Program Files\PowerShell\7\pwsh.exe` ②PATH 中的 `pwsh.exe` ③`System32\WindowsPowerShell\v1.0\powershell.exe`。**DSH 宿主 node 进程启动（09-22 09:03）早于 pwsh 安装（12:22）**，宿主 PATH 里没有 Store 别名 ⇒ 落到 ③ = 5.1。 |
| 已修 | `C:\Users\Administrator\.dsh\settings.yaml` 增加 `shell.pwshPath: <MSIX pwsh.exe 绝对路径>`（该字段在 `PwshLocalExecutor.Config` schema 内，`onChange` 会重新解析 ⇒ **热生效，无需重启**）。备份 `settings.yaml.bak-20260922-132232`。验证：工具宿主现为 `pwsh.exe` / `PSVersion 7.6.6` / `PSEdition Core` / `OutputEncoding utf-8`。 |
| **`edit` 工具会去掉 UTF-8 BOM** | 实测：含 BOM + CRLF 的文件经 `edit` 后 **BOM 丢失、CRLF 保留**；换到 pwsh 7 宿主后**行为相同** ⇒ 是工具自身写入实现所致，与 shell 无关。**规矩：编辑本来带 BOM 的文件后，必须重新校验并手工恢复 BOM**（本轮 `Init.cs` 已按此修复）。
| 各文件编码基线 | `Init.cs` = **UTF-8 BOM + CRLF**；`Boot.unity` / `ClientBoot.unity` = 无 BOM + **LF**；`CURRENT_STATE.md` = 无 BOM + CRLF；新增 md/ps1/cmd = 无 BOM + LF。改动前先测，避免制造整文件假 diff。 |

> `AGENTS.md` §3 已新增长期约束：**本机脚本一律优先 `pwsh`，不得静默回落到 `powershell.exe`**，并附上述实测原因。

---

## 0. 本次盘点遵守的纪律与边界

- 空转命令前先看事实：所有结论后面都附**命令或文件路径**这类可复查证据，不靠现象推断（`AGENTS.md` §4）。
- 不触碰 `BUG-009`（首屏黑屏 + URP shader）与 `BUG-013`（`app.json` 启动页）——负责人已明确由**其他会话**闭环，本会话不介入、不代为宣称关闭。
- 网络事实以 `TCP_WEBGL_HANDOFF.md` + 负责人提供的**权威日志**为准，不重新发明、不改 IP/端口/协议字段/SDK/项目设置。
- 未执行任何 Unity 构建：Unity Editor（PID 12984）正在运行且可能附带构建，且负责人可能在同工程内并行编辑。

---

## 1. 工作区与并发状态

| 项 | 事实 | 证据 |
|---|---|---|
| 分支 / HEAD | `main` @ `62bfdb31`（`chore: archive client development snapshot`） | `git rev-parse` |
| 未提交改动 | 已跟踪：**104 modified / 37 deleted**；未跟踪：**233 项** | `git status --porcelain` |
| 未跟踪大头 | `Bundles/**`（WebGL/StandaloneWindows64/Android 产物）、`HybridCLRData/**`、`WebGLPlugins/`、`TextToolDatas/`、`Assets/**` 新增文件；另有 `HANDOVER_PLAN.md` 本身未跟踪 | 同左 |
| 工程内已提交的删除 | `Assets/Client/Editor/ClientShellUiBuilder.cs`、`ClientShellDirectUpdater.cs`、`ClientBackpackPage.cs`、`ClientGachaPage.cs`、`ClientMarblePage.cs`、`ClientPageNavigator.cs`、`ClientUiFeedback.cs` 等（**保留现状，不得回滚**） | 同左 |
| Unity | `2022.3.57f1c2`（`ProjectSettings/ProjectVersion.txt`） | — |
| Unity Editor | **正在运行** PID 12984，启动于 2026-09-20 18:03:46；同机还有 `Unity.ILPP.Runner` / `UnityShaderCompiler` | `Get-Process Unity` |
| 结论 | 本会话**不执行命令行 Unity 批处理构建/编译**（工程被 Editor 占用，且并行会话可能正在改工程）。命令行验证改用工程既有 Roslyn 方案，见 §3。 | — |

> ⚠️ **并行写入风险（必须记录）**：负责人明确会**在其它会话**继续修 `BUG-009` / `BUG-013`。
> 因此本基线是**某一时刻的快照**；任何后续比对都必须先复核 `git status --short` 与关键文件 mtime。

---

## 2. 当前启动链（实测，唯一活跃路径）

```text
Assets/Main/Boot.unity                     ← Build Settings 中唯一启用的场景
  └─ Init  (Assets/Main/Init.cs)
       ├─ Awake: #if UNITY_WEBGL && !UNITY_EDITOR → PlayMode = WebPlayMode（覆盖 Inspector）
       ├─ OnStart: YooAssets 初始化 → WebPlayModeParameters + RemoteServices(DefaultHostServer)
       │            → UpdatePackageVersionAsync → UpdatePackageManifestAsync
       ├─ LoadDll(package)  ← 加载 HotFix.dll，HybridCLR 解释执行
       └─ YooAssets.LoadSceneAsync("LoginScene")     ← 硬编码，见 Init.cs:175-176
                                                    （OnCompleted 里也有同样一句，Init.cs:501）
Assets/HotUpdateResources/Scene/LoginScene.unity
  └─ StartGame (Assets/Scripts/GameLogic/StartGame.cs)
       └─ YooAssets.LoadAssetAsync<GameObject>("Canvas") → Instantiate → GameController.Instance.Init()
Assets/Scripts/GameLogic/GameController.cs
  └─ UIManager.OpenUICloseOthers(EnumUIType.LobbyUI)  → 旧大厅 LobbyUI（YooAsset 地址）
       └─ LobbyUI.MessageListener() 打印 "[WebGL 大厅演示] 使用本地英雄战力占位…"（LobbyUI.cs:546）
```

### 2.1 关键否定性事实（本次盘点新增）

- **`GameController.StartWebGLLobbyPreview()` 是死代码**：全工程（`*.cs` / `*.unity` / `*.prefab` / `*.asset` / `*.json`）**只有它自身的定义处**命中，**零调用方**。
  文件 mtime 2026-09-04 10:03，自 `62bfdb31` 后无改动。
  ⇒ `BUG_TRACKER.md` 中把它写成“直达大厅入口”的描述**与当前源码不符**；当前能跑到 LobbyUI 走的是 §2 那条**旧链路**（`Canvas` prefab + `GameController.Init`）。
  该分支**不会被本会话修改或删除**（属旧街机业务边界）；仅登记为事实，供负责人判断。
- ~~**`ClientShell` 没有任何运行时入口**~~ → **【已于 2026-09-21 被另一会话解决，见 §0.2 更正一】**。以下为 09-20 当时的原始取证，保留作时序证据：
  - `Assets/Client/Scenes/ClientShell.unity`（当时 5.48 MB）**不在 Build Settings**（当时 `EditorBuildSettings.asset` 只有 `Boot.unity`）。
  - `Assets/Scenes/TestScene.unity` 中**不存在** `ClientCanvas` / `ClientBootstrap` / `PagesLayer` / `PopupLayer` / `SystemLayer`（已用节点名 grep 取证）。
  - `Init.cs` 的 `firstSceneName` 是**常量** `"LoginScene"`，没有任何分支会加载 `ClientShell`。
  - **现状（09-22 复核）**：已新增 `ClientBoot.unity` + `ClientBootLoader`，清单为 `ClientBoot(0) → Boot(1) → ClientShell(2)`。
    但该方案的**新代价**见 §0.2 更正二 / 更正五（`Boot` 永不加载 ⇒ HotFix 与 TCP 不可用）——**不要据此认为入口问题已完全闭环**。

### 2.2 ClientShell 三层结构（存在，未被 WebGL 验证过）

节点名实测存在：`ClientCanvas`（`ClientShell.unity:100701`）、`PagesLayer`（68167）、`PopupLayer`（74169）、`SystemLayer`（112269）、`ToastRoot`（105805）、`ClientBootstrap`（137）。
`ClientBootstrap` 走 `Assets/Client/Runtime/ClientBootstrap.cs`，在 `Awake` 里 `ClientServices.InitializeForDevelopment()`。
`GameController.Start()` 里 `#if LOCAL_DEBUG` 之外会 `Debug.unityLogger.logEnabled = false` —— 若移植路径经过它，**WebGL 日志会被整体关掉**，取证前必须先确认这一点。

---

## 3. 编译验证（已执行，只读）

用工程既有脚本 `Logs/verify-compile.ps1`（Roslyn `csc.dll` + Bee 响应文件，**不需要关闭 Editor**，产物只写 `Logs/_verify`）：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\unity project\pinball\Logs\verify-compile.ps1" -Profile All
```

| 目标 | 退出码 | error CS |
|---|---|---|
| `Assembly-CSharp` [Editor] | 0 | 0（1 warning） |
| `Assembly-CSharp-Editor` [Editor] | 0 | 0 |
| `Assembly-CSharp` [WebGL]（去 `UNITY_EDITOR`、加 `UNITY_WEBGL`） | 0 | 0（2 warning） |

日志：`Logs/_verify/Assembly-CSharp.editor.log`、`Assembly-CSharp.webgl.log`（`ClientShopPage._nextCanvasSortingOrder`、`ClientServerPcSession.captureRawPackets` 两条 CS0414 未使用字段告警）。

**探针损坏已修复**（工作区未提交改动，属既有修复，本会话未改）：
`ClientTcpConnectionProbe.cs` 的 `ParseDisconnectNotification` 原先被误嵌进 `ParseLoginAck` 方法体内 → CS0106；
因为它整块在 `#if UNITY_WEBGL && !UNITY_EDITOR` 内，**Editor 编译看不到、WebGL 构建才会失败**。现已移出为同级方法，diff 仅 15+/11−。

> **平台教训（对后续会话的重要提示）**：`Assembly-CSharp` 里被 `#if UNITY_WEBGL && !UNITY_EDITOR` 包住的代码，
> **Editor 编译与 Unity 的 Editor 报错都不会覆盖**。任何改动过该区域的会话，**必须**跑 `-Profile WebGL`（脚本第 13/14 行就是为这个目的写的）。
> 附录 A 列出全部受影响的文件。

---

## 4. 网络链路（TCP）—— 已跑通，有权威证据

### 4.1 负责人提供的权威日志（本会话已读，非二手转述）

目录：`C:\Users\Administrator\AppData\LocalLow\xiaoshi\xiaozhen\ClientServerLogs`
工具有效组合：`session-20260910-152459.{jsonl,protocol.log,packets.bin}`（2026-09-10 15:24:59）

**运行环境（负责人 D2 口径）**：这条链路在 **Unity Play 模式**跑通过，在 **微信开发者工具**也跑通过；
**真机（Android / iOS）尚未验证**。⇒ "SDK + `TCPSocket` 在微信容器内可用"已成立；"真机可用"仍未取证。

`session-20260910-152459.protocol.log` 六个包全部 `HeaderValid`：

| # | 方向 | 协议 | 模块/协议 | 字节 | 结果 |
|---|---|---|---|---|---|
| 1 | 发送 | `CLGTHandReq` | 1 / 0 | 99 | 平台=6，产品=1，版本=7 |
| 2 | 接收 | `CLGTHandAck` | 1 / 1 | 17 | 状态=成功 |
| 3 | 发送 | `CLGTLoginReq` | 1 / 3 | 71 | 登录方式=1，令牌长度=60 |
| 4 | 接收 | `CLGTLoginAck` | 1 / 5 | 155 | 状态=成功 |
| 5 | 发送 | `CLPFGetHeroReq` | 2 / 1 | 8 | 空请求体 |
| 6 | 接收 | `CLPFGetHeroAck` | 2 / 2 | 348 | 英雄数组=4，英雄组数组=1（ID 13001/14001/14007/15001） |

⇒ `TCP_WEBGL_HANDOFF.md` 的成功判据**在 2026-09-10 已真实达成**，且**发生在微信开发者工具**（日志落在本机 `LocalLow`）。
JSONL 的时间戳为连续 90 ms 内完成（07:24:59.449 → .535 UTC）。

### 4.2 写日志的组件

- `Assets/Client/Runtime/Services/ClientServerPacketRouter.cs:72` → `Path.Combine(Application.persistentDataPath, "ClientServerLogs")`，
  产出 `.jsonl` / `.protocol.log` / `.packets.bin` 三件套（`captureRawPackets` 开关）。
- `Assets/Client/Runtime/Services/ClientServerPcSession.cs` 是 PC 侧传输适配器（`#if UNITY_WEBGL && !UNITY_EDITOR` 下明确让位给探针）。
- 探针 `Assets/Client/Runtime/ClientTcpConnectionProbe.cs`（764 行）挂在 `Boot`，序列化实测：
  `runOnStart: 1`、`useRealGuestLogin: 1`、`useDirectLocalTest: 0`、`logDeviceIdentifier: 0`、`captureServerPackets`（默认项）。
- 网关地址来源：`NetController.GetIpPort()` 的 WebGL 分支 `UnityWebRequest` 取 `SysDefines.Ip/Port`（`NetController.cs:111`、`147`、`202-204`），
  探针按 `TCP_WEBGL_HANDOFF.md` 的硬约束**用反射**访问 HotFix 类型（不得改回直接引用）。

### 4.3 探针的日志版本差

2026-09-10 的成功日志里**没有** `[TCP客户端]` 前缀文本（`.jsonl` 只有结构化记录）。
`TCP_WEBGL_HANDOFF.md` §"必须出现的成功日志"要求的 `[TCP客户端] …` 前缀文本，是否在当次 Console 中出现，**本会话无法从落盘文件证明**。
⇒ 判定：**链路本身有证据；"日志文本格式符合交接文档"这一点仍是待取证项**，下一次开发者工具运行必须整段回传 `[TCP客户端]` 过滤日志。

---

## 5. 基础设施与产物现状

| 项 | 事实 | 证据 |
|---|---|---|
| 微信 SDK | `Assets/WX-WASM-SDK-V2`（`Editor/WXConvertCore.cs` 107 KB、`convert.exe`、`Node/`）；UnityPlugin 版本 1.3.13 | `Assets/WX-WASM-SDK-V2/Editor/*` |
| 转换配置 | `Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset`（mtime 2026-09-09 10:04） | — |
| AppID | `<微信小游戏AppID>` | 同左 |
| CDN | `https://<CDN域名>/1/HotUpdate/NewAB/WebGL/2026-08-28-720` | 同左 + `Boot.unity:847-848` 完全一致 |
| 转换输出目标 | `D:/Users/Administrator/Desktop/弹珠接入text_1/720`（**工程外**，注意：目录名里的非 ASCII 路径） | 同左 `relativeDST`/`DST` |
| WebGL 编译选项 | `Webgl2: 1`、`DevelopBuild: 1`、`CleanBuild: 1`、`fbslim: 1`、`DeleteStreamingAssets: 1`、`brotliMT: 1`、`MemorySize: 512` | 同左 |
| Unity WebGL 设置 | `webGLMemorySize: 256`、`webGLCompressionFormat: 2`、`webGLThreadsSupport: 0`、`webGLTemplate: PROJECT:WXTemplate2022`、`webGLDebugSymbols: 1` | `ProjectSettings/ProjectSettings.asset:819-843` |
| 最后一次导出 | `…\720\minigame` = **66 文件 / 15.8 MB**，mtime **2026-09-09 10:04**（含 `wasmcode/…wasm.br` 15.1 MB、`unity-sdk/TCPSocket/`、`weapp-adapter.js`） | 目录实测 |
| 导出件 `app.json` | **不存在**（只有 `game.json`，无 `subpackages` 之外的 app.json）——归 `BUG-013`，**本会话不介入** | `Test-Path` |
| `assetPath` 真实拼接 | `game.js:169` `gameManager.assetPath = \`${CDN}/Assets\``（即 SDK 追加 `/Assets`） | 导出件 `game.js` |
| YooAsset 包 | 本地 `Bundles/WebGL/DefaultPackage/2026-09-09-562/` = **54 文件 / 55.3 MB**（`PackageManifest_DefaultPackage.version` 内容 `2026-09-09-562`） | 目录实测 |
| OSS 对应内容 | `/1/HotUpdate/NewAB/WebGL/2026-08-28-720/PackageManifest_DefaultPackage.version` → **HTTP 200**，内容解码为 **`2026-09-09-562`**（与本地一致） | `Invoke-WebRequest` |
| OSS 的 `/Assets` 层 | `/…/2026-08-28-720/Assets/PackageManifest_DefaultPackage.version` → **HTTP 404** (`NoSuchKey`) | 同左 → 归 **BUG-015**，仍未闭环 |
| 热更程序集 | `Assets/HotUpdateResources/Dll/WebGL/HotFix.dll.bytes` mtime **2026-09-04 10:12**；Bee 的 WebGL 输出 mtime 2026-09-09 09:59 | 文件实测 |
| 热更程序集新鲜度 | `Assets/Client/**` 最后一次改动 **2026-09-20 18:15** ⇒ 当前 WebGL 热更 DLL **落后工作区约 16 天** | 同左 |
| 同步热更 DLL 的官方入口 | `Assets/Editor/HybridCLR/WebGLHotUpdateDllCommand.cs` → 菜单 **`Build/WebGL/Compile And Sync HotFix DLL`**（`CompileDllCommand.CompileDll(WebGL)` 后拷到 `Dll/WebGL/HotFix.dll.bytes`） | 源码 |
| XMLua | `WebGLPlugins/` 有 Lua 5.3 C 源；`Assets/Lua` 仅 7 文件 / 1.2 KB ⇒ 风险≈0，逻辑在 C# | 与交接文档一致 |
| YooAsset 打包入口 | **没有** `.asset` 形式的收集器配置（只有 Space Shooter 示例）；打包走 **Editor 窗口**（`AssetBundleBuilderWindow`）⇒ **只能由负责人在 GUI 里执行**（`AGENTS.md` §4 禁止代理代操作 GUI） | `ProjectSettings`/`Assets` 全量扫描 |

### 5.1 OSS `/Assets` 与本地视角的一处矛盾（登记，不下结论）

- 客户端请求 `/Assets/…version` → **404**（`BUG-015` 证据仍成立）；
- 但 2026-09-09 那次导出**确实跑到了 LobbyUI 并完成了 TCP 全链路**，说明当时**资源清单初始化并没有被这个 404 卡死**（或走了内置/本地清单路径）。
- 结论按纪律处理：**这是两组事实并存**，不是"BUG-015 会阻断启动"的证明，也不是"BUG-015 已无影响"的证明。
  ⇒ 下一次开发者工具运行必须**原样回传 Network 面板里 YooAsset 的全部请求与状态码**，才能定性。

---

## 6. 风险主次（按"是否阻断目标"排序）

> **本表已按 D1/D2/D3 重排**：D1 选定"只做链路、不动入口"后，P0-1 从**阻塞项**降为**已知事实、暂不处理**；
> 当前真正的 P0 是热更 DLL 新鲜度（链路里跑的就是它）。D3 意味着所有"本会话"动作要等负责人放行。

| 级 | 风险 | 阻断什么 | 依据 | 归属 / 状态 |
|---|---|---|---|---|
| **P0** | WebGL 热更 `HotFix.dll` 落后工作区 16 天（09-04 vs 09-20） | 链路里加载的就是 `HotFix.dll` ⇒ 任何一次构建都会**带上旧的客户端逻辑** | §5 | 本会话（等放行） |
| **P1** | 真机未验证：TCP 全链路只在 **Unity Play + 微信开发者工具**通过，**真机从未跑过**（D2） | 目标 B 的"真机测试"——iOS WebGL2/内存/纹理压缩是交接文档列的 1 号风险 | §4.1、D2 | 本会话（等放行） |
| **P1** | OSS `/Assets` 层 404（`BUG-015` 未闭环）；`MiniGameConfig.CDN` 与 `Init.DefaultHostServer` 是同一个 URL，但 SDK 额外追加 `/Assets` | 远端版本/清单初始化，可能表现为停在加载或回退 | §5、§5.1 | 需负责人上传；代码侧本会话 |
| **P1** | 验证方法学断层：Editor 编译（含 Unity 自己的报错）**不覆盖** `UNITY_WEBGL && !UNITY_EDITOR` 代码块 | 会在 WebGL 构建才炸（探针 CS0106 就是实例） | §3、附录 A | 本会话（已固化 `-Profile WebGL`，纯只读、可随时执行） |
| **P1** | `Debug.unityLogger.logEnabled = false`（`GameController.Start`，非 `LOCAL_DEBUG`） | WebGL 日志本来就少，一旦被关，**取证直接失效** | §2.2 | 本会话（先取证再决定） |
| **P2** | 资源包分层未复验：`MiniGameConfig.CDN`(…-720) 与 YooAsset 版本(2026-09-09-562) **名义不一致但内容一致**；`Bundles/WebGL` 总计 719.5 MB 但真正有效的默认包只有 55.3 MB | 上传范围/缓存失效判断容易出错 | §5 | 本会话 |
| **P2** | 构建/转换/上传全在**工程外目录**且只能 GUI 触发（YooAsset 打包窗口、微信转换面板、OSS 上传） | 无法自动化、不可复现 | §5 | 需负责人执行（见 §8） |
| **P2** | 工作区巨大（104 M / 37 D / 233 ??）且**有并行会话在改**；`WebGL Bulids/` 是空目录 | 基线与结论**随时过期**，易误判 | §1 | 记录即可 |
| **P3** | `GameController.StartWebGLLobbyPreview` 死代码 + tracker 描述与源码不符 | 误导后续会话 | §2.1 | 记录，待负责人裁决 |
| ~~P0→已豁免~~ | `ClientShell` 无 WebGL 运行时入口（不在 Build Settings；`Init.firstSceneName` 硬编码 `LoginScene`；`TestScene` 无 `ClientCanvas`；`WXConvertCore.GetScenePaths()` 只打包 enabled 场景） | **按 D1 暂不处理**。事实已取证并留档：不做入口，小游戏里永远到不了 `ClientShell` | §2.1 | **已知事实 / 暂不处理（D1）** |
| — | `BUG-009`（首屏黑屏 + URP shader）、`BUG-013`（`app.json` 启动页） | **本会话不介入**（负责人已指派其他会话，且 D3 要求先等其闭环） | — | 其他会话 |


---

## 7. 第一步计划（本会话，按序；每步都有可复查产物）

> 原则：**按 D1 只做链路、不动入口**；先把链路本身跑通并建立日志/性能基线，新客户端 UI 的接入另开一轮。
> **按 D3**：以下步骤**全部等负责人放行后再执行**；放行前不写代码/场景/资源。

1. **B0-1 冻结点复核**：每次动手前重跑 `git status --short` + 关键文件 mtime + 二进制 SHA-256
   （`Assets/HotUpdateResources/Dll/WebGL/HotFix.dll.bytes`、`Bundles/WebGL/DefaultPackage/2026-09-09-562/*`，
   以及候选入口相关物），写入 `Logs/webgl-port/`，保证任何改动可区分"既有/本会话新增"。
2. ~~**B0-2 入口方案**~~ → **已由 D1 裁决：选 C（不动入口）**。`Init.cs` 不改、`ClientShell` 不进 Build Settings。
   本节仅保留记录：候选 A（改 `Init.cs` 加受控分支）、候选 B（进 Build Settings + 场景名可配）**均未采纳**，
   留待新客户端接入那一轮重新决策。
3. **B0-3 OSS 层对齐**：先只读取证 `/Assets/` 下的实际内容（HEAD 探测），再定最小修法——
   是补传 `/Assets`，还是让 `Init.DefaultHostServer` 指向 `/Assets`，**还是修 `assetPath` 拼接**。
   三个方案必须只选一个并写清理由与回滚方式（遵守 `MODULE.md` §12.4 同一所有者原则）。
4. **B0-4 热更对齐（当前唯一 P0）**：由负责人在 Unity 菜单执行 `Build/WebGL/Compile And Sync HotFix DLL`（代理不能代操作 GUI），
   然后本会话校验 `Dll/WebGL/HotFix.dll.bytes` 与 Bee WebGL 输出**字节一致**并记录 SHA-256。
5. **B0-5 端到端一次跑通**：WebGL Build → 微信转换 → 上传首包 → 开发者工具清缓存重跑 →
   回传**完整** Console（含 `[TCP客户端]` 前缀段、`资源包初始化成功`、YooAsset 全部网络请求）。
6. **B1**：把可复现步骤、参数、日志位置、失败排查写成《微信小游戏构建与真机手册》（阶段 C 交付物之一）。
7. **B2**：真机回归矩阵（`HANDOVER_PLAN.md` §5 B2 那张表）逐项执行，未测不得写通过。
   —— 真机是本轮**唯一还没有任何证据**的环节（D2），优先级高于任何渲染/性能微调。

---

## 8. 需要负责人配合的动作（代理不做 GUI / 不碰密钥与上传）

> ⏸ **按 D3，本表暂不执行**：负责人要先在其它会话清完 `BUG-009` / `BUG-013`，
> 清完并放行后再按本表逐项推进。表中第 1 项已由 D1 回答完毕，保留作记录。

| # | 动作 | 前置条件 | 逐步路径 | 期望结果 | 失败排查 | 需要回传 |
|---|---|---|---|---|---|---|
| 1 | ~~决策：ClientShell 的进入方式~~ | ✅ **已决：选 C（不动入口）** | — | — | — | 已完成（D1） |
| 2 | **（可选）腾出编译窗口** | 若需要真正跑 Unity 命令行构建 | 关闭正在跑构建的 Editor，或告诉代理"现在可以用命令行" | `-batchmode` 不再被 `HandleProjectAlreadyOpenInAnotherInstance` 拒绝 | 若被拒：保留日志、不重复尝试 | 是否可以批处理 |
| 3 | **补传 OSS `/Assets` 层** | 确认 §5.1 的定性 | 把 `Bundles/WebGL/DefaultPackage/2026-09-09-562/` 的运行时文件（`.version` / `.bytes` / `.hash` / `*.bundle`，**不含** `BuildReport_*.json`、`buildlogtep.json`、`link.xml`）整体上传到 `…/NewAB/WebGL/2026-08-28-720/Assets/` | `…/2026-08-28-720/Assets/PackageManifest_DefaultPackage.version` 返回 200 | 参考 `BUG-015` 记录：SDK 会固定追加 `/Assets`，不要再叠加第二层版本目录 | 上传完成 + 该 URL 的 HTTP 状态码 |
| 4 | **重建 YooAsset 资源包**（若上一步选择重建而非补传） | Unity Editor 打开 | YooAsset → AssetBundle Builder 窗口，包名 `DefaultPackage`，目标 **WebGL**，管线 **BuiltinBuildPipeline**，然后 Build | 产出新的版本号目录 + 控制台无 `The address is existed` | `BUG-012`：收集器只收 `Assets/Resources_HotUpdate/BNRes/UI/LobbyUI.prefab`（`AddressByFileName`），不要整目录收集 | 新版本号 + Build 日志 |
| 5 | **同步 WebGL 热更 DLL** | Editor 编译零错误 | 菜单 `Build/WebGL/Compile And Sync HotFix DLL` | 控制台出现 `[WebGL 热更新程序集] 已编译并同步 HotFix.dll：…` | 若 `HybridCLR did not produce the WebGL HotFix DLL` → 先修编译错误 | 日志原文 |
| 6 | **WebGL Build + 微信转换** | 上两步入参齐备 | Unity 构建设置切 **WebGL** → Build；随后微信小游戏转换面板（`MiniGameConfig` 已配好，输出到桌面 `…/720`） | 导出目录出现新的 `game.js` / 新 hash 的 `.wasm.br` / `.data…br` | 黑屏/启动页问题归 `BUG-009`/`BUG-013`（其他会话） | 构建日志 + 导出目录清单（文件数与总大小） |
| 7 | **首包上传 OSS** | 第 6 步完成 | 上传本次导出的首包（wasm/data/framework）到 `DATA_CDN` 对应层 | 与导出的 hash 完全匹配 | WebGL 导出会改 hash —— 必须传**当次**导出件 | 上传完成确认 |
| 8 | **开发者工具重跑** | 上传完成 | 微信开发者工具：清缓存（含文件缓存）→ 编译 → 运行，**至少等 30 秒** | 出现 WebGL2 上下文、`资源包初始化成功`、`[TCP客户端]` 全段 | 记 `BUG-013` 要求：导出完整 Console，不手改生成工程 | **完整 Console**（含 Network 里 YooAsset 请求状态码） |
| 9 | **真机测试** | 开发者工具通过 | 微信扫码/体验版，按 `HANDOVER_PLAN.md` §5 B2 矩阵逐项 | 每项有结论 | — | 设备型号/系统/微信版本/构建标识/截图或日志路径 |

> 安全提醒：不要在任何回传里贴 token、`random_key`、AppSecret 或真实服务端凭据。`ClientServerPacketRouter` 已对正文做脱敏（`.protocol.log` 里 token 显示为"正文已隐藏"），保持这个习惯。

---

## 附录 A：受 `#if UNITY_WEBGL && !UNITY_EDITOR` 影响的文件（Editor 编译不覆盖，改动后必跑 `-Profile WebGL`）

```text
Assets/Client/Runtime/ClientTcpConnectionProbe.cs      (31, 77, 89, 115)
Assets/Client/Runtime/Services/ClientServerPcSession.cs (41, 54)
Assets/Main/Init.cs                                     (33, 179, 262)
Assets/Scripts/GameLogic/NetController.cs               (33, 111, 142)
Assets/Scripts/GameLogic/GameController.cs              (195)
Assets/Scripts/Frame/Manager/UIManager.cs               (233, 370)
Assets/Scripts/Frame/Manager/ResManager.cs              (142, 236)
Assets/Scripts/BNRoom/UI/LobbyUI.cs                     (511, 517, 543, 559)
Assets/Editor/EditorTools/Utility/Utility.Path.cs        (47)
Assets/Main/Yooasset/Runtime/**                          (散见)
Assets/WX-WASM-SDK-V2/Runtime/**                         (散见)
```

## 附录 B：本次盘点的原始命令清单（可复现）

```powershell
# 工作区
cd 'D:\unity project\pinball'; git rev-parse --abbrev-ref HEAD; git rev-parse --short HEAD; git status --porcelain
# 并发占用
Get-Process | Where-Object { $_.ProcessName -match 'Unity|WeChat' }
# 编译验证（三目标）
powershell -NoProfile -ExecutionPolicy Bypass -File 'Logs\verify-compile.ps1' -Profile All
# 场景入口取证
Select-String -Path 'Assets\Main\Boot.unity' -Pattern 'runOnStart|useRealGuestLogin|DefaultHostServer|FallbackHostServer'
Select-String -Path 'Assets\Client\Scenes\ClientShell.unity' -Pattern 'm_Name: (ClientCanvas|PagesLayer|PopupLayer|SystemLayer)$'
Select-String -Path 'Assets\Scenes\TestScene.unity' -Pattern 'm_Name: (ClientCanvas|ClientShell)$'
# 死代码取证
Get-ChildItem Assets -Recurse -File -Include *.cs,*.unity,*.prefab,*.asset,*.json | Select-String -Pattern 'StartWebGLLobbyPreview' -List
# 资源与远端
Get-ChildItem 'Bundles\WebGL\DefaultPackage\2026-09-09-562' -File | Measure-Object Length -Sum
Invoke-WebRequest -Uri 'https://<CDN域名>/1/HotUpdate/NewAB/WebGL/2026-08-28-720/PackageManifest_DefaultPackage.version' -UseBasicParsing
Invoke-WebRequest -Uri 'https://<CDN域名>/1/HotUpdate/NewAB/WebGL/2026-08-28-720/Assets/PackageManifest_DefaultPackage.version' -Method Head
# 导出件
Get-ChildItem 'D:\Users\Administrator\Desktop\弹珠接入text_1\720\minigame' -Recurse -File | Measure-Object Length -Sum
Select-String -Path 'D:\Users\Administrator\Desktop\弹珠接入text_1\720\minigame\game.js' -Pattern 'DATA_CDN|assetPath'
# 网络权威日志
Get-Content "$env:LOCALAPPDATA\..\LocalLow\xiaoshi\xiaozhen\ClientServerLogs\session-20260910-152459.protocol.log"
```

---

## 待确认清单（本文件新增）

1. ~~**ClientShell 的 WebGL 入口方案**~~ ✅ **已由 D1 裁决：选 C（不动入口）**，本项关闭。新客户端接入时重新决策。
2. ~~**那次 TCP 全链路成功是开发者工具还是真机？**~~ ✅ **已由 D2 回答**：**Unity Play 模式 + 微信开发者工具**都通过；**真机未验证**（转为 §6 的 P1 风险）。
3. `GameController.StartWebGLLobbyPreview` 的"直达大厅"是否**已被有意废弃**？若是，请更新 `BUG_TRACKER.md`/`RISK-010` 的描述。**（仍待确认）**
4. OSS `/Assets` 404 与"09-09 实际跑通"如何取舍：**补传** 还是 **指向**？——决定 P1 的修法。**（仍待确认，等 D3 放行）**
5. `MiniGameConfig.CDN` 的目录名（`…2026-08-28-720`）与 YooAsset 实际版本（`2026-09-09-562`）名义不一致：是否统一命名，避免后续误判缓存。**（仍待确认）**

---

## 变更记录

| 时间 | 变更 | 触发 |
|---|---|---|
| 2026-09-20 18:2x | 建立本文件（只读基线盘点 A0） | 负责人交办移植任务 |
| 2026-09-20 18:4x | 追加 §0.1 决定表；按 D1 重排 §6（P0-1 降级为已知事实）；§7 步骤 2 标记已裁决；§8 标记"暂不执行"；§4.1 补运行环境；关闭待确认 1、2 | 负责人 D1/D2/D3 |

