using UnityEngine;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 弹窗身份与可见性锚点（对应 ClientUiPage 在页面层的角色）。
    ///
    /// 挂在 PopupLayer 下每个弹窗的根节点上。可见性只由 ClientPopupService 驱动，
    /// 场景里不要用序列化的 SetActive 回调另行控制，否则会出现"两个所有者"。
    ///
    /// 需要的弹窗专用逻辑（如扭蛋结果）另写脚本实现 IClientPopupView，
    /// 与服务同节点即可被自动找到；没有专用脚本的纯展示弹窗无需任何额外脚本。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ClientUiPopup : MonoBehaviour
    {
        [Tooltip("弹窗根节点。留空表示本节点即根。")]
        [SerializeField] private GameObject _popupRoot;

        [Tooltip("打开时是否用遮罩阻断下层输入。纯提示类可不勾。")]
        [SerializeField] private bool _blocksInput = true;

        [Tooltip("进入运行时自动隐藏。场景中可保持可见以便编辑排版。")]
        [SerializeField] private bool _hideOnAwake = true;

        [Tooltip("用于诊断与幂等判定的可读名字；留空则用节点名。")]
        [SerializeField] private string _popupKey;

        [Tooltip("该弹窗对应的页面 ID。ClientPopupService 用它充当 IClientNavigation 的路由表，" +
                 "这样既有的页面 View 无需改动即可继续调用 Open(pageId)/Back()/ReturnHome()。")]
        [SerializeField] private ClientUiPageId _pageId = ClientUiPageId.Home;

        [Tooltip("是否参与 pageId → 弹窗 的路由表。\n" +
                 "一级全屏界面（个人中心/英雄/商城…）保持勾选；\n" +
                 "二级弹窗（购买确认/购买成功/邮件详情/玩家阵容…）由调用方持引用直接 Open，" +
                 "不占用 pageId，应取消勾选，否则会与一级界面争抢同一个 ID。")]
        [SerializeField] private bool _routeByPageId = true;

        public ClientUiPageId PageId { get { return _pageId; } }

        public bool RouteByPageId { get { return _routeByPageId; } }

        public bool BlocksInput { get { return _blocksInput; } }

        public string PopupKey
        {
            get { return string.IsNullOrEmpty(_popupKey) ? name : _popupKey; }
        }

        /// <summary>弹窗根节点。用于让服务做 SetAsLastSibling 排序。</summary>
        public GameObject Root
        {
            get { return _popupRoot != null ? _popupRoot : gameObject; }
        }

        /// <summary>
        /// Awake 的自隐藏是否应当跳过。
        ///
        /// **为什么必须有这个标志**：场景里初始隐藏的弹窗，其 `Awake` 直到**第一次
        /// `SetVisible(true)`** 才会被触发 —— 因为 `SetActive(true)` 会在内部**同步**调用 `Awake`。
        /// 若 `Awake` 此时无条件自隐藏，就会把刚显示出来的自己立刻关回去，
        /// 表现为"服务层日志说弹窗已打开（栈深+1），屏幕上却什么都没有"。
        ///
        /// 实测证据（2026-09-20，可见性诊断）：
        ///   Open .../Product Purchase Interface 栈深=2
        ///   紧接着 activeSelf=False、首个未激活祖先=(无)、兄弟序号=18/18
        ///   —— 说明不是被祖先连坐、也不是被压在别人后面，而是它自己被关掉了。
        /// 这也解释了为什么 `ShopPage商店` 能正常显示：它是路由弹窗，早已被激活过一次，
        /// `Awake` 不会再跑；而二级弹窗是第一次被激活，正好撞上。
        /// </summary>
        private bool _suppressHideOnAwake;

        private void Awake()
        {
            if (_popupRoot == null)
                _popupRoot = gameObject;
            if (_hideOnAwake && !_suppressHideOnAwake)
                SetVisible(false);
        }

        /// <summary>仅供 ClientPopupService 调用。</summary>
        internal void SetVisible(bool visible)
        {
            if (_popupRoot == null)
                _popupRoot = gameObject;

            // 必须在 SetActive 之前置位：Awake 是在 SetActive(true) 内部同步执行的。
            if (visible)
                _suppressHideOnAwake = true;

            if (_popupRoot.activeSelf != visible)
                _popupRoot.SetActive(visible);
        }
    }
}
