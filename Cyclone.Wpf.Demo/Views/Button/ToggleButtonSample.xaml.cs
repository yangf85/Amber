using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class ToggleButtonSample : UserControl
{
    public ToggleButtonSample()
    {
        InitializeComponent();
        DataContext = new ToggleButtonViewModel();
    }
}

public partial class ToggleButtonViewModel : ObservableObject
{
    // 富文本工具栏
    [ObservableProperty]
    public partial bool IsBold { get; set; }

    [ObservableProperty]
    public partial bool IsItalic { get; set; }

    [ObservableProperty]
    public partial bool IsUnderline { get; set; }

    // 通知开关
    [ObservableProperty]
    public partial bool NotificationsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool DarkMode { get; set; }

    [ObservableProperty]
    public partial bool AutoSave { get; set; } = true;

    // 预览文本
    public string PreviewText => "在此输入预览文本 — Bold/Italic/Underline 实时联动";
}
