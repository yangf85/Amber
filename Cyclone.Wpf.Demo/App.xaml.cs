using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Themes;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace Cyclone.Wpf.Demo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. 调用 Initialize（必须！）
            ThemeManager.Initialize();

            // 2. 诊断 Output
            Debug.WriteLine("=== ThemeManager 诊断 ===");
            Debug.WriteLine($"已注册主题: {string.Join(", ", ThemeManager.AvailableThemes.Select(t => t.Name))}");
            Debug.WriteLine($"当前主题: {ThemeManager.CurrentTheme?.Name}");
            Debug.WriteLine($"App.Resources.MergedDictionaries 数量: {Resources.MergedDictionaries.Count}");
            foreach (var d in Resources.MergedDictionaries)
            {
                Debug.WriteLine($"  - {d.GetType().Name}  Source={d.Source}");
            }
        }
    }
}