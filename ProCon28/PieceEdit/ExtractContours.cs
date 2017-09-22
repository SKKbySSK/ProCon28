using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;
using ProCon28.Linker.Extensions;

namespace ProCon28.PieceEdit
{
    static class ExtractContours
    {
        public static Piece Run(Piece Piece)
        {
            Piece p = Piece.Convert();

            int mx = 0, my = 0;
            foreach (Point point in p.Vertexes)
            {
                mx = point.X > mx ? point.X : mx;
                my = point.Y > my ? point.Y : my;
            }

            Piece ret = new Piece();
            //縦方向
            for(int i = 0;mx >= i; i++)
            {
                List<Point> vp = GetVerticalStraight(p, i);

                if(vp.Count > 2)
                {
                    vp.RemoveRange(1, vp.Count - 2);
                    ret.Vertexes.AddRange(vp);
                }
            }

            //横方向            
            for (int i = 0;my >= i; i++)
            {
                List<Point> hp = GetHorizontalStraight(p, i);

                if (hp.Count > 2)
                {
                    hp.RemoveRange(1, hp.Count - 2);
                    ret.Vertexes.AddRange(hp);
                }
            }

            return ret;
        }

        static List<Point> GetVerticalStraight(Piece Piece, int X)
        {
            List<Point> points = new List<Point>();
            foreach(Point p in Piece.Vertexes)
            {
                if (p.X == X)
                    points.Add(p);
            }

            return points.OrderBy(p => p.Y).ToList();
        }

        static List<Point> GetHorizontalStraight(Piece Piece, int Y)
        {
            List<Point> points = new List<Point>();
            foreach (Point p in Piece.Vertexes)
            {
                if (p.Y == Y)
                    points.Add(p);
            }

            return points.OrderBy(p => p.X).ToList();
        }
    }
}
