﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;
using ProCon28.Linker.Tcp;

namespace TestConsole
{
    class TcpClient : IConsole
    {
        public string Title { get; } = "TCP Clientテスト";

        public void Run()
        {
            Console.WriteLine("IPアドレス");
            string ip = Console.ReadLine().Replace("\n", "").Replace("\r", "");
            Console.WriteLine("ポート番号(デフォルト:50000)");
            int port = int.Parse(Console.ReadLine());

            Console.WriteLine("接続アドレスの指定方法\n[0] 手動, [1] リモートピース");

            if(int.TryParse(Console.ReadLine(), out int res))
            {
                switch (res)
                {
                    case 0:
                        break;
                    case 1:
                        Client client = new Client(ip, port, Constants.RemotePiecesUri);
                        RemotePieces rp = (RemotePieces)client.GetObject(typeof(RemotePieces));

                        Console.WriteLine("Date : " + rp.Created);
                        PieceCollection col = new PieceCollection(rp.BytePieces);
                        foreach (Piece p in col)
                        {
                            Console.WriteLine("Piece - Vertex:{0}, Rotation:{1}", p.Vertexes.Count, p.Rotation);
                            foreach(Point point in p.Vertexes)
                            {
                                Console.WriteLine("[{0}]", point);
                            }
                        }
                        break;
                }
            }
        }
    }
}