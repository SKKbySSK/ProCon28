using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

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
            if (Vertex == 0)
            {
                p1 = Piece.Vertexes[vcount - 1];
                p2 = Piece.Vertexes[1];
            }
            else if (Vertex + 1 == vcount)
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
            rad = MathEx.Acos(calc);
            return rad;
        }

        public static double GetAngle2PI(this Piece Piece, int Vertex)
        {
            Point p1, p2;
            Point angle = Piece.Vertexes[Vertex];

            int vcount = Piece.Vertexes.Count;
            if (Vertex == 0)
            {
                p1 = Piece.Vertexes[vcount - 1];
                p2 = Piece.Vertexes[1];
            }
            else if (Vertex + 1 == vcount)
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

            double rad = GetAngle(Piece, Vertex);
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
            double crad = p1angle + Math.PI / 10 * (p2angle - p1angle);
            double cp1x = angle.X + Math.Cos(crad), cp1y = angle.Y + Math.Sin(crad);
            double cp3x = angle.X, cp3y = angle.Y;
            Point cp2, cp4;
            double cp2x, cp2y, cp4x, cp4y;
            int count = 0;
            for (int i = 0; i < vcount; i++)
            {
                if (i == Vertex || i == Vertex + 1 || (Vertex == vcount - 1 && i == 0))
                {
                    continue;
                }
                else if (i == 0)
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
                if (cp2x == cp4x)
                {
                    if (cp2y < cp4y)
                    {
                        if (cp2y < crossy && cp4y > crossy) { count++; }
                    }
                    else
                    {
                        if (cp4y < crossy && cp2y > crossy) { count++; }
                    }
                }
                else if (cp2x < cp4x)
                {
                    if (cp2x < crossx && cp4x > crossx) { count++; }
                }
                else
                {
                    if (cp4x < crossx && cp2x > crossx) { count++; }
                }
            }
            if (count % 2 == 0) { rad += Math.PI; }

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
            if (Piece is CompositePiece cp)
            {
                PieceCollection newSource = new PieceCollection();
                newSource.AddRange(cp.Source.Select(piece => piece.Convert(BasePoint)));
                cp.Source = newSource;
            }

            Piece clone = (Piece)Piece.Clone();
            for (int i = 0; clone.Vertexes.Count > i; i++)
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
            foreach (Point p in Piece.Vertexes)
            {
                if (bp.X > p.X)
                    bp.X = p.X;
                if (bp.Y > p.Y)
                    bp.Y = p.Y;
            }
            return Convert(Piece, bp);
        }

        public static Piece RotatePiece(this Piece Piece, double rad)
        {
            return UnsafeRotate(Piece, rad);


            if (Piece is CompositePiece cp)
            {
                PieceCollection newSource = new PieceCollection();
                newSource.AddRange(cp.Source.Select(piece => piece.RotatePiece(rad)));
                cp.Source = newSource;
            }


            Piece p = (Piece)Piece.Clone();
            PointCollection pcol = p.Vertexes;
            bool change = false;
            for (int i = 0; i < pcol.Count; i++)
            {
                double retX, retY;
                retX = pcol[i].X * Math.Cos(rad) - pcol[i].Y * Math.Sin(rad);
                retY = pcol[i].X * Math.Sin(rad) + pcol[i].Y * Math.Cos(rad);
                if (Math.Abs(retX - pcol[i].X) > 0.001 || Math.Abs(retY - pcol[i].Y) > 0.001)
                {
                    change = true;
                }
            }
            p = p.Convert();
            if (change) { p = Piece; }
            return p;
        }

        public static Piece UnsafeRotate(this Piece Piece, double Radian)
        {
            if (Piece is CompositePiece cp)
            {
                PieceCollection newSource = new PieceCollection();
                newSource.AddRange(cp.Source.Select(piece => piece.UnsafeRotate(Radian)));
                cp.Source = newSource;
            }

            Piece p = (Piece)Piece.Clone();

            PointCollection pcol = new PointCollection();
            foreach(var point in p.Vertexes)
            {
                double retX, retY;
                retX = point.X * Math.Cos(Radian) - point.Y * Math.Sin(Radian);
                retY = point.X * Math.Sin(Radian) + point.Y * Math.Cos(Radian);

                pcol.Add(new Point((int)Math.Round(retX), (int)Math.Round(retY)));
            }
            p.Vertexes.Clear();
            p.Vertexes.AddRange(pcol);
            p = p.Convert();
            return p;
        }

        public static Piece FlipPiece(this Piece Piece)
        {
            if(Piece is CompositePiece cp)
            {
                PieceCollection newSource = new PieceCollection();
                newSource.AddRange(cp.Source.Select(piece => piece.FlipPiece()));
                cp.Source = newSource;
            }

            Piece p = (Piece)Piece.Clone();
            for (int i = 0; i < p.Vertexes.Count; i++)
            {
                Point clone = p.Vertexes[i];
                clone.X *= -1;
                clone.Y *= -1;
                p.Vertexes[i] = clone;
            }
            p = p.Convert();
            return p;
        }
        
        class LineConnection
        {
            public int Index1_0;
            public int Index1_1;
            public int Index2_0;
            public int Index2_1;
            public Point X1;
            public Point X2;
            public Point Y1;
            public Point Y2;
            public double Length1;
            public double Length2;
            public double Difference;
        }

        public static bool IsDirectionJudge(this Piece Piece, int index) 
        {
            bool r;
            IList<(Point, Point, double)> LL = Piece.Vertexes.AsLinesWithLength();
            Point S = LL[index].Item1;
            Point E = LL[index].Item2;
            double sx = S.X, sy = S.Y;
            double ex = E.X, ey = E.Y;
            double cp1x = (sx + ex * Math.PI) / (Math.PI + 1); //ランダムな浮動小数点の代用としてPIを使用
            double cp1y = (sy + ey * Math.PI) / (Math.PI + 1); //ランダムでは遅くちょっと不味いことが起こるやもしれないため
            double cp3x, cp3y;
            if (sx != cp1x)
            {
                cp3x = cp1x + 1;
                cp3y = -(sx - cp1x) * (cp3x - cp1x) / (sy - cp1y) + cp1y;
            }
            else
            {
                cp3x = cp1x;
                cp3y = cp1y + 1;
            }

            int count = 0;
            for(int i = 0; i < LL.Count; i++)
            {
                if(i == index) { continue; }
                double cp2x = LL[i].Item1.X;
                double cp2y = LL[i].Item1.Y;
                double cp4x = LL[i].Item2.X;
                double cp4y = LL[i].Item2.Y;

                double s1 = ((cp4x - cp2x) * (cp1y - cp2y) - (cp4y - cp2y) * (cp1x - cp2x)) / 2;
                double s2 = ((cp4x - cp2x) * (cp2y - cp3y) - (cp4y - cp2y) * (cp2x - cp3x)) / 2;
                double crossx = cp1x + (cp3x - cp1x) * s1 / (s1 + s2);
                double crossy = cp1y + (cp3y - cp1y) * s1 / (s1 + s2);
                if (cp2x == cp4x)
                {
                    if (cp2y < cp4y)
                    {
                        if (cp2y < crossy && cp4y > crossy) { count++; }
                    }
                    else
                    {
                        if (cp4y < crossy && cp2y > crossy) { count++; }
                    }
                }
                else if (cp2x < cp4x)
                {
                    if (cp2x < crossx && cp4x > crossx) { count++; }
                }
                else
                {
                    if (cp4x < crossx && cp2x > crossx) { count++; }
                }
            }

            if(count % 2 == 1)
                r = true;
            else
                r = false;
            return r;
        }

        public static List<(double, bool)> PieceSideData(this Piece Piece)
        {
            List<(double, bool)> r = new List<(double, bool)>();
            IList<(Point, Point)> Line = Piece.Vertexes.AsLines();
            for(int i = 0;i < Piece.Vertexes.Count; i++)
            {
                double Slope = Math.Atan( (Line[i].Item2.Y - Line[i].Item1.Y) / (double)(Line[i].Item2.X - Line[i].Item1.X));
                Slope = double.IsNaN(Slope) ? Math.PI / 2 : Slope;
                bool IsDirect = Piece.IsDirectionJudge(i);
                r.Add((Slope, IsDirect));
            }
            return r;
        }

        public static Piece PrimaryConversion(this Piece Piece, double m11, double m12, double m21, double m22)
        {
            Piece p = (Piece)Piece.Clone();
            Piece a = p;
            PointCollection c = p.Vertexes;
            bool change = false;
            for (int i = 0; i < c.Count; i++)
            {
                double UpdateX = m11 * c[i].X + m12 * c[i].Y;
                double UpdateY = m21 * c[i].X + m22 * c[i].Y;
                Point UpdatePoint = new Point((int)UpdateX, (int)UpdateY);
                c[i] = UpdatePoint;
                if (Math.Abs(UpdateX - Math.Truncate(UpdateX)) > 0.0001 || Math.Abs(UpdateY - Math.Truncate(UpdateY)) > 0.0001)
                {
                    change = true;
                }
            }
            if (change) { p = a; }

            return p;
        }
    }
}
