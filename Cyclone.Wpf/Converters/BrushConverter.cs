using System;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cyclone.Wpf.Converters;

public class BrushConverter
{
    /// <summary>
    /// 布尔值转画刷转换器：true时返回绿色画刷，false时返回红色画刷
    /// </summary>
    public static FuncValueConverter<bool, Brush> BooleanToBrushConverter { get; } =
        new(b => b ? Brushes.Green : Brushes.Red);

    /// <summary>
    /// 整数值转画刷转换器：-1时返回黄色画刷，小于1时返回红色画刷，其他情况返回绿色画刷
    /// </summary>
    public static FuncValueConverter<int, Brush> IntToBrushConverter { get; } =
        new(i => i switch
        {
            -1 => Brushes.Yellow,
            < 1 => Brushes.Red,
            _ => Brushes.Green
        });

    /// <summary>
    /// 日历周末日期画刷转换器：当日期为周六或周日且未被选中、未悬停、未被禁用时返回红色画刷，
    /// 否则返回深灰色画刷
    /// </summary>
    public static FuncValueConverter<CalendarDayButton, Brush> WeekendDate { get; } =
        new(calendarDayButton =>
        {
            var dateTime = (DateTime)calendarDayButton.DataContext;
            if (!calendarDayButton.IsMouseOver &&
                !calendarDayButton.IsSelected &&
                !calendarDayButton.IsBlackedOut &&
                (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday))
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 47, 47));
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
            }
        });

    /// <summary>
    /// 十六进制颜色字符串转 Brush（如 "#FF0000"）
    /// </summary>
    public static FuncValueConverter<string, Brush> HexToBrush { get; } =
        new(hex =>
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                return new SolidColorBrush(color);
            }
            catch
            {
                return Brushes.Transparent;
            }
        });

    /// <summary>
    /// 根据数值在 0-1 范围内插值红绿色（0=红，1=绿）
    /// </summary>
    public static FuncValueConverter<double, Brush> ProgressBrush { get; } =
        new(value =>
        {
            var clamped = Math.Max(0, Math.Min(1, value));
            var r = (byte)(255 * (1 - clamped));
            var g = (byte)(255 * clamped);
            return new SolidColorBrush(Color.FromRgb(r, g, 0));
        });

    /// <summary>
    /// 设置 Brush 的透明度（参数为 0-1 的不透明度）
    /// </summary>
    public static FuncValueConverter<Brush, double, Brush> WithOpacity { get; } =
        new((brush, opacity) =>
        {
            if (brush is SolidColorBrush solid)
            {
                var color = solid.Color;
                color.A = (byte)(255 * Math.Max(0, Math.Min(1, opacity)));
                return new SolidColorBrush(color);
            }

            return brush;
        });
}