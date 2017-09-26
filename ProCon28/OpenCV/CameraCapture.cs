using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ProCon28.OpenCV
{
    public class CameraCapture : IDisposable
    {
        bool disposing = false;
        bool retrieve = false;
        Task captureTask;
        Window window;
        VideoCapture capture;

        public event EventHandler MouseClicked;

        public CameraCapture(int Device, string Window)
        {
            capture = new VideoCapture(Device);
            window = new Window(Window);
            window.OnMouseCallback += Window_OnMouseCallback;

            captureTask = Task.Run(action: Capture);
            Log.WriteLine("OpenCV Initialized [Camera : {0}, Window : {1}]", Device, Window);
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

            Log.WriteLine("Disposing the device...");
            Window.DestroyAllWindows();
            if (window != null)
            {
                window.Image?.Dispose();
                window.Dispose();
                window = null;
            }
            Log.WriteLine("Device was disposed");
        }

        public IList<Action<Mat>> Interruptions { get; } = new List<Action<Mat>>();
        public IList<Func<Mat, Mat>> Filters { get; } = new List<Func<Mat, Mat>>();

        public void Dispose()
        {
            disposing = true;
            captureTask.Wait();
            captureTask = null;
            capture?.Dispose();
            capture = null;
        }
    }
}
