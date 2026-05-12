using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Demo.Views;
using System.Collections.ObjectModel;

namespace Cyclone.Wpf.Demo.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial object? CurrentView { get; set; } = new object();

    [ObservableProperty]
    public partial SideMenuViewModel SideMenu { get; set; } = new SideMenuViewModel();

    [RelayCommand]
    private void SwitchView(object item)
    {
        if (item is not SideMenuItemViewModel menuItem) { return; }

        CurrentView = menuItem.Header switch
        {
            // Buttons
            "Button" => new ButtonSample(),
            "CheckBox" => new CheckBoxSample(),
            "RadioButton" => new RadioButtonSample(),
            "ToggleButton" => new ToggleButtonSample(),
            "SwitchButton" => new SwitchButtonSample(),
            "SplitButton" => new SplitButtonSample(),
            "HyperlinkButton" => new HyperlinkButtonSample(),

            // Input
            "TextBox" => new TextBoxSample(),
            "NumberBox" => new NumberBoxSample(),
            "Input" => new InputSample(),
            "HintBox" => new HintBoxSample(),
            "HighlightTextBlock" => new HighlightTextBlockSample(),
            "FilterBox" => new FilterBoxSample(),

            // Selection
            "ComboBox" => new ComboBoxSample(),
            "MultiComboBox" => new MultiComboBoxSample(),
            "CascadePicker" => new CascadePickerSample(),
            "EnumSelector" => new EnumSelectorSample(),
            "ColorPicker" => new ColorPickerSample(),
            "TransferBox" => new TransferBoxSample(),

            // DateTime
            "DateTime" => new DateSample(),
            "DateRangePicker" => new DateRangePickerSample(),
            "TimePicker" => new TimePickerSample(),

            // Numeric
            "Slider" => new SliderSample(),
            "RangeSlider" => new RangeSliderSample(),
            "ProgressBar" => new ProgressBarSample(),
            "RotationEditor" => new RotationEditorSample(),
            "CircularGaugeSample" => new CircularGaugeSample(),

            // Collections
            "ListBox" => new ListBoxSample(),
            "ListView" => new ListViewSample(),
            "DataGrid" => new DataGridSample(),
            "TreeView" => new TreeViewSample(),
            "Carousel" => new CarouselSample(),
            "Pagination" => new PaginationSample(),

            // Navigation
            "TabControl" => new TabControlView(),
            "FluidTab" => new FluidTabSample(),
            "Stepper" => new StepperSample(),
            "Breadcrumb" => new BreadcrumbBarSample(),

            // Menus
            "Menu" => new MenuSample(),

            // Containers
            "GroupBox" => new GroupBoxSample(),
            "Expander" => new ExpanderSample(),
            "Card" => new CardSample(),
            "SectionHeader" => new SectionHeaderSample(),
            "Form" => new FormSample(),
            "SettingItem" => new SettingItemSample(),
            "TransitionBox" => new TransitionBoxSample(),
            "Drawer" => new DrawerSample(),

            // Feedback
            "Notification" => new NotificationSample(),
            "Alert" => new AlertSample(),
            "LoadingBox" => new LoadingBoxSample(),
            "PopupBox" => new PopupBoxSample(),

            // Panels
            "CyclicPanel" => new CyclicPanelSample(),
            "SpacingUniformGrid" => new SpacingUniformGridSample(),
            "SpacingStackPanel" => new SpacingStackPanelSample(),
            "FisheyePanel" => new FisheyePanelSample(),
            "WaterfallPanel" => new WaterfallPanelSample(),
            "TilePanel" => new TilePanelSample(),

            // Other
            "IconBox" => new IconBoxSample(),
            "Test" => new TestView(),
            _ => null,
        };
    }

    public MainViewModel()
    {
        CurrentView = new ButtonSample();
    }
}

public partial class SideMenuViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<SideMenuItemViewModel> Items { get; set; } = [];

    public SideMenuViewModel()
    {
        // ① Buttons
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Buttons",
            Icon = "🔘",
            Items =
            [
                new SideMenuItemViewModel { Header = "Button",          Icon = "👆" },
                new SideMenuItemViewModel { Header = "CheckBox",          Icon = "✅" },
                new SideMenuItemViewModel { Header = "RadioButton",          Icon = "⚪" },
                new SideMenuItemViewModel { Header = "ToggleButton",          Icon = "⏯️" },
                new SideMenuItemViewModel { Header = "SwitchButton",    Icon = "🎚" },
                new SideMenuItemViewModel { Header = "SplitButton",     Icon = "🔀" },
                new SideMenuItemViewModel { Header = "HyperlinkButton", Icon = "🔗" },
            ]
        });

        // ② Input
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Input",
            Icon = "✏️",
            Items =
            [
                new SideMenuItemViewModel { Header = "TextBox",            Icon = "📝" },
                new SideMenuItemViewModel { Header = "NumberBox",          Icon = "🔢" },
                new SideMenuItemViewModel { Header = "Input",              Icon = "⌨️" },
                new SideMenuItemViewModel { Header = "HintBox",            Icon = "💡" },
                new SideMenuItemViewModel { Header = "HighlightTextBlock", Icon = "🖍" },
                new SideMenuItemViewModel { Header = "FilterBox",          Icon = "🔎" },
            ]
        });

        // ③ Selection
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Selection",
            Icon = "☑️",
            Items =
            [
                new SideMenuItemViewModel { Header = "ComboBox",      Icon = "⬇️" },
                new SideMenuItemViewModel { Header = "MultiComboBox", Icon = "📋" },
                new SideMenuItemViewModel { Header = "CascadePicker", Icon = "📂" },
                new SideMenuItemViewModel { Header = "EnumSelector",  Icon = "🏷" },
                new SideMenuItemViewModel { Header = "ColorPicker",   Icon = "🎨" },
                new SideMenuItemViewModel { Header = "TransferBox",   Icon = "🔄" },
            ]
        });

        // ④ DateTime
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Date & Time",
            Icon = "📅",
            Items =
            [
                new SideMenuItemViewModel { Header = "DateTime", Icon = "🕐" },
                new SideMenuItemViewModel { Header = "DateRangePicker", Icon = "📆" },
                new SideMenuItemViewModel { Header = "TimePicker", Icon = "🕐" },
            ]
        });

        // ⑤ Numeric / Progress
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Numeric",
            Icon = "📊",
            Items =
            [
                new SideMenuItemViewModel { Header = "Slider",      Icon = "🎚" },
                new SideMenuItemViewModel { Header = "RangeSlider", Icon = "📏" },
                new SideMenuItemViewModel { Header = "Range",       Icon = "📐" },
                new SideMenuItemViewModel { Header = "ProgressBar", Icon = "⏳" },
                new SideMenuItemViewModel { Header = "RotationEditor", Icon = "🎲" },
                new SideMenuItemViewModel { Header = "CircularGaugeSample", Icon = "🎲" },
            ]
        });

        // ⑥ Collections
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Collections",
            Icon = "📚",
            Items =
            [
                new SideMenuItemViewModel { Header = "ListBox",  Icon = "📋" },
                new SideMenuItemViewModel { Header = "ListView", Icon = "📃" },
                new SideMenuItemViewModel { Header = "DataGrid", Icon = "🗂" },
                new SideMenuItemViewModel { Header = "TreeView", Icon = "🌳" },
                new SideMenuItemViewModel { Header = "Carousel", Icon = "🎠" },
                new SideMenuItemViewModel { Header = "Pagination", Icon = "🗂" },
            ]
        });

        // ⑦ Navigation
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Navigation",
            Icon = "🧭",
            Items =
            [
                new SideMenuItemViewModel { Header = "TabControl",  Icon = "🗒" },
                new SideMenuItemViewModel { Header = "FluidTab",    Icon = "💧" },
                new SideMenuItemViewModel { Header = "Stepper",     Icon = "👣" },
                new SideMenuItemViewModel { Header = "Breadcrumb",  Icon = "🍞" },
            ]
        });

        // ⑧ Menu
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Menu",
            Icon = "🍔",
            Items =
            [
                new SideMenuItemViewModel { Header = "Menu", Icon = "📑" },
            ]
        });

        // ⑨ Containers
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Containers",
            Icon = "📦",
            Items =
            [
                new SideMenuItemViewModel { Header = "GroupBox",       Icon = "🗃" },
                new SideMenuItemViewModel { Header = "Expander",       Icon = "🔽" },
                new SideMenuItemViewModel { Header = "Card",           Icon = "🃏" },
                new SideMenuItemViewModel { Header = "Form",           Icon = "📋" },
                new SideMenuItemViewModel { Header = "SettingItem",    Icon = "⚙️" },
                new SideMenuItemViewModel { Header = "TransitionBox",  Icon = "🎬" },
                new SideMenuItemViewModel { Header = "Drawer",         Icon = "🗄" },
            ]
        });

        // ⑩ Feedback
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Feedback",
            Icon = "💬",
            Items =
            [
                new SideMenuItemViewModel { Header = "Notification", Icon = "🔔" },
                new SideMenuItemViewModel { Header = "Alert",        Icon = "⚠️" },
                new SideMenuItemViewModel { Header = "LoadingBox",   Icon = "⏱" },
                new SideMenuItemViewModel { Header = "PopupBox",     Icon = "💭" },
            ]
        });

        // ⑪ Panels
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Panels",
            Icon = "🧱",
            Items =
            [
                new SideMenuItemViewModel { Header = "CyclicPanel",        Icon = "🔁" },
                new SideMenuItemViewModel { Header = "SpacingUniformGrid", Icon = "🔲" },
                new SideMenuItemViewModel { Header = "SpacingStackPanel",  Icon = "🧮" },
                new SideMenuItemViewModel { Header = "FisheyePanel",       Icon = "🐟" },
                new SideMenuItemViewModel { Header = "WaterfallPanel",     Icon = "🌊" },
                new SideMenuItemViewModel { Header = "TilePanel",          Icon = "🀄" },
            ]
        });

        // ⑫ Other
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Other",
            Icon = "✨",
            Items =
            [
                new SideMenuItemViewModel { Header = "IconBox", Icon = "🎯" },
                new SideMenuItemViewModel { Header = "SectionHeader",  Icon = "📌" },
            ]
        });

        // 顶层 Test 项(无子项)
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Test",
            Icon = "🧪",
        });
    }
}

public partial class SideMenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Header { get; set; }

    [ObservableProperty]
    public partial string Icon { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<SideMenuItemViewModel> Items { get; set; } = [];
}