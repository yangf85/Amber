using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 提供对Windows原生API的封装，用于窗口操作和事件监控
/// </summary>
internal static class WindowsNativeService
{
    #region 结构体定义

    /// <summary>
    /// Windows RECT 结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;
    }

    #endregion 结构体定义

    #region 常量定义

    // ShowWindow函数的常量参数
    public const int SW_SHOW = 5;             // 显示窗口

    public const int SW_SHOWNA = 8;           // 显示窗口但不激活

    // 窗口样式常量
    public const int GWL_EXSTYLE = -20;               // 扩展窗口样式

    public const int WS_EX_NOACTIVATE = 0x08000000;   // 窗口不激活

    public const int WS_EX_TRANSPARENT = 0x00000020;  // 透明窗口（点击穿透）

    public const int HWND_TOPMOST = -1;               // 置顶窗口

    public const int HWND_NOTOPMOST = -2;             // 取消置顶

    // SetWindowPos标志常量
    public const uint SWP_NOSIZE = 0x0001;        // 保持当前大小

    public const uint SWP_NOMOVE = 0x0002;        // 保持当前位置

    public const uint SWP_NOACTIVATE = 0x0010;    // 不激活窗口

    public const uint SWP_SHOWWINDOW = 0x0040;    // 显示窗口

    // 窗口消息常量
    public const int WM_MOVE = 0x0003;            // 窗口移动消息

    public const int WM_SIZE = 0x0005;            // 窗口大小改变消息

    // 窗口事件常量
    public const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;  // 窗口位置变化事件

    public const uint EVENT_OBJECT_REORDER = 0x8004;         // 窗口Z序变化事件

    public const uint WINEVENT_OUTOFCONTEXT = 0x0000;        // 事件钩子标志

    // 窗口显示和隐藏事件
    /// <summary>
    /// 窗口显示事件，触发于窗口从隐藏变为显示时
    /// </summary>
    public const uint EVENT_OBJECT_SHOW = 0x8002;

    /// <summary>
    /// 窗口隐藏事件，触发于窗口从显示变为隐藏时
    /// </summary>
    public const uint EVENT_OBJECT_HIDE = 0x8003;

    /// <summary>
    /// 窗口获得前台焦点事件，当窗口成为前台窗口时触发
    /// </summary>
    public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

    // 用于DPI计算的默认值，用于非WPF环境
    private const double DEFAULT_DPI = 96.0;

    // MonitorFromWindow的标志
    private const uint MONITOR_DEFAULTTONULL = 0;

    private const uint MONITOR_DEFAULTTOPRIMARY = 1;

    private const uint MONITOR_DEFAULTTONEAREST = 2;

    #endregion 常量定义

    #region 委托定义

    /// <summary>
    /// 窗口事件监控委托
    /// </summary>
    public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
        IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    #endregion 委托定义

    #region Window API导入

    // GetDpiForMonitor 的 dpiType 参数
    private const int MDT_EFFECTIVE_DPI = 0;

    // GetDeviceCaps的索引常量
    private const int LOGPIXELSX = 88;

    private const int LOGPIXELSY = 90;

    // 用于获取工作区的常量
    private const uint SPI_GETWORKAREA = 0x0030;

    /// <summary>
    /// 获取窗口矩形
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    /// <summary>
    /// 判断句柄是否为有效窗口
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool IsWindow(IntPtr hWnd);

    /// <summary>
    /// 获取前台窗口句柄
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    /// <summary>
    /// 设置前台窗口
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>
    /// 将窗口置于顶层
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    /// <summary>
    /// 判断窗口是否可见
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    /// <summary>
    /// 启用或禁用窗口
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    /// <summary>
    /// 显示窗口
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    /// 获取窗口样式
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    /// <summary>
    /// 设置窗口样式
    /// </summary>
    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    /// <summary>
    /// 设置窗口位置和Z顺序
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    /// <summary>
    /// 设置窗口事件钩子
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
        IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
        uint idThread, uint dwFlags);

    /// <summary>
    /// 移除窗口事件钩子
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    /// <summary>
    /// 获取系统DPI设置（Windows 8.1及以上）
    /// </summary>
    [DllImport("shcore.dll")]
    public static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

    /// <summary>
    /// 获取窗口所在显示器的句柄
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    /// <summary>
    /// 获取指定显示器的信息（含工作区物理像素 RECT）
    /// </summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    /// <summary>
    /// 获取桌面窗口句柄
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();

    /// <summary>
    /// 获取工作区域大小
    /// </summary>
    [DllImport("user32.dll")]
    public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref RECT pvParam, uint fWinIni);

    /// <summary>
    /// 获取设备上下文的设备能力
    /// </summary>
    [DllImport("gdi32.dll")]
    public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    /// <summary>
    /// 获取窗口DC
    /// </summary>
    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);

    /// <summary>
    /// 释放DC
    /// </summary>
    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    /// <summary>
    /// 显示器信息结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;

        public RECT rcMonitor;   // 物理像素：完整显示区

        public RECT rcWork;      // 物理像素：工作区（不含任务栏）

        public uint dwFlags;
    }

    #endregion Window API导入

    #region 实用方法

    /// <summary>
    /// 判断窗口句柄是否有效
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否有效</returns>
    public static bool IsValidWindow(IntPtr windowHandle)
    {
        try
        {
            return windowHandle != IntPtr.Zero && IsWindow(windowHandle);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 设置窗口为无激活样式
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static void SetWindowNoActivate(IntPtr windowHandle)
    {
        try
        {
            if (IsValidWindow(windowHandle))
            {
                var extendedStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
                SetWindowLong(windowHandle, GWL_EXSTYLE, extendedStyle | WS_EX_NOACTIVATE);
            }
        }
        catch (Exception)
        {
            // 忽略可能的异常，确保不会中断程序流程
        }
    }

    /// <summary>
    /// 将窗口设置为置顶不激活
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static void SetWindowTopMostNoActivate(IntPtr windowHandle)
    {
        try
        {
            if (IsValidWindow(windowHandle))
            {
                SetWindowPos(windowHandle, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
        }
        catch (Exception)
        {
            // 忽略可能的异常
        }
    }

    /// <summary>
    /// 将窗口设置为置顶并显示
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static void SetWindowTopMostAndShow(IntPtr windowHandle)
    {
        try
        {
            if (IsValidWindow(windowHandle))
            {
                SetWindowPos(windowHandle, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
        }
        catch (Exception)
        {
            // 忽略可能的异常
        }
    }

    /// <summary>
    /// 显示窗口但不激活
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static void ShowWindowNoActivate(IntPtr windowHandle)
    {
        try
        {
            if (IsValidWindow(windowHandle))
            {
                ShowWindow(windowHandle, SW_SHOWNA);
            }
        }
        catch (Exception)
        {
            // 忽略可能的异常
        }
    }

    /// <summary>
    /// 激活窗口并置于前台
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static void ActivateAndBringToFront(IntPtr windowHandle)
    {
        try
        {
            if (IsValidWindow(windowHandle))
            {
                BringWindowToTop(windowHandle);
                SetForegroundWindow(windowHandle);
            }
        }
        catch (Exception)
        {
            // 忽略可能的异常
        }
    }

    /// <summary>
    /// 设置窗口的Z顺序，使指定窗口位于参考窗口之上
    /// </summary>
    /// <param name="hWnd">要设置的窗口句柄</param>
    /// <param name="hWndInsertAfter">参考窗口句柄，新窗口将置于此窗口之上</param>
    /// <returns>操作是否成功</returns>
    public static bool SetWindowZOrder(IntPtr hWnd, IntPtr hWndInsertAfter)
    {
        try
        {
            if (IsValidWindow(hWnd) && IsValidWindow(hWndInsertAfter))
            {
                // 在Windows API中，窗口Z顺序是一个链表，其中hWndInsertAfter指定的是
                // 作为参考的窗口，然后hWnd被放在这个窗口之后（在Z顺序链中）
                // 换句话说，hWnd将会在屏幕显示上位于hWndInsertAfter之上
                return SetWindowPos(hWnd, hWndInsertAfter, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE);
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 取消窗口的置顶状态
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static void SetWindowNotTopMost(IntPtr windowHandle)
    {
        try
        {
            if (IsValidWindow(windowHandle))
            {
                SetWindowPos(windowHandle, new IntPtr(HWND_NOTOPMOST), 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
        }
        catch (Exception)
        {
            // 忽略可能的异常
        }
    }

    /// <summary>
    /// 获取窗口矩形（按 hwnd 所在 monitor 的 DPI 转换为 WPF DIPs）
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>窗口矩形（DIPs），失败返回 null</returns>
    public static Rect? GetWindowRectAsWpfRect(IntPtr windowHandle)
    {
        try
        {
            if (!IsValidWindow(windowHandle))
            {
                return null;
            }

            if (!GetWindowRect(windowHandle, out RECT rect))
            {
                return null;
            }

            // 关键：用 windowHandle 自己所在 monitor 的 DPI，跨屏才能对齐
            var (sx, sy) = GetDpiScalesForWindow(windowHandle);
            return new Rect(
                rect.Left / sx,
                rect.Top / sy,
                (rect.Right - rect.Left) / sx,
                (rect.Bottom - rect.Top) / sy);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 获取指定 hwnd 所在显示器的工作区（DIPs，已按该显示器 DPI 转换）。
    /// 用于多屏 + 各屏不同缩放下定位通知 / 弹窗。
    /// </summary>
    /// <param name="windowHandle">参考窗口句柄</param>
    /// <returns>工作区矩形（DIPs）；hwnd 无效则回退到主屏 work area</returns>
    public static Rect GetWorkAreaForWindow(IntPtr windowHandle)
    {
        try
        {
            if (IsValidWindow(windowHandle))
            {
                IntPtr hMonitor = MonitorFromWindow(windowHandle, MONITOR_DEFAULTTONEAREST);
                if (hMonitor != IntPtr.Zero)
                {
                    var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
                    if (GetMonitorInfo(hMonitor, ref info))
                    {
                        var (sx, sy) = GetDpiScalesForMonitor(hMonitor);
                        return new Rect(
                            info.rcWork.Left / sx,
                            info.rcWork.Top / sy,
                            (info.rcWork.Right - info.rcWork.Left) / sx,
                            (info.rcWork.Bottom - info.rcWork.Top) / sy);
                    }
                }
            }
        }
        catch (Exception)
        {
            // 落到下面 fallback
        }

        return GetSystemWorkArea();
    }

    /// <summary>
    /// 获取指定 hwnd 所在 monitor 的 DPI 缩放因子（相对 96 DPI）。
    /// 例：100% → (1.0, 1.0)；150% → (1.5, 1.5)。
    /// hwnd 无效或 API 调用失败时回退到主屏 DPI。
    /// </summary>
    public static (double scaleX, double scaleY) GetDpiScalesForWindow(IntPtr windowHandle)
    {
        if (IsValidWindow(windowHandle))
        {
            IntPtr hMonitor = MonitorFromWindow(windowHandle, MONITOR_DEFAULTTONEAREST);
            if (hMonitor != IntPtr.Zero)
            {
                return GetDpiScalesForMonitor(hMonitor);
            }
        }

        // fallback：主屏 DPI（旧行为）
        double s = GetSystemDpiScale();
        return (s, s);
    }

    /// <summary>
    /// 获取系统工作区域（桌面区域，不包括任务栏）。
    /// <para>注意：该方法返回主屏工作区。多屏 + 不同 DPI 场景下，请用
    /// <see cref="GetWorkAreaForWindow(IntPtr)"/> 取目标 hwnd 所在显示器的工作区。</para>
    /// </summary>
    /// <returns>工作区域矩形（DIPs）</returns>
    public static Rect GetSystemWorkArea()
    {
        try
        {
            // 首先尝试使用WPF方式获取工作区
            if (Application.Current != null)
            {
                return SystemParameters.WorkArea;
            }

            // 如果WPF方式不可用，则使用Windows API
            RECT workArea = new RECT();
            if (SystemParametersInfo(SPI_GETWORKAREA, 0, ref workArea, 0))
            {
                return new Rect(
                    workArea.Left,
                    workArea.Top,
                    workArea.Right - workArea.Left,
                    workArea.Bottom - workArea.Top
                );
            }

            // 如果API调用失败，则返回屏幕默认值
            return new Rect(0, 0, 1920, 1080);
        }
        catch (Exception)
        {
            // 如果出现任何异常，返回默认值
            return new Rect(0, 0, 1920, 1080);
        }
    }

    /// <summary>
    /// 将 RECT 结构转换为 WPF Rect（设备无关单位）。
    /// <para>注意：使用 MainWindow / 主屏的单一 DPI 转换，跨屏不同缩放时会偏移。
    /// 已知 hwnd 时请改用 <see cref="GetWindowRectAsWpfRect(IntPtr)"/>。</para>
    /// </summary>
    /// <param name="rect">原始RECT结构</param>
    /// <returns>转换后的WPF Rect</returns>
    public static Rect ConvertRectToWpfUnit(RECT rect)
    {
        try
        {
            Point topLeft = ConvertPixelToWpfUnit(rect.Left, rect.Top);
            Point bottomRight = ConvertPixelToWpfUnit(rect.Right, rect.Bottom);

            return new Rect(topLeft, bottomRight);
        }
        catch (Exception)
        {
            // 如果转换失败，则直接使用像素值创建Rect
            return new Rect(
                rect.Left,
                rect.Top,
                rect.Right - rect.Left,
                rect.Bottom - rect.Top
            );
        }
    }

    /// <summary>
    /// 将物理像素坐标转换为WPF设备无关单位
    /// </summary>
    /// <param name="x">像素X坐标</param>
    /// <param name="y">像素Y坐标</param>
    /// <returns>WPF坐标点</returns>
    public static Point ConvertPixelToWpfUnit(int x, int y)
    {
        try
        {
            Matrix transformToDevice = GetDpiScale();
            double dpiX = transformToDevice.M11;
            double dpiY = transformToDevice.M22;

            // 确保DPI非零
            if (Math.Abs(dpiX) < 0.01) dpiX = 1.0;
            if (Math.Abs(dpiY) < 0.01) dpiY = 1.0;

            // 转换坐标
            return new Point(x / dpiX, y / dpiY);
        }
        catch (Exception)
        {
            // 在异常情况下，使用默认的96 DPI进行转换
            double dpiScale = GetSystemDpiScale();
            return new Point(x / dpiScale, y / dpiScale);
        }
    }

    /// <summary>
    /// 获取当前 DPI 缩放矩阵（基于 MainWindow 所在显示器或主屏）。
    /// <para>注意：返回单一 DPI，不区分多屏。已知 hwnd 时请用
    /// <see cref="GetDpiScalesForWindow(IntPtr)"/>。</para>
    /// </summary>
    /// <returns>DPI缩放矩阵</returns>
    public static Matrix GetDpiScale()
    {
        try
        {
            // 首先尝试获取WPF的DPI设置
            if (Application.Current?.MainWindow != null)
            {
                var source = PresentationSource.FromVisual(Application.Current.MainWindow);
                if (source?.CompositionTarget != null)
                {
                    return source.CompositionTarget.TransformToDevice;
                }
            }

            // 如果无法通过WPF获取，则尝试使用Windows API
            double dpiScale = GetSystemDpiScale();
            return new Matrix(dpiScale, 0, 0, dpiScale, 0, 0);
        }
        catch (Exception)
        {
            // 如果出现任何异常，返回默认值
            return Matrix.Identity; // 1:1比例
        }
    }

    /// <summary>
    /// 安全执行Windows API调用，捕获可能的异常
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="action">要执行的API调用</param>
    /// <param name="defaultValue">发生异常时的默认返回值</param>
    /// <returns>API调用的结果或默认值</returns>
    public static T SafeApiCall<T>(Func<T> action, T defaultValue)
    {
        try
        {
            return action();
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    private static (double scaleX, double scaleY) GetDpiScalesForMonitor(IntPtr hMonitor)
    {
        try
        {
            if (hMonitor != IntPtr.Zero
                && GetDpiForMonitor(hMonitor, MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY) == 0)
            {
                return (dpiX / DEFAULT_DPI, dpiY / DEFAULT_DPI);
            }
        }
        catch (Exception)
        {
            // GetDpiForMonitor 在 Windows 8.1 之前不可用——继续 fallback
        }

        double s = GetSystemDpiScale();
        return (s, s);
    }

    /// <summary>
    /// 通过Windows API获取系统DPI缩放因子
    /// </summary>
    /// <returns>系统DPI缩放因子（相对于96 DPI）</returns>
    private static double GetSystemDpiScale()
    {
        try
        {
            // 尝试使用新API（Windows 8.1+）
            IntPtr desktop = GetDesktopWindow();
            IntPtr monitor = MonitorFromWindow(desktop, MONITOR_DEFAULTTOPRIMARY);

            // 获取监视器的DPI
            if (monitor != IntPtr.Zero)
            {
                int result = GetDpiForMonitor(monitor, 0, out uint dpiX, out uint dpiY);
                if (result == 0) // S_OK
                {
                    return dpiX / DEFAULT_DPI;
                }
            }

            // 如果新API失败，尝试使用旧API
            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc != IntPtr.Zero)
            {
                try
                {
                    int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
                    if (dpiX > 0)
                    {
                        return dpiX / DEFAULT_DPI;
                    }
                }
                finally
                {
                    ReleaseDC(IntPtr.Zero, hdc);
                }
            }

            // 默认返回1.0（96 DPI）
            return 1.0;
        }
        catch (Exception)
        {
            // 如果出现任何异常，返回默认值
            return 1.0;
        }
    }

    #endregion 实用方法
}