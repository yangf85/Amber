using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Demo.Helper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cyclone.Wpf.Demo.Views;

public partial class ListViewView : UserControl
{
    /// <summary>
    /// 章节③：点击列头排序——code-behind 监听 GridViewColumnHeader 的 Click，
    /// 通过列头的 Tag（属性名）切换 ICollectionView.SortDescriptions。
    /// 这是 WPF 列头排序的标准做法，没有内建支持。
    /// </summary>
    private GridViewColumnHeader? _lastSortHeader;

    private ListSortDirection _lastSortDirection = ListSortDirection.Ascending;

    private void OnSortHeaderClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader header || header.Role == GridViewColumnHeaderRole.Padding)
        {
            return;
        }

        if (header.Column?.DisplayMemberBinding is not Binding binding)
        {
            // CellTemplate 列没有 DisplayMemberBinding——靠列头 Tag 取属性名
            if (header.Tag is not string sortPath)
            {
                return;
            }
            ApplySort(sortPath, header);
            return;
        }

        ApplySort(binding.Path.Path, header);
    }

    private void ApplySort(string sortPath, GridViewColumnHeader header)
    {
        if (DataContext is not ListViewDemoViewModel vm)
        {
            return;
        }

        // 同一列再次点击：切换升降序；不同列：重置为升序
        ListSortDirection direction;
        if (_lastSortHeader == header)
        {
            direction = _lastSortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }
        else
        {
            direction = ListSortDirection.Ascending;
        }

        vm.SortableView.SortDescriptions.Clear();
        vm.SortableView.SortDescriptions.Add(new SortDescription(sortPath, direction));

        _lastSortHeader = header;
        _lastSortDirection = direction;
        vm.CurrentSortLabel = $"{sortPath} {(direction == ListSortDirection.Ascending ? "↑" : "↓")}";
    }

    public ListViewView()
    {
        InitializeComponent();
        DataContext = new ListViewDemoViewModel();
    }
}

public partial class ListViewDemoViewModel : ObservableObject
{
    // ===== 章节 ③ 列头排序 =====
    private readonly ObservableCollection<FakerData> _sortableSource;

    // ===== 章节 ① 基础 GridView =====
    public ObservableCollection<FakerData> BasicData { get; }

    [ObservableProperty]
    public partial FakerData? SelectedBasicPerson { get; set; }

    // ===== 章节 ② CellTemplate =====
    public ObservableCollection<FakerData> TemplateData { get; }

    public ICollectionView SortableView { get; }

    [ObservableProperty]
    public partial string CurrentSortLabel { get; set; } = "（点击列头排序）";

    // ===== 章节 ④ 列宽演示 =====
    public ObservableCollection<FakerData> ColumnWidthData { get; }

    // ===== 章节 ⑤ 无 GridView 模式（fancy 列表）=====
    public ObservableCollection<FakerData> PlainListData { get; }

    [ObservableProperty]
    public partial FakerData? SelectedPlainPerson { get; set; }

    public ListViewDemoViewModel()
    {
        var pool = FakerDataHelper.GenerateFakerDataCollection(40);

        BasicData = new ObservableCollection<FakerData>(pool.Take(8));
        TemplateData = new ObservableCollection<FakerData>(pool.Skip(8).Take(6));

        _sortableSource = new ObservableCollection<FakerData>(pool.Skip(14).Take(12));
        SortableView = CollectionViewSource.GetDefaultView(_sortableSource);

        ColumnWidthData = new ObservableCollection<FakerData>(pool.Skip(26).Take(6));
        PlainListData = new ObservableCollection<FakerData>(pool.Skip(32).Take(8));
    }
}