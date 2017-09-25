using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;

namespace TestConsole
{
    public partial class CalibDialog : Form
    {
        int i = 0, times = 0;
        ProCon28.OpenCV.CameraCapture Camera;

        public CalibDialog(ProCon28.OpenCV.CameraCapture Camera, int Times)
        {
            InitializeComponent();
            this.Camera = Camera;
            Camera.Begin();
            times = Times;

            if (i == times) Close();
            Console.WriteLine("{0}回目{1}", i + 1, i + 1 == Times ? "(最後)" : "");
        }

        private void CalibDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            Camera.Stop();
            Camera.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using(Mat mat = Camera.RetrieveMat())
            {
                mat.SaveImage(string.Format("{0:0000}.jpg", i));
            }
            if (i == times) Close();
            i++;
            Console.WriteLine("{0}回目{1}", i + 1, i + 1 == times ? "(最後)" : "");
        }
    }
}
