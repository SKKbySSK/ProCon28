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
        int completed = 0;

        public MultiThreadWindow(Linker.PieceCollection Pieces)
        {
            InitializeComponent();
            PieceCollection = Pieces;

            BasePiecesView.Pieces = Pieces;
            RetryT.Text = Config.Current.MT_Retry.ToString();
            ProcessT.Text = Config.Current.MT_Limitation.ToString();
            KeyboardHook.HookEvent += KeyboardHook_HookEvent;
        }

        private void BeginB_Click(object sender, RoutedEventArgs e)
        {
            completed = 0;
            Stop();

            //KeyboardHook.Start();

            if (int.TryParse(RetryT.Text, out int retry))
                Config.Current.MT_Retry = retry;
            if (int.TryParse(ProcessT.Text, out int limit))
                Config.Current.MT_Limitation = limit;

            Action<AlgoInfo, Linker.PieceCollection> prog = (a, ps) =>
            {
                AddTab(a, ps);
                TaskStateL.Content = string.Format("完了:{0}個, 進行中:{1}個", completed, Config.Current.MT_Limitation - completed);
            };
            Action<AlgoInfo, Linker.PieceCollection> fin = (a, ps) =>
            {
                AddTab(a, ps);
                completed++;
                TaskStateL.Content = string.Format("完了:{0}個, 進行中:{1}個", completed, Config.Current.MT_Limitation - completed);

                if(completed >= Config.Current.MT_Limitation)
                {
                    List<TabPage> tp = new List<TabPage>();
                    foreach (TabPage t in ResultTab.Items)
                        tp.Add(t);
                    tp = tp.OrderBy(t => t.Info.Algorithm.PieceCollection.Count).ToList();

                    ResultTab.Items.Clear();
                    foreach (var t in tp)
                        ResultTab.Items.Add(t);
                    BeginB.IsEnabled = true;
                }
            };

            for (int i = 0;Config.Current.MT_Limitation > i; i++)
            {
                Algorithm algo = new Algorithm(ClonePieces(), Dispatcher);
                algo.Sleeping += Algo_Sleeping;
                AlgoInfo info = new AlgoInfo(algo, Dispatcher);

                info.Finished = fin;
                info.Progress = prog;
                info.Begin();
                Tasks.Add((info, AddTab(info, algo.PieceCollection)));
            }
            TaskStateL.Content = string.Format("完了:{0}個, 進行中:{1}個", completed, Config.Current.MT_Limitation - completed);
            BeginB.IsEnabled = false;
        }

        private void KeyboardHook_HookEvent(ref KeyboardHook.StateKeyboard state)
        {
            if(state.Stroke == KeyboardHook.Stroke.KEY_DOWN)
            {
                if (state.Key == System.Windows.Forms.Keys.S)
                {
                    Stop();
                }
                if (state.Key == System.Windows.Forms.Keys.Space && cproc != null)
                {
                    cproc.Valiable = false;
                }
            }
        }

        private void Algo_Sleeping(object sender, RoutedSleepEventArgs e)
        {
            List<int> usable = new List<int>();

            int i = 0;
            foreach(var cp in e.TempResults)
            {
                if (cp.Source.IsCorrect())
                {
                    usable.Add(i);
                }
                i++;
            }

            if (usable.Count == 0)
                e.Index = -1;
            else
                e.Index = usable.OrderBy(_ => Guid.NewGuid().ToString()).First();
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
            Task.WhenAll(Tasks.Select(t => t.Item1.CurrentTask)).Wait();
            Tasks.Clear();
            //KeyboardHook.Stop();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Linker.PieceCollection pcol = new Linker.PieceCollection();
            foreach(TabPage page in ResultTab.Items)
            {
                pcol.AddRange(page.Info.Algorithm.PieceCollection);
            }

            MTProcessor proc = new MTProcessor(pcol, Choice);
            proc.Completed += Proc_Completed;
            proc.Begin();
        }

        private void Proc_Completed(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SecondProcessL.Pieces = ((MTProcessor)sender).Pieces;
            }));
        }

        MTProcessor cproc = null;
        void Choice(MTProcessor Proc, IList<Linker.CompositePiece> Pieces)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                cproc = Proc;
                ChoosingL.Pieces = Pieces.Select(p => (Linker.Piece)p).ToList();
            }));
        }

        private void ChoosingL_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(ChoosingL.SelectedPiece != null && cproc != null)
            {
                cproc.Choiced = (Linker.CompositePiece)ChoosingL.SelectedPiece;
                cproc.Waiting = false;
                cproc = null;
            }
        }

        private void ChoosingL_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(cproc != null)
            {
                cproc.Valiable = false;
            }
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

                if (Finished != null)
                {
                    Linker.PieceCollection pcol = new Linker.PieceCollection();
                    foreach (var piece in Algorithm.PieceCollection)
                    {
                        if (piece is Linker.CompositePiece cp)
                        {
                            if (cp.Source.IsCorrect())
                            {
                                pcol.Add(cp);
                            }
                        }
                    }

                    Dispatcher.BeginInvoke(new Action(() => Finished(this, pcol)));
                }
            });
        }
    }
}
