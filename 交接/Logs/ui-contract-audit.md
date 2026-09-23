# UI contract audit (auto-generated, read-only)

Generated from code literals vs. scene+prefab names, and from serialized fields left at fileID 0.

## A. Node names looked up in code but absent from scene AND all prefabs (candidate gaps)

- `ClientHierarchyAudit.cs`
    - `\n            [`
    - `] `
    - `PopupLayer/HeroPage英雄`
- `ClientShellStructuralRepair.cs`
    - `upCanvas/底框Image`
- `ClientHeroDetailPage.cs`
    - `前往获取Button`
    - `前往获取Canvas`
- `ClientRankPage.cs`
    - `LV等级`

## B. Serialized fields left unwired (fileID 0) on client components

- `ClientUiPopup` @ `Exchange code interface` : _popupRoot
- `ClientHeroCard` @ `?` : _teamBadgeNumber
- `ClientHeroCard` @ `?` : _teamBadgeNumber
- `ClientShopPurchaseQuantityProgress` @ `Progress bar icon` : _minimumText, _maximumText
- `ClientUiPopup` @ `Product Purchase Interface` : _popupRoot
- `ClientMailPage` @ `MailPage邮件` : _detail, _status, _emptyHint
- `ClientUiPopup` @ `Pending shipmentCanvas` : _popupRoot
- `ClientShellController` @ `ClientCanvas` : _popupService, _feedback
- `ClientUiPopup` @ `ActivityPage活动` : _popupRoot
- `ClientUiPopup` @ `Purchase Success Page` : _popupRoot
- `ClientUiPopup` @ `Historical Order Interface` : _popupRoot
- `ClientUiPopup` @ `ProfilePage个人中心` : _popupRoot
- `ClientHeroCard` @ `角色卡牌Button-final 1 (2)` : _teamBadgeNumber
- `ClientRankPage` @ `RankPage排行榜` : _avatars[], _frames[], _badges[], _attributes[]
- `ClientHomeRedDotController` @ `MainPage主页` : _noticeDot
- `ClientUiPopup` @ `Player lineup` : _popupRoot
- `ClientUiPopup` @ `HeroPage英雄` : _popupRoot
- `ClientUiPopup` @ `RankPage排行榜` : _popupRoot
- `ClientUiPopup` @ `MailPage邮件` : _popupRoot
- `ClientProfilePage` @ `ProfilePage个人中心` : _notice
- `ClientUiPopup` @ `FormationPage编队` : _popupRoot
- `ClientHeroCard` @ `英雄卡Item` : _teamBadgeNumber
- `ClientUiPopup` @ `联系客服Page` : _popupRoot
- `ClientHeroPage` @ `HeroPage英雄` : _heroTabSelected, _guideTabSelected
- `ClientHeroCard` @ `?` : _teamBadgeNumber
- `ClientHomePage` @ `MainPage主页` : _notice
- `ClientUiPopup` @ `ShopPage商店` : _popupRoot
- `ClientUiPopup` @ `Complete Guide Event Page全图鉴活动` : _popupRoot
- `ClientUiPopup` @ `Email Details Page（Have）` : _popupRoot
- `BottomFunctionIconView` @ `Bottom2` : _label
- `ClientUiPopup` @ `Success Receipt Interface` : _popupRoot
- `ClientUiPopup` @ `Email Details Page (No)` : _popupRoot
- `ClientHeroCard` @ `角色卡牌Button-final 1 (1)` : _teamBadgeNumber
- `BottomFunctionIconView` @ `Bottom1` : _label
- `ClientUiPopup` @ `HeroDetailPage英雄详情主页` : _popupRoot
- `ClientSystemFeedback` @ `ClientCanvas` : _toastText
- `ClientHeroCard` @ `角色卡牌Button-final 1` : _teamBadgeNumber
- `BottomFunctionIconView` @ `Bottom3` : _label
- `BottomFunctionIconView` @ `Bottom4` : _label

