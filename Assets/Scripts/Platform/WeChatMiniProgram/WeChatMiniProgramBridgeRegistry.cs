/// <summary>
/// Composition point for an approved WeChat Mini Game runtime adapter.
/// It defaults to an unavailable implementation so callers get deterministic behavior
/// before the converter SDK and platform credentials are approved.
/// </summary>
public static class WeChatMiniProgramBridgeRegistry
{
    private static IWeChatMiniProgramBridge _current = new UnavailableWeChatMiniProgramBridge();

    public static IWeChatMiniProgramBridge Current
    {
        get { return _current; }
    }

    public static void Register(IWeChatMiniProgramBridge bridge)
    {
        _current = bridge ?? new UnavailableWeChatMiniProgramBridge();
    }

    public static void ResetToUnavailable()
    {
        _current = new UnavailableWeChatMiniProgramBridge();
    }
}
