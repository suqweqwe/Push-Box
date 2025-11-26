using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace SokobanGame
{
    // 定义地图元素类型
    public enum CellType
    {
        Empty,      // 空地
        Wall,       // 墙壁
        Box,        // 箱子
        Target,     // 目标点
        Player,     // 玩家
        BoxOnTarget // 箱子在目标点上
    }

    // 定义玩家移动方向
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    // 地图数据结构（新增目标点列表）
    public class MapData
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public CellType[,] Grid { get; set; }
        public int PlayerRow { get; set; }
        public int PlayerCol { get; set; }
        // 新增：存储所有目标点的坐标，确保不会丢失
        public List<Tuple<int, int>> TargetPositions { get; set; } = new List<Tuple<int, int>>();
    }

    // 游戏颜色配置
    public static class GameColors
    {
        public static readonly Brush WallBrush = Brushes.DarkBlue;
        public static readonly Brush EmptyBrush = Brushes.LightGray;
        public static readonly Brush BoxBrush = Brushes.Chocolate;
        public static readonly Brush TargetBrush = Brushes.LightGreen;
        public static readonly Brush PlayerBrush = Brushes.Blue;
        public static readonly Brush BoxOnTargetBrush = Brushes.Green;
    }
}