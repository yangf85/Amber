using Cyclone.Wpf.Themes;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 警告对话框窗口。视觉样式（颜色、按钮高度、Caption 样式、加载动画等）<b>全部</b>由
/// <c>Styles/Alert.xaml</c> 主题字典控制——主题切换时自动响应。
/// <para>
/// 14 个旧 DP（CaptionBackground / TitleForeground / AlertButtonGroupHeight / ContentForeground 等）
/// 全部删除，因为它们应该是 Style 资源而不是窗口实例属性。
/// </para>
/// </summary>
public class AlertWindow : Window
{
    static AlertWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AlertWindow),
            new FrameworkPropertyMetadata(typeof(AlertWindow)));
    }

    public AlertWindow()
    {
        this.AttachThemeManager();
        InitializeCommandBindings();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    #region Level

    public static readonly DependencyProperty LevelProperty =
        DependencyProperty.Register(
            nameof(Level),
            typeof(AlertIcon),
            typeof(AlertWindow),
            new PropertyMetadata(AlertIcon.None));

    /// <summary>
    /// 获取或设置图标级别。决定标题栏图标。<see cref="AlertIcon.None"/> 时不显示。
    /// </summary>
    public AlertIcon Level
    {
        get => (AlertIcon)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    #endregion Level

    #region HeaderIcon

    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(
            nameof(HeaderIcon),
            typeof(object),
            typeof(AlertWindow),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置标题栏左侧的自定义图标内容（任意 UI 对象）。
    /// 仅当 <see cref="Level"/> 为 <see cref="AlertIcon.None"/> 时生效——否则 Level 派生的图标优先。
    /// <para>注意：不再 hide 基类的 <c>Window.Icon</c>（任务栏图标），那个保留原语义。</para>
    /// </summary>
    public object HeaderIcon
    {
        get => GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    #endregion HeaderIcon

    #region ButtonType

    public static readonly DependencyProperty ButtonTypeProperty =
        DependencyProperty.Register(
            nameof(ButtonType),
            typeof(AlertButton),
            typeof(AlertWindow),
            new PropertyMetadata(AlertButton.Ok));

    /// <summary>获取或设置按钮组合（Ok / OkCancel）。</summary>
    public AlertButton ButtonType
    {
        get => (AlertButton)GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }

    #endregion ButtonType

    #region OkButtonText

    public static readonly DependencyProperty OkButtonTextProperty =
        DependencyProperty.Register(
            nameof(OkButtonText),
            typeof(string),
            typeof(AlertWindow),
            new PropertyMetadata("确定"));

    /// <summary>获取或设置"确定"按钮显示文本（i18n 用）。</summary>
    public string OkButtonText
    {
        get => (string)GetValue(OkButtonTextProperty);
        set => SetValue(OkButtonTextProperty, value);
    }

    #endregion OkButtonText

    #region CancelButtonText

    public static readonly DependencyProperty CancelButtonTextProperty =
        DependencyProperty.Register(
            nameof(CancelButtonText),
            typeof(string),
            typeof(AlertWindow),
            new PropertyMetadata("取消"));

    /// <summary>获取或设置"取消"按钮显示文本（i18n 用）。</summary>
    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    #endregion CancelButtonText

    #region IsLoading

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(AlertWindow),
            new PropertyMetadata(false));

    /// <summary>
    /// 是否显示加载覆盖层。异步验证执行期间由 <see cref="HandleOkAsync"/> 自动设置。
    /// </summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    #endregion IsLoading

    #region Validation Callbacks (普通 .NET 属性，不是 DP——这些是回调而非数据)

    /// <summary>异步验证回调：返回 false 阻止关闭。验证期间 IsLoading=true。</summary>
    public Func<Task<bool>> AsyncValidationCallback { get; set; }

    /// <summary>同步验证回调：返回 false 阻止关闭，窗口保持打开。</summary>
    public Func<bool> ValidationCallback { get; set; }

    #endregion Validation Callbacks (普通 .NET 属性，不是 DP——这些是回调而非数据)

    #region Commands

    /// <summary>"取消"按钮命令——直接 DialogResult=false 并关闭。</summary>
    public static readonly RoutedCommand CancelCommand = new RoutedCommand(nameof(CancelCommand), typeof(AlertWindow));

    /// <summary>"X"关闭按钮命令——DialogResult=null 并关闭。</summary>
    public static readonly RoutedCommand CloseCommand = new RoutedCommand(nameof(CloseCommand), typeof(AlertWindow));

    /// <summary>"确定"按钮命令——经过验证回调（如有）后设置 DialogResult=true。</summary>
    public static readonly RoutedCommand OkCommand = new RoutedCommand(nameof(OkCommand), typeof(AlertWindow));

    private async Task HandleOkAsync()
    {
        // 异步验证优先
        if (AsyncValidationCallback != null)
        {
            try
            {
                IsLoading = true;
                bool ok = await AsyncValidationCallback();
                if (ok)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AlertWindow] 异步验证抛出异常：{ex.Message}");
                IsLoading = false;
            }
            return;
        }

        // 同步验证
        if (ValidationCallback != null)
        {
            try
            {
                bool ok = ValidationCallback();
                if (ok)
                {
                    DialogResult = true;
                    Close();
                }

                // 验证 false：保持打开，无需操作
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AlertWindow] 同步验证抛出异常：{ex.Message}");
                DialogResult = false;
                Close();
            }
            return;
        }

        // 无验证：直接关
        DialogResult = true;
        Close();
    }

    private void InitializeCommandBindings()
    {
        CommandBindings.Add(new CommandBinding(OkCommand, async (_, _) => await HandleOkAsync()));
        CommandBindings.Add(new CommandBinding(CancelCommand, (_, _) =>
        {
            DialogResult = false;
            Close();
        }));
        CommandBindings.Add(new CommandBinding(CloseCommand, (_, _) =>
        {
            DialogResult = null;
            Close();
        }));
    }

    #endregion Commands
}