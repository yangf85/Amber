using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class CalendarSample : UserControl
{
    public CalendarSample()
    {
        InitializeComponent();
        DataContext = new CalendarViewModel();
    }

    // BlackoutDates 是只读集合 (无 setter), 不能 TwoWay 绑定, 在 Loaded 时从 VM 写入
    private void OnAppointmentCalendarLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Calendar cal || DataContext is not CalendarViewModel vm)
        {
            return;
        }

        cal.BlackoutDates.Clear();
        foreach (var d in vm.Holidays)
        {
            cal.BlackoutDates.Add(new CalendarDateRange(d));
        }
    }
}

public partial class CalendarViewModel : ObservableObject
{
    // ① 基础绑定 — 单选
    [ObservableProperty]
    public partial DateTime? BirthDate { get; set; } = DateTime.Today;

    public string BirthDateDisplay => BirthDate.HasValue
        ? $"{BirthDate.Value:yyyy-MM-dd}  {GetWeekDayName(BirthDate.Value.DayOfWeek)}"
        : "(未选择)";

    partial void OnBirthDateChanged(DateTime? value) => OnPropertyChanged(nameof(BirthDateDisplay));

    // ② BlackoutDates 演示 — 节假日不可选
    public IList<DateTime> Holidays { get; } = new List<DateTime>
    {
        DateTime.Today.AddDays(-3),
        DateTime.Today.AddDays(-2),
        DateTime.Today.AddDays(5),
        DateTime.Today.AddDays(6),
        DateTime.Today.AddDays(7),
    };

    [ObservableProperty]
    public partial DateTime? AppointmentDate { get; set; }

    public string AppointmentDisplay => AppointmentDate.HasValue
        ? $"{AppointmentDate.Value:yyyy-MM-dd}  {GetWeekDayName(AppointmentDate.Value.DayOfWeek)}"
        : "(请选择一个非节假日)";

    partial void OnAppointmentDateChanged(DateTime? value) => OnPropertyChanged(nameof(AppointmentDisplay));

    // ④ 导航限制 — 只允许在 [今天, 今天 + 30 天] 范围内切换
    public DateTime BusinessTripStart { get; } = DateTime.Today;

    public DateTime BusinessTripEnd { get; } = DateTime.Today.AddDays(30);

    [ObservableProperty]
    public partial DateTime? TripDate { get; set; }

    // ⑤ 禁用态
    [ObservableProperty]
    public partial DateTime? FrozenDate { get; set; } = new DateTime(DateTime.Today.Year, 1, 1);

    // 操作
    [RelayCommand]
    private void SetBirthToday() => BirthDate = DateTime.Today;

    [RelayCommand]
    private void ClearBirthDate() => BirthDate = null;

    private static string GetWeekDayName(DayOfWeek d) => d switch
    {
        DayOfWeek.Monday => "星期一",
        DayOfWeek.Tuesday => "星期二",
        DayOfWeek.Wednesday => "星期三",
        DayOfWeek.Thursday => "星期四",
        DayOfWeek.Friday => "星期五",
        DayOfWeek.Saturday => "星期六",
        DayOfWeek.Sunday => "星期日",
        _ => "",
    };
}
