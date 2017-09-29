using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;

namespace ProCon28.Algo
{
    class CompositePiece : Piece
    {
        public CompositePiece( List<Piece> S) : base()
        {
            Source = S;
        }
        public List<Piece> Source { get; set; }
    }
}
