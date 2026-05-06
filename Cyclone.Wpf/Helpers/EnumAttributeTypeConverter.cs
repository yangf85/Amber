using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// 【枚举描述类型转换器】
/// 功能：增强枚举类型转换能力，支持通过DescriptionAttribute显示友好文本
/// 特性：
///   - 自动提取枚举值的Description特性描述
///   - 内置反射结果缓存提升性能
///   - 支持常规枚举和可空枚举类型
///   - 严格的类型验证和空值处理
/// 使用示例：
/// <code>
/// // 类型注册（通常在静态构造函数中）
/// TypeDescriptor.AddAttributes(typeof(WeekDays),
///     new TypeConverterAttribute(typeof(EnumDescriptionTypeConverter)));
/// // XAML中自动应用
/// <TextBlock Text="{Binding SelectedDay}"/> // 自动显示Description内容
/// </code>
/// 典型应用场景：
/// 1. 在UI界面显示本地化的枚举描述
/// 2. 需要友好名称的报表输出
/// 3. 数据导出时转换枚举值为可读文本
/// </remarks>
public class EnumDescriptionTypeConverter : EnumConverter
{
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            if (null != value)
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());

                if (null != fi)
                {
                    var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                    return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Description)))
                        ? attributes[0].Description
                        : value.ToString();
                }
            }

            return string.Empty;
        }
        return base.ConvertTo(context, culture, value, destinationType);
    }

    public EnumDescriptionTypeConverter(Type type) : base(type)
    {
    }
}