using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// TextBox 附加属性 + 辅助命令。
/// <list type="bullet">
/// <item><description><see cref="WatermarkProperty"/> — 水印文本(Text 为空且未获焦时显示)</description></item>
/// <item><description><see cref="HasClearButtonProperty"/> — 是否显示清除按钮(默认 false)</description></item>
/// <item><description><see cref="ClearCommand"/> — 清除文本的路由命令,样式里 ClearButton 绑定到这个</description></item>
/// </list>
/// </summary>
public class TextBoxHelper
{
    static TextBoxHelper()
    {
        // 类级别 CommandBinding — 给所有 TextBox 实例自动响应 ClearCommand,不需要每个实例单独绑定
        // 注意:第一个参数必须是 typeof(TextBox) 而不是 typeof(TextBoxHelper) —— RegisterClassCommandBinding
        // 把绑定挂到指定类型的所有实例上,挂到 TextBoxHelper 上没有效果(TextBoxHelper 不是 UIElement)
        CommandManager.RegisterClassCommandBinding(
            typeof(TextBox),
            new CommandBinding(ClearCommand, OnClear, OnCanClear));
    }

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.RegisterAttached(
            "Watermark",
            typeof(string),
            typeof(TextBoxHelper),
            new PropertyMetadata(null));

    public static string GetWatermark(DependencyObject obj) => (string)obj.GetValue(WatermarkProperty);

    public static void SetWatermark(DependencyObject obj, string value) => obj.SetValue(WatermarkProperty, value);

    #endregion Watermark

    #region HasClearButton

    public static readonly DependencyProperty HasClearButtonProperty =
        DependencyProperty.RegisterAttached(
            "HasClearButton",
            typeof(bool),
            typeof(TextBoxHelper),
            new PropertyMetadata(false));

    public static bool GetHasClearButton(DependencyObject obj) => (bool)obj.GetValue(HasClearButtonProperty);

    public static void SetHasClearButton(DependencyObject obj, bool value) => obj.SetValue(HasClearButtonProperty, value);

    #endregion HasClearButton

    #region ClearCommand

    public static RoutedCommand ClearCommand { get; } = new RoutedCommand(nameof(ClearCommand), typeof(TextBoxHelper));

    private static void OnCanClear(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            e.CanExecute = textBox.Text.Length > 0 && !textBox.IsReadOnly && textBox.IsEnabled;
        }
    }

    private static void OnClear(object sender, ExecutedRoutedEventArgs e)
    {
        (sender as TextBox)?.Clear();
    }

    #endregion ClearCommand
}