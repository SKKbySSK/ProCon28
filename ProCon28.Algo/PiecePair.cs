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

        public List<(int, int)> MainJudge()
        {
            List<(int, int)> r = new List<(int, int)>();
            JudgeData jd = StartJudge();
            if (jd.IsJudge)
            {
                int added1 = Start1 - 1, added2 = Start2 - 1;
                if (added1 == -1) { added1 = Piece1.Vertexes.Count - 1; }
                if (added2 == -1) { added2 = Piece2.Vertexes.Count - 1; }
                if (Turn)
                {
                    r.Add((Start1, Start2));
                    r.Add((added1, added2));
                }
                else
                {
                    r.Add((Start1, added2));
                    r.Add((added1, Start2));
                }
                int updata1 = Start1, updata2 = Start2;
                int index1 = added1, index2 = added2;
                int i = 0;
                while (jd.IsPIPI1)
                {
                    i--;
                    jd = OtherJudge(i);
                    if (jd.IsJudge )
                    {
                        updata1--; index1--;
                        if (updata1 == -1) { updata1 = Piece1.Vertexes.Count - 1; }
                        if (index1 == -1) { index1 = Piece1.Vertexes.Count - 1; }
                        if (updata1 == Piece1.Vertexes.Count - 1) { updata1 = 0; }
                        if (index1 == Piece2.Vertexes.Count - 1) { index1 = 0; }
                        if (Turn)
                        {
                            updata2--; index2--;
                        }
                        else
                        {
                            updata2++; index2++;
                        }
                        if (updata2 == -1) { updata2 = Piece1.Vertexes.Count - 1; }
                        if (index2 == -1) { index2 = Piece2.Vertexes.Count - 1; }
                        if (updata2 == Piece1.Vertexes.Count - 1) { updata2 = 0; }
                        if (index2 == Piece2.Vertexes.Count - 1) { index2 = 0; }

                        if (Rounding(Length1[updata1].Item3, Length2[updata2].Item3))
                        {
                            break;
                        }

                        if (Turn)
                            r.Insert(0, (index1, index2));
                        else
                            r.Insert(0, (index1, updata2));
                    }
                }
                updata1 = Start1; updata2 = Start2;
                index1 = added1; index2 = added2;
                i = 0;
                while (jd.IsPIPI1)
                {
                    i++;
                    jd = OtherJudge(i);
                    if (jd.IsJudge)
                    {
                        updata1++; index1++;
                        if (updata1 == -1) { updata1 = Piece1.Vertexes.Count - 1; }
                        if (index1 == -1) { index1 = Piece1.Vertexes.Count - 1; }
                        if (updata1 == Piece1.Vertexes.Count - 1) { updata1 = 0; }
                        if (index1 == Piece2.Vertexes.Count - 1) { index1 = 0; }
                        if (Turn)
                        {
                            updata2++; index2++;
                        }
                        else
                        {
                            updata2--; index2--;
                        }
                        if (updata2 == -1) { updata2 = Piece1.Vertexes.Count - 1; }
                        if (index2 == -1) { index2 = Piece2.Vertexes.Count - 1; }
                        if (updata2 == Piece1.Vertexes.Count - 1) { updata2 = 0; }
                        if (index2 == Piece2.Vertexes.Count - 1) { index2 = 0; }

                        if (Rounding(Length1[updata1].Item3, Length2[updata2].Item3))
                        {
                            break;
                        }

                        if (Turn)
                            r.Add((updata1, updata2));
                        else
                            r.Add((updata1, index2));
                    }
                }
            }
            
            return r;
        }

        JudgeData StartJudge()
        {
            JudgeData r = LineJudge(Start1, Start2);

            return r;
        }

        JudgeData OtherJudge(int loop)
        {
            int index1, index2;
            index1 = Start1 + loop;
            if (Turn)
                index2 = Start2 + loop;
            else
                index2 = Start2 - loop;
            JudgeData r = LineJudge(index1, index2);
            
            return r;
        }

        JudgeData LineJudge(int index1,int index2)
        {
            bool j = false;
            bool p1 = false;
            bool p2 = false;
            List<(double, bool)> sdl1 = Piece1.PieceSideData();
            List<(double, bool)> sdl2 = Piece2.PieceSideData();
            double Angle11 = Piece1.GetAngle2PI(index1 - 1) + Piece2.GetAngle2PI(index2 - 1);
            double Angle12 = Piece1.GetAngle2PI(index1) + Piece2.GetAngle2PI(index2);
            double Angle21 = Piece1.GetAngle2PI(index1 - 1) + Piece2.GetAngle2PI(index2);
            double Angle22 = Piece1.GetAngle2PI(index1) + Piece2.GetAngle2PI(index2 - 1);

            if (Turn)
            {
                if (Rounding(Angle11, Math.PI) || Rounding(Angle12, Math.PI))
                {
                    j = true;
                }
                else
                {
                    if (Rounding(Angle11, Math.PI * 2))
                    {
                        j = true;
                        p1 = true;
                    }
                    if (Rounding(Angle12, Math.PI * 2))
                    {
                        j = true;
                        p2 = true;
                    }
                }
            }
            else
            {
                if (Rounding(Angle21, Math.PI) || Rounding(Angle22, Math.PI))
                {
                    j = true;
                }
                else
                {
                    if (Rounding(Angle21, Math.PI * 2))
                    {
                        j = true;
                        p1 = true;
                    }
                    if (Rounding(Angle22, Math.PI * 2))
                    {
                        j = true;
                        p2 = true;
                    }
                }
            }
            double difSlope = Math.Abs(sdl1[index1].Item1 - sdl2[index1].Item1);
            if (!(Rounding(difSlope, 0) || Rounding(difSlope, Math.PI / 2) || Rounding(difSlope, Math.PI) || Rounding(difSlope, Math.PI * 3 / 2) || Rounding(difSlope, Math.PI * 2)))
            {
                j = false;
            }
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
