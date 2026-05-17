using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Controls;

// 消歧义:System.Windows.Controls.ValidationResult 与 System.ComponentModel.DataAnnotations.ValidationResult 同名,
// CustomValidation 用的是后者
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace Cyclone.Wpf.Demo.Views;

public partial class PasswordBoxSample : UserControl
{
    public PasswordBoxSample()
    {
        InitializeComponent();
        DataContext = new PasswordBoxViewModel();
    }
}

public partial class PasswordBoxViewModel : ObservableValidator
{
    // 密码 — MinLength 验证(空字符串合法,以保证初始无错误状态;输入但短于 8 位报错)
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [MinLength(8, ErrorMessage = "密码至少 8 位")]
    public partial string Password { get; set; } = "";

    // 确认密码 — 用 [CustomValidation] 引用静态验证方法,做跨字段比较。
    // Password 变化时通过 OnPasswordChanged 手动触发本属性的重新校验。
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(PasswordBoxViewModel), nameof(ValidateConfirmPassword))]
    public partial string ConfirmPassword { get; set; } = "";

    [ObservableProperty]
    public partial string LoginAccount { get; set; } = "";

    [ObservableProperty]
    public partial string LoginPassword { get; set; } = "";

    [ObservableProperty]
    public partial string LastEvent { get; set; } = "(尚未输入)";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public bool CanInteract => !IsBusy;

    public int PasswordLength => Password?.Length ?? 0;

    public string PasswordStrength => GetStrength(Password);

    private static string GetStrength(string pwd)
    {
        if (string.IsNullOrEmpty(pwd))
        {
            return "—";
        }
        int score = 0;
        if (pwd.Length >= 8)
        {
            score++;
        }
        if (pwd.Any(char.IsUpper) && pwd.Any(char.IsLower))
        {
            score++;
        }
        if (pwd.Any(char.IsDigit))
        {
            score++;
        }
        if (pwd.Any(c => !char.IsLetterOrDigit(c)))
        {
            score++;
        }
        return score switch
        {
            <= 1 => "弱",
            2 => "中",
            _ => "强",
        };
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(CanInteract));
    }

    partial void OnPasswordChanged(string value)
    {
        OnPropertyChanged(nameof(PasswordLength));
        OnPropertyChanged(nameof(PasswordStrength));
        LastEvent = $"Password → 长度 {value?.Length ?? 0}";

        // Password 一变,ConfirmPassword 的校验结果可能跟着失效(原本相等的现在可能不等了),
        // 手动触发其重新校验
        if (!string.IsNullOrEmpty(ConfirmPassword))
        {
            ValidateProperty(ConfirmPassword, nameof(ConfirmPassword));
        }
    }

    partial void OnConfirmPasswordChanged(string value)
    {
        LastEvent = $"Confirm → 长度 {value?.Length ?? 0}";
    }

    partial void OnLoginPasswordChanged(string value)
    {
        LastEvent = $"LoginPwd → 长度 {value?.Length ?? 0}";
    }

    [RelayCommand]
    private void Login()
    {
        if (string.IsNullOrEmpty(LoginAccount) || string.IsNullOrEmpty(LoginPassword))
        {
            LastEvent = "[登录] 账号或密码为空";
            return;
        }
        LastEvent = $"[登录] {LoginAccount} 用 {LoginPassword.Length} 位密码尝试登录";
    }

    [RelayCommand]
    private void ToggleBusy()
    {
        IsBusy = !IsBusy;
    }

    [RelayCommand]
    private void Reset()
    {
        Password = "";
        ConfirmPassword = "";
        LoginAccount = "";
        LoginPassword = "";
        LastEvent = "(已重置)";
        ClearErrors();
    }

    /// <summary>
    /// ConfirmPassword 的跨字段验证 — CustomValidationAttribute 调用此静态方法。
    /// 空值视为合法(避免初始空状态报错),非空时与当前 Password 做比较。
    /// </summary>
    public static ValidationResult ValidateConfirmPassword(string value, ValidationContext context)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ValidationResult.Success;
        }
        var vm = (PasswordBoxViewModel)context.ObjectInstance;
        if (value != vm.Password)
        {
            return new ValidationResult("两次输入的密码不一致");
        }
        return ValidationResult.Success;
    }
}