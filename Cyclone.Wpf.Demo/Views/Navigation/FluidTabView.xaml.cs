using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Controls;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views;

public partial class FluidTabView : UserControl
{
    public FluidTabView()
    {
        InitializeComponent();
        DataContext = new FluidTabViewModel();
    }

    private void OnPlacementChecked(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is FluidTabViewModel vm
            && sender is System.Windows.Controls.Primitives.ToggleButton { Tag: FluidTabPlacement placement })
        {
            vm.CurrentPlacement = placement;
        }
    }

    private void OnSnapChecked(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is FluidTabViewModel vm
            && sender is System.Windows.Controls.Primitives.ToggleButton { Tag: FluidTabSnapAlignment snap })
        {
            vm.CurrentSnap = snap;
        }
    }
}

public partial class FluidTabViewModel : ObservableObject
{
    public ObservableCollection<TabSection> Sections { get; } = new()
    {
        new TabSection
        {
            Title = "通用",
            Subtitle = "应用启动、语言、自动更新等基础选项",
            IconText = "●",
            Body = "调整应用的常用设置。包括启动行为、默认语言、自动更新策略与备份频率。这些选项会立即生效，部分需要重启应用。每次修改都会写入本地配置并同步到云端。",
        },
        new TabSection
        {
            Title = "外观",
            Subtitle = "主题、字体、配色与界面密度",
            IconText = "◐",
            Body = "自定义主题（浅色 / 深色 / 跟随系统），调整字体与字号、配色方案和界面密度。视觉变化会立即应用到所有窗口。一些主题需要重启才能完全生效。",
        },
        new TabSection
        {
            Title = "账户",
            Subtitle = "登录、关联第三方账号、订阅",
            IconText = "◆",
            Body = "管理当前登录账户、关联第三方平台（GitHub / 微信 / 钉钉 / Microsoft）、查看与续订订阅。同一账户在多设备登录时会自动同步偏好设置。",
        },
        new TabSection
        {
            Title = "通知",
            Subtitle = "桌面通知、声音、勿扰时段",
            IconText = "★",
            Body = "控制哪些事件会弹出桌面通知，是否伴随声音；设置勿扰时段（如夜间或会议时间），系统在该时段内仅累计而不弹出。",
        },
        new TabSection
        {
            Title = "隐私",
            Subtitle = "数据收集、诊断报告、可见性",
            IconText = "○",
            Body = "决定是否参与匿名使用统计与崩溃诊断报告，控制活动状态与最近文件对其他用户的可见性。所有遥测在本地脱敏后再发送，并可随时关闭。",
        },
        new TabSection
        {
            Title = "高级",
            Subtitle = "代理、缓存、实验性功能",
            IconText = "▲",
            Body = "调试与高级用户选项：HTTP 代理、缓存策略、实验性功能开关、日志级别。这些选项可能影响稳定性，仅建议在了解影响时修改。",
        },
        new TabSection
        {
            Title = "快捷键",
            Subtitle = "全局与应用内快捷键映射",
            IconText = "▼",
            Body = "查看与自定义全局快捷键、应用内命令映射。冲突的快捷键会以红色标注；可一键恢复出厂映射。",
        },
        new TabSection
        {
            Title = "关于",
            Subtitle = "版本、许可证、致谢",
            IconText = "■",
            Body = "查看应用版本号、构建信息、开源许可证、第三方组件致谢。问题反馈链接附在底部。",
        },
    };

    [ObservableProperty]
    public partial TabSection SelectedSection { get; set; }

    [ObservableProperty]
    public partial int SelectedIndex { get; set; }

    [ObservableProperty]
    public partial FluidTabPlacement CurrentPlacement { get; set; } = FluidTabPlacement.Left;

    [ObservableProperty]
    public partial FluidTabSnapAlignment CurrentSnap { get; set; } = FluidTabSnapAlignment.Top;

    public FluidTabViewModel()
    {
        SelectedSection = Sections[0];
    }
}

/// <summary>
/// 一个示例分类项：MVVM demo 的数据模型。
/// </summary>
public class TabSection
{
    public string Title { get; init; }

    public string Subtitle { get; init; }

    public string IconText { get; init; }

    public string Body { get; init; }
}
