using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace TestConsole
{
    class CameraCalibration : IConsole
    {
        public string Title { get; } = "キャリブレーション生成";

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

            List<MatOfPoint3f> objPoints = new List<MatOfPoint3f>();
            List<MatOfPoint2f> imgs = new List<MatOfPoint2f>();
            Point3f[] objs = new Point3f[PAT_SIZE];
            for (int j = 0; j < PAT_ROW; j++)
            {
                for (int k = 0; k < PAT_COL; k++)
                {
                    objs[j * PAT_COL + k] = new Point3f(j * CHESS_SIZE, k * CHESS_SIZE, 0);
                }
            }

            Size imgSize = new Size();
            for (int i = 0;times > i; i++)
            {
                string fname = string.Format("{0:0000}.jpg", i);
                using (Mat load = Cv2.ImRead(fname))
                using (Mat gray = new Mat())
                {
                    imgSize = load.Size();

                    MatOfPoint2f corners = new MatOfPoint2f();
                    if (Cv2.FindChessboardCorners(load, patternSize, corners))
                    {
                        Console.WriteLine("Chessboard Corners found in {0}", fname);

                        Cv2.CvtColor(load, gray, ColorConversionCodes.BGR2GRAY);
                        if(Cv2.Find4QuadCornerSubpix(gray, corners, new Size(3, 3)))
                        {
                            imgs.Add(corners);
                            objPoints.Add(new MatOfPoint3f(PAT_SIZE, 1, objs));

                            Cv2.DrawChessboardCorners(load, patternSize, (InputArray)corners, true);
                        }
                    }
                    else
                        Console.WriteLine("Failed to find corners in {0}", fname);
                }
            }

            Window.DestroyAllWindows();

            Mat intrinsic = new Mat(3, 3, MatType.CV_32SC1);
            Mat distortion = new Mat(1, 4, MatType.CV_32SC1);

            Cv2.CalibrateCamera(objPoints, imgs, imgSize, intrinsic, distortion, out Mat[] rvecs, out Mat[] tvecs);

            OpenFileDialog ofd = new OpenFileDialog() { FileName = "" };
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                using (Mat load = Cv2.ImRead(ofd.FileName))
                using (Mat undis = new Mat())
                {
                    Cv2.Undistort(load, undis, intrinsic, distortion);
                    Window raw = new Window("Raw");
                    raw.ShowImage(load);

                    Window undisort = new Window("Undisort");
                    undisort.ShowImage(undis);

                    Cv2.WaitKey(0);
                    Window.DestroyAllWindows();
                }
            }

            FileStorage fs = new FileStorage("Calibration.xml", FileStorage.Mode.FormatXml | FileStorage.Mode.Write);
            fs.Write("Intrinsic", intrinsic);
            fs.Write("Distortion", distortion);
            fs.Dispose();
        }
    }
}
