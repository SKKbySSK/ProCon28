using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;
using ProCon28.Linker.Extensions;

namespace ProCon28.PieceEdit
{
    static class RatioConvert
    {
        public static Piece Run(Piece Piece, int From, int To, double Length)
        {
            Piece p = Piece.Convert();

            double act = p.Vertexes[From].GetLength(p.Vertexes[To]);

            double ratio = Length / act;

            for(int i = 0;p.Vertexes.Count > i; i++)
            {
                Point point = p.Vertexes[i];
                point = new Point((int)(point.X * ratio), (int)(point.Y * ratio));
                p.Vertexes[i] = point;
            }

            return p;
        }
    }
}
