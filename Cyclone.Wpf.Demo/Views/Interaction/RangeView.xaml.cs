using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// RangeView.xaml 的交互逻辑
    /// </summary>
    public partial class RangeView : UserControl
    {
        public RangeView()
        {
            InitializeComponent();
            DataContext = new RangeViewModel();
        }
    }

    public partial class RangeViewModel : ObservableObject
    {
        private readonly DispatcherTimer _timer;

        private readonly Random _random;

        private void InitializeValues()
        {
            // Slider 相关属性初始值
            VolumeLevel = 75;
            BassLevel = 0;
            Brightness = 80;
            Contrast = 100;
            Saturation = 100;

            // ProgressBar 相关属性初始值
            DownloadProgress = 45;
            UploadProgress = 20;
            CpuUsage = 25;
            MemoryUsage = 60;
            DiskUsage = 35;

            // RangeSlider 相关属性初始值
            MinPrice = 1000;
            MaxPrice = 5000;
            StartTime = 9.0;
            EndTime = 18.0;

            // CircularGauge 相关属性初始值
            CurrentSpeed = 60;
            Temperature = 25.5;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // 模拟进度条数据更新
            UpdateProgressBars();

            // 模拟系统监控数据更新
            UpdateSystemMonitoring();
        }

        private void UpdateProgressBars()
        {
            // 更新下载进度
            if (DownloadProgress < 100)
            {
                DownloadProgress = Math.Min(100, DownloadProgress + _random.Next(1, 8));
            }
            else
            {
                DownloadProgress = 0; // 重置进度
            }

            // 更新上传进度
            if (UploadProgress < 100)
            {
                UploadProgress = Math.Min(100, UploadProgress + _random.Next(1, 5));
            }
            else
            {
                UploadProgress = 0; // 重置进度
            }
        }

        private void UpdateSystemMonitoring()
        {
            // 模拟系统资源使用率波动
            CpuUsage = Math.Max(0, Math.Min(100, CpuUsage + _random.Next(-5, 6)));
            MemoryUsage = Math.Max(0, Math.Min(100, MemoryUsage + _random.Next(-3, 4)));
            DiskUsage = Math.Max(0, Math.Min(100, DiskUsage + _random.Next(-2, 3)));
        }

        public RangeViewModel()
        {
            _random = new Random();

            // 初始化定时器用于模拟数据更新
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // 设置初始值
            InitializeValues();
        }

        #region Slider 相关属性

        [ObservableProperty]
        public partial double VolumeLevel { get; set; }

        [ObservableProperty]
        public partial double BassLevel { get; set; }

        [ObservableProperty]
        public partial double Brightness { get; set; }

        [ObservableProperty]
        public partial double Contrast { get; set; }

        [ObservableProperty]
        public partial double Saturation { get; set; }

        #endregion Slider 相关属性

        #region ProgressBar 相关属性

        [ObservableProperty]
        public partial double DownloadProgress { get; set; }

        [ObservableProperty]
        public partial double UploadProgress { get; set; }

        [ObservableProperty]
        public partial double CpuUsage { get; set; }

        [ObservableProperty]
        public partial double MemoryUsage { get; set; }

        [ObservableProperty]
        public partial double DiskUsage { get; set; }

        #endregion ProgressBar 相关属性

        #region RangeSlider 相关属性

        [ObservableProperty]
        public partial double MinPrice { get; set; }

        [ObservableProperty]
        public partial double MaxPrice { get; set; }

        [ObservableProperty]
        public partial double StartTime { get; set; }

        [ObservableProperty]
        public partial double EndTime { get; set; }

        #endregion RangeSlider 相关属性

        #region CircularGauge 相关属性

        [ObservableProperty]
        public partial double CurrentSpeed { get; set; }

        [ObservableProperty]
        public partial double Temperature { get; set; }

        #endregion CircularGauge 相关属性

        #region 命令

        [RelayCommand]
        void RandomSpeed()
        {
            CurrentSpeed = _random.Next(0, 241);
            NotificationService.Instance.Information($"车速已设置为: {CurrentSpeed:0} km/h");
        }

        [RelayCommand]
        private void RefreshTemperature()
        {
            Temperature = _random.Next(-20, 51) + (_random.NextDouble() * 2 - 1);
            NotificationService.Instance.Information($"当前温度: {Temperature:0.1}°C");
        }

        [RelayCommand]
        private void ShowVolumeSettings()
        {
            var message = $"音量设置:\n主音量: {VolumeLevel:0}%\n低音: {BassLevel:0}dB";
            NotificationService.Instance.Information(message);
        }

        [RelayCommand]
        private void ShowImageSettings()
        {
            var message = $"图像设置:\n亮度: {Brightness:0}\n对比度: {Contrast:0}\n饱和度: {Saturation:0}";
            NotificationService.Instance.Information(message);
        }

        [RelayCommand]
        private void ShowPriceRange()
        {
            var message = $"价格区间: ¥{MinPrice:0} - ¥{MaxPrice:0}";
            NotificationService.Instance.Information(message);
        }

        [RelayCommand]
        void ShowTimeRange()
        {
            var message = $"工作时间: {StartTime:0.0}时 - {EndTime:0.0}时";
            NotificationService.Instance.Information(message);
        }

        [RelayCommand]
        private void ShowSystemStatus()
        {
            var message = $"系统状态:\nCPU: {CpuUsage:0}%\n内存: {MemoryUsage:0}%\n磁盘: {DiskUsage:0}%";
            NotificationService.Instance.Information(message);
        }

        #endregion 命令
    }
}