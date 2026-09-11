using System;
using System.IO;

namespace JBPROTO
{
    /// <summary>
    /// 登入服务通知
    /// </summary>
    public sealed class CLFMEnterServerNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 10;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 0;
        /// <summary>
        /// 当前的炮台Id
        /// </summary>
        public int gun_id;
        /// <summary>
        /// 解锁的最大炮值
        /// </summary>
        public Int64 max_gun_value;
        /// <summary>
        /// 个人累积的奖金池数量
        /// </summary>
        public Int64 bonus_pool;
        /// <summary>
        /// 个人累积的打死奖金鱼数量
        /// </summary>
        public int bonus_count;
        /// <summary>
        /// 倍击倍数
        /// </summary>
        public int multiple_hit;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(gun_id);
            bw.Write(max_gun_value);
            bw.Write(bonus_pool);
            bw.Write(bonus_count);
            bw.Write(multiple_hit);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                gun_id = br.ReadInt32();
                max_gun_value = br.ReadInt64();
                bonus_pool = br.ReadInt64();
                bonus_count = br.ReadInt32();
                multiple_hit = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 加入玩法请求
    /// </summary>
    public sealed class CLFMEnterSiteReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 10;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 1;
        /// <summary>
        /// 玩法Id 1捕鱼3D 2捕鱼2D 3竖版捕鱼 4集结号捕鱼 5超凡捕鱼 6深海捕鱼 7乱斗捕鱼
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
    /// 加入玩法回应
    /// </summary>
    public sealed class CLFMEnterSiteAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 10;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 2;
        /// <summary>
        /// 0成功 1无可用服务器 2系统错误
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
    /// 退出玩法请求
    /// </summary>
    public sealed class CLFMExitSiteReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 10;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 3;
        /// <summary>
        /// 玩法Id 1捕鱼3D 2捕鱼2D 3竖版捕鱼
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
    /// 退出玩法回应
    /// </summary>
    public sealed class CLFMExitSiteAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 10;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 4;
        /// <summary>
        /// 0成功
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

    public class ClientFishingMainResponserBase : INetResponser
    {
        public bool processPackage(BinaryReader br, INetReactor reactor, out INetProtocol responseProto)
        {
            responseProto = null;
            if (br.ReadUInt16() != 10)
                return false;

            switch(br.ReadUInt16())
            {
                case 0:
                    responseProto = new CLFMEnterServerNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFMEnterServerNtf(responseProto as CLFMEnterServerNtf);
                    break;
                case 1:
                    responseProto = new CLFMEnterSiteReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFMEnterSiteReq(responseProto as CLFMEnterSiteReq);
                    break;
                case 2:
                    responseProto = new CLFMEnterSiteAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFMEnterSiteAck(responseProto as CLFMEnterSiteAck);
                    break;
                case 3:
                    responseProto = new CLFMExitSiteReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFMExitSiteReq(responseProto as CLFMExitSiteReq);
                    break;
                case 4:
                    responseProto = new CLFMExitSiteAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFMExitSiteAck(responseProto as CLFMExitSiteAck);
                    break;
            }
            return responseProto != null;
        }

        public virtual void onRecv_CLFMEnterServerNtf(CLFMEnterServerNtf proto) { }
        public virtual void onRecv_CLFMEnterSiteReq(CLFMEnterSiteReq proto) { }
        public virtual void onRecv_CLFMEnterSiteAck(CLFMEnterSiteAck proto) { }
        public virtual void onRecv_CLFMExitSiteReq(CLFMExitSiteReq proto) { }
        public virtual void onRecv_CLFMExitSiteAck(CLFMExitSiteAck proto) { }
    }
}
