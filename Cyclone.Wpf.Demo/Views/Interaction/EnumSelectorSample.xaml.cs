using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// EnumSelector.xaml 的交互逻辑
/// </summary>
public partial class EnumSelectorSample : UserControl
{
    public EnumSelectorSample()
    {
        InitializeComponent();
        DataContext = new EnumSelectorDemoViewModel();
    }
}

public partial class EnumSelectorDemoViewModel : ObservableObject
{
    [ObservableProperty]
    public partial UserRole UserRole { get; set; } = UserRole.User;

    [ObservableProperty]
    public partial UserRole UserRoleForAliasDemo { get; set; } = UserRole.Admin;

    [ObservableProperty]
    public partial Priority Priority { get; set; } = Priority.Medium;

    [ObservableProperty]
    public partial Priority PriorityCheckboxStyle { get; set; } = Priority.High;

    [ObservableProperty]
    public partial FilePermissions FilePermissions { get; set; } = FilePermissions.Read | FilePermissions.Write;

    [ObservableProperty]
    public partial WorkDays WorkDays { get; set; } = WorkDays.Monday | WorkDays.Wednesday | WorkDays.Friday;
}

#region 枚举定义（Demo 用）

public enum UserRole
{
    [Description("访客")]
    Guest = 0,

    [Description("普通用户")]
    User = 1,

    [Description("管理员")]
    Admin = 2,

    [Description("超级管理员")]
    SuperAdmin = 3,
}

public enum Priority
{
    [Description("低")]
    Low = 1,

    [Description("中")]
    Medium = 2,

    [Description("高")]
    High = 3,

    [Description("紧急")]
    Critical = 4,
}

[Flags]
public enum FilePermissions
{
    [Description("无权限")]
    None = 0,

    [Description("读取")]
    Read = 1,

    [Description("写入")]
    Write = 2,

    [Description("执行")]
    Execute = 4,

    [Description("删除")]
    Delete = 8,

    [Description("读写")]
    ReadWrite = Read | Write,

    [Description("完全控制")]
    FullControl = Read | Write | Execute | Delete,
}

[Flags]
public enum WorkDays
{
    [Description("无")]
    None = 0,

    [Description("周一")]
    Monday = 1,

    [Description("周二")]
    Tuesday = 2,

    [Description("周三")]
    Wednesday = 4,

    [Description("周四")]
    Thursday = 8,

    [Description("周五")]
    Friday = 16,

    [Description("周六")]
    Saturday = 32,

    [Description("周日")]
    Sunday = 64,

    [Description("工作日")]
    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,

    [Description("周末")]
    Weekend = Saturday | Sunday,

    [Description("全周")]
    AllDays = Weekdays | Weekend,
}

#endregion 枚举定义（Demo 用）