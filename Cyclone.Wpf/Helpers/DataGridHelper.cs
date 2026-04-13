using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cyclone.Wpf.Helpers;

[AttributeUsage(AttributeTargets.Property)]
public class DataGridPropertyAttribute : Attribute
{
    // 列标题
    public string Header { get; set; }

    // 列宽度
    public DataGridLength Width { get; set; } = DataGridLength.Auto;

    // 显示顺序，数字越小越靠前
    public int Index { get; set; } = int.MaxValue;

    // 格式化字符串
    public string StringFormat { get; set; }

    // 是否只读
    public bool IsReadOnly { get; set; } = false;

    // 模板资源路径
    public string DataTemplatePath { get; set; }

    // 模板资源key
    public string DataTemplateKey { get; set; }

    // 构造函数
    public DataGridPropertyAttribute(string header = null)
    {
        Header = header;
    }
}

public class DataGridHelper
{
    #region SelectedItems

    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(DataGridHelper),
                    new FrameworkPropertyMetadata(default(IList), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public static IList GetSelectedItems(DataGrid obj) => (IList)obj.GetValue(SelectedItemsProperty);

    public static void SetSelectedItems(DataGrid obj, IList value) => obj.SetValue(SelectedItemsProperty, value);

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGrid dataGrid)
        {
            dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;
        }
    }

    private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid)
        {
            var selectedItems = GetSelectedItems(dataGrid);
            if (selectedItems == null || dataGrid.SelectedItems == null) { return; }
            selectedItems.Clear();
            foreach (var item in dataGrid.SelectedItems)
            {
                selectedItems.Add(item);
            }
        }
    }

    #endregion SelectedItems

    #region IsAutoGenerate

    public static readonly DependencyProperty IsAutoGenerateProperty =
                DependencyProperty.RegisterAttached("IsAutoGenerate", typeof(bool), typeof(DataGridHelper),
                    new PropertyMetadata(false, OnIsAutoGenerateChanged));

    public static bool GetIsAutoGenerate(DependencyObject obj) => (bool)obj.GetValue(IsAutoGenerateProperty);

    public static void SetIsAutoGenerate(DependencyObject obj, bool value) => obj.SetValue(IsAutoGenerateProperty, value);

    private static void OnIsAutoGenerateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGrid dataGrid && e.NewValue is bool isAutoGenerate)
        {
            if (isAutoGenerate)
            {
                // 使用 DataGridColumnManager 来处理列自动生成
                DataGridColumnManager.GenerateColumns(dataGrid);

                // 监听 ItemsSource 属性变化
                var dpd = DependencyPropertyDescriptor.FromProperty(
                    ItemsControl.ItemsSourceProperty, typeof(DataGrid));

                // 移除已存在的事件处理器（如果有）
                dpd.RemoveValueChanged(dataGrid, DataGrid_ItemsSourceChanged);

                // 添加新的事件处理器
                dpd.AddValueChanged(dataGrid, DataGrid_ItemsSourceChanged);
            }
            else
            {
                // 如果关闭自动生成，移除事件处理器
                var dpd = DependencyPropertyDescriptor.FromProperty(
                    ItemsControl.ItemsSourceProperty, typeof(DataGrid));
                dpd.RemoveValueChanged(dataGrid, DataGrid_ItemsSourceChanged);
            }
        }
    }

    private static void DataGrid_ItemsSourceChanged(object sender, EventArgs e)
    {
        if (sender is DataGrid dataGrid && GetIsAutoGenerate(dataGrid))
        {
            // 如果数据源改变并且自动生成已启用，重新生成列
            DataGridColumnManager.GenerateColumns(dataGrid);
        }
    }

    #endregion IsAutoGenerate

    #region TextColumnEditingStyle

    public static readonly DependencyProperty TextColumnEditingStyleProperty =
        DependencyProperty.RegisterAttached(
            "TextColumnEditingStyle",
            typeof(Style),
            typeof(DataGridHelper),
            new PropertyMetadata(null, OnTextColumnEditingStyleChanged));

    public static Style GetTextColumnEditingStyle(DependencyObject obj)
            => (Style)obj.GetValue(TextColumnEditingStyleProperty);

    public static void SetTextColumnEditingStyle(DependencyObject obj, Style value)
        => obj.SetValue(TextColumnEditingStyleProperty, value);

    private static void OnTextColumnEditingStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid dataGrid) return;

        if (e.NewValue is Style)
        {
            // 对已有的列立即应用
            ApplyEditingStyle(dataGrid);

            // 监听后续添加的列
            ((INotifyCollectionChanged)dataGrid.Columns).CollectionChanged
                += (_, _) => ApplyEditingStyle(dataGrid);
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

    #region 内部列管理类

    /// <summary>
    /// 内部私有类，用于管理DataGrid列的生成
    /// </summary>
    private static class DataGridColumnManager
    {
        /// <summary>
        /// 生成DataGrid列
        /// </summary>
        public static void GenerateColumns(DataGrid dataGrid)
        {
            // 清空原有列
            dataGrid.Columns.Clear();

            // 获取数据源类型
            Type itemType = GetItemSourceType(dataGrid);
            if (itemType == null)
                return;

            // 获取所有带有 DataGridColumnAttribute 特性的属性
            var properties = GetPropertiesWithDataGridColumnAttribute(itemType);

            // 按 Index 排序并创建列
            foreach (var property in properties.OrderBy(p => p.Attribute.Index))
            {
                var column = CreateColumn(property.PropertyInfo, property.Attribute);
                dataGrid.Columns.Add(column);
            }
        }

        private static Type GetItemSourceType(DataGrid dataGrid)
        {
            if (dataGrid.ItemsSource == null)
                return null;

            // 尝试从 ItemsSource 获取元素类型
            var enumerable = dataGrid.ItemsSource as IEnumerable;
            if (enumerable == null)
                return null;

            // 尝试获取第一个元素来确定类型
            foreach (var item in enumerable)
            {
                if (item != null)
                    return item.GetType();

                break;
            }

            // 如果集合为空，尝试从集合类型获取元素类型
            Type collectionType = dataGrid.ItemsSource.GetType();

            // 检查是否为泛型集合
            if (collectionType.IsGenericType)
            {
                Type[] genericArgs = collectionType.GetGenericArguments();
                if (genericArgs.Length > 0)
                    return genericArgs[0];
            }

            return null;
        }

        private static List<(PropertyInfo PropertyInfo, DataGridPropertyAttribute Attribute)> GetPropertiesWithDataGridColumnAttribute(Type type)
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
            // 默认创建文本列
            DataGridTextColumn column = new DataGridTextColumn();

            // 设置绑定
            Binding binding = new Binding(property.Name);

            // 设置格式化字符串
            if (!string.IsNullOrEmpty(attribute.StringFormat))
            {
                binding.StringFormat = attribute.StringFormat;
            }

            column.Binding = binding;

            // 设置标题
            column.Header = attribute.Header ?? property.Name;

            // 设置宽度
            column.Width = attribute.Width;

            // 设置只读状态
            // 如果属性本身不可写或者特性设置为只读，则列为只读
            bool isPropertyReadOnly = !property.CanWrite;
            column.IsReadOnly = isPropertyReadOnly || attribute.IsReadOnly;

            // TODO: 实现自定义模板的支持
            // 如果指定了模板，需要创建 DataGridTemplateColumn
            if (!string.IsNullOrEmpty(attribute.DataTemplateKey) || !string.IsNullOrEmpty(attribute.DataTemplatePath))
            {
                return CreateTemplateColumn(property, attribute);
            }

            return column;
        }

        private static DataGridColumn CreateTemplateColumn(PropertyInfo property, DataGridPropertyAttribute attribute)
        {
            // 检查属性是否可写
            bool isPropertyReadOnly = !property.CanWrite;

            // 创建模板列
            var templateColumn = new DataGridTemplateColumn
            {
                Header = attribute.Header ?? property.Name,
                Width = attribute.Width,
                IsReadOnly = isPropertyReadOnly || attribute.IsReadOnly
            };

            // 创建单元格模板 (CellTemplate)
            if (!string.IsNullOrEmpty(attribute.DataTemplateKey))
            {
                // 通过 Key 获取资源模板
                if (Application.Current.Resources.Contains(attribute.DataTemplateKey))
                {
                    var template = Application.Current.Resources[attribute.DataTemplateKey] as DataTemplate;
                    if (template != null)
                    {
                        templateColumn.CellTemplate = template;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(attribute.DataTemplatePath))
            {
                // 尝试通过路径加载资源字典并获取模板
                try
                {
                    var resourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri(attribute.DataTemplatePath, UriKind.RelativeOrAbsolute)
                    };

                    // 查找第一个 DataTemplate 资源
                    foreach (var key in resourceDictionary.Keys)
                    {
                        if (resourceDictionary[key] is DataTemplate template)
                        {
                            templateColumn.CellTemplate = template;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 记录异常，但不中断程序执行
                    System.Diagnostics.Debug.WriteLine($"加载数据模板失败: {attribute.DataTemplatePath}, 错误: {ex.Message}");
                }
            }

            // 绑定属性值
            if (templateColumn.CellTemplate == null)
            {
                // 如果找不到模板，创建一个默认的绑定
                var factory = new FrameworkElementFactory(typeof(TextBlock));
                var binding = new Binding(property.Name);

                if (!string.IsNullOrEmpty(attribute.StringFormat))
                {
                    binding.StringFormat = attribute.StringFormat;
                }

                factory.SetBinding(TextBlock.TextProperty, binding);

                var template = new DataTemplate
                {
                    VisualTree = factory
                };

                templateColumn.CellTemplate = template;
            }

            return templateColumn;
        }
    }

    #endregion 内部列管理类
}