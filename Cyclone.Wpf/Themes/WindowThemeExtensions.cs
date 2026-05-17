using System;
using System.Windows;

namespace Cyclone.Wpf.Themes;

/// <summary>
/// Window 主题挂接扩展。
/// 顶级 Window 在构造函数里调一次,自动注册到 ThemeManager,关闭时自动取消注册。
/// </summary>
public static class WindowThemeExtensions
{
    /// <summary>
    /// 把 Window 的 Resources 注册为 ThemeManager 的 host:
    /// <list type="bullet">
    /// <item>当前主题立即 merge 到 Window.Resources,模板 DynamicResource 通过 Window 层就能解析。</item>
    /// <item>ThemeManager 切换主题时,本窗口自动跟随。</item>
    /// <item>Window 关闭时自动从 ThemeManager 取消注册,不留悬挂引用。</item>
    /// </list>
    /// 不依赖 Application.Resources,在 Rhino / AutoCAD / WinForms ElementHost 等非标准宿主下也能正常工作。
    /// </summary>
    public static void AttachThemeManager(this Window window)
    {
        if (window == null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        ThemeManager.RegisterHost(window.Resources);
        window.Closed += OnWindowClosed;
    }

    private static void OnWindowClosed(object sender, EventArgs e)
    {
        if (sender is Window window)
        {
            ThemeManager.UnregisterHost(window.Resources);
        }
    }
}