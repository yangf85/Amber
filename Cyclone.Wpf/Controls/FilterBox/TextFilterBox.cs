using Cyclone.Wpf.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 文本过滤控件:复选框启用 + 操作符下拉 + 文本输入。
/// 控件只暴露 <see cref="IsActive"/> / <see cref="Operator"/> / <see cref="Text"/> / <see cref="IsCaseSensitive"/>
/// 等状态,由调用方自行根据这些状态拼装过滤委托。
/// </summary>
[TemplatePart(Name = PART_ActiveCheckBox, Type = typeof(CheckBox))]
[TemplatePart(Name = PART_OperatorComboBox, Type = typeof(ComboBox))]
[TemplatePart(Name = PART_InputTextBox, Type = typeof(TextBox))]
public class TextFilterBox : Control
{
    private const string PART_ActiveCheckBox = nameof(PART_ActiveCheckBox);

    private const string PART_OperatorComboBox = nameof(PART_OperatorComboBox);

    private const string PART_InputTextBox = nameof(PART_InputTextBox);

    private CheckBox _activeCheckBox;

    private ComboBox _operatorComboBox;

    private TextBox _inputTextBox;

    static TextFilterBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TextFilterBox),
            new FrameworkPropertyMetadata(typeof(TextFilterBox)));
    }

    #region DependencyProperties

    #region Label

    public static readonly DependencyProperty LabelProperty =
        FormItem.LabelProperty.AddOwner(
            typeof(TextFilterBox),
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
        var filterBox = (TextFilterBox)d;
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
            typeof(TextFilterBox),
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
            typeof(TextFilterBox),
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

    #region Text

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(TextFilterBox),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    #endregion Text

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register(
            nameof(Watermark),
            typeof(string),
            typeof(TextFilterBox),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 输入框空状态下的占位提示。
    /// </summary>
    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    #endregion Watermark

    #region MaxLength

    public static readonly DependencyProperty MaxLengthProperty =
        TextBox.MaxLengthProperty.AddOwner(
            typeof(TextFilterBox),
            new PropertyMetadata(0));

    /// <summary>
    /// 输入框最大字符数。0 表示不限制。透传给内部 TextBox。
    /// </summary>
    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    #endregion MaxLength

    #region IsCaseSensitive

    public static readonly DependencyProperty IsCaseSensitiveProperty =
        DependencyProperty.Register(
            nameof(IsCaseSensitive),
            typeof(bool),
            typeof(TextFilterBox),
            new PropertyMetadata(false));

    /// <summary>
    /// 字符串比较是否区分大小写。控件本身不使用此值,仅作为状态暴露给调用方,
    /// 供其在拼装比较委托时使用(典型映射:false → <see cref="System.StringComparison.OrdinalIgnoreCase"/>;
    /// Regex 模式下 false → 带 <see cref="System.Text.RegularExpressions.RegexOptions.IgnoreCase"/>)。
    /// </summary>
    public bool IsCaseSensitive
    {
        get => (bool)GetValue(IsCaseSensitiveProperty);
        set => SetValue(IsCaseSensitiveProperty, value);
    }

    #endregion IsCaseSensitive

    #region SharedName

    public static readonly DependencyProperty SharedNameProperty =
        FormItem.SharedNameProperty.AddOwner(
            typeof(TextFilterBox),
            new PropertyMetadata(default(string)));

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
            typeof(TextOperator),
            typeof(TextFilterBox),
            new FrameworkPropertyMetadata(default(TextOperator), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public TextOperator Operator
    {
        get => (TextOperator)GetValue(OperatorProperty);
        set => SetValue(OperatorProperty, value);
    }

    #endregion Operator

    #region ExtraContent

    public static readonly DependencyProperty ExtraContentProperty =
        DependencyProperty.Register(
            nameof(ExtraContent),
            typeof(object),
            typeof(TextFilterBox),
            new PropertyMetadata(default(object), OnExtraContentChanged));

    /// <summary>
    /// 输入框右侧的附加内容(如复选框组、按钮等)。为 null 时该列自动收起。
    /// </summary>
    public object ExtraContent
    {
        get => GetValue(ExtraContentProperty);
        set => SetValue(ExtraContentProperty, value);
    }

    private static void OnExtraContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var filterBox = (TextFilterBox)d;
        if (e.OldValue is not null)
        {
            filterBox.RemoveLogicalChild(e.OldValue);
        }
        if (e.NewValue is not null)
        {
            filterBox.AddLogicalChild(e.NewValue);
        }
    }

    #endregion ExtraContent

    #endregion DependencyProperties

    #region Override Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _activeCheckBox = GetTemplateChild(PART_ActiveCheckBox) as CheckBox;
        _operatorComboBox = GetTemplateChild(PART_OperatorComboBox) as ComboBox;
        _inputTextBox = GetTemplateChild(PART_InputTextBox) as TextBox;
    }

    #endregion Override Methods
}