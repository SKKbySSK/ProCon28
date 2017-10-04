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
using System.Windows.Threading;
using ProCon28.Linker.Extensions;
using System.IO;

namespace ProCon28.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class Main : Window
    {
        IList<(Linker.Point, Linker.Point)> Lines;
        List<Linker.Piece> Logs = new List<Linker.Piece>();

        public Main()
        {
            InitializeComponent();

            Log.MainWindow = this;

            PieceG.VertexAdded += PieceG_Vertex;
            PieceG.VertexMoved += PieceG_Vertex;
            PieceG.VertexRemoved += PieceG_Vertex;
            PieceG.PieceChanged += PieceG_PieceChanged;
            PieceList.Pieces.CollectionChanged += Pieces_CollectionChanged;
            OpenCvCapture.WorkerFinished += (sender, e) =>
            {
                OpenCvCapture.Visibility = Visibility.Hidden;
                PieceG.Visibility = Visibility.Visible;
            };

            BatchC.DataContext = Batch.ViewModel.Current;
            if (Batch.ViewModel.Current.BatchFiles.Count > 0)
                BatchC.SelectedIndex = 0;

            SortC.IsChecked = Config.Current.ClockwiseSort;
            BlurS.Value = Config.Current.BlurThreshold;
            StraightS.Value = Config.Current.StraightThreshold;

            var ps = new Linker.Point[] 
            {
                new Linker.Point(0, 0), new Linker.Point(5, 5), new Linker.Point(0, 10),
                new Linker.Point(0, 10), new Linker.Point(5, 5), new Linker.Point(10, 15),
                new Linker.Point(5, 5), new Linker.Point(10, 0), new Linker.Point(10, 15)
            };
            Linker.Piece p1 = new Linker.Piece(ps.ToList().GetRange(0, 3));
            Linker.Piece p2 = new Linker.Piece(ps.ToList().GetRange(3, 3));
            Linker.Piece p3 = new Linker.Piece(ps.ToList().GetRange(6, 3));
            Linker.CompositePiece cp = new Linker.CompositePiece(ps, new Linker.Piece[] { p1, p2, p3 });
            PieceList.Pieces.Add(cp);
        }

        private void Pieces_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                TransferPiecesView.Stop();
                return;
            }

            if (TransferPiecesView.IsTransferring)
            {
                TransferPiecesView.TransferPieces(PieceList.Pieces);
            }
        }

        private void PieceG_PieceChanged(object sender, EventArgs e)
        {
            if (PieceG.Piece != null)
            {
                RatioS.Maximum = PieceG.Piece.Vertexes.Count;
                RatioS.Value = 0;
                Lines = PieceG.Piece?.Vertexes.AsLines();
                PieceP.Piece = PieceG.Piece;
            }
        }

        private void PieceG_Vertex(object sender, EventArgs e)
        {
            if(PieceG.Piece != null)
            {
                RatioS.Maximum = PieceG.Piece.Vertexes.Count;
                RatioS.Value = 0;
                Lines = PieceG.Piece?.Vertexes.AsLines();
                PieceP.RedrawPiece();
            }
        }

        void AppendLog()
        {
            Logs.Add((Linker.Piece)PieceG.Piece.Clone());
        }

        void ClearLog()
        {
            Logs.Clear();
        }

        void Undo()
        {
            if (Logs.Count <= 1)
                return;

            PieceG.Piece = Logs[Logs.Count - 2];
            Logs.RemoveAt(Logs.Count - 1);
        }

        private void AddB_Click(object sender, RoutedEventArgs e)
        {
            PieceList.Pieces.Add(PieceG.Piece);
            PieceG.Piece = null;
            PieceP.Piece = null;
            ClearLog();
        }

        private void BlurB_Click(object sender, RoutedEventArgs e)
        {
            if (PieceG.Piece == null) return;
            PieceG.Piece = PieceEdit.Blur.Run(PieceG.Piece, (int)BlurS.Value);
            AppendLog();
        }

        private void StraightB_Click(object sender, RoutedEventArgs e)
        {
            if (PieceG.Piece == null) return;
            PieceG.Piece = PieceEdit.Straight.Run(PieceG.Piece, StraightS.Value);
            AppendLog();
        }

        private void UndoB_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void SortB_Click(object sender, RoutedEventArgs e)
        {
            if (PieceG.Piece == null) return;
            if (SortC.IsChecked ?? false)
                PieceG.Piece.SortVertexes(Linker.PointSortation.Clockwise);
            else
                PieceG.Piece.SortVertexes(Linker.PointSortation.AntiClockwise);
            PieceG.RedrawPiece();
            AppendLog();
        }

        private void RatioS_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PieceG.DrawLines.Clear();
            if (PieceG.Piece == null) return;
            if (RatioS.Value > 0)
            {
                PieceG.DrawLines.Add(Lines[(int)RatioS.Value - 1]);
            }
        }

        private void RatioB_Click(object sender, RoutedEventArgs e)
        {
            if (PieceG.Piece == null) return;
            if (RatioS.Value < 1)
                return;
            if(double.TryParse(RatioT.Text, out double len))
            {
                var line = Lines[(int)RatioS.Value - 1];
                int from = -1, to = -1;
                for(int i = 0;PieceG.Piece.Vertexes.Count > i; i++)
                {
                    if (PieceG.Piece.Vertexes[i] == line.Item1)
                        from = i;
                    else if (PieceG.Piece.Vertexes[i] == line.Item2)
                        to = i;

                    if (from > -1 && to > -1)
                        break;
                }

                PieceG.Piece = PieceEdit.RatioConvert.Run(PieceG.Piece, from, to, len);
                AppendLog();
            }
        }

        private void ContoursB_Click(object sender, RoutedEventArgs e)
        {
            if (PieceG.Piece == null) return;
            PieceG.Piece = PieceEdit.ExtractContours.Run(PieceG.Piece);
            AppendLog();
        }

        private void DuplicateB_Click(object sender, RoutedEventArgs e)
        {
            if (PieceG.Piece == null) return;
            Linker.Piece piece = new Linker.Piece();
            for(int i = 0;PieceG.Piece.Vertexes.Count > i; i++)
            {
                Linker.Point p = PieceG.Piece.Vertexes[i];
                if (!piece.Vertexes.Contains(p))
                    piece.Vertexes.Add(p);
            }
            PieceG.Piece = piece;
            AppendLog();
        }

        private void BatchB_Click(object sender, RoutedEventArgs e)
        {
            if (PieceG.Piece == null) return;
            if (BatchC.SelectedItem != null)
            {
                PieceG.Piece = Batch.ViewModel.Current.Batch(PieceG.Piece, BatchC.SelectedItem.ToString());
                AppendLog();
            }
        }

        private void BatchReloadB_Click(object sender, RoutedEventArgs e)
        {
            Batch.ViewModel.Current.UpdateFiles();
            if (Batch.ViewModel.Current.BatchFiles.Count > 0)
                BatchC.SelectedIndex = 0;
        }

        private void GcdB_Click(object sender, RoutedEventArgs e)
        {
            PieceG.Piece = PieceEdit.GcdConvert.Run(PieceG.Piece);
            AppendLog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Config.Current.ClockwiseSort = SortC.IsChecked ?? false;
            Config.Current.BlurThreshold = BlurS.Value;
            Config.Current.StraightThreshold = StraightS.Value;
        }

        public void Append(object Format, params object[] Args)
        {
            Append(string.Format(Format.ToString(), Args));
        }

        public void Append(object Text)
        {
            DateTime now = DateTime.Now;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                string text = Text + " (" + now + ")";
                ConsoleL.Items.Add(text);
                ConsoleL.ScrollIntoView(text);
            }));
        }

        private void EmptyB_Click(object sender, RoutedEventArgs e)
        {
            PieceG.Piece = new Linker.Piece();
        }

        private void PieceList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && PieceList.SelectedPiece != null)
            {
                Linker.Piece piece = PieceList.SelectedPiece;
                PieceList.Pieces.Remove(piece);

                PieceG.Piece = piece;
                ClearLog();
                AppendLog();
                Camera.Stop();
            }
        }

        private void Camera_Recognized(object sender, Controls.ContoursEventArgs e)
        {
            List<Linker.Piece> Pieces = new List<Linker.Piece>();
            Linker.Piece p = new Linker.Piece();

            foreach (var point in e.Contour)
            {
                Linker.Point lp =
                    new Linker.Point((int)(Math.Round(point.X * e.Scale)), (int)(Math.Round(point.Y * e.Scale)));
                if (!p.Vertexes.Contains(lp))
                    p.Vertexes.Add(lp);
            }

            p = p.Convert();
            PieceList.Pieces.Add(p);
            Pieces.Add(p);
        }

        private void TransferPiecesView_RequestingPieces(object sender, Controls.RoutedPieceEventArgs e)
        {
            if(PieceList.Pieces.Count > 0)
            {
                e.Pieces = PieceList.Pieces;
            }
        }

        private void ClearPB_Click(object sender, RoutedEventArgs e)
        {
            PieceList.Pieces.Clear();
            ShapeQRManager.Reset();
        }

        private void Camera_Initializing(object sender, EventArgs e)
        {
            OpenCV.WorkerCapture worker = new OpenCV.WorkerCapture(Config.Current.Camera);
            Camera.Capture = worker;
            OpenCvCapture.Initialize(worker);
            OpenCvCapture.Visibility = Visibility.Visible;
            PieceG.Visibility = Visibility.Hidden;
        }

        private void Camera_QrRecognized(object sender, Controls.QrReaderEventArgs e)
        {
            PieceList.Pieces.AddRange(ShapeQRManager.GeneratePieces(e.Result));
        }
    }
}
