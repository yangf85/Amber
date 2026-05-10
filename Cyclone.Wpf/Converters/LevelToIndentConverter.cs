using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Cyclone.Wpf.Converters;

/// <summary>
/// 把 TreeViewItem 嵌套深度 (Level) 转成左缩进。<br/>
/// MultiBinding 输入：[Level (int), UnitIndent (double)]<br/>
/// 输出：Thickness(Level * UnitIndent, 0, 0, 0)
/// </summary>
public class LevelToIndentConverter : IMultiValueConverter
{
    public static readonly LevelToIndentConverter Instance = new LevelToIndentConverter();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // 注意：不能用 list pattern (is [int, double])——依赖 System.Index，net48 没有
        int level = 0;
        double unit = 0;

        if (values != null && values.Length >= 2)
        {
            if (values[0] is int l)
            {
                level = l;
            }
            if (values[1] is double u)
            {
                unit = u;
            }
        }

        if (level < 0)
        {
            level = 0;
        }

        return new Thickness(level * unit, 0, 0, 0);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}