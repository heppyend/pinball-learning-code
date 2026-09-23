# Client BackpackPage 模块说明

## 职责

背包页面的场景节点和后续可替换视觉资源。当前版本展示本地库存，支持全部、材料、装备筛选、条目选中提示与返回主页。

## 依赖与限制

- 数据仅来自 `IClientDataService.GetInventory()`。
- 页面运行时控制器为 `Assets/Client/Runtime/ClientBackpackPage.cs`，导航由 `ClientPageNavigator` 管理。
- 本阶段没有负责人提供的背包专用切图，因此仅复用已导入的主页主题背景；所有内容节点均在 `ClientShell` 中可编辑。
- 不实现出售、合成、装备、锁定或服务端同步；规则确认前只展示状态。

## 验证

打开 `ClientShell`，点击底部“背包”，在全部/材料/装备间切换，点击条目，再点击返回主页。

## 待确认

背包专用美术、道具名称和图标、装备属性、出售/合成/锁定规则。
