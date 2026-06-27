using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// DataGrid 自动生成列特性。标在 ViewModel 的属性上，配合
/// hp:DataGridHelper.IsAutoGenerate=True 自动生成 DataGridColumn。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataGridPropertyAttribute : Attribute
{
    /// <summary>列标题。null 时用属性名。</summary>
    public string Header { get; set; }

    /// <summary>
    /// 列宽。CLR 特性参数不允许 DataGridLength 类型——用 double 加约定特殊值表达：
    ///   ·  0  (默认) → DataGridLength.Auto
    ///   ·  正数      → DataGridLength.Pixel(value) 固定像素宽
    ///   · -1         → DataGridLength.Star (即 1*)
    ///   · -2         → DataGridLength.SizeToCells
    ///   · -3         → DataGridLength.SizeToHeader
    /// 实际 DataGridLength 通过 GetWidthAsDataGridLength() 转换。
    /// </summary>
    public double Width { get; set; } = 0;

    /// <summary>显示顺序——数字越小越靠前。</summary>
    public int Index { get; set; } = int.MaxValue;

    /// <summary>格式化字符串（StringFormat）。</summary>
    public string StringFormat { get; set; }

    /// <summary>是否只读。属性本身不可写时强制只读。</summary>
    public bool IsReadOnly { get; set; }

    /// <summary>DataTemplate 的资源 key——优先于 DataTemplatePath。</summary>
    public string DataTemplateKey { get; set; }

    /// <summary>DataTemplate 所在资源字典 URI——次优先级。</summary>
    public string DataTemplatePath { get; set; }

    /// <summary>
    /// 把 Width 双精度值按约定转成 DataGridLength。
    /// </summary>
    internal DataGridLength GetWidthAsDataGridLength()
    {
        if (Width > 0)
        {
            return new DataGridLength(Width);
        }

        return Width switch
        {
            -1 => new DataGridLength(1, DataGridLengthUnitType.Star),
            -2 => DataGridLength.SizeToCells,
            -3 => DataGridLength.SizeToHeader,
            _ => DataGridLength.Auto,
        };
    }

    public DataGridPropertyAttribute(string header = null)
    {
        Header = header;
    }
}

/// <summary>
/// DataGrid 辅助附加属性。
///
/// 提供四个附加属性：
///   1. SelectedItems            (IList)   — 单向镜像 DataGrid 多选项到 ViewModel
///   2. IsAutoGenerate           (bool)    — 启用基于 [DataGridProperty] 特性的自动列生成
///   3. TextColumnEditingStyle   (Style)   — 给所有 DataGridTextColumn 统一应用编辑样式
///   4. DataGridPropertyAttribute          — 标在 ViewModel 属性上配合 IsAutoGenerate 用
///
/// 设计要点：
///   · SelectedItems 只把 DataGrid.SelectedItems 镜像到 VM 集合，不反向驱动 UI 选区
///   · 重入抑制 + 增量更新（用 e.AddedItems/RemovedItems 而非全量 Clear/Add）
///   · IsAutoGenerate 监听 ItemsSource 变化用 DependencyPropertyDescriptor，
///     必须在控件 Unloaded 时解绑——否则内存泄漏
///   · TextColumnEditingStyle 的 CollectionChanged 订阅用具名 handler，避免 lambda 无法解绑
/// </summary>
public static class DataGridHelper
{
    #region SelectedItems — 单向镜像多选项

    public static readonly DependencyProperty SelectedItemsProperty;

    public static IList GetSelectedItems(DependencyObject obj) =>
        (IList)obj.GetValue(SelectedItemsProperty);

    public static void SetSelectedItems(DependencyObject obj, IList value) =>
        obj.SetValue(SelectedItemsProperty, value);

    #endregion SelectedItems — 单向镜像多选项

    #region IsAutoGenerate — 基于特性自动生成列

    public static readonly DependencyProperty IsAutoGenerateProperty;

    public static bool GetIsAutoGenerate(DependencyObject obj) =>
        (bool)obj.GetValue(IsAutoGenerateProperty);

    public static void SetIsAutoGenerate(DependencyObject obj, bool value) =>
        obj.SetValue(IsAutoGenerateProperty, value);

    #endregion IsAutoGenerate — 基于特性自动生成列

    #region TextColumnEditingStyle — 文本列统一编辑样式

    public static readonly DependencyProperty TextColumnEditingStyleProperty;

    public static Style GetTextColumnEditingStyle(DependencyObject obj) =>
        (Style)obj.GetValue(TextColumnEditingStyleProperty);

    public static void SetTextColumnEditingStyle(DependencyObject obj, Style value) =>
        obj.SetValue(TextColumnEditingStyleProperty, value);

    #endregion TextColumnEditingStyle — 文本列统一编辑样式

    #region 内部状态 — 重入抑制 + 订阅追踪

    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached(
            "IsUpdating",
            typeof(bool),
            typeof(DataGridHelper),
            new PropertyMetadata(false));

    /// <summary>
    /// TextColumnEditingStyle 模式下监听 Columns 变化的具名 handler——存附加属性供解绑。
    /// </summary>
    private static readonly DependencyProperty ColumnsCollectionChangedHandlerProperty =
        DependencyProperty.RegisterAttached(
            "ColumnsCollectionChangedHandler",
            typeof(NotifyCollectionChangedEventHandler),
            typeof(DataGridHelper),
            new PropertyMetadata(null));

    /// <summary>
    /// IsAutoGenerate 模式下监听 ItemsSource 变化的 EventHandler——存附加属性供解绑。
    /// </summary>
    private static readonly DependencyProperty ItemsSourceChangedHandlerProperty =
        DependencyProperty.RegisterAttached(
            "ItemsSourceChangedHandler",
            typeof(EventHandler),
            typeof(DataGridHelper),
            new PropertyMetadata(null));

    private static bool GetIsUpdating(DependencyObject obj) =>
        (bool)obj.GetValue(IsUpdatingProperty);

    private static void SetIsUpdating(DependencyObject obj, bool value) =>
        obj.SetValue(IsUpdatingProperty, value);

    #endregion 内部状态 — 重入抑制 + 订阅追踪

    #region 静态构造 — 集中注册 DP

    static DataGridHelper()
    {
        SelectedItemsProperty = DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(DataGridHelper),
            new PropertyMetadata(null, OnSelectedItemsChanged));

        IsAutoGenerateProperty = DependencyProperty.RegisterAttached(
            "IsAutoGenerate",
            typeof(bool),
            typeof(DataGridHelper),
            new PropertyMetadata(false, OnIsAutoGenerateChanged));

        TextColumnEditingStyleProperty = DependencyProperty.RegisterAttached(
            "TextColumnEditingStyle",
            typeof(Style),
            typeof(DataGridHelper),
            new PropertyMetadata(null, OnTextColumnEditingStyleChanged));
    }

    #endregion 静态构造 — 集中注册 DP

    #region SelectedItems 单向镜像

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid)
        {
            return;
        }

        dataGrid.SelectionChanged -= OnDataGridSelectionChanged;

        if (e.NewValue is not IList newList)
        {
            return;
        }

        // DataGrid.SelectedItems 是 UI 选区的唯一状态源，VM 集合只作为结果镜像。
        SyncExternalListFromDataGrid(dataGrid, newList);
        dataGrid.SelectionChanged += OnDataGridSelectionChanged;
    }

    private static void OnDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid dataGrid)
        {
            return;
        }

        if (GetIsUpdating(dataGrid))
        {
            return;
        }

        var externalList = GetSelectedItems(dataGrid);
        if (externalList == null)
        {
            return;
        }

        SetIsUpdating(dataGrid, true);
        try
        {
            foreach (var item in e.RemovedItems)
            {
                externalList.Remove(item);
            }
            foreach (var item in e.AddedItems)
            {
                if (!externalList.Contains(item))
                {
                    externalList.Add(item);
                }
            }
        }
        finally
        {
            SetIsUpdating(dataGrid, false);
        }
    }

    private static void SyncExternalListFromDataGrid(DataGrid dataGrid, IList externalList)
    {
        if (GetIsUpdating(dataGrid))
        {
            return;
        }

        SetIsUpdating(dataGrid, true);
        try
        {
            externalList.Clear();
            foreach (var item in dataGrid.SelectedItems)
            {
                externalList.Add(item);
            }
        }
        finally
        {
            SetIsUpdating(dataGrid, false);
        }
    }

    #endregion SelectedItems 单向镜像

    #region IsAutoGenerate

    private static void OnIsAutoGenerateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid)
        {
            return;
        }

        // 解绑旧的 ItemsSource 监听
        var oldHandler = dataGrid.GetValue(ItemsSourceChangedHandlerProperty) as EventHandler;
        if (oldHandler != null)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(
                ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            dpd.RemoveValueChanged(dataGrid, oldHandler);
            dataGrid.SetValue(ItemsSourceChangedHandlerProperty, null);
        }

        if ((bool)e.NewValue)
        {
            // 启用：立即生成一次
            DataGridColumnManager.GenerateColumns(dataGrid);

            // 监听后续 ItemsSource 变化重新生成——用具名 handler 存附加属性
            EventHandler newHandler = (sender, args) =>
            {
                if (GetIsAutoGenerate(dataGrid))
                {
                    DataGridColumnManager.GenerateColumns(dataGrid);
                }
            };
            var dpd = DependencyPropertyDescriptor.FromProperty(
                ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            dpd.AddValueChanged(dataGrid, newHandler);
            dataGrid.SetValue(ItemsSourceChangedHandlerProperty, newHandler);

            // 关键：DataGrid Unloaded 时解绑——避免 DependencyPropertyDescriptor 静态字典持有强引用泄漏
            dataGrid.Unloaded += OnDataGridUnloaded;
        }
        else
        {
            dataGrid.Unloaded -= OnDataGridUnloaded;
        }
    }

    private static void OnDataGridUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is not DataGrid dataGrid)
        {
            return;
        }

        // 解绑 ItemsSource 监听
        var handler = dataGrid.GetValue(ItemsSourceChangedHandlerProperty) as EventHandler;
        if (handler != null)
        {
            var dpd = DependencyPropertyDescriptor.FromProperty(
                ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            dpd.RemoveValueChanged(dataGrid, handler);
            dataGrid.SetValue(ItemsSourceChangedHandlerProperty, null);
        }

        // 解绑 Columns 监听（来自 TextColumnEditingStyle）
        var columnsHandler = dataGrid.GetValue(ColumnsCollectionChangedHandlerProperty) as NotifyCollectionChangedEventHandler;
        if (columnsHandler != null)
        {
            ((INotifyCollectionChanged)dataGrid.Columns).CollectionChanged -= columnsHandler;
            dataGrid.SetValue(ColumnsCollectionChangedHandlerProperty, null);
        }

        dataGrid.Unloaded -= OnDataGridUnloaded;
    }

    #endregion IsAutoGenerate

    #region TextColumnEditingStyle

    private static void OnTextColumnEditingStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid)
        {
            return;
        }

        // 解绑旧的 Columns 监听——具名 handler 才能正确 -=
        var oldHandler = dataGrid.GetValue(ColumnsCollectionChangedHandlerProperty) as NotifyCollectionChangedEventHandler;
        if (oldHandler != null)
        {
            ((INotifyCollectionChanged)dataGrid.Columns).CollectionChanged -= oldHandler;
            dataGrid.SetValue(ColumnsCollectionChangedHandlerProperty, null);
        }

        if (e.NewValue is Style)
        {
            // 立即应用一次
            ApplyEditingStyle(dataGrid);

            // 监听 Columns 变化——闭包 capture dataGrid，handler 实例存附加属性供解绑
            NotifyCollectionChangedEventHandler newHandler = (sender, args) => ApplyEditingStyle(dataGrid);
            ((INotifyCollectionChanged)dataGrid.Columns).CollectionChanged += newHandler;
            dataGrid.SetValue(ColumnsCollectionChangedHandlerProperty, newHandler);

            // Unloaded 时解绑（如果 IsAutoGenerate 没挂上）
            dataGrid.Unloaded -= OnDataGridUnloaded;
            dataGrid.Unloaded += OnDataGridUnloaded;
        }
    }

    private static void ApplyEditingStyle(DataGrid dataGrid)
    {
        var style = GetTextColumnEditingStyle(dataGrid);
        if (style == null)
        {
            return;
        }

        foreach (var column in dataGrid.Columns)
        {
            if (column is DataGridTextColumn textColumn)
            {
                textColumn.EditingElementStyle = style;
            }
        }
    }

    #endregion TextColumnEditingStyle

    #region 内部列管理

    private static class DataGridColumnManager
    {
        public static void GenerateColumns(DataGrid dataGrid)
        {
            dataGrid.Columns.Clear();

            var itemType = GetItemSourceType(dataGrid);
            if (itemType == null)
            {
                return;
            }

            var properties = GetPropertiesWithAttribute(itemType);
            foreach (var (propertyInfo, attribute) in properties.OrderBy(p => p.Attribute.Index))
            {
                var column = CreateColumn(propertyInfo, attribute);
                if (column != null)
                {
                    dataGrid.Columns.Add(column);
                }
            }
        }

        private static Type GetItemSourceType(DataGrid dataGrid)
        {
            if (dataGrid.ItemsSource is not IEnumerable enumerable)
            {
                return null;
            }

            // 优先取第一个非 null 元素的运行时类型——支持继承场景
            foreach (var item in enumerable)
            {
                if (item != null)
                {
                    return item.GetType();
                }
                break;
            }

            // 集合空时退回到泛型参数
            var collectionType = dataGrid.ItemsSource.GetType();
            if (collectionType.IsGenericType)
            {
                var genericArgs = collectionType.GetGenericArguments();
                if (genericArgs.Length > 0)
                {
                    return genericArgs[0];
                }
            }

            return null;
        }

        private static List<(PropertyInfo PropertyInfo, DataGridPropertyAttribute Attribute)> GetPropertiesWithAttribute(Type type)
        {
            var result = new List<(PropertyInfo, DataGridPropertyAttribute)>();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<DataGridPropertyAttribute>();
                if (attribute != null)
                {
                    result.Add((property, attribute));
                }
            }
            return result;
        }

        private static DataGridColumn CreateColumn(PropertyInfo property, DataGridPropertyAttribute attribute)
        {
            // 模板列优先
            if (!string.IsNullOrEmpty(attribute.DataTemplateKey) || !string.IsNullOrEmpty(attribute.DataTemplatePath))
            {
                return CreateTemplateColumn(property, attribute);
            }

            // 默认文本列
            var binding = new Binding(property.Name);
            if (!string.IsNullOrEmpty(attribute.StringFormat))
            {
                binding.StringFormat = attribute.StringFormat;
            }

            return new DataGridTextColumn
            {
                Header = attribute.Header ?? property.Name,
                Width = attribute.GetWidthAsDataGridLength(),
                IsReadOnly = !property.CanWrite || attribute.IsReadOnly,
                Binding = binding,
            };
        }

        private static DataGridColumn CreateTemplateColumn(PropertyInfo property, DataGridPropertyAttribute attribute)
        {
            var templateColumn = new DataGridTemplateColumn
            {
                Header = attribute.Header ?? property.Name,
                Width = attribute.GetWidthAsDataGridLength(),
                IsReadOnly = !property.CanWrite || attribute.IsReadOnly,
            };

            DataTemplate template = null;

            if (!string.IsNullOrEmpty(attribute.DataTemplateKey))
            {
                if (Application.Current?.Resources?.Contains(attribute.DataTemplateKey) == true)
                {
                    template = Application.Current.Resources[attribute.DataTemplateKey] as DataTemplate;
                }
            }
            else if (!string.IsNullOrEmpty(attribute.DataTemplatePath))
            {
                try
                {
                    var resourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri(attribute.DataTemplatePath, UriKind.RelativeOrAbsolute)
                    };
                    foreach (var key in resourceDictionary.Keys)
                    {
                        if (resourceDictionary[key] is DataTemplate dt)
                        {
                            template = dt;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[DataGridHelper] 加载数据模板失败: {attribute.DataTemplatePath}, 错误: {ex.Message}");
                }
            }

            if (template == null)
            {
                // 找不到模板——日志警告，不再静默 fallback 到无意义的 TextBlock
                System.Diagnostics.Debug.WriteLine(
                    $"[DataGridHelper] 属性 {property.Name} 指定了模板 (Key={attribute.DataTemplateKey}, Path={attribute.DataTemplatePath}) 但未找到。请检查资源是否注册或路径是否正确。");
                return null;
            }

            templateColumn.CellTemplate = template;
            return templateColumn;
        }
    }

    #endregion 内部列管理
}