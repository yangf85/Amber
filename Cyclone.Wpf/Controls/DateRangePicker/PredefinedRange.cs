using System;
using System.Collections.Generic;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 预定义日期范围,在 DateRangePicker 弹出面板左侧列表显示。
/// </summary>
public interface IPredefinedRange
{
    /// <summary>显示名称(如"今天" / "近 7 天")。</summary>
    string Name { get; }

    /// <summary>范围起始日期。</summary>
    DateTime? Start { get; }

    /// <summary>范围结束日期。</summary>
    DateTime? End { get; }
}

/// <summary>
/// 预定义日期范围的标准实现。
/// </summary>
public class PredefinedRange : IPredefinedRange
{
    public string Name { get; }
    public DateTime? Start { get; }
    public DateTime? End { get; }

    public PredefinedRange(string name, DateTime? start, DateTime? end)
    {
        Name = name;
        Start = start;
        End = end;
    }
}

/// <summary>
/// 默认预定义范围生成器。生成 17 个常见范围,基于当前日期 (DateTime.Today)。
/// 控件 OnApplyTemplate 时调用 <see cref="Generate"/> 填充默认列表,
/// 用户可在 DateRangePicker.PredefinedRanges 里覆盖或清空。
/// </summary>
public static class PredefinedRangeGenerator
{
    public static IList<IPredefinedRange> Generate()
    {
        var today = DateTime.Today;

        DateTime FirstOfMonth(DateTime d) => new DateTime(d.Year, d.Month, 1);
        DateTime LastOfMonth(DateTime d) => FirstOfMonth(d).AddMonths(1).AddDays(-1);

        // 周一 → 周日 (中国习惯)
        DateTime StartOfWeek(DateTime d)
        {
            int diff = ((int)d.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return d.AddDays(-diff);
        }
        DateTime EndOfWeek(DateTime d) => StartOfWeek(d).AddDays(6);

        return new List<IPredefinedRange>
        {
            new PredefinedRange("今天",     today,                                    today),
            new PredefinedRange("昨天",     today.AddDays(-1),                        today.AddDays(-1)),
            new PredefinedRange("近 3 天",  today.AddDays(-2),                        today),
            new PredefinedRange("近 7 天",  today.AddDays(-6),                        today),
            new PredefinedRange("近 15 天", today.AddDays(-14),                       today),

            new PredefinedRange("本周",     StartOfWeek(today),                       EndOfWeek(today)),
            new PredefinedRange("上周",     StartOfWeek(today).AddDays(-7),           EndOfWeek(today).AddDays(-7)),
            new PredefinedRange("下周",     StartOfWeek(today).AddDays(7),            EndOfWeek(today).AddDays(7)),

            new PredefinedRange("本月",     FirstOfMonth(today),                      today),
            new PredefinedRange("上月",     FirstOfMonth(today.AddMonths(-1)),        LastOfMonth(today.AddMonths(-1))),
            new PredefinedRange("下月",     FirstOfMonth(today.AddMonths(1)),         LastOfMonth(today.AddMonths(1))),

            new PredefinedRange("近 3 个月", FirstOfMonth(today.AddMonths(-3)),       today),
            new PredefinedRange("前 3 个月", FirstOfMonth(today.AddMonths(-3)),       LastOfMonth(today.AddMonths(-3))),
            new PredefinedRange("前 6 个月", FirstOfMonth(today.AddMonths(-6)),       LastOfMonth(today.AddMonths(-6))),

            new PredefinedRange("本年",     new DateTime(today.Year, 1, 1),           today),
            new PredefinedRange("去年",     new DateTime(today.Year - 1, 1, 1),       new DateTime(today.Year - 1, 12, 31)),
            new PredefinedRange("前年",     new DateTime(today.Year - 2, 1, 1),       new DateTime(today.Year - 2, 12, 31)),
        };
    }
}
