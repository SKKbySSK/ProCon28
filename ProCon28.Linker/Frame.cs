using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker
{
    public class Frame : Piece
    {
        public Frame() { }
        public Frame(Piece Piece)
        {
            foreach (Point point in Piece.Vertexes)
                Vertexes.Add(point);
        }

        public override object Clone()
        {
            Frame Frame = new Frame();
            foreach (Point point in Vertexes)
                Frame.Vertexes.Add(point);
            return Frame;
        }
    }
}
