using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Demo.Helper;
using Cyclone.Wpf.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class DataGridSample : UserControl
{
    public DataGridSample()
    {
        InitializeComponent();
        DataContext = new DataGridViewModel();
    }
}

public partial class DataGridViewModel : ObservableObject
{
    // ===== 章节① 多种列类型 =====
    public ObservableCollection<EditableEmployee> BasicData { get; }

    // 部门下拉选项（章节①② ComboBoxColumn 用）
    public ObservableCollection<string> Departments { get; }

    // ===== 章节⑤ 行详情 =====
    public ObservableCollection<FakerData> DetailsData { get; }

    // ===== 章节② 单元格编辑 =====
    public ObservableCollection<EditableEmployee> EditableData { get; }

    // ===== 章节③ 多选 =====
    public ObservableCollection<FakerData> MultiSelectData { get; }

    // ===== 章节④ 自动生成列 =====
    public ObservableCollection<DataGridProductSample> Products { get; }

    public ObservableCollection<object> SelectedRows { get; } = new();

    private static ObservableCollection<EditableEmployee> CreateEmployees(int count)
    {
        var result = new ObservableCollection<EditableEmployee>();
        var samples = new[]
        {
            ("张伟", "zhang.wei@example.com", "技术", 28, 15000m, true),
            ("李娜", "li.na@example.com", "产品", 32, 18500m, true),
            ("王强", "wang.qiang@example.com", "运营", 26, 12000m, true),
            ("赵敏", "zhao.min@example.com", "市场", 34, 22000m, true),
            ("陈静", "chen.jing@example.com", "技术", 30, 17000m, false),
            ("刘洋", "liu.yang@example.com", "产品", 29, 16500m, true),
            ("孙莉", "sun.li@example.com", "人事", 27, 14000m, true),
            ("周杰", "zhou.jie@example.com", "财务", 36, 25000m, true),
        };

        for (int i = 0; i < count && i < samples.Length; i++)
        {
            var (name, email, dept, age, salary, active) = samples[i];
            result.Add(new EditableEmployee
            {
                Name = name,
                Email = email,
                Department = dept,
                Age = age,
                Salary = salary,
                IsActive = active,
            });
        }
        return result;
    }

    public DataGridViewModel()
    {
        Departments = new ObservableCollection<string> { "技术", "产品", "运营", "市场", "人事", "财务" };

        BasicData = CreateEmployees(8);
        EditableData = CreateEmployees(6);

        var pool = FakerDataHelper.GenerateFakerDataCollection(40);
        MultiSelectData = new ObservableCollection<FakerData>(pool.Take(10));
        DetailsData = new ObservableCollection<FakerData>(pool.Skip(10).Take(6));

        Products = new ObservableCollection<DataGridProductSample>
        {
            new DataGridProductSample { Sku = "P-001", Name = "无线鼠标", Category = "外设", Price = 89.00m, Stock = 245, IsActive = true },
            new DataGridProductSample { Sku = "P-002", Name = "机械键盘", Category = "外设", Price = 459.00m, Stock = 87, IsActive = true },
            new DataGridProductSample { Sku = "P-003", Name = "27寸显示器", Category = "显示", Price = 1899.00m, Stock = 32, IsActive = true },
            new DataGridProductSample { Sku = "P-004", Name = "蓝牙耳机", Category = "音频", Price = 299.00m, Stock = 0, IsActive = false },
            new DataGridProductSample { Sku = "P-005", Name = "USB Hub", Category = "外设", Price = 79.00m, Stock = 156, IsActive = true },
            new DataGridProductSample { Sku = "P-006", Name = "网络摄像头", Category = "影像", Price = 359.00m, Stock = 68, IsActive = true },
            new DataGridProductSample { Sku = "P-007", Name = "桌面音箱", Category = "音频", Price = 599.00m, Stock = 24, IsActive = true },
        };
    }
}

/// <summary>
/// 可编辑员工模型——所有属性可写，用于演示 DataGrid 单元格编辑。
/// </summary>
public partial class EditableEmployee : ObservableObject
{
    [ObservableProperty]
    public partial int Age { get; set; }

    [ObservableProperty]
    public partial string Department { get; set; }

    [ObservableProperty]
    public partial string Email { get; set; }

    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial decimal Salary { get; set; }

    [ObservableProperty]
    public partial EmployeeStatus Status { get; set; }
}

public enum EmployeeStatus
{
    [Description("无")]
    None,

    [Description("激活")]
    Active,

    [Description("未激活")]
    Inactive,

    [Description("待激活")]
    Pending,

    [Description("封禁")]
    Blocked,

    [Description("删除")]
    Deleted,
}

/// <summary>
/// 商品样例模型——演示 [DataGridProperty] 特性自动生成 DataGrid 列。
/// 章节④ 用。Index 控制列顺序、StringFormat 格式化、IsReadOnly 只读列。
///
/// 命名为 DataGridProductSample 而非 Product——避免和 demo 项目其他位置已有的
/// Product 业务类型撞名。本类仅作为 DataGrid 自动列生成的样例数据。
/// </summary>
public partial class DataGridProductSample : ObservableObject
{
    [DataGridProperty("品类", Index = 2, Width = 100)]
    [ObservableProperty]
    public partial string Category { get; set; }

    // 不标特性的属性不会生成列——证明特性是显式声明
    public string InternalNote { get; set; } = "（这个属性不会出现在 DataGrid 里）";

    [DataGridProperty("上架", Index = 5, Width = 60)]
    [ObservableProperty]
    public partial bool IsActive { get; set; }

    [DataGridProperty("商品名", Index = 1, Width = 160)]
    [ObservableProperty]
    public partial string Name { get; set; }

    [DataGridProperty("单价", Index = 3, Width = 100, StringFormat = "¥{0:N2}")]
    [ObservableProperty]
    public partial decimal Price { get; set; }

    [DataGridProperty("SKU", Index = 0, Width = 80, IsReadOnly = true)]
    [ObservableProperty]
    public partial string Sku { get; set; }

    [DataGridProperty("库存", Index = 4, Width = 80)]
    [ObservableProperty]
    public partial int Stock { get; set; }
}