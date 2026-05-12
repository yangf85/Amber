using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 旋转角度编辑器，用于设置物件的 X / Y / Z 轴旋转角度。
/// </summary>
[TemplatePart(Name = nameof(PART_XSlider), Type = typeof(RangeBase))]
[TemplatePart(Name = nameof(PART_YSlider), Type = typeof(RangeBase))]
[TemplatePart(Name = nameof(PART_ZSlider), Type = typeof(RangeBase))]
public class RotationEditor : Control
{
    private const string PART_XSlider = "PART_XSlider";

    private const string PART_YSlider = "PART_YSlider";

    private const string PART_ZSlider = "PART_ZSlider";

    private RangeBase _xSlider;

    private RangeBase _ySlider;

    private RangeBase _zSlider;

    static RotationEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RotationEditor),
            new FrameworkPropertyMetadata(typeof(RotationEditor)));

        CommandManager.RegisterClassCommandBinding(
            typeof(RotationEditor),
            new CommandBinding(ResetCommand, OnResetExecuted));
    }

    #region DependencyProperties

    #region AngleX

    public static readonly DependencyProperty AngleXProperty =
        DependencyProperty.Register(
            nameof(AngleX),
            typeof(double),
            typeof(RotationEditor),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnAngleXChanged,
                CoerceAngle));

    /// <summary>
    /// 获取或设置 X 轴旋转角度。
    /// </summary>
    public double AngleX
    {
        get => (double)GetValue(AngleXProperty);
        set => SetValue(AngleXProperty, value);
    }

    private static void OnAngleXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (RotationEditor)d;
        control.RaiseAngleChanged("X", (double)e.OldValue, (double)e.NewValue);
    }

    #endregion AngleX

    #region AngleY

    public static readonly DependencyProperty AngleYProperty =
        DependencyProperty.Register(
            nameof(AngleY),
            typeof(double),
            typeof(RotationEditor),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnAngleYChanged,
                CoerceAngle));

    /// <summary>
    /// 获取或设置 Y 轴旋转角度。
    /// </summary>
    public double AngleY
    {
        get => (double)GetValue(AngleYProperty);
        set => SetValue(AngleYProperty, value);
    }

    private static void OnAngleYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (RotationEditor)d;
        control.RaiseAngleChanged("Y", (double)e.OldValue, (double)e.NewValue);
    }

    #endregion AngleY

    #region AngleZ

    public static readonly DependencyProperty AngleZProperty =
        DependencyProperty.Register(
            nameof(AngleZ),
            typeof(double),
            typeof(RotationEditor),
            new FrameworkPropertyMetadata(
                0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnAngleZChanged,
                CoerceAngle));

    /// <summary>
    /// 获取或设置 Z 轴旋转角度。
    /// </summary>
    public double AngleZ
    {
        get => (double)GetValue(AngleZProperty);
        set => SetValue(AngleZProperty, value);
    }

    private static void OnAngleZChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (RotationEditor)d;
        control.RaiseAngleChanged("Z", (double)e.OldValue, (double)e.NewValue);
    }

    #endregion AngleZ

    #region Step

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(RotationEditor),
            new PropertyMetadata(1.0));

    /// <summary>
    /// 获取或设置角度调整步长。
    /// </summary>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    #endregion Step

    #region AutoToolTipPlacement

    public static readonly DependencyProperty AutoToolTipPlacementProperty =
        DependencyProperty.Register(
            nameof(AutoToolTipPlacement),
            typeof(AutoToolTipPlacement),
            typeof(RotationEditor),
            new PropertyMetadata(AutoToolTipPlacement.TopLeft));

    /// <summary>
    /// 拖动 Thumb 时显示当前角度值的 ToolTip 位置,语义跟 <see cref="Slider.AutoToolTipPlacement"/> 一致。
    /// 默认 <see cref="System.Windows.Controls.Primitives.AutoToolTipPlacement.TopLeft"/>;
    /// 设 <see cref="System.Windows.Controls.Primitives.AutoToolTipPlacement.None"/> 关闭 ToolTip。
    /// </summary>
    public AutoToolTipPlacement AutoToolTipPlacement
    {
        get => (AutoToolTipPlacement)GetValue(AutoToolTipPlacementProperty);
        set => SetValue(AutoToolTipPlacementProperty, value);
    }

    #endregion AutoToolTipPlacement

    #region AutoToolTipPrecision

    public static readonly DependencyProperty AutoToolTipPrecisionProperty =
        DependencyProperty.Register(
            nameof(AutoToolTipPrecision),
            typeof(int),
            typeof(RotationEditor),
            new PropertyMetadata(0));

    /// <summary>
    /// 拖动 Thumb 时 ToolTip 显示角度值的小数位数,语义跟 <see cref="Slider.AutoToolTipPrecision"/> 一致。
    /// 默认 0(整数显示)。
    /// </summary>
    public int AutoToolTipPrecision
    {
        get => (int)GetValue(AutoToolTipPrecisionProperty);
        set => SetValue(AutoToolTipPrecisionProperty, value);
    }

    #endregion AutoToolTipPrecision

    #region LabelX

    public static readonly DependencyProperty LabelXProperty =
        DependencyProperty.Register(
            nameof(LabelX),
            typeof(object),
            typeof(RotationEditor),
            new PropertyMetadata("X"));

    /// <summary>
    /// 获取或设置 X 轴标签内容，支持任意对象（字符串、图标、自定义控件等）。
    /// </summary>
    public object LabelX
    {
        get => GetValue(LabelXProperty);
        set => SetValue(LabelXProperty, value);
    }

    #endregion LabelX

    #region LabelY

    public static readonly DependencyProperty LabelYProperty =
        DependencyProperty.Register(
            nameof(LabelY),
            typeof(object),
            typeof(RotationEditor),
            new PropertyMetadata("Y"));

    /// <summary>
    /// 获取或设置 Y 轴标签内容，支持任意对象（字符串、图标、自定义控件等）。
    /// </summary>
    public object LabelY
    {
        get => GetValue(LabelYProperty);
        set => SetValue(LabelYProperty, value);
    }

    #endregion LabelY

    #region LabelZ

    public static readonly DependencyProperty LabelZProperty =
        DependencyProperty.Register(
            nameof(LabelZ),
            typeof(object),
            typeof(RotationEditor),
            new PropertyMetadata("Z"));

    /// <summary>
    /// 获取或设置 Z 轴标签内容，支持任意对象（字符串、图标、自定义控件等）。
    /// </summary>
    public object LabelZ
    {
        get => GetValue(LabelZProperty);
        set => SetValue(LabelZProperty, value);
    }

    #endregion LabelZ

    #endregion DependencyProperties

    #region RoutedEvents

    public static readonly RoutedEvent AngleChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(AngleChanged),
            RoutingStrategy.Bubble,
            typeof(EventHandler<AngleChangedEventArgs>),
            typeof(RotationEditor));

    /// <summary>
    /// 任一轴角度变化后触发。
    /// </summary>
    public event EventHandler<AngleChangedEventArgs> AngleChanged
    {
        add => AddHandler(AngleChangedEvent, value);
        remove => RemoveHandler(AngleChangedEvent, value);
    }

    #endregion RoutedEvents

    #region Commands

    /// <summary>
    /// 重置所有轴角度为 0。
    /// </summary>
    public static readonly RoutedCommand ResetCommand = new(nameof(ResetCommand), typeof(RotationEditor));

    private static void OnResetExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var control = (RotationEditor)sender;
        control.AngleX = 0.0;
        control.AngleY = 0.0;
        control.AngleZ = 0.0;
    }

    #endregion Commands

    #region Override Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        DetachSliderEvents();

        _xSlider = GetTemplateChild(PART_XSlider) as RangeBase;
        _ySlider = GetTemplateChild(PART_YSlider) as RangeBase;
        _zSlider = GetTemplateChild(PART_ZSlider) as RangeBase;

        AttachSliderEvents();
        SyncSlidersFromProperties();
    }

    #endregion Override Methods

    #region Private Methods

    private static object CoerceAngle(DependencyObject d, object baseValue)
    {
        var angle = (double)baseValue;
        angle %= 360.0;
        if (angle < 0)
        {
            angle += 360.0;
        }
        return angle;
    }

    private void AttachSliderEvents()
    {
        _xSlider?.ValueChanged += OnXSliderValueChanged;
        _ySlider?.ValueChanged += OnYSliderValueChanged;
        _zSlider?.ValueChanged += OnZSliderValueChanged;
    }

    private void DetachSliderEvents()
    {
        _xSlider?.ValueChanged -= OnXSliderValueChanged;
        _ySlider?.ValueChanged -= OnYSliderValueChanged;
        _zSlider?.ValueChanged -= OnZSliderValueChanged;
    }

    private void OnXSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        AngleX = e.NewValue;
    }

    private void OnYSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        AngleY = e.NewValue;
    }

    private void OnZSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        AngleZ = e.NewValue;
    }

    private void RaiseAngleChanged(string axis, double oldValue, double newValue)
    {
        RaiseEvent(new AngleChangedEventArgs(AngleChangedEvent, this, axis, oldValue, newValue));
    }

    private void SyncSlidersFromProperties()
    {
        _xSlider?.Value = AngleX;
        _ySlider?.Value = AngleY;
        _zSlider?.Value = AngleZ;
    }

    #endregion Private Methods
}

/// <summary>
/// AngleChanged 路由事件参数。
/// </summary>
public class AngleChangedEventArgs : RoutedEventArgs
{
    public string Axis { get; }

    public double NewValue { get; }

    public double OldValue { get; }

    public AngleChangedEventArgs(RoutedEvent routedEvent, object source, string axis, double oldValue, double newValue)
        : base(routedEvent, source)
    {
        Axis = axis;
        OldValue = oldValue;
        NewValue = newValue;
    }
}