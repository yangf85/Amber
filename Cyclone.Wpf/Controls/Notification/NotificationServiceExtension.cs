using System;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 通知服务的便捷扩展方法。所有方法返回 <see cref="INotificationHandle"/>，
/// 可用于后续主动 Close / Update / 监听点击。
/// </summary>
public static class NotificationServiceExtension
{
    /// <summary>
    /// 显示一条带级别的文本通知。其它命名方法（Information / Success 等）都委托到这里。
    /// </summary>
    public static INotificationHandle Notify(this INotificationService self, string message,
        NotificationLevel level = NotificationLevel.Default)
    {
        if (self == null)
        {
            throw new ArgumentNullException(nameof(self));
        }

        var content = new NotificationMessage
        {
            Message = message,
            Level = level,
        };
        return self.Show(content);
    }

    /// <summary>显示默认级别（中性）通知。</summary>
    public static INotificationHandle Message(this INotificationService self, string message)
        => self.Notify(message, NotificationLevel.Default);

    /// <summary>显示信息级别（蓝色）通知。</summary>
    public static INotificationHandle Information(this INotificationService self, string message)
        => self.Notify(message, NotificationLevel.Information);

    /// <summary>显示成功级别（绿色）通知。</summary>
    public static INotificationHandle Success(this INotificationService self, string message)
        => self.Notify(message, NotificationLevel.Success);

    /// <summary>显示警告级别（橙色）通知。</summary>
    public static INotificationHandle Warning(this INotificationService self, string message)
        => self.Notify(message, NotificationLevel.Warning);

    /// <summary>显示错误级别（红色）通知。</summary>
    public static INotificationHandle Error(this INotificationService self, string message)
        => self.Notify(message, NotificationLevel.Error);
}
