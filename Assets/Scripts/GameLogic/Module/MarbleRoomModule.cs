using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarbleRoomModule : BaseModule
{
    public MarbleRoomModule()
    {
        AutoRegister = true;
        InitData();
    }
    
    //玩家列表
    public List<RoomPlayer> RoomPlayerList;
    public RoomPlayer Playerself;

    public void InitData()
    {
        RoomPlayerList = new List<RoomPlayer>();
        Playerself = null;
    }
    
    protected override void OnLoad()
    {
        
    }


    protected override void OnRelease()
    {
        
    }
    
    // 通过用户ID获取玩家
    public RoomPlayer GetPlayerByuserId(int userId)
    {
        return RoomPlayerList.Find(a => a.UserID == userId);
    }
    
    public void AddPlayer(RoomPlayer player)
    {
        RoomPlayerList.Add(player);

        if (player.UserID == GameController.Instance.Player.UserID)
        {
            // Debug.LogError("AddPlayer Playerself = 赋值,"+ player.ServerSeat);
            Playerself = player;
            lastPlayerActTimestamp = TimeHelper.GetServerTimestamp();
            MessageCenter.Instance.AddListener(MsgType.ROOM_PLAYER_ACT, onPlayerAct);
        }

        MessageCenter.Instance.SendMessage(MsgType.ROOM_PLAYER_JOIN, this, null, new Dictionary<string, object>()
        {
            {"Player", player}
        });
    }
    
    private ulong lastPlayerActTimestamp = 0;
    private void onPlayerAct(Message msg)
    {
        lastPlayerActTimestamp = TimeHelper.GetServerTimestamp();
    }
}
