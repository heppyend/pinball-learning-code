# Client HeroDetailPage 模块说明

## 职责

存放英雄详情页面的拆分素材和场景内可编辑节点。详情页只展示 `IClientDataService` 提供的本地英雄数据，并由 `ClientPageNavigator` 负责从英雄页进入和返回。

## 素材

`Sprites/` 来自负责人提供的 `UI/弹珠英雄详情页/主页`。`参考.png` 仅用于人工核对布局，不得作为整页运行时背景。

## 依赖与限制

- 运行时数据入口：`Assets/Client/Runtime/ClientHeroDetailPage.cs`。
- 页面节点保存于 `Assets/Client/Scenes/ClientShell.unity`，每个可见区域均为独立 Hierarchy 节点。
- 不接入战斗、真实服务端、数值公式或资源加载基础设施。

## 技能页签（天赋 / 秘技 / 终结技）· 2026-09-21 取证

实测层级与美术接线方式（改动前是坏的，记录以免再次踩坑）：

```
downCanvas/Panel/group/天赋Toggle
    Background        <- Image.sprite = 选中的「选中框」贴图（未选中时运行时置空）
        Checkmark     <- Image.sprite = 「文字贴图」（天赋.png / 秘技.png / 终结技.png），常态恒显
```

- **`Checkmark` 是 `Background` 的子节点**（不是 Toggle 的直接子节点）。按"Toggle 直接子节点"找它永远找不到
  —— 这正是"文字贴图一直不显示"的根因，`ClientHeroDetailPage.BindToggleSkin` 已按该层级查找。
- 因为文字在底框之下，**不能用 `SetActive` 隐藏底框**；改为运行时换 sprite（选中 = `*选中框.png`，未选中 = 空）。
- 驱动组件：`Assets/Client/Runtime/UI/ClientHeroDetailToggleSkin.cs`；
  贴图引用与绑定由编辑器工具 `ClientShellStructuralRepair.FixHeroDetailSkillTabs` 写入（幂等）。
- 防回归断言：`ClientSelfTest.RunHeroDetailToggleChecks`（页签存在 / 持久 `SetActive` 调用 /
  三张选中框与三张文字贴图互不相同 / 皮肤绑定齐全）。
- 静态审计：`Logs/ui-adapt-audit3.ps1`。

## 验证

打开 `ClientShell`，进入“编队”→点击已拥有英雄；应显示详情页的英雄名、等级、战力，点击返回应回到英雄页。
技能区三个页签：点哪个哪个显示「选中框」+ 对应文字，另两个只显示文字（负责人 2026-09-21 复验项）。

## 待确认

技能数值、装备槽规则、品质/属性映射及最终立绘来源；
页签文字的垂直位置（现在居中于选中框内，可按负责人视觉微调）。
