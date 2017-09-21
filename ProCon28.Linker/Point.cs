﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker.Extensions;
using System.Runtime.Serialization;
using System.IO;

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

        public override string ToString()
        {
            return X + "," + Y;
        }
    }
    
    public class PointCollection : ObservableCollection<Point>
    {
        public PointCollection() { }
        public PointCollection(byte[] Bytes)
        {
            using(MemoryStream ms = new MemoryStream(Bytes))
            using(BinaryReader br = new BinaryReader(ms))
            {
                int count = br.ReadInt32();
                for(int i = 0;count > i; i++)
                {
                    Add(new Point(br.ReadInt32(), br.ReadInt32()));
                }
            }
        }

        public PointCollection Sort(PointSortation Sortation)
        {
            Point min = this[0];
            foreach (Point p in this)
            {
                if (min.X + min.Y > p.X + p.Y)
                    min = p;
            }
            return Sort(Sortation, min);
        }

        public PointCollection Sort(PointSortation Sortation, Point BasePoint)
        {
            PointCollection col = new PointCollection();
            col.AddRange(this.OrderBy(p => Math.Atan2(BasePoint.X - p.X, BasePoint.Y - p.Y)));

            if (Sortation == PointSortation.Clockwise)
            {
                IEnumerable<Point> reve = col.Reverse();
                PointCollection rev = new PointCollection();
                rev.AddRange(reve);
                return rev;
            }

            return col;
        }

        public byte[] AsBytes()
        {
            using(MemoryStream ms = new MemoryStream())
            using(BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(Count);
                foreach(Point p in this)
                {
                    bw.Write(p.X);
                    bw.Write(p.Y);
                }

                return ms.ToArray();
            }
        }
    }

    public class SortedPointCollection : IList<Point>
    {

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
