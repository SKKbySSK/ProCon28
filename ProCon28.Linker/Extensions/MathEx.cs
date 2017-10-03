using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28
{
    public static class MathEx
    {
        public static double Gcd(double a, double b)
        {
            Func<double, double, double> gcd = null;
            gcd = (x, y) => y == 0 ? x : gcd(y, x % y);
            return a > b ? gcd(a, b) : gcd(b, a);
        }

        public static double Gcd(params double[] Numbers)
        {
            if (Numbers.Length == 0) return 0;

            double gcd = Numbers[0];
            for(int i = 1;Numbers.Length > i; i++)
            {
                gcd = Gcd(gcd, Numbers[i]);
            }

            return gcd;
        }

        /// <summary>
        /// 0以上π未満の範囲で値を返します
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Threshold"></param>
        /// <returns></returns>
        public static double Acos(double X, double Threshold = 0)
        {
            double val = Math.Acos(X);
            if (Math.Abs(val - Math.PI) <= Threshold)
                return 0;
            else
                return val;
        }
    }
}
