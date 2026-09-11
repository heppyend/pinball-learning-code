using System;
using System.Collections.Generic;
using Pinball.Client.Domain;

namespace Pinball.Client.Services
{
    public interface IClientDataService
    {
        event Action DataChanged;

        ClientPlayerProfile GetProfile();
        ClientWallet GetWallet();
        IReadOnlyList<ClientHero> GetHeroes();
        IReadOnlyList<ClientInventoryItem> GetInventory();
        ClientGachaPool GetGachaPool();
        IReadOnlyList<ClientGachaDrawResult> GetGachaHistory();
        IReadOnlyList<ClientShopListing> GetShopListings();
        IReadOnlyList<ClientMail> GetMails();
        IReadOnlyList<ClientRankEntry> GetRankEntries();
        IReadOnlyList<ClientActivity> GetActivities();
        IReadOnlyList<ClientTask> GetTasks();
        IReadOnlyList<ClientNotice> GetNotices();
        IReadOnlyList<ClientMarble> GetMarbles();
        string GetShowcaseHeroId();
        string GetEquippedBadgeId();
        bool TrySpend(ClientCurrencyType currency, int amount);
        bool TryDrawGacha(string poolId, int drawCount, out ClientGachaDrawResult result);
        bool TryPurchaseShopListing(string listingId, int quantity, out string failureReason);
        int GetShopPurchasedQuantity(string listingId);
        bool TryReadMail(string mailId);
        bool TryClaimMail(string mailId, out string failureReason);
        int ClaimAllMails(out string failureReason);
        bool TryClaimActivity(string activityId, out string failureReason);
        bool TryClaimTask(string taskId, out string failureReason);
        bool TryReadNotice(string noticeId);
        bool TryUpgradeHero(string heroId, out string failureReason);
        bool TrySetInventoryItemLocked(string itemId, bool isLocked);
        bool TryStarUpHero(string heroId, out string failureReason);
        bool TrySetEquipmentEquipped(string itemId, bool equipped, out string failureReason);
        string GetFormationHeroId();
        bool TrySetFormationHero(string heroId);
        string GetEquippedMarbleId();
        bool TrySetEquippedMarble(string marbleId);
        bool TryRename(string displayName);
        bool TrySetShowcaseHero(string heroId);
        bool TrySetEquippedBadge(string badgeId);
    }
}
