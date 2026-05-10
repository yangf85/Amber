using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views;

public partial class SwitchButtonSample : UserControl
{
    public SwitchButtonSample()
    {
        InitializeComponent();
        DataContext = new SwitchButtonViewModel();
    }
}

public partial class SettingItem : ObservableObject
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string IconData { get; set; }

    [ObservableProperty]
    public partial bool IsOn { get; set; }
}

public partial class SwitchButtonViewModel : ObservableObject
{
    #region Card 1 - 基础切换

    /// <summary>"自动保存"开关本身的 on/off 状态。</summary>
    [ObservableProperty]
    public partial bool IsAutoSave { get; set; } = true;

    /// <summary>控制"自动保存"开关是否可用——演示 SwitchButton 控制其他控件 IsEnabled 的常见用法。</summary>
    [ObservableProperty]
    public partial bool IsAllowEdit { get; set; } = true;

    #endregion Card 1 - 基础切换

    #region Card 3 - 颜色主题

    [ObservableProperty]
    public partial Brush Card3CheckedBg { get; set; }

    [ObservableProperty]
    public partial Brush Card3UncheckedBg { get; set; }

    [ObservableProperty]
    public partial Brush Card3ThumbBg { get; set; }

    [ObservableProperty]
    public partial bool Card3IsChecked { get; set; } = true;

    #endregion Card 3 - 颜色主题

    #region Card 4 - 动画时长

    [ObservableProperty]
    public partial bool Card4Switch0 { get; set; }

    [ObservableProperty]
    public partial bool Card4Switch100 { get; set; }

    [ObservableProperty]
    public partial bool Card4Switch200 { get; set; }

    [ObservableProperty]
    public partial bool Card4Switch500 { get; set; }

    #endregion Card 4 - 动画时长

    #region Card 5 - 设置面板

    public ObservableCollection<SettingItem> Settings { get; } = new();

    #endregion Card 5 - 设置面板

    public SwitchButtonViewModel()
    {
        ApplyDefaultThemeCommand.Execute(null);
        InitSettings();
    }

    #region Card 3 - 主题切换 Commands

    private static Brush MakeFrozenBrush(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    [RelayCommand]
    private void ApplyDefaultTheme()
    {
        // 默认红色（控件原始默认值）
        Card3CheckedBg = MakeFrozenBrush(0xFF, 0x4B, 0x4B);
        Card3UncheckedBg = MakeFrozenBrush(0xCC, 0xCC, 0xCC);
        Card3ThumbBg = Brushes.White;
    }

    [RelayCommand]
    private void ApplyIosTheme()
    {
        // iOS 风格：开启时绿色，关闭时灰色
        Card3CheckedBg = MakeFrozenBrush(0x34, 0xC7, 0x59);
        Card3UncheckedBg = MakeFrozenBrush(0xE9, 0xE9, 0xEB);
        Card3ThumbBg = Brushes.White;
    }

    [RelayCommand]
    private void ApplyMaterialTheme()
    {
        // Material Design：蓝紫开启，浅灰关闭
        Card3CheckedBg = MakeFrozenBrush(0x67, 0x3A, 0xB7);
        Card3UncheckedBg = MakeFrozenBrush(0xBD, 0xBD, 0xBD);
        Card3ThumbBg = MakeFrozenBrush(0xF5, 0xF5, 0xF5);
    }

    [RelayCommand]
    private void ApplyDarkTheme()
    {
        // 暗色风格：青色开启，深灰关闭，深 thumb
        Card3CheckedBg = MakeFrozenBrush(0x00, 0xBC, 0xD4);
        Card3UncheckedBg = MakeFrozenBrush(0x42, 0x42, 0x42);
        Card3ThumbBg = MakeFrozenBrush(0xEE, 0xEE, 0xEE);
    }

    #endregion Card 3 - 主题切换 Commands

    #region Card 4 - 动画时长对比 Command

    /// <summary>同步切换 4 个不同动画时长的 SwitchButton——便于一眼看出时长差异。</summary>
    [RelayCommand]
    private void Card4ToggleAll()
    {
        var newValue = !Card4Switch200;
        Card4Switch0 = newValue;
        Card4Switch100 = newValue;
        Card4Switch200 = newValue;
        Card4Switch500 = newValue;
    }

    #endregion Card 4 - 动画时长对比 Command

    #region Card 5 - 设置项初始化

    private void InitSettings()
    {
        Settings.Add(new SettingItem
        {
            Name = "自动保存草稿",
            Description = "每 30 秒自动保存当前编辑内容",
            IconData = "M17 3H5c-1.11 0-2 .9-2 2v14c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V7l-4-4zm-5 16c-1.66 0-3-1.34-3-3s1.34-3 3-3 3 1.34 3 3-1.34 3-3 3zm3-10H5V5h10v4z",
            IsOn = true,
        });
        Settings.Add(new SettingItem
        {
            Name = "深色模式",
            Description = "降低光线刺激，节省 OLED 屏幕电量",
            IconData = "M9.37,5.51C9.19,6.15 9.1,6.82 9.1,7.5c0,4.08 3.32,7.4 7.4,7.4c0.68,0 1.35,-0.09 1.99,-0.27C17.45,17.19 14.93,19 12,19c-3.86,0 -7,-3.14 -7,-7C5,9.07 6.81,6.55 9.37,5.51zM12,3c-4.97,0 -9,4.03 -9,9s4.03,9 9,9s9,-4.03 9,-9c0,-0.46 -0.04,-0.92 -0.1,-1.36c-0.98,1.37 -2.58,2.26 -4.4,2.26c-2.98,0 -5.4,-2.42 -5.4,-5.4c0,-1.81 0.89,-3.42 2.26,-4.4C12.92,3.04 12.46,3 12,3L12,3z",
            IsOn = false,
        });
        Settings.Add(new SettingItem
        {
            Name = "推送通知",
            Description = "重要事件通过桌面通知提醒",
            IconData = "M12 22c1.1 0 2-.9 2-2h-4c0 1.1.89 2 2 2zm6-6v-5c0-3.07-1.64-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.63 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2z",
            IsOn = true,
        });
        Settings.Add(new SettingItem
        {
            Name = "自动检查更新",
            Description = "每天检查一次新版本",
            IconData = "M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z",
            IsOn = true,
        });
        Settings.Add(new SettingItem
        {
            Name = "操作音效",
            Description = "保存、删除等操作时播放提示音",
            IconData = "M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z",
            IsOn = false,
        });
        Settings.Add(new SettingItem
        {
            Name = "诊断数据上报",
            Description = "匿名发送崩溃报告帮助改进产品",
            IconData = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-7 4c1.66 0 3 1.34 3 3s-1.34 3-3 3-3-1.34-3-3 1.34-3 3-3zm6 12H6v-1c0-2 4-3.1 6-3.1s6 1.1 6 3.1v1z",
            IsOn = false,
        });
    }

    #endregion Card 5 - 设置项初始化
}