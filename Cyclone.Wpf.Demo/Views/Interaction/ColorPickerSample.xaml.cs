using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// ColorPickerView.xaml 的交互逻辑
/// </summary>
public partial class ColorPickerSample : UserControl
{
    public ColorPickerSample()
    {
        InitializeComponent();
        DataContext = new ColorPickerViewModel();
    }
}

public partial class ColorPickerViewModel : ObservableObject
{
    [ObservableProperty]
    public partial double Alpha { get; set; }

    [ObservableProperty]
    public partial double Bright { get; set; }

    [ObservableProperty]
    public partial double Saturation { get; set; }

    [ObservableProperty]
    public partial double Hue { get; set; }

    [ObservableProperty]
    public partial Color Color { get; set; } = Colors.Red;

    [RelayCommand]
    private void ShowColorPaletteColor(Color? color)
    {
        var service = Cyclone.Wpf.Controls.AlertService.Instance;
        service.SetOwner(App.Current.MainWindow);
        service.Information($"{color}");
    }
}