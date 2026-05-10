using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
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
/// PanelView.xaml 的交互逻辑
/// </summary>
public partial class CyclicPanelSample : UserControl
{
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        NotificationService.Instance.Information(string.Join(',', cycle.VisibleItemIndices));
    }

    public CyclicPanelSample()
    {
        InitializeComponent();
        DataContext = new CyclicPanelViewModel();
    }
}

public partial class CyclicPanelViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<FakerData> Data { get; set; } = [];

    public CyclicPanelViewModel()
    {
        Data = new ObservableCollection<FakerData>(FakerDataHelper.GenerateFakerDataCollection(15));
    }
}