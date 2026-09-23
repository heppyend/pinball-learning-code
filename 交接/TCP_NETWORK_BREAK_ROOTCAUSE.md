# TCP 网络在新客户端入口下失效：原因说明

> 文档性质：**原因说明（可对外解释）**
> 建立：2026-09-22
> 适用工程：`D:\unity project\pinball`（Unity 2022.3.57f1c2 + URP + YooAsset + HybridCLR + 微信小游戏 SDK）
> 结论性质：机制推导 + 代码取证。**尚未取得真机/Console 的 `[TCP客户端]` 超时日志实测**，见文末「证据等级说明」。

---

## 一、一句话结论

**客户端界面能跑通，但 TCP 网络断了。原因是启动入口从「`Boot`」改成了「`ClientBoot`」，
而热更程序集 `HotFix`（含 `NetController`）只有 `Boot` 上的 `Init` 能加载它 ——
新入口下 `Boot` 永远不会被加载，TCP 探针因此反射不到 `NetController`，15 秒后必然超时。**

这不是环境问题、不是上传问题、不是 bug，而是**启动链改变带来的设计后果**。

---

## 二、现象（三个并存的事实）

| # | 现象 | 状态 |
|---|---|---|
| 1 | 微信小游戏里**能看到客户端界面**，页面跳转与功能正常 | ✅ 正常 |
| 2 | TCP 探针在 Console 里只打出一条「已挂载」，**没有后续会话日志** | ⚠️ 异常 |
| 3 | 预期 15 秒后会出现 `[TCP客户端] 失败退出 TCP：原因=等待 HotFix/NetController 超时` | ⏳ 待实测确认 |

**为什么第 1 条会误导人**：界面能跑通，恰恰因为它和网络**是解耦的**。详见第五节。

---

## 三、机制：`NetController` 在哪里，谁负责把它加载进来

### 3.1 `NetController` 在热更程序集里

```
Assets/Scripts/HotFix.asmdef          ← 覆盖整个 Assets/Scripts
  ├─ Assets/Scripts/GameLogic/NetController.cs
  └─ Assets/Scripts/.../SysDefines
```

TCP 探针（`Assets/Client/Runtime/ClientTcpConnectionProbe.cs`）的反射目标正是这两个类型：

```
private const float HotFixWaitSeconds = 15f;        // 第 24 行
Type netControllerType = FindType("NetController");  // 第 180 行
...
FailSession("等待 HotFix/NetController 超时");        // 第 211 行
```

### 3.2 HotFix 不在播放器里，必须运行时加载

HotFix 是 **HybridCLR 热更程序集**，不参与 AOT 编译，**必须运行时从资源包里读出来再 `Assembly.Load`**。

全工程**只有一处**做这件事：

```
Assets/Main/Init.cs
  ├─ 第 208 行  LoadMetadataForAOTAssemblies(package)   ← 先加载 AOT 补充元数据
  │                 （mscorlib / System / System.Core / Demigiant / YooAsset / LC.Newtonsoft.Json）
  │                 数据来自 package.LoadAssetAsync<TextAsset>(...)  ← YooAsset 包
  └─ 第 216 行  Assembly.Load(textAsset.bytes)          ← ★ HotFix 在这一刻才进 AppDomain
                  数据来自 package.LoadAssetAsync<TextAsset>("HotFix.dll")
```

> **关键**：这两步都依赖 `package`，而 `package` 来自 `Init.OnStart()` 里的 YooAsset 初始化
> （WebPlayMode，从 CDN 拉清单与资源包）。**YooAsset 没初始化，HotFix 就加载不了。**

### 3.3 `Init` 只挂在 `Boot.unity` 上

```
Assets/Main/Boot.unity  →  唯一挂在它上面的启动组件是 Init.cs
```

---

## 四、对比：为什么以前能跑通，现在不行

### 4.1 以前（唯一成功记录：2026-09-10，来源见第六节）

当时的构建清单**只有 `Boot.unity` 一个场景**，启动链是：

```
Boot.unity（唯一场景、列表首位）
  └─ Init
       ├─ YooAsset 初始化（WebPlayMode）
       ├─ LoadMetadataForAOTAssemblies()     加载 AOT 补充元数据
       ├─ Assembly.Load(HotFix.dll.bytes)    ★ NetController 进 AppDomain
       └─ YooAssets.LoadSceneAsync("LoginScene")
            └─ TCP 探针反射 NetController → 成功
                 → 网关获取 → TCP 建连 → 握手 → 游客登录 → GetHero 收包 ✓
```

**结论：以前能跑通，是因为 `Boot/Init` 先跑完了，`NetController` 已经在 AppDomain 里等着探针反射。**

### 4.2 现在

为解决「`ClientShell` 没有任何运行时入口」（`BUG_TRACKER.md` 的 `BUG-025`），
按**方案 C** 新增了客户端专用入口场景 `ClientBoot`，构建清单变为：

```
ProjectSettings/EditorBuildSettings.asset
  场景[0] enabled=True  Assets/Client/Scenes/ClientBoot.unity
  场景[1] enabled=True  Assets/Main/Boot.unity
  场景[2] enabled=True  Assets/Client/Scenes/ClientShell.unity
```

而 `ClientBootLoader.cs` 用的是：

```csharp
SceneManager.LoadScene(_clientSceneName);   // 单参重载 = LoadSceneMode.Single
```

`Single` 模式会**替换掉当前场景**，于是：

```
ClientBoot.unity
  └─ ClientBootLoader
       ├─ 运行时挂载 TCP 探针            ← Console 里看到的那条日志
       └─ LoadScene("ClientShell")        ★ Single ⇒ ClientBoot 被替换
                                          ★ 而 Boot.unity 无人加载（清单里有，但没代码加载它）
```

**于是形成断链：**

```
Boot 不被加载
  → Init 不执行
    → YooAsset 不初始化（客户端 UI 不需要它，所以界面照常跑）
    → AOT 补充元数据不加载
    → Assembly.Load(HotFix) 不执行
      → NetController 不在 AppDomain
        → TCP 探针反射落空
          → 15 秒后 FailSession("等待 HotFix/NetController 超时")
```

---

## 五、为什么「界面能跳转」不代表「网络能跑」

这一点最容易误导判断，单独说明：

| 层 | 依赖什么 | 新入口下 |
|---|---|---|
| **客户端 UI**（页面/导航/弹窗/Toast） | `Resources/Table/*.json` 本地表 + 本地模拟数据服务<br>**零 YooAsset 依赖** | ✅ **通** |
| **热更逻辑**（HotFix：原公司游戏逻辑、协议、`NetController`） | **必须** `Boot/Init` 的 YooAsset 链 | ❌ **断** |
| **TCP 网络** | 探针要反射 HotFix 里的 `NetController` | ❌ **断** |

取证依据：

```
Assets/Client/Runtime/**  内  YooAssets.  引用数 = 0
Assets/Client/Runtime/Services/ClientTableSource.cs
    读表优先 {persistentDataPath}/config.pkg，缺失则回落 Resources/Table/<表名>.json
```

⇒ **界面能跑通，正是因为它与 HotFix / YooAsset 解耦。两者不能互相证明。**

---

## 六、证据清单（可复查）

| # | 事实 | 出处 |
|---|---|---|
| 1 | `NetController` / `SysDefines` 属 HotFix 程序集 | `Assets/Scripts/HotFix.asmdef`（`name: HotFix`，覆盖 `Assets/Scripts`） |
| 2 | 探针等待 HotFix 的时限 = 15 秒，超时判失败 | `ClientTcpConnectionProbe.cs:24`、`:206-211` |
| 3 | 全工程仅一处加载 HotFix 进 AppDomain | `Init.cs:216` `Assembly.Load(textAsset.bytes)`（全仓 grep `Assembly.Load` 确认） |
| 4 | 该处**先**加载 AOT 元数据、**再**加载 HotFix，两步都从 YooAsset 包取字节 | `Init.cs:202-219` → `:236-260` |
| 5 | `Init` 只挂在 `Boot.unity` | `Assets/Main/Boot.unity` 的组件序列化 |
| 6 | 构建清单现在是 3 个场景，`ClientBoot` 居首 | `ProjectSettings/EditorBuildSettings.asset` |
| 7 | `ClientBootLoader` 用 Single 模式加载 `ClientShell` | `Assets/Client/Runtime/ClientBootLoader.cs`（`SceneManager.LoadScene(name)`） |
| 8 | 客户端 UI 零 YooAsset 依赖 | `Assets/Client/Runtime/**` 内 `YooAssets.` 引用数 = 0 |
| 9 | Console 实测：只有「已挂载 TCP 探针」，无后续会话日志 | 负责人截图，2026-09-22 16:55 |
| 10 | 历史成功记录：网关→TCP→握手→游客登录→GetHero 六包全部 `HeaderValid` | `C:\Users\Administrator\AppData\LocalLow\xiaoshi\xiaozhen\ClientServerLogs\session-20260910-152459.protocol.log` |

---

## 七、三条修复路线

| 方案 | 做法 | 是否改原公司启动流程 | 代价 / 风险 |
|---|---|---|---|
| **C-2″**（推荐） | 让 `Boot` 重新进入启动链：`ClientBootLoader` 改为**附加加载** `Boot`（`LoadSceneMode.Additive`），待 `Init` 跑完后再加载 `ClientShell`。`Init.cs` 已预留 `ClientStartScene` 字段（默认空 = 原行为 `LoginScene`） | 否（`Init.cs` 逻辑不变，只改目标场景） | 需处理时序：`Init` 结尾会 `LoadSceneAsync("LoginScene")`（Single），要把它指向 `ClientShell` |
| **C-1+** | 保持 `Boot` 不加载，由客户端入口**自行**实现「YooAsset 初始化 + AOT 元数据 + 加载 HotFix」 | 否 | 工作量大，等于重写 `Init` 的加载段；后续维护两条加载链 |
| **C-4** | 接受「客户端无网络」，TCP 另开一条独立链路单独验证 | 否 | 移植目标 B 的「真实会话」不随客户端生效，需负责人确认验收口径 |

**C-2″ 的改动面**：`ClientBootLoader.cs` + `Init` 的目标场景。**`ClientShell.unity`（UI）一行不用动。**

---

## 八、证据等级说明（重要，避免过度解读）

| 结论 | 等级 |
|---|---|
| `NetController` 在 HotFix、HotFix 只由 `Init` 加载、`Init` 只在 `Boot` 上 | **已取证**（静态代码 + 工程文件） |
| 新入口下 `Boot` 不被加载 ⇒ `Init` 不执行 | **已取证**（`ClientBootLoader` 用 Single 模式；全工程无其它代码加载 `Boot`） |
| 探针会「15 秒后超时」 | **机制推导**（时限来自代码常量）。**尚未取得该行日志的实测**，需 Console 中 `[TCP客户端]` 过滤段确认 |
| 历史上网络曾全链路跑通 | **已取证**（`session-20260910-152459.protocol.log`，六包 `HeaderValid`） |

> **待补**：在新包上取一次 Console 的 `[TCP客户端]` 全段。
> 若出现 `失败退出 TCP：原因=等待 HotFix/NetController 超时`，本文第二节的预期即被实证；
> 若出现其它原因（如探针完全未启动），则需按实际日志修正第三节的推导起点。

---

## 九、附：一句话对外解释版本

> 我们把客户端入口从原来的 `Boot` 场景换成了独立的 `ClientBoot` 场景，这是为了解决
> 「客户端场景没有任何入口」的问题。但原来的 `Boot` 场景里挂着一个启动脚本，
> 它负责从资源包加载**热更程序集**（网络模块就在里面）。新入口用了「替换式」切场景，
> `Boot` 就再也不会被加载，热更程序集也就没被加载 —— 所以界面正常，
> 但网络模块找不到自己的依赖、15 秒后超时。
> 修复方向是让 `Boot` 的加载流程重新进入启动链，客户端界面本身不受影响。

---

## 十、修复经过与第二个原因（实测更新，2026-09-22 晚）

### 10.1 第一个原因已修复并实证

按第七节 **设计 A** 实施：

| 改动 | 文件 |
|---|---|
| 构建清单移除 `ClientBoot`，变为 `Boot(0) → ClientShell(1)` | `ProjectSettings/EditorBuildSettings.asset` |
| `VerifyStartupChain` 判据改为校验新链路 | `Assets/Client/Editor/ClientWebGLBuildCommand.cs` |
| `ClientBootLoader` 移除探针挂载并标注弃用 | `Assets/Client/Runtime/ClientBootLoader.cs` |

重构建后 Console 实测：

```
[TCP客户端] 已迁移到跨场景会话对象，等待 Boot 场景切换不再中断 TCP 流程
[TCP客户端] 会话开始：device=固定纯英文字母，长度=40，模块/协议=0/0，字节=0，结果=开始
[TCP客户端] 阶段=WaitingHotFix：...
[TCP客户端] 等待 HotFix 与 NetController 初始化
...（15 秒后）...
[TCP客户端] 失败退出 TCP：原因=客户端超时，阶段=WaitingHotFix ...
```

⇒ **`Boot` 已重新进入启动链、探针已启动**（第一节的机制被实证）；
但仍在 `WaitingHotFix` 超时 ⇒ **存在第二个独立原因**。

### 10.2 第二个原因：`Init` 在「获取资源版本」处中断（CDN 路径布局错误）

Console 关键错误（原文）：

```
[PLUGIN ERROR] xhr_onload: PackageManifest_DefaultPackage.version下载失败:
    statusCode= 404 , errMsg= 404
URL : https://<CDN域名>/1/HotUpdate/NewAB/WebGL/pinball-client/PackageManifest_DefaultPackage.version
Error : HTTP/1.1 404 Not Found

调用栈：
  $QueryRemotePackageVersionOperation_InternalOnUpdate
  $Init$U3COnStartU3Ed__9_MoveNext        ← Init.OnStart 协程（Init.cs:213 的 Debug.LogError）
```

**代码依据（`Init.cs:476-482`）**：

```csharp
string IRemoteServices.GetRemoteMainURL(string fileName)
    => $"{_defaultHostServer}/{fileName}";      // host + 文件名，不含 /Assets
```

而 `Boot.unity` 的 `DefaultHostServer = https://.../WebGL/pinball-client`（**根目录**）。

⇒ **YooAsset 的清单与 bundle 全部请求 `<CDN根>/<文件名>`。**

**部署错误**：当时把 53 个 YooAsset 运行时文件传到了 `<CDN根>/Assets/` 子目录。
那个 `/Assets` 层级是**微信小游戏 SDK 首包（webgl.data / wasm / 纹理）**用的路径，
与 YooAsset 无关 —— 两套路径系统被混淆了。

**OSS 实测对照**：

| 文件 | `<CDN根>/` | `<CDN根>/Assets/` |
|---|---|---|
| `PackageManifest_DefaultPackage.version` | **404** | 200 |
| `PackageManifest_DefaultPackage_2026-09-22-1420.bytes` | **404** | 200 |
| `PackageManifest_DefaultPackage_2026-09-22-1420.hash` | **404** | 200 |
| `<FileHash>.bundle`（47 个） | **404** | 200 |
| `*.webgl.data.unityweb.bin.br` / `*.webgl.wasm.code.unityweb.wasm.br` | 200 ✅ | — |

**修正动作**：把上述 53 个文件传到 `<CDN根>/`（即 `1/HotUpdate/NewAB/WebGL/pinball-client/`）。
**无需改代码、无需重构建**（`Init` 与 `Boot` 的配置本身是正确的）。

**链路中断点**：`Init.OnStart` 协程

```
资源包初始化成功！            ✅ 有
3. 获取资源版本               ❌ 无输出 → QueryRemotePackageVersionOperation 404 失败
                              → Init.cs:214 Debug.LogError(operation.Error) → 协程结束
4. 更新资源清单               ❌ 未执行
6. 强制加载程序集             ❌ 未执行
加载Init                     ❌ 未执行
LoadMetadataForAOTAssembly   ❌ 未执行
Assembly.Load(HotFix)        ❌ 未执行  ⇒ NetController 不存在 ⇒ 探针 15 秒超时
```

> 注：此处的 404 与 `BUG-015`（`/Assets` 层级缺失）**根因相同、方向相反** ——
> 当时是"少了一层 `/Assets`"，这次是"多了一层 `/Assets`"。同一类路径错配，需在交付流程里固化校验。

### 10.3 证据等级更新

| 结论 | 等级 |
|---|---|
| C-2 的设计 A 修复有效：`Boot/Init` 重新进入启动链、探针启动 | **已实证**（Console `[TCP客户端] 会话开始`） |
| 探针超时于 `WaitingHotFix` | **已实证**（Console 原文） |
| 第二原因是 YooAsset 文件放错层级导致版本查询 404 | **已实证**（404 URL + 调用栈 + 代码依据 + OSS 对照） |
| 修正路径后 TCP 能全链路连通 | **待验证**（需重新上传后再测） |

### 10.4 修正后的完整启动链（期望）

```
WeChat 首包: <CDN根>/<hash>.webgl.data...br + <hash>.webgl.wasm.code...br   ✅ 已就位
  ↓
Boot.unity（构建清单第 0 位）
  └─ Init
       ├─ YooAsset 初始化（WebPlayMode）
       ├─ 3. 获取资源版本     → <CDN根>/PackageManifest_DefaultPackage.version      ← 需补传
       ├─ 4. 更新资源清单     → <CDN根>/PackageManifest_DefaultPackage_<ver>.bytes  ← 需补传
       ├─ 6. 强制加载程序集 + 加载Init
       ├─ LoadDll → LoadMetadataForAOTAssembly → Assembly.Load(HotFix)
       ├─ 首个场景：ClientShell（ClientStartScene=ClientShell）
       └─ TCP 探针（挂在 Init 对象上）→ 反射 NetController 成功
            → 网关获取 → 建连 → 握手 → 游客登录 → GetHero 收包
```
