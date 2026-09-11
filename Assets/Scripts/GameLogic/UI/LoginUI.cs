
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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class LoginUI : BaseUI
{
    public override EnumUIType GetUIType()
    {
        return EnumUIType.LoginUI;
    }

    protected override int UIOrder => 1;
    protected override EnumAnimationType AnimType => EnumAnimationType.None;
    public override bool EscapeClose => false;

    private LoginModule module;
    public LoginModule Module
    {
        get
        {
            if (null == module)
                module = ModuleManager.Instance.Get<LoginModule>();
            return module;
        }
    }

    #region UI组件
    private Button btnStartGame;
    public Button BtnStartGame
    {
        get
        {
            if(null == btnStartGame)
                btnStartGame = Find("btnStartGame").GetComponent<Button>();
            return btnStartGame;
        }
    }
    private Button btnWeiXinStart;
    public Button BtnWeiXinStart
    {
        get
        {
            if (ReferenceEquals(btnWeiXinStart, null))
                btnWeiXinStart = UnityHelper.GetTheChildComponent<Button>(gameObject, "btnWeiXinStart");
            return btnWeiXinStart;
        }
    }
    private Button btnPhoneStart;
    public Button BtnPhoneStart
    {
        get
        {
            if (ReferenceEquals(btnPhoneStart, null))
                btnPhoneStart = UnityHelper.GetTheChildComponent<Button>(gameObject, "btnPhoneStart");
            return btnPhoneStart;
        }
    }
    private Text txtVersion;
    public Text TxtVersion
    {
        get
        {
            if(null == txtVersion)
                txtVersion = Find("txtVersion").GetComponent<Text>();
            return txtVersion;
        }
    }
    private Button btnAgreement;
    public Button BtnAgreement
    {
        get
        {
            if (ReferenceEquals(btnAgreement, null))
                btnAgreement = UnityHelper.GetTheChildComponent<Button>(gameObject, "btnAgreement");
            return btnAgreement;
        }
    }
    private Button btnPrivacy;
    public Button BtnPrivacy
    {
        get
        {
            if (ReferenceEquals(btnPrivacy, null))
                btnPrivacy = UnityHelper.GetTheChildComponent<Button>(gameObject, "btnPrivacy");
            return btnPrivacy;
        }
    }
    private Button btnSetting;
    public Button BtnSetting
    {
        get
        {
            if (ReferenceEquals(btnSetting, null))
                btnSetting = UnityHelper.GetTheChildComponent<Button>(gameObject, "BtnSetting");
            return btnSetting;
        }
    }
    
    private Toggle agreeToggle;
    public Toggle AgreeToggle
    {
        get
        {
            if (ReferenceEquals(agreeToggle, null))
                agreeToggle = UnityHelper.GetTheChildComponent<Toggle>(gameObject, "agreeToggle");
            return agreeToggle;
        }
    }
    private RectTransform autoLogin;
    public RectTransform AutoLogin
    {
        get
        {
            if (ReferenceEquals(autoLogin, null))
                autoLogin = UnityHelper.GetTheChildComponent<RectTransform>(gameObject, "BtnAutoLogin");
            return autoLogin;
        }
    }
    private RectTransform loadingIcon;
    public RectTransform LoadingIcon
    {
        get
        {
            if (ReferenceEquals(loadingIcon, null))
                loadingIcon = UnityHelper.GetTheChildComponent<RectTransform>(gameObject, "LoadingIcon");
            return loadingIcon;
        }
    }
    #endregion

    protected override void OnStart()
    {
        Debug.LogWarning("login  OnStart");
        //var es = GameController.go.transform.Find("EventSystem");
        ////Debug.Log($"Prefab name is {copyGo.name}{es}");
        //es.gameObject.SetActive(true);

        // 先移除可能存在的旧监听
        //EventTriggerListener.Get(BtnStartGame).ClearClick();
        //Debug.LogError("LoginUI onStart，"+ EventTriggerListener.Get(BtnStartGame.transform));
        //EventTriggerListener.Get(BtnStartGame).SetEventHandle(EnumTouchEventType.OnClick, onStartGame, null);
        //var eventTrigger = BtnStartGame.GetComponent<EventTriggerListener>();
        //if (eventTrigger == null)
        //    Debug.Log($"no EventTrigger:" + BtnStartGame);
        //else
        //    Debug.Log($"has EventTrigger:" + BtnStartGame);
        EventTriggerListener.Get(BtnStartGame.transform).SetEventHandle(EnumTouchEventType.OnClick, onStartGame,
            UnityHelper.CreateHashtable(EnumHashtableParamsType.LockAllClick, 2.0f));
        EventTriggerListener.Get(BtnWeiXinStart.transform).SetEventHandle(EnumTouchEventType.OnClick, onBtnWeiXinStart,
            UnityHelper.CreateHashtable(EnumHashtableParamsType.LockAllClick,2.0f));
        EventTriggerListener.Get(BtnPhoneStart.transform).SetEventHandle(EnumTouchEventType.OnClick, onBtnPhoneStart,
            UnityHelper.CreateHashtable(EnumHashtableParamsType.LockAllClick, 1f));

        EventTriggerListener.Get(BtnAgreement).SetEventHandle(EnumTouchEventType.OnClick, onBtnAgreement, null);
        EventTriggerListener.Get(BtnPrivacy).SetEventHandle(EnumTouchEventType.OnClick, onBtnPrivacy, null);
        EventTriggerListener.Get(AutoLogin).SetEventHandle(EnumTouchEventType.OnClick, onBtnAutoLogin, null);
        EventTriggerListener.Get(BtnSetting).SetEventHandle(EnumTouchEventType.OnClick, onBtnSetting, null);

        TxtVersion.text = $"1.{SysDefines.Version}";
        // AudioManager.Instance.PlayMusic(SysDefines.AUDIO + "main");

        MessageCenter.Instance.AddListener(MsgType.CLIENT_AUTHORIZE_ACK, onAuthorize);
        MessageCenter.Instance.AddListener(MsgType.CLIENT_CANCEL_AUTO_LOGIN, onCancelAutoLogin);

        //var gameNameRoot = GameObject.Find("LoginBg/GameName");
        //if (gameNameRoot.transform.childCount <= 0)
        //{
        //    var gameName = ResManager.Instance.LoadPrefab(SysDefines.ZONEPREFAB + $"GameName/GameName_0{SysDefines.ZoneId}");
        //    var obj = Instantiate(gameName);
        //    obj.transform.SetParent(gameNameRoot.transform, false);
        //}
        
        if (SysDefines.FirstLogin == 0)
        {
            SysDefines.FirstLogin = 1;
            var lastLoginType = PlayerPrefs.GetInt("LastLoginType");

            Debug.LogWarning("lastLoginType，" + lastLoginType);
            //lastLoginType = 0;

            if (lastLoginType != 0)
            {
                switch (lastLoginType)
                {
                    case 1:
                    case 2:
                    case 4:
                        StartCoroutine("delayLogin", lastLoginType);
                        break;
                    default:
                        loadingIcon.gameObject.SetActive(false);
                        setLoginButton(true);
                        break;
                }
            }
            else
            {
                LoadingIcon.gameObject.SetActive(false);
                setLoginButton(true);
            }
        }
        else
        {
            LoadingIcon.gameObject.SetActive(false);
            setLoginButton(true);
        }

        
    }

    

    protected override void OnRelease()
    {
        MessageCenter.Instance.RemoveListener(MsgType.CLIENT_AUTHORIZE_ACK, onAuthorize);
        MessageCenter.Instance.RemoveListener(MsgType.CLIENT_CANCEL_AUTO_LOGIN, onCancelAutoLogin);
    }
    private void onBtnSetting(GameObject listener, object eventData, object[] args)
    {
        Debug.LogError("打开登录设置");
        UIManager.Instance.OpenUI(EnumUIType.SettingUI);
    }
    private void setLoginButton(bool active)
    {
        if (active)
        {
            var serverControl = TServerControlHelper.GetRow(SysDefines.ZoneId);
            var array = new List<int>();
            foreach (var j in serverControl.LoginType)
            {
                if (int.Parse(j.ToString()) == 3)
                {
                    array.Add(3);
                    break;
                }
                
            }
            var activeButtonList = new List<Transform>();
            if (array.Contains(1))
            {
                BtnWeiXinStart.gameObject.SetActive(true);
                activeButtonList.Add(BtnWeiXinStart.transform);
            }
            if (array.Contains(2))
            {
                BtnPhoneStart.gameObject.SetActive(true);
                activeButtonList.Add(BtnPhoneStart.transform);
            }
            if (array.Contains(3))
            {
                BtnStartGame.gameObject.SetActive(true);
                activeButtonList.Add(BtnStartGame.transform);
            }
            var start = 0;
            var interval = 0;
            if (activeButtonList.Count == 2)
            {
                start = -300;
                interval = 600;
            }
            else if (activeButtonList.Count == 3)
            {
                start = -520;
                interval = 520;
            }
            var cur = start;
            for (int i = 0; i < activeButtonList.Count; i++)
            {
                activeButtonList[i].localPosition = new Vector3(cur, -292, 0);
                cur += interval;
            }
        }
        else
        {
            BtnWeiXinStart.gameObject.SetActive(false);
            BtnPhoneStart.gameObject.SetActive(false);
            BtnStartGame.gameObject.SetActive(false);
        }
    }

    private IEnumerator delayLogin(int loginType)
    {
        Debug.LogWarning("delayLogin>>>>");

        setLoginButton(false);
        LoadingIcon.gameObject.SetActive(true);
        var str = "";
        switch (loginType)
        {
            case 1:
                str = "正在游客登录中,请稍后...";
                break;
            case 2:
                var phoneAccount = PlayerPrefs.GetString("PhoneAccount");
                str = $"正在使用手机账号{phoneAccount}登录中,请稍后...";
                break;
            case 4:
                str = "正在微信登录中,请稍后...";
                break;
            default:
                break;
        }
        AutoLogin.GetChild(0).GetComponent<Text>().text = str;
        AutoLogin.gameObject.SetActive(true);
        //  yield return new WaitForSeconds(0.2f);
        yield return null;
        switch (loginType)
        {
            case 1:
                onStartGame(null, null, null);
                break;
            case 2:
                onBtnPhoneStart(null, null, null);
                break;
            case 4:
                onBtnWeiXinStart(null, null, null);
                break;
            default:
                break;
        }
    }

    //登录方式 1游客 2三方平台 3QQ 4微信 5Facebook 6GooglePlay 7GameCenter
    private void onStartGame(GameObject gameObject, object eventData, params object[] args)
    {
        Module.SendNetConnect(1);
    }

    //微信登录
    private void onBtnWeiXinStart(GameObject gameObject, object eventData, params object[] args)
    {
        if (!AgreeToggle.isOn) return;

        if (PlayerPrefs.HasKey("WechatOpenId") && !string.IsNullOrEmpty(PlayerPrefs.GetString("WechatOpenId")))
        {
            //微信已授权
            SysDefines.LoginToken = PlayerPrefs.GetString("WechatOpenId");
            Module.SendNetConnect(4);
        }
        else
        {
            //微信未授权 或 切换账号登录
            MessageCenter.Instance.SendMessage(MsgType.CLIENT_AUTHORIZE_REQ, this, null,
                new Dictionary<string, object>() { { "platform", 4 } });
        }
    }

    //手机登录
    private void onBtnPhoneStart(GameObject gameObject, object eventData, params object[] args)
    {
        if (!AgreeToggle.isOn) return;

        UIManager.Instance.OpenUI(EnumUIType.RegisterUI);

        //var phoneAccount = PlayerPrefs.GetString("PhoneAccount");
        //var phonePassword = PlayerPrefs.GetString("PhonePassword");

        ////进入游戏首次登录使用默认账号
        //if (!string.IsNullOrEmpty(phoneAccount) && !string.IsNullOrEmpty(phonePassword))
        //{
        //    NetController.Instance.SendAccountLoginRequest(phoneAccount, phonePassword, (token) =>
        //     {
        //         SysDefines.LoginToken = token;
        //         SysDefines.LoginType = 2;
        //         Module.SendNetConnect(SysDefines.LoginType);
        //     });
        //}
        //else
        //{
        //    UIManager.Instance.OpenUI(EnumUIType.RegisterUI);
        //}
    }

    //授权登录回调
    private void onAuthorize(Message msg)
    {
        var platform = int.Parse(msg["platform"].ToString());
        var code = msg["code"].ToString();
        switch (platform)
        {
            case 4:
                NetController.Instance.GetWechatOpenId(code, (openid) =>
                 {
                     SysDefines.LoginToken = openid;
                     Module.SendNetConnect(4);
                 });
                break;
            default:
                break;
        }
    }

    //断开连接
    private void onCancelAutoLogin(Message msg)
    {
        StopCoroutine("delayLogin");
        setLoginButton(true);
        LoadingIcon.gameObject.SetActive(false);
        AutoLogin.gameObject.SetActive(false);
    }

    protected override void OnBtnRelease(GameObject gameObject, object eventData, params object[] args)
    {
        GameController.Instance.QuitGame();
    }

    private void onBtnAgreement(GameObject gameObject, object eventData, params object[] args)
    {
        UIManager.Instance.OpenUI(EnumUIType.UserAgreementUI);
    }

    private void onBtnPrivacy(GameObject gameObject, object eventData, params object[] args)
    {
        UIManager.Instance.OpenUI(EnumUIType.PrivacyGuidelinesUI);
    }

    private void onBtnAutoLogin(GameObject gameObject, object eventData, params object[] args)
    {
        StopCoroutine("delayLogin");
        setLoginButton(true);
        LoadingIcon.gameObject.SetActive(false);
        AutoLogin.gameObject.SetActive(false);
    }
}