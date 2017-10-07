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
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenCvSharp.Extensions;

namespace ProCon28.Controls
{
    /// <summary>
    /// OpenCvCapture.xaml の相互作用ロジック
    /// </summary>
    public partial class OpenCvCapture : UserControl
    {
        OpenCV.WorkerCapture Worker;
        bool ldrag = false;
        System.Windows.Point lastpos;

        public event EventHandler WorkerFinished;

        public OpenCvCapture()
        {
            InitializeComponent();
            KeyboardHook.HookEvent += KeyboardHook_HookEvent;
        }

        bool _rec = true;
        public bool UseRecognizerPoints
        {
            get { return _rec; }
            set
            {
                _rec = value;
                if (!value) Worker?.RecognizerPoints.Clear();
            }
        }

        private void KeyboardHook_HookEvent(ref KeyboardHook.StateKeyboard state)
        {
            if(state.Stroke == KeyboardHook.Stroke.KEY_UP)
            {
                if(state.Key == System.Windows.Forms.Keys.R)
                {
                    Worker?.RecognizerPoints.Clear();
                }
            }
        }

        public void Initialize(OpenCV.WorkerCapture Worker)
        {
            this.Worker = Worker;
            Worker.Retrieved += Worker_Retrieved;
            Worker.Finished += Worker_Finished;
            KeyboardHook.Start();
        }

        private void Worker_Finished(object sender, EventArgs e)
        {
            Worker.Retrieved -= Worker_Retrieved;
            Worker.Finished -= Worker_Finished;
            Worker = null;
            MatView.Source = null;
            Last?.Dispose();
            Last = null;

            KeyboardHook.Stop();
            WorkerFinished?.Invoke(this, new EventArgs());
        }

        OpenCvSharp.Mat Last;
        private void Worker_Retrieved(object sender, OpenCV.RetrievedEventArgs e)
        {
            if(Last != null)
            {
                Last.Dispose();
                Last = null;
            }

            if(e.Image != null)
            {
                foreach(var pos in Worker.RecognizerPoints)
                {
                    OpenCvSharp.Cv2.Circle(e.Image, pos, 5, OpenCvSharp.Scalar.Blue, 3);
                }
                var img = e.Image.ToBitmapSource();

                MatView.Source = img;
                MatView.Width = img.Width;
                MatView.Height = img.Height;
                Last = e.Image;
            }
        }

        private void MatView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (UseRecognizerPoints)
            {
                var pos = e.GetPosition(MatView);
                Worker?.RecognizerPoints.Add(new OpenCvSharp.Point(Math.Round(pos.X), Math.Round(pos.Y)));
            }
        }

        private void MatView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Worker?.RecognizerPoints.Clear();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                ldrag = true;
                lastpos = e.GetPosition(MatView);
                Grid.CaptureMouse();
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Released && ldrag)
            {
                Grid.ReleaseMouseCapture();
                ldrag = false;
            }
        }
    }
}
