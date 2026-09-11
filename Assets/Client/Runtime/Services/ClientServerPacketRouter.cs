using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Pinball.Client.Services
{
    public enum ClientServerPacketCategory
    {
        Unknown,
        ClientGtHandshakeRequest,
        ClientGtHandshakeAck,
        ClientGtDisconnectNotification,
        ClientGtLoginRequest,
        ClientGtLoginAck,
        ClientPfGetHeroRequest,
        ClientPfGetHeroAck,
        ClientPfPersonalizationListRequest,
        ClientPfPersonalizationListAck,
        ClientPfPersonalizationOperateRequest,
        ClientPfPersonalizationOperateAck,
    }

    public enum ClientServerPacketDirection
    {
        Receive,
        Send,
    }

    [Serializable]
    public sealed class ClientServerPacketRecord
    {
        public int Sequence;
        public string UtcTime;
        public string Direction;
        public int PacketLength;
        public int Module;
        public int Protocol;
        public string ProtocolName;
        public string Category;
        public string ParseStatus;
        public string Detail;
    }

    /// <summary>
    /// Raw server packet boundary for the Client data pipeline.
    /// It records packet metadata as JSONL and keeps an optional local binary
    /// capture for later decoder/replay tests. It does not update UI or services.
    /// </summary>
    public sealed class ClientServerPacketRouter : IDisposable
    {
        private readonly bool _captureRawPackets;
        private readonly string _logDirectory;
        private FileStream _rawPacketStream;
        private StreamWriter _recordWriter;
        private StreamWriter _protocolWriter;
        private int _sequence;
        private static Dictionary<string, string> _protocolCatalog;

        public string LogDirectory { get { return _logDirectory; } }
        public string RecordFilePath { get; private set; }
        public string ProtocolFilePath { get; private set; }
        public string RawPacketFilePath { get; private set; }

        public event Action<ClientServerPacketRecord> PacketClassified;

        public ClientServerPacketRouter(bool captureRawPackets = true)
        {
            _captureRawPackets = captureRawPackets;
            _logDirectory = Path.Combine(Application.persistentDataPath, "ClientServerLogs");
        }

        public ClientServerPacketRecord Accept(byte[] packet)
        {
            return Record(packet, ClientServerPacketDirection.Receive);
        }

        public ClientServerPacketRecord RecordOutgoing(byte[] packet)
        {
            return Record(packet, ClientServerPacketDirection.Send);
        }

        private ClientServerPacketRecord Record(byte[] packet, ClientServerPacketDirection direction)
        {
            if (packet == null || packet.Length < 8)
                throw new InvalidDataException("服务器数据包为空或长度不足 8 字节");

            EnsureFiles();

            int declaredLength = BitConverter.ToInt32(packet, 0);
            ushort module = BitConverter.ToUInt16(packet, 4);
            ushort protocol = BitConverter.ToUInt16(packet, 6);
            ClientServerPacketCategory category = Classify(module, protocol);
            string protocolName = ResolveProtocolName(module, protocol);

            ClientServerPacketRecord record = new ClientServerPacketRecord
            {
                Sequence = ++_sequence,
                UtcTime = DateTime.UtcNow.ToString("O"),
                Direction = direction.ToString(),
                PacketLength = packet.Length,
                Module = module,
                Protocol = protocol,
                ProtocolName = protocolName,
                Category = category.ToString(),
                ParseStatus = declaredLength == packet.Length ? "HeaderValid" : "HeaderLengthMismatch",
                Detail = BuildDetail(category, protocolName, packet),
            };

            _recordWriter.WriteLine(JsonUtility.ToJson(record));
            _recordWriter.Flush();

            string readableRecord = FormatReadableProtocolRecord(record);
            _protocolWriter.WriteLine(readableRecord);
            _protocolWriter.Flush();

            if (_captureRawPackets)
            {
                byte[] length = BitConverter.GetBytes(packet.Length);
                _rawPacketStream.Write(length, 0, length.Length);
                _rawPacketStream.Write(packet, 0, packet.Length);
                _rawPacketStream.Flush();
            }

            Debug.Log("[ClientServerData] packet #" + record.Sequence +
                      " category=" + record.Category +
                      " module=" + record.Module +
                      " protocol=" + record.Protocol +
                      " bytes=" + record.PacketLength +
                      " status=" + record.ParseStatus +
                      " direction=" + record.Direction);
            Debug.Log("[ClientServerData]\n" + readableRecord);
            PacketClassified?.Invoke(record);
            return record;
        }

        public void Dispose()
        {
            if (_recordWriter != null)
            {
                _recordWriter.Flush();
                _recordWriter.Dispose();
                _recordWriter = null;
            }

            if (_protocolWriter != null)
            {
                _protocolWriter.Flush();
                _protocolWriter.Dispose();
                _protocolWriter = null;
            }

            if (_rawPacketStream != null)
            {
                _rawPacketStream.Flush();
                _rawPacketStream.Dispose();
                _rawPacketStream = null;
            }
        }

        private void EnsureFiles()
        {
            if (_recordWriter != null)
                return;

            Directory.CreateDirectory(_logDirectory);
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            RecordFilePath = Path.Combine(_logDirectory, "session-" + stamp + ".jsonl");
            ProtocolFilePath = Path.Combine(_logDirectory, "session-" + stamp + ".protocol.log");
            RawPacketFilePath = Path.Combine(_logDirectory, "session-" + stamp + ".packets.bin");
            _recordWriter = new StreamWriter(RecordFilePath, false, new UTF8Encoding(false));
            _protocolWriter = new StreamWriter(ProtocolFilePath, false, new UTF8Encoding(false));
            if (_captureRawPackets)
                _rawPacketStream = new FileStream(RawPacketFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);

            Debug.Log("[ClientServerData] 本地记录目录：" + _logDirectory);
            Debug.Log("[ClientServerData] 分类记录：" + RecordFilePath);
            Debug.Log("[ClientServerData] 可读协议记录：" + ProtocolFilePath);
            if (_captureRawPackets)
                Debug.Log("[ClientServerData] 原始数据包：" + RawPacketFilePath);
        }

        private static ClientServerPacketCategory Classify(ushort module, ushort protocol)
        {
            if (module == 1 && protocol == 0)
                return ClientServerPacketCategory.ClientGtHandshakeRequest;
            if (module == 1 && protocol == 1)
                return ClientServerPacketCategory.ClientGtHandshakeAck;
            if (module == 1 && protocol == 2)
                return ClientServerPacketCategory.ClientGtDisconnectNotification;
            if (module == 1 && protocol == 3)
                return ClientServerPacketCategory.ClientGtLoginRequest;
            if (module == 1 && protocol == 5)
                return ClientServerPacketCategory.ClientGtLoginAck;
            if (module == 2 && protocol == 1)
                return ClientServerPacketCategory.ClientPfGetHeroRequest;
            if (module == 2 && protocol == 2)
                return ClientServerPacketCategory.ClientPfGetHeroAck;
            if (module == 2 && protocol >= 3 && protocol <= 22)
            {
                int operationOffset = (protocol - 3) % 4;
                if (operationOffset == 0) return ClientServerPacketCategory.ClientPfPersonalizationListRequest;
                if (operationOffset == 1) return ClientServerPacketCategory.ClientPfPersonalizationListAck;
                if (operationOffset == 2) return ClientServerPacketCategory.ClientPfPersonalizationOperateRequest;
                return ClientServerPacketCategory.ClientPfPersonalizationOperateAck;
            }
            return ClientServerPacketCategory.Unknown;
        }

        private static string ResolveProtocolName(ushort module, ushort protocol)
        {
            if (_protocolCatalog == null)
                BuildProtocolCatalog();

            string name;
            if (_protocolCatalog.TryGetValue(module + ":" + protocol, out name))
                return name;

            // Keep the verified WebGL handshake/login/GetHero names readable even
            // when IL2CPP strips protocol types that are only discovered by reflection.
            if (module == 1 && protocol == 0) return "CLGTHandReq";
            if (module == 1 && protocol == 1) return "CLGTHandAck";
            if (module == 1 && protocol == 2) return "CLGTDisconnectNtf";
            if (module == 1 && protocol == 3) return "CLGTLoginReq";
            if (module == 1 && protocol == 5) return "CLGTLoginAck";
            if (module == 2 && protocol == 1) return "CLPFGetHeroReq";
            if (module == 2 && protocol == 2) return "CLPFGetHeroAck";
            return string.Empty;
        }

        private static void BuildProtocolCatalog()
        {
            _protocolCatalog = new Dictionary<string, string>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }

                foreach (Type type in types)
                {
                    if (type == null || (!type.Name.StartsWith("CLGT") && !type.Name.StartsWith("CLPF")))
                        continue;

                    FieldInfo midField = type.GetField("mid", BindingFlags.Public | BindingFlags.Static);
                    FieldInfo pidField = type.GetField("pid", BindingFlags.Public | BindingFlags.Static);
                    if (midField == null || pidField == null)
                        continue;

                    try
                    {
                        ushort mid = Convert.ToUInt16(midField.GetValue(null));
                        ushort pid = Convert.ToUInt16(pidField.GetValue(null));
                        _protocolCatalog[mid + ":" + pid] = type.Name;
                    }
                    catch
                    {
                        // A malformed or stripped protocol type must not stop packet capture.
                    }
                }
            }
        }

        private static string BuildDetail(ClientServerPacketCategory category, string protocolName, byte[] packet)
        {
            string personalizationDetail;
            if (TryBuildPersonalizationDetail(category, packet, out personalizationDetail))
                return personalizationDetail;

            if (category == ClientServerPacketCategory.Unknown)
                return string.IsNullOrEmpty(protocolName)
                    ? "未注册协议，已保留原始包"
                    : "已识别协议类型=" + protocolName;
            if (category == ClientServerPacketCategory.ClientPfGetHeroRequest)
                return "空请求体";

            if (category == ClientServerPacketCategory.ClientGtHandshakeRequest)
            {
                try
                {
                    using (var reader = new BinaryReader(new MemoryStream(packet)))
                    {
                        reader.ReadInt32();
                        reader.ReadUInt16();
                        reader.ReadUInt16();
                        uint platform = reader.ReadUInt32();
                        uint product = reader.ReadUInt32();
                        uint version = reader.ReadUInt32();
                        return "平台=" + platform + "，产品=" + product + "，版本=" + version + "，设备与渠道字段=已隐藏";
                    }
                }
                catch (EndOfStreamException)
                {
                    return "握手请求字段不完整";
                }
            }

            if (category == ClientServerPacketCategory.ClientGtLoginRequest)
            {
                try
                {
                    using (var reader = new BinaryReader(new MemoryStream(packet)))
                    {
                        reader.ReadInt32();
                        reader.ReadUInt16();
                        reader.ReadUInt16();
                        byte loginType = reader.ReadByte();
                        ushort tokenLength = reader.ReadUInt16();
                        return "登录方式=" + loginType + "，令牌长度=" + tokenLength + "（正文已隐藏）";
                    }
                }
                catch (EndOfStreamException)
                {
                    return "登录请求字段不完整";
                }
            }

            if (category == ClientServerPacketCategory.ClientGtLoginAck)
                return BuildLoginAckDetail(packet);

            if (category == ClientServerPacketCategory.ClientGtDisconnectNotification)
                return BuildDisconnectNotificationDetail(packet);

            if (category == ClientServerPacketCategory.ClientGtHandshakeAck)
            {
                try
                {
                    using (var reader = new BinaryReader(new MemoryStream(packet)))
                    {
                        reader.ReadInt32();
                        reader.ReadUInt16();
                        reader.ReadUInt16();
                        int errorCode = reader.ReadSByte();
                        return errorCode == 0 ? "状态=成功" : "状态=失败，错误码=" + errorCode;
                    }
                }
                catch (EndOfStreamException)
                {
                    return "状态字段缺失";
                }
            }

            if (category != ClientServerPacketCategory.ClientPfGetHeroAck)
                return "已按模块/协议分类，类型=" + protocolName;

            try
            {
                using (var reader = new BinaryReader(new MemoryStream(packet)))
                {
                    reader.ReadInt32();
                    reader.ReadUInt16();
                    reader.ReadUInt16();
                    int heroCount = reader.ReadInt32();
                    if (heroCount < 0 || heroCount > 100)
                        return "英雄数组长度异常：" + heroCount;
                    var heroSummary = new StringBuilder();
                    for (int i = 0; i < heroCount; i++)
                    {
                        int heroId = reader.ReadInt32();
                        int star = reader.ReadInt32();
                        int level = reader.ReadInt32();
                        for (int field = 0; field < 5; field++) reader.ReadInt32();
                        for (int field = 0; field < 3; field++) reader.ReadInt64();
                        for (int field = 0; field < 3; field++) reader.ReadInt32();
                        long fight = reader.ReadInt64();
                        if (i < 10)
                        {
                            if (heroSummary.Length > 0) heroSummary.Append("；");
                            heroSummary.Append("ID=").Append(heroId)
                                .Append(" 星级=").Append(star)
                                .Append(" 等级=").Append(level)
                                .Append(" 战力=").Append(fight);
                        }
                    }

                    int teamCount = reader.ReadInt32();
                    if (teamCount < 0 || teamCount > 10)
                        return "英雄数组=" + heroCount + "，英雄组数组长度异常：" + teamCount;
                    if (heroCount > 10) heroSummary.Append("；其余英雄=" + (heroCount - 10));
                    return "英雄数组=" + heroCount + "，英雄组数组=" + teamCount + "，英雄=" + heroSummary;
                }
            }
            catch (EndOfStreamException)
            {
                return "英雄数据包截断";
            }
            catch (IOException)
            {
                return "英雄数据包解析失败";
            }
        }

        private static string BuildLoginAckDetail(byte[] packet)
        {
            try
            {
                using (var reader = new BinaryReader(new MemoryStream(packet)))
                {
                    reader.ReadInt32();
                    reader.ReadUInt16();
                    reader.ReadUInt16();
                    sbyte errorCode = reader.ReadSByte();
                    if (errorCode != 0)
                        return "状态=失败，错误码=" + errorCode;

                    reader.ReadInt32(); // user_id：不记录
                    SkipProtocolString(reader, 32, "nickname");
                    reader.ReadSByte();
                    reader.ReadInt32(); // gender：不记录
                    int head = reader.ReadInt32();
                    int headFrame = reader.ReadInt32();
                    int title = reader.ReadInt32();
                    int badge = reader.ReadInt32();
                    int nameplate = reader.ReadInt32();
                    SkipProtocolString(reader, 20, "phone");
                    reader.ReadInt64(); // coupon：不记录
                    int itemCount = reader.ReadInt32();
                    if (itemCount < 0 || itemCount > 100)
                        return "状态=成功，登录物品数量异常：" + itemCount;

                    return "状态=成功，头像ID=" + head + "，头像框ID=" + headFrame +
                           "，称号ID=" + title + "，徽章ID=" + badge +
                           "，铭牌ID=" + nameplate + "，物品数量=" + itemCount;
                }
            }
            catch (EndOfStreamException)
            {
                return "登录回包字段不完整";
            }
            catch (IOException)
            {
                return "登录回包解析失败";
            }
        }

        private static string BuildDisconnectNotificationDetail(byte[] packet)
        {
            try
            {
                using (var reader = new BinaryReader(new MemoryStream(packet)))
                {
                    reader.ReadInt32();
                    reader.ReadUInt16();
                    reader.ReadUInt16();
                    byte code = reader.ReadByte();
                    return "断开码=" + code + "，原因=" + DescribeClientGtDisconnectCode(code);
                }
            }
            catch (EndOfStreamException)
            {
                return "断开通知字段不完整";
            }
        }

        /// <summary>ClientGT 协议 1/2 的服务端断开原因，不包含用户或连接敏感信息。</summary>
        public static string DescribeClientGtDisconnectCode(byte code)
        {
            switch (code)
            {
                case 0: return "服务器断开通知";
                case 1: return "连接超时";
                case 2: return "被踢下线";
                case 3: return "账号在其他位置登录";
                case 4: return "网关维护";
                case 5: return "平台维护";
                case 6: return "游戏维护";
                case 7: return "与平台服务器断开";
                case 8: return "与游戏服务器断开";
                case 9: return "系统错误";
                case 10: return "离线挂机";
                default: return "未定义断开码";
            }
        }

        private static void SkipProtocolString(BinaryReader reader, int maximumLength, string fieldName)
        {
            ushort length = reader.ReadUInt16();
            if (length > maximumLength)
                throw new InvalidDataException(fieldName + "长度超过协议上限：" + length);
            byte[] value = reader.ReadBytes(length);
            if (value.Length != length)
                throw new EndOfStreamException(fieldName + "字段截断");
        }

        private static bool TryBuildPersonalizationDetail(ClientServerPacketCategory category, byte[] packet, out string detail)
        {
            detail = null;
            if (category != ClientServerPacketCategory.ClientPfPersonalizationListRequest &&
                category != ClientServerPacketCategory.ClientPfPersonalizationListAck &&
                category != ClientServerPacketCategory.ClientPfPersonalizationOperateRequest &&
                category != ClientServerPacketCategory.ClientPfPersonalizationOperateAck)
                return false;

            try
            {
                using (var reader = new BinaryReader(new MemoryStream(packet)))
                {
                    reader.ReadInt32();
                    reader.ReadUInt16();
                    ushort protocol = reader.ReadUInt16();
                    string itemType = GetPersonalizationItemType(protocol);

                    if (category == ClientServerPacketCategory.ClientPfPersonalizationListRequest)
                    {
                        detail = "查询=" + itemType + "列表";
                        return true;
                    }

                    if (category == ClientServerPacketCategory.ClientPfPersonalizationListAck)
                    {
                        sbyte errorCode = reader.ReadSByte();
                        int count = reader.ReadInt32();
                        if (count < 0 || count > 100)
                        {
                            detail = itemType + "列表数量异常：" + count;
                            return true;
                        }

                        var identifiers = new StringBuilder();
                        for (int index = 0; index < count; index++)
                        {
                            int identifier = reader.ReadInt32();
                            if (index < 20)
                            {
                                if (identifiers.Length > 0) identifiers.Append(",");
                                identifiers.Append(identifier);
                            }
                        }

                        detail = "查询=" + itemType + "列表，状态=" + GetProtocolStatus(errorCode) + "，数量=" + count +
                                 "，ID=" + (identifiers.Length == 0 ? "无" : identifiers.ToString()) +
                                 (count > 20 ? "，其余=" + (count - 20) : string.Empty);
                        return true;
                    }

                    if (category == ClientServerPacketCategory.ClientPfPersonalizationOperateRequest)
                    {
                        int identifier = reader.ReadInt32();
                        int operation = reader.ReadInt32();
                        detail = "操作=" + itemType + "，ID=" + identifier + "，动作=" + GetPersonalizationOperationName(operation);
                        return true;
                    }

                    sbyte operateErrorCode = reader.ReadSByte();
                    int operateType = reader.ReadInt32();
                    detail = "操作=" + itemType + "，状态=" + GetProtocolStatus(operateErrorCode) +
                             "，动作=" + GetPersonalizationOperationName(operateType);
                    return true;
                }
            }
            catch (EndOfStreamException)
            {
                detail = "个性化协议数据包截断";
                return true;
            }
            catch (IOException)
            {
                detail = "个性化协议解析失败";
                return true;
            }
        }

        private static string GetPersonalizationItemType(ushort protocol)
        {
            if (protocol >= 3 && protocol <= 6) return "称号";
            if (protocol >= 7 && protocol <= 10) return "铭牌";
            if (protocol >= 11 && protocol <= 14) return "徽章";
            if (protocol >= 15 && protocol <= 18) return "头像";
            if (protocol >= 19 && protocol <= 22) return "头像框";
            return "未知个性化项";
        }

        private static string GetPersonalizationOperationName(int operation)
        {
            if (operation == 1) return "装备";
            if (operation == 2) return "卸下";
            return "未知(" + operation + ")";
        }

        private static string GetProtocolStatus(sbyte errorCode)
        {
            return errorCode == 0 ? "成功" : "失败，错误码=" + errorCode;
        }

        private static string FormatReadableProtocolRecord(ClientServerPacketRecord record)
        {
            var builder = new StringBuilder();
            string direction = record.Direction == ClientServerPacketDirection.Send.ToString() ? "发送" : "接收";
            builder.AppendLine("============================================================");
            builder.AppendLine(string.Format("数据包 #{0:0000}    {1}", record.Sequence, direction));
            builder.AppendLine("协议类型：" + record.ProtocolName);
            builder.AppendLine("协议分类：" + record.Category);
            builder.AppendLine("模块：" + record.Module + "（" + GetModuleName(record.Module) + "）");
            builder.AppendLine("协议号：" + record.Protocol);
            builder.AppendLine("长度：" + record.PacketLength + " 字节");
            builder.AppendLine("校验：" + record.ParseStatus);
            builder.AppendLine("字段摘要：");

            string detail = string.IsNullOrEmpty(record.Detail) ? "无" : record.Detail;
            string[] fields = detail.Replace("；", "\n").Replace("，", "\n").Split('\n');
            foreach (string field in fields)
                builder.AppendLine("  - " + field);
            return builder.ToString().TrimEnd();
        }

        private static string GetModuleName(int module)
        {
            if (module == 1) return "ClientGT 网关/登录";
            if (module == 2) return "ClientPF 玩家功能";
            return "未注册模块";
        }
    }
}
