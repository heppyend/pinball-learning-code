# Client Scenes 模块说明

## 职责

存放本次非战斗客户端独立开发场景。当前 `ClientShell.unity` 只提供隔离入口，不替换或引用原街机战斗场景。

## 约束与验证

- 不将本目录场景加入既有 Build Settings，除非负责人明确确认启动整合方案。
- 场景中的 UI 和资源只经客户端模块接入；移动资源时保留 `.meta` 与 GUID。
- 首次打开空场景后，从菜单执行 `Client/Build ClientShell Visual UI`。它创建 `ClientCanvas`、`MainPage` 和 `ProfilePage` 作为可编辑层级，且不会覆盖已有 `ClientCanvas`。
- 在 Unity 2022.3.57f1c2 中打开 `ClientShell.unity` 并运行，确认无旧场景跳转。
