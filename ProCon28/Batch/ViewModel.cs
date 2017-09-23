using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker.Extensions;

namespace ProCon28.Batch
{
    public class ViewModel
    {
        public static ViewModel Current { get; } = new ViewModel();

        public static Linker.Piece StartBatchProcess(Linker.Piece Piece, IList<string[]> Batch)
        {
            Linker.Piece piece = (Linker.Piece)Piece.Clone();

            foreach(string[] args in Batch)
            {
                int len = args.Length;
                if (len > 0)
                {
                    switch (args[0].ToLower())
                    {
                        case "contours":
                        case "extractcontours":
                        case "cont":
                        case "co":
                            piece = PieceEdit.ExtractContours.Run(piece);
                            break;
                        case "blur":
                        case "bl":
                            int size = 10;
                            if (len > 1)
                                size = int.Parse(args[1]);
                            piece = PieceEdit.Blur.Run(piece, size);
                            break;
                        case "sort":
                        case "so":
                            Linker.PointSortation sort = Linker.PointSortation.Clockwise;
                            if (len > 1)
                                sort = args[1] == "1" ? Linker.PointSortation.Clockwise : Linker.PointSortation.AntiClockwise;
                            piece.SortVertexes(sort);
                            break;
                        case "duplicate":
                        case "du":
                            Linker.Piece bk = new Linker.Piece();
                            for (int i = 0; piece.Vertexes.Count > i; i++)
                            {
                                Linker.Point p = piece.Vertexes[i];
                                if (!bk.Vertexes.Contains(p))
                                    bk.Vertexes.Add(p);
                            }
                            piece = bk;
                            break;
                        case "straight":
                        case "st":
                            double threshold = 0;
                            if (len > 1)
                                threshold = double.Parse(args[1]);
                            piece = PieceEdit.Straight.Run(piece, threshold);
                            break;
                        case "convert":
                        case "cv":
                            piece = piece.Convert();
                            break;
                    }
                }
            }

            return piece;
        }

        public ObservableCollection<string> BatchFiles { get; } = new ObservableCollection<string>();

        public ViewModel()
        {
            UpdateFiles();
        }

        public void UpdateFiles()
        {
            BatchFiles.Clear();
            BatchFiles.AddRange(Directory.GetFiles(@"Batch\"));
        }

        public Linker.Piece Batch(Linker.Piece Piece, string FileName)
        {
            using(StreamReader sr = new StreamReader(FileName))
            {
                List<string[]> batch = new List<string[]>();
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    batch.Add(line.Split(' '));
                }

                return StartBatchProcess(Piece, batch);
            }
        }
    }
}
