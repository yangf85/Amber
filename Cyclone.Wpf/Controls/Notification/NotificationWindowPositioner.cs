using System;
using System.Collections.Generic;
using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 处理通知窗口的定位。所有计算统一使用 WPF DIPs，
/// 所有外部坐标（Win32 RECT 等）按 owner 所在显示器的 DPI 转换后再用。
/// </summary>
internal class NotificationWindowPositioner
{
    private readonly NotificationOption _option;

    private IntPtr _ownerHandle;

    private volatile bool _useScreenForPositioning = true;

    /// <summary>
    /// 设置用于定位通知的所有者句柄
    /// </summary>
    public void SetOwner(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(windowHandle), "Invalid WindowHandle");
        }

        if (!WindowsNativeService.IsWindow(windowHandle))
        {
            throw new ArgumentException("Handle is not a Window", nameof(windowHandle));
        }

        _ownerHandle = windowHandle;
        _useScreenForPositioning = false;
    }

    /// <summary>
    /// 重置为使用屏幕坐标进行定位
    /// </summary>
    public void UseScreenPositioning()
    {
        _useScreenForPositioning = true;
    }

    /// <summary>
    /// 根据当前设置定位所有通知窗口。
    /// 入参 activeWindows 已由 NotificationService 按时间戳排序——本方法保持顺序，
    /// 不再重排（之前的 OrderBy(IndexOf) 是 O(N²) 且冗余）。
    /// </summary>
    public void PositionWindows(IList<NotificationWindow> activeWindows)
    {
        if (activeWindows == null || activeWindows.Count == 0)
        {
            return;
        }

        var useScreen = _useScreenForPositioning;
        var ownerHandle = _ownerHandle;

        // 锚点矩形 + 工作区，全部 DIPs
        Rect anchorRect;
        Rect workArea;

        if (useScreen || !WindowsNativeService.IsValidWindow(ownerHandle))
        {
            // 屏幕模式：用主屏 work area，没有 owner 上下文
            workArea = WindowsNativeService.GetSystemWorkArea();
            anchorRect = workArea;
        }
        else
        {
            // 跟随 owner 模式：按 owner 所在 monitor 的 DPI 转换坐标
            var ownerRect = WindowsNativeService.GetWindowRectAsWpfRect(ownerHandle);
            workArea = WindowsNativeService.GetWorkAreaForWindow(ownerHandle);

            if (ownerRect.HasValue)
            {
                anchorRect = ownerRect.Value;
            }
            else
            {
                // owner rect 拿不到，退到工作区
                anchorRect = workArea;
            }
        }

        // 计算第一个通知的左上角位置（DIPs）
        double baseLeft;
        double baseTop;
        bool isTop;

        switch (_option.Position)
        {
            case NotificationPosition.TopLeft:
                baseLeft = anchorRect.Left + _option.OffsetX;
                baseTop = anchorRect.Top + _option.OffsetY;
                isTop = true;
                break;

            case NotificationPosition.TopRight:
                baseLeft = anchorRect.Right - _option.Width - _option.OffsetX;
                baseTop = anchorRect.Top + _option.OffsetY;
                isTop = true;
                break;

            case NotificationPosition.BottomLeft:
                baseLeft = anchorRect.Left + _option.OffsetX;
                baseTop = anchorRect.Bottom - _option.OffsetY;
                isTop = false;
                break;

            case NotificationPosition.BottomRight:
            default:
                baseLeft = anchorRect.Right - _option.Width - _option.OffsetX;
                baseTop = anchorRect.Bottom - _option.OffsetY;
                isTop = false;
                break;
        }

        // 横向裁剪到工作区
        if (baseLeft + _option.Width > workArea.Right)
        {
            baseLeft = workArea.Right - _option.Width;
        }
        if (baseLeft < workArea.Left)
        {
            baseLeft = workArea.Left;
        }

        // 纵向起点裁剪到工作区
        if (isTop && baseTop < workArea.Top)
        {
            baseTop = workArea.Top;
        }
        else if (!isTop && baseTop > workArea.Bottom)
        {
            baseTop = workArea.Bottom;
        }

        // 按顺序堆叠定位（不再重排，service 已经排好了）
        var currentTop = baseTop;
        foreach (var window in activeWindows)
        {
            var windowHeight = window.ActualHeight > 0 ? window.ActualHeight : _option.Height;

            window.Left = baseLeft;

            if (isTop)
            {
                // 顶部定位：第一条贴顶部，向下堆叠
                if (currentTop + windowHeight > workArea.Bottom)
                {
                    // 超出工作区底部：跳过这条（视觉上让最早的几条留在屏幕里，
                    // 而不是单独 clamp 导致全部叠在屏幕底部互相覆盖）
                    window.Top = workArea.Bottom;   // 推到屏外
                    window.Visibility = Visibility.Hidden;
                }
                else
                {
                    window.Top = currentTop;
                    window.Visibility = Visibility.Visible;
                }
                currentTop += windowHeight + _option.Spacing;
            }
            else
            {
                // 底部定位：第一条贴底部，向上堆叠
                var top = currentTop - windowHeight;
                if (top < workArea.Top)
                {
                    window.Top = workArea.Top - windowHeight;   // 推到屏外
                    window.Visibility = Visibility.Hidden;
                }
                else
                {
                    window.Top = top;
                    window.Visibility = Visibility.Visible;
                }
                currentTop = top - _option.Spacing;
            }
        }
    }

    /// <summary>
    /// 根据位置为通知窗口设置动画方向
    /// </summary>
    public void SetAnimationDirection(NotificationWindow window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        switch (_option.Position)
        {
            case NotificationPosition.TopLeft:
            case NotificationPosition.BottomLeft:
                window.AnimationDirection = NotificationAnimationDirection.FromLeft;
                break;

            case NotificationPosition.TopRight:
            case NotificationPosition.BottomRight:
            default:
                window.AnimationDirection = NotificationAnimationDirection.FromRight;
                break;
        }

        if (window.ActualWidth == 0)
        {
            window.Width = _option.Width;
        }

        if (window.ActualHeight == 0)
        {
            window.Height = _option.Height;
        }
    }

    public NotificationWindowPositioner(NotificationOption option)
    {
        _option = option ?? throw new ArgumentNullException(nameof(option));
    }
}