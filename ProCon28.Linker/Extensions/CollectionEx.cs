using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker.Extensions
{
    public static class CollectionEx
    {
        public static void AddRange<T>(this ICollection<T> Collection, IEnumerable<T> Items)
        {
            foreach (T item in Items)
                Collection.Add(item);
        }

        public static PieceCollection AsPieceCollection(this IEnumerable<Piece> Pieces)
        {
            PieceCollection pcol = new PieceCollection();
            foreach (var p in Pieces)
                pcol.Add(p);
            return pcol;
        }
    }
}
