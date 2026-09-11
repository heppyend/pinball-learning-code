
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
using UnityEngine;

public class ClientFishingRoomResponser : ClientFishingRoomResponserBase
{
    // 玩家加入通知
    public override void onRecv_CLFRPlayerJoinNtf(CLFRPlayerJoinNtf proto)
    {
        MarbleRoomController.ProcessPlayerJoinNtf(proto);
    }
    
    // 房间状态通知
    public override void onRecv_CLFRRoomStatusNtf(CLFRRoomStatusNtf proto)
    {
        MarbleRoomController.ProcessRoomStatusNtf(proto);
    }
}