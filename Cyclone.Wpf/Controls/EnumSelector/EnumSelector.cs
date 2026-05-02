using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// EnumSelector 控件：基于枚举类型的单选/多选选择器。
/// 普通枚举默认渲染为 RadioButton（单选），Flags 枚举默认渲染为 CheckBox（多选）。
/// 通过 <see cref="DisplayMode"/> 可强制切换视觉模式。
/// 支持 <see cref="DescriptionAttribute"/> 提供中文/别名显示。
/// </summary>
[TemplatePart(Name = PART_ItemsHost, Type = typeof(Panel))]
public class EnumSelector : Selector
{
    private const string PART_ItemsHost = "PART_ItemsHost";

    /// <summary>所有枚举值的 ulong 数值映射缓存：value → 该 value 的所有"非零真子集"。在 EnumType 变化时重算一次。</summary>
    private Dictionary<ulong, List<ulong>> _enumValueMap;

    /// <summary>用于阻止 SelectedEnum / SelectedItem / EnumObject.IsSelected 三个同步路径之间的循环更新。</summary>
    private bool _isSyncing;

    /// <summary>每个 EnumSelector 实例独立的 RadioButton 分组名。两个 EnumSelector 同时使用 RadioButton 模式时不会互相干扰。</summary>
    private readonly string _radioButtonGroupName;

    static EnumSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(EnumSelector),
            new FrameworkPropertyMetadata(typeof(EnumSelector)));
    }

    public EnumSelector()
    {
        _radioButtonGroupName = $"EnumSelector_{Guid.NewGuid():N}";
    }

    #region EnumType

    public static readonly DependencyProperty EnumTypeProperty =
        DependencyProperty.Register(
            nameof(EnumType),
            typeof(Type),
            typeof(EnumSelector),
            new FrameworkPropertyMetadata(default(Type), OnEnumTypeChanged));

    /// <summary>
    /// 获取或设置要显示的枚举类型。
    /// </summary>
    public Type EnumType
    {
        get => (Type)GetValue(EnumTypeProperty);
        set => SetValue(EnumTypeProperty, value);
    }

    private static void OnEnumTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EnumSelector selector)
        {
            selector.RebuildItems();
        }
    }

    #endregion EnumType

    #region SelectedEnum

    public static readonly DependencyProperty SelectedEnumProperty =
        DependencyProperty.Register(
            nameof(SelectedEnum),
            typeof(Enum),
            typeof(EnumSelector),
            new FrameworkPropertyMetadata(
                default(Enum),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedEnumChanged));

    /// <summary>
    /// 获取或设置当前选中的枚举值。Flags 枚举下表示选中位的按位或；非 Flags 枚举下表示单选项。
    /// 设为 null 表示清空选择。
    /// </summary>
    public Enum SelectedEnum
    {
        get => (Enum)GetValue(SelectedEnumProperty);
        set => SetValue(SelectedEnumProperty, value);
    }

    private static void OnSelectedEnumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EnumSelector selector && !selector._isSyncing)
        {
            selector.SyncItemsFromSelectedEnum();
        }
    }

    #endregion SelectedEnum

    #region IsUseAlias

    public static readonly DependencyProperty IsUseAliasProperty =
        DependencyProperty.Register(
            nameof(IsUseAlias),
            typeof(bool),
            typeof(EnumSelector),
            new FrameworkPropertyMetadata(true, OnIsUseAliasChanged));

    /// <summary>
    /// 获取或设置是否使用 <see cref="DescriptionAttribute"/> 中的别名作为显示文本。
    /// 默认 true。
    /// </summary>
    public bool IsUseAlias
    {
        get => (bool)GetValue(IsUseAliasProperty);
        set => SetValue(IsUseAliasProperty, value);
    }

    private static void OnIsUseAliasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EnumSelector selector && selector.ItemsSource is IEnumerable<EnumItem> items)
        {
            var newValue = (bool)e.NewValue;
            foreach (var item in items)
            {
                item.IsUseAlias = newValue;
            }
        }
    }

    #endregion IsUseAlias

    #region DisplayMode

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(
            nameof(DisplayMode),
            typeof(EnumDisplayMode),
            typeof(EnumSelector),
            new FrameworkPropertyMetadata(EnumDisplayMode.Auto, OnDisplayModeChanged));

    /// <summary>
    /// 获取或设置显示模式：Auto（自动）/ RadioButton（强制单选视觉）/ CheckBox（强制多选视觉）。
    /// 注意：非 Flags 枚举即使强制 CheckBox 视觉，行为上仍为单选——避免产生无效的位组合枚举值。
    /// </summary>
    public EnumDisplayMode DisplayMode
    {
        get => (EnumDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EnumSelector selector)
        {
            selector.UpdateEffectiveDisplayMode();
            // 模式切换可能导致 IsSelected 状态语义变化（多选→单选时多个选中变得不合法），
            // 用 SelectedEnum 强制刷新一次，让 UI 状态和值保持一致。
            selector.SyncItemsFromSelectedEnum();
        }
    }

    #endregion DisplayMode

    #region HasFlags (只读)

    private static readonly DependencyPropertyKey HasFlagsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasFlags),
            typeof(bool),
            typeof(EnumSelector),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasFlagsProperty = HasFlagsPropertyKey.DependencyProperty;

    /// <summary>
    /// 获取当前 <see cref="EnumType"/> 是否标记了 <see cref="FlagsAttribute"/>。只读。
    /// </summary>
    public bool HasFlags
    {
        get => (bool)GetValue(HasFlagsProperty);
        private set => SetValue(HasFlagsPropertyKey, value);
    }

    #endregion HasFlags

    #region EffectiveDisplayMode (只读)

    private static readonly DependencyPropertyKey EffectiveDisplayModePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(EffectiveDisplayMode),
            typeof(EnumDisplayMode),
            typeof(EnumSelector),
            new PropertyMetadata(EnumDisplayMode.RadioButton));

    public static readonly DependencyProperty EffectiveDisplayModeProperty = EffectiveDisplayModePropertyKey.DependencyProperty;

    /// <summary>
    /// 获取实际渲染时使用的显示模式（解析 <see cref="DisplayMode"/> 的 Auto 后得到的具体值）。只读。
    /// </summary>
    public EnumDisplayMode EffectiveDisplayMode
    {
        get => (EnumDisplayMode)GetValue(EffectiveDisplayModeProperty);
        private set => SetValue(EffectiveDisplayModePropertyKey, value);
    }

    #endregion EffectiveDisplayMode

    #region Rows

    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.Register(
            nameof(Rows),
            typeof(int),
            typeof(EnumSelector),
            new FrameworkPropertyMetadata(0));

    /// <summary>
    /// 获取或设置内部 UniformGrid 的行数。0 表示按 <see cref="Columns"/> 自动计算。
    /// </summary>
    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    #endregion Rows

    #region Columns

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(
            nameof(Columns),
            typeof(int),
            typeof(EnumSelector),
            new FrameworkPropertyMetadata(0));

    /// <summary>
    /// 获取或设置内部 UniformGrid 的列数。0 表示按 <see cref="Rows"/> 自动计算。
    /// </summary>
    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    #endregion Columns

    #region RadioButtonGroupName (只读)

    private static readonly DependencyPropertyKey RadioButtonGroupNamePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(RadioButtonGroupName),
            typeof(string),
            typeof(EnumSelector),
            new PropertyMetadata(default(string)));

    public static readonly DependencyProperty RadioButtonGroupNameProperty = RadioButtonGroupNamePropertyKey.DependencyProperty;

    /// <summary>
    /// 获取本控件实例独立的 RadioButton 分组名。模板内部使用，外部一般不需要关心。
    /// </summary>
    public string RadioButtonGroupName
    {
        get => (string)GetValue(RadioButtonGroupNameProperty);
        private set => SetValue(RadioButtonGroupNamePropertyKey, value);
    }

    #endregion RadioButtonGroupName

    #region Override Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        // 把实例分组名暴露给模板（在 OnApplyTemplate 时设置，确保模板已就绪）
        RadioButtonGroupName = _radioButtonGroupName;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ListBoxItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ListBoxItem;
    }

    #endregion Override Methods

    #region Private Methods

    /// <summary>
    /// 从 <see cref="EnumType"/> 重新构建 ItemsSource 与位运算缓存，并刷新模式相关的派生属性。
    /// 在 EnumType 变化时被调用。
    /// </summary>
    private void RebuildItems()
    {
        // 取消旧 items 的 PropertyChanged 订阅，避免内存泄漏（Bug 修复）。
        if (ItemsSource is IEnumerable<EnumItem> oldItems)
        {
            foreach (var oldItem in oldItems)
            {
                oldItem.PropertyChanged -= OnEnumItemPropertyChanged;
            }
        }

        var enumType = EnumType;
        if (enumType == null || !enumType.IsEnum)
        {
            ItemsSource = null;
            _enumValueMap = null;
            HasFlags = false;
            UpdateEffectiveDisplayMode();
            return;
        }

        // Bug3 修复：用 ulong 取代 int，覆盖 byte/short/int/long/ulong 等所有合法底层类型。
        // 使用别名为 EnumItem 的对象包装枚举值与选中状态。
        var items = new ObservableCollection<EnumItem>(
            Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(e => new EnumItem(e, IsUseAlias)));

        foreach (var item in items)
        {
            item.PropertyChanged += OnEnumItemPropertyChanged;
        }

        ItemsSource = items;

        // 性能改进：位运算图谱在此处算一次缓存，点击时直接用，避免每次重建 O(n²)。
        HasFlags = enumType.GetCustomAttribute<FlagsAttribute>() != null;
        _enumValueMap = HasFlags ? BuildEnumValueMap(items) : null;

        UpdateEffectiveDisplayMode();
        SyncItemsFromSelectedEnum();
    }

    /// <summary>
    /// 解析 <see cref="DisplayMode"/> 与 <see cref="HasFlags"/> 得到 <see cref="EffectiveDisplayMode"/>。
    /// </summary>
    private void UpdateEffectiveDisplayMode()
    {
        EffectiveDisplayMode = DisplayMode switch
        {
            EnumDisplayMode.RadioButton => EnumDisplayMode.RadioButton,
            EnumDisplayMode.CheckBox => EnumDisplayMode.CheckBox,
            // Auto：Flags → CheckBox，否则 RadioButton
            _ => HasFlags ? EnumDisplayMode.CheckBox : EnumDisplayMode.RadioButton,
        };
    }

    /// <summary>
    /// 是否是"真多选"模式：必须同时满足 EffectiveDisplayMode=CheckBox 且 EnumType 是 Flags。
    /// Bug5 修复：非 Flags 即使强制 CheckBox 视觉，行为上仍为单选，避免合并出无效枚举值。
    /// </summary>
    private bool IsTrueMultiSelect => EffectiveDisplayMode == EnumDisplayMode.CheckBox && HasFlags;

    /// <summary>
    /// 构建枚举值的"真子集"映射：value → 包含在 value 内的所有非零真子集（不含自身、不含 0）。
    /// 在 EnumType 变化时算一次缓存到 _enumValueMap。
    /// </summary>
    private static Dictionary<ulong, List<ulong>> BuildEnumValueMap(IEnumerable<EnumItem> items)
    {
        var allValues = items.Select(i => i.NumericValue).Distinct().ToList();
        var map = new Dictionary<ulong, List<ulong>>();

        foreach (var value in allValues)
        {
            var subsets = new List<ulong>();
            foreach (var other in allValues)
            {
                if (other != 0 && other != value && (value & other) == other)
                {
                    subsets.Add(other);
                }
            }
            map[value] = subsets;
        }
        return map;
    }

    /// <summary>
    /// EnumItem.IsSelected 变化时回写到 SelectedEnum，并维护 Flags 模式下的复合关系。
    /// </summary>
    private void OnEnumItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (_isSyncing || e.PropertyName != nameof(EnumItem.IsSelected))
        {
            return;
        }

        if (sender is not EnumItem changedItem)
        {
            return;
        }

        _isSyncing = true;
        try
        {
            if (IsTrueMultiSelect)
            {
                // Flags + CheckBox：维护复合关系（选中复合值 = 选中所有子位；取消任一子位 = 取消所有包含它的复合值）
                MaintainCompositeRelations(changedItem);
                WriteSelectedEnumFromItems();
            }
            else
            {
                // 单选语义（RadioButton / 非 Flags + CheckBox）：仅当被设为 selected 时排他
                if (changedItem.IsSelected)
                {
                    foreach (EnumItem item in (IEnumerable<EnumItem>)ItemsSource)
                    {
                        if (item != changedItem && item.IsSelected)
                        {
                            item.IsSelected = false;
                        }
                    }
                    SelectedEnum = changedItem.Enum;
                }
                else
                {
                    // 单选模式不允许"全部取消"——如果用户取消了当前唯一选中项，恢复它。
                    // 例外：用户主动设 SelectedEnum=null 时已经走 SyncItemsFromSelectedEnum 路径，不会进这里。
                    if (!HasAnySelected())
                    {
                        changedItem.IsSelected = true;
                    }
                }
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    /// <summary>
    /// Flags 模式下维护"父子复合关系"与"None 互斥关系"：
    /// - 选中复合值 → 选中其所有子位
    /// - 取消复合值 → 取消其所有子位
    /// - 选中 None（0 值项）→ 取消所有其他项
    /// - 选中任何非 None → 取消所有 0 值项
    /// - 最后扫描所有复合项，按"所有子位是否全选"重新计算它的 IsSelected
    /// </summary>
    private void MaintainCompositeRelations(EnumItem changedItem)
    {
        if (_enumValueMap == null || ItemsSource is not IEnumerable<EnumItem> items)
        {
            return;
        }

        // 缓存为 List 避免对 IEnumerable 多次遍历产生的副作用。
        var itemsList = items.ToList();
        var itemValue = changedItem.NumericValue;

        // None 互斥：0 值项与任何非 0 值项不能共存。
        if (changedItem.IsSelected)
        {
            if (itemValue == 0)
            {
                // 选中 None：取消所有其他项，提前返回（None 无复合关系可维护）
                foreach (var i in itemsList)
                {
                    if (i != changedItem)
                    {
                        i.IsSelected = false;
                    }
                }
                return;
            }
            else
            {
                // 选中非 None 项：取消所有 0 值项
                foreach (var i in itemsList)
                {
                    if (i.NumericValue == 0 && i.IsSelected)
                    {
                        i.IsSelected = false;
                    }
                }
            }
        }

        if (!_enumValueMap.TryGetValue(itemValue, out var subsets))
        {
            return;
        }

        // 复合值（包含其他位的）：联动子位
        if (subsets.Count > 0)
        {
            var newState = changedItem.IsSelected;
            foreach (var sub in subsets)
            {
                var subItem = itemsList.FirstOrDefault(i => i.NumericValue == sub);
                if (subItem != null)
                {
                    subItem.IsSelected = newState;
                }
            }
        }

        // 重新计算所有复合项的 IsSelected：所有子位都被选中 → 复合项也选中
        foreach (var entry in _enumValueMap)
        {
            if (entry.Value.Count == 0)
            {
                continue;
            }

            var compositeItem = itemsList.FirstOrDefault(i => i.NumericValue == entry.Key);
            if (compositeItem == null)
            {
                continue;
            }

            bool allChildrenSelected = entry.Value.All(sub =>
                itemsList.FirstOrDefault(i => i.NumericValue == sub)?.IsSelected == true);

            compositeItem.IsSelected = allChildrenSelected;
        }
    }

    /// <summary>
    /// 由内部 EnumItem 的选中状态计算 SelectedEnum 并回写到依赖属性。
    /// </summary>
    private void WriteSelectedEnumFromItems()
    {
        if (ItemsSource is not IEnumerable<EnumItem> items || EnumType == null)
        {
            return;
        }

        if (IsTrueMultiSelect)
        {
            ulong combined = 0;
            foreach (var item in items)
            {
                if (item.IsSelected)
                {
                    combined |= item.NumericValue;
                }
            }
            SelectedEnum = (Enum)Enum.ToObject(EnumType, combined);
        }
        else
        {
            var selected = items.FirstOrDefault(i => i.IsSelected);
            SelectedEnum = selected?.Enum;
        }
    }

    /// <summary>
    /// 由 SelectedEnum 反向同步 EnumItem.IsSelected。
    /// </summary>
    private void SyncItemsFromSelectedEnum()
    {
        if (ItemsSource is not IEnumerable<EnumItem> items)
        {
            return;
        }

        _isSyncing = true;
        try
        {
            // SelectedEnum=null：清空全部
            if (SelectedEnum == null)
            {
                foreach (var item in items)
                {
                    item.IsSelected = false;
                }
                return;
            }

            ulong selectedValue = ToUInt64(SelectedEnum);

            if (IsTrueMultiSelect)
            {
                if (selectedValue == 0)
                {
                    // SelectedEnum 是 0 值（如 None）：选中 None 项（如果存在），其他全清。
                    foreach (var item in items)
                    {
                        item.IsSelected = item.NumericValue == 0;
                    }
                    return;
                }

                // Flags + CheckBox + 非零选中值：按位逻辑判断
                foreach (var item in items)
                {
                    var v = item.NumericValue;
                    // 0 值项不能被非零 selectedValue 激活。
                    item.IsSelected = v != 0 && (selectedValue & v) == v;
                }

                // 复合项需要单独检查"所有子位是否被选"——不能简单按位逻辑，因为复合项的 NumericValue
                // 自身就是若干 bit 的或值，前面的循环可能把它误判为选中。这里覆盖修正：
                if (_enumValueMap != null)
                {
                    var itemsList = items.ToList();
                    foreach (var entry in _enumValueMap)
                    {
                        if (entry.Value.Count == 0)
                        {
                            continue;
                        }

                        var compositeItem = itemsList.FirstOrDefault(i => i.NumericValue == entry.Key);
                        if (compositeItem == null)
                        {
                            continue;
                        }

                        bool allChildrenSelected = entry.Value.All(sub =>
                            itemsList.FirstOrDefault(i => i.NumericValue == sub)?.IsSelected == true);

                        compositeItem.IsSelected = allChildrenSelected;
                    }
                }
            }
            else
            {
                // 单选：仅匹配 NumericValue 严格相等的那一项
                foreach (var item in items)
                {
                    item.IsSelected = item.NumericValue == selectedValue;
                }
            }
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private bool HasAnySelected()
    {
        return ItemsSource is IEnumerable<EnumItem> items && items.Any(i => i.IsSelected);
    }

    /// <summary>
    /// Bug3 修复辅助：把任意底层类型的 Enum 转成 ulong（保留所有 bit），用于位运算与比较。
    /// 有符号底层类型（int/long）通过 unchecked 强转保留位模式，与无符号底层类型行为一致。
    /// </summary>
    internal static ulong ToUInt64(Enum value)
    {
        var underlyingType = Enum.GetUnderlyingType(value.GetType());
        return underlyingType == typeof(ulong)
            ? Convert.ToUInt64(value)
            : unchecked((ulong)Convert.ToInt64(value));
    }

    #endregion Private Methods
}
