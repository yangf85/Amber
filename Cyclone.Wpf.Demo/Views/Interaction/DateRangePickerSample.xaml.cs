using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class DateRangePickerSample : UserControl
{
    public DateRangePickerSample()
    {
        InitializeComponent();
        DataContext = new DateRangePickerViewModel();
    }
}

public partial class DateRangePickerViewModel : ObservableObject
{
    // ① 基础绑定
    [ObservableProperty]
    public partial DateTime? OrderStart { get; set; } = DateTime.Today.AddDays(-7);

    [ObservableProperty]
    public partial DateTime? OrderEnd { get; set; } = DateTime.Today;

    public int OrderDays => (OrderStart.HasValue && OrderEnd.HasValue)
        ? (OrderEnd.Value - OrderStart.Value).Days + 1
        : 0;

    partial void OnOrderStartChanged(DateTime? value) => OnPropertyChanged(nameof(OrderDays));

    partial void OnOrderEndChanged(DateTime? value) => OnPropertyChanged(nameof(OrderDays));

    // ② BlackoutDates 演示 — 节假日不可选
    public IList<DateTime> Holidays { get; } = new List<DateTime>
    {
        DateTime.Today.AddDays(-3),
        DateTime.Today.AddDays(-2),
        DateTime.Today.AddDays(2),
        DateTime.Today.AddDays(3),
    };

    // ③ 自定义预定义范围 — 业务场景:只显示 Q1/Q2/Q3/Q4
    public IList<IPredefinedRange> QuarterRanges { get; } = new List<IPredefinedRange>
    {
        new PredefinedRange("Q1 (1-3 月)",
            new DateTime(DateTime.Today.Year, 1, 1),
            new DateTime(DateTime.Today.Year, 3, 31)),
        new PredefinedRange("Q2 (4-6 月)",
            new DateTime(DateTime.Today.Year, 4, 1),
            new DateTime(DateTime.Today.Year, 6, 30)),
        new PredefinedRange("Q3 (7-9 月)",
            new DateTime(DateTime.Today.Year, 7, 1),
            new DateTime(DateTime.Today.Year, 9, 30)),
        new PredefinedRange("Q4 (10-12 月)",
            new DateTime(DateTime.Today.Year, 10, 1),
            new DateTime(DateTime.Today.Year, 12, 31)),
    };

    [ObservableProperty]
    public partial DateTime? QuarterStart { get; set; }

    [ObservableProperty]
    public partial DateTime? QuarterEnd { get; set; }

    // ④ 无预定义范围 — 极简模式
    [ObservableProperty]
    public partial DateTime? PlainStart { get; set; }

    [ObservableProperty]
    public partial DateTime? PlainEnd { get; set; }

    // ⑤ 禁用态
    [ObservableProperty]
    public partial DateTime? FrozenStart { get; set; } = new DateTime(2024, 1, 1);

    [ObservableProperty]
    public partial DateTime? FrozenEnd { get; set; } = new DateTime(2024, 12, 31);

    // 操作
    [RelayCommand]
    private void ClearOrderRange()
    {
        OrderStart = null;
        OrderEnd = null;
    }

    [RelayCommand]
    private void SetLastWeek()
    {
        OrderStart = DateTime.Today.AddDays(-7);
        OrderEnd = DateTime.Today;
    }
}
