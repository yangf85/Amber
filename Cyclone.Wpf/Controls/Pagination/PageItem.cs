using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 分页栏的单个页码项 — 替代字符串集合("1"/"2"/"···")的对象表达,
/// 样式 trigger 通过 IsEllipsis / IsCurrent 属性判断,不再做字符串比较或值转换。
/// </summary>
public class PageItem : DependencyObject
{
    public static readonly DependencyProperty PageNumberProperty =
        DependencyProperty.Register(
            nameof(PageNumber),
            typeof(int),
            typeof(PageItem),
            new PropertyMetadata(0));

    /// <summary>该项对应的页码 (1-based)。<see cref="IsEllipsis"/>=True 时为 0。</summary>
    public int PageNumber
    {
        get => (int)GetValue(PageNumberProperty);
        set => SetValue(PageNumberProperty, value);
    }

    public static readonly DependencyProperty IsEllipsisProperty =
        DependencyProperty.Register(
            nameof(IsEllipsis),
            typeof(bool),
            typeof(PageItem),
            new PropertyMetadata(false));

    /// <summary>是否为省略号占位项。True 时显示 "···",不可点击。</summary>
    public bool IsEllipsis
    {
        get => (bool)GetValue(IsEllipsisProperty);
        set => SetValue(IsEllipsisProperty, value);
    }

    public static readonly DependencyProperty IsCurrentProperty =
        DependencyProperty.Register(
            nameof(IsCurrent),
            typeof(bool),
            typeof(PageItem),
            new PropertyMetadata(false));

    /// <summary>是否为当前页 — 样式据此高亮显示。</summary>
    public bool IsCurrent
    {
        get => (bool)GetValue(IsCurrentProperty);
        set => SetValue(IsCurrentProperty, value);
    }

    public override string ToString() => IsEllipsis ? "···" : PageNumber.ToString();
}
