using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// <see cref="RangeSlider"/> 专用 track —— 根据 <see cref="LowerValue"/> / <see cref="UpperValue"/>
/// 动态分配 StartRegion / StartThumb / MiddleRegion / EndThumb / EndRegion 五段的尺寸。
/// 仿照 WPF 内置 <see cref="System.Windows.Controls.Primitives.Track"/>,但支持双 thumb,
/// 且采用统一线性映射保证 thumb 中心始终与 TickBar 刻度对齐。
/// </summary>
public class RangeTrack : Panel
{
    private RepeatButton _startRegion;
    private Thumb _startThumb;
    private RepeatButton _middleRegion;
    private Thumb _endThumb;
    private RepeatButton _endRegion;

    #region Minimum

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(RangeTrack),
            new FrameworkPropertyMetadata(
                0d,
                FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>区间最小值。由模板 TemplateBinding 到 <see cref="RangeSlider.Minimum"/>。</summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    #endregion Minimum

    #region Maximum

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(RangeTrack),
            new FrameworkPropertyMetadata(
                100d,
                FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>区间最大值。由模板 TemplateBinding 到 <see cref="RangeSlider.Maximum"/>。</summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    #endregion Maximum

    #region LowerValue

    public static readonly DependencyProperty LowerValueProperty =
        DependencyProperty.Register(
            nameof(LowerValue),
            typeof(double),
            typeof(RangeTrack),
            new FrameworkPropertyMetadata(
                0d,
                FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>当前下界值。由模板 TemplateBinding 到 <see cref="RangeSlider.LowerValue"/>。</summary>
    public double LowerValue
    {
        get => (double)GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    #endregion LowerValue

    #region UpperValue

    public static readonly DependencyProperty UpperValueProperty =
        DependencyProperty.Register(
            nameof(UpperValue),
            typeof(double),
            typeof(RangeTrack),
            new FrameworkPropertyMetadata(
                100d,
                FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>当前上界值。由模板 TemplateBinding 到 <see cref="RangeSlider.UpperValue"/>。</summary>
    public double UpperValue
    {
        get => (double)GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    #endregion UpperValue

    #region Orientation

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(RangeTrack),
            new FrameworkPropertyMetadata(
                Orientation.Horizontal,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>方向。由模板 TemplateBinding 到 <see cref="RangeSlider.Orientation"/>。</summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion Orientation

    #region IsDirectionReversed

    public static readonly DependencyProperty IsDirectionReversedProperty =
        DependencyProperty.Register(
            nameof(IsDirectionReversed),
            typeof(bool),
            typeof(RangeTrack),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>反转方向。由模板 TemplateBinding 到 <see cref="RangeSlider.IsDirectionReversed"/>。</summary>
    public bool IsDirectionReversed
    {
        get => (bool)GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    #endregion IsDirectionReversed

    #region Children

    /// <summary>StartRegion —— 下界外侧的非活动轨道段。由模板 set。</summary>
    public RepeatButton StartRegion
    {
        get => _startRegion;
        set => UpdateChild(ref _startRegion, value);
    }

    /// <summary>StartThumb —— 下界拖动手柄。由模板 set。</summary>
    public Thumb StartThumb
    {
        get => _startThumb;
        set => UpdateChild(ref _startThumb, value);
    }

    /// <summary>MiddleRegion —— 区间内的活动轨道段。由模板 set。</summary>
    public RepeatButton MiddleRegion
    {
        get => _middleRegion;
        set => UpdateChild(ref _middleRegion, value);
    }

    /// <summary>EndThumb —— 上界拖动手柄。由模板 set。</summary>
    public Thumb EndThumb
    {
        get => _endThumb;
        set => UpdateChild(ref _endThumb, value);
    }

    /// <summary>EndRegion —— 上界外侧的非活动轨道段。由模板 set。</summary>
    public RepeatButton EndRegion
    {
        get => _endRegion;
        set => UpdateChild(ref _endRegion, value);
    }

    private void UpdateChild<T>(ref T field, T value) where T : UIElement
    {
        if (Equals(field, value))
        {
            return;
        }
        if (field is not null)
        {
            Children.Remove(field);
        }
        field = value;
        if (field is not null)
        {
            Children.Add(field);
        }
    }

    #endregion Children

    #region Override Methods

    protected override Size MeasureOverride(Size availableSize)
    {
        var infinite = new Size(double.PositiveInfinity, double.PositiveInfinity);
        _startThumb?.Measure(infinite);
        _endThumb?.Measure(infinite);
        _startRegion?.Measure(infinite);
        _middleRegion?.Measure(infinite);
        _endRegion?.Measure(infinite);

        // 副轴尺寸取两个 thumb 的 desired 较大值;主轴随父级
        if (Orientation == Orientation.Horizontal)
        {
            var h = Math.Max(
                _startThumb?.DesiredSize.Height ?? 0,
                _endThumb?.DesiredSize.Height ?? 0);
            return new Size(0, h);
        }
        else
        {
            var w = Math.Max(
                _startThumb?.DesiredSize.Width ?? 0,
                _endThumb?.DesiredSize.Width ?? 0);
            return new Size(w, 0);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // 强制 Z-order:两个 region 在底,middle 在中,两个 thumb 在最上
        EnsureZOrder();

        var horizontal = Orientation == Orientation.Horizontal;
        var total = horizontal ? finalSize.Width : finalSize.Height;
        var thumbSize = GetThumbSize(horizontal);

        var range = Math.Max(Maximum - Minimum, 1e-9);
        var trackUsable = Math.Max(total - thumbSize, 0);

        var lowerRatio = Math.Min(Math.Max((LowerValue - Minimum) / range, 0), 1);
        var upperRatio = Math.Min(Math.Max((UpperValue - Minimum) / range, 0), 1);

        var startOffset = RatioToOffset(lowerRatio, trackUsable, horizontal);
        var endOffset = RatioToOffset(upperRatio, trackUsable, horizontal);

        if (horizontal)
        {
            ArrangeHorizontal(finalSize, startOffset, endOffset, thumbSize);
        }
        else
        {
            ArrangeVertical(finalSize, startOffset, endOffset, thumbSize);
        }

        return finalSize;
    }

    #endregion Override Methods

    #region Private Methods

    private double GetThumbSize(bool horizontal)
    {
        var startSize = horizontal
            ? _startThumb?.DesiredSize.Width ?? 0
            : _startThumb?.DesiredSize.Height ?? 0;
        var endSize = horizontal
            ? _endThumb?.DesiredSize.Width ?? 0
            : _endThumb?.DesiredSize.Height ?? 0;
        return Math.Max(startSize, endSize);
    }

    private double RatioToOffset(double ratio, double trackUsable, bool horizontal)
    {
        // 自然方向:水平=右边大值;垂直=上边大值(下→上递增)
        var natural = horizontal ? ratio : (1 - ratio);
        if (IsDirectionReversed)
        {
            natural = 1 - natural;
        }
        return natural * trackUsable;
    }

    private void ArrangeHorizontal(Size finalSize, double startLeft, double endLeft, double thumbSize)
    {
        var total = finalSize.Width;
        var h = finalSize.Height;

        // 物理位置:near 较靠左,far 较靠右
        var nearLeft = Math.Min(startLeft, endLeft);
        var farLeft = Math.Max(startLeft, endLeft);

        // StartRegion / EndRegion 的物理位置取决于方向
        if (!IsDirectionReversed)
        {
            // 正向:Start 在左,End 在右
            _startRegion?.Arrange(new Rect(0, 0, startLeft, h));
            _endRegion?.Arrange(new Rect(
                endLeft + thumbSize, 0,
                Math.Max(total - endLeft - thumbSize, 0), h));
        }
        else
        {
            // 反向:Start 在右,End 在左
            _startRegion?.Arrange(new Rect(
                startLeft + thumbSize, 0,
                Math.Max(total - startLeft - thumbSize, 0), h));
            _endRegion?.Arrange(new Rect(0, 0, endLeft, h));
        }

        // MiddleRegion(活动区间色块):从 near thumb 中心到 far thumb 中心
        var midX = nearLeft + thumbSize / 2;
        var midW = Math.Max((farLeft + thumbSize / 2) - midX, 0);
        _middleRegion?.Arrange(new Rect(midX, 0, midW, h));

        // Thumbs 最后 Arrange,Z-order 在最上,允许与 middleRegion 视觉重叠
        _startThumb?.Arrange(new Rect(startLeft, 0, thumbSize, h));
        _endThumb?.Arrange(new Rect(endLeft, 0, thumbSize, h));
    }

    private void ArrangeVertical(Size finalSize, double startTop, double endTop, double thumbSize)
    {
        var total = finalSize.Height;
        var w = finalSize.Width;

        var nearTop = Math.Min(startTop, endTop);
        var farTop = Math.Max(startTop, endTop);

        // 垂直默认下→上递增:LowerValue 在底部(startTop > endTop)
        if (!IsDirectionReversed)
        {
            // Start 在底,End 在顶
            _startRegion?.Arrange(new Rect(
                0, startTop + thumbSize,
                w, Math.Max(total - startTop - thumbSize, 0)));
            _endRegion?.Arrange(new Rect(0, 0, w, endTop));
        }
        else
        {
            // 反向:Start 在顶,End 在底
            _startRegion?.Arrange(new Rect(0, 0, w, startTop));
            _endRegion?.Arrange(new Rect(
                0, endTop + thumbSize,
                w, Math.Max(total - endTop - thumbSize, 0)));
        }

        // MiddleRegion:从 near thumb 中心到 far thumb 中心
        var midY = nearTop + thumbSize / 2;
        var midH = Math.Max((farTop + thumbSize / 2) - midY, 0);
        _middleRegion?.Arrange(new Rect(0, midY, w, midH));

        _startThumb?.Arrange(new Rect(0, startTop, w, thumbSize));
        _endThumb?.Arrange(new Rect(0, endTop, w, thumbSize));
    }

    /// <summary>
    /// 把子元素按视觉层次排序:StartRegion / EndRegion → MiddleRegion → StartThumb / EndThumb。
    /// Panel 按 Children 索引顺序绘制,索引大的在上。
    /// </summary>
    private void EnsureZOrder()
    {
        // 期望的从底到顶顺序
        var ordered = new UIElement[]
        {
        _startRegion,
        _endRegion,
        _middleRegion,
        _startThumb,
        _endThumb,
        };

        // 检查当前顺序是否已正确,避免每次 Arrange 都改动 Children
        var idx = 0;
        var alreadyOrdered = true;
        foreach (var el in ordered)
        {
            if (el is null)
            {
                continue;
            }
            if (idx >= Children.Count || !ReferenceEquals(Children[idx], el))
            {
                alreadyOrdered = false;
                break;
            }
            idx++;
        }
        if (alreadyOrdered)
        {
            return;
        }

        Children.Clear();
        foreach (var el in ordered)
        {
            if (el is not null)
            {
                Children.Add(el);
            }
        }
    }

    #endregion Private Methods
}