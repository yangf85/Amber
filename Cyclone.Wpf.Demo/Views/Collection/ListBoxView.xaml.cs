using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Demo.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// ListBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class ListBoxView : UserControl
    {
        public ListBoxView()
        {
            InitializeComponent();
            DataContext = new ListBoxViewModel();
        }
    }

    public partial class ListBoxViewModel : ObservableObject
    {
        private readonly ObservableCollection<FakerData> _originalData;
        private ICollectionView _filteredData;

        // ===== 基础 ListBox 数据（节①）=====
        [ObservableProperty]
        public partial ObservableCollection<FakerData> BasicData { get; set; }

        [ObservableProperty]
        public partial FakerData? SelectedBasicPerson { get; set; }

        // ===== SelectionMode 演示（节②）=====
        public ObservableCollection<string> Fruits { get; }

        // ===== IsSelectAllEnabled 演示（节③）=====
        public ObservableCollection<string> Permissions { get; }

        [ObservableProperty]
        public partial bool IsSelectAllEnabled { get; set; } = true;

        // ===== 数据模板演示（节④）=====
        public ObservableCollection<FakerData> TemplateSample { get; }

        [ObservableProperty]
        public partial FakerData? SelectedTemplatePerson { get; set; }

        // ===== 高级 ListBox 的过滤、排序、分组（Tab 2）=====
        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SelectedAgeRange { get; set; } = "全部";

        [ObservableProperty]
        public partial string SelectedSortOption { get; set; } = "姓名";

        [ObservableProperty]
        public partial bool IsDescending { get; set; } = false;

        [ObservableProperty]
        public partial string SelectedGroupOption { get; set; } = "无";

        [ObservableProperty]
        public partial string SelectedStatusFilter { get; set; } = "全部";

        [ObservableProperty]
        public partial FakerData? SelectedAdvancedPerson { get; set; }

        [ObservableProperty]
        public partial string StatusText { get; set; } = string.Empty;

        public ObservableCollection<string> AgeRanges { get; }

        public ObservableCollection<string> SortOptions { get; }

        public ObservableCollection<string> GroupOptions { get; }

        public ObservableCollection<string> StatusFilters { get; }

        public ICollectionView FilteredData
        {
            get => _filteredData;
            private set => SetProperty(ref _filteredData, value);
        }

        public ListBoxViewModel()
        {
            // 大数据集（高级 tab 用）
            var testData = FakerDataHelper.GenerateFakerDataCollection(50);

            BasicData = new ObservableCollection<FakerData>(testData.Take(8));
            TemplateSample = new ObservableCollection<FakerData>(testData.Take(6));

            // 章节用小数据集
            Fruits = new ObservableCollection<string>
            {
                "苹果", "香蕉", "橙子", "葡萄", "西瓜", "草莓", "蓝莓", "芒果",
            };

            Permissions = new ObservableCollection<string>
            {
                "读取文件", "写入文件", "删除文件", "执行程序", "网络访问", "系统设置", "用户管理", "审计日志",
            };

            // 高级 tab 数据
            _originalData = new ObservableCollection<FakerData>(testData);

            AgeRanges = new ObservableCollection<string> { "全部", "0-18", "19-30", "31-45", "46-60", "60+" };
            SortOptions = new ObservableCollection<string> { "姓名", "年龄", "城市", "邮箱", "状态" };
            GroupOptions = new ObservableCollection<string> { "无", "城市", "国家", "状态" };
            StatusFilters = new ObservableCollection<string> { "全部", "激活", "未激活", "待激活" };

            FilteredData = CollectionViewSource.GetDefaultView(_originalData);
            FilteredData.Filter = FilterItems;

            UpdateStatusText();
        }

        partial void OnSearchTextChanged(string value) => ApplyFiltersAndSort();

        partial void OnSelectedAgeRangeChanged(string value) => ApplyFiltersAndSort();

        partial void OnSelectedSortOptionChanged(string value) => ApplyFiltersAndSort();

        partial void OnIsDescendingChanged(bool value) => ApplyFiltersAndSort();

        partial void OnSelectedGroupOptionChanged(string value) => ApplyFiltersAndSort();

        partial void OnSelectedStatusFilterChanged(string value) => ApplyFiltersAndSort();

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedAgeRange = "全部";
            SelectedSortOption = "姓名";
            IsDescending = false;
            SelectedGroupOption = "无";
            SelectedStatusFilter = "全部";
        }

        private void ApplyFiltersAndSort()
        {
            FilteredData.Refresh();

            FilteredData.SortDescriptions.Clear();
            FilteredData.GroupDescriptions.Clear();

            var sortProperty = SelectedSortOption switch
            {
                "姓名" => nameof(FakerData.FirstName),
                "年龄" => nameof(FakerData.Age),
                "城市" => nameof(FakerData.City),
                "邮箱" => nameof(FakerData.Email),
                "状态" => nameof(FakerData.Status),
                _ => nameof(FakerData.FirstName)
            };

            var sortDirection = IsDescending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            FilteredData.SortDescriptions.Add(new SortDescription(sortProperty, sortDirection));

            if (SelectedGroupOption != "无")
            {
                var groupProperty = SelectedGroupOption switch
                {
                    "城市" => nameof(FakerData.City),
                    "国家" => nameof(FakerData.Country),
                    "状态" => nameof(FakerData.Status),
                    _ => null
                };

                if (groupProperty != null)
                {
                    FilteredData.GroupDescriptions.Add(new PropertyGroupDescription(groupProperty));
                }
            }

            UpdateStatusText();
        }

        private bool FilterItems(object item)
        {
            if (item is not FakerData person)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                if (!person.FirstName.ToLower().Contains(searchLower) &&
                    !person.LastName.ToLower().Contains(searchLower) &&
                    !person.Email.ToLower().Contains(searchLower) &&
                    !person.City.ToLower().Contains(searchLower) &&
                    !person.Country.ToLower().Contains(searchLower))
                {
                    return false;
                }
            }

            if (SelectedAgeRange != "全部")
            {
                var ageInRange = SelectedAgeRange switch
                {
                    "0-18" => person.Age >= 0 && person.Age <= 18,
                    "19-30" => person.Age >= 19 && person.Age <= 30,
                    "31-45" => person.Age >= 31 && person.Age <= 45,
                    "46-60" => person.Age >= 46 && person.Age <= 60,
                    "60+" => person.Age > 60,
                    _ => true
                };

                if (!ageInRange)
                {
                    return false;
                }
            }

            if (SelectedStatusFilter != "全部")
            {
                var statusMatch = SelectedStatusFilter switch
                {
                    "激活" => person.Status == UserStatus.Active,
                    "未激活" => person.Status == UserStatus.Inactive,
                    "待激活" => person.Status == UserStatus.Pending,
                    _ => true
                };

                if (!statusMatch)
                {
                    return false;
                }
            }

            return true;
        }

        private void UpdateStatusText()
        {
            if (FilteredData != null)
            {
                var filteredCount = FilteredData.Cast<object>().Count();
                var totalCount = _originalData.Count;

                StatusText = filteredCount == totalCount
                    ? $"显示 {totalCount} 项"
                    : $"显示 {filteredCount} / {totalCount} 项";
            }
        }
    }

    // 简单的分组转换器
    public class AgeGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is FakerData person)
            {
                return person.Age switch
                {
                    >= 0 and <= 18 => "未成年 (0-18)",
                    >= 19 and <= 30 => "青年 (19-30)",
                    >= 31 and <= 45 => "中年 (31-45)",
                    >= 46 and <= 60 => "中老年 (46-60)",
                    > 60 => "老年 (60+)",
                    _ => "未知"
                };
            }
            return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
