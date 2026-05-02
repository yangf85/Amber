using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 范围滑块——两个 Thumb 表示一段数值区间 [<see cref="LowerValue"/>, <see cref="UpperValue"/>]。
/// <para>
/// 不继承 <see cref="RangeBase"/>（保持职责清晰，Min/Max 自管），但仿照 <see cref="Slider"/> 的标准模式：
/// AutoToolTip / 键盘交互 / DragDelta / RepeatButton 三段式 track。
/// </para>
/// </summary>
[TemplatePart(Name = PART_StartThumb, Type = typeof(Thumb))]
[TemplatePart(Name = PART_EndThumb, Type = typeof(Thumb))]
[TemplatePart(Name = PART_StartRegion, Type = typeof(RepeatButton))]
[TemplatePart(Name = PART_MiddleRegion, Type = typeof(RepeatButton))]
[TemplatePart(Name = PART_EndRegion, Type = typeof(RepeatButton))]
public class RangeSlider : Control
{
    private const string PART_StartThumb = nameof(PART_StartThumb);
    private const string PART_EndThumb = nameof(PART_EndThumb);
    private const string PART_StartRegion = nameof(PART_StartRegion);
    private const string PART_MiddleRegion = nameof(PART_MiddleRegion);
    private const string PART_EndRegion = nameof(PART_EndRegion);

    private Thumb _startThumb;
    private Thumb _endThumb;
    private RepeatButton _startRegion;
    private RepeatButton _middleRegion;
    private RepeatButton _endRegion;

    // 拖动时按需创建的 ToolTip——标准 WPF Slider 模式
    private ToolTip _autoToolTip;

    // 当前键盘焦点所在的 thumb——决定方向键移动哪个
    private ThumbKind _focusedThumb = ThumbKind.Start;

    // 缓存的刻度线值，避免每次拖动都重算
    private List<double> _cachedTickValues;

    private bool _suppressSync;

    private enum ThumbKind { Start, End }

    #region Constructors

    static RangeSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(typeof(RangeSlider)));

        // 类级别 Thumb 拖动事件——同一个 RangeSlider 内的两个 Thumb 共用入口
        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragStartedEvent, new DragStartedEventHandler(OnDragStartedClass));
        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDeltaClass));
        EventManager.RegisterClassHandler(typeof(RangeSlider), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompletedClass));
    }

    #endregion Constructors

    #region Minimum

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(
                0d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                OnMinimumChanged));

    /// <summary>区间最小值。默认 0。</summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        // Maximum 至少要 ≥ Minimum
        slider.CoerceValue(MaximumProperty);
        slider.CoerceValue(LowerValueProperty);
        slider.CoerceValue(UpperValueProperty);
        slider.InvalidateTickCache();
    }

    #endregion Minimum

    #region Maximum

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(
                100d,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                OnMaximumChanged,
                CoerceMaximum));

    /// <summary>区间最大值。默认 100。会被 coerce 到不小于 <see cref="Minimum"/>。</summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private static object CoerceMaximum(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        var value = (double)baseValue;
        return Math.Max(value, slider.Minimum);
    }

    private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        slider.CoerceValue(LowerValueProperty);
        slider.CoerceValue(UpperValueProperty);
        slider.InvalidateTickCache();
    }

    #endregion Maximum

    #region LowerValue

    public static readonly DependencyProperty LowerValueProperty =
        DependencyProperty.Register(
            nameof(LowerValue),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(
                0d,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnLowerValueChanged,
                CoerceLowerValue));

    /// <summary>区间下界。默认 0。会被 coerce 到 [Minimum, UpperValue] 范围内。</summary>
    public double LowerValue
    {
        get => (double)GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    private static object CoerceLowerValue(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        var value = (double)baseValue;
        // [Minimum, UpperValue]
        return Math.Min(Math.Max(value, slider.Minimum), slider.UpperValue);
    }

    private static void OnLowerValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        var args = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, LowerValueChangedEvent);
        slider.RaiseEvent(args);
    }

    #endregion LowerValue

    #region UpperValue

    public static readonly DependencyProperty UpperValueProperty =
        DependencyProperty.Register(
            nameof(UpperValue),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(
                100d,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnUpperValueChanged,
                CoerceUpperValue));

    /// <summary>区间上界。默认 100。会被 coerce 到 [LowerValue, Maximum] 范围内。</summary>
    public double UpperValue
    {
        get => (double)GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    private static object CoerceUpperValue(DependencyObject d, object baseValue)
    {
        var slider = (RangeSlider)d;
        var value = (double)baseValue;
        // [LowerValue, Maximum]
        return Math.Min(Math.Max(value, slider.LowerValue), slider.Maximum);
    }

    private static void OnUpperValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        var args = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue, UpperValueChangedEvent);
        slider.RaiseEvent(args);
    }

    #endregion UpperValue

    #region Step

    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(1.0, OnStepChanged),
            ValidateStep);

    /// <summary>方向键和 IsSnapToStep 使用的步进值。必须 > 0。默认 1。</summary>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    private static bool ValidateStep(object value) => value is double d && d > 0;

    private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var slider = (RangeSlider)d;
        if (slider.IsSnapToStep)
        {
            slider.CoerceValueToStep();
        }
    }

    #endregion Step

    #region LargeChange

    public static readonly DependencyProperty LargeChangeProperty =
        DependencyProperty.Register(
            nameof(LargeChange),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(10.0),
            ValidateLargeChange);

    /// <summary>PageUp / PageDown 的步进。必须 > 0。默认 10。</summary>
    public double LargeChange
    {
        get => (double)GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    private static bool ValidateLargeChange(object value) => value is double d && d > 0;

    #endregion LargeChange

    #region TrackThickness

    public static readonly DependencyProperty TrackThicknessProperty =
        DependencyProperty.Register(
            nameof(TrackThickness),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(5.0));

    /// <summary>轨道厚度（水平模式 = 高度，垂直模式 = 宽度）。默认 5。</summary>
    public double TrackThickness
    {
        get => (double)GetValue(TrackThicknessProperty);
        set => SetValue(TrackThicknessProperty, value);
    }

    #endregion TrackThickness

    #region InactiveTrackBrush

    public static readonly DependencyProperty InactiveTrackBrushProperty =
        DependencyProperty.Register(
            nameof(InactiveTrackBrush),
            typeof(Brush),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(default(Brush)));

    /// <summary>轨道两端（区间外）的背景刷。默认 null，由模板 Style 提供。</summary>
    public Brush InactiveTrackBrush
    {
        get => (Brush)GetValue(InactiveTrackBrushProperty);
        set => SetValue(InactiveTrackBrushProperty, value);
    }

    #endregion InactiveTrackBrush

    #region ActiveTrackBrush

    public static readonly DependencyProperty ActiveTrackBrushProperty =
        DependencyProperty.Register(
            nameof(ActiveTrackBrush),
            typeof(Brush),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(default(Brush)));

    /// <summary>轨道中段（区间内）的背景刷。默认 null，由模板 Style 提供。</summary>
    public Brush ActiveTrackBrush
    {
        get => (Brush)GetValue(ActiveTrackBrushProperty);
        set => SetValue(ActiveTrackBrushProperty, value);
    }

    #endregion ActiveTrackBrush

    #region Orientation

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(Orientation.Horizontal));

    /// <summary>方向。<see cref="Orientation.Horizontal"/> = 横向，<see cref="Orientation.Vertical"/> = 纵向。</summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion Orientation

    #region ThumbSize

    public static readonly DependencyProperty ThumbSizeProperty =
        DependencyProperty.Register(
            nameof(ThumbSize),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(
                16d,
                FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// thumb 在主轴方向上的尺寸(水平=宽,垂直=高)。
    /// 同时决定 TickBar 两端的预留空间(<see cref="TickBar.ReservedSpace"/>),
    /// 保证 thumb 中心和刻度始终对齐。默认 16。
    /// </summary>
    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }

    #endregion ThumbSize

    #region IsDirectionReversed

    public static readonly DependencyProperty IsDirectionReversedProperty =
        DependencyProperty.Register(
            nameof(IsDirectionReversed),
            typeof(bool),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// 反转方向。<c>false</c>（默认）：水平=左→右递增，垂直=下→上递增；
    /// <c>true</c>：水平=右→左递增，垂直=上→下递增。
    /// </summary>
    public bool IsDirectionReversed
    {
        get => (bool)GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    #endregion IsDirectionReversed

    #region IsMoveToPoint

    public static readonly DependencyProperty IsMoveToPointProperty =
        DependencyProperty.Register(
            nameof(IsMoveToPoint),
            typeof(bool),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(true));

    /// <summary>
    /// 点击中段轨道时的行为：
    /// <c>true</c> = 离哪个 thumb 近就把它移到点击点；
    /// <c>false</c> = 按 LargeChange 步进（左键扩展 lower，右键扩展 upper）。
    /// 默认 true。
    /// </summary>
    public bool IsMoveToPoint
    {
        get => (bool)GetValue(IsMoveToPointProperty);
        set => SetValue(IsMoveToPointProperty, value);
    }

    #endregion IsMoveToPoint

    #region IsSnapToStep

    public static readonly DependencyProperty IsSnapToStepProperty =
        DependencyProperty.Register(
            nameof(IsSnapToStep),
            typeof(bool),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(true, OnIsSnapToStepChanged));

    /// <summary>是否对齐到 <see cref="Step"/> 的整数倍。默认 true。和 <see cref="IsSnapToTick"/> 同时为 true 时刻度线优先。</summary>
    public bool IsSnapToStep
    {
        get => (bool)GetValue(IsSnapToStepProperty);
        set => SetValue(IsSnapToStepProperty, value);
    }

    private static void OnIsSnapToStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue && d is RangeSlider slider)
        {
            slider.CoerceValueToStep();
        }
    }

    #endregion IsSnapToStep

    #region IsSnapToTick

    public static readonly DependencyProperty IsSnapToTickProperty =
        DependencyProperty.Register(
            nameof(IsSnapToTick),
            typeof(bool),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(false, OnIsSnapToTickChanged));

    /// <summary>是否对齐到刻度线（<see cref="Ticks"/> 或 <see cref="TickFrequency"/>）。默认 false。</summary>
    public bool IsSnapToTick
    {
        get => (bool)GetValue(IsSnapToTickProperty);
        set => SetValue(IsSnapToTickProperty, value);
    }

    private static void OnIsSnapToTickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if ((bool)e.NewValue && d is RangeSlider slider)
        {
            slider.CoerceValueToTick();
        }
    }

    #endregion IsSnapToTick

    #region TickFrequency

    public static readonly DependencyProperty TickFrequencyProperty =
        DependencyProperty.Register(
            nameof(TickFrequency),
            typeof(double),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(1.0, OnTickFrequencyChanged));

    /// <summary>等距刻度线的频率。仅在 <see cref="Ticks"/> 为空时生效。默认 1。</summary>
    public double TickFrequency
    {
        get => (double)GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    private static void OnTickFrequencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RangeSlider slider) slider.InvalidateTickCache();
    }

    #endregion TickFrequency

    #region Ticks

    public static readonly DependencyProperty TicksProperty =
        DependencyProperty.Register(
            nameof(Ticks),
            typeof(DoubleCollection),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(default(DoubleCollection), OnTicksChanged));

    /// <summary>自定义刻度线。设了之后 <see cref="TickFrequency"/> 失效。</summary>
    public DoubleCollection Ticks
    {
        get => (DoubleCollection)GetValue(TicksProperty);
        set => SetValue(TicksProperty, value);
    }

    private static void OnTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RangeSlider slider) slider.InvalidateTickCache();
    }

    #endregion Ticks

    #region TickPlacement

    public static readonly DependencyProperty TickPlacementProperty =
        DependencyProperty.Register(
            nameof(TickPlacement),
            typeof(TickPlacement),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(TickPlacement.None));

    /// <summary>刻度线显示位置。</summary>
    public TickPlacement TickPlacement
    {
        get => (TickPlacement)GetValue(TickPlacementProperty);
        set => SetValue(TickPlacementProperty, value);
    }

    #endregion TickPlacement

    #region AutoToolTipPlacement

    public static readonly DependencyProperty AutoToolTipPlacementProperty =
        DependencyProperty.Register(
            nameof(AutoToolTipPlacement),
            typeof(AutoToolTipPlacement),
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(AutoToolTipPlacement.None));

    /// <summary>拖动 thumb 时显示数值 tooltip 的位置。<see cref="AutoToolTipPlacement.None"/> = 不显示。</summary>
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
            typeof(RangeSlider),
            new FrameworkPropertyMetadata(1),
            ValidateAutoToolTipPrecision);

    /// <summary>auto tooltip 数值显示的小数位数。必须 ≥ 0。默认 1。</summary>
    public int AutoToolTipPrecision
    {
        get => (int)GetValue(AutoToolTipPrecisionProperty);
        set => SetValue(AutoToolTipPrecisionProperty, value);
    }

    private static bool ValidateAutoToolTipPrecision(object value) => value is int i && i >= 0;

    #endregion AutoToolTipPrecision

    #region RoutedEvents

    public static readonly RoutedEvent LowerValueChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(LowerValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(RangeSlider));

    public static readonly RoutedEvent UpperValueChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(UpperValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(RangeSlider));

    public event RoutedPropertyChangedEventHandler<double> LowerValueChanged
    {
        add => AddHandler(LowerValueChangedEvent, value);
        remove => RemoveHandler(LowerValueChangedEvent, value);
    }

    public event RoutedPropertyChangedEventHandler<double> UpperValueChanged
    {
        add => AddHandler(UpperValueChangedEvent, value);
        remove => RemoveHandler(UpperValueChangedEvent, value);
    }

    #endregion RoutedEvents

    #region Override (FrameworkElement)

    /// <summary>把键映射到 ±1 方向因子（考虑 IsDirectionReversed）。返回 0 表示不是移动键。</summary>
    private int GetKeyboardMoveDirection(Key key)
    {
        // base direction = 是 + 还是 - 方向
        int sign = key switch
        {
            Key.Right or Key.Up or Key.PageUp or Key.End => +1,
            Key.Left or Key.Down or Key.PageDown or Key.Home => -1,
            _ => 0,
        };
        if (sign == 0) return 0;
        return IsDirectionReversed ? -sign : sign;
    }

    /// <summary>把键盘 delta 应用到当前焦点 thumb 上。</summary>
    private void ApplyKeyboardDelta(double delta)
    {
        if (_focusedThumb == ThumbKind.Start)
        {
            var newValue = double.IsInfinity(delta)
                ? (delta < 0 ? Minimum : UpperValue)
                : LowerValue + delta;
            SetLowerValueSnapped(newValue);
        }
        else
        {
            var newValue = double.IsInfinity(delta)
                ? (delta < 0 ? LowerValue : Maximum)
                : UpperValue + delta;
            SetUpperValueSnapped(newValue);
        }
    }

    /// <summary>
    /// 键盘交互：
    /// <list type="bullet">
    /// <item>Left/Down：当前焦点 thumb -Step</item>
    /// <item>Right/Up：当前焦点 thumb +Step</item>
    /// <item>PageDown：当前焦点 thumb -LargeChange</item>
    /// <item>PageUp：当前焦点 thumb +LargeChange</item>
    /// <item>Home：当前焦点 thumb 跳到 Min（Start）或 Max（End）</item>
    /// <item>End：当前焦点 thumb 跳到 LowerValue 边界（Start）或 UpperValue 边界（End）</item>
    /// </list>
    /// IsDirectionReversed 时左右/上下键含义自动反转。
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        var direction = GetKeyboardMoveDirection(e.Key);
        if (direction == 0)
        {
            base.OnKeyDown(e);
            return;
        }

        // 计算位移大小
        double delta = e.Key switch
        {
            Key.PageUp or Key.PageDown => LargeChange * direction,
            Key.Home => double.NegativeInfinity * direction,
            Key.End => double.PositiveInfinity * direction,
            _ => Step * direction,
        };

        ApplyKeyboardDelta(delta);
        e.Handled = true;
    }

    public override void OnApplyTemplate()
    {
        // 解除旧引用的事件订阅（OnApplyTemplate 可能被调用多次，比如换模板）
        if (_startRegion is not null)
        {
            _startRegion.Click -= OnStartRegionClick;
        }
        if (_endRegion is not null)
        {
            _endRegion.Click -= OnEndRegionClick;
        }
        if (_middleRegion is not null)
        {
            _middleRegion.PreviewMouseLeftButtonDown -= OnMiddleRegionPreviewMouseLeftButtonDown;
            _middleRegion.PreviewMouseRightButtonDown -= OnMiddleRegionPreviewMouseRightButtonDown;
        }
        if (_startThumb is not null)
        {
            _startThumb.GotKeyboardFocus -= OnStartThumbGotKeyboardFocus;
        }
        if (_endThumb is not null)
        {
            _endThumb.GotKeyboardFocus -= OnEndThumbGotKeyboardFocus;
        }

        base.OnApplyTemplate();

        _startThumb = GetTemplateChild(PART_StartThumb) as Thumb;
        _endThumb = GetTemplateChild(PART_EndThumb) as Thumb;
        _startRegion = GetTemplateChild(PART_StartRegion) as RepeatButton;
        _middleRegion = GetTemplateChild(PART_MiddleRegion) as RepeatButton;
        _endRegion = GetTemplateChild(PART_EndRegion) as RepeatButton;

        if (_startRegion is not null)
        {
            _startRegion.Click += OnStartRegionClick;
        }
        if (_endRegion is not null)
        {
            _endRegion.Click += OnEndRegionClick;
        }
        if (_middleRegion is not null)
        {
            _middleRegion.PreviewMouseLeftButtonDown += OnMiddleRegionPreviewMouseLeftButtonDown;
            _middleRegion.PreviewMouseRightButtonDown += OnMiddleRegionPreviewMouseRightButtonDown;
        }
        if (_startThumb is not null)
        {
            _startThumb.GotKeyboardFocus += OnStartThumbGotKeyboardFocus;
        }
        if (_endThumb is not null)
        {
            _endThumb.GotKeyboardFocus += OnEndThumbGotKeyboardFocus;
        }
    }

    #endregion Override (FrameworkElement)

    #region Drag (Thumb)

    private static void OnDragStartedClass(object sender, DragStartedEventArgs e)
    {
        if (sender is RangeSlider rs) rs.OnDragStarted(e);
    }

    private static void OnDragDeltaClass(object sender, DragDeltaEventArgs e)
    {
        if (sender is RangeSlider rs) rs.OnDragDelta(e);
    }

    private static void OnDragCompletedClass(object sender, DragCompletedEventArgs e)
    {
        if (sender is RangeSlider rs) rs.OnDragCompleted(e);
    }

    private void OnDragCompleted(DragCompletedEventArgs e)
    {
        if (_autoToolTip is not null)
        {
            _autoToolTip.IsOpen = false;
        }
    }

    private void OnDragStarted(DragStartedEventArgs e)
    {
        if (AutoToolTipPlacement == AutoToolTipPlacement.None) return;

        // 标准 WPF Slider 模式：拖动时按需创建 ToolTip
        EnsureAutoToolTip();
        var thumb = e.OriginalSource as Thumb;
        if (thumb is null) return;

        _autoToolTip.PlacementTarget = thumb;
        _autoToolTip.Placement = GetAutoToolTipPlacement();

        var value = thumb == _startThumb ? LowerValue : UpperValue;
        _autoToolTip.Content = FormatToolTipValue(value);
        _autoToolTip.IsOpen = true;
    }

    private void OnDragDelta(DragDeltaEventArgs e)
    {
        var thumb = e.OriginalSource as Thumb;
        if (thumb is null || !TryGetTrackTotalSize(out double total) || total <= 0) return;

        // 把像素位移转成数值——只用 track 总长度（不含 padding/border）
        var range = Maximum - Minimum;
        var pixelDelta = Orientation == Orientation.Horizontal ? e.HorizontalChange : -e.VerticalChange;
        if (IsDirectionReversed) pixelDelta = -pixelDelta;
        var valueDelta = pixelDelta / total * range;

        if (thumb == _startThumb)
        {
            SetLowerValueSnapped(LowerValue + valueDelta);
            UpdateAutoToolTip(LowerValue);
        }
        else if (thumb == _endThumb)
        {
            SetUpperValueSnapped(UpperValue + valueDelta);
            UpdateAutoToolTip(UpperValue);
        }
    }

    #endregion Drag (Thumb)

    #region Region click (RepeatButton)

    private void OnStartRegionClick(object sender, RoutedEventArgs e)
    {
        // 点击左侧（小值）轨道：LowerValue 减 LargeChange
        var direction = IsDirectionReversed ? +1 : -1;
        SetLowerValueSnapped(LowerValue + LargeChange * direction);
    }

    private void OnEndRegionClick(object sender, RoutedEventArgs e)
    {
        // 点击右侧（大值）轨道：UpperValue 加 LargeChange
        var direction = IsDirectionReversed ? -1 : +1;
        SetUpperValueSnapped(UpperValue + LargeChange * direction);
    }

    private void OnMiddleRegionPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsMoveToPoint)
        {
            // LargeChange 步进——左键扩展下界
            SetLowerValueSnapped(LowerValue + (IsDirectionReversed ? +LargeChange : -LargeChange));
            e.Handled = true;
            return;
        }

        // 点击位置 → 数值；离哪个 thumb 近就移哪个
        if (TryPointToValue(e.GetPosition(this), out double targetValue))
        {
            var distToLower = Math.Abs(targetValue - LowerValue);
            var distToUpper = Math.Abs(targetValue - UpperValue);
            if (distToLower <= distToUpper)
            {
                SetLowerValueSnapped(targetValue);
            }
            else
            {
                SetUpperValueSnapped(targetValue);
            }
        }
        e.Handled = true;
    }

    private void OnMiddleRegionPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsMoveToPoint)
        {
            // LargeChange 步进——右键扩展上界
            SetUpperValueSnapped(UpperValue + (IsDirectionReversed ? -LargeChange : +LargeChange));
            e.Handled = true;
        }
        // IsMoveToPoint=true 时右键不响应（避免和左键行为冲突）
    }

    #endregion Region click (RepeatButton)

    #region Focus tracking

    private void OnStartThumbGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => _focusedThumb = ThumbKind.Start;

    private void OnEndThumbGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => _focusedThumb = ThumbKind.End;

    #endregion Focus tracking

    #region Value setting helpers

    private static double RoundToStep(double value, double step)
    {
        return step <= 0 ? value : Math.Round(value / step) * step;
    }

    /// <summary>设置 LowerValue 并应用 snap（刻度线优先 / 步进次之）。</summary>
    private void SetLowerValueSnapped(double newValue)
    {
        // clamp 到 [Minimum, UpperValue]
        newValue = Math.Min(Math.Max(newValue, Minimum), UpperValue);
        newValue = Snap(newValue);
        if (Math.Abs(newValue - LowerValue) > double.Epsilon)
        {
            LowerValue = newValue;
        }
    }

    /// <summary>设置 UpperValue 并应用 snap。</summary>
    private void SetUpperValueSnapped(double newValue)
    {
        // clamp 到 [LowerValue, Maximum]
        newValue = Math.Min(Math.Max(newValue, LowerValue), Maximum);
        newValue = Snap(newValue);
        if (Math.Abs(newValue - UpperValue) > double.Epsilon)
        {
            UpperValue = newValue;
        }
    }

    /// <summary>按当前 Snap 设置对值取整：刻度线优先，其次步进，否则原样返回。</summary>
    private double Snap(double value)
    {
        if (IsSnapToTick)
        {
            return SnapToNearestTick(value);
        }
        if (IsSnapToStep && Step > 0)
        {
            return RoundToStep(value);
        }
        return value;
    }

    private double RoundToStep(double value) => RoundToStep(value, Step);

    private double SnapToNearestTick(double value)
    {
        var ticks = GetCachedTickValues();
        if (ticks.Count == 0) return value;

        // 二分查找——刻度线已排序
        int lo = 0, hi = ticks.Count - 1;
        while (lo < hi)
        {
            int mid = (lo + hi) / 2;
            if (ticks[mid] < value) lo = mid + 1;
            else hi = mid;
        }
        // lo 指向第一个 ≥ value 的；比较前后两个取近的
        int candidate = lo;
        if (lo > 0 && Math.Abs(ticks[lo - 1] - value) < Math.Abs(ticks[lo] - value))
        {
            candidate = lo - 1;
        }
        return ticks[candidate];
    }

    private void CoerceValueToStep()
    {
        _suppressSync = true;
        try
        {
            LowerValue = RoundToStep(LowerValue);
            UpperValue = RoundToStep(UpperValue);
        }
        finally
        {
            _suppressSync = false;
        }
    }

    private void CoerceValueToTick()
    {
        _suppressSync = true;
        try
        {
            LowerValue = SnapToNearestTick(LowerValue);
            UpperValue = SnapToNearestTick(UpperValue);
        }
        finally
        {
            _suppressSync = false;
        }
    }

    #endregion Value setting helpers

    #region Tick cache

    /// <summary>取当前所有刻度线的值，已排序、去重、被缓存。Min/Max/Ticks/TickFrequency 变化时通过 <see cref="InvalidateTickCache"/> 失效。</summary>
    private List<double> GetCachedTickValues()
    {
        if (_cachedTickValues is not null) return _cachedTickValues;

        var list = new List<double>();
        if (Ticks is { Count: > 0 } customTicks)
        {
            foreach (var t in customTicks)
            {
                if (t >= Minimum && t <= Maximum) list.Add(t);
            }
        }
        else if (TickFrequency > 0)
        {
            for (double v = Minimum; v <= Maximum; v += TickFrequency)
            {
                list.Add(v);
            }
            if (list.Count == 0 || Math.Abs(list[list.Count - 1] - Maximum) > double.Epsilon)
            {
                list.Add(Maximum);
            }
        }
        list.Sort();
        _cachedTickValues = list;
        return list;
    }

    private void InvalidateTickCache() => _cachedTickValues = null;

    #endregion Tick cache

    #region Layout helpers

    /// <summary>取 track 总长度（三段 RepeatButton 的合计）。</summary>
    private bool TryGetTrackTotalSize(out double total)
    {
        total = 0;
        if (_startRegion is null || _middleRegion is null || _endRegion is null) return false;
        total = Orientation == Orientation.Horizontal
            ? _startRegion.ActualWidth + _middleRegion.ActualWidth + _endRegion.ActualWidth
            : _startRegion.ActualHeight + _middleRegion.ActualHeight + _endRegion.ActualHeight;
        return total > 0;
    }

    /// <summary>把鼠标坐标映射到对应的数值。</summary>
    private bool TryPointToValue(Point pt, out double value)
    {
        value = 0;
        if (!TryGetTrackTotalSize(out double total)) return false;

        // 鼠标坐标——this 内的坐标，需要减去 _startRegion 起点
        // 但 _startRegion 总在控件最前部，所以位置等于 thumb 起点
        var pos = Orientation == Orientation.Horizontal ? pt.X : pt.Y;

        // 限定到 track 范围
        pos = Math.Min(Math.Max(pos, 0), total);

        var ratio = pos / total;
        if (Orientation == Orientation.Vertical)
        {
            // 垂直默认是从下到上递增——上方 y 小、对应大值
            ratio = 1 - ratio;
        }
        if (IsDirectionReversed) ratio = 1 - ratio;

        value = Minimum + ratio * (Maximum - Minimum);
        return true;
    }

    #endregion Layout helpers

    #region AutoToolTip helpers

    private void EnsureAutoToolTip()
    {
        if (_autoToolTip is null)
        {
            _autoToolTip = new ToolTip
            {
                // 一些与父级 ToolTipService 行为隔离的设置
                StaysOpen = true,
            };
        }
    }

    private void UpdateAutoToolTip(double value)
    {
        if (_autoToolTip is null || !_autoToolTip.IsOpen) return;
        _autoToolTip.Content = FormatToolTipValue(value);
        // 微移触发重新定位（标准 Slider 内部也这么做）
        var nudge = 0.001;
        if (Orientation == Orientation.Horizontal)
        {
            _autoToolTip.HorizontalOffset += nudge;
            _autoToolTip.HorizontalOffset -= nudge;
        }
        else
        {
            _autoToolTip.VerticalOffset += nudge;
            _autoToolTip.VerticalOffset -= nudge;
        }
    }

    private string FormatToolTipValue(double value)
        => value.ToString($"F{AutoToolTipPrecision}", System.Globalization.CultureInfo.CurrentCulture);

    private PlacementMode GetAutoToolTipPlacement()
    {
        return Orientation == Orientation.Horizontal
            ? AutoToolTipPlacement switch
            {
                AutoToolTipPlacement.TopLeft => PlacementMode.Top,
                AutoToolTipPlacement.BottomRight => PlacementMode.Bottom,
                _ => PlacementMode.Top,
            }
            : AutoToolTipPlacement switch
            {
                AutoToolTipPlacement.TopLeft => PlacementMode.Left,
                AutoToolTipPlacement.BottomRight => PlacementMode.Right,
                _ => PlacementMode.Left,
            };
    }

    #endregion AutoToolTip helpers
}