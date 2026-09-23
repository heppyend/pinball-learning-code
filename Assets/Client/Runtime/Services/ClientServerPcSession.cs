// ---------------------------------------------------------------------------
// 平台边界（2026-09-20）：**WebGL / 微信小游戏下整段排除 PC 网络实现**。
//
// 为什么要留一个同名空壳：本组件**挂在 ClientShell 场景里**，
// 若整体编译掉，WebGL 包里那条场景引用会变成 "Missing Script"。
// 所以 WebGL 下保留一个空 MonoBehaviour 占位，PC 代码（TcpClient / System.Net.Sockets /
// Task.Run / 网关反射发现）**完全不参与编译** ⇒ 小游戏包里不会带上 PC 网络栈。
//
// 小游戏侧的网络走 `ClientTcpConnectionProbe`（`WXBase.CreateTCPSocket`，见 TCP_WEBGL_HANDOFF.md）。
// ---------------------------------------------------------------------------
#if UNITY_WEBGL && !UNITY_EDITOR

namespace Pinball.Client
{
    /// <summary>WebGL / 微信小游戏下的空壳（仅为保住场景引用，不承载任何逻辑）。</summary>
    public sealed class ClientServerPcSession : UnityEngine.MonoBehaviour
    {
    }
}

#else
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Pinball.Client.Services;
using UnityEngine;

namespace Pinball.Client
{
    /// <summary>
    /// PC development transport for ClientShell. It uses the existing gateway
    /// discovery entry point through reflection, then performs the verified
    /// GT handshake/login and PF GetHero exchange over TcpClient.
    /// </summary>
    public sealed class ClientServerPcSession : MonoBehaviour
    {
        private const string DeviceIdentifier = "WebGLMiniGameDeviceIdentifierPinballNode";
        private const int MaxPacketLength = 1024 * 1024;
        private const float GatewayTimeoutSeconds = 15f;

        [SerializeField] private bool runOnStart;
        [SerializeField] private bool allowPcServerConnection;
        [SerializeField] private bool useGateway = true;
        [SerializeField] private string directHost = string.Empty;
        [SerializeField] private int directPort;
        [SerializeField] private bool captureRawPackets = true;

        private readonly Queue<byte[]> receivedPackets = new Queue<byte[]>();
        private readonly List<byte> receiveBuffer = new List<byte>();
        private ClientServerPacketRouter packetRouter;
        private TcpClient client;
        private NetworkStream stream;
        private Coroutine sessionRoutine;
        private bool stopping;

        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("[ClientServerPC] WebGL/微信平台使用 ClientTcpConnectionProbe；PC 适配器不启动");
#else
            if (runOnStart && allowPcServerConnection)
                StartSession();
            else if (runOnStart)
                Debug.Log("[ClientServerPC] 客户端开发模式：已屏蔽 PC 自动服务器连接");
#endif
        }

        [ContextMenu("Start Client Server Session")]
        public void StartSession()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("[ClientServerPC] 当前为 WebGL/微信平台，请使用 ClientTcpConnectionProbe");
            return;
#else
            if (!allowPcServerConnection)
            {
                Debug.Log("[ClientServerPC] 客户端开发模式：PC 服务器连接已屏蔽");
                return;
            }
            StopSession();
            stopping = false;
            packetRouter = new ClientServerPacketRouter(captureRawPackets);
            sessionRoutine = StartCoroutine(RunSession());
#endif
        }

        private IEnumerator RunSession()
        {
            string host = directHost;
            int port = directPort;

            if (useGateway)
            {
                bool gatewayFinished = false;
                ResolveGateway(() => gatewayFinished = true);
                float gatewayDeadline = Time.realtimeSinceStartup + GatewayTimeoutSeconds;
                while (!gatewayFinished && !stopping && Time.realtimeSinceStartup < gatewayDeadline)
                    yield return null;

                if (!gatewayFinished && !stopping)
                {
                    Fail("网关发现超时（" + GatewayTimeoutSeconds + " 秒）；可关闭 useGateway 后配置临时 directHost/directPort");
                    yield break;
                }

                Type sysDefines = FindType("SysDefines");
                host = ReadStaticField(sysDefines, "Ip", string.Empty);
                long configuredPort = ReadStaticField(sysDefines, "Port", 0L);
                port = configuredPort >= 1 && configuredPort <= int.MaxValue ? (int)configuredPort : 0;
            }

            if (stopping)
                yield break;
            if (string.IsNullOrWhiteSpace(host) || port <= 0)
            {
                Fail("PC 会话地址无效：请检查网关或 ClientServerPcSession 的 directHost/directPort");
                yield break;
            }

            Debug.Log("[ClientServerPC] 开始 TCP 会话：地址=" + host + ":" + port);
            var connectTask = System.Threading.Tasks.Task.Run(() => ConnectAndRead(host, port));
            while (!connectTask.IsCompleted && !stopping)
            {
                DrainPackets();
                yield return null;
            }

            DrainPackets();
            if (connectTask.IsFaulted && !stopping)
                Fail("PC TCP 会话异常：" + connectTask.Exception.GetBaseException().Message);
        }

        private void ResolveGateway(Action completed)
        {
            Type netType = FindType("NetController");
            PropertyInfo instanceProperty = netType?.GetProperty(
                "Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            MethodInfo getIpPort = netType?.GetMethod("GetIpPort", BindingFlags.Public | BindingFlags.Instance);
            object instance = instanceProperty?.GetValue(null, null);

            if (instance == null || getIpPort == null)
            {
                Fail("Client 场景未找到可用的 NetController.GetIpPort；可关闭 useGateway 后配置临时 directHost/directPort");
                completed?.Invoke();
                return;
            }

            try
            {
                getIpPort.Invoke(instance, new object[] { completed });
                Debug.Log("[ClientServerPC] 已请求现有网关发现接口");
            }
            catch (Exception exception)
            {
                Fail("调用网关发现失败：" + exception.Message);
                completed?.Invoke();
            }
        }

        private void ConnectAndRead(string host, int port)
        {
            try
            {
                client = new TcpClient();
                client.Connect(host, port);
                stream = client.GetStream();
                Debug.Log("[ClientServerPC] TCP 建连成功");

                SendPacket(1, 0, writer =>
                {
                    writer.Write(6u);
                    writer.Write(1u);
                    writer.Write(ReadStaticField(FindType("SysDefines"), "Version", 7u));
                    WriteString(writer, DeviceIdentifier, 64);
                    WriteString(writer, "com.game.fishing.android", 32);
                    WriteString(writer, "ZH-CN", 16);
                    WriteString(writer, "CN", 16);
                }, "握手");

                byte[] buffer = new byte[8192];
                while (!stopping && stream != null)
                {
                    int count = stream.Read(buffer, 0, buffer.Length);
                    if (count <= 0)
                        break;

                    byte[] chunk = new byte[count];
                    Buffer.BlockCopy(buffer, 0, chunk, 0, count);
                    lock (receivedPackets)
                        receivedPackets.Enqueue(chunk);
                }
            }
            catch (Exception exception)
            {
                if (!stopping)
                    Debug.LogError("[ClientServerPC] TCP 读取失败：" + exception.Message);
            }
        }

        private void DrainPackets()
        {
            lock (receivedPackets)
            {
                while (receivedPackets.Count > 0)
                    receiveBuffer.AddRange(receivedPackets.Dequeue());
            }

            while (receiveBuffer.Count >= 4)
            {
                int packetLength = BitConverter.ToInt32(receiveBuffer.ToArray(), 0);
                if (packetLength < 8 || packetLength > MaxPacketLength)
                {
                    Fail("收到无效包长度：" + packetLength);
                    return;
                }
                if (receiveBuffer.Count < packetLength)
                    return;

                byte[] packet = receiveBuffer.GetRange(0, packetLength).ToArray();
                receiveBuffer.RemoveRange(0, packetLength);
                try
                {
                    packetRouter.Accept(packet);
                    HandlePacket(packet);
                }
                catch (Exception exception)
                {
                    Fail("数据包分类失败：" + exception.Message);
                    return;
                }
            }
        }

        private void HandlePacket(byte[] packet)
        {
            ushort module = BitConverter.ToUInt16(packet, 4);
            ushort protocol = BitConverter.ToUInt16(packet, 6);
            using (var reader = new BinaryReader(new MemoryStream(packet)))
            {
                reader.ReadInt32();
                reader.ReadUInt16();
                reader.ReadUInt16();
                if (module == 1 && protocol == 1)
                {
                    if (reader.ReadSByte() != 0)
                    {
                        Fail("服务器拒绝握手");
                        return;
                    }
                    reader.ReadInt32();
                    int randomKey = reader.ReadInt32();
                    SendLogin(randomKey);
                }
                else if (module == 1 && protocol == 5)
                {
                    if (reader.ReadSByte() != 0)
                    {
                        Fail("服务器拒绝游客登录");
                        return;
                    }
                    Debug.Log("[ClientServerPC] 游客登录应答已收到，继续请求英雄数据");
                    SendPacket(2, 1, _ => { }, "CLPFGetHeroReq");
                }
                else if (module == 1 && protocol == 2)
                {
                    byte code = reader.ReadByte();
                    Debug.LogWarning("[ClientServerPC] 收到服务器断开通知：code=" + code +
                                     "，原因=" + ClientServerPacketRouter.DescribeClientGtDisconnectCode(code));
                }
            }
        }

        private void SendLogin(int randomKey)
        {
            Type sysDefines = FindType("SysDefines");
            string openinstall = ReadStaticField(sysDefines, "OpeninstallToken", string.Empty);
            string token = EncodeCa3(DeviceIdentifier + "," + openinstall, randomKey);
            SendPacket(1, 3, writer =>
            {
                writer.Write((byte)1);
                WriteString(writer, token, 256);
            }, "游客登录");
        }

        private void SendPacket(ushort module, ushort protocol, Action<BinaryWriter> body, string name)
        {
            if (stream == null)
                return;
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory))
            {
                writer.Write(0);
                writer.Write(module);
                writer.Write(protocol);
                body(writer);
                byte[] packet = memory.ToArray();
                Buffer.BlockCopy(BitConverter.GetBytes(packet.Length), 0, packet, 0, 4);
                packetRouter?.RecordOutgoing(packet);
                stream.Write(packet, 0, packet.Length);
                stream.Flush();
                Debug.Log("[ClientServerPC] 已发送" + name + "：模块=" + module + "，协议=" + protocol + "，字节=" + packet.Length);
            }
        }

        private void StopSession()
        {
            stopping = true;
            if (sessionRoutine != null)
                StopCoroutine(sessionRoutine);
            sessionRoutine = null;
            try { stream?.Close(); } catch { }
            try { client?.Close(); } catch { }
            stream = null;
            client = null;
            packetRouter?.Dispose();
            packetRouter = null;
            receiveBuffer.Clear();
            lock (receivedPackets) receivedPackets.Clear();
        }

        private void OnDestroy() { StopSession(); }

        private void Fail(string reason)
        {
            Debug.LogError("[ClientServerPC] 会话失败：" + reason);
            StopSession();
        }

        private static void WriteString(BinaryWriter writer, string value, int maxLength)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            writer.Write((ushort)(bytes.Length >= maxLength ? 0 : bytes.Length));
            if (bytes.Length < maxLength) writer.Write(bytes);
        }

        private static string EncodeCa3(string text, int randomKey)
        {
            byte[] content = Encoding.UTF8.GetBytes(text ?? string.Empty);
            byte[] buffer = new byte[content.Length + 4];
            Buffer.BlockCopy(BitConverter.GetBytes(randomKey), 0, buffer, 0, 4);
            Buffer.BlockCopy(content, 0, buffer, 4, content.Length);
            for (int i = 0; i < buffer.Length; i++)
            {
                randomKey = (randomKey * 12347 + 20809) % 65536;
                buffer[i] ^= (byte)(randomKey & 0xff);
            }
            return Convert.ToBase64String(buffer);
        }

        private static T ReadStaticField<T>(Type type, string fieldName, T fallback)
        {
            FieldInfo field = type?.GetField(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (field == null) return fallback;
            try { return (T)Convert.ChangeType(field.GetValue(null), typeof(T)); }
            catch { return fallback; }
        }

        private static Type FindType(string name)
        {
            return Type.GetType(name + ", HotFix") ?? Type.GetType(name + ", Assembly-CSharp") ?? Type.GetType(name);
        }
    }
}

#endif
