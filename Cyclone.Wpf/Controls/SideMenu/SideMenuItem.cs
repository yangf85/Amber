using Cyclone.Wpf.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(SideMenuItem))]
public class SideMenuItem : HeaderedItemsControl
{
    private SideMenu _root;

    private bool _isMousePressed;

    private void OnSideMenuItemLoaded(object sender, RoutedEventArgs e)
    {
        // OnApplyTemplate 时 visual tree 可能未完成——Loaded 时一定完成
        if (_root == null)
        {
            _root = VisualTreeHelperExtension.TryFindVisualParent<SideMenu>(this);
            if (_root != null)
            {
                UpdateIndent(_root.Indent);
            }
        }
    }

    static SideMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SideMenuItem),
            new FrameworkPropertyMetadata(typeof(SideMenuItem)));
    }

    public SideMenuItem()
    {
        Loaded += OnSideMenuItemLoaded;
        Focusable = true;
    }

    #region RowHeight

    public static readonly DependencyProperty RowHeightProperty =
        DependencyProperty.Register(nameof(RowHeight), typeof(double), typeof(SideMenuItem),
            new PropertyMetadata(32d));

    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    #endregion RowHeight

    #region Level — 附加只读 DP

    /// <summary>
    /// 菜单层级——0 为顶级。改为附加 DP 让子菜单可监听变化、可在 trigger 里使用。
    /// </summary>
    private static readonly DependencyPropertyKey LevelPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "Level",
            typeof(int),
            typeof(SideMenuItem),
            new PropertyMetadata(0));

    public static readonly DependencyProperty LevelProperty = LevelPropertyKey.DependencyProperty;

    public int Level => GetLevel(this);

    internal static void SetLevel(DependencyObject obj, int value) =>
        obj.SetValue(LevelPropertyKey, value);

    public static int GetLevel(DependencyObject obj) => (int)obj.GetValue(LevelProperty);

    #endregion Level — 附加只读 DP

    #region Indent

    private static readonly DependencyPropertyKey IndentPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Indent), typeof(double), typeof(SideMenuItem),
            new PropertyMetadata(0d));

    public static readonly DependencyProperty IndentProperty = IndentPropertyKey.DependencyProperty;

    public double Indent
    {
        get => (double)GetValue(IndentProperty);
        private set => SetValue(IndentPropertyKey, value);
    }

    #endregion Indent

    #region IsExpanded

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(SideMenuItem),
            new PropertyMetadata(false));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    #endregion IsExpanded

    #region IsActive — 重命名自 IsActived

    private static readonly DependencyPropertyKey IsActivePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsActive),
            typeof(bool),
            typeof(SideMenuItem),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsActiveProperty = IsActivePropertyKey.DependencyProperty;

    public bool IsActive => (bool)GetValue(IsActiveProperty);

    #endregion IsActive — 重命名自 IsActived

    #region Icon / IconTemplate

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(SideMenuItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IconTemplateProperty =
        DependencyProperty.Register(nameof(IconTemplate), typeof(DataTemplate), typeof(SideMenuItem),
            new PropertyMetadata(null));

    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public DataTemplate IconTemplate
    {
        get => (DataTemplate)GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    #endregion Icon / IconTemplate

    #region Override

    protected override DependencyObject GetContainerForItemOverride()
    {
        var item = new SideMenuItem();
        SetLevel(item, Level + 1);
        if (_root != null)
        {
            item.UpdateIndent(_root.Indent);
        }
        return item;
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is SideMenuItem;

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is SideMenuItem childItem)
        {
            // 子项 Level = 父 Level + 1
            SetLevel(childItem, Level + 1);

            if (_root != null)
            {
                childItem.UpdateIndent(_root.Indent);
                _root.ApplyIconBinding(childItem, item);
            }
        }
    }

    #endregion Override

    #region 点击处理 — 鼠标 capture pattern + 键盘支持

    /// <summary>
    /// 判断点击的 OriginalSource 是否在当前菜单项的 Header 区域（而不是子菜单的 header）。
    /// 通过向上找祖先 SideMenuItem——找到的第一个就是事件源最近的 SideMenuItem。
    /// 等于 this 时才说明点击在当前项。
    /// </summary>
    private bool IsClickOnHeader(DependencyObject source)
    {
        if (source == null)
        {
            return false;
        }

        var nearestItem = VisualTreeHelperExtension.TryFindVisualParent<SideMenuItem>(source);
        return nearestItem == this;
    }

    private void HandleClick()
    {
        // 切换展开态——HasItems 时才有意义，否则只是激活
        if (HasItems)
        {
            SetValue(IsExpandedProperty, !IsExpanded);
        }

        // 通知根菜单——根菜单负责管理激活态、触发命令和事件
        _root?.OnItemClicked(this);
    }

    /// <summary>
    /// 按下：开始追踪点击——支持鼠标按下后拖出取消。
    /// </summary>
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        // 关键：只接受 OriginalSource 在当前 SideMenuItem 的 HeaderRoot 上发生的点击
        // 子菜单项的点击会冒泡上来，要在这里过滤掉——只关心点击当前项 header 区域的情况
        if (!IsClickOnHeader(e.OriginalSource as DependencyObject))
        {
            return;
        }

        _isMousePressed = true;
        Focus();
        e.Handled = true;
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);

        if (!_isMousePressed)
        {
            return;
        }

        _isMousePressed = false;

        // 鼠标松开时如果还在 header 区域内，才算 Click
        if (IsClickOnHeader(e.OriginalSource as DependencyObject))
        {
            HandleClick();
            e.Handled = true;
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        _isMousePressed = false;
    }

    /// <summary>
    /// 键盘 Enter / Space 触发 Click（无障碍支持）。
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Enter || e.Key == Key.Space)
        {
            HandleClick();
            e.Handled = true;
        }
    }

    #endregion 点击处理 — 鼠标 capture pattern + 键盘支持

    #region 公开 API

    internal void SetActive() => SetValue(IsActivePropertyKey, true);

    internal void SetInactive() => SetValue(IsActivePropertyKey, false);

    internal void UpdateIndent(double indentSize)
    {
        Indent = Level > 0 ? Level * indentSize : 0;
    }

    #endregion 公开 API
}