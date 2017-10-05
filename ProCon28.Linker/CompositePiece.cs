using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker
{
    public class CompositePiece : Piece
    {
        public CompositePiece( IEnumerable<Point> P ,IEnumerable<Piece> S )
        {
            Source = new PieceCollection();
            foreach(var piece in S)
                Source.Add(piece);

            foreach (var v in P)
                Vertexes.Add(v);
        }

        public PieceCollection Source { get; set; }

        public override object Clone()
        {
            return base.Clone();
        }
    }
}
