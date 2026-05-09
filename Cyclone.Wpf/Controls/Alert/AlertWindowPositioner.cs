using System;
using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// Alert 窗口在 owner 中心定位的辅助类。
/// 通过 <see cref="WindowsNativeService"/> 的 PerMonitor DPI API 实现跨屏正确居中。
/// </summary>
internal static class AlertWindowPositioner
{
    /// <summary>
    /// 把 alert 窗口在 owner 窗口的中心定位（按 owner 所在 monitor 的 DPI）。
    /// </summary>
    /// <returns>是否成功定位</returns>
    public static bool CenterAlertInOwner(Window alertWindow, IntPtr ownerHandle)
    {
        if (alertWindow == null || !WindowsNativeService.IsValidWindow(ownerHandle))
        {
            return false;
        }

        try
        {
            // owner 矩形（DIPs，已按 owner 所在显示器的 DPI 转换）
            var ownerRect = WindowsNativeService.GetWindowRectAsWpfRect(ownerHandle);
            if (!ownerRect.HasValue)
            {
                return false;
            }
            var owner = ownerRect.Value;

            // 触发一次测量获得 alert 期望尺寸
            if (alertWindow.ActualWidth == 0 || alertWindow.ActualHeight == 0)
            {
                alertWindow.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                alertWindow.Arrange(new Rect(alertWindow.DesiredSize));
            }

            var alertWidth = alertWindow.ActualWidth > 0 ? alertWindow.ActualWidth : alertWindow.DesiredSize.Width;
            var alertHeight = alertWindow.ActualHeight > 0 ? alertWindow.ActualHeight : alertWindow.DesiredSize.Height;
            if (alertWidth <= 0)
            {
                alertWidth = 400;
            }
            if (alertHeight <= 0)
            {
                alertHeight = 200;
            }

            alertWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            alertWindow.Left = owner.Left + (owner.Width - alertWidth) / 2;
            alertWindow.Top = owner.Top + (owner.Height - alertHeight) / 2;

            // 边界裁剪——用 owner 自己 monitor 的工作区
            EnsureWindowInWorkArea(alertWindow, ownerHandle, alertWidth, alertHeight);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void EnsureWindowInWorkArea(Window window, IntPtr ownerHandle, double width, double height)
    {
        var workArea = WindowsNativeService.GetWorkAreaForWindow(ownerHandle);

        if (window.Left < workArea.Left)
        {
            window.Left = workArea.Left;
        }
        else if (window.Left + width > workArea.Right)
        {
            window.Left = workArea.Right - width;
        }

        if (window.Top < workArea.Top)
        {
            window.Top = workArea.Top;
        }
        else if (window.Top + height > workArea.Bottom)
        {
            window.Top = workArea.Bottom - height;
        }
    }
}
