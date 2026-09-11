
/******************************************************************************
 * 
 *  Title:  捕鱼项目
 *
 *  Version:  1.0版
 *
 *  Description:
 *
 *  Author:  WangXingXing
 *       
 *  Date:  2018
 * 
 ******************************************************************************/

using System;
using System.Collections;
using System.Linq;
using BNRoom.Static;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class LoadingUI : BaseUI
{
    protected override EnumAnimationType AnimType => EnumAnimationType.None;
    public override bool EscapeClose => false;
    protected override int UIOrder => 30;

    #region UI组件
    private GameObject rectLoadScene;
    private GameObject RectLoadScene
    {
        get
        {
            if (ReferenceEquals(rectLoadScene, null))
                rectLoadScene = UnityHelper.FindTheChild(gameObject, "rectLoadScene").gameObject;
            return rectLoadScene;
        }
    }
    private GameObject rectLoadUI;
    private GameObject RectLoadUI
    {
        get
        {
            if (ReferenceEquals(rectLoadUI, null))
                rectLoadUI = UnityHelper.FindTheChild(gameObject, "rectLoadUI").gameObject;
            return rectLoadUI;
        }
    }
    private Animator loadUIAnimator;
    private Animator LoadUIAnimator
    {
        get
        {
            if (ReferenceEquals(loadUIAnimator, null))
                loadUIAnimator = UnityHelper.GetTheChildComponent<Animator>(gameObject, "rectLoadUI");
            return loadUIAnimator;
        }
    }
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
    private Text txtLoading;
    public Text TxtLoading
    {
        get
        {
            if (ReferenceEquals(txtLoading, null))
                txtLoading = UnityHelper.GetTheChildComponent<Text>(gameObject, "txtLoading");
            return txtLoading;
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
                txtTip = UnityHelper.GetTheChildComponent<Text>(gameObject, "tip");
            return txtTip;
        }
    }
    #endregion
    private AsyncOperation oper;
    private SceneHandle SceneHandle;
    private int sliderValue = 0;
    private EnumUIType openUI = EnumUIType.None;
    private string sceneName;
    private bool isLoadScene;
    private string loadStr = "Loading";
    private bool firstOpen = true;
    private int openCloudId = Animator.StringToHash("openCloud");
    private float delay = 0f;

    public override EnumUIType GetUIType()
    {
        return EnumUIType.LoadingUI;
    }

    protected override void OnStart()
    {
        EventTriggerListener.isLoading = true;
        Debug.LogWarning("LoadingUI>>>>>,isLoadScene?" + isLoadScene);
        if (isLoadScene)
        {
            RectLoadScene.SetActive(true);
            StartCoroutine(loadScene());
            StartCoroutine(refreshLoadingTxt());
            StartCoroutine(loadSceneUI());
        }
        else
        {
            RectLoadUI.SetActive(true);
            StartCoroutine(loadUI());
        }
        AudioManager.Instance.StopAllSoudEff();

        var random = UnityEngine.Random.Range(1, 12);
        var key = random > 9 ? $"LoadingTips_0{random}" : $"LoadingTips_00{random}";
    }

    private IEnumerator loadScene()
    {
        SysDefines.SceneType = (EnumSceneType)Enum.Parse(typeof(EnumSceneType), sceneName);
        delay = SysDefines.SceneType == EnumSceneType.FishScene ? 2f : 0f;
        SceneHandle = YooAssets.LoadSceneAsync(sceneName);
        //oper = SceneManager.LoadSceneAsync(sceneName);
        yield return SceneHandle;
    }
    private IEnumerator refreshLoadingTxt()
    {
        var count = 1;
        while (true)
        {
            var str = "";
            for (int i = 0; i < count; i++)
            {
                str += ".";
            }
            TxtLoading.text = loadStr + str;
            count = (count + 1) % 4;
            yield return new WaitForSeconds(0.5f);
        }
    }

    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

    private IEnumerator loadSceneUI()
    {
        ObjectPoolManager.Instance.ClearAll();
        AudioManager.Instance.StopMusic();
        SkillCooldownManager.ClearAllData();
        BuffManager.ClearAllBuffs();
        
        while (true)
        {
            if (isLoadScene)
            {
                if (SceneHandle != null && SceneHandle.IsDone && sliderValue >= 100)
                    break;
            }
            else if (sliderValue >= 100)
            {
                break;
            }
            
            if (firstOpen && sliderValue > 10 && openUI != EnumUIType.None)
            {
                // Debug.LogError(" ??!!!!" + openUI);

                firstOpen = false;
                closeOthersUI();
                UIManager.Instance.OpenUI(openUI);

                sliderValue += UnityEngine.Random.Range(3, 5);
            }
            else if (UIManager.Instance.FindUIByUIType(openUI))
            {
                // Debug.LogError(" ??!!!!??" + openUI);
                if (openUI == EnumUIType.MainUI || openUI == EnumUIType.FishingCommonUI || openUI == EnumUIType.LobbyUI)
                {
                    sw.Start();
                    var uiObj = UIManager.Instance.GetUIObject(openUI);
                    uiObj.GetComponent<BaseUI>().enabled = true;

                    sliderValue = 100;
                }
                else
                    sliderValue += UnityEngine.Random.Range(3, 5);
            }
            else
            {
                sliderValue += UnityEngine.Random.Range(3, 5);
            }

           
            SliderUI.value = sliderValue;
            TxtProgess.text = SliderUI.value + "%";
            yield return new WaitForSeconds(0.04f);
        }

        Debug.LogWarning("jump out!!!!"+ openUI);
        if (openUI == EnumUIType.MainUI || openUI == EnumUIType.FishingCommonUI || openUI == EnumUIType.LobbyUI)
        {
            sw.Stop();
            Debug.LogWarning($"jump out,{sw.ElapsedMilliseconds}ms");
        }

        if (openUI == EnumUIType.MarbleUI)
        {
            var uiObj = UIManager.Instance.GetUIObject(openUI);
            uiObj.GetComponent<MarbleUI>().OnLoadingUiClose();
        }
        
        CloseUI();
    }

    private IEnumerator loadUI()
    {
        var animClips = LoadUIAnimator.runtimeAnimatorController.animationClips;
        yield return new WaitForSeconds(animClips[0].length);
        closeOthersUI();
        UIManager.Instance.OpenUI(openUI);
        yield return UIManager.Instance.FindUIByUIType(openUI);
        LoadUIAnimator.SetBool(openCloudId, true);
        yield return new WaitForSeconds(animClips[1].length);
        CloseUI();
    }

    private void closeOthersUI()
    {
        var openUIs = UIManager.Instance.GetDicOpenUIs().Keys.ToList();
        for (int i = 0; i < openUIs.Count; i++)
        {
            var key = openUIs[i];
            //Debug.LogError("closeOthersUI==key:" + key);
            if (key != EnumUIType.LoadingUI)
                UIManager.Instance.CloseUI(key);
        }
    }

    protected override void SetUI(params object[] uiParams)
    {
        if (uiParams.Length > 0)
        {
            isLoadScene = (bool)uiParams[0];
            openUI = (EnumUIType)uiParams[1];
            //Debug.LogError("Loading openUI:"+ openUI);
            if (isLoadScene)
            {
                sceneName = uiParams[2].ToString();
               // Debug.LogError("Loading sceneName:" + sceneName);

                //if (uiParams.Length > 3)
                //{
                //    Debug.LogError("Loading 第三个参数不为空:");
                //    base.SetUIWhenOpening(uiParams);
                //    return;
                //}
            }
        }
        else
            CloseUI();
        base.SetUI(uiParams);
    }


    protected override void OnRelease()
    {
        EventTriggerListener.isLoading = false;
    }
}

