using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views;

public partial class TilePanelSample : UserControl
{
    public TilePanelSample()
    {
        InitializeComponent();
        DataContext = new TilePanelViewModel();
    }
}

/// <summary>
/// TilePanel示例的ViewModel
/// </summary>
public partial class TilePanelViewModel : ObservableObject
{
    /// <summary>
    /// 固定网格磁贴数据集合
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<TileItem> FixedGridItems { get; set; }

    /// <summary>
    /// 自适应填充磁贴数据集合
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<TileItem> AutoFillItems { get; set; }

    /// <summary>
    /// 生成固定网格磁贴数据
    /// </summary>
    private ObservableCollection<TileItem> GenerateFixedGridItems()
    {
        var items = new ObservableCollection<TileItem>();
        var random = new Random();

        // 基础颜色调色板
        var colors = new List<Color>
        {
            Color.FromRgb(41, 128, 185),   // 蓝色
            Color.FromRgb(231, 76, 60),    // 红色
            Color.FromRgb(46, 204, 113),   // 绿色
            Color.FromRgb(155, 89, 182),   // 紫色
            Color.FromRgb(241, 196, 15),   // 黄色
            Color.FromRgb(230, 126, 34),   // 橙色
            Color.FromRgb(52, 152, 219),   // 亮蓝色
            Color.FromRgb(149, 165, 166)   // 灰色
        };

        // 生成24个磁贴
        for (int i = 1; i <= 24; i++)
        {
            var color = colors[random.Next(colors.Count)];
            items.Add(new TileItem
            {
                Content = $"磁贴{i}",
                Background = new SolidColorBrush(color),
                Icon = "■"
            });
        }

        return items;
    }

    /// <summary>
    /// 生成自适应填充磁贴数据（模拟Windows风格的应用磁贴）
    /// </summary>
    private ObservableCollection<TileItem> GenerateAutoFillItems()
    {
        var items = new ObservableCollection<TileItem>
        {
            new TileItem { Content = "邮件", Background = new SolidColorBrush(Color.FromRgb(0, 120, 215)), Icon = "✉" },
            new TileItem { Content = "日历", Background = new SolidColorBrush(Color.FromRgb(16, 137, 62)), Icon = "📅" },
            new TileItem { Content = "照片", Background = new SolidColorBrush(Color.FromRgb(247, 105, 0)), Icon = "🖼" },
            new TileItem { Content = "音乐", Background = new SolidColorBrush(Color.FromRgb(255, 67, 81)), Icon = "🎵" },
            new TileItem { Content = "设置", Background = new SolidColorBrush(Color.FromRgb(107, 105, 214)), Icon = "⚙" },
            new TileItem { Content = "商店", Background = new SolidColorBrush(Color.FromRgb(0, 178, 148)), Icon = "🛒" },
            new TileItem { Content = "天气", Background = new SolidColorBrush(Color.FromRgb(0, 120, 212)), Icon = "🌤" },
            new TileItem { Content = "新闻", Background = new SolidColorBrush(Color.FromRgb(255, 140, 0)), Icon = "📰" },
            new TileItem { Content = "游戏", Background = new SolidColorBrush(Color.FromRgb(118, 185, 0)), Icon = "🎮" },
            new TileItem { Content = "计算器", Background = new SolidColorBrush(Color.FromRgb(72, 72, 72)), Icon = "🔢" },
            new TileItem { Content = "地图", Background = new SolidColorBrush(Color.FromRgb(0, 99, 177)), Icon = "🗺" },
            new TileItem { Content = "相机", Background = new SolidColorBrush(Color.FromRgb(68, 68, 68)), Icon = "📷" },
            new TileItem { Content = "文档", Background = new SolidColorBrush(Color.FromRgb(43, 87, 151)), Icon = "📄" },
            new TileItem { Content = "备忘录", Background = new SolidColorBrush(Color.FromRgb(255, 185, 0)), Icon = "📝" },
            new TileItem { Content = "视频", Background = new SolidColorBrush(Color.FromRgb(232, 17, 35)), Icon = "🎬" }
        };

        return items;
    }

    public TilePanelViewModel()
    {
        // 生成固定网格的磁贴数据（展示不同大小的磁贴）
        FixedGridItems = GenerateFixedGridItems();

        // 生成自适应填充的磁贴数据（模拟应用程序磁贴）
        AutoFillItems = GenerateAutoFillItems();
    }
}

/// <summary>
/// 磁贴项数据模型
/// </summary>
public class TileItem
{
    /// <summary>
    /// 背景颜色
    /// </summary>
    public Brush Background { get; set; }

    /// <summary>
    /// 显示内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }
}