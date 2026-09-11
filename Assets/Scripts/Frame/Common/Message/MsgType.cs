
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

public class MsgType
{
    #region client message
    
    //public const string CLIENT_GUILD_LIST_CLOSE             = "CLIENT_GUILD_LIST_CLOSE";            //加入/创建公会后关闭公会推荐列表
    public const string CLIENT_PAY_REQ                      = "CLIENT_PAY_REQ";                     //调用SDK支付请求
    public const string CLIENT_ACTIVITY_PAY_REQ             = "CLIENT_ACTIVITY_PAY_REQ";            //活动调用SDK支付请求
    public const string CLIENT_AUTHORIZE_REQ                = "CLIENT_AUTHORIZE_REQ";               //调用SDK授权请求
    public const string CLIENT_AUTHORIZE_ACK                = "CLIENT_AUTHORIZE_ACK";               //SDK授权回调
    public const string CLIENT_JSON_UPDATE_ACK              = "CLIENT_JSON_UPDATE_ACK";             //从线上更新配置完毕
    public const string CLIENT_CANCEL_AUTO_LOGIN            = "CLIENT_CANCEL_AUTO_LOGIN";           //取消自动登录
    public const string CLIENT_INNERGAME_OPENCLOSE          = "CLIENT_INNERGAME_OPENCLOSE";         //进出小游戏
    public const string CLIENT_GAME_RECONNECTED             = "CLIENT_GAME_RECONNECTED";            //游戏断线重连完成

    public const string CLIENT_ON_WECHAT_SHARE_URL = "CLIENT_ON_WECHAT_SHARE_URL";         //微信分享链接
    public const string CLIENT_ON_WECHAT_SHARE_SCREENSHOT = "CLIENT_ON_WECHAT_SHARE_SCREENSHOT";  //微信分享截屏
    #endregion

    #region net message
    public const string NET_RECEIVE_DATA                        = "NET_RECEIVE_DATA";                   // 网络接收到数据
    public const string NET_CONNECT                             = "NET_CONNECT";                        // 网络链接状态
    public const string NET_DISCONNECT_NTF                      = "NET_DISCONNECT_NTF";                 // 网络断开通知
    public const string NET_HAND_ACK                            = "NET_HAND_ACK";                       // 握手回应
    public const string NET_LOGIN_PLATFORM_ACK                  = "NET_LOGIN_PLATFORM_ACK";             // 登录大厅回应
    public const string NET_ACCESSSERVICE_ACK                   = "NET_ACCESSSERVICE_ACK";              // 连接或断开服务器回应
    public const string NET_ENTERSITE_ACK                       = "NET_ENTERSITE_ACK";                  // 加入玩法
    public const string NET_ENTERSERVER_NTF                     = "NET_ENTERSERVER_NTF";                // 登录服务器
    public const string NET_EXITSITE_ACK                        = "NET_EXITSITE_ACK";                   // 退出玩法
    public const string NET_MAIL_ARRIVE_NTF                     = "NET_MAIL_ARRIVE_NTF";                // 新邮件通知
    public const string NET_ACTIVITIES_START_NTF                = "NET_ACTIVITIES_START_NTF";           // 活动开启通知
    public const string NET_GET_PLAYER_ALL_BUFF_INFO            = "NET_GET_PLAYER_ALL_BUFF_INFO";       // 查询所有Buff回
    
    public const string NET_GET_HERO_INFO                       = "NET_GET_HERO_INFO";                  // 通用获取英雄信息
    public const string NET_UI_GET_HERO_INFO                    = "NET_UI_GET_HERO_INFO";               // UI获取英雄信息
    public const string NET_CONTROL_GET_HERO_INFO               = "NET_CONTROL_GET_HERO_INFO";          // 控制获取英雄信息
    public const string NET_GET_HERO_INFO_CALLBACK              = "NET_GET_HERO_INFO_CALLBACK";         // 获取英雄信息回调
    public const string NET_GET_USERNICKNAME                    = "NET_GET_USERNICKNAME";               // 获取用户昵称 
    #endregion

    #region 非网络

    public const string LOCAL_SEND_ENEMYCOUNTCHECK              = "LOCAL_SEND_ENEMYCOUNTCHECK";         // 检查敌人数量
    public const string LOCAL_SEND_WHOSE_TURN                   = "LOCAL_SEND_WHOSE_TURN";              // 通知切换回合
    public const string LOCAL_SEND_WHOSE_TURN_CALLBACK          = "LOCAL_SEND_WHOSE_TURN_CALLBACK";     // 切换回合回调
    public const string LOCAL_SEND_CURRENTSTAGECOUNT            = "LOCAL_SEND_CURRENTSTAGECOUNT";       // 当前关卡数
    public const string LOCAL_SEND_ADDSTAGECOUNT                = "LOCAL_SEND_ADDSTAGECOUNT";           // 增加关卡数
    public const string LOCAL_SEND_MODIFYHEALTH                 = "LOCAL_SEND_MODIFYHEALTH";            // 修改生命值
    public const string LOCAL_SEND_SETHEALTH                    = "LOCAL_SEND_SETHEALTH";               // 设置生命值
    public const string LOCAL_SEND_ADDROUNDCOUNT                = "LOCAL_SEND_ADDROUNDCOUNT";           // 增加回合数
    public const string LOCAL_SEND_ADDENEMYCOUNT                = "LOCAL_SEND_ADDENEMYCOUNT";           // 增加敌人数
    public const string LOCAL_SEND_SUBENEMYCOUNT                = "LOCAL_SEND_SUBENEMYCOUNT";           // 减少敌人数
    public const string LOCAL_SEND_RESETMARBLE                  = "LOCAL_SEND_RESETMARBLE";             // 重置弹珠
    public const string LOCAL_SEND_LEVELCHECK                   = "LOCAL_SEND_LEVELCHECK";              // 关卡检测
    public const string LOCAL_SEND_LEVELCHECK_CALLBACK          = "LOCAL_SEND_LEVELCHECK_CALLBACK";     // 关卡检测回调
    public const string LOCAL_SEND_ADDCOMBOCOUNT                = "LOCAL_SEND_ADDCOMBOCOUNT";           // 增加连击数
    public const string LOCAL_SEND_ADDCOMBOCOUNT_CALLBACK       = "LOCAL_SEND_ADDCOMBOCOUNT_CALLBACK";  // 增加连击数回调
    public const string LOCAL_SEND_SHOWMARBLESELECT             = "LOCAL_SEND_SHOWMARBLESELECT";        // 显示弹珠选择
    public const string LOCAL_SEND_CLOSEMARBLESELECT            = "LOCAL_SEND_CLOSEMARBLESELECT";       // 关闭弹珠选择
    public const string LOCAL_SEND_ADDMARBLEENERGY              = "LOCAL_SEND_ADDMARBLEENERGY";         // 增加弹珠能量
    public const string LOCAL_SEND_ADDMARBLEENERGY_CALLBACK     = "LOCAL_SEND_ADDMARBLEENERGY_CALLBACK";// 增加弹珠能量回调
    public const string LOCAL_SEND_ADDEXPERIENCE                = "LOCAL_SEND_ADDEXPERIENCE";           // 增加经验
    public const string LOCAL_SEND_ADDEXPERIENCE_CALLBACK       = "LOCAL_SEND_ADDEXPERIENCE_CALLBACK";  // 增加经验回调
    public const string LOCAL_SEND_MODIFYSKILLCD                = "LOCAL_SEND_MODIFYSKILLCD";           // 修改技能CD
    public const string LOCAL_SEND_USEULTIMATE                  = "LOCAL_SEND_USEULTIMATE";             // 使用绝招
    public const string LOCAL_SEND_BUFFADDENEMYHEALTH           = "LOCAL_SEND_BUFFADDENEMYHEALTH";      // Buff增加敌人生命值
    public const string LOCAL_SEND_GAMESETTLEMENT               = "LOCAL_SEND_GAMESETTLEMENT";          // 游戏结算
    public const string LOCAL_SEND_MODIFYCOUNTDOWN              = "LOCAL_SEND_MODIFYCOUNTDOWN";         // 修改倒计时
    
    #endregion

    #region room message
    public const string ROOM_PLAYER_JOIN                    = "ROOM_PLAYER_JOIN";                   //玩家加入
    public const string ROOM_PLAYER_LEAVE                   = "ROOM_PLAYER_LEAVE";                  //玩家离开
    
    public const string ROOM_ALL_PLAYER_SET_READY           = "ROOM_ALL_PLAYER_SET_READY";          //进入渔场时所有玩家数据加载完毕
    public const string ROOM_PLAYER_ACT                     = "ROOM_PLAYER_ACT";                    //玩家操作行为
    public const string ROOM_PLAYER_MATCHING                = "ROOM_PLAYER_MATCHING";               //玩家操作行为
    #endregion

    public const string CSHARP_RECEIVE_DATA                 = "CSHARP_RECEIVE_DATA";                //所有C#需转发至Lua的消息
}