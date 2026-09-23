using System;
using LC.Newtonsoft.Json.Linq;
using Pinball.Client.Services;
using UnityEditor;
using UnityEngine;

namespace Pinball.Client.Editor
{
    /// <summary>
    /// 客户端配置表探针（**只读、可回收**）。
    ///
    /// 它验证的是 **`Assets/Client` 自己的读表路径**（`ClientTableSource` → `Resources/Table/*.json`
    /// 或落盘的远端 `config.pkg`），**不**触碰 `HotFix` 里的 `TableManager` / `T*Helper` ——
    /// 按负责人 2026-09-20 的边界：只负责 Client 与平台移植。
    ///
    /// 为什么需要它：`TableLoadHelper` 那套在缺表时只打一条警告就"按空表继续"，UI 会安静地什么都不显示；
    /// 而客户端这条路径也必须能一句话回答"表到底读到没有、读的是哪一份"。
    ///
    /// 用法：菜单 `Client/配置表/打印客户端配置表行数`；或命令行
    /// `-batchmode -quit -executeMethod Pinball.Client.Editor.ClientTableProbe.LogRowCounts`。
    /// </summary>
    public static class ClientTableProbe
    {
        /// <summary>客户端需要的表 —— 刻意不含战斗侧的表（Skill / Monster / MapCopy / Coefficient …）。</summary>
        private static readonly string[] ClientTables =
        {
            "Hero", "Head", "HeadFrame", "Badge", "Nameplate", "Title",
            "Item", "Skill", "Potency", "RandomHero",
        };

        [MenuItem("Client/配置表/打印客户端配置表行数", false, 60)]
        public static void LogRowCounts()
        {
            ClientTableSource.Reset();

            Debug.Log("[配置表] 客户端读表路径：" + ClientTableSource.DescribeSource());

            int ready = 0;
            int empty = 0;
            int missing = 0;
            int broken = 0;

            for (int i = 0; i < ClientTables.Length; i++)
            {
                string tableName = ClientTables[i];

                string json;
                if (!ClientTableSource.TryGetJson(tableName, out json))
                {
                    Debug.LogWarning("[配置表] " + tableName + "：找不到（本地 Resources/Table/" + tableName +
                                     ".json 缺失，远端 config.pkg 里也没有）");
                    missing++;
                    continue;
                }

                try
                {
                    JArray rows = JArray.Parse(json);
                    if (rows.Count == 0)
                    {
                        Debug.LogWarning("[配置表] " + tableName + "：解析出 0 行");
                        empty++;
                        continue;
                    }

                    int visible = 0;
                    for (int r = 0; r < rows.Count; r++)
                    {
                        JToken flag = rows[r]["ClientShow"];
                        if (flag != null && flag.Type != JTokenType.Null)
                        {
                            try { if (flag.Value<int>() != 0) visible++; }
                            catch { /* 字段类型异常时不计入可见 */ }
                        }
                    }

                    Debug.Log("[配置表] " + tableName + " = " + rows.Count + " 行（ClientShow!=0 的 " + visible + " 行）");
                    ready++;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[配置表] " + tableName + "：解析失败 " + ex.Message);
                    broken++;
                }
            }

            Debug.Log("[配置表] 汇总：可用 " + ready + " 张 / 空 " + empty + " 张 / 缺失 " + missing +
                      " 张 / 解析失败 " + broken + " 张（共 " + ClientTables.Length + " 张）");

            if (ready == 0)
            {
                Debug.LogError("[配置表] 客户端一张表都没读到 —— UI 将没有数据。" +
                               "检查 Assets/Resources/Table/*.json，或磁盘上遗留的旧 config.pkg（BUG-021）。");
                return;
            }

            // ---- 顺带验证「表驱动」整条链 ----
            // `LocalClientDataService` 会用配置表构建英雄列表（静态来自表、玩家状态模拟），
            // 这一步同时把它的构造函数跑一遍 —— 那是本轮 int 化改动最密集的地方。
            ClientServices.InitializeForDevelopment();

            System.Collections.Generic.IReadOnlyList<Pinball.Client.Domain.ClientHero> heroes =
                ClientServices.Data.GetHeroes();

            if (heroes.Count == 0)
            {
                Debug.LogError("[配置表] 英雄列表为空 —— 表驱动没接上。");
                return;
            }

            Pinball.Client.Domain.ClientHero first = heroes[0];
            Debug.Log("[配置表] 英雄列表 = " + heroes.Count + " 个；首个 = Id " + first.HeroId +
                      " / " + first.Name + " / 元素" + first.ElementId + "(" + first.ElementName + ")" +
                      " / 品质" + first.Quality + " / " + first.StarLevel + "星 / 拥有=" + first.IsOwned +
                      " / 战力=" + first.CombatPower + "【战力待确认】");

            // ---- 表间关联：英雄 → 技能 / 天赋（验证 `THero.Skill` / `THero.Potency` 真的解析出来了）----
            System.Collections.Generic.IReadOnlyList<Pinball.Client.Domain.ClientSkillConfig> skills =
                ClientServices.Config.GetHeroSkills(first.HeroId);
            Debug.Log("[配置表] 英雄 " + first.HeroId + " 技能 " + skills.Count + " 个" +
                      (skills.Count > 0
                          ? "；首个 = " + skills[0].Id + " / " + skills[0].Name + " / Type" + skills[0].Type
                          : "  <- 关联没解析出来（检查 THero.Skill 字段）"));

            System.Collections.Generic.IReadOnlyList<Pinball.Client.Domain.ClientPotencyConfig> potencies =
                ClientServices.Config.GetHeroPotencies(first.HeroId);
            Debug.Log("[配置表] 英雄 " + first.HeroId + " 天赋 " + potencies.Count + " 个" +
                      (potencies.Count > 0
                          ? "；首个 = " + potencies[0].Id + " / " + potencies[0].Name + " / " + potencies[0].Desc
                          : "  <- 关联没解析出来（检查 THero.Potency 字段）"));

            System.Collections.Generic.IReadOnlyList<Pinball.Client.Domain.ClientItemConfig> items =
                ClientServices.Config.GetItems();
            Debug.Log("[配置表] 道具 " + items.Count + " 个" +
                      (items.Count > 0
                          ? "；首个 = " + items[0].Id + " / " + items[0].Name + " / Type" + items[0].Type + " / " + items[0].Icon
                          : ""));

            System.Collections.Generic.IReadOnlyList<Pinball.Client.Domain.ClientRandomHeroConfig> pool =
                ClientServices.Config.GetRandomHeroPool();
            Debug.Log("[配置表] 随机英雄池 " + pool.Count + " 条" +
                      (pool.Count > 0 ? "；首条 = 池" + pool[0].Id + " -> 英雄 " + pool[0].HeroId : ""));

            // 收藏品：列表来自配置表，拥有/装备来自玩家状态
            foreach (Pinball.Client.Domain.ClientCollectionKind kind in
                     (Pinball.Client.Domain.ClientCollectionKind[])System.Enum.GetValues(typeof(Pinball.Client.Domain.ClientCollectionKind)))
            {
                Debug.Log("[配置表] 收藏品 " + kind + "：表内可见 " +
                          ClientServices.Config.GetCollections(kind).Count +
                          " 个，已拥有 " + ClientServices.Data.GetOwnedCollectionIds(kind).Count +
                          " 个，装备 id = " + ClientServices.Data.GetEquippedCollectionId(kind));
            }

            // ---- 数值与说明模块（Stats）：属性 + 技能/天赋页签（验证"表 → 页签 → 说明文本"整条链）----
            System.Collections.Generic.IReadOnlyList<Pinball.Client.Stats.ClientAttributeValue> attributes =
                ClientServices.Stats.GetAttributes(first.HeroId);

            if (attributes.Count == 0)
            {
                Debug.Log("[配置表] Stats 属性（英雄 " + first.HeroId + "）：服务器未下发（战力即属此类）");
            }
            else
            {
                for (int index = 0; index < attributes.Count; index++)
                {
                    Debug.Log("[配置表] Stats 属性 id " + attributes[index].Id + " = " + attributes[index].Value);
                }
            }

            // ---- 说明文本格式化自检（含"每段各自带 %"的真实写法；等级 1 → 8%、等级 15 → 9%）----
            Pinball.Client.Stats.ClientDescriptionFormatter formatterSelfTest = new Pinball.Client.Stats.ClientDescriptionFormatter();
            string tierAtOne = formatterSelfTest.Format(1, "回复其8%/9%/10%生命值（冷却3回合）", new int[] { 1, 10, 20 }, 1);
            string tierAtFifteen = formatterSelfTest.Format(2, "回复其8%/9%/10%生命值（冷却3回合）", new int[] { 1, 10, 20 }, 15);
            string sharedSuffix = formatterSelfTest.Format(3, "造成60%/70%/80%攻击力的伤害", new int[] { 1, 10, 20 }, 20);
            string plainNumber = formatterSelfTest.Format(4, "冷却3回合，发射3枚追踪弹", new int[] { 1, 10, 20 }, 20);
            Debug.Log("[配置表] Stats 格式化自检：等级1=" + tierAtOne + " ｜ 等级15=" + tierAtFifteen +
                      " ｜ 末段共用后缀(等级20)=" + sharedSuffix + " ｜ 单个数字不动=" + plainNumber);

            Pinball.Client.Stats.ClientHeroAbilitySet abilitySet = ClientServices.Stats.GetHeroAbilities(first.HeroId);
            Debug.Log("[配置表] Stats 页签数 = " + abilitySet.Sections.Count +
                      "；普攻 = " + (abilitySet.BasicAttack != null ? abilitySet.BasicAttack.Name : "(无)"));

            for (int index = 0; index < abilitySet.Sections.Count; index++)
            {
                Pinball.Client.Stats.ClientAbilitySection section = abilitySet.Sections[index];
                string primaryName = section.Primary != null ? section.Primary.Name : "(空态)";
                string primaryDesc = section.Primary != null ? section.Primary.DisplayDescription : string.Empty;

                Debug.Log("[配置表] Stats 页签「" + section.DisplayName + "」主技能 = " + primaryName +
                          " / 等级 " + (section.Primary != null ? section.Primary.Level.ToString() : "-") +
                          " / 强化 " + section.Enhancements.Count + " 条" +
                          (primaryDesc.Length > 0 ? "｜" + primaryDesc : string.Empty));

                for (int e = 0; e < section.Enhancements.Count && e < 3; e++)
                {
                    Debug.Log("[配置表]    强化[" + e + "] " + section.Enhancements[e].Name +
                              " — " + section.Enhancements[e].DisplayDescription);
                }
            }
            // ---- 被动归属自检：13001 玉兔是唯一"天赋数据完整"的英雄（13 条被动）----
            // 验证弱关联规则：描述里点名了哪个主技能，就归哪个页签；都没点名 ⇒ 兜底归"天赋"页。
            Pinball.Client.Stats.ClientHeroAbilitySet richSet = ClientServices.Stats.GetHeroAbilities(13001);
            for (int index = 0; index < richSet.Sections.Count; index++)
            {
                Pinball.Client.Stats.ClientAbilitySection richSection = richSet.Sections[index];
                Debug.Log("[配置表] Stats 归属自检「" + richSection.DisplayName + "」主技能 = " +
                          (richSection.Primary != null ? richSection.Primary.Name : "(空态)") +
                          " / 强化 " + richSection.Enhancements.Count + " 条");

                for (int e = 0; e < richSection.Enhancements.Count; e++)
                {
                    Debug.Log("[配置表]    -> " + richSection.Enhancements[e].Name + "：" + richSection.Enhancements[e].DisplayDescription);
                }
            }
        }

        /// <summary>
        /// **`BUG-023` ② 的落地验证**：真正走一遍"领取邮件附件"的路径（这是原先硬编码 `ClientItemType.Material` 的地方），
        /// 检查入包道具的 `ItemType` 是否与配置表 `Item.Type` 的语义一致。
        ///
        /// 说明：本方法**会修改本地模拟状态**（batchmode 进程内的一次性验证，不影响工程资源）。
        /// </summary>
        [MenuItem("Client/配置表/验证道具类型映射（领取路径）", false, 62)]
        public static void VerifyItemTypeMapping()
        {
            ClientTableSource.Reset();
            ClientServices.InitializeForDevelopment();

            if (!ClientServices.IsInitialized || !ClientServices.HasConfig)
            {
                Debug.LogWarning("[验证] 客户端服务未就绪，无法验证道具类型映射。");
                return;
            }

            string failure;
            int claimed = ClientServices.Data.ClaimAllMails(out failure);
            Debug.Log("[验证] 一键领取邮件附件：" + claimed + " 封" +
                      (string.IsNullOrEmpty(failure) ? string.Empty : "（" + failure + "）"));

            System.Collections.Generic.IReadOnlyList<Pinball.Client.Domain.ClientInventoryItem> inventory =
                ClientServices.Data.GetInventory();
            bool allOk = true;
            for (int i = 0; i < inventory.Count; i++)
            {
                Pinball.Client.Domain.ClientItemConfig itemConfig;
                int tableType = ClientServices.Config.TryGetItem(inventory[i].ItemId, out itemConfig) && itemConfig != null
                    ? itemConfig.Type
                    : -1;
                Pinball.Client.Domain.ClientItemType expected =
                    tableType == 1 ? Pinball.Client.Domain.ClientItemType.Currency :
                    tableType == 3 ? Pinball.Client.Domain.ClientItemType.Consumable :
                    Pinball.Client.Domain.ClientItemType.Unknown;
                bool ok = inventory[i].ItemType == expected;
                allOk &= ok;
                Debug.Log("[验证] 入包道具 " + inventory[i].ItemId + "（表 Type=" + tableType + "）ItemType=" +
                          inventory[i].ItemType + " 期望=" + expected + (ok ? "  ✓" : "  ✗"));
            }

            Debug.Log(allOk
                ? "[验证] 结论：领取路径的道具类型与配置表**完全一致**（BUG-023 ② 已落地）。"
                : "[验证] 结论：**存在不一致**，需要复查 ResolveItemType 与配置表映射。");
        }

        /// <summary>
        /// **构建前全量基线**（`WEBGL_B1_BUILD.md` 步骤①）：在 <see cref="LogRowCounts"/> 的覆盖之外，
        /// 再把「运行环境 + 玩家状态层 + 各页面数据条数」打一遍，作为真机比对的基准。
        ///
        /// 用途：真机日志与基线**同构**才说明初始化跑通；缺哪一段就说明卡在哪一步
        /// （判读方法见 `WEBGL_B2_DIAGNOSTICS.md` §2）。
        /// </summary>
        [MenuItem("Client/配置表/打印构建前全量基线", false, 61)]
        public static void LogFullBaseline()
        {
            Debug.Log("[基线] ================= 客户端构建前全量基线 开始 =================");
            Debug.Log("[基线] Unity = " + Application.unityVersion +
                      " ｜ platform = " + Application.platform +
                      " ｜ persistentDataPath = " + Application.persistentDataPath);
            Debug.Log("[基线] 读表路径 = " + ClientTableSource.DescribeSource());

            // 复用既有覆盖：10 张表行数 / 英雄列表 / 技能 / 天赋 / 道具 / 随机池 / 收藏品 / Stats 属性·页签·格式化·归属。
            LogRowCounts();

            if (!ClientServices.IsInitialized || !ClientServices.HasConfig)
            {
                Debug.LogWarning("[基线] 客户端服务未就绪，玩家状态层与页面数据段跳过。");
                Debug.Log("[基线] ================= 客户端构建前全量基线 结束（不完整） =================");
                return;
            }

            // ---- 玩家状态层 ----
            Pinball.Client.Domain.ClientPlayerProfile profile = ClientServices.Data.GetProfile();
            if (profile != null)
            {
                Debug.Log("[基线] 玩家：名称=" + profile.DisplayName + " ｜ ID=" + profile.PlayerId +
                          " ｜ 战力=" + profile.CombatPower + "【战力待确认】");
            }
            Debug.Log("[基线] 主页展示英雄 id = " + ClientServices.Data.GetShowcaseHeroId());
            Debug.Log("[基线] 主力英雄 id = " + ClientServices.Data.GetFormationHeroId() +
                      " ｜ 编队队伍 index = " + ClientServices.Data.GetFormationTeamIndex() +
                      " ｜ 队伍数 = " + ClientServices.Data.GetFormationTeams().Count +
                      " ｜ 当前队伍战力 = " + ClientServices.Data.GetFormationCombatPower(ClientServices.Data.GetFormationTeamIndex()));
            Debug.Log("[基线] 装备弹珠 id = " + ClientServices.Data.GetEquippedMarbleId());

            // ---- 各页面数据条数（真机应与桌面一致）----
            Debug.Log("[基线] 背包 = " + ClientServices.Data.GetInventory().Count + " 项" +
                      " ｜ 商城商品 = " + ClientServices.Data.GetShopListings().Count + " 项" +
                      " ｜ 卡池历史 = " + ClientServices.Data.GetGachaHistory().Count + " 次");

            // 不变式自检（BUG-023 ② 的落地校验）：客户端 `ClientInventoryItem.ItemType` 必须与配置表 `Item.Type` 的语义一致。
            System.Collections.Generic.IReadOnlyList<Pinball.Client.Domain.ClientInventoryItem> inventory =
                ClientServices.Data.GetInventory();
            for (int i = 0; i < inventory.Count; i++)
            {
                Pinball.Client.Domain.ClientItemConfig itemConfig;
                int tableType = ClientServices.Config.TryGetItem(inventory[i].ItemId, out itemConfig) && itemConfig != null
                    ? itemConfig.Type
                    : -1;
                Pinball.Client.Domain.ClientItemType expected =
                    tableType == 1 ? Pinball.Client.Domain.ClientItemType.Currency :
                    tableType == 3 ? Pinball.Client.Domain.ClientItemType.Consumable :
                    Pinball.Client.Domain.ClientItemType.Unknown;
                bool ok = inventory[i].ItemType == expected;
                Debug.Log("[基线] 背包项 " + inventory[i].ItemId + "（表 Type=" + tableType + "）客户端 ItemType=" +
                          inventory[i].ItemType + " 期望=" + expected + (ok ? "  ✓一致" : "  ✗不一致"));
            }
            Debug.Log("[基线] 邮件 = " + ClientServices.Data.GetMails().Count + " 封" +
                      " ｜ 任务 = " + ClientServices.Data.GetTasks().Count +
                      " ｜ 公告 = " + ClientServices.Data.GetNotices().Count +
                      " ｜ 弹珠 = " + ClientServices.Data.GetMarbles().Count);

            Debug.Log("[基线] 排行榜：战力榜 = " + ClientServices.Data.GetLeaderboardEntries(
                          Pinball.Client.Domain.ClientLeaderboardKind.BattlePower).Count +
                      " 行 ｜ 挑战榜 = " + ClientServices.Data.GetLeaderboardEntries(
                          Pinball.Client.Domain.ClientLeaderboardKind.Challenge).Count + " 行");

            // 注意：活动任务只有 Newbie / Advanced 两类（赛季内容走 GetActivities()，不是任务）。
            Pinball.Client.Domain.ClientActivityTaskCategory[] categories =
            {
                Pinball.Client.Domain.ClientActivityTaskCategory.Newbie,
                Pinball.Client.Domain.ClientActivityTaskCategory.Advanced,
            };
            for (int i = 0; i < categories.Length; i++)
            {
                Debug.Log("[基线] 活动任务「" + categories[i] + "」= " +
                          ClientServices.Data.GetActivityTasks(categories[i]).Count + " 条");
            }
            Debug.Log("[基线] 赛季活动 = " + ClientServices.Data.GetActivities().Count + " 个");

            // ---- 收藏品：表内可见 / 已拥有 / 使用中（三层分开，便于对账）----
            Pinball.Client.Domain.ClientCollectionKind[] kinds =
            {
                Pinball.Client.Domain.ClientCollectionKind.Head,
                Pinball.Client.Domain.ClientCollectionKind.Badge,
                Pinball.Client.Domain.ClientCollectionKind.Nameplate,
                Pinball.Client.Domain.ClientCollectionKind.HeadFrame,
                Pinball.Client.Domain.ClientCollectionKind.Title,
            };
            for (int i = 0; i < kinds.Length; i++)
            {
                Debug.Log("[基线] 收藏品「" + kinds[i] + "」表内可见 = " +
                          ClientServices.Config.GetCollections(kinds[i]).Count +
                          " ｜ 已拥有 = " + ClientServices.Data.GetOwnedCollectionIds(kinds[i]).Count +
                          " ｜ 使用中 id = " + ClientServices.Data.GetEquippedCollectionId(kinds[i]));
            }

            Debug.Log("[基线] ================= 客户端构建前全量基线 结束 =================");
        }
    }
}
