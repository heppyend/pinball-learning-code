# Main 模块说明

## 职责

项目启动与资源初始化模块。`Boot.unity` 是当前 Build Settings 唯一启用场景，`Init.cs` 负责 YooAsset 初始化、资源包运行模式、HybridCLR 元数据/热更新程序集加载，并请求加载 `LoginScene`。

## 主要内容

- `Boot.unity`：应用启动场景。
- `Init.cs`：启动编排与资源包初始化入口；包含仅在 WebGL Player 生效、可回收的首屏相机/Canvas/场景日志。现有 HDR/后处理兼容分支尚未证明是首屏问题的修复，必须以开发者工具日志继续核验。
- `Yooasset`：项目使用的 YooAsset 运行时和编辑器支持代码。
- `ResHotUpdate`：热更新相关资源输入；修改前需确认分包与地址。

## 约束与验证

- 不改变默认包名、资源地址、运行模式或 DLL 名称，除非同步完成 YooAsset/HybridCLR 构建验证。
- 修改启动逻辑至少验证 Editor Simulate Mode；涉及非编辑器分支时还应验证目标平台构建与启动。
