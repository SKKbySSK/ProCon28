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

namespace ProCon28.Controls
{

    /// <summary>
    /// QrReader.xaml の相互作用ロジック
    /// </summary>
    public partial class QrReader : UserControl
    {
        public event EventHandler<QrReaderEventArgs> Recognized;

        OpenCV.CameraCapture camera;

        public QrReader()
        {
            InitializeComponent();
            DeviceT.Text = Config.Current.Camera.ToString();
        }

        private void BeginB_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DeviceT.Text, out int dev))
                Config.Current.Camera = dev;

            camera = new OpenCV.CameraCapture(dev, "QR Reader");
            camera.Interruptions.Add(Recognize);
            camera.Begin();

            BeginB.IsEnabled = false;
            StopB.IsEnabled = true;
        }

        void Recognize(OpenCvSharp.Mat Image)
        {
            string res = OpenCV.QR.Decoder.Decode(Image);

            if (!string.IsNullOrEmpty(res))
            {
                if (ShapeQRManager.AddShape(res))
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Recognized?.Invoke(this, new QrReaderEventArgs(res));
                    }));
                }
            }
        }

        private void StopB_Click(object sender, RoutedEventArgs e)
        {
            camera?.Dispose();
            camera = null;

            BeginB.IsEnabled = true;
            StopB.IsEnabled = false;
        }
    }
}
