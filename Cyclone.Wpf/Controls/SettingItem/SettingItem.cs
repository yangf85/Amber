using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// SettingItem 控件：用于设置页 / 偏好面板的"标签 + 描述 + 操作控件"语义化单元。
/// 单独使用即可，无需放入特定容器；放入任意祖先容器（如 SettingGroup、StackPanel）后，
/// 容器上设置的 <see cref="HeaderWidthProperty"/> 会通过属性继承自动下传，实现批量对齐。
/// </summary>
[TemplatePart(Name = nameof(PART_RootBorder), Type = typeof(Border))]
public class SettingItem : ContentControl
{
    private const string PART_RootBorder = "PART_RootBorder";

    private Border _rootBorder;

    static SettingItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SettingItem),
            new FrameworkPropertyMetadata(typeof(SettingItem)));
    }

    #region Icon

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(default(object)));

    /// <summary>
    /// 获取或设置左侧图标，可以是字符串、Path、IconBox 或任意可视化内容。
    /// 未设置时该列自动收起，不占用布局空间。
    /// </summary>
    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion Icon

    #region Header

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(default(object)));

    /// <summary>
    /// 获取或设置主标题，渲染在标题列。
    /// </summary>
    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    #endregion Header

    #region Description

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(default(string)));

    /// <summary>
    /// 获取或设置主标题下方的描述文本。空字符串或 null 时该行自动收起。
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    #endregion Description

    #region HeaderWidth (附加属性 + Inherits + CLR 实例包装)

    public static readonly DependencyProperty HeaderWidthProperty =
        DependencyProperty.RegisterAttached(
            "HeaderWidth",
            typeof(GridLength),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(
                GridLength.Auto,
                FrameworkPropertyMetadataOptions.Inherits |
                FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// 获取或设置标题列宽度。可在任意祖先容器上批量设置（通过 Inherits 自动下传给所有后代 SettingItem），
    /// 也可在单个 SettingItem 上覆盖。默认 <see cref="GridLength.Auto"/>，即按内容自适应。
    /// </summary>
    public GridLength HeaderWidth
    {
        get => GetHeaderWidth(this);
        set => SetHeaderWidth(this, value);
    }

    public static GridLength GetHeaderWidth(DependencyObject obj)
    {
        return (GridLength)obj.GetValue(HeaderWidthProperty);
    }

    public static void SetHeaderWidth(DependencyObject obj, GridLength value)
    {
        obj.SetValue(HeaderWidthProperty, value);
    }

    #endregion HeaderWidth (附加属性 + Inherits + CLR 实例包装)

    #region ContentAlignment (附加属性 + Inherits + CLR 实例包装)

    public static readonly DependencyProperty ContentAlignmentProperty =
        DependencyProperty.RegisterAttached(
            "ContentAlignment",
            typeof(HorizontalAlignment),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(
                HorizontalAlignment.Right,
                FrameworkPropertyMetadataOptions.Inherits |
                FrameworkPropertyMetadataOptions.AffectsArrange,
                OnContentAlignmentChanged));

    /// <summary>
    /// 获取或设置 Content 在剩余空间内的水平对齐方式。
    /// 可在任意祖先容器上批量设置（通过 Inherits 自动下传给所有后代 SettingItem），
    /// 也可在单个 SettingItem 上覆盖。默认 <see cref="HorizontalAlignment.Right"/>。
    /// </summary>
    public HorizontalAlignment ContentAlignment
    {
        get => GetContentAlignment(this);
        set => SetContentAlignment(this, value);
    }

    /// <summary>
    /// 当 ContentAlignment 通过 Inherits 或直接赋值发生变化时，
    /// 同步到 SettingItem 的 HorizontalContentAlignment 实例属性。
    /// 这样模板里可以用最稳定的 TemplateBinding HorizontalContentAlignment，
    /// 避免"模板内 Binding 附加属性"在某些场景下不可靠的问题。
    /// </summary>
    private static void OnContentAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SettingItem item)
        {
            item.SetValue(HorizontalContentAlignmentProperty, (HorizontalAlignment)e.NewValue);
        }
    }

    public static HorizontalAlignment GetContentAlignment(DependencyObject obj)
    {
        return (HorizontalAlignment)obj.GetValue(ContentAlignmentProperty);
    }

    public static void SetContentAlignment(DependencyObject obj, HorizontalAlignment value)
    {
        obj.SetValue(ContentAlignmentProperty, value);
    }

    #endregion ContentAlignment (附加属性 + Inherits + CLR 实例包装)

    #region IsClickable

    public static readonly DependencyProperty IsClickableProperty =
        DependencyProperty.Register(
            nameof(IsClickable),
            typeof(bool),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// 获取或设置整行是否可点击。为 true 时显示 Hover 反馈、鼠标变手型，并响应 <see cref="ClickEvent"/>
    /// 与 <see cref="Command"/>。子控件（如 Button、ComboBox）的点击因事件已被标记 Handled，不会冒泡触发本行点击。
    /// </summary>
    public bool IsClickable
    {
        get => (bool)GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    #endregion IsClickable

    #region Command

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(default(ICommand)));

    /// <summary>
    /// 获取或设置整行点击时执行的命令。仅 <see cref="IsClickable"/> 为 true 时生效。
    /// </summary>
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    #endregion Command

    #region CommandParameter

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(
            nameof(CommandParameter),
            typeof(object),
            typeof(SettingItem),
            new FrameworkPropertyMetadata(default(object)));

    /// <summary>
    /// 获取或设置传递给 <see cref="Command"/> 的参数。
    /// </summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    #endregion CommandParameter

    #region RoutedEvents

    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Click),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SettingItem));

    /// <summary>
    /// 整行被点击时触发。仅 <see cref="IsClickable"/> 为 true 且事件未被子控件标记 Handled 时触发。
    /// </summary>
    public event RoutedEventHandler Click
    {
        add { AddHandler(ClickEvent, value); }
        remove { RemoveHandler(ClickEvent, value); }
    }

    #endregion RoutedEvents

    #region Override Methods

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!IsClickable || e.Handled)
        {
            return;
        }

        if (Command is not null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }

        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        e.Handled = true;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _rootBorder = GetTemplateChild(PART_RootBorder) as Border;
    }

    #endregion Override Methods
}