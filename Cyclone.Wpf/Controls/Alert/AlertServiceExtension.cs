using System;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// AlertService 的便捷扩展方法。
/// <para>
/// 修复了原版的副作用问题——之前 <c>Information / Success / Warning / Error</c> 等方法会修改
/// <c>self.Option.ButtonType</c>（全局副作用），调用一次后下次 Show 的默认按钮类型也变了。
/// 现在所有扩展方法都通过参数化 ButtonType 显式传递，不再触碰 Option。
/// </para>
/// </summary>
public static class AlertServiceExtension
{
    /// <summary>
    /// 显示一条带级别的消息，按钮类型可指定。所有命名方法（Information / Success 等）都委托到这里。
    /// </summary>
    public static AlertResult Notify(this IAlertService self, string message,
        AlertIcon level = AlertIcon.None,
        AlertButton buttons = AlertButton.Ok,
        string title = null)
    {
        if (self == null)
        {
            throw new ArgumentNullException(nameof(self));
        }

        var content = new AlertMessage
        {
            Level = level,
            Message = message,
        };
        return self.Show(content, buttons, title);
    }

    /// <summary>显示无图标的普通消息（仅 OK 按钮）。</summary>
    public static AlertResult Message(this IAlertService self, string message, string title = "提示")
        => self.Notify(message, AlertIcon.None, AlertButton.Ok, title);

    /// <summary>显示信息（蓝色 i 图标，仅 OK 按钮）。</summary>
    public static AlertResult Information(this IAlertService self, string message, string title = "信息")
        => self.Notify(message, AlertIcon.Information, AlertButton.Ok, title);

    /// <summary>显示成功提示（绿色对勾，仅 OK 按钮）。</summary>
    public static AlertResult Success(this IAlertService self, string message, string title = "成功")
        => self.Notify(message, AlertIcon.Success, AlertButton.Ok, title);

    /// <summary>显示警告（橙色三角，仅 OK 按钮）。</summary>
    public static AlertResult Warning(this IAlertService self, string message, string title = "警告")
        => self.Notify(message, AlertIcon.Warning, AlertButton.Ok, title);

    /// <summary>显示错误（红色 X，仅 OK 按钮）。</summary>
    public static AlertResult Error(this IAlertService self, string message, string title = "错误")
        => self.Notify(message, AlertIcon.Error, AlertButton.Ok, title);

    /// <summary>
    /// 询问确认（蓝色 ? 图标，OK + Cancel 按钮）。
    /// 返回 <see cref="AlertResult.Ok"/> 表示用户确认；其它值表示取消或关闭。
    /// </summary>
    public static AlertResult Confirm(this IAlertService self, string message, string title = "确认")
        => self.Notify(message, AlertIcon.Question, AlertButton.OkCancel, title);
}
