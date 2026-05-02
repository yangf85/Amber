using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 表示 <see cref="MultiComboBox"/> 控件中的一个可选项。
/// <para>
/// 继承自 <see cref="ListBoxItem"/>，因此 IsSelected / Selected / Unselected 路由事件、
/// 鼠标点击切换、键盘 Space 切换、方向键移动焦点、Ctrl+A 全选等交互全部由基类处理。
/// </para>
/// <para>
/// 模板里展示一个左侧 CheckBox + Content 的标准多选项视觉。CheckBox 通过模板绑定到 IsSelected，
/// 不需要拦截鼠标事件——ListBoxItem 的内置点击逻辑会自动切换 IsSelected。
/// </para>
/// </summary>
public class MultiComboBoxItem : ListBoxItem
{
    static MultiComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MultiComboBoxItem),
            new FrameworkPropertyMetadata(typeof(MultiComboBoxItem)));
    }
}