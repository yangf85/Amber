using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// SectionHeader 控件：用于显示带有主标题、子标题和分隔线的区域标题
/// </summary>
public class SectionHeader : Control
{
    #region MainTitle

    public static readonly DependencyProperty MainTitleProperty =
        DependencyProperty.Register(nameof(MainTitle), typeof(object), typeof(SectionHeader), new PropertyMetadata(default));

    public object MainTitle
    {
        get => (object)GetValue(MainTitleProperty);
        set => SetValue(MainTitleProperty, value);
    }

    #endregion MainTitle

    #region MainTitleHeight

    public static readonly DependencyProperty MainTitleHeightProperty =
        DependencyProperty.Register(nameof(MainTitleHeight), typeof(GridLength), typeof(SectionHeader), new PropertyMetadata(new GridLength(1, GridUnitType.Star)));

    public GridLength MainTitleHeight
    {
        get => (GridLength)GetValue(MainTitleHeightProperty);
        set => SetValue(MainTitleHeightProperty, value);
    }

    #endregion MainTitleHeight

    #region MainTitleFontSize

    public static readonly DependencyProperty MainTitleFontSizeProperty =
        DependencyProperty.Register(nameof(MainTitleFontSize), typeof(double), typeof(SectionHeader), new PropertyMetadata(16.0));

    public double MainTitleFontSize
    {
        get => (double)GetValue(MainTitleFontSizeProperty);
        set => SetValue(MainTitleFontSizeProperty, value);
    }

    #endregion MainTitleFontSize

    #region MainTitleHorizontalAlignment

    public static readonly DependencyProperty MainTitleHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(MainTitleHorizontalAlignment), typeof(HorizontalAlignment), typeof(SectionHeader), new PropertyMetadata(HorizontalAlignment.Center));

    public HorizontalAlignment MainTitleHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(MainTitleHorizontalAlignmentProperty);
        set => SetValue(MainTitleHorizontalAlignmentProperty, value);
    }

    #endregion MainTitleHorizontalAlignment

    #region MainTitleVerticalAlignment

    public static readonly DependencyProperty MainTitleVerticalAlignmentProperty =
        DependencyProperty.Register(nameof(MainTitleVerticalAlignment), typeof(VerticalAlignment), typeof(SectionHeader), new PropertyMetadata(VerticalAlignment.Center));

    public VerticalAlignment MainTitleVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(MainTitleVerticalAlignmentProperty);
        set => SetValue(MainTitleVerticalAlignmentProperty, value);
    }

    #endregion MainTitleVerticalAlignment

    #region MainTitleMargin

    public static readonly DependencyProperty MainTitleMarginProperty =
        DependencyProperty.Register(nameof(MainTitleMargin), typeof(Thickness), typeof(SectionHeader), new PropertyMetadata(new Thickness(0)));

    public Thickness MainTitleMargin
    {
        get => (Thickness)GetValue(MainTitleMarginProperty);
        set => SetValue(MainTitleMarginProperty, value);
    }

    #endregion MainTitleMargin

    #region MainTitleFontFamily

    public static readonly DependencyProperty MainTitleFontFamilyProperty =
        DependencyProperty.Register(nameof(MainTitleFontFamily), typeof(FontFamily), typeof(SectionHeader), new PropertyMetadata(default));

    public FontFamily MainTitleFontFamily
    {
        get => (FontFamily)GetValue(MainTitleFontFamilyProperty);
        set => SetValue(MainTitleFontFamilyProperty, value);
    }

    #endregion MainTitleFontFamily

    #region MainTitleFontWeight

    public static readonly DependencyProperty MainTitleFontWeightProperty =
        DependencyProperty.Register(nameof(MainTitleFontWeight), typeof(FontWeight), typeof(SectionHeader), new PropertyMetadata(FontWeights.Bold));

    public FontWeight MainTitleFontWeight
    {
        get => (FontWeight)GetValue(MainTitleFontWeightProperty);
        set => SetValue(MainTitleFontWeightProperty, value);
    }

    #endregion MainTitleFontWeight

    #region MainTitleForeground

    public static readonly DependencyProperty MainTitleForegroundProperty =
        DependencyProperty.Register(nameof(MainTitleForeground), typeof(Brush), typeof(SectionHeader), new PropertyMetadata(null));

    public Brush MainTitleForeground
    {
        get => (Brush)GetValue(MainTitleForegroundProperty);
        set => SetValue(MainTitleForegroundProperty, value);
    }

    #endregion MainTitleForeground

    #region MainTitleBackground

    public static readonly DependencyProperty MainTitleBackgroundProperty =
        DependencyProperty.Register(nameof(MainTitleBackground), typeof(Brush), typeof(SectionHeader), new PropertyMetadata(Brushes.Transparent));

    public Brush MainTitleBackground
    {
        get => (Brush)GetValue(MainTitleBackgroundProperty);
        set => SetValue(MainTitleBackgroundProperty, value);
    }

    #endregion MainTitleBackground

    #region SubTitle

    public static readonly DependencyProperty SubTitleProperty =
        DependencyProperty.Register(nameof(SubTitle), typeof(object), typeof(SectionHeader), new PropertyMetadata(default));

    public object SubTitle
    {
        get => (object)GetValue(SubTitleProperty);
        set => SetValue(SubTitleProperty, value);
    }

    #endregion SubTitle

    #region SubTitleHeight

    public static readonly DependencyProperty SubTitleHeightProperty =
        DependencyProperty.Register(nameof(SubTitleHeight), typeof(GridLength), typeof(SectionHeader), new PropertyMetadata(GridLength.Auto));

    public GridLength SubTitleHeight
    {
        get => (GridLength)GetValue(SubTitleHeightProperty);
        set => SetValue(SubTitleHeightProperty, value);
    }

    #endregion SubTitleHeight

    #region SubTitleFontSize

    public static readonly DependencyProperty SubTitleFontSizeProperty =
        DependencyProperty.Register(nameof(SubTitleFontSize), typeof(double), typeof(SectionHeader), new PropertyMetadata(12.0));

    public double SubTitleFontSize
    {
        get => (double)GetValue(SubTitleFontSizeProperty);
        set => SetValue(SubTitleFontSizeProperty, value);
    }

    #endregion SubTitleFontSize

    #region SubTitleHorizontalAlignment

    public static readonly DependencyProperty SubTitleHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(SubTitleHorizontalAlignment), typeof(HorizontalAlignment), typeof(SectionHeader), new PropertyMetadata(HorizontalAlignment.Center));

    public HorizontalAlignment SubTitleHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(SubTitleHorizontalAlignmentProperty);
        set => SetValue(SubTitleHorizontalAlignmentProperty, value);
    }

    #endregion SubTitleHorizontalAlignment

    #region SubTitleVerticalAlignment

    public static readonly DependencyProperty SubTitleVerticalAlignmentProperty =
        DependencyProperty.Register(nameof(SubTitleVerticalAlignment), typeof(VerticalAlignment), typeof(SectionHeader), new PropertyMetadata(VerticalAlignment.Center));

    public VerticalAlignment SubTitleVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(SubTitleVerticalAlignmentProperty);
        set => SetValue(SubTitleVerticalAlignmentProperty, value);
    }

    #endregion SubTitleVerticalAlignment

    #region SubTitleMargin

    public static readonly DependencyProperty SubTitleMarginProperty =
        DependencyProperty.Register(nameof(SubTitleMargin), typeof(Thickness), typeof(SectionHeader), new PropertyMetadata(new Thickness(0)));

    public Thickness SubTitleMargin
    {
        get => (Thickness)GetValue(SubTitleMarginProperty);
        set => SetValue(SubTitleMarginProperty, value);
    }

    #endregion SubTitleMargin

    #region SubTitleFontFamily

    public static readonly DependencyProperty SubTitleFontFamilyProperty =
        DependencyProperty.Register(nameof(SubTitleFontFamily), typeof(FontFamily), typeof(SectionHeader), new PropertyMetadata(default));

    public FontFamily SubTitleFontFamily
    {
        get => (FontFamily)GetValue(SubTitleFontFamilyProperty);
        set => SetValue(SubTitleFontFamilyProperty, value);
    }

    #endregion SubTitleFontFamily

    #region SubTitleFontWeight

    public static readonly DependencyProperty SubTitleFontWeightProperty =
        DependencyProperty.Register(nameof(SubTitleFontWeight), typeof(FontWeight), typeof(SectionHeader), new PropertyMetadata(FontWeights.Normal));

    public FontWeight SubTitleFontWeight
    {
        get => (FontWeight)GetValue(SubTitleFontWeightProperty);
        set => SetValue(SubTitleFontWeightProperty, value);
    }

    #endregion SubTitleFontWeight

    #region SubTitleForeground

    public static readonly DependencyProperty SubTitleForegroundProperty =
        DependencyProperty.Register(nameof(SubTitleForeground), typeof(Brush), typeof(SectionHeader), new PropertyMetadata(null));

    public Brush SubTitleForeground
    {
        get => (Brush)GetValue(SubTitleForegroundProperty);
        set => SetValue(SubTitleForegroundProperty, value);
    }

    #endregion SubTitleForeground

    #region SubTitleBackground

    public static readonly DependencyProperty SubTitleBackgroundProperty =
        DependencyProperty.Register(nameof(SubTitleBackground), typeof(Brush), typeof(SectionHeader), new PropertyMetadata(Brushes.Transparent));

    public Brush SubTitleBackground
    {
        get => (Brush)GetValue(SubTitleBackgroundProperty);
        set => SetValue(SubTitleBackgroundProperty, value);
    }

    #endregion SubTitleBackground

    #region SubTitleVisibility

    public static readonly DependencyProperty SubTitleVisibilityProperty =
        DependencyProperty.Register(nameof(SubTitleVisibility), typeof(Visibility), typeof(SectionHeader),
            new PropertyMetadata(Visibility.Collapsed, OnSubTitleVisibilityChanged));

    public Visibility SubTitleVisibility
    {
        get => (Visibility)GetValue(SubTitleVisibilityProperty);
        set => SetValue(SubTitleVisibilityProperty, value);
    }

    private static void OnSubTitleVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 可以在此添加子标题可见性变化的处理逻辑
    }

    #endregion SubTitleVisibility

    #region SeparatorThickness

    public static readonly DependencyProperty SeparatorThicknessProperty =
        DependencyProperty.Register(nameof(SeparatorThickness), typeof(double), typeof(SectionHeader), new PropertyMetadata(1.0));

    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }

    #endregion SeparatorThickness

    #region SeparatorBrush

    public static readonly DependencyProperty SeparatorBrushProperty =
        DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush), typeof(SectionHeader), new PropertyMetadata(null));

    public Brush SeparatorBrush
    {
        get => (Brush)GetValue(SeparatorBrushProperty);
        set => SetValue(SeparatorBrushProperty, value);
    }

    #endregion SeparatorBrush

    #region SeparatorMargin

    public static readonly DependencyProperty SeparatorMarginProperty =
        DependencyProperty.Register(nameof(SeparatorMargin), typeof(Thickness), typeof(SectionHeader),
            new PropertyMetadata(new Thickness(0, 5, 0, 5)));

    public Thickness SeparatorMargin
    {
        get => (Thickness)GetValue(SeparatorMarginProperty);
        set => SetValue(SeparatorMarginProperty, value);
    }

    #endregion SeparatorMargin

    #region SeparatorVisibility

    public static readonly DependencyProperty SeparatorVisibilityProperty =
        DependencyProperty.Register(nameof(SeparatorVisibility), typeof(Visibility), typeof(SectionHeader),
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility SeparatorVisibility
    {
        get => (Visibility)GetValue(SeparatorVisibilityProperty);
        set => SetValue(SeparatorVisibilityProperty, value);
    }

    #endregion SeparatorVisibility

    #region IsUseUnifiedBackground

    public static readonly DependencyProperty IsUseUnifiedBackgroundProperty =
        DependencyProperty.Register(nameof(IsUseUnifiedBackground), typeof(bool), typeof(SectionHeader), new PropertyMetadata(default(bool)));

    public bool IsUseUnifiedBackground
    {
        get => (bool)GetValue(IsUseUnifiedBackgroundProperty);
        set => SetValue(IsUseUnifiedBackgroundProperty, value);
    }

    #endregion IsUseUnifiedBackground

    static SectionHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SectionHeader), new FrameworkPropertyMetadata(typeof(SectionHeader)));
    }
}