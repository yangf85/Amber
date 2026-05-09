using System;
using System.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// <see cref="INotificationHandle"/> 的内部实现。包装一个 <see cref="NotificationWindow"/>，
/// 把 window 的关闭 / 点击事件桥接到外部接口。
/// </summary>
internal sealed class NotificationHandle : INotificationHandle
{
    private readonly NotificationWindow _window;
    private int _isClosed;

    public NotificationHandle(NotificationWindow window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));
        _window.Closed += OnWindowClosed;
        _window.NotificationClicked += OnWindowClicked;
    }

    public bool IsClosed => Interlocked.CompareExchange(ref _isClosed, 0, 0) == 1;

    public event EventHandler Clicked;

    public event EventHandler Closed;

    public void Close()
    {
        if (IsClosed)
        {
            return;
        }

        // 跨线程安全：调度到 window 自己的线程
        _window.Dispatcher.BeginInvoke(new Action(() =>
        {
            _window.CloseWithAnimation();
        }));
    }

    public void Update(object content)
    {
        if (IsClosed)
        {
            return;
        }

        _window.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (IsClosed)
            {
                return;
            }

            // 字符串 + 已是 NotificationMessage：仅更新文本
            if (_window.Content is NotificationMessage message && content is string text)
            {
                message.Message = text;
            }
            else
            {
                _window.Content = content;
            }
        }));
    }

    private void OnWindowClosed(object sender, EventArgs e)
    {
        if (Interlocked.Exchange(ref _isClosed, 1) != 0)
        {
            return;
        }

        // 解除事件订阅，避免 window 持有 handle 引用阻止 GC
        _window.Closed -= OnWindowClosed;
        _window.NotificationClicked -= OnWindowClicked;

        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void OnWindowClicked(object sender, EventArgs e)
    {
        if (IsClosed)
        {
            return;
        }
        Clicked?.Invoke(this, EventArgs.Empty);
    }
}
