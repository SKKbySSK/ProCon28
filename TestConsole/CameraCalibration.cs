using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace TestConsole
{
    class CameraCalibration : IConsole
    {
        public string Title { get; } = "カメラキャリブレーション";

        const int PAT_ROW = 9;
        const int PAT_COL = 18;
        const int PAT_SIZE = PAT_ROW * PAT_COL;
        const float CHESS_SIZE = 25.0f;

        public void Run()
        {
            Size patternSize = new Size(PAT_COL, PAT_ROW);

            Console.WriteLine("キャリブレーション画像を撮影しますか？[Y/N]");

            string yn = "";
            while(yn != "Y" && yn != "N")
            { yn = Console.ReadLine().ToUpper(); }

            int times = -1;
            if(yn == "Y")
            {
                Console.WriteLine("デバイス番号");
                int dev = int.Parse(Console.ReadLine());

                Console.WriteLine("撮影回数");
                if (int.TryParse(Console.ReadLine(), out times))
                {
                    using (ProCon28.OpenCV.CameraCapture cam = new ProCon28.OpenCV.CameraCapture(dev, "Camera"))
                    {
                        CalibDialog dialog = new CalibDialog(cam, times);
                        dialog.ShowDialog();
                    }
                }
                else
                    return;
            }
            
            if(times <= 0)
            {
                Console.WriteLine("画像の読み込み枚数");
                times = int.Parse(Console.ReadLine());
            }

            int ALL_POINTS = times * PAT_SIZE;
            Point2f[][] corners = new Point2f[times][];

            List<Mat> objPoints = new List<Mat>();

            Window window = new Window("Preview");

            Size imgSize = new Size();
            for (int i = 0;times > i; i++)
            {
                Point3f[] objs = new Point3f[PAT_SIZE];
                for (int j = 0; j < PAT_ROW; j++)
                {
                    for (int k = 0; k < PAT_COL; k++)
                    {
                        objs[j * PAT_COL + k] = new Point3f(j * CHESS_SIZE, k * CHESS_SIZE, 0);
                    }
                }

                string fname = string.Format("{0:0000}.jpg", i);
                using (Mat load = Cv2.ImRead(fname))
                using (Mat gray = new Mat())
                {
                    imgSize = load.Size();
                    if (Cv2.FindChessboardCorners(load, patternSize, out corners[i]))
                    {
                        Console.WriteLine("Chessboard Corners found in {0}", fname);

                        Cv2.CvtColor(load, gray, ColorConversionCodes.BGR2GRAY);
                        corners[i] = Cv2.CornerSubPix(gray, corners[i], new Size(3, 3), new Size(-1, -1), new TermCriteria(CriteriaType.Eps, 20, 0.03));
                        objPoints.Add(new Mat(PAT_SIZE, 3, MatType.CV_32SC1, objs));
                        Cv2.DrawChessboardCorners(load, patternSize, corners[i], true);
                        window.ShowImage(load);
                        //Cv2.WaitKey(500);
                    }
                    else
                        Console.WriteLine("Failed to find corners in {0}", fname);
                }
            }

            Window.DestroyAllWindows();

            Mat intrinsic = new Mat(3, 3, MatType.CV_32SC1);
            Mat rotation = new Mat(1, 3, MatType.CV_32SC1);
            Mat translation = new Mat(1, 3, MatType.CV_32SC1);
            Mat distortion = new Mat(1, 4, MatType.CV_32SC1);

            List<Mat> imgs = new List<Mat>();
            foreach(Point2f[] ps in corners)
            {
                imgs.Add(new Mat(PAT_SIZE, 1, MatType.CV_32SC2, ps));
            }

            double val = Cv2.CalibrateCamera(objPoints, imgs, imgSize, intrinsic, distortion, out Mat[] rvecs, out Mat[] tvecs);
        }
    }
}
