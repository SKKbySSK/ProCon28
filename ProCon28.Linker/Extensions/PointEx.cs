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

        public static double GetAngle(this Point Point)
        {
            return Math.Atan2(Point.Y, Point.X);
        }

        /// <summary>
        /// ピースの座標を指定した基準点を元に変換します。
        /// </summary>
        /// <param name="Piece"></param>
        /// <param name="BasePoint">基準となる点</param>
        /// <returns></returns>
        public static PointCollection Convert(this PointCollection Points, Point BasePoint)
        {
            PointCollection pcol = new PointCollection();
            for (int i = 0; Points.Count > i; i++)
            {
                Point p = Points[i];
                p.X -= BasePoint.X;
                p.Y -= BasePoint.Y;
                pcol.Add(p);
            }
            return pcol;
        }

        public static PointCollection Convert(this PointCollection Points)
        {
            if (Points.Count == 0) return Points;
            Point bp = Points[0];
            foreach (Point p in Points)
            {
                if (bp.X > p.X)
                    bp.X = p.X;
                if (bp.Y > p.Y)
                    bp.Y = p.Y;
            }
            return Convert(Points, bp);
        }

        public static PointCollection Move(this PointCollection Points, Point Delta)
        {
            PointCollection col = new PointCollection();
            foreach (var p in Points)
                col.Add(p + Delta);
            return col;
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

