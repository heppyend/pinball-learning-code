# Client Runtime Services 模块说明

## 职责

定义页面使用的客户端数据服务边界，并提供开发阶段的内存模拟实现。

## 接入规则

- `IClientDataService` 是 UI 与业务逻辑读取账户、货币、英雄、背包的唯一入口。
- 本地实现仅用于独立演示；未来服务端实现必须保持接口语义和变更通知一致。
- 网络请求、鉴权、重试与 DTO 映射归未来适配器实现，不进入页面回调。
- 英雄详情通过 `GetHeroDetail` 和 `GetHeroAbilityUpgradePreview` 提供展示与完整消耗清单；`TryUpgradeHeroAbility` 负责原子校验与扣除，页面不得自行改写背包或钱包。
- 活动页通过 `GetActivityTasks(category)` 分别读取新手/进阶任务，`TryClaimActivityTask(category, taskId, out result)` 校验分类、完成度和重复领取，并返回拒绝/处理中/已发放结果；真实服务可将奖励投递到背包或邮件，页面不得自行入库。
- 全图鉴活动通过 `GetCompleteGuideEvent()` 读取独立状态，`TryClaimCompleteGuideEvent(eventId, out result)` 统一校验活动、完成度、处理中和重复领取；当前奖励入背包仅为本地模拟，正式投递渠道待服务端契约确认。
