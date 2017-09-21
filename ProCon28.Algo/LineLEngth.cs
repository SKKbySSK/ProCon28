using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker.Extensions;
using ProCon28.Linker;

namespace ProCon28.Algo
{
    class LineLength
    {
        public (Point, Point, double) Line;
        public LineLength(PointCollection p ,int n)
        {
            ICollection<(Point,Point,double)> LineCollection = p.AsLinesWithLength();
            Line = LineCollection[n]
        }


    }
}
