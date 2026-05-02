using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_Track, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_Thumb, Type = typeof(FrameworkElement))]
[TemplatePart(Name = PART_ThumbTransform, Type = typeof(TranslateTransform))]
public class SwitchButton : ToggleButton
{
    #region Part Names

    private const string PART_Track = "PART_Track";
    private const string PART_Thumb = "PART_Thumb";
    private const string PART_ThumbTransform = "PART_ThumbTransform";

    #endregion Part Names

    #region Fields

    private FrameworkElement _track;
    private FrameworkElement _thumb;
    private TranslateTransform _thumbTransform;

    #endregion Fields

    #region Constructor

    static SwitchButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SwitchButton), new FrameworkPropertyMetadata(typeof(SwitchButton)));
    }

    #endregion Constructor

    #region Overrides

    protected override void OnChecked(RoutedEventArgs e)
    {
        base.OnChecked(e);
        AnimateThumb(true);
    }

    protected override void OnUnchecked(RoutedEventArgs e)
    {
        base.OnUnchecked(e);
        AnimateThumb(false);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 获取模板部件
        _track = GetTemplateChild(PART_Track) as FrameworkElement;
        _thumb = GetTemplateChild(PART_Thumb) as FrameworkElement;
        _thumbTransform = GetTemplateChild(PART_ThumbTransform) as TranslateTransform;

        // 初始化控件尺寸和滑块位置
        UpdateControlSize();
        UpdateThumbPosition(false);
    }

    #endregion Overrides

    #region Private Methods

    // 当布局相关属性改变时更新滑块位置
    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SwitchButton switchButton)
        {
            switchButton.UpdateThumbPosition();
            switchButton.UpdateControlSize();
        }
    }

    private void AnimateThumb(bool isChecked)
    {
        if (_thumbTransform == null) return;

        var targetX = isChecked ? GetThumbMoveDistance() : 0;

        var animation = new DoubleAnimation
        {
            To = targetX,
            Duration = AnimationDuration,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        _thumbTransform.BeginAnimation(TranslateTransform.XProperty, animation);
    }

    private void UpdateThumbPosition(bool animate = true)
    {
        if (_thumbTransform == null) return;

        var targetX = IsChecked == true ? GetThumbMoveDistance() : 0;

        if (animate)
        {
            var animation = new DoubleAnimation
            {
                To = targetX,
                Duration = AnimationDuration,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            _thumbTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }
        else
        {
            _thumbTransform.X = targetX;
        }
    }

    private double GetThumbMoveDistance()
    {
        // 计算滑块移动距离: TrackWidth - ThumbSize - 左右边距
        var leftMargin = ThumbMargin.Left;
        var rightMargin = ThumbMargin.Right;
        return Math.Max(0, TrackWidth - ThumbSize - leftMargin - rightMargin);
    }

    private void UpdateControlSize()
    {
        // 设置最小尺寸确保开关部分正确显示
        MinWidth = TrackWidth;
        MinHeight = Math.Max(TrackHeight, ThumbSize);

        // 不要强制设置Width，让BulletDecorator自动计算
        // 这样可以包含Content的宽度
        if (double.IsNaN(Height))
            Height = Math.Max(TrackHeight, ThumbSize);

        // 注释掉Width设置，让控件自动适应Content
        // if (double.IsNaN(Width))
        //     Width = TrackWidth;
    }

    #endregion Private Methods

    #region Dependency Properties

    #region TrackWidth

    public static readonly DependencyProperty TrackWidthProperty =
                DependencyProperty.Register(
                    nameof(TrackWidth),
                    typeof(double),
                    typeof(SwitchButton),
                    new PropertyMetadata(50d, OnLayoutPropertyChanged));

    /// <summary>
    /// 获取或设置开关轨道的宽度
    /// </summary>
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

    /// <summary>
    /// 获取或设置开关轨道的高度
    /// </summary>
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

    /// <summary>
    /// 获取或设置滑块的尺寸
    /// </summary>
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

    /// <summary>
    /// 获取或设置滑块与轨道边缘的间距
    /// </summary>
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

    /// <summary>
    /// 获取或设置滑块的垂直对齐方式
    /// </summary>
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

    /// <summary>
    /// 获取或设置滑块的水平对齐方式（用于初始位置）
    /// </summary>
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

    /// <summary>
    /// 获取或设置轨道的圆角半径
    /// </summary>
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
                    new PropertyMetadata(new CornerRadius(11)));

    /// <summary>
    /// 获取或设置滑块的圆角半径
    /// </summary>
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
                    new PropertyMetadata(new SolidColorBrush(Color.FromRgb(204, 204, 204))));

    /// <summary>
    /// 获取或设置未选中状态下轨道的背景色
    /// </summary>
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
                    new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 75, 75))));

    /// <summary>
    /// 获取或设置选中状态下轨道的背景色
    /// </summary>
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

    /// <summary>
    /// 获取或设置滑块的背景色
    /// </summary>
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
    /// 获取或设置动画持续时间
    /// </summary>
    public Duration AnimationDuration
    {
        get => (Duration)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    #endregion AnimationDuration

    #endregion Dependency Properties
}