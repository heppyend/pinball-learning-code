using System;

/// <summary>
/// Safe default used until an approved Mini Game SDK adapter is registered.
/// It never invokes a native SDK, JavaScript bridge, network request, or mock account flow.
/// </summary>
public sealed class UnavailableWeChatMiniProgramBridge : IWeChatMiniProgramBridge
{
    private const string UnsupportedMessage = "WeChat Mini Game runtime integration is not configured.";

    public WeChatMiniProgramCapabilities Capabilities
    {
        get { return WeChatMiniProgramCapabilities.None; }
    }

    public void RequestLogin(Action<WeChatMiniProgramOperationResult> completed)
    {
        CompleteUnsupported(completed);
    }

    public void Share(WeChatMiniProgramShareRequest request, Action<WeChatMiniProgramOperationResult> completed)
    {
        CompleteUnsupported(completed);
    }

    private static void CompleteUnsupported(Action<WeChatMiniProgramOperationResult> completed)
    {
        if (completed == null)
            return;

        completed(WeChatMiniProgramOperationResult.FromStatus(
            WeChatMiniProgramOperationStatus.UnsupportedPlatform,
            "WX_MINIGAME_NOT_CONFIGURED",
            UnsupportedMessage));
    }
}
