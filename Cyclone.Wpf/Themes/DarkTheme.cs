using System;

namespace Cyclone.Wpf.Themes;

/// <summary>暗色主题。</summary>
public sealed class DarkTheme : Theme
{
    public override string Name => nameof(DarkTheme);

    public DarkTheme()
    {
        Source = new Uri(
            "pack://application:,,,/Cyclone.Wpf;component/Themes/DarkTheme.xaml",
            UriKind.Absolute);
    }
}