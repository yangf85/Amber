using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 继承自 <see cref="ComboBox"/> 的智能提示框控件。<br/>
/// IsEditable 强制为 true，IsTextSearchEnabled 强制为 false——本控件用<b>独立的 ListCollectionView 过滤</b>取代
/// 内置的 IsTextSearchEnabled 高亮匹配。<br/>
/// 内部包装 ItemsSource 为 <see cref="ListCollectionView"/>，<b>不污染</b> source 的默认 view——
/// 同一个 ObservableCollection 可以同时绑给多个 HintBox / 其它控件而不互相影响 filter。
/// </summary>
[TemplatePart(Name = PartEditableTextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PartPopup, Type = typeof(Popup))]
public class HintBox : ComboBox
{
    private const string PartEditableTextBox = "PART_EditableTextBox";

    private const string PartPopup = "PART_Popup";

    /// <summary>反射缓存：避免 filter 期间每个 item 都反射查 PropertyInfo。</summary>
    private static readonly ConcurrentDictionary<(Type Type, string Path), PropertyInfo[]> _propertyCache
        = new ConcurrentDictionary<(Type Type, string Path), PropertyInfo[]>();

    private TextBox _editableTextBox;

    private string _currentFilter = string.Empty;

    private ListCollectionView _filterView;

    /// <summary>
    /// filter 路径主动清 SelectedItem 时设 true——OnSelectionChanged 跳过 Text 同步。
    /// 否则会出现"用户输入 'Applex' → 清 SelectedItem → Text 重置为空 → 用户输入消失"。
    /// </summary>
    private bool _suppressTextSync;

    static HintBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HintBox),
            new FrameworkPropertyMetadata(typeof(HintBox)));

        // 重写默认行为
        IsEditableProperty.OverrideMetadata(typeof(HintBox),
            new FrameworkPropertyMetadata(true));
        IsTextSearchEnabledProperty.OverrideMetadata(typeof(HintBox),
            new FrameworkPropertyMetadata(false));
    }

    public HintBox()
    {
        IsEditable = true;
        IsTextSearchEnabled = false;
        CommandBindings.Add(new CommandBinding(ClearTextCommand, OnClearTextCommand, OnCanClearTextCommand));
    }

    #region SearchMemberPath

    public static readonly DependencyProperty SearchMemberPathProperty =
        DependencyProperty.Register(
            nameof(SearchMemberPath),
            typeof(string),
            typeof(HintBox),
            new PropertyMetadata(null, OnSearchMemberPathChanged));

    /// <summary>
    /// 用于搜索匹配的属性路径。支持嵌套（"a.b.c"）。<br/>
    /// 没设 → 退到 <see cref="ItemsControl.DisplayMemberPath"/>；都没设 → 用 <see cref="object.ToString"/>。
    /// </summary>
    public string SearchMemberPath
    {
        get => (string)GetValue(SearchMemberPathProperty);
        set => SetValue(SearchMemberPathProperty, value);
    }

    private static void OnSearchMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((HintBox)d).RefreshFilter();
    }

    #endregion SearchMemberPath

    #region StringComparison

    public static readonly DependencyProperty StringComparisonProperty =
        DependencyProperty.Register(
            nameof(StringComparison),
            typeof(StringComparison),
            typeof(HintBox),
            new PropertyMetadata(StringComparison.OrdinalIgnoreCase, OnStringComparisonChanged));

    /// <summary>用于搜索匹配的字符串比较模式。默认 <see cref="System.StringComparison.OrdinalIgnoreCase"/>。</summary>
    public StringComparison StringComparison
    {
        get => (StringComparison)GetValue(StringComparisonProperty);
        set => SetValue(StringComparisonProperty, value);
    }

    private static void OnStringComparisonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((HintBox)d).RefreshFilter();
    }

    #endregion StringComparison

    #region ClearTextCommand

    public static readonly RoutedCommand ClearTextCommand =
        new RoutedCommand(nameof(ClearTextCommand), typeof(HintBox));

    private void OnClearTextCommand(object sender, ExecutedRoutedEventArgs e)
    {
        _suppressTextSync = true;
        try
        {
            SelectedItem = null;
        }
        finally
        {
            _suppressTextSync = false;
        }

        if (_editableTextBox != null)
        {
            _editableTextBox.Clear();
            _editableTextBox.Focus();
        }
        _currentFilter = string.Empty;
        RefreshFilter();
    }

    private void OnCanClearTextCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = _editableTextBox != null && _editableTextBox.Text.Length > 0;
    }

    #endregion ClearTextCommand

    #region Override - ItemsSource (自管 ListCollectionView)

    /// <summary>
    /// 用独立的 <see cref="ListCollectionView"/> 包装 source——不污染 source 的默认 view，
    /// 多控件共享同一个 source 时各自的 filter 互不影响。
    /// </summary>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);

        // 防循环：newValue 是我们自己创建的 view → 跳过
        if (newValue is ListCollectionView own && ReferenceEquals(own, _filterView))
        {
            return;
        }

        if (newValue == null)
        {
            _filterView = null;
            return;
        }

        // 创建独立 ListCollectionView。
        // 如果 source 实现 IList + INotifyCollectionChanged，view 会自动同步集合变化（监听弱事件）。
        if (newValue is IList list)
        {
            _filterView = new ListCollectionView(list) { Filter = FilterPredicate };
        }
        else
        {
            // 不是 IList：复制一份（罕见，集合变化无法回填）
            var copy = new List<object>();
            foreach (var item in newValue)
            {
                copy.Add(item);
            }
            _filterView = new ListCollectionView(copy) { Filter = FilterPredicate };
            Trace.TraceWarning("[HintBox] ItemsSource 不是 IList——后续集合变化不会同步到下拉。建议绑 ObservableCollection 或 List<T>。");
        }

        // 把 base.ItemsSource 替换成 view。SetCurrentValue 不破坏 user binding。
        SetCurrentValue(ItemsSourceProperty, _filterView);
    }

    #endregion Override - ItemsSource (自管 ListCollectionView)

    #region Override - Template / Selection / Keyboard

    public override void OnApplyTemplate()
    {
        if (_editableTextBox != null)
        {
            _editableTextBox.TextChanged -= OnEditableTextChanged;
        }

        base.OnApplyTemplate();

        _editableTextBox = GetTemplateChild(PartEditableTextBox) as TextBox;
        if (_editableTextBox != null)
        {
            _editableTextBox.TextChanged += OnEditableTextChanged;
        }
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        // filter 路径触发的 selection clear → 不动 Text，保留用户输入
        if (_suppressTextSync)
        {
            return;
        }

        var displayText = GetItemText(SelectedItem);
        if (Text != displayText)
        {
            Text = displayText;
        }
    }

    /// <summary>Esc 关闭 dropdown。Up/Down/Enter 走 base ComboBox 的内置处理。</summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled)
        {
            return;
        }

        if (e.Key == Key.Escape && IsDropDownOpen)
        {
            IsDropDownOpen = false;
            e.Handled = true;
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new HintBoxItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is HintBoxItem;
    }

    /// <summary>
    /// 监听 TextChanged 而不是 PreviewTextInput——后者读到的是 Text 更新前的值，
    /// 而且不能捕获退格 / Delete / Cut / Paste。TextChanged 在所有变化后触发，且 Text 已是最新值。
    /// </summary>
    private void OnEditableTextChanged(object sender, TextChangedEventArgs e)
    {
        var text = _editableTextBox?.Text ?? string.Empty;

        // 区分"selection 同步触发的 TextChanged"和"用户输入触发的 TextChanged"。
        // 前者：text 等于当前 SelectedItem 的显示文本——不当作 filter 输入。
        if (SelectedItem != null && string.Equals(text, GetItemText(SelectedItem), StringComparison))
        {
            _currentFilter = text;
            return;
        }

        if (text == _currentFilter)
        {
            return;
        }

        _currentFilter = text;

        // 用户改了 text，selection 不再匹配——清掉。
        // 用 _suppressTextSync 阻止 OnSelectionChanged 把 Text 重置为空（会清空用户的输入）
        if (SelectedItem != null)
        {
            _suppressTextSync = true;
            try
            {
                SelectedItem = null;
            }
            finally
            {
                _suppressTextSync = false;
            }
        }

        if (!IsDropDownOpen && !string.IsNullOrEmpty(text))
        {
            IsDropDownOpen = true;
        }

        RefreshFilter();
    }

    #endregion Override - Template / Selection / Keyboard

    #region Private - Filter

    private void RefreshFilter()
    {
        _filterView?.Refresh();
    }

    private bool FilterPredicate(object item)
    {
        if (string.IsNullOrEmpty(_currentFilter) || item == null)
        {
            return true;
        }

        var searchText = GetSearchText(item);
        if (string.IsNullOrEmpty(searchText))
        {
            return false;
        }

        return searchText.IndexOf(_currentFilter, StringComparison) >= 0;
    }

    #endregion Private - Filter

    #region Private - Property Resolution (with cache)

    /// <summary>
    /// 解析嵌套属性路径（"a.b.c"）为 PropertyInfo 链。按 (Type, path) 缓存——
    /// 避免 filter 期间每个 item 都反射 GetProperty。
    /// </summary>
    private static PropertyInfo[] ResolvePropertyChain(Type rootType, string propertyPath)
    {
        return _propertyCache.GetOrAdd((rootType, propertyPath), key =>
        {
            var (type, path) = key;
            var names = path.Split('.');
            var chain = new PropertyInfo[names.Length];
            var current = type;

            for (int i = 0; i < names.Length; i++)
            {
                if (current == null)
                {
                    return Array.Empty<PropertyInfo>();
                }

                var prop = current.GetProperty(names[i]);
                if (prop == null)
                {
                    return Array.Empty<PropertyInfo>();
                }

                chain[i] = prop;
                current = prop.PropertyType;
            }

            return chain;
        });
    }

    private string GetItemText(object item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        // DisplayMemberPath 优先；不设时直接 ToString()——不再 fallback 到 SearchMemberPath
        // （搜索字段不应该兼任显示字段——语义混乱）
        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            return GetPropertyValue(item, DisplayMemberPath)?.ToString() ?? string.Empty;
        }

        return item.ToString() ?? string.Empty;
    }

    private string GetSearchText(object item)
    {
        if (item == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(SearchMemberPath))
        {
            return GetPropertyValue(item, SearchMemberPath)?.ToString() ?? string.Empty;
        }

        // 没设 SearchMemberPath → 用 DisplayMemberPath 或 ToString
        return GetItemText(item);
    }

    private object GetPropertyValue(object obj, string propertyPath)
    {
        if (obj == null || string.IsNullOrEmpty(propertyPath))
        {
            return null;
        }

        try
        {
            var chain = ResolvePropertyChain(obj.GetType(), propertyPath);
            object current = obj;
            foreach (var prop in chain)
            {
                if (current == null)
                {
                    return null;
                }
                current = prop.GetValue(current);
            }
            return current;
        }
        catch
        {
            return null;
        }
    }

    #endregion Private - Property Resolution (with cache)
}