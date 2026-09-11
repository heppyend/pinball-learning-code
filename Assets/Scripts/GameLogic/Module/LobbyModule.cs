using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JBPROTO;
using UnityEngine;

public class LobbyModule : BaseModule
{
    public LobbyModule()
    {
        AutoRegister = true;
    }
    
    protected override void OnLoad()
    {
        MessageCenter.Instance.AddListener(MsgType.NET_ENTERSITE_ACK, enterSiteAck);
        MessageCenter.Instance.AddListener(MsgType.NET_EXITSITE_ACK, exitSiteAck);
    }


    protected override void OnRelease()
    {
        MessageCenter.Instance.RemoveListener(MsgType.NET_ENTERSITE_ACK, enterSiteAck);
        MessageCenter.Instance.RemoveListener(MsgType.NET_EXITSITE_ACK, exitSiteAck);
    }
    
    #region 加入退出玩法
    //加入玩法
    private void enterSiteAck(Message msg)
    {
        SysDefines.RoomConfigID = 1;
        var configID = TPinBallRoomHelper.GetRow(SysDefines.RoomConfigID).Id;
        
        Debug.LogError("加入玩法," + configID);

        var rsp = msg.Content as CLFMEnterSiteAck;
        switch (rsp.errcode)
        {
            case 0:
                var siteId = (EnumSiteType)msg["SiteId"];
                var roomId = (int)msg["RoomId"];
                var seatId = (int)msg["SeatId"];
                var password = (int)msg["Password"];
                Action<string> action = (Action<string>)msg["Action"];
                
                switch (siteId)
                {
                    case EnumSiteType.None:
                        break;
                    case EnumSiteType.Fishing:
                        NetController.Instance.SendEnterFishingGameReq(configID, roomId, seatId, password, action);
                        break;
                }
                break;
            default:
                GameController.Instance.CreateHintMessageByResponseProtocol(rsp);
                break;
        }
    }

    //退出玩法
    private void exitSiteAck(Message msg)
    {
        var rsp = msg.Content as CLFMExitSiteAck;
        var reason = int.Parse(msg["Reason"].ToString());
        if (reason == 1)
        {
            
        }
        else
        {
            switch (rsp.errcode)
            {
                case 0:
                    var siteId = (EnumSiteType)msg["SiteId"];
                    Debug.LogError("退出！");
                    ModuleManager.Instance.Get<LoginModule>().AccessServiceReq(SysDefines.GroupId_Fish, EnumAccessServiceType.QuitGame, null);
                    break;
                default:
                    GameController.Instance.CreateHintMessageByResponseProtocol(rsp);
                    break;
            }
        }
    }
    #endregion
}
