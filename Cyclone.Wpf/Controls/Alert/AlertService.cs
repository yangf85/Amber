using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 警告对话框服务的默认实现。单例（<see cref="Instance"/>）或手动 new。
/// </summary>
public class AlertService : IAlertService
{
    // ---- 静态单例 ----

    private static readonly object _instanceLock = new object();

    private static Lazy<AlertService> _lazyInstance =
            new Lazy<AlertService>(() => new AlertService(), LazyThreadSafetyMode.ExecutionAndPublication);

    private int _isDisposed;

    // ---- 实例字段 ----
    private Window _ownerWindow;

    private IntPtr _ownerHandle;

    // SetOwner 挂在 owner 上的 Closed handler——保存引用以便重入时解绑
    private EventHandler _ownerClosedHandler;

    private AlertWindow _currentAlert;

    private Window _currentMask;

    /// <summary>获取单例实例。</summary>
    public static AlertService Instance
    {
        get
        {
            lock (_instanceLock)
            {
                return _lazyInstance.Value;
            }
        }
    }

    public AlertOption Option { get; }

    private bool HasOwner => _ownerWindow != null || _ownerHandle != IntPtr.Zero;

    /// <summary>
    /// 重置单例。<b>注意：Dispose 不再自动调用此方法</b>——
    /// 单例语义和 Dispose 语义独立，如需释放后重新启用一个干净单例，由调用方显式触发。
    /// </summary>
    public static void ResetInstance()
    {
        lock (_instanceLock)
        {
            _lazyInstance = new Lazy<AlertService>(
                () => new AlertService(),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }

    public AlertService() : this(new AlertOption())
    {
    }

    public AlertService(AlertOption option)
    {
        Option = option ?? throw new ArgumentNullException(nameof(option));
    }

    #region SetOwner

    /// <summary>
    /// 把 WPF 窗口设为 alert 的 owner。多次调用先解绑旧 owner 的事件。
    /// </summary>
    public void SetOwner(Window owner)
    {
        if (owner == null)
        {
            throw new ArgumentNullException(nameof(owner));
        }
        ThrowIfDisposed();

        DetachOwnerHandlers();

        _ownerWindow = owner;
        _ownerHandle = new WindowInteropHelper(owner).Handle;

        _ownerClosedHandler = (_, _) => OnOwnerClosed();
        owner.Closed += _ownerClosedHandler;
    }

    /// <summary>把非 WPF 窗口（任意 hwnd）设为 owner。</summary>
    public void SetOwner(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(windowHandle), "Invalid WindowHandle");
        }
        ThrowIfDisposed();

        if (!WindowsNativeService.IsWindow(windowHandle))
        {
            throw new ArgumentException("Handle is not a Window", nameof(windowHandle));
        }

        DetachOwnerHandlers();
        _ownerWindow = null;
        _ownerHandle = windowHandle;
    }

    /// <summary>把当前前台窗口设为 owner。</summary>
    public void SetOwnerToForegroundWindow()
    {
        ThrowIfDisposed();

        var foregroundHandle = WindowsNativeService.GetForegroundWindow();
        if (foregroundHandle != IntPtr.Zero && WindowsNativeService.IsWindow(foregroundHandle))
        {
            DetachOwnerHandlers();
            _ownerWindow = null;
            _ownerHandle = foregroundHandle;
        }
    }

    private void DetachOwnerHandlers()
    {
        if (_ownerWindow != null && _ownerClosedHandler != null)
        {
            _ownerWindow.Closed -= _ownerClosedHandler;
        }
        _ownerWindow = null;
        _ownerHandle = IntPtr.Zero;
        _ownerClosedHandler = null;
    }

    private void OnOwnerClosed()
    {
        // owner 关闭：关掉活动 alert + mask，清理 owner 引用。但<b>不</b> Dispose 单例。
        InvokeOnDispatcher(() =>
        {
            try { _currentAlert?.Close(); } catch { }
            try { _currentMask?.Close(); } catch { }
            _currentAlert = null;
            _currentMask = null;
            DetachOwnerHandlers();
        });
    }

    #endregion SetOwner

    #region Show implementations

    /// <inheritdoc />
    public AlertResult Show(object content, string title = null)
        => ShowDialogCore(content, Option.DefaultButtonType, syncValidation: null, asyncValidation: null, title);

    /// <inheritdoc />
    public AlertResult Show(object content, AlertButton buttons, string title = null)
        => ShowDialogCore(content, buttons, syncValidation: null, asyncValidation: null, title);

    /// <inheritdoc />
    public AlertResult Show(object content, Func<bool> validation, string title = null)
    {
        if (validation == null)
        {
            throw new ArgumentNullException(nameof(validation));
        }
        return ShowDialogCore(content, AlertButton.OkCancel, validation, asyncValidation: null, title);
    }

    /// <inheritdoc />
    public Task<AlertResult> ShowAsync(object content, Func<Task<bool>> asyncValidation, string title = null)
    {
        if (asyncValidation == null)
        {
            throw new ArgumentNullException(nameof(asyncValidation));
        }

        // ShowDialog 本身阻塞，在 UI 线程上跑。返回时已经关闭——所以包成已完成的 Task 即可。
        var result = ShowDialogCore(content, AlertButton.OkCancel, syncValidation: null, asyncValidation, title);
        return Task.FromResult(result);
    }

    private static AlertResult MapDialogResult(bool? dialogResult)
    {
        return dialogResult switch
        {
            true => AlertResult.Ok,
            false => AlertResult.Cancel,
            _ => AlertResult.Closed,
        };
    }

    /// <summary>
    /// 三个 Show 实现的统一核心。之前 60 行核心逻辑被复制 3 份的问题在这里收敛。
    /// </summary>
    private AlertResult ShowDialogCore(object content, AlertButton buttons,
        Func<bool> syncValidation, Func<Task<bool>> asyncValidation, string title)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }
        ThrowIfDisposed();

        var dispatcher = GetDispatcher();
        AlertResult result = AlertResult.Closed;

        Action work = () =>
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 1)
            {
                return;
            }

            Window mask = null;
            AlertWindow window = null;

            try
            {
                // 1. 蒙版（如果开启 + 有 owner）
                if (Option.IsShowMask && HasOwner)
                {
                    mask = CreateMaskWindow();
                    mask?.Show();
                }
                _currentMask = mask;

                // 2. Alert window
                window = CreateAlertWindow(content, buttons, title);
                window.ValidationCallback = syncValidation;
                window.AsyncValidationCallback = asyncValidation;
                _currentAlert = window;

                // 3. owner / 居中——alert.Owner 必须优先指向 mask（如有），否则 mask 会盖住 alert
                ConfigureOwnership(window, mask);

                // 4. mask 跟着 alert 关闭
                window.Closed += (_, _) =>
                {
                    try
                    {
                        if (mask != null && mask.IsLoaded)
                        {
                            mask.Close();
                        }
                    }
                    catch { }
                };

                // 5. 模态显示并等待关闭
                bool? dialogResult = window.ShowDialog();
                result = MapDialogResult(dialogResult);
            }
            finally
            {
                _currentAlert = null;
                if (mask != null)
                {
                    try { mask.Close(); } catch { }
                }
                _currentMask = null;

                ActivateOwnerWindow();
            }
        };

        if (dispatcher.CheckAccess())
        {
            work();
        }
        else
        {
            dispatcher.Invoke(work);
        }

        return result;
    }

    #endregion Show implementations

    #region Window creation helpers

    private AlertWindow CreateAlertWindow(object content, AlertButton buttons, string title)
    {
        var window = new AlertWindow
        {
            Title = title ?? string.Empty,
            Content = content,
            ButtonType = buttons,
            OkButtonText = Option.OkButtonText,
            CancelButtonText = Option.CancelButtonText,
        };

        // 如果 content 是 AlertMessage，把它的 Level 同步到 window——标题栏图标自动跟内容图标一致
        if (content is AlertMessage alertMessage)
        {
            window.Level = alertMessage.Level;
        }

        return window;
    }

    private Window CreateMaskWindow()
    {
        Rect ownerRect;

        if (_ownerWindow != null)
        {
            ownerRect = new Rect(_ownerWindow.Left, _ownerWindow.Top, _ownerWindow.Width, _ownerWindow.Height);
        }
        else
        {
            var wpfRect = WindowsNativeService.GetWindowRectAsWpfRect(_ownerHandle);
            if (!wpfRect.HasValue)
            {
                return null;
            }
            ownerRect = wpfRect.Value;
        }

        try
        {
            var mask = new Window
            {
                Width = ownerRect.Width,
                Height = ownerRect.Height,
                Left = ownerRect.Left,
                Top = ownerRect.Top,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                WindowStartupLocation = WindowStartupLocation.Manual,
                IsHitTestVisible = false,
                Focusable = false,
                Topmost = false,
            };

            // 蒙版用主题里的 Overlay.Dark
            var overlayBrush = Application.Current?.TryFindResource("Overlay.Dark") as Brush
                               ?? new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0));

            mask.Content = new Rectangle
            {
                Width = ownerRect.Width,
                Height = ownerRect.Height,
                Fill = overlayBrush,
            };

            if (_ownerWindow != null)
            {
                mask.Owner = _ownerWindow;
            }

            return mask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AlertService] 创建蒙版窗口失败: {ex.Message}");
            return null;
        }
    }

    private void ConfigureOwnership(AlertWindow window, Window mask)
    {
        // alert.Owner 优先级：mask > realOwner > hwnd 居中 > 屏幕居中
        // 这里 alert.Owner = mask 是<b>必须</b>的——WPF owner 关系保证子窗口 Z 序始终在父窗口之上。
        // 如果让 mask 和 alert 平级（都以 realOwner 为 owner），Z 序未定，mask 可能盖住 alert，
        // 导致 alert 收不到点击和焦点——这是"弹窗卡死"的真凶。
        if (mask != null)
        {
            window.Owner = mask;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return;
        }

        if (_ownerWindow != null)
        {
            window.Owner = _ownerWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return;
        }

        if (WindowsNativeService.IsValidWindow(_ownerHandle))
        {
            if (AlertWindowPositioner.CenterAlertInOwner(window, _ownerHandle))
            {
                return;
            }
        }

        // 兜底：屏幕居中
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void ActivateOwnerWindow()
    {
        try
        {
            if (_ownerWindow != null && _ownerWindow.Visibility == Visibility.Visible)
            {
                _ownerWindow.Activate();
                var hwnd = new WindowInteropHelper(_ownerWindow).Handle;
                WindowsNativeService.ActivateAndBringToFront(hwnd);
            }
            else if (WindowsNativeService.IsValidWindow(_ownerHandle))
            {
                WindowsNativeService.ActivateAndBringToFront(_ownerHandle);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AlertService] 激活 owner 失败: {ex.Message}");
        }
    }

    #endregion Window creation helpers

    #region Dispatcher

    private static Dispatcher GetDispatcher()
    {
        return Application.Current?.Dispatcher
            ?? throw new InvalidOperationException(
                "AlertService 需要 Application.Current.Dispatcher——请在 WPF 应用启动后使用。");
    }

    private static void InvokeOnDispatcherStatic(Action action)
    {
        var d = Application.Current?.Dispatcher;
        if (d == null || d.CheckAccess())
        {
            action();
        }
        else
        {
            d.BeginInvoke(action);
        }
    }

    private void InvokeOnDispatcher(Action action)
    {
        if (action == null)
        {
            return;
        }
        InvokeOnDispatcherStatic(action);
    }

    #endregion Dispatcher

    #region IDisposable

    /// <summary>
    /// 释放服务。立即关闭所有活动的 alert + mask，解绑 owner 事件。
    /// <b>不再自动 ResetInstance</b>——单例语义独立于 Dispose。
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
        {
            return;
        }

        try
        {
            var d = Application.Current?.Dispatcher;

            Action cleanup = () =>
            {
                DetachOwnerHandlers();
                try { _currentAlert?.Close(); } catch { }
                try { _currentMask?.Close(); } catch { }
                _currentAlert = null;
                _currentMask = null;
            };

            if (d == null || d.CheckAccess())
            {
                cleanup();
            }
            else
            {
                d.Invoke(cleanup);
            }
        }
        catch
        {
            // Dispose 必须 swallow 一切异常
        }

        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 1)
        {
            throw new ObjectDisposedException(nameof(AlertService));
        }
    }

    ~AlertService()
    {
        Interlocked.Exchange(ref _isDisposed, 1);
    }

    #endregion IDisposable
}