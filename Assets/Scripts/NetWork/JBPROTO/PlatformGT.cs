using System;
using System.IO;

namespace JBPROTO
{
    /// <summary>
    /// 登录请求
    /// </summary>
    public sealed class PFGTLoginReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 5;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 0;
        /// <summary>
        /// 客户端连接的唯一索引
        /// </summary>
        public UInt32 index;
        /// <summary>
        /// 0允许所有 1只允许非黑名单 2只允许白名单
        /// </summary>
        public int allow_mode;
        /// <summary>
        /// 运行平台 1:IOS 2:ANDRIOD 3:WINDOWS 4:LINUX 5:MAC 6:WebGL
        /// </summary>
        public UInt32 platform;
        /// <summary>
        /// 产品代号 0:未知的产品 1:游戏
        /// </summary>
        public UInt32 product;
        /// <summary>
        /// 产品版本号
        /// </summary>
        public UInt32 version;
        /// <summary>
        /// 机器设备码
        /// </summary>
        public string device;  //max:64
        /// <summary>
        /// 渠道
        /// </summary>
        public string channel;  //max:32
        /// <summary>
        /// 国家标识
        /// </summary>
        public string country;  //max:16
        /// <summary>
        /// 语言标识
        /// </summary>
        public string language;  //max:16
        /// <summary>
        /// 登录方式 1游客 2手机登录 3QQ 4微信 5Facebook 6GooglePlay 7GameCenter
        /// </summary>
        public byte login_type;
        /// <summary>
        /// 唯一标识串，明文
        /// </summary>
        public string token;  //max:64
        /// <summary>
        /// 登录IP地址
        /// </summary>
        public string ip;  //max:32
        /// <summary>
        /// 推广员代理串
        /// </summary>
        public string agent;  //max:128

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(index);
            bw.Write(allow_mode);
            bw.Write(platform);
            bw.Write(product);
            bw.Write(version);
            NetHelper.SafeWriteString(bw, device, 64);
            NetHelper.SafeWriteString(bw, channel, 32);
            NetHelper.SafeWriteString(bw, country, 16);
            NetHelper.SafeWriteString(bw, language, 16);
            bw.Write(login_type);
            NetHelper.SafeWriteString(bw, token, 64);
            NetHelper.SafeWriteString(bw, ip, 32);
            NetHelper.SafeWriteString(bw, agent, 128);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                index = br.ReadUInt32();
                allow_mode = br.ReadInt32();
                platform = br.ReadUInt32();
                product = br.ReadUInt32();
                version = br.ReadUInt32();
                device = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                channel = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                country = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                language = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                login_type = br.ReadByte();
                token = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                ip = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                agent = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 管理员登录请求
    /// </summary>
    public sealed class PFGTAdminLoginReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 5;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 1;
        /// <summary>
        /// 客户端连接的唯一索引
        /// </summary>
        public UInt32 index;
        /// <summary>
        /// 运行平台 1:IOS 2:ANDRIOD 3:WINDOWS 4:LINUX 5:MAC 6:WebGL
        /// </summary>
        public UInt32 platform;
        /// <summary>
        /// 产品代号 0:未知的产品 1:游戏
        /// </summary>
        public UInt32 product;
        /// <summary>
        /// 产品版本号
        /// </summary>
        public UInt32 version;
        /// <summary>
        /// 机器设备码
        /// </summary>
        public string device;  //max:64
        /// <summary>
        /// 渠道
        /// </summary>
        public string channel;  //max:64
        /// <summary>
        /// 国家标识
        /// </summary>
        public string country;  //max:32
        /// <summary>
        /// 语言标识
        /// </summary>
        public string language;  //max:32
        /// <summary>
        /// 目标玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 登录IP地址
        /// </summary>
        public string ip;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(index);
            bw.Write(platform);
            bw.Write(product);
            bw.Write(version);
            NetHelper.SafeWriteString(bw, device, 64);
            NetHelper.SafeWriteString(bw, channel, 64);
            NetHelper.SafeWriteString(bw, country, 32);
            NetHelper.SafeWriteString(bw, language, 32);
            bw.Write(user_id);
            NetHelper.SafeWriteString(bw, ip, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                index = br.ReadUInt32();
                platform = br.ReadUInt32();
                product = br.ReadUInt32();
                version = br.ReadUInt32();
                device = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                channel = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                country = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                language = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                user_id = br.ReadInt32();
                ip = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 物品信息结构
    /// </summary>
    public sealed class PFGTItemInfo : INetProtocol
    {
        /// <summary>
        /// 物品主类型
        /// </summary>
        public int item_id;
        /// <summary>
        /// 物品数量
        /// </summary>
        public Int64 item_count;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(item_id);
            bw.Write(item_count);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                item_id = br.ReadInt32();
                item_count = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 登录回应
    /// </summary>
    public sealed class PFGTLoginAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 5;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 2;
        /// <summary>
        /// 客户端连接的唯一索引
        /// </summary>
        public UInt32 index;
        /// <summary>
        /// 0成功 1账号被封禁，解封时间由time_string指定 2系统繁忙 3系统错误 4系统暂未开放
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 用户ID
        /// </summary>
        public int user_id;
        /// <summary>
        /// 昵称
        /// </summary>
        public string nickname;  //max:32
        /// <summary>
        /// 昵称是否修改过 1是 0否
        /// </summary>
        public sbyte nickname_mdf;
        /// <summary>
        /// 性别 0保密 1男 2女
        /// </summary>
        public int gender;
        /// <summary>
        /// 头像Id
        /// </summary>
        public int head;
        /// <summary>
        /// 头像框Id
        /// </summary>
        public int head_frame;
        /// <summary>
        /// 称号ID
        /// </summary>
        public int title;
        /// <summary>
        /// 段位ID
        /// </summary>
        public int dan;
        /// <summary>
        /// 装饰ID
        /// </summary>
        public int decorate;
        /// <summary>
        /// 绑定手机
        /// </summary>
        public string phone;  //max:20
        /// <summary>
        /// 平台点券
        /// </summary>
        public Int64 coupon;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public PFGTItemInfo[] items;  //max:100
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;
        /// <summary>
        /// 服务器时间戳
        /// </summary>
        public UInt32 server_timestamp;
        /// <summary>
        /// 额外附加参数
        /// </summary>
        public string extra_params;  //max:4096

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(index);
            bw.Write(errcode);
            bw.Write(user_id);
            NetHelper.SafeWriteString(bw, nickname, 32);
            bw.Write(nickname_mdf);
            bw.Write(gender);
            bw.Write(head);
            bw.Write(head_frame);
            bw.Write(title);
            bw.Write(dan);
            bw.Write(decorate);
            NetHelper.SafeWriteString(bw, phone, 20);
            bw.Write(coupon);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"PFGTLoginAck.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
            bw.Write(server_timestamp);
            NetHelper.SafeWriteString(bw, extra_params, 4096);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                index = br.ReadUInt32();
                errcode = br.ReadSByte();
                user_id = br.ReadInt32();
                nickname = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                nickname_mdf = br.ReadSByte();
                gender = br.ReadInt32();
                head = br.ReadInt32();
                head_frame = br.ReadInt32();
                title = br.ReadInt32();
                dan = br.ReadInt32();
                decorate = br.ReadInt32();
                phone = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                coupon = br.ReadInt64();
                item_len = br.ReadInt32();
                items = new PFGTItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new PFGTItemInfo();
                    items[i].fromBinary(br);
                }
                server_timestamp = br.ReadUInt32();
                extra_params = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 报告客户端下线
    /// </summary>
    public sealed class PFGTDisconnectRpt : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 5;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 3;
        /// <summary>
        /// 客户端连接的唯一索引
        /// </summary>
        public UInt32 index;
        /// <summary>
        /// 0客户端掉线 1网关维护 2游戏维护 3游戏断开连接 4游戏内部错误 5离线挂机 6服务维护
        /// </summary>
        public byte reason;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(index);
            bw.Write(reason);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                index = br.ReadUInt32();
                reason = br.ReadByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 通知客户端下线
    /// </summary>
    public sealed class PFGTDisconnectNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 5;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 4;
        /// <summary>
        /// 客户端连接的唯一索引
        /// </summary>
        public UInt32 index;
        /// <summary>
        /// 0正常退出 1被踢下线 2被挤下线 3系统错误 4平台维护
        /// </summary>
        public byte code;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(index);
            bw.Write(code);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                index = br.ReadUInt32();
                code = br.ReadByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 加入服务报告
    /// </summary>
    public sealed class PFGTServiceAccessRpt : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 5;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 5;
        /// <summary>
        /// 客户端连接的唯一索引
        /// </summary>
        public UInt32 index;
        /// <summary>
        /// 1加入服务 2离开服务
        /// </summary>
        public int action;
        /// <summary>
        /// 应用程序Id
        /// </summary>
        public string app_id;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(index);
            bw.Write(action);
            NetHelper.SafeWriteString(bw, app_id, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                index = br.ReadUInt32();
                action = br.ReadInt32();
                app_id = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }

    public class PlatformGTResponserBase : INetResponser
    {
        public bool processPackage(BinaryReader br, INetReactor reactor, out INetProtocol responseProto)
        {
            responseProto = null;
            if (br.ReadUInt16() != 5)
                return false;

            switch(br.ReadUInt16())
            {
                case 0:
                    responseProto = new PFGTLoginReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGTLoginReq(responseProto as PFGTLoginReq);
                    break;
                case 1:
                    responseProto = new PFGTAdminLoginReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGTAdminLoginReq(responseProto as PFGTAdminLoginReq);
                    break;
                case 2:
                    responseProto = new PFGTLoginAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGTLoginAck(responseProto as PFGTLoginAck);
                    break;
                case 3:
                    responseProto = new PFGTDisconnectRpt();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGTDisconnectRpt(responseProto as PFGTDisconnectRpt);
                    break;
                case 4:
                    responseProto = new PFGTDisconnectNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGTDisconnectNtf(responseProto as PFGTDisconnectNtf);
                    break;
                case 5:
                    responseProto = new PFGTServiceAccessRpt();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGTServiceAccessRpt(responseProto as PFGTServiceAccessRpt);
                    break;
            }
            return responseProto != null;
        }

        public virtual void onRecv_PFGTLoginReq(PFGTLoginReq proto) { }
        public virtual void onRecv_PFGTAdminLoginReq(PFGTAdminLoginReq proto) { }
        public virtual void onRecv_PFGTLoginAck(PFGTLoginAck proto) { }
        public virtual void onRecv_PFGTDisconnectRpt(PFGTDisconnectRpt proto) { }
        public virtual void onRecv_PFGTDisconnectNtf(PFGTDisconnectNtf proto) { }
        public virtual void onRecv_PFGTServiceAccessRpt(PFGTServiceAccessRpt proto) { }
    }
}
