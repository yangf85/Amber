using System;
using System.ComponentModel;
using System.Reflection;

namespace Cyclone.Wpf.Converters;

/// <summary>
/// 枚举相关的值转换器集合。
/// </summary>
public class EnumConverter
{
    /// <summary>
    /// 枚举值转 Description 特性文本（无 Description 时返回枚举名称）
    /// </summary>
    public static FuncValueConverter<Enum, string> ToDescription { get; } =
        new(value =>
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null)
            {
                return value.ToString();
            }

            var attr = field.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        });

    /// <summary>
    /// 枚举值等于参数时返回 true（RadioButton 绑定枚举用）
    /// </summary>
    public static FuncValueConverter<Enum, Enum, bool> IsEqual { get; } =
        new((value, parameter) => Equals(value, parameter),
            (result, parameter) => result ? parameter : default);
}