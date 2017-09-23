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
                    if (GeneratePieces(QRShape).Length > 0)
                    {
                        shapes.Add(QRShape);
                        Console.WriteLine(QRShape);
                        return true;
                    }
                    else
                        return false;
                }
                catch (Exception) { return false; }
            }
            else
                return false;
        }

        public static Linker.Piece[] GeneratePieces(string QRShape)
        {
            int pcount = (int)char.GetNumericValue(QRShape[0]);

            string[] rawps = QRShape.Remove(0,2).Split(':');

            bool HasFrame = pcount + 1 == rawps.Length;

            Linker.Piece[] pieces = new Linker.Piece[HasFrame ? pcount + 1 : pcount];
            
            for(int i = 0;rawps.Length > i; i++)
            {
                string[] vs = rawps[i].Split(' ');
                int count = int.Parse(vs[0]) * 2;
                Linker.Piece p = (i == rawps.Length - 1 && HasFrame) ? new Linker.Frame() : new Linker.Piece();
                for (int j = 1; count > j; j += 2)
                {
                    Linker.Point point = new Linker.Point(int.Parse(vs[j]) * 4, int.Parse(vs[j + 1]) * 4);
                    p.Vertexes.Add(point);
                }
                pieces[i] = p;
            }

            return pieces.ToArray();
        }
    }
}
