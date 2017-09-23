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
using System.Windows.Shapes;
using ProCon28.Linker.Tcp;

namespace ProCon28.Windows
{
    /// <summary>
    /// MarshalDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class MarshalDialog : Window
    {
        Server server;
        MarshalByRefObject obj;

        public MarshalDialog(MarshalByRefObject Obj)
        {
            InitializeComponent();
            Closing += MarshalDialog_Closing;
            obj = Obj;
            Marshal();
        }

        private void MarshalDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            server?.Dispose();
            server = null;
            obj = null;
        }

        void Marshal()
        {
            server = new Server(Config.Current.TCP_Port, obj, Linker.Constants.RemotePiecesUri);
            server.Marshal();

            string[] urls = server.GetUrls();

            string line = string.Format("公開中...\n\n公開アドレス\n{0}", string.Join("\n", urls));
            DescL.Content = line;
        }

        private void RetryB_Click(object sender, RoutedEventArgs e)
        {
            DescL.Content = "再試行中";
            server?.Dispose();
            server = null;
            Marshal();
        }

        private void CancelB_Click(object sender, RoutedEventArgs e)
        {
            server?.Dispose();
            server = null;
            DescL.Content = "停止";
        }
    }
}
