using UnityEngine;

namespace Pinball.Client
{
    /// <summary>
    /// 加载页的**直连入口** —— 给启动流程一个不依赖 `EventCenter` 静态事件总线的最小接口。
    ///
    /// 为什么需要它（2026-09-20 只读取证）：<see cref="ClientLoadingPage"/> 能力齐全
    /// （进度 / 平滑 / 加载点动画 / 文案 / 图片槽位），但**全工程没有任何运行时调用方**。
    /// 老启动流程 `Assets/Main/Init.cs` 的资源下载里其实已经算好了进度：
    /// <code>
    /// downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;   // 372 行
    /// double progress = (double)currentDownloadBytes / totalDownloadBytes;       // 405 行
    /// //EventCenter.Instance.EventTrigger("更新下载进度", (float)progress);        // ← 被注释掉
    /// Debug.LogError(progress);                                                  // ← 只剩刷屏
    /// </code>
    /// 于是进度被丢掉、加载页从没被调用。本类让启动流程改成直接调用：
    /// <code>
    /// ClientLoadingService.Begin("正在检查更新");
    /// ClientLoadingService.SetProgress(0.42f);          // 0..1（内部平滑）
    /// ClientLoadingService.SetLoadingText("正在下载资源");
    /// ClientLoadingService.Complete();                  // 进度满 + 停动画
    /// ClientLoadingService.Hide();                      // 收尾隐藏
    /// </code>
    ///
    /// ⚠️ **SystemLayer 在场景里恒为 `inactive`**（已取证），而 `加载Page` 就在其中、且自身
    /// `activeSelf = 1` —— 因此**一旦激活该层就会立刻全屏显示**。本服务**显式接管该层开关**：
    /// `Begin()` 打开、`Hide()` 关闭，**不会让这层常开**。
    ///
    /// ⚠️ 找不到加载页时**全部降级为 no-op**（只警告一次），绝不抛异常打断启动流程。
    ///
    /// 职责边界：本类只把状态喂给**已有** UI，**不新建/不移动任何节点，也不改尺寸与排版**。
    /// </summary>
    public static class ClientLoadingService
    {
        private const string SystemLayerName = "SystemLayer";

        private static ClientLoadingPage _page;
        private static GameObject _layer;
        private static bool _resolved;
        private static bool _missingWarned;

        /// <summary>加载页当前是否正在显示。</summary>
        public static bool IsShowing
        {
            get { return _page != null && _page.gameObject.activeInHierarchy; }
        }

        /// <summary>
        /// 开始加载：打开 `SystemLayer` 与加载页，进度归零并启动加载点动画。
        /// <paramref name="text"/> 非空时同时设置文案。
        /// </summary>
        public static void Begin(string text = null)
        {
            if (!Resolve())
                return;

            // 先开层、再开页：加载页挂在恒 inactive 的 SystemLayer 下，不先开层它不会显示。
            if (_layer != null && !_layer.activeSelf)
                _layer.SetActive(true);
            if (!_page.gameObject.activeSelf)
                _page.gameObject.SetActive(true);

            _page.Begin();
            if (!string.IsNullOrEmpty(text))
                _page.SetLoadingText(text);
        }

        /// <summary>设置进度（0..1，内部平滑过渡）。</summary>
        public static void SetProgress(float value)
        {
            if (!Resolve())
                return;
            _page.SetProgress(value);
        }

        /// <summary>设置进度（0..100）。</summary>
        public static void SetProgressPercent(int percent)
        {
            if (!Resolve())
                return;
            _page.SetProgressPercent(percent);
        }

        /// <summary>设置加载文案。</summary>
        public static void SetLoadingText(string text)
        {
            if (!Resolve())
                return;
            _page.SetLoadingText(text);
        }

        /// <summary>
        /// 标记完成：进度置满并停止加载点动画。**不会自动隐藏** ——
        /// “满进度停留多久”由调用方决定，之后调 <see cref="Hide"/> 收尾
        /// （本类是静态类，不持有协程/计时器，避免替产品决定停留时长）。
        /// </summary>
        public static void Complete()
        {
            if (!Resolve())
                return;
            _page.Complete();
        }

        /// <summary>隐藏加载页，并关闭由本服务打开的 `SystemLayer`。</summary>
        public static void Hide()
        {
            if (_page != null && _page.gameObject.activeSelf)
                _page.SetVisible(false);
            if (_layer != null && _layer.activeSelf)
                _layer.SetActive(false);
        }

        /// <summary>清空缓存（退出 Play / 域重载后重新解析）。</summary>
        public static void Reset()
        {
            _page = null;
            _layer = null;
            _resolved = false;
            _missingWarned = false;
        }

        private static bool Resolve()
        {
            if (_resolved && _page != null)
                return true;

            _resolved = true;

            // includeInactive = true 是必须的：加载页平时挂在 **inactive 的 SystemLayer** 下。
            // 也正因为它在 inactive 层级里无法靠自身 Awake 注册，才在这里做**一次性**解析并缓存
            // （不是每帧轮询，符合 AGENTS.md “避免隐式全局查找/重复分配”）。
            _page = Object.FindObjectOfType<ClientLoadingPage>(true);
            if (_page == null)
            {
                if (!_missingWarned)
                {
                    _missingWarned = true;
                    Debug.LogWarning("[Client] 未找到 ClientLoadingPage，加载页相关调用将全部忽略（不打断启动流程）。");
                }
                return false;
            }

            // 往上找 SystemLayer，由本服务负责它的开关。
            Transform node = _page.transform;
            while (node != null && node.name != SystemLayerName)
                node = node.parent;
            _layer = node != null ? node.gameObject : null;
            if (_layer == null)
                Debug.LogWarning("[Client] 加载页不在 SystemLayer 下，Begin 只切页面自身、不切层。");

            return true;
        }
    }
}
