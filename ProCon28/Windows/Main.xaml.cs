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
            PieceG.VertexAdded += PieceG_Vertex;
            PieceG.VertexMoved += PieceG_Vertex;
            PieceG.VertexRemoved += PieceG_Vertex;
            PieceG.PieceChanged += PieceG_Vertex;

            PieceG.VertexAdded += PieceG_VertexAdded;
            BatchC.DataContext = Batch.ViewModel.Current;
            if (Batch.ViewModel.Current.BatchFiles.Count > 0)
                BatchC.SelectedIndex = 0;
        }

        private void PieceG_VertexAdded(object sender, EventArgs e)
        {
            PieceG.Piece.SortVertexes(Linker.PointSortation.Clockwise);
            PieceG.RedrawPiece();
        }

        private void PieceG_Vertex(object sender, EventArgs e)
        {
            RatioS.Maximum = PieceG.Piece.Vertexes.Count;
            RatioS.Value = 0;
            Lines = PieceG.Piece?.Vertexes.AsLines();
            PieceP.Piece = PieceG.Piece;
        }

        void AppendLog()
        {
            Logs.Add((Linker.Piece)PieceG.Piece.Clone());
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
            if (PieceList.Pieces.Contains(PieceG.Piece))
                PieceList.Pieces.Remove(PieceG.Piece);

            PieceList.Pieces.Add(PieceG.Piece);
        }

        private void PieceList_SelectedPieceChanged(object sender, EventArgs e)
        {
            if (PieceList.SelectedPiece != null)
            {
                PieceG.Piece = PieceList.SelectedPiece;
            }
        }

        private void TransportB_Click(object sender, RoutedEventArgs e)
        {
            Linker.Tcp.RemotePieces ps = new Linker.Tcp.RemotePieces();
            ps.BytePieces = PieceList.Pieces.AsBytes();

            MarshalDialog dialog = new MarshalDialog(ps);
            dialog.Show();
        }

        private void LoadB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Linker.DoublePoint> points = new List<Linker.DoublePoint>();
                using (StreamReader sr = new StreamReader(LoadT.Text))
                {
                    string line = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] pair = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        double x = double.Parse(pair[0]);
                        double y = double.Parse(pair[1]);
                        points.Add(new Linker.DoublePoint(x / ThresholdS.Value, y / ThresholdS.Value));
                    }
                }

                Linker.Piece piece = new Linker.Piece();
                foreach (Linker.DoublePoint dp in points)
                {
                    Linker.Point p = new Linker.Point(dp);
                    if (!piece.Vertexes.Contains(p))
                        piece.Vertexes.Add(p);
                }

                PieceG.Piece = piece;

                Logs.Clear();
                AppendLog();
            }
            catch (Exception) { }
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

            EventHandler ev = new EventHandler((_1, _2) =>
            {
                foreach (string shape in ShapeQRWindow.Result)
                {
                    if (ShapeQRManager.AddShape(shape))
                    {
                        PieceList.Pieces.AddRange(ShapeQRManager.GeneratePieces(shape, out Linker.Frame Frame));
                        if (Frame != null) PieceList.Pieces.Add(Frame);
                    }
                }
            });

            ShapeQRWindow.ResultChanged += ev;
            ShapeQRWindow.ShowDialog();
            ShapeQRWindow.ResultChanged -= ev;
        }
    }
}
