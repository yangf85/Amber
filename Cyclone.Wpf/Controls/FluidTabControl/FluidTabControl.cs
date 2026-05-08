using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 标签项相对内容容器的位置。
/// </summary>
public enum FluidTabPlacement
{
    /// <summary>标签列在左、内容在右。</summary>
    Left,

    /// <summary>标签列在右、内容在左。</summary>
    Right,
}

/// <summary>
/// 滚动定位的锚点位置——决定哪个内容项被视作"当前项"。
/// </summary>
public enum FluidTabSnapAlignment
{
    /// <summary>视口顶部所在的内容项被视作当前项（默认）。</summary>
    Top,

    /// <summary>视口中心所在的内容项被视作当前项。</summary>
    Center,
}

/// <summary>
/// 流式标签控件：左/右侧标签列表 + 一侧的内容长滚动面板。
/// 选中标签时平滑滚动到对应内容；滚动内容时反向同步选中标签。
/// </summary>
[TemplatePart(Name = nameof(PART_Container), Type = typeof(ScrollViewer))]
[TemplatePart(Name = nameof(PART_ContentPanel), Type = typeof(Panel))]
public class FluidTabControl : Selector
{
    private const string PART_Container = "PART_Container";
    private const string PART_ContentPanel = "PART_ContentPanel";

    private ScrollViewer _container;
    private Panel _contentPanel;

    /// <summary>正在因 Selection 变化而执行滚动动画。</summary>
    private bool _isSyncingSelection;

    /// <summary>正在因 Scroll 变化而更新 Selection。</summary>
    private bool _isSyncingScroll;

    private Storyboard _scrollStoryboard;

    static FluidTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FluidTabControl),
            new FrameworkPropertyMetadata(typeof(FluidTabControl)));
    }

    #region FluidTabPlacement

    /// <summary>
    /// 定义标签列位置的依赖属性。
    /// </summary>
    public static readonly DependencyProperty FluidTabPlacementProperty =
        DependencyProperty.Register(
            nameof(FluidTabPlacement),
            typeof(FluidTabPlacement),
            typeof(FluidTabControl),
            new PropertyMetadata(FluidTabPlacement.Left));

    /// <summary>
    /// 获取或设置标签列相对内容区的位置。
    /// </summary>
    public FluidTabPlacement FluidTabPlacement
    {
        get => (FluidTabPlacement)GetValue(FluidTabPlacementProperty);
        set => SetValue(FluidTabPlacementProperty, value);
    }

    #endregion FluidTabPlacement

    #region ItemHeaderHorizontalAlignment

    /// <summary>
    /// 定义标签项 Header 水平对齐的依赖属性。
    /// </summary>
    public static readonly DependencyProperty ItemHeaderHorizontalAlignmentProperty =
        DependencyProperty.Register(
            nameof(ItemHeaderHorizontalAlignment),
            typeof(HorizontalAlignment),
            typeof(FluidTabControl),
            new PropertyMetadata(HorizontalAlignment.Left));

    /// <summary>
    /// 获取或设置标签项内 Header 的水平对齐方式。
    /// </summary>
    public HorizontalAlignment ItemHeaderHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(ItemHeaderHorizontalAlignmentProperty);
        set => SetValue(ItemHeaderHorizontalAlignmentProperty, value);
    }

    #endregion ItemHeaderHorizontalAlignment

    #region ItemHeaderTemplate

    /// <summary>
    /// 定义标签项 Header 模板的依赖属性。
    /// </summary>
    public static readonly DependencyProperty ItemHeaderTemplateProperty =
        DependencyProperty.Register(
            nameof(ItemHeaderTemplate),
            typeof(DataTemplate),
            typeof(FluidTabControl),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置应用到所有标签项 Header 的统一模板。等同于 ItemTemplate 的标头版本。
    /// </summary>
    public DataTemplate ItemHeaderTemplate
    {
        get => (DataTemplate)GetValue(ItemHeaderTemplateProperty);
        set => SetValue(ItemHeaderTemplateProperty, value);
    }

    #endregion ItemHeaderTemplate

    #region ItemHeaderTemplateSelector

    /// <summary>
    /// 定义标签项 Header 模板选择器的依赖属性。
    /// </summary>
    public static readonly DependencyProperty ItemHeaderTemplateSelectorProperty =
        DependencyProperty.Register(
            nameof(ItemHeaderTemplateSelector),
            typeof(DataTemplateSelector),
            typeof(FluidTabControl),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置标签项 Header 模板选择器。
    /// </summary>
    public DataTemplateSelector ItemHeaderTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(ItemHeaderTemplateSelectorProperty);
        set => SetValue(ItemHeaderTemplateSelectorProperty, value);
    }

    #endregion ItemHeaderTemplateSelector

    #region ItemHeaderMemberPath

    /// <summary>
    /// 定义标签项 Header 数据路径的依赖属性。
    /// </summary>
    public static readonly DependencyProperty ItemHeaderMemberPathProperty =
        DependencyProperty.Register(
            nameof(ItemHeaderMemberPath),
            typeof(string),
            typeof(FluidTabControl),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取或设置数据项中作为 Header 显示的属性路径（类似 DisplayMemberPath）。
    /// </summary>
    public string ItemHeaderMemberPath
    {
        get => (string)GetValue(ItemHeaderMemberPathProperty);
        set => SetValue(ItemHeaderMemberPathProperty, value);
    }

    #endregion ItemHeaderMemberPath

    #region AnimationDuration

    /// <summary>
    /// 定义滚动动画时长的依赖属性。
    /// </summary>
    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(
            nameof(AnimationDuration),
            typeof(Duration),
            typeof(FluidTabControl),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(450))));

    /// <summary>
    /// 获取或设置选中切换时滚动到目标位置的动画时长。
    /// </summary>
    public Duration AnimationDuration
    {
        get => (Duration)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    #endregion AnimationDuration

    #region EasingFunction

    /// <summary>
    /// 定义滚动动画缓动函数的依赖属性。
    /// </summary>
    public static readonly DependencyProperty EasingFunctionProperty =
        DependencyProperty.Register(
            nameof(EasingFunction),
            typeof(IEasingFunction),
            typeof(FluidTabControl),
            new PropertyMetadata(new QuadraticEase { EasingMode = EasingMode.EaseInOut }));

    /// <summary>
    /// 获取或设置滚动动画的缓动函数。
    /// </summary>
    public IEasingFunction EasingFunction
    {
        get => (IEasingFunction)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    #endregion EasingFunction

    #region SnapAlignment

    /// <summary>
    /// 定义滚动定位锚点的依赖属性。
    /// </summary>
    public static readonly DependencyProperty SnapAlignmentProperty =
        DependencyProperty.Register(
            nameof(SnapAlignment),
            typeof(FluidTabSnapAlignment),
            typeof(FluidTabControl),
            new PropertyMetadata(FluidTabSnapAlignment.Top));

    /// <summary>
    /// 获取或设置滚动定位的锚点（决定哪个内容项被视作当前选中项）。
    /// </summary>
    public FluidTabSnapAlignment SnapAlignment
    {
        get => (FluidTabSnapAlignment)GetValue(SnapAlignmentProperty);
        set => SetValue(SnapAlignmentProperty, value);
    }

    #endregion SnapAlignment

    #region Override Methods

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride() => new FluidTabItem();

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item) => item is FluidTabItem;

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is not FluidTabItem container || ReferenceEquals(container, item))
        {
            return;
        }

        ApplyItemHeaderBindings(container, item);
    }

    /// <inheritdoc />
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        SyncContentPanel(e);
    }

    /// <inheritdoc />
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (!_isSyncingScroll)
        {
            ScrollToSelectedItem();
        }
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled || Items.Count == 0)
        {
            return;
        }

        var currentIndex = SelectedIndex;
        var targetIndex = currentIndex;

        switch (e.Key)
        {
            case Key.Up:
                targetIndex = Math.Max(0, currentIndex - 1);
                break;

            case Key.Down:
                targetIndex = Math.Min(Items.Count - 1, currentIndex + 1);
                break;

            case Key.Home:
                targetIndex = 0;
                break;

            case Key.End:
                targetIndex = Items.Count - 1;
                break;

            default:
                return;
        }

        if (targetIndex != currentIndex && targetIndex >= 0 && targetIndex < Items.Count)
        {
            SelectedIndex = targetIndex;
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_container != null)
        {
            _container.ScrollChanged -= OnContainerScrollChanged;
        }

        _container = GetTemplateChild(PART_Container) as ScrollViewer;
        _contentPanel = GetTemplateChild(PART_ContentPanel) as Panel;

        if (_container != null)
        {
            _container.ScrollChanged += OnContainerScrollChanged;
        }

        RebuildContentPanel();
    }

    #endregion Override Methods

    #region Private Methods - Header binding

    /// <summary>
    /// 为 ItemsSource 模式生成的 FluidTabItem 容器配置 Header 相关绑定。
    /// </summary>
    private void ApplyItemHeaderBindings(FluidTabItem container, object item)
    {
        // Header 数据：优先 MemberPath；否则用整个数据项让 HeaderTemplate 渲染
        if (!string.IsNullOrEmpty(ItemHeaderMemberPath))
        {
            container.SetBinding(HeaderedContentControl.HeaderProperty,
                new Binding(ItemHeaderMemberPath));
        }
        else if (container.Header == null
                 && container.ReadLocalValue(HeaderedContentControl.HeaderProperty) == DependencyProperty.UnsetValue)
        {
            // DataContext 由框架自动设为 item，绑定空路径即拿到 item 自身
            container.SetBinding(HeaderedContentControl.HeaderProperty, new Binding());
        }

        // Header 模板：仅在容器自身没设时应用控件级模板
        if (ItemHeaderTemplate != null && container.HeaderTemplate == null)
        {
            container.HeaderTemplate = ItemHeaderTemplate;
        }

        if (ItemHeaderTemplateSelector != null && container.HeaderTemplateSelector == null)
        {
            container.HeaderTemplateSelector = ItemHeaderTemplateSelector;
        }
    }

    #endregion Private Methods - Header binding

    #region Private Methods - Content sync

    /// <summary>
    /// 全量重建内容面板（OnApplyTemplate 时调用）。
    /// </summary>
    private void RebuildContentPanel()
    {
        if (_contentPanel == null)
        {
            return;
        }

        _contentPanel.Children.Clear();

        foreach (var item in Items)
        {
            _contentPanel.Children.Add(CreateContentPresenter(item));
        }
    }

    /// <summary>
    /// 增量同步内容面板，按照 NotifyCollectionChangedAction 处理。
    /// </summary>
    private void SyncContentPanel(NotifyCollectionChangedEventArgs e)
    {
        if (_contentPanel == null)
        {
            return;
        }

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    var insertAt = e.NewStartingIndex;
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        _contentPanel.Children.Insert(insertAt + i, CreateContentPresenter(e.NewItems[i]));
                    }
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                {
                    for (var i = e.OldItems.Count - 1; i >= 0; i--)
                    {
                        var removeAt = e.OldStartingIndex + i;
                        if (removeAt >= 0 && removeAt < _contentPanel.Children.Count)
                        {
                            _contentPanel.Children.RemoveAt(removeAt);
                        }
                    }
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.NewItems != null)
                {
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        var idx = e.OldStartingIndex + i;
                        if (idx >= 0 && idx < _contentPanel.Children.Count)
                        {
                            _contentPanel.Children.RemoveAt(idx);
                            _contentPanel.Children.Insert(idx, CreateContentPresenter(e.NewItems[i]));
                        }
                    }
                }
                break;

            case NotifyCollectionChangedAction.Move:
                if (e.OldStartingIndex >= 0 && e.OldStartingIndex < _contentPanel.Children.Count)
                {
                    var moving = _contentPanel.Children[e.OldStartingIndex];
                    _contentPanel.Children.RemoveAt(e.OldStartingIndex);
                    _contentPanel.Children.Insert(e.NewStartingIndex, moving);
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                RebuildContentPanel();
                break;
        }
    }

    /// <summary>
    /// 为单个 item 创建对应的内容 ContentPresenter，绑定到容器的 Content / ContentTemplate。
    /// </summary>
    private ContentPresenter CreateContentPresenter(object item)
    {
        var presenter = new ContentPresenter();

        if (item is FluidTabItem tabItem)
        {
            // 直接添加的 FluidTabItem：绑定到它的 Content / ContentTemplate
            // 注意：FluidTabItem 自身模板不渲染 Content，所以这里独占该对象不会冲突
            presenter.SetBinding(ContentPresenter.ContentProperty,
                new Binding(nameof(FluidTabItem.Content)) { Source = tabItem });
            presenter.SetBinding(ContentPresenter.ContentTemplateProperty,
                new Binding(nameof(FluidTabItem.ContentTemplate)) { Source = tabItem });
            presenter.SetBinding(ContentPresenter.ContentTemplateSelectorProperty,
                new Binding(nameof(FluidTabItem.ContentTemplateSelector)) { Source = tabItem });
        }
        else
        {
            // ItemsSource 模式：item 是数据，由控件级 ItemTemplate / Selector 渲染
            presenter.Content = item;
            presenter.ContentTemplate = ItemTemplate;
            presenter.ContentTemplateSelector = ItemTemplateSelector;
        }

        return presenter;
    }

    #endregion Private Methods - Content sync

    #region Private Methods - Scroll sync

    /// <summary>
    /// 滚动到当前选中项对应的内容。
    /// </summary>
    private void ScrollToSelectedItem()
    {
        if (_container == null || _contentPanel == null || SelectedItem == null)
        {
            return;
        }

        var selectedIndex = Items.IndexOf(SelectedItem);
        if (selectedIndex < 0 || selectedIndex >= _contentPanel.Children.Count)
        {
            return;
        }

        if (_contentPanel.Children[selectedIndex] is not FrameworkElement target)
        {
            return;
        }

        // 计算目标内容相对内容面板的 Y 偏移
        var targetTop = target.TranslatePoint(new Point(), _contentPanel).Y;

        // SnapAlignment.Center 时，把目标元素居中于视口
        var targetOffset = SnapAlignment == FluidTabSnapAlignment.Center
            ? targetTop - Math.Max(0, (_container.ViewportHeight - target.ActualHeight) / 2)
            : targetTop;

        // 限制在可滚动范围内
        targetOffset = Math.Max(0, Math.Min(targetOffset, _container.ScrollableHeight));

        AnimateToOffset(targetOffset);
    }

    /// <summary>
    /// 启动到目标垂直偏移的滚动动画。
    /// </summary>
    private void AnimateToOffset(double targetOffset)
    {
        StopScrollAnimation();

        if (_container == null)
        {
            return;
        }

        if (AnimationDuration.HasTimeSpan == false
            || AnimationDuration.TimeSpan == TimeSpan.Zero
            || Math.Abs(_container.VerticalOffset - targetOffset) < 0.5)
        {
            _container.ScrollToVerticalOffset(targetOffset);
            return;
        }

        var animation = new DoubleAnimation
        {
            From = _container.VerticalOffset,
            To = targetOffset,
            Duration = AnimationDuration,
            EasingFunction = EasingFunction,
        };

        Storyboard.SetTarget(animation, _container);
        Storyboard.SetTargetProperty(animation, new PropertyPath(ScrollViewerOffsetBehavior.VerticalOffsetProperty));

        _scrollStoryboard = new Storyboard();
        _scrollStoryboard.Children.Add(animation);

        _isSyncingSelection = true;
        _scrollStoryboard.Completed += OnScrollStoryboardCompleted;
        _scrollStoryboard.Begin();
    }

    private void OnScrollStoryboardCompleted(object sender, EventArgs e)
    {
        if (_scrollStoryboard != null)
        {
            _scrollStoryboard.Completed -= OnScrollStoryboardCompleted;
        }

        _scrollStoryboard = null;
        _isSyncingSelection = false;
    }

    private void StopScrollAnimation()
    {
        if (_scrollStoryboard == null)
        {
            return;
        }

        _scrollStoryboard.Completed -= OnScrollStoryboardCompleted;
        _scrollStoryboard.Stop();
        _scrollStoryboard = null;
        _isSyncingSelection = false;
    }

    /// <summary>
    /// 内容滚动时反向同步 Selection。
    /// </summary>
    private void OnContainerScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // 仅处理垂直方向变化
        if (Math.Abs(e.VerticalChange) < 0.001)
        {
            return;
        }

        if (_isSyncingSelection || _scrollStoryboard != null || _container == null || _contentPanel == null)
        {
            return;
        }

        if (_contentPanel.Children.Count == 0)
        {
            return;
        }

        var anchorY = SnapAlignment == FluidTabSnapAlignment.Center
            ? e.VerticalOffset + (_container.ViewportHeight / 2)
            : e.VerticalOffset;

        var anchorIndex = FindAnchorIndex(anchorY);
        if (anchorIndex < 0 || anchorIndex >= Items.Count)
        {
            return;
        }

        var item = Items[anchorIndex];
        if (Equals(SelectedItem, item))
        {
            return;
        }

        _isSyncingScroll = true;
        try
        {
            SelectedItem = item;
        }
        finally
        {
            _isSyncingScroll = false;
        }
    }

    /// <summary>
    /// 找出包含锚点 Y 坐标的内容项索引；锚点不命中任何项时返回最近项。
    /// </summary>
    private int FindAnchorIndex(double anchorY)
    {
        if (_contentPanel == null || _contentPanel.Children.Count == 0)
        {
            return -1;
        }

        var lastIndex = -1;
        for (var i = 0; i < _contentPanel.Children.Count; i++)
        {
            if (_contentPanel.Children[i] is not FrameworkElement child)
            {
                continue;
            }

            var top = child.TranslatePoint(new Point(), _contentPanel).Y;
            var bottom = top + child.ActualHeight;

            if (anchorY < top)
            {
                // 锚点在第一个项之前
                return lastIndex < 0 ? i : lastIndex;
            }

            if (anchorY < bottom)
            {
                return i;
            }

            lastIndex = i;
        }

        // 锚点超出最后一项末尾
        return lastIndex;
    }

    #endregion Private Methods - Scroll sync

    /// <summary>
    /// 通过附加属性桥接 ScrollViewer 的只读 VerticalOffset，使其支持动画。
    /// </summary>
    private static class ScrollViewerOffsetBehavior
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "VerticalOffset",
                typeof(double),
                typeof(ScrollViewerOffsetBehavior),
                new FrameworkPropertyMetadata(0d, OnVerticalOffsetChanged));

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer sv)
            {
                sv.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        public static double GetVerticalOffset(DependencyObject obj) => (double)obj.GetValue(VerticalOffsetProperty);

        public static void SetVerticalOffset(DependencyObject obj, double value) => obj.SetValue(VerticalOffsetProperty, value);
    }
}