# 弹珠项目源码学习版

> 面向实习复盘与 Unity 客户端学习的源码快照。保留客户端架构、关键实现和工程配置；刻意移除了美术、音频、模型、材质、字体、动画、资源包与构建产物。

## 项目定位与边界

项目基于 Unity `2022.3.57f1c2`。原工程包含登录、大厅及街机/战斗兼容流程；本次客户端开发新增独立的非战斗模块，覆盖主页、个人中心、英雄、背包、养成、编队、商城、邮箱、排行榜、活动、任务和扭蛋等页面的本地演示闭环。

本仓库用于学习架构和实现，不能直接还原完整画面：场景与预制体中部分图片、字体、材质引用会因资源被排除而缺失。

### 服务端边界

原工作区未包含独立服务端工程。TCP、网关、登录和协议代码均是**客户端**实现：负责获取网关、建立连接、发送请求和解析回包；不包含服务端业务、数据库、账号体系、支付、运营后台或部署代码。

## 技术栈

| 能力 | 技术/实现 | 用途 |
| --- | --- | --- |
| 引擎与渲染 | Unity 2022.3.57f1c2、URP | 客户端运行时、UI 与渲染基础。 |
| 资源管理 | YooAsset | 编辑器模拟、离线、Host、Web 四种资源模式；资源版本、清单、下载与场景加载。 |
| 热更新 | HybridCLR | 启动时补充 AOT 元数据，并动态加载 `HotFix.dll`。 |
| 脚本 | XLua / Lua | Lua 业务脚本、C# 与 Lua 桥接、WebGL Lua 插件。 |
| PC 网络 | TCP + 二进制协议 | 网关、握手、游客登录、英雄数据请求与协议记录。 |
| 微信小游戏 | WX-WASM-SDK-V2 + WebGL | 微信运行时桥接、JS SDK、WebGL 传输与渲染兼容。 |
| UI | UGUI + TextMeshPro | 可编辑 Canvas、页面生命周期、导航栈、弹窗和全局反馈。 |

## 目录结构

```text
.
├─ Assets/
│  ├─ Main/                         启动入口、YooAsset 初始化与热更新加载
│  ├─ Scripts/                      原有 Unity/PC 客户端业务、框架、网络与协议
│  │  ├─ Frame/                     UI、资源、模块、对象池、协程等管理器
│  │  ├─ GameLogic/                 登录、大厅、玩家、房间与联网业务流程
│  │  ├─ NetWork/JBPROTO/           二进制协议定义及读写
│  │  ├─ WebWork/Protocol/          HTTP 网关请求/响应、JSON/XML 解析
│  │  └─ Platform/WeChatMiniProgram 微信小游戏平台能力抽象
│  ├─ Client/                       新增的非战斗客户端模块
│  │  ├─ Scenes/ClientShell.unity   独立客户端演示场景
│  │  ├─ Runtime/Domain/            英雄、道具、货币、邮件、活动领域模型
│  │  ├─ Runtime/Services/          服务接口、本地模拟、PC 收包与协议路由
│  │  ├─ Runtime/UI/                页面生命周期、统一导航与返回栈
│  │  ├─ Runtime/Client*.cs         页面控制器、反馈、加载与小程序探针
│  │  └─ Editor/                    仅编辑器可运行的场景辅助工具
│  ├─ Lua/                          Lua 脚本目录
│  ├─ HotUpdateResources/Dll/WebGL/ WebGL 热更新 DLL 输入
│  └─ WX-WASM-SDK-V2/               微信小游戏 WASM SDK 运行时桥接与模板脚本
├─ WebGLPlugins/                    Lua C 源码及 WebGL 适配实现
├─ General/                         XLua 生成、热修复与辅助工具源码
├─ Packages/                        Unity Package 依赖
├─ ProjectSettings/                 Unity 项目与平台设置
├─ PROJECT_OVERVIEW.md              模块地图与稳定技术事实
├─ CURRENT_STATE.md                 当前检查点、验证证据与风险
└─ DEVELOPMENT_WORKFLOW.md          客户端开发流程与验收规则
```

## 运行流程

### 原项目启动、资源与热更新

构建入口为 `Assets/Main/Boot.unity`，核心脚本为 `Assets/Main/Init.cs`：

```text
Boot.unity
  → Init.Awake：帧率设置；WebGL 下进入兼容分支
  → YooAssets.Initialize + 创建 DefaultPackage
  → 按 PlayMode 初始化资源包
       ├─ EditorSimulateMode：编辑器模拟清单
       ├─ OfflinePlayMode：本地包与偏移解密
       ├─ HostPlayMode：远端资源 URL 与清单
       └─ WebPlayMode：WebGL 缓存策略与远端资源 URL
  → 更新资源版本与资源清单
  → 加载 AOT 元数据
  → 从 YooAsset 加载 HotFix.dll
  → 异步加载 LoginScene
```

WebGL 环境会关闭 HDR 和 URP 后处理，并记录首屏相机、Canvas 与渲染管线诊断，作为微信小游戏/WebGL 渲染差异的兼容与证据采集。

### 独立客户端演示

非战斗客户端不改动原有 `Boot → LoginScene → MainScene` 链路，而是通过 `Assets/Client/Scenes/ClientShell.unity` 独立运行：

```text
ClientShell
  → ClientBootstrap
  → ClientServices.InitializeLocalData()
  → LocalClientDataService 提供本地账户、货币、英雄、背包与活动数据
  → ClientUiNavigator 注册页面
  → 各页面控制器读取服务数据并刷新 UI
```

该隔离方式先完成 UI/本地规则闭环，再通过接口适配器接入账号、服务端、支付或平台能力，避免页面回调直接绑定协议和运营数值。

## UI 架构与导航

`ClientCanvas` 是独立客户端的唯一 UI 宿主：

```text
ClientCanvas
├─ PagesLayer     可导航页面：主页、个人中心、英雄、背包、商城等
├─ PopupLayer     页面级结果/确认弹窗，例如扭蛋结果
└─ SystemLayer    全局 Toast、Loading、遮罩和红点
```

| 组件 | 职责 |
| --- | --- |
| `ClientUiPageId` | 页面唯一标识。 |
| `ClientUiPage` | `Enter`、`Pause`、`Resume`、`Exit` 生命周期。 |
| `ClientUiNavigator` | 页面注册、打开/后退、返回栈维护。 |
| `ClientUiBackButton` | 统一调用导航器 `Back()`，避免各页复制返回逻辑。 |
| `ClientUiFeedback` | Toast、Loading 等全局反馈。 |

典型导航：

```text
MainPage
  ├─ 个人中心 → ProfilePage → 主页展示 / 个性化页面
  ├─ 英雄 → HeroPage → HeroDetailPage → HeroEnhancePage
  ├─ 背包 / 编队 / 弹珠 / 活动 / 任务 → 对应独立页面
  └─ 扭蛋 → GachaPage → GachaResultPopup
```

商城、个人中心、扭蛋等页面通过服务层状态刷新 UI，覆盖加载、空数据、拥有/未拥有、锁定/解锁、货币不足、限购、领取结果、关闭和返回等本地演示状态。

## 领域模型与本地模拟数据

`Assets/Client/Runtime/Domain/ClientModels.cs` 集中定义英雄、道具、货币、邮件和活动等领域模型。`IClientDataService` 是 UI 读取状态的唯一接口，当前 `LocalClientDataService` 提供内存模拟数据。

```text
页面控制器
  → IClientDataService
      → LocalClientDataService（当前）
      → ServerClientDataService（未来，需真实协议契约）
```

实现原则：数据驱动而非在 UI 中散落硬编码；服务端接入应实现相同接口语义和变更通知；当前仅代表本地模拟闭环，不代表线上账号、支付或服务端功能完成。

## PC 端网络与协议

`Assets/Scripts/GameLogic/NetController.cs` 负责客户端网关请求、握手、登录、进入玩法和玩家数据请求。典型最小会话：

```text
HTTP 网关请求 → 获取 IP / Port → TCP 建连
  → CLGTHandReq 握手
  → CLGTLoginReq 登录（游客/平台类型）
  → CLPFGetHeroReq 获取英雄数据
```

`ClientServerPcSession` 复用现有网关逻辑执行 PC 最小会话；`ClientServerPacketRouter` 统一记录收发方向、模块号、协议号、协议类名、长度、校验状态及脱敏摘要。令牌、昵称、手机号和真实服务端地址不应写入仓库或演示日志。

协议位于 `Assets/Scripts/NetWork/JBPROTO/`：`ClientGT` 处理握手、登录、保活和断线；`ClientPF` 处理玩家资料、英雄和个性化装扮。未知包只保留头信息和安全摘要，不猜测正文。

## 微信小游戏 / WebGL

微信端不直接复用 PC 的运行时假设：

| 关注点 | PC/Editor | 微信小游戏/WebGL |
| --- | --- | --- |
| 网关 HTTP | 原有 `QLWebClient` 路径 | `UnityWebRequest` 协程，避免不兼容的 `HttpWebRequest/Task.Run`。 |
| TCP 会话 | `ClientServerPcSession` | `ClientTcpConnectionProbe`。 |
| 协议记录 | `ClientServerPacketRouter` | 复用同一分类器与脱敏协议日志。 |
| 资源模式 | 编辑器模拟/离线/Host | `WebPlayMode` 与 WebGL 缓存策略。 |
| 渲染 | 常规 URP | 首屏关闭 HDR/后处理并保留诊断。 |

相关入口：

- `Assets/Client/Runtime/ClientTcpConnectionProbe.cs`：网关、建连、握手、游客登录、英雄数据完整解析和主动关闭。
- `Assets/Scripts/Platform/WeChatMiniProgram/`：平台桥接接口、注册表和不可用平台回退。
- `Assets/WX-WASM-SDK-V2/`：微信 WASM SDK 的 JS 运行时、桥接与模板代码。
- `WebGLPlugins/`：Lua C 源码与 WebGL 插件实现。

微信会话只有在英雄数据完整解析后才报告成功；超时、无效包、服务端关闭和协议解析失败都必须记录明确原因。这不等价于正式微信发布或线上平台接入完成。

## Lua、XLua 与 HybridCLR

`Assets/Lua/` 按协议、UI、模块、表、工具和消息分类；`General/` 保留 XLua 的生成、模板、热修复和辅助工具；`WebGLPlugins/` 保留 WebGL 使用的 Lua C 源码。

HybridCLR 的流程是：启动时从 YooAsset 读取所需 AOT DLL 元数据，再读取 `HotFix.dll` 并用 `Assembly.Load` 加载。WebGL 热更新 DLL 位于 `Assets/HotUpdateResources/Dll/WebGL/`，构建辅助命令位于 `Assets/Editor/HybridCLR/`。

## 推荐阅读路线

1. `PROJECT_OVERVIEW.md`：模块边界、启动入口和资源链路。
2. `Assets/Main/Init.cs`：YooAsset、HybridCLR 与场景加载。
3. `Assets/Client/CLIENT_UI_ARCHITECTURE.md`：ClientShell、Canvas、页面与服务设计。
4. `Assets/Client/Runtime/Services/`：本地模拟、PC 会话和协议分类。
5. `Assets/Client/Runtime/UI/` 与 `Client*.cs`：导航、返回与页面状态刷新。
6. `Assets/Scripts/GameLogic/NetController.cs`、`Assets/Scripts/NetWork/JBPROTO/`：PC 客户端网关、TCP 与协议。
7. `ClientTcpConnectionProbe.cs`、`Assets/WX-WASM-SDK-V2/`、`WebGLPlugins/`：微信小游戏/WebGL 适配。

## 当前限制

- 这是源码与结构学习快照，不是可直接发布的完整游戏工程。
- 真实账号、服务端、支付、运营数据、最终美术和正式小程序桥接仍待接入。
- 原有街机/战斗逻辑作为兼容性依赖保留，本次重点是非战斗客户端制作。
- 请勿向仓库写入外部地址、令牌、个人资料或订单信息。

更多过程证据、阶段状态和风险请阅读 `CURRENT_STATE.md`、`DEVELOPMENT_WORKFLOW.md` 与各模块 `MODULE.md`。
