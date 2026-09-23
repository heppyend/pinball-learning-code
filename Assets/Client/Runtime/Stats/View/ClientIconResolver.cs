namespace Pinball.Client.Stats.View
{
    /// <summary>
    /// 图标解析委托 —— **View 不认识资源系统**：由调用方（页面 / 资源服务）决定"这个 id 对应哪张图"。
    ///
    /// <para>用委托而不是接口，是为了让 View **不依赖任何服务**（强解耦）。
    /// 委托由调用方**缓存一次**传入，不产生闭包分配（GC 原则）。</para>
    ///
    /// <para>注意：配置里图标有两种来源 —— `SkillIcon` / `ResId` 是 **int id**，
    /// 而 `Hero.Avatar` / `Item.Icon` 是**名字**。本委托只处理 int id 这一类。</para>
    /// </summary>
    /// <param name="iconId">图标 id（`SkillIcon` 等）。</param>
    /// <returns>对应的 Sprite；解析不到返回 null（View 会保持原图或留空，**不报错**）。</returns>
    public delegate UnityEngine.Sprite ClientIconResolver(int iconId);
}
