using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class GroupBoxSample : UserControl
{
    public GroupBoxSample()
    {
        InitializeComponent();
        DataContext = new GroupBoxViewModel();
    }
}

public partial class GroupBoxViewModel : ObservableObject
{
    // 个人信息
    [ObservableProperty]
    public partial string FullName { get; set; } = "Alice Chen";

    [ObservableProperty]
    public partial string Email { get; set; } = "alice@example.com";

    [ObservableProperty]
    public partial string Phone { get; set; } = "138-0000-0000";

    // 偏好设置
    [ObservableProperty]
    public partial bool EnableNotifications { get; set; } = true;

    [ObservableProperty]
    public partial bool EnableNewsletters { get; set; } = false;

    [ObservableProperty]
    public partial bool DarkMode { get; set; } = true;

    // 网络配置
    [ObservableProperty]
    public partial string ServerAddress { get; set; } = "https://api.example.com";

    [ObservableProperty]
    public partial int Port { get; set; } = 443;

    [ObservableProperty]
    public partial bool UseProxy { get; set; } = false;
}
