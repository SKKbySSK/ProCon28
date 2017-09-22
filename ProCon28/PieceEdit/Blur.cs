using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;
using ProCon28.Linker.Extensions;

namespace ProCon28.PieceEdit
{
    static class Blur
    {
        public static Piece Run(Piece Piece, int Size)
        {
            Piece p = Piece.Convert();

            int mx = 0, my = 0;
            foreach(Point point in p.Vertexes)
            {
                mx = point.X > mx ? point.X : mx;
                my = point.Y > my ? point.Y : my;
            }
            
            int xtime = (int)Math.Ceiling(mx / (double)Size);
            int ytime = (int)Math.Ceiling(my / (double)Size);

            Piece ret = new Piece();
            for(int y = 0;ytime > y; y++)
            {
                for (int x = 0; xtime > x; x++)
                {
                    IList<Point> range = GetRange(p.Vertexes, Size, x, y);

                    if (range.Count > 0)
                        ret.Vertexes.Add(Average(range));
                }
            }

            return ret;
        }

        static IList<Point> GetRange(PointCollection Points, int Size, int X, int Y)
        {
            int Left = Size * X, Top = Size * Y, Right = Left + Size, Bottom = Top + Size;

            List<Point> range = new List<Point>();
            foreach(Point p in Points)
            {
                if(p.X >= Left && p.X <= Right && p.Y >= Top && p.Y <= Bottom)
                {
                    range.Add(p);
                }
            }

            return range;
        }

        static Point Average(IList<Point> Points)
        {
            Point ret = new Point();
            foreach(Point p in Points)
            {
                ret += p;
            }

            return ret / Points.Count;
        }
    }
}
