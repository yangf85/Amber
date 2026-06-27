using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 抽屉控件：从容器边缘滑入滑出的面板控件。
/// </summary>
[TemplatePart(Name = nameof(PART_DrawerPanel), Type = typeof(FrameworkElement))]
[TemplatePart(Name = nameof(PART_Overlay), Type = typeof(FrameworkElement))]
public class Drawer : ContentControl
{
    private const string PART_DrawerPanel = "PART_DrawerPanel";

    private const string PART_Overlay = "PART_Overlay";

    /// <summary>
    /// 当 <see cref="DrawerHeight"/> 为 NaN 时使用的回退高度。
    /// </summary>
    private const double DefaultDrawerHeight = 300d;

    private FrameworkElement _drawerPanel;

    private FrameworkElement _overlay;

    private TranslateTransform _translateTransform;

    static Drawer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Drawer),
            new FrameworkPropertyMetadata(typeof(Drawer)));

        // 注册类级 CommandBinding，让命令路由到当前 Drawer 实例
        CommandManager.RegisterClassCommandBinding(typeof(Drawer),
            new CommandBinding(OpenCommand, OnOpenCommandExecuted, OnOpenCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(Drawer),
            new CommandBinding(CloseCommand, OnCloseCommandExecuted, OnCloseCommandCanExecute));
        CommandManager.RegisterClassCommandBinding(typeof(Drawer),
            new CommandBinding(ToggleCommand, OnToggleCommandExecuted));
    }

    #region DrawerContent

    /// <summary>
    /// 定义抽屉内容的依赖属性。
    /// </summary>
    public static readonly DependencyProperty DrawerContentProperty =
        DependencyProperty.Register(
            nameof(DrawerContent),
            typeof(object),
            typeof(Drawer),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置抽屉的内容。
    /// </summary>
    public object DrawerContent
    {
        get => GetValue(DrawerContentProperty);
        set => SetValue(DrawerContentProperty, value);
    }

    #endregion DrawerContent

    #region DrawerContentTemplate

    /// <summary>
    /// 定义抽屉内容模板的依赖属性。
    /// </summary>
    public static readonly DependencyProperty DrawerContentTemplateProperty =
        DependencyProperty.Register(
            nameof(DrawerContentTemplate),
            typeof(DataTemplate),
            typeof(Drawer),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置抽屉内容模板。
    /// </summary>
    public DataTemplate DrawerContentTemplate
    {
        get => (DataTemplate)GetValue(DrawerContentTemplateProperty);
        set => SetValue(DrawerContentTemplateProperty, value);
    }

    #endregion DrawerContentTemplate

    #region DrawerContentTemplateSelector

    /// <summary>
    /// 定义抽屉内容模板选择器的依赖属性。
    /// </summary>
    public static readonly DependencyProperty DrawerContentTemplateSelectorProperty =
        DependencyProperty.Register(
            nameof(DrawerContentTemplateSelector),
            typeof(DataTemplateSelector),
            typeof(Drawer),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置抽屉内容模板选择器。
    /// </summary>
    public DataTemplateSelector DrawerContentTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(DrawerContentTemplateSelectorProperty);
        set => SetValue(DrawerContentTemplateSelectorProperty, value);
    }

    #endregion DrawerContentTemplateSelector

    #region IsOpen

    /// <summary>
    /// 定义抽屉是否打开的依赖属性。
    /// </summary>
    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(Drawer),
            new PropertyMetadata(false, OnIsOpenChanged));

    /// <summary>
    /// 获取或设置抽屉是否打开。
    /// </summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var drawer = (Drawer)d;
        var newValue = (bool)e.NewValue;
        var oldValue = (bool)e.OldValue;

        if (newValue == oldValue)
        {
            return;
        }

        if (newValue)
        {
            drawer.RaiseEvent(new RoutedEventArgs(OpeningEvent, drawer));

            if (drawer._drawerPanel != null)
            {
                drawer._drawerPanel.Visibility = Visibility.Visible;
            }

            drawer.UpdateOverlayVisibility(animate: true);
            drawer.AnimateToOpenPosition();
        }
        else
        {
            drawer.RaiseEvent(new RoutedEventArgs(ClosingEvent, drawer));
            drawer.UpdateOverlayVisibility(animate: true);

            // 关闭动画期间保持抽屉面板可见，待动画完成后再 Collapse
            if (drawer._drawerPanel != null)
            {
                drawer._drawerPanel.Visibility = Visibility.Visible;
            }

            drawer.AnimateToClosedPosition();
        }

        // 状态变化通知命令重新评估 CanExecute
        CommandManager.InvalidateRequerySuggested();
    }

    #endregion IsOpen

    #region DrawerWidth

    /// <summary>
    /// 定义抽屉宽度的依赖属性。
    /// </summary>
    public static readonly DependencyProperty DrawerWidthProperty =
        DependencyProperty.Register(
            nameof(DrawerWidth),
            typeof(double),
            typeof(Drawer),
            new PropertyMetadata(300d, OnDrawerSizeChanged));

    /// <summary>
    /// 获取或设置抽屉的宽度（Left / Right 模式生效）。
    /// </summary>
    public double DrawerWidth
    {
        get => (double)GetValue(DrawerWidthProperty);
        set => SetValue(DrawerWidthProperty, value);
    }

    #endregion DrawerWidth

    #region DrawerHeight

    /// <summary>
    /// 定义抽屉高度的依赖属性。
    /// </summary>
    public static readonly DependencyProperty DrawerHeightProperty =
        DependencyProperty.Register(
            nameof(DrawerHeight),
            typeof(double),
            typeof(Drawer),
            new PropertyMetadata(double.NaN, OnDrawerSizeChanged));

    /// <summary>
    /// 获取或设置抽屉的高度（Top / Bottom 模式生效）。NaN 时使用默认值 300。
    /// </summary>
    public double DrawerHeight
    {
        get => (double)GetValue(DrawerHeightProperty);
        set => SetValue(DrawerHeightProperty, value);
    }

    #endregion DrawerHeight

    #region Placement

    /// <summary>
    /// 定义抽屉位置的依赖属性。
    /// </summary>
    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(
            nameof(Placement),
            typeof(DrawerPlacement),
            typeof(Drawer),
            new PropertyMetadata(DrawerPlacement.Left, OnPlacementChanged));

    /// <summary>
    /// 获取或设置抽屉的放置位置。
    /// </summary>
    public DrawerPlacement Placement
    {
        get => (DrawerPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var drawer = (Drawer)d;
        drawer.UpdateDrawerSize();

        if (!drawer.IsOpen)
        {
            drawer.UpdateClosedPosition();
        }
    }

    #endregion Placement

    #region AnimationDuration

    /// <summary>
    /// 定义动画持续时间的依赖属性。
    /// </summary>
    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(
            nameof(AnimationDuration),
            typeof(Duration),
            typeof(Drawer),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(300))));

    /// <summary>
    /// 获取或设置打开 / 关闭动画的持续时间。
    /// </summary>
    public Duration AnimationDuration
    {
        get => (Duration)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    #endregion AnimationDuration

    #region CloseOnOverlayClick

    /// <summary>
    /// 定义点击遮罩层是否关闭抽屉的依赖属性。
    /// </summary>
    public static readonly DependencyProperty CloseOnOverlayClickProperty =
        DependencyProperty.Register(
            nameof(CloseOnOverlayClick),
            typeof(bool),
            typeof(Drawer),
            new PropertyMetadata(true));

    /// <summary>
    /// 获取或设置点击遮罩层时是否关闭抽屉。
    /// </summary>
    public bool CloseOnOverlayClick
    {
        get => (bool)GetValue(CloseOnOverlayClickProperty);
        set => SetValue(CloseOnOverlayClickProperty, value);
    }

    #endregion CloseOnOverlayClick

    #region CloseOnEscape

    /// <summary>
    /// 定义按 ESC 键是否关闭抽屉的依赖属性。
    /// </summary>
    public static readonly DependencyProperty CloseOnEscapeProperty =
        DependencyProperty.Register(
            nameof(CloseOnEscape),
            typeof(bool),
            typeof(Drawer),
            new PropertyMetadata(true));

    /// <summary>
    /// 获取或设置抽屉打开时按下 ESC 键是否关闭抽屉。
    /// </summary>
    public bool CloseOnEscape
    {
        get => (bool)GetValue(CloseOnEscapeProperty);
        set => SetValue(CloseOnEscapeProperty, value);
    }

    #endregion CloseOnEscape

    #region FocusOnOpen

    /// <summary>
    /// 定义抽屉打开后是否自动聚焦到首个可聚焦元素的依赖属性。
    /// </summary>
    public static readonly DependencyProperty FocusOnOpenProperty =
        DependencyProperty.Register(
            nameof(FocusOnOpen),
            typeof(bool),
            typeof(Drawer),
            new PropertyMetadata(true));

    /// <summary>
    /// 获取或设置抽屉打开后是否自动将键盘焦点移入抽屉内首个可聚焦元素。
    /// </summary>
    public bool FocusOnOpen
    {
        get => (bool)GetValue(FocusOnOpenProperty);
        set => SetValue(FocusOnOpenProperty, value);
    }

    #endregion FocusOnOpen

    #region RoutedEvents

    /// <summary>
    /// 抽屉开始打开时触发的路由事件。
    /// </summary>
    public static readonly RoutedEvent OpeningEvent =
        EventManager.RegisterRoutedEvent(nameof(Opening), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Drawer));

    /// <summary>
    /// 抽屉打开完成后触发的路由事件。
    /// </summary>
    public static readonly RoutedEvent OpenedEvent =
        EventManager.RegisterRoutedEvent(nameof(Opened), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Drawer));

    /// <summary>
    /// 抽屉开始关闭时触发的路由事件。
    /// </summary>
    public static readonly RoutedEvent ClosingEvent =
        EventManager.RegisterRoutedEvent(nameof(Closing), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Drawer));

    /// <summary>
    /// 抽屉关闭完成后触发的路由事件。
    /// </summary>
    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent(nameof(Closed), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Drawer));

    /// <summary>抽屉开始打开时触发。</summary>
    public event RoutedEventHandler Opening
    {
        add => AddHandler(OpeningEvent, value);
        remove => RemoveHandler(OpeningEvent, value);
    }

    /// <summary>抽屉打开完成后触发。</summary>
    public event RoutedEventHandler Opened
    {
        add => AddHandler(OpenedEvent, value);
        remove => RemoveHandler(OpenedEvent, value);
    }

    /// <summary>抽屉开始关闭时触发。</summary>
    public event RoutedEventHandler Closing
    {
        add => AddHandler(ClosingEvent, value);
        remove => RemoveHandler(ClosingEvent, value);
    }

    /// <summary>抽屉关闭完成后触发。</summary>
    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    #endregion RoutedEvents

    #region Commands

    /// <summary>打开抽屉的路由命令。</summary>
    public static readonly RoutedCommand OpenCommand =
        new RoutedCommand(nameof(OpenCommand), typeof(Drawer));

    /// <summary>关闭抽屉的路由命令。</summary>
    public static readonly RoutedCommand CloseCommand =
        new RoutedCommand(nameof(CloseCommand), typeof(Drawer));

    /// <summary>切换抽屉打开 / 关闭状态的路由命令。</summary>
    public static readonly RoutedCommand ToggleCommand =
        new RoutedCommand(nameof(ToggleCommand), typeof(Drawer));

    private static void OnOpenCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Drawer drawer)
        {
            e.CanExecute = !drawer.IsOpen;
        }
    }

    private static void OnOpenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Drawer drawer)
        {
            drawer.Open();
            e.Handled = true;
        }
    }

    private static void OnCloseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Drawer drawer)
        {
            e.CanExecute = drawer.IsOpen;
        }
    }

    private static void OnCloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Drawer drawer)
        {
            drawer.Close();
            e.Handled = true;
        }
    }

    private static void OnToggleCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Drawer drawer)
        {
            drawer.Toggle();
            e.Handled = true;
        }
    }

    #endregion Commands

    #region Override Methods

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解除旧模板元素的事件
        if (_overlay != null)
        {
            _overlay.MouseLeftButtonDown -= OnOverlayClick;
        }

        _drawerPanel = GetTemplateChild(PART_DrawerPanel) as FrameworkElement;
        _overlay = GetTemplateChild(PART_Overlay) as FrameworkElement;

        if (_drawerPanel != null)
        {
            // 创建平移变换；如果模板里已经有 RenderTransform，则组合成 TransformGroup 不覆盖
            _translateTransform = new TranslateTransform();
            ApplyTranslateTransform(_drawerPanel, _translateTransform);

            UpdateDrawerSize();

            if (IsOpen)
            {
                _drawerPanel.Visibility = Visibility.Visible;
                _translateTransform.X = 0;
                _translateTransform.Y = 0;
            }
            else
            {
                _drawerPanel.Visibility = Visibility.Collapsed;
                UpdateClosedPosition();
            }
        }

        // 始终挂载点击事件，由 OnOverlayClick 内部根据 CloseOnOverlayClick 判断行为
        // 这样 CloseOnOverlayClick 运行时切换才能生效
        if (_overlay != null)
        {
            _overlay.MouseLeftButtonDown += OnOverlayClick;
        }

        // 初始化遮罩状态（不动画，避免首次渲染闪烁）
        UpdateOverlayVisibility(animate: false);
    }

    /// <inheritdoc />
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (!e.Handled && IsOpen && CloseOnEscape && e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    #endregion Override Methods

    #region Public Methods

    /// <summary>
    /// 打开抽屉。
    /// </summary>
    public void Open()
    {
        IsOpen = true;
    }

    /// <summary>
    /// 关闭抽屉。
    /// </summary>
    public void Close()
    {
        IsOpen = false;
    }

    /// <summary>
    /// 切换抽屉的打开 / 关闭状态。
    /// </summary>
    public void Toggle()
    {
        IsOpen = !IsOpen;
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// 解析后的抽屉高度，处理 NaN 回退。
    /// </summary>
    private double ResolvedDrawerHeight =>
        double.IsNaN(DrawerHeight) ? DefaultDrawerHeight : DrawerHeight;

    /// <summary>
    /// DrawerWidth / DrawerHeight 共享的尺寸变化回调。
    /// </summary>
    private static void OnDrawerSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var drawer = (Drawer)d;
        drawer.UpdateDrawerSize();

        if (!drawer.IsOpen)
        {
            drawer.UpdateClosedPosition();
        }
    }

    /// <summary>
    /// 将平移变换应用到抽屉面板，与既有 RenderTransform 组合而非覆盖。
    /// </summary>
    private static void ApplyTranslateTransform(FrameworkElement element, TranslateTransform translate)
    {
        var existing = element.RenderTransform;

        if (existing is TransformGroup existingGroup)
        {
            existingGroup.Children.Add(translate);
        }
        else if (existing != null && existing != Transform.Identity)
        {
            var group = new TransformGroup();
            group.Children.Add(existing);
            group.Children.Add(translate);
            element.RenderTransform = group;
        }
        else
        {
            element.RenderTransform = translate;
        }
    }

    /// <summary>
    /// 根据 Placement 更新抽屉的尺寸约束。
    /// </summary>
    private void UpdateDrawerSize()
    {
        if (_drawerPanel == null)
        {
            return;
        }

        switch (Placement)
        {
            case DrawerPlacement.Left:
            case DrawerPlacement.Right:
                _drawerPanel.Width = DrawerWidth;
                _drawerPanel.Height = double.NaN;
                break;

            case DrawerPlacement.Top:
            case DrawerPlacement.Bottom:
                _drawerPanel.Width = double.NaN;
                _drawerPanel.Height = ResolvedDrawerHeight;
                break;
        }
    }

    /// <summary>
    /// 根据 Placement 计算关闭状态下的位移。
    /// 注意：使用依赖属性值而非 ActualWidth/ActualHeight，避免首次布局未完成时取到 0。
    /// </summary>
    private void UpdateClosedPosition()
    {
        if (_translateTransform == null)
        {
            return;
        }

        switch (Placement)
        {
            case DrawerPlacement.Left:
                _translateTransform.X = -DrawerWidth;
                _translateTransform.Y = 0;
                break;

            case DrawerPlacement.Right:
                _translateTransform.X = DrawerWidth;
                _translateTransform.Y = 0;
                break;

            case DrawerPlacement.Top:
                _translateTransform.X = 0;
                _translateTransform.Y = -ResolvedDrawerHeight;
                break;

            case DrawerPlacement.Bottom:
                _translateTransform.X = 0;
                _translateTransform.Y = ResolvedDrawerHeight;
                break;
        }
    }

    /// <summary>
    /// 执行打开动画。
    /// </summary>
    private void AnimateToOpenPosition()
    {
        if (_translateTransform == null)
        {
            return;
        }

        var animation = new DoubleAnimation(0, AnimationDuration);
        animation.Completed += (_, _) => OnOpenCompleted();

        switch (Placement)
        {
            case DrawerPlacement.Left:
            case DrawerPlacement.Right:
                _translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                break;

            case DrawerPlacement.Top:
            case DrawerPlacement.Bottom:
                _translateTransform.BeginAnimation(TranslateTransform.YProperty, animation);
                break;
        }
    }

    /// <summary>
    /// 执行关闭动画。
    /// </summary>
    private void AnimateToClosedPosition()
    {
        if (_translateTransform == null)
        {
            return;
        }

        var animation = new DoubleAnimation
        {
            Duration = AnimationDuration,
        };
        animation.Completed += (_, _) => OnCloseCompleted();

        switch (Placement)
        {
            case DrawerPlacement.Left:
                animation.To = -DrawerWidth;
                _translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                break;

            case DrawerPlacement.Right:
                animation.To = DrawerWidth;
                _translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                break;

            case DrawerPlacement.Top:
                animation.To = -ResolvedDrawerHeight;
                _translateTransform.BeginAnimation(TranslateTransform.YProperty, animation);
                break;

            case DrawerPlacement.Bottom:
                animation.To = ResolvedDrawerHeight;
                _translateTransform.BeginAnimation(TranslateTransform.YProperty, animation);
                break;
        }
    }

    /// <summary>
    /// 更新遮罩层的可见性。
    /// </summary>
    /// <param name="animate">是否使用淡入淡出动画。</param>
    private void UpdateOverlayVisibility(bool animate)
    {
        if (_overlay == null)
        {
            return;
        }

        if (IsOpen)
        {
            _overlay.Visibility = Visibility.Visible;

            if (animate)
            {
                var animation = new DoubleAnimation(0, 1, AnimationDuration);
                _overlay.BeginAnimation(OpacityProperty, animation);
            }
            else
            {
                _overlay.BeginAnimation(OpacityProperty, null);
                _overlay.Opacity = 1;
            }
        }
        else
        {
            if (animate)
            {
                var animation = new DoubleAnimation(0, AnimationDuration);
                animation.Completed += (_, _) =>
                {
                    // 动画完成时再次确认状态，避免与新一次打开的竞态
                    if (!IsOpen && _overlay != null)
                    {
                        _overlay.Visibility = Visibility.Collapsed;
                    }
                };
                _overlay.BeginAnimation(OpacityProperty, animation);
            }
            else
            {
                _overlay.BeginAnimation(OpacityProperty, null);
                _overlay.Opacity = 0;
                _overlay.Visibility = Visibility.Collapsed;
            }
        }
    }

    /// <summary>
    /// 处理遮罩层点击事件。
    /// </summary>
    private void OnOverlayClick(object sender, MouseButtonEventArgs e)
    {
        if (CloseOnOverlayClick)
        {
            Close();
            e.Handled = true;
        }
    }

    /// <summary>
    /// 打开动画完成后处理：自动聚焦并触发 Opened 事件。
    /// </summary>
    private void OnOpenCompleted()
    {
        // 将焦点移入抽屉内的首个可聚焦元素
        if (FocusOnOpen && _drawerPanel != null && !_drawerPanel.IsKeyboardFocusWithin)
        {
            _drawerPanel.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }

        RaiseEvent(new RoutedEventArgs(OpenedEvent, this));
    }

    /// <summary>
    /// 关闭动画完成后处理：隐藏面板并触发 Closed 事件。
    /// </summary>
    private void OnCloseCompleted()
    {
        // 二次确认：动画完成时若用户已经再次 Open，不要把面板 Collapse 掉
        if (!IsOpen && _drawerPanel != null)
        {
            _drawerPanel.Visibility = Visibility.Collapsed;
        }

        RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
    }

    #endregion Private Methods
}

/// <summary>
/// 指定抽屉的放置位置。
/// </summary>
public enum DrawerPlacement
{
    /// <summary>从左侧滑入。</summary>
    Left,

    /// <summary>从右侧滑入。</summary>
    Right,

    /// <summary>从顶部滑入。</summary>
    Top,

    /// <summary>从底部滑入。</summary>
    Bottom
}