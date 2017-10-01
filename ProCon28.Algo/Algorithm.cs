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

        public Piece PieceBond(Piece Source1, Piece Source2, List<(int, int)> FitIndex)
        {
            Piece p = null;



            return p;
        }


        internal class LineJudge {

            public LineJudge(bool j,bool t, bool p1,bool p2)
            {
                IsJudge = j;
                IsTurn = t;
                IsPIPI1 = p1;
                IsPIPI2 = p2;
            }

            public bool IsJudge { get; }
            public bool IsTurn  { get; }
            public bool IsPIPI1 { get; }
            public bool IsPIPI2 { get; }
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
