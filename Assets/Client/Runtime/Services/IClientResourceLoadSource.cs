using System;

namespace Pinball.Client.Services
{
    /// <summary>
    /// 页面加载进度的最小适配边界。真实 YooAsset/网络加载器只需转发进度事件即可。
    /// </summary>
    public interface IClientResourceLoadSource
    {
        event Action<float> ProgressChanged;
        float CurrentProgress { get; }
        bool IsCompleted { get; }
    }
}
