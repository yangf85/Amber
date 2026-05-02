using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// <see cref="Card"/> 的标准底部容器。
/// <para>
/// 是一个不带新增 DP 的 <see cref="ContentControl"/>，主要价值是提供与 Card 视觉协调的默认 Style
/// （字号、颜色、内边距），让用户写 Footer 时省去这些细节调整：
/// </para>
/// <code>
/// &lt;cy:Card.Footer&gt;
///     &lt;cy:CardFooter&gt;
///         &lt;StackPanel Orientation="Horizontal" HorizontalAlignment="Right"&gt;
///             &lt;Button Content="详情"/&gt;
///             &lt;Button Content="操作"/&gt;
///         &lt;/StackPanel&gt;
///     &lt;/cy:CardFooter&gt;
/// &lt;/cy:Card.Footer&gt;
/// </code>
/// <para>
/// Footer 内具体放什么由用户决定（一组按钮 / 元数据信息 / 链接 / 任意排版），CardFooter 只负责统一外观基线。
/// </para>
/// </summary>
public class CardFooter : ContentControl
{
    static CardFooter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CardFooter),
            new FrameworkPropertyMetadata(typeof(CardFooter)));
    }
}
