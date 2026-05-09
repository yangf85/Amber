using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 加载指示器基类。提供 <see cref="IsActive"/> 依赖属性 + <see cref="OnIsActiveChanged"/> 虚方法，
/// 子类通过重写虚方法实现动画启停。
/// <para>
/// <b>子类不要重新 Register IsActiveProperty</b>——会 shadow 掉基类 DP 导致 LoadingBox 给基类 DP
/// 设的 binding 完全失效。子类只需 <see langword="override"/> <see cref="OnIsActiveChanged"/>。
/// </para>
/// </summary>
public abstract class LoadingIndicator : ContentControl
{
    #region IsActive

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(LoadingIndicator),
            new PropertyMetadata(true, OnIsActiveChanged));

    /// <summary>
    /// 是否激活动画。默认 <see langword="true"/>——单独使用 indicator 时一加入 visual tree 就转动；
    /// 通过 <see cref="LoadingBox"/> 使用时会被 binding 覆盖为 <see cref="LoadingBox.IsLoading"/> 的值。
    /// </summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var indicator = (LoadingIndicator)d;
        indicator.OnIsActiveChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    #endregion IsActive

    /// <summary>
    /// IsActive 改变时调用。子类重写此方法启停动画。
    /// </summary>
    protected virtual void OnIsActiveChanged(bool oldValue, bool newValue)
    {
        // 子类启停动画
    }
}
