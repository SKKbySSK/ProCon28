using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker.Temp;
using ProCon28.Linker;
using System.IO;

namespace TestConsole
{
    class Tester : IConsole
    {
        public string Title { get; } = "Test";

        public void Run()
        {
            PieceCollection pcol = null;
            using (FileStream fs = new FileStream("Pieces.pbin", FileMode.Open, FileAccess.Read))
            {
                byte[] bs = new byte[fs.Length];
                fs.Read(bs, 0, (int)fs.Length);
                pcol = new PieceCollection(bs);
            }

            Algorithm algo = new Algorithm(pcol);
            algo.Run();
        }
    }
}
