# Client 通用 UI 模块说明

## 职责

存放跨页面复用的 UI 预制体和其运行时视图组件。当前包含 `BottomFunctionIcon`：用于主页底部的扫码、客服、商城、排行和邮箱入口。

## 预制体结构

`BottomFunctionIcon` 根节点同时持有 `Image` 和 `Button`，它的 RectTransform 是唯一点击判定范围；子节点依次为 `IconFrame`、`Icon`、`Label`。

## 验证

通过 `Client/Create Bottom Function Icon Prefab` 生成预制体后，确认其四个节点均可在 Inspector 单独修改，且点击范围不超出 Root。

负责人完成母预制体调整后，使用 `Client/Create Bottom Function Icon Variants` 复制出 `Scan`、`Shop`、`Rank`、`Mail` 四个变体。该操作仅替换 `Icon` Sprite，绝不覆盖已有变体。

`Client/Create Team And Gacha Button Prefabs` 创建主页卡片入口预制体：`TeamButton` 与 `GachaButton`。Root 是 Button/底框，子节点为 `Character` 与最上层 `Label`，文字分别为“编队”和“扭蛋”。

`Client/Add Forward Icon To Team And Gacha` 仅向这两个已有预制体增量添加 `ForwardIcon`，放在同级最后以保证显示在人物、底框和文字之上；已有该节点时不重复添加。

放入 `MainPage` 后保持 Root 名称为 `TeamButton` 或 `GachaButton`。`ClientHomePage` 会在运行时按名称绑定前者到英雄页、后者到扭蛋页；预制体自身不持有场景导航引用。
