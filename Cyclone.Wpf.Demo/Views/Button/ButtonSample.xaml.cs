using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
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

public partial class ButtonViewModel : ObservableRecipient
{
    /// <summary>
    /// 接收端 VM,放在主 VM 的属性下方便 XAML 局部 DataContext 切换。
    /// </summary>
    public ButtonListenerViewModel Listener { get; }

    // ClickCount 用 [NotifyPropertyChangedRecipients]:每次变更时通过默认 Messenger
    // (WeakReferenceMessenger.Default) 广播 PropertyChangedMessage<int>。
    // 任何继承 ObservableRecipient + IRecipient<PropertyChangedMessage<int>> 的 VM
    // 都能收到通知,实现跨 VM 解耦通信。
    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    public partial int ClickCount { get; set; } = 0;

    [ObservableProperty]
    public partial string LastAction { get; set; } = "(尚未操作)";

    // IsBusy 用 [NotifyPropertyChangedFor]:变更时自动 raise CanInteract 的 PropertyChanged,
    // 省掉手写 partial void OnIsBusyChanged 调 OnPropertyChanged(nameof(CanInteract))。
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanInteract))]
    public partial bool IsBusy { get; set; }

    public bool CanInteract => !IsBusy;

    // FormInput 用 [NotifyCanExecuteChangedFor]:变更时自动通知 SubmitFormCommand
    // 重新评估 CanExecute,无需手写 SubmitFormCommand.NotifyCanExecuteChanged()。
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitFormCommand))]
    public partial string FormInput { get; set; } = "";

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
        FormInput = "";
        LastAction = "(已重置)";
    }

    // CanExecute 通过 RelayCommand 的 CanExecute 参数声明,与 [NotifyCanExecuteChangedFor] 配合。
    // FormInput 一变 → 生成器自动调用 SubmitFormCommand.NotifyCanExecuteChanged()
    // → WPF 重新查询 CanExecute → 按钮 IsEnabled 跟着切换。
    [RelayCommand(CanExecute = nameof(CanSubmitForm))]
    private void SubmitForm()
    {
        ClickCount++;
        LastAction = $"[{ClickCount}] 表单提交: \"{FormInput}\"";
    }

    private bool CanSubmitForm() => !string.IsNullOrWhiteSpace(FormInput);

    public ButtonViewModel()
    {
        Listener = new ButtonListenerViewModel();
        // 设为 true 让 ObservableRecipient 进入激活态。本 VM 自身不接收消息,
        // 但这是 ObservableRecipient 的标准用法 — 与 Broadcast 行为对称。
        IsActive = true;
    }
}

/// <summary>
/// 监听端 VM。
/// IsActive=true 时 ObservableRecipient.OnActivated 调用 Messenger.RegisterAll(this),
/// 自动扫描所有 IRecipient&lt;T&gt; 接口实现并注册到默认 Messenger。
/// 任何地方广播 PropertyChangedMessage&lt;int&gt; 都会触达 Receive 方法。
/// </summary>
public partial class ButtonListenerViewModel : ObservableRecipient, IRecipient<PropertyChangedMessage<int>>
{
    [ObservableProperty]
    public partial string LastBroadcast { get; set; } = "(等待广播...)";

    [ObservableProperty]
    public partial int BroadcastCount { get; set; } = 0;

    public void Receive(PropertyChangedMessage<int> message)
    {
        BroadcastCount++;
        LastBroadcast = $"#{BroadcastCount} {message.Sender.GetType().Name}.{message.PropertyName}: {message.OldValue} → {message.NewValue}";
    }

    public ButtonListenerViewModel()
    {
        IsActive = true;
    }
}