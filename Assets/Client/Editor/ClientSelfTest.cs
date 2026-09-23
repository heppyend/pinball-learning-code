using System;
using System.Collections.Generic;
using System.IO;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.Stats;
using Pinball.Client.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client.Editor
{
    /// <summary>
    /// 客户端**纯逻辑自检套件**（阶段 A3：为高风险逻辑补自动化验证）。
    ///
    /// <para><b>为什么不用 Unity Test Framework</b>：`Assets/Client` 的代码位于**预定义程序集 `Assembly-CSharp`**，
    /// 而 asmdef 程序集**无法引用预定义程序集** ⇒ 测试程序集看不到 `Pinball.Client` 类型。
    /// 把客户端整体改成 asmdef 属**架构变更**（需负责人授权），因此当前采用
    /// "Editor 自检 + 命令行退出码"达成同等效果：**任一断言失败即以非 0 退出码结束**，可直接进 CI / 构建前门禁。</para>
    ///
    /// <para>运行方式（命令行，见 `WEBGL_B1_BUILD.md` 步骤①）：</para>
    /// <code>
    /// Unity.exe -batchmode -quit -projectPath &lt;工程&gt; \
    ///   -executeMethod Pinball.Client.Editor.ClientSelfTest.RunAll -logFile &lt;日志&gt;
    /// </code>
    ///
    /// <para>覆盖的都是**高风险纯逻辑**：表过滤与可见条数、元素映射、道具类型映射（含领取路径不变式）、
    /// 说明文本分档格式化、能力页签归属、以及**远端 pkg 读取链**（自建自删，绝不触碰负责人已有的 `config.pkg`）。</para>
    /// </summary>
    public static class ClientSelfTest
    {
        private const string HeroTableName = "Hero";

        private static int _passed;
        private static readonly List<string> _failures = new List<string>();

        /// <summary>
        /// 命令行 / 菜单入口：跑完全部检查，并在批处理模式下**以退出码结束进程**。
        /// 这个"自己结束进程"的行为对**单独调用**是刻意的（见类注释：可进 CI / 构建前门禁），
        /// 但它使本方法**无法被多步管线复用** —— 管线中途调用会直接把 Unity 杀掉。
        /// 需要在管线里复用请改用 <see cref="RunAllCore"/>。
        /// </summary>
        [MenuItem("Client/自检/运行全部纯逻辑自检", false, 80)]
        public static void RunAll()
        {
            RunAllCore();

            if (Application.isBatchMode)
                EditorApplication.Exit(_failures.Count == 0 ? 0 : 1);
        }

        /// <summary>
        /// 只跑检查、**不结束进程**，返回失败项数量（0 = 全通过）。
        /// 供多步管线（如 <c>ClientWebGLBuildCommand.RunFullPortPipeline</c>）在同一进程内复用；
        /// <see cref="RunAll"/> 保持原有 CLI 语义不变。
        /// </summary>
        public static int RunAllCore()
        {
            _passed = 0;
            _failures.Clear();

            ClientTableSource.Reset();
            ClientServices.Reset();
            ClientServices.InitializeForDevelopment();

            RunTableLoadChecks();
            RunElementMappingChecks();
            RunItemTypeChecks();
            RunDescriptionFormatterChecks();
            RunAbilitySectionChecks();
            RunCollectionVisibilityChecks();
            RunPackagedTableRoundTrip();
            // 场景接线回归（只读打开 ClientShell，不保存）
            RunSceneWiringChecks();

            Debug.Log("[自检] ========== 通过 " + _passed + " 项 ／ 失败 " + _failures.Count + " 项 ==========");
            for (int i = 0; i < _failures.Count; i++)
                Debug.LogError("[自检] 失败项：" + _failures[i]);

            return _failures.Count;
        }

        // ------------------------------------------------------------------
        // 断言帮助
        // ------------------------------------------------------------------

        private static void Check(bool condition, string what)
        {
            if (condition)
            {
                _passed++;
            }
            else
            {
                _failures.Add(what);
                Debug.LogError("[自检] ✗ " + what);
            }
        }

        private static void CheckEqual<T>(T actual, T expected, string what)
        {
            bool ok = EqualityComparer<T>.Default.Equals(actual, expected);
            Check(ok, what + "（实际 " + actual + "，期望 " + expected + "）");
        }

        // ------------------------------------------------------------------
        // 1) 表加载
        // ------------------------------------------------------------------

        private static void RunTableLoadChecks()
        {
            Check(ClientServices.IsInitialized, "客户端服务已初始化");
            Check(ClientServices.HasConfig, "配置服务已初始化");
            if (!ClientServices.HasConfig)
                return;

            Check(ClientServices.Config.IsReady, "配置表就绪（英雄表非空）");

            int heroes = ClientServices.Config.GetHeroes().Count;
            CheckEqual(heroes, 49, "英雄可见数（ClientShow != 0）应等于 49");

            int items = ClientServices.Config.GetItems().Count;
            CheckEqual(items, 9, "道具表行数应等于 9");
        }

        // ------------------------------------------------------------------
        // 2) 元素映射（1..6 = 光/水/土/风/火/暗）
        // ------------------------------------------------------------------

        private static void RunElementMappingChecks()
        {
            string[] expected = { "光", "水", "土", "风", "火", "暗" };
            for (int i = 0; i < expected.Length; i++)
            {
                int id = ClientElements.Min + i;
                CheckEqual(ClientElements.Name(id), expected[i], "元素 " + id + " 的名称");
                CheckEqual(ClientElements.FromName(expected[i]), id, "元素名「" + expected[i] + "」反查 id");
            }

            CheckEqual(ClientElements.AllIds().Length, 6, "元素 id 全集长度");

            // 边界：越界 id 不得抛异常
            ClientElements.Name(0);
            ClientElements.Name(99);
            _passed += 2;
        }

        // ------------------------------------------------------------------
        // 3) 道具类型映射（BUG-023 ② 的回归防线）
        // ------------------------------------------------------------------

        private static void RunItemTypeChecks()
        {
            if (!ClientServices.HasConfig)
                return;

            IReadOnlyList<ClientItemConfig> items = ClientServices.Config.GetItems();
            for (int i = 0; i < items.Count; i++)
            {
                ClientItemType expected =
                    items[i].Type == 1 ? ClientItemType.Currency :
                    items[i].Type == 3 ? ClientItemType.Consumable :
                    ClientItemType.Unknown;

                // 表内只应出现 1 与 3（2 未出现）——出现别的值即说明表变了，需要人来确认。
                Check(items[i].Type == 1 || items[i].Type == 3,
                      "道具 " + items[i].Id + " 的表 Type 只应为 1 或 3（实际 " + items[i].Type + "）");
                Check(expected != ClientItemType.Unknown,
                      "道具 " + items[i].Id + " 的类型应能映射（表 Type=" + items[i].Type + "）");
            }

            // 不变式：背包里每一项的 ItemType 必须等于其表 Type 的映射（领取路径曾写错成 Material）。
            IReadOnlyList<ClientInventoryItem> inventory = ClientServices.Data.GetInventory();
            for (int i = 0; i < inventory.Count; i++)
            {
                ClientItemConfig config;
                int tableType = ClientServices.Config.TryGetItem(inventory[i].ItemId, out config) && config != null
                    ? config.Type : -1;
                ClientItemType expected =
                    tableType == 1 ? ClientItemType.Currency :
                    tableType == 3 ? ClientItemType.Consumable :
                    ClientItemType.Unknown;
                CheckEqual(inventory[i].ItemType, expected,
                           "背包项 " + inventory[i].ItemId + " 的 ItemType（表 Type=" + tableType + "）");
            }

            // 领取路径：真的领一次，再校验入包道具类型（这是原先硬编码 Material 的地方）。
            string failure;
            ClientServices.Data.ClaimAllMails(out failure);
            IReadOnlyList<ClientInventoryItem> afterClaim = ClientServices.Data.GetInventory();
            for (int i = 0; i < afterClaim.Count; i++)
            {
                ClientItemConfig config;
                int tableType = ClientServices.Config.TryGetItem(afterClaim[i].ItemId, out config) && config != null
                    ? config.Type : -1;
                ClientItemType expected =
                    tableType == 1 ? ClientItemType.Currency :
                    tableType == 3 ? ClientItemType.Consumable :
                    ClientItemType.Unknown;
                CheckEqual(afterClaim[i].ItemType, expected,
                           "领取后背包项 " + afterClaim[i].ItemId + " 的 ItemType（表 Type=" + tableType + "）");
            }
        }

        // ------------------------------------------------------------------
        // 4) 说明文本分档格式化（真实写法是"每段各自带 %"）
        // ------------------------------------------------------------------

        private static void RunDescriptionFormatterChecks()
        {
            ClientDescriptionFormatter formatter = new ClientDescriptionFormatter();
            int[] tiers = { 1, 10, 20 };

            string atOne = formatter.Format(1, "回复其8%/9%/10%生命值（冷却3回合）", tiers, 1);
            Check(atOne != null && atOne.Contains("8%") && !atOne.Contains("8%/9%"),
                  "等级 1 应替换为第一档 8%（实际：" + atOne + "）");

            string atFifteen = formatter.Format(2, "回复其8%/9%/10%生命值（冷却3回合）", tiers, 15);
            Check(atFifteen != null && atFifteen.Contains("9%") && !atFifteen.Contains("/9%"),
                  "等级 15 应替换为第二档 9%（实际：" + atFifteen + "）");

            string sharedSuffix = formatter.Format(3, "造成60%/70%/80%攻击力的伤害", tiers, 20);
            Check(sharedSuffix != null && sharedSuffix.Contains("80%"),
                  "等级 20 应替换为末档 80%（实际：" + sharedSuffix + "）");

            string plain = formatter.Format(4, "冷却3回合，发射3枚追踪弹", tiers, 20);
            CheckEqual(plain, "冷却3回合，发射3枚追踪弹", "没有分档的文本应原样返回（单个数字不动）");
        }

        // ------------------------------------------------------------------
        // 5) 能力页签归属（天赋 / 秘技 / 终结技）
        // ------------------------------------------------------------------

        private static void RunAbilitySectionChecks()
        {
            if (!ClientServices.IsInitialized || !ClientServices.HasConfig)
                return;

            // 13001 是数据最全的英雄（天赋表共 13 行：12 条归天赋 + 1 条归秘技）
            ClientHeroAbilitySet set = ClientServices.Stats.GetHeroAbilities(13001);
            Check(set != null, "英雄 13001 的能力集应非空");
            if (set == null)
                return;

            CheckEqual(set.Sections.Count, 3, "页签数（天赋/秘技/终结技）");

            // 注意：`DisplayName` 是**页签名**（天赋/秘技/终结技），主技能名在 `Primary.Name`。
            string[] expectedSections = { "天赋", "秘技", "终结技" };
            string[] expectedPrimary = { "玉兔灵药", "月华追踪", "广寒月影" };
            int[] expectedEnhancements = { 12, 1, 0 };

            for (int i = 0; i < set.Sections.Count && i < expectedSections.Length; i++)
            {
                CheckEqual(set.Sections[i].DisplayName, expectedSections[i], "页签[" + i + "] 的名称");
                Check(set.Sections[i].Primary != null, "页签「" + expectedSections[i] + "」应有主技能");
                if (set.Sections[i].Primary != null)
                    CheckEqual(set.Sections[i].Primary.Name, expectedPrimary[i], "页签「" + expectedSections[i] + "」的主技能名");
                CheckEqual(set.Sections[i].Enhancements.Count, expectedEnhancements[i],
                           "页签「" + expectedSections[i] + "」的强化条数");
            }

            // **守恒式（比硬编码数字更强的防线）**：各页签强化条数之和应等于该英雄天赋表的行数 ——
            // 等于"没有任何一条被动被漏掉或重复归属"。13001：13 = 12 + 1 + 0。
            int totalPotencies = ClientServices.Config.GetHeroPotencies(13001).Count;
            int attributed = 0;
            for (int i = 0; i < set.Sections.Count; i++)
                attributed += set.Sections[i].Enhancements.Count;
            CheckEqual(attributed, totalPotencies, "各页签强化条数之和应等于英雄天赋表行数（无漏归属）");
        }

        // ------------------------------------------------------------------
        // 6) 收藏品可见条数（表过滤回归防线；也是 ProfilePage 的验收基准）
        // ------------------------------------------------------------------

        private static void RunCollectionVisibilityChecks()
        {
            if (!ClientServices.HasConfig)
                return;

            ClientCollectionKind[] kinds =
            {
                ClientCollectionKind.Head,
                ClientCollectionKind.Badge,
                ClientCollectionKind.Nameplate,
                ClientCollectionKind.HeadFrame,
                ClientCollectionKind.Title,
            };
            int[] expected = { 6, 6, 5, 9, 9 };   // 2026-09-21：负责人选 B（打开表里更多 ClientShow 行）后，五类收藏品全部行可见

            for (int i = 0; i < kinds.Length; i++)
            {
                int actual = ClientServices.Config.GetCollections(kinds[i]).Count;
                CheckEqual(actual, expected[i], "收藏品「" + kinds[i] + "」表内可见条数");
            }
        }

        // ------------------------------------------------------------------
        // 7) 远端 pkg 读取链（自建自删；**绝不覆盖负责人已有的 config.pkg**）
        // ------------------------------------------------------------------

        private static void RunPackagedTableRoundTrip()
        {
            string pkgPath = Path.Combine(Application.persistentDataPath, "config.pkg");
            if (File.Exists(pkgPath))
            {
                Debug.Log("[自检] 已存在真实 config.pkg，**跳过**远端读取链自检（不覆盖负责人数据）：" + pkgPath);
                return;
            }

            try
            {
                // 自建一个"只有 3 行 Hero"的 pkg：GZip → [version:1B] → 重复 [utf8\0][utf8\0]
                string sourceJson;
                if (!ClientTableSource.TryGetJson(HeroTableName, out sourceJson))
                {
                    Debug.LogWarning("[自检] 取不到 Hero 表 JSON，跳过远端读取链自检。");
                    return;
                }

                string[] rows = ExtractRows(sourceJson);
                if (rows.Length < 3)
                {
                    Debug.LogWarning("[自检] Hero 表行数不足 3，跳过远端读取链自检。");
                    return;
                }

                string mutated = "[" + rows[0] + "," + rows[1] + "," + rows[2] + "]";
                WritePackage(pkgPath, HeroTableName, mutated);

                ClientTableSource.Reset();
                ClientServices.Reset();
                ClientServices.InitializeForDevelopment();

                string source = ClientTableSource.DescribeSource();
                Check(source != null && source.Contains("config.pkg"),
                      "读表路径应识别到远端包（实际：" + source + "）");

                int heroes = ClientServices.Config.GetHeroes().Count;
                CheckEqual(heroes, 1, "远端包内 Hero 3 行 ⇒ 可见英雄应为 1（证明读的是包而不是本地表）");
            }
            catch (Exception exception)
            {
                _failures.Add("远端读取链自检抛异常：" + exception.Message);
                Debug.LogError("[自检] ✗ 远端读取链自检抛异常：" + exception);
            }
            finally
            {
                if (File.Exists(pkgPath))
                    File.Delete(pkgPath);

                // 复位到本地表，避免影响同进程后续自检
                ClientTableSource.Reset();
                ClientServices.Reset();
                ClientServices.InitializeForDevelopment();
                Debug.Log("[自检] 远端读取链自检已清理临时 pkg 并复位到本地表。");
            }
        }

        private static string[] ExtractRows(string json)
        {
            List<string> rows = new List<string>();
            int index = 0;
            while (index < json.Length)
            {
                int start = json.IndexOf('{', index);
                if (start < 0)
                    break;
                int end = json.IndexOf('}', start);
                if (end < 0)
                    break;
                rows.Add(json.Substring(start, end - start + 1));
                index = end + 1;
            }
            return rows.ToArray();
        }

        /// <summary>
        /// 写出一个合法的 pkg。**用 `System.IO.Compression` 是刻意的**：
        /// 这是**编辑器自检**代码（跑在 Mono/.NET 上，不进 WebGL 包），
        /// 运行时那条链已按第 58 轮改用工程同款 `Unity.IO.Compression`。
        /// </summary>
        private static void WritePackage(string path, string tableName, string content)
        {
            using (MemoryStream plain = new MemoryStream())
            {
                plain.WriteByte(1); // version
                WriteUtf8Z(plain, tableName);
                WriteUtf8Z(plain, content);

                byte[] raw = plain.ToArray();
                using (FileStream file = File.Create(path))
                using (System.IO.Compression.GZipStream gzip =
                       new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Compress))
                {
                    gzip.Write(raw, 0, raw.Length);
                }
            }
        }

        /// <summary>
        /// **场景接线回归断言**（2026-09-21 新增，配合 `BUG-026` 的处置结果）。
        ///
        /// 为什么放在这里：本轮（及前几轮）做的接线/新建节点，如果将来被一次场景编辑或 `RepairAll` 悄悄破坏，
        /// **运行时不报错、只是"点了没反应"** —— 那正是前面几轮反复踩的坑。所以把它们变成**退出码可判定**的断言。
        ///
        /// 做法：在批处理里**只读打开** `ClientShell.unity`（不保存、不修改），用 `SerializedObject`
        /// 读私有序列化字段，断言关键引用非空、关键节点存在。
        /// </summary>
        private static void RunSceneWiringChecks()
        {
            const string scenePath = "Assets/Client/Scenes/ClientShell.unity";
            if (!System.IO.File.Exists(scenePath))
            {
                Check(false, "ClientShell 场景文件存在");
                return;
            }

            UnityEngine.SceneManagement.Scene scene =
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(
                    scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
            Check(scene.IsValid(), "ClientShell 场景可打开（只读，不保存）");

            // ⓪ 代码新鲜度：本套件读的是**编辑器内存里的**场景，若编辑器还没导入最新脚本，
            //    下面针对 ClientHeroDetailToggleSkin 的断言会以"组件不存在"的形式误报。
            //    这里显式区分"代码没刷新"和"接线真的坏了"。
            Check(EditorCodeIsFresh(), "编辑器已导入最新脚本（Assembly-CSharp.dll 不比 Assets/**/*.cs 旧）");

            // ① 弹窗遮罩（BUG-026 ①）
            ClientPopupService popupService = UnityEngine.Object.FindObjectOfType<ClientPopupService>(true);
            Check(popupService != null, "场景内有 ClientPopupService");
            if (popupService != null)
                Check(ObjectRef(popupService, "_maskRoot") != null,
                      "ClientPopupService._maskRoot 已接线（弹窗遮罩/输入阻断）");

            // ② 大厅邮件红点（BUG-026 ②）
            ClientHomeRedDotController redDot = UnityEngine.Object.FindObjectOfType<ClientHomeRedDotController>(true);
            Check(redDot != null, "场景内有 ClientHomeRedDotController");
            if (redDot != null)
                Check(ObjectRef(redDot, "_mailDot") != null,
                      "ClientHomeRedDotController._mailDot 已接线（邮件红点）");

            // ③ 个人中心：改名链路所需节点 + 收藏品模板与容器（BUG-018 / BUG-026 ③ / ProfilePage 收口）
            ClientProfilePage profile = UnityEngine.Object.FindObjectOfType<ClientProfilePage>(true);
            Check(profile != null, "场景内有 ClientProfilePage");
            if (profile != null)
            {
                Check(ObjectRef(profile, "_playerName") != null, "ClientProfilePage._playerName 已接线（BUG-018）");
                Check(ObjectRef(profile, "_playerId") != null, "ClientProfilePage._playerId 已接线（BUG-018）");
                Check(ObjectRef(profile, "_combatPower") != null, "ClientProfilePage._combatPower 已接线（BUG-018）");
                Check(ObjectRef(profile, "_avatarTemplate") != null && ObjectRef(profile, "_avatarRoot") != null,
                      "收藏品「头像」模板与容器已接线");
                Check(ObjectRef(profile, "_badgeTemplate") != null && ObjectRef(profile, "_badgeRoot") != null,
                      "收藏品「徽章」模板与容器已接线");
                Check(ObjectRef(profile, "_nameplateTemplate") != null && ObjectRef(profile, "_nameplateRoot") != null,
                      "收藏品「铭牌」模板与容器已接线");
                Check(ObjectRef(profile, "_frameTemplate") != null && ObjectRef(profile, "_frameRoot") != null,
                      "收藏品「头像框」模板与容器已接线");
                Check(ObjectRef(profile, "_titleTemplate") != null && ObjectRef(profile, "_titleRoot") != null,
                      "收藏品「称号」模板与容器已接线");
                Check(ObjectRef(profile, "_showcaseTemplate") != null && ObjectRef(profile, "_showcaseRoot") != null,
                      "主页展示模板与容器已接线");
            }

            // 改名链路的按钮节点必须存在（绑定发生在运行时，这里断言"节点在、名字对"）
            Transform profileRoot = FindInScene("ProfilePage个人中心");
            Check(profileRoot != null, "场景内存在 ProfilePage个人中心 节点（含未激活）");
            if (profileRoot != null)
            {
                Check(FindDeep(profileRoot, "改名称") != null, "存在「改名称」按钮节点");
                Check(FindDeep(profileRoot, "ConfirmButton") != null, "存在改名弹窗「ConfirmButton」");
                Check(FindDeep(profileRoot, "CancelButton") != null, "存在改名弹窗「CancelButton」");
            }

            // ④ 排行榜阵容弹窗：阵容卡 + 关闭按钮（BUG-026 ⑤⑥）
            Transform lineup = FindInScene("Player lineup");
            Check(lineup != null, "场景内存在 Player lineup 节点（含未激活）");
            if (lineup != null)
            {
                // 与 ClientRankPage.CollectLineupCards 用**同一判据**：在整个阵容弹窗内按名收集，
                // 两种前缀都算（`展示卡牌` = 工具新建；`角色卡牌Button` = 场景既有卡）。
                int cards = 0;
                Transform[] all = lineup.GetComponentsInChildren<Transform>(true);
                System.Text.StringBuilder names = new System.Text.StringBuilder();
                for (int index = 0; index < all.Length; index++)
                {
                    string childName = all[index].name;
                    // 与代码同判据：排除滚动容器；卡 = 角色卡牌Button* 或 展示卡牌+数字
                    if (childName.IndexOf("Scroll", StringComparison.OrdinalIgnoreCase) >= 0)
                        continue;
                    bool isCard = childName.StartsWith("角色卡牌Button", StringComparison.Ordinal)
                                  || (childName.StartsWith("展示卡牌", StringComparison.Ordinal) &&
                                      childName.Length > "展示卡牌".Length &&
                                      char.IsDigit(childName["展示卡牌".Length]));
                    if (isCard)
                    {
                        cards++;
                        names.Append('[').Append(childName).Append(']');
                    }
                }
                Debug.Log("[自检] 阵容弹窗内卡牌节点 = " + cards + " 个 " + names);
                Check(cards >= 1, "阵容弹窗内至少有 1 张「展示卡牌」（实际 " + cards + "）");
                Check(FindDeep(lineup, "关闭Button") != null, "阵容弹窗存在「关闭Button」");
            }

            // ⑤ 大厅一键按钮的节点存在（MailPage 绑定 `All read button`）
            Transform mailPage = FindInScene("MailPage邮件");
            Check(mailPage != null, "场景内存在 MailPage邮件 节点（含未激活）");
            if (mailPage != null)
                Check(FindDeep(mailPage, "All read button") != null, "邮箱页存在「All read button」节点");

            // ⑥ 底栏图标（BUG-026 ④）：Bottom1..4 的 _icon 必须已接线
            BottomFunctionIconView[] bottomViews = UnityEngine.Object.FindObjectsOfType<BottomFunctionIconView>(true);
            Check(bottomViews.Length > 0, "场景内有 BottomFunctionIconView 实例");
            int wiredIcons = 0;
            for (int i = 0; i < bottomViews.Length; i++)
                if (ObjectRef(bottomViews[i], "_icon") != null)
                    wiredIcons++;
            Check(bottomViews.Length == 0 || wiredIcons == bottomViews.Length,
                  "所有 BottomFunctionIconView._icon 均已接线（" + wiredIcons + "/" + bottomViews.Length + "）");

            // ⑦ UI 适配 第 1 批（P0 回归）：英雄详情 3 个技能页签的选中态皮肤
            RunHeroDetailToggleChecks();

            // ⑧ UI 适配 第 2 批：英雄详情锚点 + 英雄页卡牌列表几何
            RunHeroPageLayoutChecks();
        }

        /// <summary>
        /// **UI 适配 第 1 批（P0 回归）断言** —— 负责人 2026-09-21 注明这三个页签"以前解决过"，
        /// 所以必须留**退出码可判定**的防回归机制。
        ///
        /// <para>断言四件事（全部是"坏了运行时不报错、只是点了没反应"的类型）：</para>
        /// <list type="number">
        /// <item>三个页签节点存在且 `activeSelf`；</item>
        /// <item>每个页签仍有 1 条持久 `SetActive` 调用（`onValueChanged`）—— 这是场景事件被工具误删的探针；</item>
        /// <item>`ClientHeroDetailToggleSkin` 已挂且 3 个绑定齐全（底框 + 文字贴图都非空）；</item>
        /// <item>三个页签的**选中框/文字贴图互不相同**（防止又变回"三张图混用/没接"）。</item>
        /// </list>
        /// </summary>
        private static void RunHeroDetailToggleChecks()
        {
            Transform page = FindInScene("HeroDetailPage英雄详情主页");
            Check(page != null, "场景内存在 HeroDetailPage英雄详情主页 节点（含未激活）");
            if (page == null)
                return;

            string[] names = { "天赋Toggle", "秘技Toggle", "终结技Toggle" };
            Toggle[] toggles = new Toggle[names.Length];
            System.Collections.Generic.HashSet<int> frameIds = new System.Collections.Generic.HashSet<int>();
            System.Collections.Generic.HashSet<int> labelIds = new System.Collections.Generic.HashSet<int>();

            for (int i = 0; i < names.Length; i++)
            {
                Transform node = FindUnderPage(page, "downCanvas/Panel/group/" + names[i]);
                Check(node != null, "英雄详情存在页签节点 " + names[i]);
                if (node == null)
                    continue;

                Check(node.gameObject.activeSelf, names[i] + " 的节点处于激活状态");

                Toggle toggle = node.GetComponent<Toggle>();
                toggles[i] = toggle;
                Check(toggle != null, names[i] + " 上挂着 Toggle 组件");
                Check(toggle != null && toggle.onValueChanged.GetPersistentEventCount() >= 1,
                      names[i] + " 仍有持久 SetActive 调用（>=" + (toggle != null ? toggle.onValueChanged.GetPersistentEventCount() : 0) + "）");

                Transform frame = node.Find("Background");
                Check(frame != null, names[i] + " 存在底框节点 Background");
                // ⚠️ 实测层级是 `Toggle/Background/Checkmark`。
                Transform label = node.Find("Checkmark");
                if (label == null && frame != null)
                    label = frame.Find("Checkmark");
                Check(label != null, names[i] + " 存在文字贴图节点 Checkmark（Toggle/Background/Checkmark）");
                Check(frame != null && frame.GetComponent<Image>() != null &&
                      frame.GetComponent<Image>().sprite != null,
                      names[i] + "/Background 已接「选中框」贴图");
                Check(frame != null && frame.GetComponent<Image>() != null &&
                      frame.GetComponent<Image>().enabled,
                      names[i] + "/Background 的 Image 处于 Enabled");
                Check(label != null && label.GetComponent<Image>() != null &&
                      label.GetComponent<Image>().sprite != null,
                      names[i] + "/Checkmark 已接「文字」贴图");
                Check(label != null && label.gameObject.activeSelf,
                      names[i] + "/Checkmark（文字贴图）处于激活状态");

                if (frame != null && frame.GetComponent<Image>() != null && frame.GetComponent<Image>().sprite != null)
                    frameIds.Add(frame.GetComponent<Image>().sprite.GetInstanceID());
                if (label != null && label.GetComponent<Image>() != null && label.GetComponent<Image>().sprite != null)
                    labelIds.Add(label.GetComponent<Image>().sprite.GetInstanceID());
            }

            Check(frameIds.Count == 3, "三个页签的「选中框」贴图互不相同（实际 " + frameIds.Count + " 张）");
            Check(labelIds.Count == 3, "三个页签的「文字」贴图互不相同（实际 " + labelIds.Count + " 张）");

            ClientHeroDetailToggleSkin skin = page.GetComponent<ClientHeroDetailToggleSkin>();
            Check(skin != null, "HeroDetailPage英雄详情主页 挂着 ClientHeroDetailToggleSkin");
            Check(skin != null && skin.CountValid() == 3,
                  "页签皮肤 3 个绑定齐全（实际 " + (skin != null ? skin.CountValid() : -1) + "）");
        }

        /// <summary>
        /// **UI 适配 第 2 批断言**：英雄详情两个锚点修正 + 英雄页卡牌 `Content` 几何。
        /// 这三处都是"矮画布（1080×1920 ⇒ 1339 单位）下才暴露"的问题，所以把锚点类型钉死。
        /// </summary>
        private static void RunHeroPageLayoutChecks()
        {
            Transform detail = FindInScene("HeroDetailPage英雄详情主页");
            if (detail != null)
            {
                RectTransform back = FindUnderPage(detail, "后退Button") as RectTransform;
                Check(back != null, "英雄详情存在 后退Button");
                if (back != null)
                    Check(Mathf.Approximately(back.anchorMin.y, 1f) && Mathf.Approximately(back.anchorMax.y, 1f),
                          "后退Button 纵向为贴顶锚（原中锚会在 1080×1920 下沉到屏幕外/被底栏压住），实际 anchorMin.y=" + back.anchorMin.y);

                RectTransform card = FindUnderPage(detail, "downCanvas/leftmiddleCanvas") as RectTransform;
                Check(card != null, "英雄详情存在 downCanvas/leftmiddleCanvas");
                if (card != null)
                {
                    Check(Mathf.Approximately(card.anchorMin.y, 1f) && Mathf.Approximately(card.anchorMax.y, 1f),
                          "leftmiddleCanvas 为贴 downCanvas 顶部锚（原中锚会上移压住 upCanvas/right），实际 anchorMin.y=" + card.anchorMin.y);
                    Check(card.anchoredPosition.y <= 0.5f,
                          "leftmiddleCanvas 位于 downCanvas 顶部（anchoredPosition.y=" + card.anchoredPosition.y + "）");
                }
            }

            Transform hero = FindInScene("HeroPage英雄");
            Check(hero != null, "场景内存在 HeroPage英雄 节点（含未激活）");
            if (hero == null)
                return;

            string[] subPages = { "HeroSubPage", "GuideSubPage" };
            for (int i = 0; i < subPages.Length; i++)
            {
                RectTransform content = FindUnderPage(hero,
                    subPages[i] + "/MiddleCanvas/Canvas/Scroll View/Viewport/Content") as RectTransform;
                Check(content != null, "英雄页存在 " + subPages[i] + " 卡牌 Content");
                if (content == null)
                    continue;

                Check(Mathf.Approximately(content.anchorMin.x, 0f) && Mathf.Approximately(content.anchorMax.x, 0f),
                      subPages[i] + " 卡牌 Content 水平为左锚（原中锚使整列卡牌落到滚动框左侧外面），实际 anchorMin.x=" + content.anchorMin.x);
                Check(Mathf.Approximately(content.anchoredPosition.x, 0f),
                      subPages[i] + " 卡牌 Content 水平 offset 为 0（实际 " + content.anchoredPosition.x + "）");
            }
        }

        /// <summary>
        /// 在**指定页面根**之下按相对路径查找（含未激活）。
        ///
        /// ⚠️ 不能直接用 `FindInScene("group")` 之类：场景里有 9 个 `upCanvas`/`downCanvas` 同名节点、
        /// 多个 `Panel`/`group`（`HeroPage英雄` 与 `HeroDetailPage英雄详情主页` 共用同一套 prefab 子层级），
        /// 必须**限定父级**才能拿到英雄详情自己的那三个页签。
        /// </summary>
        private static Transform FindUnderPage(Transform pageRoot, string relativePath)
        {
            if (pageRoot == null || string.IsNullOrEmpty(relativePath))
                return null;

            string[] parts = relativePath.Split('/');
            Transform current = pageRoot;
            for (int i = 0; i < parts.Length; i++)
            {
                if (current == null)
                    return null;
                Transform next = null;
                for (int c = 0; c < current.childCount; c++)
                {
                    if (current.GetChild(c).name == parts[i])
                    {
                        next = current.GetChild(c);
                        break;
                    }
                }
                current = next;
            }
            return current;
        }

        /// <summary>
        /// 编辑器是否已导入最新脚本 —— 用来把"代码没刷新"与"接线真的坏了"区分开。
        ///
        /// <para>由来（2026-09-21 实测）：场景里直接写盘加了 `ClientHeroDetailToggleSkin`，
        /// 但编辑器进程的 `Assembly-CSharp.dll` 比该 `.cs` 旧 ⇒ 内存里没有这个类型，
        /// 针对它的断言会以"组件不存在"误报。本断言保证**失败信息指向真正的根因**。</para>
        /// </summary>
        private static bool EditorCodeIsFresh()
        {
            string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectRoot))
                return true;

            string assembly = System.IO.Path.Combine(projectRoot, "Library/ScriptAssemblies/Assembly-CSharp.dll");
            if (!System.IO.File.Exists(assembly))
                return true;

            System.DateTime assemblyTime = System.IO.File.GetLastWriteTime(assembly);
            string[] roots = { "Assets/Client", "Assets/Main" };
            for (int i = 0; i < roots.Length; i++)
            {
                string root = System.IO.Path.Combine(projectRoot, roots[i]);
                if (!System.IO.Directory.Exists(root))
                    continue;

                string[] files = System.IO.Directory.GetFiles(root, "*.cs", System.IO.SearchOption.AllDirectories);
                for (int k = 0; k < files.Length; k++)
                {
                    if (System.IO.File.GetLastWriteTime(files[k]) > assemblyTime)
                    {
                        Debug.LogError("[自检] 编辑器代码过期：Assembly-CSharp.dll=" + assemblyTime.ToString("HH:mm:ss") +
                                       " < " + files[k].Substring(projectRoot.Length + 1).Replace('\\', '/') +
                                       "=" + System.IO.File.GetLastWriteTime(files[k]).ToString("HH:mm:ss") +
                                       " ⇒ 请先刷新/重新编译，再跑本自检。");
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>读一个组件的私有序列化对象引用字段（编辑器 API，不经反射）。</summary>
        private static UnityEngine.Object ObjectRef(UnityEngine.Object target, string fieldName)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            return property != null ? property.objectReferenceValue : null;
        }

        /// <summary>
        /// 在**整场景**内按名查找（**含未激活节点**）。
        ///
        /// ⚠️ 不能用 `GameObject.Find`：它**只返回激活对象**，而客户端页面/弹窗默认是隐藏的
        /// （`Player lineup`、`MailPage邮件` 等 `activeSelf=false`）⇒ 会误报"节点不存在"。
        /// </summary>
        private static Transform FindInScene(string name)
        {
            UnityEngine.SceneManagement.Scene scene =
                UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform found = FindDeep(roots[i].transform, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>按名深搜（含未激活节点）。</summary>
        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
                return null;
            if (root.name == name)
                return root;

            for (int index = 0; index < root.childCount; index++)
            {
                Transform found = FindDeep(root.GetChild(index), name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static void WriteUtf8Z(Stream stream, string value)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value ?? string.Empty);
            stream.Write(bytes, 0, bytes.Length);
            stream.WriteByte(0);
        }
    }
}
