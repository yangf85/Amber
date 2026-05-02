using System;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// SettingGroup 控件:带标题的设置项分组容器。
/// 内部布局自由(默认垂直堆叠 SettingItem),通过 ItemsPanel 可替换。
/// 在 SettingGroup 上设置 <see cref="LabelWidth"/> 等价于在容器上设置 SettingItem.LabelWidth 附加属性,
/// 自动下传到组内所有 SettingItem。
/// </summary>
public class SettingGroup : HeaderedItemsControl
{
    static SettingGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SettingGroup),
            new FrameworkPropertyMetadata(typeof(SettingGroup)));
    }

    #region Description

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(SettingGroup),
            new FrameworkPropertyMetadata(default(string)));

    /// <summary>
    /// 获取或设置组描述文本,渲染于组标题下方。空字符串或 null 时自动收起。
    /// </summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    #endregion Description

    #region LabelWidth (转发到 SettingItem.LabelWidth 附加属性)

    /// <summary>
    /// 获取或设置组内所有 SettingItem 的标签列宽度。
    /// 本属性直接转发 <see cref="SettingItem.LabelWidthProperty"/> 附加属性的存储,
    /// 不引入新的依赖属性,避免重复定义。
    /// </summary>
    public GridLength LabelWidth
    {
        get => SettingItem.GetLabelWidth(this);
        set => SettingItem.SetLabelWidth(this, value);
    }

    #endregion LabelWidth

    #region ContentAlignment (转发到 SettingItem.ContentAlignment 附加属性)

    /// <summary>
    /// 获取或设置组内所有 SettingItem 的 Content 对齐方式。
    /// 本属性直接转发 <see cref="SettingItem.ContentAlignmentProperty"/> 附加属性的存储,
    /// 不引入新的依赖属性,避免重复定义。
    /// </summary>
    public HorizontalAlignment ContentAlignment
    {
        get => SettingItem.GetContentAlignment(this);
        set => SettingItem.SetContentAlignment(this, value);
    }

    #endregion ContentAlignment
}
