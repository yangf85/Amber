using System;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 单条通知的句柄。<see cref="INotificationService.Show"/> 返回此对象，
/// 让调用方主动关闭、更新内容、或挂载点击 / 关闭事件。
/// </summary>
public interface INotificationHandle
{
    /// <summary>通知是否已关闭。</summary>
    bool IsClosed { get; }

    /// <summary>关闭通知（带滑出动画）。重复调用幂等。</summary>
    void Close();

    /// <summary>
    /// 替换通知内容。如果原 Content 是 <see cref="NotificationMessage"/> 且传入字符串，
    /// 仅更新文本（不改 Level、不重置动画）；否则整体替换 Content。
    /// </summary>
    /// <param name="content">新的内容对象</param>
    void Update(object content);

    /// <summary>用户点击通知主体时触发（点关闭按钮或其他按钮不算）。</summary>
    event EventHandler Clicked;

    /// <summary>通知关闭后触发（关闭动画完成后）。</summary>
    event EventHandler Closed;
}
