using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

public class SideMenu : ItemsControl
{
    /// <summary>
    /// 当前菜单宽度（用于动画）——不直接污染 Width，让用户能在外层布局里正常使用 SideMenu。
    /// 模板内的 RootBorder 通过 TemplateBinding 绑定该值。
    ///
    /// 注意：理论上应该是只读 DP，但 WPF 只读 DP 的元数据默认 IsAnimationProhibited=true
    /// 无法走 BeginAnimation——只能改成普通 DP。约定外部不要直接赋值（用 IsCompact 切换驱动）。
    /// </summary>
    public static readonly DependencyProperty MenuWidthProperty =
        DependencyProperty.Register(
            nameof(MenuWidth),
            typeof(double),
            typeof(SideMenu),
            new PropertyMetadata(150d));

    public double MenuWidth
    {
        get => (double)GetValue(MenuWidthProperty);
        private set => SetValue(MenuWidthProperty, value);
    }

    static SideMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SideMenu), new FrameworkPropertyMetadata(typeof(SideMenu)));
    }

    #region IsCompact

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(SideMenu),
            new PropertyMetadata(false, OnIsCompactChanged));

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    private static void OnIsCompactChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenu menu)
        {
            menu.AnimateWidthChange((bool)e.NewValue);
        }
    }

    /// <summary>
    /// 宽度切换动画——通过 SetValue + Storyboard 改 MenuWidth，不污染外层 Width 属性。
    /// </summary>
    private void AnimateWidthChange(bool isCompact)
    {
        var targetWidth = isCompact ? CollapseWidth : ExpansionWidth;
        var currentWidth = MenuWidth;

        // 模板还没应用 / 首次进入：直接设值不走动画
        if (!_isInitialized || currentWidth == 0)
        {
            SetValue(MenuWidthProperty, targetWidth);
            return;
        }

        var animation = new DoubleAnimation
        {
            From = currentWidth,
            To = targetWidth,
            Duration = AnimationDuration,
            FillBehavior = FillBehavior.Stop, // 关键：动画结束后释放控制权，让 SetValue 设的值生效
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
        };

        animation.Completed += (_, _) =>
        {
            // 动画结束后写入 DP 真实值（不依赖 HoldEnd 占着 effective value）
            SetValue(MenuWidthProperty, targetWidth);
        };

        BeginAnimation(MenuWidthProperty, animation);
    }

    #endregion IsCompact

    #region IsShowOpenButton

    public static readonly DependencyProperty IsShowOpenButtonProperty =
        DependencyProperty.Register(nameof(IsShowOpenButton), typeof(bool), typeof(SideMenu),
            new PropertyMetadata(true));

    public bool IsShowOpenButton
    {
        get => (bool)GetValue(IsShowOpenButtonProperty);
        set => SetValue(IsShowOpenButtonProperty, value);
    }

    #endregion IsShowOpenButton

    #region CollapseWidth / ExpansionWidth

    public static readonly DependencyProperty CollapseWidthProperty =
        DependencyProperty.Register(nameof(CollapseWidth), typeof(double), typeof(SideMenu),
            new PropertyMetadata(60d, OnWidthSettingsChanged));

    public static readonly DependencyProperty ExpansionWidthProperty =
        DependencyProperty.Register(nameof(ExpansionWidth), typeof(double), typeof(SideMenu),
            new PropertyMetadata(150d, OnWidthSettingsChanged));

    public double CollapseWidth
    {
        get => (double)GetValue(CollapseWidthProperty);
        set => SetValue(CollapseWidthProperty, value);
    }

    public double ExpansionWidth
    {
        get => (double)GetValue(ExpansionWidthProperty);
        set => SetValue(ExpansionWidthProperty, value);
    }

    /// <summary>
    /// 用户外部修改 CollapseWidth/ExpansionWidth 时——同步更新当前 MenuWidth。
    /// </summary>
    private static void OnWidthSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenu menu && menu._isInitialized)
        {
            var newWidth = menu.IsCompact ? menu.CollapseWidth : menu.ExpansionWidth;
            menu.SetValue(MenuWidthProperty, newWidth);
        }
    }

    #endregion CollapseWidth / ExpansionWidth

    #region Header / Footer

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(SideMenu),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(object), typeof(SideMenu),
            new PropertyMetadata(null));

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    #endregion Header / Footer

    #region AnimationDuration

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(Duration), typeof(SideMenu),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(250))));

    public Duration AnimationDuration
    {
        get => (Duration)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    #endregion AnimationDuration

    #region Indent

    public static readonly DependencyProperty IndentProperty =
        DependencyProperty.Register(nameof(Indent), typeof(double), typeof(SideMenu),
            new PropertyMetadata(12d, OnIndentChanged));

    public double Indent
    {
        get => (double)GetValue(IndentProperty);
        set => SetValue(IndentProperty, value);
    }

    private static void OnIndentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenu menu)
        {
            menu.UpdateChildrenIndentRecursively(menu);
        }
    }

    #endregion Indent

    #region DisplayMemberIcon / DisplayMemberIconTemplate

    public static readonly DependencyProperty DisplayMemberIconProperty =
        DependencyProperty.Register(nameof(DisplayMemberIcon), typeof(string), typeof(SideMenu),
            new PropertyMetadata(null, OnDisplayMemberIconChanged));

    public static readonly DependencyProperty DisplayMemberIconTemplateProperty =
        DependencyProperty.Register(nameof(DisplayMemberIconTemplate), typeof(DataTemplate), typeof(SideMenu),
            new PropertyMetadata(null, OnDisplayMemberIconTemplateChanged));

    public string DisplayMemberIcon
    {
        get => (string)GetValue(DisplayMemberIconProperty);
        set => SetValue(DisplayMemberIconProperty, value);
    }

    public DataTemplate DisplayMemberIconTemplate
    {
        get => (DataTemplate)GetValue(DisplayMemberIconTemplateProperty);
        set => SetValue(DisplayMemberIconTemplateProperty, value);
    }

    private static void OnDisplayMemberIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenu menu)
        {
            menu.UpdateChildrenIconBindingRecursively(menu);
        }
    }

    private static void OnDisplayMemberIconTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenu menu)
        {
            menu.UpdateChildrenIconTemplateRecursively(menu);
        }
    }

    #endregion DisplayMemberIcon / DisplayMemberIconTemplate

    #region ItemClick Command + Event

    public static readonly DependencyProperty ItemClickCommandProperty =
        DependencyProperty.Register(nameof(ItemClickCommand), typeof(ICommand), typeof(SideMenu),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ItemClickCommandParameterProperty =
        DependencyProperty.Register(nameof(ItemClickCommandParameter), typeof(object), typeof(SideMenu),
            new PropertyMetadata(null));

    public static readonly RoutedEvent ItemClickEvent =
        EventManager.RegisterRoutedEvent(nameof(ItemClick), RoutingStrategy.Bubble,
            typeof(SideMenuItemClickEventHandler), typeof(SideMenu));

    public ICommand ItemClickCommand
    {
        get => (ICommand)GetValue(ItemClickCommandProperty);
        set => SetValue(ItemClickCommandProperty, value);
    }

    public object ItemClickCommandParameter
    {
        get => GetValue(ItemClickCommandParameterProperty);
        set => SetValue(ItemClickCommandParameterProperty, value);
    }

    public event SideMenuItemClickEventHandler ItemClick
    {
        add => AddHandler(ItemClickEvent, value);
        remove => RemoveHandler(ItemClickEvent, value);
    }

    #endregion ItemClick Command + Event

    #region 初始化标志 + Loaded 时机

    private bool _isInitialized;

    private void OnSideMenuLoaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        // 初始宽度——MenuWidth 默认值是 150，但可能用户改了 ExpansionWidth/IsCompact，要重算
        var initialWidth = IsCompact ? CollapseWidth : ExpansionWidth;
        SetValue(MenuWidthProperty, initialWidth);
    }

    public SideMenu()
    {
        Loaded += OnSideMenuLoaded;
    }

    #endregion 初始化标志 + Loaded 时机

    #region Override

    protected override DependencyObject GetContainerForItemOverride() => new SideMenuItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is SideMenuItem;

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is SideMenuItem menuItem)
        {
            // 顶级菜单 Level=0
            SideMenuItem.SetLevel(menuItem, 0);
            menuItem.UpdateIndent(Indent);
            ApplyIconBinding(menuItem, item);
        }
    }

    #endregion Override

    #region 激活态管理

    private static void ApplyActivationRecursively(ItemsControl container, System.Collections.Generic.HashSet<SideMenuItem> activeChain)
    {
        foreach (var item in container.Items)
        {
            if (container.ItemContainerGenerator.ContainerFromItem(item) is SideMenuItem menuItem)
            {
                if (activeChain.Contains(menuItem))
                {
                    menuItem.SetActive();
                }
                else
                {
                    menuItem.SetInactive();
                }

                if (menuItem.HasItems)
                {
                    ApplyActivationRecursively(menuItem, activeChain);
                }
            }
        }
    }

    /// <summary>
    /// 帮 SideMenuItem 找上层 ItemsControl——支持祖先链遍历。
    /// </summary>
    internal static ItemsControl ItemsControlFromItemContainer(SideMenuItem item)
    {
        var parent = ItemsControl.ItemsControlFromItemContainer(item);
        // 顶层时 parent 是 SideMenu 自身——不算 SideMenuItem，链终止
        return parent is SideMenuItem ? parent : null;
    }

    /// <summary>
    /// 当 leaf（叶子）菜单项被点击时调用——激活该项 + 该项的所有祖先链；
    /// 取消其他分支的激活。
    /// </summary>
    internal void ActivateLeaf(SideMenuItem leaf)
    {
        // 1) 收集 leaf 的祖先链（包括 leaf 自身）——这些项保持激活
        var activeChain = new System.Collections.Generic.HashSet<SideMenuItem>();
        var current = leaf;
        while (current != null)
        {
            activeChain.Add(current);
            current = ItemsControlFromItemContainer(current) as SideMenuItem;
        }

        // 2) 遍历整棵树——在 activeChain 里的 SetActive，否则 SetInactive
        ApplyActivationRecursively(this, activeChain);
    }

    #endregion 激活态管理

    #region 递归更新工具

    private void UpdateChildrenIndentRecursively(ItemsControl container)
    {
        foreach (var item in container.Items)
        {
            if (container.ItemContainerGenerator.ContainerFromItem(item) is SideMenuItem menuItem)
            {
                menuItem.UpdateIndent(Indent);
                if (menuItem.HasItems)
                {
                    UpdateChildrenIndentRecursively(menuItem);
                }
            }
        }
    }

    private void UpdateChildrenIconBindingRecursively(ItemsControl container)
    {
        for (int i = 0; i < container.Items.Count; i++)
        {
            var item = container.Items[i];
            if (container.ItemContainerGenerator.ContainerFromItem(item) is SideMenuItem menuItem)
            {
                ApplyIconBinding(menuItem, item);
                if (menuItem.HasItems)
                {
                    UpdateChildrenIconBindingRecursively(menuItem);
                }
            }
        }
    }

    private void UpdateChildrenIconTemplateRecursively(ItemsControl container)
    {
        foreach (var item in container.Items)
        {
            if (container.ItemContainerGenerator.ContainerFromItem(item) is SideMenuItem menuItem)
            {
                if (DisplayMemberIconTemplate != null)
                {
                    menuItem.IconTemplate = DisplayMemberIconTemplate;
                }
                if (menuItem.HasItems)
                {
                    UpdateChildrenIconTemplateRecursively(menuItem);
                }
            }
        }
    }

    /// <summary>
    /// 统一的图标绑定逻辑——SideMenu 和 SideMenuItem 都通过这个方法应用。
    /// </summary>
    internal void ApplyIconBinding(SideMenuItem menuItem, object dataItem)
    {
        if (!string.IsNullOrEmpty(DisplayMemberIcon))
        {
            var iconBinding = new Binding(DisplayMemberIcon)
            {
                Source = dataItem,
                Mode = BindingMode.OneWay,
            };
            menuItem.SetBinding(SideMenuItem.IconProperty, iconBinding);
        }

        if (DisplayMemberIconTemplate != null)
        {
            menuItem.IconTemplate = DisplayMemberIconTemplate;
        }
    }

    #endregion 递归更新工具

    #region 点击通知

    /// <summary>
    /// SideMenuItem 点击时调用——触发激活态变化、路由事件、命令。
    /// </summary>
    internal void OnItemClicked(SideMenuItem clickedItem)
    {
        // 1) 激活该项 + 祖先链
        ActivateLeaf(clickedItem);

        // 2) 触发路由事件
        var args = new SideMenuItemClickEventArgs(clickedItem)
        {
            RoutedEvent = ItemClickEvent,
        };
        RaiseEvent(args);

        // 3) 触发命令——参数选择简化为：
        //    用户显式设了 ItemClickCommandParameter → 用它
        //    否则用菜单项的 DataContext（数据绑定场景）
        //    数据驱动更常见，DataContext 直接就是 ViewModel 数据对象
        if (ItemClickCommand != null)
        {
            var parameter = ItemClickCommandParameter ?? clickedItem.DataContext;
            if (ItemClickCommand.CanExecute(parameter))
            {
                ItemClickCommand.Execute(parameter);
            }
        }
    }

    #endregion 点击通知
}