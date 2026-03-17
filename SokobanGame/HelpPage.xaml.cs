using System.Windows;
using System.Windows.Controls;

namespace SokobanGame
{
    /// <summary>
    /// HelpPage.xaml 的交互逻辑
    /// </summary>
    public partial class HelpPage : Page
    {
        private MainWindow _mainWindow;

        public HelpPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // 页面加载时的初始化
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // 返回开始菜单
            _mainWindow.MainFrame.Navigate(new StartMenuPage(_mainWindow));
        }
    }
}

