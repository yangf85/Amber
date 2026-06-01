using Cyclone.Wpf.Helpers;
using System.ComponentModel;

namespace Cyclone.Wpf.Controls;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum TextOperator
{
    [Description("=")]
    Equal,

    [Description("≠")]
    NotEqual,

    [Description("∋")]
    Contains,

    [Description("∌")]
    NotContains,

    [Description("^")]
    StartsWith,

    [Description("$")]
    EndsWith,

    [Description("*")]
    Regex
}