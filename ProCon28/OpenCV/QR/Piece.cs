using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.OpenCV.QR
{
    class Piece : Linker.Piece
    {
        public Piece() : base(new FixedPointCollection()) { }
    }

    public class FixedPointCollection : Linker.PointCollection
    {

    }
}
