using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 文本高亮显示控件。继承自 <see cref="TextBlock"/>。
/// <para>
/// 通过 <see cref="SourceText"/> 提供原文，<see cref="QueriesText"/> 提供查询词
/// （空格分隔多个 token；用引号包裹整体短语，如 <c>"hello world"</c> 视为一个完整匹配）。
/// 控件会找到所有匹配位置（重叠区间合并），把匹配片段渲染成
/// <see cref="HighlightBackground"/> + <see cref="HighlightForeground"/> 着色的 <see cref="Run"/>。
/// </para>
/// <para>
/// 默认颜色使用经典搜索高亮风格（黄底深字），跟浏览器 Ctrl+F 的视觉一致。
/// </para>
/// </summary>
public class HighlightTextBlock : TextBlock
{
    // ============ 默认 Brush（freeze 共享，避免每实例分配）============

    private static readonly Brush DefaultHighlightBackground = CreateFrozenBrush(0xFF, 0xEB, 0x3B);   // Material Yellow 500

    private static readonly Brush DefaultHighlightForeground = CreateFrozenBrush(0x21, 0x21, 0x21);   // Material Grey 900

    private static Brush CreateFrozenBrush(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    #region SourceText

    public static readonly DependencyProperty SourceTextProperty =
        DependencyProperty.Register(
            nameof(SourceText),
            typeof(string),
            typeof(HighlightTextBlock),
            new PropertyMetadata(null, OnRefreshRequired));

    /// <summary>需要被高亮渲染的原始文本。</summary>
    public string SourceText
    {
        get => (string)GetValue(SourceTextProperty);
        set => SetValue(SourceTextProperty, value);
    }

    #endregion SourceText

    #region QueriesText

    public static readonly DependencyProperty QueriesTextProperty =
        DependencyProperty.Register(
            nameof(QueriesText),
            typeof(string),
            typeof(HighlightTextBlock),
            new PropertyMetadata(null, OnRefreshRequired));

    /// <summary>
    /// 查询词。空格分隔多个 token，<b>双引号包裹整体短语</b>。
    /// <para>例：<c>hello world</c> → 匹配 "hello" 或 "world"；<c>"hello world"</c> → 整体匹配 "hello world"。</para>
    /// </summary>
    public string QueriesText
    {
        get => (string)GetValue(QueriesTextProperty);
        set => SetValue(QueriesTextProperty, value);
    }

    #endregion QueriesText

    #region HighlightBackground

    public static readonly DependencyProperty HighlightBackgroundProperty =
        DependencyProperty.Register(
            nameof(HighlightBackground),
            typeof(Brush),
            typeof(HighlightTextBlock),
            new PropertyMetadata(DefaultHighlightBackground, OnRefreshRequired));

    /// <summary>
    /// 匹配片段的背景色。默认 Material Yellow 500（#FFEB3B）。
    /// <para>
    /// 设为 <c>null</c>（XAML：<c>HighlightBackground="{x:Null}"</c>）时<b>不绘制背景</b>——
    /// 配合自定义 <see cref="HighlightForeground"/> 实现"仅文字色高亮"风格。
    /// </para>
    /// </summary>
    public Brush HighlightBackground
    {
        get => (Brush)GetValue(HighlightBackgroundProperty);
        set => SetValue(HighlightBackgroundProperty, value);
    }

    #endregion HighlightBackground

    #region HighlightForeground

    public static readonly DependencyProperty HighlightForegroundProperty =
        DependencyProperty.Register(
            nameof(HighlightForeground),
            typeof(Brush),
            typeof(HighlightTextBlock),
            new PropertyMetadata(DefaultHighlightForeground, OnRefreshRequired));

    /// <summary>
    /// 匹配片段的前景色。默认 Material Grey 900（#212121）。
    /// <para>
    /// 设为 <c>null</c>（XAML：<c>HighlightForeground="{x:Null}"</c>）时<b>不覆盖前景</b>——
    /// 文字颜色继承外层 TextBlock，配合自定义 <see cref="HighlightBackground"/> 实现"仅背景色高亮"风格。
    /// </para>
    /// </summary>
    public Brush HighlightForeground
    {
        get => (Brush)GetValue(HighlightForegroundProperty);
        set => SetValue(HighlightForegroundProperty, value);
    }

    #endregion HighlightForeground

    #region StringComparison

    public static readonly DependencyProperty StringComparisonProperty =
        DependencyProperty.Register(
            nameof(StringComparison),
            typeof(StringComparison),
            typeof(HighlightTextBlock),
            new PropertyMetadata(StringComparison.CurrentCultureIgnoreCase, OnRefreshRequired));

    /// <summary>
    /// 匹配时的字符串比较模式。默认 <see cref="System.StringComparison.CurrentCultureIgnoreCase"/>。
    /// 替代之前的 <c>IsIgnoreCase</c> bool——支持 6 种比较模式。
    /// </summary>
    public StringComparison StringComparison
    {
        get => (StringComparison)GetValue(StringComparisonProperty);
        set => SetValue(StringComparisonProperty, value);
    }

    #endregion StringComparison

    #region Private Methods

    /// <summary>
    /// 把 <see cref="QueriesText"/> 风格的查询字符串解析为 token 列表。
    /// 空格分隔多个 token，<b>双引号包裹整体短语</b>。公开此方法供 demo / 调试场景预览解析结果。
    /// <para>规则：</para>
    /// <list type="bullet">
    /// <item><c>a b c</c> → 三个 token [a, b, c]</item>
    /// <item><c>"a b c"</c> → 一个 phrase [a b c]</item>
    /// <item><c>a "b c" d</c> → 三个 [a, b c, d]</item>
    /// <item><c>"unclosed</c> → 容错，[unclosed]（视为开放短语到末尾）</item>
    /// </list>
    /// </summary>
    public static IEnumerable<string> ParseQueries(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            yield break;
        }

        var buffer = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < input.Length; i++)
        {
            var ch = input[i];

            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (!inQuotes && char.IsWhiteSpace(ch))
            {
                if (buffer.Length > 0)
                {
                    yield return buffer.ToString();
                    buffer.Clear();
                }
                continue;
            }

            buffer.Append(ch);
        }

        if (buffer.Length > 0)
        {
            yield return buffer.ToString();
        }
    }

    private void RefreshInlines()
    {
        Inlines.Clear();

        if (string.IsNullOrEmpty(SourceText))
        {
            return;
        }

        if (string.IsNullOrEmpty(QueriesText))
        {
            Inlines.Add(SourceText);
            return;
        }

        var sourceText = SourceText;
        var comparer = GetComparerFor(StringComparison);
        var queries = ParseQueries(QueriesText).Distinct(comparer);

        var intervals = new List<Interval>();
        foreach (var query in queries)
        {
            foreach (var interval in GetQueryIntervals(sourceText, query))
            {
                intervals.Add(interval);
            }
        }

        var mergedIntervals = MergeIntervals(intervals);
        var fragments = SplitTextByOrderedDisjointIntervals(sourceText, mergedIntervals);

        foreach (var inline in GenerateRunElements(fragments))
        {
            Inlines.Add(inline);
        }
    }

    /// <summary>所有 5 个 DP 的 callback：触发刷新。</summary>
    private static void OnRefreshRequired(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((HighlightTextBlock)d).RefreshInlines();
    }

    /// <summary>
    /// 合并重叠 / 相邻区间。<b>不修改</b>入参——内部 ToList 复制。
    /// </summary>
    private static List<Interval> MergeIntervals(IEnumerable<Interval> source)
    {
        var intervals = source.ToList();
        if (intervals.Count == 0)
        {
            return intervals;
        }

        intervals.Sort((x, y) => x.Start != y.Start ? x.Start - y.Start : x.End - y.End);

        var first = intervals[0];
        var startPointer = first.Start;
        var endPointer = first.End;

        var result = new List<Interval>();
        for (var i = 1; i < intervals.Count; i++)
        {
            var current = intervals[i];
            if (current.Start <= endPointer)
            {
                if (endPointer < current.End)
                {
                    endPointer = current.End;
                }
            }
            else
            {
                result.Add(new Interval(startPointer, endPointer));
                startPointer = current.Start;
                endPointer = current.End;
            }
        }

        result.Add(new Interval(startPointer, endPointer));
        return result;
    }

    private static IEnumerable<Fragment> SplitTextByOrderedDisjointIntervals(string sourceText, List<Interval> mergedIntervals)
    {
        if (string.IsNullOrEmpty(sourceText))
        {
            yield break;
        }

        if (mergedIntervals == null || mergedIntervals.Count == 0)
        {
            yield return new Fragment(sourceText, false);
            yield break;
        }

        var first = mergedIntervals[0];
        if (first.Start > 0)
        {
            yield return new Fragment(sourceText.Substring(0, first.Start), false);
        }
        yield return new Fragment(sourceText.Substring(first.Start, first.End - first.Start), true);

        var previousEnd = first.End;
        for (var i = 1; i < mergedIntervals.Count; i++)
        {
            var current = mergedIntervals[i];
            yield return new Fragment(sourceText.Substring(previousEnd, current.Start - previousEnd), false);
            yield return new Fragment(sourceText.Substring(current.Start, current.End - current.Start), true);
            previousEnd = current.End;
        }

        if (previousEnd < sourceText.Length)
        {
            yield return new Fragment(sourceText.Substring(previousEnd), false);
        }
    }

    private static StringComparer GetComparerFor(StringComparison comparison)
    {
        return comparison switch
        {
            StringComparison.CurrentCulture => StringComparer.CurrentCulture,
            StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
            StringComparison.InvariantCulture => StringComparer.InvariantCulture,
            StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
            StringComparison.Ordinal => StringComparer.Ordinal,
            StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
            _ => StringComparer.CurrentCulture,
        };
    }

    private IEnumerable<Interval> GetQueryIntervals(string sourceText, string query)
    {
        if (string.IsNullOrEmpty(sourceText) || string.IsNullOrEmpty(query))
        {
            yield break;
        }

        var nextStartIndex = 0;
        while (nextStartIndex < sourceText.Length)
        {
            var index = sourceText.IndexOf(query, nextStartIndex, StringComparison);
            if (index == -1)
            {
                yield break;
            }

            nextStartIndex = index + query.Length;
            yield return new Interval(index, nextStartIndex);
        }
    }

    private IEnumerable<Inline> GenerateRunElements(IEnumerable<Fragment> fragments)
    {
        foreach (var fragment in fragments)
        {
            if (fragment.IsQuery)
            {
                yield return CreateHighlightRun(fragment.Text);
            }
            else
            {
                yield return new Run(fragment.Text);
            }
        }
    }

    private Run CreateHighlightRun(string text)
    {
        var run = new Run(text);

        // null 跳过 SetValue——支持"仅前景"或"仅背景"高亮：
        //  - HighlightBackground = null → 不绘制背景（背景透明），仅文字色高亮
        //  - HighlightForeground = null → 不覆盖前景，文字颜色继承外层 TextBlock，仅背景色高亮
        var bg = HighlightBackground;
        if (bg != null)
        {
            run.Background = bg;
        }

        var fg = HighlightForeground;
        if (fg != null)
        {
            run.Foreground = fg;
        }

        return run;
    }

    #endregion Private Methods

    private readonly struct Fragment
    {
        public string Text { get; }

        public bool IsQuery { get; }

        public Fragment(string text, bool isQuery)
        {
            Text = text;
            IsQuery = isQuery;
        }
    }

    private readonly struct Interval
    {
        public int Start { get; }

        public int End { get; }

        public Interval(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}