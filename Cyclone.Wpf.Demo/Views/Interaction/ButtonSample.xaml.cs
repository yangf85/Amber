using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.Helper;
using Cyclone.Wpf.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
/// ButtonView.xaml 的交互逻辑
/// </summary>
public partial class ButtonSample : UserControl
{
    public ButtonSample()
    {
        InitializeComponent();
        DataContext = new ButtonViewModel();
    }
}

public partial class ButtonViewModel : ObservableObject, IRecipient<string>
{
    [ObservableProperty]
    public partial SplitButtonViewModel SplitButton { get; set; } = new SplitButtonViewModel();

    [ObservableProperty]
    public partial RadioButtonGroupEnum RadioButtonGroupEnum { get; set; } = RadioButtonGroupEnum.C;

    [RelayCommand]
    private void SenderMessage(string message)
    {
        WeakReferenceMessenger.Default.Send(message);
    }

    [RelayCommand]
    private void ShowSelectedRadioButton()
    {
        MessageBox.Show($"{RadioButtonGroupEnum}");
    }

    public void Receive(string message)
    {
        MessageBox.Show(message);
    }

    public ButtonViewModel()
    {
        WeakReferenceMessenger.Default.Register<string>(this);
    }
}

public partial class SplitButtonViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<FakerData> FakerData { get; set; }

    [ObservableProperty]
    public partial int Index { get; set; }

    [RelayCommand]
    private void ShowData(FakerData data)
    {
        if (data != null)
        {
            MessageBox.Show($"{data.FirstName} {data.LastName}");
        }
    }

    [RelayCommand]
    private void Test(object item)
    {
        MessageBox.Show($"{item}----{Index}");
    }

    public SplitButtonViewModel()
    {
        FakerData = new ObservableCollection<FakerData>(FakerDataHelper.GenerateFakerDataCollection(5));
    }
}

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum RadioButtonGroupEnum
{
    [Description("One")]
    A,

    [Description("Two")]
    B,

    [Description("Three")]
    C,

    [Description("Four")]
    D
}