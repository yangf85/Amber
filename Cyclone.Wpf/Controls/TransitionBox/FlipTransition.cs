using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 翻转过渡：两阶段串行——旧内容沿轴向收缩到 0，再让新内容从 0 展开到 1。
/// <para>视觉上像翻牌的前半页与后半页。</para>
/// </summary>
[MarkupExtensionReturnType(typeof(ITransition))]
public class FlipTransition : MarkupExtension, ITransition
{
    /// <summary>
    /// 翻转轴。<see cref="Orientation.Horizontal"/> = 沿垂直轴翻转（左右翻牌，宽度收缩）；
    /// <see cref="Orientation.Vertical"/> = 沿水平轴翻转（上下翻牌，高度收缩）。
    /// 默认 Horizontal。
    /// </summary>
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    private static void AddScale(
        Storyboard storyboard, FrameworkElement target, bool isX,
        double from, double to, Duration duration, TimeSpan beginTime, IEasingFunction ease)
    {
        var anim = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = duration,
            BeginTime = beginTime,
            EasingFunction = ease,
        };
        Storyboard.SetTarget(anim, target);
        var pathStr = isX
            ? "(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
            : "(UIElement.RenderTransform).(ScaleTransform.ScaleY)";
        Storyboard.SetTargetProperty(anim, new PropertyPath(pathStr));
        storyboard.Children.Add(anim);
    }

    public Storyboard CreateAnimation(
            FrameworkElement oldElement,
        FrameworkElement newElement,
        Size containerSize,
        Duration duration)
    {
        var halfDuration = new Duration(TimeSpan.FromMilliseconds(duration.TimeSpan.TotalMilliseconds / 2));
        var phaseTwoBegin = halfDuration.TimeSpan;

        bool isHorizontal = Orientation == Orientation.Horizontal;

        // 旧元素 RenderTransform 初始为 1
        oldElement.RenderTransform = new ScaleTransform(1.0, 1.0);
        oldElement.RenderTransformOrigin = new Point(0.5, 0.5);
        oldElement.Opacity = 1.0;

        // 新元素 RenderTransform 初始按轴向缩到 0
        newElement.RenderTransform = isHorizontal
            ? new ScaleTransform(0.0, 1.0)
            : new ScaleTransform(1.0, 0.0);
        newElement.RenderTransformOrigin = new Point(0.5, 0.5);
        newElement.Opacity = 1.0;

        var storyboard = new Storyboard();
        var easeIn = new PowerEase { Power = 2, EasingMode = EasingMode.EaseIn };
        var easeOut = new PowerEase { Power = 2, EasingMode = EasingMode.EaseOut };

        // 第一阶段：旧元素轴向 1 → 0
        AddScale(storyboard, oldElement, isHorizontal, 1.0, 0.0, halfDuration, TimeSpan.Zero, easeIn);

        // 第二阶段：新元素轴向 0 → 1（延迟 halfDuration）
        AddScale(storyboard, newElement, isHorizontal, 0.0, 1.0, halfDuration, phaseTwoBegin, easeOut);

        return storyboard;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}