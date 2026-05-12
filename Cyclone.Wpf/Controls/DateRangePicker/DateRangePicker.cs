using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 日期范围选择器 — 用户在弹出 Popup 中通过两次点击选取日期范围,
/// 第一次点 = RangeStart,第二次点 = RangeEnd(自动按时间排序),第三次点重置。
/// 鼠标 hover 时显示从 RangeStart 到鼠标位置的预览高亮。
/// 左侧可选预定义范围面板(今天/近 7 天/本月等)。
/// </summary>
[TemplatePart(Name = PART_ToggleButton, Type = typeof(ToggleButton))]
[TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
[TemplatePart(Name = PART_MonthView, Type = typeof(MonthView))]
[TemplatePart(Name = PART_PredefinedList, Type = typeof(ListBox))]
[TemplatePart(Name = PART_ConfirmButton, Type = typeof(Button))]
[TemplatePart(Name = PART_CancelButton, Type = typeof(Button))]
public class DateRangePicker : Control
{
    private const string PART_CancelButton = nameof(PART_CancelButton);

    private const string PART_ConfirmButton = nameof(PART_ConfirmButton);

    private const string PART_MonthView = nameof(PART_MonthView);

    private const string PART_Popup = nameof(PART_Popup);

    private const string PART_PredefinedList = nameof(PART_PredefinedList);

    private const string PART_ToggleButton = nameof(PART_ToggleButton);

    private Button _cancelButton;

    private Button _confirmButton;

    /// <summary>
    /// 反向同步保护 — 内部更新 RangeStart/RangeEnd 时设 true,
    /// 防止 OnRangeStartChanged/OnRangeEndChanged 把状态机重置。
    /// </summary>
    private bool _isInternalUpdate;

    private MonthView _monthView;

    private DateTime? _pendingEnd;

    /// <summary>
    /// 暂存值 — Popup 打开期间的用户操作只更新这两个字段,
    /// 不立刻 push 到 RangeStart/RangeEnd DP。只有"确定"时才提交。
    /// 这样"取消"可以无副作用地回滚。
    /// </summary>
    private DateTime? _pendingStart;

    private Popup _popup;

    private ListBox _predefinedList;

    private DateTime? _snapshotEnd;

    /// <summary>Popup 打开时的"原始"值 — 取消时回滚到这里。</summary>
    private DateTime? _snapshotStart;

    private ToggleButton _toggleButton;

    static DateRangePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DateRangePicker),
            new FrameworkPropertyMetadata(typeof(DateRangePicker)));
    }

    public DateRangePicker()
    {
        // 默认预定义范围(用户可以覆盖或清空)
        PredefinedRanges = PredefinedRangeGenerator.Generate();
    }

    #region RangeStart

    public static readonly DependencyProperty RangeStartProperty =
        DependencyProperty.Register(
            nameof(RangeStart),
            typeof(DateTime?),
            typeof(DateRangePicker),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRangeStartChanged));

    /// <summary>范围起始日期。</summary>
    public DateTime? RangeStart
    {
        get => (DateTime?)GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }

    private static void OnRangeStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DateRangePicker picker && !picker._isInternalUpdate)
        {
            // 外部 binding 设了新值 — 跟随 DisplayMonth 跳到新月份(便于查看)
            if (e.NewValue is DateTime newStart && picker._monthView != null)
            {
                picker._monthView.DisplayMonth = new DateTime(newStart.Year, newStart.Month, 1);
            }
        }
    }

    #endregion RangeStart

    #region RangeEnd

    public static readonly DependencyProperty RangeEndProperty =
        DependencyProperty.Register(
            nameof(RangeEnd),
            typeof(DateTime?),
            typeof(DateRangePicker),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>范围结束日期。</summary>
    public DateTime? RangeEnd
    {
        get => (DateTime?)GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }

    #endregion RangeEnd

    #region DateFormat

    public static readonly DependencyProperty DateFormatProperty =
        DependencyProperty.Register(
            nameof(DateFormat),
            typeof(string),
            typeof(DateRangePicker),
            new PropertyMetadata("yyyy-MM-dd"));

    /// <summary>日期文本框显示格式。</summary>
    public string DateFormat
    {
        get => (string)GetValue(DateFormatProperty);
        set => SetValue(DateFormatProperty, value);
    }

    #endregion DateFormat

    #region FirstDayOfWeek

    public static readonly DependencyProperty FirstDayOfWeekProperty =
        DependencyProperty.Register(
            nameof(FirstDayOfWeek),
            typeof(DayOfWeek),
            typeof(DateRangePicker),
            new PropertyMetadata(DayOfWeek.Monday));

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
            typeof(DateRangePicker),
            new PropertyMetadata(true));

    public bool IsTodayHighlighted
    {
        get => (bool)GetValue(IsTodayHighlightedProperty);
        set => SetValue(IsTodayHighlightedProperty, value);
    }

    #endregion IsTodayHighlighted

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(DateRangePicker),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsOpenChanged));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DateRangePicker picker) return;

        if ((bool)e.NewValue)
        {
            // 打开 — 缓存当前值作为快照(取消时回滚),初始化暂存值
            picker._snapshotStart = picker.RangeStart;
            picker._snapshotEnd = picker.RangeEnd;
            picker._pendingStart = picker.RangeStart;
            picker._pendingEnd = picker.RangeEnd;

            // 月历跳到 Start 所在月份(便于继续编辑)
            if (picker._monthView != null)
            {
                var anchor = picker.RangeStart ?? DateTime.Today;
                picker._monthView.DisplayMonth = new DateTime(anchor.Year, anchor.Month, 1);
                picker._monthView.RangeStart = picker._pendingStart;
                picker._monthView.RangeEnd = picker._pendingEnd;
            }
        }
    }

    #endregion IsOpen

    #region Separator

    public static readonly DependencyProperty SeparatorProperty =
        DependencyProperty.Register(
            nameof(Separator),
            typeof(object),
            typeof(DateRangePicker),
            new PropertyMetadata("→"));

    /// <summary>两个日期文本框之间的分隔显示内容(默认 "→")。</summary>
    public object Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    #endregion Separator

    #region BlackoutDates

    public static readonly DependencyProperty BlackoutDatesProperty =
        DependencyProperty.Register(
            nameof(BlackoutDates),
            typeof(IList<DateTime>),
            typeof(DateRangePicker),
            new PropertyMetadata(null));

    /// <summary>禁止选择的日期列表(可绑定到 MonthView 透传)。</summary>
    public IList<DateTime> BlackoutDates
    {
        get => (IList<DateTime>)GetValue(BlackoutDatesProperty);
        set => SetValue(BlackoutDatesProperty, value);
    }

    #endregion BlackoutDates

    #region ShowPredefinedRanges

    public static readonly DependencyProperty ShowPredefinedRangesProperty =
        DependencyProperty.Register(
            nameof(ShowPredefinedRanges),
            typeof(bool),
            typeof(DateRangePicker),
            new PropertyMetadata(true));

    /// <summary>是否在弹出 Popup 左侧显示预定义范围列表。</summary>
    public bool ShowPredefinedRanges
    {
        get => (bool)GetValue(ShowPredefinedRangesProperty);
        set => SetValue(ShowPredefinedRangesProperty, value);
    }

    #endregion ShowPredefinedRanges

    #region PredefinedRanges

    public static readonly DependencyProperty PredefinedRangesProperty =
        DependencyProperty.Register(
            nameof(PredefinedRanges),
            typeof(IList<IPredefinedRange>),
            typeof(DateRangePicker),
            new PropertyMetadata(null));

    /// <summary>
    /// 预定义范围列表。控件构造时填充默认 17 项,用户可在 XAML 中覆盖:
    /// 设空集合则隐藏列表,设自定义集合则显示用户的列表。
    /// </summary>
    public IList<IPredefinedRange> PredefinedRanges
    {
        get => (IList<IPredefinedRange>)GetValue(PredefinedRangesProperty);
        set => SetValue(PredefinedRangesProperty, value);
    }

    #endregion PredefinedRanges

    #region Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解绑旧引用
        if (_monthView != null)
        {
            _monthView.RemoveHandler(MonthView.DateClickedEvent, new EventHandler<DateClickedEventArgs>(OnMonthViewDateClicked));
            _monthView.MouseLeave -= OnMonthViewMouseLeave;
        }
        if (_predefinedList != null) _predefinedList.SelectionChanged -= OnPredefinedSelectionChanged;
        if (_popup != null) _popup.Closed -= OnPopupClosed;
        if (_confirmButton != null) _confirmButton.Click -= OnConfirmClicked;
        if (_cancelButton != null) _cancelButton.Click -= OnCancelClicked;

        _toggleButton = GetTemplateChild(PART_ToggleButton) as ToggleButton;
        _popup = GetTemplateChild(PART_Popup) as Popup;
        _monthView = GetTemplateChild(PART_MonthView) as MonthView;
        _predefinedList = GetTemplateChild(PART_PredefinedList) as ListBox;
        _confirmButton = GetTemplateChild(PART_ConfirmButton) as Button;
        _cancelButton = GetTemplateChild(PART_CancelButton) as Button;

        if (_monthView != null)
        {
            _monthView.AddHandler(MonthView.DateClickedEvent, new EventHandler<DateClickedEventArgs>(OnMonthViewDateClicked));
            _monthView.MouseLeave += OnMonthViewMouseLeave;
        }
        if (_predefinedList != null) _predefinedList.SelectionChanged += OnPredefinedSelectionChanged;
        if (_popup != null) _popup.Closed += OnPopupClosed;
        if (_confirmButton != null) _confirmButton.Click += OnConfirmClicked;
        if (_cancelButton != null) _cancelButton.Click += OnCancelClicked;
    }

    #endregion Override

    #region Private — interaction handlers

    /// <summary>"取消"按钮 — 回滚到打开 Popup 前的值,关闭 Popup。</summary>
    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        _pendingStart = _snapshotStart;
        _pendingEnd = _snapshotEnd;
        if (_monthView != null)
        {
            _monthView.RangeStart = _snapshotStart;
            _monthView.RangeEnd = _snapshotEnd;
            _monthView.PreviewDate = null;
        }
        IsOpen = false;
    }

    /// <summary>"确定"按钮 — 把暂存值提交到 DP。</summary>
    private void OnConfirmClicked(object sender, RoutedEventArgs e)
    {
        _isInternalUpdate = true;
        try
        {
            RangeStart = _pendingStart;
            RangeEnd = _pendingEnd;
            IsOpen = false;
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    /// <summary>
    /// 用户在 Calendar 上点击或拖动到某个日期。
    /// - 普通点击:走两次点击状态机(写入 _pendingStart / _pendingEnd 暂存,不立刻 push DP)
    /// - 拖动:第一个点设 Start,后续点持续更新 End
    /// </summary>
    private void OnMonthViewDateClicked(object sender, DateClickedEventArgs e)
    {
        if (_monthView == null) return;

        if (e.IsDragging)
        {
            // 拖动:第一次进 PickingEnd 后,后续 hover 持续更新 _pendingEnd
            if (_pendingStart == null)
            {
                _pendingStart = e.Date;
                _pendingEnd = null;
            }
            else
            {
                // 自动按时间排序
                if (e.Date < _pendingStart.Value)
                {
                    _pendingEnd = _pendingStart;
                    _pendingStart = e.Date;
                }
                else
                {
                    _pendingEnd = e.Date;
                }
            }
        }
        else
        {
            // 普通点击 — 两次点击状态机
            if (_pendingStart == null || (_pendingStart != null && _pendingEnd != null))
            {
                _pendingStart = e.Date;
                _pendingEnd = null;
            }
            else
            {
                if (e.Date < _pendingStart.Value)
                {
                    _pendingEnd = _pendingStart;
                    _pendingStart = e.Date;
                }
                else
                {
                    _pendingEnd = e.Date;
                }
            }
        }

        // 视觉上更新 MonthView (不通过 DP push,仅显示暂存)
        _monthView.RangeStart = _pendingStart;
        _monthView.RangeEnd = _pendingEnd;
        _monthView.PreviewDate = null;
    }

    /// <summary>
    /// 鼠标离开 MonthView — 清预览 + hover,避免离开后还残留高亮。
    /// </summary>
    private void OnMonthViewMouseLeave(object sender, MouseEventArgs e)
    {
        if (_monthView != null)
        {
            _monthView.PreviewDate = null;
            _monthView.HoverDate = null;
        }
    }

    /// <summary>
    /// Popup 关闭(用户点外部触发 StaysOpen=False) — 视同"取消",回滚。
    /// 不做这一步的话,WPF 的 Popup 内部 close 不会 push 回绑定的 IsOpen,
    /// 下次按按钮 toggle 会"假状态"导致点一下没反应,要点两下。
    /// </summary>
    private void OnPopupClosed(object sender, EventArgs e)
    {
        if (IsOpen) IsOpen = false;

        // 没按确定关闭 = 取消,暂存值回滚但不动 DP(因为本来就没 push)
        _pendingStart = _snapshotStart;
        _pendingEnd = _snapshotEnd;
        if (_monthView != null)
        {
            _monthView.RangeStart = _snapshotStart;
            _monthView.RangeEnd = _snapshotEnd;
            _monthView.PreviewDate = null;
        }
    }

    /// <summary>
    /// 用户点击预定义范围 — 直接提交并关闭 Popup(预定义不需要再点确定)。
    /// </summary>
    private void OnPredefinedSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_predefinedList?.SelectedItem is not IPredefinedRange range) return;

        _isInternalUpdate = true;
        try
        {
            // 预定义范围直接 commit
            _pendingStart = range.Start;
            _pendingEnd = range.End;
            RangeStart = _pendingStart;
            RangeEnd = _pendingEnd;
            if (_monthView != null) _monthView.PreviewDate = null;
            IsOpen = false;
        }
        finally
        {
            _isInternalUpdate = false;

            // 清掉选中态,避免下次打开 Popup 时还高亮上次的选择
            _predefinedList.SelectedItem = null;
        }
    }

    #endregion Private — interaction handlers
}