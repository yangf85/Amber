using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.Helper;
using Cyclone.Wpf.Helpers;
using Faker;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
/// DataGridView.xaml 的交互逻辑
/// </summary>
public partial class DataGridView : UserControl
{
    public DataGridView()
    {
        InitializeComponent();
        DataContext = new DataGridViewModel();
    }
}

public partial class PaginationViewModel<T> : ObservableValidator, IPagination
{
    private IEnumerable<T> _source;

    [ObservableProperty]
    public partial ObservableCollection<T> Data { get; set; } = [];

    public PaginationViewModel(IEnumerable<T> source)
    {
        _source = source;
        Update();
        Total = source.Count();
    }

    #region Implementation Pagination

    [Range(1, int.MaxValue)]
    [NotifyDataErrorInfo]
    [ObservableProperty]
    public partial int PageIndex { get; set; } = 1;

    [Range(1, int.MaxValue)]
    [NotifyDataErrorInfo]
    [ObservableProperty]
    public partial int PerPageCount { get; set; } = 10;

    [ObservableProperty]
    public partial int Total { get; set; } = 100;

    partial void OnPageIndexChanged(int value)
    {
        Update();
    }

    partial void OnPerPageCountChanged(int value)
    {
        Update();
    }

    partial void OnTotalChanged(int value)
    {
        Update();
    }

    private void Update()
    {
        // 计算最大页数
        int maxPage = Total > 0 ? (int)Math.Ceiling(Total / (double)PerPageCount) : 1;

        // 调整 PageIndex 到有效范围
        if (PageIndex < 1)
        {
            PageIndex = 1;
        }
        else if (PageIndex > maxPage && maxPage > 0)
        {
            PageIndex = maxPage;
        }

        // 计算当前页的数据
        Data = new ObservableCollection<T>(_source.Skip((PageIndex - 1) * PerPageCount).Take(PerPageCount));
    }

    #endregion Implementation Pagination
}

public partial class DataGridViewModel : ObservableObject
{
    [ObservableProperty]
    public partial PaginationViewModel<FakerData> Pagination { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<FakerData> SelectedItems { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<DataGridItem> MockData { get; set; } = [];

    [RelayCommand]
    private void ShowSelectedItems()
    {
        if (SelectedItems == null || SelectedItems.Count == 0)
        {
            MessageBox.Show("No selected items");
        }
        else
        {
            MessageBox.Show($"{string.Join("\n", SelectedItems)}");
        }
    }

    private void InitMockData()
    {
        MockData =
        [
            new DataGridItem()
            {
                Name = "John Doe",
                Gender = true,
                Age = 30,
                Status = Status.Success,
            },
            new DataGridItem()
            {
                Name = "Hally Bote",
                Gender = false,
                Age = 28,
                Status = Status.Warning,
            },
            new DataGridItem()
            {
                Name = "Jane Doe",
                Gender = false,
                Age = 35,
                Status = Status.Error,
            },

        ];
    }

    public DataGridViewModel()
    {
        InitMockData();
        Pagination = new PaginationViewModel<FakerData>(FakerDataHelper.GenerateFakerDataCollection(75));
    }
}

public enum Status
{
    Success,

    Warning,

    Error
}

public partial class DataGridItem : ObservableValidator
{
    [ObservableProperty]
    [DataGridProperty("名称", Index = 1)]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Name is required")]
    public partial string Name { get; set; }

    [ObservableProperty]
    [DataGridProperty("性别", Index = 2)]
    public partial bool Gender { get; set; }

    [ObservableProperty]
    [DataGridProperty("年龄", Index = 3)]
    [NotifyDataErrorInfo]
    [Range(1, 100, ErrorMessage = "Age must be between 1 and 100")]
    public partial int Age { get; set; }

    [ObservableProperty]
    [DataGridProperty("状态", Index = 4)]
    public partial Status Status { get; set; }

    [ObservableProperty]
    public partial string Icon { get; set; }
}