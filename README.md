# 🟡 Cyclone.Amber（琥珀）WPF UI 控件库 — 完整文档

> **Version 0.9.9.6** · 目标框架：`.NET Framework 4.8` / `.NET 8.0-Windows`
> 仓库地址：[https://github.com/yangf85/Amber](https://github.com/yangf85/Amber)

---

## 目录

1. [项目概述](#1-项目概述)
2. [快速上手](#2-快速上手)
3. [控件参考 — 交互控件](#3-控件参考--交互控件)
4. [控件参考 — 展示控件](#4-控件参考--展示控件)
5. [控件参考 — 布局面板](#5-控件参考--布局面板)
6. [控件参考 — 加载指示器](#6-控件参考--加载指示器)
7. [控件参考 — 高级窗口](#7-控件参考--高级窗口)
8. [服务层 API](#8-服务层-api)
9. [主题系统](#9-主题系统)
10. [辅助工具类](#10-辅助工具类)
11. [值转换器](#11-值转换器)

---

## 1. 项目概述

### 1.1 简介

Cyclone.Amber（琥珀）是一个功能丰富的 WPF UI 控件库，基于 **Cyclone.Wpf** 程序集发布，旨在为开发者提供多样化的界面组件和主题管理工具，简化现代化 WPF 应用程序的开发过程。该库覆盖从交互控件、展示控件到高级功能（服务层、主题切换）的广泛功能域，全面支持 MVVM 模式与数据绑定。

### 1.2 项目结构

```
Amber/
├── Cyclone.Wpf/                  # 核心控件库项目（主程序集）
│   ├── Controls/                 # 所有 UI 控件源码（按功能分子目录）
│   │   ├── Alert/                # 模态弹框服务
│   │   ├── Notification/         # 通知气泡服务
│   │   ├── Dialog/               # 对话框服务接口
│   │   ├── AdvancedWindow/       # 自定义标题栏窗口
│   │   ├── Loading/              # 加载指示器系列
│   │   ├── Panel/                # 自定义布局面板
│   │   ├── ColorPicker/          # 颜色选择器
│   │   ├── TransitionBox/        # 过渡动画容器
│   │   └── ...（其他控件）
│   ├── Converters/               # 值转换器
│   ├── Helpers/                  # 附加属性辅助类
│   └── Themes/                   # 主题资源字典及主题管理器
├── Cyclone.Wpf.Demo/             # WPF 演示应用程序
│   ├── Views/                    # 各控件 Demo 页面
│   └── ViewModels/               # Demo 视图模型
└── Wpf.sln                       # Visual Studio 解决方案文件
```

### 1.3 技术规格

| 规格项 | 值 |
|---|---|
| 目标框架 | .NET Framework 4.8 / .NET 8.0-Windows（双目标） |
| 程序集名称 | Cyclone.Wpf |
| 当前版本 | 0.9.9.6 |
| 编程语言 | C#（LangVersion: preview） |
| UI 框架 | WPF（Windows Presentation Foundation） |
| 包含不安全代码 | 是（AllowUnsafeBlocks = true） |
| NuGet 输出 | 自动打包，每次构建自动删除旧包 |

---

## 2. 快速上手

### 2.1 引用主题资源

在 `App.xaml` 中合并主题资源字典（**须放在资源列表最后**），以启用控件样式和默认主题：

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- 你自己的资源... -->
            <!-- Cyclone.Wpf 主题（必须最后加载）-->
            <themes:ThemeManager
                xmlns:themes="clr-namespace:Cyclone.Wpf.Themes;assembly=Cyclone.Wpf"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 2.2 引入 XML 命名空间

```xml
xmlns:c="clr-namespace:Cyclone.Wpf.Controls;assembly=Cyclone.Wpf"
xmlns:h="clr-namespace:Cyclone.Wpf.Helpers;assembly=Cyclone.Wpf"
xmlns:cv="clr-namespace:Cyclone.Wpf.Converters;assembly=Cyclone.Wpf"
xmlns:themes="clr-namespace:Cyclone.Wpf.Themes;assembly=Cyclone.Wpf"
```

### 2.3 切换主题（运行时）

```csharp
using Cyclone.Wpf.Themes;

// 切换到暗色主题
ThemeManager.CurrentTheme = ThemeManager.AvailableThemes
    .First(t => t is DarkTheme);

// 监听主题变更事件
ThemeManager.ThemeChanged += (s, e) => { /* 刷新 UI */ };
```

### 2.4 第一个控件示例

```xml
<!-- SwitchButton 示例 -->
<c:SwitchButton IsChecked="{Binding IsEnabled}"
               TrackWidth="56"
               TrackHeight="28"
               CheckedBackground="#409EFF"
               Content="启用功能"/>
```

```csharp
// 通知服务示例
NotificationService.Instance.Success("操作成功！");
```

---

## 3. 控件参考 — 交互控件

### 3.1 SwitchButton（开关按钮）

继承自 `ToggleButton`，实现类似移动端的滑动开关控件，切换时带有平滑动画效果。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `IsChecked` | `bool?` | 开关状态（继承自 ToggleButton） |
| `TrackWidth` | `double` | 轨道宽度，默认 50 |
| `TrackHeight` | `double` | 轨道高度，默认 26 |
| `ThumbSize` | `double` | 滑块尺寸，默认 22 |
| `ThumbMargin` | `Thickness` | 滑块与轨道边距，默认 2 |
| `TrackCornerRadius` | `CornerRadius` | 轨道圆角，默认 13 |
| `ThumbCornerRadius` | `CornerRadius` | 滑块圆角，默认 11 |
| `CheckedBackground` | `Brush` | 选中时轨道背景色，默认红色 |
| `UncheckedBackground` | `Brush` | 未选中时轨道背景色，默认灰色 |
| `ThumbBackground` | `Brush` | 滑块背景色，默认白色 |
| `AnimationDuration` | `Duration` | 滑动动画时长，默认 200ms |

```xml
<c:SwitchButton IsChecked="{Binding IsActive, Mode=TwoWay}"
               CheckedBackground="#67C23A"
               TrackWidth="60" TrackHeight="30">
    已启用
</c:SwitchButton>
```

---

### 3.2 MultiComboBox（多选下拉框）

支持多项选择的下拉框控件，提供全选功能、选中项文本拼接显示，以及一键清除按钮。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `SelectedItems` | `IList` | 已选中项的集合（双向绑定） |
| `Separator` | `string` | 选中项文本间的分隔符，默认 `", "` |
| `Watermark` | `string` | 未选择时显示的占位提示文字 |
| `IsShowSelectAll` | `bool` | 是否显示"全选"复选框 |
| `MaxDropDownHeight` | `double` | 下拉列表最大高度 |

```xml
<c:MultiComboBox ItemsSource="{Binding Cities}"
                SelectedItems="{Binding SelectedCities}"
                Watermark="请选择城市"
                IsShowSelectAll="True"/>
```

---

### 3.3 ColorPicker（颜色选择器）

下拉式颜色选择控件，内嵌三大功能模块：

- **ColorSelector** — 色盘 + 亮度滑块
- **ColorPalette** — 预设色板
- **ColorEyedropper** — 屏幕取色器

| 属性名 | 类型 | 说明 |
|---|---|---|
| `SelectedColor` | `Color` | 当前选中的颜色（双向绑定） |
| `IsShowEyedropper` | `bool` | 是否显示取色器按钮，默认 true |
| `IsShowPalette` | `bool` | 是否显示预设颜色板，默认 true |

```xml
<c:ColorPicker SelectedColor="{Binding ThemeColor, Mode=TwoWay}"
              IsShowEyedropper="True"/>
```

---

### 3.4 RangeSlider（范围滑块）

双端范围选择滑块，常用于价格区间、时间区间筛选场景。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `LowerValue` | `double` | 左侧滑块当前值（双向绑定） |
| `UpperValue` | `double` | 右侧滑块当前值（双向绑定） |
| `Minimum` | `double` | 允许的最小值 |
| `Maximum` | `double` | 允许的最大值 |

---

### 3.5 NumberBox（数字输入框）

专用数字输入控件，内置数值范围限制、步长控制，支持上下箭头键调整数值。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `Value` | `double?` | 当前数值（双向绑定） |
| `Minimum` | `double` | 允许的最小值 |
| `Maximum` | `double` | 允许的最大值 |
| `Increment` | `double` | 每次调整的步长 |

---

### 3.6 TimePicker（时间选择器）

时间选择控件，内含 `TimeSelector`（滚动选择时/分/秒），支持 12/24 小时制切换。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `SelectedTime` | `TimeSpan?` | 选中的时间（双向绑定） |
| `IsShowSeconds` | `bool` | 是否显示秒选择列 |
| `Is24HourFormat` | `bool` | 是否使用 24 小时格式 |

---

### 3.7 DateRangePicker（日期范围选择器）

支持选取起止日期区间，内置常用预定义日期范围（今天、本周、本月等）快速选取按钮。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `StartDate` | `DateTime?` | 起始日期（双向绑定） |
| `EndDate` | `DateTime?` | 结束日期（双向绑定） |
| `PredefineDates` | `IList` | 预定义日期范围选项集合 |

---

### 3.8 CascadePicker（级联选择器）

多级联动下拉选择控件，适合省市区三级联动等层级结构数据的选取场景。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `ItemsSource` | `IList` | 顶层数据源 |
| `SelectedItems` | `IList` | 各级已选中项的集合 |
| `DisplayMemberPath` | `string` | 各项显示文本的绑定路径 |
| `ChildrenPath` | `string` | 子级数据集合的绑定路径 |

---

### 3.9 TreeSelector（树形选择器）

下拉树形选择控件，支持在弹出层中展开树状结构并选择节点。

---

### 3.10 EnumSelector（枚举选择器）

自动将枚举类型转换为单选按钮组，配合 `EnumToItemsSourceExtension` 标记扩展和 `EnumAttributeTypeConverter` 支持在 XAML 中直接绑定枚举属性。

```xml
<c:EnumSelector SelectedValue="{Binding Status}"
               EnumType="{x:Type local:OrderStatus}"/>
```

---

### 3.11 TransferBox（穿梭框）

经典穿梭框（Transfer）控件，支持在左侧候选列表和右侧已选列表之间双向转移数据，常见于权限配置、人员选择等场景。

---

### 3.12 SplitButton（分割按钮）

结合主操作按钮与下拉菜单的复合控件，左侧为主按钮，右侧为下拉箭头，展开 `SplitButtonItem` 菜单列表。

---

### 3.13 RadialMenu（圆形菜单）

圆形放射状菜单控件，支持多层级菜单结构。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `Radius` | `double` | 外圆半径，默认 100 |
| `InnerRadius` | `double` | 内圆半径，默认 50 |
| `CenterContent` | `object` | 中心区域内容 |
| `SubMenuRadius` | `double` | 子菜单圆半径 |

---

### 3.14 PopupBox（弹出框）

点击触发的浮动弹出框控件，可承载任意内容，支持配置弹出方向和动画。

---

### 3.15 HyperlinkButton（超链接按钮）

样式类似超链接的按钮控件，支持点击后导航至 URL 或执行命令，默认带有下划线和颜色样式。

---

### 3.16 ValidationContent（验证内容）

提供内联错误提示的容器控件，配合数据验证（`INotifyDataErrorInfo`）在控件旁边显示验证错误信息。

---

## 4. 控件参考 — 展示控件

### 4.1 Card（卡片）

带标题栏、内容区和页脚区三段式布局的卡片容器控件，继承自 `ContentControl`。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `Title` | `string` | 卡片标题文本 |
| `Icon` | `object` | 标题栏图标（可为 Path、Image 等） |
| `HeaderBackground` | `Brush` | 标题栏背景色 |
| `HeaderForeground` | `Brush` | 标题栏前景色 |
| `Footer` | `object` | 页脚区内容 |
| `FooterBackground` | `Brush` | 页脚区背景色 |
| `FooterForeground` | `Brush` | 页脚区前景色 |

```xml
<c:Card Title="用户信息" HeaderBackground="#409EFF">
    <c:Card.Icon>
        <Path Data="..." Fill="White"/>
    </c:Card.Icon>
    <StackPanel>
        <TextBlock Text="张三"/>
    </StackPanel>
    <c:Card.Footer>
        <Button Content="编辑"/>
    </c:Card.Footer>
</c:Card>
```

---

### 4.2 BreadCrumbBar（面包屑导航）

面包屑路径导航控件，由 `BreadCrumbBarItem` 子项组成，子项之间自动添加分隔符，支持点击导航。

```xml
<c:BreadCrumbBar>
    <c:BreadCrumbBarItem Content="首页"/>
    <c:BreadCrumbBarItem Content="产品管理"/>
    <c:BreadCrumbBarItem Content="产品列表"/>
</c:BreadCrumbBar>
```

---

### 4.3 Stepper（步骤条）

引导用户完成多步骤任务的进度指示器，由 `StepperItem` 组成。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `CurrentIndex` | `int` | 当前活动步骤索引（从 0 开始） |
| `StepChanged` | `RoutedEvent` | 步骤变更事件，携带方向（Forward/Backward） |

```xml
<c:Stepper CurrentIndex="{Binding CurrentStep}">
    <c:StepperItem Content="填写信息"/>
    <c:StepperItem Content="确认信息"/>
    <c:StepperItem Content="完成"/>
</c:Stepper>
```

---

### 4.4 Pagination（分页器）

完整的数据分页控件，包含页码列表、上/下一页按钮、每页条数选择框和跳转到指定页功能，超长页码序列自动渲染省略号（`···`）。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `TotalCount` | `int` | 数据总条数 |
| `PageSize` | `int` | 每页显示条数（双向绑定） |
| `CurrentPage` | `int` | 当前页码（双向绑定） |
| `PageCount` | `int` | 总页数（只读，自动计算） |
| `PageSizeOptions` | `IList` | 可选每页条数列表 |

---

### 4.5 Carousel（轮播图）

支持自动播放和手动切换的内容轮播控件，由 `CarouselItem` 子项组成。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `AutoPlay` | `bool` | 是否自动轮播，默认 true |
| `Interval` | `double` | 自动播放时间间隔（毫秒），默认 3000 |
| `SelectedIndex` | `int` | 当前显示项索引 |

---

### 4.6 TransitionBox（过渡容器）

内容切换时播放过渡动画的容器控件，实现 `ITransition` 接口支持多种内置过渡效果：

| 过渡效果类 | 描述 |
|---|---|
| `FadeTransition` | 淡入淡出 |
| `SlideTransition` | 滑动切换 |
| `FlipTransition` | 翻转效果 |
| `ScaleTransition` | 缩放效果 |

| 属性名 | 类型 | 说明 |
|---|---|---|
| `Transition` | `ITransition` | 过渡效果实现类 |
| `TransitionDuration` | `Duration` | 动画持续时长，默认 300ms |

```xml
<c:TransitionBox>
    <c:TransitionBox.Transition>
        <c:FadeTransition/>
    </c:TransitionBox.Transition>
    <ContentControl Content="{Binding CurrentView}"/>
</c:TransitionBox>
```

---

### 4.7 CircularGauge（圆形仪表盘）

继承自 `RangeBase` 的圆形仪表盘控件，支持鼠标拖拽调整数值，绘制刻度线和数值标签。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `Value` | `double` | 当前值（继承自 RangeBase） |
| `Minimum / Maximum` | `double` | 最小/最大值 |
| `TickColor` | `Brush` | 刻度线颜色 |
| `LabelFontSize` | `double` | 刻度标签字号，默认 10 |
| `IsLabelInside` | `bool` | 标签是否显示在内侧 |
| `TickLengthRatio` | `double` | 刻度线长度比率 |

---

### 4.8 Countdown（倒计时）

内置定时器的倒计时控件，到达零时触发 `Completed` 事件。

---

### 4.9 RunningBlock（跑马灯）

水平滚动文字/内容的跑马灯控件，支持设置滚动速度和循环模式。

---

### 4.10 LcdDisplayer（LCD 数字显示器）

七段式 LCD 数字显示控件，由 `LcdDigit` 子控件组合构成，模拟仪器仪表风格的数字显示效果。

---

### 4.11 HighlightTextBlock（高亮文本块）

支持关键词高亮显示的文本控件，自动将匹配到搜索词的文字片段着色。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `Text` | `string` | 完整显示文本 |
| `HighlightText` | `string` | 要高亮的关键词 |
| `HighlightForeground` | `Brush` | 高亮文字前景色 |
| `HighlightBackground` | `Brush` | 高亮文字背景色 |

---

### 4.12 CopyableTextBlock（可复制文本块）

右键或点击按钮即可将文本内容复制到剪贴板的文本控件。

---

### 4.13 EditableTextBlock（可编辑文本块）

支持双击进入编辑模式的文本块控件，编辑完成后还原为文本显示，适合表格行内编辑场景。

---

### 4.14 SectionHeader（分组标题）

带横线装饰的分组标题控件，用于在表单或内容区中分隔不同功能区块。

---

### 4.15 HintBox（提示框）

浮动提示气泡控件，由 `HintBoxItem` 子项组成，支持上下左右四个弹出方向配置。

---

### 4.16 Drawer（抽屉面板）

从容器边缘滑入/滑出的侧边抽屉控件，支持遮罩层，适合侧边导航或设置面板场景。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `DrawerContent` | `object` | 抽屉面板中的内容 |
| `IsOpen` | `bool` | 控制抽屉展开/收起（双向绑定） |
| `DrawerWidth` | `double` | 抽屉宽度，默认 300 |
| `Placement` | `Dock` | 抽屉弹出方向（Left/Right/Top/Bottom） |

---

### 4.17 Form / FormItem（表单）

语义化表单布局控件。`Form` 作为容器，`FormItem` 包裹每个表单项，`FormSeparator` 用于在表单中插入分割线。

```xml
<c:Form LabelWidth="100">
    <c:FormItem Label="用户名">
        <TextBox Text="{Binding Username}"/>
    </c:FormItem>
    <c:FormSeperater/>
    <c:FormItem Label="密码">
        <PasswordBox/>
    </c:FormItem>
</c:Form>
```

---

### 4.18 SideMenu（侧边菜单）

垂直侧边导航菜单控件，由 `SideMenuItem` 子项组成，点击菜单项触发 `SideMenuItemClickEvent` 路由事件。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `Header` | `object` | 菜单顶部内容区 |
| `Footer` | `object` | 菜单底部内容区 |
| `IsCollapsed` | `bool` | 是否收起为图标模式 |

---

### 4.19 FluidTabControl（流体标签页）

带流畅切换动画的 TabControl，子项 `FluidTabItem` 之间切换时有滑动/淡入动效。

---

### 4.20 IconBox（图标框）

统一图标展示控件，通过 `IconTemplateSelector` 根据图标类型自动选择渲染模板，支持字体图标、Path 图标和图片图标的统一接入。

---

## 5. 控件参考 — 布局面板

### 5.1 SpacingStackPanel（间距堆栈面板）

类似原生 `StackPanel`，但增加了元素间距和类 Grid Star 尺寸功能。通过附加属性 `SpacingStackPanel.Weight` 支持三种尺寸模式：

| Weight 值 | 行为 |
|---|---|
| `Auto`（默认） | 根据内容自动确定尺寸 |
| `100`（数字） | 使用指定的像素值 |
| `1*`、`2*` | 按比例分配剩余空间 |

| 属性名 | 类型 | 说明 |
|---|---|---|
| `Orientation` | `Orientation` | 布局方向：Horizontal / Vertical |
| `Spacing` | `double` | 子元素之间的间距 |
| `Weight`（附加属性） | `string` | 子元素权重，格式：Auto / 100 / 1* |

```xml
<c:SpacingStackPanel Orientation="Horizontal" Spacing="10">
    <Button Content="固定按钮"/>
    <TextBlock c:SpacingStackPanel.Weight="1*" Text="弹性文本区"/>
    <Button c:SpacingStackPanel.Weight="2*" Content="双倍弹性按钮"/>
</c:SpacingStackPanel>
```

---

### 5.2 SpacingUniformGrid（间距均匀网格）

类似 `UniformGrid` 的等宽等高网格布局，增加了行列间距配置。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `Rows` | `int` | 行数 |
| `Columns` | `int` | 列数 |
| `RowSpacing` | `double` | 行间距 |
| `ColumnSpacing` | `double` | 列间距 |

---

### 5.3 WaterfallPanel（瀑布流面板）

Pinterest 风格瀑布流布局面板，支持横向和纵向两种排列模式，自动将新元素放入最短列（行）。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `Orientation` | `Orientation` | 布局方向：Vertical / Horizontal |
| `Columns` | `int` | 纵向布局的列数 |
| `Rows` | `int` | 横向布局的行数 |
| `Spacing` | `double` | 元素间距 |
| `MinItemSize` | `double` | 次方向最小元素尺寸 |
| `MaxItemSize` | `double` | 次方向最大元素尺寸 |
| `FillLastItem` | `bool` | 是否拉伸最后一项填充剩余空间 |

```xml
<c:WaterfallPanel Orientation="Vertical" Columns="3" Spacing="10"
                 MinItemSize="80" MaxItemSize="200">
    <!-- 子元素 -->
</c:WaterfallPanel>
```

---

### 5.4 TilePanel（磁贴面板）

以固定磁贴尺寸排列子元素的面板，可配置磁贴宽高和间距。

---

### 5.5 FisheyePanel（鱼眼效果面板）

具有鱼眼放大效果的面板控件，鼠标悬停时中心元素放大、两侧元素依次缩小，营造 macOS Dock 栏式的交互体验。

---

### 5.6 CyclicPanel（循环面板）

将子元素排列为循环列表的布局面板，子元素超出边界后自动从另一侧循环显示，适合轮播型布局场景。

---

## 6. 控件参考 — 加载指示器

所有加载指示器继承自抽象基类 `LoadingIndicator`（实现 `ILoadingIndicator` 接口），共享 `IsActive` 属性控制动画启停。

> **通用属性（所有加载指示器）**
> - `IsActive : bool` — 是否激活动画，默认 `false`
> - `Content : object` — 指示器内容区（可叠加文字提示等）

| 控件类名 | 动画效果描述 |
|---|---|
| `LoadingRing` | 旋转圆环（AlertService 异步加载的默认效果） |
| `LoadingPulse` | 脉冲跳动点阵 |
| `LoadingChase` | 追逐旋转点 |
| `LoadingFlipCube` | 三维翻转立方块 |
| `LoadingParticle` | 粒子散射效果 |
| `LoadingTesseract` | 四维超正方体旋转 |
| `LoadingBox` | 加载遮罩框（`LoadingAdorner` 附加属性版） |

### LoadingRing 专有属性

| 属性名 | 类型 | 说明 |
|---|---|---|
| `RingSize` | `double` | 环的直径大小 |
| `RingThickness` | `double` | 环的线条粗细 |
| `RingColor` | `Brush` | 环的颜色 |

```xml
<c:LoadingRing IsActive="{Binding IsLoading}"
              RingSize="48"
              RingThickness="4"
              RingColor="#409EFF"/>
```

### LoadingAdorner（附加属性遮罩）

通过附加属性将加载遮罩叠加在任意容器上，无需修改控件结构：

```xml
<Grid h:LoadingAdorner.IsLoading="{Binding IsBusy}"
      h:LoadingAdorner.LoadingContent="{StaticResource MySpinner}">
    <!-- 实际内容 -->
</Grid>
```

---

## 7. 控件参考 — 高级窗口

### 7.1 AdvancedWindow

继承自 `System.Windows.Window`，提供自定义标题栏和窗口控制按钮（最小化、最大化、还原、关闭、置顶），支持通过 TemplateBinding 深度定制外观。

| 属性名 | 类型 | 说明 |
|---|---|---|
| `Icon` | `object` | 自定义标题栏图标（覆盖原生 Icon 属性） |
| `CaptionHeight` | `double` | 标题栏高度 |
| `CaptionBackground` | `Brush` | 标题栏背景色 |
| `TitleForeground` | `Brush` | 标题文字颜色 |

内置路由命令：`CloseCommand`、`MaximizeCommand`、`RestoreCommand`、`MinimizeCommand`、`TopmostCommand`。

```xml
<c:AdvancedWindow x:Class="MyApp.MainWindow"
    Title="My App" Width="1200" Height="800"
    CaptionBackground="#1B4F8A"
    TitleForeground="White">
    <!-- 窗口内容 -->
</c:AdvancedWindow>
```

---

## 8. 服务层 API

### 8.1 AlertService（模态弹框服务）

线程安全的单例模态弹框服务，支持 WPF 窗口和非 WPF 窗口（Win32 句柄）两种 Owner 模式，提供同步和异步显示方式，以及带遮罩层效果。

- **接口**：`IAlertService`, `IDisposable`
- **单例**：`AlertService.Instance`
- **线程安全**：内部使用 `Interlocked` + `Lazy<T>` 保证

#### 主要 API 方法

| 方法签名 | 说明 |
|---|---|
| `bool? Show(object, string)` | 显示模态弹框，返回用户操作结果 |
| `bool? Show(object, Func<bool>, string)` | 显示带同步验证回调的弹框 |
| `Task ShowAsync(object, Func<Task<bool>>, string)` | 显示带异步验证回调的弹框 |
| `Task ShowAsync<T>(object, Func<T,Task<bool>>, T, string)` | 带泛型参数的异步验证弹框 |
| `void SetOwner(Window)` | 设置 WPF 窗口为 Owner |
| `void SetOwner(IntPtr)` | 设置 Win32 句柄为 Owner |
| `void SetOwnerToForegroundWindow()` | 将当前前台窗口设为 Owner |
| `static void ResetInstance()` | 重置单例（Dispose 后重新使用） |

#### AlertServiceExtension 快捷方法

```csharp
var alert = AlertService.Instance;
alert.SetOwner(this); // this = 当前 Window

alert.Messgae("这是一条普通消息");    // 仅 Ok 按钮
alert.Information("这是信息提示");   // Ok + Cancel
alert.Success("操作成功！");          // Ok + Cancel
alert.Warning("请注意此操作");        // Ok + Cancel
alert.Error("发生了错误");            // Ok + Cancel
bool? r = alert.Question("确认删除？"); // Ok + Cancel
```

#### 异步验证示例

```csharp
// 显示带异步验证的弹框（例如提交前调用服务端 API）
await alert.ShowAsync(myFormView, async () =>
{
    var success = await _apiService.SubmitAsync(viewModel.Data);
    return success; // 返回 true 关闭弹框，false 阻止关闭
}, title: "提交确认");
```

#### AlertOption 配置

| 属性名 | 类型 | 说明 |
|---|---|---|
| `ButtonType` | `AlertButton` | 按钮组合：`Ok` / `OkCancel` |
| `Title` | `string` | 弹框默认标题，默认 `"Alert"` |
| `CaptionHeight` | `double` | 标题栏高度，默认 32 |
| `CaptionBackground` | `Brush` | 标题栏背景色 |
| `TitleForeground` | `Brush` | 标题文字颜色 |
| `OkButtonText` | `string` | "确定"按钮文字 |
| `CancelButtonText` | `string` | "取消"按钮文字 |
| `IsShowMask` | `bool` | 是否显示遮罩层，默认 true |
| `MaskBrush` | `Brush` | 遮罩层画刷，默认半透明黑色 |
| `IsShowLoadingOnAsync` | `bool` | 异步时是否显示加载动画，默认 true |
| `LoadingContent` | `object` | 异步加载动画内容（默认 LoadingRing） |
| `LoadingMaskBrush` | `Brush` | 加载遮罩画刷 |

---

### 8.2 NotificationService（非模态通知服务）

线程安全的单例通知气泡服务，支持多个通知同时显示，自动按位置排列，到期自动消失，支持手动关闭。

- **接口**：`INotificationService`, `IDisposable`
- **单例**：`NotificationService.Instance`

#### NotificationServiceExtension 快捷方法

```csharp
var notify = NotificationService.Instance;

notify.Message("这是普通消息");      // 默认样式
notify.Information("这是信息");      // 蓝色信息
notify.Success("操作成功！");         // 绿色成功
notify.Warning("请注意！");           // 黄色警告
notify.Error("操作失败");             // 红色错误
```

#### NotificationOption 配置

| 属性名 | 类型 | 说明 |
|---|---|---|
| `DisplayDuration` | `TimeSpan` | 显示持续时间，默认 2400ms |
| `Position` | `NotificationPosition` | 屏幕弹出位置，默认 `BottomRight` |
| `OffsetX / OffsetY` | `double` | 距屏幕边缘偏移，默认 5 |
| `Spacing` | `double` | 通知条之间的间距，默认 5 |
| `MaxCount` | `int` | 同时显示最大数量，默认 5 |
| `Width / Height` | `double` | 通知窗口尺寸，默认 240×75 |
| `IsShowCloseButton` | `bool` | 是否显示关闭按钮，默认 true |

#### NotificationPosition 枚举值

`TopLeft` · `TopCenter` · `TopRight` · `MiddleLeft` · `MiddleCenter` · `MiddleRight` · `BottomLeft` · `BottomCenter` · `BottomRight`

---

### 8.3 DialogService（对话框服务）

遵循 MVVM 模式的对话框服务接口 `IDialogService`，通过 `IDialogRequestClose` 接口在 ViewModel 中触发关闭，提供 `IDialogWindow` 接口规范对话窗口。

```csharp
// ViewModel 实现 IDialogRequestClose
public class MyDialogViewModel : IDialogRequestClose
{
    public event EventHandler<DialogRequestCloseEventArgs> CloseRequested;

    public void Confirm()
    {
        CloseRequested?.Invoke(this, new DialogRequestCloseEventArgs(true));
    }

    public void Cancel()
    {
        CloseRequested?.Invoke(this, new DialogRequestCloseEventArgs(false));
    }
}
```

---

## 9. 主题系统

### 9.1 内置主题

Cyclone.Amber 预置三套主题，均继承自抽象基类 `Theme`（`ResourceDictionary` 子类）：

| 主题类 | Name 属性 | 说明 |
|---|---|---|
| `BasicTheme` | `"Basic"`（默认） | 基础主题，蓝白配色，系统默认加载 |
| `LightTheme` | `"Light"` | 浅色主题，清爽明亮风格 |
| `DarkTheme` | `"Dark"` | 深色主题，深灰配色护眼风格 |

### 9.2 ThemeManager（主题管理器）

静态类，管理主题注册和切换，在 `Application.Resources.MergedDictionaries` 上动态追加/移除主题 `ResourceDictionary`。

| 成员 | 类型 | 说明 |
|---|---|---|
| `CurrentTheme` | `Theme` | 当前主题（set 时自动切换） |
| `AvailableThemes` | `IReadOnlyList<Theme>` | 所有已注册主题的只读列表 |
| `ThemeChanged` | `EventHandler` | 主题切换完成事件 |
| `RegisterTheme(Theme)` | 方法 | 注册新主题 |

### 9.3 自定义主题

```csharp
// 1. 继承 Theme 基类
public class MyCustomTheme : Theme
{
    public override string Name => "Custom";

    public MyCustomTheme()
    {
        MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(
                "pack://application:,,,/MyApp;component/Themes/MyThemeBrush.xaml")
        });
    }
}

// 2. 在 App.xaml.cs 中注册（OnStartup）
ThemeManager.RegisterTheme(new MyCustomTheme());

// 3. 切换到自定义主题
ThemeManager.CurrentTheme = ThemeManager.AvailableThemes
    .First(t => t.Name == "Custom");
```

> ⚠️ **注意**：自定义主题资源字典文件名须以 `ThemeBrush.xaml` 结尾。

### 9.4 资源键（ResourceKey）

Cyclone.Amber 提供 `BrushKey` 和 `StyleKey` 两个枚举类定义所有主题资源的键名，在自定义主题或复写样式时可安全引用：

```xml
<!-- 在 XAML 中引用主题画刷 -->
<Border Background="{DynamicResource {x:Static themes:BrushKey.PrimaryBrush}}"/>
```

---

## 10. 辅助工具类

| 类名 | 功能说明 |
|---|---|
| `TextBoxHelper` | 附加属性：`Watermark`（水印占位符）、`HasClearButton`（一键清除按钮） |
| `PasswordBoxHelper` | 附加属性：将 `PasswordBox.Password` 绑定到 ViewModel 字符串属性 |
| `RippleEffect` | 附加属性：`IsRippleEnabled`、`Color`、`Duration`，为控件添加水波纹点击动效 |
| `ComboBoxHelper` | 附加属性：`Watermark`（ComboBox 水印提示） |
| `DataGridHelper` | 附加属性：扩展 DataGrid 行高、交替行色、列头样式等 |
| `ListBoxHelper` | 附加属性：扩展 ListBox 项样式和间距 |
| `SliderHelper` | 附加属性：扩展 Slider 刻度和标签显示 |
| `ScrollViewerHelper` | 附加属性：修改 ScrollViewer 滚动条样式 |
| `ExpanderHelper` | 附加属性：扩展 Expander 动画和箭头方向 |
| `WindowHelper` | 附加属性：扩展 Window 标题栏和毛玻璃效果 |
| `NotificationObject` | `INotifyPropertyChanged` 基类实现，简化 ViewModel 属性通知 |
| `DataContextProxy` | XAML 绑定辅助，解决数据模板内 DataContext 访问问题 |
| `PropertyBridge` | 跨控件属性同步辅助类 |
| `VisualTreeHelperExtension` | VisualTree 扩展方法：`FindVisualChild<T>`、`FindVisualParent<T>` 等 |
| `TreeViewItemExtension` | TreeViewItem 扩展方法：展开/折叠操作辅助 |
| `DateHelper` | 日期计算辅助：周数、季度、日期范围等实用方法 |
| `EnumToItemsSourceExtension` | XAML 标记扩展：`{h:EnumToItemsSource EnumType={x:Type ...}}` |
| `EnumAttributeTypeConverter` | 枚举值 `Description`/`Display` 特性读取转换器 |

### 常用附加属性示例

```xml
<!-- TextBox 水印 + 清除按钮 -->
<TextBox h:TextBoxHelper.Watermark="请输入搜索词"
         h:TextBoxHelper.HasClearButton="True"/>

<!-- PasswordBox 双向绑定 -->
<PasswordBox h:PasswordBoxHelper.Password="{Binding Password, Mode=TwoWay}"/>

<!-- 水波纹效果 -->
<Button Content="点击我"
        h:RippleEffect.IsRippleEnabled="True"
        h:RippleEffect.Color="White"
        h:RippleEffect.Duration="0:0:0.4"/>

<!-- 枚举到 ItemsSource -->
<ComboBox ItemsSource="{h:EnumToItemsSource {x:Type local:Status}}"/>
```

---

## 11. 值转换器

Cyclone.Amber 提供一组静态转换器集合类，无需在资源字典中注册实例，直接以 `x:Static` 引用。底层基于泛型基类 `FuncValueConverter<TFrom, TTo>` 实现。

### BooleanConverter

| 转换器 | 转换逻辑 |
|---|---|
| `BooleanConverter.ToVisibility` | `bool → Visibility`（true=Visible, false=Collapsed） |
| `BooleanConverter.Inverse` | `bool → bool`（取反） |
| `BooleanConverter.StringEquality` | `(string, string) → bool`（字符串相等比较） |
| `BooleanConverter.StringNotEquality` | `(string, string) → bool`（字符串不等比较） |
| `BooleanConverter.NullToBoolean` | `object → bool`（null = true） |
| `BooleanConverter.NotNullToBoolean` | `object → bool`（非 null = true） |

### VisibilityConverter

| 转换器 | 转换逻辑 |
|---|---|
| `VisibilityConverter.VisibleWhenTrue` | `bool? → Visibility`（true=Visible, false=Collapsed, null=Hidden） |
| `VisibilityConverter.VisibleWhenFalse` | `bool? → Visibility`（取反逻辑） |
| `VisibilityConverter.VisibleWhenNullOrEmpty` | `string → Visibility`（空字符串 = Visible） |
| `VisibilityConverter.VisibleWhenNotNullOrEmpty` | `string → Visibility`（非空 = Visible） |

### 其他转换器

- **`BrushConverter`** — 颜色/画刷转换辅助（`Color ↔ Brush` 等）
- **`MathConverter`** — 数学运算转换器（加减乘除运算绑定）

### 使用示例

```xml
<!-- 引入命名空间 -->
xmlns:cv="clr-namespace:Cyclone.Wpf.Converters;assembly=Cyclone.Wpf"

<!-- 根据 bool 控制可见性 -->
<TextBlock Visibility="{Binding IsAdmin,
    Converter={x:Static cv:BooleanConverter.ToVisibility}}"/>

<!-- 按钮在加载时禁用 -->
<Button IsEnabled="{Binding IsLoading,
    Converter={x:Static cv:BooleanConverter.Inverse}}"/>

<!-- 字符串为空时显示占位控件 -->
<TextBlock Text="暂无数据"
           Visibility="{Binding Items.Count,
    Converter={x:Static cv:VisibilityConverter.VisibleWhenNullOrEmpty}}"/>
```

---

*Copyright © 2025 Cyclone · [https://github.com/yangf85/Amber](https://github.com/yangf85/Amber)*
