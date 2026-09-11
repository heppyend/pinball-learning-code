using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cn.SMSSDK.Unity;
using System;

public class SMSSDKManager : DDOLSingleton<SMSSDKManager>, SMSSDKHandler
{
    public const string AppKey = "331c68509569c";
    public const string AppSecret = "beca535128a7fc29b1610264007e5f9e";
    public const string SMSZone = "86";
    private string template = "";

    public DateTime lastVerifyTime;

    private SMSSDK sdk;
    public void Set()
    {
        sdk = gameObject.GetOrAddComponent<SMSSDK>();
        sdk.init(AppKey, AppSecret, false);
        sdk.setHandler(this);

        var serverControl = TServerControlHelper.GetRow(SysDefines.ZoneId);
    }

    public void ReqShortMessage(string phone)
    {
        if (SysDefines.SmsChannel == 0)
            sdk.getCode(CodeType.TextCode, phone, SMSZone, template);
        else
            NetController.Instance.SendSmsSendPhoneCode(phone);
        lastVerifyTime = DateTime.Now;
    }

    public void SendVerify()
    {

    }

    public void onComplete(int action, object resp)
    {
        Debug.Log("onComplete");
        if (action == (int)ActionType.GetCode)
        {
            GameController.Instance.CreateHintMessage("请求成功");
        }
    }

    public void onError(int action, object resp)
    {
        Debug.Log("onError");
        if (action == (int)ActionType.GetCode)
        {
            GameController.Instance.CreateHintMessage((string)resp);
        }
    }
}
