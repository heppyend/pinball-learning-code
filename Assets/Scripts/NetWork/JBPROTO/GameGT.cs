using System;
using System.IO;

namespace JBPROTO
{
    /// <summary>
    /// 握手请求
    /// </summary>
    public sealed class GSGTHandReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 3;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 0;
        /// <summary>
        /// 应用程序Id
        /// </summary>
        public string app_id;  //max:32
        /// <summary>
        /// 服务器名称
        /// </summary>
        public string server_name;  //max:32
        /// <summary>
        /// 游戏服务组Id，唯一标识游戏组
        /// </summary>
        public int group_id;
        /// <summary>
        /// 游戏子服务Id，0代表主服务
        /// </summary>
        public int service_id;
        /// <summary>
        /// 处理的协议模块Id列表，用逗号隔开
        /// </summary>
        public string modules;  //max:256

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, app_id, 32);
            NetHelper.SafeWriteString(bw, server_name, 32);
            bw.Write(group_id);
            bw.Write(service_id);
            NetHelper.SafeWriteString(bw, modules, 256);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                app_id = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                server_name = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                group_id = br.ReadInt32();
                service_id = br.ReadInt32();
                modules = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 握手回应
    /// </summary>
    public sealed class GSGTHandAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 3;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 1;
        /// <summary>
        /// 0成功 1拒绝访问
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
    /// 登入请求
    /// </summary>
    public sealed class GSGTEnterServiceReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 3;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 2;
        /// <summary>
        /// 请求唯一Id
        /// </summary>
        public int req_id;
        /// <summary>
        /// 连接索引
        /// </summary>
        public UInt32 index;
        /// <summary>
        /// 用户Id
        /// </summary>
        public int user_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(req_id);
            bw.Write(index);
            bw.Write(user_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                req_id = br.ReadInt32();
                index = br.ReadUInt32();
                user_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 登入回应
    /// </summary>
    public sealed class GSGTEnterServiceAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 3;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 3;
        /// <summary>
        /// 请求唯一Id
        /// </summary>
        public int req_id;
        /// <summary>
        /// 0成功 1拒绝访问
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 上次未结束的游戏数据，json格式
        /// </summary>
        public string game_data;  //max:4096

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(req_id);
            bw.Write(errcode);
            NetHelper.SafeWriteString(bw, game_data, 4096);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                req_id = br.ReadInt32();
                errcode = br.ReadSByte();
                game_data = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 登出请求
    /// </summary>
    public sealed class GSGTLeaveServiceNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 3;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 4;
        /// <summary>
        /// 连接索引
        /// </summary>
        public UInt32 index;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(index);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                index = br.ReadUInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 放弃连接通知
    /// </summary>
    public sealed class GSGTAbandonConnectionNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 3;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 5;
        /// <summary>
        /// 连接索引
        /// </summary>
        public UInt32 index;
        /// <summary>
        /// 0通知 1服务器维护 2系统错误 3离线挂机
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
    /// 服务注册请求
    /// </summary>
    public sealed class GSGTServiceRegisterReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 3;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 6;
        /// <summary>
        /// 连接索引
        /// </summary>
        public UInt32 index;
        /// <summary>
        /// 服务路由注册动作 0反注册 1注册
        /// </summary>
        public byte action;
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
                action = br.ReadByte();
                app_id = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 服务注册应答
    /// </summary>
    public sealed class GSGTServiceRegisterAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 3;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 7;
        /// <summary>
        /// 0注册成功 1服务不存在
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

    public class GameGTResponserBase : INetResponser
    {
        public bool processPackage(BinaryReader br, INetReactor reactor, out INetProtocol responseProto)
        {
            responseProto = null;
            if (br.ReadUInt16() != 3)
                return false;

            switch(br.ReadUInt16())
            {
                case 0:
                    responseProto = new GSGTHandReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGTHandReq(responseProto as GSGTHandReq);
                    break;
                case 1:
                    responseProto = new GSGTHandAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGTHandAck(responseProto as GSGTHandAck);
                    break;
                case 2:
                    responseProto = new GSGTEnterServiceReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGTEnterServiceReq(responseProto as GSGTEnterServiceReq);
                    break;
                case 3:
                    responseProto = new GSGTEnterServiceAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGTEnterServiceAck(responseProto as GSGTEnterServiceAck);
                    break;
                case 4:
                    responseProto = new GSGTLeaveServiceNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGTLeaveServiceNtf(responseProto as GSGTLeaveServiceNtf);
                    break;
                case 5:
                    responseProto = new GSGTAbandonConnectionNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGTAbandonConnectionNtf(responseProto as GSGTAbandonConnectionNtf);
                    break;
                case 6:
                    responseProto = new GSGTServiceRegisterReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGTServiceRegisterReq(responseProto as GSGTServiceRegisterReq);
                    break;
                case 7:
                    responseProto = new GSGTServiceRegisterAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGTServiceRegisterAck(responseProto as GSGTServiceRegisterAck);
                    break;
            }
            return responseProto != null;
        }

        public virtual void onRecv_GSGTHandReq(GSGTHandReq proto) { }
        public virtual void onRecv_GSGTHandAck(GSGTHandAck proto) { }
        public virtual void onRecv_GSGTEnterServiceReq(GSGTEnterServiceReq proto) { }
        public virtual void onRecv_GSGTEnterServiceAck(GSGTEnterServiceAck proto) { }
        public virtual void onRecv_GSGTLeaveServiceNtf(GSGTLeaveServiceNtf proto) { }
        public virtual void onRecv_GSGTAbandonConnectionNtf(GSGTAbandonConnectionNtf proto) { }
        public virtual void onRecv_GSGTServiceRegisterReq(GSGTServiceRegisterReq proto) { }
        public virtual void onRecv_GSGTServiceRegisterAck(GSGTServiceRegisterAck proto) { }
    }
}
