# Resources 模块说明

## 职责

存放通过 Unity `Resources` API 直接加载的基础运行时资源，包括 UI 预制体、Shader 及全局设置。

## 约束与验证

- `Resources` 中资源会被整体纳入构建；新增大资源前评估体积，优先使用现有 YooAsset 策略。
- 删除或改名必须先扫描 `Resources.Load` 调用、预制体/场景引用与热更新加载地址。
- 修改后验证所影响 UI 或初始化路径的资源加载。
