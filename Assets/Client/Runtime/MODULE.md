# Client Runtime 模块说明

## 职责

承载非战斗客户端的运行时入口、领域模型和服务边界。页面只通过这里定义的接口读取或修改客户端状态，不直接调用旧 `NetController`、`WebWork` 或 `NetWork`。

## 入口

- `ClientBootstrap.cs`：`ClientShell` 场景的初始化组件，默认注册本地模拟服务。
- `ClientHomePage.cs`、`ClientProfilePage.cs`、`ClientPageNavigator.cs`：场景 UI 的数据绑定与页面导航；视觉层级由 `ClientShellUiBuilder` 生成并保存在场景中。
- `ClientShopPage.cs`：商城现有 GUI 的节点绑定与本地模拟购买闭环；角色卡牌占位商品默认单价为 10 水晶石，真实角色入包、高清图和兑换服务均预留待接入。
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
