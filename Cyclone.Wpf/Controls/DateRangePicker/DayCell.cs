using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 月历中的单个日期格子。继承 ButtonBase 复用 Click 事件 + Pressed 视觉,
/// 状态用一组只读 DP 表达,XAML 模板里用 trigger 染色。
/// 由 MonthView 内部创建和管理,不暴露给用户。
/// </summary>
public class DayCell : ButtonBase
{
    static DayCell()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DayCell),
            new FrameworkPropertyMetadata(typeof(DayCell)));
    }

    #region Date

    public static readonly DependencyProperty DateProperty =
        DependencyProperty.Register(
            nameof(Date),
            typeof(DateTime),
            typeof(DayCell),
            new PropertyMetadata(default(DateTime)));

    /// <summary>这个格子代表的完整日期。</summary>
    public DateTime Date
    {
        get => (DateTime)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    #endregion Date

    #region DayText

    public static readonly DependencyProperty DayTextProperty =
        DependencyProperty.Register(
            nameof(DayText),
            typeof(string),
            typeof(DayCell),
            new PropertyMetadata(string.Empty));

    /// <summary>显示的日数字字符串。</summary>
    public string DayText
    {
        get => (string)GetValue(DayTextProperty);
        set => SetValue(DayTextProperty, value);
    }

    #endregion DayText

    #region IsCurrentMonth

    public static readonly DependencyProperty IsCurrentMonthProperty =
        DependencyProperty.Register(
            nameof(IsCurrentMonth),
            typeof(bool),
            typeof(DayCell),
            new PropertyMetadata(true));

    /// <summary>该日期是否属于当前显示的月份(否则灰色显示)。</summary>
    public bool IsCurrentMonth
    {
        get => (bool)GetValue(IsCurrentMonthProperty);
        set => SetValue(IsCurrentMonthProperty, value);
    }

    #endregion IsCurrentMonth

    #region IsToday

    public static readonly DependencyProperty IsTodayProperty =
        DependencyProperty.Register(
            nameof(IsToday),
            typeof(bool),
            typeof(DayCell),
            new PropertyMetadata(false));

    public bool IsToday
    {
        get => (bool)GetValue(IsTodayProperty);
        set => SetValue(IsTodayProperty, value);
    }

    #endregion IsToday

    #region IsBlackout

    public static readonly DependencyProperty IsBlackoutProperty =
        DependencyProperty.Register(
            nameof(IsBlackout),
            typeof(bool),
            typeof(DayCell),
            new PropertyMetadata(false));

    /// <summary>该日期是否禁用(BlackoutDates 包含)。</summary>
    public bool IsBlackout
    {
        get => (bool)GetValue(IsBlackoutProperty);
        set => SetValue(IsBlackoutProperty, value);
    }

    #endregion IsBlackout

    #region IsRangeStart

    public static readonly DependencyProperty IsRangeStartProperty =
        DependencyProperty.Register(
            nameof(IsRangeStart),
            typeof(bool),
            typeof(DayCell),
            new PropertyMetadata(false));

    public bool IsRangeStart
    {
        get => (bool)GetValue(IsRangeStartProperty);
        set => SetValue(IsRangeStartProperty, value);
    }

    #endregion IsRangeStart

    #region IsRangeEnd

    public static readonly DependencyProperty IsRangeEndProperty =
        DependencyProperty.Register(
            nameof(IsRangeEnd),
            typeof(bool),
            typeof(DayCell),
            new PropertyMetadata(false));

    public bool IsRangeEnd
    {
        get => (bool)GetValue(IsRangeEndProperty);
        set => SetValue(IsRangeEndProperty, value);
    }

    #endregion IsRangeEnd

    #region IsInRange

    public static readonly DependencyProperty IsInRangeProperty =
        DependencyProperty.Register(
            nameof(IsInRange),
            typeof(bool),
            typeof(DayCell),
            new PropertyMetadata(false));

    /// <summary>该日期处于 [RangeStart, RangeEnd] 范围内(含两端)。</summary>
    public bool IsInRange
    {
        get => (bool)GetValue(IsInRangeProperty);
        set => SetValue(IsInRangeProperty, value);
    }

    #endregion IsInRange

    #region IsInPreview

    public static readonly DependencyProperty IsInPreviewProperty =
        DependencyProperty.Register(
            nameof(IsInPreview),
            typeof(bool),
            typeof(DayCell),
            new PropertyMetadata(false));

    /// <summary>该日期处于"hover 预览"范围内(Start 已选、End 未选,鼠标 hover 时)。</summary>
    public bool IsInPreview
    {
        get => (bool)GetValue(IsInPreviewProperty);
        set => SetValue(IsInPreviewProperty, value);
    }

    #endregion IsInPreview

    #region IsHovered

    public static readonly DependencyProperty IsHoveredProperty =
        DependencyProperty.Register(
            nameof(IsHovered),
            typeof(bool),
            typeof(DayCell),
            new PropertyMetadata(false));

    /// <summary>
    /// 鼠标当前所在的格子(由父 MonthView 通过 hit-test 统一控制)。
    /// 用此 DP 而不用 IsMouseOver,是因为拖动时 capture 在 grid 层级,
    /// 单个 DayCell 的 IsMouseOver 可能不准确。
    /// </summary>
    public bool IsHovered
    {
        get => (bool)GetValue(IsHoveredProperty);
        set => SetValue(IsHoveredProperty, value);
    }

    #endregion IsHovered
}