using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;

namespace ProCon28.Algo
{
    class MTProcessor
    {
        PieceCollection PieceCollection;
        List<AlgoInfo> Infos = new List<AlgoInfo>();

        public MTProcessor(PieceCollection Pieces)
        {

        }

        PieceCollection ClonePieces()
        {
            PieceCollection pcol = new PieceCollection();
            foreach (var p in PieceCollection)
                pcol.Add((Piece)p.Clone());
            return pcol;
        }
    }

    static class CompletelyRandom
    {
        public static int Next(int Min, int Max) { return new Random(Convert.ToInt32(Guid.NewGuid().ToString("N").Substring(0, 8), 16)).Next(Min, Max); }
    }
}
