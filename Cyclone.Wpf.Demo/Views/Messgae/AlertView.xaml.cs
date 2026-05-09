using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views;

public partial class AlertView : UserControl
{
    public AlertView()
    {
        InitializeComponent();
        DataContext = new AlertViewModel();
        Loaded += OnViewLoaded;
    }

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
        // 把 Alert 服务挂到主窗口——alert 会在主窗口中心显示，mask 也只盖住主窗口
        var hostWindow = Window.GetWindow(this);
        if (hostWindow != null)
        {
            AlertService.Instance.SetOwner(hostWindow);
        }
    }
}

public partial class AlertViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string LastResultText { get; set; } = "（尚未触发）";

    [ObservableProperty]
    public partial Brush LastResultColor { get; set; } = Brushes.Gray;

    [ObservableProperty]
    public partial bool IsShowMask { get; set; } = true;

    [ObservableProperty]
    public partial bool IsShowLoadingOnAsync { get; set; } = true;

    [ObservableProperty]
    public partial string OkButtonText { get; set; } = "确定";

    [ObservableProperty]
    public partial string CancelButtonText { get; set; } = "取消";

    public AlertViewModel()
    {
        // 初始同步配置到 Service
        ApplyOptions();
    }

    private void ApplyOptions()
    {
        var opt = AlertService.Instance.Option;
        opt.IsShowMask = IsShowMask;
        opt.IsShowLoadingOnAsync = IsShowLoadingOnAsync;
        opt.OkButtonText = OkButtonText ?? "确定";
        opt.CancelButtonText = CancelButtonText ?? "取消";
    }

    partial void OnIsShowMaskChanged(bool value) => ApplyOptions();

    partial void OnIsShowLoadingOnAsyncChanged(bool value) => ApplyOptions();

    partial void OnOkButtonTextChanged(string value) => ApplyOptions();

    partial void OnCancelButtonTextChanged(string value) => ApplyOptions();

    private void ShowResult(AlertResult result, string detail = null)
    {
        LastResultText = detail != null ? $"{result} — {detail}" : result.ToString();
        LastResultColor = result switch
        {
            AlertResult.Ok => (Brush)Application.Current.FindResource("Background.Success"),
            AlertResult.Cancel => (Brush)Application.Current.FindResource("Foreground.Container"),
            AlertResult.Closed => (Brush)Application.Current.FindResource("Background.Warning"),
            _ => Brushes.Gray,
        };
    }

    [RelayCommand]
    private void ShowLevel(string levelText)
    {
        if (!Enum.TryParse<AlertIcon>(levelText, out var level))
        {
            return;
        }

        var (msg, title) = level switch
        {
            AlertIcon.None => ("这是一条普通消息。", "提示"),
            AlertIcon.Information => ("已发布新版本 v2.1.0。", "信息"),
            AlertIcon.Success => ("文件已保存到云端。", "成功"),
            AlertIcon.Warning => ("此操作将影响 12 条记录。", "警告"),
            AlertIcon.Error => ("无法连接到服务器，请检查网络。", "错误"),
            AlertIcon.Question => ("确定要删除选中的 3 个项目吗？", "确认"),
            _ => ("Hello", "提示"),
        };

        AlertResult result;
        if (level == AlertIcon.Question)
        {
            // Question 用 OkCancel
            result = AlertService.Instance.Confirm(msg, title);
        }
        else if (level == AlertIcon.None)
        {
            result = AlertService.Instance.Message(msg, title);
        }
        else
        {
            // 其它单 Ok 按钮
            var method = level switch
            {
                AlertIcon.Information => (Func<IAlertService, string, string, AlertResult>)((s, m, t) => s.Information(m, t)),
                AlertIcon.Success => (s, m, t) => s.Success(m, t),
                AlertIcon.Warning => (s, m, t) => s.Warning(m, t),
                AlertIcon.Error => (s, m, t) => s.Error(m, t),
                _ => null,
            };
            result = method != null ? method(AlertService.Instance, msg, title) : AlertResult.Closed;
        }

        ShowResult(result);
    }

    [RelayCommand]
    private void ShowSyncValidation()
    {
        // 构造一个带 TextBox 的内容——同步验证：TextBox 不能为空
        var nameInput = new TextBox
        {
            Width = 260,
            Height = 32,
            Padding = new Thickness(8, 0, 8, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 14,
        };

        var content = new StackPanel
        {
            Margin = new Thickness(20, 16, 20, 16),
            MinWidth = 320,
        };
        content.Children.Add(new TextBlock
        {
            Text = "请输入您的姓名（不能为空）：",
            Foreground = (Brush)Application.Current.FindResource("Foreground.Default"),
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 8),
        });
        content.Children.Add(nameInput);

        // 同步验证：返回 false 时 alert 保持打开
        var result = AlertService.Instance.Show(
            content,
            () => !string.IsNullOrWhiteSpace(nameInput.Text),
            "同步验证 — 输入姓名");

        ShowResult(result, result == AlertResult.Ok ? $"输入了：\"{nameInput.Text}\"" : null);
    }

    [RelayCommand]
    private async Task ShowAsyncValidation()
    {
        // 异步验证：模拟服务端校验，期间 Loading overlay 出现
        var nameInput = new TextBox
        {
            Width = 260,
            Height = 32,
            Padding = new Thickness(8, 0, 8, 0),
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 14,
        };

        var content = new StackPanel
        {
            Margin = new Thickness(20, 16, 20, 16),
            MinWidth = 320,
        };
        content.Children.Add(new TextBlock
        {
            Text = "请输入用户名（'admin' 视为已占用）：",
            Foreground = (Brush)Application.Current.FindResource("Foreground.Default"),
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 8),
        });
        content.Children.Add(nameInput);
        content.Children.Add(new TextBlock
        {
            Text = "点击确定后会显示 2 秒加载动画——模拟服务端校验。",
            Foreground = (Brush)Application.Current.FindResource("Foreground.Container"),
            FontSize = 11,
            Margin = new Thickness(0, 8, 0, 0),
            TextWrapping = TextWrapping.Wrap,
        });

        var result = await AlertService.Instance.ShowAsync(
            content,
            async () =>
            {
                await Task.Delay(2000);
                return !string.IsNullOrWhiteSpace(nameInput.Text)
                       && !string.Equals(nameInput.Text.Trim(), "admin", StringComparison.OrdinalIgnoreCase);
            },
            "异步验证 — 用户名查重");

        ShowResult(result, result == AlertResult.Ok ? $"用户名 \"{nameInput.Text}\" 可用" : null);
    }

    [RelayCommand]
    private void ShowCustomContent()
    {
        // 演示 Show 接受任意 UI 元素：自定义升级提示卡片
        var card = new StackPanel
        {
            Margin = new Thickness(24, 20, 24, 20),
            MinWidth = 360,
        };

        card.Children.Add(new TextBlock
        {
            Text = "🎉 发现新版本",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)Application.Current.FindResource("Foreground.Default"),
            Margin = new Thickness(0, 0, 0, 12),
        });

        card.Children.Add(new TextBlock
        {
            Text = "Cyclone.Wpf v3.2 已发布，包含 12 项改进和 8 项 bug 修复。",
            FontSize = 13,
            Foreground = (Brush)Application.Current.FindResource("Foreground.Container"),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 12),
        });

        var changeList = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
        foreach (var line in new[] { "• 新增 PerMonitor DPI 支持", "• 修复 NotificationService 内存泄漏", "• 重构 Alert 验证流程" })
        {
            changeList.Children.Add(new TextBlock
            {
                Text = line,
                FontSize = 12,
                Foreground = (Brush)Application.Current.FindResource("Foreground.Container"),
                Margin = new Thickness(0, 0, 0, 4),
            });
        }
        card.Children.Add(changeList);

        var result = AlertService.Instance.Show(card, AlertButton.OkCancel, "升级提示");
        ShowResult(result, result == AlertResult.Ok ? "用户选择了立即升级" : null);
    }
}
