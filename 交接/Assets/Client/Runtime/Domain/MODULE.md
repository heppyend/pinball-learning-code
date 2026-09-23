# Client Runtime Domain 模块说明

## 职责

定义非战斗客户端共享的领域数据类型。模型必须可由本地模拟数据或未来服务端 DTO 映射，不能携带 Unity 场景、UI 或网络依赖。

英雄详情使用 `ClientHeroDetailData`、`ClientHeroAbilityUpgradePreview` 与 `ClientHeroUpgradeCost` 传递纯数据；图片仅保存数字 ID，禁止在领域模型中引用 `Sprite` 或场景组件。

活动页使用 `ClientActivityTaskData.Category` 明确区分新手与进阶状态归属；奖励使用列表模型，领奖结果显式区分拒绝、处理中和已发放，并记录背包/邮件投递渠道。所有图片仍只保存数字 ID。

全图鉴活动使用 `ClientCompleteGuideEventData` 传递活动 ID、可配置文本、进度、领取状态及奖励列表；正式周期、规则与奖励内容未确认，本地值不得视为产品定案。
