using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class StepperSample : UserControl
{
    public StepperSample()
    {
        InitializeComponent();
        DataContext = new StepperViewModel();
    }
}

public partial class StepperViewModel : ObservableObject
{
    // ====== 注册向导主流程 ======

    [ObservableProperty]
    public partial int RegisterStep { get; set; } = 0;

    [ObservableProperty]
    public partial string Account { get; set; } = "";

    [ObservableProperty]
    public partial string Password { get; set; } = "";

    [ObservableProperty]
    public partial string Email { get; set; } = "";

    [ObservableProperty]
    public partial bool AgreedTerms { get; set; } = false;

    // 步骤可前进的条件——绑到下一步按钮的 IsEnabled / Command CanExecute
    public bool CanGoNext => RegisterStep switch
    {
        0 => !string.IsNullOrWhiteSpace(Account) && Password.Length >= 6,
        1 => !string.IsNullOrWhiteSpace(Email) && Email.Contains('@'),
        2 => AgreedTerms,
        _ => false,
    };

    partial void OnRegisterStepChanged(int value) => OnPropertyChanged(nameof(CanGoNext));
    partial void OnAccountChanged(string value) => OnPropertyChanged(nameof(CanGoNext));
    partial void OnPasswordChanged(string value) => OnPropertyChanged(nameof(CanGoNext));
    partial void OnEmailChanged(string value) => OnPropertyChanged(nameof(CanGoNext));
    partial void OnAgreedTermsChanged(bool value) => OnPropertyChanged(nameof(CanGoNext));

    [RelayCommand]
    private void GoNext()
    {
        if (CanGoNext && RegisterStep < 3)
        {
            RegisterStep++;
        }
    }

    [RelayCommand]
    private void GoPrevious()
    {
        if (RegisterStep > 0)
        {
            RegisterStep--;
        }
    }

    [RelayCommand]
    private void Reset()
    {
        RegisterStep = 0;
        Account = "";
        Password = "";
        Email = "";
        AgreedTerms = false;
    }

    // ====== 订单进度(垂直) ======

    [ObservableProperty]
    public partial int OrderStep { get; set; } = 2;

    [RelayCommand]
    private void AdvanceOrder()
    {
        if (OrderStep < 4)
        {
            OrderStep++;
        }
    }
}
