# 首页无法显示：修复方案对比（A vs B）

> 建立：2026-09-22
> 适用工程：`D:\unity project\pinball`
> 背景文档：`TCP_NETWORK_BREAK_ROOTCAUSE.md`（TCP 断链的完整分析）
> **本文件只做方案对比，不含已实施改动；请负责人选定后再动手。**

---

## 一、问题定性

TCP 全链路已在小游戏容器内跑通（六包齐全），但**首页不显示**。日志证据：

```
L3361  首个场景：ClientShell（ClientStartScene=ClientShell）
L3363  Failed to mapping location to asset path : ClientShell
L3897  Failed to load scene ! The location is invalid : ClientShell
```

**原因**：`Init.cs:192` 用 `YooAssets.LoadSceneAsync(firstSceneName)` 加载场景 —— 这是**按资源地址**加载，
要求 `ClientShell` **出现在 YooAsset 寻址清单里**。

但收集器（`Assets/Main/Samples/Space Shooter/AssetSetting/AssetBundleCollectorSetting.asset`）
只收了 `Assets/HotUpdateResources/Scene` 等旧街机路径，**没有 `Assets/Client/Scenes`**：

```
资源包里的 .unity 资产 = 4 个：FishScene / LoadingScene / LoginScene / MainScene
ClientShell 不在其中  ⇒  地址映射失败  ⇒  场景不加载  ⇒  没有首页
```

> 这与 `BUG-025` 同源（"场景在清单外/无代码加载"），只是换了一个面：
> 场景这次**在 Build Settings 里**，但**不在 YooAsset 寻址清单里**。

---

## 二、已取证的关键事实（决定两个方案的代价）

| # | 事实 | 证据 |
|---|---|---|
| 1 | 客户端代码**零 YooAsset 依赖** | `Assets/Client/**` 内 `YooAssets.` 引用数 = **0** |
| 2 | 客户端**没有按地址动态加载资源** | `LoadAssetAsync` 引用数 = **0** |
| 3 | `Resources.Load` 只有 1 处，读配置表 | `ClientTableSource.cs:153`，表在 `Assets/Resources/Table/*.json`（21 个 / 174.1 KB），**本就随播放器打包** |
| 4 | `Instantiate(` 10 处，但都是对**已序列化引用**实例化 | 非地址加载 |
| 5 | `ClientShell.unity` 含组合根 `ClientBootstrap` | 场景内 `m_Name: ClientBootstrap` |
| 6 | 场景直接依赖 **338 个**资产 / **16.68 MB** | 320 png（16.14 MB）+ 14 controller（0.24 MB）+ 4 prefab（0.30 MB） |
| 7 | `ClientShell.unity` 自身 **4.76 MB** | 场景序列化数据 |
| 8 | `Assets/Client/UI` 全量 **1679 个 / 109.78 MB** | 其中**仅 338 个被场景引用**，未引用 1355 个 / 93.34 MB |
| 9 | `ClientShell` 已在 Build Settings 第 1 位 | `ProjectSettings/EditorBuildSettings.asset` |
| 10 | 当前首包：`data.br` 34.88 MB + `wasm.br` 14.58 MB | OSS 根目录，已 200 |

---

## 三、方案 A：改用 `SceneManager.LoadScene`

### 做法

`Init.cs` 中，当 `ClientStartScene` **非空**时改用 Unity 原生场景加载，而不是 YooAsset 地址加载：

```csharp
// 现状
var sceneOperation = YooAssets.LoadSceneAsync(firstSceneName);
yield return sceneOperation;

// 改为（仅当目标场景是内置场景时走这条路）
if (!string.IsNullOrWhiteSpace(ClientStartScene))
{
    var op = SceneManager.LoadSceneAsync(firstSceneName, LoadSceneMode.Single);
    while (!op.isDone) yield return null;
}
else
{
    var sceneOperation = YooAssets.LoadSceneAsync(firstSceneName);
    yield return sceneOperation;
}
```

### 为什么可行

`ClientShell` 已在 Build Settings。**Unity 构建播放器时会把它连同全部依赖（338 个资产 / 16.68 MB）打进播放器数据**，
所以 `SceneManager.LoadScene("ClientShell")` 不需要 YooAsset，也不需要收集器。

### 代价与影响

| 项 | 影响 |
|---|---|
| 改动面 | `Init.cs` 约 5~8 行，**可完全回滚** |
| 收集器 | **不动** |
| 基础设施 | **不动** |
| `wasm.br` | **会变**（实测 `a38979c637496e60…` → `f646dc49cc019398…`，15,283,985 → 15,284,582 B）：`Init.cs` 属 `Assembly-CSharp`（`Assets/Client` 无 asmdef，AOT 主程序集），改它必然改 `webgl.wasm`。原文「不变」只对"只调 UI"成立 |
| `data.br` | **不增长**。`ClientShell` 自 2026-09-22 17:37 那次构建起就已作为 Level 1 打进播放器（构建日志 `Level 1 '…/ClientShell.unity' uses 40.2 MB compressed`），方案 A 只换加载方式、不新增资源。实测新 `data.br` = 36,550,487 B（≈34.86 MiB），与旧 36,565,714 B 同量级 |
| 首次下载 | **基本不变（±0.1%）**。原文「约 49 MB → 约 66 MB（+35%）」是按"当前构建不含场景"估的，与实测矛盾，已更正 |
| 是否需重构建 | **是**（Unity 播放器构建），实测两阶段共约 9.2 分钟 |
| UI 后续可热更？ | **否**。改 UI 必须重跑构建 + 重传 `data.br`（实测约 36.5 MB，不是 51.6 MB） |
| 与 09-10 历史链路一致性 | 高（历史 `Boot → LoginScene` 也是场景加载；16:14 那次构建的 `ClientBoot` 入口同样是构建清单内原生启动） |
| 主要风险 | **不是**首包体积；而是 UI 迭代每次都走完整构建+上传（热更能力代价） |

### 验证方式

重构建 → 上传 `data.br`（新哈希）→ 开发者工具清缓存运行 → Console 应出现
`客户端启动` / `ClientBootstrap` / `配置表` 等标记，且首页可见。

---

## 四、方案 B：给 YooAsset 收集器加 `Assets/Client`

### 做法

在 `AssetBundleCollectorSetting.asset` 新增收集组（例如 `ClientScene`），收集：

```
Assets/Client/Scenes      → AddressByFileName / PackSeparately / CollectScene
Assets/Client/UI          → AddressByFileName / PackDirectory  / CollectAll
Assets/Client/animator controller → 同上
```

使 `ClientShell` 获得 YooAsset 地址，从而 `YooAssets.LoadSceneAsync("ClientShell")` 成功。

### 代价与影响

| 项 | 影响 |
|---|---|
| 改动面 | `AssetBundleCollectorSetting.asset`（**基础设施**，需负责人授权） |
| 基础设施 | **动到资源加载/热更基础设施**（`AGENTS.md` §3 明确要求单独授权） |
| `wasm.br` | 不变 |
| `data.br` | **不变**（34.88 MB） |
| 新增资源包体积 | 场景 4.76 MB + 被引用 16.68 MB ≈ **21.4 MB**；<br>**若用 `CollectAll` 收集整个 `UI` 目录，会变成约 114.5 MB**（含 93.34 MB 未引用资产）→ **必须用 `CollectScene`/依赖收集，不能 `CollectAll`** |
| 首次下载 | 首包不变，但**首次进入客户端要额外下载约 21 MB bundle**（总下载量相近） |
| 是否需重构建 | **是**（YooAsset 构建 + 转换），约 10 分钟 |
| UI 后续可热更？ | **是**。改 UI 只重传 bundle，**不用重编 wasm** |
| 与历史链路一致性 | 低（历史场景都走 YooAsset；但客户端是新模块，无历史包袱） |
| 主要风险 | ① 收集器配置错误会导致资源缺失或包体暴涨；<br>② `CollectAll` 误用会把 93 MB 未引用资产打进去；<br>③ 依赖链（shader / material / font）需逐个验证；<br>④ 后续每次改 UI 都要重跑 YooAsset 构建 |

### 验证方式

重构建 → 上传新 bundle + 清单 → 清缓存运行 → Console 应无
`Failed to mapping location`，且首页可见。

---

## 五、对比总表

| 维度 | A：`SceneManager.LoadScene` | B：收集器收 `Assets/Client` |
|---|---|---|
| 改动文件 | `Init.cs`（5~8 行） | `AssetBundleCollectorSetting.asset` |
| 触及基础设施 | ❌ 否 | ✅ 是（需授权） |
| 首包 `data.br` | 34.88 → **约 51.6 MB** | **不变** 34.88 MB |
| 额外下载 | 0 | 约 21 MB bundle（首次进客户端时） |
| 总下载量 | 约 66 MB | 约 70 MB |
| UI 可热更 | ❌ 否 | ✅ **是** |
| 改 UI 后要做什么 | 重构建 + 重传 51.6 MB | 重建 bundle + 重传 bundle |
| 改动可回滚性 | 高（几行代码） | 中（配置资产，需手动还原） |
| 主要风险 | 首包变大 | 收集器配错 → 资源缺失/包体暴涨 |
| 与历史链路一致性 | 高 | 低 |
| 适合 | 尽快让首页可用、UI 已基本定稿 | UI 还会频繁迭代、想让 UI 走热更 |

---

## 六、决定与建议

> ✅ **负责人已于 2026-09-22 选定【方案 A】。**
> ⛔ 但实施会触及 `Init.cs`，按 `AGENTS.md` §3 及负责人明确口径，**须先取得本次实施许可**（逐次确认，非长期授权）。

**选 A 的理由：**

1. **当前第一目标是让首页可见**。A 的改动是 5~8 行、可回滚、不动基础设施，风险最低。
2. **A 与 09-10 的历史成功链路同构**（都是场景加载），变量最少。
3. B 引入了收集器这个新变量，而我们**刚刚才因为"路径层级错配"烧了两轮**（先少一层 `/Assets`，后多一层 `/Assets`）。
   在这个节点上减少变量，比引入一个更好的架构更重要。
4. A 的代价（首包 +16.68 MB）在微信小游戏里可接受（66 MB 总下载），且**不影响已有功能**。

**已知并接受 A 的代价**：UI 不能热更 ⇒ 以后每次调 UI 都要重跑 8~10 分钟构建 + 重传约 51.6 MB。

**B 留作后续优化**：待 UI 定稿或迭代频率下降后再单独评估，把 UI 改为可热更。
**两者不冲突** —— A 改回 B 只需恢复 `Init.cs` 的那几行。

---

## 七、待确认项

| # | 待确认 | 状态 |
|---|---|---|
| 1 | 选 A 还是 B | ✅ **已定：A**（2026-09-22 负责人） |
| 2 | 本次实施许可（改动触及 `Init.cs`） | ✅ **已授权并已实施**（2026-09-22，两处调用点 + 回落加固），见 §九 |
| 3 | 微信小游戏首包体积上限 / 体验预算 | ✅ **不再是风险**：实测首包不增长（场景本就在播放器里），见 §三更正 |
| 4 | UI 后续是否还会频繁调整 | ⬜ 待确认（决定是否值得后续上 B） |
| 5 | 首页之后是否还有其它场景需要动态加载 | ⬜ 待确认（影响后续收集范围设计） |

---

## 八、附：与本次问题无关但值得记录的两条日志

| 日志 | 判定 |
|---|---|
| `Failed to load web package version file : <CDN>/StreamingAssets/yoo/DefaultPackage/PackageManifest_DefaultPackage.version 404` | **非致命**。YooAsset 回退到根目录并成功：`Updated package Version : 2026-09-22-1420`。若要消除告警，可把版本文件也复制一份到该路径。 |
| `ERROR: Shader Hidden/Universal/HDRDebugView shader is not supported on this GPU` | **非致命**。这是 URP 的调试视图 shader，微信小游戏 WebGL 不支持，不影响业务渲染。 |
| `The referenced script (UnityBridgeManager) on this Behaviour is missing!` | 待核验。`UnityBridgeManager` 是原街机残留组件，其脚本可能未参与当前构建。**与首页无关**（首页属 `Client` 模块），但建议记入待核验项。 |

---

## 九、方案 A 实施与出包记录（2026-09-22 晚）

### 已实施改动（唯一文件 `Assets/Main/Init.cs`，备份 `Logs/Init.cs.before-planA.bak`）

| # | 改动 | 说明 |
|---|---|---|
| 1 | 新增 `LoadFirstSceneAsync(string sceneName)` | 内置场景走 `SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single)`；起不来（异常或返回 `null`）时**回落** `YooAssets.LoadSceneAsync(sceneName)` |
| 2 | 运行时加载点（原 L192） | `yield return LoadFirstSceneAsync(firstSceneName);` |
| 3 | `OnCompleted()`（原 L518，编辑器 `EditorSimulateMode`） | `StartCoroutine(LoadFirstSceneAsync(ResolveFirstSceneName(ClientStartScene)));` —— 交接文档原先只提到 L192，漏了这处；不改它编辑器里 Play `Boot` 同样进不去首页 |

**加固方式说明**：判断"场景是否在构建清单"**没有**使用 `SceneUtility.GetScenePathByBuildIndex` / `Application.CanStreamedLevelBeLoaded` —— 这两个 API 在 WebGL 运行时的可用性**未取证**，按 `AGENTS.md` §4 不作为护栏；改用 `try/catch` + 返回值 `null` 判定，并新增日志 `首个场景加载方式判定：构建清单场景数=N`。

### 验证状态

- ✅ 三目标 Roslyn 编译校验 `Logs/verify-compile.ps1 -Profile All`：`Assembly-CSharp [Editor]` / `[WebGL]` 均 **0 error**（唯一 warning 是既有的 `ClientShopPage._nextCanvasSortingOrder`）。
- ✅ 两阶段出包 `run-pipeline-2phase.ps1 -PackageVersion 2026-09-22-1420`：Phase A 1.3 min + Phase B 7.9 min，**exit=0**；构建日志 `[启动链校验] 场景[1] = Assets/Client/Scenes/ClientShell.unity`、`[Builder] Scenes [1]: …/ClientShell.unity, [x]`。
- ✅ **OSS 上传已复验**：`124561790dc5891e.webgl.data.unityweb.bin.br`、`f646dc49cc019398.webgl.wasm.code.unityweb.wasm.br` 均 **200**；`<CDN>/StreamingAssets/yoo/DefaultPackage/PackageManifest_DefaultPackage.version` 为 **404**（即 §八 记录的那条非致命探测，已由本次实测定位到确切 URL）。
- ✅ **容器内复验通过（2026-09-22 晚，负责人导出的 13,647 行 Console）**：
  `首个场景加载方式判定：构建清单场景数=2，目标=ClientShell`（L2875）→ 全文件 **0 处** `Failed to mapping location to asset path` / `Failed to load scene`
  → `[配置表] … Hero 可用 49 / 表内 54`（L3212）→ `[ClientProfileNavigationController] ActiveVisualCanvases：count=7，paths=ClientCanvas | ClientCanvas/PagesLayer/MainPage主页/…`（L12578）
  → TCP 六包齐全（网关→建连→握手→游客登录→英雄数据 英雄=4/英雄组=1→成功退出）；无异常、无缺失脚本。
- ⚠️ **反例修正（重要，写给自己）**：上面预判的 `首个场景已加载（SceneManager 内置场景）` 日志**实际没有打印**（0 匹配）。
  原因是 `LoadSceneMode.Single` 切场景时 `Boot` 场景被卸载，`Init`（及其协程）一并销毁 —— **判据不能依赖这行日志**。
  后果：`Init.cs` "加载后"那段（`ApplyWebGLRenderCompatibilityToLoadedCameras()` + `WebGLFirstScreenDiagnosticsHost`）不会执行（`WebGLFirstScreen` 也是 0 匹配）。
  这是**既有设计问题，非方案 A 引入**，渲染实测正常；是否修需负责人单独授权（要动 `Init.cs`）。

### A4：加载后逻辑改为跨场景宿主执行（2026-09-22 晚，已授权并实施）

| 项 | 内容 |
|---|---|
| 新增文件 | `Assets/Main/WebGLPostSceneLoadRunner.cs` —— 跨场景宿主，订阅 `SceneManager.sceneLoaded`，执行一次后自毁；**无 `[SerializeField]` 字段** |
| `Init.cs` 改动 | 加载**前**创建该宿主并 `DontDestroyOnLoad`；"加载后"两个动作（`ApplyWebGLRenderCompatibilityToLoadedCameras()` + 首屏诊断宿主）移入新的 `internal static RunPostSceneLoadCompatibility()`，由宿主调用 |
| 为什么不改 `Init` 生命周期 | 探针明确依赖"Boot 场景会被卸载"这一前提（`ClientTcpConnectionProbe.StartPersistentProbe` 的注释），把 `Init` 变持久会改变既有语义 |
| 编译 | `verify-compile.ps1 -Profile All`：0 error（新文件已确认进入 Editor/WebGL 两份 rsp） |
| 出包 | exit=0；构建报告含 `Assets/Main/WebGLPostSceneLoadRunner.cs`；标识 `DATA_FILE_MD5=356e1b81e1be0bb4` / `CODE_FILE_MD5=2edcf1b6d32ca4ab` |
| 复验判据 | `首个场景已加载（sceneLoaded 事件）：ClientShell` + `WebGL 渲染兼容性已注册：已检查 N 台相机` + `[WebGL 首屏诊断 1s]` / `[WebGL 首屏诊断 10s]` |
| 状态 | ✅ **已容器复验通过**（2026-09-22 晚）：`首个场景已加载（sceneLoaded 事件）：ClientShell，模式=Single` + `WebGL 渲染兼容性已注册：已检查 1 台相机` + `[WebGL 首屏诊断 1s] scene=ClientShell, loaded=True, resolution=1170x2532, cameras=1, canvases=72, pipeline=Universal Render Pipeline Asset`；相机侧 `hdr=False, post=False` 证明兼容处理生效 |

### 本次 OSS 重传清单（按哈希比对实测，非推断）

| 项 | 结论 |
|---|---|
| YooAsset 53 个文件（bundle + manifest + version） | **不用传**。本地 YooAsset 产物 150 个文件中 **149 个逐字节相同**，唯一变化的是 `BuildReport_DefaultPackage_2026-09-22-1420.json`（本地构建报告，不在上传清单内） |
| 首包 `data` | **必传**：`124561790dc5891e.webgl.data.unityweb.bin.br`（36,550,487 B），路径 = CDN **根**（`unity-namespace.js` 的 `dataFileSubPrefix: ''`；且 `minigame/data-package/` 分包为空 ⇒ data 只能来自 CDN） |
| 首包 `wasm` | **建议同传**：`f646dc49cc019398.webgl.wasm.code.unityweb.wasm.br`（15,284,582 B）。证据显示它走**本地小游戏分包**（`game.json` 声明 `wasmcode` + `parallelPreloadSubpackages`，文件名由 `game.js:13 CODE_FILE_MD5` 决定），**没有证据**表明容器会从 OSS 取它；同传成本低、可消除不确定性 |
| 旧文件名 | 可以保留不删（新的 `game.js` 只请求新名字） |

> ⚠️ 转换还**删除了** `minigame/project.private.config.json`（开发者工具的本地私有配置，642 B；实测：转换前存在、转换后不存在）。重开项目时工具通常会重新生成；若"不校验合法域名"等本地开关被重置，需要重新勾选。
