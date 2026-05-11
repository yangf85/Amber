using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class BreadcrumbBarSample : UserControl
{
    public BreadcrumbBarSample()
    {
        InitializeComponent();
        DataContext = new BreadcrumbBarViewModel();
    }

    // ItemClicked 路由事件处理 — 演示如何响应点击
    private void OnBreadcrumbClicked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is BreadcrumbBarItem item && DataContext is BreadcrumbBarViewModel vm)
        {
            vm.LastClicked = item.Content?.ToString() ?? "(null)";
        }
    }
}

public partial class BreadcrumbBarViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string LastClicked { get; set; } = "(尚未点击)";

    // 动态路径演示 — 模拟文件浏览器面包屑,点击节点后截断到该节点
    public ObservableCollection<string> CurrentPath { get; } =
    [
        "C:",
        "Users",
        "alice",
        "Documents",
        "Projects",
        "Cyclone.Wpf",
        "Themes",
    ];

    [RelayCommand]
    private void NavigateTo(string segment)
    {
        if (string.IsNullOrEmpty(segment))
        {
            return;
        }

        // 找到 segment 位置,截断后面所有节点
        var index = CurrentPath.IndexOf(segment);
        if (index < 0)
        {
            return;
        }

        // 从后往前删——避免修改集合时索引漂移
        for (var i = CurrentPath.Count - 1; i > index; i--)
        {
            CurrentPath.RemoveAt(i);
        }

        LastClicked = segment;
    }

    [RelayCommand]
    private void ResetPath()
    {
        CurrentPath.Clear();
        CurrentPath.Add("C:");
        CurrentPath.Add("Users");
        CurrentPath.Add("alice");
        CurrentPath.Add("Documents");
        CurrentPath.Add("Projects");
        CurrentPath.Add("Cyclone.Wpf");
        CurrentPath.Add("Themes");
        LastClicked = "(已重置)";
    }

    [RelayCommand]
    private void GoDeeper()
    {
        // 模拟进入子目录
        var depth = CurrentPath.Count;
        CurrentPath.Add($"SubFolder{depth - 6}");
    }
}
