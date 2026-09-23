# Client Runtime UI 模块说明

## 职责

`ClientShell` 的客户端 UI 基础架构：页面标识、页面生命周期、Canvas 分层和返回栈导航。仅管理 `Assets/Client` 的独立场景 UI，不接管旧 Boot、战斗、全局场景管理或服务端。

## 结构

```text
ClientCanvas
├─ PagesLayer     所有可导航页面根节点
├─ PopupLayer     后续弹窗根节点
└─ SystemLayer    后续全局提示、遮罩、加载根节点
```

`ClientUiPage` 提供 `Enter/Pause/Resume/Exit` 生命周期；`ClientUiNavigator` 维护页面注册表与返回栈。`SystemLayer/加载Page` 不注册为可导航页，由 `ClientLoadingPage` 通过显隐和进度接口独立控制。

## 禁止事项

- 不在此处调用旧项目场景管理、网络或战斗代码。
- 不运行时创建业务 UI；页面节点必须预先保存在 `ClientShell` 的 Canvas 分层中。
- 未确认的业务规则不得写入导航器。

## 验证

从主页进入任一一级页面、英雄详情或主页展示，再逐级返回；页面根节点应始终位于 `PagesLayer`。
