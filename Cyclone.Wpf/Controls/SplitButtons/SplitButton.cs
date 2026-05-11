using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 分割按钮——左侧主按钮(执行默认动作) + 右侧下拉箭头(展开菜单)。
/// <para>
/// 主按钮通过 <see cref="Command"/> / <see cref="CommandParameter"/> / <see cref="Click"/> 三件套暴露交互;
/// 下拉项通过 <see cref="SplitButtonItem.Command"/> 各自独立绑定命令。点击任意项后 popup 自动关闭,
/// 并在 SplitButton 上冒泡 <see cref="ItemClick"/> 事件,OriginalSource 即被点击的 item。
/// </para>
/// <para>
/// 不继承 <see cref="Selector"/>——SplitButton 没有"选中"语义,只有"点击执行"。
/// </para>
/// </summary>
[StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(SplitButtonItem))]
[TemplatePart(Name = PART_MainButton, Type = typeof(ButtonBase))]
[TemplatePart(Name = PART_OpenButton, Type = typeof(ToggleButton))]
[TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
public class SplitButton : ItemsControl
{
    private const string PART_MainButton = nameof(PART_MainButton);

    private const string PART_OpenButton = nameof(PART_OpenButton);

    private const string PART_Popup = nameof(PART_Popup);

    private ButtonBase _mainButton;

    private ToggleButton _openButton;

    private Popup _popup;

    #region Constructors

    static SplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SplitButton),
            new FrameworkPropertyMetadata(typeof(SplitButton)));
    }

    #endregion Constructors

    #region Label

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(object),
            typeof(SplitButton),
            new PropertyMetadata(default(object)));

    /// <summary>主按钮显示的内容(可以是字符串、图标 + 文字、任意 UIElement)。</summary>
    public object Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    #endregion Label

    #region LabelTemplate

    public static readonly DependencyProperty LabelTemplateProperty =
        DependencyProperty.Register(
            nameof(LabelTemplate),
            typeof(DataTemplate),
            typeof(SplitButton),
            new PropertyMetadata(default(DataTemplate)));

    /// <summary>主按钮内容(<see cref="Label"/>)的数据模板。</summary>
    public DataTemplate LabelTemplate
    {
        get => (DataTemplate)GetValue(LabelTemplateProperty);
        set => SetValue(LabelTemplateProperty, value);
    }

    #endregion LabelTemplate

    #region Command

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(SplitButton),
            new PropertyMetadata(default(ICommand)));

    /// <summary>
    /// 主按钮点击时执行的命令。下拉菜单项的命令应通过 <see cref="SplitButtonItem.Command"/> 单独设置。
    /// CanExecute=false 时主按钮自动 disabled。
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
            typeof(SplitButton),
            new PropertyMetadata(default(object)));

    /// <summary>主按钮命令的参数。</summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    #endregion CommandParameter

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(SplitButton),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsOpenChanged));

    /// <summary>下拉菜单是否展开。可双向绑定;Popup 内部点击外部关闭时会同步回此值。</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var sb = (SplitButton)d;
        var routed = (bool)e.NewValue ? OpenedEvent : ClosedEvent;
        sb.RaiseEvent(new RoutedEventArgs(routed, sb));
    }

    #endregion IsOpen

    #region Placement

    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(
            nameof(Placement),
            typeof(PlacementMode),
            typeof(SplitButton),
            new PropertyMetadata(PlacementMode.Bottom));

    /// <summary>下拉菜单弹出位置。默认 <see cref="PlacementMode.Bottom"/>。</summary>
    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    #endregion Placement

    #region MaxDropDownHeight

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double),
            typeof(SplitButton),
            new PropertyMetadata(SystemParameters.PrimaryScreenHeight / 2));

    /// <summary>下拉菜单最大高度。超过时自动出现滚动条。默认为屏幕高度的一半。</summary>
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    #endregion MaxDropDownHeight

    #region RoutedEvents

    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Click),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SplitButton));

    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Closed),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SplitButton));

    public static readonly RoutedEvent ItemClickEvent =
            EventManager.RegisterRoutedEvent(
            nameof(ItemClick),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SplitButton));

    public static readonly RoutedEvent OpenedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Opened),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SplitButton));

    /// <summary>主按钮被点击时触发。OriginalSource 是 SplitButton 自己。</summary>
    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    /// <summary>下拉菜单关闭时触发。</summary>
    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    /// <summary>下拉菜单中任一 <see cref="SplitButtonItem"/> 被点击时触发。OriginalSource 是被点击的 item。</summary>
    public event RoutedEventHandler ItemClick
    {
        add => AddHandler(ItemClickEvent, value);
        remove => RemoveHandler(ItemClickEvent, value);
    }

    /// <summary>下拉菜单展开时触发。</summary>
    public event RoutedEventHandler Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    #endregion RoutedEvents

    #region Override Methods

    public override void OnApplyTemplate()
    {
        // 解除旧引用的事件订阅(OnApplyTemplate 可能被多次调用,比如换主题)
        if (_mainButton is not null)
        {
            _mainButton.Click -= OnMainButtonClick;
        }

        base.OnApplyTemplate();

        _mainButton = GetTemplateChild(PART_MainButton) as ButtonBase;
        _openButton = GetTemplateChild(PART_OpenButton) as ToggleButton;
        _popup = GetTemplateChild(PART_Popup) as Popup;

        if (_mainButton is not null)
        {
            _mainButton.Click += OnMainButtonClick;
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new SplitButtonItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is SplitButtonItem;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // Esc 关闭下拉
        if (e.Key == Key.Escape && IsOpen)
        {
            IsOpen = false;
            e.Handled = true;
            return;
        }

        // F4 / Alt+Down 切换下拉(对照 ComboBox 的快捷键)
        if (e.Key == Key.F4 ||
            (e.Key == Key.Down && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt))
        {
            IsOpen = !IsOpen;
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    #endregion Override Methods

    #region Private Methods

    /// <summary>
    /// 由 <see cref="SplitButtonItem"/> 在被点击时调用——关闭 popup 并在 SplitButton 上冒泡 <see cref="ItemClick"/>。
    /// </summary>
    internal void NotifyItemClicked(SplitButtonItem item)
    {
        IsOpen = false;
        RaiseEvent(new RoutedEventArgs(ItemClickEvent, item));
    }

    private void OnMainButtonClick(object sender, RoutedEventArgs e)
    {
        // 把主按钮(模板内部)的 Click 重新发为 SplitButton.Click(source = SplitButton 自己)
        e.Handled = true;
        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
    }

    #endregion Private Methods
}