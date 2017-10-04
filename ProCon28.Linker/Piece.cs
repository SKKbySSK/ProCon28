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

        public Piece(PointCollection Vertexes)
        {
            this.Vertexes = Vertexes;
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

        public bool Fixed { get; protected set; }
        public PointCollection Vertexes { get; private set; }
        public double Rotation { get; set; }
        public string GUID { get; private set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// ピースのクローンを作成します
        /// </summary>
        /// <returns>クローンされた新しいインスタンスのピース</returns>
        public virtual object Clone()
        {
            Piece p = new Piece();
            p.Vertexes.AddRange(Vertexes);
            p.Rotation = Rotation;
            p.GUID = GUID;
            return p;
        }

        public static bool operator ==(Piece Piece1, Piece Piece2)
        {
            return Piece1.GUID == Piece2.GUID;
        }

        public static bool operator !=(Piece Piece1, Piece Piece2)
        {
            return Piece1.GUID != Piece2.GUID;
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
