using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

public class StepChangedEventArgs : RoutedEventArgs
{
    public int Current { get; set; }

    public StepChangeDirection Direction { get; set; }

    public StepChangedEventArgs(RoutedEvent routedEvent, object source)
        : base(routedEvent, source)
    {
    }
}

public enum StepChangeDirection
{
    Forward,

    Backward,
}

public class Stepper : ItemsControl
{
    static Stepper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Stepper),
            new FrameworkPropertyMetadata(typeof(Stepper)));
    }

    public Stepper()
    {
        // 注册 Loaded 事件——首次加载时确保所有 StepperItem 状态正确
        Loaded += OnLoaded;
    }

    #region CurrentIndex

    public static readonly DependencyProperty CurrentIndexProperty =
        DependencyProperty.Register(
            nameof(CurrentIndex),
            typeof(int),
            typeof(Stepper),
            new PropertyMetadata(0, OnCurrentIndexChanged));

    /// <summary>
    /// 当前激活的步骤索引(0-based)。
    /// </summary>
    public int CurrentIndex
    {
        get => (int)GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    private static void OnCurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Stepper stepper)
        {
            return;
        }

        // 注:这里不再检查 oldIndex == newIndex
        // —— DP 系统本身保证只有值真变化才触发 changed,
        // 但外部 (Reset) 调用时如果当前已是 0,DP 系统会跳过本次回调,
        // 这种情况由 Reset 内部强制刷新逻辑处理。

        var oldIndex = (int)e.OldValue;
        var newIndex = (int)e.NewValue;

        stepper.UpdateStepperItemsStatus();

        var direction = newIndex >= oldIndex ? StepChangeDirection.Forward : StepChangeDirection.Backward;
        var args = new StepChangedEventArgs(StepChangedEvent, stepper)
        {
            Current = newIndex,
            Direction = direction,
        };
        stepper.RaiseEvent(args);
    }

    private void UpdateStepperItemsStatus()
    {
        var itemCount = Items.Count;
        for (var i = 0; i < itemCount; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is StepperItem item)
            {
                item.SetIndex(i);
                item.UpdateStatus(CurrentIndex);
                item.SetIsFirst(i == 0);
                item.SetIsLast(i == itemCount - 1);
            }
        }
    }

    #endregion CurrentIndex

    #region Orientation

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(Stepper),
            new PropertyMetadata(Orientation.Horizontal));

    /// <summary>
    /// 步骤排列方向。
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion Orientation

    #region StepChangedEvent

    public static readonly RoutedEvent StepChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(StepChanged),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(Stepper));

    /// <summary>
    /// 当前步骤索引发生变化时触发。
    /// </summary>
    public event RoutedEventHandler StepChanged
    {
        add => AddHandler(StepChangedEvent, value);
        remove => RemoveHandler(StepChangedEvent, value);
    }

    #endregion StepChangedEvent

    #region Override Methods

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new StepperItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is StepperItem;
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        // OnItemsChanged 触发时 ItemContainerGenerator 还没生成新容器——
        // 用 Dispatcher 延迟到 Loaded 优先级后刷新,此时容器已可用
        Dispatcher.BeginInvoke(new System.Action(UpdateStepperItemsStatus), DispatcherPriority.Loaded);
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is not StepperItem stepperItem)
        {
            return;
        }

        var index = ItemContainerGenerator.IndexFromContainer(element);
        stepperItem.SetIndex(index);
        stepperItem.UpdateStatus(CurrentIndex);
        stepperItem.SetIsFirst(index == 0);
        stepperItem.SetIsLast(index == Items.Count - 1);
    }

    #endregion Override Methods

    #region Private Methods

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 首次加载——确保所有 StepperItem 状态正确(覆盖 CurrentIndex=默认值 0
        // 时 DP 系统不触发 changed 的边界情况)
        UpdateStepperItemsStatus();
    }

    #endregion Private Methods

    #region Public API

    /// <summary>
    /// 跳转到指定步骤。
    /// </summary>
    /// <returns>是否成功跳转。</returns>
    public bool JumpTo(int index)
    {
        if (index >= 0 && index < Items.Count && index != CurrentIndex)
        {
            CurrentIndex = index;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 前进到下一步。
    /// </summary>
    /// <returns>是否成功前进。</returns>
    public bool MoveNext()
    {
        if (CurrentIndex < Items.Count - 1)
        {
            CurrentIndex++;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 后退到上一步。
    /// </summary>
    /// <returns>是否成功后退。</returns>
    public bool MovePrevious()
    {
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 重置到第一步。即使 CurrentIndex 已是 0 也强制刷新状态。
    /// </summary>
    public void Reset()
    {
        if (CurrentIndex == 0)
        {
            // DP 系统检测到值未变化不会触发 changed——手动强制刷新
            UpdateStepperItemsStatus();
        }
        else
        {
            CurrentIndex = 0;
        }
    }

    #endregion Public API
}