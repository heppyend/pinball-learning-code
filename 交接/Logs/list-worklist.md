# 列表数据化改造清单

> 来源：`Logs/hierarchy-audit.txt`（2026-09-20 层级普查）。负责人已确认：**Scroll View 的 Content 预制体普遍不规范，后续必须改为数据获取。**

## 一、总览

- 内联列表容器：**64 处**
- 内联列表项对象合计：**339 个**
- 去重后的**列表项类型：8 种** → 改造工作量取决于模板数，而非容器数

## 二、按类型归并（建议的改造单元）

| 项前缀 | 容器数 | 项数合计 | 分类 |
|---|---|---|---|
| `星级图标Image` | 35 | 175 | 固定数量展示（N★，靠显隐表达等级）→ 非列表，保留但需代码按等级显隐 |
| `道具卡牌prefab` | 18 | 98 | 真·数据列表（项数随数据变化）→ 改数据生成 |
| `角色卡牌Button-final 1` | 5 | 32 | 真·数据列表 → 改数据生成 |
| `任务卡prefabs` | 2 | 12 | 真·数据列表（项数随数据变化）→ 改数据生成 |
| `角色卡牌Button-final` | 1 | 6 | 真·数据列表 → 改数据生成 |
| `邮件prefab` | 1 | 6 | 真·数据列表（项数随数据变化）→ 改数据生成 |
| `玩家战力排行prefabs` | 1 | 5 | 真·数据列表（项数随数据变化）→ 改数据生成 |
| `玩家排行perfabs` | 1 | 5 | 真·数据列表（项数随数据变化）→ 改数据生成 |

## 三、按页面明细

### ActivityPage活动（12 容器 / 62 项）

- `新手任务Scroll View/Viewport/Content`  →  `任务卡prefabs` × 6（容器子节点 6）
- `新手任务Scroll View/Viewport/Content/任务卡prefabs/background/道具大底框/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `新手任务Scroll View/Viewport/Content/任务卡prefabs (1)/background/道具大底框/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `新手任务Scroll View/Viewport/Content/任务卡prefabs (2)/background/道具大底框/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `新手任务Scroll View/Viewport/Content/任务卡prefabs (3)/background/道具大底框/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `新手任务Scroll View/Viewport/Content/任务卡prefabs (4)/background/道具大底框/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `新手任务Scroll View/Viewport/Content/任务卡prefabs (5)/background/道具大底框/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `进阶任务Scroll View/Viewport/Content`  →  `任务卡prefabs` × 6（容器子节点 6）
- `进阶任务Scroll View/Viewport/Content/任务卡prefabs/background/道具大底框/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `进阶任务Scroll View/Viewport/Content/任务卡prefabs (1)/background/道具大底框/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `进阶任务Scroll View/Viewport/Content/任务卡prefabs (2)/background/道具大底框/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `进阶任务Scroll View/Viewport/Content/任务卡prefabs (3)/background/道具大底框/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）

### FormationPage编队（12 容器 / 62 项）

- `Scroll View/Viewport/Content`  →  `角色卡牌Button-final 1` × 9（容器子节点 9）
- `Scroll View/Viewport/Content/角色卡牌Button-final 1/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `Scroll View/Viewport/Content/角色卡牌Button-final 1 (1)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `Scroll View/Viewport/Content/角色卡牌Button-final 1 (2)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `Scroll View/Viewport/Content/角色卡牌Button-final 1 (3)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `Scroll View/Viewport/Content/角色卡牌Button-final 1 (4)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `Scroll View/Viewport/Content/角色卡牌Button-final 1 (5)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `Scroll View/Viewport/Content/角色卡牌Button-final 1 (6)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `Scroll View/Viewport/Content/角色卡牌Button-final 1 (7)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `Scroll View/Viewport/Content/角色卡牌Button-final 1 (8)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `编队卡牌-Panel`  →  `角色卡牌Button-final 1` × 3（容器子节点 3）
- `编队卡牌-Panel/角色卡牌Button-final 1/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）

### ProfilePage个人中心（12 容器 / 62 项）

- `PageContentRoot/Panel_Profile/MiddleImage/Panel2/弹珠展示底框Image/Panel`  →  `角色卡牌Button-final 1` × 3（容器子节点 3）
- `PageContentRoot/Panel_Profile/MiddleImage/Panel2/弹珠展示底框Image/Panel/角色卡牌Button-final 1/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `PageContentRoot/Panel_Profile/MiddleImage/Panel2/弹珠展示底框Image/Panel/角色卡牌Button-final 1 (1)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `PageContentRoot/Panel_Profile/MiddleImage/Panel2/弹珠展示底框Image/Panel/角色卡牌Button-final 1 (2)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content`  →  `角色卡牌Button-final 1` × 9（容器子节点 9）
- `PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content/角色卡牌Button-final 1/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content/角色卡牌Button-final 1 (1)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content/角色卡牌Button-final 1 (2)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content/角色卡牌Button-final 1 (3)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content/角色卡牌Button-final 1 (4)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content/角色卡牌Button-final 1 (5)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content/角色卡牌Button-final 1 (6)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）

### HeroPage英雄（9 容器 / 48 项）

- `middleCanvas/MiddleCanvas/Canvas/Scroll View/Viewport/Content`  →  `角色卡牌Button-final 1` × 8（容器子节点 8）
- `middleCanvas/MiddleCanvas/Canvas/Scroll View/Viewport/Content/角色卡牌Button-final 1/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `middleCanvas/MiddleCanvas/Canvas/Scroll View/Viewport/Content/角色卡牌Button-final 1 (1)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `middleCanvas/MiddleCanvas/Canvas/Scroll View/Viewport/Content/角色卡牌Button-final 1 (2)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `middleCanvas/MiddleCanvas/Canvas/Scroll View/Viewport/Content/角色卡牌Button-final 1 (3)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `middleCanvas/MiddleCanvas/Canvas/Scroll View/Viewport/Content/角色卡牌Button-final 1 (4)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `middleCanvas/MiddleCanvas/Canvas/Scroll View/Viewport/Content/角色卡牌Button-final 1 (5)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `middleCanvas/MiddleCanvas/Canvas/Scroll View/Viewport/Content/角色卡牌Button-final 1 (6)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `middleCanvas/MiddleCanvas/Canvas/Scroll View/Viewport/Content/角色卡牌Button-final 1 (7)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）

### Player lineup（7 容器 / 36 项）

- `background/战力底框/展示卡牌Scroll View/Viewport/Content`  →  `角色卡牌Button-final` × 6（容器子节点 6）
- `background/战力底框/展示卡牌Scroll View/Viewport/Content/角色卡牌Button-final/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `background/战力底框/展示卡牌Scroll View/Viewport/Content/角色卡牌Button-final (1)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `background/战力底框/展示卡牌Scroll View/Viewport/Content/角色卡牌Button-final (2)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `background/战力底框/展示卡牌Scroll View/Viewport/Content/角色卡牌Button-final (3)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `background/战力底框/展示卡牌Scroll View/Viewport/Content/角色卡牌Button-final (4)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）
- `background/战力底框/展示卡牌Scroll View/Viewport/Content/角色卡牌Button-final (5)/垂直星星Panel`  →  `星级图标Image` × 5（容器子节点 5）

### MailPage邮件（7 容器 / 36 项）

- `Email come/邮件Scroll View/Viewport/Content`  →  `邮件prefab` × 6（容器子节点 6）
- `Email come/邮件Scroll View/Viewport/Content/邮件prefab/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `Email come/邮件Scroll View/Viewport/Content/邮件prefab (1)/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `Email come/邮件Scroll View/Viewport/Content/邮件prefab (2)/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `Email come/邮件Scroll View/Viewport/Content/邮件prefab (3)/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `Email come/邮件Scroll View/Viewport/Content/邮件prefab (4)/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）
- `Email come/邮件Scroll View/Viewport/Content/邮件prefab (5)/Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）

### ShopPage商店（1 容器 / 13 项）

- `Product page/展示卡牌Scroll View/Viewport/Content`  →  `道具卡牌prefab` × 13（容器子节点 13）

### RankPage排行榜（2 容器 / 10 项）

- `Challenge List/rank/邮件Scroll View/Viewport/Content`  →  `玩家排行perfabs` × 5（容器子节点 5）
- `Battle Power Ranking/rank/邮件Scroll View/Viewport/Content`  →  `玩家战力排行prefabs` × 5（容器子节点 5）

### HeroDetailPage英雄详情主页（1 容器 / 5 项）

- `upCanvas/垂直Panel`  →  `星级图标Image` × 5（容器子节点 5）

### Email Details Page（Have）（1 容器 / 5 项）

- `邮件详情底框/邮件物品底框/Scroll View 1/Viewport/Content`  →  `道具卡牌prefab` × 5（容器子节点 5）

## 四、改造约定（见 Assets/Client/MODULE.md §7）

1. 场景中每种列表项**只保留 1 个模板**，置于容器内并**默认禁用**。
2. 运行时按数据条数实例化 / 回收；**禁止**再手工复制实例。
3. 列表项数量必须与数据源条数一致；不一致时**隐藏多余项**并输出告警（已在 `ClientShopPage` / `ClientMailPage` 落地）。
4. 容器配 `ContentSizeFitter` 时锚点必须单向拉伸（`(0,1)-(1,1)`、`pivot(0.5,1)`）。
5. **不改变既有美术与布局尺寸**：模板的 rect 沿用被替换实例的 rect。
