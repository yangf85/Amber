namespace Cyclone.Wpf.Controls;

/// <summary>
/// AlertService 的配置。
/// <para>
/// 注意：mutable POCO，<b>不实现</b> <see cref="System.ComponentModel.INotifyPropertyChanged"/>。
/// 在调用任何 Show 之前完成所有配置；运行时改值会影响下次 Show。
/// </para>
/// <para>
/// 视觉相关（颜色、字体、按钮高度、Caption 样式、加载动画等）全部由 <c>Styles/Alert.xaml</c>
/// 主题字典控制，不通过此类配置——这样主题切换时通知会自动响应。
/// </para>
/// </summary>
public class AlertOption
{
    /// <summary>未指定按钮组合时使用的默认值。</summary>
    public AlertButton DefaultButtonType { get; set; } = AlertButton.Ok;

    /// <summary>"确定"按钮文本（i18n 用）。</summary>
    public string OkButtonText { get; set; } = "确定";

    /// <summary>"取消"按钮文本（i18n 用）。</summary>
    public string CancelButtonText { get; set; } = "取消";

    /// <summary>是否在 owner 之上显示半透明蒙版（独立 Window 实现）。</summary>
    public bool IsShowMask { get; set; } = true;

    /// <summary>异步验证期间是否显示加载动画。</summary>
    public bool IsShowLoadingOnAsync { get; set; } = true;
}
