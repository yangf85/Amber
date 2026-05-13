using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Cyclone.Wpf.Demo.Views;

public partial class ProgressBarSample : UserControl
{
    public ProgressBarSample()
    {
        InitializeComponent();
        DataContext = new ProgressBarViewModel();
    }
}

public partial class ProgressBarViewModel : ObservableObject
{
    private readonly DispatcherTimer _downloadTimer;

    // 基础可控进度
    [ObservableProperty]
    public partial double BasicProgress { get; set; } = 45;

    // 开始按钮 IsEnabled — 下载中禁用,避免重复点击
    public bool CanStartDownload => !IsDownloading;

    // 模拟下载
    [ObservableProperty]
    public partial double DownloadProgress { get; set; } = 0;

    [ObservableProperty]
    public partial string DownloadStatus { get; set; } = "就绪";

    [ObservableProperty]
    public partial bool IsDownloading { get; set; }

    // Indeterminate 演示
    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    // 多个不同进度的"任务列表"
    public double TaskProgress1 { get; } = 100;

    public double TaskProgress2 { get; } = 72;

    public double TaskProgress3 { get; } = 28;

    public double TaskProgress4 { get; } = 0;

    private void OnDownloadTick(object? sender, EventArgs e)
    {
        // 模拟略带随机性的下载,接近 100 时变慢
        var remaining = 100 - DownloadProgress;
        var step = Math.Max(0.3, remaining * 0.04);
        DownloadProgress = Math.Min(100, DownloadProgress + step);

        if (DownloadProgress >= 100)
        {
            _downloadTimer.Stop();
            IsDownloading = false;
            DownloadStatus = "下载完成 ✓";
        }
    }

    partial void OnIsDownloadingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanStartDownload));
    }

    [RelayCommand]
    private void ResetDownload()
    {
        _downloadTimer.Stop();
        DownloadProgress = 0;
        IsDownloading = false;
        DownloadStatus = "就绪";
    }

    [RelayCommand]
    private void StartDownload()
    {
        if (IsDownloading)
        {
            return;
        }
        DownloadProgress = 0;
        IsDownloading = true;
        DownloadStatus = "下载中...";
        _downloadTimer.Start();
    }

    [RelayCommand]
    private void ToggleLoading()
    {
        IsLoading = !IsLoading;
    }

    public ProgressBarViewModel()
    {
        _downloadTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80) };
        _downloadTimer.Tick += OnDownloadTick;
    }
}