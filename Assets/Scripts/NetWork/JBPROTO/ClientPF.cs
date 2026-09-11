using System;
using System.IO;

namespace JBPROTO
{
    /// <summary>
    /// 登出请求
    /// </summary>
    public sealed class CLPFLogoutReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
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
    /// 英雄信息
    /// </summary>
    public sealed class CLPFHeroInfo : INetProtocol
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
        /// <summary>
        /// 战力值
        /// </summary>
        public Int64 fight;

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
            bw.Write(fight);
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
                fight = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 英雄组信息
    /// </summary>
    public sealed class CLPFHeroTeamInfo : INetProtocol
    {
        /// <summary>
        /// 组ID
        /// </summary>
        public int team_id;
        /// <summary>
        /// 英雄1
        /// </summary>
        public int hero_id_1;
        /// <summary>
        /// 英雄2
        /// </summary>
        public int hero_id_2;
        /// <summary>
        /// 英雄3
        /// </summary>
        public int hero_id_3;
        /// <summary>
        /// 英雄4
        /// </summary>
        public int hero_id_4;
        /// <summary>
        /// 战力值
        /// </summary>
        public Int64 fight;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(team_id);
            bw.Write(hero_id_1);
            bw.Write(hero_id_2);
            bw.Write(hero_id_3);
            bw.Write(hero_id_4);
            bw.Write(fight);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                team_id = br.ReadInt32();
                hero_id_1 = br.ReadInt32();
                hero_id_2 = br.ReadInt32();
                hero_id_3 = br.ReadInt32();
                hero_id_4 = br.ReadInt32();
                fight = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取英雄请求
    /// </summary>
    public sealed class CLPFGetHeroReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 1;

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
    /// 获取英雄回应
    /// </summary>
    public sealed class CLPFGetHeroAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 2;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int hero_len;
        /// <summary>
        /// 英雄数组
        /// </summary>
        public CLPFHeroInfo[] heros;  //max:100
        /// <summary>
        /// 英雄数组（最大长度）
        /// </summary>
        public const int heros_max_length = 100;
        /// <summary>
        /// 英雄组数组长度
        /// </summary>
        public int hero_team_len;
        /// <summary>
        /// 英雄组数组
        /// </summary>
        public CLPFHeroTeamInfo[] hero_teams;  //max:10
        /// <summary>
        /// 英雄组数组（最大长度）
        /// </summary>
        public const int hero_teams_max_length = 10;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(hero_len);
            if (hero_len > heros_max_length)
                throw new Exception($"CLPFGetHeroAck.heros数组长度超过规定限制，期望:100 实际:{hero_len}");
            for (int i = 0; i < (int)hero_len; i++)
                heros[i].toBinary(bw);
            bw.Write(hero_team_len);
            if (hero_team_len > hero_teams_max_length)
                throw new Exception($"CLPFGetHeroAck.hero_teams数组长度超过规定限制，期望:10 实际:{hero_team_len}");
            for (int i = 0; i < (int)hero_team_len; i++)
                hero_teams[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                hero_len = br.ReadInt32();
                heros = new CLPFHeroInfo[(int)hero_len];
                for (int i = 0; i < heros.Length; i++)
                {
                    heros[i] = new CLPFHeroInfo();
                    heros[i].fromBinary(br);
                }
                hero_team_len = br.ReadInt32();
                hero_teams = new CLPFHeroTeamInfo[(int)hero_team_len];
                for (int i = 0; i < hero_teams.Length; i++)
                {
                    hero_teams[i] = new CLPFHeroTeamInfo();
                    hero_teams[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取称号请求
    /// </summary>
    public sealed class CLPFGetTitleListReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 3;

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
    /// 获取称号回应
    /// </summary>
    public sealed class CLPFGetTitleListAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 4;
        /// <summary>
        /// 0成功 2系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 称号ID数组长度
        /// </summary>
        public int title_id_len;
        /// <summary>
        /// 称号的Id数组
        /// </summary>
        public int[] title_ids;  //max:100
        /// <summary>
        /// 称号的Id数组（最大长度）
        /// </summary>
        public const int title_ids_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(title_id_len);
            if (title_id_len > title_ids_max_length)
                throw new Exception($"CLPFGetTitleListAck.title_ids数组长度超过规定限制，期望:100 实际:{title_id_len}");
            for (int i = 0; i < (int)title_id_len; i++)
                bw.Write(title_ids[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                title_id_len = br.ReadInt32();
                title_ids = new int[(int)title_id_len];
                for (int i = 0; i < title_ids.Length; i++)
                    title_ids[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 称号操作请求
    /// </summary>
    public sealed class CLPFTitleOperateReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 5;
        /// <summary>
        /// 称号Id
        /// </summary>
        public int title_id;
        /// <summary>
        /// 操作类型 1装备 2卸下
        /// </summary>
        public int opt_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(title_id);
            bw.Write(opt_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                title_id = br.ReadInt32();
                opt_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 称号操作回应
    /// </summary>
    public sealed class CLPFTitleOperateAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 6;
        /// <summary>
        /// 0成功 1未获得此称号 2称号已装备 3称号已卸下 4系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 操作类型
        /// </summary>
        public int opt_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(opt_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                opt_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取铭牌请求
    /// </summary>
    public sealed class CLPFGetNameplateListReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 7;

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
    /// 获取铭牌回应
    /// </summary>
    public sealed class CLPFGetNameplateListAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 8;
        /// <summary>
        /// 0成功 2系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 铭牌ID数组长度
        /// </summary>
        public int nameplate_id_len;
        /// <summary>
        /// 铭牌的Id数组
        /// </summary>
        public int[] nameplate_ids;  //max:100
        /// <summary>
        /// 铭牌的Id数组（最大长度）
        /// </summary>
        public const int nameplate_ids_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(nameplate_id_len);
            if (nameplate_id_len > nameplate_ids_max_length)
                throw new Exception($"CLPFGetNameplateListAck.nameplate_ids数组长度超过规定限制，期望:100 实际:{nameplate_id_len}");
            for (int i = 0; i < (int)nameplate_id_len; i++)
                bw.Write(nameplate_ids[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                nameplate_id_len = br.ReadInt32();
                nameplate_ids = new int[(int)nameplate_id_len];
                for (int i = 0; i < nameplate_ids.Length; i++)
                    nameplate_ids[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 铭牌操作请求
    /// </summary>
    public sealed class CLPFNameplateOperateReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 9;
        /// <summary>
        /// 铭牌Id
        /// </summary>
        public int nameplate_id;
        /// <summary>
        /// 操作类型 1装备
        /// </summary>
        public int opt_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(nameplate_id);
            bw.Write(opt_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                nameplate_id = br.ReadInt32();
                opt_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 铭牌操作回应
    /// </summary>
    public sealed class CLPFNameplateOperateAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 10;
        /// <summary>
        /// 0成功 1未获得此铭牌 2铭牌已装备 3系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 操作类型
        /// </summary>
        public int opt_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(opt_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                opt_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取徽章请求
    /// </summary>
    public sealed class CLPFGetBadgeListReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 11;

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
    /// 获取徽章回应
    /// </summary>
    public sealed class CLPFGetBadgeListAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 12;
        /// <summary>
        /// 0成功 2系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 徽章ID数组长度
        /// </summary>
        public int badge_id_len;
        /// <summary>
        /// 徽章的Id数组
        /// </summary>
        public int[] badge_ids;  //max:100
        /// <summary>
        /// 徽章的Id数组（最大长度）
        /// </summary>
        public const int badge_ids_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(badge_id_len);
            if (badge_id_len > badge_ids_max_length)
                throw new Exception($"CLPFGetBadgeListAck.badge_ids数组长度超过规定限制，期望:100 实际:{badge_id_len}");
            for (int i = 0; i < (int)badge_id_len; i++)
                bw.Write(badge_ids[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                badge_id_len = br.ReadInt32();
                badge_ids = new int[(int)badge_id_len];
                for (int i = 0; i < badge_ids.Length; i++)
                    badge_ids[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 徽章操作请求
    /// </summary>
    public sealed class CLPFBadgeOperateReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 13;
        /// <summary>
        /// 徽章Id
        /// </summary>
        public int badge_id;
        /// <summary>
        /// 操作类型 1装备 2卸下
        /// </summary>
        public int opt_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(badge_id);
            bw.Write(opt_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                badge_id = br.ReadInt32();
                opt_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 徽章操作回应
    /// </summary>
    public sealed class CLPFBadgeOperateAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 14;
        /// <summary>
        /// 0成功 1未获得此徽章 2徽章已装备 3徽章已卸下 4系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 操作类型
        /// </summary>
        public int opt_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(opt_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                opt_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取头像请求
    /// </summary>
    public sealed class CLPFGetHeadListReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 15;

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
    /// 获取头像回应
    /// </summary>
    public sealed class CLPFGetHeadListAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 16;
        /// <summary>
        /// 0成功 2系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 头像ID数组长度
        /// </summary>
        public int head_id_len;
        /// <summary>
        /// 头像的Id数组
        /// </summary>
        public int[] head_ids;  //max:100
        /// <summary>
        /// 头像的Id数组（最大长度）
        /// </summary>
        public const int head_ids_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(head_id_len);
            if (head_id_len > head_ids_max_length)
                throw new Exception($"CLPFGetHeadListAck.head_ids数组长度超过规定限制，期望:100 实际:{head_id_len}");
            for (int i = 0; i < (int)head_id_len; i++)
                bw.Write(head_ids[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                head_id_len = br.ReadInt32();
                head_ids = new int[(int)head_id_len];
                for (int i = 0; i < head_ids.Length; i++)
                    head_ids[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 头像操作请求
    /// </summary>
    public sealed class CLPFHeadOperateReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 17;
        /// <summary>
        /// 头像Id
        /// </summary>
        public int head_id;
        /// <summary>
        /// 操作类型 1装备
        /// </summary>
        public int opt_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(head_id);
            bw.Write(opt_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                head_id = br.ReadInt32();
                opt_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 头像操作回应
    /// </summary>
    public sealed class CLPFHeadOperateAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 18;
        /// <summary>
        /// 0成功 1未获得此头像 2头像已装备 3系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 操作类型
        /// </summary>
        public int opt_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(opt_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                opt_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取头像框请求
    /// </summary>
    public sealed class CLPFGetHeadFrameListReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 19;

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
    /// 获取头像框回应
    /// </summary>
    public sealed class CLPFGetHeadFrameListAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 20;
        /// <summary>
        /// 0成功 2系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 头像框ID数组长度
        /// </summary>
        public int head_frame_id_len;
        /// <summary>
        /// 头像框的Id数组
        /// </summary>
        public int[] head_frame_ids;  //max:100
        /// <summary>
        /// 头像框的Id数组（最大长度）
        /// </summary>
        public const int head_frame_ids_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(head_frame_id_len);
            if (head_frame_id_len > head_frame_ids_max_length)
                throw new Exception($"CLPFGetHeadFrameListAck.head_frame_ids数组长度超过规定限制，期望:100 实际:{head_frame_id_len}");
            for (int i = 0; i < (int)head_frame_id_len; i++)
                bw.Write(head_frame_ids[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                head_frame_id_len = br.ReadInt32();
                head_frame_ids = new int[(int)head_frame_id_len];
                for (int i = 0; i < head_frame_ids.Length; i++)
                    head_frame_ids[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 头像框操作请求
    /// </summary>
    public sealed class CLPFHeadFrameOperateReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 21;
        /// <summary>
        /// 头像框Id
        /// </summary>
        public int head_frame_id;
        /// <summary>
        /// 操作类型 1装备 2卸下
        /// </summary>
        public int opt_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(head_frame_id);
            bw.Write(opt_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                head_frame_id = br.ReadInt32();
                opt_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 头像框操作回应
    /// </summary>
    public sealed class CLPFHeadFrameOperateAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 22;
        /// <summary>
        /// 0成功 1未获得此头像框 2头像框已装备 3头像框已卸下 4系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 操作类型
        /// </summary>
        public int opt_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(opt_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                opt_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 修改昵称请求
    /// </summary>
    public sealed class CLPFModifyNicknameReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 23;
        /// <summary>
        /// 新的昵称
        /// </summary>
        public string new_nickname;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            NetHelper.SafeWriteString(bw, new_nickname, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                new_nickname = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 修改昵称回应
    /// </summary>
    public sealed class CLPFModifyNicknameAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 24;
        /// <summary>
        /// 0成功 1格式不合法 2包含敏感字符 3昵称已存在 4钻石不足
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
    public sealed class CLPFResSyncNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 25;
        /// <summary>
        /// 钻石数量
        /// </summary>
        public Int64 diamond;
        /// <summary>
        /// 金币数量
        /// </summary>
        public Int64 currency;
        /// <summary>
        /// 积分数量
        /// </summary>
        public Int64 integral;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(diamond);
            bw.Write(currency);
            bw.Write(integral);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                diamond = br.ReadInt64();
                currency = br.ReadInt64();
                integral = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 资源变化通知
    /// </summary>
    public sealed class CLPFResChangedNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 26;
        /// <summary>
        /// 资源类型 1钻石 2金币 3绑定金币 4积分 5魔力值 6战力值
        /// </summary>
        public sbyte res_type;
        /// <summary>
        /// 资源值
        /// </summary>
        public Int64 res_value;
        /// <summary>
        /// 资源变化量
        /// </summary>
        public Int64 res_delta;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(res_type);
            bw.Write(res_value);
            bw.Write(res_delta);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                res_type = br.ReadSByte();
                res_value = br.ReadInt64();
                res_delta = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 物品信息结构
    /// </summary>
    public sealed class CLPFItemInfo : INetProtocol
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
    /// 获取物品列表请求
    /// </summary>
    public sealed class CLPFItemGetListReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 27;

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
    /// 获取物品列表回应
    /// </summary>
    public sealed class CLPFItemGetListAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 28;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFItemGetListAck.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 使用物品请求
    /// </summary>
    public sealed class CLPFItemUseReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 29;
        /// <summary>
        /// 物品信息
        /// </summary>
        public CLPFItemInfo item;
        /// <summary>
        /// 游戏Id
        /// </summary>
        public int group_id;
        /// <summary>
        /// 玩法Id
        /// </summary>
        public int service_id;
        /// <summary>
        /// 自选礼包的Id数组长度
        /// </summary>
        public int select_id_len;
        /// <summary>
        /// 自选礼包的Id数组
        /// </summary>
        public int[] select_ids;  //max:100
        /// <summary>
        /// 自选礼包的Id数组（最大长度）
        /// </summary>
        public const int select_ids_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            item.toBinary(bw);
            bw.Write(group_id);
            bw.Write(service_id);
            bw.Write(select_id_len);
            if (select_id_len > select_ids_max_length)
                throw new Exception($"CLPFItemUseReq.select_ids数组长度超过规定限制，期望:100 实际:{select_id_len}");
            for (int i = 0; i < (int)select_id_len; i++)
                bw.Write(select_ids[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                item = new CLPFItemInfo();
                item.fromBinary(br);
                group_id = br.ReadInt32();
                service_id = br.ReadInt32();
                select_id_len = br.ReadInt32();
                select_ids = new int[(int)select_id_len];
                for (int i = 0; i < select_ids.Length; i++)
                    select_ids[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 使用物品回应
    /// </summary>
    public sealed class CLPFItemUseAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 30;
        /// <summary>
        /// 0成功 1数量不足 2配置表错误 3使用失败 4鱼潮即将来临禁止使用 5狂暴下不能使用瞄准 6分身下不能使用瞄准 7vip等级不足
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 获得物品数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 获得物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 获得物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFItemUseAck.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 物品数量变化通知
    /// </summary>
    public sealed class CLPFItemCountChangeNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 31;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFItemCountChangeNtf.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 物品购买请求
    /// </summary>
    public sealed class CLPFItemBuyReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 32;
        /// <summary>
        /// 购买的物品
        /// </summary>
        public CLPFItemInfo item;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            item.toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                item = new CLPFItemInfo();
                item.fromBinary(br);
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 物品购买回应
    /// </summary>
    public sealed class CLPFItemBuyAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 33;
        /// <summary>
        /// 0购买成功 1购买数量非法 2物品不存在 3该道具不卖 4资源不足 5vip等级不足
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 获得的物品，仅用于显示
        /// </summary>
        public CLPFItemInfo item;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            item.toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                item = new CLPFItemInfo();
                item.fromBinary(br);
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 商城购买次数信息
    /// </summary>
    public sealed class CLPFShopBuyCountItem : INetProtocol
    {
        /// <summary>
        /// 商城购买项Id
        /// </summary>
        public int shop_id;
        /// <summary>
        /// 购买次数
        /// </summary>
        public int buy_count;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(shop_id);
            bw.Write(buy_count);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                shop_id = br.ReadInt32();
                buy_count = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 商城购买次数请求
    /// </summary>
    public sealed class CLPFShopQueryBuyCountReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 34;

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
    /// 商城购买次数回应
    /// </summary>
    public sealed class CLPFShopQueryBuyCountAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 35;
        /// <summary>
        /// 总购买次数数组长度
        /// </summary>
        public int total_item_len;
        /// <summary>
        /// 总购买次数数组
        /// </summary>
        public CLPFShopBuyCountItem[] total_item_array;  //max:100
        /// <summary>
        /// 总购买次数数组（最大长度）
        /// </summary>
        public const int total_item_array_max_length = 100;
        /// <summary>
        /// 当天购买次数数组长度
        /// </summary>
        public int today_item_len;
        /// <summary>
        /// 当天购买次数数组
        /// </summary>
        public CLPFShopBuyCountItem[] today_item_array;  //max:100
        /// <summary>
        /// 当天购买次数数组（最大长度）
        /// </summary>
        public const int today_item_array_max_length = 100;
        /// <summary>
        /// 当天兑换实物商品数组长度
        /// </summary>
        public int today_real_goods_len;
        /// <summary>
        /// 当天兑换实物商品数组
        /// </summary>
        public CLPFShopBuyCountItem[] today_real_goods_array;  //max:100
        /// <summary>
        /// 当天兑换实物商品数组（最大长度）
        /// </summary>
        public const int today_real_goods_array_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(total_item_len);
            if (total_item_len > total_item_array_max_length)
                throw new Exception($"CLPFShopQueryBuyCountAck.total_item_array数组长度超过规定限制，期望:100 实际:{total_item_len}");
            for (int i = 0; i < (int)total_item_len; i++)
                total_item_array[i].toBinary(bw);
            bw.Write(today_item_len);
            if (today_item_len > today_item_array_max_length)
                throw new Exception($"CLPFShopQueryBuyCountAck.today_item_array数组长度超过规定限制，期望:100 实际:{today_item_len}");
            for (int i = 0; i < (int)today_item_len; i++)
                today_item_array[i].toBinary(bw);
            bw.Write(today_real_goods_len);
            if (today_real_goods_len > today_real_goods_array_max_length)
                throw new Exception($"CLPFShopQueryBuyCountAck.today_real_goods_array数组长度超过规定限制，期望:100 实际:{today_real_goods_len}");
            for (int i = 0; i < (int)today_real_goods_len; i++)
                today_real_goods_array[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                total_item_len = br.ReadInt32();
                total_item_array = new CLPFShopBuyCountItem[(int)total_item_len];
                for (int i = 0; i < total_item_array.Length; i++)
                {
                    total_item_array[i] = new CLPFShopBuyCountItem();
                    total_item_array[i].fromBinary(br);
                }
                today_item_len = br.ReadInt32();
                today_item_array = new CLPFShopBuyCountItem[(int)today_item_len];
                for (int i = 0; i < today_item_array.Length; i++)
                {
                    today_item_array[i] = new CLPFShopBuyCountItem();
                    today_item_array[i].fromBinary(br);
                }
                today_real_goods_len = br.ReadInt32();
                today_real_goods_array = new CLPFShopBuyCountItem[(int)today_real_goods_len];
                for (int i = 0; i < today_real_goods_array.Length; i++)
                {
                    today_real_goods_array[i] = new CLPFShopBuyCountItem();
                    today_real_goods_array[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 商城购买请求
    /// </summary>
    public sealed class CLPFShopBuyReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 36;
        /// <summary>
        /// 商城购买项Id
        /// </summary>
        public int shop_id;
        /// <summary>
        /// 购买数量
        /// </summary>
        public int buy_count;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(shop_id);
            bw.Write(buy_count);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                shop_id = br.ReadInt32();
                buy_count = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 商城购买回应
    /// </summary>
    public sealed class CLPFShopBuyAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 37;
        /// <summary>
        /// 0成功 1无此购买项 2资源不足 3购买次数已达上限 4vip等级不足 5系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFShopBuyAck.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 通用充值请求
    /// </summary>
    public sealed class CLPFRechargeReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 38;
        /// <summary>
        /// 购买内容类型 1商城充值 2购买月卡 3首充礼包 4每日充值 5投资炮倍 6出海保险 7持续奖励礼包 8充值升级炮倍
        /// </summary>
        public int content_type;
        /// <summary>
        /// 购买内容Id
        /// </summary>
        public int content_id;
        /// <summary>
        /// 支付渠道 1微信支付 2支付宝 3支付猫微信 4支付猫支付宝 5聚合微信 6聚合支付宝 7线下支付 85秒支付微信 95秒支付支付宝 10和融通微信 11和融通支付宝 12联合支付微信 13联合支付宝 14使用道具充值
        /// </summary>
        public int pay_mode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(content_type);
            bw.Write(content_id);
            bw.Write(pay_mode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                content_type = br.ReadInt32();
                content_id = br.ReadInt32();
                pay_mode = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 通用充值回应
    /// </summary>
    public sealed class CLPFRechargeAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 39;
        /// <summary>
        /// 0成功 1无此购买项 2不支持的支付渠道 3购买次数已达上限 4vip等级不足 5系统错误 6资源不足
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 支付环境，json格式字符串
        /// </summary>
        public string pay_envir;  //max:4096

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            NetHelper.SafeWriteString(bw, pay_envir, 4096);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                pay_envir = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 充值到账通知
    /// </summary>
    public sealed class CLPFRechargeSuccessNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 40;
        /// <summary>
        /// 购买内容类型 1商城充值 2购买月卡 3首充礼包 4每日充值 5投资炮倍 6出海保险 7持续奖励礼包 8充值升级炮倍
        /// </summary>
        public int content_type;
        /// <summary>
        /// 购买内容Id
        /// </summary>
        public int content_id;
        /// <summary>
        /// 获得物品数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 获得物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 获得物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(content_type);
            bw.Write(content_id);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFRechargeSuccessNtf.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                content_type = br.ReadInt32();
                content_id = br.ReadInt32();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 排行榜玩家信息
    /// </summary>
    public sealed class CLPFRankPlayerInfo : INetProtocol
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
        /// 玩家等级
        /// </summary>
        public int level;
        /// <summary>
        /// 玩家vip等级
        /// </summary>
        public int vip_level;
        /// <summary>
        /// 平台货币
        /// </summary>
        public Int64 rank_value;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(user_id);
            NetHelper.SafeWriteString(bw, nickname, 32);
            bw.Write(gender);
            bw.Write(head);
            bw.Write(head_frame);
            bw.Write(level);
            bw.Write(vip_level);
            bw.Write(rank_value);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                nickname = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                gender = br.ReadInt32();
                head = br.ReadInt32();
                head_frame = br.ReadInt32();
                level = br.ReadInt32();
                vip_level = br.ReadInt32();
                rank_value = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取排行榜请求
    /// </summary>
    public sealed class CLPFGetRankListReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 41;
        /// <summary>
        /// 排行榜类型 1金币榜 2弹头榜 3自定义排行榜
        /// </summary>
        public int rank_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(rank_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                rank_type = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取排行榜回应
    /// </summary>
    public sealed class CLPFGetRankListAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 42;
        /// <summary>
        /// 0成功 1系统错误 2暂未开放
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int rank_len;
        /// <summary>
        /// 排行榜数据数组
        /// </summary>
        public CLPFRankPlayerInfo[] rank_rows;  //max:100
        /// <summary>
        /// 排行榜数据数组（最大长度）
        /// </summary>
        public const int rank_rows_max_length = 100;
        /// <summary>
        /// 从0开始，0代表第一名，-1代表未上榜
        /// </summary>
        public int my_lastday_rank;
        /// <summary>
        /// 是否已领取了昨日排行榜奖励 1是0否
        /// </summary>
        public sbyte fetched_lastday_reward;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(rank_len);
            if (rank_len > rank_rows_max_length)
                throw new Exception($"CLPFGetRankListAck.rank_rows数组长度超过规定限制，期望:100 实际:{rank_len}");
            for (int i = 0; i < (int)rank_len; i++)
                rank_rows[i].toBinary(bw);
            bw.Write(my_lastday_rank);
            bw.Write(fetched_lastday_reward);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                rank_len = br.ReadInt32();
                rank_rows = new CLPFRankPlayerInfo[(int)rank_len];
                for (int i = 0; i < rank_rows.Length; i++)
                {
                    rank_rows[i] = new CLPFRankPlayerInfo();
                    rank_rows[i].fromBinary(br);
                }
                my_lastday_rank = br.ReadInt32();
                fetched_lastday_reward = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 邮件信息结构
    /// </summary>
    public sealed class CLPFMailInfo : INetProtocol
    {
        /// <summary>
        /// 邮件唯一Id
        /// </summary>
        public int id;
        /// <summary>
        /// 邮件类型 1系统邮件 2好友赠送邮件
        /// </summary>
        public sbyte type;
        /// <summary>
        /// 邮件标题
        /// </summary>
        public string title;  //max:256
        /// <summary>
        /// 邮件内容
        /// </summary>
        public string content;  //max:512
        /// <summary>
        /// 附件物品长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 附件物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:10
        /// <summary>
        /// 附件物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 10;
        /// <summary>
        /// 邮件状态 1未读 2已读 3已领取
        /// </summary>
        public sbyte state;
        /// <summary>
        /// 邮件接收时间戳
        /// </summary>
        public UInt32 receive_time;
        /// <summary>
        /// 邮件过期时间戳
        /// </summary>
        public UInt32 expire_time;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(id);
            bw.Write(type);
            NetHelper.SafeWriteString(bw, title, 256);
            NetHelper.SafeWriteString(bw, content, 512);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFMailInfo.items数组长度超过规定限制，期望:10 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
            bw.Write(state);
            bw.Write(receive_time);
            bw.Write(expire_time);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                id = br.ReadInt32();
                type = br.ReadSByte();
                title = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                content = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
                state = br.ReadSByte();
                receive_time = br.ReadUInt32();
                expire_time = br.ReadUInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 所有邮件Id请求
    /// </summary>
    public sealed class CLPFMailQueryAllIdsReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 43;

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
    /// 所有邮件Id回应
    /// </summary>
    public sealed class CLPFMailQueryAllIdsAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 44;
        /// <summary>
        /// Id数组长度
        /// </summary>
        public int len;
        /// <summary>
        /// 邮件Id数组
        /// </summary>
        public int[] array;  //max:1000
        /// <summary>
        /// 邮件Id数组（最大长度）
        /// </summary>
        public const int array_max_length = 1000;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(len);
            if (len > array_max_length)
                throw new Exception($"CLPFMailQueryAllIdsAck.array数组长度超过规定限制，期望:1000 实际:{len}");
            for (int i = 0; i < (int)len; i++)
                bw.Write(array[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                len = br.ReadInt32();
                array = new int[(int)len];
                for (int i = 0; i < array.Length; i++)
                    array[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 批量邮件内容请求
    /// </summary>
    public sealed class CLPFMailBatchQueryContentReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 45;
        /// <summary>
        /// Id数组长度
        /// </summary>
        public int len;
        /// <summary>
        /// 邮件Id数组长度
        /// </summary>
        public int[] array;  //max:100
        /// <summary>
        /// 邮件Id数组长度（最大长度）
        /// </summary>
        public const int array_max_length = 100;
        /// <summary>
        /// 期望显示哪种语言？'CN'为中文
        /// </summary>
        public string language;  //max:16

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(len);
            if (len > array_max_length)
                throw new Exception($"CLPFMailBatchQueryContentReq.array数组长度超过规定限制，期望:100 实际:{len}");
            for (int i = 0; i < (int)len; i++)
                bw.Write(array[i]);
            NetHelper.SafeWriteString(bw, language, 16);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                len = br.ReadInt32();
                array = new int[(int)len];
                for (int i = 0; i < array.Length; i++)
                    array[i] = br.ReadInt32();
                language = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 批量邮件内容回应
    /// </summary>
    public sealed class CLPFMailBatchQueryContentAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 46;
        /// <summary>
        /// 无效Id数组长度
        /// </summary>
        public int invalid_len;
        /// <summary>
        /// 无效Id数组
        /// </summary>
        public int[] invalid_array;  //max:100
        /// <summary>
        /// 无效Id数组（最大长度）
        /// </summary>
        public const int invalid_array_max_length = 100;
        /// <summary>
        /// 结果数组长度
        /// </summary>
        public int result_len;
        /// <summary>
        /// 结果邮件数组
        /// </summary>
        public CLPFMailInfo[] result_array;  //max:100
        /// <summary>
        /// 结果邮件数组（最大长度）
        /// </summary>
        public const int result_array_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(invalid_len);
            if (invalid_len > invalid_array_max_length)
                throw new Exception($"CLPFMailBatchQueryContentAck.invalid_array数组长度超过规定限制，期望:100 实际:{invalid_len}");
            for (int i = 0; i < (int)invalid_len; i++)
                bw.Write(invalid_array[i]);
            bw.Write(result_len);
            if (result_len > result_array_max_length)
                throw new Exception($"CLPFMailBatchQueryContentAck.result_array数组长度超过规定限制，期望:100 实际:{result_len}");
            for (int i = 0; i < (int)result_len; i++)
                result_array[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                invalid_len = br.ReadInt32();
                invalid_array = new int[(int)invalid_len];
                for (int i = 0; i < invalid_array.Length; i++)
                    invalid_array[i] = br.ReadInt32();
                result_len = br.ReadInt32();
                result_array = new CLPFMailInfo[(int)result_len];
                for (int i = 0; i < result_array.Length; i++)
                {
                    result_array[i] = new CLPFMailInfo();
                    result_array[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查看邮件请求
    /// </summary>
    public sealed class CLPFMailAccessReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 47;
        /// <summary>
        /// 邮件唯一Id
        /// </summary>
        public int mail_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(mail_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                mail_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查看邮件回应
    /// </summary>
    public sealed class CLPFMailAccessAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 48;
        /// <summary>
        /// 是否还有未读邮件 1是0否
        /// </summary>
        public sbyte has_unread_mail;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(has_unread_mail);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                has_unread_mail = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 领取邮件物品请求
    /// </summary>
    public sealed class CLPFMailFetchItemReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 49;
        /// <summary>
        /// 邮件唯一Id
        /// </summary>
        public int mail_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(mail_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                mail_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 领取邮件物品回应
    /// </summary>
    public sealed class CLPFMailFetchItemAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 50;
        /// <summary>
        /// 0成功 1邮件不存在 2邮件已被领取过
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 获得的物品数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 获得的物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:10
        /// <summary>
        /// 获得的物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 10;
        /// <summary>
        /// 是否还有未读邮件 1是0否
        /// </summary>
        public sbyte has_unread_mail;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFMailFetchItemAck.items数组长度超过规定限制，期望:10 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
            bw.Write(has_unread_mail);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
                has_unread_mail = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 删除邮件请求
    /// </summary>
    public sealed class CLPFMailRemoveReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 51;
        /// <summary>
        /// 删除类型 1删除指定邮件 2删除已读且无可领取附件的邮件 3清空所有邮件
        /// </summary>
        public sbyte remove_type;
        /// <summary>
        /// 删除邮件数组长度
        /// </summary>
        public int remove_len;
        /// <summary>
        /// 需要删除的邮件Id数组
        /// </summary>
        public int[] remove_ids;  //max:1000
        /// <summary>
        /// 需要删除的邮件Id数组（最大长度）
        /// </summary>
        public const int remove_ids_max_length = 1000;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(remove_type);
            bw.Write(remove_len);
            if (remove_len > remove_ids_max_length)
                throw new Exception($"CLPFMailRemoveReq.remove_ids数组长度超过规定限制，期望:1000 实际:{remove_len}");
            for (int i = 0; i < (int)remove_len; i++)
                bw.Write(remove_ids[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                remove_type = br.ReadSByte();
                remove_len = br.ReadInt32();
                remove_ids = new int[(int)remove_len];
                for (int i = 0; i < remove_ids.Length; i++)
                    remove_ids[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 删除邮件回应
    /// </summary>
    public sealed class CLPFMailRemoveAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 52;
        /// <summary>
        /// 0成功 1包含错误的邮件Id
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 是否还有未读邮件 1是0否
        /// </summary>
        public sbyte has_unread_mail;
        /// <summary>
        /// 已删除的邮件数组长度
        /// </summary>
        public int removed_len;
        /// <summary>
        /// 已删除的邮件Id数组
        /// </summary>
        public int[] removed_ids;  //max:1000
        /// <summary>
        /// 已删除的邮件Id数组（最大长度）
        /// </summary>
        public const int removed_ids_max_length = 1000;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(has_unread_mail);
            bw.Write(removed_len);
            if (removed_len > removed_ids_max_length)
                throw new Exception($"CLPFMailRemoveAck.removed_ids数组长度超过规定限制，期望:1000 实际:{removed_len}");
            for (int i = 0; i < (int)removed_len; i++)
                bw.Write(removed_ids[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                has_unread_mail = br.ReadSByte();
                removed_len = br.ReadInt32();
                removed_ids = new int[(int)removed_len];
                for (int i = 0; i < removed_ids.Length; i++)
                    removed_ids[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 邮件到来通知
    /// </summary>
    public sealed class CLPFMailArriveNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 53;
        /// <summary>
        /// 邮件信息
        /// </summary>
        public CLPFMailInfo mail_info;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            mail_info.toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                mail_info = new CLPFMailInfo();
                mail_info.fromBinary(br);
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 完成任务信息
    /// </summary>
    public sealed class CLPFTaskInfo : INetProtocol
    {
        /// <summary>
        /// 任务Id
        /// </summary>
        public int task_id;
        /// <summary>
        /// 达成数量
        /// </summary>
        public Int64 achieve_num;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(task_id);
            bw.Write(achieve_num);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                task_id = br.ReadInt32();
                achieve_num = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询任务请求
    /// </summary>
    public sealed class CLPFTaskQueryReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
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
    /// 查询任务回应
    /// </summary>
    public sealed class CLPFTaskQueryAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 55;
        /// <summary>
        /// 任务信息数组长度
        /// </summary>
        public int task_info_len;
        /// <summary>
        /// 任务信息数组
        /// </summary>
        public CLPFTaskInfo[] task_info_array;  //max:100
        /// <summary>
        /// 任务信息数组（最大长度）
        /// </summary>
        public const int task_info_array_max_length = 100;
        /// <summary>
        /// 已完成（并领取奖励）的任务Id数组长度
        /// </summary>
        public int finish_task_id_len;
        /// <summary>
        /// 已完成（并领取奖励）的任务Id数组
        /// </summary>
        public int[] finish_task_id_array;  //max:100
        /// <summary>
        /// 已完成（并领取奖励）的任务Id数组（最大长度）
        /// </summary>
        public const int finish_task_id_array_max_length = 100;
        /// <summary>
        /// 当前日活跃值
        /// </summary>
        public int daily_active_value;
        /// <summary>
        /// 当前周活跃值
        /// </summary>
        public int weekly_active_value;
        /// <summary>
        /// 当前月活跃值
        /// </summary>
        public int monthly_active_value;
        /// <summary>
        /// 当前周是否开启人鱼令 1是0否
        /// </summary>
        public sbyte is_weekly_open_merman_token;
        /// <summary>
        /// 已领取奖励的活跃值Id数组长度
        /// </summary>
        public int finish_active_id_len;
        /// <summary>
        /// 已领取奖励的活跃值Id数组
        /// </summary>
        public int[] finish_active_id_array;  //max:100
        /// <summary>
        /// 已领取奖励的活跃值Id数组（最大长度）
        /// </summary>
        public const int finish_active_id_array_max_length = 100;
        /// <summary>
        /// 已领取普通奖励的活跃值Id数组长度
        /// </summary>
        public int finish_ordinary_active_id_len;
        /// <summary>
        /// 已领取普通奖励的活跃值Id数组
        /// </summary>
        public int[] finish_ordinary_active_id_array;  //max:100
        /// <summary>
        /// 已领取普通奖励的活跃值Id数组（最大长度）
        /// </summary>
        public const int finish_ordinary_active_id_array_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(task_info_len);
            if (task_info_len > task_info_array_max_length)
                throw new Exception($"CLPFTaskQueryAck.task_info_array数组长度超过规定限制，期望:100 实际:{task_info_len}");
            for (int i = 0; i < (int)task_info_len; i++)
                task_info_array[i].toBinary(bw);
            bw.Write(finish_task_id_len);
            if (finish_task_id_len > finish_task_id_array_max_length)
                throw new Exception($"CLPFTaskQueryAck.finish_task_id_array数组长度超过规定限制，期望:100 实际:{finish_task_id_len}");
            for (int i = 0; i < (int)finish_task_id_len; i++)
                bw.Write(finish_task_id_array[i]);
            bw.Write(daily_active_value);
            bw.Write(weekly_active_value);
            bw.Write(monthly_active_value);
            bw.Write(is_weekly_open_merman_token);
            bw.Write(finish_active_id_len);
            if (finish_active_id_len > finish_active_id_array_max_length)
                throw new Exception($"CLPFTaskQueryAck.finish_active_id_array数组长度超过规定限制，期望:100 实际:{finish_active_id_len}");
            for (int i = 0; i < (int)finish_active_id_len; i++)
                bw.Write(finish_active_id_array[i]);
            bw.Write(finish_ordinary_active_id_len);
            if (finish_ordinary_active_id_len > finish_ordinary_active_id_array_max_length)
                throw new Exception($"CLPFTaskQueryAck.finish_ordinary_active_id_array数组长度超过规定限制，期望:100 实际:{finish_ordinary_active_id_len}");
            for (int i = 0; i < (int)finish_ordinary_active_id_len; i++)
                bw.Write(finish_ordinary_active_id_array[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                task_info_len = br.ReadInt32();
                task_info_array = new CLPFTaskInfo[(int)task_info_len];
                for (int i = 0; i < task_info_array.Length; i++)
                {
                    task_info_array[i] = new CLPFTaskInfo();
                    task_info_array[i].fromBinary(br);
                }
                finish_task_id_len = br.ReadInt32();
                finish_task_id_array = new int[(int)finish_task_id_len];
                for (int i = 0; i < finish_task_id_array.Length; i++)
                    finish_task_id_array[i] = br.ReadInt32();
                daily_active_value = br.ReadInt32();
                weekly_active_value = br.ReadInt32();
                monthly_active_value = br.ReadInt32();
                is_weekly_open_merman_token = br.ReadSByte();
                finish_active_id_len = br.ReadInt32();
                finish_active_id_array = new int[(int)finish_active_id_len];
                for (int i = 0; i < finish_active_id_array.Length; i++)
                    finish_active_id_array[i] = br.ReadInt32();
                finish_ordinary_active_id_len = br.ReadInt32();
                finish_ordinary_active_id_array = new int[(int)finish_ordinary_active_id_len];
                for (int i = 0; i < finish_ordinary_active_id_array.Length; i++)
                    finish_ordinary_active_id_array[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 领取任务奖励请求
    /// </summary>
    public sealed class CLPFTaskFetchTaskRewardsReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 56;
        /// <summary>
        /// 任务Id
        /// </summary>
        public int task_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(task_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                task_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 领取任务奖励回应
    /// </summary>
    public sealed class CLPFTaskFetchTaskRewardsAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 57;
        /// <summary>
        /// 0成功 1任务不存在 2任务目标未达成 3任务奖励已领取
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFTaskFetchTaskRewardsAck.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 领取活跃度奖励请求
    /// </summary>
    public sealed class CLPFTaskFetchActiveRewardsReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 58;
        /// <summary>
        /// 活跃度奖励Id
        /// </summary>
        public int active_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(active_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                active_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 领取活跃度奖励回应
    /// </summary>
    public sealed class CLPFTaskFetchActiveRewardsAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 59;
        /// <summary>
        /// 0成功 1不存在 2目标未达成 3已领取
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFTaskFetchActiveRewardsAck.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 完成的成就任务数据
    /// </summary>
    public sealed class CLPFTaskAchieveData : INetProtocol
    {
        /// <summary>
        /// 成就任务类型
        /// </summary>
        public int kind;
        /// <summary>
        /// 累计完成数量
        /// </summary>
        public Int64 count;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(kind);
            bw.Write(count);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                kind = br.ReadInt32();
                count = br.ReadInt64();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取成就任务信息请求
    /// </summary>
    public sealed class CLPFTaskAchieveQueryInfoReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 60;

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
    /// 获取成就任务信息回应
    /// </summary>
    public sealed class CLPFTaskAchieveQueryInfoAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 61;
        /// <summary>
        /// 任务数据数组长度
        /// </summary>
        public int data_len;
        /// <summary>
        /// 任务数据数组
        /// </summary>
        public CLPFTaskAchieveData[] data_array;  //max:600
        /// <summary>
        /// 任务数据数组（最大长度）
        /// </summary>
        public const int data_array_max_length = 600;
        /// <summary>
        /// 完成的任务Id数组长度
        /// </summary>
        public int finish_id_len;
        /// <summary>
        /// 完成的任务Id数组
        /// </summary>
        public int[] finish_id_array;  //max:1800
        /// <summary>
        /// 完成的任务Id数组（最大长度）
        /// </summary>
        public const int finish_id_array_max_length = 1800;
        /// <summary>
        /// 成就点
        /// </summary>
        public int achieve_value;
        /// <summary>
        /// 完成的成就点Id数组长度
        /// </summary>
        public int finish_achieve_id_len;
        /// <summary>
        /// 完成的成就点Id数组
        /// </summary>
        public int[] finish_achieve_id_array;  //max:100
        /// <summary>
        /// 完成的成就点Id数组（最大长度）
        /// </summary>
        public const int finish_achieve_id_array_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(data_len);
            if (data_len > data_array_max_length)
                throw new Exception($"CLPFTaskAchieveQueryInfoAck.data_array数组长度超过规定限制，期望:600 实际:{data_len}");
            for (int i = 0; i < (int)data_len; i++)
                data_array[i].toBinary(bw);
            bw.Write(finish_id_len);
            if (finish_id_len > finish_id_array_max_length)
                throw new Exception($"CLPFTaskAchieveQueryInfoAck.finish_id_array数组长度超过规定限制，期望:1800 实际:{finish_id_len}");
            for (int i = 0; i < (int)finish_id_len; i++)
                bw.Write(finish_id_array[i]);
            bw.Write(achieve_value);
            bw.Write(finish_achieve_id_len);
            if (finish_achieve_id_len > finish_achieve_id_array_max_length)
                throw new Exception($"CLPFTaskAchieveQueryInfoAck.finish_achieve_id_array数组长度超过规定限制，期望:100 实际:{finish_achieve_id_len}");
            for (int i = 0; i < (int)finish_achieve_id_len; i++)
                bw.Write(finish_achieve_id_array[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                data_len = br.ReadInt32();
                data_array = new CLPFTaskAchieveData[(int)data_len];
                for (int i = 0; i < data_array.Length; i++)
                {
                    data_array[i] = new CLPFTaskAchieveData();
                    data_array[i].fromBinary(br);
                }
                finish_id_len = br.ReadInt32();
                finish_id_array = new int[(int)finish_id_len];
                for (int i = 0; i < finish_id_array.Length; i++)
                    finish_id_array[i] = br.ReadInt32();
                achieve_value = br.ReadInt32();
                finish_achieve_id_len = br.ReadInt32();
                finish_achieve_id_array = new int[(int)finish_achieve_id_len];
                for (int i = 0; i < finish_achieve_id_array.Length; i++)
                    finish_achieve_id_array[i] = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 领取成就任务奖励请求
    /// </summary>
    public sealed class CLPFTaskAchieveFetchRewardReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 62;
        /// <summary>
        /// 成就任务Id
        /// </summary>
        public int task_achieve_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(task_achieve_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                task_achieve_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 领取成就任务奖励回应
    /// </summary>
    public sealed class CLPFTaskAchieveFetchRewardAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 63;
        /// <summary>
        /// 0成功 1任务不存在 2任务未达成 3奖励已领取
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFTaskAchieveFetchRewardAck.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 客户端配置表发布通知
    /// </summary>
    public sealed class CLPFClientConfigPublishNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 64;
        /// <summary>
        /// 最新配置表的md5，里面出现的字母大写
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
    /// 账号绑定状态请求
    /// </summary>
    public sealed class CLPFAccountBindStateReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 65;

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
    /// 账号绑定状态回应
    /// </summary>
    public sealed class CLPFAccountBindStateAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 66;
        /// <summary>
        /// 0成功
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 绑定数组长度
        /// </summary>
        public sbyte bind_type_length;
        /// <summary>
        /// 绑定数组 1游客2手机号3QQ4微信5Facebook6GooglePlay7GameCenter
        /// </summary>
        public sbyte[] bind_type_array;  //max:20
        /// <summary>
        /// 绑定数组 1游客2手机号3QQ4微信5Facebook6GooglePlay7GameCenter（最大长度）
        /// </summary>
        public const int bind_type_array_max_length = 20;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(bind_type_length);
            if (bind_type_length > bind_type_array_max_length)
                throw new Exception($"CLPFAccountBindStateAck.bind_type_array数组长度超过规定限制，期望:20 实际:{bind_type_length}");
            for (int i = 0; i < (int)bind_type_length; i++)
                bw.Write(bind_type_array[i]);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                bind_type_length = br.ReadSByte();
                bind_type_array = new sbyte[(int)bind_type_length];
                for (int i = 0; i < bind_type_array.Length; i++)
                    bind_type_array[i] = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询玩家昵称请求
    /// </summary>
    public sealed class CLPFPlayerNicknameQueryReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 67;
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
    /// 查询玩家昵称回应
    /// </summary>
    public sealed class CLPFPlayerNicknameQueryAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 68;
        /// <summary>
        /// 0成功 1玩家不存在
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 昵称
        /// </summary>
        public string nickname;  //max:32

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            NetHelper.SafeWriteString(bw, nickname, 32);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                nickname = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 领取排行榜奖励请求
    /// </summary>
    public sealed class CLPFRankRewardFetchReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 69;
        /// <summary>
        /// 1金币榜 2弹头榜
        /// </summary>
        public sbyte rank_type;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(rank_type);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                rank_type = br.ReadSByte();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 领取排行榜奖励回应
    /// </summary>
    public sealed class CLPFRankRewardFetchAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 70;
        /// <summary>
        /// 0成功 1你昨日未上榜，不能领取奖励 2排行榜奖励你已领取过了 3领取排行榜奖励参数错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFRankRewardFetchAck.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取头像地址请求
    /// </summary>
    public sealed class CLPFHeadUrlQueryReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 71;
        /// <summary>
        /// 头像Id
        /// </summary>
        public int head_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(head_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                head_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 获取自定义头像回应
    /// </summary>
    public sealed class CLPFHeadUrlQueryAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 72;
        /// <summary>
        /// 0成功 1自定义头像不存在
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 头像的url地址
        /// </summary>
        public string url;  //max:255

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            NetHelper.SafeWriteString(bw, url, 255);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                url = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 修改性别请求
    /// </summary>
    public sealed class CLPFModifyGenderReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 73;
        /// <summary>
        /// 0保密 1男 2女
        /// </summary>
        public int new_gender;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(new_gender);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                new_gender = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 修改性别回应
    /// </summary>
    public sealed class CLPFModifyGenderAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 74;
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
    /// <summary>
    /// 发送邮件请求
    /// </summary>
    public sealed class CLPFMailSendReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 75;
        /// <summary>
        /// 收件人Id, 1代表客服
        /// </summary>
        public int receiver_id;
        /// <summary>
        /// 邮件标题
        /// </summary>
        public string title;  //max:256
        /// <summary>
        /// 邮件内容
        /// </summary>
        public string content;  //max:512

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(receiver_id);
            NetHelper.SafeWriteString(bw, title, 256);
            NetHelper.SafeWriteString(bw, content, 512);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                receiver_id = br.ReadInt32();
                title = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                content = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 发送邮件回应
    /// </summary>
    public sealed class CLPFMailSendAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 76;
        /// <summary>
        /// 0成功 1收件人Id不存在
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
    /// 活动信息结构
    /// </summary>
    public sealed class CLPFActivityDurationInfo : INetProtocol
    {
        /// <summary>
        /// 活动Id
        /// </summary>
        public int activity_id;
        /// <summary>
        /// 开启时间戳,未开启为0
        /// </summary>
        public UInt32 start_time;
        /// <summary>
        /// 关闭时间戳,未开启为0
        /// </summary>
        public UInt32 end_time;
        /// <summary>
        /// 玩家活动状态json格式字符串,按各活动约定字段解析
        /// </summary>
        public string activity_data;  //max:4096

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(activity_id);
            bw.Write(start_time);
            bw.Write(end_time);
            NetHelper.SafeWriteString(bw, activity_data, 4096);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                activity_id = br.ReadInt32();
                start_time = br.ReadUInt32();
                end_time = br.ReadUInt32();
                activity_data = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询活动持续时间请求
    /// </summary>
    public sealed class CLPFActivitiesDurationQueryReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 77;

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
    /// 查询活动持续时间回应
    /// </summary>
    public sealed class CLPFActivitiesDurationQueryAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 78;
        /// <summary>
        /// 活动信息数组长度
        /// </summary>
        public int info_len;
        /// <summary>
        /// 活动信息数组
        /// </summary>
        public CLPFActivityDurationInfo[] infos;  //max:100
        /// <summary>
        /// 活动信息数组（最大长度）
        /// </summary>
        public const int infos_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(info_len);
            if (info_len > infos_max_length)
                throw new Exception($"CLPFActivitiesDurationQueryAck.infos数组长度超过规定限制，期望:100 实际:{info_len}");
            for (int i = 0; i < (int)info_len; i++)
                infos[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                info_len = br.ReadInt32();
                infos = new CLPFActivityDurationInfo[(int)info_len];
                for (int i = 0; i < infos.Length; i++)
                {
                    infos[i] = new CLPFActivityDurationInfo();
                    infos[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 活动开启通知
    /// </summary>
    public sealed class CLPFActivitiesStartNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 79;
        /// <summary>
        /// 活动信息数组长度
        /// </summary>
        public int info_len;
        /// <summary>
        /// 活动信息数组
        /// </summary>
        public CLPFActivityDurationInfo[] infos;  //max:100
        /// <summary>
        /// 活动信息数组（最大长度）
        /// </summary>
        public const int infos_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(info_len);
            if (info_len > infos_max_length)
                throw new Exception($"CLPFActivitiesStartNtf.infos数组长度超过规定限制，期望:100 实际:{info_len}");
            for (int i = 0; i < (int)info_len; i++)
                infos[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                info_len = br.ReadInt32();
                infos = new CLPFActivityDurationInfo[(int)info_len];
                for (int i = 0; i < infos.Length; i++)
                {
                    infos[i] = new CLPFActivityDurationInfo();
                    infos[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询玩家活动状态通用请求
    /// </summary>
    public sealed class CLPFActivityStateQueryReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 80;
        /// <summary>
        /// 活动id
        /// </summary>
        public int activity_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(activity_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                activity_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 查询玩家活动状态通用回应
    /// </summary>
    public sealed class CLPFActivityStateQueryAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 81;
        /// <summary>
        /// 0成功 1活动尚未开放
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// json格式字符串,按各活动约定字段解析
        /// </summary>
        public string activity_data;  //max:4096

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            NetHelper.SafeWriteString(bw, activity_data, 4096);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                activity_data = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 活动领取奖励请求
    /// </summary>
    public sealed class CLPFActivityActFetchRewardReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 82;
        /// <summary>
        /// 活动id
        /// </summary>
        public int activity_id;
        /// <summary>
        /// 奖励id
        /// </summary>
        public int reward_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(activity_id);
            bw.Write(reward_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                activity_id = br.ReadInt32();
                reward_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 活动领取奖励回应
    /// </summary>
    public sealed class CLPFActivityActFetchRewardAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 83;
        /// <summary>
        /// 0成功 1活动尚未开放 2配置错误 3条件不足 4已领取奖励
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 领取的奖励Id
        /// </summary>
        public int reward_id;
        /// <summary>
        /// 数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:10
        /// <summary>
        /// 物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 10;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(reward_id);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFActivityActFetchRewardAck.items数组长度超过规定限制，期望:10 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                reward_id = br.ReadInt32();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 充值类活动购买请求
    /// </summary>
    public sealed class CLPFActivityRechargeReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 84;
        /// <summary>
        /// 活动id
        /// </summary>
        public int activity_id;
        /// <summary>
        /// 礼包Id
        /// </summary>
        public int content_id;
        /// <summary>
        /// 支付渠道 1微信支付 2支付宝 3支付猫微信 4支付猫支付宝 5聚合微信 6聚合支付宝 7线下支付 85秒支付微信 95秒支付支付宝 10和融通微信 11和融通支付宝 12联合支付微信 13联合支付宝 14使用道具充值
        /// </summary>
        public int pay_mode;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(activity_id);
            bw.Write(content_id);
            bw.Write(pay_mode);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                activity_id = br.ReadInt32();
                content_id = br.ReadInt32();
                pay_mode = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 充值类活动购买回应
    /// </summary>
    public sealed class CLPFActivityRechargeAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 85;
        /// <summary>
        /// 0成功 1活动尚未开放 2配置错误 3不支持的支付渠道 4购买次数已达上限 5系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 支付环境，json格式字符串
        /// </summary>
        public string pay_envir;  //max:4096

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            NetHelper.SafeWriteString(bw, pay_envir, 4096);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                pay_envir = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 充值类活动购买到账通知
    /// </summary>
    public sealed class CLPFActivityRechargeSuccessNtf : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 86;
        /// <summary>
        /// 活动id
        /// </summary>
        public int activity_id;
        /// <summary>
        /// 礼包Id
        /// </summary>
        public int content_id;
        /// <summary>
        /// 获得物品数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 获得物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 获得物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(activity_id);
            bw.Write(content_id);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFActivityRechargeSuccessNtf.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                activity_id = br.ReadInt32();
                content_id = br.ReadInt32();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 活动奖励信息结构
    /// </summary>
    public sealed class CLPFActivityRewardInfo : INetProtocol
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
        /// 领取的奖励Id
        /// </summary>
        public int reward_id;
        /// <summary>
        /// 获得物品数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 获得物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 获得物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;
        /// <summary>
        /// 领取奖励的时间
        /// </summary>
        public UInt32 fetch_time;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(user_id);
            NetHelper.SafeWriteString(bw, nickname, 32);
            bw.Write(reward_id);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFActivityRewardInfo.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
            bw.Write(fetch_time);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                user_id = br.ReadInt32();
                nickname = System.Text.Encoding.UTF8.GetString(br.ReadBytes(br.ReadUInt16()));
                reward_id = br.ReadInt32();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
                fetch_time = br.ReadUInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 活动奖励记录查询请求
    /// </summary>
    public sealed class CLPFActivityRewardQueryReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 87;
        /// <summary>
        /// 活动Id
        /// </summary>
        public int activity_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(activity_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                activity_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 活动奖励记录查询回应
    /// </summary>
    public sealed class CLPFActivityRewardQueryAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 88;
        /// <summary>
        /// 0成功 1活动尚未开放
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 活动奖励信息数组长度
        /// </summary>
        public int info_len;
        /// <summary>
        /// 活动奖励信息数组
        /// </summary>
        public CLPFActivityRewardInfo[] infos;  //max:100
        /// <summary>
        /// 活动奖励信息数组（最大长度）
        /// </summary>
        public const int infos_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(info_len);
            if (info_len > infos_max_length)
                throw new Exception($"CLPFActivityRewardQueryAck.infos数组长度超过规定限制，期望:100 实际:{info_len}");
            for (int i = 0; i < (int)info_len; i++)
                infos[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                info_len = br.ReadInt32();
                infos = new CLPFActivityRewardInfo[(int)info_len];
                for (int i = 0; i < infos.Length; i++)
                {
                    infos[i] = new CLPFActivityRewardInfo();
                    infos[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 平台玩家数查询请求
    /// </summary>
    public sealed class CLPFPlayerCountQueryReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 89;

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
    /// 平台玩家数查询回应
    /// </summary>
    public sealed class CLPFPlayerCountQueryAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 90;
        /// <summary>
        /// 0成功 1系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 人数
        /// </summary>
        public int count;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(errcode);
            bw.Write(count);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                errcode = br.ReadSByte();
                count = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 通用点券购买请求
    /// </summary>
    public sealed class CLPFCouponBuyReq : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 91;
        /// <summary>
        /// 购买内容类型 1商城充值 2购买月卡 3首充礼包 4每日充值 5投资炮倍 6出海保险 7持续奖励礼包 8充值升级炮倍 9日礼包 10周礼包 11月礼包
        /// </summary>
        public int content_type;
        /// <summary>
        /// 购买内容Id
        /// </summary>
        public int content_id;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(content_type);
            bw.Write(content_id);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                content_type = br.ReadInt32();
                content_id = br.ReadInt32();
            } catch (EndOfStreamException) { }
        }
    }
    /// <summary>
    /// 通用点券购买回应
    /// </summary>
    public sealed class CLPFCouponBuyAck : INetProtocol
    {
        /// <summary>
        /// 协议模块ID
        /// </summary>
        public const ushort mid = 2;
        /// <summary>
        /// 协议ID
        /// </summary>
        public const ushort pid = 92;
        /// <summary>
        /// 购买内容类型 1商城充值 2购买月卡 3首充礼包 4每日充值 5投资炮倍 6出海保险 7持续奖励礼包 8充值升级炮倍
        /// </summary>
        public int content_type;
        /// <summary>
        /// 购买内容Id
        /// </summary>
        public int content_id;
        /// <summary>
        /// 0成功 1无此购买项 2资源不足 3购买次数已达上限 4vip等级限制 5系统错误
        /// </summary>
        public sbyte errcode;
        /// <summary>
        /// 获得物品数组长度
        /// </summary>
        public int item_len;
        /// <summary>
        /// 获得物品数组
        /// </summary>
        public CLPFItemInfo[] items;  //max:100
        /// <summary>
        /// 获得物品数组（最大长度）
        /// </summary>
        public const int items_max_length = 100;

        public void toBinary(BinaryWriter bw)
        {
            bw.Write(mid);
            bw.Write(pid);
            bw.Write(content_type);
            bw.Write(content_id);
            bw.Write(errcode);
            bw.Write(item_len);
            if (item_len > items_max_length)
                throw new Exception($"CLPFCouponBuyAck.items数组长度超过规定限制，期望:100 实际:{item_len}");
            for (int i = 0; i < (int)item_len; i++)
                items[i].toBinary(bw);
        }
        public void fromBinary(BinaryReader br)
        {
            try {
                content_type = br.ReadInt32();
                content_id = br.ReadInt32();
                errcode = br.ReadSByte();
                item_len = br.ReadInt32();
                items = new CLPFItemInfo[(int)item_len];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new CLPFItemInfo();
                    items[i].fromBinary(br);
                }
            } catch (EndOfStreamException) { }
        }
    }

    public class ClientPFResponserBase : INetResponser
    {
        public bool processPackage(BinaryReader br, INetReactor reactor, out INetProtocol responseProto)
        {
            responseProto = null;
            if (br.ReadUInt16() != 2)
                return false;

            switch(br.ReadUInt16())
            {
                case 0:
                    responseProto = new CLPFLogoutReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFLogoutReq(responseProto as CLPFLogoutReq);
                    break;
                case 1:
                    responseProto = new CLPFGetHeroReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetHeroReq(responseProto as CLPFGetHeroReq);
                    break;
                case 2:
                    responseProto = new CLPFGetHeroAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetHeroAck(responseProto as CLPFGetHeroAck);
                    break;
                case 3:
                    responseProto = new CLPFGetTitleListReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetTitleListReq(responseProto as CLPFGetTitleListReq);
                    break;
                case 4:
                    responseProto = new CLPFGetTitleListAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetTitleListAck(responseProto as CLPFGetTitleListAck);
                    break;
                case 5:
                    responseProto = new CLPFTitleOperateReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTitleOperateReq(responseProto as CLPFTitleOperateReq);
                    break;
                case 6:
                    responseProto = new CLPFTitleOperateAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTitleOperateAck(responseProto as CLPFTitleOperateAck);
                    break;
                case 7:
                    responseProto = new CLPFGetNameplateListReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetNameplateListReq(responseProto as CLPFGetNameplateListReq);
                    break;
                case 8:
                    responseProto = new CLPFGetNameplateListAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetNameplateListAck(responseProto as CLPFGetNameplateListAck);
                    break;
                case 9:
                    responseProto = new CLPFNameplateOperateReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFNameplateOperateReq(responseProto as CLPFNameplateOperateReq);
                    break;
                case 10:
                    responseProto = new CLPFNameplateOperateAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFNameplateOperateAck(responseProto as CLPFNameplateOperateAck);
                    break;
                case 11:
                    responseProto = new CLPFGetBadgeListReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetBadgeListReq(responseProto as CLPFGetBadgeListReq);
                    break;
                case 12:
                    responseProto = new CLPFGetBadgeListAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetBadgeListAck(responseProto as CLPFGetBadgeListAck);
                    break;
                case 13:
                    responseProto = new CLPFBadgeOperateReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFBadgeOperateReq(responseProto as CLPFBadgeOperateReq);
                    break;
                case 14:
                    responseProto = new CLPFBadgeOperateAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFBadgeOperateAck(responseProto as CLPFBadgeOperateAck);
                    break;
                case 15:
                    responseProto = new CLPFGetHeadListReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetHeadListReq(responseProto as CLPFGetHeadListReq);
                    break;
                case 16:
                    responseProto = new CLPFGetHeadListAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetHeadListAck(responseProto as CLPFGetHeadListAck);
                    break;
                case 17:
                    responseProto = new CLPFHeadOperateReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFHeadOperateReq(responseProto as CLPFHeadOperateReq);
                    break;
                case 18:
                    responseProto = new CLPFHeadOperateAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFHeadOperateAck(responseProto as CLPFHeadOperateAck);
                    break;
                case 19:
                    responseProto = new CLPFGetHeadFrameListReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetHeadFrameListReq(responseProto as CLPFGetHeadFrameListReq);
                    break;
                case 20:
                    responseProto = new CLPFGetHeadFrameListAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetHeadFrameListAck(responseProto as CLPFGetHeadFrameListAck);
                    break;
                case 21:
                    responseProto = new CLPFHeadFrameOperateReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFHeadFrameOperateReq(responseProto as CLPFHeadFrameOperateReq);
                    break;
                case 22:
                    responseProto = new CLPFHeadFrameOperateAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFHeadFrameOperateAck(responseProto as CLPFHeadFrameOperateAck);
                    break;
                case 23:
                    responseProto = new CLPFModifyNicknameReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFModifyNicknameReq(responseProto as CLPFModifyNicknameReq);
                    break;
                case 24:
                    responseProto = new CLPFModifyNicknameAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFModifyNicknameAck(responseProto as CLPFModifyNicknameAck);
                    break;
                case 25:
                    responseProto = new CLPFResSyncNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFResSyncNtf(responseProto as CLPFResSyncNtf);
                    break;
                case 26:
                    responseProto = new CLPFResChangedNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFResChangedNtf(responseProto as CLPFResChangedNtf);
                    break;
                case 27:
                    responseProto = new CLPFItemGetListReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFItemGetListReq(responseProto as CLPFItemGetListReq);
                    break;
                case 28:
                    responseProto = new CLPFItemGetListAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFItemGetListAck(responseProto as CLPFItemGetListAck);
                    break;
                case 29:
                    responseProto = new CLPFItemUseReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFItemUseReq(responseProto as CLPFItemUseReq);
                    break;
                case 30:
                    responseProto = new CLPFItemUseAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFItemUseAck(responseProto as CLPFItemUseAck);
                    break;
                case 31:
                    responseProto = new CLPFItemCountChangeNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFItemCountChangeNtf(responseProto as CLPFItemCountChangeNtf);
                    break;
                case 32:
                    responseProto = new CLPFItemBuyReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFItemBuyReq(responseProto as CLPFItemBuyReq);
                    break;
                case 33:
                    responseProto = new CLPFItemBuyAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFItemBuyAck(responseProto as CLPFItemBuyAck);
                    break;
                case 34:
                    responseProto = new CLPFShopQueryBuyCountReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFShopQueryBuyCountReq(responseProto as CLPFShopQueryBuyCountReq);
                    break;
                case 35:
                    responseProto = new CLPFShopQueryBuyCountAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFShopQueryBuyCountAck(responseProto as CLPFShopQueryBuyCountAck);
                    break;
                case 36:
                    responseProto = new CLPFShopBuyReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFShopBuyReq(responseProto as CLPFShopBuyReq);
                    break;
                case 37:
                    responseProto = new CLPFShopBuyAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFShopBuyAck(responseProto as CLPFShopBuyAck);
                    break;
                case 38:
                    responseProto = new CLPFRechargeReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFRechargeReq(responseProto as CLPFRechargeReq);
                    break;
                case 39:
                    responseProto = new CLPFRechargeAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFRechargeAck(responseProto as CLPFRechargeAck);
                    break;
                case 40:
                    responseProto = new CLPFRechargeSuccessNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFRechargeSuccessNtf(responseProto as CLPFRechargeSuccessNtf);
                    break;
                case 41:
                    responseProto = new CLPFGetRankListReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetRankListReq(responseProto as CLPFGetRankListReq);
                    break;
                case 42:
                    responseProto = new CLPFGetRankListAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFGetRankListAck(responseProto as CLPFGetRankListAck);
                    break;
                case 43:
                    responseProto = new CLPFMailQueryAllIdsReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailQueryAllIdsReq(responseProto as CLPFMailQueryAllIdsReq);
                    break;
                case 44:
                    responseProto = new CLPFMailQueryAllIdsAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailQueryAllIdsAck(responseProto as CLPFMailQueryAllIdsAck);
                    break;
                case 45:
                    responseProto = new CLPFMailBatchQueryContentReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailBatchQueryContentReq(responseProto as CLPFMailBatchQueryContentReq);
                    break;
                case 46:
                    responseProto = new CLPFMailBatchQueryContentAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailBatchQueryContentAck(responseProto as CLPFMailBatchQueryContentAck);
                    break;
                case 47:
                    responseProto = new CLPFMailAccessReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailAccessReq(responseProto as CLPFMailAccessReq);
                    break;
                case 48:
                    responseProto = new CLPFMailAccessAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailAccessAck(responseProto as CLPFMailAccessAck);
                    break;
                case 49:
                    responseProto = new CLPFMailFetchItemReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailFetchItemReq(responseProto as CLPFMailFetchItemReq);
                    break;
                case 50:
                    responseProto = new CLPFMailFetchItemAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailFetchItemAck(responseProto as CLPFMailFetchItemAck);
                    break;
                case 51:
                    responseProto = new CLPFMailRemoveReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailRemoveReq(responseProto as CLPFMailRemoveReq);
                    break;
                case 52:
                    responseProto = new CLPFMailRemoveAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailRemoveAck(responseProto as CLPFMailRemoveAck);
                    break;
                case 53:
                    responseProto = new CLPFMailArriveNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailArriveNtf(responseProto as CLPFMailArriveNtf);
                    break;
                case 54:
                    responseProto = new CLPFTaskQueryReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTaskQueryReq(responseProto as CLPFTaskQueryReq);
                    break;
                case 55:
                    responseProto = new CLPFTaskQueryAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTaskQueryAck(responseProto as CLPFTaskQueryAck);
                    break;
                case 56:
                    responseProto = new CLPFTaskFetchTaskRewardsReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTaskFetchTaskRewardsReq(responseProto as CLPFTaskFetchTaskRewardsReq);
                    break;
                case 57:
                    responseProto = new CLPFTaskFetchTaskRewardsAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTaskFetchTaskRewardsAck(responseProto as CLPFTaskFetchTaskRewardsAck);
                    break;
                case 58:
                    responseProto = new CLPFTaskFetchActiveRewardsReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTaskFetchActiveRewardsReq(responseProto as CLPFTaskFetchActiveRewardsReq);
                    break;
                case 59:
                    responseProto = new CLPFTaskFetchActiveRewardsAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTaskFetchActiveRewardsAck(responseProto as CLPFTaskFetchActiveRewardsAck);
                    break;
                case 60:
                    responseProto = new CLPFTaskAchieveQueryInfoReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTaskAchieveQueryInfoReq(responseProto as CLPFTaskAchieveQueryInfoReq);
                    break;
                case 61:
                    responseProto = new CLPFTaskAchieveQueryInfoAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTaskAchieveQueryInfoAck(responseProto as CLPFTaskAchieveQueryInfoAck);
                    break;
                case 62:
                    responseProto = new CLPFTaskAchieveFetchRewardReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTaskAchieveFetchRewardReq(responseProto as CLPFTaskAchieveFetchRewardReq);
                    break;
                case 63:
                    responseProto = new CLPFTaskAchieveFetchRewardAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFTaskAchieveFetchRewardAck(responseProto as CLPFTaskAchieveFetchRewardAck);
                    break;
                case 64:
                    responseProto = new CLPFClientConfigPublishNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFClientConfigPublishNtf(responseProto as CLPFClientConfigPublishNtf);
                    break;
                case 65:
                    responseProto = new CLPFAccountBindStateReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFAccountBindStateReq(responseProto as CLPFAccountBindStateReq);
                    break;
                case 66:
                    responseProto = new CLPFAccountBindStateAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFAccountBindStateAck(responseProto as CLPFAccountBindStateAck);
                    break;
                case 67:
                    responseProto = new CLPFPlayerNicknameQueryReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFPlayerNicknameQueryReq(responseProto as CLPFPlayerNicknameQueryReq);
                    break;
                case 68:
                    responseProto = new CLPFPlayerNicknameQueryAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFPlayerNicknameQueryAck(responseProto as CLPFPlayerNicknameQueryAck);
                    break;
                case 69:
                    responseProto = new CLPFRankRewardFetchReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFRankRewardFetchReq(responseProto as CLPFRankRewardFetchReq);
                    break;
                case 70:
                    responseProto = new CLPFRankRewardFetchAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFRankRewardFetchAck(responseProto as CLPFRankRewardFetchAck);
                    break;
                case 71:
                    responseProto = new CLPFHeadUrlQueryReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFHeadUrlQueryReq(responseProto as CLPFHeadUrlQueryReq);
                    break;
                case 72:
                    responseProto = new CLPFHeadUrlQueryAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFHeadUrlQueryAck(responseProto as CLPFHeadUrlQueryAck);
                    break;
                case 73:
                    responseProto = new CLPFModifyGenderReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFModifyGenderReq(responseProto as CLPFModifyGenderReq);
                    break;
                case 74:
                    responseProto = new CLPFModifyGenderAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFModifyGenderAck(responseProto as CLPFModifyGenderAck);
                    break;
                case 75:
                    responseProto = new CLPFMailSendReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailSendReq(responseProto as CLPFMailSendReq);
                    break;
                case 76:
                    responseProto = new CLPFMailSendAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFMailSendAck(responseProto as CLPFMailSendAck);
                    break;
                case 77:
                    responseProto = new CLPFActivitiesDurationQueryReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivitiesDurationQueryReq(responseProto as CLPFActivitiesDurationQueryReq);
                    break;
                case 78:
                    responseProto = new CLPFActivitiesDurationQueryAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivitiesDurationQueryAck(responseProto as CLPFActivitiesDurationQueryAck);
                    break;
                case 79:
                    responseProto = new CLPFActivitiesStartNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivitiesStartNtf(responseProto as CLPFActivitiesStartNtf);
                    break;
                case 80:
                    responseProto = new CLPFActivityStateQueryReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivityStateQueryReq(responseProto as CLPFActivityStateQueryReq);
                    break;
                case 81:
                    responseProto = new CLPFActivityStateQueryAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivityStateQueryAck(responseProto as CLPFActivityStateQueryAck);
                    break;
                case 82:
                    responseProto = new CLPFActivityActFetchRewardReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivityActFetchRewardReq(responseProto as CLPFActivityActFetchRewardReq);
                    break;
                case 83:
                    responseProto = new CLPFActivityActFetchRewardAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivityActFetchRewardAck(responseProto as CLPFActivityActFetchRewardAck);
                    break;
                case 84:
                    responseProto = new CLPFActivityRechargeReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivityRechargeReq(responseProto as CLPFActivityRechargeReq);
                    break;
                case 85:
                    responseProto = new CLPFActivityRechargeAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivityRechargeAck(responseProto as CLPFActivityRechargeAck);
                    break;
                case 86:
                    responseProto = new CLPFActivityRechargeSuccessNtf();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivityRechargeSuccessNtf(responseProto as CLPFActivityRechargeSuccessNtf);
                    break;
                case 87:
                    responseProto = new CLPFActivityRewardQueryReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivityRewardQueryReq(responseProto as CLPFActivityRewardQueryReq);
                    break;
                case 88:
                    responseProto = new CLPFActivityRewardQueryAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFActivityRewardQueryAck(responseProto as CLPFActivityRewardQueryAck);
                    break;
                case 89:
                    responseProto = new CLPFPlayerCountQueryReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFPlayerCountQueryReq(responseProto as CLPFPlayerCountQueryReq);
                    break;
                case 90:
                    responseProto = new CLPFPlayerCountQueryAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFPlayerCountQueryAck(responseProto as CLPFPlayerCountQueryAck);
                    break;
                case 91:
                    responseProto = new CLPFCouponBuyReq();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFCouponBuyReq(responseProto as CLPFCouponBuyReq);
                    break;
                case 92:
                    responseProto = new CLPFCouponBuyAck();
                    responseProto.fromBinary(br);
                    reactor?.onRecvMessage(responseProto);
                    onRecv_CLPFCouponBuyAck(responseProto as CLPFCouponBuyAck);
                    break;
            }
            return responseProto != null;
        }

        public virtual void onRecv_CLPFLogoutReq(CLPFLogoutReq proto) { }
        public virtual void onRecv_CLPFGetHeroReq(CLPFGetHeroReq proto) { }
        public virtual void onRecv_CLPFGetHeroAck(CLPFGetHeroAck proto) { }
        public virtual void onRecv_CLPFGetTitleListReq(CLPFGetTitleListReq proto) { }
        public virtual void onRecv_CLPFGetTitleListAck(CLPFGetTitleListAck proto) { }
        public virtual void onRecv_CLPFTitleOperateReq(CLPFTitleOperateReq proto) { }
        public virtual void onRecv_CLPFTitleOperateAck(CLPFTitleOperateAck proto) { }
        public virtual void onRecv_CLPFGetNameplateListReq(CLPFGetNameplateListReq proto) { }
        public virtual void onRecv_CLPFGetNameplateListAck(CLPFGetNameplateListAck proto) { }
        public virtual void onRecv_CLPFNameplateOperateReq(CLPFNameplateOperateReq proto) { }
        public virtual void onRecv_CLPFNameplateOperateAck(CLPFNameplateOperateAck proto) { }
        public virtual void onRecv_CLPFGetBadgeListReq(CLPFGetBadgeListReq proto) { }
        public virtual void onRecv_CLPFGetBadgeListAck(CLPFGetBadgeListAck proto) { }
        public virtual void onRecv_CLPFBadgeOperateReq(CLPFBadgeOperateReq proto) { }
        public virtual void onRecv_CLPFBadgeOperateAck(CLPFBadgeOperateAck proto) { }
        public virtual void onRecv_CLPFGetHeadListReq(CLPFGetHeadListReq proto) { }
        public virtual void onRecv_CLPFGetHeadListAck(CLPFGetHeadListAck proto) { }
        public virtual void onRecv_CLPFHeadOperateReq(CLPFHeadOperateReq proto) { }
        public virtual void onRecv_CLPFHeadOperateAck(CLPFHeadOperateAck proto) { }
        public virtual void onRecv_CLPFGetHeadFrameListReq(CLPFGetHeadFrameListReq proto) { }
        public virtual void onRecv_CLPFGetHeadFrameListAck(CLPFGetHeadFrameListAck proto) { }
        public virtual void onRecv_CLPFHeadFrameOperateReq(CLPFHeadFrameOperateReq proto) { }
        public virtual void onRecv_CLPFHeadFrameOperateAck(CLPFHeadFrameOperateAck proto) { }
        public virtual void onRecv_CLPFModifyNicknameReq(CLPFModifyNicknameReq proto) { }
        public virtual void onRecv_CLPFModifyNicknameAck(CLPFModifyNicknameAck proto) { }
        public virtual void onRecv_CLPFResSyncNtf(CLPFResSyncNtf proto) { }
        public virtual void onRecv_CLPFResChangedNtf(CLPFResChangedNtf proto) { }
        public virtual void onRecv_CLPFItemGetListReq(CLPFItemGetListReq proto) { }
        public virtual void onRecv_CLPFItemGetListAck(CLPFItemGetListAck proto) { }
        public virtual void onRecv_CLPFItemUseReq(CLPFItemUseReq proto) { }
        public virtual void onRecv_CLPFItemUseAck(CLPFItemUseAck proto) { }
        public virtual void onRecv_CLPFItemCountChangeNtf(CLPFItemCountChangeNtf proto) { }
        public virtual void onRecv_CLPFItemBuyReq(CLPFItemBuyReq proto) { }
        public virtual void onRecv_CLPFItemBuyAck(CLPFItemBuyAck proto) { }
        public virtual void onRecv_CLPFShopQueryBuyCountReq(CLPFShopQueryBuyCountReq proto) { }
        public virtual void onRecv_CLPFShopQueryBuyCountAck(CLPFShopQueryBuyCountAck proto) { }
        public virtual void onRecv_CLPFShopBuyReq(CLPFShopBuyReq proto) { }
        public virtual void onRecv_CLPFShopBuyAck(CLPFShopBuyAck proto) { }
        public virtual void onRecv_CLPFRechargeReq(CLPFRechargeReq proto) { }
        public virtual void onRecv_CLPFRechargeAck(CLPFRechargeAck proto) { }
        public virtual void onRecv_CLPFRechargeSuccessNtf(CLPFRechargeSuccessNtf proto) { }
        public virtual void onRecv_CLPFGetRankListReq(CLPFGetRankListReq proto) { }
        public virtual void onRecv_CLPFGetRankListAck(CLPFGetRankListAck proto) { }
        public virtual void onRecv_CLPFMailQueryAllIdsReq(CLPFMailQueryAllIdsReq proto) { }
        public virtual void onRecv_CLPFMailQueryAllIdsAck(CLPFMailQueryAllIdsAck proto) { }
        public virtual void onRecv_CLPFMailBatchQueryContentReq(CLPFMailBatchQueryContentReq proto) { }
        public virtual void onRecv_CLPFMailBatchQueryContentAck(CLPFMailBatchQueryContentAck proto) { }
        public virtual void onRecv_CLPFMailAccessReq(CLPFMailAccessReq proto) { }
        public virtual void onRecv_CLPFMailAccessAck(CLPFMailAccessAck proto) { }
        public virtual void onRecv_CLPFMailFetchItemReq(CLPFMailFetchItemReq proto) { }
        public virtual void onRecv_CLPFMailFetchItemAck(CLPFMailFetchItemAck proto) { }
        public virtual void onRecv_CLPFMailRemoveReq(CLPFMailRemoveReq proto) { }
        public virtual void onRecv_CLPFMailRemoveAck(CLPFMailRemoveAck proto) { }
        public virtual void onRecv_CLPFMailArriveNtf(CLPFMailArriveNtf proto) { }
        public virtual void onRecv_CLPFTaskQueryReq(CLPFTaskQueryReq proto) { }
        public virtual void onRecv_CLPFTaskQueryAck(CLPFTaskQueryAck proto) { }
        public virtual void onRecv_CLPFTaskFetchTaskRewardsReq(CLPFTaskFetchTaskRewardsReq proto) { }
        public virtual void onRecv_CLPFTaskFetchTaskRewardsAck(CLPFTaskFetchTaskRewardsAck proto) { }
        public virtual void onRecv_CLPFTaskFetchActiveRewardsReq(CLPFTaskFetchActiveRewardsReq proto) { }
        public virtual void onRecv_CLPFTaskFetchActiveRewardsAck(CLPFTaskFetchActiveRewardsAck proto) { }
        public virtual void onRecv_CLPFTaskAchieveQueryInfoReq(CLPFTaskAchieveQueryInfoReq proto) { }
        public virtual void onRecv_CLPFTaskAchieveQueryInfoAck(CLPFTaskAchieveQueryInfoAck proto) { }
        public virtual void onRecv_CLPFTaskAchieveFetchRewardReq(CLPFTaskAchieveFetchRewardReq proto) { }
        public virtual void onRecv_CLPFTaskAchieveFetchRewardAck(CLPFTaskAchieveFetchRewardAck proto) { }
        public virtual void onRecv_CLPFClientConfigPublishNtf(CLPFClientConfigPublishNtf proto) { }
        public virtual void onRecv_CLPFAccountBindStateReq(CLPFAccountBindStateReq proto) { }
        public virtual void onRecv_CLPFAccountBindStateAck(CLPFAccountBindStateAck proto) { }
        public virtual void onRecv_CLPFPlayerNicknameQueryReq(CLPFPlayerNicknameQueryReq proto) { }
        public virtual void onRecv_CLPFPlayerNicknameQueryAck(CLPFPlayerNicknameQueryAck proto) { }
        public virtual void onRecv_CLPFRankRewardFetchReq(CLPFRankRewardFetchReq proto) { }
        public virtual void onRecv_CLPFRankRewardFetchAck(CLPFRankRewardFetchAck proto) { }
        public virtual void onRecv_CLPFHeadUrlQueryReq(CLPFHeadUrlQueryReq proto) { }
        public virtual void onRecv_CLPFHeadUrlQueryAck(CLPFHeadUrlQueryAck proto) { }
        public virtual void onRecv_CLPFModifyGenderReq(CLPFModifyGenderReq proto) { }
        public virtual void onRecv_CLPFModifyGenderAck(CLPFModifyGenderAck proto) { }
        public virtual void onRecv_CLPFMailSendReq(CLPFMailSendReq proto) { }
        public virtual void onRecv_CLPFMailSendAck(CLPFMailSendAck proto) { }
        public virtual void onRecv_CLPFActivitiesDurationQueryReq(CLPFActivitiesDurationQueryReq proto) { }
        public virtual void onRecv_CLPFActivitiesDurationQueryAck(CLPFActivitiesDurationQueryAck proto) { }
        public virtual void onRecv_CLPFActivitiesStartNtf(CLPFActivitiesStartNtf proto) { }
        public virtual void onRecv_CLPFActivityStateQueryReq(CLPFActivityStateQueryReq proto) { }
        public virtual void onRecv_CLPFActivityStateQueryAck(CLPFActivityStateQueryAck proto) { }
        public virtual void onRecv_CLPFActivityActFetchRewardReq(CLPFActivityActFetchRewardReq proto) { }
        public virtual void onRecv_CLPFActivityActFetchRewardAck(CLPFActivityActFetchRewardAck proto) { }
        public virtual void onRecv_CLPFActivityRechargeReq(CLPFActivityRechargeReq proto) { }
        public virtual void onRecv_CLPFActivityRechargeAck(CLPFActivityRechargeAck proto) { }
        public virtual void onRecv_CLPFActivityRechargeSuccessNtf(CLPFActivityRechargeSuccessNtf proto) { }
        public virtual void onRecv_CLPFActivityRewardQueryReq(CLPFActivityRewardQueryReq proto) { }
        public virtual void onRecv_CLPFActivityRewardQueryAck(CLPFActivityRewardQueryAck proto) { }
        public virtual void onRecv_CLPFPlayerCountQueryReq(CLPFPlayerCountQueryReq proto) { }
        public virtual void onRecv_CLPFPlayerCountQueryAck(CLPFPlayerCountQueryAck proto) { }
        public virtual void onRecv_CLPFCouponBuyReq(CLPFCouponBuyReq proto) { }
        public virtual void onRecv_CLPFCouponBuyAck(CLPFCouponBuyAck proto) { }
    }
}
