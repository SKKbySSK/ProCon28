﻿using System;
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
using ProCon28.Linker.Tcp;

namespace ProCon28.Controls
{
    /// <summary>
    /// TransferPiecesView.xaml の相互作用ロジック
    /// </summary>
    public partial class TransferPiecesView : UserControl
    {
        Server server;

        public event EventHandler<RoutedPieceEventArgs> RequestingPieces;

        public bool IsTransferring { get; private set; }

        public TransferPiecesView()
        {
            InitializeComponent();
            StopB.IsEnabled = false;
            IpLabel.Content = "Null";
            PortLabel.Content = "Null";
        }

        public void TransferPieces(Linker.PieceCollection Pieces)
        {
            if(server != null)
            {
                server.Dispose();
            }

            if (Pieces != null)
            {
                server = new Server(Config.Current.TCP_Port,
                    new RemotePieces { BytePieces = Pieces.AsBytes() },
                    Linker.Constants.RemoteRecognizerUri);

                server.Marshal();
                IpLabel.Content = server.ChannelIP;
                PortLabel.Content = Config.Current.TCP_Port;

                Log.Write("Transferring Pieces [{0}]", server.ChannelUri);

                BeginB.IsEnabled = false;
                StopB.IsEnabled = true;
                IsTransferring = true;
            }
        }

        private void BeginB_Click(object sender, RoutedEventArgs e)
        {
            Begin();
        }

        private void StopB_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        public void Begin()
        {
            if (server != null)
            {
                server.Dispose();
            }

            RoutedPieceEventArgs eventArgs = new RoutedPieceEventArgs();
            RequestingPieces?.Invoke(this, eventArgs);
            if (eventArgs.Pieces != null)
            {
                server = new Server(Config.Current.TCP_Port,
                    new RemotePieces { BytePieces = eventArgs.Pieces.AsBytes() },
                    Linker.Constants.RemoteRecognizerUri);

                server.Marshal();
                IpLabel.Content = server.ChannelIP;
                PortLabel.Content = Config.Current.TCP_Port;

                Log.Write("Transferring Pieces [{0}]", server.ChannelUri);

                BeginB.IsEnabled = false;
                StopB.IsEnabled = true;
                IsTransferring = true;
            }
        }

        public void Stop()
        {
            server?.Dispose();
            server = null;

            IpLabel.Content = "Null";
            PortLabel.Content = "Null";

            BeginB.IsEnabled = true;
            StopB.IsEnabled = false;
            IsTransferring = false;
        }
    }

    public class RoutedPieceEventArgs : EventArgs
    {
        public Linker.PieceCollection Pieces { get; set; }
    }
}
