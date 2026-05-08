using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// CascadePicker 的子项容器。仅承担"标头 + 子菜单展开"职责；
/// 数据查找、路径计算、索引由 <see cref="CascadePicker"/> 统一管理。
/// </summary>
public class CascadePickerItem : HeaderedItemsControl
{
    static CascadePickerItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CascadePickerItem),
            new FrameworkPropertyMetadata(typeof(CascadePickerItem)));
    }

    #region IsHighlighted

    /// <summary>
    /// 定义是否被键盘导航高亮的依赖属性。
    /// </summary>
    public static readonly DependencyProperty IsHighlightedProperty =
        DependencyProperty.Register(
            nameof(IsHighlighted),
            typeof(bool),
            typeof(CascadePickerItem),
            new PropertyMetadata(false));

    /// <summary>
    /// 获取或设置该项是否处于键盘导航高亮状态。
    /// </summary>
    public bool IsHighlighted
    {
        get => (bool)GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }

    #endregion IsHighlighted

    #region IsExpanded

    /// <summary>
    /// 定义子菜单是否展开的依赖属性。
    /// </summary>
    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(CascadePickerItem),
            new PropertyMetadata(false));

    /// <summary>
    /// 获取或设置该项的子菜单是否展开。
    /// </summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    #endregion IsExpanded

    #region IsPressed

    private static readonly DependencyPropertyKey IsPressedPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsPressed),
            typeof(bool),
            typeof(CascadePickerItem),
            new PropertyMetadata(false));

    /// <summary>
    /// 定义是否处于按下状态的只读依赖属性。
    /// </summary>
    public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;

    /// <summary>
    /// 获取该项是否处于鼠标按下状态。
    /// </summary>
    public bool IsPressed => (bool)GetValue(IsPressedProperty);

    #endregion IsPressed

    #region RoutedEvents

    /// <summary>
    /// 标签项被点击时触发的路由事件（鼠标松开且仍在元素内时触发）。
    /// </summary>
    public static readonly RoutedEvent ItemClickEvent =
        EventManager.RegisterRoutedEvent(
            nameof(ItemClick),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CascadePickerItem));

    /// <summary>
    /// 标签项被点击时触发。
    /// </summary>
    public event RoutedEventHandler ItemClick
    {
        add => AddHandler(ItemClickEvent, value);
        remove => RemoveHandler(ItemClickEvent, value);
    }

    #endregion RoutedEvents

    #region Override Methods

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item) => item is CascadePickerItem;

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride() => new CascadePickerItem();

    /// <inheritdoc />
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        // 悬停时自动展开有子项的项
        if (HasItems && !IsExpanded)
        {
            IsExpanded = true;
        }
    }

    /// <inheritdoc />
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        // 鼠标真的离开本项及其子菜单弹窗才收起
        if (!IsMouseOver && !IsMouseOverPopup)
        {
            IsExpanded = false;
        }
    }

    /// <inheritdoc />
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        SetValue(IsPressedPropertyKey, true);
        CaptureMouse();
    }

    /// <inheritdoc />
    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);

        var wasPressed = IsPressed;
        SetValue(IsPressedPropertyKey, false);
        ReleaseMouseCapture();

        // 鼠标松开时仍在元素内才视为点击
        if (wasPressed && IsMouseOver)
        {
            RaiseEvent(new RoutedEventArgs(ItemClickEvent, this));
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        SetValue(IsPressedPropertyKey, false);
    }

    #endregion Override Methods

    #region Private Methods

    /// <summary>
    /// 鼠标是否处于本项的子菜单 Popup 内（用于判定是否应该收起）。
    /// </summary>
    private bool IsMouseOverPopup
    {
        get
        {
            // 子项菜单逻辑上是本控件的逻辑后代；遍历每个子容器看 IsMouseOver
            for (var i = 0; i < Items.Count; i++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(i) is CascadePickerItem child
                    && child.IsMouseOver)
                {
                    return true;
                }
            }
            return false;
        }
    }

    #endregion Private Methods
}