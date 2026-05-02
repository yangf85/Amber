using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 内容切换时播放过渡动画的容器控件。
/// <para>
/// 模板里维护两个 ContentPresenter（旧 / 新）：每次 Content 变化时，
/// 把上一份内容搬到旧 presenter，新内容放到新 presenter，然后由 <see cref="Transition"/> 创建动画驱动两者过渡。
/// </para>
/// <para>
/// 控件统一负责动画的启动、取消、清理；<see cref="ITransition"/> 实现只负责生产 Storyboard。
/// 这样快速连续切换 Content 时也能可靠地取消上一次动画、避免视觉残留。
/// </para>
/// </summary>
[TemplatePart(Name = PART_OldPresenter, Type = typeof(ContentPresenter))]
[TemplatePart(Name = PART_NewPresenter, Type = typeof(ContentPresenter))]
public class TransitionBox : ContentControl
{
    private const string PART_OldPresenter = nameof(PART_OldPresenter);
    private const string PART_NewPresenter = nameof(PART_NewPresenter);

    private ContentPresenter _oldPresenter;
    private ContentPresenter _newPresenter;
    private Storyboard _currentStoryboard;
    private bool _isTemplateApplied;

    static TransitionBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TransitionBox),
            new FrameworkPropertyMetadata(typeof(TransitionBox)));
    }

    #region Transition

    public static readonly DependencyProperty TransitionProperty =
        DependencyProperty.Register(
            nameof(Transition),
            typeof(ITransition),
            typeof(TransitionBox),
            new FrameworkPropertyMetadata(default(ITransition)));

    /// <summary>
    /// 内容切换时使用的过渡动画。为 null 时切换无动画。
    /// </summary>
    public ITransition Transition
    {
        get => (ITransition)GetValue(TransitionProperty);
        set => SetValue(TransitionProperty, value);
    }

    #endregion Transition

    #region TransitionDuration

    public static readonly DependencyProperty TransitionDurationProperty =
        DependencyProperty.Register(
            nameof(TransitionDuration),
            typeof(Duration),
            typeof(TransitionBox),
            new FrameworkPropertyMetadata(new Duration(System.TimeSpan.FromMilliseconds(300))));

    /// <summary>过渡动画时长。默认 300ms。</summary>
    public Duration TransitionDuration
    {
        get => (Duration)GetValue(TransitionDurationProperty);
        set => SetValue(TransitionDurationProperty, value);
    }

    #endregion TransitionDuration

    #region IsAnimating (只读)

    private static readonly DependencyPropertyKey IsAnimatingPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsAnimating),
            typeof(bool),
            typeof(TransitionBox),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsAnimatingProperty = IsAnimatingPropertyKey.DependencyProperty;

    /// <summary>
    /// 只读。当前是否处于过渡动画中。可绑定到 UI 状态（例如禁用切换按钮）。
    /// </summary>
    public bool IsAnimating
    {
        get => (bool)GetValue(IsAnimatingProperty);
        private set => SetValue(IsAnimatingPropertyKey, value);
    }

    #endregion IsAnimating

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _oldPresenter = GetTemplateChild(PART_OldPresenter) as ContentPresenter;
        _newPresenter = GetTemplateChild(PART_NewPresenter) as ContentPresenter;

        if (_oldPresenter is not null && _newPresenter is not null)
        {
            // 初始：旧 presenter 透明、新 presenter 显示当前 Content
            _oldPresenter.Opacity = 0;
            _oldPresenter.Content = null;

            _newPresenter.Opacity = 1;
            _newPresenter.Content = Content;

            _isTemplateApplied = true;
        }
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        // 模板还未应用：内容会在 OnApplyTemplate 里被同步到 newPresenter，这里不动
        if (!_isTemplateApplied)
        {
            return;
        }

        // 没有 transition 或没有旧内容：直接切换，不播动画
        if (Transition is null || oldContent is null)
        {
            _newPresenter.Content = newContent;
            return;
        }

        // 1. 取消正在进行的动画
        StopCurrentAnimation();

        // 2. 把"目前 newPresenter 显示的内容"挪到 oldPresenter（不一定等于 oldContent ——
        //    如果上一次动画被中途打断，newPresenter 此时显示的可能是更早的某一帧）
        _oldPresenter.Content = _newPresenter.Content;
        _newPresenter.Content = newContent;

        // 3. 重置两个 presenter 的视觉状态——清掉上一次动画可能留下的 RenderTransform / Opacity 异常
        ResetPresenterVisuals(_oldPresenter);
        ResetPresenterVisuals(_newPresenter);

        // 4. 强制布局更新一次，保证 ActualWidth/Height 在 Slide 等需要尺寸的动画里可用
        UpdateLayout();

        // 5. 由 Transition 创建动画，控件负责启动/订阅完成
        var storyboard = Transition.CreateAnimation(
            _oldPresenter,
            _newPresenter,
            new Size(ActualWidth, ActualHeight),
            TransitionDuration);

        if (storyboard is null)
        {
            // Transition 实现返回 null：当作"不动画"处理
            return;
        }

        storyboard.Completed += OnStoryboardCompleted;
        _currentStoryboard = storyboard;
        IsAnimating = true;

        // isControllable=true：允许之后用 Stop 取消
        storyboard.Begin(this, isControllable: true);
    }

    private void OnStoryboardCompleted(object sender, System.EventArgs e)
    {
        // 动画自然完成；如果是被 StopCurrentAnimation 取消的，订阅已被解除，不会走到这里
        FinishAnimation();
    }

    private void StopCurrentAnimation()
    {
        if (_currentStoryboard is null)
        {
            return;
        }
        _currentStoryboard.Completed -= OnStoryboardCompleted;
        _currentStoryboard.Stop(this);
        _currentStoryboard = null;
    }

    private void FinishAnimation()
    {
        // 动画完成后把 oldPresenter 清空，并把视觉状态恢复
        if (_oldPresenter is not null)
        {
            _oldPresenter.Content = null;
            ResetPresenterVisuals(_oldPresenter);
            _oldPresenter.Opacity = 0;
        }
        if (_newPresenter is not null)
        {
            ResetPresenterVisuals(_newPresenter);
            _newPresenter.Opacity = 1;
        }

        _currentStoryboard = null;
        IsAnimating = false;
    }

    /// <summary>
    /// 清掉 presenter 上一次动画留下的痕迹：RenderTransform 残留、RenderTransformOrigin、Opacity 中间值。
    /// 不动 Content（由调用方决定）。
    /// </summary>
    private static void ResetPresenterVisuals(ContentPresenter presenter)
    {
        // ClearValue 而不是赋 null：让 DP 回到默认值，避免覆盖 Style / Template 里的 setter
        presenter.ClearValue(UIElement.RenderTransformProperty);
        presenter.ClearValue(UIElement.RenderTransformOriginProperty);
        presenter.ClearValue(UIElement.OpacityProperty);
    }
}
