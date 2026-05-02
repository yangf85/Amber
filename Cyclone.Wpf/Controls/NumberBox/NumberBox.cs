using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// NumberBox 控件：带步进按钮的数字输入框。
/// 继承自 <see cref="RangeBase"/>，借用其 Value / Minimum / Maximum / SmallChange / LargeChange / ValueChanged 等成员。
/// 支持整数与小数模式、前后缀、键盘 Up/Down/PgUp/PgDn 步进、滚轮步进（仅在键盘焦点内）、清空按钮、横竖向步进按钮布局。
/// </summary>
[TemplatePart(Name = PART_RootBorder, Type = typeof(Border))]
[TemplatePart(Name = PART_InputTextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PART_IncreaseRepeatButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PART_DecreaseRepeatButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PART_ClearButton, Type = typeof(Button))]
public class NumberBox : RangeBase
{
    private const string PART_RootBorder = nameof(PART_RootBorder);
    private const string PART_InputTextBox = nameof(PART_InputTextBox);
    private const string PART_IncreaseRepeatButton = nameof(PART_IncreaseRepeatButton);
    private const string PART_DecreaseRepeatButton = nameof(PART_DecreaseRepeatButton);
    private const string PART_ClearButton = nameof(PART_ClearButton);

    private TextBox _inputTextBox;
    private RepeatButton _increaseRepeatButton;
    private RepeatButton _decreaseRepeatButton;
    private Button _clearButton;

    /// <summary>
    /// 用于阻止"用户输入引发 Value 变化 → Value 变化又回写 text → 又触发 TextChanged"的循环。
    /// 同时使输入过程中不会被 reformat 打断（光标位置不丢失）。
    /// </summary>
    private bool _isUpdatingFromText;

    static NumberBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberBox),
            new FrameworkPropertyMetadata(typeof(NumberBox)));

        // RangeBase 的 Maximum 默认是 1。NumberBox 作为通用数字输入控件，更合理的默认是 double.MaxValue / MinValue。
        MinimumProperty.OverrideMetadata(
            typeof(NumberBox),
            new FrameworkPropertyMetadata(double.MinValue));
        MaximumProperty.OverrideMetadata(
            typeof(NumberBox),
            new FrameworkPropertyMetadata(double.MaxValue));
        // SmallChange 默认 0.1，对于整数模式不直观。NumberBox 改默认 1。
        SmallChangeProperty.OverrideMetadata(
            typeof(NumberBox),
            new FrameworkPropertyMetadata(1d));
        LargeChangeProperty.OverrideMetadata(
            typeof(NumberBox),
            new FrameworkPropertyMetadata(10d));

        InitializeCommands();
    }

    #region DecimalPlaces

    public static readonly DependencyProperty DecimalPlacesProperty =
        DependencyProperty.Register(
            nameof(DecimalPlaces),
            typeof(int),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(
                2,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDecimalPlacesChanged,
                CoerceDecimalPlaces));

    /// <summary>
    /// 显示与输入允许的最大小数位数。<see cref="NumberMode"/> 为 Integer 时强制为 0。
    /// </summary>
    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    private static object CoerceDecimalPlaces(DependencyObject d, object baseValue)
    {
        var box = (NumberBox)d;
        var v = (int)baseValue;
        if (v < 0)
        {
            v = 0;
        }
        // Integer 模式强制 0 位小数
        if (box.NumberMode == NumberMode.Integer)
        {
            v = 0;
        }
        return v;
    }

    private static void OnDecimalPlacesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (NumberBox)d;
        // 把当前值按新位数四舍五入；用 SetCurrentValue 不破坏外部 Binding。
        var rounded = Math.Round(box.Value, (int)e.NewValue);
        box.SetCurrentValue(ValueProperty, rounded);
        // Value 变化会通过 OnValueChanged 路径自然回写 text，无需此处再次写。
    }

    #endregion DecimalPlaces

    #region NumberMode

    public static readonly DependencyProperty NumberModeProperty =
        DependencyProperty.Register(
            nameof(NumberMode),
            typeof(NumberMode),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(NumberMode.Decimal, OnNumberModeChanged));

    /// <summary>
    /// 数字模式：Integer（整数，强制 DecimalPlaces=0）或 Decimal（小数）。
    /// 取代了原来直接暴露 <see cref="NumberStyles"/> 的设计——后者太底层、易误用。
    /// </summary>
    public NumberMode NumberMode
    {
        get => (NumberMode)GetValue(NumberModeProperty);
        set => SetValue(NumberModeProperty, value);
    }

    private static void OnNumberModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (NumberBox)d;
        // Integer 模式：强制 DecimalPlaces=0、把当前值取整。
        if ((NumberMode)e.NewValue == NumberMode.Integer)
        {
            box.CoerceValue(DecimalPlacesProperty);
            box.SetCurrentValue(ValueProperty, Math.Truncate(box.Value));
        }
        // 模式变化时刷新 text 以反映可能的取整变化。
        box.RefreshTextFromValue();
    }

    #endregion NumberMode

    #region Prefix

    public static readonly DependencyProperty PrefixProperty =
        DependencyProperty.Register(
            nameof(Prefix),
            typeof(object),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(default(object)));

    /// <summary>
    /// 输入框前缀内容（如 "$"、"￥"），为 null 时该位置自动收起。
    /// </summary>
    public object Prefix
    {
        get => GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    #endregion Prefix

    #region Suffix

    public static readonly DependencyProperty SuffixProperty =
        DependencyProperty.Register(
            nameof(Suffix),
            typeof(object),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(default(object)));

    /// <summary>
    /// 输入框后缀内容（如 "%"、"USD"），为 null 时该位置自动收起。
    /// </summary>
    public object Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    #endregion Suffix

    #region IsReadOnly

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (NumberBox)d;
        if (box._inputTextBox is not null)
        {
            box._inputTextBox.IsReadOnly = (bool)e.NewValue;
        }
    }

    #endregion IsReadOnly

    #region IsSpinButtonVisible

    public static readonly DependencyProperty IsSpinButtonVisibleProperty =
        DependencyProperty.Register(
            nameof(IsSpinButtonVisible),
            typeof(bool),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(true));

    public bool IsSpinButtonVisible
    {
        get => (bool)GetValue(IsSpinButtonVisibleProperty);
        set => SetValue(IsSpinButtonVisibleProperty, value);
    }

    #endregion IsSpinButtonVisible

    #region IsClearButtonVisible

    public static readonly DependencyProperty IsClearButtonVisibleProperty =
        DependencyProperty.Register(
            nameof(IsClearButtonVisible),
            typeof(bool),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(false));

    public bool IsClearButtonVisible
    {
        get => (bool)GetValue(IsClearButtonVisibleProperty);
        set => SetValue(IsClearButtonVisibleProperty, value);
    }

    #endregion IsClearButtonVisible

    #region SpinButtonOrientation

    public static readonly DependencyProperty SpinButtonOrientationProperty =
        DependencyProperty.Register(
            nameof(SpinButtonOrientation),
            typeof(Orientation),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(Orientation.Vertical));

    /// <summary>
    /// 步进按钮方向。
    /// Vertical：[Prefix] [Input] [Suffix] | [+/- 垂直堆叠]
    /// Horizontal：[-] [Prefix] [Input] [Suffix] [+]   （减号在左，加号在右）
    /// </summary>
    public Orientation SpinButtonOrientation
    {
        get => (Orientation)GetValue(SpinButtonOrientationProperty);
        set => SetValue(SpinButtonOrientationProperty, value);
    }

    #endregion SpinButtonOrientation

    #region Commands

    public static RoutedCommand IncreaseCommand { get; private set; }

    public static RoutedCommand DecreaseCommand { get; private set; }

    public static RoutedCommand ClearCommand { get; private set; }

    private static void InitializeCommands()
    {
        IncreaseCommand = new RoutedCommand(nameof(IncreaseCommand), typeof(NumberBox));
        DecreaseCommand = new RoutedCommand(nameof(DecreaseCommand), typeof(NumberBox));
        ClearCommand = new RoutedCommand(nameof(ClearCommand), typeof(NumberBox));

        CommandManager.RegisterClassCommandBinding(typeof(NumberBox),
            new CommandBinding(IncreaseCommand, OnIncreaseExecuted, OnCanIncrease));
        CommandManager.RegisterClassCommandBinding(typeof(NumberBox),
            new CommandBinding(DecreaseCommand, OnDecreaseExecuted, OnCanDecrease));
        CommandManager.RegisterClassCommandBinding(typeof(NumberBox),
            new CommandBinding(ClearCommand, OnClearExecuted, OnCanClear));
    }

    private static void OnIncreaseExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var box = (NumberBox)sender;
        if (box.IsReadOnly)
        {
            return;
        }
        // 加完后 RangeBase 会自动 coerce 到 [Min, Max]，但显式 clamp 一次更稳。
        var next = Math.Min(box.Value + box.SmallChange, box.Maximum);
        box.SetCurrentValue(ValueProperty, next);
    }

    private static void OnCanIncrease(object sender, CanExecuteRoutedEventArgs e)
    {
        var box = (NumberBox)sender;
        // Bug 4 修复：用 Value < Max 而不是 Value+Step <= Max，
        // 否则用户在 Step 大于剩余空间时会被卡住到不了边界。
        e.CanExecute = !box.IsReadOnly && box.Value < box.Maximum;
    }

    private static void OnDecreaseExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var box = (NumberBox)sender;
        if (box.IsReadOnly)
        {
            return;
        }
        var next = Math.Max(box.Value - box.SmallChange, box.Minimum);
        box.SetCurrentValue(ValueProperty, next);
    }

    private static void OnCanDecrease(object sender, CanExecuteRoutedEventArgs e)
    {
        var box = (NumberBox)sender;
        e.CanExecute = !box.IsReadOnly && box.Value > box.Minimum;
    }

    private static void OnClearExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        var box = (NumberBox)sender;
        if (box.IsReadOnly)
        {
            return;
        }
        box.SetCurrentValue(ValueProperty, GetClearTarget(box));
    }

    private static void OnCanClear(object sender, CanExecuteRoutedEventArgs e)
    {
        var box = (NumberBox)sender;
        // Bug 5 修复：和真实 clear 目标比较，而不是和 Minimum 比较。
        e.CanExecute = !box.IsReadOnly && box.Value != GetClearTarget(box);
    }

    /// <summary>Clear 的目标值：如果 0 在 [Min, Max] 范围内则为 0，否则为 Min。</summary>
    private static double GetClearTarget(NumberBox box)
    {
        return Math.Max(0d, box.Minimum);
    }

    #endregion Commands

    #region RangeBase Override

    /// <summary>
    /// RangeBase.OnValueChanged 是控件层面的官方钩子，比手动注册 ValueChanged 路由事件更标准。
    /// 不再自定义 ValueChangedEvent —— 使用基类提供的即可。
    /// </summary>
    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);

        // Bug 1 修复：仅在不是用户输入引发的 Value 变化时才回写 text。
        // 用户输入过程中，Value 由 InputTextBox_TextChanged 改写，期间不可重新格式化 text，
        // 否则光标位置丢失、用户中间状态（"1."、"-"）被打断。
        if (!_isUpdatingFromText)
        {
            RefreshTextFromValue();
        }

        // CanExecute 状态依赖 Value（边界判断），需要主动通知刷新按钮状态。
        CommandManager.InvalidateRequerySuggested();
    }

    protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
    {
        base.OnMinimumChanged(oldMinimum, newMinimum);
        CommandManager.InvalidateRequerySuggested();
    }

    protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
    {
        base.OnMaximumChanged(oldMaximum, newMaximum);
        CommandManager.InvalidateRequerySuggested();
    }

    #endregion RangeBase Override

    #region Override Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解订阅旧模板部件，防止重模板化时的内存泄漏与重复订阅。
        DetachInputTextBox();

        _inputTextBox = GetTemplateChild(PART_InputTextBox) as TextBox;
        _increaseRepeatButton = GetTemplateChild(PART_IncreaseRepeatButton) as RepeatButton;
        _decreaseRepeatButton = GetTemplateChild(PART_DecreaseRepeatButton) as RepeatButton;
        _clearButton = GetTemplateChild(PART_ClearButton) as Button;

        if (_inputTextBox is not null)
        {
            _inputTextBox.Text = FormatValue(Value);
            _inputTextBox.IsReadOnly = IsReadOnly;
            _inputTextBox.PreviewTextInput += InputTextBox_PreviewTextInput;
            _inputTextBox.TextChanged += InputTextBox_TextChanged;
            _inputTextBox.PreviewKeyDown += InputTextBox_PreviewKeyDown;
            _inputTextBox.LostFocus += InputTextBox_LostFocus;
            _inputTextBox.MouseWheel += InputTextBox_MouseWheel;
            DataObject.AddPastingHandler(_inputTextBox, OnPasting);
        }
    }

    private void DetachInputTextBox()
    {
        if (_inputTextBox is null)
        {
            return;
        }
        _inputTextBox.PreviewTextInput -= InputTextBox_PreviewTextInput;
        _inputTextBox.TextChanged -= InputTextBox_TextChanged;
        _inputTextBox.PreviewKeyDown -= InputTextBox_PreviewKeyDown;
        _inputTextBox.LostFocus -= InputTextBox_LostFocus;
        _inputTextBox.MouseWheel -= InputTextBox_MouseWheel;
        DataObject.RemovePastingHandler(_inputTextBox, OnPasting);
    }

    #endregion Override Methods

    #region Input Handlers

    private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (IsReadOnly)
        {
            // 只读时屏蔽方向键步进。
            if (e.Key is Key.Up or Key.Down or Key.PageUp or Key.PageDown)
            {
                e.Handled = true;
            }
            return;
        }

        switch (e.Key)
        {
            case Key.Up:
                if (IncreaseCommand.CanExecute(null, this))
                {
                    IncreaseCommand.Execute(null, this);
                    e.Handled = true;
                }
                break;

            case Key.Down:
                if (DecreaseCommand.CanExecute(null, this))
                {
                    DecreaseCommand.Execute(null, this);
                    e.Handled = true;
                }
                break;

            case Key.PageUp:
                StepBy(LargeChange);
                e.Handled = true;
                break;

            case Key.PageDown:
                StepBy(-LargeChange);
                e.Handled = true;
                break;

            case Key.Enter:
                // Enter 提交：当前 text 立即解析并 reformat
                CommitTextToValue(reformatAfter: true);
                e.Handled = true;
                break;
        }
    }

    private void StepBy(double delta)
    {
        var next = Math.Max(Minimum, Math.Min(Maximum, Value + delta));
        SetCurrentValue(ValueProperty, next);
    }

    private void InputTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (IsReadOnly)
        {
            e.Handled = true;
            return;
        }

        var newText = ComposeNewText(e.Text);
        e.Handled = !IsValidIntermediateInput(newText);
    }

    private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsReadOnly || _inputTextBox is null)
        {
            return;
        }

        var text = _inputTextBox.Text;

        // 中间合法状态不动 Value：空字符串、单独的 "-"、"."、"-." 等。
        if (string.IsNullOrEmpty(text) || text == "-" || text == "." || text == "-.")
        {
            return;
        }

        if (TryParseValue(text, out var value))
        {
            value = Clamp(Math.Round(value, DecimalPlaces));

            _isUpdatingFromText = true;
            try
            {
                SetCurrentValue(ValueProperty, value);
                // 注意：这里不主动 reformat text，否则又把光标顶到末尾。
                // 失焦或按 Enter 时统一 reformat。
            }
            finally
            {
                _isUpdatingFromText = false;
            }
        }
        // 解析失败也不强制改 text——让用户继续编辑，失焦时再统一处理。
    }

    private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // 失焦时把当前 text 提交到 Value，并按 DecimalPlaces 重新格式化。
        CommitTextToValue(reformatAfter: true);
    }

    /// <summary>
    /// 把当前 text 解析提交到 Value，可选地重新格式化 text。
    /// 解析失败时回退为格式化后的 Value（避免遗留无效 text）。
    /// </summary>
    private void CommitTextToValue(bool reformatAfter)
    {
        if (_inputTextBox is null)
        {
            return;
        }

        var text = _inputTextBox.Text;
        if (string.IsNullOrEmpty(text) || text == "-" || text == "." || text == "-.")
        {
            // 中间态作废：恢复 text 为格式化的 Value。
            if (reformatAfter)
            {
                RefreshTextFromValue();
            }
            return;
        }

        if (TryParseValue(text, out var value))
        {
            value = Clamp(Math.Round(value, DecimalPlaces));
            SetCurrentValue(ValueProperty, value);
        }

        if (reformatAfter)
        {
            RefreshTextFromValue();
        }
    }

    private void InputTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (IsReadOnly || _inputTextBox is null)
        {
            return;
        }

        // Bug 2 修复：仅在键盘焦点位于 NumberBox 内才响应滚轮，
        // 否则放行让父级 ScrollViewer / 其他容器处理滚动。
        if (!_inputTextBox.IsKeyboardFocusWithin)
        {
            return;
        }

        if (e.Delta > 0)
        {
            if (IncreaseCommand.CanExecute(null, this))
            {
                IncreaseCommand.Execute(null, this);
            }
        }
        else
        {
            if (DecreaseCommand.CanExecute(null, this))
            {
                DecreaseCommand.Execute(null, this);
            }
        }

        e.Handled = true;
    }

    private void OnPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (IsReadOnly)
        {
            e.CancelCommand();
            return;
        }

        if (!e.DataObject.GetDataPresent(typeof(string)))
        {
            e.CancelCommand();
            return;
        }

        var pasted = (string)e.DataObject.GetData(typeof(string));
        var newText = ComposeNewText(pasted);
        if (!IsValidIntermediateInput(newText))
        {
            e.CancelCommand();
        }
    }

    /// <summary>把待插入的字符串和当前 TextBox 的选区/光标位置合成"插入后的完整文本"。</summary>
    private string ComposeNewText(string insertion)
    {
        var current = _inputTextBox.Text ?? string.Empty;
        var selStart = _inputTextBox.SelectionStart;
        var selLen = _inputTextBox.SelectionLength;
        return current.Remove(selStart, selLen).Insert(selStart, insertion);
    }

    #endregion Input Handlers

    #region Parsing & Formatting

    /// <summary>
    /// 校验"输入过程中的中间文本"是否合法。允许空串、单独 "-"、"." 等中间态，方便用户继续输入。
    /// 数字模式（Integer / Decimal）决定是否允许小数点。
    /// </summary>
    private bool IsValidIntermediateInput(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        // 仅允许的字符：数字、最多一个 '-' 在开头、按模式允许 '.'
        bool allowDecimal = NumberMode == NumberMode.Decimal;
        int dotCount = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '-')
            {
                if (i != 0)
                {
                    return false;
                }
                // 允许 Min < 0 时输入负号，Min >= 0 时不允许负数。
                if (Minimum >= 0)
                {
                    return false;
                }
            }
            else if (c == '.')
            {
                if (!allowDecimal || ++dotCount > 1)
                {
                    return false;
                }
            }
            else if (!char.IsDigit(c))
            {
                return false;
            }
        }

        // 检查小数位数（Decimal 模式下）
        if (allowDecimal)
        {
            int dotIndex = text.IndexOf('.');
            if (dotIndex >= 0)
            {
                int decimals = text.Length - dotIndex - 1;
                if (decimals > DecimalPlaces)
                {
                    return false;
                }
            }
        }

        // 中间态允许（"-"、"."、"-."），最终再 TryParse。
        if (text == "-" || text == "." || text == "-.")
        {
            return true;
        }

        return TryParseValue(text, out _);
    }

    private bool TryParseValue(string text, out double value)
    {
        var style = NumberMode == NumberMode.Integer
            ? NumberStyles.Integer
            : NumberStyles.Float;
        return double.TryParse(text, style, CultureInfo.InvariantCulture, out value);
    }

    private double Clamp(double value)
    {
        if (value < Minimum)
        {
            return Minimum;
        }
        if (value > Maximum)
        {
            return Maximum;
        }
        return value;
    }

    private string FormatValue(double value)
    {
        return value.ToString($"F{DecimalPlaces}", CultureInfo.InvariantCulture);
    }

    private void RefreshTextFromValue()
    {
        if (_inputTextBox is null)
        {
            return;
        }
        var formatted = FormatValue(Value);
        if (_inputTextBox.Text != formatted)
        {
            _inputTextBox.Text = formatted;
        }
    }

    #endregion Parsing & Formatting
}
