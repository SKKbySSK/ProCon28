using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ProCon28.OpenCV;
using ProCon28.OpenCV.QR;

namespace ProCon28.Windows
{
    /// <summary>
    /// QrReader.xaml の相互作用ロジック
    /// </summary>
    public partial class QrReader : Window
    {
        string[] res;
        public string[] Result
        {
            get { return res; }
            private set
            {
                res = value;
                ResultChanged?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler ResultChanged;

        CameraCapture cc = null;
        bool Multiple;
        bool Auto;

        public QrReader()
        {
            InitializeComponent();
        }

        private void BeginB_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(DeviceNumT.Text, out int dev))
                Config.Current.Camera = dev;

            if (cc != null) return;

            MultipleC.IsEnabled = false;
            AutoCloseC.IsEnabled = false;
            Multiple = MultipleC.IsChecked ?? false;
            Auto = AutoCloseC.IsChecked ?? false;

            Decoder.Reader.TryInverted = InvertC.IsChecked ?? false;
            Decoder.Reader.AutoRotate = RotateC.IsChecked ?? false;

            cc = new CameraCapture(Config.Current.Camera, "QR");
            cc.Interruptions.Add(GetResults);
            cc.Begin();
        }

        private void StopB_Click(object sender, RoutedEventArgs e)
        {
            if(cc != null)
            {
                cc.Stop();
                cc.Dispose();
                cc = null;
            }

            MultipleC.IsEnabled = true;
            AutoCloseC.IsEnabled = true;
        }

        void GetResults(OpenCvSharp.Mat Mat)
        {
            if (Multiple)
            {
                string[] res = Decoder.DecodeMultiple(Mat);

                if(res.Length > 0)
                {
                    if (Auto)
                    {
                        Result = res;
                    }
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ResultT.Text = "";
                        foreach (string r in res)
                        {
                            ResultT.Text += r + "\r\n";
                        }
                    }));
                }
            }
            else
            {
                string res = Decoder.Decode(Mat);

                if (res != "")
                {
                    if (Auto)
                    {
                        Result = new string[] { res };
                    }
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ResultT.Text = res;
                    }));
                }
            }
        }

        private void InvertC_Checked(object sender, RoutedEventArgs e)
        {
            Decoder.Reader.TryInverted = true;
        }

        private void InvertC_Unchecked(object sender, RoutedEventArgs e)
        {
            Decoder.Reader.TryInverted = false;
        }

        private void RotateC_Checked(object sender, RoutedEventArgs e)
        {
            Decoder.Reader.AutoRotate = true;
        }

        private void RotateC_Unchecked(object sender, RoutedEventArgs e)
        {
            Decoder.Reader.AutoRotate = false;
        }

        private void ConfirmB_Click(object sender, RoutedEventArgs e)
        {
            Result = ResultT.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cc != null)
            {
                cc.Stop();
                cc.Dispose();
                cc = null;
            }
        }
    }
}
