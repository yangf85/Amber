using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 淡入淡出过渡：旧内容 Opacity 1→0，新内容 Opacity 0→1。
/// <para>同时是 <see cref="MarkupExtension"/>，可在 XAML 中以 <c>{cy:Fade}</c> 短语法使用。</para>
/// </summary>
[MarkupExtensionReturnType(typeof(ITransition))]
public class FadeTransition : MarkupExtension, ITransition
{
    public Storyboard CreateAnimation(
        FrameworkElement oldElement,
        FrameworkElement newElement,
        Size containerSize,
        Duration duration)
    {
        var storyboard = new Storyboard();
        var ease = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

        // 旧元素淡出
        var fadeOut = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = duration,
            EasingFunction = ease,
        };
        Storyboard.SetTarget(fadeOut, oldElement);
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(fadeOut);

        // 新元素淡入
        var fadeIn = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = duration,
            EasingFunction = ease,
        };
        Storyboard.SetTarget(fadeIn, newElement);
        Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(fadeIn);

        return storyboard;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
