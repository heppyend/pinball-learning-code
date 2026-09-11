# HybridCLR 编辑器辅助模块

## 职责

此目录存放项目自有的 HybridCLR 编辑器辅助命令。它们只能在 Unity Editor 中运行，用于生成或同步构建产物，不能在运行时被引用。

## 主要入口

- `WebGLHotUpdateDllCommand.cs`：显式编译 WebGL 的热更新程序集，并将 `HotFix.dll` 同步到现有的 `Assets/HotUpdateResources/Dll/WebGL/HotFix.dll.bytes` 输入位置。

## 依赖与边界

- 依赖现有 HybridCLR `CompileDllCommand` 与 `SettingsUtil`；不修改 HybridCLR 配置、包或生成规则。
- 只处理 WebGL 的 `HotFix.dll`，不上传 CDN、不构建 YooAsset、不修改其他平台产物。
- 同步后仍须由负责人按现有流程重建 YooAsset 包，并在获得发布授权后发布匹配的包版本。

## 验证

- Unity Console 应显示源和目标路径及文件大小。
- 对应的 `.bytes` 修改时间与 WebGL `HotFix.dll` 一致后，才能进行资源包构建。

## 待确认

- CDN 发布及小游戏转换由负责人手动执行；本模块不处理这些外部状态变更。
