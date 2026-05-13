using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class TimePickerSample : UserControl
{
    public TimePickerSample()
    {
        InitializeComponent();
        DataContext = new TimePickerViewModel();
    }
}

public partial class TimePickerViewModel : ObservableObject
{
    // ① 会议时间
    [ObservableProperty]
    public partial TimeSpan? MeetingStart { get; set; } = new TimeSpan(9, 30, 0);

    [ObservableProperty]
    public partial TimeSpan? MeetingEnd { get; set; } = new TimeSpan(10, 30, 0);

    public string MeetingDuration => (MeetingStart.HasValue && MeetingEnd.HasValue)
        ? $"{(MeetingEnd.Value - MeetingStart.Value).TotalMinutes:F0} 分钟"
        : "未设置";

    partial void OnMeetingStartChanged(TimeSpan? value) => OnPropertyChanged(nameof(MeetingDuration));

    partial void OnMeetingEndChanged(TimeSpan? value) => OnPropertyChanged(nameof(MeetingDuration));

    // ② null 默认 (测 Watermark)
    [ObservableProperty]
    public partial TimeSpan? AlarmTime { get; set; }

    // ③ 不同格式
    [ObservableProperty]
    public partial TimeSpan? DepartureTime { get; set; } = new TimeSpan(14, 25, 0);

    // ④ 禁用态
    [ObservableProperty]
    public partial TimeSpan? FrozenTime { get; set; } = new TimeSpan(8, 0, 0);

    // 操作
    [RelayCommand]
    private void SetNow()
    {
        AlarmTime = DateTime.Now.TimeOfDay;
    }

    [RelayCommand]
    private void ClearAlarm()
    {
        AlarmTime = null;
    }
}
