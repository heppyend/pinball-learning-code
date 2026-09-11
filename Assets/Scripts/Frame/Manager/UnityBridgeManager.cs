using JBPROTO;
using LC.Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityBridgeManager : MonoSingleton<UnityBridgeManager> {

#if UNITY_ANDROID
    private AndroidJavaObject joCurrentActivity;
#elif UNITY_IOS
    [DllImport("__Internal")]
    private static extern void U2iInitBridge(string unityReceiver);
    //[DllImport("__Internal")]
    //private static extern void U2iChannelInit(string appid, string appkey, string appsecret, string extend);
    //[DllImport("__Internal")]
    //private static extern void U2iChannelLogin();
    //[DllImport("__Internal")]
    //private static extern void U2iChannelPay(string goodsInfo, string payParams);
    [DllImport("__Internal")]
    private static extern void U2iWechatInit(string appid);
    [DllImport("__Internal")]
    private static extern void U2iWechatLogin();
    [DllImport("__Internal")]
    private static extern void U2iWechatPay(string appId, string partnerId, string prepayId, string packageValue, string nonceStr, string timeStamp, string sign);
    [DllImport("__Internal")]
    private static extern void U2iWechatShareUrl(string datas);
    [DllImport("__Internal")]
    private static extern void U2iWechatShareImage(string datas, byte[] byteDatas, int length);
    [DllImport("__Internal")]
    private static extern void U2iAliPay(string orderString);
    //[DllImport("__Internal")]
    //private static extern void U2iSecVerifyLogin();
#endif

    protected override void Initialize() {
#if UNITY_ANDROID
        AndroidJavaClass jcCurrentActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        joCurrentActivity = jcCurrentActivity.GetStatic<AndroidJavaObject>("currentActivity");
#elif UNITY_IOS
#endif
            MessageCenter.Instance.AddListener(MsgType.CLIENT_PAY_REQ, OnPayReq);
        MessageCenter.Instance.AddListener(MsgType.CLIENT_ACTIVITY_PAY_REQ, onActivityPayReq);
        MessageCenter.Instance.AddListener(MsgType.CLIENT_AUTHORIZE_REQ, OnAuthorizeReq);
        MessageCenter.Instance.AddListener(MsgType.CLIENT_ON_WECHAT_SHARE_URL, OnWechatShareUrl);
        MessageCenter.Instance.AddListener(MsgType.CLIENT_ON_WECHAT_SHARE_SCREENSHOT, OnWechatShareScreenShot);
    }

    #region Init
    public void Init() {
        //Debug.LogError("Init");
#if !UNITY_EDITOR
        SMSSDKManager.Instance.Set();
#endif
        InitBridge();
        var loginTypes = new List<int>(); // 1.微信 2.ID卡 3.游客
        
        JArray a = TServerControlHelper.GetRow(SysDefines.ZoneId).LoginType;
        // Debug.LogError("ZoneId："+ SysDefines.ZoneId);
        // Debug.LogError("GetRow："+ a.Count);
        
        foreach (var j in TServerControlHelper.GetRow(SysDefines.ZoneId).LoginType)
            loginTypes.Add(int.Parse(j.ToString()));
        if (loginTypes.Contains(1))
        {
            // InitWechat();
        }

        if (loginTypes.Contains(2))
        {
            // InitChannel();
        }
    }

    private void InitBridge() {
#if UNITY_EDITOR
#elif UNITY_ANDROID
        joCurrentActivity.Call("InitBridge", this.gameObject.name);
#elif UNITY_IOS
        U2iInitBridge(this.gameObject.name);
#endif
    }

    private void InitWechat() {
        NetController.Instance.GetWechatAppid((appid, scope) => {
#if UNITY_EDITOR
#elif UNITY_ANDROID
            joCurrentActivity.Call("WechatInit", appid);
#elif UNITY_IOS
            U2iWechatInit(appid);
#endif
        });
    }

    private void InitChannel() {
        NetController.Instance.GetChannelAppInfo((appid, appkey, appsecret, extend) => {
#if UNITY_EDITOR
#elif UNITY_ANDROID
            joCurrentActivity.Call("ChannelInit", appid, appkey, appsecret, extend);
#elif UNITY_IOS
            //U2iChannelInit(appid, appkey, appsecret, extend);
#endif
        });
    }
    #endregion

    #region Login
    //登录方式 1游客 2手机 3QQ 4微信 5Facebook 6GooglePlay 7GameCenter 8三方平台
    private void OnAuthorizeReq(Message msg) {
        var platform = int.Parse(msg["platform"].ToString());
        switch(platform) {
            case 2:
                OnSecVerifyLogin();
                break;
            case 4:
                OnWechatLogin();
                break;
            case 8:
                OnChannelLogin();
                break;
            default:
                break;
        }
    }

    //渠道登录
    private void OnChannelLogin() {
#if UNITY_EDITOR
#elif UNITY_ANDROID
        joCurrentActivity.Call("ChannelLogin");
#elif UNITY_IOS
        //U2iChannelLogin();
#endif
    }

    //渠道登录回调
    public void OnChannelLoginResult(string result) {
        MessageCenter.Instance.SendMessage(MsgType.CLIENT_AUTHORIZE_ACK, this, null,
            new Dictionary<string, object>() { { "platform", 8 }, { "result", result } });
    }

    //微信登录
    private void OnWechatLogin() {
#if UNITY_EDITOR
#elif UNITY_ANDROID
        joCurrentActivity.Call("WechatLogin");
#elif UNITY_IOS
        U2iWechatLogin();
#endif
    }

    //微信登录回调
    public void OnWechatAuthResult(string code) {
        MessageCenter.Instance.SendMessage(MsgType.CLIENT_AUTHORIZE_ACK, this, null,
            new Dictionary<string, object>() { { "platform", 4 }, { "code", code } });
    }

    //手机秒验登录
    private void OnSecVerifyLogin() {
#if UNITY_EDITOR
#elif UNITY_ANDROID
        joCurrentActivity.Call("SecVerifyLogin");
#elif UNITY_IOS
        //U2iSecVerifyLogin();
#endif
    }
    #endregion

    #region Pay
    //支付
    private void OnPayReq(Message msg) {
        var rsp = msg.Content as CLPFRechargeAck;
        if (string.IsNullOrEmpty(rsp.pay_envir))
            return;
        var payMode = int.Parse(msg["PayMode"].ToString());
        if(payMode > 100) {
            var goodsId = int.Parse(msg["GoodsId"].ToString());
            var goodsType = int.Parse(msg["GoodsType"].ToString());
            var goodsName = msg["GoodsName"].ToString();
            var goodsPrice = int.Parse(msg["GoodsPrice"].ToString());
            JObject joGoodsInfo = new JObject {
                { "PayMode", payMode },
                { "GoodsId", goodsId },
                { "GoodsType", goodsType },
                { "GoodsName", goodsName },
                { "GoodsPrice", goodsPrice }
            };
            OnChannelPay(joGoodsInfo.ToString(), rsp.pay_envir);
        } else {
            switch(payMode) {
                case 1:
                    OnWechatPayReq(rsp.pay_envir);
                    break;
                case 2:
                    OnAliPayReq(rsp.pay_envir);
                    break;
                case 3:
                case 4:
                case 15:
                case 16:
                case 17:
                    OnQrcodePayReq(rsp.pay_envir, payMode);
                    break;
                case 7:
                    OnAgentPayReq(rsp.pay_envir);
                    break;
                case 5:
                case 6:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                    Application.OpenURL(rsp.pay_envir);
                    break;
                default:
                    break;
            }
        }
    }

    private void onActivityPayReq(Message msg)
    {
        var rsp = msg.Content as CLPFActivityRechargeAck;
        if (string.IsNullOrEmpty(rsp.pay_envir))
            return;
        var payMode = int.Parse(msg["PayMode"].ToString());
        var goodsId = int.Parse(msg["GoodsId"].ToString());
        var goodsType = int.Parse(msg["GoodsType"].ToString());
        var goodsName = msg["GoodsName"].ToString();
        var goodsPrice = int.Parse(msg["GoodsPrice"].ToString());
        if (payMode > 100)
        {
            JObject joGoodsInfo = new JObject {
                { "PayMode", payMode },
                { "GoodsId", goodsId },
                { "GoodsType", goodsType },
                { "GoodsName", goodsName },
                { "GoodsPrice", goodsPrice }
            };
            OnChannelPay(joGoodsInfo.ToString(), rsp.pay_envir);
        }
        else
        {
            switch (payMode)
            {
                case 1:
                    OnWechatPayReq(rsp.pay_envir);
                    break;
                case 2:
                    OnAliPayReq(rsp.pay_envir);
                    break;
                case 3:
                case 4:
                case 15:
                case 16:
                case 17:
                    OnQrcodePayReq(rsp.pay_envir, payMode);
                    break;
                case 7:
                    OnAgentPayReq(rsp.pay_envir);
                    break;
                case 5:
                case 6:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                    Application.OpenURL(rsp.pay_envir);
                    break;
                default:
                    break;
            }
        }
    }

    //渠道支付
    private void OnChannelPay(string goodsInfo, string payParams) {
#if UNITY_EDITOR
#elif UNITY_ANDROID
        joCurrentActivity.Call("ChannelPay", goodsInfo, payParams);
#elif UNITY_IOS
        //U2iChannelPay(goodsInfo, payParams);
#endif
    }

    //渠道支付回调
    public void OnChannelPayResult(string result) {
        var joResult = JObject.Parse(result);
        var code = int.Parse(joResult["code"].ToString());
        var message = joResult["message"].ToString();
        if(code != 0)
            GameController.Instance.CreateHintMessage(message);
    }

    //渠道退出
    public void OnChannelExit() {
#if UNITY_EDITOR
#elif UNITY_ANDROID
        //joCurrentActivity.Call("ChannelExit");
#elif UNITY_IOS
#endif
    }

    //渠道退出回调
    private void OnChannelExitResult(string result) {
        var code = int.Parse(result);
        if(code == 0)
            GameController.Instance.QuitGameDirect(null);
        else
            GameController.Instance.QuitGameWithMessage();
    }

    //微信支付
    private void OnWechatPayReq(string envir) {
        var joEnvir = JObject.Parse(envir);
#if UNITY_EDITOR
#elif UNITY_ANDROID
        joCurrentActivity.Call("WechatPay", 
            joEnvir["appid"].ToString(), 
            joEnvir["partnerid"].ToString(), 
            joEnvir["prepayid"].ToString(), 
            joEnvir["package"].ToString(), 
            joEnvir["noncestr"].ToString(), 
            joEnvir["timestamp"].ToString(), 
            joEnvir["sign"].ToString());
#elif UNITY_IOS
        U2iWechatPay(joEnvir["appid"].ToString(), 
            joEnvir["partnerid"].ToString(), 
            joEnvir["prepayid"].ToString(), 
            joEnvir["package"].ToString(), 
            joEnvir["noncestr"].ToString(), 
            joEnvir["timestamp"].ToString(), 
            joEnvir["sign"].ToString());
#endif
    }

    //支付宝支付
    private void OnAliPayReq(string envir) {
#if UNITY_EDITOR
#elif UNITY_ANDROID
        joCurrentActivity.Call("AliPay", envir.ToString());
#elif UNITY_IOS
        U2iAliPay(envir);
#endif
    }

    //线下支付
    private void OnAgentPayReq(string envir) {
        var joEnvir = JObject.Parse(envir);
        UIManager.Instance.OpenUI(EnumUIType.PayAgentUI, new Dictionary<string, object>() {
            {"content", joEnvir["content"].ToString()},
            {"order_no", joEnvir["order_no"].ToString()},
        });
    }

    //扫码支付
    private void OnQrcodePayReq(string envir, int payMode) {
        var joEnvir = JObject.Parse(envir);
        UIManager.Instance.OpenUI(EnumUIType.PayQRCodeUI, new Dictionary<string, object>() {
            {"qrcode", joEnvir["qrcode"].ToString()},
            {"price", joEnvir["price"].ToString()},
            {"payMode", payMode},
        });
    }
    #endregion

    #region Share
    //微信分享链接
    private void OnWechatShareUrl(Message msg) {
        string datas = string.Empty;
        if(!ReferenceEquals(msg.Content, null))
            datas = msg.Content.ToString();
#if UNITY_EDITOR
#elif UNITY_ANDROID
        joCurrentActivity.Call("WechatShareUrl", datas);
#elif UNITY_IOS
        U2iWechatShareUrl(datas);
#endif
    }

    private bool isScreenShotSharing = false;
    //微信分享截屏
    private void OnWechatShareScreenShot(Message msg) {
        if(isScreenShotSharing)
            return;
        isScreenShotSharing = true;
        string datas = string.Empty;
        if(!ReferenceEquals(msg.Content, null))
            datas = msg.Content.ToString();
        StartCoroutine(CorWechatShareScreenShot(datas));
    }

    private IEnumerator CorWechatShareScreenShot(string datas) {
        yield return new WaitForEndOfFrame();
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        Texture2D screenShot = new Texture2D((int) rect.width, (int) rect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0);
        screenShot.Apply();
        byte[] byteDatas = screenShot.EncodeToPNG();
#if UNITY_EDITOR
#elif UNITY_ANDROID
        joCurrentActivity.Call("WechatShareImage", datas, byteDatas);
#elif UNITY_IOS
        U2iWechatShareImage(datas, byteDatas, byteDatas.Length);
#endif
        isScreenShotSharing = false;
    }
    #endregion

    #region Other
    //渠道数据上报
    public void OnChannelReport() {
        var player = GameController.Instance.Player;
        JObject joReportParams = new JObject {
            { "RoleId", player.UserID },
            { "RoleName", player.NickName },
            { "RoleLevel", player.Level },
            { "ZoneId", SysDefines.ZoneId }
        };
        var reportParams = joReportParams.ToString();
#if UNITY_EDITOR
#elif UNITY_ANDROID
        joCurrentActivity.Call("ChannelReport", reportParams);
#elif UNITY_IOS
#endif
    }

    //打印日志
    public void Log(string logInfo) {
        Debug.Log(logInfo);
    }

    public string GetAppVersionName() {
        string result = SysDefines.Version.ToString();
#if UNITY_EDITOR
#elif UNITY_ANDROID
        result = joCurrentActivity.Call<string>("getVersionName");
#elif UNITY_IOS
#endif
        return result;
    }

    public string[] GetAssetsList(string dataDir) {
#if UNITY_EDITOR
#elif UNITY_ANDROID
        return joCurrentActivity.Call<string[]>("GetAssetsList", dataDir);
#elif UNITY_IOS
#endif
        return null;
    }

    public void InstallApk(string apkPath) {
#if UNITY_EDITOR
#elif UNITY_ANDROID
        joCurrentActivity.Call("InstallApk", apkPath);
#elif UNITY_IOS
#endif
    }
    #endregion

}
