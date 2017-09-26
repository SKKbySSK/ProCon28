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

namespace ProCon28.Windows
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
    public partial class CameraDialog : System.Windows.Window
    {
        public event EventHandler<ContoursEventArgs> Recognized;

        Mat Intrinsic, Distortion;

        public System.Collections.ObjectModel.ObservableCollection<string> CalibrationFiles { get; }
            = new System.Collections.ObjectModel.ObservableCollection<string>();

        CameraCapture camera;

        public CameraDialog()
        {
            InitializeComponent();
            DataContext = this;
            UpdateCalibrations();
        }

        void UpdateCalibrations()
        {
            const string CalibDir = "Calibration";
            System.IO.Directory.CreateDirectory(CalibDir);

            string[] files = System.IO.Directory.GetFiles(CalibDir);
            CalibrationFiles.Clear();
            CalibrationFiles.Add("無効");
            CalibrationFiles.AddRange(files);

            CalibC.SelectedIndex = files.Length > 0 ? 1 : 0;
        }

        private void BeginB_Click(object sender, RoutedEventArgs e)
        {
            RefreshDirB.IsEnabled = false;
            CalibC.IsEnabled = false;
            StopB.IsEnabled = true;
            BeginB.IsEnabled = false;
            CameraOperationGrid.IsEnabled = true;

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
                double max_len = -1;
                int max_contour = 0;

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
                    if (!f) changed.Add(points);
                }

                int count = changed.Count;
                for (int i = 0; count > i; i++)
                {
                    double length = Cv2.ArcLength(changed[i], true);
                    if (length > max_len)
                    {
                        max_len = length;
                        max_contour = i;
                    }
                }

                if (count > 0)
                    Recognized?.Invoke(this, new ContoursEventArgs(changed[max_contour], ScaleS.Value));
            }
        }

        Mat Calibrate(Mat Image)
        {
            using (Image)
            {
                Mat ret = new Mat();

                OpenCvSharp.Size size = Image.Size();
                Mat newMatrix = Cv2.GetOptimalNewCameraMatrix(Intrinsic, Distortion, size, 0, size, out _);
                Cv2.Undistort(Image, ret, Intrinsic, Distortion, newMatrix);
                return ret;
            }
        }
    }
}
