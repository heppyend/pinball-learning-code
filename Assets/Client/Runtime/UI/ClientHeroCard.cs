using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pinball.Client.UI
{
    /// <summary>
    /// 单张英雄卡牌的显示与交互 —— **挂在卡牌模板上**。
    ///
    /// 设计意图（负责人 2026-09-20 要求"把英雄卡牌单独写几个脚本来管理"）：
    /// 这是"英雄卡牌"的**唯一管理者** —— 名称 / 等级 / 立绘 / 锁 / 遮罩 / 编队编号 / 点击，
    /// 全部只在这里实现一次。英雄页、图鉴页共用，未来编队、背包等列表也可直接复用。
    ///
    /// 刻意**不依赖具体数据模型**：`Apply` 只收基础类型（string/int/bool/Sprite/Action），
    /// 由调用方负责把领域对象映射过来。这样本组件与 `ClientHero` 解耦，
    /// 任何"有名字、有等级、有解锁状态"的列表都能用它。
    ///
    /// 关键约束（负责人明确）：
    /// - `_maskRoot`（Mark 遮罩）**必须与 `_lockRoot`（锁Image）同时出现/消失** —— 两者都表示"未解锁"。
    /// - `_teamBadge`（编队队内编号Image）**只由 FormationPage 显式打开**，其它场合一律隐藏。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ClientHeroCard : MonoBehaviour
    {
        [Header("显示")]
        [Tooltip("立绘 Image")]
        [SerializeField] private Image _illustration;

        [Tooltip("名称文本（TMP）。原模板没有此节点，由结构修复工具新建于 卡牌名称Image 之下。")]
        [SerializeField] private TMP_Text _nameText;

        [Tooltip("等级文本（TMP），对应模板里的 等级-Text")]
        [SerializeField] private TMP_Text _levelText;

        [Tooltip("与等级同时显隐的装饰节点（如 Lv-Image）；level<=0 时隐藏")]
        [SerializeField] private GameObject[] _levelDecorations;

        [Header("解锁状态")]
        [Tooltip("锁 Image，未解锁时显示")]
        [SerializeField] private GameObject _lockRoot;

        [Tooltip("未解锁遮罩 Mark，**必须与锁同时显隐**")]
        [SerializeField] private GameObject _maskRoot;

        [Header("编队专用")]
        [Tooltip("编队队内编号 Image，仅在 FormationPage编队 显示，其它场合一律隐藏")]
        [SerializeField] private GameObject _teamBadge;

        [Tooltip("队内编号的数字节点（编队队内编号Image/编号数字）。" +
                 "实测该节点是 Image 而非文本（见 Logs/hierarchy-audit.txt 第九节）；留空则运行时按名字自动查找。")]
        [SerializeField] private Image _teamBadgeNumber;

        [Header("交互")]
        [SerializeField] private Button _button;

        /// <summary>卡牌自带的 Button（模板根上已有）。</summary>
        public Button Button
        {
            get
            {
                if (_button == null)
                    _button = GetComponent<Button>();
                return _button;
            }
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();
        }

        /// <summary>
        /// 应用一张卡牌的显示与点击。
        /// </summary>
        /// <param name="displayName">卡面名称</param>
        /// <param name="level">等级；&lt;=0 表示不显示等级</param>
        /// <param name="unlocked">是否已解锁；未解锁时显示锁 + 遮罩</param>
        /// <param name="illustration">立绘；为 null 时不改动（保持模板图）</param>
        /// <param name="onClick">点击回调</param>
        public void Apply(string displayName, int level, bool unlocked, Sprite illustration, Action onClick)
        {
            if (_nameText != null)
                _nameText.text = displayName;

            if (_illustration != null && illustration != null)
                _illustration.sprite = illustration;

            bool showLevel = level > 0;
            if (_levelText != null)
            {
                _levelText.text = "LV." + level;
                SetActive(_levelText.gameObject, showLevel);
            }
            SetAll(_levelDecorations, showLevel);

            // 锁与遮罩必须同步 —— 二者都是"未解锁"的表达，不允许只出现一个。
            bool locked = !unlocked;
            SetActive(_lockRoot, locked);
            SetActive(_maskRoot, locked);

            // 编队编号默认隐藏；只有 FormationPage 才通过 SetTeamBadgeVisible(true) 打开。
            SetActive(_teamBadge, false);

            if (Button != null)
            {
                Button.onClick.RemoveAllListeners();
                if (onClick != null)
                    Button.onClick.AddListener(() => onClick());
            }
        }

        /// <summary>仅供 FormationPage编队 调用：显示/隐藏队内编号。</summary>
        public void SetTeamBadgeVisible(bool visible)
        {
            SetActive(_teamBadge, visible);
        }

        /// <summary>
        /// 显示/隐藏**卡面全部子节点**（只切子节点，**不动本节点**）。
        ///
        /// 用途：编队的**空槽位**要"保留格位但不显示卡牌"（负责人 2026-09-20）。
        /// 为什么不动本节点：槽位容器带 `HorizontalLayoutGroup`，隐藏本节点会让其余卡**重排**；
        /// 只切子节点则格位保留、卡面为空。
        ///
        /// 切回 `true` 后请照常调用 <see cref="Apply"/> —— 锁 / 遮罩 / 等级装饰 / 队内编号
        /// 都会由 `Apply` 重新落到正确状态。
        /// </summary>
        public void SetContentVisible(bool visible)
        {
            for (int index = 0; index < transform.childCount; index++)
                transform.GetChild(index).gameObject.SetActive(visible);
        }

        /// <summary>
        /// 仅供 `FormationPage编队` 调用：按**槽位号**（1..3）显示队内编号，`0` = 隐藏。
        ///
        /// ⚠️ 编号内容（`编队队内编号Image/编号数字`）实测是 **Image 而不是文本**
        /// （`Logs/hierarchy-audit.txt` 第九节卡牌子树取证：`编号数字 … 组件: CanvasRenderer Image`），
        /// 所以**写不了数字文本**，只能换 sprite。数字贴图来源属资源契约，**待确认**；
        /// 传 <c>null</c> 时只做显隐、不改图。
        /// </summary>
        public void SetTeamSlot(int slotNumber, Sprite numberSprite = null)
        {
            bool visible = slotNumber > 0;
            SetActive(_teamBadge, visible);
            if (!visible)
                return;

            if (_teamBadgeNumber == null && _teamBadge != null)
            {
                Transform numberNode = FindDeepChild(_teamBadge.transform, TeamBadgeNumberNodeName);
                if (numberNode != null)
                    _teamBadgeNumber = numberNode.GetComponent<Image>();
            }
            if (_teamBadgeNumber != null && numberSprite != null)
                _teamBadgeNumber.sprite = numberSprite;
        }

        private const string TeamBadgeNumberNodeName = "编号数字";

        private static Transform FindDeepChild(Transform root, string nodeName)
        {
            if (root == null)
                return null;
            for (int index = 0; index < root.childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (child.name == nodeName)
                    return child;
                Transform found = FindDeepChild(child, nodeName);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
                target.SetActive(active);
        }

        private static void SetAll(GameObject[] targets, bool active)
        {
            if (targets == null)
                return;
            for (int i = 0; i < targets.Length; i++)
                SetActive(targets[i], active);
        }
    }
}
