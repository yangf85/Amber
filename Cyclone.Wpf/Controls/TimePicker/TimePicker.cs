using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Globalization;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 时间选择器控件，集成了小时、分钟和秒的选择
/// </summary>
[TemplatePart(Name = "PART_OpenButton", Type = typeof(ToggleButton))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_HourSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_MinuteSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_SecondSelector", Type = typeof(TimeSelector))]
[TemplatePart(Name = "PART_ConfirmButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_CancelButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_DisplayText", Type = typeof(TextBox))]
public class TimePicker : Control
{
    #region 私有字段

    private Button _cancelButton;

    private Button _confirmButton;

    private TextBox _displayText;

    private TimeSelector _hourSelector;

    private bool _isSyncingSelectors = false;

    private TimeSelector _minuteSelector;

    private ToggleButton _openButton;

    /// <summary>
    /// Popup 打开时缓存的"原始时间" — Cancel / 点外部关闭时,
    /// 把 selectors 回滚到这个值,SelectedTime DP 不动。
    /// </summary>
    private TimeSpan? _originalTime;

    private Popup _popup;

    private TimeSelector _secondSelector;

    // 是否正在同步选择器

    #endregion 私有字段

    #region 构造函数

    private void TimePicker_Loaded(object sender, RoutedEventArgs e)
    {
        if (SelectedTime == null)
        {
            SelectedTime = DateTime.Now.TimeOfDay;
        }
        else
        {
            UpdateDisplayText();
        }
    }

    static TimePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
    }

    public TimePicker()
    {
        Loaded += TimePicker_Loaded;
    }

    #endregion 构造函数

    #region 依赖属性

    #region MaxContainerHeight

    public static readonly DependencyProperty MaxContainerHeightProperty =
        DependencyProperty.Register(nameof(MaxContainerHeight), typeof(double), typeof(TimePicker), new PropertyMetadata(150d));

    public double MaxContainerHeight
    {
        get => (double)GetValue(MaxContainerHeightProperty);
        set => SetValue(MaxContainerHeightProperty, value);
    }

    #endregion MaxContainerHeight

    #region SelectedTime

    public static readonly DependencyProperty SelectedTimeProperty =
        DependencyProperty.Register("SelectedTime", typeof(TimeSpan?), typeof(TimePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTimeChanged));

    public TimeSpan? SelectedTime
    {
        get { return (TimeSpan?)GetValue(SelectedTimeProperty); }
        set { SetValue(SelectedTimeProperty, value); }
    }

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timePicker = (TimePicker)d;
        timePicker.UpdateDisplayText();

        // Popup 打开时 SelectedTime 变化(外部 binding 改的),同步到 selectors
        if (timePicker.IsOpen && !timePicker._isSyncingSelectors)
        {
            timePicker.SyncSelectorsWithTime();
        }
    }

    #endregion SelectedTime

    #region DisplayText

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register("DisplayText", typeof(string), typeof(TimePicker), new PropertyMetadata(string.Empty));

    public string DisplayText
    {
        get { return (string)GetValue(DisplayTextProperty); }
        set { SetValue(DisplayTextProperty, value); }
    }

    #endregion DisplayText

    #region TimeFormat

    public static readonly DependencyProperty TimeFormatProperty =
        DependencyProperty.Register("TimeFormat", typeof(string), typeof(TimePicker),
            new PropertyMetadata("HH:mm:ss", OnTimeFormatChanged));

    public string TimeFormat
    {
        get { return (string)GetValue(TimeFormatProperty); }
        set { SetValue(TimeFormatProperty, value); }
    }

    private static void OnTimeFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timePicker = (TimePicker)d;
        timePicker.UpdateDisplayText();
    }

    #endregion TimeFormat

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register("IsOpen", typeof(bool), typeof(TimePicker),
            new PropertyMetadata(false, OnIsOpenChanged));

    public bool IsOpen
    {
        get { return (bool)GetValue(IsOpenProperty); }
        set { SetValue(IsOpenProperty, value); }
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var timePicker = (TimePicker)d;

        if ((bool)e.NewValue)
        {
            // 打开 — 缓存当前值作为 Snapshot,Cancel 回滚用
            timePicker._originalTime = timePicker.SelectedTime;
            timePicker.SyncSelectorsWithTime();
        }

        // 关闭不做事 — 取消/确定 button click 已经处理
    }

    #endregion IsOpen

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register("Watermark", typeof(string), typeof(TimePicker),
            new PropertyMetadata("请选择时间"));

    public string Watermark
    {
        get { return (string)GetValue(WatermarkProperty); }
        set { SetValue(WatermarkProperty, value); }
    }

    #endregion Watermark

    #region VisibleItemCount

    public static readonly DependencyProperty VisibleItemCountProperty =
        DependencyProperty.Register("VisibleItemCount", typeof(int), typeof(TimePicker),
            new PropertyMetadata(5));

    public int VisibleItemCount
    {
        get { return (int)GetValue(VisibleItemCountProperty); }
        set { SetValue(VisibleItemCountProperty, value); }
    }

    #endregion VisibleItemCount

    #endregion 依赖属性

    #region 私有方法

    /// <summary>
    /// 从选择器获取当前选中的时间
    /// </summary>
    private TimeSpan GetTimeFromSelectors()
    {
        // 如果选择器未初始化，则返回零时间
        if (_hourSelector == null || _minuteSelector == null || _secondSelector == null)
            return TimeSpan.Zero;

        try
        {
            // 从选择器获取值并构造TimeSpan
            int hour = _hourSelector.SelectedTimeValue;
            int minute = _minuteSelector.SelectedTimeValue;
            int second = _secondSelector.SelectedTimeValue;

            return new TimeSpan(hour, minute, second);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"从选择器获取时间错误: {ex.Message}");
            return TimeSpan.Zero;
        }
    }

    private void SyncSelectorsWithTime()
    {
        if (_hourSelector == null || _minuteSelector == null || _secondSelector == null) { return; }
        if (SelectedTime == null) { return; }

        // 设置 selectors 的 SelectedIndex 会触发 ValueChanged,在此期间标记同步中,
        // 防止 TimeSelector_ValueChanged 回头来覆盖 _originalTime / 触发 confirm 逻辑
        _isSyncingSelectors = true;
        try
        {
            _hourSelector.SelectedIndex = SelectedTime.Value.Hours;
            _minuteSelector.SelectedIndex = SelectedTime.Value.Minutes;
            _secondSelector.SelectedIndex = SelectedTime.Value.Seconds;
        }
        finally
        {
            _isSyncingSelectors = false;
        }
    }

    private void UpdateDisplayText()
    {
        if (SelectedTime.HasValue)
        {
            try
            {
                TimeSpan time = SelectedTime.Value;
                DateTime dateTime = DateTime.Today.Add(time);
                DisplayText = dateTime.ToString(TimeFormat);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"时间格式化错误: {ex.Message}");
                DisplayText = SelectedTime.Value.ToString();
            }
        }
        else
        {
            DisplayText = string.Empty;
        }
    }

    #endregion 私有方法

    #region 重写方法

    /// <summary>
    /// 应用模板时获取必要的模板部件
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        UnsubscribeEvents();

        _openButton = GetTemplateChild("PART_OpenButton") as ToggleButton;
        _popup = GetTemplateChild("PART_Popup") as Popup;
        _hourSelector = GetTemplateChild("PART_HourSelector") as TimeSelector;
        _minuteSelector = GetTemplateChild("PART_MinuteSelector") as TimeSelector;
        _secondSelector = GetTemplateChild("PART_SecondSelector") as TimeSelector;
        _confirmButton = GetTemplateChild("PART_ConfirmButton") as Button;
        _cancelButton = GetTemplateChild("PART_CancelButton") as Button;
        _displayText = GetTemplateChild("PART_DisplayText") as TextBox;

        SubscribeEvents();

        UpdateDisplayText();
    }

    /// <summary>
    /// 绑定事件
    /// </summary>
    private void SubscribeEvents()
    {
        if (_confirmButton != null)
        {
            _confirmButton.Click += ConfirmButton_Click;
        }

        if (_cancelButton != null)
        {
            _cancelButton.Click += CancelButton_Click;
        }

        // 绑定选择器事件
        if (_hourSelector != null)
        {
            _hourSelector.ValueChanged += TimeSelector_ValueChanged;
        }

        if (_minuteSelector != null)
        {
            _minuteSelector.ValueChanged += TimeSelector_ValueChanged;
        }

        if (_secondSelector != null)
        {
            _secondSelector.ValueChanged += TimeSelector_ValueChanged;
        }

        if (_popup != null)
        {
            _popup.Closed += Popup_Closed;
        }
    }

    /// <summary>
    /// 解除事件绑定
    /// </summary>
    private void UnsubscribeEvents()
    {
        if (_confirmButton != null)
        {
            _confirmButton.Click -= ConfirmButton_Click;
        }

        if (_cancelButton != null)
        {
            _cancelButton.Click -= CancelButton_Click;
        }

        // 解除选择器事件
        if (_hourSelector != null)
        {
            _hourSelector.ValueChanged -= TimeSelector_ValueChanged;
        }

        if (_minuteSelector != null)
        {
            _minuteSelector.ValueChanged -= TimeSelector_ValueChanged;
        }

        if (_secondSelector != null)
        {
            _secondSelector.ValueChanged -= TimeSelector_ValueChanged;
        }

        if (_popup != null)
        {
            _popup.Closed -= Popup_Closed;
        }
    }

    #endregion 重写方法

    #region 事件处理

    /// <summary>
    /// 取消按钮点击事件处理 — 真正回滚:把 selectors 同步回 _originalTime,
    /// SelectedTime DP 不动(因为本来确认前就没改)。
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        // 关键:把 selectors 回滚到打开 Popup 时的快照值,
        // 下次再打开 Popup 时看到的就是原值而不是用户上次"取消"前的滑动位置
        RollbackSelectorsToOriginal();
        IsOpen = false;
    }

    /// <summary>
    /// 确认按钮点击事件处理 — 把 selectors 当前值提交到 SelectedTime DP。
    /// </summary>
    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 从选择器获取时间值并提交
            SelectedTime = GetTimeFromSelectors();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"确认时间时出错: {ex.Message}");
        }

        IsOpen = false;
    }

    /// <summary>
    /// Popup关闭事件处理 — 点 Popup 外部时 StaysOpen=False 自动关闭,
    /// 此时视为"取消"(用户没按确定),回滚 selectors。
    /// </summary>
    private void Popup_Closed(object sender, EventArgs e)
    {
        if (IsOpen)
        {
            IsOpen = false;

            // 点外部 = 取消语义,回滚 selectors
            RollbackSelectorsToOriginal();
        }
    }

    /// <summary>把 selectors 视觉同步回 _originalTime,不动 SelectedTime DP。</summary>
    private void RollbackSelectorsToOriginal()
    {
        if (_originalTime == null) return;
        if (_hourSelector == null || _minuteSelector == null || _secondSelector == null) return;

        _isSyncingSelectors = true;
        try
        {
            _hourSelector.SelectedIndex = _originalTime.Value.Hours;
            _minuteSelector.SelectedIndex = _originalTime.Value.Minutes;
            _secondSelector.SelectedIndex = _originalTime.Value.Seconds;
        }
        finally
        {
            _isSyncingSelectors = false;
        }
    }

    /// <summary>
    /// 时间选择器值变化事件处理 — 旧实现写了 _tempSelectedTime 但 Confirm 不用,
    /// 现在改成 Snapshot 机制,这个事件保留只是为了将来扩展(如实时显示)。
    /// </summary>
    private void TimeSelector_ValueChanged(object sender, TimeValueChangedEventArgs e)
    {
        if (_isSyncingSelectors) return;

        // 不做任何事 — 用户操作 selectors 只更新它们自身状态,
        // 确认前不污染 SelectedTime DP。Confirm 时 GetTimeFromSelectors 取当前值。
    }

    #endregion 事件处理
}