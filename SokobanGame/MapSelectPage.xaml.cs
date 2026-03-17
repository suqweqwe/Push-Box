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
                    Margin = new Thickness(0, 0, 0, 25), // 按钮间距
                    CornerRadius = new CornerRadius(15),
                    Width = 550,
                    Height = 90,
                    Opacity = 0,
                    RenderTransform = new TranslateTransform { X = 50, Y = 0 },
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                // 渐变背景
                var backgroundBrush = new LinearGradientBrush();
                backgroundBrush.StartPoint = new Point(0, 0);
                backgroundBrush.EndPoint = new Point(0, 1);
                backgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1e3a5f"), 0));
                backgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#2a4a6f"), 1));
                border.Background = backgroundBrush;

                // 渐变边框
                var borderBrush = new LinearGradientBrush();
                borderBrush.StartPoint = new Point(0, 0);
                borderBrush.EndPoint = new Point(1, 1);
                borderBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#00D9FF"), 0));
                borderBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#0099CC"), 1));
                border.BorderBrush = borderBrush;
                border.BorderThickness = new Thickness(2);

                // 阴影效果
                border.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = (Color)ColorConverter.ConvertFromString("#00D9FF"),
                    BlurRadius = 20,
                    Opacity = 0.5,
                    ShadowDepth = 0
                };

                var button = new Button
                {
                    Content = mapName,
                    FontSize = 30,
                    FontWeight = FontWeights.Bold,
                    Height = 90,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")), // 白色文字
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
                mouseOverTrigger.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D9FF")))); // 悬停文字变亮蓝
                mouseOverTrigger.Setters.Add(new Setter(RenderTransformProperty, new ScaleTransform(1.08, 1.08)));
                buttonStyle.Triggers.Add(mouseOverTrigger);
                buttonStyle.Setters.Add(new Setter(RenderTransformOriginProperty, new Point(0.5, 0.5)));
                buttonStyle.Setters.Add(new Setter(RenderTransformProperty, new ScaleTransform(1, 1)));
                button.Style = buttonStyle;

                // Border悬停效果
                Style borderStyle = new Style(typeof(Border));
                Trigger borderMouseOverTrigger = new Trigger
                {
                    Property = UIElement.IsMouseOverProperty,
                    Value = true
                };
                borderMouseOverTrigger.Setters.Add(new Setter(RenderTransformProperty, new ScaleTransform(1.05, 1.05)));
                borderStyle.Triggers.Add(borderMouseOverTrigger);
                borderStyle.Setters.Add(new Setter(RenderTransformOriginProperty, new Point(0.5, 0.5)));
                borderStyle.Setters.Add(new Setter(RenderTransformProperty, new ScaleTransform(1, 1)));
                border.Style = borderStyle;

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