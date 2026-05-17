using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// 提供 PasswordBox 的扩展功能：水印、可绑定 Password、明文/密文切换、清空命令。
/// </summary>
public static class PasswordBoxHelper
{
    static PasswordBoxHelper()
    {
        CommandManager.RegisterClassCommandBinding(
            typeof(PasswordBox),
            new CommandBinding(clearCommand, ExecuteClearCommand));
    }

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.RegisterAttached(
            "Watermark",
            typeof(string),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(string.Empty));

    public static string GetWatermark(DependencyObject obj) => (string)obj.GetValue(WatermarkProperty);

    public static void SetWatermark(DependencyObject obj, string value) => obj.SetValue(WatermarkProperty, value);

    #endregion Watermark

    #region Password

    // 内部循环抑制标志:标记当前正处于"附加属性 <-> PasswordBox.Password" 双向同步过程中,
    // 任何一侧的赋值都通过它压制对侧的回调,防止形成无限循环。
    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached(
            "IsUpdating",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false));

    // 内部标志:记录 PasswordChanged 事件是否已订阅。
    // 解决 "绑定初值与默认值相同时 OnPasswordChanged 不会触发,导致事件订阅缺失" 的问题。
    private static readonly DependencyProperty IsHandlerAttachedProperty =
        DependencyProperty.RegisterAttached(
            "IsHandlerAttached",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false));

    public static readonly DependencyProperty PasswordProperty =
                DependencyProperty.RegisterAttached(
            "Password",
            typeof(string),
            typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(string.Empty, OnPasswordChanged)
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            });

    private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
        {
            return;
        }

        // 双保险:如果事件还没订阅(比如初值就匹配默认值,本回调之前从未触发过),这里再补一次
        AttachHandlerIfNeeded(passwordBox);

        // 由 PasswordBox_PasswordChanged 回写引发,跳过避免回环
        if ((bool)passwordBox.GetValue(IsUpdatingProperty))
        {
            return;
        }

        // 明文展示状态下,密文输入区已折叠,无需也不应该回写 PasswordBox.Password。
        // 切回密文模式时 (OnShowPasswordChanged 的 false 分支) 会做一次性同步。
        if (GetShowPassword(passwordBox))
        {
            return;
        }

        string newPassword = (string)e.NewValue ?? string.Empty;
        if (passwordBox.Password == newPassword)
        {
            return;
        }

        passwordBox.SetValue(IsUpdatingProperty, true);
        try
        {
            passwordBox.Password = newPassword;
        }
        finally
        {
            passwordBox.SetValue(IsUpdatingProperty, false);
        }
    }

    private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
        {
            return;
        }

        // 由 OnPasswordChanged 赋值引发,跳过避免回环
        if ((bool)passwordBox.GetValue(IsUpdatingProperty))
        {
            return;
        }

        passwordBox.SetValue(IsUpdatingProperty, true);
        try
        {
            SetPassword(passwordBox, passwordBox.Password);
        }
        finally
        {
            passwordBox.SetValue(IsUpdatingProperty, false);
        }
    }

    /// <summary>
    /// 幂等地为 PasswordBox 订阅 PasswordChanged 事件。
    /// 由 OnPasswordChanged / OnHelperFlagChanged 共同触发,确保至少一条路径必定订阅成功。
    /// </summary>
    private static void AttachHandlerIfNeeded(PasswordBox passwordBox)
    {
        if ((bool)passwordBox.GetValue(IsHandlerAttachedProperty))
        {
            return;
        }
        passwordBox.SetValue(IsHandlerAttachedProperty, true);
        passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
    }

    public static string GetPassword(DependencyObject obj) => (string)obj.GetValue(PasswordProperty);

    public static void SetPassword(DependencyObject obj, string value) => obj.SetValue(PasswordProperty, value);

    #endregion Password

    #region HasClearButton

    public static readonly DependencyProperty HasClearButtonProperty =
        DependencyProperty.RegisterAttached(
            "HasClearButton",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false, OnHelperFlagChanged));

    public static bool GetHasClearButton(DependencyObject obj) => (bool)obj.GetValue(HasClearButtonProperty);

    public static void SetHasClearButton(DependencyObject obj, bool value) => obj.SetValue(HasClearButtonProperty, value);

    #endregion HasClearButton

    #region HasPasswordVisibilityToggle

    public static readonly DependencyProperty HasPasswordVisibilityToggleProperty =
        DependencyProperty.RegisterAttached(
            "HasPasswordVisibilityToggle",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false, OnHelperFlagChanged));

    /// <summary>
    /// 默认样式将 HasClearButton / HasPasswordVisibilityToggle 由 false 翻转到 true,
    /// 必定触发此回调一次,作为兜底确保 PasswordChanged 事件被订阅 ——
    /// 即使用户的 Password 绑定初值与默认 string.Empty 相同导致 OnPasswordChanged 不触发,
    /// 此处也能保证后续用户键入能正确回写到 ViewModel。
    /// </summary>
    private static void OnHelperFlagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox)
        {
            AttachHandlerIfNeeded(passwordBox);
        }
    }

    public static bool GetHasPasswordVisibilityToggle(DependencyObject obj) =>
            (bool)obj.GetValue(HasPasswordVisibilityToggleProperty);

    public static void SetHasPasswordVisibilityToggle(DependencyObject obj, bool value) =>
        obj.SetValue(HasPasswordVisibilityToggleProperty, value);

    #endregion HasPasswordVisibilityToggle

    #region ShowPassword

    public static readonly DependencyProperty ShowPasswordProperty =
        DependencyProperty.RegisterAttached(
            "ShowPassword",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false, OnShowPasswordChanged));

    private static void OnShowPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
        {
            return;
        }

        bool showPassword = (bool)e.NewValue;

        if (showPassword)
        {
            // 进入明文模式:把 PasswordBox.Password 当前值同步到 Password 附加属性,
            // 之后 UI 上的明文 TextBlock 通过绑定 Password 附加属性来显示
            SetPassword(passwordBox, passwordBox.Password);

            // 失焦自动隐藏明文 — 出于安全考虑
            passwordBox.LostFocus -= PasswordBox_LostFocus;
            passwordBox.LostFocus += PasswordBox_LostFocus;
        }
        else
        {
            // 退出明文模式:若明文期间 ViewModel 通过绑定回写了 Password 附加属性,
            // 此时 PasswordBox.Password 仍是旧值(OnPasswordChanged 在 ShowPassword=true 时跳过了同步),
            // 需要在切回密文前做一次性同步,否则下方密文框会显示陈旧内容
            string current = GetPassword(passwordBox) ?? string.Empty;
            if (passwordBox.Password != current)
            {
                passwordBox.SetValue(IsUpdatingProperty, true);
                try
                {
                    passwordBox.Password = current;
                }
                finally
                {
                    passwordBox.SetValue(IsUpdatingProperty, false);
                }
            }

            passwordBox.LostFocus -= PasswordBox_LostFocus;
        }
    }

    private static void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            SetShowPassword(passwordBox, false);
        }
    }

    public static bool GetShowPassword(DependencyObject obj) => (bool)obj.GetValue(ShowPasswordProperty);

    public static void SetShowPassword(DependencyObject obj, bool value) => obj.SetValue(ShowPasswordProperty, value);

    #endregion ShowPassword

    #region ClearCommand

    private static readonly RoutedCommand clearCommand =
        new RoutedCommand("ClearPassword", typeof(PasswordBoxHelper));

    public static RoutedCommand ClearCommand => clearCommand;

    private static void ExecuteClearCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            passwordBox.Clear();
            // PasswordBox.Clear() 已经触发 PasswordChanged → 自动同步 Password 附加属性,
            // 这里显式再设一次作为兜底,应对极端情况下事件未订阅的场景。
            SetPassword(passwordBox, string.Empty);
        }
    }

    #endregion ClearCommand
}