using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker.Extensions;
using ProCon28.Linker;
using System.Collections;

namespace ProCon28.Algo.Line
{
    public class LineData
    {
        public LineData(Point Start, Point End, double Length, int LineNumber, int PieceNumber)
        {
            this.Start = Start;
            this.End = End;
            this.Length = Length;
            this.PieceNumber = PieceNumber;
            this.LineNumber = LineNumber;
            slope = Math.Tan( (End.Y - Start.Y) / (double)(End.X - Start.X));
        }

        public LineData(IList<(Point, Point, double)> Lines, int LineNumber, int PieceNumber)
        {
            var Line = Lines[LineNumber];

            Start = Line.Item1;
            End = Line.Item2;
            Length = Line.Item3;
            this.PieceNumber = PieceNumber;
            this.LineNumber = LineNumber;
            slope = Math.Tan((End.Y - Start.Y) / (double)(End.X - Start.X));
        }
        
        public Point Start { get; set; }
        public Point End { get; set; }
        public double Length { get; set; }
        public int PieceNumber { get; set; }
        public int LineNumber { get; set; }
        public double slope { get; set; }
        public bool Direction { get; set; }
    }

    public class SortedLineDataCollection : IList<LineData>
    {
        List<LineData> lines = new List<LineData>();

        public LineData this[int index] { get => ((IList<LineData>)lines)[index]; set => ((IList<LineData>)lines)[index] = value; }

        public int Count => ((IList<LineData>)lines).Count;

        public bool IsReadOnly => ((IList<LineData>)lines).IsReadOnly;

        public void Add(LineData item)
        {
            int c = Count;
            if (c == 0)
            {
                lines.Add(item);
                return;
            }
            else
            {
                for (int i = 0; c > i; i++)
                {
                    LineData ld = this[i];
                    if (ld.Length >= item.Length)
                    {
                        if (i == 0)
                        {
                            lines.Insert(0, item);
                            return;
                        }
                        else
                        {
                            lines.Insert(i, item);
                            return;
                        }
                    }
                }

                lines.Add(item);
            }
        }

        public void Clear()
        {
            ((IList<LineData>)lines).Clear();
        }

        public bool Contains(LineData item)
        {
            return ((IList<LineData>)lines).Contains(item);
        }

        public void CopyTo(LineData[] array, int arrayIndex)
        {
            ((IList<LineData>)lines).CopyTo(array, arrayIndex);
        }

        public IEnumerator<LineData> GetEnumerator()
        {
            return ((IList<LineData>)lines).GetEnumerator();
        }

        public int IndexOf(LineData item)
        {
            return ((IList<LineData>)lines).IndexOf(item);
        }

        public void Insert(int index, LineData item)
        {
            ((IList<LineData>)lines).Insert(index, item);
        }

        public bool Remove(LineData item)
        {
            return ((IList<LineData>)lines).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<LineData>)lines).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<LineData>)lines).GetEnumerator();
        }
    }
}
