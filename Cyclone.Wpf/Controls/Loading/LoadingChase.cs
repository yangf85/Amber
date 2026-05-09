using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 追逐圆点加载动画控件 - 显示沿圆形轨迹追逐的圆点
/// </summary>
public class LoadingChase : LoadingIndicator
{
    #region 依赖属性

    public static readonly DependencyProperty DotColorProperty =
        DependencyProperty.Register(nameof(DotColor), typeof(Brush), typeof(LoadingChase),
            new PropertyMetadata(new SolidColorBrush(Colors.White), OnVisualPropertyChanged));

    public static readonly DependencyProperty DotSizeProperty =
        DependencyProperty.Register(nameof(DotSize), typeof(double), typeof(LoadingChase),
            new PropertyMetadata(8.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty CircleSizeProperty =
        DependencyProperty.Register(nameof(CircleSize), typeof(double), typeof(LoadingChase),
            new PropertyMetadata(60.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty DotCountProperty =
        DependencyProperty.Register(nameof(DotCount), typeof(int), typeof(LoadingChase),
            new PropertyMetadata(8, OnStructuralPropertyChanged));

    public static readonly DependencyProperty ChaseSpeedProperty =
        DependencyProperty.Register(nameof(ChaseSpeed), typeof(double), typeof(LoadingChase),
            new PropertyMetadata(1.2, OnAnimationPropertyChanged));

    public static readonly DependencyProperty FadeEffectProperty =
        DependencyProperty.Register(nameof(FadeEffect), typeof(bool), typeof(LoadingChase),
            new PropertyMetadata(true, OnAnimationPropertyChanged));

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
    /// 圆形轨迹大小
    /// </summary>
    public double CircleSize
    {
        get { return (double)GetValue(CircleSizeProperty); }
        set { SetValue(CircleSizeProperty, value); }
    }

    /// <summary>
    /// 圆点数量
    /// </summary>
    public int DotCount
    {
        get { return (int)GetValue(DotCountProperty); }
        set { SetValue(DotCountProperty, value); }
    }

    /// <summary>
    /// 追逐速度（秒/圈）
    /// </summary>
    public double ChaseSpeed
    {
        get { return (double)GetValue(ChaseSpeedProperty); }
        set { SetValue(ChaseSpeedProperty, value); }
    }

    /// <summary>
    /// 是否启用渐隐效果
    /// </summary>
    public bool FadeEffect
    {
        get { return (bool)GetValue(FadeEffectProperty); }
        set { SetValue(FadeEffectProperty, value); }
    }

    #endregion 依赖属性

    private Canvas _container;
    private Ellipse[] _dots;
    private Storyboard _storyboard;

    public LoadingChase()
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
        // 创建容器
        _container = new Canvas
        {
            Width = CircleSize,
            Height = CircleSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        CreateDots();

        this.Content = _container;
    }

    private void CreateDots()
    {
        // 清除现有圆点
        _container.Children.Clear();

        // 创建圆点数组
        _dots = new Ellipse[DotCount];
        var radius = (CircleSize - DotSize) / 2;
        var centerX = CircleSize / 2;
        var centerY = CircleSize / 2;

        for (int i = 0; i < DotCount; i++)
        {
            var dot = new Ellipse
            {
                Width = DotSize,
                Height = DotSize,
                Fill = DotColor,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            // 计算初始位置
            var angle = (2 * Math.PI * i) / DotCount;
            var x = centerX + radius * Math.Cos(angle) - DotSize / 2;
            var y = centerY + radius * Math.Sin(angle) - DotSize / 2;

            Canvas.SetLeft(dot, x);
            Canvas.SetTop(dot, y);

            // 设置初始透明度（如果启用渐隐效果）
            if (FadeEffect)
            {
                dot.Opacity = (double)(i + 1) / DotCount;
            }

            _container.Children.Add(dot);
            _dots[i] = dot;
        }
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var chase = (LoadingChase)d;
        chase.UpdateVisualProperties();
    }

    private static void OnStructuralPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var chase = (LoadingChase)d;
        chase.RecreateVisualTree();
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
        var chase = (LoadingChase)d;
        if (chase.IsLoaded && chase.IsActive)
        {
            chase.RecreateAnimation();
        }
    }

    private void UpdateVisualProperties()
    {
        if (_container != null)
        {
            _container.Width = CircleSize;
            _container.Height = CircleSize;
        }

        if (_dots != null)
        {
            var radius = (CircleSize - DotSize) / 2;
            var centerX = CircleSize / 2;
            var centerY = CircleSize / 2;

            for (int i = 0; i < _dots.Length; i++)
            {
                var dot = _dots[i];
                dot.Width = DotSize;
                dot.Height = DotSize;
                dot.Fill = DotColor;

                // 重新计算位置
                var angle = (2 * Math.PI * i) / DotCount;
                var x = centerX + radius * Math.Cos(angle) - DotSize / 2;
                var y = centerY + radius * Math.Sin(angle) - DotSize / 2;

                Canvas.SetLeft(dot, x);
                Canvas.SetTop(dot, y);

                // 更新透明度
                if (FadeEffect)
                {
                    dot.Opacity = (double)(i + 1) / DotCount;
                }
                else
                {
                    dot.Opacity = 1.0;
                }
            }
        }
    }

    private void RecreateVisualTree()
    {
        StopAnimation();
        CreateDots();
        if (IsActive && IsLoaded)
        {
            StartAnimation();
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
    }

    private void CreateAnimation()
    {
        _storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        if (FadeEffect)
        {
            // 使用透明度动画创建追逐效果
            CreateFadeChaseAnimation();
        }
        else
        {
            // 使用位置动画创建追逐效果
            CreatePositionChaseAnimation();
        }
    }

    private void CreateFadeChaseAnimation()
    {
        var cycleDuration = TimeSpan.FromSeconds(ChaseSpeed);
        var stepDuration = cycleDuration.TotalSeconds / DotCount;

        for (int i = 0; i < DotCount; i++)
        {
            var dot = _dots[i];
            var opacityAnimation = new DoubleAnimationUsingKeyFrames
            {
                Duration = cycleDuration,
                RepeatBehavior = RepeatBehavior.Forever
            };

            // 为每个圆点创建透明度关键帧
            for (int j = 0; j < DotCount; j++)
            {
                var time = TimeSpan.FromSeconds(j * stepDuration);
                var opacity = j == i ? 1.0 : (double)(DotCount - Math.Abs(i - j)) / DotCount * 0.3;

                opacityAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(opacity, KeyTime.FromTimeSpan(time)));
            }

            Storyboard.SetTarget(opacityAnimation, dot);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Ellipse.OpacityProperty));
            _storyboard.Children.Add(opacityAnimation);
        }
    }

    private void CreatePositionChaseAnimation()
    {
        var radius = (CircleSize - DotSize) / 2;
        var centerX = CircleSize / 2;
        var centerY = CircleSize / 2;

        for (int i = 0; i < DotCount; i++)
        {
            var dot = _dots[i];
            var startAngle = (2 * Math.PI * i) / DotCount;

            // X轴动画
            var xAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(ChaseSpeed)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            var xKeyFrames = new DoubleAnimationUsingKeyFrames
            {
                Duration = new Duration(TimeSpan.FromSeconds(ChaseSpeed)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            // Y轴动画
            var yKeyFrames = new DoubleAnimationUsingKeyFrames
            {
                Duration = new Duration(TimeSpan.FromSeconds(ChaseSpeed)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            // 创建圆形路径关键帧
            var frameCount = 36; // 36个关键帧，每10度一个
            for (int frame = 0; frame <= frameCount; frame++)
            {
                var progress = (double)frame / frameCount;
                var currentAngle = startAngle + 2 * Math.PI * progress;

                var x = centerX + radius * Math.Cos(currentAngle) - DotSize / 2;
                var y = centerY + radius * Math.Sin(currentAngle) - DotSize / 2;

                var keyTime = KeyTime.FromPercent(progress);

                xKeyFrames.KeyFrames.Add(new LinearDoubleKeyFrame(x, keyTime));
                yKeyFrames.KeyFrames.Add(new LinearDoubleKeyFrame(y, keyTime));
            }

            Storyboard.SetTarget(xKeyFrames, dot);
            Storyboard.SetTargetProperty(xKeyFrames, new PropertyPath(Canvas.LeftProperty));
            _storyboard.Children.Add(xKeyFrames);

            Storyboard.SetTarget(yKeyFrames, dot);
            Storyboard.SetTargetProperty(yKeyFrames, new PropertyPath(Canvas.TopProperty));
            _storyboard.Children.Add(yKeyFrames);
        }
    }
}