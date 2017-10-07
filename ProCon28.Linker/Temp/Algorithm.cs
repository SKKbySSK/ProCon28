using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker.Extensions;

namespace ProCon28.Linker.Temp
{
    public class Algorithm
    {
        public Func<Piece, Piece, List<(int, int)>, CompositePiece> Composer { get; set; }

        public double Threshold { get; set; } = 0.1;

        IList<Piece> PieceCollection { get; }

        public Algorithm(Func<Piece, Piece, List<(int, int)>, CompositePiece> PieceBond, IList<Piece> Pieces)
        {
            Composer = PieceBond;
            PieceCollection = Pieces;
        }

        double GetValue(double Coefficient)
        {
            return Threshold * Coefficient;
        }

        bool NearEqual(double X, double Y, double Threshold = -1)
        {
            return Math.Abs(X - Y) <= (Threshold < 0 ? GetValue(5) : Threshold);
        }

        List<Piece> GetShuffled()
        {
            return PieceCollection.OrderBy((p) => Guid.NewGuid()).ToList();
        }

        public List<Piece> Run(double Threshold = 0)
        {
            List<Piece> cps = new List<Piece>();
            cps.AddRange(FindPair(Threshold));

            return cps;
        }

        List<Piece> FindPair(double Threshold, int SampleIndex = 1, List<Piece> Processed = null, int LastCount = 0)
        {
            if (Processed == null) Processed = new List<Piece>();

            List<PairInfo> Pairs = new List<PairInfo>();

            List<Piece> pieces = new List<Piece>();

            Frame f = null;
            {
                foreach (Piece p in PieceCollection)
                {
                    if (p is Frame frame)
                    {
                        f = frame;
                        break;
                    }
                }

                if (f == null) return pieces;
            }

            foreach (var p in PieceCollection)
            {
                Pairs.AddRange(GetPairsFromLines(f, f.Vertexes.AsLinesWithLength(), p, p.Vertexes.AsLinesWithLength(), Threshold));
            }

            Dictionary<PairedPiece, List<PairInfo>> PairedDictionary = new Dictionary<PairedPiece, List<PairInfo>>();
            foreach (var info in Pairs)
            {
                var pp = new PairedPiece(info.Piece1, info.Piece2);

                if (!PairedDictionary.Keys.Contains(pp))
                    PairedDictionary.Add(pp, new List<PairInfo>(new PairInfo[] { info }));
                else
                    PairedDictionary[pp].Add(info);
            }

            List<List<PairInfo>> sorted = PairedDictionary.Values.OrderByDescending(val => val.Count).ThenByDescending((val) =>
            {
                double sum = 0;
                foreach (PairInfo pi in val)
                    sum += pi.AverageLength;
                return sum;
            }).ToList();

            int c = sorted.Count;
            if (c > 1)
            {
                var piece = sorted[Math.Min(sorted.Count - 1, SampleIndex)];

                var comp = GenerateCompositePiece(piece);
                Processed.AddRange(comp);

                foreach(PairInfo pi in piece)
                {
                    if (pi.Piece1 != f)
                        PieceCollection.Remove(pi.Piece1);

                    if (pi.Piece2 != f)
                        PieceCollection.Remove(pi.Piece2);
                }

                if (c == LastCount)
                {
                    return Processed;
                }
                else
                {
                    LastCount = c;
                    return FindPair(Threshold, SampleIndex, Processed, LastCount);
                }
            }
            else
            {
                List<Piece> ps = new List<Piece>();
                ps.AddRange(Processed);
                ps.AddRange(pieces);
                return ps;
            }
        }

        List<(int, int)> CreateIndexPairs(IList<PairInfo> Infos)
        {
            List<(int, int)> ipair = new List<(int, int)>();
            foreach(PairInfo pi in Infos)
            {
                ipair.Add((pi.StartIndex1, pi.EndIndex1));
                ipair.Add((pi.StartIndex2, pi.EndIndex2));
            }

            return ipair;
        }

        List<Piece> FindPairBackup(double Threshold, List<CompositePiece> Processed = null, int LastCount = 0)
        {
            if (Processed == null) Processed = new List<CompositePiece>();

            List<PairInfo> Pairs = new List<PairInfo>();

            List<Piece> pieces = new List<Piece>();

            int count = PieceCollection.Count;
            for (int i = 0;count > i; i++)
            {
                bool add = true;
                var obj = PieceCollection[i];
                foreach (CompositePiece cp in Processed)
                {
                    if (cp.Source.Contains(obj))
                    {
                        add = false;
                    }
                }

                if (add)
                    pieces.Add(obj);
            }

            for(int a = 0;pieces.Count > a; a++)
            {
                var p1 = pieces[a];
                var lines1 = p1.Vertexes.AsLinesWithLength();

                for(int b = 0;pieces.Count > b; b++)
                {
                    if(a != b)
                    {
                        var p2 = pieces[b];
                        var lines2 = p2.Vertexes.AsLinesWithLength();
                        Pairs.AddRange(GetPairsFromLines(p1, lines1, p2, lines2, Threshold));
                    }
                }
            }

            for(int i = 0;Pairs.Count > i; i++)
            {
                int ind = i + 1 == Pairs.Count ? 0 : i + 1;
                if (Pairs[i].IsSame(Pairs[ind]))
                {
                    Pairs.RemoveAt(i);
                    i--;
                }
            }

            Dictionary<PairedPiece, List<PairInfo>> PairedDictionary = new Dictionary<PairedPiece, List<PairInfo>>();
            foreach(var info in Pairs)
            {
                var pp = new PairedPiece(info.Piece1, info.Piece2);

                if (!PairedDictionary.Keys.Contains(pp))
                    PairedDictionary.Add(pp, new List<PairInfo>(new PairInfo[] { info }));
                else
                    PairedDictionary[pp].Add(info);
            }

            List<List<PairInfo>> sorted = PairedDictionary.Values.OrderByDescending(val => val.Count).ToList();

            //以下は結合処理

            int c = sorted.Count;
            if (c > 1)
            {
                //Processed.Add(GenerateCompositePiece(sorted.First()));
                if (c == LastCount)
                {
                    return Processed.Select(p => (Piece)p).ToList();
                }
                else
                {
                    LastCount = c;
                    return null; //FindPair(Threshold, Processed, LastCount);
                }
            }
            else
            {
                List<Piece> ps = new List<Piece>();
                ps.AddRange(Processed);
                ps.AddRange(pieces);
                return ps;
            }
        }

        List<PairInfo> GetPairsFromLines(Piece Piece1, IList<(Point, Point, double)> Lines1, Piece Piece2, IList<(Point, Point, double)> Lines2, double Threshold)
        {
            List<PairInfo> pairs = new List<PairInfo>();

            foreach(var line1 in Lines1)
            {
                foreach(var line2 in Lines2)
                {
                    if (NearEqual(line1.Item3, line2.Item3, Threshold))
                        pairs.Add(new PairInfo(Piece1, line1.Item1, line1.Item2, Piece2, line2.Item1, line2.Item2));
                }
            }

            return pairs;
        }

        List<Piece> GenerateCompositePiece(List<PairInfo> Pairs)
        {
            List<Piece> pieces = new List<Piece>();
            List<(int, int)> pairsint = new List<(int, int)>();
            foreach (var pair in Pairs)
            {
                var p1 = pair.Piece1;
                var p2 = pair.Piece2;
                foreach (var pint in Pairs)
                {
                    if (pint.Piece1 == p1 || pint.Piece1 == p2)
                    {
                        if (pint.Piece2 == p1 || pint.Piece2 == p2)
                        {
                            pairsint.Add((pint.StartIndex1, pint.EndIndex1));
                            pairsint.Add((pint.StartIndex2, pint.EndIndex2));
                        }
                    }
                }

                try
                {
                    pieces.Add(Composer(p1, p2, pairsint));
                }
                catch (Exception) { }
                pairsint.Clear();
            }

            return pieces;
        }
    }

    class PairInfo
    {
        public PairInfo(Piece Piece1, Point Start1, Point End1, Piece Piece2, Point Start2, Point End2)
        {
            this.Piece1 = Piece1;
            this.Start1 = Start1;
            this.End1 = End1;

            Length1 = this.Start1.GetLength(this.End1);

            this.Piece2 = Piece2;
            this.Start2 = Start2;
            this.End2 = End2;

            Length2 = this.Start2.GetLength(this.End2);

            Difference = Length1 - Length2;
            AbsDifference = Math.Abs(Difference);
        }

        public Piece Piece1 { get; }
        public Point Start1 { get; }
        public Point End1 { get; }
        public int StartIndex1 { get { return Piece1.Vertexes.IndexOf(Start1); } }
        public int EndIndex1 { get { return Piece1.Vertexes.IndexOf(End1); } }
        public Piece Piece2 { get; }
        public Point Start2 { get; }
        public Point End2 { get; }
        public int StartIndex2 { get { return Piece2.Vertexes.IndexOf(Start2); } }
        public int EndIndex2 { get { return Piece2.Vertexes.IndexOf(End2); } }

        public double AverageLength
        {
            get { return (Length1 + Length2) / 2; }
        }

        public double Length1 { get; }
        public double Length2 { get; }
        public double Difference { get; }
        public double AbsDifference { get; }

        public bool IsSame(PairInfo To)
        {
            return To.Piece1 == Piece1 && To.Start1 == Start1 && To.Start2 == Start2 && To.End1 == End1 && To.End2 == End2;
        }
    }

    sealed class PairedPiece
    {
        public PairedPiece(Piece P1, Piece P2)
        {
            Piece1 = P1;
            Piece2 = P2;
        }

        public Piece Piece1 { get; }
        public Piece Piece2 { get; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is PairedPiece))
            {
                return false;
            }

            PairedPiece pp = (PairedPiece)obj;
            if(pp.Piece1 != Piece1)
            {
                return pp.Piece2 == Piece1 && pp.Piece1 == Piece2;
            }
            else
            {
                return pp.Piece2 == Piece2;
            }
        }

        public override int GetHashCode()
        {
            int total = 0;
            foreach (Point p in Piece1.Vertexes)
                total += p.X;
            foreach (Point p in Piece2.Vertexes)
                total += p.X;
            return total;
        }
    }
}
