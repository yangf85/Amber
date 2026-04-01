using System;
using System.Collections;
using System.Linq;
using System.Windows;

namespace Cyclone.Wpf.Converters;

public class BooleanConverter
{
    /// <summary>
    /// 将布尔值转换为可见性状态：true时显示，false时隐藏
    /// </summary>
    public static FuncValueConverter<bool, Visibility> ToVisibility { get; } =
        new(i => i ? Visibility.Visible : Visibility.Collapsed);

    /// <summary>
    /// 将布尔值取反：true变为false，false变为true
    /// </summary>
    public static FuncValueConverter<bool, bool> Inverse { get; } =
        new(value => !value);

    /// <summary>
    /// 字符串相等比较：比较输入值与参数是否相等，返回布尔结果
    /// </summary>
    public static FuncValueConverter<string, string, bool> StringEquality { get; } =
        new((value, parameter) => value == parameter);

    /// <summary>
    /// 字符串不相等比较：比较输入值与参数是否不相等，返回布尔结果
    /// </summary>
    public static FuncValueConverter<string, string, bool> StringNotEquality { get; } =
        new((value, parameter) => !(value == parameter));

    /// <summary>
    /// 判断对象是否为null：如果对象为null则返回true，否则返回false
    /// </summary>
    public static FuncValueConverter<object, bool> NullToBoolean { get; } =
        new(value => value == null);

    /// <summary>
    /// 判断对象是否不为null：如果对象不为null则返回true，否则返回false
    /// </summary>
    public static FuncValueConverter<object, bool> NotNullToBoolean { get; } =
        new(value => value != null);

    /// <summary>
    /// 值等于参数时返回 true（用于枚举或对象比较）
    /// </summary>
    public static FuncValueConverter<object, object, bool> Equals { get; } =
        new((value, parameter) => System.Object.Equals(value, parameter));

    /// <summary>
    /// 值不等于参数时返回 true
    /// </summary>
    public static FuncValueConverter<object, object, bool> NotEquals { get; } =
        new((value, parameter) => !System.Object.Equals(value, parameter));

    /// <summary>
    /// 集合为空或 null 时返回 true
    /// </summary>
    public static FuncValueConverter<IEnumerable, bool> IsEmpty { get; } =
        new(c => c == null || !c.Cast<object>().Any());

    /// <summary>
    /// 集合非空时返回 true
    /// </summary>
    public static FuncValueConverter<IEnumerable, bool> IsNotEmpty { get; } =
        new(c => c != null && c.Cast<object>().Any());

    /// <summary>
    /// 数值大于 0 时返回 true
    /// </summary>
    public static FuncValueConverter<double, bool> IsPositive { get; } =
        new(d => d > 0);

    /// <summary>
    /// 数值等于 0 时返回 true
    /// </summary>
    public static FuncValueConverter<double, bool> IsZero { get; } =
        new(d => d == 0);
}