using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker.Extensions;

namespace ProCon28
{
    static class ShapeQRManager
    {
        static List<string> shapes = new List<string>();
        public static string[] GetShapes()
        {
            return shapes.ToArray();
        }

        public static bool AddShape(string QRShape)
        {
            if (!shapes.Contains(QRShape))
            {
                try
                {
                    GeneratePieces(QRShape, out _);
                    shapes.Add(QRShape);
                    Console.WriteLine(QRShape);
                    return true;
                }
                catch (Exception) { return false; }
            }
            else
                return false;
        }

        public static Linker.Piece[] GeneratePieces(string QRShape, out Linker.Frame Frame)
        {
            Frame = null;
            Linker.Piece[] pieces = new Linker.Piece[((int)char.GetNumericValue(QRShape[0]))];

            string[] rawps = QRShape.Remove(0,2).Split(':');

            int pind = 0;
            foreach(string rpiece in rawps)
            {
                string[] vs = rpiece.Split(' ');
                int count = int.Parse(vs[0]) * 2;
                Linker.Piece p = new Linker.Piece();
                for (int i = 1; count > i; i += 2)
                {
                    Linker.Point point = new Linker.Point(int.Parse(vs[i]), int.Parse(vs[i + 1]));
                    p.Vertexes.Add(point);
                }
                if (pind == pieces.Length)
                {
                    Frame = new Linker.Frame(p);
                    pind++;
                }
                else
                {
                    pieces[pind] = p;
                    pind++;
                }
            }

            return pieces.ToArray();
        }
    }
}
