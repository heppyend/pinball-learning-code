# UI adapt batches 1+2 static audit (read-only)

PASS 37 / FAIL 0

| result | invariant | measured |
|---|---|---|
| PASS | scene id 464106634 is 终结技Toggle | actual=终结技Toggle |
| PASS | scene id 95426909 is 秘技Toggle | actual=秘技Toggle |
| PASS | scene id 135353793 is Background | actual=Background |
| PASS | scene id 511910134 is leftmiddleCanvas | actual=leftmiddleCanvas |
| PASS | scene id 4683446771436228105 is 后退Button | actual=后退Button |
| PASS | scene id 268087959 is Content | actual=Content |
| PASS | scene id 2127216787 is Content | actual=Content |
| PASS | scene id 650215604 is group | actual=group |
| PASS | scene id 1938891263 is HeroDetailPage英雄详情主页 | actual=HeroDetailPage英雄详情主页 |
| PASS | scene id 905242878 is 天赋Toggle | actual=天赋Toggle |
| PASS | ClientHeroDetailToggleSkin attached to hero detail page root | guid=b3f7c15a9d2e47b8a6c40e5f1d8392a4 |
| PASS | 天赋Toggle/Background node exists |  |
| PASS | 天赋Toggle/Checkmark node exists (Toggle/Background/Checkmark) |  |
| PASS | 天赋Toggle/Background = its own selected-frame sprite, Image enabled | spriteGuid=a724f8e2f939f1049844fe4e9a54e0aa enabled=1 |
| PASS | 天赋Toggle/Checkmark = its own label sprite | spriteGuid=a3f8c8b35b6db3e4880a3dbffaf704f5 |
| PASS | 天赋Toggle keeps its persistent SetActive call | count=1 |
| PASS | 天赋Toggle/Checkmark pivot centered (label keeps its aspect ratio) | pivot=0.5,0.5 |
| PASS | 天赋Toggle/Checkmark sized to the label art (height ~46) | size=160,46 |
| PASS | 秘技Toggle/Background node exists |  |
| PASS | 秘技Toggle/Checkmark node exists (Toggle/Background/Checkmark) |  |
| PASS | 秘技Toggle/Background = its own selected-frame sprite, Image enabled | spriteGuid=401e7ee7e03eaf4448c94ddb08ac1200 enabled=1 |
| PASS | 秘技Toggle/Checkmark = its own label sprite | spriteGuid=3bd7ff8d28b75ec4d994a7c443009bad |
| PASS | 秘技Toggle keeps its persistent SetActive call | count=1 |
| PASS | 秘技Toggle/Checkmark pivot centered (label keeps its aspect ratio) | pivot=0.5,0.5 |
| PASS | 秘技Toggle/Checkmark sized to the label art (height ~46) | size=84,46 |
| PASS | 终结技Toggle/Background node exists |  |
| PASS | 终结技Toggle/Checkmark node exists (Toggle/Background/Checkmark) |  |
| PASS | 终结技Toggle/Background = its own selected-frame sprite, Image enabled | spriteGuid=8136c186c0705ad478bbb3fdb593ba14 enabled=1 |
| PASS | 终结技Toggle/Checkmark = its own label sprite | spriteGuid=8bd8804735915b943bde4de14a763162 |
| PASS | 终结技Toggle keeps its persistent SetActive call | count=1 |
| PASS | 终结技Toggle/Checkmark pivot centered (label keeps its aspect ratio) | pivot=0.5,0.5 |
| PASS | 终结技Toggle/Checkmark sized to the label art (height ~46) | size=227,46 |
| PASS | back button anchored to an edge (not the old mid anchor) | anchorMin.y=0 anchorMax.y=0 pos=60,52 |
| PASS | leftmiddleCanvas pinned to downCanvas top (mid anchor used to cover upCanvas/right) | anchorMin.y=1 anchorMax.y=1 pivot.y=1 pos=231,243 |
| PASS | attribute bar below the level card | anchorMin.y=0.5 pos=0,287 |
| PASS | HeroSubPage card Content left-anchored with x offset 0 (mid-anchor pushed the column outside the viewport) | anchorMin=0,1 anchorMax=0,1 pivot=0,1 pos=0,0 |
| PASS | GuideSubPage card Content left-anchored with x offset 0 (mid-anchor pushed the column outside the viewport) | anchorMin=0,1 anchorMax=0,1 pivot=0,1 pos=0,0 |

