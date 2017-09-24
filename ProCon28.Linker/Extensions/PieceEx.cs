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
            double rad;

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
            rad = Math.Acos(calc);
            //角度が180以上かどうかの判定
            //ここからかなり不安な処理を行う、消す場合は次のコメントアウトまで
            double p1angle, p2angle, changing;
            if (p1.X == 0)
            {
                p1angle = Math.PI / 2;
                if (p1.Y < 0) { p1angle *= -1; }
            }
            else { p1angle = Math.Atan2(p1.Y, p1.X); }
            if (p2.X == 0)
            {
                p2angle = Math.PI / 2;
                if (p2.Y < 0) { p2angle *= -1; }
            }
            else { p2angle = Math.Atan2(p2.Y, p2.X); }
            if (Math.Abs(p2angle - p1angle) > Math.PI) { changing = p1angle; p1angle = p2angle; p2angle = changing; }
            bool iscalc = true;
            double crad = 0;
            while (iscalc)
            {
                iscalc = false;
                Random rand = new Random();
                crad = rand.NextDouble();
                crad = p1angle + crad * (p2angle - p1angle);
                for (int i = 0; i < vcount; i++)
                {
                    if (Vertex == i) { continue; }
                    if (Math.Atan2(Piece.Vertexes[i].Y - Piece.Vertexes[Vertex].Y, Piece.Vertexes[i].X - Piece.Vertexes[Vertex].X)== crad) { iscalc = true; }
                }
            }

            double cp1x = angle.X + Math.Cos(crad), cp1y = angle.Y + Math.Sin(crad);
            double cp3x = angle.X, cp3y = angle.Y;
            Point cp2, cp4;
            double cp2x, cp2y, cp4x, cp4y;
            int count = 0;
            for(int i = 0; i< vcount; i++)
            {
                if (i == Vertex || i == Vertex + 1 || (Vertex == vcount - 1 && i == 0))
                {
                    continue;
                }
                else if( i == 0)
                {
                    cp2 = Piece.Vertexes[vcount - 1];
                    cp4 = Piece.Vertexes[0];
                }
                else
                {
                    cp2 = Piece.Vertexes[i - 1];
                    cp4 = Piece.Vertexes[i];
                }
                cp2x = cp2.Y;
                cp2y = cp2.Y;
                cp4x = cp4.X;
                cp4y = cp4.Y;
                double s1 = ((cp4x - cp2x) * (cp1y - cp2y) - (cp4y - cp2y) * (cp1x - cp2x)) / 2;
                double s2 = ((cp4x - cp2x) * (cp2y - cp3y) - (cp4y - cp2y) * (cp2x - cp3x)) / 2;
                double crossx = cp1x + (cp3x - cp1x) * s1 / (s1 + s2);
                double crossy = cp1y + (cp3y - cp1y) * s1 / (s1 + s2);
                if(cp2x == cp4x)
                {
                    if(cp2y < cp4y)
                    {
                        if(cp2y < crossy && cp4y > crossy) { count++; }
                    }
                    else
                    {
                        if(cp4y < crossy && cp2y > crossy) { count++; }
                    }
                }
                else if (cp2x < cp4x)
                {
                    if(cp2x < crossx && cp4x > crossx) { count++; }
                }
                else
                {
                    if(cp4x < crossx && cp2x > crossx) { count++; }
                }
            }
            if(count %2 == 0) { rad += Math.PI; }

            //180度以上を判定する処理ここまで

            return rad;
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
            if (Piece.Vertexes.Count == 0) return Piece;
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
