using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// <see cref="Carousel"/> 的 item 容器——继承自 <see cref="ListBoxItem"/>。
/// </summary>
public class CarouselItem : ListBoxItem
{
    static CarouselItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselItem),
            new FrameworkPropertyMetadata(typeof(CarouselItem)));
    }
}