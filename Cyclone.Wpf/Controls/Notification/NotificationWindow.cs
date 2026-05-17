using Cyclone.Wpf.Themes;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 单条通知窗口。提供两种关闭方式：
/// <see cref="CloseWithAnimation"/>（带滑出动画）和 <see cref="CloseImmediately"/>（立即关）。
/// </summary>
internal class NotificationWindow : Window
{
    private DispatcherTimer _autoCloseTimer;

    /// <summary>正在关闭：0 = 否，1 = 是。Interlocked 操作保证唯一关闭路径。</summary>
    private int _isClosing;

    private double _originalLeft;

    private double _originalTop;

    public NotificationWindow()
    {
        CommandBindings.Add(new CommandBinding(CloseWindowCommand, OnExecuteCloseWindow));

        // 1. 主题挂接 —— 把 CurrentTheme(BasicTheme/DarkTheme...)merge 到 this.Resources[0]
        this.AttachThemeManager();

        // 2. 再合并本控件专属样式字典 —— 它的模板里 DynamicResource 会通过 [0] 的主题解析
        try
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Notification.xaml", UriKind.Absolute),
            };
            Resources.MergedDictionaries.Add(dict);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NotificationWindow] 合并 Notification.xaml 失败: {ex.Message}");
        }

        Loaded += OnNotificationLoaded;
    }

    #region DisplayDuration

    public static readonly DependencyProperty DisplayDurationProperty =
        DependencyProperty.Register(
            nameof(DisplayDuration),
            typeof(TimeSpan),
            typeof(NotificationWindow),
            new PropertyMetadata(TimeSpan.FromMilliseconds(2400), OnDisplayDurationChanged));

    public TimeSpan DisplayDuration
    {
        get => (TimeSpan)GetValue(DisplayDurationProperty);
        set => SetValue(DisplayDurationProperty, value);
    }

    private static void OnDisplayDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NotificationWindow w)
        {
            return;
        }

        var newDelay = (TimeSpan)e.NewValue;
        if (newDelay <= TimeSpan.Zero)
        {
            w.StopAutoCloseTimer();
        }
        else
        {
            w.ResetAutoCloseTimer();
        }
    }

    #endregion DisplayDuration

    #region IsShowCloseButton

    public static readonly DependencyProperty IsShowCloseButtonProperty =
        DependencyProperty.Register(
            nameof(IsShowCloseButton),
            typeof(bool),
            typeof(NotificationWindow),
            new PropertyMetadata(true));

    public bool IsShowCloseButton
    {
        get => (bool)GetValue(IsShowCloseButtonProperty);
        set => SetValue(IsShowCloseButtonProperty, value);
    }

    #endregion IsShowCloseButton

    #region AnimationDirection

    public static readonly DependencyProperty AnimationDirectionProperty =
        DependencyProperty.Register(
            nameof(AnimationDirection),
            typeof(NotificationAnimationDirection),
            typeof(NotificationWindow),
            new PropertyMetadata(NotificationAnimationDirection.FromRight));

    public NotificationAnimationDirection AnimationDirection
    {
        get => (NotificationAnimationDirection)GetValue(AnimationDirectionProperty);
        set => SetValue(AnimationDirectionProperty, value);
    }

    #endregion AnimationDirection

    #region NotificationClicked event

    /// <summary>
    /// 用户在通知主体上左键点击时触发。点击关闭按钮 / 任何 ButtonBase 子元素不触发。
    /// 由 <see cref="NotificationHandle"/> 转发到外部 INotificationHandle.Clicked。
    /// </summary>
    internal event EventHandler NotificationClicked;

    #endregion NotificationClicked event

    #region Commands

    /// <summary>
    /// 关闭窗口的路由命令（带动画）。模板中的关闭按钮 Command 绑定到此。
    /// </summary>
    public static readonly RoutedCommand CloseWindowCommand =
        new RoutedCommand(nameof(CloseWindowCommand), typeof(NotificationWindow));

    private void OnExecuteCloseWindow(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is NotificationWindow w)
        {
            w.CloseWithAnimation();
            e.Handled = true;
        }
    }

    #endregion Commands

    #region Public Close Methods

    /// <summary>
    /// 立即关闭（无动画）。用于：
    /// MaxCount 淘汰最老一条、Service.Dispose 关全部。
    /// 重复调用幂等。
    /// </summary>
    public void CloseImmediately()
    {
        if (Interlocked.Exchange(ref _isClosing, 1) != 0)
        {
            return;
        }

        StopAutoCloseTimer();
        base.Close();
    }

    /// <summary>
    /// 带滑出动画的关闭。重复调用幂等。用于：
    /// 用户主动关（点 X 按钮 / handle.Close()）、自动关闭计时器到期。
    /// </summary>
    public void CloseWithAnimation()
    {
        if (Interlocked.Exchange(ref _isClosing, 1) != 0)
        {
            return;
        }

        StopAutoCloseTimer();
        PlayCloseAnimation(() => base.Close());
    }

    #endregion Public Close Methods

    #region Override Methods

    /// <inheritdoc />
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        CleanupResources();
    }

    /// <inheritdoc />
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        StopAutoCloseTimer();
    }

    /// <inheritdoc />
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (DisplayDuration > TimeSpan.Zero)
        {
            StartAutoCloseTimer();
        }
    }

    /// <inheritdoc />
    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);

        if (e.Handled)
        {
            return;
        }

        // 排除按钮内部点击：任何 ButtonBase 子元素的点击都不算"通知主体被点"
        if (e.OriginalSource is DependencyObject src && IsInsideButton(src))
        {
            return;
        }

        NotificationClicked?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // 用户自定义模板若启用 WindowChrome + SizeToContent 时，强制重新测量
        if (SizeToContent == SizeToContent.WidthAndHeight && WindowChrome.GetWindowChrome(this) != null)
        {
            InvalidateMeasure();
        }
    }

    #endregion Override Methods

    #region Private - Animation

    private void OnNotificationLoaded(object sender, RoutedEventArgs e)
    {
        // 保留进入动画前的目标位置（已被 Positioner 设置好）
        _originalLeft = Left;
        _originalTop = Top;

        SetInitialPositionForAnimation();
        PlayOpenAnimation();

        if (DisplayDuration > TimeSpan.Zero)
        {
            StartAutoCloseTimer();
        }
    }

    private void PlayCloseAnimation(Action onCompleted)
    {
        var positionAnim = AnimationDirection switch
        {
            NotificationAnimationDirection.FromLeft => new DoubleAnimation
            {
                From = Left,
                To = _originalLeft - ActualWidth,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
            },
            _ => new DoubleAnimation
            {
                From = Left,
                To = _originalLeft + ActualWidth,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
            },
        };

        var opacityAnim = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
        };

        opacityAnim.Completed += (_, _) => onCompleted?.Invoke();

        BeginAnimation(LeftProperty, positionAnim);
        BeginAnimation(OpacityProperty, opacityAnim);
    }

    private void PlayOpenAnimation()
    {
        var positionAnim = AnimationDirection switch
        {
            NotificationAnimationDirection.FromLeft => new DoubleAnimation
            {
                From = _originalLeft - ActualWidth,
                To = _originalLeft,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            },
            _ => new DoubleAnimation
            {
                From = _originalLeft + ActualWidth,
                To = _originalLeft,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            },
        };

        var opacityAnim = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };

        BeginAnimation(LeftProperty, positionAnim);
        BeginAnimation(OpacityProperty, opacityAnim);
    }

    private void SetInitialPositionForAnimation()
    {
        switch (AnimationDirection)
        {
            case NotificationAnimationDirection.FromLeft:
                Left = _originalLeft - ActualWidth;
                break;

            case NotificationAnimationDirection.FromRight:
                Left = _originalLeft + ActualWidth;
                break;
        }
    }

    #endregion Private - Animation

    #region Private - Timer

    private void OnAutoCloseTick(object sender, EventArgs e)
    {
        StopAutoCloseTimer();
        CloseWithAnimation();
    }

    private void ResetAutoCloseTimer()
    {
        StopAutoCloseTimer();
        if (DisplayDuration > TimeSpan.Zero)
        {
            StartAutoCloseTimer();
        }
    }

    private void StartAutoCloseTimer()
    {
        if (_autoCloseTimer == null)
        {
            _autoCloseTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
            _autoCloseTimer.Tick += OnAutoCloseTick;
        }

        if (_autoCloseTimer.IsEnabled || IsMouseOver || DisplayDuration <= TimeSpan.Zero)
        {
            return;
        }

        _autoCloseTimer.Interval = DisplayDuration;
        _autoCloseTimer.Start();
    }

    private void StopAutoCloseTimer()
    {
        if (_autoCloseTimer != null && _autoCloseTimer.IsEnabled)
        {
            _autoCloseTimer.Stop();
        }
    }

    #endregion Private - Timer

    #region Private - Helpers

    private static bool IsInsideButton(DependencyObject src)
    {
        while (src != null)
        {
            if (src is ButtonBase)
            {
                return true;
            }
            src = VisualTreeHelper.GetParent(src) ?? LogicalTreeHelper.GetParent(src);
        }
        return false;
    }

    private void CleanupResources()
    {
        StopAutoCloseTimer();
        if (_autoCloseTimer != null)
        {
            _autoCloseTimer.Tick -= OnAutoCloseTick;
            _autoCloseTimer = null;
        }
        Loaded -= OnNotificationLoaded;
    }

    #endregion Private - Helpers
}