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

    public enum ClientItemType
    {
        Material = 1,
        Equipment = 2,
        Consumable = 3,
    }

    [Serializable]
    public sealed class ClientPlayerProfile
    {
        public string PlayerId;
        public string DisplayName;
        public int Level;
        public int CombatPower;
        public string EquippedTitleId;
        public string EquippedAvatarId;
    }

    [Serializable]
    public sealed class ClientHero
    {
        public string HeroId;
        public string Name;
        public string Element;
        public int Level;
        public int StarLevel;
        public int CombatPower;
        public bool IsOwned;
    }

    [Serializable]
    public sealed class ClientInventoryItem
    {
        public string ItemId;
        public ClientItemType ItemType;
        public int Quantity;
        public bool IsLocked;
        public string EquippedHeroId;
    }

    [Serializable]
    public sealed class ClientGachaPool
    {
        public string PoolId;
        public string DisplayName;
        public ClientCurrencyType CostCurrency;
        public int SingleDrawCost;
        public int TenDrawCost;
        public List<string> RewardHeroIds = new List<string>();
    }

    [Serializable]
    public sealed class ClientGachaDrawResult
    {
        public string PoolId;
        public int DrawCount;
        public List<string> RewardHeroIds = new List<string>();
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
        public string AttachmentItemId;
        public int AttachmentQuantity;
        public bool IsRead;
        public bool IsClaimed;
    }

    [Serializable]
    public sealed class ClientRankEntry
    {
        public int Rank;
        public string PlayerId;
        public string DisplayName;
        public int CombatPower;
        public bool IsCurrentPlayer;
    }

    [Serializable]
    public sealed class ClientActivity
    {
        public string ActivityId;
        public string Title;
        public string Description;
        public int Progress;
        public int Target;
        public string RewardItemId;
        public int RewardQuantity;
        public bool IsOpen;
        public bool IsClaimed;
    }

    [Serializable]
    public sealed class ClientTask
    {
        public string TaskId;
        public string Title;
        public string Description;
        public int Progress;
        public int Target;
        public string RewardItemId;
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
