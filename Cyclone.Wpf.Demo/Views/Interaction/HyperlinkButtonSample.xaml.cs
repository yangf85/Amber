using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class HyperlinkButtonSample : UserControl
{
    /// <summary>Card 2: 监听 Click 事件做自定义处理。</summary>
    private void OnLinkClicked(object sender, RoutedEventArgs e)
    {
        if (sender is HyperlinkButton btn && DataContext is HyperlinkButtonViewModel vm)
        {
            vm.AddClickLog($"Click 事件触发: {btn.DisplayText}");
        }
    }

    /// <summary>Card 3: 检查每个链接的 scheme 是否在白名单内，模拟控件内部行为给 user 可见反馈。</summary>
    private void OnSchemeLinkClicked(object sender, RoutedEventArgs e)
    {
        if (sender is HyperlinkButton btn && DataContext is HyperlinkButtonViewModel vm)
        {
            var uri = btn.NavigateUri;
            string[] allowed = { "http", "https", "mailto" };
            bool isAllowed = uri != null
                && uri.IsAbsoluteUri
                && Array.IndexOf(allowed, uri.Scheme.ToLowerInvariant()) >= 0;

            string msg = isAllowed
                ? $"✓ 已用浏览器/系统默认应用打开 [{uri.Scheme}]"
                : $"✗ Scheme '{uri?.Scheme}' 不在白名单 → 控件 trace warning，浏览器不会打开";
            vm.AddSchemeLog(msg);
        }
    }

    public HyperlinkButtonSample()
    {
        InitializeComponent();
        DataContext = new HyperlinkButtonViewModel();
    }
}

public partial class HyperlinkButtonViewModel : ObservableObject
{
    public ObservableCollection<string> ClickLog { get; } = new();

    public ObservableCollection<string> SchemeLog { get; } = new();

    [ObservableProperty]
    public partial int CommandClickCount { get; set; }

    /// <summary>Card 2 的 Command 演示——典型用途是跳转前 telemetry / 数据校验等。</summary>
    [RelayCommand]
    private void OnDocLink(string parameter)
    {
        CommandClickCount++;
        AddClickLog($"Command 触发: 参数 '{parameter}' (浏览器同时打开)");
    }

    public void AddClickLog(string msg)
    {
        ClickLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}]  {msg}");
        while (ClickLog.Count > 5)
        {
            ClickLog.RemoveAt(ClickLog.Count - 1);
        }
    }

    public void AddSchemeLog(string msg)
    {
        SchemeLog.Insert(0, $"[{DateTime.Now:HH:mm:ss}]  {msg}");
        while (SchemeLog.Count > 5)
        {
            SchemeLog.RemoveAt(SchemeLog.Count - 1);
        }
    }
}