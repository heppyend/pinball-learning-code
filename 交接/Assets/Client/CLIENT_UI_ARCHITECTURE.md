# 弹珠项目客户端与 UI 构建说明

> 本文描述当前 `Assets/Client` 的实际客户端实现方式，用于后续开发、素材替换和服务端接入。它只覆盖非战斗客户端；既有 `Boot`、`LoginScene`、`MainScene`、街机战斗、YooAsset/HybridCLR/XLua 基础设施不属于本文改造范围。

## 1. 目标与边界

当前客户端的目标不是替换原项目总架构，而是在独立场景中完成微信小程序端的非战斗体验：主页、个人中心、英雄、背包、养成、编队、弹珠、扭蛋、商城、邮箱、排行榜、活动、任务和公告。

当前阶段使用可替换的本地模拟数据完成交互闭环。服务端协议、账号、支付、真实运营配置、战斗属性、最终美术和微信平台桥接均保留为后续接入点，不能在 UI 脚本中自行假设或硬编码。

## 2. 总体结构

```text
Assets/Client/
├─ Scenes/
│  └─ ClientShell.unity            独立客户端开发场景
├─ Runtime/
│  ├─ Domain/                      英雄、道具、活动、邮件等领域模型
│  ├─ Services/                    数据服务接口与本地模拟实现
│  ├─ UI/                          统一页面标识、页面生命周期、导航栈
│  └─ Client*.cs                   各页面的数据绑定和交互控制器
├─ Editor/
│  └─ ClientShellDirectUpdater.cs  仅命令行调用的场景写入器
├─ UI/
│  ├─ MainPage/                    主页拆分素材
│  ├─ HeroPage/、HeroDetailPage/   页面素材
│  └─ Common/Prefabs/              通用可编辑预制体
├─ MODULE.md                       模块边界
└─ CLIENT_UI_ARCHITECTURE.md       本文
```

客户端场景是 `Assets/Client/Scenes/ClientShell.unity`。它不加入旧项目的 Build Settings，也不从原街机启动链跳转进入。开发、演示和 UI 调整均先在此场景完成。

## 3. Canvas 与场景层级

`ClientShell` 中的 `ClientCanvas` 是唯一客户端 UI 宿主。不能在 Canvas 外新增客户端业务 UI 根节点。

```text
ClientShell
└─ ClientCanvas
   ├─ PagesLayer       所有可导航页面根节点
   │  ├─ MainPage
   │  ├─ ProfilePage / HomeShowcasePage / BadgePage
   │  ├─ HeroPage / HeroDetailPage / HeroEnhancePage
   │  ├─ BackpackPage / FormationPage / MarblePage
   │  └─ GachaPage / ShopPage / MailPage / RankPage
   │     ActivityPage / TaskPage / NoticePage
   ├─ PopupLayer       页面级或结果类弹窗
   │  └─ GachaResultPopup
   └─ SystemLayer      全局轻提示、加载、遮罩、红点等系统 UI
      └─ UiFeedback
         ├─ Toast
         └─ Loading
```

这个分层解决的是显示关系和生命周期问题：

- 页面之间由 `PagesLayer` 的导航器控制，只有当前页位于前台；
- 弹窗必须在 `PopupLayer`，天然显示在页面之上；
- Toast、Loading 这类全局内容必须在 `SystemLayer`，显示层级高于页面与普通弹窗；
- 每一个背景、图标、文本、按钮、选中底框都是 Scene 中可单独选中的实体，可在 Hierarchy 调整位置、大小、Sprite、颜色和层级。

在同一个父节点下，Hierarchy 中靠后的同级 UI 会绘制在更上层。若要让 A 覆盖 B，将 A 放在 B 后面；若要让选中底框在图标后面，将选中底框放在图标按钮前面。

## 4. 页面与导航

页面使用三项共享能力，不允许再建立平行导航器：

| 能力 | 位置 | 作用 |
| --- | --- | --- |
| `ClientUiPageId` | `Runtime/UI/ClientUiPage.cs` | 页面唯一标识 |
| `ClientUiPage` | `Runtime/UI/ClientUiPage.cs` | 页面 Enter/Pause/Resume/Exit 生命周期 |
| `ClientUiNavigator` | `Runtime/UI/ClientUiNavigator.cs` | 注册页面、维护返回栈、打开与返回 |

主页入口由 `ClientHomePage` 统一发出 `NavigationRequested`，`ClientUiNavigator` 将其转换为页面 ID。返回按钮统一使用 `ClientUiBackButton`，它调用导航栈的 `Back()`，因此不应在各页面单独写“返回主页”的业务逻辑。

典型流转：

```text
MainPage
  ├─ 头像 / 个人中心 → ProfilePage → HomeShowcasePage / BadgePage
  ├─ 编队 → FormationPage
  ├─ 弹珠 → MarblePage
  ├─ 背包 → BackpackPage
  ├─ 活动 → ActivityPage
  └─ 其他入口 → 对应独立页面

HeroPage → HeroDetailPage → HeroEnhancePage
GachaPage → GachaResultPopup（仍停留在 GachaPage）
```

新增一级页面时，应先创建 `PagesLayer` 下的页面根，挂载 `ClientUiPage` 并注册到 `ClientUiNavigator`；再增加 `ClientUiPageId`、导航映射、页面控制器与本地数据服务。禁止通过 `SetActive` 散落跳转逻辑来绕开导航栈。

## 5. 页面脚本与数据流

页面控制器只负责三件事：读取共享数据、响应按钮、刷新场景中已有节点。它们不在运行时创建业务 UI 根，也不直接保存运营数据。

```text
场景 Button
    ↓
Client*Page 页面控制器
    ↓
IClientDataService（接口）
    ↓
LocalClientDataService（当前模拟实现）
    ↓ DataChanged
页面/主页/红点刷新
```

核心边界：

- `IClientDataService` 是 UI 与数据层的唯一入口；
- `LocalClientDataService` 目前提供英雄、背包、货币、扭蛋、商品、邮件、活动、任务、公告、编队和弹珠的示例数据；
- `ClientServices` 只保存当前的数据服务实例；未来接入服务端时，用同一接口替换实现，页面不应大规模改写；
- 成功、失败、资源不足、已领取、未读、锁定、空数据等状态都应由服务返回或领域模型驱动。

例如商城流程是“点击商品 → 仅显示详情与价格 → 点击确认购买 → 服务校验货币与限购 → 扣款/入包 → `DataChanged` → 刷新余额和状态”。邮件、活动、任务和扭蛋也遵循同样的读数据—执行动作—刷新机制。

## 6. UI 构建方式

### 6.1 当前原则：Scene 是 UI 的真实载体

所有可见内容都应最终写入 `ClientShell`，这样美术和策划可直接在 Unity Hierarchy 中选中并改动。运行时代码只改变文字、图标、显隐、颜色与状态，不负责“临时拼页面”。

### 6.2 场景写入器

`Editor/ClientShellDirectUpdater.cs` 是当前的场景写入工具。它只在 Unity 命令行批处理时执行，用于安全、可重复地向已有页面补节点或绑定引用，例如：

- 新增页面根、页面标题、卡片、详情区、操作按钮；
- 写入 `PopupLayer/GachaResultPopup`；
- 写入 `SystemLayer/UiFeedback`；
- 写入主页红点与底部导航选中底框；
- 为已有场景节点建立页面脚本引用。

场景写入器不是运行时依赖，也不再出现在 Unity 顶部 `Client` 菜单中，避免误触覆盖人工布局。需要执行时由开发流程调用对应静态方法，完成后检查 Unity 日志中是否同时包含：

```text
Tundra build success
Exiting batchmode successfully now!
```

写入前应关闭正在打开该工程的 Unity Editor；同一工程不能同时由命令行 Unity 和 Editor 打开。

### 6.3 旧 UI Builder

`ClientShellUiBuilder.cs` 保留历史构建和预制体辅助方法，但其 `Client/*` 顶部菜单已经移除。不要重新启用这些菜单作为日常开发方式，以免重建页面时覆盖负责人已经手工调整的节点。

## 7. 素材、预制体与显示层级

素材路径按页面归属：

- `UI/MainPage/Sprites`：主页背景、头像、顶部栏、主页卡片、底部栏和图标；
- `UI/HeroPage/Sprites`、`UI/HeroDetailPage/Sprites`、`UI/BadgePage/Sprites`：对应页面拆分素材；
- `UI/Common/Prefabs`：可复用的入口预制体，例如 `BottomFunctionIcon`、编队/扭蛋按钮。

导入 PNG 后必须确认 Unity Import Settings 为 `Sprite (2D and UI)`，否则 `Image.sprite` 可能为空。现有素材通过场景 `Image` 直接引用，后续纳入 YooAsset 前需确认地址、分包、生命周期与回收策略。

预制体的建议结构：

```text
功能入口根（Image + Button，负责点击范围）
├─ SelectedFrame（选中背景；默认隐藏）
├─ IconFrame（图标底框）
├─ Icon（图标）
└─ Label（文字）
```

根按钮是唯一的点击判定区域；装饰节点应关闭 `Raycast Target`，以免遮挡点击。

## 8. 底部五导航的选中状态

主页底部的五个按钮为：

```text
ProfileButton   个人中心
MarbleButton    弹珠
HomeButton      主页
BackpackButton  背包
ActivityButton  活动
```

`ClientBottomNavigationBar` 管理它们的视觉状态：

- 未选中使用 `底部icon/<名称>2.png` 的白色图标；
- 选中使用 `底部icon/<名称>1.png` 的黄色图标；
- 每个按钮旁有可编辑的 `<ButtonName>_SelectedFrame`，只在选中时显示；
- 主页默认选中；点击头像进入个人中心时同步选中个人中心；回到主页时恢复主页选中。

选中底框位于按钮同一父级，Hierarchy 顺序在图标按钮之前，因此底框在后、图标在前。需要改变覆盖顺序时，直接在 Hierarchy 调整兄弟节点顺序即可。

## 9. 全局反馈与红点

`ClientUiFeedback` 位于 `SystemLayer/UiFeedback`，包含：

- `Toast`：成功、失败、资源不足等短提示；
- `Loading`：未来异步请求、资源加载时的全局遮罩。

当前商城确认购买和邮箱一键领取会调用 Toast。未来异步服务请求开始时调用 `ClientUiFeedback.SetLoading(true)`，在成功、失败或取消的统一出口调用 `SetLoading(false)`。

`ClientHomeRedDotController` 监听 `IClientDataService.DataChanged`，根据本地数据刷新主页的：

- 邮件红点：未读或还有附件未领；
- 活动红点：活动已开启、达到目标、尚未领取；
- 公告红点：存在未读公告。

真实服务端接入后仍然只更新数据服务；红点控制器不应改为直接请求网络。

## 10. 新页面的标准步骤

1. 先从交互草图确认页面入口、返回、页签、空态、禁用态、领取/失败反馈；未定义的规则记录为待确认。
2. 在 `Domain` 与 `IClientDataService` 增加所需模型和接口，在本地服务实现示例数据与状态变化。
3. 在 `PagesLayer` 创建结构化页面骨架：标题栏、页签、卡片/列表、详情区、状态区、固定操作区和返回入口。
4. 编写 `ClientXxxPage`，通过 `Configure(...)` 接收场景节点引用；`Show/Refresh` 中绑定数据，不在运行时建 UI。
5. 注册 `ClientUiPageId` 和 `ClientUiNavigator`，接入主页入口或上级页面入口。
6. 将弹窗写到 `PopupLayer`，Toast/Loading 写到 `SystemLayer`。
7. 关闭 Unity Editor 后执行命令行场景写入与编译；再由负责人在 Play Mode 手动走通。

## 11. 验证与人工回归

命令行验证使用项目 Unity `2022.3.57f1c2`，必须带：

```powershell
$env:ALLUSERSPROFILE = 'C:\ProgramData'
```

人工验证路径：

1. 打开 `Assets/Client/Scenes/ClientShell.unity`。
2. 在 Hierarchy 检查 `ClientCanvas/PagesLayer`、`PopupLayer`、`SystemLayer` 是否存在。
3. 点击 Play。
4. 逐一验证主页五导航、头像、编队、扭蛋、商城、邮箱、活动、任务、公告、英雄培养与背包。
5. 重点验证：返回栈、扭蛋结果弹窗、商城二次确认、邮箱一键领取、红点刷新、底部选中高亮。
6. 如失败，回传 Console 第一条 Error/Exception、页面截图和复现步骤。

## 12. 当前完成度与后续接入

当前完成的是“本地模拟闭环完成”：页面结构、导航、主要交互、资源不足/空态/领取/锁定等本地状态和可编辑场景节点已经具备。

尚未完成且不能自行补假设的内容：真实服务端协议、登录账号、支付、真实概率/保底、运营后台、活动周期、最终数值、战斗装备属性、微信小游戏平台桥接、YooAsset 分包策略和最终美术适配。

后续服务端接入的正确方式是新增 `IClientDataService` 的网络实现或适配层，保持页面控制器和 Canvas 层级稳定；不要将 HTTP、订单、密钥或服务器地址写入页面脚本或场景。
