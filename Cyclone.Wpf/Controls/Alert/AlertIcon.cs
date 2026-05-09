namespace Cyclone.Wpf.Controls;

/// <summary>
/// 警告对话框的图标语义级别。
/// 决定 <see cref="AlertMessage"/> 的颜色和图标，以及 <see cref="AlertWindow"/> 标题栏图标。
/// </summary>
public enum AlertIcon
{
    /// <summary>无图标。</summary>
    None,

    /// <summary>询问（蓝色问号）——用于 OkCancel 二选一确认场景。</summary>
    Question,

    /// <summary>信息（蓝色 i）——一般性提示。</summary>
    Information,

    /// <summary>错误（红色 X）——操作失败。</summary>
    Error,

    /// <summary>警告（橙色三角感叹号）——潜在风险提醒。</summary>
    Warning,

    /// <summary>成功（绿色对勾）——操作成功。</summary>
    Success,
}
