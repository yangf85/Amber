using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 时间选择器控件，支持小时、分钟、秒的选择
/// </summary>
[TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
[TemplatePart(Name = "PART_CyclicPanel", Type = typeof(CyclicPanel))]
public class TimeSelector : Selector
{
    #region 私有字段

    private CyclicPanel _cyclicPanel;

    private DependencyPropertyDescriptor _dpd;

    private bool _isScrolling = false;

    private bool _isUpdatingSelection = false;

    private ScrollViewer _scrollViewer;

    #endregion 私有字段

    #region 事件

    public event EventHandler<TimeValueChangedEventArgs> ValueChanged;

    #endregion 事件

    #region 构造函数

    private void HandlePanelVisbleIndicesChanged(object? sender, EventArgs e)
    {
        try
        {
            if (_cyclicPanel?.VisibleItemIndices == null || _cyclicPanel.VisibleItemIndices.Count == 0 || _isScrolling)
                return;

            int centerIndex = (int)Math.Floor(VisibleItemCount / 2d);
            if (centerIndex < _cyclicPanel.VisibleItemIndices.Count)
            {
                SelectedIndex = _cyclicPanel.VisibleItemIndices[centerIndex];
                UpdateSelectedIndex();
            }
        }
        catch (Exception ex)
        {
            // 记录异常但不抛出，保持UI稳定性
            System.Diagnostics.Debug.WriteLine($"HandlePanelVisbleIndicesChanged异常: {ex.Message}");
        }
    }

    // 添加 ItemClick 事件处理方法
    private void OnTimeSelectorItemClicked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is TimeSelectorItem clickedItem)
        {
            // 获取点击项的索引
            int index = Items.IndexOf(clickedItem);
            if (index >= 0)
            {
                // 设置选中项
                SelectedIndex = index;
                SelectedTimeValue = clickedItem.Value;

                // 滚动到中间位置
                if (_cyclicPanel != null)
                {
                    // 先标记为正在滚动，避免事件循环
                    _isScrolling = true;
                    try
                    {
                        // 更新选中状态
                        UpdateSelectedIndex();

                        // 使用前面添加的方法滚动到中间位置
                        _cyclicPanel.ScrollToIndexCentered(index, true); // 使用动画效果
                    }
                    finally
                    {
                        // 延迟重置滚动标志
                        Dispatcher.BeginInvoke(new Action(() => _isScrolling = false), DispatcherPriority.Background);
                    }
                }
            }
        }
    }

    private void TimeSelector_Loaded(object sender, RoutedEventArgs e)
    {
        if (_cyclicPanel == null) { return; }
        AddHandler(TimeSelectorItem.ItemClickedEvent, new RoutedEventHandler(OnTimeSelectorItemClicked));

        // 确保 CyclicPanel 禁用动画
        _cyclicPanel.IsAnimationEnabled = false;

        // 监听面板可见索引的变化
        _dpd = DependencyPropertyDescriptor.FromProperty(CyclicPanel.VisibleItemIndicesProperty, typeof(CyclicPanel));
        _dpd.AddValueChanged(_cyclicPanel, HandlePanelVisbleIndicesChanged);

        // 使用延迟初始化，确保布局已完成
        Dispatcher.BeginInvoke(new Action(() =>
        {
            // 使控件先完成布局和容器生成
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // 如果已设置 SelectedTimeValue，滚动到对应值
                if (SelectedTimeValue > 0)
                {
                    ScrollToValue(SelectedTimeValue);
                }
                else
                {
                    // 否则初始化选中索引，设置为中间项
                    SelectedIndex = VisibleItemCount / 2;
                    UpdateSelectedIndex();
                    ScrollToSelectedIndex();
                }

                // 再次确保所有项目已正确更新选中状态
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateSelectedIndex();
                }), DispatcherPriority.Background);
            }), DispatcherPriority.Render);
        }), DispatcherPriority.Loaded);
    }

    private void TimeSelector_Unloaded(object sender, RoutedEventArgs e)
    {
        // 移除事件监听，防止内存泄漏
        if (_cyclicPanel != null && _dpd != null)
        {
            _dpd.RemoveValueChanged(_cyclicPanel, HandlePanelVisbleIndicesChanged);
        }

        // 保存当前选中的索引
        if (Items.Count > VisibleItemCount && _cyclicPanel?.VisibleItemIndices?.Count > 0)
        {
            SelectedIndex = _cyclicPanel.VisibleItemIndices[VisibleItemCount / 2];
        }
    }

    /// <summary>
    /// 更新所有项目的选中状态
    /// </summary>
    private void UpdateSelectedIndex()
    {
        // 避免重复更新，提高性能
        if (_isUpdatingSelection)
            return;

        _isUpdatingSelection = true;
        try
        {
            for (int i = 0; i < Items.Count; i++)
            {
                // 获取项目容器
                var container = ItemContainerGenerator.ContainerFromIndex(i);
                if (container is TimeSelectorItem item)
                {
                    bool shouldBeSelected = i == SelectedIndex;
                    if (item.IsSelected != shouldBeSelected)
                    {
                        item.IsSelected = shouldBeSelected;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // 记录异常但不抛出，保持UI稳定性
            System.Diagnostics.Debug.WriteLine($"UpdateSelectedIndex异常: {ex.Message}");
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    static TimeSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSelector), new FrameworkPropertyMetadata(typeof(TimeSelector)));
    }

    public TimeSelector()
    {
        GenerateItems();
        Loaded += TimeSelector_Loaded;
        Unloaded += TimeSelector_Unloaded;
    }

    #endregion 构造函数

    #region 依赖属性

    #region VisibleItemCount

    public static readonly DependencyProperty VisibleItemCountProperty =
        DependencyProperty.Register(nameof(VisibleItemCount), typeof(int), typeof(TimeSelector),
            new PropertyMetadata(5, OnVisibleItemCountChanged, OnCoerceVisibleItemCount));

    public int VisibleItemCount
    {
        get => (int)GetValue(VisibleItemCountProperty);
        set => SetValue(VisibleItemCountProperty, value);
    }

    private void UpdateItemHeight()
    {
        if (ActualHeight <= 0 || VisibleItemCount <= 0)
            return;

        var height = ActualHeight / VisibleItemCount;

        foreach (var item in Items.OfType<TimeSelectorItem>())
        {
            if (item.Height != height)
            {
                item.Height = height;
            }
        }
    }

    private static object OnCoerceVisibleItemCount(DependencyObject d, object baseValue)
    {
        // Coerce 不抛异常 — 它在 WPF 属性系统内部调用,异常会破坏 DP 状态。
        // 这里强制 clamp 到合法范围:奇数,3 ≤ v ≤ Items.Count
        var value = (int)baseValue;
        var selector = (TimeSelector)d;

        if (value < 3) value = 3;
        if (value % 2 == 0) value += 1;  // 强制奇数
        if (selector.Items.Count > 0 && value > selector.Items.Count)
        {
            // 不能超过 Items.Count(否则中心索引算法越界)
            value = selector.Items.Count;
            if (value % 2 == 0) value -= 1;
        }

        return value;
    }

    private static void OnVisibleItemCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = (TimeSelector)d;

        selector.Dispatcher.BeginInvoke(new Action(() =>
        {
            selector.UpdateItemHeight();
        }), DispatcherPriority.Loaded);
    }

    #endregion VisibleItemCount

    #region SelectedTimeValue

    public static readonly DependencyProperty SelectedTimeValueProperty =
        DependencyProperty.Register("SelectedTimeValue", typeof(int), typeof(TimeSelector),
            new PropertyMetadata(0, OnSelectedTimeValueChanged));

    public int SelectedTimeValue
    {
        get { return (int)GetValue(SelectedTimeValueProperty); }
        set { SetValue(SelectedTimeValueProperty, value); }
    }

    private static void OnSelectedTimeValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = (TimeSelector)d;
        int value = (int)e.NewValue;

        if (selector.Items.Count <= 0)
            return;

        // 查找与值匹配的项目并选中
        var item = selector.Items.OfType<TimeSelectorItem>().FirstOrDefault(i => i.Value == value);
        if (item != null)
        {
            int index = selector.Items.IndexOf(item);
            if (index >= 0 && index != selector.SelectedIndex)
            {
                // 标记为正在滚动，防止事件循环
                selector._isScrolling = true;
                try
                {
                    selector.SelectedIndex = index;

                    // 确保立即更新选中状态
                    selector.UpdateSelectedIndex();

                    // 滚动到选中项的位置
                    selector.ScrollToSelectedIndex();
                }
                finally
                {
                    selector.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        selector._isScrolling = false;
                    }), DispatcherPriority.Background);
                }
            }
        }
    }

    #endregion SelectedTimeValue

    #region SelectorType

    public static readonly DependencyProperty SelectorTypeProperty =
        DependencyProperty.Register("SelectorType", typeof(TimeSelectorType), typeof(TimeSelector),
            new PropertyMetadata(TimeSelectorType.Hour, OnSelectorTypeChanged));

    public TimeSelectorType SelectorType
    {
        get { return (TimeSelectorType)GetValue(SelectorTypeProperty); }
        set { SetValue(SelectorTypeProperty, value); }
    }

    private void GenerateItems()
    {
        Items.Clear();
        var values = GetValues(SelectorType);

        foreach (var value in values)
        {
            var item = new TimeSelectorItem
            {
                Value = value,
            };
            Items.Add(item);
        }
    }

    private static void OnSelectorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = (TimeSelector)d;
        selector.GenerateItems();
    }

    private IEnumerable<int> GetValues(TimeSelectorType type)
    {
        return type == TimeSelectorType.Hour
            ? Enumerable.Range(0, 24)  // 小时：0-23
            : Enumerable.Range(0, 60); // 分钟或秒：0-59
    }

    #endregion SelectorType

    #endregion 依赖属性

    #region 公共方法

    /// <summary>
    /// 滚动到指定值
    /// </summary>
    /// <param name="value">要滚动到的时间值</param>
    /// <summary>
    /// 滚动到指定值
    /// </summary>
    /// <param name="value">要滚动到的时间值</param>
    public void ScrollToValue(int value)
    {
        if (_cyclicPanel == null || Items.Count == 0)
            return;

        int index = -1;
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i] is TimeSelectorItem item && item.Value == value)
            {
                index = i;
                break;
            }
        }

        if (index >= 0)
        {
            // 先标记为正在滚动，避免事件循环
            _isScrolling = true;
            try
            {
                SelectedIndex = index;
                SelectedTimeValue = value;

                // 先更新选中项状态
                UpdateSelectedIndex();

                // 使用新方法滚动到中心位置
                _cyclicPanel.ScrollToIndexCentered(index, false);

                // 滚动后再次确保更新选中状态
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateSelectedIndex();
                }), DispatcherPriority.Loaded);
            }
            finally
            {
                Dispatcher.BeginInvoke(new Action(() => _isScrolling = false), DispatcherPriority.Background);
            }
        }
    }

    /// <summary>
    /// 滚动到当前选中的索引位置
    /// </summary>
    /// <summary>
    /// 滚动到当前选中的索引位置
    /// </summary>
    private void ScrollToSelectedIndex()
    {
        if (_cyclicPanel != null && SelectedIndex >= 0 && SelectedIndex < Items.Count)
        {
            _isScrolling = true;
            try
            {
                UpdateSelectedIndex();

                // 使用新方法滚动到中心位置
                _cyclicPanel.ScrollToIndexCentered(SelectedIndex, false);
            }
            finally
            {
                // 使用延迟重置标志
                Dispatcher.BeginInvoke(new Action(() => _isScrolling = false), DispatcherPriority.Background);
            }
        }
    }

    #endregion 公共方法

    #region 重写方法

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 获取必要的模板部件
        _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        _cyclicPanel = GetTemplateChild("PART_CyclicPanel") as CyclicPanel;

        if (_cyclicPanel != null)
        {
            // 禁用动画效果
            _cyclicPanel.IsAnimationEnabled = false;
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TimeSelectorItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TimeSelectorItem;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        // 如果高度发生变化，更新项目高度
        if (sizeInfo.HeightChanged)
        {
            UpdateItemHeight();
        }
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (SelectedIndex >= 0 && SelectedIndex < Items.Count)
        {
            // 查找选中的项目并更新值
            if (Items[SelectedIndex] is TimeSelectorItem selectedItem)
            {
                int oldValue = SelectedTimeValue;
                SelectedTimeValue = selectedItem.Value;

                // 仅当值实际变化时触发事件
                if (oldValue != selectedItem.Value)
                {
                    // 触发ValueChanged事件
                    ValueChanged?.Invoke(this, new TimeValueChangedEventArgs(selectedItem.Value));
                }
            }
        }
    }

    #endregion 重写方法
}