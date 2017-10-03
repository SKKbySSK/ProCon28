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

        public event EventHandler WorkerFinished;

        public OpenCvCapture()
        {
            InitializeComponent();
        }

        public void Initialize(OpenCV.WorkerCapture Worker)
        {
            this.Worker = Worker;
            Worker.Retrieved += Worker_Retrieved;
            Worker.Finished += Worker_Finished;
        }

        private void Worker_Finished(object sender, EventArgs e)
        {
            Worker.Retrieved -= Worker_Retrieved;
            Worker.Finished -= Worker_Finished;
            MatView.Source = null;
            Last?.Dispose();
            Last = null;
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
                var img = e.Image.ToBitmapSource();
                MatView.Source = img;
                Last = e.Image;
            }
        }
    }
}
