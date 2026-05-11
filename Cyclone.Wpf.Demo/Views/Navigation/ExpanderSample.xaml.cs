using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class ExpanderSample : UserControl
{
    public ExpanderSample()
    {
        InitializeComponent();
        DataContext = new ExpanderViewModel();
    }
}

public partial class ExpanderViewModel : ObservableObject
{
    // MVVM 双向绑定演示——按钮可以反向触发展开/折叠
    [ObservableProperty]
    public partial bool IsAccountExpanded { get; set; } = true;

    public string AccountExpandedText => IsAccountExpanded ? "已展开" : "已折叠";

    partial void OnIsAccountExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(AccountExpandedText));
    }

    // 设置项面板演示
    [ObservableProperty]
    public partial bool IsNotificationsEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool IsEmailEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool IsSmsEnabled { get; set; } = false;

    [ObservableProperty]
    public partial string UserName { get; set; } = "Alice";

    [ObservableProperty]
    public partial string Email { get; set; } = "alice@example.com";
}
