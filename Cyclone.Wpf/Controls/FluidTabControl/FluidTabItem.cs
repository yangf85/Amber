using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 流式标签控件的标签项。
/// <para>
/// **重要设计决策**:继承 <see cref="Control"/> 而不是 <see cref="HeaderedContentControl"/> 或 <see cref="ContentControl"/>。
/// 原因:ContentControl 会把 Content 当 logical child,而 FluidTabControl 把同一 Content 显示在另一处
/// (内容滚动区),会导致 logical tree 冲突和视觉元素连接错误。
/// 因此自己声明 ContentProperty 而不复用 ContentControl 的实现。
/// </para>
/// </summary>
[ContentProperty(nameof(Content))]
public class FluidTabItem : Control
{
    static FluidTabItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FluidTabItem),
            new FrameworkPropertyMetadata(typeof(FluidTabItem)));
    }

    #region Content

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(
            nameof(Content),
            typeof(object),
            typeof(FluidTabItem),
            new PropertyMetadata(null));

    /// <summary>该 Tab 项的内容,由 FluidTabControl 在内容滚动区独立渲染。</summary>
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    #endregion Content

    #region ContentTemplate

    public static readonly DependencyProperty ContentTemplateProperty =
        DependencyProperty.Register(
            nameof(ContentTemplate),
            typeof(DataTemplate),
            typeof(FluidTabItem),
            new PropertyMetadata(null));

    public DataTemplate ContentTemplate
    {
        get => (DataTemplate)GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    #endregion ContentTemplate

    #region ContentTemplateSelector

    public static readonly DependencyProperty ContentTemplateSelectorProperty =
        DependencyProperty.Register(
            nameof(ContentTemplateSelector),
            typeof(DataTemplateSelector),
            typeof(FluidTabItem),
            new PropertyMetadata(null));

    public DataTemplateSelector ContentTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(ContentTemplateSelectorProperty);
        set => SetValue(ContentTemplateSelectorProperty, value);
    }

    #endregion ContentTemplateSelector

    #region Header

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(FluidTabItem),
            new PropertyMetadata(null));

    /// <summary>该 Tab 项左侧列表中显示的 Header。</summary>
    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    #endregion Header

    #region HeaderTemplate

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(
            nameof(HeaderTemplate),
            typeof(DataTemplate),
            typeof(FluidTabItem),
            new PropertyMetadata(null));

    public DataTemplate HeaderTemplate
    {
        get => (DataTemplate)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    #endregion HeaderTemplate

    #region HeaderTemplateSelector

    public static readonly DependencyProperty HeaderTemplateSelectorProperty =
        DependencyProperty.Register(
            nameof(HeaderTemplateSelector),
            typeof(DataTemplateSelector),
            typeof(FluidTabItem),
            new PropertyMetadata(null));

    public DataTemplateSelector HeaderTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(HeaderTemplateSelectorProperty);
        set => SetValue(HeaderTemplateSelectorProperty, value);
    }

    #endregion HeaderTemplateSelector

    #region Icon

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(FluidTabItem),
            new PropertyMetadata(null));

    /// <summary>该 Tab 项左侧的图标内容。</summary>
    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion Icon

    #region IconTemplate

    public static readonly DependencyProperty IconTemplateProperty =
        DependencyProperty.Register(
            nameof(IconTemplate),
            typeof(DataTemplate),
            typeof(FluidTabItem),
            new PropertyMetadata(null));

    public DataTemplate IconTemplate
    {
        get => (DataTemplate)GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    #endregion IconTemplate

    #region IsSelected

    public static readonly DependencyProperty IsSelectedProperty =
        Selector.IsSelectedProperty.AddOwner(
            typeof(FluidTabItem),
            new FrameworkPropertyMetadata(false, OnIsSelectedChanged));

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FluidTabItem item) return;
        var routed = (bool)e.NewValue ? Selector.SelectedEvent : Selector.UnselectedEvent;
        item.RaiseEvent(new RoutedEventArgs(routed, item));
    }

    #endregion IsSelected

    #region Override

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (ItemsControl.ItemsControlFromItemContainer(this) is not FluidTabControl tab)
        {
            return;
        }

        var dataItem = tab.ItemContainerGenerator.ItemFromContainer(this);
        tab.SelectedItem = dataItem != DependencyProperty.UnsetValue ? dataItem : this;
    }

    #endregion Override
}