using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 通知服务接口。<see cref="Show"/> 返回 <see cref="INotificationHandle"/>，
/// 调用方可通过 handle 关闭、更新内容或监听点击事件。
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// 显示一条通知。
    /// </summary>
    /// <param name="content">通知内容（不可为 null）。可以是 <see cref="NotificationMessage"/>、字符串或任意 UI 对象。</param>
    /// <param name="title">窗口标题（一般不可见，仅 Win32 调试用）。</param>
    /// <returns>通知句柄，可用于后续操作。已 disposed 时返回 null。</returns>
    INotificationHandle Show(object content, string title = null);
}

/// <summary>
/// 通知服务的默认实现。单例（<see cref="Instance"/>），亦可手动 new。
/// </summary>
public class NotificationService : INotificationService, IDisposable
{
    // ---- 静态单例 ----

    private static Lazy<NotificationService> _lazyInstance =
        new Lazy<NotificationService>(() => new NotificationService(), LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly object _instanceLock = new object();

    /// <summary>获取通知服务的单例实例。</summary>
    public static NotificationService Instance => _lazyInstance.Value;

    /// <summary>
    /// 重置单例实例。<b>注意：Dispose 不再自动调用此方法</b>——
    /// 单例语义和 Dispose 语义彼此独立，如需释放后重新启用一个干净的单例，由调用方显式触发。
    /// </summary>
    public static void ResetInstance()
    {
        lock (_instanceLock)
        {
            _lazyInstance = new Lazy<NotificationService>(
                () => new NotificationService(),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }

    // ---- 实例字段 ----

    /// <summary>
    /// 活动通知窗口字典；value 是创建时间戳，用于排序。
    /// 所有读写都在 UI 线程上发生（通过 InvokeOnDispatcher 调度）。
    /// 用 ConcurrentDictionary 而非锁，主要图它的 TryAdd / TryRemove 一组幂等 API。
    /// </summary>
    private readonly ConcurrentDictionary<NotificationWindow, DateTime> _activeWindows
        = new ConcurrentDictionary<NotificationWindow, DateTime>();

    private readonly NotificationWindowPositioner _windowPositioner;

    private IntPtr _ownerHandle;

    // SetOwner 挂在 owner 上的 handler 引用，重入时先解绑
    private Window _ownerWindow;

    private EventHandler _ownerLocationChangedHandler;
    private SizeChangedEventHandler _ownerSizeChangedHandler;
    private EventHandler _ownerStateChangedHandler;
    private EventHandler _ownerClosedHandler;

    private int _isDisposed;

    public NotificationOption Option { get; }

    public NotificationService() : this(new NotificationOption())
    {
    }

    public NotificationService(NotificationOption option)
    {
        Option = option ?? throw new ArgumentNullException(nameof(option));
        _windowPositioner = new NotificationWindowPositioner(option);
    }

    #region SetOwner

    /// <summary>
    /// 把 WPF 窗口设为通知 owner——通知会跟随 owner 移动 / 缩放。
    /// 多次调用会先解绑旧 owner 的事件，避免内存泄漏。
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
        _ownerLocationChangedHandler = (_, _) => RepositionActiveWindows();
        _ownerSizeChangedHandler = (_, _) => RepositionActiveWindows();
        _ownerStateChangedHandler = (_, _) => RepositionActiveWindows();
        _ownerClosedHandler = (_, _) => OnOwnerClosed();

        owner.LocationChanged += _ownerLocationChangedHandler;
        owner.SizeChanged += _ownerSizeChangedHandler;
        owner.StateChanged += _ownerStateChangedHandler;
        owner.Closed += _ownerClosedHandler;

        var handle = new WindowInteropHelper(owner).Handle;
        SetOwnerInternal(handle);
    }

    /// <summary>
    /// 把非 WPF 窗口（任意 hwnd）设为通知 owner。
    /// 注意：此重载不会自动跟随 owner 的移动 / 缩放——这部分需要 host 应用主动通知服务。
    /// </summary>
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
        SetOwnerInternal(windowHandle);
    }

    /// <summary>
    /// 把当前前台窗口设为 owner。前台窗口不存在时回退到屏幕坐标。
    /// </summary>
    public void SetOwnerToForegroundWindow()
    {
        ThrowIfDisposed();

        var foregroundHandle = WindowsNativeService.GetForegroundWindow();
        if (foregroundHandle != IntPtr.Zero)
        {
            DetachOwnerHandlers();
            SetOwnerInternal(foregroundHandle);
        }
        else
        {
            _windowPositioner.UseScreenPositioning();
        }
    }

    private void SetOwnerInternal(IntPtr handle)
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 1)
        {
            return;
        }

        if (WindowsNativeService.IsValidWindow(handle))
        {
            _ownerHandle = handle;
            _windowPositioner.SetOwner(handle);
            InvokeOnDispatcher(RepositionActiveWindows);
        }
    }

    private void DetachOwnerHandlers()
    {
        if (_ownerWindow == null)
        {
            return;
        }

        _ownerWindow.LocationChanged -= _ownerLocationChangedHandler;
        _ownerWindow.SizeChanged -= _ownerSizeChangedHandler;
        _ownerWindow.StateChanged -= _ownerStateChangedHandler;
        _ownerWindow.Closed -= _ownerClosedHandler;

        _ownerWindow = null;
        _ownerLocationChangedHandler = null;
        _ownerSizeChangedHandler = null;
        _ownerStateChangedHandler = null;
        _ownerClosedHandler = null;
    }

    private void OnOwnerClosed()
    {
        // owner 关闭：关掉所有通知 + 切回屏幕定位。但不 Dispose 单例——
        // 调用方仍然可以再 SetOwner 给另一个窗口，继续用同一个服务实例。
        InvokeOnDispatcher(() =>
        {
            CloseAllImmediately();
            DetachOwnerHandlers();
            _ownerHandle = IntPtr.Zero;
            _windowPositioner.UseScreenPositioning();
        });
    }

    #endregion SetOwner

    #region Show

    /// <inheritdoc />
    public INotificationHandle Show(object content, string title = null)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }
        ThrowIfDisposed();

        var dispatcher = GetDispatcher();
        NotificationWindow window = null;

        Action create = () =>
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 1)
            {
                return;
            }

            window = new NotificationWindow
            {
                Title = title ?? string.Empty,
                Content = content,
                Width = Option.Width,
                Height = Option.Height,
                IsShowCloseButton = Option.IsShowCloseButton,
                DisplayDuration = Option.DisplayDuration,
            };

            AddWindow(window);
        };

        // 用 Invoke 同步等待 window 创建完成——这样 Show 返回时 handle 立即可用
        if (dispatcher.CheckAccess())
        {
            create();
        }
        else
        {
            dispatcher.Invoke(create);
        }

        return window != null ? new NotificationHandle(window) : null;
    }

    private void AddWindow(NotificationWindow window)
    {
        // 超过 MaxCount：找最早的一条，立即关闭（不走动画）避免视觉跳变
        while (_activeWindows.Count >= Option.MaxCount)
        {
            var ordered = _activeWindows.OrderBy(p => p.Value).ToList();
            if (ordered.Count == 0)
            {
                break;
            }

            var oldest = ordered[0].Key;
            if (_activeWindows.TryRemove(oldest, out _))
            {
                oldest.CloseImmediately();
            }
            else
            {
                break;
            }
        }

        _activeWindows.TryAdd(window, DateTime.Now);
        window.Closed += OnWindowClosed;

        _windowPositioner.SetAnimationDirection(window);
        RepositionActiveWindows();
        window.Show();
    }

    private void OnWindowClosed(object sender, EventArgs e)
    {
        if (sender is not NotificationWindow window)
        {
            return;
        }

        window.Closed -= OnWindowClosed;
        _activeWindows.TryRemove(window, out _);
        RepositionActiveWindows();
    }

    private void RepositionActiveWindows()
    {
        InvokeOnDispatcher(() =>
        {
            // 顶部位置：最新的在底部（按时间升序）
            // 底部位置：最新的在顶部（按时间降序）
            var isTopPosition = Option.Position == NotificationPosition.TopLeft
                                 || Option.Position == NotificationPosition.TopRight;

            var snapshot = isTopPosition
                ? _activeWindows.OrderBy(p => p.Value).Select(p => p.Key).ToList()
                : _activeWindows.OrderByDescending(p => p.Value).Select(p => p.Key).ToList();

            _windowPositioner.PositionWindows(snapshot);
        });
    }

    private void CloseAllImmediately()
    {
        var windows = _activeWindows.Keys.ToArray();
        _activeWindows.Clear();
        foreach (var w in windows)
        {
            w.Closed -= OnWindowClosed;
            w.CloseImmediately();
        }
    }

    #endregion Show

    #region Option update

    /// <summary>运行时修改配置。修改后需要调用 <see cref="RepositionActiveWindows"/> 视情况重排。</summary>
    public void UpdateOption(Action<NotificationOption> action)
    {
        ThrowIfDisposed();
        action?.Invoke(Option);
    }

    #endregion Option update

    #region Dispatcher

    private Dispatcher GetDispatcher()
    {
        return Application.Current?.Dispatcher
            ?? throw new InvalidOperationException(
                "NotificationService 需要 Application.Current.Dispatcher——请在 WPF 应用启动后使用。");
    }

    private void InvokeOnDispatcher(Action action)
    {
        if (action == null)
        {
            return;
        }

        var dispatcher = GetDispatcher();
        if (dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            dispatcher.BeginInvoke(action);
        }
    }

    #endregion Dispatcher

    #region IDisposable

    private void ThrowIfDisposed()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 0, 0) == 1)
        {
            throw new ObjectDisposedException(nameof(NotificationService));
        }
    }

    /// <summary>
    /// 释放服务并立即关闭所有活动通知。<b>不再自动 ResetInstance</b>——
    /// 单例语义独立于 Dispose；如需重置单例请显式调 <see cref="ResetInstance"/>。
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) != 0)
        {
            return;
        }

        // 关闭走立即关，避免动画期间的引用悬挂
        try
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                DetachOwnerHandlers();
                CloseAllImmediately();
            }
            else
            {
                dispatcher.Invoke(() =>
                {
                    DetachOwnerHandlers();
                    CloseAllImmediately();
                });
            }
        }
        catch
        {
            // Dispose 必须 swallow 所有异常
        }

        GC.SuppressFinalize(this);
    }

    ~NotificationService()
    {
        // finalizer 路径：不能碰 dispatcher / 其他托管对象
        Interlocked.Exchange(ref _isDisposed, 1);
    }

    #endregion IDisposable
}
