using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// ListBox 多选 / 全选辅助附加属性。
///
/// 提供三个附加属性：
///   1. IsSelectAllEnabled (bool)        — 启用全选 CheckBox 列（UI 显示开关）
///   2. IsSelectedAll      (bool?)       — 全选状态三态：true / false / null（半选）
///   3. SelectedItems      (IList)       — 单向绑定多选项容器到 ViewModel；helper 内部双向同步集合内容
///
/// 设计要点：
///   · IsSelectAllEnabled 是 SelectionChanged 事件订阅的入口——切到 true 时才挂事件
///   · 用 IsUpdating 附加属性做重入抑制，避免三方互相触发的循环
///   · SelectedItems 的 binding 是 OneWay 语义（VM → View 传集合引用即可，VM 端 getter-only 属性也合法）
///     真正的"双向同步"通过监听集合的 INotifyCollectionChanged 实现——
///     推荐 ObservableCollection 类型，helper 监听 CollectionChanged 同步两端
///   · 订阅 INPC 用闭包 capture ListBox 引用，handler 实例存附加属性供解绑用
///
/// 为什么 SelectedItems 不用 BindsTwoWayByDefault？
///   集合类绑定的语义是"共享同一个集合实例"，不是"赋值整个集合引用"——
///   TwoWay 会要求 VM 属性有 setter，强制用户写不必要的 set 方法。
///   OneWay 时 VM 的 ObservableCollection 用 getter-only 属性即可（更符合 C# 现代写法）。
/// </summary>
public static class ListBoxHelper
{
    #region IsSelectAllEnabled — UI 显示开关 + 事件订阅入口

    public static readonly DependencyProperty IsSelectAllEnabledProperty;

    public static bool GetIsSelectAllEnabled(DependencyObject obj) =>
        (bool)obj.GetValue(IsSelectAllEnabledProperty);

    public static void SetIsSelectAllEnabled(DependencyObject obj, bool value) =>
        obj.SetValue(IsSelectAllEnabledProperty, value);

    #endregion IsSelectAllEnabled — UI 显示开关 + 事件订阅入口

    #region IsSelectedAll — 三态全选状态

    public static readonly DependencyProperty IsSelectedAllProperty;

    public static bool? GetIsSelectedAll(DependencyObject obj) =>
        (bool?)obj.GetValue(IsSelectedAllProperty);

    public static void SetIsSelectedAll(DependencyObject obj, bool? value) =>
        obj.SetValue(IsSelectedAllProperty, value);

    #endregion IsSelectedAll — 三态全选状态

    #region SelectedItems — 双向同步多选项

    public static readonly DependencyProperty SelectedItemsProperty;

    public static IList GetSelectedItems(DependencyObject obj) =>
        (IList)obj.GetValue(SelectedItemsProperty);

    public static void SetSelectedItems(DependencyObject obj, IList value) =>
        obj.SetValue(SelectedItemsProperty, value);

    #endregion SelectedItems — 双向同步多选项

    #region 内部状态 — 重入抑制 + 订阅追踪

    /// <summary>
    /// 当前 ListBox 是否处于"程序更新中"——避免事件循环。
    /// </summary>
    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached(
            "IsUpdating",
            typeof(bool),
            typeof(ListBoxHelper),
            new PropertyMetadata(false));

    /// <summary>
    /// 当前订阅在 SelectedItems IList 上的 NotifyCollectionChanged 委托实例——
    /// 闭包 capture 了 ListBox 引用。属性值切换时用它正确解绑。
    /// </summary>
    private static readonly DependencyProperty CollectionChangedHandlerProperty =
        DependencyProperty.RegisterAttached(
            "CollectionChangedHandler",
            typeof(NotifyCollectionChangedEventHandler),
            typeof(ListBoxHelper),
            new PropertyMetadata(null));

    private static bool GetIsUpdating(DependencyObject obj) =>
        (bool)obj.GetValue(IsUpdatingProperty);

    private static void SetIsUpdating(DependencyObject obj, bool value) =>
        obj.SetValue(IsUpdatingProperty, value);

    #endregion 内部状态 — 重入抑制 + 订阅追踪

    #region 静态构造 — 集中注册 DP

    static ListBoxHelper()
    {
        IsSelectAllEnabledProperty = DependencyProperty.RegisterAttached(
            "IsSelectAllEnabled",
            typeof(bool),
            typeof(ListBoxHelper),
            new PropertyMetadata(false, OnIsSelectAllEnabledChanged));

        IsSelectedAllProperty = DependencyProperty.RegisterAttached(
            "IsSelectedAll",
            typeof(bool?),
            typeof(ListBoxHelper),
            new PropertyMetadata(false, OnIsSelectedAllChanged));

        SelectedItemsProperty = DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(ListBoxHelper),
            new PropertyMetadata(null, OnSelectedItemsChanged));
    }

    #endregion 静态构造 — 集中注册 DP

    #region 回调

    private static void OnIsSelectAllEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            EnsureSelectionChangedSubscribed(listBox);
            UpdateIsSelectedAllFromListBox(listBox);
        }
        else
        {
            // 仅当 SelectedItems 也未启用时才解绑——否则要保留事件给 SelectedItems 用
            if (GetSelectedItems(listBox) == null)
            {
                listBox.SelectionChanged -= OnListBoxSelectionChanged;
            }
        }
    }

    private static void OnIsSelectedAllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox)
        {
            return;
        }

        // 重入抑制：本次赋值如果是由 SelectionChanged 反向同步触发的，不要倒回去操作 ListBox
        if (GetIsUpdating(listBox))
        {
            return;
        }

        // 单选模式不支持全选语义——明确忽略
        if (listBox.SelectionMode == SelectionMode.Single)
        {
            return;
        }

        SetIsUpdating(listBox, true);
        try
        {
            switch (e.NewValue as bool?)
            {
                case true:
                    listBox.SelectAll();
                    break;

                case false:
                    listBox.UnselectAll();
                    break;
                    // null 由 ListBox 选择驱动，不回写
            }
        }
        finally
        {
            SetIsUpdating(listBox, false);
        }
    }

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox)
        {
            return;
        }

        // 1) 解绑旧 IList 的 INPC 订阅
        var oldHandler = listBox.GetValue(CollectionChangedHandlerProperty) as NotifyCollectionChangedEventHandler;
        if (oldHandler != null && e.OldValue is INotifyCollectionChanged oldInpc)
        {
            oldInpc.CollectionChanged -= oldHandler;
        }
        listBox.SetValue(CollectionChangedHandlerProperty, null);

        // 2) 确保 ListBox.SelectionChanged 已订阅（无论 IsSelectAllEnabled 状态）
        if (e.NewValue != null)
        {
            EnsureSelectionChangedSubscribed(listBox);
        }
        else if (!GetIsSelectAllEnabled(listBox))
        {
            // SelectedItems 设为 null 且 IsSelectAllEnabled=false——可以解绑 SelectionChanged
            listBox.SelectionChanged -= OnListBoxSelectionChanged;
            return;
        }

        // 3) 绑定新 IList 的 INPC（如果实现了）——闭包 capture ListBox
        if (e.NewValue is INotifyCollectionChanged newInpc)
        {
            // 闭包 capture——解决静态 handler 没法反查 ListBox 的问题
            NotifyCollectionChangedEventHandler handler = (sender, args) =>
                OnExternalCollectionChanged(listBox, args);
            newInpc.CollectionChanged += handler;
            listBox.SetValue(CollectionChangedHandlerProperty, handler);
        }

        // 4) 初始同步
        if (e.NewValue is IList newList && newList.Count > 0)
        {
            // IList 非空——以 IList 为准对齐 ListBox 选中
            SyncListBoxFromExternalList(listBox, newList);
        }
        else if (e.NewValue is IList emptyList)
        {
            // IList 为空——把 ListBox 当前选中同步到 IList
            SyncExternalListFromListBox(listBox, emptyList);
        }
    }

    #endregion 回调

    #region 双向同步 helpers

    private static void EnsureSelectionChangedSubscribed(ListBox listBox)
    {
        // -= 一个未订阅的 handler 是 no-op，安全；之后 += 保证只有一份订阅
        listBox.SelectionChanged -= OnListBoxSelectionChanged;
        listBox.SelectionChanged += OnListBoxSelectionChanged;
    }

    private static void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox)
        {
            return;
        }

        if (GetIsUpdating(listBox))
        {
            return;
        }

        SetIsUpdating(listBox, true);
        try
        {
            // 同步到外部 IList
            var externalList = GetSelectedItems(listBox);
            if (externalList != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    externalList.Remove(item);
                }
                foreach (var item in e.AddedItems)
                {
                    if (!externalList.Contains(item))
                    {
                        externalList.Add(item);
                    }
                }
            }

            // 更新 IsSelectedAll 三态
            UpdateIsSelectedAllValueLocked(listBox);
        }
        finally
        {
            SetIsUpdating(listBox, false);
        }
    }

    private static void OnExternalCollectionChanged(ListBox listBox, NotifyCollectionChangedEventArgs e)
    {
        if (GetIsUpdating(listBox))
        {
            return;
        }

        SetIsUpdating(listBox, true);
        try
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (!listBox.SelectedItems.Contains(item))
                            {
                                listBox.SelectedItems.Add(item);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            listBox.SelectedItems.Remove(item);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    listBox.SelectedItems.Clear();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            listBox.SelectedItems.Remove(item);
                        }
                    }
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            if (!listBox.SelectedItems.Contains(item))
                            {
                                listBox.SelectedItems.Add(item);
                            }
                        }
                    }
                    break;
            }

            UpdateIsSelectedAllValueLocked(listBox);
        }
        finally
        {
            SetIsUpdating(listBox, false);
        }
    }

    private static void SyncListBoxFromExternalList(ListBox listBox, IList externalList)
    {
        if (GetIsUpdating(listBox))
        {
            return;
        }

        SetIsUpdating(listBox, true);
        try
        {
            listBox.SelectedItems.Clear();
            foreach (var item in externalList)
            {
                listBox.SelectedItems.Add(item);
            }
            UpdateIsSelectedAllValueLocked(listBox);
        }
        finally
        {
            SetIsUpdating(listBox, false);
        }
    }

    private static void SyncExternalListFromListBox(ListBox listBox, IList externalList)
    {
        if (GetIsUpdating(listBox))
        {
            return;
        }

        SetIsUpdating(listBox, true);
        try
        {
            externalList.Clear();
            foreach (var item in listBox.SelectedItems)
            {
                externalList.Add(item);
            }
        }
        finally
        {
            SetIsUpdating(listBox, false);
        }
    }

    /// <summary>
    /// 计算 IsSelectedAll 三态值并更新——调用方需已置 IsUpdating=true 防重入。
    /// </summary>
    private static void UpdateIsSelectedAllValueLocked(ListBox listBox)
    {
        if (!GetIsSelectAllEnabled(listBox))
        {
            return;
        }

        var selectedCount = listBox.SelectedItems.Count;
        var totalCount = listBox.Items.Count;

        bool? newValue;
        if (totalCount == 0 || selectedCount == 0)
        {
            newValue = false;
        }
        else if (selectedCount == totalCount)
        {
            newValue = true;
        }
        else
        {
            newValue = null;
        }

        if (GetIsSelectedAll(listBox) != newValue)
        {
            SetIsSelectedAll(listBox, newValue);
        }
    }

    /// <summary>
    /// 外部调用版本——内部自行加锁。供 IsSelectAllEnabled 启用时初始计算。
    /// </summary>
    private static void UpdateIsSelectedAllFromListBox(ListBox listBox)
    {
        if (GetIsUpdating(listBox))
        {
            return;
        }

        SetIsUpdating(listBox, true);
        try
        {
            UpdateIsSelectedAllValueLocked(listBox);
        }
        finally
        {
            SetIsUpdating(listBox, false);
        }
    }

    #endregion 双向同步 helpers
}