using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker.Extensions
{
    public static class PieceEx
    {
        public static double GetAngle(this Piece Piece, int Vertex)
        {
            if (Piece.Vertexes.Count < 3)
                return double.NaN;

            Point p1, p2;
            Point angle = Piece.Vertexes[Vertex];

            int vcount = Piece.Vertexes.Count;
            if(Vertex == 0)
            {
                p1 = Piece.Vertexes[vcount - 1];
                p2 = Piece.Vertexes[1];
            }
            else if(Vertex + 1 == vcount)
            {
                p1 = Piece.Vertexes[Vertex - 1];
                p2 = Piece.Vertexes[0];
            }
            else
            {
                p1 = Piece.Vertexes[Vertex - 1];
                p2 = Piece.Vertexes[Vertex + 1];
            }

            p1 -= angle;
            p2 -= angle;
            double len1 = p1.GetLength(), len2 = p2.GetLength();
            double calc = ((p1.X * p2.X) + (p1.Y * p2.Y)) / (len1 * len2);
            return Math.Acos(calc);
        }

        /// <summary>
        /// ピースの座標を指定した基準点を元に変換します。
        /// </summary>
        /// <param name="Piece"></param>
        /// <param name="BasePoint">基準となる点</param>
        /// <returns></returns>
        public static Piece Convert(this Piece Piece, Point BasePoint)
        {
            Piece clone = (Piece)Piece.Clone();
            for(int i = 0;clone.Vertexes.Count > i; i++)
            {
                Point p = clone.Vertexes[i];
                p.X -= BasePoint.X;
                p.Y -= BasePoint.Y;
                clone.Vertexes[i] = p;
            }
            return clone;
        }

        public static Piece Convert(this Piece Piece)
        {
            Point bp = Piece.Vertexes[0];
            foreach(Point p in Piece.Vertexes)
            {
                if (bp.X > p.X)
                    bp.X = p.X;
                if (bp.Y > p.Y)
                    bp.Y = p.Y;
            }
            return Convert(Piece, bp);
        }
    }
}
