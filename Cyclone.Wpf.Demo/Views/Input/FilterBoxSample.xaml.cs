using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cyclone.Wpf.Demo.Views;

public partial class FilterBoxSample : UserControl
{
    public FilterBoxSample()
    {
        InitializeComponent();
        DataContext = new FilterBoxViewModel();
    }
}

public sealed record FilterProduct(string Name, string Category, double Price, int Stock, double Rating);

public partial class FilterBoxViewModel : ObservableObject
{
    // ① 基础用法 - 数字 + 文本对照
    [ObservableProperty]
    public partial bool BasicNumberIsActive { get; set; }

    [ObservableProperty]
    public partial NumberOperator BasicNumberOperator { get; set; }

    [ObservableProperty]
    public partial double BasicNumberValue { get; set; }

    [ObservableProperty]
    public partial bool BasicTextIsActive { get; set; }

    [ObservableProperty]
    public partial TextOperator BasicTextOperator { get; set; }

    [ObservableProperty]
    public partial string BasicTextValue { get; set; }

    // ② 操作符全集
    [ObservableProperty]
    public partial NumberOperator NumOperatorOperator { get; set; }

    [ObservableProperty]
    public partial double NumOperatorValue { get; set; }

    [ObservableProperty]
    public partial TextOperator TextOperatorOperator { get; set; }

    [ObservableProperty]
    public partial string TextOperatorValue { get; set; }

    // ③ Decimal 模式 + Tolerance
    [ObservableProperty]
    public partial bool DecimalIsActive { get; set; }

    [ObservableProperty]
    public partial NumberOperator DecimalOperator { get; set; }

    [ObservableProperty]
    public partial double DecimalValue { get; set; }

    [ObservableProperty]
    public partial double DecimalTolerance { get; set; }

    // ④ IsCaseSensitive 对照
    [ObservableProperty]
    public partial bool CaseInsensitiveActive { get; set; }

    [ObservableProperty]
    public partial string CaseInsensitiveText { get; set; }

    [ObservableProperty]
    public partial bool CaseSensitiveActive { get; set; }

    [ObservableProperty]
    public partial string CaseSensitiveText { get; set; }

    // ⑤ Regex
    [ObservableProperty]
    public partial bool RegexIsActive { get; set; }

    [ObservableProperty]
    public partial string RegexText { get; set; }

    // ⑥ MVVM 联合过滤
    public ObservableCollection<FilterProduct> Products { get; }

    public ICollectionView ProductsView { get; }

    [ObservableProperty]
    public partial bool PriceFilterActive { get; set; }

    [ObservableProperty]
    public partial NumberOperator PriceFilterOperator { get; set; }

    [ObservableProperty]
    public partial double PriceFilterValue { get; set; }

    [ObservableProperty]
    public partial bool NameFilterActive { get; set; }

    [ObservableProperty]
    public partial TextOperator NameFilterOperator { get; set; }

    [ObservableProperty]
    public partial string NameFilterText { get; set; }

    [ObservableProperty]
    public partial bool NameFilterCaseSensitive { get; set; }

    public string BasicNumberSummary => BasicNumberIsActive
        ? $"x {DescOf(BasicNumberOperator)} {BasicNumberValue:F0}"
        : "未启用";

    public string BasicTextSummary => BasicTextIsActive
        ? $"s {DescOf(BasicTextOperator)} \"{BasicTextValue}\""
        : "未启用";

    public string NumOperatorSummary => $"x {DescOf(NumOperatorOperator)} {NumOperatorValue:F0}";

    public string TextOperatorSummary => $"s {DescOf(TextOperatorOperator)} \"{TextOperatorValue}\"";

    public string DecimalSummary => DecimalIsActive
        ? $"|x − {DecimalValue:F2}| ≤ {DecimalTolerance:F3}"
        : "未启用";

    public int CaseInsensitiveMatchCount => CaseInsensitiveActive
        ? Products.Count(p => (p.Name ?? "").IndexOf(CaseInsensitiveText ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
        : Products.Count;

    public int CaseSensitiveMatchCount => CaseSensitiveActive
        ? Products.Count(p => (p.Name ?? "").IndexOf(CaseSensitiveText ?? "", StringComparison.Ordinal) >= 0)
        : Products.Count;

    public int RegexMatchCount
    {
        get
        {
            if (!RegexIsActive || string.IsNullOrEmpty(RegexText))
            {
                return Products.Count;
            }
            try
            {
                var regex = new Regex(RegexText);
                return Products.Count(p => regex.IsMatch(p.Name ?? ""));
            }
            catch (ArgumentException)
            {
                return 0;
            }
        }
    }

    private static Func<string, bool> BuildRegexPredicate(string pattern, bool caseSensitive)
    {
        var options = RegexOptions.CultureInvariant;
        if (!caseSensitive)
        {
            options |= RegexOptions.IgnoreCase;
        }
        try
        {
            var regex = new Regex(pattern, options);
            return s => regex.IsMatch(s ?? "");
        }
        catch (ArgumentException)
        {
            return static _ => false;
        }
    }

    /// <summary>
    /// 读取枚举值的 [Description] attribute,失败时回退到枚举名。
    /// 单一真相源:符号显示完全跟随枚举定义,demo 不维护重复映射。
    /// </summary>
    private static string DescOf<TEnum>(TEnum value) where TEnum : Enum
        => typeof(TEnum).GetField(value.ToString())
                        ?.GetCustomAttribute<DescriptionAttribute>()
                        ?.Description ?? value.ToString();

    private bool FilterProductPredicate(object item)
    {
        if (item is not FilterProduct product)
        {
            return false;
        }
        var pricePred = BuildPricePredicate();
        var namePred = BuildNamePredicate();
        return pricePred(product.Price) && namePred(product.Name);
    }

    private Func<double, bool> BuildPricePredicate()
    {
        if (!PriceFilterActive)
        {
            return static _ => true;
        }
        var target = PriceFilterValue;
        return PriceFilterOperator switch
        {
            NumberOperator.Equal => x => Math.Abs(x - target) <= 1e-9,
            NumberOperator.NotEqual => x => Math.Abs(x - target) > 1e-9,
            NumberOperator.LessThan => x => x < target,
            NumberOperator.LessThanOrEqual => x => x <= target,
            NumberOperator.GreaterThan => x => x > target,
            NumberOperator.GreaterThanOrEqual => x => x >= target,
            _ => static _ => true,
        };
    }

    private Func<string, bool> BuildNamePredicate()
    {
        if (!NameFilterActive || string.IsNullOrEmpty(NameFilterText))
        {
            return static _ => true;
        }
        var pattern = NameFilterText;
        var cmp = NameFilterCaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        return NameFilterOperator switch
        {
            TextOperator.Equal => s => string.Equals(s ?? "", pattern, cmp),
            TextOperator.NotEqual => s => !string.Equals(s ?? "", pattern, cmp),
            TextOperator.Contains => s => (s ?? "").Contains(pattern, cmp),
            TextOperator.NotContains => s => (s ?? "").IndexOf(pattern, cmp) < 0,
            TextOperator.StartsWith => s => (s ?? "").StartsWith(pattern, cmp),
            TextOperator.EndsWith => s => (s ?? "").EndsWith(pattern, cmp),
            TextOperator.Regex => BuildRegexPredicate(pattern, NameFilterCaseSensitive),
            _ => static _ => true,
        };
    }

    partial void OnPriceFilterActiveChanged(bool value) => ProductsView?.Refresh();

    partial void OnPriceFilterOperatorChanged(NumberOperator value) => ProductsView?.Refresh();

    partial void OnPriceFilterValueChanged(double value) => ProductsView?.Refresh();

    partial void OnNameFilterActiveChanged(bool value) => ProductsView?.Refresh();

    partial void OnNameFilterOperatorChanged(TextOperator value) => ProductsView?.Refresh();

    partial void OnNameFilterTextChanged(string value) => ProductsView?.Refresh();

    partial void OnNameFilterCaseSensitiveChanged(bool value) => ProductsView?.Refresh();

    partial void OnBasicNumberIsActiveChanged(bool value) => OnPropertyChanged(nameof(BasicNumberSummary));

    partial void OnBasicNumberOperatorChanged(NumberOperator value) => OnPropertyChanged(nameof(BasicNumberSummary));

    partial void OnBasicNumberValueChanged(double value) => OnPropertyChanged(nameof(BasicNumberSummary));

    partial void OnBasicTextIsActiveChanged(bool value) => OnPropertyChanged(nameof(BasicTextSummary));

    partial void OnBasicTextOperatorChanged(TextOperator value) => OnPropertyChanged(nameof(BasicTextSummary));

    partial void OnBasicTextValueChanged(string value) => OnPropertyChanged(nameof(BasicTextSummary));

    partial void OnNumOperatorOperatorChanged(NumberOperator value) => OnPropertyChanged(nameof(NumOperatorSummary));

    partial void OnNumOperatorValueChanged(double value) => OnPropertyChanged(nameof(NumOperatorSummary));

    partial void OnTextOperatorOperatorChanged(TextOperator value) => OnPropertyChanged(nameof(TextOperatorSummary));

    partial void OnTextOperatorValueChanged(string value) => OnPropertyChanged(nameof(TextOperatorSummary));

    partial void OnDecimalIsActiveChanged(bool value) => OnPropertyChanged(nameof(DecimalSummary));

    partial void OnDecimalOperatorChanged(NumberOperator value) => OnPropertyChanged(nameof(DecimalSummary));

    partial void OnDecimalValueChanged(double value) => OnPropertyChanged(nameof(DecimalSummary));

    partial void OnDecimalToleranceChanged(double value) => OnPropertyChanged(nameof(DecimalSummary));

    partial void OnCaseInsensitiveActiveChanged(bool value) => OnPropertyChanged(nameof(CaseInsensitiveMatchCount));

    partial void OnCaseInsensitiveTextChanged(string value) => OnPropertyChanged(nameof(CaseInsensitiveMatchCount));

    partial void OnCaseSensitiveActiveChanged(bool value) => OnPropertyChanged(nameof(CaseSensitiveMatchCount));

    partial void OnCaseSensitiveTextChanged(string value) => OnPropertyChanged(nameof(CaseSensitiveMatchCount));

    partial void OnRegexIsActiveChanged(bool value) => OnPropertyChanged(nameof(RegexMatchCount));

    partial void OnRegexTextChanged(string value) => OnPropertyChanged(nameof(RegexMatchCount));

    public FilterBoxViewModel()
    {
        // ① ~ ⑤ 与列表无关的展示属性,先全部赋默认值
        BasicNumberOperator = NumberOperator.GreaterThanOrEqual;
        BasicNumberValue = 50;

        BasicTextOperator = TextOperator.Contains;
        BasicTextValue = "abc";

        NumOperatorOperator = NumberOperator.Equal;
        NumOperatorValue = 100;

        TextOperatorOperator = TextOperator.StartsWith;
        TextOperatorValue = string.Empty;

        DecimalIsActive = true;
        DecimalOperator = NumberOperator.Equal;
        DecimalValue = 3.14;
        DecimalTolerance = 0.01;

        CaseInsensitiveActive = true;
        CaseInsensitiveText = "Pro";

        CaseSensitiveActive = true;
        CaseSensitiveText = "Pro";

        RegexIsActive = true;
        RegexText = @"^[A-Z]\w+$";

        // ⑥ 联合过滤:必须先建数据 + ICollectionView,再赋值过滤参数,
        // 否则 OnXxxChanged partial 钩子里 ProductsView 仍为 null,导致 NRE。
        Products = [with(new[]
        {
            new FilterProduct("Wireless Headphones Pro", "音频", 199, 42, 4.5),
            new FilterProduct("Mechanical Keyboard", "外设", 599, 18, 4.7),
            new FilterProduct("Gaming Mouse", "外设", 299, 65, 4.3),
            new FilterProduct("USB Hub", "配件", 79, 120, 4.0),
            new FilterProduct("4K Monitor Pro", "显示", 2499, 8, 4.8),
            new FilterProduct("Ergonomic Chair", "家具", 1899, 5, 4.6),
            new FilterProduct("HDMI Cable", "配件", 39, 230, 4.1),
            new FilterProduct("Portable SSD", "存储", 449, 33, 4.4),
            new FilterProduct("Webcam HD", "音频", 159, 27, 3.9),
            new FilterProduct("Studio Microphone", "音频", 899, 14, 4.5),
            new FilterProduct("Standing Desk", "家具", 1299, 12, 4.4),
            new FilterProduct("Cable Organizer", "配件", 29, 350, 3.8),
        })];

        ProductsView = CollectionViewSource.GetDefaultView(Products);
        ProductsView.Filter = FilterProductPredicate;

        // 此时 ProductsView 已就绪,设过滤参数会自然触发一次正确的 Refresh
        PriceFilterActive = true;
        PriceFilterOperator = NumberOperator.LessThanOrEqual;
        PriceFilterValue = 1000;

        NameFilterActive = false;
        NameFilterOperator = TextOperator.Contains;
        NameFilterText = string.Empty;
    }
}