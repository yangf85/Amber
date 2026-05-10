using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Demo.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// MasonryPanelView.xaml 的交互逻辑
/// </summary>
public partial class WaterfallPanelSample : UserControl
{
    public WaterfallPanelSample()
    {
        InitializeComponent();
        DataContext = new WaterfallPanelViewModel();
    }
}

public partial class WaterfallPanelViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<FakerData> Data { get; set; } = [];

    public WaterfallPanelViewModel()
    {
        Data = new ObservableCollection<FakerData>(FakerDataHelper.GenerateFakerDataCollection(56));
    }
}