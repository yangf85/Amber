using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 类似 ComboBox 的下拉多选控件：未展开时把每个选中项渲染为一个可交互的 chip（带删除 × 按钮），
/// 展开后是一个带 CheckBox 的 ListBox。
/// <para>
/// 继承自 <see cref="ListBox"/>——白送 SelectionMode、内部 SelectedItems 集合、键盘 Space / 方向键 / Ctrl+A
/// 全套交互。在外层包了一个可绑定的 <see cref="SelectedItemsBindable"/> DP 暴露给 MVVM。
/// </para>
/// <para>
/// chip 风格的设计相比"用 VisualBrush 把选中项截图"的传统 ComboBox 显示方式（参见 WPF 源码 ComboBox.SelectionBoxItem 的注释），
/// 优势是 chip 完全可交互——× 按钮真实可点击，无需 hack。
/// </para>
/// </summary>
[TemplatePart(Name = PART_ToggleButton, Type = typeof(ToggleButton))]
[TemplatePart(Name = PART_ClearButton, Type = typeof(Button))]
[TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
[TemplatePart(Name = PART_SelectAllCheckBox, Type = typeof(CheckBox))]
[TemplatePart(Name = PART_ChipPanel, Type = typeof(ItemsControl))]
[StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(MultiComboBoxItem))]
public class MultiComboBox : ListBox
{
    private const string PART_ToggleButton = nameof(PART_ToggleButton);
    private const string PART_ClearButton = nameof(PART_ClearButton);
    private const string PART_Popup = nameof(PART_Popup);
    private const string PART_SelectAllCheckBox = nameof(PART_SelectAllCheckBox);
    private const string PART_ChipPanel = nameof(PART_ChipPanel);

    private CheckBox _selectAllCheckBox;
    private ItemsControl _chipPanel;
    private bool _suppressSync;

    // 缓存委托保证 AddHandler / RemoveHandler 用同一个引用
    private MouseButtonEventHandler _globalPreviewMouseDownHandler;

    #region Constructors

    private static void OnDisplayMemberPathChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MultiComboBox box)
        {
            box.RefreshDefaultChipTemplate();
        }
    }

    static MultiComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(typeof(MultiComboBox)));

        // 默认 SelectionMode = Multiple（ListBox 默认是 Single——本控件存在的意义就是多选）
        SelectionModeProperty.OverrideMetadata(
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(SelectionMode.Multiple));

        // DisplayMemberPath 变化时重新生成默认 chip 模板
        DisplayMemberPathProperty.OverrideMetadata(
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(string.Empty, OnDisplayMemberPathChangedCallback));

        CommandManager.RegisterClassCommandBinding(
            typeof(MultiComboBox),
            new CommandBinding(ClearCommand, OnClearCommandExecuted, OnClearCommandCanExecute));

        // 单项删除命令：chip 上的 × 按钮调用，参数为数据项
        CommandManager.RegisterClassCommandBinding(
            typeof(MultiComboBox),
            new CommandBinding(RemoveItemCommand, OnRemoveItemCommandExecuted));
    }

    public MultiComboBox()
    {
        // 给每个实例独立的 ObservableCollection，避免 DP 默认值共享 bug
        SetCurrentValue(SelectedItemsBindableProperty, new ObservableCollection<object>());
    }

    #endregion Constructors

    #region SelectedItemsBindable

    public static readonly DependencyProperty SelectedItemsBindableProperty =
        DependencyProperty.Register(
            nameof(SelectedItemsBindable),
            typeof(IList),
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(
                default(IList),
                // 注意：不是 BindsTwoWayByDefault——集合本身的引用不应该被反向写回 ViewModel。
                // 集合的"内容"双向同步靠 INotifyCollectionChanged，调用方提供 ObservableCollection 即可。
                // 这样 ViewModel 里可以用 `public ObservableCollection<T> SelectedXxx { get; } = new()` 这种只读属性。
                FrameworkPropertyMetadataOptions.None,
                OnSelectedItemsBindableChanged));

    /// <summary>
    /// 可绑定的选中项集合。这是 MVVM 入口——通过 <see cref="ObservableCollection{T}"/> 的
    /// <see cref="INotifyCollectionChanged"/> 双向同步集合内容。
    /// <para>
    /// 注意默认 binding 是 OneWay：集合本身的引用不应该被反向写回 ViewModel；
    /// 集合内容的双向同步靠 ObservableCollection 通知。
    /// 因此 ViewModel 可以用 <c>public ObservableCollection&lt;T&gt; Selected { get; } = new();</c> 这种只读属性。
    /// </para>
    /// <para>之所以叫 SelectedItemsBindable 而不是 SelectedItems，是因为基类 <see cref="ListBox"/> 已经占用了 SelectedItems（只读、不可绑定）。</para>
    /// </summary>
    public IList SelectedItemsBindable
    {
        get => (IList)GetValue(SelectedItemsBindableProperty);
        set => SetValue(SelectedItemsBindableProperty, value);
    }

    #endregion SelectedItemsBindable

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(
            nameof(IsOpen),
            typeof(bool),
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsOpenChanged));

    /// <summary>下拉是否展开。</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (MultiComboBox)d;
        bool isOpen = (bool)e.NewValue;

        var window = System.Windows.Window.GetWindow(box);
        if (window is null)
        {
            return;
        }

        // 懒初始化——在第一次需要时创建并缓存
        if (box._globalPreviewMouseDownHandler is null)
        {
            box._globalPreviewMouseDownHandler = box.OnGlobalPreviewMouseDown;
        }

        if (isOpen)
        {
            // 注册全局 PreviewMouseDown——任何点击都先经过窗口，可判断是否在 popup 子树内
            // 这种"全局监听"方式比 Popup.StaysOpen=False 的鼠标捕获机制更稳，不会和 ToggleButton 的双向绑定打架
            window.AddHandler(
                System.Windows.UIElement.PreviewMouseDownEvent,
                box._globalPreviewMouseDownHandler,
                handledEventsToo: true);
        }
        else
        {
            window.RemoveHandler(
                System.Windows.UIElement.PreviewMouseDownEvent,
                box._globalPreviewMouseDownHandler);
        }
    }

    #endregion IsOpen

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register(
            nameof(Watermark),
            typeof(string),
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(string.Empty));

    /// <summary>没有任何选中项时显示的占位文字。默认空字符串。</summary>
    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    #endregion Watermark

    #region MaxDropDownHeight

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double),
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(200.0));

    /// <summary>下拉面板最大高度。默认 200。命名沿用 WPF <see cref="ComboBox.MaxDropDownHeight"/> 惯例。</summary>
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    #endregion MaxDropDownHeight

    #region IsSelectAllVisible

    public static readonly DependencyProperty IsSelectAllVisibleProperty =
        DependencyProperty.Register(
            nameof(IsSelectAllVisible),
            typeof(bool),
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(false));

    /// <summary>是否在下拉顶部显示"全选"复选框。默认 false。</summary>
    public bool IsSelectAllVisible
    {
        get => (bool)GetValue(IsSelectAllVisibleProperty);
        set => SetValue(IsSelectAllVisibleProperty, value);
    }

    #endregion IsSelectAllVisible

    #region SelectAllText

    public static readonly DependencyProperty SelectAllTextProperty =
        DependencyProperty.Register(
            nameof(SelectAllText),
            typeof(string),
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata("Select All"));

    /// <summary>"全选"复选框旁的文字。默认 <c>"Select All"</c>。</summary>
    public string SelectAllText
    {
        get => (string)GetValue(SelectAllTextProperty);
        set => SetValue(SelectAllTextProperty, value);
    }

    #endregion SelectAllText

    #region IsClearButtonVisible

    public static readonly DependencyProperty IsClearButtonVisibleProperty =
        DependencyProperty.Register(
            nameof(IsClearButtonVisible),
            typeof(bool),
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(true));

    /// <summary>是否显示清除按钮。默认 true。即便为 true，没有选中项时也会自动隐藏。</summary>
    public bool IsClearButtonVisible
    {
        get => (bool)GetValue(IsClearButtonVisibleProperty);
        set => SetValue(IsClearButtonVisibleProperty, value);
    }

    #endregion IsClearButtonVisible

    #region SelectionChipTemplate

    public static readonly DependencyProperty SelectionChipTemplateProperty =
        DependencyProperty.Register(
            nameof(SelectionChipTemplate),
            typeof(DataTemplate),
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(default(DataTemplate), OnSelectionChipTemplateChanged));

    /// <summary>
    /// 顶部每个选中项 chip 的视觉模板。<c>DataContext</c> 是数据项本身。
    /// <para>
    /// 用户自定义 chip 时如需保留删除功能，需在新模板中包含一个绑定 <see cref="RemoveItemCommand"/> 的按钮：
    /// </para>
    /// <code>
    /// &lt;Button Command="{x:Static cy:MultiComboBox.RemoveItemCommand}" CommandParameter="{Binding}" /&gt;
    /// </code>
    /// <para>为 null 时使用控件内置的默认 chip 视觉（边框 + 文本 + ×）。</para>
    /// </summary>
    public DataTemplate SelectionChipTemplate
    {
        get => (DataTemplate)GetValue(SelectionChipTemplateProperty);
        set => SetValue(SelectionChipTemplateProperty, value);
    }

    private static void OnSelectionChipTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MultiComboBox box)
        {
            box.RefreshDefaultChipTemplate();
        }
    }

    #endregion SelectionChipTemplate

    #region HasSelectedItems

    private static readonly DependencyPropertyKey HasSelectedItemsPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasSelectedItems),
            typeof(bool),
            typeof(MultiComboBox),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty HasSelectedItemsProperty = HasSelectedItemsPropertyKey.DependencyProperty;

    /// <summary>只读。是否有任何选中项。给模板 Trigger 用（控制清除按钮、水印的可见性）。</summary>
    public bool HasSelectedItems
    {
        get => (bool)GetValue(HasSelectedItemsProperty);
        private set => SetValue(HasSelectedItemsPropertyKey, value);
    }

    #endregion HasSelectedItems

    #region Private helpers (popup outside-click detection)

    /// <summary>
    /// 全局鼠标按下时检查是否点在 popup 子树外——是则关闭下拉。
    /// 不依赖 Mouse.Capture，避免和 ToggleButton 双向绑定的捕获/焦点逻辑冲突。
    /// </summary>
    private void OnGlobalPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsOpen)
        {
            return;
        }

        // OriginalSource 是被点击的最内层元素
        if (e.OriginalSource is DependencyObject origin && IsInThisControlOrPopup(origin))
        {
            return; // 点的是控件本身或 popup 内的东西——不关闭
        }

        IsOpen = false;
    }

    /// <summary>
    /// 检查 origin 是否在本控件视觉树内（包括 popup 内的 ListBoxItem 等）。
    /// popup 的 visual parent 链跨越窗口边界，需要 visual + logical 双路上溯。
    /// </summary>
    private bool IsInThisControlOrPopup(DependencyObject origin)
    {
        var current = origin;
        while (current is not null)
        {
            if (ReferenceEquals(current, this))
            {
                return true;
            }
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(current)
                       ?? LogicalTreeHelper.GetParent(current);
            if (ReferenceEquals(parent, current))
            {
                break;
            }
            current = parent;
        }
        return false;
    }

    #endregion Private helpers (popup outside-click detection)

    #region RoutedCommands

    /// <summary>清空所有选中项。可在模板中通过 <c>Command="{x:Static cy:MultiComboBox.ClearCommand}"</c> 引用。</summary>
    public static readonly RoutedCommand ClearCommand = new(nameof(ClearCommand), typeof(MultiComboBox));

    /// <summary>
    /// 移除单个选中项。<c>CommandParameter</c> 是要移除的数据项。
    /// chip 上的 × 按钮通过此命令实现取消选中。
    /// </summary>
    public static readonly RoutedCommand RemoveItemCommand = new(nameof(RemoveItemCommand), typeof(MultiComboBox));

    private static void OnClearCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is MultiComboBox box)
        {
            box.UnselectAll();
        }
    }

    private static void OnClearCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is MultiComboBox box && box.SelectedItems.Count > 0;
    }

    private static void OnRemoveItemCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is MultiComboBox box && e.Parameter is object item && box.SelectedItems.Contains(item))
        {
            // 从基类 SelectedItems 移除——会触发 OnSelectionChanged 自动镜像同步到 SelectedItemsBindable
            box.SelectedItems.Remove(item);
            // 标 Handled 阻止冒泡，避免点击 × 同时触发外层"打开下拉"行为
            e.Handled = true;
        }
    }

    #endregion RoutedCommands

    #region Override (ItemsControl)

    /// <summary>给每个数据项创建一个 MultiComboBoxItem 容器（替代默认的 ListBoxItem）。</summary>
    protected override DependencyObject GetContainerForItemOverride() => new MultiComboBoxItem();

    /// <summary>已经是 MultiComboBoxItem 的元素直接当容器，不再包一层。</summary>
    protected override bool IsItemItsOwnContainerOverride(object item) => item is MultiComboBoxItem;

    #endregion Override (ItemsControl)

    #region Override (Selector)

    /// <summary>
    /// 选中项变化（来自基类——任何渠道：鼠标 / 键盘 / 直接 Add 到内部 SelectedItems / Ctrl+A）。
    /// 我们镜像写到外部 SelectedItemsBindable，让 MVVM 拿到。
    /// </summary>
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (_suppressSync)
        {
            return;
        }

        // 把基类 SelectedItems 镜像到 SelectedItemsBindable
        var bindable = SelectedItemsBindable;
        if (bindable is null)
        {
            return;
        }

        _suppressSync = true;
        try
        {
            // 移除：不在新选中集合里的旧项
            for (int i = bindable.Count - 1; i >= 0; i--)
            {
                if (!base.SelectedItems.Contains(bindable[i]))
                {
                    bindable.RemoveAt(i);
                }
            }
            // 添加：新选中但还不在 bindable 里的
            foreach (var item in base.SelectedItems)
            {
                if (!bindable.Contains(item))
                {
                    bindable.Add(item);
                }
            }
        }
        finally
        {
            _suppressSync = false;
        }

        UpdateSelectionState();
    }

    #endregion Override (Selector)

    #region Override (FrameworkElement)

    /// <summary>
    /// 键盘：Esc 关闭下拉、Alt+下打开下拉、Tab 关下拉。
    /// 其余（Space / 方向键 / Ctrl+A）由 ListBox 基类处理。
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape when IsOpen:
                IsOpen = false;
                e.Handled = true;
                return;

            case Key.System when e.SystemKey == Key.Down:
                // Alt+Down：标准 ComboBox 行为——打开下拉
                IsOpen = !IsOpen;
                e.Handled = true;
                return;

            case Key.Tab when IsOpen:
                IsOpen = false;
                // 不 Handle——让焦点正常 Tab 走
                break;
        }

        base.OnKeyDown(e);
    }

    public override void OnApplyTemplate()
    {
        // 取消旧引用的事件订阅
        if (_selectAllCheckBox is not null)
        {
            _selectAllCheckBox.Checked -= SelectAllCheckBox_Checked;
            _selectAllCheckBox.Unchecked -= SelectAllCheckBox_Unchecked;
        }

        base.OnApplyTemplate();

        _selectAllCheckBox = GetTemplateChild(PART_SelectAllCheckBox) as CheckBox;
        if (_selectAllCheckBox is not null)
        {
            _selectAllCheckBox.Checked += SelectAllCheckBox_Checked;
            _selectAllCheckBox.Unchecked += SelectAllCheckBox_Unchecked;
        }

        _chipPanel = GetTemplateChild(PART_ChipPanel) as ItemsControl;
        RefreshDefaultChipTemplate();

        UpdateSelectionState();
    }

    #endregion Override (FrameworkElement)

    #region 双向同步：外部 SelectedItemsBindable 变化 → 基类 SelectedItems

    private static void OnSelectedItemsBindableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (MultiComboBox)d;

        if (e.OldValue is INotifyCollectionChanged oldNotify)
        {
            oldNotify.CollectionChanged -= control.OnBindableCollectionChanged;
        }

        if (e.NewValue is INotifyCollectionChanged newNotify)
        {
            newNotify.CollectionChanged += control.OnBindableCollectionChanged;
        }

        control.SyncBindableToInternal();
        control.UpdateSelectionState();
    }

    private void OnBindableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (_suppressSync)
        {
            return;
        }
        SyncBindableToInternal();
    }

    /// <summary>
    /// 把 <see cref="SelectedItemsBindable"/> 的内容同步到基类 <see cref="ListBox.SelectedItems"/>。
    /// 用 SetSelectedItems(IEnumerable) 一次性设置，避免一项项 Add 引发多次 SelectionChanged。
    /// </summary>
    private void SyncBindableToInternal()
    {
        if (SelectedItemsBindable is null)
        {
            return;
        }

        _suppressSync = true;
        try
        {
            // 过滤：只保留 ItemsSource 里实际存在的项（防止脏引用）
            var validItems = SelectedItemsBindable.Cast<object>()
                .Where(item => Items.Contains(item))
                .ToList();
            SetSelectedItems(validItems);
        }
        finally
        {
            _suppressSync = false;
        }
    }

    #endregion 双向同步：外部 SelectedItemsBindable 变化 → 基类 SelectedItems

    #region SelectAll CheckBox

    private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (_suppressSync)
        {
            return;
        }
        SelectAll();
    }

    private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_suppressSync)
        {
            return;
        }
        UnselectAll();
    }

    #endregion SelectAll CheckBox

    #region Chip Template 动态生成

    /// <summary>
    /// 当 SelectionChipTemplate 为 null 时，根据当前 DisplayMemberPath 动态生成默认 chip 模板：
    /// 边框 + 文本（按 DisplayMemberPath 取字段）+ 删除 × 按钮。
    /// 用户显式设置 SelectionChipTemplate 时不覆盖。
    /// 用 XamlReader.Parse 生成 DataTemplate——WPF 动态构造模板的标准做法。
    /// </summary>
    private void RefreshDefaultChipTemplate()
    {
        if (_chipPanel is null)
        {
            return;
        }

        // 用户给了自定义模板：尊重，不动
        if (SelectionChipTemplate is not null)
        {
            _chipPanel.ItemTemplate = SelectionChipTemplate;
            return;
        }

        // 动态生成默认 chip 模板：根据 DisplayMemberPath 决定文本 binding 路径
        // - 设了 DisplayMemberPath="Name" → Text="{Binding Name}"
        // - 没设 → Text="{Binding}"（用 ToString）
        var textBindingPath = string.IsNullOrEmpty(DisplayMemberPath) ? string.Empty : DisplayMemberPath;
        var textBinding = string.IsNullOrEmpty(textBindingPath) ? "{Binding}" : $"{{Binding {textBindingPath}}}";

        var xaml = $@"
<DataTemplate
    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    xmlns:cy='clr-namespace:Cyclone.Wpf.Controls;assembly=Cyclone.Wpf'>
    <Border Margin='2' Padding='6,2,2,2'
            Background='{{DynamicResource Background.Header}}'
            BorderBrush='{{DynamicResource Border.Default}}'
            BorderThickness='1'>
        <StackPanel Orientation='Horizontal'>
            <TextBlock VerticalAlignment='Center' Text='{textBinding}' />
            <Button Margin='6,0,0,0' VerticalAlignment='Center'
                    Command='{{x:Static cy:MultiComboBox.RemoveItemCommand}}'
                    CommandParameter='{{Binding}}'
                    Style='{{DynamicResource MultiComboBox.ChipDeleteButton.Style.Basic}}'
                    ToolTip='Remove' />
        </StackPanel>
    </Border>
</DataTemplate>";

        try
        {
            var template = (DataTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
            _chipPanel.ItemTemplate = template;
        }
        catch
        {
            // xaml 解析失败时退化——清空模板让 ItemsControl 用默认 ToString 渲染
            _chipPanel.ItemTemplate = null;
        }
    }

    #endregion Chip Template 动态生成

    #region 状态更新

    /// <summary>
    /// 选中项变化时刷新派生状态：HasSelectedItems / 全选 CheckBox 三态。
    /// </summary>
    private void UpdateSelectionState()
    {
        HasSelectedItems = base.SelectedItems.Count > 0;
        UpdateSelectAllCheckBoxState();
    }

    private void UpdateSelectAllCheckBoxState()
    {
        if (_selectAllCheckBox is null || !IsSelectAllVisible)
        {
            return;
        }

        _suppressSync = true;
        try
        {
            int total = Items.Count;
            int selected = base.SelectedItems.Count;
            // 三态：全选 / 部分选 / 全未选
            _selectAllCheckBox.IsChecked = total > 0 && selected == total
                ? true
                : selected == 0 ? false : null;
        }
        finally
        {
            _suppressSync = false;
        }
    }

    #endregion 状态更新
}