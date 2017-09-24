using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;
using ProCon28.Linker.Tcp;

namespace TestConsole
{
    class TcpServer : IConsole
    {
        public string Title { get; } = "Tcp Serverテスト";
        public void Run()
        {
            Console.WriteLine("ポート番号(デフォルト:50001)");

            int port;
            if (!int.TryParse(Console.ReadLine(), out port))
            {
                port = 50001;
            }

            Piece p = new Piece();
            p.Rotation = 20;
            p.Vertexes.Add(new Point(0, 1));
            p.Vertexes.Add(new Point(10, 50));
            Server server = new Server(port, new RemotePiece(p.AsBytes()), Constants.RemoteRecognizerUri);
            server.Marshal();
            Console.WriteLine(server.ChannelUri);
            Console.ReadKey();
            server.Dispose();
        }
    }
}
