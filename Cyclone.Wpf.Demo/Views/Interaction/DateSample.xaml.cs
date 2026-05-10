using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// DateView.xaml 的交互逻辑
/// </summary>
public partial class DateSample : UserControl
{
    public DateSample()
    {
        InitializeComponent();
        DataContext = new DateViewModel();
    }
}

public partial class DateViewModel : ObservableObject
{
    #region DateRangePicker 属性

    [ObservableProperty]
    public partial DateTime StartDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

    [ObservableProperty]
    public partial DateTime StartDate2 { get; set; } = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    public partial DateTime EndDate2 { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial DateTime StartDate3 { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial DateTime EndDate3 { get; set; } = DateTime.Today.AddDays(10);

    [ObservableProperty]
    public partial string DateRangeInfo { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<DateTime> BlockoutDates { get; set; }

    #endregion DateRangePicker 属性

    #region TimePicker 属性

    [ObservableProperty]
    public partial TimeSpan? SelectedTime { get; set; } = new TimeSpan(9, 30, 0);

    [ObservableProperty]
    public partial TimeSpan? SelectedTime2 { get; set; } = new TimeSpan(14, 15, 30);

    [ObservableProperty]
    public partial TimeSpan? SelectedTime3 { get; set; } = new TimeSpan(18, 0, 0);

    [ObservableProperty]
    public partial string TimeInfo { get; set; } = string.Empty;

    #endregion TimePicker 属性

    #region Calendar 属性

    [ObservableProperty]
    public partial DateTime? SelectedDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial ObservableCollection<DateTime> CalendarBlockoutDates { get; set; }

    #endregion Calendar 属性

    private void DateViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(StartDate2):
            case nameof(EndDate2):
                UpdateDateRangeInfo();
                break;

            case nameof(SelectedTime3):
                UpdateTimeInfo();
                break;
        }
    }

    private void UpdateDateRangeInfo()
    {
        var span = EndDate2 - StartDate2;
        DateRangeInfo = $"共 {span.Days + 1} 天";
    }

    private void UpdateTimeInfo()
    {
        if (SelectedTime3.HasValue)
        {
            var time = SelectedTime3.Value;
            var totalMinutes = (int)time.TotalMinutes;
            TimeInfo = $"总计: {totalMinutes} 分钟";
        }
        else
        {
            TimeInfo = "未选择时间";
        }
    }

    public DateViewModel()
    {
        // 初始化禁用日期
        BlockoutDates = new ObservableCollection<DateTime>
        {
            DateTime.Today.AddDays(1),
            DateTime.Today.AddDays(3),
            DateTime.Today.AddDays(5)
        };

        CalendarBlockoutDates = new ObservableCollection<DateTime>
        {
            DateTime.Today.AddDays(2),
            DateTime.Today.AddDays(4),
            DateTime.Today.AddDays(6)
        };

        // 监听属性变化
        PropertyChanged += DateViewModel_PropertyChanged;

        UpdateDateRangeInfo();
        UpdateTimeInfo();
    }

    #region Commands

    [RelayCommand]
    private void ShowDateRange()
    {
        var message = $"选择的日期范围:\n开始: {StartDate:yyyy-MM-dd}\n结束: {EndDate:yyyy-MM-dd}\n天数: {(EndDate - StartDate).Days + 1}";
        MessageBox.Show(message, "日期范围", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void SetCurrentTime()
    {
        SelectedTime2 = DateTime.Now.TimeOfDay;
        MessageBox.Show($"已设置为当前时间: {DateTime.Now:HH:mm:ss}", "时间设置", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void AddBlockoutDates()
    {
        // 添加一些随机的禁用日期
        var random = new Random();
        for (int i = 0; i < 3; i++)
        {
            var randomDays = random.Next(7, 30);
            var newDate = DateTime.Today.AddDays(randomDays);

            if (!CalendarBlockoutDates.Contains(newDate))
            {
                CalendarBlockoutDates.Add(newDate);
            }
        }
    }

    [RelayCommand]
    private void ClearBlockoutDates()
    {
        CalendarBlockoutDates.Clear();
        MessageBox.Show("已清除所有禁用日期", "操作完成", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void SetToday()
    {
        SelectedDate = DateTime.Today;
        MessageBox.Show($"已选择今天: {DateTime.Today:yyyy年MM月dd日}", "日期设置", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion Commands
}