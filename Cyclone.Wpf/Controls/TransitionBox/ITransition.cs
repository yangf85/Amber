using System.Windows;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 内容过渡动画的扩展点。
/// <para>
/// 实现者只负责"生产 Storyboard"——不负责启动、取消、清理资源。
/// 这些职责由 <see cref="TransitionBox"/> 统一管理，避免实现者重复处理状态机和资源生命周期。
/// </para>
/// </summary>
public interface ITransition
{
    /// <summary>
    /// 创建一个驱动两个元素过渡的 Storyboard（不启动）。
    /// </summary>
    /// <param name="oldElement">即将退场的旧内容元素（已经在视觉树中，初始可见）。</param>
    /// <param name="newElement">即将进场的新内容元素（已经在视觉树中，调用前 Opacity 可能为 0，需要由动画驱动到 1）。</param>
    /// <param name="containerSize">承载容器（TransitionBox）的实际尺寸，供需要尺寸的动画使用（如滑动）。</param>
    /// <param name="duration">动画总时长。</param>
    /// <returns>
    /// 一个完整的 Storyboard，启动后驱动整个过渡动画。
    /// 调用方需要负责 Begin / Stop / Completed 订阅。
    /// </returns>
    Storyboard CreateAnimation(
        FrameworkElement oldElement,
        FrameworkElement newElement,
        Size containerSize,
        Duration duration);
}
