using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ProCon28.OpenCV
{
    class CameraCapture : IDisposable
    {
        bool disposing = false;
        bool retrieve = false;
        Task captureTask;
        Window window;
        VideoCapture capture;

        public CameraCapture(int Device, string Window)
        {
            capture = new VideoCapture(Device);
            window = new Window(Window);

            captureTask = Task.Run(action: Capture);
            Log.WriteLine("OpenCV Initialized [Camera : {0}, Window : {1}]", Device, Window);
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
        public Mat RetrieveMat()
        {
            return capture.RetrieveMat();
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
