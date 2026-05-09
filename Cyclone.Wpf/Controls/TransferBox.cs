// ============================================================================
//  破坏性变更说明（vs 旧 TransferBox）：
//    - 拼写修正：SourceDismemberPath → SourceDisplayMemberPath
//                 TargetDismemberPath → TargetDisplayMemberPath
//    - 命名澄清：ItemsSource → SourceItems （避免跟 ItemsControl.ItemsSource 概念冲突）
//                 ItemsTarget → TargetItems
//    - 复数纠正：ItemPanel → ItemsPanel    （跟 ItemsControl.ItemsPanel 一致）
//    - 事件跟随：ItemsSourceChangedEvent → SourceItemsChangedEvent
//                 ItemsTargetChangedEvent → TargetItemsChangedEvent
// ============================================================================
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 双列表选择控件（穿梭框 / shuttle box）。<br/>
/// 左侧"源列表"显示可选项，右侧"目标列表"显示已选项，中间两个箭头按钮在两侧之间移动选中的 item。<br/>
/// 配合 multi-select + 全选 CheckBox，常用于"配置可见列"、"分配权限"、"成员管理"等场景。
/// </summary>
[TemplatePart(Name = PartSourceListBox, Type = typeof(ListBox))]
[TemplatePart(Name = PartTargetListBox, Type = typeof(ListBox))]
[TemplatePart(Name = PartToSourceRepeatButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PartToTargetRepeatButton, Type = typeof(RepeatButton))]
public class TransferBox : Control
{
    private const string PartSourceListBox = "PART_SourceListBox";

    private const string PartTargetListBox = "PART_TargetListBox";

    private const string PartToSourceRepeatButton = "PART_ToSourceRepeatButton";

    private const string PartToTargetRepeatButton = "PART_ToTargetRepeatButton";

    private ListBox _sourceListBox;

    private ListBox _targetListBox;

    static TransferBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TransferBox),
            new FrameworkPropertyMetadata(typeof(TransferBox)));

        CommandManager.RegisterClassCommandBinding(typeof(TransferBox),
            new CommandBinding(ToSourceCommand, OnToSourceExecuted, OnCanToSourceExecute));
        CommandManager.RegisterClassCommandBinding(typeof(TransferBox),
            new CommandBinding(ToTargetCommand, OnToTargetExecuted, OnCanToTargetExecute));
    }

    #region SourceHeader

    public static readonly DependencyProperty SourceHeaderProperty =
        DependencyProperty.Register(
            nameof(SourceHeader),
            typeof(object),
            typeof(TransferBox),
            new PropertyMetadata(null));

    /// <summary>源列表（左侧）的标题。可以是字符串或任意对象（搭配 HeaderTemplate）。</summary>
    public object SourceHeader
    {
        get => GetValue(SourceHeaderProperty);
        set => SetValue(SourceHeaderProperty, value);
    }

    #endregion SourceHeader

    #region TargetHeader

    public static readonly DependencyProperty TargetHeaderProperty =
        DependencyProperty.Register(
            nameof(TargetHeader),
            typeof(object),
            typeof(TransferBox),
            new PropertyMetadata(null));

    /// <summary>目标列表（右侧）的标题。</summary>
    public object TargetHeader
    {
        get => GetValue(TargetHeaderProperty);
        set => SetValue(TargetHeaderProperty, value);
    }

    #endregion TargetHeader

    #region SourceDisplayMemberPath

    public static readonly DependencyProperty SourceDisplayMemberPathProperty =
        DependencyProperty.Register(
            nameof(SourceDisplayMemberPath),
            typeof(string),
            typeof(TransferBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 源列表显示文本的属性路径。等价于 <see cref="ItemsControl.DisplayMemberPath"/>。
    /// 不设时使用 <see cref="object.ToString"/>。
    /// </summary>
    public string SourceDisplayMemberPath
    {
        get => (string)GetValue(SourceDisplayMemberPathProperty);
        set => SetValue(SourceDisplayMemberPathProperty, value);
    }

    #endregion SourceDisplayMemberPath

    #region TargetDisplayMemberPath

    public static readonly DependencyProperty TargetDisplayMemberPathProperty =
        DependencyProperty.Register(
            nameof(TargetDisplayMemberPath),
            typeof(string),
            typeof(TransferBox),
            new PropertyMetadata(null));

    /// <summary>目标列表显示文本的属性路径。</summary>
    public string TargetDisplayMemberPath
    {
        get => (string)GetValue(TargetDisplayMemberPathProperty);
        set => SetValue(TargetDisplayMemberPathProperty, value);
    }

    #endregion TargetDisplayMemberPath

    #region SourceItems

    public static readonly DependencyProperty SourceItemsProperty =
        DependencyProperty.Register(
            nameof(SourceItems),
            typeof(IList),
            typeof(TransferBox),
            new PropertyMetadata(null, OnCollectionPropertyChanged));

    /// <summary>
    /// 源列表的数据集合。<b>必须实现 <see cref="INotifyCollectionChanged"/></b>
    /// （推荐 <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>）——
    /// 否则 add/remove 后 ListBox 不会刷新，控件会输出 trace warning。
    /// </summary>
    public IList SourceItems
    {
        get => (IList)GetValue(SourceItemsProperty);
        set => SetValue(SourceItemsProperty, value);
    }

    #endregion SourceItems

    #region TargetItems

    public static readonly DependencyProperty TargetItemsProperty =
        DependencyProperty.Register(
            nameof(TargetItems),
            typeof(IList),
            typeof(TransferBox),
            new PropertyMetadata(null, OnCollectionPropertyChanged));

    /// <summary>
    /// 目标列表的数据集合。同样要求实现 <see cref="INotifyCollectionChanged"/>。
    /// </summary>
    public IList TargetItems
    {
        get => (IList)GetValue(TargetItemsProperty);
        set => SetValue(TargetItemsProperty, value);
    }

    #endregion TargetItems

    #region ItemTemplate

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(TransferBox),
            new PropertyMetadata(null));

    /// <summary>列表项的 DataTemplate。同时应用于源列表和目标列表。</summary>
    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    #endregion ItemTemplate

    #region ItemsPanel

    public static readonly DependencyProperty ItemsPanelProperty =
        DependencyProperty.Register(
            nameof(ItemsPanel),
            typeof(ItemsPanelTemplate),
            typeof(TransferBox),
            new PropertyMetadata(null));

    /// <summary>列表项的容器面板（默认垂直 StackPanel，可改为 WrapPanel 等）。</summary>
    public ItemsPanelTemplate ItemsPanel
    {
        get => (ItemsPanelTemplate)GetValue(ItemsPanelProperty);
        set => SetValue(ItemsPanelProperty, value);
    }

    #endregion ItemsPanel

    #region ItemContainerStyle

    public static readonly DependencyProperty ItemContainerStyleProperty =
        DependencyProperty.Register(
            nameof(ItemContainerStyle),
            typeof(Style),
            typeof(TransferBox),
            new PropertyMetadata(null));

    /// <summary>
    /// 列表项 <see cref="ListBoxItem"/> 的样式。default style 已经设了带左侧勾选 CheckBox 的内置样式；
    /// 用户可以覆盖此 DP 自定义渲染。
    /// </summary>
    public Style ItemContainerStyle
    {
        get => (Style)GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    #endregion ItemContainerStyle

    #region SourceItemsChanged Event

    public static readonly RoutedEvent SourceItemsChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(SourceItemsChanged),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TransferBox));

    /// <summary>源列表内容变化（通过移动按钮移入/移出）后触发。</summary>
    public event RoutedEventHandler SourceItemsChanged
    {
        add => AddHandler(SourceItemsChangedEvent, value);
        remove => RemoveHandler(SourceItemsChangedEvent, value);
    }

    #endregion SourceItemsChanged Event

    #region TargetItemsChanged Event

    public static readonly RoutedEvent TargetItemsChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(TargetItemsChanged),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(TransferBox));

    /// <summary>目标列表内容变化后触发。</summary>
    public event RoutedEventHandler TargetItemsChanged
    {
        add => AddHandler(TargetItemsChangedEvent, value);
        remove => RemoveHandler(TargetItemsChangedEvent, value);
    }

    #endregion TargetItemsChanged Event

    #region Commands

    /// <summary>把目标列表选中项（或第一项）移回源列表。</summary>
    public static readonly RoutedCommand ToSourceCommand =
        new RoutedCommand(nameof(ToSourceCommand), typeof(TransferBox));

    /// <summary>把源列表选中项（或第一项）移到目标列表。</summary>
    public static readonly RoutedCommand ToTargetCommand =
        new RoutedCommand(nameof(ToTargetCommand), typeof(TransferBox));

    private static void OnToSourceExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TransferBox box)
        {
            box.MoveSelectedToSource();
        }
    }

    private static void OnCanToSourceExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is TransferBox box
            && box.TargetItems != null
            && box.TargetItems.Count > 0;
    }

    private static void OnToTargetExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TransferBox box)
        {
            box.MoveSelectedToTarget();
        }
    }

    private static void OnCanToTargetExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is TransferBox box
            && box.SourceItems != null
            && box.SourceItems.Count > 0;
    }

    #endregion Commands

    #region Override Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 缓存模板部件——之前每次 MoveItem 都 GetTemplateChild 遍历 visual tree
        _sourceListBox = GetTemplateChild(PartSourceListBox) as ListBox;
        _targetListBox = GetTemplateChild(PartTargetListBox) as ListBox;
    }

    #endregion Override Methods

    #region Private Methods

    /// <summary>
    /// 公共移动逻辑：从 <paramref name="from"/> 拿出选中项（空选则取第一项）放到 <paramref name="to"/>。
    /// <para>
    /// 用 <c>ToList()</c> 拷贝 SelectedItems 快照——避免遍历期间集合在变（旧实现用
    /// <c>for i-- + [0]</c> 双重索引能 work 但脆弱）。
    /// </para>
    /// </summary>
    private static void MoveSelected(ListBox sourceListBox, IList from, IList to)
    {
        if (sourceListBox == null || from == null || to == null)
        {
            return;
        }

        if (sourceListBox.SelectedItems != null && sourceListBox.SelectedItems.Count > 0)
        {
            // 多选：先拷贝快照
            var toMove = sourceListBox.SelectedItems.Cast<object>().ToList();
            foreach (var item in toMove)
            {
                to.Add(item);
                from.Remove(item);
            }
        }
        else if (from.Count > 0)
        {
            // 空选：移第一项
            var first = from[0];
            to.Add(first);
            from.Remove(first);
        }
    }

    /// <summary>
    /// SourceItems / TargetItems 改变时检查是否实现 INotifyCollectionChanged。
    /// 不实现时输出 trace warning（不抛异常——保持 user 在 List&lt;T&gt; 等场景下控件不崩，但提示问题）。
    /// </summary>
    private static void OnCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null && e.NewValue is not INotifyCollectionChanged)
        {
            Trace.TraceWarning(
                "[TransferBox] {0} 不是 INotifyCollectionChanged——集合 add/remove 后 ListBox 不会刷新。建议传 ObservableCollection<T>。",
                e.Property.Name);
        }
    }

    /// <summary>移动源列表选中项到目标列表（空选时移第一项——配合 RepeatButton 长按可以批量推送）。</summary>
    private void MoveSelectedToTarget()
    {
        MoveSelected(_sourceListBox, SourceItems, TargetItems);
        RaiseChangedEvents();
    }

    /// <summary>移动目标列表选中项回源列表。</summary>
    private void MoveSelectedToSource()
    {
        MoveSelected(_targetListBox, TargetItems, SourceItems);
        RaiseChangedEvents();
    }

    private void RaiseChangedEvents()
    {
        RaiseEvent(new RoutedEventArgs(SourceItemsChangedEvent, this));
        RaiseEvent(new RoutedEventArgs(TargetItemsChangedEvent, this));
    }

    #endregion Private Methods
}