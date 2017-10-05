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

namespace ProCon28.Algo
{
    /// <summary>
    /// MultiThreadWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MultiThreadWindow : Window
    {
        List<(Task, Algorithm)> Tasks = new List<(Task, Algorithm)>();
        Linker.PieceCollection PieceCollection;

        public MultiThreadWindow(Linker.PieceCollection Pieces)
        {
            InitializeComponent();
            PieceCollection = Pieces;

            RetryT.Text = Config.Current.MT_Retry.ToString();
            ProcessT.Text = Config.Current.MT_Limitation.ToString();
        }

        private void BeginB_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(RetryT.Text, out int retry))
                Config.Current.MT_Retry = retry;
            if (int.TryParse(ProcessT.Text, out int limit))
                Config.Current.MT_Limitation = limit;

            for(int i = 0;Config.Current.MT_Limitation > i; i++)
            {
                Algorithm algo = new Algorithm(ClonePieces(), Dispatcher);
                Tasks.Add((algo.SearchAsync(), algo));
            }
        }

        Linker.PieceCollection ClonePieces()
        {
            Linker.PieceCollection pcol = new Linker.PieceCollection();
            foreach (var p in PieceCollection)
                pcol.Add((Linker.Piece)p.Clone());
            return pcol;
        }
    }
}
