using System;
using System.Collections;
using System.Linq;
using System.Windows;

namespace Cyclone.Wpf.Converters;

public class VisibilityConverter
{
    /// <summary>
    /// 将布尔值转换为可见性状态：true时显示，false时隐藏，null时不可见但占位
    /// </summary>
    public static FuncValueConverter<bool?, Visibility> VisibleWhenTrue { get; } =
        new(b =>
        {
            return b switch
            {
                true => Visibility.Visible,
                false => Visibility.Collapsed,
                _ => Visibility.Hidden
            };
        });

    /// <summary>
    /// 将布尔值转换为可见性状态：false时显示，true时隐藏，null时不可见但占位
    /// </summary>
    public static FuncValueConverter<bool?, Visibility> VisibleWhenFalse { get; } =
        new(b =>
        {
            return b switch
            {
                true => Visibility.Collapsed,
                false => Visibility.Visible,
                _ => Visibility.Hidden
            };
        });

    /// <summary>
    /// 将字符串转换为可见性状态：当字符串为null或空字符串时显示，否则隐藏
    /// </summary>
    public static FuncValueConverter<string, Visibility> VisibleWhenNullOrEmpty { get; } =
        new(s => string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed);

    /// <summary>
    /// 将字符串转换为可见性状态：当字符串不为null且不为空字符串时显示，否则隐藏
    /// </summary>
    public static FuncValueConverter<string, Visibility> VisibleWhenNotNullOrEmpty { get; } =
        new(s => string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible);

    /// <summary>
    /// 将对象转换为可见性状态：当对象为null时显示，否则隐藏
    /// </summary>
    public static FuncValueConverter<object, Visibility> VisibleWhenNull { get; } =
        new(o => o == null ? Visibility.Visible : Visibility.Collapsed);

    /// <summary>
    /// 将对象转换为可见性状态：当对象不为null时显示，否则隐藏
    /// </summary>
    public static FuncValueConverter<object, Visibility> VisibleWhenNotNull { get; } =
        new(o => o != null ? Visibility.Visible : Visibility.Collapsed);

    /// <summary>
    /// 集合为空或 null 时可见
    /// </summary>
    public static FuncValueConverter<IEnumerable, Visibility> VisibleWhenEmpty { get; } =
        new(c => c == null || !c.Cast<object>().Any() ? Visibility.Visible : Visibility.Collapsed);

    /// <summary>
    /// 集合非空时可见
    /// </summary>
    public static FuncValueConverter<IEnumerable, Visibility> VisibleWhenNotEmpty { get; } =
        new(c => c != null && c.Cast<object>().Any() ? Visibility.Visible : Visibility.Collapsed);

    /// <summary>
    /// 数值大于 0 时可见
    /// </summary>
    public static FuncValueConverter<double, Visibility> VisibleWhenPositive { get; } =
        new(d => d > 0 ? Visibility.Visible : Visibility.Collapsed);

    /// <summary>
    /// 数值等于 0 时可见
    /// </summary>
    public static FuncValueConverter<double, Visibility> VisibleWhenZero { get; } =
        new(d => d == 0 ? Visibility.Visible : Visibility.Collapsed);

    /// <summary>
    /// 数值不等于 0 时可见
    /// </summary>
    public static FuncValueConverter<double, Visibility> VisibleWhenNotZero { get; } =
        new(d => d != 0 ? Visibility.Visible : Visibility.Collapsed);

    /// <summary>
    /// 值等于参数时可见（用于枚举比较）
    /// </summary>
    public static FuncValueConverter<object, object, Visibility> VisibleWhenEquals { get; } =
        new((value, parameter) => Equals(value, parameter) ? Visibility.Visible : Visibility.Collapsed);

    /// <summary>
    /// 值不等于参数时可见
    /// </summary>
    public static FuncValueConverter<object, object, Visibility> VisibleWhenNotEquals { get; } =
        new((value, parameter) => !Equals(value, parameter) ? Visibility.Visible : Visibility.Collapsed);
}