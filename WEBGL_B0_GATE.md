# 微信小游戏 / WebGL 迁移「B0 门禁」记录

> 依据：`HANDOVER_PLAN.md` §5 **B0：迁移前门禁** —— 「在 UI 闭环可稳定运行前，不把 WebGL 构建失败当成 UI 问题修补。先确认并留档」。
> 建立日期：2026-09-20（第 66 轮）。**本文件只记录已取证的事实与结论；证据不足的一律标"待负责人"或"待采集"。**
> 适用：移植由**新会话**执行；本文件是那一轮的入口清单。

---

## 0. 结论速览

| 类别 | 结论 |
|---|---|
| 工具链版本 | ✅ **已留档**（下表） |
| 构建入口 | 🔴 **未通过 —— 硬阻断**：构建场景清单只有 `Assets/Main/Boot.unity`，`ClientShell` 不在其中，且**无任何代码加载它** ⇒ WebGL 包**进不去客户端 UI** |
| XLua WebGL 适配 | ✅ 适配物存在（`Assets/Plugins/WebGL/xlua_webgl.cpp`） |
| WebGL Player 设置 | ⚠️ **已留档但有两处风险**：引擎代码剥离=关（体积）、压缩=Disabled |
| 首包 / 分包 / 远端资源 | ⚠️ 待负责人确认（YooAsset 策略未在本文件定稿） |
| 真机环境 | ⏸ **待负责人提供**：微信开发者工具版本/路径、AppID、设备与微信版本、日志回传方式 |

---

## 1. 工具链版本与安装位置（已留档）

| 组件 | 版本 / 位置 | 证据 |
|---|---|---|
| Unity | **2022.3.57f1c2**，`C:\Program Files\Unity\Hub\Editor\2022.3.57f1c2\Editor\Unity.exe` | 本轮命令行编译均用此路径 |
| URP | `com.unity.render-pipelines.universal` **14.0.11** | `Packages/manifest.json` |
| TextMeshPro | `com.unity.textmeshpro` **3.0.7** | 同上 |
| HybridCLR | `com.code-philosophy.hybridclr`（**gitee git 依赖**，无版本号） | 同上；另有 `Assets/HybridCLRGenerate/{AOTGenericReferences.cs,link.xml}` |
| XLua | 工程内集成（非包），含 **WebGL 适配** `Assets/Plugins/WebGL/xlua_webgl.cpp`（1.5 KB） | 原生构建树在 `build/plugin_lua53/Plugins`、`build/plugin_luajit/Plugins` |
| 微信小游戏 SDK | `com.qq.weixin.minigame` **0.1.1**（`WXSDK`，"WeChat Mini Game **Tuanjie Engine** Adapter SDK Package"，`unity: 2019.4`） | `Assets/WX-WASM-SDK-V2/package.json` |
| WX SDK 转换入口（GUI） | `Assets/WX-WASM-SDK-V2/Editor/`：`WXEditorWindow.cs` / `WXConvertCore.cs` / `WXMultiPackageMergeWindow.cs` / `WXEditorSettingHelper.cs` | 目录列举 |

**待负责人提供**（代理不操作 GUI、不接触账号）：
- 微信开发者工具的**版本与安装路径**、是否已登录、有无上传/体验版权限；
- **AppID / 环境配置**的保管方式；
- 谁有**导出、上传、体验版与真机测试**权限。

---

## 2. 🔴 阻断项：构建入口（必须先定，否则 B1 无从谈起）

**取证（2026-09-20）**：
- `ProjectSettings/EditorBuildSettings.asset` **只有一个场景**：`Assets/Main/Boot.unity`（`enabled: 1`）；
- 全工程 `Assets/**/*.cs` 中**没有任何 `LoadScene`/`SceneManager` 加载 `ClientShell`**（只有编辑器工具引用其**路径常量**）；
- `Assets/Main/Boot.unity`（32.4 KB）中 `ClientServices` / `ClientShell` / `ClientBootstrap` / `ClientLoadingPage` 出现次数**均为 0**。

⇒ **结论**：按现状构建 WebGL，包内**不含客户端场景**，运行后停在原公司的 `Boot` 流程，**永远进不到客户端 UI**。

**三个可选方案（需负责人选一个；代理不擅自改构建清单或原流程）**：

| 方案 | 做法 | 影响 / 代价 |
|---|---|---|
| **A** | 把 `Assets/Client/Scenes/ClientShell.unity` **加入 Build Settings** 并作为**启动场景（index 0）** | 小游戏直接进客户端；但**改变现有构建入口**（`Boot` 不再先跑），需负责人确认这正是移植目标 |
| **B** | 保留 `Boot`，在其流程后**追加一次 `SceneManager.LoadScene("ClientShell")`** | 保留原启动链；但要**改原公司 Boot 流程**（`AGENTS.md` §1/§3 对改动既有流程有约束，需单独授权） |
| **C** | 新建一个只做客户端初始化的 **`ClientBoot` 场景**，加进清单并置首；`Boot` 完全不动 | 最符合"客户端新功能从 `Assets/Client` 独立场景开始"的约束；代价是**需要新建场景**（场景写入需授权 + GUI 操作） |

> 关联记录：`BUG_TRACKER.md` **BUG-025**。

---

## 3. WebGL 平台差异：本工程暴露面（已按代码/设置取证）

| 维度 | 本工程现状 | 风险 |
|---|---|---|
| **线程** | Player 设置 `webGLThreadsSupport: 0`（关） | ✅ 与微信小游戏一致 |
| **网络** | 小游戏路径已走 `ClientTcpConnectionProbe` 的 `WXBase.CreateTCPSocket`（`#if UNITY_WEBGL && !UNITY_EDITOR`）；PC 路径 `ClientServerPcSession` 已整段排除（WebGL 留空壳） | ✅ 第 58 轮已处理 |
| **文件系统** | 客户端读表：`{persistentDataPath}/config.pkg` 优先、回落 `Resources/Table/*.json` | ⚠️ **纳入首次真机清单**（见 §6-①） |
| **压缩** | 客户端 gzip 已改用工程同款 `Unity.IO.Compression`（与 `TableLoadHelper` 同源） | ⚠️ **纳入首次真机清单**（见 §6-①）；同时 Player `webGLCompressionFormat: 2`（**Disabled**） |
| **内存** | `webGLMemorySize: 256`（MB）、`webGLInitialMemorySize: 32` | ⚠️ 首包/运行峰值需实测（见 §6-③） |
| **纹理 / Shader** | URP 14.0.11；`BUG-009`（首屏黑屏 / URP shader）**调查中** | 🔴 **B2 首要取证项**；按纪律**先取日志证据，不猜根因** |
| **字体** | TMP 3.0.7 + `Assets/Resources_HotUpdate/Font/...` | ⚠️ 小游戏字体包体/动态字体待真机确认 |
| **输入** | `ENABLE_LEGACY_INPUT_MANAGER` 分支存在（`ClientUiClickTracer`） | ⚠️ 触摸路径待真机确认 |
| **URL / 资源加载** | 走既有 YooAsset；客户端目前通过 `Resources/Table/*.json`（**随包**，实测 174 KB） | ⚠️ 见 §6-③ |

---

## 4. 兼容矩阵（YooAsset / HybridCLR / XLua）

| 组件 | 现状 | 待确认 |
|---|---|---|
| **XLua** | WebGL 适配物**存在**（`xlua_webgl.cpp`）；原生构建树在 `build/plugin_lua53` / `build/plugin_luajit` | ⚠️ 该适配是否**已在当前 Unity 版本成功构建过**、以及**客户端是否根本不需要 Lua**（若客户端不依赖 XLua，最佳做法是**不在小游戏包里带上它**——需负责人确认） |
| **HybridCLR** | 包来自 gitee git（无版本号）+ `Assets/HybridCLRGenerate/`（AOT 泛型引用、link.xml） | ⚠️ 当前提交对应的兼容版本、以及**热更 dll 的加载路径**在小游戏下的可用性 |
| **YooAsset** | ✅ **已确证：`com.tuyoogame.yooasset` 2.1.1**，位置 `Assets/Main/Yooasset`（工程内本地包） | 版本与位置已定；构建参数与分包策略仍待定（§5） |

> 说明（第 67 轮更新）：`Packages/manifest.json` 中**没有** `yooasset` 条目（它以工程内本地包形式存在），
> 第 66 轮曾把它标为"待采集"；本轮已用 `Assets/Main/Yooasset/package.json` 确证版本为 **2.1.1**。

---

## 5. 分包 / 首包 / 远端资源（待负责人确认）

- 客户端**随包**内容：`Assets/Resources/Table/*.json`（10 张表，实测 **174 KB**）；
- 远端：`{persistentDataPath}/config.pkg`（**格式已验证**：`GZip → [version:1B] → 重复 [utf8\0][utf8\0]`，见 §6-①）；
- **需负责人定**：首包预算、分包策略、YooAsset 地址与回收生命周期、远端包由谁发布。

---

## 6. ★ 首次真机清单：三个 WebGL 验证点（本轮新代码引入，必须真机确认）

| # | 验证点 | 为什么 | 怎么验 |
|---|---|---|---|
| **①** | **表读取链**：`persistentDataPath/config.pkg`（`Unity.IO.Compression` 解压）与回落 `Resources/Table` | gzip 实现第 58 轮刚从 `System.IO.Compression` 换成 `Unity.IO.Compression`，**小游戏下未跑过**；且 `persistentDataPath` 在小游戏里是 Emscripten 虚拟 FS | ① 只放本地表启动 → 应打印 `客户端读表路径：本地 Resources/Table` + `Hero = 54 行`；② 再放一个真实 `config.pkg` → 应打印 `远端落盘包 config.pkg（N 张表）`。**桌面侧已用"变异 pkg"验证过同一条链**（`Logs/pkg-verify3.log`），真机只需确认平台行为 |
| **②** | **`Span` / `stackalloc` 使用**（`ClientDescriptionFormatter`） | IL2CPP 一般支持，但小游戏工具链版本差异需实测 | 打开英雄详情 → 三个页签说明文本应正常显示（`8%→9%→80%` 那类分档替换生效，不报错） |
| **③** | **`Resources/Table` 体积与首包** | 表随包 +174 KB；`stripEngineCode: 0`（剥离关闭）会放大包体 | 看转换工具报告的包体；确认首包是否超限，必要时改分包 |

---

## 7. 待负责人清单（B0 收口需要的输入）

1. **构建入口方案 A / B / C**（§2）—— **这是唯一硬阻断，其余都可在其后进行**；
2. 微信开发者工具版本/路径、AppID 与环境配置的保管方式、上传/体验版权限归属；
3. 真机设备清单、微信版本、网络环境、日志回传方式；
4. 首包预算与分包策略（§5）；
5. ~~YooAsset 的实际位置与版本（§4 待采集）~~ → ✅ **已在第 67 轮确证：`com.tuyoogame.yooasset` 2.1.1 @ `Assets/Main/Yooasset`**；
6. 客户端是否需要 XLua（若不需要，小游戏包可不带 Lua，见 §4）。

---

## 9. 配套文档（第 67 轮新增）

| 文档 | 内容 |
|---|---|
| `WEBGL_B1_BUILD.md` | **B1 可复现构建清单**：前置条件、8 步链路、逐步校验、失败排查表、证据归档命名 |
| `WEBGL_B2_DIAGNOSTICS.md` | **B2 证据采集清单**：可用日志通路（含 `WX.GetRealtimeLogManager`）、客户端日志标签判读、`BUG-009`/`BUG-013` 逐条采集项、三个验证点真机判据、回归矩阵、最小可回收诊断提案 |

---

## 8. 证据索引

| 主题 | 证据 |
|---|---|
| 构建场景清单 | `ProjectSettings/EditorBuildSettings.asset`（仅 `Assets/Main/Boot.unity`） |
| 无人加载 ClientShell | 全工程 `grep 'ClientShell'`：仅编辑器工具引用路径常量 |
| WebGL Player 设置 | `ProjectSettings/ProjectSettings.asset`（`webGLMemorySize: 256`、`webGLCompressionFormat: 2`、`webGLThreadsSupport: 0`、`stripEngineCode: 0`、`apiCompatibilityLevel: 6`、`allowUnsafeCode: 1`） |
| XLua WebGL 适配 | `Assets/Plugins/WebGL/xlua_webgl.cpp` |
| 远端表链已验证 | `Logs/pkg-verify3.log`（变异 pkg：`Hero = 3 行`）、`Logs/pkg-verify4-local.log`（回落：`Hero = 54 行`） |
| 平台条件编译 | `Assets/Client/Runtime/Services/ClientServerPcSession.cs`、`ClientTcpConnectionProbe.cs` |
| WX SDK | `Assets/WX-WASM-SDK-V2/package.json`（`com.qq.weixin.minigame` 0.1.1） |
