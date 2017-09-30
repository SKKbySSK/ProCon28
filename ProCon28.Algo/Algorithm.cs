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
                    if (Rounding(Line1.Length, LineList[index2].Length))
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
                    if (Rounding(Line1.Length , LineList[index2].Length))
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
            LineJudge First = JudgeLine(Piece1, index1, Piece2, index2);
            if (First.IsJudge)
            { 
                int added1 = index1 - 1, added2 = index2 - 1;
                if (added1 == -1) { added1 = Piece1.Vertexes.Count; }
                if (added2 == -1) { added2 = Piece2.Vertexes.Count; }

                if (First.IsTurn)
                {
                    r.Add((index1, index2));
                    r.Add((added1, added2));

                    if (First.IsPIPI1)
                    {
                        
                    }
                    if (First.IsPIPI2)
                    {

                    }
                }
                else
                {
                    r.Add((index1, added2));
                    r.Add((added1, index2));
                }
                r.RemoveAt(0);
            }

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
            double Angle11 = Piece1.GetAngle2PI(index1 - 1) + Piece2.GetAngle2PI(index2 - 1);
            double Angle12 = Piece1.GetAngle2PI(index1) + Piece2.GetAngle2PI(index2);
            double Angle21 = Piece1.GetAngle2PI(index1 - 1) + Piece2.GetAngle2PI(index2);
            double Angle22 = Piece1.GetAngle2PI(index1) + Piece2.GetAngle2PI(index2 - 1);

            if ( Rounding(Angle11 , Math.PI) || Rounding(Angle12 , Math.PI))
            {
                j = true;
            }
            else 
            {
                if (Rounding(Angle11 , Math.PI * 2))
                {
                    j = true;
                    p1 = true;
                }
                if (Rounding(Angle12 , Math.PI * 2))
                {
                    j = true;
                    p2 = true;
                }
            }

            if (Rounding(Angle21 , Math.PI)|| Rounding(Angle22 , Math.PI))
            {
                j = true;
                t = false;
            }
            else
            {
                if (Rounding(Angle21 , Math.PI * 2))
                {
                    j = true;
                    p1 = true;
                    t = false;
                }
                if (Rounding(Angle22 , Math.PI * 2))
                {
                    j = true;
                    p2 = true;
                    t = false;
                }
            }
            double difSlope = Math.Abs(sdl1[index1].Item1 - sdl2[index1].Item1);
            if (!(Rounding(difSlope,0) || Rounding(difSlope , Math.PI / 2 ) || Rounding(difSlope , Math.PI) || Rounding(difSlope , Math.PI * 3 / 2) || Rounding(difSlope,Math.PI * 2)))
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
