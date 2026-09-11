using System.Collections;
using System.Collections.Generic;
using JBPROTO;
using UnityEngine;

public static class MarbleRoomController
{
    public static MarbleRoomModule GetRoomModule() => ModuleManager.Instance.Get<MarbleRoomModule>();
    
    public static void InitRoomModule()
    {
        var m = GetRoomModule();
        m.InitData();
    }
    
    //玩家加入通知
    public static void ProcessPlayerJoinNtf(CLFRPlayerJoinNtf ntf)
    {
        var m = GetRoomModule();
        m.AddPlayer(new RoomPlayer(ntf.player));
    }
    
    //表示客户端已经准备好接受数据ACk
    public static void ProcessGetReadyAck(CLFRGetReadyAck ack)
    {
        Debug.LogError("0成功 1系统错误，当前：" + ack.errcode);
        
        switch (ack.errcode)
        {
            case 0:
                Debug.LogError("房间内玩家数量：" + ack.player_len);
                var m = GetRoomModule();
                int playerNum = 0;
                //处理玩家
                foreach (var playerInfo in ack.players)
                {
                    playerNum++;
                    Debug.LogError($"房间内玩家{playerNum}ID：" + playerInfo.user_id);
                    Debug.LogError($"房间内玩家{playerNum}昵称：" + playerInfo.nickname);
                    Debug.LogError($"房间内玩家{playerNum}性别：" + playerInfo.gender);

                    if (playerInfo.user_id == GameController.Instance.Player.UserID)
                    {
                        m.AddPlayer(new RoomPlayer(playerInfo));
                        MessageCenter.Instance.SendMessage(MsgType.ROOM_PLAYER_MATCHING, null, ack,
                            new Dictionary<string, object>()
                            {
                                {"nickname", playerInfo.nickname},
                            });
                    }
                    else
                    {
                        m.AddPlayer(new RoomPlayer(playerInfo));
                    }
                }
                break;
            default:
                GameController.Instance.CreateHintMessageByResponseProtocol(ack);
                break;
        }
    }
    
    //房间状态通知
    public static void ProcessRoomStatusNtf(CLFRRoomStatusNtf ntf)
    { 
        var m = GetRoomModule();
        string mynickname = null;
        string othernickname = null;
        for (int i = 0; i < m.RoomPlayerList.Count; i++)
        {
            if (m.RoomPlayerList[i].UserID == GameController.Instance.Player.UserID)
            {
                mynickname = m.RoomPlayerList[i].NickName;
            }
            else
            {
                othernickname = m.RoomPlayerList[i].NickName;
            }
        }
        
        Debug.LogError("房间状态0:匹配状态 1:组队状态 2:战斗状态 3:结束状态,当前：" + ntf.status);
        MessageCenter.Instance.SendMessage(MsgType.ROOM_ALL_PLAYER_SET_READY, null, ntf,
            new Dictionary<string, object>()
            {
                {"othernickname", othernickname},
            });
        
        switch (ntf.status)
        {
            case 1:
                NetController.Instance.SendHeroTeamReq();
                break;
        }
        
    }
}
