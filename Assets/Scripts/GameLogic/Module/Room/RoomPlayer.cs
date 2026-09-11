using JBPROTO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayer
{
    public int UserID;                          // 用户ID
    public string NickName;                     // 昵称
    public EnumPlayerGenderType Gender;         // 性别
    public int Head;                            // 头像
    public int HeadFrame;                       // 头像框
    public int title;                           // 称号
    public int dan;                             // 段位
    public int decorate;                        // 装饰
    public Int64 fight;                         // 战力

    public RoomPlayer(CLFRRoomPlayerInfo playerInfo)
    {
        Debug.LogError("初始化玩家");
        UserID = playerInfo.user_id;
        NickName = playerInfo.nickname;
        Gender = (EnumPlayerGenderType)playerInfo.gender;
        Head = playerInfo.head;
        HeadFrame = playerInfo.head_frame;
        title = playerInfo.title;
        dan = playerInfo.dan;
        decorate = playerInfo.decorate;
        //TODO: 协议待更新
        var roomType = TPinBallRoomHelper.GetRow(SysDefines.RoomConfigID).Type;
        // CannoId = roomType != 4 ? playerInfo.gun_id : 10;
    }
}
