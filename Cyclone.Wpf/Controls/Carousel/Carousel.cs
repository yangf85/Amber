// ============================================================================
//  破坏性变更说明（vs 旧 Carousel）：
//    - PART 命名：PART_PrevButton 等 → PartPrevButton 等（PascalCase 跟项目规约一致）
//      迁移：自定义 ControlTemplate 时 x:Name 用新值
//    - NavigationBar logical child 处理改为对称（OldValue 总是 RemoveLogicalChild）
//      迁移：行为更正确，几乎不影响 user 代码
// ============================================================================
using Cyclone.Wpf.Helpers;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 轮播控件——平滑动画 + 自动播放 + 左右导航 + 指示器。
/// </summary>
[TemplatePart(Name = PartPrevButton, Type = typeof(Button))]
[TemplatePart(Name = PartNextButton, Type = typeof(Button))]
[TemplatePart(Name = PartScrollViewer, Type = typeof(ScrollViewer))]
[TemplatePart(Name = PartIndicatorsListBox, Type = typeof(ListBox))]
public class Carousel : ListBox
{
    #region Constants & Fields

    private const string PartPrevButton = nameof(PartPrevButton);

    private const string PartNextButton = nameof(PartNextButton);

    private const string PartScrollViewer = nameof(PartScrollViewer);

    private const string PartIndicatorsListBox = nameof(PartIndicatorsListBox);

    private readonly DispatcherTimer _autoPlayTimer;

    private readonly Storyboard _storyboard;

    private readonly DoubleAnimation _animation;

    private Button _prevButton;

    private Button _nextButton;

    private ScrollViewer _scrollViewer;

    private ListBox _indicatorsListBox;

    private bool _isAnimating;

    #endregion Constants & Fields

    #region Static Constructor & Constructor

    static Carousel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Carousel),
            new FrameworkPropertyMetadata(typeof(Carousel)));
    }

    public Carousel()
    {
        InitializeCommand();

        _autoPlayTimer = new DispatcherTimer();
        _autoPlayTimer.Tick += OnAutoPlayTimerTick;

        _animation = new DoubleAnimation
        {
            EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
        };

        _storyboard = new Storyboard();
        _storyboard.Children.Add(_animation);
        _storyboard.Completed += OnAnimationCompleted;

        Loaded += OnCarouselLoaded;
        Unloaded += OnCarouselUnloaded;

        // 阻止 Carousel 内部触发的 RequestBringIntoView 冒泡到外层 ScrollViewer——
        // ScrollIntoView / 焦点切换 都会触发该路由事件，未拦截的话外层 ScrollViewer 会
        // "自动滚动"到 Carousel 位置，导致包含 Carousel 的页面发生意外跳跃。
        // 内层 PART_ScrollViewer 仍然能正常响应（在事件到达 Carousel 之前已处理）。
        AddHandler(
            RequestBringIntoViewEvent,
            new RequestBringIntoViewEventHandler(OnRequestBringIntoView));

        // 监听 Items 变化——AutoPlay 在 count 跨 1 阈值时 stop/start，命令 CanExecute 也要刷新
        ((INotifyCollectionChanged)Items).CollectionChanged += OnItemsCollectionChanged;
    }

    #endregion Static Constructor & Constructor

    #region DependencyProperty - IsEnableAnimation

    public static readonly DependencyProperty IsEnableAnimationProperty =
        DependencyProperty.Register(nameof(IsEnableAnimation), typeof(bool), typeof(Carousel),
            new PropertyMetadata(true));

    /// <summary>是否启用切换动画。注意：循环 wrap 跳转（最后到第一项 / 第一到最后）始终无动画——直接定位。</summary>
    public bool IsEnableAnimation
    {
        get => (bool)GetValue(IsEnableAnimationProperty);
        set => SetValue(IsEnableAnimationProperty, value);
    }

    #endregion DependencyProperty - IsEnableAnimation

    #region DependencyProperty - IsRepeatPlayback

    public static readonly DependencyProperty IsRepeatPlaybackProperty =
        DependencyProperty.Register(nameof(IsRepeatPlayback), typeof(bool), typeof(Carousel),
            new PropertyMetadata(true));

    /// <summary>是否循环播放——到末尾后跳回开头，反之亦然。</summary>
    public bool IsRepeatPlayback
    {
        get => (bool)GetValue(IsRepeatPlaybackProperty);
        set => SetValue(IsRepeatPlaybackProperty, value);
    }

    #endregion DependencyProperty - IsRepeatPlayback

    #region DependencyProperty - IsWrapAnimated

    public static readonly DependencyProperty IsWrapAnimatedProperty =
        DependencyProperty.Register(nameof(IsWrapAnimated), typeof(bool), typeof(Carousel),
            new PropertyMetadata(false));

    /// <summary>
    /// 循环 wrap 跳转（最后项→第一项 / 第一项→最后项）是否播放动画。<br/>
    /// 默认 <c>false</c>——wrap 时直接定位，无动画。<br/>
    /// 设 <c>true</c> 时用 slide 动画一路滚到目标位置——视觉上是"倒带"效果
    /// （比硬切平滑，但能感受到跨越多项的滚动距离）。<br/>
    /// 仅在 <see cref="IsRepeatPlayback"/> 和 <see cref="IsEnableAnimation"/> 都为 <c>true</c> 时生效。
    /// </summary>
    public bool IsWrapAnimated
    {
        get => (bool)GetValue(IsWrapAnimatedProperty);
        set => SetValue(IsWrapAnimatedProperty, value);
    }

    #endregion DependencyProperty - IsWrapAnimated

    #region DependencyProperty - AnimationDuration

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(Duration), typeof(Carousel),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(200)), OnAnimationDurationChanged));

    public Duration AnimationDuration
    {
        get => (Duration)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    private static void OnAnimationDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var carousel = (Carousel)d;
        carousel._animation.Duration = (Duration)e.NewValue;
    }

    #endregion DependencyProperty - AnimationDuration

    #region DependencyProperty - FunctionBar

    public static readonly DependencyProperty FunctionBarProperty =
        DependencyProperty.Register(nameof(FunctionBar), typeof(object), typeof(Carousel),
            new PropertyMetadata(default));

    public object FunctionBar
    {
        get => GetValue(FunctionBarProperty);
        set => SetValue(FunctionBarProperty, value);
    }

    #endregion DependencyProperty - FunctionBar

    #region DependencyProperty - NavigationBar

    public static readonly DependencyProperty NavigationBarProperty =
        DependencyProperty.Register(nameof(NavigationBar), typeof(object), typeof(Carousel),
            new PropertyMetadata(default(object), OnNavigationBarChanged));

    public object NavigationBar
    {
        get => GetValue(NavigationBarProperty);
        set => SetValue(NavigationBarProperty, value);
    }

    /// <summary>
    /// NavigationBar 变化：对称 Remove/Add logical child——避免 OldValue 残留在 logical tree。
    /// </summary>
    private static void OnNavigationBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var carousel = (Carousel)d;
        if (e.OldValue != null)
        {
            carousel.RemoveLogicalChild(e.OldValue);
        }
        if (e.NewValue != null)
        {
            carousel.AddLogicalChild(e.NewValue);
        }
    }

    #endregion DependencyProperty - NavigationBar

    #region DependencyProperty - AutoPlay

    public static readonly DependencyProperty AutoPlayProperty =
        DependencyProperty.Register(nameof(AutoPlay), typeof(bool), typeof(Carousel),
            new PropertyMetadata(false, OnAutoPlayChanged));

    public bool AutoPlay
    {
        get => (bool)GetValue(AutoPlayProperty);
        set => SetValue(AutoPlayProperty, value);
    }

    private static void OnAutoPlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var carousel = (Carousel)d;
        carousel.UpdateAutoPlayState();
    }

    #endregion DependencyProperty - AutoPlay

    #region DependencyProperty - AutoPlayInterval

    public static readonly DependencyProperty AutoPlayIntervalProperty =
        DependencyProperty.Register(nameof(AutoPlayInterval), typeof(TimeSpan), typeof(Carousel),
            new PropertyMetadata(TimeSpan.FromSeconds(3), OnAutoPlayIntervalChanged));

    public TimeSpan AutoPlayInterval
    {
        get => (TimeSpan)GetValue(AutoPlayIntervalProperty);
        set => SetValue(AutoPlayIntervalProperty, value);
    }

    private static void OnAutoPlayIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var carousel = (Carousel)d;
        carousel.UpdateAutoPlayInterval();
    }

    #endregion DependencyProperty - AutoPlayInterval

    #region RoutedCommands

    public static readonly RoutedCommand PrevCommand = new RoutedCommand(nameof(PrevCommand), typeof(Carousel));

    public static readonly RoutedCommand NextCommand = new RoutedCommand(nameof(NextCommand), typeof(Carousel));

    private void InitializeCommand()
    {
        CommandBindings.Add(new CommandBinding(PrevCommand, OnExecutedPrevCommand, OnCanExecutePrevCommand));
        CommandBindings.Add(new CommandBinding(NextCommand, OnExecutedNextCommand, OnCanExecuteNextCommand));
    }

    private void OnCanExecutePrevCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = Items.Count > 0 && (IsRepeatPlayback || SelectedIndex > 0);
    }

    private void OnExecutedPrevCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (_isAnimating)
        {
            return;
        }

        if (SelectedIndex > 0)
        {
            SelectedIndex--;
        }
        else if (IsRepeatPlayback && Items.Count > 0)
        {
            SelectedIndex = Items.Count - 1;
        }
    }

    private void OnCanExecuteNextCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = Items.Count > 0 && (IsRepeatPlayback || SelectedIndex < Items.Count - 1);
    }

    private void OnExecutedNextCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (_isAnimating)
        {
            return;
        }

        if (SelectedIndex < Items.Count - 1)
        {
            SelectedIndex++;
        }
        else if (IsRepeatPlayback && Items.Count > 0)
        {
            SelectedIndex = 0;
        }
    }

    #endregion RoutedCommands

    #region Override Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 防止前一次模板的动画状态残留——主题切换 / 重新应用模板时 OnApplyTemplate 会再调
        _storyboard?.Stop();
        _isAnimating = false;

        _prevButton = GetTemplateChild(PartPrevButton) as Button;
        _nextButton = GetTemplateChild(PartNextButton) as Button;
        _scrollViewer = GetTemplateChild(PartScrollViewer) as ScrollViewer;
        _indicatorsListBox = GetTemplateChild(PartIndicatorsListBox) as ListBox;

        if (_scrollViewer != null)
        {
            Storyboard.SetTarget(_animation, _scrollViewer);
            Storyboard.SetTargetProperty(_animation,
                new PropertyPath(ScrollViewerHelper.HorizontalOffsetProperty));
            _animation.Duration = AnimationDuration;
        }

        // OnApplyTemplate 可能多次调用——先 -= 再 += 避免 SizeChanged handler 累积
        SizeChanged -= OnCarouselSizeChanged;
        SizeChanged += OnCarouselSizeChanged;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new CarouselItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is CarouselItem;
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (_scrollViewer == null || Items.Count == 0)
        {
            return;
        }

        int oldIndex = e.RemovedItems.Count > 0 ? Items.IndexOf(e.RemovedItems[0]) : -1;
        int newIndex = SelectedIndex;

        // 首次选中（无前一项）—— 直接定位
        if (oldIndex == -1 || newIndex < 0)
        {
            if (newIndex >= 0)
            {
                ScrollIntoView(Items[newIndex]);
            }
            return;
        }

        if (IsEnableAnimation)
        {
            int diff = Math.Abs(newIndex - oldIndex);

            // 相邻切换——播放标准 slide 动画
            if (diff == 1)
            {
                PlayAnimation(oldIndex, newIndex, isWrap: false);
                return;
            }

            // 循环 wrap 切换（首↔尾跳转）——仅 IsWrapAnimated + IsRepeatPlayback 时播放动画
            if (IsWrapAnimated && IsRepeatPlayback && diff == Items.Count - 1)
            {
                PlayAnimation(oldIndex, newIndex, isWrap: true);
                return;
            }
        }

        // 其他情况（非相邻 / 跨多项跳转 / 动画禁用）—— 直接定位
        ScrollIntoView(Items[newIndex]);
    }

    #endregion Override Methods

    #region Event Handlers

    private void OnCarouselLoaded(object sender, RoutedEventArgs e)
    {
        UpdateAutoPlayState();
        CommandManager.InvalidateRequerySuggested();
    }

    private void OnCarouselUnloaded(object sender, RoutedEventArgs e)
    {
        StopAutoPlay();

        // 卸载时确保动画停止——避免 detached visual 上动画继续 tick
        _storyboard?.Stop();
        _isAnimating = false;

        _autoPlayTimer.Tick -= OnAutoPlayTimerTick;
        _storyboard.Completed -= OnAnimationCompleted;
        ((INotifyCollectionChanged)Items).CollectionChanged -= OnItemsCollectionChanged;

        SizeChanged -= OnCarouselSizeChanged;
    }

    private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // Items 变化时——AutoPlay 跨 count=1 阈值时 stop/start，命令 CanExecute 刷新
        UpdateAutoPlayState();
        CommandManager.InvalidateRequerySuggested();
    }

    private void OnAutoPlayTimerTick(object sender, EventArgs e)
    {
        // 防御除零——理论上 UpdateAutoPlayState 在 Items.Count <= 1 时 stop 了，
        // 但 timer tick 跟集合变化的 race 可能让 timer 在空集合时跑一次
        if (_isAnimating || Items.Count == 0)
        {
            return;
        }

        SelectedIndex = (SelectedIndex + 1) % Items.Count;
    }

    /// <summary>
    /// 阻止 Carousel 内部触发的 RequestBringIntoView 路由事件向外层冒泡。
    /// 内层 PART_ScrollViewer 已在事件到达本控件之前响应——无需外层 ScrollViewer 干预。
    /// </summary>
    private void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        e.Handled = true;
    }

    private void OnAnimationCompleted(object sender, EventArgs e)
    {
        _isAnimating = false;

        // 收尾对齐：动画结束后强制 ScrollIntoView 当前选中项，避免误差累积
        if (_scrollViewer != null && SelectedIndex >= 0 && SelectedIndex < Items.Count)
        {
            ScrollIntoView(Items[SelectedIndex]);
        }

        CommandManager.InvalidateRequerySuggested();
    }

    private void OnCarouselSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_scrollViewer == null || Items.Count == 0 || SelectedIndex < 0)
        {
            return;
        }

        // 延迟到下一帧 layout pass 之后——避免在当前 layout pass 中 ForceLayout 嵌套
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            if (_scrollViewer != null && SelectedIndex >= 0 && SelectedIndex < Items.Count)
            {
                ScrollIntoView(Items[SelectedIndex]);
            }
        }));
    }

    #endregion Event Handlers

    #region Private Methods

    private void PlayAnimation(int oldIndex, int newIndex, bool isWrap)
    {
        if (_scrollViewer == null || _isAnimating)
        {
            return;
        }

        _isAnimating = true;

        var oldContainer = ItemContainerGenerator.ContainerFromIndex(oldIndex) as CarouselItem;
        if (oldContainer == null)
        {
            // 容器还没生成（数据虚拟化等）—— 降级到直接定位
            _isAnimating = false;
            ScrollIntoView(Items[newIndex]);
            return;
        }

        double from = _scrollViewer.HorizontalOffset;
        double to;

        if (isWrap)
        {
            // wrap 跳转：动画到 newIndex 的实际位置（跨多项滚动）
            // 视觉上"倒带" / "快进"——比硬切平滑
            if (ItemContainerGenerator.ContainerFromIndex(newIndex) is not CarouselItem newContainer)
            {
                _isAnimating = false;
                ScrollIntoView(Items[newIndex]);
                return;
            }
            to = newIndex * newContainer.ActualWidth;
        }
        else
        {
            // 相邻跳转：滚动一个 item 宽度
            to = newIndex > oldIndex
                ? from + oldContainer.ActualWidth
                : from - oldContainer.ActualWidth;
        }

        _storyboard.Stop();
        _animation.From = from;
        _animation.To = to;

        try
        {
            _storyboard.Begin();
        }
        catch (InvalidOperationException)
        {
            // Storyboard target / property 配置错误——降级到直接定位
            _isAnimating = false;
            ScrollIntoView(Items[newIndex]);
        }
    }

    private void UpdateAutoPlayState()
    {
        if (AutoPlay && Items.Count > 1)
        {
            StartAutoPlay();
        }
        else
        {
            StopAutoPlay();
        }
    }

    private void UpdateAutoPlayInterval()
    {
        _autoPlayTimer.Interval = AutoPlayInterval;

        // timer 正在跑就重启让新 interval 立即生效（仍走 UpdateAutoPlayState 检查 Items.Count > 1）
        if (_autoPlayTimer.IsEnabled)
        {
            StopAutoPlay();
            UpdateAutoPlayState();
        }
    }

    private void StartAutoPlay()
    {
        _autoPlayTimer.Interval = AutoPlayInterval;
        _autoPlayTimer.Start();
    }

    private void StopAutoPlay()
    {
        _autoPlayTimer.Stop();
    }

    #endregion Private Methods
}