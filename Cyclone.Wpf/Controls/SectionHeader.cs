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

    public object MainTitle
    {
        get => (object)GetValue(MainTitleProperty);
        set => SetValue(MainTitleProperty, value);
    }

    public static readonly DependencyProperty MainTitleProperty =
        DependencyProperty.Register(nameof(MainTitle), typeof(object), typeof(SectionHeader), new PropertyMetadata(default));

    #endregion MainTitle

    #region MainTitleHeight

    public GridLength MainTitleHeight
    {
        get => (GridLength)GetValue(MainTitleHeightProperty);
        set => SetValue(MainTitleHeightProperty, value);
    }

    public static readonly DependencyProperty MainTitleHeightProperty =
        DependencyProperty.Register(nameof(MainTitleHeight), typeof(GridLength), typeof(SectionHeader), new PropertyMetadata(new GridLength(1, GridUnitType.Star)));

    #endregion MainTitleHeight

    #region MainTitleFontSize

    public double MainTitleFontSize
    {
        get => (double)GetValue(MainTitleFontSizeProperty);
        set => SetValue(MainTitleFontSizeProperty, value);
    }

    public static readonly DependencyProperty MainTitleFontSizeProperty =
        DependencyProperty.Register(nameof(MainTitleFontSize), typeof(double), typeof(SectionHeader), new PropertyMetadata(16.0));

    #endregion MainTitleFontSize

    #region MainTitleHorizontalAlignment

    public HorizontalAlignment MainTitleHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(MainTitleHorizontalAlignmentProperty);
        set => SetValue(MainTitleHorizontalAlignmentProperty, value);
    }

    public static readonly DependencyProperty MainTitleHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(MainTitleHorizontalAlignment), typeof(HorizontalAlignment), typeof(SectionHeader), new PropertyMetadata(HorizontalAlignment.Center));

    #endregion MainTitleHorizontalAlignment

    #region MainTitleVerticalAlignment

    public VerticalAlignment MainTitleVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(MainTitleVerticalAlignmentProperty);
        set => SetValue(MainTitleVerticalAlignmentProperty, value);
    }

    public static readonly DependencyProperty MainTitleVerticalAlignmentProperty =
        DependencyProperty.Register(nameof(MainTitleVerticalAlignment), typeof(VerticalAlignment), typeof(SectionHeader), new PropertyMetadata(VerticalAlignment.Center));

    #endregion MainTitleVerticalAlignment

    #region MainTitleMargin

    public Thickness MainTitleMargin
    {
        get => (Thickness)GetValue(MainTitleMarginProperty);
        set => SetValue(MainTitleMarginProperty, value);
    }

    public static readonly DependencyProperty MainTitleMarginProperty =
        DependencyProperty.Register(nameof(MainTitleMargin), typeof(Thickness), typeof(SectionHeader), new PropertyMetadata(new Thickness(0)));

    #endregion MainTitleMargin

    #region MainTitleFontFamily

    public FontFamily MainTitleFontFamily
    {
        get => (FontFamily)GetValue(MainTitleFontFamilyProperty);
        set => SetValue(MainTitleFontFamilyProperty, value);
    }

    public static readonly DependencyProperty MainTitleFontFamilyProperty =
        DependencyProperty.Register(nameof(MainTitleFontFamily), typeof(FontFamily), typeof(SectionHeader), new PropertyMetadata(default));

    #endregion MainTitleFontFamily

    #region MainTitleFontWeight

    public FontWeight MainTitleFontWeight
    {
        get => (FontWeight)GetValue(MainTitleFontWeightProperty);
        set => SetValue(MainTitleFontWeightProperty, value);
    }

    public static readonly DependencyProperty MainTitleFontWeightProperty =
        DependencyProperty.Register(nameof(MainTitleFontWeight), typeof(FontWeight), typeof(SectionHeader), new PropertyMetadata(FontWeights.Bold));

    #endregion MainTitleFontWeight

    #region MainTitleForeground

    public Brush MainTitleForeground
    {
        get => (Brush)GetValue(MainTitleForegroundProperty);
        set => SetValue(MainTitleForegroundProperty, value);
    }

    public static readonly DependencyProperty MainTitleForegroundProperty =
        DependencyProperty.Register(nameof(MainTitleForeground), typeof(Brush), typeof(SectionHeader), new PropertyMetadata(null));

    #endregion MainTitleForeground

    #region MainTitleBackground

    public Brush MainTitleBackground
    {
        get => (Brush)GetValue(MainTitleBackgroundProperty);
        set => SetValue(MainTitleBackgroundProperty, value);
    }

    public static readonly DependencyProperty MainTitleBackgroundProperty =
        DependencyProperty.Register(nameof(MainTitleBackground), typeof(Brush), typeof(SectionHeader), new PropertyMetadata(Brushes.Transparent));

    #endregion MainTitleBackground

    #region SubTitle

    public object SubTitle
    {
        get => (object)GetValue(SubTitleProperty);
        set => SetValue(SubTitleProperty, value);
    }

    public static readonly DependencyProperty SubTitleProperty =
        DependencyProperty.Register(nameof(SubTitle), typeof(object), typeof(SectionHeader), new PropertyMetadata(default));

    #endregion SubTitle

    #region SubTitleHeight

    public GridLength SubTitleHeight
    {
        get => (GridLength)GetValue(SubTitleHeightProperty);
        set => SetValue(SubTitleHeightProperty, value);
    }

    public static readonly DependencyProperty SubTitleHeightProperty =
        DependencyProperty.Register(nameof(SubTitleHeight), typeof(GridLength), typeof(SectionHeader), new PropertyMetadata(GridLength.Auto));

    #endregion SubTitleHeight

    #region SubTitleFontSize

    public double SubTitleFontSize
    {
        get => (double)GetValue(SubTitleFontSizeProperty);
        set => SetValue(SubTitleFontSizeProperty, value);
    }

    public static readonly DependencyProperty SubTitleFontSizeProperty =
        DependencyProperty.Register(nameof(SubTitleFontSize), typeof(double), typeof(SectionHeader), new PropertyMetadata(12.0));

    #endregion SubTitleFontSize

    #region SubTitleHorizontalAlignment

    public HorizontalAlignment SubTitleHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(SubTitleHorizontalAlignmentProperty);
        set => SetValue(SubTitleHorizontalAlignmentProperty, value);
    }

    public static readonly DependencyProperty SubTitleHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(SubTitleHorizontalAlignment), typeof(HorizontalAlignment), typeof(SectionHeader), new PropertyMetadata(HorizontalAlignment.Center));

    #endregion SubTitleHorizontalAlignment

    #region SubTitleVerticalAlignment

    public VerticalAlignment SubTitleVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(SubTitleVerticalAlignmentProperty);
        set => SetValue(SubTitleVerticalAlignmentProperty, value);
    }

    public static readonly DependencyProperty SubTitleVerticalAlignmentProperty =
        DependencyProperty.Register(nameof(SubTitleVerticalAlignment), typeof(VerticalAlignment), typeof(SectionHeader), new PropertyMetadata(VerticalAlignment.Center));

    #endregion SubTitleVerticalAlignment

    #region SubTitleMargin

    public Thickness SubTitleMargin
    {
        get => (Thickness)GetValue(SubTitleMarginProperty);
        set => SetValue(SubTitleMarginProperty, value);
    }

    public static readonly DependencyProperty SubTitleMarginProperty =
        DependencyProperty.Register(nameof(SubTitleMargin), typeof(Thickness), typeof(SectionHeader), new PropertyMetadata(new Thickness(0)));

    #endregion SubTitleMargin

    #region SubTitleFontFamily

    public FontFamily SubTitleFontFamily
    {
        get => (FontFamily)GetValue(SubTitleFontFamilyProperty);
        set => SetValue(SubTitleFontFamilyProperty, value);
    }

    public static readonly DependencyProperty SubTitleFontFamilyProperty =
        DependencyProperty.Register(nameof(SubTitleFontFamily), typeof(FontFamily), typeof(SectionHeader), new PropertyMetadata(default));

    #endregion SubTitleFontFamily

    #region SubTitleFontWeight

    public FontWeight SubTitleFontWeight
    {
        get => (FontWeight)GetValue(SubTitleFontWeightProperty);
        set => SetValue(SubTitleFontWeightProperty, value);
    }

    public static readonly DependencyProperty SubTitleFontWeightProperty =
        DependencyProperty.Register(nameof(SubTitleFontWeight), typeof(FontWeight), typeof(SectionHeader), new PropertyMetadata(FontWeights.Normal));

    #endregion SubTitleFontWeight

    #region SubTitleForeground

    public Brush SubTitleForeground
    {
        get => (Brush)GetValue(SubTitleForegroundProperty);
        set => SetValue(SubTitleForegroundProperty, value);
    }

    public static readonly DependencyProperty SubTitleForegroundProperty =
        DependencyProperty.Register(nameof(SubTitleForeground), typeof(Brush), typeof(SectionHeader), new PropertyMetadata(null));

    #endregion SubTitleForeground

    #region SubTitleBackground

    public Brush SubTitleBackground
    {
        get => (Brush)GetValue(SubTitleBackgroundProperty);
        set => SetValue(SubTitleBackgroundProperty, value);
    }

    public static readonly DependencyProperty SubTitleBackgroundProperty =
        DependencyProperty.Register(nameof(SubTitleBackground), typeof(Brush), typeof(SectionHeader), new PropertyMetadata(Brushes.Transparent));

    #endregion SubTitleBackground

    #region SubTitleVisibility

    public Visibility SubTitleVisibility
    {
        get => (Visibility)GetValue(SubTitleVisibilityProperty);
        set => SetValue(SubTitleVisibilityProperty, value);
    }

    public static readonly DependencyProperty SubTitleVisibilityProperty =
        DependencyProperty.Register(nameof(SubTitleVisibility), typeof(Visibility), typeof(SectionHeader),
            new PropertyMetadata(Visibility.Collapsed, OnSubTitleVisibilityChanged));

    private static void OnSubTitleVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 可以在此添加子标题可见性变化的处理逻辑
    }

    #endregion SubTitleVisibility

    #region SeparatorThickness

    public double SeparatorThickness
    {
        get => (double)GetValue(SeparatorThicknessProperty);
        set => SetValue(SeparatorThicknessProperty, value);
    }

    public static readonly DependencyProperty SeparatorThicknessProperty =
        DependencyProperty.Register(nameof(SeparatorThickness), typeof(double), typeof(SectionHeader), new PropertyMetadata(1.0));

    #endregion SeparatorThickness

    #region SeparatorBrush

    public Brush SeparatorBrush
    {
        get => (Brush)GetValue(SeparatorBrushProperty);
        set => SetValue(SeparatorBrushProperty, value);
    }

    public static readonly DependencyProperty SeparatorBrushProperty =
        DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush), typeof(SectionHeader), new PropertyMetadata(null));

    #endregion SeparatorBrush

    #region SeparatorMargin

    public Thickness SeparatorMargin
    {
        get => (Thickness)GetValue(SeparatorMarginProperty);
        set => SetValue(SeparatorMarginProperty, value);
    }

    public static readonly DependencyProperty SeparatorMarginProperty =
        DependencyProperty.Register(nameof(SeparatorMargin), typeof(Thickness), typeof(SectionHeader),
            new PropertyMetadata(new Thickness(0, 5, 0, 5)));

    #endregion SeparatorMargin

    #region SeparatorVisibility

    public Visibility SeparatorVisibility
    {
        get => (Visibility)GetValue(SeparatorVisibilityProperty);
        set => SetValue(SeparatorVisibilityProperty, value);
    }

    public static readonly DependencyProperty SeparatorVisibilityProperty =
        DependencyProperty.Register(nameof(SeparatorVisibility), typeof(Visibility), typeof(SectionHeader),
            new PropertyMetadata(Visibility.Collapsed));

    #endregion SeparatorVisibility

    #region IsUseUnifiedBackground

    public bool IsUseUnifiedBackground
    {
        get => (bool)GetValue(IsUseUnifiedBackgroundProperty);
        set => SetValue(IsUseUnifiedBackgroundProperty, value);
    }

    public static readonly DependencyProperty IsUseUnifiedBackgroundProperty =
        DependencyProperty.Register(nameof(IsUseUnifiedBackground), typeof(bool), typeof(SectionHeader), new PropertyMetadata(default(bool)));

    #endregion IsUseUnifiedBackground

    static SectionHeader()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SectionHeader), new FrameworkPropertyMetadata(typeof(SectionHeader)));
    }
}