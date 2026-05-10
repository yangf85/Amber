using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// InputView.xaml 的交互逻辑
/// </summary>
public partial class InputSample : UserControl
{
    public InputSample()
    {
        InitializeComponent();
        DataContext = new InputViewModel();
    }
}

public partial class InputViewModel : ObservableValidator
{
    public InputViewModel()
    {
        this.ErrorsChanged += (s, e) => OnPropertyChanged(nameof(IsValid));
    }

    #region 基础属性

    [ObservableProperty]
    [Required(ErrorMessage = "文本不能为空")]
    [NotifyDataErrorInfo]
    [NotifyPropertyChangedFor(nameof(TextLength))]
    public partial string Text { get; set; } = "";

    [ObservableProperty]
    public partial string SearchText { get; set; } = "";

    [ObservableProperty]
    public partial string MultilineText { get; set; } = "这是多行文本\n可以换行输入";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordMatch))]
    public partial string Password { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordMatch))]
    public partial string ConfirmPassword { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalValue))]
    public partial double NumberDouble { get; set; } = 10.5;

    [NotifyDataErrorInfo]
    [ObservableProperty]
    [Range(1, 100, ErrorMessage = "数量必须在1-100之间")]
    [NotifyPropertyChangedFor(nameof(TotalValue))]
    public partial int NumberInt { get; set; } = 100;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalValue))]
    public partial double Price { get; set; } = 299.99;

    [ObservableProperty]
    public partial double Percentage { get; set; } = 75.0;

    [ObservableProperty]
    public partial string EditableTitle { get; set; } = "标题";

    [ObservableProperty]
    public partial string EditableDescription { get; set; } = "描述";

    [ObservableProperty]
    public partial string EditableQuantity { get; set; } = "数量: 10";

    [ObservableProperty]
    public partial string ApiKey { get; set; } = "sk-1234567890abcdef";

    [ObservableProperty]
    public partial string ConfigPath { get; set; } = @"C:\Config\app.json";

    [ObservableProperty]
    public partial string HighlightText { get; set; } = "这是一段演示文本高亮功能的内容。The quick brown fox jumps over the lazy dog. 支持中英文搜索高亮显示。";

    #endregion 基础属性

    #region 计算属性

    /// <summary>
    /// 文本长度
    /// </summary>
    public int TextLength => Text?.Length ?? 0;

    /// <summary>
    /// 验证状态
    /// </summary>
    public bool IsValid => !HasErrors && !string.IsNullOrWhiteSpace(Text);

    /// <summary>
    /// 密码匹配
    /// </summary>
    public bool IsPasswordMatch => !string.IsNullOrEmpty(Password) && Password == ConfirmPassword;

    /// <summary>
    /// 总价值
    /// </summary>
    public double TotalValue => Price + NumberDouble + NumberInt;

    #endregion 计算属性

    #region 命令

    [RelayCommand]
    private void ShowPassword()
    {
        if (string.IsNullOrEmpty(Password))
        {
            NotificationService.Instance.Warning("密码为空");
            return;
        }

        NotificationService.Instance.Information($"当前密码: {Password}");
    }

    #endregion 命令
}