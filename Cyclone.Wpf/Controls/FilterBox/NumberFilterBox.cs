using Cyclone.Wpf.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 数字过滤控件:复选框启用 + 操作符下拉 + NumberBox 输入。
/// 控件只暴露 <see cref="IsActive"/> / <see cref="Operator"/> / <see cref="Value"/> 等状态,
/// 由调用方自行根据这些状态拼装过滤委托。
/// </summary>
[TemplatePart(Name = PART_ActiveCheckBox, Type = typeof(CheckBox))]
[TemplatePart(Name = PART_OperatorComboBox, Type = typeof(ComboBox))]
[TemplatePart(Name = PART_ValueNumberBox, Type = typeof(NumberBox))]
public class NumberFilterBox : Control
{
    private const string PART_ActiveCheckBox = nameof(PART_ActiveCheckBox);

    private const string PART_OperatorComboBox = nameof(PART_OperatorComboBox);

    private const string PART_ValueNumberBox = nameof(PART_ValueNumberBox);

    private CheckBox _activeCheckBox;

    private ComboBox _operatorComboBox;

    private NumberBox _valueNumberBox;

    static NumberFilterBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberFilterBox),
            new FrameworkPropertyMetadata(typeof(NumberFilterBox)));
    }

    #region DependencyProperties

    #region Label

    public static readonly DependencyProperty LabelProperty =
        FormItem.LabelProperty.AddOwner(
            typeof(NumberFilterBox),
            new PropertyMetadata(default, OnLabelChanged));

    /// <summary>
    /// 控件标签内容。支持任意 object,会以逻辑子节点形式挂入逻辑树以便绑定继承 DataContext。
    /// </summary>
    public object Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var filterBox = (NumberFilterBox)d;
        if (e.OldValue is not null)
        {
            filterBox.RemoveLogicalChild(e.OldValue);
        }
        if (e.NewValue is not null)
        {
            filterBox.AddLogicalChild(e.NewValue);
        }
    }

    #endregion Label

    #region Description

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(object),
            typeof(NumberFilterBox),
            new PropertyMetadata(default(object)));

    /// <summary>
    /// 描述文字,显示在控件下方第二行。为 null 时整行收起。
    /// </summary>
    public object Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    #endregion Description

    #region IsActive

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(NumberFilterBox),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// 是否启用此过滤项。绑定时通常与下游过滤逻辑联动:false 表示该过滤项不参与判定。
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    #endregion IsActive

    #region Value

    public static readonly DependencyProperty ValueProperty =
        RangeBase.ValueProperty.AddOwner(
            typeof(NumberFilterBox),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    #endregion Value

    #region SmallChange

    public static readonly DependencyProperty SmallChangeProperty =
        RangeBase.SmallChangeProperty.AddOwner(
            typeof(NumberFilterBox),
            new FrameworkPropertyMetadata(1d));

    /// <summary>
    /// 小步长(方向键、滚轮)。透传给内部 NumberBox。
    /// </summary>
    public double SmallChange
    {
        get => (double)GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    #endregion SmallChange

    #region LargeChange

    public static readonly DependencyProperty LargeChangeProperty =
        RangeBase.LargeChangeProperty.AddOwner(
            typeof(NumberFilterBox),
            new FrameworkPropertyMetadata(10d));

    /// <summary>
    /// 大步长(PageUp/PageDown)。透传给内部 NumberBox。
    /// </summary>
    public double LargeChange
    {
        get => (double)GetValue(LargeChangeProperty);
        set => SetValue(LargeChangeProperty, value);
    }

    #endregion LargeChange

    #region Tolerance

    public static readonly DependencyProperty ToleranceProperty =
        DependencyProperty.Register(
            nameof(Tolerance),
            typeof(double),
            typeof(NumberFilterBox),
            new PropertyMetadata(1e-9));

    /// <summary>
    /// 浮点比较容差。控件本身不使用此值,仅作为状态暴露给调用方,
    /// 供其在拼装 Equal / NotEqual 比较委托时使用(典型实现:|x − Value| ≤ Tolerance 视为相等)。
    /// 默认 1e-9。Integer 模式下建议设为 0 走精确比较。
    /// </summary>
    public double Tolerance
    {
        get => (double)GetValue(ToleranceProperty);
        set => SetValue(ToleranceProperty, value);
    }

    #endregion Tolerance

    #region DecimalPlaces

    public static readonly DependencyProperty DecimalPlacesProperty =
        NumberBox.DecimalPlacesProperty.AddOwner(
            typeof(NumberFilterBox),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    #endregion DecimalPlaces

    #region SharedName

    public static readonly DependencyProperty SharedNameProperty =
        FormItem.SharedNameProperty.AddOwner(
            typeof(NumberFilterBox),
            new FrameworkPropertyMetadata(default(string)));

    /// <summary>
    /// 与同组其他过滤控件共用的 Grid.SharedSizeScope 标识,用于对齐 Label 列宽。
    /// </summary>
    public string SharedName
    {
        get => (string)GetValue(SharedNameProperty);
        set => SetValue(SharedNameProperty, value);
    }

    #endregion SharedName

    #region Operator

    public static readonly DependencyProperty OperatorProperty =
        DependencyProperty.Register(
            nameof(Operator),
            typeof(NumberOperator),
            typeof(NumberFilterBox),
            new FrameworkPropertyMetadata(default(NumberOperator), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public NumberOperator Operator
    {
        get => (NumberOperator)GetValue(OperatorProperty);
        set => SetValue(OperatorProperty, value);
    }

    #endregion Operator

    #region NumberMode

    public static readonly DependencyProperty NumberModeProperty =
        NumberBox.NumberModeProperty.AddOwner(
            typeof(NumberFilterBox),
            new PropertyMetadata(NumberMode.Integer));

    /// <summary>
    /// 数字模式(Integer / Decimal)。透传给内部 NumberBox。
    /// 默认 Integer——配合默认的 Maximum/Minimum 使用 int 范围。
    /// 注意:Integer 模式下若用 <see cref="NumberOperator.Equal"/>,建议把 <see cref="Tolerance"/> 设为 0 走精确比较。
    /// </summary>
    public NumberMode NumberMode
    {
        get => (NumberMode)GetValue(NumberModeProperty);
        set => SetValue(NumberModeProperty, value);
    }

    #endregion NumberMode

    #region Maximum

    public static readonly DependencyProperty MaximumProperty =
        RangeBase.MaximumProperty.AddOwner(
            typeof(NumberFilterBox),
            new PropertyMetadata((double)int.MaxValue));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    #endregion Maximum

    #region Minimum

    public static readonly DependencyProperty MinimumProperty =
        RangeBase.MinimumProperty.AddOwner(
            typeof(NumberFilterBox),
            new PropertyMetadata((double)int.MinValue));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    #endregion Minimum

    #endregion DependencyProperties

    #region Override Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _activeCheckBox = GetTemplateChild(PART_ActiveCheckBox) as CheckBox;
        _operatorComboBox = GetTemplateChild(PART_OperatorComboBox) as ComboBox;
        _valueNumberBox = GetTemplateChild(PART_ValueNumberBox) as NumberBox;
    }

    #endregion Override Methods
}