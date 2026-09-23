# Dead-button audit (static, read-only)

A button is reported when it has NO persistent UnityEvent call in the scene
AND its GameObject name does not appear in any runtime binding/lookup line (Assets/Client/Runtime).

buttons in scene = 169 ; scene-wired (persistent calls) = 0 ; candidates = 63

- `水Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/GuideSubPage/DownCanvas-7属性/Panel/水Button`
- `个性化确定键Button` @ `ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubContentRoot/Content_Info/默认徽章Image/个性化确定键Button`
- `全Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/GuideSubPage/DownCanvas-7属性/Panel/全Button`
- `火Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/HeroSubPage/DownCanvas-7属性/Panel/火Button`
- `图鉴Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/downCanvas/图鉴Button`
- `光Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/HeroSubPage/DownCanvas-7属性/Panel/光Button`
- `玩家战力排行prefabs (3)` @ `ClientCanvas/PopupLayer/RankPage排行榜/Battle Power Ranking/rank/邮件Scroll View/Viewport/Content/玩家战力排行prefabs (3)`
- `底框` @ `ClientCanvas/PopupLayer/RankPage排行榜/Challenge List/player rankprefab/底框`
- `玩家战力排行prefabs (1)` @ `ClientCanvas/PopupLayer/RankPage排行榜/Battle Power Ranking/rank/邮件Scroll View/Viewport/Content/玩家战力排行prefabs (1)`
- `玩家排行perfabs (3)` @ `ClientCanvas/PopupLayer/RankPage排行榜/Challenge List/rank/邮件Scroll View/Viewport/Content/玩家排行perfabs (3)`
- `个性化确定键Button` @ `ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubContentRoot/Content_Title/默认称号Image/个性化确定键Button`
- `英雄卡Item` @ `ClientCanvas/PopupLayer/HeroPage英雄/GuideSubPage/MiddleCanvas/Canvas/Scroll View/Viewport/Content/英雄卡Item`
- `风Button` @ `ClientCanvas/PopupLayer/FormationPage编队/元素属性分类列表-Image/Panel/风Button`
- `全Button` @ `ClientCanvas/PopupLayer/FormationPage编队/元素属性分类列表-Image/Panel/全Button`
- `SubtractButton` @ `ClientCanvas/PopupLayer/Product Purchase Interface/Purchase of digital items/底框背景/SubtractButton`
- `暗Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/HeroSubPage/DownCanvas-7属性/Panel/暗Button`
- `暗Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/GuideSubPage/DownCanvas-7属性/Panel/暗Button`
- `土Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/HeroSubPage/DownCanvas-7属性/Panel/土Button`
- `个性化确定键Button` @ `ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubTabBar/默认背景Image/个性化确定键Button`
- `MaxButton` @ `ClientCanvas/PopupLayer/Product Purchase Interface/Purchase of digital items/底框背景/MaxButton`
- `Bottom1` @ `ClientCanvas/PopupLayer/HeroDetailPage英雄详情主页/upCanvas/Panel/Bottom1`
- `玩家排行perfabs (2)` @ `ClientCanvas/PopupLayer/RankPage排行榜/Challenge List/rank/邮件Scroll View/Viewport/Content/玩家排行perfabs (2)`
- `全Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/HeroSubPage/DownCanvas-7属性/Panel/全Button`
- `?` @ ``
- `底框` @ `ClientCanvas/PopupLayer/RankPage排行榜/Battle Power Ranking/Battle Power rankprefab/底框`
- `邮件Item` @ `ClientCanvas/PopupLayer/MailPage邮件/Email come/邮件Scroll View/Viewport/Content/邮件Item`
- `光Button` @ `ClientCanvas/PopupLayer/FormationPage编队/元素属性分类列表-Image/Panel/光Button`
- `发货键Button` @ `ClientCanvas/PopupLayer/Pending shipmentCanvas/Bottom page function bar/发货键Canvas/发货键Button`
- `道具Button` @ `ClientCanvas/PopupLayer/Product Purchase Interface/道具Button`
- `个性化确定键Button` @ `ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubContentRoot/Content_Widget/默认铭牌背景Image/个性化确定键Button`
- `玩家排行perfabs (4)` @ `ClientCanvas/PopupLayer/RankPage排行榜/Challenge List/rank/邮件Scroll View/Viewport/Content/玩家排行perfabs (4)`
- `玩家战力排行prefabs (2)` @ `ClientCanvas/PopupLayer/RankPage排行榜/Battle Power Ranking/rank/邮件Scroll View/Viewport/Content/玩家战力排行prefabs (2)`
- `绑定ID卡 Button` @ `ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/Panel_Profile/downImage/绑定ID卡 Button`
- `领取键Button` @ `ClientCanvas/PopupLayer/Email Details Page（Have）/邮件详情底框/领取键Button`
- `风Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/GuideSubPage/DownCanvas-7属性/Panel/风Button`
- `Progress bar icon` @ `ClientCanvas/PopupLayer/Product Purchase Interface/Purchase of digital items/底框背景/Purchase progress bar/Progress bar icon`
- `?` @ ``
- `AddButton` @ `ClientCanvas/PopupLayer/Product Purchase Interface/Purchase of digital items/底框背景/AddButton`
- `水Button` @ `ClientCanvas/PopupLayer/FormationPage编队/元素属性分类列表-Image/Panel/水Button`
- `风Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/HeroSubPage/DownCanvas-7属性/Panel/风Button`
- `Bottom3` @ `ClientCanvas/PopupLayer/HeroDetailPage英雄详情主页/upCanvas/Panel (1)/Bottom3`
- `Bottom2` @ `ClientCanvas/PopupLayer/HeroDetailPage英雄详情主页/upCanvas/Panel/Bottom2`
- `火Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/GuideSubPage/DownCanvas-7属性/Panel/火Button`
- `土Button` @ `ClientCanvas/PopupLayer/FormationPage编队/元素属性分类列表-Image/Panel/土Button`
- `玩家排行perfabs (1)` @ `ClientCanvas/PopupLayer/RankPage排行榜/Challenge List/rank/邮件Scroll View/Viewport/Content/玩家排行perfabs (1)`
- `火Button` @ `ClientCanvas/PopupLayer/FormationPage编队/元素属性分类列表-Image/Panel/火Button`
- `玩家战力排行prefabs` @ `ClientCanvas/PopupLayer/RankPage排行榜/Battle Power Ranking/rank/邮件Scroll View/Viewport/Content/玩家战力排行prefabs`
- `Bottom4` @ `ClientCanvas/PopupLayer/HeroDetailPage英雄详情主页/upCanvas/Panel (1)/Bottom4`
- `玩家排行perfabs` @ `ClientCanvas/PopupLayer/RankPage排行榜/Challenge List/rank/邮件Scroll View/Viewport/Content/玩家排行perfabs`
- `水Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/HeroSubPage/DownCanvas-7属性/Panel/水Button`
- `锁Button` @ `ClientCanvas/PopupLayer/HeroDetailPage英雄详情主页/upCanvas/卡牌信息Canvas/Card  info 1/锁Button`
- `?` @ ``
- `删除Button` @ `ClientCanvas/PopupLayer/Email Details Page（Have）/邮件详情底框/删除Button`
- `删除Button` @ `ClientCanvas/PopupLayer/Email Details Page (No)/邮件详情底框/删除Button`
- `暗Button` @ `ClientCanvas/PopupLayer/FormationPage编队/元素属性分类列表-Image/Panel/暗Button`
- `英雄Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/downCanvas/英雄Button`
- `玩家战力排行prefabs (4)` @ `ClientCanvas/PopupLayer/RankPage排行榜/Battle Power Ranking/rank/邮件Scroll View/Viewport/Content/玩家战力排行prefabs (4)`
- `个性化确定键Button` @ `ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubContentRoot/Content_Avatar/默认头像Image/个性化确定键Button`
- `?` @ ``
- `个性化确定键Button` @ `ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubContentRoot/Content_Frame/默认头像框Image/个性化确定键Button`
- `光Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/GuideSubPage/DownCanvas-7属性/Panel/光Button`
- `搜索icon-image` @ `ClientCanvas/PopupLayer/FormationPage编队/搜索icon-image`
- `土Button` @ `ClientCanvas/PopupLayer/HeroPage英雄/GuideSubPage/DownCanvas-7属性/Panel/土Button`

