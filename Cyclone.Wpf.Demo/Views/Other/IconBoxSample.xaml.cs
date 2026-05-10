using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// IconBoxView.xaml 的交互逻辑
/// </summary>
public partial class IconBoxSample : UserControl
{
    public IconBoxSample()
    {
        InitializeComponent();
    }
}

public partial class IconBoxViewModel : ObservableObject
{
    [ObservableProperty]
    private string currentFontIcon = "\xe71a";

    [ObservableProperty]
    private Geometry currentGeometry;

    [ObservableProperty]
    private ImageSource currentImage;

    [ObservableProperty]
    private string currentImageName = "百度";

    // 字体图标集合
    public ObservableCollection<FontIconItem> FontIcons { get; } = new();

    // 路径图标集合
    public ObservableCollection<PathIconItem> PathIcons { get; } = new();

    // 网络图片集合
    public ObservableCollection<NetworkImageItem> NetworkImages { get; } = new();

    // 工具栏项集合
    public ObservableCollection<ToolbarItem> ToolbarItems { get; } = new();

    // 菜单项集合
    public ObservableCollection<MenuItem> MenuItems { get; } = new();

    private void InitializeFontIcons()
    {
        FontIcons.Add(new FontIconItem("按钮", "\xe74c", Brushes.Blue));
        FontIcons.Add(new FontIconItem("输入框", "\xe60b", Brushes.Green));
        FontIcons.Add(new FontIconItem("菜单", "\xe888", Brushes.Red));
    }

    private void InitializePathIcons()
    {
        var heartGeometry = Geometry.Parse("M12,21.35L10.55,20.03C5.4,15.36 2,12.27 2,8.5 2,5.41 4.42,3 7.5,3C9.24,3 10.91,3.81 12,5.08C13.09,3.81 14.76,3 16.5,3C19.58,3 22,5.41 22,8.5C22,12.27 18.6,15.36 13.45,20.03L12,21.35Z");
        var settingsGeometry = Geometry.Parse("M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.22,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.22,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.68 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z");
        var homeGeometry = Geometry.Parse("M10,20V14H14V20H19V12H22L12,3L2,12H5V20H10Z");

        PathIcons.Add(new PathIconItem("喜欢", heartGeometry, Brushes.Red));
        PathIcons.Add(new PathIconItem("设置", settingsGeometry, Brushes.Gray));
        PathIcons.Add(new PathIconItem("主页", homeGeometry, Brushes.DarkBlue));
    }

    private void InitializeNetworkImages()
    {
        // 使用国内CDN或者常用的图片地址
        var baiduImage = new BitmapImage();
        baiduImage.BeginInit();
        baiduImage.UriSource = new Uri("https://www.baidu.com/favicon.ico");
        baiduImage.EndInit();

        var tencentImage = new BitmapImage();
        tencentImage.BeginInit();
        tencentImage.UriSource = new Uri("https://mat1.gtimg.com/pingjs/ext2020/qqindex2018/dist/img/qq_logo_2x.png");
        tencentImage.EndInit();

        var alibabaImage = new BitmapImage();
        alibabaImage.BeginInit();
        alibabaImage.UriSource = new Uri("https://img.alicdn.com/tfs/TB1_uT8a5ERMeJjSspiXXbZLFXa-143-59.png");
        alibabaImage.EndInit();

        NetworkImages.Add(new NetworkImageItem("百度", baiduImage));
        NetworkImages.Add(new NetworkImageItem("腾讯", tencentImage));
        NetworkImages.Add(new NetworkImageItem("阿里巴巴", alibabaImage));
    }

    private void InitializeToolbarItems()
    {
        ToolbarItems.Add(new ToolbarItem("\xe60b", "新建"));
        ToolbarItems.Add(new ToolbarItem("\xe888", "刷新"));
        ToolbarItems.Add(new ToolbarItem("\xe8fb", "搜索"));
    }

    private void InitializeMenuItems()
    {
        var homeGeometry = Geometry.Parse("M10,20V14H14V20H19V12H22L12,3L2,12H5V20H10Z");
        var settingsGeometry = Geometry.Parse("M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.22,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.22,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.68 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z");
        var listGeometry = Geometry.Parse("M3,5H9V11H3V5M5,7V9H7V7H5M11,7H21V9H11V7M11,15H21V17H11V15M5,20V18H7V20H5M3,17H9V23H3V17M11,19H21V21H11V19Z");

        MenuItems.Add(new MenuItem(homeGeometry, "首页", Brushes.DarkBlue));
        MenuItems.Add(new MenuItem(listGeometry, "数据管理", Brushes.DarkGreen));
        MenuItems.Add(new MenuItem(settingsGeometry, "系统设置", Brushes.Gray));
    }

    private void InitializeCurrentItems()
    {
        // 初始化当前显示的项
        CurrentGeometry = Geometry.Parse("M12,21.35L10.55,20.03C5.4,15.36 2,12.27 2,8.5 2,5.41 4.42,3 7.5,3C9.24,3 10.91,3.81 12,5.08C13.09,3.81 14.76,3 16.5,3C19.58,3 22,5.41 22,8.5C22,12.27 18.6,15.36 13.45,20.03L12,21.35Z");

        var currentImage = new BitmapImage();
        currentImage.BeginInit();
        currentImage.UriSource = new Uri("https://www.baidu.com/favicon.ico");
        currentImage.EndInit();
        CurrentImage = currentImage;
    }

    public IconBoxViewModel()
    {
        InitializeFontIcons();
        InitializePathIcons();
        InitializeNetworkImages();
        InitializeToolbarItems();
        InitializeMenuItems();
        InitializeCurrentItems();
    }
}

/// <summary>
/// 字体图标项
/// </summary>
public class FontIconItem
{
    public string Name { get; set; }

    public string Code { get; set; }

    public Brush Color { get; set; }

    public FontIconItem(string name, string code, Brush color)
    {
        Name = name;
        Code = code;
        Color = color;
    }
}

/// <summary>
/// 路径图标项
/// </summary>
public class PathIconItem
{
    public string Name { get; set; }

    public Geometry Geometry { get; set; }

    public Brush Color { get; set; }

    public PathIconItem(string name, Geometry geometry, Brush color)
    {
        Name = name;
        Geometry = geometry;
        Color = color;
    }
}

/// <summary>
/// 网络图片项
/// </summary>
public class NetworkImageItem
{
    public string Name { get; set; }

    public ImageSource Image { get; set; }

    public NetworkImageItem(string name, ImageSource image)
    {
        Name = name;
        Image = image;
    }
}

/// <summary>
/// 工具栏项
/// </summary>
public class ToolbarItem
{
    public string Icon { get; set; }

    public string Text { get; set; }

    public ToolbarItem(string icon, string text)
    {
        Icon = icon;
        Text = text;
    }
}

/// <summary>
/// 菜单项
/// </summary>
public class MenuItem
{
    public Geometry Geometry { get; set; }

    public string Text { get; set; }

    public Brush Color { get; set; }

    public MenuItem(Geometry geometry, string text, Brush color)
    {
        Geometry = geometry;
        Text = text;
        Color = color;
    }
}