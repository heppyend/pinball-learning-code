using System;
using System.Collections.Generic;
using Pinball.Client.Domain;

namespace Pinball.Client.Services
{
    public sealed class LocalClientDataService : IClientDataService
    {
        private readonly ClientPlayerProfile _profile;
        private readonly ClientWallet _wallet;
        private readonly List<ClientHero> _heroes;
        private readonly List<ClientInventoryItem> _inventory;
        private readonly ClientGachaPool _gachaPool;
        private readonly List<ClientGachaDrawResult> _gachaHistory = new List<ClientGachaDrawResult>();
        private readonly List<ClientShopListing> _shopListings;
        private readonly Dictionary<string, int> _shopPurchaseCounts = new Dictionary<string, int>();
        private readonly List<ClientMail> _mails;
        private readonly List<ClientRankEntry> _rankEntries;
        private readonly List<ClientActivity> _activities;
        private readonly List<ClientTask> _tasks;
        private readonly List<ClientNotice> _notices;
        private readonly List<ClientMarble> _marbles;
        private string _formationHeroId;
        private string _equippedMarbleId;
        private string _showcaseHeroId;
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
                EquippedTitleId = string.Empty,
                EquippedAvatarId = "default-avatar",
            };

            _wallet = new ClientWallet();
            _wallet.SetBalance(ClientCurrencyType.Gold, 1000);
            _wallet.SetBalance(ClientCurrencyType.Diamond, 100);
            _wallet.SetBalance(ClientCurrencyType.Energy, 20);

            _heroes = new List<ClientHero>
            {
                new ClientHero { HeroId = "hero-001", Name = "演示英雄", Element = "Fire", Level = 1, StarLevel = 1, CombatPower = 100, IsOwned = true },
                new ClientHero { HeroId = "hero-002", Name = "未拥有英雄", Element = "Water", Level = 1, StarLevel = 1, CombatPower = 80, IsOwned = false },
            };
            _showcaseHeroId = "hero-001";
            _profile.EquippedTitleId = "badge-001";

            _inventory = new List<ClientInventoryItem>
            {
                new ClientInventoryItem { ItemId = "material-001", ItemType = ClientItemType.Material, Quantity = 20, IsLocked = false },
                new ClientInventoryItem { ItemId = "equipment-001", ItemType = ClientItemType.Equipment, Quantity = 1, IsLocked = false },
            };

            // 开发期卡池：仅作为可替换的本地模拟配置，不代表正式概率或运营内容。
            _gachaPool = new ClientGachaPool
            {
                PoolId = "local-standard",
                DisplayName = "新手体验卡池",
                CostCurrency = ClientCurrencyType.Diamond,
                SingleDrawCost = 10,
                TenDrawCost = 100,
                RewardHeroIds = new List<string> { "hero-001", "hero-002" },
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
                new ClientMail { MailId = "local-mail-welcome", Title = "欢迎来到弹珠世界", Content = "这是一封本地模拟欢迎邮件。", AttachmentItemId = "material-001", AttachmentQuantity = 8, IsRead = false, IsClaimed = false },
                new ClientMail { MailId = "local-mail-maintenance", Title = "开发期补偿", Content = "感谢参与客户端开发演示。", AttachmentItemId = "material-001", AttachmentQuantity = 5, IsRead = false, IsClaimed = false },
            };
            _rankEntries = new List<ClientRankEntry>
            {
                new ClientRankEntry { Rank = 1, PlayerId = "local-rank-001", DisplayName = "弹珠大师", CombatPower = 5200, IsCurrentPlayer = false },
                new ClientRankEntry { Rank = 2, PlayerId = "local-rank-002", DisplayName = "星辉旅人", CombatPower = 4300, IsCurrentPlayer = false },
                new ClientRankEntry { Rank = 3, PlayerId = "local-rank-003", DisplayName = "火焰骑士", CombatPower = 3800, IsCurrentPlayer = false },
                new ClientRankEntry { Rank = 58, PlayerId = "local-player", DisplayName = _profile.DisplayName, CombatPower = _profile.CombatPower, IsCurrentPlayer = true },
            };
            _activities = new List<ClientActivity>
            {
                new ClientActivity { ActivityId = "local-activity-login", Title = "新手七日礼", Description = "完成体验任务，领取养成材料。", Progress = 1, Target = 1, RewardItemId = "material-001", RewardQuantity = 10, IsOpen = true, IsClaimed = false },
                new ClientActivity { ActivityId = "local-activity-growth", Title = "成长挑战", Description = "提升英雄等级后可领取。", Progress = 0, Target = 1, RewardItemId = "material-001", RewardQuantity = 20, IsOpen = true, IsClaimed = false },
            };
            _tasks = new List<ClientTask>
            {
                new ClientTask { TaskId = "local-task-login", Title = "每日登录", Description = "进入客户端一次。", Progress = 1, Target = 1, RewardItemId = "material-001", RewardQuantity = 3, IsClaimed = false },
                new ClientTask { TaskId = "local-task-hero", Title = "查看英雄", Description = "进入英雄页面。", Progress = 0, Target = 1, RewardItemId = "material-001", RewardQuantity = 5, IsClaimed = false },
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
            _formationHeroId = "hero-001";
            _equippedMarbleId = "marble-001";
        }

        public ClientPlayerProfile GetProfile() { return _profile; }
        public ClientWallet GetWallet() { return _wallet; }
        public IReadOnlyList<ClientHero> GetHeroes() { return _heroes; }
        public IReadOnlyList<ClientInventoryItem> GetInventory() { return _inventory; }
        public ClientGachaPool GetGachaPool() { return _gachaPool; }
        public IReadOnlyList<ClientGachaDrawResult> GetGachaHistory() { return _gachaHistory; }
        public IReadOnlyList<ClientShopListing> GetShopListings() { return _shopListings; }
        public IReadOnlyList<ClientMail> GetMails() { return _mails; }
        public IReadOnlyList<ClientRankEntry> GetRankEntries()
        {
            ClientRankEntry current = _rankEntries.Find(candidate => candidate.IsCurrentPlayer);
            if (current != null)
            {
                current.DisplayName = _profile.DisplayName;
                current.CombatPower = _profile.CombatPower;
            }
            return _rankEntries;
        }
        public IReadOnlyList<ClientActivity> GetActivities() { return _activities; }
        public IReadOnlyList<ClientTask> GetTasks() { return _tasks; }
        public IReadOnlyList<ClientNotice> GetNotices() { return _notices; }
        public IReadOnlyList<ClientMarble> GetMarbles() { return _marbles; }
        public string GetShowcaseHeroId() { return _showcaseHeroId; }
        public string GetEquippedBadgeId() { return _profile.EquippedTitleId; }

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
                string heroId = _gachaPool.RewardHeroIds[_gachaCursor % _gachaPool.RewardHeroIds.Count];
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
                item = new ClientInventoryItem { ItemId = mail.AttachmentItemId, ItemType = ClientItemType.Material, Quantity = 0, IsLocked = false };
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
                item = new ClientInventoryItem { ItemId = activity.RewardItemId, ItemType = ClientItemType.Material, Quantity = 0, IsLocked = false };
                _inventory.Add(item);
            }
            item.Quantity += activity.RewardQuantity;
            activity.IsClaimed = true;
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
                item = new ClientInventoryItem { ItemId = task.RewardItemId, ItemType = ClientItemType.Material, Quantity = 0, IsLocked = false };
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

        public bool TryUpgradeHero(string heroId, out string failureReason)
        {
            failureReason = string.Empty;
            ClientHero hero = _heroes.Find(candidate => candidate.HeroId == heroId);
            if (hero == null || !hero.IsOwned) { failureReason = "英雄未拥有"; return false; }
            if (hero.Level >= 10) { failureReason = "已达到本地等级上限"; return false; }
            ClientInventoryItem material = _inventory.Find(candidate => candidate.ItemId == "material-001");
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

        public bool TrySetInventoryItemLocked(string itemId, bool isLocked)
        {
            ClientInventoryItem item = _inventory.Find(candidate => candidate.ItemId == itemId);
            if (item == null) return false;
            item.IsLocked = isLocked;
            DataChanged?.Invoke();
            return true;
        }

        public bool TryStarUpHero(string heroId, out string failureReason)
        {
            failureReason = string.Empty;
            ClientHero hero = _heroes.Find(candidate => candidate.HeroId == heroId);
            if (hero == null || !hero.IsOwned) { failureReason = "英雄未拥有"; return false; }
            if (hero.StarLevel >= 5) { failureReason = "已达到本地星级上限"; return false; }
            ClientInventoryItem material = _inventory.Find(candidate => candidate.ItemId == "material-001");
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

        public bool TrySetEquipmentEquipped(string itemId, bool equipped, out string failureReason)
        {
            failureReason = string.Empty;
            ClientInventoryItem item = _inventory.Find(candidate => candidate.ItemId == itemId);
            if (item == null || item.ItemType != ClientItemType.Equipment)
            {
                failureReason = "请选择装备";
                return false;
            }
            if (item.IsLocked)
            {
                failureReason = "装备已锁定";
                return false;
            }
            item.EquippedHeroId = equipped ? "hero-001" : string.Empty;
            DataChanged?.Invoke();
            return true;
        }

        public string GetFormationHeroId() { return _formationHeroId; }
        public bool TrySetFormationHero(string heroId)
        {
            ClientHero hero = _heroes.Find(candidate => candidate.HeroId == heroId);
            if (hero == null || !hero.IsOwned) return false;
            _formationHeroId = heroId;
            DataChanged?.Invoke();
            return true;
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

        public bool TrySetShowcaseHero(string heroId)
        {
            ClientHero hero = _heroes.Find(candidate => candidate.HeroId == heroId);
            if (hero == null || !hero.IsOwned)
                return false;

            _showcaseHeroId = hero.HeroId;
            DataChanged?.Invoke();
            return true;
        }

        public bool TrySetEquippedBadge(string badgeId)
        {
            if (badgeId != "badge-001" && badgeId != "badge-002" && badgeId != "badge-003")
                return false;
            _profile.EquippedTitleId = badgeId;
            DataChanged?.Invoke();
            return true;
        }
    }
}
