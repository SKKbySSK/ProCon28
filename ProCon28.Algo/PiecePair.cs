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
        public PiecePair(Piece p1,int i1,Piece p2,int i2)
        {
            Piece1 = p1;
            Piece2 = p2;
            Start1 = i1;
            Start2 = i2;
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
            r.Add((-1, -1));


            return r;
        }

        JudgeData StartJudge()
        {


            return new JudgeData(); ;
        }

        JudgeData OtherJudge(int loop)
        {

            return new JudgeData(); ;
        }

        JudgeData LineJudge(int index1,int index2)
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

            if (Rounding(Angle21, Math.PI) || Rounding(Angle22, Math.PI))
            {
                j = true;
                t = false;
            }
            else
            {
                if (Rounding(Angle21, Math.PI * 2))
                {
                    j = true;
                    p1 = true;
                    t = false;
                }
                if (Rounding(Angle22, Math.PI * 2))
                {
                    j = true;
                    p2 = true;
                    t = false;
                }
            }
            double difSlope = Math.Abs(sdl1[index1].Item1 - sdl2[index1].Item1);
            if (!(Rounding(difSlope, 0) || Rounding(difSlope, Math.PI / 2) || Rounding(difSlope, Math.PI) || Rounding(difSlope, Math.PI * 3 / 2) || Rounding(difSlope, Math.PI * 2)))
            {
                j = false;
            }
            return new JudgeData(j, t, p1, p2);
        }

        public bool Rounding(double Value1, double Value2)
        {
            bool r;
            if (Math.Abs(Value1 - Value2) < 0.001)
                r = true;
            else
                r = false;
         
            return r;
        }

        internal class JudgeData
        {
             public JudgeData(bool j, bool t, bool p1, bool p2)
            {
                IsJudge = j;
                IsTurn = t;
                IsPIPI1 = p1;
                IsPIPI1 = p2;
            }

            public bool IsJudge { get; }
            public bool IsTurn { get; }
            public bool IsPIPI1 { get; }
            public bool IsPIPI2 { get; }
        }
    }
}
