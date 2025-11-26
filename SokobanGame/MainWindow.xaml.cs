using System.Windows;

namespace SokobanGame
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // 在窗口加载完成后，导航到开始菜单页面
            MainFrame.Navigated += MainFrame_Navigated;
            MainFrame.Navigate(new StartMenuPage(this));
        }

        // 这个事件处理程序是为了防止导航历史累积，每次导航都是一个全新的开始
        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            MainFrame.Navigated -= MainFrame_Navigated;
            MainFrame.NavigationService.RemoveBackEntry();
        }
    }
}