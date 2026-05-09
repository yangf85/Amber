// ============================================================================
//  破坏性变更说明（vs 旧 LoadingBox）：
//    - LoadingContent DP 类型 ILoadingIndicator → LoadingIndicator
//      原因：ILoadingIndicator 接口的 IsActive 不是 DP，无法 binding。改成具体基类后
//      可以让 LoadingBox 内部正确给 IsActive 设 binding。
//      迁移：把自定义 indicator 改为继承 LoadingIndicator 而不是实现 ILoadingIndicator
//      （ILoadingIndicator 接口已删除）。
// ============================================================================
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 加载遮罩控件——在内容上叠加 mask + 加载指示器。<br/>
/// IsLoading 切换控制 mask 与 indicator 的显隐，indicator 通过内部 binding 自动启停动画。
/// </summary>
[TemplatePart(Name = PartMask, Type = typeof(Rectangle))]
[TemplatePart(Name = PartLoadingPresenter, Type = typeof(ContentPresenter))]
public class LoadingBox : ContentControl
{
    private const string PartMask = nameof(PartMask);
    private const string PartLoadingPresenter = nameof(PartLoadingPresenter);

    private static readonly Brush DefaultMaskBackground = CreateFrozenMaskBrush();

    static LoadingBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingBox),
            new FrameworkPropertyMetadata(typeof(LoadingBox)));
    }

    private static Brush CreateFrozenMaskBrush()
    {
        // 半透明黑遮罩（#80000000）。frozen 后可跨实例共享，避免每个 LoadingBox new 一个
        var brush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
        brush.Freeze();
        return brush;
    }

    #region IsLoading

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(LoadingBox),
            new PropertyMetadata(false));

    /// <summary>是否正在加载——true 时显示 mask 和 indicator。</summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    #endregion IsLoading

    #region LoadingContent

    public static readonly DependencyProperty LoadingContentProperty =
        DependencyProperty.Register(
            nameof(LoadingContent),
            typeof(LoadingIndicator),
            typeof(LoadingBox),
            new PropertyMetadata(null, OnLoadingContentChanged));

    /// <summary>
    /// 加载指示器实例。<br/>
    /// <b>不要</b>通过 default Style 的 Setter.Value 设默认 LoadingIndicator——多个 LoadingBox
    /// 会共享同一个 Visual instance 抛 "Specified Visual is already a child" 异常。
    /// 默认行为：不设此 DP 时，<see cref="OnApplyTemplate"/> 会自动 new 一个 <see cref="LoadingRing"/>。
    /// </summary>
    public LoadingIndicator LoadingContent
    {
        get => (LoadingIndicator)GetValue(LoadingContentProperty);
        set => SetValue(LoadingContentProperty, value);
    }

    private static void OnLoadingContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var loadingBox = (LoadingBox)d;

        // 旧 indicator：解绑 + 显式停止动画（避免被换出后还在转）
        if (e.OldValue is LoadingIndicator oldIndicator)
        {
            BindingOperations.ClearBinding(oldIndicator, LoadingIndicator.IsActiveProperty);
            oldIndicator.IsActive = false;
        }

        // 新 indicator：把 IsActive 绑到 LoadingBox.IsLoading
        if (e.NewValue is LoadingIndicator newIndicator)
        {
            var binding = new Binding(nameof(IsLoading))
            {
                Source = loadingBox,
                Mode = BindingMode.OneWay,
            };
            BindingOperations.SetBinding(newIndicator, LoadingIndicator.IsActiveProperty, binding);
        }
    }

    #endregion LoadingContent

    #region MaskBackground

    public static readonly DependencyProperty MaskBackgroundProperty =
        DependencyProperty.Register(
            nameof(MaskBackground),
            typeof(Brush),
            typeof(LoadingBox),
            new PropertyMetadata(DefaultMaskBackground));

    /// <summary>遮罩层背景 brush。默认半透明黑（#80000000），frozen。</summary>
    public Brush MaskBackground
    {
        get => (Brush)GetValue(MaskBackgroundProperty);
        set => SetValue(MaskBackgroundProperty, value);
    }

    #endregion MaskBackground

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 如果 user 没设 LoadingContent，给一个默认 LoadingRing 实例。
        // 不在 default Style 里设——会导致多 LoadingBox 共享同一 Visual instance 崩溃。
        if (LoadingContent == null)
        {
            SetCurrentValue(LoadingContentProperty, new LoadingRing());
        }
    }
}
