# Client MainPage 模块说明

## 职责

主页面视觉资源与入口交互。背景、立绘、头像、面板、卡片、导航框与每个入口均由 `ClientShell` 中独立的 UI 节点构成，可在 Hierarchy/Inspector 中选中、替换 Sprite、调整布局和绑定事件。

## 当前限制

- `参考.png` 只用于布局核对，禁止再作为运行时整页背景。
- 主页专用中央立绘尚未提供独立素材；当前临时复用 `UI/HomeDisplay/Hero001.png` 的独立节点，获得正式素材后直接在 Inspector 替换。
