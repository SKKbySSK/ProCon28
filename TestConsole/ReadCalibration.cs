using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Windows.Forms;

namespace TestConsole
{
    class ReadCalibration : IConsole
    {
        public string Title { get; } = "キャリブレーション";

        public void Run()
        {
            string fname = "";
            Console.WriteLine("ファイル名(Calibration.xml)");
            string read = Console.ReadLine();
            fname = string.IsNullOrEmpty(read) ? "Calibration.xml" : read;
            FileStorage fs = new FileStorage(fname, FileStorage.Mode.FormatXml | FileStorage.Mode.Read);
            Mat intrinsic = fs["Intrinsic"].ReadMat();
            Mat distortion = fs["Distortion"].ReadMat();
            fs.Dispose();

            Console.WriteLine("読み込みに成功しました");

            do
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (Mat mat = Cv2.ImRead(ofd.FileName))
                    using (Mat undis = new Mat())
                    {
                        Window w = new Window("Undisort");

                        Cv2.Undistort(mat, undis, intrinsic, distortion);
                        w.ShowImage(undis);
                        Cv2.WaitKey(0);

                        w.Close();
                        w.Dispose();
                        w = null;
                    }
                }
                Console.WriteLine("もう一度行いますか?");
            } while (Console.ReadLine().ToUpper() == "Y");

            intrinsic.Dispose();
            distortion.Dispose();
        }
    }
}
