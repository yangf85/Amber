using System;
using System.Collections;
using System.Linq;

namespace Cyclone.Wpf.Converters;

public class MathConverter
{
    /// <summary>
    /// 取绝对值
    /// </summary>
    public static FuncValueConverter<double, double> Abs { get; } =
        new(number => Math.Abs(number));

    /// <summary>
    /// 加法转换器：将两个数字相加
    /// </summary>
    public static FuncValueConverter<double, double, double> Addition { get; } =
        new((number, add) => number + add);

    /// <summary>
    /// 加一转换器：将整数加1
    /// </summary>
    public static FuncValueConverter<int, int> AddOne { get; } =
        new(number => number + 1);

    /// <summary>
    /// 向上取整
    /// </summary>
    public static FuncValueConverter<double, double> Ceiling { get; } =
        new(number => Math.Ceiling(number));

    /// <summary>
    /// 限制数值范围（参数格式: "min,max"）
    /// </summary>
    public static FuncValueConverter<double, string, double> Clamp { get; } =
        new((number, range) =>
        {
            var parts = range.Split(',');
            if (parts.Length != 2)
            {
                return number;
            }

            if (double.TryParse(parts[0], out var min) && double.TryParse(parts[1], out var max))
            {
                return Math.Max(min, Math.Min(max, number));
            }

            return number;
        });

    /// <summary>
    /// 集合元素数量
    /// </summary>
    public static FuncValueConverter<IEnumerable, int> Count { get; } =
        new(c => c?.Cast<object>().Count() ?? 0);

    /// <summary>
    /// 除法转换器：将第一个数字除以第二个数字
    /// </summary>
    public static FuncValueConverter<double, double, double> Division { get; } =
        new((number, div) => number / div);

    /// <summary>
    /// 向下取整
    /// </summary>
    public static FuncValueConverter<double, double> Floor { get; } =
        new(number => Math.Floor(number));

    /// <summary>
    /// 取半转换器：将数字乘以0.5，即取其一半
    /// </summary>
    public static FuncValueConverter<double, double> Half { get; } =
        new(number => number * 0.5);

    /// <summary>
    /// 乘法转换器：将两个数字相乘
    /// </summary>
    public static FuncValueConverter<double, double, double> Multiplication { get; } =
        new((number, mult) => number * mult);

    /// <summary>
    /// 取反（乘以 -1）
    /// </summary>
    public static FuncValueConverter<double, double> Negate { get; } =
        new(number => -number);

    /// <summary>
    /// 对象集合转索引集合转换器：将对象集合转换为其索引集合
    /// </summary>
    public static FuncValueConverter<IEnumerable, IEnumerable> ObjectsToIndexes { get; } =
        new(objects => objects.OfType<object>().Select((i, j) => j));

    /// <summary>
    /// 四舍五入到指定小数位（参数为小数位数）
    /// </summary>
    public static FuncValueConverter<double, double, double> Round { get; } =
        new((number, digits) => Math.Round(number, (int)digits));

    /// <summary>
    /// 缩放转换器：将数字按指定比例缩放
    /// </summary>
    public static FuncValueConverter<double, double, double> Scale { get; } =
        new((number, scale) => number * scale);

    /// <summary>
    /// 减法转换器：从第一个数字中减去第二个数字
    /// </summary>
    public static FuncValueConverter<double, double, double> Subtraction { get; } =
        new((number, sub) => number - sub);

    /// <summary>
    /// 保留指定小数位的字符串（参数为小数位数）
    /// </summary>
    public static FuncValueConverter<double, double, string> ToFixed { get; } =
        new((number, digits) => number.ToString($"F{(int)digits}"));

    /// <summary>
    /// 百分比格式化（0.75 → "75%"）
    /// </summary>
    public static FuncValueConverter<double, string> ToPercent { get; } =
        new(number => $"{number * 100:F0}%");
}