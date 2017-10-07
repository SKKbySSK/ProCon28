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

            if (Instance.ProConMode)
            {
                DateTime dt = DateTime.Now;
                Directory.CreateDirectory("PieceLogs");
                byte[] raw = PieceList.Pieces.AsBytes();
                using (FileStream fs = new FileStream("PieceLogs/" + dt.ToString("yy MM dd HH mm ss") + ".pbin", FileMode.Create, FileAccess.Write))
                    fs.Write(raw, 0, raw.Length);
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
            if (PieceG.Piece != null && PieceG.Piece.Vertexes.Count > 0)
            {
                PieceList.Pieces.Add(PieceG.Piece);
            }
            PieceG.Piece = new Linker.Piece();
        }

        private void PieceList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed && PieceList.SelectedPiece != null)
            {
                Linker.Piece piece = PieceList.SelectedPiece;
                PieceList.Pieces.Remove(piece);

                if (PieceG.Piece != null && PieceG.Piece.Vertexes.Count > 0)
                {
                    PieceList.Pieces.Add(PieceG.Piece);
                }

                PieceG.Piece = piece;
                ClearLog();
                AppendLog();
                Camera.Stop();
            }
        }

        private void Camera_Recognized(object sender, Controls.ContoursEventArgs e)
        {
            if (e.Contour.Length < 2) return;

            Linker.Piece dp = new Linker.Piece();

            foreach (var point in e.Contour)
            {
                Linker.Point lp =
                    new Linker.Point(point.X, point.Y);
                if (!dp.Vertexes.Contains(lp))
                    dp.Vertexes.Add(lp);
            }

            Linker.Point p1 = dp.Vertexes[0], p2 = dp.Vertexes[1];

            dp = dp.Convert();
            double angle = Math.Atan2(Math.Abs(p1.Y - p2.Y), Math.Abs(p1.X - p2.X));

            DoublePiece rp = new DoublePiece();
            foreach (var point in dp.Vertexes)
            {
                double retX, retY;
                retX = point.X * Math.Cos(angle) - point.Y * Math.Sin(angle);
                retY = point.X * Math.Sin(angle) + point.Y * Math.Cos(angle);

                rp.Vertexes.Add((retX, retY));
            }

            Linker.Piece scaled
                = new Linker.Piece(rp.Vertexes.Select(p =>
                new Linker.Point((int)(Math.Round(p.Item1 * e.Scale)), (int)(Math.Round(p.Item2 * e.Scale))))).Convert();
            int c = 0;
            foreach (var p in scaled.Vertexes)
            {
                if (p.X == 0)
                    c++;
            }
            if (c < 2)
            {
                for(int i = 1; 360 >= i; i++)
                {
                    rp.Vertexes.Clear();
                    double ag = ((Math.PI / 360) * i) - angle;
                    foreach (var point in dp.Vertexes)
                    {
                        double retX, retY;
                        retX = point.X * Math.Cos(ag) - point.Y * Math.Sin(ag);
                        retY = point.X * Math.Sin(ag) + point.Y * Math.Cos(ag);

                        rp.Vertexes.Add((retX, retY));
                    }
                    scaled = new Linker.Piece(rp.Vertexes.Select(p =>
                    new Linker.Point((int)(Math.Round(p.Item1 * e.Scale)), (int)(Math.Round(p.Item2 * e.Scale))))).Convert();

                    c = 0;
                    foreach (var p in scaled.Vertexes)
                    {
                        if (p.X == 0)
                            c++;
                    }

                    if (c >= 2) break;
                }

            }

            PieceList.Pieces.Add(scaled);
        }

        class DoublePiece
        {
            public List<(double, double)> Vertexes { get; } = new List<(double, double)>();
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

        private void Camera_Initializing(object sender, Controls.InitializingEventArgs e)
        {
            OpenCV.WorkerCapture worker = new OpenCV.WorkerCapture(Config.Current.Camera);

            Camera.Capture = worker;
            OpenCvCapture.UseRecognizerPoints = e.Mode == Controls.CaptureMode.Pieces;
            OpenCvCapture.Initialize(worker);
            OpenCvCapture.Visibility = Visibility.Visible;
            PieceG.Visibility = Visibility.Hidden;
        }

        private void Camera_QrRecognized(object sender, Controls.QrReaderEventArgs e)
        {
            PieceList.Pieces.AddRange(ShapeQRManager.GeneratePieces(e.Result));
        }

        private void TestAlgoB_Click(object sender, RoutedEventArgs e)
        {
        }

        private void LogClearItem_Click(object sender, RoutedEventArgs e)
        {
            ConsoleL.Items.Clear();
        }

        private void Camera_CameraFixed(object sender, EventArgs e)
        {

        }
    }
}
