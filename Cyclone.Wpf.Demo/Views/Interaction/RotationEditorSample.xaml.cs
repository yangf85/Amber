using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace Cyclone.Wpf.Demo.Views;

public partial class RotationEditorSample : UserControl
{
    public RotationEditorSample()
    {
        InitializeComponent();
        DataContext = new RotationEditorViewModel();
    }
}

public partial class RotationEditorViewModel : ObservableObject
{
    // Cube 旋转 — 三轴
    [ObservableProperty]
    public partial double AngleX { get; set; } = 25;

    [ObservableProperty]
    public partial double AngleY { get; set; } = 35;

    [ObservableProperty]
    public partial double AngleZ { get; set; } = 0;

    // 2D 旋转图标场景
    [ObservableProperty]
    public partial double IconRotation { get; set; } = 0;

    // 联动 Step
    [ObservableProperty]
    public partial double Step { get; set; } = 1;

    // 联动 Precision
    [ObservableProperty]
    public partial int Precision { get; set; } = 0;

    // 状态 — 记录最后一次 AngleChanged 事件
    [ObservableProperty]
    public partial string LastChangeLog { get; set; } = "(尚未编辑)";

    public void RecordAngleChange(string axis, double oldValue, double newValue)
    {
        LastChangeLog = $"{axis} 轴: {oldValue:F1}° → {newValue:F1}°";
    }

    [RelayCommand]
    private void Preset45()
    {
        AngleX = 45;
        AngleY = 45;
        AngleZ = 45;
    }

    [RelayCommand]
    private void PresetFront()
    {
        AngleX = 0;
        AngleY = 0;
        AngleZ = 0;
    }

    [RelayCommand]
    private void PresetIsometric()
    {
        AngleX = 30;
        AngleY = 45;
        AngleZ = 0;
    }

    [RelayCommand]
    private void PresetTop()
    {
        AngleX = 90;
        AngleY = 0;
        AngleZ = 0;
    }
}
