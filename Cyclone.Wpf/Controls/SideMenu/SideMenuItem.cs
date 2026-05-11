using Cyclone.Wpf.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(SideMenuItem))]
[TemplatePart(Name = nameof(PART_HeaderRoot), Type = typeof(FrameworkElement))]
public class SideMenuItem : HeaderedItemsControl
{
    private const string PART_HeaderRoot = "PART_HeaderRoot";

    private FrameworkElement _headerRoot;

    private bool _isMousePressed;

    private SideMenu _root;

    static SideMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SideMenuItem),
            new FrameworkPropertyMetadata(typeof(SideMenuItem)));

        // 只读 DP 在静态构造函数里显式按序初始化——避免依赖字段声明顺序,
        // 即使代码整理工具重排字段也不会出现 NRE

        // Level (附加只读)
        LevelPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "Level",
            typeof(int),
            typeof(SideMenuItem),
            new PropertyMetadata(0));
        LevelProperty = LevelPropertyKey.DependencyProperty;

        // Indent (只读)
        IndentPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Indent),
            typeof(double),
            typeof(SideMenuItem),
            new PropertyMetadata(0d));
        IndentProperty = IndentPropertyKey.DependencyProperty;

        // IsActive (只读)
        IsActivePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsActive),
            typeof(bool),
            typeof(SideMenuItem),
            new PropertyMetadata(false));
        IsActiveProperty = IsActivePropertyKey.DependencyProperty;
    }

    public SideMenuItem()
    {
        Loaded += OnLoaded;
        Focusable = true;
    }

    #region RowHeight

    public static readonly DependencyProperty RowHeightProperty =
        DependencyProperty.Register(
            nameof(RowHeight),
            typeof(double),
            typeof(SideMenuItem),
            new PropertyMetadata(32d));

    /// <summary>
    /// 获取或设置 Header 行的高度。
    /// </summary>
    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    #endregion RowHeight

    #region Level

    public static readonly DependencyProperty LevelProperty;

    /// <summary>
    /// 菜单层级——0 为顶级。附加只读 DP,让子菜单可监听变化、可在 trigger 里使用。
    /// 初始化见静态构造函数。
    /// </summary>
    private static readonly DependencyPropertyKey LevelPropertyKey;

    /// <summary>
    /// 获取当前菜单项的层级。
    /// </summary>
    public int Level => GetLevel(this);

    public static int GetLevel(DependencyObject obj)
    {
        return (int)obj.GetValue(LevelProperty);
    }

    internal static void SetLevel(DependencyObject obj, int value)
    {
        obj.SetValue(LevelPropertyKey, value);
    }

    #endregion Level

    #region Indent

    public static readonly DependencyProperty IndentProperty;

    /// <summary>
    /// 缩进 (Level * SideMenu.Indent),只读 DP——初始化见静态构造函数。
    /// </summary>
    private static readonly DependencyPropertyKey IndentPropertyKey;

    /// <summary>
    /// 获取根据 Level 和 SideMenu.Indent 计算出的缩进值。
    /// </summary>
    public double Indent
    {
        get => (double)GetValue(IndentProperty);
        private set => SetValue(IndentPropertyKey, value);
    }

    #endregion Indent

    #region IsExpanded

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(SideMenuItem),
            new PropertyMetadata(false));

    /// <summary>
    /// 获取或设置当前菜单项是否展开子项。
    /// </summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    #endregion IsExpanded

    #region IsActive

    public static readonly DependencyProperty IsActiveProperty;

    /// <summary>
    /// 激活态,只读 DP——初始化见静态构造函数。
    /// </summary>
    private static readonly DependencyPropertyKey IsActivePropertyKey;

    /// <summary>
    /// 获取当前菜单项是否处于激活态(被点击或位于激活链上)。
    /// </summary>
    public bool IsActive => (bool)GetValue(IsActiveProperty);

    #endregion IsActive

    #region Icon

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(SideMenuItem),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置菜单项图标内容。
    /// </summary>
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
            typeof(SideMenuItem),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置 Icon 的 DataTemplate。
    /// </summary>
    public DataTemplate IconTemplate
    {
        get => (DataTemplate)GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    #endregion IconTemplate

    #region Override Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 旧 HeaderRoot 解绑——支持模板重载场景
        _headerRoot?.MouseLeftButtonDown -= OnHeaderRootMouseLeftButtonDown;
        _headerRoot?.MouseLeftButtonUp -= OnHeaderRootMouseLeftButtonUp;
        _headerRoot?.MouseLeave -= OnHeaderRootMouseLeave;

        _headerRoot = GetTemplateChild(PART_HeaderRoot) as FrameworkElement;

        // 挂到 HeaderRoot 的冒泡事件——子控件(Button、Hyperlink 等)若 e.Handled=true,
        // 事件不会冒泡到这里,从而避免影响 Header 内部的可交互元素
        _headerRoot?.MouseLeftButtonDown += OnHeaderRootMouseLeftButtonDown;
        _headerRoot?.MouseLeftButtonUp += OnHeaderRootMouseLeftButtonUp;
        _headerRoot?.MouseLeave += OnHeaderRootMouseLeave;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        var item = new SideMenuItem();
        SetLevel(item, Level + 1);
        if (_root is not null)
        {
            item.UpdateIndent(_root.Indent);
        }
        return item;
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is SideMenuItem;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Enter:
            case Key.Space:
                HandleClick();
                e.Handled = true;
                break;

            case Key.Right:

                // 右键——未展开则展开,已展开则进入第一个子项
                if (HasItems)
                {
                    if (!IsExpanded)
                    {
                        IsExpanded = true;
                    }
                    else
                    {
                        MoveFocusToFirstChild();
                    }
                    e.Handled = true;
                }
                break;

            case Key.Left:

                // 左键——已展开则折叠,否则跳到父项
                if (IsExpanded)
                {
                    IsExpanded = false;
                    e.Handled = true;
                }
                else if (MoveFocusToParent())
                {
                    e.Handled = true;
                }
                break;

            case Key.Down:
                if (MoveFocusToSibling(1))
                {
                    e.Handled = true;
                }
                break;

            case Key.Up:
                if (MoveFocusToSibling(-1))
                {
                    e.Handled = true;
                }
                break;
        }
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is SideMenuItem childItem)
        {
            // 子项 Level = 父 Level + 1
            SetLevel(childItem, Level + 1);

            if (_root is not null)
            {
                childItem.UpdateIndent(_root.Indent);
                _root.ApplyIconBinding(childItem, item);
            }
        }
    }

    #endregion Override Methods

    #region Private Methods

    private void HandleClick()
    {
        // 切换展开态——HasItems 时才有意义,否则只是激活
        if (HasItems)
        {
            SetValue(IsExpandedProperty, !IsExpanded);
        }

        // 通知根菜单——根菜单负责管理激活态、触发命令和事件
        _root?.OnItemClicked(this);
    }

    /// <summary>
    /// 判断点击的 OriginalSource 是否在当前菜单项的 Header 区域(而不是子菜单的 header)。
    /// 通过向上找祖先 SideMenuItem——找到的第一个就是事件源最近的 SideMenuItem。
    /// 等于 this 时才说明点击在当前项。
    /// </summary>
    private bool IsClickOnHeader(DependencyObject source)
    {
        if (source is null)
        {
            return false;
        }

        var nearestItem = VisualTreeHelperExtension.TryFindVisualParent<SideMenuItem>(source);
        return nearestItem == this;
    }

    private void MoveFocusToFirstChild()
    {
        if (Items.Count == 0)
        {
            return;
        }

        if (ItemContainerGenerator.ContainerFromIndex(0) is SideMenuItem firstChild)
        {
            firstChild.Focus();
        }
    }

    private bool MoveFocusToParent()
    {
        if (ItemsControl.ItemsControlFromItemContainer(this) is SideMenuItem parent)
        {
            return parent.Focus();
        }
        return false;
    }

    private bool MoveFocusToSibling(int delta)
    {
        var parent = ItemsControl.ItemsControlFromItemContainer(this);
        if (parent is null)
        {
            return false;
        }

        var index = parent.ItemContainerGenerator.IndexFromContainer(this);
        if (index < 0)
        {
            return false;
        }

        var targetIndex = index + delta;
        if (targetIndex < 0 || targetIndex >= parent.Items.Count)
        {
            return false;
        }

        if (parent.ItemContainerGenerator.ContainerFromIndex(targetIndex) is SideMenuItem sibling)
        {
            return sibling.Focus();
        }
        return false;
    }

    private void OnHeaderRootMouseLeave(object sender, MouseEventArgs e)
    {
        // 鼠标拖出 HeaderRoot——取消点击
        _isMousePressed = false;
    }

    private void OnHeaderRootMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 子菜单项的 HeaderRoot 触发后冒泡上来,此时 OriginalSource 不在当前 HeaderRoot 子树内,过滤掉
        if (!IsClickOnHeader(e.OriginalSource as DependencyObject))
        {
            return;
        }

        _isMousePressed = true;
        Focus();
        e.Handled = true;
    }

    private void OnHeaderRootMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isMousePressed)
        {
            return;
        }

        _isMousePressed = false;

        // 鼠标松开时如果还在 header 区域内,才算 Click
        if (IsClickOnHeader(e.OriginalSource as DependencyObject))
        {
            HandleClick();
            e.Handled = true;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // OnApplyTemplate 时 visual tree 可能未完成——Loaded 时一定完成
        if (_root is not null)
        {
            return;
        }

        _root = VisualTreeHelperExtension.TryFindVisualParent<SideMenu>(this);
        if (_root is null)
        {
            return;
        }

        // 数据驱动场景下,PrepareContainerForItemOverride 可能在 _root 找到之前调用,
        // 这里补救缩进和图标绑定
        UpdateIndent(_root.Indent);
        _root.ApplyIconBinding(this, DataContext);
    }

    #endregion Private Methods

    #region Internal API

    internal void SetActive()
    {
        SetValue(IsActivePropertyKey, true);
    }

    internal void SetInactive()
    {
        SetValue(IsActivePropertyKey, false);
    }

    internal void UpdateIndent(double indentSize)
    {
        Indent = Level > 0 ? Level * indentSize : 0;
    }

    #endregion Internal API
}