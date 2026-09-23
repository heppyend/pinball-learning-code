namespace Pinball.Client.UI
{
    /// <summary>
    /// 全局轻提示（SystemLayer 的一部分）。
    ///
    /// 取代原先 ClientUiFeedback 的静态单例 + static ShowToast：
    /// 静态单例在场景未挂载时静默 no-op（此前 Toast 实际处于死代码状态），
    /// 且无法在测试中替换。本接口由 ClientShellController 注入到需要提示的视图。
    /// </summary>
    public interface IClientFeedback
    {
        /// <summary>显示一条轻提示；重复调用会重置计时。</summary>
        void ShowToast(string message);

        /// <summary>立即隐藏提示。</summary>
        void HideToast();
    }

    /// <summary>需要弹提示的 View 实现此接口以接收注入。</summary>
    public interface IClientFeedbackHost
    {
        void BindFeedback(IClientFeedback feedback);
    }
}
