using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Demo.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
        // 处理传入的参数，统一为SideMenuItemViewModel
        if (item is not SideMenuItemViewModel menuItem) { return; }

        CurrentView = menuItem.Header switch
        {
            "Button" => new ButtonSample(),
            "SwitchButton" => new SwitchButtonSample(),
            "SplitButton" => new SplitButtonSample(),
            "NumberBox" => new NumberBoxSample(),
            "Input" => new InputSample(),
            "Menu" => new MenuSample(),
            "TreeView" => new TreeViewSample(),
            "CascadePicker" => new CascadePickerSample(),
            "LoadingBox" => new LoadingBoxSample(),
            "DataGrid" => new DataGridView(),
            "DateTime" => new DateSample(),
            "Range" => new RangeSample(),
            "RangeSlider" => new RangeSliderSample(),
            "TabControl" => new TabControlView(),
            "FluidTab" => new FluidTabSample(),
            "ComboBox" => new ComboBoxSample(),
            "ListView" => new ListViewView(),
            "ListBox" => new ListBoxView(),
            "Notification" => new NotificationSample(),
            "Alert" => new AlertSample(),
            "TransferBox" => new TransferBoxSample(),
            "HintBox" => new HintBoxSample(),
            "CyclicPanel" => new CyclicPanelSample(),
            "SpacingUniformGrid" => new SpacingUniformGridSample(),
            "SpacingStackPanel" => new SpacingStackPanelSample(),
            "FisheyePanel" => new FisheyePanelSample(),
            "WaterfallPanel" => new WaterfallPanelSample(),
            "TilePanel" => new TilePanelSample(),
            "TransitionBox" => new TransitionBoxSample(),
            "Form" => new FormSample(),
            "Slider" => new SliderSample(),
            "ProgressBar" => new ProgressBarSample(),
            "Expander" => new ExpanderSample(),
            "Carousel" => new CarouselSample(),
            "Drawer" => new DrawerSample(),
            "FilterBox" => new FilterBoxSample(),
            "IconBox" => new IconBoxSample(),
            "Stepper" => new StepperSample(),
            "Breadcrumb" => new BreadcrumbBarSample(),
            "ColorPicker" => new ColorPickerSample(),
            "EnumSelector" => new EnumSelectorSample(),
            "SectionHeader" => new SectionHeaderSample(),
            "SettingItem" => new SettingItemSample(),
            "Card" => new CardSample(),
            "MultiComboBox" => new MultiComboBoxSample(),
            "PopupBox" => new PopupBoxSample(),
            "HighlightTextBlock" => new HighlightTextBlockSample(),
            "HyperlinkButton" => new HyperlinkButtonSample(),
            "GroupBox" => new GroupBoxSample(),
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
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Interaction",
            Icon = "\xe60f",
            Items =
            [
                new SideMenuItemViewModel
                {
                    Header="Button",
                    Icon= "\xe605",
                },
                 new SideMenuItemViewModel
                {
                    Header="SwitchButton",
                    Icon= "\xe605",
                },
                 new SideMenuItemViewModel
                {
                    Header="HyperlinkButton",
                    Icon= "\xe605",
                },
                   new SideMenuItemViewModel
                {
                    Header="SplitButton",
                    Icon= "\xe605",
                },
                new SideMenuItemViewModel
                {
                    Header="Input",
                    Icon= "\xe603",
                },
                new SideMenuItemViewModel
                {
                    Header="HighlightTextBlock",
                    Icon= "\xe605",
                },
                 new SideMenuItemViewModel
                {
                    Header="NumberBox",
                    Icon= "\xe603",
                },
                new SideMenuItemViewModel
                {
                    Header="ProgressBar",
                    Icon= "\xe605",
                },
                 new SideMenuItemViewModel
                {
                    Header="Slider",
                    Icon= "\xe605",
                },

                new SideMenuItemViewModel
                {
                    Header="RangeSlider",
                    Icon= "\xe605",
                },
                 new SideMenuItemViewModel
                {
                    Header="ComboBox",
                    Icon= "\xe665",
                },
                 new SideMenuItemViewModel
                {
                    Header="MultiComboBox",
                    Icon= "\xe665",
                },

                new SideMenuItemViewModel
                {
                    Header="HintBox",
                    Icon= "\xe606",
                },
                new SideMenuItemViewModel
                {
                    Header="DateTime",
                    Icon= "\xe604",
                },

                  new SideMenuItemViewModel
                {
                    Header="EnumSelector",
                    Icon= "\xe71a",
                },
                new SideMenuItemViewModel
                {
                    Header="FilterBox",
                    Icon= "\xe74c",
                },
                new SideMenuItemViewModel
                {
                    Header="Form",
                    Icon= "\xe60b",
                },
                new SideMenuItemViewModel
                {
                    Header="SectionHeader",
                    Icon= "\xe888",
                },
                new SideMenuItemViewModel
                {
                    Header="SettingItem",
                    Icon= "\xe888",
                },
                new SideMenuItemViewModel
                {
                    Header="ColorPicker",
                    Icon= "\xe8fb",
                },

            ]
        });
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Collection",
            Icon = "\xe6d5",
            Items =
            [

                new SideMenuItemViewModel
                {
                    Header="ListBox",
                    Icon= "\xe627",
                },
                new SideMenuItemViewModel
                {
                    Header="ListView",
                    Icon= "\xe6a6",
                },
                new SideMenuItemViewModel
                {
                    Header="DataGrid",
                    Icon= "\xe751",
                },
                new SideMenuItemViewModel
                {
                    Header="Carousel",
                    Icon= "\xe6d2",
                },

            ]
        });
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Navigation",
            Icon = "\xe81a",
            Items =
            [
                new SideMenuItemViewModel
                {
                    Header="TabControl",
                    Icon= "\xe602",
                },
                 new SideMenuItemViewModel
                {
                    Header="FluidTab",
                    Icon= "\xe609",
                },
                new SideMenuItemViewModel
                {
                    Header="TransitionBox",
                    Icon= "\xe64a",
                },
                new SideMenuItemViewModel
                {
                    Header="TransferBox",
                    Icon= "\xe642",
                },
                new SideMenuItemViewModel
                {
                    Header="Expander",
                    Icon= "\xe6dd",
                },
                  new SideMenuItemViewModel
                {
                    Header="Card",
                    Icon= "\xe6dd",
                },
                new SideMenuItemViewModel
                {
                    Header="Drawer",
                    Icon= "\xe650",
                },
                 new SideMenuItemViewModel
                {
                    Header="Stepper",
                    Icon= "\xe756",
                },
                new SideMenuItemViewModel
                {
                    Header="Breadcrumb",
                    Icon= "\xe8d4",
                },
                 new SideMenuItemViewModel
                {
                    Header="PopupBox",
                    Icon= "\xe8d4",
                },
                 new SideMenuItemViewModel
                {
                    Header="GroupBox",
                    Icon= "\xe8d4",
                },

            ]
        });

        Items.Add(new SideMenuItemViewModel
        {
            Header = "Hierarchy",
            Icon = "\xe817",
            Items =
            [
                new SideMenuItemViewModel
                {
                    Header="Menu",
                    Icon= "\xe633",
                },
                new SideMenuItemViewModel
                {
                    Header="TreeView",
                    Icon= "\xe970",
                },
                new SideMenuItemViewModel
                {
                    Header="CascadePicker",
                    Icon= "\xe78a",
                },
            ]
        });

        Items.Add(new SideMenuItemViewModel
        {
            Header = "Message",
            Icon = "\xe60e",
            Items =
            [
                 new SideMenuItemViewModel
                {
                    Header="Notification",
                    Icon= "\xe60e",
                },
                new SideMenuItemViewModel
                {
                    Header="Alert",
                    Icon= "\xe60e",
                },

            ]
        });
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Panel",
            Icon = "\xe614",
            Items =
            [
                new SideMenuItemViewModel
                {
                    Header="CyclicPanel",
                    Icon= "\xe863",
                },
                new SideMenuItemViewModel
                {
                    Header="SpacingUniformGrid",
                    Icon= "\xe608",
                },
                new SideMenuItemViewModel
                {
                    Header="SpacingStackPanel",
                    Icon= "\xe668",
                },
                 new SideMenuItemViewModel
                {
                    Header="FisheyePanel",
                    Icon= "\xe60a",
                },
                 new SideMenuItemViewModel
                {
                    Header="WaterfallPanel",
                    Icon= "\xe607",
                },
                 new SideMenuItemViewModel
                {
                    Header="TilePanel",
                    Icon= "\xe62b",
                },

            ]
        });
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Other",
            Icon = "\xe61a",
            Items =
            [
                new SideMenuItemViewModel
                {
                    Header = "LoadingBox",
                    Icon = "\xe891",
                },

               new SideMenuItemViewModel
               {
                   Header="IconBox",
                   Icon = "\xe617",
               }
            ]
        });
        Items.Add(new SideMenuItemViewModel
        {
            Header = "Test",
            Icon = "\xe629",
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