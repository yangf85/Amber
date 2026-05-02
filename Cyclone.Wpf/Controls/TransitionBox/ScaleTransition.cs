using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 缩放过渡：旧内容从 1 缩放至 0，新内容从 0 放大至 1，两阶段串行（共占 <c>duration</c>）。
/// </summary>
[MarkupExtensionReturnType(typeof(ITransition))]
public class ScaleTransition : MarkupExtension, ITransition
{
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

        // 给两个元素各装 ScaleTransform，origin 在中心
        oldElement.RenderTransform = new ScaleTransform(1.0, 1.0);
        newElement.RenderTransform = new ScaleTransform(0.0, 0.0);
        oldElement.RenderTransformOrigin = new Point(0.5, 0.5);
        newElement.RenderTransformOrigin = new Point(0.5, 0.5);
        newElement.Opacity = 1.0;
        oldElement.Opacity = 1.0;

        var storyboard = new Storyboard();
        var ease = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut };

        // 第一阶段：旧元素 ScaleX / Y 从 1 → 0
        AddScale(storyboard, oldElement, isX: true, 1.0, 0.0, halfDuration, TimeSpan.Zero, ease);
        AddScale(storyboard, oldElement, isX: false, 1.0, 0.0, halfDuration, TimeSpan.Zero, ease);

        // 第二阶段：新元素 ScaleX / Y 从 0 → 1
        AddScale(storyboard, newElement, isX: true, 0.0, 1.0, halfDuration, phaseTwoBegin, ease);
        AddScale(storyboard, newElement, isX: false, 0.0, 1.0, halfDuration, phaseTwoBegin, ease);

        return storyboard;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}