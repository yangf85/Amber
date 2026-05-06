using Cyclone.Wpf.Helpers;
using System.ComponentModel;

namespace Cyclone.Wpf.Controls;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum NumberOperator
{
    [Description("=")]
    Equal,

    [Description("≠")]
    NotEqual,

    [Description("<")]
    LessThan,

    [Description("≤")]
    LessThanOrEqual,

    [Description(">")]
    GreaterThan,

    [Description("≥")]
    GreaterThanOrEqual
}
