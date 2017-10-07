using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker
{
    public class CompositePiece : Piece
    {
        public CompositePiece( IEnumerable<Point> P ,IEnumerable<Piece> S )
        {
            pcol.CollectionChanged += Pcol_CollectionChanged;
            foreach(var piece in S)
                Source.Add(piece);

            foreach (var v in P)
                Vertexes.Add(v);
        }

        private void Pcol_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
        }

        PieceCollection pcol = new PieceCollection();
        public PieceCollection Source
        {
            get { return pcol; }
            set
            {
                if (pcol != null)
                    pcol.CollectionChanged -= Pcol_CollectionChanged;

                pcol = value;

                if (pcol != null)
                    pcol.CollectionChanged += Pcol_CollectionChanged;
            }
        }
    }
}
