using System;

namespace Cyclone.Wpf.Themes;

/// <summary>
/// <see cref="ThemeManager.ThemeChanged"/> 事件参数——携带前后主题，方便 subscribers 做差异化处理。
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    public Theme OldTheme { get; }

    public Theme NewTheme { get; }

    public ThemeChangedEventArgs(Theme oldTheme, Theme newTheme)
    {
        OldTheme = oldTheme;
        NewTheme = newTheme;
    }
}