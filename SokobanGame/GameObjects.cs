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
        // 墙壁：深灰色渐变，营造砖块效果
        public static Brush CreateWallBrush()
        {
            return new LinearGradientBrush(
                Color.FromRgb(60, 60, 70),
                Color.FromRgb(40, 40, 50),
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1));
        }

        // 空地：浅灰色渐变
        public static Brush CreateEmptyBrush()
        {
            return new LinearGradientBrush(
                Color.FromRgb(240, 240, 240),
                Color.FromRgb(220, 220, 220),
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1));
        }

        // 箱子：棕色渐变，营造木箱效果
        public static Brush CreateBoxBrush()
        {
            return new LinearGradientBrush(
                Color.FromRgb(210, 180, 140),
                Color.FromRgb(139, 90, 43),
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1));
        }

        // 目标点：绿色圆形标记
        public static Brush CreateTargetBrush()
        {
            return new RadialGradientBrush(
                Color.FromArgb(200, 144, 238, 144),
                Color.FromArgb(150, 50, 205, 50));
        }

        // 箱子在目标点上：深绿色渐变
        public static Brush CreateBoxOnTargetBrush()
        {
            return new LinearGradientBrush(
                Color.FromRgb(180, 220, 180),
                Color.FromRgb(100, 180, 100),
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1));
        }

        // 创建阴影效果
        public static System.Windows.Media.Effects.Effect CreateShadowEffect()
        {
            return new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 315,
                ShadowDepth = 3,
                Opacity = 0.5,
                BlurRadius = 4
            };
        }

        // 创建墙壁阴影效果（更明显）
        public static System.Windows.Media.Effects.Effect CreateWallShadowEffect()
        {
            return new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 315,
                ShadowDepth = 4,
                Opacity = 0.7,
                BlurRadius = 5
            };
        }
    }
}