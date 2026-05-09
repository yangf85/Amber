using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 旋转圆环加载动画控件 - 显示旋转的圆环
/// </summary>
public class LoadingRing : LoadingIndicator
{
    #region 依赖属性

    public static readonly DependencyProperty RingColorProperty =
        DependencyProperty.Register(nameof(RingColor), typeof(Brush), typeof(LoadingRing),
            new PropertyMetadata(new SolidColorBrush(Colors.White), OnVisualPropertyChanged));

    public static readonly DependencyProperty RingSizeProperty =
        DependencyProperty.Register(nameof(RingSize), typeof(double), typeof(LoadingRing),
            new PropertyMetadata(50.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty RingThicknessProperty =
        DependencyProperty.Register(nameof(RingThickness), typeof(double), typeof(LoadingRing),
            new PropertyMetadata(4.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty RotationSpeedProperty =
        DependencyProperty.Register(nameof(RotationSpeed), typeof(double), typeof(LoadingRing),
            new PropertyMetadata(1.5, OnAnimationPropertyChanged));

    public static readonly DependencyProperty ArcLengthProperty =
        DependencyProperty.Register(nameof(ArcLength), typeof(double), typeof(LoadingRing),
            new PropertyMetadata(270.0, OnVisualPropertyChanged));

    /// <summary>
    /// 圆环颜色
    /// </summary>
    public Brush RingColor
    {
        get { return (Brush)GetValue(RingColorProperty); }
        set { SetValue(RingColorProperty, value); }
    }

    /// <summary>
    /// 圆环大小
    /// </summary>
    public double RingSize
    {
        get { return (double)GetValue(RingSizeProperty); }
        set { SetValue(RingSizeProperty, value); }
    }

    /// <summary>
    /// 圆环厚度
    /// </summary>
    public double RingThickness
    {
        get { return (double)GetValue(RingThicknessProperty); }
        set { SetValue(RingThicknessProperty, value); }
    }

    /// <summary>
    /// 旋转速度（秒/圈）
    /// </summary>
    public double RotationSpeed
    {
        get { return (double)GetValue(RotationSpeedProperty); }
        set { SetValue(RotationSpeedProperty, value); }
    }

    /// <summary>
    /// 圆弧长度（角度）
    /// </summary>
    public double ArcLength
    {
        get { return (double)GetValue(ArcLengthProperty); }
        set { SetValue(ArcLengthProperty, value); }
    }

    #endregion 依赖属性

    private Grid _container;
    private Path _ringPath;
    private Storyboard _storyboard;

    public LoadingRing()
    {
        CreateVisualTree();

        // 进入 visual tree 时按当前 IsActive 状态启动动画
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
        _container = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = RingSize,
            Height = RingSize,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new RotateTransform()
        };

        // 创建圆环路径
        _ringPath = new Path
        {
            Stroke = RingColor,
            StrokeThickness = RingThickness,
            StrokeEndLineCap = PenLineCap.Round,

            StrokeStartLineCap = PenLineCap.Round,
            Fill = Brushes.Transparent
        };

        UpdateRingGeometry();

        _container.Children.Add(_ringPath);
        this.Content = _container;
    }

    private void UpdateRingGeometry()
    {
        if (_ringPath == null) return;

        var radius = (RingSize - RingThickness) / 2;
        var center = new Point(RingSize / 2, RingSize / 2);

        // 计算圆弧的起始和结束角度
        var startAngle = 0;
        var endAngle = ArcLength * Math.PI / 180; // 转换为弧度

        // 计算起始和结束点
        var startPoint = new Point(
            center.X + radius * Math.Cos(startAngle),
            center.Y + radius * Math.Sin(startAngle)
        );

        var endPoint = new Point(
            center.X + radius * Math.Cos(endAngle),
            center.Y + radius * Math.Sin(endAngle)
        );

        // 创建圆弧几何
        var geometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = startPoint };

        var arcSegment = new ArcSegment
        {
            Point = endPoint,
            Size = new Size(radius, radius),
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc = ArcLength > 180
        };

        figure.Segments.Add(arcSegment);
        geometry.Figures.Add(figure);

        _ringPath.Data = geometry;
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ring = (LoadingRing)d;
        ring.UpdateVisualProperties();
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
        var ring = (LoadingRing)d;
        if (ring.IsLoaded && ring.IsActive)
        {
            ring.RecreateAnimation();
        }
    }

    private void UpdateVisualProperties()
    {
        if (_container != null)
        {
            _container.Width = RingSize;
            _container.Height = RingSize;
        }

        if (_ringPath != null)
        {
            _ringPath.Stroke = RingColor;
            _ringPath.StrokeThickness = RingThickness;
            UpdateRingGeometry();
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

        // 重置旋转角度
        if (_container?.RenderTransform is RotateTransform rotate)
        {
            rotate.Angle = 0;
        }
    }

    private void CreateAnimation()
    {
        _storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        // 创建旋转动画
        var rotationAnimation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = new Duration(TimeSpan.FromSeconds(RotationSpeed)),
            RepeatBehavior = RepeatBehavior.Forever
        };

        Storyboard.SetTarget(rotationAnimation, _container);
        Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

        _storyboard.Children.Add(rotationAnimation);
    }
}