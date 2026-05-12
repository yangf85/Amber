using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 分页栏控件。提供页码导航、每页数量切换、跳转输入、信息显示。
/// <para>用法 — 手动绑定三个 DP:</para>
/// <code>
/// &lt;ctl:Pagination PageIndex="{Binding PageIndex, Mode=TwoWay}"
///                 PageSize="{Binding PageSize, Mode=TwoWay}"
///                 ItemCount="{Binding ItemCount}" /&gt;
/// </code>
/// <para>
/// 命令:<see cref="FirstCommand"/> / <see cref="PrevCommand"/> / <see cref="NextCommand"/> /
/// <see cref="LastCommand"/> / <see cref="GotoCommand"/> — 全部类级 CommandBinding 自动响应。
/// </para>
/// </summary>
[TemplatePart(Name = PART_GotoNumberBox, Type = typeof(NumberBox))]
public class Pagination : Control
{
    private const string PART_GotoNumberBox = nameof(PART_GotoNumberBox);

    private NumberBox _gotoNumberBox;

    #region Constructors

    static Pagination()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Pagination),
            new FrameworkPropertyMetadata(typeof(Pagination)));

        // 命令 + 类级 CommandBinding — 所有 Pagination 实例自动响应
        FirstCommand = new RoutedCommand(nameof(FirstCommand), typeof(Pagination));
        PrevCommand = new RoutedCommand(nameof(PrevCommand), typeof(Pagination));
        NextCommand = new RoutedCommand(nameof(NextCommand), typeof(Pagination));
        LastCommand = new RoutedCommand(nameof(LastCommand), typeof(Pagination));
        GotoCommand = new RoutedCommand(nameof(GotoCommand), typeof(Pagination));

        CommandManager.RegisterClassCommandBinding(typeof(Pagination),
            new CommandBinding(FirstCommand, OnFirstExecuted, OnCanPrev));
        CommandManager.RegisterClassCommandBinding(typeof(Pagination),
            new CommandBinding(PrevCommand, OnPrevExecuted, OnCanPrev));
        CommandManager.RegisterClassCommandBinding(typeof(Pagination),
            new CommandBinding(NextCommand, OnNextExecuted, OnCanNext));
        CommandManager.RegisterClassCommandBinding(typeof(Pagination),
            new CommandBinding(LastCommand, OnLastExecuted, OnCanNext));
        CommandManager.RegisterClassCommandBinding(typeof(Pagination),
            new CommandBinding(GotoCommand, OnGotoExecuted));

        // 只读 DP 注册放在 cctor 里 — 项目惯例
        PageCountPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(PageCount),
            typeof(int),
            typeof(Pagination),
            new PropertyMetadata(1));
        PageCountProperty = PageCountPropertyKey.DependencyProperty;

        PageItemsPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(PageItems),
            typeof(IReadOnlyList<PageItem>),
            typeof(Pagination),
            new PropertyMetadata(Array.Empty<PageItem>()));
        PageItemsProperty = PageItemsPropertyKey.DependencyProperty;
    }

    public Pagination()
    {
        // 默认 PageSizeOptions 给个常用集合
        SetCurrentValue(PageSizeOptionsProperty, new[] { 10, 20, 30, 50, 100 });
    }

    #endregion Constructors

    #region PageIndex

    public static readonly DependencyProperty PageIndexProperty =
        DependencyProperty.Register(
            nameof(PageIndex),
            typeof(int),
            typeof(Pagination),
            new FrameworkPropertyMetadata(
                1,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPageIndexChanged,
                CoercePageIndex));

    /// <summary>当前页索引 (1-based)。</summary>
    public int PageIndex
    {
        get => (int)GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }

    private static object CoercePageIndex(DependencyObject d, object baseValue)
    {
        var p = (Pagination)d;
        var v = (int)baseValue;
        return Math.Max(1, Math.Min(v, Math.Max(1, p.PageCount)));
    }

    private static void OnPageIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var p = (Pagination)d;
        p.UpdatePageItems();
        p.SyncGotoNumberBox();
        p.RaiseEvent(new RoutedEventArgs(PageIndexChangedEvent, p));
    }

    #endregion PageIndex

    #region PageSize

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(
            nameof(PageSize),
            typeof(int),
            typeof(Pagination),
            new FrameworkPropertyMetadata(
                20,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPageSizeChanged,
                CoercePageSize));

    /// <summary>每页显示数量。默认 20。</summary>
    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    private static object CoercePageSize(DependencyObject d, object baseValue)
    {
        return Math.Max(1, (int)baseValue);
    }

    private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var p = (Pagination)d;

        // 不强制跳首页 — 让用户停在原数据位置附近
        int oldSize = (int)e.OldValue;
        int newSize = (int)e.NewValue;
        int firstItemOffset = (p.PageIndex - 1) * oldSize;

        p.RecalculatePageCount();
        int newIndex = firstItemOffset / newSize + 1;
        p.SetCurrentValue(PageIndexProperty, newIndex);

        // PageIndex 没变也要刷一次 PageItems(PageCount 变了)
        p.UpdatePageItems();
    }

    #endregion PageSize

    #region ItemCount

    public static readonly DependencyProperty ItemCountProperty =
        DependencyProperty.Register(
            nameof(ItemCount),
            typeof(int),
            typeof(Pagination),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnItemCountChanged,
                CoerceItemCount));

    /// <summary>总条目数。默认 0。</summary>
    public int ItemCount
    {
        get => (int)GetValue(ItemCountProperty);
        set => SetValue(ItemCountProperty, value);
    }

    private static object CoerceItemCount(DependencyObject d, object baseValue)
    {
        return Math.Max(0, (int)baseValue);
    }

    private static void OnItemCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var p = (Pagination)d;
        p.RecalculatePageCount();
        // ItemCount 减少可能让 PageIndex 越界,触发一次 coerce
        p.CoerceValue(PageIndexProperty);
        p.UpdatePageItems();
    }

    #endregion ItemCount

    #region PageCount (只读)

    private static readonly DependencyPropertyKey PageCountPropertyKey;

    public static readonly DependencyProperty PageCountProperty;

    /// <summary>总页数。由 ItemCount / PageSize 计算,至少为 1。</summary>
    public int PageCount => (int)GetValue(PageCountProperty);

    private void RecalculatePageCount()
    {
        // ItemCount=0 时仍显示"第 1 页 / 共 1 页",避免显示矛盾
        int newCount = ItemCount > 0
            ? (int)Math.Ceiling(ItemCount * 1.0 / PageSize)
            : 1;
        SetValue(PageCountPropertyKey, newCount);
    }

    #endregion PageCount

    #region PageItems (只读)

    private static readonly DependencyPropertyKey PageItemsPropertyKey;

    public static readonly DependencyProperty PageItemsProperty;

    /// <summary>当前可见的页码集合(含省略号占位)。模板通过此属性渲染页码按钮。</summary>
    public IReadOnlyList<PageItem> PageItems => (IReadOnlyList<PageItem>)GetValue(PageItemsProperty);

    /// <summary>重建 PageItems — 5/7 个页码 + 至多 2 个省略号,当前页标 IsCurrent。</summary>
    private void UpdatePageItems()
    {
        int total = Math.Max(1, PageCount);
        int current = Math.Max(1, Math.Min(PageIndex, total));

        var items = new List<PageItem>();

        if (total <= 7)
        {
            for (int i = 1; i <= total; i++)
            {
                items.Add(new PageItem { PageNumber = i, IsCurrent = i == current });
            }
        }
        else if (current <= 4)
        {
            for (int i = 1; i <= 5; i++)
                items.Add(new PageItem { PageNumber = i, IsCurrent = i == current });
            items.Add(new PageItem { IsEllipsis = true });
            items.Add(new PageItem { PageNumber = total, IsCurrent = total == current });
        }
        else if (current >= total - 3)
        {
            items.Add(new PageItem { PageNumber = 1, IsCurrent = current == 1 });
            items.Add(new PageItem { IsEllipsis = true });
            for (int i = total - 4; i <= total; i++)
                items.Add(new PageItem { PageNumber = i, IsCurrent = i == current });
        }
        else
        {
            items.Add(new PageItem { PageNumber = 1 });
            items.Add(new PageItem { IsEllipsis = true });
            items.Add(new PageItem { PageNumber = current - 1 });
            items.Add(new PageItem { PageNumber = current, IsCurrent = true });
            items.Add(new PageItem { PageNumber = current + 1 });
            items.Add(new PageItem { IsEllipsis = true });
            items.Add(new PageItem { PageNumber = total });
        }

        SetValue(PageItemsPropertyKey, (IReadOnlyList<PageItem>)items);
    }

    #endregion PageItems

    #region PageSizeOptions

    public static readonly DependencyProperty PageSizeOptionsProperty =
        DependencyProperty.Register(
            nameof(PageSizeOptions),
            typeof(IEnumerable<int>),
            typeof(Pagination),
            new PropertyMetadata(null));

    /// <summary>每页数量的可选项集合。默认 {10, 20, 30, 50, 100}。</summary>
    public IEnumerable<int> PageSizeOptions
    {
        get => (IEnumerable<int>)GetValue(PageSizeOptionsProperty);
        set => SetValue(PageSizeOptionsProperty, value);
    }

    #endregion PageSizeOptions

    #region ShowJumper / ShowSizeChanger / ShowInfo

    public static readonly DependencyProperty ShowJumperProperty =
        DependencyProperty.Register(
            nameof(ShowJumper),
            typeof(bool),
            typeof(Pagination),
            new PropertyMetadata(true));

    /// <summary>是否显示"前往第 N 页"输入框。</summary>
    public bool ShowJumper
    {
        get => (bool)GetValue(ShowJumperProperty);
        set => SetValue(ShowJumperProperty, value);
    }

    public static readonly DependencyProperty ShowSizeChangerProperty =
        DependencyProperty.Register(
            nameof(ShowSizeChanger),
            typeof(bool),
            typeof(Pagination),
            new PropertyMetadata(true));

    /// <summary>是否显示"每页 N 条"切换器。</summary>
    public bool ShowSizeChanger
    {
        get => (bool)GetValue(ShowSizeChangerProperty);
        set => SetValue(ShowSizeChangerProperty, value);
    }

    public static readonly DependencyProperty ShowInfoProperty =
        DependencyProperty.Register(
            nameof(ShowInfo),
            typeof(bool),
            typeof(Pagination),
            new PropertyMetadata(true));

    /// <summary>是否显示"共 N 条"信息文本。</summary>
    public bool ShowInfo
    {
        get => (bool)GetValue(ShowInfoProperty);
        set => SetValue(ShowInfoProperty, value);
    }

    #endregion ShowJumper / ShowSizeChanger / ShowInfo

    #region RoutedEvents

    public static readonly RoutedEvent PageIndexChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(PageIndexChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(Pagination));

    /// <summary>PageIndex 改变后触发。</summary>
    public event RoutedEventHandler PageIndexChanged
    {
        add => AddHandler(PageIndexChangedEvent, value);
        remove => RemoveHandler(PageIndexChangedEvent, value);
    }

    #endregion RoutedEvents

    #region Commands

    public static RoutedCommand FirstCommand { get; }
    public static RoutedCommand PrevCommand { get; }
    public static RoutedCommand NextCommand { get; }
    public static RoutedCommand LastCommand { get; }

    /// <summary>跳到指定页。CommandParameter 传 int 页码或 PageItem(自动取 PageNumber)。</summary>
    public static RoutedCommand GotoCommand { get; }

    private static void OnCanPrev(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is Pagination p && p.PageIndex > 1;
    }

    private static void OnCanNext(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is Pagination p && p.PageIndex < p.PageCount;
    }

    private static void OnFirstExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Pagination p) p.PageIndex = 1;
    }

    private static void OnPrevExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Pagination p) p.PageIndex--;
    }

    private static void OnNextExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Pagination p) p.PageIndex++;
    }

    private static void OnLastExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Pagination p) p.PageIndex = p.PageCount;
    }

    private static void OnGotoExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is not Pagination p) return;

        int target = e.Parameter switch
        {
            PageItem item when !item.IsEllipsis => item.PageNumber,
            int i => i,
            string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) => n,
            _ => -1
        };

        if (target >= 1) p.PageIndex = target;
    }

    #endregion Commands

    #region Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _gotoNumberBox = GetTemplateChild(PART_GotoNumberBox) as NumberBox;
        SyncGotoNumberBox();
        UpdatePageItems();
    }

    private void SyncGotoNumberBox()
    {
        _gotoNumberBox?.SetCurrentValue(NumberBox.ValueProperty, (double)PageIndex);
    }

    #endregion Override
}
