# 微信小游戏 / WebGL「B1 可复现构建」清单

> 依据：`HANDOVER_PLAN.md` §5 **B1：建立一个不包含发布/上传权限的本地构建说明和检查清单**。
> 建立：2026-09-20（第 67 轮）。配套：`WEBGL_B0_GATE.md`（门禁与版本）、`WEBGL_B2_DIAGNOSTICS.md`（真机证据采集）。
> **本文件不含上传、发布、真实账号、支付、密钥写入**；这些由负责人单独授权并手动执行。
> ⚠️ 代理不操作 GUI —— 表中标 **[GUI]** 的步骤由负责人执行，代理只准备前置与校验脚本。

---

## 0. 前置条件（已核）

| 项 | 值 | 证据 |
|---|---|---|
| Unity | **2022.3.57f1c2** | `C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe` |
| 微信小游戏 SDK | `com.qq.weixin.minigame` **0.1.1**（`Assets/WX-WASM-SDK-V2`） | `package.json` |
| YooAsset | **`com.tuyoogame.yooasset` 2.1.1**（`Assets/Main/Yooasset`） | `Assets/Main/Yooasset/package.json` |
| HybridCLR | gitee git 依赖 + `Assets/HybridCLRGenerate/`（`AOTGenericReferences.cs`、`link.xml`） | 目录 |
| XLua | 工程内集成，**含 WebGL 适配** `Assets/Plugins/WebGL/xlua_webgl.cpp` | 文件存在 |
| URP / TMP | 14.0.11 / 3.0.7 | `Packages/manifest.json` |
| WebGL Player | `compressionFormat = Disabled`、`debugSymbols = true`、`webGLThreadsSupport = 0`、内存 256MB、`stripEngineCode = 0` | `ProjectSettings.asset`；转换器亦强制 `Disabled`（`WXConvertCore.cs:56`） |
| 微信开发者工具 | **待负责人提供版本/路径与登录状态** | — |

> 🔴 **开工先决条件**：`BUG-025`（构建场景清单不含 `ClientShell`）必须先定下 A/B/C 方案，
> 否则构建出的包**进不到客户端 UI**，后续步骤全部无意义。见 `WEBGL_B0_GATE.md` §2。

---

## 1. 构建链路（目标形态）

```text
① 客户端侧验证（命令行，可自动化）
   └ Unity -batchmode -executeMethod ClientTableProbe.LogRowCounts
② HybridCLR：AOT 泛型/link.xml 生成 + WebGL 热更 dll 构建
   └ Editor\HybridCLR\WebGLHotUpdateDllCommand.cs（BuildTarget.WebGL）
③ YooAsset：构建资源包（Editor 菜单 [GUI] 或命令行）
④ Unity WebGL 构建（Build Settings → WebGL）
⑤ 微信小游戏转换 [GUI]：Assets/WX-WASM-SDK-V2/Editor/WXEditorWindow.cs
   └ 产物目录：webgl/ 与 minigame/
⑥ 微信开发者工具导入 minigame/ 并本地预览 [GUI]
⑦ 真机预览/体验版（由获授权人员执行）
⑧ 日志与问题证据归档（见 WEBGL_B2_DIAGNOSTICS.md）
```

---

## 2. 逐步操作与校验

### 步骤 ① 客户端侧验证（代理可跑，必做）

```powershell
$env:ALLUSERSPROFILE = 'C:\ProgramData'
& 'C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe' -batchmode -quit `
  -projectPath 'D:\unity project\pinball' `
  -executeMethod 'Pinball.Client.Editor.ClientTableProbe.LogRowCounts' `
  -logFile 'D:\unity project\pinball\Logs\b1-step1-probe.log'
```

**通过标准**：`error CS = 0`；`[配置表] 汇总：可用 10 张 / 空 0 张 / 缺失 0 张 / 解析失败 0 张`；
`[配置表] 客户端读表路径：本地 Resources/Table（未发现可用的 config.pkg）`。
（参考：`Logs/pkg-verify4-local.log`）

### 步骤 ② HybridCLR 生成（代理可跑）

- 产物位置：`Assets/HybridCLRGenerate/`（已存在 `AOTGenericReferences.cs`、`link.xml`）；
- WebGL 热更 dll 命令：`Assets/Editor/HybridCLR/WebGLHotUpdateDllCommand.cs`（`BuildTarget.WebGL`）；
- **校验**：命令退出码 0；`Logs/` 中留日志；确认热更 dll 输出目录与运行时加载路径一致（`WEBGL_B0_GATE.md` §4 待确认项）。

### 步骤 ③ YooAsset 资源包构建 [GUI]

- 入口：Unity 菜单 → YooAsset 的 AssetBundle Builder（`Assets/Main/Yooasset/Editor/AssetBundleBuilder`）；
- **构建参数由负责人确认**（包名/版本/构建模式/是否拷贝到 StreamingAssets）；
- **校验**：构建报告无 Error；产物目录存在；`TaskCopyBuildinFiles` 若启用则确认内置文件已复制；
- ⚠️ 首包预算与分包策略未定（`WEBGL_B0_GATE.md` §5）。

### 步骤 ④ Unity WebGL 构建 [GUI]

- Build Settings → **WebGL** → Build（**注意**：场景清单当前只有 `Assets/Main/Boot.unity`，见 `BUG-025`）；
- 关键设置（已核对，勿擅自改）：压缩 **Disabled**、`debugSymbols = true`、线程 **关**、内存 256MB；
- **校验**：构建报告无 Error；产物目录含 `index.html` / `*.wasm` / `*.data` / `*.framework.js`；
- 体积观察：`Resources/Table/*.json` 随包 **+174 KB**（实测）；`stripEngineCode = 0` 会放大包体。

### 步骤 ⑤ 微信小游戏转换 [GUI]

- 入口：菜单 WX → 转换（`WXEditorWindow.cs`）；核心在 `WXConvertCore.cs`；
- 关键常量（已核）：`webglDir = "webgl"`、`miniGameDir = "minigame"`、模板 `wechat-default`、
  框架文件名 `webgl.wasm.framework.unityweb.js`、worker 索引 `webgl.worker.js`；
- **默认图**：`Assets/WX-WASM-SDK-V2/Runtime/wechat-default/images/background.jpg`（首屏背景，见 `BUG-013`）；
- **校验**：转换窗口报告 Success；`minigame/` 下存在 `game.js` / `game.json` / `project.config.json` 等；
- ⚠️ **不手改生成工程**（`AGENTS.md`/`BUG-013` 已明确：不得手改生成工程或 `WAGame.js` 作为长期修复）。

### 步骤 ⑥ 开发者工具本地预览 [GUI]

- 导入 `minigame/` 目录（或用转换窗口的"打开开发者工具"）；
- **采日志前先清缓存**（`BUG-013` 的既有规避），首次加载**等 ≥30 秒**再导出 Console；
- 采集内容与判读见 `WEBGL_B2_DIAGNOSTICS.md`。

### 步骤 ⑦⑧ 真机与归档

- 真机由**获授权人员**执行；每次记录：设备型号 / 系统 / 微信版本 / **构建标识** / 复现步骤 / 截图或日志路径 / 结论 / 下一动作（`HANDOVER_PLAN.md` §5 B2 要求"未测试不能标记通过"）。

---

## 3. 失败排查表（先取证、再结论）

| 现象 | 先采集什么 | 不要做什么 |
|---|---|---|
| 开发者工具黑屏 | 完整 Console（≥30s）、是否出现 WebGL2 上下文 / Wasm 加载 / `资源包初始化成功` | **不要**凭现象断言是 Shader 或 URP 根因（`BUG-009` 纪律） |
| `app.json` 启动页报错 | 导出工程的 `game.json`/`app.json` 内容、转换日志 | 不要手改生成工程 |
| 找不到场景/停在 Boot | 构建日志里的场景清单 | 不要靠改 Boot 流程绕过（先走 `BUG-025` 决策） |
| 表读不到 | `[配置表]` 全部行 + `config.pkg` 是否存在 | 不要改 `ClientTableSource` 优先级 |
| Shader 不支持日志 | 具体缺失的 shader 名 + 是否影响可见画面 | 不要升级/调整 URP 包或渲染资源（`BUG-009` 已禁） |

---

## 4. 证据归档约定

| 类型 | 位置 | 命名 |
|---|---|---|
| 命令行编译/探针日志 | `Logs/` | `b1-step<N>-<用途>.log` |
| 转换与构建报告 | `Logs/` | `b1-build-webgl-<日期>.log` |
| 开发者工具 Console 导出 | `Logs/` | `b2-devtools-console-<日期>.txt` |
| 真机记录 | `Logs/` 或负责人指定位置 | `b2-device-<机型>-<日期>.md` |

> 所有证据**不得**含 AppID、token、订单、个人资料或真实服务端地址（`HANDOVER_PLAN.md` §5 B1）。
