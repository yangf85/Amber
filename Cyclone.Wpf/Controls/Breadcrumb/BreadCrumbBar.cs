using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public class BreadCrumbBar : ListBox
{
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is BreadCrumbBarItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new BreadCrumbBarItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is BreadCrumbBarItem breadCrumbItem)
        {
            int index = ItemContainerGenerator.IndexFromContainer(element);
            breadCrumbItem.IsFirst = (index == 0);
            breadCrumbItem.IsLast = (index == Items.Count - 1);
        }
    }

    static BreadCrumbBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadCrumbBar), new FrameworkPropertyMetadata(typeof(BreadCrumbBar)));
    }
}