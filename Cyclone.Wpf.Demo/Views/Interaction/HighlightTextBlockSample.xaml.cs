using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views;

public partial class HighlightTextBlockSample : UserControl
{
    public HighlightTextBlockSample()
    {
        InitializeComponent();
        DataContext = new HighlightTextBlockViewModel();
    }
}

public partial class HighlightTextBlockViewModel : ObservableObject
{
    private const string IntroText =
        "Cyclone.Wpf 是一个现代化的 WPF UI 控件库，提供了 ComboBox、Dialog、Notification 等丰富的控件。HighlightTextBlock 控件可以高亮显示文本中的关键词，支持多 token 查询和引号 phrase 语法。";

    public string Card1Source => IntroText;

    public string Card2Source => IntroText;

    public string Card3Source =>
        "WPF wpf Wpf wpfui WPFAPP wpfdemo Microsoft .NET Foundation —— 大小写混合的搜索测试文本。";

    public string Card4Source =>
        "WPF 是一个现代化的 UI 框架，支持丰富的样式、动画和数据绑定。";

    [ObservableProperty]
    public partial string Card1Query { get; set; } = "WPF 控件";

    [ObservableProperty]
    public partial string Card2Query { get; set; } = "\"WPF 控件\"";

    public ObservableCollection<string> Card2Tokens { get; } = new();

    [ObservableProperty]
    public partial string Card3Query { get; set; } = "wpf";

    [ObservableProperty]
    public partial StringComparison Card3Comparison { get; set; } = StringComparison.CurrentCultureIgnoreCase;

    public string Card4Query => "WPF UI 动画";

    [ObservableProperty]
    public partial Brush Card4HighlightBg { get; set; }

    [ObservableProperty]
    public partial Brush Card4HighlightFg { get; set; }

    [ObservableProperty]
    public partial string Card5Query { get; set; } = "Service";

    public ObservableCollection<string> FileList { get; } = new();

    public HighlightTextBlockViewModel()
    {
        UpdateCard2Tokens(Card2Query);
        ApplyDefaultThemeCommand.Execute(null);
        InitFileList();
    }

    #region Card 2 - 解析展示

    partial void OnCard2QueryChanged(string value)
    {
        UpdateCard2Tokens(value);
    }

    private void UpdateCard2Tokens(string query)
    {
        Card2Tokens.Clear();
        foreach (var token in HighlightTextBlock.ParseQueries(query))
        {
            Card2Tokens.Add(token);
        }
    }

    #endregion Card 2 - 解析展示

    #region Card 3 - StringComparison RadioButton 联动

    public bool IsCmpOrdinalIgnoreCase
    {
        get => Card3Comparison == StringComparison.OrdinalIgnoreCase;
        set { if (value) { Card3Comparison = StringComparison.OrdinalIgnoreCase; } }
    }

    public bool IsCmpOrdinal
    {
        get => Card3Comparison == StringComparison.Ordinal;
        set { if (value) { Card3Comparison = StringComparison.Ordinal; } }
    }

    public bool IsCmpCurrentCultureIgnoreCase
    {
        get => Card3Comparison == StringComparison.CurrentCultureIgnoreCase;
        set { if (value) { Card3Comparison = StringComparison.CurrentCultureIgnoreCase; } }
    }

    public bool IsCmpInvariantCulture
    {
        get => Card3Comparison == StringComparison.InvariantCulture;
        set { if (value) { Card3Comparison = StringComparison.InvariantCulture; } }
    }

    partial void OnCard3ComparisonChanged(StringComparison value)
    {
        OnPropertyChanged(nameof(IsCmpOrdinalIgnoreCase));
        OnPropertyChanged(nameof(IsCmpOrdinal));
        OnPropertyChanged(nameof(IsCmpCurrentCultureIgnoreCase));
        OnPropertyChanged(nameof(IsCmpInvariantCulture));
    }

    #endregion Card 3 - StringComparison RadioButton 联动

    #region Card 4 - 颜色主题切换

    private static Brush MakeFrozenBrush(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    [RelayCommand]
    private void ApplyDefaultTheme()
    {
        // Material Yellow 500 + Grey 900（控件内置默认）
        Card4HighlightBg = MakeFrozenBrush(0xFF, 0xEB, 0x3B);
        Card4HighlightFg = MakeFrozenBrush(0x21, 0x21, 0x21);
    }

    [RelayCommand]
    private void ApplyWordTheme()
    {
        // 蓝底白字（类似 IDE 编辑器选区）
        Card4HighlightBg = MakeFrozenBrush(0x42, 0xA5, 0xF5);
        Card4HighlightFg = Brushes.White;
    }

    [RelayCommand]
    private void ApplyErrorTheme()
    {
        // 红底白字（标错）
        Card4HighlightBg = MakeFrozenBrush(0xE5, 0x39, 0x35);
        Card4HighlightFg = Brushes.White;
    }

    [RelayCommand]
    private void ApplyWarningTheme()
    {
        // 橙底深字（警示）
        Card4HighlightBg = MakeFrozenBrush(0xFF, 0xA7, 0x26);
        Card4HighlightFg = MakeFrozenBrush(0x21, 0x21, 0x21);
    }

    [RelayCommand]
    private void ApplyOnlyBgTheme()
    {
        // 仅背景：HighlightForeground = null → 文字色继承外层 TextBlock
        Card4HighlightBg = MakeFrozenBrush(0xFF, 0xEB, 0x3B);
        Card4HighlightFg = null;
    }

    [RelayCommand]
    private void ApplyOnlyFgTheme()
    {
        // 仅文字：HighlightBackground = null → 不绘制背景
        Card4HighlightBg = null;
        Card4HighlightFg = MakeFrozenBrush(0xE5, 0x39, 0x35);   // 红字
    }

    #endregion Card 4 - 颜色主题切换

    #region Card 5 - 文件列表

    private void InitFileList()
    {
        var files = new[]
        {
            "AlertService.cs",
            "AlertWindow.cs",
            "AlertMessage.cs",
            "AlertWindowPositioner.cs",
            "NotificationService.cs",
            "NotificationHandle.cs",
            "NotificationWindow.cs",
            "NotificationServiceExtension.cs",
            "HintBox.cs",
            "HintBoxItem.cs",
            "HighlightTextBlock.cs",
            "CascadePicker.cs",
            "CascadePickerItem.cs",
            "FluidTabControl.cs",
            "FluidTabItem.cs",
            "Drawer.cs",
            "WindowsNativeService.cs",
        };
        foreach (var f in files)
        {
            FileList.Add(f);
        }
    }

    #endregion Card 5 - 文件列表
}