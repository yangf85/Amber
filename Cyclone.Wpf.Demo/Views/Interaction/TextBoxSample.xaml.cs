using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class TextBoxSample : UserControl
{
    public TextBoxSample()
    {
        InitializeComponent();
        DataContext = new TextBoxViewModel();
    }
}

public partial class TextBoxViewModel : ObservableObject
{
    // 基础字段
    [ObservableProperty]
    public partial string UserName { get; set; } = "";

    [ObservableProperty]
    public partial string Email { get; set; } = "";

    [ObservableProperty]
    public partial string SearchKeyword { get; set; } = "";

    // 状态控制
    [ObservableProperty]
    public partial bool IsFormEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool IsFormReadOnly { get; set; } = false;

    // IsFormEnabled 的反向,绑给 "禁用模式" CheckBox(避免用 Converter)
    public bool IsFormDisabled
    {
        get => !IsFormEnabled;
        set => IsFormEnabled = !value;
    }

    partial void OnIsFormEnabledChanged(bool value) => OnPropertyChanged(nameof(IsFormDisabled));

    // 多行文本 + 字符计数
    [ObservableProperty]
    public partial string Description { get; set; } = "";

    public int DescriptionLength => Description?.Length ?? 0;

    public int DescriptionLimit { get; } = 200;

    partial void OnDescriptionChanged(string value) => OnPropertyChanged(nameof(DescriptionLength));

    // 验证示例
    [ObservableProperty]
    public partial string PhoneNumber { get; set; } = "";

    public bool IsPhoneValid => string.IsNullOrEmpty(PhoneNumber) || PhoneNumber.Length == 11;

    partial void OnPhoneNumberChanged(string value) => OnPropertyChanged(nameof(IsPhoneValid));

    [RelayCommand]
    private void FillSample()
    {
        UserName = "alice_chen";
        Email = "alice@example.com";
        PhoneNumber = "13800000000";
        Description = "这是一段示例描述文字,用于演示多行 TextBox 的字符计数功能。";
    }

    [RelayCommand]
    private void ClearAll()
    {
        UserName = "";
        Email = "";
        PhoneNumber = "";
        Description = "";
        SearchKeyword = "";
    }
}
