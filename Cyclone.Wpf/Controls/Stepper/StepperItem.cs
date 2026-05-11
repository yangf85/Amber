using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

public enum StepStatus
{
    Completed,

    Current,

    Pending,
}

public class StepperItem : ContentControl
{
    static StepperItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StepperItem),
            new FrameworkPropertyMetadata(typeof(StepperItem)));

        // 只读 DP 在静态构造函数里显式按序初始化——避免依赖字段声明顺序,
        // 即使代码整理工具重排字段也不会出现 NRE

        // Status
        StatusPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Status),
            typeof(StepStatus),
            typeof(StepperItem),
            new PropertyMetadata(StepStatus.Pending, OnStatusChangedCallback));
        StatusProperty = StatusPropertyKey.DependencyProperty;

        // IsFirst
        IsFirstPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsFirst),
            typeof(bool),
            typeof(StepperItem),
            new PropertyMetadata(default(bool)));
        IsFirstProperty = IsFirstPropertyKey.DependencyProperty;

        // IsLast
        IsLastPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsLast),
            typeof(bool),
            typeof(StepperItem),
            new PropertyMetadata(default(bool)));
        IsLastProperty = IsLastPropertyKey.DependencyProperty;

        // Index
        IndexPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Index),
            typeof(int),
            typeof(StepperItem),
            new PropertyMetadata(-1));
        IndexProperty = IndexPropertyKey.DependencyProperty;
    }

    #region Description

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(StepperItem),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 步骤的描述文本(主标题用 Content,辅助说明用 Description)。
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    #endregion Description

    #region Status (只读)

    public static readonly DependencyProperty StatusProperty;

    /// <summary>
    /// 步骤状态(Pending / Current / Completed),只读 DP——初始化见静态构造函数。
    /// </summary>
    private static readonly DependencyPropertyKey StatusPropertyKey;

    /// <summary>
    /// 步骤当前状态(Pending / Current / Completed),由父 Stepper 根据 CurrentIndex 自动维护。
    /// </summary>
    public StepStatus Status => (StepStatus)GetValue(StatusProperty);

    /// <summary>
    /// 内部方法,根据当前步骤索引更新自身状态。
    /// </summary>
    internal void UpdateStatus(int currentIndex)
    {
        StepStatus next;
        if (Index < currentIndex)
        {
            next = StepStatus.Completed;
        }
        else if (Index == currentIndex)
        {
            next = StepStatus.Current;
        }
        else
        {
            next = StepStatus.Pending;
        }
        SetValue(StatusPropertyKey, next);
    }

    private static void OnStatusChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StepperItem item)
        {
            item.RaiseEvent(new RoutedEventArgs(StatusChangedEvent, item));
        }
    }

    #endregion Status (只读)

    #region IsFirst (只读)

    public static readonly DependencyProperty IsFirstProperty;

    /// <summary>
    /// 是否首项,只读 DP——初始化见静态构造函数。
    /// </summary>
    private static readonly DependencyPropertyKey IsFirstPropertyKey;

    public bool IsFirst => (bool)GetValue(IsFirstProperty);

    internal void SetIsFirst(bool value)
    {
        SetValue(IsFirstPropertyKey, value);
    }

    #endregion IsFirst (只读)

    #region IsLast (只读)

    public static readonly DependencyProperty IsLastProperty;

    /// <summary>
    /// 是否末项,只读 DP——初始化见静态构造函数。
    /// </summary>
    private static readonly DependencyPropertyKey IsLastPropertyKey;

    public bool IsLast => (bool)GetValue(IsLastProperty);

    internal void SetIsLast(bool value)
    {
        SetValue(IsLastPropertyKey, value);
    }

    #endregion IsLast (只读)

    #region Index (只读)

    public static readonly DependencyProperty IndexProperty;

    /// <summary>
    /// 步骤索引,只读 DP——初始化见静态构造函数。
    /// </summary>
    private static readonly DependencyPropertyKey IndexPropertyKey;

    /// <summary>
    /// 该步骤在 Stepper 中的索引(0-based)。
    /// </summary>
    public int Index => (int)GetValue(IndexProperty);

    internal void SetIndex(int value)
    {
        SetValue(IndexPropertyKey, value);
    }

    #endregion Index (只读)

    #region CanNavigate

    public static readonly DependencyProperty CanNavigateProperty =
        DependencyProperty.Register(
            nameof(CanNavigate),
            typeof(bool),
            typeof(StepperItem),
            new PropertyMetadata(true));

    /// <summary>
    /// 是否允许点击该步骤跳转。常用于强制用户线性走流程的场景(把未来步骤设为 false)。
    /// </summary>
    public bool CanNavigate
    {
        get => (bool)GetValue(CanNavigateProperty);
        set => SetValue(CanNavigateProperty, value);
    }

    #endregion CanNavigate

    #region StatusChanged Event

    public static readonly RoutedEvent StatusChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(StatusChanged),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(StepperItem));

    /// <summary>
    /// Status 发生变化时触发(Pending / Current / Completed 之间切换)。
    /// </summary>
    public event RoutedEventHandler StatusChanged
    {
        add => AddHandler(StatusChangedEvent, value);
        remove => RemoveHandler(StatusChangedEvent, value);
    }

    #endregion StatusChanged Event

    #region Override Methods

    /// <summary>
    /// 鼠标左键按下——若 CanNavigate=true 则尝试导航到此步骤。
    /// </summary>
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (e.Handled)
        {
            return;
        }

        if (NavigateTo())
        {
            e.Handled = true;
        }
    }

    #endregion Override Methods

    #region Public API

    /// <summary>
    /// 导航到此步骤——若 CanNavigate=false 或未挂载到 Stepper,返回 false。
    /// </summary>
    public bool NavigateTo()
    {
        if (!CanNavigate)
        {
            return false;
        }

        if (ItemsControl.ItemsControlFromItemContainer(this) is Stepper parent && Index >= 0)
        {
            parent.CurrentIndex = Index;
            return true;
        }
        return false;
    }

    #endregion Public API
}