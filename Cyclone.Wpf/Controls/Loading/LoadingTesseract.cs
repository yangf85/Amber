using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 四维立方体（超立方体）加载动画控件 - 显示四维空间投影效果
/// </summary>
public class LoadingTesseract : LoadingIndicator
{
    #region 依赖属性

    public static readonly DependencyProperty LineColorProperty =
        DependencyProperty.Register(nameof(LineColor), typeof(Color), typeof(LoadingTesseract),
            new PropertyMetadata(Colors.Black, OnVisualPropertyChanged));

    public static readonly DependencyProperty AnimationSpeedProperty =
        DependencyProperty.Register(nameof(AnimationSpeed), typeof(double), typeof(LoadingTesseract),
            new PropertyMetadata(0.3));

    public static readonly DependencyProperty ScaleProperty =
        DependencyProperty.Register(nameof(Scale), typeof(double), typeof(LoadingTesseract),
            new PropertyMetadata(1.0, OnScaleChanged));

    /// <summary>
    /// 线条颜色
    /// </summary>
    public Color LineColor
    {
        get { return (Color)GetValue(LineColorProperty); }
        set { SetValue(LineColorProperty, value); }
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
    /// 缩放大小
    /// </summary>
    public double Scale
    {
        get { return (double)GetValue(ScaleProperty); }
        set { SetValue(ScaleProperty, Math.Max(0.1, value)); }
    }

    #endregion 依赖属性

    private Viewport3D _viewport;
    private ModelVisual3D _tesseractModel;
    private Model3DGroup _lineGroup;
    private DispatcherTimer _animationTimer;
    private double _time;
    private DateTime _lastUpdateTime;

    // 四维立方体的16个顶点坐标（4D）
    private readonly Point4D[] _tesseractVertices = new Point4D[16];

    public LoadingTesseract()
    {
        Width = 200;
        Height = 200;
        InitializeTesseractVertices();
        CreateVisualTree();

        _animationTimer = new DispatcherTimer();
        _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
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

    private void InitializeTesseractVertices()
    {
        // 四维立方体的16个顶点 (±1, ±1, ±1, ±1)
        int index = 0;
        for (int w = -1; w <= 1; w += 2)
        {
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        _tesseractVertices[index] = new Point4D(x, y, z, w);
                        index++;
                    }
                }
            }
        }
    }

    private void CreateVisualTree()
    {
        _viewport = new Viewport3D();

        // 设置相机
        var camera = new PerspectiveCamera
        {
            Position = new Point3D(0, 0, 8),
            LookDirection = new Vector3D(0, 0, -1),
            UpDirection = new Vector3D(0, 1, 0),
            FieldOfView = 45
        };
        _viewport.Camera = camera;

        // 添加光源
        AddLights();

        // 创建超立方体模型
        CreateTesseractModel();

        Content = _viewport;
    }

    private void AddLights()
    {
        var lights = new ModelVisual3D();
        var lightGroup = new Model3DGroup();

        lightGroup.Children.Add(new AmbientLight(Color.FromRgb(80, 80, 80)));
        lightGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)));

        lights.Content = lightGroup;
        _viewport.Children.Add(lights);
    }

    private void CreateTesseractModel()
    {
        if (_tesseractModel != null && _viewport.Children.Contains(_tesseractModel))
        {
            _viewport.Children.Remove(_tesseractModel);
        }

        _tesseractModel = new ModelVisual3D();
        _lineGroup = new Model3DGroup();

        UpdateTesseractProjection();

        _tesseractModel.Content = _lineGroup;
        _viewport.Children.Add(_tesseractModel);
    }

    private void UpdateTesseractProjection()
    {
        _lineGroup.Children.Clear();

        // 投影四维顶点到三维空间
        Point3D[] projectedVertices = new Point3D[16];
        for (int i = 0; i < 16; i++)
        {
            projectedVertices[i] = ProjectTo3D(_tesseractVertices[i], _time);
        }

        // 创建超立方体的边
        CreateTesseractEdges(projectedVertices);
    }

    private Point3D ProjectTo3D(Point4D point4D, double time)
    {
        // 应用四维旋转变换
        var rotated = Rotate4D(point4D, time);

        // 透视投影：将4D点投影到3D空间
        double distance = 3.0; // 观察距离
        double scale = distance / (distance + rotated.W);

        return new Point3D(
            rotated.X * scale * Scale,
            rotated.Y * scale * Scale,
            rotated.Z * scale * Scale
        );
    }

    private Point4D Rotate4D(Point4D point, double time)
    {
        // 在XY, XZ, XW, YZ, YW, ZW平面上应用旋转
        double angleXY = time * 0.5;
        double angleXZ = time * 0.3;
        double angleXW = time * 0.7;
        double angleYZ = time * 0.4;
        double angleYW = time * 0.6;
        double angleZW = time * 0.8;

        var result = point;

        // XY平面旋转
        var newX = result.X * Math.Cos(angleXY) - result.Y * Math.Sin(angleXY);
        var newY = result.X * Math.Sin(angleXY) + result.Y * Math.Cos(angleXY);
        result.X = newX;
        result.Y = newY;

        // XZ平面旋转
        newX = result.X * Math.Cos(angleXZ) - result.Z * Math.Sin(angleXZ);
        var newZ = result.X * Math.Sin(angleXZ) + result.Z * Math.Cos(angleXZ);
        result.X = newX;
        result.Z = newZ;

        // XW平面旋转
        newX = result.X * Math.Cos(angleXW) - result.W * Math.Sin(angleXW);
        var newW = result.X * Math.Sin(angleXW) + result.W * Math.Cos(angleXW);
        result.X = newX;
        result.W = newW;

        // YZ平面旋转
        newY = result.Y * Math.Cos(angleYZ) - result.Z * Math.Sin(angleYZ);
        newZ = result.Y * Math.Sin(angleYZ) + result.Z * Math.Cos(angleYZ);
        result.Y = newY;
        result.Z = newZ;

        // YW平面旋转
        newY = result.Y * Math.Cos(angleYW) - result.W * Math.Sin(angleYW);
        newW = result.Y * Math.Sin(angleYW) + result.W * Math.Cos(angleYW);
        result.Y = newY;
        result.W = newW;

        // ZW平面旋转
        newZ = result.Z * Math.Cos(angleZW) - result.W * Math.Sin(angleZW);
        newW = result.Z * Math.Sin(angleZW) + result.W * Math.Cos(angleZW);
        result.Z = newZ;
        result.W = newW;

        return result;
    }

    private void CreateTesseractEdges(Point3D[] vertices)
    {
        // 超立方体的连接规则：相邻顶点（汉明距离为1）相连
        for (int i = 0; i < 16; i++)
        {
            for (int j = i + 1; j < 16; j++)
            {
                if (GetHammingDistance(i, j) == 1)
                {
                    CreateLine(vertices[i], vertices[j]);
                }
            }
        }
    }

    private int GetHammingDistance(int i, int j)
    {
        // 计算两个顶点索引的汉明距离（二进制表示中不同位的数量）
        int xor = i ^ j;
        int count = 0;
        while (xor != 0)
        {
            count += xor & 1;
            xor >>= 1;
        }
        return count;
    }

    private void CreateLine(Point3D start, Point3D end)
    {
        var mesh = new MeshGeometry3D();
        double thickness = 0.015;

        // 创建圆柱体作为线条
        Vector3D direction = end - start;
        double length = direction.Length;
        direction.Normalize();

        // 创建圆柱体的顶点
        Vector3D perpendicular1 = GetPerpendicularVector(direction);
        Vector3D perpendicular2 = Vector3D.CrossProduct(direction, perpendicular1);

        int segments = 6;
        for (int i = 0; i <= segments; i++)
        {
            double angle = 2 * Math.PI * i / segments;
            Vector3D offset = thickness * (Math.Cos(angle) * perpendicular1 + Math.Sin(angle) * perpendicular2);

            mesh.Positions.Add(start + offset);
            mesh.Positions.Add(end + offset);
        }

        // 创建三角形索引
        for (int i = 0; i < segments; i++)
        {
            int current = i * 2;
            int next = ((i + 1) % (segments + 1)) * 2;

            // 侧面的两个三角形
            mesh.TriangleIndices.Add(current);
            mesh.TriangleIndices.Add(next);
            mesh.TriangleIndices.Add(current + 1);

            mesh.TriangleIndices.Add(next);
            mesh.TriangleIndices.Add(next + 1);
            mesh.TriangleIndices.Add(current + 1);
        }

        var material = new DiffuseMaterial(new SolidColorBrush(LineColor));
        var geometry = new GeometryModel3D(mesh, material);
        _lineGroup.Children.Add(geometry);
    }

    private Vector3D GetPerpendicularVector(Vector3D vector)
    {
        if (Math.Abs(vector.X) < 0.9)
            return Vector3D.CrossProduct(vector, new Vector3D(1, 0, 0));
        else
            return Vector3D.CrossProduct(vector, new Vector3D(0, 1, 0));
    }

    private void OnAnimationTick(object sender, EventArgs e)
    {
        var now = DateTime.Now;
        var deltaTime = _lastUpdateTime == default ? 0 : (now - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = now;

        if (deltaTime > 0 && deltaTime < 0.1)
        {
            _time += deltaTime * AnimationSpeed;
            UpdateTesseractProjection();
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
        _time = 0;
        UpdateTesseractProjection();
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (LoadingTesseract)d;
        control.UpdateTesseractProjection();
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

    private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (LoadingTesseract)d;
        control.UpdateTesseractProjection();
    }

    /// <summary>
    /// 四维点结构
    /// </summary>
    private struct Point4D
    {
        public double X, Y, Z, W;

        public Point4D(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}