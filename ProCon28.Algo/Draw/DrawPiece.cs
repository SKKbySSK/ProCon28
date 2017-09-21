using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading.Tasks;
using ProCon28.Linker;

namespace ProCon28.Algo
{
    class DrawPiece
    {
        public DrawPiece(Piece piece)
        {
            Piece p = (Piece)piece.Clone();
            ProCon28.Linker.PointCollection pcol = p.Vertexes;
            System.Windows.Media.PointCollection scol = new System.Windows.Media.PointCollection();
            foreach(Point c in pcol)
            {
                scol.Add(new System.Windows.Point(c.X, c.Y));
            }
            Polygon pol = new Polygon();
            pol.Points = scol;
            pol.Fill = Brushes.SkyBlue;
            pol.Width = 50;
            pol.Height = 50;
            pol.Stretch = Stretch.Uniform;
            pol.Stroke = Brushes.Black;
            pol.StrokeThickness = 2;
        }
    }

    class DrawPieceCollection : ObservableCollection<DrawPiece> {  }

    public class DrawPieceCollectionDraw
    {
        public DrawPieceCollectionDraw(PieceCollection pc)
        {
            DrawPieceCollection dpc = new DrawPieceCollection();
            foreach(Piece c in pc)
            {
                dpc.Add(new DrawPiece(c));
            }
        }
    }
}
