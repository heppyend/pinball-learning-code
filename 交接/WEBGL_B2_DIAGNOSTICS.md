# 微信小游戏 / WebGL「B2 真机与首屏证据采集」清单

> 依据：`HANDOVER_PLAN.md` §5 **B2 真机回归矩阵** + `AGENTS.md` §4 —— **黑屏/卡顿/资源/渲染问题不得仅依据现象猜测根因或实施修复；
> 必须先引用官方文档、SDK 当前实现，或添加可回收的最小调试检测取得日志证据。证据不足时只提出采集项。**
> 建立：2026-09-20（第 67 轮）。配套：`WEBGL_B0_GATE.md`（门禁）、`WEBGL_B1_BUILD.md`（构建）。
> **本文件只列"采什么、在哪采、看什么"；不写根因。**

---

## 0. 两条纪律（写在这里防止被绕过）

1. **证据不足不写根因。** `BUG-009` 的关闭条件是"取得可将首屏表现与具体日志或可回收检测关联的证据"；
   `BUG-013` 的关闭条件是"不再报 `app.json` 启动页错误，并出现 WebGL 上下文、Unity 资源包初始化和大厅演示日志"。
2. **不动基础设施换线索。** 不升级/调整 URP 包、渲染资源、桌面配置；**不手改生成工程**、不改 `WAGame.js`（`BUG-009` / `BUG-013` 均已写明）。

---

## 1. 可用的日志通路（已确证存在）

| 通路 | 用法 | 证据 |
|---|---|---|
| **Unity 侧 `Debug.Log`** | 常规 Console 输出 | 客户端已有大量标签，见 §2 |
| **`WX.SetEnableDebug`** | 打开微信调试开关（真机/开发者工具） | `WX.cs:2294` |
| **`WX.GetLogManager({level})`** | 取微信日志管理器 | `WX.cs:4507` |
| **`WX.GetRealtimeLogManager()`** | **实时日志**（真机黑屏时最有用：可在小程序后台看，不必连数据线） | `WX.cs:4519` |
| 开发者工具 Console | 导入 `minigame/` 后导出完整 Console | 构建产物见 `WEBGL_B1_BUILD.md` §2 步骤⑤⑥ |

> ⚠️ 上述 API 均为**微信官方接口**（SDK 注释里带官方文档链接）；代理未在真机验证过它们的可用性，**首次使用即属采集项**。

---

## 2. 客户端已有的日志标签（采日志时按这些行判读）

| 标签 | 何时打印 | 说明 |
|---|---|---|
| `[配置表] 客户端读表路径：…` | 客户端初始化读表时 | 区分"本地 `Resources/Table`"与"远端落盘包 `config.pkg`" |
| `[配置表] 汇总：可用 N 张 / 空 N 张 / 缺失 N 张 / 解析失败 N 张` | 同一次初始化 | **`解析失败 > 0` 即表链有问题** |
| `[配置表] <表名> = N 行（ClientShow!=0 的 M 行）` | 逐表 | 行数是否与桌面侧一致（桌面基准：`Hero = 54 行`） |
| `[Client] …` | 页面/服务日志 | 通用前缀 |
| `[结构修复] …` | 仅编辑器工具 | 真机不会出现，别拿它当判据 |

> **桌面侧基准**：`Logs/pkg-verify4-local.log`（本地表）与 `Logs/pkg-verify3.log`（远端 pkg 变异测试）。
> 真机日志应与前者**同构**；若少了 `[配置表]` 段，说明**初始化没跑起来**，问题在更早的启动阶段。

### 2.1 桌面侧全量基线（第 68 轮实测，真机照此对账）

命令（`WEBGL_B1_BUILD.md` 步骤①的完整版）：

```powershell
$env:ALLUSERSPROFILE = 'C:\ProgramData'
& 'C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe' -batchmode -quit `
  -projectPath 'D:\unity project\pinball' `
  -executeMethod 'Pinball.Client.Editor.ClientTableProbe.LogFullBaseline' `
  -logFile 'D:\unity project\pinball\Logs\b1-prebuild-baseline.log'
```

实测值（`Logs/b1-prebuild-baseline.log`，`error CS 0` / 异常 0 / 解析出错 0）：

| 项 | 基线值 |
|---|---|
| 平台 / 读表路径 | `WindowsEditor` / `本地 Resources/Table（未发现可用的 config.pkg）` |
| 表汇总 | **可用 10 张 / 空 0 / 缺失 0 / 解析失败 0**；`Hero = 54 行（可见 49）` |
| 英雄列表 | **49 个**，首个 `Id 13003 / 蝎子精 / 元素1(光) / 品质3 / 1星` |
| Stats | `页签数 = 3`、`普攻 = 捣药杵击`、`属性 id1 = 1500` |
| 玩家 | 名称 `体验玩家`、ID `local-player`、战力 `100`【待确认】 |
| 主页展示 / 主力英雄 | `13003` / `13003`；编队队伍 `0 / 4`，**当前队伍战力 = 0** |
| 装备弹珠 | `marble-001`（字符串 id，属服务端不透明标识） |
| 背包 / 商城 / 卡池历史 | `3` / `3` / `0` |
| 邮件 / 任务 / 公告 / 弹珠 | `2` / `2` / `2` / `2` |
| 排行榜 | 战力榜 `5` 行 / 挑战榜 `5` 行 |
| 活动任务 | `Newbie = 5`、`Advanced = 5`；赛季活动 `6` 个 |
| 收藏品（表内可见 / 已拥有 / 使用中） | Head `1/1/10001`、Badge `1/1/10002`、Nameplate `1/1/10001`、**HeadFrame `3/2/10002`**、Title `1/1/10002` |

**真机判读**：上表任一项缺失或数量不同，先记录**差异本身**，再回到 §3/§4 找"卡在哪一步"；
**不要**用"数量不同"直接推断服务端或配置问题。

> 顺带记录一个**数据层观察（非缺陷，待确认）**：`玩家战力 = 100` 而 `当前队伍战力 = 0` —— 前者是本地模拟的展示值，
> 后者是"队伍各槽位英雄战力之和"（英雄战力未由配置提供，属服务器下发）⇒ 两者口径不同，需负责人确认展示口径。

### 2.2 客户端启动阶段判定表（真机日志"卡在哪一步"速查）

**场景根结构（实测）**：`ClientBootstrap`（场景根节点）与 `ClientCanvas`、`EventSystem` 平级；
`ClientCanvas` 下为 `PopupLayer` / `PagesLayer` / `SystemLayer` / `ToastRoot`；
入口组件：`ClientCanvas` 上有 `ClientShellController` + `ClientUiNavigator` + `ClientSystemFeedback` + `ClientUiClickTracer`。

> ⚠️ **不能断言日志先后**：工程**没有 `ProjectSettings/MonoManager.asset`**（未配置脚本执行顺序）
> ⇒ 各 `Awake/Start` 的先后**未定义**。因此下表是**"应出现的标记集合"**，判读方式是"**哪条缺失**"，而不是"顺序对不对"。

| 标记（原文前缀） | 打印位置 | 缺失 ⇒ 说明卡在 |
|---|---|---|
| `[配置表] 数据源：…` | `TableClientConfigService`（`Report`） | 配置服务未启动（`ClientServices.InitializeForDevelopment` 没跑到） |
| `[配置表] 其他：道具 N(…)、技能 N(…)` | 同上 | 同上 |
| `[Client] Local development data initialized.` | `ClientBootstrap.cs:12-14` | 客户端启动脚本未执行（**场景根本没加载** ⇒ 先查 `BUG-025`） |
| `[Client] 组合根注入完成：弹窗宿主 N 个，提示宿主 N 个。` | `ClientShellController.cs:96` | 组合根未执行，或弹窗/提示服务缺失（后者会另有 warning） |
| `[Client] PopupLayer 收录弹窗：按 pageId 路由 N …` | `ClientPopupService.cs:91` | 弹窗层未收录节点 |
| `[Client] 从场景自动收录 N 个未登记页面。` | `ClientUiNavigator.cs:105` | 导航未收录页面（页面不可达） |
| `[Client] 导航能力已注入 N 个页面视图，导航观察者 N 个。` | `ClientUiNavigator.cs:345` | 页面视图未注入（点击无反应但不报错） |

**典型组合判读**：

| 现象 | 结论方向 |
|---|---|
| **一条 `[Client]`/`[配置表]` 都没有** | 场景没加载 / 卡在 Wasm 或资源初始化之前 ⇒ 走 §3（`BUG-009`）与 §4（`BUG-013`）采集，**不要**先怀疑表或 UI |
| 有 `Local development data initialized.` 但无 `[配置表]` | 配置服务初始化失败 ⇒ 采 `ClientServices` 那条"配置表未就绪"warning 与异常原文 |
| 有 `[配置表] 数据源：…` 但 `解析失败 > 0` | 表链问题（与真机验证点①同源） |
| 有组合根/导航标记但无页面数据日志 | 页面未进入（导航未触发）⇒ 记 `Open(...)` 追踪行（`ClientUiTrace`） |

> 运行时追溯开关：`ClientShellController` 里有 `ClientUiTrace.Line("追踪", ...)`，导航/弹窗的关键动作都以
> `ClientUiTrace.Line("导航"/"弹窗", …)` 输出；真机采集时**保留这些行**（它们才是"点不动/关不掉"类问题的证据）。

---

## 3. `BUG-009`（首屏黑屏）采集清单

| # | 采集项 | 在哪采 | 判读要点 |
|---|---|---|---|
| 9-1 | 开发者工具**完整 Console**（首次加载后**等 ≥30 秒**再导出） | 微信开发者工具 | 是否出现 **WebGL2 上下文**创建的日志 |
| 9-2 | 是否出现 `资源包初始化成功` / YooAsset 初始化日志 | 同上 | 无 ⇒ 卡在资源初始化之前 |
| 9-3 | 是否出现客户端 `[配置表]` 段 | 同上 | 无 ⇒ 客户端初始化未执行（多半是 `BUG-025` 场景清单问题） |
| 9-4 | 全部 `Error` / `Exception`（含 Wasm / 网络 / patch） | 同上 | 逐条贴出原文，不要转述 |
| 9-5 | 缺失 Shader 日志的**完整列表**（含 `Hidden/Universal/*`） | 同上 | `Hidden/Universal/HDRDebugView` 一类通常**不影响可见画面**；需与"是否真的黑"分开判断 |
| 9-6 | 首屏截图 + 是否**部分可见**（黑屏 / 全白 / 有 UI 无 3D / 有 3D 无 UI） | 截图 | 形态不同指向不同阶段，必须如实记录 |
| 9-7 | 清缓存后**再采一次** | 开发者工具 | 排除缓存因素（与 `BUG-013` 的既有规避一致） |
| 9-8 | 真机 `GetRealtimeLogManager` 日志（若开发者工具无法复现） | 真机 + 小程序后台 | 黑屏只真机复现时，这是唯一通路 |

**不要写**："URP Shader 不支持导致黑屏" —— 现有证据只表明二者**同时发生**（`BUG-009` 原文已注明）。

---

## 4. `BUG-013`（启动页 / app.json）采集清单

| # | 采集项 | 在哪采 | 判读要点 |
|---|---|---|---|
| 13-1 | 导出工程里 `game.json` / `app.json` 的**启动页字段**内容 | 生成工程（**只读，不改**） | 是否仍报"启动页未定义" |
| 13-2 | 转换窗口日志 + 转换产物清单 | `WXEditorWindow` / `minigame/` | 是否有转换阶段的告警 |
| 13-3 | 加载阶段日志序列：`game starting` → `mgp inited` → 公共目录存储 patch → 性能检测 → **？** | 开发者工具 | 现有记录**停在性能检测**，其后应有 WebGL2/Wasm/资源包初始化 |
| 13-4 | 是否出现 `webgl.wasm.framework.unityweb.js` 加载成功 | 同上 | 该文件名见 `WXConvertCore.cs:88` |
| 13-5 | `webgl.worker.js` 是否加载（线程关闭时行为可能不同） | 同上 | 项目 `webGLThreadsSupport = 0`，worker 用途需与 SDK 核对（**待查证，不预设**） |
| 13-6 | 首屏背景图是否为 SDK 默认图 | 视觉 | 默认图路径见 `WEBGL_B1_BUILD.md` §2 步骤⑤ |

---

## 5. 三个 WebGL 验证点（本会话新代码引入，与 `WEBGL_B0_GATE.md` §6 同一清单）

| # | 验证点 | 真机判据 | 失败时先采什么 |
|---|---|---|---|
| **①** | 表读取链：`persistentDataPath/config.pkg`（`Unity.IO.Compression` 解压）+ 回落 `Resources/Table` | 出现 `[配置表] 客户端读表路径：…` 且 `解析失败 0 张`；有 pkg 时应显示"远端落盘包 config.pkg（N 张表）" | 该行原文 + `config.pkg` 是否真实存在 + 文件大小 |
| **②** | `Span` / `stackalloc`（`ClientDescriptionFormatter`） | 英雄详情三个页签说明文本正常显示（形如 `8%→9%→80%` 的分档替换生效），无异常 | `Exception` 原文（若抛，必带类型与堆栈） |
| **③** | `Resources/Table` 体积（+174 KB）与首包 | 转换工具报告的包体数字 | 包体报告截图/文本、是否超首包限制 |

---

## 6. 真机回归矩阵（`HANDOVER_PLAN.md` §5 B2 的落地表，只列"采什么"）

| 类别 | 采集点 |
|---|---|
| 启动 | 首次启动 / 资源初始化 / Loading 完成·失败·重试 / **后台切回** |
| UI | 分辨率与安全区 / 横竖屏策略 / 字体 / 触摸 / 滚动 / 返回 / 遮罩与输入阻断 |
| 导航 | 页面互斥 / 返回栈 / 弹窗栈 / **Toast·Loading 是否残留** |
| 数据 | 本地模拟初始化 / 空态 / 奖励与货币刷新 / 存档边界（若确认） |
| 资源 | 首包 / 分包下载 / 缓存 / 资源失败提示 / 内存峰值与回收 |
| 平台兼容 | 不支持的 API / 网络限制 / 音频·渲染·Shader / 异常日志 |
| 稳定性 | 连续页面跳转 / 重复开关弹窗 / 弱网断网（如适用）/ 冷启动与热重进 |

每条记录格式（`HANDOVER_PLAN.md` §5 B2 强制）：
**设备型号 / 系统 / 微信版本 / 构建标识 / 复现步骤 / 截图或日志路径 / 结论 / 下一动作**。**未测试不能标记通过。**

---

## 7. 若日志仍不足：最小可回收诊断（**提案，未实现**）

只有当 §3/§4 的采集**确实无法区分**卡在哪个阶段时，才考虑加一个**可回收**的启动阶段标记，例如：
在客户端初始化各阶段各打一行**带统一前缀**的日志（`[Boot] 0/4 场景就绪`、`1/4 服务初始化`、…），
并在真机上用 `WX.GetRealtimeLogManager()` 回传。

**约束**：不引入新依赖、不常驻轮询、可通过删除少量调用整体回收；**是否需要加，等采集结果出来再定**（现在加属于无证据的猜测性投入）。
