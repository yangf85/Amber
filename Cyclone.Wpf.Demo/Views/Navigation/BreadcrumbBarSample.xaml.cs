using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// BreadcrumbView.xaml 的交互逻辑
/// </summary>
public partial class BreadcrumbBarSample : UserControl
{
    public BreadcrumbBarSample()
    {
        InitializeComponent();
        DataContext = new BreadcrumbBarViewModel();
    }
}

public partial class BreadcrumbBarViewModel : ObservableObject
{
    public ObservableCollection<BreadcrumbItemViewModel> NavigationPath { get; }

    public ICommand NavigateCommand { get; }

    private void Navigate(BreadcrumbItemViewModel item)
    {
        // 导航逻辑
        if (item != null)
        {
            // 设置当前项
            foreach (var pathItem in NavigationPath)
            {
                pathItem.IsCurrent = (pathItem == item);
            }

            // 这里可以添加其他导航逻辑
            // 例如：更新内容区域，加载相关数据等
        }
    }

    public BreadcrumbBarViewModel()
    {
        // 创建导航路径
        NavigationPath = new ObservableCollection<BreadcrumbItemViewModel>
        {
            new BreadcrumbItemViewModel { Title = "首页", Icon = "\uE80F" },
            new BreadcrumbItemViewModel
            {
                Title = "产品",
                Icon = "\uE7FC",
                Children = new ObservableCollection<BreadcrumbItemViewModel>
                {
                    new BreadcrumbItemViewModel { Title = "服装" },
                    new BreadcrumbItemViewModel { Title = "电子产品" },
                    new BreadcrumbItemViewModel { Title = "家居" }
                }
            },
            new BreadcrumbItemViewModel { Title = "电子产品", Icon = "\uEC4F" },
            new BreadcrumbItemViewModel { Title = "智能手机", IsCurrent = true, Icon = "\uE8EA" }
        };

        // 创建导航命令
        NavigateCommand = new RelayCommand<BreadcrumbItemViewModel>(Navigate);
    }
}

public partial class BreadcrumbItemViewModel : ObservableObject
{
    private string _title;

    private object _icon;

    private bool _isCurrent;

    private ObservableCollection<BreadcrumbItemViewModel> _children;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public object Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    public bool IsCurrent
    {
        get => _isCurrent;
        set => SetProperty(ref _isCurrent, value);
    }

    public ObservableCollection<BreadcrumbItemViewModel> Children
    {
        get => _children;
        set => SetProperty(ref _children, value);
    }
}