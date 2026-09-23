namespace Pinball.Client.UI
{
    /// <summary>
    /// 弹窗视图契约（PopupLayer 的 View 侧）。
    ///
    /// 与页面生命周期对称：弹窗被压栈显示、被更上层弹窗覆盖、被恢复、出栈。
    /// 实现方只负责自己这一层的显隐与控件刷新，不得自己关闭别的弹窗，
    /// 也不得直接改动遮罩或跨页面的全局界面。
    /// </summary>
    public interface IClientPopupView
    {
        /// <summary>成为栈顶并显示。</summary>
        void OnPopupOpen();

        /// <summary>被更上层弹窗覆盖而隐藏。不得在此释放需要恢复的状态。</summary>
        void OnPopupSuspend();

        /// <summary>上层弹窗关闭，重新成为栈顶。</summary>
        void OnPopupResume();

        /// <summary>出栈关闭。应解除本次弹窗期间的临时监听。</summary>
        void OnPopupClose();
    }

    /// <summary>
    /// 弹窗服务：PopupLayer 的唯一所有者。
    ///
    /// 负责弹窗栈顺序、遮罩显隐、输入阻断与关闭回调，保证任何页面切换或
    /// 返回操作之后都不会遗留遮罩或输入阻断（HANDOVER_PLAN §3.1）。
    /// </summary>
    public interface IClientPopupService
    {
        /// <summary>当前是否有打开的弹窗。</summary>
        bool HasOpenPopup { get; }

        /// <summary>弹窗栈深度。</summary>
        int OpenCount { get; }

        /// <summary>打开弹窗并压栈。已打开的同名实例不会重复入栈。</summary>
        void Open(ClientUiPopup popup);

        /// <summary>关闭指定弹窗（若在栈中）。</summary>
        void Close(ClientUiPopup popup);

        /// <summary>关闭栈顶弹窗。返回是否有弹窗被关闭，供返回键判定。</summary>
        bool CloseTop();

        /// <summary>关闭全部弹窗并隐藏遮罩。页面级导航前必须调用。</summary>
        void CloseAll();
    }

    /// <summary>需要开关弹窗的 View 实现此接口以接收注入。</summary>
    public interface IClientPopupHost
    {
        void BindPopups(IClientPopupService popups);
    }
}
