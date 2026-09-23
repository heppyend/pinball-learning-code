# UI 适配问题清单（负责人 2026-09-21 复验反馈 · 1080×1920）

> 反馈分辨率：**1080×1920**（画布比例 1.778 ⇒ 画布高仅 **1339** 单位，而设计基准是 **1630** 单位 ⇒ **矮了 291 单位**）。
> 这解释了大面积问题：**按 1630 高度摆好的固定尺寸内容，在 1339 的画布上会溢出/被裁/挤压**。

## 系统性问题（先记住这条，下面每一项都受它影响）

| 画布 | 宽 | 高 |
|---|---|---|
| 设计基准 1080×2280 | 753 | 1590 |
| 1080×2160 | 753 | 1506 |
| **1080×1920** | 753 | **1339** |

⇒ **横向是稳定的（宽恒 753）**；**纵向全部依赖"贴顶/贴底/拉伸"锚点**。凡是**固定高度/固定位置**的纵向排版，在 1920 上必然出问题。

---

## 工作清单（按优先级）

### 🔴 P0：回归项（以前修好过，现在又坏了）

| # | 页面 | 现象 | 初步怀疑 |
|---|---|---|---|
| 1 | **英雄详情** | `downCanvas/Panel` 的 **3 个 toggle（天赋/秘技/终结技）没有正常显示** | 负责人注明"以前解决过" ⇒ 需先查**是否被后续工具步骤覆盖**（`ClientShellStructuralRepair` 的某步可能改了它们的显隐/父级/锚点），并加**自检断言**防再次回归 |

### 🟠 P1：英雄 / 英雄详情 / 编队

| # | 页面 | 现象 | 初步怀疑 |
|---|---|---|---|
| 2 | 英雄 | 中间 Scroll View 的 **英雄卡牌跑到左上角、与"弹珠"重叠** | `Content` 的锚点/位置不对（上一版滚动修复只改了竖直轴；这里可能是 **Content 的水平几何**或**父级 Viewport 偏移**） |
| 3 | 英雄详情 | ① 下方 `后退Button` **半边被遮挡** ② `downCanvas/leftmiddleCanvas` **上移挡住 `upCanvas/right`** | 纵向锚点：`downCanvas` 很可能是**中锚或固定位置** ⇒ 画布变矮时上移 ✗；`后退Button` 可能被父级裁切（Mask/Viewport 边界） |
| 4 | 编队 | 上方**被拉伸**；部分 UI 不在原位置 | 顶部容器锚点被拉伸（可能是 `NormalizeAllPageBars` 的全屏拉伸波及到内容层） |

### 🟡 P2：商店 / 排行榜 / 邮箱 / 活动

| # | 页面 | 现象 | 初步怀疑 |
|---|---|---|---|
| 5 | 商店 | 道具卡牌的 `Content` 有问题 | 同 #2（Content 几何） |
| 6 | 排行榜 | 上方 UI 错位；**两个 ranking 穿过下方 UI 底框** | `Scroll View` / `Content` 的高度超出其父容器（Viewport 没裁切 / 尺寸不对） |
| 7 | 邮箱 | Scroll View 里的邮件**跑到上面**；下方按钮**下移** | Content 顶部锚定 + 高度不当；底栏锚点 |
| 8 | 活动 / 全图鉴 | 页面整体 OK，但 `新手任务Scroll View` / `进阶任务Scroll View` **超出框** | Scroll View 或其 Viewport 高度超出父容器 |

### 🔵 P3：面板式弹窗偏大 / 个人中心

| # | 页面 | 现象 | 初步怀疑 |
|---|---|---|---|
| 9 | **个人中心** | **没有缩小，保持原大小（又高又宽）**；5 个子页面比例跟着坏 | 页面内容很可能是**按 1630 单位固定摆放**（未做纵向拉伸/贴顶贴底）⇒ 在 1339 画布上溢出 |
| 10 | 历史订单 / 待发货 | 1080×1920 **底部被拉伸** + 一些 UI 重叠 | 面板高度固定 + 底部锚点 |
| 11 | 联系客服 | 面板**偏大**，希望改小 | 面板尺寸固定（按 1630 设计）⇒ 需按画布高度约束 |

---

## 建议的分批执行方式（每批一轮）

**流程（沿用已验证过的那套）**：
1. **取证**：读出该页涉及节点的真实锚点/offset/尺寸（脚本或场景 YAML），**不猜**
2. **定位**：对照"现象→锚点嫌疑"表（见 `UI_LAYOUT_DEBUG_GUIDE.md`）
3. **最小改动**：只改与问题相关的轴；改锚点按世界坐标/等效 offset 回写**保证不跳位**
4. **工具化**：写进 `ClientShellStructuralRepair`（幂等），你点菜单即可应用
5. **验证**：审计脚本 + 编译三目标 + `ClientSelfTest`（并按需**加断言防回归**，例如 P0 的 3 个 toggle）
6. **你复验**：1080×1920 / 2160 / 2280 三个比例

**分批建议（每批 2~3 项，做完你验一批）**：

| 批次 | 内容 | 理由 |
|---|---|---|
| **第 1 批** | #1（英雄详情 3 个 toggle 回归）+ 加自检断言 | 回归项，且**能顺手把防回归机制补上** |
| **第 2 批** | #3（英雄详情后退键被遮 / leftmiddleCanvas 上移）+ #2（英雄卡牌重叠） | 同一页族，锚点问题同源 |
| **第 3 批** | #4（编队拉伸）+ #5（商店 Content） | 顶部容器 + 列表容器 |
| **第 4 批** | #6（排行榜穿框）+ #7（邮箱）+ #8（活动 Scroll View 超框） | 都是"Scroll View 超出父容器"同源 |
| **第 5 批** | #9（个人中心整体偏大）+ #10（历史订单/待发货）+ #11（联系客服面板偏大） | 都是"按 1630 固定尺寸、在 1339 画布上溢出" |

---

## 需要负责人确认的一点（会影响整体方案）

**个人中心"没有缩小、又高又宽"，以及多个面板偏大，根因都是"内容按 1630 高度设计"。** 有两个方向：

- **方向 A（逐页修锚点，推荐）**：让每页的**主内容容器**纵向做"贴顶+贴底"或"按高度缩放"，保留横向按宽匹配的稳定性 ⇒ 页面在任何比例都自适应，改动分散但每页可控。
- **方向 B（改画布匹配策略）**：把 `matchWidthOrHeight` 改成 **1（按高匹配）** ⇒ 画布**高恒 1630**、宽按比例变 ⇒ 纵向问题一次性消失，但**横向**会变成新的适配面（横向 753→可能在窄机上更窄）⇒ 等于把问题从纵向挪到横向，**不推荐**。

> 若你选 A，我按上面的分批表往下做；若你想先看效果，我可以**只做第 1 批**（P0 回归）给你验，确认流程与效果后再继续。

---

# 执行记录

## 第 1+2 批 —— ✅ 负责人 2026-09-22 复验通过（"修好了"）

验收证据：三目标编译 0 error · 静态审计 `PASS 37/0` · 自检断言静态镜像 `PASS 35/0` · 场景完整性 `OK` · 文本补丁幂等；
且补丁经过 **Unity 自身保存往返**（09:27 保存后 37/0 仍成立）⇒ 手写 YAML 补丁有效。
负责人随后**手动微调**了后退键 / 等级卡 / 属性条三处 offset，已记入 `UI_ANCHOR_SIGNOFF.md` §三之二。

> ⚠️ **本轮的一次事故与修复（诚实记录）**：验收后我重跑离线校验套件时，`apply-adapt12-text.ps1`
> 把上面三处**回退成了 09-21 的基线**（覆盖了负责人的微调）。已用采集到的值**恢复**，并加了两道锁：
> 检测到 `Unity` 进程就 `exit 3` 拒绝写入；位置类字段只在显式 `-ForceLayout` 时才改写。
> 审计脚本也改成**只断言锚点类型/是否贴边，不断言具体 offset**（原先误报了 3 条"失败"）。

## 第 4 批 #6 · 排行榜（2026-09-22）

负责人反馈：**上方 UI 错位**；**两个 ranking 穿过下方 UI 底框**。

### 取证（`Logs/rank-layout.ps1` 逐步手算 + `Logs/rank-resolve-check.ps1` 路径解析，非估算）

排行榜页里放了**两份**列表容器 —— `Challenge List`（挑战榜）与 `Battle Power Ranking`（战力榜），
**位置完全相同**（各自 `rank` 均 y `209.4~1062.9`；底栏 `Bottom page function bar` y `0~149`），
由底部 `Battle power` / `'challenge '` 两个按钮切换显隐。

| 结论 | 证据 |
|---|---|
| **两个症状都来自 Viewport/Content 配置不一致**（两份互为对照） | 挑战榜 Viewport `(0,0)-(1,1)`、sizeDelta `(-17,0)`（基本正确）；战力榜 Viewport **`(0,0)-(0,0)` + sizeDelta `(0,0)`** |
| **战力榜视口塌成 0×0 ⇒ `RectMask2D` 形同不存在** ⇒ 内容不被裁切、按自家位置画到框外 | `tr=2137436814` 实读 |
| **两份 Content 都是中锚 + offset x 375.6 / 383.2，而 pivot 是 `(0,1)`** ⇒ 配合运行时把 y 改成贴顶后横向被推出去 | `tr=8650495860089157156` / `tr=1573695479` 实读 |
| **`GridLayoutGroup.childAlignment` 都是 `MiddleCenter(4)`**（项目规则应为 `UpperLeft`） | 同一批实读；`FixHeroCardGridAlignment` 只覆盖英雄页，**没覆盖排行榜** |

### 修法（幂等，已写进 `ClientShellStructuralRepair.FixRankPageLists()` + 菜单 `8. 排行榜列表修复`）

| 节点 | 改动 |
|---|---|
| 两个 `.../邮件Scroll View/Viewport` | 贴满 Scroll View（`(0,0)-(1,1)`、offset 归零、pivot `(0,1)`）⇒ **遮罩恢复面积** |
| 两个 `.../Viewport/Content` | 锚 `(0,1)`、pivot `(0,1)`、offset 归零 ⇒ 列从视口左上开始 |
| 两个 `Content` 的 `GridLayoutGroup` | `childAlignment → UpperLeft(0)` |

**不动**尺寸、行高、底栏、tips。

### 状态：✅ 已修复并验证（2026-09-22）

| 验证 | 结果 |
|---|---|
| `Logs/rank-resolve-check.ps1` | ✅ **RANK CHECK: OK**（两个 Viewport 均贴满、两份 Content 均左上锚、网格均 UpperLeft） |
| 全场景布局体检 `Logs/rank-layout-check.ps1` | 16 份竖向列表**只剩 1 份不合格**，且是**商店页**（#5，不属本批） |
| `Logs/ui-adapt-audit3.ps1` | ✅ PASS 37 / FAIL 0 |
| `Logs/selftest-scene-mirror.ps1` | ✅ PASS 35 / FAIL 0 |
| 三目标编译 | ✅ 0 error |

**落地方式（本轮最耗时的部分，教训已记入 `UI_LAYOUT_DEBUG_GUIDE.md` §7）**：
编辑器那个进程**一直没重新编译**，所以点菜单 8 多次都只跑了旧代码（Content 变了、Viewport 没变）。
先后试过"再点菜单 / 加 `AssetDatabase.Refresh()`"，都不可靠（后者还会引发域重载打断执行）。
**最终靠两条互补手段落地**：
1. **批处理**（`Logs/repair-rank-batch.ps1`，一定从源码全新编译）把主修复写进场景；
2. **文本补丁** `Logs/apply-rank-text.ps1` 补上唯一漏项（挑战榜 Viewport 的 `sizeDelta` −17→0），
   跑完立刻断言：`patched 1 field(s)`，复验 `RANK CHECK: OK`。

> 排查期的一个坑：场景文件 mtime 曾被读到 09:39:11 而实际已被写入 —— **判断"工具到底改没改"要以
> "重新解析文件内容"为准**（本仓库的 `rank-resolve-check.ps1` / `rank-layout-check.ps1` 都是这么做的），
> 不要只看 mtime。

### 顺带查到的线索：另外两页也有同样的"中锚 + pivot(0,1)"组合（**改了排行榜之后要盯这两页**）

`Logs/scrollfix-blast-radius.ps1` 扫了全场景 **30 份**竖向滚动列表，只有 **4 份**是"固定宽度 + 中锚 + pivot(0,1)"这个
会横向位移的组合：

| 页面 | 列表 | Content 现状 | 处置 |
|---|---|---|---|
| 排行榜 | `Battle Power Ranking/rank/邮件Scroll View` | 中锚 pos `(383.2, 426.8)` size `(-750.7, 283)` | ✅ 本批已修 |
| 排行榜 | `Challenge List/rank/邮件Scroll View` | 中锚 pos `(375.6, 426.8)` size `(-743.1, 283)` | ✅ 本批已修 |
| **邮箱** | `MailPage邮件/Email come/邮件Scroll View` | 中锚 pos `(922, 448.1)` size `(-1861, 283)` | ⚠️ **未动**，去了第 4 批 #7 一并看 |
| **商店** | `ShopPage商店/Product page/展示卡牌Scroll View` | 中锚 pos `(-256, 564.3)` size `(522, 33)` | ⚠️ **未动**，去了第 4 批 #5 一并看 |

> 为什么不在运行时统一"搬正"：这三份的锚点/offset 组合虽然语义可疑（锚点与轴心不一致），
> 但在 1080×1920 下**可能本来就摆对了**——排行榜那份是**有负责人截图证据**才敢改的。
> 因此 `ClientScrollFix` 运行时只做"轴心与锚点语义对齐"这条**不产生位移**的修正，
> **位置一律交给场景侧工具逐页确认**（避免又造一次"改了别页"的事故）。

### 布局级体检（`Logs/rank-layout-check.ps1`，比"锚点断言"更接近现象）

不看锚点、只看**结果**：竖向列表的 Viewport 必须有真实面积（否则 `RectMask2D` 什么都裁不住 ⇒ 内容画到框外），
且内容不能比视口更宽（否则左右被裁）。全场景 16 份竖向列表实测 **2 份不合格**：

| 列表 | 实测 | 症状归属 |
|---|---|---|
| `RankPage排行榜/Battle Power Ranking/rank/邮件Scroll View` | **视口 0×0** ⇒ 遮罩失效、内容画到框外 | 本批 #6（正在修） |
| `ShopPage商店/Product page/展示卡牌Scroll View` | 视口宽 `678.2` < 内容宽 `704.9` ⇒ **左右被裁** | ⭐ 正是 backlog **#5 商店**「道具卡牌的 `Content` 有问题」 |

> 这条体检已接入 `Logs/verify-adapt12-offline.ps1` 第 7 步 ⇒ 修完排行榜后若商店那条还在，
> 就说明 **#5 商店**只需要把这份 Content 收窄/换成贴满视口的锚点即可（病根已定位，不必重新取证）。

## 第 1 批（P0 回归）：英雄详情 3 个 toggle —— 取证结果

**负责人选择：方向 A + 按批次走（2026-09-21）** ✓

### 已取证（`ClientShell.unity` 实读，2026-09-21）

| 结论 | 证据 |
|---|---|
| **3 个 toggle 节点都存在且 `active=1`** | `HeroDetailPage英雄详情主页/downCanvas/Panel/group/{天赋Toggle, 秘技Toggle, 终结技Toggle}` |
| **场景事件完好、没被破坏** | 三个 toggle 各有 1 条持久调用 `SetActive`，目标 fileID：`天赋Toggle→607240859`、`秘技Toggle→1744901391`、`终结技Toggle→859971019`；`m_IsOn`：天赋=1、秘技=0、终结技=0 |
| **toggle 的视觉子节点也都在** | 每个 toggle 下有 `Background`（含 `Checkmark`），均 `active=1` |
| 三个目标页存在 | `downCanvas/天赋页`、`downCanvas/秘技页`、`downCanvas/终结技页`（均 `active=1`） |

### 因此结论（重要）

> **不是"接线/事件"坏了，而是"位置/遮挡"问题** —— 与负责人同批反馈的
> "`downCanvas/leftmiddleCanvas` 上移挡住 `upCanvas/right`" **同源**：**纵向锚点**在 1339 单位的画布上把 `downCanvas` 推离了原位。

### 下一步（第 1 批收尾所需的确切读取项）

1. 读 `downCanvas`、`downCanvas/Panel`、`downCanvas/Panel/group` 的 `anchorMin/anchorMax/anchoredPosition/sizeDelta` ⇒ 判断是**中锚**还是固定位置；
2. 读 `upCanvas`、`upCanvas/right`、`downCanvas/leftmiddleCanvas`、`downCanvas/{后退Button 所在容器}` 的同类值 ⇒ 定位重叠与遮挡；
3. 读 `HeroDetailPage英雄详情主页` **页面根**的锚点（是否全屏拉伸 —— 之前批量核查里它是 full=True ✓）；
4. 按"同类元素统一锚点类型 + 等效 offset 回写"修正（方向 A）；
5. **加自检断言**（`ClientSelfTest`）：断言这 3 个 toggle 存在、`active`、且各有 1 条持久调用 ⇒ **防止再次回归**（负责人注明"以前解决过"，必须留防回归机制）。

### 本轮未改动任何场景/代码（纯取证）

---

## 第 1+2 批（本轮执行 · 2026-09-21 第二轮）

> 负责人本轮补充澄清：
> ① 3 个 toggle 的问题**不是"看不见"，而是"点一个时 3 个的字体贴图和底框不跟着变"**（底框可先不管，**字体贴图必须显示**）；
> ② `leftmiddleCanvas` 选择**下移到 downCanvas 顶部、与属性条同一区**；
> ③ 英雄页"卡牌跑到左上角、与『弹珠』重叠"确认就是这个现象。

### 第 1 批（P0 回归）· 英雄详情 3 个技能页签 —— **真根因（比上一轮取证更深一层）**

上一轮结论"节点/事件完好，是位置遮挡"**对了一半**。本轮继续读场景 YAML 得到 4 条硬证据：

| # | 结论 | 证据（实读） |
|---|---|---|
| 1 | **层级是 `Toggle/Background/Checkmark`** —— `Checkmark` 是 `Background` 的**子节点**，不是 Toggle 的直接子节点 | `Background` tr=2127185209，其 `m_Children` = `[1407973536]`，而 1407973536 的 GO 名 = `Checkmark` |
| 2 | 三张「选中框」贴图**全工程零引用** | `天赋/秘技/终结技选中框.png` 的 guid 在场景里 0 次出现 |
| 3 | 三个页签的底框贴图**接错且状态不一致** | 天赋Toggle/Background = `秘技选中框`(401e7ee7)、秘技Toggle/Background = `天赋选中框`(a724f8e2) **且 `m_Enabled: 0`**、终结技 = 自己那张 ✓ |
| 4 | Toggle 的 `onValueChanged` 持久调用**都还在**（天赋 607240859 / 秘技 1744901391 / 终结技 859971019），`group` 也挂着 ToggleGroup | 与上一轮结论一致 ✓ |

⇒ **所以"字体贴图不显示"不是接线坏死，而是"美术资源根本没接上 + 代码按错误的层级去找 Checkmark"**。

**修法（本轮）**：
1. 新增 `Assets/Client/Runtime/UI/ClientHeroDetailToggleSkin.cs`：按选中段**换底框 sprite**（选中 = `*选中框.png`、未选中 = 空，与《参考.png》"只有选中页签有底框"一致），并保证文字贴图恒显；
2. `ClientHeroDetailPage.BindToggleSkin` 改为**按真实层级**找 `Background` → `Background/Checkmark`（旧代码找 Toggle 直接子节点 ⇒ 恒为 null）；
3. 编辑器工具新增 `FixHeroDetailSkillTabs()`（幂等）：写 `Background` = 自己那张选中框、`Checkmark` = 自己的文字贴图、
   恢复 `秘技Toggle/Background` 的 Image、按贴图原始宽高比统一文字尺寸（46 高）、补挂皮肤组件并绑定、把 `Toggle.graphic` 置空；
4. `ClientSelfTest` 新增 `RunHeroDetailToggleChecks()`（**防回归**）：页签存在/激活、持久 `SetActive` 仍在、
   三张选中框与三张文字贴图**互不相同**、皮肤 3 个绑定齐全。

> ⚠️ 为什么不能把文字贴图交给 `Toggle.graphic`：Unity 的 `UI.Toggle.PlayEffect` 在 `isOn == false` 时会把
> `graphic` 的 alpha 置 0 ⇒ 未选中的两个页签文字会消失。也不 `SetActive` 隐藏底框 —— 会连带隐藏它的子节点文字。

### 第 2 批 · 英雄详情锚点 + 英雄页卡牌几何

| # | 页面 | 实测根因 | 修法 |
|---|---|---|---|
| 3 | 英雄详情 `后退Button` | 中锚 `(0.5,0.5)` + offset `(-286,-674)`；画布高 1339 时落在 `y=-35~26`（屏幕外/被底栏压），1630 时才在 `110~171` | 改**左上锚** `(0,1)`，offset 取设计基准 1630 的等效值 `(44.5,-1488.5)` ⇒ 任何比例都稳在左上 |
| 3 | 英雄详情 `downCanvas/leftmiddleCanvas` | 中锚 offset `(231,510.72)`、尺寸 `301×227` ⇒ 1339 下落在 `y=730~957`、`x=457~758`，与 `upCanvas/right`(`y=787~863`,`x=680~753`)**重叠** | 改**贴 downCanvas 顶部**：anchor `(0.5,1)`、pivot `(0.5,1)`、pos `(231,0)`；联动把 `downCanvas/Background`(属性条) 移到卡片下方 `y=+44.9`（同区不重叠） |
| 2 | 英雄页卡牌列表 | `HeroPage英雄/{HeroSubPage,GuideSubPage}/MiddleCanvas/Canvas/Scroll View/Viewport/Content` 是**中锚** `(0.5,0.5)` + pivot `(0,1)` + offset `(-299,503)` ⇒ 左边缘算到"视口左界 − 299" ⇒ **整列卡牌跑到滚动框左侧外面**（绝对坐标实测 x ≈ −309~207） | 锚点改 `(0,1)-(0,1)`、pivot `(0,1)`、offset `(0,0)` ⇒ 内容左边缘 = 视口左边缘、首行贴视口顶；只动相关轴 |

**同一批新增的防回归断言**：`ClientSelfTest.RunHeroPageLayoutChecks()`（后退键贴顶锚 / leftmiddleCanvas 贴 downCanvas 顶部 / 两个子页 Content 左锚且 x offset = 0）。

### 本批验证状态

| 验证 | 结果 |
|---|---|
| **场景写入** | ✅ **已应用**：6 张贴图、3 个文字 rect、3 个锚点、3 个 Toggle 的 `graphic` 置空、皮肤组件 + 3 组绑定、**英雄页两份卡牌 Content** |
| 静态审计 `Logs/ui-adapt-audit3.ps1`（只读） | ✅ **PASS 37 / FAIL 0**（修复前 PASS 26 / FAIL 11） |
| **自检断言静态镜像** `Logs/selftest-scene-mirror.ps1` | ✅ **PASS 35 / FAIL 0** —— 逐条镜像 `ClientSelfTest` 的场景类断言（页签存在/激活/持久调用/`graphic` 已清/三张选中框与三张文字互不相同/皮肤 3 组绑定/后退键贴顶/两份 Content 左锚） |
| 场景结构完整性 `Logs/check-scene-integrity.ps1` | ✅ **INTEGRITY: OK** —— 7922 个 document（+1 = 皮肤组件）、**无重复 fileID、无畸形头、未丢任何 document** |
| 文本补丁幂等性 | ✅ 二次运行输出 `nothing to change (already patched)` |
| 三目标编译 `Logs/verify-compile.ps1 -Profile All` | ✅ **0 error** |
| `ClientSelfTest`（编辑器内真跑） | ⏳ **被阻塞**：编辑器占着工程，批处理实测报 `Multiple Unity instances cannot open the same project`（`HandleProjectAlreadyOpenInAnotherInstance`，退出码 `0x40000005`）。命令已备好 —— 关编辑器后跑 `Logs\verify-adapt12.ps1` 即可；上面那份**静态镜像**已用同一组判据先行验证通过。 |

> ⚠️ 本轮另修掉一处**我自己的取证错误**：第一版审计脚本把两份卡牌 `Content` 的 transform id 写错了
> （`268087959` 实为**图鉴页**、`639542599` 根本不是这两份之一），导致**英雄页那份（`2127216787`）漏改**。
> 是"自检镜像"按真实层级走一遍才暴露出来的 —— 说明**路径式查找比硬编码 fileID 可靠**（C# 工具用的就是路径式，所以它一直是对的）。

### 为什么本轮是"文本补丁 + 编辑器工具"两条路

Unity 编辑器从 18:05 起一直开着，批处理 Unity **无法启动**（`HandleProjectAlreadyOpenInAnotherInstance`），
所以本轮用 `Logs/apply-adapt12-text.ps1` 把**与编辑器工具完全相同的值**直接写进场景 YAML，并做了三重护栏：

1. 写入前**强制备份**；
2. 写入后**重新解析**，逐条断言 6 张贴图 guid 与 3 个锚点确实生效，任一不符就**不写盘**并报错；
3. 再用 `check-scene-integrity.ps1` 与备份对比 document 集合（不能丢 document / 不能重复 fileID）。

> 过程中踩到并修掉两个**会静默出错**的坑，已写进 `UI_LAYOUT_DEBUG_GUIDE.md` §7：
> ① 正则字符类 `[-\d]` 是**范围** `\`..`d`，不是"减号或数字"（接 `, type: 3}` 后缀时会静默失配）；
> ② PowerShell 里 **`,` 的优先级低于 `+`**，`@($a + 'x', $b + 'y')` 会被当成**一个**元素。

**编辑器工具仍是权威路径**（`Client → 结构修复 → 7. UI 适配 第 1+2 批` 或 `2. 执行结构修复`）：
它幂等、与文本补丁目标值一致，负责人随时可再跑一次校正。

### 负责人请按序执行（3 步）

> ⚠️ **本轮实测到两个前提，先处理否则你会看到 "Missing (Mono Script)"**：
> ① 那个编辑器进程**还停在 Play 模式、且场景是脏的**（窗口标题 `pinball - ClientShell - WebGL - Unity 2022.3.57f1c2* <DX11>`，
> 末尾 `*` = 有未保存改动）；
> ② 它**还没导入本轮代码** —— `Library/ScriptAssemblies/Assembly-CSharp.dll` 停在 **17:43**、
> `Assembly-CSharp-Editor.dll` 停在 **18:17**、`Assembly-CSharp.csproj` **不含** `ClientHeroDetailToggleSkin.cs`。
> 场景是直接写盘的 ⇒ **内存里还没有这个类型**。

1. **先退出 Play 模式**（会触发脚本刷新 + 重新编译）。等控制台编译完成；
   若担心那份"脏场景"里的旧数据被保存覆盖，**先别按 Ctrl+S**。
   工具已加**刷新前自检**：只要脚本比 `Assembly-CSharp.dll` 新，`RepairAll` / 菜单 7 都会打出红色警告
   （`⚠ 编辑器内存里的代码是旧的…`），`ClientSelfTest` 也会把"代码过期"与"接线坏了"分开报 —— 不会再静默出错。
2. **若页签组件显示 Missing (Mono Script)**：点菜单 **`Client / 结构修复 / 7. UI 适配 第 1+2 批`**
   （幂等；会补挂组件、绑定 3 组皮肤）。然后进 Play 复验 1080×1920 / 2160 / 2280。
3. **跑自检**：编辑器内菜单 **`Client / 自检 / 运行全部纯逻辑自检`**；
   或（关掉编辑器后）`powershell -NoProfile -ExecutionPolicy Bypass -File Logs\verify-adapt12.ps1`。

### 不用打开 Unity 的一条命令（本轮新增）

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Logs\verify-adapt12-offline.ps1
```
一次跑完 6 项（编译 → 静态审计 → 自检镜像 → 引用校验 → 补丁幂等 → 场景完整性），末尾给**单一结论**：
`VERDICT: ALL GREEN (offline checks)`。当前实测即为 ALL GREEN。

### 复验要点

- 英雄详情：点三个页签 → **文字与底框一起变**（选中页签有「选中框」底图，另两个只显示文字）；后退键完整不被压；等级卡贴在下方区域顶部、不再压右侧翻页键。
- 英雄页：卡牌从滚动框**左上**开始、不再压「弹珠」。

### 本轮改动的文件

- 新增 `Assets/Client/Runtime/UI/ClientHeroDetailToggleSkin.cs`（页签选中态皮肤）
- 改 `Assets/Client/Runtime/ClientHeroDetailPage.cs`（按真实层级绑定皮肤 + 选中时施加皮肤）
- 改 `Assets/Client/Editor/ClientShellStructuralRepair.cs`（新增 4 个幂等方法 + 菜单 7）
- 改 `Assets/Client/Editor/ClientSelfTest.cs`（3 组防回归断言）
- 改 `Assets/Client/UI/HeroDetailPage/MODULE.md`（记录真实层级与坑）
- 新增取证/审计脚本（`Logs/`，都是证据，勿删）：`ui-node-query.ps1`、`ui-blocks.ps1`、`ui-image-dump.ps1`、
  `ui-geom-compute.ps1`、`ui-absrects.ps1`、`ui-herodetail-stack.ps1`、`ui-adapt-audit3.ps1`、`verify-adapt12.ps1`

### 仍未做 / 待确认

- 页签**文字贴图的垂直位置**：现居中于选中框内；《参考.png》里文字偏下，属可微调项（等负责人看图定）。
- `upCanvas` 与 `downCanvas` 的总高（734+666.6）在 1339 画布上仍**偏紧**（本轮只把"被压/被遮"改掉，未压缩内容）；
  若复验后觉得上半区更挤，再按第 2 批同源方案继续收。

