using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 统一的图标控件。根据 Content 类型自动渲染:
/// <list type="bullet">
/// <item><description><see cref="System.Windows.Media.Geometry"/> → 矢量 Path</description></item>
/// <item><description><see cref="System.Windows.Media.ImageSource"/> → Image</description></item>
/// <item><description><see cref="string"/> → 字体图标 Glyph (用 FontFamily + Text)</description></item>
/// </list>
/// 默认 16×16,可通过 Width/Height 或 FontSize 调整大小。
/// <para>
/// 注:Path Data 字符串 (如 "M12,5 L12,19") 也会被识别为 string 类型,
/// 渲染为 Glyph (乱码)。如要传 Path Data,请显式构造 Geometry:
/// <code>Content="{x:Static Geometry.Parse...}"</code> 或在 XAML 中用 GeometryConverter。
/// </para>
/// </summary>
public sealed class IconBox : ContentControl
{
    static IconBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IconBox),
            new FrameworkPropertyMetadata(typeof(IconBox)));

        // 默认 16×16,跟 Win10/WinUI 标准图标尺寸一致
        WidthProperty.OverrideMetadata(typeof(IconBox),
            new FrameworkPropertyMetadata(16.0));
        HeightProperty.OverrideMetadata(typeof(IconBox),
            new FrameworkPropertyMetadata(16.0));
    }
}