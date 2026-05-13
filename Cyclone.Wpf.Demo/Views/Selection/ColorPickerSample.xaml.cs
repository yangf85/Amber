using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class ColorPickerSample : UserControl
{
    public ColorPickerSample()
    {
        InitializeComponent();
        DataContext = new ColorPickerViewModel();
    }
}

/// <summary>
/// ColorPickerSample 的演示 ViewModel
/// 命名空间与 View 一致,与 XAML 的 d:DesignInstance Type={x:Type local:ColorPickerViewModel} 对齐
/// </summary>
public partial class ColorPickerViewModel : ObservableObject
{
    private static readonly Random _random = new();

    #region Card 1: 基础绑定 (PickerColor)

    /// <summary>
    /// 主演示色,默认 OrangeRed
    /// </summary>
    [ObservableProperty]
    public partial Color PickerColor { get; set; }

    /// <summary>
    /// PickerColor 的十六进制文本 (OneWay 计算属性)
    /// </summary>
    public string PickerColorHex => ColorToHex(PickerColor);

    /// <summary>
    /// PickerColor 的 RGB 文本 (OneWay 计算属性)
    /// </summary>
    public string PickerColorRgb => ColorToRgb(PickerColor);

    partial void OnPickerColorChanged(Color value)
    {
        OnPropertyChanged(nameof(PickerColorHex));
        OnPropertyChanged(nameof(PickerColorRgb));
    }

    #endregion Card 1: 基础绑定 (PickerColor)

    #region Card 2: 配置选项 (ConfigColor / TextFormat / DisplayColorText / IsControlEnabled)

    /// <summary>
    /// 配置卡的演示色,默认 MediumSeaGreen
    /// </summary>
    [ObservableProperty]
    public partial Color ConfigColor { get; set; }

    /// <summary>
    /// 颜色文本格式 (HEX / RGB)
    /// </summary>
    [ObservableProperty]
    public partial ColorTextMode TextFormat { get; set; }

    /// <summary>
    /// HEX RadioButton 的 IsChecked 绑定 (OneWay)
    /// </summary>
    public bool IsHexFormat => TextFormat == ColorTextMode.HEX;

    /// <summary>
    /// RGB RadioButton 的 IsChecked 绑定 (OneWay)
    /// </summary>
    public bool IsRgbFormat => TextFormat == ColorTextMode.RGB;

    partial void OnTextFormatChanged(ColorTextMode value)
    {
        OnPropertyChanged(nameof(IsHexFormat));
        OnPropertyChanged(nameof(IsRgbFormat));
    }

    /// <summary>
    /// 是否在下拉触发器上显示颜色文本
    /// </summary>
    [ObservableProperty]
    public partial bool DisplayColorText { get; set; }

    /// <summary>
    /// 控件是否启用 (演示 Disabled 状态)
    /// </summary>
    [ObservableProperty]
    public partial bool IsControlEnabled { get; set; }

    #endregion Card 2: 配置选项

    #region Card 3: 独立面板 (PanelColor)

    /// <summary>
    /// 内嵌 ColorSelector 的演示色,默认 DodgerBlue
    /// </summary>
    [ObservableProperty]
    public partial Color PanelColor { get; set; }

    public string PanelColorHex => ColorToHex(PanelColor);

    partial void OnPanelColorChanged(Color value)
    {
        OnPropertyChanged(nameof(PanelColorHex));
    }

    #endregion Card 3: 独立面板 (PanelColor)

    #region Card 4: 独立色板 (PaletteColor)

    /// <summary>
    /// 内嵌 ColorPalette 的演示色,默认 MediumOrchid
    /// </summary>
    [ObservableProperty]
    public partial Color PaletteColor { get; set; }

    public string PaletteColorHex => ColorToHex(PaletteColor);

    partial void OnPaletteColorChanged(Color value)
    {
        OnPropertyChanged(nameof(PaletteColorHex));
    }

    #endregion Card 4: 独立色板 (PaletteColor)

    #region 构造

    public ColorPickerViewModel()
    {
        PickerColor       = Colors.OrangeRed;
        ConfigColor       = Colors.MediumSeaGreen;
        TextFormat        = ColorTextMode.HEX;
        DisplayColorText  = true;
        IsControlEnabled  = true;
        PanelColor        = Colors.DodgerBlue;
        PaletteColor      = Colors.MediumOrchid;
    }

    #endregion 构造

    #region 命令

    [RelayCommand]
    private void SetHexFormat() => TextFormat = ColorTextMode.HEX;

    [RelayCommand]
    private void SetRgbFormat() => TextFormat = ColorTextMode.RGB;

    /// <summary>
    /// 重置为初始 OrangeRed
    /// </summary>
    [RelayCommand]
    private void ResetPickerColor() => PickerColor = Colors.OrangeRed;

    /// <summary>
    /// 随机生成一个不透明色
    /// </summary>
    [RelayCommand]
    private void RandomPickerColor()
    {
        PickerColor = Color.FromRgb(
            (byte)_random.Next(0, 256),
            (byte)_random.Next(0, 256),
            (byte)_random.Next(0, 256));
    }

    #endregion 命令

    #region 辅助方法

    private static string ColorToHex(Color c)
        => string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);

    private static string ColorToRgb(Color c)
        => string.Format(CultureInfo.InvariantCulture, "RGB({0}, {1}, {2})", c.R, c.G, c.B);

    #endregion 辅助方法
}
