using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class RadioButtonSample : UserControl
{
    public RadioButtonSample()
    {
        InitializeComponent();
        DataContext = new RadioButtonViewModel();
    }
}

public enum AppTheme { Light, Dark, System }

public enum FontSizeLevel { Small, Medium, Large, ExtraLarge }

public enum NotificationMode { All, Mentions, None }

public partial class RadioButtonViewModel : ObservableObject
{
    [ObservableProperty]
    public partial FontSizeLevel FontSize { get; set; } = FontSizeLevel.Medium;

    [ObservableProperty]
    public partial NotificationMode Notifications { get; set; } = NotificationMode.Mentions;

    // 普通字符串绑定(用 Tag + IsChecked converter 模式)
    [ObservableProperty]
    public partial string PlanType { get; set; } = "Pro";

    [ObservableProperty]
    public partial AppTheme Theme { get; set; } = AppTheme.System;
}