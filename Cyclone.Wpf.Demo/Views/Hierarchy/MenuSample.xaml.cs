using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// MenuView.xaml 的交互逻辑
/// </summary>
public partial class MenuSample : UserControl
{
    public MenuSample()
    {
        InitializeComponent();
        DataContext = new MenuViewModel();
    }
}

public partial class MenuViewModel : ObservableObject
{
    public MenuViewModel()
    {
        // 初始化数据
        Items = new ObservableCollection<string>
        {
            "项目 1", "项目 2", "项目 3", "项目 4"
        };

        // 监听属性变化
        PropertyChanged += MenuViewModel_PropertyChanged;
    }

    #region 基本属性

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LastMenuAction))]
    public partial string LastMenuAction { get; set; } = "等待菜单操作...";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LastContextAction))]
    public partial string LastContextAction { get; set; } = "等待右键菜单操作...";

    [ObservableProperty]
    public partial string SampleText { get; set; } = "这是一个示例文本框\n可以编辑内容\n右键查看编辑菜单";

    #endregion 基本属性

    #region 视图设置

    [ObservableProperty]
    public partial bool ShowToolbar { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowStatusBar { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowSidebar { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLightTheme), nameof(IsDarkTheme), nameof(IsAutoTheme))]
    public partial string CurrentTheme { get; set; } = "Light";

    public bool IsLightTheme => CurrentTheme == "Light";
    public bool IsDarkTheme => CurrentTheme == "Dark";
    public bool IsAutoTheme => CurrentTheme == "Auto";

    #endregion 视图设置

    #region 文件操作

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRecentFiles))]
    public partial string RecentFile1 { get; set; } = "Document1.txt";

    [ObservableProperty]
    public partial string RecentFile2 { get; set; } = "Project.json";

    [ObservableProperty]
    public partial string RecentFile3 { get; set; } = "Settings.xml";

    public bool HasRecentFiles => !string.IsNullOrEmpty(RecentFile1);

    [ObservableProperty]
    public partial bool CanSave { get; set; } = true;

    [ObservableProperty]
    public partial bool CanUndo { get; set; } = true;

    [ObservableProperty]
    public partial bool CanRedo { get; set; } = false;

    #endregion 文件操作

    #region 权限控制

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEdit), nameof(CanDelete), nameof(AdminMenuVisibility))]
    public partial bool IsAdminMode { get; set; } = false;

    public bool CanEdit => IsAdminMode || true; // 普通用户也可以编辑
    public bool CanDelete => IsAdminMode; // 只有管理员可以删除

    public Visibility AdminMenuVisibility => IsAdminMode ? Visibility.Visible : Visibility.Collapsed;

    #endregion 权限控制

    #region 列表操作

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedItem), nameof(CanMoveUp), nameof(CanMoveDown))]
    public partial string SelectedItem { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasItems))]
    public partial ObservableCollection<string> Items { get; set; }

    public bool HasSelectedItem => !string.IsNullOrEmpty(SelectedItem);
    public bool HasItems => Items?.Count > 0;
    public bool CanMoveUp => HasSelectedItem && Items.IndexOf(SelectedItem) > 0;
    public bool CanMoveDown => HasSelectedItem && Items.IndexOf(SelectedItem) < Items.Count - 1;

    #endregion 列表操作

    #region 文件菜单命令

    [RelayCommand]
    private void NewFile()
    {
        LastMenuAction = "新建文件";
        NotificationService.Instance.Information("创建新文件");
    }

    [RelayCommand]
    private void NewProject()
    {
        LastMenuAction = "新建项目";
        NotificationService.Instance.Information("创建新项目");
    }

    [RelayCommand]
    private void NewFolder()
    {
        LastMenuAction = "新建文件夹";
        NotificationService.Instance.Information("创建新文件夹");
    }

    [RelayCommand]
    private void Open()
    {
        LastMenuAction = "打开文件";
        NotificationService.Instance.Information("打开文件对话框");
    }

    [RelayCommand]
    private void OpenRecent(string fileName)
    {
        LastMenuAction = $"打开最近文件: {fileName}";
        NotificationService.Instance.Information($"打开最近文件: {fileName}");
    }

    [RelayCommand]
    private void ClearRecent()
    {
        RecentFile1 = "";
        RecentFile2 = "";
        RecentFile3 = "";
        LastMenuAction = "清除最近文件列表";
        NotificationService.Instance.Success("已清除最近文件列表");
    }

    [RelayCommand]
    private void Save()
    {
        LastMenuAction = "保存文件";
        NotificationService.Instance.Success("文件已保存");
    }

    [RelayCommand]
    private void SaveAs()
    {
        LastMenuAction = "另存为";
        NotificationService.Instance.Information("另存为对话框");
    }

    [RelayCommand]
    private void Exit()
    {
        LastMenuAction = "退出应用程序";
        NotificationService.Instance.Warning("退出应用程序");
    }

    #endregion 文件菜单命令

    #region 编辑菜单命令

    [RelayCommand]
    private void Undo()
    {
        LastMenuAction = "撤销操作";
        CanUndo = false;
        CanRedo = true;
        NotificationService.Instance.Information("已撤销");
    }

    [RelayCommand]
    private void Redo()
    {
        LastMenuAction = "重做操作";
        CanUndo = true;
        CanRedo = false;
        NotificationService.Instance.Information("已重做");
    }

    [RelayCommand]
    private void Cut()
    {
        LastMenuAction = "剪切";
        NotificationService.Instance.Information("已剪切到剪贴板");
    }

    [RelayCommand]
    private void Copy()
    {
        LastMenuAction = "复制";
        NotificationService.Instance.Information("已复制到剪贴板");
    }

    [RelayCommand]
    private void Paste()
    {
        LastMenuAction = "粘贴";
        NotificationService.Instance.Information("已从剪贴板粘贴");
    }

    [RelayCommand]
    private void SelectAll()
    {
        LastMenuAction = "全选";
        NotificationService.Instance.Information("已全选");
    }

    [RelayCommand]
    private void Find()
    {
        LastMenuAction = "查找";
        NotificationService.Instance.Information("打开查找对话框");
    }

    [RelayCommand]
    private void Replace()
    {
        LastMenuAction = "替换";
        NotificationService.Instance.Information("打开替换对话框");
    }

    #endregion 编辑菜单命令

    #region 视图菜单命令

    [RelayCommand]
    private void ZoomIn()
    {
        LastMenuAction = "放大";
        NotificationService.Instance.Information("视图已放大");
    }

    [RelayCommand]
    private void ZoomOut()
    {
        LastMenuAction = "缩小";
        NotificationService.Instance.Information("视图已缩小");
    }

    [RelayCommand]
    private void ZoomReset()
    {
        LastMenuAction = "重置缩放";
        NotificationService.Instance.Information("缩放已重置为100%");
    }

    [RelayCommand]
    private void FitToWindow()
    {
        LastMenuAction = "适合窗口";
        NotificationService.Instance.Information("视图已适合窗口大小");
    }

    [RelayCommand]
    private void SetLightTheme()
    {
        CurrentTheme = "Light";
        LastMenuAction = "切换到浅色主题";
        NotificationService.Instance.Information("已切换到浅色主题");
    }

    [RelayCommand]
    private void SetDarkTheme()
    {
        CurrentTheme = "Dark";
        LastMenuAction = "切换到深色主题";
        NotificationService.Instance.Information("已切换到深色主题");
    }

    [RelayCommand]
    private void SetAutoTheme()
    {
        CurrentTheme = "Auto";
        LastMenuAction = "切换到自动主题";
        NotificationService.Instance.Information("已切换到自动主题");
    }

    #endregion 视图菜单命令

    #region 工具菜单命令

    [RelayCommand]
    private void Options()
    {
        LastMenuAction = "打开选项";
        NotificationService.Instance.Information("打开选项对话框");
    }

    [RelayCommand]
    private void Customize()
    {
        LastMenuAction = "自定义界面";
        NotificationService.Instance.Information("打开自定义对话框");
    }

    [RelayCommand]
    private void PluginManager()
    {
        LastMenuAction = "插件管理器";
        NotificationService.Instance.Information("打开插件管理器");
    }

    #endregion 工具菜单命令

    #region 帮助菜单命令

    [RelayCommand]
    private void Documentation()
    {
        LastMenuAction = "打开文档";
        NotificationService.Instance.Information("打开在线文档");
    }

    [RelayCommand]
    private void Shortcuts()
    {
        LastMenuAction = "快捷键列表";
        NotificationService.Instance.Information("显示快捷键列表");
    }

    [RelayCommand]
    private void CheckUpdates()
    {
        LastMenuAction = "检查更新";
        NotificationService.Instance.Information("正在检查更新...");
    }

    [RelayCommand]
    private void About()
    {
        LastMenuAction = "关于";
        NotificationService.Instance.Information("显示关于对话框");
    }

    #endregion 帮助菜单命令

    #region 右键菜单命令

    [RelayCommand]
    private void OpenFile()
    {
        LastContextAction = "打开文件";
        NotificationService.Instance.Information("打开选中文件");
    }

    [RelayCommand]
    private void EditFile()
    {
        LastContextAction = "编辑文件";
        NotificationService.Instance.Information("编辑选中文件");
    }

    [RelayCommand]
    private void CopyFile()
    {
        LastContextAction = "复制文件";
        NotificationService.Instance.Information("文件已复制");
    }

    [RelayCommand]
    private void CutFile()
    {
        LastContextAction = "剪切文件";
        NotificationService.Instance.Information("文件已剪切");
    }

    [RelayCommand]
    private void DeleteFile()
    {
        LastContextAction = "删除文件";
        NotificationService.Instance.Warning("文件已删除");
    }

    [RelayCommand]
    private void RenameFile()
    {
        LastContextAction = "重命名文件";
        NotificationService.Instance.Information("进入重命名模式");
    }

    [RelayCommand]
    private void FileProperties()
    {
        LastContextAction = "文件属性";
        NotificationService.Instance.Information("显示文件属性");
    }

    [RelayCommand]
    private void View()
    {
        LastContextAction = "查看";
        NotificationService.Instance.Information("查看项目");
    }

    [RelayCommand]
    private void Edit()
    {
        LastContextAction = "编辑";
        NotificationService.Instance.Information("编辑项目");
    }

    [RelayCommand]
    private void Delete()
    {
        LastContextAction = "删除";
        NotificationService.Instance.Warning("删除项目");
    }

    [RelayCommand]
    private void SystemSettings()
    {
        LastContextAction = "系统设置";
        NotificationService.Instance.Information("打开系统设置");
    }

    [RelayCommand]
    private void UserManagement()
    {
        LastContextAction = "用户管理";
        NotificationService.Instance.Information("打开用户管理");
    }

    [RelayCommand]
    private void ViewLogs()
    {
        LastContextAction = "查看日志";
        NotificationService.Instance.Information("打开日志查看器");
    }

    #endregion 右键菜单命令

    #region 列表操作命令

    [RelayCommand]
    private void AddItem()
    {
        var newItem = $"新项目 {Items.Count + 1}";
        Items.Add(newItem);
        LastContextAction = $"添加项目: {newItem}";
        NotificationService.Instance.Success($"已添加: {newItem}");
    }

    [RelayCommand]
    private void EditItem()
    {
        if (HasSelectedItem)
        {
            LastContextAction = $"编辑项目: {SelectedItem}";
            NotificationService.Instance.Information($"编辑: {SelectedItem}");
        }
    }

    [RelayCommand]
    private void DeleteItem()
    {
        if (HasSelectedItem)
        {
            var item = SelectedItem;
            Items.Remove(item);
            LastContextAction = $"删除项目: {item}";
            NotificationService.Instance.Warning($"已删除: {item}");
        }
    }

    [RelayCommand]
    private void MoveUp()
    {
        if (CanMoveUp)
        {
            var index = Items.IndexOf(SelectedItem);
            Items.Move(index, index - 1);
            LastContextAction = $"上移项目: {SelectedItem}";
            NotificationService.Instance.Information("项目已上移");
        }
    }

    [RelayCommand]
    private void MoveDown()
    {
        if (CanMoveDown)
        {
            var index = Items.IndexOf(SelectedItem);
            Items.Move(index, index + 1);
            LastContextAction = $"下移项目: {SelectedItem}";
            NotificationService.Instance.Information("项目已下移");
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        Items.Clear();
        LastContextAction = "清除所有项目";
        NotificationService.Instance.Success("已清除所有项目");
    }

    #endregion 列表操作命令

    #region 事件处理

    private void MenuViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // 当选中项变化时，更新相关计算属性
        if (e.PropertyName == nameof(SelectedItem) || e.PropertyName == nameof(Items))
        {
            OnPropertyChanged(nameof(HasSelectedItem));
            OnPropertyChanged(nameof(CanMoveUp));
            OnPropertyChanged(nameof(CanMoveDown));
            OnPropertyChanged(nameof(HasItems));
        }
    }

    #endregion 事件处理
}