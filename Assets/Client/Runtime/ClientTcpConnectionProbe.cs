using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using Pinball.Client.Services;

/// <summary>
/// 微信小游戏 TCP 最小会话探针：网关发现、ClientGT 握手/游客登录和 ClientPF 英雄数据。
/// 保持 Assembly-CSharp 到 HotFix 的纯反射边界，避免 WebGL UnityLinker 解析 HotFix 引用。
/// </summary>
public sealed class ClientTcpConnectionProbe : MonoBehaviour
{
    private const string FixedDeviceIdentifier = "WebGLMiniGameDeviceIdentifierPinballNode";
    private const uint WebGlPlatform = 6;
    private const uint ProductCode = 1;
    private const byte GuestLoginType = 1;
    private const string Channel = "com.game.fishing.android";
    private const string Country = "ZH-CN";
    private const string Language = "CN";
    private const int MaxPacketLength = 1024 * 1024;
    private const float HotFixWaitSeconds = 15f;
    private const float StageTimeoutSeconds = 15f;

    [SerializeField] private bool captureServerPackets = true;

    // ------------------------------------------------------------------
    // 以下 [SerializeField] 字段**必须声明在 `#if UNITY_WEBGL` 之外**。
    //
    // 原因（2026-09-22 实测，代价是多次构建失败）：Unity 打包 AssetBundle 时会校验
    // 「被序列化的脚本类在编辑器侧与播放器侧的字段布局是否一致」。若这些字段只在
    // `UNITY_WEBGL && !UNITY_EDITOR` 下存在，则 Windows 编辑器看不到它们、WebGL 播放器看得到，
    // Unity 直接拒绝构建：
    //     Error building player because script class layout is incompatible between
    //     the editor and the player.
    // 并把本类的字段清单一并打印出来。
    //
    // 字段本身不依赖任何 WebGL 专属类型，因此放在外面是安全的；真正需要平台隔离的
    // 是使用它们的逻辑（见下方 `#if UNITY_WEBGL` 的会话状态机与方法）。
    // ------------------------------------------------------------------
    [SerializeField] private bool runOnStart;
    [SerializeField] private bool logDeviceIdentifier;
    [SerializeField] private bool logFullDeviceIdentifier;
    [SerializeField] private bool useFallbackDeviceIdentifierForLogin;
    [SerializeField] private bool useDirectLocalTest;
    [SerializeField] private string directTestHost = "";
    [SerializeField] private int directTestPort = 1;
    [SerializeField] private bool useRealGuestLogin = true;

    // 兼容既有 Boot 场景序列化；本次会话统一使用固定超时和固定 device，不读取这些旧开关。
    [SerializeField] private float hotFixReadyTimeoutSeconds = 15f;
    [SerializeField] private float loginResponseTimeoutSeconds = 8f;

    private ClientServerPacketRouter packetRouter;

#if UNITY_WEBGL && !UNITY_EDITOR
    private enum SessionStage
    {
        None,
        WaitingHotFix,
        ResolvingGateway,
        Connecting,
        Handshake,
        GuestLogin,
        GetHero,
        SuccessClosing,
        Closed,
    }

    private WeChatWASM.WXTCPSocket socket;
    private readonly List<byte> receiveBuffer = new List<byte>();
    private Coroutine sessionCoroutine;
    private Coroutine stageTimeoutCoroutine;
    private SessionStage stage = SessionStage.None;
    private ushort currentModule;
    private ushort currentProtocol;
    private bool connected;
    private bool gatewayCallbackReceived;
    private bool loginAckReceived;
    private bool heroAckReceived;
    private bool heroDataReceived;
    private bool failureLogged;
    private bool closeRequested;
    private bool sessionFinished;
#endif

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WeChatWASM.WX.InitSDK(_ => { });
        if (runOnStart)
            StartPersistentProbe();
#endif
    }

    [ContextMenu("Start TCP Client Session")]
    public void StartProbe()
    {
        DisposePacketRouter();
        packetRouter = new ClientServerPacketRouter(captureServerPackets);
#if UNITY_WEBGL && !UNITY_EDITOR
        if (sessionCoroutine != null)
            StopCoroutine(sessionCoroutine);

        ResetSession();
        sessionCoroutine = StartCoroutine(StartSessionRoutine());
#else
        Debug.Log("[TCP客户端] PC 端已准备数据包记录器；当前传输适配器尚未启动");
        Debug.Log("[ClientServerData] 请通过 PC 传输适配器或数据包回放器注入原始包");
#endif
    }

    private void OnDestroy()
    {
        DisposePacketRouter();
    }

    private void DisposePacketRouter()
    {
        if (packetRouter == null)
            return;

        packetRouter.Dispose();
        packetRouter = null;
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    /// <summary>
    /// Boot 会以 Single 模式切换到 LoginScene。探针不能附着在 Boot 的 Init 对象上继续会话，
    /// 否则握手回包恰好到达时会被场景卸载中断。将实际会话迁移到独立的跨场景对象。
    /// </summary>
    private void StartPersistentProbe()
    {
        var host = new GameObject("WebGLTcpSessionProbe");
        DontDestroyOnLoad(host);

        var persistentProbe = host.AddComponent<ClientTcpConnectionProbe>();
        persistentProbe.runOnStart = false;
        persistentProbe.useDirectLocalTest = useDirectLocalTest;
        persistentProbe.directTestHost = directTestHost;
        persistentProbe.directTestPort = directTestPort;
        persistentProbe.useRealGuestLogin = useRealGuestLogin;
        persistentProbe.hotFixReadyTimeoutSeconds = hotFixReadyTimeoutSeconds;
        persistentProbe.loginResponseTimeoutSeconds = loginResponseTimeoutSeconds;
        persistentProbe.logDeviceIdentifier = logDeviceIdentifier;
        persistentProbe.logFullDeviceIdentifier = logFullDeviceIdentifier;
        persistentProbe.useFallbackDeviceIdentifierForLogin = useFallbackDeviceIdentifierForLogin;

        Debug.Log("[TCP客户端] 已迁移到跨场景会话对象，等待 Boot 场景切换不再中断 TCP 流程");
        persistentProbe.StartProbe();
        Destroy(this);
    }

    private void ResetSession()
    {
        StopStageTimeout();
        receiveBuffer.Clear();
        stage = SessionStage.None;
        currentModule = 0;
        currentProtocol = 0;
        connected = false;
        gatewayCallbackReceived = false;
        loginAckReceived = false;
        heroAckReceived = false;
        heroDataReceived = false;
        failureLogged = false;
        closeRequested = false;
        sessionFinished = false;
        socket = null;
    }

    private IEnumerator StartSessionRoutine()
    {
        Debug.Log("[TCP客户端] 会话开始：device=固定纯英文字母，长度=40，模块/协议=0/0，字节=0，结果=开始");
        if (!useRealGuestLogin)
        {
            FailSession("useRealGuestLogin 已关闭，未执行真实握手/游客登录");
            yield break;
        }

        SetStage(SessionStage.WaitingHotFix, 0, 0);
        if (useDirectLocalTest)
        {
            Debug.LogWarning("[TCP客户端] 使用本地直连测试参数；正式最小会话仍应使用 HTTP 网关");
            ConnectToEndpoint(directTestHost, directTestPort);
            yield break;
        }

        float deadline = Time.realtimeSinceStartup + HotFixWaitSeconds;
        while (!sessionFinished && Time.realtimeSinceStartup < deadline)
        {
            Type netControllerType = FindType("NetController");
            PropertyInfo instanceProperty = netControllerType == null
                ? null
                : netControllerType.GetProperty(
                    "Instance",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            MethodInfo getIpPortMethod = netControllerType == null
                ? null
                : netControllerType.GetMethod("GetIpPort", BindingFlags.Public | BindingFlags.Instance);
            object instance = instanceProperty == null ? null : instanceProperty.GetValue(null, null);

            if (instance != null && getIpPortMethod != null)
            {
                SetStage(SessionStage.ResolvingGateway, 0, 0);
                try
                {
                    getIpPortMethod.Invoke(instance, new object[] { (Action)ConnectToGateway });
                    yield break;
                }
                catch (Exception exception)
                {
                    FailSession("调用 NetController.GetIpPort 失败：" + exception.Message);
                    yield break;
                }
            }

            Debug.Log("[TCP客户端] 等待 HotFix 与 NetController 初始化");
            yield return new WaitForSecondsRealtime(1f);
        }

        if (!sessionFinished)
            FailSession("等待 HotFix/NetController 超时");
    }

    private void ConnectToGateway()
    {
        if (sessionFinished || gatewayCallbackReceived)
            return;

        gatewayCallbackReceived = true;
        Type sysDefinesType = FindType("SysDefines");
        string host = ReadStaticField(sysDefinesType, "Ip", string.Empty);
        long configuredPort = ReadStaticField(sysDefinesType, "Port", 0L);
        int port = configuredPort >= 1 && configuredPort <= int.MaxValue
            ? (int)configuredPort
            : 0;

        Debug.Log($"[TCP客户端] 网关成功：IP={host}, Port={port}，模块/协议=HTTP/网关，字节=0，结果=成功");
        ConnectToEndpoint(host, port);
    }

    private void ConnectToEndpoint(string host, int port)
    {
        if (sessionFinished)
            return;

        if (string.IsNullOrWhiteSpace(host) || port <= 0)
        {
            FailSession("网关 IP/端口无效");
            return;
        }

        SetStage(SessionStage.Connecting, 0, 0);
        Debug.Log($"[TCP客户端] TCP 连接目标：IP={host}, Port={port}");
        try
        {
            socket = WeChatWASM.WXBase.CreateTCPSocket();
            socket.OnConnect(OnConnected);
            socket.OnError(OnError);
            socket.OnClose(OnClosed);
            socket.OnMessage(OnMessage, true);
            socket.Connect(new WeChatWASM.TCPSocketConnectOption
            {
                address = host,
                port = port,
                timeout = 5000,
            });
        }
        catch (Exception exception)
        {
            FailSession("创建或连接 TCP Socket 失败：" + exception.Message);
        }
    }

    private void OnConnected(WeChatWASM.GeneralCallbackResult _)
    {
        if (sessionFinished)
            return;

        connected = true;
        Debug.Log("[TCP客户端] TCP 建连成功：模块/协议=TCP/Connect，字节=0，结果=成功");
        SetStage(SessionStage.Handshake, 1, 0);
        SendHandshake();
    }

    private void SendHandshake()
    {
        uint version = ReadStaticField(FindType("SysDefines"), "Version", 7u);
        SendPacket(1, 0, writer =>
        {
            writer.Write(WebGlPlatform);
            writer.Write(ProductCode);
            writer.Write(version);
            WriteProtocolString(writer, FixedDeviceIdentifier, 64);
            WriteProtocolString(writer, Channel, 32);
            WriteProtocolString(writer, Country, 16);
            WriteProtocolString(writer, Language, 16);
        }, "握手");
        Debug.Log($"[TCP客户端] 会话设备：固定纯英文 device，长度={FixedDeviceIdentifier.Length}");
    }

    private void OnError(WeChatWASM.GeneralCallbackResult result)
    {
        string sdkError = result == null ? "未知 SDK 错误" : result.errMsg;
        FailSession("TCP API 失败：" + (string.IsNullOrEmpty(sdkError) ? "未知 SDK 错误" : sdkError));
    }

    private void OnClosed(WeChatWASM.GeneralCallbackResult result)
    {
        connected = false;
        StopStageTimeout();

        if (heroDataReceived && closeRequested && !failureLogged)
        {
            sessionFinished = true;
            stage = SessionStage.Closed;
            Debug.Log("[TCP客户端] 成功退出 TCP：原因=已成功接收英雄数据");
            return;
        }

        string sdkReason = result == null ? string.Empty : result.errMsg;
        if (!failureLogged)
        {
            failureLogged = true;
            Debug.LogError($"[TCP客户端] 失败退出 TCP：原因={(string.IsNullOrEmpty(sdkReason) ? "服务器主动断开或连接非预期关闭" : "服务器主动断开或连接非预期关闭：" + sdkReason)}，阶段={stage}，模块={currentModule}，协议={currentProtocol}，收到登录回包={loginAckReceived}，收到英雄回包={heroAckReceived}");
        }

        sessionFinished = true;
        stage = SessionStage.Closed;
        Debug.LogWarning($"[TCP客户端] TCP 连接已关闭：阶段={stage}，收到登录回包={loginAckReceived}，收到英雄回包={heroAckReceived}");
    }

    private void OnMessage(WeChatWASM.TCPSocketOnMessageListenerResult result)
    {
        if (sessionFinished || failureLogged)
            return;

        if (result == null)
        {
            FailSession("TCP 消息回调结果为空");
            return;
        }

        byte[] data = result.message as byte[];
        if (data == null)
        {
            FailSession("TCP 消息不是 byte[]，无法解析");
            return;
        }

        if (data.Length == 0)
            return;

        receiveBuffer.AddRange(data);
        Debug.Log($"[TCP客户端] 收到 TCP 数据：字节={data.Length}，阶段={stage}");

        while (!sessionFinished && !failureLogged && receiveBuffer.Count >= 4)
        {
            int packetLength = BitConverter.ToInt32(receiveBuffer.ToArray(), 0);
            if (packetLength < 8 || packetLength > MaxPacketLength)
            {
                FailSession("收到无效包长度：" + packetLength);
                return;
            }

            if (receiveBuffer.Count < packetLength)
                return;

            byte[] packet = receiveBuffer.GetRange(0, packetLength).ToArray();
            receiveBuffer.RemoveRange(0, packetLength);
            if (packetRouter != null)
                packetRouter.Accept(packet);
            ParsePacket(packet);
        }
    }

    private void ParsePacket(byte[] packet)
    {
        try
        {
            using (var reader = new BinaryReader(new MemoryStream(packet)))
            {
                int declaredLength = reader.ReadInt32();
                ushort module = reader.ReadUInt16();
                ushort protocol = reader.ReadUInt16();
                currentModule = module;
                currentProtocol = protocol;
                Debug.Log($"[TCP客户端] 收到 TCP 包：模块={module}，协议={protocol}，字节={packet.Length}");

                if (declaredLength != packet.Length)
                    throw new InvalidDataException("包长度字段不匹配：" + declaredLength);

                if (module == 1 && protocol == 1)
                {
                    ParseHandshakeAck(reader, packet.Length);
                    return;
                }

                if (module == 1 && protocol == 5)
                {
                    ParseLoginAck(reader, packet.Length);
                    return;
                }

                if (module == 1 && protocol == 2)
                {
                    ParseDisconnectNotification(reader, packet.Length);
                    return;
                }

                if (module == 2 && protocol == 2)
                {
                    ParseGetHeroAck(reader, packet.Length);
                    return;
                }

                FailSession($"收到非预期协议：模块={module}，协议={protocol}，当前阶段={stage}");
            }
        }
        catch (Exception exception)
        {
            FailSession($"协议解析失败：模块={currentModule}，协议={currentProtocol}，原因={exception.Message}");
        }
    }

    private void ParseHandshakeAck(BinaryReader reader, int packetLength)
    {
        SetStage(SessionStage.Handshake, 1, 1);
        EnsureRemaining(reader, 9, "握手应答");
        sbyte errorCode = reader.ReadSByte();
        reader.ReadInt32();
        int randomKey = reader.ReadInt32();
        EnsurePacketEnd(reader, packetLength, "握手应答");

        if (errorCode != 0)
        {
            FailSession($"握手被服务器拒绝：errcode={errorCode}");
            return;
        }

        Debug.Log($"[TCP客户端] 握手成功：模块=1，协议=1，字节={packetLength}，结果=成功");
        SetStage(SessionStage.GuestLogin, 1, 3);
        SendGuestLogin(randomKey);
    }

    private void SendGuestLogin(int randomKey)
    {
        Type sysDefinesType = FindType("SysDefines");
        string openinstallToken = ReadStaticField(sysDefinesType, "OpeninstallToken", string.Empty);
        string token = EncodeCa3(FixedDeviceIdentifier + "," + openinstallToken, randomKey);
        SendPacket(1, 3, writer =>
        {
            writer.Write(GuestLoginType);
            WriteProtocolString(writer, token, 256);
        }, "游客登录");
    }

        private void ParseLoginAck(BinaryReader reader, int packetLength)
    {
        SetStage(SessionStage.GuestLogin, 1, 5);
        loginAckReceived = true;
        EnsureRemaining(reader, 1, "游客登录应答");
        sbyte errorCode = reader.ReadSByte();
        if (errorCode != 0)
        {
            FailSession($"游客登录被服务器拒绝：errcode={errorCode}");
            return;
        }

        // 与新版 ClientGT.CLGTLoginAck.fromBinary 保持同一字段顺序。
        // 头像、头像框、称号、徽章、铭牌均为 Int32；不记录这些值，避免诊断日志泄露账号画像。
        reader.ReadInt32();
        ReadProtocolString(reader, 32, "nickname");
        reader.ReadSByte();
        reader.ReadInt32();
        reader.ReadInt32(); // head
        reader.ReadInt32(); // head_frame
        reader.ReadInt32(); // title
        reader.ReadInt32(); // badge
        reader.ReadInt32(); // nameplate
        ReadProtocolString(reader, 20, "phone");
        reader.ReadInt64();
        int itemLength = reader.ReadInt32();
        EnsureCount(itemLength, 100, "登录物品数组");
        for (int i = 0; i < itemLength; i++)
        {
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt64();
        }
        reader.ReadUInt32();
        ReadProtocolString(reader, 4096, "extra_params");
        EnsurePacketEnd(reader, packetLength, "游客登录应答");

        Debug.Log($"[TCP客户端] 游客登录成功：模块=1，协议=5，字节={packetLength}，结果=成功");
        SetStage(SessionStage.GetHero, 2, 1);
        StartCoroutine(SendGetHeroNextFrame());
    }

    // 服务器断开通知（ClientGT 模块 1 / 协议 2）。
    // 原先被误放在 ParseLoginAck 方法体内部，属非法 C#（CS0106）；Editor 编译因
    // #if UNITY_WEBGL && !UNITY_EDITOR 排除该区域而掩盖了它，WebGL 构建必然失败。
    // 此处仅把它移出为同级方法，字段顺序与判定逻辑完全未改。
        private void ParseDisconnectNotification(BinaryReader reader, int packetLength)
        {
            EnsureRemaining(reader, 1, "服务器断开通知");
            byte code = reader.ReadByte();
            EnsurePacketEnd(reader, packetLength, "服务器断开通知");
            string reason = ClientServerPacketRouter.DescribeClientGtDisconnectCode(code);
            Debug.LogWarning($"[TCP客户端] 收到服务器断开通知：模块=1，协议=2，断开码={code}，原因={reason}");
            if (!heroDataReceived)
                FailSession($"服务器断开连接：code={code}，原因={reason}");
        }

    private IEnumerator SendGetHeroNextFrame()
    {
        yield return null;
        if (!sessionFinished && !failureLogged && connected && stage == SessionStage.GetHero)
            SendPacket(2, 1, _ => { }, "CLPFGetHeroReq");
    }

    private void ParseGetHeroAck(BinaryReader reader, int packetLength)
    {
        SetStage(SessionStage.GetHero, 2, 2);
        heroAckReceived = true;
        int heroLength = reader.ReadInt32();
        EnsureCount(heroLength, 100, "英雄数组");
        for (int i = 0; i < heroLength; i++)
        {
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt64();
            reader.ReadInt64();
            reader.ReadInt64();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt64();
        }

        int heroTeamLength = reader.ReadInt32();
        EnsureCount(heroTeamLength, 10, "英雄组数组");
        for (int i = 0; i < heroTeamLength; i++)
        {
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt32();
            reader.ReadInt64();
        }
        EnsurePacketEnd(reader, packetLength, "CLPFGetHeroAck");

        heroDataReceived = true;
        StopStageTimeout();
        closeRequested = true;
        SetStage(SessionStage.SuccessClosing, 2, 2);
        Debug.Log($"[TCP客户端] 英雄数据接收并解析成功：模块=2，协议=2，字节={packetLength}，英雄={heroLength}，英雄组={heroTeamLength}，结果=成功");
        CloseSocket();
    }

    private void SendPacket(ushort module, ushort protocol, Action<BinaryWriter> body, string name)
    {
        if (!connected || socket == null)
        {
            FailSession($"发送{name}失败：TCP 尚未连接");
            return;
        }

        try
        {
            byte[] packet;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(0);
                writer.Write(module);
                writer.Write(protocol);
                body(writer);
                packet = stream.ToArray();
            }

            Buffer.BlockCopy(BitConverter.GetBytes(packet.Length), 0, packet, 0, 4);
            if (packetRouter != null)
                packetRouter.RecordOutgoing(packet);
            socket.Write(packet);
            Debug.Log($"[TCP客户端] 已发送{name}：模块={module}，协议={protocol}，字节={packet.Length}，结果=成功");
        }
        catch (Exception exception)
        {
            FailSession($"发送{name}失败：{exception.Message}");
        }
    }

    private void FailSession(string reason)
    {
        if (failureLogged || sessionFinished)
            return;

        failureLogged = true;
        StopStageTimeout();
        Debug.LogError($"[TCP客户端] 失败退出 TCP：原因={reason}，阶段={stage}，模块={currentModule}，协议={currentProtocol}，收到登录回包={loginAckReceived}，收到英雄回包={heroAckReceived}");
        CloseSocket();
        sessionFinished = true;
        stage = SessionStage.Closed;
    }

    private void CloseSocket()
    {
        if (socket == null)
            return;

        try
        {
            socket.Close();
        }
        catch (Exception exception)
        {
            if (!failureLogged)
            {
                failureLogged = true;
                Debug.LogError($"[TCP客户端] 失败退出 TCP：原因=客户端主动关闭 TCP 失败：{exception.Message}，阶段={stage}，模块={currentModule}，协议={currentProtocol}，收到登录回包={loginAckReceived}，收到英雄回包={heroAckReceived}");
            }
            if (heroDataReceived)
            {
                sessionFinished = true;
                stage = SessionStage.Closed;
            }
        }
    }

    private void SetStage(SessionStage nextStage, ushort module, ushort protocol)
    {
        StopStageTimeout();
        stage = nextStage;
        currentModule = module;
        currentProtocol = protocol;
        Debug.Log($"[TCP客户端] 阶段={nextStage}：模块={module}，协议={protocol}，字节=0，结果=开始");
        if (nextStage == SessionStage.Closed || nextStage == SessionStage.SuccessClosing)
            return;

        stageTimeoutCoroutine = StartCoroutine(StageTimeoutRoutine(nextStage));
    }

    private IEnumerator StageTimeoutRoutine(SessionStage expectedStage)
    {
        yield return new WaitForSecondsRealtime(StageTimeoutSeconds);
        if (!sessionFinished && !failureLogged && stage == expectedStage)
            FailSession($"客户端超时，阶段={expectedStage}，未收到预期回包");
    }

    private void StopStageTimeout()
    {
        if (stageTimeoutCoroutine == null)
            return;

        StopCoroutine(stageTimeoutCoroutine);
        stageTimeoutCoroutine = null;
    }

    private static void WriteProtocolString(BinaryWriter writer, string value, int maxLength)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        if (bytes.Length >= maxLength)
        {
            writer.Write((ushort)0);
            return;
        }

        writer.Write((ushort)bytes.Length);
        writer.Write(bytes);
    }

    private static string ReadProtocolString(BinaryReader reader, int maxLength, string fieldName)
    {
        ushort length = reader.ReadUInt16();
        if (length >= maxLength)
            throw new InvalidDataException(fieldName + " 字符串长度超限：" + length);

        byte[] bytes = reader.ReadBytes(length);
        if (bytes.Length != length)
            throw new EndOfStreamException(fieldName + " 字符串数据不完整");
        return Encoding.UTF8.GetString(bytes);
    }

    private static void EnsureRemaining(BinaryReader reader, int bytes, string packetName)
    {
        if (reader.BaseStream.Length - reader.BaseStream.Position < bytes)
            throw new EndOfStreamException(packetName + " 正文不完整");
    }

    private static void EnsurePacketEnd(BinaryReader reader, int packetLength, string packetName)
    {
        if (reader.BaseStream.Position != packetLength)
            throw new InvalidDataException(packetName + " 存在未解析正文：" + (packetLength - reader.BaseStream.Position));
    }

    private static void EnsureCount(int count, int max, string name)
    {
        if (count < 0 || count > max)
            throw new InvalidDataException(name + " 长度非法：" + count);
    }

    private static string EncodeCa3(string originContent, int randomKey)
    {
        byte[] content = Encoding.UTF8.GetBytes(originContent ?? string.Empty);
        byte[] buffer = new byte[content.Length + 4];
        Array.Copy(BitConverter.GetBytes(randomKey), 0, buffer, 0, 4);
        Array.Copy(content, 0, buffer, 4, content.Length);

        int a = 12347;
        int b = 20809;
        int c = 65536;
        for (int i = 0; i < buffer.Length; i++)
        {
            randomKey = (randomKey * a + b) % c;
            buffer[i] ^= (byte)(randomKey & 0xff);
        }
        return Convert.ToBase64String(buffer);
    }

    private static T ReadStaticField<T>(Type type, string fieldName, T defaultValue)
    {
        if (type == null)
            return defaultValue;

        FieldInfo field = type.GetField(
            fieldName,
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (field == null)
            return defaultValue;

        try
        {
            object value = field.GetValue(null);
            if (value == null)
                return defaultValue;
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    private static Type FindType(string name)
    {
        Type type = Type.GetType(name + ", HotFix") ?? Type.GetType(name + ", Assembly-CSharp") ?? Type.GetType(name);
        if (type != null)
            return type;

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            try
            {
                type = assemblies[i].GetType(name, false);
                if (type != null)
                    return type;
            }
            catch
            {
                // 某些动态程序集在枚举类型时可能失败，继续检查其他已加载程序集。
            }
        }
        return null;
    }
#endif
}
