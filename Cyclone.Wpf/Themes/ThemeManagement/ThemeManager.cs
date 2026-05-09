using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Controls;

namespace Cyclone.Wpf.Themes;

/// <summary>
/// 主题管理 在xaml中使用，要在资源的最后加载，否则无效
/// </summary>
public static class ThemeManager
{
    private static readonly List<Theme> _themes = [];

    private static Theme _currentTheme;

    public static IReadOnlyList<Theme> AvailableThemes => _themes;

    public static Theme CurrentTheme
    {
        get => _currentTheme;

        set
        {
            if (_currentTheme == value) return;

            // 移除旧主题
            if (_currentTheme != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(_currentTheme);
            }

            // 添加新主题
            _currentTheme = value;
            Application.Current.Resources.MergedDictionaries.Add(_currentTheme);

            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static void RegisterTheme(Theme theme)
    {
        _themes.Add(theme);
    }

    public static event EventHandler ThemeChanged;

    static ThemeManager()
    {
        // 注册默认主题
        RegisterTheme(new BasicTheme());
        RegisterTheme(new LightTheme());
        RegisterTheme(new DarkTheme());
        CurrentTheme = AvailableThemes[0];
    }
}