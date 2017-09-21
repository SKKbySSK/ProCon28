﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static List<IConsole> commands = new List<IConsole>();

        static void Main(string[] args)
        {
            commands.Add(new TcpClient());

            while (true)
            {
                int i = 0;
                foreach (IConsole c in commands)
                {
                    Console.Write("[{0}] {1}", i, c.Title);
                    i++;
                }
                Console.Write("\n");

                string input = Console.ReadLine();
                if (int.TryParse(input, out int res))
                {
                    commands[res].Run();
                }
                else
                {
                    break;
                }
            }
        }
    }
}