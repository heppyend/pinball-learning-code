using System;

/// <summary>
/// Unity-facing contract for an approved WeChat Mini Game runtime adapter.
/// SDK-specific JavaScript and platform code must stay behind this interface.
/// </summary>
public interface IWeChatMiniProgramBridge
{
    WeChatMiniProgramCapabilities Capabilities { get; }

    void RequestLogin(Action<WeChatMiniProgramOperationResult> completed);

    void Share(WeChatMiniProgramShareRequest request, Action<WeChatMiniProgramOperationResult> completed);
}

[Flags]
public enum WeChatMiniProgramCapabilities
{
    None = 0,
    Login = 1 << 0,
    Share = 1 << 1,
}

public enum WeChatMiniProgramOperationStatus
{
    Succeeded,
    Cancelled,
    Rejected,
    UnsupportedPlatform,
    Failed,
}

public sealed class WeChatMiniProgramOperationResult
{
    public WeChatMiniProgramOperationStatus Status { get; private set; }
    public string AuthorizationCode { get; private set; }
    public string ErrorCode { get; private set; }
    public string ErrorMessage { get; private set; }

    private WeChatMiniProgramOperationResult(
        WeChatMiniProgramOperationStatus status,
        string authorizationCode,
        string errorCode,
        string errorMessage)
    {
        Status = status;
        AuthorizationCode = authorizationCode;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static WeChatMiniProgramOperationResult Success(string authorizationCode)
    {
        return new WeChatMiniProgramOperationResult(
            WeChatMiniProgramOperationStatus.Succeeded,
            authorizationCode,
            null,
            null);
    }

    public static WeChatMiniProgramOperationResult FromStatus(
        WeChatMiniProgramOperationStatus status,
        string errorCode,
        string errorMessage)
    {
        return new WeChatMiniProgramOperationResult(status, null, errorCode, errorMessage);
    }
}

public sealed class WeChatMiniProgramShareRequest
{
    public string Title { get; private set; }
    public string Query { get; private set; }

    public WeChatMiniProgramShareRequest(string title, string query)
    {
        Title = title;
        Query = query;
    }
}
