using UnityEngine;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 客户端组合根（Composition Root）。
    ///
    /// 唯一职责：把 UI 内核服务装配给需要它们的视图，使视图不再依赖静态单例
    /// 或全局查找。所有槽位都可以留空，留空时会在自身子树内自动解析，
    /// 因此常规情况下不需要人工拖拽；需要替换实现时再手动指定。
    ///
    /// 与 ClientUiNavigator 的分工：
    /// - ClientUiNavigator 自己负责 IClientNavigation 的注入（导航是它自身的服务），
    ///   因此即使本组件缺席，页面导航依然可用；
    /// - 本组件负责弹窗服务与全局提示的注入。
    ///
    /// 位置：挂在 ClientCanvas（或 ClientShell 的场景根）上即可。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ClientShellController : MonoBehaviour
    {
        [Tooltip("弹窗服务；留空则在子树内自动查找。")]
        [SerializeField] private ClientPopupService _popupService;

        [Tooltip("全局提示；留空则在子树内自动查找。")]
        [SerializeField] private ClientSystemFeedback _feedback;

        private void Awake()
        {
            ResolveMissingServices();
            EnsureDiagnostics();
        }

        /// <summary>
        /// 诊断期：确保点击追踪器存在，用于记录"鼠标点了什么 / 谁接收 / 是否跳转"。
        ///
        /// 这是纯诊断组件，不创建任何 UI。诊断结束后有两种关闭方式：
        ///   1) 把 ClientUiTrace.Enabled 置 false（静音但保留）；
        ///   2) 移除本方法与场景上的 ClientUiClickTracer 组件。
        /// </summary>
        private void EnsureDiagnostics()
        {
            if (GetComponent<ClientUiClickTracer>() == null)
                gameObject.AddComponent<ClientUiClickTracer>();
            if (GetComponent<ClientUiPerfProbe>() == null)
                gameObject.AddComponent<ClientUiPerfProbe>();

            // 关键性能项：Unity 默认给每条 Debug.Log / LogWarning 附带完整调用栈。
            // 追踪开启后每次点击都会产生多条日志，控制台要解析并渲染这些栈，
            // 是"Play 模式发卡 + 点击后延迟但最终仍生效"最常见的成因。
            // 这里只关掉 Log / Warning 的栈（Error / Exception 仍保留完整栈，便于排错）。
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

            ClientUiTrace.Line("追踪", "UI 追踪已启用（Log/Warning 已关闭栈回溯）");
        }

        private void Start()
        {
            InjectServices();
        }

        private void ResolveMissingServices()
        {
            if (_popupService == null)
                _popupService = GetComponentInChildren<ClientPopupService>(true);
            if (_feedback == null)
                _feedback = GetComponentInChildren<ClientSystemFeedback>(true);
        }

        private void InjectServices()
        {
            MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            int popupBound = 0;
            int feedbackBound = 0;

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (_popupService != null && behaviours[i] is IClientPopupHost popupHost)
                {
                    popupHost.BindPopups(_popupService);
                    popupBound++;
                }
                if (_feedback != null && behaviours[i] is IClientFeedbackHost feedbackHost)
                {
                    feedbackHost.BindFeedback(_feedback);
                    feedbackBound++;
                }
            }

            if (_popupService == null)
                Debug.LogWarning("[Client] 未找到 ClientPopupService：弹窗功能不可用。");
            if (_feedback == null)
                Debug.LogWarning("[Client] 未找到 ClientSystemFeedback：全局提示不可用。");

            Debug.Log("[Client] 组合根注入完成：弹窗宿主 " + popupBound + " 个，提示宿主 " + feedbackBound + " 个。");
        }
    }
}
