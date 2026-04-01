using System;

namespace Cyclone.Wpf.Converters;

/// <summary>
/// 字符串相关的值转换器集合。
/// </summary>
public class StringConverter
{
    /// <summary>
    /// 转大写
    /// </summary>
    public static FuncValueConverter<string, string> ToUpper { get; } =
        new(s => s?.ToUpperInvariant());

    /// <summary>
    /// 转小写
    /// </summary>
    public static FuncValueConverter<string, string> ToLower { get; } =
        new(s => s?.ToLowerInvariant());

    /// <summary>
    /// 截断字符串（参数为最大长度）
    /// </summary>
    public static FuncValueConverter<string, string, string> Truncate { get; } =
        new((s, length) =>
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            var max = int.Parse(length);
            return s.Length <= max ? s : s.Substring(0, max) + "...";
        });

    /// <summary>
    /// 格式化字符串（参数为格式模板，用 {0} 占位）
    /// </summary>
    public static FuncValueConverter<object, string, string> Format { get; } =
        new((value, format) => string.Format(format, value));

    /// <summary>
    /// 添加前缀
    /// </summary>
    public static FuncValueConverter<string, string, string> Prefix { get; } =
        new((s, prefix) => string.IsNullOrEmpty(s) ? s : prefix + s);

    /// <summary>
    /// 添加后缀
    /// </summary>
    public static FuncValueConverter<string, string, string> Suffix { get; } =
        new((s, suffix) => string.IsNullOrEmpty(s) ? s : s + suffix);

    /// <summary>
    /// null 或空时显示默认文本
    /// </summary>
    public static FuncValueConverter<string, string, string> DefaultIfEmpty { get; } =
        new((s, defaultText) => string.IsNullOrEmpty(s) ? defaultText : s);
}