namespace Pinball.Client.UI
{
    /// <summary>
    /// 页面视图契约（MVC 的 View 侧）。
    ///
    /// 由每个页面根节点上的 View 组件实现，向导航内核暴露统一生命周期，
    /// 使内核不再需要知道任何具体页面类型（原先 ClientUiNavigator 用 if/else
    /// 逐个 GetComponent 具体类型来刷新页面）。
    ///
    /// 实现方职责边界：
    /// - 只负责显示/隐藏自己，以及刷新自己持有的控件引用；
    /// - 不持有运营数值、奖励/扣费规则、服务端规则；
    /// - 不直接发起导航，导航能力由 IClientNavigation 注入（见 IClientNavigationHost）。
    /// </summary>
    public interface IClientPageView
    {
        /// <summary>
        /// 成为当前页（首次进入，或从主页切换过来）。
        /// 应在此完成数据绑定与一次刷新。
        /// </summary>
        void OnPageEnter();

        /// <summary>
        /// 被压入返回栈而隐藏。应停止仅前台需要的表现（动画、轮询）。
        /// 不得在此解除导航注册。
        /// </summary>
        void OnPagePause();

        /// <summary>
        /// 从返回栈恢复为当前页。必须可重复调用，不得重复订阅事件。
        /// </summary>
        void OnPageResume();

        /// <summary>
        /// 离开页面栈。应解除数据监听并清理临时状态，避免遗留遮罩或输入阻断。
        /// </summary>
        void OnPageExit();
    }
}
