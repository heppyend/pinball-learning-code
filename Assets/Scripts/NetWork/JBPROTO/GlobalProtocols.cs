using System;
using System.IO;

namespace JBPROTO
{
    /// <summary>
    /// 云端配置发生变化通知
    /// </summary>
    public sealed class GlobalTableConfigChangedNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 0;
        /// <summary>
        /// 分类标识
        /// </summary>
        public string oss_key;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, oss_key, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                oss_key = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 流控配置更新通知
    /// </summary>
    public sealed class GlobalFlowControlConfigChangedNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 1;
        /// <summary>
        /// 最新流控配置
        /// </summary>
        public string config_string;  //max:4096

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, config_string, 4096);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                config_string = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 发送邮件请求
    /// </summary>
    public sealed class GlobalMailSendReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 2;
        /// <summary>
        /// 1发送给所有玩家 2发送给指定玩家
        /// </summary>
        public int send_type;
        /// <summary>
        /// 邮件类型 1系统邮件 2赠送邮件
        /// </summary>
        public int type;
        /// <summary>
        /// 发送者Id 0代表系统邮件 1代表后台邮件
        /// </summary>
        public int sender;
        /// <summary>
        /// 接受者Id
        /// </summary>
        public int receiver;
        /// <summary>
        /// 标题 json多语言
        /// </summary>
        public string title;  //max:1024
        /// <summary>
        /// 内容 json多语言
        /// </summary>
        public string content;  //max:4096
        /// <summary>
        /// 附件物品 json格式
        /// </summary>
        public string items;  //max:1024
        /// <summary>
        /// 有效期，单位秒
        /// </summary>
        public int duration;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(send_type);
            bw.Write(type);
            bw.Write(sender);
            bw.Write(receiver);
            NetHelper.SafeWriteString(bw, title, 1024);
            NetHelper.SafeWriteString(bw, content, 4096);
            NetHelper.SafeWriteString(bw, items, 1024);
            bw.Write(duration);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                send_type = br.ReadInt32();
                type = br.ReadInt32();
                sender = br.ReadInt32();
                receiver = br.ReadInt32();
                title = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                content = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                items = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                duration = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 发送邮件回应
    /// </summary>
    public sealed class GlobalMailSendAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 3;
        /// <summary>
        /// 0成功 1接收方不存在 2系统错误
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 公告变化通知
    /// </summary>
    public sealed class GlobalAnnouncementChangedNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 4;
        /// <summary>
        /// 区服Id，0代表所有区服
        /// </summary>
        public int zone_id;
        /// <summary>
        /// 0公告 1客服
        /// </summary>
        public sbyte content_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(zone_id);
            bw.Write(content_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                zone_id = br.ReadInt32();
                content_type = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 添加封禁请求
    /// </summary>
    public sealed class GlobalBlockAccountAddReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 5;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 封禁类型 1永久封禁 2限时封禁
        /// </summary>
        public int block_type;
        /// <summary>
        /// 封禁时长，单位秒
        /// </summary>
        public int block_duration;
        /// <summary>
        /// 封禁原因
        /// </summary>
        public string operation_reason;  //max:1024
        /// <summary>
        /// 操作人
        /// </summary>
        public string operater;  //max:255

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(block_type);
            bw.Write(block_duration);
            NetHelper.SafeWriteString(bw, operation_reason, 1024);
            NetHelper.SafeWriteString(bw, operater, 255);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                block_type = br.ReadInt32();
                block_duration = br.ReadInt32();
                operation_reason = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                operater = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 添加封禁账号回应
    /// </summary>
    public sealed class GlobalBlockAccountAddAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 6;
        /// <summary>
        /// 0成功 1玩家不存在
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 解除封禁请求
    /// </summary>
    public sealed class GlobalBlockAccountRemoveReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 7;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 封禁原因
        /// </summary>
        public string operation_reason;  //max:256
        /// <summary>
        /// 操作人
        /// </summary>
        public string operater;  //max:255

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            NetHelper.SafeWriteString(bw, operation_reason, 256);
            NetHelper.SafeWriteString(bw, operater, 255);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                operation_reason = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                operater = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 解除封禁回应
    /// </summary>
    public sealed class GlobalBlockAccountRemoveAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 8;
        /// <summary>
        /// 0成功 1该账号不在封禁列表中
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 生成订单请求
    /// </summary>
    public sealed class GlobalPayGenerateOrderReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 9;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 订单类型 1微信支付 2支付宝 3支付猫微信 4支付猫支付宝 5聚合微信 6聚合支付宝 7线下支付
        /// </summary>
        public int order_type;
        /// <summary>
        /// 订单金额 单位分
        /// </summary>
        public int order_amount;
        /// <summary>
        /// 商品类型 1商城
        /// </summary>
        public int content_type;
        /// <summary>
        /// 商品Id
        /// </summary>
        public int content_id;
        /// <summary>
        /// 终端IP地址
        /// </summary>
        public string terminal_ip;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(order_type);
            bw.Write(order_amount);
            bw.Write(content_type);
            bw.Write(content_id);
            NetHelper.SafeWriteString(bw, terminal_ip, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                order_type = br.ReadInt32();
                order_amount = br.ReadInt32();
                content_type = br.ReadInt32();
                content_id = br.ReadInt32();
                terminal_ip = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 生成订单回应
    /// </summary>
    public sealed class GlobalPayGenerateOrderAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 10;
        /// <summary>
        /// 0成功 1不支持的支付渠道 2非法的订单金额 3无法找到商户配置信息 4系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 错误码描述信息
        /// </summary>
        public string code_desc;  //max:1024
        /// <summary>
        /// 内部唯一订单编号
        /// </summary>
        public string order_no;  //max:32
        /// <summary>
        /// 订单环境参数 ，用于前端发起sdk调用 ，json格式字符串
        /// </summary>
        public string order_envir;  //max:4096

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            NetHelper.SafeWriteString(bw, code_desc, 1024);
            NetHelper.SafeWriteString(bw, order_no, 32);
            NetHelper.SafeWriteString(bw, order_envir, 4096);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                code_desc = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                order_no = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                order_envir = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 订单完成通知
    /// </summary>
    public sealed class GlobalPayOrderFinishNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 11;
        /// <summary>
        /// 内部订单号
        /// </summary>
        public string order_no;  //max:32
        /// <summary>
        /// 是否欺骗完成 1是0否
        /// </summary>
        public int is_cheat;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, order_no, 32);
            bw.Write(is_cheat);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                order_no = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                is_cheat = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询微信用户信息请求
    /// </summary>
    public sealed class GlobalWxUserQueryInfoReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 12;
        /// <summary>
        /// 微信openid
        /// </summary>
        public string open_id;  //max:64

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, open_id, 64);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                open_id = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询微信用户信息回应
    /// </summary>
    public sealed class GlobalWxUserQueryInfoAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 13;
        /// <summary>
        /// 0成功 1用户不存在
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 微信昵称
        /// </summary>
        public string nickname;  //max:32
        /// <summary>
        /// 性别 0未知 1男 2女
        /// </summary>
        public int sex;
        /// <summary>
        /// 头像url地址
        /// </summary>
        public string head_img_url;  //max:255

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            NetHelper.SafeWriteString(bw, nickname, 32);
            bw.Write(sex);
            NetHelper.SafeWriteString(bw, head_img_url, 255);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                nickname = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                sex = br.ReadInt32();
                head_img_url = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 设置玩家个人游戏难度请求
    /// </summary>
    public sealed class GlobalUserGameDifficultySetReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 14;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 游戏难度等级012、456
        /// </summary>
        public int difficulty_level;
        /// <summary>
        /// 总输赢金币数量
        /// </summary>
        public Int64 total_winlost;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(difficulty_level);
            bw.Write(total_winlost);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                difficulty_level = br.ReadInt32();
                total_winlost = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 设置玩家个人游戏难度回应
    /// </summary>
    public sealed class GlobalUserGameDifficultySetAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 15;
        /// <summary>
        /// 0成功 1用户不存在
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家关键数据附加信息
    /// </summary>
    public sealed class GlobalUserCriticalExtraData : INetProtocol
    {
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 强控个人难度等级
        /// </summary>
        public sbyte strong_difficulty_level;
        /// <summary>
        /// 剩余总输赢
        /// </summary>
        public Int64 strong_difficulty_winlost;
        /// <summary>
        /// 个人奖池当前值
        /// </summary>
        public Int64 personal_current_prize_pool;
        /// <summary>
        /// 个人奖池上限
        /// </summary>
        public Int64 personal_max_prize_pool;
        /// <summary>
        /// 回血点数
        /// </summary>
        public int personal_recover_point;
        /// <summary>
        /// 是否处于回血状态 1是0否
        /// </summary>
        public sbyte personal_in_recover_state;
        /// <summary>
        /// 自控个人难度等级
        /// </summary>
        public sbyte personal_difficulty_level;
        /// <summary>
        /// 分红临时奖池
        /// </summary>
        public Int64 personal_share_bonus;
        /// <summary>
        /// 附加字段信息
        /// </summary>
        public string extra_string;  //max:1024

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(user_id);
            bw.Write(strong_difficulty_level);
            bw.Write(strong_difficulty_winlost);
            bw.Write(personal_current_prize_pool);
            bw.Write(personal_max_prize_pool);
            bw.Write(personal_recover_point);
            bw.Write(personal_in_recover_state);
            bw.Write(personal_difficulty_level);
            bw.Write(personal_share_bonus);
            NetHelper.SafeWriteString(bw, extra_string, 1024);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                strong_difficulty_level = br.ReadSByte();
                strong_difficulty_winlost = br.ReadInt64();
                personal_current_prize_pool = br.ReadInt64();
                personal_max_prize_pool = br.ReadInt64();
                personal_recover_point = br.ReadInt32();
                personal_in_recover_state = br.ReadSByte();
                personal_difficulty_level = br.ReadSByte();
                personal_share_bonus = br.ReadInt64();
                extra_string = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家关键数据信息请求
    /// </summary>
    public sealed class GlobalUserCriticalDataQueryReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 16;
        /// <summary>
        /// 玩家Id数组用逗号隔开
        /// </summary>
        public string user_id_array;  //max:1024

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, user_id_array, 1024);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id_array = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家关键数据信息回应
    /// </summary>
    public sealed class GlobalUserCriticalDataQueryAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 17;
        /// <summary>
        /// 玩家信息数组，分隔符,|
        /// </summary>
        public string player_info_array;  //max:4096
        /// <summary>
        /// 数组长度
        /// </summary>
        public sbyte data_length;
        /// <summary>
        /// 数据数组
        /// </summary>
        public GlobalUserCriticalExtraData[] data_array;  //max:50
        /// <summary>
        /// 数据数组（最大长度）
        /// </summary>
        public const int data_array_max_length = 50;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, player_info_array, 4096);
            bw.Write(data_length);
            if (data_length > data_array_max_length)
                throw new Exception($"GlobalUserCriticalDataQueryAck.data_array数组长度超过规定限制，期望:50 实际:{data_length}");
            for (int i = 0; i < (int)data_length; i++)
                data_array[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                player_info_array = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                data_length = br.ReadSByte();
                data_array = new GlobalUserCriticalExtraData[(int)data_length];
                for (int i = 0; i < data_array.Length; i++)
                {
                    data_array[i] = new GlobalUserCriticalExtraData();
                    data_array[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 客户端配置表发布通知
    /// </summary>
    public sealed class GlobalClientConfigPublishNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 18;
        /// <summary>
        /// 最新配置文件的md5
        /// </summary>
        public string md5;  //max:40

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, md5, 40);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                md5 = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取房间服务器Id列表请求
    /// </summary>
    public sealed class GlobalGameRoomGetServerIdsReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 19;
        /// <summary>
        /// 玩法Id 1捕鱼3D 2捕鱼2D
        /// </summary>
        public int site_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(site_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                site_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取房间服务器Id列表回应
    /// </summary>
    public sealed class GlobalGameRoomGetServerIdsAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 20;
        /// <summary>
        /// 房间服务器Id串，用逗号隔开
        /// </summary>
        public string server_ids;  //max:256

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, server_ids, 256);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                server_ids = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取捕鱼房间服务器Id所对应的AppId请求
    /// </summary>
    public sealed class GlobalGameRoomGetAppIdReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 21;
        /// <summary>
        /// 玩法Id 1捕鱼3D 2捕鱼2D
        /// </summary>
        public int site_id;
        /// <summary>
        /// 服务器Id
        /// </summary>
        public int server_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(site_id);
            bw.Write(server_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                site_id = br.ReadInt32();
                server_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取捕鱼房间服务器Id所对应的AppId回应
    /// </summary>
    public sealed class GlobalGameRoomGetAppIdAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 22;
        /// <summary>
        /// 应用程序Id
        /// </summary>
        public string app_id;  //max:64

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, app_id, 64);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                app_id = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 房间设置难度请求
    /// </summary>
    public sealed class GlobalGameRoomSetDifficultyReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 23;
        /// <summary>
        /// 房间配置Id
        /// </summary>
        public int config_id;
        /// <summary>
        /// 台子Id
        /// </summary>
        public int platform_id;
        /// <summary>
        /// 难度级别
        /// </summary>
        public int difficulty_level;
        /// <summary>
        /// 操作员
        /// </summary>
        public string oprater;  //max:255

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(config_id);
            bw.Write(platform_id);
            bw.Write(difficulty_level);
            NetHelper.SafeWriteString(bw, oprater, 255);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                config_id = br.ReadInt32();
                platform_id = br.ReadInt32();
                difficulty_level = br.ReadInt32();
                oprater = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 房间设置难度回应
    /// </summary>
    public sealed class GlobalGameRoomSetDifficultyAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 24;
        /// <summary>
        /// 0成功 1房间配置不存在 2台子不存在 3操作员非法
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 房间设置参数请求
    /// </summary>
    public sealed class GlobalGameRoomSetParamReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 25;
        /// <summary>
        /// 房间配置Id
        /// </summary>
        public int config_id;
        /// <summary>
        /// 台子Id
        /// </summary>
        public int platform_id;
        /// <summary>
        /// 1水位 2彩金池
        /// </summary>
        public int action;
        /// <summary>
        /// 修改值 大于0放水 小于0抽水
        /// </summary>
        public Int64 delta;
        /// <summary>
        /// 操作员
        /// </summary>
        public string oprater;  //max:255

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(config_id);
            bw.Write(platform_id);
            bw.Write(action);
            bw.Write(delta);
            NetHelper.SafeWriteString(bw, oprater, 255);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                config_id = br.ReadInt32();
                platform_id = br.ReadInt32();
                action = br.ReadInt32();
                delta = br.ReadInt64();
                oprater = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 房间设置难度回应
    /// </summary>
    public sealed class GlobalGameRoomSetParamAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 26;
        /// <summary>
        /// 0成功 1房间配置不存在 2台子不存在 3操作员非法 4参数不对 5彩金池额度不足
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 区服开放状态变化通知
    /// </summary>
    public sealed class GlobalZoneConfigOpenStateChangedNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 27;
        /// <summary>
        /// 开放状态 1开放0封闭
        /// </summary>
        public int is_open;
        /// <summary>
        /// 开放时间，该字段仅在设置封闭时有效，格式：yyyy-MM-dd HH:mm:ss
        /// </summary>
        public string open_time;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(is_open);
            NetHelper.SafeWriteString(bw, open_time, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                is_open = br.ReadInt32();
                open_time = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 区服版本更新配置变化通知
    /// </summary>
    public sealed class GlobalZoneConfigVersionUpdateChangedNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 28;
        /// <summary>
        /// 版本更新配置信息，格式：渠道,允许的最低版本号|...
        /// </summary>
        public string config_value;  //max:4096

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, config_value, 4096);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                config_value = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 区服信息变化通知
    /// </summary>
    public sealed class GlobalZoneConfigInfoChangedNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 29;
        /// <summary>
        /// 信息类型 1ip黑名单 2设备黑名单 3ip白名单 4设备白名单 5账号白名单
        /// </summary>
        public int info_type;
        /// <summary>
        /// 1添加 2移除
        /// </summary>
        public sbyte action;
        /// <summary>
        /// 内容
        /// </summary>
        public string info_value;  //max:64

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(info_type);
            bw.Write(action);
            NetHelper.SafeWriteString(bw, info_value, 64);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                info_type = br.ReadInt32();
                action = br.ReadSByte();
                info_value = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 机器mysql自动备份配置变更通知
    /// </summary>
    public sealed class GlobalMachineMysqlAutoBackupConfigChangedNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 30;
        /// <summary>
        /// 配置字符串
        /// </summary>
        public string config_string;  //max:4096

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, config_string, 4096);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                config_string = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 公会解散请求
    /// </summary>
    public sealed class GlobalGuildDismissReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 31;
        /// <summary>
        /// 公会Id
        /// </summary>
        public int guild_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(guild_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                guild_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 公会解散回应
    /// </summary>
    public sealed class GlobalGuildDismissAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 32;
        /// <summary>
        /// 0成功 1公会不存在
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 短信验证码校验请求
    /// </summary>
    public sealed class GlobalPhoneSmsCodeCheckReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 33;
        /// <summary>
        /// 手机号码
        /// </summary>
        public string phone;  //max:16
        /// <summary>
        /// 短信验证的AppKey
        /// </summary>
        public string sms_app_key;  //max:64
        /// <summary>
        /// 短信验证的区号
        /// </summary>
        public string sms_zone;  //max:10
        /// <summary>
        /// 短信验证码
        /// </summary>
        public string sms_code;  //max:10
        /// <summary>
        /// 0Mob渠道 1其他渠道
        /// </summary>
        public int sms_channel;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, phone, 16);
            NetHelper.SafeWriteString(bw, sms_app_key, 64);
            NetHelper.SafeWriteString(bw, sms_zone, 10);
            NetHelper.SafeWriteString(bw, sms_code, 10);
            bw.Write(sms_channel);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                phone = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                sms_app_key = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                sms_zone = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                sms_code = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                sms_channel = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 短信验证码校验回应
    /// </summary>
    public sealed class GlobalPhoneSmsCodeCheckAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 34;
        /// <summary>
        /// 0成功 1失败
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 公会设置显示级别通知
    /// </summary>
    public sealed class GlobalGuildSetDisplayLevelNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 35;
        /// <summary>
        /// 公会Id
        /// </summary>
        public int guild_id;
        /// <summary>
        /// 公会推荐列表显示级别 0默认 1已屏蔽 2已上榜 3已置顶
        /// </summary>
        public int display_level;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(guild_id);
            bw.Write(display_level);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                guild_id = br.ReadInt32();
                display_level = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 设置玩家商人请求
    /// </summary>
    public sealed class GlobalUserBusinessmanSetReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 36;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 是否成为商人 1是0否
        /// </summary>
        public sbyte businessman;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(businessman);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                businessman = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 设置玩家商人回应
    /// </summary>
    public sealed class GlobalUserBusinessmanSetAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 37;
        /// <summary>
        /// 0成功 1玩家不存在
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 添加跑马灯消息请求
    /// </summary>
    public sealed class GlobalBroadcastMessageAddReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 38;
        /// <summary>
        /// 消息内容
        /// </summary>
        public string content;  //max:255
        /// <summary>
        /// 播报间隔
        /// </summary>
        public int interval;
        /// <summary>
        /// 总时长
        /// </summary>
        public int duration;
        /// <summary>
        /// 操作人员
        /// </summary>
        public string operater;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, content, 255);
            bw.Write(interval);
            bw.Write(duration);
            NetHelper.SafeWriteString(bw, operater, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                content = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                interval = br.ReadInt32();
                duration = br.ReadInt32();
                operater = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 添加跑马灯消息回应
    /// </summary>
    public sealed class GlobalBroadcastMessageAddAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 39;
        /// <summary>
        /// 0成功 1失败
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 移除跑马灯消息请求
    /// </summary>
    public sealed class GlobalBroadcastMessageRemoveReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 40;
        /// <summary>
        /// 消息Id
        /// </summary>
        public int id;
        /// <summary>
        /// 操作人员
        /// </summary>
        public string operater;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(id);
            NetHelper.SafeWriteString(bw, operater, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                id = br.ReadInt32();
                operater = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 移除跑马灯消息回应
    /// </summary>
    public sealed class GlobalBroadcastMessageRemoveAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 41;
        /// <summary>
        /// 0成功 1失败
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 增加玩家经验请求
    /// </summary>
    public sealed class GlobalUserAddVipExpReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 42;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 玩家vip经验
        /// </summary>
        public int vip_exp_addition;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(vip_exp_addition);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                vip_exp_addition = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 增加玩家经验回应
    /// </summary>
    public sealed class GlobalUserAddVipExpAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 43;
        /// <summary>
        /// 0成功 1参数不合法 2玩家不存在
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 子账号贡献信息结构
    /// </summary>
    public sealed class GlobalAgentContributionInfo : INetProtocol
    {
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 注册时间戳
        /// </summary>
        public UInt32 register_time;
        /// <summary>
        /// 昵称
        /// </summary>
        public string nickname;  //max:32
        /// <summary>
        /// 充值总额，单位分
        /// </summary>
        public int total_recharge;
        /// <summary>
        /// 总贡献，客户端显示时先除以100
        /// </summary>
        public int total_contribution;
        /// <summary>
        /// 当天充值，单位分
        /// </summary>
        public int today_recharge;
        /// <summary>
        /// 当天贡献，客户端显示时先除以100
        /// </summary>
        public int today_contribution;
        /// <summary>
        /// 代理级别
        /// </summary>
        public int agent_level;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(user_id);
            bw.Write(register_time);
            NetHelper.SafeWriteString(bw, nickname, 32);
            bw.Write(total_recharge);
            bw.Write(total_contribution);
            bw.Write(today_recharge);
            bw.Write(today_contribution);
            bw.Write(agent_level);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                register_time = br.ReadUInt32();
                nickname = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                total_recharge = br.ReadInt32();
                total_contribution = br.ReadInt32();
                today_recharge = br.ReadInt32();
                today_contribution = br.ReadInt32();
                agent_level = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询子账号的贡献列表请求
    /// </summary>
    public sealed class GlobalAgentQueryContributionListReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 44;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 查询第几页？
        /// </summary>
        public int page_index;
        /// <summary>
        /// 每页多少纪录？
        /// </summary>
        public int page_count;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(page_index);
            bw.Write(page_count);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                page_index = br.ReadInt32();
                page_count = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询子账号的贡献列表回应
    /// </summary>
    public sealed class GlobalAgentQueryContributionListAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 45;
        /// <summary>
        /// 0成功 1玩家不存在 2全民代理尚未开放
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 一共多少条纪录
        /// </summary>
        public int total_count;
        /// <summary>
        /// 子账号贡献数组长度
        /// </summary>
        public int data_length;
        /// <summary>
        /// 子账号贡献数组
        /// </summary>
        public GlobalAgentContributionInfo[] data_array;  //max:100
        /// <summary>
        /// 子账号贡献数组（最大长度）
        /// </summary>
        public const int data_array_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(total_count);
            bw.Write(data_length);
            if (data_length > data_array_max_length)
                throw new Exception($"GlobalAgentQueryContributionListAck.data_array数组长度超过规定限制，期望:100 实际:{data_length}");
            for (int i = 0; i < (int)data_length; i++)
                data_array[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                total_count = br.ReadInt32();
                data_length = br.ReadInt32();
                data_array = new GlobalAgentContributionInfo[(int)data_length];
                for (int i = 0; i < data_array.Length; i++)
                {
                    data_array[i] = new GlobalAgentContributionInfo();
                    data_array[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 后台操作账号绑定手机请求
    /// </summary>
    public sealed class GlobalAccountBindPhoneReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 46;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 绑定的手机号
        /// </summary>
        public string phone;  //max:16
        /// <summary>
        /// 登录密码，明文
        /// </summary>
        public string password;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            NetHelper.SafeWriteString(bw, phone, 16);
            NetHelper.SafeWriteString(bw, password, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                phone = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                password = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 后台操作账号绑定手机回应
    /// </summary>
    public sealed class GlobalAccountBindPhoneAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 47;
        /// <summary>
        /// 0成功 1玩家不存在 2新手机号已关联其他账号 3参数错误
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 后台操作修改金库密码请求
    /// </summary>
    public sealed class GlobalAccountResetBankPasswordReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 48;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 新的金库密码
        /// </summary>
        public string password;  //max:20

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            NetHelper.SafeWriteString(bw, password, 20);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                password = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 后台操作修改金库密码回应
    /// </summary>
    public sealed class GlobalAccountResetBankPasswordAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 49;
        /// <summary>
        /// 0成功 1玩家不存在 2玩家尚未绑定手机号 3参数错误
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 后台踢某玩家下线请求
    /// </summary>
    public sealed class GlobalAccountKickoffReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 50;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 后台踢某玩家下线回应
    /// </summary>
    public sealed class GlobalAccountKickoffAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 51;
        /// <summary>
        /// 0成功
        /// </summary>
        public sbyte user_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 设置游戏难度请求
    /// </summary>
    public sealed class GlobalGameDifficultySetReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 52;
        /// <summary>
        /// 游戏组Id
        /// </summary>
        public int group_id;
        /// <summary>
        /// 游戏玩法Id
        /// </summary>
        public int service_id;
        /// <summary>
        /// 游戏难度等级0123456
        /// </summary>
        public int difficulty_level;
        /// <summary>
        /// 操作人
        /// </summary>
        public string operater;  //max:64

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(group_id);
            bw.Write(service_id);
            bw.Write(difficulty_level);
            NetHelper.SafeWriteString(bw, operater, 64);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                group_id = br.ReadInt32();
                service_id = br.ReadInt32();
                difficulty_level = br.ReadInt32();
                operater = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 设置游戏难度回应
    /// </summary>
    public sealed class GlobalGameDifficultySetAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 53;
        /// <summary>
        /// 0成功 1参数错误
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询全局信息请求
    /// </summary>
    public sealed class GlobalGlobalInformationQueryReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 54;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
        }
        public void fromBinary(BinaryReader br)
        {
        }
    }
    /// <summary>
    /// 查询全局信息回应
    /// </summary>
    public sealed class GlobalGlobalInformationQueryAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 55;
        /// <summary>
        /// 全局奖池水位线
        /// </summary>
        public Int64 prize_pool;
        /// <summary>
        /// 全局游戏难度
        /// </summary>
        public int difficulty_level;
        /// <summary>
        /// 上次抽水时间戳
        /// </summary>
        public UInt32 last_fetch_time;
        /// <summary>
        /// 上次分红时间戳
        /// </summary>
        public UInt32 last_share_time;
        /// <summary>
        /// 战力全局奖池水位线
        /// </summary>
        public Int64 fight_prize_pool;
        /// <summary>
        /// 战力全局游戏难度
        /// </summary>
        public int fight_difficulty_level;
        /// <summary>
        /// 战力上次抽水时间戳
        /// </summary>
        public UInt32 fight_last_fetch_time;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(prize_pool);
            bw.Write(difficulty_level);
            bw.Write(last_fetch_time);
            bw.Write(last_share_time);
            bw.Write(fight_prize_pool);
            bw.Write(fight_difficulty_level);
            bw.Write(fight_last_fetch_time);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                prize_pool = br.ReadInt64();
                difficulty_level = br.ReadInt32();
                last_fetch_time = br.ReadUInt32();
                last_share_time = br.ReadUInt32();
                fight_prize_pool = br.ReadInt64();
                fight_difficulty_level = br.ReadInt32();
                fight_last_fetch_time = br.ReadUInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 全局奖池抽放水请求
    /// </summary>
    public sealed class GlobalPrizePoolWaterLineAddReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 56;
        /// <summary>
        /// 水位抽放水值
        /// </summary>
        public Int64 delta;
        /// <summary>
        /// 操作员
        /// </summary>
        public string operater;  //max:32
        /// <summary>
        /// 类型 0金币池 1乱斗池
        /// </summary>
        public int type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(delta);
            NetHelper.SafeWriteString(bw, operater, 32);
            bw.Write(type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                delta = br.ReadInt64();
                operater = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 全局奖池抽放水回应
    /// </summary>
    public sealed class GlobalPrizePoolWaterLineAddAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 57;
        /// <summary>
        /// 0成功 1参数错误
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 立即分红请求
    /// </summary>
    public sealed class GlobalPrizePoolShareBonusImmediateReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 58;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
        }
        public void fromBinary(BinaryReader br)
        {
        }
    }
    /// <summary>
    /// 立即分红回应
    /// </summary>
    public sealed class GlobalPrizePoolShareBonusImmediateAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 59;
        /// <summary>
        /// 0成功 1奖池不足，无法分红
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家个人奖池调整请求
    /// </summary>
    public sealed class GlobalPlayerPrizePoolAddReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 60;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 个人奖池上限修改值
        /// </summary>
        public Int64 delta;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(delta);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                delta = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家个人奖池调整回应
    /// </summary>
    public sealed class GlobalPlayerPrizePoolAddAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 61;
        /// <summary>
        /// 0成功 1玩家不存在
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家资源调整请求
    /// </summary>
    public sealed class GlobalPlayerResourceAddReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 62;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 资源类型 1玩家携带 2玩家仓库
        /// </summary>
        public int res_type;
        /// <summary>
        /// 物品主类型
        /// </summary>
        public int item_id;
        /// <summary>
        /// 物品子类型
        /// </summary>
        public int item_sub_id;
        /// <summary>
        /// 物品增加或减少的量
        /// </summary>
        public Int64 delta;
        /// <summary>
        /// 操作员
        /// </summary>
        public string operater;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(res_type);
            bw.Write(item_id);
            bw.Write(item_sub_id);
            bw.Write(delta);
            NetHelper.SafeWriteString(bw, operater, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                res_type = br.ReadInt32();
                item_id = br.ReadInt32();
                item_sub_id = br.ReadInt32();
                delta = br.ReadInt64();
                operater = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家资源调整回应
    /// </summary>
    public sealed class GlobalPlayerResourceAddAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 63;
        /// <summary>
        /// 0成功或部分成功 1玩家不存在 2参数错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 实际调整生效的量
        /// </summary>
        public Int64 affect_delta;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(affect_delta);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                affect_delta = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 修改玩家昵称请求
    /// </summary>
    public sealed class GlobalPlayerNicknameModifyReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 64;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 新昵称
        /// </summary>
        public string new_nickname;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            NetHelper.SafeWriteString(bw, new_nickname, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                new_nickname = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 修改玩家昵称回应
    /// </summary>
    public sealed class GlobalPlayerNicknameModifyAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 65;
        /// <summary>
        /// 0成功 1玩家不存在 2昵称不合法
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 重置玩家昵称修改次数请求
    /// </summary>
    public sealed class GlobalPlayerNicknameModifyCountResetReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 66;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 重置玩家昵称修改次数回应
    /// </summary>
    public sealed class GlobalPlayerNicknameModifyCountResetAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 67;
        /// <summary>
        /// 0成功 1玩家不存在
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 金库物品赠送请求
    /// </summary>
    public sealed class GlobalBankItemSendReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 68;
        /// <summary>
        /// 源玩家Id
        /// </summary>
        public int source_user_id;
        /// <summary>
        /// 目标玩家Id
        /// </summary>
        public int dest_user_id;
        /// <summary>
        /// 赠送的物品Id
        /// </summary>
        public int item_id;
        /// <summary>
        /// 赠送的物品子Id
        /// </summary>
        public int item_sub_id;
        /// <summary>
        /// 赠送的物品数量
        /// </summary>
        public Int64 item_count;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(source_user_id);
            bw.Write(dest_user_id);
            bw.Write(item_id);
            bw.Write(item_sub_id);
            bw.Write(item_count);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                source_user_id = br.ReadInt32();
                dest_user_id = br.ReadInt32();
                item_id = br.ReadInt32();
                item_sub_id = br.ReadInt32();
                item_count = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 金库物品赠送回应
    /// </summary>
    public sealed class GlobalBankItemSendAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 69;
        /// <summary>
        /// 0成功 1参数无效 2源玩家不存在 3目标玩家不存在 4资源数量不足
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 关闭服务通知
    /// </summary>
    public sealed class GlobalStopRunningNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 70;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
        }
        public void fromBinary(BinaryReader br)
        {
        }
    }
    /// <summary>
    /// 启动服务请求
    /// </summary>
    public sealed class GlobalStartInstanceReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 71;
        /// <summary>
        /// 应用程序模板下载地址
        /// </summary>
        public string demo_url;  //max:256
        /// <summary>
        /// 最后修改时间，用作编译时间
        /// </summary>
        public string last_modify_time;  //max:32
        /// <summary>
        /// 中心url地址
        /// </summary>
        public string center_url;  //max:256
        /// <summary>
        /// 应用key
        /// </summary>
        public string app_key;  //max:64
        /// <summary>
        /// 应用秘钥
        /// </summary>
        public string app_secret;  //max:64
        /// <summary>
        /// 应用秘钥串
        /// </summary>
        public string app_key_chain;  //max:64

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, demo_url, 256);
            NetHelper.SafeWriteString(bw, last_modify_time, 32);
            NetHelper.SafeWriteString(bw, center_url, 256);
            NetHelper.SafeWriteString(bw, app_key, 64);
            NetHelper.SafeWriteString(bw, app_secret, 64);
            NetHelper.SafeWriteString(bw, app_key_chain, 64);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                demo_url = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                last_modify_time = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                center_url = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                app_key = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                app_secret = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                app_key_chain = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 启动服务回应
    /// </summary>
    public sealed class GlobalStartInstanceAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 72;
        /// <summary>
        /// 0成功 1错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 错误信息
        /// </summary>
        public string err_message;  //max:256
        /// <summary>
        /// 控制台内容
        /// </summary>
        public string console_content;  //max:4096

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            NetHelper.SafeWriteString(bw, err_message, 256);
            NetHelper.SafeWriteString(bw, console_content, 4096);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                err_message = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                console_content = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 设置玩家贵客属性请求
    /// </summary>
    public sealed class GlobalUserHonorableSetReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 73;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 是否成为贵客 1是0否
        /// </summary>
        public sbyte honorable;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(honorable);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                honorable = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 设置玩家贵客属性回应
    /// </summary>
    public sealed class GlobalUserHonorableSetAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 74;
        /// <summary>
        /// 0成功 1玩家不存在
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询渠道用户信息请求
    /// </summary>
    public sealed class GlobalChannelUserQueryInfoReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 75;
        /// <summary>
        /// 渠道Id
        /// </summary>
        public int channel_id;
        /// <summary>
        /// 渠道用户唯一Id
        /// </summary>
        public string unique_token;  //max:64

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(channel_id);
            NetHelper.SafeWriteString(bw, unique_token, 64);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                channel_id = br.ReadInt32();
                unique_token = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询渠道用户信息回应
    /// </summary>
    public sealed class GlobalChannelUserQueryInfoAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 76;
        /// <summary>
        /// 0成功 1用户不存在
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 性别 0未知 1男 2女
        /// </summary>
        public int sex;
        /// <summary>
        /// 昵称
        /// </summary>
        public string name;  //max:32
        /// <summary>
        /// 头像url地址
        /// </summary>
        public string head_img_url;  //max:255

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(sex);
            NetHelper.SafeWriteString(bw, name, 32);
            NetHelper.SafeWriteString(bw, head_img_url, 255);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                sex = br.ReadInt32();
                name = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                head_img_url = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 金库赠送订单审核请求
    /// </summary>
    public sealed class GlobalBankItemSendExamineReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 77;
        /// <summary>
        /// 订单Id
        /// </summary>
        public int order_id;
        /// <summary>
        /// 操作 0不通过 1通过
        /// </summary>
        public sbyte act;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(order_id);
            bw.Write(act);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                order_id = br.ReadInt32();
                act = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 金库赠送订单审核回应
    /// </summary>
    public sealed class GlobalBankItemSendExamineAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 78;
        /// <summary>
        /// 0成功 1失败
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 商人金库物品赠送请求
    /// </summary>
    public sealed class GlobalBankItemBusinessmanSendReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 79;
        /// <summary>
        /// 源玩家Id
        /// </summary>
        public int source_user_id;
        /// <summary>
        /// 目标玩家Id
        /// </summary>
        public int dest_user_id;
        /// <summary>
        /// 赠送的物品Id
        /// </summary>
        public int item_id;
        /// <summary>
        /// 赠送的物品子Id
        /// </summary>
        public int item_sub_id;
        /// <summary>
        /// 赠送的物品数量
        /// </summary>
        public Int64 item_count;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(source_user_id);
            bw.Write(dest_user_id);
            bw.Write(item_id);
            bw.Write(item_sub_id);
            bw.Write(item_count);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                source_user_id = br.ReadInt32();
                dest_user_id = br.ReadInt32();
                item_id = br.ReadInt32();
                item_sub_id = br.ReadInt32();
                item_count = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 商人金库物品赠送回应
    /// </summary>
    public sealed class GlobalBankItemBusinessmanSendAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 80;
        /// <summary>
        /// 0成功 1参数无效 2源玩家不存在 3目标玩家不存在 4资源数量不足 5权限不足
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 赠送日志Id
        /// </summary>
        public int order_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(order_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                order_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 商人金库物品赠送撤回请求
    /// </summary>
    public sealed class GlobalBankItemBusinessmanWithdrawReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 81;
        /// <summary>
        /// 源玩家Id
        /// </summary>
        public int source_user_id;
        /// <summary>
        /// 赠送日志Id
        /// </summary>
        public int order_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(source_user_id);
            bw.Write(order_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                source_user_id = br.ReadInt32();
                order_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 商人金库物品赠送撤回回应
    /// </summary>
    public sealed class GlobalBankItemBusinessmanWithdrawAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 82;
        /// <summary>
        /// 0成功 1权限不足 2找不到该订单 3超时 4对方金库物品不足,撤回失败 5订单不可撤回
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 银行卡交易订单审核请求
    /// </summary>
    public sealed class GlobalBankCardCashTradeConfirmReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 83;
        /// <summary>
        /// 订单流水Id
        /// </summary>
        public int order_id;
        /// <summary>
        /// 交易使用的平台银行卡号
        /// </summary>
        public string card_number;  //max:255
        /// <summary>
        /// 手续费
        /// </summary>
        public Int64 charges;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(order_id);
            NetHelper.SafeWriteString(bw, card_number, 255);
            bw.Write(charges);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                order_id = br.ReadInt32();
                card_number = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                charges = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 银行卡交易订单审核回应
    /// </summary>
    public sealed class GlobalBankCardCashTradeConfirmAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 84;
        /// <summary>
        /// 0成功 1订单不存在 2订单已审核
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 手机账号生成上报
    /// </summary>
    public sealed class GlobalPhoneAccountGenerateRpt : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 85;
        /// <summary>
        /// 手机号码
        /// </summary>
        public string phone;  //max:16
        /// <summary>
        /// 登录密码
        /// </summary>
        public string password;  //max:64
        /// <summary>
        /// 推荐用户手机号码
        /// </summary>
        public string phone_agent;  //max:16

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, phone, 16);
            NetHelper.SafeWriteString(bw, password, 64);
            NetHelper.SafeWriteString(bw, phone_agent, 16);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                phone = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                password = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                phone_agent = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 手机账号更新上报
    /// </summary>
    public sealed class GlobalPhoneAccountUpdatePhoneRpt : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 86;
        /// <summary>
        /// 旧手机号码
        /// </summary>
        public string phone_old;  //max:16
        /// <summary>
        /// 新手机号码
        /// </summary>
        public string phone_new;  //max:16

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, phone_old, 16);
            NetHelper.SafeWriteString(bw, phone_new, 16);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                phone_old = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                phone_new = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 手机账号密码更新上报
    /// </summary>
    public sealed class GlobalPhoneAccountUpdatePasswordRpt : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 87;
        /// <summary>
        /// 手机号码
        /// </summary>
        public string phone;  //max:16
        /// <summary>
        /// 登录密码
        /// </summary>
        public string password;  //max:64

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, phone, 16);
            NetHelper.SafeWriteString(bw, password, 64);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                phone = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                password = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 手机账号充值上报
    /// </summary>
    public sealed class GlobalPhoneAccountRechargeRpt : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 88;
        /// <summary>
        /// 手机号码
        /// </summary>
        public string phone;  //max:16
        /// <summary>
        /// 充值金额，单位分
        /// </summary>
        public int recharge;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, phone, 16);
            bw.Write(recharge);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                phone = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                recharge = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 生成视频广告订单请求
    /// </summary>
    public sealed class GlobalAdvertisementGenerateOrderReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 89;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 目的类型
        /// </summary>
        public int target_type;
        /// <summary>
        /// 目的id
        /// </summary>
        public int target_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(target_type);
            bw.Write(target_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                target_type = br.ReadInt32();
                target_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 生成视频广告订单回应
    /// </summary>
    public sealed class GlobalAdvertisementGenerateOrderAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 90;
        /// <summary>
        /// 0成功 1配置错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 内部唯一订单编号
        /// </summary>
        public string order_no;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            NetHelper.SafeWriteString(bw, order_no, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                order_no = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 视频广告订单完成通知
    /// </summary>
    public sealed class GlobalAdvertisementOrderFinishNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 91;
        /// <summary>
        /// 内部订单号
        /// </summary>
        public string order_no;  //max:32
        /// <summary>
        /// 虚拟观看 1是0否
        /// </summary>
        public int is_cheat;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, order_no, 32);
            bw.Write(is_cheat);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                order_no = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                is_cheat = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 生成红包提现订单请求
    /// </summary>
    public sealed class GlobalAdvCashoutGenerateOrderReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 92;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 提现类型 1红包满整提现 2累计视频任务提现
        /// </summary>
        public sbyte cashout_type;
        /// <summary>
        /// 提现金额
        /// </summary>
        public Int64 cashout_value;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(cashout_type);
            bw.Write(cashout_value);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                cashout_type = br.ReadSByte();
                cashout_value = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 生成红包提现订单回应
    /// </summary>
    public sealed class GlobalAdvCashoutGenerateOrderAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 93;
        /// <summary>
        /// 0成功 1配置错误 2提现失败
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 内部唯一订单编号
        /// </summary>
        public string order_no;  //max:32
        /// <summary>
        /// 虚拟提现 1是0否
        /// </summary>
        public int is_cheat;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            NetHelper.SafeWriteString(bw, order_no, 32);
            bw.Write(is_cheat);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                order_no = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                is_cheat = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 开启关闭活动请求
    /// </summary>
    public sealed class GlobalActivityOpenOrCloseReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 94;
        /// <summary>
        /// 活动Id
        /// </summary>
        public int activity_id;
        /// <summary>
        /// 1开启 2关闭
        /// </summary>
        public int open_close;
        /// <summary>
        /// 活动开启时间戳
        /// </summary>
        public UInt32 start_time;
        /// <summary>
        /// 活动关闭时间戳
        /// </summary>
        public UInt32 end_time;
        /// <summary>
        /// 操作员
        /// </summary>
        public string operater;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(activity_id);
            bw.Write(open_close);
            bw.Write(start_time);
            bw.Write(end_time);
            NetHelper.SafeWriteString(bw, operater, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                activity_id = br.ReadInt32();
                open_close = br.ReadInt32();
                start_time = br.ReadUInt32();
                end_time = br.ReadUInt32();
                operater = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 开启关闭活动回应
    /// </summary>
    public sealed class GlobalActivityOpenOrCloseAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 7;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 95;
        /// <summary>
        /// 0成功 1活动已开启 2不存在已开启的该活动 3参数错误 4配置错误
        /// </summary>
        public sbyte errcode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }

    public class GlobalProtocolsResponserBase : INetResponser
    {
        public bool processPackage(BinaryReader br, INetReactor reactor, out INetProtocol responseProto)
        {
            responseProto = null;
            if (br.ReadUInt16() != 7)
                return false;

            switch(br.ReadUInt16())
            {
                case 0:
                    responseProto = new GlobalTableConfigChangedNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalTableConfigChangedNtf(responseProto as GlobalTableConfigChangedNtf);
                    break;
                case 1:
                    responseProto = new GlobalFlowControlConfigChangedNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalFlowControlConfigChangedNtf(responseProto as GlobalFlowControlConfigChangedNtf);
                    break;
                case 2:
                    responseProto = new GlobalMailSendReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalMailSendReq(responseProto as GlobalMailSendReq);
                    break;
                case 3:
                    responseProto = new GlobalMailSendAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalMailSendAck(responseProto as GlobalMailSendAck);
                    break;
                case 4:
                    responseProto = new GlobalAnnouncementChangedNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAnnouncementChangedNtf(responseProto as GlobalAnnouncementChangedNtf);
                    break;
                case 5:
                    responseProto = new GlobalBlockAccountAddReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBlockAccountAddReq(responseProto as GlobalBlockAccountAddReq);
                    break;
                case 6:
                    responseProto = new GlobalBlockAccountAddAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBlockAccountAddAck(responseProto as GlobalBlockAccountAddAck);
                    break;
                case 7:
                    responseProto = new GlobalBlockAccountRemoveReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBlockAccountRemoveReq(responseProto as GlobalBlockAccountRemoveReq);
                    break;
                case 8:
                    responseProto = new GlobalBlockAccountRemoveAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBlockAccountRemoveAck(responseProto as GlobalBlockAccountRemoveAck);
                    break;
                case 9:
                    responseProto = new GlobalPayGenerateOrderReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPayGenerateOrderReq(responseProto as GlobalPayGenerateOrderReq);
                    break;
                case 10:
                    responseProto = new GlobalPayGenerateOrderAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPayGenerateOrderAck(responseProto as GlobalPayGenerateOrderAck);
                    break;
                case 11:
                    responseProto = new GlobalPayOrderFinishNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPayOrderFinishNtf(responseProto as GlobalPayOrderFinishNtf);
                    break;
                case 12:
                    responseProto = new GlobalWxUserQueryInfoReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalWxUserQueryInfoReq(responseProto as GlobalWxUserQueryInfoReq);
                    break;
                case 13:
                    responseProto = new GlobalWxUserQueryInfoAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalWxUserQueryInfoAck(responseProto as GlobalWxUserQueryInfoAck);
                    break;
                case 14:
                    responseProto = new GlobalUserGameDifficultySetReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalUserGameDifficultySetReq(responseProto as GlobalUserGameDifficultySetReq);
                    break;
                case 15:
                    responseProto = new GlobalUserGameDifficultySetAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalUserGameDifficultySetAck(responseProto as GlobalUserGameDifficultySetAck);
                    break;
                case 16:
                    responseProto = new GlobalUserCriticalDataQueryReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalUserCriticalDataQueryReq(responseProto as GlobalUserCriticalDataQueryReq);
                    break;
                case 17:
                    responseProto = new GlobalUserCriticalDataQueryAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalUserCriticalDataQueryAck(responseProto as GlobalUserCriticalDataQueryAck);
                    break;
                case 18:
                    responseProto = new GlobalClientConfigPublishNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalClientConfigPublishNtf(responseProto as GlobalClientConfigPublishNtf);
                    break;
                case 19:
                    responseProto = new GlobalGameRoomGetServerIdsReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGameRoomGetServerIdsReq(responseProto as GlobalGameRoomGetServerIdsReq);
                    break;
                case 20:
                    responseProto = new GlobalGameRoomGetServerIdsAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGameRoomGetServerIdsAck(responseProto as GlobalGameRoomGetServerIdsAck);
                    break;
                case 21:
                    responseProto = new GlobalGameRoomGetAppIdReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGameRoomGetAppIdReq(responseProto as GlobalGameRoomGetAppIdReq);
                    break;
                case 22:
                    responseProto = new GlobalGameRoomGetAppIdAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGameRoomGetAppIdAck(responseProto as GlobalGameRoomGetAppIdAck);
                    break;
                case 23:
                    responseProto = new GlobalGameRoomSetDifficultyReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGameRoomSetDifficultyReq(responseProto as GlobalGameRoomSetDifficultyReq);
                    break;
                case 24:
                    responseProto = new GlobalGameRoomSetDifficultyAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGameRoomSetDifficultyAck(responseProto as GlobalGameRoomSetDifficultyAck);
                    break;
                case 25:
                    responseProto = new GlobalGameRoomSetParamReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGameRoomSetParamReq(responseProto as GlobalGameRoomSetParamReq);
                    break;
                case 26:
                    responseProto = new GlobalGameRoomSetParamAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGameRoomSetParamAck(responseProto as GlobalGameRoomSetParamAck);
                    break;
                case 27:
                    responseProto = new GlobalZoneConfigOpenStateChangedNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalZoneConfigOpenStateChangedNtf(responseProto as GlobalZoneConfigOpenStateChangedNtf);
                    break;
                case 28:
                    responseProto = new GlobalZoneConfigVersionUpdateChangedNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalZoneConfigVersionUpdateChangedNtf(responseProto as GlobalZoneConfigVersionUpdateChangedNtf);
                    break;
                case 29:
                    responseProto = new GlobalZoneConfigInfoChangedNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalZoneConfigInfoChangedNtf(responseProto as GlobalZoneConfigInfoChangedNtf);
                    break;
                case 30:
                    responseProto = new GlobalMachineMysqlAutoBackupConfigChangedNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalMachineMysqlAutoBackupConfigChangedNtf(responseProto as GlobalMachineMysqlAutoBackupConfigChangedNtf);
                    break;
                case 31:
                    responseProto = new GlobalGuildDismissReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGuildDismissReq(responseProto as GlobalGuildDismissReq);
                    break;
                case 32:
                    responseProto = new GlobalGuildDismissAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGuildDismissAck(responseProto as GlobalGuildDismissAck);
                    break;
                case 33:
                    responseProto = new GlobalPhoneSmsCodeCheckReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPhoneSmsCodeCheckReq(responseProto as GlobalPhoneSmsCodeCheckReq);
                    break;
                case 34:
                    responseProto = new GlobalPhoneSmsCodeCheckAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPhoneSmsCodeCheckAck(responseProto as GlobalPhoneSmsCodeCheckAck);
                    break;
                case 35:
                    responseProto = new GlobalGuildSetDisplayLevelNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGuildSetDisplayLevelNtf(responseProto as GlobalGuildSetDisplayLevelNtf);
                    break;
                case 36:
                    responseProto = new GlobalUserBusinessmanSetReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalUserBusinessmanSetReq(responseProto as GlobalUserBusinessmanSetReq);
                    break;
                case 37:
                    responseProto = new GlobalUserBusinessmanSetAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalUserBusinessmanSetAck(responseProto as GlobalUserBusinessmanSetAck);
                    break;
                case 38:
                    responseProto = new GlobalBroadcastMessageAddReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBroadcastMessageAddReq(responseProto as GlobalBroadcastMessageAddReq);
                    break;
                case 39:
                    responseProto = new GlobalBroadcastMessageAddAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBroadcastMessageAddAck(responseProto as GlobalBroadcastMessageAddAck);
                    break;
                case 40:
                    responseProto = new GlobalBroadcastMessageRemoveReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBroadcastMessageRemoveReq(responseProto as GlobalBroadcastMessageRemoveReq);
                    break;
                case 41:
                    responseProto = new GlobalBroadcastMessageRemoveAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBroadcastMessageRemoveAck(responseProto as GlobalBroadcastMessageRemoveAck);
                    break;
                case 42:
                    responseProto = new GlobalUserAddVipExpReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalUserAddVipExpReq(responseProto as GlobalUserAddVipExpReq);
                    break;
                case 43:
                    responseProto = new GlobalUserAddVipExpAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalUserAddVipExpAck(responseProto as GlobalUserAddVipExpAck);
                    break;
                case 44:
                    responseProto = new GlobalAgentQueryContributionListReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAgentQueryContributionListReq(responseProto as GlobalAgentQueryContributionListReq);
                    break;
                case 45:
                    responseProto = new GlobalAgentQueryContributionListAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAgentQueryContributionListAck(responseProto as GlobalAgentQueryContributionListAck);
                    break;
                case 46:
                    responseProto = new GlobalAccountBindPhoneReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAccountBindPhoneReq(responseProto as GlobalAccountBindPhoneReq);
                    break;
                case 47:
                    responseProto = new GlobalAccountBindPhoneAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAccountBindPhoneAck(responseProto as GlobalAccountBindPhoneAck);
                    break;
                case 48:
                    responseProto = new GlobalAccountResetBankPasswordReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAccountResetBankPasswordReq(responseProto as GlobalAccountResetBankPasswordReq);
                    break;
                case 49:
                    responseProto = new GlobalAccountResetBankPasswordAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAccountResetBankPasswordAck(responseProto as GlobalAccountResetBankPasswordAck);
                    break;
                case 50:
                    responseProto = new GlobalAccountKickoffReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAccountKickoffReq(responseProto as GlobalAccountKickoffReq);
                    break;
                case 51:
                    responseProto = new GlobalAccountKickoffAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAccountKickoffAck(responseProto as GlobalAccountKickoffAck);
                    break;
                case 52:
                    responseProto = new GlobalGameDifficultySetReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGameDifficultySetReq(responseProto as GlobalGameDifficultySetReq);
                    break;
                case 53:
                    responseProto = new GlobalGameDifficultySetAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGameDifficultySetAck(responseProto as GlobalGameDifficultySetAck);
                    break;
                case 54:
                    responseProto = new GlobalGlobalInformationQueryReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGlobalInformationQueryReq(responseProto as GlobalGlobalInformationQueryReq);
                    break;
                case 55:
                    responseProto = new GlobalGlobalInformationQueryAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalGlobalInformationQueryAck(responseProto as GlobalGlobalInformationQueryAck);
                    break;
                case 56:
                    responseProto = new GlobalPrizePoolWaterLineAddReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPrizePoolWaterLineAddReq(responseProto as GlobalPrizePoolWaterLineAddReq);
                    break;
                case 57:
                    responseProto = new GlobalPrizePoolWaterLineAddAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPrizePoolWaterLineAddAck(responseProto as GlobalPrizePoolWaterLineAddAck);
                    break;
                case 58:
                    responseProto = new GlobalPrizePoolShareBonusImmediateReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPrizePoolShareBonusImmediateReq(responseProto as GlobalPrizePoolShareBonusImmediateReq);
                    break;
                case 59:
                    responseProto = new GlobalPrizePoolShareBonusImmediateAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPrizePoolShareBonusImmediateAck(responseProto as GlobalPrizePoolShareBonusImmediateAck);
                    break;
                case 60:
                    responseProto = new GlobalPlayerPrizePoolAddReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPlayerPrizePoolAddReq(responseProto as GlobalPlayerPrizePoolAddReq);
                    break;
                case 61:
                    responseProto = new GlobalPlayerPrizePoolAddAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPlayerPrizePoolAddAck(responseProto as GlobalPlayerPrizePoolAddAck);
                    break;
                case 62:
                    responseProto = new GlobalPlayerResourceAddReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPlayerResourceAddReq(responseProto as GlobalPlayerResourceAddReq);
                    break;
                case 63:
                    responseProto = new GlobalPlayerResourceAddAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPlayerResourceAddAck(responseProto as GlobalPlayerResourceAddAck);
                    break;
                case 64:
                    responseProto = new GlobalPlayerNicknameModifyReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPlayerNicknameModifyReq(responseProto as GlobalPlayerNicknameModifyReq);
                    break;
                case 65:
                    responseProto = new GlobalPlayerNicknameModifyAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPlayerNicknameModifyAck(responseProto as GlobalPlayerNicknameModifyAck);
                    break;
                case 66:
                    responseProto = new GlobalPlayerNicknameModifyCountResetReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPlayerNicknameModifyCountResetReq(responseProto as GlobalPlayerNicknameModifyCountResetReq);
                    break;
                case 67:
                    responseProto = new GlobalPlayerNicknameModifyCountResetAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPlayerNicknameModifyCountResetAck(responseProto as GlobalPlayerNicknameModifyCountResetAck);
                    break;
                case 68:
                    responseProto = new GlobalBankItemSendReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBankItemSendReq(responseProto as GlobalBankItemSendReq);
                    break;
                case 69:
                    responseProto = new GlobalBankItemSendAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBankItemSendAck(responseProto as GlobalBankItemSendAck);
                    break;
                case 70:
                    responseProto = new GlobalStopRunningNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalStopRunningNtf(responseProto as GlobalStopRunningNtf);
                    break;
                case 71:
                    responseProto = new GlobalStartInstanceReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalStartInstanceReq(responseProto as GlobalStartInstanceReq);
                    break;
                case 72:
                    responseProto = new GlobalStartInstanceAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalStartInstanceAck(responseProto as GlobalStartInstanceAck);
                    break;
                case 73:
                    responseProto = new GlobalUserHonorableSetReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalUserHonorableSetReq(responseProto as GlobalUserHonorableSetReq);
                    break;
                case 74:
                    responseProto = new GlobalUserHonorableSetAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalUserHonorableSetAck(responseProto as GlobalUserHonorableSetAck);
                    break;
                case 75:
                    responseProto = new GlobalChannelUserQueryInfoReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalChannelUserQueryInfoReq(responseProto as GlobalChannelUserQueryInfoReq);
                    break;
                case 76:
                    responseProto = new GlobalChannelUserQueryInfoAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalChannelUserQueryInfoAck(responseProto as GlobalChannelUserQueryInfoAck);
                    break;
                case 77:
                    responseProto = new GlobalBankItemSendExamineReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBankItemSendExamineReq(responseProto as GlobalBankItemSendExamineReq);
                    break;
                case 78:
                    responseProto = new GlobalBankItemSendExamineAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBankItemSendExamineAck(responseProto as GlobalBankItemSendExamineAck);
                    break;
                case 79:
                    responseProto = new GlobalBankItemBusinessmanSendReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBankItemBusinessmanSendReq(responseProto as GlobalBankItemBusinessmanSendReq);
                    break;
                case 80:
                    responseProto = new GlobalBankItemBusinessmanSendAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBankItemBusinessmanSendAck(responseProto as GlobalBankItemBusinessmanSendAck);
                    break;
                case 81:
                    responseProto = new GlobalBankItemBusinessmanWithdrawReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBankItemBusinessmanWithdrawReq(responseProto as GlobalBankItemBusinessmanWithdrawReq);
                    break;
                case 82:
                    responseProto = new GlobalBankItemBusinessmanWithdrawAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBankItemBusinessmanWithdrawAck(responseProto as GlobalBankItemBusinessmanWithdrawAck);
                    break;
                case 83:
                    responseProto = new GlobalBankCardCashTradeConfirmReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBankCardCashTradeConfirmReq(responseProto as GlobalBankCardCashTradeConfirmReq);
                    break;
                case 84:
                    responseProto = new GlobalBankCardCashTradeConfirmAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalBankCardCashTradeConfirmAck(responseProto as GlobalBankCardCashTradeConfirmAck);
                    break;
                case 85:
                    responseProto = new GlobalPhoneAccountGenerateRpt();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPhoneAccountGenerateRpt(responseProto as GlobalPhoneAccountGenerateRpt);
                    break;
                case 86:
                    responseProto = new GlobalPhoneAccountUpdatePhoneRpt();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPhoneAccountUpdatePhoneRpt(responseProto as GlobalPhoneAccountUpdatePhoneRpt);
                    break;
                case 87:
                    responseProto = new GlobalPhoneAccountUpdatePasswordRpt();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPhoneAccountUpdatePasswordRpt(responseProto as GlobalPhoneAccountUpdatePasswordRpt);
                    break;
                case 88:
                    responseProto = new GlobalPhoneAccountRechargeRpt();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalPhoneAccountRechargeRpt(responseProto as GlobalPhoneAccountRechargeRpt);
                    break;
                case 89:
                    responseProto = new GlobalAdvertisementGenerateOrderReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAdvertisementGenerateOrderReq(responseProto as GlobalAdvertisementGenerateOrderReq);
                    break;
                case 90:
                    responseProto = new GlobalAdvertisementGenerateOrderAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAdvertisementGenerateOrderAck(responseProto as GlobalAdvertisementGenerateOrderAck);
                    break;
                case 91:
                    responseProto = new GlobalAdvertisementOrderFinishNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAdvertisementOrderFinishNtf(responseProto as GlobalAdvertisementOrderFinishNtf);
                    break;
                case 92:
                    responseProto = new GlobalAdvCashoutGenerateOrderReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAdvCashoutGenerateOrderReq(responseProto as GlobalAdvCashoutGenerateOrderReq);
                    break;
                case 93:
                    responseProto = new GlobalAdvCashoutGenerateOrderAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalAdvCashoutGenerateOrderAck(responseProto as GlobalAdvCashoutGenerateOrderAck);
                    break;
                case 94:
                    responseProto = new GlobalActivityOpenOrCloseReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalActivityOpenOrCloseReq(responseProto as GlobalActivityOpenOrCloseReq);
                    break;
                case 95:
                    responseProto = new GlobalActivityOpenOrCloseAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GlobalActivityOpenOrCloseAck(responseProto as GlobalActivityOpenOrCloseAck);
                    break;
            }
            return responseProto != null;
        }

        public virtual void onRecv_GlobalTableConfigChangedNtf(GlobalTableConfigChangedNtf proto) { }
        public virtual void onRecv_GlobalFlowControlConfigChangedNtf(GlobalFlowControlConfigChangedNtf proto) { }
        public virtual void onRecv_GlobalMailSendReq(GlobalMailSendReq proto) { }
        public virtual void onRecv_GlobalMailSendAck(GlobalMailSendAck proto) { }
        public virtual void onRecv_GlobalAnnouncementChangedNtf(GlobalAnnouncementChangedNtf proto) { }
        public virtual void onRecv_GlobalBlockAccountAddReq(GlobalBlockAccountAddReq proto) { }
        public virtual void onRecv_GlobalBlockAccountAddAck(GlobalBlockAccountAddAck proto) { }
        public virtual void onRecv_GlobalBlockAccountRemoveReq(GlobalBlockAccountRemoveReq proto) { }
        public virtual void onRecv_GlobalBlockAccountRemoveAck(GlobalBlockAccountRemoveAck proto) { }
        public virtual void onRecv_GlobalPayGenerateOrderReq(GlobalPayGenerateOrderReq proto) { }
        public virtual void onRecv_GlobalPayGenerateOrderAck(GlobalPayGenerateOrderAck proto) { }
        public virtual void onRecv_GlobalPayOrderFinishNtf(GlobalPayOrderFinishNtf proto) { }
        public virtual void onRecv_GlobalWxUserQueryInfoReq(GlobalWxUserQueryInfoReq proto) { }
        public virtual void onRecv_GlobalWxUserQueryInfoAck(GlobalWxUserQueryInfoAck proto) { }
        public virtual void onRecv_GlobalUserGameDifficultySetReq(GlobalUserGameDifficultySetReq proto) { }
        public virtual void onRecv_GlobalUserGameDifficultySetAck(GlobalUserGameDifficultySetAck proto) { }
        public virtual void onRecv_GlobalUserCriticalDataQueryReq(GlobalUserCriticalDataQueryReq proto) { }
        public virtual void onRecv_GlobalUserCriticalDataQueryAck(GlobalUserCriticalDataQueryAck proto) { }
        public virtual void onRecv_GlobalClientConfigPublishNtf(GlobalClientConfigPublishNtf proto) { }
        public virtual void onRecv_GlobalGameRoomGetServerIdsReq(GlobalGameRoomGetServerIdsReq proto) { }
        public virtual void onRecv_GlobalGameRoomGetServerIdsAck(GlobalGameRoomGetServerIdsAck proto) { }
        public virtual void onRecv_GlobalGameRoomGetAppIdReq(GlobalGameRoomGetAppIdReq proto) { }
        public virtual void onRecv_GlobalGameRoomGetAppIdAck(GlobalGameRoomGetAppIdAck proto) { }
        public virtual void onRecv_GlobalGameRoomSetDifficultyReq(GlobalGameRoomSetDifficultyReq proto) { }
        public virtual void onRecv_GlobalGameRoomSetDifficultyAck(GlobalGameRoomSetDifficultyAck proto) { }
        public virtual void onRecv_GlobalGameRoomSetParamReq(GlobalGameRoomSetParamReq proto) { }
        public virtual void onRecv_GlobalGameRoomSetParamAck(GlobalGameRoomSetParamAck proto) { }
        public virtual void onRecv_GlobalZoneConfigOpenStateChangedNtf(GlobalZoneConfigOpenStateChangedNtf proto) { }
        public virtual void onRecv_GlobalZoneConfigVersionUpdateChangedNtf(GlobalZoneConfigVersionUpdateChangedNtf proto) { }
        public virtual void onRecv_GlobalZoneConfigInfoChangedNtf(GlobalZoneConfigInfoChangedNtf proto) { }
        public virtual void onRecv_GlobalMachineMysqlAutoBackupConfigChangedNtf(GlobalMachineMysqlAutoBackupConfigChangedNtf proto) { }
        public virtual void onRecv_GlobalGuildDismissReq(GlobalGuildDismissReq proto) { }
        public virtual void onRecv_GlobalGuildDismissAck(GlobalGuildDismissAck proto) { }
        public virtual void onRecv_GlobalPhoneSmsCodeCheckReq(GlobalPhoneSmsCodeCheckReq proto) { }
        public virtual void onRecv_GlobalPhoneSmsCodeCheckAck(GlobalPhoneSmsCodeCheckAck proto) { }
        public virtual void onRecv_GlobalGuildSetDisplayLevelNtf(GlobalGuildSetDisplayLevelNtf proto) { }
        public virtual void onRecv_GlobalUserBusinessmanSetReq(GlobalUserBusinessmanSetReq proto) { }
        public virtual void onRecv_GlobalUserBusinessmanSetAck(GlobalUserBusinessmanSetAck proto) { }
        public virtual void onRecv_GlobalBroadcastMessageAddReq(GlobalBroadcastMessageAddReq proto) { }
        public virtual void onRecv_GlobalBroadcastMessageAddAck(GlobalBroadcastMessageAddAck proto) { }
        public virtual void onRecv_GlobalBroadcastMessageRemoveReq(GlobalBroadcastMessageRemoveReq proto) { }
        public virtual void onRecv_GlobalBroadcastMessageRemoveAck(GlobalBroadcastMessageRemoveAck proto) { }
        public virtual void onRecv_GlobalUserAddVipExpReq(GlobalUserAddVipExpReq proto) { }
        public virtual void onRecv_GlobalUserAddVipExpAck(GlobalUserAddVipExpAck proto) { }
        public virtual void onRecv_GlobalAgentQueryContributionListReq(GlobalAgentQueryContributionListReq proto) { }
        public virtual void onRecv_GlobalAgentQueryContributionListAck(GlobalAgentQueryContributionListAck proto) { }
        public virtual void onRecv_GlobalAccountBindPhoneReq(GlobalAccountBindPhoneReq proto) { }
        public virtual void onRecv_GlobalAccountBindPhoneAck(GlobalAccountBindPhoneAck proto) { }
        public virtual void onRecv_GlobalAccountResetBankPasswordReq(GlobalAccountResetBankPasswordReq proto) { }
        public virtual void onRecv_GlobalAccountResetBankPasswordAck(GlobalAccountResetBankPasswordAck proto) { }
        public virtual void onRecv_GlobalAccountKickoffReq(GlobalAccountKickoffReq proto) { }
        public virtual void onRecv_GlobalAccountKickoffAck(GlobalAccountKickoffAck proto) { }
        public virtual void onRecv_GlobalGameDifficultySetReq(GlobalGameDifficultySetReq proto) { }
        public virtual void onRecv_GlobalGameDifficultySetAck(GlobalGameDifficultySetAck proto) { }
        public virtual void onRecv_GlobalGlobalInformationQueryReq(GlobalGlobalInformationQueryReq proto) { }
        public virtual void onRecv_GlobalGlobalInformationQueryAck(GlobalGlobalInformationQueryAck proto) { }
        public virtual void onRecv_GlobalPrizePoolWaterLineAddReq(GlobalPrizePoolWaterLineAddReq proto) { }
        public virtual void onRecv_GlobalPrizePoolWaterLineAddAck(GlobalPrizePoolWaterLineAddAck proto) { }
        public virtual void onRecv_GlobalPrizePoolShareBonusImmediateReq(GlobalPrizePoolShareBonusImmediateReq proto) { }
        public virtual void onRecv_GlobalPrizePoolShareBonusImmediateAck(GlobalPrizePoolShareBonusImmediateAck proto) { }
        public virtual void onRecv_GlobalPlayerPrizePoolAddReq(GlobalPlayerPrizePoolAddReq proto) { }
        public virtual void onRecv_GlobalPlayerPrizePoolAddAck(GlobalPlayerPrizePoolAddAck proto) { }
        public virtual void onRecv_GlobalPlayerResourceAddReq(GlobalPlayerResourceAddReq proto) { }
        public virtual void onRecv_GlobalPlayerResourceAddAck(GlobalPlayerResourceAddAck proto) { }
        public virtual void onRecv_GlobalPlayerNicknameModifyReq(GlobalPlayerNicknameModifyReq proto) { }
        public virtual void onRecv_GlobalPlayerNicknameModifyAck(GlobalPlayerNicknameModifyAck proto) { }
        public virtual void onRecv_GlobalPlayerNicknameModifyCountResetReq(GlobalPlayerNicknameModifyCountResetReq proto) { }
        public virtual void onRecv_GlobalPlayerNicknameModifyCountResetAck(GlobalPlayerNicknameModifyCountResetAck proto) { }
        public virtual void onRecv_GlobalBankItemSendReq(GlobalBankItemSendReq proto) { }
        public virtual void onRecv_GlobalBankItemSendAck(GlobalBankItemSendAck proto) { }
        public virtual void onRecv_GlobalStopRunningNtf(GlobalStopRunningNtf proto) { }
        public virtual void onRecv_GlobalStartInstanceReq(GlobalStartInstanceReq proto) { }
        public virtual void onRecv_GlobalStartInstanceAck(GlobalStartInstanceAck proto) { }
        public virtual void onRecv_GlobalUserHonorableSetReq(GlobalUserHonorableSetReq proto) { }
        public virtual void onRecv_GlobalUserHonorableSetAck(GlobalUserHonorableSetAck proto) { }
        public virtual void onRecv_GlobalChannelUserQueryInfoReq(GlobalChannelUserQueryInfoReq proto) { }
        public virtual void onRecv_GlobalChannelUserQueryInfoAck(GlobalChannelUserQueryInfoAck proto) { }
        public virtual void onRecv_GlobalBankItemSendExamineReq(GlobalBankItemSendExamineReq proto) { }
        public virtual void onRecv_GlobalBankItemSendExamineAck(GlobalBankItemSendExamineAck proto) { }
        public virtual void onRecv_GlobalBankItemBusinessmanSendReq(GlobalBankItemBusinessmanSendReq proto) { }
        public virtual void onRecv_GlobalBankItemBusinessmanSendAck(GlobalBankItemBusinessmanSendAck proto) { }
        public virtual void onRecv_GlobalBankItemBusinessmanWithdrawReq(GlobalBankItemBusinessmanWithdrawReq proto) { }
        public virtual void onRecv_GlobalBankItemBusinessmanWithdrawAck(GlobalBankItemBusinessmanWithdrawAck proto) { }
        public virtual void onRecv_GlobalBankCardCashTradeConfirmReq(GlobalBankCardCashTradeConfirmReq proto) { }
        public virtual void onRecv_GlobalBankCardCashTradeConfirmAck(GlobalBankCardCashTradeConfirmAck proto) { }
        public virtual void onRecv_GlobalPhoneAccountGenerateRpt(GlobalPhoneAccountGenerateRpt proto) { }
        public virtual void onRecv_GlobalPhoneAccountUpdatePhoneRpt(GlobalPhoneAccountUpdatePhoneRpt proto) { }
        public virtual void onRecv_GlobalPhoneAccountUpdatePasswordRpt(GlobalPhoneAccountUpdatePasswordRpt proto) { }
        public virtual void onRecv_GlobalPhoneAccountRechargeRpt(GlobalPhoneAccountRechargeRpt proto) { }
        public virtual void onRecv_GlobalAdvertisementGenerateOrderReq(GlobalAdvertisementGenerateOrderReq proto) { }
        public virtual void onRecv_GlobalAdvertisementGenerateOrderAck(GlobalAdvertisementGenerateOrderAck proto) { }
        public virtual void onRecv_GlobalAdvertisementOrderFinishNtf(GlobalAdvertisementOrderFinishNtf proto) { }
        public virtual void onRecv_GlobalAdvCashoutGenerateOrderReq(GlobalAdvCashoutGenerateOrderReq proto) { }
        public virtual void onRecv_GlobalAdvCashoutGenerateOrderAck(GlobalAdvCashoutGenerateOrderAck proto) { }
        public virtual void onRecv_GlobalActivityOpenOrCloseReq(GlobalActivityOpenOrCloseReq proto) { }
        public virtual void onRecv_GlobalActivityOpenOrCloseAck(GlobalActivityOpenOrCloseAck proto) { }
    }
}
