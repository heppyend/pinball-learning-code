
/******************************************************************************
 * 
 *  Title:  捕鱼项目
 *
 *  Version:  1.0版
 *
 *  Description:
 *         1：所有UI窗体的父类
 *
 *  Author:  WangXingXing
 *       
 *  Date:  2018
 * 
 ******************************************************************************/

using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using JBPROTO;
using System;



/// <summary>
/// UI基类
/// </summary>
public abstract class BaseUI : MonoBehaviour
{

    protected virtual int UIOrder { get { return -1; } }

    protected virtual int ActivityID { get { return -1; } }//活动Id
    protected virtual EnumAnimationType AnimType { get { return EnumAnimationType.Scale; } }
    public virtual bool EscapeClose { get { return true; } }

    protected CLPFActivityDurationInfo Data;

    private Tweener tweener;
    private float tweenerTime = 0.3f;

    public abstract EnumUIType GetUIType();

    public string UIName = string.Empty;

    private GameObject cacheGameObjet;
    public GameObject CacheGameObject
    {
        get
        {
            if (null == cacheGameObjet)
                cacheGameObjet = gameObject;
            return cacheGameObjet;
        }
    }

    private Transform cacheTransform;
    public Transform CacheTransform
    {
        get
        {
            if (null == cacheTransform)
                cacheTransform = transform;
            return cacheTransform;
        }
    }

    protected EnumObjectState state = EnumObjectState.None;
    public event StateChangeEvent StateChanged;
    public EnumObjectState State
    {
        get { return state; }
        protected set
        {
            if (value != state)
            {
                EnumObjectState oldState = state;
                state = value;
                StateChanged?.Invoke(this, state, oldState);
            }
        }
    }

    public void Awake()
    {
        setAnimation();
        State = EnumObjectState.Initial;
        OnAwake();
        State = EnumObjectState.Loading;
        OnPlayOpenUIAudio();
    }
    private Button button;

    protected Action onCallback;

    // 定义按钮点击时要执行的方法
    void OnButtonClick()
    {
        Destroy(gameObject);
    }
    private void Start()
    {
        OnFalseUI();
        if (UIOrder >= -1)
        {
            var canvas = this.GetOrAddComponent<Canvas>();
            this.GetOrAddComponent<GraphicRaycaster>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = UIOrder;
        }
        for(int i = 0; i < 6; i++)
        {
            Transform btnRelease;
            if (i==0)
                btnRelease = UnityHelper.FindTheChild(gameObject, "btnRelease");
            else
                btnRelease = UnityHelper.FindTheChild(gameObject, "btnRelease"+i);
            if (btnRelease)
                EventTriggerListener.Get(btnRelease).SetEventHandle(EnumTouchEventType.OnClick, OnBtnRelease,
                    UnityHelper.CreateHashtable(EnumHashtableParamsType.LockAllClick, ""));
        }
        


        playAnimation(true);
        InitTogPicPage();

        OnInit();
        //if (ActivityID>0)
        //    gameObject.OnExit(ActivityID, out Data, () =>
        //    {
        //        //OnInit();
        //        OnStart();
        //    }, onCallback);
        //else
        //{
        //    //OnInit();
        //    OnStart();
        //}

        OnStart();

        // 获取当前脚本所挂载物体上的Button组件
        //button = transform.Find("ico_button").GetComponent<Button>();

        //if (button != null)
        //{
        //    // 为Button的点击事件添加监听，当按钮被点击时调用OnButtonClick方法
        //    button.onClick.AddListener(OnButtonClick);
        //}
        //else
        //{
        //    Debug.LogError("当前物体上未找到Button组件！");
        //}
    }

    protected virtual void OnBtnRelease(GameObject gameObject, object eventData, params object[] args)
    {
        CloseUI();
    }

    protected void CloseUI()
    {
        if (GetUIType() != EnumUIType.LuaUI)
            UIManager.Instance.CloseUI(GetUIType());
        else
            UIManager.Instance.CloseLuaUIObject(UIName);
    }

    private void Update()
    {
        if (State == EnumObjectState.Ready)
        {
            OnUpdate(Time.deltaTime);
        }
    }
    public void Release()
    {
        State = EnumObjectState.Closing;

        //if (AwardUI.IsAward)//物资出现
        //{
        //    NetController.Instance.SendTaskAchieveQueryReq();
        //    AwardUI.IsAward = false;
        //}
        
        OnClear();
        OnRelease();
        playAnimation(false);
    }

    private void OnDestroy()
    {

        State = EnumObjectState.None;
        OnPlayCloseUIAudio();
    }

    //ui层级设置
    protected virtual void SetDepthToTop() { }

    protected virtual void OnAwake() { }
    protected virtual void OnStart() { }

    protected virtual void OnInit() { }
    protected virtual void OnUpdate(float deltaTime) { }

    protected virtual void OnLoadData() { }

    protected virtual void OnRelease() { }

    protected virtual void OnClear() { }

    protected virtual void OnFalseUI() { }
    protected virtual void OnPlayOpenUIAudio() { }
    protected virtual void OnPlayCloseUIAudio() { }

    protected virtual void InitTogPicPage() { }
    protected virtual void SetUI(params object[] uiParams)
    {
        CacheTransform.SetParent(GameController.Instance.UIParent, false);
        State = EnumObjectState.Loading;
    }

    public void SetUIWhenOpening(params object[] uiParams)
    {
        OnFalseUI();
        SetUI(uiParams);
        StartCoroutine(LoadDataAsyn());
    }

    private IEnumerator LoadDataAsyn()
    {
        yield return new WaitForSeconds(0f);

        //Debug.LogError($"LoadDataAsyn,{State}");
        if (State == EnumObjectState.Loading)
        {
            OnLoadData();
            State = EnumObjectState.Ready;
        }
    }

    public Transform Find(string name)
    {
        return CacheTransform.Find(name);
    }

    private void setAnimation()
    {
        if (AnimType == EnumAnimationType.None) return;
        switch (AnimType)
        {
            case EnumAnimationType.Scale:
                tweener = CacheTransform.DOScale(Vector3.one * 1.2f, tweenerTime).From().SetEase(Ease.OutQuart);
                break;
        }
        tweener.OnRewind(() => { Destroy(CacheGameObject); });
        tweener.SetAutoKill(false);
        tweener.Pause();
    }

    private void playAnimation(bool isOpen)
    {
        if (AnimType == EnumAnimationType.None)
        {
            if (!isOpen)
                Destroy(CacheGameObject);
        }else
        {
            if (isOpen)
                tweener.PlayForward();
            else
                tweener.PlayBackwards();
        }
    }
}

public enum EnumType
{
    NULL,               //空类型
    Task,               //任务
    TaskStrip,          //任务条
    Purchase,           //购买
    Lottery,            //抽奖
    Exchange,           //兑换
    Finally,            //最后
    Clock,              //签到
}