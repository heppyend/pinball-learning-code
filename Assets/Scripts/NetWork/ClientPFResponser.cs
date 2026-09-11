
using JBPROTO;
using UnityEngine;

public class ClientPFResponser : ClientPFResponserBase
{
    //玩家道具更新
    public override void onRecv_CLPFItemCountChangeNtf(CLPFItemCountChangeNtf proto)
    {
        // GameController.Instance.Player.SetItem(proto.items);
    }

    //资源强同步
    public override void onRecv_CLPFResSyncNtf(CLPFResSyncNtf proto)
    {
        //Debug.LogError("强烈执行种、、、");
        // GameController.Instance.Player.SyncResource(proto.diamond, proto.currency, proto.integral);
    }

    //玩家个人资源信息更新
    public override void onRecv_CLPFResChangedNtf(CLPFResChangedNtf proto)
    {
        if(proto.res_type==2)
            Debug.LogError("玩家个人资源信息更新,"+ proto.res_delta);
        //1 钻石 2 金币 3 绑金 4 积分 5 魔力
        Debug.LogError("玩家个人资源信息更新:" + proto.res_type);
        switch (proto.res_type)
        {
            case 1:
                //GameController.Instance.Player.SetDiamond(proto.res_value);
                break;
            case 2:
                //GameController.Instance.Player.SetCurrency(proto.res_value);
                break;
            case 3:
                break;
            case 4:
                break;
            //case 5:
            //    GameController.Instance.Player.DeltaMagicValue(proto.res_delta);
            //    break;
            case 7:
                break;
        }
    }
    

    //新邮件通知
    public override void onRecv_CLPFMailArriveNtf(CLPFMailArriveNtf proto)
    {
        MessageCenter.Instance.SendMessage(MsgType.NET_MAIL_ARRIVE_NTF, this, proto);
    }

    //客户端配置表变化通知
    public override void onRecv_CLPFClientConfigPublishNtf(CLPFClientConfigPublishNtf proto)
    {
        TableLoadHelper.LoadFromNet(SysDefines.OssUrl, proto.md5);
    }

    //开启活动通知
    public override void onRecv_CLPFActivitiesStartNtf(CLPFActivitiesStartNtf proto)
    {
        MessageCenter.Instance.SendMessage(MsgType.NET_ACTIVITIES_START_NTF, this, proto);
    }
}
