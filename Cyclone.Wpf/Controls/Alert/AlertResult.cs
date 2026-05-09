namespace Cyclone.Wpf.Controls;

/// <summary>
/// 警告对话框关闭后的结果。替代之前 <c>bool?</c> 三态写法，语义更明确。
/// </summary>
public enum AlertResult
{
    /// <summary>用户点击了"确定"按钮。对应旧 API 的 <c>true</c>。</summary>
    Ok,

    /// <summary>用户点击了"取消"按钮。对应旧 API 的 <c>false</c>。</summary>
    Cancel,

    /// <summary>对话框被关闭（点 X 按钮、按 Esc、系统强制关闭等），不是显式确认或取消。对应旧 API 的 <c>null</c>。</summary>
    Closed,
}
