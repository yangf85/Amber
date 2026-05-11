using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;

namespace Cyclone.Wpf.Helpers;

[Obsolete("实现起来有点复杂,不要使用")]
public static class ExpanderHelper
{
    #region AnimationDuration 附加属性

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.RegisterAttached(
            "AnimationDuration",
            typeof(TimeSpan),
            typeof(ExpanderHelper),
            new PropertyMetadata(TimeSpan.FromMilliseconds(200)));

    public static TimeSpan GetAnimationDuration(DependencyObject obj)
    {
        return (TimeSpan)obj.GetValue(AnimationDurationProperty);
    }

    public static void SetAnimationDuration(DependencyObject obj, TimeSpan value)
    {
        obj.SetValue(AnimationDurationProperty, value);
    }

    #endregion AnimationDuration 附加属性

    #region IsAnimationEnabled 附加属性

    public static readonly DependencyProperty IsAnimationEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsAnimationEnabled",
            typeof(bool),
            typeof(ExpanderHelper),
            new PropertyMetadata(false, OnIsAnimationEnabledChanged));

    public static bool GetIsAnimationEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsAnimationEnabledProperty);
    }

    public static void SetIsAnimationEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsAnimationEnabledProperty, value);
    }

    private static void OnIsAnimationEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var expander = d as Expander;
        if (expander == null)
            return;

        if ((bool)e.NewValue)
        {
            // 添加展开状态改变的事件处理
            expander.Expanded += Expander_Expanded;
            expander.Collapsed += Expander_Collapsed;
        }
        else
        {
            // 移除事件处理
            expander.Expanded -= Expander_Expanded;
            expander.Collapsed -= Expander_Collapsed;
        }
    }

    #endregion IsAnimationEnabled 附加属性

    #region OriginalHeight 附加属性

    public static readonly DependencyProperty OriginalHeightProperty =
        DependencyProperty.RegisterAttached(
            "OriginalHeight",
            typeof(double),
            typeof(ExpanderHelper),
            new PropertyMetadata(double.NaN));

    private static double GetOriginalHeight(DependencyObject obj)
    {
        return (double)obj.GetValue(OriginalHeightProperty);
    }

    private static void SetOriginalHeight(DependencyObject obj, double value)
    {
        obj.SetValue(OriginalHeightProperty, value);
    }

    #endregion OriginalHeight 附加属性

    #region IsAnimating 附加属性

    // 添加一个新的附加属性，用于跟踪控件是否正在动画中
    public static readonly DependencyProperty IsAnimatingProperty =
        DependencyProperty.RegisterAttached(
            "IsAnimating",
            typeof(bool),
            typeof(ExpanderHelper),
            new PropertyMetadata(false));

    private static bool GetIsAnimating(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsAnimatingProperty);
    }

    private static void SetIsAnimating(DependencyObject obj, bool value)
    {
        obj.SetValue(IsAnimatingProperty, value);
    }

    #endregion IsAnimating 附加属性

    // 处理 Expander 收缩的事件
    private static void Expander_Collapsed(object sender, RoutedEventArgs e)
    {
        var expander = sender as Expander;
        if (expander?.Content is FrameworkElement content)
        {
            // 如果已经在动画中，忽略此次请求
            if (GetIsAnimating(content))
                return;

            // 标记为正在动画
            SetIsAnimating(content, true);

            // 获取动画时长
            var duration = GetAnimationDuration(expander);

            // 获取当前高度
            double currentHeight = content.ActualHeight;

            // 确保我们保存了原始高度
            double originalHeight = GetOriginalHeight(content);
            if (double.IsNaN(originalHeight))
            {
                originalHeight = currentHeight > 0 ? currentHeight : 100; // 使用默认高度防止错误
                SetOriginalHeight(content, originalHeight);
            }

            // 创建并应用高度动画
            var animation = new DoubleAnimation
            {
                From = currentHeight,
                To = 0,
                Duration = new Duration(duration),
                FillBehavior = FillBehavior.HoldEnd
            };

            animation.Completed += (s, args) =>
            {
                // 动画完成后设置状态
                content.Visibility = Visibility.Collapsed;
                content.Height = 0;

                // 标记动画已完成
                SetIsAnimating(content, false);

                // 检查Expander当前状态，如果已展开，立即应用展开效果
                if (expander.IsExpanded)
                {
                    content.Visibility = Visibility.Visible;
                    content.Height = originalHeight;
                }
            };

            content.BeginAnimation(FrameworkElement.HeightProperty, animation);
        }
    }

    // 处理 Expander 展开的事件
    private static void Expander_Expanded(object sender, RoutedEventArgs e)
    {
        var expander = sender as Expander;
        if (expander?.Content is FrameworkElement content)
        {
            // 如果已经在动画中，忽略此次请求
            if (GetIsAnimating(content))
                return;

            // 标记为正在动画
            SetIsAnimating(content, true);

            // 确保 Content 可见
            content.Visibility = Visibility.Visible;

            // 获取动画时长
            var duration = GetAnimationDuration(expander);

            // 获取或确定目标高度
            double targetHeight = GetOriginalHeight(content);
            if (double.IsNaN(targetHeight))
            {
                // 首次展开，需要确定并存储原始高度
                bool hasExplicitHeight = !content.ReadLocalValue(FrameworkElement.HeightProperty).Equals(DependencyProperty.UnsetValue);

                if (hasExplicitHeight)
                {
                    // 如果内容有显式设置的高度，使用这个高度
                    targetHeight = content.Height;
                }
                else
                {
                    // 先移除任何可能设置的Height属性
                    content.ClearValue(FrameworkElement.HeightProperty);
                    content.UpdateLayout();

                    // 测量内容
                    content.Measure(new Size(content.ActualWidth > 0 ? content.ActualWidth :
                                            (expander.ActualWidth > 0 ? expander.ActualWidth : double.PositiveInfinity),
                                            double.PositiveInfinity));
                    targetHeight = content.DesiredSize.Height;

                    // 如果测量高度仍然为0，尝试使用ActualHeight
                    if (targetHeight <= 0 && content.ActualHeight > 0)
                    {
                        targetHeight = content.ActualHeight;
                    }

                    // 如果高度仍然为0，设置一个默认高度防止错误
                    if (targetHeight <= 0)
                    {
                        targetHeight = 100; // 默认高度
                    }
                }

                // 存储这个高度以供将来使用
                SetOriginalHeight(content, targetHeight);
            }

            // 创建并应用高度动画
            content.Height = 0;
            var animation = new DoubleAnimation
            {
                From = 0,
                To = targetHeight,
                Duration = new Duration(duration),
                FillBehavior = FillBehavior.HoldEnd
            };

            animation.Completed += (s, args) =>
            {
                // 动画完成后，设置为存储的原始高度
                content.Height = targetHeight;

                // 标记动画已完成
                SetIsAnimating(content, false);

                // 检查Expander当前状态，如果不是展开状态，立即应用收缩效果
                if (expander.IsExpanded == false)
                {
                    content.Visibility = Visibility.Collapsed;
                    content.Height = 0;
                }
            };

            content.BeginAnimation(FrameworkElement.HeightProperty, animation);
        }
    }
}