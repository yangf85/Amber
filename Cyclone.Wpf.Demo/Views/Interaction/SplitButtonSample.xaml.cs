using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class SplitButtonSample : UserControl
{
    /// <summary>演示 Click 路由事件——主按钮被点击时由 SplitButton 自身冒泡。</summary>
    private void OnPlainClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is SplitButtonViewModel vm)
        {
            vm.LastAction = "Click 路由事件触发(主按钮)";
        }
    }

    /// <summary>演示 ItemClick 路由事件——OriginalSource 是被点击的 SplitButtonItem。</summary>
    private void OnPlainItemClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is SplitButtonViewModel vm && e.OriginalSource is SplitButtonItem item)
        {
            vm.LastAction = $"ItemClick 路由事件触发(item.Content = \"{item.Content}\")";
        }
    }

    public SplitButtonSample()
    {
        InitializeComponent();
        DataContext = new SplitButtonViewModel();
    }
}

#region 主 ViewModel

public partial class SplitButtonViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string LastAction { get; set; } = "(尚未执行任何命令)";

    [ObservableProperty]
    public partial bool IsMainEnabled { get; set; } = true;

    /// <summary>用于演示 ItemsSource 数据驱动场景——见第 ③ 张卡片。</summary>
    public ExportPanelViewModel ExportPanel { get; } = new();

    // ===== 主按钮 Command =====

    [RelayCommand]
    private void Save() => LastAction = "执行: 保存(主按钮 Command)";

    // ===== 各菜单项 Command =====

    [RelayCommand]
    private void SaveAsPdf() => LastAction = "执行: 另存为 PDF";

    [RelayCommand]
    private void SaveAsWord() => LastAction = "执行: 另存为 Word";

    [RelayCommand]
    private void SaveAsHtml() => LastAction = "执行: 另存为 HTML";

    [RelayCommand]
    private void SaveAsMarkdown() => LastAction = "执行: 另存为 Markdown";

    [RelayCommand]
    private void NewFile() => LastAction = "执行: 新建文件";

    [RelayCommand]
    private void NewFolder() => LastAction = "执行: 新建文件夹";

    [RelayCommand]
    private void NewProject() => LastAction = "执行: 新建项目";

    [RelayCommand]
    private void ExportPng() => LastAction = "执行: 导出 PNG";

    [RelayCommand]
    private void ExportSvg() => LastAction = "执行: 导出 SVG";

    [RelayCommand]
    private void ExportJson() => LastAction = "执行: 导出 JSON";

    [RelayCommand]
    private void Rename() => LastAction = "执行: 重命名";

    [RelayCommand]
    private void Duplicate() => LastAction = "执行: 复制副本";

    [RelayCommand]
    private void Delete() => LastAction = "执行: 删除";
}

#endregion 主 ViewModel

#region ItemsSource 数据驱动:子项 VM + 主 VM

/// <summary>子项 VM——每个实例都有自己的 ExecuteCommand,但属性名相同。</summary>
public partial class ExportFormatViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string DisplayName { get; set; }

    [ObservableProperty]
    public partial string Extension { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExecuteCommand))]
    public partial bool IsAvailable { get; set; } = true;

    /// <summary>所有子项 VM 都叫 ExecuteCommand,但执行时拿到的是各自的 this。</summary>
    [RelayCommand(CanExecute = nameof(CanExecute))]
    private void Execute()
    {
        // 这里能拿到 this 自身的 DisplayName / Extension / 任何状态
        Executed?.Invoke(this);
    }

    private bool CanExecute() => IsAvailable;

    /// <summary>由父 VM 订阅,把每项的执行汇总到主 VM 的 LastResult 上。</summary>
    public event Action<ExportFormatViewModel> Executed;

    public ExportFormatViewModel(string displayName, string extension)
    {
        DisplayName = displayName;
        Extension = extension;
    }
}

/// <summary>持有子项 VM 列表的主 VM。</summary>
public partial class ExportPanelViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string LastResult { get; set; } = "(等待操作)";

    public ObservableCollection<ExportFormatViewModel> Formats { get; }

    /// <summary>切换"HTML 网页"项的可用性,演示 CanExecute 实时刷新 disabled 视觉。</summary>
    [RelayCommand]
    private void ToggleHtmlAvailability()
    {
        if (Formats.Count > 2)
        {
            Formats[2].IsAvailable = !Formats[2].IsAvailable;
        }
    }

    public ExportPanelViewModel()
    {
        Formats = new ObservableCollection<ExportFormatViewModel>
        {
            new("PDF 文档",   ".pdf"),
            new("Word 文档",  ".docx"),
            new("HTML 网页",  ".html"),
            new("Markdown",  ".md"),
            new("纯文本",     ".txt"),
        };

        // 把第三项设为不可用,演示 CanExecute 各自独立
        Formats[2].IsAvailable = false;

        foreach (var f in Formats)
        {
            f.Executed += vm => LastResult = $"已导出: {vm.DisplayName} ({vm.Extension})";
        }
    }
}

#endregion ItemsSource 数据驱动:子项 VM + 主 VM