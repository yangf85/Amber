using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 警告对话框的标准消息内容。通过 <see cref="Level"/> 切换颜色和图标。
/// 替代之前 6 个 AlertXxxMessage UserControl（Default / Question / Information / Success / Warning / Error）。
/// </summary>
public class AlertMessage : Control
{
    static AlertMessage()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AlertMessage),
            new FrameworkPropertyMetadata(typeof(AlertMessage)));
    }

    #region Level

    public static readonly DependencyProperty LevelProperty =
        DependencyProperty.Register(
            nameof(Level),
            typeof(AlertIcon),
            typeof(AlertMessage),
            new PropertyMetadata(AlertIcon.None));

    /// <summary>
    /// 获取或设置消息级别。决定图标和颜色。<see cref="AlertIcon.None"/> 时不显示图标。
    /// </summary>
    public AlertIcon Level
    {
        get => (AlertIcon)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    #endregion Level

    #region Message

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(object),
            typeof(AlertMessage),
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
