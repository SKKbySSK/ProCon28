using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;
using ProCon28.Linker.Extensions;

namespace ProCon28.PieceEdit
{
    static class Straight
    {
        public static Piece Run(Piece Piece, double Threshold)
        {
            Piece p = Piece.Convert();

            List<Point> rem = new List<Point>();
            
            for(int i = 0;p.Vertexes.Count > i; i++)
            {
                double angle = p.GetAngle(i);
                if (Math.PI - angle <= Threshold)
                    rem.Add(p.Vertexes[i]);
            }

            foreach (Point point in rem)
                p.Vertexes.Remove(point);

            return p;
        }
    }
}
