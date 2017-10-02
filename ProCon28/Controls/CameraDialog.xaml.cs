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
        public ContoursEventArgs(OpenCvSharp.Point[] Contour, double Scale, double Rotation)
        {
            this.Contour = Contour;
            this.Scale = Scale;
            this.Rotation = Rotation;
        }

        public OpenCvSharp.Point[] Contour { get; }
        public double Scale { get; }
        public double Rotation { get; }
    }

    /// <summary>
    /// CameraDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class Camera : UserControl
    {
        public event EventHandler<ContoursEventArgs> Recognized;

        public double PieceCoefficient { get; private set; } = 1.0;
        public double PieceRotation { get; private set; } = 0;
        int MinimumArea { get; set; } = 200;
        int SquareMaximumArcLength { get; set; } = 300;
        int ContourIndex { get; set; } = 1;
        Mat PerspectiveTransform { get; set; }

        Mat Intrinsic, Distortion;

        public System.Collections.ObjectModel.ObservableCollection<string> CalibrationFiles { get; }
            = new System.Collections.ObjectModel.ObservableCollection<string>();

        CameraCapture camera;

        public Camera()
        {
            InitializeComponent();
            Grid.DataContext = this;
            UpdateCalibrations();

            CamT.Text = Config.Current.Camera.ToString();
            ThreshS.Value = Config.Current.PieceApprox;
            SqThreshS.Value = Config.Current.SquareApprox;

            KeyboardHook.HookEvent += OnKeyStateChanged;
        }

        void OnKeyStateChanged(ref KeyboardHook.StateKeyboard State)
        {
            if(State.Stroke == KeyboardHook.Stroke.KEY_UP)
            {
                if (State.Key == System.Windows.Forms.Keys.C)
                {
                    CaptureB_Click(null, null);
                }
                if (State.Key == System.Windows.Forms.Keys.F)
                {
                    using(Mat img = camera.RetrieveMat(false))
                    {
                        FixCamera(img.Size(), FindContours(img));
                    }
                }
                if(State.Key == System.Windows.Forms.Keys.R)
                {
                    PerspectiveTransform?.Dispose();
                    PerspectiveTransform = null;
                }
            }
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
            camera.UseGammaOptimization();
            camera.AddSlider("Index", ContourIndex, 100, (val) => ContourIndex = val);
            camera.AddSlider("Min Length", MinimumArea, 1000, val => MinimumArea = val);
            camera.AddSlider("Sq Length", SquareMaximumArcLength, 1000, val => SquareMaximumArcLength = val);

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

            KeyboardHook.Start();
            camera.Begin();
        }

        private void StopB_Click(object sender, RoutedEventArgs e)
        {
            KeyboardHook.Stop();
            Intrinsic?.Dispose();
            Intrinsic = null;

            Distortion?.Dispose();
            Distortion = null;

            PerspectiveTransform?.Dispose();
            PerspectiveTransform = null;

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
            using(Mat capture = camera.RetrieveMat(false))
            {
                OpenCvSharp.Size size = capture.Size();
                var contours = FindContours(capture);
                var changed = PieceApprox(size, contours);

                int ind = Math.Max(0, Math.Min(changed.Count - 1, ContourIndex));
                Recognized?.Invoke(this, new ContoursEventArgs(changed[ind],
                    PieceCoefficient, PieceRotation));
            }
        }

        public void FixCamera(OpenCvSharp.Size Size, OpenCvSharp.Point[][] Contours)
        {
            var sqpoints = SquareApprox(Size, Contours);
            if (sqpoints == null) return;
            sqpoints = AsAntiClockwise(sqpoints);

            double arclen = Cv2.ArcLength(sqpoints, true);
            PieceCoefficient = (6 * 4) / arclen;

            OpenCvSharp.Point p1 = sqpoints[0], p2 = sqpoints[1];
            int x = Math.Abs(p1.X - p2.X), y = Math.Abs(p1.Y - p2.Y);

            PieceRotation = Math.Atan2(y, x);

            Log.Write("Scale : {0}, Rotation : {1}", PieceCoefficient, PieceRotation);

            bool shouldConvert = false;

            for(int i = 0;sqpoints.Length > i; i++)
            {
                if(i == sqpoints.Length - 1)
                {
                    if ((int)(Math.Round(sqpoints[i].DistanceTo(sqpoints[0]) * PieceCoefficient)) != 6)
                        shouldConvert = true;
                }
                else
                {
                    if ((int)(Math.Round(sqpoints[i].DistanceTo(sqpoints[i + 1]) * PieceCoefficient)) != 6)
                        shouldConvert = true;
                }

                if (shouldConvert)
                    break;
            }

            if (!shouldConvert)
            {
                Log.Write("No need to convert");
            }
            else
            {

                PerspectiveTransform?.Dispose();
                PerspectiveTransform = null;

                int len = p2.Y - p1.Y;
                Point2f[] dst = new Point2f[4];
                dst[0] = p1;
                dst[1] = new Point2f(p1.X, p1.Y + len);
                dst[2] = new Point2f(p1.X + len, p1.Y + len);
                dst[3] = new Point2f(p1.X + len, p1.Y);

                PerspectiveTransform = Cv2.GetPerspectiveTransform(ToFloatArray(sqpoints), dst);
            }
        }

        OpenCvSharp.Point[] AsAntiClockwise(OpenCvSharp.Point[] Points)
        {
            OpenCvSharp.Point BasePoint = Points[0];
            var ps = Points.OrderBy(p => Math.Atan2(BasePoint.X - p.X, BasePoint.Y - p.Y)).ToList();

            int ind = 0, sum = 0;
            for(int i = 0;ps.Count > i; i++)
            {
                int temp = ps[i].X + ps[i].Y;
                if(temp < sum || i == 0)
                {
                    ind = i;
                    sum = temp;
                }
            }

            if (ind > 0)
            {
                List<OpenCvSharp.Point> sorted = new List<OpenCvSharp.Point>();
                sorted.AddRange(ps.GetRange(ind, ps.Count - ind));
                sorted.AddRange(ps.GetRange(0, ind));

                return sorted.ToArray();
            }
            else
                return ps.ToArray();
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

        private void ThreshS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Config.Current.PieceApprox = ThreshS.Value;
        }

        Mat Contours(Mat Image)
        {
            if (Image == null) return null;

            OpenCvSharp.Size size = Image.Size();
            var contours = FindContours(Image);

            var Pieces = PieceApprox(size, contours);
            
            int ind = Math.Max(0, Math.Min(Pieces.Count - 1, ContourIndex));
            Cv2.DrawContours(Image, new OpenCvSharp.Point[][] { Pieces[ind] }, -1, Scalar.Red, 3);

            var Square = SquareApprox(size, contours);

            if (Square != null)
                Cv2.DrawContours(Image, new OpenCvSharp.Point[][] { Square }, -1, Scalar.LightBlue, 3);

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
                    if (p.X == 0 || p.Y == 0 || p.X == Size.Width - 1 || p.Y == Size.Height - 1)
                    {
                        f = true;
                        break;
                    }
                }
                if (!f)
                {
                    if(Cv2.ContourArea(points, false) >= MinimumArea)
                    {
                        double len = Cv2.ArcLength(points, true);
                        var approx = Cv2.ApproxPolyDP(points, Config.Current.PieceApprox * len, true);
                        changed.Add(approx);
                    }
                }
            }

            return changed.OrderByDescending((ps => Cv2.ArcLength(ps, true))).ToList();
        }

        //VertexComparer vcomparer = new VertexComparer();

        //class VertexComparer : IEqualityComparer<OpenCvSharp.Point[]>
        //{
        //    public bool Equals(OpenCvSharp.Point[] x, OpenCvSharp.Point[] y)
        //    {
        //        return x.Length == y.Length;
        //    }

        //    public int GetHashCode(OpenCvSharp.Point[] obj)
        //    {
        //        return obj.Length;
        //    }
        //}

        OpenCvSharp.Point[] SquareApprox(OpenCvSharp.Size Size, OpenCvSharp.Point[][] Contours, double Thresh = 0.7)
        {
            List<OpenCvSharp.Point[]> changed = new List<OpenCvSharp.Point[]>();
            foreach (var points in Contours)
            {
                bool f = false;
                foreach (var p in points)
                {
                    if (p.X == 0 || p.Y == 0 || p.X == Size.Width - 1 || p.Y == Size.Height - 1)
                    {
                        f = true;
                        break;
                    }
                }
                if (!f)
                {
                    double len = Cv2.ArcLength(points, true);

                    if(Cv2.ContourArea(points, false) >= MinimumArea && len <= SquareMaximumArcLength)
                    {
                        changed.Add(Cv2.ApproxPolyDP(points, Config.Current.SquareApprox * len, true));
                    }
                }
            }

            changed = changed.OrderByDescending(ps => Cv2.ArcLength(ps, true)).ToList();

            List<OpenCvSharp.Point[]> returns = new List<OpenCvSharp.Point[]>();
            foreach (OpenCvSharp.Point[] contour in changed)
            {
                if (contour.Length == 4)
                    return contour;
            }

            return null;
        }

        bool IsInRatio(double ExpectRatio, double Threshold, params double[] Args)
        {
            List<double> diff = new List<double>();

            int count = Args.Length;
            for(int i = 0;count > i; i++)
            {
                if (i == count - 1)
                    diff.Add(Math.Abs(ExpectRatio - (Args[0] / Args[count - 1])));
                else
                    diff.Add(Math.Abs(ExpectRatio - (Args[i] / Args[i + 1])));
            }

            foreach(double d in diff)
            {
                if (d > Threshold)
                    return false;
            }

            return true;
        }

        Point2f[] ToFloatArray(OpenCvSharp.Point[] Points)
        {
            Point2f[] ret = new Point2f[Points.Length];
            for (int i = 0; Points.Length > i; i++)
                ret[i] = Points[i];
            return ret;
        }

        private void SqThreshS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Config.Current.SquareApprox = SqThreshS.Value;
        }

        OpenCvSharp.Point[][] FindContours(Mat Image)
        {
            if(PerspectiveTransform != null && !PerspectiveTransform.IsDisposed)
            {
                Cv2.WarpPerspective(Image, Image, PerspectiveTransform, Image.Size());
            }

            Mat gray = new Mat();
            Cv2.CvtColor(Image, gray, ColorConversionCodes.BGR2GRAY);
            //Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);
            Cv2.Threshold(gray, gray, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            Cv2.AdaptiveThreshold(gray, gray, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);

            Cv2.FindContours(gray, out OpenCvSharp.Point[][] contours, out _,
                RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            gray.Dispose();
            gray = null;

            return contours;
        }
    }
}
