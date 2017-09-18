using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker.Extensions
{
    public static class PieceEx
    {
        public static float GetAngle(this Piece Piece, int Vertex)
        {
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

            float len1 = p1.GetLength(angle), len2 = p2.GetLength(angle);
            return (float)Math.Acos(((p1.X * p2.X + p1.Y * p2.Y)) / (len1 * len2));
        }
    }
}
