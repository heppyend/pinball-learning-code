# Client 模块说明

本文件是 `Assets/Client` 的**强制约定**。进入、修改或新增本模块内容前先读它；
`AGENTS.md` 的长期约束优先于本文件，本文件优先于临时的口头约定。

---

## 1. 职责

新建的非战斗客户端模块：承载养成及其他小程序端界面、领域模型、可替换的本地模拟数据和独立开发场景。
**不承载**既有街机战斗、服务端协议或基础设施改造。

---

## 2. 入口与目录（2026-09-20 校正）

| 路径 | 内容 |
|---|---|
| `Scenes/ClientShell.unity` | 客户端开发入口场景。**不是空场景**：含 `ClientCanvas` 三层结构与 23 个页面（约 2841 个 GameObject）。不加入 Build Settings。 |
| `Runtime/` | 运行时脚本（页面 View、导航、弹窗、服务、领域模型）。 |
| `Runtime/UI/` | 共享 UI 内核：`IClientNavigation` / `IClientPopup` / `IClientFeedback` / `ClientPageViewBase` / `ClientUiNavigator` / `ClientPopupService` / `ClientShellController` / `ClientUiTrace`。 |
| `Editor/` | 编辑器工具：`ClientShellStructuralRepair`（结构修复）、`ClientHierarchyAudit`（层级普查）。 |
| `UI/` | 页面美术资源。正式打包前确认 YooAsset 分包、地址与回收策略。 |
| `Tests/` | 纯逻辑自动化验证。 |

---

## 3. 三层结构约定

```
ClientCanvas
├─ PagesLayer    只放「大厅」一个页面：MainPage主页
├─ PopupLayer    其余全部界面：一级全屏界面 + 二级弹窗
└─ SystemLayer   跨页面常驻：加载Page、Toast
```

- **禁止**把除 `MainPage主页` 以外的界面放进 `PagesLayer`。
- 一级全屏界面在 `ClientUiPopup._pageId` 上标注路由 ID 且 `_routeByPageId = true`；
  二级弹窗（购买确认、成功页、邮件详情等）**不占用 pageId**，`_routeByPageId = false`，由调用方持引用打开。
- 弹窗根**必须全屏拉伸**（`anchorMin=(0,0)`、`anchorMax=(1,1)`、offset 全 0）。
  非全屏根没有天然模态边界，曾导致必须外挂全局遮罩，最终引发"遮罩盖住栈顶弹窗""遮罩残留锁死界面"两个故障。
  > 普查现状：23 个页面中 **22 个不满足**（仅 `Purchase Success Page` 全屏）。收敛时逐页补齐。

---

## 4. 命名约定

| 角色 | 命名 | 说明 |
|---|---|---|
| 页面根 | `XxxPage`（英文）+ 中文后缀可选 | 如 `ShopPage商店`、`MainPage主页` |
| 区域容器 | `XxxContent` / `XxxArea` | 页面内的功能分区 |
| 列表容器 | `XxxList` | 仅承载 Item，自身不画图 |
| 列表项模板 | `XxxItem` | **场景中只允许 1 个，且默认禁用** |
| 按钮 | `XxxButton` | 交互节点 |
| 图片 | `XxxImage` | 纯显示节点 |

**禁止**：
- 跨页面关键词（`RankPage` 里叫 `邮件Scroll View`、`MailPage` 里叫 `道具卡牌prefab`）。
  > 普查现状：`MailPage` 有 **30 个**此类节点，`RankPage` 2 个。
- 名字含首尾空格（现状 3 处：`Panel_Personalize␣␣`、`␣Card info2`、`challenge␣`）。
- 拼写错误（现状 5 处：`玩家排行perfabs` 应为 `perfabs→prefab`）。
- 裸 `Image` 作为同级多个子节点的名字（现状 18 处；无法在代码里可靠区分）。
- 复制粘贴产生的指数后缀堆叠（`xxx (1)/Scroll View/.../yyy (2)`）。

---

## 5. 交互约定

- **交互节点必须自带命中面**：Button/Toggle/ScrollRect 所在节点，或它的直接子节点，必须有
  `raycastTarget = true` 的 `Graphic`。**禁止依赖祖先的 Image 接收射线**——祖先的射线一旦按第 6 条关闭，该交互立即失效。
  > 普查现状：全场景 **0 个违规**（历史上有 17 个 `…/avatar Mask/Avatar` 属于此类）。这是必须保持的基线。
- **装饰 Graphic 一律 `raycastTarget = false`**：不承担交互的 Image / RawImage / 文本不得吞掉点击。
  > 普查现状：**0 个违规**（2026-09-20 已全场景解耦 479 个）。同样必须保持。
- 交互组件只挂在能接收射线的交互父节点上；文本子节点只负责显示。

---

## 6. 模态约定

- **每个页面根自带 `CanvasGroup`，由它控制模态**：
  打开弹窗时把**其下层页面**的 `blocksRaycasts = false`，关闭后恢复。
- **禁止再引入全局遮罩节点**。2026-09-20 曾新增 `弹窗遮罩`，实测造成两个新故障并已撤除：
  遮罩盖住栈顶弹窗（购买界面点不动）、栈未清空时遮罩残留把整个界面锁死。
- 需要暗色背景时，用**弹窗自身根上的半透明 Image**（随弹窗生死），不要用跨页面的常驻节点。
- 每个页面根应自带独立 `Canvas`（现状多数二级弹窗已满足：`CanvasScaler + GraphicRaycaster`），
  以隔离 Canvas 重建；**禁止**把全部弹窗摊在大厅同一个 Canvas 下。

---

## 7. 列表约定

> **负责人已确认（2026-09-20）**：Scroll View 的 Content 预制体普遍不规范，**后续必须改为通过数据获取**。
> 改造清单见 `Logs/list-worklist.md`（由层级普查报告归并生成）。
>
> 普查归并结果：内联列表 **64 个容器 / 约 339 个项对象**，但去重后**只有 8 种列表项类型**
> （`星级图标Image`×175、`道具卡牌prefab`×98、`角色卡牌Button-final 1`×32、`任务卡prefabs`×12、
> `角色卡牌Button-final`×6、`邮件prefab`×6、`玩家战力排行prefabs`×5、`玩家排行perfabs`×5）。
> 因此改造工作量取决于**模板种类数**，而非容器数——先做这 8 个模板即可覆盖全部。
>
> 注意区分两类：`星级图标Image` 这类**固定数量展示**（5 星，靠显隐表达等级）不是数据列表，
> 保留节点结构、由代码按等级显隐即可；只有**项数随数据变化**的才是真列表。

- **列表由数据生成**：场景中只保留 1 个 `XxxItem` 模板（禁用），运行时按数据实例化与回收。

### 收敛一页的四步流程（必须按序，缺一步就会出回归）

1. **取证**：查 `Logs/hierarchy-audit.txt` 的"数据 ↔ 节点"节，确认容器项数与数据条数是否失配。
2. **校验前提**：容器必须自带布局组件（`GridLayoutGroup`/`VerticalLayoutGroup`/`HorizontalLayoutGroup`）。
   `ConvergeList` 会拒绝无布局组件的容器——否则克隆体会叠在同一坐标，那才是真的改布局。
   `星级图标Image` 这类**固定数量展示不得套用**（它是靠显隐表达等级，不是列表）。
3. **先改代码，后动场景**：必须**先**给该页 View 加模板生成逻辑（含内联回退路径）并编译通过。
   ⚠️ 顺序颠倒会让页面只剩 1 个禁用模板 —— 这是**可见的功能回归**。
4. **登记白名单再执行**：把容器登记到
   `Assets/Client/Editor/ClientShellStructuralRepair.cs` 的 `ListTargets` 白名单，跑 `RepairAll`，
   最后用普查报告核对**节点数与内联列表数确实下降**。白名单是显式的，不做"扫描到就收敛"。

> 已完成：`ShopPage商店`（130→46 节点）、`MailPage邮件`（263→63 节点）。
- **禁止**在场景里内联展开列表项。
  > 普查现状：**64 处**内联列表。最严重的是 `MailPage` 的 `邮件prefab ×6`，每个又内嵌 `道具卡牌prefab ×5`；
  > `ProfilePage个人中心` 26 处、`ActivityPage活动` 15 处、`FormationPage编队` 14 处。
- 列表项数量必须与数据源条数对应。曾出现"商城 13 张卡但 `_listings` 更少"，导致多数卡点了无反应——
  `OpenListing(index)` 有越界保护，所以**表现为静默无响应**，极难排查。
- 列表容器配 `GridLayoutGroup`/`VerticalLayoutGroup` + `ContentSizeFitter` 时，
  锚点必须是**单向拉伸**（`anchorMin=(0,1)`、`anchorMax=(1,1)`、`pivot=(0.5,1)`）；
  双向拉伸会让 `ContentSizeFitter` 完全失效、列表滚不动。

---

## 8. 深度与规模上限

- 单页**层级深度 ≤ 8**、节点数建议 ≤ 200。超出即需拆分区域或改为数据生成。
  > 普查现状超标页：`ActivityPage活动`（687 节点 / 深度 13）、`ProfilePage个人中心`（540 / 11）、
  > `MailPage邮件`（263 / 12）、`RankPage排行榜`（282 / 11）、`HeroPage英雄`（190 / 10）。

---

## 9. 依赖与禁止事项

- 允许通过最小适配器复用既有共享能力；**不得**直接重构 `Assets/Main`、`Assets/Scripts/GameLogic`、
  战斗场景、YooAsset、HybridCLR、XLua、URP 或微信 SDK。
- 未有服务端契约时只使用本地模拟接口；**UI 不可直接写入**货币、背包、奖励或运营数值。
- 所有可配置内容（英雄、道具、商品、活动、任务、文案、货币）必须数据驱动，
  **禁止**把运营内容与数值硬编码在 UI 回调里。
- 新增子目录时必须创建其 `MODULE.md`。
- 不得为普通实现另建隔离工作区；可直接在 `D:\unity project\pinball` 原项目内修改。

---

## 10. 验证方式

1. **编译**：任何影响 Unity 编译输入的改动，交付前执行
   `& "D:\unity project\pinball\Logs\verify-compile.ps1" -Profile All`，
   要求 `Assembly-CSharp[Editor]` / `Assembly-CSharp-Editor[Editor]` / `Assembly-CSharp[WebGL]` 三目标 **0 error**。
2. **层级自检**：改完页面结构后运行
   `Client → 结构修复 → 6. 层级普查报告（只读）`
   （批处理：`-executeMethod Pinball.Client.Editor.ClientHierarchyAudit.RunAudit`），
   报告落盘 `Logs/hierarchy-audit.txt`，核对第 5、7、8 节的指标是否改善且未回退。
3. **运行时自检**：Play 后 Console 筛 `[UI追踪]`，应有
   `[点击]`（命中层级 + 接收组件）、`[导航]`、`[弹窗]`（含栈深）、`[显隐]`、`[路由`、`[性能]`。
   追踪器由 `ClientShellController` 自动补挂，`ClientUiTrace.Enabled = false` 可整体静音。
4. **场景写入**：结构类修改统一走 `ClientShellStructuralRepair.RepairAll()`（幂等、可重复运行）。
   运行前必须关闭 Unity（批处理需要工程锁）；场景以磁盘文件为准，未保存的 Inspector 改动会丢失。

---

## 11. 交付与验收流程（负责人强制要求）

**一次只做一页，做完必须说明，验收通过才算完成。**

1. **不得跨页批量推进**。当前页未验收前，不开工下一页。
2. 每页完成后必须向负责人**书面说明**：改了什么、依据是什么、影响哪些节点数、
   如何验证、有哪些遗留与风险。
3. **负责人在 Unity 里 Play 验收通过后，该页才算"完成"**；在验收前只能标注为"待验收"，
   `CURRENT_STATE.md` 中不得写成"已完成"。
4. 场景写入是**不可逆**的（会以磁盘文件为准、丢弃未保存改动），
   所以写入前应先说明将要发生的变化并取得确认。
5. 若一页内包含多个独立子项（如某页有多个列表容器），仍按**一页一验收**处理，
   不拆成多次反复打断负责人。

> 当前待验收：`ShopPage商店`、`MailPage邮件`（已执行场景写入，等待 Play 验收）。
> 进行中：`HeroPage英雄`（代码侧已就绪，**场景写入未执行**）。

---

## 12. 诊断与修复纪律（2026-09-20 复盘，来自弹窗层级问题）

这一节的每一条都是实际踩过的坑，**不是泛泛建议**。

### 12.1 排序不由层级单独决定 —— 先查清"排序按什么决定"

- 症状：弹窗"层级全部跑到商店后面"。
- 真相：**`overrideSorting = true` 的嵌套 Canvas 完全无视层级顺序**，只按 `sortingOrder` 比大小。
  `ShopPage商店/Product page` 被抬到 `order=1005`，弹窗都在 0/100 → 商店永远在最上。
- 我连续 4 轮改**兄弟序号**（`SetAsLastSibling`、整体重排整个栈）**全部无效** —— 因为那是一个
  **不参与排序的维度**。
- 纪律：**动手前先打印每个图层的 `Canvas.overrideSorting` 与 `sortingOrder`**，确认排序依据。
  只检查兄弟序号会得出"顺序没问题"的错误结论。

### 12.2 运行时读数不能当布局事实

- 我把 Play 中读到的 `rect.rect.size = 215.9 × 2219.1` 当成排版事实，据此判断成功页是"216 宽的窄竖带"，
  并让负责人授权"按全屏修正" —— 实际场景值是**居中的 752×1527 面板**，改完把界面盖住了。
- 纪律：**布局事实以场景文件 / Inspector 为准**。运行时打印只能说明"当前帧的渲染状态"，
  `rect.rect.size` 受父级与 CanvasScaler 影响，不能用来推断排版。

### 12.3 沿"谁能改变这个状态"穷举，而不是沿"哪里可能不对"猜

- 弹窗不显示：先猜 View 桥接（`OnPopupSuspend`）→ **错**，真因是服务里的 `previousTop.SetVisible(false)`。
- 层级反了：先改兄弟序号 → **错**，真因是页面侧 `BringToFront` 里的 canvas 覆盖。
- 纪律：**用一次全局搜索列出所有能写该属性的代码路径**，再判断谁是所有者；不要改一处试一次。

### 12.4 同一所有者原则

`Awake` 自隐藏 / `Open` 隐藏下层 / `Show` 越权置顶 —— 三个 bug 同源：
这套 UI 原本是"**一个界面独占屏幕、谁显示谁置顶**"，迁移到弹窗栈后，
**显隐与层级必须只由 `ClientPopupService` 决定**，而遗留代码还在各自动手。

- 页面 View **不得**自行 `SetActive` 根节点、**不得**自行改层级（`SetAsLastSibling` /
  `overrideSorting` / `sortingOrder`）。
- 弹窗之间的层级统一走 `ClientPopupService.ApplyStackOrder()`。

### 12.5 反证要当成反证

负责人连续说"没有任何变化"时，我仍在同一方向继续调整。
**当修复完全不产生可观察变化时，首先要怀疑"我打在了不参与该结果的维度上"**，
而不是加大力度或换一个同类改法。

### 12.6 日志要打印"状态量"，不是"调用成功"

定位到根因的三次突破，全部来自打印真实状态：
`activeSelf / activeInHierarchy / 首个未激活祖先 / 兄弟序号`、栈快照、`Canvas` 链。
"我调用了某方法"不构成证据；**"该对象此刻处于什么状态"才是**。

### 12.7 工作流纪律

- **纯代码改动**：不关 Unity、不重启；只做独立编译验证，由负责人在 Unity 里 `Ctrl+R`。
  （此前的"关→跑→重启"一条龙会每次触发 Unity 提权警告，打扰负责人。）
- **需要写场景**：才关 Unity 跑批处理。场景写入不可逆，写入前说明并取得确认。
- 一次一页、每页独立验收（见 §11）。

---

## 13. 卡牌与子界面（HeroPage 确立的模式，其它列表页可照搬）

负责人 2026-09-20 指出"英雄卡牌的各个接口逻辑应该单独写几个脚本来管理"，据此确立：

- **`ClientHeroCard`**（挂在卡牌模板上）：**单张卡牌的唯一管理者** —— 名称 / 等级 / 立绘 / 锁 / 遮罩 / 编队编号 / 点击全在这里。
  **刻意不依赖领域模型**：`Apply(string, int, bool, Sprite, Action)` 只收基础类型，由调用方做映射 → 任何"有名字/等级/解锁状态"的列表都能复用。
  内置两条硬约束：**锁与遮罩必须同步显隐**；**编队编号默认强制隐藏**（仅 FormationPage 显式打开）。
- **`ClientHeroCardList`**（非 MonoBehaviour，由页面持有）：管理"**1 模板 + N 实例**"。**实例数恒等于数据条数**（不足新建、多余隐藏），每次 `Show` 重新对齐 → 杜绝"只按绑定数量判断、从不回收"导致的 2→4→6 累积。
- **页面（如 `ClientHeroPage`）只负责**：当前子页、页签、进度文本，把数据交给 CardList。
- **子页模式（方案 A）**：本页维护"当前子页"，两个子页（`HeroSubPage` / `GuideSubPage`）**共用同一套卡牌逻辑**，只是数据范围与 `unlockedOf` 不同。
- **索引映射**：**不要用"下标 % 全表长度"** —— 两个子页数据长度与顺序不同，必然错位。各列表在自己的回调里**按自己的快照取对象**。
- **只读语义**：详情页对**未解锁英雄**只读（装备栏与升级按钮不可交互），对已解锁英雄全可交互。
  判定放在**详情页 `Show()` 内按 `IsOwned` 自动**完成，保证**任何入口**行为一致。
  "未解锁英雄无法获取"不是死结 —— 该按钮语义是**获取英雄技能/天赋的升级材料**，不是获取英雄。
- **页签选中态**：用 Button 的 `Transition = Animation` + Unity 标准 trigger（`Normal`/`Highlighted`/`Pressed`/`Selected`/`Disabled`），**显式驱动 Animator** 而不是只设 `EventSystem` 选中对象（否则被点过的按钮会停在 `Pressed`）。
- **商店卡面数字 = 售价**（负责人 2026-09-20 明确）：写入节点是 **`道具数量`**，值取 `ClientShopListing.UnitCrystalStoneCost`，**不是拥有数**。
  ⚠️ **不要写成 `数量角标`**：直接解析 `Assets/Client/UI/Prefabs/道具卡牌prefab.prefab`（商店卡是它的 prefab 实例）可见，卡上**只有两个文本节点** `道具名称` / `道具数量`，且两者设计时 `m_text` 就是**各自的节点名**（典型占位符）；左下角 `数量角标` 与 `数量黑底` **都只有 Image、没有文本**，写它们不会有任何显示。
  语义由**页面**决定：同一张卡在奖励 / 邮件场景下 `道具数量` 表示“数量”（`ClientActivityPage` 绑的就是它）。
- **卡牌管理器的编队能力**（2026-09-20，`FormationPage编队` 需求）：`ClientHeroCard.SetTeamSlot(slotNumber, numberSprite = null)` 按槽位号（1..3）显隐队内编号；`ClientHeroCardList.Show(..., Func<T,int> teamSlotOf, onClick)` 为带编号装饰的重载，**其余语义与原重载完全一致**。
  ⚠️ 编号写在 `Apply` **之后** —— 因为 `ClientHeroCard.Apply` 末尾会把队内编号强制隐藏。
  ⚠️ `编队队内编号Image/编号数字` 实测是 **Image 而非文本**（`Logs/hierarchy-audit.txt` 第九节：`编号数字 … 组件: CanvasRenderer Image`），**写不了数字文本**。
  **数字贴图工程内已存在**（2026-09-20 核实）：`Assets/Client/UI/HeroDetailPage/Sprites/属性icon/新建文件夹/数字/0..9.png`（卡的 `编号数字` 用的就是其中的 `1.png`）。
  因此动态数字走**换 sprite**：`ClientFormationPage._teamSlotDigits`（下标 = 数字）由结构修复工具写入，运行时按“该英雄在当前队伍的槽位号(1..3)”取图。
  编号显示在 **`Scroll View` 的选卡**上，**不在** 3 个槽位卡上。

---


## 14. 待确认

- 首个页面优先级、UI 适配基线、字体、图标来源、动效与性能预算。
- 本地模拟数据与未来服务端契约的字段边界。
- `Email Details Page（Have）/(No)` 与 `Success Receipt Interface` 已迁入 `PopupLayer`，
  但**代码中没有任何打开它们的逻辑**；进入条件与内容待负责人确认。
- ~~`Purchase Success Page`、`Success Receipt Interface` 交互节点数为 **0**，打开后无关闭手段，待补齐。~~
  **2026-09-20 更正（第 61 轮取证）**：
  · **`Purchase Success Page` 不存在"关不掉"** —— `ClientShopPage` 第 233 行 `Set(_success, true)` 打开它，
    第 288 行 `BindSuccessDismissHandlers()` 在**运行时**遍历其全部 `Graphic`、恢复 `raycastTarget` 并补 `ClientShopCloseOnClick`（→ `CloseSuccessPage`），
    注释写明负责人要求"点击任何地方直接退回购买界面" ⇒ 关闭路径完整 ✓。
  · **`Success Receipt Interface` 是死内容**：全工程**只有结构修复工具**引用它，**没有任何运行时代码打开它**；
    根节点无 Image、子节点无 Button ⇒ 既打不开也无从关闭。属"**尚未确定用途的场景残留**"，进入条件待产品确认，**不擅自删除**。
- `FormationPage编队`（3 槽位 / 4 队伍 / 队内编号 / 详情键）与全图鉴活动的完整逻辑尚未实现。
  **2026-09-20 取证补全（阻断原因，勿再重复排查）**：场景已把意图摆好 —— `编队卡牌-Panel` **3 槽位**（含 `编队队内编号Image`，默认关）、`页面分栏/Panel` **选中点1..4 + 左右Button + `编组号-Text`**（4 队伍）、`元素属性分类列表` **7 个元素筛选**、`搜索icon-image`、`战力框`、`查看卡面信息tips`、`确认键Button`、`Scroll View/Content` **9 张英雄卡**。
  但 **`IClientDataService` 只有单个** `GetFormationHeroId()` / `TrySetFormationHero(heroId)`（要求 `IsOwned`）⇒ 多槽位 / 队伍 / 筛选 / 搜索 / 编队战力**均无接口**（`ClientPlayerLineup.Cards` 是排行榜打榜阵容，不是自己的编队）。
  且**场景结构未就绪**：该页的卡**都没有 `ClientHeroCard`**（全场景仅 HeroPage 两处），`角色卡牌Button-final 1.prefab` 的 `卡牌名称Image` **没有文本子节点**（HeroPage 的 `卡牌名称` 系 `WireHeroCard()` 新建）。⇒ 要与 HeroPage 同款的场景处理才可绑定数据。
  `_slot` / `_status` 未接线属**意图未定**（修复工具原文：“需要负责人按实际意图拖拽”），不是漏拖。
- **`ProfilePage个人中心` 的真列表缺数据源（2026-09-20 只读取证，**阻断该页列表收敛**）**：
  审计对该页报 **26 处**"疑似内联列表"，但其中 **`星级图标Image`（`垂直星星Panel` 下 ×5）属固定 5 星展示，不是列表，不得模板化**。注意审计工具**自身**在 `ClientHierarchyAudit.cs` 的说明文字里也写明"`星级图标Image` 属固定 5 星展示，不是列表"，只是逐页清单仍**机械**标记"✔ 可安全模板化" —— **以本条人工结论为准**。
  其余 **6 处真列表**：`Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content`（9 张 `角色卡牌Button-final 1`，Grid 3 列）、`Panel_Personalize` 下的 头像(5)/徽章(5)/铭牌(9)/头像框(2)/称号(3+3)。
  **`IClientDataService` 中没有任何对应集合 API**：只有单个 `GetShowcaseHeroId()` / `GetEquippedBadgeId()`，以及 `ClientPlayerProfile.EquippedTitleId` / `EquippedAvatarId`；`ClientItemType` 仅 `Material/Equipment/Consumable`，**没有头像 / 徽章 / 称号 / 铭牌 / 头像框分类**。
  ⇒ ~~条数来源、拥有/未拥有规则、图标映射、空态全部未定义~~ **2026-09-20 更正：数据源与规则均已存在**，此前"未定义"的结论基于工程内无表可读，属**查证不足**。
  **权威来源**：`D:\Users\Administrator\Desktop\弹珠配置\excel\1服-弹珠\Client\`（`Json` 为真实数据、`Cs` 为生成类）。`Head / HeadFrame / Badge / Nameplate / Title` **同一形状**：
  `Id / Name / Desc / Sorting（排序）/ ClientShow（前端是否显示）/ NotUnlockedClientShow（未解锁是否显示）/ ResId（美术资源）`；`Head` 用字符串 `Avatar`（约定 `HeroAvatar_<id>`）代替 `ResId`。
  ⇒ **条数 = 表行数（按 `ClientShow` 过滤）、排序 = `Sorting`、未解锁是否占位 = `NotUnlockedClientShow`、图标 = `ResId`/`Avatar`**，空态与"拥有/未拥有"规则都由此推导，**不要再当"未定义"处理**。
  ⚠️ **但工程侧仍缺件**（这才是真正阻断）：`Assets/Scripts/Table/` **没有** `TBadge.cs` / `TNameplate.cs` / `TPotency.cs` / `TPotencyLevel.cs`（配置目录里有），`TableLoadConfig.TableHelperTypeList` 的 17 张清单里也没有这四张；且全工程**没有任何表 JSON**（无 `Resources/Table`），`TableLoadHelper` 因此走"本地配置表缺失，使用空表继续"分支。**见 `CURRENT_STATE.md` 的「📦 配置表来源已确认」与 `BUG_TRACKER.md` BUG-019。**
---


## 15. 客户端 ↔ 配置表 对齐审计（2026-09-20）

**背景**：负责人定调「**表驱动**」，且配置以现状为准。本节回答"现有客户端功能与接口**对不对得上**配置、缺什么"。
配置权威源：`D:\Users\Administrator\Desktop\弹珠配置\excel\1服-弹珠\Client\`；客户端读表入口 `ClientTableSource`（远端 `config.pkg` → 本地 `Resources/Table/*.json`），静态配置查询走 `IClientConfigService`（`ClientServices.Config`）。

### 15.1 结论：配置里**有**、客户端却还在用假数据的（要接）

| 功能 | 客户端现状 | 配置来源 | 对不对得上 |
|---|---|---|---|
| 英雄图鉴 / 列表 | `LocalClientDataService` 手写 **9 个**假英雄；`HeroId="hero-001"`、`Element="Fire"`、自带 `CombatPower` | `THero` **54 行**（`ClientShow!=0` 的 **49**） | ❌ id 应为 **int**（13001…）；`Element` 应为 **int 1..6**；缺 `Quality/QualityIcon/CatapultType/CatapultIcon/Star/MaxStar/Avatar/Portrait1/Portrait2`；**表里没有"战力"字段** |
| 英雄详情（天赋/秘技/终结技） | `ClientHeroDetailData` 用 **int 图片 id** + 硬编码文案 | `THero.Skill`(4 个技能 id) → `TSkill`(33 行)；`THero.Potency` → `TPotency`(68) / `TPotencyLevel`(3) | ❌ 名字 ↔ 数字 id 不一致；技能/潜能表**未接** |
| 道具 / 背包 | `ClientInventoryItem.ItemId` 为 **string**；`ClientItemType` = Material(1)/Equipment(2)/Consumable(3) | `TItem` **9 行**，`Type∈{1,3}`：**1=货币**（点数/源晶/晶核）、**3=消耗品**；`Type=2` **表里不存在** | ❌ `Type=1` 客户端叫 `Material`，实为**货币**；`Icon` 是**名字**（`Item_110001`） |
| 头像 / 头像框 / 徽章 / 铭牌 / 称号 | **接口里完全没有集合能力**（只有单个 `GetEquippedBadgeId()`，且是 string） | `THead`(6) / `THeadFrame`(9) / `TBadge`(6) / `TNameplate`(5) / `TTitle`(9) | ❌ **缺一整套接口**（列表来自 `Config`，拥有/装备属玩家状态） |
| 主页展示 | `GetShowcaseHeroId()` 返回 **string** | `THero` | ❌ id 类型不符 |
| 卡池 | `ClientGachaPool.RewardHeroIds` 为 **string** 列表 + 手写价格 | `TRandomHero`(4 行：`Id`/`Heroid`) | ⚠️ 表里**没有价格**（抽卡消耗属服务端）；`Heroid` 为 int |

### 15.2 配置里**本来就没有** → 属服务端（客户端本地模拟继续，等接服务端）

商城商品、活动、任务、公告、邮件、排行榜、抽卡消耗、玩家档案/钱包/拥有列表/等级、编队、弹珠。
佐证：`ClientShopListing` 自己的注释就是"本地占位配置；正式商品资料接入时由服务端 DTO 替换"；配置目录 20 张表里也确实没有这些表。

### 15.3 三条待拍板点（**2026-09-20 负责人已拍板，见下**）

**结论（落地见 `CURRENT_STATE.md` 第 53 轮）：**
1. **id 体系**：**全部改成 int**（原选项 A）。已执行 —— 与配置对应的 **26 个字段**转 `int`（英雄 / 道具 / 收藏品 / 技能潜能 / 编队槽位 / 卡池奖励 / 奖励道具 / 升级消耗资源）；
   **服务端不透明标识**（订单 / 邮件 / 活动 / 任务 / 公告 / 卡池 / 商品 / 玩家）与**美术资源名**（`IllustrationId` / `AttributeIconId` / `QualityId` —— 配置给的就是名字，如 `QualityIcon_3`）**保持 `string`**。
2. **元素顺序**：**按筛选按钮的排列，`1..6` = 光 / 水 / 土 / 风 / 火 / 暗**。已落地为 `ClientElements`（`Name` / `FromName` / `AllIds`）；`ClientHero.Element`(string) 改为 `ElementId`(int) + `ElementName` + `ElementIcon`。
3. **战斗力**：**配置里没有，先标待确认**。`ClientHero.CombatPower` 保持占位 `0`，代码、探针输出与文档均标注【战力待确认】；**不得据此设计数值玩法**，等公式（是否 `属性 × TCoefficient 的 *Fight 系数`）确认后再实现。

<details>
<summary>原始提问（留档）</summary>

1. **id 体系**：配置一律 **int**，客户端一律 **string**（英雄 / 形象 / 称号 / 徽章 / 道具 / 卡池奖励）。
2. **元素**：`Element` 是 **int 1..6**，编队页 7 个筛选按钮 = `全` + 6 个；`int ↔ 光/水/土/风/火/暗` 顺序未定义。
3. **战斗力来源**：`THero` **没有**战力字段；`TCoefficient` 有 `HPFight 0.5 / AttackFight 2.0 / DefenceFight 0.8 / SpeedFight 1.5`。

</details>

### 15.4 让 Play 直接用真实数据的落地顺序（建议）

1. **英雄图鉴接配置**（最直观、且不需要新接口）：`LocalClientDataService.GetHeroes()` 改为「**配置给静态字段**（名字/品质/元素/星级/图标名/立绘名）+ **本地模拟给玩家状态**（拥有、等级、战力）」；`ClientHero` 增量补上述字段；`HeroId` 暂用 `cfg.HeroId.ToString()`（真实值 `"13001"`）以**不改动现有 string 通道**。
2. **收藏品集合接口**：`IClientDataService` 增 `GetOwnedCollectionIds(kind)` / `GetEquippedCollectionId(kind)` / `TryEquipCollection(kind, id)`，列表用 `Config.GetCollections(kind)`；供 `ProfilePage` 后续列表收敛。
3. **道具/背包接 `TItem`**（9 行），同时按真实语义修正 `ClientItemType`（`Type=1` 是货币）。
4. **英雄详情接 `TSkill` / `TPotency`**（天赋/秘技/终结技）。

### 15.5 已知美术缺口（影响"按名加载立绘"）

配置给的是**资源名**（`HeroAvatar_13001` / `HeroPortrait_13001_c` / `QualityIcon_3` / `ElementIcon_4` / `Item_110001`）。
工程实测：`QualityIcon_*` 20 个、`ElementIcon_*` 10 个、`HeroPortrait_*` **8 个**（`Assets/Resources_HotUpdate/BNRes/Textures/` 下），**`HeroAvatar_*` 与 `CatapultIcon_*` 为 0 个**。
⇒ 立绘/头像按名加载目前**只能命中一部分**，其余会取不到图（不报错，显示为空/占位）。

### 15.6 道具 / 技能 / 天赋 / 随机英雄池（2026-09-20 已接入客户端）

已进入客户端模型与**只读**接口（`IClientConfigService`）：`GetItems` / `TryGetItem` / `TryGetSkill` / `TryGetPotency` /
`GetHeroSkills` / `GetHeroPotencies` / `GetRandomHeroPool`；探针一次读 **10 张表**
（`Hero` / `Head` / `HeadFrame` / `Badge` / `Nameplate` / `Title` / `Item` / `Skill` / `Potency` / `RandomHero`）。

| 表 | 行数 | 关联方式 | 实测状态 |
|---|---|---|---|
| `Item` | 9 | 直接按 `Id` | ✅ 已读入。`Type` **只出现 `1` 与 `3`**：`1` = **货币**（点数/源晶/晶核），`3` = **消耗品**（启迪之星、共鸣结晶系列）；**`2` 表内没有任何一行**，含义未定义 ⇒ 不推测 |
| `Skill` | 33 | `THero.Skill`（每英雄 **4** 个） | ✅ 可解析，但 ⚠️ **不是"每个英雄一套"**：54 个英雄的 `Skill` 只有 **4 种取值** —— 同一"系列"的英雄（**16 / 6 / 16 / 16** 个）**共用**同一套技能。被引用的 **16** 个技能 = 4 个系列 × `Type 1/2/3/4` **各一个**；**另有 17 个技能没有任何英雄引用**（`Type5`×16 + `Type6`×1 + `999999 占位`），描述全是"对敌人造成伤害"、名字是怪物名（火焰追击 / 毒牙毒液 / 蛮牛冲撞 / 九头齐噬…）⇒ **那批属怪物（战斗侧）技能**。`Type` 分布 `1:4 / 2:4 / 3:4 / 4:4 / 5:16 / 6:1` |
| `Potency` | 68 | `THero.Potency` | ⚠️ **只有 4 个英雄（= 每个系列的"首个英雄"）**的天赋数据是完整的：`13001 玉兔`(13) / `14001 嫦娥`(17) / `14007 唐三藏`(17) / `15001 孙悟空`(21)，id 属 `300101 / 400101 / 400701 / 500101` 系列。**其余 50 个英雄的值是占位 `[1,2]`**，而表里最小 id 是 `300101` ⇒ **一个都解析不到**（已确认按"空着、不报错"处理） |
| `PotencyLevel` | 3 | 天赋等级经验（`Id 1..3`） | 已读入（客户端暂无天赋等级玩法） |
| `RandomHero` | 4 | 条目 `Id` → `Heroid` | ✅ 已读入。⚠️ **表里没有权重 / 概率 / 价格**（旧生成类的 `Weight` 在新表中不存在、恒为 0），那些属服务端 |

**技能页签的冲突（未确认前不写映射、不当作规则）**

UI `ClientHeroDetailPage` 有 **3 个页签：天赋 / 秘技 / 终结技**，而配置给的是 **4 类英雄技能 + 1 个天赋列表**，**数量与形态都对不齐**：

| UI 页签 | 配置里对应的东西 | 形态 | 冲突 |
|---|---|---|---|
| 秘技 **+** 终结技 | `Type1` 主动攻击（捣药杵击 / 月轮飞击 / 禅杖普度 / 如意金箍棒）<br>`Type2` 回复·增益（玉兔灵药 / 月华祝福 / 佛法回复 / 战意昂扬）<br>`Type3` 被动·护体（月华追踪 / 广寒清辉 / 金蝉护体 / 金刚不坏）<br>`Type4` 大招（广寒月影 / 皓月当空 / 紧箍咒 / 法天象地） | **4 类**，每英雄各一个 | ❌ **4 类要塞进 2 个页签**：即便 `Type4` = 终结技，剩下 `Type1/2/3` **三类**也只能挤进「秘技」一个页签 |
| 天赋 | `TPotency`（如"生命+18%"） | **列表**，每英雄 13~21 条 | ❌ 页签的呈现形态（列表全列？只列已解锁？按等级？）**无依据** |

补充：`Type` **本身不区分英雄 / 怪物技能**（实测靠值域区分：`1~4` = 英雄、`5~6` = 怪物），
所以「Type → 页签」的映射**无法只从 `Type` 推出**，配置里也没有别的字段或文档说明。

### 15.7 技能 `Type` ↔ 页签：**已从数据推出**（2026-09-20，待产品最终确认）

原以为"4 类技能要塞进 2 个页签"，**实际不冲突** —— 页签只有 3 个（天赋 / 秘技 / 终结技），对应 **`Type2` / `Type3` / `Type4`**；**`Type1`（普攻）不在详情页展示**。

**证据链（两条独立证据互相印证）**

1. **天赋表（`Potency`）里点名提到某个技能的行** —— 行名自己就写着"秘技 / 终结技"：

| 天赋行 | 描述 | 提到的技能 | 该技能 `Type` |
|---|---|---|---|
| `300110 秘技强化` | 月华追踪弹50%概率触发冰冻 | 月华追踪 | **3** |
| `400115 秘技治疗` | 广寒清辉治疗量+50% | 广寒清辉 | **3** |
| `400715 秘技治疗` | 金蝉护体治疗量+50% | 金蝉护体 | **3** |
| `400716 紧箍觉醒` | **终结技**眩晕延长至2回合 | 紧箍咒 | **4** |
| `400117 皓月觉醒` | 皓月当空额外附加净化 | 皓月当空 | **4** |
| `500120 乱舞觉醒` | 法天象地乱舞次数+4 | 法天象地 | **4** |
| `500114 能量节约` | **终结技消耗**-15% | — | 4（无冷却 ⇒ 靠能量） |

2. **冷却 / 消耗特征**（`TSkill.Desc` 里的"冷却N回合"）：

| `Type` | 语义 | 冷却 | 四系列实例 |
|---|---|---|---|
| `1` | **普攻** | **无** | 捣药杵击 / 月轮飞击 / 禅杖普度 / 如意金箍棒 |
| `2` | **天赋（技能）** | **有** | 玉兔灵药 / 月华祝福 / 佛法回复 / 战意昂扬 |
| `3` | **秘技** | **有** | 月华追踪 / 广寒清辉 / 金蝉护体 / 金刚不坏 |
| `4` | **终结技** | **无**（耗能量） | 广寒月影 / 皓月当空 / 紧箍咒 / 法天象地 |

3. 天赋里 `300112 天赋冷却`（"**天赋技能**冷却-1回合"）与 `300113 秘技冷却`（"秘技技能冷却-1回合"）
   **正好分别对应 `Type2` 与 `Type3`**；`500114 能量节约`（"终结技**消耗**-15%"）对应 `Type4` 无冷却、耗能量。

**仍待产品确认的 1 点**：`TPotency`（每英雄 13~21 条被动加成）与 3 个页签的**归属规则** ——
是否"**描述里提到哪个技能就归哪个页签**"（弱关联，`300110` 就是靠这个归到秘技），
以及页签是"**主技能 + 强化列表**"还是"**只列强化**"（`300109 天赋强化` 提到的是"月华恩赐"，**在当前技能表里找不到同名技能**，疑似该英雄的天赋技能改过名）。

---

## 16. 数值与说明模块（`Stats`）—— 负责人 2026-09-20 定的架构原则

> 原话：**数据未来只由权威服务器计算并分发，客户端负责「属性接口」和「技能/天赋等数值接口」以及「逻辑说明显示」，
> 最好单独做一个模块来管理，要贴近 MVC，强解耦、拓展性强、维护方便、GC 消耗少 —— 这是所有代码和设计的原则。**

### 16.1 硬约束（由"服务器权威"推出，优先级最高）

1. **客户端不做任何数值计算**：战力、属性加成、技能效果值、冷却、概率、经验 …… 一律**取服务器下发的值**。
   接口签名里**不得出现"计算结果"形态的方法**（如 `CalcCombatPower()`），只允许 `Get*` 查询服务器已算好的值。
2. **配置表在客户端的用途只有两个**：① 展示用的**名称 / 图标 / 说明文本**；② **结构定义**（有哪些技能/天赋、归属哪个页签）。
   **配置表不作为数值来源**（表里的 `Effect1`、`EffectParameters`、`Hero.HpBase` 等是**服务端计算输入**，客户端不解析、不套公式）。
3. **本地模拟层只扮演服务器**：`LocalClientDataService` 产生"权威快照"，**不得被 View 直接依赖**；接服务端时整体替换，View 一行不改。
4. 由此，`MODULE.md` §15.3 的**"战斗力来源"待确认项结案**：**战力由服务器下发**，客户端不自算（占位 `0` 在接入服务器后由下发值替换）。

### 16.2 模块落点与目录

模块根：**`Assets/Client/Runtime/Stats/`**（自带 `MODULE.md`；名字可按负责人偏好改，职责不变）

```
Stats/
├── MODULE.md
├── Model/                    纯 C#，零 UnityEngine 依赖（可单测），只有数据
│   ├── ClientAttributeIds.cs       属性 id 定义（**数据驱动**，不用 enum 硬编码，避免每加一条属性就改代码）
│   ├── ClientAttributeValue.cs     一条属性：{ int Id; int Value; }
│   ├── ClientAbilityKind.cs        Skill / Potency
│   ├── ClientHeroAbilitySection.cs 天赋 / 秘技 / 终结技
│   ├── ClientAbilityEntry.cs       一条能力（结构 + 服务器数值 + 显示字段）
│   ├── ClientAbilitySection.cs     一个页签（主技能 + 强化列表）
│   └── ClientHeroAbilitySet.cs     一个英雄的完整能力（页面唯一入口）
├── Gateway/                  服务端契约层（唯一"入口"）
│   └── IClientStatsGateway.cs      Snapshot / Delta 拉取与订阅（未接入前由本地模拟实现）
├── Service/                  Controller + 缓存（权威数据的唯一持有者）
│   ├── IClientStatsService.cs
│   └── ClientStatsService.cs       原地更新缓存 + 广播变更 + 零分配查询
├── Format/                   说明文本（"逻辑说明显示"的唯一落点）
│   ├── IClientDescriptionFormatter.cs
│   └── ClientDescriptionFormatter.cs
└── View/                     Unity 显示组件（只写控件）
    ├── ClientAttributeRowView.cs
    ├── ClientAbilitySectionView.cs
    └── ClientAbilityEntryView.cs
```

### 16.3 MVC 映射与依赖方向（强解耦的关键）

| 角色 | 由谁承担 | 铁律 |
|---|---|---|
| **Model** | `Model/` + `Service/` 里的缓存 | 纯数据 + 服务器值；**零游戏逻辑、零公式** |
| **View** | `View/` 下的 Unity 组件 | **只写控件**：不查数据、不算数、不读表、不找节点；数据由外部 `Bind(...)` 推入 |
| **Controller** | 页面脚本（如 `ClientHeroDetailPage`） | 从 `Service` 取数据 → 推给 `View`；把 View 的输入事件上报；**单向数据流** |

**依赖方向**（违反即为缺陷）：

```
Gateway → Service → (Controller) → View
Config  ↗                                    ↘ 只读 Model
```
- `View` **不引用** `Service`/`Gateway`/配置表 ⇒ 可单独预览、可复用、可换皮。
- `Service` **不引用**任何 Unity UI 类型。
- **配置表只被 `Service`/`Format` 读取**，页面与 View 都不碰表。

### 16.4 接口草案（以"服务器给值"为前提）

```csharp
public interface IClientStatsService
{
    // —— 属性接口 ——
    bool TryGetAttribute(int ownerId, int attributeId, out int value);
    IReadOnlyList<ClientAttributeValue> GetAttributes(int ownerId);   // 顺序由服务器/展示表给

    // —— 技能 / 天赋数值接口 ——
    bool TryGetAbilityState(int abilityId, out ClientAbilityState state);  // 等级/解锁/冷却，全部服务器值
    ClientHeroAbilitySet GetHeroAbilities(int heroId);                     // 结构 + 数值 + 说明（页面唯一入口）

    // —— 变更通知（事件驱动，禁止 Update 轮询）——
    event Action<int> AttributesChanged;   // 参数：ownerId
    event Action<int> AbilitiesChanged;    // 参数：heroId
}

public interface IClientDescriptionFormatter
{
    /// <summary>把"描述模板 + 当前数值"渲染成玩家可见文本；同一 (id, level) 结果缓存，不重复拼接。</summary>
    string FormatSkill(int skillId, int level);
    bool TryFormatSkill(int skillId, int level, StringBuilder buffer);   // 零分配路径，供高频刷新用
}
```

### 16.5 GC 消耗少 —— 必须遵守的写法清单

1. **DTO 只建一次、原地更新**：快照变化时改 `List<T>[i]` 的字段，**不 new 新列表**；列表容量一次预留。
2. **查询返回 `IReadOnlyList<T>`，调用方用 `for` + 索引**：不用 `foreach`（接口枚举器会分配）、**不用 LINQ**（每条链都分配）。
3. **不产生临时字符串**：数值转文本走**复用的 `StringBuilder` / `int.TryFormat(Span<char>)`**（.NET Standard 2.1 可用，零分配）；
   **禁止** `Update` 里 `"LV." + level` 这种拼接。
4. **值没变就不写控件**：`TMP_Text.text` 的 setter 会触发重建，先比较再赋值。
5. **事件用缓存委托**：`_onChanged` 字段 + 方法组订阅，避免闭包；`OnDestroy` 里 `-=`。
6. **不轮询**：所有刷新由 `AttributesChanged` / `AbilitiesChanged` 驱动，`Update` 内零业务查找。
7. **属性存储按 id 密度选型**：连续用 `int[]`，稀疏才用 `Dictionary`；**不用 `enum → object` 装箱**。
8. **避免 `params` 与临时数组**：用固定参数重载，不用 `params object[]`。
9. **View 不做字符串格式化**：文本一律由 `Format/` 产出后传入。

### 16.6 拓展性 / 维护性

- **新增一条属性**：只改服务器下发的 id + 展示表 ⇒ **客户端零代码改动**（所以属性 id 用 int 常量而非 enum）。
- **技能 Type → 页签 规则变化**：只改 `SectionRules` 一处（§15.7 的映射表），View 与页面不动。
- **换数据来源**（本地模拟 → 真实服务器）：只换 `IClientStatsGateway` 实现。
- **换 UI**：只改 `View/`，`Model`/`Service` 不动。

**待确认**：模块名（`Stats` / `GameData` / 其它）、属性 id 与展示顺序的**权威来源**（服务器下发还是客户端展示表）。


---

## 17. ProfilePage个人中心 收藏品列表与主页展示（2026-09-20 收口）

### 17.1 数据分层（表驱动 + 玩家状态）

| 问题 | 来源 | 接口 |
|---|---|---|
| 游戏里**有哪些**头像 / 徽章 / 铭牌 / 头像框 / 称号 | **配置表**（`ClientShow` 过滤 + `Sorting` 排序） | `IClientConfigService.GetCollections(kind)` |
| **我拥有哪些** / **正在用哪个** | **玩家状态**（服务端；未接入前本地模拟） | `GetOwnedCollectionIds(kind)` / `GetEquippedCollectionId(kind)` / `TryEquipCollection(kind,id)` |
| 主页展示的是哪位英雄 | 玩家状态 | `GetShowcaseHeroId()` / `TrySetShowcaseHero(id)` |

未拥有的条目**不是隐藏**，而是显示条目自带的"未解锁"子节点 —— 对应表字段 `NotUnlockedClientShow`（未解锁是否占位）。

### 17.2 实现范式（与英雄卡完全相同）

- `ClientCollectionItemView`：**只写控件**（图标 / 未解锁子节点 / 使用中标记 / 点击），值没变不写控件。
- `ClientCollectionList`：**1 模板 + N 实例**，多余实例隐藏而非销毁；与 `ClientHeroCardList` 同一套范式，且都不依赖领域模型（用委托映射）。
- 主页展示走 `ClientHeroCardList`（与 HeroPage / 编队选卡共用卡牌逻辑）。
- 5 个列表的模板与容器由**结构修复工具**接线：`_avatarTemplate/_avatarRoot`、`_badgeTemplate/_badgeRoot`、`_nameplateTemplate/_nameplateRoot`、`_frameTemplate/_frameRoot`、`_titleTemplate/_titleRoot`、`_showcaseTemplate/_showcaseRoot`。

### 17.3 两个实测命名陷阱（改列表前必读）

1. **"未解锁"项常排在第一位**：如 `徽章未解锁Image (3)`。工具默认取 `items[0]` 当模板会**拿错** ⇒ 必须用
   `ListTarget.TemplatePrefix` 指定"已解锁"那项（现在传 `徽章已解锁` 等）。
2. **名字大小写与空格都不统一**：`徽章已解锁image` / `小铭牌已解锁image` / `头像已解锁image` 是**小写 i**；
   `称号已解锁-无image` 带 `-无`；节点 `'Panel_Personalize  '` **尾部有 2 个空格**（Unity 用单引号包裹这类名字）。
   ⇒ 路径匹配用 `FindByPathTrimmed` 兜底，字符串不要手写尾空格。

### 17.4 BUG-018（玩家名 / 玩家ID / 战力）

- 三者场景内都是 **`TMP_Text`**（guid `f4688fdb…`），而原代码把它们声明为旧版 `UnityEngine.UI.Text` ——
  违反 `AGENTS.md` §4，也是"三个字段一直没接线"的原因。**已改为 `TMP_Text` 并接线**：
  · `Panel_Profile/底框/玩家名称Image/Text (TMP)`
  · `Panel_Profile/底框/玩家IDImage/Text (TMP)`
  · `Panel_Profile/MiddleImage/Panel1/战力底框/Text (TMP)`

### 17.5 待确认 / 未接

- **收藏品图标未解析**：`ResId`（整型资源 id）→ `Sprite` 的映射规则**未定义**（与 `BUG-019` 同类），
  `Head` 用的 `HeroAvatar_*` 贴图工程内 **0 个** ⇒ 当前保持模板图。需要负责人给规则或指定可用贴图。

  > **2026-09-20 更正（第 62 轮）**：映射规则**已经找到，不需要负责人提供** —— `Resources` 表（256 行）的 `Id → SkeletonData`
  > 就是「资源 id → 资源名」映射，收藏品 `ResId` 正是 `Resources.Id`（`Title` 51001→`Title_51001`、`HeadFrame` 52001→`AvatarFrame_52001`、
  > `Nameplate` 53001→`Nameplate_53001`、`Badge` 54001→**`Medal_54001`**）。
  > 真实阻断是**美术缺资源**：`QualityIcon_*`(**5**：`_2`..`_6`) / `ElementIcon_*`(**5**：`_1`..`_5`，**元素 6（暗）缺**) / `HeroPortrait_*`(**4**：12001/13001/14001/15001) **有**；
  > `HeroAvatar_*` / `Title_*` / `AvatarFrame_*` / `Nameplate_*` / `Medal_*` / `Item_*` / `CatapultIcon_*` / `AttributeIcon_*` **没有**。
  > （数量于第 65 轮用计数命令实测更正：此前写的 20/10/8 是错的。）

- **主页展示的槽位数**：现按"展示 1 位英雄"实现（数据层只有 `GetShowcaseHeroId`）；若产品要求多位展示需另定契约。
- `SystemLayer` 分层定案仍待负责人选 A/B/C（见 `BUG_TRACKER.md` RISK-020）。

---

## 18. UI ↔ 代码「节点契约」核对法（2026-09-20 新增，建议每页沿用）

页面按**名字**在运行时解析场景节点时，**代码里的字面量就是一份隐式契约**；场景改名/漏建不会报错，只会静默失效。
核对方法（纯静态、可脚本化，已用它验证 `ActivityPage`）：

1. 从页面代码里抽出所有"按名查找"的字面量，例如
   `Select-String -Pattern 'FindDirectChild\(|FindDeepChild\(|GetChild\('`，再从每行中提取 `"..."` 的内容；
2. 用场景解析脚本取目标页面子树内的**全部节点名**（注意非 ASCII 名在 YAML 里以 `\uXXXX` 转义存储，必须先反转义）；
3. 逐个比对：**精确名 / 仅 Trim 后相等 / 缺失** 三态，并检查数量是否自洽
   （例：12 张任务卡 ⇒ `任务进度条Canvas` 应恰好 12 个；60 个奖励位 ⇒ `道具卡面` 应为 60 个）。

**ActivityPage 实测（第 64 轮）**：32/32 期望节点名**精确命中**（含双空格的 `Newbie Task  Button`），数量自洽
⇒ 该页 **UI 与代码完全对齐**；此前"只有结构 / 待实现"的记录**过时**。其数据也真实：读
`ClientServices.Data.GetActivityTasks(category)`，本地模拟提供新手 5 条 + 进阶 5 条 + 6 个赛季活动。

**已知盲区**：**prefab 实例根节点的名字**存在 `PrefabInstance` 的 `m_Modifications`（`propertyPath: m_Name`）里，
**不在 GameObject 文档中**，上述脚本扫不到它 —— 判定实例类节点要另查 `PrefabInstance` 块。

**入口可达性核对**：先 `grep` 导航绑定名（如 `ClientHomePage.BindPrefabNavigation("ActivityButton", …)`），
再到场景确认该名字的节点是否存在。实测：`ActivityButton` = 1 ✓ 可达；`TaskButton` / `NoticeButton` = 0
⇒ **任务页与公告页在场景里没有入口**（与"缺失页面显示开发中"一致）。

### 18.1 系统化审计脚本（2026-09-21 新增，可重跑）

`Logs/ui-contract-audit.ps1`（**纯 ASCII**，避免 `.ps1` 被 PowerShell 按 ANSI 读坏）→ 报告 `Logs/ui-contract-audit.md`。

两条判据：
- **A 类**：代码里"按名查找"的字面量，在**场景 + 全部 prefab 的节点名集合**（实测 920 个名字）里都不存在 ⇒ 候选缺口；
- **B 类**：场景里客户端组件的**序列化字段为 `fileID 0`**（未接线）⇒ 候选缺口。

**判读须知（避免误报/漏报）**：
- **必须把 prefab 资产一起算进名字集合**：场景 YAML 里看不到 prefab 实例**内部**的节点（只有实例根名）——
  只查场景会把"其实存在于 prefab 里"的节点误判为缺失。
- **反向的坑**：**prefab 实例根节点的名字**存在 `PrefabInstance.m_Modifications`（`propertyPath: m_Name`）里，**不在 GameObject 文档中**；
  只按 GameObject 扫会漏掉它们（脚本已额外处理）。
- 常见**假阳性**：编辑器工具里的路径片段、`"Material " + i` 这类**前缀**字面量、以及**代码自己创建**的节点名（脚本已用 `name = "…"` 排除）。
- **B 类不等于缺陷**：`Assemblies` 常见的"属性回退 `gameObject`"（如 `ClientUiPopup.Root`）、
  `GetComponentInChildren` 兜底（`ClientShellController`）、代码注释明确标注**可选**的字段（`ClientHeroPage._heroTabSelected`）
  都属于**正常**；分类结论见 `BUG_TRACKER.md` **BUG-026**。

### 18.2 类型一致性审计（`Logs/ui-type-audit.ps1`，2026-09-21 新增）

检查**字段声明类型**与**它引用的场景组件实际类型**是否一致 —— 这是 `AGENTS.md` §4 的硬要求，
不一致会让引用**静默失效**（**`BUG-018` 的成因**：`ClientProfilePage` 把 TMP 文本声明成了旧版 `Text`）。

判据：解析每个客户端类的序列化字段类型（`Text` / `TMP_Text` / `Image` …），再解析场景里该字段指向的组件 guid
（`f4688f…` = TMP、`5f7201…` = 旧版 UGUI Text、`fe87c0…` = Image、`4e29b1…` = Button）后比对。
报告 `Logs/ui-type-audit.md`。

**实测（2026-09-21）**：**127 处引用、0 处不符** —— 说明"文本类型声明与场景不符"这一类**只出现过 `BUG-018` 一次、且已修复**。
**阳性对照**（确认审计不是"空转"）：同一次运行解析出的被引用组件种类包含 `class1`(GameObject) 49、**`TMP_Text` 22**、`Transform` 17、`Image` 17、`Button` 12、**`UGUI_Text` 8**
⇒ 两类文本都在场，若真有错配必然会被捕获。

### 18.3 死按钮审计（`Logs/ui-dead-buttons.ps1`，2026-09-21 新增）

**判据**：场景里的 `Button` 若 **既没有场景持久事件**（`m_MethodName`），**其名字又没有出现在任何运行时代码的绑定/查找行里** ⇒ 列为"候选死按钮"。
报告 `Logs/ui-dead-buttons.md`。**实测：169 个按钮、0 个走场景持久事件**（按钮一律由代码绑定，符合本工程架构）。

**判读须知（否则会误判）**：
- **看不到"用序列化字段接的按钮"** ⇒ `_leftButton`/`_rightButton`/`_equipmentButtons`（英雄详情）、`_heroTabButton`/`_guideTabButton`（英雄页）、
  `_addButton`/`_subtractButton`/`_maxButton`（购买数量条）等会**误报**；
- 绑定时**必须用深搜**：`ClientProfilePage.BindButton` 原先用 `transform.Find`（只查直接子级），
  而 `改名称` 在 `Panel_Profile/底框/` 下、改名弹窗的确认/取消在 `Panel_Profile/RenameDialog/` 下 ⇒ **静默找不到、点了没反应**。已改深搜 ✓
- **未实现的功能**（如邮箱的 `All delete`、元素筛选按钮、未接入页面内的按钮）**不是缺陷**，应记为"未实现"而不是"坏按钮"。

### 18.4 分层与列表排版审计（`Logs/ui-layout-audit.ps1`，2026-09-21 新增）

**问题**：页面根挂在哪一层、是否全屏、列表容器有没有布局组件 —— 这三件事决定**弹窗遮罩是否合理**、**克隆出来的条目会不会叠在同一坐标**。

**判据**：① 读 `ClientCanvas` 的直接子节点与激活态；② 用结构修复工具的 `LayerMove` 表当"期望层"，与场景**实际父层**对照，并读页面根的 `anchorMin/anchorMax` 判断是否全屏；③ 遍历所有名为 `Content` 的节点，检查是否含 `GridLayoutGroup`/`VerticalLayoutGroup`/`HorizontalLayoutGroup` 与 `ContentSizeFitter`。

**实测（2026-09-21）**：`PopupLayer[1]`/`PagesLayer[1]`/**`SystemLayer[0]`**/`ToastRoot[0]`；
**19/19 已登记节点都在 `PopupLayer`**；**12 个页面根全屏、7 个面板型**（面板型不拉伸是**设计意图**，非缺陷）；
**31 个列表容器全部具备 LayoutGroup + ContentSizeFitter**（0 个缺）⇒ **无异常**。
唯一仍然偏差的是 **`SystemLayer` 常驻 inactive**（见 `RISK-020`，待负责人选 A/B/C）。

### 18.5 射线可达性审计（`Logs/ui-raycast-audit.ps1`，2026-09-21 新增）

**问题**：按钮"看着在、点不到"的**第二种机制** —— 它的图不接收射线（`m_RaycastTarget=0`），或祖先 `CanvasGroup` 关掉了 `blocksRaycasts`。
（**第一种**机制是"没人绑定处理函数"，见 §18.3。）

**判据**：按钮可点 ⇔ 其**场景可见子树**里至少有一个 `m_RaycastTarget = 1` 的 Graphic
（GraphicRaycaster 命中后向**最近的 Button 祖先**冒泡）；若任一祖先 `CanvasGroup.m_BlocksRaycasts = 0` 则直接不可点。
`CanvasGroup` **按字段特征识别**（不依赖 guid —— 它来自内置模块，不在 `Library/PackageCache` 里）。

**实测（2026-09-21）**：**169 个按钮 → 4 个候选，且 4 个候选的路径全为空** ⇒ 它们正是 **prefab 实例根**
（实例根的 Transform 在场景 YAML 里是 **stripped** 的，静态解析取不到路径）⇒ 属**已知假阳性**，**无真缺口**；
其余 165 个按钮的子树里都有可接收射线的图 ✓

**判读须知**：与 §18.3 相同的盲区 —— **prefab 实例内部**的节点不在场景 YAML 中，凡涉及实例内部的判断都要人工复核；
另外**"0 发现"必须先确认工具真的在工作**（本次曾因"6 位 guid 前缀 vs 32 位全 guid"导致 `buttons = 0`，靠"按钮数与 169 矛盾"才发现）。

---

## 19. 客户端纯逻辑自检套件（`ClientSelfTest`，2026-09-20 新增）

### 19.1 为什么不是 Unity Test Framework

`Assets/Client` **没有 asmdef** ⇒ 其代码位于**预定义程序集 `Assembly-CSharp`**；
而 **asmdef 程序集无法引用预定义程序集** ⇒ 用 Unity Test Framework 建的测试程序集**看不到 `Pinball.Client` 类型**。
把客户端整体改成 asmdef 属**架构变更**（需负责人授权），因此当前采用：

> **Editor 自检套件 + 命令行退出码** —— 任一断言失败即以**非 0** 结束，可直接当构建前门禁 / CI 用。

```powershell
$env:ALLUSERSPROFILE = 'C:\ProgramData'
& '<Unity>\Unity.exe' -batchmode -quit -projectPath '<工程>' `
  -executeMethod 'Pinball.Client.Editor.ClientSelfTest.RunAll' `
  -logFile '<工程>\Logs\self-test.log'
# 退出码 0 = 全部通过；非 0 = 有断言失败（失败项同时以 LogError 打印）
```

### 19.2 覆盖内容（70 项断言）

| 分组 | 断言要点 |
|---|---|
| 表加载 | 服务/配置就绪、英雄可见数 = 49、道具表 = 9 |
| 元素映射 | `1..6` ↔ 光/水/土/风/火/暗 双向、越界不抛异常 |
| **道具类型映射** | 表 `Type` 只允许 `1`/`3`；**背包项 `ItemType` 必须等于表 `Type` 的映射**；并**真的走一遍"一键领取"**再校验（`BUG-023` ② 的回归防线） |
| 说明文本格式化 | 等级 1→`8%`、15→`9%`、20→`80%`、无分档文本原样返回 |
| 能力页签归属 | 页签名 = 天赋/秘技/终结技；主技能名 = 玉兔灵药/月华追踪/广寒月影；强化条数 = 12/1/0；**守恒式**：各页签条数之和 = 该英雄天赋表行数（13）⇒ 无漏归属 |
| 收藏品可见数 | Head `1` / Badge `1` / Nameplate `1` / HeadFrame `3` / Title `1`（= ProfilePage 的验收基准） |
| **远端 pkg 读取链** | **自建**一个"只有 3 行 Hero"的 pkg → 断言读表路径变成远端包且可见英雄 = 1 → **自删并复位**到本地表 |

### 19.3 纪律与安全

- **绝不覆盖负责人数据**：若 `persistentDataPath/config.pkg` **已存在**，该项自检**直接跳过**（只打一行说明）。
- 自检**只在 Editor 程序集**，不进 WebGL 包；写 pkg 用的 `System.IO.Compression` 也仅出现在这里
  （**运行时**那条链已按第 58 轮改用工程同款 `Unity.IO.Compression`，两者不要混淆）。
- 首次运行曾 **57 通过 / 6 失败** —— 6 项全部是**我写的期望值错了**（把 `DisplayName` 当成主技能名、把英雄天赋表总行数当成各页签条数）。
  修正后 **70 通过 / 0 失败**。**这正是自检的价值：它先抓住了写测试的人。**
