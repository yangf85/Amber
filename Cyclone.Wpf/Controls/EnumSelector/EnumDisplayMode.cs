namespace Cyclone.Wpf.Controls;

/// <summary>
/// EnumSelector 的显示模式。
/// </summary>
public enum EnumDisplayMode
{
    /// <summary>
    /// 自动模式：Flags 枚举 → CheckBox（多选），普通枚举 → RadioButton（单选）。
    /// </summary>
    Auto,

    /// <summary>
    /// 强制 RadioButton 视觉，行为单选。
    /// </summary>
    RadioButton,

    /// <summary>
    /// 强制 CheckBox 视觉。仅 Flags 枚举行为为多选；非 Flags 枚举仍为单选语义，避免合并出无效值。
    /// </summary>
    CheckBox,
}
