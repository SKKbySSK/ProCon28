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

namespace ProCon28.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();
            PieceG.VertexAdded += PieceG_VertexChanged;
            PieceG.VertexRemoved += PieceG_VertexChanged;
            PieceG.VertexMoved += PieceG_VertexChanged;
        }

        private void PieceG_VertexChanged(object sender, EventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Linker.Piece p = new Linker.Piece();

            Random rnd = new Random();
            int count = rnd.Next(3, 5);

            for(int i = 0; count > i; i++)
            {
                Linker.Point point = new Linker.Point(new Random(Environment.TickCount + 1 + (i * i)).Next(0, 30), new Random(Environment.TickCount + 2 + (i * i)).Next(0, 30));
                p.Vertexes.Add(point);
            }
            
            for(int i = 0;p.Vertexes.Count > i; i++)
            {
                Console.WriteLine("Angle 0({0} deg):{1}", Math.PI / 2, p.GetAngle(i));
            }

            PieceG.Piece = p;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (PieceG.Piece == null) return;
            PieceG.Piece.SortVertexes(Linker.PointSortation.Clockwise);

            Linker.Piece p = PieceG.Piece.Convert();
            PieceG.Piece = p;
        }

        private void AddB_Click(object sender, RoutedEventArgs e)
        {
            if (PieceList.Pieces.Contains(PieceG.Piece))
                PieceList.Pieces.Remove(PieceG.Piece);
            
            PieceList.Pieces.Add(PieceG.Piece);
        }

        private void PieceList_SelectedPieceChanged(object sender, EventArgs e)
        {
            if(PieceList.SelectedPiece != null)
            {
                PieceG.Piece = PieceList.SelectedPiece;
            }
        }
    }
}
