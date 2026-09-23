using System;
using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;

namespace Pinball.Client.Stats
{
    /// <summary>
    /// <see cref="IClientStatsService"/> 的实现 —— **权威数值的客户端缓存 + 页签聚合**。
    ///
    /// <para><b>它做什么</b>：① 从 <see cref="IClientStatsGateway"/> 取服务器算好的数值；
    /// ② 从 <see cref="IClientConfigService"/> 取**结构与展示字段**（有哪些技能/天赋、名字、图标 id、描述模板）；
    /// ③ 按 <see cref="SectionRules"/> 聚合成 3 个页签；④ 交给 <see cref="IClientDescriptionFormatter"/> 渲染说明文本。</para>
    ///
    /// <para><b>它绝不做什么</b>：不算战力/加成/效果值/冷却/概率，不检查材料是否够升级 —— 那些都是服务器的活。
    /// 也不引用任何 Unity UI 类型（View 层反过来只吃数据）。</para>
    ///
    /// <para><b>GC 原则</b>：能力集合按英雄**建一次、原地刷新**；属性用复用列表；查询全部 `for` + 索引，
    /// 无 LINQ、无 `foreach`（接口枚举器会分配）、无闭包（网关事件用缓存委托）。</para>
    /// </summary>
    public sealed class ClientStatsService : IClientStatsService
    {
        /// <summary>
        /// ★ **唯一映射规则点**（2026-09-20 由数据推出，证据见 `Assets/Client/MODULE.md` §15.7）。
        /// 产品给了正式定义后只改这里。
        /// </summary>
        private static readonly ClientAbilitySectionRule[] SectionRules =
        {
            new ClientAbilitySectionRule(ClientHeroAbilitySection.Talent,          "天赋",   2, true),
            new ClientAbilitySectionRule(ClientHeroAbilitySection.SecretTechnique, "秘技",   3, false),
            new ClientAbilitySectionRule(ClientHeroAbilitySection.Ultimate,        "终结技", 4, false),
        };

        /// <summary>普攻的 `Type`（不进详情页，放在 <see cref="ClientHeroAbilitySet.BasicAttack"/>）。</summary>
        private const int BasicAttackSkillType = 1;

        private const int AttributeListCapacity = 16;

        private readonly IClientConfigService _config;
        private readonly IClientStatsGateway _gateway;
        private readonly IClientDescriptionFormatter _formatter;

        /// <summary>英雄 id → 能力集合（建一次、原地刷新）。</summary>
        private readonly Dictionary<int, ClientHeroAbilitySet> _abilitySets = new Dictionary<int, ClientHeroAbilitySet>();

        /// <summary>拥有者 id → 属性列表（复用，避免每次查询都分配）。</summary>
        private readonly Dictionary<int, List<ClientAttributeValue>> _attributes =
            new Dictionary<int, List<ClientAttributeValue>>();

        /// <summary>缓存的方法组委托 —— 订阅网关事件时不产生闭包（GC 原则）。</summary>
        private readonly Action<int> _onGatewayChanged;

        private bool _disposed;

        public event Action<int> AttributesChanged;
        public event Action<int> AbilitiesChanged;

        public ClientStatsService(IClientConfigService config, IClientStatsGateway gateway, IClientDescriptionFormatter formatter)
        {
            _config = config;
            _gateway = gateway;
            _formatter = formatter;
            _onGatewayChanged = OnGatewayChanged;

            if (_gateway != null)
                _gateway.Changed += _onGatewayChanged;
        }

        /// <summary>解除网关订阅（页面/容器销毁时调用；避免回调打到已销毁的对象）。</summary>
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            if (_gateway != null)
                _gateway.Changed -= _onGatewayChanged;

            _abilitySets.Clear();
            _attributes.Clear();
        }

        /// <summary>清空所有缓存（退出 Play / 域重载 / 配置表更新后调用）。</summary>
        public void Reset()
        {
            _abilitySets.Clear();
            _attributes.Clear();
            if (_formatter != null)
                _formatter.Clear();
        }

        // ------------------------------------------------------------------
        // 属性
        // ------------------------------------------------------------------

        public bool TryGetAttribute(int ownerId, int attributeId, out int value)
        {
            List<ClientAttributeValue> list = GetAttributeList(ownerId);
            for (int index = 0; index < list.Count; index++)
            {
                if (list[index].Id == attributeId)
                {
                    value = list[index].Value;
                    return true;
                }
            }

            value = 0;
            return false;
        }

        public IReadOnlyList<ClientAttributeValue> GetAttributes(int ownerId)
        {
            return GetAttributeList(ownerId);
        }

        private List<ClientAttributeValue> GetAttributeList(int ownerId)
        {
            List<ClientAttributeValue> list;
            if (!_attributes.TryGetValue(ownerId, out list))
            {
                list = new List<ClientAttributeValue>(AttributeListCapacity);
                _attributes[ownerId] = list;
            }

            // 网关负责"先清空再填"，因此这里不会累积、也不会产生新对象。
            if (_gateway != null)
                _gateway.CopyAttributes(ownerId, list);
            else
                list.Clear();

            return list;
        }

        // ------------------------------------------------------------------
        // 技能 / 天赋
        // ------------------------------------------------------------------

        public bool TryGetAbilityState(int abilityId, out ClientAbilityState state)
        {
            if (_gateway != null)
                return _gateway.TryGetAbilityState(abilityId, out state);

            state = default(ClientAbilityState);
            return false;
        }

        public ClientHeroAbilitySet GetHeroAbilities(int heroId)
        {
            ClientHeroAbilitySet set;
            if (!_abilitySets.TryGetValue(heroId, out set))
            {
                set = BuildAbilitySet(heroId);
                _abilitySets[heroId] = set;
            }

            ApplyStates(set);
            return set;
        }

        /// <summary>
        /// 建"结构"（有哪些技能/天赋、归哪个页签）—— **只做一次**。
        /// 数值状态由 <see cref="ApplyStates"/> 每次刷新写入。
        /// </summary>
        private ClientHeroAbilitySet BuildAbilitySet(int heroId)
        {
            ClientHeroAbilitySet set = new ClientHeroAbilitySet();
            set.HeroId = heroId;

            // 页签先按固定顺序建好（即使没有数据也保留 3 个 ⇒ 页面切页签不会空引用）。
            for (int index = 0; index < SectionRules.Length; index++)
            {
                ClientAbilitySection section = new ClientAbilitySection();
                section.Section = SectionRules[index].Section;
                section.DisplayName = SectionRules[index].DisplayName;
                set.Sections.Add(section);
            }

            if (_config == null || !_config.IsReady)
                return set;   // 表未就绪 ⇒ 3 个空页签，页面显示空态（不报错）

            AttachSkills(set, heroId);
            AttachPotencies(set, heroId);
            return set;
        }

        private void AttachSkills(ClientHeroAbilitySet set, int heroId)
        {
            IReadOnlyList<ClientSkillConfig> skills = _config.GetHeroSkills(heroId);
            for (int index = 0; index < skills.Count; index++)
            {
                ClientSkillConfig skill = skills[index];
                if (skill == null)
                    continue;

                ClientAbilityEntry entry = CreateSkillEntry(skill);

                if (skill.Type == BasicAttackSkillType)
                {
                    set.BasicAttack = entry;
                    continue;
                }

                ClientAbilitySection section = FindSectionBySkillType(set, skill.Type);
                // 同一个 Type 只取第一个（表里每个英雄每个 Type 恰好一个）
                if (section != null && section.Primary == null)
                    section.Primary = entry;
            }
        }

        private void AttachPotencies(ClientHeroAbilitySet set, int heroId)
        {
            IReadOnlyList<ClientPotencyConfig> potencies = _config.GetHeroPotencies(heroId);
            for (int index = 0; index < potencies.Count; index++)
            {
                ClientPotencyConfig potency = potencies[index];
                if (potency == null)
                    continue;

                ClientAbilitySection target = ResolvePotencySection(set, potency.Desc);
                if (target == null)
                    continue;   // 没有可挂的页签（连兜底页签都没有）⇒ 丢弃而不是报错

                target.Enhancements.Add(CreatePotencyEntry(potency));
            }
        }

        /// <summary>
        /// 被动归属（**弱关联，唯一的替换点**）：
        /// ① 若某页签的主技能名出现在被动描述里 ⇒ 归该页签（例：`300110 秘技强化` 描述含"月华追踪" ⇒ 秘技页）；
        /// ② 都没命中 ⇒ 归 <see cref="ClientAbilitySectionRule.CatchUnmatchedPotency"/> 的页签（当前是天赋页）；
        /// ③ 仍没有 ⇒ 归第一个页签；一个页签都没有 ⇒ 返回 null（调用方丢弃）。
        /// </summary>
        private static ClientAbilitySection ResolvePotencySection(ClientHeroAbilitySet set, string potencyDescription)
        {
            if (!string.IsNullOrEmpty(potencyDescription))
            {
                for (int index = 0; index < set.Sections.Count; index++)
                {
                    ClientAbilitySection section = set.Sections[index];
                    if (section == null || section.Primary == null || string.IsNullOrEmpty(section.Primary.Name))
                        continue;

                    // IndexOf(..., Ordinal) 不产生任何分配
                    if (potencyDescription.IndexOf(section.Primary.Name, StringComparison.Ordinal) >= 0)
                        return section;
                }
            }

            for (int index = 0; index < SectionRules.Length; index++)
            {
                if (!SectionRules[index].CatchUnmatchedPotency)
                    continue;

                ClientAbilitySection fallback = set.GetSection(SectionRules[index].Section);
                if (fallback != null)
                    return fallback;
            }

            return set.Sections.Count > 0 ? set.Sections[0] : null;
        }

        private static ClientAbilitySection FindSectionBySkillType(ClientHeroAbilitySet set, int skillType)
        {
            for (int index = 0; index < SectionRules.Length; index++)
            {
                if (SectionRules[index].SkillType != skillType)
                    continue;
                return set.GetSection(SectionRules[index].Section);
            }
            return null;
        }

        /// <summary>
        /// 原地刷新**数值状态 + 说明文本**（不产生新对象）。
        /// 数值全部来自网关（服务器）；文本由 <see cref="IClientDescriptionFormatter"/> 按当前等级渲染。
        /// </summary>
        private void ApplyStates(ClientHeroAbilitySet set)
        {
            if (set == null)
                return;

            ApplyState(set.BasicAttack);

            for (int index = 0; index < set.Sections.Count; index++)
            {
                ClientAbilitySection section = set.Sections[index];
                if (section == null)
                    continue;

                ApplyState(section.Primary);

                List<ClientAbilityEntry> enhancements = section.Enhancements;
                for (int i = 0; i < enhancements.Count; i++)
                    ApplyState(enhancements[i]);
            }
        }

        private void ApplyState(ClientAbilityEntry entry)
        {
            if (entry == null)
                return;

            ClientAbilityState state;
            if (_gateway != null && _gateway.TryGetAbilityState(entry.Id, out state))
                entry.ApplyState(state);

            // 说明文本：等级没变就复用上次结果（formatter 内部还有 (id, level) 缓存，两层都省）
            if (entry.FormattedLevel != entry.Level || entry.DisplayDescription.Length == 0)
            {
                entry.DisplayDescription = _formatter != null
                    ? _formatter.Format(entry.Id, entry.Description, entry.EffectLevels, entry.Level)
                    : entry.Description;
                entry.FormattedLevel = entry.Level;
            }
        }

        // ------------------------------------------------------------------
        // 构建（结构）
        // ------------------------------------------------------------------

        private static ClientAbilityEntry CreateSkillEntry(ClientSkillConfig skill)
        {
            ClientAbilityEntry entry = new ClientAbilityEntry();
            entry.Kind = ClientAbilityKind.Skill;
            entry.Id = skill.Id;
            entry.Name = skill.Name ?? string.Empty;
            entry.Description = skill.Desc ?? string.Empty;
            entry.IconId = skill.SkillIcon;
            entry.SkillType = skill.Type;
            entry.EffectLevels = CopyToArray(skill.EffectLevel);
            return entry;
        }

        private static ClientAbilityEntry CreatePotencyEntry(ClientPotencyConfig potency)
        {
            ClientAbilityEntry entry = new ClientAbilityEntry();
            entry.Kind = ClientAbilityKind.Potency;
            entry.Id = potency.Id;
            entry.Name = potency.Name ?? string.Empty;
            entry.Description = potency.Desc ?? string.Empty;
            entry.IconId = potency.SkillIcon;
            entry.EffectType = potency.EffectType;
            entry.Quality = potency.Quality;
            entry.EffectParameters = CopyToArray(potency.EffectParameters);
            return entry;
        }

        /// <summary>配置层的 `List&lt;int&gt;` → `int[]`（只在建结构时发生一次，之后刷新零分配）。</summary>
        private static int[] CopyToArray(List<int> source)
        {
            if (source == null || source.Count == 0)
                return new int[0];

            int[] result = new int[source.Count];
            for (int index = 0; index < source.Count; index++)
                result[index] = source[index];
            return result;
        }

        // ------------------------------------------------------------------
        // 变更通知
        // ------------------------------------------------------------------

        /// <summary>
        /// 网关报告变化 —— 转发给订阅者。事件只带一个 id（网关不区分属性/能力），
        /// 两个事件都会发；**订阅方的刷新必须是幂等的**（重复刷新不得有副作用）。
        /// </summary>
        private void OnGatewayChanged(int id)
        {
            Action<int> attributesHandler = AttributesChanged;
            if (attributesHandler != null)
                attributesHandler(id);

            Action<int> abilitiesHandler = AbilitiesChanged;
            if (abilitiesHandler != null)
                abilitiesHandler(id);
        }
    }
}
