# Per-screen UI adaptation audit (read-only)

Rules checked: root/background full-screen stretch; bottom bar horizontally stretched;
list containers: GridLayoutGroup childAlignment (4 = MiddleCenter is wrong for fixed-slot lists)
and ContentSizeFitter verticalFit (0 = Unconstrained means the list never grows => no scrolling).

| page/popup | root full-screen | panel by design | background | bottom bar | grid MiddleCenter | fitter Unconstrained | verdict |
|---|---|---|---|---|---|---|---|
| ActivityPage活动 | True | False | YES | stretched | 0 | 12 | OK |
| Complete Guide Event Page全图鉴活动 | True | False | YES | stretched | 0 | 0 | OK |
| Email Details Page (No) | True | False | n/a | n/a | 0 | 0 | OK |
| Email Details Page（Have） | True | False | n/a | n/a | 0 | 1 | OK |
| Exchange code interface | False | True | YES | n/a | 0 | 0 | OK |
| FormationPage编队 | True | False | YES | n/a | 0 | 1 | OK |
| HeroDetailPage英雄详情主页 | True | False | YES | n/a | 0 | 0 | OK |
| HeroPage英雄 | True | False | YES | n/a | 0 | 0 | OK |
| Historical Order Interface | False | True | YES | n/a | 0 | 0 | OK |
| MailPage邮件 | True | False | YES | n/a | 0 | 2 | OK |
| MainPage主页 | True | False | YES | n/a | 0 | 0 | OK |
| Pending shipmentCanvas | False | True | YES | stretched | 0 | 0 | OK |
| Player lineup | False | True | YES | n/a | 0 | 0 | OK |
| Product Purchase Interface | False | True | no | n/a | 0 | 0 | **background not full-screen** |
| ProfilePage个人中心 | True | False | n/a | n/a | 0 | 5 | OK |
| Purchase Success Page | True | True | no | n/a | 0 | 0 | **background not full-screen** |
| RankPage排行榜 | True | False | YES | stretched | 0 | 2 | OK |
| ShopPage商店 | True | False | YES | stretched | 0 | 0 | OK |
| Success Receipt Interface | True | False | n/a | n/a | 0 | 0 | OK |
| 弹窗遮罩 | True | False | n/a | n/a | 0 | 0 | OK |
| 联系客服Page | False | False | no | n/a | 0 | 0 | **background not full-screen** |

