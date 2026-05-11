using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

public class BreadcrumbBarItem : ListBoxItem
{
    static BreadcrumbBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbBarItem),
            new FrameworkPropertyMetadata(typeof(BreadcrumbBarItem)));

        // 只读 DP 在静态构造函数里显式按序初始化——避免依赖字段声明顺序,
        // 即使代码整理工具重排字段也不会出现 NRE

        IsFirstPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsFirst),
            typeof(bool),
            typeof(BreadcrumbBarItem),
            new PropertyMetadata(false));
        IsFirstProperty = IsFirstPropertyKey.DependencyProperty;

        IsLastPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsLast),
            typeof(bool),
            typeof(BreadcrumbBarItem),
            new PropertyMetadata(false));
        IsLastProperty = IsLastPropertyKey.DependencyProperty;
    }

    #region Icon

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(BreadcrumbBarItem),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置面包屑项左侧的图标内容。null 时图标列隐藏(不占空间)。
    /// </summary>
    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion Icon

    #region Indicator

    public static readonly DependencyProperty IndicatorProperty =
        DependencyProperty.Register(
            nameof(Indicator),
            typeof(object),
            typeof(BreadcrumbBarItem),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置该项右侧的分隔符内容(默认 ">",最后一项不显示)。
    /// 可设为任何对象,常用值:">" / "/" / "→",或自定义 Path。
    /// </summary>
    public object Indicator
    {
        get => GetValue(IndicatorProperty);
        set => SetValue(IndicatorProperty, value);
    }

    #endregion Indicator

    #region IndicatorTemplate

    public static readonly DependencyProperty IndicatorTemplateProperty =
        DependencyProperty.Register(
            nameof(IndicatorTemplate),
            typeof(DataTemplate),
            typeof(BreadcrumbBarItem),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置 Indicator 的 DataTemplate。
    /// </summary>
    public DataTemplate IndicatorTemplate
    {
        get => (DataTemplate)GetValue(IndicatorTemplateProperty);
        set => SetValue(IndicatorTemplateProperty, value);
    }

    #endregion IndicatorTemplate

    #region IsFirst (只读)

    /// <summary>
    /// 是否首项,只读 DP——初始化见静态构造函数。
    /// </summary>
    private static readonly DependencyPropertyKey IsFirstPropertyKey;

    public static readonly DependencyProperty IsFirstProperty;

    public bool IsFirst => (bool)GetValue(IsFirstProperty);

    internal void SetIsFirst(bool value)
    {
        SetValue(IsFirstPropertyKey, value);
    }

    #endregion IsFirst (只读)

    #region IsLast (只读)

    /// <summary>
    /// 是否末项,只读 DP——初始化见静态构造函数。
    /// </summary>
    private static readonly DependencyPropertyKey IsLastPropertyKey;

    public static readonly DependencyProperty IsLastProperty;

    public bool IsLast => (bool)GetValue(IsLastProperty);

    internal void SetIsLast(bool value)
    {
        SetValue(IsLastPropertyKey, value);
    }

    #endregion IsLast (只读)

    #region Override Methods

    /// <summary>
    /// 鼠标左键按下——选中自身 + 触发父 BreadcrumbBar 的 ItemClicked 事件。
    /// </summary>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (e.Handled)
        {
            return;
        }

        if (ItemsControl.ItemsControlFromItemContainer(this) is BreadcrumbBar bar)
        {
            IsSelected = true;
            bar.RaiseItemClicked(this);
            e.Handled = true;
        }
    }

    #endregion Override Methods
}
