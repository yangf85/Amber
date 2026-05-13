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

            // Text & Number
            "TextBox" => new TextBoxSample(),
            "NumberBox" => new NumberBoxSample(),
            "Input" => new PasswordBoxSample(),
            "HintBox" => new HintBoxSample(),
            "FilterBox" => new FilterBoxSample(),

            // Selection
            "ComboBox" => new ComboBoxSample(),
            "MultiComboBox" => new MultiComboBoxSample(),
            "CascadePicker" => new CascadePickerSample(),
            "EnumSelector" => new EnumSelectorSample(),
            "ColorPicker" => new ColorPickerSample(),
            "TransferBox" => new TransferBoxSample(),

            // Date & Time
            "Calendar" => new CalendarSample(),
            "DateRangePicker" => new DateRangePickerSample(),
            "TimePicker" => new TimePickerSample(),

            // Numeric
            "Slider" => new SliderSample(),
            "RangeSlider" => new RangeSliderSample(),
            "ProgressBar" => new ProgressBarSample(),
            "RotationEditor" => new RotationEditorSample(),
            "CircularGauge" => new CircularGaugeSample(),

            // Collections
            "ListBox" => new ListBoxSample(),
            "ListView" => new ListViewSample(),
            "DataGrid" => new DataGridSample(),
            "TreeView" => new TreeViewSample(),
            "Carousel" => new CarouselSample(),

            // Navigation
            "TabControl" => new TabControlView(),
            "FluidTab" => new FluidTabSample(),
            "Stepper" => new StepperSample(),
            "Breadcrumb" => new BreadcrumbBarSample(),
            "Pagination" => new PaginationSample(),

            // Menu
            "Menu" => new MenuSample(),

            // Containers
            "GroupBox" => new GroupBoxSample(),
            "Expander" => new ExpanderSample(),
            "Card" => new CardSample(),
            "SectionHeader" => new SectionHeaderSample(),
            "Form" => new FormSample(),
            "SettingItem" => new SettingItemSample(),
            "TransitionBox" => new TransitionBoxSample(),

            // Feedback
            "Notification" => new NotificationSample(),
            "Alert" => new AlertSample(),
            "LoadingBox" => new LoadingBoxSample(),
            "PopupBox" => new PopupBoxSample(),
            "Drawer" => new DrawerSample(),

            // Panels
            "CyclicPanel" => new CyclicPanelSample(),
            "SpacingUniformGrid" => new SpacingUniformGridSample(),
            "SpacingStackPanel" => new SpacingStackPanelSample(),
            "WaterfallPanel" => new WaterfallPanelSample(),
            "TilePanel" => new TilePanelSample(),

            // Display
            "HighlightTextBlock" => new HighlightTextBlockSample(),
            "IconBox" => new IconBoxSample(),

            // Test
            "Test1" => new TestView(),
            "Test2" => new TestView(),
            "Test3" => new TestView(),
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
            Header = "Button",
            Icon = "🔘",
            Items =
            [
                new SideMenuItemViewModel { Header = "Button",          Icon = "👆" },
                new SideMenuItemViewModel { Header = "CheckBox",        Icon = "✅" },
                new SideMenuItemViewModel { Header = "RadioButton",     Icon = "⚪" },
                new SideMenuItemViewModel { Header = "ToggleButton",    Icon = "⏯️" },
                new SideMenuItemViewModel { Header = "SwitchButton",    Icon = "🎚" },
                new SideMenuItemViewModel { Header = "SplitButton",     Icon = "🔀" },
                new SideMenuItemViewModel { Header = "HyperlinkButton", Icon = "🔗" },
            ]
        });

        // ② Text & Number
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Input",
            Icon = "✏️",
            Items =
            [
                new SideMenuItemViewModel { Header = "TextBox",   Icon = "📝" },
                new SideMenuItemViewModel { Header = "NumberBox", Icon = "🔢" },
                new SideMenuItemViewModel { Header = "PasswordBox",     Icon = "⌨️" },
                new SideMenuItemViewModel { Header = "HintBox",   Icon = "💡" },
                new SideMenuItemViewModel { Header = "FilterBox", Icon = "🔎" },
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
                new SideMenuItemViewModel { Header = "EnumSelector",  Icon = "🏷" },
                new SideMenuItemViewModel { Header = "ColorPicker",   Icon = "🎨" },
                new SideMenuItemViewModel { Header = "TransferBox",   Icon = "🔄" },
            ]
        });

        // ④ Date & Time
        Items.Add(new SideMenuItemViewModel
        {
            Header = "DateTime",
            Icon = "📅",
            Items =
            [
                new SideMenuItemViewModel { Header = "Calendar",        Icon = "📆" },
                new SideMenuItemViewModel { Header = "TimePicker",      Icon = "🕐" },
                new SideMenuItemViewModel { Header = "DateRangePicker", Icon = "📆" },
            ]
        });

        // ⑤ Numeric
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Range",
            Icon = "📊",
            Items =
            [
                new SideMenuItemViewModel { Header = "Slider",         Icon = "🎚" },
                new SideMenuItemViewModel { Header = "RangeSlider",    Icon = "📏" },
                new SideMenuItemViewModel { Header = "ProgressBar",    Icon = "⏳" },
                new SideMenuItemViewModel { Header = "RotationEditor", Icon = "🎲" },
                new SideMenuItemViewModel { Header = "CircularGauge",  Icon = "🎛" },
            ]
        });

        // ⑥ Collections
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Collection",
            Icon = "📚",
            Items =
            [
                new SideMenuItemViewModel { Header = "ListBox",  Icon = "📋" },
                new SideMenuItemViewModel { Header = "ListView", Icon = "📃" },
                new SideMenuItemViewModel { Header = "DataGrid", Icon = "🗂" },
                new SideMenuItemViewModel { Header = "Carousel", Icon = "🎠" },
                new SideMenuItemViewModel { Header = "Pagination", Icon = "📄" },
            ]
        });

        // ⑦ Navigation
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Navigation",
            Icon = "🧭",
            Items =
            [
                new SideMenuItemViewModel { Header = "TabControl", Icon = "🗒" },
                new SideMenuItemViewModel { Header = "FluidTab",   Icon = "💧" },
                new SideMenuItemViewModel { Header = "Stepper",    Icon = "👣" },
                new SideMenuItemViewModel { Header = "Breadcrumb", Icon = "🍞" },
                new SideMenuItemViewModel { Header = "Drawer",       Icon = "🗄" },
                 new SideMenuItemViewModel { Header = "PopupBox",     Icon = "💭" },

            ]
        });

        // ⑧ Menu
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Hierarchy",
            Icon = "🍔",
            Items =
            [
                new SideMenuItemViewModel { Header = "Menu", Icon = "📑" },
                new SideMenuItemViewModel { Header = "TreeView", Icon = "🌳" },
                new SideMenuItemViewModel { Header = "CascadePicker", Icon = "📂" },
            ]
        });

        // ⑨ Containers
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Container",
            Icon = "📦",
            Items =
            [
                new SideMenuItemViewModel { Header = "GroupBox",      Icon = "🗃" },
                new SideMenuItemViewModel { Header = "Expander",      Icon = "🔽" },
                new SideMenuItemViewModel { Header = "Card",          Icon = "🃏" },
                new SideMenuItemViewModel { Header = "Form",          Icon = "📋" },
                new SideMenuItemViewModel { Header = "SettingItem",   Icon = "⚙️" },
                new SideMenuItemViewModel { Header = "TransitionBox", Icon = "🎬" },
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

            ]
        });

        // ⑪ Panels
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Panel",
            Icon = "🧱",
            Items =
            [
                new SideMenuItemViewModel { Header = "CyclicPanel",        Icon = "🔁" },
                new SideMenuItemViewModel { Header = "SpacingUniformGrid", Icon = "🔲" },
                new SideMenuItemViewModel { Header = "SpacingStackPanel",  Icon = "🧮" },
                new SideMenuItemViewModel { Header = "WaterfallPanel",     Icon = "🌊" },
                new SideMenuItemViewModel { Header = "TilePanel",          Icon = "🀄" },
            ]
        });

        // ⑫ Display
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Display",
            Items =
            [
                new SideMenuItemViewModel { Header = "SectionHeader", Icon = "📌" },
                new SideMenuItemViewModel { Header = "HighlightTextBlock", Icon = "🖍" },
                new SideMenuItemViewModel { Header = "IconBox",            Icon = "🎯" },
            ]
        });

        // 顶层 Test 项(无子项)
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Test1",
            Icon = "🧪",
            Items =
            [
                new SideMenuItemViewModel
                {
                    Header = "Test2",
                    Icon = "🧪",
                    Items =
                    [
                        new SideMenuItemViewModel { Header = "Test3", Icon = "🧪" },
                        new SideMenuItemViewModel { Header = "Test4", Icon = "🧪" },
                    ]
                },

            ]
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