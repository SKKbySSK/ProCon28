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
        SortedLineDataCollection LineList;
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
                List<int> Judge = new List<int>();
                List<int> Ans = new List<int>();
                Random rand = new Random();
                int index = rand.Next(LineList.Count);
                LineData Line1 = LineList[index];
                int index2 = index;
                bool UpDown = true;
                while (UpDown)
                {
                    index2++;
                    if (index2 > LineList.Count) { UpDown = false; }
                    if (Math.Abs(Line1.Length - LineList[index2].Length) < 0.0001)
                    {
                        Judge.Add(index2);
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
                    if (Math.Abs(Line1.Length - LineList[index2].Length) < 0.0001)
                    {
                        Judge.Add(index2);
                    }
                    else
                    {
                        UpDown = false;
                    }
                }
                foreach (int i in Judge)
                {

                }
            }
        }

        public IList<(int, int)> JudgePiecePair(Piece Piece1, int index1, Piece Piece2, int index2)
        {
            List<(int, int)> r = new List<(int, int)>();
            r.Add((-1, -1));


            return r;
        }

        internal LineJudge JudgeLine(Piece Piece1, int index1, Piece Piece2, int index2)
        {
            bool j = false;
            bool t = true;
            bool p1 = false;
            bool p2 = false;
            List<(double, bool)> sdl1 = Piece1.PieceSideData();
            List<(double, bool)> sdl2 = Piece2.PieceSideData();
            double Angle11 = Piece1.GetAngle(index1 - 1) + Piece2.GetAngle(index2 - 1);
            double Angle12 = Piece1.GetAngle(index1) + Piece2.GetAngle(index2);
            double Angle21 = Piece1.GetAngle(index1 - 1) + Piece2.GetAngle(index2);
            double Angle22 = Piece1.GetAngle(index1) + Piece2.GetAngle(index2 - 1);
            if ( Math.Abs(Angle11 - Math.PI) < 0.001 || Math.Abs(Angle12 - Math.PI) < 0.001)
            {
                j = true;
            }
            else 
            {
                if (Math.Abs(Angle11 - Math.PI * 2) < 0.001)
                {
                    j = true;
                    p1 = true;
                }
                if (Math.Abs(Angle12 - Math.PI * 2) < 0.001)
                {
                    j = true;
                    p2 = true;
                }
            }
            if (Math.Abs(Angle21 - Math.PI) < 0.001 || Math.Abs(Angle22 - Math.PI) < 0.001)
            {
                j = true;
                t = false;
            }
            else
            {
                if (Math.Abs(Angle21 - Math.PI * 2) < 0.001)
                {
                    j = true;
                    p1 = true;
                    t = false;
                }
                if (Math.Abs(Angle22 - Math.PI * 2) < 0.001)
                {
                    j = true;
                    p2 = true;
                    t = false;
                }
            }
            double difSlope = Math.Abs(sdl1[index1].Item1 - sdl2[index1].Item1);
            if (!(difSlope < 0.001 || Math.Abs(difSlope - Math.PI / 2 ) < 0.001 || Math.Abs(difSlope - Math.PI) < 0.001 || Math.Abs(difSlope - Math.PI * 3 / 2) < 0.001))
            {
                j = false;
            }
            LineJudge r = new LineJudge(j,t,p1,p2);
            return r;
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
    }
}
