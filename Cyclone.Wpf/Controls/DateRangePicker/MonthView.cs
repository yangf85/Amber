using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 月历视图 — DateRangePicker 的核心 sub-component。
/// 不使用 WPF Calendar (其 SelectionMode 不支持两次点击选范围 + hover 预览语义),
/// 而是用 Grid 6 行 × 7 列共 42 个 DayCell 自己管选中态/范围/hover 预览。
/// </summary>
[TemplatePart(Name = PART_PrevButton, Type = typeof(Button))]
[TemplatePart(Name = PART_NextButton, Type = typeof(Button))]
[TemplatePart(Name = PART_HeaderText, Type = typeof(TextBlock))]
[TemplatePart(Name = PART_DayGrid, Type = typeof(Grid))]
[TemplatePart(Name = PART_WeekHeader, Type = typeof(Grid))]
public class MonthView : Control
{
    private const int DayCols = 7;

    private const int DayCount = DayRows * DayCols;

    private const int DayRows = 6;

    private const string PART_DayGrid = nameof(PART_DayGrid);

    private const string PART_HeaderText = nameof(PART_HeaderText);

    private const string PART_NextButton = nameof(PART_NextButton);

    private const string PART_PrevButton = nameof(PART_PrevButton);

    private const string PART_WeekHeader = nameof(PART_WeekHeader);

    private readonly DayCell[] _dayCells = new DayCell[DayCount];

    private Grid _dayGrid;

    private TextBlock _headerText;

    private Button _nextButton;

    private Button _prevButton;

    private Grid _weekHeader;

    static MonthView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MonthView),
            new FrameworkPropertyMetadata(typeof(MonthView)));
    }

    #region DisplayMonth

    public static readonly DependencyProperty DisplayMonthProperty =
        DependencyProperty.Register(
            nameof(DisplayMonth),
            typeof(DateTime),
            typeof(MonthView),
            new FrameworkPropertyMetadata(DateTime.Today,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRefreshNeeded));

    /// <summary>当前显示的月份(取年/月,日期部分忽略)。</summary>
    public DateTime DisplayMonth
    {
        get => (DateTime)GetValue(DisplayMonthProperty);
        set => SetValue(DisplayMonthProperty, value);
    }

    #endregion DisplayMonth

    #region RangeStart

    public static readonly DependencyProperty RangeStartProperty =
        DependencyProperty.Register(
            nameof(RangeStart),
            typeof(DateTime?),
            typeof(MonthView),
            new PropertyMetadata(null, OnRefreshNeeded));

    /// <summary>已选范围的起始日期(null 表示未选)。</summary>
    public DateTime? RangeStart
    {
        get => (DateTime?)GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }

    #endregion RangeStart

    #region RangeEnd

    public static readonly DependencyProperty RangeEndProperty =
        DependencyProperty.Register(
            nameof(RangeEnd),
            typeof(DateTime?),
            typeof(MonthView),
            new PropertyMetadata(null, OnRefreshNeeded));

    /// <summary>已选范围的结束日期(null 表示未选)。</summary>
    public DateTime? RangeEnd
    {
        get => (DateTime?)GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }

    #endregion RangeEnd

    #region HoverDate

    public static readonly DependencyProperty HoverDateProperty =
        DependencyProperty.Register(
            nameof(HoverDate),
            typeof(DateTime?),
            typeof(MonthView),
            new PropertyMetadata(null, OnRefreshNeeded));

    /// <summary>
    /// 当前鼠标所在的日期 — 用于显示"我现在在哪"的浅色高亮,
    /// 不同于 PreviewDate (只在 PickingEnd 时才有意义)。
    /// </summary>
    public DateTime? HoverDate
    {
        get => (DateTime?)GetValue(HoverDateProperty);
        set => SetValue(HoverDateProperty, value);
    }

    #endregion HoverDate

    #region PreviewDate

    public static readonly DependencyProperty PreviewDateProperty =
        DependencyProperty.Register(
            nameof(PreviewDate),
            typeof(DateTime?),
            typeof(MonthView),
            new PropertyMetadata(null, OnRefreshNeeded));

    /// <summary>
    /// hover 预览日期。只在 RangeStart 已设且 RangeEnd 未设时有意义 —
    /// 从 RangeStart 到 PreviewDate 之间显示淡色预览高亮。
    /// </summary>
    public DateTime? PreviewDate
    {
        get => (DateTime?)GetValue(PreviewDateProperty);
        set => SetValue(PreviewDateProperty, value);
    }

    #endregion PreviewDate

    #region FirstDayOfWeek

    public static readonly DependencyProperty FirstDayOfWeekProperty =
        DependencyProperty.Register(
            nameof(FirstDayOfWeek),
            typeof(DayOfWeek),
            typeof(MonthView),
            new PropertyMetadata(DayOfWeek.Monday, OnRefreshNeeded));

    /// <summary>一周的第一天(默认周一)。</summary>
    public DayOfWeek FirstDayOfWeek
    {
        get => (DayOfWeek)GetValue(FirstDayOfWeekProperty);
        set => SetValue(FirstDayOfWeekProperty, value);
    }

    #endregion FirstDayOfWeek

    #region IsTodayHighlighted

    public static readonly DependencyProperty IsTodayHighlightedProperty =
        DependencyProperty.Register(
            nameof(IsTodayHighlighted),
            typeof(bool),
            typeof(MonthView),
            new PropertyMetadata(true, OnRefreshNeeded));

    /// <summary>今天是否加粗高亮。</summary>
    public bool IsTodayHighlighted
    {
        get => (bool)GetValue(IsTodayHighlightedProperty);
        set => SetValue(IsTodayHighlightedProperty, value);
    }

    #endregion IsTodayHighlighted

    #region BlackoutDates

    public static readonly DependencyProperty BlackoutDatesProperty =
        DependencyProperty.Register(
            nameof(BlackoutDates),
            typeof(IList<DateTime>),
            typeof(MonthView),
            new PropertyMetadata(null, OnRefreshNeeded));

    /// <summary>禁止选择的日期列表。</summary>
    public IList<DateTime> BlackoutDates
    {
        get => (IList<DateTime>)GetValue(BlackoutDatesProperty);
        set => SetValue(BlackoutDatesProperty, value);
    }

    #endregion BlackoutDates

    #region DateClicked event

    public static readonly RoutedEvent DateClickedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(DateClicked),
            RoutingStrategy.Bubble,
            typeof(EventHandler<DateClickedEventArgs>),
            typeof(MonthView));

    /// <summary>用户点击某个日期(non-blackout)时触发。</summary>
    public event EventHandler<DateClickedEventArgs> DateClicked
    {
        add => AddHandler(DateClickedEvent, value);
        remove => RemoveHandler(DateClickedEvent, value);
    }

    #endregion DateClicked event

    #region Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_prevButton != null) _prevButton.Click -= OnPrevClicked;
        if (_nextButton != null) _nextButton.Click -= OnNextClicked;

        _prevButton = GetTemplateChild(PART_PrevButton) as Button;
        _nextButton = GetTemplateChild(PART_NextButton) as Button;
        _headerText = GetTemplateChild(PART_HeaderText) as TextBlock;
        _dayGrid = GetTemplateChild(PART_DayGrid) as Grid;
        _weekHeader = GetTemplateChild(PART_WeekHeader) as Grid;

        if (_prevButton != null) _prevButton.Click += OnPrevClicked;
        if (_nextButton != null) _nextButton.Click += OnNextClicked;

        BuildCells();
        Refresh();
    }

    #endregion Override

    #region Private

    private DayCell _dragStartCell;

    /// <summary>用户是否在拖动中。</summary>
    private bool _isDragging;

    private static void OnRefreshNeeded(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MonthView mv && mv._dayGrid != null)
        {
            mv.Refresh();
        }
    }

    /// <summary>
    /// 在 OnApplyTemplate 调用一次 — 创建 42 个 DayCell + 周标头。
    /// </summary>
    private void BuildCells()
    {
        if (_dayGrid == null) return;

        // 解绑旧 handler 防重复(OnApplyTemplate 可能多次触发)
        _dayGrid.PreviewMouseLeftButtonDown -= OnGridMouseDown;
        _dayGrid.PreviewMouseLeftButtonUp -= OnGridMouseUp;
        _dayGrid.MouseMove -= OnGridMouseMove;

        _dayGrid.Children.Clear();
        _dayGrid.RowDefinitions.Clear();
        _dayGrid.ColumnDefinitions.Clear();
        for (int i = 0; i < DayRows; i++) _dayGrid.RowDefinitions.Add(new RowDefinition());
        for (int i = 0; i < DayCols; i++) _dayGrid.ColumnDefinitions.Add(new ColumnDefinition());

        for (int i = 0; i < DayCount; i++)
        {
            var cell = new DayCell();
            cell.Click += OnDayClicked;
            Grid.SetRow(cell, i / DayCols);
            Grid.SetColumn(cell, i % DayCols);
            _dayGrid.Children.Add(cell);
            _dayCells[i] = cell;
        }

        // grid 层统一处理拖动 + hover hit-test
        _dayGrid.PreviewMouseLeftButtonDown += OnGridMouseDown;
        _dayGrid.PreviewMouseLeftButtonUp += OnGridMouseUp;
        _dayGrid.MouseMove += OnGridMouseMove;

        // 周标头
        if (_weekHeader != null)
        {
            _weekHeader.Children.Clear();
            _weekHeader.ColumnDefinitions.Clear();
            for (int i = 0; i < DayCols; i++)
            {
                _weekHeader.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }
    }

    /// <summary>从鼠标事件参数找出鼠标当前所在的 DayCell。</summary>
    private DayCell FindCellAtMouse(MouseEventArgs e)
    {
        if (_dayGrid == null) return null;
        var pos = e.GetPosition(_dayGrid);
        var hit = _dayGrid.InputHitTest(pos) as DependencyObject;
        while (hit != null && hit is not DayCell)
        {
            hit = System.Windows.Media.VisualTreeHelper.GetParent(hit);
        }
        return hit as DayCell;
    }

    private void OnDayClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not DayCell cell || cell.IsBlackout) return;
        if (_isDragging) return;  // 拖动中由 grid 层处理,不重复抛

        RaiseEvent(new DateClickedEventArgs(DateClickedEvent, this, cell.Date));
    }

    private void OnDayLeft(object sender, MouseEventArgs e)
    {
        // grid 级别的 MouseLeave 由父控件统一处理
    }

    /// <summary>
    /// 在 grid 层级处理 MouseDown — 找到当前 cell,记录拖动起点。
    /// 在 grid 层处理而不是 DayCell 层,是为了不干扰 ButtonBase 的 Click 事件。
    /// </summary>
    private void OnGridMouseDown(object sender, MouseButtonEventArgs e)
    {
        var cell = FindCellAtMouse(e);
        if (cell == null || cell.IsBlackout) return;

        _isDragging = false;  // 还没真正拖,先标记起点
        _dragStartCell = cell;

        // 按下瞬间:作为拖动的"起点候选" —— 视觉先 highlight 它
        // 但不发 DateClicked,等到鼠标真正移动到另一个 cell 才确认是拖动
        HoverDate = cell.Date;
    }

    private void OnGridMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            // 鼠标没按 — 普通 hover
            var cell = FindCellAtMouse(e);
            if (cell != null && !cell.IsBlackout && cell.IsCurrentMonth)
            {
                HoverDate = cell.Date;

                // 仅在 PickingEnd 状态(Start 已设、End 未设)时设预览
                if (RangeStart.HasValue && !RangeEnd.HasValue)
                {
                    PreviewDate = cell.Date;
                }
            }
            return;
        }

        // 鼠标按住中 — 检测进入拖动态
        if (_dragStartCell == null) return;

        var currentCell = FindCellAtMouse(e);
        if (currentCell == null || currentCell.IsBlackout) return;

        // 鼠标移动到了另一个 cell —> 这是真拖动
        if (!ReferenceEquals(currentCell, _dragStartCell))
        {
            if (!_isDragging)
            {
                // 首次进拖动态 — 把起点 cell 当 Start raise 一次
                _isDragging = true;
                RaiseEvent(new DateClickedEventArgs(DateClickedEvent, this, _dragStartCell.Date) { IsDragging = true });
            }

            // 后续每次移动 — 更新 End
            RaiseEvent(new DateClickedEventArgs(DateClickedEvent, this, currentCell.Date) { IsDragging = true });
            HoverDate = currentCell.Date;
        }
    }

    private void OnGridMouseUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
        _dragStartCell = null;

        // 不清 HoverDate — 让用户能看到最后停留位置
    }

    private void OnNextClicked(object sender, RoutedEventArgs e)
    {
        DisplayMonth = DisplayMonth.AddMonths(1);
    }

    private void OnPrevClicked(object sender, RoutedEventArgs e)
    {
        DisplayMonth = DisplayMonth.AddMonths(-1);
    }

    /// <summary>更新 42 个 DayCell 的日期/选中态/预览态/禁用态。</summary>
    private void Refresh()
    {
        if (_dayGrid == null || _dayCells[0] == null) return;

        var firstOfMonth = new DateTime(DisplayMonth.Year, DisplayMonth.Month, 1);
        var dayOfWeekValue = ((int)firstOfMonth.DayOfWeek - (int)FirstDayOfWeek + 7) % 7;
        var firstCellDate = firstOfMonth.AddDays(-dayOfWeekValue);

        // 月份标题
        if (_headerText != null)
        {
            _headerText.Text = firstOfMonth.ToString("yyyy 年 M 月", CultureInfo.CurrentCulture);
        }

        // 周标头
        UpdateWeekHeader();

        // 范围排序 — 处理用户跨范围选 (Start > End 情况)
        DateTime? rangeMin = null, rangeMax = null;
        if (RangeStart.HasValue && RangeEnd.HasValue)
        {
            rangeMin = RangeStart.Value < RangeEnd.Value ? RangeStart : RangeEnd;
            rangeMax = RangeStart.Value < RangeEnd.Value ? RangeEnd : RangeStart;
        }

        // 预览范围 — 仅在 RangeStart 已设、RangeEnd 未设、PreviewDate 已设时显示
        DateTime? previewMin = null, previewMax = null;
        if (RangeStart.HasValue && !RangeEnd.HasValue && PreviewDate.HasValue)
        {
            previewMin = RangeStart.Value < PreviewDate.Value ? RangeStart : PreviewDate;
            previewMax = RangeStart.Value < PreviewDate.Value ? PreviewDate : RangeStart;
        }

        var today = DateTime.Today;

        for (int i = 0; i < DayCount; i++)
        {
            var date = firstCellDate.AddDays(i);
            var cell = _dayCells[i];
            cell.Date = date;
            cell.DayText = date.Day.ToString();
            cell.IsCurrentMonth = date.Month == DisplayMonth.Month && date.Year == DisplayMonth.Year;
            cell.IsToday = IsTodayHighlighted && date.Date == today;
            cell.IsBlackout = BlackoutDates != null && BlackoutDates.Contains(date.Date);

            // 选中范围 — 完整 [min, max] (含两端)
            cell.IsRangeStart = rangeMin.HasValue && date.Date == rangeMin.Value.Date;
            cell.IsRangeEnd = rangeMax.HasValue && date.Date == rangeMax.Value.Date;
            cell.IsInRange = rangeMin.HasValue && rangeMax.HasValue
                          && date.Date >= rangeMin.Value.Date && date.Date <= rangeMax.Value.Date;

            // 预览范围 — Start 已设但 End 未设时,从 Start 到 Hover 的预览段
            cell.IsInPreview = !cell.IsInRange
                            && previewMin.HasValue && previewMax.HasValue
                            && date.Date >= previewMin.Value.Date && date.Date <= previewMax.Value.Date;

            // 仅 Start 已设但还没 End 时,Start 自身也算选中
            if (RangeStart.HasValue && !RangeEnd.HasValue && date.Date == RangeStart.Value.Date)
            {
                cell.IsRangeStart = true;
            }

            // Hover 视觉反馈 — 鼠标当前所在的格子加亮显示
            cell.IsHovered = HoverDate.HasValue && date.Date == HoverDate.Value.Date;
        }
    }

    private void UpdateWeekHeader()
    {
        if (_weekHeader == null) return;

        // 重建文字 (BuildCells 已经准备好 Column,这里填内容)
        if (_weekHeader.Children.Count != DayCols)
        {
            _weekHeader.Children.Clear();
            for (int i = 0; i < DayCols; i++)
            {
                var tb = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(tb, i);
                _weekHeader.Children.Add(tb);
            }
        }

        string[] names = { "日", "一", "二", "三", "四", "五", "六" };
        for (int i = 0; i < DayCols; i++)
        {
            int dayIdx = ((int)FirstDayOfWeek + i) % 7;
            ((TextBlock)_weekHeader.Children[i]).Text = names[dayIdx];
        }
    }

    #endregion Private
}

#region DateClickedEventArgs

public class DateClickedEventArgs : RoutedEventArgs
{
    public DateTime Date { get; }

    /// <summary>是否处于拖动中(MouseDown 后跨格 MouseEnter)。父控件用来区分单次点击 vs 拖动选范围。</summary>
    public bool IsDragging { get; set; }

    public DateClickedEventArgs(RoutedEvent routedEvent, object source, DateTime date)
        : base(routedEvent, source)
    {
        Date = date;
    }
}

#endregion DateClickedEventArgs