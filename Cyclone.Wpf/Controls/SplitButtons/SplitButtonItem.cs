using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// <see cref="SplitButton"/> 下拉菜单中的单项。继承 <see cref="ButtonBase"/>,
/// 天然支持 <see cref="ButtonBase.Command"/> / <see cref="ButtonBase.CommandParameter"/> /
/// <see cref="ButtonBase.Click"/>——每个菜单项可以绑定独立的命令。
/// <para>点击后通知 root <see cref="SplitButton"/> 关闭 popup 并冒泡 ItemClick,再执行自身 Command。</para>
/// </summary>
public class SplitButtonItem : ButtonBase
{
    #region Constructors

    static SplitButtonItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SplitButtonItem),
            new FrameworkPropertyMetadata(typeof(SplitButtonItem)));
    }

    #endregion Constructors

    #region Icon

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(SplitButtonItem),
            new PropertyMetadata(default(object)));

    /// <summary>菜单项左侧的图标(可选)。可以是字符串、Path、Image 或任意 UIElement。</summary>
    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion Icon

    #region Override Methods

    protected override void OnClick()
    {
        // 先通知 root SplitButton 关闭 popup —— 这样即使下面 Command 执行抛异常,UI 状态也已恢复
        var root = ItemsControl.ItemsControlFromItemContainer(this) as SplitButton;
        root?.NotifyItemClicked(this);

        // ButtonBase.OnClick 会:1) Execute Command;2) raise Click 路由事件
        base.OnClick();
    }

    #endregion Override Methods
}