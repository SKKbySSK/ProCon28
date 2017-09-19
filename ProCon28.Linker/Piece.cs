using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker.Extensions;

namespace ProCon28.Linker
{
    public class Piece : ICloneable
    {
        public PointCollection Vertexes { get; private set; } = new PointCollection();
        public float Rotation { get; set; }

        /// <summary>
        /// ピースのクローンを作成します
        /// </summary>
        /// <returns>クローンされた新しいインスタンスのピース</returns>
        public object Clone()
        {
            Piece p = new Piece();
            p.Vertexes.AddRange(Vertexes);
            p.Rotation = Rotation;
            return p;
        }

        /// <summary>
        /// 指定の方法で頂点をソートします
        /// </summary>
        /// <param name="Sortation">ソート方向。時計回りか反時計回りで指定できます</param>
        public void SortVertexes(PointSortation Sortation)
        {
            Vertexes = Vertexes.Sort(Sortation);
        }
    }

    public class PieceCollection : ObservableCollection<Piece> { }
}
