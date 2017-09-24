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

namespace ProCon28.Controls
{
    /// <summary>
    /// PieceViewer.xaml の相互作用ロジック
    /// </summary>
    public partial class PieceViewer : UserControl
    {
        public event EventHandler SelectedPieceChanged;

        public PieceViewer()
        {
            InitializeComponent();
            Pieces.CollectionChanged += Pieces_CollectionChanged;
        }

        private void Pieces_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Linker.Piece p;
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    p = (Linker.Piece)e.NewItems[0];
                    if (p == null) return;
                    PieceView view = new PieceView() { Piece = p, Margin = new Thickness(20) };
                    view.MouseLeftButtonDown += View_MouseLeftButtonDown;
                    view.MouseRightButtonDown += View_MouseRightButtonDown;
                    PiecesStack.Children.Add(view);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    p = (Linker.Piece)e.OldItems[0];
                    foreach(PieceView pv in PiecesStack.Children)
                    {
                        if(pv.Piece == p)
                        {
                            PiecesStack.Children.Remove(pv);
                            pv.MouseLeftButtonDown -= View_MouseLeftButtonDown;
                            pv.MouseRightButtonDown -= View_MouseRightButtonDown;
                            break;
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    PiecesStack.Children.Clear();
                    break;
            }
        }

        private void View_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PieceView view = (PieceView)sender;

            foreach (PieceView pv in PiecesStack.Children)
                pv.Background = null;

            SelectedPiece = view.Piece;
        }

        private void View_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PieceView view = (PieceView)sender;

            foreach (PieceView pv in PiecesStack.Children)
                pv.Background = null;

            if (SelectedPieceView != view)
                SelectedPiece = view.Piece;
            else
                SelectedPiece = null;
        }

        PieceView _sel = null;
        PieceView SelectedPieceView
        {
            get { return _sel; }
            set
            {
                if (_sel != null)
                    _sel.Background = null;

                _sel = value;

                if (_sel != null)
                    _sel.Background = new SolidColorBrush(Color.FromArgb(70, 0, 100, 255));
            }
        }

        public Linker.Piece SelectedPiece
        {
            get { return SelectedPieceView == null ? null : SelectedPieceView.Piece; }
            set
            {
                if (value == null)
                    SelectedPieceView = null;
                else
                {
                    foreach (PieceView pv in PiecesStack.Children)
                    {
                        if (pv.Piece == value)
                        {
                            SelectedPieceView = pv;
                            break;
                        }
                    }
                }
                SelectedPieceChanged?.Invoke(this, new EventArgs());
            }
        }

        public Linker.PieceCollection Pieces { get; } = new Linker.PieceCollection();

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            Pieces.Remove(SelectedPiece);
            SelectedPiece = null;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (SelectedPiece != null) RemoveItem.IsEnabled = true;
            else RemoveItem.IsEnabled = false;
        }
    }
}
