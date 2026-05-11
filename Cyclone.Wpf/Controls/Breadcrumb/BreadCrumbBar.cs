using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

public class BreadcrumbBar : ListBox
{
    static BreadcrumbBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbBar),
            new FrameworkPropertyMetadata(typeof(BreadcrumbBar)));
    }

    #region ItemClicked Event

    public static readonly RoutedEvent ItemClickedEvent = EventManager.RegisterRoutedEvent(
        nameof(ItemClicked),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(BreadcrumbBar));

    /// <summary>
    /// 用户点击某个面包屑项时触发。OriginalSource 是被点击的 <see cref="BreadcrumbBarItem"/>。
    /// </summary>
    public event RoutedEventHandler ItemClicked
    {
        add => AddHandler(ItemClickedEvent, value);
        remove => RemoveHandler(ItemClickedEvent, value);
    }

    /// <summary>
    /// 内部:由 <see cref="BreadcrumbBarItem"/> 在被点击时调用。
    /// </summary>
    internal void RaiseItemClicked(BreadcrumbBarItem item)
    {
        RaiseEvent(new RoutedEventArgs(ItemClickedEvent, item));
    }

    #endregion ItemClicked Event

    #region Override Methods

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new BreadcrumbBarItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is BreadcrumbBarItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        UpdatePositionFlags(element);
    }

    /// <summary>
    /// 集合内容变化时(增/删/重排)重新刷新所有现有容器的 IsFirst / IsLast。
    /// </summary>
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        // Add/Reset/Move 等都可能改变首末位置——全量刷新一次
        for (var i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is BreadcrumbBarItem item)
            {
                UpdatePositionFlags(item);
            }
        }
    }

    #endregion Override Methods

    #region Private Methods

    private void UpdatePositionFlags(DependencyObject element)
    {
        if (element is not BreadcrumbBarItem item)
        {
            return;
        }

        var index = ItemContainerGenerator.IndexFromContainer(element);
        item.SetIsFirst(index == 0);
        item.SetIsLast(index == Items.Count - 1);
    }

    #endregion Private Methods
}
