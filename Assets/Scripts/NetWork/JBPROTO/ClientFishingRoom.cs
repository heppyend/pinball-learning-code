using System;
using System.IO;

namespace JBPROTO
{
    /// <summary>
    /// 房间内玩家信息
    /// </summary>
    public sealed class CLFRRoomPlayerInfo : INetProtocol
    {
        /// <summary>
        /// 玩家Id
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
    /// 进入游戏请求
    /// </summary>
    public sealed class CLFREnterGameReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 0;
        /// <summary>
        /// 房间配置Id，对应房间配置表
        /// </summary>
        public int config_id;
        /// <summary>
        /// 指定房间Id，-1代表随机分配
        /// </summary>
        public int room_id;
        /// <summary>
        /// 指定座位Id，-1代表随机分配
        /// </summary>
        public int seat_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(config_id);
            bw.Write(room_id);
            bw.Write(seat_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                config_id = br.ReadInt32();
                room_id = br.ReadInt32();
                seat_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 进入游戏回应
    /// </summary>
    public sealed class CLFREnterGameAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 1;
        /// <summary>
        /// 0成功 1房间未开放 2房间已满 3指定的座位已被占用 4系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 当前时间戳
        /// </summary>
        public UInt64 time_stamp;
        /// <summary>
        /// 房间Id
        /// </summary>
        public int room_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(time_stamp);
            bw.Write(room_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                time_stamp = br.ReadUInt64();
                room_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 退出游戏请求
    /// </summary>
    public sealed class CLFRExitGameReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 2;

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
    /// 退出游戏回应
    /// </summary>
    public sealed class CLFRExitGameAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 3;
        /// <summary>
        /// 0成功 1不在房间中
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
    /// 我已准备好，可以接收数据了
    /// </summary>
    public sealed class CLFRGetReadyReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 4;

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
    /// 当前房间内玩家信息
    /// </summary>
    public sealed class CLFRGetReadyAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 5;
        /// <summary>
        /// 0成功 1系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 房间内玩家数量
        /// </summary>
        public int player_len;
        /// <summary>
        /// 房间内玩家信息
        /// </summary>
        public CLFRRoomPlayerInfo[] players;  //max:2
        /// <summary>
        /// 房间内玩家信息（最大长度）
        /// </summary>
        public const int players_max_length = 2;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(player_len);
            if (player_len > players_max_length)
                throw new Exception($"CLFRGetReadyAck.players数组长度超过规定限制，期望:2 实际:{player_len}");
            for (int i = 0; i < (int)player_len; i++)
                players[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                player_len = br.ReadInt32();
                players = new CLFRRoomPlayerInfo[(int)player_len];
                for (int i = 0; i < players.Length; i++)
                {
                    players[i] = new CLFRRoomPlayerInfo();
                    players[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家加入通知
    /// </summary>
    public sealed class CLFRPlayerJoinNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 6;
        /// <summary>
        /// 玩家信息
        /// </summary>
        public CLFRRoomPlayerInfo player;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            player.toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                player = new CLFRRoomPlayerInfo();
                player.fromBinary(br);
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 玩家离开通知
    /// </summary>
    public sealed class CLFRPlayerLeaveNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 7;
        /// <summary>
        /// 位置：0~1
        /// </summary>
        public int seat_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(seat_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                seat_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 进入密码房游戏请求
    /// </summary>
    public sealed class CLFREnterGameWithPasswordReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 8;
        /// <summary>
        /// 房间配置Id，对应房间配置表
        /// </summary>
        public int config_id;
        /// <summary>
        /// 指定房间Id，-1代表随机分配
        /// </summary>
        public int room_id;
        /// <summary>
        /// 指定座位Id，-1代表随机分配
        /// </summary>
        public int seat_id;
        /// <summary>
        /// 房间密码
        /// </summary>
        public int password;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(config_id);
            bw.Write(room_id);
            bw.Write(seat_id);
            bw.Write(password);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                config_id = br.ReadInt32();
                room_id = br.ReadInt32();
                seat_id = br.ReadInt32();
                password = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 进入密码房游戏回应
    /// </summary>
    public sealed class CLFREnterGameWithPasswordAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 9;
        /// <summary>
        /// 0成功 1条件不足尚未解锁 2房间未开放 3房间已满 4指定的座位  已被占用 5密码错误 6房间状态有误 7系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 当前时间戳
        /// </summary>
        public UInt64 time_stamp;
        /// <summary>
        /// 房间Id
        /// </summary>
        public int room_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(time_stamp);
            bw.Write(room_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                time_stamp = br.ReadUInt64();
                room_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 设置房间密码请求
    /// </summary>
    public sealed class CLFRSetRoomPasswordReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 10;
        /// <summary>
        /// 房间密码
        /// </summary>
        public int password;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(password);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                password = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 设置房间密码回应
    /// </summary>
    public sealed class CLFRSetRoomPasswordAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 11;
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
    /// 房间状态通知
    /// </summary>
    public sealed class CLFRRoomStatusNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 12;
        /// <summary>
        /// 房间状态0:匹配状态 1:组队状态 2:战斗状态 3:结束状态
        /// </summary>
        public int status;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(status);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                status = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 英雄组队请求
    /// </summary>
    public sealed class CLFRHeroTeamReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 13;
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
            bw.Write(hero_id_len);
            if (hero_id_len > hero_ids_max_length)
                throw new Exception($"CLFRHeroTeamReq.hero_ids数组长度超过规定限制，期望:4 实际:{hero_id_len}");
            for (int i = 0; i < (int)hero_id_len; i++)
                bw.Write(hero_ids[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                hero_id_len = br.ReadInt32();
                hero_ids = new int[(int)hero_id_len];
                for (int i = 0; i < hero_ids.Length; i++)
                    hero_ids[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 英雄组队回应
    /// </summary>
    public sealed class CLFRHeroTeamAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 14;
        /// <summary>
        /// 0成功 1英雄ID有误 2系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 总血量
        /// </summary>
        public Int64 total_hp;
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
            bw.Write(errcode);
            bw.Write(total_hp);
            bw.Write(hero_id_len);
            if (hero_id_len > hero_ids_max_length)
                throw new Exception($"CLFRHeroTeamAck.hero_ids数组长度超过规定限制，期望:4 实际:{hero_id_len}");
            for (int i = 0; i < (int)hero_id_len; i++)
                bw.Write(hero_ids[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                total_hp = br.ReadInt64();
                hero_id_len = br.ReadInt32();
                hero_ids = new int[(int)hero_id_len];
                for (int i = 0; i < hero_ids.Length; i++)
                    hero_ids[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 怪物出现信息
    /// </summary>
    public sealed class CLFRMonsterAppearInfo : INetProtocol
    {
        /// <summary>
        /// 怪物Id
        /// </summary>
        public int monster_id;
        /// <summary>
        /// 怪物配置Id
        /// </summary>
        public int config_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(monster_id);
            bw.Write(config_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                monster_id = br.ReadInt32();
                config_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 新怪物出现通知
    /// </summary>
    public sealed class CLFRMonsterAppearNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 15;
        /// <summary>
        /// 怪物数组数量
        /// </summary>
        public int monster_count;
        /// <summary>
        /// 怪物信息数组
        /// </summary>
        public CLFRMonsterAppearInfo[] monsters;  //max:20
        /// <summary>
        /// 怪物信息数组（最大长度）
        /// </summary>
        public const int monsters_max_length = 20;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(monster_count);
            if (monster_count > monsters_max_length)
                throw new Exception($"CLFRMonsterAppearNtf.monsters数组长度超过规定限制，期望:20 实际:{monster_count}");
            for (int i = 0; i < (int)monster_count; i++)
                monsters[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                monster_count = br.ReadInt32();
                monsters = new CLFRMonsterAppearInfo[(int)monster_count];
                for (int i = 0; i < monsters.Length; i++)
                {
                    monsters[i] = new CLFRMonsterAppearInfo();
                    monsters[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 当前攻击对象通知
    /// </summary>
    public sealed class CLFRCurrentAttackObjNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 16;
        /// <summary>
        /// 攻击对象 0英雄攻击 1怪物攻击
        /// </summary>
        public int attack_obj;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(attack_obj);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                attack_obj = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 发炮请求
    /// </summary>
    public sealed class CLFRShootReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 17;
        /// <summary>
        /// 英雄下标 第几个英雄
        /// </summary>
        public int hero_index;
        /// <summary>
        /// 英雄Id
        /// </summary>
        public int hero_id;
        /// <summary>
        /// 发炮角度
        /// </summary>
        public int angle;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(hero_index);
            bw.Write(hero_id);
            bw.Write(angle);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                hero_index = br.ReadInt32();
                hero_id = br.ReadInt32();
                angle = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 发炮回应
    /// </summary>
    public sealed class CLFRShootAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 18;
        /// <summary>
        /// 0成功 1不是该英雄回合 2该回合已经攻击过 3不是战斗状态 4系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 英雄下标 第几个英雄
        /// </summary>
        public int hero_index;
        /// <summary>
        /// 英雄Id
        /// </summary>
        public int hero_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(hero_index);
            bw.Write(hero_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                hero_index = br.ReadInt32();
                hero_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 发炮通知
    /// </summary>
    public sealed class CLFRShootNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 19;
        /// <summary>
        /// 座位Id
        /// </summary>
        public int seat_id;
        /// <summary>
        /// 英雄下标 第几个英雄
        /// </summary>
        public int hero_index;
        /// <summary>
        /// 英雄Id
        /// </summary>
        public int hero_id;
        /// <summary>
        /// 发炮角度
        /// </summary>
        public int angle;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(seat_id);
            bw.Write(hero_index);
            bw.Write(hero_id);
            bw.Write(angle);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                seat_id = br.ReadInt32();
                hero_index = br.ReadInt32();
                hero_id = br.ReadInt32();
                angle = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 攻击信息
    /// </summary>
    public sealed class CLFRHitInfo : INetProtocol
    {
        /// <summary>
        /// 怪物Id
        /// </summary>
        public int monster_id;
        /// <summary>
        /// 攻击掉血量
        /// </summary>
        public Int64 attack_hp;
        /// <summary>
        /// 是否打爆 0否 1是 打爆需移除
        /// </summary>
        public sbyte is_boom;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(monster_id);
            bw.Write(attack_hp);
            bw.Write(is_boom);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                monster_id = br.ReadInt32();
                attack_hp = br.ReadInt64();
                is_boom = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 命中请求
    /// </summary>
    public sealed class CLFRHitReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 20;
        /// <summary>
        /// 英雄下标 第几个英雄
        /// </summary>
        public int hero_index;
        /// <summary>
        /// 英雄Id
        /// </summary>
        public int hero_id;
        /// <summary>
        /// 技能Id
        /// </summary>
        public int skill_id;
        /// <summary>
        /// 怪物数组长度
        /// </summary>
        public sbyte monster_len;
        /// <summary>
        /// 怪物Id数组
        /// </summary>
        public int[] monster_array;  //max:20
        /// <summary>
        /// 怪物Id数组（最大长度）
        /// </summary>
        public const int monster_array_max_length = 20;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(hero_index);
            bw.Write(hero_id);
            bw.Write(skill_id);
            bw.Write(monster_len);
            if (monster_len > monster_array_max_length)
                throw new Exception($"CLFRHitReq.monster_array数组长度超过规定限制，期望:20 实际:{monster_len}");
            for (int i = 0; i < (int)monster_len; i++)
                bw.Write(monster_array[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                hero_index = br.ReadInt32();
                hero_id = br.ReadInt32();
                skill_id = br.ReadInt32();
                monster_len = br.ReadSByte();
                monster_array = new int[(int)monster_len];
                for (int i = 0; i < monster_array.Length; i++)
                    monster_array[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 命中回应
    /// </summary>
    public sealed class CLFRHitAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 21;
        /// <summary>
        /// 0成功 1不是战斗状态 2系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 信息数量
        /// </summary>
        public int hit_len;
        /// <summary>
        /// 攻击信息数组
        /// </summary>
        public CLFRHitInfo[] hit_infos;  //max:20
        /// <summary>
        /// 攻击信息数组（最大长度）
        /// </summary>
        public const int hit_infos_max_length = 20;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(hit_len);
            if (hit_len > hit_infos_max_length)
                throw new Exception($"CLFRHitAck.hit_infos数组长度超过规定限制，期望:20 实际:{hit_len}");
            for (int i = 0; i < (int)hit_len; i++)
                hit_infos[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                hit_len = br.ReadInt32();
                hit_infos = new CLFRHitInfo[(int)hit_len];
                for (int i = 0; i < hit_infos.Length; i++)
                {
                    hit_infos[i] = new CLFRHitInfo();
                    hit_infos[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 英雄攻击结束请求
    /// </summary>
    public sealed class CLFRHitOverReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 22;
        /// <summary>
        /// 英雄下标 第几个英雄
        /// </summary>
        public int hero_index;
        /// <summary>
        /// 英雄Id
        /// </summary>
        public int hero_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(hero_index);
            bw.Write(hero_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                hero_index = br.ReadInt32();
                hero_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 英雄攻击结束回应
    /// </summary>
    public sealed class CLFRHitOverAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 23;
        /// <summary>
        /// 0成功 1不是战斗状态 2系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 英雄下标 第几个英雄
        /// </summary>
        public int hero_index;
        /// <summary>
        /// 英雄Id
        /// </summary>
        public int hero_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(hero_index);
            bw.Write(hero_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                hero_index = br.ReadInt32();
                hero_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 怪物命中请求
    /// </summary>
    public sealed class CLFRMonsterHitReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 24;
        /// <summary>
        /// 怪物Id
        /// </summary>
        public int monster_id;
        /// <summary>
        /// 技能Id
        /// </summary>
        public int skill_id;
        /// <summary>
        /// 英雄数组长度
        /// </summary>
        public sbyte hero_len;
        /// <summary>
        /// 英雄下标Id数组
        /// </summary>
        public int[] hero_array;  //max:4
        /// <summary>
        /// 英雄下标Id数组（最大长度）
        /// </summary>
        public const int hero_array_max_length = 4;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(monster_id);
            bw.Write(skill_id);
            bw.Write(hero_len);
            if (hero_len > hero_array_max_length)
                throw new Exception($"CLFRMonsterHitReq.hero_array数组长度超过规定限制，期望:4 实际:{hero_len}");
            for (int i = 0; i < (int)hero_len; i++)
                bw.Write(hero_array[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                monster_id = br.ReadInt32();
                skill_id = br.ReadInt32();
                hero_len = br.ReadSByte();
                hero_array = new int[(int)hero_len];
                for (int i = 0; i < hero_array.Length; i++)
                    hero_array[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 怪物命中回应
    /// </summary>
    public sealed class CLFRMonsterHitAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 25;
        /// <summary>
        /// 0成功 1不是战斗状态 2系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 英雄ID数组长度
        /// </summary>
        public int hero_id_len;
        /// <summary>
        /// 英雄下标Id数组
        /// </summary>
        public int[] hero_ids;  //max:4
        /// <summary>
        /// 英雄下标Id数组（最大长度）
        /// </summary>
        public const int hero_ids_max_length = 4;
        /// <summary>
        /// 攻击掉血量
        /// </summary>
        public Int64 attack_hp;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(hero_id_len);
            if (hero_id_len > hero_ids_max_length)
                throw new Exception($"CLFRMonsterHitAck.hero_ids数组长度超过规定限制，期望:4 实际:{hero_id_len}");
            for (int i = 0; i < (int)hero_id_len; i++)
                bw.Write(hero_ids[i]);
            bw.Write(attack_hp);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                hero_id_len = br.ReadInt32();
                hero_ids = new int[(int)hero_id_len];
                for (int i = 0; i < hero_ids.Length; i++)
                    hero_ids[i] = br.ReadInt32();
                attack_hp = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 怪物攻击结束请求
    /// </summary>
    public sealed class CLFRMonsterHitOverReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 26;

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
    /// 怪物攻击结束回应
    /// </summary>
    public sealed class CLFRMonsterHitOverAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 27;
        /// <summary>
        /// 0成功 1不是战斗状态 2系统错误
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
    /// 对战信息
    /// </summary>
    public sealed class CLFRBattleInfo : INetProtocol
    {
        /// <summary>
        /// 座位Id
        /// </summary>
        public int seat_id;
        /// <summary>
        /// 当前血量
        /// </summary>
        public Int64 current_hp;
        /// <summary>
        /// 目前怪物数
        /// </summary>
        public int monster_count;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(seat_id);
            bw.Write(current_hp);
            bw.Write(monster_count);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                seat_id = br.ReadInt32();
                current_hp = br.ReadInt64();
                monster_count = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 对战信息通知
    /// </summary>
    public sealed class CLFRBattleInfoNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 28;
        /// <summary>
        /// 第几回合
        /// </summary>
        public int battle_turn;
        /// <summary>
        /// 信息数量
        /// </summary>
        public int info_len;
        /// <summary>
        /// 对战信息数组
        /// </summary>
        public CLFRBattleInfo[] battle_infos;  //max:2
        /// <summary>
        /// 对战信息数组（最大长度）
        /// </summary>
        public const int battle_infos_max_length = 2;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(battle_turn);
            bw.Write(info_len);
            if (info_len > battle_infos_max_length)
                throw new Exception($"CLFRBattleInfoNtf.battle_infos数组长度超过规定限制，期望:2 实际:{info_len}");
            for (int i = 0; i < (int)info_len; i++)
                battle_infos[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                battle_turn = br.ReadInt32();
                info_len = br.ReadInt32();
                battle_infos = new CLFRBattleInfo[(int)info_len];
                for (int i = 0; i < battle_infos.Length; i++)
                {
                    battle_infos[i] = new CLFRBattleInfo();
                    battle_infos[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 物品信息结构
    /// </summary>
    public sealed class CLFRItemInfo : INetProtocol
    {
        /// <summary>
        /// 物品Id
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
    /// 对战结束通知
    /// </summary>
    public sealed class CLFRBattleOverNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 11;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 29;
        /// <summary>
        /// 胜利座位Id
        /// </summary>
        public int win_seat_id;
        /// <summary>
        /// 几回合胜利
        /// </summary>
        public int win_turn;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public CLFRItemInfo[] items;  //max:10
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 10;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(win_seat_id);
            bw.Write(win_turn);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLFRBattleOverNtf.items数组长度超过规定限制，期望:10 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                win_seat_id = br.ReadInt32();
                win_turn = br.ReadInt32();
                item_len = br.ReadInt32();
                items = new CLFRItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLFRItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }

    public class ClientFishingRoomResponserBase : INetResponser
    {
        public bool processPackage(BinaryReader br, INetReactor reactor, out INetProtocol responseProto)
        {
            responseProto = null;
            if (br.ReadUInt16() != 11)
                return false;

            switch(br.ReadUInt16())
            {
                case 0:
                    responseProto = new CLFREnterGameReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFREnterGameReq(responseProto as CLFREnterGameReq);
                    break;
                case 1:
                    responseProto = new CLFREnterGameAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFREnterGameAck(responseProto as CLFREnterGameAck);
                    break;
                case 2:
                    responseProto = new CLFRExitGameReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRExitGameReq(responseProto as CLFRExitGameReq);
                    break;
                case 3:
                    responseProto = new CLFRExitGameAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRExitGameAck(responseProto as CLFRExitGameAck);
                    break;
                case 4:
                    responseProto = new CLFRGetReadyReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRGetReadyReq(responseProto as CLFRGetReadyReq);
                    break;
                case 5:
                    responseProto = new CLFRGetReadyAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRGetReadyAck(responseProto as CLFRGetReadyAck);
                    break;
                case 6:
                    responseProto = new CLFRPlayerJoinNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRPlayerJoinNtf(responseProto as CLFRPlayerJoinNtf);
                    break;
                case 7:
                    responseProto = new CLFRPlayerLeaveNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRPlayerLeaveNtf(responseProto as CLFRPlayerLeaveNtf);
                    break;
                case 8:
                    responseProto = new CLFREnterGameWithPasswordReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFREnterGameWithPasswordReq(responseProto as CLFREnterGameWithPasswordReq);
                    break;
                case 9:
                    responseProto = new CLFREnterGameWithPasswordAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFREnterGameWithPasswordAck(responseProto as CLFREnterGameWithPasswordAck);
                    break;
                case 10:
                    responseProto = new CLFRSetRoomPasswordReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRSetRoomPasswordReq(responseProto as CLFRSetRoomPasswordReq);
                    break;
                case 11:
                    responseProto = new CLFRSetRoomPasswordAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRSetRoomPasswordAck(responseProto as CLFRSetRoomPasswordAck);
                    break;
                case 12:
                    responseProto = new CLFRRoomStatusNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRRoomStatusNtf(responseProto as CLFRRoomStatusNtf);
                    break;
                case 13:
                    responseProto = new CLFRHeroTeamReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRHeroTeamReq(responseProto as CLFRHeroTeamReq);
                    break;
                case 14:
                    responseProto = new CLFRHeroTeamAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRHeroTeamAck(responseProto as CLFRHeroTeamAck);
                    break;
                case 15:
                    responseProto = new CLFRMonsterAppearNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRMonsterAppearNtf(responseProto as CLFRMonsterAppearNtf);
                    break;
                case 16:
                    responseProto = new CLFRCurrentAttackObjNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRCurrentAttackObjNtf(responseProto as CLFRCurrentAttackObjNtf);
                    break;
                case 17:
                    responseProto = new CLFRShootReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRShootReq(responseProto as CLFRShootReq);
                    break;
                case 18:
                    responseProto = new CLFRShootAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRShootAck(responseProto as CLFRShootAck);
                    break;
                case 19:
                    responseProto = new CLFRShootNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRShootNtf(responseProto as CLFRShootNtf);
                    break;
                case 20:
                    responseProto = new CLFRHitReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRHitReq(responseProto as CLFRHitReq);
                    break;
                case 21:
                    responseProto = new CLFRHitAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRHitAck(responseProto as CLFRHitAck);
                    break;
                case 22:
                    responseProto = new CLFRHitOverReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRHitOverReq(responseProto as CLFRHitOverReq);
                    break;
                case 23:
                    responseProto = new CLFRHitOverAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRHitOverAck(responseProto as CLFRHitOverAck);
                    break;
                case 24:
                    responseProto = new CLFRMonsterHitReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRMonsterHitReq(responseProto as CLFRMonsterHitReq);
                    break;
                case 25:
                    responseProto = new CLFRMonsterHitAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRMonsterHitAck(responseProto as CLFRMonsterHitAck);
                    break;
                case 26:
                    responseProto = new CLFRMonsterHitOverReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRMonsterHitOverReq(responseProto as CLFRMonsterHitOverReq);
                    break;
                case 27:
                    responseProto = new CLFRMonsterHitOverAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRMonsterHitOverAck(responseProto as CLFRMonsterHitOverAck);
                    break;
                case 28:
                    responseProto = new CLFRBattleInfoNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRBattleInfoNtf(responseProto as CLFRBattleInfoNtf);
                    break;
                case 29:
                    responseProto = new CLFRBattleOverNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLFRBattleOverNtf(responseProto as CLFRBattleOverNtf);
                    break;
            }
            return responseProto != null;
        }

        public virtual void onRecv_CLFREnterGameReq(CLFREnterGameReq proto) { }
        public virtual void onRecv_CLFREnterGameAck(CLFREnterGameAck proto) { }
        public virtual void onRecv_CLFRExitGameReq(CLFRExitGameReq proto) { }
        public virtual void onRecv_CLFRExitGameAck(CLFRExitGameAck proto) { }
        public virtual void onRecv_CLFRGetReadyReq(CLFRGetReadyReq proto) { }
        public virtual void onRecv_CLFRGetReadyAck(CLFRGetReadyAck proto) { }
        public virtual void onRecv_CLFRPlayerJoinNtf(CLFRPlayerJoinNtf proto) { }
        public virtual void onRecv_CLFRPlayerLeaveNtf(CLFRPlayerLeaveNtf proto) { }
        public virtual void onRecv_CLFREnterGameWithPasswordReq(CLFREnterGameWithPasswordReq proto) { }
        public virtual void onRecv_CLFREnterGameWithPasswordAck(CLFREnterGameWithPasswordAck proto) { }
        public virtual void onRecv_CLFRSetRoomPasswordReq(CLFRSetRoomPasswordReq proto) { }
        public virtual void onRecv_CLFRSetRoomPasswordAck(CLFRSetRoomPasswordAck proto) { }
        public virtual void onRecv_CLFRRoomStatusNtf(CLFRRoomStatusNtf proto) { }
        public virtual void onRecv_CLFRHeroTeamReq(CLFRHeroTeamReq proto) { }
        public virtual void onRecv_CLFRHeroTeamAck(CLFRHeroTeamAck proto) { }
        public virtual void onRecv_CLFRMonsterAppearNtf(CLFRMonsterAppearNtf proto) { }
        public virtual void onRecv_CLFRCurrentAttackObjNtf(CLFRCurrentAttackObjNtf proto) { }
        public virtual void onRecv_CLFRShootReq(CLFRShootReq proto) { }
        public virtual void onRecv_CLFRShootAck(CLFRShootAck proto) { }
        public virtual void onRecv_CLFRShootNtf(CLFRShootNtf proto) { }
        public virtual void onRecv_CLFRHitReq(CLFRHitReq proto) { }
        public virtual void onRecv_CLFRHitAck(CLFRHitAck proto) { }
        public virtual void onRecv_CLFRHitOverReq(CLFRHitOverReq proto) { }
        public virtual void onRecv_CLFRHitOverAck(CLFRHitOverAck proto) { }
        public virtual void onRecv_CLFRMonsterHitReq(CLFRMonsterHitReq proto) { }
        public virtual void onRecv_CLFRMonsterHitAck(CLFRMonsterHitAck proto) { }
        public virtual void onRecv_CLFRMonsterHitOverReq(CLFRMonsterHitOverReq proto) { }
        public virtual void onRecv_CLFRMonsterHitOverAck(CLFRMonsterHitOverAck proto) { }
        public virtual void onRecv_CLFRBattleInfoNtf(CLFRBattleInfoNtf proto) { }
        public virtual void onRecv_CLFRBattleOverNtf(CLFRBattleOverNtf proto) { }
    }
}
