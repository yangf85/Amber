using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

public partial class TextBoxViewModel : ObservableValidator
{
    // UserName 用 [NotifyPropertyChangedFor]:变更时自动 raise UserNameLength PropertyChanged,
    // 替代旧的手写 partial void OnUserNameChanged 调 OnPropertyChanged(nameof(UserNameLength))
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UserNameLength))]
    public partial string UserName { get; set; } = "";

    public int UserNameLength => UserName?.Length ?? 0;

    // 一对多派生属性的演示 — SourceText 变更时三个派生属性同步通知
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SourceTextLength))]
    [NotifyPropertyChangedFor(nameof(SourceTextUpper))]
    [NotifyPropertyChangedFor(nameof(SourceTextReversed))]
    public partial string SourceText { get; set; } = "Hello, Cyclone";

    public int SourceTextLength => SourceText?.Length ?? 0;

    public string SourceTextUpper => SourceText?.ToUpperInvariant() ?? "";

    public string SourceTextReversed
        => string.IsNullOrEmpty(SourceText) ? "" : new string(SourceText.Reverse().ToArray());

    // 邮箱字段 — 由 ObservableValidator 自动通过 INotifyDataErrorInfo 报告错误,
    // 控件样式上的 Validation.ErrorTemplate 自动渲染。
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [EmailAddress(ErrorMessage = "邮箱格式不正确,需要 name@domain.com")]
    public partial string Email { get; set; } = "";

    [ObservableProperty]
    public partial string SearchText { get; set; } = "";

    // 备注字段 — MaxLength 同时作为验证(超长报错)
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MaxLength(200, ErrorMessage = "备注最多 200 字")]
    public partial string Comments { get; set; } = "";

    [ObservableProperty]
    public partial string LiveSyncText { get; set; } = "";

    [ObservableProperty]
    public partial string LastChange { get; set; } = "(尚未输入)";

    [ObservableProperty]
    public partial bool IsReadOnlyDemo { get; set; }

    // IsBusy 同样用 [NotifyPropertyChangedFor] 简化 CanInteract 的派生通知
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanInteract))]
    public partial bool IsBusy { get; set; }

    public bool CanInteract => !IsBusy;

    partial void OnUserNameChanged(string value)
    {
        // 不再需要 OnPropertyChanged(nameof(UserNameLength)) — [NotifyPropertyChangedFor] 已自动处理
        LastChange = $"UserName → \"{value}\"";
    }

    partial void OnSourceTextChanged(string value)
    {
        LastChange = $"SourceText → \"{value}\"";
    }

    partial void OnEmailChanged(string value)
    {
        LastChange = $"Email → \"{value}\"";
    }

    partial void OnSearchTextChanged(string value)
    {
        LastChange = $"Search → \"{value}\"";
    }

    partial void OnCommentsChanged(string value)
    {
        LastChange = $"Comments → {value?.Length ?? 0} 字";
    }

    partial void OnLiveSyncTextChanged(string value)
    {
        LastChange = $"LiveSync → \"{value}\"";
    }

    [RelayCommand]
    private void ToggleReadOnly()
    {
        IsReadOnlyDemo = !IsReadOnlyDemo;
    }

    [RelayCommand]
    private void ToggleBusy()
    {
        IsBusy = !IsBusy;
    }

    [RelayCommand]
    private void Reset()
    {
        UserName = "";
        SourceText = "";
        Email = "";
        SearchText = "";
        Comments = "";
        LiveSyncText = "";
        LastChange = "(已重置)";
        ClearErrors();
    }
}
