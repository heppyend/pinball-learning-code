# 主开发1交接

## 开始方式

新会话进入项目根目录后，必须先按既定顺序阅读：`AGENTS.md`、`DEVELOPMENT_WORKFLOW.md`、`PROJECT_OVERVIEW.md`、`VERSION_HISTORY.md`、`BUG_TRACKER.md`，再根据本次目标读取目标目录的 `MODULE.md`。本文件仅补充当前会话的交接事实，不替代上述约束。

## 当前项目状态

- Unity 版本：`2022.3.57f1c2`；基础设施为 URP、YooAsset、HybridCLR、XLua，未经单独评估不得替换或升级。
- 阶段状态：阶段 0“项目勘察与技术定标”进行中；阶段 1 至 8 尚未开始。下一步应继续完成阶段 0 的架构、交互稿、模拟数据边界与目标平台决策盘点，不能直接将未确认的业务规则实现为既定功能。
- 已建立根目录治理文档、模块 `MODULE.md` 与面试项目介绍：`Project Introduction/Project Introduction.md`。
- 负责人已在 Unity 中人工确认：Console 红色 Error 为 0、已退出 Safe Mode、工程能正常打开并完成编译。

## 本轮已处理事项

- 因 Git 镜像地址返回 HTTP 403，负责人将 `Packages/manifest.json` 的 AssetBundle Browser 来源改回官方 GitHub 地址；相应 lock 文件已变化。
- 因小程序端不使用 Tiled 怪物生成，已按负责人明确授权移除 `Assets/Scripts/BNRoom/SinglePlayer/BNMonsterSpawnManager.cs` 及其 `.meta`，并移除 `Assets/HybridCLRGenerate/link.xml` 中的 SuperTiled2Unity 链接条目。
- 静态扫描已确认 C#、asmdef 和 HybridCLR 链接配置不再包含 `SuperTiled2Unity`、`SuperObject` 或 `SuperCustomProperties` 编译期引用。

## 未关闭风险与待确认项

- `RISK-005`：`Assets/HotUpdateResources/Scene/FishScene.unity` 可能仍含已移除的 `BNMonsterSpawnManager` Missing Script。当前不加载该历史场景；只有负责人确认其不进入小程序资源包，或在 Unity 中清理组件并回传验证，才可关闭。
- `RISK-001` 至 `RISK-004` 仍开放，分别涉及 YooAsset/发布目录保留策略、历史场景/示例引用、启动代码遗留 Helper，以及可再生 IDE 产物的人工清理。详情以 `BUG_TRACKER.md` 为准。
- 小程序容器、目标设备与适配基线、服务端/账号/支付、数值与玩法、美术音频及第三方能力均未确认。

## 工作区保护

- 工作区存在大量未提交改动，尤其是 `Assets/Resources_HotUpdate/BNRes/Spines` 下的 Unity 自动更新 `.meta`、HybridCLR 子模块状态、Android XLua `.meta` 删除、Packages 改动及本轮文档。除负责人明确授权外，不得重置、清理、覆盖或批量暂存这些改动。
- 当前 Git 最近提交：`f738611d chore: configure HybridCLR submodules`、`9b7128e7 chore: configure Git LFS`、`27ee4728 chore: initialize pinball project`。

## 人机边界

- 代理只处理代码、配置、Markdown 与命令行验证；Unity Editor、IDE、构建窗口、平台后台等 GUI 均由负责人手动操作。
- 需要 GUI 验证时，必须提供前置条件、逐步操作、预期结果、失败排查与回传内容；未回传不能标为已通过。

## 建议的首条新会话指令

`继续阶段 0：先盘点交互稿位置与已明确页面状态，再提出客户端基础层的本地模拟数据与导航边界方案；不要修改代码，先输出计划和待确认项。`
