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
        public ContoursEventArgs(OpenCvSharp.Point[][] Contours, double Scale, double Rotation)
        {
            this.Contours = Contours;
            this.Scale = Scale;
            this.Rotation = Rotation;
        }

        public OpenCvSharp.Point[][] Contours { get; }
        public double Scale { get; }
        public double Rotation { get; }
    }

    /// <summary>
    /// CameraDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class Camera : UserControl
    {
        const int ContourLimit = 2;

        public event EventHandler<ContoursEventArgs> Recognized;

        public double PieceCoefficient { get; private set; } = 1.0;
        public double PieceRotation { get; private set; } = 0;
        int MinimumArcLength { get; set; } = 30;

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
                    using(Mat img = camera.RetrieveMat(true))
                    {
                        UpdateCoefficientAndRotation(img.Size(), FindContours(img));
                    }
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
            camera.AddSlider("Arc Length", MinimumArcLength, 1000, val => MinimumArcLength = val);

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
                var contours = FindContours(capture);
                var changed = PieceApprox(size, contours);

                int count = Math.Min(changed.Count, ContourLimit);
                Recognized?.Invoke(this, new ContoursEventArgs(changed.GetRange(0, count).ToArray(),
                    PieceCoefficient, PieceRotation));
            }
        }

        public void UpdateCoefficientAndRotation(OpenCvSharp.Size Size, OpenCvSharp.Point[][] Contours)
        {
            var sqpoints = SquareApprox(Size, Contours);
            if (sqpoints == null) return;

            Linker.Piece Square = new Linker.Piece();
            foreach (var ps in sqpoints)
            {
                Square.Vertexes.Add(new Linker.Point(ps.X, ps.Y));
            }

            double av = 0;
            var lines = Square.Vertexes.AsLinesWithLength();
            foreach (var line in lines)
                av += line.Item3;
            av /= lines.Count;

            PieceCoefficient = 6 / av;

            var bpoint = lines[0];
            double cos = Math.Abs(bpoint.Item1.X - bpoint.Item2.X) / bpoint.Item3;
            PieceRotation= Math.Acos(cos);

            Log.Write("Scale : {0}, Rotation : {1}", PieceCoefficient, PieceRotation);
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

            int count = Math.Min(Pieces.Count, ContourLimit);
            Cv2.DrawContours(Image, Pieces.GetRange(0, count), -1, Scalar.Red, 3);

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
                    double len = Cv2.ArcLength(points, true);

                    if(len >= MinimumArcLength)
                    {
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

                    if (len >= MinimumArcLength)
                        changed.Add(Cv2.ApproxPolyDP(points, Config.Current.SquareApprox * len, true));
                }
            }

            changed = changed.OrderByDescending(ps => Cv2.ArcLength(ps, true)).ToList();

            List<OpenCvSharp.Point[]> returns = new List<OpenCvSharp.Point[]>();
            foreach (OpenCvSharp.Point[] contour in changed)
            {
                if (contour.Length == 4 && Cv2.ContourArea(contour, false) > 1000)
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
