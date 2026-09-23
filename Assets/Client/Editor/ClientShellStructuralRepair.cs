using System.Collections.Generic;
using System.Text;
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
    /// ClientShell 场景结构修复（安全 / 幂等 / 只动层级与组件）。
    ///
    /// 为什么不复用 ClientShellDirectUpdater：
    /// 1. 它按**精确节点名**查找页面（如 Transform.Find("MainPage")），而场景中的页面已被
    ///    重命名为 "MainPage主页" 等，于是 AddUiPage 全部静默失败，随后
    ///    navigator.Configure(空列表) 会**清空**已序列化的 _pages，一次运行即导致导航全断。
    /// 2. 其中若干 pass（ApplyGachaResultPopupPass / ApplySystemFeedbackPass /
    ///    ApplyBottomNavigationSelectionPass / ApplyGachaPage 等）会改写**已存在节点**的
    ///    anchor/sizeDelta/anchoredPosition，违反"不得改动 UI 尺寸与排版"的约束。
    ///
    /// 本文件的新增操作遵守三条硬规则：
    /// - 只按**组件类型**定位，不按节点名推断页面身份（名字可能因复制界面而忘记修改）；
    /// - 迁移节点一律用 SetParent(parent, worldPositionStays: true)，并在迁移前校验
    ///   该节点锚点非拉伸（anchorMin == anchorMax），否则放弃并报错；
    /// - 任何情况下都不写 anchorMin/anchorMax/pivot/anchoredPosition/sizeDelta。
    ///
    /// 命令行用法：
    ///   Unity.exe -batchmode -quit -projectPath "D:\unity project\pinball" \
    ///             -executeMethod Pinball.Client.Editor.ClientShellStructuralRepair.RepairAll \
    ///             -logFile Logs/client-structure-repair.log
    /// 只盘点不写入：
    ///   -executeMethod Pinball.Client.Editor.ClientShellStructuralRepair.ReportOnly
    /// </summary>
    public static class ClientShellStructuralRepair
    {
        private const string ScenePath = "Assets/Client/Scenes/ClientShell.unity";

        // 已由负责人确认的节点名（仅在"删除旧实现/迁移弹窗"这类已确认事项上使用）。
        private const string ObsoleteHeroDetailNode = "BackpackPage英雄详情装备页";
        private const string CurrentHeroDetailNode = "HeroDetailPage英雄详情主页";
        private const string ContactServiceNode = "联系客服Page";

        // ------------------------------------------------------------------
        // 命令入口
        // ------------------------------------------------------------------

        /// <summary>只读盘点：输出结构报告，不修改场景。</summary>
        [MenuItem("Client/结构修复/1. 只读盘点报告", false, 10)]
        public static void ReportOnly()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log(BuildReport(scene));
            Finish(0);
        }

        /// <summary>执行全部已确认的结构修复。幂等，可重复运行。</summary>
        [MenuItem("Client/结构修复/2. 执行结构修复", false, 11)]
        public static void RepairAll()
        {
            WarnIfEditorCodeIsStale();

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = FindClientCanvas();
            if (canvas == null)
            {
                Fail("未找到 ClientCanvas，未修改场景。");
                return;
            }

            int changes = 0;
            changes += RemoveMismatchedComponents();
            changes += RemoveObsoleteHeroDetailDuplicate();
            changes += MigrateToLayers(canvas);
            changes += RewireAfterMigration();
            changes += EnsureTogglesAndBackButtons();
            changes += EnsureKernelComponents(canvas);

            // ---- UI 适配 第 1+2 批（负责人 2026-09-21 的 11 项反馈）----
            // 第 1 批：英雄详情 3 个技能页签的选中态皮肤（贴图引用 + 显隐驱动）
            changes += FixHeroDetailSkillTabs();
            // 第 2 批：英雄详情倒退/卡片锚点 + 英雄页卡牌列表几何
            changes += FixHeroDetailBackButtonAnchor();
            changes += FixHeroDetailLeftMiddleCanvas();
            changes += FixHeroPageCardListGeometry();
            // 第 4 批：排行榜两份列表（Viewport 塌成 0×0 + Content 中锚推出去）
            changes += FixRankPageLists();
            // 第 4 批续：排行榜随画布高度漂移的 6 个中锚元素 → 统一贴顶锚
            changes += FixRankPageStableAnchors();

            if (changes > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
            }

            Debug.Log("[结构修复] 完成，共 " + changes + " 处变更。\n" + BuildReport(scene));
            Finish(0);
        }

        /// <summary>
        /// **刷新前自检**：如果脚本比最近一次编译还新，说明编辑器内存里还没有最新代码 ——
        /// 此时跑结构修复会出现"组件显示为 Missing (Mono Script)"甚至把刚写进场景的引用覆盖掉。
        ///
        /// <para>由来（2026-09-21 实测）：场景里的 `ClientHeroDetailToggleSkin` 是**直接写盘**加的，
        /// 而当时编辑器的 `Library/ScriptAssemblies/Assembly-CSharp.dll` 停在 17:43、`.csproj` 里也没有该文件 ⇒
        /// 一旦编辑器保存场景，就会把该组件序列化成"缺失脚本"并抹掉它的 3 组绑定。</para>
        ///
        /// <para>本方法**只警告、不阻断**（刷新是编辑器的行为，工具无权也不该强行触发）。</para>
        /// </summary>
        private static void WarnIfEditorCodeIsStale()
        {
            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectRoot))
                return;

            string assemblyPath = System.IO.Path.Combine(projectRoot, "Library/ScriptAssemblies/Assembly-CSharp.dll");
            if (!System.IO.File.Exists(assemblyPath))
                return;

            System.DateTime assemblyTime = System.IO.File.GetLastWriteTime(assemblyPath);
            string newestScript = null;
            System.DateTime newestTime = assemblyTime;

            string[] roots = { "Assets/Client", "Assets/Main" };
            for (int i = 0; i < roots.Length; i++)
            {
                string root = System.IO.Path.Combine(projectRoot, roots[i]);
                if (!System.IO.Directory.Exists(root))
                    continue;

                string[] files = System.IO.Directory.GetFiles(root, "*.cs", System.IO.SearchOption.AllDirectories);
                for (int k = 0; k < files.Length; k++)
                {
                    System.DateTime t = System.IO.File.GetLastWriteTime(files[k]);
                    if (t > newestTime)
                    {
                        newestTime = t;
                        newestScript = files[k].Substring(projectRoot.Length + 1).Replace('\\', '/');
                    }
                }
            }

            if (newestScript == null)
                return;

            Debug.LogError("[结构修复] ⚠ 编辑器内存里的代码是**旧的**：Assembly-CSharp.dll 编译于 " +
                           assemblyTime.ToString("HH:mm:ss") + "，但 " + newestScript +
                           " 修改于 " + newestTime.ToString("HH:mm:ss") + "。\n" +
                           "  ⇒ 现在跑修复会出现组件显示为 Missing (Mono Script)，甚至覆盖刚写进场景的引用。\n" +
                           "  ⇒ 请先让 Unity 刷新并重新编译（点一下 Unity 窗口 / Ctrl+R / Assets → Refresh），" +
                           "等控制台出现编译完成后**再**运行本工具或保存场景。");
        }

        /// <summary>
        /// **只跑 UI 适配 第 1+2 批**（英雄详情技能页签皮肤 / 英雄详情锚点 / 英雄页卡牌几何）。
        ///
        /// 单独一个入口的理由：这两批是负责人 2026-09-21 的复验反馈，
        /// 与他手动微调过的其它页面解耦 —— 只想应用这两批时用这个菜单，不必重跑全部结构修复。
        /// 幂等，可重复运行。
        /// </summary>
        [MenuItem("Client/结构修复/7. UI 适配 第 1+2 批（英雄详情页签/锚点 + 英雄页卡牌）", false, 16)]
        public static void RepairAdaptBatch12()
        {
            WarnIfEditorCodeIsStale();

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (FindClientCanvas() == null)
            {
                Fail("未找到 ClientCanvas，未修改场景。");
                return;
            }

            int changes = 0;
            changes += FixHeroDetailSkillTabs();
            changes += FixHeroDetailBackButtonAnchor();
            changes += FixHeroDetailLeftMiddleCanvas();
            changes += FixHeroPageCardListGeometry();
            changes += FixRankPageLists();

            if (changes > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
            }

            Debug.Log("[结构修复] UI 适配 第 1+2 批完成，共 " + changes + " 处变更。");
            Finish(0);
        }

        /// <summary>
        /// **只跑排行榜（第 4 批 #6）**：两份列表的 Viewport / Content / 网格对齐规范化。
        /// 单独入口的理由同菜单 7 —— 与负责人已定稿的其它页面解耦。幂等，可重复运行。
        /// </summary>
        [MenuItem("Client/结构修复/8. 排行榜列表修复（Viewport 塌陷 + Content 错位）", false, 17)]
        public static void RepairRankPageLists()
        {
            WarnIfEditorCodeIsStale();

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (FindClientCanvas() == null)
            {
                Fail("未找到 ClientCanvas，未修改场景。");
                return;
            }

            int changes = FixRankPageLists();
            changes += FixRankPageStableAnchors();
            if (changes > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
            }

            Debug.Log("[结构修复] 排行榜列表修复完成，共 " + changes + " 处变更。");
            Finish(0);
        }

        // ------------------------------------------------------------------
        // 1. 移除已停用的旧导航器组件
        // ------------------------------------------------------------------

        /// <summary>
        /// ClientPageNavigator 在场景中已是 m_Enabled=0，且其导航职责已由
        /// ClientUiNavigator 完全接管。移除组件即可，不影响任何 RectTransform。
        /// </summary>
        /// <summary>
        /// 移除迁移后仍然挂错的页面 View 组件。
        ///
        /// 目标：联系客服Page。它是主页 BottomFunctionIcon 弹出的弹窗，但节点是从
        /// ProfilePage个人中心 复制来的，因此残留了一个 ClientProfilePage。留着它会带来两个问题：
        /// 1. 页面上同时存在两套行为（弹窗锚点 + 个人中心绑定逻辑）；
        /// 2. ClientShellController / ClientUiNavigator 的注入会把它当成个人中心视图。
        ///
        /// 旧导航器组件（ClientPageNavigator）已在首次执行时移除，且其类型已删除，
        /// 因此这里不再需要对应的清理步骤。
        /// </summary>
        private static int RemoveMismatchedComponents()
        {
            GameObject contact = FindSceneObject(ContactServiceNode, true);
            if (contact == null)
            {
                Debug.Log("[结构修复] 未找到 " + ContactServiceNode + "，跳过组件清理。");
                return 0;
            }

            int changes = 0;
            ClientProfilePage wrongProfile = contact.GetComponent<ClientProfilePage>();
            if (wrongProfile != null)
            {
                Debug.Log("[结构修复] 移除 " + ContactServiceNode +
                          " 上误挂的 ClientProfilePage（复制界面残留，该组件属于 ProfilePage个人中心）。");
                Object.DestroyImmediate(wrongProfile, true);
                changes++;
            }
            if (changes == 0)
                Debug.Log("[结构修复] " + ContactServiceNode + " 无需要清理的组件。");
            return changes;
        }

        // ------------------------------------------------------------------
        // 2. 删除旧的英雄详情实现（负责人确认为被新实现取代的旧界面）
        // ------------------------------------------------------------------

        private static int RemoveObsoleteHeroDetailDuplicate()
        {
            GameObject obsolete = FindSceneObject(ObsoleteHeroDetailNode, true);
            if (obsolete == null)
            {
                Debug.Log("[结构修复] 未找到旧英雄详情节点 " + ObsoleteHeroDetailNode + "，跳过。");
                return 0;
            }

            // 安全前提：新的英雄详情页必须存在且挂有 ClientHeroDetailPage，
            // 否则删除会造成功能缺口。
            GameObject current = FindSceneObject(CurrentHeroDetailNode, true);
            ClientHeroDetailPage currentPage = current != null ? current.GetComponent<ClientHeroDetailPage>() : null;
            if (current == null || currentPage == null)
            {
                Fail("前提不满足：新英雄详情页 " + CurrentHeroDetailNode + " 不存在或未挂 ClientHeroDetailPage，" +
                     "拒绝删除旧节点 " + ObsoleteHeroDetailNode + "。");
                return 0;
            }

            int childCount = CountDescendants(obsolete.transform);
            ClientUiPage[] pages = obsolete.GetComponents<ClientUiPage>();
            ClientHeroDetailPage[] duplicates = obsolete.GetComponents<ClientHeroDetailPage>();

            Debug.Log("[结构修复] 删除旧英雄详情实现节点 " + ObsoleteHeroDetailNode +
                      "：子树节点数=" + childCount +
                      "，ClientUiPage=" + pages.Length +
                      "，ClientHeroDetailPage=" + duplicates.Length +
                      "（新实现 " + CurrentHeroDetailNode + " 已存在，安全）");

            Object.DestroyImmediate(obsolete, true);
            return 1;
        }

        // ------------------------------------------------------------------
        // 3. 联系客服：从 PagesLayer 迁入 PopupLayer（负责人确认它是主页弹窗）
        // ------------------------------------------------------------------

        private static int MigrateContactServiceToPopupLayer()
        {
            GameObject contact = FindSceneObject(ContactServiceNode, true);
            if (contact == null)
            {
                Debug.Log("[结构修复] 未找到 " + ContactServiceNode + "，跳过。");
                return 0;
            }

            Canvas canvas = FindClientCanvas();
            Transform popupLayer = canvas != null ? canvas.transform.Find("PopupLayer") : null;
            if (popupLayer == null)
            {
                Fail("未找到 PopupLayer，未迁移 " + ContactServiceNode + "。");
                return 0;
            }

            int changes = 0;

            // 3a. 移除误挂的个人中心组件与页面标识：联系客服不是可导航页面。
            ClientProfilePage wrongProfile = contact.GetComponent<ClientProfilePage>();
            if (wrongProfile != null)
            {
                Debug.Log("[结构修复] 移除 " + ContactServiceNode + " 上误挂的 ClientProfilePage（复制界面残留）。");
                Object.DestroyImmediate(wrongProfile, true);
                changes++;
            }
            ClientUiPage wrongPage = contact.GetComponent<ClientUiPage>();
            if (wrongPage != null)
            {
                Debug.Log("[结构修复] 移除 " + ContactServiceNode + " 上的 ClientUiPage(pageId=" +
                          wrongPage.PageId + ")，它与 ProfilePage个人中心 重复。");
                Object.DestroyImmediate(wrongPage, true);
                changes++;
            }

            // 3b. 迁移到 PopupLayer。必须先校验锚点非拉伸，否则 SetParent(worldPositionStays:true)
            //     仍会因父级尺寸不同而改变实际大小。
            if (contact.transform.parent != popupLayer)
            {
                RectTransform rect = contact.GetComponent<RectTransform>();
                if (rect == null || rect.anchorMin != rect.anchorMax)
                {
                    Fail("拒绝迁移 " + ContactServiceNode + "：其锚点是拉伸的（anchorMin != anchorMax），" +
                         "跨父级迁移会改变实际尺寸。请先在 Inspector 确认该节点的锚点。");
                    return changes;
                }

                Debug.Log("[结构修复] " + ContactServiceNode + " 从 " + contact.transform.parent.name +
                          " 迁移到 PopupLayer（保留世界变换，不写任何 rect 字段）。");
                contact.transform.SetParent(popupLayer, true);
                changes++;
            }

            // 3c. 挂弹窗标识，使其由 ClientPopupService 统一管理。
            if (contact.GetComponent<ClientUiPopup>() == null)
            {
                contact.AddComponent<ClientUiPopup>();
                Debug.Log("[结构修复] 为 " + ContactServiceNode + " 添加 ClientUiPopup（弹窗身份与可见性锚点）。");
                changes++;
            }

            return changes;
        }

        // ------------------------------------------------------------------
        // 3. 按确认架构迁移层级：PagesLayer 只留 MainPage主页，其余进 PopupLayer
        // ------------------------------------------------------------------

        private static int MigrateToLayers(Canvas canvas)
        {
            int changes = 0;

            for (int i = 0; i < LayerMoves.Length; i++)
            {
                LayerMove move = LayerMoves[i];
                GameObject node = FindSceneObject(move.Node, true);
                if (node == null)
                {
                    Debug.Log("[结构修复] 节点不存在，跳过：" + move.Node);
                    continue;
                }

                Transform target = canvas.transform.Find(move.TargetLayer);
                if (target == null)
                {
                    Fail("未找到目标层 " + move.TargetLayer + "，中止迁移。");
                    return changes;
                }

                // 3a. 迁移层级。用 worldPositionStays: true 保留世界变换；
                //     本工具全程不写 anchorMin/anchorMax/pivot/anchoredPosition/sizeDelta。
                if (node.transform.parent != target)
                {
                    RectTransform rect = node.GetComponent<RectTransform>();
                    bool stretched = rect != null && rect.anchorMin != rect.anchorMax;
                    if (stretched)
                    {
                        // 拉伸锚点的节点，其实际尺寸由父级决定，跨父级迁移会改变尺寸。
                        // 这一类节点均由负责人逐一确认后才允许迁移。
                        Debug.LogWarning("[结构修复] " + move.Node +
                            " 使用拉伸锚点，迁入 " + move.TargetLayer +
                            " 后实际尺寸会随父级改变（负责人已确认接受）。");
                    }
                    Debug.Log("[结构修复] " + move.Node + "：" + node.transform.parent.name +
                              " -> " + move.TargetLayer + "（保留世界变换）");
                    node.transform.SetParent(target, true);
                    changes++;
                }

                // 3b. ClientUiPage -> ClientUiPopup。
                //     必须移除 ClientUiPage：否则 ClientUiNavigator 的场景自动收录会把它当页面，
                //     与 ClientPopupService 同时对同一节点 SetActive，重新引入"两个所有者"。
                ClientUiPage uiPage = node.GetComponent<ClientUiPage>();
                if (uiPage != null)
                {
                    Debug.Log("[结构修复] 移除 " + move.Node + " 的 ClientUiPage(pageId=" + uiPage.PageId +
                              ")，改由弹窗服务统一管理可见性。");
                    Object.DestroyImmediate(uiPage, true);
                    changes++;
                }

                // 3c. 挂弹窗标识并写入路由信息。
                ClientUiPopup popup = node.GetComponent<ClientUiPopup>();
                if (popup == null)
                {
                    popup = node.AddComponent<ClientUiPopup>();
                    changes++;
                }
                WireInt(popup, "_pageId", (int)move.PageId);
                WireBool(popup, "_routeByPageId", move.Routed);
                Debug.Log("[结构修复] " + move.Node + " => ClientUiPopup(pageId=" +
                          (move.Routed ? move.PageId.ToString() : "不路由") + ")");
            }

            // 3d. 补挂页面脚本：这两个节点原先完全没有 Client 脚本。
            changes += AttachComponentIfMissing("FormationPage编队");
            changes += AttachComponentIfMissing("Complete Guide Event Page全图鉴活动");

            return changes;
        }

        /// <summary>为已知的两个无脚本页面节点补挂其 View 组件。</summary>
        private static int AttachComponentIfMissing(string nodeName)
        {
            GameObject node = FindSceneObject(nodeName, true);
            if (node == null)
            {
                Debug.Log("[结构修复] 未找到 " + nodeName + "，跳过补挂脚本。");
                return 0;
            }

            int changes = 0;
            if (nodeName == "FormationPage编队" && node.GetComponent<ClientFormationPage>() == null)
            {
                node.AddComponent<ClientFormationPage>();
                Debug.Log("[结构修复] " + nodeName + " 已挂 ClientFormationPage。");
                changes++;
            }
            if (nodeName == "Complete Guide Event Page全图鉴活动" &&
                node.GetComponent<ClientCompleteGuideEventPage>() == null)
            {
                node.AddComponent<ClientCompleteGuideEventPage>();
                Debug.Log("[结构修复] " + nodeName + " 已挂 ClientCompleteGuideEventPage。");
                changes++;
            }
            return changes;
        }

        private static void WireInt(Object target, string fieldName, int value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogWarning("[结构修复] 未找到序列化字段 " + fieldName + "，跳过。");
                return;
            }
            property.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireBool(Object target, string fieldName, bool value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogWarning("[结构修复] 未找到序列化字段 " + fieldName + "，跳过。");
                return;
            }
            property.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ------------------------------------------------------------------
        // 3.5 迁移后的重新接线
        // ------------------------------------------------------------------

        /// <summary>
        /// 迁移会打破两类"按层级找节点"的隐含假设，这里逐条重新接线。
        /// </summary>
        private static int RewireAfterMigration()
        {
            int changes = 0;

            // A. ClientProfileNavigationController 必须与它的宿主同层。
            //    它原先挂在 PagesLayer 上，用 GetComponentsInChildren 找 Panel_Profile / Content_*。
            //    迁移后 ProfilePage个人中心 已不在 PagesLayer 子树内，7 条路由会全部"未找到页面"。
            GameObject profilePage = FindSceneObject("ProfilePage个人中心", true);
            if (profilePage != null)
            {
                ClientProfileNavigationController existing =
                    profilePage.GetComponent<ClientProfileNavigationController>();
                if (existing == null)
                {
                    ClientProfileNavigationController[] all =
                        Object.FindObjectsOfType<ClientProfileNavigationController>(true);
                    for (int i = 0; i < all.Length; i++)
                    {
                        if (all[i] == null || !all[i].gameObject.scene.IsValid())
                            continue;
                        Debug.Log("[结构修复] 移除 " + all[i].gameObject.name +
                                  " 上的 ClientProfileNavigationController（迁移到 ProfilePage个人中心）。");
                        Object.DestroyImmediate(all[i], true);
                        changes++;
                    }
                    existing = profilePage.AddComponent<ClientProfileNavigationController>();
                    Debug.Log("[结构修复] ProfilePage个人中心 已挂 ClientProfileNavigationController。");
                    changes++;
                }
                // 搜索根显式指定，避免再次依赖"自身在 PagesLayer 下"这一前提。
                WireObjectReference(existing, "_pageSearchRoot", profilePage.transform);
                WireObjectReference(existing, "_mainPanelRoot",
                    FindDescendantByName(profilePage.transform, "PageContentRoot"));
                WireObjectReference(existing, "_personalizePanelRoot",
                    FindDescendantByNameTrimmed(profilePage.transform, "Panel_Personalize"));
                WireObjectReference(existing, "_personalizeContentRoot",
                    FindDescendantByName(profilePage.transform, "SubContentRoot"));
            }
            else
            {
                Debug.LogWarning("[结构修复] 未找到 ProfilePage个人中心，跳过个人中心路由重新接线。");
            }

            // B. FormationPage编队 的 ClientFormationPage 是本次补挂的，序列化字段全空，
            //    运行时会抛 UnassignedReferenceException: _root ... has not been assigned。
            //    _root 就是页面自身，可以直接写入；_slot / _status 需要负责人按实际意图拖拽。
            GameObject formation = FindSceneObject("FormationPage编队", true);
            if (formation != null)
            {
                ClientFormationPage page = formation.GetComponent<ClientFormationPage>();
                if (page != null)
                {
                    WireObjectReference(page, "_root", formation);
                    Debug.Log("[结构修复] ClientFormationPage._root 已写入 FormationPage编队 自身。" +
                              "（`_slot` / `_status` 已于 2026-09-20 按负责人指示删除：" +
                              "场景内无可用文本节点，提示改走 Console。）");
                    changes++;
                }
            }

            // C. 页面 → 已迁出弹窗的序列化槽位。
            //    这些弹窗移到 PopupLayer 后，页面内原来的 Find("Product Purchase Interface") 之类
            //    查找会全部返回 null，表现为"点了不弹窗"。这里按节点名把引用写进各页面的槽位，
            //    既恢复功能，又避免再引入运行时按名字查找。
            GameObject shopPage = FindSceneObject("ShopPage商店", true);
            if (shopPage != null)
            {
                ClientShopPage shop = shopPage.GetComponent<ClientShopPage>();
                if (shop != null)
                {
                    changes += WirePopupSlot(shop, "_purchasePopup", "Product Purchase Interface");
                    changes += WirePopupSlot(shop, "_successPopup", "Purchase Success Page");
                    changes += WirePopupSlot(shop, "_exchangePopup", "Exchange code interface");
                    changes += WirePopupSlot(shop, "_ordersPopup", "Historical Order Interface");
                    changes += WirePopupSlot(shop, "_shipmentPopup", "Pending shipmentCanvas");
                }
            }

            GameObject rankPage = FindSceneObject("RankPage排行榜", true);
            if (rankPage != null)
            {
                ClientRankPage rank = rankPage.GetComponent<ClientRankPage>();
                if (rank != null)
                    changes += WirePopupSlot(rank, "_playerLineupPopup", "Player lineup");
            }

            GameObject homePage = FindSceneObject("MainPage主页", true);
            if (homePage != null)
            {
                ClientHomePage home = homePage.GetComponent<ClientHomePage>();
                if (home != null)
                    changes += WirePopupSlot(home, "_contactServicePopup", "联系客服Page");
            }

            return changes;
        }

        /// <summary>把指定节点写入目标的序列化引用槽位（幂等：已是同一引用则不变更）。</summary>
        private static int WirePopupSlot(Object target, string fieldName, string nodeName)
        {
            GameObject node = FindSceneObject(nodeName, true);
            if (node == null)
            {
                Debug.LogWarning("[结构修复] 未找到弹窗节点 " + nodeName + "，跳过 " + fieldName + " 接线。");
                return 0;
            }

            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogWarning("[结构修复] " + target.GetType().Name +
                                 " 上没有序列化字段 " + fieldName + "，跳过。");
                return 0;
            }
            if (property.objectReferenceValue == node)
                return 0;

            property.objectReferenceValue = node;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[结构修复] " + target.GetType().Name + "." + fieldName + " <- " + nodeName);
            return 1;
        }

        /// <summary>
        /// 关闭纯装饰节点的射线。只改 Graphic.m_RaycastTarget，
        /// 不动渲染顺序、不动 anchor/sizeDelta/anchoredPosition。
        /// </summary>
        private static int DisableRaycast(string path)
        {
            Transform node = FindByPath(path);
            if (node == null)
            {
                Debug.LogWarning("[结构修复] 未找到节点，跳过 raycastTarget 关闭：" + path);
                return 0;
            }

            Graphic graphic = node.GetComponent<Graphic>();
            if (graphic == null)
            {
                Debug.LogWarning("[结构修复] " + path + " 上没有 Graphic 组件，跳过。");
                return 0;
            }
            if (!graphic.raycastTarget)
                return 0;

            SerializedObject so = new SerializedObject(graphic);
            SerializedProperty property = so.FindProperty("m_RaycastTarget");
            if (property == null)
                return 0;

            property.boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("[结构修复] " + path + " 的 raycastTarget 已关闭（装饰节点，不再吞掉点击）。");
            return 1;
        }

        /// <summary>
        /// 给指定容器下所有匹配前缀的卡牌节点补 Button（已存在则跳过）。
        /// 只添加交互组件，不改尺寸/布局。EventSystem 会从子节点的 Graphic 向上冒泡到该 Button。
        /// </summary>
        private static int AddCardButtons(string contentPath, string cardPrefix)
        {
            Transform content = FindByPath(contentPath);
            if (content == null)
            {
                Debug.LogWarning("[结构修复] 未找到卡牌容器：" + contentPath);
                return 0;
            }

            int changes = 0;
            for (int i = 0; i < content.childCount; i++)
            {
                Transform card = content.GetChild(i);
                if (!card.name.StartsWith(cardPrefix, System.StringComparison.Ordinal))
                    continue;
                if (card.GetComponent<Button>() != null)
                    continue;

                Button button = card.gameObject.AddComponent<Button>();
                // 有 Graphic 就设为目标图（便于后续统一做交互反馈）；没有也能靠冒泡收到点击。
                button.targetGraphic = card.GetComponent<Graphic>();
                button.transition = Selectable.Transition.None;
                Debug.Log("[结构修复] 卡牌 " + card.name + " 已补 Button（用于 OpenListing(index)）。");
                changes++;
            }
            if (changes == 0)
                Debug.Log("[结构修复] " + contentPath + " 下的卡牌已有 Button 或未匹配到，跳过。");
            return changes;
        }

        // ------------------------------------------------------------------
        // 5. 射线与显示解耦（全场景）
        // ------------------------------------------------------------------

        /// <summary>
        /// 把"是否参与射线"与"是否显示"彻底解耦：只有交互元素需要参与射线，
        /// 纯显示用的 Image / RawImage / 文本一律关闭 raycastTarget，不再吞掉点击。
        ///
        /// 判定规则（保守，避免把交互区点成死区）：
        ///   自身、或自身到最近 Canvas 之间的任一祖先上存在交互组件 → **保留** raycastTarget。
        ///   否则 → 关闭。
        ///
        /// 为什么不能按"该节点没有 Button 就关"：交互节点的命中面常常由**子节点的 Image** 提供。
        /// 例如商城的 道具卡牌prefab 根节点挂 Button，但根节点自身没有 Graphic，
        /// 点击是靠子节点立绘接收后向上冒泡到 Button 的。若把子节点的射线一起关掉，
        /// 卡牌会立刻变成不可点的死区。
        /// </summary>
        [MenuItem("Client/结构修复/5. 解耦射线与显示（全场景）", false, 14)]
        public static void DecoupleRaycastMenu()
        {
            if (!Application.isBatchMode && !EditorUtility.DisplayDialog(
                    "解耦射线与显示",
                    "将遍历整个 ClientShell 场景，把非交互 Graphic 的 raycastTarget 关闭。\n\n" +
                    "规则：自身或到 Canvas 之间的祖先上存在交互组件 → 保留；否则关闭。\n" +
                    "不修改任何尺寸、锚点、位置或渲染顺序。\n\n是否继续？",
                    "执行", "取消"))
                return;

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            int changed = DecoupleRaycastFromDisplay();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[结构修复] 射线解耦完成，共关闭 " + changed + " 个 Graphic 的 raycastTarget。");
            Finish(0);
        }

        private static int DecoupleRaycastFromDisplay()
        {
            Graphic[] graphics = Object.FindObjectsOfType<Graphic>(true);
            StringBuilder disabledList = new StringBuilder();
            int scanned = 0;
            int alreadyOff = 0;
            int keptInteractive = 0;
            int disabled = 0;

            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic == null || !graphic.gameObject.scene.IsValid())
                    continue;
                scanned++;

                if (!graphic.raycastTarget)
                {
                    alreadyOff++;
                    continue;
                }

                if (HasInteractiveSelfOrAncestor(graphic.transform))
                {
                    keptInteractive++;
                    continue;
                }

                SerializedObject so = new SerializedObject(graphic);
                SerializedProperty property = so.FindProperty("m_RaycastTarget");
                if (property == null)
                    continue;
                property.boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
                disabled++;

                if (disabled <= 40)
                    disabledList.Append("\n    - ").Append(ClientUiTrace.Path(graphic.transform));
            }

            Debug.Log("[结构修复] 射线解耦：扫描 Graphic " + scanned +
                      " 个；关闭 " + disabled +
                      " 个；保留（交互区） " + keptInteractive +
                      " 个；本来就是关的 " + alreadyOff + " 个。" +
                      (disabled > 0 ? "\n  本次关闭的前若干项：" + disabledList : ""));

            // 自愈：有些交互节点的命中面原本在**祖先**的 Image 上（祖先不是交互节点，会被关掉），
            // 例如 …/avatar Mask/Avatar 上的 Button —— Avatar 自身没有 Graphic。
            // 这里为这类节点恢复一个最近的可用射线表面，避免把它们点成死区。
            RestoreHitAreasForInteractives();
            ReportInteractiveWithoutHitArea();
            return disabled;
        }

        /// <summary>为失去射线表面的交互节点恢复最近的可用命中面。</summary>
        private static int RestoreHitAreasForInteractives()
        {
            Selectable[] selectables = Object.FindObjectsOfType<Selectable>(true);
            int restored = 0;
            StringBuilder list = new StringBuilder();

            for (int i = 0; i < selectables.Length; i++)
            {
                Selectable selectable = selectables[i];
                if (selectable == null || !selectable.gameObject.scene.IsValid())
                    continue;
                if (HasRaycastableGraphicInSubtree(selectable.transform))
                    continue;

                Graphic candidate = FindHitAreaCandidate(selectable);
                if (candidate == null)
                    continue;

                SerializedObject so = new SerializedObject(candidate);
                SerializedProperty property = so.FindProperty("m_RaycastTarget");
                if (property == null || property.boolValue)
                    continue;
                property.boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
                restored++;

                if (restored <= 25)
                    list.Append("\n    + ").Append(ClientUiTrace.Path(selectable.transform))
                        .Append("  ← 命中面恢复为 ").Append(ClientUiTrace.Path(candidate.transform));
            }

            if (restored > 0)
                Debug.Log("[结构修复] 命中面自愈：为 " + restored +
                          " 个交互节点恢复了射线表面（它们的命中面原本挂在祖先上）：" + list);
            return restored;
        }

        private static Graphic FindHitAreaCandidate(Selectable selectable)
        {
            // 1) 组件自己指定的目标图最合适。
            if (selectable.targetGraphic != null)
                return selectable.targetGraphic;

            // 2) 退一步用它子树里的第一个 Graphic。
            Graphic[] own = selectable.GetComponentsInChildren<Graphic>(true);
            if (own.Length > 0)
                return own[0];

            // 3) 再退一步：向上找最近的祖先 Graphic（命中面原本就在那里）。
            Transform parent = selectable.transform.parent;
            int guard = 0;
            while (parent != null && guard < 8)
            {
                Graphic graphic = parent.GetComponent<Graphic>();
                if (graphic != null)
                    return graphic;
                parent = parent.parent;
                guard++;
            }
            return null;
        }

        /// <summary>体检：交互节点最终是否还存在可射线的表面，否则它点不动。</summary>
        private static void ReportInteractiveWithoutHitArea()
        {
            Selectable[] selectables = Object.FindObjectsOfType<Selectable>(true);
            int broken = 0;
            StringBuilder list = new StringBuilder();
            for (int i = 0; i < selectables.Length; i++)
            {
                Selectable selectable = selectables[i];
                if (selectable == null || !selectable.gameObject.scene.IsValid())
                    continue;
                if (HasRaycastableGraphicInSubtree(selectable.transform))
                    continue;

                broken++;
                if (broken <= 20)
                    list.Append("\n    ! ").Append(ClientUiTrace.Path(selectable.transform))
                        .Append("  (").Append(selectable.GetType().Name).Append(")");
            }

            if (broken == 0)
                Debug.Log("[结构修复] 交互区体检：所有 Button/Toggle 等仍保有可射线表面 ✓");
            else
                Debug.LogWarning("[结构修复] 交互区体检：发现 " + broken +
                                 " 个交互节点没有任何可射线表面，它们将无法被点击（需人工确认）：" + list);
        }

        private static bool HasRaycastableGraphicInSubtree(Transform root)
        {
            Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
                if (graphics[i] != null && graphics[i].raycastTarget)
                    return true;
            return false;
        }

        /// <summary>自身或到最近 Canvas 之间的祖先上是否存在交互组件（不跨 Canvas 继承）。</summary>
        private static bool HasInteractiveSelfOrAncestor(Transform node)
        {
            Transform current = node;
            int guard = 0;
            while (current != null && guard < 32)
            {
                if (IsInteractive(current.gameObject))
                    return true;

                // 不跨 Canvas 继承：子 Canvas 是独立的交互域。
                if (current != node && current.GetComponent<Canvas>() != null)
                    return false;

                current = current.parent;
                guard++;
            }
            return false;
        }

        private static bool IsInteractive(GameObject target)
        {
            if (target == null)
                return false;

            // Selectable 覆盖 Button / Toggle / Slider / Scrollbar / Dropdown / InputField / TMP_InputField。
            if (target.GetComponent<Selectable>() != null)
                return true;
            if (target.GetComponent<ScrollRect>() != null)
                return true;
            if (target.GetComponent<EventTrigger>() != null)
                return true;

            // 自定义交互脚本：任何事件处理器接口都视为交互。
            MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour == null)
                    continue;
                if (behaviour is IPointerClickHandler || behaviour is IPointerDownHandler ||
                    behaviour is IPointerUpHandler || behaviour is IPointerEnterHandler ||
                    behaviour is IPointerExitHandler || behaviour is IDragHandler ||
                    behaviour is IBeginDragHandler || behaviour is IEndDragHandler ||
                    behaviour is IDropHandler || behaviour is IScrollHandler)
                    return true;
            }
            return false;
        }

        private static Transform FindDescendantByName(Transform root, string name)
        {
            if (root == null)
                return null;
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i].name == name)
                    return all[i];
            return null;
        }

        private static Transform FindDescendantByNameTrimmed(Transform root, string name)
        {
            if (root == null)
                return null;
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
                if (all[i].name.Trim() == name)
                    return all[i];
            return null;
        }

        // ------------------------------------------------------------------
        // 3.6 ToggleGroup 与后退按钮
        // ------------------------------------------------------------------

        /// <summary>
        /// 后退按钮路径（相对 ClientCanvas，逐级精确名）。
        /// 语义统一为"关闭当前弹窗回上一层"，由 ClientUiBackButton → navigator.Back() 实现。
        /// 负责人已确认最后一条（活动任务Canvas 的后退）语义同样是关闭活动页。
        /// </summary>
        private static readonly string[] BackButtonPaths =
        {
            "ProfilePage个人中心/后退Button",
            "HeroPage英雄/downCanvas/后退Button",
            "HeroDetailPage英雄详情主页/后退Button",
            "MailPage邮件/后退Button",
            "FormationPage编队/下功能底框/后退Button",
            "ShopPage商店/Bottom page function bar/BackoffButton",
            "RankPage排行榜/Bottom page function bar/BackoffButton",
            "ActivityPage活动/Bottom page function bar/BackoffButton",
            "ActivityPage活动/活动任务Canvas/BackoffButton",
            "Complete Guide Event Page全图鉴活动/Bottom page function bar/BackoffButton",
            "Historical Order Interface/BackoffButton",
            "Pending shipmentCanvas/BackoffButton",
            "Pending shipmentCanvas/Bottom page function bar/BackoffButton",
        };

        private static int EnsureTogglesAndBackButtons()
        {
            int changes = 0;

            // A. ToggleGroup A 上补 ToggleGroup，并把 3 个顶部 Toggle 指过去。
            //
            //    实测：该节点上没有任何 ToggleGroup 组件，3 个 Toggle 的 m_Group 全为 0。
            //    代码侧已放宽（按名字识别），但"互斥视觉"（选中底框互斥）仍需要真正的分组。
            GameObject toggleGroupA = FindSceneObject("ToggleGroup A", true);
            if (toggleGroupA != null)
            {
                ToggleGroup group = toggleGroupA.GetComponent<ToggleGroup>();
                if (group == null)
                {
                    group = toggleGroupA.AddComponent<ToggleGroup>();
                    WireBool(group, "m_AllowSwitchOff", false);
                    Debug.Log("[结构修复] ToggleGroup A 已补挂 ToggleGroup（AllowSwitchOff=false）。");
                    changes++;
                }

                string[] topNames = { "个人中心Toggle", "主页展示Toggle", "个性化Toggle" };
                for (int i = 0; i < topNames.Length; i++)
                {
                    Toggle toggle = FindDescendantToggle(toggleGroupA.transform, topNames[i]);
                    if (toggle == null)
                    {
                        Debug.LogWarning("[结构修复] ToggleGroup A 下未找到 " + topNames[i] + "。");
                        continue;
                    }
                    if (toggle.group == group)
                        continue;
                    WireObjectReference(toggle, "m_Group", group);
                    Debug.Log("[结构修复] " + topNames[i] + ".m_Group <- ToggleGroup A");
                    changes++;
                }
            }
            else
            {
                Debug.LogWarning("[结构修复] 未找到 ToggleGroup A，跳过顶部 Toggle 分组。");
            }

            // B. 后退按钮：全场景此前 0 个 ClientUiBackButton
            //    （旧工具的 AttachBackButton 找的是不存在的 ReturnButton），因此所有后退键都是死的。
            for (int i = 0; i < BackButtonPaths.Length; i++)
                changes += EnsureBackButton(BackButtonPaths[i]);

            // C. 点击被装饰图吃掉的问题——**不要用改渲染顺序去修**。
            //
            //    上一次尝试把 PageContentRoot 提到最前，虽然点击通了，但改变了视觉叠层，
            //    负责人明确不接受。正确做法：恢复原渲染顺序，只把"纯装饰、不承担交互"的
            //    Graphic 的 raycastTarget 关掉，让射线穿透到下方的 Toggle。
            //
            //    日志证据（EventSystem 命中列表首位即最上层）：
            //      .../ProfilePage个人中心/PageContentRoot/Panel_Showcase/大铭牌立绘image
            Transform pageContentRoot = FindByPath("ProfilePage个人中心/PageContentRoot");
            if (pageContentRoot != null && pageContentRoot.GetSiblingIndex() != 1)
            {
                // 恢复场景原本的顺序：1) ToggleGroup A  2) PageContentRoot  3) 后退Button
                pageContentRoot.SetSiblingIndex(1);
                Debug.Log("[结构修复] ProfilePage个人中心/PageContentRoot 已恢复到原有的渲染顺序（第 2 位），" +
                          "视觉叠层与改动前一致。");
                changes++;
            }

            changes += DisableRaycast("ProfilePage个人中心/PageContentRoot/Panel_Showcase");
            changes += DisableRaycast("ProfilePage个人中心/PageContentRoot/Panel_Showcase/铭牌Image");
            changes += DisableRaycast("ProfilePage个人中心/PageContentRoot/Panel_Showcase/大铭牌立绘image");

            // E. 商城的道具卡牌补 Button。
            //
            //    实测：道具卡牌prefab 整棵子树 0 个 Button，且 ClientShopPage 原来按
            //    "展示卡牌Button" 前缀绑定（场景中不存在该名），因此道具卡完全不可点、
            //    购买流程无法从 UI 触发。负责人已确认补 Button 并修正绑定名。
            //    只添加 Button 组件（交互），不改任何尺寸/布局；Button 通过 EventSystem
            //    从子节点的 Graphic 向上冒泡即可收到点击，无需 targetGraphic。
            changes += AddCardButtons("ShopPage商店/Product page/展示卡牌Scroll View/Viewport/Content", "道具卡牌prefab");

            // D. 展示卡牌Scroll View 1 的 Content 锚点是"双向拉伸"(0,0)-(1,1)，
            //    而 ContentSizeFitter 无法驱动双向拉伸的 RectTransform → Content 高度恒等于
            //    Viewport → 无溢出 → 列表滚不动。对照可滚的 Scroll View / 活动任务Scroll View，
            //    它们的 Content 都是 (0,1)-(1,1)（只横向拉伸）。此处改为同样的标准配置。
            //    负责人已确认；只写 3 个锚点值，不改任何尺寸数值。
            Transform showcaseContent = FindByPath(
                "ProfilePage个人中心/PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content");
            if (showcaseContent != null)
            {
                RectTransform rect = showcaseContent.GetComponent<RectTransform>();
                Vector2 targetMin = new Vector2(0f, 1f);
                Vector2 targetMax = new Vector2(1f, 1f);
                Vector2 targetPivot = new Vector2(0.5f, 1f);
                if (rect != null &&
                    (rect.anchorMin != targetMin || rect.anchorMax != targetMax || rect.pivot != targetPivot))
                {
                    rect.anchorMin = targetMin;
                    rect.anchorMax = targetMax;
                    rect.pivot = targetPivot;
                    Debug.Log("[结构修复] 展示卡牌Scroll View 1/Viewport/Content 锚点改为 (0,1)-(1,1) + pivot(0.5,1)，" +
                              "使 ContentSizeFitter 能按卡牌数量驱动高度，列表恢复可滚动。");
                    changes++;
                }

                // D2. 真正的滚动阻塞点：ContentSizeFitter 的两个轴都是 Unconstrained(0)，
                //     等于完全不生效 → Content 高度不随卡牌增长 → 无溢出 → ScrollRect 无内容可滚。
                //     实测：GridLayoutGroup 固定 3 列、cell 204x326、9 张卡 → 需要约 1012 高，
                //     而 Viewport 只有约 625。设为 VerticalFit = PreferredSize(1) 即可。
                ContentSizeFitter fitter = showcaseContent.GetComponent<ContentSizeFitter>();
                if (fitter != null)
                {
                    SerializedObject fitterSo = new SerializedObject(fitter);
                    SerializedProperty verticalFit = fitterSo.FindProperty("m_VerticalFit");
                    if (verticalFit != null && verticalFit.intValue != 1)
                    {
                        verticalFit.intValue = 1; // 1 = PreferredSize
                        fitterSo.ApplyModifiedPropertiesWithoutUndo();
                        Debug.Log("[结构修复] 展示卡牌Scroll View 1 的 ContentSizeFitter.VerticalFit = PreferredSize，" +
                                  "Content 高度开始随卡牌数量增长（这是滚动的必要条件）。");
                        changes++;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[结构修复] 未找到 展示卡牌Scroll View 1/Viewport/Content，跳过锚点修复。");
            }

            return changes;
        }

        private static int EnsureBackButton(string path)
        {
            Transform node = FindByPath(path);
            if (node == null)
            {
                Debug.LogWarning("[结构修复] 未找到后退按钮：" + path);
                return 0;
            }
            if (node.GetComponent<ClientUiBackButton>() != null)
                return 0;

            node.gameObject.AddComponent<ClientUiBackButton>();
            Debug.Log("[结构修复] " + path + " 已挂 ClientUiBackButton（→ navigator.Back()）。");
            return 1;
        }

        /// <summary>按 "根节点/子/孙" 逐级精确名查找。</summary>
        /// <summary>
        /// **BUG-018**：把 玩家名 / 玩家ID / 战力 三个文本接到 `ClientProfilePage`。
        ///
        /// 节点路径已取证（`CURRENT_STATE.md` 第 59 轮）：三者都是 **`TMP_Text`**
        /// （实测 guid `f4688fdb…`）—— 这也是原代码用旧版 `Text` 字段一直接不上的原因。
        /// </summary>
        private static int WireProfileTexts()
        {
            GameObject page = FindSceneObject("ProfilePage个人中心", true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 ProfilePage个人中心，跳过 BUG-018 文本接线。");
                return 0;
            }

            ClientProfilePage profile = page.GetComponent<ClientProfilePage>();
            if (profile == null)
            {
                Debug.LogWarning("[结构修复] ProfilePage个人中心 上没有 ClientProfilePage，跳过 BUG-018 文本接线。");
                return 0;
            }

            int changes = 0;
            changes += WireTextComponent(profile, "_playerName",
                "ProfilePage个人中心/PageContentRoot/Panel_Profile/底框/玩家名称Image/Text (TMP)");
            changes += WireTextComponent(profile, "_playerId",
                "ProfilePage个人中心/PageContentRoot/Panel_Profile/底框/玩家IDImage/Text (TMP)");
            changes += WireTextComponent(profile, "_combatPower",
                "ProfilePage个人中心/PageContentRoot/Panel_Profile/MiddleImage/Panel1/战力底框/Text (TMP)");
            return changes;
        }

        /// <summary>按路径找到节点上的 **TMP 文本组件** 并接到字段上（找不到只警告，不新建）。</summary>
        private static int WireTextComponent(Object target, string field, string path)
        {
            Transform node = FindByPath(path);
            if (node == null)
                node = FindByPathTrimmed(path);
            if (node == null)
            {
                Debug.LogWarning("[结构修复] 未找到文本节点：" + path + "（字段 " + field + " 未接线）");
                return 0;
            }

            TMPro.TMP_Text text = node.GetComponent<TMPro.TMP_Text>();
            if (text == null)
            {
                Debug.LogWarning("[结构修复] " + path + " 上没有 TMP 文本组件（字段 " + field + " 未接线）");
                return 0;
            }

            WireObjectReference(target, field, text);
            Debug.Log("[结构修复] " + field + " <- " + node.name + "（TMP）");
            return 1;
        }
        /// <summary>
        /// **BUG-020**：购买数量进度条 —— 接 `_progressFill`。
        ///
        /// 取证（2026-09-20）：
        /// · 组件 `ClientShopPurchaseQuantityProgress` 挂在
        ///   `Product Purchase Interface/Purchase of digital items/底框背景/Purchase progress bar/Progress bar icon`；
        /// · 其**同级** `Progress bar` 是唯一的填充 Image（无 sprite、纯色、`raycastTarget=0`）⇒ 就是 `_progressFill`。
        ///
        /// ⚠️ `_minimumText` / `_maximumText` **刻意不接**：设计里**没有**对应的"0 / 上限"文本节点
        /// （`Progress bar` 无任何子节点）；唯一可疑的 `Prop Description/限购数量` 烘死文案是 `（1/6）`（当前/上限），
        /// 与代码写入的 `"0"` / `_maximumQuantity` **语义不同** —— 硬接会覆盖设计文案。
        /// 组件对这两个字段都判空，留空不报错；是否需要新增节点由负责人决定。
        /// </summary>
        private static int WireShopProgress()
        {
            const string iconPath =
                "Product Purchase Interface/Purchase of digital items/底框背景/Purchase progress bar/Progress bar icon";

            Transform icon = FindByPath(iconPath);
            if (icon == null)
                icon = FindByPathTrimmed(iconPath);
            if (icon == null)
            {
                Debug.LogWarning("[结构修复] 未找到购买进度条组件节点，跳过 BUG-020。");
                return 0;
            }

            Transform fill = icon.parent != null ? icon.parent.Find("Progress bar") : null;
            if (fill == null)
            {
                Debug.LogWarning("[结构修复] 未在同级找到填充节点 `Progress bar`，跳过 BUG-020。");
                return 0;
            }

            ClientShopPurchaseQuantityProgress progress = icon.GetComponent<ClientShopPurchaseQuantityProgress>();
            if (progress == null)
            {
                Debug.LogWarning("[结构修复] Progress bar icon 上没有 ClientShopPurchaseQuantityProgress，跳过 BUG-020。");
                return 0;
            }

            Image fillImage = fill.GetComponent<Image>();
            if (fillImage == null)
            {
                Debug.LogWarning("[结构修复] `Progress bar` 上没有 Image 组件，跳过 BUG-020。");
                return 0;
            }

            WireObjectReference(progress, "_progressFill", fillImage);
            Debug.Log("[结构修复] BUG-020: _progressFill <- Progress bar(Image)（" +
                      "`_minimumText`/`_maximumText` 按取证结论留空：设计里没有对应文本节点）");
            return 1;
        }
        /// <summary>
        /// **BUG-019 的可做部分**：填 `ClientRankPage` 的资源查找表。
        ///
        /// 背景：6 张 `VisualBinding[]`（`Id` + `Sprite`）在场景里**全是空的**，
        /// `SetSprite()` 按字符串 id 精确匹配 ⇒ 拿不到图时卡片只剩烘焙占位图。
        ///
        /// 本轮只填**工程内确有贴图**的两类（数量已实测）：
        /// · `_qualities`：契约见 `ClientConfigModels` 注释 —— 资源名约定 `QualityIcon_&lt;Quality&gt;`；
        ///   实测有 `QualityIcon_2..6.png`（5 个）⇒ 注册 id `"2".."6"`；
        ///   另外本地模拟的阵容卡用的是占位 id `"quality-placeholder"`，一并注册（指到 `QualityIcon_3`）以便 Play 时可见。
        /// · `_illustrations`：`HeroPortrait_&lt;heroId&gt;.png` 实测 4 个 ⇒ 注册 id 为"英雄 id"（`12001` 等，与文件名一致），
        ///   并把占位 id `"illustration-placeholder"` 指到第一张，便于本地模拟可见。
        ///
        /// **刻意不填**（工程内 0 个贴图，填了只会是空引用）：`_avatars`（`HeroAvatar_*`）、`_frames`（`AvatarFrame_*`）、
        /// `_badges`（`Medal_*`）、`_attributes`（`AttributeIcon_*`）。
        ///
        /// 幂等：每次都按当前工程内贴图重建这两张表。
        /// </summary>
        private static int FillRankVisualTables()
        {
            GameObject page = FindSceneObject("RankPage排行榜", true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 RankPage排行榜，跳过排行榜资源表。");
                return 0;
            }

            ClientRankPage rank = page.GetComponent<ClientRankPage>();
            if (rank == null)
            {
                Debug.LogWarning("[结构修复] RankPage排行榜 上没有 ClientRankPage，跳过其资源表。");
                return 0;
            }

            string qualityDir = FindSpriteDirectory("QualityIcon_2");
            string portraitDir = FindSpriteDirectory("HeroPortrait_13001");
            if (qualityDir == null && portraitDir == null)
            {
                Debug.LogWarning("[结构修复] 未找到 QualityIcon_* / HeroPortrait_* 贴图目录，跳过排行榜资源表。");
                return 0;
            }

            SerializedObject so = new SerializedObject(rank);
            int changes = 0;
            changes += FillVisualTable(so, "_qualities", qualityDir, "QualityIcon_", "quality-placeholder");
            changes += FillVisualTable(so, "_illustrations", portraitDir, "HeroPortrait_", "illustration-placeholder");
            so.ApplyModifiedPropertiesWithoutUndo();

            if (changes > 0)
                Debug.Log("[结构修复] 排行榜资源表已按工程内实际贴图重建（共 " + changes + " 项）：" +
                          "`_qualities` = QualityIcon_*，`_illustrations` = HeroPortrait_*；" +
                          "`_avatars`/`_frames`/`_badges`/`_attributes` 保持空（工程内无对应贴图）。");
            return changes;
        }

        /// <summary>在 `Assets` 下按文件基名找目录（如 `QualityIcon_2` / `HeroPortrait_13001`），找不到返回 null。</summary>
        private static string FindSpriteDirectory(string baseName)
        {
            string[] guids = AssetDatabase.FindAssets(baseName + " t:Sprite");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == baseName)
                    return System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            }
            return null;
        }

        /// <summary>
        /// 用 SerializedProperty 填一张 `VisualBinding[]`（不去引用那个 private 嵌套类型）。
        /// <paramref name="placeholderId"/> 非空时，额外把该占位 id 指向第一张图，便于本地模拟可见。
        /// </summary>
        private static int FillVisualTable(SerializedObject so, string fieldName, string directory,
                                           string prefix, string placeholderId)
        {
            if (string.IsNullOrEmpty(directory))
                return 0;

            SerializedProperty property = so.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogWarning("[结构修复] ClientRankPage 上找不到字段 " + fieldName + "，跳过。");
                return 0;
            }

            string[] guids = AssetDatabase.FindAssets(prefix + " t:Sprite", new[] { directory });
            List<string> ids = new List<string>();
            List<Sprite> sprites = new List<Sprite>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                    continue;

                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (!name.StartsWith(prefix, System.StringComparison.Ordinal))
                    continue;

                ids.Add(name.Substring(prefix.Length));
                sprites.Add(sprite);
            }

            if (ids.Count == 0)
            {
                Debug.Log("[结构修复] " + fieldName + "：目录 " + directory + " 下没有 " + prefix + "* 的 Sprite，保持为空。");
                return 0;
            }

            int total = ids.Count + (string.IsNullOrEmpty(placeholderId) ? 0 : 1);
            property.arraySize = total;
            int written = 0;
            for (int i = 0; i < ids.Count; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("Id").stringValue = ids[i];
                element.FindPropertyRelative("Sprite").objectReferenceValue = sprites[i];
                written++;
            }
            if (total > ids.Count)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(ids.Count);
                element.FindPropertyRelative("Id").stringValue = placeholderId;
                element.FindPropertyRelative("Sprite").objectReferenceValue = sprites[0];
                written++;
            }

            Debug.Log("[结构修复] " + fieldName + " 已重建：" + total + " 项（" + string.Join(", ", ids.ToArray()) +
                      (total > ids.Count ? " + 占位 id " + placeholderId : "") + "）。");
            return written;
        }
        /// <summary>
        /// **BUG-026 ②**：给大厅的邮件入口补一个红点，并接线 `ClientHomeRedDotController._mailDot`。
        ///
        /// 取证（2026-09-21）：全场景只有 `MainPage主页/downCanvas/ActivityButton/RedDot` 一个红点节点
        /// （活动红点已接线 ✓）；邮件入口**没有**红点节点，而且**公告在场景里根本没有入口按钮** ⇒ 本步骤只处理邮件。
        ///
        /// 做法：克隆现成的 `RedDot`（它带 UGUI `Text`，与字段类型一致 ✓），挂到邮件入口下，再接线。
        /// 幂等：已存在同名节点则只补接线；新节点默认隐藏（由控制器按未读邮件数决定显隐）。
        /// </summary>
        private static int EnsureMailRedDot()
        {
            GameObject page = FindSceneObject("MainPage主页", true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 MainPage主页，跳过邮件红点。");
                return 0;
            }

            ClientHomeRedDotController controller = page.GetComponent<ClientHomeRedDotController>();
            if (controller == null)
            {
                Debug.LogWarning("[结构修复] MainPage主页 上没有 ClientHomeRedDotController，跳过邮件红点。");
                return 0;
            }

            Transform mailEntry = FindDescendantByName(page.transform, "BottomFunctionIcon_Mail");
            if (mailEntry == null)
            {
                Debug.LogWarning("[结构修复] 未找到邮件入口 BottomFunctionIcon_Mail，跳过邮件红点。");
                return 0;
            }

            int changes = 0;
            GameObject dotObject = FindChildObject(mailEntry, MailRedDotName);
            Transform dot = dotObject != null ? dotObject.transform : null;
            if (dot == null)
            {
                Transform source = FindByPath("MainPage主页/downCanvas/ActivityButton/RedDot");
                if (source == null)
                    source = FindDescendantByName(page.transform, "RedDot");
                if (source == null)
                {
                    Debug.LogWarning("[结构修复] 找不到可克隆的红点源节点（ActivityButton/RedDot），跳过。");
                    return 0;
                }

                GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, mailEntry);
                clone.name = MailRedDotName;
                clone.SetActive(false);
                dot = clone.transform;
                changes++;
                Debug.Log("[结构修复] 已在邮件入口下新建红点节点「" + MailRedDotName + "」（克隆自活动红点）。");
            }

            Text dotText = dot.GetComponent<Text>();
            if (dotText == null)
            {
                Debug.LogWarning("[结构修复] 邮件红点节点上没有 UGUI Text（字段 _mailDot 是 Text 类型），未接线。");
                return changes;
            }

            WireObjectReference(controller, "_mailDot", dotText);
            Debug.Log("[结构修复] ClientHomeRedDotController._mailDot <- " + mailEntry.name + "/" + MailRedDotName);
            return changes + 1;
        }

        /// <summary>
        /// **BUG-026 ④**：接线 `BottomFunctionIconView._icon`（英雄详情页页签按钮的图标）。
        ///
        /// 取证（2026-09-21）：`Bottom1`..`Bottom4` 并不在大厅，而在 `HeroDetailPage英雄详情主页/upCanvas/Panel*`；
        /// 每个节点只有一个 `Image` 子节点、**没有文字节点** ⇒ `_label` **无节点可接**（且 `Configure` 目前没有调用方）。
        /// 因此本步骤只接 `_icon`，并把该结论写进日志备查。
        /// </summary>
        private static int WireBottomFunctionIcons()
        {
            GameObject detail = FindSceneObject("HeroDetailPage英雄详情主页", true);
            if (detail == null)
            {
                Debug.LogWarning("[结构修复] 未找到 HeroDetailPage英雄详情主页，跳过 BottomFunctionIconView 接线。");
                return 0;
            }

            int changes = 0;
            Transform[] all = detail.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                Transform view = all[i];
                if (!view.name.StartsWith("Bottom", System.StringComparison.Ordinal))
                    continue;

                BottomFunctionIconView component = view.GetComponent<BottomFunctionIconView>();
                if (component == null)
                    continue;

                GameObject iconNode = FindChildObject(view, "Image");
                Image icon = iconNode != null ? iconNode.GetComponent<Image>() : null;
                if (icon == null)
                {
                    Debug.LogWarning("[结构修复] " + view.name + " 下没有 Image 子节点，_icon 未接线。");
                    continue;
                }

                WireObjectReference(component, "_icon", icon);
                changes++;
                Debug.Log("[结构修复] BottomFunctionIconView._icon <- " + view.name + "/Image（_label 无对应节点，保持未接线）");
            }
            return changes;
        }

        /// <summary>邮件红点节点名（BUG-026 ②）。</summary>
        private const string MailRedDotName = "RedDot";

        /// <summary>阵容卡节点名前缀（BUG-026 ⑤）。</summary>
        private const string ShowcaseCardPrefix = "展示卡牌";

        /// <summary>客户端专用启动场景（BUG-025 方案 C）。</summary>
        private const string ClientBootScenePath = "Assets/Client/Scenes/ClientBoot.unity";
        private const string ClientShellScenePath = "Assets/Client/Scenes/ClientShell.unity";

        /// <summary>
        /// **BUG-026 ⑤**：给"玩家阵容"弹窗建出阵容卡节点。
        ///
        /// 取证（2026-09-21）：`Player lineup` 子树里**一个 `展示卡牌*` 都没有**，而 `ClientRankPage.CollectLineupCards()`
        /// 正是按 `Content` 下 `展示卡牌` 前缀收集 ⇒ 弹窗只有"层数/回合数/战力"，没有卡牌。
        ///
        /// 做法（**不新增美术、不臆造版式**）：在 `Player lineup/玩家阵容` 下建一个 `Content`（GridLayoutGroup 3 列），
        /// 放入 **3 个共用卡 prefab 的实例**并命名为 `展示卡牌1..3` —— 该 prefab **已包含** `卡牌名称` / `等级-Text` /
        /// `立绘Image` / `品质Image` / `品质底框Image` / `属性图标Image` / `星级图标Image (1..5)`，
        /// 正好覆盖 `LineupCardRow` 需要的节点（等级名差异已在代码里加 `等级-Text` 回退）。
        ///
        /// 幂等：同名子节点已存在则不重建；每次运行都确保 3 个。
        /// </summary>
        private static int EnsureLineupCards()
        {
            GameObject page = FindSceneObject("Player lineup", true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 Player lineup，跳过阵容卡。");
                return 0;
            }

            Transform holder = FindDescendantByName(page.transform, "玩家阵容");
            if (holder == null)
                holder = page.transform;

            int changes = 0;

            // 1) Content 容器（3 列网格）
            Transform content = FindChildObject(holder, "Content") != null
                ? FindChildObject(holder, "Content").transform
                : null;
            if (content == null)
            {
                GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
                contentObject.transform.SetParent(holder, false);
                content = contentObject.transform;
                changes++;
                Debug.Log("[结构修复] 已在玩家阵容下新建 Content 容器（GridLayoutGroup）。");
            }

            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 0f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.offsetMin = new Vector2(20f, 20f);
            contentRect.offsetMax = new Vector2(-20f, -20f);

            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
            if (grid == null)
                grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(204f, 326f);   // 与英雄页/编队一致的卡牌格尺寸
            grid.spacing = new Vector2(12f, 12f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            // 2) 三个共用卡实例
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeroCardPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("[结构修复] 未找到卡牌 prefab：" + HeroCardPrefabPath + "，阵容卡未生成。");
                return changes;
            }

            const int wanted = 3;
            for (int index = 1; index <= wanted; index++)
            {
                string cardName = ShowcaseCardPrefix + index;
                if (FindChildObject(content, cardName) != null)
                    continue;

                GameObject clone = PrefabUtility.InstantiatePrefab(prefab, content) as GameObject;
                if (clone == null)
                {
                    Debug.LogWarning("[结构修复] 实例化卡牌 prefab 失败，阵容卡中止。");
                    break;
                }

                clone.name = cardName;
                clone.SetActive(false);   // 由代码按数据条数显隐（CollectLineupCards 只收集，不主动显示）
                changes++;
                Debug.Log("[结构修复] 玩家阵容：已新建 " + cardName + "（克隆自共用卡 prefab，保留 prefab 关联）。");
            }

            return changes;
        }

        /// <summary>
        /// **BUG-026 ⑥**：给"玩家阵容"弹窗补一个**关闭按钮** —— 该弹窗原先子树里**没有任何按钮**，打开后关不掉。
        ///
        /// 做法：克隆一个现成的页面返回按钮（`RankPage排行榜/Bottom page function bar/BackoffButton`），
        /// 命名为 `关闭Button`、挂到 `Player lineup` 弹窗根下，并摆到弹窗右上角；关闭动作由 `ClientRankPage.OpenLineup()` 绑定。
        /// 幂等：已存在则不重建（只校正位置）。
        /// </summary>
        private static int EnsureLineupCloseButton()
        {
            GameObject page = FindSceneObject("Player lineup", true);
            if (page == null)
                return 0;

            const string CloseButtonName = "关闭Button";
            GameObject existing = FindChildObject(page.transform, CloseButtonName);
            if (existing != null)
                return 0;

            Transform source = FindByPath("RankPage排行榜/Bottom page function bar/BackoffButton");
            if (source == null)
                source = FindDescendantByName(FindSceneObject("RankPage排行榜", true) != null
                    ? FindSceneObject("RankPage排行榜", true).transform : null, "BackoffButton");
            if (source == null)
            {
                Debug.LogWarning("[结构修复] 找不到可克隆的返回按钮源（RankPage/Bottom page function bar/BackoffButton），关闭按钮未建。");
                return 0;
            }

            GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, page.transform);
            clone.name = CloseButtonName;

            RectTransform rect = clone.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = new Vector2(-16f, -16f);
            }

            Debug.Log("[结构修复] 已在 Player lineup 右上角新建「" + CloseButtonName + "」（克隆自排行榜返回按钮），由 ClientRankPage 绑定关闭。");
            return 1;
        }

        /// <summary>
        /// **`BUG-025` 方案 C（2026-09-21 负责人选定）**：新建"只做客户端初始化"的 `ClientBoot` 场景并**置首**。
        ///
        /// 现状（实测）：构建清单只有 `Assets/Main/Boot.unity`，客户端场景 `ClientShell` 不在清单且无人加载
        /// ⇒ 导出的 WebGL / 小游戏进不到客户端 UI。
        ///
        /// 本步骤做的事（幂等）：
        /// 1) 场景文件不存在则新建空场景 + 一个挂 `ClientBootLoader` 的 `ClientBoot` 对象并保存；
        /// 2) 把 `ClientBoot` 放进 `EditorBuildSettings` 的**第 0 位**（置首），`ClientShell` 确保在清单中紧随其后；
        /// 3) **不改动、不删除**原 `Boot.unity` 条目（保留兼容性依赖，可随时调整顺序回滚）。
        /// </summary>
        private static int EnsureClientBootScene()
        {
            int changes = 0;

            if (!System.IO.File.Exists(ClientBootScenePath))
            {
                UnityEngine.SceneManagement.Scene scene =
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                GameObject root = new GameObject("ClientBoot", typeof(ClientBootLoader));
                scene.name = "ClientBoot";
                EditorSceneManager.SaveScene(scene, ClientBootScenePath);
                Debug.Log("[结构修复] 已新建客户端启动场景：" + ClientBootScenePath + "（对象 " + root.name + " + ClientBootLoader）。");
                changes++;
            }

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // 1) 确保 ClientShell 在清单里（若缺则追加）
            bool hasShell = false;
            for (int i = 0; i < scenes.Count; i++)
                if (scenes[i].path == ClientShellScenePath) hasShell = true;
            if (!hasShell)
            {
                scenes.Add(new EditorBuildSettingsScene(ClientShellScenePath, true));
                Debug.Log("[结构修复] 构建清单已加入 " + ClientShellScenePath + "。");
                changes++;
            }

            // 2) 确保 ClientBoot 在清单里，并置首
            int bootIndex = -1;
            for (int i = 0; i < scenes.Count; i++)
                if (scenes[i].path == ClientBootScenePath) bootIndex = i;
            if (bootIndex < 0)
            {
                scenes.Insert(0, new EditorBuildSettingsScene(ClientBootScenePath, true));
                Debug.Log("[结构修复] 构建清单已加入并置首 " + ClientBootScenePath + "。");
                changes++;
            }
            else if (bootIndex != 0)
            {
                EditorBuildSettingsScene boot = scenes[bootIndex];
                scenes.RemoveAt(bootIndex);
                scenes.Insert(0, boot);
                Debug.Log("[结构修复] 构建清单已把 " + ClientBootScenePath + " 移到第 0 位。");
                changes++;
            }

            if (changes > 0)
            {
                EditorBuildSettings.scenes = scenes.ToArray();
                StringBuilder order = new StringBuilder();
                for (int i = 0; i < scenes.Count && i < 5; i++)
                    order.Append(i == 0 ? "" : " → ").Append(System.IO.Path.GetFileNameWithoutExtension(scenes[i].path));
                Debug.Log("[结构修复] 构建场景顺序（前 5 个）：" + order);
            }

            return changes;
        }

        /// <summary>
        /// **待核验清理项落地（2026-09-21 负责人确认"清"）**：删除上一轮我为阵容弹窗新建的冗余节点
        /// `Player lineup/玩家阵容/Content`（含 3 张 `展示卡牌1..3`）。
        ///
        /// **安全护栏**：只有当该容器下的**全部子节点名都以 `展示卡牌` 开头**（即确实是我建的那一个）时才删除；
        /// 场景**既有**的 6 张 `角色卡牌Button-final` 位于别处，**不受影响**（代码对两种名字都收，删掉后仍能显示卡）。
        /// 幂等：找不到或护栏不通过即跳过。
        /// </summary>
        private static int RemoveRedundantLineupCards()
        {
            GameObject page = FindSceneObject("Player lineup", true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 清理阵容卡：未找到 Player lineup。");
                return 0;
            }

            // 直接在弹窗内找"**全部子节点名都以 `展示卡牌` 开头**的 Content" —— 那就是我 2026-09-21 建的那个。
            // （上一版沿 `玩家阵容/Content` 路径查找，任一环没命中就静默返回，导致清理未生效。）
            Transform[] all = page.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name != "Content" || all[i].childCount == 0)
                    continue;

                bool allShowcase = true;
                for (int c = 0; c < all[i].childCount; c++)
                {
                    if (!all[i].GetChild(c).name.StartsWith("Showcase", System.StringComparison.Ordinal) &&
                        !all[i].GetChild(c).name.StartsWith("展示卡牌", System.StringComparison.Ordinal))
                    {
                        allShowcase = false;
                        break;
                    }
                }
                if (!allShowcase)
                    continue;

                int removed = all[i].childCount;
                UnityEngine.Object.DestroyImmediate(all[i].gameObject);
                Debug.Log("[结构修复] 已删除冗余的阵容卡容器（含我新建的 " + removed +
                          " 张展示卡牌）；场景既有的 角色卡牌Button-final 不受影响。");
                return 1;
            }

            Debug.Log("[结构修复] 清理阵容卡：未找到\"全部子节点都是展示卡牌\"的 Content（可能已清理过）。");
            return 0;
        }

        /// <summary>
        /// **UI 适配（2026-09-21 逐页核查发现）**：`Pending shipmentCanvas` 的 `Bottom page function bar`
        /// **没有横向铺满**（`anchorMin.x` / `anchorMax.x` 不是 0/1）⇒ 在不同宽高比设备上底栏宽窄不一。
        /// 按商店同一套规则改为**底部横向铺满**：只改水平锚点与水平 offset，**高度与竖直位置保持不变**（不会跳位）。
        /// </summary>
        private static int NormalizePendingShipmentBar()
        {
            GameObject page = FindSceneObject("Pending shipmentCanvas", true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 Pending shipmentCanvas，跳过节庆底栏适配。");
                return 0;
            }

            Transform bar = FindDescendantByName(page.transform, "Bottom page function bar");
            RectTransform rect = bar as RectTransform;
            if (rect == null)
            {
                Debug.LogWarning("[结构修复] Pending shipmentCanvas 下没有 Bottom page function bar。");
                return 0;
            }

            if (rect.anchorMin.x == 0f && rect.anchorMax.x == 1f)
                return 0;

            rect.anchorMin = new Vector2(0f, rect.anchorMin.y);
            rect.anchorMax = new Vector2(1f, rect.anchorMax.y);
            rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
            rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
            Debug.Log("[结构修复] Pending shipmentCanvas 底栏已改为横向铺满（高度与竖直位置不变）。");
            return 1;
        }
        /// <summary>
        /// **UI 适配（2026-09-21 负责人反馈：窄高比设备上大厅背景左右黑边、两个底栏重叠）**。
        ///
        /// 实测根因（`ClientCanvas` 参考 753x1630、`matchWidthOrHeight = 0` ⇒ 画布宽恒 753、**高 = 753 x (H/W)**）：
        /// 1) `MainPage主页/Background` 虽是全屏锚点，但 `Image.m_PreserveAspect = 1` ⇒ 画布变矮（1080x2160 → 1506 单位、1080x1920 → 1339 单位）时按高缩放 ⇒ **比 753 窄 ⇒ 左右黑边**；
        /// 2) `CanvasBottomFunction0` 是**底锚**（y=0）而 `CanvasBottomFunction1` 是**中锚**（y=0.5、offset -417）⇒ 画布变矮时后者下压 ⇒ 高 1339 时与前者**重叠**。
        ///
        /// 修法（只改锚点与一个 bool，不动美术尺寸/位置）：
        /// 1) 背景 `preserveAspect = false`（铺满；若需要保持比例，改用 AspectRatioFitter.EnvelopeParent 做"覆盖"）；
        /// 2) `CanvasBottomFunction1` 改为**底锚**，offset 取设计高度（1630）下的等效值 398（= 1630/2 - 417）⇒ 两者都跟底边走，**间距恒定**。
        /// </summary>
        private static int NormalizeLobbyBackgroundAndBottomBars()
        {
            GameObject page = FindSceneObject("MainPage主页", true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 MainPage主页，跳过大厅底部适配。");
                return 0;
            }

            int changes = 0;

            // 1) 背景：取消保持比例（否则窄高比设备左右出现黑边）
            Transform background = FindChildObject(page.transform, "Background") != null
                ? FindChildObject(page.transform, "Background").transform : null;
            if (background != null)
            {
                Image image = background.GetComponent<Image>();
                if (image != null && image.preserveAspect)
                {
                    image.preserveAspect = false;
                    Debug.Log("[结构修复] 大厅 Background 已取消 PreserveAspect（改为铺满，消除窄高比设备的左右黑边）。");
                    changes++;
                }
            }

            // 2) CanvasBottomFunction1：中锚 -> 底锚，保持设计高度下的视觉位置
            Transform bar1 = FindChildObject(page.transform, "CanvasBottomFunction1") != null
                ? FindChildObject(page.transform, "CanvasBottomFunction1").transform : null;
            RectTransform rect = bar1 as RectTransform;
            if (rect != null && Mathf.Approximately(rect.anchorMin.y, 0.5f) && Mathf.Approximately(rect.anchorMax.y, 0.5f))
            {
                const float designHalfHeight = 1630f * 0.5f;   // 设计高度的一半
                float y = rect.anchoredPosition.y;
                rect.anchorMin = new Vector2(rect.anchorMin.x, 0f);
                rect.anchorMax = new Vector2(rect.anchorMax.x, 0f);
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, designHalfHeight + y);
                Debug.Log("[结构修复] CanvasBottomFunction1 已改为底锚定（offset " + y + " -> " + (designHalfHeight + y) +
                          "），与 CanvasBottomFunction0 间距恒定，不再随画布变矮而重叠。");
                changes++;
            }

            return changes;
        }
        /// <summary>主页展示列表的模板卡名（新建的隐藏节点）。</summary>
        private const string ShowcaseTemplateName = "展示卡Item";

        /// <summary>
        /// **ProfilePage 主页展示列表的模板卡**。
        ///
        /// 实测（2026-09-20 取证）：`ProfilePage个人中心/PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content`
        /// **是空容器**（0 个子节点；历史备份里也是 0）—— 审计文档里"该列表有 9 张英雄卡"的说法与实场景不符。
        /// 所以这里**新建 1 张隐藏模板卡**（`PrefabUtility.InstantiatePrefab` 克隆共用卡prefab，保留 prefab 关联），
        /// 由 `ClientHeroCardList` 在运行时按数据生成实例。
        ///
        /// 幂等：已有同名模板就只补接线与显隐。
        /// </summary>
        private static int EnsureProfileShowcase()
        {
            GameObject page = FindSceneObject("ProfilePage个人中心", true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 ProfilePage个人中心，跳过主页展示模板。");
                return 0;
            }

            const string containerPath = "ProfilePage个人中心/PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content";
            Transform container = FindByPath(containerPath);
            if (container == null)
                container = FindByPathTrimmed(containerPath);
            if (container == null)
            {
                Debug.LogWarning("[结构修复] 未找到主页展示列表容器，跳过。");
                return 0;
            }

            int changes = 0;
            Transform template = container.Find(ShowcaseTemplateName);

            if (template == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeroCardPrefabPath);
                if (prefab == null)
                {
                    Debug.LogWarning("[结构修复] 未找到卡牌 prefab：" + HeroCardPrefabPath + "，无法新建主页展示模板卡。");
                    return 0;
                }

                GameObject clone = PrefabUtility.InstantiatePrefab(prefab, container) as GameObject;
                if (clone == null)
                {
                    Debug.LogWarning("[结构修复] 实例化卡牌 prefab 失败，跳过主页展示模板卡。");
                    return 0;
                }

                clone.name = ShowcaseTemplateName;
                clone.SetActive(false);
                template = clone.transform;
                changes++;
                Debug.Log("[结构修复] 已在主页展示 Content 下新建隐藏模板卡「" + ShowcaseTemplateName +
                          "」（克隆自共用卡prefab，保留 prefab 关联）。");
            }
            else if (template.gameObject.activeSelf)
            {
                template.gameObject.SetActive(false);
                changes++;
            }

            // 幂等：补 ClientHeroCard / 卡牌名称节点 / 全部字段接线（与 HeroPage、编队选卡同一套）。
            changes += WireHeroCardOn(template);

            ClientProfilePage profile = page.GetComponent<ClientProfilePage>();
            if (profile != null)
            {
                WireObjectReference(profile, "_showcaseTemplate", template.gameObject);
                WireObjectReference(profile, "_showcaseRoot", container as RectTransform);
                Debug.Log("[结构修复] ClientProfilePage: _showcaseTemplate / _showcaseRoot 已接线。");
            }
            else
            {
                Debug.LogWarning("[结构修复] ProfilePage个人中心 上没有 ClientProfilePage，主页展示模板未接线。");
            }

            return changes;
        }
        /// <summary>
        /// <see cref="FindByPath"/> 的兜底版：**每段按 Trim 后的名字匹配**。
        /// 为什么需要：Unity 的 YAML 会用单引号包裹带首尾空格的节点名（实测 `m_Name: 'Panel_Personalize  '`，
        /// 真实名字尾部有 2 个空格）。把尾空格写进 C# 字面量极易被编辑器/工具吃掉，故用 Trim 匹配规避。
        /// </summary>
        private static Transform FindByPathTrimmed(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            string[] parts = path.Split('/');
            GameObject root = FindSceneObject(parts[0], true);
            if (root == null)
                return null;

            Transform current = root.transform;
            for (int i = 1; i < parts.Length; i++)
            {
                Transform next = current.Find(parts[i]);
                if (next == null)
                {
                    string want = parts[i].Trim();
                    for (int c = 0; c < current.childCount; c++)
                    {
                        Transform child = current.GetChild(c);
                        if (child.name.Trim() == want) { next = child; break; }
                    }
                }
                if (next == null)
                    return null;
                current = next;
            }
            return current;
        }

        private static Transform FindByPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            string[] parts = path.Split('/');
            GameObject root = FindSceneObject(parts[0], true);
            if (root == null)
                return null;

            Transform current = root.transform;
            for (int i = 1; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);
                if (current == null)
                    return null;
            }
            return current;
        }

        private static Toggle FindDescendantToggle(Transform root, string name)
        {
            if (root == null)
                return null;
            Toggle[] toggles = root.GetComponentsInChildren<Toggle>(true);
            for (int i = 0; i < toggles.Length; i++)
                if (toggles[i].name == name)
                    return toggles[i];
            return null;
        }

        // ------------------------------------------------------------------
        // 4. 安装 UI 内核组件（弹窗服务 / 组合根）
        // ------------------------------------------------------------------

        private static int EnsureKernelComponents(Canvas canvas)
        {
            int changes = 0;
            Transform popupLayer = canvas.transform.Find("PopupLayer");
            Transform systemLayer = canvas.transform.Find("SystemLayer");

            // 弹窗服务挂在 PopupLayer 上。
            if (popupLayer != null && popupLayer.GetComponent<ClientPopupService>() == null)
            {
                ClientPopupService service = popupLayer.gameObject.AddComponent<ClientPopupService>();
                WireObjectReference(service, "_popupLayer", popupLayer);
                // 遮罩槽位保持为空：场景中现有遮罩内嵌于各页面，属于页面自己的视觉，
                // 由负责人决定是否新建/指定 PopupLayer 专用遮罩。留空时服务不切换任何遮罩。
                Debug.Log("[结构修复] PopupLayer 已安装 ClientPopupService（遮罩槽位留空，待你指定或新建）。");
                changes++;
            }

            // 全局提示挂在 **ClientCanvas**（组合根，恒为激活）。
            //
            // ⚠️ 2026-09-20 修正：原先安装在 `SystemLayer`，但**该层在场景里恒为 `inactive`**
            //    （`加载Page` 的 activeSelf=1 但父层关着，且没有任何代码激活 SystemLayer）⇒ 两个后果：
            //    ① Toast 处于 inactive 层级，**显示不出来**；
            //    ② `ClientSystemFeedback` 自身也在 inactive 物体上，`StartCoroutine` 直接报
            //       "Coroutine couldn't be started because the game object is inactive"。
            //    ClientCanvas 恒激活，作为跨页面提示的宿主才成立。
            if (canvas.GetComponent<ClientSystemFeedback>() == null)
            {
                canvas.gameObject.AddComponent<ClientSystemFeedback>();
                Debug.Log("[结构修复] ClientCanvas 已安装 ClientSystemFeedback（全局提示宿主）。");
                changes++;
            }

            // 清理历史错误位置：SystemLayer 上遗留的 ClientSystemFeedback（该层恒 inactive，协程无法启动）。
            if (systemLayer != null)
            {
                ClientSystemFeedback stale = systemLayer.GetComponent<ClientSystemFeedback>();
                if (stale != null)
                {
                    Object.DestroyImmediate(stale, true);
                    Debug.Log("[结构修复] 已移除 SystemLayer 上失效的 ClientSystemFeedback" +
                              "（该层恒 inactive，Toast 显示不出且协程报错）。");
                    changes++;
                }
            }

            // 组合根挂在 ClientCanvas 上。
            if (canvas.GetComponent<ClientShellController>() == null)
            {
                canvas.gameObject.AddComponent<ClientShellController>();
                Debug.Log("[结构修复] ClientCanvas 已安装 ClientShellController（组合根）。");
                changes++;
            }

            // 诊断期：点击追踪器持久化到场景（与 ClientShellController 的运行时兜底一致）。
            if (canvas.GetComponent<ClientUiClickTracer>() == null)
            {
                canvas.gameObject.AddComponent<ClientUiClickTracer>();
                Debug.Log("[结构修复] ClientCanvas 已安装 ClientUiClickTracer（鼠标点击追踪，诊断用）。");
                changes++;
            }

            changes += RemovePopupMask(canvas);

            // ⚠️ 卡牌接线必须排在列表收敛**之前**：收敛会删掉多余内联卡、并把剩下的那张改名成
            // `编队英雄卡Item`，之后按原名 `角色卡牌Button-final*` 就一个都找不到（首次执行踩过：
            // 槽位卡接了 3 张、选卡模板 0 张）。
            changes += WireFormationCards();
            changes += ConvergeKnownLists();
            changes += ClearNestedCanvasOverride();
            changes += FixSuccessPageRect();
            changes += StretchFullScreenPopups();
            changes += NormalizeShopBars();
            changes += RemoveNestedCanvasScalers();
            changes += NormalizeCanvasScaler();
            changes += NormalizeAllPageBars();
            changes += RestorePlayerLineupRect();
            changes += NormalizeTopAreaElements();
            changes += NormalizePendingShipmentBar();   // UI 适配：Pending shipmentCanvas 底栏横向铺满
            changes += NormalizeLobbyBackgroundAndBottomBars();   // UI 适配：大厅背景铺满 + 底栏间距恒定
            changes += WireHeroCard();
            // 卡面名称节点迁移（负责人要求"取消名称底图、直接显示文字"，范围=全改）。
            // 放在 WireHeroCard 之后（此时卡已接好线）、CreateGuideSubPage 之前（复制子页时带上新结构）。
            changes += MigrateCardNameNode();
            // ProfilePage 主页展示列表：容器是空的 ⇒ 新建隐藏模板卡（负责人已授权写入）。
            changes += EnsureProfileShowcase();
            // BUG-018：玩家名 / 玩家ID / 战力（三者场景里都是 TMP，原代码用旧版 Text 所以接不上）。
            changes += WireProfileTexts();
            // BUG-020：购买数量进度条填充（_minimumText/_maximumText 按取证结论不接）。
            changes += WireShopProgress();
            // BUG-019 可做部分：按工程内实际贴图重建排行榜的品质 / 立绘查找表。
            changes += FillRankVisualTables();
            // BUG-026 ①：弹窗遮罩（方法早已写好但从未被调用 —— 场景里因此一直没有遮罩节点）。
            changes += EnsurePopupMask(canvas);
            // BUG-026 ②：大厅邮件入口红点（活动红点已接线，邮件红点缺节点）。
            changes += EnsureMailRedDot();
            // BUG-026 ④：BottomFunctionIconView._icon（_label 无对应节点，只接 icon）。
            changes += WireBottomFunctionIcons();
            // BUG-026 ⑥：玩家阵容弹窗 —— 右上角关闭按钮。
            // ⑤ 的"新建阵容卡"已按负责人 2026-09-21 的决定**取消调用**：场景既有的 6 张 `角色卡牌Button-final` 已足够，
            // 我上一轮新建的 3 张属冗余（`RemoveRedundantLineupCards()` 会把它删掉）。方法本身保留备用。
            changes += EnsureLineupCloseButton();
            // BUG-025 方案 C：客户端专用启动场景（新建并置首）。
            changes += EnsureClientBootScene();
            // 待核验清理项：删除我上一轮新建的冗余阵容卡容器（有护栏）。
            changes += RemoveRedundantLineupCards();
            changes += CreateGuideSubPage();
            changes += EnsureSystemToast();
            changes += WireHeroPageSubPages();
            changes += FixHeroCardGridAlignment();
            return changes;
        }

        // ------------------------------------------------------------------
        // 全局提示（Toast）—— 负责人 2026-09-20：“加一个视觉显示，只在日志弹太拉了”
        // ------------------------------------------------------------------

        private const string SystemToastRootName = "ToastRoot";
        private const string SystemToastLabelName = "ToastLabel";

        /// <summary>工程里提示专用的 TMP 字体（与其它提示同一套）。</summary>
        private const string SystemToastFontPath =
            "Assets/Resources_HotUpdate/Font/TextMeshPro/Tips SDF.asset";

        /// <summary>
        /// 在 `SystemLayer` 下确保存在**全局 Toast 节点**并接上 `ClientSystemFeedback`。
        ///
        /// 背景（2026-09-20 取证）：
        /// - `ClientSystemFeedback._toastRoot` / `_toastLabel` 一直是**空的** ⇒ `ShowToast()` 只打警告；
        /// - `ClientHomePage._notice` 也是 `fileID 0`（未接线）⇒ `ShowNotice()` 只进 Console；
        ///   两者叠加的结果就是负责人看到的“点了只在日志里有反应”。
        ///
        /// 视觉为**最小方案**（可随时按验收意见调）：居中偏下的深色半透明底 + 白金居中文本，
        /// 不参与射线（`raycastTarget=false`，不会挡住页面点击）；字体沿用 `Tips SDF`。
        /// **幂等**：节点已存在时只补接线，不重复新建。
        /// </summary>
        private static int EnsureSystemToast()
        {
            Canvas canvas = FindClientCanvas();
            if (canvas == null)
            {
                Debug.LogWarning("[结构修复] 未找到 ClientCanvas，跳过全局 Toast。");
                return 0;
            }

            int changes = 0;

            // 清理历史错误位置：SystemLayer 下的 ToastRoot —— 该层在场景里恒为 inactive，
            // 挂在那儿既显示不出来、也会让 StartCoroutine 报“game object is inactive”。
            Transform staleLayer = FindByPath("SystemLayer");
            if (staleLayer != null)
            {
                Transform staleToast = staleLayer.Find(SystemToastRootName);
                if (staleToast != null)
                {
                    Object.DestroyImmediate(staleToast.gameObject, true);
                    Debug.Log("[结构修复] 已移除 SystemLayer 下失效的 ToastRoot（该层恒 inactive）。");
                    changes++;
                }
            }

            Transform root = canvas.transform.Find(SystemToastRootName);
            if (root == null)
            {
                GameObject rootObject = new GameObject(SystemToastRootName,
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                RectTransform rootRect = rootObject.GetComponent<RectTransform>();
                rootRect.SetParent(canvas.transform, false);
                rootRect.anchorMin = new Vector2(0.5f, 0.5f);
                rootRect.anchorMax = new Vector2(0.5f, 0.5f);
                rootRect.pivot = new Vector2(0.5f, 0.5f);
                rootRect.sizeDelta = new Vector2(640f, 104f);
                rootRect.anchoredPosition = new Vector2(0f, -400f);

                Image background = rootObject.GetComponent<Image>();
                background.color = new Color(0f, 0f, 0f, 0.75f);
                background.raycastTarget = false;   // 提示不吞点击

                GameObject labelObject = new GameObject(SystemToastLabelName,
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.SetParent(rootObject.transform, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(24f, 12f);
                labelRect.offsetMax = new Vector2(-24f, -12f);

                TMPro.TextMeshProUGUI label = labelObject.GetComponent<TMPro.TextMeshProUGUI>();
                label.text = "提示";
                label.alignment = TMPro.TextAlignmentOptions.Center;
                label.fontSize = 30f;
                label.color = Color.white;
                label.raycastTarget = false;

                TMPro.TMP_FontAsset font =
                    UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(SystemToastFontPath);
                if (font != null)
                    label.font = font;
                else
                    Debug.LogWarning("[结构修复] 未找到提示字体，Toast 将用 TMP 默认字体：" + SystemToastFontPath);

                rootObject.SetActive(false);   // 默认隐藏，由 ShowToast 打开
                root = rootObject.transform;
                Debug.Log("[结构修复] ClientCanvas 下已新建全局提示节点 " + SystemToastRootName +
                          " + " + SystemToastLabelName + "（默认隐藏）。");
                changes++;
            }

            // 宿主组件装在组合根（ClientCanvas）上 —— 恒为激活，SetActive / StartCoroutine 都成立。
            ClientSystemFeedback feedback = canvas.GetComponent<ClientSystemFeedback>();
            if (feedback == null)
            {
                Debug.LogWarning("[结构修复] ClientCanvas 上没有 ClientSystemFeedback" +
                                 "（EnsureKernelComponents 本应先安装），无法接 Toast。");
                return changes;
            }

            WireObjectReference(feedback, "_toastRoot", root.gameObject);
            Transform labelNode = root.Find(SystemToastLabelName);
            WireObjectReference(feedback, "_toastLabel",
                labelNode != null ? labelNode.GetComponent<TMPro.TMP_Text>() : null);
            Debug.Log("[结构修复] ClientSystemFeedback：_toastRoot / _toastLabel 已接线（全局 Toast 可用）。");
            return changes + 1;
        }

        /// <summary>
        /// 卡牌网格对齐改为**左上**（负责人 2026-09-20）。
        /// 问题：只有 1 张卡时，卡片跑到中间而不是固定的第一格。
        /// 根因：`GridLayoutGroup.childAlignment` 默认为 `MiddleCenter` ——
        /// 子节点未填满时整组居中。背包式列表应为 `UpperLeft`，卡片永远落在固定格位。
        /// </summary>
        private static int FixHeroCardGridAlignment()
        {
            int changes = 0;
            string[] subPages = { "HeroSubPage", "GuideSubPage" };
            for (int i = 0; i < subPages.Length; i++)
            {
                Transform content = FindByPath("HeroPage英雄/" + subPages[i] +
                                                "/MiddleCanvas/Canvas/Scroll View/Viewport/Content");
                if (content == null)
                {
                    Debug.LogWarning("[结构修复] 未找到卡牌容器：" + subPages[i]);
                    continue;
                }

                GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
                if (grid == null)
                {
                    Debug.LogWarning("[结构修复] " + subPages[i] + " 的 Content 上没有 GridLayoutGroup。");
                    continue;
                }
                if (grid.childAlignment == TextAnchor.UpperLeft)
                    continue;

                Debug.Log("[结构修复] " + subPages[i] + " 卡牌网格对齐："
                          + grid.childAlignment + " → UpperLeft（卡片固定在左上第一格，不再居中）");
                grid.childAlignment = TextAnchor.UpperLeft;
                changes++;
            }
            return changes;
        }

        // ==================================================================
        // UI 适配 第 4 批（负责人 2026-09-22）：排行榜
        // ==================================================================

        /// <summary>
        /// **排行榜（#6）**：① 上方 UI 错位；② 两个 ranking 穿过下方 UI 底框。
        ///
        /// <para><b>取证（场景 YAML 实读 + 逐步坐标推算，见 `Logs/rank-layout.ps1`）</b>：
        /// 排行榜页里放着**两份**列表容器 —— `Challenge List`（挑战榜）与 `Battle Power Ranking`（战力榜），
        /// 两者位置完全相同（`rank` 均为 y `209.4~1062.9`，底栏在 y `0~149`），由底部两个按钮切换显隐。
        /// 两份列表的 Viewport / Content 配置**不一致**，这才是两个症状的根因：</para>
        /// <list type="number">
        /// <item>`Battle Power Ranking/rank/邮件Scroll View/Viewport` 的锚点是 **`(0,0)-(0,0)` + sizeDelta `(0,0)`**
        /// ⇒ **视口塌成 0×0**，`RectMask2D` 等于不存在 ⇒ 内容不被裁切、按自己的位置画到框外
        /// （"穿过下方 UI 底框"）。对照：挑战榜那份是正确的 `(0,0)-(1,1)`。</item>
        /// <item>两份的 `Content` 都是 **中锚 + offset x 375.6 / 383.2**，而 pivot 是 `(0,1)`
        /// ⇒ 配合 `ClientScrollFix` 把 y 改成贴顶后，**横向被推出去**（"上方 UI 错位"）。</item>
        /// </list>
        ///
        /// <para><b>本方法</b>把两份列表都规范成项目里已验证的竖向列表配置（幂等）：</para>
        /// <list type="bullet">
        /// <item>Viewport：贴满 Scroll View（`(0,0)-(1,1)`、offset 归零、pivot `(0,1)`）—— 让遮罩真正生效；</item>
        /// <item>Content：贴左上（锚 `(0,1)`、pivot `(0,1)`、offset 归零）—— 列从视口左上开始；</item>
        /// <item>GridLayoutGroup：`childAlignment = UpperLeft`（行固定在左上，不再随数量居中）。</item>
        /// </list>
        /// 不动尺寸、不动行高、不动底栏。
        /// </summary>
        private static int FixRankPageLists()
        {
            int changes = 0;
            string[] lists =
            {
                "RankPage排行榜/Challenge List/rank/邮件Scroll View",
                "RankPage排行榜/Battle Power Ranking/rank/邮件Scroll View",
            };

            for (int i = 0; i < lists.Length; i++)
            {
                Transform scroll = FindByPath(lists[i]);
                if (scroll == null)
                {
                    Debug.LogWarning("[结构修复] 未找到排行榜列表：" + lists[i]);
                    continue;
                }

                // ① Viewport 必须贴满 Scroll View，遮罩才有面积。
                RectTransform viewport = FindDirectChild(scroll, "Viewport") as RectTransform;
                if (viewport == null)
                {
                    Debug.LogWarning("[结构修复] " + lists[i] + " 下没有 Viewport。");
                }
                else
                {
                    Vector2 zero = Vector2.zero;
                    // ⚠️ 必须连 sizeDelta 一起判：只比锚点会让 `(0,0)-(1,1)` + size `(-17,0)` 这种
                    // "锚点对了但尺寸还差 17" 的组合被判成"已贴满"而跳过（2026-09-22 实测踩到，
                    // 挑战榜 Viewport 因此一直留着 -17）。
                    bool stretched = viewport.anchorMin == zero && viewport.anchorMax == Vector2.one &&
                                     viewport.sizeDelta == zero && viewport.anchoredPosition == zero;
                    if (!stretched)
                    {
                        Debug.Log("[结构修复] " + lists[i] + "/Viewport 锚 " + viewport.anchorMin + "-" + viewport.anchorMax +
                                  " size " + viewport.sizeDelta + " → 贴满父级（原锚点使视口塌成 0×0，遮罩失效、内容画到框外）");
                        viewport.anchorMin = zero;
                        viewport.anchorMax = Vector2.one;
                        viewport.anchoredPosition = Vector2.zero;
                        viewport.sizeDelta = zero;
                        changes++;
                    }
                    if (viewport.pivot != new Vector2(0f, 1f))
                    {
                        viewport.pivot = new Vector2(0f, 1f);
                        changes++;
                    }
                }

                // ② Content 贴左上（ScrollRect.m_Content 指向的就是它）。
                RectTransform content = FindDirectChild(viewport, "Content") as RectTransform;
                if (content == null && viewport != null)
                    content = FindByPath(lists[i] + "/Viewport/Content") as RectTransform;
                if (content == null)
                {
                    Debug.LogWarning("[结构修复] " + lists[i] + " 下没有 Content。");
                    continue;
                }

                if (!Mathf.Approximately(content.anchorMin.x, 0f) || !Mathf.Approximately(content.anchorMax.x, 0f) ||
                    !Mathf.Approximately(content.anchorMin.y, 1f) || !Mathf.Approximately(content.anchorMax.y, 1f))
                {
                    Debug.Log("[结构修复] " + lists[i] + "/Content 锚 " + content.anchorMin + "-" + content.anchorMax +
                              " → 左上锚 (0,1)（原中锚 + pivot(0,1) 把整列横向推出去）");
                    content.anchorMin = new Vector2(0f, 1f);
                    content.anchorMax = new Vector2(0f, 1f);
                    changes++;
                }
                Vector2 targetPivot = new Vector2(0f, 1f);
                if (content.pivot != targetPivot)
                {
                    content.pivot = targetPivot;
                    changes++;
                }
                if (content.anchoredPosition != Vector2.zero)
                {
                    Debug.Log("[结构修复] " + lists[i] + "/Content offset " + content.anchoredPosition + " → (0,0)");
                    content.anchoredPosition = Vector2.zero;
                    changes++;
                }

                // ③ 网格对齐左上（只改竖直分量之外的对齐，不动 CellSize/Spacing）。
                GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
                if (grid != null && grid.childAlignment != TextAnchor.UpperLeft)
                {
                    Debug.Log("[结构修复] " + lists[i] + "/Content 网格对齐 " + grid.childAlignment + " → UpperLeft");
                    grid.childAlignment = TextAnchor.UpperLeft;
                    changes++;
                }
            }
            return changes;
        }

        /// <summary>
        /// 按"父节点路径 + 直接子节点的名字片段"定位节点，再统一成贴顶锚。
        ///
        /// <para>**为什么要按片段找**：排行榜左上角标题的 GameObject 名字**字面含斜杠** ——
        /// `Challange/Ballte Mall`（不是层级路径！）。`Transform.Find("Challange/Ballte Mall")`
        /// 会被当成两级路径 ⇒ 永远找不到。2026-09-22 实测踩到。</para>
        /// </summary>
        private static int FixRankPageStableAnchorOnChild(string parentPath, string childNameFragment,
                                                          string label, float designHeight)
        {
            Transform parent = FindByPath(parentPath);
            if (parent == null)
            {
                Debug.LogWarning("[结构修复] 未找到父节点：" + parentPath);
                return 0;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name.IndexOf(childNameFragment, System.StringComparison.Ordinal) < 0)
                    continue;
                return AnchorVerticallyTopStable(child as RectTransform, label, designHeight);
            }

            Debug.LogWarning("[结构修复] " + parentPath + " 下找不到名字含 '" + childNameFragment + "' 的子节点。");
            return 0;
        }

        /// <summary>
        /// 把节点统一成**贴顶锚**，并按设计基准（1630 高）回写等效 offset —— **保证 752×1630 下位置不变**。
        ///
        /// <para>方向 A 的核心规则（`UI_ANCHOR_SIGNOFF.md` §三）：画布宽恒 753、**高随比例变**：
        /// 1080×2280→1590、1080×2160→1506、1080×1920→**1339**。
        /// **中锚**元素的纵向位置 = `画布高/2 + offset`，画布一变矮就整体下移 ⇒ 与固定内容重叠。
        /// 贴顶锚则是 `画布高 - offset`，**在矮画布上纹丝不动**。</para>
        ///
        /// <para>本次用途（负责人 2026-09-22 截图反馈）：排行榜页的
        /// `Challange/Ballte Mall`、`Explanation text frame`、两份 `tips`、两个榜单容器
        /// 都还是中锚 —— 1339 上整体下移 145.5 单位，"挑战榜标题向上出框 / 说明框与未上榜位置不对"。</para>
        ///
        /// <para>只改**锚点类型**与等效 offset，不动尺寸、不动父级、不动渲染顺序。</para>
        /// </summary>
        private static int AnchorVerticallyTopStable(RectTransform rect, string label, float designHeight)
        {
            if (rect == null)
                return 0;

            // 已是贴顶锚：位置交给负责人手调，这里不碰。
            if (Mathf.Approximately(rect.anchorMin.y, 1f) && Mathf.Approximately(rect.anchorMax.y, 1f))
                return 0;

            float centerY = rect.anchorMin.y * designHeight + rect.anchoredPosition.y;
            Vector2 target = new Vector2(rect.anchoredPosition.x, centerY - designHeight);

            Debug.Log("[结构修复] " + label + "：中锚 pos " + rect.anchoredPosition + " → 贴顶锚 pos " + target +
                      "（等效于 1630 基准下中心 y=" + centerY.ToString("F1") + "，矮画布上不再下移）");
            rect.anchorMin = new Vector2(rect.anchorMin.x, 1f);
            rect.anchorMax = new Vector2(rect.anchorMax.x, 1f);
            rect.anchoredPosition = target;
            return 1;
        }

        /// <summary>
        /// **排行榜页"随高度漂移"的元素统一锚点**（负责人 2026-09-22 第二轮反馈）：
        /// `Challange/Ballte Mall` 向上出框、`Explanation text frame` 与 `tips` 位置不对。
        ///
        /// <para>根因：这 4 个节点都是**中锚**，纵向位置 = `画布高/2 + offset`。
        /// 1080×1920 时画布只有 1339（设计 1630）⇒ 它们比设计位置**下移 145.5 单位**，
        /// 于是标题被顶出框、说明框/未上榜贴到卡片上。</para>
        ///
        /// <para>修法：全部改**贴顶锚**（含两个榜单容器），offset 用 1630 基准回写 ⇒ 设计比例下位置不变，
        /// 矮画布上不再漂移。**不动尺寸/渲染顺序**；`tips` 在层级上已排在两个榜单之前，
        /// 改为贴顶后落到顶部区域，与卡片列表不再重叠。</para>
        /// </summary>
        private static int FixRankPageStableAnchors()
        {
            const float DesignHeight = 1630f;
            int changes = 0;

            changes += FixRankPageStableAnchorOnChild(
                "RankPage排行榜", "Ballte Mall", "排行榜/左上角标题 Challange/Ballte Mall", DesignHeight);
            changes += AnchorVerticallyTopStable(
                FindByPath("RankPage排行榜/Explanation text frame") as RectTransform,
                "排行榜/说明文字框 Explanation text frame", DesignHeight);

            string[] tipPaths =
            {
                "RankPage排行榜/Challenge List/tips",
                "RankPage排行榜/Battle Power Ranking/tips",
            };
            for (int i = 0; i < tipPaths.Length; i++)
                changes += AnchorVerticallyTopStable(FindByPath(tipPaths[i]) as RectTransform,
                                                     "排行榜/" + tipPaths[i].Substring(tipPaths[i].IndexOf('/') + 1), DesignHeight);

            string[] listPaths =
            {
                "RankPage排行榜/Challenge List",
                "RankPage排行榜/Battle Power Ranking",
            };
            for (int i = 0; i < listPaths.Length; i++)
                changes += AnchorVerticallyTopStable(FindByPath(listPaths[i]) as RectTransform, listPaths[i], DesignHeight);

            return changes;
        }

        // ==================================================================
        // UI 适配 第 1+2 批（负责人 2026-09-21 的 11 项反馈）
        // ==================================================================

        /// <summary>
        /// **第 1 批（P0 回归）**：英雄详情 `天赋 / 秘技 / 终结技` 三个页签的**选中态皮肤**接上美术。
        ///
        /// <para><b>取证结论（不是猜的）</b>：节点与事件本来就完好 —— 三个 Toggle 都 `active=1`、
        /// 各有 1 条持久 `SetActive` 调用（目标 fileID 天赋 607240859 / 秘技 1744901391 / 终结技 859971019）、
        /// 目标页 `downCanvas/{天赋页,秘技页,终结技页}` 都在。**坏的只有美术引用**：</para>
        /// <list type="bullet">
        /// <item>`Assets/Client/UI/HeroDetailPage/Sprites/技能区/{天赋,秘技,终结技}选中框.png`
        /// 在**全工程零引用**（`grep` 过 4 个 guid）；</item>
        /// <item>三个页签的 `Background` / `Checkmark` 上挂的贴图与名字对不上
        /// （实测 `天赋Toggle/Background` = 秘技选中框、`秘技Toggle/Background` 的 Image 还是 `m_Enabled: 0`）；</item>
        /// <item>负责人原话：*"点击一个时 3 个的字体贴图和底框都要变"*。</item>
        /// </list>
        ///
        /// <para><b>本方法的修法（只写引用与尺寸，不动锚点/父级/层级）</b>：</para>
        /// <list type="number">
        /// <item>`Background` = 「选中框」贴图（`*选中框.png`）⇒ 它只在该页签选中时显示；</item>
        /// <item>`Checkmark` = 「文字贴图」（`天赋.png` 等）⇒ **常态恒显**，尺寸按贴图原始宽高比落到页签里；</item>
        /// <item>补挂 `ClientHeroDetailToggleSkin` 并把两组 Image 绑上去（runtime 按选中段切显隐）；</item>
        /// <item>`Toggle.graphic` 保持置空（由皮肤组件控），避免 Unity 把未选中页签的文字 alpha 置 0。</item>
        /// </list>
        /// 幂等：贴图/尺寸/绑定都已是目标值时返回 0。
        /// </summary>
        private static int FixHeroDetailSkillTabs()
        {
            GameObject page = FindSceneObject(CurrentHeroDetailNode, true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 " + CurrentHeroDetailNode + "，跳过技能页签皮肤。");
                return 0;
            }

            // 1) 选中框贴图（底框；未选中态没有独立美术，运行时把 sprite 置空只留文字）
            string[] framePaths =
            {
                "Assets/Client/UI/HeroDetailPage/Sprites/技能区/天赋选中框.png",
                "Assets/Client/UI/HeroDetailPage/Sprites/技能区/秘技选中框.png",
                "Assets/Client/UI/HeroDetailPage/Sprites/技能区/终结技选中框.png",
            };
            // 2) 常态文字贴图（负责人：这两个字是**图**，不是 TMP 文本）
            string[] labelPaths =
            {
                "Assets/Client/UI/HeroDetailPage/Sprites/技能区/天赋.png",
                "Assets/Client/UI/HeroDetailPage/Sprites/技能区/秘技.png",
                "Assets/Client/UI/HeroDetailPage/Sprites/技能区/终结技.png",
            };
            string[] toggleNames = { "天赋Toggle", "秘技Toggle", "终结技Toggle" };

            RectTransform group = FindByPath(CurrentHeroDetailNode + "/downCanvas/Panel/group") as RectTransform;
            if (group == null)
            {
                Debug.LogWarning("[结构修复] 未找到 " + CurrentHeroDetailNode + "/downCanvas/Panel/group，跳过技能页签皮肤。");
                return 0;
            }

            ClientHeroDetailToggleSkin skin = page.GetComponent<ClientHeroDetailToggleSkin>();
            bool skinWasMissing = skin == null;
            int changes = 0;
            if (skinWasMissing)
            {
                skin = page.AddComponent<ClientHeroDetailToggleSkin>();
                Debug.Log("[结构修复] " + CurrentHeroDetailNode + " 已补挂 ClientHeroDetailToggleSkin（技能页签选中态皮肤）。");
                changes++;
            }

            for (int i = 0; i < toggleNames.Length; i++)
            {
                Transform toggleNode = FindDirectChild(group, toggleNames[i]);
                if (toggleNode == null)
                {
                    Debug.LogWarning("[结构修复] group 下未找到 " + toggleNames[i] + "，跳过。");
                    continue;
                }

                Toggle toggle = toggleNode.GetComponent<Toggle>();
                RectTransform frameRect = FindDirectChild(toggleNode, "Background") as RectTransform;
                // ⚠️ 实测层级是 `Toggle/Background/Checkmark`：Checkmark 是 **Background 的子节点**。
                //    按 Toggle 的直接子节点找它永远找不到 —— 这正是"文字贴图一直不显示"的根因。
                RectTransform labelRect = FindDirectChild(toggleNode, "Checkmark") as RectTransform;
                if (labelRect == null && frameRect != null)
                {
                    labelRect = FindDirectChild(frameRect, "Checkmark") as RectTransform;
                    if (labelRect != null)
                        Debug.Log("[结构修复] " + toggleNames[i] + "：Checkmark 在 Background 之下（实测层级），已按该层级接线。");
                }

                Sprite frame = AssetDatabase.LoadAssetAtPath<Sprite>(framePaths[i]);
                Sprite label = AssetDatabase.LoadAssetAtPath<Sprite>(labelPaths[i]);
                if (frame == null || label == null)
                {
                    Debug.LogWarning("[结构修复] 技能页签贴图缺失：" + framePaths[i] + " / " + labelPaths[i] + "，跳过 " + toggleNames[i] + "。");
                    continue;
                }

                changes += SetImageSprite(frameRect, frame, toggleNames[i] + "/Background");
                changes += SetImageSprite(labelRect, label, toggleNames[i] + "/Checkmark");
                changes += EnsureLabelRect(labelRect, label, toggleNames[i] + "/Checkmark");

                // 3) Toggle 的 graphic 交给皮肤组件，不交给 Unity 自动隐藏。
                if (toggle != null)
                {
                    SerializedObject toggleSo = new SerializedObject(toggle);
                    SerializedProperty graphic = toggleSo.FindProperty("m_Graphic");
                    if (graphic == null)
                        graphic = toggleSo.FindProperty("graphic");
                    if (graphic != null && graphic.objectReferenceValue != null)
                    {
                        graphic.objectReferenceValue = null;
                        toggleSo.ApplyModifiedPropertiesWithoutUndo();
                        Debug.Log("[结构修复] " + toggleNames[i] + ".graphic 置空（改由 ClientHeroDetailToggleSkin 控显隐，" +
                                  "否则未选中页签的文字贴图会被 Toggle 把 alpha 置 0）。");
                        changes++;
                    }

                    // 4) 绑定皮肤（幂等：同一 Toggle 原地覆盖；已是目标值则不计数）。
                    Image frameImage = frameRect != null ? frameRect.GetComponent<Image>() : null;
                    Image labelImage = labelRect != null ? labelRect.GetComponent<Image>() : null;
                    if (!skin.Matches(i, toggle, frameImage, labelImage, frame))
                    {
                        skin.Bind(toggle, frameImage, labelImage, frame);
                        Debug.Log("[结构修复] " + toggleNames[i] + " 已绑定皮肤（底框 + 文字贴图）。");
                        changes++;
                    }
                }
            }

            // 5) 只在**首次补挂组件**时初始化一次显隐（天赋为默认页）。
            //    之后每次 RepairAll 都不覆盖运行时/负责人的当前选中态。
            if (skinWasMissing)
            {
                skin.Apply(0);
                EditorUtility.SetDirty(skin);
            }
            return changes;
        }

        /// <summary>把 Image 的 sprite 换成目标贴图；已是目标值则返回 0。</summary>
        private static int SetImageSprite(RectTransform rect, Sprite sprite, string label)
        {
            if (rect == null || sprite == null)
                return 0;

            Image image = rect.GetComponent<Image>();
            if (image == null)
            {
                Debug.LogWarning("[结构修复] " + label + " 上没有 Image，跳过贴图接线。");
                return 0;
            }

            int changes = 0;
            if (image.sprite != sprite)
            {
                image.sprite = sprite;
                Debug.Log("[结构修复] " + label + " 贴图 → " + sprite.name);
                changes++;
            }
            // 秘技Toggle/Background 的 Image 实测是 m_Enabled: 0（这也是它"看不见"的另一半原因）。
            if (!image.enabled)
            {
                image.enabled = true;
                Debug.Log("[结构修复] " + label + " 的 Image 由 Disabled 恢复 Enabled。");
                changes++;
            }
            return changes;
        }

        /// <summary>
        /// 文字贴图按**贴图原始宽高比**落到页签里（只改 sizeDelta，保持节点中心不动）。
        ///
        /// 为什么必须做：文字贴图是 `156x45 / 88x48 / 222x45` 这种很扁的图，
        /// 而场景里 `Checkmark` 的 rect 是 `~215x76`（宽高比 2.8 且三者还不一致）⇒
        /// 直接换图会被拉扁/拉长。这里按"高度取页签高的 40%、宽度按原始比例"统一，避免变形。
        /// </summary>
        private static int EnsureLabelRect(RectTransform rect, Sprite sprite, string label)
        {
            if (rect == null || sprite == null)
                return 0;

            float aspect = sprite.rect.height > 0f ? sprite.rect.width / sprite.rect.height : 1f;
            float height = 46f;                       // 约页签高 115 的 40%，与《参考.png》里文字占比接近
            Vector2 target = new Vector2(Mathf.Round(height * aspect), height);

            int changes = 0;
            if (Vector2.Distance(rect.sizeDelta, target) >= 0.5f)
            {
                Debug.Log("[结构修复] " + label + " 尺寸 " + rect.sizeDelta + " → " + target +
                          "（按贴图原始比例 " + aspect.ToString("F2") + "，避免换图后被拉扁）");
                rect.sizeDelta = target;
                changes++;
            }

            // 居中落在页签里（原场景 3 个 Checkmark 的 offset/宽高比各不相同：-12.1 / 0 / 1.1）。
            Vector2 center = new Vector2(0.5f, 0.5f);
            if (rect.pivot != center || Vector2.Distance(rect.anchoredPosition, Vector2.zero) > 0.5f)
            {
                rect.pivot = center;
                rect.anchoredPosition = Vector2.zero;
                changes++;
            }
            return changes;
        }

        /// <summary>
        /// **第 2 批 #1**：英雄详情 `后退Button` 的**半边被遮**（1080×1920 实测下缘 −35、被底栏压住）。
        ///
        /// <para>根因：它是**中锚** `(0.5,0.5)` + offset `(-286,-674)`。
        /// 画布高 1339 时中心在 669.5 ⇒ 按钮落在 `y = -35~26`（屏幕最底、被 `下功能底框`/手机底边压住）；
        /// 而设计高 1630 时在 `110~171`。**同一套 offset 在矮画布上必然沉底** —— 典型"中锚 + 固定纵向位置"。</para>
        ///
        /// <para>修法（方向 A：统一锚点类型 + 等效 offset 回写不跳位）：改成**左上锚** `(0,1)`，
        /// offset 取设计基准（1630 高）下的实测等效值 `(44.5, -1603.5)`。
        /// 这样任何画布高度下按钮都稳在左上角同一位置，且与"贴顶"的同类元素同一锚点系。</para>
        /// </summary>
        private static int FixHeroDetailBackButtonAnchor()
        {
            RectTransform rect = FindByPath(CurrentHeroDetailNode + "/后退Button") as RectTransform;
            if (rect == null)
            {
                Debug.LogWarning("[结构修复] 未找到 " + CurrentHeroDetailNode + "/后退Button，跳过锚点修正。");
                return 0;
            }

            // pivot 已实测 = (0.5,0.5)：保持"中心锚点"的视觉中心，只换锚点类型。
            // 设计基准 1630 下的实测中心 = (-259.5, 141.5)（left → canvas x = 753/2 - 286 = 90.5，中心 x = 90.5-46=44.5）
            // 左上锚 (0,1) 时：anchoredPosition = (中心x - 半个宽, -(1630 - 中心y) + 半个高) = (44.5 - 46, -(1630-141.5) + 30.5)
            Vector2 targetMin = new Vector2(0f, 1f);
            Vector2 targetMax = new Vector2(0f, 1f);
            // 设计基准 1630 下的实测中心：左边界 90.5（=753/2−286），按钮 92x61 ⇒ 中心 (44.5, 141.5)。
            // 左上锚 (0,1) + pivot(0.5,0.5)、父级全屏 ⇒ anchoredPosition = (44.5, 141.5−1630) = (44.5, −1488.5)。
            Vector2 targetPos = new Vector2(44.5f, 141.5f - 1630f);

            if (rect.anchorMin == targetMin && rect.anchorMax == targetMax &&
                Vector2.Distance(rect.anchoredPosition, targetPos) < 0.5f)
                return 0;

            Debug.Log("[结构修复] " + CurrentHeroDetailNode + "/后退Button 锚点 " +
                      rect.anchorMin + "-" + rect.anchorMax + " pos " + rect.anchoredPosition +
                      " → 左上锚 " + targetMin + " pos " + targetPos +
                      "（原为中锚，1080×1920 下会沉到屏幕外/被底栏压住）");
            rect.anchorMin = targetMin;
            rect.anchorMax = targetMax;
            rect.anchoredPosition = targetPos;
            return 1;
        }

        /// <summary>
        /// **第 2 批 #2**：`downCanvas/leftmiddleCanvas`（等级/突破/前往获取那张卡）**下移到 downCanvas 顶部**
        /// —— 负责人 2026-09-21 选定方案。
        ///
        /// <para>实测：它现在是**中锚** offset `(231, 510.71857)`、尺寸 `301.33x226.85` ⇒
        /// 在 1339 画布上落在 `y = 730.6~957.4`、`x = 456.8~758.2`，
        /// 与 `upCanvas/right`（`y = 787~863`、`x = 680.5~752.5`）重叠 ⇒ 负责人反馈的
        /// "leftmiddleCanvas 上移挡住 upCanvas/right"。</para>
        ///
        /// <para>修法：改为**贴 downCanvas 顶部**：anchor `(0.5,1)`、pivot `(0.5,1)`、
        /// `anchoredPosition = (231, 0)`。这样它与 `downCanvas` 的相对位置**恒定**，
        /// 任何画布高度下都紧贴下区顶部，不再随高度漂移去压上半区。</para>
        ///
        /// <para>联动：`downCanvas/Background`（属性条，中锚 offset `(0,296.6)`）下移到卡片下方，
        /// 保持"同区、不重叠"（负责人原话：*和属性条同一区*）。</para>
        /// </summary>
        private static int FixHeroDetailLeftMiddleCanvas()
        {
            RectTransform card = FindByPath(CurrentHeroDetailNode + "/downCanvas/leftmiddleCanvas") as RectTransform;
            if (card == null)
            {
                Debug.LogWarning("[结构修复] 未找到 downCanvas/leftmiddleCanvas，跳过。");
                return 0;
            }

            int changes = 0;

            // 1) 卡片：贴 downCanvas 顶部（卡片高 226.85 ⇒ 占 y = downTop-226.85 .. downTop）
            Vector2 cardMin = new Vector2(0.5f, 1f);
            Vector2 cardPivot = new Vector2(0.5f, 1f);
            Vector2 cardPos = new Vector2(231f, 0f);
            if (card.anchorMin != cardMin || card.anchorMax != cardMin || card.pivot != cardPivot ||
                Vector2.Distance(card.anchoredPosition, cardPos) > 0.5f)
            {
                Debug.Log("[结构修复] downCanvas/leftmiddleCanvas：中锚 pos(231,510.7) → 贴顶锚 pos(231,0)，" +
                          "下移到 downCanvas 顶部、不再压 upCanvas/right。");
                card.anchorMin = cardMin;
                card.anchorMax = cardMin;
                card.pivot = cardPivot;
                card.anchoredPosition = cardPos;
                changes++;
            }

            // 2) 属性条：放到卡片下方，保持同区不重叠。
            //    downCanvas 高 666.5629；卡片占 [439.71, 666.56]（相对 downCanvas 底）。
            //    属性条高 111 ⇒ 中心 y 应 ≈ 439.71 - 6 - 55.5 = 378.2 ⇒ 相对 downCanvas 中心 (333.28) 的 offset = 44.9
            RectTransform bar = FindByPath(CurrentHeroDetailNode + "/downCanvas/Background") as RectTransform;
            if (bar != null)
            {
                Vector2 barPos = new Vector2(0f, 44.9f);
                if (Vector2.Distance(bar.anchoredPosition, barPos) > 0.5f)
                {
                    Debug.Log("[结构修复] downCanvas/Background（属性条）pos " + bar.anchoredPosition +
                              " → " + barPos + "（让到卡片下方，与卡片同区但不重叠）");
                    bar.anchoredPosition = barPos;
                    changes++;
                }
            }

            return changes;
        }

        /// <summary>
        /// **第 2 批 #3**：英雄页卡牌列表 `Content` 跑到**左上角并与「弹珠」重叠**。
        ///
        /// <para>实测（场景 YAML 实读 + 绝对坐标推算，`Logs/ui-absrects.md`）：
        /// `HeroPage英雄/HeroSubPage/MiddleCanvas/Canvas/Scroll View/Viewport/Content`
        /// 的锚点是**中锚** `(0.5,0.5)`、pivot `(0,1)`、offset `(-298.995, 503)`、
        /// sizeDelta.x = 516.3 ⇒ 左边缘算到视口左界再减 299 ⇒ **整列卡牌跑到滚动框左侧外面**。</para>
        ///
        /// <para>修法（方向 A，只动相关轴、不跳位）：锚点改**左上** `(0,1)-(0,1)`、pivot 保持 `(0,1)`、
        /// offset 归零 ⇒ 内容左边缘 = 视口左边缘，首行贴视口顶部。
        /// 这同时让 `ScrollRect`（H+V + Elastic）能正确夹紧到顶部，`ClientScrollFix` 之后也只改竖直轴。</para>
        ///
        /// <para>`HeroSubPage` 与 `GuideSubPage` 各有一份，**两个都改**。</para>
        /// </summary>
        private static int FixHeroPageCardListGeometry()
        {
            int changes = 0;
            string[] subPages = { "HeroSubPage", "GuideSubPage" };
            for (int i = 0; i < subPages.Length; i++)
            {
                string path = "HeroPage英雄/" + subPages[i] + "/MiddleCanvas/Canvas/Scroll View/Viewport/Content";
                RectTransform content = FindByPath(path) as RectTransform;
                if (content == null)
                {
                    Debug.LogWarning("[结构修复] 未找到卡牌容器：" + path);
                    continue;
                }

                Vector2 min = new Vector2(0f, 1f);
                Vector2 pos = Vector2.zero;
                if (content.anchorMin == min && content.anchorMax == min &&
                    Vector2.Distance(content.anchoredPosition, pos) < 0.5f &&
                    Mathf.Approximately(content.pivot.x, 0f) && Mathf.Approximately(content.pivot.y, 1f))
                    continue;

                Debug.Log("[结构修复] " + subPages[i] + " 卡牌 Content：锚 " + content.anchorMin + "-" + content.anchorMax +
                          " pivot " + content.pivot + " pos " + content.anchoredPosition +
                          " → 左上锚 (0,1) pivot(0,1) pos(0,0)（原中锚使整列卡牌落到滚动框左侧外面）");
                content.anchorMin = min;
                content.anchorMax = min;
                content.pivot = new Vector2(0f, 1f);
                content.anchoredPosition = pos;
                changes++;
            }
            return changes;
        }

        /// <summary>按 "根节点/子/孙" 逐级精确名查找（`Transform.Find` 版本，找不到返回 null）。</summary>
        private static Transform FindDirectChild(Transform parent, string name)
        {
            if (parent == null || string.IsNullOrEmpty(name))
                return null;
            return parent.Find(name);
        }

        /// <summary>
        /// 第 4 步场景侧：给 `ClientHeroPage` 接线两个子页与页签按钮。
        ///
        /// 不做的事（有意）：
        /// - **不接 `_heroTabSelected` / `_guideTabSelected`**：那是承载负责人自定义 animation
        ///   状态的节点，由他在 Inspector 里拖入；代码只负责按需显隐。
        /// - **不接进度文本**：`已收集进度` / `总英雄数量` 的实际组件类型未知（TMP 还是旧版 Text），
        ///   按 AGENTS.md"UI 文本引用必须按场景实际组件类型声明"，这里先**只报告类型**，不盲接。
        /// </summary>
        private static int WireHeroPageSubPages()
        {
            int changes = 0;

            MonoBehaviour page = null;
            MonoBehaviour[] all = Object.FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].GetType().Name == "ClientHeroPage")
                {
                    page = all[i];
                    break;
                }
            }
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 ClientHeroPage 组件，跳过子页接线。");
                return 0;
            }

            Transform heroPage = FindByPath("HeroPage英雄");
            if (heroPage == null)
                return 0;

            Transform heroSub = heroPage.Find("HeroSubPage");
            Transform guideSub = heroPage.Find("GuideSubPage");
            if (heroSub != null) { WireObjectReference(page, "_heroSubPage", heroSub.gameObject); changes++; }
            if (guideSub != null) { WireObjectReference(page, "_guideSubPage", guideSub.gameObject); changes++; }

            // 页签按钮：在 downCanvas 下按名字找（含"英雄"/"图鉴"）
            Transform down = heroPage.Find("downCanvas");
            if (down != null)
            {
                Button[] buttons = down.GetComponentsInChildren<Button>(true);
                Button heroTab = null;
                Button guideTab = null;
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] == null)
                        continue;
                    if (buttons[i].name.Contains("英雄") && heroTab == null)
                        heroTab = buttons[i];
                    else if (buttons[i].name.Contains("图鉴") && guideTab == null)
                        guideTab = buttons[i];
                }
                if (heroTab != null) { WireObjectReference(page, "_heroTabButton", heroTab); changes++; }
                if (guideTab != null) { WireObjectReference(page, "_guideTabButton", guideTab); changes++; }
                Debug.Log("[结构修复] 页签按钮：英雄=" + (heroTab != null ? heroTab.name : "(未找到)") +
                          "  图鉴=" + (guideTab != null ? guideTab.name : "(未找到)"));
            }

            // 进度文本：实测均为 TextMeshProUGUI，与字段类型 TMP_Text 一致 → 直接接线。
            Transform barFrame = heroSub != null ? heroSub.Find("upCanvas/底框Image") : null;
            ReportTextType(barFrame, "已收集进度");
            ReportTextType(barFrame, "总英雄数量");

            Transform owned = barFrame != null ? barFrame.Find("已收集进度") : null;
            Transform total = barFrame != null ? barFrame.Find("总英雄数量") : null;

            // ⚠️ 必须传**组件**而不是 Transform。
            // `_ownedProgressText` 的字段类型是 `TMP_Text`，上一版传的是节点的 Transform，
            // 类型不符 → Unity 静默拒绝赋值 → 日志说"已接线"、场景里却始终是 fileID=0。
            TMPro.TMP_Text ownedText = owned != null ? owned.GetComponent<TMPro.TMP_Text>() : null;
            TMPro.TMP_Text totalText = total != null ? total.GetComponent<TMPro.TMP_Text>() : null;
            if (ownedText != null) { WireObjectReference(page, "_ownedProgressText", ownedText); changes++; }
            if (totalText != null) { WireObjectReference(page, "_totalHeroCountText", totalText); changes++; }
            Debug.Log("[结构修复] 进度文本已接线（传组件）：已收集进度=" + (ownedText != null) +
                      " (" + (ownedText != null ? ownedText.GetType().Name : "-") + ")" +
                      "  总英雄数量=" + (totalText != null) +
                      " (" + (totalText != null ? totalText.GetType().Name : "-") + ")");

            return changes;
        }

        /// <summary>只报告某节点的文本组件类型（供决定用什么字段类型接线）。</summary>
        private static void ReportTextType(Transform parent, string name)
        {
            Transform node = parent != null ? parent.Find(name) : null;
            if (node == null)
            {
                Debug.LogWarning("[结构修复] 未找到文本节点 " + name);
                return;
            }
            TMPro.TMP_Text tmp = node.GetComponent<TMPro.TMP_Text>();
            Text legacy = node.GetComponent<Text>();
            Debug.Log("[结构修复] 文本节点 " + name + " 的组件：" +
                      (tmp != null ? "TMP_Text(" + tmp.GetType().Name + ")" : "") +
                      (legacy != null ? " 旧版 Text" : "") +
                      (tmp == null && legacy == null ? "(无文本组件)" : ""));
        }

        /// <summary>
        /// 第 3 步：把英雄页的两个子界面分开（负责人 2026-09-20 批准）。
        ///
        /// `middleCanvas` → 重命名为 `HeroSubPage`（英雄子界面，仅显示已解锁英雄）
        /// 复制一份    → `GuideSubPage`（图鉴子界面，显示全部英雄 + 锁/遮罩区分）
        ///
        /// 图鉴子界面加一个**临时视觉标记**（半透明色块 + 文字）便于区分；
        /// 它是临时物，命名以 `_TEMP_` 开头，后续定稿时整体删掉即可。
        /// 幂等：已存在 `GuideSubPage` 时只补标记。
        /// </summary>
        private static int CreateGuideSubPage()
        {
            Transform page = FindByPath("HeroPage英雄");
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 HeroPage英雄，跳过子界面拆分。");
                return 0;
            }

            int changes = 0;

            // 1) 重命名 middleCanvas → HeroSubPage
            Transform middle = page.Find("middleCanvas");
            if (middle != null)
            {
                middle.name = "HeroSubPage";
                Debug.Log("[结构修复] middleCanvas 已重命名为 HeroSubPage（英雄子界面）。");
                changes++;
            }

            Transform heroSub = page.Find("HeroSubPage");
            if (heroSub == null)
            {
                Debug.LogWarning("[结构修复] 未找到 HeroSubPage，无法复制出图鉴子界面。");
                return changes;
            }

            // 2) 复制出 GuideSubPage
            Transform guide = page.Find("GuideSubPage");
            if (guide == null)
            {
                GameObject clone = Object.Instantiate(heroSub.gameObject, heroSub.parent);
                clone.name = "GuideSubPage";
                // 复制体默认保持与原体相同的锚点/尺寸，仅整体平移以便在编辑器里区分
                RectTransform cloneRect = clone.GetComponent<RectTransform>();
                RectTransform heroRect = heroSub.GetComponent<RectTransform>();
                if (cloneRect != null && heroRect != null)
                {
                    cloneRect.anchorMin = heroRect.anchorMin;
                    cloneRect.anchorMax = heroRect.anchorMax;
                    cloneRect.pivot = heroRect.pivot;
                    cloneRect.sizeDelta = heroRect.sizeDelta;
                    cloneRect.anchoredPosition = heroRect.anchoredPosition;
                }
                guide = clone.transform;
                Debug.Log("[结构修复] 已由 HeroSubPage 复制出 GuideSubPage（图鉴子界面）。");
                changes++;
            }

            // 3) 临时视觉标记 `_TEMP_GuideMarker` —— 定稿后**不再创建**
            //    （负责人 2026-09-20 验收 HeroPage 后定稿；待办 6）。
            //    ⚠️ 注意 `RepairAll` 是幂等的：只要这里还在创建，场景里那个旧标记节点
            //    就算删掉，也会被下一次修复重新加回来。场景中已存在的节点需**另行删除**，
            //    属场景写入，须负责人确认后单独执行。

            return changes;
        }

        /// <summary>
        /// 第 2 步场景侧：把英雄卡牌抽成独立脚本所需的最小场景改动（负责人 2026-09-20 批准）。
        ///
        /// 依据（只读取证【九】）：卡牌模板 `英雄卡Item` 的实测子树里
        /// **没有任何名称 Text 节点**（只有 `卡牌名称Image`），而旧代码找的是"卡牌名称"与
        /// "Text (TMP)" —— 两个都匹配不上，所以**名称与等级从来没被写入过**（卡面一直是占位字）。
        ///
        /// 动作：
        ///   1. 在 `卡牌名称Image` 下**新建 TMP 文本节点** `卡牌名称`（负责人已批准新建）
        ///   2. 给 `英雄卡Item` 挂 `ClientHeroCard` 组件
        ///   3. 按名字自动接线：立绘 / 名称 / 等级 / 等级装饰 / 锁 / 遮罩 / 编队编号 / Button
        /// 幂等：已存在则只补缺失部分。
        /// </summary>
        private static int WireHeroCard()
        {
            Transform template = FindCardUnder("HeroPage英雄");
            if (template == null)
            {
                Debug.LogWarning("[结构修复] 未在 HeroPage英雄 下找到 ClientHeroCard，跳过接线。");
                return 0;
            }
            return WireHeroCardOn(template);
        }

        /// <summary>在指定页面路径片段下，按**现有** `ClientHeroCard` 组件定位卡牌（避免路径依赖）。</summary>
        private static Transform FindCardUnder(string pagePathFragment)
        {
            Pinball.Client.UI.ClientHeroCard[] found =
                Object.FindObjectsOfType<Pinball.Client.UI.ClientHeroCard>(true);
            for (int i = 0; i < found.Length; i++)
            {
                if (found[i] == null)
                    continue;
                if (ClientUiTrace.Path(found[i].transform).Contains(pagePathFragment))
                    return found[i].transform;
            }
            return null;
        }

        // ------------------------------------------------------------------
        // FormationPage编队 的卡牌接线（负责人 2026-09-20 确认“写”）
        // ------------------------------------------------------------------

        private const string FormationCardPrefix = "角色卡牌Button-final";

        /// <summary>列表收敛后的模板名，须与 `ListTargets` 里 FormationPage编队 那条的 TemplateName 一致。</summary>
        private const string FormationTemplateName = "编队英雄卡Item";

        private const string FormationSlotPanelPath = "FormationPage编队/编队卡牌-Panel";
        private const string FormationPickerContentPath = "FormationPage编队/Scroll View/Viewport/Content";

        /// <summary>
        /// 编队页卡牌接线。背景（只读取证 `Logs/verify-formationpage.txt`）：该页 3 张槽位卡 + 9 张选卡上
        /// **都没有 `ClientHeroCard`**（全场景该组件仅 HeroPage 两处），且卡prefab的 `卡牌名称Image`
        /// **没有文本子节点** ⇒ 页面代码无处可写，卡面逻辑整体休眠。
        ///
        /// 动作（幂等，全部复用 HeroPage 已验收过的接线）：给这些卡**补 Button** → 补 `卡牌名称` TMP 节点
        /// → 挂 `ClientHeroCard` → 按名字接线。
        /// **不删任何节点** —— 选卡的“1 模板 + N 实例”由 `ConvergeKnownLists` 白名单负责。
        ///
        /// ⚠️ **必须在 `ConvergeKnownLists()` 之前调用**：收敛会删掉多余内联卡、并把剩下的那张改名成
        /// `编队英雄卡Item`，之后按原名 `角色卡牌Button-final*` 就一个都找不到（首次执行踩过：
        /// 槽位卡接了 3 张、选卡模板 0 张）。
        /// </summary>
        private static int WireFormationCards()
        {
            int changes = 0;
            changes += AddCardButtons(FormationSlotPanelPath, FormationCardPrefix);
            changes += AddCardButtons(FormationPickerContentPath, FormationCardPrefix);

            changes += WireFormationCardsUnder(FormationSlotPanelPath);
            changes += WireFormationCardsUnder(FormationPickerContentPath);
            changes += WireFormationSlotDigits();
            return changes;
        }

        /// <summary>队内编号数字贴图所在目录（工程内**已存在**的 0..9 数字图）。</summary>
        private const string TeamSlotDigitFolder =
            "Assets/Client/UI/HeroDetailPage/Sprites/属性icon/新建文件夹/数字";

        /// <summary>
        /// 给 `ClientFormationPage._teamSlotDigits` 写入数字贴图 0..9（下标 = 数字本身）。
        ///
        /// 为什么是贴图而不是文本：卡prefab 里的 `编队队内编号Image/编号数字` 实测是 **Image**
        /// （`m_Script guid fe87c0e1…` = `UnityEngine.UI.Image`，自带 sprite `数字/1.png`），
        /// 所以“数字动态变化”只能靠**换 sprite**；工程里 0..9 已齐全，无需新建文本节点。
        /// </summary>
        private static int WireFormationSlotDigits()
        {
            GameObject page = FindSceneObject("FormationPage编队", true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 FormationPage编队，跳过队内编号数字贴图接线。");
                return 0;
            }

            ClientFormationPage formation = page.GetComponent<ClientFormationPage>();
            if (formation == null)
            {
                Debug.LogWarning("[结构修复] FormationPage编队 上没有 ClientFormationPage，跳过数字贴图接线。");
                return 0;
            }

            Sprite[] digits = new Sprite[10];
            int missing = 0;
            for (int digit = 0; digit < digits.Length; digit++)
            {
                digits[digit] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                    TeamSlotDigitFolder + "/" + digit + ".png");
                if (digits[digit] == null)
                    missing++;
            }

            int changes = WireArrayReference(formation, "_teamSlotDigits", digits);
            if (missing > 0)
            {
                Debug.LogWarning("[结构修复] 队内编号数字贴图缺失 " + missing + " 张（" + TeamSlotDigitFolder +
                                 "）；运行时越界只会显隐、不换图。");
            }
            Debug.Log("[结构修复] ClientFormationPage._teamSlotDigits <- 数字贴图 0..9（缺 " + missing + " 张）。");
            return changes;
        }

        private static int WireFormationCardsUnder(string containerPath)
        {
            Transform container = FindByPath(containerPath);
            if (container == null)
            {
                Debug.LogWarning("[结构修复] 未找到编队卡牌容器：" + containerPath);
                return 0;
            }

            int changes = 0;
            for (int i = 0; i < container.childCount; i++)
            {
                Transform card = container.GetChild(i);
                // 收敛前卡名是 `角色卡牌Button-final 1*`；收敛后只剩模板 `编队英雄卡Item`。
                // 两种名字都要匹配，否则第二次 RepairAll 会漏掉模板（本步骤必须幂等）。
                if (!card.name.StartsWith(FormationCardPrefix, System.StringComparison.Ordinal) &&
                    card.name != FormationTemplateName)
                    continue;
                changes += WireHeroCardOn(card);
            }
            Debug.Log("[结构修复] " + containerPath + " 下接线卡牌 " + changes + " 处。");
            return changes;
        }

        /// <summary>
        /// 把一张卡牌接成 `ClientHeroCard` 可驱动的形态（幂等）：补 `卡牌名称` TMP 节点 → 挂组件 → 按名字接线。
        /// `WireHeroCard()`（HeroPage）与 `WireFormationCards()`（编队页）共用。
        /// </summary>
        private static int WireHeroCardOn(Transform template)
        {
            if (template == null)
                return 0;

            int changes = 0;

            // ---- 1) 名称 TMP 节点 ----
            //
            // 查找顺序（2026-09-20 加固，配合负责人对卡面名称的微调）：
            //   ① **在整张卡的子树里找 `卡牌名称`** —— 负责人可能把名称文本从 `卡牌名称Image`
            //      下**移走**、甚至**删掉那张 Image**（"取消英雄名称的 image"）。按名深搜可以适配任意父节点。
            //   ② 找不到才回退旧路径：`卡牌名称Image` 下建一个。
            //   ③ 两处都没有 ⇒ 只警告、不新建（避免凭空造节点）。
            //
            // ⚠️ 为什么必须加固：本方法结尾会 `WireObjectReference(card, "_nameText", nameNode)`。
            //    若名称节点挪了位置而这里仍按旧路径找，`nameNode` 会是 null ⇒ **把 `_nameText` 清空**，
            //    表现为"名称静默不显示"（`Apply` 里有 null 判断，不报错）。**节点移动本身不会断引用**
            //    （Unity 的引用按对象走），只有"工具把引用写成 null"才会断 —— 所以在源头修。
            Transform nameTransform = FindDeepChildTransform(template, "卡牌名称");
            GameObject nameNode = nameTransform != null ? nameTransform.gameObject : null;

            if (nameNode == null)
            {
                Transform nameHolder = template.Find("卡牌名称Image");
                if (nameHolder != null)
                {
                    nameNode = new GameObject("卡牌名称", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
                    RectTransform rt = nameNode.GetComponent<RectTransform>();
                    rt.SetParent(nameHolder, false);
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    rt.localScale = Vector3.one;

                    TMPro.TextMeshProUGUI tmp = nameNode.GetComponent<TMPro.TextMeshProUGUI>();
                    tmp.text = "名称";
                    tmp.alignment = TMPro.TextAlignmentOptions.Center;
                    tmp.fontSize = 20f;
                    tmp.color = Color.black;
                    tmp.raycastTarget = false;   // 文本只显示，不参与射线（交互由卡牌根 Button 承担）
                    Debug.Log("[结构修复] 已在 卡牌名称Image 下新建 TMP 文本节点「卡牌名称」" +
                              "（原模板没有名称文本节点，这是旧代码写不进名称的原因）。");
                    changes++;
                }
                else
                {
                    Debug.LogWarning("[结构修复] 卡牌 " + template.name +
                                     " 下既没有 `卡牌名称`，也没有 `卡牌名称Image`，跳过名称节点处理。");
                }
            }

            // ---- 2) 挂组件 ----
            Pinball.Client.UI.ClientHeroCard card = template.GetComponent<Pinball.Client.UI.ClientHeroCard>();
            if (card == null)
            {
                card = template.gameObject.AddComponent<Pinball.Client.UI.ClientHeroCard>();
                Debug.Log("[结构修复] 已给 " + template.name + " 挂上 ClientHeroCard 组件。");
                changes++;
            }

            // ---- 3) 接线（WireObjectReference 返回 void，故不累加 changes）----
            WireObjectReference(card, "_illustration", FindChildObject(template, "立绘Image"));
            WireObjectReference(card, "_nameText", nameNode);
            WireObjectReference(card, "_levelText", FindChildObject(template, "等级-Text"));
            WireObjectReference(card, "_lockRoot", FindChildObject(template, "锁Image"));
            WireObjectReference(card, "_maskRoot", FindChildObject(template, "Mark"));
            WireObjectReference(card, "_teamBadge", FindChildObject(template, "编队队内编号Image"));
            WireObjectReference(card, "_button", template.GetComponent<Button>());

            GameObject[] decorations = new GameObject[] { FindChildObject(template, "Lv-Image") };
            changes += WireArrayReference(card, "_levelDecorations", decorations);

            Debug.Log("[结构修复] ClientHeroCard 接线完成：立绘=" + Name4Log(FindChildObject(template, "立绘Image")) +
                      " 名称=" + Name4Log(nameNode) +
                      " 等级=" + Name4Log(FindChildObject(template, "等级-Text")) +
                      " 锁=" + Name4Log(FindChildObject(template, "锁Image")) +
                      " 遮罩=" + Name4Log(FindChildObject(template, "Mark")) +
                      " 编队编号=" + Name4Log(FindChildObject(template, "编队队内编号Image")) +
                      " Button=" + (template.GetComponent<Button>() != null ? "有" : "无"));
            return changes;
        }

        /// <summary>卡牌 prefab 的资产路径（英雄卡的共用来源：HeroPage / FormationPage / ProfilePage 主页展示…）。</summary>
        private const string HeroCardPrefabPath = "Assets/Client/UI/Prefabs/角色卡牌Button-final 1.prefab";

        /// <summary>
        /// **卡面名称节点迁移**（负责人 2026-09-20 要求："取消英雄名称的 image，直接显示文字"，范围=**全改**）。
        ///
        /// 背景：卡prefab 里原本只有一张 `卡牌名称Image`（纯 Image、**没有文本子节点**），
        /// 名称 TMP 是结构修复工具**在场景实例里**新建的（路径 `卡牌名称Image/卡牌名称`）。
        ///
        /// 目标：名称文本**直接挂在卡根上**（锚点/尺寸/位置**沿用原 Image**，所以文字落在原处），
        /// 并且**把文本节点放进 prefab 本体** —— 这样以后新建的卡实例天生就带名称节点，不依赖场景补建。
        ///
        /// 顺序很重要（否则会静默丢名称）：
        ///   ① 先在 prefab 里建 `卡牌名称`（此时 `卡牌名称Image` 还在）
        ///   ② 再删 `卡牌名称Image`
        ///   ③ 清掉场景实例里**旧的、实例级**的 `卡牌名称`（`IsAddedGameObjectOverride`）并**重新接线**
        ///      —— 若直接删 Image，挂在它下面的实例级子节点会一并消失，`_nameText` 就断了。
        ///
        /// 幂等：prefab 里已有 `卡牌名称` 且 `卡牌名称Image` 已删 ⇒ 直接跳过（步骤③仍会补接线）。
        /// </summary>
        private static int MigrateCardNameNode()
        {
            int changes = 0;

            // ---------- ① / ② prefab 本体 ----------
            GameObject prefabRoot = null;
            try
            {
                prefabRoot = PrefabUtility.LoadPrefabContents(HeroCardPrefabPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[结构修复] 无法以编辑方式打开卡牌 prefab：" + HeroCardPrefabPath + " — " + ex.Message);
                return 0;
            }

            if (prefabRoot == null)
            {
                Debug.LogWarning("[结构修复] 卡牌 prefab 不存在或为空：" + HeroCardPrefabPath);
                return 0;
            }

            try
            {
                Transform image = FindDeepChildTransform(prefabRoot.transform, "卡牌名称Image");
                Transform existing = FindDeepChildTransform(prefabRoot.transform, "卡牌名称");

                if (existing == null && image != null)
                {
                    RectTransform source = image as RectTransform;
                    GameObject node = new GameObject("卡牌名称", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
                    RectTransform rect = node.GetComponent<RectTransform>();

                    // 与原 Image 同级、同位置、同尺寸 —— 视觉上文字就落在原来那块底图的位置。
                    rect.SetParent(image.parent, false);
                    rect.SetSiblingIndex(image.GetSiblingIndex());
                    if (source != null)
                    {
                        rect.anchorMin = source.anchorMin;
                        rect.anchorMax = source.anchorMax;
                        rect.pivot = source.pivot;
                        rect.anchoredPosition = source.anchoredPosition;
                        rect.sizeDelta = source.sizeDelta;
                    }
                    rect.localScale = Vector3.one;
                    rect.localRotation = Quaternion.identity;

                    TMPro.TextMeshProUGUI text = node.GetComponent<TMPro.TextMeshProUGUI>();
                    text.text = "名称";
                    text.alignment = TMPro.TextAlignmentOptions.Center;
                    text.fontSize = 20f;
                    text.color = Color.black;
                    text.raycastTarget = false;   // 交互由卡根 Button 承担

                    Debug.Log("[结构修复] 已在卡牌 prefab 本体新建 `卡牌名称`（沿用原 `卡牌名称Image` 的位置与尺寸）。");
                    changes++;
                }

                if (image != null)
                {
                    Object.DestroyImmediate(image.gameObject, true);
                    Debug.Log("[结构修复] 已删除卡牌 prefab 的 `卡牌名称Image`（负责人要求取消名称底图）。");
                    changes++;
                }

                if (changes > 0)
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, HeroCardPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            // ---------- ③ 场景侧：把所有仍在的 `卡牌名称Image` 处理掉 ----------
            //
            // ⚠️ 实测（2026-09-20）：改完 prefab 后，场景里**仍有 4 处 `卡牌名称Image`**
            //    （FormationPage 编队卡牌-Panel 的 3 张槽位卡 + HeroPage 图鉴子页的模板卡）
            //    —— 说明它们**并非**该 prefab 的实例（或新结构未传播过去），
            //    因此这一步**不能依赖 prefab 血缘**，必须按节点名在场景里逐个处理。
            //
            // 规则（每处 `卡牌名称Image`）：
            //   · 若该卡子树里**已存在**一个不在它下面的 `卡牌名称` ⇒ 直接删掉这个 Image（连同旧子节点），避免重复；
            //   · 否则把它的 `卡牌名称` 子节点**上提到同一父节点**（沿用 Image 的锚点/尺寸/位置，视觉不变），再删 Image。
            int promoted = 0;
            int removed = 0;

            List<Transform> holders = new List<Transform>();
            Transform[] allTransforms = Object.FindObjectsOfType<Transform>(true);
            for (int index = 0; index < allTransforms.Length; index++)
            {
                if (allTransforms[index] != null && allTransforms[index].name == "卡牌名称Image")
                    holders.Add(allTransforms[index]);
            }

            for (int index = 0; index < holders.Count; index++)
            {
                Transform holder = holders[index];
                if (holder == null || holder.parent == null)
                    continue;

                Transform nameChild = holder.Find("卡牌名称");

                // 卡根子树里是否已有"不在该 Image 之下"的名称节点（通常是 prefab 本体带来的那份）
                bool alreadyHasNameOutside = false;
                List<Transform> existing = new List<Transform>();
                CollectDeepChildren(holder.parent, "卡牌名称", existing);
                for (int i = 0; i < existing.Count; i++)
                {
                    if (!existing[i].IsChildOf(holder))
                    {
                        alreadyHasNameOutside = true;
                        break;
                    }
                }

                if (nameChild != null && !alreadyHasNameOutside)
                {
                    RectTransform childRect = nameChild as RectTransform;
                    RectTransform holderRect = holder as RectTransform;

                    int sibling = holder.GetSiblingIndex();
                    nameChild.SetParent(holder.parent, false);
                    nameChild.SetSiblingIndex(sibling);

                    // 子节点原本是"填满 Image"的，所以上提后要**继承 Image 的矩形**才能保持原位。
                    if (childRect != null && holderRect != null)
                    {
                        childRect.anchorMin = holderRect.anchorMin;
                        childRect.anchorMax = holderRect.anchorMax;
                        childRect.pivot = holderRect.pivot;
                        childRect.anchoredPosition = holderRect.anchoredPosition;
                        childRect.sizeDelta = holderRect.sizeDelta;
                        childRect.localScale = Vector3.one;
                        childRect.localRotation = Quaternion.identity;
                    }

                    Debug.Log("[结构修复] 已把 " + holder.parent.name + "/" + holder.name + " 下的 `卡牌名称` 上提一级" +
                              "（沿用原 Image 的位置与尺寸），随后删除该 Image。");
                    promoted++;
                }

                Object.DestroyImmediate(holder.gameObject, true);
                removed++;
            }

            if (promoted > 0 || removed > 0)
                Debug.Log("[结构修复] 场景侧：上提名称文本 " + promoted + " 处，删除 `卡牌名称Image` " + removed + " 处。");

            // ---------- ④ 重新接线（幂等）----------
            ClientHeroCard[] cards = Object.FindObjectsOfType<ClientHeroCard>(true);
            for (int index = 0; index < cards.Length; index++)
            {
                ClientHeroCard card = cards[index];
                if (card == null)
                    continue;

                Transform nameNode = FindDeepChildTransform(card.transform, "卡牌名称");
                WireObjectReference(card, "_nameText", nameNode != null ? nameNode.gameObject : null);

                if (nameNode == null)
                    Debug.LogWarning("[结构修复] 卡 " + card.name + " 下没有 `卡牌名称`，名称将无法显示（请检查该卡的结构）。");
            }

            return changes + promoted + removed;
        }

        /// <summary>把 <paramref name="root"/> 子树里所有名叫 <paramref name="name"/> 的节点收集到 <paramref name="result"/>。</summary>
        private static void CollectDeepChildren(Transform root, string name, List<Transform> result)
        {
            if (root == null || result == null)
                return;

            for (int index = 0; index < root.childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (child.name == name)
                    result.Add(child);
                CollectDeepChildren(child, name, result);
            }
        }

        private static GameObject FindChildObject(Transform root, string name)
        {
            Transform found = root != null ? root.Find(name) : null;
            return found != null ? found.gameObject : null;
        }

        /// <summary>
        /// **按名深度查找**（任意层级）。
        /// 与 <see cref="FindChildObject"/>（只看直接子节点）相对；用于"负责人可能挪动过节点"的场合，
        /// 例如卡面名称文本从 `卡牌名称Image` 下移走之后仍能被找到。
        /// </summary>
        private static Transform FindDeepChildTransform(Transform root, string name)
        {
            if (root == null)
                return null;

            for (int index = 0; index < root.childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (child.name == name)
                    return child;

                Transform found = FindDeepChildTransform(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static string Name4Log(GameObject go)
        {
            return go != null ? go.name : "(未找到)";
        }

        /// <summary>
        /// 接线数组字段（SerializedProperty 才能改数组）。
        /// 形参用 <see cref="Object"/>[] 而非 GameObject[]，这样 `GameObject[]`（等级装饰）与
        /// `Sprite[]`（队内编号数字贴图）都能传进来。
        /// </summary>
        private static int WireArrayReference(Object target, string field, Object[] values)
        {
            if (target == null || values == null)
                return 0;

            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(field);
            if (prop == null || !prop.isArray)
            {
                Debug.LogWarning("[结构修复] 未找到数组字段 " + field);
                return 0;
            }

            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return 1;
        }

        /// <summary>
        /// 把"顶部区域、却仍用中心锚"的元素改为贴顶锚（负责人 2026-09-20 确认，共 5 处）。
        ///
        /// 来源：只读扫描【七】筛过后的真候选（已排除"我已改好的"与"面板型页面里的"）。
        /// 这些页面都已改为全屏拉伸，中心锚意味着"离画布中心的距离固定"，
        /// 而 `match=0` 下画布高度随屏幕比例变化（1080×1920→1339 最矮、1170×2532→1630、
        /// 1080×2400→1673），于是它们随高度漂移，只在最矮那档明显撞车。
        ///
        /// 处理方式与水晶石一致：**只改纵向锚点为贴顶，横向锚点原样保留，世界坐标回写**。
        /// </summary>
        private static readonly string[] TopAreaElements =
        {
            "MailPage邮件/Email Title",
            "ActivityPage活动/task title",
            "FormationPage编队/页面分栏-Image",
            "Complete Guide Event Page全图鉴活动/顶部角色-Image",
            "Complete Guide Event Page全图鉴活动/task title",
        };

        private static int NormalizeTopAreaElements()
        {
            int changes = 0;
            for (int i = 0; i < TopAreaElements.Length; i++)
            {
                RectTransform rect = FindByPath(TopAreaElements[i]) as RectTransform;
                if (rect == null)
                {
                    Debug.LogWarning("[结构修复] 未找到顶部区域元素 " + TopAreaElements[i] + "，跳过。");
                    continue;
                }
                changes += AnchorVerticallyTop(rect, TopAreaElements[i]);
            }
            return changes;
        }

        /// <summary>
        /// 还原 `Player lineup`（玩家阵容）为**固定尺寸 + 中央锚点**（负责人 2026-09-20 验收判定）。
        ///
        /// 由来：本轮把它当"全屏型"加入了拉伸白名单（依据只是"尺寸等于参考分辨率 752×1630"），
        /// 但负责人验收认为它是**面板型**、不该铺满。**这说明"尺寸等于参考分辨率"不足以判定类型**
        /// —— 类型是设计意图，必须由负责人确认，不能仅凭尺寸推断。
        ///
        /// 还原值取自修改前的实测记录：`752×1630`、锚点 `(0.5,0.5)-(0.5,0.5)`、居中。
        /// 被拉伸时只改了根（背景未被匹配拉伸），故只需还原根。
        /// </summary>
        private static int RestorePlayerLineupRect()
        {
            GameObject page = FindSceneObject("Player lineup", true);
            if (page == null)
                return 0;

            RectTransform rect = page.GetComponent<RectTransform>();
            if (rect == null)
                return 0;

            Vector2 half = new Vector2(0.5f, 0.5f);

            // ⚠️ 只在"仍是拉伸态"时才还原，**绝不强行写尺寸**。
            // 原因：负责人随后把它手动调成了 `675×1127`（该界面的正常尺寸）。
            // 若无条件写回 752×1630，每次 RepairAll 都会覆盖负责人的手工调整。
            // 判定依据用"锚点是否仍是 (0,0)-(1,1)"，与尺寸解耦。
            bool stillStretched = rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one;
            if (!stillStretched)
                return 0;

            Debug.Log("[结构修复] Player lineup 当前：size=" + rect.rect.size.ToString("F0") +
                      " 锚=" + rect.anchorMin + "-" + rect.anchorMax + "（仍为拉伸态，执行还原）");
            rect.anchorMin = half;
            rect.anchorMax = half;
            rect.pivot = half;
            rect.sizeDelta = new Vector2(752f, 1630f);   // 仅首次还原时使用；之后由负责人自行调整
            rect.anchoredPosition = Vector2.zero;
            Debug.Log("[结构修复] Player lineup 已从拉伸态还原为固定尺寸中央锚点（面板型，不应全屏拉伸）。");
            return 1;
        }

        /// <summary>
        /// 删除**非根画布**上的 `CanvasScaler`（负责人 2026-09-20 授权"123 全部修复"）。
        ///
        /// **这是贯穿全项目的分辨率适配根因。**
        /// 体检（`Logs/hierarchy-audit.txt` 第六节）实测：全场景 **123 处**嵌套 Canvas 各自带着
        /// `CanvasScaler`，且模式全是 `ConstantPixelSize`、参考 `800×600` —— 即那些子树
        /// **完全不跟随根画布缩放**，尺寸被钉死在固定像素上。Unity 在 Inspector 里本就警告：
        ///   "Non-root Canvases will not be scaled."
        ///
        /// 后果：根画布按 `ScaleWithScreenSize` 缩放，而嵌套子树不动 → 两者随屏幕比例**相对漂移**。
        /// 实测对照（负责人自己观察到的）：
        ///   `Top function bar`（带 Canvas+CanvasScaler）→ 飘 ✗
        ///   `Bottom page function bar`（不带）        → 稳 ✓
        ///
        /// 因此本方法**只删除 `CanvasScaler` 组件**：
        ///   - 嵌套 `Canvas` 本身保留（它负责渲染与射线的隔离，删了会改变层级行为）；
        ///   - **不触碰任何 Pos / Size / 锚点**，所以不会移动任何东西；
        ///   - 删除后嵌套 Canvas 自动继承根画布的缩放，与其它节点一致。
        /// 根画布的 `CanvasScaler` **保留**（用 `Canvas.isRootCanvas` 判定）。
        /// 幂等：已无多余 Scaler 时返回 0。
        /// </summary>
        private static int RemoveNestedCanvasScalers()
        {
            CanvasScaler[] all = Object.FindObjectsOfType<CanvasScaler>(true);
            int removed = 0;
            int skippedPrefab = 0;
            StringBuilder list = new StringBuilder();

            for (int i = 0; i < all.Length; i++)
            {
                CanvasScaler scaler = all[i];
                if (scaler == null)
                    continue;

                Canvas owner = scaler.GetComponent<Canvas>();
                if (owner == null)
                    continue;                       // 没有 Canvas 的 Scaler 不动（异常情况留给人看）

                // ⚠️ 口径必须与体检一致：只保留 `ClientCanvas` **自身**的 Scaler，其余一律删除。
                // 之前用 owner.isRootCanvas 判定，结果把 111 处全放过了（第一次只删掉 12 处，
                // 而体检仍报 111 处）——两套口径不一致导致的漏删。
                GameObject rootCanvasGo = FindSceneObject("ClientCanvas", true);
                if (rootCanvasGo != null && scaler.gameObject == rootCanvasGo)
                    continue;

                if (!scaler.gameObject.scene.IsValid())
                {
                    skippedPrefab++;                // 预制体资源，不在场景里，不碰
                    continue;
                }

                if (removed < 15)
                    list.Append("\n        - ").Append(ClientUiTrace.Path(scaler.transform))
                        .Append("  (").Append(scaler.uiScaleMode).Append(" ref=")
                        .Append(scaler.referenceResolution).Append(")");
                Object.DestroyImmediate(scaler, true);
                removed++;
            }

            if (removed == 0)
                Debug.Log("[结构修复] 非根画布 CanvasScaler：已无多余项（幂等，无改动）。");
            else
                Debug.Log("[结构修复] 已删除非根画布上的 CanvasScaler 共 " + removed + " 处" +
                          "（保留嵌套 Canvas 本身，未改动任何 Pos/Size/锚点）。前几处：" + list +
                          (skippedPrefab > 0 ? "\n        另有 " + skippedPrefab + " 处属于预制体资源，未触碰。" : ""));
            return removed;
        }

        /// <summary>
        /// 商店顶部/底部栏的**机械性**锚点规范化（负责人 2026-09-20 确认分工）。
        ///
        /// 负责人确认的期望结构：**标题贴顶、功能栏在标题下方**、底栏横向铺满。
        ///
        /// 结构病因：`Shopping Mall Title` 实际是 **`Bottom page function bar` 的子节点**
        /// （见 Canvas 链 `.../Bottom page function bar/Shopping Mall Title/...`），
        /// 于是它继承了"底部锚"父级的运动，在 `matchWidthOrHeight=0.5`（画布高度随比例变化）
        /// 下与顶部功能栏错位重叠。**只改自己的锚点救不了，必须换父级。**
        ///
        /// 本方法只做"锚点类型 + 父级"这类机械调整，**间距/大小/视觉位置由负责人微调**：
        ///   1. 把 `Shopping Mall Title` 移到页面根下并改为**贴顶锚**
        ///   2. `Top function bar` 改为贴顶锚，并把它的顶边对齐到标题的下边缘（间距留 0，由负责人微调）
        ///   3. `Bottom page function bar` 改为**底部横向铺满**（左右 offset 归零、高度不变）
        /// 改锚点时以世界坐标回写，保证当前视觉位置不跳。
        /// </summary>
        private static int NormalizeShopBars()
        {
            GameObject page = FindSceneObject("ShopPage商店", true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 ShopPage商店，跳过顶/底栏锚点规范化。");
                return 0;
            }

            RectTransform pageRect = page.GetComponent<RectTransform>();
            if (pageRect == null)
                return 0;

            int changes = 0;

            // 1) 标题：换父级到页面根 + 贴顶
            Transform bottomBarNode = FindByPath("ShopPage商店/Bottom page function bar");
            Transform title = bottomBarNode != null ? bottomBarNode.Find("Shopping Mall Title") : null;
            if (title == null)
                title = FindByPath("ShopPage商店/Shopping Mall Title");

            RectTransform titleRect = title != null ? title.GetComponent<RectTransform>() : null;
            if (titleRect != null)
            {
                if (titleRect.parent != pageRect)
                {
                    Vector3 world = titleRect.position;
                    titleRect.SetParent(pageRect, true);   // 保持世界位置
                    titleRect.position = world;
                    Debug.Log("[结构修复] Shopping Mall Title 父级由 " +
                              (bottomBarNode != null ? bottomBarNode.name : "(未知)") +
                              " 改为页面根（原本挂在底部栏下，会跟着底栏漂移）。");
                    changes++;
                }
                changes += AnchorTopCenter(titleRect, "Shopping Mall Title");
            }
            else
            {
                Debug.LogWarning("[结构修复] 未找到 Shopping Mall Title，跳过标题锚点调整。");
            }

            // 2) 顶部功能栏：贴顶 + 置于标题下方
            RectTransform topBar = FindByPath("ShopPage商店/Top function bar") as RectTransform;
            if (topBar != null)
            {
                changes += AnchorTopCenter(topBar, "Top function bar");
                if (titleRect != null)
                {
                    Vector3[] titleCorners = new Vector3[4];
                    titleRect.GetWorldCorners(titleCorners);   // 0=左下 1=左上 2=右上 3=右下
                    Vector3[] barCorners = new Vector3[4];
                    topBar.GetWorldCorners(barCorners);
                    float delta = titleCorners[0].y - barCorners[1].y;   // 标题下边缘 - 功能栏上边缘
                    if (Mathf.Abs(delta) > 0.5f)
                    {
                        topBar.position += new Vector3(0f, delta, 0f);
                        Debug.Log("[结构修复] Top function bar 已对齐到标题下方（间距 0，可自行微调）：位移 " +
                                  delta.ToString("F1"));
                        changes++;
                    }
                }
            }

            // 3) 底部功能栏：底部横向铺满
            RectTransform bottomBar = bottomBarNode as RectTransform;
            if (bottomBar != null)
                changes += StretchHorizontallyAtBottom(bottomBar, "Bottom page function bar");

            // 4) 水晶石兑换条：它是"顶部区域"的元素，但用的是中心锚 →
            //    根拉伸后随画布高度漂移（`match=0` 下 1080×1920 的参考高仅 1339，
            //    是三档里最矮的，所以只有那一档明显错位）。
            //    只改**纵向**锚点为贴顶，横向锚点原样保留，避免影响它的左右定位。
            RectTransform crystal = FindByPath("ShopPage商店/Regular store resource exchange - Crystal stones") as RectTransform;
            if (crystal != null)
                changes += AnchorVerticallyTop(crystal, "Regular store resource exchange - Crystal stones");

            return changes;
        }

        /// <summary>
        /// 只把**纵向**锚点改为贴顶（1），**横向锚点保持不变**，并以世界坐标回写保持视觉位置。
        ///
        /// 为什么只改纵向：这次要解决的只是"随画布高度漂移"，
        /// 保留横向锚点可避免同时改动左右定位、把问题从一处挪到另一处。
        /// </summary>
        private static int AnchorVerticallyTop(RectTransform rect, string label)
        {
            if (rect == null)
                return 0;

            if (Mathf.Approximately(rect.anchorMin.y, 1f) && Mathf.Approximately(rect.anchorMax.y, 1f))
                return 0;

            Vector3 world = rect.position;
            rect.anchorMin = new Vector2(rect.anchorMin.x, 1f);
            rect.anchorMax = new Vector2(rect.anchorMax.x, 1f);
            rect.position = world;
            Debug.Log("[结构修复] " + label + " 纵向锚点改为贴顶 (y=1)，横向锚点保持不变" +
                      "（此前是中心锚，会随画布高度漂移）。");
            return 1;
        }

        /// <summary>改为顶部居中的固定锚；以世界坐标回写，保持当前视觉位置。</summary>
        private static int AnchorTopCenter(RectTransform rect, string label)
        {
            if (rect == null)
                return 0;

            Vector2 anchor = new Vector2(0.5f, 1f);
            if (rect.anchorMin == anchor && rect.anchorMax == anchor && rect.pivot == anchor)
                return 0;

            Vector3 world = rect.position;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.position = world;
            Debug.Log("[结构修复] " + label + " 锚点改为顶部居中 (0.5,1)（此前为中心锚，会随画布高度漂移）。");
            return 1;
        }

        /// <summary>改为底部横向铺满：左右 offset 归零，高度与纵向位置保持。</summary>
        private static int StretchHorizontallyAtBottom(RectTransform rect, string label)
        {
            if (rect == null)
                return 0;

            Vector2 wantMin = new Vector2(0f, 0f);
            Vector2 wantMax = new Vector2(1f, 0f);
            Vector2 wantPivot = new Vector2(0.5f, 0f);
            bool already = rect.anchorMin == wantMin && rect.anchorMax == wantMax &&
                           rect.pivot == wantPivot &&
                           Mathf.Approximately(rect.offsetMin.x, 0f) &&
                           Mathf.Approximately(rect.offsetMax.x, 0f);
            if (already)
                return 0;

            float offsetMinY = rect.offsetMin.y;
            float offsetMaxY = rect.offsetMax.y;
            rect.anchorMin = wantMin;
            rect.anchorMax = wantMax;
            rect.pivot = wantPivot;
            rect.offsetMin = new Vector2(0f, offsetMinY);
            rect.offsetMax = new Vector2(0f, offsetMaxY);
            Debug.Log("[结构修复] " + label + " 改为底部横向铺满（左右 offset 归零，高度不变）。" +
                      " 原尺寸 " + rect.rect.size.ToString("F0"));
            return 1;
        }

        /// <summary>
        /// 把"全屏型"弹窗的根与其背景子节点改为全屏拉伸（负责人 2026-09-20 授权"全部都要改"）。
        ///
        /// 依据（只读体检，Logs/hierarchy-audit.txt 第五节）：
        ///   CanvasScaler = ScaleWithScreenSize，参考 753×1630，matchWidthOrHeight = 0.5
        ///   → 宽高混合匹配，屏幕比例一变画布参考尺寸**宽高都会变**，
        ///     而各弹窗根写成了固定尺寸（如商店 752×1527），于是必然露边。
        ///
        /// 体检结论：这些页面的内容**绝大多数是中心锚**，改根后位置不变；
        /// 边缘锚的多为底栏/顶栏（Bottom page function bar、upCanvas 等），
        /// 改根后它们会自然贴到**真正的屏幕边缘** —— 这正是期望行为。
        /// 因此改动是安全的，但**光改根不够**：背景多为固定尺寸子节点，必须一并拉伸。
        ///
        /// 本方法：
        ///   1. 根 → 锚点 (0,0)-(1,1)、offset 归零
        ///   2. 名称含 background/bg/背景/底图 的**直接子节点** → 同样铺满
        ///   3. 其余子节点一律不动
        /// 并把每个直接子节点的尺寸打进日志，便于核对背景节点是否找对。
        /// </summary>
        private static readonly string[] FullScreenPopups =
        {
            "ShopPage商店",
            "MailPage邮件",
            "RankPage排行榜",
            "ActivityPage活动",
            "HeroPage英雄",
            "HeroDetailPage英雄详情主页",
            "ProfilePage个人中心",
            "FormationPage编队",
            "Complete Guide Event Page全图鉴活动",
            // 2026-09-20 负责人确认一并加入：这四页尺寸等于参考分辨率（752×1630），
            // 属全屏型而非面板型，此前遗漏在名单外，同样会露边。
            "Email Details Page（Have）",
            "Email Details Page (No)",
            "Success Receipt Interface",
            // ⚠️ "Player lineup" 曾于本轮误加入，负责人验收判定它为**面板型**（不该全屏拉伸），
            //    已从名单移除，并由 RestorePlayerLineupRect() 还原为固定尺寸中央锚点。
        };

        /// <summary>本轮执行范围：负责人已确认"全部都做"，故与全屏型白名单一致。</summary>
        private static readonly string[] FullScreenPopupsThisRun = FullScreenPopups;

        /// <summary>
        /// ② 把 `ClientCanvas` 的 `matchWidthOrHeight` 从 0.5 改为 0（按宽匹配）。
        ///
        /// 竖屏游戏按**宽**匹配最稳：参考宽度恒定（恒为 753 单位），横向布局在所有设备上一致；
        /// 竖屏多出来的高度由**贴顶/贴底锚点**自然吸收，正与锚点规范配套。
        /// 0.5（宽高混合）会让宽高同时变化，是最难设计的档位，也是此前测量反复对不上的来源之一。
        /// 同时取消 `pixelPerfect`（非整数缩放下会引入 1px 抖动）。
        /// </summary>
        private static int NormalizeCanvasScaler()
        {
            GameObject root = FindSceneObject("ClientCanvas", true);
            if (root == null)
            {
                Debug.LogWarning("[结构修复] 未找到 ClientCanvas，跳过 CanvasScaler 调整。");
                return 0;
            }

            int changes = 0;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            if (scaler != null && !Mathf.Approximately(scaler.matchWidthOrHeight, 0f))
            {
                Debug.Log("[结构修复] ClientCanvas.matchWidthOrHeight：" + scaler.matchWidthOrHeight +
                          " → 0（改为按宽匹配：参考宽度恒定、高度交给边锚吸收）。");
                scaler.matchWidthOrHeight = 0f;
                changes++;
            }

            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas != null && canvas.pixelPerfect)
            {
                canvas.pixelPerfect = false;
                Debug.Log("[结构修复] ClientCanvas.pixelPerfect：true → false（避免非整数缩放的 1px 抖动）。");
                changes++;
            }
            return changes;
        }

        /// <summary>
        /// ③ 其余 8 页套用商店同一套规则：根与背景全屏拉伸 + 底栏横向铺满。
        /// 负责人已确认"①②③ 都同意，全部都做"。
        /// </summary>
        private static int NormalizeAllPageBars()
        {
            int changes = 0;
            for (int i = 0; i < FullScreenPopups.Length; i++)
            {
                string pageName = FullScreenPopups[i];
                GameObject page = FindSceneObject(pageName, true);
                if (page == null)
                    continue;

                for (int k = 0; k < page.transform.childCount; k++)
                {
                    Transform child = page.transform.GetChild(k);
                    string lower = child.name.ToLowerInvariant();
                    bool isBottomBar = lower.Contains("bottom page function bar") ||
                                       child.name.Contains("下功能底框");
                    if (!isBottomBar)
                        continue;

                    RectTransform childRect = child.GetComponent<RectTransform>();
                    if (childRect == null)
                        continue;
                    // 只在"固定宽度"时才改，避免重复处理
                    if (Mathf.Approximately(childRect.anchorMin.x, 0f) &&
                        Mathf.Approximately(childRect.anchorMax.x, 1f))
                        continue;

                    changes += StretchHorizontallyAtBottom(childRect, pageName + "/" + child.name);
                }
            }
            return changes;
        }

        private static int StretchFullScreenPopups()
        {
            int changes = 0;
            for (int i = 0; i < FullScreenPopupsThisRun.Length; i++)
            {
                string pageName = FullScreenPopupsThisRun[i];
                GameObject page = FindSceneObject(pageName, true);
                if (page == null)
                {
                    Debug.LogWarning("[结构修复] 未找到全屏型弹窗 " + pageName + "，跳过拉伸。");
                    continue;
                }

                RectTransform rect = page.GetComponent<RectTransform>();
                if (rect == null)
                    continue;

                changes += StretchToFill(rect, "根 " + pageName);

                // 背景子节点：名称匹配则一并铺满，否则只报告，避免误拉伸内容容器。
                StringBuilder children = new StringBuilder();
                for (int k = 0; k < page.transform.childCount; k++)
                {
                    Transform child = page.transform.GetChild(k);
                    RectTransform childRect = child.GetComponent<RectTransform>();
                    if (childRect == null)
                        continue;

                    children.Append("\n        · ").Append(child.name)
                            .Append("  ").Append(childRect.rect.size.ToString("F0"))
                            .Append("  锚=").Append(childRect.anchorMin).Append('-').Append(childRect.anchorMax);

                    string lower = child.name.ToLowerInvariant();
                    bool looksLikeBackground = lower.Contains("background") || lower.Contains("bg") ||
                                               child.name.Contains("背景") || child.name.Contains("底图");
                    if (looksLikeBackground)
                    {
                        children.Append("   ← 识别为背景，已铺满");
                        changes += StretchToFill(childRect, "背景 " + child.name);
                    }
                }
                Debug.Log("[结构修复] " + pageName + " 直接子节点清单（用于核对背景是否找对）：" + children);
            }
            return changes;
        }

        /// <summary>把节点设为铺满父级：锚点 (0,0)-(1,1)、offset 归零、pivot 居中。已是则不改。</summary>
        private static int StretchToFill(RectTransform rect, string label)
        {
            if (rect == null)
                return 0;

            Vector2 zero = Vector2.zero;
            bool ok = rect.anchorMin == zero && rect.anchorMax == Vector2.one &&
                      rect.offsetMin == zero && rect.offsetMax == zero;
            if (ok)
                return 0;

            Debug.Log("[结构修复] " + label + "：原 " + rect.rect.size.ToString("F0") +
                      " 锚=" + rect.anchorMin + "-" + rect.anchorMax +
                      " → 改为铺满父级。");
            rect.anchorMin = zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = zero;
            rect.offsetMax = zero;
            return 1;
        }

        /// <summary>
        /// 关闭商店内部嵌套 Canvas 的 `overrideSorting`（负责人 2026-09-20 授权）。
        ///
        /// **这是"层级全部跑到商店后面"的真正根因。**
        /// `overrideSorting = true` 的嵌套 Canvas **完全无视层级顺序**，只按 `sortingOrder` 排序。
        /// 实测日志：
        ///   Canvas[.../ShopPage商店/Product page                       override=True order=1005]
        ///   Canvas[.../ShopPage商店/Bottom page function bar/Event Mall/Activity Mall Page override=True order=1002]
        /// 而弹窗都在 order 0/100 —— 于是商店的 Product page **永远渲染在所有弹窗之上**，
        /// 无论兄弟序号是多少。这就解释了为何前几轮改兄弟顺序（SetAsLastSibling / 整体重排栈）
        /// 全都没有效果：overrideSorting 打开时，兄弟序号根本不参与排序。
        ///
        /// 这两个节点是商店**内部**的分页容器，本就该随父级排序，不应越出弹窗栈。
        /// 同时扫描并**报告**其余仍开着 overrideSorting 的 Canvas（不改动，供后续确认）。
        /// </summary>
        private static int ClearNestedCanvasOverride()
        {
            int changes = 0;

            string[] targets =
            {
                "ShopPage商店/Product page",
                "ShopPage商店/Bottom page function bar/Event Mall/Activity Mall Page",
            };

            for (int i = 0; i < targets.Length; i++)
            {
                Transform node = FindByPath(targets[i]);
                if (node == null)
                {
                    Debug.LogWarning("[结构修复] 未找到 " + targets[i] + "，跳过 overrideSorting 关闭。");
                    continue;
                }

                Canvas canvas = node.GetComponent<Canvas>();
                if (canvas == null)
                {
                    Debug.LogWarning("[结构修复] " + targets[i] + " 上没有 Canvas，跳过。");
                    continue;
                }
                if (!canvas.overrideSorting)
                    continue;

                Debug.Log("[结构修复] " + targets[i] + "：overrideSorting=true order=" + canvas.sortingOrder +
                          " → 改为 false（回到随父级排序）。");
                canvas.overrideSorting = false;
                changes++;
            }

            // 体检：把其余仍开着 overrideSorting 的 Canvas 列出来（不改）。
            Canvas[] all = Object.FindObjectsOfType<Canvas>(true);
            StringBuilder list = new StringBuilder();
            int remaining = 0;
            for (int i = 0; i < all.Length; i++)
            {
                Canvas canvas = all[i];
                if (canvas == null || !canvas.overrideSorting)
                    continue;
                if (!canvas.gameObject.scene.IsValid())
                    continue;
                remaining++;
                if (remaining <= 25)
                    list.Append("\n    ! ").Append(ClientUiTrace.Path(canvas.transform))
                        .Append("  order=").Append(canvas.sortingOrder);
            }

            if (remaining == 0)
                Debug.Log("[结构修复] overrideSorting 体检：全场景已无开启该项的 Canvas ✓");
            else
                Debug.LogWarning("[结构修复] overrideSorting 体检：仍有 " + remaining +
                                 " 个 Canvas 开启该项（本轮未改，供确认是否会压住弹窗）：" + list);
            return changes;
        }

        /// <summary>
        /// 回滚 `Purchase Success Page` 的矩形（2026-09-20）。
        ///
        /// **背景：这里曾是我的误改，必须还原。**
        /// 我一度根据运行时 `rect.rect.size` 的瞬态读数，判断该节点是"216 宽的窄竖带"，
        /// 并向负责人描述为缺陷、取得"按全屏修正"的授权。实际场景值是
        /// anchor=(0,0)-(1,1)、offsetMin=(263.63,-283.78)、offsetMax=(-263.63,283.78)，
        /// 即**画布中一个居中的 752×1527 面板**（画布约 1279×2219，左右各留 264）。
        ///
        /// 负责人明确了关键事实：**`Purchase Success Page` 本身就是那块面板**，
        /// 不存在"全屏底图 + 面板"两层。因此把它铺满整屏会**盖住身后的购买界面与商店**，
        /// 负责人反馈"看不到三层"即由此而来。
        ///
        /// 结论：面板式弹窗本就该是居中面板。普查报告里"非全屏 ✘"的判定对面板式弹窗**过宽**，
        /// 不能一律当成缺陷。本方法只负责把被改坏的值还原。
        /// </summary>
        private static int FixSuccessPageRect()
        {
            GameObject page = FindSceneObject("Purchase Success Page", true);
            if (page == null)
            {
                Debug.LogWarning("[结构修复] 未找到 Purchase Success Page，跳过矩形还原。");
                return 0;
            }

            RectTransform rect = page.GetComponent<RectTransform>();
            if (rect == null)
                return 0;

            // 负责人已明确：成功页**应为全屏覆盖**（"成功页逻辑应该是全屏覆盖"）。
            // 早前我把它改成全屏、又因误判"看不到三层"回滚过；现按要求恢复为全屏。
            Vector2 zero = Vector2.zero;
            int changes = 0;

            Debug.Log("[结构修复] Purchase Success Page 当前 rect：offsetMin=" + rect.offsetMin +
                      " offsetMax=" + rect.offsetMax + " size=" + rect.rect.size);
            rect.anchorMin = zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = zero;
            rect.offsetMax = zero;
            Debug.Log("[结构修复] Purchase Success Page 已设为全屏覆盖（锚点 (0,0)-(1,1)、offset 归零）。");

            // 矩形全屏还不够：该页**根节点上没有任何 Graphic**（普查：根组件只有
            // GraphicRaycaster + ClientUiPopup），所以既没有东西铺满屏幕、也没有东西接收点击——
            // 表现为"购买界面从底下透出来"+"点任意处无反应"，两个症状同源。
            // 这里补一块全屏底图：既做视觉覆盖，又当整屏的接点击面。
            Image backdrop = page.GetComponent<Image>();
            if (backdrop == null)
            {
                backdrop = page.AddComponent<Image>();
                backdrop.sprite = null;
                backdrop.type = Image.Type.Simple;
                Debug.Log("[结构修复] Purchase Success Page 已补全屏底图 Image（颜色可自行调整）。");
                changes++;
            }
            if (!backdrop.raycastTarget)
            {
                backdrop.raycastTarget = true;
                changes++;
            }
            // 半透明黑底：盖住身后内容、视觉上像"成功页覆盖"，同时保留一点层次感。
            // 负责人如需改成不透明白底或调透明度，直接在 Inspector 改 color 即可。
            if (backdrop.color.a < 0.01f)
                backdrop.color = new Color(0f, 0f, 0f, 0.6f);

            return changes;
        }

        /// <summary>
        /// 通用列表收敛：把「场景内联展开的列表」改为「1 个模板 + 运行时生成」（负责人 2026-09-20 全权授权）。
        ///
        /// **前提**：容器必须自带布局组件（GridLayoutGroup / VerticalLayoutGroup 等）来自动排列克隆体。
        /// 否则原本由布局排布的卡会全部叠在同一坐标——那才是真的改布局。
        /// 本方法会**先验证**该前提，不满足时拒绝收敛并明确告警。
        ///
        /// 动作：保留第 1 个项并重命名为模板名、禁用；删除其余；把模板与容器写入目标组件。幂等。
        /// </summary>
        private static int ConvergeList(string containerPath, string itemPrefix, string templateName,
                                        Object target, string templateField, string rootField,
                                        string templatePrefix = null)
        {
            Transform container = FindByPath(containerPath);
            if (container == null)
                container = FindByPathTrimmed(containerPath);   // 兜底：节点名可能带首尾空格
            if (container == null)
            {
                Debug.LogWarning("[结构修复] 未找到列表容器，跳过收敛：" + containerPath);
                return 0;
            }

            // 前提校验：必须有布局组件。
            bool hasLayout = container.GetComponent<GridLayoutGroup>() != null ||
                             container.GetComponent<VerticalLayoutGroup>() != null ||
                             container.GetComponent<HorizontalLayoutGroup>() != null;
            if (!hasLayout)
            {
                Debug.LogWarning("[结构修复] 拒绝收敛 " + containerPath +
                                 "：容器没有 GridLayoutGroup/VerticalLayoutGroup/HorizontalLayoutGroup，" +
                                 "克隆体会叠在同一坐标（等于改变布局）。请先在 Unity 中确认排布方式。");
                return 0;
            }

            List<Transform> items = new List<Transform>();
            for (int i = 0; i < container.childCount; i++)
            {
                Transform child = container.GetChild(i);
                if (child.name.StartsWith(itemPrefix, System.StringComparison.Ordinal) ||
                    child.name == templateName)
                    items.Add(child);
            }
            if (items.Count == 0)
            {
                Debug.LogWarning("[结构修复] " + containerPath + " 下没有匹配「" + itemPrefix + "」的项，跳过。");
                return 0;
            }

            int changes = 0;

            // 选模板：默认第一项；给了 TemplatePrefix 就优先选它（避免把"未解锁"项当模板，见 ListTarget.TemplatePrefix）。
            Transform template = items[0];
            if (!string.IsNullOrEmpty(templatePrefix))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].name.StartsWith(templatePrefix, System.StringComparison.Ordinal))
                    {
                        template = items[i];
                        break;
                    }
                }
                if (template != items[0])
                    Debug.Log("[结构修复] 模板按前缀「" + templatePrefix + "」选中：" + template.name);
            }

            // 删除除模板外的全部内联项（含"未解锁"等其它状态节点）。
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == template)
                    continue;
                Debug.Log("[结构修复] 删除内联项 " + items[i].name + "（改为运行时按数据生成）。");
                Object.DestroyImmediate(items[i].gameObject, true);
                changes++;
            }

            if (template.name != templateName)
            {
                Debug.Log("[结构修复] 列表模板重命名：" + template.name + " -> " + templateName);
                template.name = templateName;
                changes++;
            }
            if (template.gameObject.activeSelf)
            {
                template.gameObject.SetActive(false);
                Debug.Log("[结构修复] 列表模板已禁用（仅作克隆源，永不显示）。");
                changes++;
            }

            if (target != null)
            {
                WireObjectReference(target, templateField, template.gameObject);
                WireObjectReference(target, rootField, container as RectTransform);
                Debug.Log("[结构修复] " + target.GetType().Name + "." + templateField + " <- " + templateName +
                          "，" + rootField + " <- " + container.name + "（改走模板生成模式）。");
            }
            else
            {
                Debug.LogWarning("[结构修复] " + containerPath + " 的目标组件缺失，模板未接线。");
            }
            return changes;
        }

        /// <summary>
        /// 一个待收敛的列表目标。
        ///
        /// **登记规则（重要）**：只有 View **已经实现模板生成**的页面才能登记在此。
        /// 否则删掉内联实例后页面只剩 1 个禁用模板，等于可见的功能回归。
        /// 因此这里是一份**显式白名单**，而不是"扫描到就收敛"。
        /// </summary>
        private struct ListTarget
        {
            public string PageName;
            public string ContainerPath;
            public string ItemPrefix;
            public string TemplateName;
            public string TemplateField;
            public string RootField;

            /// <summary>
            /// **模板前缀**（可空）：容器里第一个匹配项未必是想要的模板 ——
            /// 实测 ProfilePage 的收藏品列表**第一项常是"未解锁"节点**（如 `徽章未解锁Image (3)`），
            /// 直接取 `items[0]` 会拿未解锁项当模板。给了本字段就优先选**名字以此为前缀**的那一项当模板。
            /// </summary>
            public string TemplatePrefix;

            /// <summary>
            /// **验收闸门**：只有负责人在 Unity 里 Play 验收通过后才置 true。
            ///
            /// 为什么需要它：登记进白名单只表示"该页代码侧已具备模板生成能力"，
            /// 并不表示"允许现在动场景"。若登记即生效，之后任何一次 RepairAll
            /// （哪怕是为别的修复而跑）都会顺手删掉该页的内联实例，
            /// 等于**绕过负责人的逐页验收**。所以未验收的页面必须停在 false。
            /// </summary>
            public bool Approved;

            public ListTarget(string pageName, string containerPath, string itemPrefix,
                              string templateName, string templateField, string rootField, bool approved,
                              string templatePrefix = null)
            {
                PageName = pageName;
                ContainerPath = containerPath;
                ItemPrefix = itemPrefix;
                TemplateName = templateName;
                TemplateField = templateField;
                RootField = rootField;
                Approved = approved;
                TemplatePrefix = templatePrefix;
            }
        }

        /// <summary>
        /// 列表收敛白名单。
        ///
        /// 新增一页的流程（见 MODULE.md §11）：先给该页 View 加模板生成（含内联回退）→ 编译通过 →
        /// 登记到此处并保持 `Approved = false` → **向负责人说明并取得确认** → 改 `Approved = true` → 跑 RepairAll。
        /// </summary>
        private static readonly ListTarget[] ListTargets =
        {
            // 已执行场景写入（当时经负责人全权授权），等待 Play 验收。
            new ListTarget("ShopPage商店",
                "ShopPage商店/Product page/展示卡牌Scroll View/Viewport/Content",
                "道具卡牌", "道具卡牌Item", "_cardTemplate", "_cardListRoot", true),

            new ListTarget("MailPage邮件",
                "MailPage邮件/Email come/邮件Scroll View/Viewport/Content",
                "邮件prefab", "邮件Item", "_cardTemplate", "_cardListRoot", true),

            // 代码侧早已就绪（ClientHeroPage 模板支持 + 修正 heroes[index % count] 取模填充），
            // 2026-09-20 负责人指示"继续列表收敛" → 打开验收闸门执行场景写入。
            new ListTarget("HeroPage英雄",
                "HeroPage英雄/HeroSubPage/MiddleCanvas/Canvas/Scroll View/Viewport/Content",
                "角色卡牌Button-final", "英雄卡Item", "_cardTemplate", "_cardListRoot", true),

            // FormationPage编队 选卡列表：负责人 2026-09-20 明确“写”。
            // 前提（都已就绪）：① `WireFormationCards()` 给卡牌补 `ClientHeroCard` + `卡牌名称` 文本节点；
            //                  ② `ClientFormationPage` 已实现模板生成，模板名与本条 TemplateName 一致；
            //                  ③ 容器带 GridLayoutGroup（普查报告：cell 204x326 / 固定 3 列）。
            new ListTarget("FormationPage编队",
                "FormationPage编队/Scroll View/Viewport/Content",
                "角色卡牌Button-final", "编队英雄卡Item", "_cardTemplate", "_cardListRoot", true),

            // ---- ProfilePage个人中心 的 5 个收藏品列表（负责人 2026-09-20 **授权全部写入**）----
            // ⚠️ 两个实测陷阱，都靠 TemplatePrefix / 前缀规避：
            //   ① 这些列表的**第一项常是"未解锁"节点**（`徽章未解锁Image (3)`）⇒ 必须指定已解锁项当模板；
            //   ② 命名大小写不一致（`徽章已解锁image` 是小写 i）与特殊名（`称号已解锁-无image`）。
            // ItemPrefix 用"品类名"，这样同容器的未解锁项也会被删掉（只留 1 个模板）。
            new ListTarget("ProfilePage个人中心",
                "ProfilePage个人中心/PageContentRoot/Panel_Personalize/SubContentRoot/Content_Avatar/头像Scroll View/Viewport/Content",
                "头像", "头像Item", "_avatarTemplate", "_avatarRoot", true, "头像已解锁"),
            new ListTarget("ProfilePage个人中心",
                "ProfilePage个人中心/PageContentRoot/Panel_Personalize/SubContentRoot/Content_Info/徽章Scroll View/Viewport/Content",
                "徽章", "徽章Item", "_badgeTemplate", "_badgeRoot", true, "徽章已解锁"),
            new ListTarget("ProfilePage个人中心",
                "ProfilePage个人中心/PageContentRoot/Panel_Personalize/SubContentRoot/Content_Widget/铭牌Scroll View/Viewport/Content",
                "小铭牌", "铭牌Item", "_nameplateTemplate", "_nameplateRoot", true, "小铭牌已解锁"),
            new ListTarget("ProfilePage个人中心",
                "ProfilePage个人中心/PageContentRoot/Panel_Personalize/SubContentRoot/Content_Frame/头像框Scroll View/Viewport/Content",
                "头像", "头像框Item", "_frameTemplate", "_frameRoot", true, "头像已解锁"),
            new ListTarget("ProfilePage个人中心",
                "ProfilePage个人中心/PageContentRoot/Panel_Personalize/SubContentRoot/Content_Title/称号Scroll View/Viewport/Content",
                "称号", "称号Item", "_titleTemplate", "_titleRoot", true, "称号已解锁"),
        };

        /// <summary>批量收敛白名单内**已验收**的列表容器。</summary>
        private static int ConvergeKnownLists()
        {
            int changes = 0;
            for (int i = 0; i < ListTargets.Length; i++)
            {
                ListTarget target = ListTargets[i];

                if (!target.Approved)
                {
                    Debug.Log("[结构修复] 跳过 " + target.PageName +
                              "：尚未通过负责人验收（ListTarget.Approved=false）。" +
                              "代码侧已就绪，确认后置 true 即可执行。");
                    continue;
                }

                GameObject page = FindSceneObject(target.PageName, true);
                if (page == null)
                {
                    Debug.LogWarning("[结构修复] 未找到页面 " + target.PageName + "，跳过其列表收敛。");
                    continue;
                }

                // 用反射取字段描述符不便，这里直接按已知类型分派；新增页面时在此补一个分支。
                Object component = null;
                if (target.PageName == "ShopPage商店")
                    component = page.GetComponent<ClientShopPage>();
                else if (target.PageName == "MailPage邮件")
                    component = page.GetComponent<ClientMailPage>();
                else if (target.PageName == "HeroPage英雄")
                    component = page.GetComponent<ClientHeroPage>();
                else if (target.PageName == "FormationPage编队")
                    component = page.GetComponent<ClientFormationPage>();
                else if (target.PageName == "ProfilePage个人中心")
                    component = page.GetComponent<ClientProfilePage>();

                changes += ConvergeList(target.ContainerPath, target.ItemPrefix, target.TemplateName,
                                        component, target.TemplateField, target.RootField, target.TemplatePrefix);
            }
            return changes;
        }

        /// <summary>
        /// 撤掉全局弹窗遮罩（负责人 2026-09-20 决定）。
        ///
        /// 为什么撤：全局遮罩只挡得住"下层界面"，却解决不了更根本的问题——
        /// 弹窗根节点尺寸不是全屏、列表与数据不对应、交互节点自身没有命中面。
        /// 实测还暴露两个新问题：遮罩会盖住栈顶弹窗（购买界面点不动）、
        /// 以及栈未清空时遮罩残留把整个界面锁死。
        ///
        /// 后续改为**每个页面根自带 CanvasGroup，由它控制模态**（见 Assets/Client/MODULE.md）。
        /// </summary>
        private static int RemovePopupMask(Canvas canvas)
        {
            Transform popupLayer = canvas.transform.Find("PopupLayer");
            if (popupLayer == null)
                return 0;

            int changes = 0;

            ClientPopupService service = popupLayer.GetComponent<ClientPopupService>();
            if (service != null)
            {
                SerializedObject so = new SerializedObject(service);
                SerializedProperty property = so.FindProperty("_maskRoot");
                if (property != null && property.objectReferenceValue != null)
                {
                    property.objectReferenceValue = null;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    Debug.Log("[结构修复] ClientPopupService._maskRoot 已清空（撤掉全局遮罩）。");
                    changes++;
                }
            }

            Transform mask = popupLayer.Find("弹窗遮罩");
            if (mask != null)
            {
                Debug.Log("[结构修复] 已删除全局遮罩节点 弹窗遮罩。");
                Object.DestroyImmediate(mask.gameObject, true);
                changes++;
            }
            else if (changes == 0)
            {
                Debug.Log("[结构修复] 未发现全局遮罩，无需撤除。");
            }
            return changes;
        }

        /// <summary>
        /// 全屏弹窗遮罩（负责人已明确授权新建该节点）。
        ///
        /// 为什么必须有它：全屏弹窗打开时，主页底部图标仍然可点 → 点击穿透到大厅
        /// → 每点一次就往弹窗栈上再叠一层，用户只看到最上面那层，
        /// 等上层关掉下面那些"早就打开了"的弹窗才依次显现，表现为"延迟弹出"。
        /// 实测日志：栈深一路 1→2→3→4→5→6，全程 `ReturnHome` 0 次、帧时仅 2ms。
        ///
        /// 该节点只做一件事：全屏铺满 + raycastTarget，挡住下层输入（半透明黑底兼顾视觉）。
        /// 渲染位置由 ClientPopupService.ApplyMask 动态维护在栈顶弹窗之下。
        /// </summary>
        private static int EnsurePopupMask(Canvas canvas)
        {
            Transform popupLayer = canvas.transform.Find("PopupLayer");
            if (popupLayer == null)
            {
                Debug.LogWarning("[结构修复] 未找到 PopupLayer，跳过弹窗遮罩。");
                return 0;
            }

            int changes = 0;
            const string MaskName = "弹窗遮罩";
            Transform existing = popupLayer.Find(MaskName);
            GameObject mask;
            if (existing != null)
            {
                mask = existing.gameObject;
            }
            else
            {
                mask = new GameObject(MaskName, typeof(RectTransform), typeof(Image));
                mask.transform.SetParent(popupLayer, false);
                mask.SetActive(false);
                Debug.Log("[结构修复] 已在 PopupLayer 下新建全屏弹窗遮罩节点 \"" + MaskName + "\"。");
                changes++;
            }

            // 全屏铺满：遮罩的"尺寸"就是父级，不存在需要保护的美术排版。
            RectTransform rect = mask.GetComponent<RectTransform>();
            if (rect == null)
                rect = mask.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = mask.GetComponent<Image>();
            if (image == null)
                image = mask.AddComponent<Image>();
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = new Color(0f, 0f, 0f, 0.45f);
            image.raycastTarget = true;

            // 接到弹窗服务上。
            ClientPopupService service = popupLayer.GetComponent<ClientPopupService>();
            if (service != null)
            {
                WireObjectReference(service, "_maskRoot", mask);
                Debug.Log("[结构修复] ClientPopupService._maskRoot <- " + MaskName);
            }
            else
            {
                Debug.LogWarning("[结构修复] PopupLayer 上没有 ClientPopupService，遮罩未接线。");
            }
            return changes;
        }

        // ------------------------------------------------------------------
        // 已确认的 S6 计划（数据表）
        // ------------------------------------------------------------------

        /// <summary>一条层级迁移计划。</summary>
        private struct LayerMove
        {
            public string Node;
            public string TargetLayer;
            public ClientUiPageId PageId;
            /// <summary>true = 参与 pageId 路由（一级全屏界面）；false = 仅按引用打开（二级弹窗）。</summary>
            public bool Routed;
            public LayerMove(string node, string layer, ClientUiPageId pageId, bool routed)
            {
                Node = node; TargetLayer = layer; PageId = pageId; Routed = routed;
            }
        }

        /// <summary>
        /// 按负责人确认的架构：PagesLayer 只保留 MainPage主页（大厅），
        /// 其余全部界面都是 PopupLayer 的全屏弹窗（可叠加、可关闭、带输入阻断）。
        /// _pageId 仅在 Routed=true 时有效；二级弹窗由调用方持引用直接 Open，不占 pageId。
        /// </summary>
        private static readonly LayerMove[] LayerMoves =
        {
            // ── 一级全屏界面（主页一级导航可达，按 pageId 路由）──
            new LayerMove("ProfilePage个人中心",                              "PopupLayer", ClientUiPageId.Profile,           true),
            new LayerMove("HeroPage英雄",                                    "PopupLayer", ClientUiPageId.Hero,              true),
            new LayerMove("HeroDetailPage英雄详情主页",                        "PopupLayer", ClientUiPageId.HeroDetail,        true),
            new LayerMove("ShopPage商店",                                    "PopupLayer", ClientUiPageId.Shop,              true),
            new LayerMove("MailPage邮件",                                    "PopupLayer", ClientUiPageId.Mail,              true),
            new LayerMove("RankPage排行榜",                                  "PopupLayer", ClientUiPageId.Rank,              true),
            new LayerMove("ActivityPage活动",                                "PopupLayer", ClientUiPageId.Activity,          true),
            new LayerMove("FormationPage编队",                               "PopupLayer", ClientUiPageId.Formation,         true),
            new LayerMove("Complete Guide Event Page全图鉴活动",               "PopupLayer", ClientUiPageId.CompleteGuideEvent, true),
            // 联系客服：负责人确认为主页 BottomFunctionIcon 弹出的弹窗（归 PopupLayer）
            new LayerMove("联系客服Page",                                     "PopupLayer", ClientUiPageId.Home,              false),

            // ── 二级弹窗（由一级界面内按钮打开，仅按引用 Open）──
            new LayerMove("Product Purchase Interface",                      "PopupLayer", ClientUiPageId.Home, false),
            new LayerMove("Purchase Success Page",                           "PopupLayer", ClientUiPageId.Home, false),
            new LayerMove("Exchange code interface",                         "PopupLayer", ClientUiPageId.Home, false),
            new LayerMove("Historical Order Interface",                      "PopupLayer", ClientUiPageId.Home, false),
            new LayerMove("Pending shipmentCanvas",                          "PopupLayer", ClientUiPageId.Home, false),
            new LayerMove("Email Details Page（Have）",                       "PopupLayer", ClientUiPageId.Home, false),
            new LayerMove("Email Details Page (No)",                         "PopupLayer", ClientUiPageId.Home, false),
            new LayerMove("Success Receipt Interface",                       "PopupLayer", ClientUiPageId.Home, false),
            new LayerMove("Player lineup",                                   "PopupLayer", ClientUiPageId.Home, false),
        };

        /// <summary>需要挂上页面组件（但原本没有脚本）的节点。</summary>
        private static readonly string[] NeedsFormationScript = { "FormationPage编队" };
        private static readonly string[] NeedsCompleteGuideScript = { "Complete Guide Event Page全图鉴活动" };

        /// <summary>
        /// 已确认删除的孤儿页脚本（场景引用数均为 0，功能已由其它控制器接管）。
        /// 删除时须同步清理旧 Editor 工具中引用它们的 pass，否则编译失败。
        /// </summary>
        private static readonly string[] ObsoleteScripts =
        {
            "ClientGachaPage",        // 扭蛋：暂不做，只留主页入口
            "ClientMarblePage",       // 弹珠：未实现
            "ClientNoticePage",       // 公告：未实现
            "ClientBackpackPage",     // 背包：未做
            "ClientHeroEnhancePage",  // 英雄培养：无任何 UI
            "ClientBadgePage",        // 徽章：已由 Panel_Personalize/SubContentRoot/Content_Info 接管
            "ClientBadgeButton",      // 上一项的附属适配器
        };

        /// <summary>只读：输出 S6 计划表与每个节点的当前状态/迁移安全性，不修改场景。</summary>
        [MenuItem("Client/结构修复/3. S6 计划报告（只读）", false, 12)]
        public static void PlanReport()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========== S6 计划：层级迁移 ===========");
            sb.AppendLine("节点".PadRight(38) + "目标层".PadRight(13) + "pageId".PadRight(20) + "路由  当前父层        锚点  迁移影响");
            for (int i = 0; i < LayerMoves.Length; i++)
            {
                LayerMove move = LayerMoves[i];
                GameObject node = FindSceneObject(move.Node, true);
                if (node == null)
                {
                    sb.AppendLine(move.Node.PadRight(38) + move.TargetLayer.PadRight(13) + "(节点不存在，跳过)");
                    continue;
                }
                RectTransform rect = node.GetComponent<RectTransform>();
                bool stretched = rect != null && rect.anchorMin != rect.anchorMax;
                ClientUiPage uiPage = node.GetComponent<ClientUiPage>();
                ClientUiPopup uiPopup = node.GetComponent<ClientUiPopup>();
                string parentName = node.transform.parent != null ? node.transform.parent.name : "(根)";
                sb.AppendLine(
                    move.Node.PadRight(38) +
                    move.TargetLayer.PadRight(13) +
                    (move.Routed ? move.PageId.ToString() : "(不路由)").PadRight(20) +
                    (move.Routed ? "是  " : "否  ") +
                    parentName.PadRight(16) +
                    (stretched ? "拉伸 " : "非拉伸") + "  " +
                    (stretched ? "★ 跨父级会改变实际尺寸" : "布局不变") +
                    "  [" + (uiPage != null ? "ClientUiPage=" + uiPage.PageId : "无 ClientUiPage") +
                    (uiPopup != null ? ", 已有 ClientUiPopup" : "") + "]");
            }

            sb.AppendLine();
            sb.AppendLine("=========== S6 计划：删除孤儿脚本 ===========");
            for (int i = 0; i < ObsoleteScripts.Length; i++)
            {
                MonoBehaviour[] found = FindSceneComponentsByTypeName(ObsoleteScripts[i]);
                sb.AppendLine("  " + ObsoleteScripts[i].PadRight(24) + " 场景引用数=" + found.Length);
            }

            sb.AppendLine();
            sb.AppendLine("=========== S6 计划：补挂脚本 ===========");
            for (int i = 0; i < NeedsFormationScript.Length; i++)
                sb.AppendLine("  " + NeedsFormationScript[i] + " -> ClientFormationPage + ClientUiPopup(Formation, routed)");
            for (int i = 0; i < NeedsCompleteGuideScript.Length; i++)
                sb.AppendLine("  " + NeedsCompleteGuideScript[i] + " -> ClientCompleteGuideEventPage + ClientUiPopup(CompleteGuideEvent, routed)");

            sb.AppendLine();
            sb.AppendLine("=========== S6 计划：其它 ===========");
            sb.AppendLine("  删除节点 BackpackPage英雄详情装备页（旧英雄详情实现）");
            sb.AppendLine("  移除已禁用的旧导航器组件 ClientPageNavigator");
            sb.AppendLine("  安装 ClientPopupService(PopupLayer) / ClientSystemFeedback(SystemLayer) / ClientShellController(ClientCanvas)");

            Debug.Log(sb.ToString());
            Finish(0);
        }

        // ------------------------------------------------------------------
        // 盘点报告
        // ------------------------------------------------------------------

        private static string BuildReport(Scene scene)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("================ ClientShell 结构盘点 ================");
            sb.AppendLine("场景：" + scene.path);
            sb.AppendLine();

            Canvas canvas = FindClientCanvas();
            if (canvas == null)
            {
                sb.AppendLine("!! 未找到 ClientCanvas");
                return sb.ToString();
            }

            string[] layers = { "PagesLayer", "PopupLayer", "SystemLayer" };
            for (int i = 0; i < layers.Length; i++)
            {
                Transform layer = canvas.transform.Find(layers[i]);
                sb.AppendLine("── " + layers[i] + " ──");
                if (layer == null)
                {
                    sb.AppendLine("   (缺失)");
                    continue;
                }
                AppendRect(sb, layer.GetComponent<RectTransform>(), "   layer");
                for (int c = 0; c < layer.childCount; c++)
                {
                    Transform child = layer.GetChild(c);
                    ClientUiPage uiPage = child.GetComponent<ClientUiPage>();
                    ClientUiPopup uiPopup = child.GetComponent<ClientUiPopup>();
                    List<string> tags = new List<string>();
                    if (uiPage != null) tags.Add("pageId=" + uiPage.PageId);
                    if (uiPopup != null) tags.Add("popup" + (uiPopup.BlocksInput ? "(blocking)" : ""));
                    List<string> views = new List<string>();
                    MonoBehaviour[] behaviours = child.GetComponents<MonoBehaviour>();
                    for (int b = 0; b < behaviours.Length; b++)
                        if (behaviours[b] is IClientPageView || behaviours[b] is IClientPopupView)
                            views.Add(behaviours[b].GetType().Name);
                    if (views.Count > 0) tags.Add("view=" + string.Join(",", views.ToArray()));
                    sb.AppendLine("   - " + child.name + "   [" + string.Join(" | ", tags.ToArray()) + "]");
                }
                sb.AppendLine();
            }

            // 重复 pageId 检测
            Dictionary<ClientUiPageId, List<string>> byId = new Dictionary<ClientUiPageId, List<string>>();
            ClientUiPage[] allPages = Object.FindObjectsOfType<ClientUiPage>(true);
            for (int i = 0; i < allPages.Length; i++)
            {
                if (allPages[i] == null || !allPages[i].gameObject.scene.IsValid())
                    continue;
                if (!byId.ContainsKey(allPages[i].PageId))
                    byId[allPages[i].PageId] = new List<string>();
                byId[allPages[i].PageId].Add(allPages[i].gameObject.name);
            }
            sb.AppendLine("── 重复 pageId 检测 ──");
            bool anyDuplicate = false;
            foreach (KeyValuePair<ClientUiPageId, List<string>> pair in byId)
            {
                if (pair.Value.Count > 1)
                {
                    anyDuplicate = true;
                    sb.AppendLine("   !! " + pair.Key + " 被 " + pair.Value.Count + " 个节点占用：" +
                                  string.Join(" / ", pair.Value.ToArray()));
                }
            }
            if (!anyDuplicate)
                sb.AppendLine("   (无重复)");

            sb.AppendLine();
            sb.AppendLine("── 未注册为页面的 Page 脚本（可能缺少 ClientUiPage 或缺少节点）──");
            string[] expectedViews =
            {
                "ClientHomePage", "ClientProfilePage", "ClientHomeShowcasePage", "ClientHeroPage",
                "ClientHeroDetailPage", "ClientBackpackPage", "ClientGachaPage", "ClientShopPage",
                "ClientMailPage", "ClientRankPage", "ClientActivityPage", "ClientTaskPage",
                "ClientNoticePage", "ClientBadgePage", "ClientHeroEnhancePage", "ClientFormationPage",
                "ClientMarblePage", "ClientCompleteGuideEventPage",
            };
            for (int i = 0; i < expectedViews.Length; i++)
            {
                MonoBehaviour[] found = FindSceneComponentsByTypeName(expectedViews[i]);
                if (found.Length == 0)
                {
                    sb.AppendLine("   (无实例) " + expectedViews[i]);
                    continue;
                }
                for (int f = 0; f < found.Length; f++)
                {
                    bool underPages = IsUnderLayer(found[f].transform, "PagesLayer");
                    bool underPopup = IsUnderLayer(found[f].transform, "PopupLayer");
                    if (!underPages && !underPopup)
                        sb.AppendLine("   (不在三层内) " + expectedViews[i] + " @ " + GetPath(found[f].transform));
                }
            }

            sb.AppendLine("=====================================================");
            return sb.ToString();
        }

        // ------------------------------------------------------------------
        // 工具方法
        // ------------------------------------------------------------------

        private static Canvas FindClientCanvas()
        {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
                if (canvases[i] != null && canvases[i].gameObject.name == "ClientCanvas")
                    return canvases[i];
            return null;
        }

        private static GameObject FindSceneObject(string name, bool includeInactive)
        {
            GameObject[] objects = Object.FindObjectsOfType<GameObject>(includeInactive);
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null && objects[i].name == name && objects[i].scene.IsValid())
                    return objects[i];
            }
            return null;
        }

        private static MonoBehaviour[] FindSceneComponentsByTypeName(string typeName)
        {
            List<MonoBehaviour> result = new List<MonoBehaviour>();
            MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null || !behaviours[i].gameObject.scene.IsValid())
                    continue;
                if (behaviours[i].GetType().Name == typeName)
                    result.Add(behaviours[i]);
            }
            return result.ToArray();
        }

        private static bool IsUnderLayer(Transform transform, string layerName)
        {
            Transform current = transform;
            while (current != null)
            {
                if (current.name == layerName)
                    return true;
                current = current.parent;
            }
            return false;
        }

        private static int CountDescendants(Transform root)
        {
            int count = 0;
            for (int i = 0; i < root.childCount; i++)
                count += 1 + CountDescendants(root.GetChild(i));
            return count;
        }

        private static void AppendRect(StringBuilder sb, RectTransform rect, string label)
        {
            if (rect == null)
                return;
            sb.AppendLine(label + " rect: ancMin=" + rect.anchorMin + " ancMax=" + rect.anchorMax +
                          " size=" + rect.sizeDelta + " pos=" + rect.anchoredPosition + " pivot=" + rect.pivot);
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        private static void WireObjectReference(Object target, string fieldName, Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogWarning("[结构修复] 未找到序列化字段 " + fieldName + "，跳过接线。");
                return;
            }
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Fail(string message)
        {
            Debug.LogError("[结构修复] " + message);
            Finish(2);
        }

        /// <summary>
        /// 手动从菜单运行时绝不能退出编辑器；只在批处理模式下落定退出码。
        /// </summary>
        private static void Finish(int exitCode)
        {
            if (Application.isBatchMode)
                EditorApplication.Exit(exitCode);
        }
    }
}
