namespace Cyclone.Wpf.Controls;

/// <summary>
/// NumberBox 的数字模式。取代直接暴露 <see cref="System.Globalization.NumberStyles"/>——
/// 后者太底层、易误用（用户能配出与 IsValidNumericInput 不一致的状态）。
/// </summary>
public enum NumberMode
{
    /// <summary>整数。强制 DecimalPlaces=0，不允许小数点输入。</summary>
    Integer,

    /// <summary>小数。按 DecimalPlaces 限制小数位数。</summary>
    Decimal,
}
