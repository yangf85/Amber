using System;

namespace Cyclone.Wpf.Themes;

/// <summary>基础亮色主题。</summary>
public sealed class BasicTheme : Theme
{
    public override string Name => nameof(BasicTheme);

    public BasicTheme()
    {
        Source = new Uri(
            "pack://application:,,,/Cyclone.Wpf;component/Themes/BasicTheme.xaml",
            UriKind.Absolute);
    }
}