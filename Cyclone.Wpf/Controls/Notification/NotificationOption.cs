using System;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 通知服务配置。
/// <para>
/// 注意：该类是 mutable POCO，<b>不实现</b> <see cref="System.ComponentModel.INotifyPropertyChanged"/>。
/// 推荐用法：在调用 <see cref="NotificationService"/> 任何方法之前完成所有配置；
/// 运行时若要变更（例如切换 Position），改完之后手动触发服务的相关方法（如重新调用 SetOwner）
/// 才会立即生效，否则只在下一条 Show 时生效。
/// </para>
/// </summary>
public class NotificationOption
{
    /// <summary>显示持续时间，&lt;= 0 表示不自动关闭。</summary>
    public TimeSpan DisplayDuration { get; set; } = TimeSpan.FromMilliseconds(2400);

    /// <summary>通知出现的位置（owner 或屏幕的某个角）。</summary>
    public NotificationPosition Position { get; set; } = NotificationPosition.BottomRight;

    /// <summary>X 轴偏移量（DIPs）。</summary>
    public double OffsetX { get; set; } = 5;

    /// <summary>Y 轴偏移量（DIPs）。</summary>
    public double OffsetY { get; set; } = 5;

    /// <summary>通知之间的间距（DIPs）。</summary>
    public double Spacing { get; set; } = 5;

    /// <summary>最多同时显示多少条通知，超过时最早的会被淘汰。</summary>
    public int MaxCount { get; set; } = 5;

    /// <summary>通知宽度（DIPs）。</summary>
    public double Width { get; set; } = 300;

    /// <summary>通知最大高度（DIPs）。Auto-size 时实际高度可能小于此值。</summary>
    public double Height { get; set; } = 75;

    /// <summary>是否显示右上角关闭按钮。</summary>
    public bool IsShowCloseButton { get; set; } = true;
}
