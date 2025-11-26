using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
        private Border _player; // 保存玩家控件引用，避免频繁查找

        public GamePage(MainWindow mainWindow, int mapIndex)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _currentMapIndex = mapIndex;
            _currentMap = MapManager.GetMap(mapIndex);
            Loaded += GamePage_Loaded;
        }

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            DrawMap();
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
                    Border cell = new Border { BorderThickness = new Thickness(1), BorderBrush = Brushes.Black };

                    // 决定格子的背景色
                    if (IsTarget(i, j))
                    {
                        if (_currentMap.Grid[i, j] == CellType.Box || _currentMap.Grid[i, j] == CellType.BoxOnTarget)
                        {
                            cell.Background = GameColors.BoxOnTargetBrush;
                        }
                        else
                        {
                            cell.Background = GameColors.TargetBrush;
                        }
                    }
                    else
                    {
                        switch (_currentMap.Grid[i, j])
                        {
                            case CellType.Wall:
                                cell.Background = GameColors.WallBrush;
                                break;
                            case CellType.Box:
                                cell.Background = GameColors.BoxBrush;
                                break;
                            default:
                                cell.Background = GameColors.EmptyBrush;
                                break;
                        }
                    }

                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);
                    GameGrid.Children.Add(cell);
                }
            }

            // 重新创建玩家控件（解决隐身问题的关键）
            _player = new Border
            {
                Background = GameColors.PlayerBrush,
                CornerRadius = new CornerRadius(_cellSize / 4),
                Tag = "Player" // 标记为玩家
            };
            Grid.SetRow(_player, _currentMap.PlayerRow);
            Grid.SetColumn(_player, _currentMap.PlayerCol);
            GameGrid.Children.Add(_player);
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
            }
        }

        // 重置关卡
        private void ResetLevel()
        {
            _currentMap = MapManager.GetMap(_currentMapIndex);
            _isMoving = false;
            DrawMap();
            SuccessGrid.Visibility = Visibility.Collapsed;
        }

        private void BackToMapSelect_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new MapSelectPage(_mainWindow));
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetLevel();
        }
    }
}