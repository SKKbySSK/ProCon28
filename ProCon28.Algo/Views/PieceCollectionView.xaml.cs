using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using ProCon28.Controls;
using ProCon28.Linker;
using Reactive.Bindings;

namespace ProCon28.Algo.Views
{
    /// <summary>
    /// PieceCollectionView.xaml の相互作用ロジック
    /// </summary>
    public partial class PieceCollectionView : UserControl
    {
        public PieceCollectionView()
        {
            InitializeComponent();
            
            DataContext = this;

            PieceCollection col = new PieceCollection();
            col.CollectionChanged += Pieces_CollectionChanged;

            pieces = col;
        }

        private void Pieces_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Views.Add(new PieceView() { Piece = (Piece)e.NewItems[0] });
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Views.RemoveAt(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Views.Clear();
                    break;
            }
        }

        IList<Piece> pieces;
        public IList<Piece> Pieces
        {
            get { return pieces; }
            set
            {
                if(pieces != null && pieces is INotifyCollectionChanged rcolc)
                    rcolc.CollectionChanged -= Pieces_CollectionChanged;

                pieces = value;
                Views.Clear();

                if (pieces != null)
                {
                    if (pieces is INotifyCollectionChanged acolc)
                        acolc.CollectionChanged += Pieces_CollectionChanged;

                    foreach (var p in pieces)
                        Views.Add(new PieceView() { Piece = p });
                }
            }
        }

        /// <summary>
        /// バインドのために隠蔽したビューコレクション
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ReactiveCollection<PieceView> Views { get; } = new ReactiveCollection<PieceView>();

        public ReactiveProperty<double> ItemHeight { get; } = new ReactiveProperty<double>(150);
        public ReactiveProperty<double> ItemWidth { get; } = new ReactiveProperty<double>(150);
    }
}
