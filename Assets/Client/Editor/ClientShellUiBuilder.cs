using Pinball.Client;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pinball.Client.Editor
{
    /// <summary>
    /// 将首版客户端 UI 生成为可在 Hierarchy 中直接编辑的节点。
    /// 该工具不会覆盖已有 ClientCanvas，避免破坏人工调整。
    /// </summary>
    public static class ClientShellUiBuilder
    {
        private const string ClientShellPath = "Assets/Client/Scenes/ClientShell.unity";
        private const string ProfileVisualPath = "Assets/Client/UI/ProfilePage/ProfileReference.png";
        private const string ShowcaseVisualPath = "Assets/Client/UI/HomeDisplay/HomeDisplayReference.png";
        private const string Hero001Path = "Assets/Client/UI/HomeDisplay/Hero001.png";
        private const string Hero002Path = "Assets/Client/UI/HomeDisplay/Hero002.png";
        private const string MainSpriteRoot = "Assets/Client/UI/MainPage/Sprites";
        private const string HeroSpriteRoot = "Assets/Client/UI/HeroPage/Sprites";
        private const string HeroDetailSpriteRoot = "Assets/Client/UI/HeroDetailPage/Sprites";
        private static readonly Vector2 ReferenceResolution = new Vector2(752f, 1630f);

        public static void Build()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ClientShellPath)
            {
                Debug.LogError("[Client] 请先打开 " + ClientShellPath + "，再执行 UI 生成。");
                return;
            }

            if (GameObject.Find("ClientCanvas") != null)
            {
                Debug.LogWarning("[Client] 已存在 ClientCanvas。为保护人工调整，本工具不会覆盖它；如需重新生成，请先手动删除该根节点。");
                return;
            }

            ClientBootstrap bootstrap = Object.FindObjectOfType<ClientBootstrap>();
            ClientPageNavigator navigator = Object.FindObjectOfType<ClientPageNavigator>();
            if (bootstrap == null || navigator == null)
            {
                Debug.LogError("[Client] ClientShell 缺少 ClientBootstrap 或 ClientPageNavigator，无法生成页面。");
                return;
            }

            EnsureMainPageSprites();
            EnsureSprites(HeroSpriteRoot);
            EnsureSprites(HeroDetailSpriteRoot);
            Sprite profileVisual = AssetDatabase.LoadAssetAtPath<Sprite>(ProfileVisualPath);
            Sprite showcaseVisual = AssetDatabase.LoadAssetAtPath<Sprite>(ShowcaseVisualPath);
            Sprite hero001 = AssetDatabase.LoadAssetAtPath<Sprite>(Hero001Path);
            Sprite hero002 = AssetDatabase.LoadAssetAtPath<Sprite>(Hero002Path);
            if (profileVisual == null || showcaseVisual == null || hero001 == null || hero002 == null)
            {
                Debug.LogError("[Client] 主页或个人中心参考图未导入为 Sprite，请检查 UI 资源及其 Import Settings。");
                return;
            }

            EnsureEventSystem();
            Canvas canvas = CreateCanvas(scene);
            ClientHomePage homePage = BuildHomePage(canvas.transform);
            ClientProfilePage profilePage = BuildProfilePage(canvas.transform, profileVisual, navigator);
            ClientHomeShowcasePage showcasePage = BuildHomeShowcasePage(canvas.transform, showcaseVisual, hero001, hero002, navigator);
            ClientFeaturePage featurePage = BuildFeaturePage(canvas.transform, navigator);
            ClientHeroPage heroPage = BuildHeroPage(canvas.transform, navigator);
            navigator.Configure(homePage, profilePage, showcasePage, featurePage);
            navigator.ConfigureHeroPage(heroPage);
            profilePage.Hide();
            showcasePage.Hide();
            featurePage.Hide();
            heroPage.Hide();

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("[Client] ClientShell 可视化 UI 已生成。请检查 Hierarchy 后保存场景。");
        }

        public static void AddHeroPage()
        {
            if (SceneManager.GetActiveScene().path != ClientShellPath || GameObject.Find("ClientCanvas") == null || GameObject.Find("HeroPage") != null)
            {
                Debug.LogWarning("[Client] 请在已有 ClientCanvas 的 ClientShell 中执行；HeroPage 已存在时不会覆盖。");
                return;
            }

            EnsureSprites(HeroSpriteRoot);
            ClientPageNavigator navigator = Object.FindObjectOfType<ClientPageNavigator>();
            ClientHeroPage page = BuildHeroPage(GameObject.Find("ClientCanvas").transform, navigator);
            navigator.ConfigureHeroPage(page);
            page.Hide();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        public static void CreateBottomFunctionIconPrefab()
        {
            const string prefabDirectory = "Assets/Client/UI/Common/Prefabs";
            const string prefabPath = prefabDirectory + "/BottomFunctionIcon.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.LogWarning("[Client] BottomFunctionIcon.prefab 已存在，为保护人工调整未覆盖。");
                return;
            }

            EnsureMainPageSprites();
            if (!AssetDatabase.IsValidFolder(prefabDirectory))
                AssetDatabase.CreateFolder("Assets/Client/UI/Common", "Prefabs");

            GameObject root = new GameObject("BottomFunctionIcon", typeof(RectTransform), typeof(Image), typeof(Button), typeof(BottomFunctionIconView));
            Image rootImage = root.GetComponent<Image>();
            rootImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MainSpriteRoot + "/底部功能icon/底框.png");
            rootImage.preserveAspect = true;
            SetCenteredRect(root.GetComponent<RectTransform>(), Vector2.zero, new Vector2(128f, 142f));

            CreateSpriteImage("IconFrame", root.transform, MainSpriteRoot + "/底部功能icon/icon底框.png", new Vector2(0f, 18f), new Vector2(90f, 90f));
            Image icon = CreateSpriteImage("Icon", root.transform, MainSpriteRoot + "/底部功能icon/联系客服.png", new Vector2(0f, 22f), new Vector2(76f, 76f));
            Text label = CreateText("Label", root.transform, new Vector2(0f, -48f), new Vector2(118f, 34f), 19, TextAnchor.MiddleCenter, Color.black);
            label.text = "联系客服";
            root.GetComponent<BottomFunctionIconView>().Initialize(icon, label);

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Debug.Log("[Client] 已创建 BottomFunctionIcon 预制体。");
        }

        public static void CreateBottomFunctionIconVariants()
        {
            const string prefabDirectory = "Assets/Client/UI/Common/Prefabs";
            const string sourcePath = prefabDirectory + "/BottomFunctionIcon.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath) == null)
            {
                Debug.LogError("[Client] 未找到母预制体 BottomFunctionIcon.prefab，请先创建并完成手动调整。");
                return;
            }

            string[] variantNames = { "Scan", "Shop", "Rank", "Mail" };
            string[] iconPaths =
            {
                MainSpriteRoot + "/底部功能icon/扫码.png",
                MainSpriteRoot + "/底部功能icon/商店.png",
                MainSpriteRoot + "/底部功能icon/排行榜.png",
                MainSpriteRoot + "/底部功能icon/邮件.png",
            };

            EnsureMainPageSprites();
            for (int index = 0; index < variantNames.Length; index++)
            {
                string targetPath = prefabDirectory + "/BottomFunctionIcon_" + variantNames[index] + ".prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(targetPath) != null)
                {
                    Debug.LogWarning("[Client] 已存在 " + targetPath + "，为保护现有调整未覆盖。");
                    continue;
                }

                if (!AssetDatabase.CopyAsset(sourcePath, targetPath))
                {
                    Debug.LogError("[Client] 复制预制体失败：" + targetPath);
                    continue;
                }

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(targetPath);
                Image icon = prefabRoot.GetComponent<BottomFunctionIconView>().IconImage;
                icon.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPaths[index]);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, targetPath);
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.SaveAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(prefabDirectory);
            Debug.Log("[Client] 已基于当前 BottomFunctionIcon 创建四个图标变体；只替换了 Icon Sprite。");
        }

        public static void CreateTeamAndGachaButtonPrefabs()
        {
            const string prefabDirectory = "Assets/Client/UI/Common/Prefabs";
            EnsureMainPageSprites();
            CreateMainCardButtonPrefab(
                prefabDirectory + "/TeamButton.prefab",
                "TeamButton",
                MainSpriteRoot + "/编队底框.png",
                MainSpriteRoot + "/编队人物.png",
                "编队");
            CreateMainCardButtonPrefab(
                prefabDirectory + "/GachaButton.prefab",
                "GachaButton",
                MainSpriteRoot + "/弹珠底框.png",
                MainSpriteRoot + "/弹珠人物.png",
                "扭蛋");
            AssetDatabase.SaveAssets();
        }

        public static void AddForwardIconToTeamAndGacha()
        {
            EnsureMainPageSprites();
            AddForwardIconToCardPrefab("Assets/Client/UI/Common/Prefabs/TeamButton.prefab");
            AddForwardIconToCardPrefab("Assets/Client/UI/Common/Prefabs/GachaButton.prefab");
            AssetDatabase.SaveAssets();
        }

        private static void AddForwardIconToCardPrefab(string prefabPath)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                Debug.LogError("[Client] 未找到预制体：" + prefabPath);
                return;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot.transform.Find("ForwardIcon") == null)
            {
                Image forwardIcon = CreateSpriteImage("ForwardIcon", prefabRoot.transform, MainSpriteRoot + "/前进icon.png", new Vector2(77f, -48f), new Vector2(52f, 52f));
                forwardIcon.transform.SetAsLastSibling();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        private static void CreateMainCardButtonPrefab(string prefabPath, string rootName, string framePath, string characterPath, string label)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            {
                Debug.LogWarning("[Client] 已存在 " + prefabPath + "，为保护现有调整未覆盖。");
                return;
            }

            GameObject root = new GameObject(rootName, typeof(RectTransform), typeof(Image), typeof(Button));
            Image rootImage = root.GetComponent<Image>();
            rootImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
            rootImage.preserveAspect = true;
            SetCenteredRect(root.GetComponent<RectTransform>(), Vector2.zero, new Vector2(230f, 190f));
            CreateSpriteImage("Character", root.transform, characterPath, new Vector2(0f, 12f), new Vector2(170f, 150f));
            Text title = CreateText("Label", root.transform, new Vector2(0f, -62f), new Vector2(180f, 45f), 31, TextAnchor.MiddleCenter, Color.black);
            title.text = label;
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            Debug.Log("[Client] 已创建 " + rootName + " 预制体。");
        }

        public static void AddHomeShowcasePage()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ClientShellPath || GameObject.Find("ClientCanvas") == null || GameObject.Find("HomeShowcasePage") != null)
            {
                Debug.LogWarning("[Client] 请打开已生成 UI 的 ClientShell；主页展示页已存在时不会覆盖。");
                return;
            }

            ClientPageNavigator navigator = Object.FindObjectOfType<ClientPageNavigator>();
            ClientHomePage homePage = Object.FindObjectOfType<ClientHomePage>();
            ClientProfilePage profilePage = Resources.FindObjectsOfTypeAll<ClientProfilePage>()[0];
            Sprite showcaseVisual = AssetDatabase.LoadAssetAtPath<Sprite>(ShowcaseVisualPath);
            Sprite hero001 = AssetDatabase.LoadAssetAtPath<Sprite>(Hero001Path);
            Sprite hero002 = AssetDatabase.LoadAssetAtPath<Sprite>(Hero002Path);
            ClientHomeShowcasePage page = BuildHomeShowcasePage(GameObject.Find("ClientCanvas").transform, showcaseVisual, hero001, hero002, navigator);
            ClientFeaturePage featurePage = Object.FindObjectOfType<ClientFeaturePage>();
            navigator.Configure(homePage, profilePage, page, featurePage);
            Button homeDisplayButton = profilePage.transform.Find("HomeDisplayButton").GetComponent<Button>();
            homeDisplayButton.onClick.RemoveAllListeners();
            homeDisplayButton.onClick.AddListener(profilePage.OpenHomeShowcase);
            page.Hide();
            EditorSceneManager.MarkSceneDirty(scene);
        }

        public static void AddPrimaryNavigationPages()
        {
            if (SceneManager.GetActiveScene().path != ClientShellPath || GameObject.Find("ClientCanvas") == null || GameObject.Find("FeaturePage") != null)
            {
                Debug.LogWarning("[Client] 请在已有 ClientCanvas 的 ClientShell 中执行；功能页已存在时不会覆盖。");
                return;
            }

            ClientPageNavigator navigator = Object.FindObjectOfType<ClientPageNavigator>();
            ClientHomePage homePage = Object.FindObjectOfType<ClientHomePage>();
            ClientProfilePage profilePage = Resources.FindObjectsOfTypeAll<ClientProfilePage>()[0];
            ClientHomeShowcasePage showcasePage = Resources.FindObjectsOfTypeAll<ClientHomeShowcasePage>()[0];
            ClientFeaturePage featurePage = BuildFeaturePage(GameObject.Find("ClientCanvas").transform, navigator);
            navigator.Configure(homePage, profilePage, showcasePage, featurePage);
            featurePage.Hide();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static Canvas CreateCanvas(Scene scene)
        {
            GameObject canvasObject = new GameObject("ClientCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasObject, scene);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void EnsureMainPageSprites()
        {
            EnsureSprites(MainSpriteRoot);
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

        private static ClientHeroPage BuildHeroPage(Transform canvas, ClientPageNavigator navigator)
        {
            GameObject pageRoot = CreatePageRoot("HeroPage", canvas);
            CreateSpriteImage("Background", pageRoot.transform, HeroSpriteRoot + "/背景.png", Vector2.zero, ReferenceResolution);
            Text title = CreateText("Title", pageRoot.transform, new Vector2(0f, 685f), new Vector2(400f, 55f), 36, TextAnchor.MiddleCenter, Color.white);
            title.text = "英雄 / 图鉴";
            Text status = CreateText("FilterStatus", pageRoot.transform, new Vector2(0f, 540f), new Vector2(420f, 42f), 24, TextAnchor.MiddleCenter, Color.white);
            ClientHeroPage page = pageRoot.AddComponent<ClientHeroPage>();
            CreateLabeledButton("HeroTab", pageRoot.transform, new Vector2(-120f, 600f), "英雄", () => page.SetElementFilter("All"));
            CreateLabeledButton("CodexTab", pageRoot.transform, new Vector2(120f, 600f), "图鉴", () => page.SetElementFilter("All"));
            CreateLabeledButton("AllFilter", pageRoot.transform, new Vector2(-220f, 485f), "全", () => page.SetElementFilter("All"));
            CreateLabeledButton("FireFilter", pageRoot.transform, new Vector2(-75f, 485f), "火", () => page.SetElementFilter("Fire"));
            CreateLabeledButton("WaterFilter", pageRoot.transform, new Vector2(75f, 485f), "水", () => page.SetElementFilter("Water"));
            CreateLabeledButton("WindFilter", pageRoot.transform, new Vector2(220f, 485f), "风", () => page.SetElementFilter("Wind"));
            GameObject owned = CreateHeroCard("OwnedHeroCard", pageRoot.transform, HeroSpriteRoot + "/底框/红.png", new Vector2(-175f, 120f), "演示英雄", true, page.SelectOwnedHero);
            GameObject locked = CreateHeroCard("LockedHeroCard", pageRoot.transform, HeroSpriteRoot + "/底框/蓝未激活.png", new Vector2(175f, 120f), "未拥有英雄", false, page.SelectLockedHero);
            CreateLabeledButton("ReturnButton", pageRoot.transform, new Vector2(0f, -650f), "返回主页", navigator.ReturnFromHero);
            page.Configure(pageRoot, owned, locked, status);
            return page;
        }

        private static GameObject CreateHeroCard(string name, Transform parent, string framePath, Vector2 position, string label, bool owned, UnityEngine.Events.UnityAction action)
        {
            GameObject card = CreateUiObject(name, parent, typeof(Image), typeof(Button));
            card.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
            SetCenteredRect(card.GetComponent<RectTransform>(), position, new Vector2(250f, 340f));
            card.GetComponent<Button>().onClick.AddListener(action);
            CreateSpriteImage("Portrait", card.transform, HeroSpriteRoot + "/未选中英雄参考.png", new Vector2(0f, 35f), new Vector2(210f, 215f));
            Text text = CreateText("Name", card.transform, new Vector2(0f, -105f), new Vector2(220f, 42f), 23, TextAnchor.MiddleCenter, Color.white);
            text.text = label + (owned ? "\nLv.1" : "\n未拥有");
            return card;
        }

        private static ClientHomePage BuildHomePage(Transform canvas)
        {
            GameObject pageRoot = CreatePageRoot("MainPage", canvas);
            ClientHomePage page = pageRoot.AddComponent<ClientHomePage>();
            // 首页不使用整张参考图：每个可见元素都是可在 Hierarchy 选中的独立节点。
            CreateSpriteImage("Background", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/背景.png", Vector2.zero, ReferenceResolution);
            CreateSpriteImage("ShowcaseHero", pageRoot.transform, Hero001Path, new Vector2(0f, -50f), new Vector2(520f, 640f));
            CreateSpriteImage("AvatarFrame", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/头像框.png", new Vector2(-315f, 690f), new Vector2(130f, 130f));
            CreateSpriteImage("Avatar", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/头像1.png", new Vector2(-315f, 690f), new Vector2(110f, 110f));
            CreateSpriteImage("TopFunctionPanel", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/上功能底框.png", new Vector2(80f, 690f), new Vector2(520f, 110f));
            CreateSpriteImage("CombatPowerPanel", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/战斗力底框.png", new Vector2(-160f, 640f), new Vector2(260f, 42f));
            CreateSpriteButton("TeamButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/编队底框.png", new Vector2(-185f, -130f), new Vector2(230f, 190f), () => page.RequestNavigation(ClientHomeDestination.Hero));
            CreateSpriteImage("TeamCharacter", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/编队人物.png", new Vector2(-185f, -115f), new Vector2(170f, 150f));
            CreateSpriteButton("GachaButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/弹珠底框.png", new Vector2(185f, -130f), new Vector2(230f, 190f), () => page.RequestNavigation(ClientHomeDestination.Gacha));
            CreateSpriteImage("MarbleCharacter", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/弹珠人物.png", new Vector2(185f, -115f), new Vector2(170f, 150f));
            CreateSpriteImage("BottomNavigationFrame", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/底部功能框.png", new Vector2(0f, -710f), new Vector2(752f, 190f));
            Text playerName = CreateText("PlayerName", pageRoot.transform, new Vector2(-235f, 696f), new Vector2(190f, 38f), 24, TextAnchor.MiddleLeft, Color.white);
            Text combatPower = CreateText("CombatPower", pageRoot.transform, new Vector2(-110f, 646f), new Vector2(210f, 32f), 20, TextAnchor.MiddleLeft, Color.white);
            Text notice = CreateText("Notice", pageRoot.transform, new Vector2(0f, -720f), new Vector2(590f, 45f), 20, TextAnchor.MiddleCenter, Color.white);
            notice.gameObject.SetActive(false);
            page.Configure(pageRoot, playerName, combatPower, notice);

            CreateHotspot("AvatarButton", pageRoot.transform, new Vector2(-315f, 690f), new Vector2(130f, 130f), () => page.RequestNavigation(ClientHomeDestination.Profile));
            CreateSpriteButton("ProfileButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/底部icon/个人中心1.png", new Vector2(-300f, -710f), new Vector2(110f, 120f), () => page.RequestNavigation(ClientHomeDestination.Profile));
            CreateSpriteButton("MarbleButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/底部icon/弹珠1.png", new Vector2(-150f, -710f), new Vector2(110f, 120f), () => page.RequestNavigation(ClientHomeDestination.Hero));
            CreateSpriteButton("HomeButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/底部icon/主页1.png", new Vector2(0f, -710f), new Vector2(110f, 120f), () => page.ShowNotice("当前已在主页"));
            CreateSpriteButton("BackpackButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/底部icon/背包1.png", new Vector2(150f, -710f), new Vector2(110f, 120f), () => page.RequestNavigation(ClientHomeDestination.Backpack));
            CreateSpriteButton("ActivityButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/底部icon/活动1.png", new Vector2(300f, -710f), new Vector2(110f, 120f), () => page.RequestNavigation(ClientHomeDestination.Activity));
            CreateSpriteButton("RankButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/底部功能icon/排行榜.png", new Vector2(285f, -480f), new Vector2(110f, 110f), () => page.RequestNavigation(ClientHomeDestination.Rank));
            CreateSpriteButton("ShopButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/底部功能icon/商店.png", new Vector2(95f, -480f), new Vector2(110f, 110f), () => page.RequestNavigation(ClientHomeDestination.Shop));
            CreateSpriteButton("MailButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/底部功能icon/邮件.png", new Vector2(285f, -610f), new Vector2(110f, 110f), () => page.RequestNavigation(ClientHomeDestination.Mail));
            CreateSpriteButton("SupportButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/底部功能icon/联系客服.png", new Vector2(-95f, -480f), new Vector2(110f, 110f), () => page.RequestNavigation(ClientHomeDestination.Notice));
            CreateSpriteButton("ScanButton", pageRoot.transform, "Assets/Client/UI/MainPage/Sprites/底部功能icon/扫码.png", new Vector2(-285f, -480f), new Vector2(110f, 110f), () => page.RequestNavigation(ClientHomeDestination.Notice));
            return page;
        }

        private static Image CreateSpriteImage(string name, Transform parent, string path, Vector2 position, Vector2 size)
        {
            GameObject imageObject = CreateUiObject(name, parent, typeof(Image));
            Image image = imageObject.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            image.preserveAspect = true;
            image.raycastTarget = false;
            SetCenteredRect(imageObject.GetComponent<RectTransform>(), position, size);
            return image;
        }

        private static void CreateSpriteButton(string name, Transform parent, string path, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = CreateUiObject(name, parent, typeof(Image), typeof(Button));
            Image image = buttonObject.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            image.preserveAspect = true;
            SetCenteredRect(buttonObject.GetComponent<RectTransform>(), position, size);
            buttonObject.GetComponent<Button>().onClick.AddListener(action);
        }

        private static ClientProfilePage BuildProfilePage(Transform canvas, Sprite visual, ClientPageNavigator navigator)
        {
            GameObject pageRoot = CreatePageRoot("ProfilePage", canvas);
            ClientProfilePage page = pageRoot.AddComponent<ClientProfilePage>();
            CreateReferenceVisual("ProfileReference", pageRoot.transform, visual);
            Text playerName = CreateText("PlayerName", pageRoot.transform, new Vector2(-38f, 434f), new Vector2(210f, 36f), 24, TextAnchor.MiddleCenter, Color.white);
            Text playerId = CreateText("PlayerId", pageRoot.transform, new Vector2(-22f, 385f), new Vector2(230f, 28f), 20, TextAnchor.MiddleCenter, Color.white);
            Text combatPower = CreateText("CombatPower", pageRoot.transform, new Vector2(105f, 150f), new Vector2(210f, 46f), 28, TextAnchor.MiddleCenter, Color.white);
            Text notice = CreateText("Notice", pageRoot.transform, new Vector2(0f, -635f), new Vector2(550f, 42f), 20, TextAnchor.MiddleCenter, Color.white);
            notice.gameObject.SetActive(false);

            CreateHotspot("ReturnButton", pageRoot.transform, new Vector2(-325f, -735f), new Vector2(140f, 105f), navigator.ReturnToHome);
            CreateHotspot("RenameButton", pageRoot.transform, new Vector2(88f, 434f), new Vector2(70f, 60f), page.OpenRenameDialog);
            CreateHotspot("HomeDisplayButton", pageRoot.transform, new Vector2(150f, 580f), new Vector2(190f, 80f), page.OpenHomeShowcase);
            CreateHotspot("PersonalizeButton", pageRoot.transform, new Vector2(315f, 580f), new Vector2(160f, 80f), () => page.ShowNotice("个性化页面待开发"));
            CreateHotspot("MarbleDisplayButton", pageRoot.transform, new Vector2(0f, -70f), new Vector2(520f, 80f), () => page.ShowNotice("弹珠展示页面待开发"));
            CreateHotspot("BindIdButton", pageRoot.transform, new Vector2(0f, -480f), new Vector2(310f, 100f), () => page.ShowNotice("ID 卡绑定待平台能力确认"));

            GameObject renameDialog = BuildRenameDialog(pageRoot.transform, page, out InputField renameInput);
            page.Configure(pageRoot, playerName, playerId, combatPower, notice, renameDialog, renameInput);
            return page;
        }

        private static ClientHomeShowcasePage BuildHomeShowcasePage(Transform canvas, Sprite visual, Sprite hero001, Sprite hero002, ClientPageNavigator navigator)
        {
            GameObject pageRoot = CreatePageRoot("HomeShowcasePage", canvas);
            ClientHomeShowcasePage page = pageRoot.AddComponent<ClientHomeShowcasePage>();
            CreateReferenceVisual("HomeShowcaseReference", pageRoot.transform, visual);
            GameObject preview = CreateUiObject("SelectedHeroPreview", pageRoot.transform, typeof(Image));
            SetCenteredRect(preview.GetComponent<RectTransform>(), new Vector2(-240f, -145f), new Vector2(210f, 270f));
            Text heroName = CreateText("HeroName", pageRoot.transform, new Vector2(-225f, -350f), new Vector2(250f, 44f), 20, TextAnchor.MiddleCenter, Color.black);
            Text notice = CreateText("Notice", pageRoot.transform, new Vector2(0f, 650f), new Vector2(560f, 42f), 20, TextAnchor.MiddleCenter, Color.white);
            notice.gameObject.SetActive(false);
            page.Configure(pageRoot, preview.GetComponent<Image>(), heroName, notice, hero001, hero002);
            CreateHotspot("Hero001Button", pageRoot.transform, new Vector2(-210f, -140f), new Vector2(230f, 310f), () => page.SelectHero("hero-001"));
            CreateHotspot("Hero002Button", pageRoot.transform, new Vector2(50f, -140f), new Vector2(230f, 310f), () => page.SelectHero("hero-002"));
            CreateHotspot("ConfirmButton", pageRoot.transform, new Vector2(0f, -560f), new Vector2(240f, 90f), page.ConfirmSelection);
            CreateHotspot("ReturnButton", pageRoot.transform, new Vector2(-330f, -735f), new Vector2(130f, 100f), navigator.ReturnToProfile);
            return page;
        }

        private static ClientFeaturePage BuildFeaturePage(Transform canvas, ClientPageNavigator navigator)
        {
            GameObject pageRoot = CreatePageRoot("FeaturePage", canvas);
            Image panel = pageRoot.AddComponent<Image>();
            panel.color = new Color(0.16f, 0.12f, 0.34f, 0.98f);
            ClientFeaturePage page = pageRoot.AddComponent<ClientFeaturePage>();
            Text title = CreateText("Title", pageRoot.transform, new Vector2(0f, 590f), new Vector2(600f, 70f), 40, TextAnchor.MiddleCenter, Color.white);
            Text content = CreateText("Content", pageRoot.transform, new Vector2(0f, 40f), new Vector2(590f, 800f), 27, TextAnchor.UpperLeft, Color.white);
            page.Configure(pageRoot, title, content);
            CreateLabeledButton("ReturnButton", pageRoot.transform, new Vector2(0f, -620f), "返回主页", navigator.ReturnFromFeature);
            return page;
        }

        private static GameObject BuildRenameDialog(Transform parent, ClientProfilePage page, out InputField inputField)
        {
            GameObject dialog = CreateUiObject("RenameDialog", parent, typeof(Image));
            Image panel = dialog.GetComponent<Image>();
            panel.color = new Color(0.22f, 0.16f, 0.42f, 0.97f);
            SetCenteredRect(dialog.GetComponent<RectTransform>(), Vector2.zero, new Vector2(550f, 290f));
            CreateText("Title", dialog.transform, new Vector2(0f, 90f), new Vector2(420f, 45f), 30, TextAnchor.MiddleCenter, Color.white).text = "修改玩家名称";

            GameObject inputObject = CreateUiObject("NameInput", dialog.transform, typeof(Image), typeof(InputField));
            inputObject.GetComponent<Image>().color = Color.white;
            SetCenteredRect(inputObject.GetComponent<RectTransform>(), new Vector2(0f, 15f), new Vector2(440f, 66f));
            inputField = inputObject.GetComponent<InputField>();
            Text inputText = CreateText("Text", inputObject.transform, Vector2.zero, new Vector2(410f, 56f), 26, TextAnchor.MiddleLeft, Color.black);
            Text placeholder = CreateText("Placeholder", inputObject.transform, Vector2.zero, new Vector2(410f, 56f), 22, TextAnchor.MiddleLeft, new Color(0.35f, 0.35f, 0.35f, 0.7f));
            placeholder.text = "请输入新的昵称";
            inputField.textComponent = inputText;
            inputField.placeholder = placeholder;

            CreateLabeledButton("CancelButton", dialog.transform, new Vector2(-125f, -90f), "取消", page.CancelRename);
            CreateLabeledButton("ConfirmButton", dialog.transform, new Vector2(125f, -90f), "确认", page.ConfirmRename);
            dialog.SetActive(false);
            return dialog;
        }

        private static GameObject CreatePageRoot(string name, Transform parent)
        {
            GameObject pageRoot = CreateUiObject(name, parent);
            RectTransform rect = pageRoot.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return pageRoot;
        }

        private static void CreateReferenceVisual(string name, Transform parent, Sprite sprite)
        {
            GameObject visual = CreateUiObject(name, parent, typeof(Image));
            Image image = visual.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            SetCenteredRect(visual.GetComponent<RectTransform>(), Vector2.zero, ReferenceResolution);
        }

        private static void CreateHotspot(string name, Transform parent, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = CreateUiObject(name, parent, typeof(Image), typeof(Button));
            buttonObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            SetCenteredRect(buttonObject.GetComponent<RectTransform>(), position, size);
            buttonObject.GetComponent<Button>().onClick.AddListener(action);
        }

        private static void CreateLabeledButton(string name, Transform parent, Vector2 position, string label, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = CreateUiObject(name, parent, typeof(Image), typeof(Button));
            buttonObject.GetComponent<Image>().color = new Color(1f, 0.78f, 0.2f, 1f);
            SetCenteredRect(buttonObject.GetComponent<RectTransform>(), position, new Vector2(185f, 58f));
            buttonObject.GetComponent<Button>().onClick.AddListener(action);
            Text text = CreateText("Label", buttonObject.transform, Vector2.zero, new Vector2(180f, 52f), 24, TextAnchor.MiddleCenter, Color.black);
            text.text = label;
        }

        private static Text CreateText(string name, Transform parent, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment, Color color)
        {
            GameObject textObject = CreateUiObject(name, parent, typeof(Text), typeof(Outline));
            SetCenteredRect(textObject.GetComponent<RectTransform>(), position, size);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            Outline outline = textObject.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
            outline.effectDistance = new Vector2(2f, -2f);
            return text;
        }

        private static GameObject CreateUiObject(string name, Transform parent, params System.Type[] components)
        {
            System.Type[] allComponents = new System.Type[components.Length + 1];
            allComponents[0] = typeof(RectTransform);
            for (int index = 0; index < components.Length; index++)
                allComponents[index + 1] = components[index];

            GameObject gameObject = new GameObject(name, allComponents);
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void SetCenteredRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
