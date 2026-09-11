
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

using JBPROTO;
using LC.Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine;


public class LoginModule : BaseModule 
{
    public LoginModule()
    {
        AutoRegister = true;
    }

    protected override void OnLoad()
    {
        MessageCenter.Instance.AddListener(MsgType.NET_CONNECT, netConnectState);
        MessageCenter.Instance.AddListener(MsgType.NET_DISCONNECT_NTF, disconnectNtf);
        MessageCenter.Instance.AddListener(MsgType.NET_HAND_ACK, handAck);
        MessageCenter.Instance.AddListener(MsgType.NET_LOGIN_PLATFORM_ACK, loginPlatformAck);
        MessageCenter.Instance.AddListener(MsgType.NET_ACCESSSERVICE_ACK, accessServiceAck);
        MessageCenter.Instance.AddListener(MsgType.NET_GET_PLAYER_ALL_BUFF_INFO, AllBuffAck);
    }


    protected override void OnRelease()
    {
        MessageCenter.Instance.RemoveListener(MsgType.NET_CONNECT, netConnectState);
        MessageCenter.Instance.RemoveListener(MsgType.NET_DISCONNECT_NTF, disconnectNtf);
        MessageCenter.Instance.RemoveListener(MsgType.NET_HAND_ACK, handAck);
        MessageCenter.Instance.RemoveListener(MsgType.NET_LOGIN_PLATFORM_ACK, loginPlatformAck);
        MessageCenter.Instance.RemoveListener(MsgType.NET_ACCESSSERVICE_ACK, accessServiceAck);
        MessageCenter.Instance.RemoveListener(MsgType.NET_GET_PLAYER_ALL_BUFF_INFO, AllBuffAck);
    }


    //连接或断开服务器请求
    private int siteId = -1;     
    public void AccessServiceReq(int groupId, EnumAccessServiceType actionType, Action<string> action) {
        siteId = groupId;

        //if (EnumAccessServiceType.JoinGame == actionType)
        Debug.LogError(">>>>>AccessServiceReq:"+ groupId+","+ actionType);
        NetController.Instance.SendAccessServiceReq(groupId, actionType, action);
    }

    //连接或断开服务器回应
    private void accessServiceAck(Message msg)
    {
        
        var rsp = msg.Content as CLGTAccessServiceAck;
       Debug.LogError("accessServiceAck>>>0成功 1服务不存在 2拒绝访问,当前："+ rsp.errcode);
        switch (rsp.errcode)
        {
            case 0:
                var actionType = (EnumAccessServiceType)msg["ActionType"];
                //Debug.LogError("actionType:" + actionType);
                var action = (Action<string>)msg["Action"];
                var str = 1 == (int)actionType ? "加入" : "断开";
                //Debug.LogFormat($"{str} 服务组 {SysDefines.SiteId} 成功");
                switch (actionType) {
                    case EnumAccessServiceType.JoinGame:
                        SysDefines.SiteId = siteId;
                        siteId = -1;
                        action?.Invoke(rsp.game_data);
                        if (!string.IsNullOrEmpty(rsp.game_data)) {
                          //  Debug.Log("accessServiceAck game_data: " + rsp.game_data);
                            MessageCenter.Instance.SendMessage(MsgType.CLIENT_GAME_RECONNECTED, this, rsp.game_data);
                        }
                        break;
                    case EnumAccessServiceType.QuitGame:
                        action?.Invoke(null);
                        break;
                }
                break;
            default:
                GameController.Instance.CreateHintMessageByResponseProtocol(rsp);
                break;
        }
    }

    //连接或断开服务器回应
    //private void accessServiceAck(Message msg)
    //{
    //    var rsp = msg.Content as CLGTAccessServiceAck;
    //    switch (rsp.errcode)
    //    {
    //        case 0:
    //            var actionType = (EnumAccessServiceType)msg["ActionType"];
    //            switch (actionType)
    //            {
    //                case EnumAccessServiceType.JoinGame:
    //                    UIManager.Instance.OpenUI(EnumUIType.LoadingUI, true, EnumUIType.MainUI, EnumSceneType.MainScene);
    //                    SysDefines.SiteId = EnumSiteType.Fishing;
    //                    break;
    //                case EnumAccessServiceType.QuitGame:
    //                    NetController.Instance.SendLogoutReq();
    //                    break;
    //            }
    //            var str = 1 == (int)actionType ? "加入" : "断开";
    //            Debug.LogFormat($"{str} 服务组成功");
    //            break;
    //        default:
    //            GameController.Instance.CreateHintMessageByResponseProtocol(rsp);
    //            break;
    //    }
    //}

    //所有Buff回应
    private void AllBuffAck(Message msg)
    {
    }
    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    //登录回应
    private void loginPlatformAck(Message msg)
    {
        var rsp = msg.Content as CLGTLoginAck;
        Debug.Log("登录平台应答 0成功 1平台服务器不可用 2账号被封禁 3系统繁忙 4系统错误 5系统暂未开放,当前值：" +rsp.errcode);
        switch (rsp.errcode)
        {
            case 0:
                Debug.LogWarning("登录回应");
                if (SysDefines.LoginType == 4)
                    PlayerPrefs.SetString("WechatOpenId", SysDefines.LoginToken);
               

                sw.Stop();
                Debug.LogWarning($"登录回应,{sw.ElapsedMilliseconds}ms");

                CoroutineController.Instance.StartAliveCor();
                SysDefines.IsDisconnect = false;
                
                // UIManager.Instance.OpenUICloseOthers(EnumUIType.LoadingUI, true, EnumUIType.LobbyUI, EnumSceneType.MainScene);
                
                //登录成功后(包括断线重连),如果在小游戏中,查询是否有未结束的游戏,否则跳转主场景MaiUI,该UI启动时也会进行查询
                if (AppRoot.Get().curState == 1)
                {
                    
                }
                else
                {
                    UIManager.Instance.OpenUICloseOthers(EnumUIType.LoadingUI, true, EnumUIType.LobbyUI, EnumSceneType.MainScene);
                    //UIManager.Instance.OpenUI1(EnumUIType.FishingSelectUI);
                }
                GameController.Instance.Player = new GamePlayer(rsp);
                //临时屏蔽
                //GameController.Instance.RedisPlayer = new RedisPlayer();
                break;
            default:
                var hintMsg = ErrcodeHelper.GetText(rsp);
                Debug.LogError("登录回应===errcode:" + rsp.errcode+",extra_params：" + rsp.extra_params);
                if (rsp.errcode == 2 && !string.IsNullOrEmpty(rsp.extra_params))
                {
                    var paramsContainer = JObject.Parse(rsp.extra_params);
                    JToken token = paramsContainer["time_string"];
                    var str = "";
                    if (token != null)
                        str = token.ToString();
                    if (!string.IsNullOrEmpty(str))
                        hintMsg += $"，将于{str}解封";
                }
                else if (rsp.errcode == 5 && !string.IsNullOrEmpty(rsp.extra_params))
                {
                    var paramsContainer = JObject.Parse(rsp.extra_params);
                    JToken token = paramsContainer["time_string"];
                    var str = "";
                    if (token != null)
                        str = token.ToString();
                    if (!string.IsNullOrEmpty(str))
                        hintMsg += $"，将于{str}开放";
                }
                GameController.Instance.CreateHintMessage(hintMsg);
                break;
        }
    }

    //握手回应
    private void handAck(Message msg)
    {
        var rsp = msg.Content as CLGTHandAck;
        switch (rsp.errcode)
        {
            case 0:
                sw.Stop();
                Debug.LogWarning($"获取到随机密钥,{sw.ElapsedMilliseconds}ms");
                NetController.Instance.SendLoginPlatformReq(rsp.random_key);
                sw.Restart();
                break;
            default:
                GameController.Instance.CreateHintMessageByResponseProtocol(rsp);
                break;
        }
    }

    public void SendNetConnect(byte loginType)
    {
        //Debug.LogError("SendNetConnect:" + loginType);
        SysDefines.LoginType = loginType;
        PlayerPrefs.SetInt("LastLoginType", SysDefines.LoginType);

        sw.Start();
        NetController.Instance.GetIpPort(netConnect);
    }


    private void netConnect()
    {
        if (string.IsNullOrEmpty(SysDefines.Ip))
            GameController.Instance.CreateHintMessage("连接服务器出了点问题，请重新连接");
        else
            NetController.Instance.netComponent.connectWithTimeout(SysDefines.Ip, (int)SysDefines.Port, 5000);
    }

    //网络连接状态
    private void netConnectState(Message msg)
    {
        EnumNetConnectState state = (EnumNetConnectState)Enum.Parse(typeof(EnumNetConnectState), msg.Content.ToString(), true);
        string stateMsg = DateTime.Now.ToString("HH:mm:ss:fff");
        CoroutineController.Instance.StopReconnetCor(); //网络状态发生改变时取消正在执行的断线重连协程
        switch (state)
        {
            case EnumNetConnectState.Error:
                //与服务器建立tcp连接失败，这时需告知玩家，并返回登录界面和取消自动登录
                stateMsg = string.Format("[{0}]连接遇到错误!IP:{1},Port:{2}", stateMsg, SysDefines.Ip, SysDefines.Port);
                GameController.Instance.CreateHintMessage("连接服务器失败，请稍后再试！");
                if (SysDefines.SceneType != EnumSceneType.None && SysDefines.SceneType != EnumSceneType.LoginScene)
                    GameController.Instance.ReturnToLogin();
                else
                    MessageCenter.Instance.SendMessage(MsgType.CLIENT_CANCEL_AUTO_LOGIN, null);
                break;
            case EnumNetConnectState.Established:
                stateMsg = string.Format("[{0}]连接建立成功！IP:{1},Port:{2}", stateMsg, SysDefines.Ip, SysDefines.Port);
                sw.Stop();
                Debug.LogWarning($"连接建立成功,{sw.ElapsedMilliseconds}ms");

                NetController.Instance.SendHandReq();
                sw.Restart();
                break;
            case EnumNetConnectState.Disconnect:
                if (!SysDefines.IsDisconnect)
                {
                    //由于本地网络信号不好导致与服务器断开连接，这时告知玩家，并返回登录界面
                    SysDefines.IsDisconnect = true;
                    CoroutineController.Instance.StopAliveCor();
                    var hintMsg = "抱歉，与服务器连接被中断，请检查本地网络设置。";
                    if (SysDefines.SceneType != EnumSceneType.None && SysDefines.SceneType != EnumSceneType.LoginScene)
                        UIManager.Instance.OpenMessageBoxUI(hintMsg, 10, EnumMessageBoxType.OK,
                            obj => CoroutineController.Instance.StartReconnetCor());
                    else {
                        MessageCenter.Instance.SendMessage(MsgType.CLIENT_CANCEL_AUTO_LOGIN, null);
                        GameController.Instance.CreateHintMessage(hintMsg);
                    }
                }
                stateMsg = string.Format("[{0}]连接被断开！IP:{1},Port:{2}", stateMsg, SysDefines.Ip, SysDefines.Port);
                break;
            default:
                stateMsg = string.Format("[{0}]未知消息连接状态", stateMsg);
                break;
        }
        //Debug.Log(stateMsg);
    }

    //网络断开通知
    private void disconnectNtf(Message msg)
    {
        if (!SysDefines.IsDisconnect)
        {
            SysDefines.IsDisconnect = true;
            CoroutineController.Instance.StopAliveCor();
            var rsp = msg.Content as CLGTDisconnectNtf;
            var hintMsg = string.Empty;
            switch (rsp.code)
            {
                case 0:
                    if (SysDefines.SceneType != EnumSceneType.None && SysDefines.SceneType != EnumSceneType.LoginScene)
                        //玩家主动点击退出游戏按钮，返回登录界面
                        GameController.Instance.ReturnToLogin();
                    else
                        //登录过程中被中断，取消自动登录
                        MessageCenter.Instance.SendMessage(MsgType.CLIENT_CANCEL_AUTO_LOGIN, null);
                    break;
                default:
                    //意外被服务端踢下线，弹框提示，10秒钟返回登录界面
                    hintMsg = TLanguageHelper.DataMap.Values.ToList().Find(a => a.key == $"TextSystemPrompt_{rsp.code:d2}").CN;
                    UIManager.Instance.OpenMessageBoxUI(hintMsg, 10, EnumMessageBoxType.OK, GameController.Instance.ReturnToLogin);
                    break;
            }
        }
    }
}

