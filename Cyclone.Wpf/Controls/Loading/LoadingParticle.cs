using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 加载动画控件 - 显示5个旋转的粒子
/// </summary>
public class LoadingParticle : LoadingIndicator
{
    #region 依赖属性

    public static readonly DependencyProperty ParticleColorProperty =
        DependencyProperty.Register(nameof(ParticleColor), typeof(Brush), typeof(LoadingParticle),
            new PropertyMetadata(new SolidColorBrush(Colors.White), OnVisualPropertyChanged));

    public static readonly DependencyProperty ParticleRadiusProperty =
        DependencyProperty.Register(nameof(ParticleRadius), typeof(double), typeof(LoadingParticle),
            new PropertyMetadata(5.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty OrbitRadiusProperty =
        DependencyProperty.Register(nameof(OrbitRadius), typeof(double), typeof(LoadingParticle),
            new PropertyMetadata(20.0, OnVisualPropertyChanged));

    /// <summary>
    /// 粒子颜色
    /// </summary>
    public Brush ParticleColor
    {
        get { return (Brush)GetValue(ParticleColorProperty); }
        set { SetValue(ParticleColorProperty, value); }
    }

    /// <summary>
    /// 粒子半径
    /// </summary>
    public double ParticleRadius
    {
        get { return (double)GetValue(ParticleRadiusProperty); }
        set { SetValue(ParticleRadiusProperty, value); }
    }

    /// <summary>
    /// 轨道半径
    /// </summary>
    public double OrbitRadius
    {
        get { return (double)GetValue(OrbitRadiusProperty); }
        set { SetValue(OrbitRadiusProperty, value); }
    }

    #endregion 依赖属性

    private Canvas _canvas;

    private Ellipse[] _particles;

    private Border[] _particleBorders;

    private Storyboard _storyboard;

    protected override void OnIsActiveChanged(bool oldValue, bool newValue)
    {
        base.OnIsActiveChanged(oldValue, newValue);

        if (IsLoaded)
        {
            UpdateAnimationState();
        }
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var spinner = (LoadingParticle)d;
        spinner.UpdateVisualProperties();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (IsActive)
        {
            StartAnimation();
        }
    }

    private void CreateVisualTree()
    {
        // 创建Canvas - 使用固定大小，让外部控件的Width/Height通过Stretch来控制显示大小
        _canvas = new Canvas
        {
            Width = 100,
            Height = 100,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // 创建5个粒子
        _particles = new Ellipse[5];
        _particleBorders = new Border[5];

        double[] particleOriginAngles = { 0, -10, -20, -30, -40 };

        for (int i = 0; i < 5; i++)
        {
            // 创建Border容器 - 设置尺寸确保可见
            var border = new Border
            {
                Width = 100,
                Height = 100,
                Background = Brushes.Transparent,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform()
            };

            // 创建粒子(Ellipse)
            var particle = new Ellipse
            {
                Width = ParticleRadius * 2,
                Height = ParticleRadius * 2,
                Fill = ParticleColor,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            // 设置粒子的初始位置和旋转 - 保持原来的Transform结构
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(0, -OrbitRadius));
            transformGroup.Children.Add(new RotateTransform(particleOriginAngles[i]));
            particle.RenderTransform = transformGroup;

            border.Child = particle;

            // 将border放置在canvas中心
            Canvas.SetLeft(border, 0);
            Canvas.SetTop(border, 0);

            _canvas.Children.Add(border);

            _particles[i] = particle;
            _particleBorders[i] = border;
        }

        this.Content = _canvas;
    }

    private void UpdateVisualProperties()
    {
        if (_particles != null)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                var particle = _particles[i];
                particle.Width = ParticleRadius * 2;
                particle.Height = ParticleRadius * 2;
                particle.Fill = ParticleColor;

                // 更新轨道位置
                var transformGroup = particle.RenderTransform as TransformGroup;
                if (transformGroup?.Children.Count >= 2)
                {
                    var translateTransform = transformGroup.Children[0] as TranslateTransform;
                    if (translateTransform != null)
                    {
                        translateTransform.X = 0;
                        translateTransform.Y = -OrbitRadius;
                    }
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

    private void StartAnimation()
    {
        StopAnimation(); // 先停止现有动画

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

        // 动画参数
        var particleBeginDelays = new[] { 0.0, 0.1, 0.2, 0.3, 0.4 };

        // 动画阶段参数
        var phases = new[]
        {
            new { BeginAngle = 0.0, EndAngle = 90.0, BeginTime = TimeSpan.Zero, Duration = new Duration(TimeSpan.FromSeconds(0.75)) },
            new { BeginAngle = 90.0, EndAngle = 270.0, BeginTime = TimeSpan.FromSeconds(0.751), Duration = new Duration(TimeSpan.FromSeconds(0.3)) },
            new { BeginAngle = 270.0, EndAngle = 360.0, BeginTime = TimeSpan.FromSeconds(1.052), Duration = new Duration(TimeSpan.FromSeconds(0.75)) }
        };

        // 为每个粒子创建动画
        for (int i = 0; i < 5; i++)
        {
            var border = _particleBorders[i];
            var beginDelay = TimeSpan.FromSeconds(particleBeginDelays[i]);

            // 创建包含延迟的子Storyboard
            var particleStoryboard = new Storyboard
            {
                BeginTime = beginDelay,
                Duration = new Duration(TimeSpan.FromSeconds(1.8))
            };

            // 为每个阶段创建动画
            foreach (var phase in phases)
            {
                var animation = new DoubleAnimation
                {
                    From = phase.BeginAngle,
                    To = phase.EndAngle,
                    BeginTime = phase.BeginTime,
                    Duration = phase.Duration
                };

                Storyboard.SetTarget(animation, border);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
                particleStoryboard.Children.Add(animation);
            }

            _storyboard.Children.Add(particleStoryboard);
        }
    }

    public LoadingParticle()
    {
        CreateVisualTree();

        Loaded += OnLoaded;
        Unloaded += (s, e) => StopAnimation();
    }
}