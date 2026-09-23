# 新会话交接 Prompt（复制以下全文给新会话）

> 用途：让一个新会话（GPT / 其它模型）在**不改代码的前提下**快速熟悉本项目，并知道所有文档与 skill 在哪。
> 生成日期：2026-09-22。配套文档：`HANDOVER_DEV.md`（同目录，跨两侧的现状总入口）。

---

你是接手「弹珠项目 Unity 客户端」的开发者 / 编码代理。请**先按下面顺序快速熟悉项目，不要一上来就改代码**。全程用中文回答。

## 0. 工程坐标

| 项 | 值 |
| --- | --- |
| 工程根目录 | `D:\unity project\pinball`（**路径含空格，命令里必须加引号**） |
| Unity | `2022.3.57f1c2` → `C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe` |
| Shell | 只用 **`pwsh`（PowerShell 7）**；禁用 `powershell.exe`(5.1)（它把无 BOM 的 UTF-8 `.ps1` 当 ANSI 读，含中文的脚本直接解析失败） |
| 工作区状态 | **未提交改动数百项（约 450）**，含负责人的手调值与未跟踪文档 ⇒ **绝不 `git reset` / 清理 / 覆盖** |
| 最近提交 | `62bfdb31 chore: archive client development snapshot` |
| 当前阶段 | 客户端 UI 验收通过（"全部界面算通过了"）；移植侧**阶段 B 五项已达成**（可构建/可启动/可操作/真实会话 TCP/真机首轮），B2 深度项与内存峰值未做，正式环境还缺 HTTPS 域名 |

## 1. 第一件事：读这份新交接文档

**`D:\unity project\pinball\HANDOVER_DEV.md`（约 1043 行，11 章 + 3 附录）—— 跨两侧的现状总入口。**

包含：5 分钟上手 · 权威文档地图 + **9 条"文档与现实不一致"冲突表** · 架构与模块接口总表 · 数据流 · 启动链 · WebGL 移植（含移植经过与被证伪的假设）· 工具手册 · 步骤流程集 · 试错与坑（15 类）· 遗留清单 · 检查清单与红线 · 命令速查。

**建议阅读顺序：§0 → §1 → §2 → §3 → §9 → §10**，其余按需。

## 2. 分层阅读路由（**不要全量加载根文档**）

| 你要做的事 | 读这些 |
| --- | --- |
| 每次开工前 | `AGENTS.md`（硬约束；§6 是阅读路由） |
| 继续任务 / 问进度 | `CURRENT_STATE.md` → **只读顶部「🔖 交接快照」一节**（文件约 1943 行，其余是历史轮次，可跳过） |
| 计划 / 实现 / 修复 | `DEVELOPMENT_WORKFLOW.md`（七步闭环 + §2026-09-22 移植阶段与决策记录） |
| 进某个目录前 | 该目录的 `MODULE.md`（工程内共 **37 份**，`Assets/Client` 下 18 份） |
| 跨模块设计 | `PROJECT_OVERVIEW.md` |
| 缺陷 / 风险 | `BUG_TRACKER.md`（按 `BUG-0xx` / `RISK-0xx` 查；已到 `BUG-028` / `RISK-021`） |
| 移植 / WebGL / 真机 | **`WEBGL_MINIGAME_HANDBOOK.md`（移植侧唯一入口）** → `WEBGL_B2_REALDEVICE.md`（§八 真机实测记录）→ `WEBGL_B1_BUILD.md` · `WEBGL_B2_DIAGNOSTICS.md` · `WEBGL_B0_GATE.md` · `WEBGL_PORT_BASELINE.md` |
| 启动链 / 首页问题 | `Assets\Main\MODULE.md` → `HOME_PAGE_SCENE_FIX_OPTIONS.md`（§九 实施与出包记录） |
| 网络断链 | `TCP_NETWORK_BREAK_ROOTCAUSE.md` |
| UI 适配 | `UI_LAYOUT_DEBUG_GUIDE.md`（五步排查法）→ `UI_ADAPT_BACKLOG.md` → `UI_ANCHOR_SIGNOFF.md` → `UI_ACCEPTANCE_CHECKLIST.md` |
| 目标与阶段门 | `HANDOVER_PLAN.md`（A/B/C 三阶段；**是计划，不是现状**） |

## 3. 所有 md 在哪里

### A. 工程根 `D:\unity project\pinball\`（24 份 md；**只有这个仓库是交付物**）

| 类别 | 文件 | 状态 |
| --- | --- | --- |
| 约束 / 治理 | `AGENTS.md`、`DEVELOPMENT_WORKFLOW.md`、`PROJECT_OVERVIEW.md`、`CURRENT_STATE.md`、`BUG_TRACKER.md` | ✅ 有效 |
| 同上 | `VERSION_HISTORY.md`（停在 08-28）、`AI_MODEL_UPGRADE.md`（过程存档）、`HANDOFF_主开发1.md` | ⛔ 已过期/存档 |
| 交接 / 现状 | **`HANDOVER_DEV.md`（最新，先读它）**、`HANDOVER_PLAN.md`（计划） | ✅ |
| 移植侧 | `WEBGL_MINIGAME_HANDBOOK.md`、`WEBGL_PORT_BASELINE.md`、`WEBGL_B0_GATE.md`、`WEBGL_B1_BUILD.md`、`WEBGL_B2_DIAGNOSTICS.md`、`WEBGL_B2_REALDEVICE.md`、`TCP_NETWORK_BREAK_ROOTCAUSE.md`、`HOME_PAGE_SCENE_FIX_OPTIONS.md` | ✅ |
| 同上 | `TCP_WEBGL_HANDOFF.md` | ⚠️ **工作区已删**，副本在 `Archive\`；`git checkout -- TCP_WEBGL_HANDOFF.md` 可恢复 |
| UI 侧 | `UI_ADAPT_BACKLOG.md`、`UI_ANCHOR_SIGNOFF.md`、`UI_ACCEPTANCE_CHECKLIST.md`、`UI_LAYOUT_DEBUG_GUIDE.md` | ✅（三份状态重叠，待合并） |
| 其它 | `README.md`、`README_EN.md` | 参考 |
| 归档 | `Archive\README.md` + `Archive\{TCP_WEBGL_HANDOFF,HANDOFF_主开发1,VERSION_HISTORY,AI_MODEL_UPGRADE}.md`（均带"已归档"指向头） | 归档 |

### B. 模块文档（37 份 `MODULE.md`）

```powershell
Get-ChildItem 'D:\unity project\pinball\Assets' -Recurse -File -Filter MODULE.md | ForEach-Object FullName
```

关键几份：`Assets\Client\MODULE.md`（58 KB，客户端总模块：§12 诊断纪律 / §19 自检套件）、`Assets\Main\MODULE.md`（启动链 + 跨场景宿主 + 授权边界）、`Assets\Client\Runtime\{Services,Stats,UI,Domain}\MODULE.md`、`Assets\Client\Editor\MODULE.md`、`Assets\Scripts\Platform\WeChatMiniProgram\MODULE.md`。

### C. 工程内其它 md

- `Logs\`：25 份审计/取证报告（`ui-*.md`、`hierarchy-audit.txt`、`selftest-scene-mirror.md` 等）—— 是**证据**，改完功能用它核对
- `Notes\`：学习笔记 + `Notes\MODULE.md`（生成约定）
- `Project Introduction\Project Introduction.md`：面试展示版（阶段描述已过期）
- `Assets\Client\CLIENT_UI_ARCHITECTURE.md`：架构说明（**有 3 处已过期**，见 `HANDOVER_DEV.md` §2.3）
- `docs\`：16 份 md = **XLua 第三方文档站**，不是项目文档

### D. 非交付物（别用、别改）

工作区根下：`pinball-learning-code`（源码学习版发布件）、`pinball-learning-source` / `-source-clean`（**0 提交**的导出存档）、`My project` / `Test` / `DLLTest` / `WXBridgeSmokeTest`（**与 pinball 无关**的独立练习工程）、`.lce`（SQLite 索引库）、`ArrowOfTheKing.rar`（无关）。

## 4. Skill 在哪里（**本工程没有项目级 skill**）

- ⚠️ 先明确事实：`pinball\.codex\` 存在但是**空目录**；工程内**不存在** `.dsh\skills` 或任何项目级 skill。
  工程里搜到的 `Assets\Resources\Table\Skill.json`、`Assets\Scripts\Table\TSkill.cs` 是**游戏配置表**，与 agent skill 无关。
- 客户端 agent skill 由**运行时注入**（例：本次会话目录里的 `dsh-task-board` 只出现在会话缓存 `sub: skill, name: dsh-task-board`，磁盘上不在工程内）。
- 磁盘上**真实存在**的 skill 位置：

| 来源 | 路径 |
| --- | --- |
| Codex 用户级 skill | `C:\Users\Administrator\.codex\skills\` → `gh-address-comments`、`gh-fix-ci`、`playwright`、`security-best-practices` |
| Codex 系统内置 | `C:\Users\Administrator\.codex\skills\.system\` → `imagegen`、`openai-docs`、`plugin-creator`、`review-agent`、`skill-creator`、`skill-installer` |
| Codex 插件 skill（暂存） | `C:\Users\Administrator\.codex\.tmp\plugins\plugins\<插件>\skills\<skill>\SKILL.md` |
| DSH 预设 skill | `C:\Users\Administrator\.dsh\profiles\node_modules\@deepseek-ai\dsh\node_modules\@deepseek-ai\dsh-agent-presets\presets\cordis\skills\{cordis-plugin-development, editing-cordis-compositions}\SKILL.md` |
| DSH 第三方 skill | `C:\Users\Administrator\.dsh\profiles\web\node_modules\@liustack\modlens\skills\modlens\SKILL.md` |
| 会话 / 任务板数据（**非** skill 源） | `C:\Users\Administrator\.dsh\{sessions,storages,taskboards}\` |

⇒ 每个 skill 都是 `<目录>\SKILL.md`。若你想新增项目级 skill，**先向我确认客户端支持的 skill 根目录**，不要自行写入。

## 5. 五分钟上手（命令行）

```powershell
$env:ALLUSERSPROFILE = 'C:\ProgramData'   # 只传给子进程，别写系统环境变量

# ① 三目标编译（Editor / Assembly-CSharp-Editor / WebGL）—— 不需要关 Unity
pwsh -NoProfile -ExecutionPolicy Bypass -File "D:\unity project\pinball\Logs\verify-compile.ps1" -Profile All

# ② 纯逻辑自检（96 项；非 0 退出码 = 有失败）—— 批处理前必须关 Unity
& "C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe" -batchmode -quit `
  -projectPath "D:\unity project\pinball" `
  -executeMethod Pinball.Client.Editor.ClientSelfTest.RunAll `
  -logFile "D:\unity project\pinball\Logs\self-test.log"

# ③ 一条命令跑 8 项离线校验（编译/审计/自检镜像/引用校验/补丁安全/排行榜/布局/完整性）
pwsh -File "D:\unity project\pinball\Logs\verify-adapt12-offline.ps1"
```

GUI 侧（**由负责人操作，代理不碰 GUI**）：Unity 打开 `D:\unity project\pinball` → 场景 `Assets\Client\Scenes\ClientShell.unity` → Play；Console 应出现配置表行数汇总与本地模拟数据就绪日志。

## 6. 模块接口速查（细节见 `HANDOVER_DEV.md` §3–§5）

- **服务定位**：`Pinball.Client.Services.ClientServices`（`Data` / `Config` / `Stats`；`InitializeForDevelopment()` 顺序敏感：先 Config 再 Data；`ReplaceDataService()` 是接服务端的**唯一入口**）
- **玩家状态**：`IClientDataService`（`event DataChanged`）；**静态配置**：`IClientConfigService`（只 10 张表）
- **数值与说明**：`Assets/Client/Runtime/Stats/`（Model / Gateway / Service / Format / View 五层，已实现）—— 铁律：**客户端不做任何数值计算**，只取服务器下发值
- **UI 内核**：`IClientNavigation`（Open/Back/ReturnHome）· `IClientPageView`(Enter/Pause/Resume/Exit) · `IClientPopupService` · `IClientFeedback`；组合根 `ClientShellController`（**接口注入，非静态单例**）
- **启动链**：构建清单 `Boot(0) → ClientShell(1)`；`Assets\Main\Init.cs` 的 `LoadFirstSceneAsync` 对内置场景走 `SceneManager.LoadSceneAsync`；加载后逻辑由跨场景宿主 **`Assets\Main\WebGLPostSceneLoadRunner.cs`** 执行
- ⛔ 不要把已弃用的 `ClientBoot` 加回构建清单（会被启动链校验拦截）
- ⚠️ 这些"看起来该是表"的东西**不是表**：商店 / 活动 / 任务 / 邮件 / 公告 / 卡池 / 文案 / 货币（目前写在 `LocalClientDataService` 里）

## 7. 红线（违反即返工）

1. 不替换/升级/移除 URP、YooAsset、HybridCLR、XLua、微信 SDK、`Packages/manifest.json`、项目设置。
2. 不 `git reset` / 清理 / 覆盖工作区既有改动（含 `Logs\` 下的脚本、日志、`*.unity.bak` 证据）。
3. 不清理无法由版本控制/生成规则/引用分析证明安全的资源（先记 `BUG_TRACKER.md` 待核验项）。
4. 不在 UI 脚本里硬编码运营内容与数值；不在页面里写支付/发奖/扣币规则。
5. 不自行新建/生成 UI 节点、替换素材、调排版 —— **默认只改脚本 + 现有节点引用**；只有负责人明确要求"写入场景"才允许。
6. 不代替负责人操作 GUI（Unity Editor、IDE、构建窗口、微信开发者工具、平台后台）。
7. 不把密钥、账号、订单、个人资料、真实服务端地址写进仓库/日志/演示数据。
8. 同一实现路径第一次失败后，**没有新证据不重复尝试**。
9. 不凭现象断言 WebGL/微信问题的根因（必须先取日志/官方文档证据，否则只提"采集项"）。

**逐次授权文件（每改一次确认一次）**：`Assets\Main\Init.cs` · `Assets\Main\Boot.unity` · `ProjectSettings\EditorBuildSettings.asset` · `Assets\WX-WASM-SDK-V2\Editor\MiniGameConfig.asset`。

## 8. 汇报格式（每次交付都要说全）

改了什么 / 涉及文件 / 验证命令与结果（含日志路径）/ **未验证范围** / 遗留风险与待确认项 / 下一步建议。
未通过负责人 Play 验收的，只能写"**待验收**"，不得写"已完成"。

## 9. 先问我，不要自己拍板

交互稿未定义的规则、服务端契约、支付、账号、数值公式、活动周期与概率、最终美术与字体、性能预算、小程序正式环境域名、任何"删除/清理"动作、以及本 prompt 没覆盖到的架构级改动。

## 10. 你的第一个动作

1. 读 `D:\unity project\pinball\HANDOVER_DEV.md` 的 §0–§3、§9–§10；
2. 执行 `git -C 'D:\unity project\pinball' status --short` 并只报告数量（**不要做任何清理**）；
3. 跑 §5 的第 ① 条编译校验；
4. 用一段话向我复述：当前阶段、构建入口、UI ↔ 数据的唯一入口、以及**三件绝对不能做的事**；
5. 等我给任务，不要自行开工。
