using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 颜色处理辅助类
/// </summary>
public static class ColorHelper
{
    #region 颜色空间转换

    /// <summary>
    /// 计算给定色调的无饱和度颜色
    /// </summary>
    /// <param name="hue">色调 (0-360)</param>
    /// <returns>对应色调的灰色</returns>
    public static Color GetDesaturatedColor(double hue)
    {
        // 将HSV转RGB，使用饱和度为0，亮度为0.5
        double h = hue;
        double s = 0;
        double v = 0.5;

        HsvToRgb(h, s, v, out byte r, out byte g, out byte b);
        return Color.FromRgb(r, g, b);
    }

    /// <summary>
    /// 计算给定颜色的无饱和度版本
    /// </summary>
    /// <param name="color">原颜色</param>
    /// <returns>无饱和度颜色</returns>
    public static Color GetDesaturatedColor(Color color)
    {
        RgbToHsv(color.R, color.G, color.B, out double h, out _, out double v);
        return GetDesaturatedColor(h);
    }

    /// <summary>
    /// RGB转HSV
    /// </summary>
    public static void RgbToHsv(byte r, byte g, byte b, out double h, out double s, out double v)
    {
        double red = r / 255.0;
        double green = g / 255.0;
        double blue = b / 255.0;

        double max = Math.Max(Math.Max(red, green), blue);
        double min = Math.Min(Math.Min(red, green), blue);
        double delta = max - min;

        // 计算色调 (Hue)
        if (delta == 0)
        {
            h = 0; // 无色调 (灰色)
        }
        else if (max == red)
        {
            h = 60 * ((green - blue) / delta % 6);
        }
        else if (max == green)
        {
            h = 60 * ((blue - red) / delta + 2);
        }
        else // max == blue
        {
            h = 60 * ((red - green) / delta + 4);
        }

        if (h < 0)
            h += 360;

        // 计算饱和度 (Saturation)
        s = max == 0 ? 0 : delta / max;

        // 计算明度 (Value)
        v = max;
    }

    /// <summary>
    /// HSV转RGB
    /// </summary>
    public static void HsvToRgb(double h, double s, double v, out byte r, out byte g, out byte b)
    {
        double hh = h / 60.0;
        int i = (int)Math.Floor(hh);
        double ff = hh - i;

        double p = v * (1.0 - s);
        double q = v * (1.0 - (s * ff));
        double t = v * (1.0 - (s * (1.0 - ff)));

        double red, green, blue;

        switch (i)
        {
            case 0:
                red = v; green = t; blue = p;
                break;

            case 1:
                red = q; green = v; blue = p;
                break;

            case 2:
                red = p; green = v; blue = t;
                break;

            case 3:
                red = p; green = q; blue = v;
                break;

            case 4:
                red = t; green = p; blue = v;
                break;

            default: // case 5 or 6
                red = v; green = p; blue = q;
                break;
        }

        r = (byte)(red * 255);
        g = (byte)(green * 255);
        b = (byte)(blue * 255);
    }

    /// <summary>
    /// RGB转HSL
    /// </summary>
    public static void RgbToHsl(byte r, byte g, byte b, out double h, out double s, out double l)
    {
        double red = r / 255.0;
        double green = g / 255.0;
        double blue = b / 255.0;

        double max = Math.Max(Math.Max(red, green), blue);
        double min = Math.Min(Math.Min(red, green), blue);
        double delta = max - min;

        // 计算亮度 (Lightness)
        l = (max + min) / 2.0;

        // 计算色调 (Hue) 和 饱和度 (Saturation)
        if (delta == 0)
        {
            h = 0; // 无色调 (灰色)
            s = 0; // 无饱和度
        }
        else
        {
            s = l <= 0.5 ? delta / (max + min) : delta / (2.0 - max - min);

            if (max == red)
            {
                h = ((green - blue) / delta) % 6;
            }
            else if (max == green)
            {
                h = ((blue - red) / delta) + 2;
            }
            else // max == blue
            {
                h = ((red - green) / delta) + 4;
            }

            h *= 60;
            if (h < 0)
                h += 360;
        }
    }

    /// <summary>
    /// HSL转RGB
    /// </summary>
    public static void HslToRgb(double h, double s, double l, out byte r, out byte g, out byte b)
    {
        double red, green, blue;

        if (s == 0)
        {
            // 灰度
            red = green = blue = l;
        }
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;

            red = HueToRgb(p, q, h / 360 + 1.0 / 3.0);
            green = HueToRgb(p, q, h / 360);
            blue = HueToRgb(p, q, h / 360 - 1.0 / 3.0);
        }

        r = (byte)(red * 255);
        g = (byte)(green * 255);
        b = (byte)(blue * 255);
    }

    /// <summary>
    /// RGB转CMYK
    /// </summary>
    public static void RgbToCmyk(byte r, byte g, byte b, out double c, out double m, out double y, out double k)
    {
        double red = r / 255.0;
        double green = g / 255.0;
        double blue = b / 255.0;

        k = 1 - Math.Max(Math.Max(red, green), blue);

        if (Math.Abs(k - 1.0) < 0.00001)
        {
            c = m = y = 0;
        }
        else
        {
            c = (1 - red - k) / (1 - k);
            m = (1 - green - k) / (1 - k);
            y = (1 - blue - k) / (1 - k);
        }
    }

    /// <summary>
    /// CMYK转RGB
    /// </summary>
    public static void CmykToRgb(double c, double m, double y, double k, out byte r, out byte g, out byte b)
    {
        r = (byte)((1 - c) * (1 - k) * 255);
        g = (byte)((1 - m) * (1 - k) * 255);
        b = (byte)((1 - y) * (1 - k) * 255);
    }

    /// <summary>
    /// 色调转RGB分量辅助方法
    /// </summary>
    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
        return p;
    }

    #endregion 颜色空间转换

    #region 颜色比较

    /// <summary>
    /// 判断两个颜色是否接近
    /// </summary>
    public static bool AreColorsClose(Color a, Color b, int tolerance = 5)
    {
        return Math.Abs(a.R - b.R) <= tolerance &&
               Math.Abs(a.G - b.G) <= tolerance &&
               Math.Abs(a.B - b.B) <= tolerance &&
               Math.Abs(a.A - b.A) <= tolerance;
    }

    /// <summary>
    /// 获取颜色的相反色
    /// </summary>
    public static Color GetComplementaryColor(Color color)
    {
        return Color.FromRgb((byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B));
    }

    /// <summary>
    /// 根据颜色亮度自动判断应使用黑色或白色文本
    /// </summary>
    public static Color GetContrastTextColor(Color background)
    {
        // 计算颜色亮度 (基于人眼对不同颜色的感知)
        double luminance = 0.299 * background.R + 0.587 * background.G + 0.114 * background.B;

        // 亮度高于128使用黑色文本，否则使用白色
        return luminance > 128 ? Colors.Black : Colors.White;
    }

    #endregion 颜色比较

    #region 颜色分类

    /// <summary>
    /// 根据颜色获取默认分类
    /// </summary>
    public static string GetColorCategory(Color color)
    {
        RgbToHsv(color.R, color.G, color.B, out double h, out double s, out double v);

        // 灰度色
        if (s < 0.15)
        {
            if (v < 0.1) return "黑色";
            if (v > 0.9) return "白色";
            return "灰色";
        }

        // 按色相分类
        if (h < 30) return "红色";
        if (h < 60) return "橙色";
        if (h < 90) return "黄色";
        if (h < 150) return "绿色";
        if (h < 210) return "青色";
        if (h < 270) return "蓝色";
        if (h < 330) return "紫色";
        return "红色";
    }

    /// <summary>
    ///
    /// 获取颜色名称
    /// </summary>
    public static string GetColorName(Color color)
    {
        // 基本颜色命名
        if (color == Colors.Red) return "红色";
        if (color == Colors.Green) return "绿色";
        if (color == Colors.Blue) return "蓝色";
        if (color == Colors.Yellow) return "黄色";
        if (color == Colors.Purple) return "紫色";
        if (color == Colors.Cyan) return "青色";
        if (color == Colors.Magenta) return "品红";
        if (color == Colors.White) return "白色";
        if (color == Colors.Black) return "黑色";
        if (color == Colors.Gray) return "灰色";
        if (color == Colors.Orange) return "橙色";

        // 如果不是标准颜色，返回十六进制值作为名称
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    #endregion 颜色分类

    #region 颜色文本转换

    /// <summary>
    /// 将颜色转换为HEX格式字符串
    /// </summary>
    /// <param name="color">要转换的颜色</param>
    /// <param name="includeAlpha">是否包含透明度</param>
    /// <returns>HEX格式字符串</returns>
    public static string ColorToHexString(Color color, bool includeAlpha = false)
    {
        if (includeAlpha && color.A < 255)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// 将颜色转换为RGB格式字符串
    /// </summary>
    /// <param name="color">要转换的颜色</param>
    /// <param name="includeAlpha">是否包含透明度</param>
    /// <returns>RGB格式字符串</returns>
    public static string ColorToRgbString(Color color, bool includeAlpha = false)
    {
        if (includeAlpha && color.A < 255)
        {
            return $"RGBA({color.R}, {color.G}, {color.B}, {color.A / 255.0:F2})";
        }
        return $"RGB({color.R}, {color.G}, {color.B})";
    }

    /// <summary>
    /// 将颜色转换为HSL格式字符串
    /// </summary>
    /// <param name="color">要转换的颜色</param>
    /// <returns>HSL格式字符串</returns>
    public static string ColorToHslString(Color color)
    {
        RgbToHsl(color.R, color.G, color.B, out double h, out double s, out double l);
        return $"HSL: {h:F0}°, {s:P0}, {l:P0}";
    }

    /// <summary>
    /// 将颜色转换为HSV格式字符串
    /// </summary>
    /// <param name="color">要转换的颜色</param>
    /// <returns>HSV格式字符串</returns>
    public static string ColorToHsvString(Color color)
    {
        RgbToHsv(color.R, color.G, color.B, out double h, out double s, out double v);
        return $"HSV: {h:F0}°, {s:P0}, {v:P0}";
    }

    /// <summary>
    /// 将颜色转换为CMYK格式字符串
    /// </summary>
    /// <param name="color">要转换的颜色</param>
    /// <returns>CMYK格式字符串</returns>
    public static string ColorToCmykString(Color color)
    {
        RgbToCmyk(color.R, color.G, color.B, out double c, out double m, out double y, out double k);
        return $"CMYK: {c:P0}, {m:P0}, {y:P0}, {k:P0}";
    }

    /// <summary>
    /// 将指定格式的颜色文本转换为颜色对象
    /// </summary>
    /// <param name="text">颜色文本</param>
    /// <param name="mode">颜色文本格式</param>
    /// <param name="color">输出颜色</param>
    /// <returns>是否转换成功</returns>
    public static bool TryParseColorText(string text, ColorTextMode mode, out Color color)
    {
        if (mode == ColorTextMode.HEX)
        {
            return TryParseHexColor(text, out color);
        }
        else if (mode == ColorTextMode.RGB)
        {
            return TryParseRgbColor(text, out color);
        }
        else
        {
            color = Colors.Transparent;
            return false;
        }
    }

    /// <summary>
    /// 尝试将HEX格式字符串解析为颜色
    /// </summary>
    /// <param name="text">要解析的字符串</param>
    /// <param name="color">输出颜色</param>
    /// <returns>是否解析成功</returns>
    public static bool TryParseHexColor(string text, out Color color)
    {
        color = Colors.Black;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.TrimStart('#');

        // 支持ARGB格式
        if (text.Length == 8)
        {
            try
            {
                byte a = Convert.ToByte(text.Substring(0, 2), 16);
                byte r = Convert.ToByte(text.Substring(2, 2), 16);
                byte g = Convert.ToByte(text.Substring(4, 2), 16);
                byte b = Convert.ToByte(text.Substring(6, 2), 16);

                color = Color.FromArgb(a, r, g, b);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 支持简写形式 #RGB
        if (text.Length == 3)
        {
            text = $"{text[0]}{text[0]}{text[1]}{text[1]}{text[2]}{text[2]}";
        }

        // 标准RGB格式
        if (text.Length != 6)
            return false;

        try
        {
            byte r = Convert.ToByte(text.Substring(0, 2), 16);
            byte g = Convert.ToByte(text.Substring(2, 2), 16);
            byte b = Convert.ToByte(text.Substring(4, 2), 16);

            color = Color.FromRgb(r, g, b);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 尝试将RGB格式字符串解析为颜色
    /// </summary>
    /// <param name="text">要解析的字符串</param>
    /// <param name="color">输出颜色</param>
    /// <returns>是否解析成功</returns>
    public static bool TryParseRgbColor(string text, out Color color)
    {
        color = Colors.Black;

        // 支持RGBA格式
        var rgbaMatch = Regex.Match(text, @"RGBA\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*([\d\.]+)\s*\)", RegexOptions.IgnoreCase);
        if (rgbaMatch.Success)
        {
            try
            {
                byte r = Clamp(byte.Parse(rgbaMatch.Groups[1].Value), 0, 255);
                byte g = Clamp(byte.Parse(rgbaMatch.Groups[2].Value), 0, 255);
                byte b = Clamp(byte.Parse(rgbaMatch.Groups[3].Value), 0, 255);
                byte a = (byte)(Clamp(double.Parse(rgbaMatch.Groups[4].Value), 0, 1) * 255);

                color = Color.FromArgb(a, r, g, b);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 支持RGB格式
        var rgbMatch = Regex.Match(text, @"RGB\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)", RegexOptions.IgnoreCase);
        if (!rgbMatch.Success)
            return false;

        try
        {
            byte r = Clamp(byte.Parse(rgbMatch.Groups[1].Value), 0, 255);
            byte g = Clamp(byte.Parse(rgbMatch.Groups[2].Value), 0, 255);
            byte b = Clamp(byte.Parse(rgbMatch.Groups[3].Value), 0, 255);

            color = Color.FromRgb(r, g, b);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 将指定格式的颜色文本转换为颜色对象，转换失败返回 null
    /// </summary>
    /// <param name="text">颜色文本</param>
    /// <param name="mode">颜色文本格式</param>
    /// <returns>成功返回颜色，失败返回 null</returns>
    public static Color? ParseColorText(string text, ColorTextMode mode)
    {
        return TryParseColorText(text, mode, out Color color) ? color : null;
    }

    /// <summary>
    /// 将HEX格式字符串解析为颜色，解析失败返回 null
    /// </summary>
    /// <param name="text">要解析的字符串</param>
    /// <returns>成功返回颜色，失败返回 null</returns>
    public static Color? ParseHexColor(string text)
    {
        return TryParseHexColor(text, out Color color) ? color : null;
    }

    /// <summary>
    /// 将RGB格式字符串解析为颜色，解析失败返回 null
    /// </summary>
    /// <param name="text">要解析的字符串</param>
    /// <returns>成功返回颜色，失败返回 null</returns>
    public static Color? ParseRgbColor(string text)
    {
        return TryParseRgbColor(text, out Color color) ? color : null;
    }

    #endregion 颜色文本转换

    #region 颜色文本验证

    /// <summary>
    /// 验证Hex颜色字符
    /// </summary>
    public static bool IsValidHexChar(string text)
    {
        return Regex.IsMatch(text, "^[#0-9a-fA-F]+$");
    }

    /// <summary>
    /// 验证RGB颜色字符
    /// </summary>
    public static bool IsValidRgbChar(string text)
    {
        return Regex.IsMatch(text, "^[rgbRGB0-9()\\s,.]+$");
    }

    /// <summary>
    /// 验证Hex颜色字符串
    /// </summary>
    public static bool IsValidHexString(string text)
    {
        return Regex.IsMatch(text, "^#?([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$");
    }

    /// <summary>
    /// 验证RGB颜色字符串
    /// </summary>
    public static bool IsValidRgbString(string text)
    {
        return Regex.IsMatch(text, @"^RGB\(\s*\d+\s*,\s*\d+\s*,\s*\d+\s*\)$", RegexOptions.IgnoreCase) ||
               Regex.IsMatch(text, @"^RGBA\(\s*\d+\s*,\s*\d+\s*,\s*\d+\s*,\s*[\d\.]+\s*\)$", RegexOptions.IgnoreCase);
    }

    #endregion 颜色文本验证

    #region 辅助方法

    /// <summary>
    /// 将数值限制在指定范围内
    /// </summary>
    private static byte Clamp(int value, int min, int max)
    {
        return (byte)Math.Max(min, Math.Min(value, max));
    }

    /// <summary>
    /// 将浮点数值限制在指定范围内
    /// </summary>
    private static double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(value, max));
    }

    #endregion 辅助方法
}