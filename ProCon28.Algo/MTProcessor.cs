using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;
using ProCon28.Linker.Extensions;

namespace ProCon28.Algo
{
    class MTProcessor
    {
        public PieceCollection Pieces { get; private set; }
        List<AlgoInfo> Infos = new List<AlgoInfo>();

        public bool Waiting { get; set; } = false;
        public bool Valiable { get; set; } = true;
        public CompositePiece Choiced { get; set; }

        public Action<MTProcessor, IList<CompositePiece>> ChoosingFunction { get; }

        public event EventHandler Completed;

        public MTProcessor(PieceCollection Pieces, Action<MTProcessor, IList<CompositePiece>> ChoosingFunction)
        {
            this.ChoosingFunction = ChoosingFunction;
            this.Pieces = ClonePieces(Pieces);
        }

        public Task Begin()
        {
            return Task.Run(() =>
            {
                RemovePieces(Pieces);
                RemoveIncorrects(Pieces);
                RemoveDuplicated(Pieces);
                Completed?.Invoke(this, new EventArgs());
            });
        }

        void RemovePieces(PieceCollection Pieces)
        {
            for(int i = 0;Pieces.Count > i; i++)
            {
                if(!(Pieces[i] is CompositePiece))
                {
                    Pieces.RemoveAt(i);
                    i--;
                }
            }
        }

        void RemoveIncorrects(PieceCollection Pieces)
        {
            for(int i = 0;Pieces.Count > i; i++)
            {
                if (Pieces[i] is CompositePiece cp)
                {
                    if (!cp.Source.IsCorrect())
                    {
                        Pieces.Remove(cp);
                        i--;
                    }
                }
            }
        }

        void RemoveDuplicated(PieceCollection Pieces)
        {
            Func<CompositePiece, CompositePiece, bool> HasSameGUID = (pi1, pi2) =>
            {
                foreach (var p1 in pi1.Source)
                {
                    foreach (var p2 in pi2.Source)
                    {
                        if (p1.GUID == p2.GUID) return true;
                    }
                }

                return false;
            };

            IList<Piece> pcol = ClonePieces(Pieces);

            Solver(HasSameGUID, pcol);

            Pieces.Clear();
            Pieces.AddRange(pcol);
        }

        const int TryLimit = 3;
        private void Solver(Func<CompositePiece, CompositePiece, bool> HasSameGUID, IList<Piece> pcol)
        {
            for (int i = 0; pcol.Count > i; i++)
            {
                List<CompositePiece> duplicates = new List<CompositePiece>();
                var p1 = (CompositePiece)pcol[i];
                duplicates.Add(p1);

                for (int j = 0; pcol.Count > j; j++)
                {
                    if (i != j)
                    {
                        var p2 = (CompositePiece)pcol[j];

                        if (HasSameGUID(p1, p2))
                        {
                            duplicates.Add(p2);
                        }
                    }
                }
                
                if (duplicates.Count > 1)
                {
                    Valiable = true;
                    Waiting = true;
                    ChoosingFunction(this, duplicates);
                    while (Waiting && Valiable) ;

                    if (Valiable)
                    {
                        pcol.Add(Choiced);
                        foreach (var child in Choiced.Source)
                        {
                            List<Piece> remove = FindPiecesByGUID(pcol, child.GUID);
                            foreach (Piece rem in remove)
                                pcol.Remove(rem);
                        }
                    }
                    else
                    {
                    }
                }
            }
        }

        List<Piece> FindPiecesByGUID(IList<Piece> Pieces, string GUID)
        {
            Func<CompositePiece, string, bool> cpRemover = (cp, guid) =>
            {
                foreach (var p in cp.Source)
                {
                    if (p.GUID == guid)
                    {
                        return true;
                    }
                }

                return false;
            };

            List<Piece> res = new List<Piece>();
            foreach(var p in Pieces)
            {
                if(p is CompositePiece cp)
                {
                    if(cpRemover(cp, GUID))
                    {
                        res.Add(cp);
                    }
                }
                else
                {
                    if (p.GUID == GUID)
                        res.Add(p);
                }
            }

            return res;
        }

        PieceCollection ClonePieces(PieceCollection PieceCollection)
        {
            PieceCollection pcol = new PieceCollection();
            foreach (var p in PieceCollection)
                pcol.Add((Piece)p.Clone());
            return pcol;
        }
    }

    static class CompletelyRandom
    {
        public static int Next(int Min, int Max) { return new Random(Convert.ToInt32(Guid.NewGuid().ToString("N").Substring(0, 8), 16)).Next(Min, Max); }
    }
}
