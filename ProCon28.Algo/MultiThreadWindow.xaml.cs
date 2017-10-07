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
using ProCon28.Linker.Extensions;

namespace ProCon28.Algo
{
    /// <summary>
    /// MultiThreadWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MultiThreadWindow : Window
    {
        List<(AlgoInfo, TabPage)> Tasks = new List<(AlgoInfo, TabPage)>();
        Linker.PieceCollection PieceCollection;

        public MultiThreadWindow(Linker.PieceCollection Pieces)
        {
            InitializeComponent();
            PieceCollection = Pieces;

            BasePiecesView.Pieces = Pieces;
            RetryT.Text = Config.Current.MT_Retry.ToString();
            ProcessT.Text = Config.Current.MT_Limitation.ToString();
        }

        private void BeginB_Click(object sender, RoutedEventArgs e)
        {
            Stop();

            if (int.TryParse(RetryT.Text, out int retry))
                Config.Current.MT_Retry = retry;
            if (int.TryParse(ProcessT.Text, out int limit))
                Config.Current.MT_Limitation = limit;

            for(int i = 0;Config.Current.MT_Limitation > i; i++)
            {
                Algorithm algo = new Algorithm(ClonePieces(), Dispatcher);
                algo.Sleeping += Algo_Sleeping;
                AlgoInfo info = new AlgoInfo(algo, Dispatcher);
                Action<AlgoInfo, Linker.PieceCollection> prog = (a, ps) =>
                {
                    AddTab(a, ps);
                };

                info.Finished = prog;
                info.Progress = prog;
                info.Begin();
                Tasks.Add((info, AddTab(info, algo.PieceCollection)));
            }
        }

        private void Algo_Sleeping(object sender, RoutedSleepEventArgs e)
        {
            foreach(var cp in e.TempResults)
            {
                IList<Linker.Piece> ps = cp.Source.RemoveIncorrectPieces();
                cp.Source = ps;
            }

            Random rnd = new Random();
            e.Index = rnd.Next(e.TempResults.Length);
            e.Wait = false;
        }

        Linker.PieceCollection ClonePieces()
        {
            Linker.PieceCollection pcol = new Linker.PieceCollection();
            foreach (var p in PieceCollection)
                pcol.Add((Linker.Piece)p.Clone());
            return pcol;
        }

        TabPage AddTab(AlgoInfo Info, IList<Linker.Piece> Pieces)
        {
            foreach(TabPage tb in ResultTab.Items)
            {
                if(tb.Info == Info)
                {
                    tb.Header = Pieces.Count + "個";
                    tb.Pieces = Pieces;
                    return tb;
                }
            }

            TabPage tp = new TabPage();
            tp.Header = Pieces.Count + "個";
            tp.Pieces = Pieces;
            tp.Info = Info;
            ResultTab.Items.Add(tp);
            return tp;
        }

        private void CancelB_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        void Stop()
        {
            foreach (var val in Tasks)
            {
                val.Item1.Cancel = true;
            }
            Task.WhenAll(Tasks.Select(t => t.Item1.CurrentTask));
            Tasks.Clear();
        }
    }

    class TabPage : TabItem
    {
        Views.PieceCollectionView pcolv = new Views.PieceCollectionView();
        public TabPage()
        {
            Content = pcolv;
        }

        public AlgoInfo Info { get; set; }
        Linker.PieceCollection ps = new Linker.PieceCollection();
        public IList<Linker.Piece> Pieces
        {
            get { return pcolv.Pieces; }
            set { pcolv.Pieces = value; }
        }
    }

    class AlgoInfo
    {
        public AlgoInfo(Algorithm Algo, System.Windows.Threading.Dispatcher Dispatcher)
        {
            Algorithm = Algo;
            this.Dispatcher = Dispatcher;
        }

        public Algorithm Algorithm { get; }
        public int TryCount { get; private set; } = 0;
        public Action<AlgoInfo, Linker.PieceCollection> Finished { get; set; }
        public Action<AlgoInfo, Linker.PieceCollection> Progress { get; set; }
        private System.Windows.Threading.Dispatcher Dispatcher;
        public Task CurrentTask { get; private set; }

        public bool Cancel { get; set; } = false;

        public void Begin()
        {
            if (CurrentTask != null) return;

            CurrentTask = Task.Run(() =>
            {
                int lastcount = Algorithm.PieceCollection.Count;
                for(int i = 0;Config.Current.MT_Limitation > i; i++)
                {
                    if (Cancel)
                        break;

                    Algorithm.search();
                    if(Progress != null)
                    {
                        Dispatcher.BeginInvoke(new Action(() => Progress(this, Algorithm.PieceCollection)));
                    }

                    if (Cancel)
                        break;

                    int count = Algorithm.PieceCollection.Count;
                    if (count == 1)
                        break;
                    else if(count != lastcount)
                    {
                        if (Cancel)
                            break;
                        lastcount = count;
                        i = 0;
                    }
                }

                if(Finished != null)
                {
                    Dispatcher.BeginInvoke(new Action(() => Finished(this, Algorithm.PieceCollection)));
                }
            });
        }
    }
}
