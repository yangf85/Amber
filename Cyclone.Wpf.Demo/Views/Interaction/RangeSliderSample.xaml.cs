using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class RangeSliderSample : UserControl
{
    public RangeSliderSample()
    {
        InitializeComponent();
        DataContext = new RangeSliderViewModel();
    }
}

public partial class RangeSliderViewModel : ObservableObject
{
    [ObservableProperty]
    public partial double PriceLower { get; set; }

    [ObservableProperty]
    public partial double PriceUpper { get; set; }

    public string PriceRangeText => $"¥{PriceLower:F0} - ¥{PriceUpper:F0}";

    [ObservableProperty]
    public partial double AgeLower { get; set; }

    [ObservableProperty]
    public partial double AgeUpper { get; set; }

    public string AgeRangeText => $"{AgeLower:F0} - {AgeUpper:F0} 岁";

    [ObservableProperty]
    public partial double HourLower { get; set; }

    [ObservableProperty]
    public partial double HourUpper { get; set; }

    public string HourRangeText => $"{HourLower:F0}:00 - {HourUpper:F0}:00";

    [ObservableProperty]
    public partial double OpacityLower { get; set; }

    [ObservableProperty]
    public partial double OpacityUpper { get; set; }
}