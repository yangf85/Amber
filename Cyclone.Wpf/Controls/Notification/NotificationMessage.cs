using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 单条通知消息控件。通过 <see cref="Level"/> 切换颜色和图标。
/// 替代之前的 5 个 NotificationXxxMessage UserControl（Default / Information / Success / Warning / Error）。
/// </summary>
public class NotificationMessage : Control
{
    static NotificationMessage()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationMessage),
            new FrameworkPropertyMetadata(typeof(NotificationMessage)));
    }

    #region Level

    public static readonly DependencyProperty LevelProperty =
        DependencyProperty.Register(
            nameof(Level),
            typeof(NotificationLevel),
            typeof(NotificationMessage),
            new PropertyMetadata(NotificationLevel.Default));

    /// <summary>
    /// 获取或设置通知级别。
    /// </summary>
    public NotificationLevel Level
    {
        get => (NotificationLevel)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    #endregion Level

    #region Message

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(object),
            typeof(NotificationMessage),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置消息内容（通常是 string，也可以是任意对象——经 ContentPresenter 渲染）。
    /// </summary>
    public object Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    #endregion Message
}
