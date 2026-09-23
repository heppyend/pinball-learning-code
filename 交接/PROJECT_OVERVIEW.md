# 项目概况与模块地图

## 项目定位

本仓库是 Unity `2022.3.57f1c2` 弹珠项目客户端。当前产品依据是小程序端交互稿；现阶段优先建立可替换的本地模拟交互闭环，线上账号、服务端、支付、小程序容器与最终数值仍待确认。

## 阅读顺序

本工作区内的每次聊天与操作均受 `AGENTS.md` 约束，并按其中的任务路由读取最小上下文。本文件只在项目勘察、跨模块设计或需要核对稳定技术事实与模块边界时读取；继续任务先读 `CURRENT_STATE.md`，进入自有模块前再读对应 `MODULE.md`。

## 人机操作边界

人机权限与禁止事项的完整规则以 `AGENTS.md` 为唯一权威；GUI 验证的操作步骤、预期结果、失败排查和回传格式以 `DEVELOPMENT_WORKFLOW.md` 为准。本文件只保留项目结构相关的稳定事实。

## 当前模块

| 路径 | 职责 | 状态 |
| --- | --- | --- |
| `Assets/Main` | 启动场景、YooAsset 初始化、热更新启动入口、**首个场景加载策略**与跨场景宿主（`WebGLPostSceneLoadRunner`）与调试控制台。 | **链路已实测**：`Boot → ClientShell` 在容器与真机均进首页；见 `Assets/Main/MODULE.md` |
| `Assets/Scripts` | 项目业务、通用能力、网络、表格、热更新与编辑器脚本。 | 已存在，待勘察 |
| `Assets/Client` | 本次新建的非战斗客户端模块：独立开发场景、场景可编辑 UI、养成/背包/商城等领域逻辑、本地模拟数据、`Stats` 数值与说明模块、以及初始化 UI 层级/自检的 Editor 工具。 | **已表驱动**：10 张客户端表在 Play 中可见；ProfilePage/Activity 收口完成；自检 70 项通过（详见 `CURRENT_STATE.md` 与 `Assets/Client/MODULE.md`） |
| `Assets/Scripts/Platform/WeChatMiniProgram` | 微信小游戏平台能力契约与默认回退。 | 已建立，待 SDK 适配器接入 |
| `Assets/Editor/HybridCLR` | 项目自有 HybridCLR 编辑器辅助命令；当前用于 WebGL HotFix 编译与同步。 | **已纳入一键出包管线**（阶段 A 自动编译并同步 `HotFix.dll.bytes`） |
| `Assets/WX-WASM-SDK-V2` | 已导入的微信小游戏 WASM 转换 SDK；包含 Editor 转换工具、运行时桥接和模板资源。 | 第三方目录，受控维护 |
| `Assets/WebGLTemplates/WXTemplate*` | 微信小游戏导出用 WebGL 模板。 | 第三方模板，受控维护 |
| `Assets/Scenes` | 启动后加载的游戏与测试场景、场景特效资源。 | 已存在，待勘察 |
| `Assets/Resources` | 通过 Unity Resources 机制直接加载的基础资源。 | 已存在，受控维护 |
| `Assets/Resources_HotUpdate` | 通过热更新资源流程提供的 UI、字体、材质与加载资源。 | 已存在，受控维护 |
| `Assets/HotUpdateResources` | 热更新 DLL、预制体和场景的构建输入。 | 已存在，受控维护 |
| `StreamingAssets`、`StreamingAssetsPublish` | 内置/发布资源包输出，受 YooAsset 发布链路影响。 | 待确认发布策略 |
| `Bundles`、`AssetBundles` | 已生成或导出的资源包；保留至完成引用与发布策略核验。 | 待核验 |
| `docs` | 现有项目资料与交互稿相关文件。 | 已存在 |
| `UI素材` | 原始 UI 参考素材；不是最终可直接删除的运行时资源。 | 待美术确认 |
| `Notes` | 本对话生成的项目学习笔记与知识整理；不包含功能实现或运行时配置。 | 已建立 |

## 启动与资源链路（已核验）

构建设置启用 **`Boot(0) → ClientShell(1)`**（2026-09-22 起）。`Assets/Main/Boot.unity` 由 `Assets/Main/Init.cs` 初始化 YooAsset、按运行模式加载资源包与热更新程序集，然后加载**首个游戏场景**：`Init.ClientStartScene` 指向 `ClientShell`（属构建清单内的内置场景 ⇒ 用 `SceneManager.LoadSceneAsync` 原生加载；留空则回落原行为 `LoginScene`，走 YooAsset 地址加载）。因此不得在未验证资源地址、包清单和场景引用前删除 `Assets/Main`、热更新资源、`StreamingAssets` 或发布资源包。完整移植手册见 `WEBGL_MINIGAME_HANDBOOK.md`。

> ⚠️ **2026-09-20 补充（保留原文，见 `BUG_TRACKER.md` BUG-025）**：
> 当时链路**只到 `LoginScene`** —— 客户端场景 `Assets/Client/Scenes/ClientShell.unity` **既不在构建设置里，也没有任何运行时代码加载它**
> ⇒ **按当时现状构建 WebGL / 微信小游戏，包内不含客户端场景，永远进不到客户端 UI。**
> 三个方案（A 把 `ClientShell` 加进构建设置并置首 / B 在 `Boot` 之后追加 `SceneManager.LoadScene("ClientShell")` /
> C 新建只做客户端初始化的 `ClientBoot` 场景并置首）当时**待负责人选定**。
>
> ✅ **2026-09-22 更新（已解决）**：最终落地为「`Boot(0) → ClientShell(1)` + `Init` 原生加载」（方案 A 的变体），
> `ClientBoot` 已退出启动链；容器与**真机**均进首页，真机 TCP 六包齐全
> （证据：构建日志 `[Builder] Scenes [1]: …/ClientShell.unity, [x]`；`HOME_PAGE_SCENE_FIX_OPTIONS.md` §九；`WEBGL_B2_REALDEVICE.md` §八）。

## 结构优化原则

1. 新业务按领域模块归入 `Assets/Scripts`，共享基础能力不得被页面模块复制。
2. 运行时代码、编辑器代码和第三方代码保持隔离；不移动第三方与热更新基础设施来追求表面整洁。
3. 目录迁移必须保持 Unity `.meta` 文件、场景/预制体引用和 YooAsset 地址一致，并有回滚路径。
4. 清理优先处理可再生 IDE 产物与日志；已纳入版本控制的资源包、构建物及不明资源先核验后处理。
5. 新客户端不得依赖或改造既有街机战斗场景；需要接入既有启动、资源或导航能力时，先以适配器边界隔离。

## 待确认

- `Bundles`、`AssetBundles`、`StreamingAssetsPublish` 是否为当前发布所需产物及其再生成命令。
- `Assets/Scenes` 中的历史测试/示例场景、第三方示例目录及未使用资源是否仍需保留。
- 面向小程序的目标平台、性能预算和最终资源分包方案。
- 微信小游戏转换 SDK、AppID、域名/隐私合规、登录换票协议和开发者工具/真机测试权限。
