using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker.Tcp
{
    public class RemotePieces : MarshalByRefObject
    {
        public RemotePieces()
        {
            Created = DateTime.Now;
        }

        public DateTime Created { get; }
        public byte[] BytePieces { get; set; }
    }
}
