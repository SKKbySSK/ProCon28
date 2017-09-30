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
            ItemHS.Value = Config.Current.ItemHeight;
            ItemWS.Value = Config.Current.ItemWidth;

            IpBox.Text = Config.Current.TCP_IP;
            PortBox.Text = Config.Current.TCP_Port.ToString();
            constructing = false;
        }

        private void import_Click(object sender, RoutedEventArgs e)
        {
            //これ使ってね
            PieceCollection pcol = new PieceCollection();

            Algorithm AlgoLithm = new Algorithm(pcol);
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
    }
}
