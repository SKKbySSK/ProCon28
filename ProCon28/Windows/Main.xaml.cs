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

            BatchC.DataContext = Batch.ViewModel.Current;
            if (Batch.ViewModel.Current.BatchFiles.Count > 0)
                BatchC.SelectedIndex = 0;

            SortC.IsChecked = Config.Current.ClockwiseSort;
            BlurS.Value = Config.Current.BlurThreshold;
            StraightS.Value = Config.Current.StraightThreshold;
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

        private void TransportB_Click(object sender, RoutedEventArgs e)
        {
            Linker.Tcp.RemotePieces ps = new Linker.Tcp.RemotePieces();
            ps.BytePieces = PieceList.Pieces.AsBytes();

            MarshalDialog dialog = new MarshalDialog(ps);
            dialog.Show();
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

        private void ShapeQrB_Click(object sender, RoutedEventArgs e)
        {
            QrReader ShapeQRWindow = new QrReader();

            EventHandler ev = null;
            ev = new EventHandler((_1, _2) =>
            {
                ShapeQRWindow.ResultChanged -= ev;

                string[] copied = new string[ShapeQRWindow.Result.Length];
                for(int i = 0;ShapeQRWindow.Result.Length > i; i++)
                {
                    copied[i] = ShapeQRWindow.Result[i];
                }

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    foreach (string shape in copied)
                    {
                        if (ShapeQRManager.AddShape(shape))
                        {
                            PieceList.Pieces.AddRange(ShapeQRManager.GeneratePieces(shape));
                            System.Threading.Thread.Sleep(200);
                        }
                    }
                    ShapeQRWindow.ResultChanged += ev;
                }));
            });

            ShapeQRWindow.ResultChanged += ev;
            ShapeQRWindow.ShowDialog();
            ShapeQRWindow.ResultChanged -= ev;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Config.Current.ClockwiseSort = SortC.IsChecked ?? false;
            Config.Current.BlurThreshold = BlurS.Value;
            Config.Current.StraightThreshold = StraightS.Value;
        }

        public void Append(object Format, params object[] Args)
        {
            Dispatcher.BeginInvoke(new Action(() => ConsoleT.AppendText(string.Format(Format.ToString(), Args))));
        }

        public void Append(object Text)
        {
            Dispatcher.BeginInvoke(new Action(() => ConsoleT.AppendText(Text.ToString())));
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
            }
        }

        private void Camera_Recognized(object sender, Controls.ContoursEventArgs e)
        {
            List<Linker.Piece> Pieces = new List<Linker.Piece>();
            foreach (var shape in e.Contours)
            {
                Linker.Piece p = new Linker.Piece();

                foreach(var point in shape)
                {
                    Linker.Point lp =
                        new Linker.Point((int)(Math.Round(point.X * e.Scale)), (int)(Math.Round(point.Y * e.Scale)));
                    if (!p.Vertexes.Contains(lp))
                        p.Vertexes.Add(lp);
                }

                p = p.Convert().UnsafeRotate(e.Rotation);
                PieceList.Pieces.Add(p);
                Pieces.Add(p);
            }

            if(Pieces.Count > 2)
            {
                ComparePieces(Pieces[0], Pieces[1]);
            }
        }

        void ComparePieces(Linker.Piece Piece1, Linker.Piece Piece2)
        {
            var lines1 = Piece1.Vertexes.AsLinesWithLength();
            var lines2 = Piece2.Vertexes.AsLinesWithLength();


            (Linker.Point, Linker.Point, double)? p1 = null, p2 = null;
            foreach (var line1 in lines1)
            {
                foreach(var line2 in lines2)
                {
                    if(Math.Abs(line1.Item3 - line2.Item3) <= 0.500)
                    {
                        p1 = line1;
                        p2 = line2;
                        break;
                    }
                }
            }

            if (p1.HasValue && p2.HasValue)
                Log.WriteLine("{0} - {1}", p1.Value.Item3, p2.Value.Item3);
        }
    }
}
