using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 脉冲点加载动画控件 - 显示3个依次脉冲的圆点
/// </summary>
public class LoadingPulse : LoadingIndicator
{
    #region 依赖属性

    public static readonly DependencyProperty DotColorProperty =
        DependencyProperty.Register(nameof(DotColor), typeof(Brush), typeof(LoadingPulse),
            new PropertyMetadata(new SolidColorBrush(Colors.White), OnVisualPropertyChanged));

    public static readonly DependencyProperty DotSizeProperty =
        DependencyProperty.Register(nameof(DotSize), typeof(double), typeof(LoadingPulse),
            new PropertyMetadata(12.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty DotSpacingProperty =
        DependencyProperty.Register(nameof(DotSpacing), typeof(double), typeof(LoadingPulse),
            new PropertyMetadata(8.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty PulseDurationProperty =
        DependencyProperty.Register(nameof(PulseDuration), typeof(double), typeof(LoadingPulse),
            new PropertyMetadata(0.6, OnAnimationPropertyChanged));

    public static readonly DependencyProperty DelayBetweenDotsProperty =
        DependencyProperty.Register(nameof(DelayBetweenDots), typeof(double), typeof(LoadingPulse),
            new PropertyMetadata(0.2, OnAnimationPropertyChanged));

    /// <summary>
    /// 圆点颜色
    /// </summary>
    public Brush DotColor
    {
        get { return (Brush)GetValue(DotColorProperty); }
        set { SetValue(DotColorProperty, value); }
    }

    /// <summary>
    /// 圆点大小
    /// </summary>
    public double DotSize
    {
        get { return (double)GetValue(DotSizeProperty); }
        set { SetValue(DotSizeProperty, value); }
    }

    /// <summary>
    /// 圆点间距
    /// </summary>
    public double DotSpacing
    {
        get { return (double)GetValue(DotSpacingProperty); }
        set { SetValue(DotSpacingProperty, value); }
    }

    /// <summary>
    /// 脉冲动画持续时间（秒）
    /// </summary>
    public double PulseDuration
    {
        get { return (double)GetValue(PulseDurationProperty); }
        set { SetValue(PulseDurationProperty, value); }
    }

    /// <summary>
    /// 圆点之间的延迟时间（秒）
    /// </summary>
    public double DelayBetweenDots
    {
        get { return (double)GetValue(DelayBetweenDotsProperty); }
        set { SetValue(DelayBetweenDotsProperty, value); }
    }

    #endregion 依赖属性

    private StackPanel _container;
    private Ellipse[] _dots;
    private Storyboard _storyboard;

    public LoadingPulse()
    {
        CreateVisualTree();

        Loaded += (s, e) =>
        {
            if (IsActive)
            {
                StartAnimation();
            }
        };
        Unloaded += (s, e) => StopAnimation();
    }

    private void CreateVisualTree()
    {
        // 创建水平排列的容器
        _container = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // 创建3个圆点
        _dots = new Ellipse[3];

        for (int i = 0; i < 3; i++)
        {
            var dot = new Ellipse
            {
                Width = DotSize,
                Height = DotSize,
                Fill = DotColor,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1.0, 1.0),
                Opacity = 0.3 // 初始透明度
            };

            // 设置间距（除了第一个圆点）
            if (i > 0)
            {
                dot.Margin = new Thickness(DotSpacing, 0, 0, 0);
            }

            _container.Children.Add(dot);
            _dots[i] = dot;
        }

        this.Content = _container;
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var pulse = (LoadingPulse)d;
        pulse.UpdateVisualProperties();
    }

    protected override void OnIsActiveChanged(bool oldValue, bool newValue)
    {
        base.OnIsActiveChanged(oldValue, newValue);
        if (IsLoaded)
        {
            UpdateAnimationState();
        }
    }

    private static void OnAnimationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var pulse = (LoadingPulse)d;
        if (pulse.IsLoaded && pulse.IsActive)
        {
            pulse.RecreateAnimation();
        }
    }

    private void UpdateVisualProperties()
    {
        if (_dots != null)
        {
            for (int i = 0; i < _dots.Length; i++)
            {
                var dot = _dots[i];
                dot.Width = DotSize;
                dot.Height = DotSize;
                dot.Fill = DotColor;

                // 更新间距（除了第一个圆点）
                if (i > 0)
                {
                    dot.Margin = new Thickness(DotSpacing, 0, 0, 0);
                }
            }
        }
    }

    private void UpdateAnimationState()
    {
        if (IsActive)
        {
            StartAnimation();
        }
        else
        {
            StopAnimation();
        }
    }

    private void RecreateAnimation()
    {
        StopAnimation();
        _storyboard = null;
        if (IsActive)
        {
            StartAnimation();
        }
    }

    private void StartAnimation()
    {
        if (_storyboard == null)
        {
            CreateAnimation();
        }
        _storyboard?.Begin();
    }

    private void StopAnimation()
    {
        _storyboard?.Stop();

        // 重置所有圆点状态
        if (_dots != null)
        {
            foreach (var dot in _dots)
            {
                dot.Opacity = 0.3;
                if (dot.RenderTransform is ScaleTransform scale)
                {
                    scale.ScaleX = 1.0;
                    scale.ScaleY = 1.0;
                }
            }
        }
    }

    private void CreateAnimation()
    {
        _storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        // 计算总循环时间
        var totalCycleTime = PulseDuration + (DelayBetweenDots * 2) + 0.3; // 额外0.3秒的间隔

        // 为每个圆点创建脉冲动画
        for (int i = 0; i < 3; i++)
        {
            var dot = _dots[i];
            var beginDelay = TimeSpan.FromSeconds(i * DelayBetweenDots);

            // 创建透明度动画
            var opacityAnimation = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = beginDelay,
                Duration = new Duration(TimeSpan.FromSeconds(totalCycleTime))
            };

            // 添加关键帧
            opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.Zero))); // 初始状态
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(PulseDuration * 0.5)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            }); // 脉冲峰值
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(PulseDuration)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            }); // 回到初始状态
            opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(totalCycleTime)))); // 保持初始状态直到循环结束

            Storyboard.SetTarget(opacityAnimation, dot);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Ellipse.OpacityProperty));
            _storyboard.Children.Add(opacityAnimation);

            // 创建缩放动画
            var scaleAnimationX = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = beginDelay,
                Duration = new Duration(TimeSpan.FromSeconds(totalCycleTime))
            };

            var scaleAnimationY = new DoubleAnimationUsingKeyFrames
            {
                BeginTime = beginDelay,
                Duration = new Duration(TimeSpan.FromSeconds(totalCycleTime))
            };

            // 添加缩放关键帧
            scaleAnimationX.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            scaleAnimationX.KeyFrames.Add(new EasingDoubleKeyFrame(1.3, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(PulseDuration * 0.5)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });
            scaleAnimationX.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(PulseDuration)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            });
            scaleAnimationX.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(totalCycleTime))));

            scaleAnimationY.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            scaleAnimationY.KeyFrames.Add(new EasingDoubleKeyFrame(1.3, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(PulseDuration * 0.5)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });
            scaleAnimationY.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(PulseDuration)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            });
            scaleAnimationY.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(totalCycleTime))));

            Storyboard.SetTarget(scaleAnimationX, dot);
            Storyboard.SetTargetProperty(scaleAnimationX, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            _storyboard.Children.Add(scaleAnimationX);

            Storyboard.SetTarget(scaleAnimationY, dot);
            Storyboard.SetTargetProperty(scaleAnimationY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            _storyboard.Children.Add(scaleAnimationY);
        }
    }
}