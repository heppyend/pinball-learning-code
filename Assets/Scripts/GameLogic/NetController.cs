
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
using UnityEngine;
using JBPROTO;
using System.Collections.Generic;
using QL.Core;
using QL.Protocol;
using System.Threading.Tasks;
using QL.Parser;
using UnityEngine.Networking;

public class NetController : DDOLSingleton<NetController> 
{
    private const string WebGLDeviceIdentifier = "WebGLMiniGameDeviceIdentifierPinballNode";

    private static string GetLoginDeviceIdentifier()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return WebGLDeviceIdentifier;
#else
        return SystemInfo.deviceUniqueIdentifier;
#endif
    }


    private DefaultQLClient webClient;
    public DefaultQLClient WebClient
    {
        get
        {
            if (null == webClient)
            {
                webClient = new DefaultQLClient("client", "IHtrvnloNHfelF5a");

                //如果区服Id小于0，修改为测试环境url
                int zoneId = SysDefines.ZoneId;
                if (zoneId < 0)
                {
                   // webClient.ServerUrl = "http://api2.qq1798.com:8000/router/rest";
                    webClient.ServerUrl = "http://0.0.0.0:8000/router/rest";
                }
            }
            return webClient;
        }
    }

    public INetComponent netComponent;

    private void Awake()
    {
        netComponent = NetHelper.createNetComponent(new NetReactor());
        netComponent.addResponser(new ClientGTResponser());
        netComponent.addResponser(new ClientFishingMainResponser());
        netComponent.addResponser(new ClientFishingRoomResponser());
        netComponent.addResponser(new ClientPFResponser());
    }

    private void Update()
    {
        netComponent.run();
    }

    #region 与web通信
    //获取热更下载地址
    public void GetDownloadUrl(Action<string> onGetUrl)
    {
        ClientGetDownloadUrlRequest webReq = new ClientGetDownloadUrlRequest();
        webReq.ZoneId = SysDefines.ZoneId;
        asyncExecuteWebRequest(webReq, webRsp =>
        {
            if (webRsp.IsError)
            {
                Debug.LogError($"获取下载地址失败：{webRsp.ErrMsg}");
                //TODO: 最好将错误信息提示出来
            }
            else
            {
                SysDefines.OssUrl = webRsp.OssUrl;
                //SysDefines.OssUrl = "https://game.hofoo.top/";
                SysDefines.PopularizeUrl = webRsp.PopularizeUrl;
                onGetUrl?.Invoke(webRsp.ClientConfigPkgMd5);
            }
        });
    }

    /// <summary>
    /// 获取连接IP 与 Port
    /// </summary>
    /// <param name="connectGame">回调函数</param>
    public void GetIpPort(Action connectGame)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        Debug.LogWarning("GetIpPort");
        Debug.LogWarning($"GetIpPort,ZoneId={SysDefines.ZoneId}");

#if UNITY_WEBGL && !UNITY_EDITOR
        StartCoroutine(GetIpPortWithUnityWebRequest(connectGame, sw));
#else
        ClientGetGateConnectionRequest webReq = new ClientGetGateConnectionRequest();
        webReq.ZoneId = SysDefines.ZoneId;
        sw.Start();
        asyncExecuteWebRequest(webReq, webRsp =>
        {
            if (webRsp.IsError)
            {
                Debug.LogError($"获取网关连接方式失败：{webRsp.ErrMsg}");
                //TODO: 最好将错误信息提示出来
            }
            else
            {
                SysDefines.Ip = webRsp.Ip;
                SysDefines.Port = webRsp.Port;

                // 打印IP和端口
                Debug.Log($"获取到网关连接信息 - IP: {SysDefines.Ip}, Port: {SysDefines.Port}");

                connectGame?.Invoke();

                sw.Stop();
                Debug.LogWarning($"GetIpPort,{sw.ElapsedMilliseconds}ms");
                //Debug.LogFormat("ip: {0}, port: {1}", SysDefines.Ip, SysDefines.Port);
            }
        });
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    /// <summary>
    /// 微信小游戏不能使用旧 QLWebClient 的 HttpWebRequest/Task.Run 链路。
    /// 这里保留同一 API、参数、签名和 JSON/XML 返回格式，只替换传输实现。
    /// </summary>
    private IEnumerator GetIpPortWithUnityWebRequest(Action connectGame, System.Diagnostics.Stopwatch sw)
    {
        ClientGetGateConnectionRequest webReq = new ClientGetGateConnectionRequest
        {
            ZoneId = SysDefines.ZoneId
        };

        DefaultQLClient client = WebClient;
        if (string.IsNullOrWhiteSpace(client.ServerUrl))
        {
            Debug.LogError("获取网关连接方式失败：ServerUrl 为空");
            yield break;
        }

        QLDictionary parameters = new QLDictionary(webReq.GetParameters());
        parameters.Add(DefaultQLClient.METHOD, webReq.GetApiName());
        parameters.Add(DefaultQLClient.SIGN_METHOD, QLConstants.SIGN_METHOD_MD5);
        parameters.Add(DefaultQLClient.APP_KEY, client.Key);
        parameters.Add(DefaultQLClient.FORMAT, client.Format.ToString());
        parameters.Add(DefaultQLClient.SDK_VERSION, DefaultQLClient.SDK_VERSION_VALUE);
        parameters.Add(DefaultQLClient.TIMESTAMP, DateTime.Now);
        parameters.Add(DefaultQLClient.SESSION, null);
        parameters.Add(DefaultQLClient.SIGN, QLUtil.SignRequestByMd5Method(parameters, client.Password));

        sw.Start();
        using (UnityWebRequest request = UnityWebRequest.Post(client.ServerUrl, parameters))
        {
            request.timeout = Math.Max(1, client.Timeout / 1000);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"获取网关连接方式失败：{request.error}");
                yield break;
            }

            ClientGetGateConnectionResponse response;
            try
            {
                response = client.Format == QLResponseFormat.Xml
                    ? new QLXmlParser().Parse<ClientGetGateConnectionResponse>(request.downloadHandler.text)
                    : new QLJsonParser().Parse<ClientGetGateConnectionResponse>(request.downloadHandler.text);
            }
            catch (Exception exception)
            {
                Debug.LogError($"解析网关连接方式失败：{exception.Message}");
                yield break;
            }

            if (response == null || response.IsError)
            {
                Debug.LogError($"获取网关连接方式失败：{response?.ErrMsg ?? "返回为空"}");
                yield break;
            }

            SysDefines.Ip = response.Ip;
            SysDefines.Port = response.Port;
            Debug.Log($"已通过 UnityWebRequest 获取 TCP 网关地址：IP={SysDefines.Ip}, Port={SysDefines.Port}");
            connectGame?.Invoke();
        }

        sw.Stop();
        Debug.LogWarning($"GetIpPort,{sw.ElapsedMilliseconds}ms");
    }
#endif

    //获取渠道应用信息
    public void GetChannelAppInfo(Action<string, string, string, string> callback)
    {
        ClientChannelLoginGetAppInfoRequest webReq = new ClientChannelLoginGetAppInfoRequest
        {
            ChannelId = SysDefines.ChannelId,
            ZoneId = SysDefines.ZoneId
        };
        asyncExecuteWebRequest(webReq, webRsp => {
            if (webRsp.IsError)
            {
                Debug.LogError($"获取渠道应用信息失败：{webRsp.ErrMsg}");
                GameController.Instance.CreateHintMessage(webRsp.ErrMsg);
            }
            else
            {
                callback?.Invoke(webRsp.AppId, webRsp.AppKey, webRsp.AppSecret, webRsp.Extend);
            }
        });
    }

    //短信验证
    public void SendSmsSendPhoneCode(string phone)
    {
        ClientSmsSendPhoneCodeRequest webReq = new ClientSmsSendPhoneCodeRequest();
        webReq.ZoneId = (long)SysDefines.ZoneId;
        webReq.Phone = phone;
        webReq.SmsChannel = SysDefines.SmsChannel;
        asyncExecuteWebRequest(webReq, webRsp =>
        {
            if (webRsp.IsError)
            {
                GameController.Instance.CreateHintMessage(webRsp.ErrMsg);
            }
        });
    }

    //微信授权登录获取appid
    public void GetWechatAppid(Action<string, string> callback)
    {
        ClientWxLoginGetAppInfoRequest webReq = new ClientWxLoginGetAppInfoRequest();
        webReq.ZoneId = SysDefines.ZoneId;
        asyncExecuteWebRequest(webReq, webRsp =>
        {
            if (webRsp.IsError)
            {
                Debug.LogError($"获取微信登录AppId失败：{webRsp.ErrMsg}");
                GameController.Instance.CreateHintMessage(webRsp.ErrMsg);
            }
            else
            {
                callback?.Invoke(webRsp.AppId, webRsp.Scope);
            }
        });
    }

    //微信授权登录code交换openid
    public void GetWechatOpenId(string code, Action<string> callback)
    {
        ClientWxLoginGetOpenIdRequest webReq = new ClientWxLoginGetOpenIdRequest();
        webReq.ZoneId = SysDefines.ZoneId;
        webReq.Code = code;
        asyncExecuteWebRequest(webReq, webRsp =>
        {
            if (webRsp.IsError)
            {
                Debug.LogError($"获取微信登录OpenId失败：{webRsp.ErrMsg}");
                GameController.Instance.CreateHintMessage(webRsp.ErrMsg);
            }
            else
            {
                callback?.Invoke(webRsp.OpenId);
            }
        });
    }

    //异步执行web请求
    private async void asyncExecuteWebRequest<T>(IQLRequest<T> request, Action<T> callback) where T : QLResponse
    {
        T webRsp = await Task.Run(() => WebClient.Execute(request));
        callback?.Invoke(webRsp);
    }

    #endregion
    /// <summary>
    /// 握手协议
    /// </summary>
    public void SendHandReq()
    {
        Debug.LogWarning("requ=======握手协议");

        CLGTHandReq req = new CLGTHandReq();
        req.platform = SysDefines.Platform;
        req.product = 1;
        req.version = SysDefines.Version;
        req.device = GetLoginDeviceIdentifier();
        req.channel = "com.game.fishing.android";
        req.country = "ZH-CN";
        req.language = "CN";
        netComponent.asyncRequest<CLGTHandAck>(req, rsp => 
        {
            MessageCenter.Instance.SendMessage(MsgType.NET_HAND_ACK, this, rsp);
        });
    }

    /// <summary>
    /// 登录大厅请求
    /// </summary>
    /// <param name="randomKey">随机密钥</param>
    public void SendLoginPlatformReq(int randomKey)
    {
        Debug.LogWarning("requ=======登录大厅请求");
        string deviceIdentifier = GetLoginDeviceIdentifier();
        Debug.LogWarning($"游客登录 device：固定值，长度：{deviceIdentifier.Length}");

        CLGTLoginReq req = new CLGTLoginReq();
        req.login_type = SysDefines.LoginType;
        var expendToken = SysDefines.OpeninstallToken;
        //Debug.Log("expendToken:" + expendToken);
        switch (SysDefines.LoginType)
        {
            case 1:
                //req.token = UnityHelper.CA3Encode("zhubin000", randomKey);
                req.token = UnityHelper.CA3Encode($"{deviceIdentifier/*+"1"*/},{expendToken}", randomKey);
                break;
            case 2:
                req.token = UnityHelper.CA3Encode($"{SysDefines.LoginToken},{expendToken}", randomKey);
                break;
            case 4:
                req.token = UnityHelper.CA3Encode($"{SysDefines.LoginToken},{expendToken}", randomKey);
                break;
            default:
                return;
        }
        netComponent.asyncRequestWithLock<CLGTLoginAck>(req, rsp =>
         {
             Debug.LogError("玩家登录ID：" + rsp.user_id);
             Debug.LogError("玩家昵称：" + rsp.nickname);
             SysDefines.UserNickName = rsp.nickname;
             MessageCenter.Instance.SendMessage(MsgType.NET_LOGIN_PLATFORM_ACK, this, rsp);

             //打开app到登录完成需要一定时间，这段时间内有可能发布了客户端配置表
             //为了在这种情况下也能加载到最新配置，这时调用一下LoadFromNet ^_^
             //下面还是先注释掉吧，2019年7月13日11:01:30 client_config_md5参数需要从json串里查找
             //if (rsp.errcode == 0)
             //{
             //    TableLoadHelper.LoadFromNet(SysDefines.OssUrl, rsp.client_config_md5, null);
             //}
         });
    }
    
    /// <summary>
    /// 加入玩法请求
    /// </summary>
    /// <param name="">游戏玩法的ID</param>
    public void SendEnterSiteReq(int siteId, int roomId = -1, int seatId = -1, int password = -1, Action<string> action=null)
    {
        Debug.LogError("requ=======SendEnterSiteReq");

        //Debug.LogError("SendEnterSiteReq,siteId=" + siteId);
        //清理上次渔场内的ack协议缓存
        netComponent.clearWaitingResponse<JBPROTO.CLFREnterGameAck>();
        netComponent.clearWaitingResponse<JBPROTO.CLFREnterGameWithPasswordAck>();
        netComponent.clearWaitingResponse<JBPROTO.CLFRExitGameAck>();
        netComponent.clearWaitingResponse<JBPROTO.CLFRGetReadyAck>();
        netComponent.clearWaitingResponse<JBPROTO.CLFRShootAck>();
        netComponent.clearWaitingResponse<JBPROTO.CLFRHitAck>();

        //发送进入渔场协议
        CLFMEnterSiteReq req = new CLFMEnterSiteReq();
        Debug.LogError("发送进入渔场协议");
        req.site_id = siteId;
        netComponent.asyncRequestWithLock<CLFMEnterSiteAck>(req, rsp =>
        {
            Debug.LogError("0成功 1无可用服务器 2系统错误" + rsp.errcode);
            MessageCenter.Instance.SendMessage(MsgType.NET_ENTERSITE_ACK, this, rsp, new Dictionary<string, object>()
            {
                { "SiteId", siteId },
                { "RoomId", roomId },
                { "SeatId", seatId },
                { "Password", password },
                {"Action", action}
            });
        });
    }
    
    /// <summary>
    /// 进入游戏
    /// </summary>
    /// <param name="configID">配置的房间号</param>
    public void SendEnterFishingGameReq(int configID, int roomId, int seatId, int password = -1, Action<string> action = null)
    {
        Debug.LogError("requ=======SendEnterFishingGameReq");

        CLFREnterGameWithPasswordReq req = new CLFREnterGameWithPasswordReq();
        req.config_id = configID;
        req.room_id = roomId;
        req.seat_id = seatId;
        req.password = password;
        netComponent.asyncRequestWithLock<CLFREnterGameWithPasswordAck>(req, rsp =>
        {
            switch (rsp.errcode)
            {
                case 0: 
                    Debug.Log("已进入桌子");
                    SendGetReadyReq();
                    break;
                case 1:
                    Debug.LogError("条件不足尚未解锁");
                    break;
                case 2:
                    Debug.LogError("房间未开放");
                    break;
                case 3:
                    Debug.LogError("房间已满");
                    break;
                case 4:
                    Debug.LogError("指定的座位已被占用");
                    break;
                case 5:
                    Debug.LogError("密码错误");
                    break;
                case 6:
                    Debug.LogError("房间状态有误");
                    break;
                case 7:
                    Debug.LogError("系统错误");
                    break;
            }
        });
    }
    
    // 英雄组队请求
    public void SendHeroTeamReq(Action action = null)
    {
        Debug.LogError("requ=======英雄组队请求");

        CLFRHeroTeamReq req = new CLFRHeroTeamReq();
        req.hero_id_len = CLFRHeroTeamReq.hero_ids_max_length;
        int[] heroIds = new int[CLFRHeroTeamReq.hero_ids_max_length];
        for (int i = 0; i < heroIds.Length; i++)
        {
            heroIds[i] = 0;
        }
        req.hero_ids = heroIds;
        netComponent.asyncRequestWithLock<CLFRHeroTeamAck>(req, rsp =>
        {
            Debug.LogError("组队状态0成功 1英雄ID有误 2系统错误，当前：" + rsp.errcode);
            Debug.LogError("总血量" + rsp.total_hp);
            action?.Invoke();
        });
    }
    /// <summary>
    /// 登出大厅请求
    /// </summary>
    public void SendLogoutReq()
    {

        Debug.LogError("requ=======登出大厅请求");

        CLPFLogoutReq req = new CLPFLogoutReq();
        netComponent.send(req);
    }

    /// <summary>
    /// 连接或断开服务器请求
    /// </summary>
    /// <param name="groupId">服务组Id</param>
    /// <param name="actionType">加入或者离开服务组</param>
    /// <param name="action">是否有回调</param>
    public void SendAccessServiceReq(int groupId, EnumAccessServiceType actionType, Action<string> action) {

        Debug.LogError("requ=======连接或断开服务器请求");

        CLGTAccessServiceReq req = new CLGTAccessServiceReq();
        req.group_id = groupId;
        req.action = (int)actionType;
        Debug.LogError("链接请求！");
        netComponent.asyncRequestWithLock<CLGTAccessServiceAck>(req, rsp => {
            Debug.LogError("请求返回！");
            Debug.LogError("0成功 1服务不存在 2拒绝访问,当前：" + rsp.errcode);
            MessageCenter.Instance.SendMessage(MsgType.NET_ACCESSSERVICE_ACK, this, rsp, new Dictionary<string, object>() {
                {"ActionType", actionType},
                {"Action", action}
            });
        });
    }

    /// <summary>
    /// 退出玩法请求
    /// </summary>
    /// <param name="siteId">游戏玩法的ID</param>
    /// <param name="reason">退出原因 0正常渔场退出 1选座失败</param>
    public void SendExitSiteReq(int siteId, int reason = 0)
    {
        Debug.LogError("requ=======SendExitSiteReq");

        CLFMExitSiteReq req = new CLFMExitSiteReq();
        req.site_id = siteId;
        netComponent.asyncRequestWithLock<CLFMExitSiteAck>(req, rsp =>
        {
            MessageCenter.Instance.SendMessage(MsgType.NET_EXITSITE_ACK, this, rsp, new Dictionary<string, object>()
            {
                {"SiteId", siteId},
                {"Reason", reason},
            });
        });
    }

    /// <summary>
    /// 退出捕鱼房间
    /// </summary>
    public void SendExitGameReq(Action action)
    {
        Debug.LogError("requ=======SendExitGameReq");

        CLFRExitGameReq req = new CLFRExitGameReq();
        netComponent.asyncRequestWithLock<CLFRExitGameAck>(req, rsp =>
        {
            
        });
    }

    #region 数据获取
    
    /// <summary>
    /// 获取英雄数据请求
    /// </summary>
    public void SendGetHeroReq(string msgType ,Action action = null)
    {
        CLPFGetHeroReq req = new CLPFGetHeroReq();
        Debug.LogError("发起获取英雄数据请求" + ">>>>");
        netComponent.asyncRequest<CLPFGetHeroAck>(req, rsp =>
        {
            Debug.LogError("请求返回..." + msgType);
            MessageCenter.Instance.SendMessage(msgType, this, rsp);
        });
    }
    
    /// <summary>
    /// 批量获取英雄信息请求
    /// </summary>
    public void SendGetHeroInfoReq(Action action = null)
    {
        PFGSGetHeroInfoReq req = new PFGSGetHeroInfoReq();
        Debug.LogError("发起批量获取英雄信息请求" + ">>>>");
        netComponent.asyncRequest<PFGSGetHeroInfoAck>(req, rsp =>
        {
            Debug.LogError("批量获取英雄信息请求返回...");
            for (int i = 0; i < rsp.heros.Length; i++)
            {
                Debug.LogError($"英雄{i}ID：" + rsp.heros[i].heroId);
                Debug.LogError($"英雄{i}星级：" + rsp.heros[i].star);
                Debug.LogError($"英雄{i}武器：" + rsp.heros[i].weapon);
                Debug.LogError($"英雄{i}头盔：" + rsp.heros[i].helmet);
                Debug.LogError($"英雄{i}衣服：" + rsp.heros[i].clothes);
                Debug.LogError($"英雄{i}裤子：" + rsp.heros[i].trousers);
                Debug.LogError($"英雄{i}鞋子：" + rsp.heros[i].shoe);
            }
        });
    }
    
    /// <summary>
    /// 查询玩家昵称请求
    /// </summary>
    public void SendPlayerNicknameQueryReq()
    {
        CLPFPlayerNicknameQueryReq req = new CLPFPlayerNicknameQueryReq();
        netComponent.asyncRequest<CLPFPlayerNicknameQueryAck>(req, rsp =>
        {
            Debug.LogError("查询玩家昵称请求返回..." + rsp.nickname);
            MessageCenter.Instance.SendMessage(MsgType.NET_GET_USERNICKNAME , this, rsp);
        });
    }
    
    /// <summary>
    /// 修改用户昵称请求
    /// </summary>
    public void SendModifyNicknameReq()
    {
        CLPFModifyNicknameReq req = new CLPFModifyNicknameReq();
        req.new_nickname = "玩家昵称七七";
        netComponent.asyncRequest<CLPFModifyNicknameAck>(req, rsp =>
        {
            Debug.LogError("修改用户昵称请求返回...0成功 1格式不合法 2包含敏感字符 3昵称已存在 4钻石不足 : " + rsp.errcode);
        });
    }
    
    /// <summary>
    /// 表示客户端已准备可以接收服务器的推送
    /// </summary>
    public void SendGetReadyReq()
    {
        Debug.LogError("requ=======SendGetReadyReq发送表示客户端已准备可以接收服务器的推送");

        CLFRGetReadyReq req = new CLFRGetReadyReq();
        netComponent.asyncRequest<CLFRGetReadyAck>(req, rsp =>
        {
            MarbleRoomController.ProcessGetReadyAck(rsp);
        });
    }
    
    #endregion
    
    /// <summary>
    /// 心跳包
    /// </summary>
    public IEnumerator SendTKeepAlive()
    {
        while (true)
        {
            yield return new WaitForSeconds(10.0f);
            CLGTKeepAliveReq alive = new CLGTKeepAliveReq();
            netComponent.send(alive);
        }
    }

    /// <summary>
    /// 断线重连
    /// </summary>
    public IEnumerator TryReconnet() {
        while (true) {
            ModuleManager.Instance.Get<LoginModule>().SendNetConnect(SysDefines.LoginType);
            yield return new WaitForSeconds(8.0f);
        }
    }
    
    public class NetComponent
    {
        public void asyncRequest<T>(object req, Action<T> callback)
        {
            // 模拟异步请求
            Task.Run(() =>
            {
                var rsp = Activator.CreateInstance<T>();
                callback(rsp);
            });
        }
    }
}
