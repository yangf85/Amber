using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 圆形仪表盘控件 — 用指针在弧形刻度盘上指示 Value。
/// <para>
/// 默认弧形 270°(底部对称开口,跟汽车仪表盘类似),通过 <see cref="SweepAngle"/> 调整跨度。
/// 0° = 顶部 12 点钟方向,符合用户直觉。
/// </para>
/// <para>功能特性:</para>
/// <list type="bullet">
/// <item><description><see cref="Ranges"/> 分段染色(红/黄/绿区)</description></item>
/// <item><description>Value 改变指针平滑动画(<see cref="AnimationDuration"/>)</description></item>
/// <item><description>键盘操作:↑↓ ±SmallChange / PgUp/Dn ±LargeChange / Home/End</description></item>
/// <item><description>鼠标拖动调整 Value(<see cref="IsDraggable"/>)</description></item>
/// <item><description><see cref="ValueStringFormat"/> 自定义数值格式 / <see cref="Unit"/> 单位文本</description></item>
/// </list>
/// </summary>
[TemplatePart(Name = PART_PointerRotation, Type = typeof(RotateTransform))]
public class CircularGauge : RangeBase
{
    private const string PART_PointerRotation = nameof(PART_PointerRotation);

    private bool _isDragging;

    private Pen _longTickPen;

    private RotateTransform _pointerRotation;

    // 缓存 — 避免每次 OnRender 重建对象
    private Pen _shortTickPen;

    private bool _suppressAnimation;

    private Typeface _typeface;

    #region Constructors

    static CircularGauge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CircularGauge),
            new FrameworkPropertyMetadata(typeof(CircularGauge)));

        WidthProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(160d));
        HeightProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(160d));

        // RangeBase 默认 Maximum=1,仪表盘改 100
        MaximumProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(100d));
        LargeChangeProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(10d));
        SmallChangeProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(1d));

        FocusableProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(true));

        // 只读 DP 注册放在 cctor 里 — 避免字段初始化器顺序问题
        PointerAnglePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(PointerAngle),
            typeof(double),
            typeof(CircularGauge),
            new PropertyMetadata(0d));
        PointerAngleProperty = PointerAnglePropertyKey.DependencyProperty;

        DisplayTextPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(DisplayText),
            typeof(string),
            typeof(CircularGauge),
            new PropertyMetadata(string.Empty));
        DisplayTextProperty = DisplayTextPropertyKey.DependencyProperty;
    }

    public CircularGauge()
    {
        Ranges = new ObservableCollection<GaugeRange>();
    }

    #endregion Constructors

    #region DependencyProperties

    #region SweepAngle

    public static readonly DependencyProperty SweepAngleProperty =
        DependencyProperty.Register(
            nameof(SweepAngle),
            typeof(double),
            typeof(CircularGauge),
            new FrameworkPropertyMetadata(270d, OnScaleParamChanged),
            ValidateSweepAngle);

    /// <summary>
    /// 弧形跨度(度),默认 270°(底部对称开口)。
    /// 0° = 顶部 12 点钟,顺时针展开 SweepAngle 度。取值 (0, 360]。
    /// </summary>
    public double SweepAngle
    {
        get => (double)GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    private static bool ValidateSweepAngle(object value)
    {
        var v = (double)value;
        return v > 0 && v <= 360;
    }

    #endregion SweepAngle

    #region TickBrush

    public static readonly DependencyProperty TickBrushProperty =
        DependencyProperty.Register(
            nameof(TickBrush),
            typeof(Brush),
            typeof(CircularGauge),
            new FrameworkPropertyMetadata(Brushes.Gray, OnScaleParamChanged));

    public Brush TickBrush
    {
        get => (Brush)GetValue(TickBrushProperty);
        set => SetValue(TickBrushProperty, value);
    }

    #endregion TickBrush

    #region LabelFontSize

    public static readonly DependencyProperty LabelFontSizeProperty =
        DependencyProperty.Register(
            nameof(LabelFontSize),
            typeof(double),
            typeof(CircularGauge),
            new FrameworkPropertyMetadata(10d, OnScaleParamChanged));

    public double LabelFontSize
    {
        get => (double)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    #endregion LabelFontSize

    #region TickLengthRatio

    public static readonly DependencyProperty TickLengthRatioProperty =
        DependencyProperty.Register(
            nameof(TickLengthRatio),
            typeof(double),
            typeof(CircularGauge),
            new FrameworkPropertyMetadata(0.05, OnScaleParamChanged));

    /// <summary>短刻度长度 / 半径,默认 0.05。</summary>
    public double TickLengthRatio
    {
        get => (double)GetValue(TickLengthRatioProperty);
        set => SetValue(TickLengthRatioProperty, value);
    }

    #endregion TickLengthRatio

    #region LongTickRatio

    public static readonly DependencyProperty LongTickRatioProperty =
        DependencyProperty.Register(
            nameof(LongTickRatio),
            typeof(double),
            typeof(CircularGauge),
            new FrameworkPropertyMetadata(2.0, OnScaleParamChanged));

    /// <summary>长刻度 / 短刻度比例,默认 2.0。</summary>
    public double LongTickRatio
    {
        get => (double)GetValue(LongTickRatioProperty);
        set => SetValue(LongTickRatioProperty, value);
    }

    #endregion LongTickRatio

    #region RangeRingThickness

    public static readonly DependencyProperty RangeRingThicknessProperty =
        DependencyProperty.Register(
            nameof(RangeRingThickness),
            typeof(double),
            typeof(CircularGauge),
            new FrameworkPropertyMetadata(6d, OnScaleParamChanged));

    /// <summary>分段染色环厚度(像素),仅 <see cref="Ranges"/> 非空时有效。</summary>
    public double RangeRingThickness
    {
        get => (double)GetValue(RangeRingThicknessProperty);
        set => SetValue(RangeRingThicknessProperty, value);
    }

    #endregion RangeRingThickness

    #region Ranges

    public static readonly DependencyProperty RangesProperty =
        DependencyProperty.Register(
            nameof(Ranges),
            typeof(ObservableCollection<GaugeRange>),
            typeof(CircularGauge),
            new FrameworkPropertyMetadata(null, OnRangesChanged));

    /// <summary>分段染色配置,每段 (From, To, Brush) 在刻度盘外画一段彩色弧。</summary>
    public ObservableCollection<GaugeRange> Ranges
    {
        get => (ObservableCollection<GaugeRange>)GetValue(RangesProperty);
        set => SetValue(RangesProperty, value);
    }

    private void OnRangesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        InvalidateScale();
    }

    private static void OnRangesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gauge = (CircularGauge)d;
        if (e.OldValue is ObservableCollection<GaugeRange> oldCol)
        {
            oldCol.CollectionChanged -= gauge.OnRangesCollectionChanged;
        }
        if (e.NewValue is ObservableCollection<GaugeRange> newCol)
        {
            newCol.CollectionChanged += gauge.OnRangesCollectionChanged;
        }
        gauge.InvalidateScale();
    }

    #endregion Ranges

    #region IsDraggable

    public static readonly DependencyProperty IsDraggableProperty =
        DependencyProperty.Register(
            nameof(IsDraggable),
            typeof(bool),
            typeof(CircularGauge),
            new FrameworkPropertyMetadata(true, OnIsDraggableChanged));

    public bool IsDraggable
    {
        get => (bool)GetValue(IsDraggableProperty);
        set => SetValue(IsDraggableProperty, value);
    }

    private static void OnIsDraggableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gauge = (CircularGauge)d;
        if (!(bool)e.NewValue && gauge._isDragging)
        {
            gauge._isDragging = false;
            gauge.ReleaseMouseCapture();
        }
    }

    #endregion IsDraggable

    #region ValueStringFormat

    public static readonly DependencyProperty ValueStringFormatProperty =
        DependencyProperty.Register(
            nameof(ValueStringFormat),
            typeof(string),
            typeof(CircularGauge),
            new FrameworkPropertyMetadata("{0:F0}", OnValueStringFormatChanged));

    /// <summary>
    /// 当前值显示格式,默认 "{0:F0}" (整数)。
    /// 例:"{0:F2}"、"{0:F1}" 单小数、"{0:P0}" 百分比。
    /// </summary>
    public string ValueStringFormat
    {
        get => (string)GetValue(ValueStringFormatProperty);
        set => SetValue(ValueStringFormatProperty, value);
    }

    private static void OnValueStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CircularGauge)d).UpdateDisplayText();
    }

    #endregion ValueStringFormat

    #region Unit

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(
            nameof(Unit),
            typeof(string),
            typeof(CircularGauge),
            new PropertyMetadata(null));

    /// <summary>单位文本,显示在数值下方(如 "km/h"、"°C")。null/空时不显示。</summary>
    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    #endregion Unit

    #region AnimationDuration

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(
            nameof(AnimationDuration),
            typeof(TimeSpan),
            typeof(CircularGauge),
            new PropertyMetadata(TimeSpan.FromMilliseconds(300)));

    /// <summary>Value 改变时指针平滑动画时长。TimeSpan.Zero 关闭动画。</summary>
    public TimeSpan AnimationDuration
    {
        get => (TimeSpan)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    #endregion AnimationDuration

    #region PointerAngle (只读 - 给模板用)

    public static readonly DependencyProperty PointerAngleProperty;

    // 读写字段在 cctor 里初始化,避免字段初始化器顺序问题(项目惯例)
    private static readonly DependencyPropertyKey PointerAnglePropertyKey;

    /// <summary>当前指针角度(由 Value 派生,模板内部使用)。</summary>
    public double PointerAngle => (double)GetValue(PointerAngleProperty);

    #endregion PointerAngle (只读 - 给模板用)

    #region DisplayText (只读 - 给模板用)

    public static readonly DependencyProperty DisplayTextProperty;

    // Binding.StringFormat 不是 DP,不能用 Binding 赋值
    // 所以在 cs 端把 Value + ValueStringFormat 合成好的字符串暴露给模板
    private static readonly DependencyPropertyKey DisplayTextPropertyKey;

    /// <summary>由 Value 经 ValueStringFormat 格式化后的显示文本,模板 binding 此属性。</summary>
    public string DisplayText => (string)GetValue(DisplayTextProperty);

    #endregion DisplayText (只读 - 给模板用)

    #endregion DependencyProperties

    #region Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _pointerRotation = GetTemplateChild(PART_PointerRotation) as RotateTransform;

        // 首次显示不动画,直接到位
        _suppressAnimation = true;
        UpdatePointerAngle();
        _suppressAnimation = false;

        UpdateDisplayText();
    }

    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        base.OnMaximumChanged(oldMaximum, newMaximum);
        InvalidateScale();
        UpdatePointerAngle();
    }

    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        base.OnMinimumChanged(oldMinimum, newMinimum);
        InvalidateScale();
        UpdatePointerAngle();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        DrawScale(drawingContext);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        // Size 变化自动触发 OnRender,无需手动 invalidate
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        UpdatePointerAngle();
        UpdateDisplayText();
    }

    private static void OnScaleParamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CircularGauge)d).InvalidateScale();
    }

    /// <summary>
    /// Scale 参数改变 (Min/Max/SmallChange/LargeChange/SweepAngle/Tick* 等) 时触发重画刻度。
    /// Value 改变不调这里。
    /// </summary>
    private void InvalidateScale()
    {
        _shortTickPen = null;
        _longTickPen = null;
        _typeface = null;
        InvalidateVisual();
    }

    #endregion Overrides

    #region Mouse Interaction

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsEnabled) return;

        double newValue = Value;
        bool handled = true;
        switch (e.Key)
        {
            case Key.Up:
            case Key.Right:
                newValue = Math.Min(Maximum, Value + SmallChange);
                break;

            case Key.Down:
            case Key.Left:
                newValue = Math.Max(Minimum, Value - SmallChange);
                break;

            case Key.PageUp:
                newValue = Math.Min(Maximum, Value + LargeChange);
                break;

            case Key.PageDown:
                newValue = Math.Max(Minimum, Value - LargeChange);
                break;

            case Key.Home:
                newValue = Minimum;
                break;

            case Key.End:
                newValue = Maximum;
                break;

            default:
                handled = false;
                break;
        }

        if (handled)
        {
            Value = newValue;
            e.Handled = true;
        }
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        _isDragging = false;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (!IsDraggable || !IsEnabled) return;

        Focus();
        if (TryUpdateValueFromMouse(e.GetPosition(this)))
        {
            _isDragging = true;
            CaptureMouse();
            e.Handled = true;
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (_isDragging)
        {
            _isDragging = false;
            ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            TryUpdateValueFromMouse(e.GetPosition(this));
        }
    }

    #endregion Mouse Interaction

    #region Private Methods

    /// <summary>角度 (0° = 顶部,顺时针为正) → 笛卡尔坐标。</summary>
    private static Point AngleToPoint(double cx, double cy, double r, double angleDeg)
    {
        double rad = (angleDeg - 90) * Math.PI / 180;
        return new Point(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
    }

    /// <summary>从鼠标坐标算 Value。死区返回 false,不改 Value(避免点底部跳到 Max)。</summary>
    private bool TryUpdateValueFromMouse(Point position)
    {
        double cx = ActualWidth / 2;
        double cy = ActualHeight / 2;
        double dx = position.X - cx;
        double dy = position.Y - cy;
        double radius = Math.Min(ActualWidth, ActualHeight) * 0.5;

        if (Math.Sqrt(dx * dx + dy * dy) > radius) return false;

        // 计算"从顶部顺时针"的角度 [0, 360)
        // atan2(dx, -dy) — 自定义坐标:顶部为 0°,顺时针 +
        double mouseAngle = Math.Atan2(dx, -dy) * 180 / Math.PI;
        if (mouseAngle < 0) mouseAngle += 360;

        double sweep = SweepAngle;
        double half = sweep / 2;

        // 扫描区在 [-half, +half],换算到 [0, 360) 范围:
        // 左半 (360-half, 360)  +  右半 [0, half]
        // 死区是 (half, 360-half)

        if (mouseAngle > half && mouseAngle < 360 - half)
        {
            return false;   // 死区
        }

        // angleFromStart: 从 -half 起的角度差,范围 [0, sweep]
        double angleFromStart = mouseAngle <= half
            ? half + mouseAngle             // 上半右侧
            : mouseAngle - (360 - half);    // 上半左侧

        double range = Maximum - Minimum;
        double ratio = sweep == 0 ? 0 : angleFromStart / sweep;
        Value = Math.Max(Minimum, Math.Min(Maximum, Minimum + ratio * range));
        return true;
    }

    private void UpdateDisplayText()
    {
        string format = ValueStringFormat;
        string text;
        if (string.IsNullOrEmpty(format))
        {
            text = Value.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            try
            {
                text = string.Format(CultureInfo.CurrentCulture, format, Value);
            }
            catch (FormatException)
            {
                text = Value.ToString(CultureInfo.CurrentCulture);
            }
        }
        SetValue(DisplayTextPropertyKey, text);
    }

    private void UpdatePointerAngle()
    {
        double range = Maximum - Minimum;
        double ratio = range == 0 ? 0 : (Value - Minimum) / range;
        double sweep = SweepAngle;
        double newAngle = -sweep / 2 + ratio * sweep;

        SetValue(PointerAnglePropertyKey, newAngle);

        if (_pointerRotation is not null)
        {
            var duration = AnimationDuration;
            if (_suppressAnimation || duration == TimeSpan.Zero)
            {
                _pointerRotation.BeginAnimation(RotateTransform.AngleProperty, null);
                _pointerRotation.Angle = newAngle;
            }
            else
            {
                var anim = new DoubleAnimation
                {
                    To = newAngle,
                    Duration = duration,
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                _pointerRotation.BeginAnimation(RotateTransform.AngleProperty, anim);
            }
        }
    }

    #endregion Private Methods

    #region Drawing

    private static Pen FreezePen(Pen p)
    {
        if (p.CanFreeze) p.Freeze();
        return p;
    }

    private void DrawRangeRing(DrawingContext dc, double cx, double cy, double radius)
    {
        if (Ranges is null || Ranges.Count == 0) return;

        double ringThickness = RangeRingThickness;
        double outerR = radius - 1;
        double innerR = radius - ringThickness;
        if (innerR <= 0) return;

        double sweep = SweepAngle;
        double range = Maximum - Minimum;
        if (range <= 0) return;

        foreach (var seg in Ranges)
        {
            if (seg.Brush is null) continue;
            double from = Math.Max(Minimum, seg.From);
            double to = Math.Min(Maximum, seg.To);
            if (to <= from) continue;

            double angleStart = -sweep / 2 + (from - Minimum) / range * sweep;
            double angleEnd = -sweep / 2 + (to - Minimum) / range * sweep;

            var pOuter0 = AngleToPoint(cx, cy, outerR, angleStart);
            var pOuter1 = AngleToPoint(cx, cy, outerR, angleEnd);
            var pInner0 = AngleToPoint(cx, cy, innerR, angleStart);
            var pInner1 = AngleToPoint(cx, cy, innerR, angleEnd);

            bool isLarge = (angleEnd - angleStart) > 180;

            var fig = new PathFigure { StartPoint = pOuter0, IsClosed = true };
            fig.Segments.Add(new ArcSegment(pOuter1, new Size(outerR, outerR), 0, isLarge, SweepDirection.Clockwise, true));
            fig.Segments.Add(new LineSegment(pInner1, true));
            fig.Segments.Add(new ArcSegment(pInner0, new Size(innerR, innerR), 0, isLarge, SweepDirection.Counterclockwise, true));

            var geom = new PathGeometry();
            geom.Figures.Add(fig);
            if (geom.CanFreeze) geom.Freeze();

            dc.DrawGeometry(seg.Brush, null, geom);
        }
    }

    private void DrawScale(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 0 || height <= 0) return;

        double radius = Math.Min(width, height) * 0.5 - 1;  // -1 留出边框空间
        double cx = width / 2;
        double cy = height / 2;

        // 缓存 Pen / Typeface
        _shortTickPen ??= FreezePen(new Pen(TickBrush, 1));
        _longTickPen ??= FreezePen(new Pen(TickBrush, 2));
        _typeface ??= new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);

        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        // 1. 背景圆 + 边框 (替代了原模板里的 Ellipse,因为模板会覆盖 OnRender)
        var borderPen = new Pen(BorderBrush, 1);
        if (borderPen.CanFreeze) borderPen.Freeze();
        dc.DrawEllipse(Background, borderPen, new Point(cx, cy), radius, radius);

        // 2. 分段染色环
        DrawRangeRing(dc, cx, cy, radius);

        // 3. 刻度 + 标签
        DrawTicksAndLabels(dc, cx, cy, radius, dpi);
    }

    private void DrawTicksAndLabels(DrawingContext dc, double cx, double cy, double radius, double dpi)
    {
        double sweep = SweepAngle;
        double range = Maximum - Minimum;
        if (range <= 0 || SmallChange <= 0) return;

        // 用 int 循环避免浮点累加误差
        int totalTicks = (int)Math.Round(range / SmallChange);
        if (totalTicks <= 0 || totalTicks > 10000) return;

        int longEvery = LargeChange > 0
            ? Math.Max(1, (int)Math.Round(LargeChange / SmallChange))
            : 1;

        double tickLen = radius * TickLengthRatio;
        double longLen = tickLen * LongTickRatio;
        double rangeOffset = (Ranges?.Count > 0) ? RangeRingThickness + 2 : 0;
        double tickOuter = radius - rangeOffset;

        for (int i = 0; i <= totalTicks; i++)
        {
            bool isLong = i % longEvery == 0;
            double currentLen = isLong ? longLen : tickLen;
            double angle = -sweep / 2 + (double)i / totalTicks * sweep;

            var p1 = AngleToPoint(cx, cy, tickOuter, angle);
            var p2 = AngleToPoint(cx, cy, tickOuter - currentLen, angle);

            dc.DrawLine(isLong ? _longTickPen : _shortTickPen, p1, p2);

            if (isLong)
            {
                double labelValue = Minimum + i * SmallChange;
                var ft = new FormattedText(
                    labelValue.ToString(CultureInfo.CurrentCulture),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    _typeface,
                    LabelFontSize,
                    TickBrush,
                    dpi);

                var labelCenter = AngleToPoint(cx, cy, tickOuter - currentLen - LabelFontSize, angle);
                dc.DrawText(ft, new Point(
                    labelCenter.X - ft.Width / 2,
                    labelCenter.Y - ft.Height / 2));
            }
        }
    }

    #endregion Drawing
}

/// <summary>仪表盘分段染色配置 — 表示一段数值范围用一种颜色填充。</summary>
public class GaugeRange : Freezable
{
    public static readonly DependencyProperty BrushProperty =
        DependencyProperty.Register(nameof(Brush), typeof(Brush), typeof(GaugeRange), new PropertyMetadata(null));

    public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register(nameof(From), typeof(double), typeof(GaugeRange), new PropertyMetadata(0d));

    public static readonly DependencyProperty ToProperty =
        DependencyProperty.Register(nameof(To), typeof(double), typeof(GaugeRange), new PropertyMetadata(0d));

    public Brush Brush { get => (Brush)GetValue(BrushProperty); set => SetValue(BrushProperty, value); }

    public double From { get => (double)GetValue(FromProperty); set => SetValue(FromProperty, value); }

    public double To { get => (double)GetValue(ToProperty); set => SetValue(ToProperty, value); }

    protected override Freezable CreateInstanceCore() => new GaugeRange();
}