using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 滑动开关控件。继承自 <see cref="ToggleButton"/>，在 <see cref="ToggleButton.IsChecked"/>
/// 切换时滑块带动画从一端移到另一端。<br/>
/// 锁定为二态（<see cref="ToggleButton.IsThreeState"/> 默认 false）——switch 控件
/// 本质不存在 indeterminate 状态。
/// </summary>
[TemplatePart(Name = PartTrack, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartThumb, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PartThumbTransform, Type = typeof(TranslateTransform))]
public class SwitchButton : ToggleButton
{
    private const string PartThumb = "PART_Thumb";

    private const string PartThumbTransform = "PART_ThumbTransform";

    private const string PartTrack = "PART_Track";

    // ============ 默认 Brush(freeze 共享,避免每实例分配)============
    // 注意:这些是 fallback 默认值,实际样式 xaml 里会 setter 覆盖成主题 token。
    // 颜色 #2196F3 = BlueDefault (BasicTheme.xaml 的主蓝),保证不应用样式时也是主蓝
    private static readonly Brush DefaultCheckedBackground = CreateFrozenBrush(33, 150, 243);

    private static readonly Brush DefaultUncheckedBackground = CreateFrozenBrush(204, 204, 204);

    private FrameworkElement _thumb;

    private TranslateTransform _thumbTransform;

    private FrameworkElement _track;

    private static Brush CreateFrozenBrush(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    static SwitchButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SwitchButton),
            new FrameworkPropertyMetadata(typeof(SwitchButton)));

        // SwitchButton 本质二态——锁定 IsThreeState 默认 false。
        // 防止 user 设 IsThreeState=true + IsChecked=null 时 thumb 卡住不动（因为 OnIndeterminate 不会触发动画）。
        IsThreeStateProperty.OverrideMetadata(typeof(SwitchButton),
            new FrameworkPropertyMetadata(false));
    }

    #region TrackWidth

    public static readonly DependencyProperty TrackWidthProperty =
        DependencyProperty.Register(
            nameof(TrackWidth),
            typeof(double),
            typeof(SwitchButton),
            new PropertyMetadata(50d, OnLayoutPropertyChanged));

    /// <summary>开关轨道的宽度。</summary>
    public double TrackWidth
    {
        get => (double)GetValue(TrackWidthProperty);
        set => SetValue(TrackWidthProperty, value);
    }

    #endregion TrackWidth

    #region TrackHeight

    public static readonly DependencyProperty TrackHeightProperty =
        DependencyProperty.Register(
            nameof(TrackHeight),
            typeof(double),
            typeof(SwitchButton),
            new PropertyMetadata(26d, OnLayoutPropertyChanged));

    /// <summary>开关轨道的高度。</summary>
    public double TrackHeight
    {
        get => (double)GetValue(TrackHeightProperty);
        set => SetValue(TrackHeightProperty, value);
    }

    #endregion TrackHeight

    #region ThumbSize

    public static readonly DependencyProperty ThumbSizeProperty =
        DependencyProperty.Register(
            nameof(ThumbSize),
            typeof(double),
            typeof(SwitchButton),
            new PropertyMetadata(22d, OnLayoutPropertyChanged));

    /// <summary>滑块的尺寸（正方形）。</summary>
    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }

    #endregion ThumbSize

    #region ThumbMargin

    public static readonly DependencyProperty ThumbMarginProperty =
        DependencyProperty.Register(
            nameof(ThumbMargin),
            typeof(Thickness),
            typeof(SwitchButton),
            new PropertyMetadata(new Thickness(2), OnLayoutPropertyChanged));

    /// <summary>滑块与轨道边缘的间距。</summary>
    public Thickness ThumbMargin
    {
        get => (Thickness)GetValue(ThumbMarginProperty);
        set => SetValue(ThumbMarginProperty, value);
    }

    #endregion ThumbMargin

    #region ThumbVerticalAlignment

    public static readonly DependencyProperty ThumbVerticalAlignmentProperty =
        DependencyProperty.Register(
            nameof(ThumbVerticalAlignment),
            typeof(VerticalAlignment),
            typeof(SwitchButton),
            new PropertyMetadata(VerticalAlignment.Center));

    /// <summary>滑块的垂直对齐方式。</summary>
    public VerticalAlignment ThumbVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(ThumbVerticalAlignmentProperty);
        set => SetValue(ThumbVerticalAlignmentProperty, value);
    }

    #endregion ThumbVerticalAlignment

    #region ThumbHorizontalAlignment

    public static readonly DependencyProperty ThumbHorizontalAlignmentProperty =
        DependencyProperty.Register(
            nameof(ThumbHorizontalAlignment),
            typeof(HorizontalAlignment),
            typeof(SwitchButton),
            new PropertyMetadata(HorizontalAlignment.Left));

    /// <summary>滑块的水平对齐方式（用于初始位置）。</summary>
    public HorizontalAlignment ThumbHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(ThumbHorizontalAlignmentProperty);
        set => SetValue(ThumbHorizontalAlignmentProperty, value);
    }

    #endregion ThumbHorizontalAlignment

    #region TrackCornerRadius

    public static readonly DependencyProperty TrackCornerRadiusProperty =
        DependencyProperty.Register(
            nameof(TrackCornerRadius),
            typeof(CornerRadius),
            typeof(SwitchButton),
            new PropertyMetadata(new CornerRadius(13)));

    /// <summary>轨道的圆角半径。</summary>
    public CornerRadius TrackCornerRadius
    {
        get => (CornerRadius)GetValue(TrackCornerRadiusProperty);
        set => SetValue(TrackCornerRadiusProperty, value);
    }

    #endregion TrackCornerRadius

    #region ThumbCornerRadius

    public static readonly DependencyProperty ThumbCornerRadiusProperty =
        DependencyProperty.Register(
            nameof(ThumbCornerRadius),
            typeof(CornerRadius),
            typeof(SwitchButton),
            new PropertyMetadata(new CornerRadius(8)));

    /// <summary>滑块的圆角半径。</summary>
    public CornerRadius ThumbCornerRadius
    {
        get => (CornerRadius)GetValue(ThumbCornerRadiusProperty);
        set => SetValue(ThumbCornerRadiusProperty, value);
    }

    #endregion ThumbCornerRadius

    #region UncheckedBackground

    public static readonly DependencyProperty UncheckedBackgroundProperty =
        DependencyProperty.Register(
            nameof(UncheckedBackground),
            typeof(Brush),
            typeof(SwitchButton),
            new PropertyMetadata(DefaultUncheckedBackground));

    /// <summary>未选中状态下轨道的背景色。</summary>
    public Brush UncheckedBackground
    {
        get => (Brush)GetValue(UncheckedBackgroundProperty);
        set => SetValue(UncheckedBackgroundProperty, value);
    }

    #endregion UncheckedBackground

    #region CheckedBackground

    public static readonly DependencyProperty CheckedBackgroundProperty =
        DependencyProperty.Register(
            nameof(CheckedBackground),
            typeof(Brush),
            typeof(SwitchButton),
            new PropertyMetadata(DefaultCheckedBackground));

    /// <summary>选中状态下轨道的背景色。</summary>
    public Brush CheckedBackground
    {
        get => (Brush)GetValue(CheckedBackgroundProperty);
        set => SetValue(CheckedBackgroundProperty, value);
    }

    #endregion CheckedBackground

    #region ThumbBackground

    public static readonly DependencyProperty ThumbBackgroundProperty =
        DependencyProperty.Register(
            nameof(ThumbBackground),
            typeof(Brush),
            typeof(SwitchButton),
            new PropertyMetadata(Brushes.White));

    /// <summary>滑块的背景色。</summary>
    public Brush ThumbBackground
    {
        get => (Brush)GetValue(ThumbBackgroundProperty);
        set => SetValue(ThumbBackgroundProperty, value);
    }

    #endregion ThumbBackground

    #region AnimationDuration

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(
            nameof(AnimationDuration),
            typeof(Duration),
            typeof(SwitchButton),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(200))));

    /// <summary>
    /// 滑块切换动画的持续时间。设为 <see cref="TimeSpan.Zero"/>（XAML <c>0:0:0</c>）等价于立即切换无动画。
    /// </summary>
    public Duration AnimationDuration
    {
        get => (Duration)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    #endregion AnimationDuration

    #region Override Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _track = GetTemplateChild(PartTrack) as FrameworkElement;
        _thumb = GetTemplateChild(PartThumb) as FrameworkElement;
        _thumbTransform = GetTemplateChild(PartThumbTransform) as TranslateTransform;

        UpdateThumbPosition(animate: false);
    }

    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        UpdateThumbPosition(animate: true);
    }

    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        UpdateThumbPosition(animate: true);
    }

    #endregion Override Methods

    #region Private Methods

    /// <summary>
    /// 把 thumb 移动到 IsChecked 对应的位置。<paramref name="animate"/>=true 时带 ease-out 动画；
    /// false 时立即跳转——会先清除已有 animation（否则 active animation 在 effective value 计算里
    /// 优先级高于 local value，直接 SetValue X 不会生效）。
    /// </summary>
    private void UpdateThumbPosition(bool animate)
    {
        if (_thumbTransform == null)
        {
            return;
        }

        var targetX = IsChecked == true ? GetThumbMoveDistance() : 0;

        if (animate)
        {
            _thumbTransform.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation
                {
                    To = targetX,
                    Duration = AnimationDuration,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                });
        }
        else
        {
            // 必须先清除 animation，否则 SetValue 无效
            _thumbTransform.BeginAnimation(TranslateTransform.XProperty, null);
            _thumbTransform.X = targetX;
        }
    }

    /// <summary>
    /// 布局相关 DP（TrackWidth / TrackHeight / ThumbSize / ThumbMargin）改变时立即重定位 thumb，
    /// <b>不带动画</b>——动画只用于 user 交互触发的 IsChecked 切换。
    /// </summary>
    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SwitchButton switchButton)
        {
            switchButton.UpdateThumbPosition(animate: false);
        }
    }

    /// <summary>滑块移动距离 = 轨道宽度 - 滑块尺寸 - 左右边距。</summary>
    private double GetThumbMoveDistance()
    {
        var leftMargin = ThumbMargin.Left;
        var rightMargin = ThumbMargin.Right;
        return Math.Max(0, TrackWidth - ThumbSize - leftMargin - rightMargin);
    }

    #endregion Private Methods
}