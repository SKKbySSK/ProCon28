using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28
{
    static class Log
    {
        public static Windows.Main MainWindow { get; set; }

        public static void Write(object Format, params object[] Args)
        {
            string line = string.Format(Format.ToString(), Args);
            MainWindow?.Append(line);
        }
    }
}
