using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class MenuSample : UserControl
{
    public MenuSample()
    {
        InitializeComponent();
        DataContext = new MenuViewModel();
    }
}

public partial class MenuViewModel : ObservableObject
{
    // 视图菜单的勾选项(IsCheckable 演示)
    [ObservableProperty]
    public partial bool ShowToolbar { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowStatusBar { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowSidebar { get; set; } = false;

    [ObservableProperty]
    public partial bool WordWrap { get; set; } = true;

    // 操作日志反馈
    [ObservableProperty]
    public partial string LastAction { get; set; } = "(尚无操作)";

    // 动态菜单数据 — 最近文件列表
    public ObservableCollection<string> RecentFiles { get; } =
    [
        "report-2026-Q1.docx",
        "budget-2026.xlsx",
        "presentation.pptx",
        "notes.txt",
        "design.md",
    ];

    [RelayCommand]
    private void Execute(string action)
    {
        LastAction = $"执行: {action}";
    }
}
