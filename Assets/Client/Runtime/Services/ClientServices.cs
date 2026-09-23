using System;
using Pinball.Client.Stats;
using UnityEngine;

namespace Pinball.Client.Services
{
    public static class ClientServices
    {
        private static IClientDataService _data;
        private static IClientConfigService _config;
        private static ClientStatsService _stats;
        private static LocalClientStatsGateway _statsGateway;

        public static bool IsInitialized { get { return _data != null; } }

        /// <summary>配置服务是否已建好（与 <see cref="IsInitialized"/> 不同：它只反映静态配置，先于玩家数据就绪）。</summary>
        public static bool HasConfig { get { return _config != null; } }

        public static IClientDataService Data
        {
            get
            {
                if (_data == null)
                    throw new InvalidOperationException("Client services have not been initialized.");
                return _data;
            }
        }

        /// <summary>
        /// **静态配置**（表驱动：英雄 / 头像 / 头像框 / 徽章 / 铭牌 / 称号 / 道具 / 技能 / 天赋 / 卡池）。
        ///
        /// 与 <see cref="Data"/> 的分工（负责人 2026-09-20 定调「表驱动」）：
        /// `Data` 回答“**我**拥有哪些、等级多少、装备了哪个”（玩家状态，来自服务端/本地模拟）；
        /// `Config` 回答“游戏里**有哪些**”（人人相同的静态定义，来自配置表，不需要服务端）。
        /// </summary>
        public static IClientConfigService Config
        {
            get
            {
                if (_config == null)
                    throw new InvalidOperationException("Client config service has not been initialized.");
                return _config;
            }
        }

        /// <summary>
        /// **数值与说明**（属性接口 / 技能天赋数值接口 / 说明文本）。
        ///
        /// <para>负责人 2026-09-20 的架构原则：**数值只由权威服务器计算并分发**，客户端只做展示。
        /// 因此这里暴露的 <see cref="IClientStatsService"/> **只有查询、没有计算**；
        /// 未接入服务器前由 <see cref="LocalClientStatsGateway"/> 扮演服务器。
        /// 设计与约定见 `Assets/Client/Runtime/Stats/MODULE.md`。</para>
        /// </summary>
        public static IClientStatsService Stats
        {
            get
            {
                if (_stats == null)
                    throw new InvalidOperationException("Client stats service has not been initialized.");
                return _stats;
            }
        }

        /// <summary>本地模拟的"服务器替身"（仅在未接入真实服务器时存在；接入后整体替换）。</summary>
        public static LocalClientStatsGateway StatsGateway { get { return _statsGateway; } }

        public static void InitializeForDevelopment()
        {
            // ⚠️ 顺序要紧：**先建配置服务，再建依赖配置表的服务**。
            // `LocalClientDataService` 的构造函数会用配置表构建英雄列表，
            // 而 `ClientServices.IsInitialized`（= `_data != null`）在它自己的构造期间仍为 false ——
            // 早期版本把顺序写反，导致"配置能读到 49 个英雄，数据服务却报英雄配置为空"。
            if (_config == null)
            {
                _config = new TableClientConfigService();

                // 刻意**立刻**触碰一次，让配置表在启动时就加载并打印行数，而不是等某个页面第一次查询才跑。
                // 原因有二：
                //   ① 诊断要主动出现 —— 表加载失败时底层只打一条警告就"按空表继续"，
                //      若无人查询就永远看不到线索（首轮实测就是这样：Console 里什么都没有）；
                //   ② 避免把"首次访问要读表 + 反射"的开销落在第一个打开页面的那一帧上。
                // `IsReady` 的 getter 会触发 `TableClientConfigService.EnsureBuilt()`。
                bool ready = _config.IsReady;
                if (!ready)
                    Debug.LogWarning("[Client] 配置表未就绪：本帧取不到任何静态配置，页面会显示空态。");
            }

            if (_data == null)
                _data = new LocalClientDataService();

            // 数值与说明模块：**网关负责"扮演服务器"，服务负责聚合与说明文本**。
            // 两者都不引用 Unity UI，View 反过来只吃数据（强解耦，见 Stats/MODULE.md §3）。
            if (_statsGateway == null)
                _statsGateway = new LocalClientStatsGateway(_config);

            if (_stats == null)
                _stats = new ClientStatsService(_config, _statsGateway, new ClientDescriptionFormatter());
        }

        public static void ReplaceDataService(IClientDataService dataService)
        {
            if (dataService == null)
                throw new ArgumentNullException(nameof(dataService));
            _data = dataService;
        }

        /// <summary>
        /// 退出 Play / 域重载 / 配置表更新后清空所有缓存（含数值模块的页签缓存与说明文本缓存）。
        /// </summary>
        public static void Reset()
        {
            if (_stats != null)
            {
                _stats.Dispose();
                _stats = null;
            }

            _statsGateway = null;
            _data = null;
            _config = null;
        }
    }
}
