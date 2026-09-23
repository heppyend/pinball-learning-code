# 弹珠项目开发交接文档（HANDOVER_DEV）

> **本文定位：as-built 现状交接 + 权威索引 + 硬约束与坑。**
> 不是目标计划（那是 `HANDOVER_PLAN.md`），也不是阶段流程（那是 `DEVELOPMENT_WORKFLOW.md`），更不是规则约束（那是 `AGENTS.md`）。
>
> | 项 | 值 |
> | --- | --- |
> | 核对日期 | 2026-09-23 |
> | 工程根目录 | `D:\unity project\pinball`（**含空格**，命令里必须加引号） |
> | Unity | `2022.3.57f1c2`，`C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe` |
> | 最近提交 | `62bfdb31 chore: archive client development snapshot`（2026-09-11 17:16） |
> | 工作区状态 | **有大量未提交改动**；数量、分支与远端差异以接手时 `git status --short` / `git status -sb` 为准，不沿用旧快照数字 |
> | 交接范围 | `Assets/Client` 非战斗客户端 + WebGL/微信小游戏移植链路 |
> | 不在范围 | 旧街机战斗与关卡、真实服务端/账号/支付、运营后台、发布上传、基础设施升级 |
>
> **读到冲突时以谁为准：以本文 + 磁盘实际内容 + 代码为准。** 已知的文档过期点全部列在 §2.3，不再逐处猜测。
>
> 🟠 **关于并发编辑（2026-09-22）**：移植侧由**另一个会话**同期完成，它新写了 `WEBGL_MINIGAME_HANDBOOK.md`（18.6 KB，移植侧总入口），并改动了 `CURRENT_STATE.md`、`BUG_TRACKER.md`、`DEVELOPMENT_WORKFLOW.md`、`PROJECT_OVERVIEW.md`、`HANDOVER_PLAN.md`、`WEBGL_B2_REALDEVICE.md`、`HOME_PAGE_SCENE_FIX_OPTIONS.md`、`Assets/Main/MODULE.md`、`Assets/Main/Init.cs`，并新增 `Assets/Main/WebGLPostSceneLoadRunner.cs`。
> 该会话**已结束**，其成果**已全部并入本文**：§0 状态 · §2.2 文档地图 · §2.5 替代关系 · §5.1 启动链 · §6.3–§6.7 移植 · §9.1⑬⑭⑮ 坑 · §10 遗留 · §11.2/§11.3 授权与敏感项。
> ⚠️ 但**行号（`文件:行`）仍会随后续编辑漂移** ⇒ 行号只作"定位提示"，请以**关键词/小标题**为准重新定位；动手前先 `git status --short` + 看文件 mtime。
>
> ⚠️ 本文档**每一条结论都带证据指针**（`文件:行` / 命令 / 日志路径），这是本项目自身的纪律（`AGENTS.md` §4）。请保持这个写法，不要写悬空结论。
## 阅读路线（按需要逐层深入）

1. **先看本页 §0–§1**：确认当前阶段、怎样打开客户端、如何生成新的小游戏导出物，以及哪些动作仍需负责人操作。
2. **要改客户端页面或数据接口**：读 `AGENTS.md`、`CURRENT_STATE.md` 顶部交接快照、`Assets/Client/MODULE.md`，再读对应目录的 `MODULE.md`；接口和调用关系看本页 §3–§4。
3. **要构建/排查微信小游戏**：读 `WEBGL_MINIGAME_HANDBOOK.md`，按需进入 `WEBGL_B1_BUILD.md`、`WEBGL_B2_DIAGNOSTICS.md`、`WEBGL_B2_REALDEVICE.md`。
4. **要处理遗留问题**：以 `BUG_TRACKER.md` 的当前条目为准；`CURRENT_STATE.md` 与本页中的日期记录是历史证据，不能直接当作今天仍未解决的清单。

> 文档分层：本页是跨客户端与移植的**总入口**；`Assets/Client/**/MODULE.md` 解释代码模块；`WEBGL_MINIGAME_HANDBOOK.md` 是移植侧操作手册；`CURRENT_STATE.md` 是短期状态入口；`HANDOVER_PLAN.md` 是计划，不是完成证明。

### 改完 UI / 客户端代码后：重新生成小游戏

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File 'D:\unity project\pinball\Logs\webgl-port\run-pipeline-2phase.ps1'
```

- 本机脚本优先使用 **PowerShell 7 (`pwsh`)**，不要调用 `powershell.exe` 5.1（见 `AGENTS.md` §3）。
- **先保存并关闭 Unity Editor**。脚本会强制结束当前机器上所有名为 Unity 的进程并清理项目锁文件；先保存改动并关闭编辑器，否则未保存的 Inspector/场景改动会丢失。实现位置：`Logs/webgl-port/run-pipeline-2phase.ps1` 的 `Stop-UnityLeftovers`。
- 两阶段依次准备 WebGL/热更程序集，再构建 YooAsset 包、WebGL Player 并调用微信转换。以脚本退出码和 Phase A/B 日志确认成功；日志写入 `Logs/b1-phaseA-*.log`、`Logs/b1-phaseB-*.log`。
- 代码、场景、贴图或其他资源变化都应重新生成并检查产物；**不要预先假定只需上传某一个文件**。导出后按 `WEBGL_MINIGAME_HANDBOOK.md` 的哈希清单比较文件、核对客户端实际请求 URL，再按授权执行上传。
- 该脚本只负责本地构建与转换。OSS 上传、微信开发者工具预览、真机回归、体验版/正式版配置是后续独立步骤；真实上传与发布不会由导出自动完成。
- “接口或 UI 改动后重新导出”可按上述本地流程理解。真实服务端契约、账号/支付、域名与生产环境接入不会由导出自动完成。

### 接口替换边界（实际源码）

- 页面经 `ClientServices.Data` 访问 `IClientDataService`；开发期由 `LocalClientDataService` 提供模拟玩家状态。当前数据服务替换方法是 `ClientServices.ReplaceDataService(IClientDataService)`（`Assets/Client/Runtime/Services/ClientServices.cs`）。
- 静态配置经 `ClientServices.Config` 读取 `IClientConfigService`，当前由 `TableClientConfigService` 读取项目配置表；不要把玩家拥有状态塞进静态配置。
- 数值展示经 `ClientServices.Stats`；当前启动时仍构造 `LocalClientStatsGateway`。服务端协议和 DTO 未提供，不能据此推断真实接口。
- 改动接口前先读 `Assets/Client/Runtime/Services/MODULE.md` 与接口定义；保持页面调用服务边界，不在 UI 回调里自行扣币、领奖或实现网络协议。

---

# 0. 交接状态一句话

**客户端侧**：负责人 2026-09-22 宣布"**全部界面的 UI 验收算通过了**"，当时 UI 适配主线收口；该结论是人工验收记录，不代表之后的改动已回归。客户端以 `Assets/Client/Scenes/ClientShell.unity` 与 `Assets/Client/Runtime/` 为主要范围，实际页面和数据服务边界见 §3–§4。

**移植侧（2026-09-22 记录）：阶段 B 五项曾全部达成** —— ✅ 可构建 / ✅ 可启动 / ✅ 可操作 / ✅ 真实会话 TCP / ✅ 真机。
真机（Android 1080×2400，预览版）实测：首页正常渲染、首页→编队页→新手任务页可跳转、**TCP 六包齐全**（英雄=4，英雄组=1）、`RT-FPS 50~63 / Jank 0 / BigJank 0 / Stutter 0.00%`、无致命异常（`WEBGL_B2_REALDEVICE.md` §八）。
移植物料：一键出包 `Logs/webgl-port/run-pipeline-2phase.ps1`（两阶段约 **9.2 min**，`WXConvertCore.DoExport` 返回 `SUCCEED`）；移植侧**唯一入口** = `WEBGL_MINIGAME_HANDBOOK.md`。

**当时记录的未完成项**：B2 矩阵深度项（安全区与遮挡、返回栈压测、弱网/断网、后台切回）、**内存峰值未采集**、**生产化前置**（`RISK-021`）。接手时先核对 `CURRENT_STATE.md`、`BUG_TRACKER.md` 与移植手册的最新记录。导出成功只证明构建/转换完成，不代表已上传、真机复验或正式环境可用。

**文档层面**：存在多处过期/重复与两处"同一文件内部前后不一致"（§2.3），且**本仓库有并发编辑**（见顶部提示）。

---

# 1. 五分钟上手

## 1.1 环境三件套（缺一即失败）

| 项 | 要求 | 说明 |
| --- | --- | --- |
| PowerShell | **`pwsh`（PowerShell 7）** | ⛔ 禁用 `powershell.exe`(5.1)：它把**无 BOM 的 UTF-8 `.ps1` 当 ANSI 读**，含中文的脚本直接解析失败（`WEBGL_PORT_BASELINE.md` §0.5） |
| Unity | `2022.3.57f1c2` | 不要用其它版本打开本工程 |
| 批处理环境变量 | `$env:ALLUSERSPROFILE = 'C:\ProgramData'` | 只传给子进程，**不要写系统环境变量**（`AGENTS.md` §3）。已内置在 `Logs\webgl-port\run-webgl-port.ps1:38`、`run-pipeline-2phase.ps1:34`、`repair-rank-batch.ps1:31`、`verify-adapt12.ps1:17`、`verify-adapt12-offline.ps1:9` |

> ⛔ **路径含空格**：不要用 `Start-Process -ArgumentList`（会被拆成 `D:/unity project/D:/unity`）。用 `ProcessStartInfo.ArgumentList`。

## 1.2 第一次把它跑起来（GUI，由负责人操作）

1. Unity Hub → 打开 `D:\unity project\pinball`（首次导入耗时较长）。
2. 打开场景 `Assets/Client/Scenes/ClientShell.unity`（**不是** `Boot.unity`；客户端 UI 全在这个场景里）。
3. 点 Play。
4. Console 应出现：配置表路径/行数汇总（`[配置表] …`）、本地模拟数据就绪的日志。
   桌面基线参照 `Logs/b1-prebuild-baseline.log`：**可用 10 张 / 缺失 0 / 解析失败 0，`Hero = 54 行`**。
5. 逐项走：主页五导航、头像→个人中心、编队、扭蛋、商城、邮箱、活动、任务、公告、英雄→英雄详情→培养、以及返回栈。
6. Console 筛 `[UI追踪]` 可看到 `[点击]`/`[导航]`/`[弹窗]`/`[显隐]`/`[路由]`/`[性能]`（追踪器由 `ClientShellController` 自动补挂；`ClientUiTrace.Enabled = false` 可整体静音）。

## 1.3 三条可复现验证命令（命令行）

```powershell
# ① 三目标编译（Editor / Assembly-CSharp-Editor / WebGL profile）—— 不需要关 Unity
pwsh -NoProfile -ExecutionPolicy Bypass -File "D:\unity project\pinball\Logs\verify-compile.ps1" -Profile All

# ② 纯逻辑自检（96 项断言；非 0 退出码 = 有失败）—— 需先关闭 Unity
& "C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe" -batchmode -quit `
  -projectPath "D:\unity project\pinball" `
  -executeMethod Pinball.Client.Editor.ClientSelfTest.RunAll `
  -logFile "D:\unity project\pinball\Logs\self-test.log"

# ③ 构建前全量基线（真机对账用）—— 需先关闭 Unity
& "C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe" -batchmode -quit `
  -projectPath "D:\unity project\pinball" `
  -executeMethod Pinball.Client.Editor.ClientTableProbe.LogFullBaseline `
  -logFile "D:\unity project\pinball\Logs\b1-prebuild-baseline.log"
```

一条命令跑 8 项离线检查：`pwsh -File "D:\unity project\pinball\Logs\verify-adapt12-offline.ps1"`。

## 1.4 什么时候必须关 Unity —— 记住这条分界线

| 你要做的事 | 关 Unity？ | 用什么 |
| --- | --- | --- |
| 改纯 C# 代码 | ❌ 不用 | `Logs\verify-compile.ps1`；再由负责人在 Unity 里 `Ctrl+R` |
| 只读审计（UI 契约/层级/几何） | ❌ 不用 | `Logs\ui-*.ps1`、`Client/结构修复/6` |
| 跑 `-executeMethod` 批处理 | ✅ **必须关** | 否则报 `HandleProjectAlreadyOpenInAnotherInstance`，**且不产出任何日志** |
| 写场景（`RepairAll` / 文本补丁） | ✅ **必须关** | 场景以磁盘为准，未保存的 Inspector 改动会丢失 |

## 1.5 四条一上手就要知道的红线

1. **不要 `git reset` / 清理 / 覆盖工作区** —— 现有未提交改动里包含负责人的人工成果（含手调锚点、未跟踪文档）。`AGENTS.md` §3。
2. **不要改** `Packages/manifest.json`、项目设置、YooAsset/HybridCLR/XLua 基础设施、微信 SDK（唯一例外：负责人已授权可改 `MiniGameConfig.asset` 的 CDN 与构建清单）。
3. **不要写密钥/账号/订单/真实服务端地址**进仓库、日志或演示数据。
4. **GUI 一律由负责人操作**（Unity Editor、IDE、构建窗口、开发者工具、平台后台）；代理只做代码/配置/Markdown + 命令行验证。

---

# 2. 项目地图、权威文档地图、已知冲突

## 2.1 目录职责（只列要动的；第三方见附录 C）

| 路径 | 职责 | 状态 |
| --- | --- | --- |
| `Assets/Client/Scenes/ClientShell.unity` | **客户端唯一开发场景**，UI 的真实载体 | 已进 Build Settings（index 1） |
| `Assets/Client/Scenes/ClientBoot.unity` | 方案 C 的专用启动场景 | ⛔ **已弃用**，且已移出构建清单；`ClientBootLoader.cs:9-33` 明写"不要加回" |
| `Assets/Client/Runtime/*.cs` | 页面控制器（`Client*Page.cs`）、启动（`ClientBootstrap.cs`）、红点、TCP 探针 | 已实现 |
| `Assets/Client/Runtime/Domain/` | 纯 C# 领域模型（禁 UI/Sprite/网络） | 已实现 |
| `Assets/Client/Runtime/Services/` | 数据服务边界 + 本地模拟 + 表实现 + 包日志 | 已实现 |
| `Assets/Client/Runtime/Stats/` | 数值与说明模块（Model/Gateway/Service/Format/View 五层） | 已实现（非设计稿） |
| `Assets/Client/Runtime/UI/` | UI 内核：导航 / 弹窗 / 提示 / 组合根 / 追踪器 | 已实现 |
| `Assets/Client/UI/**` | 页面资源与**场景节点引用**（部分目录为原样导入，见附录 C） | 受控维护 |
| `Assets/Client/Editor/` | 仅编辑器：结构修复、自检、表探针、WebGL 构建命令 | 已实现 |
| `Assets/Main/Init.cs` | YooAsset 初始化 + 热更程序集加载 + 客户端起始场景 | 原公司代码，**改动需单独授权** |
| `Assets/Scripts/HotFix.asmdef` | HybridCLR 热更程序集边界 | 不要直接引用其类型（见 §9） |
| `Logs/` | 全部脚本、日志、审计报告、场景回滚备份（`*.unity.bak`） | 关键资产，见 §7 |

## 2.2 权威文档地图（**按"什么时候读"用，不要全量加载**）

| 文档 | 什么时候读 | 现状 |
| --- | --- | --- |
| `AGENTS.md` | 每次开工前（硬约束） | ✅ 有效，§6 是阅读路由 |
| `DEVELOPMENT_WORKFLOW.md` | 计划/实现/修复时（七步闭环） | ✅ 有效 |
| `CURRENT_STATE.md` | 只读**顶部「交接快照」一节** | ⚠️ 权威但已膨胀到 354 KB / 1922 行，见 §2.3 |
| `BUG_TRACKER.md` | 按编号查缺陷/风险/待核验清理项 | ✅ 有效（RISK-001~020、BUG-003~027） |
| `PROJECT_OVERVIEW.md` | 跨模块设计、需要稳定技术事实时 | ✅ 有效 |
| `Assets/Client/MODULE.md`（58 KB） | 进 `Assets/Client` 前必读（§12 诊断纪律、§19 自检套件） | ✅ 有效 |
| 各子模块 `MODULE.md`（约 38 份） | 进对应目录前 | ⚠️ 有 3 份引用了不存在的代码，见 §2.3 |
| `WEBGL_PORT_BASELINE.md` | 移植侧 A0 只读基线（工作区/启动链/编译/TCP 证据/风险） | ✅ 有效 |
| `WEBGL_B0_GATE.md` / `WEBGL_B1_BUILD.md` | 迁移门禁 / 可复现构建 | ✅ 有效 |
| `WEBGL_B2_DIAGNOSTICS.md` | 真机日志采集手段与判据清单 | ✅ 有效 |
| `WEBGL_B2_REALDEVICE.md` | **真机手册**：前置条件、三条测试路径、判据、B2 矩阵、**§八 真机实测记录（2026-09-22）** | ✅ 有效（已扩写到 18 KB） |
| `WEBGL_MINIGAME_HANDBOOK.md`（18.6 KB） | **移植侧唯一入口**：权威文档索引 / 环境事实 / 一键出包 / OSS 上传规则 / 容器与真机取证 / 日志排查 / 坑清单 / 授权边界 / 未完成清单 | 🆕 2026-09-22 由另一会话建立；**移植侧以它为准**，本文 §6 是骨架索引 |
| `HOME_PAGE_SCENE_FIX_OPTIONS.md` | 「首页不显示」根因、方案 A/B 对比、**§九 实施与出包记录**、**§九 A4 跨场景宿主**、本次 OSS 重传清单 | ✅ 有效 |
| `Assets/Main/MODULE.md` | 启动模块（`Boot` / `Init` / **新增跨场景宿主**）职责、约束与一次性验证入口 | ✅ 有效，进 `Assets/Main` 前必读 |
| `DEVELOPMENT_WORKFLOW.md` | 七步闭环 + **§2026-09-22 移植阶段状态与决策记录（阶段 B 达成）** | ✅ 有效 |
| `TCP_NETWORK_BREAK_ROOTCAUSE.md` | 网络断链排查（首选） | ✅ 有效（取代 `TCP_WEBGL_HANDOFF.md`） |
| `HOME_PAGE_SCENE_FIX_OPTIONS.md` | 首页加载阻塞（已实施方案 A） | ✅ 有效 |
| `UI_ADAPT_BACKLOG.md` / `UI_ANCHOR_SIGNOFF.md` / `UI_LAYOUT_DEBUG_GUIDE.md` / `UI_ACCEPTANCE_CHECKLIST.md` | UI 适配与验收 | ✅ 有效，但**三份状态重叠**（§2.5） |
| `HANDOVER_PLAN.md` | **目标与阶段门**（A/B/C 三阶段） | ✅ 作为计划有效，**不是现状** |
| `HANDOFF_主开发1.md` | —— | ⛔ **已过期**（仍写"阶段 0 进行中"） |
| `VERSION_HISTORY.md` | 历史追溯 | ⛔ 停在 2026-08-28，被 `CURRENT_STATE.md` 取代 |
| `AI_MODEL_UPGRADE.md` | —— | ⛔ 过程存档，非交接 |
| `Notes/`、`Project Introduction/` | 学习笔记 / 面试展示 | 参考；`Project Introduction` 的阶段描述已过期 |

## 2.3 ⚠️ 已核实的"文档与现实不一致"（**照这份表，别照旧文档**）

| # | 主题 | **现实（证据）** | 旧文档说法 |
| --- | --- | --- | --- |
| 1 | 构建入口 | `ProjectSettings/EditorBuildSettings.asset` 实为 `Boot(0) → ClientShell(1)`；`ClientBootLoader.cs:9` 明写"ClientBoot 已弃用、已从构建清单移除"；`ClientWebGLBuildCommand.cs:156-159` 会在校验时**报错拦截**把 `ClientBoot` 放回去 | `CURRENT_STATE.md` 顶部快照第 14 行仍写"BUG-025 方案 C"，**而同文件后文（"移植任务交接"表）已写"清单改为 `Boot(0) → ClientShell(1)`（移除 `ClientBoot`）、`ClientBootLoader` 标注弃用"** ⇒ **同一文件内部前后不一致** |
| 2 | 页面分层 | 实测 `PagesLayer` 只有 `MainPage主页`，其余页面（英雄/编队/商店/邮件/排行/活动…）都在 `PopupLayer`；`SystemLayer` **恒为 inactive**（唯一子节点 `加载Page`）；Toast 挂在 `ClientCanvas` 下 | `CLIENT_UI_ARCHITECTURE.md` §3、`Runtime/UI/MODULE.md` 仍写"所有页面根节点应位于 `PagesLayer`"；`HANDOVER_PLAN.md` §3 同 |
| 3 | 项目阶段 | UI 适配已收口并验收、WebGL 已到 B2 真机 | `HANDOFF_主开发1.md`、`Project Introduction/Project Introduction.md` 仍写"阶段 0 进行中" |
| 4 | TCP 权威文档 | `TCP_WEBGL_HANDOFF.md` **工作区已删除**（`git status` 显示 ` D`，HEAD 里仍是完整的 105 行）；本文已把 HEAD 版本归档到 `Archive/TCP_WEBGL_HANDOFF.md` | 多份文档仍按"就在根目录"引用它 |
| 5 | `CURRENT_STATE.md` 自身 | 文件内**有两个 `# 当前项目状态`**（第 1 行 + 约第 1206 行，两次拼接）；顶部标题写 09-21 却含 09-22 内容；**行号随另一会话编辑而漂移**（现约 1943 行，2026-09-22 曾 1922 行） | 文档头自称"唯一短入口" |
| 6 | 模块文档 | `Assets/Client/UI/PrimaryPages/` 当前只检出 `MODULE.md` 文档，没有页面运行时代码；`Assets/Client/Editor/MODULE.md` 引用的 `ClientShellUiBuilder`、`ClientShellDirectUpdater` **代码不存在**；`UI/BackpackPage/MODULE.md` 引用的 `ClientBackpackPage.cs`、`ClientPageNavigator` **不存在**（背包实为 `Runtime/UI/ClientCollectionList.cs`） | 三份 `MODULE.md` 的描述 |
| 7 | 重复文档 | `-source`/`-source-clean` 根文档字节相同但 `CURRENT_STATE.md` 只有 79.8 KB（pinball 是 354.8 KB） | 副本易被误当权威 |
| 8 | 组合根/提示组件 | 静态单例 `ClientUiFeedback` 已被 `IClientFeedback` + `ClientSystemFeedback` 取代 | 旧文档仍提 `ClientUiFeedback` |
| 9 | `ClientStartScene` | `Assets/Main/Init.cs:38` 的 `ClientStartScene = ClientShell`，由 `ResolveFirstSceneName`（`:232`/`:577`）解析 → **客户端场景靠 Init 进入，不靠 Build Settings 顺序** | 多处只讲"加进 Build Settings" |

## 2.4 仓库权威性（**只有 `pinball` 是交付物**）

| 目录 | 性质 | 证据 |
| --- | --- | --- |
| `pinball` | ✅ **唯一主线/交付物**，有 remote、文档最新（09-21/22） | `https://github.com/heppyend/pinball.git`；提交差异与工作区状态请接手时实时核对 |
| `pinball-learning-code` | 已推送、工作区干净的"源码学习版"发布件 | remote `pinball-learning-code.git`，2 提交，0 脏改动 |
| `pinball-learning-source` / `-source-clean` | **中间导出存档**，**0 个提交**（无版本控制基线） | `rev-list --all --count = 0`；`Assets/Client` 各 626 文件、差集为 0 |
| `My project`（猪八戒(齐)）、`Test`、`DLLTest`、`WXBridgeSmokeTest` | 与 pinball **无关**的独立练习工程（Unity `2022.3.62f3c1`） | 无 `.git`；注意：它们在 **pinball 仓库内不存在**，别在工程里找 |
| `.lce` | 本地代码索引库（SQLite，272 MB） | 文件头 `SQLite format 3`，抽样指向 `D:\unity project\ball\...` |
| `ArrowOfTheKing.rar` | 2.66 GiB，**与 pinball 无对应关系**（创建早于 pinball 初始化） | 未解压、未列入交付 |

## 2.5 文档替代关系与合并计划

**已归档**（原件已进 `Archive/`，带指向头）：见 `Archive/README.md`。
**待合并**（内容重叠，原件保留待合并后归档）：

| 合并组 | 成员 | 结论 | 为什么不能靠摘要合并 |
| --- | --- | --- | --- |
| 真机文档 | `WEBGL_B2_DIAGNOSTICS.md` + `WEBGL_B2_REALDEVICE.md` | ✅ **已定案：不合并，按分工共存** —— `DIAGNOSTICS` = 采集手段与判据清单；`REALDEVICE` = 操作手册 + **§八 实测记录**；两者的**共同入口**是 `WEBGL_MINIGAME_HANDBOOK.md` | 合并会打散"手段 vs 记录"的边界；且两份都在被移植侧会话继续追加 |
| UI 适配 | `UI_ADAPT_BACKLOG.md`(25.4 KB) + `UI_ANCHOR_SIGNOFF.md`(8.5 KB) + `UI_ACCEPTANCE_CHECKLIST.md`(8.8 KB) | ⏸ **待合并**为一份 `UI_ADAPT_HANDBOOK.md`（客户端侧，另开会话办理） | 含**负责人手调值**与已验收结论，必须逐条搬运，不能摘要 |

> 🟠 移植侧会话已结束，但其改过的文件仍可能被第三方（负责人或新会话）继续编辑 ⇒ 动 `Archive/` 或改上述任何文件前，先看 mtime 与 `git status --short`，避免互相覆盖。

> **为什么原件不直接删除**：`pinball` 有 268 个未跟踪文件，`HANDOVER_PLAN.md`、`WEBGL_*`、`UI_*` 等**恰恰都未跟踪** ⇒ 删了**没有 git 副本可恢复**；且 `VERSION_HISTORY.md` 被 `AGENTS.md` §6 引用，移走必须同步改治理文档。故一律"归档 + 声明替代"。

---

# 3. 架构与模块接口（"有什么接口、怎么接"）

## 3.1 分层：设计意图 vs 实测现状

设计意图（`HANDOVER_PLAN.md` §3）：

```text
ClientCanvas                      ← 客户端业务 UI 的唯一宿主
├─ PagesLayer                    互斥的可导航页面
├─ PopupLayer                    可叠加、带遮罩的弹窗
└─ SystemLayer                   跨页面全局界面（Loading / Toast / 红点 / 遮罩）
```

⚠️ **实测现状与上图不一致**（见 §2.3 第 2 条）：`ClientCanvas` 下是 `PopupLayer` / `PagesLayer` / `SystemLayer` / `ToastRoot`；`PagesLayer` **只有 `MainPage主页`**，其余客户端页面都在 `PopupLayer`；`SystemLayer` **恒为 inactive**；Toast 挂在 `ClientCanvas` 下。此分层问题登记为 `BUG_TRACKER.md` **RISK-020**（A/B/C 三方案待负责人选定），**定案前不要把这张图当强制约束**。

## 3.2 接口总表（真实类型名 + 真实文件）

| 能力 | 类型 / 成员 | 文件 | 调用方式 |
| --- | --- | --- | --- |
| **组合根** | `ClientShellController` | `Runtime/UI/ClientShellController.cs` | 挂在 `ClientCanvas`；`Start()` 遍历 `GetComponentsInChildren<MonoBehaviour>(true)`，对实现 `IClientPopupHost` / `IClientFeedbackHost` 的组件注入 |
| **导航** | `IClientNavigation{ CurrentPage, CanGoBack, Open(ClientUiPageId), Back(), ReturnHome() }`、`IClientNavigationHost.BindNavigation`、`IClientHomeNavigation.OpenFromHome(ClientHomeDestination)`、`IClientHeroDetailOpener.OpenHeroDetail(ClientHero, Sprite)`、`IClientNavigationObserver.OnNavigated(ClientUiPageId)` | `Runtime/UI/IClientNavigation.cs` | 由 `ClientUiNavigator : MonoBehaviour, IClientNavigation, IClientHomeNavigation, IClientHeroDetailOpener` 实现（`Runtime/UI/ClientUiNavigator.cs`；内部 `List<ClientUiPage> _pages` + `Dictionary<ClientUiPageId, ClientUiPage>` + `Stack<ClientUiPageId> _history`）。**接口注入，不是静态单例** |
| **页面标识 / 生命周期** | `ClientUiPageId`（18 项）、`IClientPageView{ OnPageEnter/OnPagePause/OnPageResume/OnPageExit }`、基类 `ClientPageViewBase` | `Runtime/UI/ClientUiPage.cs`、`IClientPageView.cs` | 页面根节点挂实现组件，注册到导航器 |
| **弹窗栈** | `IClientPopupService`（`HasOpenPopup` / `OpenCount` / `CloseTop` / `CloseAll`）、`IClientPopupView`、`IClientPopupHost.BindPopups` | `Runtime/UI/IClientPopupService.cs`、实现 `ClientPopupService.cs`（挂 `PopupLayer`） | 接口注入；它**刻意不实现** `IClientNavigationObserver`（避免被当成页面） |
| **提示 Toast** | `IClientFeedback{ ShowToast(string), HideToast() }`、`IClientFeedbackHost.BindFeedback` | `Runtime/UI/IClientFeedback.cs`、实现 `ClientSystemFeedback.cs`（挂 `SystemLayer`） | 接口注入；已取代旧静态单例 `ClientUiFeedback` |
| **服务定位** | `ClientServices.Data` / `.Config` / `.Stats` / `.StatsGateway`；`IsInitialized` / `HasConfig` / `InitializeForDevelopment()` / `ReplaceDataService(IClientDataService)` / `Reset()` | `Runtime/Services/ClientServices.cs` | **静态入口**；未初始化时直接抛 `InvalidOperationException` |
| **玩家状态** | `IClientDataService`（含 `event Action DataChanged`） | `Runtime/Services/IClientDataService.cs` | 经 `ClientServices.Data`；实现 `LocalClientDataService` |
| **静态配置** | `IClientConfigService`（`IsReady`、`TryGetHero/Item/Skill/Potency`、`GetHeroes`、`GetCollections(kind)`…） | `Runtime/Services/IClientConfigService.cs` | 经 `ClientServices.Config`；实现 `TableClientConfigService` |
| **数值与说明** | `IClientStatsService`（`GetHeroAbilities` / `GetAttributes` / `TryGetAttribute` / `TryGetAbilityState`、`AttributesChanged` / `AbilitiesChanged`）、`IClientStatsGateway`、`IClientDescriptionFormatter` | `Runtime/Stats/{Service,Gateway,Format}/` | 经 `ClientServices.Stats`；未接服务器前由 `LocalClientStatsGateway` 扮演服务器 |
| **红点** | `ClientHomeRedDotController` | `Runtime/ClientHomeRedDotController.cs` | `ClientServices.Data.DataChanged += Refresh`；`[SerializeField] Text _mailDot/_activityDot/_noticeDot` |
| **货币** | `ClientWallet.GetBalance/SetBalance`；`IClientDataService.TrySpend(ClientCurrencyType, int)` | `Runtime/Domain/ClientModels.cs`、`Services/LocalClientDataService.cs` | 页面只读钱包展示；**扣费一律走服务方法**，页面不得自行改写 |
| **表源** | `ClientTableSource`（`DescribeSource()` / `UsingPackagedTables`） | `Runtime/Services/ClientTableSource.cs` | 静态 |
| **服务端包日志** | `ClientServerPacketRouter` / `ClientServerPcSession` | `Runtime/Services/` | 写 `Application.persistentDataPath/ClientServerLogs`（JSONL + `.protocol.log`） |
| **列表范式** | `ClientHeroCard`（`Apply(string,int,bool,Sprite,Action)`、`SetTeamSlot`）、`ClientHeroCardList`（`Show(...)`） | `Assets/Client/UI/HeroPage` 相关 + `Runtime` | "1 个模板 + N 个实例，实例数恒等于数据条数" |
| **平台边界** | `Assets/Scripts/Platform/WeChatMiniProgram` 契约 + 默认回退 | `Assets/Scripts/Platform/WeChatMiniProgram/` | SDK 适配器**未接入** |
| **临时诊断（例外）** | `ClientTcpConnectionProbe` | `Runtime/ClientTcpConnectionProbe.cs` | 通过反射调用既有 `NetController.GetIpPort` 并读 `SysDefines.Ip/Port`；**不得形成 AOT→HotFix 编译依赖，不得被业务 UI 使用** |

> 想重新生成方法级清单：`grep -n "public .*(" Assets/Client/Runtime/Services/IClientDataService.cs`。

## 3.3 导航与生命周期语义（新人最容易做错的地方）

- 主页入口由 `ClientHomePage` 统一发出 `NavigationRequested`，由 `ClientUiNavigator` 转成页面 ID。**不要在各页面自己写跳转。**
- 返回统一用 `ClientUiBackButton` → `ClientUiNavigator.Back()`。**不要写"返回主页"的业务逻辑**（会破坏返回栈）。
- 页面根节点必须预先存在于 `ClientShell` 场景中，并注册到导航器。**禁止运行时创建业务 UI 根节点**，也禁止用散落的 `SetActive` 绕开导航栈。
- 弹窗之间的层级**只由** `ClientPopupService.ApplyStackOrder()` 决定；页面 View **不得**自行 `SetActive` 根节点、`SetAsLastSibling`、改 `overrideSorting` / `sortingOrder`。
- `SystemLayer/加载Page` **不注册为可导航页**，由 `ClientLoadingPage` 独立控制显隐与进度。

## 3.4 页面与节点

- 场景：`Assets/Client/Scenes/ClientShell.unity`；`ClientCanvas` 挂 4 个自有组件：`ClientUiNavigator`、`ClientShellController`、`ClientPopupService`、`ClientSystemFeedback`。
- 场景内页面节点（23 个，含二级弹窗），例：`MainPage主页`、`ProfilePage个人中心`、`HeroPage英雄`、`HeroDetailPage英雄详情主页`、`ShopPage商店`、`RankPage排行榜`、`ActivityPage活动`、`FormationPage编队`、`MailPage邮件`、`Complete Guide Event Page全图鉴活动`、`加载Page`。
- `ClientUiPageId` 共 18 项（`Runtime/UI/ClientUiPage.cs`）。
- 导航流示例：`MainPage → ProfilePage → HomeShowcasePage / BadgePage`；`HeroPage → HeroDetailPage → HeroEnhancePage`；`GachaPage → GachaResultPopup`（仍停留在 GachaPage）。

## 3.5 三条"怎么接"的路径

**路径 A：新增一个页面（7 步，`CLIENT_UI_ARCHITECTURE.md` §10 同）**
1. 在 `ClientShell` 的场景分层中确认页面根节点**已存在**（没有就先和负责人确认是否授权写场景）。
2. 页面根挂 `ClientUiPage`（或继承 `ClientPageViewBase`）。
3. 在 `ClientUiPageId` 增加枚举项。
4. 在 `ClientUiNavigator` 的注册表/导航映射里登记。
5. 写页面控制器 `Client*Page.cs`，只做"读共享数据 → 响应按钮 → 刷新既有节点"。
6. 在主页或上级页面加入口（走 `ClientHomePage.NavigationRequested` 或上级页面的导航请求）。
7. 弹窗放 `PopupLayer`，Toast/Loading 放 `SystemLayer`；然后编译 + 自检 + 人工 Play。

**路径 B：接入真实服务端（3 步）**
1. 新增 `IClientDataService` 的网络实现或适配层（不要在页面里写 HTTP/协议）。
2. 通过 `ClientServices.ReplaceDataService(...)` 注入。
3. 保持接口语义与 `DataChanged` 通知一致 ⇒ **页面控制器与 Canvas 层级不动**。
   （数值侧同理：实现 `IClientStatsGateway` 替换 `LocalClientStatsGateway`。）

**路径 C：接入真实美术资源 —— ⚠️ 通道尚不存在**
"资源名/图片 ID → Sprite"的解析器**目前没有实现**（`BUG_TRACKER.md` BUG-023），当前多为美术占位或 `fileID 0` 未接线字段。已知缺口：`HeroAvatar_*` / `AvatarFrame_*` / `Medal_*` / `AttributeIcon_*` 四类缺图，且 `ElementIcon_*` 缺元素 6（暗）。

## 3.6 列表范式（照抄 `HeroPage` 的模式）

- `ClientHeroCard`（挂卡牌模板）是**单张卡牌的唯一管理者**：名称/等级/立绘/锁/遮罩/编队编号/点击都在这里；刻意**不依赖领域模型**，`Apply(string, int, bool, Sprite, Action)` 只收基础类型 ⇒ 任何"有名字/等级/解锁状态"的列表都能复用。
- `ClientHeroCardList`（非 MonoBehaviour）管"1 模板 + N 实例"，**实例数恒等于数据条数**（不足新建、多余隐藏）⇒ 杜绝 2→4→6 累积。
- 容器必须自带布局组件（`GridLayoutGroup`/`VerticalLayoutGroup`/`HorizontalLayoutGroup`）；配 `ContentSizeFitter` 时锚点必须**单向拉伸**（`anchorMin=(0,1)`、`anchorMax=(1,1)`、`pivot=(0.5,1)`），双向拉伸会让 `ContentSizeFitter` 完全失效、列表滚不动。
- 禁止在场景里内联展开列表项；场景只保留 1 个禁用的模板。

## 3.7 模块间禁止事项（违反即为缺陷）

- 页面只通过 `ClientServices` 暴露的接口读写状态；**不得**直接调用旧 `NetController` / `WebWork` / `NetWork`。
- `Assets/Client` **不得直接引用 `HotFix` 程序集类型**（会复发 `IL1005` / 读到旧成员，`RISK-018`）。
- 不得在 `Update` 中轮询业务状态或隐式全局查找；用明确的绑定/刷新入口，退出时解绑。
- 不得把运营内容与数值硬编码在 UI 回调里（必须数据驱动）。

---

# 4. 数据流：表、模拟服务、数值模块

## 4.1 装配顺序（有坑，别改）

`ClientServices.InitializeForDevelopment()`（`Runtime/Services/ClientServices.cs:67`）顺序**必须**是：

```text
IClientConfigService (TableClientConfigService)      ← 先
  → IClientDataService (LocalClientDataService)      ← 后（构造时要用配置表建英雄列表）
  → LocalClientStatsGateway(Config)
  → ClientStatsService(Config, Gateway, Formatter)
```

- 顺序写反的后果（源码注释里明确写了）：**"配置能读到 49 个英雄，数据服务却报英雄配置为空"**。
- `_config.IsReady` 会被**刻意立刻触碰一次**，让表在启动时加载并打印行数；否则表加载失败时底层只打一条警告就"按空表继续"，Console 里什么都看不到。
- 退出 Play / 域重载后由 `ClientServices.Reset()` 清空缓存（含 Stats 的页签与说明文本缓存）。

## 4.2 玩家状态接口（`IClientDataService`，按业务分组）

| 业务 | 代表成员 |
| --- | --- |
| 账户/钱包 | `GetProfile()`、`GetWallet()`、`TrySpend(currency, amount)` |
| 英雄 | `GetHeroes()`、`GetHeroDetail(heroId)`、`GetHeroAbilityUpgradePreview(...)`、`TryUpgradeHeroAbility(heroId, abilityType, out reason)` |
| 背包 | `GetInventory()` |
| 扭蛋 | `GetGachaPool()`、`GetGachaHistory()`、`TryDrawGacha(poolId, drawCount, out result)` |
| 商城 | `GetShopListings()`、`GetShopPurchasedQuantity(listingId)`、`TryPurchaseShopListing(listingId, quantity, out reason)` |
| 邮件 | `GetMails()`、`TryReadMail(mailId)`、`TryClaimMail(mailId, out reason)`、`ClaimAllMails(out reason)`、`TryDeleteAllMails(out reason)` |
| 排行 | `GetLeaderboardEntries(kind)`、`GetPlayerLineup()` |
| 活动/任务 | `GetActivities()`、`TryClaimActivity(...)`、`GetActivityTasks(category)`、`TryClaimActivityTask(category, taskId, out result)`、`GetCompleteGuideEvent()`、`TryClaimCompleteGuideEvent(eventId, out result)` |
| 任务/公告/弹珠 | `GetTasks()`、`GetNotices()`、`GetMarbles()` |
| 收藏品/展示 | `GetOwnedCollectionIds(kind)`、`GetEquippedCollectionId(kind)`、`TryEquipCollection(kind, id)`、`GetShowcaseHeroId()`、`GetEquippedBadgeId()` |

**业务规则要点**（都由服务实现，页面不得复制）：
- 邮件"全部删除"必须先"全部领取 + 全部已读"，否则返回 false 并给原因（2026-09-21 负责人确认）。
- 活动任务按 `category` 分类读取，`TryClaimActivityTask` 校验分类/完成度/重复领取，返回"拒绝 / 处理中 / 已发放"。
- 本地模拟初始值：`Gold 1000 / Diamond 100 / Energy 20`。

## 4.3 静态配置（表驱动）

- **只有 10 张表**走 `IClientConfigService`：`Hero`、`Head`、`HeadFrame`、`Badge`、`Nameplate`、`Title`、`Item`、`Skill`、`Potency`、`RandomHero`（`PotencyLevel` 由 Stats 侧读取）。
- 表源优先级（`Runtime/Services/ClientTableSource.cs`）：
  1. `{Application.persistentDataPath}/config.pkg`（GZip，格式 `[version=1][表名\0][内容\0]…`，由启动流程落盘；仓库内另有 `Assets/pkg/config.pkg`）；
  2. 兜底 `Resources/Table/<表名>.json`（**实有 21 个 json**：Audios, Badge, Coefficient, Dan, Energy, Head, HeadFrame, Hero, Item, MapCopy, Monster, MonsterTemplate, Nameplate, PinBallRoom, Potency, PotencyLevel, RandomHero, Resources, ServerControl, Skill, Title）。
- ⚠️ **`config.pkg` 优先于本地 json** ⇒ "新表放了不生效"的经典坑（`BUG-021`）。**不要为了调试去改 `ClientTableSource` 的优先级**。
- ⚠️ 刻意**不引用** HotFix 的 `T*Helper` / `TableManager`（避免 `IL1005` 与 `BUG-022` 的 99 条解析报错）。

## 4.4 ⚠️ 这些"看起来该是表"的东西**不是表**

`商店`、`活动`、`任务`、`邮件`、`公告`、`卡池`、`文案`、`货币` 的数据目前**写在 `LocalClientDataService` 里**（本地模拟），货币是 `ClientCurrencyType` 枚举 + 钱包余额。**不要去找不存在的表**；接入服务端时它们由 `IClientDataService` 的实现提供。

## 4.5 数值与说明模块（`Stats`）—— 负责人定的架构原则

> 原话（2026-09-20）：*数据未来只由权威服务器计算并分发，客户端负责"属性接口""技能/天赋数值接口"和"逻辑说明显示"，单独做一个模块管理，贴近 MVC，强解耦、拓展性强、维护方便、GC 消耗少。*

**硬约束（由"服务器权威"推出，优先级最高）**：
1. **客户端不做任何数值计算** —— 战力、属性加成、技能效果值、冷却、概率、经验一律取服务器下发值；接口签名里**不得出现 `Calc*` 形态的方法**。
2. **配置表在客户端只有两个用途**：① 展示用的名称/图标/说明文本；② 结构定义（有哪些技能/天赋、归属哪个页签）。**表不作为数值来源**（`Effect1`、`Hero.HpBase` 是服务端计算输入，客户端不解析、不套公式）。
3. 本地模拟层只扮演服务器：`LocalClientStatsGateway` 产生"权威快照"，**不得被 View 直接依赖**；接服务端时整体替换，View 一行不改。

**目录与依赖方向（违反即为缺陷）**：

```text
Assets/Client/Runtime/Stats/
├─ Model/    纯 C#，零 UnityEngine 依赖（ClientAttributeIds / ClientAttributeValue /
│            ClientAbilityKind / ClientHeroAbilitySection / ClientAbilityEntry /
│            ClientAbilitySection / ClientHeroAbilitySet / ClientAbilityState）
├─ Gateway/  IClientStatsGateway + LocalClientStatsGateway（唯一"入口"）
├─ Service/  IClientStatsService + ClientStatsService（权威数据唯一持有者，原地更新 + 广播 + 零分配查询）
├─ Format/   IClientDescriptionFormatter + ClientDescriptionFormatter（说明文本唯一落点）
└─ View/     ClientAttributeRowView / ClientAbilitySectionView / ClientAbilityEntryView / ClientIconResolver

依赖方向：Gateway → Service → (Controller) → View        配置表只被 Service/Format 读取
```

- **View 不引用** Service/Gateway/配置表 ⇒ 可单独预览、可复用、可换皮；**Service 不引用**任何 Unity UI 类型。
- 变更通知走事件（**禁止 `Update` 轮询**）。

## 4.6 id 类型约定（2026-09-20 负责人定）

- 与配置表对应的实体 id 一律 **`int`**（英雄、道具、头像/头像框/徽章/铭牌/称号、技能/潜能）。
- 服务端自有的不透明标识仍是 **`string`**（订单、邮件、活动、任务、公告、卡池、商品、玩家）。
- **美术资源名**也是 **`string`**（如 `QualityIcon_3`、`HeroPortrait_13001_c`）。

---

# 5. 启动链与资源链

## 5.1 现状链路（**这一节是移植侧的地基，务必按现状读**）

```text
Build Settings:  Assets/Main/Boot.unity (index 0)   →   Assets/Client/Scenes/ClientShell.unity (index 1)
                        │
                   Assets/Main/Init.cs
                   ├─ YooAssets.Initialize()                                (:121 / :551)
                   ├─ CreatePackage("DefaultPackage") + 四种运行模式分支    (:104–:169)
                   ├─ Assembly.Load(热更 DLL)                                (:216 / :272)
                   ├─ RuntimeApi.LoadMetadataForAOTAssembly(...)             (:313)
                   ├─ ClientStartScene = "ClientShell" (:38) → ResolveFirstSceneName()（显式配置优先，否则回落 LoginScene）
                   └─ LoadFirstSceneAsync(sceneName)
                        ├─ 内置场景（已进 Build Settings）→ SceneManager.LoadSceneAsync   ← 当前走这条
                        └─ 回落 → YooAssets.LoadSceneAsync（按地址）
                        │
                   【跨场景宿主】WebGLPostSceneLoadRunner（Init 在触发加载**之前**创建并 DontDestroyOnLoad）
                        └─ 订阅 SceneManager.sceneLoaded → 切换完成后调用一次
                           Init.RunPostSceneLoadCompatibility()（WebGL 相机 HDR/后处理兼容 + 首屏诊断）→ 自毁
```

- **为什么 `ClientShell` 必须走原生加载**：它由 Build Settings 打进播放器，**不在 YooAsset 收集器范围内**，按地址加载必然失败（`Failed to mapping location to asset path : ClientShell`）。方案 A 的取舍见 `HOME_PAGE_SCENE_FIX_OPTIONS.md`（§三 方案 A / §四 方案 B / §九 实施与出包记录）。
- **为什么需要跨场景宿主（A4，2026-09-22）**：`LoadSceneMode.Single` 卸载 `Boot` 场景时 `Init` 及其协程一并销毁 ⇒ 写在协程 `yield return` **之后**的代码**永远不会执行**（容器日志实测该分支 0 命中）。
- ⛔ **`ClientBoot` 方案已废弃**：`ClientBootLoader` 用 `SceneManager.LoadScene(name)`（单参 = `Single`）**替换**了场景 ⇒ `Boot` 永不加载 ⇒ `Init` 不跑 ⇒ YooAsset 未初始化 ⇒ `Assembly.Load(HotFix)` 不执行 ⇒ `NetController` 不在 AppDomain ⇒ TCP 探针 15 秒超时。改用 `LoadSceneMode.Additive` 也必须先论证（`ClientBootLoader.cs:32`）。
- ⛔ `ClientWebGLBuildCommand.cs:122,156-159` 里有**启动链校验**：若发现 `ClientBoot` 回到构建清单会直接 `LogError` 拦截。
- ⚠️ **三条硬约束**（`Assets/Main/MODULE.md`）：
  1. **不要把 TCP 探针移出 `Boot`** —— 它靠反射访问 HotFix 里的 `NetController` / `SysDefines`，而 HotFix **只有 `Boot/Init` 这条 YooAsset 链能加载**；它自身用 `StartPersistentProbe()` 迁移到 `DontDestroyOnLoad` 以存活过场景切换。
  2. **不要把 `Init` 改成 `DontDestroyOnLoad`** —— `StartPersistentProbe` 明确依赖"Boot 场景会被卸载"这一前提。
  3. **`WebGLPostSceneLoadRunner` 不得声明任何 `[SerializeField]` 字段** —— 字段布局在编辑器/播放器间不一致会让 Unity **直接拒绝构建**。
- 🔒 **逐次授权文件**（改前必须逐一确认，见 §11.2）：`Assets/Main/Init.cs`、`Assets/Main/Boot.unity`、`ProjectSettings/EditorBuildSettings.asset`、`Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset`。
- 🧹 `WebGLFirstScreenDiagnosticsHost` 属**可回收诊断代码**：真机与容器稳定后可整体移除（`Assets/Main/MODULE.md` §待确认）。

## 5.2 运行模式（`Init.cs:104–169`）

`EditorSimulateMode` / `OfflinePlayMode` / `HostPlayMode` / `WebPlayMode` 四种分支。已知历史坑：微信小游戏运行时**误用 YooAsset Editor Simulate 模式**（`BUG-005`）。

## 5.3 远端资源 URL 与 `/Assets` 层级（**踩过两次的坑**）

- YooAsset 远端拼接是 `GetRemoteMainURL(fileName) => $"{host}/{fileName}"`（`Init.cs:476-482`），`host` 指向 **CDN 根**，**不是 `/Assets/`**。
- 那个 `/Assets` 层级是**微信 SDK 首包**（`webgl.data` / `wasm` / 纹理）用的，**与 YooAsset 无关**。
- 同一个路径错配**踩了两次**：先"少一层"（`BUG-015`）、后"多一层"（53 个文件 404）。
  ⇒ **纪律：上传后必须按客户端实际请求的 URL 逐个自检，不能靠推断。**
- 改 CDN 目录**必须同时改两处**：`MiniGameConfig.asset` 的 `CDN` 与 `Boot.unity` 的 `DefaultHostServer` / `FallbackHostServer`；改完**必须重跑转换**（`game.js` 里写死了 `DATA_CDN`）。
- 只调 UI 时，重跑构建后通常**只需重传 `data.br` 一个新文件**（未改 C# 时 `wasm.br` 哈希不变）。

## 5.4 HybridCLR

| 项 | 值 |
| --- | --- |
| 设置 | `ProjectSettings/HybridCLRSettings.asset`（`enable: 1`；`hotUpdateAssemblyDefinitions` = `HotFix.asmdef`；`patchAOTAssemblies` = mscorlib / System / System.Core / Demigiant / YooAsset / LC.Newtonsoft.Json；`outputLinkFile: HybridCLRGenerate/link.xml`） |
| 热更目标 | `Assets/Scripts/HotFix.asmdef`（GUID `5e9815524708f8a449b9b0e0cc4cd749`） |
| 加载点 | `Assets/Main/Init.cs:272`、`:313` |
| 构建入口（编辑器） | `Assets/Editor/HybridCLR/{AssetBundleBuildCommand, BuildAssetsCommand, BuildPlayerCommand, WebGLHotUpdateDllCommand}.cs` |
| 包依赖 | `com.code-philosophy.hybridclr`（**gitee** 依赖） |
| 子模块 | `HybridCLRData/hybridclr_repo`、`HybridCLRData/il2cpp_plus_repo`（**当前未初始化**，`git submodule status` 前缀为 `-`） |
| 生成物 | `HybridCLRData/`（17117 文件）、`HybridCLRGenerate/`、`HybridCLRBuildCache/` ⇒ **勿手改** |

## 5.5 XLua：客户端零依赖

`Assets/Client` 与本次客户端功能**完全没有 XLua 依赖**。C# 侧仅旧代码 3 处（`Assets/Scripts/Common/AnimPlayer.cs:4`、`Assets/Scripts/CSharp/HotUpdate/{Util,Context}.cs`、`BuildInInit.cs`），且 `Assets/Scripts/AppRoot.cs:158/190/198/207` 的 `XLuaMain` 相关代码**已全部注释**。原生源码在 `WebGLPlugins/`（65 文件，第三方）、`build/`（824 文件，第三方）。**XLua 是否仍在该项目运行时启用：未确认。**

## 5.6 资源与产物目录现状

| 目录 | 现实 | 状态 |
| --- | --- | --- |
| `Bundles/`（226 文件） | **YooAsset 输出**：`Bundles/{WebGL,Android,StandaloneWindows64}/{包名}/{版本}` | 当前链路的产物 |
| `AssetBundles/`、`StreamingAssets(43)`、`StreamingAssetsPublish(45)` | **旧 AssetBundle 体系**输出，由 `Assets/Editor/AssetBundle/AssetBundleTool.cs`、`Assets/Editor/Packager.cs` 复制 | ⚠️ **看不到独立的发布 `.ps1`；与新 YooAsset 链的关系未确认** |
| `TextToolDatas/`（130 文件） | 116 张 png + `FIRSTBUNDLETEXTURES.json` | ⚠️ 产出者与发布依赖**未确认**（BUG_TRACKER 记未闭环） |
| `YooAsset 正式收集器/分包配置` | `Assets` 下只见到样例 `Assets/Main/Samples/Space Shooter/AssetSetting/AssetBundleCollectorSetting.asset` | ⚠️ **正式配置位置未确认** |
| `KeyStores/xiaoshikeji.keystore` | Android 签名密钥 | 🔒 **敏感，不随仓库流转**（见 §11.3） |

---

# 6. WebGL / 微信小游戏移植

> 📌 **移植侧总入口是 `WEBGL_MINIGAME_HANDBOOK.md`**（2026-09-22 由另一会话建立：权威文档索引 / 环境事实 / 一键出包 / OSS 上传规则 / 容器与真机取证 / 日志排查 / 坑清单 / 授权边界 / 未完成清单）。**本节只做骨架索引与交叉引用，不替代它。**
>
> ⚠️ **本节定位：骨架 + 已取证事实 + 移植经过（§6.7）。** 所有结论都来自权威记录，**不凭现象推断**；`AGENTS.md` §4 明文要求：WebGL/微信的黑屏、卡顿、资源或渲染问题必须先引用微信官方文档、SDK 实现或最小可回收诊断日志取得证据，证据不足只能提"采集项"。

## 6.1 工具链版本（已留档，`WEBGL_B0_GATE.md` §1）

| 组件 | 版本 / 位置 |
| --- | --- |
| Unity | `2022.3.57f1c2`，`C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe` |
| URP | `com.unity.render-pipelines.universal` **14.0.11** |
| TextMeshPro | `com.unity.textmeshpro` **3.0.7** |
| HybridCLR | `com.code-philosophy.hybridclr`（gitee git 依赖，无版本号） |
| XLua | 工程内集成（非包），WebGL 适配 `Assets/Plugins/WebGL/xlua_webgl.cpp` |
| 微信小游戏 SDK | `com.qq.weixin.minigame` **0.1.1**（`Assets/WX-WASM-SDK-V2/package.json`） |
| 转换入口（GUI） | `Assets/WX-WASM-SDK-V2/Editor/`：`WXEditorWindow.cs` / `WXConvertCore.cs` / `WXMultiPackageMergeWindow.cs` / `WXEditorSettingHelper.cs` |

## 6.2 B0 迁移前门禁（5 项，`WEBGL_B0_GATE.md`）

| 类别 | 结论 |
| --- | --- |
| 工具链版本 | ✅ 已留档（§6.1） |
| 构建入口 | 见 §5.1（`Boot → ClientShell`） |
| XLua WebGL 适配 | ✅ 适配物存在（`Assets/Plugins/WebGL/xlua_webgl.cpp`） |
| WebGL Player 设置 | ⚠️ 已留档但有两处风险：引擎代码剥离=关（体积）、压缩=Disabled |
| 首包 / 分包 / 远端资源 | ⚠️ 待确认（YooAsset 策略未定稿） |
| 真机环境 | ⏸ 待负责人提供（开发者工具版本/路径、AppID、设备与微信版本、日志回传方式） |

`ProjectSettings.asset` 相关实测值：`webGLMemorySize: 256`、`webGLInitialMemorySize: 32`、`webGLCompressionFormat: 2`（Disabled）、`webGLThreadsSupport: 0`（关，与微信小游戏一致）、`stripEngineCode: 0`、`apiCompatibilityLevel: 6`、`allowUnsafeCode: 1`。

## 6.3 B1 可复现构建链路 —— ✅ 已达成（2026-09-22）

```text
① 客户端侧验证（代理可跑，必做）
② HybridCLR 生成（代理可跑）
③ YooAsset 资源包构建                [GUI]
④ Unity WebGL 构建                   [GUI]
⑤ 微信小游戏转换                     [GUI]
⑥ 开发者工具本地预览                 [GUI]
⑦⑧ 真机与归档
```

**唯一推荐入口（一键出包，两阶段）**：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File 'D:\unity project\pinball\Logs\webgl-port\run-pipeline-2phase.ps1'
```

| 要点 | 说明 |
| --- | --- |
| **前置** | **必须先关闭 Unity 编辑器**（脚本会强杀残留 Unity 进程 `Stop-UnityLeftovers`，编辑器内未保存改动会丢） |
| 两阶段原因 | 阶段 A 编译 WebGL 程序集 + 同步 `HotFix.dll.bytes`，再 `AssetDatabase.Refresh()` 触发域重载（**必须跨进程**），否则下一次构建报 `script class layout is incompatible between the editor and the player` |
| 阶段 A / B | A：编译 + 同步热更 DLL；B：YooAsset 资源包 → WebGL 构建 → 微信转换 |
| 实测耗时 | A 约 1.3~1.8 min，B 约 7.3~7.9 min，**合计约 9.2 min** |
| 版本号 | 沿用已有版本（如 `2026-09-22-1420`）⇒ bundle 内容不变，**上传面最小** |
| 成功判据 | 末行 `All requested phases succeeded.` + 日志内 `4/4 WXConvertCore.DoExport 返回：SUCCEED` |
| 日志 / 产物 | `Logs/b1-phaseA-<时间戳>.log`、`Logs/b1-phaseB-<时间戳>.log`；YooAsset 产物 `Bundles/WebGL/DefaultPackage/<版本>/` |

**构建日志里必须复核的三行**（缺一即视为出包不完整）：

```text
[启动链校验] 构建清单场景数 = 2
[启动链校验]   场景[1] enabled=True path=Assets/Client/Scenes/ClientShell.unity
[Builder] Scenes [1]: Assets/Client/Scenes/ClientShell.unity, [x]
```

> 分步主管线（调试用，步骤 0–5：启动链校验 → 自检 → 配置表基线 → YooAsset 打包 → 热更 DLL → WebGL 构建 + 微信转换）：
> `pwsh -File Logs\webgl-port\run-webgl-port.ps1 [-UntilStep 2] [-PackageVersion v]`。细节见 `WEBGL_B1_BUILD.md` 与 `WEBGL_MINIGAME_HANDBOOK.md` §2。
> 另一种编译校验写法（覆盖新增文件）：`verify-compile.ps1 -Profile All -DiscoverRoot 'Assets\Main'`。

## 6.4 B2 真机验证 —— ✅ 首轮通过（2026-09-22）

- **构建标识**：取自 `minigame/game.js` 的 `DATA_FILE_MD5` + `CODE_FILE_MD5`（每次出包都会变，**必须逐次记录**）。首轮实测值：`DATA=356e1b81e1be0bb4`、`CODE=2edcf1b6d32ca4ab`。
- **三条硬前提**（`WEBGL_MINIGAME_HANDBOOK.md` §5）：
  1. **手机与网关同一局域网**（发现服务就是裸内网 IP；手机浏览器能拿到 `<error_response><code>2</code><msg>请求无效</msg></error_response>` 即为通）；
  2. **手机必须开启「调试」** —— A/B 实测：不开被 `downloadFile:fail url not in domain list` 拦；开则放行，且该开关覆盖 `downloadFile` / `request` / `socket`（裸 IP 的发现服务与 TCP 网关一并放行）；
  3. **首包较大**：`data` 约 **36.5 MB**（从 OSS 拉）+ `wasm` 约 **15.3 MB**（随包下发）⇒ 首次进入慢，**别中途退出**。
- **别混淆两个「调试」**：开发者工具里的「不校验合法域名…」（`project.config.json` 的 `urlCheck: false`）**只对模拟器生效，真机不读**；手机右上角「…」→「调试」才是真机唯一的跳过开关。
- **进入方式不止扫码**：预览（首次要扫）／`auto-preview` 自动推送到同账号微信／**再次进入免扫码**（微信「发现 → 小游戏 → 最近使用」或转发卡片点开）／体验版（需配合法域名）／真机调试 2.0（日志回传 IDE，取证最方便）。
- **首轮实测结果（Android 1080×2400，预览版）**：域名校验 🔴→✅（开调试后）· 首页正常渲染（立绘/货币条/功能按钮/底部页签齐全）· 首页 → 编队页（6 张英雄卡，LV/稀有度/名称，编组 01）→ 新手任务页（三条任务，进度 1/1、2/3、0/1）· **TCP 六包全部走完**（`英雄=4，英雄组=1`）· `RT-FPS 50~63 / Jank 0 / BigJank 0 / Stutter 0.00%` · 无致命异常（仅已知非致命 WARNING，如 ETC 纹理回退）· **内存峰值未采集**。
- **生产含义（未完成，需服务端配合）**：正式版/体验版**没有**调试开关 ⇒ ① OSS 域名加 `downloadFile` / `request` 合法域名（HTTPS + 已备案）；② 发现服务与 TCP 网关是**裸 IP + HTTP，加不进白名单** ⇒ 需服务端提供 HTTPS 域名（`BUG-028` / `RISK-021`）。
- **判据（先看这四条，再看矩阵）**：`WEBGL_B2_REALDEVICE.md` §三。
- **回归矩阵 7 类**：启动 / UI / 导航 / 数据 / 资源 / 平台兼容 / 稳定性 —— 每条记录设备型号、系统、微信版本、构建标识、复现步骤、截图或日志路径、结论、下一动作。**未测试不能标记通过。**
- **已知非致命项**：`WEBGL_B2_REALDEVICE.md` §五（真机上看到不要误判为新缺陷）。
- **失败排查顺序**：先环境、后代码（同文 §六）；**回传模板**见同文 §七。
- **日志通路**：`WX.SetEnableDebug` / `WX.GetLogManager` / `WX.GetRealtimeLogManager`（`WX.cs:2294 / 4507 / 4519`）；采集项清单 `WEBGL_B2_DIAGNOSTICS.md`。⚠️ **真机日志拉取脚本不存在**（只有 API 通路，无自动化脚本）。
- ⛔ **已验证的死路**：不要用 `miniprogram-automator` 读小游戏 Console —— `cli auto` 能握手但 `evaluate`/`systemInfo`/`currentPage` 全部超时，且小游戏**没有 `getConsoleMessages` API**。另：用 `cli auto` 时不要连带 kill 父进程（会杀掉开发者工具主进程）。
- 微信开发者工具的**服务端口开关**（设置 → 安全设置 → 服务端口）必须开着 `cli` 才能用；但即使开着，对小游戏也只能握手、不能查数据。

### 6.4.1 容器（微信开发者工具）取证判据

1. 打开**本次导出目录**的 `minigame`（**必须是本次导出目录**：首包文件名随构建变化）。
2. 详情 → 本地设置 → 勾「不校验合法域名…」（只对模拟器生效）。
3. 编译 ▾ → 清缓存 → **全部清除** → 重新编译。
4. Console 应出现（缺一即视为未真正跑起来）：

```text
首个场景：ClientShell（ClientStartScene=ClientShell）
首个场景加载方式判定：构建清单场景数=2，目标=ClientShell
首个场景已加载（sceneLoaded 事件）：ClientShell，模式=Single        ← A4 跨场景宿主生效
WebGL 渲染兼容性已注册：已检查 N 台相机                             ← 加载后兼容处理执行
[WebGL 首屏诊断 1s] / [10s] scene=…, cameras=…, canvases=…          ← 首屏诊断在跑
全文件 0 处：Failed to mapping location to asset path / Failed to load scene
[配置表] … → ActiveVisualCanvases 含 ClientCanvas/PagesLayer/MainPage主页
[TCP客户端] 网关成功 → TCP 建连成功 → 握手成功 → 游客登录成功 → 英雄数据解析成功 → 成功退出
```

### 6.4.2 OSS 上传规则（**踩过两次的坑**）

| 类型 | 客户端请求的路径 | 依据 |
| --- | --- | --- |
| **YooAsset**（清单 + bundle） | `<CDN>/<fileName>` = **根目录** | `Init.cs` `RemoteServices.GetRemoteMainURL(fileName) => $"{host}/{fileName}"` |
| **微信 SDK 首包**（data / wasm） | `<CDN>/<文件名>` = **根目录**（`dataFileSubPrefix: ''`） | `minigame/unity-namespace.js:25`；文件名由 `game.js` 的 `DATA_FILE_MD5` / `CODE_FILE_MD5` 决定 |
| 微信 SDK 的**纹理/音频**流式加载 | `<CDN>/Assets/…` | `game.js` `gameManager.assetPath = DATA_CDN + '/Assets'`（**当前未启用**，实测该路径 404 无害） |
| YooAsset 的**内置版本探测** | `<CDN>/StreamingAssets/yoo/DefaultPackage/PackageManifest_DefaultPackage.version` | 实测 **404 非致命**（YooAsset 随后从根目录取到版本） |

- **怎么知道要传哪几个文件（实测方法，别猜）**：出包前落一份导出物哈希快照 → 出包后重算 `webgl\` 与 `minigame\` 下所有文件的 `SHA256`(前 16 位) → 比对出"新增/变化/消失"→ **只传变化的**。现成产物：`Logs/webgl-port/_snapshot-before-planA.txt`、`_snapshot-after-A4.txt`。
- **两条实测规律**：改 **UI 资源**（不动 C#）⇒ YooAsset bundle + manifest 变，传 YooAsset 增量 + 首包 `data.br`；改 **C#**（含 `Init.cs`）⇒ `Assembly-CSharp` 是 AOT 主程序集（`Assets/Client` 无 asmdef）⇒ **`webgl.wasm` 与 `data` 都变**，首包两个文件都要重传（本会话两次实测：150 个文件里 149 个逐字节相同，仅本地 `BuildReport_*.json` 变）。
- **上传后自检必须全部 200**；上传需 OSS 凭据（**代理没有**），由负责人执行；清单模板 `Logs/webgl-port/OSS_UPLOAD_MANIFEST*.txt`。

## 6.5 证据归档命名约定

| 类型 | 位置 |
| --- | --- |
| 出包（阶段 A / B） | `Logs/b1-phaseA-<时间戳>.log`、`Logs/b1-phaseB-<时间戳>.log` |
| 分步主管线 | `Logs/b1-step{N}-{name}-{yyyyMMdd-HHmmss}.log` + 同名 `.console.txt` |
| 启动链校验 | 阶段 B 日志内的 `[启动链校验]` 段（三行必查，见 §6.3） |
| 编译校验 | `Logs/verify-compile.ps1` → 产物与日志在 `Logs/_verify/` |
| 导出物哈希快照 | `Logs/webgl-port/_snapshot-*.txt`（判断"要重传哪几个文件"的依据） |
| OSS 上传清单 | `Logs/webgl-port/OSS_UPLOAD_MANIFEST*.txt`（含路径、逐项文件名、自检 URL） |
| 容器 / 真机运行日志 | 微信开发者工具 Console（可导出）；vConsole（开调试后）；真机调试 2.0 回传 IDE；体验版用公众平台「实时日志」 |
| 管线产物 | `Bundles/WebGL/DefaultPackage/<版本>/`；微信导出目录另存 |

## 6.6 环境值在哪、还缺什么

**已在 `WEBGL_MINIGAME_HANDBOOK.md` §1 记录（照抄即可复现）**：工程路径、Unity 版本、基础设施版本（含 **YooAsset 2.1.1**）、小游戏 **AppID**、OSS **Bucket / Endpoint**、**CDN 目录**、**导出目录**、微信开发者工具路径、**网关发现服务与 TCP 网关地址**、区服 `ZoneId`、部署机。

> ⚠️ **需要负责人裁决的一处张力**：该手册把 **AppID / OSS Bucket / 网关裸 IP 与端口**写进了仓库，而 `AGENTS.md` §3 明文"不将密钥、账号、订单信息、个人资料或**真实服务端地址**写入仓库、日志或演示数据"。
> 本文正文**不复述这些具体值**，只登记"它们在 `WEBGL_MINIGAME_HANDBOOK.md` §1"；是否继续保留在仓库内、或改为占位符 + 文档外交付，请负责人定。

**仍然缺的**：
- 谁有**导出 / 上传 / 体验版 / 真机测试**权限（**没有单点保管人**）。
- 真机**基线设备覆盖范围与通过阈值**、性能采集标准（内存峰值怎么采）。
- YooAsset **正式收集器 / 分包策略**与首包预算（当前只见样例配置）。
- 正式环境 **HTTPS + 已备案域名**（服务端范围，`RISK-021`）。
- OSS 上传凭据（`ossutil` 的 AK）如何取用 —— **不要写进仓库**。

## 6.7 移植经过与结论（2026-09-22 补写）

> 本节内容全部取自另一会话的权威记录（`WEBGL_MINIGAME_HANDBOOK.md`、`TCP_NETWORK_BREAK_ROOTCAUSE.md`、`HOME_PAGE_SCENE_FIX_OPTIONS.md` §九、`DEVELOPMENT_WORKFLOW.md` §2026-09-22、`WEBGL_B2_REALDEVICE.md` §八），**不是推断**。

**一句话结论**：这次"转到 WebGL / 微信小游戏"的实质是**三件事** —— ① 启动链改造（谁能加载客户端场景）、② 资源寻址对齐（谁在哪个路径取资源）、③ 跨场景宿主补丁（`Single` 模式下的加载后逻辑）。**不是渲染层或业务代码的移植**。

**经过（按因果，不按时间）**

1. **起点**：WebGL 能构建、能跑，但**永远进不到客户端 UI**。根因（`BUG-025`）：构建清单只有 `Assets/Main/Boot.unity`，`ClientShell` 既不在清单、也没有任何运行时代码加载它。
2. **方案选择与一次失败**：A = 把 `ClientShell` 加进 Build Settings 并由 `Init` 加载；B = 改原 `Boot` 流程追加 `LoadScene`；C = 新建 `ClientBoot` 置首。
   **先落地方案 C，随即失败并回退**：`ClientBootLoader` 用 `LoadScene` 单参（`Single`）顶掉 `Boot` ⇒ `Init` 不跑 ⇒ YooAsset 未初始化 ⇒ `Assembly.Load(HotFix)` 不执行 ⇒ `NetController` 不在 AppDomain ⇒ TCP 探针 15 秒超时（详见 `TCP_NETWORK_BREAK_ROOTCAUSE.md`）。
   **最终落地方案 A**：构建清单 `Boot(0) → ClientShell(1)`；`Init.LoadFirstSceneAsync` 对**内置场景**走 `SceneManager.LoadSceneAsync` —— 因为 `ClientShell` 在播放器里但**不在 YooAsset 收集器范围**，按地址加载必然 `Failed to mapping location to asset path : ClientShell`。
3. **A4 补丁（跨场景宿主）**：`Single` 模式卸载 `Boot` 会连同销毁 `Init` 及其协程 ⇒ 写在协程 `yield return` **之后**的"加载后相机兼容处理 / 首屏诊断"**永不执行**（容器日志实测 0 命中）。解法是新增 `WebGLPostSceneLoadRunner`：`Init` 在触发加载**之前**创建它并 `DontDestroyOnLoad`，它订阅 `SceneManager.sceneLoaded`，切换完成后调用一次 `Init.RunPostSceneLoadCompatibility()` 再自毁。**刻意不改 `Init` 的生命周期**（探针依赖"`Boot` 场景会被卸载"）。
4. **资源寻址对齐**：YooAsset 远端 = **CDN 根目录**（`{host}/{fileName}`），不是 `/Assets/`；`/Assets` 那层是**微信 SDK 的纹理/音频**用的（当前未启用）；**首包 `data`/`wasm` 也在根目录**（`dataFileSubPrefix: ''`）；YooAsset 的内置版本探测路径 404 **非致命**。⚠️ **同一类路径错配被踩过两次**（先少一层、后多一层）⇒ 纪律：**上传后按客户端实际请求的 URL 逐个自检，不能推断**。
5. **真机**：第一道拦路是**微信域名校验**（`downloadFile:fail url not in domain list`）。A/B 对照取证：**唯一变量 = 手机端「调试」开关**；开着才放行，且它覆盖 `downloadFile` / `request` / `socket`（裸 IP 的发现服务与 TCP 网关一并放行）。**生产化前置**：正式版/体验版没有这个开关，而发现服务与 TCP 网关是**裸 IP + HTTP、加不进合法域名白名单** ⇒ 需服务端提供 HTTPS + 已备案域名（`RISK-021`）。
6. **出包工程化**：把 GUI 七步收敛成一条命令（两阶段 ≈ 9.2 min）。必须两阶段的原因见 §6.3 / §7.4（`CompileDll` 不重载程序集 + `AssetDatabase.Refresh()` 的域重载会打断 `executeMethod` + `-quit` 下 Unity 可能空转不退）。

**被证伪 / 被推翻的假设（交接时最容易再犯的）**

| 曾经的假设 | 实际情况 |
| --- | --- |
| 方案 C（`ClientBoot` 置首）能让客户端"独立启动" | ❌ 证伪：会让 `Boot/Init` 不加载 ⇒ 断 HotFix 与 TCP |
| `ClientShell` 只要进 Build Settings 就能被加载 | ⚠️ 不完整：还必须让 `Init` 走**原生**加载（YooAsset 按地址找不到它） |
| 首屏黑屏 = URP Shader 不支持 | ⚠️ **未证实因果**（`BUG-009` 仍"调查中"，两者仅同时发生）；现有相机 HDR/后处理关闭只算**诊断性规避** |
| "启动页 `app.json` 未定义"是当前阻塞 | ⚠️ 已越过（新日志不再报该错误），但 `BUG-013` 仍"调查中"、未闭环 |
| 改了东西就要重传整个首包 | ❌ 证伪：只改 UI 通常**只重传 `data.br`**；改了 C# 才连 `wasm.br` 一起变 |
| 用 `miniprogram-automator` 读小游戏 Console | ❌ 死路：`cli auto` 能握手，但 `evaluate`/`systemInfo`/`currentPage` 全超时，小游戏无 `getConsoleMessages` |

**达成的边界**：达成的是 `HANDOVER_PLAN.md` §5 的 **B1/B2 五项（可构建/可启动/可操作/真实会话 TCP/真机首轮）**；
**不等于可上线**：正式环境仍需合法域名 + 服务端 HTTPS 域名，且 B2 矩阵深度项（安全区与遮挡、返回栈压测、弱网/断网、后台切回）与**内存峰值**未完成。

---

# 7. 工具手册

> 全部脚本：**38 个 `.ps1` + 1 个 `.cmd`，无自研 `.py` / `.sh`**（`build/**`、`Library/PackageCache/**` 下的都是第三方）。全部要求 **`pwsh` 7**。

## 7.1 编译与验证

| 工具 | 用途 | 命令 / 退出码 | 注意 |
| --- | --- | --- | --- |
| `Logs\verify-compile.ps1` | **不关 Editor 的 Roslyn 等价全量编译**（Unity 自带 `csc.dll` + `Library\Bee\artifacts\*\Assembly-CSharp*.rsp`） | `pwsh -File Logs\verify-compile.ps1 -Profile Editor\|WebGL\|All`；`0`=通过 / `3`=缺工具 / 非 0=有 `error CS` | 会自动把 `Assets/Client` 下**未被 Bee 响应文件收录**的新 `.cs` 补进编译；产物写 `Logs\_verify`。⚠️ **它绿 ≠ 编辑器里的 DLL 是新的**（见 §9.6） |
| `Pinball.Client.Editor.ClientSelfTest.RunAll` | 纯逻辑自检（**96 项断言**），失败即非 0 退出 | 见 §1.3 命令 ② | ⚠️ **它会自行 `Exit`，管线内不可复用** ⇒ 改用 `RunAllCore` |
| `Logs\selftest-scene-mirror.ps1` | `ClientSelfTest.RunSceneWiringChecks` 的**静态镜像**（不需要 Unity） | `pwsh -File …`；`0` | 报告 `Logs\selftest-scene-mirror.md` |
| `Logs\verify-adapt12-offline.ps1` | 一条命令跑 **8 项**（编译/审计/自检镜像/引用校验/补丁安全/排行榜/布局/完整性） | `0` | 改 UI 适配后首选 |
| `Logs\verify-adapt12.ps1` | 备份 → 幂等修复 → 自检 | `0` / `3` / `4`(Editor 未关) | 会写 `ClientShell.before-adapt12-*.unity.bak` |
| `Logs\verify-scene-facts.ps1`（35 KB） | 场景事实大批量校验 | `0` | 只读 |
| `Logs\verify-table-drift.ps1` | 客户端 ↔ 配置表漂移校验 | `0` | 只读 |
| `Logs\check-scene-integrity.ps1` | 场景 document 集合完整性对比（**写场景前后必跑**） | `0` | 只读 |
| `Logs\check-handoff-refs.ps1` | ⚠️ **不是文档引用检查器**：它校验英雄详情页签皮肤在 `ClientShell.unity` 里引用的 **fileID / sprite GUID** 是否存在且命名正确 | `0` | 只读；名字有误导性 |

## 7.2 只读审计（幂等，只写 `Logs\*.md`）

`ui-contract-audit`、`ui-type-audit`、`ui-flow-matrix`、`ui-dead-buttons`、`ui-layout-audit`、`ui-adapt-audit`、`ui-adapt-audit2`、`ui-adapt-audit3`、`ui-raycast-audit`、`ui-tree-dump`、`ui-node-query`、`ui-blocks`、`ui-image-dump`、`ui-geometry-dump`、`ui-geom-compute`、`ui-absrects`、`ui-absrect-filter`、`ui-herodetail-stack`、`rank-layout`、`rank-layout-check`、`rank-row-internals`、`rank-resolve-check`、`_rank-viewport-inspect`、`scrollfix-blast-radius`、`_scene-id-diff`、`_crop-cards`。

调用：`pwsh -File "D:\unity project\pinball\Logs\<名>.ps1"`。**审计脚本只断言"锚点类型 / 是否贴边"，不再断言具体 offset**（避免把负责人手调值判成失败）—— 改脚本时请保持这条。

## 7.3 场景写入 / 修复（⛔ 危险区，先备份）

| 工具 | 作用 | 护栏 / 退出码 |
| --- | --- | --- |
| `ClientShellStructuralRepair`（编辑器类，菜单 `Client/结构修复/…`） | 1 只读盘点 / 2 `RepairAll`（幂等）/ 3 S6 计划报告 / 5 解耦射线与显示 / 6 层级普查 / 7 UI 适配第 1+2 批 / 8 排行榜列表修复（`RepairRankPageLists`） | 场景写入**不可逆**；运行前必须关 Unity；`FixRankPageStableAnchors()` 只改"还是中锚"的节点，已是贴顶锚的一律跳过 |
| `Logs\apply-adapt12-text.ps1`（17 KB） | 把与编辑器工具**完全相同的值**写进场景 YAML | 检测到编辑器在跑 **拒绝写入（exit 3）**；不加 `-ForceLayout` 不碰任何位置值 |
| `Logs\apply-rank-text.ps1`（12 KB） | 排行榜文本补丁 | **默认拒绝写入（exit 5）**，需显式 `-OwnerApproved`；`-DryRun` 仍只读可用 |
| `Logs\repair-rank-batch.ps1` | 只跑 `RepairRankPageLists` | `3`=无 Unity / `4`=Editor 占用 |
| `Logs\move-round-heads.ps1` | 改 Markdown 轮次标题（审计报告整理用） | 非场景操作 |
| 回滚 | `Logs\ClientShell.before-*.unity.bak`（约 34 个，最大 5.5 MB，覆盖 09-20 ~ 09-22 每次写入前） | **改场景前先确认有对应 .bak** |

## 7.4 WebGL 构建与发布

| 工具 | 用途 | 破坏性 |
| --- | --- | --- |
| `Logs\webgl-port\run-webgl-port.ps1` | 主管线步骤 0–5（启动链校验→自检→配置表基线→YooAsset 打包→热更 DLL→WebGL 构建+微信转换） | ⛔ **会覆盖 `Assets/HotUpdateResources/Dll/WebGL/HotFix.dll.bytes`** |
| `Logs\webgl-port\run-webgl-port.cmd` | 双击包装器，强制走 `pwsh`（缺 `pwsh` 时 `exit /b 2`，**不静默回落 5.1**） | 无 |
| `Logs\webgl-port\run-pipeline-2phase.ps1` | 两阶段出包（Phase A 编译热更 DLL → Refresh；Phase B 打 AB + WebGL + 转换） | ⛔ `Stop-UnityLeftovers` 会**强杀 Unity 进程并删 `Temp\UnityLockfile`** |
| `Logs\webgl-port\upload-to-oss.ps1` | 首包 + AB 上传 OSS（含上传后自检） | ⛔ **不可逆发布操作**；依赖外部 `ossutil`，**AK 不入库**；`-DryRun` 只看命令 |
| `ClientWebGLBuildCommand`（编辑器类） | 13 个 `public static` 入口：`RunFullPortPipeline` / `RunBuildBundles` / `RunSyncHotFixDll` / `RunVerifyStartupChain` / `RunSelfTest` / `RunTableBaseline` / `ConvertMiniGame` / `BuildBundles` / `BuildBundlesAndHotFix` / `SyncHotFixDll` / `VerifyStartupChain` / `PreparePlayerAssemblies` / `BuildBundlesAndConvert` | 见上 |

**为什么必须两阶段**（实测 2026-09-22，`run-pipeline-2phase.ps1:8-16`）：`CompileDllCommand.CompileDll` 会把新程序集写到磁盘，但**运行中的 Editor 仍加载着旧程序集**，`BuildPipeline.BuildAssetBundles` 校验序列化布局时会报 `script class layout is incompatible between the editor and the player`；而修复需要程序集重载，`AssetDatabase.Refresh()` 触发的域重载**会中断当前 `executeMethod`** ⇒ 必须跨进程。

**Unity CLI 约定的两个坑**：
1. **禁用 `Start-Process -ArgumentList`**（工程路径含空格会被拆开）⇒ 用 `ProcessStartInfo.ArgumentList`。
2. `-batchmode -quit` 在本工程**可能空转不退出**（实测日志冻结 7 分钟以上、CPU ~200%）⇒ 脚本靠"步骤完成标记 + 日志静默 45 秒"判定完成后主动 reap，不要死等进程退出。

## 7.5 编辑器菜单（人工可点）与命令行等价

| 菜单 | 位置 | 命令行等价 |
| --- | --- | --- |
| `Client/结构修复/1. 只读盘点报告` | `ClientShellStructuralRepair.cs:51` | `.ReportOnly` |
| `Client/结构修复/2. 执行结构修复` | `:60` | `.RepairAll` |
| `Client/结构修复/3. S6 计划报告（只读）` | `:4561` | — |
| `Client/结构修复/5. 解耦射线与显示（全场景）` | `:718` | — |
| `Client/结构修复/6. 层级普查报告（只读）` → `Logs/hierarchy-audit.txt` | `ClientHierarchyAudit.cs:41` | `.RunAudit` |
| `Client/结构修复/7. UI 适配 第 1+2 批` | `:165` | — |
| `Client/结构修复/8. 排行榜列表修复` | `:199` | `.RepairRankPageLists` |
| `Client/自检/运行全部纯逻辑自检` | `ClientSelfTest.cs:44` | `.RunAll`（⚠️ 会自行 Exit）/ `.RunAllCore` |
| `Client/配置表/打印客户端配置表行数` | `ClientTableProbe.cs:31` | `.LogRowCounts` |
| `Client/配置表/验证道具类型映射（领取路径）` | `:223` | — |
| `Client/配置表/打印构建前全量基线` | `:271` | `.LogFullBaseline` |
| `Client/加载页/演示 5 秒加载`（仅 Play 模式） | `ClientLoadingPageEditor.cs:49` | — |
| `Build/WebGL/Compile And Sync HotFix DLL` | `Assets\Editor\HybridCLR\WebGLHotUpdateDllCommand.cs:19` | `.SyncHotFixDll` |
| `Build/BuildDll` | `Assets\Editor\HybridCLR\BuildAssetsCommand.cs:81` | — |
| `HybridCLR/BuildBundles/*` | `Assets\Editor\HybridCLR\AssetBundleBuildCommand.cs:91-118` | — |
| 旧 Lua 框架：`LuaFramework/Build * Resource`、`AssetBundle/Build/*`、`Tools/*` | `Packager.cs`、`AssetBundleTool.cs`、`AddBuildMapUtility.cs` | 旧体系，非本次链路 |

> ⚠️ **不要在菜单方法里调 `AssetDatabase.Refresh()`** —— 会触发域重载把正在执行的方法打断（`UI_LAYOUT_DEBUG_GUIDE.md` §7）。

## 7.6 日志与证据目录约定

| 位置 | 内容 |
| --- | --- |
| `Logs/` | 283 个 `.log`；脚本；审计报告 `*.md`；场景备份 `ClientShell.before-*.unity.bak` |
| `Logs/_verify/` | `verify-compile.ps1` 的 DLL 与日志（不覆盖 `Library/Bee` 下 Unity 产物） |
| `Logs/webgl-port/` | 管线日志 `b1-step*.log` / `.console.txt`、`phaseA/B-*.console.txt`、上传清单 |
| `Logs/*.cs`（4 个） | `_newtool_methods{,2,3}.cs`、`_selftest_scene.cs` —— 补丁方法/断言**草稿**，**未编入 Assets**，改工具时可能要同步 |
| `Application.persistentDataPath/ClientServerLogs` | `ClientServerPacketRouter` 的 JSONL + `.protocol.log` + 可选原始包 |

---

# 8. 步骤流程集

## 8.1 新会话开工（5 步）

1. 读 `AGENTS.md`（硬约束）→ 按 §6 路由读最小上下文（继续任务先读 `CURRENT_STATE.md` 顶部快照；进模块前读该模块 `MODULE.md`）。
2. `git status --short`（**只读**，确认接手前的工作区改动仍在，且不覆盖无关改动）。
3. 建立最小计划：目标 / 涉及模块 / 验收条件 / 验证方式 / 风险（本会话的 `todo_write` 或计划模式）。
4. 只实现已确认的最小闭环；碰到未确认需求、外部服务、支付、删除数据、大规模重构 ⇒ **停下来问负责人**。
5. 交付时说明：改了什么、涉及文件、验证结果、遗留风险/待确认项、下一步建议。

## 8.2 改一页 UI 的完整闭环（负责人强制要求）

**一次只做一页，做完必须书面说明，负责人在 Unity 里 Play 验收通过才算"完成"。** 未验收前 `CURRENT_STATE.md` 里不得写"已完成"。

1. 取证：`Logs/hierarchy-audit.txt` 或 `Client/结构修复/6` 拿到该页节点与数据的对应关系。
2. 定范围：只改这一页；不顺手重构别的。
3. 改代码（页面控制器/View），编译 `verify-compile.ps1 -Profile All`。
4. 需要改节点/位置时：走 §8.4 的场景写入流程（**先说明将要发生的变化并取得确认**）。
5. 验证：`ClientSelfTest` + 相关 `ui-*` 审计 + `check-scene-integrity.ps1`。
6. 向负责人交付说明 + 请其在 Unity 中 Play 验收。
7. 验收通过后更新 `CURRENT_STATE.md` 与受影响 `MODULE.md`。

## 8.3 列表收敛四步（**顺序错就会出可见回归**）

1. **取证**：查 `Logs/hierarchy-audit.txt` 的"数据 ↔ 节点"节，确认容器项数与数据条数是否失配。
2. **校验前提**：容器必须自带布局组件（`GridLayoutGroup`/`VerticalLayoutGroup`/`HorizontalLayoutGroup`）；`ConvergeList` 会拒绝无布局组件的容器。`星级图标Image` 这类**固定数量展示不得套用**（它靠显隐表达等级）。
3. **先改代码，后动场景**：**先**给该页 View 加模板生成逻辑（含内联回退路径）并编译通过 —— 顺序颠倒会让页面只剩 1 个禁用模板，这是**可见的功能回归**。
4. **登记白名单再执行**：把容器登记到 `ClientShellStructuralRepair.cs` 的 `ListTargets`，跑 `RepairAll`，最后用普查报告核对**节点数与内联列表数确实下降**。白名单是显式的，不做"扫描到就收敛"。

## 8.4 场景写入与回滚

1. **关掉 Unity**（批处理需要工程锁；场景以磁盘文件为准，未保存的 Inspector 改动会丢失）。
2. 确认 `Logs/ClientShell.before-*.unity.bak` 已有当前基线（没有就手工备份）。
3. 说明将要发生的变更并取得负责人确认（不可逆）。
4. 执行 `RepairAll` / `RepairRankPageLists` / 文本补丁脚本（注意 `apply-rank-text.ps1` 需 `-OwnerApproved`）。
5. 写后三验：`check-scene-integrity.ps1`（document 集合）+ `selftest-scene-mirror.ps1` + 相关 `ui-*` 审计必须回到 PASS。
6. 失败回滚：用 `.bak` 覆盖回 `ClientShell.unity`。

## 8.5 交付前验证三件套（**任何影响编译输入的改动**）

```text
① Logs\verify-compile.ps1 -Profile All        → 三目标 0 error
② ClientSelfTest（96 项）                     → 失败 0
③ 相关审计脚本 + Logs\check-scene-integrity.ps1 → PASS
```

⚠️ `RISK-019`：**只看 `error CS` 会漏掉运行时的 `LogError`**（曾漏掉 99 条运行时 `FormatException`）⇒ 必须同时检查 `-logFile` 日志正文。
⚠️ `RISK-017`：`verify-compile.ps1` **不覆盖 `HotFix` 程序集**（`Assets/Scripts`）；改到那边要单独验证，否则会出现假 `CS1061`。

## 8.6 其它流程

- **新增页面**：§3.5 路径 A（7 步）。
- **接服务端**：§3.5 路径 B（3 步）。
- **WebGL 出包 / 真机**：§6.3 / §6.4。
- **记录纪律**：`/compact`、换会话、交接前，以及当前目标/检查点/阻塞/验证结果发生变化后，**必须刷新 `CURRENT_STATE.md`**；详细证据回写到缺陷/版本/模块文档，不堆在入口文档里。

---

# 9. 试错与纪律（**上一批人踩过的坑，别再踩**）

## 9.1 坑清单（症状 → 根因 → 解法/现状 → 权威文档）

### ① 启动链与网络（最贵的一次）
- **症状**：小游戏里界面能跳转，但 TCP 探针只打一条"已挂载"，15 秒超时。
- **根因**：入口从 `Boot` 换成 `ClientBoot`，`ClientBootLoader` 用 `LoadScene` 单参（= `Single`）**替换**了场景 ⇒ `Boot` 永不加载 ⇒ `Init` 不跑 ⇒ YooAsset 未初始化 ⇒ `Assembly.Load(HotFix)` 不执行 ⇒ `NetController` 不在 AppDomain（探针靠反射找它）。
- **解法/现状**：改为 `Boot(0) → ClientShell(1)`；`ClientBoot` 已弃用并从构建清单移除，`ClientWebGLBuildCommand` 会在校验时报错拦截。
- **权威**：`TCP_NETWORK_BREAK_ROOTCAUSE.md` §三/§四/§七/§十.1、`BUG-025`。

### ② 同一类 CDN 路径错配，踩了两次
- **症状**：53 个文件 404；更早一次是资源找不到。
- **根因**：YooAsset 远端是 `{host}/{fileName}`，`host` 是 **CDN 根**，不是 `/Assets/`；而 `/Assets` 是**微信 SDK 首包**用的层级。两次分别是"少一层"（`BUG-015`）和"多一层"。
- **纪律**：**上传后必须按客户端实际请求的 URL 逐个自检，不能靠推断**；改 CDN 要同改 `MiniGameConfig.asset` + `Boot.unity` 两处并重跑转换。
- **权威**：`CURRENT_STATE.md` §「本会话踩过、务必记住的坑」第 2 条（关键词："同一类路径错配本会话踩了两次"）、`BUG-015`。

### ③ UI 适配的系统性根因（记住这一条就够）
- **根因**：`ClientCanvas` 参考分辨率 `753×1630` + `matchWidthOrHeight = 0`（按宽）⇒ **宽恒 753、高 = 753×(H/W)**：
  `1080×2280 → 1590`、`1080×2160 → 1506`、**`1080×1920 → 1339`（比设计矮 291）**。
- **推论**：横向稳定，**纵向全靠贴顶/贴底/拉伸**；凡"固定高度或固定纵向位置"的内容在矮画布上必然溢出/被裁/重叠。
- **症状**：排行榜 6 个中锚节点在 1920 下整体下移 **145.5 单位** ⇒ 负责人反馈的"标题出框 / 说明框与未上榜位置不对"。
- **权威**：`CURRENT_STATE.md` §「系统性根因（记住这一条）」（关键词：`1080×1920→1339`）、`UI_ADAPT_BACKLOG.md:6-14`、`UI_LAYOUT_DEBUG_GUIDE.md:31-41`。

### ④ 列表/视口塌陷
- 战力榜 `Viewport` 锚点 `(0,0)-(0,0)` + `sizeDelta (0,0)` ⇒ **视口塌成 0×0** ⇒ `RectMask2D` 失效、内容画到框外。
- 两份 `Content` 是**中锚 + `pivot(0,1)`** ⇒ 被横向推出；网格对齐需 `MiddleCenter`。
- `ContentSizeFitter` + 双向拉伸 ⇒ 完全失效、列表滚不动（必须单向拉伸）。
- **现状**：已修（`FixRankPageLists()` + 菜单 8 + 文本补丁）；**两个 Viewport 的 `sizeDelta` 疑似互换**（战力榜 `(-17,0)`、挑战榜 `(0,0)`）仍待负责人核对。
- **权威**：`CURRENT_STATE.md` 顶部快照的「已知未闭环项」与「第 4 批 #6（排行榜）」两节。

### ⑤ UI"静默失效"（最难查的一类）
- 弹窗页面**从未被 `Fallback` 解析**（`_pageById` 只收 `ClientUiPage`）⇒ `Show()` 不被调用（`BUG-017`）。
- `FindDirectChild("前往获取Canvas")` 恒 `null`（`BUG-016`）。
- 序列化字段 `fileID: 0`（未接线）（`BUG-018`/`019`/`020`、`RISK-015`）。
- 列表越界有保护 ⇒ 表现为**点了没反应**，不报错（商城 13 张卡但 `_listings` 更少）。
- **纪律**：**不要用工具"没扫到"去否定运行时"确实存在"**（`CURRENT_STATE.md` 历史轮次里的同名踩坑记录）。

### ⑥ 排序不由层级决定
- `overrideSorting = true` 的嵌套 Canvas **完全无视兄弟序号**，只按 `sortingOrder` 比大小（商店被抬到 `order=1005` ⇒ 永远在最上）。
- 曾**连续 4 轮改兄弟序号**（`SetAsLastSibling`、整体重排）**全部无效** —— 那是一个**不参与排序的维度**。
- **纪律**：动手前先打印每个图层的 `Canvas.overrideSorting` 与 `sortingOrder`。

### ⑦ 运行时读数不能当布局事实
- 把 Play 中读到的 `rect.rect.size = 215.9 × 2219.1` 当成排版事实，据此判断"窄竖带"并改布局 ⇒ 实际是居中的 `752×1527` 面板，**改完把界面盖住了**。
- **纪律**：**布局事实以场景文件 / Inspector 为准**；运行时打印只说明"当前帧的渲染状态"。

### ⑧ 编辑器在跑旧编译产物
- 菜单项存在但方法体是上一版 ⇒ 表现为"点了只改一半"。
- 判别：比对 `Library/ScriptAssemblies/Assembly-CSharp-Editor.dll` 与源码时间戳，或在 DLL 原始字节里搜你新写的日志字面量。
- ⚠️ **`verify-compile.ps1` 绿 ≠ 编辑器 DLL 是新的**（它是独立 Roslyn 编译）。可靠解法：点回 Unity 窗口 / `Ctrl+R` 等编译完成，或关掉编辑器用批处理 `-executeMethod`。
- ⚠️ **别在菜单方法里调 `AssetDatabase.Refresh()`**（域重载会打断正在执行的方法）。
- ⚠️ 批处理前不关编辑器 ⇒ `0x40000015`，**且不产出日志**。
- **权威**：`UI_LAYOUT_DEBUG_GUIDE.md` §7。

### ⑨ 构建管线的两个硬事实
- `CompileDll` 不重载程序集 ⇒ **必须两阶段构建**（§7.4）。
- Unity `-batchmode -quit` 在本工程**可能不退出发热自旋** ⇒ 靠日志完成标记 + 静默判定后 reap。

### ⑩ 配置表三连坑
- `%persistentDataPath%/config.pkg` 优先级高于 `Resources/Table/*.json` ⇒ **新表"放了不生效"**（`BUG-021`）。**不要改 `ClientTableSource` 的优先级**。
- 工程生成类与实际配置表**结构漂移** ⇒ 运行时 99 条 `FormatException`（`BUG-022`，例如 `Skill` 的 `[]` vs `int`，33×3 条）。
- `verify-compile.ps1` **不覆盖 `HotFix`**（`RISK-017`）⇒ 23 个假 `CS1061`；只看 `error CS` 会漏掉运行时 `LogError`（`RISK-019`）。

### ⑪ 编辑器/编码环境
- **`pwsh` 7 vs 5.1**：5.1 把无 BOM 的 UTF-8 `.ps1` 当 ANSI ⇒ 含中文的脚本直接解析失败。
- `edit` 类工具会**剥掉 UTF-8 BOM**（CRLF 保留）⇒ 改完 BOM 文件要复查并手工补回。
- 路径含空格 ⇒ 见 §7.4 的 `Start-Process` 禁令。

### ⑫ 历史遗留依赖
- `SuperTiled2Unity` 导致 `CS0246`：根因是脚本引用了命名空间但工程里没有该插件。**只删 `using` 是错的**；最终按负责人授权**整体移除该脚本**并清掉 `Assets/HybridCLRGenerate/link.xml` 里的链接条目。
- **权威**：`Notes/Unity-CS0246-SuperTiled2Unity-依赖缺失.md`、`HANDOFF_主开发1.md:17-18`。

### ⑬ 移植期新坑（`WEBGL_MINIGAME_HANDBOOK.md` §7，都是烧时间换来的）
- **`[SerializeField]` 不得放在 `#if UNITY_WEBGL` 内** —— 编辑器与播放器字段集不一致，Unity **直接拒绝构建**（`Error building player because script class layout is incompatible between the editor and the player`）。字段声明放外面，只把**逻辑**放进 `#if`。
- **`LoadSceneMode.Single` 会销毁 `Boot`（含 `Init`）** ⇒ 写在协程 `yield return` **之后**的代码永远不会执行（实测该分支 0 命中）；"加载后"的逻辑必须交给跨场景宿主（`WebGLPostSceneLoadRunner`）。
- **改完脚本、构建却仍用旧程序集** ⇒ 域重载未跨进程；必须走两阶段出包，不要在一个 `executeMethod` 里连做。
- **改 CDN 目录必须同改两处**（`MiniGameConfig.asset` 的 `CDN` + `Boot.unity` 的 `DefaultHostServer`/`FallbackHostServer`）且**必须重跑转换**（`game.js` 里写死 `DATA_CDN`）。
- **`edit` 类工具会剥掉 UTF-8 BOM** —— 改 `Init.cs` 这类带 BOM 的文件后要复查补回。

### ⑭ UI 审计工具的**已知判据盲区**（如实记录，避免把"盲区"误读成"缺陷"）
- 返回判据只认 `后退` / `关闭` / `Backoff` 等关键词，**不覆盖 `取消键Button` 这类业务命名** ⇒ 例如 `Product Purchase Interface` 实际由 `取消键Button` 关闭，矩阵里显示 `back=False` 属**判据盲区**。
- **prefab 实例内部**的节点不在场景 YAML 中 ⇒ 实例内的关闭按钮可能被漏判。
- 新发现缺口：`Player lineup`（玩家阵容）子树里**没有任何按钮** ⇒ 打开后**没有关闭手段**（该弹窗整体未完成）。

### ⑮ 旧街机残留
`Boot.unity` 中某 Behaviour 引用的 `UnityBridgeManager` 在运行时缺失（Console 报 `The referenced script ... is missing!`）。与首页/客户端无关，**不要删** —— 先确认它是否属原街机流程遗留、是否仍在启动链上（`BUG_TRACKER.md` 待核验项）。

## 9.2 反面教材（**明确写过"不要这样做"的清单**）

1. 不要 `git reset` / 清理 / 覆盖工作区既有改动 —— 里面有负责人手调值。
2. **不要把负责人手调的锚点"拽回去"**（`apply-rank-text.ps1` 默认 exit 5、`apply-adapt12-text.ps1` 编辑器在跑就 exit 3，这两道护栏就是为此设的）。
3. 不要直接引用 `HotFix` 程序集类型（会复发 `IL1005` / 读到旧成员）。
4. 不要用 `RepairAll` 充当"编译验证"。
5. 不要为"补 Inspector"去写 `RISK-016` 已核实为**良性**的字段（重复排查浪费）。
6. 不要凭猜测去填排行榜 6 张查找表（`BUG-019`）。
7. 不要凭现象断言"URP Shader 不支持"是黑屏根因（`BUG-009` 尚无因果证据，只知两件事同时发生）。
8. 不要手改小游戏生成工程 / `WAGame.js`（`BUG-013`）。
9. 不要用 `miniprogram-automator` 读小游戏 Console（已验证死路）。
10. 不要改 `ClientTableSource` 的读取优先级。
11. 不要用工具"没扫到"否定运行时"确实存在"。
12. 不要把"待实现"写成"已完成"；未接真实服务/支付/平台桥接的只能写"本地模拟闭环完成"或"待接入"。
13. 不要用大范围 `SetActive(false)` 掩盖重复页面问题（迁移要证明"旧入口被唯一替代"）。
14. 不要在没关编辑器时跑批处理（会静默失败）。

## 9.3 诊断七条纪律（`Assets/Client/MODULE.md` §12，都是实际踩坑）

1. **排序不由层级单独决定** —— 先查清"排序按什么决定"。
2. **运行时读数不能当布局事实** —— 布局以场景/Inspector 为准。
3. **沿"谁能改变这个状态"穷举**，而不是沿"哪里可能不对"猜（用一次全局搜索列出所有能写该属性的代码路径）。
4. **同一所有者原则** —— 显隐与层级只由 `ClientPopupService` 决定，遗留代码不得各自动手。
5. **反证要当成反证** —— 修复完全不产生可观察变化时，先怀疑"我打在了不参与该结果的维度上"。
6. **日志要打印"状态量"**（`activeSelf`/`activeInHierarchy`/首个未激活祖先/兄弟序号/栈快照/Canvas 链），不是"我调用了某方法"。
7. **工作流纪律** —— 纯代码改动不关 Unity；需要写场景才关；一次一页、每页独立验收。

---

# 10. 遗留问题与待确认（按优先级；完整版看 `BUG_TRACKER.md`）

| 优先级 | 项 | 位置 |
| --- | --- | --- |
| **P0** | 排行榜 6 个节点仍是**中锚**，1920 下整体下移 145.5；修复**已编译通过但未写入场景**（负责人选择自行调整） | `CURRENT_STATE.md` 顶部快照「已知未闭环项」第 1 条 |
| **P0** | 两个 Viewport 的 `sizeDelta` 疑似互换（战力榜 `(-17,0)`、挑战榜 `(0,0)`） | 同节第 2 条 |
| **P0** | `BUG-009` 首屏黑屏与 URP Shader 日志的关联**未证实因果**（B2 首要取证项） | `BUG_TRACKER.md:67-73` |
| **P0** | `BUG-013` 小游戏导出工程启动页未在 `app.json` 定义 | `BUG_TRACKER.md:59-65` |
| **P0** | `BUG-024` 邮件正文/领取状态在场景里**没有文本节点**（设计缺口，负责人三选一） | `BUG_TRACKER.md:498-503` |
| P1 | `BUG-023` "资源名/id → Sprite"**无解析通路** + 缺美术（4 类缺图 + 元素 6 暗） | `BUG_TRACKER.md:375-417,490-496` |
| P1 | `RISK-020` `SystemLayer` 分层与文档不一致，A/B/C 待定（定案前 §3.1 那张图不是强制约束） | `BUG_TRACKER.md:355-373` |
| P1 | `BUG-022` 配置表结构漂移（战斗侧仍开放） | `BUG_TRACKER.md:323-345` |
| P1 | `RISK-015` 其余未接线项（红点无目标节点 / 详情底键图标 / 页签选中图 / 邮件详情 / 弹窗遮罩） | `BUG_TRACKER.md:259-269` |
| P1 | `BUG-010/011/012` 待线上复验（首包版本 404 / 本地表缺失致空引用 / `Resources_HotUpdate` 重复地址） | `BUG_TRACKER.md:35-57` |
| P1 | `RISK-011` WebGL 大厅直进与资源清理**待人工验收** | `BUG_TRACKER.md:27-33` |
| P1 | 商店/邮箱列表"视口宽 < 内容宽"被裁（backlog #5/#7；负责人已宣布验收通过，故未实施） | 同节第 3 条 |
| P1 | `RISK-014/004` 可再生产物清理待负责人执行 | `BUG_TRACKER.md:19-25,155` |
| P1 | `Player lineup`（玩家阵容）弹窗**无任何关闭按钮** ⇒ 打开后关不掉（该弹窗整体未完成） | `BUG_TRACKER.md` BUG-026 补记 |
| P2 | `RISK-005` 历史 `FishScene` 可能保留已移除组件的 Missing Script | `HANDOFF_主开发1.md:22` |
| P2 | `UnityBridgeManager` 运行时缺失（旧街机残留，Console 警告；与首页无关） | `BUG_TRACKER.md` 待核验项 |
| P2 | `WEBGL_B2_REALDEVICE.md` 中 `[TCP客户端] 网关成功` 原行截图未补（结论不受影响） | `WEBGL_MINIGAME_HANDBOOK.md` §9 |
| P2 | 开发者工具 Console 内 `1 error` 未归位（疑为已知非致命 404，**未取证**） | 同上 |
| ⬜ 未测 | **B2 矩阵深度项**：安全区与遮挡、返回栈压测、弱网/断网、后台切回 | `DEVELOPMENT_WORKFLOW.md` §2026-09-22 一 |
| ❌ 未采集 | **内存峰值**（现有面板只有 FPS/Jank/Stutter） | 同上 |
| 🔴 生产化前置 | **正式版/体验版需服务端提供 HTTPS + 已备案域名**（发现服务与 TCP 网关是裸 IP + HTTP，无法加白）；OSS 域名需加 `downloadFile`/`request` 合法域名 | `BUG-028` / `RISK-021`（`BUG_TRACKER.md` 尾部） |

**移植侧未完成清单（6 项，权威版见 `WEBGL_MINIGAME_HANDBOOK.md` §9）**：真机 B2 深度项 · 内存峰值采集 · 合法域名 + 服务端 HTTPS 域名改造 · 真机截图补齐 · Console `1 error` 归位 · `UnityBridgeManager` 待核验。

> 📌 `BUG-025`（阶段 B 硬阻断）按 `DEVELOPMENT_WORKFLOW.md` §2026-09-22 三的记录，**关闭条件已由构建日志 + 容器/真机证据满足**（文档间状态尚未同步到 `BUG_TRACKER.md`，接手时可顺手对齐）。

**产品/服务端侧待确认（未定义即不得实现为既定规则）**：实际战斗/关卡玩法、服务端协议、账号体系、支付接入、数值公式、活动周期、奖池与概率、最终美术/字体/音效、性能预算、小程序容器方案。

---

# 11. 交接检查清单与硬约束

## 11.1 接手人 DoD（能独立完成即算接手成功）

- [ ] 能在命令行跑通 §1.3 三条验证命令，并解释每条的作用与前置条件。
- [ ] 能说清 `Boot(0) → ClientShell(1)` 与 `Init.ClientStartScene` 的关系，以及 `ClientBoot` 为什么**不能**加回去。
- [ ] 能画出 `ClientCanvas` 三层的**设计意图**与**实测现状**差异，并知道 `RISK-020` 未定案。
- [ ] 能指出 UI ↔ 数据的唯一入口（`ClientServices` 三个属性）与"接服务端只换一个实现"的落点。
- [ ] 能说出哪些"看起来该是表"的东西其实**不是表**（§4.4）。
- [ ] 能在改一页 UI 后自己跑完 §8.5 三件套，并知道哪些操作**必须先关 Unity**。
- [ ] 能列出至少 5 条 §9.2 反面教材并说出对应根因。
- [ ] 知道 `git status` 里的数百项脏改动**不许动**，以及敏感文件（§11.3）不随仓库流转。
- [ ] 读完 §2.3 冲突表，知道"读文档时以谁为准"。
- [ ] 能说清**阶段 B 五项**具体指什么（可构建/可启动/可操作/真实会话 TCP/真机），以及**为什么仍不等于可上线**（合法域名 + 服务端 HTTPS 域名 + B2 深度项 + 内存峰值）。
- [ ] 知道移植侧唯一入口是 `WEBGL_MINIGAME_HANDBOOK.md`，并能复述三条机制：`LoadSceneMode.Single` 会销毁 `Boot`（⇒ 跨场景宿主）、两阶段出包为什么必须跨进程、`[SerializeField]` 不能放进 `#if UNITY_WEBGL`。
- [ ] 能说出"同一类 CDN 路径错配踩过两次"，以及上传后必须做的**逐个 URL 自检**动作。

## 11.2 禁止事项（红线）

| # | 禁止 | 依据 |
| --- | --- | --- |
| 1 | 替换/升级/移除 URP、YooAsset、HybridCLR、XLua、微信 SDK、`Packages/manifest.json`、项目设置（唯一例外：负责人已授权改 `MiniGameConfig.asset` 的 CDN 与构建清单） | `AGENTS.md` §1/§3 |
| 2 | `git reset` / 清理 / 覆盖工作区既有改动 | `AGENTS.md` §3 |
| 3 | 清理无法由版本控制/生成规则/引用分析证明安全的资源（先记 `BUG_TRACKER.md` 待核验清理项） | `AGENTS.md` §6 |
| 4 | 在 UI 脚本里硬编码运营内容与数值；在页面里写支付/发奖/扣币规则 | `AGENTS.md` §3 |
| 5 | 自己新建/生成 UI 节点、替换素材、调排版（**默认只改脚本 + 现有节点引用**；只有负责人明确要求"写入场景"才允许） | `AGENTS.md` §4 |
| 6 | 代替负责人操作 GUI（Editor / IDE / 构建窗口 / 开发者工具 / 平台后台） | `AGENTS.md` §4 |
| 7 | 把密钥、账号、订单、个人资料、真实服务端地址写进仓库/日志/演示数据 | `AGENTS.md` §3 |
| 8 | 同一实现路径第一次失败后无新证据就重复尝试 | `AGENTS.md` §6 |
| 9 | 依据现象直接断言 WebGL/微信问题的根因并动手修 | `AGENTS.md` §4 |

**逐次授权文件（每改一次确认一次，不是一次性授权）**：
`Assets/Main/Init.cs` · `Assets/Main/Boot.unity` · `ProjectSettings/EditorBuildSettings.asset`（构建清单） · `Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset`（CDN 等）。
依据：`WEBGL_MINIGAME_HANDBOOK.md` §8、`Assets/Main/MODULE.md` §约束与验证。
可自由改动（无需单独确认）：`Logs/` 下的脚本与文档、纯 Markdown 文档。

## 11.3 敏感信息处理（**写清位置与文件名，不写密钥本体**）

| 项 | 位置（文件名可写） | 处理 |
| --- | --- | --- |
| Android 签名密钥 | `KeyStores\xiaoshikeji.keystore` | 🔒 **不随仓库流转**；口令由负责人单独提供 |
| OSS 上传凭据 | `Logs\webgl-port\upload-to-oss.ps1` 依赖外部 `ossutil` 配置 | 🔒 AK **不入库**；走本机 `ossutil` 既有配置或负责人临时提供 |
| CDN / 远端资源地址 | `MiniGameConfig.asset` 的 `CDN` + `Boot.unity` 的 `DefaultHostServer` / `FallbackHostServer` | 只写"在这两处"；**改完必须重跑转换**（`game.js` 写死 `DATA_CDN`）。具体值见 `WEBGL_MINIGAME_HANDBOOK.md` §1 |
| 微信 AppID / 环境配置 | 微信小游戏 SDK 配置（含导出工程侧） | 保管方式与权限**待负责人指定单点**；具体值见 `WEBGL_MINIGAME_HANDBOOK.md` §1 |
| OSS Bucket / Endpoint / 导出目录 | 见 `WEBGL_MINIGAME_HANDBOOK.md` §1 | 同上 |
| 服务端 IP / 端口 | HotFix 侧 `SysDefines.Ip` / `SysDefines.Port`（探针反射读取） | 本文不登记真实值；见 `RISK-021` 与 `WEBGL_MINIGAME_HANDBOOK.md` §1 |

> ⚠️ **一处需要负责人裁决的张力**：`WEBGL_MINIGAME_HANDBOOK.md` §1 已把 **AppID / OSS Bucket / 网关裸 IP 与端口** 写进仓库，而 `AGENTS.md` §3 要求"不将密钥、账号、订单信息、个人资料或真实服务端地址写入仓库、日志或演示数据"。
> 本文正文**只登记"去哪找"**；是否把该手册的这些值改为占位符 + 文档外交付，请负责人决定。

## 11.4 人机边界

- **代理**：代码、配置、Markdown 的阅读/设计/修改 + 可通过命令行完成的验证。
- **负责人**：Unity Editor、IDE、构建/发布窗口、微信开发者工具、平台后台等 GUI 操作；以及账号、支付、发布、上传、密钥、不可逆后台操作。
- 需要 GUI 验证时，代理必须给出：前置条件 → 逐步点击路径 → 预期结果 → 失败排查 → 完成后要回传的信息；**未回传不得标记为已通过**。

---

# 附录 A：命令速查

```powershell
# —— 环境 ——
$env:ALLUSERSPROFILE = 'C:\ProgramData'        # 只给子进程，别写系统变量
$unity  = 'C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe'
$proj   = 'D:\unity project\pinball'
$logs   = "$proj\Logs"

# —— 编译（不用关 Unity）——
pwsh -NoProfile -ExecutionPolicy Bypass -File "$logs\verify-compile.ps1" -Profile All

# —— 批处理入口模板（必须先关 Unity）——
& $unity -batchmode -quit -projectPath $proj -executeMethod <命名空间.类.方法> -logFile "$logs\<名>.log"

# —— 自检 / 基线 ——
& $unity -batchmode -quit -projectPath $proj -executeMethod Pinball.Client.Editor.ClientSelfTest.RunAll      -logFile "$logs\self-test.log"
& $unity -batchmode -quit -projectPath $proj -executeMethod Pinball.Client.Editor.ClientTableProbe.LogFullBaseline -logFile "$logs\b1-prebuild-baseline.log"

# —— 离线一条龙（8 项）——
pwsh -File "$logs\verify-adapt12-offline.ps1"

# —— 场景写入（先关 Unity、先备份）——
pwsh -File "$logs\apply-adapt12-text.ps1"      # 编辑器在跑 → exit 3
pwsh -File "$logs\apply-rank-text.ps1" -DryRun # 只读；写入需 -OwnerApproved
pwsh -File "$logs\check-scene-integrity.ps1"   # 写前写后都要跑

# —— WebGL 出包 ——
pwsh -File "$proj\Logs\webgl-port\run-webgl-port.ps1" -UntilStep 2
pwsh -File "$proj\Logs\webgl-port\run-pipeline-2phase.ps1" -PackageVersion <版本>
pwsh -File "$proj\Logs\webgl-port\upload-to-oss.ps1" -DryRun

# —— 仓库状态（只读）——
git -C $proj status --short | Measure-Object -Line
```

# 附录 B：术语表

| 术语 | 含义 |
| --- | --- |
| YooAsset | 资源加载/热更新框架；`DefaultPackage`，四种运行模式 |
| HybridCLR | 热更 C# 方案；热更目标 = `HotFix.asmdef`（`Assets/Scripts`） |
| AOT / HotFix | 主包程序集 / 运行时 `Assembly.Load` 加载的热更程序集 |
| XLua | 旧代码遗留的 Lua 绑定；**本次客户端零依赖** |
| URP / TMP | 通用渲染管线 14.0.11 / TextMeshPro 3.0.7 |
| asmdef | Unity 程序集定义；`Assets/Client` **没有** asmdef（在 `Assembly-CSharp`）⇒ 用不了 Unity Test Framework，改走 `ClientSelfTest` |
| AB | AssetBundle；`Bundles/` 是 YooAsset 输出，`AssetBundles/` 是旧体系 |
| CDN / OSS | 远端资源服务器 / 阿里云对象存储（上传目标） |
| B0 / B1 / B2 | `HANDOVER_PLAN.md` §5 的门禁 / 可复现构建 / 真机回归三阶段 |
| 三层 | `ClientCanvas` 的 `PagesLayer` / `PopupLayer` / `SystemLayer`（现状见 §3.1） |
| 手调值 | 负责人在 Unity 里手工调过的锚点/位置 ⇒ **不许被脚本拽回** |
| 护栏脚本 | 默认拒绝写的脚本（`apply-rank-text.ps1` exit 5、`apply-adapt12-text.ps1` exit 3） |
| 内联列表 | 在场景里逐个展开的列表项（普查到 64 处）⇒ 目标是由数据生成 |
| 死按钮 | 有 Button 但没有点击处理/引用的入口（`Logs\ui-dead-buttons.ps1`） |
| RISK-*** / BUG-*** | `BUG_TRACKER.md` 的风险编号 / 缺陷编号 |

# 附录 C：第三方、自动生成与存档目录（**只列不展开**）

| 类别 | 目录 |
| --- | --- |
| 第三方插件 | `Assets/WX-WASM-SDK-V2/`、`Assets/AmplifyShaderEditor/`、`Assets/Demigiant/`(DOTween)、`Assets/Spine/`、`Assets/VertexEffects*/`、`Assets/VolumetricLines/`、`Assets/DarkArts Studios/`、`Assets/JsonDotNet/`、`Assets/Unity.IO.Compression/`、`Assets/Plugins/{OpenInstall,SMSSDK,xlua.bundle}`、`Assets/Main/{Yooasset,IngameDebugConsole,Samples}/`、`WebGLPlugins/`、`build/` |
| 自动生成（勿手改） | `Library/`、`Packages/`、`Temp/`、`obj/`、`HybridCLRData/`、`HybridCLRGenerate/`、`HybridCLRBuildCache/`、`Logs/_verify/` |
| 原公司游戏（兼容性依赖，不改） | `Assets/Main/Boot.unity`、`Assets/HotUpdateResources/Scene/`(MainScene/LoginScene/LoadingScene/FishScene)、`Assets/Scripts/{GameLogic,NetWork,WebWork,BNRoom,Frame}`、`Assets/Lua/`、`Assets/BNAssetsRoom/` |
| 原样导入资源（不覆盖） | `Assets/Client/UI/{切图(11),更新,排行榜,商城(1),邮件}/`、`Assets/Client/animator controller/` |
| 第三方文档 | `docs/`（XLua 文档站，16 个 md） |
| 本仓库存档 | `Archive/`（见 `Archive/README.md`） |
| 非交付物（仓库外或无关） | `pinball-learning-code`（源码学习版发布件）、`pinball-learning-source`、`pinball-learning-source-clean`（0 提交存档）、`My project`、`Test`、`DLLTest`、`WXBridgeSmokeTest`（独立练习工程）、`.lce`（SQLite 索引库）、`ArrowOfTheKing.rar` |

---

> **维护约定**：本文档是"现状快照"，**不是长期约束**（长期约束归 `AGENTS.md`）。
> 当以下任一情况发生时请更新本文并改顶部快照日期：启动链/构建清单变化、新增或删除公开接口、新增工具、新的"文档与现实不一致"、`CURRENT_STATE.md` 顶部快照被刷新、**移植侧手册（`WEBGL_MINIGAME_HANDBOOK.md`）或真机实测记录更新**。
> 分工提醒：**移植侧细则以 `WEBGL_MINIGAME_HANDBOOK.md` 为准、客户端侧细则以 `Assets/Client/MODULE.md` + `UI_*` 为准**，本文只维护"跨两侧的现状与接口"。
> 文档写完请同步检查 `AGENTS.md` §6 的阅读路由是否需要把本文加入。
