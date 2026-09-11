# WebGL/微信小游戏 TCP 验证交接

更新时间：2026-09-03

当前执行范围：验证 WebGL/微信小游戏中的最小真实会话——HTTP 网关、TCP、握手、游客登录和英雄数据链路。`ClientTcpConnectionProbe` 已挂载至 `Boot`，用于独立验证，不替代原业务登录流程。

## 唯一目标与完成标准

在微信小游戏 WebGL 包内完成一次可审计的最小网络会话：

1. 通过既有 WebGL `UnityWebRequest` 网关流程取得 IP 和端口。
2. 使用微信 `TCPSocket` 对该 IP/端口建立 TCP 连接。
3. 按 PC 端 `ClientGT` 的真实协议完成握手与游客登录；device 固定为 `WebGLMiniGameDeviceIdentifierPinballNode`（40 个纯英文字母）。
4. 发送 `CLPFGetHeroReq`（模块 2、协议 1），成功接收并正确解析 `CLPFGetHeroAck`（模块 2、协议 2）。
5. **只有英雄数据成功解析后**才由客户端主动关闭 TCP，并输出“成功退出”。

其他退出均必须判定为失败，并在微信开发者工具 Console 输出原因：

- `onError`：TCP API 失败，记录当前阶段和 SDK 错误信息（不得输出 token）。
- `onClose` 且尚未成功解析英雄数据：记录“服务器主动断开或连接非预期关闭”、当前阶段、是否收到登录/英雄回包。
- 客户端超时、无效包、协议拒绝或解析失败：记录客户端主动关闭的具体原因。
- 禁止将“已建连”“已握手”“已登录”或“连接关闭”表述为整体成功。

每个阶段必须输出：阶段名、模块/协议 ID、包字节数、结果；网关回调必须输出 IP 和端口。不得输出登录 token、随机密钥或个人资料。

## 当前已确认事实

- 项目为 Unity `2022.3.57f1c2`，使用 HybridCLR/YooAsset/微信 WebGL SDK。
- `NetController.GetIpPort()` 在 WebGL 分支已改为 `UnityWebRequest` 获取网关地址并写入 `SysDefines.Ip/Port`；PC 分支仍保留旧实现。
- 微信 SDK 支持 `WXBase.CreateTCPSocket()`、`Connect`、`OnConnect`、`OnError`、`OnClose`、`OnMessage`、`Write`。
- 协议包格式：小端 `int32` 总长度（包含长度字段）+ `ushort` 模块 ID + `ushort` 协议 ID + 正文。
- 握手请求是 ClientGT 模块 1/协议 0；握手应答是模块 1/协议 1，正文按旧代码读取 `sbyte errcode`、`int payload`、`int random_key`。
- 游客登录请求是模块 1/协议 3，响应模块 1/协议 5；`CLPFGetHeroReq` 是模块 2/协议 1，响应 `CLPFGetHeroAck` 是模块 2/协议 2。
- 历史运行日志曾证明：网关获取、TCP 建连、握手、游客登录及 GetHero 收包可全部跑通；问题在后续临时修改把探针脚本损坏。
- 固定 device 是负责人的明确新决定，不使用 `n/a`、机器码、PlayerPrefs 或随机 SHA-1。

## 当前阻塞与禁止事项

- `Assets/Client/Runtime/ClientTcpConnectionProbe.cs` 作为 `Boot` 启动入口，按反射边界调用原 `NetController.GetIpPort` 并继续执行真实 TCP 最小会话。当前需执行 Unity 编译和微信回归验证：
  - 直接引用 `NetController`/`SysDefines` 已移除，改为等待 HotFix 加载后通过反射获取 `Instance`、`GetIpPort` 和 `SysDefines.Ip/Port`。
  - 握手字段已补齐；游客 token 使用与 PC `UnityHelper.CA3Encode($"{device},{OpeninstallToken}", randomKey)` 等价的本地实现，不再使用错误的 Base64 占位。
  - 登录应答和新版 `ClientPF.cs` 的英雄/英雄组字段均做完整解析、计数上限和包尾校验；只有 `heroDataReceived` 成功后才主动关闭。
  - 已增加单一阶段超时协程、包字节数/模块/协议/结果日志和失败退出日志。
- 不要再以“直接引用 HotFix 类型”解决反射问题；这会复发 IL1005/程序集解析失败。
- 不要更改 IP、端口、服务端、账号、token、YooAsset 基础设施、微信 SDK 或项目设置。
- 不要在未获得新的日志证据时反复改端口、随机改协议字段或宣称成功。

## 权威源码与阅读顺序

1. `AGENTS.md`、`CURRENT_STATE.md`、`DEVELOPMENT_WORKFLOW.md`、`Assets/Client/MODULE.md`。
2. `Assets/Client/Runtime/ClientTcpConnectionProbe.cs`：待修复的 AOT 探针。
3. `Assets/Scripts/GameLogic/NetController.cs`：`GetIpPort`、`SendClientHandReq`、`SendLoginPlatformReq`、`SendGetHeroReq`。
4. `Assets/Scripts/NetWork/JBPROTO/ClientGT.cs`：ClientGT 字段、模块/协议 ID 和二进制顺序。
5. `Assets/Scripts/NetWork/JBPROTO/ClientPF.cs`：新版 `CLPFGetHeroAck`、英雄 `fight:Int64` 和英雄组字段顺序。
6. `Assets/Scripts/GameLogic/NetController.cs` 所用的 `UnityHelper.CA3Encode` 实现。

## 实施计划

### 阶段 A：恢复可构建的 AOT 探针

- 删除错误的直接 `NetController`/`SysDefines` 类型引用。
- 用 `Type.GetType("NetController, HotFix")` + 反射获取 `Instance` 和 `GetIpPort(Action)`；轮询直到 HotFix 已加载和单例就绪，最长 15 秒。
- 同样通过反射读取 `SysDefines.Ip/Port`，并明确输出两值。
- 对任何 C# 改动先由 Unity Editor 编译；再尝试命令行 Unity 编译。若项目锁阻止批处理，保留日志，不关闭用户 Editor。

### 阶段 B：精确复原协议与状态机

状态：`WaitingHotFix → ResolvingGateway → Connecting → Handshake → GuestLogin → GetHero → SuccessClosing → Closed`。

- 按 PC 端二进制写入握手的全部字段，平台编号保持已验证的 WebGL 值 `6`。
- 握手成功后使用完全等价的 CA3 token 生成逻辑发送游客登录。
- 登录 `errcode != 0` 是失败；登录成功后下一帧发送空正文的 `CLPFGetHeroReq`。
- 严格按 `ClientPF.cs` 读取 `CLPFGetHeroAck`；只有读取完成才置 `heroDataReceived=true` 并主动关闭。
- `OnClose` 中根据状态/标志输出成功或失败；“未收到英雄数据”一律失败。
- 设置单一的阶段超时协程；超时日志必须带阶段和主动关闭原因。

### 阶段 C：人工验收与发布同步

1. Unity 无编译错误后执行 WebGL Build。
2. 转换微信小游戏。
3. WebGL 导出会改变 data/wasm hash，必须上传当前导出的首包文件到 OSS；仅 C# 代码改动通常无需重建 YooAsset bundle，但 OSS 文件必须与当前导出匹配。
4. 清微信开发者工具缓存并重新运行。
5. 回传从启动到关闭的完整 `[TCP客户端]` 筛选日志。

## 必须出现的成功日志（建议固定前缀）

```text
[TCP客户端] 会话开始：device=固定纯英文字母，长度=40
[TCP客户端] 网关成功：IP=<IP>, Port=<端口>
[TCP客户端] TCP 建连成功
[TCP客户端] 已发送握手：模块=1，协议=0，字节=<n>
[TCP客户端] 握手成功
[TCP客户端] 已发送游客登录：模块=1，协议=3，字节=<n>
[TCP客户端] 游客登录成功
[TCP客户端] 已发送 CLPFGetHeroReq：模块=2，协议=1，字节=8
[TCP客户端] 英雄数据接收并解析成功：英雄=<n>，英雄组=<n>
[TCP客户端] 成功退出 TCP：原因=已成功接收英雄数据
```

失败日志应如：

```text
[TCP客户端] 失败退出 TCP：原因=服务器在 GetHero 阶段主动断开，未收到英雄数据
[TCP客户端] 失败退出 TCP：原因=客户端超时，阶段=GuestLogin，未收到登录回包
```
