using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class ButtonSample : UserControl
{
    public ButtonSample()
    {
        InitializeComponent();
        DataContext = new ButtonViewModel();
    }
}

public partial class ButtonViewModel : ObservableObject
{
    [ObservableProperty]
    public partial int ClickCount { get; set; } = 0;

    [ObservableProperty]
    public partial string LastAction { get; set; } = "(尚未操作)";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public bool CanInteract => !IsBusy;

    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(CanInteract));

    [RelayCommand]
    private void Execute(string action)
    {
        ClickCount++;
        LastAction = $"[{ClickCount}] {action}";
    }

    [RelayCommand]
    private void ToggleBusy()
    {
        IsBusy = !IsBusy;
    }

    [RelayCommand]
    private void ResetCounter()
    {
        ClickCount = 0;
        LastAction = "(已重置)";
    }
}
