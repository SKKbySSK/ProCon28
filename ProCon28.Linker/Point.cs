using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker.Extensions;

namespace ProCon28.Linker
{
    public struct Point
    {
        public Point(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }

    public class PointCollection : ObservableCollection<Point>
    {
        public PointCollection Sort(PointSortation Sortation)
        {
            PointCollection col = new PointCollection();
            col.AddRange(this.OrderBy(p => Math.Atan2(p.X, p.Y)));
            if (Sortation == PointSortation.AntiClockwise)
            {
                IEnumerable<Point> rev = col.Reverse();
                col.Clear();
                col.AddRange(rev);
            }

            return col;
        }
    }

    public enum PointSortation
    {
        /// <summary>
        /// 時計回り
        /// </summary>
        Clockwise,
        /// <summary>
        /// 反時計回り
        /// </summary>
        AntiClockwise
    }
}
