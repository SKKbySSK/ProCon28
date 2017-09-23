using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;
using ProCon28.Linker.Extensions;

namespace ProCon28.PieceEdit
{
    public class GcdConvert
    {
        public static Piece Run(Piece Piece)
        {
            if (Piece.Vertexes.Count < 2)
                return Piece;

            var lines = Piece.Vertexes.AsLinesWithLength();

            List<double> lengths = new List<double>();
            foreach (var line in lines)
                lengths.Add(line.Item3);

            double gcd = MathEx.Gcd(lengths.ToArray());

            Piece = Straight.Run(Piece, 0);

            Piece bk = new Piece();
            for (int i = 0; Piece.Vertexes.Count > i; i++)
            {
                Point p = Piece.Vertexes[i];
                if (!bk.Vertexes.Contains(p))
                    bk.Vertexes.Add(p);
            }
            Piece = bk;

            return RatioConvert.Run(Piece, 0, 1, gcd);
        }
    }
}
