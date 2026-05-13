using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class MultiComboBoxSample : UserControl
{
    public MultiComboBoxSample()
    {
        InitializeComponent();
        DataContext = new MultiComboBoxViewModel();
    }
}

public class MultiComboBoxViewModel : INotifyPropertyChanged
{
    private ObservableCollection<object> _selectedRecipients = new();

    // ===== 简单 string 列表 =====
    public ObservableCollection<string> Cities { get; } = new()
    {
        "北京", "上海", "广州", "深圳", "杭州", "成都", "南京", "武汉", "西安", "重庆"
    };

    public ObservableCollection<object> SelectedCities { get; } = new();

    // ===== 数据对象 =====
    public ObservableCollection<Product> Products { get; } = new()
    {
        new Product { Name = "MacBook Pro", Category = "笔记本" },
        new Product { Name = "iPhone 15", Category = "手机" },
        new Product { Name = "iPad Air", Category = "平板" },
        new Product { Name = "AirPods Pro", Category = "耳机" },
        new Product { Name = "Apple Watch", Category = "手表" },
        new Product { Name = "Magic Keyboard", Category = "配件" },
    };

    public ObservableCollection<object> SelectedProducts { get; } = new();

    // ===== 权限演示 =====
    public ObservableCollection<string> Permissions { get; } = new()
    {
        "读取", "写入", "删除", "执行", "管理用户", "系统配置"
    };

    public ObservableCollection<object> SelectedPermissions { get; } = new();

    // ===== 标签 =====
    public ObservableCollection<string> Tags { get; } = new()
    {
        "WPF", "C#", ".NET", "MVVM", "Avalonia", "MAUI", "Blazor"
    };

    public ObservableCollection<object> SelectedTags { get; } = new();

    // ===== 收件人（MVVM 演示）=====
    public ObservableCollection<Recipient> Recipients { get; } = new()
    {
        new Recipient { Name = "张三", Email = "zhang@example.com" },
        new Recipient { Name = "李四", Email = "li@example.com" },
        new Recipient { Name = "王五", Email = "wang@example.com" },
        new Recipient { Name = "赵六", Email = "zhao@example.com" },
    };

    public ObservableCollection<object> SelectedRecipients
    {
        get => _selectedRecipients;
        set
        {
            _selectedRecipients = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedRecipientsCount));
        }
    }

    public int SelectedRecipientsCount => _selectedRecipients?.Count ?? 0;

    // ===== 范围过滤演示 =====
    public ObservableCollection<string> PriceRanges { get; } = new()
    {
        "0 - 100", "100 - 500", "500 - 1000", "1000 - 5000", "5000+"
    };

    public ObservableCollection<object> SelectedRanges { get; } = new();

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;
}

public class Product
{
    public string Name { get; set; }
    public string Category { get; set; }
}

public class Recipient
{
    public string Name { get; set; }
    public string Email { get; set; }
}