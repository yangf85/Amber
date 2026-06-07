using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class CascadePickerSample : UserControl
{
    public CascadePickerSample()
    {
        InitializeComponent();
        DataContext = new CascadePickerViewModel();
    }
}

public partial class CascadePickerViewModel : ObservableObject
{
    public ObservableCollection<Region> Regions { get; }

    [ObservableProperty]
    public partial Region SelectedRegion { get; set; }

    [ObservableProperty]
    public partial string SelectedRegionId { get; set; }

    [ObservableProperty]
    public partial string SelectedRegionPath { get; set; }

    [ObservableProperty]
    public partial bool IsReadOnlyEnabled { get; set; }

    [ObservableProperty]
    public partial string Text { get; set; } = "用户输入的文本.";

    private static ObservableCollection<Region> BuildRegionData()
    {
        return new ObservableCollection<Region>
        {
            new Region
            {
                Id = "44", Name = "广东省",
                Children = new ObservableCollection<Region>
                {
                    new Region
                    {
                        Id = "4403", Name = "深圳市",
                        Children = new ObservableCollection<Region>
                        {
                            new Region { Id = "440305", Name = "南山区" },
                            new Region { Id = "440304", Name = "福田区" },
                            new Region { Id = "440303", Name = "罗湖区" },
                            new Region { Id = "440307", Name = "宝安区" },
                        },
                    },
                    new Region
                    {
                        Id = "4401", Name = "广州市",
                        Children = new ObservableCollection<Region>
                        {
                            new Region { Id = "440106", Name = "天河区" },
                            new Region { Id = "440104", Name = "越秀区" },
                            new Region { Id = "440105", Name = "海珠区" },
                        },
                    },
                    new Region
                    {
                        Id = "4419", Name = "东莞市",
                        Children = new ObservableCollection<Region>
                        {
                            new Region { Id = "441900001", Name = "南城街道" },
                            new Region { Id = "441900002", Name = "东城街道" },
                        },
                    },
                },
            },
            new Region
            {
                Id = "11", Name = "北京市",
                Children = new ObservableCollection<Region>
                {
                    new Region
                    {
                        Id = "1101", Name = "市辖区",
                        Children = new ObservableCollection<Region>
                        {
                            new Region { Id = "110105", Name = "朝阳区" },
                            new Region { Id = "110108", Name = "海淀区" },
                            new Region { Id = "110102", Name = "西城区" },
                            new Region { Id = "110101", Name = "东城区" },
                        },
                    },
                },
            },
            new Region
            {
                Id = "31", Name = "上海市",
                Children = new ObservableCollection<Region>
                {
                    new Region
                    {
                        Id = "3101", Name = "市辖区",
                        Children = new ObservableCollection<Region>
                        {
                            new Region { Id = "310115", Name = "浦东新区" },
                            new Region { Id = "310101", Name = "黄浦区" },
                            new Region { Id = "310104", Name = "徐汇区" },
                            new Region { Id = "310105", Name = "长宁区" },
                        },
                    },
                },
            },
            new Region
            {
                Id = "33", Name = "浙江省",
                Children = new ObservableCollection<Region>
                {
                    new Region
                    {
                        Id = "3301", Name = "杭州市",
                        Children = new ObservableCollection<Region>
                        {
                            new Region { Id = "330106", Name = "西湖区" },
                            new Region { Id = "330108", Name = "滨江区" },
                            new Region { Id = "330110", Name = "余杭区" },
                        },
                    },
                },
            },
        };
    }

    [RelayCommand]
    private void ModifyText()
    {
        Text = "后台修改文本";
    }

    [RelayCommand]
    private void SetId(string id)
    {
        SelectedRegionId = id;
    }

    public CascadePickerViewModel()
    {
        Regions = BuildRegionData();
    }
}

public class Region
{
    public string Id { get; init; }

    public string Name { get; init; }

    public ObservableCollection<Region> Children { get; init; } = new();
}