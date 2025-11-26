using System.Windows;
using System.Windows.Controls;

namespace SokobanGame
{
    /// <summary>
    /// MapSelectPage.xaml 的交互逻辑
    /// </summary>
    public partial class MapSelectPage : Page
    {
        private MainWindow _mainWindow;

        public MapSelectPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            Loaded += MapSelectPage_Loaded; // 页面加载完成后执行
        }

        private void MapSelectPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 动态生成地图按钮
            for (int i = 0; i < MapManager.MapNames.Count; i++)
            {
                int mapIndex = i; // 重要：在循环中捕获变量需要一个临时变量
                Button btn = new Button();
                btn.Content = MapManager.MapNames[i];
                btn.FontSize = 20;
                btn.Margin = new Thickness(10);
                btn.Padding = new Thickness(20, 10, 20, 10);
                btn.Click += (s, args) => StartMap_Click(mapIndex);
                MapButtonsPanel.Children.Add(btn);
            }
        }

        private void StartMap_Click(int mapIndex)
        {
            // 导航到游戏页面，并传递选择的地图索引
            _mainWindow.MainFrame.Navigate(new GamePage(_mainWindow, mapIndex));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // 返回开始菜单
            _mainWindow.MainFrame.Navigate(new StartMenuPage(_mainWindow));
        }
    }
}