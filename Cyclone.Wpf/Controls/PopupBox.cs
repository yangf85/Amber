using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 浮动弹出框——一个触发按钮(Content) + 一个浮窗(PopupContent)。
/// <para>
/// 支持四种触发方式(Click / Hover / RightClick / Focus),完整的 MVVM 三件套
/// (<see cref="Command"/>、<see cref="CloseCommand"/>、<see cref="OpenedCommand"/> / <see cref="ClosedCommand"/>),
/// 以及 <see cref="IsPositionUpdateProperty"/> 附加属性以便 popup 在窗口移动 / 大小变化时跟随。
/// </para>
/// </summary>
[ContentProperty(nameof(PopupContent))]
[TemplatePart(Name = PART_ToggleButton, Type = typeof(ToggleButton))]
[TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
public class PopupBox : Control
{
    private const string PART_ToggleButton = nameof(PART_ToggleButton);
    private const string PART_Popup = nameof(PART_Popup);

    private ToggleButton _toggleButton;
    private Popup _popup;

    // hover 模式下用于"延迟打开"的计时器
    private DispatcherTimer _hoverOpenTimer;

    // hover 模式下用于"延迟关闭"的计时器(鼠标短暂离开 popup 后再关)
    private DispatcherTimer _hoverCloseTimer;

    #region Constructors

    static PopupBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PopupBox),
            new FrameworkPropertyMetadata(typeof(PopupBox)));
    }

    public PopupBox()
    {
        // CloseCommand 是只读 DP,在构造函数里初始化
        SetValue(CloseCommandPropertyKey, new RelayClosePopupCommand(this));
    }

    #endregion Constructors

    #region Content

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(
            nameof(Content),
            typeof(object),
            typeof(PopupBox),
            new PropertyMetadata(default(object)));

    /// <summary>触发按钮上显示的内容。</summary>
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    #endregion Content

    #region ContentTemplate

    public static readonly DependencyProperty ContentTemplateProperty =
        DependencyProperty.Register(
            nameof(ContentTemplate),
            typeof(DataTemplate),
            typeof(PopupBox),
            new PropertyMetadata(default(DataTemplate)));

    /// <summary>触发按钮内容(<see cref="Content"/>)的数据模板。</summary>
    public DataTemplate ContentTemplate
    {
        get => (DataTemplate)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    #endregion ContentTemplate

    #region PopupContent

    public static readonly DependencyProperty PopupContentProperty =
        DependencyProperty.Register(
            nameof(PopupContent),
            typeof(object),
            typeof(PopupBox),
            new PropertyMetadata(default(object), OnPopupContentChanged));

    /// <summary>popup 内显示的主内容(XAML 默认内容属性)。</summary>
    public object PopupContent
    {
        get => GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }

    private static void OnPopupContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (PopupBox)d;
        if (e.OldValue is not null)
        {
            box.RemoveLogicalChild(e.OldValue);
        }
        if (e.NewValue is not null)
        {
            box.AddLogicalChild(e.NewValue);
        }

        // ★ 新内容如果是 FrameworkElement,把 DataContext 推过去
        if (e.NewValue is FrameworkElement fe)
        {
            fe.DataContext = box.DataContext;
        }
    }

    #endregion PopupContent

    #region PopupContentTemplate

    public static readonly DependencyProperty PopupContentTemplateProperty =
        DependencyProperty.Register(
            nameof(PopupContentTemplate),
            typeof(DataTemplate),
            typeof(PopupBox),
            new PropertyMetadata(default(DataTemplate)));

    /// <summary>popup 内主内容(<see cref="PopupContent"/>)的数据模板。</summary>
    public DataTemplate PopupContentTemplate
    {
        get => (DataTemplate)GetValue(PopupContentTemplateProperty);
        set => SetValue(PopupContentTemplateProperty, value);
    }

    #endregion PopupContentTemplate

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(PopupBox),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsOpenChanged));

    /// <summary>popup 是否展开。可双向绑定。</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 路由事件 + Command 触发集中在 popup 的 Opened/Closed 回调里,避免重复
    }

    #endregion IsOpen

    #region TriggerMode

    public static readonly DependencyProperty TriggerModeProperty =
        DependencyProperty.Register(
            nameof(TriggerMode),
            typeof(PopupTriggerMode),
            typeof(PopupBox),
            new PropertyMetadata(PopupTriggerMode.Click));

    /// <summary>触发 popup 展开的方式。默认 <see cref="PopupTriggerMode.Click"/>。</summary>
    public PopupTriggerMode TriggerMode
    {
        get => (PopupTriggerMode)GetValue(TriggerModeProperty);
        set => SetValue(TriggerModeProperty, value);
    }

    #endregion TriggerMode

    #region HoverDelay

    public static readonly DependencyProperty HoverDelayProperty =
        DependencyProperty.Register(
            nameof(HoverDelay),
            typeof(TimeSpan),
            typeof(PopupBox),
            new PropertyMetadata(TimeSpan.FromMilliseconds(200)));

    /// <summary><see cref="PopupTriggerMode.Hover"/> 模式下,鼠标悬停多久后才展开。默认 200ms。</summary>
    public TimeSpan HoverDelay
    {
        get => (TimeSpan)GetValue(HoverDelayProperty);
        set => SetValue(HoverDelayProperty, value);
    }

    #endregion HoverDelay

    #region Command

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(
            nameof(Command),
            typeof(ICommand),
            typeof(PopupBox),
            new PropertyMetadata(default(ICommand)));

    /// <summary>触发按钮被点击时执行的命令(独立于 popup 的开关)。CanExecute=false 时按钮 disabled。</summary>
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
            typeof(PopupBox),
            new PropertyMetadata(default(object)));

    /// <summary><see cref="Command"/> 的参数。</summary>
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    #endregion CommandParameter

    #region CloseCommand (ReadOnly)

    private static readonly DependencyPropertyKey CloseCommandPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(CloseCommand),
            typeof(ICommand),
            typeof(PopupBox),
            new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty CloseCommandProperty = CloseCommandPropertyKey.DependencyProperty;

    /// <summary>
    /// 关闭 popup 的命令。供 popup 内的按钮直接绑定使用,无需 ViewModel 中转。
    /// 用法:&lt;Button Command="{Binding CloseCommand, RelativeSource={RelativeSource AncestorType=ctl:PopupBox}}" /&gt;
    /// </summary>
    public ICommand CloseCommand => (ICommand)GetValue(CloseCommandProperty);

    #endregion CloseCommand (ReadOnly)

    #region OpenedCommand

    public static readonly DependencyProperty OpenedCommandProperty =
        DependencyProperty.Register(
            nameof(OpenedCommand),
            typeof(ICommand),
            typeof(PopupBox),
            new PropertyMetadata(default(ICommand)));

    /// <summary>popup 展开后执行的命令(VM 副作用,例如刷新数据)。</summary>
    public ICommand OpenedCommand
    {
        get => (ICommand)GetValue(OpenedCommandProperty);
        set => SetValue(OpenedCommandProperty, value);
    }

    #endregion OpenedCommand

    #region ClosedCommand

    public static readonly DependencyProperty ClosedCommandProperty =
        DependencyProperty.Register(
            nameof(ClosedCommand),
            typeof(ICommand),
            typeof(PopupBox),
            new PropertyMetadata(default(ICommand)));

    /// <summary>popup 关闭后执行的命令(VM 副作用,例如保存表单)。</summary>
    public ICommand ClosedCommand
    {
        get => (ICommand)GetValue(ClosedCommandProperty);
        set => SetValue(ClosedCommandProperty, value);
    }

    #endregion ClosedCommand

    #region Placement

    public static readonly DependencyProperty PlacementProperty =
        Popup.PlacementProperty.AddOwner(
            typeof(PopupBox),
            new FrameworkPropertyMetadata(PlacementMode.Bottom));

    /// <summary>popup 弹出位置。默认 <see cref="PlacementMode.Bottom"/>。</summary>
    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    #endregion Placement

    #region HorizontalOffset

    public static readonly DependencyProperty HorizontalOffsetProperty =
        Popup.HorizontalOffsetProperty.AddOwner(typeof(PopupBox));

    /// <summary>popup 水平偏移。</summary>
    public double HorizontalOffset
    {
        get => (double)GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    #endregion HorizontalOffset

    #region VerticalOffset

    public static readonly DependencyProperty VerticalOffsetProperty =
        Popup.VerticalOffsetProperty.AddOwner(typeof(PopupBox));

    /// <summary>popup 垂直偏移。</summary>
    public double VerticalOffset
    {
        get => (double)GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    #endregion VerticalOffset

    #region PopupAnimation

    public static readonly DependencyProperty PopupAnimationProperty =
        Popup.PopupAnimationProperty.AddOwner(
            typeof(PopupBox),
            new FrameworkPropertyMetadata(PopupAnimation.Fade));

    /// <summary>popup 动画。默认 <see cref="PopupAnimation.Fade"/>。</summary>
    public PopupAnimation PopupAnimation
    {
        get => (PopupAnimation)GetValue(PopupAnimationProperty);
        set => SetValue(PopupAnimationProperty, value);
    }

    #endregion PopupAnimation

    #region StaysOpen

    public static readonly DependencyProperty StaysOpenProperty =
        Popup.StaysOpenProperty.AddOwner(
            typeof(PopupBox),
            new FrameworkPropertyMetadata(false));

    /// <summary>popup 是否点击外部仍保持打开。默认 false(点击外部关闭)。</summary>
    public bool StaysOpen
    {
        get => (bool)GetValue(StaysOpenProperty);
        set => SetValue(StaysOpenProperty, value);
    }

    #endregion StaysOpen

    #region MaxDropDownHeight

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double),
            typeof(PopupBox),
            new PropertyMetadata(double.PositiveInfinity));

    /// <summary>popup 最大高度。超过则出现滚动条。</summary>
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    #endregion MaxDropDownHeight

    #region MaxDropDownWidth

    public static readonly DependencyProperty MaxDropDownWidthProperty =
        DependencyProperty.Register(
            nameof(MaxDropDownWidth),
            typeof(double),
            typeof(PopupBox),
            new PropertyMetadata(double.PositiveInfinity));

    /// <summary>popup 最大宽度。</summary>
    public double MaxDropDownWidth
    {
        get => (double)GetValue(MaxDropDownWidthProperty);
        set => SetValue(MaxDropDownWidthProperty, value);
    }

    #endregion MaxDropDownWidth

    #region PopupBackground

    public static readonly DependencyProperty PopupBackgroundProperty =
        DependencyProperty.Register(
            nameof(PopupBackground),
            typeof(Brush),
            typeof(PopupBox),
            new PropertyMetadata(default(Brush)));

    /// <summary>popup 容器的背景色。模板中的 ContentPresenter 会被包在带此背景的 Border 内。</summary>
    public Brush PopupBackground
    {
        get => (Brush)GetValue(PopupBackgroundProperty);
        set => SetValue(PopupBackgroundProperty, value);
    }

    #endregion PopupBackground

    #region PopupBorderBrush

    public static readonly DependencyProperty PopupBorderBrushProperty =
        DependencyProperty.Register(
            nameof(PopupBorderBrush),
            typeof(Brush),
            typeof(PopupBox),
            new PropertyMetadata(default(Brush)));

    /// <summary>popup 容器的边框色。</summary>
    public Brush PopupBorderBrush
    {
        get => (Brush)GetValue(PopupBorderBrushProperty);
        set => SetValue(PopupBorderBrushProperty, value);
    }

    #endregion PopupBorderBrush

    #region PopupBorderThickness

    public static readonly DependencyProperty PopupBorderThicknessProperty =
        DependencyProperty.Register(
            nameof(PopupBorderThickness),
            typeof(Thickness),
            typeof(PopupBox),
            new PropertyMetadata(new Thickness(1)));

    /// <summary>popup 容器的边框粗细。</summary>
    public Thickness PopupBorderThickness
    {
        get => (Thickness)GetValue(PopupBorderThicknessProperty);
        set => SetValue(PopupBorderThicknessProperty, value);
    }

    #endregion PopupBorderThickness

    #region PopupPadding

    public static readonly DependencyProperty PopupPaddingProperty =
        DependencyProperty.Register(
            nameof(PopupPadding),
            typeof(Thickness),
            typeof(PopupBox),
            new PropertyMetadata(new Thickness(8)));

    /// <summary>popup 容器内的 padding。</summary>
    public Thickness PopupPadding
    {
        get => (Thickness)GetValue(PopupPaddingProperty);
        set => SetValue(PopupPaddingProperty, value);
    }

    #endregion PopupPadding

    #region HasDropShadow

    public static readonly DependencyProperty HasDropShadowProperty =
        DependencyProperty.Register(
            nameof(HasDropShadow),
            typeof(bool),
            typeof(PopupBox),
            new PropertyMetadata(true));

    /// <summary>popup 是否带阴影。默认 true。Win10 风格场景可关闭。</summary>
    public bool HasDropShadow
    {
        get => (bool)GetValue(HasDropShadowProperty);
        set => SetValue(HasDropShadowProperty, value);
    }

    #endregion HasDropShadow

    #region RoutedEvents

    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Click),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(PopupBox));

    public static readonly RoutedEvent OpenedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Opened),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(PopupBox));

    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Closed),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(PopupBox));

    /// <summary>触发按钮被点击时触发(独立于 popup 开关)。</summary>
    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    /// <summary>popup 展开后触发。</summary>
    public event RoutedEventHandler Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    /// <summary>popup 关闭后触发。</summary>
    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    #endregion RoutedEvents

    #region Override Methods

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (TriggerMode == PopupTriggerMode.Hover)
        {
            CancelHoverCloseTimer();
            ScheduleHoverOpen();
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (TriggerMode == PopupTriggerMode.Hover)
        {
            CancelHoverOpenTimer();
            ScheduleHoverClose();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen)
        {
            IsOpen = false;
            e.Handled = true;
            return;
        }
        base.OnKeyDown(e);
    }

    public override void OnApplyTemplate()
    {
        // 解除旧引用的事件订阅 —— OnApplyTemplate 可能被多次调用(换主题等)
        UnhookToggleButton();
        UnhookPopup();
        UnhookDataContextSync();

        base.OnApplyTemplate();

        _toggleButton = GetTemplateChild(PART_ToggleButton) as ToggleButton;
        _popup = GetTemplateChild(PART_Popup) as Popup;

        HookToggleButton();
        HookPopup();
        HookDataContextSync();
    }

    #endregion Override Methods

    #region Private Methods — event hooking

    private void HookToggleButton()
    {
        if (_toggleButton is null)
        {
            return;
        }
        _toggleButton.Click += OnToggleButtonClick;
        _toggleButton.IsVisibleChanged += OnToggleButtonVisibleChanged;
    }

    private void UnhookToggleButton()
    {
        if (_toggleButton is null)
        {
            return;
        }
        _toggleButton.Click -= OnToggleButtonClick;
        _toggleButton.IsVisibleChanged -= OnToggleButtonVisibleChanged;
    }

    private void HookPopup()
    {
        if (_popup is null)
        {
            return;
        }
        _popup.Opened += OnPopupOpened;
        _popup.Closed += OnPopupClosed;
        _popup.MouseEnter += OnPopupMouseEnter;
        _popup.MouseLeave += OnPopupMouseLeave;
    }

    private void UnhookPopup()
    {
        if (_popup is null)
        {
            return;
        }
        _popup.Opened -= OnPopupOpened;
        _popup.Closed -= OnPopupClosed;
        _popup.MouseEnter -= OnPopupMouseEnter;
        _popup.MouseLeave -= OnPopupMouseLeave;
    }

    /// <summary>
    /// 把 PopupBox.DataContext 同步到 Popup.DataContext + PopupContent.DataContext。
    /// <para>
    /// 关键:popup 的 Content 不在 PopupBox 的视觉/逻辑树里,DataContext 不会自动继承,
    /// 必须手动桥接,否则 popup 内的 {Binding XXX} 全部失效。
    /// </para>
    /// <para>
    /// 更隐蔽的是,即使 Popup.DataContext 设了,如果 PopupContent 本身是 UIElement
    /// (比如直接写 <c>&lt;Border&gt;...&lt;/Border&gt;</c>),它有独立的 DataContext,
    /// 不会从 ContentPresenter 继承——必须主动把 DataContext 推给它。
    /// </para>
    /// </summary>
    private void HookDataContextSync()
    {
        DataContextChanged += OnDataContextChangedSync;
        SyncPopupDataContext();
    }

    private void UnhookDataContextSync()
    {
        DataContextChanged -= OnDataContextChangedSync;
    }

    private void OnDataContextChangedSync(object sender, DependencyPropertyChangedEventArgs e)
    {
        SyncPopupDataContext();
    }

    private void SyncPopupDataContext()
    {
        _popup?.DataContext = DataContext;

        // PopupContent 如果是 UIElement(用户直接写的 <Border>...</Border> 等),
        // 它有自己独立的 DataContext,不会从 Popup 继承——必须主动同步。
        if (PopupContent is FrameworkElement fe)
        {
            fe.DataContext = DataContext;
        }
    }

    private void OnToggleButtonClick(object sender, RoutedEventArgs e)
    {
        // Click 路由事件 —— 用户可在 PopupBox 上监听
        RaiseEvent(new RoutedEventArgs(ClickEvent, this));

        // 主按钮 Command —— 与 popup 开关无关的"附带动作"
        if (Command is { } cmd && cmd.CanExecute(CommandParameter))
        {
            cmd.Execute(CommandParameter);
        }
    }

    private void OnToggleButtonVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // 触发按钮被隐藏时,popup 也跟着关
        if (_toggleButton is not null && !_toggleButton.IsVisible && IsOpen)
        {
            IsOpen = false;
        }
    }

    private void OnPopupOpened(object sender, EventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(OpenedEvent, this));
        if (OpenedCommand is { } cmd && cmd.CanExecute(null))
        {
            cmd.Execute(null);
        }
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
        if (ClosedCommand is { } cmd && cmd.CanExecute(null))
        {
            cmd.Execute(null);
        }
    }

    private void OnPopupMouseEnter(object sender, MouseEventArgs e)
    {
        // hover 模式下鼠标进入 popup 时取消"延迟关闭",保持打开
        if (TriggerMode == PopupTriggerMode.Hover)
        {
            CancelHoverCloseTimer();
        }
    }

    private void OnPopupMouseLeave(object sender, MouseEventArgs e)
    {
        if (TriggerMode == PopupTriggerMode.Hover)
        {
            ScheduleHoverClose();
        }
    }

    #endregion Private Methods — event hooking

    #region Private Methods — hover timers

    private void ScheduleHoverOpen()
    {
        if (IsOpen)
        {
            return;
        }
        if (_hoverOpenTimer is null)
        {
            _hoverOpenTimer = new DispatcherTimer { Interval = HoverDelay };
            _hoverOpenTimer.Tick += OnHoverOpenTick;
        }
        _hoverOpenTimer.Interval = HoverDelay;
        _hoverOpenTimer.Stop();
        _hoverOpenTimer.Start();
    }

    private void CancelHoverOpenTimer() => _hoverOpenTimer?.Stop();

    private void OnHoverOpenTick(object sender, EventArgs e)
    {
        _hoverOpenTimer.Stop();
        IsOpen = true;
    }

    private void ScheduleHoverClose()
    {
        if (!IsOpen)
        {
            return;
        }
        if (_hoverCloseTimer is null)
        {
            _hoverCloseTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
            _hoverCloseTimer.Tick += OnHoverCloseTick;
        }
        _hoverCloseTimer.Stop();
        _hoverCloseTimer.Start();
    }

    private void CancelHoverCloseTimer() => _hoverCloseTimer?.Stop();

    private void OnHoverCloseTick(object sender, EventArgs e)
    {
        _hoverCloseTimer.Stop();
        // 鼠标既不在 PopupBox 上、也不在 popup 上,才关
        if (!IsMouseOver && (_popup is null || !_popup.IsMouseOver))
        {
            IsOpen = false;
        }
    }

    #endregion Private Methods — hover timers

    #region Attached Property: IsPositionUpdate

    /// <summary>
    /// 用于在附加属性回调里"成对解订阅"——把订阅句柄寄存到 popup 自身的私有附加属性上。
    /// </summary>
    private static readonly DependencyProperty PositionUpdateHookProperty =
        DependencyProperty.RegisterAttached(
            "PositionUpdateHook",
            typeof(PositionUpdateHook),
            typeof(PopupBox),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsPositionUpdateProperty =
            DependencyProperty.RegisterAttached(
            "IsPositionUpdate",
            typeof(bool),
            typeof(PopupBox),
            new PropertyMetadata(false, OnIsPositionUpdateChanged));

    private static void OnIsPositionUpdateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Popup popup)
        {
            return;
        }

        // 先清掉旧 hook(支持 true→false 的关闭)
        if (popup.GetValue(PositionUpdateHookProperty) is PositionUpdateHook old)
        {
            old.Dispose();
            popup.ClearValue(PositionUpdateHookProperty);
        }

        if (e.NewValue is true)
        {
            popup.SetValue(PositionUpdateHookProperty, new PositionUpdateHook(popup));
        }
    }

    public static bool GetIsPositionUpdate(Popup obj) => (bool)obj.GetValue(IsPositionUpdateProperty);

    public static void SetIsPositionUpdate(Popup obj, bool value) => obj.SetValue(IsPositionUpdateProperty, value);

    /// <summary>
    /// 替代原版反射调用 Popup.UpdatePosition 的方案 —— 通过临时切换 IsOpen 触发 popup 重新定位。
    /// 订阅 popup 所在 Window 的 LocationChanged / SizeChanged,popup 已打开时自动跟随。
    /// </summary>
    private sealed class PositionUpdateHook : IDisposable
    {
        private readonly Popup _popup;
        private Window _window;

        private void OnPopupLoaded(object sender, RoutedEventArgs e) => AttachWindow();

        private void OnPopupUnloaded(object sender, RoutedEventArgs e) => DetachWindow();

        private void AttachWindow()
        {
            DetachWindow();
            _window = Window.GetWindow(_popup);
            if (_window is null)
            {
                return;
            }
            _window.LocationChanged += OnWindowChanged;
            _window.SizeChanged += OnWindowChanged;
        }

        private void DetachWindow()
        {
            if (_window is null)
            {
                return;
            }
            _window.LocationChanged -= OnWindowChanged;
            _window.SizeChanged -= OnWindowChanged;
            _window = null;
        }

        private void OnWindowChanged(object sender, EventArgs e)
        {
            if (!_popup.IsOpen)
            {
                return;
            }
            // 触发 popup 重新计算位置 —— 通过快速切换 HorizontalOffset 强制重布局
            // (相比反射调用 UpdatePosition 私有方法,这种做法对 .NET 版本无依赖)
            var offset = _popup.HorizontalOffset;
            _popup.HorizontalOffset = offset + 0.001;
            _popup.HorizontalOffset = offset;
        }

        public void Dispose()
        {
            _popup.Loaded -= OnPopupLoaded;
            _popup.Unloaded -= OnPopupUnloaded;
            DetachWindow();
        }

        public PositionUpdateHook(Popup popup)
        {
            _popup = popup;
            _popup.Loaded += OnPopupLoaded;
            _popup.Unloaded += OnPopupUnloaded;
            // 已经在 visual tree 中(比如重新设属性)就立刻挂上
            if (_popup.IsLoaded)
            {
                AttachWindow();
            }
        }
    }

    #endregion Attached Property: IsPositionUpdate

    #region Nested: RelayClosePopupCommand

    /// <summary>CloseCommand 的实现 —— 简单关闭 popup。</summary>
    private sealed class RelayClosePopupCommand : ICommand
    {
        private readonly PopupBox _owner;

        public bool CanExecute(object parameter) => _owner.IsOpen;

        public void Execute(object parameter) => _owner.IsOpen = false;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayClosePopupCommand(PopupBox owner) => _owner = owner;
    }

    #endregion Nested: RelayClosePopupCommand
}

/// <summary>触发 <see cref="PopupBox"/> 展开的方式。</summary>
public enum PopupTriggerMode
{
    /// <summary>左键点击触发按钮(默认)。</summary>
    Click,

    /// <summary>鼠标悬停触发按钮(带 <see cref="PopupBox.HoverDelay"/> 延迟,鼠标进入 popup 时保持打开)。</summary>
    Hover,
}