// ============================================================================
//  破坏性变更说明（vs 旧 HyperlinkButton）：
//    - 基类 Control → ButtonBase（自动获得 Click 事件 / Command / 键盘 Space-Enter 触发）
//    - 删除 OpenUrlCommand RoutedCommand —— 不再需要内嵌 Button 中转
//      迁移：原本绑 Command="{x:Static cy:HyperlinkButton.OpenUrlCommand}" 的代码不再需要
//    - URI scheme 白名单：仅允许 http / https / mailto——其他协议（file/ms-msdt/自定义）会被忽略
//      迁移：如果原代码依赖 file:// 等协议，需改用 Click 事件自定义跳转
// ============================================================================
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 超链接按钮——点击后用系统默认浏览器打开 <see cref="NavigateUri"/> 指定的 URL。<br/>
/// 继承自 <see cref="ButtonBase"/>，自动支持 Click 事件 / Command / 键盘 Space-Enter 触发。
/// 仅允许 http / https / mailto 三种 scheme。如需其他跳转行为，监听 Click 事件 / 绑定 Command。
/// </summary>
[TemplatePart(Name = PartDisplayTextBlock, Type = typeof(TextBlock))]
public class HyperlinkButton : ButtonBase
{
    private const string PartDisplayTextBlock = nameof(PartDisplayTextBlock);

    /// <summary>允许的 URI scheme 白名单——避免 file:/// 或 ms-msdt: 等协议被 shell 直接执行。</summary>
    private static readonly string[] AllowedSchemes = { "http", "https", "mailto" };

    static HyperlinkButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HyperlinkButton),
            new FrameworkPropertyMetadata(typeof(HyperlinkButton)));

        // 链接默认左对齐——超链接通常按文字宽度而非撑满父容器
        HorizontalAlignmentProperty.OverrideMetadata(typeof(HyperlinkButton),
            new FrameworkPropertyMetadata(HorizontalAlignment.Left));
    }

    #region NavigateUri

    public static readonly DependencyProperty NavigateUriProperty =
        DependencyProperty.Register(
            nameof(NavigateUri),
            typeof(Uri),
            typeof(HyperlinkButton),
            new PropertyMetadata(null));

    /// <summary>点击时打开的 URL。必须是绝对 URI 且 scheme 在白名单内（http/https/mailto）。</summary>
    public Uri NavigateUri
    {
        get => (Uri)GetValue(NavigateUriProperty);
        set => SetValue(NavigateUriProperty, value);
    }

    #endregion NavigateUri

    #region DisplayText

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(
            nameof(DisplayText),
            typeof(string),
            typeof(HyperlinkButton),
            new PropertyMetadata(null));

    /// <summary>显示的链接文本。一般跟 NavigateUri 不同（如 "点这里" 链接到 https://...）。</summary>
    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    #endregion DisplayText

    #region Override Methods

    /// <summary>
    /// 点击触发。先 raise Click 事件 + 触发 Command（ButtonBase 标准行为），
    /// 然后默认尝试用系统浏览器打开 NavigateUri。
    /// </summary>
    protected override void OnClick()
    {
        base.OnClick();
        OpenUrl();
    }

    #endregion Override Methods

    #region Private Methods

    private void OpenUrl()
    {
        var uri = NavigateUri;
        if (uri == null)
        {
            return;
        }

        // 必须是 absolute URI——访问 AbsoluteUri 在相对 URI 上抛 InvalidOperationException
        if (!uri.IsAbsoluteUri)
        {
            Trace.TraceWarning("[HyperlinkButton] NavigateUri 是相对 URI，已忽略：{0}", uri);
            return;
        }

        // 安全：scheme 白名单——拒绝 file:/// / ms-msdt: / 自定义协议等
        if (Array.IndexOf(AllowedSchemes, uri.Scheme.ToLowerInvariant()) < 0)
        {
            Trace.TraceWarning("[HyperlinkButton] scheme '{0}' 不在白名单，已忽略：{1}", uri.Scheme, uri);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            // 不静默吞——trace 让 dev 知道（但不抛——避免 dispatcher unhandled）
            Trace.TraceWarning("[HyperlinkButton] 打开 URL 失败：{0}", ex.Message);
        }
    }

    #endregion Private Methods
}