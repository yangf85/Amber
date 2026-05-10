using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Themes;
using System.Diagnostics;
using System.Windows;

namespace Cyclone.Wpf.Demo.ViewModels;

public partial class ShellWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsClosing { get; set; }

    /// <summary>
    /// 切换主题。委托给 ThemeManager.SwitchTo——找不到主题时静默 trace warning，
    /// 不会把 CurrentTheme 设为 null（避免应用失去主题资源）。
    /// </summary>
    [RelayCommand]
    private void SwitchTheme(string themeName)
    {
        if (string.IsNullOrEmpty(themeName)) return;

        Debug.WriteLine($"\n=== 切换前 ===");
        foreach (var d in Application.Current.Resources.MergedDictionaries)
            Debug.WriteLine($"  - {d.GetType().Name}  Source={d.Source}");

        var ok = ThemeManager.SwitchTo(themeName);

        Debug.WriteLine($"\n=== 切换后 ({themeName} ok={ok}) ===");
        foreach (var d in Application.Current.Resources.MergedDictionaries)
            Debug.WriteLine($"  - {d.GetType().Name}  Source={d.Source}");
    }

    partial void OnIsClosingChanged(bool value)
    {
        if (value)
        {
            Debug.WriteLine("Closing application");
        }
    }
}