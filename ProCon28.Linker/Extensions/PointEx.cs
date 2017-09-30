using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker.Extensions
{
    public static class PointEx
    {
        /// <summary>
        /// 0位置を基準とした点との距離を計算します
        /// </summary>
        public static double GetLength(this Point Point)
        {
            return Math.Sqrt((Point.X * Point.X) + (Point.Y * Point.Y));
        }

        /// <summary>
        /// 2点間の距離を計算します
        /// </summary>
        public static double GetLength(this Point From, Point To)
        {
            return GetLength(new Point(From.X - To.X, From.Y - To.Y));
        }

        public static IList<(Point, Point)> AsLines(this PointCollection Points)
        {
            List<(Point, Point)> ls = new List<(Point, Point)>();

            int c = Points.Count;
            for(int i = 0; c > i; i++)
            {
                if (i == 0)
                    ls.Add((Points[c - 1], Points[0]));
                else
                    ls.Add((Points[i - 1], Points[i]));
            }

            return ls;
        }

        public static IList<(Point, Point, double)> AsLinesWithLength(this PointCollection Points)
        {
            var lines = AsLines(Points);
            List<(Point, Point, double)> ls = new List<(Point, Point, double)>();
            foreach(var l in lines)
            {
                ls.Add((l.Item1, l.Item2, GetLength(l.Item1, l.Item2)));
            }
            return ls;
        }
    }

}

