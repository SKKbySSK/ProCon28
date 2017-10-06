using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using OpenCvSharp;

namespace ProCon28.OpenCV
{
    public class RetrievedEventArgs : EventArgs
    {
        public RetrievedEventArgs(Mat Image)
        {
            this.Image = Image;
        }

        public Mat Image { get; }
    }

    public class WorkerCapture : ICamera
    {
        bool retr = false;
        bool disposing = false;
        VideoCapture capture;

        BackgroundWorker worker = new BackgroundWorker();

        public IList<Action<Mat>> Interruptions { get; } = new List<Action<Mat>>();
        public IList<Func<Mat, Mat>> Filters { get; } = new List<Func<Mat, Mat>>();
        public int Width { get => capture.FrameWidth; set => capture.FrameWidth = value; }
        public int Height { get => capture.FrameHeight; set => capture.FrameHeight = value; }

        public IList<Point> RecognizerPoints { get; } = new List<Point>();

        public event EventHandler<RetrievedEventArgs> Retrieved;
        public event EventHandler Finished;

        public WorkerCapture(int Device)
        {
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = false;

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.ProgressChanged += Worker_ProgressChanged;

            capture = new VideoCapture(Device);

            worker.RunWorkerAsync();

            Log.Write("OpenCV Initialized [Camera : {0}]", Device);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.UserState != null)
            {
                Retrieved?.Invoke(this, new RetrievedEventArgs((Mat)e.UserState));
                ((IDisposable)e.UserState).Dispose();
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            worker.Dispose();
            Log.Write("Background Worker Disposed");
            Finished?.Invoke(this, new EventArgs());
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
        
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!disposing)
            {
                if (retr)
                {
                    Mat img = capture.RetrieveMat();

                    foreach (var filter in Filters)
                        img = filter(img);
                    foreach (Action<Mat> interrupt in Interruptions)
                        interrupt(img);

                    worker.ReportProgress(0, img);
                }
            }

            capture?.Dispose();
            capture = null;
            Log.Write("Capture Device Disposed");
        }

        public void Begin()
        {
            retr = true;
        }

        public void Stop()
        {
            retr = false;
        }

        public void Dispose()
        {
            try
            {
                disposing = true;

                capture?.Dispose();
                capture = null;
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }

            GC.SuppressFinalize(this);
        }
    }
}
