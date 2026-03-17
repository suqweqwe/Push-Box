using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SokobanGame
{
    /// <summary>
    /// GamePage.xaml 的交互逻辑
    /// </summary>
    public partial class GamePage : Page
    {
        private MainWindow _mainWindow;
        private MapData _currentMap;
        private int _cellSize = 40; // 每个格子的大小（像素）
        private int _currentMapIndex; // 当前地图索引
        private bool _isMoving = false; // 标记是否正在移动中
        private double _moveDuration = 0.1; // 移动动画持续时间（秒）
        private Image _player; // 保存玩家控件引用，避免频繁查找
        private ImageSource _playerUpSource;
        private ImageSource _playerDownSource;
        private ImageSource _playerLeftSource;
        private ImageSource _playerRightSource;
        private Direction _lastDirection = Direction.Down;
        private readonly string _playerUpPath = System.IO.Path.Combine("Assets", "player_up.png");
        private readonly string _playerDownPath = System.IO.Path.Combine("Assets", "player_down.png");
        private readonly string _playerLeftPath = System.IO.Path.Combine("Assets", "player_left.png");
        private readonly string _playerRightPath = System.IO.Path.Combine("Assets", "player_right.png");
        
        // 步数统计和撤回功能
        private int _stepCount = 0;
        private Stack<GameState> _historyStack = new Stack<GameState>(); // 历史状态栈
        
        // 激光系统
        private DispatcherTimer _laserTimer;
        private Random _random = new Random();
        private Rectangle _warningLaser; // 预警激光
        private Rectangle _actualLaser; // 实际激光
        private bool _laserActive = false;
        private const double LaserWarningDuration = 1.5; // 预警时间（秒）
        private const double LaserActiveDuration = 0.3; // 激光持续时间（秒）

        public GamePage(MainWindow mainWindow, int mapIndex)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _currentMapIndex = mapIndex;
            _currentMap = MapManager.GetMap(mapIndex);
            LoadPlayerImages();
            Loaded += GamePage_Loaded;
        }

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            DrawMap();
            UpdateStepCount();
            // 保存初始状态
            SaveGameState();
            // 确保页面获得键盘焦点，以便接收KeyDown事件
            Keyboard.Focus(this);
        }

        // 绘制地图（只在初始化和地图变化时调用）
        private void DrawMap()
        {
            // 清除所有元素，包括玩家（解决隐身问题的关键）
            GameGrid.Children.Clear();

            // 重新创建行列定义
            GameGrid.RowDefinitions.Clear();
            for (int i = 0; i < _currentMap.Rows; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_cellSize) });
            }

            GameGrid.ColumnDefinitions.Clear();
            for (int j = 0; j < _currentMap.Cols; j++)
            {
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(_cellSize) });
            }

            // 绘制地图格子
            for (int i = 0; i < _currentMap.Rows; i++)
            {
                for (int j = 0; j < _currentMap.Cols; j++)
                {
                    Border cell = new Border 
                    { 
                        BorderThickness = new Thickness(0.5), 
                        BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100))
                    };

                    // 决定格子的背景色和效果
                    if (IsTarget(i, j))
                    {
                        if (_currentMap.Grid[i, j] == CellType.Box || _currentMap.Grid[i, j] == CellType.BoxOnTarget)
                        {
                            cell.Background = GameColors.CreateEmptyBrush();
                        }
                        else
                        {
                            // 目标点：使用Grid叠加圆形标记
                            Grid targetGrid = new Grid();
                            targetGrid.Background = GameColors.CreateEmptyBrush();
                            
                            // 添加圆形目标标记
                            Ellipse targetMarker = new Ellipse
                            {
                                Width = _cellSize * 0.6,
                                Height = _cellSize * 0.6,
                                Fill = GameColors.CreateTargetBrush(),
                                Stroke = new SolidColorBrush(Color.FromRgb(50, 150, 50)),
                                StrokeThickness = 2,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            
                            targetGrid.Children.Add(targetMarker);
                            cell.Child = targetGrid;
                        }
                    }
                    else
                    {
                        switch (_currentMap.Grid[i, j])
                        {
                            case CellType.Wall:
                                cell.Background = GameColors.CreateWallBrush();
                                cell.Effect = GameColors.CreateWallShadowEffect();
                                break;
                            case CellType.Box:
                                // 箱子会在后面单独绘制，这里只绘制背景
                                cell.Background = GameColors.CreateEmptyBrush();
                                break;
                            default:
                                cell.Background = GameColors.CreateEmptyBrush();
                                break;
                        }
                    }

                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);
                    GameGrid.Children.Add(cell);
                }
            }

            // 单独绘制箱子，添加更真实的样式
            for (int i = 0; i < _currentMap.Rows; i++)
            {
                for (int j = 0; j < _currentMap.Cols; j++)
                {
                    if (_currentMap.Grid[i, j] == CellType.Box || _currentMap.Grid[i, j] == CellType.BoxOnTarget)
                    {
                        Border box = CreateBoxElement(_currentMap.Grid[i, j] == CellType.BoxOnTarget);
                        Grid.SetRow(box, i);
                        Grid.SetColumn(box, j);
                        GameGrid.Children.Add(box);
                    }
                }
            }

            // 重新创建玩家控件（解决隐身问题的关键）
            _player = new Image
            {
                Width = _cellSize,
                Height = _cellSize,
                Stretch = Stretch.Uniform,
                Source = GetSourceForDirection(_lastDirection),
                Tag = "Player" // 标记为玩家
            };
            Grid.SetRow(_player, _currentMap.PlayerRow);
            Grid.SetColumn(_player, _currentMap.PlayerCol);
            GameGrid.Children.Add(_player);
        }

        // 创建箱子元素，带真实的3D效果
        private Border CreateBoxElement(bool isOnTarget)
        {
            // 使用Grid容器来叠加高光效果
            Grid container = new Grid();
            
            // 主箱子
            Border box = new Border
            {
                Width = _cellSize * 0.9,
                Height = _cellSize * 0.9,
                CornerRadius = new CornerRadius(3),
                Background = isOnTarget ? GameColors.CreateBoxOnTargetBrush() : GameColors.CreateBoxBrush(),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromRgb(100, 70, 40)),
                Effect = GameColors.CreateShadowEffect(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // 添加高光效果，让箱子看起来更立体
            Border highlight = new Border
            {
                Width = _cellSize * 0.25,
                Height = _cellSize * 0.25,
                CornerRadius = new CornerRadius(1),
                Background = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(_cellSize * 0.1, _cellSize * 0.1, 0, 0)
            };

            container.Children.Add(box);
            container.Children.Add(highlight);

            return new Border
            {
                Child = container,
                Background = Brushes.Transparent
            };
        }

        // 检查某个位置是否是目标点
        private bool IsTarget(int row, int col)
        {
            return _currentMap.TargetPositions.Contains(Tuple.Create(row, col));
        }

        // 玩家移动逻辑（带动画）
        private void MovePlayer(Direction dir)
        {
            // 如果正在移动中，忽略新的移动指令
            if (_isMoving) return;

            int newRow = _currentMap.PlayerRow;
            int newCol = _currentMap.PlayerCol;

            switch (dir)
            {
                case Direction.Up: newRow--; break;
                case Direction.Down: newRow++; break;
                case Direction.Left: newCol--; break;
                case Direction.Right: newCol++; break;
            }

            // 根据移动方向切换人物贴图
            _lastDirection = dir;
            _player.Source = GetSourceForDirection(dir);

            // 检查边界
            if (newRow < 0 || newRow >= _currentMap.Rows || newCol < 0 || newCol >= _currentMap.Cols)
                return;

            CellType newCell = _currentMap.Grid[newRow, newCol];
            bool isPushingBox = false;
            int boxNewRow = 0, boxNewCol = 0;

            // 检查是否可以移动
            if (newCell == CellType.Wall)
            {
                return; // 撞墙
            }
            else if (newCell == CellType.Box || newCell == CellType.BoxOnTarget)
            {
                // 计算箱子新位置
                boxNewRow = newRow + (newRow - _currentMap.PlayerRow);
                boxNewCol = newCol + (newCol - _currentMap.PlayerCol);

                if (boxNewRow < 0 || boxNewRow >= _currentMap.Rows || boxNewCol < 0 || boxNewCol >= _currentMap.Cols)
                    return;

                CellType boxNewCell = _currentMap.Grid[boxNewRow, boxNewCol];
                if (boxNewCell != CellType.Empty && boxNewCell != CellType.Target)
                {
                    return; // 箱子推不动
                }
                isPushingBox = true;
            }

            // 确认可以移动后，在移动前保存当前状态（用于撤回）
            SaveGameState();

            // 开始移动动画
            _isMoving = true;

            // 创建位移动画
            var rowAnimation = new Int32Animation
            {
                To = newRow,
                Duration = TimeSpan.FromSeconds(_moveDuration)
            };

            var colAnimation = new Int32Animation
            {
                To = newCol,
                Duration = TimeSpan.FromSeconds(_moveDuration)
            };

            // 动画完成后更新数据并重新绘制
            rowAnimation.Completed += (s, e) =>
            {
                // 更新玩家位置
                _currentMap.PlayerRow = newRow;
                _currentMap.PlayerCol = newCol;

                // 如果推了箱子，更新箱子位置
                if (isPushingBox)
                {
                    _currentMap.Grid[boxNewRow, boxNewCol] = IsTarget(boxNewRow, boxNewCol)
                        ? CellType.BoxOnTarget
                        : CellType.Box;

                    _currentMap.Grid[newRow, newCol] = IsTarget(newRow, newCol)
                        ? CellType.Target
                        : CellType.Empty;

                    // 箱子移动时重绘地图，包括玩家（解决隐身问题）
                    DrawMap();
                }
                else
                {
                    // 仅玩家移动时，直接更新玩家位置而不重绘整个地图
                    Grid.SetRow(_player, newRow);
                    Grid.SetColumn(_player, newCol);
                }

                _isMoving = false;

                // 增加步数并更新显示
                _stepCount++;
                UpdateStepCount();

                // 检查是否通关
                if (CheckWin())
                {
                    SuccessGrid.Visibility = Visibility.Visible;
                    // 延迟返回地图选择
                    System.Timers.Timer timer = new System.Timers.Timer(2000);
                    timer.Elapsed += (sTimer, args) =>
                    {
                        timer.Dispose();
                        Dispatcher.Invoke(() => BackToMapSelect_Click(null, null));
                    };
                    timer.Start();
                }
            };

            // 应用动画
            _player.BeginAnimation(Grid.RowProperty, rowAnimation);
            _player.BeginAnimation(Grid.ColumnProperty, colAnimation);
        }

        // 检查是否所有箱子都在目标点上
        private bool CheckWin()
        {
            for (int i = 0; i < _currentMap.Rows; i++)
            {
                for (int j = 0; j < _currentMap.Cols; j++)
                {
                    if (_currentMap.Grid[i, j] == CellType.Box)
                        return false;
                }
            }
            return true;
        }

        // 加载人物图片资源（上下、左右各一张），缺失时使用简易占位图
        private void LoadPlayerImages()
        {
            _playerUpSource = LoadImageOrNull(_playerUpPath);
            _playerDownSource = LoadImageOrNull(_playerDownPath);
            _playerLeftSource = LoadImageOrNull(_playerLeftPath);
            _playerRightSource = LoadImageOrNull(_playerRightPath);

            // 如果缺失某个方向，则尝试用下、上、左、右的已存在图片依次填充，最后用占位
            var fallback = CreateFallbackImage();
            if (_playerDownSource == null) _playerDownSource = _playerUpSource ?? _playerLeftSource ?? _playerRightSource ?? fallback;
            if (_playerUpSource == null) _playerUpSource = _playerDownSource ?? _playerLeftSource ?? _playerRightSource ?? fallback;
            if (_playerLeftSource == null) _playerLeftSource = _playerRightSource ?? _playerDownSource ?? _playerUpSource ?? fallback;
            if (_playerRightSource == null) _playerRightSource = _playerLeftSource ?? _playerDownSource ?? _playerUpSource ?? fallback;
        }

        private ImageSource LoadImageOrNull(string relativePath)
        {
            try
            {
                // 使用 pack://application URI 从资源加载
                // 格式：pack://application:,,,/AssemblyName;component/Path
                var normalizedPath = relativePath.Replace('\\', '/');
                var uriString = "pack://application:,,,/SokobanGame;component/" + normalizedPath;
                var resourceUri = new Uri(uriString, UriKind.Absolute);
                
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = resourceUri;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                // 如果资源加载失败，尝试从文件系统加载（备用方案）
                try
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var fullPath = System.IO.Path.Combine(baseDir, relativePath);
                    if (!File.Exists(fullPath)) return null;

                    var fileImage = new BitmapImage();
                    fileImage.BeginInit();
                    fileImage.UriSource = new Uri(fullPath, UriKind.Absolute);
                    fileImage.CacheOption = BitmapCacheOption.OnLoad;
                    fileImage.EndInit();
                    fileImage.Freeze();
                    return fileImage;
                }
                catch
                {
                    return null;
                }
            }
        }

        private ImageSource CreateFallbackImage()
        {
            // 简单的蓝色圆角方块占位图，避免缺失资源时崩溃
            var geometry = new RectangleGeometry(new Rect(0, 0, _cellSize, _cellSize), _cellSize / 6, _cellSize / 6);
            var drawing = new GeometryDrawing(
                Brushes.SteelBlue,
                new Pen(Brushes.Black, 2),
                geometry);
            var image = new DrawingImage(drawing);
            image.Freeze();
            return image;
        }

        private ImageSource GetSourceForDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.Up: return _playerUpSource;
                case Direction.Left: return _playerLeftSource;
                case Direction.Right: return _playerRightSource;
                case Direction.Down:
                default: return _playerDownSource;
            }
        }

        // 键盘事件处理
        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (SuccessGrid.Visibility == Visibility.Visible) return;

            // 防止按键重复导致的卡顿
            e.Handled = true;

            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    MovePlayer(Direction.Up);
                    break;
                case Key.Down:
                case Key.S:
                    MovePlayer(Direction.Down);
                    break;
                case Key.Left:
                case Key.A:
                    MovePlayer(Direction.Left);
                    break;
                case Key.Right:
                case Key.D:
                    MovePlayer(Direction.Right);
                    break;
                case Key.R:
                    ResetLevel();
                    break;
                case Key.Q:
                    UndoMove();
                    break;
            }
        }

        // 重置关卡
        private void ResetLevel()
        {
            _currentMap = MapManager.GetMap(_currentMapIndex);
            _isMoving = false;
            _stepCount = 0;
            _historyStack.Clear();
            RemoveLaser(); // 移除激光
            DrawMap();
            UpdateStepCount();
            SaveGameState(); // 保存重置后的初始状态
            SuccessGrid.Visibility = Visibility.Collapsed;
        }

        // 更新步数显示
        private void UpdateStepCount()
        {
            if (StepCountText != null)
            {
                StepCountText.Text = _stepCount.ToString();
            }
            if (UndoButton != null)
            {
                UndoButton.IsEnabled = _historyStack.Count > 0;
            }
        }

        // 保存游戏状态（用于撤回）
        private void SaveGameState()
        {
            GameState state = new GameState
            {
                PlayerRow = _currentMap.PlayerRow,
                PlayerCol = _currentMap.PlayerCol,
                Grid = (CellType[,])_currentMap.Grid.Clone(),
                LastDirection = _lastDirection
            };
            _historyStack.Push(state);
        }

        // 撤回上一步
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            UndoMove();
        }

        // 撤回移动（可被按钮和快捷键调用）
        private void UndoMove()
        {
            // 至少需要2个状态才能撤回（初始状态 + 至少一次移动）
            if (_historyStack.Count <= 1 || _isMoving) return;

            // 栈顶是当前状态（移动前保存的），先恢复它
            GameState stateToRestore = _historyStack.Peek();
            
            // 恢复到上一步状态
            _currentMap.PlayerRow = stateToRestore.PlayerRow;
            _currentMap.PlayerCol = stateToRestore.PlayerCol;
            _currentMap.Grid = (CellType[,])stateToRestore.Grid.Clone();
            _lastDirection = stateToRestore.LastDirection;

            // 弹出已恢复的状态
            _historyStack.Pop();

            _stepCount = Math.Max(0, _stepCount - 1);
            UpdateStepCount();
            DrawMap();
        }

        private void BackToMapSelect_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new MapSelectPage(_mainWindow));
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetLevel();
        }

        // 初始化激光系统
        private void InitializeLaserSystem()
        {
            _laserTimer = new DispatcherTimer();
            _laserTimer.Interval = TimeSpan.FromSeconds(5);
            _laserTimer.Tick += LaserTimer_Tick;
            _laserTimer.Start();
        }

        // 激光定时器事件
        private void LaserTimer_Tick(object sender, EventArgs e)
        {
            if (_isMoving || SuccessGrid.Visibility == Visibility.Visible) return;
            
            FireLaser();
        }

        // 发射激光
        private void FireLaser()
        {
            // 获取所有可移动的位置（非墙壁）
            List<Tuple<int, int>> movablePositions = new List<Tuple<int, int>>();
            for (int i = 0; i < _currentMap.Rows; i++)
            {
                for (int j = 0; j < _currentMap.Cols; j++)
                {
                    if (_currentMap.Grid[i, j] != CellType.Wall)
                    {
                        movablePositions.Add(Tuple.Create(i, j));
                    }
                }
            }

            if (movablePositions.Count == 0) return;

            // 随机选择方向（上、下、左、右）
            Direction[] directions = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
            Direction laserDirection = directions[_random.Next(directions.Length)];

            // 根据方向随机选择行或列
            int targetRow = 0, targetCol = 0;
            switch (laserDirection)
            {
                case Direction.Up:
                case Direction.Down:
                    // 垂直激光，随机选择一列
                    List<int> validCols = new List<int>();
                    for (int j = 0; j < _currentMap.Cols; j++)
                    {
                        bool hasMovable = false;
                        for (int i = 0; i < _currentMap.Rows; i++)
                        {
                            if (_currentMap.Grid[i, j] != CellType.Wall)
                            {
                                hasMovable = true;
                                break;
                            }
                        }
                        if (hasMovable) validCols.Add(j);
                    }
                    if (validCols.Count > 0)
                        targetCol = validCols[_random.Next(validCols.Count)];
                    else
                        return;
                    targetRow = _currentMap.Rows / 2; // 中间行作为参考
                    break;
                case Direction.Left:
                case Direction.Right:
                    // 水平激光，随机选择一行
                    List<int> validRows = new List<int>();
                    for (int i = 0; i < _currentMap.Rows; i++)
                    {
                        bool hasMovable = false;
                        for (int j = 0; j < _currentMap.Cols; j++)
                        {
                            if (_currentMap.Grid[i, j] != CellType.Wall)
                            {
                                hasMovable = true;
                                break;
                            }
                        }
                        if (hasMovable) validRows.Add(i);
                    }
                    if (validRows.Count > 0)
                        targetRow = validRows[_random.Next(validRows.Count)];
                    else
                        return;
                    targetCol = _currentMap.Cols / 2; // 中间列作为参考
                    break;
            }

            // 显示预警激光
            ShowWarningLaser(targetRow, targetCol, laserDirection);
        }

        // 显示预警激光（透明红色）
        private void ShowWarningLaser(int row, int col, Direction direction)
        {
            // 移除之前的激光
            RemoveLaser();

            // 计算激光的起始和结束位置（从边界射向目标位置）
            int startRow = row, startCol = col;
            int endRow = row, endCol = col;

            switch (direction)
            {
                case Direction.Up:
                    // 从上往下射
                    startRow = 0;
                    endRow = _currentMap.Rows - 1;
                    break;
                case Direction.Down:
                    // 从下往上射
                    startRow = 0;
                    endRow = _currentMap.Rows - 1;
                    break;
                case Direction.Left:
                    // 从左往右射
                    startCol = 0;
                    endCol = _currentMap.Cols - 1;
                    break;
                case Direction.Right:
                    // 从右往左射
                    startCol = 0;
                    endCol = _currentMap.Cols - 1;
                    break;
            }

            // 创建预警激光（透明红色）
            _warningLaser = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(150, 255, 0, 0)), // 半透明红色
                Stroke = new SolidColorBrush(Color.FromArgb(200, 255, 100, 100)),
                StrokeThickness = 2,
                Opacity = 0.6
            };

            // 设置激光位置和大小
            if (direction == Direction.Up || direction == Direction.Down)
            {
                // 垂直激光
                Grid.SetColumn(_warningLaser, startCol);
                Grid.SetColumnSpan(_warningLaser, 1);
                Grid.SetRow(_warningLaser, Math.Min(startRow, endRow));
                Grid.SetRowSpan(_warningLaser, Math.Abs(endRow - startRow) + 1);
                _warningLaser.Width = _cellSize;
                _warningLaser.Height = _cellSize * (Math.Abs(endRow - startRow) + 1);
            }
            else
            {
                // 水平激光
                Grid.SetRow(_warningLaser, startRow);
                Grid.SetRowSpan(_warningLaser, 1);
                Grid.SetColumn(_warningLaser, Math.Min(startCol, endCol));
                Grid.SetColumnSpan(_warningLaser, Math.Abs(endCol - startCol) + 1);
                _warningLaser.Width = _cellSize * (Math.Abs(endCol - startCol) + 1);
                _warningLaser.Height = _cellSize;
            }

            GameGrid.Children.Add(_warningLaser);

            // 预警后显示实际激光
            var warningTimer = new DispatcherTimer();
            warningTimer.Interval = TimeSpan.FromSeconds(LaserWarningDuration);
            warningTimer.Tick += (s, e) =>
            {
                warningTimer.Stop();
                ShowActualLaser(row, col, direction);
            };
            warningTimer.Start();
        }

        // 显示实际激光（实心红色）
        private void ShowActualLaser(int row, int col, Direction direction)
        {
            // 移除预警激光
            if (_warningLaser != null)
            {
                GameGrid.Children.Remove(_warningLaser);
                _warningLaser = null;
            }

            // 计算激光的起始和结束位置（从边界射向目标位置）
            int startRow = row, startCol = col;
            int endRow = row, endCol = col;

            switch (direction)
            {
                case Direction.Up:
                    // 从上往下射
                    startRow = 0;
                    endRow = _currentMap.Rows - 1;
                    break;
                case Direction.Down:
                    // 从下往上射
                    startRow = 0;
                    endRow = _currentMap.Rows - 1;
                    break;
                case Direction.Left:
                    // 从左往右射
                    startCol = 0;
                    endCol = _currentMap.Cols - 1;
                    break;
                case Direction.Right:
                    // 从右往左射
                    startCol = 0;
                    endCol = _currentMap.Cols - 1;
                    break;
            }

            // 创建实际激光（实心红色）
            _actualLaser = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(220, 255, 0, 0)), // 实心红色
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 3,
                Opacity = 0.9
            };

            // 设置激光位置和大小
            if (direction == Direction.Up || direction == Direction.Down)
            {
                // 垂直激光
                Grid.SetColumn(_actualLaser, startCol);
                Grid.SetColumnSpan(_actualLaser, 1);
                Grid.SetRow(_actualLaser, Math.Min(startRow, endRow));
                Grid.SetRowSpan(_actualLaser, Math.Abs(endRow - startRow) + 1);
                _actualLaser.Width = _cellSize;
                _actualLaser.Height = _cellSize * (Math.Abs(endRow - startRow) + 1);
            }
            else
            {
                // 水平激光
                Grid.SetRow(_actualLaser, startRow);
                Grid.SetRowSpan(_actualLaser, 1);
                Grid.SetColumn(_actualLaser, Math.Min(startCol, endCol));
                Grid.SetColumnSpan(_actualLaser, Math.Abs(endCol - startCol) + 1);
                _actualLaser.Width = _cellSize * (Math.Abs(endCol - startCol) + 1);
                _actualLaser.Height = _cellSize;
            }

            GameGrid.Children.Add(_actualLaser);
            _laserActive = true;

            // 检测是否击中玩家
            CheckLaserHitPlayer(row, col, direction);

            // 激光持续一段时间后消失
            var laserTimer = new DispatcherTimer();
            laserTimer.Interval = TimeSpan.FromSeconds(LaserActiveDuration);
            laserTimer.Tick += (s, e) =>
            {
                laserTimer.Stop();
                RemoveLaser();
            };
            laserTimer.Start();
        }

        // 检测激光是否击中玩家
        private void CheckLaserHitPlayer(int row, int col, Direction direction)
        {
            // 检查激光路径上是否有玩家
            int startRow = row, startCol = col;
            int endRow = row, endCol = col;

            switch (direction)
            {
                case Direction.Up:
                    startRow = 0;
                    endRow = row;
                    break;
                case Direction.Down:
                    startRow = row;
                    endRow = _currentMap.Rows - 1;
                    break;
                case Direction.Left:
                    startCol = 0;
                    endCol = col;
                    break;
                case Direction.Right:
                    startCol = col;
                    endCol = _currentMap.Cols - 1;
                    break;
            }

            // 检查激光路径上的所有位置
            if (direction == Direction.Up || direction == Direction.Down)
            {
                // 垂直激光，检查整列
                for (int i = Math.Min(startRow, endRow); i <= Math.Max(startRow, endRow); i++)
                {
                    if (i == _currentMap.PlayerRow && col == _currentMap.PlayerCol)
                    {
                        // 玩家被击中，重置关卡
                        OnPlayerHitByLaser();
                        return;
                    }
                }
            }
            else
            {
                // 水平激光，检查整行
                for (int j = Math.Min(startCol, endCol); j <= Math.Max(startCol, endCol); j++)
                {
                    if (row == _currentMap.PlayerRow && j == _currentMap.PlayerCol)
                    {
                        // 玩家被击中，重置关卡
                        OnPlayerHitByLaser();
                        return;
                    }
                }
            }
        }

        // 玩家被激光击中
        private void OnPlayerHitByLaser()
        {
            RemoveLaser();
            // 显示提示信息
            MessageBox.Show("你被激光击中了！关卡重置。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            // 重置关卡
            ResetLevel();
        }

        // 移除激光
        private void RemoveLaser()
        {
            if (_warningLaser != null)
            {
                GameGrid.Children.Remove(_warningLaser);
                _warningLaser = null;
            }
            if (_actualLaser != null)
            {
                GameGrid.Children.Remove(_actualLaser);
                _actualLaser = null;
            }
            _laserActive = false;
        }

    }

    // 游戏状态类，用于保存和恢复游戏状态（撤回功能）
    public class GameState
    {
        public int PlayerRow { get; set; }
        public int PlayerCol { get; set; }
        public CellType[,] Grid { get; set; }
        public Direction LastDirection { get; set; }
    }
}