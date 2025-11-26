using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SokobanGame
{
    public partial class StartMenuPage : Page
    {
        private Random _random = new Random();
        private DispatcherTimer _meteorTimer = new DispatcherTimer();
        private MainWindow _mainWindow; // 存储主窗口引用

        // 带参数的构造函数（解决CS1729错误）
        public StartMenuPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化流星雨定时器
            _meteorTimer.Interval = TimeSpan.FromMilliseconds(200);
            _meteorTimer.Tick += CreateMeteor;
            _meteorTimer.Start();
        }

        // 创建流星（解决CS0103错误：MeteorCanvas已在XAML中正确定义）
        private void CreateMeteor(object sender, EventArgs e)
        {
            // 避免页面未加载完成时执行
            if (MeteorCanvas.ActualWidth == 0 || MeteorCanvas.ActualHeight == 0)
                return;

            // 随机流星起始位置（顶部区域）
            double startX = _random.Next(0, (int)MeteorCanvas.ActualWidth);
            double startY = 0;

            // 随机流星结束位置（底部区域）
            double endX = startX + _random.Next(100, 300);
            double endY = _random.Next((int)MeteorCanvas.ActualHeight / 2, (int)MeteorCanvas.ActualHeight);

            // 随机流星长度和大小
            int length = _random.Next(10, 40);
            int width = _random.Next(1, 3);
            double duration = _random.Next(500, 1500);

            // 创建流星主体
            var meteor = new Border
            {
                Width = length,
                Height = width,
                Background = new LinearGradientBrush(
                    Colors.White,
                    Color.FromArgb(100, 200, 200, 255),
                    0),
                CornerRadius = new CornerRadius(2),
                RenderTransformOrigin = new Point(0, 0.5)
            };

            // 计算流星角度
            double angle = Math.Atan2(endY - startY, endX - startX) * 180 / Math.PI;
            meteor.RenderTransform = new RotateTransform(angle);

            // 添加到画布
            Canvas.SetLeft(meteor, startX);
            Canvas.SetTop(meteor, startY);
            MeteorCanvas.Children.Add(meteor);

            // 创建流星动画
            var animation = new DoubleAnimation
            {
                From = startY,
                To = endY,
                Duration = TimeSpan.FromMilliseconds(duration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            // 流星位置动画（Y轴）
            animation.Completed += (s, args) =>
            {
                // 动画结束后移除流星
                MeteorCanvas.Children.Remove(meteor);
            };

            // 流星透明度动画（逐渐消失）
            var opacityAnim = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(duration)
            };

            // 应用动画
            meteor.BeginAnimation(Canvas.TopProperty, animation);
            meteor.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation(startX, endX, TimeSpan.FromMilliseconds(duration)));
            meteor.BeginAnimation(OpacityProperty, opacityAnim);
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            // 停止流星动画
            _meteorTimer.Stop();

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