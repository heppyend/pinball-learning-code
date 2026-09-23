# 非战斗客户端开发流程

> 2026-09-23 源码学习仓库同步：补入微信小游戏导出所需的本地 SDK、YooAsset 与编译依赖；补齐后按负责人要求未再次编译或导出。该仓库的原游戏美术仍不完整，正式构建沿用完整工程。详见 `README.md`。

## 当前范围

本流程用于微信小程序目标的 Unity 客户端制作：养成、英雄、背包、商城、邮箱、排行榜、活动、任务、扭蛋和个人中心等非战斗页面及其本地模拟逻辑。战斗/关卡、服务端、支付、账号、总架构和既有街机场景不在本流程范围。

页面和交互验收以 `D:\Users\Administrator\Desktop\J 界面交互草图-弹珠项目-小程序端.pdf` 为依据：已明确的文案、页签、跳转、筛选、空态、禁用态、领取反馈与弹窗必须实现；数值、服务端协议、支付和最终美术等未定义内容保持可配置并标为待确认。

当前环境内置的 `openai-docs` 是官方 AI 开发指南来源；它用于保持任务说明、证据和验证最小化，不是 Unity 运行时依赖。

## 每次任务的七步闭环

| 步骤 | 必做动作 | 退出条件 |
| --- | --- | --- |
| 1. 定义 | 明确页面、用户动作、状态与非目标；未给出的数值/协议标为待确认。 | 有最小计划和验收路径。 |
| 2. 定位 | 阅读 `CURRENT_STATE.md`、相关 `MODULE.md`、必要共享模块与工作区差异。 | 已知复用入口、受保护改动和禁止范围。 |
| 3. 设计 | 定义数据模型、模拟服务接口、页面流转和失败/空态。 | UI 不直接持有运营数据或服务端规则。 |
| 4. 实现 | 在 `Assets/Client` 完成最小可演示闭环；运行时与 Editor 代码分离。 | 可从客户端独立场景或正常导航抵达。 |
| 5. 验证 | 执行文本、逻辑测试和必要的命令行 Unity 编译。 | 记录命令、结果、日志位置和未覆盖范围。 |
| 6. 人工验证 | 提供 Unity Editor/微信开发者工具操作步骤。 | 明确通过、失败或待人工验证。 |
| 7. 记录 | 更新当前状态、模块说明、版本或风险条目。 | 下次会话能从当前状态继续。 |

## 客户端实施顺序

当前实施决策：在深入 C2-C5 各页面业务前，先保证主页每一个一级入口都有独立、可编辑的场景页面和返回闭环；未确认的业务规则仅保留可替换状态位，不以临时共用内容页代替最终页面结构。

1. **C0 隔离与骨架**：建立 `Assets/Client`、独立 `ClientShell` 场景、可在 Hierarchy 编辑的 Canvas/UI 根、导航/弹窗/提示接口和模拟数据边界。场景节点由命令行 `ClientShellDirectUpdater` 写入；生成后由场景/预制体维护，运行时只负责绑定与刷新。顶部 `Client` 菜单不作为构建入口，以避免覆盖人工调整。
2. **C1 大厅与个人中心**：入口矩阵、资料和装扮展示的本地闭环。
3. **C2 英雄、图鉴与背包**：筛选、详情、空态和数据刷新。
4. **C3 养成与装备**：升级、升星、材料校验、装备锁定/替换/卸下；规则须可测试。
5. **C4 商城、邮箱与排行榜**：限购、余额不足、邮件附件和榜单状态。
6. **C5 活动、任务与扭蛋**：活动周期、红点、领取、抽取和奖励入库的本地模拟闭环。
7. **C6 集成准备**：仅定义服务端/支付/平台适配接口，不做真实接入。

当前 C0 加载层检查点（2026-09-15）：新 `加载Page` 已替换至 `ClientCanvas/SystemLayer`，不再参与 `PagesLayer` 导航。页内 7 个 Image 均有数字 ID 赋值边界，TMP 文本有泛型赋值入口，加载进度和三标点时序已形成本地表现闭环。正式图片 ID 解析与实际 YooAsset/网络加载源尚未接入，不标记为线上加载链路完成。

当前 C2/C3 检查点（2026-09-14）：英雄列表任一卡牌到新详情主页的导航与立绘传递、详情数据刷新、三个技能页互斥切换及两项资源的本地升级校验已形成模拟闭环，并已由负责人在 Unity 中人工验证无报错。正式材料配方、装备槽动作、图片资源映射和 Tips 弹窗未确认，保持“待接入”，不计为线上功能完成。

当前 C5 检查点（2026-09-14）：`ActivityPage活动` 已由负责人完成人工验证。新手/进阶任务采用独立分类数据，三页互斥、蓝底黄条离散进度、满进度领取态、奖励卡数量保持和不可重复领奖已形成本地模拟闭环。真实任务持久化、服务端领奖确认、邮件投递、图片 ID 映射与 Tips 弹窗仍待接入。

当前 C5 全图鉴活动检查点（2026-09-16）：现有 `Complete Guide Event Page全图鉴活动` 已注册为 `ClientUiPageId.CompleteGuideEvent`，活动列表现有卡片没有预挂 Button，因此由 `ClientActivityPage` 在运行时为既有卡片 Image 补充点击入口，经统一导航进入并返回活动列表。全部现有 Image/Text 均有数据赋值边界，规则弹窗默认隐藏，进度和领取状态形成可演示的本地模拟闭环。正式活动规则、周期、奖励契约、图片 ID 解析、服务端状态与统一 Tips 未接入，人工 Play 验证未完成，不标记为完成。

## 结构规则

```text
Assets/Client/
├─ Scenes/        独立客户端开发场景，不加入既有战斗启动链
├─ Runtime/       领域模型、服务、页面控制器与导航适配（含 Stats/ 数值与说明模块）
├─ UI/            客户端预制体、主题和图集（后续经 YooAsset 策略接入）
├─ Editor/        编辑器工具与**纯逻辑自检套件**（`ClientSelfTest.cs`、`ClientTableProbe.cs`、`ClientShellStructuralRepair.cs` 等）
└─ MODULE.md      模块边界与验证入口
```

> **2026-09-20 更正**：原文此处写的是 `Tests/`（可纯逻辑验证的编辑器/运行时测试）。实测**不存在 `Assets/Client/Tests/`**；
> 由于 `Assets/Client` 没有 asmdef（代码在预定义程序集 `Assembly-CSharp`，而 **asmdef 测试程序集无法引用预定义程序集**），
> 无法直接用 Unity Test Framework，故改为 **Editor 自检套件 + 命令行退出码**（详见 `Assets/Client/MODULE.md` §19）。
> 若将来要启用 Unity Test Framework，需先把客户端改为 asmdef —— 属架构变更，**需负责人授权**。

- 所有运营内容和数值通过配置/模拟服务提供；页面控制器不得硬编码商品、英雄、货币或奖励数据。
- 客户端 UI 的唯一宿主是 `ClientShell` 内的 `ClientCanvas`；禁止在 Canvas 外创建客户端业务 UI 根节点。

  > ⚠️ **2026-09-20 更正（原文与实况冲突）**：本行原文为"**所有可导航页面必须位于 `PagesLayer`**，弹窗位于 `PopupLayer`，全局提示/遮罩/加载位于 `SystemLayer`"。
  > **实测场景并非如此**：`ClientCanvas` 下为 `PopupLayer` / `PagesLayer` / `SystemLayer` / `ToastRoot`，
  > 其中 **`PagesLayer` 只有 `MainPage主页`（大厅）**，而**其余客户端页面（英雄/编队/个人中心/商店/邮件/排行/活动…）都在 `PopupLayer`**；
  > `SystemLayer` **恒为 inactive**（唯一子节点是 `加载Page`），Toast 实际挂在 `ClientCanvas` 下。
  > 事实依据与三方案影响分析见 `BUG_TRACKER.md` **RISK-020**（待负责人选 A/B/C）。**在定案前，本行不再作为强制约束。**
- 页面必须使用 `ClientUiPageId` 与 `ClientUiPage` 注册到 `ClientUiNavigator`，通过统一生命周期和返回栈流转；禁止新增平行导航器、页面静态事件链或运行时临时业务页面根。
- 新资源接入前须确认 YooAsset 地址、分包、生命周期和回收策略；本阶段不移动既有资源。
- 当页面尚无素材时，按交互草图保留可替换的结构化版式节点（标题栏、内容区、列表/卡片、状态区、固定操作区、返回入口），并使用命名清晰的 `Image`/`Text` 节点作为替换位；禁止退化为通用纯色占位页。
- 目录迁移或清理必须先在 `BUG_TRACKER.md` 记录候选和证据。只有可再生产物可由负责人确认后清理。

## 当前 C0 验收

- `ClientShell` 可独立打开，且不加载战斗或旧街机场景。
- 已有客户端模块入口与目录职责；没有改变 `Boot`、Build Settings、YooAsset、HybridCLR、XLua、URP 或微信 SDK。
- 已列出本地模拟数据边界和后续第一个页面的待确认项。
- Unity 编译输入有变动时，须以本工程的 Unity `2022.3.57f1c2` 批处理编译验证；若 Unity 已被占用，记录日志并由负责人释放后再执行。

## 人工验证模板

```text
前置条件：Unity 2022.3.57f1c2，工作区保存完成。
操作步骤：打开 Assets/Client/Scenes/ClientShell.unity，点击 Play。
预期结果：仅显示客户端开发场景；不切换到 Boot、Login、Main 或任何战斗场景。
失败排查：检查 Console、场景引用和 Client 模块新增资源的 GUID。
请回传：Console 首条 Error/Exception、截图及复现步骤。
状态：待人工验证。
```

## 2026-09-03 TCP 会话门禁记录

- 当前仅完成探针源码修复，目标为 HTTP 网关、TCP、ClientGT 握手/游客登录和 ClientPF GetHero 的本地可审计会话；不代表线上功能完成。
- 在 Unity 命令行编译通过前，不执行 WebGL 构建、小游戏转换、OSS 上传或微信开发者工具验证。
- 命令行编译因同工程 Unity Editor 已占用项目而未进入脚本编译，日志为 `Logs/codex-tcp-client-session-compile.log`；需负责人释放 Editor 后重试并记录结果。

## 2026-09-04 TCP 会话编译记录

- 已以 Unity `2022.3.57f1c2` 命令行执行 `Pinball.EditorTools.WebGLHotUpdateDllCommand.CompileAndSync`，日志为 `Logs/codex-tcp-session-hotfix-sync.log`；包含 `Tundra build success`、`[WebGL 热更新程序集] 已编译并同步 HotFix.dll` 与 `Exiting batchmode successfully now!`，未检出本轮 `error CS`。
- 后续仍需负责人使用现有 YooAsset/微信 GUI 流程构建资源包、导出/转换小游戏并回传从 `[TCP客户端] 会话开始` 到关闭的完整日志；未进行 OSS/CDN 上传。

## 2026-09-20 客户端阶段状态与决策记录

### 一、本会话已完成（均有可复查证据）

| 项 | 结论 | 证据 |
|---|---|---|
| 客户端**表驱动** | 10 张客户端表（Hero / Head / HeadFrame / Badge / Nameplate / Title / Item / Skill / Potency / RandomHero）读入客户端模型；英雄可见 **49**、首个 `13003 蝎子精` | `Logs/b1-prebuild-baseline.log` |
| `Stats` 数值与说明模块 | 独立模块（端口-适配器 + MVC + 服务端权威 + 零 GC 目标），页签 3、属性与分档说明可用 | `Logs/stats-module-probe3.log`、`Runtime/Stats/MODULE.md` |
| **ProfilePage 收口** | 5 个收藏品列表（收敛为模板）+ 主页展示模板卡 + **BUG-018**（玩家名/ID/战力）；场景写入 53 处变更、15 字段全接线 | `Logs/repair-profilelists.log` |
| **BUG-020** 购买进度条 | `_progressFill` 接线完成 | `Logs/repair-shopbug020.log` |
| **BUG-019** 可做部分 | 排行榜品质（5 项）与立绘（4 项）查找表按实际贴图重建 | `Logs/repair-rankvisuals.log` |
| **BUG-023 ②** 道具类型语义 | 枚举按表对齐（`Currency=1`/`Consumable=3`，`2` 未定义）+ 5 处硬编码改为表驱动 | `Logs/verify-itemtype.log` |
| **阶段 A3** 自动化测试 | 自检套件 **70 项断言 / 退出码 0**，含远端 pkg 读取链自动化 | `Logs/self-test.log` |
| WebGL 降风险 | gzip 换工程同款 `Unity.IO.Compression`；PC 网络整段平台条件编译 | `Logs/pkg-verify*.log`、`verify-compile-*.log` |
| B0 门禁 / B1 / B2 文档 | `WEBGL_B0_GATE.md`（含硬阻断）、`WEBGL_B1_BUILD.md`、`WEBGL_B2_DIAGNOSTICS.md` | 三份文档 |

### 二、未完成 / 阻断

- 🔴 **`BUG-025`（阶段 B 硬阻断）**：`EditorBuildSettings` 只有 `Assets/Main/Boot.unity`，**`ClientShell` 不在构建清单**且**无任何代码加载它**
  ⇒ 现状构建出的 WebGL 包**进不到客户端 UI**。三方案（A 加进清单并置首 / B 在 Boot 后追加加载 / C 新建 `ClientBoot` 场景）**待负责人选**。
- **两个"关不掉的弹窗"结论已更正**：`Purchase Success Page` **本来就能关**（运行时补点击处理）；`Success Receipt Interface` 是**不可达死内容**。
- **活动页无缺口**：32/32 期望节点名精确命中、数据与入口齐备（此前"只有结构"的记录**过时**）。
- **MailPage 唯一真实缺口**：`_detail` / `_status` 在场景里**没有文本节点**（`BUG-024`，三方案待选）。
- **人工 Play 验收尚未进行**：ProfilePage 收藏品列表、主页展示、BUG-018 三项**只差负责人在 Unity 里 Play 验收**（预期：列表 `1/1/1/3/1` 项，与自检基准一致）。

### 三、负责人决策记录（本会话）

| 决策 | 内容 |
|---|---|
| id 类型 | 配置相关 id **全改 `int`**；服务端不透明标识（PlayerId/ListingId/…）与美术**名字**保持 `string` |
| 元素定义 | `1..6 = 光 / 水 / 土 / 风 / 火 / 暗` |
| 战力 | **暂不确定，先标"待确认"**（配置表无战力字段，属服务器下发） |
| 范围边界 | 只负责 `Assets/Client` 与平台移植；战斗端不碰 |
| ProfilePage | **授权全部场景写入**（主页展示 + 5 个收藏品列表）；**BUG-018 可做** |
| 道具类型 | 语义修正方向**已同意**（`1` 货币 / `3` 消耗品 / `2` 表内无）—— 已于第 70 轮落地 |
| `SystemLayer` | 负责人要求**先给影响分析再定**（`RISK-020` 已给 A/B/C 三方案） |

### 四、待负责人拍板（阻塞后续）

1. **构建入口 A/B/C**（`BUG-025`，阶段 B 前置）；
2. 邮件正文 A/B/C（`BUG-024`）；
3. `SystemLayer` 分层 A/B/C（`RISK-020`）；
4. 缺图资源 4 类（`HeroAvatar_*` / `AvatarFrame_*` / `Medal_*` / `AttributeIcon_*`）**等美术**还是**走热更包**；另 `ElementIcon_*` **缺元素 6（暗）**；
5. 战力展示口径（`玩家战力 = 100` vs `当前队伍战力 = 0`）；
6. **装备数据来源**（表内无"装备"类型 ⇒ `TrySetEquipmentEquipped` 当前恒失败）。

### 五、验证入口（命令行，可复现）

```powershell
# 编译三目标（Editor / Assembly-CSharp-Editor / WebGL profile）
& '<工程>\Logs\verify-compile.ps1' -Profile All

# 纯逻辑自检（失败即非 0 退出）
& '<Unity>\Unity.exe' -batchmode -quit -projectPath '<工程>' `
  -executeMethod 'Pinball.Client.Editor.ClientSelfTest.RunAll' -logFile '<工程>\Logs\self-test.log'

# 构建前全量基线（真机对账用）
& '<Unity>\Unity.exe' -batchmode -quit -projectPath '<工程>' `
  -executeMethod 'Pinball.Client.Editor.ClientTableProbe.LogFullBaseline' -logFile '<工程>\Logs\b1-prebuild-baseline.log'
```

> 批处理子进程须临时传 `ALLUSERSPROFILE=C:\ProgramData`（不写入系统环境变量）。

## 2026-09-22 移植阶段状态与决策记录（**阶段 B 达成**）

### 一、阶段状态

| 阶段 | 状态 | 证据 |
|---|---|---|
| B1 可复现构建链路 | ✅ | 一条命令出包（`Logs/webgl-port/run-pipeline-2phase.ps1`），两阶段合计约 9.2 min，`WXConvertCore.DoExport` 返回 `SUCCEED` |
| B 可启动 / 可操作 | ✅ | 微信开发者工具容器内进客户端；首页 → 编队页 → 新手任务页可跳转 |
| B 真实会话（TCP） | ✅ | 容器与**真机**均六包齐全（网关成功 → 建连 → 握手 → 游客登录 → 英雄数据 英雄=4/英雄组=1 → 成功退出） |
| B2 真机 | ✅ 首轮通过 | Android 1080×2400；首页正常渲染、导航可用、TCP 六包、`RT-FPS 50~63 / Jank 0 / Stutter 0.00%`、无致命异常。记录见 `WEBGL_B2_REALDEVICE.md` §八 |
| B2 矩阵深度项 | ⬜ 未完成 | 安全区与遮挡、返回栈压测、弱网/断网、后台切回、**内存峰值未采集** |
| C 文档交接 | 🔄 进行中 | 移植侧新增 `WEBGL_MINIGAME_HANDBOOK.md`（总入口）；UI 侧页面/弹窗矩阵由原作者会话维护 |

### 二、决策记录（负责人 2026-09-22）

| 决策 | 内容 |
|---|---|
| 首页加载方案 | 选定并授权**方案 A**：`Init.cs` 用 `SceneManager.LoadSceneAsync` 加载构建清单内的 `ClientShell`，不动 YooAsset 收集器（对比见 `HOME_PAGE_SCENE_FIX_OPTIONS.md`） |
| A4 加载后逻辑 | 授权修复「`Single` 模式销毁 `Init` 协程 ⇒ 加载后代码不执行」：新增跨场景宿主 `WebGLPostSceneLoadRunner.cs`；**不改 `Init` 生命周期**（探针依赖 Boot 被卸载） |
| 真机域名校验 | 实测：**手机开「调试」可绕过**域名校验（A/B 对照取证）；正式版/体验版仍需配合法域名 |
| 生产化前置 | 发现服务与 TCP 网关均为**裸 IP + HTTP**，**无法加入合法域名白名单** ⇒ 正式环境需服务端提供 HTTPS + 已备案域名（**服务端范围，代理不改客户端 IP/端口**） |

### 三、对 2026-09-20 记录的更正（保留原文，不删）

- 09-20 记录的 **🔴 `BUG-025`（阶段 B 硬阻断）已解决**：构建清单现为 `Boot(0) → ClientShell(1)`，
  `Init` 按 `ClientStartScene` 原生加载 —— 关闭条件「构建产物能进入客户端 `ClientShell`」已由构建日志与容器/真机运行证据满足。
  注意：最终落地方式与 09-21 记录的「方案 C（`ClientBoot` 置首）」不同，`ClientBoot` 已退出启动链。
- 09-20 记录的「人工 Play 验收尚未进行」等 UI 侧事项**仍由原作者会话维护**，本记录不覆盖。

### 四、本次新增的可复现验证入口

```powershell
# 一键出包（先关闭 Unity 编辑器）
pwsh -NoProfile -ExecutionPolicy Bypass -File '<工程>\Logs\webgl-port\run-pipeline-2phase.ps1' -PackageVersion 2026-09-22-1420

# 编译三目标（含新增文件发现）
pwsh -NoProfile -ExecutionPolicy Bypass -File '<工程>\Logs\verify-compile.ps1' -Profile All -DiscoverRoot 'Assets\Main'

# 上传后自检 / 重传清单
Get-Content '<工程>\Logs\webgl-port\OSS_UPLOAD_MANIFEST-A4.txt'
```

> 移植侧完整手册见 **`WEBGL_MINIGAME_HANDBOOK.md`**；真机步骤见 `WEBGL_B2_REALDEVICE.md`。
