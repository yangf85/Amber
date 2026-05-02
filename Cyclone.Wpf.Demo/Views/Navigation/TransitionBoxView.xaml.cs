using Cyclone.Wpf.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// TransitionBoxView.xaml 的交互逻辑
/// </summary>
public partial class TransitionBoxView : UserControl
{
    // 各对比小卡当前页面索引（0=Page1 / 1=Page2 / 2=Page3）
    private int _fadeIndex = 0;
    private int _slideIndex = 1;
    private int _scaleIndex = 2;
    private int _flipIndex = 0;

    public TransitionBoxView()
    {
        InitializeComponent();
    }

    #region 主交互区：Page 切换

    private void Page1_Checked(object sender, RoutedEventArgs e)
    {
        if (MyTransitionBox is null) return;
        MyTransitionBox.Content = Resources["Page1"];
    }

    private void Page2_Checked(object sender, RoutedEventArgs e)
    {
        if (MyTransitionBox is null) return;
        MyTransitionBox.Content = Resources["Page2"];
    }

    private void Page3_Checked(object sender, RoutedEventArgs e)
    {
        if (MyTransitionBox is null) return;
        MyTransitionBox.Content = Resources["Page3"];
    }

    #endregion 主交互区：Page 切换

    #region 主交互区：过渡选择

    private void TransitionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MyTransitionBox is null) return;

        // 按 ComboBox.SelectedIndex 创建新的 transition 实例
        // 顺序与 XAML 里的 ComboBoxItem 一一对应
        MyTransitionBox.Transition = TransitionSelector.SelectedIndex switch
        {
            0 => new FadeTransition(),
            1 => new SlideTransition { Direction = SlideDirection.RightToLeft },
            2 => new SlideTransition { Direction = SlideDirection.LeftToRight },
            3 => new SlideTransition { Direction = SlideDirection.TopToBottom },
            4 => new SlideTransition { Direction = SlideDirection.BottomToTop },
            5 => new ScaleTransition(),
            6 => new FlipTransition { Orientation = Orientation.Horizontal },
            7 => new FlipTransition { Orientation = Orientation.Vertical },
            _ => new FadeTransition(),
        };
    }

    #endregion 主交互区：过渡选择

    #region 主交互区：时长

    private void DurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (MyTransitionBox is null) return;
        MyTransitionBox.TransitionDuration = new Duration(TimeSpan.FromMilliseconds(DurationSlider.Value));
    }

    #endregion 主交互区：时长

    #region 对比小卡：切换按钮

    private void CycleFade_Click(object sender, RoutedEventArgs e)
    {
        _fadeIndex = (_fadeIndex + 1) % 3;
        FadeBox.Content = Resources[$"Page{_fadeIndex + 1}"];
    }

    private void CycleSlide_Click(object sender, RoutedEventArgs e)
    {
        _slideIndex = (_slideIndex + 1) % 3;
        SlideBox.Content = Resources[$"Page{_slideIndex + 1}"];
    }

    private void CycleScale_Click(object sender, RoutedEventArgs e)
    {
        _scaleIndex = (_scaleIndex + 1) % 3;
        ScaleBox.Content = Resources[$"Page{_scaleIndex + 1}"];
    }

    private void CycleFlip_Click(object sender, RoutedEventArgs e)
    {
        _flipIndex = (_flipIndex + 1) % 3;
        FlipBox.Content = Resources[$"Page{_flipIndex + 1}"];
    }

    #endregion 对比小卡：切换按钮
}
