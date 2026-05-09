using System;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 警告对话框服务接口。所有 <c>Show</c> 方法都是<b>模态阻塞</b>——返回时窗口已关闭。
/// <para>异步验证场景用 <see cref="ShowAsync"/>，验证期间会显示加载动画（受 <see cref="AlertOption.IsShowLoadingOnAsync"/> 控制）。</para>
/// </summary>
public interface IAlertService : IDisposable
{
    /// <summary>使用 <see cref="AlertOption.DefaultButtonType"/> 显示模态对话框。</summary>
    AlertResult Show(object content, string title = null);

    /// <summary>使用指定的按钮组合显示模态对话框。</summary>
    AlertResult Show(object content, AlertButton buttons, string title = null);

    /// <summary>
    /// 显示带同步验证的对话框（按钮固定 OkCancel）。
    /// 用户点 OK 时调用 <paramref name="validation"/>——返回 <c>true</c> 才允许关闭，<c>false</c> 时窗口保持打开。
    /// </summary>
    AlertResult Show(object content, Func<bool> validation, string title = null);

    /// <summary>
    /// 显示带异步验证的对话框（按钮固定 OkCancel）。验证期间显示加载动画。
    /// </summary>
    Task<AlertResult> ShowAsync(object content, Func<Task<bool>> asyncValidation, string title = null);

    /// <summary>配置选项。</summary>
    AlertOption Option { get; }
}
