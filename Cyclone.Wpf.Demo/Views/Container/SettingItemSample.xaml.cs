using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// SettingItem.xaml 的交互逻辑
/// </summary>
public partial class SettingItemSample : UserControl
{
    public SettingItemSample()
    {
        InitializeComponent();
        DataContext = new SettingItemViewModel();
    }
}

public partial class SettingItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool NotifyEnabled { get; set; }

    [ObservableProperty]
    public partial bool AutoStart { get; set; }

    [ObservableProperty]
    public partial string CurrentLanguage { get; set; }

    [ObservableProperty]
    public partial double FontSize { get; set; }

    [ObservableProperty]
    public partial string ProxyAddress { get; set; }

    [ObservableProperty]
    public partial double TimeoutSeconds { get; set; }

    [ObservableProperty]
    public partial string CurrentLogLevel { get; set; }

    public List<string> Languages { get; } = new() { "简体中文", "English", "日本語", "한국어" };

    public List<string> LogLevels { get; } = new() { "Trace", "Debug", "Info", "Warning", "Error" };

    [RelayCommand]
    private void OpenAccount()
    {
        NotificationService.Instance.Information("打开账户详情页");
    }

    [RelayCommand]
    private void ExportData()
    {
        NotificationService.Instance.Information("已导出数据");
    }

    public SettingItemViewModel()
    {
        NotifyEnabled = true;
        AutoStart = false;
        CurrentLanguage = "简体中文";
        FontSize = 14;
        ProxyAddress = "127.0.0.1:7890";
        TimeoutSeconds = 30;
        CurrentLogLevel = "Info";
    }
}