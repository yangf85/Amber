using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views;

public partial class LoadingBoxSample : UserControl
{
    public LoadingBoxSample()
    {
        InitializeComponent();
        DataContext = new LoadingBoxViewModel();
    }
}

public class DataItem
{
    public string Title { get; set; }

    public string Subtitle { get; set; }

    public string Status { get; set; }
}

public class MaskPreset
{
    public string Name { get; set; }

    public Brush Brush { get; set; }
}

public partial class LoadingBoxViewModel : ObservableObject
{
    public LoadingBoxViewModel()
    {
        // 字段先初始化（partial property setter callback 会调 UpdateIndicator）
        SelectedMaskPreset = MaskPresets[0];
        UpdateIndicator(SelectedIndicatorType);
        InitDataItems();
    }

    #region Card 1 - 基础切换

    [ObservableProperty]
    public partial bool IsLoadingBasic { get; set; } = true;

    #endregion Card 1 - 基础切换

    #region Card 2 - 6 种 indicator 切换

    public ObservableCollection<string> IndicatorTypes { get; } = new()
    {
        "Ring",
        "Chase",
        "Pulse",
        "Particle",
        "FlipCube",
        "Tesseract",
    };

    [ObservableProperty]
    public partial string SelectedIndicatorType { get; set; } = "Ring";

    /// <summary>当前实例化的 indicator——切换 SelectedIndicatorType 时重新 new 一个。</summary>
    [ObservableProperty]
    public partial LoadingIndicator CurrentIndicator { get; set; }

    partial void OnSelectedIndicatorTypeChanged(string value) => UpdateIndicator(value);

    private void UpdateIndicator(string type)
    {
        var white = Brushes.White;

        CurrentIndicator = type switch
        {
            "Ring" => new LoadingRing { RingColor = white, RingSize = 56 },
            "Chase" => new LoadingChase { DotColor = white, CircleSize = 56 },
            "Pulse" => new LoadingPulse { DotColor = white },
            "Particle" => new LoadingParticle { ParticleColor = white },
            "FlipCube" => new LoadingFlipCube { CubeColor = Colors.White, CubeSize = 1.2 },
            "Tesseract" => new LoadingTesseract { LineColor = Colors.White, Scale = 0.4 },
            _ => new LoadingRing { RingColor = white },
        };
    }

    #endregion Card 2 - 6 种 indicator 切换

    #region Card 3 - LoadingRing 属性调节

    [ObservableProperty]
    public partial double Card3RingSize { get; set; } = 60;

    [ObservableProperty]
    public partial double Card3RingThickness { get; set; } = 5;

    [ObservableProperty]
    public partial double Card3RotationSpeed { get; set; } = 1.5;

    [ObservableProperty]
    public partial double Card3ArcLength { get; set; } = 270;

    #endregion Card 3 - LoadingRing 属性调节

    #region Card 4 - Mask 样式预设

    public ObservableCollection<MaskPreset> MaskPresets { get; } = new()
    {
        new MaskPreset
        {
            Name = "默认半透黑（80）",
            Brush = CreateFrozenBrush(Color.FromArgb(128, 0, 0, 0)),
        },
        new MaskPreset
        {
            Name = "重色（DC）",
            Brush = CreateFrozenBrush(Color.FromArgb(220, 0, 0, 0)),
        },
        new MaskPreset
        {
            Name = "半透白（CC）",
            Brush = CreateFrozenBrush(Color.FromArgb(204, 255, 255, 255)),
        },
        new MaskPreset
        {
            Name = "蓝调（B4）",
            Brush = CreateFrozenBrush(Color.FromArgb(180, 0x21, 0x96, 0xF3)),
        },
        new MaskPreset
        {
            Name = "高斯模糊感（紫调）",
            Brush = CreateFrozenBrush(Color.FromArgb(180, 0x67, 0x3A, 0xB7)),
        },
    };

    [ObservableProperty]
    public partial MaskPreset SelectedMaskPreset { get; set; }

    private static SolidColorBrush CreateFrozenBrush(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    #endregion Card 4 - Mask 样式预设

    #region Card 5 - 综合实战（异步加载）

    [ObservableProperty]
    public partial bool IsDataLoading { get; set; }

    public ObservableCollection<DataItem> DataItems { get; } = new();

    [ObservableProperty]
    public partial int RefreshCount { get; set; }

    partial void OnIsDataLoadingChanged(bool value)
    {
        RefreshDataCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshDataAsync()
    {
        IsDataLoading = true;
        try
        {
            // 模拟异步加载（网络请求 / 数据库查询等）
            await Task.Delay(2500);

            DataItems.Clear();
            InitDataItems();
            RefreshCount++;
        }
        finally
        {
            IsDataLoading = false;
        }
    }

    private bool CanRefresh() => !IsDataLoading;

    private void InitDataItems()
    {
        var data = new (string Title, string Subtitle, string Status)[]
        {
            ("月度销售报告", "本月营收同比增长 12.4%", "已完成"),
            ("用户行为分析", "DAU 趋势 + 留存数据", "处理中"),
            ("财务月结摘要", "Q4 损益表初稿", "待审核"),
            ("市场调研报告", "竞品功能对比 + 定价策略", "已完成"),
            ("运营数据看板", "实时监控关键指标", "已完成"),
        };

        foreach (var (title, subtitle, status) in data)
        {
            DataItems.Add(new DataItem { Title = title, Subtitle = subtitle, Status = status });
        }
    }

    #endregion Card 5 - 综合实战（异步加载）
}