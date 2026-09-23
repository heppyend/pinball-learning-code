using System;
using System.Collections.Generic;
using Pinball.Client.Domain;
using Pinball.Client.Services;
using Pinball.Client.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pinball.Client
{
    public interface IClientActivitySpriteResolver
    {
        Sprite Resolve(int imageId);
    }

    public enum ClientActivitySubPage
    {
        Newbie = 0,
        Advanced = 1,
        Seasonal = 2,
    }

    /// <summary>绑定负责人已重构的 ActivityPage活动；不生成、删除或调整 GUI 节点。</summary>
    public sealed class ClientActivityPage : ClientPageViewBase
    {
        private const string ExpectedRootName = "ActivityPage活动";

        private sealed class TextBinding
        {
            public TMP_Text Tmp;
            public Text Legacy;

            public string Value
            {
                set
                {
                    if (Tmp != null) Tmp.text = value ?? string.Empty;
                    if (Legacy != null) Legacy.text = value ?? string.Empty;
                }
            }

            public void BringToFront()
            {
                if (Tmp != null) Tmp.transform.SetAsLastSibling();
                else if (Legacy != null) Legacy.transform.SetAsLastSibling();
            }
        }

        private sealed class RewardBinding
        {
            public GameObject Root;
            public Image CardImage;
            public TextBinding Name;
            public TextBinding Quantity;
        }

        private sealed class TaskBinding
        {
            public GameObject Root;
            public TextBinding Description;
            public TextBinding ProgressLeft;
            public TextBinding ProgressMiddle;
            public TextBinding ProgressRight;
            public RectTransform ProgressFill;
            public RectTransform ProgressBase;
            public Image ClaimButtonImage;
            public Image ClaimLabelImage;
            public Button ClaimButton;
            public Animator ClaimAnimator;
            public Sprite PendingButtonSprite;
            public Sprite PendingLabelSprite;
            public Sprite ClaimableButtonSprite;
            public Sprite ClaimableLabelSprite;
            public GameObject ClaimCanvas;
            public GameObject ClaimedIcon;
            public readonly List<RewardBinding> Rewards = new List<RewardBinding>();
            public float FullProgressWidth;
            public ClientActivityTaskData Data;
        }

        private sealed class SeasonalBinding
        {
            public GameObject Root;
            public TextBinding Title;
            public TextBinding Date;
            public TextBinding PopularText;
            public Image Banner;
            public Image HotSpot;
            public Image Popular;
            public ClientActivity Data;
        }

        public event Action<ClientActivitySubPage> SubPageChanged;
        public event Action<ClientActivityTaskData> ClaimStarted;
        public event Action<ClientActivityTaskData, ClientActivityTaskClaimResult> ClaimFinished;
        public event Action<ClientActivityTaskData, string> ClaimFailed;
        public event Action<string> TipRequested;
        public event Action<ClientActivity> SeasonalActivityRequested;

        [SerializeField] private GameObject _pageRoot;

        private readonly Dictionary<int, Sprite> _registeredSprites = new Dictionary<int, Sprite>();
        private readonly List<TaskBinding> _newbieBindings = new List<TaskBinding>();
        private readonly List<TaskBinding> _advancedBindings = new List<TaskBinding>();
        private readonly List<SeasonalBinding> _seasonalBindings = new List<SeasonalBinding>();

        private IClientActivitySpriteResolver _spriteResolver;
        private ClientActivitySubPage _currentSubPage = ClientActivitySubPage.Newbie;
        private GameObject _newbiePage;
        private GameObject _advancedPage;
        private GameObject _seasonalPage;
        private Button _backButton;
        private Button _newbieButton;
        private Button _advancedButton;
        private Button _seasonalButton;
        private Button _seasonalBackButton;
        private TextBinding _taskTitle;
        private bool _isBound;
        private bool _dataSubscribed;

        protected override GameObject PageRoot { get { return _pageRoot; } }
        protected override void OnPageRefresh() { Show(); }
        protected override void OnPageHidden() { Hide(); }

        private void Awake()
        {
            if (gameObject.name != ExpectedRootName)
            {
                enabled = false;
                return;
            }
            EnsureBound();
        }

        private void OnEnable()
        {
            if (gameObject.name != ExpectedRootName)
                return;
            EnsureBound();
            SubscribeData();
            SetTaskTitle("任务");
            SelectSubPage(ClientActivitySubPage.Newbie);
        }

        private void OnDisable() { UnsubscribeData(); }

        private void Start()
        {
            if (gameObject.name != ExpectedRootName || !gameObject.activeInHierarchy)
                return;
            SubscribeData();
            RefreshVisiblePage();
            RestoreTabSelection();
        }

        public void Show()
        {
            // 2026-09-21（负责人决定：其余列表页也接入）：竖向列表撑高 / Clamped / 顶部对齐。
            ClientScrollFix.FixAll(gameObject, true);
            if (_pageRoot == null) _pageRoot = gameObject;
            _pageRoot.SetActive(true);
            EnsureBound();
            SubscribeData();
            SetTaskTitle("任务");
            ShowNewbieTasks();
        }

        public void Hide()
        {
            if (_pageRoot != null) _pageRoot.SetActive(false);
        }

        public void SetTaskTitle(string value)
        {
            EnsureBound();
            if (_taskTitle != null) _taskTitle.Value = value;
        }

        public void ShowNewbieTasks() { SelectSubPage(ClientActivitySubPage.Newbie); }
        public void ShowAdvancedTasks() { SelectSubPage(ClientActivitySubPage.Advanced); }
        public void ShowSeasonalTasks() { SelectSubPage(ClientActivitySubPage.Seasonal); }

        public void SelectSubPage(ClientActivitySubPage subPage)
        {
            EnsureBound();
            _currentSubPage = subPage;
            if (_newbiePage != null) _newbiePage.SetActive(subPage == ClientActivitySubPage.Newbie);
            if (_advancedPage != null) _advancedPage.SetActive(subPage == ClientActivitySubPage.Advanced);
            if (_seasonalPage != null) _seasonalPage.SetActive(subPage == ClientActivitySubPage.Seasonal);
            RefreshVisiblePage();
            RestoreTabSelection();
            SubPageChanged?.Invoke(subPage);
        }

        public void SetSpriteResolver(IClientActivitySpriteResolver resolver)
        {
            _spriteResolver = resolver;
            RefreshVisiblePage();
        }

        public void RegisterSprite(int imageId, Sprite sprite)
        {
            if (imageId > 0 && sprite != null) _registeredSprites[imageId] = sprite;
        }

        public void SetTaskData(ClientActivityTaskCategory category, int cardIndex, ClientActivityTaskData data)
        {
            List<TaskBinding> bindings = GetTaskBindings(category);
            if (cardIndex >= 0 && cardIndex < bindings.Count) ApplyTask(bindings[cardIndex], data, category);
        }

        public void SetTaskDescription(ClientActivityTaskCategory category, int cardIndex, string value)
        {
            TaskBinding binding = GetTaskBinding(category, cardIndex);
            if (binding != null && binding.Description != null) binding.Description.Value = value;
        }

        public void SetTaskProgress(ClientActivityTaskCategory category, int cardIndex, int progress, int target)
        {
            TaskBinding binding = GetTaskBinding(category, cardIndex);
            if (binding != null) ApplyProgress(binding, progress, target);
        }

        public void SetRewardCard(ClientActivityTaskCategory category, int taskIndex, int rewardIndex, ClientActivityTaskReward reward)
        {
            TaskBinding task = GetTaskBinding(category, taskIndex);
            if (task != null && rewardIndex >= 0 && rewardIndex < task.Rewards.Count)
                ApplyReward(task.Rewards[rewardIndex], reward);
        }

        public void SetTaskClaimImages(
            ClientActivityTaskCategory category, int taskIndex,
            int pendingLabelImageId, int claimLabelImageId, int claimableButtonImageId)
        {
            TaskBinding binding = GetTaskBinding(category, taskIndex);
            if (binding == null || binding.Data == null)
                return;
            binding.Data.PendingLabelImageId = pendingLabelImageId;
            binding.Data.ClaimLabelImageId = claimLabelImageId;
            binding.Data.ClaimableButtonImageId = claimableButtonImageId;
            ApplyTask(binding, binding.Data, category);
        }

        public void SetSeasonalCard(int index, ClientActivity data)
        {
            if (index >= 0 && index < _seasonalBindings.Count) ApplySeasonal(_seasonalBindings[index], data);
        }

        public void SetSeasonalText(int index, string title, string dateText, string popularText)
        {
            if (index < 0 || index >= _seasonalBindings.Count) return;
            SeasonalBinding binding = _seasonalBindings[index];
            if (binding.Title != null) binding.Title.Value = title;
            if (binding.Date != null) binding.Date.Value = dateText;
            if (binding.PopularText != null) binding.PopularText.Value = popularText;
        }

        public void SetSeasonalHotSpotVisible(int index, bool visible)
        {
            if (index >= 0 && index < _seasonalBindings.Count && _seasonalBindings[index].HotSpot != null)
                _seasonalBindings[index].HotSpot.gameObject.SetActive(visible);
        }

        public void SetSeasonalPopularVisible(int index, bool visible)
        {
            if (index >= 0 && index < _seasonalBindings.Count && _seasonalBindings[index].Popular != null)
                _seasonalBindings[index].Popular.gameObject.SetActive(visible);
        }

        public void SetSeasonalImages(int index, int bannerImageId, int hotSpotImageId, int popularImageId)
        {
            if (index < 0 || index >= _seasonalBindings.Count) return;
            SeasonalBinding binding = _seasonalBindings[index];
            SetImageById(binding.Banner, bannerImageId);
            SetImageById(binding.HotSpot, hotSpotImageId);
            SetImageById(binding.Popular, popularImageId);
        }

        public void SelectSeasonalActivity(int index)
        {
            if (index >= 0 && index < _seasonalBindings.Count && _seasonalBindings[index].Data != null)
                SeasonalActivityRequested?.Invoke(_seasonalBindings[index].Data);
        }

        private void EnsureBound()
        {
            if (_isBound || gameObject.name != ExpectedRootName) return;
            if (_pageRoot == null) _pageRoot = gameObject;

            Canvas.ForceUpdateCanvases();
            Transform root = _pageRoot.transform;
            _taskTitle = BindText(FindDirectChild(root, "task title"));
            _newbiePage = GetObject(FindDirectChild(root, "新手任务Scroll View"));
            _advancedPage = GetObject(FindDirectChild(root, "进阶任务Scroll View"));
            _seasonalPage = GetObject(FindDirectChild(root, "活动任务Canvas"));

            Transform bottomBar = FindDirectChild(root, "Bottom page function bar");
            _backButton = GetButton(FindDirectChild(bottomBar, "BackoffButton"));
            _newbieButton = GetButton(FindDeepChild(bottomBar, "Newbie Task  Button"));
            _advancedButton = GetButton(FindDeepChild(bottomBar, "Advanced Task  Button"));
            _seasonalButton = GetButton(FindDeepChild(bottomBar, "Seasonal Task Button"));
            _seasonalBackButton = GetButton(FindDirectChild(_seasonalPage != null ? _seasonalPage.transform : null, "BackoffButton"));

            BindTaskList(_newbiePage, _newbieBindings);
            BindTaskList(_advancedPage, _advancedBindings);
            BindSeasonalList();
            BindControls();
            _isBound = _newbiePage != null && _advancedPage != null && _seasonalPage != null;
            if (!_isBound) Debug.LogError("[Client] ActivityPage活动缺少新手、进阶或活动任务页面节点。");
        }

        private void BindTaskList(GameObject page, List<TaskBinding> target)
        {
            if (page == null) return;
            Transform content = FindDeepChild(page.transform, "Content");
            if (content == null) return;
            for (int index = 0; index < content.childCount; index++)
            {
                Transform card = content.GetChild(index);
                if (!card.name.StartsWith("任务卡prefabs", StringComparison.Ordinal)) continue;
                Transform background = FindDirectChild(card, "background");
                Transform progressCanvas = FindDeepChild(background, "任务进度条Canvas");
                Transform claimCanvas = FindDeepChild(background, "领取+待完成键Canvas");
                Transform claimButtonNode = FindDeepChild(claimCanvas, "领取+待完成键Button");
                TaskBinding binding = new TaskBinding
                {
                    Root = card.gameObject,
                    Description = BindText(FindDeepChild(background, "任务说明文本")),
                    ProgressLeft = BindText(FindDeepChild(progressCanvas, "进度文本表示左")),
                    ProgressMiddle = BindText(FindDeepChild(progressCanvas, "进度文本表示中")),
                    ProgressRight = BindText(FindDeepChild(progressCanvas, "进度文本表示右")),
                    // 当前 Hierarchy 的两个名称写反：黄色“进度条底框”实际是进度，蓝色“进度条”实际是底框。
                    ProgressFill = GetRect(FindDeepChild(progressCanvas, "进度条底框")),
                    ProgressBase = GetRect(FindDeepChild(progressCanvas, "进度条")),
                    ClaimCanvas = GetObject(claimCanvas),
                    ClaimButton = GetButton(claimButtonNode),
                    ClaimLabelImage = GetImage(FindDeepChild(claimButtonNode, "Image")),
                    ClaimedIcon = GetObject(FindDeepChild(background, "已领取icon")),
                };
                binding.ClaimButtonImage = binding.ClaimButton != null ? binding.ClaimButton.GetComponent<Image>() : null;
                binding.ClaimAnimator = binding.ClaimButton != null ? binding.ClaimButton.GetComponent<Animator>() : null;
                CaptureClaimSprites(binding);
                binding.FullProgressWidth = GetProgressWidth(binding);
                BindRewards(FindRewardContent(background), binding.Rewards);
                int capturedIndex = target.Count;
                if (binding.ClaimButton != null)
                {
                    binding.ClaimButton.onClick = new Button.ButtonClickedEvent();
                    binding.ClaimButton.onClick.AddListener(() => RequestClaim(target, capturedIndex));
                }
                target.Add(binding);
            }
        }

        private void BindRewards(Transform content, List<RewardBinding> target)
        {
            if (content == null) return;
            for (int index = 0; index < content.childCount; index++)
            {
                Transform card = content.GetChild(index);
                if (!card.name.StartsWith("道具卡牌prefab", StringComparison.Ordinal)) continue;
                target.Add(new RewardBinding
                {
                    Root = card.gameObject,
                    CardImage = GetImage(FindDeepChild(card, "道具卡面")),
                    Name = BindText(FindDeepChild(card, "道具名称")),
                    Quantity = BindText(FindDeepChild(card, "道具数量")),
                });
            }
        }

        private void BindSeasonalList()
        {
            if (_seasonalPage == null) return;
            Transform scrollView = FindDeepChild(_seasonalPage.transform, "活动任务Scroll View");
            Transform content = FindDeepChild(scrollView, "Content");
            if (content == null) return;
            for (int index = 0; index < content.childCount; index++)
            {
                Transform card = content.GetChild(index);
                if (!card.name.StartsWith("活动底框prefabs", StringComparison.Ordinal)) continue;
                SeasonalBinding binding = new SeasonalBinding
                {
                    Root = card.gameObject,
                    Title = BindText(FindDeepChild(card, "活动标题")),
                    Date = BindText(FindDeepChild(card, "活动日期")),
                    Banner = GetImage(card),
                    HotSpot = GetImage(FindDeepChild(card, "红点")),
                    Popular = GetImage(FindDeepChild(card, "热门")),
                    PopularText = BindText(FindDeepChild(card, "标注文本")),
                };
                int capturedIndex = _seasonalBindings.Count;
                Button button = card.GetComponent<Button>();
                // 现有活动卡只有 Image，没有预挂 Button。运行时补充交互组件，
                // 保留负责人已完成的节点、素材与排版，不要求手工修改每张活动卡。
                if (button == null) button = card.gameObject.AddComponent<Button>();
                if (binding.Banner != null)
                {
                    binding.Banner.raycastTarget = true;
                    button.targetGraphic = binding.Banner;
                }
                button.transition = Selectable.Transition.None;
                button.interactable = true;
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(() => SelectSeasonalActivity(capturedIndex));
                _seasonalBindings.Add(binding);
            }
        }

        private void BindControls()
        {
            BindButton(_newbieButton, ShowNewbieTasks);
            BindButton(_advancedButton, ShowAdvancedTasks);
            BindButton(_seasonalButton, ShowSeasonalTasks);
            BindButton(_seasonalBackButton, ShowNewbieTasks);
            if (_backButton == null) return;
            _backButton.onClick = new Button.ButtonClickedEvent();
            ClientUiNavigator navigator = GetComponentInParent<ClientUiNavigator>();
            if (navigator != null) _backButton.onClick.AddListener(navigator.Back);
            else _backButton.onClick.AddListener(Hide);
        }

        private void RefreshVisiblePage()
        {
            if (!ClientServices.IsInitialized) return;
            if (_currentSubPage == ClientActivitySubPage.Newbie)
                RefreshTasks(ClientActivityTaskCategory.Newbie, _newbieBindings);
            else if (_currentSubPage == ClientActivitySubPage.Advanced)
                RefreshTasks(ClientActivityTaskCategory.Advanced, _advancedBindings);
            else
                RefreshSeasonal();
        }

        private void RefreshTasks(ClientActivityTaskCategory category, List<TaskBinding> bindings)
        {
            IReadOnlyList<ClientActivityTaskData> tasks = ClientServices.Data.GetActivityTasks(category);
            for (int index = 0; index < bindings.Count; index++)
                ApplyTask(bindings[index], tasks != null && index < tasks.Count ? tasks[index] : null, category);
        }

        private void ApplyTask(TaskBinding binding, ClientActivityTaskData data, ClientActivityTaskCategory expectedCategory)
        {
            if (binding == null) return;
            bool exists = data != null && data.Category == expectedCategory;
            if (binding.Root != null) binding.Root.SetActive(exists);
            binding.Data = exists ? data : null;
            if (!exists) return;

            if (binding.Description != null) binding.Description.Value = data.Description;
            ApplyProgress(binding, data.Progress, data.Target);
            bool claimed = data.IsClaimed;
            bool claimable = data.IsComplete && !claimed && !data.IsClaimPending;
            if (binding.ClaimCanvas != null) binding.ClaimCanvas.SetActive(!claimed);
            if (binding.ClaimedIcon != null) binding.ClaimedIcon.SetActive(claimed);
            if (binding.ClaimButton != null) binding.ClaimButton.interactable = claimable;
            ApplyClaimVisual(binding, claimable);
            SetImageById(binding.ClaimLabelImage, claimable ? data.ClaimLabelImageId : data.PendingLabelImageId);
            if (claimable) SetImageById(binding.ClaimButtonImage, data.ClaimableButtonImageId);
            for (int index = 0; index < binding.Rewards.Count; index++)
                ApplyReward(binding.Rewards[index], index < data.Rewards.Count ? data.Rewards[index] : null);
        }

        private static void ApplyProgress(TaskBinding binding, int progress, int target)
        {
            int safeTarget = Mathf.Max(0, target);
            int safeProgress = Mathf.Clamp(progress, 0, safeTarget);
            if (binding.ProgressLeft != null) binding.ProgressLeft.Value = safeProgress.ToString();
            if (binding.ProgressMiddle != null) binding.ProgressMiddle.Value = "/";
            if (binding.ProgressRight != null) binding.ProgressRight.Value = safeTarget.ToString();
            if (binding.ProgressFill == null) return;

            float ratio = safeTarget > 0 ? (float)safeProgress / safeTarget : 0f;
            binding.ProgressFill.gameObject.SetActive(ratio > 0f);
            if (ratio <= 0f) return;
            float width = binding.FullProgressWidth > 0f ? binding.FullProgressWidth : binding.ProgressFill.rect.width;
            if (binding.ProgressBase != null && binding.ProgressBase.parent == binding.ProgressFill.parent)
            {
                float left = binding.ProgressBase.anchoredPosition.x - binding.ProgressBase.rect.width * binding.ProgressBase.pivot.x;
                binding.ProgressFill.pivot = new Vector2(0f, binding.ProgressFill.pivot.y);
                binding.ProgressFill.anchoredPosition = new Vector2(left, binding.ProgressFill.anchoredPosition.y);
                // Canvas 同级节点越靠后越晚绘制：底框在下、黄色进度在上，最后再把数字放到最上层。
                binding.ProgressFill.SetSiblingIndex(binding.ProgressBase.GetSiblingIndex() + 1);
            }
            binding.ProgressFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * ratio);
            Image fillImage = binding.ProgressFill.GetComponent<Image>();
            if (fillImage != null && fillImage.sprite != null && fillImage.sprite.border.sqrMagnitude > 0f)
                fillImage.type = Image.Type.Sliced;
            if (binding.ProgressLeft != null) binding.ProgressLeft.BringToFront();
            if (binding.ProgressMiddle != null) binding.ProgressMiddle.BringToFront();
            if (binding.ProgressRight != null) binding.ProgressRight.BringToFront();
        }

        private void ApplyReward(RewardBinding binding, ClientActivityTaskReward reward)
        {
            if (binding == null) return;
            // 数据数量不能改变负责人已经摆好的卡牌数量；无数据的卡牌保留场景原始显示。
            if (binding.Root != null) binding.Root.SetActive(true);
            if (reward == null) return;
            if (binding.Name != null) binding.Name.Value = reward.DisplayName;
            if (binding.Quantity != null) binding.Quantity.Value = reward.Quantity.ToString();
            SetImageById(binding.CardImage, reward.ItemImageId);
        }

        private static void CaptureClaimSprites(TaskBinding binding)
        {
            binding.PendingButtonSprite = binding.ClaimButtonImage != null ? binding.ClaimButtonImage.sprite : null;
            binding.PendingLabelSprite = binding.ClaimLabelImage != null ? binding.ClaimLabelImage.sprite : null;
            Animator animator = binding.ClaimAnimator;
            int pressedState = Animator.StringToHash("Pressed");
            if (animator == null || animator.runtimeAnimatorController == null || !animator.HasState(0, pressedState))
                return;

            bool wasEnabled = animator.enabled;
            animator.enabled = true;
            animator.Play(pressedState, 0, 0f);
            animator.Update(0f);
            binding.ClaimableButtonSprite = binding.ClaimButtonImage != null ? binding.ClaimButtonImage.sprite : null;
            binding.ClaimableLabelSprite = binding.ClaimLabelImage != null ? binding.ClaimLabelImage.sprite : null;
            animator.Rebind();
            animator.Update(0f);
            if (binding.ClaimButtonImage != null) binding.ClaimButtonImage.sprite = binding.PendingButtonSprite;
            if (binding.ClaimLabelImage != null) binding.ClaimLabelImage.sprite = binding.PendingLabelSprite;
            animator.enabled = wasEnabled;
        }

        private static void ApplyClaimVisual(TaskBinding binding, bool claimable)
        {
            if (binding.ClaimAnimator != null)
            {
                if (!claimable)
                {
                    binding.ClaimAnimator.enabled = true;
                    binding.ClaimAnimator.Rebind();
                    binding.ClaimAnimator.Update(0f);
                }
                else
                {
                    // 可领取态由现有 Pressed 动画提供两张 Sprite；冻结 Animator，避免 Selectable 立即切回待完成图。
                    binding.ClaimAnimator.enabled = false;
                }
            }

            if (binding.ClaimButtonImage != null)
                binding.ClaimButtonImage.sprite = claimable && binding.ClaimableButtonSprite != null
                    ? binding.ClaimableButtonSprite : binding.PendingButtonSprite;
            if (binding.ClaimLabelImage != null)
                binding.ClaimLabelImage.sprite = claimable && binding.ClaimableLabelSprite != null
                    ? binding.ClaimableLabelSprite : binding.PendingLabelSprite;
        }

        private void RefreshSeasonal()
        {
            IReadOnlyList<ClientActivity> activities = ClientServices.Data.GetActivities();
            for (int index = 0; index < _seasonalBindings.Count; index++)
                ApplySeasonal(_seasonalBindings[index], activities != null && index < activities.Count ? activities[index] : null);
        }

        private void ApplySeasonal(SeasonalBinding binding, ClientActivity data)
        {
            if (binding == null) return;
            bool exists = data != null && data.IsOpen;
            if (binding.Root != null) binding.Root.SetActive(exists);
            binding.Data = exists ? data : null;
            if (!exists) return;
            if (binding.Title != null) binding.Title.Value = data.Title;
            if (binding.Date != null) binding.Date.Value = data.DateText;
            if (binding.PopularText != null) binding.PopularText.Value = "热门";
            if (binding.HotSpot != null) binding.HotSpot.gameObject.SetActive(data.ShowHotSpot);
            if (binding.Popular != null) binding.Popular.gameObject.SetActive(data.ShowPopular);
            SetImageById(binding.Banner, data.BannerImageId);
            SetImageById(binding.HotSpot, data.HotSpotImageId);
            SetImageById(binding.Popular, data.PopularImageId);
        }

        private void RequestClaim(List<TaskBinding> source, int index)
        {
            if (index < 0 || index >= source.Count) return;
            TaskBinding binding = source[index];
            ClientActivityTaskData task = binding.Data;
            if (task == null || task.IsClaimed || task.IsClaimPending || !task.IsComplete) return;

            ClaimStarted?.Invoke(task);
            ClientActivityTaskClaimResult result;
            bool accepted = ClientServices.Data.TryClaimActivityTask(task.Category, task.TaskId, out result);
            if (!accepted || result == null || result.Status == ClientRewardDeliveryStatus.Rejected)
            {
                string message = result != null && !string.IsNullOrEmpty(result.Message) ? result.Message : "奖励领取失败";
                ClaimFailed?.Invoke(task, message);
                TipRequested?.Invoke(message);
                return;
            }
            ClaimFinished?.Invoke(task, result);
            RefreshTasks(task.Category, source);
            RestoreTabSelection();
        }

        private void HandleDataChanged() { RefreshVisiblePage(); }

        private void SubscribeData()
        {
            if (_dataSubscribed || !ClientServices.IsInitialized) return;
            ClientServices.Data.DataChanged += HandleDataChanged;
            _dataSubscribed = true;
        }

        private void UnsubscribeData()
        {
            if (!_dataSubscribed || !ClientServices.IsInitialized) return;
            ClientServices.Data.DataChanged -= HandleDataChanged;
            _dataSubscribed = false;
        }

        private void SetImageById(Image target, int imageId)
        {
            if (target == null || imageId <= 0) return;
            Sprite sprite;
            if (!_registeredSprites.TryGetValue(imageId, out sprite) && _spriteResolver != null)
                sprite = _spriteResolver.Resolve(imageId);
            if (sprite != null) target.sprite = sprite;
        }

        private void RestoreTabSelection()
        {
            Button activeButton = _currentSubPage == ClientActivitySubPage.Advanced ? _advancedButton
                : _currentSubPage == ClientActivitySubPage.Seasonal ? _seasonalButton : _newbieButton;
            if (activeButton == null)
                return;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(activeButton.gameObject);
            else
                activeButton.Select();
        }

        private List<TaskBinding> GetTaskBindings(ClientActivityTaskCategory category)
        {
            return category == ClientActivityTaskCategory.Advanced ? _advancedBindings : _newbieBindings;
        }

        private TaskBinding GetTaskBinding(ClientActivityTaskCategory category, int index)
        {
            List<TaskBinding> bindings = GetTaskBindings(category);
            return index >= 0 && index < bindings.Count ? bindings[index] : null;
        }

        private static float GetProgressWidth(TaskBinding binding)
        {
            if (binding.ProgressBase != null && binding.ProgressBase.rect.width > 0f) return binding.ProgressBase.rect.width;
            return binding.ProgressFill != null ? binding.ProgressFill.rect.width : 0f;
        }

        private static Transform FindRewardContent(Transform background)
        {
            Transform rewardBase = FindDeepChild(background, "道具大底框");
            Transform scrollView = FindDeepChild(rewardBase, "Scroll View");
            return FindDeepChild(scrollView, "Content");
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(action);
        }

        private static TextBinding BindText(Transform transform)
        {
            if (transform == null) return null;
            TMP_Text tmp = transform.GetComponent<TMP_Text>();
            Text legacy = transform.GetComponent<Text>();
            return tmp != null || legacy != null ? new TextBinding { Tmp = tmp, Legacy = legacy } : null;
        }

        private static Transform FindDirectChild(Transform root, string nodeName)
        {
            if (root == null) return null;
            for (int index = 0; index < root.childCount; index++)
                if (root.GetChild(index).name == nodeName) return root.GetChild(index);
            return null;
        }

        private static Transform FindDeepChild(Transform root, string nodeName)
        {
            if (root == null) return null;
            for (int index = 0; index < root.childCount; index++)
            {
                Transform child = root.GetChild(index);
                if (child.name == nodeName) return child;
                Transform found = FindDeepChild(child, nodeName);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject GetObject(Transform value) { return value != null ? value.gameObject : null; }
        private static Image GetImage(Transform value) { return value != null ? value.GetComponent<Image>() : null; }
        private static Button GetButton(Transform value) { return value != null ? value.GetComponent<Button>() : null; }
        private static RectTransform GetRect(Transform value) { return value != null ? value as RectTransform : null; }
    }
}
