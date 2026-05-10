using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class TreeViewSample : UserControl
{
    public TreeViewSample()
    {
        InitializeComponent();
        DataContext = new TreeViewViewModel();
    }
}

public partial class TreeViewViewModel : ObservableObject
{
    public ObservableCollection<FileNode> FileTree { get; }

    public ObservableCollection<DepartmentNode> OrgTree { get; }

    public ObservableCollection<FileNode> FocusedTree { get; }

    public ObservableCollection<FileNode> UnfocusedTree { get; }

    private static void SetExpandedRecursive(TreeNodeBase node, bool expanded)
    {
        node.IsExpanded = expanded;
        foreach (var child in node.Children)
        {
            SetExpandedRecursive(child, expanded);
        }
    }

    private static ObservableCollection<FileNode> BuildFileTree()
    {
        return new ObservableCollection<FileNode>
        {
            new FileNode
            {
                Name = "Cyclone.Wpf",
                IsFolder = true,
                IsExpanded = true,
                Size = 0,
                Children =
                {
                    new FileNode
                    {
                        Name = "Controls",
                        IsFolder = true,
                        IsExpanded = true,
                        Children =
                        {
                            new FileNode { Name = "Carousel.cs", IsFolder = false, Size = 18432 },
                            new FileNode { Name = "RangeSlider.cs", IsFolder = false, Size = 24768 },
                            new FileNode { Name = "TreeView.xaml", IsFolder = false, Size = 6912 },
                        },
                    },
                    new FileNode
                    {
                        Name = "Themes",
                        IsFolder = true,
                        Children =
                        {
                            new FileNode { Name = "BasicTheme.xaml", IsFolder = false, Size = 14336 },
                            new FileNode { Name = "DarkTheme.xaml", IsFolder = false, Size = 12288 },
                        },
                    },
                    new FileNode { Name = "Cyclone.Wpf.csproj", IsFolder = false, Size = 2048 },
                },
            },
            new FileNode
            {
                Name = "Cyclone.Wpf.Demo",
                IsFolder = true,
                Children =
                {
                    new FileNode
                    {
                        Name = "Views",
                        IsFolder = true,
                        Children =
                        {
                            new FileNode { Name = "CarouselView.xaml", IsFolder = false, Size = 22016 },
                            new FileNode { Name = "RangeSliderView.xaml", IsFolder = false, Size = 30720 },
                            new FileNode { Name = "TreeViewView.xaml", IsFolder = false, Size = 25600 },
                        },
                    },
                    new FileNode { Name = "App.xaml", IsFolder = false, Size = 1024 },
                },
            },
        };
    }

    private static ObservableCollection<DepartmentNode> BuildOrgTree()
    {
        return new ObservableCollection<DepartmentNode>
        {
            new DepartmentNode
            {
                Name = "Cyclone Engineering",
                IsExpanded = true,
                Children =
                {
                    new DepartmentNode
                    {
                        Name = "Frontend",
                        IsExpanded = true,
                        Children =
                        {
                            new EmployeeNode { Name = "James Yang", Title = "Tech Lead" },
                            new EmployeeNode { Name = "Aria Chen", Title = "Senior Engineer" },
                            new EmployeeNode { Name = "Owen Liu", Title = "Engineer" },
                        },
                    },
                    new DepartmentNode
                    {
                        Name = "Backend",
                        Children =
                        {
                            new EmployeeNode { Name = "Mei Zhang", Title = "Staff Engineer" },
                            new EmployeeNode { Name = "Kai Wang", Title = "Senior Engineer" },
                        },
                    },
                    new DepartmentNode
                    {
                        Name = "Design",
                        Children =
                        {
                            new EmployeeNode { Name = "Lin Hao", Title = "Lead Designer" },
                            new EmployeeNode { Name = "Yuki Sato", Title = "Designer" },
                        },
                    },
                },
            },
        };
    }

    [RelayCommand]
    private void ExpandAll()
    {
        foreach (var root in OrgTree)
        {
            SetExpandedRecursive(root, true);
        }
    }

    [RelayCommand]
    private void CollapseAll()
    {
        foreach (var root in OrgTree)
        {
            SetExpandedRecursive(root, false);
        }
    }

    public TreeViewViewModel()
    {
        // 每个 sample 用独立集合实例——避免 default ICollectionView CurrentItem 跨控件同步
        FileTree = BuildFileTree();
        OrgTree = BuildOrgTree();
        FocusedTree = BuildFileTree();
        UnfocusedTree = BuildFileTree();
    }
}

/// <summary>
/// 树节点抽象基类——所有树形数据共用 IsExpanded / IsSelected / Children 三件套，
/// 通过 HierarchicalDataTemplate.DataType 在派生类型上分发不同视觉。
/// </summary>
public abstract partial class TreeNodeBase : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public ObservableCollection<TreeNodeBase> Children { get; } = new();
}

public partial class FileNode : TreeNodeBase
{
    [ObservableProperty]
    public partial bool IsFolder { get; set; }

    [ObservableProperty]
    public partial long Size { get; set; }

    public string SizeText => IsFolder ? $"{Children.Count} items" : FormatSize(Size);

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024):F1} MB";
    }
}

public partial class DepartmentNode : TreeNodeBase
{
    public int MemberCount => CountEmployees(this);

    private static int CountEmployees(TreeNodeBase node)
    {
        return node.Children.OfType<EmployeeNode>().Count()
             + node.Children.OfType<DepartmentNode>().Sum(CountEmployees);
    }
}

public partial class EmployeeNode : TreeNodeBase
{
    [ObservableProperty]
    public partial string Title { get; set; }
}