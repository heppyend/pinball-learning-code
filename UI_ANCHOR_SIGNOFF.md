# UI 锚点定稿确认表（所见即所得 · 可直接微调）

> **用途**：负责人按这张表逐页微调视觉（位置/大小/间距/字号），**改完就是终态**，不会因为锚点没定好而在别的分辨率上跑位。
> **依据**：`Logs/ui-adapt-audit.ps1`（21 页 × 布局规则）与 `Logs/ui-adapt-audit2.ps1`（15 个标题/顶区节点）两批只读核查，2026-09-21。
> **图例**：✅ 锚点已定稿，可安全微调 · ➖ 未接入页面（本阶段不做，不用管）· ⚠️ 有注意事项（见下）

## 一、逐页定稿状态

| # | 界面 | 根/背景 | 底栏 | 标题/顶区 | 列表容器 | 结论 |
|---|---|---|---|---|---|---|
| 1 | `MainPage主页`（大厅） | ✅ 全屏 | n/a | ✅ | ✅ | **✅ 可微调** |
| 2 | `ProfilePage个人中心` | ✅ 全屏 | ✅ | ✅ | ✅* | **✅ 可微调** |
| 3 | `HeroPage英雄` | ✅ 全屏 | ✅ | ✅ | ✅ | **✅ 可微调** |
| 4 | `HeroDetailPage英雄详情主页` | ✅ 全屏 | ✅ | ✅ | ✅ | **✅ 可微调** |
| 5 | `FormationPage编队` | ✅ 全屏 | ✅ | ✅ | ✅* | **✅ 可微调** |
| 6 | `ShopPage商店` | ✅ 全屏 | ✅ 铺满 | ✅ 标题贴顶（曾错挂底栏，已换父级） | ✅ | **✅ 可微调** |
| 7 | `RankPage排行榜` | ✅ 全屏 | ✅ 铺满 | ✅ | ✅* | **✅ 可微调** |
| 8 | `MailPage邮件` | ✅ 全屏 | ✅ | ✅ 贴顶 | ✅* | **✅ 可微调** |
| 9 | `ActivityPage活动` | ✅ 全屏 | ✅ 铺满 | ✅ 贴顶 | ✅* | **✅ 可微调** |
| 10 | `Complete Guide Event Page全图鉴活动` | ✅ 全屏 | ✅ | ✅ 贴顶 | ✅ | **✅ 可微调** |
| 11 | `Product Purchase Interface`（购买界面） | ✅ 面板式（豁免不全屏） | n/a | ✅ | ✅ | **✅ 可微调** |
| 12 | `Purchase Success Page`（购买成功） | ✅ 面板式（**本身就是那块面板**，别全屏化） | n/a | n/a | n/a | **✅ 可微调** |
| 13 | `Exchange code interface`（兑换码） | ✅ 面板式 | n/a | ✅ | ✅ | **✅ 可微调** |
| 14 | `Historical Order Interface`（历史订单） | ✅ 面板式 | n/a | ✅ | ✅ | **✅ 可微调** |
| 15 | `Pending shipmentCanvas`（待发货） | ✅ 面板式 | ✅ **本轮已修为铺满** | ✅ | ✅ | **✅ 可微调** |
| 16 | `Player lineup`（玩家阵容） | ✅ 面板式 | n/a | n/a | ✅ | **✅ 可微调** |
| 17 | `联系客服Page` | ✅ 面板式 | n/a | ✅ | n/a | **✅ 可微调** |
| 18 | `弹窗遮罩`（本轮新建，运行时显隐） | ✅ 全屏 | n/a | n/a | n/a | **✅ 可微调**（尺寸别改） |
| 19 | `Email Details Page（Have）` | ✅ 全屏 | — | — | — | **➖ 未接入**（无运行时代码） |
| 20 | `Email Details Page (No)` | ✅ 全屏 | — | — | — | **➖ 未接入** |
| 21 | `Success Receipt Interface` | ✅ 全屏 | — | — | — | **➖ 未接入**（且不可达） |

\* 列表容器在**场景里**是 `ContentSizeFitter = Unconstrained`，但**运行时**已由 `ClientScrollFix` 改为 `PreferredSize`（+ `Clamped` + 顶部对齐）⇒ 见下方第 2 条注意事项。

**小计：19 页可微调 ✅ / 2 页未接入 ➖**（另 1 页 `Success Receipt` 不可达）

## 二、微调注意事项（4 条，避免白改）

1. **面板式弹窗不要改成全屏**（第 11~17 行）—— 这是负责人确认过的教训：`Purchase Success Page` **本身就是那块居中面板**，铺满整屏会盖住身后的购买界面与商店。
2. **列表容器（`Content`）的高度由运行时撑开**：`ContentSizeFitter.verticalFit` 在运行时被设为 `PreferredSize` ⇒ 你手改的容器高度会被覆盖。**微调 `GridLayoutGroup` 的 `CellSize` / `Spacing` / `Padding` 是安全的**（撑高会跟着算）。
3. **改锚点务必保持"全屏拉伸"**（根/背景 `anchorMin=(0,0)`、`anchorMax=(1,1)`）；`弹窗遮罩`（第 18 行）**只改颜色/透明度，别改尺寸**（它是输入阻断层，全屏是功能要求）。
4. **卡牌网格的 `childAlignment` 保持 `UpperLeft`** —— 否则只有 1 张卡时会跑到中间（负责人 2026-09-20 的反馈，已修）。

## 三、已定稿的锚点规则（这些不用再动）

| 规则 | 定稿值 |
|---|---|
| 画布缩放 | `matchWidthOrHeight = 0`（**按宽匹配**，参考宽 **753**）、`pixelPerfect = false` |
| 页面根 / 背景 | 全屏拉伸 `(0,0)-(1,1)` |
| 底栏 `Bottom page function bar` | **横向铺满**（左右 offset 归零，高度不变） |
| 标题 / 顶区 | **贴顶锚**（`anchorMin.y = anchorMax.y = 1`），且**不得挂在底栏下** |
| 竖向列表 | 容器撑高 + `Clamped` + 顶部对齐 + 打开即置顶（**只改竖直轴**） |
| 卡牌网格 | `childAlignment = UpperLeft` |
| 面板式弹窗 | **保持居中面板**，不全屏 |

## 三之二、负责人 2026-09-22 复验后手动微调（**以这些为准，勿被工具回退**）

英雄详情页这三处是负责人在 1080×1920 下自己调好的终态（第 1+2 批已验收通过）：

| 节点 | 锚点 | offset | 备注 |
|---|---|---|---|
| `HeroDetailPage英雄详情主页/后退Button` | `(0,0)` **左下锚** | `(60, 52)` | 原中锚会沉到屏幕外；**不要**改回贴顶 |
| `downCanvas/leftmiddleCanvas` | `(0.5,1)` 贴 downCanvas 顶、pivot `(0.5,1)` | `(231, 243)` | 不再压 `upCanvas/right` |
| `downCanvas/Background`（属性条） | `(0.5,0.5)` 中锚 | `(0, 287)` | 位于等级卡下方 |

> ⚠️ 由此新增的护栏（2026-09-22）：
> - `Logs/apply-adapt12-text.ps1` **检测到有编辑器在跑就拒绝写入**（exit 3），而且**不加 `-ForceLayout` 绝不碰位置值**
>   —— 起因：它曾在验收后把上面这三处回退成 09-21 的基线（已恢复并加锁）。
> - 审计 `Logs/ui-adapt-audit3.ps1` 只断言**锚点类型 / 是否贴边**，**不再断言具体 offset**，
>   避免把负责人的微调判成失败（本轮就误报过 3 条）。

## 三之三、排行榜页：**负责人自行调整**（2026-09-22，本人决定）

负责人决定这块自己调（"这块太难和你表达了，我还是自己调吧"），并宣布**全部界面 UI 验收算通过**。
下列数值是**当时场景里的实际值**，仅作记录，**不代表定稿**：

| 节点 | 锚点 | 当时的 offset | 说明 |
|---|---|---|---|
| `RankPage排行榜/Challange/Ballte Mall` | **中锚** `(0.5,0.5)` | `(-290.7, 709)` | 名字字面含斜杠，不是路径 |
| `RankPage排行榜/Explanation text frame` | **中锚** `(0.5,0.5)` | `(0, 386)` | |
| `RankPage排行榜/Challenge List` | **中锚** `(0.5,0.5)` | `(0, 0)` | |
| `RankPage排行榜/Battle Power Ranking` | **中锚** `(0.5,0.5)` | `(0, 0)` | |
| `RankPage排行榜/Challenge List/tips` | **中锚** `(0.5,0.5)` | `(-153, 426)` | |
| `RankPage排行榜/Battle Power Ranking/tips` | **中锚** `(0.5,0.5)` | `(-153, 464)` | |

> ⚠️ **两个 Viewport 的 `sizeDelta` 看起来被互换了**（作者手调时可能拿错）：
> - `Challenge List/rank/邮件Scroll View/Viewport` → `sizeDelta (0,0)` ✅ 正常
> - `Battle Power Ranking/rank/邮件Scroll View/Viewport` → `sizeDelta (-17,0)` ← 原属挑战榜的值
>
> 战力榜视口已是 `(0,0)-(1,1)` 锚点，所以**不再塌陷**（不会画到框外），只是右边窄 17 单位。
> 建议你肉眼对一下；要改就把战力榜那份改成 `(0,0)`。

> 🛑 **护栏（2026-09-22 追加）**：`Logs/apply-rank-text.ps1` 现在**默认拒绝写入**（exit 5），
> 必须显式加 `-OwnerApproved` 才生效 —— 因为它写的全是"我这版算出来的基线"，
> 会在你手调之后把它们拽回去（本会话已经发生过一次，抱歉）。
> `-DryRun` 仍然只读、可随时用来核对。
> 编辑器工具侧：`FixRankPageStableAnchors()` **只改"还是中锚"的节点**，已是贴顶锚的一律跳过 ⇒ 不会覆盖你的手调。

## 四、复现方式

```powershell
& 'D:\unity project\pinball\Logs\ui-adapt-audit.ps1'    # 21 页 × 布局规则
& 'D:\unity project\pinball\Logs\ui-adapt-audit2.ps1'   # 标题 / 顶区
# 报告：Logs/ui-adapt-audit.md 、Logs/ui-adapt-audit2.md
```

> 场景侧的机械性归一化由编辑器工具 `ClientShellStructuralRepair.RepairAll` 负责（幂等）：
> `NormalizeCanvasScaler` / `NormalizeAllPageBars` / `NormalizeShopBars` / `NormalizeTopAreaElements` / `NormalizePendingShipmentBar` / `FixHeroCardGridAlignment` / `FixSuccessPageRect`。
> 运行时修正由 `Assets/Client/Runtime/UI/ClientScrollFix.cs` 负责。
