using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pinball.Client
{
    /// <summary>
    /// **`BUG-025` 方案 C**：客户端专用启动场景 `ClientBoot` 里的唯一组件。
    ///
    /// <para><b>⚠️ 现状（2026-09-22）：本组件已不在启动链中，`ClientBoot` 也已从构建清单移除。</b>
    /// 保留文件是因为编辑器工具 `ClientShellStructuralRepair.EnsureClientBootScene()` 仍按类型名引用它，
    /// 且删掉它会让 `ClientBoot.unity` 出现 missing script。**不要**把它重新加回构建清单，
    /// 除非同时解决下面的 HotFix 问题。</para>
    ///
    /// <para><b>为什么弃用</b>：方案 C 能解决"客户端场景没有入口"，但它用
    /// `SceneManager.LoadScene(name)`（单参 = <c>LoadSceneMode.Single</c>）替换场景，
    /// 结果 `Assets/Main/Boot.unity` **永远不会被加载**；而 `Boot` 上的 `Init` 是**全工程唯一**
    /// 加载 HybridCLR 热更程序集 `HotFix` 的地方（`Init.cs:216` `Assembly.Load(...)`，其前一步
    /// `:208` 还需先加载 AOT 补充元数据，两者都从 YooAsset 包取字节）。
    ///
    /// <para>后果：`HotFix` 不进 AppDomain ⇒ 其中的 `NetController` / `SysDefines` 不存在
    /// ⇒ TCP 探针（`ClientTcpConnectionProbe`，反射这两个类型，等待上限 15 秒）必然失败。
    /// 客户端 UI 不受影响，因为它只用 `Resources/Table` 本地表与本地模拟数据、零 YooAsset 依赖 —— 
    /// 所以表现为"界面能跑、网络不通"。
    /// 完整分析见工程根目录 `TCP_NETWORK_BREAK_ROOTCAUSE.md`。</para>
    ///
    /// <para><b>现行设计（设计 A）</b>：构建清单回归 <c>Boot.unity</c> 唯一入口，
    /// 由 `Boot` 上的 `Init` 通过其 `ClientStartScene` 字段（值 <c>ClientShell</c>）把首个场景指向客户端。
    /// `Init` 会先跑完 YooAsset → AOT 元数据 → `HotFix` 加载，因此 TCP 探针能反射到目标。
    /// 探针挂在 `Boot.unity` 的 `Init` 对象上（历史位置），靠自身的 `StartPersistentProbe()`
    /// 迁移到 `DontDestroyOnLoad` 对象以存活过场景切换。</para>
    ///
    /// <para><b>如果将来仍想要"客户端独立入口"</b>：必须改用附加加载（`LoadSceneMode.Additive`）
    /// 让 `Boot` 的初始化链先跑完，并显式等待 `HotFix` 就绪后再切 `ClientShell`；
    /// 不能像本组件这样直接用 Single 模式替换场景。</para>
    /// </summary>
    public sealed class ClientBootLoader : MonoBehaviour
    {
        [Tooltip("要加载的客户端场景名（必须已加入 Build Settings）。")]
        [SerializeField] private string _clientSceneName = "ClientShell";

        private void Start()
        {
            if (string.IsNullOrEmpty(_clientSceneName))
            {
                Debug.LogError("[ClientBoot] 未指定客户端场景名，无法启动客户端。");
                return;
            }

            Debug.LogWarning("[客户端启动] ClientBoot 入口已弃用（它会让 Boot/Init 不被加载，导致 HotFix 与 TCP 断链）。" +
                             "当前启动链应为 Boot.unity → Init → ClientShell。详见 TCP_NETWORK_BREAK_ROOTCAUSE.md");
            SceneManager.LoadScene(_clientSceneName);
        }
    }
}
