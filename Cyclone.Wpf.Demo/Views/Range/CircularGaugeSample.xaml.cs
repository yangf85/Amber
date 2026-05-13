using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Cyclone.Wpf.Demo.Views;

public partial class CircularGaugeSample : UserControl
{
    public CircularGaugeSample()
    {
        InitializeComponent();
        DataContext = new CircularGaugeViewModel();
    }
}

public partial class CircularGaugeViewModel : ObservableObject
{
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();

    // 基础值
    [ObservableProperty]
    public partial double Value1 { get; set; } = 65;

    // 模拟 CPU 监控
    [ObservableProperty]
    public partial double CpuUsage { get; set; } = 35;

    // 温度计
    [ObservableProperty]
    public partial double Temperature { get; set; } = 22;

    // 速度表
    [ObservableProperty]
    public partial double Speed { get; set; } = 60;

    // 电量百分比
    [ObservableProperty]
    public partial double BatteryLevel { get; set; } = 78;

    // 是否启用模拟数据更新
    [ObservableProperty]
    public partial bool IsSimulating { get; set; } = true;

    // 联动 SweepAngle 演示
    [ObservableProperty]
    public partial double DemoSweepAngle { get; set; } = 270;

    [ObservableProperty]
    public partial double DemoValue { get; set; } = 50;

    public CircularGaugeViewModel()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(800)
        };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    partial void OnIsSimulatingChanged(bool value)
    {
        if (value) _timer.Start(); else _timer.Stop();
    }

    private void OnTimerTick(object sender, EventArgs e)
    {
        // CPU 用平滑随机游走
        CpuUsage = Clamp(CpuUsage + (_random.NextDouble() - 0.5) * 30, 5, 95);

        // 温度小幅波动
        Temperature = Clamp(Temperature + (_random.NextDouble() - 0.5) * 4, -10, 50);

        // 速度跳变
        Speed = Clamp(Speed + (_random.NextDouble() - 0.5) * 40, 0, 200);

        // 电量缓慢下降
        BatteryLevel = Clamp(BatteryLevel - _random.NextDouble() * 0.5, 0, 100);
        if (BatteryLevel <= 0) BatteryLevel = 100;
    }

    private static double Clamp(double v, double min, double max) =>
        v < min ? min : v > max ? max : v;

    [RelayCommand]
    private void ResetAll()
    {
        CpuUsage = 35;
        Temperature = 22;
        Speed = 60;
        BatteryLevel = 100;
    }
}
