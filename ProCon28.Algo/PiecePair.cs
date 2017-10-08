using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;
using ProCon28.Linker.Extensions;


namespace ProCon28.Algo
{
    public class PiecePair
    {
        public PiecePair(Piece p1, int i1, Piece p2, int i2, bool t)
        {
            Piece1 = p1;
            Piece2 = p2;
            Start1 = i1;
            Start2 = i2;
            Turn = t;
            Length1 = p1.Vertexes.AsLinesWithLength();
            Length2 = p2.Vertexes.AsLinesWithLength();
        }
        
        Piece Piece1 { get; }
        Piece Piece2 { get; }
        int Start1 { get; }
        int Start2 { get; }
        IList<(Point, Point, double)> Length1 { get; }
        IList<(Point, Point, double)> Length2 { get; }
        bool Turn { get; set; }

        List<(int, int)> Return { get; set; } = new List<(int, int)>();

        public List<(int, int)> MainJudge()
        {
            JudgeData jd = StartJudge();
            if (jd.IsJudge)
            {
                int added1 = Start1 - 1, added2 = Start2 - 1;
                if (added1 == -1) { added1 = Piece1.Vertexes.Count - 1; }
                if (added2 == -1) { added2 = Piece2.Vertexes.Count - 1; }
                if (Turn)
                {
                    Return.Add((Start1, Start2));
                    Return.Add((added1, added2));
                }
                else
                {
                    Return.Add((Start1, added2));
                    Return.Add((added1, Start2));
                }

                int l = 0;
                while (jd.IsPIPI1)
                {
                    l++;
                    jd = OtherJudge(l);
                }

                l = 0;
                while (jd.IsPIPI2)
                {
                    l--;
                    jd = OtherJudge(l);
                }
            }
            if (Piece2 is CompositePiece)
                Return = new List<(int, int)>();

            return Return; 
        }

        JudgeData StartJudge()
        {
            return LineJudge(Start1, Start2);
        }

        JudgeData OtherJudge(int loop)
        {
            int index1, index2;
            index1 = Start1 + loop;
            if (index1 < 0) { index1 += Piece1.Vertexes.Count ; }
            if (index1 >= Piece1.Vertexes.Count) { index1 -= Piece1.Vertexes.Count; }
            if (Turn)
                index2 = Start2 + loop;
            else
                index2 = Start2 - loop;
            if (index2 < 0) { index2 += Piece2.Vertexes.Count; }
            if (index2 >= Piece2.Vertexes.Count) { index2 -= Piece2.Vertexes.Count; }

            if(!Rounding( Length1[index1].Item3, Length2[index2].Item3))
            {
                return new JudgeData(false, false, false);
            }

            JudgeData r = LineJudge(index1, index2);

            if (!r.IsJudge)
            {
                return new JudgeData(false, false, false); ;
            }

            int added1 = index1 - 1, added2 = index2 - 1;
            if(added1 == -1) { added1 = Piece1.Vertexes.Count - 1; }
            if(added1 == Piece1.Vertexes.Count) { added1 = 0; }
            if(added2 == -1) { added2 = Piece2.Vertexes.Count - 1; }
            if(added2 == Piece2.Vertexes.Count) { added2 = 0; }

            if(loop > 0)
            {
                if (Turn)
                    Return.Insert(0, (index1, index2));
                else
                    Return.Insert(0, (index1, added2));
            }
            else
            {
                if (Turn)
                    Return.Add((index1, index2));
                else
                    Return.Add((index1, added2));
            }
            return r;
        }

        JudgeData LineJudge(int index1,int index2)
        {
            bool j = false;
            bool p1 = false;
            bool p2 = false;
            int minus1 = index1 - 1, minus2 = index2 - 1;
            List<(double, bool)> sdl1 = Piece1.PieceSideData();
            List<(double, bool)> sdl2 = Piece2.PieceSideData();
            if (index1 == 0) { minus1 = Piece1.Vertexes.Count - 1; }
            if (index2 == 0) { minus2 = Piece2.Vertexes.Count - 1; }
            double rad11 = Piece1.GetAngle(minus1);
            double rad12 = Piece1.GetAngle(index1);
            double rad21 = Piece2.GetAngle(minus2);
            double rad22 = Piece2.GetAngle(index2);
            double Angle11 = Piece1.GetAngle2PI(minus1) + Piece2.GetAngle2PI(minus2);
            double Angle12 = Piece1.GetAngle2PI(index1) + Piece2.GetAngle2PI(index2);
            double Angle21 = Piece1.GetAngle2PI(minus1) + Piece2.GetAngle2PI(index2);
            double Angle22 = Piece1.GetAngle2PI(index1) + Piece2.GetAngle2PI(minus2);

            if (Turn)
            {
                if (Rounding(Angle11, Math.PI) || Rounding(Angle12, Math.PI))
                {
                    j = true;
                }
                else
                {
                    if (Rounding(rad11, rad12))
                    {
                        j = true;
                        p1 = true;
                    }
                    if (Rounding(rad21, rad22))
                    {
                        j = true;
                        p2 = true;
                    }
                }
                //if (Angle11 > Math.PI * 2 || Angle12 > Math.PI * 2)
                //    j = false;
            }
            else
            {
                if (Rounding(Angle21, Math.PI) || Rounding(Angle22, Math.PI))
                {
                    j = true;
                }
                else
                {
                    if (Rounding(rad11, rad22))
                    {
                        j = true;
                        p1 = true;
                    }
                    if (Rounding(rad12, rad21))
                    {
                        j = true;
                        p2 = true;
                    }
                }
                //if (Angle21 > Math.PI * 2 || Angle22 > Math.PI * 2)
                //    j = false;
            }
            
            //double difSlope = Math.Abs(sdl1[index1].Item1 - sdl2[index2].Item1);
            //if (!(Rounding(difSlope, 0) || Rounding(difSlope, Math.PI / 2) || Rounding(difSlope, Math.PI) || Rounding(difSlope, Math.PI * 3 / 2) || Rounding(difSlope, Math.PI * 2)))
            //{
            //    j = false;
            //}
            return new JudgeData(j, p1, p2);
        }

        public bool Rounding(double Value1, double Value2)
        {
            return Math.Abs(Value1 - Value2) < Config.Current.Threshold;
        }

        internal class JudgeData
        {
             public JudgeData(bool j, bool p1, bool p2)
            {
                IsJudge = j;
                IsPIPI1 = p1;
                IsPIPI1 = p2;
            }

            public bool IsJudge { get; }
            public bool IsPIPI1 { get; }
            public bool IsPIPI2 { get; }
        }
    }
}
