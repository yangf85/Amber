using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 3D方块翻转加载动画控件 - 显示3D翻转效果的方块
/// </summary>
public class LoadingFlipCube : LoadingIndicator
{
    #region 依赖属性

    public static readonly DependencyProperty CubeColorProperty =
        DependencyProperty.Register(nameof(CubeColor), typeof(Color), typeof(LoadingFlipCube),
            new PropertyMetadata(Colors.DodgerBlue, OnVisualPropertyChanged));

    public static readonly DependencyProperty AnimationSpeedProperty =
        DependencyProperty.Register(nameof(AnimationSpeed), typeof(double), typeof(LoadingFlipCube),
            new PropertyMetadata(0.5)); // 默认速度降低到0.5

    public static readonly DependencyProperty CubeSizeProperty =
        DependencyProperty.Register(nameof(CubeSize), typeof(double), typeof(LoadingFlipCube),
            new PropertyMetadata(1.5, OnCubeSizeChanged));

    /// <summary>
    /// 方块颜色
    /// </summary>
    public Color CubeColor
    {
        get { return (Color)GetValue(CubeColorProperty); }
        set { SetValue(CubeColorProperty, value); }
    }

    /// <summary>
    /// 动画速度倍数
    /// </summary>
    public double AnimationSpeed
    {
        get { return (double)GetValue(AnimationSpeedProperty); }
        set { SetValue(AnimationSpeedProperty, value); }
    }

    /// <summary>
    /// 立方体大小
    /// </summary>
    public double CubeSize
    {
        get { return (double)GetValue(CubeSizeProperty); }
        set { SetValue(CubeSizeProperty, Math.Max(0.1, value)); } // 最小值0.1，防止过小
    }

    #endregion 依赖属性

    private Viewport3D _viewport;
    private ModelVisual3D _cubeModel;
    private AxisAngleRotation3D _rotation;
    private DispatcherTimer _animationTimer;
    private double _currentAngle;
    private DateTime _lastUpdateTime;

    public LoadingFlipCube()
    {
        Width = 100;
        Height = 100;
        CreateVisualTree();

        // 初始化动画计时器
        _animationTimer = new DispatcherTimer();
        _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // 约60 FPS
        _animationTimer.Tick += OnAnimationTick;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (IsActive)
        {
            StartAnimation();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopAnimation();
    }

    private void CreateVisualTree()
    {
        // 创建Viewport3D
        _viewport = new Viewport3D();

        // 设置相机
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 5),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 60
        };
        _viewport.Camera = camera;

        // 添加光源
        AddLights();

        // 创建单个方块
        CreateCube();

        this.Content = _viewport;
    }

    private void AddLights()
    {
        var lights = new ModelVisual3D();
        var lightGroup = new Model3DGroup();

        // 环境光
        lightGroup.Children.Add(new AmbientLight(Color.FromRgb(100, 100, 100)));

        // 方向光
        lightGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -3)));
        lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(200, 200, 200), new Vector3D(1, 1, 3)));

        lights.Content = lightGroup;
        _viewport.Children.Add(lights);
    }

    private void CreateCube()
    {
        // 清除旧的方块
        if (_cubeModel != null && _viewport.Children.Contains(_cubeModel))
        {
            _viewport.Children.Remove(_cubeModel);
        }

        _cubeModel = CreateSingleCube();
        _viewport.Children.Add(_cubeModel);
        _currentAngle = 0;
    }

    private ModelVisual3D CreateSingleCube()
    {
        var cubeModel = new ModelVisual3D();

        // 创建立方体网格
        var mesh = CreateCubeMesh();

        // 创建材质
        var material = new MaterialGroup();
        var diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(CubeColor));
        var specularMaterial = new SpecularMaterial(new SolidColorBrush(Colors.White), 30);
        material.Children.Add(diffuseMaterial);
        material.Children.Add(specularMaterial);

        // 创建几何模型
        var geometry = new GeometryModel3D(mesh, material);

        // 创建变换组
        var transformGroup = new Transform3DGroup();

        // 旋转变换 - 围绕对角轴旋转
        _rotation = new AxisAngleRotation3D(new Vector3D(1, 1, 0), 0);
        var rotateTransform = new RotateTransform3D(_rotation);
        transformGroup.Children.Add(rotateTransform);

        cubeModel.Transform = transformGroup;
        cubeModel.Content = geometry;

        return cubeModel;
    }

    private MeshGeometry3D CreateCubeMesh()
    {
        var mesh = new MeshGeometry3D();
        double size = CubeSize; // 使用CubeSize属性

        // 定义8个顶点
        var p0 = new Point3D(-size, -size, -size);
        var p1 = new Point3D(size, -size, -size);
        var p2 = new Point3D(size, size, -size);
        var p3 = new Point3D(-size, size, -size);
        var p4 = new Point3D(-size, -size, size);
        var p5 = new Point3D(size, -size, size);
        var p6 = new Point3D(size, size, size);
        var p7 = new Point3D(-size, size, size);

        // 添加顶点
        mesh.Positions.Add(p0); mesh.Positions.Add(p1); mesh.Positions.Add(p2); mesh.Positions.Add(p3);
        mesh.Positions.Add(p4); mesh.Positions.Add(p5); mesh.Positions.Add(p6); mesh.Positions.Add(p7);

        // 前面 (Z+)
        mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(7);
        mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(7);

        // 后面 (Z-)
        mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(2);
        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(3); mesh.TriangleIndices.Add(2);

        // 左面 (X-)
        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(3);
        mesh.TriangleIndices.Add(4); mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(3);

        // 右面 (X+)
        mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(6);
        mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(6);

        // 上面 (Y+)
        mesh.TriangleIndices.Add(7); mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(3);
        mesh.TriangleIndices.Add(6); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(3);

        // 下面 (Y-)
        mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(4);
        mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(5); mesh.TriangleIndices.Add(4);

        return mesh;
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        var now = DateTime.Now;
        var deltaTime = _lastUpdateTime == default ? 0 : (now - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = now;

        if (deltaTime > 0 && deltaTime < 0.1) // 防止跳跃
        {
            double baseSpeed = 90 * AnimationSpeed; // 每秒90度（原来是180度，现在更慢了）

            if (_rotation != null)
            {
                _currentAngle += baseSpeed * deltaTime;
                if (_currentAngle > 360)
                    _currentAngle -= 360;

                _rotation.Angle = _currentAngle;
            }
        }
    }

    private void StartAnimation()
    {
        _lastUpdateTime = DateTime.Now;
        _animationTimer.Start();
    }

    private void StopAnimation()
    {
        _animationTimer.Stop();

        // 重置角度
        if (_rotation != null)
        {
            _rotation.Angle = 0;
            _currentAngle = 0;
        }
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var loader = (LoadingFlipCube)d;
        loader.UpdateVisualProperties();
    }

    protected override void OnIsActiveChanged(bool oldValue, bool newValue)
    {
        base.OnIsActiveChanged(oldValue, newValue);
        if (IsLoaded)
        {
            if (IsActive)
                StartAnimation();
            else
                StopAnimation();
        }
    }

    private static void OnCubeSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var loader = (LoadingFlipCube)d;
        bool wasActive = loader._animationTimer.IsEnabled;

        if (wasActive)
            loader.StopAnimation();

        loader.CreateCube();

        if (wasActive && loader.IsActive && loader.IsLoaded)
            loader.StartAnimation();
    }

    private void UpdateVisualProperties()
    {
        if (_cubeModel?.Content is GeometryModel3D geometry &&
            geometry.Material is MaterialGroup materialGroup &&
            materialGroup.Children.Count > 0 &&
            materialGroup.Children[0] is DiffuseMaterial diffuseMaterial)
        {
            diffuseMaterial.Brush = new SolidColorBrush(CubeColor);
        }
    }
}