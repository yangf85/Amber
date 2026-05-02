using Cyclone.Wpf.Helpers;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// EnumSelector 内部使用的枚举项包装类。
/// 承担三件事：包装一个 Enum 值（只读）、维护 IsSelected 状态、提供可绑定的 DisplayText。
/// 同时缓存 ulong 形式的 NumericValue 供位运算使用，避免每次都做 Enum→ulong 转换。
/// </summary>
internal class EnumItem : NotificationObject
{
    private bool _isSelected;
    private bool _isUseAlias;

    public EnumItem(Enum @enum, bool isUseAlias = true)
    {
        Enum = @enum;
        NumericValue = EnumSelector.ToUInt64(@enum);
        _isUseAlias = isUseAlias;
    }

    #region IsSelected

    public bool IsSelected
    {
        get => _isSelected;
        set => Set(ref _isSelected, value);
    }

    #endregion IsSelected

    #region Enum (只读)

    /// <summary>
    /// 包装的枚举值。构造后不可变——若要变更，新建 EnumItem 即可。
    /// </summary>
    public Enum Enum { get; }

    #endregion Enum (只读)

    #region NumericValue (只读)

    /// <summary>
    /// Enum 转 ulong 后的位模式缓存。EnumSelector 的位运算与缓存图谱使用此值，
    /// 避免每次都调用 EnumSelector.ToUInt64(Enum)。
    /// </summary>
    public ulong NumericValue { get; }

    #endregion NumericValue (只读)

    #region IsUseAlias

    public bool IsUseAlias
    {
        get => _isUseAlias;
        set
        {
            if (Set(ref _isUseAlias, value))
            {
                // IsUseAlias 切换时显式通知 DisplayText 也变了，
                // 否则 binding 不会知道要重新求值（这是 ToString-only 实现的根本缺陷）。
                NotifyPropertyChanged(nameof(DisplayText));
            }
        }
    }

    #endregion IsUseAlias

    #region DisplayText

    /// <summary>
    /// UI 上展示的文本。设计成属性而非依赖 ToString()，
    /// 是为了让 IsUseAlias 变化时能通过 PropertyChanged 触发 binding 刷新。
    /// 模板里应使用 Content="{Binding DisplayText}" 而不是 Content="{Binding}"。
    /// </summary>
    public string DisplayText => IsUseAlias ? GetEnumDescription() : Enum.ToString();

    #endregion DisplayText

    #region Helpers

    private string GetEnumDescription()
    {
        FieldInfo fieldInfo = Enum.GetType().GetField(Enum.ToString());
        if (fieldInfo is null)
        {
            return Enum.ToString();
        }

        return fieldInfo.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute attr
            ? attr.Description
            : Enum.ToString();
    }

    public override string ToString() => DisplayText;

    #endregion Helpers
}