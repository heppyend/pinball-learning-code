# 弹珠项目客户端源码学习版

这里保存 Unity 客户端源码、关键工程配置和交接资料，供接手开发与学习。当前工程的新增非战斗客户端在 `Assets/Client`；旧游戏的启动、登录和网络代码保留作兼容依赖。

**先读 [交接资料首页](./交接/README_从这里开始.md)**。其中 [给人看的接手说明](./交接/00_给人看/00_接手说明.md) 介绍 TCP 协议链路与各 Client 模块；[微信小游戏资源导出与上传](./交接/00_给人看/01_微信小游戏资源导出与上传.md) 说明 YooAsset 包和两个 `.br` 的区别、改动后的上传范围以及 Unity 手动导出步骤。

## 当前代码的边界

- 构建启动链为 `Boot → ClientShell`。场景和相关实现分别在 `Assets/Main`、`Assets/Client/Scenes` 与 `Assets/Client/Runtime`。
- Client 页面现阶段通过配置表和本地模拟服务展示数据；小游戏 TCP 探针已能完成网关发现、握手、游客登录及英雄回包验证，但没有把回包接到新 Client 页面。
- 本次只加入 `ClientShell` 场景及其 Prefab 通过 GUID 实际引用的演示 UI 素材，并连带所需的字体、控制器等依赖；未使用的 UI 素材没有复制。
- 这是**学习和交接仓库**，不是完整 Unity 工程镜像。旧游戏的大量美术与构建输出没有放入仓库，直接打开场景仍可能出现旧资源缺失。生成的 `Bundles/`、WebGL `.br`、缓存和运行日志也未提交。
- 仓库副本中的真实网关地址和交接材料中的环境地址已脱敏；接入实际环境需要由项目负责人另行配置。

## 微信小游戏导出源码

仓库现已补入原工程用于导出的微信小游戏 SDK（含转换器）、YooAsset、HybridCLR 生成文件、WebGL 插件，以及旧启动链编译所需的 Spine、DOTween、JSON 等代码依赖。构建入口仍是 `Boot → ClientShell`；命令行入口见 `Assets/Client/Editor/ClientWebGLBuildCommand.cs`。

在 Unity 2022.3.57f1c2 中，先完成 WebGL 热更程序集与 `defaultPackage` 资源包构建，再通过 **微信小游戏 → 转换小游戏** 导出。转换配置在 `Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset`：导出目录预设为仓库内的 `wx-export/`，AppID 与 CDN 是占位值，使用者须在 Unity 面板中换成自己的有效配置。把导出目录中的 `minigame/` 导入微信开发者工具；`webgl/` 是转换过程的 WebGL 产物。

本仓库仍按“源码学习版”控制素材范围：保留 Client 演示实际引用的 UI 素材，没有补入原公司游戏的全部美术与资源包。同步依赖后按负责人要求**未再次编译或导出**；独立出包若提示缺少旧资源，应从有权使用的完整工程补齐相应资产。正式出包和 OSS 上传仍以完整工程及实际环境配置为准。
