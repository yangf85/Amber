using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Cyclone.Wpf.Themes;

/// <summary>
/// 主题管理器——管理已注册主题、运行时切换主题。支持多 host 场景（WPF 主程序 / WinForms ElementHost / Win32 HwndSource / MFC 嵌入等）。
/// <para>
/// <b>WPF 主程序场景</b>（最常见）：
/// </para>
/// <code>
/// // App.xaml.cs:
/// protected override void OnStartup(StartupEventArgs e)
/// {
///     base.OnStartup(e);
///     ThemeManager.Initialize();   // 自动注册 Application.Current.Resources 为 host
/// }
///
/// ThemeManager.SwitchTo("DarkTheme");
/// </code>
/// <para>
/// <b>二次开发场景</b>（WinForms / Win32 / MFC 嵌入 WPF，没有 WPF Application）：
/// </para>
/// <code>
/// // WinForms 用 ElementHost 嵌入 WPF UserControl:
/// var elementHost = new ElementHost();
/// var wpfControl = new MyWpfUserControl();
/// elementHost.Child = wpfControl;
/// ThemeManager.RegisterHost(wpfControl.Resources);   // 显式注册 host
/// ThemeManager.SwitchTo("DarkTheme");
/// </code>
/// <para>
/// <b>注意事项</b>：
/// </para>
/// <list type="bullet">
/// <item>不要在控件 XAML 中 merge 主题字典——会跟 ThemeManager 维护的实例不一致导致切换失效。</item>
/// <item><see cref="ThemeChanged"/> 是静态事件——subscriber 必须在销毁时取消订阅，否则永久持有引用。</item>
/// <item>多数场景不需要订阅 <see cref="ThemeChanged"/>——DynamicResource 会自动更新颜色字体。</item>
/// </list>
/// </summary>
public static class ThemeManager
{
    private static readonly List<Theme> _themes = new();

    private static readonly List<ResourceDictionary> _hosts = new();

    private static Theme _currentTheme;

    /// <summary>已注册的主题列表（只读）。</summary>
    public static IReadOnlyList<Theme> AvailableThemes => _themes;

    /// <summary>已注册的主题宿主列表（只读）——主题字典会被 merge 到每个 host 的 MergedDictionaries。</summary>
    public static IReadOnlyList<ResourceDictionary> Hosts => _hosts;

    /// <summary>
    /// 当前主题。
    /// <para>
    /// 设置时遍历所有已注册 host，对每个 host 先 <c>Insert(0)</c> 新主题再 <c>Remove</c> 旧主题——
    /// 保证 MergedDictionaries 始终有可用主题，避免 DynamicResource 在中间一帧 fallback 到默认值。
    /// Insert 到位置 0 让 user 自定义字典在末尾能覆盖主题资源。
    /// </para>
    /// </summary>
    /// <exception cref="ArgumentNullException">value 为 null（避免应用失去所有主题资源）。</exception>
    public static Theme CurrentTheme
    {
        get => _currentTheme;

        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value),
                    "CurrentTheme 不允许为 null——会导致应用失去所有主题资源。");
            }

            if (ReferenceEquals(_currentTheme, value))
            {
                return;
            }

            var oldTheme = _currentTheme;
            _currentTheme = value;

            // 遍历所有 host，对每个 host 先 Insert 新再 Remove 旧
            foreach (var host in _hosts)
            {
                ApplyThemeToHost(host, oldTheme, value);
            }

            ThemeChanged?.Invoke(typeof(ThemeManager),
                new ThemeChangedEventArgs(oldTheme, value));
        }
    }

    /// <summary>
    /// 默认初始化——如果 <see cref="Application.Current"/> 存在，自动注册其 Resources 为 host。
    /// 在 <see cref="Application"/> 不存在的二次开发场景（WinForms / Win32 / MFC 嵌入），
    /// 用 <see cref="RegisterHost"/> 显式传入 ResourceDictionary。
    /// 多次调用安全（同一 ResourceDictionary 重复注册会被忽略）。
    /// </summary>
    public static void Initialize()
    {
        var appResources = Application.Current?.Resources;
        if (appResources != null)
        {
            RegisterHost(appResources);
        }
    }

    /// <summary>
    /// 注册主题宿主。注册时立即把当前主题 merge 到该 host 的 MergedDictionaries。
    /// 同一 ResourceDictionary 重复注册会被忽略。
    /// </summary>
    public static void RegisterHost(ResourceDictionary host)
    {
        if (host == null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        if (_hosts.Contains(host))
        {
            return;
        }

        _hosts.Add(host);

        // 立即把当前主题 merge 到新 host
        if (_currentTheme != null && !host.MergedDictionaries.Contains(_currentTheme))
        {
            host.MergedDictionaries.Insert(0, _currentTheme);
        }
    }

    /// <summary>取消注册主题宿主。会从该 host 的 MergedDictionaries 移除当前主题字典。</summary>
    public static void UnregisterHost(ResourceDictionary host)
    {
        if (host == null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        if (!_hosts.Remove(host))
        {
            return;
        }

        if (_currentTheme != null)
        {
            host.MergedDictionaries.Remove(_currentTheme);
        }
    }

    /// <summary>注册主题。同 Type 重复注册会被忽略。</summary>
    public static void RegisterTheme(Theme theme)
    {
        if (theme == null)
        {
            throw new ArgumentNullException(nameof(theme));
        }

        var newType = theme.GetType();
        if (_themes.Any(t => t.GetType() == newType))
        {
            return;
        }

        _themes.Add(theme);
    }

    /// <summary>
    /// 按名称切换主题。匹配规则：忽略大小写的完全匹配。
    /// </summary>
    /// <returns>true 切换成功；false 找不到匹配主题。</returns>
    public static bool SwitchTo(string themeName)
    {
        if (string.IsNullOrEmpty(themeName))
        {
            return false;
        }

        var theme = _themes.FirstOrDefault(t =>
            string.Equals(t.Name, themeName, StringComparison.OrdinalIgnoreCase));

        if (theme == null)
        {
            return false;
        }

        CurrentTheme = theme;
        return true;
    }

    /// <summary>
    /// 在指定 host 上执行主题切换：先 Insert 新（位置 0）再 Remove 旧——
    /// MergedDictionaries 始终至少包含一个主题，避免 DynamicResource 中间一帧无法 resolve。
    /// </summary>
    private static void ApplyThemeToHost(ResourceDictionary host, Theme oldTheme, Theme newTheme)
    {
        var dictionaries = host.MergedDictionaries;

        if (!dictionaries.Contains(newTheme))
        {
            dictionaries.Insert(0, newTheme);
        }

        if (oldTheme != null && !ReferenceEquals(oldTheme, newTheme))
        {
            dictionaries.Remove(oldTheme);
        }
    }

    /// <summary>
    /// 主题改变后触发。<br/>
    /// <b>警告</b>：静态事件——subscriber 必须在销毁时取消订阅，否则会永久持有引用导致内存泄漏。
    /// 多数场景下不需要订阅此事件——DynamicResource 自动更新颜色字体。
    /// </summary>
    public static event EventHandler<ThemeChangedEventArgs> ThemeChanged;

    static ThemeManager()
    {
        RegisterTheme(new BasicTheme());
        RegisterTheme(new DarkTheme());
        _currentTheme = _themes[0];
    }
}