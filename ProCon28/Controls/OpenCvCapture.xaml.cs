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
            MatParent.Width = Worker.Width;
            MatParent.Height = Worker.Height;
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
                Last = e.Image;
            }
        }

        private void MatView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MatView);
            Worker?.RecognizerPoints.Add(new OpenCvSharp.Point(Math.Round(pos.X), Math.Round(pos.Y)));
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

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!ldrag) return;

            Point mouse = e.GetPosition(MatView);
            Point diff = new Point(mouse.X - lastpos.X, mouse.Y - lastpos.Y);
            double l = 0, t = 0, w = 0, h = 0;

            if (diff.X >= 0 && diff.Y >= 0)
            {
                l = lastpos.X;
                t = lastpos.Y;
                w = diff.X;
                h = diff.Y;
            }
            else if (diff.X >= 0 && diff.Y <= 0)
            {
                l = lastpos.X;
                t = mouse.Y;
                w = diff.X;
                h = -diff.Y;
            }
            else if (diff.X <= 0 && diff.Y >= 0)
            {
                l = mouse.X;
                t = lastpos.Y;
                w = -diff.X;
                h = diff.Y;
            }
            else
            {
                l = mouse.X;
                t = mouse.Y;
                w = -diff.X;
                h = -diff.Y;
            }

            MatCrop.Margin = new Thickness(l, t, 0, 0);
            MatCrop.Width = w;
            MatCrop.Height = h;
        }
    }
}
