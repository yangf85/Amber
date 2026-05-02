using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>滑动方向。</summary>
public enum SlideDirection
{
    /// <summary>新内容从左侧滑入，旧内容向右滑出。</summary>
    LeftToRight,

    /// <summary>新内容从右侧滑入，旧内容向左滑出。</summary>
    RightToLeft,

    /// <summary>新内容从上方滑入，旧内容向下滑出。</summary>
    TopToBottom,

    /// <summary>新内容从下方滑入，旧内容向上滑出。</summary>
    BottomToTop,
}

/// <summary>
/// 滑动过渡：新旧内容沿指定方向同时位移。
/// </summary>
[MarkupExtensionReturnType(typeof(ITransition))]
public class SlideTransition : MarkupExtension, ITransition
{
    /// <summary>滑动方向。默认 <see cref="SlideDirection.RightToLeft"/>（新内容从右侧滑入）。</summary>
    public SlideDirection Direction { get; set; } = SlideDirection.RightToLeft;

    private static void AddTranslateAnimation(
        Storyboard storyboard, FrameworkElement target, bool isX,
        double from, double to, Duration duration, IEasingFunction ease)
    {
        if (from == 0 && to == 0)
        {
            return;
        }
        var anim = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = duration,
            EasingFunction = ease,
        };
        Storyboard.SetTarget(anim, target);
        var pathStr = isX
            ? "(UIElement.RenderTransform).(TranslateTransform.X)"
            : "(UIElement.RenderTransform).(TranslateTransform.Y)";
        Storyboard.SetTargetProperty(anim, new PropertyPath(pathStr));
        storyboard.Children.Add(anim);
    }

    public Storyboard CreateAnimation(
            FrameworkElement oldElement,
        FrameworkElement newElement,
        Size containerSize,
        Duration duration)
    {
        double w = containerSize.Width;
        double h = containerSize.Height;
        double oldToX = 0, oldToY = 0;
        double newFromX = 0, newFromY = 0;

        switch (Direction)
        {
            case SlideDirection.LeftToRight:
                newFromX = -w;
                oldToX = w;
                break;

            case SlideDirection.RightToLeft:
                newFromX = w;
                oldToX = -w;
                break;

            case SlideDirection.TopToBottom:
                newFromY = -h;
                oldToY = h;
                break;

            case SlideDirection.BottomToTop:
                newFromY = h;
                oldToY = -h;
                break;
        }

        // 给两个元素各装一个 TranslateTransform 作为 RenderTransform
        // 关键：动画 target 是元素，路径沿 RenderTransform 链——这样 isControllable Storyboard 才能解析
        oldElement.RenderTransform = new TranslateTransform();
        newElement.RenderTransform = new TranslateTransform(newFromX, newFromY);

        newElement.Opacity = 1.0;
        oldElement.Opacity = 1.0;

        var storyboard = new Storyboard();
        var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };

        // 旧元素：从 (0,0) 滑到 (oldToX, oldToY)
        AddTranslateAnimation(storyboard, oldElement, isX: true, 0, oldToX, duration, ease);
        AddTranslateAnimation(storyboard, oldElement, isX: false, 0, oldToY, duration, ease);

        // 新元素：从 (newFromX, newFromY) 滑回 (0, 0)
        AddTranslateAnimation(storyboard, newElement, isX: true, newFromX, 0, duration, ease);
        AddTranslateAnimation(storyboard, newElement, isX: false, newFromY, 0, duration, ease);

        return storyboard;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}