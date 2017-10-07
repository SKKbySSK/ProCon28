﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker
{
    public class CompositePiece : Piece
    {
        private CompositePiece() { }
        public CompositePiece( IEnumerable<Point> P ,IEnumerable<Piece> S )
        {
            Source = new PieceCollection();
            foreach(var piece in S)
                Source.Add(piece);

            foreach (var v in P)
                Vertexes.Add(v);
        }

        public IList<Piece> Source { get; set; }

        public override object Clone()
        {
            CompositePiece cp = new CompositePiece();
            PieceCollection pcol = new PieceCollection();
            foreach (var p in Source)
                pcol.Add((Piece)p.Clone());
            cp.Source = pcol;

            foreach (var p in Vertexes)
                cp.Vertexes.Add(p);

            return cp;
        }
    }

    
}
