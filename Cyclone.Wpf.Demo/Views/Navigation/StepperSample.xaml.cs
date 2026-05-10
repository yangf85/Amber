using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// StepperView.xaml 的交互逻辑
/// </summary>
public partial class StepperSample : UserControl
{
    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        HorizontalStepper.MovePrevious();
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        //这里只控制垂直Stepper，也可以同时控制两个
        HorizontalStepper.MoveNext();
    }

    public StepperSample()
    {
        InitializeComponent();
        DataContext = new StepperViewModel();
    }
}

public partial class StepperViewModel : ObservableObject
{
    [ObservableProperty]
    public partial int CurrentStepIndex { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<StepperItemViewModel> StepperItems { get; set; }

    [RelayCommand]
    private void Next()
    {
        if (CurrentStepIndex < StepperItems.Count)
        {
            CurrentStepIndex++;
        }
    }

    [RelayCommand]
    private void Previous()
    {
        if (CurrentStepIndex > 0)
        {
            CurrentStepIndex--;
        }
    }

    public StepperViewModel()
    {
        // 初始化步骤项集合
        StepperItems =
        [
            new StepperItemViewModel
            {
                Header = "需求分析",
                Description = "收集和分析用户需求",
            },
            new StepperItemViewModel
            {
                Header = "设计方案",
                Description = "制定系统设计方案",
            },
            new StepperItemViewModel
            {
                Header = "编码实现",
                Description = "实现系统功能",
            },
            new StepperItemViewModel
            {
                Header = "测试验证",
                Description = "进行系统测试",
            },
            new StepperItemViewModel
            {
                Header = "部署上线",
                Description = "系统部署和上线",
            }
        ];
    }
}

/// <summary>
/// 步骤项视图模型
/// </summary>
public partial class StepperItemViewModel : ObservableObject
{
    /// <summary>
    /// 步骤标题
    /// </summary>
    [ObservableProperty]
    private string _header;

    /// <summary>
    /// 步骤描述
    /// </summary>
    [ObservableProperty]
    private string _description;
}