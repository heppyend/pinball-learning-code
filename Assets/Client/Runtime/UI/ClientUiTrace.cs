using System.Text;
using UnityEngine;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 客户端 UI 追踪日志（诊断用，可一键关闭）。
    ///
    /// 目的：把"鼠标点了什么 → 谁处理 → 是否发生跳转 → 谁的显隐被改了"串成一条可读证据链，
    /// 避免靠猜现象定位 UI 结构问题。
    ///
    /// 类别：
    ///   [点击]  EventSystem 射线命中的 UI 层级与将接收点击的组件
    ///   [导航]  Open / Back / ReturnHome 的请求、路由解析结果与失败原因
    ///   [弹窗]  弹窗栈的压栈 / 出栈 / 全清，含栈深
    ///   [显隐]  页面 / 弹窗根节点的 SetActive 变化，标明所有者
    ///   [路由]  个人中心内部 3 主面板 + 5 子页的互斥切换
    ///
    /// 关闭方式：把 Enabled 置 false（或删掉 ClientUiClickTracer 组件）。
    /// </summary>
    public static class ClientUiTrace
    {
        /// <summary>
        /// 总开关。诊断结束后设为 false 即可静音，无需改调用点。
        ///
        /// **2026-09-20 起默认 false**（待办 8「调试诊断待清理」）：可见性诊断 / 栈快照 /
        /// 显隐 / 路由 / 导航 / 点击 全部经此开关，置 false 即一次性静音；**调用点原样保留**，
        /// 需要复查时改回 true 即可，不必重新埋点。
        /// </summary>
        public static bool Enabled = false;

        /// <summary>是否记录每次点击（由 ClientUiClickTracer 驱动）。受 <see cref="Enabled"/> 共同约束。</summary>
        public static bool LogClicks = true;

        private static readonly StringBuilder Builder = new StringBuilder(256);

        public static void Line(string category, string message)
        {
            if (!Enabled)
                return;
            Debug.Log("[UI追踪][" + category + "] " + message);
        }

        public static void Warn(string category, string message)
        {
            if (!Enabled)
                return;
            Debug.LogWarning("[UI追踪][" + category + "] " + message);
        }

        // ------------------------------------------------------------------
        // 点击
        // ------------------------------------------------------------------

        public static void Click(Vector2 screenPosition, string hitSummary, string handlerSummary)
        {
            if (!Enabled || !LogClicks)
                return;
            Debug.Log("[UI追踪][点击] 屏幕坐标=" + screenPosition +
                      "\n    命中层级: " + hitSummary +
                      "\n    接收组件: " + handlerSummary);
        }

        public static void ClickMiss(Vector2 screenPosition)
        {
            if (!Enabled || !LogClicks)
                return;
            Debug.Log("[UI追踪][点击] 屏幕坐标=" + screenPosition + " 未命中任何 UI（点到了空处）");
        }

        // ------------------------------------------------------------------
        // 节点路径（证据里必须能唯一定位到 Hierarchy 里的谁）
        // ------------------------------------------------------------------

        public static string Path(Transform transform)
        {
            if (transform == null)
                return "(null)";

            Builder.Length = 0;
            Transform current = transform;
            int guard = 0;
            while (current != null && guard < 64)
            {
                if (Builder.Length > 0)
                    Builder.Insert(0, '/');
                Builder.Insert(0, current.name);
                current = current.parent;
                guard++;
            }
            return Builder.ToString();
        }

        public static string Path(GameObject target)
        {
            return target == null ? "(null)" : Path(target.transform);
        }

        /// <summary>描述一个 MonoBehaviour 的宿主路径与类型，用于标明"谁改的"。</summary>
        public static string Owner(MonoBehaviour behaviour)
        {
            if (behaviour == null)
                return "(null)";
            return behaviour.GetType().Name + "@" + Path(behaviour.transform);
        }
    }
}
