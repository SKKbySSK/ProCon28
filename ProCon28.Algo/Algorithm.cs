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
                    
                }
            }
        }

        public (bool,bool) JudgePiecePair(Piece Piece1,int index1, Piece Piece2,int index2)
        {
            bool Judge = true;
            bool Flip = false;
            double Angle11 = Piece1.GetAngle(index1 - 1) + Piece2.GetAngle(index2 - 1);
            double Angle12 = Piece1.GetAngle(index1) + Piece2.GetAngle(index2);
            double Angle21 = Piece1.GetAngle(index1 - 1) + Piece2.GetAngle(index2);
            double Angle22 = Piece1.GetAngle(index1) + Piece2.GetAngle(index2 - 1);
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
                    Flip = true;
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
            
            return (Judge,Flip);
        }

        public Piece PieceBond(Piece Source1, int n1, Piece Source2, int n2)
        {
            Piece p = null;



            return p;
        }
    }
}
