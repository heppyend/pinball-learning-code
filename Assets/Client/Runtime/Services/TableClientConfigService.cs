using System;
using System.Collections.Generic;
using LC.Newtonsoft.Json.Linq;
using Pinball.Client.Domain;
using UnityEngine;

namespace Pinball.Client.Services
{
    /// <summary>
    /// <see cref="IClientConfigService"/> 的**表实现** —— 直接解析 json 成客户端只读模型。
    /// **只读，不写表**；结果缓存一次。
    ///
    /// <para><b>边界（2026-09-20 负责人明确：只负责 Client 与平台移植）：</b>
    /// 本类**不引用** `HotFix` 程序集里的任何 `T*Helper` / `TableManager`，也不触发那套"21 张表整批加载" ——
    /// 后者会把客户端用不到、且与工程旧生成类结构不符的 `Skill` 表也解析一遍，刷 99 条
    /// `配置表解析出错`（`BUG_TRACKER.md` BUG-022），把 Console 淹掉挡住验收。
    /// 客户端只读自己需要的那几张，数据源优先级由 <see cref="ClientTableSource"/> 负责
    /// （远端 `config.pkg` 优先 → 本地 `Resources/Table/*.json` 兜底）。
    /// 这样既不碰原公司/战斗代码，也不受 HotFix 的 IL1005 与旧 ref 问题影响。</para>
    ///
    /// <para><b>过滤与排序一律用表自身的字段</b>：`ClientShow`（前端是否显示）、
    /// `NotUnlockedClientShow`（未解锁是否显示）、`Sorting`（排序）；这里不另立规则。</para>
    /// </summary>
    public sealed class TableClientConfigService : IClientConfigService
    {
        private static readonly List<ClientCollectionConfig> EmptyCollections = new List<ClientCollectionConfig>();

        /// <summary>客户端需要的表 —— **刻意不包含**战斗侧的表（Skill / Monster / MapCopy / Coefficient …）。</summary>
        private static readonly string[] HeroTable = { "Hero" };
        private static readonly string[] CollectionTables = { "Head", "HeadFrame", "Badge", "Nameplate", "Title" };

        private bool _built;
        private bool _reported;

        private readonly List<ClientHeroConfig> _heroes = new List<ClientHeroConfig>();
        /// <summary>**前端可见**英雄（列表展示用；按 `ClientShow` 过滤）。</summary>
        private readonly Dictionary<int, ClientHeroConfig> _heroById = new Dictionary<int, ClientHeroConfig>();

        /// <summary>
        /// **表内全部**英雄（按 id 查定义用）。
        /// ⚠️ 与 <see cref="_heroById"/> 分开是必须的：`ClientShow` 是"**列表要不要显示它**"的规则，
        /// 不是"**能不能查这张表**"的规则。实测那 4 个天赋数据完整的英雄（13001/14001/14007/15001）
        /// `ClientShow` 都是 0 —— 若按可见性过滤，就查不到它们的技能/天赋/属性。
        /// </summary>
        private readonly Dictionary<int, ClientHeroConfig> _heroByIdAll = new Dictionary<int, ClientHeroConfig>();
        private readonly Dictionary<ClientCollectionKind, List<ClientCollectionConfig>> _collections =
            new Dictionary<ClientCollectionKind, List<ClientCollectionConfig>>();
        private readonly Dictionary<string, int> _rowCounts = new Dictionary<string, int>();

        // ---- 道具 / 技能 / 天赋 / 随机英雄池（2026-09-20 补齐）----
        private readonly List<ClientItemConfig> _items = new List<ClientItemConfig>();
        private readonly Dictionary<int, ClientItemConfig> _itemById = new Dictionary<int, ClientItemConfig>();
        private readonly Dictionary<int, ClientSkillConfig> _skillById = new Dictionary<int, ClientSkillConfig>();
        private readonly Dictionary<int, ClientPotencyConfig> _potencyById = new Dictionary<int, ClientPotencyConfig>();
        private readonly List<ClientRandomHeroConfig> _randomHeroPool = new List<ClientRandomHeroConfig>();

        public bool IsReady
        {
            get { EnsureBuilt(); return _heroById.Count > 0; }
        }

        public IReadOnlyList<ClientHeroConfig> GetHeroes()
        {
            EnsureBuilt();
            return _heroes;
        }

        public bool TryGetHero(int heroId, out ClientHeroConfig hero)
        {
            EnsureBuilt();
            // 按 id 查定义 ⇒ 走"全部英雄"（不受 ClientShow 影响，见 _heroByIdAll 说明）
            return _heroByIdAll.TryGetValue(heroId, out hero);
        }

        public IReadOnlyList<ClientCollectionConfig> GetCollections(ClientCollectionKind kind)
        {
            EnsureBuilt();

            List<ClientCollectionConfig> list;
            if (_collections.TryGetValue(kind, out list))
                return list;
            return EmptyCollections;
        }

        // ------------------------------------------------------------------
        // 构建
        // ------------------------------------------------------------------

        private void EnsureBuilt()
        {
            if (_built)
                return;

            _heroes.Clear();
            _heroById.Clear();
            _heroByIdAll.Clear();
            _collections.Clear();
            _rowCounts.Clear();

            BuildHeroes();
            BuildCollections();
            BuildItems();
            BuildSkills();
            BuildPotencies();
            BuildRandomHeroPool();

            // 只有真的拿到英雄数据才算建好；否则留待下次访问重试（例如远端包稍后才落盘）。
            _built = _heroById.Count > 0;

            if (!_reported)
            {
                _reported = true;
                Report();
            }
        }

        private void BuildHeroes()
        {
            JArray rows = LoadRows("Hero");
            if (rows == null)
                return;

            for (int i = 0; i < rows.Count; i++)
            {
                JToken row = rows[i];

                int heroId = ReadInt(row, "Id", 0);
                if (heroId <= 0)
                    continue;

                bool showInClient = ReadInt(row, "ClientShow", 0) != 0;

                ClientHeroConfig config = new ClientHeroConfig
                {
                    HeroId = heroId,
                    Name = ReadString(row, "Name"),
                    Quality = ReadInt(row, "Quality", 0),
                    QualityIcon = ReadString(row, "QualityIcon"),
                    CatapultType = ReadInt(row, "CatapultType", 0),
                    CatapultIcon = ReadString(row, "CatapultIcon"),
                    Element = ReadInt(row, "Element", 0),
                    ElementIcon = ReadString(row, "ElementIcon"),
                    Star = ReadInt(row, "Star", 0),
                    MaxStar = ReadInt(row, "MaxStar", 0),
                    Avatar = ReadString(row, "Avatar"),
                    Portrait1 = ReadString(row, "Portrait1"),
                    Portrait2 = ReadString(row, "Portrait2"),
                    ShowInClient = showInClient,
                    Skills = ReadIntList(row, "Skill"),        // 表字段 Skill：本英雄的 4 个技能 id
                    Potencies = ReadIntList(row, "Potency"),   // 表字段 Potency：本英雄的 13 个天赋 id
                    // 逐级属性数组：**服务器计算属性的输入**，客户端不套公式（见 Stats/MODULE.md）
                    HpBase = ReadIntArray(row, "HpBase"),
                    Attack = ReadIntArray(row, "Attack"),
                    Defence = ReadIntArray(row, "Defence"),
                    Speed = ReadIntArray(row, "Speed"),
                    CritRate = ReadIntArray(row, "CritRate"),
                    CritHurt = ReadIntArray(row, "CritHurt"),
                    LevelCap = ReadIntArray(row, "levelCap"),
                };

                // 表内**全部**英雄都登记，供"按 id 查定义"（技能 / 天赋 / 属性）；
                // 只有前端可见的才进列表（`_heroes` / `_heroById`）。
                _heroByIdAll[heroId] = config;

                if (showInClient)
                {
                    _heroes.Add(config);
                    _heroById[heroId] = config;
                }
            }

            _heroes.Sort(CompareHero);
        }

        private static int CompareHero(ClientHeroConfig left, ClientHeroConfig right)
        {
            if (left == null || right == null)
                return 0;
            return left.HeroId.CompareTo(right.HeroId);
        }

        private void BuildCollections()
        {
            Publish(ClientCollectionKind.Head, "Head", true);
            Publish(ClientCollectionKind.HeadFrame, "HeadFrame", false);
            Publish(ClientCollectionKind.Badge, "Badge", false);
            Publish(ClientCollectionKind.Nameplate, "Nameplate", false);
            Publish(ClientCollectionKind.Title, "Title", false);
        }

        private void Publish(ClientCollectionKind kind, string tableName, bool avatarIsName)
        {
            List<ClientCollectionConfig> list = new List<ClientCollectionConfig>();
            JArray rows = LoadRows(tableName);

            if (rows != null)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    JToken row = rows[i];

                    if (ReadInt(row, "ClientShow", 0) == 0)
                        continue;   // 前端不显示

                    list.Add(new ClientCollectionConfig
                    {
                        Id = ReadInt(row, "Id", 0),
                        Name = ReadString(row, "Name"),
                        Desc = ReadString(row, "Desc"),
                        Sorting = ReadInt(row, "Sorting", 0),
                        ShowInClient = true,
                        // 未解锁时前端是否仍显示 —— 拥有/未解锁规则的唯一来源
                        ShowWhenLocked = ReadInt(row, "NotUnlockedClientShow", 0) != 0,
                        ResId = avatarIsName ? 0 : ReadInt(row, "ResId", 0),
                        Avatar = avatarIsName ? ReadString(row, "Avatar") : null,
                    });
                }
            }

            list.Sort(CompareCollection);
            _collections[kind] = list;
        }

        private static int CompareCollection(ClientCollectionConfig left, ClientCollectionConfig right)
        {
            if (left == null || right == null)
                return 0;
            int bySorting = left.Sorting.CompareTo(right.Sorting);
            return bySorting != 0 ? bySorting : left.Id.CompareTo(right.Id);
        }

        // ------------------------------------------------------------------
        // 道具 / 技能 / 天赋 / 随机英雄池（只读；同样"有哪些"由配置表回答）
        // ------------------------------------------------------------------

        private void BuildItems()
        {
            JArray rows = LoadRows("Item");
            if (rows == null)
                return;

            for (int index = 0; index < rows.Count; index++)
            {
                JToken row = rows[index];
                int id = ReadInt(row, "Id", 0);
                if (id <= 0)
                    continue;

                ClientItemConfig item = new ClientItemConfig
                {
                    Id = id,
                    Name = ReadString(row, "Name"),
                    Desc = ReadString(row, "Desc"),
                    Icon = ReadString(row, "Icon"),
                    Type = ReadInt(row, "Type", 0),
                    Quality = ReadInt(row, "Quality", 0),
                    Stack = ReadInt(row, "Stack", 0),
                    Place = ReadInt(row, "Place", 0),
                    IsUse = ReadInt(row, "is_use", 0) != 0,
                    IsUseValue = ReadInt(row, "is_use_value", 0),
                };

                _items.Add(item);
                _itemById[id] = item;
            }
        }

        private void BuildSkills()
        {
            JArray rows = LoadRows("Skill");
            if (rows == null)
                return;

            for (int index = 0; index < rows.Count; index++)
            {
                JToken row = rows[index];
                int id = ReadInt(row, "Id", 0);
                if (id <= 0)
                    continue;

                _skillById[id] = new ClientSkillConfig
                {
                    Id = id,
                    Name = ReadString(row, "Name"),
                    Desc = ReadString(row, "Desc"),
                    SkillIcon = ReadInt(row, "SkillIcon", 0),
                    Type = ReadInt(row, "Type", 0),
                    EffectLevel = ReadIntList(row, "EffectLevel"),
                };
            }
        }

        private void BuildPotencies()
        {
            JArray rows = LoadRows("Potency");
            if (rows == null)
                return;

            for (int index = 0; index < rows.Count; index++)
            {
                JToken row = rows[index];
                int id = ReadInt(row, "Id", 0);
                if (id <= 0)
                    continue;

                _potencyById[id] = new ClientPotencyConfig
                {
                    Id = id,
                    Name = ReadString(row, "Name"),
                    Desc = ReadString(row, "Desc"),
                    EffectType = ReadInt(row, "EffectType", 0),
                    EffectParameters = ReadIntList(row, "EffectParameters"),
                    Quality = ReadInt(row, "Quality", 0),
                    SkillIcon = ReadInt(row, "SkillIcon", 0),
                };
            }
        }

        private void BuildRandomHeroPool()
        {
            JArray rows = LoadRows("RandomHero");
            if (rows == null)
                return;

            for (int index = 0; index < rows.Count; index++)
            {
                JToken row = rows[index];
                _randomHeroPool.Add(new ClientRandomHeroConfig
                {
                    Id = ReadInt(row, "Id", 0),
                    HeroId = ReadInt(row, "Heroid", 0),   // 注意表字段名是 Heroid
                });
            }
        }

        public IReadOnlyList<ClientItemConfig> GetItems()
        {
            EnsureBuilt();
            return _items;
        }

        public bool TryGetItem(int itemId, out ClientItemConfig item)
        {
            EnsureBuilt();
            return _itemById.TryGetValue(itemId, out item);
        }

        public bool TryGetSkill(int skillId, out ClientSkillConfig skill)
        {
            EnsureBuilt();
            return _skillById.TryGetValue(skillId, out skill);
        }

        public bool TryGetPotency(int potencyId, out ClientPotencyConfig potency)
        {
            EnsureBuilt();
            return _potencyById.TryGetValue(potencyId, out potency);
        }

        public IReadOnlyList<ClientSkillConfig> GetHeroSkills(int heroId)
        {
            EnsureBuilt();

            List<ClientSkillConfig> result = new List<ClientSkillConfig>();
            ClientHeroConfig hero;
            if (!_heroByIdAll.TryGetValue(heroId, out hero) || hero.Skills == null)
                return result;

            for (int index = 0; index < hero.Skills.Count; index++)
            {
                ClientSkillConfig skill;
                if (_skillById.TryGetValue(hero.Skills[index], out skill))
                    result.Add(skill);
            }
            return result;
        }

        public IReadOnlyList<ClientPotencyConfig> GetHeroPotencies(int heroId)
        {
            EnsureBuilt();

            // ⚠️ 54 个英雄里**只有 4 个**（13001 玉兔 / 14001 嫦娥 / 14007 唐三藏 / 15001 孙悟空）的
            // `THero.Potency` 是真实 id；其余 50 个是**占位 `[1,2]`**，而 `Potency` 表最小 id 是 `300101`
            // ⇒ 这里**一个都查不到**。
            //
            // **负责人 2026-09-20 明确：占位的地方空着就行、不要报错。**
            // 所以此处**静默跳过**未知 id、返回空集合（UI 显示空态）——
            // **这是预期行为，不是缺陷**：不要"修"成按行号取，也不要补默认值或打警告。
            // 详见 `Assets/Client/MODULE.md` §15.6。
            List<ClientPotencyConfig> result = new List<ClientPotencyConfig>();
            ClientHeroConfig hero;
            if (!_heroByIdAll.TryGetValue(heroId, out hero) || hero.Potencies == null)
                return result;

            for (int index = 0; index < hero.Potencies.Count; index++)
            {
                ClientPotencyConfig potency;
                if (_potencyById.TryGetValue(hero.Potencies[index], out potency))
                    result.Add(potency);
            }
            return result;
        }

        public IReadOnlyList<ClientRandomHeroConfig> GetRandomHeroPool()
        {
            EnsureBuilt();
            return _randomHeroPool;
        }

        /// <summary>
        /// 读一个**整型数组**字段。表里的数组有两种写法，都要支持：
        /// 真数组（`[1,2]`）与**字符串形式的数组**（`"[10,20,30]"`，实测 `THero.levelCap`、`TSkill.EffectLevel` 都是这种）。
        /// </summary>
        private static List<int> ReadIntList(JToken row, string name)
        {
            List<int> values = new List<int>();
            if (row == null)
                return values;

            JToken token = row[name];
            if (token == null || token.Type == JTokenType.Null)
                return values;

            JArray array = token as JArray;
            if (array == null && token.Type == JTokenType.String)
            {
                try { array = JArray.Parse(token.Value<string>()); }
                catch { array = null; }   // 不是合法 json 数组就当作空，不抛异常打断整表
            }
            if (array == null)
                return values;

            for (int index = 0; index < array.Count; index++)
            {
                try { values.Add(array[index].Value<int>()); }
                catch { /* 单个元素异常只跳过它 */ }
            }
            return values;
        }

        /// <summary>同上，但返回 <c>int[]</c>（只在建结构时调用一次，之后刷新零分配）。</summary>
        private static int[] ReadIntArray(JToken row, string name)
        {
            List<int> values = ReadIntList(row, name);
            return values.Count > 0 ? values.ToArray() : new int[0];
        }
        // ------------------------------------------------------------------
        // json 读取（容错：字段缺失/类型不符一律回落默认值，不抛异常）
        // ------------------------------------------------------------------

        private JArray LoadRows(string tableName)
        {
            string json;
            if (!ClientTableSource.TryGetJson(tableName, out json))
            {
                _rowCounts[tableName] = -1;   // 表不存在
                return null;
            }

            try
            {
                JArray rows = JArray.Parse(json);
                _rowCounts[tableName] = rows.Count;
                return rows;
            }
            catch (Exception ex)
            {
                _rowCounts[tableName] = -2;   // 解析失败
                Debug.LogWarning("[Client] 配置表 " + tableName + " 解析失败：" + ex.Message);
                return null;
            }
        }

        private static int ReadInt(JToken row, string name, int fallback)
        {
            if (row == null)
                return fallback;

            JToken token = row[name];
            if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Array || token.Type == JTokenType.Object)
                return fallback;

            try { return token.Value<int>(); }
            catch { return fallback; }
        }

        private static string ReadString(JToken row, string name)
        {
            if (row == null)
                return string.Empty;

            JToken token = row[name];
            if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Array || token.Type == JTokenType.Object)
                return string.Empty;

            try { return token.Value<string>() ?? string.Empty; }
            catch { return string.Empty; }
        }

        // ------------------------------------------------------------------
        // 诊断（可回收：删掉本方法即可，不影响功能）
        // ------------------------------------------------------------------

        private void Report()
        {
            Debug.Log("[配置表] 数据源：" + ClientTableSource.DescribeSource() +
                      "；Hero 可用 " + _heroes.Count + " / " + Describe("Hero") + "（按 ClientShow 过滤）；收藏品 " +
                      "头像=" + _collections[ClientCollectionKind.Head].Count + "(" + Describe("Head") + ")" +
                      " 头像框=" + _collections[ClientCollectionKind.HeadFrame].Count + "(" + Describe("HeadFrame") + ")" +
                      " 徽章=" + _collections[ClientCollectionKind.Badge].Count + "(" + Describe("Badge") + ")" +
                      " 铭牌=" + _collections[ClientCollectionKind.Nameplate].Count + "(" + Describe("Nameplate") + ")" +
                      " 称号=" + _collections[ClientCollectionKind.Title].Count + "(" + Describe("Title") + ")");

            Debug.Log("[配置表] 其他：道具 " + _items.Count + "(" + Describe("Item") + ")、技能 " + _skillById.Count +
                      "(" + Describe("Skill") + ")、天赋 " + _potencyById.Count + "(" + Describe("Potency") + ")、随机英雄池 " +
                      _randomHeroPool.Count + "(" + Describe("RandomHero") + ")");

            if (_rowCounts.ContainsKey("Hero") && _rowCounts["Hero"] > 0)
                return;

            Debug.LogWarning("[配置表] 英雄表为空，UI 将没有可显示的数据。请检查：" +
                             "① 本地 `Assets/Resources/Table/Hero.json` 是否存在；" +
                             "② 远端 `{OssUrl}/{ZoneId}/Table/Data/config.pkg` 是否已发布。" +
                             "（`ClientTableSource` 优先用落盘的 config.pkg，其次才是本地 json；" +
                             "磁盘上若留着旧 pkg，就会一直读到旧表 —— 见 BUG_TRACKER.md BUG-021。）");
        }

        private string Describe(string tableName)
        {
            int count;
            if (!_rowCounts.TryGetValue(tableName, out count))
                return "未加载";
            if (count == -1)
                return "表缺失";
            if (count == -2)
                return "解析失败";
            return "表内 " + count;
        }
    }
}
