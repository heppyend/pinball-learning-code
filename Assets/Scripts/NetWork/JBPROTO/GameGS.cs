using System;
using System.IO;

namespace JBPROTO
{
    /// <summary>
    /// 捕鱼房间玩家数量信息
    /// </summary>
    public sealed class GSGSFishingRoomUserCountDetailInfo : INetProtocol
    {
        /// <summary>
        /// 房间配置Id
        /// </summary>
        public int config_id;
        /// <summary>
        /// 房间人数信息数组
        /// </summary>
        public sbyte[] amount_array;  //max:100
        /// <summary>
        /// 房间人数信息数组（最大长度）
        /// </summary>
        public const int amount_array_max_length = 100;
        /// <summary>
        /// 该场次玩家的总人数
        /// </summary>
        public int total_count;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(config_id);
            for (int i = 0; i < 100; i++)
                bw.Write(amount_array[i]);
            bw.Write(total_count);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                config_id = br.ReadInt32();
                amount_array = new sbyte[100];
                for (int i = 0; i < amount_array.Length; i++)
                    amount_array[i] = br.ReadSByte();
                total_count = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 订阅房间人数变动请求
    /// </summary>
    public sealed class GSGSFishingRoomUserCountSubscribeReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 4;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 0;

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
    /// 订阅房间人数变动回应
    /// </summary>
    public sealed class GSGSFishingRoomUserCountSubscribeAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 4;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 1;
        /// <summary>
        /// 玩家数量数组个数
        /// </summary>
        public int info_len;
        /// <summary>
        /// 玩家数量数组
        /// </summary>
        public GSGSFishingRoomUserCountDetailInfo[] info_array;  //max:20
        /// <summary>
        /// 玩家数量数组（最大长度）
        /// </summary>
        public const int info_array_max_length = 20;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(info_len);
            if (info_len > info_array_max_length)
                throw new Exception($"GSGSFishingRoomUserCountSubscribeAck.info_array数组长度超过规定限制，期望:20 实际:{info_len}");
            for (int i = 0; i < (int)info_len; i++)
                info_array[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                info_len = br.ReadInt32();
                info_array = new GSGSFishingRoomUserCountDetailInfo[(int)info_len];
                for (int i = 0; i < info_array.Length; i++)
                {
                    info_array[i] = new GSGSFishingRoomUserCountDetailInfo();
                    info_array[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家进入或离开某房间
    /// </summary>
    public sealed class GSGSFishingRoomUserCountChangeNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 4;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 2;
        /// <summary>
        /// 房间配置Id
        /// </summary>
        public int config_id;
        /// <summary>
        /// 1加入 2离开
        /// </summary>
        public sbyte action;
        /// <summary>
        /// 房间Id
        /// </summary>
        public int room_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(config_id);
            bw.Write(action);
            bw.Write(room_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                config_id = br.ReadInt32();
                action = br.ReadSByte();
                room_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 房间服务器分配Id通知
    /// </summary>
    public sealed class GSGSRoomAssignServerIdNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 4;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 3;
        /// <summary>
        /// 服务器Id
        /// </summary>
        public int server_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(server_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                server_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 房间设置或清除密码
    /// </summary>
    public sealed class GSGSFishingRoomPasswordChangeNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 4;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 4;
        /// <summary>
        /// 房间配置Id
        /// </summary>
        public int config_id;
        /// <summary>
        /// 1设置密码 2清除
        /// </summary>
        public sbyte action;
        /// <summary>
        /// 房间Id
        /// </summary>
        public int room_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(config_id);
            bw.Write(action);
            bw.Write(room_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                config_id = br.ReadInt32();
                action = br.ReadSByte();
                room_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }

    public class GameGSResponserBase : INetResponser
    {
        public bool processPackage(BinaryReader br, INetReactor reactor, out INetProtocol responseProto)
        {
            responseProto = null;
            if (br.ReadUInt16() != 4)
                return false;

            switch(br.ReadUInt16())
            {
                case 0:
                    responseProto = new GSGSFishingRoomUserCountSubscribeReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGSFishingRoomUserCountSubscribeReq(responseProto as GSGSFishingRoomUserCountSubscribeReq);
                    break;
                case 1:
                    responseProto = new GSGSFishingRoomUserCountSubscribeAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGSFishingRoomUserCountSubscribeAck(responseProto as GSGSFishingRoomUserCountSubscribeAck);
                    break;
                case 2:
                    responseProto = new GSGSFishingRoomUserCountChangeNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGSFishingRoomUserCountChangeNtf(responseProto as GSGSFishingRoomUserCountChangeNtf);
                    break;
                case 3:
                    responseProto = new GSGSRoomAssignServerIdNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGSRoomAssignServerIdNtf(responseProto as GSGSRoomAssignServerIdNtf);
                    break;
                case 4:
                    responseProto = new GSGSFishingRoomPasswordChangeNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_GSGSFishingRoomPasswordChangeNtf(responseProto as GSGSFishingRoomPasswordChangeNtf);
                    break;
            }
            return responseProto != null;
        }

        public virtual void onRecv_GSGSFishingRoomUserCountSubscribeReq(GSGSFishingRoomUserCountSubscribeReq proto) { }
        public virtual void onRecv_GSGSFishingRoomUserCountSubscribeAck(GSGSFishingRoomUserCountSubscribeAck proto) { }
        public virtual void onRecv_GSGSFishingRoomUserCountChangeNtf(GSGSFishingRoomUserCountChangeNtf proto) { }
        public virtual void onRecv_GSGSRoomAssignServerIdNtf(GSGSRoomAssignServerIdNtf proto) { }
        public virtual void onRecv_GSGSFishingRoomPasswordChangeNtf(GSGSFishingRoomPasswordChangeNtf proto) { }
    }
}
