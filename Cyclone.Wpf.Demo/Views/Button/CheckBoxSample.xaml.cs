using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class CheckBoxSample : UserControl
{
    public CheckBoxSample()
    {
        InitializeComponent();
        DataContext = new CheckBoxViewModel();
    }
}

public partial class CheckBoxViewModel : ObservableObject
{
    // ① 基础勾选
    [ObservableProperty]
    public partial bool AcceptTerms { get; set; }

    [ObservableProperty]
    public partial bool SubscribeNewsletter { get; set; } = true;

    [ObservableProperty]
    public partial bool RememberMe { get; set; }

    // ② 三态父子联动
    public ObservableCollection<TodoItem> Todos { get; } = new()
    {
        new TodoItem { Title = "完成 WPF 控件库重构" },
        new TodoItem { Title = "写完所有 Sample" },
        new TodoItem { Title = "整理 Generic.xaml" },
        new TodoItem { Title = "更新 README" },
        new TodoItem { Title = "发布 nuget 包" },
    };

    // 父级 - 三态显示
    [ObservableProperty]
    public partial bool? ParentChecked { get; set; }

    // 计数
    public int CheckedCount => Todos.Count(t => t.IsDone);

    public int TotalCount => Todos.Count;

    public CheckBoxViewModel()
    {
        // 初始 2 个完成
        Todos[0].IsDone = true;
        Todos[1].IsDone = true;
        RecomputeParent();

        foreach (var todo in Todos)
        {
            todo.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TodoItem.IsDone))
                {
                    RecomputeParent();
                    OnPropertyChanged(nameof(CheckedCount));
                }
            };
        }
    }

    private bool _suppressChildSync;

    partial void OnParentCheckedChanged(bool? value)
    {
        // 防止父级 → 子级反向同步触发循环
        if (_suppressChildSync || value is null) return;

        foreach (var todo in Todos)
        {
            todo.IsDone = value.Value;
        }
    }

    private void RecomputeParent()
    {
        int done = CheckedCount;
        bool? newParent = done == 0 ? false
                        : done == TotalCount ? true
                        : (bool?)null;

        if (ParentChecked != newParent)
        {
            _suppressChildSync = true;
            ParentChecked = newParent;
            _suppressChildSync = false;
        }
    }

    [RelayCommand]
    private void CheckAll()
    {
        foreach (var todo in Todos) todo.IsDone = true;
    }

    [RelayCommand]
    private void UncheckAll()
    {
        foreach (var todo in Todos) todo.IsDone = false;
    }
}

public partial class TodoItem : ObservableObject
{
    [ObservableProperty]
    public partial bool IsDone { get; set; }

    public string Title { get; set; }
}
