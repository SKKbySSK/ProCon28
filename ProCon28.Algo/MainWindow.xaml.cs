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
using ProCon28.Linker.Extensions;
using ProCon28.Linker;
using ProCon28.Algo.Line;
using OFD = System.Windows.Forms.OpenFileDialog;
using System.IO;

namespace ProCon28.Algo
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        bool constructing = true;

        public MainWindow()
        {
            InitializeComponent();

            PieceView.Pieces = Shared.Pieces;
            PiecesInfo.ItemsSource = Shared.Pieces;
            ItemHS.Value = Config.Current.ItemHeight;
            ItemWS.Value = Config.Current.ItemWidth;

            IpBox.Text = Config.Current.TCP_IP;
            PortBox.Text = Config.Current.TCP_Port.ToString();
            FilepathT.Text = Config.Current.LastFilePath;
            constructing = false;
        }

        private void import_Click(object sender, RoutedEventArgs e)
        {
            PieceCollection pcol = new PieceCollection();
            pcol.AddRange(Shared.Pieces);

            Algorithm Algorithm = new Algorithm(pcol);
            Shared.Pieces.AddRange(Algorithm.SearchCanBondPiecePair());
        }

        private void ItemWS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PieceView.ItemWidth.Value = ItemWS.Value;

            if (constructing) return;
            Config.Current.ItemWidth = ItemWS.Value;
        }

        private void ItemHS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PieceView.ItemHeight.Value = ItemHS.Value;

            if (constructing) return;
            Config.Current.ItemHeight = ItemHS.Value;
        }

        private void RecB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Config.Current.TCP_IP = IpBox.Text;
                if (int.TryParse(PortBox.Text, out int port))
                    Config.Current.TCP_Port = port;

                using (Linker.Tcp.Client client = new Linker.Tcp.Client(Config.Current.TCP_IP, Config.Current.TCP_Port, Constants.RemoteRecognizerUri))
                {
                    var rps = client.GetObject<Linker.Tcp.RemotePieces>();
                    if (rps != null)
                    {
                        Shared.Pieces.AddRange(new PieceCollection(rps.BytePieces));
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void RefFileB_Click(object sender, RoutedEventArgs e)
        {
            OFD ofd = new OFD();
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FilepathT.Text = ofd.FileName;
            }
        }

        private void LoadB_Click(object sender, RoutedEventArgs e)
        {
            using (FileStream fs = new FileStream(FilepathT.Text, FileMode.Open, FileAccess.Read))
            {
                byte[] bs = new byte[fs.Length];
                fs.Read(bs, 0, (int)fs.Length);
                PieceCollection pcol = new PieceCollection(bs);
                Shared.Pieces.AddRange(pcol);
                Config.Current.LastFilePath = FilepathT.Text;
            }
        }
    }
}
