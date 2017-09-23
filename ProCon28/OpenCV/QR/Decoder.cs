using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using ZXing;

namespace ProCon28.OpenCV.QR
{
    public class Decoder
    {
        static Decoder()
        {
            Reader.AutoRotate = true;
            Reader.TryInverted = false;
        }

        public static BarcodeReader Reader { get; } = new BarcodeReader();

        public static string Decode(System.Drawing.Bitmap Image)
        {
            Result res = Reader.Decode(Image);

            return res == null ? "" : (res.Text == null ? "" : res.Text);
        }

        public static string Decode(Mat Image)
        {
            using (System.Drawing.Bitmap bmp = Image.ToBitmap())
                return Decode(bmp);
        }

        public static string[] DecodeMultiple(System.Drawing.Bitmap Image)
        {
            Result[] results = Reader.DecodeMultiple(Image);

            if (results == null)
                return new string[0];
            else
            {
                string[] ret = new string[results.Length];
                for (int i = 0; results.Length > i; i++)
                    ret[i] = results[i].Text;

                return ret;
            }
        }

        public static string[] DecodeMultiple(Mat Image)
        {
            using (System.Drawing.Bitmap bmp = Image.ToBitmap())
                return DecodeMultiple(bmp);
        }
    }
}
