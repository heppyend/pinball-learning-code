using System;
using System.Collections.Generic;
using Pinball.Client.Domain;
using UnityEngine;

namespace Pinball.Client.Services
{
    public sealed class LocalClientDataService : IClientDataService
    {
        private readonly ClientPlayerProfile _profile;
        private readonly ClientWallet _wallet;
        private readonly List<ClientHero> _heroes;
        private readonly List<ClientInventoryItem> _inventory;
        private readonly Dictionary<int, int[]> _heroAbilityLevels = new Dictionary<int, int[]>();
        private readonly ClientGachaPool _gachaPool;
        private readonly List<ClientGachaDrawResult> _gachaHistory = new List<ClientGachaDrawResult>();
        private readonly List<ClientShopListing> _shopListings;
        private readonly Dictionary<string, int> _shopPurchaseCounts = new Dictionary<string, int>();
        private readonly List<ClientMail> _mails;
        private readonly List<ClientLeaderboardEntry> _battlePowerLeaderboard;
        private readonly List<ClientLeaderboardEntry> _challengeLeaderboard;
        private readonly Dictionary<string, ClientPlayerLineup> _playerLineups;
        private readonly List<ClientActivity> _activities;
        private readonly Dictionary<ClientActivityTaskCategory, List<ClientActivityTaskData>> _activityTasks;
        private readonly ClientCompleteGuideEventData _completeGuideEvent;
        private readonly List<ClientTask> _tasks;
        private readonly List<ClientNotice> _notices;
        private readonly List<ClientMarble> _marbles;
        private readonly List<ClientFormationTeam> _formationTeams;
        private int _formationTeamIndex;
        private string _equippedMarbleId;
        private int _showcaseHeroId;
        private int _gachaCursor;

        public event Action DataChanged;

        public LocalClientDataService()
        {
            _profile = new ClientPlayerProfile
            {
                PlayerId = "local-player",
                DisplayName = "体验玩家",
                Level = 1,
                CombatPower = 100,
                EquippedTitleId = 0,
                EquippedAvatarId = 0,
            };

            _wallet = new ClientWallet();
            _wallet.SetBalance(ClientCurrencyType.Gold, 1000);
            _wallet.SetBalance(ClientCurrencyType.Diamond, 100);
            _wallet.SetBalance(ClientCurrencyType.Energy, 20);

            // 英雄图鉴 —— **表驱动**（负责人 2026-09-20 定调）：
            //   静态字段（名字 / 品质 / 元素 / 星级 / 图标名 / 立绘名）来自配置表 `IClientConfigService`；
            //   玩家状态（拥有 / 等级 / 战力）仍是本地模拟，等接服务端。
            // 配置 `Hero` 表 54 行、前端可见 49 行（按 `ClientShow` 过滤）。
            _heroes = BuildHeroesFromConfig();

            _showcaseHeroId = HeroIdAt(0);
            _profile.EquippedTitleId = 0;

            _inventory = new List<ClientInventoryItem>
            {
                new ClientInventoryItem { ItemId = 110004, ItemType = ClientItemType.Consumable, Quantity = 20, IsLocked = false },
                new ClientInventoryItem { ItemId = 120002, ItemType = ClientItemType.Consumable, Quantity = 8, IsLocked = false },
                new ClientInventoryItem { ItemId = 120003, ItemType = ClientItemType.Consumable, Quantity = 1, IsLocked = false },
            };
            foreach (ClientHero hero in _heroes)
                _heroAbilityLevels[hero.HeroId] = new[] { 1, 1, 1 };

            // 开发期卡池：仅作为可替换的本地模拟配置，不代表正式概率或运营内容。
            _gachaPool = new ClientGachaPool
            {
                PoolId = "local-standard",
                DisplayName = "新手体验卡池",
                CostCurrency = ClientCurrencyType.Diamond,
                SingleDrawCost = 10,
                TenDrawCost = 100,
                RewardHeroIds = new List<int>(),
            };
            // 角色卡牌目前只是商品占位。单价 10 水晶石和限购均为本地模拟值。
            _shopListings = new List<ClientShopListing>
            {
                new ClientShopListing { ListingId = "shop-card-001", DisplayName = "角色卡牌 1", UnitCrystalStoneCost = 10, PurchaseLimit = 5 },
                new ClientShopListing { ListingId = "shop-card-002", DisplayName = "角色卡牌 2", UnitCrystalStoneCost = 10, PurchaseLimit = 5 },
                new ClientShopListing { ListingId = "shop-card-003", DisplayName = "角色卡牌 3", UnitCrystalStoneCost = 10, PurchaseLimit = 5 },
            };
            _mails = new List<ClientMail>
            {
                new ClientMail { MailId = "local-mail-welcome", Title = "欢迎来到弹珠世界", Content = "这是一封本地模拟欢迎邮件。", AttachmentItemId = 110004, AttachmentQuantity = 8, IsRead = false, IsClaimed = false },
                new ClientMail { MailId = "local-mail-maintenance", Title = "开发期补偿", Content = "感谢参与客户端开发演示。", AttachmentItemId = 110004, AttachmentQuantity = 5, IsRead = false, IsClaimed = false },
            };
            // 2026-09-21（负责人反馈）：两个排行榜原先各只有 **5 条**模拟数据，界面自然只显示前 5 名。
            // 要求是"显示前 50 名" ⇒ 这里补足到 50 条（前 5 名保留原样例，其余按确定性规则生成）。
            _battlePowerLeaderboard = BuildLeaderboard(50, false);
            _challengeLeaderboard = BuildLeaderboard(50, true);
            _playerLineups = new Dictionary<string, ClientPlayerLineup>();
            foreach (ClientLeaderboardEntry entry in _battlePowerLeaderboard)
                _playerLineups[entry.PlayerId] = CreateLocalLineup(entry);
            _activities = new List<ClientActivity>
            {
                CreateSeasonalActivity(1, "全图鉴活动", "活动时间待确认", true, true),
                CreateSeasonalActivity(2, "成长挑战", "09.14 - 10.14", true, false),
                CreateSeasonalActivity(3, "限时签到", "09.16 - 09.30", false, true),
                CreateSeasonalActivity(4, "英雄试炼", "09.20 - 10.05", false, false),
                CreateSeasonalActivity(5, "材料加倍", "09.22 - 09.29", true, false),
                CreateSeasonalActivity(6, "赛季纪念", "09.14 - 11.01", false, true),
            };
            _activityTasks = new Dictionary<ClientActivityTaskCategory, List<ClientActivityTaskData>>
            {
                {
                    ClientActivityTaskCategory.Newbie,
                    new List<ClientActivityTaskData>
                    {
                        CreateActivityTask(ClientActivityTaskCategory.Newbie, 1, "完成一次登录", 1, 1, 3),
                        CreateActivityTask(ClientActivityTaskCategory.Newbie, 2, "查看三次英雄详情", 2, 3, 5),
                        CreateActivityTask(ClientActivityTaskCategory.Newbie, 3, "提升一次英雄能力", 0, 1, 8),
                        CreateActivityTask(ClientActivityTaskCategory.Newbie, 4, "获得四件养成材料", 1, 4, 10),
                        CreateActivityTask(ClientActivityTaskCategory.Newbie, 5, "完成六项新手目标", 2, 6, 15),
                    }
                },
                {
                    ClientActivityTaskCategory.Advanced,
                    new List<ClientActivityTaskData>
                    {
                        CreateActivityTask(ClientActivityTaskCategory.Advanced, 1, "进阶任务一", 0, 5, 10),
                        CreateActivityTask(ClientActivityTaskCategory.Advanced, 2, "进阶任务二", 0, 6, 12),
                        CreateActivityTask(ClientActivityTaskCategory.Advanced, 3, "进阶任务三", 0, 7, 14),
                        CreateActivityTask(ClientActivityTaskCategory.Advanced, 4, "进阶任务四", 0, 8, 16),
                        CreateActivityTask(ClientActivityTaskCategory.Advanced, 5, "进阶任务五", 0, 9, 20),
                    }
                },
            };
            // 仅用于客户端独立演示；正式活动期数、规则、进度来源和奖励内容均待产品/服务端契约确认。
            _completeGuideEvent = new ClientCompleteGuideEventData
            {
                EventId = "local-seasonal-1",
                CollectionText = "[全图鉴]卡！",
                RulesText = "本地模拟活动说明（正式规则待确认）",
                DateText = "活动时间待确认",
                Progress = 2,
                Target = 2,
                PendingLabelImageId = 8101,
                ClaimLabelImageId = 8102,
                ClaimableButtonImageId = 8103,
                Rewards = new List<ClientActivityTaskReward>
                {
                    new ClientActivityTaskReward
                    {
                        ItemId = 110004,
                        ItemImageId = 8201,
                        DisplayName = "养成材料",
                        Quantity = 10,
                    },
                },
            };
            _tasks = new List<ClientTask>
            {
                new ClientTask { TaskId = "local-task-login", Title = "每日登录", Description = "进入客户端一次。", Progress = 1, Target = 1, RewardItemId = 110004, RewardQuantity = 3, IsClaimed = false },
                new ClientTask { TaskId = "local-task-hero", Title = "查看英雄", Description = "进入英雄页面。", Progress = 0, Target = 1, RewardItemId = 110004, RewardQuantity = 5, IsClaimed = false },
            };
            _notices = new List<ClientNotice>
            {
                new ClientNotice { NoticeId = "local-notice-welcome", Title = "客户端开发公告", Content = "当前页面为本地模拟数据展示，后续将按服务端公告契约替换。", IsRead = false },
                new ClientNotice { NoticeId = "local-notice-assets", Title = "素材接入说明", Content = "无素材页面已保留可编辑布局节点，可直接替换为正式 UI 素材。", IsRead = false },
            };
            _marbles = new List<ClientMarble>
            {
                new ClientMarble { MarbleId = "marble-001", DisplayName = "新手弹珠", Description = "本地演示弹珠。", IsOwned = true },
                new ClientMarble { MarbleId = "marble-002", DisplayName = "星辉弹珠", Description = "尚未拥有的演示弹珠。", IsOwned = false },
            };
            // 编队：4 支队伍 × 3 个出战槽位。队伍数与槽位数**取自场景节点**
            // （`页面分栏-Image/Panel` 的 选中点1..4、`编队卡牌-Panel` 的 3 张槽位卡），
            // 见 `ClientFormationTeam` 注释；正式规则待产品/服务端契约确认。
            // 预置样本（本地演示/测试用）：第 1 队 3 个槽位填满 —— 打开编队页即可看到选卡上的
            // 1/2/3 队内编号；第 2 队放 1 个、其余留空，便于连续切 4 支队伍观察“编号随编队动态变化”。
            _formationTeams = new List<ClientFormationTeam>();
            for (int teamIndex = 0; teamIndex < FormationTeamCount; teamIndex++)
                _formationTeams.Add(new ClientFormationTeam(teamIndex));
            _formationTeams[0].SlotHeroIds[0] = HeroIdAt(0);
            _formationTeams[0].SlotHeroIds[1] = HeroIdAt(1);
            _formationTeams[0].SlotHeroIds[2] = HeroIdAt(2);
            _formationTeams[1].SlotHeroIds[0] = HeroIdAt(3);
            _formationTeamIndex = 0;
            _equippedMarbleId = "marble-001";
            SeedOwnedCollections();
        }

        // ------------------------------------------------------------------
        // 表驱动：英雄图鉴（静态来自配置表，玩家状态本地模拟）
        // ------------------------------------------------------------------

        /// <summary>
        /// 演示用：**除最后 N 个之外都视为"已拥有"**。
        /// 这样英雄子页（只列已拥有）有内容可看，图鉴子页同时还能看到"未拥有"的锁态。
        /// ⚠️ 这是**玩家状态**的本地模拟；真实"拥有哪些"应由服务端下发。
        /// </summary>
        private const int SimulatedLockedHeroCount = 3;

        /// <summary>
        /// 用配置表构建英雄列表 —— 「表驱动」的落点（负责人 2026-09-20）。
        /// 静态字段来自 `IClientConfigService.GetHeroes()`（已按 `ClientShow` 过滤、按 Id 升序）；
        /// 玩家状态（拥有 / 等级）本地模拟，等接服务端。
        /// 战力：配置 `Hero` 表**没有**战力字段 ⇒ 统一 0，见 `ClientHero.CombatPower` 的说明。
        /// </summary>
        private static List<ClientHero> BuildHeroesFromConfig()
        {
            List<ClientHero> heroes = new List<ClientHero>();

            // 用 HasConfig 而不是 IsInitialized：本方法在 `LocalClientDataService` 构造期间被调用，
            // 那时 `_data` 还没赋值 ⇒ IsInitialized 为 false（但配置表其实已经就绪）。
            IReadOnlyList<ClientHeroConfig> configs = ClientServices.HasConfig
                ? ClientServices.Config.GetHeroes()
                : null;

            if (configs == null || configs.Count == 0)
            {
                Debug.LogWarning("[配置表] 英雄配置为空：英雄页将没有数据。请检查 " +
                                 "Assets/Resources/Table/Hero.json，或磁盘上遗留的旧 config.pkg（BUG-021）。");
                return heroes;
            }

            for (int index = 0; index < configs.Count; index++)
            {
                ClientHeroConfig config = configs[index];
                heroes.Add(new ClientHero
                {
                    // ---- 静态：来自配置表 ----
                    HeroId = config.HeroId,
                    Name = config.Name,
                    ElementId = config.Element,
                    ElementName = ClientElements.Name(config.Element),
                    ElementIcon = config.ElementIcon,
                    Quality = config.Quality,
                    QualityIcon = config.QualityIcon,
                    CatapultType = config.CatapultType,
                    CatapultIcon = config.CatapultIcon,
                    StarLevel = config.Star,
                    MaxStar = config.MaxStar,
                    Avatar = config.Avatar,
                    Portrait1 = config.Portrait1,
                    Portrait2 = config.Portrait2,

                    // ---- 玩家状态：本地模拟（未接入服务端）----
                    Level = 1,
                    IsOwned = index < configs.Count - SimulatedLockedHeroCount,
                    CombatPower = 0,   // 【战力待确认】配置表无该字段，见 ClientHero.CombatPower 注释
                });
            }

            return heroes;
        }

        /// <summary>第 <paramref name="index"/> 个英雄的 Id；越界返回 0。</summary>
        private int HeroIdAt(int index)
        {
            return index >= 0 && index < _heroes.Count ? _heroes[index].HeroId : 0;
        }

        // ------------------------------------------------------------------
        // 收藏品（头像 / 头像框 / 徽章 / 铭牌 / 称号）
        //   "有哪些"  → 配置表 `IClientConfigService.GetCollections(kind)`
        //   "我拥有哪些 / 装备了哪个" → 本类（玩家状态，本地模拟）
        // ------------------------------------------------------------------

        /// <summary>演示用：每类收藏品把前 N 个"前端可见"条目视为已拥有。</summary>
        private const int SimulatedOwnedCollectionCount = 2;

        private readonly Dictionary<ClientCollectionKind, List<int>> _ownedCollections =
            new Dictionary<ClientCollectionKind, List<int>>();
        private readonly Dictionary<ClientCollectionKind, int> _equippedCollections =
            new Dictionary<ClientCollectionKind, int>();

        private void SeedOwnedCollections()
        {
            if (!ClientServices.HasConfig)   // 同上：构造期间 IsInitialized 尚为 false
                return;

            Array kinds = Enum.GetValues(typeof(ClientCollectionKind));
            foreach (ClientCollectionKind kind in kinds)
            {
                List<int> owned = new List<int>();
                IReadOnlyList<ClientCollectionConfig> entries = ClientServices.Config.GetCollections(kind);
                for (int index = 0; index < entries.Count && index < SimulatedOwnedCollectionCount; index++)
                    owned.Add(entries[index].Id);

                _ownedCollections[kind] = owned;
                _equippedCollections[kind] = owned.Count > 0 ? owned[0] : 0;
            }
        }

        public IReadOnlyList<int> GetOwnedCollectionIds(ClientCollectionKind kind)
        {
            List<int> owned;
            if (_ownedCollections.TryGetValue(kind, out owned))
                return owned;
            return new List<int>();
        }

        public int GetEquippedCollectionId(ClientCollectionKind kind)
        {
            int id;
            return _equippedCollections.TryGetValue(kind, out id) ? id : 0;
        }

        public bool TryEquipCollection(ClientCollectionKind kind, int id)
        {
            List<int> owned;
            if (!_ownedCollections.TryGetValue(kind, out owned) || !owned.Contains(id))
                return false;

            _equippedCollections[kind] = id;

            // 兼容字段：档案上的称号 / 头像（ProfilePage 在用）。
            if (kind == ClientCollectionKind.Title)
                _profile.EquippedTitleId = id;
            else if (kind == ClientCollectionKind.Head)
                _profile.EquippedAvatarId = id;

            DataChanged?.Invoke();
            return true;
        }
        public ClientPlayerProfile GetProfile() { return _profile; }
        public ClientWallet GetWallet() { return _wallet; }
        public IReadOnlyList<ClientHero> GetHeroes() { return _heroes; }
        public ClientHeroDetailData GetHeroDetail(int heroId)
        {
            ClientHero hero = _heroes.Find(candidate => candidate.HeroId == heroId);
            if (hero == null)
                return null;

            int numericHeroId = hero.HeroId;
            ClientHeroDetailData detail = new ClientHeroDetailData
            {
                HeroId = hero.HeroId,
                CardName = hero.Name,
                UpImageId = 1000 + numericHeroId,
            };
            detail.IllustrationIds.Add(2000 + numericHeroId);
            for (int index = 0; index < 5; index++)
                detail.VerticalImageIds.Add(3000 + index);
            detail.CardInfoImageIds.AddRange(new[] { 4001, 4002, 4003, 4004 });
            detail.LeftMiddleImageIds.AddRange(new[] { 5001, 5002, 5003 });
            detail.LeftMiddleTexts.AddRange(new[] { "Lv.", hero.Level.ToString(), hero.StarLevel.ToString() });
            detail.AttributeValues.AddRange(new[]
            {
                (100 + hero.Level * 10).ToString(),
                (500 + hero.Level * 30).ToString(),
                (40 + hero.Level * 2).ToString(),
                "5%", "150%", (10 + hero.Level).ToString(),
            });
            return detail;
        }

        public ClientHeroAbilityUpgradePreview GetHeroAbilityUpgradePreview(int heroId, ClientHeroAbilityType abilityType)
        {
            ClientHero hero = _heroes.Find(candidate => candidate.HeroId == heroId);
            if (hero == null)
                return null;

            int[] levels;
            if (!_heroAbilityLevels.TryGetValue(heroId, out levels))
            {
                levels = new[] { 1, 1, 1 };
                _heroAbilityLevels[heroId] = levels;
            }

            int abilityIndex = (int)abilityType;
            int currentLevel = levels[abilityIndex];
            int materialAmount = 2 + currentLevel + abilityIndex;
            int goldAmount = 50 * (currentLevel + abilityIndex + 1);
            ClientInventoryItem material = _inventory.Find(candidate => candidate.ItemId == 110004);
            ClientHeroAbilityUpgradePreview preview = new ClientHeroAbilityUpgradePreview
            {
                HeroId = heroId,
                AbilityType = abilityType,
                CurrentLevel = currentLevel,
                NextLevel = currentLevel + 1,
            };
            preview.Costs.Add(new ClientHeroUpgradeCost
            {
                ResourceType = ClientHeroUpgradeResourceType.InventoryItem,
                ResourceId = material != null ? material.ItemId : 110004,
                IconId = 6001 + abilityIndex,
                OwnedQuantity = material != null ? material.Quantity : 0,
                RequiredQuantity = materialAmount,
            });
            preview.Costs.Add(new ClientHeroUpgradeCost
            {
                ResourceType = ClientHeroUpgradeResourceType.Currency,
                ResourceId = 0,
                CurrencyType = ClientCurrencyType.Gold,
                IconId = 6101,
                OwnedQuantity = _wallet.GetBalance(ClientCurrencyType.Gold),
                RequiredQuantity = goldAmount,
            });
            return preview;
        }

        public bool TryUpgradeHeroAbility(int heroId, ClientHeroAbilityType abilityType, out string failureReason)
        {
            failureReason = string.Empty;
            ClientHero hero = _heroes.Find(candidate => candidate.HeroId == heroId);
            if (hero == null || !hero.IsOwned)
            {
                failureReason = "英雄未拥有";
                return false;
            }

            ClientHeroAbilityUpgradePreview preview = GetHeroAbilityUpgradePreview(heroId, abilityType);
            if (preview == null || !preview.CanUpgrade)
            {
                failureReason = "升级材料不足";
                return false;
            }

            foreach (ClientHeroUpgradeCost cost in preview.Costs)
            {
                if (cost.ResourceType == ClientHeroUpgradeResourceType.Currency)
                {
                    _wallet.SetBalance(cost.CurrencyType, _wallet.GetBalance(cost.CurrencyType) - cost.RequiredQuantity);
                    continue;
                }

                ClientInventoryItem item = _inventory.Find(candidate => candidate.ItemId == cost.ResourceId);
                if (item != null)
                    item.Quantity -= cost.RequiredQuantity;
            }

            _heroAbilityLevels[heroId][(int)abilityType]++;
            hero.CombatPower += 10;
            _profile.CombatPower += 10;
            DataChanged?.Invoke();
            return true;
        }
        public IReadOnlyList<ClientInventoryItem> GetInventory() { return _inventory; }
        public ClientGachaPool GetGachaPool() { return _gachaPool; }
        public IReadOnlyList<ClientGachaDrawResult> GetGachaHistory() { return _gachaHistory; }
        public IReadOnlyList<ClientShopListing> GetShopListings() { return _shopListings; }
        public IReadOnlyList<ClientMail> GetMails() { return _mails; }
        public IReadOnlyList<ClientLeaderboardEntry> GetLeaderboardEntries(ClientLeaderboardKind leaderboardKind)
        {
            return leaderboardKind == ClientLeaderboardKind.Challenge ? _challengeLeaderboard : _battlePowerLeaderboard;
        }

        public ClientPlayerLineup GetPlayerLineup(string playerId)
        {
            ClientPlayerLineup lineup;
            return !string.IsNullOrEmpty(playerId) && _playerLineups.TryGetValue(playerId, out lineup) ? lineup : null;
        }
        public IReadOnlyList<ClientActivity> GetActivities() { return _activities; }
        public IReadOnlyList<ClientActivityTaskData> GetActivityTasks(ClientActivityTaskCategory category)
        {
            List<ClientActivityTaskData> tasks;
            return _activityTasks.TryGetValue(category, out tasks) ? tasks : new List<ClientActivityTaskData>();
        }
        public ClientCompleteGuideEventData GetCompleteGuideEvent() { return _completeGuideEvent; }
        public IReadOnlyList<ClientTask> GetTasks() { return _tasks; }
        public IReadOnlyList<ClientNotice> GetNotices() { return _notices; }
        public IReadOnlyList<ClientMarble> GetMarbles() { return _marbles; }
        public int GetShowcaseHeroId() { return _showcaseHeroId; }
        public int GetEquippedBadgeId() { return GetEquippedCollectionId(ClientCollectionKind.Badge); }

        public bool TrySpend(ClientCurrencyType currency, int amount)
        {
            if (amount <= 0 || _wallet.GetBalance(currency) < amount)
                return false;

            _wallet.SetBalance(currency, _wallet.GetBalance(currency) - amount);
            DataChanged?.Invoke();
            return true;
        }

        public bool TryDrawGacha(string poolId, int drawCount, out ClientGachaDrawResult result)
        {
            result = null;
            if (poolId != _gachaPool.PoolId || (drawCount != 1 && drawCount != 10) || _gachaPool.RewardHeroIds.Count == 0)
                return false;

            int cost = drawCount == 1 ? _gachaPool.SingleDrawCost : _gachaPool.TenDrawCost;
            if (_wallet.GetBalance(_gachaPool.CostCurrency) < cost)
                return false;
            _wallet.SetBalance(_gachaPool.CostCurrency, _wallet.GetBalance(_gachaPool.CostCurrency) - cost);

            result = new ClientGachaDrawResult { PoolId = poolId, DrawCount = drawCount };
            for (int index = 0; index < drawCount; index++)
            {
                int heroId = _gachaPool.RewardHeroIds[_gachaCursor % _gachaPool.RewardHeroIds.Count];
                _gachaCursor++;
                result.RewardHeroIds.Add(heroId);
                ClientHero hero = _heroes.Find(candidate => candidate.HeroId == heroId);
                if (hero != null)
                    hero.IsOwned = true;
            }

            DataChanged?.Invoke();
            _gachaHistory.Add(result);
            return true;
        }

        public int GetShopPurchasedQuantity(string listingId)
        {
            int purchased;
            return _shopPurchaseCounts.TryGetValue(listingId, out purchased) ? purchased : 0;
        }

        public bool TryPurchaseShopListing(string listingId, int quantity, out string failureReason)
        {
            failureReason = string.Empty;
            ClientShopListing listing = _shopListings.Find(candidate => candidate.ListingId == listingId);
            if (listing == null)
            {
                failureReason = "商品不存在";
                return false;
            }
            if (quantity <= 0)
            {
                failureReason = "请选择购买数量";
                return false;
            }
            int purchased = GetShopPurchasedQuantity(listingId);
            if (purchased + quantity > listing.PurchaseLimit)
            {
                failureReason = "已达到本地限购次数";
                return false;
            }
            int totalCost = listing.UnitCrystalStoneCost * quantity;
            if (_wallet.GetBalance(ClientCurrencyType.Diamond) < totalCost)
            {
                failureReason = "水晶石不足";
                return false;
            }
            _wallet.SetBalance(ClientCurrencyType.Diamond, _wallet.GetBalance(ClientCurrencyType.Diamond) - totalCost);
            // 角色背包入库待角色/道具契约确认后在这里接入；当前只完成扣款与限购闭环。
            _shopPurchaseCounts[listingId] = purchased + quantity;
            DataChanged?.Invoke();
            return true;
        }

        public bool TryReadMail(string mailId)
        {
            ClientMail mail = _mails.Find(candidate => candidate.MailId == mailId);
            if (mail == null || mail.IsRead)
                return false;
            mail.IsRead = true;
            DataChanged?.Invoke();
            return true;
        }

        public bool TryClaimMail(string mailId, out string failureReason)
        {
            failureReason = string.Empty;
            ClientMail mail = _mails.Find(candidate => candidate.MailId == mailId);
            if (mail == null)
            {
                failureReason = "邮件不存在";
                return false;
            }
            if (mail.IsClaimed)
            {
                failureReason = "附件已领取";
                return false;
            }

            ClientInventoryItem item = _inventory.Find(candidate => candidate.ItemId == mail.AttachmentItemId);
            if (item == null)
            {
                item = new ClientInventoryItem { ItemId = mail.AttachmentItemId, ItemType = ResolveItemType(mail.AttachmentItemId), Quantity = 0, IsLocked = false };
                _inventory.Add(item);
            }
            item.Quantity += mail.AttachmentQuantity;
            mail.IsRead = true;
            mail.IsClaimed = true;
            DataChanged?.Invoke();
            return true;
        }

        public int ClaimAllMails(out string failureReason)
        {
            int claimed = 0;
            failureReason = string.Empty;
            foreach (ClientMail mail in _mails)
            {
                string reason;
                if (TryClaimMail(mail.MailId, out reason)) claimed++;
            }
            if (claimed == 0) failureReason = "没有可领取的邮件附件";
            return claimed;
        }

        /// <summary>
        /// **全部删除邮件**（2026-09-21 负责人确认的规则）：只有**全部邮件都已读、且附件都已领取**时才允许删除；
        /// 删除后邮件列表清空，邮箱页会显示"暂无邮件"。任一条件不满足则拒绝并给出原因。
        /// </summary>
        public bool TryDeleteAllMails(out string failureReason)
        {
            failureReason = string.Empty;

            if (_mails.Count == 0)
            {
                failureReason = "没有可删除的邮件";
                return false;
            }

            for (int index = 0; index < _mails.Count; index++)
            {
                ClientMail mail = _mails[index];
                if (!mail.IsRead)
                {
                    failureReason = "还有未读邮件，请先全部已读";
                    return false;
                }
                if (!mail.IsClaimed)
                {
                    failureReason = "还有附件未领取，请先全部领取";
                    return false;
                }
            }

            _mails.Clear();
            DataChanged?.Invoke();
            return true;
        }

        public bool TryClaimActivity(string activityId, out string failureReason)
        {
            failureReason = string.Empty;
            ClientActivity activity = _activities.Find(candidate => candidate.ActivityId == activityId);
            if (activity == null || !activity.IsOpen)
            {
                failureReason = "活动未开启";
                return false;
            }
            if (activity.IsClaimed)
            {
                failureReason = "奖励已领取";
                return false;
            }
            if (activity.Progress < activity.Target)
            {
                failureReason = "进度未完成";
                return false;
            }

            ClientInventoryItem item = _inventory.Find(candidate => candidate.ItemId == activity.RewardItemId);
            if (item == null)
            {
                item = new ClientInventoryItem { ItemId = activity.RewardItemId, ItemType = ResolveItemType(activity.RewardItemId), Quantity = 0, IsLocked = false };
                _inventory.Add(item);
            }
            item.Quantity += activity.RewardQuantity;
            activity.IsClaimed = true;
            DataChanged?.Invoke();
            return true;
        }

        public bool TryClaimActivityTask(ClientActivityTaskCategory category, string taskId, out ClientActivityTaskClaimResult result)
        {
            result = new ClientActivityTaskClaimResult
            {
                Status = ClientRewardDeliveryStatus.Rejected,
                Channel = ClientRewardDeliveryChannel.Unknown,
                Message = string.Empty,
            };

            List<ClientActivityTaskData> tasks;
            if (!_activityTasks.TryGetValue(category, out tasks))
            {
                result.Message = "任务分类不存在";
                return false;
            }

            ClientActivityTaskData task = tasks.Find(candidate =>
                candidate != null && candidate.Category == category && candidate.TaskId == taskId);
            if (task == null)
            {
                result.Message = "任务不存在";
                return false;
            }
            if (task.IsClaimed)
            {
                result.Message = "奖励已领取";
                return false;
            }
            if (task.IsClaimPending)
            {
                result.Status = ClientRewardDeliveryStatus.Pending;
                result.Message = "奖励正在发放";
                return true;
            }
            if (!task.IsComplete)
            {
                result.Message = "任务未完成";
                return false;
            }

            foreach (ClientActivityTaskReward reward in task.Rewards)
            {
                if (reward == null || reward.Quantity <= 0 || reward.ItemId <= 0)
                    continue;
                ClientInventoryItem item = _inventory.Find(candidate => candidate.ItemId == reward.ItemId);
                if (item == null)
                {
                    item = new ClientInventoryItem
                    {
                        ItemId = reward.ItemId,
                        ItemType = ResolveItemType(reward.ItemId),
                        Quantity = 0,
                        IsLocked = false,
                    };
                    _inventory.Add(item);
                }
                item.Quantity += reward.Quantity;
            }

            task.IsClaimed = true;
            task.IsClaimPending = false;
            result.Status = ClientRewardDeliveryStatus.Granted;
            result.Channel = ClientRewardDeliveryChannel.Inventory;
            result.Message = "奖励已发放到背包";
            DataChanged?.Invoke();
            return true;
        }

        public bool TryClaimCompleteGuideEvent(string eventId, out ClientActivityTaskClaimResult result)
        {
            result = new ClientActivityTaskClaimResult
            {
                Status = ClientRewardDeliveryStatus.Rejected,
                Channel = ClientRewardDeliveryChannel.Unknown,
                Message = string.Empty,
            };
            if (_completeGuideEvent == null || _completeGuideEvent.EventId != eventId)
            {
                result.Message = "活动不存在";
                return false;
            }
            if (_completeGuideEvent.IsClaimed)
            {
                result.Message = "奖励已领取";
                return false;
            }
            if (_completeGuideEvent.IsClaimPending)
            {
                result.Status = ClientRewardDeliveryStatus.Pending;
                result.Message = "奖励正在发放";
                return true;
            }
            if (!_completeGuideEvent.IsComplete)
            {
                result.Message = "活动进度未完成";
                return false;
            }

            foreach (ClientActivityTaskReward reward in _completeGuideEvent.Rewards)
            {
                if (reward == null || reward.Quantity <= 0 || reward.ItemId <= 0)
                    continue;
                ClientInventoryItem item = _inventory.Find(candidate => candidate.ItemId == reward.ItemId);
                if (item == null)
                {
                    item = new ClientInventoryItem
                    {
                        ItemId = reward.ItemId,
                        ItemType = ResolveItemType(reward.ItemId),
                        Quantity = 0,
                        IsLocked = false,
                    };
                    _inventory.Add(item);
                }
                item.Quantity += reward.Quantity;
            }

            _completeGuideEvent.IsClaimed = true;
            _completeGuideEvent.IsClaimPending = false;
            result.Status = ClientRewardDeliveryStatus.Granted;
            result.Channel = ClientRewardDeliveryChannel.Inventory;
            result.Message = "奖励已发放到背包";
            DataChanged?.Invoke();
            return true;
        }

        public bool TryClaimTask(string taskId, out string failureReason)
        {
            failureReason = string.Empty;
            ClientTask task = _tasks.Find(candidate => candidate.TaskId == taskId);
            if (task == null)
            {
                failureReason = "任务不存在";
                return false;
            }
            if (task.IsClaimed)
            {
                failureReason = "奖励已领取";
                return false;
            }
            if (task.Progress < task.Target)
            {
                failureReason = "任务未完成";
                return false;
            }
            ClientInventoryItem item = _inventory.Find(candidate => candidate.ItemId == task.RewardItemId);
            if (item == null)
            {
                item = new ClientInventoryItem { ItemId = task.RewardItemId, ItemType = ResolveItemType(task.RewardItemId), Quantity = 0, IsLocked = false };
                _inventory.Add(item);
            }
            item.Quantity += task.RewardQuantity;
            task.IsClaimed = true;
            DataChanged?.Invoke();
            return true;
        }

        public bool TryReadNotice(string noticeId)
        {
            ClientNotice notice = _notices.Find(candidate => candidate.NoticeId == noticeId);
            if (notice == null || notice.IsRead)
                return false;
            notice.IsRead = true;
            DataChanged?.Invoke();
            return true;
        }

        public bool TryUpgradeHero(int heroId, out string failureReason)
        {
            failureReason = string.Empty;
            ClientHero hero = _heroes.Find(candidate => candidate.HeroId == heroId);
            if (hero == null || !hero.IsOwned) { failureReason = "英雄未拥有"; return false; }
            if (hero.Level >= 10) { failureReason = "已达到本地等级上限"; return false; }
            ClientInventoryItem material = _inventory.Find(candidate => candidate.ItemId == 110004);
            if (material == null || material.Quantity < 5) { failureReason = "养成材料不足"; return false; }
            if (_wallet.GetBalance(ClientCurrencyType.Gold) < 50) { failureReason = "金币不足"; return false; }
            material.Quantity -= 5;
            _wallet.SetBalance(ClientCurrencyType.Gold, _wallet.GetBalance(ClientCurrencyType.Gold) - 50);
            hero.Level++;
            hero.CombatPower += 20;
            _profile.CombatPower += 20;
            DataChanged?.Invoke();
            return true;
        }

        public bool TrySetInventoryItemLocked(int itemId, bool isLocked)
        {
            ClientInventoryItem item = _inventory.Find(candidate => candidate.ItemId == itemId);
            if (item == null) return false;
            item.IsLocked = isLocked;
            DataChanged?.Invoke();
            return true;
        }

        public bool TryStarUpHero(int heroId, out string failureReason)
        {
            failureReason = string.Empty;
            ClientHero hero = _heroes.Find(candidate => candidate.HeroId == heroId);
            if (hero == null || !hero.IsOwned) { failureReason = "英雄未拥有"; return false; }
            if (hero.StarLevel >= 5) { failureReason = "已达到本地星级上限"; return false; }
            ClientInventoryItem material = _inventory.Find(candidate => candidate.ItemId == 110004);
            if (material == null || material.Quantity < 10) { failureReason = "养成材料不足"; return false; }
            if (_wallet.GetBalance(ClientCurrencyType.Gold) < 100) { failureReason = "金币不足"; return false; }
            material.Quantity -= 10;
            _wallet.SetBalance(ClientCurrencyType.Gold, _wallet.GetBalance(ClientCurrencyType.Gold) - 100);
            hero.StarLevel++;
            hero.CombatPower += 50;
            _profile.CombatPower += 50;
            DataChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 装备判定点。**当前恒为 false** —— 配置表 `Item.Type` **只有 `1`（货币）与 `3`（消耗品），没有任何"装备"类型**
        /// ⇒ 装备功能目前**没有数据支撑**。
        ///
        /// <para>2026-09-20（`BUG-023` ②）：该判定原先写的是 `item.ItemType != ClientItemType.Equipment`，
        /// 而枚举里的 `Equipment = 2` 在表内**一行都没有** ⇒ 这个方法**永远失败**。
        /// 为**不擅自引入未确认的产品规则**，这里保持"一律失败"的原行为，只是把原因说清楚；
        /// 等负责人确认装备的数据来源（配置表？服务端？）后再改为真实判定。</para>
        /// </summary>
        private static bool IsEquippableItem(ClientInventoryItem item)
        {
            return false;
        }

        /// <summary>
        /// 由**配置表**决定道具类型（`Item.Type`：`1` = 货币、`3` = 消耗品，`2` 表内未出现）。
        ///
        /// <para>为什么不在各处硬编码：奖励 / 附件 / 入包道具的类型完全由表决定，硬编码会随表变化而变错 ——
        /// 历史上这里曾把**货币与消耗品都写成 `Material`**（`BUG-023` ②）。表内没有的 id ⇒ <see cref="ClientItemType.Unknown"/>。</para>
        /// </summary>
        private static ClientItemType ResolveItemType(int itemId)
        {
            ClientItemConfig config;
            if (ClientServices.Config != null &&
                ClientServices.Config.TryGetItem(itemId, out config) &&
                config != null)
            {
                switch (config.Type)
                {
                    case 1: return ClientItemType.Currency;
                    case 3: return ClientItemType.Consumable;
                }
            }

            return ClientItemType.Unknown;
        }

        public bool TrySetEquipmentEquipped(int itemId, bool equipped, out string failureReason)
        {
            failureReason = string.Empty;
            ClientInventoryItem item = _inventory.Find(candidate => candidate.ItemId == itemId);
            if (item == null || !IsEquippableItem(item))
            {
                failureReason = item == null ? "请选择装备" : "装备功能尚未接入（配置表暂无装备类型）";
                return false;
            }
            if (item.IsLocked)
            {
                failureReason = "装备已锁定";
                return false;
            }
            item.EquippedHeroId = equipped ? HeroIdAt(0) : 0;
            DataChanged?.Invoke();
            return true;
        }

        // ------------------------------------------------------------------
        // 编队：4 支队伍 × 3 个出战槽位（本地模拟，可替换）
        // 单槽方法保留为兼容写法 = "当前队伍的槽位 0"。
        // ------------------------------------------------------------------

        /// <summary>队伍数量，与场景 `页面分栏-Image/Panel` 的 选中点1..4 一致。</summary>
        private const int FormationTeamCount = 4;

        /// <summary>兼容写法：等价于「当前队伍槽位 0 的英雄」。</summary>
        public int GetFormationHeroId() { return GetFormationSlotHeroId(_formationTeamIndex, 0); }

        /// <summary>兼容写法：等价于「写入当前队伍槽位 0」。</summary>
        public bool TrySetFormationHero(int heroId) { return TrySetFormationSlot(0, heroId); }

        public IReadOnlyList<ClientFormationTeam> GetFormationTeams() { return _formationTeams; }

        public int GetFormationTeamIndex() { return _formationTeamIndex; }

        public bool TrySwitchFormationTeam(int teamIndex)
        {
            if (_formationTeams == null || teamIndex < 0 || teamIndex >= _formationTeams.Count)
                return false;
            if (_formationTeamIndex == teamIndex)
                return true;
            _formationTeamIndex = teamIndex;
            DataChanged?.Invoke();
            return true;
        }

        public bool TrySetFormationSlot(int slotIndex, int heroId)
        {
            ClientFormationTeam team = GetFormationTeam(_formationTeamIndex);
            if (team == null || slotIndex < 0 || slotIndex >= ClientFormationTeam.SlotCount)
                return false;

            // 空串 = 清空该槽位（不校验英雄）。
            if (heroId <= 0)
            {
                team.SlotHeroIds[slotIndex] = 0;
                DataChanged?.Invoke();
                return true;
            }

            ClientHero hero = FindHero(heroId);
            if (hero == null || !hero.IsOwned)
                return false;

            // 原则（负责人 2026-09-20）：**同一队伍内不能出现重复英雄**。
            // 该英雄已在同队别的槽位 ⇒ 拒绝（返回 false，由页面给出提示），不改任何状态。
            for (int index = 0; index < team.SlotHeroIds.Count; index++)
            {
                if (index != slotIndex && team.SlotHeroIds[index] == heroId)
                    return false;
            }

            team.SlotHeroIds[slotIndex] = heroId;
            DataChanged?.Invoke();
            return true;
        }

        public int GetFormationCombatPower(int teamIndex)
        {
            ClientFormationTeam team = GetFormationTeam(teamIndex);
            if (team == null)
                return 0;
            int total = 0;
            for (int index = 0; index < team.SlotHeroIds.Count; index++)
            {
                ClientHero hero = FindHero(team.SlotHeroIds[index]);
                if (hero != null)
                    total += hero.CombatPower;
            }
            return total;
        }

        /// <summary>
        /// 编队点选规则（负责人 2026-09-20）：已在队 ⇒ 移除且**不补位**；不在队 ⇒ 补**编号最小的空位**；
        /// 三个编号占满 ⇒ 不加入。详见接口注释。
        /// </summary>
        public bool TryToggleFormationHero(int heroId, out int slotNumber, out string failureReason)
        {
            slotNumber = 0;
            failureReason = string.Empty;

            ClientFormationTeam team = GetFormationTeam(_formationTeamIndex);
            if (team == null || heroId <= 0)
            {
                failureReason = "编队数据未就绪";
                return false;
            }

            // ① 已在当前队伍 ⇒ 移除。**不补位**：其余槽位保持原编号，空出的编号留给下一个加入者。
            for (int index = 0; index < team.SlotHeroIds.Count; index++)
            {
                if (team.SlotHeroIds[index] != heroId)
                    continue;

                team.SlotHeroIds[index] = 0;
                DataChanged?.Invoke();
                return true;   // slotNumber = 0 ⇒ 调用方据此提示“已移出”
            }

            ClientHero hero = FindHero(heroId);
            if (hero == null || !hero.IsOwned)
            {
                failureReason = "该英雄尚未拥有";
                return false;
            }

            // ② 自动补到**编号最小的空位**。同时空出 2 个及以上编号时，必须补编号较小的那个
            //    （例如 1 号与 3 号都空着，下一个加入者应拿到 1 号）—— 故从下标 0 起顺序找。
            for (int index = 0; index < team.SlotHeroIds.Count; index++)
            {
                if (team.SlotHeroIds[index] > 0)
                    continue;

                team.SlotHeroIds[index] = heroId;
                slotNumber = index + 1;
                DataChanged?.Invoke();
                return true;
            }

            // ③ 三个编号都占满 ⇒ 不加入（要换人得先点已编号的卡把它踢出来）。
            failureReason = "编队已满（" + ClientFormationTeam.SlotCount + " 个编号都已占用），请先移出一位";
            return false;
        }

        private ClientFormationTeam GetFormationTeam(int teamIndex)
        {
            if (_formationTeams == null || teamIndex < 0 || teamIndex >= _formationTeams.Count)
                return null;
            return _formationTeams[teamIndex];
        }

        private int GetFormationSlotHeroId(int teamIndex, int slotIndex)
        {
            ClientFormationTeam team = GetFormationTeam(teamIndex);
            if (team == null || slotIndex < 0 || slotIndex >= team.SlotHeroIds.Count)
                return 0;
            return team.SlotHeroIds[slotIndex];
        }

        private ClientHero FindHero(int heroId)
        {
            if (heroId <= 0)
                return null;
            return _heroes.Find(candidate => candidate.HeroId == heroId);
        }
        public string GetEquippedMarbleId() { return _equippedMarbleId; }
        public bool TrySetEquippedMarble(string marbleId)
        {
            ClientMarble marble = _marbles.Find(candidate => candidate.MarbleId == marbleId);
            if (marble == null || !marble.IsOwned) return false;
            _equippedMarbleId = marbleId;
            DataChanged?.Invoke();
            return true;
        }

        public bool TryRename(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return false;

            _profile.DisplayName = displayName.Trim();
            DataChanged?.Invoke();
            return true;
        }

        public bool TrySetShowcaseHero(int heroId)
        {
            ClientHero hero = _heroes.Find(candidate => candidate.HeroId == heroId);
            if (hero == null || !hero.IsOwned)
                return false;

            _showcaseHeroId = hero.HeroId;
            DataChanged?.Invoke();
            return true;
        }

        public bool TrySetEquippedBadge(int badgeId)
        {
            // 兼容写法：徽章的"拥有 / 装备"统一走收藏品接口
            // （"有哪些"来自配置表 `TBadge`，"我拥有哪些"是玩家状态）—— 见 MODULE.md §15。
            return TryEquipCollection(ClientCollectionKind.Badge, badgeId);
        }

        /// <summary>
        /// 生成排行榜数据（2026-09-21：负责人要求两个榜都显示**前 50 名**，原先各只有 5 条）。
        ///
        /// <para>前 5 名保留原先手写的样例（便于与既有观察对照）；其余按**确定性规则**补足到 50 条 ——
        /// **不使用随机数**，保证每次运行、每台机器的结果都一致，便于验收与自动化自检。</para>
        /// </summary>
        private static List<ClientLeaderboardEntry> BuildLeaderboard(int count, bool challengeOrder)
        {
            string[] names = { "弹珠大师", "星辉旅人", "火焰骑士", "冰原回响", "晨光使者" };
            string[] titles = { "登顶者", "星辰领航员", "烈焰先锋", "霜冻守望", "曙光信使" };
            int[] powers = { 5200, 4300, 3800, 3500, 3200 };
            int[] order = challengeOrder ? new[] { 0, 2, 1, 4, 3 } : new[] { 0, 1, 2, 3, 4 };

            List<ClientLeaderboardEntry> list = new List<ClientLeaderboardEntry>(count);
            int rank = 1;
            for (int i = 0; i < order.Length; i++)
            {
                int k = order[i];
                list.Add(CreateLeaderboardEntry(rank++, "local-rank-" + (k + 1).ToString("D3"),
                    names[k], titles[k], powers[k], 120 - i * 6, 18 + i * 3));
            }

            while (list.Count < count)
            {
                int index = list.Count;                       // 0 基名次索引
                int power = Mathf.Max(600, 3000 - (index - 4) * 45);
                list.Add(CreateLeaderboardEntry(rank++, "local-rank-" + (index + 1).ToString("D3"),
                    "挑战者" + index.ToString("D2"), titles[index % titles.Length],
                    power, Mathf.Max(30, 96 - (index - 4) * 2), 30 + (index - 4) * 2));
            }

            return list;
        }

        private static ClientLeaderboardEntry CreateLeaderboardEntry(int rank, string playerId, string displayName, string title, int combatPower, int floor, int turnCount)
        {            return new ClientLeaderboardEntry
            {
                Rank = rank, PlayerId = playerId, DisplayName = displayName, Title = title,
                CombatPower = combatPower, Floor = floor, TurnCount = turnCount,
                AvatarId = 0, AvatarFrameId = 0, BadgeId = 0,
            };
        }

        private static ClientActivity CreateSeasonalActivity(int index, string title, string dateText, bool showHotSpot, bool showPopular)
        {
            return new ClientActivity
            {
                ActivityId = "local-seasonal-" + index,
                Title = title,
                Description = "本地活动说明 " + index,
                DateText = dateText,
                BannerImageId = 7100 + index,
                HotSpotImageId = 7200 + index,
                PopularImageId = 7300 + index,
                ShowHotSpot = showHotSpot,
                ShowPopular = showPopular,
                IsOpen = true,
            };
        }

        private static ClientActivityTaskData CreateActivityTask(
            ClientActivityTaskCategory category, int index, string description, int progress, int target, int rewardQuantity)
        {
            ClientActivityTaskData task = new ClientActivityTaskData
            {
                TaskId = (category == ClientActivityTaskCategory.Newbie ? "newbie-" : "advanced-") + index,
                Category = category,
                Description = description,
                Progress = progress,
                Target = target,
                PendingLabelImageId = 8101,
                ClaimLabelImageId = 8102,
                ClaimableButtonImageId = 8103,
            };
            task.Rewards.Add(new ClientActivityTaskReward
            {
                ItemId = 110004,
                ItemImageId = 8201,
                DisplayName = "养成材料",
                Quantity = rewardQuantity,
            });
            return task;
        }

        private static int ParseTrailingNumber(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;
            int separator = value.LastIndexOf('-');
            int parsed;
            return separator >= 0 && int.TryParse(value.Substring(separator + 1), out parsed) ? parsed : 0;
        }

        private static ClientPlayerLineup CreateLocalLineup(ClientLeaderboardEntry entry)
        {
            ClientPlayerLineup lineup = new ClientPlayerLineup
            {
                PlayerId = entry.PlayerId, CombatPower = entry.CombatPower,
                Floor = entry.Floor, TurnCount = entry.TurnCount,
            };
            for (int index = 0; index < 6; index++)
            {
                lineup.Cards.Add(new ClientLineupCard
                {
                    CardId = entry.PlayerId + "-card-" + (index + 1),
                    DisplayName = "阵容卡牌 " + (index + 1), Level = 60 - index * 5,
                    IllustrationId = "illustration-placeholder", AttributeIconId = "attribute-placeholder",
                    QualityId = "quality-placeholder", StarCount = 5 - index % 3,
                });
            }
            return lineup;
        }
    }
}
