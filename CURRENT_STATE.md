# 当前项目状态

> 2026-09-11：为实习开发学习与成果复盘创建本地 Git 快照提交 `840fc330`（客户端源码、Lua/WebGL/DLL、场景、预制体、工程配置与治理文档；不含图片、音频、模型、材质、字体、动画和资源包）。推送至配置的 `origin` 时 GitHub 返回 `Repository not found`；待负责人提供已创建且当前凭据可访问的仓库地址或授予访问权限后重试。

> 2026-09-10：按负责人要求关闭商城导航诊断输出但保留脚本。`ClientShopNavigationDiagnostics` 与返回链路仍在，`ClientShopPage.enableNavigationDiagnostics` 默认关闭；后续若需复现，仅在该现有组件 Inspector 勾选此字段即可恢复完整 `[ClientShop导航诊断]` 日志。

> 2026-09-10：商城回归测试日志已核对通过。`[ClientShop导航诊断]` 连续四次均显示 `PointerDown → PointerUp → 商城后退 onClick 已触发 → shopSelf=False/shopHierarchy=False/home=True`，证明主返回按钮、统一导航与主页恢复完整闭环。`Event MallButton` 空函数名 Animation Event 仅出现于修复前的日志段；当前控制器已无空事件，重新导入后的后续运行记录未再出现该警告。未发现本轮商城运行时代码的异常。

> 2026-09-10：负责人要求商城主返回按证据诊断，不再猜测。场景树已确认 `Product page` 只含展示卡牌 Scroll View，之前按“后退Button”绑定的对象均属其它页面；商城实际返回节点为 `ShopPage商店/Bottom page function bar/BackoffButton`。现已精确绑定该节点，并新增开发期 `ClientShopNavigationDiagnostics`，记录绑定、PointerDown/Up/Click、Button onClick 及处理前后商城/主页/导航器状态，供 Console 回传定位。另已清除 `Event MallButton.controller` 的 `Pressed` 动画中空函数名 Animation Event，消除该警告。待 Unity 自动编译与人工复现日志。

> 2026-09-10：负责人反馈商城主界面“后退Button”仍无效。已为 `Product page` 下该按钮及其全部既有可射线子节点添加独立 `IPointerClickHandler`，不再只依赖可能被通用返回组件重置的 `Button.onClick`；点击时重置购买态、关闭商城弹层、调用统一导航 `ReturnHome`，并以直接隐藏商城/激活主页作为导航状态异常时的兜底。待 Unity 自动编译与人工点击验证。

> 2026-09-10：商城逻辑正在替换为角色卡牌占位商品方案：已移除早期 `ClientShopProduct`、`GetShopProducts` 与 `TryPurchaseShopProduct`，改用可替换的 `ClientShopListing`、批量购买与已购数量查询接口；本地单价暂定 10 水晶石。`ClientShopPage` 正在绑定负责人已完成的商城 GUI（常规/活动页、购买弹窗、成功页、兑换码、订单和待发货）。Unity 命令行编译因已有 Unity Editor 占用工程而在脚本编译前退出，日志：`Logs/codex-shop-compile.log`；尚待 Editor 空闲后重验。

> 2026-09-10：根据负责人提供的 Unity Console，已删除 `ClientShellDirectUpdater` 中仍调用旧商城 API 的三个早期编辑器路径（旧商城生成、商品确认生成、核心交互中的商城绑定）。此前 `ConfigureDetail`、`Purchase`、`Configure` 的 4 个 `CS1061` 阻断错误不再有源码引用；未触碰已完成的商城 GUI。待 Unity 自动重编译后确认 Console 无新错误。

> 2026-09-10：负责人提供的重编译 Console 只剩 4 个 Editor 警告，均已按来源修正：`AddBuildMapUtility` 避免遮蔽基类成员、`AssetBundleTool` 移除已废弃的确定性选项、HybridCLR 自有命令移除不可达旧分支、CodeGuard 修正错误标记为校验器的实际菜单项（第三方目录改动由负责人本次“修警告”请求明确授权）。尚待 Unity 再次完成自动重编译确认无警告。

> 2026-09-10：负责人反馈主页商城图标无响应。已定位为主页只绑定旧名 `ShopButton`、而实际节点名为 `BottomFunctionIcon_Shop`，且 `ClientUiNavigator` 被禁用导致导航事件无人处理；已补充图标绑定并启用导航器。PC 客户端会话新增 `allowPcServerConnection` 总开关，`ClientShell` 配置为 `0`：自动启动和手动启动都会只输出“客户端开发模式：已屏蔽”，不执行网关、TCP、握手或登录。未删除服务器代码；待 Unity 自动编译及点击验证。

> 2026-09-10：负责人提供的 Console 剩余 6 条警告已处理：`Init.OnCompleted` 去除无 await 的 `async`；`ClientTcpConnectionProbe` 的仅 WebGL 会话配置改在 `UNITY_WEBGL && !UNITY_EDITOR` 条件块内声明，避免 PC 编译把其序列化值判为未使用。未改变 WebGL 运行路径或 PC 连接屏蔽开关；待 Unity 自动重编译确认。

> 2026-09-10：商城层级与交互修正：所有商城页/弹层每次打开都会 `SetAsLastSibling`，购买成功页的全部现有可射线节点均绑定点击关闭，商城所有同名“后退Button”统一路由至 `Back`，且返回导航改为全场景查找已启用的 `ClientUiNavigator`。`ClientUiFeedback` 的 Toast 会置顶并在 2 秒后自动隐藏；待 Unity 编译及手动点击验证。

> 2026-09-10：负责人反馈常规商城的展示卡牌 Scroll View 覆盖新弹层。已将商城页面/弹层打开时的提升策略扩展为根 `Canvas.overrideSorting = true` 与递增 `sortingOrder`，不再只调整 Transform sibling；因此购买、成功、兑换、订单和待发货会始终高于角色卡牌独立 Canvas。待 Unity 编译与手动验证。

> 2026-09-10：负责人反馈卡牌无法打开购买页、商城主后退未回主页、兑换码图标/文字无法聚焦。`OpenListing` 现实时刷新本地商城条目而非依赖进入页时缓存；`ClientUiNavigator` 新增清空历史的 `ReturnHome` 并由商城主后退调用；兑换输入框的现有 `Image` 与 `Text (TMP)` 增加点击聚焦转发。待 Unity 编译和手动验证。

> 2026-09-09：已在 PC 与小程序共享的 `ClientServerPacketRouter` 中将 `CLGTDisconnectNtf`（模块 1、协议 2）分类为 `ClientGtDisconnectNotification`，可读协议日志会输出断开码和中文原因（正常通知、超时、踢线、异地登录、维护、与平台/游戏服断连、系统错误、离线挂机等）。PC `ClientServerPcSession` 收到该包会输出原因；小程序 `ClientTcpConnectionProbe` 收到该包也会输出原因，且在英雄数据未完成时将其作为失败原因。未改变连接策略或添加自动重连。文本差异检查通过；Unity 命令行编译被当前打开的 Unity Editor 占用，未进入脚本编译，日志为 `Logs/codex-disconnect-notification-compile.log`。

> 2026-09-09：PC 协议会话 `session-20260909-194125.protocol.log` 已验证握手、游客登录（头像/头像框/称号/徽章/铭牌 ID 均已分类输出）及 `CLPFGetHeroAck` 全部成功，随后接收 `CLGTDisconnectNtf`（模块 1、协议 2、9 字节）并发生远端强制断开。Editor.log 中“TCP 读取失败”由后台读取线程先收到连接重置、主线程后归档该断开通知的时序造成；本次没有协议解析失败或前置数据丢失证据。原始包文件仍被运行中会话占用，未读取断开通知的具体 code；若需要区分正常断开与踢线/维护，待会话关闭释放文件后再读取或在记录器中增加该协议摘要。

> 2026-09-09：`codex-clientgt-profile-sync-compile.log` 已显示 Unity 批处理完成，但其无窗口进程未自行退出并留下 `Temp/UnityLockfile`，导致 Hub 显示项目仍打开。已确认无桌面 Editor 窗口后，仅结束本次由代理启动的 batchmode 进程并删除其 0 字节临时锁文件；项目锁已释放，未删除项目资源或用户数据。

> 2026-09-09：已按 `D:\Users\Administrator\Desktop\弹珠配置\tools\jbproto\Protocol\ClientGT.lua/.hpp` 同步新版 `CLGTLoginAck`：字段顺序为头像、头像框、称号、徽章、铭牌，后两项替代旧 `dan/decorate`。项目 `ClientGT.cs` 的二进制读写已更新；PC 端 `GamePlayer` 保存五项装扮 ID；小程序 `ClientTcpConnectionProbe` 按同一顺序读取；`ClientServerPacketRouter` 的登录摘要会分类输出五项内容 ID，继续隐藏用户 ID、昵称、手机号、令牌和点券。未主动发送登录/装扮业务请求。`ClientPF.cs` 的协议 3–22 个性化分类保持生效。文本差异检查通过；Unity `2022.3.57f1c2` 命令行编译已完成，日志 `Logs/codex-clientgt-profile-sync-compile.log` 包含 `Batchmode quit successfully invoked`，未检出 `error CS`。

> 2026-09-09：后端同步的 `ClientPF.cs` 已包含个性化协议：称号、铭牌、徽章、头像、头像框的查询列表及装备/卸下（模块 2、协议 3–22）。`ClientServerPacketRouter` 已补充分类和脱敏字段摘要：列表记录状态、数量和最多 20 个内容 ID，操作记录对象 ID、装备/卸下动作及回包状态；不会主动发送业务请求。初始检查时工作区未找到新版 `ClientGT.cs`，后续已由负责人提供的 `.lua/.hpp` 核对并同步，详见本文件最新记录。Unity `2022.3.57f1c2` 命令行编译已完成，日志 `Logs/codex-personalization-protocol-compile.log` 包含 `Batchmode quit successfully invoked`，未检出 `error CS`。

> 2026-09-09：负责人确认购买数量节点 `Purchase quantity` 为 `TextMeshProUGUI`。`ClientShopPurchaseQuantityProgress` 的数量及端点文字引用已改用 `TMP_Text`，可直接绑定该文字节点；长期基线已补充 TMP/UGUI 文本引用类型一致性与交互组件挂载位置约束。未改场景或 GUI。Unity 命令行编译再次被已打开的同项目 Unity 实例阻止，未进入脚本编译；日志为 `Logs/codex-shop-purchase-tmp-compile.log`。

> 2026-09-09：新增 `Assets/Client/Runtime/UI/ClientShopPurchaseQuantityProgress.cs`，仅实现商城购买弹窗的数量选择进度条逻辑，未改场景、GUI、商品交易或本地服务。组件可按商品 `PurchaseLimit` 初始化 0 至最大值的离散挡位；支持减、加、最大值、拖动图标、图标数量/左右端点文字及填充进度同步。Unity 命令行编译被已打开的同项目 Unity 实例阻止，未进入脚本编译；日志为 `Logs/codex-shop-purchase-quantity-compile.log`。待负责人关闭 Editor 后重跑编译并在 `ClientShell` 手动绑定现有节点验证。

> 2026-09-08：负责人反馈 7 页面仍出现叠加；已读取 Editor.log，当前日志只有旧 `ClientProfileNavigationBinder` 的历史运行记录，没有新 `ClientProfileNavigationController` 的运行时记录，无法据此判断接管链路。已在新控制器加入临时诊断：启动绑定数量、顶部/底部 Toggle 路由、每次 Navigate/Back，以及 ApplyRoute 后实际激活页面数量。待负责人测试后根据 `[ClientProfileNavigationController]` 日志继续定位；本次未改 GUI。

> 2026-09-08：按负责人确认，删除旧的 `ClientProfileNavigationBinder`，改为 `ClientProfileNavigationController` 统一管理个人中心、主页展示及个性化 5 个页面，共 7 个互斥路由；顶部 3 个 Toggle 全局切换，个性化底部 5 个 Toggle 从现有持久化目标解析路由，后退使用历史栈。场景只保留一个活动路由，未调整 GUI。Unity 命令行编译仍被其他 Unity 实例占用，日志为 `Logs/codex-profile-route-controller-compile.log`。

> 2026-09-08：已根据 `C:\Users\Administrator\AppData\Local\Unity\Editor\Editor.log` 的真实运行日志修复主页展示后退：`ReturnFromShowcase` 已收到点击，但个人中心 Toggle 原本就是 `True`，因此没有触发 Toggle 事件；现在设置 Toggle 后始终显式调用 `ShowPage(_profilePage)`。临时诊断已移除。Unity 命令行验证仍被其他 Unity 实例占用，日志为 `Logs/codex-showcase-back-fix-compile.log`。

> 2026-09-08：已根据 Unity Editor.log 定位主页展示后退问题：按钮持久化回调正常触发，但“个人中心Toggle”原本已为 `isOn=True`，重复赋值不会触发 `onValueChanged`，所以页面仍保持主页展示。`ReturnFromShowcase` 现无论 Toggle 状态如何都会显式执行 `ShowPage(_profilePage)`；临时诊断组件已移除。

> 2026-09-08：修正主页展示后退与客户端初始页：`HomepageDisplayPage主页展示` 下现有 `后退Button` 已在场景中持久化绑定到 `PagesLayer` 导航绑定器，点击后选中“个人中心Toggle”；场景初始状态改为仅激活 `MainPage主页`，`ProfilePage个人中心` 初始关闭，需点击主页 `ProfileButton` 后才打开。未调整任何 GUI 尺寸、位置、层级或素材。

> 2026-09-08：按负责人要求不调整主页展示页面任何 GUI 尺寸、位置、层级或素材；仅在 `ClientProfileNavigationBinder` 中绑定现有 `后退Button`，点击后选中“个人中心Toggle”并返回个人中心。待 Unity Editor 空闲后进行命令行编译验证。

> 2026-09-08：已将个人中心导航绑定器挂到 `ClientShell/PagesLayer`。运行时按现有节点名绑定顶部“个人中心 / 主页展示 / 个性化”3 页；个性化进入时默认显示铭牌，并在 5 个个性化页面切换时刷新各自底部 5 项 ToggleGroup 的现有页面显隐事件。新增 `Assets/Client/Runtime/UI/ClientProfileNavigationBinder.cs`；Unity 命令行日志为 `Logs/codex-profile-navigation-bind-compile.log`，本次未取得实际编译结果，日志同时记录 Unity Licensing Client 访问令牌不可用与项目已被其他 Unity 实例占用。

> 2026-09-08：继续修复 `ClientShell` 中个人中心相关两组“个人中心 / 主页展示 / 个性化” Toggle 的 Checkmark 常态显隐。两组共 6 个 Toggle 已解除 `graphic` 自动绑定，保留 `isOn`、ToggleGroup 与页签切换事件；待 Unity Editor 关闭后完成命令行编译验证。

> 2026-09-07：修复 `ClientShell` 中 `BadgePage/Panel` 的 5 个 Toggle Checkmark 显隐问题。已解除 Toggle `graphic` 自动绑定，保留各 Toggle 的 `isOn` 与 `onValueChanged`；场景中的 Checkmark 节点可常显。Unity 命令行编译因当前 Editor 占用项目未进入编译，证据见 `Logs/codex-badge-checkmark-compile.log`。

> 当前短入口（2026-09-04）：WebGL/微信 TCP 最小会话探针目标为“HTTP 获取 IP/端口 → TCP 建连 → 握手 → 游客登录 → 完整解析英雄数据 → 仅成功后主动关闭”。固定 device 为 `WebGLMiniGameDeviceIdentifierPinballNode`（40 位纯英文）。最新微信日志已确认 HTTP、TCP 与握手回包均成功；未进入游客登录的根因是探针附着的 Boot `Init` 对象在切换 `LoginScene` 时被卸载。探针现会迁移至独立的跨场景对象，待关闭当前 Unity Editor 后重新编译/同步 HotFix、构建并验证。

- 更新时间：2026-08-28
- 当前分支：`main`
- 当前阶段：C2「英雄、图鉴与背包」
- 当前唯一目标：在不影响原街机/战斗、服务端或总架构的前提下，建立可持续开发的微信小程序客户端界面与养成模块基础。

## 已确认边界

- 开发范围：养成、英雄/图鉴、背包、商城、邮箱、排行榜、活动、任务、扭蛋、个人中心及本地模拟逻辑。
- 交互范围权威输入：`D:\Users\Administrator\Desktop\J 界面交互草图-弹珠项目-小程序端.pdf`。该 PDF 为单页全流程草图；其中已画出的页面、页签、跳转、空态、禁用态、弹窗、筛选与领取反馈属于客户端验收范围。
- 不开发：战斗/关卡、服务端、支付、账号、总架构；不改造既有 `Boot`、`MainScene`、`LoginScene` 和街机战斗业务。
- 新功能入口：`Assets/Client/Scenes/ClientShell.unity`，不加入现有 Build Settings；后续从该场景验证客户端页面。
- 保留现有 YooAsset、HybridCLR、XLua、URP 和微信 SDK；真实平台接入仅做接口准备。

## 工作区保护

- 已提交前的场景、热更新、YooAsset、HybridCLR、项目设置和微信 SDK 差异均视为既有受保护改动，禁止清理、重置、批量暂存或覆盖。
- 可再生产物（`Library/`、`Logs/`、`Temp/`、`.vs/`、`obj/`、`*.csproj`、`*.sln`）仅记录为清理候选；本轮未删除。
- 历史 WebGL/小游戏调试证据保留于 `BUG_TRACKER.md`、`VERSION_HISTORY.md` 和 `AI_MODEL_UPGRADE.md`，不再作为 C0 的阻塞项。

## 当前检查点

- 2026-09-02 微信开发者工具自动预览失败，错误为 `80051 source size 31013KB exceed max limit 30MB`。已检查 `D:\Users\Administrator\Desktop\弹珠接入text_1\720\minigame`，目录约 30.26 MiB，主要文件为 `data-package/*.webgl.data.unityweb.bin.br` 约 17.6 MB、`wasmcode/*.wasm.br` 约 12.0 MB。结论为上传包超限，运行尚未开始，当前日志不能用于判断 `UnityWebRequest` 或 TCP；需先减少小游戏上传包约 1–2 MB，再继续验证。
- 2026-09-02 诊断微信日志“请求网关地址后无后续回调”：源码 `NetController.cs` 已包含 `GetIpPortWithUnityWebRequest`，但实际小游戏资源 `Assets/HotUpdateResources/Dll/WebGL/HotFix.dll.bytes` 与 `HybridCLRData/HotUpdateDlls/WebGL/HotFix.dll` 时间均为 2026-08-28 13:55:15，未包含本次 2026-09-02 的 UnityWebRequest 改动。结论为 HotFix DLL 未重新编译/同步，当前不是 TCP 或 `GetHeroReq` 问题。待关闭 Unity Editor 后执行 `Build/WebGL/Compile And Sync HotFix DLL`，再重新生成 WebGL/小游戏资源并清理微信开发者工具旧缓存验证。
- 2026-09-02 已将 `Assets/Main/Boot.unity` 中 `ClientTcpConnectionProbe.useDirectLocalTest` 改为 `0`，保留 `runOnStart=1`、`useRealGuestLogin=1`，下次小游戏启动将从本地直连切换为 `GetIpPort → UnityWebRequest` 网关发现流程。当前 `DefaultQLClient.ServerUrl` 仍为局域网 HTTP 地址，开发者工具验证需临时允许未校验合法域名；正式验证前仍需替换为服务端 HTTPS 网关域名。
- 2026-09-02 已按负责人提供的微信小游戏兼容方案改造 `NetController.GetIpPort()`：WebGL/小游戏分支改为 `UnityWebRequest.Post` 协程，复用 `DefaultQLClient.ServerUrl`、`ClientGetGateConnectionRequest` 的 `zone_id`、原 QL 参数、MD5 签名及 JSON/XML 解析，成功后继续写入 `SysDefines.Ip/Port` 并调用原连接回调；PC/Editor 分支仍保留旧 QL 客户端链路。当前网关地址未在仓库新增，小游戏正式验证前需将 `DefaultQLClient.ServerUrl` 配置为负责人提供的 HTTPS 网关域名，并在微信开发者工具加入 request 合法域名。Unity 批处理日志为 `Logs/codex-gateway-uwr-compile.log`，已完成脚本编译并显示 `Batchmode quit successfully invoked`，无本轮 `error CS`；进程退出状态仍待系统回收确认，尚未进行小游戏人工验证。
- 2026-09-01 已重写 `Assets/Client/Runtime/ClientTcpConnectionProbe.cs` 为小程序 TCP 客户端最小会话：连接后按旧协议握手（正确读取 `errcode + payload + random_key`）、游客登录、登录成功后发送 `CLPFGetHeroReq`（模块 2、协议 1、空请求体），并解析 `CLPFGetHeroAck`（模块 2、协议 2）。解析英雄数据已包含新版 `CLPFHeroInfo.fight` `Int64` 字段及英雄组数据；默认不发送真实业务协议，需临时开启 `useRealGuestLogin`。本轮命令行编译因 Unity 2022.3.57f1c2 工程被当前 Editor 占用而未进入编译，日志为 `Logs/codex-tcp-client-compile.log`；待负责人关闭 Editor 后重试，并在微信开发者工具验证游客登录与 GetHero 响应。
- 2026-08-31 已新增加载页运行时逻辑：`IClientResourceLoadSource` 声明可替换的资源进度事件边界，`ClientLoadingPage` 将进度映射到从左到右的 `Image.FillAmount` 并显示百分比，`ClientLoadingDotsAnimator` 以不受 Time.timeScale 影响的协程让 1/2/3 三个加载点依次上移下落并循环。当前脚本已改为自动绑定负责人已有的“加载Page”节点，待挂载验证。
- 2026-08-31 已新增 `ClientTcpConnectionProbe`：通过既有 `NetController.GetIpPort` 获取网关地址，再调用微信 SDK `WXBase.CreateTCPSocket` 仅验证 TCP 建连，成功后立即关闭，不发送登录或业务协议。因 `Assets/Client` 属于 `Assembly-CSharp`，探针已改为反射调用 HotFix 类型，解除 AOT→HotFix 的链接依赖。微信运行日志已证明 Unity 与探针均正常启动；首次探针早于 HotFix 加载而退出，现已改为最多等待 15 秒，待 `NetController` 就绪后再请求网关。该 C# 改动待负责人关闭当前 Unity Editor 后执行 Unity 批处理编译，并重新构建/转换小游戏验证。
- 2026-08-31 TCP 探针复测证据：已输出“等待 HotFix 与 NetController 初始化”，但远程版本请求 `https://localpinball-oss.oss-cn-shenzhen.aliyuncs.com/1/HotUpdate/NewAB/WebGL/2026-08-28-720/StreamingAssets/yoo/DefaultPackage/PackageManifest_DefaultPackage.version` 返回 HTTP 404；本次日志没有网关 IP/端口、`TCPSocket.connect`、成功或失败回调。结论：当前尚不能判断服务器或 TCP，先修复/绕过该 YooAsset 远程版本文件路径后再验证。
- 2026-08-31 OSS 路径修复后复测：版本文件、Manifest 与 HotFix DLL 均已下载，`StartGame.Start`、`GameController.Init` 完成并进入大厅资源加载。探针仍只输出“等待 HotFix 与 NetController 初始化”；经代码检查，`NetController.Instance` 为单例基类继承的静态属性，原反射查询缺少 `BindingFlags.FlattenHierarchy`，已补齐。命令行 Unity 编译再次因负责人 Unity Editor 占用工程被拒绝，日志为 `Logs/codex-tcp-probe-compile.log`；待关闭 Editor 后重新构建/转换小游戏验证。
- 2026-08-31 后续导出目录 `D:\Users\Administrator\Desktop\弹珠接入text_1\720\minigame` 已在 18:22 重新生成，晚于探针修正时间；`Boot.unity` 中 `ClientTcpConnectionProbe.runOnStart: 1`。负责人提供的下一份开发者工具日志从运行 18 秒后的插件帧率监控开始，不含任何 Unity 启动段或 `[TCP探针]` 字样，不能作为 TCP 成功/失败证据；需清空 Console 后重新编译并从启动时采集。
- 2026-08-31 负责人确认运行中曾出现“`[TCP探针] HotFix 已就绪，开始请求网关 IP/端口（HTTP）`”。结合 `NetController.asyncExecuteWebRequest` 的 `Task.Run(() => WebClient.Execute(...))` 和 `QLWebClient` 的 `System.Net.HttpWebRequest` 实现，当前 TCP 验证的阻塞点已确定为旧 HTTP 网关发现层：Unity WebGL 不支持 `System.Net` 网络类和线程，故不会进入网关回调及 `WXTCPSocket.connect`。后续需负责人提供当前网关 HTTP 契约以实现 `UnityWebRequest` 适配，或提供非仓库化的固定测试主机/端口以跳过网关验证。探针日志已改为不记录真实 IP/端口。
- 2026-09-01 负责人提供局域网 TCP 测试目标；已为 `ClientTcpConnectionProbe` 新增不含真实地址的 `useDirectLocalTest`、`directTestHost`、`directTestPort` 本地直连配置。启用后会跳过旧 HTTP 网关，直接调用微信 `TCPSocket.connect`，成功后立即关闭且不发送业务数据。真实地址仅由负责人临时在 Inspector 输入，禁止保存、提交或写入日志。Unity 批处理 `Logs/codex-tcp-probe-direct-compile.log` 已出现 `Tundra build success`，但进程仍处于资源刷新，尚未有最终 `Exiting batchmode successfully` 退出标记。
- 2026-09-01 局域网最小 TCP 连通性验证通过：微信开发者工具日志依次出现“使用本地直连测试参数”“已调用微信 `TCPSocket.connect`”“TCP 建连成功；未发送任何业务数据”“TCP 连接已关闭”。结论仅覆盖开发者工具到当前局域网测试目标的 TCP 建连；不覆盖 HTTP 网关发现、登录/业务协议、真机网络、正式域名白名单或线上服务器。测试完成后必须清空本地直连参数并禁止提交。
- 2026-09-01 已只读审查旧 TCP 协议：每个包为“小端 4 字节总长度（含长度字段）+ 2 字节模块 ID + 2 字节协议 ID + 正文”；旧心跳请求为模块 1、协议 8，应答为模块 1、协议 9 且携带状态码。但旧连接流程要求先执行握手，握手包含平台、版本及设备标识，不能在未取得服务端确认的情况下假定它无副作用。微信 SDK 已具备二进制 `TCPSocket.write` 和 `onMessage` 能力；当前探针仍刻意未注册收包或发送。下一步需服务端负责人书面确认“握手+一次心跳”可用于测试，或提供独立 echo 测试契约（请求字节、期望响应、无副作用保证），然后才可扩展探针进行实际收发验证。
- 2026-09-01 负责人已明确授权当前服务器的真实登录验证。`ClientTcpConnectionProbe` 已扩展为仅在“本地直连 + `useRealGuestLogin`”均开启时执行一次“旧协议握手 → 取得随机密钥 → 旧协议游客登录 → 接收登录响应 → 关闭”的最小链路。地址、token、随机密钥及用户资料均不记录；日志只输出包长度、协议阶段和响应状态。Unity `2022.3.57f1c2` 批处理命令已执行，但被当前同版本 Editor 的项目锁阻止，未进入编译；日志为 `Logs/codex-tcp-real-login-compile.log`。未关闭负责人 Editor，待其在当前 Editor Console 确认无编译错误后进行 WebGL/微信小游戏人工验证。
- 2026-09-01 微信开发者工具存在高频“request 错误”日志，普通 Console 记录会被刷出缓冲区。探针已增加仅在真实登录测试开启时显示的微信原生结果弹窗，覆盖连接失败、异常数据、无效包、握手拒绝、登录成功/拒绝与登录超时；弹窗不显示地址、token、随机密钥或用户资料。第二次 Unity `2022.3.57f1c2` 批处理仍因打开的 Editor 项目锁而未编译，日志为 `Logs/codex-tcp-login-modal-compile.log`。待当前 Editor 自动编译确认后重新导出小游戏验证。
- 2026-09-01 已用两个负责人提供的候选端口进行真实登录测试；两次均出现“TCP 建连成功 → 已发送 62 字节旧协议握手 → `ECONNRESET`”，无握手响应。结论：小游戏的 TCP 建连与二进制发包均已验证；候选端口的服务端在解析/拒绝握手后主动断开，尚未验证登录或收包。探针弹窗已修正为在该状态显示“建连成功后被服务器断开”；当前批处理仍被已打开的 Editor 锁阻断，日志为 `Logs/codex-tcp-reset-message-compile.log`。不再盲测端口，等待服务端确认网关端口及握手兼容性。
- 2026-09-03 已重新核对 PC 端 `ClientGT.cs`、新 `ClientPF.cs`、`ClientPFResponser.cs`、`NetController.cs` 与微信 `ClientTcpConnectionProbe.cs`：微信当前发送 `CLPFGetHeroReq`（模块 2、协议 1），按新 `CLPFGetHeroAck` 顺序读取英雄数组与英雄队伍数组，现有日志已显示获取英雄成功。探针新增可选 `logDeviceIdentifier`，开启后仅打印设备来源、长度、首尾片段和 SHA-256 指纹，不打印完整设备值。Unity 批处理再次被已打开 Editor 锁阻止，日志为 `Logs/codex-tcp-device-log-compile.log`；需在当前 Editor 自动编译后导出验证。
- 2026-09-03 负责人明确授权内网临时查看完整机器码。探针新增默认关闭的 `logFullDeviceIdentifier`，仅在同时开启设备诊断时打印完整值；测试结束必须关闭并清理临时构建/日志，不得提交或用于正式包。批处理验证日志为 `Logs/codex-tcp-full-device-compile.log`，当前仍受已打开 Editor 项目锁影响。
- 2026-09-03 最新微信运行日志显示启动阶段通过 CDN 请求 `e112a2cd0d6ade4f.webgl.data.unityweb.bin.br` 并返回 404，随后 `start game fail: 404`；本地新导出文件存在于 `D:\Users\Administrator\Desktop\弹珠接入text_1\720\webgl`。此前“仅脚本改动无需上传 OSS”的判断不适用于当前 CDN 配置：WebGL 重新导出产生了新的带 hash 数据文件，必须上传该文件（及实际出现 404 的其他 CDN 文件）或清空 CDN 配置改用本地包。
- 2026-09-03 新日志已无 WebGL 资源 404，且出现 TCP 建连、握手、游客登录、`CLPFGetHeroReq` 发送及英雄回包解析日志，说明小游戏到服务器的游客 TCP 登录和 GetHero 数据链路已跑通。设备诊断显示 `SystemInfo.deviceUniqueIdentifier` 返回 `n/a`（长度 3）；探针已修正为将 `n/a`、`unknown`、`undefined`、全零及过短值视为占位值并回退 `PlayerPrefs.DeviceId`。批处理日志为 `Logs/codex-tcp-device-fallback-compile.log`，仍因打开 Editor 项目锁未进入编译；待 Editor 自动编译后重新导出验证回退值。
- 负责人新增长期决定：已有 UI 页面默认只写脚本，不新建 Page、UI 节点，不替换素材或改变 GUI 排版；场景结构修改必须得到明确授权。

- 已建立客户端目录/模块说明和独立场景；`ClientBootstrap` 默认注册本地模拟账户、货币、英雄与背包服务，未触碰既有街机启动链或网络层。
- `ClientShell` 已切换为“场景可编辑 UI”路径：运行时页面脚本仅负责数据绑定和导航；`Assets/Client/Editor/ClientShellUiBuilder.cs` 会在编辑器中生成 `ClientCanvas`、`MainPage`、`ProfilePage`、按钮与改名弹窗，避免运行时临时创建 UI。
- 首次执行 UI 生成工具时，Unity 2022.3.57f1c2 报出内置字体 `Arial.ttf` 已失效；现已改用其要求的 `LegacyRuntime.ttf`。该次生成在第一个文字节点前中断，需删除已生成的 `ClientCanvas` 后重新执行菜单。
- 场景首次运行曾因 `MainPage.OnEnable` 早于 `ClientBootstrap.Awake` 访问本地服务而报错；页面首轮绑定已改为 `Start`，重新启用页面时才在 `OnEnable` 刷新，消除了根节点启用顺序依赖。
- 主页与个人中心使用负责人提供的参考图作为临时构图底图，展示本地名称、ID、战力；主页可进入个人中心，个人中心支持返回与本地改名。未完成入口只显示待开发提示，不进入旧街机页面。
- 主页展示页已实现本地最小闭环：根据本地英雄拥有状态选择展示立绘、确认后写回 `IClientDataService`。展示资源位于 `UI/HomeDisplay`；第二名示例英雄未拥有，选择时应提示锁定。
- 主页面所有一级入口已由“待开发提示”改为实际导航。英雄和背包页展示本地模拟列表；其余入口先进入统一内容页并保留返回主页，待对应规则/交互稿细节实施后替换为独立页面。
- 负责人已确认主页面必须由独立可编辑的 Scene 节点构成。生成器已改为不使用 `HomeReference.png`，改为创建背景、主立绘、头像框、头像、顶部栏、战力栏、编队/弹珠卡、底部导航及独立入口节点；已有旧 Canvas 需由负责人删除后重建。
- 已定位主页素材未显示的直接证据：复制进工程的 PNG 默认 `textureType: 0`、`spriteMode: 0`，`LoadAssetAtPath<Sprite>` 会得到空对象。生成器现会在首次构建前将 `UI/MainPage/Sprites` 中的纹理统一设置为单图 Sprite 后再生成节点。
- 已进入 C2 英雄/图鉴页面：拆分素材已复制至 `Assets/Client/UI/HeroPage/Sprites`，不使用整张参考图；英雄/图鉴页签、全/火/水/风筛选、已拥有/未拥有示例英雄卡和返回主页已可演示。
- 英雄详情页已使用 `Assets/Client/UI/HeroDetailPage/Sprites` 的拆分素材直接写入 `ClientShell`：`HeroDetailPage`、背景、战力区、属性区、技能区、文案和返回按钮均为可单独选中的场景节点。点击已拥有英雄会由 `ClientPageNavigator` 打开详情页，并绑定本地名称、等级、战力；返回会回到英雄页。技能、装备和真实数值仍待确认。
- 英雄页卡牌视觉已补齐为独立节点：品质框、属性图标、三颗星级和未拥有遮罩/文案均可在 Hierarchy 调整。现有全/火/水/风筛选、已拥有英雄详情入口和未拥有提示保持不变；尚未提供独立英雄立绘，因此继续复用已有的英雄卡素材。
- 背包页已直接写入 `ClientShell`：`BackpackPage`、筛选按钮、材料槽、装备槽、状态文案和返回按钮均为可编辑场景节点。主页底部背包入口进入该页；数据来自 `IClientDataService.GetInventory()`，支持全部/材料/装备筛选及条目选中提示。背包专用切图、道具名称/图标和出售/合成/装备规则待确认。
- 负责人已将优先级调整为“先完成主页所有可点击一级界面”。扭蛋、商城、邮箱、排行榜、活动、任务、公告已从统一 `FeaturePage` 拆成独立的 `GachaPage`、`ShopPage`、`MailPage`、`RankPage`、`ActivityPage`、`TaskPage`、`NoticePage` 场景根节点；每页有独立标题、状态区和返回主页按钮，并由共享导航控制器分发。当前仅完成本地展示/返回闭环，业务数据和规则待对应资料确认。
- 客户端 UI 已按统一 Canvas 架构重构：`ClientCanvas` 是唯一 UI 宿主，包含 `PagesLayer`、`PopupLayer`、`SystemLayer`。现有所有客户端页面根节点已迁入 `PagesLayer`；`ClientUiPageId`、`ClientUiPage` 和 `ClientUiNavigator` 负责页面标识、生命周期（Enter/Pause/Resume/Exit）与返回栈。旧 `ClientPageNavigator` 已在 `ClientShell` 停用，不再参与运行时导航；所有现有返回按钮由 `ClientUiBackButton` 在运行时接入统一返回栈。
- 个人中心、主页展示、铭牌、英雄详情已接入新页面栈：个人中心的“主页展示”与“个性化”分别进入展示页和 `BadgePage`；铭牌页使用 `UI/BadgePage/Sprites` 中的拆分素材，提供三项选择、预览和本地确认；英雄详情新增元素、星级、拥有状态绑定。真实解锁、装备/技能、铭牌服务器同步仍待确认。
- 2026-08-28 已针对运行中反馈补强统一导航：主页每次显示都会重新接管所有一级入口（含头像/资料），返回按钮每次启用都会重接 `ClientUiNavigator.Back`；返回栈因域重载或 Inspector 直接激活而为空时会兜底回到主页。待 Unity Editor 自动重编译后的人工验证与关闭 Editor 后的命令行编译。
- 2026-08-29 已完成扭蛋与商城页的本地模拟闭环：`ClientGachaPool`/`TryDrawGacha` 演示单抽、十连、钻石不足、奖励结果和英雄解锁；`ClientShopProduct`/`TryPurchaseShopProduct` 演示商品、余额校验、限购与奖励入背包。`ApplyGachaPage`、`ApplyShopPage` 已将二者写入 `ClientShell` 的 `PagesLayer`，页面根及其背景、标题、货币、商品/抽取按钮、结果/状态和返回按钮均为可编辑场景节点。正式概率、保底、运营周期、商品价格、限购与货币规则仍待产品确认。
- 2026-08-29 已完成邮箱页本地模拟闭环：`ClientMail`、读取和附件领取均通过 `IClientDataService` 处理；`MailPage` 可展示未读/已领状态、阅读详情、领取附件、重复领取提示和空列表节点。`ApplyMailPage` 已将页面节点写入 `PagesLayer`；真实邮件协议、有效期、批量领取和附件类型仍待产品确认。
- 2026-08-29 已完成排行榜页本地快照展示：榜单通过 `ClientRankEntry`/`GetRankEntries` 提供，页面显示前三名、自己的排名与刷新状态；真实赛季、奖励、跨服与实时刷新待服务端契约。首次 `ApplyRankPage` 在 `Logs/codex-rank-page-apply.log` 因未先添加 `ClientRankPage` 组件出现空引用，已按定位补齐组件创建；重试成功并写入 `PagesLayer`。
- 负责人已确认后续无素材页面的视觉实施规则：先按交互草图生成可编辑的完整页面版式骨架，再由负责人替换节点素材；不再使用通用纯色占位页作为页面成品。草图已完成视觉核对，活动、任务与公告等后续页面将沿用其“顶部标题/页签、主体内容卡或列表、状态信息、固定底部操作/返回”的结构。
- 2026-08-29 已完成活动页本地模拟闭环并按草图结构落地：`ClientActivity`/`TryClaimActivity` 支持活动状态、进度、领取成功、进度不足与重复领取；`ActivityPage` 含标题栏、页签、活动卡、详情面板、领取区和返回入口，均为 `PagesLayer` 下可替换的场景节点。运营日历、真实活动条件、奖励配置和红点规则待产品/服务端确认。
- 2026-08-29 已完成任务页本地模拟闭环并按草图结构落地：`ClientTask`/`TryClaimTask` 支持任务进度、领取成功、未完成与重复领取；`TaskPage` 含标题栏、每日/成长页签、任务卡、详情面板、领取区和返回入口，均为 `PagesLayer` 下可替换的场景节点。日/周重置、真实任务条件、奖励配置和红点规则待产品/服务端确认。
- 2026-08-29 已完成公告页本地模拟闭环并按草图结构落地：`ClientNotice`/`TryReadNotice` 支持公告列表、未读状态、阅读详情和空态；`NoticePage` 含标题栏、公告列表区、详情区和返回入口，均为 `PagesLayer` 下可替换的场景节点。真实运营后台、公告有效期、排序与推送规则待产品/服务端确认。
- 2026-08-29 已完成英雄培养最小闭环：英雄详情新增“培养”入口，`HeroEnhancePage` 可展示等级、战力、金币/材料消耗并执行 `TryUpgradeHero`；成功时扣除本地资源、提升英雄及主页战力，覆盖材料不足、金币不足和本地等级上限。正式升级公式、材料类型、等级上限、升星与装备规则待产品确认。
- 2026-08-29 已补齐背包本地锁定交互：选择材料或装备后可切换锁定/解锁，状态经 `TrySetInventoryItemLocked` 写回本地数据并显示在条目文本；`LockToggleButton` 已写入现有 `BackpackPage`。装备穿戴、出售/分解、批量操作和真实锁定协议待产品/服务端确认。
- 2026-08-29 已在英雄培养页补齐本地升星：`TryStarUpHero` 会消耗本地金币与材料、提升星级及英雄/主页战力，并覆盖资源不足与星级上限。`StarUpButton` 已写入 `HeroEnhancePage`；正式升星条件、消耗、成功率与星级效果待产品确认。
- 2026-08-29 已在背包补齐本地装备/卸下：选中演示装备后可装备给本地演示英雄或卸下，锁定装备会被阻止操作；装备状态经 `EquippedHeroId` 写回并显示在条目中。装备槽位、属性、替换确认与战斗数值影响不在当前授权范围。
- 2026-08-29 已打通扭蛋与英雄图鉴的本地拥有状态：扭蛋获得第二名英雄后，英雄页会移除锁定遮罩、显示英雄名并允许进入详情；未获得时保持锁定提示。该同步使用同一 `IClientDataService` 英雄数据，不新增临时状态。
- TeamButton/GachaButton 作为独立预制体放入 MainPage 后，导航由 `ClientHomePage` 按 Root 名称在运行时绑定，避免替换预制体导致生成器一次性 Button 回调丢失。
- 下一检查点：按统一页面架构逐页补齐一级页面已确认交互；新增弹窗统一放入 `PopupLayer`，全局提示/加载统一放入 `SystemLayer`，不再新增独立导航器或运行时业务 UI 根。
- 2026-08-29 缺口盘点：主页的编队与弹珠入口目前复用英雄页，尚未形成独立的编队/弹珠页面；扭蛋缺卡池切换、结果弹窗和历史，商城缺商品详情/确认，邮箱缺批量领取/筛选，活动/任务/公告缺真实页签切换与筛选，英雄/背包缺装备详情/替换确认和养成预览，通用 `PopupLayer`/`SystemLayer` 的弹窗、红点、加载尚未实现。后续按当前计划逐项补齐；服务端、支付、战斗、真实运营配置仍不在范围内。
- 2026-08-29 已完成缺口计划的第一批：主页“编队”与“弹珠”已不再复用英雄页，分别进入 `FormationPage` 和 `MarblePage`；前者支持选择已拥有英雄进入单出战槽位，后者支持选择已拥有弹珠装备，均使用 `IClientDataService` 本地保存状态。队伍人数、编队属性、弹珠战斗效果与真实资源解锁仍待产品/服务端契约。
- 2026-08-29 已补充扭蛋抽取历史：`GetGachaHistory` 记录本地抽取次数，`GachaPage` 显示“暂无抽取记录/已抽取次数”；历史节点已写入页面，卡池切换与结果弹窗仍待完成。
- 2026-08-29 已实现扭蛋结果弹窗代码：`ClientGachaResultPopup` 归属 `PopupLayer`，单抽/十连成功后展示奖励并可关闭；`ApplyGachaResultPopupPass` 已准备将弹窗节点写入 `ClientShell`，待 Unity 批处理完成场景写入确认。
- 2026-08-29 扭蛋结果弹窗已实际写入 `ClientShell/ClientCanvas/PopupLayer/GachaResultPopup`；正确版本 Unity 2022.3.57f1c2 批处理成功，日志为 `Logs/codex-gacha-result-popup-apply-57.log`。
- 2026-08-29 已补充邮箱批量领取与筛选逻辑：`IClientDataService.ClaimAllMails`、`ClientMailPage.ShowAll/ShowUnread/ClaimAll` 覆盖全部/未读和一键领取；场景按钮由 `ApplyMailBatchAndFilterPass` 写入，待 Unity 批处理验证。
- 2026-08-29 已补充活动/任务“进行中/已领取”筛选逻辑及场景写入器 `ApplyActivityTaskFilterPass`，待 Unity 批处理验证。
- 2026-08-29 已完成商城商品详情与二次确认逻辑：首次点击商品只选中并展示价格/奖励，`ConfirmPurchase` 才执行扣款和入包；`ApplyShopConfirmPass` 负责写入详情与确认按钮。
- 2026-08-29 已补充背包道具详情展示：选中材料/装备时显示数量、装备状态及待确认属性说明；`ApplyBackpackDetailPass` 负责写入详情节点。
- 2026-08-29 核心本地交互已完成场景落地：`ApplyCoreInteractionScenePass` 已将商城详情/确认、邮箱全部/未读/一键领取、活动与任务筛选、公告全部/未读、背包详情一次性写入 `ClientShell`。活动和任务复用现有顶部页签节点并附加按钮回调，不创建覆盖节点。
- 2026-08-29 已完成 `SystemLayer` 统一反馈：`UiFeedback` 包含可编辑的 Toast 与 Loading 节点；商城购买失败/成功及邮箱一键领取接入 Toast。主页的邮件、活动、公告入口已增加 `RedDot`，由本地数据的未读/可领取状态刷新。
- 2026-08-29 已补齐核心防误触和养成预览：装备首次点击穿戴只进入确认态，再次点击才执行；英雄培养页显示升级/升星后等级和星级预览。本地演示仅有一件装备，真实多装备替换规则仍待产品配置。
- 2026-08-29 已实现五个底部导航的选中视觉代码：个人中心/弹珠/主页/背包/活动未选中使用各自 `2.png` 白色图标，选中使用 `1.png` 黄色图标，并显示独立 `*_SelectedFrame` 高亮底框；主页默认选中，跳转与返回主页均会刷新选中项。场景写入入口为 `ApplyBottomNavigationSelectionPass`。

## 验证与风险

- 2026-08-31 已执行 Unity `2022.3.57f1c2` 批处理场景写入与编译，日志为 `Logs/codex-loading-page-apply.log`；退出日志为 `Exiting batchmode successfully now!`，包含 `Tundra build success` 和加载页写入成功信息，未检出本轮 `error CS` 或异常。仍需负责人打开场景进入 Play Mode，手动提供一个实现 `IClientResourceLoadSource` 的加载器并验证视觉节奏。

- 铭牌素材首次导入尝试未写入资源：PowerShell 对 `Copy-Item -LiteralPath "...\\铭牌\\*"` 不解析通配符，报路径不存在；目标 `Assets/Client/UI/BadgePage/Sprites` 保持为空。已定位为复制命令参数问题，后续仅以明确目录枚举重试。
- 个人中心/铭牌/英雄详情场景写入首次批处理未执行：`Logs/codex-personal-pages-apply.log` 报“another Unity instance is running with this project open”。当前有一个由 Hub 启动的主 Unity Editor 及两个其子进程，均不得由自动化结束；素材已复制至 `Assets/Client/UI/BadgePage/Sprites`，代码尚待该 Editor 关闭后执行 `ApplyPersonalCenterPages` 编译与场景写入。
- 负责人关闭 Editor 后，已执行 `ClientShellDirectUpdater.ApplyPersonalCenterPages`：退出码 `0`，场景已保存，日志为 `Logs/codex-personal-pages-apply-retry.log`；日志包含 `Tundra build success`、个人中心/展示/铭牌/英雄详情写入成功和 `Exiting batchmode successfully now!`，未检出本轮编译错误或异常。
- 2026-08-28 已以 Unity `2022.3.57f1c2` 批处理执行 `ClientShellDirectUpdater.ApplyHeroDetailPage`：脚本编译图构建成功，退出码 `0`，日志为 `Logs/codex-hero-detail-apply.log`；日志包含 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。既有项目警告仍存在：`Assets/Main/Init.cs` 的 CS1998、`Assets/Editor/AddBuildMapUtility.cs` 的 CS0108、旧 AssetBundle API 的 CS0618 与 HybridCLR 编辑器代码的 CS0162。
- 2026-08-28 已以同一 Unity 批处理执行 `ClientShellDirectUpdater.ApplyBackpackPage`：退出码 `0`，场景已保存，日志为 `Logs/codex-backpack-apply.log`；日志包含 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 英雄卡视觉增补的首次批处理在 `Logs/codex-hero-visual-pass.log` 中止：场景加载后 `GameObject.Find("HeroPage")` 无法找到被禁用的页面根。已定位为 Unity 的活动对象查找限制，改为包含禁用对象的组件查找后再继续；首次命令未写入场景。
- 修正后已执行 `ClientShellDirectUpdater.ApplyHeroVisualPass`：退出码 `0`，场景已保存，日志为 `Logs/codex-hero-visual-pass-retry.log`；日志包含 `Tundra build success` 与 `Exiting batchmode successfully now!`，未检出本轮编译错误或异常。
- 2026-08-28 已执行 `ClientShellDirectUpdater.ApplyPrimaryPages`：退出码 `0`，场景已保存，日志为 `Logs/codex-primary-pages-apply.log`；日志包含 `Tundra build success` 与 `Exiting batchmode successfully now!`，未检出本轮编译错误或异常。
- 2026-08-28 已执行 `ClientShellDirectUpdater.ApplyCanvasUiArchitecture`：退出码 `0`，场景已保存，日志为 `Logs/codex-client-ui-architecture.log`；日志包含 `Tundra build success` 与 `Exiting batchmode successfully now!`，未检出本轮编译错误或异常。
- 2026-08-28 已在 Editor 释放后以 Unity `2022.3.57f1c2` 批处理编译导航修复：退出码 `0`，日志为 `Logs/codex-navigation-fix-compile.log`；日志含 `Tundra build success` 与 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。仍待负责人在 Play Mode 走通头像 → 个人中心 → 主页展示/个性化（铭牌）及返回。
- 2026-08-29 已以同一 Unity 批处理分别执行 `ClientShellDirectUpdater.ApplyGachaPage` 与 `ApplyShopPage`：退出码均为 `0`，日志为 `Logs/codex-gacha-page-apply.log`、`Logs/codex-shop-page-apply.log`；均含 `Tundra build success`、页面写入成功与 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 已以同一 Unity 批处理执行 `ClientShellDirectUpdater.ApplyMailPage`：退出码 `0`，日志为 `Logs/codex-mail-page-apply.log`；包含 `Tundra build success`、邮箱页面写入成功与 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 已以同一 Unity 批处理重试执行 `ClientShellDirectUpdater.ApplyRankPage`：退出码 `0`，日志为 `Logs/codex-rank-page-apply-retry.log`；包含 `Tundra build success`、排行榜页面写入成功与 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 已以同一 Unity 批处理执行 `ClientShellDirectUpdater.ApplyActivityPage`：退出码 `0`，日志为 `Logs/codex-activity-page-apply.log`；包含 `Tundra build success`、活动页面写入成功与 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 已以同一 Unity 批处理执行 `ClientShellDirectUpdater.ApplyTaskPage`：退出码 `0`，日志为 `Logs/codex-task-page-apply.log`；包含 `Tundra build success`、任务页面写入成功与 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 已以同一 Unity 批处理执行 `ClientShellDirectUpdater.ApplyNoticePage`：退出码 `0`，日志为 `Logs/codex-notice-page-apply.log`；包含 `Tundra build success`、公告页面写入成功与 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 已以同一 Unity 批处理执行 `ClientShellDirectUpdater.ApplyHeroEnhancePage`：退出码 `0`，日志为 `Logs/codex-hero-enhance-page-apply.log`；包含 `Tundra build success`、英雄培养页面与详情入口写入成功及 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 已以同一 Unity 批处理执行 `ClientShellDirectUpdater.ApplyBackpackInteractionPass`：退出码 `0`，日志为 `Logs/codex-backpack-interaction-apply.log`；包含 `Tundra build success`、背包交互节点写入成功及 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 已以同一 Unity 批处理编译英雄拥有状态联动：退出码 `0`，日志为 `Logs/codex-hero-ownership-sync-compile.log`；包含 `Tundra build success` 与 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 已以同一 Unity 批处理执行 `ClientShellDirectUpdater.ApplyHeroEnhanceStarPass`：退出码 `0`，日志为 `Logs/codex-hero-enhance-star-apply.log`；包含 `Tundra build success`、英雄培养升星节点写入成功及 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 已以同一 Unity 批处理再次执行 `ClientShellDirectUpdater.ApplyBackpackInteractionPass` 写入装备切换按钮：退出码 `0`，日志为 `Logs/codex-backpack-equip-apply.log`；包含 `Tundra build success`、场景写入成功及 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 已以同一 Unity 批处理执行 `ClientShellDirectUpdater.ApplyFormationAndMarblePages`：退出码 `0`，日志为 `Logs/codex-formation-marble-apply.log`；包含 `Tundra build success`、编队与弹珠页面写入成功及 `Exiting batchmode successfully now!`，未检出本轮 `error CS`、`Exception` 或编译失败。
- 2026-08-29 扭蛋历史首次编译在 `Logs/codex-gacha-history-apply.log` 因记录插入错误位置出现 `CS0103`，已按日志定位修正；重试 `Logs/codex-gacha-history-apply-retry.log` 退出码 `0`，场景写入与 Unity 编译通过。
- 2026-08-29 扭蛋结果弹窗批处理首次启动触发 Unity 资源导入，期间未到达执行方法日志；Unity 进程随后退出但日志无最终退出标记，需在缓存稳定后重新执行并核对 `Tundra build success`/`Exiting batchmode successfully now!`。本轮未结束任何负责人 Unity 进程。
- 2026-08-29 使用正确 Unity 2022.3.57f1c2 完成纯批处理编译，`Logs/codex-client-full-compile-57.log` 含 `Tundra build success` 与 `Exiting batchmode successfully now!`，未检出 `error CS`。邮箱/活动/任务/公告新增场景控件写入器因 Unity 启动竞争尚未得到独立落盘日志，逻辑代码已进入编译输入，需下一轮在 Unity 空闲时执行对应方法。
- 2026-08-29 核心交互第二轮纯编译 `Logs/codex-core-interaction-compile-2.log` 通过；商城/背包场景写入尝试未生成独立日志，不能视为已落盘，需 Unity 空闲后执行 `ApplyShopConfirmPass`、`ApplyBackpackDetailPass`、`ApplyMailBatchAndFilterPass`、`ApplyActivityTaskFilterPass`、`ApplyNoticeFilterPass`。
- 2026-08-29 已用 Unity 2022.3.57f1c2 成功执行核心交互场景写入，`Logs/codex-core-interaction-scene-apply.log` 含 `Tundra build success`、节点写入成功及 `Exiting batchmode successfully now!`；系统反馈与红点写入日志为 `Logs/codex-system-feedback-apply.log`、`Logs/codex-home-red-dot-apply.log`，均通过。最终代码编译为 `Logs/codex-core-local-final-compile.log`，未检出 `error CS` 或 `Exception`。
- 2026-08-29 底部导航选中状态首次写入未执行：`Logs/codex-bottom-navigation-selection-apply.log` 明确显示 `another Unity instance is running with this project open`，Unity 在加载工程前退出，未产生场景或编译改动。等待负责人关闭该工程的 Unity Editor 后，仅执行一次 `ApplyBottomNavigationSelectionPass` 并验证日志；不自动结束负责人的进程。
- 2026-08-29 负责人关闭 Unity Editor 后已成功执行 `ApplyBottomNavigationSelectionPass`：`Logs/codex-bottom-navigation-selection-apply.log` 含 `Tundra build success`、五个底部导航选中状态写入成功及 `Exiting batchmode successfully now!`。`ProfileButton`、`MarbleButton`、`HomeButton`、`BackpackButton`、`ActivityButton` 均有白色未选中图标、黄色选中图标和可编辑的 `*_SelectedFrame` 高亮底框。
- 2026-08-29 已将负责人提供的 `D:\Users\Administrator\Desktop\UI\切图(11)` 原样复制为独立素材目录 `Assets/Client/UI/切图(11)`，共 358 个 PNG，未覆盖现有页面素材或场景引用。该目录等待 Unity 导入完成后按页面逐项替换；使用单张切图前需设置为 `Sprite (2D and UI)`。
- `BUG-009` 至 `BUG-013` 和 `RISK-010`、`RISK-011` 仍是既有微信/WebGL 调试风险，后续只有接入旧启动/资源链时才按编号恢复。

## 待确认

1. 首个优先页面（建议先做大厅/个人中心或英雄/背包）。
2. UI 适配基线、字体/图标来源、最终美术资源和性能预算。
3. 养成、装备、货币与奖励的真实数值/规则来源；确认前仅实现可配置的本地示例数据。

## 2026-09-02 资源清理检查点

- 已检查微信小游戏导出目录 `D:\\Users\\Administrator\\Desktop\\弹珠接入text_1\\720\\minigame`：当前源文件总量约 31.73 MB，超过微信 30 MB 上传限制；主要体积来自 data-package 约 17.65 MB 与 wasmcode 约 12.01 MB，不能直接删除。
- 已反查 `Assets/Effects ALL/unity tex/8K.png` 的 GUID `701e45d0f28ccd34580e3f64324a7d17`，在工程场景、Prefab、脚本、配置中未发现外部引用；已将该 PNG 及其 `.meta` 从工程移至 `D:\\Users\\Administrator\\Desktop\\pinball-resource-quarantine-20260902\\Effects ALL\\unity tex`，保留可恢复副本，未直接永久删除。
- 其他大体积候选（战斗 UI 字体/图集等）仍有明确 Prefab 或 ClientShell 引用，未清理。旧导出包未改变，需重新导出后才能验证体积和运行效果。
- 2026-09-02 已将 `Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset` 的 `ProjectConf.assetLoadType` 固定为 `0`（CDN）。SDK 会将 `game.js` 的资源根替换为 `CDN/Assets`，不再把首资源包放入小游戏包；重新导出前需确认 CDN 对应目录已上传并可公开 HTTPS 访问。
- 2026-09-02 重新导出验证：`minigame` 已降至约 15.75 MB，`game.js` 已生成 `DATA_CDN` 且 `loadDataPackageFromSubpackage: false`；`data-package` 仅保留空占位文件。对当前 CDN 地址探测数据文件返回 404，说明资源尚未上传或路径不匹配，暂不能直接预览验证。
- 2026-09-02 微信日志进一步确认 CDN 已生效但数据文件返回 404；日志请求文件名为 `97be491cfe81277d.webgl.data.unityweb.bin.br`，与当前检查到的新导出文件名 `a81346d6b223f67d.webgl.data.unityweb.bin.br` 不一致，需重新导入最新导出目录或按最新 `game.js` 的 `DATA_FILE_MD5` 上传对应文件。
- 2026-09-02 已完成 `2026-08-28-720` 的 YooAsset 增量构建；本地输出目录包含一套运行时清单 `PackageManifest_DefaultPackage_2026-08-28-720.bytes/.hash` 与 `PackageManifest_DefaultPackage.version`。同目录的 `PackageManifest_*.json`、`BuildReport_*.json`、`buildlogtep.json` 是报告/日志，不作为运行时清单上传。
- 2026-09-02 OSS 根目录在线探测：`PackageManifest_DefaultPackage.version/.bytes/.hash` 返回 200，但最新数据文件 `97be491cfe81277d.webgl.data.unityweb.bin.br` 与 bundle（含 `65fe2931113959a247385aecb582299e.bundle`）返回 404；说明只更新了清单，仍需上传数据文件和全部 bundle。
- 2026-09-02 再次探测：`PackageManifest_DefaultPackage.version/.bytes/.hash` 与最新数据文件 `97be491cfe81277d.webgl.data.unityweb.bin.br` 已返回 200；`65fe2931113959a247385aecb582299e.bundle`、`a3c400ea8fa068e432ee745022f0711e.bundle`、`bf71706251bf4132766a28b15d0b446c.bundle` 仍返回 404，bundle 尚未全部位于 CDN 根目录。
- 2026-09-02 更正验证：上述三个 404 哈希属于旧日志，不在当前构建目录，不能作为当前上传判断。当前构建清单中 HotFix bundle 的 FileHash 为 `f2f6a09db1e4930193e8b772ff1d8dd1`；该 bundle、其他当前构建 bundle 示例、清单和 `97be491cfe81277d.webgl.data.unityweb.bin.br` 均已从 OSS 根目录返回 200。
- 2026-09-02 最新微信日志仍请求 `c233ed8e38c5d8c785e8244969fba8c7.bundle` 并返回 404，随后因 HotFix.NetController 未加载在 15 秒超时。已核对该文件既不在当前 `2026-08-28-720` 本地输出（47 个 bundle），也不在 OSS；当前本地与 OSS 的 version、manifest bytes、manifest hash 三件套一致，因此优先怀疑微信开发者工具仍使用旧资源缓存或运行时加载了旧清单引用。处理顺序：清除开发者工具全部缓存并重新导入最新 `minigame`；若仍请求 `c233...`，需回查生成该旧引用的导出/清单来源，不应伪造或上传本地不存在的 bundle。
- 2026-09-02 15:43 新日志确认 `c233...` 对应 `assets_hotupdateresources_dll_webgl_mscorlib_dll.bundle`。当前构建报告/清单对应的 mscorlib 文件为 `26efc0d84c5d4b11e39fb99673983285.bundle`，本地和 OSS 均可访问（200，1,165,915 bytes），而 `c233...` 本地不存在且 OSS 为 404。由此当前运行实例实际使用的是旧清单/缓存引用；无需修改 TCP 或 NetController 代码。若清除缓存并重新导入后仍请求 `c233...`，应先关闭并重新打开微信开发者工具、删除该项目后从最新 `minigame` 目录重新导入；在坚持 720 版本时暂不上传伪造文件或改版本号。
- 2026-09-02 重新导出后复核 `D:\Users\Administrator\Desktop\弹珠接入text_1\720\minigame`：根目录 `game.js` 为 8,084 bytes、`game.json` 为 654 bytes，`wasmcode/de4eef4dc8d1d159.webgl.wasm.code.unityweb.wasm.br` 为 15,151,684 bytes，已不再是此前的 0/4 字节占位文件；可重新导入微信开发者工具验证。`wasmcode/game.js` 仍为 0 bytes，暂不作为 wasm 二进制完整性判断依据。
- 2026-09-02 16:11 新日志确认 wasm 分包已成功下载并编译；当前失败变为 CDN 首包数据 404。导出目录生成的新文件是 `D:\Users\Administrator\Desktop\弹珠接入text_1\720\webgl\64e29d07d6223d9d.webgl.data.unityweb.bin.br`（18,045,804 bytes），运行时请求同名文件，但 OSS 720 根目录仍只有旧的 `97be491cfe81277d.webgl.data.unityweb.bin.br`（200），`64e29...` 返回 404。需将新 `64e29...` 文件上传到同一 OSS 根目录后再预览；这一步完成前 HotFix/NetController 不会执行。
- 2026-09-02 随后复核 OSS：`64e29d07d6223d9d.webgl.data.unityweb.bin.br` 已返回 HTTP 200，Content-Length 18,045,804，说明新导出的首包数据已上传成功。可清除微信开发者工具缓存后重新预览，继续观察 HotFix bundle 与 `GetIpPort` 日志。
- 2026-09-02 16:18 新日志确认 wasm 与首包数据均已成功下载/解压，但首包初始化后仍按 720 清单请求旧的 `c233ed8e38c5d8c785e8244969fba8c7.bundle`（mscorlib）并 404。当前本地 `StreamingAssets`/构建报告对应 mscorlib 为 `26efc0d84c5d4b11e39fb99673983285.bundle`，说明重新导出的 data 包与当前 YooAsset bundle 清单未同步；需按“先构建 720 AssetBundle，再导出小游戏，再上传同一批 data、manifest、bundle”的顺序重新生成，不能只替换 OSS data 文件或 TCP 代码。
- 2026-09-02 重新构建/导出并上传后复核 OSS：当前最新 data 文件 `b4b7d574128ba5ce.webgl.data.unityweb.bin.br` 返回 200，大小 18,045,937 bytes；3 个运行时清单和当前构建目录全部 47 个 bundle 共 51 个对象均返回 200，无 404。可清除微信开发者工具全部缓存后重新预览，观察是否进入 HotFix 与 NetController。
- 2026-09-02 16:54 最新预览已使用新 `b4b7...data` 并完成资源包下载/初始化，但随后仍请求 `c233...bundle` 404；说明运行时实际加载的 YooAsset 清单仍包含旧 mscorlib 映射，`GetIpPort` 尚未执行。当前应停止修改网络代码，重新核对“同一批 AssetBundle 构建输出→WebGL 导出→manifest/data 上传”的完整链路，或用新的唯一包版本/路径进行缓存隔离。
- 2026-09-02 17:37 新日志确认清单已切换到当前 mscorlib `26efc0d84c5d4b11e39fb99673983285`，但 bundle 请求路径为 `StreamingAssets/yoo/DefaultPackage/<hash>.bundle`；此前仅上传 720 根目录 bundle，子路径样例返回 404。需按 WebGL 导出目录保留相对路径，将 `D:\Users\Administrator\Desktop\弹珠接入text_1\720\webgl\StreamingAssets\yoo\DefaultPackage\` 下全部 `.bundle` 上传到 OSS `.../2026-08-28-720/StreamingAssets/yoo/DefaultPackage/`，清缓存后重试。
- 2026-09-02 随后复核 OSS 子目录：`StreamingAssets/yoo/DefaultPackage/` 下本地对应的 47 个 bundle 全部返回 HTTP 200；其中 mscorlib `26ef...` 大小 1,165,915 bytes，HotFix `f2f6...` 大小 267,636 bytes。资源路径现已完整，下一步只需清缓存并重新预览。
- 2026-09-02 最新上传复核：当前本地包版本为 `2026-08-28-720-r2`；OSS 根目录的 `PackageManifest_DefaultPackage.version`、`PackageManifest_DefaultPackage_2026-08-28-720-r2.bytes/.hash` 和最新 data `19a66249237aa142.webgl.data.unityweb.bin.br` 均返回 HTTP 200，47 个 `StreamingAssets/yoo/DefaultPackage/*.bundle` 也全部返回 HTTP 200。上传路径与当前运行时请求路径已匹配。
- 2026-09-02 R3 上传复核：本地版本为 `2026-08-28-720-r3`；OSS 根目录 version、r3 bytes/hash、最新 data `7a69f3891b08fea6.webgl.data.unityweb.bin.br` 均返回 200，大小分别为 17、7226、32、18051766 bytes；`StreamingAssets/yoo/DefaultPackage/` 下 47 个 bundle 全部返回 200。R3 文件路径完整，可清缓存后预览握手。
- 2026-09-02 微信日志已进入真实 TCP：网关发现成功、`TCPSocket.connect` 成功、握手请求已发送；服务器返回握手状态码 1（无法识别的平台），随后关闭连接。已将 `Assets/Scripts/GameLogic/GameController.cs` 的 `RuntimePlatform.WebGLPlayer` 映射为协议平台编号 6；Unity 批处理编译未执行成功，因当前工程已有 Unity 实例占用项目（日志 `Logs/codex-platform-webgl-compile.log`），未结束负责人进程。
- 2026-09-03 网络链路现状：微信正式流程使用 `NetController.GetIpPort()` 的 WebGL `UnityWebRequest` 获取网关地址，不使用直连测试参数；随后由 `ClientTcpConnectionProbe` 通过微信 `TCPSocket` 执行 ClientGT 握手、游客登录，再发送 ClientPF `CLPFGetHeroReq`（模块 2/协议 1）并解析 `CLPFGetHeroAck`（模块 2/协议 2）。最新日志已证明网关获取、TCP 建连、游客登录和 GetHero 收包均成功；开发者工具的 `SystemInfo.deviceUniqueIdentifier` 返回 `n/a`，探针已回退到 `PlayerPrefs.DeviceId`，并保留临时设备诊断打印。WebGL 导出产生新 data/wasm hash 时，必须同步上传 OSS；YooAsset 资源本身未变时无需单独重建。
- 2026-09-03 最新一次设备回退验证：HTTP 网关、TCP 建连、握手及游客登录请求均执行；设备值已回退为 32 位 `PlayerPrefs.DeviceId`，但登录请求后未收到可识别的 `LoginAck`，服务器关闭连接，因此没有发送 `CLPFGetHeroReq`。探针新增收包长度、模块/协议号及关闭阶段日志，用于区分服务器无回包与协议号不匹配；重新导出后需提供该段完整日志。Unity 批处理日志 `Logs/codex-tcp-gethero-diagnose-compile.log` 仍受打开 Editor 项目锁影响。
- 2026-09-03 登录回归根因已确认：服务器可接受 WebGL `SystemInfo.deviceUniqueIdentifier` 原始值 `n/a`（此前 23 字节游客登录请求后成功进入 GetHero），但切换为 32 位 `PlayerPrefs.DeviceId` 后登录包变为 63 字节并被服务器关闭。探针默认恢复发送原始值以保持已验证链路；新增 `useFallbackDeviceIdentifierForLogin` 仅供服务端确认兼容后试用，默认关闭。Unity 批处理日志 `Logs/codex-tcp-login-device-compat-compile.log` 仍受打开 Editor 项目锁影响。
- 2026-09-03 已在 PC 端 `NetController.SendLoginPlatformReq` 发送游客登录前增加临时 device 值及长度日志，用于与微信端对比；未改动登录类型、CA3 加密或协议字段。Unity 批处理验证日志为 `Logs/codex-pc-device-log-compile.log`，仍受当前打开的 Editor 项目锁影响。测试后应移除该完整 device 日志。
- 2026-09-03 PC 日志显示成功登录所用 device 为 40 位十六进制；微信使用 32 位 `PlayerPrefs.DeviceId` 时登录被服务器断开。探针回退逻辑已调整为生成/复用 40 位十六进制 SHA-1 值，旧 32 位缓存自动替换；仍需开启 `useFallbackDeviceIdentifierForLogin` 后重新导出验证。Unity 批处理日志为 `Logs/codex-device-40hex-compile.log`，仍受 Editor 项目锁影响。
- 2026-09-03 服务端日志确认存在游客设备/会话处理：32 位设备值的两次握手后出现 `can't find client session ... OnRecv_PFGTLoginAck`；PC 40 位设备值返回 `errcode:0` 并分配用户 ID；WebGL `n/a` 返回 `errcode:0` 并分配用户 ID；WebGL 40 位设备值也返回 `errcode:0` 并分配用户 ID。结论：服务端接受 40 位设备值和 `n/a`，32 位值不兼容；服务端日志尚未包含对应 `GetHeroReq` 处理记录，需继续核对登录应答后的连接/英雄请求。
- 2026-09-03 负责人决定不再使用 `n/a` 或随机 40 位设备值；微信探针登录设备字段改为固定纯英文字母常量 `WebGLMiniGameDeviceIdentifierPinballNode`（40 字符、无符号），不再读取 `SystemInfo`/`PlayerPrefs`。旧回退开关保留但已标记废弃，不参与设备值生成。Unity 批处理日志为 `Logs/codex-fixed-device-compile.log`，仍受打开 Editor 项目锁影响；需重新导出并验证固定值登录及 GetHero 收发。
2026-09-03 编译修复：`Assets/Client/Runtime/ClientTcpConnectionProbe.cs` 原文件存在多处语法截断，导致 CS1622。已恢复为可编译的 TCP 网关/握手/收包最小结构，固定设备值保持不变；完整游客登录与 GetHero 协议发送需在 Unity 编译通过后依据现有 ClientGT/ClientPF 再补回。命令行编译因 Unity 编辑器占用工程未执行到脚本编译，日志 `Logs/codex-clienttcp-repair-compile.log`。
2026-09-03 已补回握手回包后的游客登录请求及登录成功后的 `CLPFGetHeroReq` 发送和日志；登录 token 当前为固定设备值的 Base64 占位，需以服务端验证结果确认是否要求 CA3 加密。
2026-09-03 微信日志显示 `NetController.GetIpPort 不可用`，根因是探针只按 HotFix 程序集查找类型；已改为同时查找 `Assembly-CSharp` 及所有已加载程序集。需重新 WebGL 构建/转换后验证。
2026-09-03 新日志确认 WebGL IL2CPP 运行时反射仍找不到 `NetController`。已改为直接调用 `NetController.Instance.GetIpPort`，并直接读取 `SysDefines.Ip/Port`，避免裁剪/程序集反射失败。
2026-09-03 修复 `SysDefines.Port` 为 `long` 与 TCP API `int` 的 CS0266，加入 1..int.MaxValue 范围校验后显式转换。
2026-09-03 WebGL UnityLinker 报 `Failed to resolve assembly: HotFix`，确认直接引用 `NetController`/`SysDefines` 会使 Assembly-CSharp 产生 HotFix 编译依赖；已移除直接引用，恢复反射查找并在 NetController 尚未初始化时每秒重试。
2026-09-03 已重建 `Assets/Client/Runtime/ClientTcpConnectionProbe.cs`：保留 AOT→HotFix 纯反射边界并用单一协程等待 HotFix/网关；握手补齐平台、产品、版本、device、channel、country、language；游客登录改为 `CA3Encode(fixedDevice + "," + OpeninstallToken, randomKey)` 等价实现；登录应答和 `CLPFGetHeroAck` 均按 `ClientGT.cs`/`ClientPF.cs` 字段顺序、数组上限和包尾完整校验。状态机只在英雄数据成功解析后请求主动关闭，所有 `onError`、超时、无效包和服务器关闭均输出失败原因。命令行 Unity 编译尝试被已打开的同工程 Editor 项目锁阻止，日志为 `Logs/codex-tcp-client-session-compile.log`；尚未构建、上传或操作 GUI。
2026-09-08 个人中心七页面互斥导航清理：`ClientProfileNavigationController` 改为按个性化五按钮在 ToggleGroup 中的固定顺序映射页面，不再读取旧的持久化跳转目标；从 46 个相关顶部/底部 Toggle 移除旧的序列化 `GameObject.SetActive` 回调，避免旧控制器与新控制器并行打开页面；`MainPage主页` 默认恢复激活。GUI 节点、层级、尺寸和位置未改动。Unity 2022.3.57f1c2 批处理验证仍被已打开的同工程 Editor 锁阻止，日志为 `Logs/codex-profile-route-cleanup-compile.log`；需关闭/释放 Editor 后重新编译并运行页面点击回归。
2026-09-08 日志诊断确认旧版控制器存在页面激活重入：`ApplyRoute` 激活页面时触发 `ToggleGroup.OnEnable`，ToggleGroup 又触发导航回调，造成递归 `ApplyRoute` 和 `ArgumentOutOfRangeException`。已在 `ClientProfileNavigationController` 增加 `_isApplyingRoute` 回调抑制，页面切换期间忽略 ToggleGroup 自动初始化事件；日志同时证明七页、21 个顶部 Toggle、25 个底部 Toggle 均已绑定，未绑定数为 0。待负责人重新运行当前脚本后复核。
2026-09-08 根据最新测试日志，顶部 Toggle 在铭牌页可正常返回个人中心，但进入其他个性化页后未观察到后续顶部返回事件；已增加 `PagesLayerActiveRoots` 诊断，下一次测试将记录所有仍处于 `activeInHierarchy` 的一级页面根节点，用于区分 GUI 点击未命中与页面内容脱离七页根节点。Unity 批处理再次受现有 Editor 实例占用，未结束负责人进程。
2026-09-08 页面导航重构：`ClientProfileNavigationController.ApplyRoute` 改为以 `PagesLayer` 为唯一可见性边界，每次切换先关闭 `PagesLayer` 下全部一级节点，再只打开目标七页面；这样会同时清理遗留的 MainPage、旧页面和独立包装 Canvas，不再只关闭七页字典中的节点。未修改节点布局和尺寸。Unity 批处理编译日志 `Logs/codex-profile-route-global-exclusive-compile.log` 仍显示工程被已有 Editor 占用，待 Editor 空闲后验证。
2026-09-08 通过最新日志和场景结构确认第二套运行时导航：`ClientCanvas` 上的 `ClientUiNavigator` 仍订阅旧页面事件并调用 `ClientUiPage.Enter/Pause/Resume`，与新控制器同时改页面状态。已将该场景组件禁用（GUID `6f19541d58330684484d3fd37121ff65`），保留脚本供旧编辑器工具引用；新控制器作为唯一运行时页面导航入口。验证日志 `Logs/codex-disable-legacy-ui-navigator-compile.log` 仍被现有 Unity Editor 项目锁阻止。
2026-09-08 针对负责人最新截图继续收敛页面互斥：运行日志证明当前控制器的 `PagesLayer` 一级根节点每次只有一个激活，但画面仍存在重叠，说明可能有旧流程留下的重复页面实例或脱离首次页面字典的同名页面。`ClientProfileNavigationController` 现会扫描当前场景所有位于 `PagesLayer` 下的 7 个页面实例，切换时先全部关闭，再打开唯一目标；原有关闭 `PagesLayer` 全部一级子节点的保护仍保留，并新增 `AllPageInstancesActive` 诊断记录全场景同名页面的实际激活数量。未修改 GUI 尺寸、位置、层级或素材。待负责人重新运行后回传最新 `[ClientProfileNavigationController] Bind` 中的 `allPageInstances` 数量和截图；Unity 命令行编译需继续在 Editor 释放后执行。
2026-09-08 最新运行日志已确认 7 个页面实例互斥有效（`allPageInstances=7`、每次 `AllPageInstancesActive：count=1`），但负责人截图仍重叠。因此新增 `ActiveVisualCanvases` 诊断，记录 `ClientCanvas` 下所有实际启用的 Canvas 路径与排序层级，用于定位页面外 Popup/System 或页面内部独立 Canvas 的第二套显示来源；下一步依据该证据重构唯一显示宿主，不再继续叠加页面 `SetActive` 补丁。
2026-09-08 交接记录：负责人要求将当前聊天交接到新聊天。当前页面导航事实：`ClientProfileNavigationController` 日志持续证明 7 个页面互斥，`AllPageInstancesActive：count=1`；`ActiveVisualCanvases` 显示的 Canvas 全部位于当前激活页面路径下，例如头像/铭牌页自己的 `Scroll View`、默认背景和顶部 Toggle，未发现其他 BadgePage 同级页面同时渲染。Hierarchy 截图也显示 5 个 BadgePage 为 `PagesLayer` 下同级节点，当前头像页内部包含 `Background、Canvas、Scroll View、PersonalizeLabel、默认背景+标签Canvas、个人中心/主页展示/个性化(1)`。因此剩余“重叠”尚未被证明是页面导航残留，可能是当前页面内部子节点布局/显隐或运行画面未刷新；未在证据不足时关闭这些内部 Canvas。已禁用场景中的旧 `ClientUiNavigator`，保留旧源码供 Editor 工具引用；顶部/底部旧序列化 `SetActive` 回调已清理。最近代码文件为 `Assets/Client/Runtime/UI/ClientProfileNavigationController.cs`，当前 Unity 编译仍被已有 Editor 项目锁阻止，最近日志为 `Logs/codex-profile-visual-canvas-compile.log`。新聊天应先读取本记录、`AGENTS.md`、相关 `MODULE.md` 和最新 `Editor.log`，再决定是否修改 GUI 子节点。
2026-09-08 新目标：负责人暂停 UI 重叠修复，改为在独立 `ClientShell/Client` 场景进行服务器数据对接。目标顺序为“建立连接 → 接收原始数据包 → 按模块/协议分类 → 后续再接入各业务接口”，暂不做 UI 展示。已确认不能直接沿用 Boot 的场景生命周期：当前 `ClientShell` 没有 Boot 初始化链，现有 `ClientTcpConnectionProbe` 仅在微信 WebGL 分支执行。已新增 `Assets/Client/Runtime/Services/ClientServerPacketRouter.cs`，按 ClientGT 1/1、1/5 和 ClientPF 2/1、2/2 分类，输出 JSONL 摘要并可保存本地原始包捕获到 `Application.persistentDataPath/ClientServerLogs`，不写入 UI/业务服务。下一步是增加可替换的 Client 传输适配器：PC 端优先用真实 TCP/网关链路测试，微信端复用同一分类器；不得把真实 token、个人资料或服务端地址写入仓库。当前 Unity 编译仍需在已有 Editor 释放后执行。
2026-09-08 服务器数据对接继续：新增 `ClientServerPcSession` 作为 ClientShell 的 PC/Editor 传输入口，通过现有 `NetController.GetIpPort` 反射获取网关，不把地址写入仓库；连接后执行握手、游客登录和 `CLPFGetHeroReq`，所有收到的完整包统一送入 `ClientServerPacketRouter`。路由器对 `CLPFGetHeroAck` 额外记录英雄/英雄组数量，并识别长度异常与截断包；网关发现增加 15 秒超时。ClientShell 已挂载该入口，WebGL 下自动让位给原微信探针，不改 GUI。命令行验证日志为 `Logs/codex-client-server-compile.log`，仍被已有 Unity Editor 项目锁阻止（`HandleProjectAlreadyOpenInAnotherInstance`），没有取得脚本编译结果；待负责人重新进入并释放工程后自动重试，编译后等待短暂资源刷新再继续运行验证。
2026-09-08 负责人重新进入 Unity 后，Editor.log 已确认本轮 `Assembly-CSharp.dll` 编译成功并完成域重载，未发现本轮 `error CS`；仅有既有未使用字段及旧代码警告。按流程再次执行的命令行验证仍被当前运行的 Unity Editor 拦截，日志为 `Logs/codex-client-server-compile-retry.log`，原因仍是 `HandleProjectAlreadyOpenInAnotherInstance`。下一步可直接在已完成编译的 Unity Editor 中运行 `ClientShell`，观察 `[ClientServerPC]` 与 `[ClientServerData]` 日志，并把 `Application.persistentDataPath/ClientServerLogs` 下的 JSONL 路径/内容回传。
2026-09-08 ClientShell PC 端真实运行验证通过：Editor.log 显示网关解析到临时服务端地址并成功 TCP 建连，发送握手 99 字节、游客登录 71 字节、`CLPFGetHeroReq` 8 字节；收到并分类 3 个服务端包：模块 1/协议 1 握手应答 17 字节、模块 1/协议 5 登录应答 155 字节、模块 2/协议 2 英雄应答 348 字节。JSONL 记录确认 `HeaderValid`，英雄数组 4、英雄组数组 1。原始包与摘要已保存到本地 `ClientServerLogs`，当前目标“连接→收包→分类”闭环完成；下一步再把分类结果映射到可替换的客户端数据接口，暂不接 UI 展示。
2026-09-08 分类可读性增强：`ClientServerPacketRouter` 现在同时记录发送和接收方向；PC 传输每次发送握手、游客登录或 `CLPFGetHeroReq` 前均写入分类器。除 JSONL 和原始二进制外，新增同会话 `.protocol.log`，按序号、方向、协议名、模块/协议号、字节数、状态和安全摘要输出，登录令牌等敏感正文不落盘。此前一次运行仍只有 3 个接收包，是因为旧版本只接收记录；需重新运行 ClientShell 生成包含收发双方的新版协议日志。代码修改后待 Unity 编辑器重新编译验证。
2026-09-08 协议分类继续扩展：路由器通过反射扫描已加载的 `CLGT*`、`CLPF*` 类型及其 `mid/pid` 常量，自动建立完整协议名目录；记录新增 `ProtocolName`，不再只显示有限枚举。未知或被裁剪的协议仍记录模块/协议号和原始包，不猜测正文。`git diff --check` 未发现空白错误；命令行验证日志 `Logs/codex-client-protocol-catalog-compile.log` 仍因 Unity Editor 占用项目而未进入编译。待 Editor 完成本轮自动编译后运行 ClientShell，确认 `.protocol.log` 中出现双向及完整协议类名。
2026-09-08 可读分类摘要继续增强：握手请求记录平台/产品/版本，登录请求记录登录方式/令牌长度，握手与登录应答记录状态，英雄应答记录英雄 ID、星级、等级、战力和数组数量；设备、渠道、令牌、密钥、昵称、电话等敏感或个人字段不写日志。发送请求已按请求类别分类，不再显示 `Unknown`。本次命令行日志未生成，说明仍被运行中的 Unity Editor 抢占；待编辑器自动完成本轮编译后重新运行 ClientShell 验证。
2026-09-08 协议日志格式优化：`.protocol.log` 现在按数据包分隔成多行报告，显示发送/接收、协议类名、协议分类、模块中文用途、协议号、长度、校验和字段摘要；字段摘要自动逐项换行，便于人工阅读。未新增接口绑定或 UI 展示逻辑。代码改动后需重新编译并运行一次 ClientShell 才会生成新格式日志。
2026-09-09 微信端协议日志接入：`ClientTcpConnectionProbe` 的握手、游客登录、`CLPFGetHeroReq` 等发送包现在也写入统一 `ClientServerPacketRouter`，微信收到的完整包继续统一分类；可读分段报告同时写入 `ClientServerLogs` 并通过 Unity/微信日志输出。未改动 UI 和接口绑定。`git diff --check` 通过；命令行编译因当前 Unity Editor 占用项目被阻止，日志为 `Logs/codex-wechat-readable-packets-compile.log`，待 Editor 自动编译后重新导出微信包验证。
2026-09-09 微信端测试目标确认：负责人要求仅验证 Console 中是否能输出与 PC 相同的双向可读协议报告，测试通过后再移除长期 Console 输出；当前已在微信探针的发送路径调用 `RecordOutgoing`，接收路径调用 `Accept`，并增加 IL2CPP 协议名兜底。最新命令行编译仍被 Editor 锁阻止，需在 Unity Editor 自动编译完成后重新构建/导出微信包并人工回传 Console。
