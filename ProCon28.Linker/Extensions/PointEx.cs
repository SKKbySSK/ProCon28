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
        public static float GetLength(this Point Point)
        {
            return (Point.X * Point.X) + (Point.Y * Point.Y);
        }

        /// <summary>
        /// 2点間の距離を計算します
        /// </summary>
        public static float GetLength(this Point From, Point To)
        {
            return GetLength(new Point(From.X - To.X, From.Y - To.Y));
        }
    }
}
