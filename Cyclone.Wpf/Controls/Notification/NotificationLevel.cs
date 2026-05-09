namespace Cyclone.Wpf.Controls;

/// <summary>
/// 通知消息级别——决定 <see cref="NotificationMessage"/> 的视觉样式。
/// </summary>
public enum NotificationLevel
{
    /// <summary>默认（中性色）</summary>
    Default,

    /// <summary>信息（蓝色）</summary>
    Information,

    /// <summary>成功（绿色）</summary>
    Success,

    /// <summary>警告（橙色）</summary>
    Warning,

    /// <summary>错误（红色）</summary>
    Error,
}
