using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 通用卡片容器：一个带可选 Header / Footer 槽位的内容控件。
/// <para>
/// 继承自 <see cref="HeaderedContentControl"/>，因此 Header / HeaderTemplate / Content / ContentTemplate
/// 等成员均来自基类，无需重新声明。新增 <see cref="Footer"/> 一个槽位与 <see cref="SeparatorVisibility"/> 一个外观开关。
/// </para>
/// <para>
/// 推荐用法是把 <see cref="CardHeader"/> 放进 Header 槽、<see cref="CardFooter"/> 放进 Footer 槽，
/// 但两个槽位都接受任意 object，用户可以放任何想放的内容。
/// </para>
/// <para>
/// 如果需要"整张卡片可点击"的语义，请把 Card 包在 <c>Button</c> 里——这是 WPF 的标准做法，
/// 也是把"行为（可点击）"和"布局（卡片）"职责分开的体现。
/// </para>
/// </summary>
public class Card : HeaderedContentControl
{
    static Card()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Card),
            new FrameworkPropertyMetadata(typeof(Card)));
    }

    #region Footer

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(
            nameof(Footer),
            typeof(object),
            typeof(Card),
            new FrameworkPropertyMetadata(default(object)));

    /// <summary>
    /// 卡片底部内容槽。为 null 时模板会收起整个 Footer 区域（含分隔线和内边距）。
    /// </summary>
    public object Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    #endregion Footer

    #region SeparatorVisibility

    public static readonly DependencyProperty SeparatorVisibilityProperty =
        DependencyProperty.Register(
            nameof(SeparatorVisibility),
            typeof(CardSeparatorVisibility),
            typeof(Card),
            new FrameworkPropertyMetadata(CardSeparatorVisibility.Both));

    /// <summary>
    /// Header / Footer 区域的分隔线可见性配置。默认 <see cref="CardSeparatorVisibility.Both"/>。
    /// </summary>
    public CardSeparatorVisibility SeparatorVisibility
    {
        get => (CardSeparatorVisibility)GetValue(SeparatorVisibilityProperty);
        set => SetValue(SeparatorVisibilityProperty, value);
    }

    #endregion SeparatorVisibility
}
