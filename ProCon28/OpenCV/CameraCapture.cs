using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ProCon28.OpenCV
{
    public class CameraCapture : ICamera
    {
        bool disposing = false;
        bool retrieve = false;
        Task captureTask;
        Window window;
        VideoCapture capture;
        double gammaparam = 1.0;

        public event EventHandler MouseClicked;

        public CameraCapture(int Device, string Window)
        {
            capture = new VideoCapture(Device);
            window = new Window(Window);
            window.OnMouseCallback += Window_OnMouseCallback;

            captureTask = Task.Run(action: Capture);
            Log.Write("OpenCV Initialized [Camera : {0}, Window : {1}]", Device, Window);
        }

        public void UseGammaOptimization()
        {
            AddSlider("Gamma", (int)(gammaparam * 100), 500, (val) =>
            {
                gammaparam = val / 100.0;
                if (gammaparam == 0)
                    gammaparam = 0.01;
            });
            Filters.Add(Gamma);
        }

        Mat Gamma(Mat Image)
        {
            byte[] lut = new byte[256];
            for(int i = 0; 256 > i; i++)
            {
                lut[i] = (byte)(Math.Pow(i / 255.0, 1.0 / gammaparam) * 255);
            }
            Mat gamma = new Mat();
            Cv2.LUT(Image, lut, gamma);
            Image.Dispose();
            return gamma;
        }

        private void Window_OnMouseCallback(MouseEvent @event, int x, int y, MouseEvent flags)
        {
            switch (@event)
            {
                case MouseEvent.LButtonDown:
                    MouseClicked?.Invoke(this, new EventArgs());
                    break;
            }
        }

        public void Begin()
        {
            retrieve = true;
        }

        public void Stop()
        {
            retrieve = false;
        }

        /// <summary>
        /// カメラから新しく画像を取得します
        /// </summary>
        /// <returns></returns>
        public Mat RetrieveMat(bool UseFilters)
        {
            if (disposing)
                return null;

            Mat mat = capture.RetrieveMat();

            if (UseFilters)
            {
                foreach (var filter in Filters)
                    mat = filter(mat);
            }

            return mat;
        }

        /// <summary>
        /// 現在のウィンドウに表示されている画像を取得します
        /// </summary>
        /// <returns></returns>
        public Mat CurrentMat()
        {
            retrieve = false;
            while (window.Image == null) ;
            Mat cpy = new Mat();
            window.Image.CopyTo(cpy);
            return cpy;
        }

        void Capture()
        {
            while (!disposing)
            {
                if (retrieve && window != null)
                {
                    if (!window.IsDisposed)
                    {
                        if(window.Image != null)
                        {
                            window.Image.Dispose();
                            window.Image = null;
                        }

                        Mat img = capture.RetrieveMat();

                        foreach (var filter in Filters)
                            img = filter(img);
                        foreach (Action<Mat> interrupt in Interruptions)
                            interrupt(img);

                        window.Image = img;
                    }
                }
            }

            Log.Write("Disposing the device...");
            Window.DestroyAllWindows();
            if (window != null)
            {
                window.Image?.Dispose();
                window.Dispose();
                window = null;
            }
            Log.Write("Device was disposed");
        }

        public IList<Action<Mat>> Interruptions { get; } = new List<Action<Mat>>();
        public IList<Func<Mat, Mat>> Filters { get; } = new List<Func<Mat, Mat>>();
        public int Width { get => capture.FrameWidth; set => capture.FrameWidth = value; }
        public int Height { get => capture.FrameHeight; set => capture.FrameHeight = value; }

        public IList<Point> RecognizerPoints { get; } = new List<Point>();

        public void AddSlider(string Name, int Value, int Max, Action<int> ValueChanged)
        {
            window.CreateTrackbar(Name, Value, Max, new CvTrackbarCallback(ValueChanged));
        }

        public void Dispose()
        {
            try
            {
                disposing = true;

                captureTask.Wait();
                captureTask = null;

                Interruptions.Clear();
                Filters.Clear();

                capture?.Dispose();
                capture = null;
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}
