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

        public static void Reset()
        {
            shapes.Clear();
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
                        Log.Write("QR : " + QRShape);
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
            try
            {
                string[] rawps = QRShape.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                int pcount = int.Parse(rawps[0]);

                bool HasFrame = pcount + 2 == rawps.Length;

                Linker.Piece[] pieces = new Linker.Piece[HasFrame ? pcount + 1 : pcount];

                for (int i = 1; rawps.Length > i; i++)
                {
                    string[] vs = rawps[i].Split(' ');
                    int count = int.Parse(vs[0]) * 2;
                    Linker.Piece p = (i == rawps.Length - 1 && HasFrame) ? new Linker.Frame() : new Linker.Piece();
                    for (int j = 1; count > j; j += 2)
                    {
                        Linker.Point point = new Linker.Point(int.Parse(vs[j]), int.Parse(vs[j + 1]));
                        p.Vertexes.Add(point);
                    }
                    pieces[i - 1] = p;
                }

                return pieces;
            }
            catch(Exception)
            {
                return new Linker.Piece[0];
            }
        }
    }
}
