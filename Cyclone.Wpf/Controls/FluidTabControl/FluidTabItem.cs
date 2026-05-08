using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 流式标签控件的标签项。继承自 <see cref="HeaderedContentControl"/>：
/// Header 在 Tab 头列表中渲染；Content 不出现在 FluidTabItem 自身的视觉树里，
/// 而是由 <see cref="FluidTabControl"/> 在内容滚动区独立渲染，避免单个对象被多父级持有。
/// </summary>
public class FluidTabItem : HeaderedContentControl
{
    static FluidTabItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FluidTabItem),
            new FrameworkPropertyMetadata(typeof(FluidTabItem)));
    }

    #region Icon

    /// <summary>
    /// 定义图标内容的依赖属性。
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(FluidTabItem),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置标签项左侧的图标内容。
    /// </summary>
    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion Icon

    #region IconTemplate

    /// <summary>
    /// 定义图标模板的依赖属性。
    /// </summary>
    public static readonly DependencyProperty IconTemplateProperty =
        DependencyProperty.Register(
            nameof(IconTemplate),
            typeof(DataTemplate),
            typeof(FluidTabItem),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置图标模板。
    /// </summary>
    public DataTemplate IconTemplate
    {
        get => (DataTemplate)GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    #endregion IconTemplate

    #region IsSelected

    /// <summary>
    /// 定义是否选中的依赖属性（来自 <see cref="Selector.IsSelectedProperty"/>）。
    /// </summary>
    public static readonly DependencyProperty IsSelectedProperty =
        Selector.IsSelectedProperty.AddOwner(
            typeof(FluidTabItem),
            new FrameworkPropertyMetadata(false, OnIsSelectedChanged));

    /// <summary>
    /// 获取或设置标签项是否被选中。
    /// </summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FluidTabItem item)
        {
            return;
        }

        var routed = (bool)e.NewValue ? Selector.SelectedEvent : Selector.UnselectedEvent;
        item.RaiseEvent(new RoutedEventArgs(routed, item));
    }

    #endregion IsSelected

    #region Override Methods

    /// <inheritdoc />
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

    #endregion Override Methods
}