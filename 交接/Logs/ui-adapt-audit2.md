# UI adaptation audit, batch 2 (titles / top area, read-only)

Rule: a page title / top bar should be TOP-anchored (anchorMin.y = anchorMax.y = 1)
and must NOT be parented under "Bottom page function bar" (that caused overlap at other aspect ratios).

| node | kind | top-anchored | under bottom bar | verdict |
|---|---|---|---|---|
| `ClientCanvas/PopupLayer/ShopPage商店/Shopping Mall Title/Shopping Mall TitleCanvas/Shop title background frame` | title | False | False | **not top-anchored** |
| `ClientCanvas/PopupLayer/ActivityPage活动/活动任务Canvas/background/Activity title` | title | False | False | **not top-anchored** |
| `ClientCanvas/PopupLayer/Historical Order Interface/Title for Historical Order/Title for Historical Order` | title | False | False | **not top-anchored** |
| `ClientCanvas/PopupLayer/Historical Order Interface/Title for Historical Order` | title | False | False | **not top-anchored** |
| `ClientCanvas/PopupLayer/Pending shipmentCanvas/Title for Goods to Be Shipped` | title | False | False | **not top-anchored** |
| `ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubTabBar/Image/Panel/Toggle_Title` | title | True | False | OK |
| `ClientCanvas/PopupLayer/MailPage邮件/Email Title` | title | True | False | OK |
| `ClientCanvas/PopupLayer/ActivityPage活动/task title` | title | True | False | OK |
| `ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/Panel_Profile/RenameDialog/Title` | title | False | False | **not top-anchored** |
| `ClientCanvas/PopupLayer/ShopPage商店/Top function bar` | top-bar | True | False | OK |
| `ClientCanvas/PopupLayer/ProfilePage个人中心/PageContentRoot/'Panel_Personalize  '/SubContentRoot/Content_Title` | title | False | False | **not top-anchored** |
| `ClientCanvas/PopupLayer/MailPage邮件/Email Title/Email TitleCanvas/Emial title background frame` | title | False | False | **not top-anchored** |
| `ClientCanvas/PopupLayer/RankPage排行榜/Challange/Ballte Mall/Challange/Ballte Mall TitleCanvas/Challange/Ballte title background frame` | title | False | False | **not top-anchored** |
| `ClientCanvas/PopupLayer/ShopPage商店/Shopping Mall Title` | title | True | False | OK |
| `ClientCanvas/PopupLayer/Complete Guide Event Page全图鉴活动/task title` | title | True | False | OK |

