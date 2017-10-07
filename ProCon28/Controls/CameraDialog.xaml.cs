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
using OpenCvSharp.Extensions;

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

    public class QrReaderEventArgs : EventArgs
    {
        public QrReaderEventArgs(string Result)
        {
            this.Result = Result;
        }

        public string Result { get; }
    }
    
    public enum CaptureMode
    {
        Pieces,
        ShapeQR
    }

    public class InitializingEventArgs : EventArgs
    {
        public InitializingEventArgs(CaptureMode Mode)
        {
            this.Mode = Mode;
        }

        public CaptureMode Mode { get; }
    }

    /// <summary>
    /// CameraDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class Camera : UserControl
    {
        public event EventHandler<ContoursEventArgs> Recognized;
        public event EventHandler<QrReaderEventArgs> QrRecognized;
        public event EventHandler<InitializingEventArgs> Initializing;
        public event EventHandler CameraFixed;

        public double PieceCoefficient { get; private set; } = 1.0;
        public double PieceRotation { get; private set; } = 0;
        
        Mat PerspectiveTransform { get; set; }
        bool UsePerspective { get; set; } = false;

        Mat Intrinsic, Distortion;
        bool Locked;

        public System.Collections.ObjectModel.ObservableCollection<string> CalibrationFiles { get; }
            = new System.Collections.ObjectModel.ObservableCollection<string>();

        public Camera()
        {
            InitializeComponent();
            Grid.DataContext = this;
            UpdateCalibrations();

            CamT.Text = Config.Current.Camera.ToString();

            GammaS.Value = Config.Current.Gamma;
            SqArcS.Value = Config.Current.SquareMaximumArcLength;
            PiAreaS.Value = Config.Current.MinimumArea;
            ThreshS.Value = Config.Current.PieceApprox;
            SqThreshS.Value = Config.Current.SquareApprox;
            ThreshS.ValueChanged += (sender, e) => Config.Current.PieceApprox = ThreshS.Value;
            SqThreshS.ValueChanged += (sender, e) => Config.Current.SquareApprox = SqThreshS.Value;
            PiAreaS.ValueChanged += (sender, e) => Config.Current.MinimumArea = PiAreaS.Value;
            SqArcS.ValueChanged += (sender, e) => Config.Current.SquareMaximumArcLength = SqArcS.Value;
            GammaS.ValueChanged += (sender, e) => Config.Current.Gamma = GammaS.Value;

            KeyboardHook.HookEvent += OnKeyStateChanged;
        }

        private void ResetB_Click(object sender, RoutedEventArgs e)
        {
            GammaS.Value = CameraParams.Gamma;
            SqArcS.Value = CameraParams.SquareMaximumArcLength;
            PiAreaS.Value = CameraParams.MinimumArea;
            ThreshS.Value = CameraParams.PieceApprox;
            SqThreshS.Value = CameraParams.SquareApprox;
        }

        ICamera cam = null;
        public ICamera Capture
        {
            get { return cam; }
            set
            {
                if (!Locked)
                    cam = value;
                else
                    throw new Exception("ロック中です");
            }
        }

        void OnKeyStateChanged(ref KeyboardHook.StateKeyboard State)
        {
            if (Capture == null) return;

            if(State.Stroke == KeyboardHook.Stroke.KEY_UP)
            {
                if (State.Key == System.Windows.Forms.Keys.C)
                {
                    CaptureCurrent();
                }
                if (State.Key == System.Windows.Forms.Keys.F)
                {
                    using(Mat img = Capture.RetrieveMat(true))
                    {
                        FixCamera(img.Size(), ReduceContours(img).ToArray());
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

        #region ShapeQR

        private void BeginQrB_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(CamT.Text, out int dev) && dev > -1)
                Config.Current.Camera = dev;

            Initializing?.Invoke(this, new InitializingEventArgs(CaptureMode.ShapeQR));
            if (Capture == null) return;
            Locked = true;

            RefreshDirB.IsEnabled = false;
            CalibC.IsEnabled = false;
            StopB.IsEnabled = true;
            BeginB.IsEnabled = false;
            BeginQrB.IsEnabled = false;

            Capture.Filters.Clear();
            Capture.Interruptions.Clear();
            Capture.Drawings.Clear();

            Capture.Filters.Add(ApplyGamma);
            if (CalibC.SelectedIndex > 0)
            {
                FileStorage fs = new FileStorage(CalibC.SelectedItem.ToString(),
                    FileStorage.Mode.FormatXml | FileStorage.Mode.Read);
                Intrinsic = fs["Intrinsic"].ReadMat();
                Distortion = fs["Distortion"].ReadMat();
                Capture.Width = fs["Width"].ReadInt();
                Capture.Height = fs["Height"].ReadInt();
                fs.Dispose();

                Capture.Filters.Add(Calibrate);
            }

            Capture.Interruptions.Add(QrRecognize);
            Capture.Begin();
        }

        void QrRecognize(Mat Image)
        {
            string res = OpenCV.QR.Decoder.Decode(Image);

            if (!string.IsNullOrEmpty(res))
            {
                if (ShapeQRManager.AddShape(res))
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        QrRecognized?.Invoke(this, new QrReaderEventArgs(res));
                    }));
                }
            }
        }

        #endregion

        private void BeginB_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(CamT.Text, out int dev) && dev > -1)
                Config.Current.Camera = dev;

            Initializing?.Invoke(this, new InitializingEventArgs(CaptureMode.Pieces));
            if (Capture == null) return;
            Locked = true;

            RefreshDirB.IsEnabled = false;
            CalibC.IsEnabled = false;
            StopB.IsEnabled = true;
            BeginB.IsEnabled = false;
            BeginQrB.IsEnabled = false;

            Capture.Filters.Clear();
            Capture.Interruptions.Clear();
            Capture.Drawings.Clear();

            Capture.Filters.Add(ApplyGamma);
            if (CalibC.SelectedIndex > 0)
            {
                FileStorage fs = new FileStorage(CalibC.SelectedItem.ToString(),
                    FileStorage.Mode.FormatXml | FileStorage.Mode.Read);
                Intrinsic = fs["Intrinsic"].ReadMat();
                Distortion = fs["Distortion"].ReadMat();
                Capture.Width = fs["Width"].ReadInt();
                Capture.Height = fs["Height"].ReadInt();
                fs.Dispose();

                Capture.Filters.Add(Calibrate);
            }
            Capture.Drawings.Add(Contours);

            KeyboardHook.Start();
            Capture.Begin();
        }

        private void StopB_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        public void Stop()
        {
            try
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
                BeginQrB.IsEnabled = true;

                Locked = false;
                Capture?.Stop();
                Capture?.Dispose();
                Capture = null;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void RefreshDirB_Click(object sender, RoutedEventArgs e)
        {
            UpdateCalibrations();
        }

        /// <summary>
        /// ガンマ補正を行います
        /// </summary>
        /// <param name="Image"></param>
        /// <returns></returns>
        Mat ApplyGamma(Mat Image)
        {
            byte[] lut = new byte[256];
            for (int i = 0; 256 > i; i++)
            {
                lut[i] = (byte)(Math.Pow(i / 255.0, 1.0 / Config.Current.Gamma) * 255);
            }
            Mat gamma = new Mat();
            Cv2.LUT(Image, lut, gamma);
            Image.Dispose();
            return gamma;
        }

        /// <summary>
        /// 現在のフレームをキャプチャし、ピースを認識します
        /// </summary>
        private void CaptureCurrent()
        {
            using(Mat capture = Capture.RetrieveMat(true))
            {
                List<OpenCvSharp.Point[]> changed;
                if (CalibC.SelectedIndex > 0)
                {
                    using (Mat calib = Calibrate(capture))
                        changed = ReduceContours(calib);
                }
                else
                    changed = ReduceContours(capture);

                foreach (var ps in changed)
                {
                    Recognized?.Invoke(this, new ContoursEventArgs(ps,
                        PieceCoefficient, PieceRotation));
                }
            }
        }

        /// <summary>
        /// 四角形を輪郭から検出した後、カメラの歪みとピースとの距離を計算してスケールを取得します
        /// </summary>
        /// <param name="Size"></param>
        /// <param name="Contours"></param>
        public void FixCamera(OpenCvSharp.Size Size, OpenCvSharp.Point[][] Contours)
        {
            var sqpoints = SquareApprox(Size, Contours);
            if (sqpoints == null) return;
            sqpoints = AsAntiClockwise(sqpoints);

            double ratio = 0;
            for (int i = 0;4 > i; i++)
            {
                int ind = i + 1 == sqpoints.Length ? 0 : i + 1;
                ratio += 6 / sqpoints[i].DistanceTo(sqpoints[ind]);
            }
            PieceCoefficient = ratio / 4;

            OpenCvSharp.Point p1 = sqpoints[0], p2 = sqpoints[1];
            int x = Math.Abs(p1.X - p2.X), y = Math.Abs(p1.Y - p2.Y);

            PieceRotation = Math.Atan2(y, x);

            Log.Write("Scale : {0}, Rotation : {1}", PieceCoefficient, PieceRotation);

            CameraFixed?.Invoke(this, new EventArgs());

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

                PerspectiveTransform = Cv2.GetPerspectiveTransform(sqpoints.Select(p => (Point2f)p1), dst);
            }
        }

        /// <summary>
        /// 四角形の頂点を反時計回りにソートします
        /// </summary>
        /// <param name="Points"></param>
        /// <returns></returns>
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

        /// <summary>
        /// キャリブレーションを行います
        /// </summary>
        /// <param name="Image"></param>
        /// <returns></returns>
        Mat Calibrate(Mat Image)
        {
            if (Intrinsic == null || Distortion == null || Image == null) return null;
            using (Image)
            {
                Mat ret = new Mat();

                OpenCvSharp.Size size = Image.Size();
                Mat newMatrix = Cv2.GetOptimalNewCameraMatrix(Intrinsic, Distortion, size, 0, size, out _);
                Cv2.Undistort(Image, ret, Intrinsic, Distortion, newMatrix);
                newMatrix.Dispose();
                return ret;
            }
        }

        /// <summary>
        /// 輪郭を画像に描画します
        /// </summary>
        /// <param name="Image"></param>
        /// <returns></returns>
        Mat Contours(Mat Image)
        {
            if (Image == null) return null;

            var Contours = ReduceContours(Image);
            Cv2.DrawContours(Image, Contours, -1, Scalar.Red, 3);

            var Square = SquareApprox(Image.Size(), Contours.ToArray());

            if (Square != null)
                Cv2.DrawContours(Image, new OpenCvSharp.Point[][] { Square }, -1, Scalar.LightBlue, 3);
            return Image;
        }

        List<OpenCvSharp.Point[]> ReduceContours(Mat Image)
        {
            var contours = FindContours(Image);

            var Contours = PieceApprox(Image.Size(), contours);

            List<OpenCvSharp.Point[]> cs = new List<OpenCvSharp.Point[]>();

            var ps = Capture.RecognizerPoints.ToArray();
            foreach (var rec in ps)
            {
                double min = -1;
                int minind = -1;

                for (int i = 0; Contours.Count > i; i++)
                {
                    var contour = Contours[i];

                    if(Cv2.ContourArea(contour) >= Config.Current.MinimumArea)
                    {
                        foreach (var p in contour)
                        {
                            double diff = rec.DistanceTo(p);
                            if (min == -1 || diff < min)
                            {
                                min = diff;
                                minind = i;
                            }
                        }
                    }
                }

                if (minind > -1)
                    cs.Add(Contours[minind]);
            }
            return cs;
        }

        /// <summary>
        /// ピースを検出します
        /// </summary>
        /// <param name="Size"></param>
        /// <param name="Contours"></param>
        /// <returns></returns>
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
                    if(Cv2.ContourArea(points, false) >= Config.Current.MinimumArea)
                    {
                        double len = Cv2.ArcLength(points, true);
                        var approx = Cv2.ApproxPolyDP(points, Config.Current.PieceApprox * len, true);
                        changed.Add(approx);
                    }
                }
            }

            return changed.OrderByDescending((ps => Cv2.ArcLength(ps, true))).ToList();
        }

        /// <summary>
        /// 正方形を検出します
        /// </summary>
        /// <param name="Size"></param>
        /// <param name="Contours"></param>
        /// <param name="Thresh"></param>
        /// <returns></returns>
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

                    if(Cv2.ContourArea(points, false) >= Config.Current.MinimumArea && len <= Config.Current.SquareMaximumArcLength)
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

        private void PerspectiveC_StateChanged(object sender, RoutedEventArgs e)
        {
            UsePerspective = PerspectiveC.IsChecked ?? false;
        }

        /// <summary>
        /// 輪郭検出
        /// </summary>
        /// <param name="Image"></param>
        /// <returns></returns>
        OpenCvSharp.Point[][] FindContours(Mat Image)
        {
            if(PerspectiveTransform != null && !PerspectiveTransform.IsDisposed && UsePerspective)
            {
                Cv2.WarpPerspective(Image, Image, PerspectiveTransform, Image.Size());
            }

            Mat gray = new Mat();
            Cv2.CvtColor(Image, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);
            Cv2.MedianBlur(gray, gray, 3);
            //Cv2.FastNlMeansDenoising(gray, gray);
            Cv2.Threshold(gray, gray, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            Cv2.AdaptiveThreshold(gray, gray, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 11, 2);

            Cv2.ImWrite("test.jpg", gray);

            Cv2.FindContours(gray, out OpenCvSharp.Point[][] contours, out _,
                RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            gray.Dispose();
            gray = null;

            return contours;
        }
    }
}
