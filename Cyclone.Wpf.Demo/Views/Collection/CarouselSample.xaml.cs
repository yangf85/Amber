using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

public partial class CarouselSample : UserControl
{
    public CarouselSample()
    {
        InitializeComponent();
        DataContext = new CarouselViewModel();
    }
}

public partial class CarouselViewModel : ObservableValidator
{
    public ObservableCollection<ImageViewModel> BasicSample { get; }

    public ObservableCollection<ImageViewModel> AutoPlaySample { get; }

    public ObservableCollection<ImageViewModel> WrapSample { get; }

    public ObservableCollection<ImageViewModel> ShowcaseSample { get; }

    private static ObservableCollection<ImageViewModel> CreateImages()
    {
        return new ObservableCollection<ImageViewModel>
        {
            new ImageViewModel
            {
                MainTitle = "Golden Horizon",
                SubTitle = "A Serene Evening Painted in Shades of Gold",
                ImagePath = "/Assets/carousel1.jpeg",
            },
            new ImageViewModel
            {
                MainTitle = "Reflections of Tranquility",
                SubTitle = "Nature's Mirror Reflecting the Beauty of the Surroundings",
                ImagePath = "/Assets/carousel2.jpeg",
            },
            new ImageViewModel
            {
                MainTitle = "Majestic Peaks",
                SubTitle = "Touching the Sky with Their Towering Presence and Rugged Beauty",
                ImagePath = "/Assets/carousel3.jpeg",
            },
            new ImageViewModel
            {
                MainTitle = "Winter Wonderland",
                SubTitle = "A Blanket of Serenity Transforming the Landscape into a Magical Realm",
                ImagePath = "/Assets/carousel4.jpeg",
            },
        };
    }

    public CarouselViewModel()
    {
        // 每个 sample 用独立的 ObservableCollection 实例——避免 WPF Selector 通过共享集合的
        // default ICollectionView.CurrentItem 自动同步 selection（IsSynchronizedWithCurrentItem 默认 auto）
        BasicSample = CreateImages();
        AutoPlaySample = CreateImages();
        WrapSample = CreateImages();
        ShowcaseSample = CreateImages();
    }
}

public partial class ImageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string MainTitle { get; set; }

    [ObservableProperty]
    public partial string SubTitle { get; set; }

    [ObservableProperty]
    public partial string ImagePath { get; set; }
}