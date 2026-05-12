using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class PaginationSample : UserControl
{
    public PaginationSample()
    {
        InitializeComponent();
        DataContext = new PaginationViewModel();
    }
}

public partial class PaginationViewModel : ObservableObject
{
    // 模拟整个数据集 (实际项目中数据在数据库/接口,只查当前页)
    private readonly List<UserRow> _allUsers;

    [ObservableProperty]
    public partial ObservableCollection<UserRow> CurrentPageUsers { get; set; } = [];

    // 状态显示
    [ObservableProperty]
    public partial string CurrentRange { get; set; } = "";

    [ObservableProperty]
    public partial int ItemCount { get; set; }

    [ObservableProperty]
    public partial int LoadCount { get; set; }

    // 分页三件套 — 纯 WPF binding 用法,ViewModel 不依赖控件库
    [ObservableProperty]
    public partial int PageIndex { get; set; } = 1;

    [ObservableProperty]
    public partial int PageSize { get; set; } = 10;

    // 操作总数据 — 演示 ItemCount 变化时分页自动适应
    [RelayCommand]
    private void AddRandomUsers()
    {
        var rng = new Random();
        for (int i = 0; i < 20; i++)
        {
            _allUsers.Add(new UserRow
            {
                Id = _allUsers.Count + 1,
                Name = "新用户 " + (_allUsers.Count + 1),
                Department = "其它",
                Email = $"new{_allUsers.Count + 1}@example.com",
                Age = 25 + rng.Next(20)
            });
        }
        ItemCount = _allUsers.Count;
        LoadCurrentPage();
    }

    [RelayCommand]
    private void ClearAll()
    {
        _allUsers.Clear();
        ItemCount = 0;
        LoadCurrentPage();
    }

    private void LoadCurrentPage()
    {
        if (ItemCount == 0)
        {
            CurrentPageUsers = [];
            CurrentRange = "0 / 0";
            return;
        }

        int skip = (PageIndex - 1) * PageSize;
        var rows = _allUsers.Skip(skip).Take(PageSize).ToList();
        CurrentPageUsers = new ObservableCollection<UserRow>(rows);

        int firstItem = skip + 1;
        int lastItem = Math.Min(skip + PageSize, ItemCount);
        CurrentRange = $"{firstItem} - {lastItem} / {ItemCount}";

        LoadCount++;
    }

    // 分页参数变化自动重新"查询"
    partial void OnPageIndexChanged(int value) => LoadCurrentPage();

    partial void OnPageSizeChanged(int value) => LoadCurrentPage();

    [RelayCommand]
    private void RemoveHalfUsers()
    {
        int target = _allUsers.Count / 2;
        _allUsers.RemoveRange(target, _allUsers.Count - target);
        ItemCount = _allUsers.Count;
        LoadCurrentPage();
    }

    public PaginationViewModel()
    {
        // 造点假数据
        var names = new[] { "Alice", "Bob", "Charlie", "Diana", "Eve", "Frank", "Grace", "Henry", "Iris", "Jack" };
        var deps = new[] { "工程", "设计", "市场", "财务", "运营", "客服" };
        var rng = new Random(42);

        _allUsers = Enumerable.Range(1, 137)
            .Select(i => new UserRow
            {
                Id = i,
                Name = names[i % names.Length] + " " + i,
                Department = deps[rng.Next(deps.Length)],
                Email = $"user{i}@example.com",
                Age = 22 + rng.Next(30)
            })
            .ToList();

        ItemCount = _allUsers.Count;
        LoadCurrentPage();
    }
}

public class UserRow
{
    public int Age { get; set; }

    public string Department { get; set; }

    public string Email { get; set; }

    public int Id { get; set; }

    public string Name { get; set; }
}