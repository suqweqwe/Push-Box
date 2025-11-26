using System.Windows;
using System.Windows.Controls;

namespace SokobanGame
{
    /// <summary>
    /// StartMenuPage.xaml 的交互逻辑
    /// </summary>
    public partial class StartMenuPage : Page
    {
        private MainWindow _mainWindow;

        public StartMenuPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow; // 保存对主窗口的引用，用于导航
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            // 导航到地图选择页面
            _mainWindow.MainFrame.Navigate(new MapSelectPage(_mainWindow));
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            // 关闭应用程序
            Application.Current.Shutdown();
        }
    }
}