# Client Runtime 模块说明

## 职责

承载非战斗客户端的运行时入口、领域模型和服务边界。页面只通过这里定义的接口读取或修改客户端状态，不直接调用旧 `NetController`、`WebWork` 或 `NetWork`。

## 入口

- `ClientBootstrap.cs`：`ClientShell` 场景的初始化组件，默认注册本地模拟服务；场景没有有效 Camera 时建立一个只负责清屏的运行时 Camera，避免 Overlay UI 透明区域显示 Unity 的无相机提示。
- `ClientHomePage.cs`、`ClientProfilePage.cs`、`ClientPageNavigator.cs`：场景 UI 的数据绑定与页面导航；视觉层级由 `ClientShellUiBuilder` 生成并保存在场景中。
- `ClientShopPage.cs`：商城现有 GUI 的节点绑定与本地模拟购买闭环；角色卡牌占位商品默认单价为 10 水晶石，真实角色入包、高清图和兑换服务均预留待接入。
- `ClientRankPage.cs`：排行榜既有场景节点绑定。默认战力榜，挑战/战力榜互斥；只刷新现有排行行与玩家阵容卡牌节点，不在运行时生成或调整 UI。数据由 `IClientDataService.GetLeaderboardEntries` 与 `GetPlayerLineup` 提供。
- `ClientHeroPage.cs`、`ClientHeroDetailPage.cs`：绑定负责人已有英雄列表和详情 GUI。任一卡牌点击后经统一导航打开详情并传递该卡牌立绘；详情页通过数字图片 ID、文本列表和升级预览数据刷新，不创建或重排 UI。天赋/秘技/终结技共享本地升级校验入口，材料不足提示通过事件等待统一 Tips 接入。
- `ClientActivityPage.cs`：绑定负责人已有活动 GUI。默认显示新手任务；新手、进阶、活动三页互斥，新手与进阶按独立分类读取进度和领奖状态。任务文本、离散进度、奖励卡、领奖结果以及活动卡文本/标记/图片均保留可替换接口；活动卡未预挂 Button 时在运行时以现有卡片 Image 补充点击能力，不创建或重排节点。
- `ClientCompleteGuideEventPage.cs`：绑定现有全图鉴活动 GUI。页面全部 Image/Text 通过枚举槽接收图片 ID/文本，按语义正确的进度底框与进度条刷新完成度，复用活动页的待完成/领取/已领取状态规则；问号弹窗默认隐藏，入口与返回使用统一导航，不修改场景节点。
- `ClientLoadingPage.cs`：绑定 `ClientCanvas/SystemLayer/加载Page` 现有 GUI。背景、Logo、进度底框、进度条和三个标点通过数字图片 ID 接口替换，TMP 文本通过泛型接口更新；进度条使用目标值与 `SmoothDamp` 从左向右平滑填充，默认平滑时间 0.15 秒，标点使用非受时间缩放影响的协程循环。正式 YooAsset 图片解析器仍待接入。
- `UI/ClientShopPurchaseQuantityProgress.cs`：商城购买数量的离散进度条组件；通过剩余限购数初始化 0 到最大值挡位，负责加减/最大值/图标拖动、TMP 数量文字和填充进度同步。
- `Domain/`：账户、货币、英雄和背包等不依赖 Unity UI 的领域模型。
- `Services/`：数据服务接口、开发期本地实现、服务注册表和服务器数据包边界。
- `Services/ClientServerPacketRouter.cs`：记录收发原始 TCP 包，按模块/协议分类，并将 JSONL 结构记录、可读 `.protocol.log` 摘要与可选原始包捕获写入 `Application.persistentDataPath/ClientServerLogs`；支持 ClientGT 登录/断开原因、ClientPF 英雄及称号/铭牌/徽章/头像/头像框的列表与装备操作摘要，不更新 UI 或主动发送业务请求。

## 禁止事项

- 不保存真实服务端地址、令牌、密钥、订单或账号资料。
- 不在此处实现战斗、关卡或旧街机模块。
- 服务端接入只能新增接口实现并通过 `ClientServices` 注入，不能让 UI 直接依赖网络协议。
- 例外：`ClientTcpConnectionProbe` 是临时 WebGL/微信小游戏链路诊断入口，可按 `TCP_WEBGL_HANDOFF.md` 通过反射调用既有 `NetController.GetIpPort` 并读取 `SysDefines.Ip/Port`；不得形成 AOT→HotFix 编译依赖，也不得被业务 UI 使用。
- 服务器传输必须通过可替换适配器注入 `ClientServerPacketRouter`；不得把 Boot 场景生命周期或微信 SDK 直接写进领域模型和 UI 页面。

## 验证

在 `ClientShell` 运行时 Console 应出现本地模拟数据已初始化的日志；纯逻辑规则应在后续 `Tests/` 中覆盖。
TCP 探针须另按根目录交接文档执行 Unity 编译和微信开发者工具人工验证；未通过英雄数据完整解析前不得报告 TCP 会话成功。
