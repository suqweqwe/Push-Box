using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SokobanGame
{
    public partial class MapSelectPage : Page
    {
        private MainWindow _mainWindow;
        private Random _random = new Random();

        public MapSelectPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            InitializeMapButtons();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CreateStars();
        }

        // 创建星空背景
        private void CreateStars()
        {
            if (StarsCanvas.ActualWidth == 0 || StarsCanvas.ActualHeight == 0)
                return;

            for (int i = 0; i < 100; i++)
            {
                var star = new Ellipse
                {
                    Width = _random.Next(1, 3),
                    Height = _random.Next(1, 3),
                    Fill = new SolidColorBrush(Color.FromArgb(
                        (byte)_random.Next(100, 255),
                        (byte)_random.Next(200, 255),
                        (byte)_random.Next(200, 255),
                        (byte)_random.Next(255))),
                    Opacity = _random.NextDouble() * 0.7 + 0.3
                };

                Canvas.SetLeft(star, _random.Next(0, (int)StarsCanvas.ActualWidth));
                Canvas.SetTop(star, _random.Next(0, (int)StarsCanvas.ActualHeight));
                StarsCanvas.Children.Add(star);

                // 星星闪烁动画
                var anim = new DoubleAnimation
                {
                    From = star.Opacity,
                    To = star.Opacity + (_random.NextDouble() * 0.4 - 0.2),
                    Duration = TimeSpan.FromSeconds(_random.Next(2, 6)),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                star.BeginAnimation(OpacityProperty, anim);
            }
        }

        // 初始化地图按钮时，修改按钮样式
        private void InitializeMapButtons()
        {
            MapButtonsPanel.Children.Clear();
            var mapNames = MapManager.MapNames; // 你的地图名称列表

            for (int i = 0; i < mapNames.Count; i++)
            {
                int mapIndex = i;
                string mapName = mapNames[i];

                var border = new Border
                {
                    Margin = new Thickness(0, 0, 0, 20), // 按钮间距
                    CornerRadius = new CornerRadius(10),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e293b")), // 深蓝灰按钮背景
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38bdf8")), // 亮蓝边框
                    BorderThickness = new Thickness(2),
                    Width = 500,
                    Opacity = 0,
                    RenderTransform = new TranslateTransform { X = 30, Y = 0 }
                };

                var button = new Button
                {
                    Content = mapName,
                    FontSize = 28,
                    Height = 80, // 按钮高度
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e2e8f0")), // 浅灰文字
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    BorderThickness = new Thickness(0)
                };

                button.Click += (sender, e) =>
                {
                    _mainWindow.MainFrame.Navigate(new GamePage(_mainWindow, mapIndex));
                };

                // 按钮样式
                Style buttonStyle = new Style(typeof(Button));
                Trigger mouseOverTrigger = new Trigger
                {
                    Property = UIElement.IsMouseOverProperty,
                    Value = true
                };
                mouseOverTrigger.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38bdf8")))); // 悬停文字变亮蓝
                mouseOverTrigger.Setters.Add(new Setter(RenderTransformProperty, new ScaleTransform(1.05, 1.05)));
                buttonStyle.Triggers.Add(mouseOverTrigger);
                buttonStyle.Setters.Add(new Setter(RenderTransformOriginProperty, new Point(0.5, 0.5)));
                buttonStyle.Setters.Add(new Setter(RenderTransformProperty, new ScaleTransform(1, 1)));
                button.Style = buttonStyle;

                border.Child = button;
                MapButtonsPanel.Children.Add(border);

                // 按钮入场动画
                var delay = TimeSpan.FromMilliseconds(i * 100);
                var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500)) { BeginTime = delay };
                var translateAnim = new DoubleAnimation(30, 0, TimeSpan.FromMilliseconds(500)) { BeginTime = delay };
                border.BeginAnimation(OpacityProperty, opacityAnim);
                ((TranslateTransform)border.RenderTransform).BeginAnimation(TranslateTransform.XProperty, translateAnim);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new StartMenuPage(_mainWindow));
        }
    }
}