using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.Helper;
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
/// Form.xaml 的交互逻辑
/// </summary>
public partial class FormSample : UserControl
{
    public FormSample()
    {
        InitializeComponent();
        DataContext = new FormViewModel();
    }
}

public partial class FormViewModel : ObservableObject
{
    [ObservableProperty]
    public partial FakerData Data { get; set; }

    [RelayCommand]
    private void Show()
    {
        NotificationService.Instance.Information(Data.FirstName);
    }

    public FormViewModel()
    {
        Data = FakerDataHelper.GenerateFakerDataCollection(1).FirstOrDefault()!;
        Data.Status = UserStatus.Inactive;
    }
}