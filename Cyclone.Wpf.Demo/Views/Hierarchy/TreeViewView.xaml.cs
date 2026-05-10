using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class TreeViewView : UserControl
{
    /// <summary>
    /// SelectedItemChanged code-behind handler——TreeView.SelectedItem 是 readonly DP，
    /// 不能 TwoWay binding；这里收到事件后同步到 VM。
    /// </summary>
    private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is TreeViewViewModel vm && e.NewValue is TreeNode node)
        {
            vm.SelectedNode = node;
        }
    }

    public TreeViewView()
    {
        InitializeComponent();
    }
}

public partial class TreeViewViewModel : ObservableObject
{
    [ObservableProperty]
    public partial TreeNode SelectedNode { get; set; }

    public ObservableCollection<TreeNode> DataDrivenTree { get; }

    public ObservableCollection<TreeNode> FileTree { get; }

    public ObservableCollection<TreeNode> SelectableTree { get; }

    private static ObservableCollection<TreeNode> CreateProjectTree()
    {
        return new ObservableCollection<TreeNode>
        {
            new TreeNode
            {
                Name = "MyProject",
                Children =
                {
                    new TreeNode
                    {
                        Name = "src",
                        Children =
                        {
                            new TreeNode { Name = "Models" },
                            new TreeNode { Name = "Views" },
                            new TreeNode { Name = "ViewModels" },
                        },
                    },
                    new TreeNode
                    {
                        Name = "tests",
                        Children =
                        {
                            new TreeNode { Name = "UnitTests" },
                            new TreeNode { Name = "IntegrationTests" },
                        },
                    },
                    new TreeNode { Name = "docs" },
                    new TreeNode { Name = "README.md" },
                },
            },
        };
    }

    private static ObservableCollection<TreeNode> CreateFileTree()
    {
        return new ObservableCollection<TreeNode>
        {
            new TreeNode
            {
                Name = "C:\\",
                Icon = "💾",
                Children =
                {
                    new TreeNode
                    {
                        Name = "Documents",
                        Icon = "📁",
                        Count = 3,
                        Children =
                        {
                            new TreeNode { Name = "Report.docx", Icon = "📄" },
                            new TreeNode { Name = "Budget.xlsx", Icon = "📊" },
                            new TreeNode
                            {
                                Name = "Photos",
                                Icon = "📁",
                                Count = 2,
                                Children =
                                {
                                    new TreeNode { Name = "vacation.jpg", Icon = "🖼" },
                                    new TreeNode { Name = "family.jpg", Icon = "🖼" },
                                },
                            },
                        },
                    },
                    new TreeNode
                    {
                        Name = "Downloads",
                        Icon = "📁",
                        Count = 2,
                        Children =
                        {
                            new TreeNode { Name = "installer.exe", Icon = "📦" },
                            new TreeNode { Name = "readme.txt", Icon = "📄" },
                        },
                    },
                    new TreeNode
                    {
                        Name = "Music",
                        Icon = "📁",
                        Count = 1,
                        Children =
                        {
                            new TreeNode { Name = "playlist.mp3", Icon = "🎵" },
                        },
                    },
                },
            },
        };
    }

    public TreeViewViewModel()
    {
        // 每个 sample 独立的 root collection——避免 Selection / IsExpanded 跨样例同步
        DataDrivenTree = CreateProjectTree();
        FileTree = CreateFileTree();
        SelectableTree = CreateProjectTree();
    }
}

public partial class TreeNode : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Icon { get; set; }

    [ObservableProperty]
    public partial int Count { get; set; }

    public ObservableCollection<TreeNode> Children { get; } = new();
}