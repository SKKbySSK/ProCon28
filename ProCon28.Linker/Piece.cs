using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker.Extensions;
using System.Runtime.Serialization;
using System.IO;

namespace ProCon28.Linker
{
    public class Piece : ICloneable
    {
        public Piece()
        {
            Vertexes = new PointCollection();
        }

        public Piece(byte[] Bytes)
        {
            using (MemoryStream ms = new MemoryStream(Bytes))
            using (BinaryReader br = new BinaryReader(ms))
            {
                int vlen = br.ReadInt32();
                Vertexes = new PointCollection(br.ReadBytes(vlen));
                Rotation = br.ReadSingle();
            }
        }

        public PointCollection Vertexes { get; private set; }
        public double Rotation { get; set; }

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

        public byte[] AsBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            using(BinaryWriter bw = new BinaryWriter(ms))
            {
                byte[] ver = Vertexes.AsBytes();
                bw.Write(ver.Length);
                bw.Write(ver);
                bw.Write(Rotation);

                return ms.ToArray();
            }
        }
    }
    
    public class PieceCollection : ObservableCollection<Piece>
    {
        public PieceCollection() { }

        public PieceCollection(byte[] Bytes)
        {
            using (MemoryStream ms = new MemoryStream(Bytes))
            using (BinaryReader br = new BinaryReader(ms))
            {
                int count = br.ReadInt32();
                for(int i = 0;count > i; i++)
                {
                    int len = br.ReadInt32();
                    Add(new Piece(br.ReadBytes(len)));
                }
            }
        }

        public byte[] AsBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(Count);
                foreach(Piece p in this)
                {
                    byte[] bs = p.AsBytes();
                    bw.Write(bs.Length);
                    bw.Write(bs);
                }
                return ms.ToArray();
            }
        }
    }
}
