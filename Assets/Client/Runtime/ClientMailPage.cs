using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client
{
    /// <summary>开发期邮箱：本地邮件、已读状态和附件领取，不包含真实邮件协议。</summary>
    public sealed class ClientMailPage : ClientPageViewBase, IClientFeedbackHost
    {
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private Text[] _mailLabels;
        [SerializeField] private Text _detail;
        [SerializeField] private Text _status;
        [SerializeField] private GameObject _emptyHint;
        private readonly List<ClientMail> _mails = new List<ClientMail>();
        private int _selectedIndex;
        private bool _unreadOnly;

        private IClientFeedback _feedback;

        protected override GameObject PageRoot { get { return _pageRoot; } }
        protected override void OnPageRefresh() { Show(); }

        /// <summary>由组合根注入；取代原先的 ClientUiFeedback 静态单例。</summary>
        public void BindFeedback(IClientFeedback feedback) { _feedback = feedback; }

        private void Toast(string message)
        {
            if (_feedback != null)
                _feedback.ShowToast(message);
            else
                Debug.Log("[Client] " + message);
        }

        public void Configure(GameObject pageRoot, Text[] mailLabels, Text detail, Text status, GameObject emptyHint)
        {
            _pageRoot = pageRoot;
            _mailLabels = mailLabels;
            _detail = detail;
            _status = status;
            _emptyHint = emptyHint;
        }

        // ------------------------------------------------------------------
        // 邮件卡 ↔ 邮件数据 的显式映射
        //
        // 背景（2026-09-20 层级普查）：
        //   场景 Email come/邮件Scroll View/Viewport/Content 下有 **6 张** 邮件prefab，
        //   而 _mails 只有 **2** 条（LocalClientDataService._mails）。
        //   旧实现的 _mailLabels 只是一个 2 槽数组，在场景里还全是空的，
        //   于是既写不进邮件名，也管不了另外 4 张卡的显隐。
        // 现在改为按容器逐张收集、按下标映射、超出部分隐藏——与商场卡牌同一套做法。
        // ------------------------------------------------------------------

        /// <summary>按层级顺序缓存的邮件卡节点（**回退路径**：未配置模板时使用）。</summary>
        private readonly List<GameObject> _mailCards = new List<GameObject>();
        private bool _cardsResolved;

        [Header("列表模板（MODULE.md §7：1 个模板 + 运行时生成）")]
        [Tooltip("邮件卡模板：场景中保留 1 个并禁用，运行时按邮件数克隆。" +
                 "未指定时回退为直接使用场景内的邮件卡节点。")]
        [SerializeField] private GameObject _cardTemplate;

        [Tooltip("邮件卡容器：Email come/邮件Scroll View/Viewport/Content。" +
                 "该容器自带 GridLayoutGroup（实测步长 -308.62），克隆体会被自动排布。")]
        [SerializeField] private RectTransform _cardListRoot;

        /// <summary>运行时按数据生成的邮件卡实例。</summary>
        private readonly List<GameObject> _cardInstances = new List<GameObject>();

        private bool UseCardTemplate { get { return _cardTemplate != null && _cardListRoot != null; } }

        /// <summary>路径与场景一致：Email come / 邮件Scroll View / Viewport / Content。</summary>
        private Transform ResolveCardContainer()
        {
            if (_pageRoot == null)
                return null;

            Transform current = _pageRoot.transform;
            string[] path = { "Email come", "邮件Scroll View", "Viewport", "Content" };
            for (int i = 0; i < path.Length; i++)
            {
                if (current == null)
                    return null;
                current = current.Find(path[i]);
            }
            return current;
        }

        /// <summary>收集邮件卡。找不到容器时明确告警，而不是静默什么都不做。</summary>
        private void ResolveCards()
        {
            _cardsResolved = true;
            _mailCards.Clear();

            // 已配置模板时走"运行时生成"，不再收集场景内的内联邮件卡。
            if (UseCardTemplate)
            {
                Debug.Log("[Client] 邮件卡走模板生成模式（模板=" + _cardTemplate.name +
                          "，容器=" + _cardListRoot.name + "）。");
                return;
            }

            Transform container = ResolveCardContainer();
            if (container == null)
            {
                Debug.LogWarning("[Client] 未找到邮件列表容器 Email come/邮件Scroll View/Viewport/Content，" +
                                 "邮件列表将不会刷新。");
                return;
            }

            for (int i = 0; i < container.childCount; i++)
            {
                Transform card = container.GetChild(i);
                if (card.name.StartsWith("邮件prefab", System.StringComparison.Ordinal))
                    _mailCards.Add(card.gameObject);
            }
            Debug.Log("[Client] 邮件卡已收集 " + _mailCards.Count + " 张。");
        }

        /// <summary>补齐未在 Inspector 指定的引用（不新增任何节点）。</summary>
        private void ResolveMissingReferences()
        {
            if (_pageRoot == null)
                _pageRoot = gameObject;
            if (!_cardsResolved)
                ResolveCards();
            if (_emptyHint == null)
            {
                Transform empty = _pageRoot.transform.Find("Default");
                if (empty != null)
                    _emptyHint = empty.gameObject;
            }
        }

        /// <summary>
        /// 按数据条数控制邮件卡显隐并写入邮件名。
        /// 只改 activeSelf 与文本，不动任何 anchor / sizeDelta / anchoredPosition。
        /// </summary>
        private void ApplyMailCards()
        {
            if (UseCardTemplate)
            {
                ApplyMailCardsFromTemplate();
                return;
            }

            if (_mailCards.Count == 0)
                return;

            if (_mailCards.Count != _mails.Count)
                Debug.Log("[Client] 邮件卡数(" + _mailCards.Count + ") 与邮件数(" + _mails.Count +
                          ") 不一致：超出部分隐藏。建议按 MODULE.md 约定改为「1 个模板 + 运行时生成」。");

            for (int i = 0; i < _mailCards.Count; i++)
            {
                bool exists = i < _mails.Count;
                if (_mailCards[i].activeSelf != exists)
                    _mailCards[i].SetActive(exists);
                if (!exists)
                    continue;

                ClientMail mail = _mails[i];
                SetCardText(_mailCards[i], (mail.IsRead ? "" : "● ") + mail.Title +
                                           (mail.IsClaimed ? "\n已领取" : "\n附件待领取"));
            }
        }

        /// <summary>
        /// 1 个模板 + 运行时生成（MODULE.md §7）。
        ///
        /// 容器 `Email come/邮件Scroll View/Viewport/Content` 自带
        /// `GridLayoutGroup`（实测 6 张卡的 y 步长恒为 -308.62 = cell 283 + spacing 25.62），
        /// 邮件卡原本就是布局组件自动排布的，因此本方式**不改变既有美术与布局尺寸**。
        ///
        /// 模板自身始终禁用，只作克隆源；未被数据用到的实例保持隐藏而非销毁，便于数据增多时复用。
        /// </summary>
        private void ApplyMailCardsFromTemplate()
        {
            _cardTemplate.SetActive(false);

            while (_cardInstances.Count < _mails.Count)
            {
                GameObject clone = Instantiate(_cardTemplate, _cardListRoot);
                clone.name = "邮件Item";
                clone.SetActive(false);

                // 场景中的邮件卡此前没有任何点击绑定（旧 Editor 工具已废弃），
                // 这里为每个克隆体绑上 SelectMail(index)，点击才真正能选邮件。
                int index = _cardInstances.Count;
                Button button = clone.GetComponent<Button>();
                if (button == null)
                    button = clone.GetComponentInChildren<Button>(true);
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => SelectMail(index));
                }
                else
                {
                    Debug.LogWarning("[Client] 邮件卡模板上没有 Button，克隆体点击无效（模板：" +
                                     _cardTemplate.name + "）。");
                }
                _cardInstances.Add(clone);
            }

            if (_cardInstances.Count != _mails.Count)
                Debug.Log("[Client] 邮件卡实例(" + _cardInstances.Count + ") 与邮件数(" + _mails.Count +
                          ") 不一致：超出部分隐藏。");

            for (int i = 0; i < _cardInstances.Count; i++)
            {
                bool exists = i < _mails.Count;
                if (_cardInstances[i].activeSelf != exists)
                    _cardInstances[i].SetActive(exists);
                if (!exists)
                    continue;

                ClientMail mail = _mails[i];
                SetCardText(_cardInstances[i], (mail.IsRead ? "" : "● ") + mail.Title +
                                               (mail.IsClaimed ? "\n已领取" : "\n附件待领取"));
            }
        }

        /// <summary>
        /// 写入邮件名。同时兼容 TMP 与旧版 UGUI 文本——按场景实际组件类型取用，
        /// 而不是用一个可能有误的序列化字段类型去要求绑定。
        /// </summary>
        private static void SetCardText(GameObject card, string value)
        {
            Transform[] all = card.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name != "邮件名称")
                    continue;

                TMPro.TMP_Text tmp = all[i].GetComponent<TMPro.TMP_Text>();
                if (tmp != null)
                {
                    tmp.text = value;
                    return;
                }
                Text legacy = all[i].GetComponent<Text>();
                if (legacy != null)
                    legacy.text = value;
                return;
            }
        }

        public void Show()
        {
            // 2026-09-21 UI 适配：竖向列表撑高 / Clamped / 顶部对齐。
            ClientScrollFix.FixAll(gameObject, true);
            _pageRoot.SetActive(true);
            BindPageActions();
            Refresh();
        }

        private bool _actionsBound;

        /// <summary>
        /// 按名绑定页面按钮（首次显示时执行一次）。
        ///
        /// <para>2026-09-21（死按钮审计）：`All read button`（一键领取附件）**此前没有任何绑定** ——
        /// 处理函数 `ClaimAll()` 早就存在，只是没人接上 ⇒ 点了没反应。</para>
        ///
        /// <para>⚠️ 同一区域的 `All delete`（全部删除）**没有对应的数据接口**
        /// （`IClientDataService` 只有 `ClaimAllMails`，没有任何删除/批量已读接口）⇒ 属**未实现功能**，
        /// 这里**不臆造行为**，保留为未绑定按钮并记录在 `BUG_TRACKER.md`。</para>
        /// </summary>
        private void BindPageActions()
        {
            if (_actionsBound || _pageRoot == null)
                return;
            _actionsBound = true;

            BindButton("All read button", ClaimAll);

            // 2026-09-21（负责人反馈）：`All delete`（全部删除）此前没有实现。
            // 场景里的删除键挂在 `Panel/All delete/Button` 下 —— **按钮节点本身叫 `Button`**（名字太通用），
            // 所以按**父节点 `All delete`** 查找再取其下的 Button，避免误绑到别的 `Button` 上。
            BindButtonUnder("All delete", DeleteAll);
        }

        /// <summary>
        /// **全部删除**（2026-09-21 负责人确认的规则）：**全部领取 + 全部已读后**才允许；
        /// 删除成功后列表清空，背景显示"暂无邮件"（由 <see cref="Refresh"/> 与 `Default/No emails` 负责）。
        /// 不满足前置条件时给出原因（如"还有附件未领取，请先全部领取"）。
        /// </summary>
        public void DeleteAll()
        {
            string failureReason;
            bool deleted = ClientServices.Data.TryDeleteAllMails(out failureReason);
            string message = deleted ? "已删除全部邮件" : failureReason;
            SetText(_status, message);
            Toast(message);
            Refresh();
        }

        /// <summary>在名为 <paramref name="parentName"/> 的节点（含其后代）里找 Button 并绑定。</summary>
        private void BindButtonUnder(string parentName, UnityEngine.Events.UnityAction action)
        {
            Transform parent = FindDeep(_pageRoot.transform, parentName);
            if (parent == null)
            {
                Debug.LogWarning("[Client] 邮箱页找不到「" + parentName + "」节点，未绑定：" + action.Method.Name);
                return;
            }

            Button button = parent.GetComponent<Button>();
            if (button == null)
                button = parent.GetComponentInChildren<Button>(true);
            if (button == null)
            {
                Debug.LogWarning("[Client] 邮箱页「" + parentName + "」下没有 Button，未绑定：" + action.Method.Name);
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
            Debug.Log("[Client] 邮箱页：「" + parentName + "」下的按钮已绑定 " + action.Method.Name + "。");
        }

        private void BindButton(string name, UnityEngine.Events.UnityAction action)
        {
            Transform target = FindDeep(_pageRoot.transform, name);
            Button button = target != null ? target.GetComponent<Button>() : null;
            if (button == null)
            {
                Debug.LogWarning("[Client] 邮箱页找不到按钮「" + name + "」，未绑定：" + action.Method.Name);
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
                return null;
            if (root.name == name)
                return root;

            for (int index = 0; index < root.childCount; index++)
            {
                Transform found = FindDeep(root.GetChild(index), name);
                if (found != null)
                    return found;
            }
            return null;
        }

        public void SelectMail(int index)
        {
            if (index < 0 || index >= _mails.Count)
                return;
            _selectedIndex = index;
            ClientMail mail = _mails[index];
            ClientServices.Data.TryReadMail(mail.MailId);
            SetText(_detail, mail.Title + "\n\n" + mail.Content + "\n\n附件：养成材料 × " + mail.AttachmentQuantity);
            SetText(_status, mail.IsClaimed ? "附件已领取" : "点击领取附件");
            RefreshLabels();
        }

        public void ClaimSelected()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _mails.Count)
            {
                SetText(_status, "请选择邮件");
                return;
            }
            string failureReason;
            if (!ClientServices.Data.TryClaimMail(_mails[_selectedIndex].MailId, out failureReason))
            {
                SetText(_status, failureReason);
                return;
            }
            SetText(_status, "附件已加入本地背包");
            Refresh();
            SelectMail(_selectedIndex);
        }

        public void ClaimAll()
        {
            string failureReason;
            int count = ClientServices.Data.ClaimAllMails(out failureReason);
            SetText(_status, count > 0 ? "已批量领取 " + count + " 封邮件附件" : failureReason);
            Toast(_status != null ? _status.text : "批量领取完成");
            Refresh();
        }

        public void ShowAll() { _unreadOnly = false; Refresh(); }
        public void ShowUnread() { _unreadOnly = true; Refresh(); }

        private void Refresh()
        {
            ResolveMissingReferences();
            _mails.Clear();
            foreach (ClientMail mail in ClientServices.Data.GetMails())
                if (!_unreadOnly || !mail.IsRead)
                    _mails.Add(mail);
            if (_emptyHint != null)
                _emptyHint.SetActive(_mails.Count == 0);
            RefreshLabels();
            if (_mails.Count == 0)
            {
                SetText(_detail, "暂无邮件");
                SetText(_status, "本地邮件列表为空");
                return;
            }
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _mails.Count - 1);
            SelectMail(_selectedIndex);
        }

        /// <summary>
        /// 安全写文本。
        ///
        /// 本页的 _detail / _status / _emptyHint / _mailLabels 在场景里**尚未绑定**
        /// （邮件详情逻辑本身也还没实现，负责人已确认），因此不能再无条件解引用：
        /// 否则每次 Refresh 都会抛 UnassignedReferenceException / NullReferenceException，
        /// 并把后续的列表刷新一并打断。
        /// </summary>
        private static void SetText(Text target, string value)
        {
            if (target != null)
                target.text = value;
        }

        private void RefreshLabels()
        {
            // 旧实现依赖 2 槽的 _mailLabels 数组（场景中全为空），既写不进邮件名也管不了显隐。
            // 现改为按容器逐张收集 + 按下标映射 + 超出隐藏。
            ResolveMissingReferences();
            ApplyMailCards();
        }
    }
}
