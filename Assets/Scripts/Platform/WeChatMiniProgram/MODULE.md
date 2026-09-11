# 微信小游戏桥接模块

## 职责

本模块定义 Unity 客户端与微信小游戏运行时之间的可替换边界。当前只提供平台能力契约、能力探测和不触发外部行为的受控回退；不承担账号换取、支付下单、资源发布或小游戏工程转换。

## 依赖与边界

- 不直接依赖 YooAsset、HybridCLR、XLua、URP、网络协议或 `UnityBridgeManager`。
- 不保存或传递 AppID、AppSecret、登录 code、openid、订单参数或真实服务端地址。
- 真实微信实现必须由已确认的 SDK/导出链路通过 `IWeChatMiniProgramBridge` 注入；不得修改既有 Android/iOS 原生桥接。

## 验证

非微信运行时调用接口应返回一次 `UnsupportedPlatform`，不抛异常且不访问网络。每次代码改动后在原项目执行 Unity 命令行编译。

## 待确认

转换 SDK 与版本、导出模板、AppID、域名白名单、隐私声明、登录换票协议、支付路径、CDN/首包策略及发布权限。
