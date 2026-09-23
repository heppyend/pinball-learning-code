using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 列表滚动视图的**运行时**修正。
    ///
    /// <para><b>背景（2026-09-21 负责人反馈）</b>：排行榜与编队等页面"下滑松手后被强制拉回、默认不显示最上面一条、
    /// 只显示 6 个"。**场景实测的根因有三条**（都不是代码逻辑错）：</para>
    /// <list type="number">
    /// <item>`Content` 的 `ContentSizeFitter` 是 **Unconstrained**（`m_VerticalFit: 0`）⇒ **内容从不撑高**，
    /// 只有作者高度可见，于是列表"只有几条"且**没有滚动范围**（一滑就被弹回）；</item>
    /// <item>`ScrollRect.m_MovementType` 是 **Elastic**（`1`）⇒ 松手**弹回**；</item>
    /// <item>`GridLayoutGroup.m_ChildAlignment` 是 **MiddleCenter**（`4`）⇒ 内容**垂直居中**，默认看到的是中间而不是顶部。</item>
    /// </list>
    ///
    /// <para>本工具只把这三项改成标准的**竖向列表**配置（撑高 + Clamped + 顶部对齐 + 轴心在顶），
    /// **不写场景、不改美术尺寸与排版**，可随时停用。</para>
    /// </summary>
    public static class ClientScrollFix
    {
        /// <summary>修正页面内**所有**竖向滚动列表；<paramref name="resetToTop"/> 为 true 时回到最上面。</summary>
        public static int FixAll(GameObject pageRoot, bool resetToTop)
        {
            if (pageRoot == null)
                return 0;

            int fixedCount = 0;
            ScrollRect[] scrolls = pageRoot.GetComponentsInChildren<ScrollRect>(true);
            for (int index = 0; index < scrolls.Length; index++)
                if (Fix(scrolls[index], resetToTop))
                    fixedCount++;
            return fixedCount;
        }

        /// <summary>按列表容器（`Content`）修正它所属的竖向滚动列表。</summary>
        public static bool FixContainer(RectTransform content, bool resetToTop)
        {
            if (content == null)
                return false;

            ScrollRect scroll = content.GetComponentInParent<ScrollRect>(true);
            if (scroll == null)
                return false;

            return Fix(scroll, resetToTop);
        }

        /// <summary>修正单个竖向滚动列表。横向列表与二维滚动**不动**（避免影响其它排版）。</summary>
        public static bool Fix(ScrollRect scroll, bool resetToTop)
        {
            if (scroll == null || scroll.content == null)
                return false;
            if (!scroll.vertical || scroll.horizontal)
                return false;

            // 1) 松手不再弹回
            if (scroll.movementType != ScrollRect.MovementType.Clamped)
                scroll.movementType = ScrollRect.MovementType.Clamped;

            RectTransform content = scroll.content;

            // 2) **只调整竖直方向**：顶部锚点 + 轴心在顶（保证"第一条在最上面"）。
            //
            // ⚠️ 2026-09-21 负责人复验截图（排行榜整列被推到右半边、右边缘被裁掉）：
            // 上一版把 **水平**也改成了拉伸（anchorMin.x=0 / anchorMax.x=1），容器宽度于是变成
            // "视口宽 + 原本的 sizeDelta.x"，比作者设定宽很多 ⇒ 网格按居中排布后**整列右移并被裁切**。
            // 因此这里**不动水平锚点**。
            content.anchorMin = new Vector2(content.anchorMin.x, 1f);
            content.anchorMax = new Vector2(content.anchorMax.x, 1f);

            // 2b) 竖直轴心钉在顶部；水平轴心**只在一种情况下修正**：
            //     水平锚点是"固定"的（anchorMin.x == anchorMax.x，即这个列表本来就不横向拉伸），
            //     而轴心却落在别处 —— 此时 `anchoredPosition.x` 的语义与锚点不一致，运行时会整列平移。
            //     排行榜两份列表正是这个组合（中锚 `(0.5,0.5)` + pivot `(0,1)` + offset.x ≈ 376）。
            //
            //     ⚠️ 但**不在这里搬家**：同样组合在**邮件页**与**商店页**也存在，
            //     它们在 1080×1920 下可能本来就摆对了（见 `Logs/scrollfix-blast-radius.ps1`）。
            //     位置由场景侧工具负责（`ClientShellStructuralRepair.FixRankPageLists()`），
            //     运行时只做"轴心与锚点语义对齐"这条**不产生位移**的修正。
            bool horizontallyFixed = Mathf.Approximately(content.anchorMin.x, content.anchorMax.x);
            content.pivot = new Vector2(horizontallyFixed ? content.anchorMin.x : 0.5f, 1f);

            // 3) 让容器按内容撑高（否则没有滚动范围）
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter != null && fitter.verticalFit != ContentSizeFitter.FitMode.PreferredSize)
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 4) 对齐：**只把竖直分量改成顶部**，保留作者设定的水平分量（Left/Center/Right）。
            //    TextAnchor 的编码是：0..2 = Upper{Left,Center,Right}，3..5 = Middle…，6..8 = Lower…
            //    因此 `value % 3` 就是水平分量。
            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                TextAnchor target = (TextAnchor)(((int)grid.childAlignment) % 3);
                if (grid.childAlignment != target)
                    grid.childAlignment = target;
            }

            VerticalLayoutGroup column = content.GetComponent<VerticalLayoutGroup>();
            if (column != null)
            {
                TextAnchor target = (TextAnchor)(((int)column.childAlignment) % 3);
                if (column.childAlignment != target)
                    column.childAlignment = target;
            }

            // 4) 立即重排，再决定是否回到顶部
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            if (resetToTop)
                scroll.verticalNormalizedPosition = 1f;

            return true;
        }
    }
}
