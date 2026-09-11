using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HotUpdateUI : BaseUI
{
    public override EnumUIType GetUIType()
    {
        return EnumUIType.HotUpdateUI;
    }

    protected override EnumAnimationType AnimType => EnumAnimationType.None;
    public override bool EscapeClose => false;
    protected override int UIOrder => 30;

    private const float bytesToMb = 1048576;
    private const float bytesToKb = 1024;

    float autoProgress = 0f;
    float m_fCurrentSmoothProgress = 0;
    float m_fRealTotalProgress = 0;

    // 目标进度值
    private float m_fTargetProgress = 0f;
    // 添加当前进度和目标进度双变量系统
    private float m_fCurrentProgress = 0;
    private float m_fLastProgress = 0;
    bool isEndProgress = false;

    float autoSpeed = 0.005f; // 自动前进的速度
    bool mFlg = false;

    #region UI组件
    private Slider sliderUI;
    public Slider SliderUI
    {
        get
        {
            if (ReferenceEquals(sliderUI, null))
                sliderUI = UnityHelper.GetTheChildComponent<Slider>(gameObject, "slider");
            return sliderUI;
        }
    }
    private RectTransform loadingIcon;
    public RectTransform LoadingIcon
    {
        get
        {
            if (ReferenceEquals(loadingIcon, null))
                loadingIcon = UnityHelper.GetTheChildComponent<RectTransform>(gameObject, "LoadlingIcon");
            return loadingIcon;
        }
    }

    private Text txtProgess;
    public Text TxtProgess
    {
        get
        {
            if (ReferenceEquals(txtProgess, null))
                txtProgess = UnityHelper.GetTheChildComponent<Text>(gameObject, "TxtProgess");
            return txtProgess;
        }
    }

    private Text txtTip;
    public Text TxtTip
    {
        get
        {
            if (ReferenceEquals(txtTip, null))
                txtTip = UnityHelper.GetTheChildComponent<Text>(gameObject, "TxtUPload");
            return txtTip;
        }
    }
    #endregion

    protected override void OnAwake()
    {
        MessageCenter.Instance.AddListener(MsgType.CLIENT_JSON_UPDATE_ACK, onJsonLoaded);

        isEndProgress = false;
        SliderUI.value = 0;
        TxtProgess.text = "0%";
        TxtTip.text = "";
       
        m_fCurrentProgress = 0;
        m_fLastProgress = 0;
        autoProgress = 0f;
       // SliderUI.gameObject.SetActive(true);
        
    }

    protected override void OnRelease()
    {
        MessageCenter.Instance.RemoveListener(MsgType.CLIENT_JSON_UPDATE_ACK, onJsonLoaded);
    }

    private void onJsonLoaded(Message msg)
    {
        Debug.LogWarning("onJsonLoaded---");

        AppRoot.Get().AddHotUpdateListener(onCheckDone, onProgress, onError, onComplete);
        // var hallVersion = THotUpdateHelper.DataMap.Values.Where(t => t.GameId == SysDefines.GameID_Hall).ToList().Last().Version;
        AppRoot.Get().HotUpdate("260526");
    }

    private void onCheckDone(int totalBytes, int totalBytes1, int gameid)
    {
        Debug.LogError("totalBytes:" + totalBytes);

        if (totalBytes > 0)
        {
            string str = "";
            if (totalBytes >= bytesToMb)
            {
                str = UnityHelper.NumberFormat((long)((float)totalBytes / bytesToMb), 0) + "M";
            }
            else if (totalBytes >= bytesToKb)
            {
                str = UnityHelper.NumberFormat((long)((float)totalBytes / bytesToKb), 0) + "K";
            }
            else
            {
                str = totalBytes + "B";
            }
            var msg = $"您有{str}资源需要下载";

            Debug.LogError("弹出框？");

            TxtTip.text = "共 " + UnityHelper.NumberFormat((long)((float)totalBytes1 / bytesToMb), 0) + "MB      已下载" + (1 - totalBytes / totalBytes1) + "%";
            // UIManager.Instance.OpenMessageBoxUI(msg, 0, EnumMessageBoxType.OK_CANCEL, onBtnOK, null, onBtnCancel);
            onBtnOK(new object());
        }
        else
        {
            // TxtTip.text = "共 " + UnityHelper.NumberFormat((long)((float)totalBytes1 / bytesToMb), 0) + "MB      已下载" + (1 - totalBytes / totalBytes1) + "%";
            // TxtTip.text = "不需要下载。";

            // onComplete(0);
            //if (type == ProgressType.CheckFileList)
            {
                mFlg = true;
                Debug.LogError("?进度条:isEndProgress:" + isEndProgress + "," + m_fCurrentSmoothProgress);
                // autoSpeed = 0.1f;
            }
        }
    }

    private void onBtnOK(object args)
    {
        AppRoot.Get().StartHotUpdateDownload();
        StartCoroutine(SmoothProgressRoutine());
    }

    private void onBtnCancel(object args)
    {
        Application.Quit();
    }

    private void onProgress(ProgressType type, int loaded, int total, int gameid)
    {
        if (LoadingIcon.gameObject.activeSelf)
            LoadingIcon.gameObject.SetActive(false);
        if (!SliderUI.gameObject.activeSelf)
            SliderUI.gameObject.SetActive(true);

        // 
        if (total > 0)
        {            
            m_fRealTotalProgress = (float)loaded / (float)total;

            //Debug.LogError("进度条:" + loaded + "," + total+",="+ m_fRealTotalProgress);
            // SliderUI.value = (float)loaded * 100 / total;

            // TxtProgess.text = SliderUI.value + "%";
            //Debug.LogError("进度条:" + m_fRealTotalProgress);
        }
        else
        {

            TxtTip.text = "?进度条:";
            //mFlg = true;
            //StartCoroutine(SmoothProgressRoutine1());
            //StartSmoothProgress();
        }
        //UpdateProgressBar(m_fCurrentSmoothProgress);


    }
    //private Coroutine smoothProgressCoroutine;

    //// 启动协程前先停止之前的
    //void StartSmoothProgress()
    //{
    //    // 停止之前可能存在的协程
    //    if (smoothProgressCoroutine != null)
    //    {
    //        Debug.LogError("停止之前可能存在的协程");
    //        StopCoroutine(smoothProgressCoroutine);
    //    }
    //    else
    //        Debug.LogError("新开协程");
    //    smoothProgressCoroutine = StartCoroutine(SmoothProgressRoutine1());
    //}

    private void onError(int errorCode, string error, int gameid)
    {

    }

    private void onComplete(int gameid)
    {
        Debug.LogWarning("k完成:");

        //TxtTip.text = "完成:";
       // UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //{
            StartCoroutine(complete());
       // });
    }

    private IEnumerator complete()
    {
        Debug.LogWarning("打开进度条。complete10");
        //TxtTip.text = "complete:";
        //yield return null;
        //isEndProgress = true;
        //yield return new WaitForSeconds(0.01f);
        //LoadingIcon.gameObject.SetActive(true);

        //yield return new WaitForSeconds(0.02f);
        //UIManager.Instance.PreloadUI(SysDefines.preloadUIArray);

        //yield return new WaitForSeconds(0.02f);

        UIManager.Instance.OpenUI(EnumUIType.LoginUI);
       

        yield return new WaitForSeconds(0.02f);
       
        LoadingIcon.gameObject.SetActive(true);
		yield return new WaitForSeconds(0.02f);

        isEndProgress = true;
        UpdateProgressBar(1);

        var uiObj = UIManager.Instance.GetUIObject(EnumUIType.LoginUI);
        uiObj.GetComponent<BaseUI>().enabled = true;

       

        CloseUI();

	}

    IEnumerator SmoothProgressRoutine()
    {
       // Debug.LogError("SmoothProgressRoutine,isEndProgress:" + isEndProgress);
       
        while (!isEndProgress)
        {
            // 先让进度条自己往前走（保证持续移动）
            autoProgress += /*Time.unscaledDeltaTime **/ autoSpeed;
            autoProgress = Mathf.Min(autoProgress, 1); // 最多自动走到90%

            // 同时关注真实进度，取两者中较大的值
            float targetProgress = Mathf.Min(m_fRealTotalProgress, autoProgress);
            
            // 平滑过渡到目标值
            m_fCurrentSmoothProgress = Mathf.Lerp(m_fCurrentSmoothProgress, targetProgress, 1f * Time.unscaledDeltaTime);

            //Debug.LogError("m_fRealTotalProgress:" + m_fRealTotalProgress + "," + m_fCurrentSmoothProgress);

            UpdateProgressBar(m_fCurrentSmoothProgress);
            yield return null;
        }

        // 最后走到100%
       // UpdateProgressBar(1f);
    }
  

    protected override void OnUpdate(float deltaTime)
    {
        base.OnUpdate(deltaTime);

        if (mFlg)
        {
            if (!isEndProgress && m_fCurrentSmoothProgress < 0.95f)
            {
              //  Debug.LogError("?？？？?进度条:m_fCurrentSmoothProgress:" + m_fCurrentSmoothProgress);

                m_fCurrentSmoothProgress += Time.unscaledDeltaTime * 1.2f; // 调整这个系数控制速度
                m_fCurrentSmoothProgress = Mathf.Min(m_fCurrentSmoothProgress, 0.95f);
                UpdateProgressBar(m_fCurrentSmoothProgress);
                //TxtTip.text = $"进度: {m_fCurrentSmoothProgress:P0}";
            }
        }
    }
    IEnumerator SmoothProgressRoutine1()
    {
        //lastUpdateTime = DateTime.Now;
        //while (!isEndProgress)
        //{
        //    if (Time.unscaledDeltaTime < 0.1f) // 主线程不忙时才更新进度条
        //    {
        //        // 先让进度条自己往前走（保证持续移动）
        //        autoProgress += /*Time.unscaledDeltaTime **/ autoSpeed;
        //        autoProgress = Mathf.Min(autoProgress, 1); // 最多自动走到90%

        //        m_fCurrentSmoothProgress = autoProgress;
        //        TxtTip.text = "2-autoProgress:" + m_fCurrentSmoothProgress;
        //        //Debug.LogError("m_fRealTotalProgress:" + m_fRealTotalProgress + "," + m_fCurrentSmoothProgress);

        //        // 检测帧率
        //        //TimeSpan delta = DateTime.Now - lastUpdateTime;

        //        UpdateProgressBar(m_fCurrentSmoothProgress);

        //        //if (delta.TotalMilliseconds > 100) // 如果间隔超过100ms说明主线程被阻塞
        //        //{
        //        //    Debug.LogWarning($"主线程被阻塞! 间隔: {delta.TotalMilliseconds}ms");
        //        //}
        //    }

        yield return new WaitForSecondsRealtime(0.02f); // 更小的间隔让动画更平滑
                                                        //}


    }

    private void UpdateProgressBar(float progress)
    {
		//Debug.LogError("progress:" + progress);
		//if (progress >= m_fLastProgress)
		{
            m_fLastProgress = progress;
            //m_ary_Slider[SMGlobal.M_iGameNoCur].fillAmount = progress;
            //m_ary_TextLoad[SMGlobal.M_iGameNoCur].text = ((int)(progress * 100)).ToString();

            SliderUI.value = ((int)(progress * 100));
            TxtProgess.text = ((int)(progress * 100)).ToString() + "%";
        }
    }

}
