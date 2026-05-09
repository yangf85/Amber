using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class HintBoxView : UserControl
{
    public HintBoxView()
    {
        InitializeComponent();
        DataContext = new HintBoxViewModel();
    }
}

public class AuthorInfo
{
    public string Name { get; set; }

    public string Country { get; set; }
}

public class BookInfo
{
    public string Id { get; set; }

    public string Title { get; set; }

    public string Isbn { get; set; }

    public AuthorInfo Author { get; set; }

    public override string ToString() => $"{Title} ({Id})";
}

public partial class HintBoxViewModel : ObservableObject
{
    public ObservableCollection<string> Languages { get; } = new();

    public ObservableCollection<BookInfo> Books { get; } = new();

    public ObservableCollection<string> SharedFruits { get; } = new();

    public ObservableCollection<string> DynamicItems { get; } = new();

    public ObservableCollection<string> ComparisonItems { get; } = new();

    [ObservableProperty]
    public partial string SelectedLanguage { get; set; }

    [ObservableProperty]
    public partial BookInfo SelectedByIsbn { get; set; }

    [ObservableProperty]
    public partial BookInfo SelectedByAuthor { get; set; }

    [ObservableProperty]
    public partial BookInfo SelectedDefaultBook { get; set; }

    [ObservableProperty]
    public partial string DynamicSelected { get; set; }

    [ObservableProperty]
    public partial StringComparison Comparison { get; set; } = StringComparison.OrdinalIgnoreCase;

    public HintBoxViewModel()
    {
        InitLanguages();
        InitBooks();
        InitShared();
        InitDynamic();
        InitComparison();
    }

    #region 初始数据

    private void InitLanguages()
    {
        var list = new[]
        {
            "C", "C++", "C#", "Java", "JavaScript", "TypeScript", "Python",
            "Go", "Rust", "Swift", "Kotlin", "Ruby", "PHP", "Lua", "Scala",
            "Haskell", "F#", "Dart", "Julia", "Elixir",
        };
        foreach (var item in list)
        {
            Languages.Add(item);
        }
    }

    private void InitBooks()
    {
        var liu = new AuthorInfo { Name = "刘慈欣", Country = "中国" };
        var yu = new AuthorInfo { Name = "余华", Country = "中国" };
        var marquez = new AuthorInfo { Name = "马尔克斯", Country = "哥伦比亚" };
        var orwell = new AuthorInfo { Name = "乔治·奥威尔", Country = "英国" };
        var calvino = new AuthorInfo { Name = "卡尔维诺", Country = "意大利" };
        var murakami = new AuthorInfo { Name = "村上春树", Country = "日本" };

        Books.Add(new BookInfo { Id = "B001", Title = "三体", Isbn = "978-7-5366-9293-0", Author = liu });
        Books.Add(new BookInfo { Id = "B002", Title = "球状闪电", Isbn = "978-7-5366-7456-1", Author = liu });
        Books.Add(new BookInfo { Id = "B003", Title = "活着", Isbn = "978-7-5063-7937-1", Author = yu });
        Books.Add(new BookInfo { Id = "B004", Title = "兄弟", Isbn = "978-7-5063-3592-6", Author = yu });
        Books.Add(new BookInfo { Id = "B005", Title = "百年孤独", Isbn = "978-7-5447-1027-2", Author = marquez });
        Books.Add(new BookInfo { Id = "B006", Title = "1984", Isbn = "978-7-5447-2031-8", Author = orwell });
        Books.Add(new BookInfo { Id = "B007", Title = "看不见的城市", Isbn = "978-7-5447-3158-1", Author = calvino });
        Books.Add(new BookInfo { Id = "B008", Title = "挪威的森林", Isbn = "978-7-5447-1052-4", Author = murakami });
    }

    private void InitShared()
    {
        var fruits = new[] { "Apple", "Apricot", "Avocado", "Banana", "Blueberry", "Cherry", "Coconut", "Date", "Mango", "Orange", "Peach", "Pear" };
        foreach (var f in fruits)
        {
            SharedFruits.Add(f);
        }
    }

    private void InitDynamic()
    {
        DynamicItems.Add("项目-001");
        DynamicItems.Add("项目-002");
        DynamicItems.Add("项目-003");
    }

    private void InitComparison()
    {
        // 故意混合大小写——便于测试不同 StringComparison 模式
        var list = new[] { "Apple", "APPLE", "apple", "Application", "BANANA", "banana", "Cherry", "CHERRY", "Watermelon" };
        foreach (var item in list)
        {
            ComparisonItems.Add(item);
        }
    }

    #endregion 初始数据

    #region Dynamic 卡片命令

    [RelayCommand]
    private void AddRandomItem()
    {
        var n = DynamicItems.Count + 1;
        DynamicItems.Add($"项目-{n:D3}-{Random.Shared.Next(100, 999)}");
    }

    [RelayCommand]
    private void RemoveSelectedDynamic()
    {
        if (!string.IsNullOrEmpty(DynamicSelected) && DynamicItems.Contains(DynamicSelected))
        {
            DynamicItems.Remove(DynamicSelected);
        }
    }

    [RelayCommand]
    private void ClearDynamic()
    {
        DynamicItems.Clear();
    }

    #endregion Dynamic 卡片命令

    #region StringComparison RadioButton 联动

    public bool IsCmpOrdinalIgnoreCase
    {
        get => Comparison == StringComparison.OrdinalIgnoreCase;
        set { if (value) { Comparison = StringComparison.OrdinalIgnoreCase; } }
    }

    public bool IsCmpOrdinal
    {
        get => Comparison == StringComparison.Ordinal;
        set { if (value) { Comparison = StringComparison.Ordinal; } }
    }

    public bool IsCmpCurrentCultureIgnoreCase
    {
        get => Comparison == StringComparison.CurrentCultureIgnoreCase;
        set { if (value) { Comparison = StringComparison.CurrentCultureIgnoreCase; } }
    }

    public bool IsCmpInvariantCulture
    {
        get => Comparison == StringComparison.InvariantCulture;
        set { if (value) { Comparison = StringComparison.InvariantCulture; } }
    }

    partial void OnComparisonChanged(StringComparison value)
    {
        OnPropertyChanged(nameof(IsCmpOrdinalIgnoreCase));
        OnPropertyChanged(nameof(IsCmpOrdinal));
        OnPropertyChanged(nameof(IsCmpCurrentCultureIgnoreCase));
        OnPropertyChanged(nameof(IsCmpInvariantCulture));
    }

    #endregion StringComparison RadioButton 联动
}