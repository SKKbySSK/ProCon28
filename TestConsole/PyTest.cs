using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace TestConsole
{
    class PyTest : IConsole
    {
        public PyTest()
        {
            System.IO.Directory.CreateDirectory("Python");
        }

        public string Title { get; } = "Pythonスクリプトの呼び出し";

        public void Run()
        {
            Console.WriteLine("[0] ファイル名指定, [1] HelloWorld.py");
            if (int.TryParse(Console.ReadLine(), out int res))
            {
                switch (res)
                {
                    case 0:
                        break;
                    case 1:
                        ScriptRuntime py = Python.CreateRuntime();
                        dynamic sc = py.UseFile(@"Python/HelloWorld.py");
                        var result = sc.Say();
                        Console.WriteLine(result);
                        break;
                }
            }
        }
    }
}
