using UnityEngine;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 页面 View 的公共基类，提供 IClientPageView 的默认实现。
    ///
    /// 设计意图（MVC 的 View 侧）：
    /// - View 只拥有一条可见性通路（PageRoot），不再与 ClientUiPage 的
    ///   SetActive 并行控制同一个节点——那是此前"两处改写可见性"的结构问题根源。
    /// - View 只负责显示与刷新自己的控件；数据由 Controller 通过 Bind/Show 传入。
    /// - 具体页面只需覆写 PageRoot，并按需覆写 OnPageRefresh / OnPageHidden。
    ///
    /// 同时实现 IClientPopupView：两者的生命周期是同构的
    /// （进入/暂停/恢复/离开 ↔ 打开/挂起/恢复/关闭）。
    /// 这样同一个 View 既能作为 PagesLayer 页面由 ClientUiNavigator 管理，
    /// 也能作为 PopupLayer 弹窗由 ClientPopupService 管理，
    /// 节点归属哪一层由它挂 ClientUiPage 还是 ClientUiPopup 决定，无需改 View 代码。
    ///
    /// 迁移期约定：PageRoot 返回该页面在场景中的根节点（通常即页面自身或其
    /// _pageRoot 字段）。迁移完成后由 ClientUiNavigator 统一按 IClientPageView 派发，
    /// 不再需要按具体类型 GetComponent。
    /// </summary>
    public abstract class ClientPageViewBase : MonoBehaviour, IClientPageView, IClientPopupView
    {
        /// <summary>页面根节点。返回 null 时视为页面即自身。</summary>
        protected virtual GameObject PageRoot { get { return gameObject; } }

        /// <summary>进入/恢复时刷新视图。数据由 Controller 预先通过 Bind 类方法写入。</summary>
        protected virtual void OnPageRefresh() { }

        /// <summary>隐藏时的清理钩子（停止动画等）。不得在此解除导航注册。</summary>
        protected virtual void OnPageHidden() { }

        /// <summary>离开页面栈时的清理钩子（解除数据监听等）。</summary>
        protected virtual void OnPageExited() { }

        public virtual void OnPageEnter()
        {
            SetRootActive(true);
            OnPageRefresh();
        }

        public virtual void OnPagePause()
        {
            OnPageHidden();
            SetRootActive(false);
        }

        public virtual void OnPageResume()
        {
            SetRootActive(true);
            OnPageRefresh();
        }

        public virtual void OnPageExit()
        {
            OnPageExited();
            SetRootActive(false);
        }

        // ------------------------------------------------------------------
        // IClientPopupView —— 与页面生命周期同构的桥接实现
        // 使同一份 View 代码既可作为页面、也可作为弹窗被管理。
        //
        // 注意 Open/Resume/Close 直接复用页面语义，但 **Suspend 不能复用 OnPagePause**。
        // ------------------------------------------------------------------

        public virtual void OnPopupOpen() { OnPageEnter(); }

        /// <summary>
        /// 弹窗被挂起：**只暂停逻辑，不隐藏节点**。
        ///
        /// 页面语义下"暂停"＝离开当前页，可以隐藏；但**弹窗栈语义**下，被挂起的弹窗是
        /// 栈顶弹窗的背景，必须继续可见。若复用 OnPagePause（其内部会 SetRootActive(false)），
        /// 打开二级弹窗时身后的一级界面会消失、露出大厅 ——
        /// 实测现象：打开 Product Purchase Interface 后，背景的商店消失、直接看到主页。
        /// </summary>
        public virtual void OnPopupSuspend() { OnPageHidden(); }

        public virtual void OnPopupResume() { OnPageResume(); }

        public virtual void OnPopupClose() { OnPageExit(); }

        private void SetRootActive(bool active)
        {
            GameObject root = PageRoot;
            if (root == null)
                root = gameObject;
            if (root.activeSelf == active)
                return;

            ClientUiTrace.Line("显隐", (active ? "显示 " : "隐藏 ") + ClientUiTrace.Path(root) +
                                       "   所有者=" + GetType().Name);
            root.SetActive(active);
        }
    }
}
