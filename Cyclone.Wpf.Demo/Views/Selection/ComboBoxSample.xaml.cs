using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cyclone.Wpf.Demo.Views;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum ComboBoxEnum
{
    [Description("苹果")]
    Apple,

    [Description("香蕉")]
    Banana,

    [Description("橘子")]
    Orange,

    [Description("桃子")]
    Pear,

    [Description("葡萄")]
    Grape,
}

/// <summary>
/// ComboBoxView.xaml 的交互逻辑
/// </summary>
public partial class ComboBoxSample : UserControl
{
    public ComboBoxSample()
    {
        InitializeComponent();
        DataContext = new ComboBoxViewModel();
        //TypeDescriptor.AddAttributes(typeof(ComboBoxEnum), new TypeConverterAttribute(typeof(EnumDescriptionTypeConverter)));
    }
}

/// <summary>
/// 员工实体类
/// </summary>
public class Employee
{
    public string Name { get; set; }

    public string Department { get; set; }

    public string Position { get; set; }
}

public partial class ComboBoxViewModel : ObservableObject
{
    #region 属性

    [ObservableProperty]
    public partial string SelectedFruit { get; set; }

    [ObservableProperty]
    public partial ComboBoxEnum SelectedEnum { get; set; }

    [ObservableProperty]
    public partial Employee SelectedEmployee { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<string> SelectedFruits { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<string> SelectedFruitsNoClear { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<Employee> SelectedEmployees { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<Employee> SelectedGroupedEmployees { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<Employee> Employees { get; set; } = new();

    [ObservableProperty]
    public partial CollectionViewSource GroupedEmployees { get; set; } = new();

    #endregion 属性

    public ComboBoxViewModel()
    {
        InitializeData();
    }

    #region 初始化数据

    private void InitializeData()
    {
        // 创建员工数据
        Employees = new ObservableCollection<Employee>
        {
            new Employee { Name = "张三", Department = "技术部", Position = "高级工程师" },
            new Employee { Name = "李四", Department = "技术部", Position = "前端工程师" },
            new Employee { Name = "王五", Department = "技术部", Position = "后端工程师" },
            new Employee { Name = "赵六", Department = "产品部", Position = "产品经理" },
            new Employee { Name = "钱七", Department = "产品部", Position = "UI设计师" },
            new Employee { Name = "孙八", Department = "产品部", Position = "交互设计师" },
            new Employee { Name = "周九", Department = "市场部", Position = "市场经理" },
            new Employee { Name = "吴十", Department = "市场部", Position = "商务专员" },
            new Employee { Name = "郑一", Department = "人事部", Position = "HR经理" },
            new Employee { Name = "王二", Department = "人事部", Position = "招聘专员" }
        };

        // 设置分组数据源
        GroupedEmployees = new CollectionViewSource();
        GroupedEmployees.Source = Employees;
        // 设置分组数据源
        GroupedEmployees = new CollectionViewSource();
        GroupedEmployees.Source = Employees;
        GroupedEmployees.GroupDescriptions.Add(new PropertyGroupDescription("Department"));
    }

    #endregion 初始化数据

    #region 命令

    [RelayCommand]
    private void ShowSelectedEnum()
    {
        MessageBox.Show($"当前选中的枚举值: {SelectedEnum}", "枚举结果", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ShowSelectedFruits()
    {
        if (SelectedFruits?.Count > 0)
        {
            var fruits = string.Join("、", SelectedFruits);
            MessageBox.Show($"选中的水果: {fruits}", "多选结果", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("没有选中任何水果", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void ShowSelectedEmployees()
    {
        var allSelectedEmployees = new List<Employee>();

        if (SelectedEmployees?.Count > 0)
            allSelectedEmployees.AddRange(SelectedEmployees);

        if (SelectedGroupedEmployees?.Count > 0)
            allSelectedEmployees.AddRange(SelectedGroupedEmployees);

        if (allSelectedEmployees.Count > 0)
        {
            var employees = string.Join("、", allSelectedEmployees.Distinct().Select(e => $"{e.Name}({e.Department})"));
            MessageBox.Show($"选中的员工: {employees}", "多选结果", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("没有选中任何员工", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void ClearAllSelections()
    {
        SelectedFruit = null;
        SelectedEmployee = null;
        SelectedFruits?.Clear();
        SelectedFruitsNoClear?.Clear();
        SelectedEmployees?.Clear();
        SelectedGroupedEmployees?.Clear();

        MessageBox.Show("已清空所有选择", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion 命令
}