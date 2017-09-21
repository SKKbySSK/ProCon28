using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker.Extensions;
using ProCon28.Linker;

namespace ProCon28.Algo
{
    public class LineLength
    {

        public LineLength(int PointN, int PieceN)
        {
            PieceCollection col = new PieceCollection();
            Piece aPiece = col[PieceN];
            IList<(Point,Point,double)> LineCollection = aPiece.Vertexes.AsLinesWithLength();
            Line = LineCollection[PointN];

            Length = Line.Item3;
            PieceNumber = PieceN;
            PointNumber = PointN;
        }

        public (Point, Point, double) Line { get; set; }
        double Length { get; set; }
        int PieceNumber { get; set; }
        int PointNumber { get; set; }
    }

    public class LineLengthCollection : ObservableCollection<LineLength>
    {
        public LineLengthCollection() { }

    }

}
