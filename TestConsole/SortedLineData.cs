using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class SortedLineData : IConsole
    {
        public string Title { get; } = "SortedLineDataテスト";

        public void Run()
        {
            Console.WriteLine("要素数を入力");
            if(int.TryParse(Console.ReadLine(), out int count))
            {
                ProCon28.Algo.Line.SortedLineDataCollection col = new ProCon28.Algo.Line.SortedLineDataCollection();

                Console.WriteLine("入力データ");
                for(int i = 0;count > i; i++)
                {
                    ProCon28.Algo.Line.LineData data = new ProCon28.Algo.Line.LineData(new ProCon28.Linker.Point(), new ProCon28.Linker.Point(),
                        new Random(Environment.TickCount + i).Next(1, 300), 0, 0);
                    Console.WriteLine(data.Length);
                    col.Add(data);
                }

                Console.WriteLine();
                Console.WriteLine("出力データ");
                foreach(var ld in col)
                {
                    Console.WriteLine(ld.Length);
                }
            }
        }
    }
}
