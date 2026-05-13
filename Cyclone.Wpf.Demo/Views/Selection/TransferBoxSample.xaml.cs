using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class TransferBoxSample : UserControl
{
    /// <summary>Card 4：监听 SourceItemsChanged 路由事件。</summary>
    private void OnSourceItemsChanged(object sender, RoutedEventArgs e)
    {
        if (sender is TransferBox box && DataContext is TransferBoxViewModel vm)
        {
            vm.RecordActivity($"源列表变化 — 当前 {box.SourceItems?.Count ?? 0} 项");
        }
    }

    /// <summary>Card 4：监听 TargetItemsChanged 路由事件。</summary>
    private void OnTargetItemsChanged(object sender, RoutedEventArgs e)
    {
        if (sender is TransferBox box && DataContext is TransferBoxViewModel vm)
        {
            vm.RecordActivity($"目标列表变化 — 当前 {box.TargetItems?.Count ?? 0} 项");
        }
    }

    public TransferBoxSample()
    {
        InitializeComponent();
        DataContext = new TransferBoxViewModel();
    }
}

public class TeamMember
{
    public string Name { get; set; }
    public string Role { get; set; }
}

public class Permission
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string IconData { get; set; }
}

public class TableColumn
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public partial class TransferBoxViewModel : ObservableObject
{
    #region Card 1 - 基础字符串

    public ObservableCollection<string> AvailableLanguages { get; } = new();
    public ObservableCollection<string> SelectedLanguages { get; } = new();

    #endregion Card 1 - 基础字符串

    #region Card 2 - 自定义对象 + DisplayMemberPath

    public ObservableCollection<TeamMember> AvailableMembers { get; } = new();
    public ObservableCollection<TeamMember> SelectedMembers { get; } = new();

    #endregion Card 2 - 自定义对象 + DisplayMemberPath

    #region Card 3 - ItemTemplate

    public ObservableCollection<Permission> AvailablePermissions { get; } = new();
    public ObservableCollection<Permission> GrantedPermissions { get; } = new();

    #endregion Card 3 - ItemTemplate

    #region Card 4 - 路由事件监听

    public ObservableCollection<string> AvailableFruits { get; } = new();
    public ObservableCollection<string> SelectedFruits { get; } = new();

    public ObservableCollection<string> RecentActivity { get; } = new();

    [ObservableProperty]
    public partial int TotalMoveCount { get; set; }

    #endregion Card 4 - 路由事件监听

    #region Card 5 - 表格列配置实战

    public ObservableCollection<TableColumn> HiddenColumns { get; } = new();
    public ObservableCollection<TableColumn> VisibleColumns { get; } = new();

    #endregion Card 5 - 表格列配置实战

    /// <summary>Card 4：被 code-behind 的事件 handler 调用，更新活动日志和总移动次数。</summary>
    public void RecordActivity(string message)
    {
        TotalMoveCount++;
        RecentActivity.Insert(0, $"[{DateTime.Now:HH:mm:ss}]  {message}");
        while (RecentActivity.Count > 6)
        {
            RecentActivity.RemoveAt(RecentActivity.Count - 1);
        }
    }

    public TransferBoxViewModel()
    {
        InitLanguages();
        InitMembers();
        InitPermissions();
        InitFruits();
        InitColumns();
    }

    #region 初始化数据

    private void InitLanguages()
    {
        var langs = new[]
        {
            "C", "C++", "C#", "Java", "JavaScript", "TypeScript",
            "Python", "Go", "Rust", "Swift",
        };
        foreach (var l in langs)
        {
            AvailableLanguages.Add(l);
        }
    }

    private void InitMembers()
    {
        AvailableMembers.Add(new TeamMember { Name = "张三", Role = "前端工程师" });
        AvailableMembers.Add(new TeamMember { Name = "李四", Role = "后端工程师" });
        AvailableMembers.Add(new TeamMember { Name = "王五", Role = "产品经理" });
        AvailableMembers.Add(new TeamMember { Name = "赵六", Role = "UI 设计师" });
        AvailableMembers.Add(new TeamMember { Name = "孙七", Role = "QA 工程师" });
        AvailableMembers.Add(new TeamMember { Name = "周八", Role = "DevOps 工程师" });
        AvailableMembers.Add(new TeamMember { Name = "吴九", Role = "数据分析师" });
    }

    private void InitPermissions()
    {
        AvailablePermissions.Add(new Permission
        {
            Name = "读取",
            Description = "查看数据，不能修改",
            IconData = "M12 4.5C7 4.5 2.73 7.61 1 12c1.73 4.39 6 7.5 11 7.5s9.27-3.11 11-7.5c-1.73-4.39-6-7.5-11-7.5zM12 17c-2.76 0-5-2.24-5-5s2.24-5 5-5 5 2.24 5 5-2.24 5-5 5zm0-8c-1.66 0-3 1.34-3 3s1.34 3 3 3 3-1.34 3-3-1.34-3-3-3z",
        });
        AvailablePermissions.Add(new Permission
        {
            Name = "写入",
            Description = "创建和修改数据",
            IconData = "M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z",
        });
        AvailablePermissions.Add(new Permission
        {
            Name = "删除",
            Description = "永久删除数据，不可恢复",
            IconData = "M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z",
        });
        AvailablePermissions.Add(new Permission
        {
            Name = "管理用户",
            Description = "添加、移除、修改用户",
            IconData = "M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5c-1.66 0-3 1.34-3 3s1.34 3 3 3zm-8 0c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5C6.34 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5c0-2.33-4.67-3.5-7-3.5zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z",
        });
        AvailablePermissions.Add(new Permission
        {
            Name = "配置系统",
            Description = "修改系统级配置",
            IconData = "M19.14 12.94c.04-.3.06-.61.06-.94 0-.32-.02-.64-.07-.94l2.03-1.58c.18-.14.23-.41.12-.61l-1.92-3.32c-.12-.22-.37-.29-.59-.22l-2.39.96c-.5-.38-1.03-.7-1.62-.94l-.36-2.54c-.04-.24-.24-.41-.48-.41h-3.84c-.24 0-.43.17-.47.41l-.36 2.54c-.59.24-1.13.56-1.62.94l-2.39-.96c-.22-.08-.47 0-.59.22L2.74 8.87c-.12.21-.08.47.12.61l2.03 1.58c-.05.3-.09.63-.09.94s.02.64.07.94l-2.03 1.58c-.18.14-.23.41-.12.61l1.92 3.32c.12.22.37.29.59.22l2.39-.96c.5.38 1.03.7 1.62.94l.36 2.54c.05.24.24.41.48.41h3.84c.24 0 .44-.17.47-.41l.36-2.54c.59-.24 1.13-.56 1.62-.94l2.39.96c.22.08.47 0 .59-.22l1.92-3.32c.12-.22.07-.47-.12-.61l-2.01-1.58zM12 15.6c-1.98 0-3.6-1.62-3.6-3.6s1.62-3.6 3.6-3.6 3.6 1.62 3.6 3.6-1.62 3.6-3.6 3.6z",
        });
        AvailablePermissions.Add(new Permission
        {
            Name = "导出数据",
            Description = "下载或导出报表数据",
            IconData = "M19 12v7H5v-7H3v7c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2v-7h-2zm-6 .67l2.59-2.58L17 11.5l-5 5-5-5 1.41-1.41L11 12.67V3h2v9.67z",
        });
    }

    private void InitFruits()
    {
        var fruits = new[]
        {
            "Apple", "Banana", "Cherry", "Date",
            "Elderberry", "Fig", "Grape", "Honeydew",
        };
        foreach (var f in fruits)
        {
            AvailableFruits.Add(f);
        }
    }

    private void InitColumns()
    {
        var allColumns = new[]
        {
            ("ID", "员工编号"),
            ("Name", "姓名"),
            ("Department", "部门"),
            ("Email", "邮箱"),
            ("Phone", "电话"),
            ("Office", "办公室"),
            ("Title", "职位"),
            ("Tenure", "司龄"),
        };

        // 默认 ID / Name / Email 可见
        var defaultVisible = new[] { "ID", "Name", "Email" };
        foreach (var (name, desc) in allColumns)
        {
            var col = new TableColumn { Name = name, Description = desc };
            if (Array.IndexOf(defaultVisible, name) >= 0)
            {
                VisibleColumns.Add(col);
            }
            else
            {
                HiddenColumns.Add(col);
            }
        }
    }

    #endregion 初始化数据
}