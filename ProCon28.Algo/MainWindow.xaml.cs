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
using System.ComponentModel;

namespace ProCon28.Algo
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Algorithm algo;
        RoutedSleepEventArgs _ev;
        bool constructing = true;

        public MainWindow()
        {
            InitializeComponent();
            
            PieceView.Pieces = Shared.Pieces;
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

            foreach(Piece p in Shared.Pieces)
            {
                if (!(p is Linker.Frame))
                    pcol.Add(p);
            }

            algo?.Abort();
            algo = new Algorithm(pcol, Dispatcher);
            algo.Bonded += Algorithm_Bonded;
            algo.Sleeping += Algorithm_Sleeping;
            algo.SearchCanBondPiecePair();
        }

        private void Algorithm_Sleeping(object sender, RoutedSleepEventArgs e)
        {
            TempPieces.Pieces.Clear();
            TempPieces.Pieces.AddRange(e.TempResults);
            _ev = e;
        }

        private void Algorithm_Bonded(object sender, BondEventArgs e)
        {
            Shared.Pieces.Clear();
            Shared.Pieces.AddRange(e.Pieces);
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

        private void RemoveI_Click(object sender, RoutedEventArgs e)
        {
            if (PieceView.SelectedPiece != null) Shared.Pieces.Remove(PieceView.SelectedPiece);
        }

        private void ClearB_Click(object sender, RoutedEventArgs e)
        {
            Shared.Pieces.Clear();
        }

        private void TempPieces_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(TempPieces.SelectedPiece != null)
            {
                _ev.Index = TempPieces.SelectedIndex;
                _ev.Wait = false;
            }
        }

        private void AbortB_Click(object sender, RoutedEventArgs e)
        {
            if (_ev != null)
            {
                _ev.Index = -1;
                _ev.Wait = false;
            }
        }

        private void auto_Click(object sender, RoutedEventArgs e)
        {
            MultiThreadWindow multiThreadWindow = new MultiThreadWindow(Shared.Pieces);
            multiThreadWindow.Show();
        }
    }
}
