using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 级联选择器。支持三种绑定方式：
/// <see cref="SelectedItem"/>（对象引用，通用）、
/// <see cref="SelectedValue"/> + <see cref="SelectedValuePath"/>（按 ID，O(1) 无歧义，推荐），
/// <see cref="SelectedPath"/>（字符串路径，best-effort，路径唯一时可双向）。
/// </summary>
[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(CascadePickerItem))]
[TemplatePart(Name = nameof(PART_DisplayedTextBox), Type = typeof(TextBox))]
[TemplatePart(Name = nameof(PART_ItemsPopup), Type = typeof(Popup))]
[TemplatePart(Name = nameof(PART_ClearButton), Type = typeof(Button))]
[TemplatePart(Name = nameof(PART_OpenToggleButton), Type = typeof(ToggleButton))]
public class CascadePicker : ItemsControl
{
    private const string PART_DisplayedTextBox = "PART_DisplayedTextBox";

    private const string PART_ItemsPopup = "PART_ItemsPopup";

    private const string PART_ClearButton = "PART_ClearButton";

    private const string PART_OpenToggleButton = "PART_OpenToggleButton";

    private static readonly Dictionary<(Type, string), PropertyInfo> _propertyCache = new();

    // ---- 反射缓存 ----
    private static readonly object _propertyCacheLock = new();

    private readonly Dictionary<object, NodeInfo> _byItem
        = new(ReferenceComparer.Instance);

    // ---- 索引：三张表，ItemsSource 变化时重建 ----
    /// <summary>SelectedValuePath 设置后才建。</summary>
    private readonly Dictionary<object, object> _byValue
        = new();

    /// <summary>路径字符串到 item；可能存在路径冲突，遇冲突保留首个。</summary>
    private readonly Dictionary<string, object> _byPath
        = new(StringComparer.Ordinal);

    private TextBox _textBox;

    private Popup _popup;

    private Button _clearButton;

    private ToggleButton _openToggleButton;

    private CascadePickerItem _focusedItem;

    /// <summary>正在内部同步三个 Selected 属性，防止回环。</summary>
    private bool _isSyncing;

    /// <summary>SelectedValue / SelectedPath 在 ItemsSource 就绪前被设置时的暂存值。</summary>
    private object _pendingValue;

    private string _pendingPath;

    /// <summary>路径冲突列表，仅 trace 警告用。</summary>
    private List<string> _pathConflicts;

    static CascadePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CascadePicker),
            new FrameworkPropertyMetadata(typeof(CascadePicker)));

        CommandManager.RegisterClassCommandBinding(typeof(CascadePicker),
            new CommandBinding(ClearCommand, OnClearCommandExecuted, OnClearCommandCanExecute));
    }

    public CascadePicker()
    {
        // 一次性挂载子项点击事件，不放在 Loaded 里避免重复注册
        AddHandler(CascadePickerItem.ItemClickEvent, new RoutedEventHandler(OnChildItemClick));
    }

    /// <summary>
    /// 引用相等比较器——避免数据 model 重写 Equals 后字典 key 冲突。
    /// （.NET 5+ 自带 ReferenceEqualityComparer，但要兼容 .NET Framework 4.8 故自带一份。）
    /// </summary>
    private sealed class ReferenceComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceComparer Instance = new();

        public new bool Equals(object x, object y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj)
            => obj is null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);

        private ReferenceComparer()
        {
        }
    }

    private struct NodeInfo
    {
        public object Item;

        public object[] Ancestors;   // 根到该节点（不含自身）

        public string Path;          // 完整路径字符串（含自身）

        public string NodeText;      // 自身节点文本
    }

    #region IsEditable

    public static readonly DependencyProperty IsEditableProperty =
    DependencyProperty.Register(nameof(IsEditable), typeof(bool),
        typeof(CascadePicker), new PropertyMetadata(true));

    public bool IsEditable
    {
        get => (bool)GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }

    #endregion IsEditable

    #region IsOpened

    public static readonly DependencyProperty IsOpenedProperty =
        DependencyProperty.Register(
            nameof(IsOpened),
            typeof(bool),
            typeof(CascadePicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenedChanged));

    /// <summary>
    /// 获取或设置下拉是否打开。
    /// </summary>
    public bool IsOpened
    {
        get => (bool)GetValue(IsOpenedProperty);
        set => SetValue(IsOpenedProperty, value);
    }

    private static void OnIsOpenedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CascadePicker p)
        {
            return;
        }

        if (p.IsReadOnly && (bool)e.NewValue)
        {
            p.SetCurrentValue(IsOpenedProperty, false);
            return;
        }

        if ((bool)e.NewValue)
        {
            p.SetFocusedItem(null);
        }
        else
        {
            // 关闭时折叠所有子菜单
            p.CollapseAllItems();
        }

        p.RaiseEvent(new RoutedEventArgs(OpenedChangedEvent, p));
    }

    #endregion IsOpened

    #region IsReadOnly

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(CascadePicker),
            new PropertyMetadata(false, OnIsReadOnlyChanged));

    /// <summary>
    /// 获取或设置控件是否只读。
    /// </summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CascadePicker p)
        {
            return;
        }

        if ((bool)e.NewValue && p.IsOpened)
        {
            p.SetCurrentValue(IsOpenedProperty, false);
        }

        CommandManager.InvalidateRequerySuggested();
    }

    #endregion IsReadOnly

    #region IsShowFullPath

    public static readonly DependencyProperty IsShowFullPathProperty =
        DependencyProperty.Register(
            nameof(IsShowFullPath),
            typeof(bool),
            typeof(CascadePicker),
            new PropertyMetadata(false, OnIsShowFullPathChanged));

    /// <summary>
    /// 获取或设置是否在文本框中显示完整路径（false 时只显示叶子节点文本）。
    /// </summary>
    public bool IsShowFullPath
    {
        get => (bool)GetValue(IsShowFullPathProperty);
        set => SetValue(IsShowFullPathProperty, value);
    }

    private static void OnIsShowFullPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePicker p)
        {
            p.UpdateText();
        }
    }

    #endregion IsShowFullPath

    #region Separator

    public static readonly DependencyProperty SeparatorProperty =
        DependencyProperty.Register(
            nameof(Separator),
            typeof(string),
            typeof(CascadePicker),
            new PropertyMetadata("/", OnSeparatorChanged));

    /// <summary>
    /// 获取或设置路径分隔符。
    /// </summary>
    public string Separator
    {
        get => (string)GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    private static void OnSeparatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePicker p)
        {
            p.RebuildIndex();
        }
    }

    #endregion Separator

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register(
            nameof(Watermark),
            typeof(string),
            typeof(CascadePicker),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 获取或设置无值时显示的占位文本。
    /// </summary>
    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    #endregion Watermark

    #region NodeMemberPath

    public static readonly DependencyProperty NodeMemberPathProperty =
        DependencyProperty.Register(
            nameof(NodeMemberPath),
            typeof(string),
            typeof(CascadePicker),
            new PropertyMetadata(null, OnIndexAffectingPropertyChanged));

    /// <summary>
    /// 获取或设置数据项中作为节点显示文本的属性路径（类似 DisplayMemberPath）。
    /// </summary>
    public string NodeMemberPath
    {
        get => (string)GetValue(NodeMemberPathProperty);
        set => SetValue(NodeMemberPathProperty, value);
    }

    #endregion NodeMemberPath

    #region ChildrenMemberPath

    public static readonly DependencyProperty ChildrenMemberPathProperty =
        DependencyProperty.Register(
            nameof(ChildrenMemberPath),
            typeof(string),
            typeof(CascadePicker),
            new PropertyMetadata(null, OnIndexAffectingPropertyChanged));

    /// <summary>
    /// 获取或设置数据项中作为子集合的属性路径（如 "Children" / "Cities"）。
    /// 设置后控件能递归索引整棵树并自动给容器装配 ItemsSource binding。
    /// </summary>
    public string ChildrenMemberPath
    {
        get => (string)GetValue(ChildrenMemberPathProperty);
        set => SetValue(ChildrenMemberPathProperty, value);
    }

    #endregion ChildrenMemberPath

    #region SelectedValuePath

    public static readonly DependencyProperty SelectedValuePathProperty =
        DependencyProperty.Register(
            nameof(SelectedValuePath),
            typeof(string),
            typeof(CascadePicker),
            new PropertyMetadata(null, OnIndexAffectingPropertyChanged));

    /// <summary>
    /// 获取或设置数据项中作为唯一值（ID）的属性路径。配合 <see cref="SelectedValue"/> 使用。
    /// </summary>
    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    #endregion SelectedValuePath

    #region SelectedItem

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(CascadePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    /// <summary>
    /// 获取或设置当前选中的数据项（对象引用）。所有绑定方式中最通用。
    /// </summary>
    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CascadePicker p || p._isSyncing)
        {
            return;
        }

        p.SyncFromSelectedItem(e.NewValue);
        p.RaiseEvent(new RoutedEventArgs(SelectedChangedEvent, p));
        CommandManager.InvalidateRequerySuggested();
    }

    #endregion SelectedItem

    #region SelectedValue

    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(
            nameof(SelectedValue),
            typeof(object),
            typeof(CascadePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

    /// <summary>
    /// 获取或设置当前选中项的值（来自 <see cref="SelectedValuePath"/> 字段）。推荐用于 MVVM 绑定。
    /// </summary>
    public object SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CascadePicker p || p._isSyncing)
        {
            return;
        }

        p.SyncFromSelectedValue(e.NewValue);
    }

    #endregion SelectedValue

    #region SelectedPath

    public static readonly DependencyProperty SelectedPathProperty =
        DependencyProperty.Register(
            nameof(SelectedPath),
            typeof(string),
            typeof(CascadePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedPathChanged));

    /// <summary>
    /// 获取或设置当前选中项的字符串路径。仅当树中路径唯一时支持反向定位。
    /// </summary>
    public string SelectedPath
    {
        get => (string)GetValue(SelectedPathProperty);
        set => SetValue(SelectedPathProperty, value);
    }

    private static void OnSelectedPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CascadePicker p || p._isSyncing)
        {
            return;
        }

        p.SyncFromSelectedPath(e.NewValue as string);
    }

    #endregion SelectedPath

    #region Text

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(CascadePicker),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    private bool _isSyncingFromText;

    /// <summary>
    /// 获取或设置文本框中显示的文本（受 <see cref="IsShowFullPath"/> 影响）。
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var p = (CascadePicker)d;
        CommandManager.InvalidateRequerySuggested();

        if (p._isSyncing || !p.IsEditable)
        {
            return; // 来自选中项同步、或非编辑模式，不反查
        }

        p._isSyncingFromText = true;
        try
        {
            var text = e.NewValue as string;
            if (!string.IsNullOrEmpty(text) && p._byPath.TryGetValue(text, out var item))
            {
                p.SetCurrentValue(SelectedItemProperty, item); // 输入恰好匹配某个完整路径
            }
            else
            {
                p.SetCurrentValue(SelectedItemProperty, null); // 列表外的自定义文本
            }
        }
        finally
        {
            p._isSyncingFromText = false;
        }
    }

    #endregion Text

    #region MaxDropDownHeight

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double),
            typeof(CascadePicker),
            new PropertyMetadata(300d));

    /// <summary>
    /// 获取或设置下拉面板的最大高度。
    /// </summary>
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    #endregion MaxDropDownHeight

    #region RoutedEvents

    /// <summary>
    /// SelectedItem 变化时触发。
    /// </summary>
    public static readonly RoutedEvent SelectedChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SelectedChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CascadePicker));

    /// <summary>
    /// IsOpened 变化时触发。
    /// </summary>
    public static readonly RoutedEvent OpenedChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(OpenedChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(CascadePicker));

    /// <summary>
    /// 选中项改变时触发。
    /// </summary>
    public event RoutedEventHandler SelectedChanged
    {
        add => AddHandler(SelectedChangedEvent, value);
        remove => RemoveHandler(SelectedChangedEvent, value);
    }

    /// <summary>
    /// 下拉打开 / 关闭时触发。
    /// </summary>
    public event RoutedEventHandler OpenedChanged
    {
        add => AddHandler(OpenedChangedEvent, value);
        remove => RemoveHandler(OpenedChangedEvent, value);
    }

    #endregion RoutedEvents

    #region Commands

    /// <summary>
    /// 清空选中项的命令（默认绑定 Delete 键）。
    /// </summary>
    public static readonly RoutedCommand ClearCommand =
        new RoutedCommand(
            "Clear",
            typeof(CascadePicker),
            new InputGestureCollection { new KeyGesture(Key.Delete) });

    /// <summary>
    /// 清空当前选中项与文本。
    /// </summary>
    public void Clear()
    {
        SetCurrentValue(SelectedItemProperty, null);
        SetCurrentValue(SelectedValueProperty, null);
        SetCurrentValue(SelectedPathProperty, null);
        SetCurrentValue(TextProperty, string.Empty);

        _pendingValue = null;
        _pendingPath = null;

        SetCurrentValue(IsOpenedProperty, false);
        _textBox?.Focus();

        CommandManager.InvalidateRequerySuggested();
    }

    private static void OnClearCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is CascadePicker p && !p.IsReadOnly)
        {
            p.Clear();
            e.Handled = true;
        }
    }

    private static void OnClearCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is CascadePicker p)
        {
            e.CanExecute = !p.IsReadOnly && (p.SelectedItem != null || !string.IsNullOrEmpty(p.Text));
        }
    }

    #endregion Commands

    #region Override Methods

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _clearButton?.Click -= OnClearButtonClick;

        _textBox = GetTemplateChild(PART_DisplayedTextBox) as TextBox;
        _popup = GetTemplateChild(PART_ItemsPopup) as Popup;
        _clearButton = GetTemplateChild(PART_ClearButton) as Button;
        _openToggleButton = GetTemplateChild(PART_OpenToggleButton) as ToggleButton;

        if (_clearButton != null)
        {
            _clearButton.Command = ClearCommand;
            _clearButton.CommandTarget = this;
            _clearButton.Click += OnClearButtonClick;
        }
    }

    /// <summary>
    /// 装配单个容器的 Header / ItemsSource binding。任意层级共用此方法——
    /// 顶层由本类的 PrepareContainerForItemOverride 调用，深层级由 CascadePickerItem 调用。
    /// </summary>
    internal void PrepareCascadeContainer(CascadePickerItem container, object item)
    {
        if (container == null || ReferenceEquals(container, item))
        {
            return;
        }

        // 注意：base.PrepareContainerForItemOverride 已经把 Header 默认设为 item 本身
        // （HeaderedItemsControl 标准行为，让数据驱动也能用 HeaderTemplate）。
        // 所以这里若有 NodeMemberPath / DisplayMemberPath，需要强制覆盖那个默认值——
        // 不能用 ReadLocalValue == UnsetValue 判断（base 已经设过本地值）。
        if (!string.IsNullOrEmpty(NodeMemberPath))
        {
            container.SetBinding(HeaderedItemsControl.HeaderProperty, new Binding(NodeMemberPath));
        }
        else if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            container.SetBinding(HeaderedItemsControl.HeaderProperty, new Binding(DisplayMemberPath));
        }

        // else: 不动，保留 base 设置的 Header = item，由 HeaderTemplate 渲染

        // ItemTemplate 用作 HeaderTemplate
        if (ItemTemplate != null && container.HeaderTemplate == null)
        {
            container.HeaderTemplate = ItemTemplate;
        }

        // 子集合 binding：用 BindingExpression 检测，避免与 HierarchicalDataTemplate 冲突
        if (!string.IsNullOrEmpty(ChildrenMemberPath)
            && container.ItemsSource == null
            && BindingOperations.GetBindingExpression(container, ItemsSourceProperty) == null)
        {
            container.SetBinding(ItemsSourceProperty, new Binding(ChildrenMemberPath));
        }
    }

    /// <inheritdoc />
    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusWithinChanged(e);

        // 焦点离开整个控件（含 Popup 子树）时关闭下拉
        if (!(bool)e.NewValue && IsOpened)
        {
            SetCurrentValue(IsOpenedProperty, false);
            SetFocusedItem(null);
        }
    }

    /// <inheritdoc />
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (IsReadOnly)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Down:
                if (!IsOpened)
                {
                    SetCurrentValue(IsOpenedProperty, true);
                }
                else
                {
                    NavigateToNextItem();
                }
                e.Handled = true;
                break;

            case Key.Up:
                if (IsOpened)
                {
                    NavigateToPreviousItem();
                    e.Handled = true;
                }
                break;

            case Key.Right:
                if (IsOpened && _focusedItem?.HasItems == true)
                {
                    ExpandFocusedItem();
                    e.Handled = true;
                }
                break;

            case Key.Left:
                if (IsOpened)
                {
                    CollapseFocusedItem();
                    e.Handled = true;
                }
                break;

            case Key.Enter:
                if (IsOpened && _focusedItem != null)
                {
                    SelectFocusedItem();
                    e.Handled = true;
                }
                break;

            case Key.Escape:
                if (IsOpened)
                {
                    SetCurrentValue(IsOpenedProperty, false);
                    _textBox?.Focus();
                    e.Handled = true;
                }
                break;

            case Key.Space:
                if (!IsOpened && _textBox?.IsFocused == true)
                {
                    SetCurrentValue(IsOpenedProperty, true);
                    e.Handled = true;
                }
                break;
        }
    }

    /// <inheritdoc />
    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        RebuildIndex();

        // 暂存的 value / path 在 ItemsSource 就绪后再尝试解析
        if (_pendingValue != null)
        {
            SyncFromSelectedValue(_pendingValue);
        }
        if (_pendingPath != null)
        {
            SyncFromSelectedPath(_pendingPath);
        }
    }

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item) => item is CascadePickerItem;

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride() => new CascadePickerItem();

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is CascadePickerItem container)
        {
            PrepareCascadeContainer(container, item);
        }
    }

    #endregion Override Methods

    #region Private Methods - Index

    private void RebuildIndex()
    {
        _byItem.Clear();
        _byValue.Clear();
        _byPath.Clear();
        _pathConflicts?.Clear();

        if (Items == null || Items.Count == 0)
        {
            return;
        }

        var ancestors = new List<object>();
        foreach (var item in Items)
        {
            IndexNode(item, ancestors);
        }

        if (_pathConflicts != null && _pathConflicts.Count > 0)
        {
            System.Diagnostics.Trace.TraceWarning(
                $"[CascadePicker] 检测到 {_pathConflicts.Count} 条重复路径，SelectedPath 反查仅命中首个：{string.Join(", ", _pathConflicts)}");
        }
    }

    /// <summary>
    /// 数据结构变化或关键属性（NodeMemberPath / ChildrenMemberPath / SelectedValuePath / Separator）变化时重建索引。
    /// </summary>
    private static void OnIndexAffectingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePicker p)
        {
            p.RebuildIndex();
        }
    }

    /// <summary>
    /// 反射读取属性，按 (类型, 路径) 缓存 PropertyInfo。仅支持单层属性名。
    /// </summary>
    private static object GetMemberValue(object item, string path)
    {
        if (item == null || string.IsNullOrEmpty(path))
        {
            return null;
        }

        var key = (item.GetType(), path);
        PropertyInfo prop;
        lock (_propertyCacheLock)
        {
            if (!_propertyCache.TryGetValue(key, out prop))
            {
                prop = item.GetType().GetProperty(path);
                _propertyCache[key] = prop;
            }
        }

        return prop?.GetValue(item);
    }

    private void IndexNode(object item, List<object> ancestors)
    {
        if (item == null)
        {
            return;
        }

        var nodeText = GetNodeText(item);
        var ancestorsArray = ancestors.ToArray();

        var info = new NodeInfo
        {
            Item = item,
            Ancestors = ancestorsArray,
            NodeText = nodeText,
            Path = BuildPathString(ancestorsArray, nodeText),
        };

        _byItem[item] = info;

        // SelectedValue 索引
        if (!string.IsNullOrEmpty(SelectedValuePath))
        {
            var value = GetMemberValue(item, SelectedValuePath);
            if (value != null && !_byValue.ContainsKey(value))
            {
                _byValue[value] = item;
            }
        }

        // 路径索引（冲突保留首个）
        if (!string.IsNullOrEmpty(info.Path))
        {
            if (_byPath.ContainsKey(info.Path))
            {
                (_pathConflicts ??= new List<string>()).Add(info.Path);
            }
            else
            {
                _byPath[info.Path] = item;
            }
        }

        // 递归子项：直接嵌套场景从 CascadePickerItem.Items 取，数据驱动场景从 ChildrenMemberPath 取
        IEnumerable children = null;
        if (item is CascadePickerItem cpItem && cpItem.HasItems)
        {
            children = cpItem.Items;
        }
        else if (!string.IsNullOrEmpty(ChildrenMemberPath))
        {
            children = GetMemberValue(item, ChildrenMemberPath) as IEnumerable;
        }

        if (children != null)
        {
            ancestors.Add(item);
            foreach (var child in children)
            {
                IndexNode(child, ancestors);
            }
            ancestors.RemoveAt(ancestors.Count - 1);
        }
    }

    private string BuildPathString(object[] ancestors, string nodeText)
    {
        if (ancestors.Length == 0)
        {
            return nodeText;
        }

        var parts = new string[ancestors.Length + 1];
        for (var i = 0; i < ancestors.Length; i++)
        {
            parts[i] = GetNodeText(ancestors[i]);
        }
        parts[ancestors.Length] = nodeText;
        return string.Join(Separator ?? "/", parts);
    }

    private string GetNodeText(object item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        // 直接嵌套场景：item 是 CascadePickerItem 容器本身，取它的 Header
        if (item is CascadePickerItem container)
        {
            return container.Header?.ToString() ?? string.Empty;
        }

        if (!string.IsNullOrEmpty(NodeMemberPath))
        {
            return GetMemberValue(item, NodeMemberPath)?.ToString() ?? string.Empty;
        }

        return item.ToString() ?? string.Empty;
    }

    #endregion Private Methods - Index

    #region Private Methods - Selection sync

    /// <summary>
    /// SelectedItem 改变时同步 SelectedValue / SelectedPath / Text。
    /// </summary>
    private void SyncFromSelectedItem(object item)
    {
        _isSyncing = true;
        try
        {
            if (item == null)
            {
                SetCurrentValue(SelectedValueProperty, null);
                SetCurrentValue(SelectedPathProperty, null);
                if (!_isSyncingFromText)                       // ← 新增判断
                {
                    SetCurrentValue(TextProperty, string.Empty);
                }

                _pendingValue = null;
                _pendingPath = null;
                return;
            }

            if (_byItem.TryGetValue(item, out var info))
            {
                if (!string.IsNullOrEmpty(SelectedValuePath))
                {
                    SetCurrentValue(SelectedValueProperty, GetMemberValue(item, SelectedValuePath));
                }
                SetCurrentValue(SelectedPathProperty, info.Path);
                SetCurrentValue(TextProperty, IsShowFullPath ? info.Path : info.NodeText);
                _pendingValue = null;
                _pendingPath = null;
            }
            else
            {
                // item 不在索引：可能是用户给了一个不在 ItemsSource 里的对象
                SetCurrentValue(TextProperty, GetNodeText(item));
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    /// <summary>
    /// SelectedValue 改变时反查 item。
    /// </summary>
    private void SyncFromSelectedValue(object value)
    {
        if (value == null)
        {
            _pendingValue = null;
            SetCurrentValue(SelectedItemProperty, null);
            return;
        }

        if (_byValue.TryGetValue(value, out var item))
        {
            _pendingValue = null;
            SetCurrentValue(SelectedItemProperty, item);
        }
        else
        {
            // ItemsSource 还没就绪或 SelectedValuePath 还没设——暂存
            _pendingValue = value;
        }
    }

    /// <summary>
    /// SelectedPath 改变时反查 item。
    /// </summary>
    private void SyncFromSelectedPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            _pendingPath = null;
            SetCurrentValue(SelectedItemProperty, null);
            return;
        }

        if (_byPath.TryGetValue(path, out var item))
        {
            _pendingPath = null;
            SetCurrentValue(SelectedItemProperty, item);
        }
        else
        {
            _pendingPath = path;
        }
    }

    private void UpdateText()
    {
        if (SelectedItem != null && _byItem.TryGetValue(SelectedItem, out var info))
        {
            SetCurrentValue(TextProperty, IsShowFullPath ? info.Path : info.NodeText);
        }
    }

    #endregion Private Methods - Selection sync

    #region Private Methods - Item click & keyboard

    private void OnChildItemClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not CascadePickerItem container || IsReadOnly)
        {
            return;
        }

        // 用容器的"直接 owner ItemsControl"取数据 item，
        // 这样无论是 CascadePicker 顶层还是嵌套的 CascadePickerItem 都能正确拿到数据
        var owner = ItemsControl.ItemsControlFromItemContainer(container);
        var item = owner?.ItemContainerGenerator.ItemFromContainer(container);

        if (item == null || item == DependencyProperty.UnsetValue)
        {
            // 直接嵌套场景：item 即容器本身
            item = container;
        }

        SetCurrentValue(SelectedItemProperty, item);

        if (!container.HasItems)
        {
            SetCurrentValue(IsOpenedProperty, false);
        }
    }

    private void SetFocusedItem(CascadePickerItem item)
    {
        if (_focusedItem != null)
        {
            _focusedItem.IsHighlighted = false;
        }

        _focusedItem = item;

        if (_focusedItem != null)
        {
            _focusedItem.IsHighlighted = true;
            _focusedItem.BringIntoView();
        }
    }

    private void NavigateToNextItem()
    {
        var visible = CollectVisibleContainers();
        if (visible.Count == 0)
        {
            return;
        }

        if (_focusedItem == null)
        {
            SetFocusedItem(visible[0]);
            return;
        }

        var idx = visible.IndexOf(_focusedItem);
        if (idx >= 0 && idx < visible.Count - 1)
        {
            SetFocusedItem(visible[idx + 1]);
        }
    }

    private void NavigateToPreviousItem()
    {
        var visible = CollectVisibleContainers();
        if (visible.Count == 0)
        {
            return;
        }

        if (_focusedItem == null)
        {
            SetFocusedItem(visible[visible.Count - 1]);
            return;
        }

        var idx = visible.IndexOf(_focusedItem);
        if (idx > 0)
        {
            SetFocusedItem(visible[idx - 1]);
        }
    }

    private void ExpandFocusedItem()
    {
        if (_focusedItem?.HasItems != true)
        {
            return;
        }

        _focusedItem.IsExpanded = true;
        if (_focusedItem.ItemContainerGenerator.ContainerFromIndex(0) is CascadePickerItem firstChild)
        {
            SetFocusedItem(firstChild);
        }
    }

    private void CollapseFocusedItem()
    {
        if (_focusedItem == null)
        {
            return;
        }

        // 当前项已展开 → 先折叠
        if (_focusedItem.IsExpanded)
        {
            _focusedItem.IsExpanded = false;
            return;
        }

        // 否则定位到父项；若已经在顶层，关闭整个下拉
        var parent = ItemsControl.ItemsControlFromItemContainer(_focusedItem) as CascadePickerItem;
        if (parent != null)
        {
            parent.IsExpanded = false;
            SetFocusedItem(parent);
        }
        else
        {
            SetCurrentValue(IsOpenedProperty, false);
            _textBox?.Focus();
        }
    }

    private void SelectFocusedItem()
    {
        if (_focusedItem == null)
        {
            return;
        }

        // 分支节点 → 仅展开（焦点转入子项）；叶子节点 → 选中并关闭
        if (_focusedItem.HasItems)
        {
            ExpandFocusedItem();
        }
        else
        {
            var owner = ItemsControl.ItemsControlFromItemContainer(_focusedItem);
            var item = owner?.ItemContainerGenerator.ItemFromContainer(_focusedItem);

            if (item == null || item == DependencyProperty.UnsetValue)
            {
                item = _focusedItem;
            }

            SetCurrentValue(SelectedItemProperty, item);
            SetCurrentValue(IsOpenedProperty, false);
            _textBox?.Focus();
        }
    }

    private List<CascadePickerItem> CollectVisibleContainers()
    {
        var list = new List<CascadePickerItem>();
        Walk(this, list);
        return list;

        static void Walk(ItemsControl host, List<CascadePickerItem> output)
        {
            for (var i = 0; i < host.Items.Count; i++)
            {
                if (host.ItemContainerGenerator.ContainerFromIndex(i) is not CascadePickerItem container)
                {
                    continue;
                }

                if (!container.IsVisible)
                {
                    continue;
                }

                output.Add(container);

                if (container.IsExpanded && container.HasItems)
                {
                    Walk(container, output);
                }
            }
        }
    }

    private void CollapseAllItems()
    {
        for (var i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is CascadePickerItem container)
            {
                CollapseRecursive(container);
            }
        }

        static void CollapseRecursive(CascadePickerItem item)
        {
            item.IsExpanded = false;
            item.IsHighlighted = false;
            for (var i = 0; i < item.Items.Count; i++)
            {
                if (item.ItemContainerGenerator.ContainerFromIndex(i) is CascadePickerItem child)
                {
                    CollapseRecursive(child);
                }
            }
        }
    }

    private void OnClearButtonClick(object sender, RoutedEventArgs e)
    {
        // 防止按钮点击冒泡触发其他处理；实际清空走 RoutedCommand
        e.Handled = true;
    }

    #endregion Private Methods - Item click & keyboard
}