# Stats —— 数值与说明模块

> 负责人 2026-09-20 定的架构原则：**数据只由权威服务器计算并分发**，客户端负责
> **属性接口**、**技能/天赋等数值接口** 以及 **逻辑说明显示**；单独成模块，贴近 **MVC**，
> 要求 **强解耦、拓展性强、维护方便、GC 消耗少**。总设计见 `../../MODULE.md` §16。

## 1. 职责

| 做 | 不做 |
|---|---|
| 持有**服务器算好的**属性与技能/天赋数值 | ❌ 任何数值计算（战力、加成、效果值、冷却、概率、材料是否够） |
| 按规则把技能与被动聚合成 3 个页签 | ❌ 判断"该归哪个页签"以外的业务规则（归属规则集中在一处常量，可替换） |
| 把描述模板按当前等级渲染成玩家可见文本 | ❌ 拼接其它 UI 文案（那是页面的活） |
| 向订阅者广播"数据变了" | ❌ 每帧轮询、`Update` 里查数据 |

## 2. 目录与分层

```
Stats/
├── MODULE.md                 本文件
├── Model/                    纯 C#，**零 UnityEngine 依赖**（可单测）
│   ├── ClientAttributeIds.cs     属性 id（int 常量，数据驱动，加属性零代码改动）
│   ├── ClientAttributeValue.cs   一条属性 { Id, Value }
│   ├── ClientAbilityKind.cs      Skill / Potency
│   ├── ClientHeroAbilitySection.cs  天赋 / 秘技 / 终结技
│   ├── ClientAbilityState.cs     能力状态（等级/解锁/冷却，全服务器值）
│   └── ClientAbilityEntry.cs     能力条目 + ClientAbilitySection + ClientHeroAbilitySet
├── Gateway/                  服务端契约层（客户端唯一数值入口）
│   ├── IClientStatsGateway.cs
│   └── LocalClientStatsGateway.cs    **扮演服务器**的本地实现（允许读表、允许计算）
├── Service/                  Controller + 权威数据缓存
│   ├── ClientAbilitySectionRule.cs   ★ 技能 Type → 页签 的唯一映射点
│   ├── IClientStatsService.cs
│   └── ClientStatsService.cs
├── Format/                   说明文本（"逻辑说明显示"的唯一落点）
│   ├── IClientDescriptionFormatter.cs
│   └── ClientDescriptionFormatter.cs
└── View/                     Unity 显示组件（只写控件）
    ├── ClientIconResolver.cs
    ├── ClientAttributeRowView.cs
    ├── ClientAbilityEntryView.cs
    └── ClientAbilitySectionView.cs
```

## 3. MVC 映射与依赖方向（违反即缺陷）

| 角色 | 承担者 | 铁律 |
|---|---|---|
| Model | `Model/` + `Service/` 的缓存 | 纯数据 + 服务器值；**零游戏逻辑** |
| View | `View/` | **只写控件**：不查数据、不算数、不读表、不找节点；数据靠 `Bind(...)` 推入 |
| Controller | 页面脚本 | 从 `Service` 取 → 推给 `View`；View 的输入事件上报。**单向数据流** |

```
Gateway → Service → (Controller) → View
Config  ↗                                ↘ 只读 Model
```

- `View` **不引用** `Service` / `Gateway` / 配置表 ⇒ 可单独预览、可复用、可换皮。
- `Service` **不引用**任何 Unity UI 类型。
- **配置表只被 `Service` / `Format` / `Gateway`（扮演服务器时）读取**，页面与 View 都不碰表。

## 4. 主要入口

```csharp
ClientServices.Stats                       // 组合根装配好的 IClientStatsService
ClientServices.Stats.GetHeroAbilities(id)  // 页面唯一需要调的方法（3 个页签 + 普攻）
ClientServices.Stats.GetAttributes(id)     // 属性（复用列表，立即消费，勿缓存引用）
ClientServices.Stats.AttributesChanged / AbilitiesChanged   // 事件驱动刷新
```

## 5. GC 约定（本模块必须遵守）

1. 能力集合按英雄**建一次、原地刷新**（`ClientStatsService._abilitySets`）。
2. 属性用**复用列表**，`CopyAttributes` 先清空再填，不产生新对象。
3. 查询一律 `for` + 索引：**不用 `foreach`**（接口枚举器会分配）、**不用 LINQ**。
4. 说明文本：`(id, level)` 缓存；模板无分档片段时**直接返回模板字符串本身**（零分配）。
5. View：**值没变就不写控件**（`TMP_Text.text` setter 会触发重建）；等级没变不重建字符串。
6. 强化列表用**对象池**（复用 View，不反复 `Instantiate`/`Destroy`）。
7. 事件订阅用**缓存的方法组委托**，不产生闭包；`Dispose()` 里解除订阅。

## 6. 验证方式

- **编译**：`Logs/verify-compile.ps1 -Profile All`（必须 0 error）。
- **全工程 + 运行时**：命令行 Unity
  `-executeMethod Pinball.Client.Editor.ClientTableProbe.LogRowCounts`
  —— 探针会打印英雄 13001 的 3 个页签内容与属性值，用于确认"表 → 页签 → 文本"整条链。
- 纯 Model 层无 Unity 依赖，可直接单测（当前项目未引入测试框架，故以探针代替）。

## 7. 禁止事项

- ❌ 在 `Service` / `View` / 页面里出现"算数值"的代码（战力公式、加成叠加、概率、冷却推算）。
- ❌ View 里出现 `Find`、`GetComponent` 于每帧路径、字符串拼接、读配置表。
- ❌ 把 `LocalClientStatsGateway` 的计算逻辑复制到别处 —— 那是**服务器替身**，接入真实服务器时整体替换。
- ❌ 在 `Update` 里轮询数值。

## 8. 待确认

1. **属性 id 与数值的权威来源**：目前 `ClientAttributeIds` 是**本地占位编号**，等服务器契约。
2. **属性展示顺序 / 分组 / 单位与格式**：建议放**客户端展示表**（纯显示，不涉数值）——待确认。
3. **属性名称来源**：配置目录 20 张表里**没有属性名表**，需确认由服务器下发还是客户端展示表。
4. **战力**：公式未确认 ⇒ 本地模拟**不下发**该属性（查询返回 false，UI 显示空，不猜）。
5. **被动的页签归属**：目前是**弱关联**（描述里是否出现主技能名），产品给强结构后替换
   `ClientStatsService.ResolvePotencySection`。
6. **`Type → 页签` 映射**：已由数据推出（`Type2/3/4` = 天赋/秘技/终结技，证据见 `../../MODULE.md` §15.7），
   待产品最终确认；确认后只改 `ClientAbilitySectionRule[] SectionRules`。

## 9. 落地状态（2026-09-20 已实现并验证）

17 个文件已落地，Roslyn 三目标 **0 error**，命令行 Unity 全工程编译 + 探针 **0 error / 0 异常**。

**验证证据**：`Logs/stats-module-probe3.log`

```
Stats 格式化自检：等级1=回复其8%生命值（冷却3回合） ｜ 等级15=回复其9%生命值 ｜ 末段共用后缀(等级20)=造成80%攻击力的伤害 ｜ 单个数字不动=冷却3回合，发射3枚追踪弹
Stats 页签数 = 3；普攻 = 捣药杵击
Stats 页签「天赋」 主技能 = 玉兔灵药 ｜「秘技」主技能 = 月华追踪 ｜「终结技」主技能 = 广寒月影
Stats 属性（英雄 13003，服务器替身）：Hp=1500 Atk=480 Def=160 Spd=65 暴击=400 暴伤=14500
Stats 归属自检（13001 玉兔，13 条被动）：天赋 12 条 / 秘技 1 条（`秘技强化`，描述点名"月华追踪"⇒ 正确归秘技）/ 终结技 0 条
```

### 实测中修正的两个实现问题

1. **说明文本的分档写法与我的假设不符**：实测是**每段各自带单位**（`8%/9%/10%`，字符码 `…0038 0025 002F 0039…`），
   而不是"只有末段带 `%`"。第一版只认后者 ⇒ 分档片段未被替换、原样输出。现已同时支持两种写法（用 `stackalloc Span<int>` 保证零分配）。
2. **"前端可见"与"按 id 查定义"被我混为一谈**：`GetHeroPotencies` / `GetHeroSkills` / `TryGetHero` 原先只查
   `ClientShow != 0` 的英雄 ⇒ **查不到**那 4 个天赋数据完整的英雄（它们 `ClientShow` 全是 0）。
   现已分开：`_heroByIdAll`（表内全部，供按 id 查）与 `_heroById`（前端可见，供列表）。

### 数据侧发现（不是代码问题）

- **49 个前端可见英雄的被动数据全是占位 `[1,2]`**；有完整被动数据的 4 个英雄（`13001/14001/14007/15001`）`ClientShow` **全为 0**
  ⇒ 接入正式数据前，**可见英雄的天赋页必然显示空态**（已按约定"空着、不报错"处理）。
- 被动 `300112 天赋冷却` 与 `300113 秘技冷却` 的**描述文本完全相同**（都是"天赋技能冷却-1回合"，疑似漏改），
  导致按弱关联判断时 `秘技冷却` 被兜底归到**天赋页**。这正说明"被动归属"必须是**可替换点**（`ClientStatsService.ResolvePotencySection`）。
