using System.Windows;

namespace Cyclone.Wpf.Themes;

/// <summary>
/// 主题基类——派生类通过设置 <see cref="ResourceDictionary.Source"/> 指向 XAML 主题字典。
/// 由 <see cref="ThemeManager"/> 注册和管理。
/// </summary>
public abstract class Theme : ResourceDictionary
{
    /// <summary>主题名称——用作 SwitchTo(name) 的 key，应该唯一且稳定。</summary>
    public abstract string Name { get; }
}