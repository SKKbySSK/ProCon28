using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenCvSharp;
using ProCon28.OpenCV;
using ProCon28.Linker.Extensions;

namespace ProCon28.Controls
{
    public class ContoursEventArgs : EventArgs
    {
        public ContoursEventArgs(OpenCvSharp.Point[] Contours, double Scale)
        {
            this.Contours = Contours;
            this.Scale = Scale;
        }

        public OpenCvSharp.Point[] Contours { get; }
        public double Scale { get; }
    }

    /// <summary>
    /// CameraDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class Camera : UserControl
    {
        const int ContourLimit = 2;

        public event EventHandler<ContoursEventArgs> Recognized;

        Mat Intrinsic, Distortion;

        public System.Collections.ObjectModel.ObservableCollection<string> CalibrationFiles { get; }
            = new System.Collections.ObjectModel.ObservableCollection<string>();

        CameraCapture camera;

        public Camera()
        {
            InitializeComponent();
            Grid.DataContext = this;
            UpdateCalibrations();

            ScaleS.Value = Config.Current.CameraScale;
            CamT.Text = Config.Current.Camera.ToString();
        }

        void UpdateCalibrations()
        {
            try
            {
                const string CalibDir = "Calibration";
                System.IO.Directory.CreateDirectory(CalibDir);

                string[] files = System.IO.Directory.GetFiles(CalibDir);
                CalibrationFiles.Clear();
                CalibrationFiles.Add("無効");
                CalibrationFiles.AddRange(files);

                CalibC.SelectedIndex = files.Length > 0 ? 1 : 0;
            }
            catch (Exception) { }
        }

        private void BeginB_Click(object sender, RoutedEventArgs e)
        {
            RefreshDirB.IsEnabled = false;
            CalibC.IsEnabled = false;
            StopB.IsEnabled = true;
            BeginB.IsEnabled = false;
            CameraOperationGrid.IsEnabled = true;

            if (int.TryParse(CamT.Text, out int dev) && dev > -1)
                Config.Current.Camera = dev;

            camera = new CameraCapture(Config.Current.Camera, "Recognizer");

            if (CalibC.SelectedIndex > 0)
            {
                FileStorage fs = new FileStorage(CalibC.SelectedItem.ToString(),
                    FileStorage.Mode.FormatXml | FileStorage.Mode.Read);
                Intrinsic = fs["Intrinsic"].ReadMat();
                Distortion = fs["Distortion"].ReadMat();
                fs.Dispose();

                camera.Filters.Add(Calibrate);
            }
            camera.Filters.Add(Contours);

            camera.Begin();
        }

        private void StopB_Click(object sender, RoutedEventArgs e)
        {
            Intrinsic?.Dispose();
            Intrinsic = null;

            Distortion?.Dispose();
            Distortion = null;

            RefreshDirB.IsEnabled = true;
            CalibC.IsEnabled = true;
            StopB.IsEnabled = false;
            BeginB.IsEnabled = true;
            CameraOperationGrid.IsEnabled = false;

            camera.Stop();
            camera.Dispose();
            camera = null;
        }

        private void RefreshDirB_Click(object sender, RoutedEventArgs e)
        {
            UpdateCalibrations();
        }

        private void CaptureB_Click(object sender, RoutedEventArgs e)
        {
            using(Mat capture = camera.RetrieveMat(true))
            {
                OpenCvSharp.Size size = capture.Size();
                Mat gray = new Mat();
                Cv2.CvtColor(capture, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);
                Cv2.Threshold(gray, gray, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                Cv2.AdaptiveThreshold(gray, gray, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);

                Cv2.FindContours(gray, out OpenCvSharp.Point[][] contours, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                gray.Dispose();
                gray = null;

                List<OpenCvSharp.Point[]> changed = new List<OpenCvSharp.Point[]>();
                foreach(var points in contours)
                {
                    bool f = false;
                    foreach(var p in points)
                    {
                        if (p.X == 0 || p.Y == 0 || p.X == size.Width || p.Y == size.Height)
                        {
                            f = true;
                            break;
                        }
                    }
                    if (!f)
                    {
                        double len = Cv2.ArcLength(points, true);
                        changed.Add(Cv2.ApproxPolyDP(points, ThreshS.Value * len, true));
                    }
                }

                changed = changed.OrderByDescending((ps => Cv2.ArcLength(ps, true))).ToList();

                Linker.Piece Square = new Linker.Piece();

                const double thresh = 0.3;

                foreach(OpenCvSharp.Point[] contour in changed)
                {
                    if(contour.Length == 4)
                    {
                        double l1, l2, l3, l4;
                        l1 = GetLength(contour[0], contour[1]);
                        l2 = GetLength(contour[1], contour[2]);
                        l3 = GetLength(contour[2], contour[3]);
                        l4 = GetLength(contour[3], contour[0]);

                        if(InRatio(l1, l2, 1, thresh) && InRatio(l2, l3, 1, thresh) &&
                            InRatio(l3, l4, 1, thresh) && InRatio(l4, l1, 1, thresh))
                        {
                            foreach(OpenCvSharp.Point p in contour)
                            {
                                Square.Vertexes.Add(new Linker.Point(p.X, p.Y));
                            }
                            break;
                        }
                    }
                }

                int count = Math.Min(changed.Count, ContourLimit);
                for (int i = 0; count > i; i++)
                {
                    Recognized?.Invoke(this, new ContoursEventArgs(changed[i], ScaleS.Value));
                }
            }
        }

        bool InRatio(double X, double Y, double ExpectRatio, double Threshold)
        {
            double ratio = Math.Abs(X / Y);
            return Math.Abs(ExpectRatio - ratio) <= Threshold;
        }

        double GetLength(OpenCvSharp.Point P1, OpenCvSharp.Point P2)
        {
            double x = P1.X - P2.Y;
            double y = P1.Y - P2.Y;
            return Math.Sqrt(x * x + y * y);
        }

        Mat Calibrate(Mat Image)
        {
            if (Intrinsic == null || Distortion == null || Image == null) return null;
            using (Image)
            {
                Mat ret = new Mat();

                OpenCvSharp.Size size = Image.Size();
                Mat newMatrix = Cv2.GetOptimalNewCameraMatrix(Intrinsic, Distortion, size, 0, size, out _);
                Cv2.Undistort(Image, ret, Intrinsic, Distortion, newMatrix);
                return ret;
            }
        }

        private void ScaleS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Config.Current.CameraScale = ScaleS.Value;
        }

        private void ThreshS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Config.Current.PieceApprox = ThreshS.Value;
        }

        Mat Contours(Mat Image)
        {
            if (Image == null) return null;
            OpenCvSharp.Size size = Image.Size();
            Mat gray = new Mat();
            Cv2.CvtColor(Image, gray, ColorConversionCodes.BGR2GRAY);
            //Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);
            Cv2.Threshold(gray, gray, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            Cv2.AdaptiveThreshold(gray, gray, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);

            Cv2.FindContours(gray, out OpenCvSharp.Point[][] contours, out _, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            gray.Dispose();
            gray = null;

            var Pieces = PieceApprox(size, contours);

            int count = Math.Min(Pieces.Count, ContourLimit);
            Cv2.DrawContours(Image, Pieces.GetRange(0, count), -1, Scalar.Red, 3);

            var Square = SquareApprox(size, contours);

            if (Square != null)
            {
                Cv2.DrawContours(Image, new OpenCvSharp.Point[][] { Square }, -1, Scalar.Green, 3);

                List<Point2f> dst = new List<Point2f>();

                dst.Add(new Point2f(0, 0));
                dst.Add(new Point2f(0, 6));
                dst.Add(new Point2f(6, 6));
                dst.Add(new Point2f(6, 0));
                
                Mat transform = 
                    Cv2.GetPerspectiveTransform(ToFloatArray(Square), dst);
                Cv2.WarpPerspective(Image, Image, transform, size);
            }

            return Image;
        }

        List<OpenCvSharp.Point[]> PieceApprox(OpenCvSharp.Size Size, OpenCvSharp.Point[][] Contours)
        {
            List<OpenCvSharp.Point[]> changed = new List<OpenCvSharp.Point[]>();
            foreach (var points in Contours)
            {
                bool f = false;
                foreach (var p in points)
                {
                    if (p.X == 0 || p.Y == 0 || p.X == Size.Width || p.Y == Size.Height)
                    {
                        f = true;
                        break;
                    }
                }
                if (!f)
                {
                    double len = Cv2.ArcLength(points, true);
                    changed.Add(Cv2.ApproxPolyDP(points, Config.Current.PieceApprox * len, true));
                }
            }

            return changed.OrderByDescending((ps => Cv2.ArcLength(ps, true))).ToList();
        }

        OpenCvSharp.Point[] SquareApprox(OpenCvSharp.Size Size, OpenCvSharp.Point[][] Contours, double Thresh = 0.3)
        {
            List<OpenCvSharp.Point[]> changed = new List<OpenCvSharp.Point[]>();
            foreach (var points in Contours)
            {
                bool f = false;
                foreach (var p in points)
                {
                    if (p.X == 0 || p.Y == 0 || p.X == Size.Width || p.Y == Size.Height)
                    {
                        f = true;
                        break;
                    }
                }
                if (!f)
                {
                    double len = Cv2.ArcLength(points, true);


                    changed.Add(Cv2.ApproxPolyDP(points, Config.Current.SquareApprox * len, true));
                }
            }

            foreach (OpenCvSharp.Point[] contour in changed)
            {
                if (contour.Length == 4)
                {
                    double l1, l2, l3, l4;
                    l1 = GetLength(contour[0], contour[1]);
                    l2 = GetLength(contour[1], contour[2]);
                    l3 = GetLength(contour[2], contour[3]);
                    l4 = GetLength(contour[3], contour[0]);

                    if (InRatio(l1, l2, 1, Thresh) && InRatio(l2, l3, 1, Thresh) &&
                        InRatio(l3, l4, 1, Thresh) && InRatio(l4, l1, 1, Thresh))
                    {
                        return contour;
                    }
                }
            }
            
            return null;
        }

        Point2f[] ToFloatArray(OpenCvSharp.Point[] Points)
        {
            Point2f[] ret = new Point2f[Points.Length];
            for (int i = 0; Points.Length > i; i++)
                ret[i] = Points[i];
            return ret;
        }
    }
}
