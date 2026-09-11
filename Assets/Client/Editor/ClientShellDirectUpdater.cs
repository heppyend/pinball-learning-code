using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using Pinball.Client.UI;

namespace Pinball.Client.Editor
{
    /// <summary>
    /// 仅供命令行执行的 ClientShell 场景增量写入器；不提供菜单入口，且不覆盖人工节点。
    /// </summary>
    public static class ClientShellDirectUpdater
    {
        private const string ScenePath = "Assets/Client/Scenes/ClientShell.unity";
        private const string SpriteRoot = "Assets/Client/UI/HeroDetailPage/Sprites";
        private const string MainSpriteRoot = "Assets/Client/UI/MainPage/Sprites";
        private const string HeroSpriteRoot = "Assets/Client/UI/HeroPage/Sprites";
        private const string BadgeSpriteRoot = "Assets/Client/UI/BadgePage/Sprites";

        // 清理本次误生成的节点；不触碰负责人已有的“加载Page”及其子节点。
        public static void RemoveAccidentalLoadingPage()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            Transform pagesLayer = canvas != null ? canvas.transform.Find("PagesLayer") : null;
            Transform accidental = pagesLayer != null ? pagesLayer.Find("LoadingPage") : null;
            if (accidental != null) Object.DestroyImmediate(accidental.gameObject);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("[Client] 已移除误生成的 LoadingPage 节点，未修改已有加载Page。");
        }

        public static void AttachLoadingScriptsToExistingPage()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject pageObject = GameObject.Find("加载Page");
            if (pageObject == null)
            {
                Debug.LogError("[Client] 未找到负责人已有的 加载Page，未修改场景。");
                EditorApplication.Exit(2);
                return;
            }
            ClientLoadingDotsAnimator dots = pageObject.GetComponent<ClientLoadingDotsAnimator>();
            if (dots == null) dots = pageObject.AddComponent<ClientLoadingDotsAnimator>();
            ClientLoadingPage page = pageObject.GetComponent<ClientLoadingPage>();
            if (page == null) page = pageObject.AddComponent<ClientLoadingPage>();
            page.AutoBindFromHierarchy();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("[Client] 已仅为已有 加载Page 挂载加载脚本，未创建或调整 UI 节点。");
        }

        public static void ApplyHeroDetailPage()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            ClientPageNavigator navigator = Object.FindObjectOfType<ClientPageNavigator>();
            if (canvas == null || navigator == null)
            {
                Debug.LogError("[Client] ClientShell 缺少 ClientCanvas 或 ClientPageNavigator，未写入英雄详情页。");
                EditorApplication.Exit(2);
                return;
            }

            EnsureSprites();
            ClientHeroDetailPage page = Object.FindObjectOfType<ClientHeroDetailPage>(true);
            if (page == null)
                page = BuildPage(canvas.transform, navigator);

            navigator.ConfigureHeroDetailPage(page);
            page.Hide();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] HeroDetailPage 已直接写入 ClientShell 场景。");
        }

        public static void ApplyBackpackPage()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            ClientPageNavigator navigator = Object.FindObjectOfType<ClientPageNavigator>();
            if (canvas == null || navigator == null)
            {
                Debug.LogError("[Client] ClientShell 缺少 ClientCanvas 或 ClientPageNavigator，未写入背包页。");
                EditorApplication.Exit(2);
                return;
            }

            ClientBackpackPage page = Object.FindObjectOfType<ClientBackpackPage>(true);
            if (page == null)
                page = BuildBackpackPage(canvas.transform, navigator);
            navigator.ConfigureBackpackPage(page);
            page.Hide();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] BackpackPage 已直接写入 ClientShell 场景。");
        }

        public static void ApplyHeroVisualPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClientHeroPage heroPage = Object.FindObjectOfType<ClientHeroPage>(true);
            if (heroPage == null)
            {
                Debug.LogError("[Client] 未找到 HeroPage，未写入英雄卡视觉节点。");
                EditorApplication.Exit(2);
                return;
            }

            EnsureSprites(HeroSpriteRoot);
            AddHeroCardVisuals(heroPage.transform.Find("OwnedHeroCard"), true);
            AddHeroCardVisuals(heroPage.transform.Find("LockedHeroCard"), false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 英雄卡视觉状态已直接写入 ClientShell 场景。");
        }

        public static void ApplyPrimaryPages()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            ClientPageNavigator navigator = Object.FindObjectOfType<ClientPageNavigator>();
            if (canvas == null || navigator == null)
            {
                Debug.LogError("[Client] ClientShell 缺少 ClientCanvas 或 ClientPageNavigator，未写入一级页面。");
                EditorApplication.Exit(2);
                return;
            }

            List<ClientPrimaryPage> pages = new List<ClientPrimaryPage>();
            AddPrimaryPage(canvas.transform, navigator, pages, "GachaPage", ClientHomeDestination.Gacha);
            AddPrimaryPage(canvas.transform, navigator, pages, "ShopPage", ClientHomeDestination.Shop);
            AddPrimaryPage(canvas.transform, navigator, pages, "MailPage", ClientHomeDestination.Mail);
            AddPrimaryPage(canvas.transform, navigator, pages, "RankPage", ClientHomeDestination.Rank);
            AddPrimaryPage(canvas.transform, navigator, pages, "ActivityPage", ClientHomeDestination.Activity);
            AddPrimaryPage(canvas.transform, navigator, pages, "TaskPage", ClientHomeDestination.Task);
            AddPrimaryPage(canvas.transform, navigator, pages, "NoticePage", ClientHomeDestination.Notice);
            navigator.ConfigurePrimaryPages(pages);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 主页全部独立一级页面已直接写入 ClientShell 场景。");
        }

        public static void ApplyCanvasUiArchitecture()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null || canvas.gameObject.name != "ClientCanvas")
            {
                Debug.LogError("[Client] 未找到 ClientCanvas，无法迁移客户端 UI 架构。");
                EditorApplication.Exit(2);
                return;
            }

            Transform pagesLayer = EnsureLayer(canvas.transform, "PagesLayer");
            EnsureLayer(canvas.transform, "PopupLayer");
            EnsureLayer(canvas.transform, "SystemLayer");
            string[] pageNames =
            {
                "MainPage", "ProfilePage", "HomeShowcasePage", "HeroPage", "HeroDetailPage", "BackpackPage",
                "GachaPage", "ShopPage", "MailPage", "RankPage", "ActivityPage", "TaskPage", "NoticePage", "FeaturePage"
            };
            foreach (string name in pageNames)
            {
                Transform page = canvas.transform.Find(name);
                if (page != null)
                    page.SetParent(pagesLayer, false);
            }

            ClientPageNavigator legacyNavigator = Object.FindObjectOfType<ClientPageNavigator>();
            if (legacyNavigator != null)
                legacyNavigator.enabled = false;

            ClientUiNavigator navigator = canvas.GetComponent<ClientUiNavigator>();
            if (navigator == null)
                navigator = canvas.gameObject.AddComponent<ClientUiNavigator>();
            List<ClientUiPage> pages = new List<ClientUiPage>();
            AddUiPage(pagesLayer, pages, "MainPage", ClientUiPageId.Home);
            AddUiPage(pagesLayer, pages, "ProfilePage", ClientUiPageId.Profile);
            AddUiPage(pagesLayer, pages, "HomeShowcasePage", ClientUiPageId.HomeShowcase);
            AddUiPage(pagesLayer, pages, "HeroPage", ClientUiPageId.Hero);
            AddUiPage(pagesLayer, pages, "HeroDetailPage", ClientUiPageId.HeroDetail);
            AddUiPage(pagesLayer, pages, "BackpackPage", ClientUiPageId.Backpack);
            AddUiPage(pagesLayer, pages, "GachaPage", ClientUiPageId.Gacha);
            AddUiPage(pagesLayer, pages, "ShopPage", ClientUiPageId.Shop);
            AddUiPage(pagesLayer, pages, "MailPage", ClientUiPageId.Mail);
            AddUiPage(pagesLayer, pages, "RankPage", ClientUiPageId.Rank);
            AddUiPage(pagesLayer, pages, "ActivityPage", ClientUiPageId.Activity);
            AddUiPage(pagesLayer, pages, "TaskPage", ClientUiPageId.Task);
            AddUiPage(pagesLayer, pages, "NoticePage", ClientUiPageId.Notice);
            navigator.Configure(pages);
            foreach (ClientUiPage page in pages)
                if (page.PageId == ClientUiPageId.Home)
                    page.Enter();
                else
                    page.Exit();
            AttachBackButton(pagesLayer, "ProfilePage");
            AttachBackButton(pagesLayer, "HomeShowcasePage");
            AttachBackButton(pagesLayer, "HeroPage");
            AttachBackButton(pagesLayer, "HeroDetailPage");
            AttachBackButton(pagesLayer, "BackpackPage");
            AttachBackButton(pagesLayer, "GachaPage");
            AttachBackButton(pagesLayer, "ShopPage");
            AttachBackButton(pagesLayer, "MailPage");
            AttachBackButton(pagesLayer, "RankPage");
            AttachBackButton(pagesLayer, "ActivityPage");
            AttachBackButton(pagesLayer, "TaskPage");
            AttachBackButton(pagesLayer, "NoticePage");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] Canvas 分层、页面生命周期和返回栈已写入 ClientShell 场景。");
        }

        public static void ApplyPersonalCenterPages()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            ClientUiNavigator navigator = canvas != null ? canvas.GetComponent<ClientUiNavigator>() : null;
            Transform pagesLayer = canvas != null ? canvas.transform.Find("PagesLayer") : null;
            if (navigator == null || pagesLayer == null)
            {
                Debug.LogError("[Client] 未找到新 Canvas UI 架构，未写入个人中心扩展页面。");
                EditorApplication.Exit(2);
                return;
            }

            EnsureSprites(BadgeSpriteRoot);
            ClientUiPage badgePage = BuildBadgePage(pagesLayer, navigator);
            navigator.Register(badgePage);
            badgePage.Exit();
            EnhanceHeroDetailPage(pagesLayer);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 个人中心展示、铭牌与英雄详情扩展已写入 ClientShell 场景。");
        }

        public static void ApplyGachaPage()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            Transform pagesLayer = canvas != null ? canvas.transform.Find("PagesLayer") : null;
            if (pagesLayer == null)
            {
                Debug.LogError("[Client] 未找到 PagesLayer，未写入扭蛋页面。");
                EditorApplication.Exit(2);
                return;
            }

            BuildGachaPage(pagesLayer);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 扭蛋本地模拟页面已写入 ClientShell 场景。");
        }

        public static void ApplyMailPage()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            Transform pagesLayer = canvas != null ? canvas.transform.Find("PagesLayer") : null;
            if (pagesLayer == null)
            {
                Debug.LogError("[Client] 未找到 PagesLayer，未写入邮箱页面。");
                EditorApplication.Exit(2);
                return;
            }

            BuildMailPage(pagesLayer);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 邮箱本地模拟页面已写入 ClientShell 场景。");
        }

        public static void ApplyRankPage()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            Transform pagesLayer = canvas != null ? canvas.transform.Find("PagesLayer") : null;
            if (pagesLayer == null)
            {
                Debug.LogError("[Client] 未找到 PagesLayer，未写入排行榜页面。");
                EditorApplication.Exit(2);
                return;
            }

            BuildRankPage(pagesLayer);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 排行榜本地模拟页面已写入 ClientShell 场景。");
        }

        public static void ApplyActivityPage()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            Transform pagesLayer = canvas != null ? canvas.transform.Find("PagesLayer") : null;
            if (pagesLayer == null)
            {
                Debug.LogError("[Client] 未找到 PagesLayer，未写入活动页面。");
                EditorApplication.Exit(2);
                return;
            }

            BuildActivityPage(pagesLayer);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 活动本地模拟页面已写入 ClientShell 场景。");
        }

        public static void ApplyTaskPage()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            Transform pagesLayer = canvas != null ? canvas.transform.Find("PagesLayer") : null;
            if (pagesLayer == null)
            {
                Debug.LogError("[Client] 未找到 PagesLayer，未写入任务页面。");
                EditorApplication.Exit(2);
                return;
            }

            BuildTaskPage(pagesLayer);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 任务本地模拟页面已写入 ClientShell 场景。");
        }

        public static void ApplyNoticePage()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            Transform pagesLayer = canvas != null ? canvas.transform.Find("PagesLayer") : null;
            if (pagesLayer == null)
            {
                Debug.LogError("[Client] 未找到 PagesLayer，未写入公告页面。");
                EditorApplication.Exit(2);
                return;
            }

            BuildNoticePage(pagesLayer);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 公告本地模拟页面已写入 ClientShell 场景。");
        }

        public static void ApplyHeroEnhancePage()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            ClientUiNavigator navigator = canvas != null ? canvas.GetComponent<ClientUiNavigator>() : null;
            Transform pagesLayer = canvas != null ? canvas.transform.Find("PagesLayer") : null;
            if (navigator == null || pagesLayer == null)
            {
                Debug.LogError("[Client] 未找到 Canvas 页面导航，未写入英雄培养页面。");
                EditorApplication.Exit(2);
                return;
            }
            EnhanceHeroDetailEntry(pagesLayer);
            ClientUiPage enhancePage = BuildHeroEnhancePage(pagesLayer);
            navigator.Register(enhancePage);
            enhancePage.Exit();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 英雄培养页面与详情入口已写入 ClientShell 场景。");
        }

        public static void ApplyBackpackInteractionPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClientBackpackPage page = Object.FindObjectOfType<ClientBackpackPage>(true);
            if (page == null)
            {
                Debug.LogError("[Client] 未找到背包页面，未写入背包交互节点。");
                EditorApplication.Exit(2);
                return;
            }
            if (page.transform.Find("LockToggleButton") == null)
                CreateButton("LockToggleButton", page.transform, new Vector2(0f, -400f), "锁定/解锁", page.ToggleSelectedLock);
            if (page.transform.Find("EquipToggleButton") == null)
                CreateButton("EquipToggleButton", page.transform, new Vector2(0f, -500f), "装备/卸下", page.ToggleSelectedEquipment);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 背包锁定交互节点已写入 ClientShell 场景。");
        }

        public static void ApplyHeroEnhanceStarPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClientHeroEnhancePage page = Object.FindObjectOfType<ClientHeroEnhancePage>(true);
            if (page == null)
            {
                Debug.LogError("[Client] 未找到英雄培养页面，未写入升星节点。");
                EditorApplication.Exit(2);
                return;
            }
            if (page.transform.Find("StarUpButton") == null)
                CreateButton("StarUpButton", page.transform, new Vector2(0f, -425f), "升星", page.StarUp);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 英雄培养升星节点已写入 ClientShell 场景。");
        }

        public static void ApplyFormationAndMarblePages()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            ClientUiNavigator navigator = canvas != null ? canvas.GetComponent<ClientUiNavigator>() : null;
            Transform pagesLayer = canvas != null ? canvas.transform.Find("PagesLayer") : null;
            if (navigator == null || pagesLayer == null) { Debug.LogError("[Client] 缺少页面导航，未写入编队/弹珠页。"); EditorApplication.Exit(2); return; }
            ClientUiPage formation=BuildFormationPage(pagesLayer); ClientUiPage marble=BuildMarblePage(pagesLayer);
            navigator.Register(formation); navigator.Register(marble); formation.Exit(); marble.Exit();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("[Client] 编队与弹珠独立页面已写入 ClientShell 场景。");
        }

        public static void ApplyGachaHistoryPass()
        {
            Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            ClientGachaPage page=Object.FindObjectOfType<ClientGachaPage>(true);
            if(page==null){Debug.LogError("[Client] 未找到扭蛋页，未写入历史节点。");EditorApplication.Exit(2);return;}
            if(page.transform.Find("History")==null){Text history=CreateText("History",page.transform,new Vector2(0f,-205f),new Vector2(520f,45f),22,TextAnchor.MiddleCenter,Color.white);page.ConfigureHistory(history);}
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("[Client] 扭蛋历史节点已写入 ClientShell 场景。");
        }

        public static void ApplyGachaResultPopupPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>(true);
            ClientGachaPage page = Object.FindObjectOfType<ClientGachaPage>(true);
            Transform popupLayer = canvas != null ? canvas.transform.Find("PopupLayer") : null;
            if (page == null || popupLayer == null)
            {
                Debug.LogError("[Client] 缺少扭蛋页或 PopupLayer，未写入抽取结果弹窗。");
                EditorApplication.Exit(2);
                return;
            }
            Transform existing = popupLayer.Find("GachaResultPopup");
            GameObject root = existing != null ? existing.gameObject : CreateObject("GachaResultPopup", popupLayer, typeof(Image));
            Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
            root.GetComponent<Image>().raycastTarget = true;
            Transform panelTransform = root.transform.Find("Panel");
            GameObject panel = panelTransform != null ? panelTransform.gameObject : CreateObject("Panel", root.transform, typeof(Image));
            panel.GetComponent<Image>().color = new Color(0.20f, 0.13f, 0.38f, 0.98f);
            Center(panel.GetComponent<RectTransform>(), new Vector2(0f, 40f), new Vector2(620f, 470f));
            Text title = panel.transform.Find("Title") != null ? panel.transform.Find("Title").GetComponent<Text>() : CreateText("Title", panel.transform, new Vector2(0f, 155f), new Vector2(520f, 62f), 34, TextAnchor.MiddleCenter, Color.white);
            title.text = "抽取结果";
            Text content = panel.transform.Find("Content") != null ? panel.transform.Find("Content").GetComponent<Text>() : CreateText("Content", panel.transform, new Vector2(0f, 20f), new Vector2(540f, 210f), 26, TextAnchor.MiddleCenter, Color.white);
            GameObject close = panel.transform.Find("CloseButton") != null ? panel.transform.Find("CloseButton").gameObject : CreateObject("CloseButton", panel.transform, typeof(Image), typeof(Button));
            close.GetComponent<Image>().color = new Color(0.86f, 0.75f, 0.28f, 1f);
            Center(close.GetComponent<RectTransform>(), new Vector2(0f, -145f), new Vector2(220f, 64f));
            Text closeLabel = close.transform.Find("Label") != null ? close.transform.Find("Label").GetComponent<Text>() : CreateText("Label", close.transform, Vector2.zero, new Vector2(210f, 56f), 25, TextAnchor.MiddleCenter, Color.black);
            closeLabel.text = "关闭";
            ClientGachaResultPopup popup = root.GetComponent<ClientGachaResultPopup>();
            if (popup == null) popup = root.AddComponent<ClientGachaResultPopup>();
            close.GetComponent<Button>().onClick.RemoveAllListeners();
            close.GetComponent<Button>().onClick.AddListener(popup.Hide);
            popup.Configure(root, content);
            page.ConfigureResultPopup(popup);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 扭蛋结果弹窗已写入 PopupLayer。");
        }

        public static void ApplyMailBatchAndFilterPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClientMailPage page = Object.FindObjectOfType<ClientMailPage>(true);
            if (page == null)
            {
                Debug.LogError("[Client] 未找到邮箱页，未写入批量操作节点。");
                EditorApplication.Exit(2);
                return;
            }
            if (page.transform.Find("AllButton") == null)
                CreateButton("AllButton", page.transform, new Vector2(-190f, 560f), "全部", page.ShowAll);
            if (page.transform.Find("UnreadButton") == null)
                CreateButton("UnreadButton", page.transform, new Vector2(0f, 560f), "未读", page.ShowUnread);
            if (page.transform.Find("ClaimAllButton") == null)
                CreateButton("ClaimAllButton", page.transform, new Vector2(190f, -380f), "一键领取", page.ClaimAll);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 邮箱批量领取与筛选节点已写入 ClientShell 场景。");
        }

        public static void ApplyActivityTaskFilterPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClientActivityPage activity = Object.FindObjectOfType<ClientActivityPage>(true);
            ClientTaskPage task = Object.FindObjectOfType<ClientTaskPage>(true);
            if (activity != null)
            {
                if (activity.transform.Find("AvailableButton") == null)
                    CreateButton("AvailableButton", activity.transform, new Vector2(-150f, 560f), "进行中", activity.ShowAvailable);
                if (activity.transform.Find("ClaimedButton") == null)
                    CreateButton("ClaimedButton", activity.transform, new Vector2(150f, 560f), "已领取", activity.ShowClaimed);
            }
            if (task != null)
            {
                if (task.transform.Find("AvailableButton") == null)
                    CreateButton("AvailableButton", task.transform, new Vector2(-150f, 560f), "进行中", task.ShowAvailable);
                if (task.transform.Find("ClaimedButton") == null)
                    CreateButton("ClaimedButton", task.transform, new Vector2(150f, 560f), "已领取", task.ShowClaimed);
            }
            if (activity == null && task == null)
            {
                Debug.LogError("[Client] 未找到活动或任务页，未写入筛选节点。");
                EditorApplication.Exit(2);
                return;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 活动与任务筛选节点已写入 ClientShell 场景。");
        }

        public static void ApplyNoticeFilterPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClientNoticePage page = Object.FindObjectOfType<ClientNoticePage>(true);
            if (page == null)
            {
                Debug.LogError("[Client] 未找到公告页，未写入公告筛选节点。");
                EditorApplication.Exit(2);
                return;
            }
            if (page.transform.Find("AllButton") == null)
                CreateButton("AllButton", page.transform, new Vector2(-150f, 560f), "全部", page.ShowAll);
            if (page.transform.Find("UnreadButton") == null)
                CreateButton("UnreadButton", page.transform, new Vector2(150f, 560f), "未读", page.ShowUnread);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 公告筛选节点已写入 ClientShell 场景。");
        }

        public static void ApplyBackpackDetailPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClientBackpackPage page = Object.FindObjectOfType<ClientBackpackPage>(true);
            if (page == null)
            {
                Debug.LogError("[Client] 未找到背包页，未写入道具详情节点。");
                EditorApplication.Exit(2);
                return;
            }
            Text detail = page.transform.Find("Detail") != null ? page.transform.Find("Detail").GetComponent<Text>() : CreateText("Detail", page.transform, new Vector2(0f, -120f), new Vector2(600f, 180f), 24, TextAnchor.MiddleCenter, Color.white);
            page.ConfigureDetail(detail);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 背包道具详情节点已写入 ClientShell 场景。");
        }

        public static void ApplyCoreInteractionScenePass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClientMailPage mail = Object.FindObjectOfType<ClientMailPage>(true);
            ClientActivityPage activity = Object.FindObjectOfType<ClientActivityPage>(true);
            ClientTaskPage task = Object.FindObjectOfType<ClientTaskPage>(true);
            ClientNoticePage notice = Object.FindObjectOfType<ClientNoticePage>(true);
            ClientBackpackPage backpack = Object.FindObjectOfType<ClientBackpackPage>(true);

            if (mail != null)
            {
                if (mail.transform.Find("AllButton") == null) CreateButton("AllButton", mail.transform, new Vector2(-190f, 560f), "全部", mail.ShowAll);
                if (mail.transform.Find("UnreadButton") == null) CreateButton("UnreadButton", mail.transform, new Vector2(0f, 560f), "未读", mail.ShowUnread);
                if (mail.transform.Find("ClaimAllButton") == null) CreateButton("ClaimAllButton", mail.transform, new Vector2(190f, -380f), "一键领取", mail.ClaimAll);
            }
            if (activity != null)
            {
                BindExistingOrCreateButton(activity.transform, "Tab_Ongoing", new Vector2(-145f, 565f), "进行中", activity.ShowAvailable);
                BindExistingOrCreateButton(activity.transform, "Tab_Upcoming", new Vector2(145f, 565f), "已领取", activity.ShowClaimed);
            }
            if (task != null)
            {
                BindExistingOrCreateButton(task.transform, "Tab_Daily", new Vector2(-145f, 565f), "进行中", task.ShowAvailable);
                BindExistingOrCreateButton(task.transform, "Tab_Growth", new Vector2(145f, 565f), "已领取", task.ShowClaimed);
            }
            if (notice != null)
            {
                if (notice.transform.Find("AllButton") == null) CreateButton("AllButton", notice.transform, new Vector2(-150f, 560f), "全部", notice.ShowAll);
                if (notice.transform.Find("UnreadButton") == null) CreateButton("UnreadButton", notice.transform, new Vector2(150f, 560f), "未读", notice.ShowUnread);
            }
            if (backpack != null)
            {
                Text detail = backpack.transform.Find("Detail") != null ? backpack.transform.Find("Detail").GetComponent<Text>() : CreateText("Detail", backpack.transform, new Vector2(0f, -120f), new Vector2(600f, 180f), 24, TextAnchor.MiddleCenter, Color.white);
                backpack.ConfigureDetail(detail);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 核心本地交互节点已一次性写入 ClientShell 场景。");
        }

        public static void ApplySystemFeedbackPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindObjectOfType<Canvas>(true);
            Transform systemLayer = canvas != null ? canvas.transform.Find("SystemLayer") : null;
            if (systemLayer == null)
            {
                Debug.LogError("[Client] 缺少 SystemLayer，未写入全局反馈节点。");
                EditorApplication.Exit(2);
                return;
            }
            GameObject root = systemLayer.Find("UiFeedback") != null ? systemLayer.Find("UiFeedback").gameObject : CreateObject("UiFeedback", systemLayer);
            Stretch(root.GetComponent<RectTransform>());
            GameObject toast = root.transform.Find("Toast") != null ? root.transform.Find("Toast").gameObject : CreateObject("Toast", root.transform, typeof(Image));
            toast.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);
            Center(toast.GetComponent<RectTransform>(), new Vector2(0f, -500f), new Vector2(560f, 86f));
            Text toastText = toast.transform.Find("Text") != null ? toast.transform.Find("Text").GetComponent<Text>() : CreateText("Text", toast.transform, Vector2.zero, new Vector2(520f, 70f), 24, TextAnchor.MiddleCenter, Color.white);
            GameObject loading = root.transform.Find("Loading") != null ? root.transform.Find("Loading").gameObject : CreateObject("Loading", root.transform, typeof(Image));
            Stretch(loading.GetComponent<RectTransform>());
            loading.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
            Text loadingText = loading.transform.Find("Text") != null ? loading.transform.Find("Text").GetComponent<Text>() : CreateText("Text", loading.transform, Vector2.zero, new Vector2(360f, 70f), 28, TextAnchor.MiddleCenter, Color.white);
            loadingText.text = "加载中…";
            ClientUiFeedback feedback = root.GetComponent<ClientUiFeedback>();
            if (feedback == null) feedback = root.AddComponent<ClientUiFeedback>();
            feedback.Configure(toast, toastText, loading);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 全局提示与加载节点已写入 SystemLayer。");
        }

        public static void ApplyHomeRedDotPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClientHomePage home = Object.FindObjectOfType<ClientHomePage>(true);
            if (home == null)
            {
                Debug.LogError("[Client] 未找到主页，未写入红点节点。");
                EditorApplication.Exit(2);
                return;
            }
            Text mailDot = CreateOrGetRedDot(home.transform, "MailButton");
            Text activityDot = CreateOrGetRedDot(home.transform, "ActivityButton");
            Text noticeDot = CreateOrGetRedDot(home.transform, "SupportButton");
            ClientHomeRedDotController controller = home.GetComponent<ClientHomeRedDotController>();
            if (controller == null) controller = home.gameObject.AddComponent<ClientHomeRedDotController>();
            controller.Configure(mailDot, activityDot, noticeDot);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 主页邮件、活动、公告红点节点已写入场景。");
        }

        public static void ApplyBottomNavigationSelectionPass()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ClientHomePage home = Object.FindObjectOfType<ClientHomePage>(true);
            if (home == null)
            {
                Debug.LogError("[Client] 未找到主页，未写入底部导航选中状态。");
                EditorApplication.Exit(2);
                return;
            }
            string[] names = { "ProfileButton", "MarbleButton", "HomeButton", "BackpackButton", "ActivityButton" };
            string[] sprites = { "个人中心", "弹珠", "主页", "背包", "活动" };
            Image[] icons = new Image[names.Length];
            GameObject[] frames = new GameObject[names.Length];
            Sprite[] normal = new Sprite[names.Length];
            Sprite[] selected = new Sprite[names.Length];
            for (int index = 0; index < names.Length; index++)
            {
                Button button = FindButton(home.transform, names[index]);
                if (button == null) continue;
                icons[index] = button.GetComponent<Image>();
                normal[index] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Client/UI/MainPage/Sprites/底部icon/" + sprites[index] + "2.png");
                selected[index] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Client/UI/MainPage/Sprites/底部icon/" + sprites[index] + "1.png");
                Transform existing = button.transform.parent.Find(names[index] + "_SelectedFrame");
                GameObject frame = existing != null ? existing.gameObject : CreateObject(names[index] + "_SelectedFrame", button.transform.parent, typeof(Image));
                Image frameImage = frame.GetComponent<Image>();
                frameImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Client/UI/MainPage/Sprites/底部功能icon/icon底框.png");
                frameImage.color = new Color(1f, 0.83f, 0.20f, 1f);
                frameImage.preserveAspect = true;
                frameImage.raycastTarget = false;
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                RectTransform frameRect = frame.GetComponent<RectTransform>();
                frameRect.anchorMin = buttonRect.anchorMin; frameRect.anchorMax = buttonRect.anchorMax; frameRect.pivot = buttonRect.pivot;
                frameRect.anchoredPosition = buttonRect.anchoredPosition + new Vector2(0f, 4f); frameRect.sizeDelta = buttonRect.sizeDelta + new Vector2(18f, 18f);
                frame.transform.SetSiblingIndex(button.transform.GetSiblingIndex());
                frames[index] = frame;
            }
            ClientBottomNavigationBar bar = home.GetComponent<ClientBottomNavigationBar>();
            if (bar == null) bar = home.gameObject.AddComponent<ClientBottomNavigationBar>();
            bar.Configure(names, icons, frames, normal, selected);
            bar.Select("HomeButton");
            home.ConfigureBottomNavigation(bar);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[Client] 五个底部导航按钮的未选中/高亮选中状态已写入场景。");
        }

        private static Button FindButton(Transform root, string name)
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true)) if (button.name == name) return button;
            return null;
        }

        private static Text CreateOrGetRedDot(Transform root, string buttonName)
        {
            Transform target = null;
            foreach (Button button in root.GetComponentsInChildren<Button>(true)) if (button.name == buttonName) { target = button.transform; break; }
            if (target == null) return null;
            Transform existing = target.Find("RedDot");
            Text dot = existing != null ? existing.GetComponent<Text>() : CreateText("RedDot", target, new Vector2(60f, 48f), new Vector2(42f, 42f), 28, TextAnchor.MiddleCenter, Color.white);
            dot.text = "•";
            dot.color = new Color(1f, 0.22f, 0.22f, 1f);
            return dot;
        }

        private static void BindExistingOrCreateButton(Transform parent, string name, Vector2 position, string label, UnityEngine.Events.UnityAction action)
        {
            Transform existing = parent.Find(name);
            if (existing == null)
            {
                CreateButton(name, parent, position, label, action);
                return;
            }
            Button button = existing.GetComponent<Button>();
            if (button == null) button = existing.gameObject.AddComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
            Text text = existing.Find("Label") != null ? existing.Find("Label").GetComponent<Text>() : null;
            if (text != null) text.text = label;
        }

        private static ClientHeroDetailPage BuildPage(Transform parent, ClientPageNavigator navigator)
        {
            GameObject root = CreateObject("HeroDetailPage", parent);
            Stretch(root.GetComponent<RectTransform>());
            CreateImage("Background", root.transform, SpriteRoot + "/背景中部.png", Vector2.zero, new Vector2(752f, 1630f));
            Text title = CreateText("Title", root.transform, new Vector2(0f, 685f), new Vector2(500f, 60f), 38, TextAnchor.MiddleCenter, Color.white);
            title.text = "英雄详情";

            CreateImage("CombatPowerPanel", root.transform, SpriteRoot + "/战力框.png", new Vector2(0f, 560f), new Vector2(360f, 95f));
            Text combatPower = CreateText("CombatPower", root.transform, new Vector2(25f, 560f), new Vector2(300f, 50f), 27, TextAnchor.MiddleCenter, Color.white);
            CreateImage("AttributePanel", root.transform, SpriteRoot + "/属性底框.png", new Vector2(-185f, 430f), new Vector2(290f, 130f));
            Text name = CreateText("HeroName", root.transform, new Vector2(105f, 440f), new Vector2(350f, 52f), 34, TextAnchor.MiddleCenter, Color.white);
            Text level = CreateText("HeroLevel", root.transform, new Vector2(105f, 380f), new Vector2(240f, 42f), 25, TextAnchor.MiddleCenter, Color.white);
            CreateImage("SkillPanel", root.transform, SpriteRoot + "/技能底框.png", new Vector2(0f, -100f), new Vector2(640f, 550f));
            Text skillTitle = CreateText("SkillTitle", root.transform, new Vector2(0f, 125f), new Vector2(400f, 48f), 28, TextAnchor.MiddleCenter, Color.white);
            skillTitle.text = "技能与养成信息";
            Text pending = CreateText("PendingInfo", root.transform, new Vector2(0f, -65f), new Vector2(520f, 250f), 24, TextAnchor.UpperLeft, Color.white);
            pending.text = "技能、装备槽和数值规则待产品确认\n\n当前展示本地模拟英雄数据";
            CreateButton("ReturnButton", root.transform, new Vector2(0f, -650f), "返回英雄页", navigator.ReturnFromHeroDetail);

            ClientHeroDetailPage page = root.AddComponent<ClientHeroDetailPage>();
            page.Configure(root, name, level, combatPower);
            return page;
        }

        private static void AddPrimaryPage(Transform parent, ClientPageNavigator navigator, List<ClientPrimaryPage> pages, string rootName, ClientHomeDestination destination)
        {
            Transform existing = parent.Find(rootName);
            ClientPrimaryPage page;
            if (existing == null)
            {
                GameObject root = CreateObject(rootName, parent);
                Stretch(root.GetComponent<RectTransform>());
                Image background = root.AddComponent<Image>();
                background.color = new Color(0.16f, 0.12f, 0.34f, 0.98f);
                Text title = CreateText("Title", root.transform, new Vector2(0f, 620f), new Vector2(560f, 70f), 42, TextAnchor.MiddleCenter, Color.white);
                Text status = CreateText("Status", root.transform, Vector2.zero, new Vector2(600f, 400f), 29, TextAnchor.MiddleCenter, Color.white);
                CreateButton("ReturnButton", root.transform, new Vector2(0f, -620f), "返回主页", navigator.ReturnFromPrimary);
                page = root.AddComponent<ClientPrimaryPage>();
                page.Configure(destination, root, title, status);
                page.Hide();
            }
            else
            {
                page = existing.GetComponent<ClientPrimaryPage>();
            }
            if (page != null)
                pages.Add(page);
        }

        private static Transform EnsureLayer(Transform canvas, string name)
        {
            Transform layer = canvas.Find(name);
            if (layer != null)
                return layer;
            GameObject root = CreateObject(name, canvas);
            Stretch(root.GetComponent<RectTransform>());
            return root.transform;
        }

        private static void AddUiPage(Transform pagesLayer, List<ClientUiPage> pages, string rootName, ClientUiPageId pageId)
        {
            Transform root = pagesLayer.Find(rootName);
            if (root == null)
                return;
            ClientUiPage page = root.GetComponent<ClientUiPage>();
            if (page == null)
                page = root.gameObject.AddComponent<ClientUiPage>();
            page.Configure(pageId);
            pages.Add(page);
        }

        private static void AttachBackButton(Transform pagesLayer, string pageName)
        {
            Transform page = pagesLayer.Find(pageName);
            if (page == null)
                return;
            Transform button = page.Find("ReturnButton");
            if (button != null && button.GetComponent<Button>() != null && button.GetComponent<ClientUiBackButton>() == null)
                button.gameObject.AddComponent<ClientUiBackButton>();
        }

        private static ClientBackpackPage BuildBackpackPage(Transform parent, ClientPageNavigator navigator)
        {
            GameObject root = CreateObject("BackpackPage", parent);
            Stretch(root.GetComponent<RectTransform>());
            CreateImage("Background", root.transform, MainSpriteRoot + "/背景.png", Vector2.zero, new Vector2(752f, 1630f));
            Text title = CreateText("Title", root.transform, new Vector2(0f, 680f), new Vector2(450f, 64f), 40, TextAnchor.MiddleCenter, Color.white);
            title.text = "背包";
            Text status = CreateText("FilterStatus", root.transform, new Vector2(0f, 580f), new Vector2(480f, 44f), 25, TextAnchor.MiddleCenter, Color.white);

            ClientBackpackPage page = root.AddComponent<ClientBackpackPage>();
            CreateButton("AllFilter", root.transform, new Vector2(-200f, 500f), "全部", () => page.SetFilter("All"));
            CreateButton("MaterialFilter", root.transform, new Vector2(0f, 500f), "材料", () => page.SetFilter("Material"));
            CreateButton("EquipmentFilter", root.transform, new Vector2(200f, 500f), "装备", () => page.SetFilter("Equipment"));
            GameObject material = CreateItemSlot("MaterialSlot", root.transform, new Vector2(-180f, 220f), "材料", page.SelectMaterial, out Text materialLabel);
            GameObject equipment = CreateItemSlot("EquipmentSlot", root.transform, new Vector2(180f, 220f), "装备", page.SelectEquipment, out Text equipmentLabel);
            Text emptyHint = CreateText("EmptyHint", root.transform, new Vector2(0f, -120f), new Vector2(600f, 60f), 23, TextAnchor.MiddleCenter, Color.white);
            emptyHint.text = "更多道具待接入配置与服务端契约";
            CreateButton("ReturnButton", root.transform, new Vector2(0f, -650f), "返回主页", navigator.ReturnFromBackpack);
            page.Configure(root, material, equipment, materialLabel, equipmentLabel, status);
            return page;
        }

        private static ClientUiPage BuildBadgePage(Transform parent, ClientUiNavigator navigator)
        {
            Transform existing = parent.Find("BadgePage");
            if (existing != null)
                return existing.GetComponent<ClientUiPage>();

            GameObject root = CreateObject("BadgePage", parent);
            Stretch(root.GetComponent<RectTransform>());
            CreateImage("Background", root.transform, BadgeSpriteRoot + "/背景.png", Vector2.zero, new Vector2(752f, 1630f));
            CreateImage("PersonalizeLabel", root.transform, BadgeSpriteRoot + "/个性化标签.png", new Vector2(0f, 665f), new Vector2(390f, 65f));
            CreateImage("BadgeFrame", root.transform, BadgeSpriteRoot + "/铭牌底框.png", new Vector2(0f, 330f), new Vector2(540f, 180f));
            Image preview = CreateImage("BadgePreview", root.transform, BadgeSpriteRoot + "/铭牌/1.png", new Vector2(0f, 330f), new Vector2(440f, 125f));
            Text status = CreateText("Status", root.transform, new Vector2(0f, 160f), new Vector2(520f, 50f), 26, TextAnchor.MiddleCenter, Color.white);
            Sprite[] badges =
            {
                AssetDatabase.LoadAssetAtPath<Sprite>(BadgeSpriteRoot + "/铭牌/1.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>(BadgeSpriteRoot + "/铭牌/2.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>(BadgeSpriteRoot + "/铭牌/3.png"),
            };
            ClientBadgePage page = root.AddComponent<ClientBadgePage>();
            page.Configure(root, preview, badges, status);
            CreateBadgeOption("BadgeOption1", root.transform, new Vector2(-230f, -60f), 0);
            CreateBadgeOption("BadgeOption2", root.transform, new Vector2(0f, -60f), 1);
            CreateBadgeOption("BadgeOption3", root.transform, new Vector2(230f, -60f), 2);
            CreateConfirmButton(root.transform, new Vector2(0f, -310f));
            GameObject back = CreateObject("ReturnButton", root.transform, typeof(Image), typeof(Button), typeof(ClientUiBackButton));
            back.GetComponent<Image>().color = new Color(1f, 0.78f, 0.2f, 1f);
            Center(back.GetComponent<RectTransform>(), new Vector2(0f, -650f), new Vector2(220f, 68f));
            CreateText("Label", back.transform, Vector2.zero, new Vector2(210f, 58f), 26, TextAnchor.MiddleCenter, Color.black).text = "返回个人中心";
            ClientUiPage uiPage = root.AddComponent<ClientUiPage>();
            uiPage.Configure(ClientUiPageId.Badge);
            return uiPage;
        }

        private static void BuildGachaPage(Transform pagesLayer)
        {
            Transform existing = pagesLayer.Find("GachaPage");
            GameObject root = existing != null ? existing.gameObject : CreateObject("GachaPage", pagesLayer);
            ClientGachaPage page = root.GetComponent<ClientGachaPage>();
            if (page != null)
                return;

            // GachaPage 目前只是由一级页面占位器生成；仅替换该占位器的直属节点，
            // 保留页面根、ClientUiPage 与其在导航器中的引用。
            ClearChildren(root.transform);
            ClientPrimaryPage primary = root.GetComponent<ClientPrimaryPage>();
            if (primary != null)
                Object.DestroyImmediate(primary);
            Stretch(root.GetComponent<RectTransform>());

            Image background = root.GetComponent<Image>();
            if (background == null)
                background = root.AddComponent<Image>();
            background.color = new Color(0.14f, 0.10f, 0.30f, 0.99f);
            background.raycastTarget = false;

            Text title = CreateText("Title", root.transform, new Vector2(0f, 660f), new Vector2(600f, 68f), 42, TextAnchor.MiddleCenter, Color.white);
            title.text = "扭蛋";
            Text poolTitle = CreateText("PoolTitle", root.transform, new Vector2(0f, 430f), new Vector2(560f, 120f), 30, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.28f, 1f));
            Text currency = CreateText("Currency", root.transform, new Vector2(0f, 325f), new Vector2(380f, 50f), 28, TextAnchor.MiddleCenter, Color.white);
            Text status = CreateText("Status", root.transform, new Vector2(0f, 125f), new Vector2(600f, 90f), 23, TextAnchor.MiddleCenter, Color.white);
            Text result = CreateText("Result", root.transform, new Vector2(0f, -85f), new Vector2(620f, 145f), 28, TextAnchor.MiddleCenter, Color.white);

            page = root.AddComponent<ClientGachaPage>();
            CreateButton("DrawOneButton", root.transform, new Vector2(-145f, -330f), "单抽", page.DrawOne);
            CreateButton("DrawTenButton", root.transform, new Vector2(145f, -330f), "十连", page.DrawTen);
            GameObject back = CreateObject("ReturnButton", root.transform, typeof(Image), typeof(Button), typeof(ClientUiBackButton));
            back.GetComponent<Image>().color = new Color(0.86f, 0.75f, 0.28f, 1f);
            Center(back.GetComponent<RectTransform>(), new Vector2(0f, -640f), new Vector2(220f, 68f));
            CreateText("Label", back.transform, Vector2.zero, new Vector2(210f, 58f), 26, TextAnchor.MiddleCenter, Color.black).text = "返回主页";
            page.Configure(root, poolTitle, currency, status, result);
            Text history = CreateText("History", root.transform, new Vector2(0f, -205f), new Vector2(520f, 45f), 22, TextAnchor.MiddleCenter, Color.white);
            page.ConfigureHistory(history);

            ClientUiPage uiPage = root.GetComponent<ClientUiPage>();
            if (uiPage == null)
                uiPage = root.AddComponent<ClientUiPage>();
            uiPage.Configure(ClientUiPageId.Gacha);
        }

        private static void BuildMailPage(Transform pagesLayer)
        {
            Transform existing = pagesLayer.Find("MailPage");
            GameObject root = existing != null ? existing.gameObject : CreateObject("MailPage", pagesLayer);
            ClientMailPage page = root.GetComponent<ClientMailPage>();
            if (page != null)
                return;

            ClearChildren(root.transform);
            ClientPrimaryPage primary = root.GetComponent<ClientPrimaryPage>();
            if (primary != null)
                Object.DestroyImmediate(primary);
            Stretch(root.GetComponent<RectTransform>());
            Image background = root.GetComponent<Image>();
            if (background == null)
                background = root.AddComponent<Image>();
            background.color = new Color(0.20f, 0.13f, 0.33f, 0.99f);
            background.raycastTarget = false;
            Text title = CreateText("Title", root.transform, new Vector2(0f, 660f), new Vector2(600f, 68f), 42, TextAnchor.MiddleCenter, Color.white);
            title.text = "邮箱";
            page = root.AddComponent<ClientMailPage>();
            Text[] labels = new Text[2];
            for (int index = 0; index < labels.Length; index++)
            {
                int mailIndex = index;
                GameObject mail = CreateObject("MailItem" + (index + 1), root.transform, typeof(Image), typeof(Button));
                mail.GetComponent<Image>().color = new Color(0.34f, 0.25f, 0.57f, 0.94f);
                Center(mail.GetComponent<RectTransform>(), new Vector2(0f, index == 0 ? 440f : 290f), new Vector2(620f, 120f));
                mail.GetComponent<Button>().onClick.AddListener(() => page.SelectMail(mailIndex));
                labels[index] = CreateText("Label", mail.transform, Vector2.zero, new Vector2(570f, 102f), 24, TextAnchor.MiddleLeft, Color.white);
            }
            Text detail = CreateText("Detail", root.transform, new Vector2(0f, 5f), new Vector2(620f, 300f), 26, TextAnchor.UpperLeft, Color.white);
            Text status = CreateText("Status", root.transform, new Vector2(0f, -245f), new Vector2(600f, 50f), 23, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.28f, 1f));
            GameObject emptyHint = CreateObject("EmptyHint", root.transform, typeof(Text));
            Center(emptyHint.GetComponent<RectTransform>(), new Vector2(0f, 130f), new Vector2(550f, 80f));
            emptyHint.GetComponent<Text>().font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emptyHint.GetComponent<Text>().fontSize = 28;
            emptyHint.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            emptyHint.GetComponent<Text>().color = Color.white;
            emptyHint.GetComponent<Text>().text = "暂无邮件";
            CreateButton("ClaimButton", root.transform, new Vector2(0f, -380f), "领取附件", page.ClaimSelected);
            GameObject back = CreateObject("ReturnButton", root.transform, typeof(Image), typeof(Button), typeof(ClientUiBackButton));
            back.GetComponent<Image>().color = new Color(0.86f, 0.75f, 0.28f, 1f);
            Center(back.GetComponent<RectTransform>(), new Vector2(0f, -640f), new Vector2(220f, 68f));
            CreateText("Label", back.transform, Vector2.zero, new Vector2(210f, 58f), 26, TextAnchor.MiddleCenter, Color.black).text = "返回主页";
            page.Configure(root, labels, detail, status, emptyHint);
            ClientUiPage uiPage = root.GetComponent<ClientUiPage>();
            if (uiPage == null)
                uiPage = root.AddComponent<ClientUiPage>();
            uiPage.Configure(ClientUiPageId.Mail);
        }

        private static void BuildRankPage(Transform pagesLayer)
        {
            Transform existing = pagesLayer.Find("RankPage");
            GameObject root = existing != null ? existing.gameObject : CreateObject("RankPage", pagesLayer);
            ClientRankPage page = root.GetComponent<ClientRankPage>();
            if (page != null)
                return;

            ClearChildren(root.transform);
            ClientPrimaryPage primary = root.GetComponent<ClientPrimaryPage>();
            if (primary != null)
                Object.DestroyImmediate(primary);
            Stretch(root.GetComponent<RectTransform>());
            Image background = root.GetComponent<Image>();
            if (background == null)
                background = root.AddComponent<Image>();
            background.color = new Color(0.29f, 0.18f, 0.10f, 0.99f);
            background.raycastTarget = false;
            Text title = CreateText("Title", root.transform, new Vector2(0f, 660f), new Vector2(600f, 68f), 42, TextAnchor.MiddleCenter, Color.white);
            title.text = "排行榜";
            Text[] topEntries = new Text[3];
            for (int index = 0; index < topEntries.Length; index++)
            {
                GameObject entry = CreateObject("Top" + (index + 1), root.transform, typeof(Image));
                entry.GetComponent<Image>().color = index == 0 ? new Color(0.70f, 0.52f, 0.16f, 0.98f) : new Color(0.40f, 0.29f, 0.20f, 0.98f);
                Center(entry.GetComponent<RectTransform>(), new Vector2(0f, 440f - index * 150f), new Vector2(620f, 110f));
                topEntries[index] = CreateText("Label", entry.transform, Vector2.zero, new Vector2(570f, 90f), 26, TextAnchor.MiddleCenter, Color.white);
            }
            Text status = CreateText("Status", root.transform, new Vector2(0f, -155f), new Vector2(640f, 100f), 22, TextAnchor.MiddleCenter, Color.white);
            GameObject self = CreateObject("SelfEntry", root.transform, typeof(Image));
            self.GetComponent<Image>().color = new Color(0.25f, 0.52f, 0.63f, 0.98f);
            Center(self.GetComponent<RectTransform>(), new Vector2(0f, -350f), new Vector2(660f, 100f));
            Text selfEntry = CreateText("Label", self.transform, Vector2.zero, new Vector2(620f, 80f), 24, TextAnchor.MiddleCenter, Color.white);
            page = root.AddComponent<ClientRankPage>();
            CreateButton("RefreshButton", root.transform, new Vector2(0f, -465f), "刷新", () => page.Refresh());
            GameObject back = CreateObject("ReturnButton", root.transform, typeof(Image), typeof(Button), typeof(ClientUiBackButton));
            back.GetComponent<Image>().color = new Color(0.86f, 0.75f, 0.28f, 1f);
            Center(back.GetComponent<RectTransform>(), new Vector2(0f, -640f), new Vector2(220f, 68f));
            CreateText("Label", back.transform, Vector2.zero, new Vector2(210f, 58f), 26, TextAnchor.MiddleCenter, Color.black).text = "返回主页";
            page.Configure(root, topEntries, selfEntry, status);
            ClientUiPage uiPage = root.GetComponent<ClientUiPage>();
            if (uiPage == null)
                uiPage = root.AddComponent<ClientUiPage>();
            uiPage.Configure(ClientUiPageId.Rank);
        }

        private static void BuildActivityPage(Transform pagesLayer)
        {
            Transform existing = pagesLayer.Find("ActivityPage");
            GameObject root = existing != null ? existing.gameObject : CreateObject("ActivityPage", pagesLayer);
            ClientActivityPage page = root.GetComponent<ClientActivityPage>();
            if (page != null)
                return;

            ClearChildren(root.transform);
            ClientPrimaryPage primary = root.GetComponent<ClientPrimaryPage>();
            if (primary != null)
                Object.DestroyImmediate(primary);
            Stretch(root.GetComponent<RectTransform>());
            Image background = root.GetComponent<Image>();
            if (background == null)
                background = root.AddComponent<Image>();
            background.color = new Color(0.18f, 0.15f, 0.36f, 0.99f);
            background.raycastTarget = false;
            Text title = CreateText("TopBar_Title", root.transform, new Vector2(0f, 660f), new Vector2(600f, 68f), 42, TextAnchor.MiddleCenter, Color.white);
            title.text = "活动中心";
            CreateStructuredTab("Tab_Ongoing", root.transform, new Vector2(-145f, 565f), "进行中", true);
            CreateStructuredTab("Tab_Upcoming", root.transform, new Vector2(145f, 565f), "即将开启", false);
            page = root.AddComponent<ClientActivityPage>();
            Text[] labels = new Text[2];
            for (int index = 0; index < labels.Length; index++)
            {
                int activityIndex = index;
                GameObject card = CreateObject("ActivityCard" + (index + 1), root.transform, typeof(Image), typeof(Button));
                card.GetComponent<Image>().color = new Color(0.37f, 0.28f, 0.66f, 0.96f);
                Center(card.GetComponent<RectTransform>(), new Vector2(0f, index == 0 ? 405f : 225f), new Vector2(640f, 140f));
                card.GetComponent<Button>().onClick.AddListener(() => page.SelectActivity(activityIndex));
                labels[index] = CreateText("CardText", card.transform, new Vector2(-125f, 0f), new Vector2(370f, 110f), 25, TextAnchor.MiddleLeft, Color.white);
                Text actionHint = CreateText("ActionHint", card.transform, new Vector2(215f, 0f), new Vector2(130f, 70f), 22, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.28f, 1f));
                actionHint.text = "查看";
            }
            GameObject detailPanel = CreateObject("DetailPanel", root.transform, typeof(Image));
            detailPanel.GetComponent<Image>().color = new Color(0.12f, 0.10f, 0.28f, 0.96f);
            Center(detailPanel.GetComponent<RectTransform>(), new Vector2(0f, -55f), new Vector2(650f, 310f));
            Text detail = CreateText("DetailText", detailPanel.transform, new Vector2(0f, 25f), new Vector2(590f, 215f), 25, TextAnchor.UpperLeft, Color.white);
            Text status = CreateText("StatusText", detailPanel.transform, new Vector2(0f, -105f), new Vector2(590f, 46f), 22, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.28f, 1f));
            CreateButton("ClaimButton", root.transform, new Vector2(0f, -345f), "领取奖励", page.ClaimSelected);
            GameObject back = CreateObject("ReturnButton", root.transform, typeof(Image), typeof(Button), typeof(ClientUiBackButton));
            back.GetComponent<Image>().color = new Color(0.86f, 0.75f, 0.28f, 1f);
            Center(back.GetComponent<RectTransform>(), new Vector2(0f, -640f), new Vector2(220f, 68f));
            CreateText("Label", back.transform, Vector2.zero, new Vector2(210f, 58f), 26, TextAnchor.MiddleCenter, Color.black).text = "返回主页";
            page.Configure(root, labels, detail, status);
            ClientUiPage uiPage = root.GetComponent<ClientUiPage>();
            if (uiPage == null)
                uiPage = root.AddComponent<ClientUiPage>();
            uiPage.Configure(ClientUiPageId.Activity);
        }

        private static void CreateStructuredTab(string name, Transform parent, Vector2 position, string label, bool selected)
        {
            GameObject tab = CreateObject(name, parent, typeof(Image));
            tab.GetComponent<Image>().color = selected ? new Color(0.98f, 0.75f, 0.20f, 1f) : new Color(0.33f, 0.26f, 0.55f, 1f);
            Center(tab.GetComponent<RectTransform>(), position, new Vector2(240f, 64f));
            CreateText("Label", tab.transform, Vector2.zero, new Vector2(220f, 52f), 24, TextAnchor.MiddleCenter, selected ? Color.black : Color.white).text = label;
        }

        private static void BuildTaskPage(Transform pagesLayer)
        {
            Transform existing = pagesLayer.Find("TaskPage");
            GameObject root = existing != null ? existing.gameObject : CreateObject("TaskPage", pagesLayer);
            ClientTaskPage page = root.GetComponent<ClientTaskPage>();
            if (page != null)
                return;

            ClearChildren(root.transform);
            ClientPrimaryPage primary = root.GetComponent<ClientPrimaryPage>();
            if (primary != null)
                Object.DestroyImmediate(primary);
            Stretch(root.GetComponent<RectTransform>());
            Image background = root.GetComponent<Image>();
            if (background == null)
                background = root.AddComponent<Image>();
            background.color = new Color(0.12f, 0.29f, 0.34f, 0.99f);
            background.raycastTarget = false;
            Text title = CreateText("TopBar_Title", root.transform, new Vector2(0f, 660f), new Vector2(600f, 68f), 42, TextAnchor.MiddleCenter, Color.white);
            title.text = "任务";
            CreateStructuredTab("Tab_Daily", root.transform, new Vector2(-145f, 565f), "每日", true);
            CreateStructuredTab("Tab_Growth", root.transform, new Vector2(145f, 565f), "成长", false);
            page = root.AddComponent<ClientTaskPage>();
            Text[] labels = new Text[2];
            for (int index = 0; index < labels.Length; index++)
            {
                int taskIndex = index;
                GameObject card = CreateObject("TaskCard" + (index + 1), root.transform, typeof(Image), typeof(Button));
                card.GetComponent<Image>().color = new Color(0.20f, 0.50f, 0.56f, 0.96f);
                Center(card.GetComponent<RectTransform>(), new Vector2(0f, index == 0 ? 405f : 225f), new Vector2(640f, 140f));
                card.GetComponent<Button>().onClick.AddListener(() => page.SelectTask(taskIndex));
                labels[index] = CreateText("CardText", card.transform, new Vector2(-125f, 0f), new Vector2(370f, 110f), 25, TextAnchor.MiddleLeft, Color.white);
                Text actionHint = CreateText("ActionHint", card.transform, new Vector2(215f, 0f), new Vector2(130f, 70f), 22, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.28f, 1f));
                actionHint.text = "查看";
            }
            GameObject detailPanel = CreateObject("DetailPanel", root.transform, typeof(Image));
            detailPanel.GetComponent<Image>().color = new Color(0.08f, 0.20f, 0.24f, 0.96f);
            Center(detailPanel.GetComponent<RectTransform>(), new Vector2(0f, -55f), new Vector2(650f, 310f));
            Text detail = CreateText("DetailText", detailPanel.transform, new Vector2(0f, 25f), new Vector2(590f, 215f), 25, TextAnchor.UpperLeft, Color.white);
            Text status = CreateText("StatusText", detailPanel.transform, new Vector2(0f, -105f), new Vector2(590f, 46f), 22, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.28f, 1f));
            CreateButton("ClaimButton", root.transform, new Vector2(0f, -345f), "领取奖励", page.ClaimSelected);
            GameObject back = CreateObject("ReturnButton", root.transform, typeof(Image), typeof(Button), typeof(ClientUiBackButton));
            back.GetComponent<Image>().color = new Color(0.86f, 0.75f, 0.28f, 1f);
            Center(back.GetComponent<RectTransform>(), new Vector2(0f, -640f), new Vector2(220f, 68f));
            CreateText("Label", back.transform, Vector2.zero, new Vector2(210f, 58f), 26, TextAnchor.MiddleCenter, Color.black).text = "返回主页";
            page.Configure(root, labels, detail, status);
            ClientUiPage uiPage = root.GetComponent<ClientUiPage>();
            if (uiPage == null)
                uiPage = root.AddComponent<ClientUiPage>();
            uiPage.Configure(ClientUiPageId.Task);
        }

        private static void BuildNoticePage(Transform pagesLayer)
        {
            Transform existing = pagesLayer.Find("NoticePage");
            GameObject root = existing != null ? existing.gameObject : CreateObject("NoticePage", pagesLayer);
            ClientNoticePage page = root.GetComponent<ClientNoticePage>();
            if (page != null)
                return;

            ClearChildren(root.transform);
            ClientPrimaryPage primary = root.GetComponent<ClientPrimaryPage>();
            if (primary != null)
                Object.DestroyImmediate(primary);
            Stretch(root.GetComponent<RectTransform>());
            Image background = root.GetComponent<Image>();
            if (background == null)
                background = root.AddComponent<Image>();
            background.color = new Color(0.13f, 0.17f, 0.34f, 0.99f);
            background.raycastTarget = false;
            Text title = CreateText("TopBar_Title", root.transform, new Vector2(0f, 660f), new Vector2(600f, 68f), 42, TextAnchor.MiddleCenter, Color.white);
            title.text = "公告";
            GameObject listPanel = CreateObject("NoticeListPanel", root.transform, typeof(Image));
            listPanel.GetComponent<Image>().color = new Color(0.19f, 0.24f, 0.46f, 0.96f);
            Center(listPanel.GetComponent<RectTransform>(), new Vector2(0f, 420f), new Vector2(660f, 230f));
            page = root.AddComponent<ClientNoticePage>();
            Text[] labels = new Text[2];
            for (int index = 0; index < labels.Length; index++)
            {
                int noticeIndex = index;
                GameObject item = CreateObject("NoticeItem" + (index + 1), listPanel.transform, typeof(Image), typeof(Button));
                item.GetComponent<Image>().color = new Color(0.32f, 0.38f, 0.65f, 0.98f);
                Center(item.GetComponent<RectTransform>(), new Vector2(0f, index == 0 ? 52f : -52f), new Vector2(610f, 86f));
                item.GetComponent<Button>().onClick.AddListener(() => page.SelectNotice(noticeIndex));
                labels[index] = CreateText("Label", item.transform, Vector2.zero, new Vector2(560f, 66f), 24, TextAnchor.MiddleLeft, Color.white);
            }
            GameObject detailPanel = CreateObject("NoticeDetailPanel", root.transform, typeof(Image));
            detailPanel.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.24f, 0.96f);
            Center(detailPanel.GetComponent<RectTransform>(), new Vector2(0f, 70f), new Vector2(660f, 360f));
            Text detail = CreateText("DetailText", detailPanel.transform, new Vector2(0f, 0f), new Vector2(600f, 300f), 26, TextAnchor.UpperLeft, Color.white);
            GameObject emptyHint = CreateObject("EmptyHint", root.transform, typeof(Text));
            Center(emptyHint.GetComponent<RectTransform>(), new Vector2(0f, 70f), new Vector2(600f, 80f));
            Text emptyText = emptyHint.GetComponent<Text>();
            emptyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            emptyText.fontSize = 28;
            emptyText.alignment = TextAnchor.MiddleCenter;
            emptyText.color = Color.white;
            emptyText.text = "暂无公告";
            GameObject back = CreateObject("ReturnButton", root.transform, typeof(Image), typeof(Button), typeof(ClientUiBackButton));
            back.GetComponent<Image>().color = new Color(0.86f, 0.75f, 0.28f, 1f);
            Center(back.GetComponent<RectTransform>(), new Vector2(0f, -640f), new Vector2(220f, 68f));
            CreateText("Label", back.transform, Vector2.zero, new Vector2(210f, 58f), 26, TextAnchor.MiddleCenter, Color.black).text = "返回主页";
            page.Configure(root, labels, detail, emptyHint);
            ClientUiPage uiPage = root.GetComponent<ClientUiPage>();
            if (uiPage == null)
                uiPage = root.AddComponent<ClientUiPage>();
            uiPage.Configure(ClientUiPageId.Notice);
        }

        private static void EnhanceHeroDetailEntry(Transform pagesLayer)
        {
            Transform root = pagesLayer.Find("HeroDetailPage");
            ClientHeroDetailPage detail = root != null ? root.GetComponent<ClientHeroDetailPage>() : null;
            if (detail == null || root.Find("EnhanceButton") != null) return;
            CreateButton("EnhanceButton", root, new Vector2(0f, -535f), "培养", detail.RequestUpgrade);
        }

        private static ClientUiPage BuildHeroEnhancePage(Transform pagesLayer)
        {
            Transform existing = pagesLayer.Find("HeroEnhancePage");
            if (existing != null) return existing.GetComponent<ClientUiPage>();
            GameObject root = CreateObject("HeroEnhancePage", pagesLayer);
            Stretch(root.GetComponent<RectTransform>());
            Image background = root.AddComponent<Image>();
            background.color = new Color(0.18f, 0.11f, 0.32f, 0.99f);
            background.raycastTarget = false;
            CreateText("TopBar_Title", root.transform, new Vector2(0f, 660f), new Vector2(600f, 68f), 42, TextAnchor.MiddleCenter, Color.white).text = "英雄培养";
            GameObject heroPanel = CreateObject("HeroInfoPanel", root.transform, typeof(Image));
            heroPanel.GetComponent<Image>().color = new Color(0.38f, 0.25f, 0.60f, 0.96f);
            Center(heroPanel.GetComponent<RectTransform>(), new Vector2(0f, 380f), new Vector2(640f, 200f));
            Text heroInfo = CreateText("HeroInfo", heroPanel.transform, Vector2.zero, new Vector2(580f, 150f), 30, TextAnchor.MiddleCenter, Color.white);
            GameObject costPanel = CreateObject("CostPanel", root.transform, typeof(Image));
            costPanel.GetComponent<Image>().color = new Color(0.12f, 0.09f, 0.24f, 0.96f);
            Center(costPanel.GetComponent<RectTransform>(), new Vector2(0f, 105f), new Vector2(640f, 180f));
            Text costInfo = CreateText("CostInfo", costPanel.transform, Vector2.zero, new Vector2(580f, 130f), 26, TextAnchor.MiddleCenter, Color.white);
            Text status = CreateText("Status", root.transform, new Vector2(0f, -80f), new Vector2(610f, 50f), 24, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.28f, 1f));
            ClientHeroEnhancePage page = root.AddComponent<ClientHeroEnhancePage>();
            CreateButton("UpgradeButton", root.transform, new Vector2(0f, -320f), "升级", page.Upgrade);
            GameObject back = CreateObject("ReturnButton", root.transform, typeof(Image), typeof(Button), typeof(ClientUiBackButton));
            back.GetComponent<Image>().color = new Color(0.86f, 0.75f, 0.28f, 1f);
            Center(back.GetComponent<RectTransform>(), new Vector2(0f, -640f), new Vector2(220f, 68f));
            CreateText("Label", back.transform, Vector2.zero, new Vector2(210f, 58f), 26, TextAnchor.MiddleCenter, Color.black).text = "返回详情";
            page.Configure(root, heroInfo, costInfo, status);
            ClientUiPage uiPage = root.AddComponent<ClientUiPage>();
            uiPage.Configure(ClientUiPageId.HeroEnhance);
            return uiPage;
        }

        private static ClientUiPage BuildFormationPage(Transform parent)
        {
            Transform existing=parent.Find("FormationPage"); if (existing!=null) return existing.GetComponent<ClientUiPage>();
            GameObject root=CreateObject("FormationPage",parent); Stretch(root.GetComponent<RectTransform>()); Image bg=root.AddComponent<Image>(); bg.color=new Color(.13f,.22f,.38f,.99f); bg.raycastTarget=false;
            CreateText("TopBar_Title",root.transform,new Vector2(0,660),new Vector2(600,68),42,TextAnchor.MiddleCenter,Color.white).text="编队";
            GameObject slot=CreateObject("FormationSlot",root.transform,typeof(Image)); slot.GetComponent<Image>().color=new Color(.25f,.48f,.68f,.96f); Center(slot.GetComponent<RectTransform>(),new Vector2(0,380),new Vector2(640,190)); Text slotText=CreateText("SlotText",slot.transform,Vector2.zero,new Vector2(560,140),30,TextAnchor.MiddleCenter,Color.white);
            ClientFormationPage page=root.AddComponent<ClientFormationPage>(); Text status=CreateText("Status",root.transform,new Vector2(0,100),new Vector2(600,60),24,TextAnchor.MiddleCenter,new Color(1,.84f,.28f,1));
            CreateButton("HeroOption1",root.transform,new Vector2(-165,-150),"演示英雄",()=>page.SelectHero("hero-001")); CreateButton("HeroOption2",root.transform,new Vector2(165,-150),"第二英雄",()=>page.SelectHero("hero-002"));
            GameObject back=CreateObject("ReturnButton",root.transform,typeof(Image),typeof(Button),typeof(ClientUiBackButton)); back.GetComponent<Image>().color=new Color(.86f,.75f,.28f,1); Center(back.GetComponent<RectTransform>(),new Vector2(0,-640),new Vector2(220,68)); CreateText("Label",back.transform,Vector2.zero,new Vector2(210,58),26,TextAnchor.MiddleCenter,Color.black).text="返回主页";
            page.Configure(root,slotText,status); ClientUiPage ui=root.AddComponent<ClientUiPage>(); ui.Configure(ClientUiPageId.Formation); return ui;
        }

        private static ClientUiPage BuildMarblePage(Transform parent)
        {
            Transform existing=parent.Find("MarblePage"); if (existing!=null) return existing.GetComponent<ClientUiPage>();
            GameObject root=CreateObject("MarblePage",parent); Stretch(root.GetComponent<RectTransform>()); Image bg=root.AddComponent<Image>(); bg.color=new Color(.30f,.15f,.35f,.99f); bg.raycastTarget=false;
            CreateText("TopBar_Title",root.transform,new Vector2(0,660),new Vector2(600,68),42,TextAnchor.MiddleCenter,Color.white).text="弹珠";
            GameObject equipped=CreateObject("EquippedPanel",root.transform,typeof(Image)); equipped.GetComponent<Image>().color=new Color(.55f,.27f,.58f,.96f); Center(equipped.GetComponent<RectTransform>(),new Vector2(0,360),new Vector2(640,210)); Text equippedText=CreateText("EquippedText",equipped.transform,Vector2.zero,new Vector2(560,160),30,TextAnchor.MiddleCenter,Color.white);
            ClientMarblePage page=root.AddComponent<ClientMarblePage>(); Text status=CreateText("Status",root.transform,new Vector2(0,80),new Vector2(600,60),24,TextAnchor.MiddleCenter,new Color(1,.84f,.28f,1));
            CreateButton("MarbleOption1",root.transform,new Vector2(-165,-150),"新手弹珠",()=>page.SelectMarble("marble-001")); CreateButton("MarbleOption2",root.transform,new Vector2(165,-150),"星辉弹珠",()=>page.SelectMarble("marble-002"));
            GameObject back=CreateObject("ReturnButton",root.transform,typeof(Image),typeof(Button),typeof(ClientUiBackButton)); back.GetComponent<Image>().color=new Color(.86f,.75f,.28f,1); Center(back.GetComponent<RectTransform>(),new Vector2(0,-640),new Vector2(220,68)); CreateText("Label",back.transform,Vector2.zero,new Vector2(210,58),26,TextAnchor.MiddleCenter,Color.black).text="返回主页";
            page.Configure(root,equippedText,status); ClientUiPage ui=root.AddComponent<ClientUiPage>(); ui.Configure(ClientUiPageId.Marble); return ui;
        }

        private static void CreateBadgeOption(string name, Transform parent, Vector2 position, int index)
        {
            GameObject option = CreateObject(name, parent, typeof(Image), typeof(Button), typeof(ClientBadgeButton));
            option.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BadgeSpriteRoot + "/默认背景底框.png");
            option.GetComponent<Image>().preserveAspect = true;
            Center(option.GetComponent<RectTransform>(), position, new Vector2(190f, 150f));
            CreateImage("Badge", option.transform, BadgeSpriteRoot + "/铭牌（小）/" + (index + 1) + ".png", Vector2.zero, new Vector2(160f, 90f));
            option.GetComponent<ClientBadgeButton>().Configure(index, false);
        }

        private static void ClearChildren(Transform parent)
        {
            for (int index = parent.childCount - 1; index >= 0; index--)
                Object.DestroyImmediate(parent.GetChild(index).gameObject);
        }

        private static void CreateConfirmButton(Transform parent, Vector2 position)
        {
            GameObject button = CreateObject("ConfirmButton", parent, typeof(Image), typeof(Button), typeof(ClientBadgeButton));
            button.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BadgeSpriteRoot + "/确定底框.png");
            button.GetComponent<Image>().preserveAspect = true;
            Center(button.GetComponent<RectTransform>(), position, new Vector2(220f, 78f));
            CreateImage("Label", button.transform, BadgeSpriteRoot + "/文字/确定.png", Vector2.zero, new Vector2(92f, 40f));
            button.GetComponent<ClientBadgeButton>().Configure(0, true);
        }

        private static void EnhanceHeroDetailPage(Transform pagesLayer)
        {
            Transform root = pagesLayer.Find("HeroDetailPage");
            if (root == null)
                return;
            ClientHeroDetailPage page = root.GetComponent<ClientHeroDetailPage>();
            if (page == null)
                return;
            Text element = FindOrCreateText(root, "Element", new Vector2(-150f, 315f), new Vector2(280f, 40f));
            Text star = FindOrCreateText(root, "StarLevel", new Vector2(150f, 315f), new Vector2(220f, 40f));
            Text ownership = FindOrCreateText(root, "Ownership", new Vector2(0f, 260f), new Vector2(260f, 42f));
            page.ConfigureDetails(element, star, ownership);
        }

        private static Text FindOrCreateText(Transform parent, string name, Vector2 position, Vector2 size)
        {
            Transform existing = parent.Find(name);
            return existing != null ? existing.GetComponent<Text>() : CreateText(name, parent, position, size, 24, TextAnchor.MiddleCenter, Color.white);
        }

        private static GameObject CreateItemSlot(string name, Transform parent, Vector2 position, string iconText, UnityEngine.Events.UnityAction action, out Text label)
        {
            GameObject target = CreateObject(name, parent, typeof(Image), typeof(Button));
            target.GetComponent<Image>().color = new Color(0.20f, 0.16f, 0.42f, 0.94f);
            Center(target.GetComponent<RectTransform>(), position, new Vector2(280f, 300f));
            target.GetComponent<Button>().onClick.AddListener(action);
            Text icon = CreateText("Icon", target.transform, new Vector2(0f, 65f), new Vector2(150f, 110f), 58, TextAnchor.MiddleCenter, new Color(1f, 0.80f, 0.28f, 1f));
            icon.text = iconText;
            label = CreateText("Label", target.transform, new Vector2(0f, -75f), new Vector2(250f, 90f), 25, TextAnchor.MiddleCenter, Color.white);
            return target;
        }

        private static void AddHeroCardVisuals(Transform card, bool isOwned)
        {
            if (card == null)
            {
                Debug.LogWarning("[Client] 英雄卡节点缺失，跳过视觉增补。");
                return;
            }

            if (card.Find("QualityFrame") == null)
                CreateImage("QualityFrame", card, HeroSpriteRoot + "/品质底框/底框.png", new Vector2(0f, 40f), new Vector2(220f, 230f));
            if (card.Find("Portrait") != null && !isOwned)
            {
                Image portrait = card.Find("Portrait").GetComponent<Image>();
                portrait.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HeroSpriteRoot + "/尚未绑定英雄图案.png");
            }
            if (card.Find("ElementIcon") == null)
            {
                string icon = isOwned ? "/属性icon/选中/火.png" : "/属性icon/未选中/水.png";
                CreateImage("ElementIcon", card, HeroSpriteRoot + icon, new Vector2(-82f, 122f), new Vector2(48f, 48f));
            }
            if (card.Find("Star1") == null)
            {
                string star = isOwned ? "/星级点亮.png" : "/星级未点亮.png";
                CreateImage("Star1", card, HeroSpriteRoot + star, new Vector2(-38f, -62f), new Vector2(30f, 30f));
                CreateImage("Star2", card, HeroSpriteRoot + star, new Vector2(0f, -62f), new Vector2(30f, 30f));
                CreateImage("Star3", card, HeroSpriteRoot + star, new Vector2(38f, -62f), new Vector2(30f, 30f));
            }
            if (!isOwned && card.Find("LockedOverlay") == null)
            {
                Image overlay = CreateImage("LockedOverlay", card, HeroSpriteRoot + "/黑底狂.png", new Vector2(0f, 45f), new Vector2(210f, 215f));
                overlay.color = new Color(1f, 1f, 1f, 0.45f);
                Text locked = CreateText("LockedLabel", card, new Vector2(0f, 35f), new Vector2(180f, 42f), 24, TextAnchor.MiddleCenter, Color.white);
                locked.text = "未拥有";
            }
        }

        private static void EnsureSprites()
        {
            EnsureSprites(SpriteRoot);
        }

        private static void EnsureSprites(string root)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.textureType == TextureImporterType.Sprite)
                    continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        private static Image CreateImage(string name, Transform parent, string path, Vector2 position, Vector2 size)
        {
            GameObject target = CreateObject(name, parent, typeof(Image));
            Image image = target.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            image.preserveAspect = true;
            image.raycastTarget = false;
            Center(target.GetComponent<RectTransform>(), position, size);
            return image;
        }

        private static Text CreateText(string name, Transform parent, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject target = CreateObject(name, parent, typeof(Text), typeof(Outline));
            Center(target.GetComponent<RectTransform>(), position, size);
            Text text = target.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            Outline outline = target.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
            outline.effectDistance = new Vector2(2f, -2f);
            return text;
        }

        private static void CreateButton(string name, Transform parent, Vector2 position, string label, UnityEngine.Events.UnityAction action)
        {
            GameObject target = CreateObject(name, parent, typeof(Image), typeof(Button));
            target.GetComponent<Image>().color = new Color(1f, 0.78f, 0.2f, 1f);
            Center(target.GetComponent<RectTransform>(), position, new Vector2(220f, 68f));
            target.GetComponent<Button>().onClick.AddListener(action);
            Text text = CreateText("Label", target.transform, Vector2.zero, new Vector2(210f, 58f), 26, TextAnchor.MiddleCenter, Color.black);
            text.text = label;
        }

        private static GameObject CreateObject(string name, Transform parent, params System.Type[] components)
        {
            System.Type[] types = new System.Type[components.Length + 1];
            types[0] = typeof(RectTransform);
            for (int index = 0; index < components.Length; index++)
                types[index + 1] = components[index];
            GameObject target = new GameObject(name, types);
            target.transform.SetParent(parent, false);
            return target;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Center(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
