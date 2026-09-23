using System;
using System.Collections.Generic;
using Pinball.Client.Domain;

namespace Pinball.Client.Services
{
    /// <summary>
    /// **玩家状态**与**本地模拟业务**的入口。
    ///
    /// 与 <see cref="IClientConfigService"/> 的分工（负责人 2026-09-20 定调「表驱动」）：
    /// <list type="bullet">
    /// <item>本接口回答"**我**拥有哪些、等级多少、装备了哪个、买了几个" —— 玩家状态，未来来自服务端；未接入前是本地模拟。</item>
    /// <item>`IClientConfigService` 回答"游戏里**有哪些**英雄 / 头像 / 徽章 / 道具" —— 静态配置，来自配置表，不需要服务端。</item>
    /// </list>
    ///
    /// ⚠️ **id 类型（2026-09-20 负责人定：全改 int）**：凡与配置表对应的实体 id 一律 `int`
    /// （英雄、道具、头像 / 头像框 / 徽章 / 铭牌 / 称号、技能 / 潜能）；
    /// 服务端自有的不透明标识（订单、邮件、活动、任务、公告、卡池、商品、玩家）仍是 `string`；
    /// 美术**资源名**（如 `QualityIcon_3`、`HeroPortrait_13001_c`）也是 `string`。
    /// </summary>
    public interface IClientDataService
    {
        event Action DataChanged;

        ClientPlayerProfile GetProfile();
        ClientWallet GetWallet();
        IReadOnlyList<ClientHero> GetHeroes();
        ClientHeroDetailData GetHeroDetail(int heroId);
        ClientHeroAbilityUpgradePreview GetHeroAbilityUpgradePreview(int heroId, ClientHeroAbilityType abilityType);
        bool TryUpgradeHeroAbility(int heroId, ClientHeroAbilityType abilityType, out string failureReason);
        IReadOnlyList<ClientInventoryItem> GetInventory();
        ClientGachaPool GetGachaPool();
        IReadOnlyList<ClientGachaDrawResult> GetGachaHistory();
        IReadOnlyList<ClientShopListing> GetShopListings();
        IReadOnlyList<ClientMail> GetMails();

        /// <summary>
        /// 删除全部邮件。**规则（2026-09-21 负责人确认）**：必须先"全部领取 + 全部已读"才允许删除；
        /// 删除后列表清空，邮箱页显示"暂无邮件"。不满足前置条件时返回 false 并给出原因。
        /// </summary>
        bool TryDeleteAllMails(out string failureReason);
        IReadOnlyList<ClientLeaderboardEntry> GetLeaderboardEntries(ClientLeaderboardKind leaderboardKind);
        ClientPlayerLineup GetPlayerLineup(string playerId);
        IReadOnlyList<ClientActivity> GetActivities();
        IReadOnlyList<ClientActivityTaskData> GetActivityTasks(ClientActivityTaskCategory category);
        ClientCompleteGuideEventData GetCompleteGuideEvent();
        IReadOnlyList<ClientTask> GetTasks();
        IReadOnlyList<ClientNotice> GetNotices();
        IReadOnlyList<ClientMarble> GetMarbles();
        int GetShowcaseHeroId();
        int GetEquippedBadgeId();
        bool TrySpend(ClientCurrencyType currency, int amount);
        bool TryDrawGacha(string poolId, int drawCount, out ClientGachaDrawResult result);
        bool TryPurchaseShopListing(string listingId, int quantity, out string failureReason);
        int GetShopPurchasedQuantity(string listingId);
        bool TryReadMail(string mailId);
        bool TryClaimMail(string mailId, out string failureReason);
        int ClaimAllMails(out string failureReason);
        bool TryClaimActivity(string activityId, out string failureReason);
        bool TryClaimActivityTask(ClientActivityTaskCategory category, string taskId, out ClientActivityTaskClaimResult result);
        bool TryClaimCompleteGuideEvent(string eventId, out ClientActivityTaskClaimResult result);
        bool TryClaimTask(string taskId, out string failureReason);
        bool TryReadNotice(string noticeId);
        bool TryUpgradeHero(int heroId, out string failureReason);
        bool TrySetInventoryItemLocked(int itemId, bool isLocked);
        bool TryStarUpHero(int heroId, out string failureReason);
        bool TrySetEquipmentEquipped(int itemId, bool equipped, out string failureReason);

        // ------------------------------------------------------------------
        // 收藏品（头像 / 头像框 / 徽章 / 铭牌 / 称号）
        //
        // "**有哪些**"来自配置表：`IClientConfigService.GetCollections(kind)`
        //   —— 已按 `ClientShow`（前端是否显示）过滤、按 `Sorting` 排序。
        // "**我拥有哪些 / 装备了哪个**"是玩家状态，在本接口：
        //   `NotUnlockedClientShow`（未解锁是否显示）决定未拥有的条目是否仍要占位显示。
        // ------------------------------------------------------------------

        /// <summary>某类收藏品中玩家**已拥有**的 id（未排序；显示顺序以配置 `Sorting` 为准）。</summary>
        IReadOnlyList<int> GetOwnedCollectionIds(ClientCollectionKind kind);

        /// <summary>当前**装备/使用中**的那一件 id；`0` 表示未装备。</summary>
        int GetEquippedCollectionId(ClientCollectionKind kind);

        /// <summary>装备某件收藏品（需已拥有）。成功会触发 <see cref="DataChanged"/>。</summary>
        bool TryEquipCollection(ClientCollectionKind kind, int id);

        // ------------------------------------------------------------------
        // 兼容写法：单槽编队 = "当前队伍的槽位 0"。
        // 正式玩法见下方 4 队伍 × 3 槽位。
        // ------------------------------------------------------------------

        int GetFormationHeroId();
        bool TrySetFormationHero(int heroId);

        // ------------------------------------------------------------------
        // 编队（FormationPage编队）：4 支队伍 × 3 个出战槽位
        //
        // 槽位数与队伍数**取自场景**（`编队卡牌-Panel` 3 张槽位卡、`页面分栏/Panel` 选中点1..4），
        // 属**可替换的本地模拟契约**；正式队伍/槽位/解锁规则待产品与服务端契约确认。
        // ------------------------------------------------------------------

        /// <summary>全部队伍（按 TeamIndex 升序，长度 4）。</summary>
        IReadOnlyList<ClientFormationTeam> GetFormationTeams();

        /// <summary>当前正在编辑的队伍下标（0 起）；对应场景 `编组号-Text` 与 `选中点1..4`。</summary>
        int GetFormationTeamIndex();

        /// <summary>切换当前队伍。越界返回 false，不改任何状态。</summary>
        bool TrySwitchFormationTeam(int teamIndex);

        /// <summary>
        /// 设置**当前队伍**某个槽位的英雄；<paramref name="heroId"/> 传 `0` 表示清空该槽位。
        /// 英雄不存在或未拥有时返回 false。会触发 <see cref="DataChanged"/>。
        /// </summary>
        bool TrySetFormationSlot(int slotIndex, int heroId);

        /// <summary>某支队伍的编队战力（该队各槽位英雄战力之和）；对应场景 `战力框/战力`。</summary>
        int GetFormationCombatPower(int teamIndex);

        /// <summary>
        /// **编队点选规则**（负责人 2026-09-20 明确的交互；页面只需调这一个方法）：
        /// <list type="number">
        /// <item>该英雄**已在当前队伍** ⇒ **移除**它，`slotNumber = 0`；**其余槽位不补位**（编号保持，空出的编号留给后来者）。</item>
        /// <item>该英雄不在队中且**有空位** ⇒ 加入**编号最小的空位**，`slotNumber` 返回该槽位号（1 起）。
        /// 同时空出多个编号时，必须补到**编号较小**的那个。</item>
        /// <item>该英雄不在队中且**三个编号都占满** ⇒ 返回 false，`failureReason` 说明原因（不加入）。</item>
        /// </list>
        /// 同样遵循"同队不可重复英雄"。成功时会触发 <see cref="DataChanged"/>。
        /// </summary>
        bool TryToggleFormationHero(int heroId, out int slotNumber, out string failureReason);

        string GetEquippedMarbleId();
        bool TrySetEquippedMarble(string marbleId);
        bool TryRename(string displayName);
        bool TrySetShowcaseHero(int heroId);
        bool TrySetEquippedBadge(int badgeId);
    }
}
