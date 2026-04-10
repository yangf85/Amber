# Cyclone.Amber（琥珀）— 控件设计文档

> **Version 0.0.0.2** · .NET Framework 4.8 / .NET 8.0-Windows（双目标）  
> 仓库：https://github.com/yangf85/Amber

---

## 一、框架概览

### 1.1 设计思路

1. **主题驱动**：通过 `ThemeManager`（`ResourceDictionary` 子类）统一管理主题资源，内置 Basic / Light / Dark 三套主题 主题是无圆角风格(类似WIN10)，运行时一行代码切换，支持继承 `Theme` 基类扩展自定义主题。所有颜色通过 `BrushKey` / `StyleKey` 枚举引用，保证主题一致性。

2. **MVVM 优先**：控件属性全面支持双向绑定；服务层（Alert / Notification / Dialog）均为单例接口，ViewModel 通过 `IDialogRequestClose` 等接口驱动 UI，不依赖 View 引用。

3. **附加属性增强原生控件**：通过 `TextBoxHelper`、`PasswordBoxHelper`、`RippleEffect` 等 Helper 类，以附加属性方式为原生 WPF 控件添加水印、清除按钮、水波纹动效等能力，无需替换控件。

4. **静态转换器集合**：`BooleanConverter`、`VisibilityConverter` 等基于泛型 `FuncValueConverter<TFrom, TTo>` 实现，以 `x:Static` 直接引用，免去资源字典注册。

5. **服务层解耦 UI 交互**：`AlertService`（模态弹框）、`NotificationService`（通知气泡）、`DialogService`（MVVM 对话框）三大服务均为线程安全单例，支持同步/异步调用和验证回调。

### 1.2 控件清单

#### 交互控件

| 控件 | 说明 |
|------|------|
| SwitchButton | 滑动开关，带平滑动画 |
| MultiComboBox | 多选下拉框，支持全选 |
| ColorPicker | 下拉颜色选择器（色盘 + 预设色板 + 取色器） |
| RangeSlider | 双端范围滑块 |
| NumberBox | 数字输入框，带步长和范围限制 |
| TimePicker | 时间选择器，支持 12/24 小时制 |
| DateRangePicker | 日期范围选择器，内置预定义快捷选项 |
| CascadePicker | 多级联动选择器（如省市区） |
| TreeSelector | 下拉树形选择器 |
| EnumSelector | 枚举自动转单选按钮组 |
| TransferBox | 穿梭框 |
| SplitButton | 分割按钮（主按钮 + 下拉菜单） |
| RadialMenu | 圆形放射状菜单 |
| PopupBox | 浮动弹出框 |
| HyperlinkButton | 超链接按钮 |
| ValidationContent | 内联验证错误提示容器 |

#### 展示控件

| 控件 | 说明 |
|------|------|
| Card | 三段式卡片（标题 / 内容 / 页脚） |
| BreadCrumbBar | 面包屑导航 |
| Stepper | 多步骤进度条 |
| Pagination | 分页器 |
| Carousel | 轮播图 |
| TransitionBox | 过渡动画容器（淡入、滑动、翻转、缩放） |
| CircularGauge | 圆形仪表盘 |
| Countdown | 倒计时 |
| RunningBlock | 跑马灯 |
| LcdDisplayer | LCD 七段数字显示器 |
| HighlightTextBlock | 关键词高亮文本 |
| CopyableTextBlock | 可复制文本块 |
| EditableTextBlock | 双击可编辑文本块 |
| SectionHeader | 分组标题 |
| HintBox | 智能提示输入框 |
| Drawer | 侧边抽屉面板 |
| Form / FormItem | 语义化表单布局 |
| SideMenu | 侧边导航菜单 |
| FluidTabControl | 流体切换动画标签页 |
| IconBox | 统一图标展示控件 |

#### 布局面板

| 控件 | 说明 |
|------|------|
| SpacingStackPanel | 带间距和 Star 权重的 StackPanel |
| SpacingUniformGrid | 带行列间距的 UniformGrid |
| WaterfallPanel | 瀑布流布局 |
| TilePanel | 磁贴面板 |
| FisheyePanel | 鱼眼放大效果面板（macOS Dock 风格） |
| CyclicPanel | 循环滚动面板 |

#### 加载指示器

| 控件 | 说明 |
|------|------|
| LoadingRing | 旋转圆环 |
| LoadingPulse | 脉冲跳动点阵 |
| LoadingChase | 追逐旋转点 |
| LoadingFlipCube | 三维翻转立方块 |
| LoadingParticle | 粒子散射 |
| LoadingTesseract | 四维超正方体旋转 |
| LoadingBox / LoadingAdorner | 加载遮罩（附加属性版） |

#### 高级窗口

| 控件 | 说明 |
|------|------|
| AdvancedWindow | 自定义标题栏窗口，内置最小化/最大化/置顶命令 |

### 1.3 值转换器清单

所有转换器基于 `FuncValueConverter<TIn, TOut>` 实现，以 `x:Static` 引用，命名空间 `Cyclone.Wpf.Converters`。

#### BooleanConverter

| 转换器 | 输入 → 输出 | 说明 |
|--------|-------------|------|
| ToVisibility | bool → Visibility | true=Visible, false=Collapsed |
| Inverse | bool → bool | 取反 |
| StringEquality | (string, string) → bool | 字符串相等 |
| StringNotEquality | (string, string) → bool | 字符串不等 |
| NullToBoolean | object → bool | null=true |
| NotNullToBoolean | object → bool | 非null=true |
| Equals | (object, object) → bool | 值等于参数 |
| NotEquals | (object, object) → bool | 值不等于参数 |
| IsEmpty | IEnumerable → bool | 集合为空 |
| IsNotEmpty | IEnumerable → bool | 集合非空 |
| IsPositive | double → bool | 大于0 |
| IsZero | double → bool | 等于0 |

#### VisibilityConverter

| 转换器 | 输入 → 输出 | 说明 |
|--------|-------------|------|
| VisibleWhenTrue | bool? → Visibility | true=Visible, false=Collapsed, null=Hidden |
| VisibleWhenFalse | bool? → Visibility | 取反逻辑 |
| VisibleWhenNullOrEmpty | string → Visibility | 空串时可见 |
| VisibleWhenNotNullOrEmpty | string → Visibility | 非空串时可见 |
| VisibleWhenNull | object → Visibility | null时可见 |
| VisibleWhenNotNull | object → Visibility | 非null时可见 |
| VisibleWhenEmpty | IEnumerable → Visibility | 集合为空时可见 |
| VisibleWhenNotEmpty | IEnumerable → Visibility | 集合非空时可见 |
| VisibleWhenPositive | double → Visibility | 大于0时可见 |
| VisibleWhenZero | double → Visibility | 等于0时可见 |
| VisibleWhenNotZero | double → Visibility | 不等于0时可见 |
| VisibleWhenEquals | (object, object) → Visibility | 值等于参数时可见 |
| VisibleWhenNotEquals | (object, object) → Visibility | 值不等于参数时可见 |

#### MathConverter

| 转换器 | 输入 → 输出 | 说明 |
|--------|-------------|------|
| Scale | (double, double) → double | 按比例缩放 |
| Half | double → double | 取半 |
| Subtraction | (double, double) → double | 减法 |
| Addition | (double, double) → double | 加法 |
| Multiplication | (double, double) → double | 乘法 |
| Division | (double, double) → double | 除法 |
| Negate | double → double | 取反（×-1） |
| Abs | double → double | 绝对值 |
| Ceiling | double → double | 向上取整 |
| Floor | double → double | 向下取整 |
| Round | (double, double) → double | 四舍五入到指定小数位 |
| Clamp | (double, "min,max") → double | 限制范围 |
| Count | IEnumerable → int | 集合元素数量 |
| ToPercent | double → string | 百分比格式化（0.75→"75%"） |
| ToFixed | (double, double) → string | 保留指定小数位 |
| AddOne | int → int | 加一 |
| ObjectsToIndexes | IEnumerable → IEnumerable | 对象集合转索引集合 |

#### StringConverter

| 转换器 | 输入 → 输出 | 说明 |
|--------|-------------|------|
| ToUpper | string → string | 转大写 |
| ToLower | string → string | 转小写 |
| Truncate | (string, string) → string | 截断并加省略号 |
| Format | (object, string) → string | 格式化（{0}占位） |
| Prefix | (string, string) → string | 添加前缀 |
| Suffix | (string, string) → string | 添加后缀 |
| DefaultIfEmpty | (string, string) → string | 空时显示默认文本 |

#### BrushConverter

| 转换器 | 输入 → 输出 | 说明 |
|--------|-------------|------|
| BooleanToBrush | bool → Brush | true=绿, false=红 |
| IntToBrush | int → Brush | -1=黄, <1=红, 其他=绿 |
| HexToBrush | string → Brush | 十六进制颜色字符串转Brush |
| ProgressBrush | double → Brush | 0~1 红绿渐变 |
| WithOpacity | (Brush, double) → Brush | 设置透明度 |

#### EnumConverter

| 转换器 | 输入 → 输出 | 说明 |
|--------|-------------|------|
| ToDescription | Enum → string | 读取 Description 特性 |
| IsEqual | (Enum, Enum) → bool | 枚举相等比较（支持双向） |

### 1.4 主题资源键

所有控件样式通过 `DynamicResource` 引用以下资源键，自定义主题时须覆盖对应键值。

#### Background

`Global` · `Default` · `Invert` · `Active` · `Inactive` · `Disabled` · `Hover` · `Pressed` · `UnChecked` · `Checked` · `Focused` · `Highlighted` · `Editing` · `Loading` · `Dragging` · `Selected` · `InputError` · `Warning` · `Error` · `Success` · `Info` · `Close` · `Mask` · `Container` · `Header` · `Caption` · `Transition` · `TransparentLight` · `TransparentMedium` · `TransparentDark`

#### Foreground

`Global` · `Default` · `Active` · `Inactive` · `Disabled` · `Hover` · `Pressed` · `Checked` · `Focused` · `Highlighted` · `Editing` · `Loading` · `Dragging` · `Selected` · `InputError` · `Warning` · `Error` · `Success` · `Info` · `Close` · `Invert` · `Container` · `Header` · `Caption` · `Mask` · `Link` · `Placeholder` · `Transition`

#### Border

`Global` · `Default` · `Active` · `Inactive` · `Disabled` · `Hover` · `Pressed` · `Checked` · `Focused` · `Highlighted` · `Editing` · `Loading` · `Dragging` · `Selected` · `InputError` · `Warning` · `Error` · `Success` · `Info` · `Close` · `Container` · `Header` · `Caption` · `Mask` · `Transition` · `FocusRing`

#### Text

`Title` · `Subtitle` · `Header` · `Caption` · `Content` · `Prompt` · `Invert`

#### Icon.Foreground

`Default` · `Hover` · `Checked` · `Focused` · `Selected` · `Pressed` · `Editing` · `Dragging` · `Success` · `Error` · `Warning` · `Info` · `Disabled` · `Link` · `Invert`

#### 其他画刷

| 键名 | 说明 |
|------|------|
| Divider.Default | 分割线 |
| Overlay.Light / Dark | 遮罩层 |
| Link.Default / Hover | 超链接 |
| Tick.Primary / Secondary | 刻度线 |
| Alternation.Dark / Light / Invert | 交替行 |
| Shadow.Default | 阴影 |
| Highlight.Default / Invert | 高亮 |

#### 尺寸令牌

| 类别 | 键名 | 默认值 |
|------|------|--------|
| 高度 | Height.Horizontal.Caption / Header / Control / Item | 40 / 36 / 32 / 28 |
| 宽度 | Width.Vertical.Caption / Header / Control / Item | 48 / 40 / 36 / 32 |
| 字号 | Font.Title / Subtitle / Header / Body / Input / Prompt | 16 / 15 / 14 / 12 / 12 / 11 |
| 边框 | BorderThickness.None / Thin / Medium / Thick | 0 / 1 / 2 / 3 |
| 圆角 | CornerRadius.None / Small / Medium / Large | 0 / 2 / 4 / 8 |
| 间距 | Spacing.Horizontal / Vertical | 8 / 8 |
| 图标 | Icon.Small / Medium / Large | 16 / 20 / 24 |

#### Margin / Padding

| 键名 | 值 |
|------|-----|
| *.Left | 8,4,4,4 |
| *.Right | 4,4,8,4 |
| *.Top | 4,8,4,4 |
| *.Bottom | 4,4,4,8 |
| *.Horizontal | 8,4 |
| *.Vertical | 4,8 |
| *.All | 8 |

---

## 二、编码规范

### 2.1 C# 规范（C# 14）

**文件作用域命名空间**：所有控件统一使用 `Cyclone.Wpf.Controls`，不按子目录追加层级，避免过深的命名空间。

```csharp
// ✅ 正确 — 无论控件放在哪个子目录，命名空间始终为：
namespace Cyclone.Wpf.Controls;

// ❌ 禁止 — 不要按目录结构嵌套命名空间：
// namespace Cyclone.Wpf.Controls.ColorPicker;
// namespace Cyclone.Wpf.Controls.Panel;
```

**不省略大括号，不压行**：

```csharp
// ✅ 正确
if (value is null)
{
    return;
}

// ❌ 禁止
if (value is null) return;
```

**Region 顺序**：

```
静态构造函数 → 实例构造函数
→ #region DependencyProperties（每个属性一个 sub-region）
→ #region RoutedEvents
→ #region Commands
→ #region Override Methods
→ #region Private Methods
```

**依赖属性标准写法**（静态字段 → CLR 包装器 → 静态回调）：

```csharp
#region Title

public static readonly DependencyProperty TitleProperty =
    DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(MyControl),
        new PropertyMetadata(string.Empty, OnTitleChanged));

/// <summary>
/// 获取或设置控件标题文本。
/// </summary>
public string Title
{
    get => (string)GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
}

private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    var control = (MyControl)d;
    control.UpdateTitle();
}

#endregion Title
```

**模板部件**：`[TemplatePart]` 标注 + `const string` 常量 + `nameof` 引用，避免魔法字符串；字段使用下划线命名。

```csharp
[TemplatePart(Name = nameof(PART_ContentHost), Type = typeof(ContentPresenter))]
public class MyControl : Control
{
    private const string PART_ContentHost = "PART_ContentHost";

    private ContentPresenter _contentHost;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _contentHost = GetTemplateChild(PART_ContentHost) as ContentPresenter;
    }
}
```

**Null 传播语法（C# 14）**：对可能为 `null` 的模板部件，使用 `?.` 简化空判断，包括属性赋值和事件订阅/取消订阅。

```csharp
// ✅ 正确 — 使用 null 传播
_slider?.ValueChanged += OnSliderValueChanged;
_slider?.ValueChanged -= OnSliderValueChanged;
_slider?.Value = 0.0;

// ❌ 禁止 — 冗余的 null 判断
if (_slider is not null)
{
    _slider.ValueChanged += OnSliderValueChanged;
}
```

### 2.2 XAML 规范

**命名空间简写**（样式文件统一使用）：

```xml
xmlns:conv="clr-namespace:Cyclone.Wpf.Converters"
xmlns:ctl="clr-namespace:Cyclone.Wpf.Controls"
xmlns:hp="clr-namespace:Cyclone.Wpf.Helpers"
```

**资源引用路径**（每个控件样式文件头部合并）：

```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/Cyclone.Wpf;component/Themes/BasicTheme.xaml" />
    <ResourceDictionary Source="pack://application:,,,/Cyclone.Wpf;component/Styles/Control.xaml" />
</ResourceDictionary.MergedDictionaries>
```

**资源键命名**：

| 类别 | 格式 | 示例 |
|------|------|------|
| 样式 | `控件名.Style.主题名` | `TagBox.Style.Basic` |
| 模板 | `控件名.Template.主题名` | `TagBox.Template.Basic` |
| 内部样式 | `控件名.描述Style` | `TagBox.ClearButtonStyle` |

**默认样式**不带 `x:Key`，直接 `TargetType`。

**颜色一律 `DynamicResource`，禁止硬编码**：

```xml
<!-- ✅ -->
<Setter Property="Background" Value="{DynamicResource Background.Control}" />
<!-- ❌ -->
<Setter Property="Background" Value="#FFFFFF" />
```


### 2.3 文件结构

```
Controls/
└── MyControl/
    ├── MyControl.cs          # 控件逻辑
    ├── MyControlItem.cs      # 子项控件（如有）
    └── MyControl.xaml        # 样式与模板
```

---

## 三、控件设计模板

新增控件时，按以下表格填写设计说明，然后按规范实现代码。

### 设计说明

| 项目 | 内容 |
|------|------|
| 控件名 | TagBox |
| 用途 | 标签输入框，回车添加、点击删除 |
| 继承自 | ItemsControl |
| 子项容器 | TagBoxItem : ContentControl |

### 模板部件

| 部件名 | 类型 | 说明 |
|--------|------|------|
| PART_InputBox | TextBox | 输入区域 |
| PART_ClearButton | Button | 清空按钮 |

### 依赖属性

| 属性名 | 类型 | 默认值 | 说明 |
|--------|------|--------|------|
| Tags | IList | null | 标签集合（双向绑定） |
| Watermark | string | "" | 占位提示 |
| MaxTags | int | MaxValue | 最大数量 |
| IsReadOnly | bool | false | 只读模式 |

### 路由事件

| 事件名 | 说明 |
|--------|------|
| TagAdded | 标签添加后触发 |
| TagRemoved | 标签移除后触发 |

### 命令

| 命令 | 说明 |
|------|------|
| ClearAllCommand | 清空所有标签 |
| RemoveTagCommand | 移除指定标签 |

### XAML 用法

```xml
<c:TagBox Tags="{Binding UserTags}"
          Watermark="输入后按回车"
          MaxTags="10" />
```

### 控件创建完成后需要在Cyclone.Wpf.Demo项目 添加一个使用案例
简单的控件可以和其他类似的控件放在一个页面 复杂的控件可以单独创建一个页面展示其功能和用法