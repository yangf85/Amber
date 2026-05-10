using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views;

public partial class PopupBoxSample : UserControl
{
    public PopupBoxSample()
    {
        InitializeComponent();
        DataContext = new PopupBoxViewModel();
    }
}

public partial class PopupBoxViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string LastEvent { get; set; } = "(尚未发生事件)";

    [ObservableProperty]
    public partial int OpenCount { get; set; }

    [ObservableProperty]
    public partial int CloseCount { get; set; }

    /// <summary>用于演示 popup 内表单绑定 + CloseCommand。</summary>
    public ContactFormViewModel ContactForm { get; } = new();

    /// <summary>主按钮 Command。</summary>
    [RelayCommand]
    private void Trigger() => LastEvent = $"主按钮 Command 触发(@ {DateTime.Now:HH:mm:ss})";

    /// <summary>OpenedCommand —— popup 展开时执行。</summary>
    [RelayCommand]
    private void HandleOpened()
    {
        OpenCount++;
        LastEvent = $"OpenedCommand 触发(累计 {OpenCount} 次)";
    }

    /// <summary>ClosedCommand —— popup 关闭时执行。</summary>
    [RelayCommand]
    private void HandleClosed()
    {
        CloseCount++;
        LastEvent = $"ClosedCommand 触发(累计 {CloseCount} 次)";
    }
}

/// <summary>用于演示 popup 内表单的子 VM。</summary>
public partial class ContactFormViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = "请输入姓名...";

    [ObservableProperty]
    public partial string Email { get; set; } = "请输入邮箱地址...";

    [ObservableProperty]
    public partial string LastSubmitted { get; set; } = "(未提交)";

    /// <summary>
    /// 双向绑定到 PopupBox.IsOpen —— VM 决定 popup 何时关闭。
    /// 验证失败时保持打开,成功提交后关闭;比 CloseCommand 更适合"提交成功才关"这类条件关闭场景。
    /// </summary>
    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    [RelayCommand]
    private void Submit()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            LastSubmitted = "(姓名为空,未提交)";
            return;   // 验证失败,popup 保持打开
        }

        LastSubmitted = $"已提交: {Name} <{Email}>";
        Name = string.Empty;
        Email = string.Empty;
        IsOpen = false;   // 成功提交后由 VM 主动关闭 popup
    }

    public ContactFormViewModel()
    {
    }
}