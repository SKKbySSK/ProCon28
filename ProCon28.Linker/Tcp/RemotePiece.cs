using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker.Tcp
{
    public class RemotePiece : MarshalByRefObject
    {
        public RemotePiece(byte[] Piece)
        {
            RawPiece = Piece;
        }

        public byte[] RawPiece { get; }
    }
}
