using System.Collections.Generic;
using System.IO;
using System.Text;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pinball.Client.Editor
{
    /// <summary>
    /// ClientShell 层级普查（**只读**，绝不写入场景）。
    ///
    /// 目的：把"UI 层级不合理"从主观感受变成一份可核对的清单，
    /// 供后续"按页面逐个收敛"排优先级，而不是靠一次次 Play 撞 bug。
    ///
    /// 报告落盘：Logs/hierarchy-audit.txt（同时打印摘要到 Console）。
    /// </summary>
    public static class ClientHierarchyAudit
    {
        private const string ScenePath = "Assets/Client/Scenes/ClientShell.unity";
        private const string ReportPath = "Logs/hierarchy-audit.txt";

        private static readonly string[] LayerNames = { "PagesLayer", "PopupLayer", "SystemLayer" };

        /// <summary>页面归属关键词：用于发现"从别的页面复制过来的残留节点名"。</summary>
        private static readonly Dictionary<string, string[]> PageKeywords = new Dictionary<string, string[]>
        {
            { "ShopPage商店",   new[] { "邮件", "排行", "英雄", "个人中心" } },
            { "MailPage邮件",   new[] { "商店", "排行", "英雄", "个人中心", "道具卡牌" } },
            { "RankPage排行榜", new[] { "邮件", "商店", "个人中心" } },
            { "HeroPage英雄",   new[] { "邮件", "商店", "排行", "个人中心" } },
            { "ActivityPage活动", new[] { "邮件", "商店", "排行", "个人中心" } },
        };

        [MenuItem("Client/结构修复/6. 层级普查报告（只读）", false, 15)]
        public static void RunAudit()
        {
            if (!Application.isBatchMode)
            {
                Scene active = EditorSceneManager.GetActiveScene();
                if (active.isDirty)
                {
                    bool proceed = EditorUtility.DisplayDialog(
                        "层级普查",
                        "当前场景有未保存的改动。\n\n普查会重新打开 ClientShell.unity，未保存的改动将丢失。\n\n" +
                        "建议先在 Unity 里按 Ctrl+S 保存。是否继续？",
                        "继续（丢弃未保存改动）", "取消");
                    if (!proceed)
                        return;
                }
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            string report = BuildReport(scene);
            File.WriteAllText(ReportPath, report, new UTF8Encoding(false));
            Debug.Log("[层级普查] 报告已写入 " + ReportPath + "\n" + BuildSummary(report));
            Finish();
        }

        private static void Finish()
        {
            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        }

        // ------------------------------------------------------------------

        private static string BuildReport(Scene scene)
        {
            GameObject[] all = Object.FindObjectsOfType<GameObject>(true);
            List<GameObject> sceneObjects = new List<GameObject>(all.Length);
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].scene == scene)
                    sceneObjects.Add(all[i]);

            StringBuilder sb = new StringBuilder(64 * 1024);
            sb.AppendLine("================ ClientShell 层级普查报告 ================");
            sb.AppendLine("生成时间: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("场景: " + ScenePath);
            sb.AppendLine("GameObject 总数: " + sceneObjects.Count);
            sb.AppendLine();

            Canvas canvas = FindClientCanvas(sceneObjects);
            if (canvas == null)
            {
                sb.AppendLine("!! 未找到 ClientCanvas，无法继续。");
                return sb.ToString();
            }

            AppendOverview(sb, canvas, sceneObjects);
            AppendGlobalIssues(sb, canvas, sceneObjects, scene);
            AppendPerPage(sb, canvas, scene);

            // 列表节点数从场景实时统计，**不硬编码**——随收敛推进自动更新。
            // 前缀同时匹配旧的内联名与新的模板名（道具卡牌prefab/道具卡牌Item、邮件prefab/邮件Item）。
            int shopCards = CountChildrenStartingWith(
                FindExactPath(canvas.transform, "PopupLayer/ShopPage商店/Product page/展示卡牌Scroll View/Viewport/Content"),
                "道具卡牌");
            int mailCards = CountChildrenStartingWith(
                FindExactPath(canvas.transform, "PopupLayer/MailPage邮件/Email come/邮件Scroll View/Viewport/Content"),
                "邮件");
            AppendDataCorrespondence(sb, shopCards, mailCards);
            AppendFullScreenReadiness(sb, canvas);
            AppendCanvasLayoutAudit(sb, canvas);
            AppendTopAreaAnchorScan(sb, canvas);
            AppendHeroPageForensics(sb, canvas);
            AppendCardTemplateForensics(sb, canvas);

            sb.AppendLine();
            sb.AppendLine("=========== 报告结束 ===========");
            return sb.ToString();
        }

        private static string BuildSummary(string report)
        {
            StringBuilder sb = new StringBuilder();
            string[] lines = report.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("【") || line.Contains("命中面来自祖先") ||
                    line.Contains("装饰 Graphic 仍参与射线") || line.Contains("命名异常") ||
                    line.Contains("内联实例信号") || line.Contains("不可交互"))
                    sb.AppendLine("  " + line.TrimEnd());
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------------
        // 总览
        // ------------------------------------------------------------------

        /// <summary>
        /// 数据 ↔ 节点 对应关系。
        ///
        /// `LocalClientDataService` 是无引擎依赖的纯 C# 服务，可以在编辑器里直接实例化，
        /// 因此这里能拿到"应该有几条"，与普查到的"实际有几条节点"对照，
        /// 直接暴露"场景里内联了 N 张卡但数据只有 M 条"这类静默故障。
        /// </summary>
        /// <summary>
        /// 全屏适配体检（只读）。
        ///
        /// 目的：判断"把某个弹窗根改成全屏拉伸 (0,0)-(1,1)"会不会移动它的内容。
        /// 依据是**直接子节点的锚点类型**：
        ///   中心固定锚 (0.5,0.5) → 改根后位置不变（安全）
        ///   其它固定锚（如底部对齐）→ 改变根的高度会**位移**，需要补偿
        ///   拉伸锚 → 尺寸会跟着变，需单独评估
        /// 同时打印 CanvasScaler 配置，确认参考宽度是否恒定（match width）。
        /// 本方法只读，不改任何东西。
        /// </summary>
        private static void AppendFullScreenReadiness(StringBuilder sb, Canvas canvas)
        {
            sb.AppendLine();
            sb.AppendLine("【五、全屏适配体检】");

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                sb.AppendLine("  CanvasScaler: " + scaler.uiScaleMode +
                              "  参考分辨率=" + scaler.referenceResolution +
                              "  matchWidthOrHeight=" + scaler.matchWidthOrHeight +
                              "（0=按宽匹配 → 参考宽度恒定、高度随屏幕变化）" +
                              "  screenMatchMode=" + scaler.screenMatchMode);
            }
            else
            {
                sb.AppendLine("  !! ClientCanvas 上没有 CanvasScaler");
            }

            Transform popupLayer = canvas.transform.Find("PopupLayer");
            if (popupLayer == null)
            {
                sb.AppendLine("  !! 未找到 PopupLayer");
                return;
            }

            sb.AppendLine("  逐弹窗（判断改为全屏拉伸是否安全）：");
            for (int i = 0; i < popupLayer.childCount; i++)
            {
                Transform page = popupLayer.GetChild(i);
                RectTransform rect = page.GetComponent<RectTransform>();
                if (rect == null)
                    continue;

                bool fullStretch = rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one;
                sb.AppendLine("    ── " + page.name +
                              "   根 " + rect.rect.size.ToString("F0") +
                              "  锚点 " + rect.anchorMin + "-" + rect.anchorMax +
                              (fullStretch ? "  [已是全屏拉伸]" : "  [固定尺寸]"));

                int centered = 0, edgeFixed = 0, stretched = 0;
                StringBuilder edgeList = new StringBuilder();
                for (int k = 0; k < page.childCount; k++)
                {
                    Transform child = page.GetChild(k);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    if (childRect == null)
                        continue;

                    if (childRect.anchorMin == childRect.anchorMax)
                    {
                        if (childRect.anchorMin == new Vector2(0.5f, 0.5f))
                            centered++;
                        else
                        {
                            edgeFixed++;
                            if (edgeFixed <= 6)
                                edgeList.Append("\n          · ").Append(child.name)
                                        .Append("  锚=").Append(childRect.anchorMin);
                        }
                    }
                    else
                    {
                        stretched++;
                    }
                }

                sb.AppendLine("        直接子节点：中心锚(安全)=" + centered +
                              "  边缘固定锚(会位移)=" + edgeFixed +
                              "  拉伸锚(尺寸会变)=" + stretched);
                if (edgeList.Length > 0)
                    sb.AppendLine("        会位移的节点：" + edgeList);
            }
        }

        /// <summary>
        /// Canvas 与布局适配体检（只读）。
        ///
        /// 由来：负责人问"PagesLayer / PopupLayer / MainPage 以及各一级页面到底有什么布局问题、
        /// 怎样才能适配分辨率、ClientCanvas 该怎么设"。
        ///
        /// 重点查两类事情：
        ///   ① 根画布与各层的缩放/矩形配置 —— 决定"参考坐标"是怎么来的；
        ///   ② **非根画布自带 CanvasScaler** 这一反模式 —— Unity 会警告
        ///      "Non-root Canvases will not be scaled"，即该子树**不跟随根画布缩放**，
        ///      于是同一份 Pos/Width 只在某一个分辨率成立，跨分辨率必然漂移。
        ///      实测 `ShopPage商店/Top function bar` 就带着 Canvas + CanvasScaler。
        /// 本方法只读。
        /// </summary>
        private static void AppendCanvasLayoutAudit(StringBuilder sb, Canvas canvas)
        {
            sb.AppendLine();
            sb.AppendLine("【六、Canvas 与布局适配体检】");

            // ---- 根画布 ----
            sb.AppendLine("  ● ClientCanvas");
            sb.AppendLine("      renderMode=" + canvas.renderMode +
                          "  pixelPerfect=" + canvas.pixelPerfect +
                          "  sortingOrder=" + canvas.sortingOrder);
            RectTransform rootRect = canvas.GetComponent<RectTransform>();
            if (rootRect != null)
                sb.AppendLine("      RectTransform: 锚=" + rootRect.anchorMin + "-" + rootRect.anchorMax +
                              "  sizeDelta=" + rootRect.sizeDelta + "  rect=" + rootRect.rect.size.ToString("F0"));
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                sb.AppendLine("      CanvasScaler: uiScaleMode=" + scaler.uiScaleMode +
                              "  referenceResolution=" + scaler.referenceResolution +
                              "  screenMatchMode=" + scaler.screenMatchMode +
                              "  matchWidthOrHeight=" + scaler.matchWidthOrHeight +
                              "  referencePixelsPerUnit=" + scaler.referencePixelsPerUnit +
                              "  scaleFactor=" + scaler.scaleFactor);
                sb.AppendLine("      → matchWidthOrHeight=0 表示按宽匹配（参考宽度恒定、高度随比例变）；" +
                              "=1 按高匹配；=0.5 宽高混合。");
            }
            else
            {
                sb.AppendLine("      !! 没有 CanvasScaler");
            }

            // ---- 各层矩形 ----
            AppendLayerRect(sb, canvas.transform, "PagesLayer");
            AppendLayerRect(sb, canvas.transform, "PopupLayer");
            AppendLayerRect(sb, canvas.transform, "SystemLayer");

            // ---- 各一级页面 ----
            sb.AppendLine("  ● 各一级页面（矩形 + 非根画布反模式）");
            AppendPageLayout(sb, canvas.transform, "PagesLayer");
            AppendPageLayout(sb, canvas.transform, "PopupLayer");

            // ---- 全场景：非根画布自带 CanvasScaler ----
            sb.AppendLine("  ● 全场景「非根画布自带 CanvasScaler」清单（反模式，逐个确认）：");
            CanvasScaler[] allScalers = Object.FindObjectsOfType<CanvasScaler>(true);
            int offenders = 0;
            for (int i = 0; i < allScalers.Length; i++)
            {
                CanvasScaler item = allScalers[i];
                if (item == null || item.gameObject == canvas.gameObject)
                    continue;
                if (!item.gameObject.scene.IsValid())
                    continue;

                Canvas owner = item.GetComponent<Canvas>();
                offenders++;
                if (offenders <= 40)
                    sb.AppendLine("      ! " + ClientUiTrace.Path(item.transform) +
                                  "   有Canvas=" + (owner != null) +
                                  "  其Scaler: " + item.uiScaleMode +
                                  " ref=" + item.referenceResolution +
                                  " match=" + item.matchWidthOrHeight);
            }
            if (offenders == 0)
                sb.AppendLine("      ✓ 无（根画布之外没有自带 CanvasScaler 的节点）");
            else
                sb.AppendLine("      合计 " + offenders + " 处");
        }

        private static void AppendLayerRect(StringBuilder sb, Transform canvas, string layerName)
        {
            Transform layer = canvas.Find(layerName);
            if (layer == null)
            {
                sb.AppendLine("  ● " + layerName + "：不存在");
                return;
            }
            RectTransform rect = layer.GetComponent<RectTransform>();
            if (rect == null)
            {
                sb.AppendLine("  ● " + layerName + "：无 RectTransform");
                return;
            }
            bool stretch = rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one &&
                           rect.offsetMin == Vector2.zero && rect.offsetMax == Vector2.zero;
            sb.AppendLine("  ● " + layerName + "  子节点=" + layer.childCount +
                          "  锚=" + rect.anchorMin + "-" + rect.anchorMax +
                          "  sizeDelta=" + rect.sizeDelta +
                          "  rect=" + rect.rect.size.ToString("F0") +
                          (stretch ? "   [已铺满画布 ✓]" : "   [非铺满 ✗]"));
        }

        private static void AppendPageLayout(StringBuilder sb, Transform canvas, string layerName)
        {
            Transform layer = canvas.Find(layerName);
            if (layer == null)
                return;

            for (int i = 0; i < layer.childCount; i++)
            {
                Transform page = layer.GetChild(i);
                RectTransform rect = page.GetComponent<RectTransform>();
                if (rect == null)
                    continue;

                bool stretch = rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one;
                sb.AppendLine("      · " + layerName + "/" + page.name +
                              "  rect=" + rect.rect.size.ToString("F0") +
                              "  锚=" + rect.anchorMin + "-" + rect.anchorMax +
                              (stretch ? "  [全屏拉伸]" : "  [固定尺寸]"));

                // 该页面子树里的"非根画布"数量
                Canvas[] nested = page.GetComponentsInChildren<Canvas>(true);
                int withScaler = 0;
                StringBuilder names = new StringBuilder();
                for (int k = 0; k < nested.Length; k++)
                {
                    Canvas item = nested[k];
                    if (item == null)
                        continue;
                    CanvasScaler itemScaler = item.GetComponent<CanvasScaler>();
                    if (itemScaler == null)
                        continue;
                    withScaler++;
                    if (withScaler <= 5)
                        names.Append("\n            !! ").Append(ClientUiTrace.Path(item.transform))
                             .Append("  match=").Append(itemScaler.matchWidthOrHeight)
                             .Append(" ref=").Append(itemScaler.referenceResolution);
                }
                if (withScaler > 0)
                {
                    sb.AppendLine("          非根画布自带 CanvasScaler：" + withScaler + " 处（不跟随根缩放）" + names);
                }
            }
        }

        /// <summary>
        /// 顶部区域元素锚点扫描（只读）。
        ///
        /// 由来：商店已发现**两处**"位于顶部区域、却用中心锚"的节点
        /// （`Shopping Mall Title`、`Regular store resource exchange - Crystal stones`）——
        /// 根改为全屏拉伸后，中心锚意味着"离画布中心的距离固定"，而 `match=0` 下画布高度
        /// 随屏幕比例变化（1080×1920→1339、1170×2532→1630、1080×2400→1673），
        /// 于是它们随高度漂移，只在最矮那档明显撞车。
        ///
        /// 本方法按同一模式扫全场景，**只报告不动手**，供负责人确认后统一修正。
        /// 判定：直接子节点中，其顶边落在页面顶部 30% 以内、且锚点为中心 (0.5,0.5) 的。
        /// </summary>
        private static void AppendTopAreaAnchorScan(StringBuilder sb, Canvas canvas)
        {
            sb.AppendLine();
            sb.AppendLine("【七、顶部区域元素锚点扫描（只读）】");
            sb.AppendLine("  判定：页面已全屏拉伸 + 名字像标题/栏 + 锚点仍为中心 (0.5,0.5)");
            sb.AppendLine("        （面板型页面未做根拉伸，中心锚不会漂，已排除）");
            sb.AppendLine("        → 根为全屏拉伸后会随画布高度漂移（商店的标题与水晶石即此类）。");

            Transform popupLayer = canvas.transform.Find("PopupLayer");
            if (popupLayer == null)
                return;

            int total = 0;
            for (int i = 0; i < popupLayer.childCount; i++)
            {
                Transform page = popupLayer.GetChild(i);
                RectTransform pageRect = page.GetComponent<RectTransform>();
                if (pageRect == null || pageRect.rect.height <= 1f)
                    continue;

                float pageTop = pageRect.rect.yMax;
                float threshold = pageRect.rect.height * 0.3f;
                StringBuilder hits = new StringBuilder();
                int count = 0;

                for (int k = 0; k < page.childCount; k++)
                {
                    Transform child = page.GetChild(k);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    if (childRect == null)
                        continue;

                    // 判定 = 名字像标题/栏 **且** 锚点仍为中心 (0.5,0.5)。
                    // ⚠️ 上一版把锚点检查误删、又硬编码打印 "锚=(0.5,0.5)"，
                    // 导致已改好的节点（贴顶/贴底）与面板型页面里的节点都混进结果（15 项假阳性）。
                    string n = child.name.ToLowerInvariant();
                    bool looksTitleOrBar = n.Contains("title") || n.Contains("bar") ||
                                           child.name.Contains("标题") || child.name.Contains("栏") ||
                                           child.name.Contains("顶");
                    if (!looksTitleOrBar)
                        continue;

                    bool centered = childRect.anchorMin == new Vector2(0.5f, 0.5f) &&
                                    childRect.anchorMax == new Vector2(0.5f, 0.5f);
                    if (!centered)
                        continue;

                    // 面板型页面没有做根拉伸，中心锚不会漂移，排除掉以免误导。
                    RectTransform ownerRect = page.GetComponent<RectTransform>();
                    bool pageStretched = ownerRect != null &&
                                         ownerRect.anchorMin == Vector2.zero &&
                                         ownerRect.anchorMax == Vector2.one;
                    if (!pageStretched)
                        continue;

                    count++;
                    total++;
                    if (count <= 8)
                        hits.Append("\n        ! ").Append(child.name)
                            .Append("  尺寸 ").Append(childRect.rect.size.ToString("F0"))
                            .Append("  锚点Y ").Append(childRect.anchoredPosition.y.ToString("F0"))
                            .Append("  锚=").Append(childRect.anchorMin).Append('-').Append(childRect.anchorMax)
                            .Append(" → 建议改贴顶");
                }

                if (count > 0)
                    sb.AppendLine("    ── " + page.name + "：命中 " + count + " 处" + hits);
            }

            if (total == 0)
                sb.AppendLine("    ✓ 未发现（无「顶部区域+中心锚」的节点）");
            else
                sb.AppendLine("    合计 " + total + " 处，建议逐个改为贴顶锚（只动纵向、横向保持不变）。");
        }

        /// <summary>
        /// `HeroPage英雄` 结构取证（只读）。
        ///
        /// 负责人报告：初次进入显示 4 张卡、返回后变 6 张，且只有一张能点进详情；
        /// `upCanvas` 初始位置不对；`Background` 垂直拉伸不够。
        /// 另需确认是否已存在"图鉴子界面"。
        ///
        /// 本方法把这些一次性量清楚：关键节点锚点/偏移、**所有含"卡牌"的节点**（含残留）、
        /// 卡牌容器子节点数与激活数、以及是否存在图鉴相关节点。
        /// </summary>
        private static void AppendHeroPageForensics(StringBuilder sb, Canvas canvas)
        {
            sb.AppendLine();
            sb.AppendLine("【八、HeroPage 结构取证（只读）】");

            Transform page = canvas.transform.Find("PopupLayer/HeroPage英雄");
            if (page == null)
            {
                sb.AppendLine("  未找到 PopupLayer/HeroPage英雄");
                return;
            }

            RectTransform pageRect = page.GetComponent<RectTransform>();
            sb.AppendLine("  根: rect=" + pageRect.rect.size.ToString("F0") +
                          "  锚=" + pageRect.anchorMin + "-" + pageRect.anchorMax +
                          "  offsetMin=" + pageRect.offsetMin + " offsetMax=" + pageRect.offsetMax);

            sb.AppendLine("  ── 直接子节点：");
            for (int i = 0; i < page.childCount; i++)
            {
                Transform child = page.GetChild(i);
                RectTransform r = child.GetComponent<RectTransform>();
                if (r == null)
                    continue;
                sb.AppendLine("      · " + child.name + "  rect=" + r.rect.size.ToString("F0") +
                              "  锚=" + r.anchorMin + "-" + r.anchorMax +
                              "  offsetMin=" + r.offsetMin + " offsetMax=" + r.offsetMax +
                              "  activeSelf=" + child.gameObject.activeSelf);
            }

            sb.AppendLine("  ── 关键节点（upCanvas / Background / downCanvas）：");
            RectTransform[] all = page.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                RectTransform r = all[i];
                if (r == null)
                    continue;
                string name = r.name;
                if (name != "upCanvas" && name != "Background" && name != "downCanvas" && name != "middleCanvas")
                    continue;
                sb.AppendLine("      · " + ClientUiTrace.Path(r.transform) +
                              "  rect=" + r.rect.size.ToString("F0") +
                              "  锚=" + r.anchorMin + "-" + r.anchorMax +
                              "  offsetMin=" + r.offsetMin + " offsetMax=" + r.offsetMax +
                              "  activeSelf=" + r.gameObject.activeSelf);
            }

            sb.AppendLine("  ── 全部含「卡牌」的节点（用于找残留）：");
            int cardTotal = 0;
            for (int i = 0; i < all.Length; i++)
            {
                RectTransform r = all[i];
                if (r == null || !r.name.Contains("卡牌"))
                    continue;
                cardTotal++;
                if (cardTotal <= 30)
                    sb.AppendLine("      [" + cardTotal + "] " + ClientUiTrace.Path(r.transform) +
                                  "  rect=" + r.rect.size.ToString("F0") +
                                  "  锚=" + r.anchorMin + "-" + r.anchorMax +
                                  "  activeSelf=" + r.gameObject.activeSelf);
            }
            sb.AppendLine("      合计 " + cardTotal + " 个");

            Transform container = page.Find("middleCanvas/MiddleCanvas/Canvas/Scroll View/Viewport/Content");
            if (container == null)
                container = FindDeepByName(page, "Content");
            if (container != null)
            {
                int activeChildren = 0;
                StringBuilder kids = new StringBuilder();
                for (int i = 0; i < container.childCount; i++)
                {
                    Transform c = container.GetChild(i);
                    if (c.gameObject.activeSelf)
                        activeChildren++;
                    if (i < 14)
                        kids.Append("\n          · ").Append(c.name)
                            .Append("  activeSelf=").Append(c.gameObject.activeSelf);
                }
                sb.AppendLine("  ── 卡牌容器 " + ClientUiTrace.Path(container) +
                              "  子节点=" + container.childCount + "  其中激活=" + activeChildren + kids);
            }

            sb.AppendLine("  ── 是否存在「图鉴」类节点（判断是否已有子界面）：");
            int guide = 0;
            for (int i = 0; i < all.Length; i++)
            {
                RectTransform r = all[i];
                if (r == null)
                    continue;
                string n = r.name.ToLowerInvariant();
                if (!r.name.Contains("图鉴") && !n.Contains("guide") && !n.Contains("illustration") &&
                    !n.Contains("handbook") && !n.Contains("图集"))
                    continue;
                guide++;
                if (guide <= 20)
                    sb.AppendLine("      [" + guide + "] " + ClientUiTrace.Path(r.transform) +
                                  "  activeSelf=" + r.gameObject.activeSelf);
            }
            if (guide == 0)
                sb.AppendLine("      ✓ 未发现（说明图鉴子界面尚未创建）");
            else
                sb.AppendLine("      合计 " + guide + " 个");

            // 进度文本的渲染参数：值正确却看不见时，用这些量定位原因。
            sb.AppendLine("  ── 进度文本渲染参数（值对但不显示时看这里）：");
            DumpTextNode(sb, page, "已收集进度");
            DumpTextNode(sb, page, "总英雄数量");
        }

        private static void DumpTextNode(StringBuilder sb, Transform page, string name)
        {
            Transform node = FindDeepByName(page, name);
            if (node == null)
            {
                sb.AppendLine("      " + name + "：未找到");
                return;
            }

            RectTransform r = node.GetComponent<RectTransform>();
            TMPro.TMP_Text tmp = node.GetComponent<TMPro.TMP_Text>();
            sb.AppendLine("      " + name + "  路径=" + ClientUiTrace.Path(node) +
                          "\n          activeSelf=" + node.gameObject.activeSelf +
                          "  activeInHierarchy=" + node.gameObject.activeInHierarchy +
                          (r != null ? "  rect=" + r.rect.size.ToString("F1") +
                                      "  锚=" + r.anchorMin + "-" + r.anchorMax +
                                      "  pos=" + r.anchoredPosition.ToString("F1") : "") +
                          (tmp != null ? "\n          文本='" + tmp.text + "'  字号=" + tmp.fontSize +
                                         "  颜色=" + tmp.color +
                                         "  raycast=" + tmp.raycastTarget +
                                         "  启用=" + tmp.enabled : "\n          (无 TMP 组件)"));

            // 同级顺序：越靠后越在上层。若同级有 Image 排在文本之后，会把文本盖住。
            Transform parent = node.parent;
            if (parent != null)
            {
                StringBuilder order = new StringBuilder();
                for (int i = 0; i < parent.childCount; i++)
                    order.Append("\n            [").Append(i).Append("] ").Append(parent.GetChild(i).name);
                sb.AppendLine("          父节点 " + parent.name + " 的子节点顺序（靠后=在上层）：" + order);
            }
        }

        private static Transform FindDeepByName(Transform root, string name)
        {
            if (root == null)
                return null;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == name)
                    return child;
                Transform found = FindDeepByName(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// 卡牌模板 `英雄卡Item` 的完整子树取证（只读）。
        ///
        /// 目的：为 `ClientHeroCard` 自动接线 6 个字段
        /// （立绘 / 名称 / 等级 / 锁 / 遮罩 / Button）拿到**真实的节点名与路径**，
        /// 避免靠猜接线。同时列出各节点上的组件类型，便于判断哪个是 Text、哪个是 Image。
        /// </summary>
        private static void AppendCardTemplateForensics(StringBuilder sb, Canvas canvas)
        {
            sb.AppendLine();
            sb.AppendLine("【九、卡牌模板子树取证（只读）】");

            Transform page = canvas.transform.Find("PopupLayer/HeroPage英雄");
            if (page == null)
            {
                sb.AppendLine("  未找到 HeroPage英雄");
                return;
            }

            Transform template = FindDeepByName(page, "英雄卡Item");
            if (template == null)
            {
                sb.AppendLine("  未找到 英雄卡Item 模板");
                return;
            }

            sb.AppendLine("  模板路径: " + ClientUiTrace.Path(template) +
                          "  activeSelf=" + template.gameObject.activeSelf);

            Transform[] all = template.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform node = all[i];
                if (node == null)
                    continue;

                string indent = "";
                Transform walk = node;
                int depth = 0;
                while (walk != template && walk.parent != null && depth < 12)
                {
                    indent += "  ";
                    walk = walk.parent;
                    depth++;
                }

                RectTransform r = node.GetComponent<RectTransform>();
                sb.AppendLine("    " + indent + "· " + node.name +
                              (node == template ? "（模板根）" : "") +
                              "  active=" + node.gameObject.activeSelf +
                              (r != null ? "  rect=" + r.rect.size.ToString("F0") : "") +
                              "  组件: " + DescribeComponents(node));
            }
        }

        private static string DescribeComponents(Transform node)
        {
            Component[] comps = node.GetComponents<Component>();
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < comps.Length; i++)
            {
                Component c = comps[i];
                if (c == null || c is RectTransform)
                    continue;
                text.Append(c.GetType().Name).Append(' ');
            }
            return text.Length > 0 ? text.ToString() : "(无)";
        }

        private static void AppendDataCorrespondence(StringBuilder sb, int shopCardNodes, int mailCardNodes)
        {
            sb.AppendLine("【四、数据 ↔ 节点 对应关系】");

            LocalClientDataService data;
            try
            {
                data = new LocalClientDataService();
            }
            catch (System.Exception exception)
            {
                sb.AppendLine("  !! LocalClientDataService 实例化失败，本节跳过：" + exception.Message);
                sb.AppendLine();
                return;
            }

            int shopListings = SafeCount(() => data.GetShopListings().Count);
            int mails = SafeCount(() => data.GetMails().Count);
            int heroes = SafeCount(() => data.GetHeroes().Count);
            int inventory = SafeCount(() => data.GetInventory().Count);
            int tasks = SafeCount(() => data.GetTasks().Count);
            int notices = SafeCount(() => data.GetNotices().Count);
            int activities = SafeCount(() => data.GetActivities().Count);
            int marbles = SafeCount(() => data.GetMarbles().Count);

            sb.AppendLine("  数据源（本地模拟）：");
            sb.AppendLine("    商城商品 GetShopListings=" + shopListings +
                          "   邮件 GetMails=" + mails +
                          "   英雄 GetHeroes=" + heroes);
            sb.AppendLine("    背包 GetInventory=" + inventory +
                          "   任务 GetTasks=" + tasks +
                          "   公告 GetNotices=" + notices);
            sb.AppendLine("    活动 GetActivities=" + activities +
                          "   弹珠 GetMarbles=" + marbles);

            Dictionary<string, int> leaderboards = new Dictionary<string, int>();
            foreach (ClientLeaderboardKind kind in System.Enum.GetValues(typeof(ClientLeaderboardKind)))
                leaderboards[kind.ToString()] = SafeCount(() => data.GetLeaderboardEntries(kind).Count);
            Dictionary<string, int> activityTasks = new Dictionary<string, int>();
            foreach (ClientActivityTaskCategory category in System.Enum.GetValues(typeof(ClientActivityTaskCategory)))
                activityTasks[category.ToString()] = SafeCount(() => data.GetActivityTasks(category).Count);

            StringBuilder leaderboardText = new StringBuilder();
            foreach (KeyValuePair<string, int> pair in leaderboards)
                leaderboardText.Append("   排行榜[").Append(pair.Key).Append("]=").Append(pair.Value);
            sb.AppendLine(leaderboardText.ToString().Length > 0 ? "  " + leaderboardText : "");

            StringBuilder taskText = new StringBuilder();
            foreach (KeyValuePair<string, int> pair in activityTasks)
                taskText.Append("   活动任务[").Append(pair.Key).Append("]=").Append(pair.Value);
            sb.AppendLine(taskText.ToString().Length > 0 ? "  " + taskText : "");

            // 已核实的对照（列表项前缀 → 数据源）。其余前缀待逐页确认后补入。
            sb.AppendLine();
            sb.AppendLine("  对照（列表项前缀 → 数据源；✘ 表示场景节点数与数据条数不一致）：");
            AppendPair(sb, "道具卡牌prefab（商城）", shopCardNodes, shopListings);
            AppendPair(sb, "邮件prefab", mailCardNodes, mails);
            sb.AppendLine("    其余前缀（角色卡牌Button-final / 任务卡prefabs / 玩家*排行prefabs / 星级图标Image）" +
                          "的数据源归属待逐页确认；`星级图标Image` 属固定 5 星展示，不是列表。");
            sb.AppendLine();
        }

        /// <summary>按 "A/B/C" 逐级精确名查找（Transform.Find 只匹配直接子节点）。</summary>
        private static Transform FindExactPath(Transform root, string path)
        {
            if (root == null || string.IsNullOrEmpty(path))
                return null;
            string[] parts = path.Split('/');
            Transform current = root;
            for (int i = 0; i < parts.Length && current != null; i++)
                current = current.Find(parts[i]);
            return current;
        }

        private static int CountChildrenStartingWith(Transform root, string prefix)
        {
            if (root == null)
                return 0;
            int count = 0;
            for (int i = 0; i < root.childCount; i++)
                if (root.GetChild(i).name.StartsWith(prefix, System.StringComparison.Ordinal))
                    count++;
            return count;
        }

        private static void AppendPair(StringBuilder sb, string label, int nodeCount, int dataCount)
        {
            // 模板模式下场景只保留 1 个禁用模板，条目由运行时按数据生成——
            // 此时"节点数 != 数据数"是**预期**，不是缺陷。
            if (nodeCount <= 1)
            {
                sb.AppendLine("    ✔ " + label + "：模板模式（场景保留 " + nodeCount +
                              " 个模板，运行时按数据生成 " + dataCount + " 个）");
                return;
            }

            string mark = nodeCount == dataCount ? "✔" : "✘";
            sb.AppendLine("    " + mark + " " + label + "：场景节点 " + nodeCount +
                          "  vs  数据 " + dataCount +
                          (nodeCount == dataCount ? "" : "   → 差 " + System.Math.Abs(nodeCount - dataCount) + " 项"));
        }

        private static int SafeCount(System.Func<int> getter)
        {
            try { return getter(); }
            catch { return -1; }
        }

        private static void AppendOverview(StringBuilder sb, Canvas canvas, List<GameObject> objects)
        {
            sb.AppendLine("【一、总览】");
            int buttons = 0, toggles = 0, scrolls = 0, custom = 0, graphics = 0, raycastOn = 0;
            for (int i = 0; i < objects.Count; i++)
            {
                GameObject go = objects[i];
                Button[] b = go.GetComponents<Button>();
                Toggle[] t = go.GetComponents<Toggle>();
                ScrollRect[] s = go.GetComponents<ScrollRect>();
                buttons += b.Length;
                toggles += t.Length;
                scrolls += s.Length;

                Graphic[] g = go.GetComponents<Graphic>();
                graphics += g.Length;
                for (int k = 0; k < g.Length; k++)
                    if (g[k].raycastTarget)
                        raycastOn++;

                if (HasCustomPointerHandler(go))
                    custom++;
            }

            sb.AppendLine("  三层结构：");
            for (int i = 0; i < LayerNames.Length; i++)
            {
                Transform layer = canvas.transform.Find(LayerNames[i]);
                if (layer == null)
                {
                    sb.AppendLine("    " + LayerNames[i] + " : 缺失");
                    continue;
                }
                int childCount = 0;
                for (int k = 0; k < layer.childCount; k++)
                    if (layer.GetChild(k).gameObject.activeSelf || true)
                        childCount++;
                sb.AppendLine("    " + LayerNames[i] + " : " + childCount + " 个直接子节点");
            }
            sb.AppendLine("  交互组件：Button=" + buttons + "  Toggle=" + toggles +
                          "  ScrollRect=" + scrolls + "  自定义指针处理器宿主=" + custom);
            sb.AppendLine("  Graphic 总数=" + graphics + "  其中 raycastTarget=true 的有 " + raycastOn);
            sb.AppendLine();
        }

        // ------------------------------------------------------------------
        // 全局问题
        // ------------------------------------------------------------------

        private static void AppendGlobalIssues(StringBuilder sb, Canvas canvas, List<GameObject> objects, Scene scene)
        {
            sb.AppendLine("【二、全局问题】");

            List<string> ancestorHit = new List<string>();
            List<string> orphanRaycast = new List<string>();
            List<string> typos = new List<string>();
            List<string> trailing = new List<string>();
            List<string> dupSiblings = new List<string>();

            for (int i = 0; i < objects.Count; i++)
            {
                GameObject go = objects[i];
                if (IsInteractive(go))
                {
                    string origin = DescribeHitOrigin(go);
                    if (origin.StartsWith("祖先") || origin == "无")
                        ancestorHit.Add(origin + "  <- " + Path(go.transform));
                }

                if (go.name != go.name.Trim())
                    trailing.Add(Path(go.transform));

                string lower = go.name.ToLowerInvariant();
                if (lower.Contains("perfab"))
                    typos.Add(Path(go.transform) + "   (应为 prefab)");

                Transform parent = go.transform.parent;
                if (parent != null)
                {
                    int same = 0;
                    for (int k = 0; k < parent.childCount; k++)
                        if (parent.GetChild(k).name == go.name)
                            same++;
                    if (same > 1 && parent.GetChild(0) == go.transform)
                        dupSiblings.Add(parent.name + " 下有 " + same + " 个同名子节点「" + go.name + "」");
                }
            }

            for (int i = 0; i < objects.Count; i++)
            {
                Graphic[] gs = objects[i].GetComponents<Graphic>();
                for (int k = 0; k < gs.Length; k++)
                {
                    if (gs[k] == null || !gs[k].raycastTarget)
                        continue;
                    if (HasInteractiveSelfOrAncestor(gs[k].transform))
                        continue;
                    orphanRaycast.Add(Path(gs[k].transform));
                }
            }

            sb.AppendLine("  A. 交互节点命中面来自祖先或缺失（结构不良）: " + ancestorHit.Count + " 个");
            AppendList(sb, ancestorHit, 30);

            sb.AppendLine("  B. 装饰 Graphic 仍参与射线（本应关闭）: " + orphanRaycast.Count + " 个");
            AppendList(sb, orphanRaycast, 30);

            sb.AppendLine("  C. 命名疑似拼写错误: " + typos.Count + " 个");
            AppendList(sb, typos, 15);

            sb.AppendLine("  D. 名字含首尾空格: " + trailing.Count + " 个");
            AppendList(sb, trailing, 15);

            sb.AppendLine("  E. 同级同名节点: " + dupSiblings.Count + " 处");
            AppendList(sb, dupSiblings, 15);
            sb.AppendLine();
        }

        // ------------------------------------------------------------------
        // 逐页报告
        // ------------------------------------------------------------------

        private static void AppendPerPage(StringBuilder sb, Canvas canvas, Scene scene)
        {
            sb.AppendLine("【三、逐页报告】");

            List<Transform> pages = new List<Transform>();
            for (int i = 0; i < LayerNames.Length; i++)
            {
                Transform layer = canvas.transform.Find(LayerNames[i]);
                if (layer == null)
                    continue;
                for (int k = 0; k < layer.childCount; k++)
                {
                    Transform child = layer.GetChild(k);
                    if (child.name == "弹窗遮罩")
                        continue;
                    pages.Add(child);
                }
            }

            for (int i = 0; i < pages.Count; i++)
                AppendOnePage(sb, pages[i]);
        }

        /// <summary>
        /// 已收敛/在途页面的验收要点（数据驱动，随普查报告输出）。
        /// 目的是让负责人的 Play 验收有**客观对照口径**，而不是凭印象。
        /// </summary>
        private static readonly Dictionary<string, string> AcceptanceNotes = new Dictionary<string, string>
        {
            { "ShopPage商店",
              "模板模式：容器内 道具卡牌Item 为禁用模板；运行时按 GetShopListings()=3 生成 3 张卡；" +
              "点任意一张应打开 Product Purchase Interface。" },
            { "MailPage邮件",
              "模板模式：容器内 邮件Item 为禁用模板；运行时按 GetMails()=2 生成 2 张卡；" +
              "点卡应选中邮件（该点击绑定是本次补上的）。" },
            { "HeroPage英雄",
              "模板模式：容器内 英雄卡Item 为禁用模板；运行时按 GetHeroes()=2 生成 2 张卡" +
              "（旧实现用 heroes[index % count] 取模填充，8 张卡重复显示同样 2 个英雄，此问题已随模板化消失）。" },
        };

        private static void AppendOnePage(StringBuilder sb, Transform page)
        {
            sb.AppendLine("  ─────────────────────────────────────────");
            sb.AppendLine("  【" + page.name + "】");

            RectTransform rect = page.GetComponent<RectTransform>();
            string fullScreen = "(无 RectTransform)";
            if (rect != null)
            {
                bool stretched = rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one;
                fullScreen = stretched ? "全屏拉伸 ✔" : "非全屏 ✘ (锚点 " + rect.anchorMin + " - " + rect.anchorMax + ")";
            }

            int total = 0, maxDepth = 0;
            CollectScale(page, 1, ref total, ref maxDepth);
            sb.AppendLine("    规模: 节点=" + total + "  最大深度=" + maxDepth + "  根尺寸: " + fullScreen);

            // 组件
            StringBuilder comps = new StringBuilder();
            MonoBehaviour[] behaviours = page.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null)
                    continue;
                if (comps.Length > 0)
                    comps.Append(", ");
                comps.Append(behaviours[i].GetType().Name);
            }
            sb.AppendLine("    根组件: " + (comps.Length > 0 ? comps.ToString() : "(无)"));

            string note;
            if (AcceptanceNotes.TryGetValue(page.name, out note))
                sb.AppendLine("    ★ 验收要点: " + note);

            // 交互节点与命中面
            List<string> bad = new List<string>();
            List<string> goods = new List<string>();
            List<string> customs = new List<string>();
            int interactiveCount = 0;

            MonoBehaviour[] all = page.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < all.Length; i++)
            {
                MonoBehaviour behaviour = all[i];
                if (behaviour == null || !IsInteractiveComponent(behaviour))
                    continue;
                interactiveCount++;
                GameObject go = behaviour.gameObject;
                string origin = DescribeHitOrigin(go);
                string entry = behaviour.GetType().Name + " @ " + RelativePath(page, go.transform) + "  ← 命中面: " + origin;
                if (origin.StartsWith("祖先") || origin == "无")
                    bad.Add(entry);
                else
                    goods.Add(entry);
            }

            sb.AppendLine("    交互节点: " + interactiveCount +
                          "（命中面合格 " + goods.Count + " / 结构不良 " + bad.Count + "）");
            if (bad.Count > 0)
            {
                sb.AppendLine("      ✘ 结构不良（命中面在祖先或缺失，一旦祖先的射线被关掉就点不动）:");
                AppendList(sb, bad, 20, "        ");
            }

            // 列表容器（内联实例信号）
            List<string> lists = FindInlineListContainers(page);
            if (lists.Count > 0)
            {
                sb.AppendLine("      ⚠ 疑似「场景内联展开的列表」: " + lists.Count + " 处");
                AppendList(sb, lists, 12, "        ");
            }

            // 装饰射线
            List<string> deco = new List<string>();
            Graphic[] graphics = page.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] == null || !graphics[i].raycastTarget)
                    continue;
                if (HasInteractiveSelfOrAncestor(graphics[i].transform))
                    continue;
                deco.Add(RelativePath(page, graphics[i].transform));
            }
            if (deco.Count > 0)
            {
                sb.AppendLine("      ⚠ 装饰 Graphic 仍参与射线: " + deco.Count + " 个");
                AppendList(sb, deco, 10, "        ");
            }

            // 命名异常（跨页关键词）
            string[] keywords;
            if (PageKeywords.TryGetValue(page.name, out keywords))
            {
                List<string> clash = new List<string>();
                Transform[] allT = page.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < allT.Length; i++)
                {
                    for (int k = 0; k < keywords.Length; k++)
                    {
                        if (allT[i].name.Contains(keywords[k]))
                        {
                            clash.Add(RelativePath(page, allT[i]) + "   (含「" + keywords[k] + "」，疑似复制残留)");
                            break;
                        }
                    }
                }
                if (clash.Count > 0)
                {
                    sb.AppendLine("      ⚠ 命名疑似复制残留: " + clash.Count + " 个");
                    AppendList(sb, clash, 12, "        ");
                }
            }
        }

        /// <summary>找出"子节点成批同名"的容器 —— 这是列表在场景里被内联展开的信号。</summary>
        private static List<string> FindInlineListContainers(Transform page)
        {
            List<string> result = new List<string>();
            Transform[] all = page.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform node = all[i];
                if (node.childCount < 3)
                    continue;

                Dictionary<string, int> byBase = new Dictionary<string, int>();
                for (int k = 0; k < node.childCount; k++)
                {
                    string baseName = StripIndexSuffix(node.GetChild(k).name);
                    int count;
                    byBase.TryGetValue(baseName, out count);
                    byBase[baseName] = count + 1;
                }

                foreach (KeyValuePair<string, int> pair in byBase)
                {
                    if (pair.Value >= 3)
                    {
                        // 能否安全模板化，取决于容器是否自带布局组件——否则克隆体会叠在同一坐标。
                        // 这里提前标出，避免执行时才被 ConvergeList 拒绝。
                        string layout = DescribeLayout(node);
                        result.Add(RelativePath(page, node) + "  子节点=" + node.childCount +
                                   "  同名前缀「" + pair.Key + "」×" + pair.Value +
                                   "  布局=" + layout +
                                   (layout.StartsWith("无") ? "  ✘ 不可模板化（会叠在一起）"
                                                            : "  ✔ 可安全模板化"));
                    }
                }
            }
            return result;
        }

        /// <summary>描述容器自带的布局组件；"无"表示克隆体会叠在一起、不可模板化。</summary>
        private static string DescribeLayout(Transform node)
        {
            GridLayoutGroup grid = node.GetComponent<GridLayoutGroup>();
            if (grid != null)
                return "GridLayoutGroup cell=" + grid.cellSize.x + "x" + grid.cellSize.y +
                       " 固定列数=" + grid.constraintCount;
            if (node.GetComponent<VerticalLayoutGroup>() != null)
                return "VerticalLayoutGroup";
            if (node.GetComponent<HorizontalLayoutGroup>() != null)
                return "HorizontalLayoutGroup";
            if (node.GetComponent<ScrollRect>() == null && node.parent != null)
                return "无（仅 ContentSizeFitter 不足以定位子节点）";
            return "无";
        }

        private static string StripIndexSuffix(string name)
        {
            int paren = name.LastIndexOf(" (", System.StringComparison.Ordinal);
            if (paren > 0 && name.EndsWith(")", System.StringComparison.Ordinal))
                return name.Substring(0, paren);
            return name;
        }

        // ------------------------------------------------------------------
        // 命中面判定
        // ------------------------------------------------------------------

        /// <summary>描述某交互节点的"命中面"来自哪里。</summary>
        private static string DescribeHitOrigin(GameObject interactive)
        {
            Graphic own = interactive.GetComponent<Graphic>();
            if (own != null && own.raycastTarget)
                return "自身";

            Graphic[] children = interactive.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < children.Length; i++)
                if (children[i] != null && children[i].raycastTarget)
                    return "子节点 " + RelativePath(interactive.transform, children[i].transform);

            Transform parent = interactive.transform.parent;
            int guard = 0;
            while (parent != null && guard < 16)
            {
                Graphic g = parent.GetComponent<Graphic>();
                if (g != null && g.raycastTarget)
                    return "祖先 " + RelativePath(interactive.transform, parent);
                if (parent.GetComponent<Canvas>() != null)
                    break;
                parent = parent.parent;
                guard++;
            }
            return "无";
        }

        private static bool HasInteractiveSelfOrAncestor(Transform node)
        {
            Transform current = node;
            int guard = 0;
            while (current != null && guard < 32)
            {
                if (IsInteractive(current.gameObject))
                    return true;
                if (current != node && current.GetComponent<Canvas>() != null)
                    return false;
                current = current.parent;
                guard++;
            }
            return false;
        }

        private static bool IsInteractive(GameObject go)
        {
            if (go == null)
                return false;
            if (go.GetComponent<Selectable>() != null || go.GetComponent<ScrollRect>() != null ||
                go.GetComponent<EventTrigger>() != null)
                return true;
            return HasCustomPointerHandler(go);
        }

        private static bool IsInteractiveComponent(MonoBehaviour behaviour)
        {
            if (behaviour is Selectable || behaviour is ScrollRect || behaviour is EventTrigger)
                return true;
            return IsPointerHandler(behaviour);
        }

        private static bool HasCustomPointerHandler(GameObject go)
        {
            MonoBehaviour[] behaviours = go.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
                if (behaviours[i] != null && IsPointerHandler(behaviours[i]))
                    return true;
            return false;
        }

        private static bool IsPointerHandler(MonoBehaviour behaviour)
        {
            return behaviour is IPointerClickHandler || behaviour is IPointerDownHandler ||
                   behaviour is IPointerUpHandler || behaviour is IPointerEnterHandler ||
                   behaviour is IPointerExitHandler || behaviour is IDragHandler ||
                   behaviour is IBeginDragHandler || behaviour is IEndDragHandler ||
                   behaviour is IDropHandler || behaviour is IScrollHandler;
        }

        // ------------------------------------------------------------------
        // 工具方法
        // ------------------------------------------------------------------

        private static void CollectScale(Transform node, int depth, ref int total, ref int maxDepth)
        {
            total++;
            if (depth > maxDepth)
                maxDepth = depth;
            for (int i = 0; i < node.childCount; i++)
                CollectScale(node.GetChild(i), depth + 1, ref total, ref maxDepth);
        }

        private static string RelativePath(Transform root, Transform target)
        {
            if (target == null)
                return "(null)";
            if (root != null && target == root)
                return root.name;

            List<string> parts = new List<string>();
            Transform current = target;
            int guard = 0;
            while (current != null && current != root && guard < 32)
            {
                parts.Insert(0, current.name);
                current = current.parent;
                guard++;
            }
            return string.Join("/", parts.ToArray());
        }

        private static string Path(Transform node)
        {
            if (node == null)
                return "(null)";
            List<string> parts = new List<string>();
            Transform current = node;
            int guard = 0;
            while (current != null && guard < 40)
            {
                parts.Insert(0, current.name);
                current = current.parent;
                guard++;
            }
            return string.Join("/", parts.ToArray());
        }

        private static void AppendList(StringBuilder sb, List<string> items, int limit)
        {
            AppendList(sb, items, limit, "      ");
        }

        private static void AppendList(StringBuilder sb, List<string> items, int limit, string indent)
        {
            int count = Mathf.Min(limit, items.Count);
            for (int i = 0; i < count; i++)
                sb.AppendLine(indent + "- " + items[i]);
            if (items.Count > limit)
                sb.AppendLine(indent + "... 另有 " + (items.Count - limit) + " 条，详见报告文件");
        }

        private static Canvas FindClientCanvas(List<GameObject> objects)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                Canvas candidate = objects[i].GetComponent<Canvas>();
                if (candidate != null && objects[i].name == "ClientCanvas")
                    return candidate;
            }
            for (int i = 0; i < objects.Count; i++)
            {
                Canvas candidate = objects[i].GetComponent<Canvas>();
                if (candidate != null)
                    return candidate;
            }
            return null;
        }
    }
}
