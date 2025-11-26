using System;
using System.Collections.Generic;

namespace SokobanGame
{
    public static class MapManager
    {
        // 存储所有地图的名称
        public static List<string> MapNames { get; private set; } = new List<string>
        {
            "经典第一关",
            "仓库",
            "迷阵",
            "铁索连环"
        };

        // 根据索引获取地图数据
        public static MapData GetMap(int index)
        {
            switch (index)
            {
                case 0:
                    return CreateClassicMap();
                case 1:
                    return CreateWarehouseMap();
                case 2:
                    return CreateMazeMap();
                case 3:
                    return CreateBoxOnTargetMap();
                default:
                    return CreateClassicMap();
            }
        }

        // 经典第一关（已修复行长度不一致问题）
        private static MapData CreateClassicMap()
        {
            // 第一行是"########"，共8个字符，所有行都保持8个字符长度
            string[] mapString = {
                "########",  // 第1行：8个字符
                "##     #",  // 第3行：8个字符（注意前面没有多余空格）
                "## .$. #",  // 第4行：8个字符（已修复）
                "## $@$ #",  // 第5行：8个字符
                "#  .$. #",  // 第6行：8个字符
                "#      #", // 第7行：8个字符
                "########"   // 第9行：8个字符
            };
            return ParseMapString(mapString);
        }

        // 仓库地图
        private static MapData CreateWarehouseMap()
        {
            string[] mapString = {
                "#######",
                "#. #  #",
                "#  $  #",
                "#. $#@#",
                "#  $  #",
                "#. #  #",
                "#######",
            };
            return ParseMapString(mapString);
        }

        // 迷阵地图
        private static MapData CreateMazeMap()
        {
            string[] mapString = {
                "#######",
                "##  ###",
                "#. $###",
                "#.$ ###",
                "#.$ ###",
                "#.$ ###",
                "#. $###",
                "#   @##",
                "##   ##",
                "#######"
            };
            return ParseMapString(mapString);
        }

        // 初始箱子在目标点上的地图
        private static MapData CreateBoxOnTargetMap()
        {
            string[] mapString = {
                "########",
                "#  #.  #",
                "##  *  #",
                "#  @ .##",
                "##$$# .#",
                "# $    #",
                "# $ #  #",
                "#   # .#",
                "########"
            };
            return ParseMapString(mapString);
        }

        // 解析字符串为MapData对象
        private static MapData ParseMapString(string[] mapString)
        {
            MapData map = new MapData();
            map.Rows = mapString.Length;
            int rowLength = mapString[0].Length;

            // 验证行长度
            for (int i = 0; i < map.Rows; i++)
            {
                if (mapString[i].Length != rowLength)
                {
                    // 增强错误信息，显示具体长度差异
                    throw new System.Exception(
                        $"地图数据错误：第 {i + 1} 行长度为 {mapString[i].Length}，" +
                        $"第一行长度为 {rowLength}，两者不一致");
                }
            }

            map.Cols = rowLength;
            map.Grid = new CellType[map.Rows, map.Cols];
            map.TargetPositions = new List<Tuple<int, int>>();

            for (int i = 0; i < map.Rows; i++)
            {
                for (int j = 0; j < map.Cols; j++)
                {
                    char c = mapString[i][j];
                    switch (c)
                    {
                        case '#':
                            map.Grid[i, j] = CellType.Wall;
                            break;
                        case ' ':
                            map.Grid[i, j] = CellType.Empty;
                            break;
                        case '$':
                            map.Grid[i, j] = CellType.Box;
                            break;
                        case '.':
                            map.Grid[i, j] = CellType.Target;
                            map.TargetPositions.Add(Tuple.Create(i, j));
                            break;
                        case '@':
                            map.Grid[i, j] = CellType.Empty;
                            map.PlayerRow = i;
                            map.PlayerCol = j;
                            break;
                        case '*':
                            map.Grid[i, j] = CellType.BoxOnTarget;
                            map.TargetPositions.Add(Tuple.Create(i, j));
                            break;
                        default:
                            map.Grid[i, j] = CellType.Empty;
                            break;
                    }
                }
            }
            return map;
        }
    }
}