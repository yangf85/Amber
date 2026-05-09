using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class NotificationView : UserControl
{
    public NotificationView()
    {
        InitializeComponent();
        DataContext = new NotificationViewModel();
        Loaded += OnViewLoaded;
    }

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
        // 把通知服务挂到主窗口——通知就会跟随主窗口移动 / 缩放
        var hostWindow = Window.GetWindow(this);
        if (hostWindow != null)
        {
            NotificationService.Instance.SetOwner(hostWindow);
        }
    }
}

public partial class NotificationViewModel : ObservableObject
{
    [ObservableProperty]
    public partial NotificationPosition Position { get; set; } = NotificationPosition.BottomRight;

    [ObservableProperty]
    public partial int MaxCount { get; set; } = 5;

    [ObservableProperty]
    public partial double DisplayDurationSeconds { get; set; } = 2.4;

    [ObservableProperty]
    public partial bool HasActiveTrackedHandle { get; set; }

    [ObservableProperty]
    public partial int CustomClickCount { get; set; }

    // RadioButton 直接 enum 绑定要 converter，这里用 4 个 bool 属性更直接
    public bool IsTopLeft
    {
        get => Position == NotificationPosition.TopLeft;
        set { if (value) { Position = NotificationPosition.TopLeft; } }
    }

    public bool IsTopRight
    {
        get => Position == NotificationPosition.TopRight;
        set { if (value) { Position = NotificationPosition.TopRight; } }
    }

    public bool IsBottomLeft
    {
        get => Position == NotificationPosition.BottomLeft;
        set { if (value) { Position = NotificationPosition.BottomLeft; } }
    }

    public bool IsBottomRight
    {
        get => Position == NotificationPosition.BottomRight;
        set { if (value) { Position = NotificationPosition.BottomRight; } }
    }

    private INotificationHandle _trackedHandle;

    public NotificationViewModel()
    {
        // 应用初始 Option
        NotificationService.Instance.UpdateOption(opt =>
        {
            opt.Position = Position;
            opt.MaxCount = MaxCount;
            opt.DisplayDuration = TimeSpan.FromSeconds(DisplayDurationSeconds);
        });
    }

    [RelayCommand]
    private void ShowLevel(string level)
    {
        if (!Enum.TryParse<NotificationLevel>(level, out var lv))
        {
            return;
        }

        var text = lv switch
        {
            NotificationLevel.Default => "这是一条普通通知",
            NotificationLevel.Information => "检测到新版本 v2.1.0",
            NotificationLevel.Success => "文件已成功保存",
            NotificationLevel.Warning => "硬盘空间不足 10%",
            NotificationLevel.Error => "无法连接到服务器",
            _ => "Hello",
        };

        NotificationService.Instance.Notify(text, lv);
    }

    [RelayCommand]
    private async Task SimulateUpload()
    {
        // 进行中——先关掉旧的跟踪通知
        _trackedHandle?.Close();

        _trackedHandle = NotificationService.Instance.Information("文件上传中（点我有惊喜）...");
        HasActiveTrackedHandle = true;

        var handle = _trackedHandle;
        handle.Closed += (s, e) => Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (ReferenceEquals(_trackedHandle, handle))
            {
                HasActiveTrackedHandle = false;
            }
        }));
        handle.Clicked += (s, e) => Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            CustomClickCount++;
        }));

        await Task.Delay(3000);

        if (handle.IsClosed)
        {
            return;
        }

        // 上传完成——关掉旧的，弹出 Success 提示。
        // （Update 只能改 Message 文本，改不了 Level，所以新建一条）
        handle.Close();
        _trackedHandle = NotificationService.Instance.Success("文件上传成功！");
        HasActiveTrackedHandle = true;
        var doneHandle = _trackedHandle;
        doneHandle.Closed += (s, e) => Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (ReferenceEquals(_trackedHandle, doneHandle))
            {
                HasActiveTrackedHandle = false;
            }
        }));
    }

    [RelayCommand]
    private void CancelTracked()
    {
        _trackedHandle?.Close();
        _trackedHandle = null;
        HasActiveTrackedHandle = false;
    }

    [RelayCommand]
    private void ShowCustomContent()
    {
        // 自定义 UI 内容：任意 UIElement 都能塞进去
        var card = new Border
        {
            Background = (System.Windows.Media.Brush)Application.Current.FindResource("Background.Highlighted"),
            Padding = new Thickness(12, 8, 12, 8),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = "Alice 给你发了消息",
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.White,
                    },
                    new TextBlock
                    {
                        Text = "在吗？想问下今天的会议时间",
                        Margin = new Thickness(0, 4, 0, 0),
                        Foreground = System.Windows.Media.Brushes.White,
                        TextWrapping = TextWrapping.WrapWithOverflow,
                    },
                },
            },
        };

        var handle = NotificationService.Instance.Show(card);
        handle.Clicked += (s, e) => Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
        {
            CustomClickCount++;
        }));
    }

    [RelayCommand]
    private void TestMaxCount()
    {
        // 连发 8 条——会按当前 MaxCount 自动淘汰最旧的
        for (var i = 1; i <= 8; i++)
        {
            var n = i;
            NotificationService.Instance.Information($"批量消息 #{n}");
        }
    }

    partial void OnPositionChanged(NotificationPosition value)
    {
        // 通知 4 个 IsXxx 属性的 binding 重新求值
        OnPropertyChanged(nameof(IsTopLeft));
        OnPropertyChanged(nameof(IsTopRight));
        OnPropertyChanged(nameof(IsBottomLeft));
        OnPropertyChanged(nameof(IsBottomRight));

        NotificationService.Instance.UpdateOption(opt => opt.Position = value);
        // 立即重新弹一条，让用户能看到位置变化
        NotificationService.Instance.Notify($"位置已切换到 {value}", NotificationLevel.Information);
    }

    partial void OnMaxCountChanged(int value)
    {
        NotificationService.Instance.UpdateOption(opt => opt.MaxCount = value);
    }

    partial void OnDisplayDurationSecondsChanged(double value)
    {
        NotificationService.Instance.UpdateOption(opt => opt.DisplayDuration = TimeSpan.FromSeconds(value));
    }
}
