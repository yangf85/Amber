using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views;

public partial class SliderSample : UserControl
{
    public SliderSample()
    {
        InitializeComponent();
        DataContext = new SliderViewModel();
    }
}

public partial class SliderViewModel : ObservableObject
{
    // 基础数值
    [ObservableProperty]
    public partial double Volume { get; set; } = 65;

    [ObservableProperty]
    public partial double Brightness { get; set; } = 80;

    // 离散步长 — 评分
    [ObservableProperty]
    public partial double Rating { get; set; } = 7;

    // 价格 — 大范围 + 货币显示
    [ObservableProperty]
    public partial double Price { get; set; } = 2500;

    public string PriceText => $"¥{Price:N0}";

    partial void OnPriceChanged(double value) => OnPropertyChanged(nameof(PriceText));

    // RGB 颜色调节
    [ObservableProperty]
    public partial double Red { get; set; } = 64;

    [ObservableProperty]
    public partial double Green { get; set; } = 158;

    [ObservableProperty]
    public partial double Blue { get; set; } = 255;

    public SolidColorBrush PreviewBrush =>
        new(Color.FromRgb((byte)Red, (byte)Green, (byte)Blue));

    public string ColorHex =>
        $"#{(byte)Red:X2}{(byte)Green:X2}{(byte)Blue:X2}";

    partial void OnRedChanged(double value)
    {
        OnPropertyChanged(nameof(PreviewBrush));
        OnPropertyChanged(nameof(ColorHex));
    }

    partial void OnGreenChanged(double value)
    {
        OnPropertyChanged(nameof(PreviewBrush));
        OnPropertyChanged(nameof(ColorHex));
    }

    partial void OnBlueChanged(double value)
    {
        OnPropertyChanged(nameof(PreviewBrush));
        OnPropertyChanged(nameof(ColorHex));
    }

    // 垂直音量
    [ObservableProperty]
    public partial double VerticalLevel1 { get; set; } = 30;

    [ObservableProperty]
    public partial double VerticalLevel2 { get; set; } = 75;

    [ObservableProperty]
    public partial double VerticalLevel3 { get; set; } = 50;
}
