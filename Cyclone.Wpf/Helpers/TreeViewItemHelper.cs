using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// TreeViewItem 辅助附加属性。
/// 静态构造函数中显式注册所有 DP——避免 beforefieldinit 模式下
/// LevelProperty / LevelPropertyKey 字段初始化顺序导致 NullReferenceException。
/// </summary>
public static class TreeViewItemHelper
{
    #region Level (read-only attached) — 节点深度，绑定用

    /// <summary>
    /// 当前 TreeViewItem 在 TreeView 中的嵌套深度（根节点=0，每深一层 +1）。
    /// 样式中通过 MultiBinding 配合 LevelToIndentConverter 用此值计算 Header 左 Margin，
    /// 让选中/hover 背景能横向占满整行。
    /// 由 AutoLevel 附加属性自动维护——不用手动设置。
    /// </summary>
    public static readonly DependencyProperty LevelProperty;

    private static readonly DependencyPropertyKey LevelPropertyKey;

    public static int GetLevel(DependencyObject obj)
    {
        return (int)obj.GetValue(LevelProperty);
    }

    private static void UpdateLevel(TreeViewItem item)
    {
        int level = 0;
        var parent = ItemsControl.ItemsControlFromItemContainer(item);
        while (parent is TreeViewItem)
        {
            level++;
            parent = ItemsControl.ItemsControlFromItemContainer(parent);
        }
        item.SetValue(LevelPropertyKey, level);
    }

    #endregion Level (read-only attached) — 节点深度，绑定用

    #region AutoLevel (attached) — 自动挂载 Loaded 事件计算 Level

    /// <summary>
    /// 设为 True 时自动挂载 TreeViewItem.Loaded 事件，在加载完成时计算 Level。
    /// 在 TreeViewItem 样式中通过 Setter 设置：
    /// <code>
    /// &lt;Setter Property="hp:TreeViewItemHelper.AutoLevel" Value="True" /&gt;
    /// </code>
    /// 用附加属性 setter 挂事件——避开 EventSetter.Handler 不接受 x:Static 的限制
    /// （ResourceDictionary 没有 code-behind 时 EventSetter 无法引用静态方法）。
    /// </summary>
    public static readonly DependencyProperty AutoLevelProperty;

    public static bool GetAutoLevel(DependencyObject obj)
    {
        return (bool)obj.GetValue(AutoLevelProperty);
    }

    public static void SetAutoLevel(DependencyObject obj, bool value)
    {
        obj.SetValue(AutoLevelProperty, value);
    }

    private static void OnAutoLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TreeViewItem item)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            // 已加载就立即计算；未加载则等 Loaded 触发
            if (item.IsLoaded)
            {
                UpdateLevel(item);
            }
            else
            {
                item.Loaded += OnTreeViewItemLoaded;
            }
        }
        else
        {
            item.Loaded -= OnTreeViewItemLoaded;
        }
    }

    private static void OnTreeViewItemLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is TreeViewItem item)
        {
            UpdateLevel(item);

            // 一次性事件——计算后即解绑，避免节点重新 Loaded（如样式重应用）时累积
            item.Loaded -= OnTreeViewItemLoaded;
        }
    }

    #endregion AutoLevel (attached) — 自动挂载 Loaded 事件计算 Level

    #region Static Constructor — 集中注册所有 DP，保证初始化顺序

    static TreeViewItemHelper()
    {
        // Level 只读 DP：先创建 key，再从 key 取出 DependencyProperty
        LevelPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "Level",
            typeof(int),
            typeof(TreeViewItemHelper),
            new PropertyMetadata(0));
        LevelProperty = LevelPropertyKey.DependencyProperty;

        // AutoLevel 普通 DP
        AutoLevelProperty = DependencyProperty.RegisterAttached(
            "AutoLevel",
            typeof(bool),
            typeof(TreeViewItemHelper),
            new PropertyMetadata(false, OnAutoLevelChanged));
    }

    #endregion Static Constructor — 集中注册所有 DP，保证初始化顺序
}