# 微信小游戏构建与真机手册（阶段 C 交接 · 移植侧总入口）

> 建立：2026-09-22 · 维护范围：Unity WebGL → 微信小游戏的**构建、导出、上传、运行、真机验证**
> 状态：**阶段 B 五项全部达成**（可构建 / 可启动 / 可操作 / 真实会话 TCP / 真机）
> 本文是移植侧的**唯一入口**；构建细节、诊断、真机矩阵分别在 `WEBGL_B1_BUILD.md` / `WEBGL_B2_DIAGNOSTICS.md` / `WEBGL_B2_REALDEVICE.md`。

---

## 0. 权威文档索引

| 文档 | 作用 |
|---|---|
| **本文件** | 移植侧总入口：环境事实 / 一键出包 / 上传规则 / 运行与取证 / 排查 / 坑清单 |
| `HANDOVER_PLAN.md` | A/B/C 三阶段总纲与阶段门 |
| `CURRENT_STATE.md` | 全项目唯一短入口（当前状态、检查点、阻塞、下一步） |
| `WEBGL_B0_GATE.md` | 迁移前门禁：工具链版本、WebGL 暴露面、兼容矩阵、首包预算 |
| `WEBGL_B1_BUILD.md` | 构建链路细节 |
| `WEBGL_B2_DIAGNOSTICS.md` | 诊断手段与日志采集 |
| `WEBGL_B2_REALDEVICE.md` | **真机手册**：前置条件、三条测试路径、判据、B2 矩阵、**§八 真机实测记录** |
| `HOME_PAGE_SCENE_FIX_OPTIONS.md` | 「首页不显示」根因、方案 A/B 对比、**§九 实施与复验记录** |
| `TCP_WEBGL_HANDOFF.md` | TCP 那条路的权威记录（⚠️ 当前不在工作区，见 §7） |
| `TCP_NETWORK_BREAK_ROOTCAUSE.md` | TCP 断链完整分析与修复证据链 |
| `Assets/Main/MODULE.md` | 启动模块（`Init` / `Boot` / 新增跨场景宿主）的职责与约束 |
| `Assets/Client/MODULE.md` | 客户端模块（另一会话维护） |

---

## 1. 环境事实（照抄即可复现）

| 项 | 值 |
|---|---|
| 工程路径 | `D:\unity project\pinball` |
| Unity | `2022.3.57f1c2`（`C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe`） |
| 基础设施 | URP + YooAsset 2.1.1 + HybridCLR + XLua + 微信小游戏 SDK（`Assets/WX-WASM-SDK-V2`） |
| 小游戏 AppID | `<微信小游戏AppID>`（项目名"弹珠英雄"，`compileType: game`） |
| OSS | Bucket `localpinball-oss`；Endpoint `oss-cn-shenzhen.aliyuncs.com` |
| CDN 目录 | `1/HotUpdate/NewAB/WebGL/pinball-client/`（**根目录**，见 §3） |
| 导出目录 | `D:\Users\Administrator\Desktop\弹珠接入text_1\720`（`webgl/` = Unity 输出，`minigame/` = 小游戏包） |
| 微信开发者工具 | `D:\WeChatDeveloperTool\微信web开发者工具` |
| 网关发现服务 | `http://<服务器地址>:8000/router/rest`（**裸 IP + HTTP**，见 §6.2） |
| TCP 网关 | `<服务器地址>:53413`（由发现服务下发） |
| 区服 | `SysDefines.ZoneId = 1`（不等于 <0，因此**不走**公网备用环境 `<服务器地址>:8000`） |
| 部署机 | 开发机 `<服务器地址>`；网关机器 `<服务器地址>`（**另一台机器**，同局域网） |

> 代理**不得**修改 IP / 端口 / 服务端 / 账号 / token（`TCP_WEBGL_HANDOFF.md` 硬约束）。

---

## 2. 一键出包（唯一推荐入口）

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File 'D:\unity project\pinball\Logs\webgl-port\run-pipeline-2phase.ps1' -PackageVersion <版本号>
```

| 要点 | 说明 |
|---|---|
| **前置** | **必须先关闭 Unity 编辑器**。脚本会强杀残留 Unity 进程（`Stop-UnityLeftovers`），编辑器内有未保存改动会丢 |
| 两阶段原因 | 阶段 A 编译 WebGL 程序集 + 同步 `HotFix.dll.bytes`，再 `AssetDatabase.Refresh()` 触发域重载（**跨进程**才能让下一次构建用上新程序集），避免 `script class layout is incompatible between the editor and the player` |
| 阶段 A / B | A：编译 + 同步热更 DLL；B：YooAsset 资源包 → WebGL 构建 → 微信转换 |
| 实测耗时 | A 约 1.3~1.8 min，B 约 7.3~7.9 min，合计约 **9.2 min** |
| 版本号 | 沿用已有版本（如 `2026-09-22-1420`）⇒ YooAsset bundle 内容不变，**上传面最小** |
| 成功判据 | 末行 `All requested phases succeeded.` + 日志内 `4/4 WXConvertCore.DoExport 返回：SUCCEED` |
| 构建日志 | `Logs/b1-phaseA-<时间戳>.log`、`Logs/b1-phaseB-<时间戳>.log` |
| YooAsset 产物 | `Bundles/WebGL/DefaultPackage/<版本>/` |
| 环境变量 | 脚本已自行设置 `ALLUSERSPROFILE=C:\ProgramData`（批处理子进程要求） |

**构建日志里必须复核的三行**（缺一即视为出包不完整）：

```
[启动链校验] 构建清单场景数 = 2
[启动链校验]   场景[1] enabled=True path=Assets/Client/Scenes/ClientShell.unity
[Builder] Scenes [1]: Assets/Client/Scenes/ClientShell.unity, [x]
```

---

## 3. OSS 上传规则（**踩过两次的坑**）

### 3.1 两条路径，互不相关

| 类型 | 客户端请求的路径 | 依据 |
|---|---|---|
| **YooAsset**（清单 + bundle） | `<CDN>/<fileName>` = **根目录** | `Init.cs` `RemoteServices.GetRemoteMainURL(fileName) => $"{host}/{fileName}"` |
| **微信 SDK 首包**（data / wasm） | `<CDN>/<文件名>` = **根目录**（`dataFileSubPrefix: ''`） | `minigame/unity-namespace.js:25`；文件名由 `game.js` 的 `DATA_FILE_MD5` / `CODE_FILE_MD5` 决定 |
| 微信 SDK 的**纹理/音频**流式加载 | `<CDN>/Assets/…` | `game.js` `gameManager.assetPath = DATA_CDN + '/Assets'`（当前未启用纹理流，实测该路径 404 无害） |
| YooAsset 的**内置版本探测** | `<CDN>/StreamingAssets/yoo/DefaultPackage/PackageManifest_DefaultPackage.version` | 实测 **404**，**非致命**（YooAsset 随后从根目录取到版本） |

> ⚠️ **上传后必须按客户端实际请求的 URL 逐个自检，不能推断**。历史上同类路径错配被踩过两次（先少一层、后多一层）。

### 3.2 怎么知道要传哪几个文件（实测方法，别猜）

1. 出包前落一份导出物哈希快照（现成产物：`Logs/webgl-port/_snapshot-before-planA.txt`、`_snapshot-after-A4.txt`）
2. 出包后重新计算 `webgl\` 与 `minigame\` 下所有文件的 SHA256(前 16 位) 并比对
3. 得出「新增 / 变化 / 消失」三张清单，**只传变化的**

**两条实测规律**：
- 改 **UI 资源**（不动 C#）：YooAsset bundle 与 manifest 会变 → 传 YooAsset 增量 + 首包 `data.br`
- 改 **C#**（含 `Init.cs`）：`Assembly-CSharp` 是 AOT 主程序集（`Assets/Client` 无 asmdef）⇒ **`webgl.wasm` 与 `data` 都会变**，首包两个文件都要重传；YooAsset 资源通常**未变**（本会话两次实测：150 个文件里 149 个逐字节相同，仅本地 `BuildReport_*.json` 变）

### 3.3 上传后自检（必须全部 200）

```powershell
$base='https://<CDN域名>/1/HotUpdate/NewAB/WebGL/pinball-client'
foreach ($k in @('<game.js 的 DATA_FILE_MD5>.webgl.data.unityweb.bin.br',
                 '<game.js 的 CODE_FILE_MD5>.webgl.wasm.code.unityweb.wasm.br',
                 'PackageManifest_DefaultPackage.version')) {
  try { (Invoke-WebRequest "$base/$k" -Method Head -TimeoutSec 25).StatusCode.ToString() + "  $k" }
  catch { $_.Exception.Response.StatusCode.value__ ; $k }
}
```

> 上传需要 OSS 凭据（代理没有），由负责人执行。清单模板见 `Logs/webgl-port/OSS_UPLOAD_MANIFEST*.txt`。

---

## 4. 开发者工具（容器）运行与取证

1. 打开项目目录 `…\720\minigame`（**必须是本次导出目录**：首包文件名随构建变化）
2. 详情 → 本地设置 → 勾「不校验合法域名…」（`project.config.json` 的 `urlCheck: false`，**只在模拟器内生效**）
3. 编译 ▾ → 清缓存 → **全部清除** → 重新编译
4. Console 判据：

```
首个场景：ClientShell（ClientStartScene=ClientShell）
首个场景加载方式判定：构建清单场景数=2，目标=ClientShell
首个场景已加载（sceneLoaded 事件）：ClientShell，模式=Single        ← A4 跨场景宿主生效
WebGL 渲染兼容性已注册：已检查 N 台相机                             ← 加载后兼容处理执行
[WebGL 首屏诊断 1s] / [10s] scene=…, cameras=…, canvases=…          ← 首屏诊断在跑
全文件 0 处：Failed to mapping location to asset path / Failed to load scene
[配置表] … → ActiveVisualCanvases 含 ClientCanvas/PagesLayer/MainPage主页
[TCP客户端] 网关成功 → TCP 建连成功 → 握手成功 → 游客登录成功 → 英雄数据解析成功 → 成功退出
```

⚠️ **不要**用 `miniprogram-automator` 读小游戏 Console：已验证为死路（`cli auto` 能握手，但 `evaluate`/`systemInfo`/`currentPage` 全超时，小游戏无 `getConsoleMessages`）。

---

## 5. 真机验证

**完整步骤与判据见 `WEBGL_B2_REALDEVICE.md`**；三条硬前提速查：

1. **手机与 `<服务器地址>` 同一局域网**（网关发现服务就是这个裸内网 IP；实测手机浏览器能拿到 `<error_response><code>2</code><msg>请求无效</msg></error_response>` 即为通）
2. **手机开启「调试」** —— 2026-09-22 真机 A/B 实测：不开调试被 `downloadFile:fail url not in domain list` 拦；**开调试后放行**（裸 IP 的发现服务与 TCP 也一并放行，六包走通）
3. **首包较大**：data 约 36.5 MB（从 OSS 拉）+ wasm 约 15.3 MB（随包下发）⇒ 首次进入慢，别中途退出

**真机实测结果（2026-09-22，Android 1080×2400）**：首页正常渲染、首页→编队页→新手任务页可跳转、
**TCP 六包齐全**（英雄=4，英雄组=1）、`RT-FPS 50~63 / Jank 0 / BigJank 0 / Stutter 0.00%`、无致命异常。

### 5.1 为什么"直接扫"不行、必须开手机端「调试」

| 层 | 开关 | 作用范围 |
|---|---|---|
| 开发者工具 | 详情 → 本地设置 →「不校验合法域名…」（= `project.config.json` 的 `urlCheck: false`） | **只对模拟器生效**；真机**不读**这个文件 |
| **真机** | 手机右上角「…」→「**调试**」 | 进入**开发调试模式**，客户端**跳过域名白名单校验** —— 真机上唯一的跳过开关 |

- **已取证（A/B 对照）**：同一构建、同一网络，唯一变量 = 调试开关。
  不开 ⇒ `downloadFile:fail url not in domain list`；开 ⇒ 资源下载通过、进首页、**TCP 六包走通**
  ⇒ 跳过机制覆盖 `downloadFile` / `request` / `socket`，**不只是"下载"**，所以裸 IP 的发现服务（`:8000`）与 TCP 网关（`:53413`）也一并放行。
- **机制说明（与实测一致，官方条文原文未取到）**：正式版强制校验 `request` / `socket` / `downloadFile` 合法域名；
  开发版/体验版 + 手机开「调试」可跳过（开发者测试便利）；非法域名 / IP / 非 HTTPS ⇒ `url not in domain list`。
  官方入口：<https://developers.weixin.qq.com/minigame/dev/guide/base-ability/network.html>
  （正文为 JS 渲染，本次未能取到条文原文，故机制描述**以实测行为为准**）。
- **⚠️ 两个「调试」别混淆**：开发者工具里的「不校验合法域名」只影响模拟器；手机上的「打开调试」才影响真机（顺带出现 vConsole）。
- **生产含义**：正式版**没有**这个开关 ⇒ ① OSS 域名加 `downloadFile` / `request` 合法域名（HTTPS + 已备案）；
  ② 网关是裸 IP、**无法加白** ⇒ 需服务端提供 HTTPS 域名（台账见 `BUG_TRACKER.md` `RISK-021`）。

### 5.2 真机的几种进入方式（扫码不是唯一）

| 方式 | 需要扫码？ | 需要配合法域名？ | 备注 |
|---|---|---|---|
| **预览**（开发版） | 首次要 | 否（靠手机端「调试」跳过校验） | 最常用 |
| **自动预览**（`auto-preview`） | **不需要** | 否 | 开发者工具编译后**自动推送到已关联的微信号**；需手机微信与工具登录同一账号。CLI 命令名为 `auto-preview`（[微信小游戏开发文档](https://developers.weixin.qq.com/minigame/en/dev/)），**工具内的确切开关位置本次未取证** |
| **再次进入**（免扫码） | 不需要 | 否 | 打开过一次后，微信「**发现 → 小游戏 → 最近使用**」即可直接进；或转发卡片后从聊天点开 |
| **体验版** | 首次要（或从分享卡片进） | ✅ 需要 | 公众平台 → 版本管理 → 选为体验版 → 加体验成员；最接近线上 |
| **真机调试 2.0** | 首次要 | 否 | 日志回传 IDE，取证最方便 |

> **「调试」开关要不要每次都点？** 该开关按「设备 + 该小游戏」记忆，通常**开一次即可**——
> 但**本次未取证**，建议自测一次：退出小游戏再从「最近使用」重进，看 vConsole 是否仍在。
>
> **有没有"完全不用调试"的路？** 只有配合法域名这一条：OSS 域名可加白，但**网关是裸 IP 加不进去**
> ⇒ 目前"不用调试 + TCP 能通"不成立，需要服务端提供 HTTPS 域名。

**生产化前置（未完成，需服务端配合）**：正式版/体验版**没有调试开关**，必须配置合法域名；
而发现服务与 TCP 网关都是**裸 IP + HTTP，无法加入合法域名白名单** ⇒ 正式环境需服务端提供 **HTTPS + 已备案域名**。
OSS 域名（`<CDN域名>`）可加入 `downloadFile` / `request` 合法域名。

---

## 6. 日志位置与排查

### 6.1 日志位置一览

| 类型 | 位置 |
|---|---|
| 出包（阶段 A/B） | `Logs/b1-phaseA-*.log`、`Logs/b1-phaseB-*.log` |
| 编译校验 | `Logs/verify-compile.ps1` → 产物与日志在 `Logs/_verify/` |
| 启动链校验 | 阶段 B 日志内 `[启动链校验]` 段 |
| 容器运行日志 | 微信开发者工具 Console（可导出） |
| 真机运行日志 | vConsole（开调试后）；或「真机调试 2.0」回传 IDE；体验版可用公众平台「实时日志」 |
| 导出物哈希快照 | `Logs/webgl-port/_snapshot-*.txt` |

### 6.2 失败排查（现象 → 判据 → 动作）

| 现象 | 判据 | 动作 |
|---|---|---|
| Console：`Failed to mapping location to asset path : X` / `Failed to load scene !` | 目标场景不在 YooAsset 寻址清单 | 若场景已进 Build Settings ⇒ 走原生加载（`Init.LoadFirstSceneAsync`）；否则补进构建清单或收集器 |
| 首页不显示（原阻塞） | 见上 | **已修复**：`SceneManager.LoadSceneAsync` 原生加载，见 `HOME_PAGE_SCENE_FIX_OPTIONS.md` §九 |
| 真机：`资源下载失败 / downloadFile:fail url not in domain list` | 域名校验拦下载 | ① 手机开「调试」（实测有效）；② 配合法域名（OSS 域名） |
| 真机：`request:fail url not in domain list`（预期） | 发现服务/网关是裸 IP，无法加白 | 需服务端提供 HTTPS 域名（**服务端范围**） |
| TCP 停在 `ResolvingGateway` | 手机连不到 `<服务器地址>:8000` | 检查是否同一局域网、是否走了移动数据、AP 隔离 |
| TCP 停在 `Connecting` | 手机连不到 `<服务器地址>:53413` | 同上；另确认服务端未按源 IP 白名单限制 |
| 首包 404 / 卡 Loading | 上传路径或文件名不匹配 | 核对 `game.js` 的 `DATA_FILE_MD5`/`CODE_FILE_MD5` 与 OSS 根目录实际文件名 |
| 构建报 `script class layout is incompatible between the editor and the player` | 编辑器/播放器字段布局不一致（典型：`[SerializeField]` 被 `#if` 包住） | 字段声明移到 `#if` 外，只把逻辑放里面 |
| 批处理 Unity 崩溃退出码 `0x40000015` | 编辑器已开着（`HandleProjectAlreadyOpenInAnotherInstance`） | 先关闭 Unity 编辑器 |
| 改完脚本构建仍用旧程序集 | 域重载未跨进程 | 必须走 `run-pipeline-2phase.ps1` 的两阶段（不要在一个 executeMethod 里连做） |

---

## 7. 坑清单（都是本会话/前任会话烧时间换来的）

1. **`[SerializeField]` 不得放在 `#if UNITY_WEBGL` 内** —— 否则编辑器与播放器字段集不一致，Unity 直接拒绝构建。
2. **YooAsset 资源放 CDN【根目录】**，不是 `/Assets`（`/Assets` 那层是微信 SDK 纹理/音频用的）。
3. **不要用 `miniprogram-automator` 读小游戏 Console** —— 死路，已删产物不要重建。
4. **改 CDN 目录必须同时改两处**：`MiniGameConfig.asset` 的 `CDN` 与 `Boot.unity` 的 `DefaultHostServer`/`FallbackHostServer`，且**必须重跑转换**（`game.js` 里写死 `DATA_CDN`）。
5. **只调 UI 时通常只重传 `data.br`**；**改了 C# 则 `wasm.br` 也变**（`Assembly-CSharp` 是 AOT 主程序集）。
6. **`edit` 类工具会剥掉 UTF-8 BOM** —— 改 `Init.cs` 这类带 BOM 的文件后要复查补回。
7. **本机一律用 `pwsh`（PowerShell 7）**：5.1 会把无 BOM 的 UTF-8 脚本当 ANSI 读，含中文直接解析失败。
8. **`LoadSceneMode.Single` 会销毁 `Boot` 场景（含 `Init`）** —— 写在协程 `yield return` **之后**的代码永远不会执行；
   需要"加载后"的逻辑必须交给跨场景宿主（`WebGLPostSceneLoadRunner`，见 `Assets/Main/MODULE.md`）。
9. **`TCP_WEBGL_HANDOFF.md` 当前不在工作区**（git 追踪、工作区已删）：`git checkout -- TCP_WEBGL_HANDOFF.md` 恢复，或读副本 `D:\Users\Administrator\Desktop\notd\TCP_WEBGL_HANDOFF.md`。
10. **不要清理工作区未提交改动**（`Logs/` 下的脚本、日志、`*.unity.bak` 都是证据）。

---

## 8. 授权边界（改动前必须逐次确认）

以下文件属**负责人逐次授权**范围，未经确认不得修改：

- `Assets/Main/Init.cs`
- `Assets/Main/Boot.unity`
- `ProjectSettings/EditorBuildSettings.asset`（构建清单）
- `Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset`（CDN 等）

可自由改动（无需单独确认）：`Logs/` 下的脚本与文档、纯 Markdown 文档。
不得改：`Packages/manifest.json`、基础设施版本、项目设置、IP/端口/服务端/账号/token。
代理不操作 GUI（Unity Editor、开发者工具、公众平台），需要点界面的步骤由负责人执行。

---

## 9. 未完成 / 待确认（截至 2026-09-22）

| # | 项 | 状态 |
|---|---|---|
| 1 | 真机 B2 矩阵深度项：安全区与遮挡、返回栈压测、弱网/断网、后台切回 | ⬜ 未测 |
| 2 | **内存峰值采集** | ❌ 未采集（现有面板仅 FPS/Jank/Stutter） |
| 3 | 正式版/体验版合法域名 + 服务端 HTTPS 域名改造 | ⬜ 需服务端/平台侧决定 |
| 4 | `WEBGL_B2_REALDEVICE.md` 中 `[TCP客户端] 网关成功` 原行截图 | ⬜ 可选补齐（结论不受影响） |
| 5 | 开发者工具 Console 内 `1 error` 的具体内容 | ⬜ 未归位（怀疑为已知非致命 404，**未取证**） |
| 6 | 旧街机残留组件 `UnityBridgeManager` missing script | ⬜ 待核验（与首页无关） |
