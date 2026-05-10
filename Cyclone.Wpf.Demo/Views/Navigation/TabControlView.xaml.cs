using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views
{
    public partial class TabControlView : UserControl
    {
        public TabControlView()
        {
            InitializeComponent();
            DataContext = new TabControlViewModel();
        }
    }

    public partial class TabControlViewModel : ObservableObject
    {
        private int _newTabSeed = 1;

        // ===== 章节④ MVVM 数据绑定 + ⑤ 动态增删 共享 =====
        public ObservableCollection<DocumentTab> Documents { get; }

        [ObservableProperty]
        public partial DocumentTab? SelectedDocument { get; set; }

        public TabControlViewModel()
        {
            Documents = new ObservableCollection<DocumentTab>
            {
                new DocumentTab { Title = "README.md", Icon = "📄", Body = "项目说明文档——介绍 Cyclone.Wpf 控件库的核心特性和使用方式。", Modified = false },
                new DocumentTab { Title = "Program.cs", Icon = "💻", Body = "应用程序入口——StartupUri 指向 MainWindow.xaml。", Modified = true },
                new DocumentTab { Title = "App.config", Icon = "⚙️", Body = "应用配置文件——主题切换、语言设置等。", Modified = false },
            };

            SelectedDocument = Documents.Count > 0 ? Documents[0] : null;
        }

        [RelayCommand]
        private void AddDocument()
        {
            _newTabSeed++;
            var doc = new DocumentTab
            {
                Title = $"Untitled{_newTabSeed}.txt",
                Icon = "📝",
                Body = $"这是第 {_newTabSeed} 份新建文档——开始编辑吧。",
                Modified = true,
            };
            Documents.Add(doc);
            SelectedDocument = doc;
        }

        [RelayCommand]
        private void CloseDocument(DocumentTab doc)
        {
            if (doc == null)
            {
                return;
            }

            // 删除前找好下一个要选中的——避免选中态丢失后 ContentTemplate 闪空白
            var index = Documents.IndexOf(doc);
            Documents.Remove(doc);

            if (Documents.Count == 0)
            {
                SelectedDocument = null;
            }
            else if (SelectedDocument == doc)
            {
                // 优先选下一项；如果删的是最后一项就选前一项
                var nextIndex = index < Documents.Count ? index : Documents.Count - 1;
                SelectedDocument = Documents[nextIndex];
            }
        }
    }

    /// <summary>
    /// 文档标签页模型——演示 ItemsSource 数据绑定 + Closable tab。
    /// </summary>
    public partial class DocumentTab : ObservableObject
    {
        [ObservableProperty]
        public partial string Title { get; set; }

        [ObservableProperty]
        public partial string Icon { get; set; }

        [ObservableProperty]
        public partial string Body { get; set; }

        /// <summary>是否有未保存的修改——header 上显示小圆点提示。</summary>
        [ObservableProperty]
        public partial bool Modified { get; set; }
    }
}
