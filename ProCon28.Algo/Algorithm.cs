﻿using System;
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
        PieceCollection NonRemove = new PieceCollection();
        SortedLineDataCollection LineList = new SortedLineDataCollection();
        public Algorithm(PieceCollection pcol)
        {
            PieceCollection = pcol;
            NonRemove = pcol;
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
            List<Piece> Source = new List<Piece>();
            Source.Add(Source1);
            Source.Add(Source2);
            Piece Piece1 = (Piece)Source1.Clone();
            Piece Piece2 = (Piece)Source2.Clone();
            int sideIndex1, sideIndex2;
            int disIndex1 = FitIndex[0].Item1 - FitIndex[1].Item1;
            int disIndex2 = FitIndex[0].Item2 - FitIndex[1].Item2;
            if (Math.Abs(disIndex1) != 1) { disIndex1 *= -1; }
            if (Math.Abs(disIndex2) != 1) { disIndex2 *= -1; }
            if (disIndex1 > 0)
                sideIndex1 = FitIndex[0].Item1;
            else
                sideIndex1 = FitIndex[1].Item1;

            if (disIndex2 > 0)
                sideIndex2 = FitIndex[0].Item2;
            else
                sideIndex2 = FitIndex[1].Item2;

            List<(double, bool)> sl1 = Piece1.PieceSideData();
            List<(double, bool)> sl2 = Piece2.PieceSideData();
            double Slope1 = sl1[sideIndex1].Item1;
            double Slope2 = sl2[sideIndex2].Item1;
            double disSlope = Slope1 - Slope2;
            Piece2 = Piece2.RotatePiece(disSlope);

            sl1 = Piece1.PieceSideData();
            sl2 = Piece2.PieceSideData();
            bool Dir1 = sl1[sideIndex1].Item2;
            bool Dir2 = sl1[sideIndex2].Item2;
            if (Dir1 == Dir2)
                Piece2 = Piece2.RotatePiece(Math.PI);

            Point Move = Piece2.Vertexes[FitIndex[0].Item2] - Piece1.Vertexes[FitIndex[0].Item1];
            Piece2 = Piece2.Convert(Move);

            if (Piece1.Vertexes[FitIndex[1].Item1] != Piece2.Vertexes[FitIndex[1].Item2])
            {
                Piece2 = Piece2.FlipPiece();

                sl1 = Piece1.PieceSideData();
                sl2 = Piece2.PieceSideData();
                Slope1 = sl1[sideIndex1].Item1;
                Slope2 = sl2[sideIndex2].Item1;
                disSlope = Slope1 - Slope2;
                Piece2 = Piece2.RotatePiece(disSlope);

                sl1 = Piece1.PieceSideData();
                sl2 = Piece2.PieceSideData();
                Dir1 = sl1[sideIndex1].Item2;
                Dir2 = sl1[sideIndex2].Item2;
                if (Dir1 == Dir2)
                    Piece2 = Piece2.RotatePiece(Math.PI);

                Move = Piece2.Vertexes[FitIndex[0].Item2] - Piece1.Vertexes[FitIndex[0].Item1];
                Piece2 = Piece2.Convert(Move);
            }


            int Start1 = FitIndex[0].Item1;
            int End1 = FitIndex[FitIndex.Count - 1].Item1;
            int Start2 = FitIndex[FitIndex.Count - 1].Item2;
            int End2 = FitIndex[0].Item2;
            bool rot1, rot2;
            if (disIndex1 > 0)
                rot1 = true;
            else
                rot1 = false;
            if (disIndex2 > 0)
                rot2 = false;
            else
                rot2 = true;

            List<Point> Return = new List<Point>();
            if (rot1)
            {
                if (Start1 > End1)
                    End1 += Piece1.Vertexes.Count;
                for(int i = Start1; i <= End1; i++)
                {
                    int ri = i % Piece1.Vertexes.Count;
                    Return.Add(Piece1.Vertexes[ri]);
                }
            }
            else
            {
                if (Start1 < End1)
                    End1 -= Piece1.Vertexes.Count;
                for(int i = Start1; i >= End1; i--)
                {
                    int ri = i;
                    if (ri < 0)
                        ri += Piece1.Vertexes.Count;
                    Return.Add(Piece1.Vertexes[ri]);
                }
            }
            if (rot2)
            {
                if (Start2 > End1)
                    End2 += Piece2.Vertexes.Count;
                for (int i = Start2; i <= End2; i++)
                {
                    int ri = i % Piece2.Vertexes.Count;
                    Return.Add(Piece2.Vertexes[ri]);
                }
            }
            else
            {
                if (Start2 < End2)
                    End2 -= Piece2.Vertexes.Count;
                for (int i = Start2; i >= End1; i--)
                {
                    int ri = i;
                    if (ri < 0)
                        ri += Piece2.Vertexes.Count;
                    Return.Add(Piece2.Vertexes[ri]);
                }
            }
            Piece rt = new Piece();
            rt.Vertexes.AddRange(Return);
            for(int i = 0; i < Return.Count; i++)
            {
                if( Rounding(rt.GetAngle(i), Math.PI))
                {
                    rt.Vertexes.RemoveAt(i);
                    i--;
                }
            }
            return new CompositePiece(rt.Vertexes, Source);
        }

        public bool Rounding(double Value1, double Value2)
        {
            return Math.Abs(Value1 - Value2) < Config.Current.Threshold;
        }
    }
}
