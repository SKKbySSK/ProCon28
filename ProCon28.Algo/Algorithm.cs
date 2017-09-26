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
                    if(index2 > LineList.Count) { UpDown = false; }
                    if(Math.Abs(Line1.Length - LineList[index2].Length) < 0.0001)
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
                foreach(int i in Judge)
                {
                    (bool, bool) res;
                    res = JudgePiecePair(Line1, LineList[i]);
                    if (res.Item1) { Ans.Add(i); }
                }
            }
        }

        public (bool,bool) JudgePiecePair(LineData Line1, LineData Line2)
        {
            bool Judge = true;
            bool Turn = false;
            double Angle11 = PieceCollection[Line1.PieceNumber].GetAngle(Line1.LineNumber - 1) + PieceCollection[Line2.PieceNumber].GetAngle(Line2.LineNumber - 1);
            double Angle12 = PieceCollection[Line1.PieceNumber].GetAngle(Line1.LineNumber) + PieceCollection[Line2.PieceNumber].GetAngle(Line2.LineNumber);
            double Angle21 = PieceCollection[Line1.PieceNumber].GetAngle(Line1.LineNumber - 1) + PieceCollection[Line2.PieceNumber].GetAngle(Line2.LineNumber);
            double Angle22 = PieceCollection[Line1.PieceNumber].GetAngle(Line1.LineNumber) + PieceCollection[Line2.PieceNumber].GetAngle(Line2.LineNumber - 1);
            if (Math.Abs(Angle11 - Math.PI) < 0.0001 || Math.Abs(Angle11 - Math.PI * 2) < 0.0001)
            {
                if (Math.Abs(Angle12 - Math.PI) < 0.0001 || Math.Abs(Angle12 - Math.PI * 2) < 0.0001)
                {
                }
                else
                {
                    Judge = false;
                }
            }
            else if (Math.Abs(Angle21 - Math.PI) < 0.0001 || Math.Abs(Angle21 - Math.PI * 2) < 0.0001)
            {
                if (Math.Abs(Angle22 - Math.PI) < 0.0001 || Math.Abs(Angle22 - Math.PI * 2) < 0.0001)
                {
                    Turn = true;
                }
                else
                {
                    Judge = false;
                }
            }
            else
            {
                Judge = false;
            }
            
            return (Judge,Turn);
        }

        public Piece PieceBond(Piece Source1, int n1, Piece Source2, int n2)
        {
            Piece p;



            return p;
        }
    }
}
