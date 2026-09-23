# UI layout / layer audit (read-only)

## 1. ClientCanvas children

| child | active |
|---|---|
| PopupLayer | 1 |
| PagesLayer | 1 |
| SystemLayer | 0 |
| ToastRoot | 0 |

## 2. registered page/popup nodes (from the tool LayerMove table) vs actual scene

| node | expected layer | actual parent | full-screen root |
|---|---|---|---|
| ActivityPage活动 | PopupLayer | PopupLayer | YES |
| Complete Guide Event Page全图鉴活动 | PopupLayer | PopupLayer | YES |
| Email Details Page (No) | PopupLayer | PopupLayer | YES |
| Email Details Page（Have） | PopupLayer | PopupLayer | YES |
| Exchange code interface | PopupLayer | PopupLayer | no |
| FormationPage编队 | PopupLayer | PopupLayer | YES |
| HeroDetailPage英雄详情主页 | PopupLayer | PopupLayer | YES |
| HeroPage英雄 | PopupLayer | PopupLayer | YES |
| Historical Order Interface | PopupLayer | PopupLayer | no |
| MailPage邮件 | PopupLayer | PopupLayer | YES |
| Pending shipmentCanvas | PopupLayer | PopupLayer | no |
| Player lineup | PopupLayer | PopupLayer | no |
| Product Purchase Interface | PopupLayer | PopupLayer | no |
| ProfilePage个人中心 | PopupLayer | PopupLayer | YES |
| Purchase Success Page | PopupLayer | PopupLayer | YES |
| RankPage排行榜 | PopupLayer | PopupLayer | YES |
| ShopPage商店 | PopupLayer | PopupLayer | YES |
| Success Receipt Interface | PopupLayer | PopupLayer | YES |
| 联系客服Page | PopupLayer | PopupLayer | no |

## 3. list containers (nodes named Content)

| path | layout group | content-size fitter |
|---|---|---|
| ClientCanvas/PopupLayer/FormationPage编队/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/新手任务Scroll View/Viewport/Content/任务卡prefabs (2)/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/新手任务Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/活动任务Canvas/background/活动任务Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubContentRoot/Content_Widget/铭牌Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubContentRoot/Content_Title/称号Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/进阶任务Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/Email Details Page（Have）/邮件详情底框/邮件物品底框/Scroll View 1/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/进阶任务Scroll View/Viewport/Content/任务卡prefabs (5)/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubContentRoot/Content_Avatar/头像Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/新手任务Scroll View/Viewport/Content/任务卡prefabs (5)/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/RankPage排行榜/Challenge List/rank/邮件Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/新手任务Scroll View/Viewport/Content/任务卡prefabs/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/HeroPage英雄/HeroSubPage/MiddleCanvas/Canvas/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/Panel_Showcase/展示卡牌Scroll View 1/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/新手任务Scroll View/Viewport/Content/任务卡prefabs (1)/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/进阶任务Scroll View/Viewport/Content/任务卡prefabs (1)/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/HeroPage英雄/GuideSubPage/MiddleCanvas/Canvas/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/MailPage邮件/Email come/邮件Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/RankPage排行榜/Battle Power Ranking/rank/邮件Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/新手任务Scroll View/Viewport/Content/任务卡prefabs (3)/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/Player lineup/玩家阵容/Content | GridLayoutGroup | no |
| ClientCanvas/PopupLayer/ActivityPage活动/进阶任务Scroll View/Viewport/Content/任务卡prefabs (3)/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/进阶任务Scroll View/Viewport/Content/任务卡prefabs/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/进阶任务Scroll View/Viewport/Content/任务卡prefabs (4)/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/进阶任务Scroll View/Viewport/Content/任务卡prefabs (2)/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubContentRoot/Content_Frame/头像框Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ShopPage商店/Product page/展示卡牌Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubContentRoot/Content_Info/徽章Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/ActivityPage活动/新手任务Scroll View/Viewport/Content/任务卡prefabs (4)/background/道具大底框/Scroll View/Viewport/Content | GridLayoutGroup | yes |
| ClientCanvas/PopupLayer/MailPage邮件/Email come/邮件Scroll View/Viewport/Content/邮件Item/Scroll View/Viewport/Content | GridLayoutGroup | yes |

