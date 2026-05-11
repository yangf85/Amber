using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 根据 Content 类型选择 IconBox 的渲染模板。
/// <list type="bullet">
/// <item><description><see cref="Geometry"/> → <see cref="PathTemplate"/></description></item>
/// <item><description><see cref="ImageSource"/> → <see cref="ImageTemplate"/></description></item>
/// <item><description><see cref="string"/> → <see cref="FontTemplate"/> (字体图标 Glyph)</description></item>
/// </list>
/// </summary>
public class IconTemplateSelector : DataTemplateSelector
{
    /// <summary>字体图标模板 (string Glyph)。</summary>
    public DataTemplate FontTemplate { get; set; }

    /// <summary>位图图标模板 (ImageSource)。</summary>
    public DataTemplate ImageTemplate { get; set; }

    /// <summary>路径图标模板 (Geometry)。</summary>
    public DataTemplate PathTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            Geometry => PathTemplate,
            ImageSource => ImageTemplate,
            string => FontTemplate,
            _ => base.SelectTemplate(item, container),
        };
    }
}