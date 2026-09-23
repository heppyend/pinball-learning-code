# Client Editor 模块说明

## 职责

提供仅在 Unity Editor 中运行的客户端场景辅助工具。当前入口 `ClientShellUiBuilder` 会在 `ClientShell` 中创建可见、可编辑的 Canvas、页面节点、按钮和示例弹窗；运行时不依赖该工具。

## 入口与依赖

- 不再暴露 `Client` 顶部菜单；场景写入仅由命令行 `ClientShellDirectUpdater` 执行，避免误触覆盖人工调整。
- `ClientLoadingPageEditor.cs`：为 `SystemLayer/加载Page` Inspector 提供仅 Play 模式可用的“播放 5 秒加载演示”按钮，不进入正式 Player 运行链。
- 仅允许作用于 `Assets/Client/Scenes/ClientShell.unity`。
- 依赖 `Runtime` 的页面绑定组件，以及 `UI/MainPage/HomeReference.png`、`UI/ProfilePage/ProfileReference.png` 两张临时构图参考图。

## 禁止事项

- 不修改旧街机场景、Build Settings、资源加载、热更新或项目设置。
- 不自动覆盖已有的 `ClientCanvas`；需要重建时由负责人手动删除该根节点并保存场景后再执行菜单。

## 验证

在 Unity 中打开 `ClientShell` 后执行菜单，确认 Hierarchy 出现 `ClientCanvas`、`MainPage`、`ProfilePage` 与 `EventSystem`；Play 后验证主页进入个人中心、返回及改名。

## 待确认

实际字体、图集拆分、最终预制体粒度、适配基线与 YooAsset 分包策略尚未确定。
