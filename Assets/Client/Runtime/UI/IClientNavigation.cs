using Pinball.Client.Domain;
using UnityEngine;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 页面导航能力（MVC 的 Controller 侧基础设施）。
    ///
    /// 这是 View 唯一被允许使用的导航入口。页面不得自己 SetActive 别的页面，
    /// 也不得依赖静态事件——静态事件会让"所有还活着的订阅者"同时响应，
    /// 正是此前多个导航所有者并行改写可见性的根因。
    /// </summary>
    public interface IClientNavigation
    {
        /// <summary>当前页面 ID。</summary>
        ClientUiPageId CurrentPage { get; }

        /// <summary>返回栈中是否还有上一页。</summary>
        bool CanGoBack { get; }

        /// <summary>打开目标页面并压栈。当前页会收到 OnPagePause。</summary>
        void Open(ClientUiPageId pageId);

        /// <summary>返回上一页。栈为空时由实现决定兜底行为（通常是回到主页）。</summary>
        void Back();

        /// <summary>直接回到主页并清空返回栈，用于一级页面的"返回主页"入口。</summary>
        void ReturnHome();
    }

    /// <summary>
    /// 需要发起导航的 View 实现此接口，以便在导航内核启用时被注入 IClientNavigation。
    ///
    /// 替代原先是 public static event 的导航请求（ClientHomePage.NavigationRequested、
    /// ClientProfilePage.HomeShowcaseRequested / BadgeRequested、
    /// ClientHeroPage.HeroDetailRequested）。
    /// </summary>
    public interface IClientNavigationHost
    {
        /// <summary>
        /// 由导航内核注入。实现方应立即保存引用；不得在 OnDisable 中清空。
        /// 需要额外能力的视图可在此对 navigation 做接口转换（见下方两个接口）。
        /// </summary>
        void BindNavigation(IClientNavigation navigation);
    }

    /// <summary>
    /// 主页一级入口导航：视图只表达"用户想去哪个目的地"，
    /// 目的地到页面 ID 的翻译属于 Controller 职责，由内核完成。
    /// </summary>
    public interface IClientHomeNavigation
    {
        void OpenFromHome(ClientHomeDestination destination);

        /// <summary>
        /// 该目的地当前**能否真的打开**（弹窗已按 pageId 注册，或已在 PagesLayer 登记）。
        ///
        /// 用途：主页在“点了没反应”之前拦住 —— 场景里尚未实现的页面（扭蛋 / 背包 / 任务 / 公告 / 弹珠）
        /// 应当提示“开发中”，而不是走一次注定被忽略的 `Open`。
        /// </summary>
        bool CanOpenFromHome(ClientHomeDestination destination);
    }

    /// <summary>
    /// 携带选中英雄打开详情页。英雄与卡牌立绘是 UI 侧选中上下文，
    /// 因此随导航一起传递，而不是塞进 IClientNavigation 的纯路由签名里。
    /// </summary>
    public interface IClientHeroDetailOpener
    {
        void OpenHeroDetail(ClientHero hero, Sprite cardIllustration);
    }

    /// <summary>
    /// 导航结果观察者：页面级路由变化后收到通知，用于驱动"页内子路由"
    /// （例如个人中心内部的 铭牌/称号/头像 等子页）。
    ///
    /// 页内子路由只能由本控制器管理自身子页的可见性，
    /// 不得反向控制 PagesLayer 上的同级页面。
    /// </summary>
    public interface IClientNavigationObserver
    {
        void OnNavigated(ClientUiPageId pageId);
    }
}
