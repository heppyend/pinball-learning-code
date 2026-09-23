using System;
using System.Collections.Generic;

namespace Pinball.Client.Domain
{
    public enum ClientCurrencyType
    {
        Gold = 1,
        Diamond = 2,
        Energy = 3,
    }

    /// <summary>
    /// 道具类型。**取值与配置表 `Item.Type` 一一对应**（2026-09-20 按表实测修正，见 `BUG_TRACKER.md` BUG-023 ②）：
    /// `1` = **货币**（点数 / 源晶 / 晶核）、`3` = **消耗品**（启迪之星 / 共鸣结晶·绿蓝紫橙红）。
    /// **`2` 在表内没有任何一行** ⇒ 语义未定义，故**不设成员**（遇到 2 会落到未定义值，属异常数据，需负责人确认后补）。
    ///
    /// <para><b>历史（为什么改）</b>：本枚举原为 <c>Material = 1, Equipment = 2, Consumable = 3</c> ——
    /// 三个成员**全部与表不符**，且表内**根本没有"装备"类型**，直接导致 <c>TrySetEquipmentEquipped</c> 永远失败。</para>
    /// </summary>
    public enum ClientItemType
    {
        /// <summary>未知 / 未定义（表内不存在 `Type` 为 0 或 2 的行；用于承接异常数据，不参与业务分支）。</summary>
        Unknown = 0,

        /// <summary>货币：点数 / 源晶 / 晶核（表 `Type = 1`）。</summary>
        Currency = 1,

        // 2 —— 表内没有任何行使用该值，语义待负责人确认，故此处**不定义成员**。

        /// <summary>消耗品：启迪之星 / 共鸣结晶（表 `Type = 3`）。</summary>
        Consumable = 3,
    }

    [Serializable]
    public sealed class ClientPlayerProfile
    {
        public string PlayerId;
        public string DisplayName;
        public int Level;
        public int CombatPower;
        public int EquippedTitleId;
        public int EquippedAvatarId;
    }

    /// <summary>
    /// 英雄（**表驱动**：静态字段来自配置表 `THero`，玩家状态本地模拟/服务端）。
    ///
    /// 静态：<see cref="HeroId"/> / <see cref="Name"/> / <see cref="Quality"/> / <see cref="ElementId"/> /
    /// <see cref="StarLevel"/> 等 —— 由 `IClientConfigService.GetHeroes()` 提供（按 `ClientShow` 过滤）。
    /// 玩家状态：<see cref="Level"/> / <see cref="IsOwned"/> / <see cref="CombatPower"/>。
    /// </summary>
    [Serializable]
    public sealed class ClientHero
    {
        /// <summary>英雄 Id（配置 `THero.Id`，如 `13001`；**int**）。</summary>
        public int HeroId;

        public string Name;

        /// <summary>元素 id（配置 `THero.Element`，**int 1..6**）。1..6 = 光/水/土/风/火/暗，见 <see cref="ClientElements"/>。</summary>
        public int ElementId;

        /// <summary>元素中文名（由 <see cref="ClientElements.Name"/> 映射，便于直接显示）。</summary>
        public string ElementName;

        /// <summary>元素图标资源名（配置 `THero.ElementIcon`，如 `ElementIcon_4`）。</summary>
        public string ElementIcon;

        /// <summary>品质（配置 `THero.Quality`，实测 3..6）。</summary>
        public int Quality;

        /// <summary>品质图标资源名（配置 `THero.QualityIcon`，如 `QualityIcon_3`）。</summary>
        public string QualityIcon;

        /// <summary>弹射类型（配置 `THero.CatapultType`，实测 1..3）。</summary>
        public int CatapultType;

        /// <summary>弹射图标资源名（配置 `THero.CatapultIcon`）。</summary>
        public string CatapultIcon;

        /// <summary>当前等级（**玩家状态**）。</summary>
        public int Level;

        /// <summary>当前星级（初始星级来自配置 `THero.Star`；升星后应由服务端给出）。</summary>
        public int StarLevel;

        /// <summary>最高星级（配置 `THero.MaxStar`）。</summary>
        public int MaxStar;

        /// <summary>
        /// 战力（**玩家状态**）。
        /// ⚠️ **配置 `THero` 表里没有战力字段** —— `Coefficient` 表有 `HPFight/AttackFight/DefenceFight/SpeedFight`
        /// 四个系数，看起来是"属性 × 系数"，但**公式与取值等级未经确认**。在确认前本字段只作为占位（默认 0），
        /// 不得据此设计数值玩法。见 `Assets/Client/MODULE.md` §15.3。
        /// </summary>
        public int CombatPower;

        /// <summary>是否已拥有（**玩家状态**）。</summary>
        public bool IsOwned;

        /// <summary>英雄头像资源名（配置 `THero.Avatar`，约定 `HeroAvatar_&lt;id&gt;`）。</summary>
        public string Avatar;

        /// <summary>半身像-卡面（配置 `THero.Portrait1`，约定 `HeroPortrait_&lt;id&gt;_c`）。</summary>
        public string Portrait1;

        /// <summary>半身像-出战位（配置 `THero.Portrait2`，约定 `HeroPortrait_&lt;id&gt;_d`）。</summary>
        public string Portrait2;
    }

    public enum ClientHeroAbilityType
    {
        Talent = 0,
        SecretTechnique = 1,
        Ultimate = 2,
    }

    public enum ClientHeroUpgradeResourceType
    {
        InventoryItem = 0,
        Currency = 1,
    }

    /// <summary>详情页图片均使用数字 ID；Sprite 的实际加载由可替换的视图资源解析器负责。</summary>
    [Serializable]
    public sealed class ClientHeroDetailData
    {
        public int HeroId;
        public string CardName;
        public int UpImageId;
        public List<int> IllustrationIds = new List<int>();
        public List<int> VerticalImageIds = new List<int>();
        public List<int> CardInfoImageIds = new List<int>();
        public List<int> LeftMiddleImageIds = new List<int>();
        public List<string> LeftMiddleTexts = new List<string>();
        public List<string> AttributeValues = new List<string>();
    }

    [Serializable]
    public sealed class ClientHeroUpgradeCost
    {
        public ClientHeroUpgradeResourceType ResourceType;
        public int ResourceId;
        public ClientCurrencyType CurrencyType;
        public int IconId;
        public int OwnedQuantity;
        public int RequiredQuantity;

        public bool IsEnough { get { return OwnedQuantity >= RequiredQuantity; } }
    }

    [Serializable]
    public sealed class ClientHeroAbilityUpgradePreview
    {
        public int HeroId;
        public ClientHeroAbilityType AbilityType;
        public int CurrentLevel;
        public int NextLevel;
        public List<ClientHeroUpgradeCost> Costs = new List<ClientHeroUpgradeCost>();

        public bool CanUpgrade
        {
            get
            {
                if (Costs.Count == 0)
                    return false;
                foreach (ClientHeroUpgradeCost cost in Costs)
                    if (cost == null || !cost.IsEnough)
                        return false;
                return true;
            }
        }
    }

    [Serializable]
    public sealed class ClientInventoryItem
    {
        public int ItemId;
        public ClientItemType ItemType;
        public int Quantity;
        public bool IsLocked;
        public int EquippedHeroId;
    }

    [Serializable]
    public sealed class ClientGachaPool
    {
        public string PoolId;
        public string DisplayName;
        public ClientCurrencyType CostCurrency;
        public int SingleDrawCost;
        public int TenDrawCost;
        public List<int> RewardHeroIds = new List<int>();
    }

    [Serializable]
    public sealed class ClientGachaDrawResult
    {
        public string PoolId;
        public int DrawCount;
        public List<int> RewardHeroIds = new List<int>();
    }

    /// <summary>商城角色卡牌的本地占位配置；正式商品资料接入时由服务端 DTO 替换。</summary>
    [Serializable]
    public sealed class ClientShopListing
    {
        public string ListingId;
        public string DisplayName;
        public int UnitCrystalStoneCost;
        public int PurchaseLimit;
    }

    [Serializable]
    public sealed class ClientMail
    {
        public string MailId;
        public string Title;
        public string Content;
        public int AttachmentItemId;
        public int AttachmentQuantity;
        public bool IsRead;
        public bool IsClaimed;
    }

    public enum ClientLeaderboardKind
    {
        BattlePower = 1,
        Challenge = 2,
    }

    /// <summary>排行榜行数据；图片字段由未来资源/服务适配器提供，本地演示可为空。</summary>
    [Serializable]
    public sealed class ClientLeaderboardEntry
    {
        public int Rank;
        public string PlayerId;
        public string DisplayName;
        public string Title;
        public int CombatPower;
        public int Floor;
        public int TurnCount;
        public int AvatarId;
        public int AvatarFrameId;
        public int BadgeId;
    }

    [Serializable]
    public sealed class ClientPlayerLineup
    {
        public string PlayerId;
        public int CombatPower;
        public int Floor;
        public int TurnCount;
        public List<ClientLineupCard> Cards = new List<ClientLineupCard>();
    }

    /// <summary>玩家打榜阵容卡牌数据。立绘、属性、品质与星级资源映射待资源契约确认。</summary>
    [Serializable]
    public sealed class ClientLineupCard
    {
        public string CardId;
        public string DisplayName;
        public int Level;
        public string IllustrationId;
        public string AttributeIconId;
        public string QualityId;
        public int StarCount;
    }

    /// <summary>
    /// 编队的一支队伍：<see cref="SlotCount"/> 个出战槽位。
    ///
    /// 结构**取自场景**（非杜撰）：`FormationPage编队/编队卡牌-Panel` 有 3 张槽位卡
    /// （含 `编队队内编号Image`），`页面分栏-Image/Panel` 有 `选中点1..4` + 左/右Button
    /// + `编组号-Text`(设计时文本 `01`) ⇒ 3 槽位 × 4 队伍。
    ///
    /// ⚠️ 属**可替换的本地模拟契约**：正式队伍数、槽位上限、解锁条件、
    /// 同名英雄可否重复上阵等规则**均未定义**，待产品/服务端契约确认后替换本模型。
    /// </summary>
    [Serializable]
    public sealed class ClientFormationTeam
    {
        /// <summary>槽位数量，与场景 `编队卡牌-Panel` 的子卡数一致。</summary>
        public const int SlotCount = 3;

        public int TeamIndex;

        /// <summary>编组号显示文本，如 `01`；对应场景 `编组号-Text`。</summary>
        public string TeamCode;

        /// <summary>各槽位英雄 ID；空串表示该槽位未选。长度恒为 <see cref="SlotCount"/>。</summary>
        public List<int> SlotHeroIds = new List<int>();

        public ClientFormationTeam() { }

        public ClientFormationTeam(int teamIndex)
        {
            TeamIndex = teamIndex;
            TeamCode = (teamIndex + 1).ToString("00");
            for (int index = 0; index < SlotCount; index++)
                SlotHeroIds.Add(0);
        }
    }

    [Serializable]
    public sealed class ClientActivity
    {
        public string ActivityId;
        public string Title;
        public string Description;
        public string DateText;
        public int BannerImageId;
        public int HotSpotImageId;
        public int PopularImageId;
        public bool ShowHotSpot;
        public bool ShowPopular;
        public int Progress;
        public int Target;
        public int RewardItemId;
        public int RewardQuantity;
        public bool IsOpen;
        public bool IsClaimed;
    }

    public enum ClientActivityTaskCategory
    {
        Newbie = 0,
        Advanced = 1,
    }

    public enum ClientRewardDeliveryStatus
    {
        Rejected = 0,
        Pending = 1,
        Granted = 2,
    }

    public enum ClientRewardDeliveryChannel
    {
        Unknown = 0,
        Inventory = 1,
        Mail = 2,
    }

    [Serializable]
    public sealed class ClientActivityTaskReward
    {
        public int ItemId;
        public int ItemImageId;
        public string DisplayName;
        public int Quantity;
    }

    /// <summary>活动页的新手/进阶任务数据；Category 是状态归属边界，不得跨分类复用实例。</summary>
    [Serializable]
    public sealed class ClientActivityTaskData
    {
        public string TaskId;
        public ClientActivityTaskCategory Category;
        public string Description;
        public int Progress;
        public int Target;
        public int PendingLabelImageId;
        public int ClaimLabelImageId;
        public int ClaimableButtonImageId;
        public bool IsClaimed;
        public bool IsClaimPending;
        public List<ClientActivityTaskReward> Rewards = new List<ClientActivityTaskReward>();

        public bool IsComplete { get { return Target > 0 && Progress >= Target; } }
    }

    [Serializable]
    public sealed class ClientActivityTaskClaimResult
    {
        public ClientRewardDeliveryStatus Status;
        public ClientRewardDeliveryChannel Channel;
        public string Message;
    }

    /// <summary>全图鉴活动页的可替换展示与领奖状态；正式规则和奖励契约待服务端确认。</summary>
    [Serializable]
    public sealed class ClientCompleteGuideEventData
    {
        public string EventId;
        public string CollectionText;
        public string RulesText;
        public string DateText;
        public int Progress;
        public int Target;
        public int PendingLabelImageId;
        public int ClaimLabelImageId;
        public int ClaimableButtonImageId;
        public bool IsClaimed;
        public bool IsClaimPending;
        public List<ClientActivityTaskReward> Rewards = new List<ClientActivityTaskReward>();

        public bool IsComplete { get { return Target > 0 && Progress >= Target; } }
    }

    [Serializable]
    public sealed class ClientTask
    {
        public string TaskId;
        public string Title;
        public string Description;
        public int Progress;
        public int Target;
        public int RewardItemId;
        public int RewardQuantity;
        public bool IsClaimed;
    }

    [Serializable]
    public sealed class ClientNotice
    {
        public string NoticeId;
        public string Title;
        public string Content;
        public bool IsRead;
    }

    [Serializable]
    public sealed class ClientMarble
    {
        public string MarbleId;
        public string DisplayName;
        public string Description;
        public bool IsOwned;
    }

    [Serializable]
    public sealed class ClientWallet
    {
        private readonly Dictionary<ClientCurrencyType, int> _balances = new Dictionary<ClientCurrencyType, int>();

        public int GetBalance(ClientCurrencyType currency)
        {
            int balance;
            return _balances.TryGetValue(currency, out balance) ? balance : 0;
        }

        public void SetBalance(ClientCurrencyType currency, int balance)
        {
            _balances[currency] = Math.Max(0, balance);
        }

        public IReadOnlyDictionary<ClientCurrencyType, int> GetBalances()
        {
            return _balances;
        }
    }
}
