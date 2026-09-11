using System;
using System.IO;

namespace JBPROTO
{
    /// <summary>
    /// 服务器信息
    /// </summary>
    public sealed class PFGSGameServerInfo : INetProtocol
    {
        /// <summary>
        /// 应用程序Id
        /// </summary>
        public string app_id;  //max:64
        /// <summary>
        /// 区服Id
        /// </summary>
        public int zone_id;
        /// <summary>
        /// 服务器类型 1网关 2平台 3游戏 4web
        /// </summary>
        public int server_type;
        /// <summary>
        /// 游戏服务组Id，唯一标识游戏组
        /// </summary>
        public int group_id;
        /// <summary>
        /// 游戏子服务Id，0代表主服务
        /// </summary>
        public int service_id;
        /// <summary>
        /// 公网Ip
        /// </summary>
        public string public_ip;  //max:32
        /// <summary>
        /// 私网Ip
        /// </summary>
        public string private_ip;  //max:32
        /// <summary>
        /// 监听端口1
        /// </summary>
        public int listen_port1;
        /// <summary>
        /// 监听端口2
        /// </summary>
        public int listen_port2;
        /// <summary>
        /// 处理的协议模块id，用逗号分隔
        /// </summary>
        public string modules;  //max:32
        /// <summary>
        /// 版本号
        /// </summary>
        public string version;  //max:32
        /// <summary>
        /// 编译时间
        /// </summary>
        public string compile_time;  //max:32
        /// <summary>
        /// 启动时间
        /// </summary>
        public string startup_time;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            NetHelper.SafeWriteString(bw, app_id, 64);
            bw.Write(zone_id);
            bw.Write(server_type);
            bw.Write(group_id);
            bw.Write(service_id);
            NetHelper.SafeWriteString(bw, public_ip, 32);
            NetHelper.SafeWriteString(bw, private_ip, 32);
            bw.Write(listen_port1);
            bw.Write(listen_port2);
            NetHelper.SafeWriteString(bw, modules, 32);
            NetHelper.SafeWriteString(bw, version, 32);
            NetHelper.SafeWriteString(bw, compile_time, 32);
            NetHelper.SafeWriteString(bw, startup_time, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                app_id = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                zone_id = br.ReadInt32();
                server_type = br.ReadInt32();
                group_id = br.ReadInt32();
                service_id = br.ReadInt32();
                public_ip = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                private_ip = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                listen_port1 = br.ReadInt32();
                listen_port2 = br.ReadInt32();
                modules = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                version = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                compile_time = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                startup_time = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 上报服务器信息
    /// </summary>
    public sealed class PFGSGameServerInfoRpt : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 0;
        /// <summary>
        /// 服务器信息
        /// </summary>
        public PFGSGameServerInfo my_info;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            my_info.toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                my_info = new PFGSGameServerInfo();
                my_info.fromBinary(br);
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 服务器加入通知
    /// </summary>
    public sealed class PFGSGameServerEnterNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 1;
        /// <summary>
        /// 服务器数组长度
        /// </summary>
        public int svr_len;
        /// <summary>
        /// 服务器数组
        /// </summary>
        public PFGSGameServerInfo[] svr_array;  //max:100
        /// <summary>
        /// 服务器数组（最大长度）
        /// </summary>
        public const int svr_array_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(svr_len);
            if (svr_len > svr_array_max_length)
                throw new Exception($"PFGSGameServerEnterNtf.svr_array数组长度超过规定限制，期望:100 实际:{svr_len}");
            for (int i = 0; i < (int)svr_len; i++)
                svr_array[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                svr_len = br.ReadInt32();
                svr_array = new PFGSGameServerInfo[(int)svr_len];
                for (int i = 0; i < svr_array.Length; i++)
                {
                    svr_array[i] = new PFGSGameServerInfo();
                    svr_array[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 服务器离开通知
    /// </summary>
    public sealed class PFGSGameServerLeaveNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 2;
        /// <summary>
        /// 应用程序Id
        /// </summary>
        public string app_id;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, app_id, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                app_id = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家上线通知
    /// </summary>
    public sealed class PFGSUserOnlineNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 3;
        /// <summary>
        /// 用户Id
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
    /// 玩家信息
    /// </summary>
    public sealed class PFGSUserInfo : INetProtocol
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 昵称
        /// </summary>
        public string nickname;  //max:32
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
        /// 战力
        /// </summary>
        public Int64 fight;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(user_id);
            NetHelper.SafeWriteString(bw, nickname, 32);
            bw.Write(gender);
            bw.Write(head);
            bw.Write(head_frame);
            bw.Write(title);
            bw.Write(dan);
            bw.Write(decorate);
            bw.Write(fight);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                nickname = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                gender = br.ReadInt32();
                head = br.ReadInt32();
                head_frame = br.ReadInt32();
                title = br.ReadInt32();
                dan = br.ReadInt32();
                decorate = br.ReadInt32();
                fight = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取用户信息请求
    /// </summary>
    public sealed class PFGSGetUserInfoReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 4;
        /// <summary>
        /// 用户Id
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
    /// 获取用户信息回应
    /// </summary>
    public sealed class PFGSGetUserInfoAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 5;
        /// <summary>
        /// 0成功 1玩家不存在
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 玩家信息结构
        /// </summary>
        public PFGSUserInfo info;
        /// <summary>
        /// ip地址
        /// </summary>
        public string ip;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            info.toBinary(bw);
            NetHelper.SafeWriteString(bw, ip, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                info = new PFGSUserInfo();
                info.fromBinary(br);
                ip = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 英雄信息
    /// </summary>
    public sealed class PFGSHeroInfo : INetProtocol
    {
        /// <summary>
        /// 英雄ID
        /// </summary>
        public int heroId;
        /// <summary>
        /// 星级
        /// </summary>
        public int star;
        /// <summary>
        /// 等级
        /// </summary>
        public int level;
        /// <summary>
        /// 武器
        /// </summary>
        public int weapon;
        /// <summary>
        /// 头盔
        /// </summary>
        public int helmet;
        /// <summary>
        /// 衣服
        /// </summary>
        public int clothes;
        /// <summary>
        /// 裤子
        /// </summary>
        public int trousers;
        /// <summary>
        /// 鞋子
        /// </summary>
        public int shoe;
        /// <summary>
        /// 血量
        /// </summary>
        public Int64 hp;
        /// <summary>
        /// 攻击
        /// </summary>
        public Int64 attack;
        /// <summary>
        /// 防御
        /// </summary>
        public Int64 defence;
        /// <summary>
        /// 速度
        /// </summary>
        public int speed;
        /// <summary>
        /// 暴击率
        /// </summary>
        public int crit_rate;
        /// <summary>
        /// 暴击伤害
        /// </summary>
        public int crit_hurt;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(heroId);
            bw.Write(star);
            bw.Write(level);
            bw.Write(weapon);
            bw.Write(helmet);
            bw.Write(clothes);
            bw.Write(trousers);
            bw.Write(shoe);
            bw.Write(hp);
            bw.Write(attack);
            bw.Write(defence);
            bw.Write(speed);
            bw.Write(crit_rate);
            bw.Write(crit_hurt);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                heroId = br.ReadInt32();
                star = br.ReadInt32();
                level = br.ReadInt32();
                weapon = br.ReadInt32();
                helmet = br.ReadInt32();
                clothes = br.ReadInt32();
                trousers = br.ReadInt32();
                shoe = br.ReadInt32();
                hp = br.ReadInt64();
                attack = br.ReadInt64();
                defence = br.ReadInt64();
                speed = br.ReadInt32();
                crit_rate = br.ReadInt32();
                crit_hurt = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 批量获取英雄信息请求
    /// </summary>
    public sealed class PFGSGetHeroInfoReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 6;
        /// <summary>
        /// 用户Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 英雄ID数组长度
        /// </summary>
        public int hero_id_len;
        /// <summary>
        /// 英雄的Id数组
        /// </summary>
        public int[] hero_ids;  //max:4
        /// <summary>
        /// 英雄的Id数组（最大长度）
        /// </summary>
        public const int hero_ids_max_length = 4;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(hero_id_len);
            if (hero_id_len > hero_ids_max_length)
                throw new Exception($"PFGSGetHeroInfoReq.hero_ids数组长度超过规定限制，期望:4 实际:{hero_id_len}");
            for (int i = 0; i < (int)hero_id_len; i++)
                bw.Write(hero_ids[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                hero_id_len = br.ReadInt32();
                hero_ids = new int[(int)hero_id_len];
                for (int i = 0; i < hero_ids.Length; i++)
                    hero_ids[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 批量获取英雄信息回应
    /// </summary>
    public sealed class PFGSGetHeroInfoAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 7;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int hero_len;
        /// <summary>
        /// 英雄数组
        /// </summary>
        public PFGSHeroInfo[] heros;  //max:4
        /// <summary>
        /// 英雄数组（最大长度）
        /// </summary>
        public const int heros_max_length = 4;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(hero_len);
            if (hero_len > heros_max_length)
                throw new Exception($"PFGSGetHeroInfoAck.heros数组长度超过规定限制，期望:4 实际:{hero_len}");
            for (int i = 0; i < (int)hero_len; i++)
                heros[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                hero_len = br.ReadInt32();
                heros = new PFGSHeroInfo[(int)hero_len];
                for (int i = 0; i < heros.Length; i++)
                {
                    heros[i] = new PFGSHeroInfo();
                    heros[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 添加资源变动监听请求
    /// </summary>
    public sealed class PFGSResAddListenerReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 8;
        /// <summary>
        /// 用户Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 1添加监听 2释放监听
        /// </summary>
        public int action;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(action);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                action = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 添加资源变动监听回应
    /// </summary>
    public sealed class PFGSResAddListenerAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 9;
        /// <summary>
        /// 0成功 1系统错误
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
    /// 资源同步通知
    /// </summary>
    public sealed class PFGSResSyncNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 10;
        /// <summary>
        /// 用户Id
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
    /// 物品信息结构
    /// </summary>
    public sealed class PFGSItemInfo : INetProtocol
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
    /// 物品数量变化请求
    /// </summary>
    public sealed class PFGSItemChangeCountReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 11;
        /// <summary>
        /// 用户Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public PFGSItemInfo[] items;  //max:100
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;
        /// <summary>
        /// 资源修改原因
        /// </summary>
        public int reason;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"PFGSItemChangeCountReq.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
            bw.Write(reason);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                item_len = br.ReadInt32();
                items = new PFGSItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new PFGSItemInfo();
                    items[i].fromBinary(br);
                }
                reason = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 物品数量变化回应
    /// </summary>
    public sealed class PFGSItemChangeCountAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 12;
        /// <summary>
        /// 0成功 1物品不足 2用户不存在
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
    /// 使用物品请求
    /// </summary>
    public sealed class PFGSItemUseReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 13;
        /// <summary>
        /// 用户Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 物品信息
        /// </summary>
        public PFGSItemInfo item;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            item.toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                item = new PFGSItemInfo();
                item.fromBinary(br);
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 使用物品回应
    /// </summary>
    public sealed class PFGSItemUseAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 14;
        /// <summary>
        /// 0成功 其他同物品使用失败错误码
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
    /// 资源和道具修改请求
    /// </summary>
    public sealed class PFGSResAndItemsModifyReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 15;
        /// <summary>
        /// 用户Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 钻石变化量
        /// </summary>
        public Int64 diamond_delta;
        /// <summary>
        /// 非绑定金币变化量
        /// </summary>
        public Int64 unbind_currency_delta;
        /// <summary>
        /// 绑定金币变化量
        /// </summary>
        public Int64 bind_currency_delta;
        /// <summary>
        /// 统一金币变化量
        /// </summary>
        public Int64 currency_delta;
        /// <summary>
        /// 积分变化量
        /// </summary>
        public Int64 integral_delta;
        /// <summary>
        /// 乱斗变化量
        /// </summary>
        public Int64 fight_delta;
        /// <summary>
        /// 修改原因
        /// </summary>
        public int reason;
        /// <summary>
        /// 是否通知客户端 1是0否
        /// </summary>
        public sbyte is_notify;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public PFGSItemInfo[] items;  //max:100
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(diamond_delta);
            bw.Write(unbind_currency_delta);
            bw.Write(bind_currency_delta);
            bw.Write(currency_delta);
            bw.Write(integral_delta);
            bw.Write(fight_delta);
            bw.Write(reason);
            bw.Write(is_notify);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"PFGSResAndItemsModifyReq.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                diamond_delta = br.ReadInt64();
                unbind_currency_delta = br.ReadInt64();
                bind_currency_delta = br.ReadInt64();
                currency_delta = br.ReadInt64();
                integral_delta = br.ReadInt64();
                fight_delta = br.ReadInt64();
                reason = br.ReadInt32();
                is_notify = br.ReadSByte();
                item_len = br.ReadInt32();
                items = new PFGSItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new PFGSItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 资源和道具修改回应
    /// </summary>
    public sealed class PFGSResAndItemsModifyAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 16;
        /// <summary>
        /// 0成功 1资源不足 2道具不足 3用户不存在
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 钻石最新值
        /// </summary>
        public Int64 diamond;
        /// <summary>
        /// 金币最新值
        /// </summary>
        public Int64 currency;
        /// <summary>
        /// 金币变化量
        /// </summary>
        public Int64 currency_delta;
        /// <summary>
        /// 绑定金币最新值
        /// </summary>
        public Int64 bind_currency;
        /// <summary>
        /// 绑定金币变化量
        /// </summary>
        public Int64 bind_currency_delta;
        /// <summary>
        /// 积分最新值
        /// </summary>
        public Int64 integral;
        /// <summary>
        /// 乱斗最新值
        /// </summary>
        public Int64 fight;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(diamond);
            bw.Write(currency);
            bw.Write(currency_delta);
            bw.Write(bind_currency);
            bw.Write(bind_currency_delta);
            bw.Write(integral);
            bw.Write(fight);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                diamond = br.ReadInt64();
                currency = br.ReadInt64();
                currency_delta = br.ReadInt64();
                bind_currency = br.ReadInt64();
                bind_currency_delta = br.ReadInt64();
                integral = br.ReadInt64();
                fight = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 发送邮件请求
    /// </summary>
    public sealed class PFGSMailSendReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 17;
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
    public sealed class PFGSMailSendAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 18;
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
    /// 游戏事件通知，上报到平台，用作数据统计
    /// </summary>
    public sealed class PFGSGameEventRpt : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 19;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 事件类型
        /// </summary>
        public int event_type;
        /// <summary>
        /// 数量
        /// </summary>
        public Int64 count;
        /// <summary>
        /// 配置Id(鱼或者道具)
        /// </summary>
        public int config_id;
        /// <summary>
        /// 鱼类型
        /// </summary>
        public int fish_type;
        /// <summary>
        /// 炮倍
        /// </summary>
        public Int64 gun_value;
        /// <summary>
        /// 效果
        /// </summary>
        public int item_effect;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(event_type);
            bw.Write(count);
            bw.Write(config_id);
            bw.Write(fish_type);
            bw.Write(gun_value);
            bw.Write(item_effect);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                event_type = br.ReadInt32();
                count = br.ReadInt64();
                config_id = br.ReadInt32();
                fish_type = br.ReadInt32();
                gun_value = br.ReadInt64();
                item_effect = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家加入或离开服务通知
    /// </summary>
    public sealed class PFGSOnlineLocationAccessRpt : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 20;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 1加入服务 2离开服务
        /// </summary>
        public int type;
        /// <summary>
        /// 服务组Id
        /// </summary>
        public int group_id;
        /// <summary>
        /// 服务子Id
        /// </summary>
        public int service_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(type);
            bw.Write(group_id);
            bw.Write(service_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                type = br.ReadInt32();
                group_id = br.ReadInt32();
                service_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家详细位置更新通知
    /// </summary>
    public sealed class PFGSOnlineLocationUpdateRpt : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 21;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 服务组Id
        /// </summary>
        public int group_id;
        /// <summary>
        /// 服务子Id
        /// </summary>
        public int service_id;
        /// <summary>
        /// 位置相关的附加数据1--房间Id
        /// </summary>
        public int extra1;
        /// <summary>
        /// 位置相关的附加数据2--台子Id
        /// </summary>
        public int extra2;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(group_id);
            bw.Write(service_id);
            bw.Write(extra1);
            bw.Write(extra2);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                group_id = br.ReadInt32();
                service_id = br.ReadInt32();
                extra1 = br.ReadInt32();
                extra2 = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 资源和道具探测是否足够请求，注意协议中的值需为负数
    /// </summary>
    public sealed class PFGSResAndItemsTestEnoughReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 22;
        /// <summary>
        /// 用户Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 钻石变化量
        /// </summary>
        public Int64 diamond_delta;
        /// <summary>
        /// 非绑定金币变化量
        /// </summary>
        public Int64 unbind_currency_delta;
        /// <summary>
        /// 绑定金币变化量
        /// </summary>
        public Int64 bind_currency_delta;
        /// <summary>
        /// 统一金币变化量
        /// </summary>
        public Int64 currency_delta;
        /// <summary>
        /// 积分变化量
        /// </summary>
        public Int64 integral_delta;
        /// <summary>
        /// 乱斗变化量
        /// </summary>
        public Int64 fight_delta;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public PFGSItemInfo[] items;  //max:100
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(diamond_delta);
            bw.Write(unbind_currency_delta);
            bw.Write(bind_currency_delta);
            bw.Write(currency_delta);
            bw.Write(integral_delta);
            bw.Write(fight_delta);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"PFGSResAndItemsTestEnoughReq.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                diamond_delta = br.ReadInt64();
                unbind_currency_delta = br.ReadInt64();
                bind_currency_delta = br.ReadInt64();
                currency_delta = br.ReadInt64();
                integral_delta = br.ReadInt64();
                fight_delta = br.ReadInt64();
                item_len = br.ReadInt32();
                items = new PFGSItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new PFGSItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 资源和道具探测是否足够回应
    /// </summary>
    public sealed class PFGSResAndItemsTestEnoughAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 23;
        /// <summary>
        /// 0足够 1资源不足 2道具不足 3用户不存在
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
    /// 查询好友房间桌子是否存在请求
    /// </summary>
    public sealed class PFGSPrivateRoomDeskQueryReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 24;
        /// <summary>
        /// 房间号
        /// </summary>
        public int desk_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(desk_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                desk_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询好友房间桌子是否存在回应
    /// </summary>
    public sealed class PFGSPrivateRoomDeskQueryAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 25;
        /// <summary>
        /// 0存在 1不存在
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
    /// 批量获取玩家信息请求
    /// </summary>
    public sealed class PFGSBatchGetUserInfoReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 26;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int id_length;
        /// <summary>
        /// 数组
        /// </summary>
        public int[] id_array;  //max:100
        /// <summary>
        /// 数组（最大长度）
        /// </summary>
        public const int id_array_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(id_length);
            if (id_length > id_array_max_length)
                throw new Exception($"PFGSBatchGetUserInfoReq.id_array数组长度超过规定限制，期望:100 实际:{id_length}");
            for (int i = 0; i < (int)id_length; i++)
                bw.Write(id_array[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                id_length = br.ReadInt32();
                id_array = new int[(int)id_length];
                for (int i = 0; i < id_array.Length; i++)
                    id_array[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 批量获取玩家信息回应
    /// </summary>
    public sealed class PFGSBatchGetUserInfoAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 27;
        /// <summary>
        /// 玩家数组长度
        /// </summary>
        public int user_length;
        /// <summary>
        /// 玩家数组
        /// </summary>
        public PFGSUserInfo[] user_array;  //max:100
        /// <summary>
        /// 玩家数组（最大长度）
        /// </summary>
        public const int user_array_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_length);
            if (user_length > user_array_max_length)
                throw new Exception($"PFGSBatchGetUserInfoAck.user_array数组长度超过规定限制，期望:100 实际:{user_length}");
            for (int i = 0; i < (int)user_length; i++)
                user_array[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_length = br.ReadInt32();
                user_array = new PFGSUserInfo[(int)user_length];
                for (int i = 0; i < user_array.Length; i++)
                {
                    user_array[i] = new PFGSUserInfo();
                    user_array[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询玩家是否在某个游戏服务中请求
    /// </summary>
    public sealed class PFGSUserInGameServerQueryReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 28;
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
    /// 查询玩家是否在某个游戏服务中回应
    /// </summary>
    public sealed class PFGSUserInGameServerQueryAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 29;
        /// <summary>
        /// 0玩家不在任何游戏服务中 1玩家已加入某个游戏服务
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
    /// 装备炮台通知
    /// </summary>
    public sealed class PFGSGunEquipNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 30;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 炮台Id
        /// </summary>
        public int gun_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(gun_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                gun_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 装备翅膀通知
    /// </summary>
    public sealed class PFGSWingEquipNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 31;
        /// <summary>
        /// 玩家Id
        /// </summary>
        public int user_id;
        /// <summary>
        /// 翅膀Id
        /// </summary>
        public int wing_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(user_id);
            bw.Write(wing_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                wing_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 活动开启关闭通知
    /// </summary>
    public sealed class PFGSActivityOpenCloseNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 6;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 32;
        /// <summary>
        /// 活动id
        /// </summary>
        public int activity_id;
        /// <summary>
        /// 类型1开启 0关闭
        /// </summary>
        public int act_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(activity_id);
            bw.Write(act_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                activity_id = br.ReadInt32();
                act_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }

    public class PlatformGSResponserBase : INetResponser
    {
        public bool processPackage(BinaryReader br, INetReactor reactor, out INetProtocol responseProto)
        {
            responseProto = null;
            if (br.ReadUInt16() != 6)
                return false;

            switch(br.ReadUInt16())
            {
                case 0:
                    responseProto = new PFGSGameServerInfoRpt();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSGameServerInfoRpt(responseProto as PFGSGameServerInfoRpt);
                    break;
                case 1:
                    responseProto = new PFGSGameServerEnterNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSGameServerEnterNtf(responseProto as PFGSGameServerEnterNtf);
                    break;
                case 2:
                    responseProto = new PFGSGameServerLeaveNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSGameServerLeaveNtf(responseProto as PFGSGameServerLeaveNtf);
                    break;
                case 3:
                    responseProto = new PFGSUserOnlineNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSUserOnlineNtf(responseProto as PFGSUserOnlineNtf);
                    break;
                case 4:
                    responseProto = new PFGSGetUserInfoReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSGetUserInfoReq(responseProto as PFGSGetUserInfoReq);
                    break;
                case 5:
                    responseProto = new PFGSGetUserInfoAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSGetUserInfoAck(responseProto as PFGSGetUserInfoAck);
                    break;
                case 6:
                    responseProto = new PFGSGetHeroInfoReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSGetHeroInfoReq(responseProto as PFGSGetHeroInfoReq);
                    break;
                case 7:
                    responseProto = new PFGSGetHeroInfoAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSGetHeroInfoAck(responseProto as PFGSGetHeroInfoAck);
                    break;
                case 8:
                    responseProto = new PFGSResAddListenerReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSResAddListenerReq(responseProto as PFGSResAddListenerReq);
                    break;
                case 9:
                    responseProto = new PFGSResAddListenerAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSResAddListenerAck(responseProto as PFGSResAddListenerAck);
                    break;
                case 10:
                    responseProto = new PFGSResSyncNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSResSyncNtf(responseProto as PFGSResSyncNtf);
                    break;
                case 11:
                    responseProto = new PFGSItemChangeCountReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSItemChangeCountReq(responseProto as PFGSItemChangeCountReq);
                    break;
                case 12:
                    responseProto = new PFGSItemChangeCountAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSItemChangeCountAck(responseProto as PFGSItemChangeCountAck);
                    break;
                case 13:
                    responseProto = new PFGSItemUseReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSItemUseReq(responseProto as PFGSItemUseReq);
                    break;
                case 14:
                    responseProto = new PFGSItemUseAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSItemUseAck(responseProto as PFGSItemUseAck);
                    break;
                case 15:
                    responseProto = new PFGSResAndItemsModifyReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSResAndItemsModifyReq(responseProto as PFGSResAndItemsModifyReq);
                    break;
                case 16:
                    responseProto = new PFGSResAndItemsModifyAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSResAndItemsModifyAck(responseProto as PFGSResAndItemsModifyAck);
                    break;
                case 17:
                    responseProto = new PFGSMailSendReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSMailSendReq(responseProto as PFGSMailSendReq);
                    break;
                case 18:
                    responseProto = new PFGSMailSendAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSMailSendAck(responseProto as PFGSMailSendAck);
                    break;
                case 19:
                    responseProto = new PFGSGameEventRpt();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSGameEventRpt(responseProto as PFGSGameEventRpt);
                    break;
                case 20:
                    responseProto = new PFGSOnlineLocationAccessRpt();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSOnlineLocationAccessRpt(responseProto as PFGSOnlineLocationAccessRpt);
                    break;
                case 21:
                    responseProto = new PFGSOnlineLocationUpdateRpt();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSOnlineLocationUpdateRpt(responseProto as PFGSOnlineLocationUpdateRpt);
                    break;
                case 22:
                    responseProto = new PFGSResAndItemsTestEnoughReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSResAndItemsTestEnoughReq(responseProto as PFGSResAndItemsTestEnoughReq);
                    break;
                case 23:
                    responseProto = new PFGSResAndItemsTestEnoughAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSResAndItemsTestEnoughAck(responseProto as PFGSResAndItemsTestEnoughAck);
                    break;
                case 24:
                    responseProto = new PFGSPrivateRoomDeskQueryReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSPrivateRoomDeskQueryReq(responseProto as PFGSPrivateRoomDeskQueryReq);
                    break;
                case 25:
                    responseProto = new PFGSPrivateRoomDeskQueryAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSPrivateRoomDeskQueryAck(responseProto as PFGSPrivateRoomDeskQueryAck);
                    break;
                case 26:
                    responseProto = new PFGSBatchGetUserInfoReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSBatchGetUserInfoReq(responseProto as PFGSBatchGetUserInfoReq);
                    break;
                case 27:
                    responseProto = new PFGSBatchGetUserInfoAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSBatchGetUserInfoAck(responseProto as PFGSBatchGetUserInfoAck);
                    break;
                case 28:
                    responseProto = new PFGSUserInGameServerQueryReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSUserInGameServerQueryReq(responseProto as PFGSUserInGameServerQueryReq);
                    break;
                case 29:
                    responseProto = new PFGSUserInGameServerQueryAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSUserInGameServerQueryAck(responseProto as PFGSUserInGameServerQueryAck);
                    break;
                case 30:
                    responseProto = new PFGSGunEquipNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSGunEquipNtf(responseProto as PFGSGunEquipNtf);
                    break;
                case 31:
                    responseProto = new PFGSWingEquipNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSWingEquipNtf(responseProto as PFGSWingEquipNtf);
                    break;
                case 32:
                    responseProto = new PFGSActivityOpenCloseNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_PFGSActivityOpenCloseNtf(responseProto as PFGSActivityOpenCloseNtf);
                    break;
            }
            return responseProto != null;
        }

        public virtual void onRecv_PFGSGameServerInfoRpt(PFGSGameServerInfoRpt proto) { }
        public virtual void onRecv_PFGSGameServerEnterNtf(PFGSGameServerEnterNtf proto) { }
        public virtual void onRecv_PFGSGameServerLeaveNtf(PFGSGameServerLeaveNtf proto) { }
        public virtual void onRecv_PFGSUserOnlineNtf(PFGSUserOnlineNtf proto) { }
        public virtual void onRecv_PFGSGetUserInfoReq(PFGSGetUserInfoReq proto) { }
        public virtual void onRecv_PFGSGetUserInfoAck(PFGSGetUserInfoAck proto) { }
        public virtual void onRecv_PFGSGetHeroInfoReq(PFGSGetHeroInfoReq proto) { }
        public virtual void onRecv_PFGSGetHeroInfoAck(PFGSGetHeroInfoAck proto) { }
        public virtual void onRecv_PFGSResAddListenerReq(PFGSResAddListenerReq proto) { }
        public virtual void onRecv_PFGSResAddListenerAck(PFGSResAddListenerAck proto) { }
        public virtual void onRecv_PFGSResSyncNtf(PFGSResSyncNtf proto) { }
        public virtual void onRecv_PFGSItemChangeCountReq(PFGSItemChangeCountReq proto) { }
        public virtual void onRecv_PFGSItemChangeCountAck(PFGSItemChangeCountAck proto) { }
        public virtual void onRecv_PFGSItemUseReq(PFGSItemUseReq proto) { }
        public virtual void onRecv_PFGSItemUseAck(PFGSItemUseAck proto) { }
        public virtual void onRecv_PFGSResAndItemsModifyReq(PFGSResAndItemsModifyReq proto) { }
        public virtual void onRecv_PFGSResAndItemsModifyAck(PFGSResAndItemsModifyAck proto) { }
        public virtual void onRecv_PFGSMailSendReq(PFGSMailSendReq proto) { }
        public virtual void onRecv_PFGSMailSendAck(PFGSMailSendAck proto) { }
        public virtual void onRecv_PFGSGameEventRpt(PFGSGameEventRpt proto) { }
        public virtual void onRecv_PFGSOnlineLocationAccessRpt(PFGSOnlineLocationAccessRpt proto) { }
        public virtual void onRecv_PFGSOnlineLocationUpdateRpt(PFGSOnlineLocationUpdateRpt proto) { }
        public virtual void onRecv_PFGSResAndItemsTestEnoughReq(PFGSResAndItemsTestEnoughReq proto) { }
        public virtual void onRecv_PFGSResAndItemsTestEnoughAck(PFGSResAndItemsTestEnoughAck proto) { }
        public virtual void onRecv_PFGSPrivateRoomDeskQueryReq(PFGSPrivateRoomDeskQueryReq proto) { }
        public virtual void onRecv_PFGSPrivateRoomDeskQueryAck(PFGSPrivateRoomDeskQueryAck proto) { }
        public virtual void onRecv_PFGSBatchGetUserInfoReq(PFGSBatchGetUserInfoReq proto) { }
        public virtual void onRecv_PFGSBatchGetUserInfoAck(PFGSBatchGetUserInfoAck proto) { }
        public virtual void onRecv_PFGSUserInGameServerQueryReq(PFGSUserInGameServerQueryReq proto) { }
        public virtual void onRecv_PFGSUserInGameServerQueryAck(PFGSUserInGameServerQueryAck proto) { }
        public virtual void onRecv_PFGSGunEquipNtf(PFGSGunEquipNtf proto) { }
        public virtual void onRecv_PFGSWingEquipNtf(PFGSWingEquipNtf proto) { }
        public virtual void onRecv_PFGSActivityOpenCloseNtf(PFGSActivityOpenCloseNtf proto) { }
    }
}
