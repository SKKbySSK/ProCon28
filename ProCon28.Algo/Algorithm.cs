using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;
using ProCon28.Algo.Line;
using ProCon28.Linker.Extensions;

namespace ProCon28.Algo
{
    public class Algorithm
    {
        PieceCollection PieceCollection;
        SortedLineDataCollection LineList = new SortedLineDataCollection();
        public Algorithm(PieceCollection pcol)
        {
            PieceCollection = pcol;
            for (int i = 0; i < pcol.Count; i++)
            {
                PointCollection PointCol = pcol[i].Vertexes;
                IList<(Point, Point, double)> pl = PointCol.AsLinesWithLength();
                for (int j = 0; j < PointCol.Count; j++)
                {
                    LineList.Add(new LineData(pl, j, i));
                }
            }
        }

        public void SearchCanBondPiecePair()
        {
            while (true)
            {
                List<List<(int, int)>> Judge = new List<List<(int, int)>>();

                Random rand = new Random();
                int index = rand.Next(LineList.Count);
                LineData Line1 = LineList[index];
                int index2 = index;
                bool UpDown = true;
                while (UpDown)
                {
                    index2++;
                    if (index2 > LineList.Count) { UpDown = false; }
                    LineData Line2 = LineList[index2];
                    if (Rounding(Line1.Length, Line2.Length))
                    {
                        PiecePair pp = new PiecePair(PieceCollection[Line1.PieceNumber],Line1.LineNumber, PieceCollection[Line2.PieceNumber], Line2.LineNumber, true);
                        List<(int, int)> ppResult = pp.MainJudge();
                        if (ppResult[0].Item1 != -1) { Judge.Add(ppResult); }
                        pp = new PiecePair(PieceCollection[Line1.PieceNumber], Line1.LineNumber, PieceCollection[Line2.PieceNumber], Line2.LineNumber, false);
                        ppResult = pp.MainJudge();
                        if (ppResult[0].Item1 != -1) { Judge.Add(ppResult); }

                    }
                    else
                    {
                        UpDown = false;
                    }
                }
                index2 = index;
                UpDown = true;
                while (UpDown)
                {
                    index2--;
                    if (index2 < 0) { UpDown = false; }
                    LineData Line2 = LineList[index2];
                    if (Rounding(Line1.Length , Line2.Length))
                    {
                        PiecePair pp = new PiecePair(PieceCollection[Line1.PieceNumber], Line1.LineNumber, PieceCollection[Line2.PieceNumber], Line2.LineNumber, true);
                        List<(int, int)> ppResult = pp.MainJudge();
                        if (ppResult[0].Item1 != -1) { Judge.Add(ppResult); }
                        pp = new PiecePair(PieceCollection[Line1.PieceNumber], Line1.LineNumber, PieceCollection[Line2.PieceNumber], Line2.LineNumber, false);
                        ppResult = pp.MainJudge();
                        if (ppResult[0].Item1 != -1) { Judge.Add(ppResult); }
                    }
                    else
                    {
                        UpDown = false;
                    }
                }

            }
        }

        public CompositePiece PieceBond(Piece Source1, Piece Source2, List<(int, int)> FitIndex)
        {
            CompositePiece p = null;
            List<(double, bool)> Side1 = Source1.PieceSideData();
            List<(double, bool)> Side2 = Source2.PieceSideData();
            bool Turn ;
            int sideIndex1, sideIndex2;
            int disIndex1 = FitIndex[0].Item1 - FitIndex[1].Item1;
            int disIndex2 = FitIndex[0].Item2 - FitIndex[0].Item2;
            if (Math.Abs(disIndex1) != 1) { disIndex1 *= -1; }
            if (Math.Abs(disIndex2) != 1) { disIndex2 *= -1; }
            if (disIndex1 * disIndex2 > 0)
                Turn = false;
            else
                Turn = true;
            if(Turn)
                Source2 = Source2.FlipPiece();

            List<(double, bool)> sl1 = Source1.PieceSideData();
            List<(double, bool)> sl2 = Source2.PieceSideData();
            if (disIndex1 > 0)
                sideIndex1 = FitIndex[0].Item1;
            else
                sideIndex1 = FitIndex[1].Item1;
            if (disIndex2 > 0)
                sideIndex2 = FitIndex[0].Item2;
            else
                sideIndex2 = FitIndex[1].Item2;
            double Slope1 = sl1[sideIndex1].Item1;
            double Slope2 = sl2[sideIndex2].Item1;
            double disSlope = Slope1 - Slope2;
            Source2 = Source2.RotatePiece(disSlope);

            bool Dir1 = sl1[sideIndex1].Item2;
            bool Dir2 = sl1[sideIndex2].Item2;
            if (Dir1 == Dir2)
                Source2 = Source2.RotatePiece(Math.PI);

            Point Move = Source2.Vertexes[FitIndex[0].Item2] - Source1.Vertexes[FitIndex[0].Item1];
            Source2 = Source2.Convert(Move);
            

            return p;
        }

        public bool Rounding(double Value1, double Value2)
        {
            bool r;
            if (Math.Abs(Value1 - Value2) < 0.001)
            {
                r = true;
            }
            else
            {
                r = false;
            }
            return r;
        }
    }
}
