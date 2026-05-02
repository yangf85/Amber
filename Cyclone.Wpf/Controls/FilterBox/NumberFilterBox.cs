using Cyclone.Wpf.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum NumberOperator
{
    [Description("=")]               // Equal
    Equal,

    [Description("≠")]               // NotEqual（数学符号“不等于”）
    NotEqual,

    [Description("<")]               // LessThan
    LessThan,

    [Description("≤")]               // LessThanOrEqual（数学符号“小于等于”）
    LessThanOrEqual,

    [Description(">")]               // GreaterThan
    GreaterThan,

    [Description("≥")]               // GreaterThanOrEqual（数学符号“大于等于”）
    GreaterThanOrEqual
}

/// <summary>
/// 一个用于数字过滤的控件 ，比如>=,<,=等等
/// </summary>
[TemplatePart(Name = PART_ActivedCheckBox, Type = typeof(CheckBox))]
[TemplatePart(Name = PART_OperatorComboBox, Type = typeof(ComboBox))]
[TemplatePart(Name = PART_ValueNumberBox, Type = typeof(NumberBox))]
public class NumberFilterBox : Control
{
    private const string PART_ActivedCheckBox = nameof(PART_ActivedCheckBox);
    private const string PART_OperatorComboBox = nameof(PART_OperatorComboBox);
    private const string PART_ValueNumberBox = nameof(PART_ValueNumberBox);

    #region Label

    public static readonly DependencyProperty LabelProperty =
                FormItem.LabelProperty.AddOwner(typeof(NumberFilterBox), new PropertyMetadata(default, OnLabelChanged));

    public object Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var filterBox = (NumberFilterBox)d;
        if (e.NewValue != null)
        {
            filterBox.RemoveLogicalChild(e.OldValue);
            filterBox.AddLogicalChild(e.NewValue);
        }
    }

    #endregion Label

    #region Description

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(NumberFilterBox), new PropertyMetadata(default(object)));

    public object Description
    {
        get => (object)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    #endregion Description

    #region IsActived

    public static readonly DependencyProperty IsActivedProperty =
        DependencyProperty.Register(nameof(IsActived), typeof(bool), typeof(NumberFilterBox), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsActived
    {
        get => (bool)GetValue(IsActivedProperty);
        set => SetValue(IsActivedProperty, value);
    }

    #endregion IsActived

    #region Value

    public static readonly DependencyProperty ValueProperty =
        RangeBase.ValueProperty.AddOwner(typeof(NumberFilterBox), new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    #endregion Value

    #region Step

    public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(nameof(Step), typeof(double), typeof(NumberFilterBox), new PropertyMetadata(1d));

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    #endregion Step

    #region Tolerance

    public static readonly DependencyProperty ToleranceProperty =
        DependencyProperty.Register(nameof(Tolerance), typeof(double), typeof(NumberFilterBox), new PropertyMetadata(double.Epsilon));

    public double Tolerance
    {
        get => (double)GetValue(ToleranceProperty);
        set => SetValue(ToleranceProperty, value);
    }

    #endregion Tolerance

    #region DecimalPlaces

    public static readonly DependencyProperty DecimalPlacesProperty =
        NumberBox.DecimalPlacesProperty.AddOwner(typeof(NumberFilterBox), new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    #endregion DecimalPlaces

    #region SharedName

    public static readonly DependencyProperty SharedNameProperty =
                        FormItem.SharedNameProperty.AddOwner(typeof(NumberFilterBox), new FrameworkPropertyMetadata(default(string)));

    public string SharedName
    {
        get => (string)GetValue(SharedNameProperty);
        set => SetValue(SharedNameProperty, value);
    }

    #endregion SharedName

    #region Operator

    public static readonly DependencyProperty OperatorProperty =
        DependencyProperty.Register(nameof(Operator), typeof(NumberOperator), typeof(NumberFilterBox), new FrameworkPropertyMetadata(default(NumberOperator), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public NumberOperator Operator
    {
        get => (NumberOperator)GetValue(OperatorProperty);
        set => SetValue(OperatorProperty, value);
    }

    #endregion Operator

    #region NumberStyle

    public static readonly DependencyProperty NumberStyleProperty =
        NumberBox.NumberModeProperty.AddOwner(typeof(NumberFilterBox), new PropertyMetadata(NumberMode.Integer));

    public NumberMode NumberStyle
    {
        get => (NumberMode)GetValue(NumberStyleProperty);
        set => SetValue(NumberStyleProperty, value);
    }

    #endregion NumberStyle

    #region Maximum

    public static readonly DependencyProperty MaximumProperty =
        RangeBase.MaximumProperty.AddOwner(typeof(NumberFilterBox), new PropertyMetadata(double.MaxValue));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    #endregion Maximum

    #region Minimum

    public static readonly DependencyProperty MinimumProperty =
        RangeBase.MinimumProperty.AddOwner(typeof(NumberFilterBox), new PropertyMetadata(double.MinValue));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    #endregion Minimum
}