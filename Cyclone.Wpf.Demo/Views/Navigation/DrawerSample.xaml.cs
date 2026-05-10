using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Controls;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class DrawerSample : UserControl
{
    // 切换方向期间忽略额外点击，避免连击产生残留 Closed handler
    private bool _placementSwitching;

    private void OnPlacementButtonClick(object sender, RoutedEventArgs e)
    {
        if (_placementSwitching)
        {
            return;
        }

        if (sender is not Button { Tag: DrawerPlacement placement }
            || DataContext is not DrawerViewModel vm)
        {
            return;
        }

        // 同方向：抽屉关着就开、开着就关，纯 toggle
        if (vm.CurrentPlacement == placement)
        {
            vm.IsPlacementDrawerOpen = !vm.IsPlacementDrawerOpen;
            return;
        }

        // 异方向：抽屉关着 → 切方向 + 打开（一个动画）
        if (!vm.IsPlacementDrawerOpen)
        {
            vm.CurrentPlacement = placement;
            vm.IsPlacementDrawerOpen = true;
            return;
        }

        // 异方向：抽屉开着 → 先关（动画），关闭完成后切方向再开（第二个动画）
        _placementSwitching = true;
        RoutedEventHandler closedHandler = null;
        closedHandler = (_, _) =>
        {
            PlacementDrawer.Closed -= closedHandler;
            vm.CurrentPlacement = placement;
            vm.IsPlacementDrawerOpen = true;
            _placementSwitching = false;
        };
        PlacementDrawer.Closed += closedHandler;
        vm.IsPlacementDrawerOpen = false;
    }

    private void OnDrawerOpening(object sender, RoutedEventArgs e)
    {
        if (DataContext is DrawerViewModel vm)
        {
            vm.OpeningCount++;
        }
    }

    private void OnDrawerOpened(object sender, RoutedEventArgs e)
    {
        if (DataContext is DrawerViewModel vm)
        {
            vm.OpenedCount++;
        }
    }

    private void OnDrawerClosing(object sender, RoutedEventArgs e)
    {
        if (DataContext is DrawerViewModel vm)
        {
            vm.ClosingCount++;
        }
    }

    private void OnDrawerClosed(object sender, RoutedEventArgs e)
    {
        if (DataContext is DrawerViewModel vm)
        {
            vm.ClosedCount++;
        }
    }

    public DrawerSample()
    {
        InitializeComponent();
        DataContext = new DrawerViewModel();
    }
}

public partial class DrawerViewModel : ObservableObject
{
    // ① 基础用法
    [ObservableProperty]
    public partial bool IsBasicDrawerOpen { get; set; }

    // ② 四方向
    [ObservableProperty]
    public partial DrawerPlacement CurrentPlacement { get; set; } = DrawerPlacement.Left;

    [ObservableProperty]
    public partial bool IsPlacementDrawerOpen { get; set; }

    // ③ 路由命令（不需要 VM 字段，纯 XAML 触发）
    [ObservableProperty]
    public partial bool IsCommandDrawerOpen { get; set; }

    // ④ MVVM + 事件
    [ObservableProperty]
    public partial bool IsEventDrawerOpen { get; set; }

    [ObservableProperty]
    public partial int OpeningCount { get; set; }

    [ObservableProperty]
    public partial int OpenedCount { get; set; }

    [ObservableProperty]
    public partial int ClosingCount { get; set; }

    [ObservableProperty]
    public partial int ClosedCount { get; set; }

    // ⑤ 配置开关
    [ObservableProperty]
    public partial bool CloseOnOverlayClickEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool CloseOnEscapeEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool FocusOnOpenEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool IsConfigDrawerOpen { get; set; }
}