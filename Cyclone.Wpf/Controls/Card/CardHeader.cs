using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// <see cref="Card"/> 的标准业务头部：图标 + 标题 + 副标题 + 右侧操作。
/// <para>
/// 典型用法：
/// </para>
/// <code>
/// &lt;cy:Card&gt;
///     &lt;cy:Card.Header&gt;
///         &lt;cy:CardHeader Title="任务概览" Subtitle="数据同步状态总览"&gt;
///             &lt;cy:CardHeader.Icon&gt;...&lt;/cy:CardHeader.Icon&gt;
///             &lt;cy:CardHeader.Action&gt;
///                 &lt;Button Content="⋯"/&gt;
///             &lt;/cy:CardHeader.Action&gt;
///         &lt;/cy:CardHeader&gt;
///     &lt;/cy:Card.Header&gt;
/// &lt;/cy:Card&gt;
/// </code>
/// <para>
/// 四个槽位均支持独立 null 收起：模板里通过 Trigger 检测各自非空，
/// 让用户只填需要的部分（例如不要 Icon 时整个 Icon 列消失，不留空白槽）。
/// </para>
/// </summary>
public class CardHeader : Control
{
    static CardHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CardHeader),
            new FrameworkPropertyMetadata(typeof(CardHeader)));
    }

    #region Icon

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(CardHeader),
            new FrameworkPropertyMetadata(default(object)));

    /// <summary>左侧图标槽。常见用法是放一个有色 Border 包着 Path。</summary>
    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion Icon

    #region Title

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(CardHeader),
            new FrameworkPropertyMetadata(default(string)));

    /// <summary>主标题文字。</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    #endregion Title

    #region Subtitle

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(CardHeader),
            new FrameworkPropertyMetadata(default(string)));

    /// <summary>副标题文字。为 null / 空字符串时收起整行。</summary>
    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    #endregion Subtitle

    #region Action

    public static readonly DependencyProperty ActionProperty =
        DependencyProperty.Register(
            nameof(Action),
            typeof(object),
            typeof(CardHeader),
            new FrameworkPropertyMetadata(default(object)));

    /// <summary>右侧操作槽。常见用法是放一个 ⋯ 菜单按钮。</summary>
    public object Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }

    #endregion Action
}
