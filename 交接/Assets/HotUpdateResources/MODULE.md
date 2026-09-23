# HotUpdateResources 模块说明

## 职责

热更新 DLL、预制体和场景的构建输入目录；与 HybridCLR 和 YooAsset 的构建/加载链路关联。

## 约束与验证

- 不手工删除 DLL、场景或预制体产物；先确认生成来源及其在 YooAsset 包中的地址。
- 变更程序集、DLL 名称或资源目录后，必须重新生成热更新产物并验证启动加载。
